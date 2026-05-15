import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import { spawn, type ChildProcess } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { DesktopPaths } from './paths';
import { startWebHost, type WebHost } from './webHost';
import type { ControlPlanePreflight, DesktopRuntimeStatus, ServiceStatus } from './types';

const appId = 'com.milustudio.desktop';

interface ManagedProcess {
  name: 'controlApi' | 'worker';
  process?: ChildProcess;
  status: ServiceStatus;
}

export class DesktopRuntime {
  private apiBaseUrl = '';
  private webHost?: WebHost;
  private health?: unknown;
  private preflight?: ControlPlanePreflight;
  private readonly sessionToken = randomBytes(32).toString('base64url');
  private controlApi: ManagedProcess = {
    name: 'controlApi',
    status: { name: 'controlApi', status: 'stopped', message: 'Control API is not started.' }
  };
  private worker: ManagedProcess = {
    name: 'worker',
    status: { name: 'worker', status: 'stopped', message: 'Worker is not started.' }
  };

  public constructor(private readonly paths: DesktopPaths) {}

  public get webHostUrl(): string {
    return this.webHost?.url ?? '';
  }

  public get controlApiBaseUrl(): string {
    return this.apiBaseUrl;
  }

  public get desktopSessionToken(): string {
    return this.sessionToken;
  }

  public async start(): Promise<DesktopRuntimeStatus> {
    fs.mkdirSync(this.paths.logsRoot, { recursive: true });

    this.webHost = await startWebHost(this.paths.webRoot);
    this.apiBaseUrl = `http://127.0.0.1:${await reservePort()}`;

    this.startControlApi();
    await this.waitForHealth();
    await this.refreshPreflight();
    this.startWorker();

    return this.status();
  }

  public async restartServices(): Promise<DesktopRuntimeStatus> {
    await this.stopProcess(this.worker);
    await this.stopProcess(this.controlApi);
    this.health = undefined;
    this.preflight = undefined;

    this.startControlApi();
    await this.waitForHealth();
    await this.refreshPreflight();
    this.startWorker();

    return this.status();
  }

  public async refresh(): Promise<DesktopRuntimeStatus> {
    if (this.controlApi.status.status === 'running') {
      await this.refreshHealth();
      await this.refreshPreflight();
    }

    return this.status();
  }

  public async stop(): Promise<void> {
    await this.stopProcess(this.worker);
    await this.stopProcess(this.controlApi);

    if (this.webHost) {
      await this.webHost.close();
      this.webHost = undefined;
    }
  }

  public status(): DesktopRuntimeStatus {
    return {
      appId,
      apiBaseUrl: this.apiBaseUrl,
      webHostUrl: this.webHost?.url ?? '',
      logsAvailable: fs.existsSync(this.paths.logsRoot),
      outputDirectoryAvailable: fs.existsSync(this.paths.outputsRoot),
      services: {
        controlApi: this.controlApi.status,
        worker: this.worker.status,
        webHost: {
          name: 'webHost',
          status: this.webHost ? 'running' : 'stopped',
          port: this.webHost?.port,
          message: this.webHost ? 'Desktop web host is serving the built Web UI.' : 'Desktop web host is stopped.'
        }
      },
      health: this.health,
      preflight: this.preflight,
      checkedAt: new Date().toISOString()
    };
  }

  private startControlApi(): void {
    const launch = resolveDotnetLaunch(
      this.paths.apiRoot,
      'MiLuStudio.Api.exe',
      'MiLuStudio.Api.dll',
      this.paths.dotnetExecutable
    );
    const log = this.openLog('control-api.log');
    const dotnetEnvironment = process.env.MILUSTUDIO_DESKTOP_DOTNET_ENVIRONMENT ?? 'Production';
    const env = this.backendEnvironment({
      ASPNETCORE_ENVIRONMENT: dotnetEnvironment,
      DOTNET_ENVIRONMENT: dotnetEnvironment,
      ASPNETCORE_URLS: this.apiBaseUrl,
      ControlPlane__WorkerId: 'milu-desktop-api'
    });

    this.controlApi.status = {
      name: 'controlApi',
      status: 'starting',
      message: `Starting Control API on ${this.apiBaseUrl}.`
    };
    this.controlApi.process = spawn(launch.command, launch.args, {
      cwd: this.paths.apiRoot,
      env,
      stdio: ['ignore', log, log],
      windowsHide: true
    });
    this.watchProcess(this.controlApi);
  }

  private startWorker(): void {
    const launch = resolveDotnetLaunch(
      this.paths.workerRoot,
      'MiLuStudio.Worker.exe',
      'MiLuStudio.Worker.dll',
      this.paths.dotnetExecutable
    );
    const log = this.openLog('worker.log');
    const dotnetEnvironment = process.env.MILUSTUDIO_DESKTOP_DOTNET_ENVIRONMENT ?? 'Production';
    const env = this.backendEnvironment({
      DOTNET_ENVIRONMENT: dotnetEnvironment,
      ControlPlane__WorkerId: `milu-desktop-worker-${process.pid}`
    });

    this.worker.status = {
      name: 'worker',
      status: 'starting',
      message: 'Starting Windows Worker for production task claiming.'
    };
    this.worker.process = spawn(launch.command, launch.args, {
      cwd: this.paths.workerRoot,
      env,
      stdio: ['ignore', log, log],
      windowsHide: true
    });
    this.worker.status = {
      name: 'worker',
      status: 'running',
      pid: this.worker.process.pid,
      message: 'Windows Worker process is running.'
    };
    this.watchProcess(this.worker);
  }

  private backendEnvironment(overrides: NodeJS.ProcessEnv): NodeJS.ProcessEnv {
    return {
      ...process.env,
      ConnectionStrings__MiLuStudioControlPlane: process.env.ConnectionStrings__MiLuStudioControlPlane ??
        `Data Source=${path.join(this.paths.storageRoot, 'milu-control-plane.sqlite3')}`,
      ControlPlane__RepositoryProvider: 'SQLite',
      ControlPlane__MigrationsPath: this.paths.sqliteRoot,
      ControlPlane__DesktopMode: 'true',
      ControlPlane__AllowedDesktopOrigin: this.webHost?.url ?? '',
      ControlPlane__DesktopSessionToken: this.sessionToken,
      ControlPlane__StorageRoot: this.paths.storageRoot,
      ControlPlane__PythonExecutablePath: this.paths.pythonExecutable,
      ControlPlane__PythonSkillsRoot: this.paths.pythonSkillsRoot,
      ControlPlane__SkillRunTempRoot: this.paths.skillRunTempRoot,
      ControlPlane__SkillRunRetentionCount: '30',
      ControlPlane__SkillRunTimeoutSeconds: '120',
      ...overrides
    };
  }

  private async waitForHealth(): Promise<void> {
    const deadline = Date.now() + 45_000;
    let lastError = 'Control API did not respond yet.';

    while (Date.now() < deadline) {
      try {
        await this.refreshHealth();
        this.controlApi.status = {
          name: 'controlApi',
          status: 'running',
          pid: this.controlApi.process?.pid,
          port: Number(new URL(this.apiBaseUrl).port),
          message: 'Control API health endpoint is reachable.'
        };
        return;
      } catch (error) {
        lastError = error instanceof Error ? error.message : String(error);
        await delay(500);
      }
    }

    this.controlApi.status = {
      name: 'controlApi',
      status: 'failed',
      pid: this.controlApi.process?.pid,
      message: lastError
    };
    throw new Error(`Control API failed to become healthy: ${lastError}`);
  }

  private async refreshHealth(): Promise<void> {
    this.health = await getJson(`${this.apiBaseUrl}/health`);
  }

  private async refreshPreflight(): Promise<void> {
    try {
      this.preflight = await getJson<ControlPlanePreflight>(`${this.apiBaseUrl}/api/system/preflight`);
    } catch (error) {
      if (error instanceof HttpJsonError) {
        const problem = error.body as { preflight?: ControlPlanePreflight; extensions?: { preflight?: ControlPlanePreflight } };
        this.preflight = problem.preflight ?? problem.extensions?.preflight;
        return;
      }

      throw error;
    }
  }

  private async stopProcess(service: ManagedProcess): Promise<void> {
    if (!service.process || service.process.killed) {
      service.status = { name: service.name, status: 'stopped', message: `${service.name} is stopped.` };
      return;
    }

    await new Promise<void>(resolve => {
      const processToStop = service.process;
      if (!processToStop || processToStop.killed) {
        resolve();
        return;
      }

      processToStop.once('exit', () => resolve());
      processToStop.kill();
      setTimeout(() => {
        if (!processToStop.killed) {
          processToStop.kill('SIGKILL');
        }
        resolve();
      }, 3_000).unref();
    });

    service.status = { name: service.name, status: 'stopped', message: `${service.name} is stopped.` };
    service.process = undefined;
  }

  private watchProcess(service: ManagedProcess): void {
    service.process?.once('exit', (code, signal) => {
      service.status = {
        name: service.name,
        status: code === 0 ? 'stopped' : 'failed',
        message: `${service.name} exited with code ${code ?? 'n/a'}${signal ? ` and signal ${signal}` : ''}.`
      };
    });
  }

  private openLog(fileName: string): number {
    fs.mkdirSync(this.paths.logsRoot, { recursive: true });
    return fs.openSync(path.join(this.paths.logsRoot, fileName), 'a');
  }
}

function resolveDotnetLaunch(
  root: string,
  exeName: string,
  dllName: string,
  dotnetExecutable: string
): { command: string; args: string[] } {
  const exePath = path.join(root, exeName);
  if (fs.existsSync(exePath)) {
    return { command: exePath, args: [] };
  }

  const dllPath = path.join(root, dllName);
  if (!fs.existsSync(dllPath)) {
    throw new Error(`Missing .NET runtime entry: ${dllPath}`);
  }

  return {
    command: dotnetExecutable,
    args: [dllPath]
  };
}

async function reservePort(): Promise<number> {
  const server = http.createServer();
  return new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(0, '127.0.0.1', () => {
      const address = server.address();
      server.close();
      if (!address || typeof address === 'string') {
        reject(new Error('Unable to reserve a local TCP port.'));
        return;
      }

      resolve(address.port);
    });
  });
}

function getJson<T = unknown>(url: string): Promise<T> {
  return new Promise((resolve, reject) => {
    http.get(url, response => {
      let body = '';
      response.setEncoding('utf8');
      response.on('data', chunk => {
        body += chunk;
      });
      response.on('end', () => {
        const parsed = body ? JSON.parse(body) as T : undefined as T;
        if (response.statusCode && response.statusCode >= 400) {
          reject(new HttpJsonError(response.statusCode, parsed));
          return;
        }

        resolve(parsed);
      });
    }).on('error', reject);
  });
}

class HttpJsonError extends Error {
  public constructor(public readonly statusCode: number, public readonly body: unknown) {
    super(`HTTP ${statusCode}`);
  }
}

function delay(milliseconds: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, milliseconds));
}

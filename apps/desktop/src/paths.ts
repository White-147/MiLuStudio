import { app } from 'electron';
import fs from 'node:fs';
import path from 'node:path';

export interface DesktopPaths {
  appRoot: string;
  projectRoot: string;
  resourceRoot: string;
  dataRoot: string;
  electronUserDataRoot: string;
  electronSessionDataRoot: string;
  webRoot: string;
  controlPlaneRoot: string;
  apiRoot: string;
  workerRoot: string;
  migrationsRoot: string;
  pythonSkillsRoot: string;
  pythonRuntimeRoot: string;
  storageRoot: string;
  skillRunTempRoot: string;
  logsRoot: string;
  outputsRoot: string;
  pythonExecutable: string;
  dotnetExecutable: string;
}

export function resolveDesktopPaths(): DesktopPaths {
  const appRoot = app.getAppPath();
  const projectRoot = process.env.MILUSTUDIO_PROJECT_ROOT
    ? path.resolve(process.env.MILUSTUDIO_PROJECT_ROOT)
    : app.isPackaged
      ? path.dirname(process.execPath)
      : path.resolve(appRoot, '..', '..');

  const resourceRoot = process.env.MILUSTUDIO_DESKTOP_RUNTIME_ROOT
    ? path.resolve(process.env.MILUSTUDIO_DESKTOP_RUNTIME_ROOT)
    : app.isPackaged
      ? process.resourcesPath
      : path.join(appRoot, 'runtime');

  const dataRoot = process.env.MILUSTUDIO_DESKTOP_DATA_ROOT
    ? path.resolve(process.env.MILUSTUDIO_DESKTOP_DATA_ROOT)
    : app.isPackaged
      ? path.join(path.dirname(process.execPath), 'data')
      : projectRoot;

  const controlPlaneRoot = path.join(resourceRoot, 'control-plane');
  const pythonRuntimeRoot = process.env.MILUSTUDIO_DESKTOP_PYTHON_RUNTIME_ROOT
    ? path.resolve(process.env.MILUSTUDIO_DESKTOP_PYTHON_RUNTIME_ROOT)
    : path.join(resourceRoot, 'python-runtime');
  const packagedPythonExecutable = path.join(pythonRuntimeRoot, 'python.exe');

  return {
    appRoot,
    projectRoot,
    resourceRoot,
    dataRoot,
    electronUserDataRoot: process.env.MILUSTUDIO_ELECTRON_USER_DATA_ROOT
      ? path.resolve(process.env.MILUSTUDIO_ELECTRON_USER_DATA_ROOT)
      : path.join(dataRoot, '.tmp', 'electron-user-data'),
    electronSessionDataRoot: process.env.MILUSTUDIO_ELECTRON_SESSION_DATA_ROOT
      ? path.resolve(process.env.MILUSTUDIO_ELECTRON_SESSION_DATA_ROOT)
      : path.join(dataRoot, '.tmp', 'electron-session-data'),
    webRoot: process.env.MILUSTUDIO_WEB_DIST
      ? path.resolve(process.env.MILUSTUDIO_WEB_DIST)
      : path.join(resourceRoot, 'web'),
    controlPlaneRoot,
    apiRoot: path.join(controlPlaneRoot, 'api'),
    workerRoot: path.join(controlPlaneRoot, 'worker'),
    migrationsRoot: path.join(controlPlaneRoot, 'db', 'migrations'),
    pythonSkillsRoot: process.env.MILUSTUDIO_PYTHON_SKILLS_ROOT
      ? path.resolve(process.env.MILUSTUDIO_PYTHON_SKILLS_ROOT)
      : path.join(resourceRoot, 'python-skills'),
    pythonRuntimeRoot,
    storageRoot: process.env.MILUSTUDIO_STORAGE_ROOT
      ? path.resolve(process.env.MILUSTUDIO_STORAGE_ROOT)
      : path.join(dataRoot, 'storage'),
    skillRunTempRoot: process.env.MILUSTUDIO_SKILL_RUN_TEMP_ROOT
      ? path.resolve(process.env.MILUSTUDIO_SKILL_RUN_TEMP_ROOT)
      : path.join(dataRoot, '.tmp', 'skill-runs'),
    logsRoot: process.env.MILUSTUDIO_DESKTOP_LOG_DIR
      ? path.resolve(process.env.MILUSTUDIO_DESKTOP_LOG_DIR)
      : path.join(dataRoot, 'logs', 'desktop'),
    outputsRoot: process.env.MILUSTUDIO_OUTPUTS_ROOT
      ? path.resolve(process.env.MILUSTUDIO_OUTPUTS_ROOT)
      : path.join(dataRoot, 'outputs'),
    pythonExecutable:
      process.env.MILUSTUDIO_PYTHON ??
      (fs.existsSync(packagedPythonExecutable)
        ? packagedPythonExecutable
        : 'D:\\soft\\program\\Python\\Python313\\python.exe'),
    dotnetExecutable: process.env.MILUSTUDIO_DOTNET_PATH ?? 'D:\\soft\\program\\dotnet\\dotnet.exe'
  };
}

export function assertRequiredRuntime(paths: DesktopPaths): void {
  const required = [
    ['Web dist', paths.webRoot],
    ['Control API runtime', paths.apiRoot],
    ['Worker runtime', paths.workerRoot],
    ['SQL migrations', paths.migrationsRoot],
    ['Python skills root', paths.pythonSkillsRoot],
    ['Python runtime', paths.pythonExecutable]
  ] as const;

  const missing = required
    .filter(([, candidate]) => !fs.existsSync(candidate))
    .map(([label, candidate]) => `${label}: ${candidate}`);

  if (missing.length > 0) {
    throw new Error(`MiLuStudio desktop runtime is incomplete. Run npm run prepare:runtime.\n${missing.join('\n')}`);
  }
}

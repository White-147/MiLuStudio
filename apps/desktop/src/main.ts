import { app, BrowserWindow, dialog, ipcMain, Menu, session, shell, Tray, type IpcMainInvokeEvent } from 'electron';
import fs from 'node:fs';
import path from 'node:path';
import { DesktopRuntime } from './runtime';
import { assertRequiredRuntime, resolveDesktopPaths, type DesktopPaths } from './paths';
import type { DesktopCommandResult } from './types';

const appId = 'com.milustudio.desktop';
const isSmokeTest = process.argv.includes('--smoke-test');

let runtime: DesktopRuntime | undefined;
let mainWindow: BrowserWindow | undefined;
let tray: Tray | undefined;
let paths: DesktopPaths;
let isQuitting = false;

app.setAppUserModelId(appId);
paths = resolveDesktopPaths();
configureElectronDataPaths(paths);

const singleInstanceLock = app.requestSingleInstanceLock();
if (!singleInstanceLock && !isSmokeTest) {
  app.quit();
}

app.on('second-instance', () => {
  if (mainWindow) {
    if (mainWindow.isMinimized()) {
      mainWindow.restore();
    }
    mainWindow.focus();
  }
});

app.whenReady()
  .then(async () => {
    assertRequiredRuntime(paths);
    runtime = new DesktopRuntime(paths);
    await runtime.start();

    if (isSmokeTest) {
      const status = await runtime.refresh();
      const runtimeToStop = runtime;
      runtime = undefined;
      console.log(JSON.stringify(status, null, 2));
      await runtimeToStop.stop();
      app.exit(0);
      return;
    }

    configureDefaultSession();
    createTray();
    createMainWindow();
  })
  .catch(error => {
    if (isSmokeTest) {
      console.error(error);
      app.exit(1);
      return;
    }

    dialog.showErrorBox('MiLuStudio 启动失败', error instanceof Error ? error.message : String(error));
    app.quit();
  });

app.on('window-all-closed', () => {
  mainWindow = undefined;
});

app.on('before-quit', async event => {
  if (!runtime) {
    return;
  }

  event.preventDefault();
  isQuitting = true;
  const runtimeToStop = runtime;
  runtime = undefined;
  await runtimeToStop.stop();
  app.exit(0);
});

ipcMain.handle('desktop:get-status', async event => {
  assertTrustedSender(event);
  return runtime?.refresh();
});
ipcMain.handle('desktop:restart-services', async event => {
  assertTrustedSender(event);
  return runtime?.restartServices();
});
ipcMain.handle('desktop:open-logs', async event => {
  assertTrustedSender(event);
  return openDirectory(paths.logsRoot, '日志目录不存在。');
});
ipcMain.handle('desktop:open-output-directory', async event => {
  assertTrustedSender(event);
  return openDirectory(paths.outputsRoot, '输出目录尚不存在。');
});

function createMainWindow(): void {
  if (!runtime) {
    throw new Error('MiLuStudio runtime is not started.');
  }

  mainWindow = new BrowserWindow({
    width: 1440,
    height: 920,
    minWidth: 1100,
    minHeight: 720,
    title: 'MiLuStudio',
    backgroundColor: '#f6f7fb',
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      additionalArguments: [
        `--milu-api-base=${runtime.controlApiBaseUrl}`,
        `--milu-desktop-token=${runtime.desktopSessionToken}`
      ],
      contextIsolation: true,
      sandbox: true,
      nodeIntegration: false,
      webSecurity: true
    }
  });

  mainWindow.removeMenu();
  mainWindow.on('close', event => {
    if (isQuitting) {
      return;
    }

    event.preventDefault();
    mainWindow?.hide();
  });
  mainWindow.on('closed', () => {
    mainWindow = undefined;
  });
  mainWindow.webContents.setWindowOpenHandler(() => ({ action: 'deny' }));
  mainWindow.webContents.on('will-navigate', (event, url) => {
    if (!isTrustedDesktopUrl(url)) {
      event.preventDefault();
    }
  });
  mainWindow.once('ready-to-show', () => mainWindow?.show());
  mainWindow.loadURL(`${runtime.webHostUrl}/#/project/demo-episode-01`);
}

function createTray(): void {
  const iconPath = resolveTrayIconPath();
  tray = new Tray(iconPath);
  tray.setToolTip('MiLuStudio');
  tray.setContextMenu(Menu.buildFromTemplate([
    {
      label: '打开 MiLuStudio',
      click: () => {
        mainWindow?.show();
        mainWindow?.focus();
      }
    },
    {
      label: '重启本地服务',
      click: () => {
        void runtime?.restartServices();
      }
    },
    {
      label: '打开输出目录',
      click: () => {
        void openDirectory(paths.outputsRoot, '输出目录尚不存在。');
      }
    },
    {
      label: '查看日志',
      click: () => {
        void openDirectory(paths.logsRoot, '日志目录不存在。');
      }
    },
    { type: 'separator' },
    {
      label: '退出',
      click: () => {
        isQuitting = true;
        app.quit();
      }
    }
  ]));
}

function resolveTrayIconPath(): string {
  const packagedIcon = path.join(process.resourcesPath, 'build', 'icon.ico');
  if (app.isPackaged && fs.existsSync(packagedIcon)) {
    return packagedIcon;
  }

  const devIcon = path.join(app.getAppPath(), 'build', 'icon.ico');
  return fs.existsSync(devIcon)
    ? devIcon
    : path.join(app.getAppPath(), '..', 'web', 'public', 'brand', 'logo.png');
}

async function openDirectory(directory: string, missingMessage: string): Promise<DesktopCommandResult> {
  if (!fs.existsSync(directory)) {
    return { ok: false, message: missingMessage };
  }

  const result = await shell.openPath(directory);
  return result
    ? { ok: false, message: result }
    : { ok: true, message: directory };
}

function configureElectronDataPaths(resolvedPaths: DesktopPaths): void {
  fs.mkdirSync(resolvedPaths.electronUserDataRoot, { recursive: true });
  fs.mkdirSync(resolvedPaths.electronSessionDataRoot, { recursive: true });
  fs.mkdirSync(resolvedPaths.logsRoot, { recursive: true });
  app.setPath('userData', resolvedPaths.electronUserDataRoot);
  app.setPath('sessionData', resolvedPaths.electronSessionDataRoot);
  app.setPath('logs', resolvedPaths.logsRoot);
}

function configureDefaultSession(): void {
  session.defaultSession.setPermissionRequestHandler((_webContents, _permission, callback) => {
    callback(false);
  });
}

function assertTrustedSender(event: IpcMainInvokeEvent): void {
  const senderUrl = event.senderFrame?.url ?? event.sender.getURL();
  if (!isTrustedDesktopUrl(senderUrl)) {
    throw new Error('Rejected desktop IPC from an untrusted renderer.');
  }
}

function isTrustedDesktopUrl(rawUrl: string): boolean {
  if (!runtime?.webHostUrl) {
    return false;
  }

  try {
    return new URL(rawUrl).origin === new URL(runtime.webHostUrl).origin;
  } catch {
    return false;
  }
}

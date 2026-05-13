import { contextBridge, ipcRenderer } from 'electron';
import type { DesktopCommandResult, DesktopRuntimeStatus } from './types';

const apiBaseArg = process.argv.find(argument => argument.startsWith('--milu-api-base='));
const apiBaseUrl = apiBaseArg?.slice('--milu-api-base='.length) ?? 'http://127.0.0.1:5368';
const desktopTokenArg = process.argv.find(argument => argument.startsWith('--milu-desktop-token='));
const desktopToken = desktopTokenArg?.slice('--milu-desktop-token='.length) ?? '';

contextBridge.exposeInMainWorld('__MILUSTUDIO_CONTROL_API_BASE__', apiBaseUrl);
contextBridge.exposeInMainWorld('__MILUSTUDIO_DESKTOP_TOKEN__', desktopToken);
contextBridge.exposeInMainWorld('miluDesktop', {
  getStatus: () => ipcRenderer.invoke('desktop:get-status') as Promise<DesktopRuntimeStatus>,
  restartServices: () => ipcRenderer.invoke('desktop:restart-services') as Promise<DesktopRuntimeStatus>,
  openLogs: () => ipcRenderer.invoke('desktop:open-logs') as Promise<DesktopCommandResult>,
  openOutputDirectory: () => ipcRenderer.invoke('desktop:open-output-directory') as Promise<DesktopCommandResult>
});

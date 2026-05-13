export interface DesktopRuntimeStatus {
  appId: string;
  apiBaseUrl: string;
  webHostUrl: string;
  logsAvailable: boolean;
  outputDirectoryAvailable: boolean;
  services: Record<string, DesktopServiceStatus>;
  preflight?: ControlPlanePreflight;
  checkedAt: string;
}

export interface DesktopServiceStatus {
  name: string;
  status: 'starting' | 'running' | 'stopped' | 'failed';
  pid?: number;
  port?: number;
  message: string;
}

export interface ControlPlanePreflight {
  repositoryProvider: string;
  healthy: boolean;
  checks: PreflightCheck[];
  recommendations: string[];
}

export interface PreflightCheck {
  name: string;
  status: string;
  message: string;
  details: Record<string, string>;
}

export interface DesktopCommandResult {
  ok: boolean;
  message: string;
}

declare global {
  interface Window {
    __MILUSTUDIO_CONTROL_API_BASE__?: string;
    __MILUSTUDIO_DESKTOP_TOKEN__?: string;
    miluDesktop?: {
      getStatus: () => Promise<DesktopRuntimeStatus>;
      restartServices: () => Promise<DesktopRuntimeStatus>;
      openLogs: () => Promise<DesktopCommandResult>;
      openOutputDirectory: () => Promise<DesktopCommandResult>;
    };
  }
}

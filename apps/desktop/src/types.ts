export type ManagedServiceName = 'controlApi' | 'worker' | 'webHost';

export interface ServiceStatus {
  name: ManagedServiceName;
  status: 'starting' | 'running' | 'stopped' | 'failed';
  pid?: number;
  port?: number;
  message: string;
}

export interface PreflightCheck {
  name: string;
  status: string;
  message: string;
  details: Record<string, string>;
}

export interface ControlPlanePreflight {
  repositoryProvider: string;
  healthy: boolean;
  checks: PreflightCheck[];
  recommendations: string[];
}

export interface DesktopRuntimeStatus {
  appId: string;
  apiBaseUrl: string;
  webHostUrl: string;
  logsAvailable: boolean;
  outputDirectoryAvailable: boolean;
  services: Record<ManagedServiceName, ServiceStatus>;
  health?: unknown;
  preflight?: ControlPlanePreflight;
  checkedAt: string;
}

export interface DesktopCommandResult {
  ok: boolean;
  message: string;
}

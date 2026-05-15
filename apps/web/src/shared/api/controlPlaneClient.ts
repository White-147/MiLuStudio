import type {
  AuthSession,
  AuthState,
  CostLedgerRecord,
  GenerationTaskRecord,
  LoginRequest,
  ProviderSafetyStatus,
  ProviderConnectionTestRequest,
  ProviderConnectionTestResponse,
  ProviderSpendGuardCheckRequest,
  ProviderSpendGuardDecision,
  ProviderSettingsPreflight,
  ProviderSettingsResponse,
  ProviderSettingsUpdateRequest,
  SystemDependenciesReport,
  ProductionJob,
  ProductionJobEvent,
  ProjectAssetUploadIntent,
  ProjectAssetUploadResponse,
  ProjectAssetRecord,
  ProjectDetail,
  ProjectUpdateRequest,
  ProjectSummary,
  RegisterAccountRequest,
  StructuredOutputEditRequest,
  StructuredOutputEditResponse,
  StoryboardEditRequest,
  StoryboardEditResponse,
  StoryboardShotRegenerateRequest,
} from '../types/production';

declare global {
  interface Window {
    __MILUSTUDIO_CONTROL_API_BASE__?: string;
    __MILUSTUDIO_DESKTOP_TOKEN__?: string;
  }
}

const DEFAULT_API_BASE_URL = 'http://127.0.0.1:5368';

const injectedApiBaseUrl =
  typeof window !== 'undefined' ? window.__MILUSTUDIO_CONTROL_API_BASE__ : undefined;
const injectedDesktopToken =
  typeof window !== 'undefined' ? window.__MILUSTUDIO_DESKTOP_TOKEN__ : undefined;

const apiBaseUrl = (
  injectedApiBaseUrl ??
  (import.meta.env.VITE_CONTROL_API_BASE as string | undefined) ??
  DEFAULT_API_BASE_URL
).replace(/\/$/, '');

const accessTokenStorageKey = 'milu.auth.accessToken';
const refreshTokenStorageKey = 'milu.auth.refreshToken';

let accessToken =
  typeof window !== 'undefined' ? window.localStorage.getItem(accessTokenStorageKey) ?? undefined : undefined;
let refreshToken =
  typeof window !== 'undefined' ? window.localStorage.getItem(refreshTokenStorageKey) ?? undefined : undefined;

export function getStoredAccessToken(): string | undefined {
  return accessToken;
}

export function getStoredRefreshToken(): string | undefined {
  return refreshToken;
}

export function getControlApiBaseUrl(): string {
  return apiBaseUrl;
}

export function storeAuthSession(session: AuthSession) {
  accessToken = session.accessToken;
  refreshToken = session.refreshToken;

  if (typeof window !== 'undefined') {
    window.localStorage.setItem(accessTokenStorageKey, accessToken);
    window.localStorage.setItem(refreshTokenStorageKey, refreshToken);
  }
}

export function clearAuthSession() {
  accessToken = undefined;
  refreshToken = undefined;

  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(accessTokenStorageKey);
    window.localStorage.removeItem(refreshTokenStorageKey);
  }
}

export async function getAuthState(signal?: AbortSignal): Promise<AuthState> {
  return request<AuthState>('/api/auth/me', { signal }, { allowAnonymous: true });
}

export async function registerAccount(payload: RegisterAccountRequest, signal?: AbortSignal): Promise<AuthSession> {
  const session = await request<AuthSession>(
    '/api/auth/register',
    {
      method: 'POST',
      body: JSON.stringify(payload),
      signal,
    },
    { allowAnonymous: true },
  );
  storeAuthSession(session);
  return session;
}

export async function loginAccount(payload: LoginRequest, signal?: AbortSignal): Promise<AuthSession> {
  const session = await request<AuthSession>(
    '/api/auth/login',
    {
      method: 'POST',
      body: JSON.stringify(payload),
      signal,
    },
    { allowAnonymous: true },
  );
  storeAuthSession(session);
  return session;
}

export async function refreshAuthSession(
  deviceFingerprint: string,
  deviceName: string,
  signal?: AbortSignal,
): Promise<AuthSession> {
  const session = await request<AuthSession>(
    '/api/auth/refresh',
    {
      method: 'POST',
      body: JSON.stringify({ refreshToken, deviceFingerprint, deviceName }),
      signal,
    },
    { allowAnonymous: true },
  );
  storeAuthSession(session);
  return session;
}

export async function logout(signal?: AbortSignal): Promise<void> {
  try {
    await request<{ status: string }>('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
      signal,
    });
  } finally {
    clearAuthSession();
  }
}

export async function listProjects(signal?: AbortSignal): Promise<ProjectSummary[]> {
  return request<ProjectSummary[]>('/api/projects', { signal });
}

export async function getProject(projectId: string, signal?: AbortSignal): Promise<ProjectDetail> {
  return request<ProjectDetail>(`/api/projects/${encodeURIComponent(projectId)}`, { signal });
}

export async function createProject(signal?: AbortSignal): Promise<ProjectDetail> {
  return request<ProjectDetail>('/api/projects', {
    method: 'POST',
    body: JSON.stringify({
      title: '新建漫剧项目',
      storyText: defaultStoryText,
      mode: 'director',
      targetDuration: 45,
      aspectRatio: '9:16',
      stylePreset: '轻写实国漫',
    }),
    signal,
  });
}

export async function updateProject(
  projectId: string,
  payload: ProjectUpdateRequest,
  signal?: AbortSignal,
): Promise<ProjectDetail> {
  return request<ProjectDetail>(`/api/projects/${encodeURIComponent(projectId)}`, {
    method: 'PATCH',
    body: JSON.stringify(payload),
    signal,
  });
}

export async function deleteProject(projectId: string, signal?: AbortSignal): Promise<void> {
  await request<void>(`/api/projects/${encodeURIComponent(projectId)}`, {
    method: 'DELETE',
    signal,
  });
}

export async function startProductionJob(projectId: string, signal?: AbortSignal): Promise<ProductionJob> {
  return request<ProductionJob>(`/api/projects/${encodeURIComponent(projectId)}/production-jobs`, {
    method: 'POST',
    body: JSON.stringify({ requestedBy: 'web-ui' }),
    signal,
  });
}

export async function pauseProductionJob(jobId: string, signal?: AbortSignal): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'pause', undefined, signal);
}

export async function resumeProductionJob(jobId: string, signal?: AbortSignal): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'resume', undefined, signal);
}

export async function retryProductionJob(jobId: string, signal?: AbortSignal): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'retry', undefined, signal);
}

export async function getProductionJob(jobId: string, signal?: AbortSignal): Promise<ProductionJob> {
  return request<ProductionJob>(`/api/production-jobs/${encodeURIComponent(jobId)}`, { signal });
}

export async function listProductionTasks(jobId: string, signal?: AbortSignal): Promise<GenerationTaskRecord[]> {
  return request<GenerationTaskRecord[]>(`/api/production-jobs/${encodeURIComponent(jobId)}/tasks`, { signal });
}

export async function listProjectAssets(projectId: string, signal?: AbortSignal): Promise<ProjectAssetRecord[]> {
  return request<ProjectAssetRecord[]>(`/api/projects/${encodeURIComponent(projectId)}/assets`, { signal });
}

export async function uploadProjectAsset(
  projectId: string,
  file: File,
  intent: ProjectAssetUploadIntent,
  signal?: AbortSignal,
): Promise<ProjectAssetUploadResponse> {
  const form = new FormData();
  form.append('file', file);
  form.append('intent', intent);

  return request<ProjectAssetUploadResponse>(`/api/projects/${encodeURIComponent(projectId)}/assets/upload`, {
    method: 'POST',
    body: form,
    signal,
  });
}

export async function listProjectCosts(projectId: string, signal?: AbortSignal): Promise<CostLedgerRecord[]> {
  return request<CostLedgerRecord[]>(`/api/projects/${encodeURIComponent(projectId)}/cost-ledger`, { signal });
}

export async function approveProductionCheckpoint(
  jobId: string,
  notes: string,
  signal?: AbortSignal,
): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'checkpoint', { approved: true, notes }, signal);
}

export async function rejectProductionCheckpoint(
  jobId: string,
  notes: string,
  signal?: AbortSignal,
): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'checkpoint', { approved: false, notes }, signal);
}

export async function rollbackProductionJob(
  jobId: string,
  skillName: string,
  notes: string,
  signal?: AbortSignal,
): Promise<ProductionJob> {
  return productionJobCommand(jobId, 'rollback', { skillName, notes }, signal);
}

export async function updateStoryboardTask(
  taskId: string,
  payload: StoryboardEditRequest,
  signal?: AbortSignal,
): Promise<StoryboardEditResponse> {
  return request<StoryboardEditResponse>(`/api/generation-tasks/${encodeURIComponent(taskId)}/storyboard`, {
    method: 'PATCH',
    body: JSON.stringify(payload),
    signal,
  });
}

export async function updateStructuredOutputTask(
  taskId: string,
  payload: StructuredOutputEditRequest,
  signal?: AbortSignal,
): Promise<StructuredOutputEditResponse> {
  return request<StructuredOutputEditResponse>(`/api/generation-tasks/${encodeURIComponent(taskId)}/structured-output`, {
    method: 'PATCH',
    body: JSON.stringify(payload),
    signal,
  });
}

export async function regenerateStoryboardShot(
  taskId: string,
  shotId: string,
  payload: StoryboardShotRegenerateRequest,
  signal?: AbortSignal,
): Promise<StoryboardEditResponse> {
  return request<StoryboardEditResponse>(
    `/api/generation-tasks/${encodeURIComponent(taskId)}/storyboard/shots/${encodeURIComponent(shotId)}/regenerate`,
    {
      method: 'POST',
      body: JSON.stringify(payload),
      signal,
    },
  );
}

export async function getProviderSettings(signal?: AbortSignal): Promise<ProviderSettingsResponse> {
  return request<ProviderSettingsResponse>('/api/settings/providers', { signal });
}

export async function updateProviderSettings(
  payload: ProviderSettingsUpdateRequest,
  signal?: AbortSignal,
): Promise<ProviderSettingsResponse> {
  return request<ProviderSettingsResponse>('/api/settings/providers', {
    method: 'PATCH',
    body: JSON.stringify(payload),
    signal,
  });
}

export async function getProviderSettingsPreflight(signal?: AbortSignal): Promise<ProviderSettingsPreflight> {
  return request<ProviderSettingsPreflight>('/api/settings/providers/preflight', { signal });
}

export async function getProviderSafety(signal?: AbortSignal): Promise<ProviderSafetyStatus> {
  return request<ProviderSafetyStatus>('/api/settings/providers/safety', { signal });
}

export async function testProviderConnection(
  providerKind: string,
  payload: ProviderConnectionTestRequest,
  signal?: AbortSignal,
): Promise<ProviderConnectionTestResponse> {
  return request<ProviderConnectionTestResponse>(
    `/api/settings/providers/${encodeURIComponent(providerKind)}/connection-test`,
    {
      method: 'POST',
      body: JSON.stringify(payload),
      signal,
    },
  );
}

export async function checkProviderSpendGuard(
  payload: ProviderSpendGuardCheckRequest,
  signal?: AbortSignal,
): Promise<ProviderSpendGuardDecision> {
  return request<ProviderSpendGuardDecision>('/api/settings/providers/spend-guard/check', {
    method: 'POST',
    body: JSON.stringify(payload),
    signal,
  });
}

export async function getSystemDependencies(signal?: AbortSignal): Promise<SystemDependenciesReport> {
  return request<SystemDependenciesReport>('/api/system/dependencies', { signal });
}

export function watchProductionJob(
  jobId: string,
  onEvent: (event: ProductionJobEvent) => void,
  onError: (error: Event) => void,
) {
  const authQuery = accessToken ? `?access_token=${encodeURIComponent(accessToken)}` : '';
  const source = new EventSource(`${apiBaseUrl}/api/production-jobs/${encodeURIComponent(jobId)}/events${authQuery}`);
  const eventTypes = [
    'stage_changed',
    'task_started',
    'task_progress',
    'task_completed',
    'task_failed',
    'checkpoint_required',
    'stage_paused',
    'cost_updated',
    'artifact_ready',
  ];

  eventTypes.forEach((type) => {
    source.addEventListener(type, (message) => {
      onEvent(JSON.parse((message as MessageEvent<string>).data) as ProductionJobEvent);
    });
  });

  source.onerror = onError;

  return () => source.close();
}

async function productionJobCommand(
  jobId: string,
  command: 'pause' | 'resume' | 'retry' | 'checkpoint' | 'rollback',
  body?: unknown,
  signal?: AbortSignal,
): Promise<ProductionJob> {
  return request<ProductionJob>(`/api/production-jobs/${encodeURIComponent(jobId)}/${command}`, {
    method: 'POST',
    body: JSON.stringify(body ?? {}),
    signal,
  });
}

async function request<T>(
  path: string,
  init: RequestInit = {},
  options: { allowAnonymous?: boolean } = {},
): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      ...init,
      headers: {
        ...(isFormDataBody(init.body) ? {} : { 'Content-Type': 'application/json' }),
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        ...(injectedDesktopToken ? { 'X-MiLuStudio-Desktop-Token': injectedDesktopToken } : {}),
        ...init.headers,
      },
    });
  } catch (error) {
    if (isAbortError(error)) {
      throw error;
    }

    throw new Error(formatNetworkError(error));
  }

  if (!response.ok) {
    const detail = await response.text();
    throw new Error(parseErrorMessage(response.status, detail));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError';
}

function isFormDataBody(body: BodyInit | null | undefined): body is FormData {
  return typeof FormData !== 'undefined' && body instanceof FormData;
}

function formatNetworkError(error: any): string {
  const readableDetail = error instanceof Error && error.message ? ` 原始错误：${error.message}` : '';
  return `Control API 未连接（${apiBaseUrl}）。请确认本地服务已启动并可访问。${readableDetail}`;
}

function parseErrorMessage(status: number, detail: string): string {
  if (!detail) {
    return `Control API request failed: ${status}`;
  }

  try {
    const payload = JSON.parse(detail) as { error?: string; detail?: string; title?: string; details?: string[] };
    const message = payload.error ?? payload.detail ?? payload.title;
    const details = Array.isArray(payload.details) ? ` ${payload.details.join(' ')}` : '';
    return message ? `${message}${details}` : `Control API request failed: ${status}`;
  } catch {
    return `Control API request failed: ${status}`;
  }
}

const defaultStoryText =
  '雨夜里，林溪在旧巷口捡到一只会发光的纸鹤。纸鹤飞得很慢，像在等她跟上。它穿过挂满旧招牌的巷子，落在一间废弃照相馆门口。林溪的哥哥三年前在这里失踪，警方只找到一卷被雨水泡坏的胶片。她推门进去，发现暗房里亮着微弱红光，墙上贴满哥哥拍下的陌生人影。纸鹤钻进显影盘，胶片上忽然浮出一行字：不要相信明天早上的自己。林溪以为这是恶作剧，却在玻璃柜里看到一张刚冲洗好的照片，照片中的她站在同一间暗房，手里拿着哥哥的相机，身后有一道没有脸的影子。她开始按照纸鹤留下的光点寻找线索，每一步都揭开一段被人刻意删除的记忆。最后她发现哥哥并不是失踪，而是被困在照片之间的时间缝隙里，只有在天亮前拍下真正凶手的脸，才能把他带回现实。林溪带着相机回到巷口，发现所有招牌都变成了哥哥当年拍过的日期。纸鹤停在钟楼下，翅膀上浮出一串倒计时。她必须在雨停之前找到照片里那道影子的主人，否则第二天醒来，她会忘记哥哥，也会忘记自己为什么来到这里。她最终选择把镜头对准橱窗里的倒影，才看见凶手一直跟在她身后，披着哥哥的雨衣，脸却和未来的她一模一样。快门按下时，整条巷子的灯同时熄灭，纸鹤化成一束光钻进相机，哥哥的声音从暗房深处传来：别回头，把我带出去。';

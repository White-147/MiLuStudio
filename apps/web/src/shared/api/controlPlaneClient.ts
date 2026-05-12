import type { ProductionJob, ProductionJobEvent, ProjectDetail, ProjectSummary } from '../types/production';

const DEFAULT_API_BASE_URL = 'http://127.0.0.1:5268';

const apiBaseUrl = ((import.meta.env.VITE_CONTROL_API_BASE as string | undefined) ?? DEFAULT_API_BASE_URL).replace(
  /\/$/,
  '',
);

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
      storyText: '输入一段 500 到 2000 字故事后，MiLuStudio 会自动拆解脚本、角色、分镜和生成任务。',
      mode: 'director',
      targetDuration: 45,
      aspectRatio: '9:16',
      stylePreset: '轻写实国漫',
    }),
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

export function watchProductionJob(
  jobId: string,
  onEvent: (event: ProductionJobEvent) => void,
  onError: (error: Event) => void,
) {
  const source = new EventSource(`${apiBaseUrl}/api/production-jobs/${encodeURIComponent(jobId)}/events`);
  const eventTypes = [
    'stage_changed',
    'task_started',
    'task_progress',
    'task_completed',
    'task_failed',
    'checkpoint_required',
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

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init.headers,
    },
  });

  if (!response.ok) {
    throw new Error(`Control API request failed: ${response.status}`);
  }

  return (await response.json()) as T;
}

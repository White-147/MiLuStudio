export type ProjectMode = 'fast' | 'director';

export type ProjectStatus = 'draft' | 'running' | 'paused' | 'completed' | 'failed';

export type StageStatus = 'done' | 'running' | 'waiting' | 'review' | 'blocked';

export type ProductionJobStatus = 'queued' | 'running' | 'paused' | 'completed' | 'failed';

export interface ProjectSummary {
  id: string;
  title: string;
  description: string;
  mode: ProjectMode;
  status: ProjectStatus;
  targetDuration: number;
  aspectRatio: '9:16' | '16:9' | '1:1';
  updatedAt: string;
  progress: number;
}

export interface ProjectDetail extends ProjectSummary {
  stylePreset: string;
  storyText: string;
}

export interface ProductionStage {
  id: string;
  label: string;
  skill: string;
  status: StageStatus;
  duration: string;
  cost: string;
  needsReview: boolean;
}

export interface ProductionJob {
  id: string;
  projectId: string;
  status: ProductionJobStatus;
  currentStage: string;
  progress: number;
  startedAt: string;
  finishedAt: string | null;
  errorMessage: string | null;
  stages: ProductionStage[];
}

export interface ProductionJobEvent {
  type: string;
  jobId: string;
  projectId: string;
  stageId: string;
  stageLabel: string;
  skill: string;
  status: StageStatus;
  jobStatus: ProductionJobStatus;
  progress: number;
  message: string;
  occurredAt: string;
}

export interface ResultCard {
  id: string;
  title: string;
  kind: 'script' | 'character' | 'style' | 'storyboard' | 'media' | 'delivery';
  status: 'ready' | 'locked' | 'draft';
  summary: string;
  details: string[];
}

export interface DeliveryAsset {
  id: string;
  label: string;
  format: string;
  size: string;
  state: 'ready' | 'waiting';
}

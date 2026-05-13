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

export type GenerationTaskRecordStatus = 'waiting' | 'running' | 'review' | 'completed' | 'failed';

export interface GenerationTaskRecord {
  id: string;
  jobId: string;
  projectId: string;
  shotId: string | null;
  queueIndex: number;
  skillName: string;
  provider: string;
  inputJson: string;
  outputJson: string | null;
  status: GenerationTaskRecordStatus;
  attemptCount: number;
  costEstimate: number;
  costActual: number | null;
  startedAt: string | null;
  finishedAt: string | null;
  lockedBy: string | null;
  lockedUntil: string | null;
  lastHeartbeatAt: string | null;
  errorMessage: string | null;
}

export interface ProjectAssetRecord {
  id: string;
  projectId: string;
  kind: string;
  localPath: string;
  mimeType: string;
  fileSize: number;
  sha256: string | null;
  metadataJson: string | null;
  createdAt: string;
}

export interface CostLedgerRecord {
  id: string;
  projectId: string;
  taskId: string | null;
  provider: string;
  model: string;
  unit: string;
  quantity: number;
  estimatedCost: number;
  actualCost: number | null;
  createdAt: string;
}

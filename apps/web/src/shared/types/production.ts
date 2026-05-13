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

export interface ProjectUpdateRequest {
  title: string;
  storyText: string;
  mode: ProjectMode;
  targetDuration: number;
  aspectRatio: '9:16' | '16:9' | '1:1';
  stylePreset: string;
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
  checkpointNotes: string | null;
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

export interface AuthAccount {
  id: string;
  email: string | null;
  phone: string | null;
  displayName: string;
  status: 'active' | 'locked' | 'deleted';
  createdAt: string;
  lastLoginAt: string | null;
}

export interface AuthDevice {
  id: string;
  deviceName: string;
  trusted: boolean;
  firstSeenAt: string;
  lastSeenAt: string;
}

export interface LicenseStatus {
  status: 'missing' | 'active' | 'expired' | 'revoked';
  isActive: boolean;
  plan: string;
  licenseType: string;
  startsAt: string | null;
  expiresAt: string | null;
  maxDevices: number;
  message: string;
}

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  account: AuthAccount;
  device: AuthDevice;
  license: LicenseStatus;
}

export interface AuthState {
  authenticated: boolean;
  account: AuthAccount | null;
  device: AuthDevice | null;
  license: LicenseStatus;
  errorCode: string | null;
  message: string;
}

export interface RegisterAccountRequest {
  email: string;
  displayName: string;
  password: string;
  deviceFingerprint: string;
  deviceName: string;
  activationCode?: string;
}

export interface LoginRequest {
  identifier: string;
  password: string;
  deviceFingerprint: string;
  deviceName: string;
}

export interface ActivateLicenseRequest {
  activationCode: string;
  deviceFingerprint: string;
  deviceName: string;
}

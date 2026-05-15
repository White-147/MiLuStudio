export type ProjectMode = 'fast' | 'director';

export type ProjectStatus = 'draft' | 'running' | 'paused' | 'completed' | 'failed';

export type StageStatus = 'done' | 'running' | 'waiting' | 'review' | 'blocked' | 'failed';

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

export interface StoryboardShotEdit {
  shotId: string;
  durationSeconds: number;
  scene: string;
  visualAction: string;
  shotSize: string;
  cameraMovement: string;
  soundNote: string;
  dialogue: string;
  narration: string;
}

export interface StoryboardEditRequest {
  shots: StoryboardShotEdit[];
  notes: string;
}

export interface StoryboardShotRegenerateRequest {
  notes: string;
}

export interface StoryboardEditResponse {
  taskId: string;
  jobId: string;
  projectId: string;
  status: string;
  resetDownstreamTaskCount: number;
  message: string;
}

export interface StructuredOutputFieldEdit {
  path: string;
  value: unknown;
}

export interface StructuredOutputEditRequest {
  edits: StructuredOutputFieldEdit[];
  notes: string;
}

export interface StructuredOutputEditResponse {
  taskId: string;
  jobId: string;
  projectId: string;
  skillName: string;
  status: string;
  resetDownstreamTaskCount: number;
  message: string;
}

export interface ProviderSettingsResponse {
  mode: string;
  updatedAt: string;
  costGuardrails: ProviderCostGuardrails;
  adapters: ProviderAdapterSettings[];
  preflight: ProviderSettingsPreflight;
  safety: ProviderSafetyStatus;
}

export interface ProviderCostGuardrails {
  projectCostCapCny: number;
  retryLimit: number;
}

export interface ProviderAdapterSettings {
  kind: string;
  label: string;
  supplier: string;
  model: string;
  baseUrl: string;
  enabled: boolean;
  apiKeyConfigured: boolean;
  apiKeyPreview: string;
  secretFingerprint: string;
  supportedSuppliers: string[];
  capabilityFlags: string[];
  safety: ProviderAdapterSafety;
}

export interface ProviderSettingsPreflight {
  healthy: boolean;
  checks: ProviderPreflightCheck[];
  recommendations: string[];
}

export interface ProviderPreflightCheck {
  kind: string;
  label: string;
  status: 'ok' | 'warning' | 'error' | 'skipped';
  message: string;
  details: Record<string, string>;
}

export interface ProviderSafetyStatus {
  stage: string;
  mode: string;
  secretStore: ProviderSecretStoreStatus;
  spendGuard: ProviderSpendGuardStatus;
  sandbox: ProviderSandboxStatus;
  blockingReasons: string[];
}

export interface ProviderAdapterSafety {
  secretReferenceId: string;
  secretStoreMode: string;
  rawSecretPersisted: boolean;
  usableForProviderCalls: boolean;
  sandboxMode: string;
  providerCallsAllowed: boolean;
  externalNetworkAllowed: boolean;
  mediaReadAllowed: boolean;
  ffmpegAllowed: boolean;
}

export interface ProviderSecretStoreStatus {
  mode: string;
  storageScope: string;
  metadataStoreAvailable: boolean;
  rawSecretPersistenceAllowed: boolean;
  providerCallSecretsAvailable: boolean;
  checks: string[];
}

export interface ProviderSpendGuardStatus {
  enabled: boolean;
  enforcementMode: string;
  projectCostCapCny: number;
  retryLimit: number;
  blocksProviderCalls: boolean;
  blocksWhenCapExceeded: boolean;
}

export interface ProviderSandboxStatus {
  mode: string;
  providerCallsAllowed: boolean;
  externalNetworkAllowed: boolean;
  mediaReadAllowed: boolean;
  ffmpegAllowed: boolean;
  allowedAdapterKinds: string[];
  outputContract: string[];
}

export interface ProviderSpendGuardCheckRequest {
  projectId: string;
  providerKind: string;
  currentSpendCny: number;
  estimatedIncrementCny: number;
  attemptNumber: number;
}

export interface ProviderSpendGuardDecision {
  budgetAllowed: boolean;
  providerCallAllowed: boolean;
  decision: string;
  reason: string;
  projectCostCapCny: number;
  currentSpendCny: number;
  estimatedIncrementCny: number;
  projectedSpendCny: number;
  retryLimit: number;
  attemptNumber: number;
  appliedRules: string[];
}

export interface ProviderSettingsUpdateRequest {
  costGuardrails: ProviderCostGuardrails;
  adapters: ProviderAdapterUpdateRequest[];
}

export interface ProviderAdapterUpdateRequest {
  kind: string;
  supplier: string;
  model: string;
  baseUrl?: string | null;
  enabled: boolean;
  apiKey?: string | null;
  clearApiKey: boolean;
}

export interface ProviderConnectionTestRequest {
  supplier?: string | null;
  model?: string | null;
  baseUrl?: string | null;
  apiKey?: string | null;
}

export interface ProviderConnectionTestResponse {
  ok: boolean;
  status: string;
  message: string;
  providerKind: string;
  supplier: string;
  model: string;
  baseUrl: string;
  httpStatusCode: number | null;
  durationMs: number;
  checkedEndpoints: string[];
  details: Record<string, string>;
}

export type SystemDependencyStatus = 'ok' | 'warning' | 'error' | 'skipped';

export interface SystemDependencyCheck {
  id: string;
  status: SystemDependencyStatus;
  message: string;
  details: Record<string, string>;
}

export interface SystemDependenciesReport {
  status: 'ready' | 'attention_required';
  repositoryProvider: string;
  installStrategy: {
    preferred: string;
    onlineDownload: string;
    managedBy: string;
  };
  dependencies: SystemDependencyCheck[];
  recommendations: string[];
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

export type ProjectAssetUploadIntent = 'storyText' | 'imageReference' | 'videoReference' | 'storyboardReference' | 'reference';

export interface ProjectAssetUploadResponse extends ProjectAssetRecord {
  originalFileName: string;
  extractedText: string | null;
  message: string;
}

export interface ProjectAssetUploadSessionCreateRequest {
  intent: ProjectAssetUploadIntent;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  chunkSize?: number | null;
}

export interface ProjectAssetUploadSessionResponse {
  id: string;
  projectId: string;
  kind: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  intent: string | null;
  chunkSize: number;
  totalChunks: number;
  uploadedChunks: number[];
  status: string;
  assetId: string | null;
  createdAt: string;
  expiresAt: string;
}

export interface ProjectAssetChunkUploadResponse {
  sessionId: string;
  chunkIndex: number;
  bytesReceived: number;
  sha256: string;
  uploadedChunks: number[];
  readyToComplete: boolean;
}

export interface ProjectAssetUploadCompleteResponse {
  asset: ProjectAssetUploadResponse;
  session: ProjectAssetUploadSessionResponse;
}

export interface ProjectAssetAnalysisBoundary {
  uiElectronFileAccess: boolean | null;
  generationPayloadSent: boolean | null;
  modelProviderUsed: boolean | null;
  backendAdapterOnly: boolean | null;
}

export interface ProjectAssetChunkManifestSummary {
  status: string;
  strategy: string;
  totalChunks: number;
  chunkSizeCharacters: number;
  overlapCharacters: number;
  usableAsStoryCandidate: boolean;
}

export interface ProjectAssetDerivativeSummary {
  count: number;
  kinds: string[];
  accessPolicy: string;
}

export interface ProjectAssetOcrSummary {
  engine: string | null;
  status: string;
  candidate: boolean;
  runtimeAvailable: boolean;
  invoked: boolean;
  checkedPathCount: number;
  language: string | null;
  extractedTextLength: number;
  uiElectronFileAccess: boolean | null;
  modelProviderUsed: boolean | null;
}

export interface ProjectAssetAnalysisResponse {
  id: string;
  projectId: string;
  kind: string;
  mimeType: string;
  fileSize: number;
  sha256: string | null;
  createdAt: string;
  originalFileName: string | null;
  stage: string | null;
  analysisSchemaVersion: string | null;
  boundary: ProjectAssetAnalysisBoundary;
  chunkManifestSummary: ProjectAssetChunkManifestSummary;
  parse: unknown;
  upload: unknown;
  parser: unknown;
  ocr: ProjectAssetOcrSummary;
  text: unknown;
  contentBlocks: unknown;
  chunkManifest: unknown;
  documentStructure: unknown;
  limits: unknown;
  derivatives: ProjectAssetDerivativeSummary;
  metadataJsonParsed: boolean;
  metadataParseError: string | null;
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
  status: 'missing' | 'active' | 'expired' | 'revoked' | 'not_required';
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
}

export interface LoginRequest {
  identifier: string;
  password: string;
  deviceFingerprint: string;
  deviceName: string;
}

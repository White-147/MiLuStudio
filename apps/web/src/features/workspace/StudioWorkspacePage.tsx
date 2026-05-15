import {
  CheckCircle2,
  ChevronRight,
  Clock3,
  Circle,
  File as FileIcon,
  FileText,
  Film,
  Folder,
  ImageIcon,
  Loader2,
  LogOut,
  MessageSquarePlus,
  MonitorCog,
  PackageCheck,
  PanelLeftClose,
  PanelLeftOpen,
  Plus,
  RefreshCw,
  Search,
  SendHorizontal,
  Settings,
  SlidersHorizontal,
  Sparkles,
  Trash2,
  UserCircle2,
  WalletCards,
  X,
} from 'lucide-react';
import type { CSSProperties, KeyboardEvent as ReactKeyboardEvent, PointerEvent as ReactPointerEvent } from 'react';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { DesktopDiagnosticsPanel } from '../diagnostics/DesktopDiagnosticsPanel';
import { DependencySettingsPage } from '../settings/DependencySettingsPage';
import { ProviderSettingsPage } from '../settings/ProviderSettingsPage';
import {
  approveProductionCheckpoint,
  createProject,
  deleteProject,
  getProductionJob,
  getProject,
  listProductionTasks,
  listProjectAssets,
  listProjects,
  regenerateStoryboardShot,
  rejectProductionCheckpoint,
  rollbackProductionJob,
  startProductionJob,
  updateStructuredOutputTask,
  updateProject,
  updateStoryboardTask,
  uploadProjectAsset,
  watchProductionJob,
} from '../../shared/api/controlPlaneClient';
import type {
  AuthState,
  GenerationTaskRecord,
  GenerationTaskRecordStatus,
  ProductionJob,
  ProductionJobEvent,
  ProductionStage,
  ProjectAssetRecord,
  ProjectAssetUploadResponse,
  ProjectDetail,
  ProjectStatus,
  ProjectSummary,
  ProjectUpdateRequest,
  StageStatus,
  StoryboardShotEdit,
} from '../../shared/types/production';

interface StudioWorkspacePageProps {
  authState: AuthState;
  onSignOut: () => Promise<void> | void;
}

type SettingsPanel = 'account' | 'dependencies' | 'diagnostics' | 'providers' | null;

type WorkspaceMessage = {
  id: string;
  role: 'assistant' | 'user';
  title: string;
  body: string;
  time: string;
};

type SkillEnvelope = {
  ok?: boolean;
  data?: Record<string, unknown>;
  error?: {
    message?: string;
  };
};

type StoryboardShotDraft = StoryboardShotEdit & {
  label: string;
  startSecond: number;
  regenerateNotes: string;
};

type EditableStructuredSkill =
  | 'character_bible'
  | 'style_bible'
  | 'image_prompt_builder'
  | 'video_prompt_builder';

type JsonObjectValue = Record<string, unknown>;

type CharacterDraft = {
  character_id: string;
  name: string;
  role_type: string;
  identity: string;
  appearanceText: string;
  costumeText: string;
  voiceProfileText: string;
  continuityRulesText: string;
  source: JsonObjectValue;
};

type StylePaletteDraft = {
  name: string;
  hex: string;
  usage: string;
};

type StyleDraft = {
  styleName: string;
  visualStyleText: string;
  colorPalette: StylePaletteDraft[];
  cameraLanguageText: string;
  negativePromptText: string;
  reusablePromptBlocksText: string;
  imagePromptGuidelinesText: string;
  videoPromptGuidelinesText: string;
  continuityNotesText: string;
};

type PromptRequestDraft = {
  request_id: string;
  label: string;
  prompt: string;
  negativePromptText: string;
  selected: boolean;
  source: JsonObjectValue;
};

type PromptDraft = {
  requests: PromptRequestDraft[];
  negativePromptText: string;
  supportText: string;
};

type StructuredEditorDraft =
  | { skillName: 'character_bible'; characters: CharacterDraft[]; relationshipNotesText: string; continuityRulesText: string }
  | { skillName: 'style_bible'; style: StyleDraft }
  | { skillName: 'image_prompt_builder'; prompt: PromptDraft }
  | { skillName: 'video_prompt_builder'; prompt: PromptDraft };

type StructuredDiffItem = {
  label: string;
  before: string;
  after: string;
};

type FlowItemState = 'done' | 'review' | 'active' | 'pending';

type ProductionFlowItem = {
  label: string;
  skillName: string;
  state: FlowItemState;
  needsReview: boolean;
};

type ActiveReview = {
  label: string;
  stage: ProductionStage;
  task: GenerationTaskRecord | null;
};

type CheckpointAction = 'approve' | 'reject' | null;

type ComposerAttachmentKind = 'storyText' | 'imageReference' | 'videoReference' | 'storyboardReference' | 'reference';
type UploadMenuKind = 'text' | 'image' | 'video';

type ComposerAttachment = {
  id: string;
  name: string;
  extension: string;
  mimeType: string;
  size: number;
  kind: ComposerAttachmentKind;
  file?: File;
  text?: string;
  assetId?: string;
  uploadStatus?: 'pending' | 'uploaded' | 'failed';
  message?: string;
};

type UploadMenuOption = {
  kind: UploadMenuKind;
  label: string;
  description: string;
  accept: string;
  allowedKinds: ComposerAttachmentKind[];
  enabled: boolean;
  disabledReason: string;
};

const SAVED_JOB_PREFIX = 'milu.workspace.latestJob.';
const SIDEBAR_WIDTH_STORAGE_KEY = 'milu.workspace.sidebarWidth';
const SIDEBAR_COLLAPSED_STORAGE_KEY = 'milu.workspace.sidebarCollapsed';
const SIDEBAR_DEFAULT_WIDTH = 320;
const SIDEBAR_MIN_WIDTH = 260;
const SIDEBAR_MAX_WIDTH = 420;
const TEXT_UPLOAD_MAX_BYTES = 50 * 1024 * 1024;
const IMAGE_UPLOAD_MAX_BYTES = 50 * 1024 * 1024;
const VIDEO_UPLOAD_MAX_BYTES = 1024 * 1024 * 1024;
const TEXT_ATTACHMENT_EXTENSIONS = new Set([
  'txt',
  'md',
  'markdown',
  'csv',
  'json',
  'srt',
  'ass',
  'vtt',
  'log',
  'xml',
  'yaml',
  'yml',
  'rtf',
  'docx',
  'doc',
  'pdf',
]);
const IMAGE_REFERENCE_EXTENSIONS = new Set(['png', 'jpg', 'jpeg', 'webp', 'gif', 'bmp', 'tif', 'tiff', 'avif', 'heic', 'heif']);
const VIDEO_REFERENCE_EXTENSIONS = new Set(['mp4', 'mov', 'webm', 'mkv', 'avi', 'm4v', 'wmv', 'flv', 'mpeg', 'mpg', 'ts', 'm2ts', '3gp']);
const STORYBOARD_REFERENCE_EXTENSIONS = new Set(['json', 'csv', 'srt']);
const STORY_TEXT_ACCEPT = '.txt,.md,.markdown,.csv,.json,.srt,.ass,.vtt,.log,.xml,.yaml,.yml,.rtf,.docx,.doc,.pdf,text/*,application/pdf';
const STORYBOARD_TEXT_ACCEPT = `${STORY_TEXT_ACCEPT},.json,.csv,.srt`;
const IMAGE_REFERENCE_ACCEPT = '.png,.jpg,.jpeg,.webp,.gif,.bmp,.tif,.tiff,.avif,.heic,.heif,image/*';
const VIDEO_REFERENCE_ACCEPT = '.mp4,.mov,.webm,.mkv,.avi,.m4v,.wmv,.flv,.mpeg,.mpg,.ts,.m2ts,.3gp,video/*';
const FIXED_PRODUCTION_FLOW: Array<{ label: string; skillName: string }> = [
  { label: '故事解析', skillName: 'story_intake' },
  { label: '短剧改编', skillName: 'plot_adaptation' },
  { label: '脚本生成', skillName: 'episode_writer' },
  { label: '角色设定', skillName: 'character_bible' },
  { label: '画风设定', skillName: 'style_bible' },
  { label: '分镜审核', skillName: 'storyboard_director' },
  { label: '图片提示词', skillName: 'image_prompt_builder' },
  { label: '图片资产', skillName: 'image_generation' },
  { label: '视频提示词', skillName: 'video_prompt_builder' },
  { label: '视频片段', skillName: 'video_generation' },
  { label: '配音任务', skillName: 'voice_casting' },
  { label: '字幕结构', skillName: 'subtitle_generator' },
  { label: '粗剪计划', skillName: 'auto_editor' },
  { label: '质量检查', skillName: 'quality_checker' },
  { label: '导出占位', skillName: 'export_packager' },
];

function clampSidebarWidth(value: number): number {
  return Math.max(SIDEBAR_MIN_WIDTH, Math.min(SIDEBAR_MAX_WIDTH, Math.round(value)));
}

function readStoredSidebarWidth(): number {
  if (typeof window === 'undefined') {
    return SIDEBAR_DEFAULT_WIDTH;
  }

  const value = Number(window.localStorage.getItem(SIDEBAR_WIDTH_STORAGE_KEY));
  return Number.isFinite(value) ? clampSidebarWidth(value) : SIDEBAR_DEFAULT_WIDTH;
}

function readStoredSidebarCollapsed(): boolean {
  if (typeof window === 'undefined') {
    return false;
  }

  return window.localStorage.getItem(SIDEBAR_COLLAPSED_STORAGE_KEY) === 'true';
}

export function StudioWorkspacePage({ authState, onSignOut }: StudioWorkspacePageProps) {
  const [projects, setProjects] = useState<ProjectSummary[]>([]);
  const [activeProjectId, setActiveProjectId] = useState<string | null>(null);
  const [project, setProject] = useState<ProjectDetail | null>(null);
  const [job, setJob] = useState<ProductionJob | null>(null);
  const [tasks, setTasks] = useState<GenerationTaskRecord[]>([]);
  const [assets, setAssets] = useState<ProjectAssetRecord[]>([]);
  const [draftText, setDraftText] = useState('');
  const [composerAttachments, setComposerAttachments] = useState<ComposerAttachment[]>([]);
  const [uploadMenuOpen, setUploadMenuOpen] = useState(false);
  const [messages, setMessages] = useState<WorkspaceMessage[]>([]);
  const [selectedTask, setSelectedTask] = useState<GenerationTaskRecord | null>(null);
  const [settingsMenuOpen, setSettingsMenuOpen] = useState(false);
  const [settingsPanel, setSettingsPanel] = useState<SettingsPanel>(null);
  const [streamJobId, setStreamJobId] = useState<string | null>(null);
  const [loadingProjects, setLoadingProjects] = useState(true);
  const [loadingProject, setLoadingProject] = useState(false);
  const [composerBusy, setComposerBusy] = useState(false);
  const [checkpointAction, setCheckpointAction] = useState<CheckpointAction>(null);
  const [checkpointNotes, setCheckpointNotes] = useState('');
  const [checkpointNotice, setCheckpointNotice] = useState('');
  const [rollbackConfirmSkill, setRollbackConfirmSkill] = useState<string | null>(null);
  const [rollbackActionSkill, setRollbackActionSkill] = useState<string | null>(null);
  const [deleteConfirmProjectId, setDeleteConfirmProjectId] = useState<string | null>(null);
  const [deletingProjectId, setDeletingProjectId] = useState<string | null>(null);
  const [notice, setNotice] = useState('');
  const [sidebarWidth, setSidebarWidth] = useState(readStoredSidebarWidth);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(readStoredSidebarCollapsed);
  const [sidebarDragging, setSidebarDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const uploadIntentRef = useRef<UploadMenuOption | null>(null);

  const sortedProjects = useMemo(
    () =>
      [...projects].sort(
        (left, right) => new Date(right.updatedAt).getTime() - new Date(left.updatedAt).getTime(),
      ),
    [projects],
  );
  const hasStoryTextAttachment = useMemo(
    () =>
      composerAttachments.some(
        (attachment) =>
          attachment.kind === 'storyText' && Boolean(attachment.file || attachment.text?.trim() || attachment.assetId),
      ),
    [composerAttachments],
  );
  const canSubmitComposer = project ? Boolean(draftText.trim() || composerAttachments.length) : hasStoryTextAttachment;
  const generatedResults = useMemo(
    () =>
      tasks
        .filter((task) => Boolean(task.outputJson) && (task.status === 'review' || task.status === 'completed'))
        .sort((left, right) => left.queueIndex - right.queueIndex),
    [tasks],
  );
  const productionFlow = useMemo(() => buildProductionFlow(job, tasks), [job, tasks]);
  const activeReview = useMemo(() => findActiveReview(job, tasks), [job, tasks]);
  const rollbackCandidateSkill = useMemo(() => findLatestConfirmedReviewSkill(job, tasks), [job, tasks]);
  const uploadOptions = useMemo(() => buildUploadMenuOptions(job, project), [job, project]);
  const completedFlowCount = productionFlow.filter((item) => item.state === 'done').length;
  const progress = Math.round((completedFlowCount / FIXED_PRODUCTION_FLOW.length) * 100);
  const canCheckpoint = Boolean(job && activeReview && job.status === 'paused');
  const isEmptyWorkspace = !project && messages.length === 0 && !loadingProject;
  const workspaceShellClassName = [
    'codex-workspace-shell',
    sidebarCollapsed ? 'sidebar-collapsed' : '',
    sidebarDragging ? 'sidebar-resizing' : '',
  ]
    .filter(Boolean)
    .join(' ');
  const workspaceShellStyle = { '--workspace-sidebar-width': `${sidebarWidth}px` } as CSSProperties;

  useEffect(() => {
    window.localStorage.setItem(SIDEBAR_WIDTH_STORAGE_KEY, String(sidebarWidth));
  }, [sidebarWidth]);

  useEffect(() => {
    window.localStorage.setItem(SIDEBAR_COLLAPSED_STORAGE_KEY, sidebarCollapsed ? 'true' : 'false');
  }, [sidebarCollapsed]);

  const collapseSidebar = () => {
    setSidebarCollapsed(true);
    setSettingsMenuOpen(false);
    setUploadMenuOpen(false);
  };

  const expandSidebar = () => {
    setSidebarCollapsed(false);
  };

  const startSidebarResize = (event: ReactPointerEvent<HTMLDivElement>) => {
    if (event.button !== 0 || sidebarCollapsed) {
      return;
    }

    event.preventDefault();
    const startX = event.clientX;
    const startWidth = sidebarWidth;
    setSidebarDragging(true);

    const handleMove = (moveEvent: PointerEvent) => {
      setSidebarWidth(clampSidebarWidth(startWidth + moveEvent.clientX - startX));
    };

    const stopResize = () => {
      setSidebarDragging(false);
      window.removeEventListener('pointermove', handleMove);
      window.removeEventListener('pointerup', stopResize);
      window.removeEventListener('pointercancel', stopResize);
    };

    window.addEventListener('pointermove', handleMove);
    window.addEventListener('pointerup', stopResize, { once: true });
    window.addEventListener('pointercancel', stopResize, { once: true });
  };

  const handleSidebarResizeKeyDown = (event: ReactKeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      event.preventDefault();
      setSidebarWidth((current) => clampSidebarWidth(current + (event.key === 'ArrowLeft' ? -16 : 16)));
    }

    if (event.key === 'Home') {
      event.preventDefault();
      setSidebarWidth(SIDEBAR_MIN_WIDTH);
    }

    if (event.key === 'End') {
      event.preventDefault();
      setSidebarWidth(SIDEBAR_MAX_WIDTH);
    }
  };

  const loadProjectSummaries = useCallback(async (signal?: AbortSignal) => {
    setLoadingProjects(true);
    try {
      const nextProjects = await listProjects(signal);
      setProjects(nextProjects);
      setNotice('');
    } catch (error) {
      setNotice(error instanceof Error ? error.message : '无法连接 Control API。');
    } finally {
      setLoadingProjects(false);
    }
  }, []);

  const refreshOutputs = useCallback(async (jobId: string, projectId: string, signal?: AbortSignal) => {
    const [nextTasks, nextAssets] = await Promise.all([
      listProductionTasks(jobId, signal),
      listProjectAssets(projectId, signal),
    ]);

    setTasks(nextTasks);
    setAssets(nextAssets);
    return nextTasks;
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void loadProjectSummaries(controller.signal);
    return () => controller.abort();
  }, [loadProjectSummaries]);

  useEffect(() => {
    if (!activeProjectId) {
      setProject(null);
      setJob(null);
      setTasks([]);
      setAssets([]);
      setSelectedTask(null);
      setStreamJobId(null);
      setDraftText('');
      setComposerAttachments([]);
      setUploadMenuOpen(false);
      setMessages([]);
      return;
    }

    const controller = new AbortController();
    setLoadingProject(true);

    getProject(activeProjectId, controller.signal)
      .then(async (nextProject) => {
        setProject(nextProject);
        setDraftText('');
        setComposerAttachments([]);
        setUploadMenuOpen(false);
        setMessages([
          {
            id: `project-${nextProject.id}`,
            role: 'assistant',
            title: nextProject.title,
            body: nextProject.description,
            time: formatTime(nextProject.updatedAt),
          },
        ]);

        const savedJobId = getSavedJobId(nextProject.id);
        if (!savedJobId) {
          setJob(null);
          setTasks([]);
          setAssets(await listProjectAssets(nextProject.id, controller.signal));
          return;
        }

        try {
          const nextJob = await getProductionJob(savedJobId, controller.signal);
          setJob(nextJob);
          await refreshOutputs(nextJob.id, nextJob.projectId, controller.signal);
          if (nextJob.status === 'queued' || nextJob.status === 'running') {
            setStreamJobId(nextJob.id);
          }
        } catch {
          removeSavedJobId(nextProject.id);
          setJob(null);
          setTasks([]);
          setAssets(await listProjectAssets(nextProject.id, controller.signal));
        }
      })
      .catch((error) => {
        if (!controller.signal.aborted) {
          setNotice(error instanceof Error ? error.message : '项目加载失败。');
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoadingProject(false);
        }
      });

    return () => controller.abort();
  }, [activeProjectId, refreshOutputs]);

  useEffect(() => {
    if (!streamJobId) {
      return undefined;
    }

    const stop = watchProductionJob(
      streamJobId,
      (event) => {
        setJob((current) => {
          if (!current || current.id !== event.jobId) {
            return current;
          }

          return {
            ...current,
            currentStage: event.stageId,
            progress: event.progress,
            status: event.jobStatus,
            stages: applyEventToStages(current.stages, event),
          };
        });
        setNotice(event.message || `${event.stageLabel}：${formatStageStatus(event.status)}`);
        void refreshOutputs(event.jobId, event.projectId).catch(() => undefined);

        if (event.jobStatus === 'completed' || event.jobStatus === 'failed') {
          setStreamJobId(null);
          void loadProjectSummaries();
        }
      },
      () => setNotice('生产事件连接中断，已保留当前任务状态。'),
    );

    return stop;
  }, [loadProjectSummaries, refreshOutputs, streamJobId]);

  useEffect(() => {
    setCheckpointNotes('');
    setCheckpointNotice('');
  }, [activeReview?.stage.id, job?.id]);

  const startNewProject = () => {
    setActiveProjectId(null);
    setDeleteConfirmProjectId(null);
    setDraftText('');
    setComposerAttachments([]);
    setUploadMenuOpen(false);
    setNotice('');
  };

  const submitComposer = async () => {
    if (!project && !hasStoryTextAttachment) {
      setNotice('请先通过加号上传剧本文本文件；输入框只填写制作要求。');
      return;
    }

    setComposerBusy(true);
    setNotice('正在上传解析附件并启动生产任务。');

    try {
      const initialProject = project ?? (await createProject());
      const uploadedAttachments = await uploadPendingComposerAttachments(initialProject.id, composerAttachments);
      setComposerAttachments(uploadedAttachments);

      const sourceProject = project ? initialProject : null;
      const rawText = buildSubmissionStoryText(draftText, uploadedAttachments, sourceProject);
      const text = rawText.trim();

      if (!text) {
        setNotice('文本附件已上传，但没有解析出可用于故事解析的正文；DOC/PDF/OCR 支持会在后续阶段继续补齐。');
        return;
      }

      const payload = buildProjectPayload(text, initialProject);
      const savedProject = await updateProject(initialProject.id, payload);
      setProject(savedProject);
      setActiveProjectId(savedProject.id);
      setMessages((current) => [
        ...current.filter((message) => message.id !== 'draft-submit'),
        {
          id: 'draft-submit',
          role: 'user',
          title: savedProject.title,
          body: previewText(savedProject.storyText, 220),
          time: formatTime(new Date().toISOString()),
        },
      ]);

      const nextJob = await startProductionJob(savedProject.id);
      setJob(nextJob);
      saveJobId(savedProject.id, nextJob.id);
      setStreamJobId(nextJob.id);
      await refreshOutputs(nextJob.id, savedProject.id);
      await loadProjectSummaries();
      setDraftText('');
      setComposerAttachments([]);
      setNotice('附件已上传解析，生产任务已启动。');
    } catch (error) {
      setNotice(error instanceof Error ? error.message : '启动生产任务失败。');
    } finally {
      setComposerBusy(false);
    }
  };

  const openUploadPicker = (option: UploadMenuOption) => {
    if (!option.enabled) {
      setNotice(option.disabledReason);
      return;
    }

    uploadIntentRef.current = option;
    setUploadMenuOpen(false);

    if (fileInputRef.current) {
      fileInputRef.current.accept = option.accept;
      fileInputRef.current.click();
    }
  };

  const uploadComposerFiles = async (fileList: FileList | null, option: UploadMenuOption | null) => {
    const files = Array.from(fileList ?? []);
    if (!files.length) {
      return;
    }

    if (!option?.enabled) {
      setNotice(option?.disabledReason ?? '当前阶段不能添加附件。');
      return;
    }

    const accepted: ComposerAttachment[] = [];
    const skipped: string[] = [];

    for (const file of files) {
      const kind = classifyComposerFile(file, option);
      if (!kind) {
        skipped.push(file.name);
        continue;
      }

      if (!option.allowedKinds.includes(kind)) {
        skipped.push(`${file.name}（${option.label}入口不支持该类型）`);
        continue;
      }

      const limit = uploadLimitForKind(kind);
      if (file.size > limit) {
        skipped.push(`${file.name}（超过 ${formatFileSize(limit)}）`);
        continue;
      }

      accepted.push(createComposerAttachment(file, kind));
    }

    if (accepted.length) {
      setComposerAttachments((current) => [...current, ...accepted]);
    }

    const textCount = accepted.filter((item) => item.kind === 'storyText').length;
    const referenceCount = accepted.length - textCount;
    const noticeParts = [
      accepted.length ? `已添加 ${accepted.length} 个附件` : '',
      textCount ? `${textCount} 个文本将在开始生成前上传解析` : '',
      referenceCount ? `${referenceCount} 个参考文件将在开始生成前上传解析` : '',
      skipped.length ? `未添加：${skipped.join('、')}` : '',
    ].filter(Boolean);

    setNotice(noticeParts.join('，') || '没有可添加的附件。');
  };

  const uploadPendingComposerAttachments = async (
    projectId: string,
    attachments: ComposerAttachment[],
  ): Promise<ComposerAttachment[]> => {
    const uploaded: ComposerAttachment[] = [];

    for (const attachment of attachments) {
      if (!attachment.file || attachment.assetId) {
        uploaded.push(attachment);
        continue;
      }

      try {
        const response = await uploadProjectAsset(projectId, attachment.file, attachment.kind);
        uploaded.push(applyUploadResponse(attachment, response));
      } catch (error) {
        uploaded.push({
          ...attachment,
          uploadStatus: 'failed',
          message: error instanceof Error ? error.message : '上传解析失败',
        });
        throw error;
      }
    }

    return uploaded;
  };

  const openSettingsPanel = (panel: Exclude<SettingsPanel, null>) => {
    setSettingsPanel(panel);
    setSettingsMenuOpen(false);
  };

  const handleProjectDelete = async (item: ProjectSummary) => {
    if (item.status === 'running' || item.status === 'paused') {
      setNotice('项目正在生成或暂停审核中，暂不能删除。');
      return;
    }

    if (deleteConfirmProjectId !== item.id) {
      setDeleteConfirmProjectId(item.id);
      setNotice(`再点一次删除“${item.title}”。`);
      return;
    }

    setDeletingProjectId(item.id);
    setNotice(`正在删除“${item.title}”。`);

    try {
      await deleteProject(item.id);
      removeSavedJobId(item.id);
      setProjects((current) => current.filter((projectItem) => projectItem.id !== item.id));
      setDeleteConfirmProjectId(null);

      if (item.id === activeProjectId) {
        setActiveProjectId(null);
      }

      setNotice(`已删除“${item.title}”。`);
      void loadProjectSummaries();
    } catch (error) {
      setNotice(error instanceof Error ? error.message : '删除项目失败。');
    } finally {
      setDeletingProjectId(null);
    }
  };

  const runCheckpointAction = async (approved: boolean) => {
    if (!job || !activeReview || job.status !== 'paused') {
      return;
    }

    const trimmedNotes = checkpointNotes.trim();
    if (!approved && !trimmedNotes) {
      setCheckpointNotice('退回修改前请写明需要调整的地方。');
      return;
    }

    const action: Exclude<CheckpointAction, null> = approved ? 'approve' : 'reject';
    setCheckpointAction(action);
    setCheckpointNotice('');
    setNotice(approved ? '正在通过当前审核。' : '正在退回当前审核。');

    try {
      const nextJob = approved
        ? await approveProductionCheckpoint(job.id, trimmedNotes)
        : await rejectProductionCheckpoint(job.id, trimmedNotes);

      setJob(nextJob);
      await refreshOutputs(nextJob.id, nextJob.projectId);
      await loadProjectSummaries();
      setCheckpointNotes('');
      setNotice(approved ? '已通过当前审核，任务会继续进入下一步。' : '已退回当前审核，任务已暂停在可重试状态。');

      if (approved && nextJob.status === 'running') {
        setStreamJobId(nextJob.id);
      }
    } catch (error) {
      setCheckpointNotice(error instanceof Error ? error.message : 'Control API 暂时没有接受审核操作。');
    } finally {
      setCheckpointAction(null);
    }
  };

  const runRollbackAction = async (skillName: string, label: string) => {
    if (!job) {
      return;
    }

    setRollbackConfirmSkill(skillName);
    const confirmed = window.confirm(`确认回退到「${label}」待审核？该步骤之后的所有任务输出会清空，并重新等待计算。`);
    if (!confirmed) {
      setRollbackConfirmSkill(null);
      return;
    }

    setRollbackActionSkill(skillName);
    setNotice(`正在回退「${label}」。`);

    try {
      const nextJob = await rollbackProductionJob(job.id, skillName, `用户回退到「${label}」待审核。`);
      setJob(nextJob);
      await refreshOutputs(nextJob.id, nextJob.projectId);
      await loadProjectSummaries();
      setCheckpointNotes('');
      setNotice(`已回退到「${label}」待审核，下游步骤已重置。`);
    } catch (error) {
      setNotice(error instanceof Error ? error.message : '回退失败。');
    } finally {
      setRollbackActionSkill(null);
      setRollbackConfirmSkill(null);
    }
  };

  const renderFlowStatus = (item: ProductionFlowItem, index: number) => {
    const isFirstVisiblePending =
      item.state === 'pending' &&
      (index === 0 || productionFlow[index - 1].state !== 'pending') &&
      !productionFlow.slice(0, index).some((candidate) => candidate.state === 'active' || candidate.state === 'review');

    if (!job && index === 0) {
      return <span className="flow-status-label">未开始</span>;
    }

    if (item.state === 'active') {
      return <span className="flow-status-label active">进行中</span>;
    }

    if (item.state === 'review') {
      return (
        <button
          className="flow-status-button review"
          disabled={!canCheckpoint || checkpointAction !== null}
          onClick={() => void runCheckpointAction(true)}
          title="确认当前审核"
          type="button"
        >
          待确认
        </button>
      );
    }

    if (item.state === 'done') {
      if (!item.needsReview) {
        return <span className="flow-status-label done">已完成</span>;
      }

      const canRollback = rollbackCandidateSkill === item.skillName;
      return (
        <button
          className="flow-status-button confirmed"
          disabled={!canRollback || rollbackActionSkill === item.skillName || rollbackConfirmSkill === item.skillName}
          onClick={() => void runRollbackAction(item.skillName, item.label)}
          title={canRollback ? '回退到当前步骤待审核' : '只能回退最近一个已确认审核步骤'}
          type="button"
        >
          {rollbackActionSkill === item.skillName ? (
            <Loader2 className="spin" size={13} />
          ) : (
            <>
              <span className="normal-label">已确认</span>
              <span className="hover-label">回退</span>
            </>
          )}
        </button>
      );
    }

    if (isFirstVisiblePending) {
      return <span className="flow-status-label">未开始</span>;
    }

    return null;
  };

  return (
    <div className={workspaceShellClassName} style={workspaceShellStyle}>
      {sidebarCollapsed && (
        <button
          aria-label="展开项目栏"
          className="workspace-sidebar-expand-button"
          onClick={expandSidebar}
          title="展开项目栏"
          type="button"
        >
          <PanelLeftOpen size={15} />
        </button>
      )}

      <aside className="workspace-sidebar" aria-label="历史项目">
        <button
          aria-label="收起项目栏"
          className="workspace-sidebar-collapse-button"
          onClick={collapseSidebar}
          title="收起项目栏"
          type="button"
        >
          <PanelLeftClose size={15} />
        </button>

        <div className="workspace-brand" aria-label="麋鹿">
          <img alt="" className="workspace-brand-logo" src="/brand/logo.png" />
          <span>麋鹿</span>
        </div>

        <div className="workspace-history">
          <div className="workspace-section-label">
            <span>项目</span>
            <div className="workspace-section-actions">
              {loadingProjects && <Loader2 className="spin" size={14} />}
              <button
                aria-label="搜索项目"
                className="workspace-section-action"
                onClick={() => void loadProjectSummaries()}
                title="搜索"
                type="button"
              >
                <Search size={15} />
              </button>
              <button
                aria-label="新项目"
                className="workspace-section-action"
                onClick={startNewProject}
                title="新项目"
                type="button"
              >
                <MessageSquarePlus size={15} />
              </button>
            </div>
          </div>
          <div className="workspace-project-list">
            {sortedProjects.map((item) => (
              <div
                className={item.id === activeProjectId ? 'workspace-project active' : 'workspace-project'}
                key={item.id}
              >
                <button
                  className="workspace-project-main"
                  onClick={() => {
                    setDeleteConfirmProjectId(null);
                    setActiveProjectId(item.id);
                  }}
                  type="button"
                >
                  <span className="workspace-project-title">{item.title}</span>
                  <span className="workspace-project-time">{formatRelativeProjectTime(item.updatedAt)}</span>
                </button>
                <button
                  aria-label={`删除项目 ${item.title}`}
                  className={deleteConfirmProjectId === item.id ? 'workspace-project-delete confirming' : 'workspace-project-delete'}
                  disabled={deletingProjectId !== null}
                  onClick={() => void handleProjectDelete(item)}
                  title={item.status === 'running' || item.status === 'paused' ? '生成中或暂停审核中的项目暂不能删除' : '删除项目'}
                  type="button"
                >
                  {deletingProjectId === item.id ? (
                    <Loader2 className="spin" size={14} />
                  ) : deleteConfirmProjectId === item.id ? (
                    <span>确认</span>
                  ) : (
                    <Trash2 size={14} />
                  )}
                </button>
              </div>
            ))}
            {!loadingProjects && sortedProjects.length === 0 && (
              <p className="workspace-empty-copy">暂无项目</p>
            )}
          </div>
        </div>

        <div className="workspace-settings-anchor">
          {settingsMenuOpen && (
            <div className="workspace-settings-menu" role="menu">
              <div className="settings-menu-account">
                <UserCircle2 size={16} />
                <span>{authState.account?.email ?? authState.account?.displayName ?? '本机账户'}</span>
              </div>
              <button onClick={() => openSettingsPanel('diagnostics')} type="button">
                <MonitorCog size={16} />
                <span>诊断</span>
              </button>
              <button onClick={() => openSettingsPanel('account')} type="button">
                <UserCircle2 size={16} />
                <span>个人账户</span>
              </button>
              <div className="settings-menu-separator" />
              <button className="provider-menu-entry" onClick={() => openSettingsPanel('providers')} type="button">
                <SlidersHorizontal size={16} />
                <span className="provider-menu-label">模型</span>
                <span>设置</span>
              </button>
              <button onClick={() => openSettingsPanel('dependencies')} type="button">
                <PackageCheck size={16} />
                <span>依赖</span>
              </button>
              <button onClick={() => setNotice('当前版本未接入真实计费，暂无余额数据。')} type="button">
                <WalletCards size={16} />
                <span>剩余额度</span>
                <ChevronRight className="settings-menu-chevron" size={15} />
              </button>
              <button onClick={() => void onSignOut()} type="button">
                <LogOut size={16} />
                <span>退出登录</span>
              </button>
            </div>
          )}
          <button
            className={settingsMenuOpen ? 'workspace-settings-button active' : 'workspace-settings-button'}
            onClick={() => setSettingsMenuOpen((current) => !current)}
            type="button"
          >
            <Settings size={17} />
            <span>设置</span>
          </button>
        </div>

        <div
          aria-label="调整项目栏宽度"
          aria-orientation="vertical"
          className="workspace-sidebar-resizer"
          onDoubleClick={() => setSidebarWidth(SIDEBAR_DEFAULT_WIDTH)}
          onKeyDown={handleSidebarResizeKeyDown}
          onPointerDown={startSidebarResize}
          role="separator"
          tabIndex={0}
          title="拖拽调整项目栏宽度"
        />
      </aside>

      <main className={isEmptyWorkspace ? 'workspace-main empty' : 'workspace-main'}>
        {(project || messages.length > 0) && (
          <header className="workspace-thread-header">
            <div>
              <p className="eyebrow">{project ? formatProjectStatus(project.status) : '新项目'}</p>
              <h1>{project?.title ?? 'MiLuStudio'}</h1>
            </div>
            <button className="ghost-button" onClick={() => void loadProjectSummaries()} type="button">
              <RefreshCw size={16} />
              <span>刷新</span>
            </button>
          </header>
        )}

        <div className="workspace-thread">
          {loadingProject && (
            <div className="workspace-loading-line">
              <Loader2 className="spin" size={18} />
              <span>正在读取项目</span>
            </div>
          )}

          {messages.map((message) => (
            <article className={`workspace-message ${message.role}`} key={message.id}>
              <div className="message-avatar">{message.role === 'assistant' ? <Sparkles size={16} /> : <UserCircle2 size={16} />}</div>
              <div className="message-bubble">
                <div className="message-heading">
                  <strong>{message.title}</strong>
                  <span>{message.time}</span>
                </div>
                <p>{message.body}</p>
              </div>
            </article>
          ))}

          {isEmptyWorkspace && <div className="workspace-empty-spacer" />}
        </div>

        <section className="workspace-composer" aria-label="项目输入">
          {composerAttachments.length > 0 && (
            <div className="composer-attachments" aria-label="已添加附件">
              {composerAttachments.map((attachment) => (
                <div className={`composer-attachment ${attachment.kind}`} key={attachment.id}>
                  <span className="composer-attachment-icon">{attachmentIcon(attachment.kind)}</span>
                  <span className="composer-attachment-copy">
                    <strong>{attachment.name}</strong>
                    <span>
                      {attachmentKindLabel(attachment.kind)} · {attachment.extension.toUpperCase() || '文件'} · {formatFileSize(attachment.size)} ·{' '}
                      {formatUploadStatus(attachment)}
                    </span>
                  </span>
                  <button
                    aria-label={`移除 ${attachment.name}`}
                    className="composer-attachment-remove"
                    onClick={() => setComposerAttachments((current) => current.filter((item) => item.id !== attachment.id))}
                    type="button"
                  >
                    <X size={14} />
                  </button>
                </div>
              ))}
            </div>
          )}
          <textarea
            placeholder="上传剧本文档后，写下本次制作要求"
            value={draftText}
            onChange={(event) => setDraftText(event.target.value)}
          />
          <div className="workspace-composer-footer">
            <div className="composer-left-actions">
              <input
                accept={STORY_TEXT_ACCEPT}
                className="sr-only"
                multiple
                ref={fileInputRef}
                type="file"
                onChange={(event) => {
                  void uploadComposerFiles(event.currentTarget.files, uploadIntentRef.current);
                  event.currentTarget.value = '';
                  uploadIntentRef.current = null;
                }}
              />
              <div className="composer-upload-anchor">
                <button
                  aria-expanded={uploadMenuOpen}
                  aria-label="添加附件"
                  className="composer-upload-trigger"
                  onClick={() => setUploadMenuOpen((open) => !open)}
                  title="添加附件"
                  type="button"
                >
                  <Plus size={18} />
                </button>
                {uploadMenuOpen && (
                  <div className="composer-upload-menu" role="menu">
                    {uploadOptions.map((option) => (
                      <button
                        className="composer-upload-menu-item"
                        disabled={!option.enabled}
                        key={option.kind}
                        onClick={() => openUploadPicker(option)}
                        title={option.enabled ? option.description : option.disabledReason}
                        type="button"
                      >
                        <span className="composer-upload-menu-icon">{uploadMenuIcon(option.kind)}</span>
                        <span>
                          <strong>{option.label}</strong>
                          <small>{option.enabled ? option.description : option.disabledReason}</small>
                        </span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>
            <button className="composer-submit-button" disabled={composerBusy || !canSubmitComposer} onClick={submitComposer} type="button">
              {composerBusy ? <Loader2 className="spin" size={17} /> : <SendHorizontal size={17} />}
              <span>{project ? '更新生成' : '开始生成'}</span>
            </button>
          </div>
          {notice && (
            <p className={notice.includes('失败') || notice.includes('无法') || notice.includes('需要') ? 'workspace-notice warn' : 'workspace-notice'}>
              {notice}
            </p>
          )}
        </section>
      </main>

      <aside className="workspace-run-panel" aria-label="当前项目进度与生成结果">
        <section className="run-card">
          <div className="run-panel-header">
            <div>
              <p className="eyebrow">进度</p>
              <h2>{project?.title ?? '等待项目'}</h2>
            </div>
            <span className="pin-dot" aria-hidden="true" />
          </div>

          <div className="project-progress-summary">
            <span>{completedFlowCount}/{FIXED_PRODUCTION_FLOW.length}</span>
          </div>
          <div className="progress-meter" aria-label={`当前项目固定流程完成度 ${progress}%`}>
            <span style={{ width: `${progress}%` }} />
          </div>

          <div className="fixed-flow-list" aria-label="固定生产流程">
            {productionFlow.map((item, index) => (
              <div className={`fixed-flow-item ${item.state}`} key={item.skillName}>
                <span className="fixed-flow-icon">
                  {item.state === 'done' ? (
                    <CheckCircle2 size={16} />
                  ) : item.state === 'review' ? (
                    <Clock3 size={16} />
                  ) : item.state === 'active' ? (
                    <Loader2 className="spin" size={16} />
                  ) : (
                    <Circle size={16} />
                  )}
                </span>
                <span>{item.label}</span>
                {renderFlowStatus(item, index)}
              </div>
            ))}
          </div>

          {activeReview && (
            <section className="checkpoint-review-panel" aria-label="当前审核">
              <div className="checkpoint-review-heading">
                <div>
                  <span>待审核</span>
                  <strong>{activeReview.label}</strong>
                </div>
                <span className="status-pill review">待确认</span>
              </div>
              <p>
                {activeReview.task
                  ? summarizeTask(activeReview.task, parseSkillEnvelope(activeReview.task.outputJson))
                  : '当前步骤已暂停，等待 Control API 写入可审核产物。'}
              </p>
              <textarea
                aria-label="审核备注"
                value={checkpointNotes}
                onChange={(event) => {
                  setCheckpointNotes(event.target.value);
                  if (event.target.value.trim()) {
                    setCheckpointNotice('');
                  }
                }}
                placeholder="可填写通过备注；如果退回，请写明需要修改的地方。"
              />
              <div className="checkpoint-review-actions">
                <button
                  className="secondary-button danger"
                  disabled={!canCheckpoint || checkpointAction !== null}
                  onClick={() => void runCheckpointAction(false)}
                  type="button"
                >
                  {checkpointAction === 'reject' ? <Loader2 className="spin" size={16} /> : <X size={16} />}
                  <span>退回修改</span>
                </button>
                <button
                  className="primary-button"
                  disabled={!canCheckpoint || checkpointAction !== null}
                  onClick={() => void runCheckpointAction(true)}
                  type="button"
                >
                  {checkpointAction === 'approve' ? <Loader2 className="spin" size={16} /> : <CheckCircle2 size={16} />}
                  <span>通过审核</span>
                </button>
              </div>
              {checkpointNotice && <p className="checkpoint-review-notice">{checkpointNotice}</p>}
            </section>
          )}

          <div className="run-panel-divider" />

          <div className="run-results-section">
            <div className="run-section-title">
              <FileText size={16} />
              <span>生成结果</span>
            </div>
            <div className="result-list">
              {generatedResults.map((task) => {
                const envelope = parseSkillEnvelope(task.outputJson);

                return (
                  <article className="result-row" key={task.id}>
                    <FileText className="result-file-icon" size={16} />
                    <div className="result-row-main">
                      <span className="result-row-title-line">
                        <strong>{resultFileName(task)}</strong>
                        {task.status === 'review' && <span className="status-pill review">待审核</span>}
                      </span>
                      <small>{summarizeTask(task, envelope)}</small>
                    </div>
                    <button className="result-open-button" onClick={() => setSelectedTask(task)} type="button">
                      打开
                    </button>
                  </article>
                );
              })}
              {assets.map((asset) => (
                <button className="result-row asset-result-row" key={asset.id} onClick={() => setNotice(`产物路径：${asset.localPath}`)} type="button">
                  <Folder className="result-file-icon" size={16} />
                  <span className="result-row-main">
                    <strong>{asset.kind}</strong>
                    <small>{formatFileSize(asset.fileSize)}</small>
                  </span>
                  <ChevronRight size={15} />
                </button>
              ))}
              {generatedResults.length === 0 && assets.length === 0 && <p className="workspace-empty-copy">暂无生成结果</p>}
            </div>
          </div>
        </section>
      </aside>

      {selectedTask && (
        <TaskPreviewPanel
          task={selectedTask}
          onClose={() => setSelectedTask(null)}
          onRefresh={async () => {
            if (job && project) {
              const refreshedTasks = await refreshOutputs(job.id, project.id);
              setSelectedTask((current) => {
                if (!current) {
                  return current;
                }

                return refreshedTasks.find((item) => item.id === current.id) ?? current;
              });
            }
          }}
        />
      )}

      {settingsPanel && (
        <SettingsPanelOverlay
          authState={authState}
          panel={settingsPanel}
          onClose={() => setSettingsPanel(null)}
          onSignOut={onSignOut}
        />
      )}
    </div>
  );
}

function TaskPreviewPanel({
  task,
  onClose,
  onRefresh,
}: {
  task: GenerationTaskRecord;
  onClose: () => void;
  onRefresh: () => Promise<void>;
}) {
  const envelope = useMemo(() => parseSkillEnvelope(task.outputJson), [task.outputJson]);
  const [shots, setShots] = useState<StoryboardShotDraft[]>(() => extractStoryboardShots(envelope));
  const [structuredDraft, setStructuredDraft] = useState<StructuredEditorDraft | null>(() =>
    createStructuredEditorDraft(task.skillName, envelope),
  );
  const [notes, setNotes] = useState('');
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);
  const isStoryboard = task.skillName === 'storyboard_director' && shots.length > 0;
  const structuredDiff = useMemo(
    () => buildStructuredDiff(task.skillName, envelope, structuredDraft),
    [envelope, structuredDraft, task.skillName],
  );
  const canEditStructuredOutput = Boolean(structuredDraft);

  useEffect(() => {
    setShots(extractStoryboardShots(envelope));
    setStructuredDraft(createStructuredEditorDraft(task.skillName, envelope));
    setNotes('');
    setMessage('');
  }, [envelope, task.id, task.skillName]);

  const updateShot = <K extends keyof StoryboardShotDraft>(shotId: string, key: K, value: StoryboardShotDraft[K]) => {
    setShots((current) => current.map((shot) => (shot.shotId === shotId ? { ...shot, [key]: value } : shot)));
  };

  const saveStoryboard = async () => {
    setBusy(true);
    setMessage('正在保存分镜。');
    try {
      const response = await updateStoryboardTask(task.id, {
        notes,
        shots: shots.map(toStoryboardShotEdit),
      });
      await onRefresh();
      setMessage(response.message);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '保存分镜失败。');
    } finally {
      setBusy(false);
    }
  };

  const regenerateShot = async (shot: StoryboardShotDraft) => {
    const trimmedNotes = shot.regenerateNotes.trim();
    if (!trimmedNotes) {
      setMessage('请先填写单镜头重算备注。');
      return;
    }

    setBusy(true);
    setMessage(`正在重算 ${shot.label}。`);
    try {
      const response = await regenerateStoryboardShot(task.id, shot.shotId, { notes: trimmedNotes });
      await onRefresh();
      setMessage(response.message);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '镜头重算失败。');
    } finally {
      setBusy(false);
    }
  };

  const saveStructuredOutput = async () => {
    if (!structuredDraft) {
      return;
    }

    setBusy(true);
    setMessage('正在保存结构化产物。');

    try {
      const edits = buildStructuredOutputEdits(structuredDraft);
      if (edits.length === 0) {
        setMessage('当前结果暂时没有可保存的结构化字段。');
        return;
      }

      const response = await updateStructuredOutputTask(task.id, {
        edits,
        notes,
      });
      await onRefresh();
      setMessage(response.message);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '保存结构化产物失败。');
    } finally {
      setBusy(false);
    }
  };

  const updateCharacterDraft = <K extends keyof CharacterDraft>(
    characterId: string,
    key: K,
    value: CharacterDraft[K],
  ) => {
    setStructuredDraft((current) => {
      if (!current || current.skillName !== 'character_bible') {
        return current;
      }

      return {
        ...current,
        characters: current.characters.map((character) =>
          character.character_id === characterId ? { ...character, [key]: value } : character,
        ),
      };
    });
  };

  const updateStyleDraft = <K extends keyof StyleDraft>(key: K, value: StyleDraft[K]) => {
    setStructuredDraft((current) => {
      if (!current || current.skillName !== 'style_bible') {
        return current;
      }

      return {
        ...current,
        style: {
          ...current.style,
          [key]: value,
        },
      };
    });
  };

  const updateStylePalette = <K extends keyof StylePaletteDraft>(index: number, key: K, value: StylePaletteDraft[K]) => {
    setStructuredDraft((current) => {
      if (!current || current.skillName !== 'style_bible') {
        return current;
      }

      return {
        ...current,
        style: {
          ...current.style,
          colorPalette: current.style.colorPalette.map((color, colorIndex) =>
            colorIndex === index ? { ...color, [key]: value } : color,
          ),
        },
      };
    });
  };

  const updatePromptDraft = <K extends keyof PromptRequestDraft>(
    requestId: string,
    key: K,
    value: PromptRequestDraft[K],
  ) => {
    setStructuredDraft((current) => {
      if (!current || (current.skillName !== 'image_prompt_builder' && current.skillName !== 'video_prompt_builder')) {
        return current;
      }

      return {
        ...current,
        prompt: {
          ...current.prompt,
          requests: current.prompt.requests.map((request) =>
            request.request_id === requestId ? { ...request, [key]: value } : request,
          ),
        },
      };
    });
  };

  const updatePromptSupportText = (value: string) => {
    setStructuredDraft((current) => {
      if (!current || (current.skillName !== 'image_prompt_builder' && current.skillName !== 'video_prompt_builder')) {
        return current;
      }

      return {
        ...current,
        prompt: {
          ...current.prompt,
          supportText: value,
        },
      };
    });
  };

  const updatePromptNegativeText = (value: string) => {
    setStructuredDraft((current) => {
      if (!current || (current.skillName !== 'image_prompt_builder' && current.skillName !== 'video_prompt_builder')) {
        return current;
      }

      return {
        ...current,
        prompt: {
          ...current.prompt,
          negativePromptText: value,
        },
      };
    });
  };

  return (
    <div className="workspace-modal-backdrop">
      <section className="task-preview-panel" aria-label="结果预览">
        <header className="modal-heading">
          <div>
            <p className="eyebrow">{formatTaskStatus(task.status)}</p>
            <h2>{stageLabel(task.skillName)}</h2>
          </div>
          <button className="icon-button" onClick={onClose} type="button" aria-label="关闭">
            <X size={18} />
          </button>
        </header>

        <div className="task-preview-body">
          <section className="task-summary-block">
            <h3>预览</h3>
            <p>{summarizeTask(task, envelope)}</p>
            <small>{buildTaskPreview(task, envelope)}</small>
          </section>

          {isStoryboard && (
            <section className="storyboard-lite-editor">
              <div className="storyboard-lite-heading">
                <h3>分镜表</h3>
                <textarea
                  placeholder="整体修改备注"
                  value={notes}
                  onChange={(event) => setNotes(event.target.value)}
                />
              </div>
              <div className="storyboard-lite-list">
                {shots.map((shot) => (
                  <article className="storyboard-lite-row" key={shot.shotId}>
                    <div className="storyboard-row-head">
                      <strong>{shot.label}</strong>
                      <label>
                        <Clock3 size={14} />
                        <input
                          min={1}
                          max={12}
                          type="number"
                          value={shot.durationSeconds}
                          onChange={(event) => updateShot(shot.shotId, 'durationSeconds', Number(event.target.value))}
                        />
                      </label>
                    </div>
                    <textarea
                      value={shot.scene}
                      onChange={(event) => updateShot(shot.shotId, 'scene', event.target.value)}
                    />
                    <textarea
                      value={shot.visualAction}
                      onChange={(event) => updateShot(shot.shotId, 'visualAction', event.target.value)}
                    />
                    <input
                      value={shot.cameraMovement}
                      onChange={(event) => updateShot(shot.shotId, 'cameraMovement', event.target.value)}
                    />
                    <div className="shot-regenerate-row">
                      <input
                        placeholder="单镜头重算备注"
                        value={shot.regenerateNotes}
                        onChange={(event) => updateShot(shot.shotId, 'regenerateNotes', event.target.value)}
                      />
                      <button className="secondary-button" disabled={busy} onClick={() => void regenerateShot(shot)} type="button">
                        <Sparkles size={15} />
                        <span>重算</span>
                      </button>
                    </div>
                  </article>
                ))}
              </div>
              <div className="modal-actions">
                <button className="primary-button" disabled={busy} onClick={() => void saveStoryboard()} type="button">
                  {busy ? <Loader2 className="spin" size={16} /> : <CheckCircle2 size={16} />}
                  <span>保存分镜</span>
                </button>
              </div>
            </section>
          )}

          {canEditStructuredOutput && structuredDraft && (
            <section className="structured-output-editor">
              <div className="structured-editor-heading">
                <div>
                  <h3>{stageLabel(task.skillName)}编辑</h3>
                  <p>保存后只改写当前 JSON envelope，并重置下游任务等待重算。</p>
                </div>
                <textarea
                  placeholder="本次修改备注"
                  value={notes}
                  onChange={(event) => setNotes(event.target.value)}
                />
              </div>

              {structuredDraft.skillName === 'character_bible' && (
                <div className="structured-editor-grid">
                  {structuredDraft.characters.map((character) => (
                    <article className="structured-editor-row" key={character.character_id}>
                      <div className="structured-row-title">
                        <strong>{character.name || character.character_id}</strong>
                        <span>{character.role_type || '角色'}</span>
                      </div>
                      <label>
                        角色定位
                        <textarea
                          value={character.identity}
                          onChange={(event) => updateCharacterDraft(character.character_id, 'identity', event.target.value)}
                        />
                      </label>
                      <label>
                        外观 JSON
                        <textarea
                          value={character.appearanceText}
                          onChange={(event) => updateCharacterDraft(character.character_id, 'appearanceText', event.target.value)}
                        />
                      </label>
                      <label>
                        服装 JSON
                        <textarea
                          value={character.costumeText}
                          onChange={(event) => updateCharacterDraft(character.character_id, 'costumeText', event.target.value)}
                        />
                      </label>
                      <label>
                        声音 JSON
                        <textarea
                          value={character.voiceProfileText}
                          onChange={(event) => updateCharacterDraft(character.character_id, 'voiceProfileText', event.target.value)}
                        />
                      </label>
                      <label>
                        连续性规则
                        <textarea
                          value={character.continuityRulesText}
                          onChange={(event) =>
                            updateCharacterDraft(character.character_id, 'continuityRulesText', event.target.value)
                          }
                        />
                      </label>
                    </article>
                  ))}
                  <label className="structured-wide-field">
                    关系备注
                    <textarea
                      value={structuredDraft.relationshipNotesText}
                      onChange={(event) =>
                        setStructuredDraft((current) =>
                          current?.skillName === 'character_bible'
                            ? { ...current, relationshipNotesText: event.target.value }
                            : current,
                        )
                      }
                    />
                  </label>
                  <label className="structured-wide-field">
                    全局连续性规则
                    <textarea
                      value={structuredDraft.continuityRulesText}
                      onChange={(event) =>
                        setStructuredDraft((current) =>
                          current?.skillName === 'character_bible'
                            ? { ...current, continuityRulesText: event.target.value }
                            : current,
                        )
                      }
                    />
                  </label>
                </div>
              )}

              {structuredDraft.skillName === 'style_bible' && (
                <div className="structured-editor-grid">
                  <label className="structured-wide-field">
                    画风名称
                    <input
                      value={structuredDraft.style.styleName}
                      onChange={(event) => updateStyleDraft('styleName', event.target.value)}
                    />
                  </label>
                  <label>
                    视觉风格 JSON
                    <textarea
                      value={structuredDraft.style.visualStyleText}
                      onChange={(event) => updateStyleDraft('visualStyleText', event.target.value)}
                    />
                  </label>
                  <label>
                    镜头语言 JSON
                    <textarea
                      value={structuredDraft.style.cameraLanguageText}
                      onChange={(event) => updateStyleDraft('cameraLanguageText', event.target.value)}
                    />
                  </label>
                  <div className="palette-editor structured-wide-field">
                    <span>色板</span>
                    {structuredDraft.style.colorPalette.map((color, index) => (
                      <div className="palette-editor-row" key={`${color.name}-${index}`}>
                        <input value={color.name} onChange={(event) => updateStylePalette(index, 'name', event.target.value)} />
                        <input value={color.hex} onChange={(event) => updateStylePalette(index, 'hex', event.target.value)} />
                        <input value={color.usage} onChange={(event) => updateStylePalette(index, 'usage', event.target.value)} />
                      </div>
                    ))}
                  </div>
                  <label>
                    负面提示词
                    <textarea
                      value={structuredDraft.style.negativePromptText}
                      onChange={(event) => updateStyleDraft('negativePromptText', event.target.value)}
                    />
                  </label>
                  <label>
                    可复用提示词块 JSON
                    <textarea
                      value={structuredDraft.style.reusablePromptBlocksText}
                      onChange={(event) => updateStyleDraft('reusablePromptBlocksText', event.target.value)}
                    />
                  </label>
                  <label>
                    图片提示词规则
                    <textarea
                      value={structuredDraft.style.imagePromptGuidelinesText}
                      onChange={(event) => updateStyleDraft('imagePromptGuidelinesText', event.target.value)}
                    />
                  </label>
                  <label>
                    视频提示词规则
                    <textarea
                      value={structuredDraft.style.videoPromptGuidelinesText}
                      onChange={(event) => updateStyleDraft('videoPromptGuidelinesText', event.target.value)}
                    />
                  </label>
                  <label className="structured-wide-field">
                    连续性备注
                    <textarea
                      value={structuredDraft.style.continuityNotesText}
                      onChange={(event) => updateStyleDraft('continuityNotesText', event.target.value)}
                    />
                  </label>
                </div>
              )}

              {(structuredDraft.skillName === 'image_prompt_builder' ||
                structuredDraft.skillName === 'video_prompt_builder') && (
                <div className="structured-editor-grid">
                  <label>
                    全局负面提示词
                    <textarea
                      value={structuredDraft.prompt.negativePromptText}
                      onChange={(event) => updatePromptNegativeText(event.target.value)}
                    />
                  </label>
                  <label>
                    {structuredDraft.skillName === 'image_prompt_builder' ? '参考策略 JSON' : '来源资产清单 JSON'}
                    <textarea
                      value={structuredDraft.prompt.supportText}
                      onChange={(event) => updatePromptSupportText(event.target.value)}
                    />
                  </label>
                  {structuredDraft.prompt.requests.map((request) => (
                    <article className="structured-editor-row structured-wide-field" key={request.request_id}>
                      <div className="structured-row-title">
                        <strong>{request.label}</strong>
                        <label className="structured-check-row">
                          <input
                            type="checkbox"
                            checked={request.selected}
                            onChange={(event) => updatePromptDraft(request.request_id, 'selected', event.target.checked)}
                          />
                          选中
                        </label>
                      </div>
                      <label>
                        提示词
                        <textarea
                          value={request.prompt}
                          onChange={(event) => updatePromptDraft(request.request_id, 'prompt', event.target.value)}
                        />
                      </label>
                      <label>
                        负面提示词
                        <textarea
                          value={request.negativePromptText}
                          onChange={(event) => updatePromptDraft(request.request_id, 'negativePromptText', event.target.value)}
                        />
                      </label>
                    </article>
                  ))}
                </div>
              )}

              <div className="structured-diff-panel">
                <h4>变更摘要</h4>
                {structuredDiff.length > 0 ? (
                  structuredDiff.map((item) => (
                    <div className="structured-diff-row" key={item.label}>
                      <strong>{item.label}</strong>
                      <span>{item.before}</span>
                      <ChevronRight size={14} />
                      <span>{item.after}</span>
                    </div>
                  ))
                ) : (
                  <p>暂无字段变化</p>
                )}
              </div>

              <div className="modal-actions">
                <button className="primary-button" disabled={busy} onClick={() => void saveStructuredOutput()} type="button">
                  {busy ? <Loader2 className="spin" size={16} /> : <CheckCircle2 size={16} />}
                  <span>保存结构化产物</span>
                </button>
              </div>
            </section>
          )}

          <section className="task-json-block">
            <h3>结构化输出</h3>
            <pre>{formatTaskJson(envelope, task.outputJson)}</pre>
          </section>

          {message && <p className={message.includes('失败') ? 'workspace-notice warn' : 'workspace-notice'}>{message}</p>}
        </div>
      </section>
    </div>
  );
}

function SettingsPanelOverlay({
  authState,
  panel,
  onClose,
  onSignOut,
}: {
  authState: AuthState;
  panel: Exclude<SettingsPanel, null>;
  onClose: () => void;
  onSignOut: () => Promise<void> | void;
}) {
  const titleMap: Record<Exclude<SettingsPanel, null>, string> = {
    account: '账户',
    dependencies: '依赖',
    diagnostics: '桌面诊断',
    providers: '模型配置',
  };

  return (
    <div className="workspace-modal-backdrop">
      <section className="settings-panel-sheet" aria-label={titleMap[panel]}>
        <header className="modal-heading">
          <div>
            <p className="eyebrow">设置</p>
            <h2>{titleMap[panel]}</h2>
          </div>
          <button className="icon-button" onClick={onClose} type="button" aria-label="关闭">
            <X size={18} />
          </button>
        </header>
        {panel === 'providers' && <ProviderSettingsPage />}
        {panel === 'dependencies' && <DependencySettingsPage />}
        {panel === 'diagnostics' && <DesktopDiagnosticsPanel />}
        {panel === 'account' && (
          <div className="account-settings-panel">
            <div className="account-settings-card">
              <span className="account-avatar large">{getAccountInitial(authState)}</span>
              <div>
                <h3>{authState.account?.displayName ?? 'MiLuStudio 用户'}</h3>
                <p>{authState.account?.email ?? authState.device?.deviceName ?? '本机账户'}</p>
              </div>
            </div>
            <dl className="account-meta-list">
              <div>
                <dt>设备</dt>
                <dd>{authState.device?.deviceName ?? 'Web'}</dd>
              </div>
              <div>
                <dt>授权状态</dt>
                <dd>{authState.license.message || '无需许可证'}</dd>
              </div>
              <div>
                <dt>账户状态</dt>
                <dd>{authState.account?.status ?? 'active'}</dd>
              </div>
            </dl>
            <button className="secondary-button danger" onClick={() => void onSignOut()} type="button">
              <LogOut size={16} />
              <span>退出登录</span>
            </button>
          </div>
        )}
      </section>
    </div>
  );
}

function buildUploadMenuOptions(job: ProductionJob | null, project: ProjectDetail | null): UploadMenuOption[] {
  const stageKey = uploadStageKey(job, project);
  const stageLabelText = uploadStageLabel(stageKey);
  const allowedMenuKinds = allowedUploadMenuKinds(stageKey);
  const globalDisabledReason = uploadGlobalDisabledReason(stageKey, job, project);
  const storySourceStage = isStorySourceUploadStage(stageKey);
  const textAllowedKinds: ComposerAttachmentKind[] = storySourceStage
    ? ['storyText']
    : ['reference', 'storyboardReference'];

  return [
    {
      kind: 'text',
      label: '文本',
      description: storySourceStage
        ? `上传剧本文本 · 最大 ${formatFileSize(TEXT_UPLOAD_MAX_BYTES)}`
        : `上传文字要求或分镜参考 · 最大 ${formatFileSize(TEXT_UPLOAD_MAX_BYTES)}`,
      accept: storySourceStage ? STORY_TEXT_ACCEPT : STORYBOARD_TEXT_ACCEPT,
      allowedKinds: textAllowedKinds,
      enabled: !globalDisabledReason && allowedMenuKinds.has('text'),
      disabledReason: globalDisabledReason || `${stageLabelText}暂不接收文本附件`,
    },
    {
      kind: 'image',
      label: '图片',
      description: `角色图或画风参考 · 最大 ${formatFileSize(IMAGE_UPLOAD_MAX_BYTES)}`,
      accept: IMAGE_REFERENCE_ACCEPT,
      allowedKinds: ['imageReference'],
      enabled: !globalDisabledReason && allowedMenuKinds.has('image'),
      disabledReason: globalDisabledReason || `${stageLabelText}暂不接收图片，图片参考请在角色、画风、图片资产、视频或质检阶段添加`,
    },
    {
      kind: 'video',
      label: '视频',
      description: `视频分镜综合参考 · 最大 ${formatFileSize(VIDEO_UPLOAD_MAX_BYTES)}`,
      accept: VIDEO_REFERENCE_ACCEPT,
      allowedKinds: ['videoReference'],
      enabled: !globalDisabledReason && allowedMenuKinds.has('video'),
      disabledReason: globalDisabledReason || `${stageLabelText}暂不接收视频，视频参考请在分镜、视频、粗剪或质检阶段添加`,
    },
  ];
}

function uploadStageKey(job: ProductionJob | null, project: ProjectDetail | null): string {
  if (!project) {
    return 'story';
  }

  if (!job) {
    return project.status === 'completed' ? 'completed' : 'story';
  }

  if (job.status === 'queued') {
    return 'queued';
  }

  if (job.status === 'completed') {
    return 'completed';
  }

  if (job.status === 'failed') {
    return 'failed';
  }

  return job.currentStage || 'story';
}

function uploadGlobalDisabledReason(stageKey: string, job: ProductionJob | null, project: ProjectDetail | null): string {
  if (job?.status === 'queued') {
    return '任务正在排队，进入具体阶段后再添加附件';
  }

  if (stageKey === 'export') {
    return '导出占位阶段不接收新附件';
  }

  if (stageKey === 'completed' || project?.status === 'completed') {
    return '项目已完成，重新开始新项目后再添加附件';
  }

  if (stageKey === 'failed' || stageKey.startsWith('failed')) {
    return '当前任务失败，请先处理失败状态后再添加附件';
  }

  return '';
}

function allowedUploadMenuKinds(stageKey: string): Set<UploadMenuKind> {
  if (['story', 'created', 'plot', 'script', 'image_prompt', 'voice', 'subtitle'].includes(stageKey)) {
    return new Set(['text']);
  }

  if (['character', 'style', 'image'].includes(stageKey)) {
    return new Set(['text', 'image']);
  }

  if (['storyboard', 'video_prompt', 'video', 'edit', 'quality'].includes(stageKey)) {
    return new Set(['text', 'image', 'video']);
  }

  return new Set();
}

function isStorySourceUploadStage(stageKey: string): boolean {
  return ['story', 'created', 'plot', 'script'].includes(stageKey);
}

function uploadStageLabel(stageKey: string): string {
  const labels: Record<string, string> = {
    character: '角色设定阶段',
    completed: '已完成项目',
    created: '新项目',
    edit: '粗剪计划阶段',
    export: '导出占位阶段',
    failed: '失败状态',
    image: '图片资产阶段',
    image_prompt: '图片提示词阶段',
    plot: '短剧改编阶段',
    quality: '质量检查阶段',
    script: '脚本生成阶段',
    story: '故事解析阶段',
    storyboard: '分镜审核阶段',
    style: '画风设定阶段',
    subtitle: '字幕结构阶段',
    queued: '排队中',
    video: '视频片段阶段',
    video_prompt: '视频提示词阶段',
    voice: '配音任务阶段',
  };

  return labels[stageKey] ?? '当前阶段';
}

function resolveSubmissionStorySource(
  draftText: string,
  attachments: ComposerAttachment[],
  currentProject: ProjectDetail | null,
): { text: string; source: 'attachment' | 'currentProject' | 'draft' | 'none' } {
  const textAttachments = attachments.filter((attachment) => attachment.kind === 'storyText' && attachment.text?.trim());

  if (textAttachments.length === 1) {
    return { text: textAttachments[0].text?.trim() ?? '', source: 'attachment' };
  }

  if (textAttachments.length > 1) {
    return {
      text: textAttachments
        .map((attachment) => `【${attachment.name}】\n${attachment.text?.trim() ?? ''}`)
        .join('\n\n')
        .trim(),
      source: 'attachment',
    };
  }

  if (currentProject?.storyText.trim()) {
    return { text: currentProject.storyText.trim(), source: 'currentProject' };
  }

  if (draftText.trim()) {
    return { text: draftText.trim(), source: 'draft' };
  }

  return { text: '', source: 'none' };
}

function buildSubmissionStoryText(
  draftText: string,
  attachments: ComposerAttachment[],
  currentProject: ProjectDetail | null,
): string {
  const source = resolveSubmissionStorySource(draftText, attachments, currentProject);
  if (!source.text) {
    return '';
  }

  const parts = [source.text];
  const requestText = source.source === 'draft' ? '' : draftText.trim();
  const referenceSummary = formatReferenceAttachmentSummary(attachments);

  if (requestText) {
    parts.push(`制作要求：\n${requestText}`);
  }

  if (referenceSummary) {
    parts.push(referenceSummary);
  }

  return parts.join('\n\n').trim();
}

function formatReferenceAttachmentSummary(attachments: ComposerAttachment[]): string {
  const references = attachments.filter((attachment) => attachment.kind !== 'storyText');
  if (!references.length) {
    return '';
  }

  return [
    '参考附件（已上传到项目资产并做技术解析）：',
    ...references.map(
      (attachment) =>
        `- ${attachment.name}｜${attachmentKindLabel(attachment.kind)}｜${attachment.extension.toUpperCase() || '文件'}｜${formatFileSize(attachment.size)}`,
    ),
  ].join('\n');
}

function classifyComposerFile(file: File, option: UploadMenuOption): ComposerAttachmentKind | null {
  const extension = fileExtension(file.name);

  if (TEXT_ATTACHMENT_EXTENSIONS.has(extension)) {
    return option.allowedKinds.includes('storyText') ? 'storyText' : 'reference';
  }

  if (IMAGE_REFERENCE_EXTENSIONS.has(extension) || file.type.startsWith('image/')) {
    return 'imageReference';
  }

  if (VIDEO_REFERENCE_EXTENSIONS.has(extension) || file.type.startsWith('video/')) {
    return 'videoReference';
  }

  if (STORYBOARD_REFERENCE_EXTENSIONS.has(extension)) {
    return 'storyboardReference';
  }

  return null;
}

function createComposerAttachment(file: File, kind: ComposerAttachmentKind, text?: string): ComposerAttachment {
  return {
    id: createAttachmentId(),
    name: file.name,
    extension: fileExtension(file.name),
    mimeType: file.type || 'application/octet-stream',
    size: file.size,
    kind,
    file,
    text,
    uploadStatus: 'pending',
  };
}

function applyUploadResponse(attachment: ComposerAttachment, response: ProjectAssetUploadResponse): ComposerAttachment {
  return {
    ...attachment,
    assetId: response.id,
    text: response.extractedText?.trim() || attachment.text,
    uploadStatus: 'uploaded',
    message: response.message,
  };
}

function uploadLimitForKind(kind: ComposerAttachmentKind): number {
  if (kind === 'imageReference') {
    return IMAGE_UPLOAD_MAX_BYTES;
  }

  if (kind === 'videoReference') {
    return VIDEO_UPLOAD_MAX_BYTES;
  }

  return TEXT_UPLOAD_MAX_BYTES;
}

function createAttachmentId(): string {
  if (window.crypto?.randomUUID) {
    return window.crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function fileExtension(fileName: string): string {
  const extension = fileName.split('.').pop();
  return extension ? extension.toLowerCase() : '';
}

function attachmentKindLabel(kind: ComposerAttachmentKind): string {
  const labels: Record<ComposerAttachmentKind, string> = {
    imageReference: '角色图参考',
    reference: '参考文件',
    storyboardReference: '分镜参考',
    storyText: '剧本文本',
    videoReference: '视频参考',
  };

  return labels[kind];
}

function formatUploadStatus(attachment: ComposerAttachment): string {
  if (attachment.uploadStatus === 'uploaded') {
    return '已上传解析';
  }

  if (attachment.uploadStatus === 'failed') {
    return '上传失败';
  }

  return '待上传解析';
}

function attachmentIcon(kind: ComposerAttachmentKind) {
  if (kind === 'storyText') {
    return <FileText size={16} />;
  }

  if (kind === 'imageReference') {
    return <ImageIcon size={16} />;
  }

  if (kind === 'videoReference') {
    return <Film size={16} />;
  }

  return <FileIcon size={16} />;
}

function uploadMenuIcon(kind: UploadMenuKind) {
  if (kind === 'text') {
    return <FileText size={16} />;
  }

  if (kind === 'image') {
    return <ImageIcon size={16} />;
  }

  return <Film size={16} />;
}

function buildProjectPayload(text: string, currentProject: ProjectDetail | null): ProjectUpdateRequest {
  return {
    title: currentProject?.title?.trim() || inferProjectTitle(text),
    storyText: text,
    mode: 'director',
    targetDuration: currentProject?.targetDuration ?? 45,
    aspectRatio: currentProject?.aspectRatio ?? '9:16',
    stylePreset: currentProject?.stylePreset?.trim() || '轻写实国漫',
  };
}

function inferProjectTitle(text: string): string {
  const firstLine = text
    .split(/\r?\n/)
    .map((line) => line.trim())
    .find(Boolean);

  if (!firstLine) {
    return `新漫剧 ${new Date().toLocaleDateString('zh-CN')}`;
  }

  return firstLine.length > 24 ? `${firstLine.slice(0, 24)}...` : firstLine;
}

function buildProductionFlow(job: ProductionJob | null, tasks: GenerationTaskRecord[]): ProductionFlowItem[] {
  const tasksBySkill = new Map(tasks.map((task) => [task.skillName, task]));

  return FIXED_PRODUCTION_FLOW.map((item) => {
    const task = tasksBySkill.get(item.skillName);
    const stage = job?.stages.find((entry) => entry.skill === item.skillName || entry.id === item.skillName);
    const doneByStage = stage?.status === 'done';
    const doneByTask = Boolean(task?.outputJson && task.status === 'completed');
    const reviewByStage = Boolean(stage?.needsReview && stage.status === 'review');
    const reviewByTask = Boolean(task?.outputJson && task.status === 'review');
    const activeByStage = Boolean(
      stage && (job?.currentStage === stage.id || stage.status === 'running'),
    );

    return {
      ...item,
      needsReview: Boolean(stage?.needsReview),
      state: doneByTask || doneByStage ? 'done' : reviewByStage || reviewByTask ? 'review' : activeByStage ? 'active' : 'pending',
    };
  });
}

function findLatestConfirmedReviewSkill(job: ProductionJob | null, tasks: GenerationTaskRecord[]): string | null {
  if (!job) {
    return null;
  }

  const reviewSkills = new Set(job.stages.filter((stage) => stage.needsReview).map((stage) => stage.skill));
  return (
    tasks
      .filter((task) => task.status === 'completed' && reviewSkills.has(task.skillName))
      .sort((left, right) => right.queueIndex - left.queueIndex)[0]?.skillName ?? null
  );
}

function findActiveReview(job: ProductionJob | null, tasks: GenerationTaskRecord[]): ActiveReview | null {
  if (!job) {
    return null;
  }

  const reviewTask = tasks.find((entry) => entry.status === 'review' && entry.outputJson);
  const reviewStage =
    job.stages.find((stage) => stage.needsReview && stage.status === 'review') ??
    (reviewTask ? job.stages.find((stage) => stage.needsReview && stage.skill === reviewTask.skillName) : undefined);

  if (!reviewStage) {
    return null;
  }

  const task =
    (reviewTask?.skillName === reviewStage.skill ? reviewTask : null) ??
    tasks.find((entry) => entry.skillName === reviewStage.skill && entry.outputJson) ??
    null;

  return {
    label: stageLabel(reviewStage.skill),
    stage: reviewStage,
    task,
  };
}

function applyEventToStages(stages: ProductionStage[], event: ProductionJobEvent): ProductionStage[] {
  const activeIndex = stages.findIndex((stage) => stage.id === event.stageId || stage.skill === event.skill);

  if (activeIndex < 0) {
    return stages;
  }

  return stages.map((stage, index) => {
    if (index < activeIndex) {
      return { ...stage, status: 'done' };
    }

    if (index === activeIndex) {
      return { ...stage, status: event.status };
    }

    return { ...stage, status: 'waiting' };
  });
}

function parseSkillEnvelope(outputJson: string | null): SkillEnvelope | null {
  if (!outputJson) {
    return null;
  }

  try {
    return JSON.parse(outputJson) as SkillEnvelope;
  } catch {
    return null;
  }
}

function summarizeTask(task: GenerationTaskRecord, envelope: SkillEnvelope | null): string {
  if (!envelope) {
    return task.errorMessage ?? '结构化结果暂不可解析。';
  }

  if (envelope.ok === false) {
    return envelope.error?.message ?? task.errorMessage ?? '该步骤执行失败。';
  }

  const data = envelope.data ?? {};

  if (task.skillName === 'storyboard_director') {
    const shots = asArray(data.shots);
    const parts = asArray(data.storyboard_parts);
    return `已生成 ${shots.length} 个镜头，${parts.length} 个段落。`;
  }

  if (task.skillName === 'style_bible') {
    const palette = asArray(data.color_palette);
    return `${asString(data.style_name, '画风设定')}，包含 ${palette.length} 个色板项。`;
  }

  if (task.skillName === 'character_bible') {
    const characters = asArray(data.characters);
    return `已生成 ${characters.length} 个角色设定。`;
  }

  if (task.skillName === 'export_packager') {
    const deliveryAssets = asArray(data.delivery_assets);
    return `已生成 ${deliveryAssets.length} 个导出占位结构。`;
  }

  return asString(data.summary, `${stageLabel(task.skillName)}结果已写入数据库。`);
}

function buildTaskPreview(task: GenerationTaskRecord, envelope: SkillEnvelope | null): string {
  if (!envelope?.data) {
    return task.errorMessage ?? '暂无预览。';
  }

  const data = envelope.data;

  if (task.skillName === 'storyboard_director') {
    return asArray(data.shots)
      .slice(0, 3)
      .map((entry, index) => {
        const shot = asRecord(entry);
        return `${asString(shot.shot_label, `镜头 ${index + 1}`)}：${asString(shot.scene)} ${asString(shot.visual_action)}`.trim();
      })
      .filter(Boolean)
      .join(' / ');
  }

  if (task.skillName === 'style_bible') {
    return asArray(data.color_palette)
      .slice(0, 4)
      .map((entry) => {
        const color = asRecord(entry);
        return `${asString(color.name, '颜色')} ${asString(color.hex)}`.trim();
      })
      .join(' / ');
  }

  const fields = Object.entries(data)
    .slice(0, 4)
    .map(([key, value]) => `${formatFieldName(key)}：${previewUnknown(value, 42)}`)
    .filter(Boolean);

  return fields.join(' / ') || '暂无预览。';
}

function extractStoryboardShots(envelope: SkillEnvelope | null): StoryboardShotDraft[] {
  if (!envelope?.data) {
    return [];
  }

  return asArray(envelope.data.shots).map((entry, index) => {
    const shot = asRecord(entry);
    const camera = asRecord(shot.camera);

    return {
      shotId: asString(shot.shot_id, `shot-${index + 1}`),
      label: asString(shot.shot_label, `镜头 ${index + 1}`),
      startSecond: Number(shot.start_second ?? 0),
      durationSeconds: Number(shot.duration_seconds ?? 1),
      scene: asString(shot.scene),
      visualAction: asString(shot.visual_action),
      shotSize: asString(shot.shot_size, '中景'),
      cameraMovement: asString(camera.motion, '固定镜头'),
      soundNote: asString(shot.sound_note),
      dialogue: formatDialogue(shot.dialogue),
      narration: asString(shot.narration),
      regenerateNotes: '',
    };
  });
}

function createStructuredEditorDraft(skillName: string, envelope: SkillEnvelope | null): StructuredEditorDraft | null {
  if (!envelope?.data || !isEditableStructuredSkill(skillName)) {
    return null;
  }

  const data = envelope.data;

  if (skillName === 'character_bible') {
    return {
      skillName,
      characters: asArray(data.characters).map((entry, index) => {
        const character = asRecord(entry);
        return {
          character_id: asString(character.character_id, `char_${index + 1}`),
          name: asString(character.name, `角色 ${index + 1}`),
          role_type: asString(character.role_type, 'supporting'),
          identity: asString(character.identity),
          appearanceText: formatEditableJson(character.appearance ?? {}),
          costumeText: formatEditableJson(character.costume ?? {}),
          voiceProfileText: formatEditableJson(character.voice_profile ?? {}),
          continuityRulesText: arrayToLines(character.continuity_rules),
          source: character,
        };
      }),
      relationshipNotesText: arrayToLines(data.relationship_notes),
      continuityRulesText: arrayToLines(data.continuity_rules),
    };
  }

  if (skillName === 'style_bible') {
    return {
      skillName,
      style: {
        styleName: asString(data.style_name),
        visualStyleText: formatEditableJson(data.visual_style ?? {}),
        colorPalette: asArray(data.color_palette).map((entry) => {
          const color = asRecord(entry);
          return {
            name: asString(color.name),
            hex: asString(color.hex),
            usage: asString(color.usage),
          };
        }),
        cameraLanguageText: formatEditableJson(data.camera_language ?? {}),
        negativePromptText: arrayToLines(data.negative_prompt),
        reusablePromptBlocksText: formatEditableJson(data.reusable_prompt_blocks ?? {}),
        imagePromptGuidelinesText: arrayToLines(data.image_prompt_guidelines),
        videoPromptGuidelinesText: arrayToLines(data.video_prompt_guidelines),
        continuityNotesText: arrayToLines(data.continuity_notes),
      },
    };
  }

  if (skillName === 'image_prompt_builder') {
    return {
      skillName,
      prompt: {
        requests: buildPromptRequestDrafts(data.image_requests, 'image'),
        negativePromptText: arrayToLines(data.negative_prompt),
        supportText: formatEditableJson(data.reference_strategy ?? {}),
      },
    };
  }

  return {
    skillName,
    prompt: {
      requests: buildPromptRequestDrafts(data.video_requests, 'video'),
      negativePromptText: arrayToLines(data.negative_prompt),
      supportText: formatEditableJson(data.source_asset_manifest ?? {}),
    },
  };
}

function buildPromptRequestDrafts(value: unknown, kind: 'image' | 'video'): PromptRequestDraft[] {
  return asArray(value).map((entry, index) => {
    const request = asRecord(entry);
    const outputSlot = asRecord(request.output_slot);
    return {
      request_id: asString(request.request_id, `${kind}_request_${index + 1}`),
      label:
        kind === 'image'
          ? `${asString(request.asset_type, 'image')} ${asString(request.shot_id, asString(request.character_id))}`.trim()
          : `镜头 ${String(request.shot_index ?? index + 1)} ${asString(request.shot_id)}`.trim(),
      prompt: asString(request.prompt),
      negativePromptText: arrayToLines(request.negative_prompt),
      selected: Boolean(outputSlot.selected),
      source: request,
    };
  });
}

function buildStructuredOutputEdits(draft: StructuredEditorDraft): Array<{ path: string; value: unknown }> {
  return Object.entries(structuredDraftToFieldMap(draft)).map(([path, value]) => ({ path, value }));
}

function structuredDraftToFieldMap(draft: StructuredEditorDraft): Record<string, unknown> {
  if (draft.skillName === 'character_bible') {
    return {
      characters: draft.characters.map((character) => ({
        ...character.source,
        character_id: character.character_id,
        name: character.name,
        role_type: character.role_type,
        identity: character.identity.trim(),
        appearance: parseEditableJsonObject(character.appearanceText, `${character.name} 外观 JSON`),
        costume: parseEditableJsonObject(character.costumeText, `${character.name} 服装 JSON`),
        voice_profile: parseEditableJsonObject(character.voiceProfileText, `${character.name} 声音 JSON`),
        continuity_rules: linesToArray(character.continuityRulesText),
      })),
      relationship_notes: linesToArray(draft.relationshipNotesText),
      continuity_rules: linesToArray(draft.continuityRulesText),
    };
  }

  if (draft.skillName === 'style_bible') {
    return {
      style_name: draft.style.styleName.trim(),
      visual_style: parseEditableJsonObject(draft.style.visualStyleText, '视觉风格 JSON'),
      color_palette: draft.style.colorPalette.map((color) => ({
        name: color.name.trim(),
        hex: color.hex.trim(),
        usage: color.usage.trim(),
      })),
      camera_language: parseEditableJsonObject(draft.style.cameraLanguageText, '镜头语言 JSON'),
      negative_prompt: linesToArray(draft.style.negativePromptText),
      reusable_prompt_blocks: parseEditableJsonObject(draft.style.reusablePromptBlocksText, '可复用提示词块 JSON'),
      image_prompt_guidelines: linesToArray(draft.style.imagePromptGuidelinesText),
      video_prompt_guidelines: linesToArray(draft.style.videoPromptGuidelinesText),
      continuity_notes: linesToArray(draft.style.continuityNotesText),
    };
  }

  if (draft.skillName === 'image_prompt_builder') {
    return {
      image_requests: draft.prompt.requests.map(toPromptRequestValue),
      negative_prompt: linesToArray(draft.prompt.negativePromptText),
      reference_strategy: parseEditableJsonObject(draft.prompt.supportText, '参考策略 JSON'),
    };
  }

  return {
    video_requests: draft.prompt.requests.map(toPromptRequestValue),
    negative_prompt: linesToArray(draft.prompt.negativePromptText),
    source_asset_manifest: parseEditableJsonObject(draft.prompt.supportText, '来源资产清单 JSON'),
  };
}

function toPromptRequestValue(request: PromptRequestDraft): JsonObjectValue {
  const outputSlot = {
    ...asRecord(request.source.output_slot),
    selected: request.selected,
  };

  return {
    ...request.source,
    prompt: request.prompt.trim(),
    negative_prompt: linesToArray(request.negativePromptText),
    output_slot: outputSlot,
  };
}

function buildStructuredDiff(
  skillName: string,
  envelope: SkillEnvelope | null,
  draft: StructuredEditorDraft | null,
): StructuredDiffItem[] {
  if (!envelope?.data || !draft) {
    return [];
  }

  try {
    return Object.entries(structuredDraftToFieldMap(draft))
      .map(([path, value]) => {
        const before = envelope.data?.[path];
        return jsonStableStringify(before) === jsonStableStringify(value)
          ? null
          : {
              label: structuredFieldLabel(skillName, path),
              before: previewUnknown(before, 44),
              after: previewUnknown(value, 44),
            };
      })
      .filter((item): item is StructuredDiffItem => Boolean(item));
  } catch (error) {
    return [
      {
        label: 'JSON 校验',
        before: '可保存',
        after: error instanceof Error ? error.message : '存在无法解析的 JSON 字段',
      },
    ];
  }
}

function structuredFieldLabel(skillName: string, path: string): string {
  const labels: Record<string, Record<string, string>> = {
    character_bible: {
      characters: '角色卡',
      relationship_notes: '关系备注',
      continuity_rules: '全局连续性',
    },
    style_bible: {
      style_name: '画风名称',
      visual_style: '视觉风格',
      color_palette: '色板',
      camera_language: '镜头语言',
      negative_prompt: '负面提示词',
      reusable_prompt_blocks: '提示词块',
      image_prompt_guidelines: '图片规则',
      video_prompt_guidelines: '视频规则',
      continuity_notes: '连续性备注',
    },
    image_prompt_builder: {
      image_requests: '图片提示词',
      negative_prompt: '全局负面提示词',
      reference_strategy: '参考策略',
    },
    video_prompt_builder: {
      video_requests: '视频提示词',
      negative_prompt: '全局负面提示词',
      source_asset_manifest: '来源资产清单',
    },
  };

  return labels[skillName]?.[path] ?? path;
}

function isEditableStructuredSkill(skillName: string): skillName is EditableStructuredSkill {
  return (
    skillName === 'character_bible' ||
    skillName === 'style_bible' ||
    skillName === 'image_prompt_builder' ||
    skillName === 'video_prompt_builder'
  );
}

function formatEditableJson(value: unknown): string {
  return JSON.stringify(value ?? {}, null, 2);
}

function parseEditableJsonObject(value: string, label: string): JsonObjectValue {
  try {
    const parsed = JSON.parse(value || '{}') as unknown;
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
      throw new Error(`${label} 必须是 JSON object。`);
    }

    return parsed as JsonObjectValue;
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error(`${label} 不是合法 JSON。`);
    }

    throw error;
  }
}

function arrayToLines(value: unknown): string {
  return asArray(value)
    .map((item) => (typeof item === 'string' ? item : JSON.stringify(item)))
    .filter(Boolean)
    .join('\n');
}

function linesToArray(value: string): string[] {
  return value
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
}

function jsonStableStringify(value: unknown): string {
  return JSON.stringify(value ?? null);
}

function toStoryboardShotEdit(shot: StoryboardShotDraft): StoryboardShotEdit {
  return {
    shotId: shot.shotId,
    durationSeconds: shot.durationSeconds,
    scene: shot.scene,
    visualAction: shot.visualAction,
    shotSize: shot.shotSize,
    cameraMovement: shot.cameraMovement,
    soundNote: shot.soundNote,
    dialogue: shot.dialogue,
    narration: shot.narration,
  };
}

function formatDialogue(value: unknown): string {
  if (Array.isArray(value)) {
    return value
      .map((entry) => {
        const line = asRecord(entry);
        const speaker = asString(line.speaker);
        const text = asString(line.line);
        return speaker && text ? `${speaker}: ${text}` : text;
      })
      .filter(Boolean)
      .join('\n');
  }

  return asString(value);
}

function formatTaskJson(envelope: SkillEnvelope | null, outputJson: string | null): string {
  if (envelope?.data) {
    return JSON.stringify(envelope.data, null, 2).slice(0, 12000);
  }

  return outputJson ?? '暂无结构化输出。';
}

function stageLabel(skillName: string): string {
  return stageLabelMap[skillName] ?? skillName.replaceAll('_', ' ');
}

function formatProjectStatus(status: ProjectStatus): string {
  const labels: Record<ProjectStatus, string> = {
    completed: '已完成',
    draft: '草稿',
    failed: '失败',
    paused: '已暂停',
    running: '进行中',
  };

  return labels[status];
}

function formatTaskStatus(status: GenerationTaskRecordStatus): string {
  const labels: Record<GenerationTaskRecordStatus, string> = {
    completed: '已完成',
    failed: '失败',
    review: '待审核',
    running: '进行中',
    waiting: '等待中',
  };

  return labels[status];
}

function formatStageStatus(status: StageStatus): string {
  const labels: Record<StageStatus, string> = {
    blocked: '已阻塞',
    done: '已完成',
    failed: '失败',
    review: '待审核',
    running: '进行中',
    waiting: '等待中',
  };

  return labels[status];
}

function getAccountInitial(authState: AuthState): string {
  return (authState.account?.displayName ?? authState.account?.email ?? 'M').slice(0, 1).toUpperCase();
}

function formatTime(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '--';
  }

  return new Intl.DateTimeFormat('zh-CN', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
  }).format(date);
}

function formatRelativeProjectTime(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '--';
  }

  const diffMs = Date.now() - date.getTime();
  const minute = 60 * 1000;
  const hour = 60 * minute;
  const day = 24 * hour;

  if (diffMs < minute) {
    return '刚刚';
  }

  if (diffMs < hour) {
    return `${Math.max(1, Math.floor(diffMs / minute))} 分钟`;
  }

  if (diffMs < day) {
    return `${Math.floor(diffMs / hour)} 小时`;
  }

  if (diffMs < 7 * day) {
    return `${Math.floor(diffMs / day)} 天`;
  }

  return formatTime(value);
}

function resultFileName(task: GenerationTaskRecord): string {
  const extension = task.skillName === 'storyboard_director' ? 'md' : 'json';
  return `${stageLabel(task.skillName)}.${extension}`;
}

function formatFileSize(size: number): string {
  if (size < 1024) {
    return `${size} B`;
  }

  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }

  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}

function previewText(value: string, maxLength: number): string {
  const text = value.replace(/\s+/g, ' ').trim();
  return text.length > maxLength ? `${text.slice(0, maxLength - 1)}...` : text;
}

function previewUnknown(value: unknown, maxLength: number): string {
  if (Array.isArray(value)) {
    return `${value.length} 项`;
  }

  if (value && typeof value === 'object') {
    return `${Object.keys(value).length} 项`;
  }

  return previewText(String(value ?? ''), maxLength);
}

function formatFieldName(value: string): string {
  return value
    .split('_')
    .filter(Boolean)
    .map((part) => `${part.slice(0, 1).toUpperCase()}${part.slice(1)}`)
    .join(' ');
}

function clampProgress(value: number): number {
  return Math.max(0, Math.min(100, Math.round(value)));
}

function asArray(value: unknown): unknown[] {
  return Array.isArray(value) ? value : [];
}

function asRecord(value: unknown): Record<string, unknown> {
  return value && typeof value === 'object' && !Array.isArray(value) ? (value as Record<string, unknown>) : {};
}

function asString(value: unknown, fallback = ''): string {
  return typeof value === 'string' && value.trim() ? value.trim() : fallback;
}

function getSavedJobId(projectId: string): string | null {
  return window.localStorage.getItem(`${SAVED_JOB_PREFIX}${projectId}`);
}

function saveJobId(projectId: string, jobId: string) {
  window.localStorage.setItem(`${SAVED_JOB_PREFIX}${projectId}`, jobId);
}

function removeSavedJobId(projectId: string) {
  window.localStorage.removeItem(`${SAVED_JOB_PREFIX}${projectId}`);
}

const stageLabelMap: Record<string, string> = {
  auto_editor: '粗剪计划',
  character_bible: '角色设定',
  episode_writer: '脚本审核',
  export_packager: '导出占位',
  image_generation: '图片资产审核',
  image_prompt_builder: '图片提示词',
  plot_adaptation: '短剧改编',
  quality_checker: '质量检查',
  storyboard_director: '分镜审核',
  story_intake: '故事解析',
  style_bible: '画风设定',
  subtitle_generator: '字幕结构',
  video_generation: '视频片段审核',
  video_prompt_builder: '视频提示词',
  voice_casting: '配音任务',
};

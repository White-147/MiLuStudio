import {
  ArrowLeft,
  CheckCircle2,
  Download,
  FileText,
  Film,
  Image,
  Pause,
  PencilLine,
  Play,
  RotateCcw,
  Save,
  Send,
  SlidersHorizontal,
  Video,
  XCircle,
} from 'lucide-react';
import type { Dispatch, SetStateAction } from 'react';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  approveProductionCheckpoint,
  getProductionJob,
  getProject,
  listProductionTasks,
  listProjectAssets,
  listProjectCosts,
  pauseProductionJob,
  rejectProductionCheckpoint,
  regenerateStoryboardShot,
  resumeProductionJob,
  retryProductionJob,
  startProductionJob,
  updateStoryboardTask,
  updateProject,
  watchProductionJob,
} from '../../shared/api/controlPlaneClient';
import { mockDeliveryAssets, mockProjects, mockResultCards, mockStages } from '../../shared/mock/studioMock';
import type {
  CostLedgerRecord,
  GenerationTaskRecord,
  GenerationTaskRecordStatus,
  ProjectAssetRecord,
  ProjectDetail,
  ProjectUpdateRequest,
  ProductionJob,
  ProductionJobEvent,
  ProductionStage,
  ResultCard,
  StoryboardEditRequest,
  StoryboardShotEdit,
} from '../../shared/types/production';

interface ProductionConsolePageProps {
  onBack: () => void;
  projectId: string;
}

type ApiState = 'loading' | 'control-api' | 'mock-fallback';

type JobCommand = 'pause' | 'resume' | 'retry' | 'checkpoint-approve' | 'checkpoint-reject';

type ProjectDraft = ProjectUpdateRequest;
type StageDisplayInfo = {
  title: string;
  shortTitle: string;
  output: string;
  check: string[];
  approveNext: string;
  rejectNext: string;
};
type DisplayResultCard = ResultCard & {
  envelope?: SkillEnvelope | null;
  isCurrent?: boolean;
  previewSections?: SkillPreviewSection[];
  queueIndex?: number;
  skillName?: string;
  taskStatus?: GenerationTaskRecordStatus;
  technicalDetails?: string[];
};
type SkillPreviewItem = {
  color?: string;
  label: string;
  value: string;
  variant?: 'block' | 'code' | 'text';
};
type SkillPreviewSection = {
  items: SkillPreviewItem[];
  title: string;
};
type StoryboardSaveHandler = (taskId: string, request: StoryboardEditRequest) => Promise<string>;
type StoryboardRegenerateHandler = (taskId: string, shotId: string, notes: string) => Promise<string>;
type EditableStoryboardShot = StoryboardShotEdit & {
  label: string;
  startSecond: number;
};

const stageDisplayMap: Record<string, StageDisplayInfo> = {
  story_intake: {
    title: '故事解析',
    shortTitle: '解析',
    output: '提取故事类型、核心冲突、人物关系和一句话梗概。',
    check: ['确认故事核心没有跑偏。', '确认标题、模式和目标时长与输入一致。'],
    approveNext: '进入短剧改编。',
    rejectNext: '回到故事入口，补充或修改原始文本后重新生成。',
  },
  plot_adaptation: {
    title: '短剧改编',
    shortTitle: '改编',
    output: '把原始故事改写成短剧结构，补齐风险点、节奏和看点。',
    check: ['确认故事适合当前模式。', '确认看点、风险点和梗概符合预期。'],
    approveNext: '进入脚本生成。',
    rejectNext: '退回改编步骤，按备注重新调整结构。',
  },
  episode_writer: {
    title: '脚本审核',
    shortTitle: '脚本',
    output: '生成可继续制作的分镜前脚本、对白或字幕条目。',
    check: ['确认剧情顺序清楚。', '确认语气、节奏和字幕数量适合目标时长。'],
    approveNext: '锁定脚本并进入角色设定。',
    rejectNext: '退回脚本步骤，按备注重新生成。',
  },
  character_bible: {
    title: '角色设定',
    shortTitle: '角色',
    output: '整理角色外观、性格、关系和后续生成需要遵守的设定。',
    check: ['确认角色数量和关系正确。', '确认角色描述适合后续画面生成。'],
    approveNext: '锁定角色设定并进入画风设定。',
    rejectNext: '退回角色设定步骤，按备注重新整理。',
  },
  style_bible: {
    title: '画风设定',
    shortTitle: '画风',
    output: '确定画面风格、色彩、镜头质感和统一的视觉规则。',
    check: ['确认画风与用户选择一致。', '确认色彩、镜头和禁忌项没有矛盾。'],
    approveNext: '锁定画风，继续生成分镜。',
    rejectNext: '退回画风设定，按备注重新生成规则。',
  },
  storyboard_director: {
    title: '分镜审核',
    shortTitle: '分镜',
    output: '把脚本拆成镜头列表，包含镜头动作、画面说明和节奏。',
    check: ['确认镜头顺序和剧情一致。', '确认每个镜头都能被后续图片或视频步骤消费。'],
    approveNext: '进入图片提示词生成。',
    rejectNext: '退回分镜步骤，按备注重新拆分镜头。',
  },
  image_prompt_builder: {
    title: '图片提示词',
    shortTitle: '图像词',
    output: '把分镜转换成结构化图片提示词。',
    check: ['确认提示词覆盖所有关键镜头。', '确认没有直接读取或生成真实媒体文件。'],
    approveNext: '进入模拟图片资产步骤。',
    rejectNext: '退回图片提示词步骤。',
  },
  image_generation: {
    title: '图片资产审核',
    shortTitle: '图片',
    output: '生成模拟图片资产记录，供后续流程验证使用。',
    check: ['确认占位资产数量合理。', '确认仍然只是模拟资产，不是真实图片文件。'],
    approveNext: '进入视频提示词生成。',
    rejectNext: '退回图片资产步骤。',
  },
  video_prompt_builder: {
    title: '视频提示词',
    shortTitle: '视频词',
    output: '把图片资产和分镜转换成视频生成提示词结构。',
    check: ['确认镜头动作和时长合理。', '确认提示词结构可被 Worker 消费。'],
    approveNext: '进入模拟视频片段步骤。',
    rejectNext: '退回视频提示词步骤。',
  },
  video_generation: {
    title: '视频片段审核',
    shortTitle: '视频',
    output: '生成模拟视频片段记录，不触发真实视频生成。',
    check: ['确认片段数量和分镜匹配。', '确认没有触发 FFmpeg 或真实 MP4 输出。'],
    approveNext: '进入配音任务规划。',
    rejectNext: '退回视频片段步骤。',
  },
  voice_casting: {
    title: '配音任务',
    shortTitle: '配音',
    output: '规划角色配音、旁白和音色占位信息。',
    check: ['确认角色与声音安排匹配。', '确认没有生成真实 WAV 文件。'],
    approveNext: '进入字幕结构生成。',
    rejectNext: '退回配音任务步骤。',
  },
  subtitle_generator: {
    title: '字幕结构',
    shortTitle: '字幕',
    output: '生成可导出的字幕条目结构。',
    check: ['确认字幕数量和时间结构合理。', '确认文本没有明显错漏。'],
    approveNext: '进入粗剪计划。',
    rejectNext: '退回字幕结构步骤。',
  },
  auto_editor: {
    title: '粗剪计划',
    shortTitle: '粗剪',
    output: '生成可交给后续剪辑器消费的时间线计划。',
    check: ['确认镜头、字幕、配音的顺序合理。', '确认没有生成真实视频文件。'],
    approveNext: '进入质量检查。',
    rejectNext: '退回粗剪计划步骤。',
  },
  quality_checker: {
    title: '质量检查',
    shortTitle: '质检',
    output: '检查结构完整性、风险和可交付状态。',
    check: ['确认没有阻断性问题。', '确认需要人工注意的风险已经清楚列出。'],
    approveNext: '进入导出占位包。',
    rejectNext: '退回质量检查或前序步骤。',
  },
  export_packager: {
    title: '导出占位包',
    shortTitle: '导出',
    output: '生成交付包的占位元数据，不生成真实 ZIP。',
    check: ['确认交付清单完整。', '确认仍然没有生成真实媒体包。'],
    approveNext: '任务完成。',
    rejectNext: '退回导出占位包步骤。',
  },
};

const englishStageLabelMap: Record<string, string> = {
  'Story intake': '故事解析',
  'Plot adaptation': '短剧改编',
  'Script review': '脚本审核',
  'Character bible': '角色设定',
  'Style bible': '画风设定',
  Storyboard: '分镜审核',
  'Image prompts': '图片提示词',
  'Mock images': '图片资产审核',
  'Video prompts': '视频提示词',
  'Mock videos': '视频片段审核',
  'Voice casting': '配音任务',
  'Subtitle structure': '字幕结构',
  'Rough edit plan': '粗剪计划',
  'Quality report': '质量检查',
  'Export package': '导出占位包',
};

const fieldNameMap: Record<string, string> = {
  assets: '资产',
  camera: '镜头',
  characters: '角色',
  cues: '字幕条目',
  error: '错误',
  genres: '类型',
  language: '语言',
  logline: '一句话梗概',
  mode: '模式',
  package: '交付包',
  palette: '色彩',
  prompts: '提示词',
  camera_language: '镜头语言',
  character_rendering_rules: '角色一致性',
  checkpoint: '审核节点',
  color_palette: '色板',
  continuity_notes: '连续性备注',
  delivery_assets: '交付资产',
  environment_design: '场景设计',
  image_prompt_guidelines: '图片提示词规则',
  issues: '检查项',
  lighting: '灯光',
  negative_prompt: '负面提示词',
  quality_status: '质量状态',
  reusable_prompt_blocks: '复用提示词块',
  subtitle_cues: '字幕条目',
  video_prompt_guidelines: '视频提示词规则',
  visual_style: '视觉规则',
  review: '审核建议',
  risks: '风险点',
  scenes: '场景',
  storyboard: '分镜',
  style: '风格',
  summary: '摘要',
  timeline: '时间线',
  title: '标题',
};

const productionStagePreview: ProductionStage[] = [
  { id: 'story', label: '故事解析', skill: 'story_intake', status: 'waiting', duration: '00:18', cost: 'local', needsReview: false },
  { id: 'plot', label: '短剧改编', skill: 'plot_adaptation', status: 'waiting', duration: '00:24', cost: 'local', needsReview: false },
  { id: 'script', label: '脚本审核', skill: 'episode_writer', status: 'waiting', duration: '01:12', cost: 'local', needsReview: true },
  { id: 'character', label: '角色设定', skill: 'character_bible', status: 'waiting', duration: '00:46', cost: 'local', needsReview: true },
  { id: 'style', label: '画风设定', skill: 'style_bible', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'storyboard', label: '分镜审核', skill: 'storyboard_director', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'image_prompt', label: '图片提示词', skill: 'image_prompt_builder', status: 'waiting', duration: '--', cost: 'local', needsReview: false },
  { id: 'image', label: '图片资产审核', skill: 'image_generation', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'video_prompt', label: '视频提示词', skill: 'video_prompt_builder', status: 'waiting', duration: '--', cost: 'local', needsReview: false },
  { id: 'video', label: '视频片段审核', skill: 'video_generation', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'voice', label: '配音任务', skill: 'voice_casting', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'subtitle', label: '字幕结构', skill: 'subtitle_generator', status: 'waiting', duration: '--', cost: 'local', needsReview: false },
  { id: 'edit', label: '粗剪计划', skill: 'auto_editor', status: 'waiting', duration: '--', cost: 'local', needsReview: false },
  { id: 'quality', label: '质量检查', skill: 'quality_checker', status: 'waiting', duration: '--', cost: 'local', needsReview: true },
  { id: 'export', label: '导出占位包', skill: 'export_packager', status: 'waiting', duration: '--', cost: 'local', needsReview: false },
];

export function ProductionConsolePage({ onBack, projectId }: ProductionConsolePageProps) {
  const fallbackProject = useMemo(() => toProjectDetail(projectId), [projectId]);
  const [project, setProject] = useState<ProjectDetail>(fallbackProject);
  const [draft, setDraft] = useState<ProjectDraft>(() => toProjectDraft(fallbackProject));
  const [running, setRunning] = useState(false);
  const [starting, setStarting] = useState(false);
  const [saving, setSaving] = useState(false);
  const [activeIndex, setActiveIndex] = useState(2);
  const [apiState, setApiState] = useState<ApiState>('loading');
  const [job, setJob] = useState<ProductionJob | null>(null);
  const [stages, setStages] = useState<ProductionStage[]>(productionStagePreview);
  const [streamJobId, setStreamJobId] = useState<string | null>(null);
  const [syncMessage, setSyncMessage] = useState('正在连接 Control API');
  const [actioning, setActioning] = useState<JobCommand | null>(null);
  const [taskOutputs, setTaskOutputs] = useState<GenerationTaskRecord[]>([]);
  const [projectAssets, setProjectAssets] = useState<ProjectAssetRecord[]>([]);
  const [costLedger, setCostLedger] = useState<CostLedgerRecord[]>([]);
  const [draftMessage, setDraftMessage] = useState('输入尚未保存');

  const refreshProductionArtifacts = useCallback(
    async (jobId: string) => {
      const [nextJob, nextTasks, nextAssets, nextCosts] = await Promise.all([
        getProductionJob(jobId),
        listProductionTasks(jobId),
        listProjectAssets(project.id),
        listProjectCosts(project.id),
      ]);

      setJob(nextJob);
      setStages(nextJob.stages);
      setTaskOutputs(nextTasks);
      setProjectAssets(nextAssets);
      setCostLedger(nextCosts);
    },
    [project.id],
  );

  useEffect(() => {
    const controller = new AbortController();

    setApiState('loading');
    setSyncMessage('正在连接 Control API');

    getProject(projectId, controller.signal)
      .then((nextProject) => {
        setProject(nextProject);
        setDraft(toProjectDraft(nextProject));
        setJob(null);
        setStages(productionStagePreview);
        setTaskOutputs([]);
        setProjectAssets([]);
        setCostLedger([]);
        setStreamJobId(null);
        setRunning(false);
        setActiveIndex(0);
        setApiState('control-api');
        setSyncMessage('Control API 已连接');
        setDraftMessage('输入已从 Control API 恢复');
      })
      .catch(() => {
        setProject(fallbackProject);
        setDraft(toProjectDraft(fallbackProject));
        setJob(null);
        setStages(mockStages);
        setTaskOutputs([]);
        setProjectAssets([]);
        setCostLedger([]);
        setStreamJobId(null);
        setRunning(false);
        setApiState('mock-fallback');
        setSyncMessage('Control API 未启动，当前只显示本地示例数据');
        setDraftMessage('Control API 未连接，暂不能保存输入');
      });

    return () => controller.abort();
  }, [fallbackProject, projectId]);

  useEffect(() => {
    if (!running || apiState !== 'mock-fallback') {
      return undefined;
    }

    const timer = window.setInterval(() => {
      setActiveIndex((current) => (current >= mockStages.length - 1 ? 1 : current + 1));
    }, 1200);

    return () => window.clearInterval(timer);
  }, [apiState, running]);

  useEffect(() => {
    if (!streamJobId) {
      return undefined;
    }

    return watchProductionJob(
      streamJobId,
      (event) => {
        applyProductionEvent(event, setJob, setStages);
        setSyncMessage(event.message);
        void refreshProductionArtifacts(event.jobId).catch(() => {
          setSyncMessage('Control API 已推送状态，但结果索引暂时未同步。');
        });

        if (event.jobStatus === 'completed' || event.jobStatus === 'failed') {
          setRunning(false);
          setStreamJobId(null);
        }
      },
      () => {
        setSyncMessage('SSE 连接已结束或等待下一次进度事件');
      },
    );
  }, [refreshProductionArtifacts, streamJobId]);

  const visibleStages = useMemo(() => {
    if (apiState !== 'mock-fallback' || !running) {
      return stages;
    }

    return mockStages.map((stage, index): ProductionStage => {
      if (index < activeIndex) {
        return { ...stage, status: 'done' };
      }

      if (index === activeIndex) {
        return { ...stage, status: stage.needsReview ? 'review' : 'running' };
      }

      return { ...stage, status: 'waiting' };
    });
  }, [activeIndex, apiState, running, stages]);
  const activeVisibleStage = useMemo(
    () => visibleStages.find((stage) => stage.status === 'running' || stage.status === 'review') ?? null,
    [visibleStages],
  );

  const saveDraft = useCallback(async () => {
    const validationError = validateProjectDraft(draft);
    if (validationError) {
      setDraftMessage(validationError);
      return null;
    }

    if (apiState !== 'control-api') {
      setDraftMessage('Control API 未连接，暂不能保存输入');
      return null;
    }

    setSaving(true);

    try {
      const savedProject = await updateProject(project.id, draft);
      setProject(savedProject);
      setDraft(toProjectDraft(savedProject));
      setDraftMessage('输入已保存到 Control API / SQLite');
      return savedProject;
    } catch (error) {
      setDraftMessage(error instanceof Error ? error.message : '输入保存失败');
      return null;
    } finally {
      setSaving(false);
    }
  }, [apiState, draft, project.id]);

  const startJob = async () => {
    const savedProject = await saveDraft();
    if (!savedProject) {
      return;
    }

    setStarting(true);

    try {
      setJob(null);
      setStages(productionStagePreview);
      setTaskOutputs([]);
      setProjectAssets([]);
      setCostLedger([]);
      const nextJob = await startProductionJob(savedProject.id);
      setJob(nextJob);
      setStages(nextJob.stages);
      setRunning(true);
      setApiState('control-api');
      setStreamJobId(nextJob.id);
      setSyncMessage(`Control API 已根据当前输入启动任务 ${nextJob.id}`);
      void refreshProductionArtifacts(nextJob.id);
    } catch (error) {
      setRunning(false);
      setSyncMessage(error instanceof Error ? error.message : 'Control API 暂时无法启动生产任务。');
    } finally {
      setStarting(false);
    }
  };

  const runJobCommand = async (command: JobCommand, notes = '') => {
    if (!job || apiState !== 'control-api') {
      return;
    }

    setActioning(command);

    try {
      const commandMap: Record<'pause' | 'resume' | 'retry', (jobId: string) => Promise<ProductionJob>> = {
        pause: pauseProductionJob,
        resume: resumeProductionJob,
        retry: retryProductionJob,
      };
      const nextJob =
        command === 'checkpoint-approve'
          ? await approveProductionCheckpoint(job.id, notes)
          : command === 'checkpoint-reject'
            ? await rejectProductionCheckpoint(job.id, notes)
            : await commandMap[command](job.id);

      setJob(nextJob);
      setStages(nextJob.stages);
      setRunning(nextJob.status === 'running' || nextJob.status === 'paused');
      void refreshProductionArtifacts(nextJob.id);

      if ((command === 'retry' || command === 'resume' || command === 'checkpoint-approve') && nextJob.status === 'running') {
        setStreamJobId(nextJob.id);
      }

      setSyncMessage(commandMessages[command]);
    } catch {
      setSyncMessage('Control API 暂时没有接受该操作，请稍后重试。');
    } finally {
      setActioning(null);
    }
  };

  const saveStoryboardEdits = useCallback<StoryboardSaveHandler>(
    async (taskId, request) => {
      if (apiState !== 'control-api') {
        throw new Error('Control API 未连接，无法保存分镜。');
      }

      const response = await updateStoryboardTask(taskId, request);
      setSyncMessage(response.message);
      await refreshProductionArtifacts(response.jobId);
      setRunning(true);
      setStreamJobId(null);
      return response.message;
    },
    [apiState, refreshProductionArtifacts],
  );

  const regenerateStoryboardShotEdit = useCallback<StoryboardRegenerateHandler>(
    async (taskId, shotId, notes) => {
      if (apiState !== 'control-api') {
        throw new Error('Control API 未连接，无法重算镜头。');
      }

      const response = await regenerateStoryboardShot(taskId, shotId, { notes });
      setSyncMessage(response.message);
      await refreshProductionArtifacts(response.jobId);
      setRunning(true);
      setStreamJobId(null);
      return response.message;
    },
    [apiState, refreshProductionArtifacts],
  );

  return (
    <section className="console-page">
      <header className="console-header">
        <button className="ghost-button" onClick={onBack} type="button" aria-label="返回项目列表">
          <ArrowLeft size={18} />
        </button>
        <div>
          <p className="eyebrow">生产控制台</p>
          <h1>{project.title}</h1>
        </div>
        <div className="console-header-actions">
          <span className={apiState === 'control-api' ? 'api-chip connected' : 'api-chip'}>
            {apiState === 'control-api' ? 'Control API' : apiState === 'loading' ? '连接中' : '示例数据'}
          </span>
        </div>
      </header>

      <div className="console-layout">
        <ConversationPanel
          apiReady={apiState === 'control-api'}
          draft={draft}
          draftMessage={draftMessage}
          onDraftChange={setDraft}
          onSave={saveDraft}
          onStart={startJob}
          running={running}
          saving={saving}
          starting={starting}
        />
        <ProgressPanel
          actioning={actioning}
          canUseActions={apiState === 'control-api'}
          job={job}
          onApproveCheckpoint={(notes) => runJobCommand('checkpoint-approve', notes)}
          onPause={() => runJobCommand('pause')}
          onRejectCheckpoint={(notes) => runJobCommand('checkpoint-reject', notes)}
          onResume={() => runJobCommand('resume')}
          onRetry={() => runJobCommand('retry')}
          stages={visibleStages}
          syncMessage={syncMessage}
          tasks={taskOutputs}
        />
        <ResultsPanel
          apiState={apiState}
          assets={projectAssets}
          costs={costLedger}
          currentSkillName={activeVisibleStage?.skill ?? null}
          jobStarted={Boolean(job)}
          jobCompleted={job?.status === 'completed'}
          onRegenerateStoryboardShot={regenerateStoryboardShotEdit}
          onSaveStoryboard={saveStoryboardEdits}
          tasks={taskOutputs}
        />
      </div>
    </section>
  );
}

interface ConversationPanelProps {
  apiReady: boolean;
  draft: ProjectDraft;
  draftMessage: string;
  onDraftChange: Dispatch<SetStateAction<ProjectDraft>>;
  onSave: () => Promise<ProjectDetail | null>;
  onStart: () => void;
  running: boolean;
  saving: boolean;
  starting: boolean;
}

function ConversationPanel({
  apiReady,
  draft,
  draftMessage,
  onDraftChange,
  onSave,
  onStart,
  running,
  saving,
  starting,
}: ConversationPanelProps) {
  const storyLength = countStoryCharacters(draft.storyText);

  return (
    <section className="workspace-panel input-panel" aria-label="对话输入区">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">输入</p>
          <h2>故事入口</h2>
        </div>
        <SlidersHorizontal size={18} />
      </div>

      <label>
        <span>标题</span>
        <input
          aria-label="项目标题"
          value={draft.title}
          onChange={(event) => onDraftChange((current) => ({ ...current, title: event.target.value }))}
        />
      </label>

      <textarea
        aria-label="故事或创作要求"
        value={draft.storyText}
        onChange={(event) => onDraftChange((current) => ({ ...current, storyText: event.target.value }))}
      />

      <div className={storyLength >= 500 && storyLength <= 2000 ? 'input-hint ok' : 'input-hint'}>
        <span>{storyLength}/2000</span>
        <span>{draftMessage}</span>
      </div>

      <div className="segmented-control" aria-label="生产模式">
        <button
          className={draft.mode === 'fast' ? 'selected' : ''}
          onClick={() => onDraftChange((current) => ({ ...current, mode: 'fast' }))}
          type="button"
        >
          极速模式
        </button>
        <button
          className={draft.mode === 'director' ? 'selected' : ''}
          onClick={() => onDraftChange((current) => ({ ...current, mode: 'director' }))}
          type="button"
        >
          导演模式
        </button>
      </div>

      <div className="field-grid">
        <label>
          <span>目标时长</span>
          <select
            value={String(draft.targetDuration)}
            onChange={(event) =>
              onDraftChange((current) => ({ ...current, targetDuration: Number(event.target.value) }))
            }
          >
            <option value="30">30 秒</option>
            <option value="45">45 秒</option>
            <option value="60">60 秒</option>
          </select>
        </label>
        <label>
          <span>画幅</span>
          <select
            value={draft.aspectRatio}
            onChange={(event) =>
              onDraftChange((current) => ({
                ...current,
                aspectRatio: event.target.value as ProjectDraft['aspectRatio'],
              }))
            }
          >
            <option value="9:16">9:16 竖屏</option>
            <option value="16:9">16:9 横屏</option>
            <option value="1:1">1:1 方屏</option>
          </select>
        </label>
      </div>

      <label>
        <span>风格</span>
        <input
          aria-label="风格预设"
          value={draft.stylePreset}
          onChange={(event) => onDraftChange((current) => ({ ...current, stylePreset: event.target.value }))}
        />
      </label>

      <div className="input-actions">
        <button
          className="secondary-button"
          disabled={!apiReady || saving || starting}
          onClick={() => void onSave()}
          type="button"
        >
          <Save size={17} />
          <span>{saving ? '保存中' : '保存'}</span>
        </button>
        <button className="primary-button full-width" disabled={!apiReady || starting || saving} onClick={onStart} type="button">
        {running ? <RotateCcw size={18} /> : <Send size={18} />}
        <span>{starting ? '启动中' : running ? '重新生成' : '开始生成'}</span>
      </button>
      </div>
    </section>
  );
}

function ProgressPanel({
  actioning,
  canUseActions,
  job,
  onApproveCheckpoint,
  onPause,
  onRejectCheckpoint,
  onResume,
  onRetry,
  stages,
  syncMessage,
  tasks,
}: {
  actioning: JobCommand | null;
  canUseActions: boolean;
  job: ProductionJob | null;
  onApproveCheckpoint: (notes: string) => void;
  onPause: () => void;
  onRejectCheckpoint: (notes: string) => void;
  onResume: () => void;
  onRetry: () => void;
  stages: ProductionStage[];
  syncMessage: string;
  tasks: GenerationTaskRecord[];
}) {
  const [checkpointNotes, setCheckpointNotes] = useState('');
  const [checkpointNotesHint, setCheckpointNotesHint] = useState('');
  const checkpointNotesRef = useRef<HTMLTextAreaElement | null>(null);
  const activeStage = stages.find((stage) => stage.status === 'running' || stage.status === 'review');
  const activeStageInfo = activeStage ? getStageDisplayInfo(activeStage) : null;
  const activeTask =
    activeStage && tasks.length > 0
      ? tasks.find((task) => task.skillName === activeStage.skill && task.outputJson) ?? null
      : null;
  const activeTaskEnvelope = parseSkillEnvelope(activeTask?.outputJson ?? null);
  const activeTaskPreviewSections = activeTask ? buildSkillPreviewSections(activeTask, activeTaskEnvelope) : [];
  const isCheckpointReady = Boolean(activeStage?.needsReview && activeStage.status === 'review');
  const canPause = canUseActions && job?.status === 'running';
  const canResume = canUseActions && job?.status === 'paused' && !isCheckpointReady;
  const canCheckpoint = canUseActions && job?.status === 'paused' && isCheckpointReady;
  const canRejectCheckpoint = canCheckpoint;
  const rejectNeedsNotes = canCheckpoint && checkpointNotes.trim().length === 0;
  const canRetry = canUseActions && (job?.status === 'failed' || stages.some((stage) => stage.status === 'blocked' || stage.status === 'failed'));
  const syncTone = syncMessage.includes('退回') || syncMessage.includes('拒绝') ? 'danger' : syncMessage.includes('通过') ? 'success' : '';
  const reviewChipClass = (stage: ProductionStage) =>
    `review-chip ${stage.status === 'review' ? 'active' : stage.status === 'done' ? 'done' : ''}`;
  const controlHint = buildJobControlHint(job?.status ?? null, isCheckpointReady);
  const handleRejectCheckpoint = () => {
    if (rejectNeedsNotes) {
      setCheckpointNotesHint('请先写明要修改的地方，例如：主角应为杜丽娘，春香和柳梦梅是配角。');
      checkpointNotesRef.current?.focus();
      return;
    }

    onRejectCheckpoint(checkpointNotes);
  };

  useEffect(() => {
    setCheckpointNotes('');
    setCheckpointNotesHint('');
  }, [activeStage?.skill, job?.id]);

  return (
    <section className="workspace-panel progress-panel" aria-label="任务进度流">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">进度</p>
          <h2>{activeStage ? getStageDisplayInfo(activeStage).title : job?.status === 'completed' ? '已完成' : '等待开始'}</h2>
        </div>
        <span className={job?.status === 'running' ? 'live-dot active' : 'live-dot'} />
      </div>

      <div className="job-summary">
        <span>{job ? `job ${job.id.slice(0, 12)}` : '尚未创建 job'}</span>
        <strong>{job ? `${job.progress}%` : '0%'}</strong>
      </div>

      <div className="job-controls" aria-label="生产任务操作">
        <button
          className="command-button"
          disabled={!canPause || actioning !== null}
          onClick={onPause}
          title={isCheckpointReady ? '当前已暂停在审核点，无需再次暂停。' : undefined}
          type="button"
        >
          <Pause size={15} />
          <span>{isCheckpointReady ? '已暂停' : actioning === 'pause' ? '暂停中' : '暂停'}</span>
        </button>
        <button
          className="command-button"
          disabled={!canResume || actioning !== null}
          onClick={onResume}
          title={isCheckpointReady ? '当前需要先通过或退回审核；通过后会自动继续。' : undefined}
          type="button"
        >
          <Play size={15} />
          <span>{isCheckpointReady ? '通过后继续' : actioning === 'resume' ? '恢复中' : '恢复'}</span>
        </button>
        <button
          className="command-button confirm"
          disabled={!canCheckpoint || actioning !== null}
          onClick={() => onApproveCheckpoint(checkpointNotes)}
          type="button"
        >
          <CheckCircle2 size={15} />
          <span>{actioning === 'checkpoint-approve' ? '确认中' : '通过并继续'}</span>
        </button>
        <button
          className="command-button danger"
          disabled={!canRejectCheckpoint || actioning !== null}
          onClick={handleRejectCheckpoint}
          type="button"
          title={rejectNeedsNotes ? '点击后会先要求填写需要修改的地方。' : undefined}
        >
          <XCircle size={15} />
          <span>{actioning === 'checkpoint-reject' ? '退回中' : '退回修改'}</span>
        </button>
        <button className="command-button" disabled={!canRetry || actioning !== null} onClick={onRetry} type="button">
          <RotateCcw size={15} />
          <span>{isCheckpointReady ? '退回后重试' : actioning === 'retry' ? '重试中' : '重试失败项'}</span>
        </button>
      </div>
      <p className="job-control-hint">{controlHint}</p>

      {isCheckpointReady && activeStageInfo && (
        <section className="checkpoint-review-card" aria-label="当前审核说明">
          <div className="review-card-heading">
            <div>
              <span>当前需要确认</span>
              <strong>{activeStageInfo.title}</strong>
            </div>
            <span className="current-result-chip">待你确认</span>
          </div>
          <p>本步产出：{activeStageInfo.output}</p>
          {activeTaskPreviewSections.length > 0 ? (
            <SkillOutputPreview compact sections={activeTaskPreviewSections} />
          ) : (
            <p className="review-output-empty">正在等待本步结果写入 Control API，稍后会在这里显示可审核内容。</p>
          )}
          <div className="review-checklist">
            {activeStageInfo.check.map((item) => (
              <span key={item}>{item}</span>
            ))}
          </div>
          <div className="review-next-actions">
            <span>通过后：{activeStageInfo.approveNext}</span>
            <span>拒绝后：{activeStageInfo.rejectNext}</span>
          </div>
        </section>
      )}

      {isCheckpointReady && (
        <textarea
          ref={checkpointNotesRef}
          className="checkpoint-notes"
          value={checkpointNotes}
          onChange={(event) => {
            setCheckpointNotes(event.target.value);
            if (event.target.value.trim().length > 0) {
              setCheckpointNotesHint('');
            }
          }}
          aria-label="审核备注"
          aria-invalid={Boolean(checkpointNotesHint)}
          placeholder={canCheckpoint ? '可填写确认备注；如果退回，请写明需要修改的地方。' : '等待 Control API 同步审核状态'}
        />
      )}
      {checkpointNotesHint && <p className="checkpoint-notes-hint">{checkpointNotesHint}</p>}

      <p className={`sync-message ${syncTone}`}>{localizeProgressMessage(syncMessage)}</p>

      <div className="stage-list">
        {stages.map((stage) => (
          <article className={`stage-row ${stage.status}`} key={stage.id} aria-current={stage === activeStage ? 'step' : undefined}>
            <span className="stage-marker" />
            <div>
              <strong>{getStageDisplayInfo(stage).title}</strong>
              <span>{getStageDisplayInfo(stage).output}</span>
            </div>
            <div className="stage-meta">
              <span>{formatStageDuration(stage.duration)}</span>
              <span>{formatProviderLabel(stage.cost)}</span>
            </div>
            {stage.needsReview && <span className={reviewChipClass(stage)}>{formatReviewChip(stage)}</span>}
          </article>
        ))}
      </div>
    </section>
  );
}

function ResultsPanel({
  apiState,
  assets,
  costs,
  currentSkillName,
  jobStarted,
  jobCompleted,
  onRegenerateStoryboardShot,
  onSaveStoryboard,
  tasks,
}: {
  apiState: ApiState;
  assets: ProjectAssetRecord[];
  costs: CostLedgerRecord[];
  currentSkillName: string | null;
  jobStarted: boolean;
  jobCompleted: boolean;
  onRegenerateStoryboardShot: StoryboardRegenerateHandler;
  onSaveStoryboard: StoryboardSaveHandler;
  tasks: GenerationTaskRecord[];
}) {
  const realCards = buildRealResultCards(tasks, costs, currentSkillName);
  const cards = apiState === 'control-api' ? realCards : mockResultCards;

  return (
    <section className="results-column" aria-label="中间结果和最终交付">
      <div className="result-stack">
        {cards.length > 0 ? (
          cards.map((card) => (
            <ResultCardView
              card={card}
              key={card.id}
              onRegenerateStoryboardShot={onRegenerateStoryboardShot}
              onSaveStoryboard={onSaveStoryboard}
            />
          ))
        ) : (
          <ResultsPlaceholder jobStarted={jobStarted} />
        )}
      </div>
      <DeliveryPanel assets={assets} jobCompleted={jobCompleted} tasks={tasks} />
    </section>
  );
}

function ResultsPlaceholder({ jobStarted }: { jobStarted: boolean }) {
  const previewStages = productionStagePreview.slice(0, 6);

  return (
    <article className="result-card results-placeholder-card">
      <div className="result-card-heading">
        <span className="result-icon">
          <FileText size={17} />
        </span>
        <div>
          <h3>{jobStarted ? '等待第一步结果' : '结果将在生成后出现'}</h3>
          <span>{jobStarted ? '正在同步 Control API' : '尚未开始'}</span>
        </div>
      </div>
      <p>
        {jobStarted
          ? 'Worker 写入每一步结果后，右侧会按流程逐步添加真实结果卡。'
          : '点击开始生成后，这里只展示 Control API 返回的真实产物；未生成前不显示示例内容。'}
      </p>
      <div className="placeholder-step-list" aria-label="待生成步骤预览">
        {previewStages.map((stage) => (
          <div className="placeholder-step" key={stage.id}>
            <span>{getStageDisplayInfo(stage).title}</span>
            <small>待生成</small>
          </div>
        ))}
      </div>
    </article>
  );
}

function ResultCardView({
  card,
  onRegenerateStoryboardShot,
  onSaveStoryboard,
}: {
  card: DisplayResultCard;
  onRegenerateStoryboardShot: StoryboardRegenerateHandler;
  onSaveStoryboard: StoryboardSaveHandler;
}) {
  const Icon = resultIcons[card.kind];
  const statusLabel = card.taskStatus ? formatTaskStatus(card.taskStatus) : card.status === 'locked' ? '已锁定' : card.status === 'ready' ? '可确认' : '草稿';
  const canEditStoryboard = card.skillName === 'storyboard_director' && Boolean(card.envelope?.ok && card.envelope.data);

  return (
    <article className={`result-card ${card.isCurrent ? 'current' : ''} ${card.taskStatus === 'failed' ? 'failed' : ''}`}>
      <div className="result-card-heading">
        <span className="result-icon">
          <Icon size={17} />
        </span>
        <div>
          <h3>{card.title}</h3>
          <span>{statusLabel}</span>
        </div>
        {card.isCurrent && <span className="current-result-chip">当前审核</span>}
      </div>
      <p>{card.summary}</p>
      {card.previewSections && card.previewSections.length > 0 && <SkillOutputPreview sections={card.previewSections} />}
      {canEditStoryboard && (
        <StoryboardEditor
          card={card}
          onRegenerateStoryboardShot={onRegenerateStoryboardShot}
          onSaveStoryboard={onSaveStoryboard}
        />
      )}
      <ul className="result-details">
        {card.details.map((detail) => (
          <li key={detail}>{detail}</li>
        ))}
      </ul>
      {card.technicalDetails && card.technicalDetails.length > 0 && (
        <details className="technical-details">
          <summary>技术详情</summary>
          <ul>
            {card.technicalDetails.map((detail) => (
              <li key={detail}>{detail}</li>
            ))}
          </ul>
        </details>
      )}
    </article>
  );
}

function StoryboardEditor({
  card,
  onRegenerateStoryboardShot,
  onSaveStoryboard,
}: {
  card: DisplayResultCard;
  onRegenerateStoryboardShot: StoryboardRegenerateHandler;
  onSaveStoryboard: StoryboardSaveHandler;
}) {
  const initialShots = useMemo(() => extractStoryboardEditableShots(card.envelope), [card.envelope]);
  const [shots, setShots] = useState<EditableStoryboardShot[]>(initialShots);
  const [notes, setNotes] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [regeneratingShotId, setRegeneratingShotId] = useState<string | null>(null);
  const targetSeconds = getStoryboardTargetSeconds(card.envelope);
  const totalSeconds = useMemo(
    () => shots.reduce((total, shot) => total + Number(shot.durationSeconds || 0), 0),
    [shots],
  );
  const timingTone = targetSeconds > 0 && Math.abs(totalSeconds - targetSeconds) > 1 ? 'danger' : 'success';

  useEffect(() => {
    setShots(initialShots);
    setMessage(null);
  }, [initialShots]);

  const updateShot = useCallback(
    <K extends keyof StoryboardShotEdit>(shotId: string, field: K, value: StoryboardShotEdit[K]) => {
      setShots((current) =>
        current.map((shot) => (shot.shotId === shotId ? { ...shot, [field]: value } : shot)),
      );
    },
    [],
  );

  const saveEdits = async () => {
    setSaving(true);
    setMessage(null);

    try {
      const feedback = await onSaveStoryboard(card.id, {
        notes,
        shots: shots.map(toStoryboardShotEdit),
      });
      setMessage(feedback);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '分镜保存失败，请稍后重试。');
    } finally {
      setSaving(false);
    }
  };

  const regenerateShot = async (shot: EditableStoryboardShot) => {
    const trimmedNotes = notes.trim();
    if (!trimmedNotes) {
      setMessage('请先填写修改备注，再重算单个镜头。');
      return;
    }

    setRegeneratingShotId(shot.shotId);
    setMessage(null);

    try {
      const feedback = await onRegenerateStoryboardShot(card.id, shot.shotId, trimmedNotes);
      setMessage(feedback);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '镜头重算失败，请稍后重试。');
    } finally {
      setRegeneratingShotId(null);
    }
  };

  if (shots.length === 0) {
    return null;
  }

  return (
    <section className="storyboard-editor" aria-label="分镜编辑">
      <div className="storyboard-editor-heading">
        <span>
          <PencilLine size={15} />
          分镜编辑
        </span>
        <strong className={`storyboard-timing ${timingTone}`}>
          {totalSeconds} / {targetSeconds || '未定'} 秒
        </strong>
      </div>
      <label className="storyboard-notes">
        <span>修改备注</span>
        <textarea
          onChange={(event) => setNotes(event.target.value)}
          placeholder="记录为什么调整分镜；退回或单镜头重算会消费这段备注。"
          value={notes}
        />
      </label>
      <div className="storyboard-shot-list">
        {shots.map((shot, index) => (
          <section className="storyboard-shot-editor" key={shot.shotId}>
            <div className="storyboard-shot-heading">
              <div>
                <strong>{shot.label || `镜头 ${index + 1}`}</strong>
                <span>开始 {shot.startSecond}s · ID {shot.shotId}</span>
              </div>
              <button
                className="command-button"
                disabled={saving || regeneratingShotId !== null}
                onClick={() => regenerateShot(shot)}
                type="button"
              >
                <RotateCcw size={13} />
                {regeneratingShotId === shot.shotId ? '重算中' : '单镜头重算'}
              </button>
            </div>
            <div className="storyboard-shot-fields">
              <label className="compact-field">
                <span>时长</span>
                <input
                  max={15}
                  min={1}
                  onChange={(event) => updateShot(shot.shotId, 'durationSeconds', Number(event.target.value))}
                  type="number"
                  value={shot.durationSeconds}
                />
              </label>
              <label className="compact-field">
                <span>景别</span>
                <input
                  onChange={(event) => updateShot(shot.shotId, 'shotSize', event.target.value)}
                  value={shot.shotSize}
                />
              </label>
              <label className="wide-field">
                <span>场景</span>
                <textarea
                  onChange={(event) => updateShot(shot.shotId, 'scene', event.target.value)}
                  value={shot.scene}
                />
              </label>
              <label className="wide-field">
                <span>画面动作</span>
                <textarea
                  onChange={(event) => updateShot(shot.shotId, 'visualAction', event.target.value)}
                  value={shot.visualAction}
                />
              </label>
              <label className="wide-field">
                <span>镜头运动</span>
                <input
                  onChange={(event) => updateShot(shot.shotId, 'cameraMovement', event.target.value)}
                  value={shot.cameraMovement}
                />
              </label>
              <label className="wide-field">
                <span>声音</span>
                <input
                  onChange={(event) => updateShot(shot.shotId, 'soundNote', event.target.value)}
                  value={shot.soundNote}
                />
              </label>
              <label className="wide-field">
                <span>对白</span>
                <textarea
                  onChange={(event) => updateShot(shot.shotId, 'dialogue', event.target.value)}
                  value={shot.dialogue}
                />
              </label>
              <label className="wide-field">
                <span>旁白</span>
                <textarea
                  onChange={(event) => updateShot(shot.shotId, 'narration', event.target.value)}
                  value={shot.narration}
                />
              </label>
            </div>
          </section>
        ))}
      </div>
      {message && <p className="storyboard-editor-message">{message}</p>}
      <div className="storyboard-editor-actions">
        <button className="primary-button" disabled={saving || regeneratingShotId !== null} onClick={saveEdits} type="button">
          <Save size={14} />
          {saving ? '保存中' : '保存分镜并重置下游'}
        </button>
      </div>
    </section>
  );
}

function SkillOutputPreview({ compact = false, sections }: { compact?: boolean; sections: SkillPreviewSection[] }) {
  return (
    <div className={`skill-output-preview ${compact ? 'compact' : ''}`}>
      {sections.map((section) => (
        <section className="preview-section" key={section.title}>
          <strong>{section.title}</strong>
          <div className="preview-items">
            {section.items.map((item) => (
              <div
                className={[
                  'preview-item',
                  item.color ? 'with-swatch' : '',
                  item.variant === 'block' ? 'block' : '',
                ]
                  .filter(Boolean)
                  .join(' ')}
                key={`${section.title}-${item.label}-${item.value}`}
              >
                {item.color && <span className="preview-swatch" style={{ background: item.color }} />}
                <span className="preview-label">{item.label}</span>
                <span className={item.variant === 'code' ? 'preview-value code' : 'preview-value'}>{item.value}</span>
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

function DeliveryPanel({
  assets,
  jobCompleted,
  tasks,
}: {
  assets: ProjectAssetRecord[];
  jobCompleted: boolean;
  tasks: GenerationTaskRecord[];
}) {
  const exportAssets = extractExportDeliveryAssets(tasks);
  const rows =
    exportAssets.length > 0
      ? exportAssets
      : jobCompleted && assets.length > 0
        ? assets.slice(0, 6).map((asset) => ({
            id: asset.id,
            label: asset.kind,
            format: asset.mimeType,
            size: formatFileSize(asset.fileSize),
            state: 'ready' as const,
          }))
        : mockDeliveryAssets.map((asset) => ({
            ...asset,
            state: jobCompleted ? ('ready' as const) : asset.state,
          }));

  return (
    <section className="workspace-panel delivery-panel" aria-label="最终交付区">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">交付</p>
          <h2>导出包</h2>
        </div>
        <Download size={18} />
      </div>
      <div className="asset-list">
        {rows.map((asset) => (
          <div className="asset-row" key={asset.id}>
            <div>
              <strong>{asset.label}</strong>
              <span>{asset.format}</span>
            </div>
            <span>{asset.state === 'ready' ? asset.size : '等待生成'}</span>
          </div>
        ))}
      </div>
    </section>
  );
}

function buildRealResultCards(
  tasks: GenerationTaskRecord[],
  costs: CostLedgerRecord[],
  currentSkillName: string | null,
): DisplayResultCard[] {
  return tasks
    .filter((task) => task.outputJson)
    .map((task): DisplayResultCard => {
      const envelope = parseSkillEnvelope(task.outputJson);
      const data = envelope?.data;
      const cost = costs.find((entry) => entry.taskId === task.id);
      const isCurrent = Boolean(currentSkillName && task.skillName === currentSkillName && task.status === 'review');
      const outputFields = data ? Object.keys(data).slice(0, 5).map(formatFieldName).join(' / ') : '结果解析失败';
      const info = getStageDisplayInfoBySkill(task.skillName);

      return {
        id: task.id,
        title: skillTitle(task.skillName),
        kind: resultKindForSkill(task.skillName),
        status: task.status === 'completed' ? 'locked' : task.status === 'review' ? 'ready' : 'draft',
        envelope,
        isCurrent,
        previewSections: buildSkillPreviewSections(task, envelope),
        queueIndex: task.queueIndex,
        skillName: task.skillName,
        taskStatus: task.status,
        summary: summarizeSkillOutput(task, envelope),
        details: [
          `状态：${formatTaskStatus(task.status)}`,
          `尝试次数：第 ${task.attemptCount} 次`,
          `产出：${info.output}`,
          cost ? `成本：${formatCostAmount(cost)}` : '成本：本地测试，无真实计费',
        ],
        technicalDetails: [
          `内部技能：${task.skillName}`,
          `运行来源：${formatProviderLabel(task.provider)}`,
          `数据字段：${outputFields}`,
        ],
      };
    })
    .sort((left, right) => (left.queueIndex ?? 0) - (right.queueIndex ?? 0));
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

function extractStoryboardEditableShots(envelope: SkillEnvelope | null | undefined): EditableStoryboardShot[] {
  if (!envelope?.ok || !envelope.data) {
    return [];
  }

  return asArray(envelope.data.shots)
    .map((entry, index): EditableStoryboardShot => {
      const shot = asRecord(entry);
      const camera = asRecord(shot.camera);
      const shotId = asString(shot.shot_id, `shot-${index + 1}`);

      return {
        shotId,
        label: asString(shot.shot_label, `镜头 ${index + 1}`),
        startSecond: Number(shot.start_second || 0),
        durationSeconds: Number(shot.duration_seconds || 1),
        scene: asString(shot.scene),
        visualAction: asString(shot.visual_action),
        shotSize: asString(shot.shot_size, '中景'),
        cameraMovement: asString(camera.motion, '固定镜头'),
        soundNote: asString(shot.sound_note),
        dialogue: formatEditableDialogue(shot.dialogue),
        narration: asString(shot.narration),
      };
    })
    .filter((shot) => shot.shotId.length > 0);
}

function toStoryboardShotEdit(shot: EditableStoryboardShot): StoryboardShotEdit {
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

function getStoryboardTargetSeconds(envelope: SkillEnvelope | null | undefined): number {
  const data = envelope?.data ?? {};
  const overview = asRecord(data.film_overview);
  const timing = asRecord(data.timing_summary);
  return Number(data.target_duration_seconds ?? overview.target_duration_seconds ?? timing.target_duration_seconds ?? 0);
}

function formatEditableDialogue(value: unknown): string {
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

function buildSkillPreviewSections(task: GenerationTaskRecord, envelope: SkillEnvelope | null): SkillPreviewSection[] {
  if (!envelope?.ok || !envelope.data) {
    return [];
  }

  const data = envelope.data;
  if (task.skillName === 'style_bible') {
    return buildStyleBiblePreview(data);
  }

  if (task.skillName === 'storyboard_director') {
    return buildStoryboardPreview(data);
  }

  return buildGenericSkillPreview(data);
}

function buildStyleBiblePreview(data: Record<string, any>): SkillPreviewSection[] {
  const visualStyle = asRecord(data.visual_style);
  const lighting = asRecord(data.lighting);
  const environment = asRecord(data.environment_design);
  const camera = asRecord(data.camera_language);
  const promptBlocks = asRecord(data.reusable_prompt_blocks);
  const review = asRecord(data.review);

  return [
    {
      title: '画风总览',
      items: compactItems([
        previewItem('画风名', data.style_name),
        previewItem('渲染', visualStyle.rendering),
        previewItem('纹理', visualStyle.texture),
        previewItem('情绪', visualStyle.mood),
      ]),
    },
    {
      title: '色板',
      items: asArray(data.color_palette)
        .slice(0, 6)
        .map((color) => ({
          color: asString(asRecord(color).hex),
          label: asString(asRecord(color).name, '颜色'),
          value: `${asString(asRecord(color).hex)}，${asString(asRecord(color).usage)}`,
        }))
        .filter((item) => item.value.trim().length > 1),
    },
    {
      title: '灯光与场景',
      items: compactItems([
        previewItem('主光', lighting.key_light),
        previewItem('提示光', lighting.accent_light),
        previewItem('灯光规则', joinPreviewList(lighting.rules, 3)),
        previewItem('地点', joinPreviewList(environment.primary_locations, 3)),
        previewItem('避免', environment.avoid),
      ]),
    },
    {
      title: '镜头语言',
      items: compactItems([
        previewItem('画幅', camera.aspect_ratio),
        previewItem('镜头规则', joinPreviewList(camera.shot_rules, 3)),
        previewItem('运动规则', joinPreviewList(camera.movement_rules, 2)),
        previewItem('构图规则', joinPreviewList(camera.composition_rules, 2)),
      ]),
    },
    {
      title: '角色与禁忌',
      items: compactItems([
        previewItem('角色一致性', summarizeCharacterRules(data.character_rendering_rules)),
        previewItem('负面提示词', joinPreviewList(data.negative_prompt, 8)),
        previewItem('审核提示', joinPreviewList(review.notes, 2)),
      ]),
    },
    {
      title: '后续可复用提示词块',
      items: compactItems([
        previewItem('基础风格', promptBlocks.base_style, 'code'),
        previewItem('角色锁定', promptBlocks.character_consistency, 'code'),
        previewItem('场景', promptBlocks.environment, 'code'),
      ]),
    },
  ].filter((section) => section.items.length > 0);
}

function buildStoryboardPreview(data: Record<string, any>): SkillPreviewSection[] {
  const overview = asRecord(data.film_overview);
  const timing = asRecord(data.timing_summary);
  const validation = asRecord(data.validation_report);
  const parts = asArray(data.storyboard_parts).map(asRecord);
  const sections: SkillPreviewSection[] = [];

  const overviewItems = compactItems([
    previewItem('影片主题', overview.theme ?? data.title),
    previewItem('总时长', formatStoryboardDuration(overview, timing)),
    previewItem('镜头总数', formatStoryboardShotCount(overview, data.shots)),
    previewItem('风格基调', overview.style_tone),
    previewItem('摄影定调', overview.camera_setup, 'block'),
  ]);

  if (overviewItems.length > 0) {
    sections.push({ title: '影片概览', items: overviewItems });
  }

  parts.forEach((part, index) => {
    const partShots = asArray(part.shots).map(asRecord);
    const partItems = compactItems([
      previewItem('时间光线', part.time_weather_light, 'block'),
      previewItem('人物道具', part.cast_and_props, 'block'),
      previewItem('人物站位', part.absolute_blocking, 'block'),
      previewItem('本段风格', part.style, 'block'),
      ...partShots.map((shot, shotIndex) =>
        previewItem(
          asString(shot.shot_label, `镜头 ${shotIndex + 1}`),
          formatStoryboardShotPreview(shot),
          'block',
        ),
      ),
    ]);

    if (partItems.length > 0) {
      sections.push({ title: asString(part.title, `第 ${index + 1} 部分`), items: partItems });
    }
  });

  const checks = asArray(validation.checks)
    .map((entry) => {
      const check = asRecord(entry);
      const name = asString(check.name);
      const status = asString(check.status);
      const detail = asString(check.detail);
      return previewItem(name || '校验项', [status, detail].filter(Boolean).join('：'), 'block');
    })
    .filter((item): item is SkillPreviewItem => Boolean(item));

  if (checks.length > 0) {
    sections.push({ title: '格式校验', items: checks });
  }

  return sections.length > 0 ? sections : buildGenericSkillPreview(data);
}

function formatStoryboardDuration(overview: Record<string, any>, timing: Record<string, any>): string {
  const total = Number(overview.total_duration_seconds ?? timing.total_shot_seconds ?? 0);
  const target = Number(overview.target_duration_seconds ?? timing.target_duration_seconds ?? 0);

  if (total > 0 && target > 0 && total !== target) {
    return `${total} 秒，目标 ${target} 秒`;
  }

  return total > 0 ? `${total} 秒` : '';
}

function formatStoryboardShotCount(overview: Record<string, any>, shots: unknown): string {
  const count = Number(overview.shot_count || asArray(shots).length || 0);
  return count > 0 ? `${count} 个镜头` : '';
}

function formatStoryboardShotPreview(shot: Record<string, any>): string {
  const duration = Number(shot.duration_seconds || 0);
  const lines = [
    duration > 0 ? `时长：${duration.toFixed(1)} 秒` : '',
    asString(shot.environment_description) ? `环境描写：${asString(shot.environment_description)}` : '',
    asString(shot.time_slice) ? `时间切片与画面细分：${asString(shot.time_slice)}` : '',
    asString(shot.shot_size) ? `镜头景别：${asString(shot.shot_size)}` : '',
    asString(shot.camera_movement) ? `镜头运动与衔接：${asString(shot.camera_movement)}` : '',
    asString(shot.sound_effect) ? `音效：${asString(shot.sound_effect)}` : '',
    asString(shot.background_music) ? `背景音乐：${asString(shot.background_music)}` : '',
  ].filter(Boolean);

  return lines.join('\n');
}

function buildGenericSkillPreview(data: Record<string, any>): SkillPreviewSection[] {
  const headlineItems = compactItems([
    previewItem('标题', data.title),
    previewItem('摘要', data.summary),
    previewItem('一句话梗概', data.logline),
    previewItem('类型', joinPreviewList(data.genres, 4)),
    previewItem('风险点', joinPreviewList(data.risks, 4)),
  ]);
  const sections: SkillPreviewSection[] = headlineItems.length > 0 ? [{ title: '核心内容', items: headlineItems }] : [];

  const arraySections: Array<[string, unknown, string]> = [
    ['角色', data.characters, 'name'],
    ['分镜', data.shots, 'shot_id'],
    ['视频片段', data.clips, 'clip_id'],
    ['字幕条目', data.subtitle_cues, 'text'],
    ['交付资产', data.delivery_assets, 'label'],
    ['检查项', data.issues, 'message'],
  ];

  arraySections.forEach(([title, value, labelKey]) => {
    const items = asArray(value)
      .slice(0, 5)
      .map((entry, index) => {
        const record = asRecord(entry);
        const label = asString(record[labelKey], `${title} ${index + 1}`);
        const description =
          asString(record.description) ||
          asString(record.summary) ||
          asString(record.visual_direction) ||
          asString(record.text) ||
          asString(record.status) ||
          previewText(record);
        return previewItem(label, description);
      })
      .filter((item): item is SkillPreviewItem => Boolean(item));

    if (items.length > 0) {
      sections.push({ title, items });
    }
  });

  if (sections.length > 0) {
    return sections;
  }

  const fallbackItems = Object.entries(data)
    .filter(([, value]) => ['string', 'number', 'boolean'].includes(typeof value))
    .slice(0, 6)
    .map(([key, value]) => previewItem(formatFieldName(key), value))
    .filter((item): item is SkillPreviewItem => Boolean(item));

  return fallbackItems.length > 0 ? [{ title: '结构化产物', items: fallbackItems }] : [];
}

function previewItem(label: string, value: unknown, variant: SkillPreviewItem['variant'] = 'text'): SkillPreviewItem | null {
  const text = previewText(value);
  return text ? { label, value: text, variant } : null;
}

function compactItems(items: Array<SkillPreviewItem | null>): SkillPreviewItem[] {
  return items.filter((item): item is SkillPreviewItem => Boolean(item));
}

function summarizeCharacterRules(value: unknown): string {
  return asArray(value)
    .slice(0, 3)
    .map((entry) => {
      const record = asRecord(entry);
      const features = joinPreviewList(record.locked_features, 2);
      return `${asString(record.name, '角色')}：${features}`;
    })
    .filter(Boolean)
    .join('；');
}

function joinPreviewList(value: unknown, limit: number): string {
  return asArray(value)
    .slice(0, limit)
    .map((entry) => previewText(entry, 90))
    .filter(Boolean)
    .join('；');
}

function previewText(value: unknown, maxLength = 150): string {
  if (value === null || value === undefined) {
    return '';
  }

  if (Array.isArray(value)) {
    return value.map((item) => previewText(item, 90)).filter(Boolean).join('；');
  }

  if (typeof value === 'object') {
    const record = asRecord(value);
    const preferred = ['title', 'name', 'summary', 'description', 'text', 'usage', 'message'];
    const preferredValue = preferred.map((key) => record[key]).find((item) => item !== undefined && item !== null);
    if (preferredValue !== undefined) {
      return previewText(preferredValue, maxLength);
    }

    return Object.entries(record)
      .slice(0, 4)
      .map(([key, entry]) => `${formatFieldName(key)}：${previewText(entry, 60)}`)
      .filter((entry) => !entry.endsWith('：'))
      .join('；');
  }

  const text = String(value).trim();
  return text.length > maxLength ? `${text.slice(0, maxLength - 1)}…` : text;
}

function asArray(value: unknown): unknown[] {
  return Array.isArray(value) ? value : [];
}

function asRecord(value: unknown): Record<string, any> {
  return value && typeof value === 'object' && !Array.isArray(value) ? (value as Record<string, any>) : {};
}

function asString(value: unknown, fallback = ''): string {
  return typeof value === 'string' && value.trim() ? value.trim() : fallback;
}

function summarizeSkillOutput(task: GenerationTaskRecord, envelope: SkillEnvelope | null): string {
  if (!envelope) {
    return task.errorMessage ?? '结果 JSON 暂时无法解析。';
  }

  if (!envelope.ok) {
    return envelope.error?.message ?? task.errorMessage ?? '该生产步骤执行失败。';
  }

  const data = envelope.data ?? {};
  if (task.skillName === 'style_bible') {
    const visualStyle = asRecord(data.visual_style);
    const paletteCount = asArray(data.color_palette).length;
    return `${asString(data.style_name, '画风设定')}：${asString(visualStyle.rendering, '已生成视觉规则')}，包含 ${paletteCount} 个色板项。`;
  }

  if (task.skillName === 'storyboard_director') {
    const overview = asRecord(data.film_overview);
    const partCount = asArray(data.storyboard_parts).length;
    const shotCount = Number(overview.shot_count || asArray(data.shots).length || 0);
    const duration = Number(overview.total_duration_seconds || data.target_duration_seconds || 0);
    return `已生成中文分镜稿：${shotCount} 个镜头，${partCount} 个部分，总时长 ${duration} 秒。`;
  }

  if (task.skillName === 'quality_checker') {
    const issueCount = Array.isArray(data.issues) ? data.issues.length : 0;
    return `质量状态 ${formatQualityStatus(data.quality_status)}，发现 ${issueCount} 个结构化检查项。`;
  }

  if (task.skillName === 'export_packager') {
    const assetCount = Array.isArray(data.delivery_assets) ? data.delivery_assets.length : 0;
    return `已生成 ${assetCount} 个导出占位资产结构，等待后续真实渲染适配器。`;
  }

  if (Array.isArray(data.shots)) {
    return `已生成 ${data.shots.length} 个可审阅分镜。`;
  }

  if (Array.isArray(data.clips)) {
    return `已生成 ${data.clips.length} 个模拟视频片段结构。`;
  }

  if (Array.isArray(data.characters)) {
    return `已生成 ${data.characters.length} 个角色设定。`;
  }

  if (Array.isArray(data.subtitle_cues)) {
    return `已生成 ${data.subtitle_cues.length} 条可导出字幕条目。`;
  }

  return String(data.summary ?? data.title ?? `${skillTitle(task.skillName)}结果已写入数据库。`);
}

function extractExportDeliveryAssets(tasks: GenerationTaskRecord[]) {
  const exportTask = tasks.find((task) => task.skillName === 'export_packager' && task.outputJson);
  const envelope = parseSkillEnvelope(exportTask?.outputJson ?? null);
  const deliveryAssets = envelope?.data?.delivery_assets;

  if (!Array.isArray(deliveryAssets)) {
    return [];
  }

  return deliveryAssets.map((asset, index) => ({
    id: String(asset.asset_id ?? `delivery-${index}`),
    label: String(asset.label ?? asset.kind ?? '导出资产'),
    format: String(asset.format ?? 'json'),
    size: asset.file_written ? '已生成' : '占位结构',
    state: 'ready' as const,
  }));
}

function skillTitle(skillName: string): string {
  return getStageDisplayInfoBySkill(skillName).title;
}

function getStageDisplayInfo(stage: ProductionStage): StageDisplayInfo {
  return getStageDisplayInfoBySkill(stage.skill, stage.label);
}

function getStageDisplayInfoBySkill(skillName: string, fallbackLabel?: string): StageDisplayInfo {
  const fallbackTitle = fallbackLabel ? englishStageLabelMap[fallbackLabel] : skillNameMap[skillName];
  return (
    stageDisplayMap[skillName] ?? {
      title: fallbackTitle ?? '生产步骤',
      shortTitle: fallbackTitle ?? '步骤',
      output: '生成结构化中间结果，供后续流程继续使用。',
      check: ['确认本步结果符合预期。', '确认没有出现阻断后续流程的问题。'],
      approveNext: '继续进入下一步。',
      rejectNext: '退回当前步骤，按备注重新处理。',
    }
  );
}

function formatTaskStatus(status: GenerationTaskRecordStatus): string {
  const statusMap: Record<GenerationTaskRecordStatus, string> = {
    waiting: '排队中',
    running: '生成中',
    review: '等待确认',
    completed: '已完成',
    failed: '已退回',
  };

  return statusMap[status];
}

function formatReviewChip(stage: ProductionStage): string {
  if (stage.status === 'review') {
    return '待你确认';
  }

  if (stage.status === 'done') {
    return '已确认';
  }

  if (stage.status === 'failed' || stage.status === 'blocked') {
    return '已退回';
  }

  return '需确认';
}

function formatStageDuration(duration: string): string {
  return duration === '--' ? '待生成' : `预计 ${duration}`;
}

function buildJobControlHint(jobStatus: string | null, isCheckpointReady: boolean): string {
  if (isCheckpointReady) {
    return '当前任务已暂停在人工审核点：通过会自动继续生成；退回修改需要先填写备注，退回后可重试失败项。';
  }

  if (jobStatus === 'running') {
    return '任务正在生成，可暂停等待人工处理。';
  }

  if (jobStatus === 'paused') {
    return '任务已手动暂停，可恢复继续。';
  }

  if (jobStatus === 'failed') {
    return '任务存在失败项，可重试失败步骤或重新生成。';
  }

  return '开始生成后，这里会显示可用的任务操作。';
}

function formatProviderLabel(value: string): string {
  const normalized = value.toLowerCase();
  if (normalized === 'local' || normalized.includes('mock') || normalized.includes('deterministic')) {
    return '本地确定性';
  }

  if (value === '$0' || value === '0') {
    return '本地测试';
  }

  return value || '本地处理';
}

function formatCostAmount(cost: CostLedgerRecord): string {
  const amount = cost.actualCost ?? cost.estimatedCost;
  if (amount <= 0) {
    return '本地测试，无真实计费';
  }

  return `${amount} ${cost.unit}`;
}

function formatFieldName(field: string): string {
  const normalized = field.split('-').join('_');
  return fieldNameMap[field] ?? fieldNameMap[normalized] ?? field.split('_').join(' ');
}

function formatQualityStatus(value: unknown): string {
  const status = String(value ?? 'unknown');
  const statusMap: Record<string, string> = {
    passed: '通过',
    pass: '通过',
    ok: '正常',
    warning: '有提醒',
    blocked: '有阻断',
    failed: '失败',
    unknown: '未知',
  };

  return statusMap[status] ?? status;
}

function localizeProgressMessage(message: string): string {
  let next = message;
  Object.entries(englishStageLabelMap).forEach(([english, chinese]) => {
    next = next.split(english).join(chinese);
  });

  return next
    .split('checkpoint')
    .join('审核')
    .replace('已启动 job', '已启动任务')
    .replace('job ', '任务 ');
}

function resultKindForSkill(skillName: string): ResultCard['kind'] {
  if (skillName.includes('story') || skillName.includes('episode') || skillName.includes('plot')) {
    return 'script';
  }

  if (skillName.includes('character')) {
    return 'character';
  }

  if (skillName.includes('style')) {
    return 'style';
  }

  if (skillName.includes('board')) {
    return 'storyboard';
  }

  if (skillName.includes('export')) {
    return 'delivery';
  }

  return 'media';
}

function formatFileSize(value: number): string {
  if (value <= 0) {
    return 'DB 索引';
  }

  if (value < 1024) {
    return `${value} B`;
  }

  return `${Math.round(value / 1024)} KB`;
}

interface SkillEnvelope {
  ok: boolean;
  skill_name: string;
  schema_version: string;
  data: Record<string, any> | null;
  error: { code?: string; message?: string; details?: unknown } | null;
  runtime?: Record<string, unknown>;
}

const skillNameMap: Record<string, string> = {
  story_intake: '故事解析',
  plot_adaptation: '短剧改编',
  episode_writer: '脚本审核',
  character_bible: '角色设定',
  style_bible: '画风设定',
  storyboard_director: '分镜审核',
  image_prompt_builder: '图片提示词',
  image_generation: '图片资产审核',
  video_prompt_builder: '视频提示词',
  video_generation: '视频片段审核',
  voice_casting: '配音任务',
  subtitle_generator: '字幕结构',
  auto_editor: '粗剪计划',
  quality_checker: '质量检查',
  export_packager: '导出占位包',
};

function applyProductionEvent(
  event: ProductionJobEvent,
  setJob: Dispatch<SetStateAction<ProductionJob | null>>,
  setStages: Dispatch<SetStateAction<ProductionStage[]>>,
) {
  setJob((current) =>
          current
            ? {
                ...current,
                currentStage: event.stageId,
                progress: event.progress,
                status: event.jobStatus,
                finishedAt: event.jobStatus === 'completed' ? event.occurredAt : current.finishedAt,
              }
            : current,
  );

  setStages((current) => {
    if (event.stageId === 'delivery') {
      return current.map((stage) => ({ ...stage, status: 'done' }));
    }

    const activeIndex = current.findIndex((stage) => stage.id === event.stageId);

    if (activeIndex < 0) {
      return current;
    }

    return current.map((stage, index) => {
      if (index < activeIndex) {
        return { ...stage, status: 'done' };
      }

      if (index === activeIndex) {
        return { ...stage, status: event.status };
      }

      return { ...stage, status: 'waiting' };
    });
  });
}

function toProjectDetail(projectId: string): ProjectDetail {
  const project = mockProjects.find((item) => item.id === projectId) ?? mockProjects[0];

  return {
    ...project,
    stylePreset: '轻写实国漫',
    storyText: fallbackStoryText,
  };
}

function toProjectDraft(project: ProjectDetail): ProjectDraft {
  return {
    title: project.title,
    storyText: project.storyText,
    mode: project.mode,
    targetDuration: project.targetDuration,
    aspectRatio: project.aspectRatio,
    stylePreset: project.stylePreset,
  };
}

function validateProjectDraft(draft: ProjectDraft): string | null {
  if (!draft.title.trim()) {
    return '标题不能为空。';
  }

  if (!draft.stylePreset.trim()) {
    return '风格不能为空。';
  }

  const storyLength = countStoryCharacters(draft.storyText);
  if (storyLength < 500 || storyLength > 2000) {
    return `故事正文需保持在 500 到 2000 个非空白字符之间，当前 ${storyLength}。`;
  }

  if (![30, 45, 60].includes(draft.targetDuration)) {
    return '目标时长只能选择 30、45 或 60 秒。';
  }

  if (!['9:16', '16:9', '1:1'].includes(draft.aspectRatio)) {
    return '画幅只能选择 9:16、16:9 或 1:1。';
  }

  return null;
}

function countStoryCharacters(storyText: string): number {
  return Array.from(storyText).filter((character) => !/\s/.test(character)).length;
}

const resultIcons = {
  script: FileText,
  character: Film,
  style: SlidersHorizontal,
  storyboard: Image,
  media: Video,
  delivery: Download,
};

const commandMessages = {
  pause: '生产任务已暂停。',
  resume: '生产任务已恢复。',
  retry: '失败任务已重新排队。',
  'checkpoint-approve': '已通过当前审核，任务会继续进入下一步。',
  'checkpoint-reject': '已退回当前审核，任务已暂停在可重试状态。',
};

const fallbackStoryText =
  '雨夜里，林溪在旧巷口捡到一只会发光的纸鹤。纸鹤飞得很慢，像在等她跟上。它穿过挂满旧招牌的巷子，落在一间废弃照相馆门口。林溪的哥哥三年前在这里失踪，警方只找到一卷被雨水泡坏的胶片。她推门进去，发现暗房里亮着微弱红光，墙上贴满哥哥拍下的陌生人影。纸鹤钻进显影盘，胶片上忽然浮出一行字：不要相信明天早上的自己。林溪以为这是恶作剧，却在玻璃柜里看到一张刚冲洗好的照片，照片中的她站在同一间暗房，手里拿着哥哥的相机，身后有一道没有脸的影子。她开始按照纸鹤留下的光点寻找线索，每一步都揭开一段被人刻意删除的记忆。最后她发现哥哥并不是失踪，而是被困在照片之间的时间缝隙里，只有在天亮前拍下真正凶手的脸，才能把他带回现实。林溪带着相机回到巷口，发现所有招牌都变成了哥哥当年拍过的日期。纸鹤停在钟楼下，翅膀上浮出一串倒计时。她必须在雨停之前找到照片里那道影子的主人，否则第二天醒来，她会忘记哥哥，也会忘记自己为什么来到这里。她最终选择把镜头对准橱窗里的倒影，才看见凶手一直跟在她身后，披着哥哥的雨衣，脸却和未来的她一模一样。快门按下时，整条巷子的灯同时熄灭，纸鹤化成一束光钻进相机，哥哥的声音从暗房深处传来：别回头，把我带出去。';

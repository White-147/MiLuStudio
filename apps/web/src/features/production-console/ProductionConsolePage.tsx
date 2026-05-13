import {
  ArrowLeft,
  CheckCircle2,
  Download,
  FileText,
  Film,
  FolderOpen,
  Image,
  Lock,
  Pause,
  Play,
  RotateCcw,
  Send,
  SlidersHorizontal,
  Video,
} from 'lucide-react';
import type { Dispatch, SetStateAction } from 'react';
import { useEffect, useMemo, useState } from 'react';
import {
  approveProductionCheckpoint,
  getProject,
  pauseProductionJob,
  resumeProductionJob,
  retryProductionJob,
  startProductionJob,
  watchProductionJob,
} from '../../shared/api/controlPlaneClient';
import { mockDeliveryAssets, mockProjects, mockResultCards, mockStages } from '../../shared/mock/studioMock';
import type { ProjectDetail, ProjectMode, ProductionJob, ProductionJobEvent, ProductionStage, ResultCard } from '../../shared/types/production';

interface ProductionConsolePageProps {
  onBack: () => void;
  projectId: string;
}

type ApiState = 'loading' | 'control-api' | 'mock-fallback';

type JobCommand = 'pause' | 'resume' | 'retry' | 'checkpoint';

export function ProductionConsolePage({ onBack, projectId }: ProductionConsolePageProps) {
  const fallbackProject = useMemo(() => toProjectDetail(projectId), [projectId]);
  const [project, setProject] = useState<ProjectDetail>(fallbackProject);
  const [mode, setMode] = useState<ProjectMode>(fallbackProject.mode);
  const [running, setRunning] = useState(false);
  const [starting, setStarting] = useState(false);
  const [activeIndex, setActiveIndex] = useState(2);
  const [apiState, setApiState] = useState<ApiState>('loading');
  const [job, setJob] = useState<ProductionJob | null>(null);
  const [stages, setStages] = useState<ProductionStage[]>(mockStages);
  const [streamJobId, setStreamJobId] = useState<string | null>(null);
  const [syncMessage, setSyncMessage] = useState('正在连接 Control API');
  const [actioning, setActioning] = useState<JobCommand | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    setApiState('loading');
    setSyncMessage('正在连接 Control API');

    getProject(projectId, controller.signal)
      .then((nextProject) => {
        setProject(nextProject);
        setMode(nextProject.mode);
        setApiState('control-api');
        setSyncMessage('Control API 已连接');
      })
      .catch(() => {
        setProject(fallbackProject);
        setMode(fallbackProject.mode);
        setApiState('mock-fallback');
        setSyncMessage('Control API 未启动，当前使用前端 mock 数据');
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

        if (event.jobStatus === 'completed' || event.jobStatus === 'failed') {
          setRunning(false);
          setStreamJobId(null);
        }
      },
      () => {
        setSyncMessage('SSE 连接已结束或等待下一次 mock 事件');
      },
    );
  }, [streamJobId]);

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

  const startJob = async () => {
    setStarting(true);

    try {
      const nextJob = await startProductionJob(project.id);
      setJob(nextJob);
      setStages(nextJob.stages);
      setRunning(true);
      setApiState('control-api');
      setStreamJobId(nextJob.id);
      setSyncMessage(`Control API 已启动 job ${nextJob.id}`);
    } catch {
      setRunning(true);
      setApiState('mock-fallback');
      setJob(null);
      setStages(mockStages);
      setSyncMessage('Control API 未响应，已切回前端 mock 进度');
    } finally {
      setStarting(false);
    }
  };

  const runJobCommand = async (command: JobCommand) => {
    if (!job || apiState !== 'control-api') {
      return;
    }

    setActioning(command);

    try {
      const commandMap = {
        pause: pauseProductionJob,
        resume: resumeProductionJob,
        retry: retryProductionJob,
        checkpoint: approveProductionCheckpoint,
      };
      const nextJob = await commandMap[command](job.id);

      setJob(nextJob);
      setStages(nextJob.stages);
      setRunning(nextJob.status === 'running' || nextJob.status === 'paused');

      if ((command === 'retry' || command === 'resume') && nextJob.status === 'running') {
        setStreamJobId(nextJob.id);
      }

      setSyncMessage(commandMessages[command]);
    } catch {
      setSyncMessage('Control API 暂时没有接受该操作，请稍后重试。');
    } finally {
      setActioning(null);
    }
  };

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
            {apiState === 'control-api' ? 'Control API' : apiState === 'loading' ? '连接中' : 'Mock fallback'}
          </span>
          <button className="secondary-button" type="button">
            <FolderOpen size={17} />
            <span>输出目录</span>
          </button>
        </div>
      </header>

      <div className="console-layout">
        <ConversationPanel
          mode={mode}
          onModeChange={setMode}
          onStart={startJob}
          project={project}
          running={running}
          starting={starting}
        />
        <ProgressPanel
          actioning={actioning}
          canUseActions={apiState === 'control-api'}
          job={job}
          onApproveCheckpoint={() => runJobCommand('checkpoint')}
          onPause={() => runJobCommand('pause')}
          onResume={() => runJobCommand('resume')}
          onRetry={() => runJobCommand('retry')}
          stages={visibleStages}
          syncMessage={syncMessage}
        />
        <ResultsPanel jobCompleted={job?.status === 'completed'} />
      </div>
    </section>
  );
}

interface ConversationPanelProps {
  mode: ProjectMode;
  onModeChange: (mode: ProjectMode) => void;
  onStart: () => void;
  project: ProjectDetail;
  running: boolean;
  starting: boolean;
}

function ConversationPanel({ mode, onModeChange, onStart, project, running, starting }: ConversationPanelProps) {
  return (
    <section className="workspace-panel input-panel" aria-label="对话输入区">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">输入</p>
          <h2>故事入口</h2>
        </div>
        <SlidersHorizontal size={18} />
      </div>

      <textarea defaultValue={project.storyText} key={project.id} aria-label="故事或创作要求" />

      <div className="segmented-control" aria-label="生产模式">
        <button className={mode === 'fast' ? 'selected' : ''} onClick={() => onModeChange('fast')} type="button">
          极速模式
        </button>
        <button className={mode === 'director' ? 'selected' : ''} onClick={() => onModeChange('director')} type="button">
          导演模式
        </button>
      </div>

      <div className="field-grid">
        <label>
          <span>目标时长</span>
          <select defaultValue={String(project.targetDuration)} key={`${project.id}-duration`}>
            <option value="30">30 秒</option>
            <option value="45">45 秒</option>
            <option value="60">60 秒</option>
          </select>
        </label>
        <label>
          <span>画幅</span>
          <select defaultValue={project.aspectRatio} key={`${project.id}-ratio`}>
            <option value="9:16">9:16 竖屏</option>
            <option value="16:9">16:9 横屏</option>
            <option value="1:1">1:1 方屏</option>
          </select>
        </label>
      </div>

      <button className="primary-button full-width" disabled={starting} onClick={onStart} type="button">
        {running ? <RotateCcw size={18} /> : <Send size={18} />}
        <span>{starting ? '启动中' : running ? '重新监听进度' : '开始生成'}</span>
      </button>
    </section>
  );
}

function ProgressPanel({
  actioning,
  canUseActions,
  job,
  onApproveCheckpoint,
  onPause,
  onResume,
  onRetry,
  stages,
  syncMessage,
}: {
  actioning: JobCommand | null;
  canUseActions: boolean;
  job: ProductionJob | null;
  onApproveCheckpoint: () => void;
  onPause: () => void;
  onResume: () => void;
  onRetry: () => void;
  stages: ProductionStage[];
  syncMessage: string;
}) {
  const activeStage = stages.find((stage) => stage.status === 'running' || stage.status === 'review');
  const isCheckpointReady = Boolean(activeStage?.needsReview && activeStage.status === 'review');
  const canPause = canUseActions && job?.status === 'running';
  const canResume = canUseActions && job?.status === 'paused' && !isCheckpointReady;
  const canCheckpoint = canUseActions && job?.status === 'paused' && isCheckpointReady;
  const canRetry = canUseActions && (job?.status === 'failed' || stages.some((stage) => stage.status === 'blocked'));

  return (
    <section className="workspace-panel progress-panel" aria-label="任务进度流">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">进度</p>
          <h2>{activeStage ? activeStage.label : job?.status === 'completed' ? '已完成' : '等待开始'}</h2>
        </div>
        <span className={job?.status === 'running' ? 'live-dot active' : 'live-dot'} />
      </div>

      <div className="job-summary">
        <span>{job ? `job ${job.id.slice(0, 12)}` : '尚未创建 job'}</span>
        <strong>{job ? `${job.progress}%` : '0%'}</strong>
      </div>

      <div className="job-controls" aria-label="生产任务操作">
        <button className="command-button" disabled={!canPause || actioning !== null} onClick={onPause} type="button">
          <Pause size={15} />
          <span>{actioning === 'pause' ? '暂停中' : '暂停'}</span>
        </button>
        <button className="command-button" disabled={!canResume || actioning !== null} onClick={onResume} type="button">
          <Play size={15} />
          <span>{actioning === 'resume' ? '恢复中' : '恢复'}</span>
        </button>
        <button className="command-button confirm" disabled={!canCheckpoint || actioning !== null} onClick={onApproveCheckpoint} type="button">
          <CheckCircle2 size={15} />
          <span>{actioning === 'checkpoint' ? '确认中' : '确认节点'}</span>
        </button>
        <button className="command-button" disabled={!canRetry || actioning !== null} onClick={onRetry} type="button">
          <RotateCcw size={15} />
          <span>{actioning === 'retry' ? '重试中' : '重试失败项'}</span>
        </button>
      </div>

      <p className="sync-message">{syncMessage}</p>

      <div className="stage-list">
        {stages.map((stage) => (
          <article className={`stage-row ${stage.status}`} key={stage.id}>
            <span className="stage-marker" />
            <div>
              <strong>{stage.label}</strong>
              <span>{stage.skill}</span>
            </div>
            <div className="stage-meta">
              <span>{stage.duration}</span>
              <span>{stage.cost}</span>
            </div>
            {stage.needsReview && <span className="review-chip">需确认</span>}
          </article>
        ))}
      </div>
    </section>
  );
}

function ResultsPanel({ jobCompleted }: { jobCompleted: boolean }) {
  return (
    <section className="results-column" aria-label="中间结果和最终交付">
      <div className="result-stack">
        {mockResultCards.map((card) => (
          <ResultCardView card={card} key={card.id} />
        ))}
      </div>
      <DeliveryPanel jobCompleted={jobCompleted} />
    </section>
  );
}

function ResultCardView({ card }: { card: ResultCard }) {
  const Icon = resultIcons[card.kind];

  return (
    <article className="result-card">
      <div className="result-card-heading">
        <span className="result-icon">
          <Icon size={17} />
        </span>
        <div>
          <h3>{card.title}</h3>
          <span>{card.status === 'locked' ? '已锁定' : card.status === 'ready' ? '可确认' : '草稿'}</span>
        </div>
      </div>
      <p>{card.summary}</p>
      <ul>
        {card.details.map((detail) => (
          <li key={detail}>{detail}</li>
        ))}
      </ul>
      <div className="card-actions">
        <button type="button">
          <Lock size={15} />
          <span>锁定</span>
        </button>
        <button type="button">
          <RotateCcw size={15} />
          <span>重生成</span>
        </button>
      </div>
    </article>
  );
}

function DeliveryPanel({ jobCompleted }: { jobCompleted: boolean }) {
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
        {mockDeliveryAssets.map((asset) => {
          const state = jobCompleted ? 'ready' : asset.state;

          return (
            <div className="asset-row" key={asset.id}>
              <div>
                <strong>{asset.label}</strong>
                <span>{asset.format}</span>
              </div>
              <span>{state === 'ready' ? asset.size : '等待生成'}</span>
            </div>
          );
        })}
      </div>
    </section>
  );
}

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
    storyText: '雨夜里，林溪在旧巷口捡到一只会发光的纸鹤。纸鹤不断飞向废弃照相馆，那里藏着哥哥失踪前留下的最后一卷胶片。',
  };
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
  checkpoint: '节点已确认，状态机将继续推进。',
};

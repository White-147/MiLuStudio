import { Clock, Film, Plus, Ratio, Sparkles } from 'lucide-react';
import { useEffect, useState } from 'react';
import { createProject, listProjects } from '../../shared/api/controlPlaneClient';
import { mockProjects } from '../../shared/mock/studioMock';
import type { ProjectSummary } from '../../shared/types/production';

interface ProjectListPageProps {
  onOpenProject: (projectId: string) => void;
}

export function ProjectListPage({ onOpenProject }: ProjectListPageProps) {
  const [projects, setProjects] = useState<ProjectSummary[]>(mockProjects);
  const [apiMode, setApiMode] = useState<'loading' | 'control-api' | 'mock-fallback'>('loading');
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    listProjects(controller.signal)
      .then((nextProjects) => {
        setProjects(nextProjects);
        setApiMode('control-api');
      })
      .catch(() => {
        setProjects(mockProjects);
        setApiMode('mock-fallback');
      });

    return () => controller.abort();
  }, []);

  const createAndOpenProject = async () => {
    setCreating(true);

    try {
      const project = await createProject();
      setProjects((current) => [project, ...current.filter((item) => item.id !== project.id)]);
      setApiMode('control-api');
      onOpenProject(project.id);
    } catch {
      setApiMode('mock-fallback');
      onOpenProject(mockProjects[0].id);
    } finally {
      setCreating(false);
    }
  };

  return (
    <section className="project-list-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Windows 原生 AI 漫剧 Agent</p>
          <h1>项目</h1>
        </div>
        <button className="primary-button" disabled={creating} onClick={createAndOpenProject} type="button">
          <Plus size={18} />
          <span>{creating ? '创建中' : '新建漫剧'}</span>
        </button>
      </header>

      <div className="project-toolbar" aria-label="生产摘要">
        <div>
          <strong>{projects.length}</strong>
          <span>本地项目</span>
        </div>
        <div>
          <strong>30-60s</strong>
          <span>MVP 时长</span>
        </div>
        <div>
          <strong>9:16</strong>
          <span>默认画幅</span>
        </div>
        <div>
          <strong>{apiMode === 'control-api' ? 'API' : apiMode === 'loading' ? '连接中' : 'Mock'}</strong>
          <span>数据来源</span>
        </div>
      </div>

      <div className="project-grid">
        {projects.map((project) => (
          <button className="project-card" key={project.id} onClick={() => onOpenProject(project.id)} type="button">
            <span className={`status-pill ${project.status}`}>{statusLabel[project.status]}</span>
            <h2>{project.title}</h2>
            <p>{project.description}</p>
            <div className="project-meta">
              <span>
                <Sparkles size={15} />
                {project.mode === 'fast' ? '极速模式' : '导演模式'}
              </span>
              <span>
                <Clock size={15} />
                {project.targetDuration}s
              </span>
              <span>
                <Ratio size={15} />
                {project.aspectRatio}
              </span>
            </div>
            <div className="progress-track" aria-label={`${project.title} 进度 ${project.progress}%`}>
              <span style={{ width: `${project.progress}%` }} />
            </div>
            <div className="card-footer">
              <span>{project.updatedAt}</span>
              <Film size={17} />
            </div>
          </button>
        ))}
      </div>
    </section>
  );
}

const statusLabel = {
  draft: '草稿',
  running: '生产中',
  paused: '已暂停',
  completed: '已完成',
  failed: '失败',
};

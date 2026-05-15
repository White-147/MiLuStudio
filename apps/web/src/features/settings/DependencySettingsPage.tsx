import { AlertTriangle, CheckCircle2, Database, Folder, Loader2, PackageCheck, RefreshCcw } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { getSystemDependencies } from '../../shared/api/controlPlaneClient';
import type { SystemDependenciesReport, SystemDependencyCheck, SystemDependencyStatus } from '../../shared/types/production';

const dependencyLabels: Record<string, string> = {
  repository_provider: '持久化',
  database_file: 'SQLite 文件',
  database_reachable: 'SQLite 连接',
  sqlite_schema: 'SQLite Schema',
  storage_root: 'Storage',
  uploads_root: 'Uploads',
  ffmpeg_runtime: 'FFmpeg',
  ocr_runtime: 'OCR',
  python_runtime: 'Python',
  python_skills_root: 'Python Skills',
};

const strategyLabels: Record<string, string> = {
  bundled_or_offline_runtime: '随包 / 离线包优先',
  auxiliary_only: '辅助',
  'Control API': 'Control API',
};

const statusLabels: Record<SystemDependencyStatus, string> = {
  ok: '正常',
  warning: '注意',
  error: '异常',
  skipped: '跳过',
};

export function DependencySettingsPage() {
  const [report, setReport] = useState<SystemDependenciesReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('正在检查本地依赖');

  useEffect(() => {
    const controller = new AbortController();
    void loadDependencies(controller.signal);
    return () => controller.abort();
  }, []);

  const counts = useMemo(() => {
    const dependencies = report?.dependencies ?? [];
    return {
      ok: dependencies.filter((item) => item.status === 'ok').length,
      warning: dependencies.filter((item) => item.status === 'warning').length,
      error: dependencies.filter((item) => item.status === 'error').length,
      total: dependencies.length,
    };
  }, [report]);

  const loadDependencies = async (signal?: AbortSignal) => {
    setLoading(true);
    try {
      const nextReport = await getSystemDependencies(signal);
      setReport(nextReport);
      setMessage(nextReport.status === 'ready' ? '依赖检查通过' : '依赖检查需要处理');
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '依赖检查失败');
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="provider-settings-view dependency-settings-view">
      <div className="provider-settings-heading">
        <div>
          <p className="eyebrow">本地运行</p>
          <h1>依赖</h1>
        </div>
        <div className="provider-settings-actions">
          <span className={report?.status === 'ready' ? 'api-chip connected' : 'api-chip'}>
            {report?.status === 'ready' ? '就绪' : '需处理'}
          </span>
          <button
            className="secondary-button"
            disabled={loading}
            onClick={() => void loadDependencies()}
            type="button"
          >
            {loading ? <Loader2 className="spin" size={17} /> : <RefreshCcw size={17} />}
            <span>刷新</span>
          </button>
        </div>
      </div>

      <p className={report?.status === 'ready' ? 'diagnostics-message ok' : 'diagnostics-message'}>{message}</p>

      <div className="provider-summary-strip dependency-summary-strip">
        <div>
          <span>持久化</span>
          <strong>{report?.repositoryProvider ?? '-'}</strong>
        </div>
        <div>
          <span>正常</span>
          <strong>{counts.ok}/{counts.total}</strong>
        </div>
        <div>
          <span>注意</span>
          <strong>{counts.warning}</strong>
        </div>
        <div>
          <span>异常</span>
          <strong>{counts.error}</strong>
        </div>
      </div>

      <div className="dependency-settings-layout">
        <div className="preflight-list">
          {(report?.dependencies ?? []).map((dependency) => (
            <DependencyRow dependency={dependency} key={dependency.id} />
          ))}
          {!loading && (report?.dependencies.length ?? 0) === 0 && (
            <div className="preflight-row">
              <div>
                <strong>暂无依赖结果</strong>
                <p>Control API 暂未返回依赖检查项。</p>
              </div>
              <span className="status-pill skipped">跳过</span>
            </div>
          )}
        </div>

        <aside className="provider-side-panel">
          <div className="provider-settings-panel">
            <div className="provider-panel-title">
              <div>
                <h2>安装策略</h2>
                <p>{formatStrategy(report?.installStrategy.preferred)}</p>
              </div>
              <span className="result-icon">
                <PackageCheck size={17} />
              </span>
            </div>
            <dl className="dependency-meta-list">
              <div>
                <dt>在线下载</dt>
                <dd>{formatStrategy(report?.installStrategy.onlineDownload)}</dd>
              </div>
              <div>
                <dt>管理边界</dt>
                <dd>{formatStrategy(report?.installStrategy.managedBy)}</dd>
              </div>
            </dl>
          </div>

          <div className="provider-settings-panel recommendation-list">
            <div className="provider-panel-title">
              <div>
                <h2>建议</h2>
                <p>{report?.recommendations.length ? '待处理项' : '当前无建议'}</p>
              </div>
              <span className="result-icon">
                <AlertTriangle size={17} />
              </span>
            </div>
            {report?.recommendations.length ? (
              <ul>
                {report.recommendations.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            ) : (
              <p>依赖检测未返回修复建议。</p>
            )}
          </div>
        </aside>
      </div>
    </section>
  );
}

function DependencyRow({ dependency }: { dependency: SystemDependencyCheck }) {
  const details = Object.entries(dependency.details ?? {});

  return (
    <div className="preflight-row dependency-row">
      <div className="dependency-row-main">
        <span className="result-icon">
          {getDependencyIcon(dependency.id)}
        </span>
        <div>
          <strong>{dependencyLabels[dependency.id] ?? dependency.id}</strong>
          <p>{dependency.message}</p>
          {details.length > 0 && (
            <dl className="dependency-detail-list">
              {details.map(([key, value]) => (
                <div key={key}>
                  <dt>{key}</dt>
                  <dd>{value}</dd>
                </div>
              ))}
            </dl>
          )}
        </div>
      </div>
      <span className={`status-pill ${dependency.status}`}>{statusLabels[dependency.status] ?? dependency.status}</span>
    </div>
  );
}

function getDependencyIcon(id: string) {
  if (id.includes('database') || id.includes('sqlite') || id.includes('repository')) {
    return <Database size={17} />;
  }

  if (id.includes('root') || id.includes('uploads') || id.includes('storage')) {
    return <Folder size={17} />;
  }

  if (id.includes('runtime') || id.includes('skills')) {
    return <PackageCheck size={17} />;
  }

  return <CheckCircle2 size={17} />;
}

function formatStrategy(value?: string) {
  if (!value) {
    return '-';
  }

  return strategyLabels[value] ?? value;
}

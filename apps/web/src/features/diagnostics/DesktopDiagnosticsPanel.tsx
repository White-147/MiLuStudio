import { Activity, FolderOpen, RefreshCcw, Server, ShieldCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import type { DesktopCommandResult, DesktopRuntimeStatus } from '../../shared/types/desktop';

type CommandState = 'idle' | 'running';

export function DesktopDiagnosticsPanel() {
  const [status, setStatus] = useState<DesktopRuntimeStatus | null>(null);
  const [message, setMessage] = useState('桌面宿主状态等待刷新');
  const [commandState, setCommandState] = useState<CommandState>('idle');
  const desktop = typeof window !== 'undefined' ? window.miluDesktop : undefined;

  useEffect(() => {
    if (!desktop) {
      setMessage('当前不是 Electron 桌面宿主环境');
      return;
    }

    let cancelled = false;
    const refresh = async () => {
      const nextStatus = await desktop.getStatus();
      if (!cancelled) {
        setStatus(nextStatus);
        setMessage(nextStatus.preflight?.healthy ? 'Control API preflight 通过' : 'Control API preflight 需要处理');
      }
    };

    void refresh();
    const timer = window.setInterval(() => void refresh(), 5000);

    return () => {
      cancelled = true;
      window.clearInterval(timer);
    };
  }, [desktop]);

  const runCommand = async (command: () => Promise<DesktopRuntimeStatus | DesktopCommandResult>, fallback: string) => {
    setCommandState('running');
    try {
      const result = await command();
      if ('checkedAt' in result) {
        setStatus(result);
        setMessage(result.preflight?.healthy ? '本地服务已重启' : '本地服务已重启，preflight 仍需处理');
      } else {
        setMessage(result.message || fallback);
      }
    } catch (error) {
      setMessage(error instanceof Error ? error.message : fallback);
    } finally {
      setCommandState('idle');
    }
  };

  const services = status ? Object.values(status.services) : [];
  const checks = status?.preflight?.checks ?? [];

  return (
    <section className="diagnostics-view">
      <div className="diagnostics-heading">
        <div>
          <p className="eyebrow">诊断</p>
          <h1>桌面宿主与后端状态</h1>
        </div>
        <ShieldCheck size={24} />
      </div>

      <div className="diagnostics-toolbar">
        <button
          className="secondary-button"
          disabled={!desktop || commandState === 'running'}
          onClick={() => desktop && void runCommand(desktop.restartServices, '本地服务重启失败')}
          type="button"
        >
          <RefreshCcw size={17} />
          <span>重启服务</span>
        </button>
        <button
          className="secondary-button"
          disabled={!desktop || commandState === 'running'}
          onClick={() => desktop && void runCommand(desktop.openLogs, '无法打开日志目录')}
          type="button"
        >
          <FolderOpen size={17} />
          <span>日志</span>
        </button>
        <button
          className="secondary-button"
          disabled={!desktop || commandState === 'running'}
          onClick={() => desktop && void runCommand(desktop.openOutputDirectory, '无法打开输出目录')}
          type="button"
        >
          <FolderOpen size={17} />
          <span>输出目录</span>
        </button>
      </div>

      <p className={status?.preflight?.healthy ? 'diagnostics-message ok' : 'diagnostics-message'}>
        {message}
      </p>

      <div className="diagnostics-grid">
        {services.map((service) => (
          <article className="diagnostic-card" key={service.name}>
            <div className="diagnostic-card-title">
              <Server size={17} />
              <strong>{service.name}</strong>
              <span className={`status-pill ${service.status}`}>{service.status}</span>
            </div>
            <p>{service.message}</p>
            <small>{service.pid ? `PID ${service.pid}` : service.port ? `Port ${service.port}` : '等待本地宿主'}</small>
          </article>
        ))}
      </div>

      <div className="preflight-list">
        {checks.map((check) => (
          <article className="preflight-row" key={check.name}>
            <Activity size={16} />
            <div>
              <strong>{check.name}</strong>
              <p>{check.message}</p>
            </div>
            <span className={`status-pill ${check.status}`}>{check.status}</span>
          </article>
        ))}
      </div>

      {status?.preflight?.recommendations.length ? (
        <div className="recommendation-list">
          {status.preflight.recommendations.map((recommendation) => (
            <p key={recommendation}>{recommendation}</p>
          ))}
        </div>
      ) : null}
    </section>
  );
}

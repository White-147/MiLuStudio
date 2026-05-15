import { useCallback, useEffect, useState } from 'react';
import { RefreshCw } from 'lucide-react';
import { AuthGate } from '../features/auth/AuthGate';
import { StudioWorkspacePage } from '../features/workspace/StudioWorkspacePage';
import { getAuthState, getControlApiBaseUrl, logout } from '../shared/api/controlPlaneClient';
import type { AuthState } from '../shared/types/production';

export function App() {
  const [authState, setAuthState] = useState<AuthState | null>(null);
  const [authReady, setAuthReady] = useState(false);

  const checkAuthState = useCallback(() => {
    const controller = new AbortController();
    setAuthReady(false);

    getAuthState(controller.signal)
      .then((nextState) => setAuthState(nextState))
      .catch(() => setAuthState(buildSignedOutState('Control API 未连接，请先启动本地服务。', 'control_api_unavailable')))
      .finally(() => setAuthReady(true));

    return controller;
  }, []);

  useEffect(() => {
    const controller = checkAuthState();
    return () => controller.abort();
  }, [checkAuthState]);

  const signOut = async () => {
    await logout();
    setAuthState(buildSignedOutState('已退出当前账号。', 'not_authenticated'));
  };

  if (!authReady) {
    return (
      <main className="auth-shell">
        <section className="auth-panel">
          <div className="auth-brand">
            <img alt="" className="brand-logo large" src="/brand/logo.png" />
            <div>
              <p className="eyebrow">MiLuStudio</p>
              <h1>正在检查账号</h1>
            </div>
          </div>
        </section>
      </main>
    );
  }

  if (!authState?.authenticated) {
    if (authState?.errorCode === 'control_api_unavailable') {
      return <ControlApiUnavailableGate onRetry={checkAuthState} />;
    }

    return <AuthGate initialState={authState} onAuthorized={setAuthState} />;
  }

  return <StudioWorkspacePage authState={authState} onSignOut={signOut} />;
}

interface ControlApiUnavailableGateProps {
  onRetry: () => AbortController;
}

function ControlApiUnavailableGate({ onRetry }: ControlApiUnavailableGateProps) {
  return (
    <main className="auth-shell">
      <section className="auth-panel service-panel" aria-label="本地服务未连接">
        <div className="auth-brand">
          <img alt="" className="brand-logo large" src="/brand/logo.png" />
          <div>
            <p className="eyebrow">MiLuStudio</p>
            <h1>本地服务未连接</h1>
          </div>
        </div>

        <div className="service-status">
          <span className="status-pill warning">Control API</span>
          <p>Web 工作台需要后端本地服务在线后才能进入项目和生成流程。</p>
          <code>{getControlApiBaseUrl()}</code>
        </div>

        <div className="service-actions">
          <button
            className="primary-button full-width"
            type="button"
            onClick={() => {
              onRetry();
            }}
          >
            <RefreshCw size={17} aria-hidden="true" />
            重试连接
          </button>
        </div>

        <p className="auth-message">
          开发环境可从 apps/web 执行 npm run dev:local，或先执行 npm run services:start 再打开 Web。
        </p>
      </section>
    </main>
  );
}

function buildSignedOutState(message: string, errorCode: string): AuthState {
  return {
    account: null,
    authenticated: false,
    device: null,
    errorCode,
    license: {
      expiresAt: null,
      isActive: false,
      licenseType: 'none',
      maxDevices: 0,
      message,
      plan: 'local',
      startsAt: null,
      status: 'not_required',
    },
    message,
  };
}

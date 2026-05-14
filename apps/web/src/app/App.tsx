import { useEffect, useState } from 'react';
import { AuthGate } from '../features/auth/AuthGate';
import { StudioWorkspacePage } from '../features/workspace/StudioWorkspacePage';
import { getAuthState, logout } from '../shared/api/controlPlaneClient';
import type { AuthState } from '../shared/types/production';

export function App() {
  const [authState, setAuthState] = useState<AuthState | null>(null);
  const [authReady, setAuthReady] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    getAuthState(controller.signal)
      .then((nextState) => setAuthState(nextState))
      .catch(() => setAuthState(buildSignedOutState('Control API 未连接，请先启动本地服务。', 'control_api_unavailable')))
      .finally(() => setAuthReady(true));

    return () => controller.abort();
  }, []);

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
    return <AuthGate initialState={authState} onAuthorized={setAuthState} />;
  }

  return <StudioWorkspacePage authState={authState} onSignOut={signOut} />;
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

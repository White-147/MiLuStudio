import { LogIn, UserPlus } from 'lucide-react';
import { useMemo, useState } from 'react';
import { loginAccount, registerAccount } from '../../shared/api/controlPlaneClient';
import type { AuthSession, AuthState } from '../../shared/types/production';

interface AuthGateProps {
  initialState: AuthState | null;
  onAuthorized: (state: AuthState) => void;
}

type AuthMode = 'login' | 'register';

export function AuthGate({ initialState, onAuthorized }: AuthGateProps) {
  const [mode, setMode] = useState<AuthMode>('login');
  const [identifier, setIdentifier] = useState('');
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('MiLuStudio 用户');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState(initialState?.message ?? '请登录或注册后继续。');
  const device = useMemo(() => getLocalDevice(), []);

  const submitAuth = async () => {
    setBusy(true);
    setMessage(mode === 'login' ? '正在登录...' : '正在注册...');

    try {
      const session =
        mode === 'login'
          ? await loginAccount({
              identifier,
              password,
              deviceFingerprint: device.fingerprint,
              deviceName: device.name,
            })
          : await registerAccount({
              email,
              displayName,
              password,
              deviceFingerprint: device.fingerprint,
              deviceName: device.name,
            });

      const nextState = toAuthState(session);
      setMessage(nextState.message);
      onAuthorized(nextState);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '认证请求失败。');
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="auth-shell">
      <section className="auth-panel" aria-label="账号登录">
        <div className="auth-brand">
          <img alt="" className="brand-logo large" src="/brand/logo.png" />
          <div>
            <p className="eyebrow">MiLuStudio</p>
            <h1>{mode === 'login' ? '登录账号' : '注册账号'}</h1>
          </div>
        </div>

        <div className="segmented-control auth-switch" aria-label="认证模式">
          <button className={mode === 'login' ? 'selected' : ''} onClick={() => setMode('login')} type="button">
            <LogIn size={15} />
            <span>登录</span>
          </button>
          <button className={mode === 'register' ? 'selected' : ''} onClick={() => setMode('register')} type="button">
            <UserPlus size={15} />
            <span>注册</span>
          </button>
        </div>

        <div className="auth-form">
          {mode === 'login' ? (
            <label>
              <span>邮箱或手机号</span>
              <input
                autoComplete="username"
                value={identifier}
                onChange={(event) => setIdentifier(event.target.value)}
              />
            </label>
          ) : (
            <>
              <label>
                <span>邮箱</span>
                <input autoComplete="email" value={email} onChange={(event) => setEmail(event.target.value)} />
              </label>
              <label>
                <span>昵称</span>
                <input value={displayName} onChange={(event) => setDisplayName(event.target.value)} />
              </label>
            </>
          )}
          <label>
            <span>密码</span>
            <input
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>
          <button
            className="primary-button full-width"
            disabled={busy || !password.trim() || (mode === 'login' ? !identifier.trim() : !email.trim())}
            onClick={submitAuth}
            type="button"
          >
            {mode === 'login' ? <LogIn size={18} /> : <UserPlus size={18} />}
            <span>{busy ? '处理中' : mode === 'login' ? '登录' : '注册'}</span>
          </button>
        </div>

        <p className="auth-message">{message}</p>
        <div className="auth-device">
          <span>{device.name}</span>
          <span>{device.fingerprint.slice(0, 18)}</span>
        </div>
      </section>
    </main>
  );
}

function toAuthState(session: AuthSession): AuthState {
  return {
    authenticated: true,
    account: session.account,
    device: session.device,
    license: session.license,
    errorCode: null,
    message: '账号已登录。',
  };
}

function getLocalDevice() {
  const key = 'milu.device.fingerprint';
  const existing = window.localStorage.getItem(key);
  const fingerprint = existing ?? createDeviceFingerprint();
  if (!existing) {
    window.localStorage.setItem(key, fingerprint);
  }

  return {
    fingerprint,
    name: navigator.userAgent.includes('Electron') ? 'MiLuStudio Desktop' : 'MiLuStudio Web',
  };
}

function createDeviceFingerprint() {
  if (typeof crypto.randomUUID === 'function') {
    return `web-${crypto.randomUUID()}`;
  }

  return `web-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

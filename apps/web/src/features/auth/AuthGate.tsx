import { KeyRound, LogIn, LogOut, ShieldCheck, UserPlus } from 'lucide-react';
import { useMemo, useState } from 'react';
import {
  activateLicense,
  clearAuthSession,
  loginAccount,
  registerAccount,
} from '../../shared/api/controlPlaneClient';
import type { AuthSession, AuthState } from '../../shared/types/production';

interface AuthGateProps {
  initialState: AuthState | null;
  onAuthorized: (state: AuthState) => void;
}

type AuthMode = 'login' | 'register';

export function AuthGate({ initialState, onAuthorized }: AuthGateProps) {
  const [authState, setAuthState] = useState<AuthState | null>(initialState);
  const [mode, setMode] = useState<AuthMode>('login');
  const [identifier, setIdentifier] = useState('');
  const [email, setEmail] = useState('');
  const [displayName, setDisplayName] = useState('MiLuStudio 用户');
  const [password, setPassword] = useState('');
  const [activationCode, setActivationCode] = useState('');
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState(initialState?.message ?? '请登录或注册后继续。');
  const device = useMemo(() => getLocalDevice(), []);
  const needsActivation = Boolean(authState?.authenticated && !authState.license.isActive);

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
              activationCode: activationCode || undefined,
              deviceFingerprint: device.fingerprint,
              deviceName: device.name,
            });

      const nextState = toAuthState(session);
      setAuthState(nextState);
      setMessage(nextState.license.message);

      if (nextState.license.isActive) {
        onAuthorized(nextState);
      }
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '认证请求失败。');
    } finally {
      setBusy(false);
    }
  };

  const submitActivation = async () => {
    setBusy(true);
    setMessage('正在校验激活码...');

    try {
      const nextState = await activateLicense({
        activationCode,
        deviceFingerprint: device.fingerprint,
        deviceName: device.name,
      });
      setAuthState(nextState);
      setMessage(nextState.message);

      if (nextState.license.isActive) {
        onAuthorized(nextState);
      }
    } catch (error) {
      setMessage(error instanceof Error ? error.message : '激活失败。');
    } finally {
      setBusy(false);
    }
  };

  const signOut = () => {
    clearAuthSession();
    setAuthState(null);
    setMessage('已退出当前账号。');
  };

  return (
    <main className="auth-shell">
      <section className="auth-panel" aria-label="账号授权">
        <div className="auth-brand">
          <img alt="" className="brand-logo large" src="/brand/logo.png" />
          <div>
            <p className="eyebrow">MiLuStudio</p>
            <h1>{needsActivation ? '激活许可证' : mode === 'login' ? '登录账号' : '注册账号'}</h1>
          </div>
        </div>

        {!needsActivation && (
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
        )}

        {needsActivation ? (
          <div className="auth-form">
            <div className="license-state">
              <ShieldCheck size={20} />
              <div>
                <strong>{authState?.account?.displayName}</strong>
                <span>{authState?.license.message}</span>
              </div>
            </div>
            <label>
              <span>激活码</span>
              <input
                autoComplete="one-time-code"
                value={activationCode}
                onChange={(event) => setActivationCode(event.target.value)}
                placeholder="MILU-STAGE16-TEST"
              />
            </label>
            <button className="primary-button full-width" disabled={busy || !activationCode.trim()} onClick={submitActivation} type="button">
              <KeyRound size={18} />
              <span>{busy ? '激活中' : '激活并进入工作台'}</span>
            </button>
            <button className="secondary-button full-width" disabled={busy} onClick={signOut} type="button">
              <LogOut size={17} />
              <span>换一个账号</span>
            </button>
          </div>
        ) : (
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
                <label>
                  <span>激活码</span>
                  <input
                    autoComplete="one-time-code"
                    value={activationCode}
                    onChange={(event) => setActivationCode(event.target.value)}
                    placeholder="可注册后再激活"
                  />
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
        )}

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
    errorCode: session.license.isActive ? null : 'license_required',
    message: session.license.message,
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

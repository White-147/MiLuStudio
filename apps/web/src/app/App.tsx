import { FolderKanban, HardDrive, KeyRound, LogOut, Settings } from 'lucide-react';
import { useEffect, useState } from 'react';
import { AuthGate } from '../features/auth/AuthGate';
import { DesktopDiagnosticsPanel } from '../features/diagnostics/DesktopDiagnosticsPanel';
import { ProjectListPage } from '../features/projects/ProjectListPage';
import { ProductionConsolePage } from '../features/production-console/ProductionConsolePage';
import { getAuthState, logout } from '../shared/api/controlPlaneClient';
import type { AuthState } from '../shared/types/production';
import { routes, type AppRoute } from './routes';

const navItems: Array<{ route: AppRoute; label: string; icon: typeof FolderKanban }> = [
  { route: routes.projects, label: '项目', icon: FolderKanban },
  { route: routes.providerSettings, label: '模型', icon: KeyRound },
  { route: routes.storageSettings, label: '存储', icon: HardDrive },
  { route: routes.about, label: '诊断', icon: Settings },
];

export function App() {
  const [route, setRoute] = useState<AppRoute>(() => resolveRoute(currentClientPath()));
  const [activeProjectId, setActiveProjectId] = useState(() => resolveProjectId(currentClientPath()));
  const [authState, setAuthState] = useState<AuthState | null>(null);
  const [authReady, setAuthReady] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    getAuthState(controller.signal)
      .then((nextState) => setAuthState(nextState))
      .catch(() =>
        setAuthState({
          authenticated: false,
          account: null,
          device: null,
          license: {
            status: 'missing',
            isActive: false,
            plan: 'unlicensed',
            licenseType: 'none',
            startsAt: null,
            expiresAt: null,
            maxDevices: 0,
            message: 'Control API 未连接，请先启动本地服务。',
          },
          errorCode: 'control_api_unavailable',
          message: 'Control API 未连接，请先启动本地服务。',
        }),
      )
      .finally(() => setAuthReady(true));

    return () => controller.abort();
  }, []);

  useEffect(() => {
    const syncRoute = () => {
      const path = currentClientPath();
      setRoute(resolveRoute(path));
      setActiveProjectId(resolveProjectId(path));
    };

    window.addEventListener('popstate', syncRoute);
    window.addEventListener('hashchange', syncRoute);

    return () => {
      window.removeEventListener('popstate', syncRoute);
      window.removeEventListener('hashchange', syncRoute);
    };
  }, []);

  const navigate = (nextRoute: AppRoute) => {
    window.location.hash = nextRoute;
    setRoute(nextRoute);
  };

  const openProject = (projectId: string) => {
    const projectPath = `/project/${projectId}` as AppRoute;
    window.location.hash = projectPath;
    setActiveProjectId(projectId);
    setRoute(routes.project);
  };

  const signOut = async () => {
    await logout();
    setAuthState({
      authenticated: false,
      account: null,
      device: null,
      license: {
        status: 'missing',
        isActive: false,
        plan: 'unlicensed',
        licenseType: 'none',
        startsAt: null,
        expiresAt: null,
        maxDevices: 0,
        message: '已退出当前账号。',
      },
      errorCode: 'not_authenticated',
      message: '已退出当前账号。',
    });
    navigate(routes.projects);
  };

  if (!authReady) {
    return (
      <main className="auth-shell">
        <section className="auth-panel">
          <div className="auth-brand">
            <img alt="" className="brand-logo large" src="/brand/logo.png" />
            <div>
              <p className="eyebrow">MiLuStudio</p>
              <h1>正在检查授权</h1>
            </div>
          </div>
        </section>
      </main>
    );
  }

  if (!authState?.authenticated || !authState.license.isActive) {
    return <AuthGate initialState={authState} onAuthorized={setAuthState} />;
  }

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="主导航">
        <div className="brand-mark" aria-label="MiLuStudio">
          <img alt="" className="brand-logo" src="/brand/logo.png" />
          <span>MiLuStudio</span>
        </div>

        <nav className="nav-list">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = route === item.route || (item.route === routes.projects && route === routes.project);

            return (
              <button
                className={active ? 'nav-button active' : 'nav-button'}
                key={item.route}
                onClick={() => navigate(item.route)}
                type="button"
              >
                <Icon size={18} />
                <span>{item.label}</span>
              </button>
            );
          })}
        </nav>
        <div className="account-strip">
          <div>
            <strong>{authState.account?.displayName}</strong>
            <span>{authState.license.plan}</span>
          </div>
          <button aria-label="退出登录" className="icon-button" onClick={() => void signOut()} type="button">
            <LogOut size={16} />
          </button>
        </div>
      </aside>

      <main className="main-surface">
        {route === routes.projects && <ProjectListPage onOpenProject={openProject} />}
        {route === routes.project && <ProductionConsolePage onBack={() => navigate(routes.projects)} projectId={activeProjectId} />}
        {route === routes.about && <DesktopDiagnosticsPanel />}
        {route !== routes.projects && route !== routes.project && route !== routes.about && (
          <section className="placeholder-view">
            <p className="eyebrow">配置</p>
            <h1>配置入口已预留</h1>
            <p>模型、存储和诊断页面会在 Control API 接入后补齐。</p>
          </section>
        )}
      </main>
    </div>
  );
}

function currentClientPath(): string {
  if (window.location.hash.startsWith('#/')) {
    return window.location.hash.slice(1);
  }

  return window.location.pathname;
}

function resolveRoute(pathname: string): AppRoute {
  if (pathname.startsWith('/project/')) {
    return routes.project;
  }

  const matched = Object.values(routes).find((route) => route === pathname);

  return matched ?? routes.projects;
}

function resolveProjectId(pathname: string): string {
  if (pathname.startsWith('/project/')) {
    const [, , projectId] = pathname.split('/');
    return projectId || 'demo-episode-01';
  }

  return 'demo-episode-01';
}

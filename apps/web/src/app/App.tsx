import { FolderKanban, HardDrive, KeyRound, Settings } from 'lucide-react';
import { useEffect, useState } from 'react';
import { ProjectListPage } from '../features/projects/ProjectListPage';
import { ProductionConsolePage } from '../features/production-console/ProductionConsolePage';
import { routes, type AppRoute } from './routes';

const navItems: Array<{ route: AppRoute; label: string; icon: typeof FolderKanban }> = [
  { route: routes.projects, label: '项目', icon: FolderKanban },
  { route: routes.providerSettings, label: '模型', icon: KeyRound },
  { route: routes.storageSettings, label: '存储', icon: HardDrive },
  { route: routes.about, label: '诊断', icon: Settings },
];

export function App() {
  const [route, setRoute] = useState<AppRoute>(() => resolveRoute(window.location.pathname));
  const [activeProjectId, setActiveProjectId] = useState(() => resolveProjectId(window.location.pathname));

  useEffect(() => {
    const syncRoute = () => {
      setRoute(resolveRoute(window.location.pathname));
      setActiveProjectId(resolveProjectId(window.location.pathname));
    };

    window.addEventListener('popstate', syncRoute);

    return () => window.removeEventListener('popstate', syncRoute);
  }, []);

  const navigate = (nextRoute: AppRoute) => {
    window.history.pushState({}, '', nextRoute);
    setRoute(nextRoute);
  };

  const openProject = (projectId: string) => {
    const projectPath = `/project/${projectId}` as AppRoute;
    window.history.pushState({}, '', projectPath);
    setActiveProjectId(projectId);
    setRoute(routes.project);
  };

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
      </aside>

      <main className="main-surface">
        {route === routes.projects && <ProjectListPage onOpenProject={openProject} />}
        {route === routes.project && <ProductionConsolePage onBack={() => navigate(routes.projects)} projectId={activeProjectId} />}
        {route !== routes.projects && route !== routes.project && (
          <section className="placeholder-view">
            <p className="eyebrow">Stage 1 mock</p>
            <h1>配置入口已预留</h1>
            <p>模型、存储和诊断页面会在 Control API 接入后补齐。</p>
          </section>
        )}
      </main>
    </div>
  );
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

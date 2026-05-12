export const routes = {
  projects: '/',
  project: '/project/demo-episode-01',
  providerSettings: '/settings/providers',
  storageSettings: '/settings/storage',
  about: '/settings/about',
} as const;

export type AppRoute = (typeof routes)[keyof typeof routes];

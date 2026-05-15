import { useEffect, useState } from 'react';

export type AppTheme = 'dark' | 'light';

const THEME_STORAGE_KEY = 'milu.theme';
const THEME_CHANGE_EVENT = 'milu:theme-change';
const DEFAULT_THEME: AppTheme = 'dark';

function normalizeTheme(value: string | null | undefined): AppTheme | null {
  return value === 'dark' || value === 'light' ? value : null;
}

export function getCurrentTheme(): AppTheme {
  if (typeof window === 'undefined') {
    return DEFAULT_THEME;
  }

  return normalizeTheme(window.localStorage.getItem(THEME_STORAGE_KEY)) ?? DEFAULT_THEME;
}

export function applyTheme(theme: AppTheme) {
  if (typeof document === 'undefined') {
    return;
  }

  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;
}

export function initializeTheme() {
  const theme = getCurrentTheme();
  applyTheme(theme);
  return theme;
}

export function setCurrentTheme(theme: AppTheme) {
  if (typeof window !== 'undefined') {
    window.localStorage.setItem(THEME_STORAGE_KEY, theme);
  }

  applyTheme(theme);

  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent<AppTheme>(THEME_CHANGE_EVENT, { detail: theme }));
  }
}

export function subscribeThemeChange(listener: (theme: AppTheme) => void) {
  if (typeof window === 'undefined') {
    return () => {};
  }

  const handleStorage = (event: StorageEvent) => {
    if (event.key !== THEME_STORAGE_KEY) {
      return;
    }

    const nextTheme = normalizeTheme(event.newValue) ?? DEFAULT_THEME;
    applyTheme(nextTheme);
    listener(nextTheme);
  };

  const handleCustomEvent = (event: Event) => {
    const nextTheme = normalizeTheme((event as CustomEvent<AppTheme>).detail) ?? getCurrentTheme();
    applyTheme(nextTheme);
    listener(nextTheme);
  };

  window.addEventListener('storage', handleStorage);
  window.addEventListener(THEME_CHANGE_EVENT, handleCustomEvent as EventListener);

  return () => {
    window.removeEventListener('storage', handleStorage);
    window.removeEventListener(THEME_CHANGE_EVENT, handleCustomEvent as EventListener);
  };
}

export function useTheme() {
  const [theme, setThemeState] = useState<AppTheme>(() => initializeTheme());

  useEffect(() => {
    setThemeState(initializeTheme());
    return subscribeThemeChange(setThemeState);
  }, []);

  const setTheme = (nextTheme: AppTheme | ((currentTheme: AppTheme) => AppTheme)) => {
    const resolvedTheme = typeof nextTheme === 'function' ? nextTheme(getCurrentTheme()) : nextTheme;
    setCurrentTheme(resolvedTheme);
    setThemeState(resolvedTheme);
  };

  return [theme, setTheme] as const;
}

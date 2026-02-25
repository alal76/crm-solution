/**
 * useNavigationPreferences - Persist sidebar section collapse/expand state.
 * TODO-UX-12: Customizable sidebar navigation.
 *
 * Stores user's expanded/collapsed state for both main navigation categories
 * AND admin sub-categories in localStorage under `crm-nav-preferences`.
 * State survives page refresh.
 */

import { useState, useCallback, useEffect } from 'react';

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

const STORAGE_KEY = 'crm-nav-preferences';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface NavigationPreferences {
  categories: Record<string, boolean>;
  adminSections: Record<string, boolean>;
}

// ---------------------------------------------------------------------------
// Defaults (mirrors the defaults in Navigation.tsx)
// ---------------------------------------------------------------------------

const DEFAULT_CATEGORIES: Record<string, boolean> = {
  main: true,
  sales: true,
  marketing: true,
  support: true,
  itsm: true,
  productivity: true,
  agents: true,
  info: false,
  admin: true,
};

const DEFAULT_ADMIN_SECTIONS: Record<string, boolean> = {
  'system-config': true,
  'user-management': true,
  'crm-config': true,
  'ai-integrations': false,
  infrastructure: false,
  customization: false,
  workflows: false,
  'developer-tools': false,
};

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function loadPreferences(): NavigationPreferences {
  if (typeof window === 'undefined') {
    return { categories: DEFAULT_CATEGORIES, adminSections: DEFAULT_ADMIN_SECTIONS };
  }
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw) as Partial<NavigationPreferences>;
      return {
        categories: { ...DEFAULT_CATEGORIES, ...(parsed.categories ?? {}) },
        adminSections: { ...DEFAULT_ADMIN_SECTIONS, ...(parsed.adminSections ?? {}) },
      };
    }
  } catch {
    // Corrupt storage — use defaults
  }
  return { categories: DEFAULT_CATEGORIES, adminSections: DEFAULT_ADMIN_SECTIONS };
}

function savePreferences(prefs: NavigationPreferences): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
  } catch {
    // Quota exceeded or private browsing — silently ignore
  }
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

export interface UseNavigationPreferencesReturn {
  /** Whether each main category is expanded */
  expandedCategories: Record<string, boolean>;
  /** Whether each admin sub-category is expanded */
  expandedAdminSections: Record<string, boolean>;
  /** Toggle a main navigation category */
  toggleCategory: (categoryId: string) => void;
  /** Toggle an admin sub-category */
  toggleAdminSection: (sectionId: string) => void;
  /** Force-expand a category (e.g., on route match) without toggling */
  expandCategory: (categoryId: string) => void;
  /** Force-expand an admin section (e.g., on route match) without toggling */
  expandAdminSection: (sectionId: string) => void;
  /** Reset all preferences to defaults */
  resetPreferences: () => void;
}

export function useNavigationPreferences(): UseNavigationPreferencesReturn {
  const [preferences, setPreferences] = useState<NavigationPreferences>(loadPreferences);

  // Persist every time preferences change
  useEffect(() => {
    savePreferences(preferences);
  }, [preferences]);

  const toggleCategory = useCallback((categoryId: string) => {
    setPreferences((prev) => ({
      ...prev,
      categories: {
        ...prev.categories,
        [categoryId]: !(prev.categories[categoryId] ?? true),
      },
    }));
  }, []);

  const toggleAdminSection = useCallback((sectionId: string) => {
    setPreferences((prev) => ({
      ...prev,
      adminSections: {
        ...prev.adminSections,
        [sectionId]: !(prev.adminSections[sectionId] ?? false),
      },
    }));
  }, []);

  const expandCategory = useCallback((categoryId: string) => {
    setPreferences((prev) => {
      if (prev.categories[categoryId]) return prev; // already expanded — no-op
      return {
        ...prev,
        categories: { ...prev.categories, [categoryId]: true },
      };
    });
  }, []);

  const expandAdminSection = useCallback((sectionId: string) => {
    setPreferences((prev) => {
      if (prev.adminSections[sectionId]) return prev; // already expanded — no-op
      return {
        ...prev,
        adminSections: { ...prev.adminSections, [sectionId]: true },
      };
    });
  }, []);

  const resetPreferences = useCallback(() => {
    const defaults: NavigationPreferences = {
      categories: DEFAULT_CATEGORIES,
      adminSections: DEFAULT_ADMIN_SECTIONS,
    };
    setPreferences(defaults);
    savePreferences(defaults);
  }, []);

  return {
    expandedCategories: preferences.categories,
    expandedAdminSections: preferences.adminSections,
    toggleCategory,
    toggleAdminSection,
    expandCategory,
    expandAdminSection,
    resetPreferences,
  };
}

export default useNavigationPreferences;

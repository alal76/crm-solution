// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Source-Available License (see LICENSE) as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// Source-Available License (see LICENSE) for more details.
//
// You should have received a copy of the Source-Available License (see LICENSE)
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React, { createContext, useContext, useEffect, useState } from 'react';
import apiClient from '../services/apiClient';

interface UIPreference {
  id: number;
  userId: number;
  theme: 'light' | 'dark' | 'auto';
  sidebarPosition: 'left' | 'right' | 'hidden';
  sidebarWidth: number;
  fontSize: 'small' | 'normal' | 'large';
  showBreadcrumbs: boolean;
  showStatusBar: boolean;
  showTopNavigation: boolean;
  defaultPageSize: number;
  dateFormat: string;
  timeFormat: string;
  customColorScheme?: string;
  lastPreferenceUpdate: string;
}

interface UIContextType {
  preferences: UIPreference | null;
  loading: boolean;
  error: string | null;
  savePreferences: (prefs: Partial<UIPreference>) => Promise<void>;
  resetPreferences: () => Promise<void>;
  applyTheme: (theme: string) => void;
}

const UIPreferencesContext = createContext<UIContextType | undefined>(undefined);

export { UIPreferencesContext };

export const UIPreferencesProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [preferences, setPreferences] = useState<UIPreference | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load preferences on mount
  useEffect(() => {
    loadPreferences();
  }, []);

  // Apply theme when preferences load
  useEffect(() => {
    if (preferences?.theme) {
      applyTheme(preferences.theme);
    }
  }, [preferences?.theme]);

  const loadPreferences = async () => {
    try {
      const response = await apiClient.get<UIPreference>('/api/ui-preferences');
      setPreferences(response.data);
      setError(null);
    } catch (err) {
      console.error('Failed to load UI preferences:', err);
      setError('Failed to load UI preferences');
      // Set defaults if load fails
      setPreferences(getDefaultPreferences());
    } finally {
      setLoading(false);
    }
  };

  const savePreferences = async (prefs: Partial<UIPreference>) => {
    try {
      const response = await apiClient.post<UIPreference>('/api/ui-preferences', prefs);
      setPreferences(response.data);
      setError(null);
    } catch (err) {
      console.error('Failed to save UI preferences:', err);
      setError('Failed to save preferences');
    }
  };

  const resetPreferences = async () => {
    try {
      await apiClient.post('/api/ui-preferences/reset');
      await loadPreferences();
      setError(null);
    } catch (err) {
      console.error('Failed to reset preferences:', err);
      setError('Failed to reset preferences');
    }
  };

  const applyTheme = (theme: string) => {
    const root = document.documentElement;
    if (theme === 'dark' || (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
      root.setAttribute('data-theme', 'dark');
    } else {
      root.setAttribute('data-theme', 'light');
    }
  };

  return (
    <UIPreferencesContext.Provider value={{ preferences, loading, error, savePreferences, resetPreferences, applyTheme }}>
      {children}
    </UIPreferencesContext.Provider>
  );
};

export const useUIPreferences = () => {
  const context = useContext(UIPreferencesContext);
  if (!context) {
    throw new Error('useUIPreferences must be used within UIPreferencesProvider');
  }
  return context;
};

const getDefaultPreferences = (): UIPreference => ({
  id: 0,
  userId: 0,
  theme: 'auto',
  sidebarPosition: 'left',
  sidebarWidth: 250,
  fontSize: 'normal',
  showBreadcrumbs: true,
  showStatusBar: true,
  showTopNavigation: true,
  defaultPageSize: 20,
  dateFormat: 'MM/dd/yyyy',
  timeFormat: 'hh:mm a',
  lastPreferenceUpdate: new Date().toISOString()
});

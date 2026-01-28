/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import React, { createContext, useContext, useState, useEffect, useMemo, ReactNode } from 'react';
import { Theme } from '@mui/material/styles';
import { createAppTheme, ThemeMode } from '../theme/muiTheme';

interface ThemeContextType {
  themeMode: ThemeMode;
  effectiveTheme: 'light' | 'dark' | 'high-contrast';
  theme: Theme;
  setThemeMode: (mode: ThemeMode) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const STORAGE_KEY = 'crm_theme_preference';

// Detect system preference
const getSystemTheme = (): 'light' | 'dark' => {
  if (typeof window !== 'undefined' && window.matchMedia) {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
  return 'light';
};

// Get effective theme from mode
const resolveThemeMode = (mode: ThemeMode): 'light' | 'dark' | 'high-contrast' => {
  if (mode === 'system') {
    return getSystemTheme();
  }
  return mode;
};

interface ThemeProviderProps {
  children: ReactNode;
}

export const AppThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  // Initialize from localStorage or default to 'system'
  const [themeMode, setThemeModeState] = useState<ThemeMode>(() => {
    if (typeof window !== 'undefined') {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored && ['system', 'light', 'dark', 'high-contrast'].includes(stored)) {
        return stored as ThemeMode;
      }
    }
    return 'system';
  });

  // Track effective theme for when mode is 'system'
  const [effectiveTheme, setEffectiveTheme] = useState<'light' | 'dark' | 'high-contrast'>(() => 
    resolveThemeMode(themeMode)
  );

  // Listen for system theme changes
  useEffect(() => {
    if (typeof window === 'undefined' || !window.matchMedia) return;

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handleChange = () => {
      if (themeMode === 'system') {
        setEffectiveTheme(getSystemTheme());
      }
    };

    // Initial set
    if (themeMode === 'system') {
      setEffectiveTheme(getSystemTheme());
    } else {
      setEffectiveTheme(themeMode);
    }

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [themeMode]);

  // Update effective theme when mode changes
  useEffect(() => {
    setEffectiveTheme(resolveThemeMode(themeMode));
  }, [themeMode]);

  // Create theme based on effective mode
  const theme = useMemo(() => createAppTheme(effectiveTheme), [effectiveTheme]);

  // Set theme mode and persist
  const setThemeMode = (mode: ThemeMode) => {
    setThemeModeState(mode);
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, mode);
    }
  };

  // Apply theme class to body for any CSS that needs it
  useEffect(() => {
    if (typeof document !== 'undefined') {
      document.body.classList.remove('theme-light', 'theme-dark', 'theme-high-contrast');
      document.body.classList.add(`theme-${effectiveTheme}`);
      
      // Set color-scheme for browser native elements
      document.documentElement.style.colorScheme = effectiveTheme === 'light' ? 'light' : 'dark';
    }
  }, [effectiveTheme]);

  const value = useMemo(() => ({
    themeMode,
    effectiveTheme,
    theme,
    setThemeMode,
  }), [themeMode, effectiveTheme, theme]);

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
};

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within an AppThemeProvider');
  }
  return context;
};

export default ThemeContext;

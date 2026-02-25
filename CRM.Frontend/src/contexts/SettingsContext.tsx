import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import settingsService, { SystemSettingsDto } from '../services/settingsService';
import { useAuth } from './AuthContext';

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────

export interface SettingsContextValue {
  /** Full settings object (null while loading or unauthenticated) */
  settings: SystemSettingsDto | null;
  loading: boolean;
  /** Default currency ISO code, e.g. "USD" */
  defaultCurrency: string;
  /** Default IANA timezone, e.g. "America/New_York" */
  defaultTimezone: string;
  /** Default BCP-47 language tag, e.g. "en-US" */
  defaultLanguage: string;
  /** Date format pattern, e.g. "MM/dd/yyyy" */
  dateFormat: string;
  /** Time format: "12h" or "24h" */
  timeFormat: string;
  /**
   * Format a monetary amount using the org default currency.
   * Passing a non-null `currencyOverride` uses that currency instead.
   * Null/undefined amount returns "-".
   */
  formatCurrency: (amount: number | null | undefined, currencyOverride?: string | null) => string;
  /**
   * Format a date value using the user's locale.
   * Returns "-" for null/undefined.
   */
  formatDate: (date: string | Date | null | undefined) => string;
  /** Re-fetch settings from API (call after saving changes in admin panel) */
  refresh: () => Promise<void>;
}

// ─────────────────────────────────────────────────────────────────────────────
// Context
// ─────────────────────────────────────────────────────────────────────────────

const SettingsContext = createContext<SettingsContextValue | null>(null);

// ─────────────────────────────────────────────────────────────────────────────
// Provider
// ─────────────────────────────────────────────────────────────────────────────

export const SettingsProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [settings, setSettings] = useState<SystemSettingsDto | null>(null);
  const [loading, setLoading] = useState(false);
  const { isAuthenticated } = useAuth();

  const fetchSettings = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      setLoading(true);
      const data = await settingsService.getSettings();
      setSettings(data);
    } catch {
      // Silently degrade — hardcoded defaults below will apply
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchSettings();
    } else {
      setSettings(null);
    }
  }, [isAuthenticated, fetchSettings]);

  // ── Derived values with hardcoded safe defaults ───────────────────────────
  const defaultCurrency = settings?.defaultCurrency || 'USD';
  const defaultTimezone = settings?.defaultTimezone || 'UTC';
  const defaultLanguage = settings?.defaultLanguage || 'en';
  const dateFormat = settings?.dateFormat || 'MM/dd/yyyy';
  const timeFormat = settings?.timeFormat || '12h';

  // ── Helpers ────────────────────────────────────────────────────────────────

  const formatCurrency = useCallback(
    (amount: number | null | undefined, currencyOverride?: string | null): string => {
      if (amount == null) return '-';
      const currency = (currencyOverride || defaultCurrency) || 'USD';
      try {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount);
      } catch {
        return `${currency} ${amount.toFixed(2)}`;
      }
    },
    [defaultCurrency]
  );

  const formatDate = useCallback(
    (date: string | Date | null | undefined): string => {
      if (!date) return '-';
      try {
        return new Date(date).toLocaleDateString();
      } catch {
        return '-';
      }
    },
    []
  );

  return (
    <SettingsContext.Provider
      value={{
        settings,
        loading,
        defaultCurrency,
        defaultTimezone,
        defaultLanguage,
        dateFormat,
        timeFormat,
        formatCurrency,
        formatDate,
        refresh: fetchSettings,
      }}
    >
      {children}
    </SettingsContext.Provider>
  );
};

// ─────────────────────────────────────────────────────────────────────────────
// Hook
// ─────────────────────────────────────────────────────────────────────────────

export const useSettings = (): SettingsContextValue => {
  const ctx = useContext(SettingsContext);
  if (!ctx) {
    // If called outside the provider (e.g. in tests) return safe defaults
    return {
      settings: null,
      loading: false,
      defaultCurrency: 'USD',
      defaultTimezone: 'UTC',
      defaultLanguage: 'en',
      dateFormat: 'MM/dd/yyyy',
      timeFormat: '12h',
      formatCurrency: (amount, currencyOverride) => {
        if (amount == null) return '-';
        const currency = currencyOverride || 'USD';
        try { return new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount); }
        catch { return `${currency} ${amount.toFixed(2)}`; }
      },
      formatDate: (date) => (!date ? '-' : new Date(date).toLocaleDateString()),
      refresh: async () => {},
    };
  }
  return ctx;
};

export default SettingsContext;

/**
 * useDashboardLayout - Hook for persisting and loading user dashboard widget layouts.
 * Implements TODO-PORTAL-05.
 *
 * Provides load/save operations against:
 *   GET  /api/users/{userId}/dashboard-layout/default
 *   PUT  /api/users/{userId}/dashboard-layout
 */

import { useState, useCallback, useEffect } from 'react';
import apiClient from '../services/apiClient';
import { useAuth } from '../contexts/AuthContext';

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface DashboardLayout {
  userId: number;
  name: string;
  layoutJson: string;
  isDefault: boolean;
}

export interface UseDashboardLayoutResult {
  layout: DashboardLayout | null;
  loading: boolean;
  error: string | null;
  saveLayout: (layoutJson: string, name?: string) => Promise<void>;
  reload: () => void;
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

export function useDashboardLayout(): UseDashboardLayoutResult {
  const { user } = useAuth();
  const [layout, setLayout] = useState<DashboardLayout | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  const reload = useCallback(() => setReloadKey((k) => k + 1), []);

  // Load default layout for current user
  useEffect(() => {
    if (!user?.id) return;

    let cancelled = false;
    setLoading(true);
    setError(null);

    apiClient
      .get<DashboardLayout>(`/users/${user.id}/dashboard-layout/default`)
      .then((res) => {
        if (!cancelled) setLayout(res.data);
      })
      .catch((err) => {
        if (!cancelled) {
          // 404 is acceptable – user has no saved layout yet
          if ((err as any)?.response?.status !== 404) {
            setError((err as any)?.response?.data?.message ?? 'Failed to load dashboard layout');
          }
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [user?.id, reloadKey]);

  // Save (upsert) layout
  const saveLayout = useCallback(
    async (layoutJson: string, name = 'Default') => {
      if (!user?.id) return;
      setLoading(true);
      setError(null);
      try {
        const res = await apiClient.put<DashboardLayout>(`/users/${user.id}/dashboard-layout`, {
          layoutJson,
          name,
          isDefault: true,
        });
        setLayout(res.data);
      } catch (err: unknown) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr?.response?.data?.message ?? 'Failed to save dashboard layout');
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [user?.id],
  );

  return { layout, loading, error, saveLayout, reload };
}

export default useDashboardLayout;

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

import React from 'react';

/**
 * Hook to check if a feature flag is enabled for the current user
 */
export const useFeatureFlag = (flagName: string) => {
  const [isEnabled, setIsEnabled] = React.useState(false);
  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    const checkFlag = async () => {
      try {
        const response = await fetch(`/api/feature-flags/${flagName}/check`);
        const enabled = await response.json();
        setIsEnabled(enabled);
      } catch (error) {
        console.error(`Failed to check feature flag ${flagName}:`, error);
        setIsEnabled(false);
      } finally {
        setLoading(false);
      }
    };

    checkFlag();
  }, [flagName]);

  return { isEnabled, loading };
};

/**
 * Hook to get A/B testing variant for the current user
 */
export const useFeatureFlagVariant = (flagName: string) => {
  const [variant, setVariant] = React.useState<{ name: string; config: any } | null>(null);
  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    const getVariant = async () => {
      try {
        const response = await fetch(`/api/feature-flags/${flagName}/variant`);
        if (response.ok) {
          const v = await response.json();
          setVariant(v);
        }
      } catch (error) {
        console.error(`Failed to get variant for ${flagName}:`, error);
      } finally {
        setLoading(false);
      }
    };

    getVariant();
  }, [flagName]);

  return { variant, loading };
};

/**
 * Hook for saving user dashboard customization
 */
export const useDashboardCustomization = (dashboardName: string) => {
  const [dashboard, setDashboard] = React.useState<any>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const loadDashboard = React.useCallback(async () => {
    try {
      const response = await fetch(`/api/ui-preferences/dashboards/${dashboardName}`);
      if (response.ok) {
        const data = await response.json();
        setDashboard(data);
      }
      setError(null);
    } catch (err) {
      console.error('Failed to load dashboard:', err);
      setError('Failed to load dashboard');
    } finally {
      setLoading(false);
    }
  }, [dashboardName]);

  const saveDashboard = React.useCallback(async (config: any) => {
    try {
      const response = await fetch('/api/ui-preferences/dashboards', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ dashboardName, ...config })
      });
      if (response.ok) {
        const saved = await response.json();
        setDashboard(saved);
      }
      setError(null);
    } catch (err) {
      console.error('Failed to save dashboard:', err);
      setError('Failed to save dashboard');
    }
  }, [dashboardName]);

  React.useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  return { dashboard, loading, error, saveDashboard };
};

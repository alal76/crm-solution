/**
 * ITSM Context Provider
 *
 * Provides shared state management for ITSM module pages:
 * - Dashboard metrics (cached, refreshable)
 * - Active filters for incident/problem/change lists
 * - Module-level navigation state (active tab)
 * - SLA breach alerts count
 */
import React, { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import { useAuth } from './AuthContext';
import itsmService from '../services/itsmService';

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────

export type ITSMModuleTab = 'incidents' | 'problems' | 'changes' | 'knowledge' | 'cmdb' | 'sla' | 'dashboard';

export interface ITSMDashboardMetrics {
  openIncidents: number;
  criticalIncidents: number;
  openProblems: number;
  pendingChanges: number;
  slaBreaches: number;
  avgResolutionTime: number;
}

export interface ITSMFilters {
  status?: number;
  priority?: number;
  assignedToId?: number;
  search?: string;
  dateFrom?: string;
  dateTo?: string;
}

export interface ITSMContextValue {
  /** Current active ITSM module tab */
  activeTab: ITSMModuleTab;
  setActiveTab: (tab: ITSMModuleTab) => void;

  /** Dashboard metrics (null while loading) */
  metrics: ITSMDashboardMetrics | null;
  metricsLoading: boolean;
  refreshMetrics: () => Promise<void>;

  /** Shared filters for list pages */
  incidentFilters: ITSMFilters;
  setIncidentFilters: (filters: ITSMFilters) => void;
  problemFilters: ITSMFilters;
  setProblemFilters: (filters: ITSMFilters) => void;
  changeFilters: ITSMFilters;
  setChangeFilters: (filters: ITSMFilters) => void;

  /** Clear all filters */
  clearAllFilters: () => void;
}

// ─────────────────────────────────────────────────────────────────────────────
// Context
// ─────────────────────────────────────────────────────────────────────────────

const defaultFilters: ITSMFilters = {};

const ITSMContext = createContext<ITSMContextValue | null>(null);

// ─────────────────────────────────────────────────────────────────────────────
// Provider
// ─────────────────────────────────────────────────────────────────────────────

export const ITSMProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();

  const [activeTab, setActiveTab] = useState<ITSMModuleTab>('dashboard');
  const [metrics, setMetrics] = useState<ITSMDashboardMetrics | null>(null);
  const [metricsLoading, setMetricsLoading] = useState(false);
  const [incidentFilters, setIncidentFilters] = useState<ITSMFilters>(defaultFilters);
  const [problemFilters, setProblemFilters] = useState<ITSMFilters>(defaultFilters);
  const [changeFilters, setChangeFilters] = useState<ITSMFilters>(defaultFilters);

  const refreshMetrics = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      setMetricsLoading(true);
      const response = await itsmService.getITSMMetrics();
      const data = response.data;
      if (data) {
        setMetrics({
          openIncidents: data.openIncidents ?? 0,
          criticalIncidents: data.criticalIncidents ?? 0,
          openProblems: data.openProblems ?? 0,
          pendingChanges: data.pendingChanges ?? 0,
          slaBreaches: data.slaBreaches ?? 0,
          avgResolutionTime: data.avgResolutionTime ?? 0,
        });
      }
    } catch {
      // Silently degrade — metrics will show as null
    } finally {
      setMetricsLoading(false);
    }
  }, [isAuthenticated]);

  // Auto-fetch metrics on authentication
  useEffect(() => {
    if (isAuthenticated) {
      refreshMetrics();
    } else {
      setMetrics(null);
    }
  }, [isAuthenticated, refreshMetrics]);

  const clearAllFilters = useCallback(() => {
    setIncidentFilters(defaultFilters);
    setProblemFilters(defaultFilters);
    setChangeFilters(defaultFilters);
  }, []);

  const value: ITSMContextValue = {
    activeTab,
    setActiveTab,
    metrics,
    metricsLoading,
    refreshMetrics,
    incidentFilters,
    setIncidentFilters,
    problemFilters,
    setProblemFilters,
    changeFilters,
    setChangeFilters,
    clearAllFilters,
  };

  return <ITSMContext.Provider value={value}>{children}</ITSMContext.Provider>;
};

// ─────────────────────────────────────────────────────────────────────────────
// Hook
// ─────────────────────────────────────────────────────────────────────────────

export const useITSM = (): ITSMContextValue => {
  const context = useContext(ITSMContext);
  if (!context) {
    throw new Error('useITSM must be used within an ITSMProvider');
  }
  return context;
};

export default ITSMContext;

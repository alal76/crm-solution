// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import apiClient from './apiClient';

// ── Types ──────────────────────────────────────────────────────────────────────

export interface RevenueSnapshotDto {
  id: number;
  snapshotDate: string;
  mrr: number;
  arr: number;
  newMRR: number;
  expansionMRR: number;
  contractionMRR: number;
  churnMRR: number;
  netNewMRR: number;
  customerCount: number;
  newCustomers: number;
  churnedCustomers: number;
  notes?: string;
  snapshotType: string;
  createdAt: string;
  updatedAt?: string;
}

export interface RevenueMetricsDto {
  currentMRR: number;
  currentARR: number;
  moMGrowthRate: number;
  churnRate: number;
  expansionRate: number;
  netRevenueRetention: number;
  totalCustomers: number;
  averageRevenuePerCustomer: number;
  trend: RevenueSnapshotDto[];
}

export interface RevenueMRRMovementDto {
  period: string;
  label: string;
  openingMRR: number;
  newMRR: number;
  expansionMRR: number;
  contractionMRR: number;
  churnMRR: number;
  closingMRR: number;
}

export interface CreateRevenueSnapshotDto {
  snapshotDate: string;
  mrr: number;
  newMRR: number;
  expansionMRR: number;
  contractionMRR: number;
  churnMRR: number;
  customerCount: number;
  newCustomers: number;
  churnedCustomers: number;
  notes?: string;
  snapshotType?: string;
}

// ── API Calls ──────────────────────────────────────────────────────────────────

const BASE = '/api/revenue';

export const revenueAnalyticsService = {
  /** Returns aggregated revenue metrics including trend. */
  getMetrics: async (from?: string, to?: string): Promise<RevenueMetricsDto> => {
    const params = new URLSearchParams();
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const query = params.toString() ? `?${params}` : '';
    const res = await apiClient.get<RevenueMetricsDto>(`${BASE}/metrics${query}`);
    return res.data;
  },

  /** Returns the last N monthly snapshots sorted ascending by date. */
  getTrend: async (months = 12): Promise<RevenueSnapshotDto[]> => {
    const res = await apiClient.get<RevenueSnapshotDto[]>(`${BASE}/trend?months=${months}`);
    return res.data;
  },

  /** Returns MRR waterfall movements for the last N months. */
  getMRRMovements: async (months = 12): Promise<RevenueMRRMovementDto[]> => {
    const res = await apiClient.get<RevenueMRRMovementDto[]>(`${BASE}/movements?months=${months}`);
    return res.data;
  },

  /** Returns the current MRR value. */
  getCurrentMRR: async (): Promise<number> => {
    const res = await apiClient.get<{ mrr: number }>(`${BASE}/mrr`);
    return res.data.mrr;
  },

  /** Returns the current ARR value. */
  getCurrentARR: async (): Promise<number> => {
    const res = await apiClient.get<{ arr: number }>(`${BASE}/arr`);
    return res.data.arr;
  },

  /** Returns the churn rate for a date range. */
  getChurnRate: async (from?: string, to?: string): Promise<number> => {
    const params = new URLSearchParams();
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const query = params.toString() ? `?${params}` : '';
    const res = await apiClient.get<{ churnRate: number }>(`${BASE}/churn-rate${query}`);
    return res.data.churnRate;
  },

  /** Creates a manual revenue snapshot (Admin only). */
  createSnapshot: async (dto: CreateRevenueSnapshotDto): Promise<RevenueSnapshotDto> => {
    const res = await apiClient.post<RevenueSnapshotDto>(`${BASE}/snapshots`, dto);
    return res.data;
  },

  /** Calculates a snapshot from live subscriptions (Admin only). */
  calculateCurrentSnapshot: async (): Promise<RevenueSnapshotDto> => {
    const res = await apiClient.post<RevenueSnapshotDto>(`${BASE}/snapshots/calculate`, {});
    return res.data;
  },
};

export default revenueAnalyticsService;

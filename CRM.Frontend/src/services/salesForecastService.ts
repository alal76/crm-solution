// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import apiClient from './apiClient';

// ── Types ──────────────────────────────────────────────────────────────────────

/**
 * Forecast category, mirrors CRM.Core.Entities.ForecastCategory (Opportunity.cs).
 * Note: Closed = 3 and ClosedWon = 3 are aliases on the backend enum.
 */
export enum ForecastCategory {
  Pipeline = 0,
  BestCase = 1,
  Commit = 2,
  Closed = 3,
  ClosedWon = 3,
  Omitted = 4,
  MostLikely = 5,
}

export const FORECAST_CATEGORY_LABELS: Record<number, string> = {
  0: 'Pipeline',
  1: 'Best Case',
  2: 'Commit',
  3: 'Closed Won',
  4: 'Omitted',
  5: 'Most Likely',
};

/** Mirrors CRM.Core.Entities.SalesForecast (defined in SalesQuota.cs). */
export interface SalesForecastDto {
  id: number;
  createdAt: string;
  updatedAt?: string;
  isDeleted?: boolean;

  // Identification
  name: string;
  period: string;
  periodStartDate: string;
  periodEndDate: string;
  fiscalYear: number;
  fiscalQuarter?: number;
  fiscalMonth?: number;

  // Quota context
  quotaAmount: number;
  currencyCode: string;

  // Forecast categories
  closedWonAmount: number;
  commitAmount: number;
  bestCaseAmount: number;
  pipelineAmount: number;
  omittedAmount: number;

  // Calculated fields (computed server-side, read-only)
  forecastAmount?: number;
  gapToQuota?: number;
  coverageRatio?: number;
  forecastAttainmentPercent?: number;

  // Deal counts
  closedWonCount?: number;
  commitCount?: number;
  bestCaseCount?: number;
  pipelineCount?: number;

  // Manager adjustments
  adjustedCommitAmount?: number;
  adjustedBestCaseAmount?: number;
  adjustmentNotes?: string;
  adjustedById?: number;
  adjustedAt?: string;

  // Snapshot / submission
  snapshotDate?: string;
  isSubmitted: boolean;
  submittedAt?: string;

  // Relationships
  userId?: number;
  teamId?: number;
  salesQuotaId?: number;
  parentForecastId?: number;

  lineItems?: ForecastLineItemDto[];
}

/** Mirrors CRM.Core.Entities.ForecastLineItem (defined in SalesQuota.cs). */
export interface ForecastLineItemDto {
  id: number;
  createdAt: string;
  updatedAt?: string;

  category: ForecastCategory;
  amount: number;
  closeDate: string;
  stage?: string;
  probability: number;

  overrideCategory?: ForecastCategory;
  overrideAmount?: number;
  overrideNotes?: string;

  salesForecastId: number;
  opportunityId: number;
}

/** Mirrors CRM.Core.Entities.ForecastHistory (defined in SalesQuota.cs). */
export interface ForecastHistoryDto {
  id: number;
  createdAt: string;
  updatedAt?: string;

  snapshotDate: string;
  period: string;
  userId?: number;
  teamId?: number;
  quotaAmount: number;
  closedWonAmount: number;
  commitAmount: number;
  bestCaseAmount: number;
  pipelineAmount: number;
  weeksRemaining: number;
}

/** Query filters supported by GET /api/sales-forecasts */
export interface SalesForecastFilters {
  userId?: number;
  teamId?: number;
  fiscalYear?: number;
  isSubmitted?: boolean;
}

/** Payload accepted by create/update (backend takes the full SalesForecast entity). */
export type SalesForecastInput = Partial<Omit<SalesForecastDto, 'id' | 'createdAt' | 'updatedAt'>>;

// ── API Calls ──────────────────────────────────────────────────────────────────

const BASE = '/api/sales-forecasts';

export const salesForecastService = {
  /** GET /api/sales-forecasts — list with optional filters. */
  getAll: async (filters?: SalesForecastFilters): Promise<SalesForecastDto[]> => {
    const params = new URLSearchParams();
    if (filters?.userId !== undefined) params.append('userId', String(filters.userId));
    if (filters?.teamId !== undefined) params.append('teamId', String(filters.teamId));
    if (filters?.fiscalYear !== undefined) params.append('fiscalYear', String(filters.fiscalYear));
    if (filters?.isSubmitted !== undefined) params.append('isSubmitted', String(filters.isSubmitted));
    const query = params.toString() ? `?${params}` : '';
    const res = await apiClient.get<SalesForecastDto[]>(`${BASE}${query}`);
    return res.data;
  },

  /** GET /api/sales-forecasts/{id} */
  getById: async (id: number): Promise<SalesForecastDto> => {
    const res = await apiClient.get<SalesForecastDto>(`${BASE}/${id}`);
    return res.data;
  },

  /** POST /api/sales-forecasts */
  create: async (dto: SalesForecastInput): Promise<SalesForecastDto> => {
    const res = await apiClient.post<SalesForecastDto>(`${BASE}`, dto);
    return res.data;
  },

  /** PUT /api/sales-forecasts/{id} */
  update: async (id: number, dto: SalesForecastInput): Promise<void> => {
    await apiClient.put(`${BASE}/${id}`, dto);
  },

  /** DELETE /api/sales-forecasts/{id} (soft delete) */
  remove: async (id: number): Promise<void> => {
    await apiClient.delete(`${BASE}/${id}`);
  },

  /** POST /api/sales-forecasts/{id}/submit */
  submit: async (id: number): Promise<void> => {
    await apiClient.post(`${BASE}/${id}/submit`);
  },

  /** GET /api/sales-forecasts/history?period=&userId= */
  getHistory: async (period: string, userId?: number): Promise<ForecastHistoryDto[]> => {
    const params = new URLSearchParams();
    params.append('period', period);
    if (userId !== undefined) params.append('userId', String(userId));
    const res = await apiClient.get<ForecastHistoryDto[]>(`${BASE}/history?${params}`);
    return res.data;
  },

  /** POST /api/sales-forecasts/{id}/snapshot */
  createSnapshot: async (id: number): Promise<ForecastHistoryDto> => {
    const res = await apiClient.post<ForecastHistoryDto>(`${BASE}/${id}/snapshot`);
    return res.data;
  },

  /** GET /api/sales-forecasts/{forecastId}/line-items */
  getLineItems: async (forecastId: number): Promise<ForecastLineItemDto[]> => {
    const res = await apiClient.get<ForecastLineItemDto[]>(`${BASE}/${forecastId}/line-items`);
    return res.data;
  },
};

export default salesForecastService;

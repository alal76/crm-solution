// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
import apiClient from './apiClient';

// ── Enums ─────────────────────────────────────────────────────────────────────

export enum SurveyType {
  CSAT = 0,
  NPS = 1,
  CES = 2,
}

export enum SurveyStatus {
  Pending = 0,
  Sent = 1,
  Responded = 2,
  Expired = 3,
  Cancelled = 4,
}

export enum SentimentType {
  VeryPositive = 0,
  Positive = 1,
  Neutral = 2,
  Negative = 3,
  VeryNegative = 4,
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

export interface SatisfactionSurveyDto {
  id: number;
  entityType: string;
  entityId: number;
  type: SurveyType;
  status: SurveyStatus;
  contactId?: number;
  contactName?: string;
  accountId?: number;
  sentAt?: string;
  expiresAt?: string;
  responseReceivedAt?: string;
  score?: number;
  comment?: string;
  sentiment?: SentimentType;
  subject?: string;
  createdAt: string;
}

export interface CreateSatisfactionSurveyDto {
  entityType: string;
  entityId: number;
  type: SurveyType;
  contactId?: number;
  accountId?: number;
  subject?: string;
}

export interface SatisfactionResponseDto {
  id: number;
  surveyId: number;
  score: number;
  comment?: string;
  sentiment: SentimentType;
  respondedAt: string;
}

export interface SubmitSatisfactionResponseDto {
  surveyToken: string;
  score: number;
  comment?: string;
}

export interface MonthlyMetricDto {
  month: string;
  averageScore: number;
  count: number;
}

export interface SatisfactionMetricsDto {
  averageCSATScore: number;
  npsScore: number;
  totalSurveys: number;
  totalResponses: number;
  responseRate: number;
  byMonth: MonthlyMetricDto[];
  scoreDistribution: Record<number, number>;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Service ───────────────────────────────────────────────────────────────────

const BASE = '/satisfaction';

const satisfactionService = {
  getSurveys: async (
    page = 1,
    pageSize = 20,
    entityType?: string,
  ): Promise<PagedResult<SatisfactionSurveyDto>> => {
    const params: Record<string, unknown> = { page, pageSize };
    if (entityType) params.entityType = entityType;
    const res = await apiClient.get<PagedResult<SatisfactionSurveyDto>>(BASE, { params });
    return res.data;
  },

  getSurveyById: async (id: number): Promise<SatisfactionSurveyDto> => {
    const res = await apiClient.get<SatisfactionSurveyDto>(`${BASE}/${id}`);
    return res.data;
  },

  createSurvey: async (dto: CreateSatisfactionSurveyDto): Promise<SatisfactionSurveyDto> => {
    const res = await apiClient.post<SatisfactionSurveyDto>(BASE, dto);
    return res.data;
  },

  submitResponse: async (dto: SubmitSatisfactionResponseDto): Promise<SatisfactionResponseDto> => {
    const res = await apiClient.post<SatisfactionResponseDto>(`${BASE}/respond`, dto);
    return res.data;
  },

  getMetrics: async (
    from?: string,
    to?: string,
    entityType?: string,
  ): Promise<SatisfactionMetricsDto> => {
    const params: Record<string, unknown> = {};
    if (from) params.from = from;
    if (to) params.to = to;
    if (entityType) params.entityType = entityType;
    const res = await apiClient.get<SatisfactionMetricsDto>(`${BASE}/metrics`, { params });
    return res.data;
  },

  getNPS: async (from?: string, to?: string): Promise<number> => {
    const params: Record<string, unknown> = {};
    if (from) params.from = from;
    if (to) params.to = to;
    const res = await apiClient.get<{ npsScore: number }>(`${BASE}/nps`, { params });
    return res.data.npsScore;
  },

  getCSAT: async (from?: string, to?: string): Promise<number> => {
    const params: Record<string, unknown> = {};
    if (from) params.from = from;
    if (to) params.to = to;
    const res = await apiClient.get<{ csatScore: number }>(`${BASE}/csat`, { params });
    return res.data.csatScore;
  },

  /** Public call - no auth token required */
  getSurveyByToken: async (token: string): Promise<SatisfactionSurveyDto | null> => {
    try {
      const res = await apiClient.get<SatisfactionSurveyDto>(`${BASE}`, {
        params: { token },
      });
      return res.data;
    } catch {
      return null;
    }
  },
};

export default satisfactionService;

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Lead Service — wraps the real `/api/leads` Lead endpoints
// (CRM.Api.Controllers.LeadsController). REM-ORPHAN-003: replaces the legacy
// "Contacts-as-Leads" flow (`/contacts/type/Lead`) that LeadsPage previously used.
//
// DTO shapes mirror CRM.Core.Dtos.LeadDtos (LeadSummaryDto/LeadDto) and the
// request DTOs declared inline in LeadsController.cs (CreateLeadDto, UpdateLeadDto,
// ConvertLeadDto, etc.) — read those files fresh if the backend contract changes.

import apiClient from './apiClient';

// ── Response DTOs (mirror CRM.Core.Dtos.LeadDtos) ─────────────────────────────────

/** Summary projection — used in list (GET /leads) and status-filtered responses. */
export interface LeadSummaryDto {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  companyName?: string;
  title?: string;
  /** LeadLifecycleStatus name: New | Working | Nurturing | Qualified | Disqualified | Converted */
  status: string;
  /** Configurable status FK (ENUM-MIG-001) */
  statusId?: number;
  /** LeadSource name: Web | Campaign | Referral | Event | Partner | Manual */
  source: string;
  score: number;
  fitScore: number;
  engagementScore: number;
  ownerId?: number;
  createdAt: string;
  updatedAt?: string;
  territoryId?: number;
  qualificationFrameworkType: string;
  nurtureCampaignId?: number;
  lastContactedAt?: string;
  daysSinceLastContact?: number;
}

/** Full detail projection — used in GET /leads/{id}. */
export interface LeadDto extends LeadSummaryDto {
  qualificationNotes?: string;
  region?: string;
  website?: string;
  tags?: string;
  accountId?: number;
  contactId?: number;
  campaignId?: number;
  mqlDate?: string;
  sqlDate?: string;
  lastActivityDate?: string;

  // Source Attribution (TODO-CRM002-03)
  leadSourceId?: number;
  originalSource?: string;
  firstTouchDate?: string;
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;

  // BANT Qualification Scoring (TODO-CRM002-08)
  budgetScore?: number;
  authorityScore?: number;
  needScore?: number;
  timelineScore?: number;

  // MEDDIC Qualification Scoring (TODO-CRM002-08)
  metricsScore?: number;
  economicBuyerScore?: number;
  decisionCriteriaScore?: number;
  decisionProcessScore?: number;
  identifyPainScore?: number;
  championScore?: number;
  customQualificationJson?: string;

  // Nurturing (TODO-CRM002-06)
  nurtureCampaignEnrolledAt?: string;
  lastScoreDecayDate?: string;
}

export interface LeadsListResponse {
  data: LeadSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LeadSourceAnalyticsDto {
  source: string;
  totalLeads: number;
  convertedLeads: number;
  qualifiedLeads: number;
  disqualifiedLeads: number;
  conversionRate: number;
  averageScore: number;
}

export interface LeadAttributionDto {
  utmSource?: string;
  utmMedium?: string;
  utmCampaign?: string;
  totalLeads: number;
  convertedLeads: number;
  conversionRate: number;
  averageScore: number;
}

export interface LeadAgingAlertDto {
  leadId: number;
  leadName: string;
  assignedToUserId?: number;
  daysSinceLastActivity: number;
  lastActivityDate?: string;
  /** "Warning" or "Critical" */
  stalenessLevel: string;
}

export interface CheckDuplicateLeadResponse {
  isDuplicate: boolean;
  existingLeadId?: number;
  /** "email" or "name" indicating which field(s) matched */
  matchedOn?: string;
}

export interface LeadQualificationResult {
  leadId: number;
  framework: string;
  combinedScore: number;
  dimensionScores: Record<string, number>;
  qualificationLevel: string;
  recommendations: string[];
  scoredAt: string;
}

// ── Request DTOs (mirror the controller-local request classes in LeadsController.cs) ──

/** POST /leads body. NOTE: does not accept tags/mqlDate/sqlDate — backend has no write path for those yet. */
export interface CreateLeadDto {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  company?: string;
  companyName?: string;
  title?: string;
  source?: string;
  region?: string;
  website?: string;
  notes?: string;
  description?: string;
  ownerId?: number;
  campaignId?: number;
  status?: number;
  statusId?: number;
}

/** PUT /leads/{id} body. NOTE: does not accept tags/mqlDate/sqlDate — backend has no write path for those yet. */
export interface UpdateLeadDto {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  companyName?: string;
  title?: string;
  /** LeadLifecycleStatus name, parsed case-sensitively server-side */
  status?: string;
  /** LeadSource name, parsed case-sensitively server-side */
  source?: string;
  region?: string;
  website?: string;
  notes?: string;
  score?: number;
  ownerId?: number;
  campaignId?: number;
  statusId?: number;
}

export interface ConvertLeadDto {
  opportunityName?: string;
  /**
   * Existing Account to attach the new Opportunity to. The backend does NOT create an
   * Account during conversion (unlike the legacy client-side flow) — if this is omitted
   * and the lead has no AccountId already matched, the Opportunity is created with
   * AccountId=0, which is invalid. Callers should always supply this.
   */
  accountId?: number;
  estimatedValue?: number;
  expectedCloseDate?: string;
}

export interface ConvertLeadResult {
  message: string;
  opportunityId: number;
  leadId: number;
}

export interface AssignNurtureCampaignDto {
  campaignId: number;
}

/**
 * BANT + MEDDIC qualification payload. If any MEDDIC string field is provided the
 * MEDDIC framework is used server-side; otherwise BANT boolean scores are applied.
 */
export interface LeadQualificationDto {
  hasBudget: boolean;
  hasAuthority: boolean;
  hasNeed: boolean;
  hasTimeline: boolean;
  metrics?: string;
  economicBuyer?: string;
  decisionCriteria?: string;
  decisionProcess?: string;
  identifyPain?: string;
  champion?: string;
}

// ── Service ────────────────────────────────────────────────────────────────────────
//
// Methods return the raw Axios response (matching this repo's accountService.ts
// convention), so callers access `.data`.

const leadService = {
  // === CRUD ===

  /** GET /leads?page=&pageSize= */
  getAll: (page = 1, pageSize = 1000) =>
    apiClient.get<LeadsListResponse>('/leads', { params: { page, pageSize } }),

  /** GET /leads/{id} */
  getById: (id: number) => apiClient.get<LeadDto>(`/leads/${id}`),

  /** POST /leads */
  create: (dto: CreateLeadDto) =>
    apiClient.post<{ id: number; message: string }>('/leads', dto),

  /** PUT /leads/{id} */
  update: (id: number, dto: UpdateLeadDto) =>
    apiClient.put<{ message: string }>(`/leads/${id}`, dto),

  /** DELETE /leads/{id} (soft delete) */
  delete: (id: number) => apiClient.delete<{ message: string }>(`/leads/${id}`),

  // === Conversion ===

  /** POST /leads/{id}/convert — creates an Opportunity and marks the lead Converted. */
  convert: (id: number, dto: ConvertLeadDto) =>
    apiClient.post<ConvertLeadResult>(`/leads/${id}/convert`, dto),

  // === Duplicate check ===

  /** GET /leads/check-duplicate?email=&firstName=&lastName=&company= */
  checkDuplicate: (params: { email?: string; firstName?: string; lastName?: string; company?: string }) =>
    apiClient.get<CheckDuplicateLeadResponse>('/leads/check-duplicate', { params }),

  // === Status / stats / analytics ===

  /** GET /leads/status/{status} */
  getByStatus: (status: string) => apiClient.get<LeadSummaryDto[]>(`/leads/status/${status}`),

  /** GET /leads/stats */
  getStats: () => apiClient.get<Record<string, unknown>>('/leads/stats'),

  /** GET /leads/analytics/sources */
  getSourceAnalytics: () => apiClient.get<LeadSourceAnalyticsDto[]>('/leads/analytics/sources'),

  /** GET /leads/analytics/attribution */
  getAttributionAnalytics: () => apiClient.get<LeadAttributionDto[]>('/leads/analytics/attribution'),

  /** GET /leads/aging-alerts?staleDays= */
  getAgingAlerts: (staleDays = 14) =>
    apiClient.get<LeadAgingAlertDto[]>('/leads/aging-alerts', { params: { staleDays } }),

  // === Nurture campaigns ===

  /** POST /leads/{id}/nurture */
  assignNurtureCampaign: (id: number, campaignId: number) =>
    apiClient.post<{ message: string; leadId: number; campaignId: number }>(
      `/leads/${id}/nurture`,
      { campaignId } as AssignNurtureCampaignDto
    ),

  /** GET /leads/{id}/nurture-campaigns */
  getNurtureCampaigns: (id: number) =>
    apiClient.get<Array<{ id: number; name: string }>>(`/leads/${id}/nurture-campaigns`),

  /** DELETE /leads/{id}/nurture-campaigns/{campaignId} */
  removeNurtureCampaign: (id: number, campaignId: number) =>
    apiClient.delete<{ message: string }>(`/leads/${id}/nurture-campaigns/${campaignId}`),

  // === Qualification (BANT/MEDDIC) ===

  /** POST /leads/{id}/qualify */
  qualify: (id: number, dto: LeadQualificationDto) =>
    apiClient.post<LeadQualificationResult>(`/leads/${id}/qualify`, dto),
};

export default leadService;
export { leadService };

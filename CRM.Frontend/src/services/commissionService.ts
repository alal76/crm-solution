/**
 * Commission Service - Manages commissions, plans, tiers, statements, and payouts
 * 
 * This service provides methods for managing commissions including:
 * - CRUD operations for commissions
 * - Status management (approve, reject, mark paid, clawback)
 * - Commission calculation for deals/orders
 * - Commission plans and tiers management
 * - Statements generation and finalization
 * - Statistics, leaderboards, and forecasting
 */
import apiClient from './apiClient';

// Raw API response data type (JSON object with unknown field types)
type RawApiData = Record<string, unknown>;

// ============================================================================
// DATA NORMALIZATION
// ============================================================================
// Backend returns different property names. These helpers ensure safe data access.

const normalizeCommission = (raw: RawApiData): Commission => ({
  ...(raw as unknown as Commission),
  quotaAttainmentAtTime: ((raw['quotaAttainmentAtTime'] ?? raw['attainmentPercent']) ?? 0) as number,
  tierLevel: (raw['tierLevel'] ?? 0) as number,
  quotaPeriod: (raw['quotaPeriod'] ?? '') as string,
  originalAmount: (raw['originalAmount'] ?? 0) as number,
  commissionRate: (raw['commissionRate'] ?? 0) as number,
  commissionAmount: (raw['commissionAmount'] ?? 0) as number,
  dealAmount: (raw['dealAmount'] ?? 0) as number,
  commissionableAmount: (raw['commissionableAmount'] ?? 0) as number,
  finalCommissionAmount: (raw['finalCommissionAmount'] ?? 0) as number,
  splitPercent: (raw['splitPercent'] ?? 100) as number,
});

const normalizeCommissionTier = (raw: RawApiData): CommissionTier => ({
  ...(raw as unknown as CommissionTier),
  maxValue: ((raw['maxValue'] ?? raw['maxAttainmentPercent']) ?? 100) as number,
  minValue: ((raw['minValue'] ?? raw['minAttainmentPercent']) ?? 0) as number,
  commissionRate: (raw['commissionRate'] ?? 0) as number,
  fixedAmount: (raw['fixedAmount'] ?? 0) as number,
  multiplier: (raw['multiplier'] ?? 1) as number,
  tierOrder: (raw['tierOrder'] ?? 0) as number,
});

const normalizeStatement = (raw: RawApiData): CommissionStatement => ({
  ...(raw as unknown as CommissionStatement),
  finalizedBy: (raw['finalizedBy'] ?? undefined) as number | undefined,
  notes: (raw['notes'] ?? '') as string,
  totalEarned: (raw['totalEarned'] ?? 0) as number,
  totalAmount: ((raw['totalAmount'] ?? raw['totalEarned']) ?? 0) as number,
  totalAdjustments: (raw['totalAdjustments'] ?? 0) as number,
  totalClawbacks: (raw['totalClawbacks'] ?? 0) as number,
  netPayout: (raw['netPayout'] ?? 0) as number,
});

const normalizePlan = (raw: RawApiData): CommissionPlan => ({
  ...(raw as unknown as CommissionPlan),
  baseRate: (raw['baseRate'] ?? 0) as number,
  tiers: ((raw['tiers'] as RawApiData[] | undefined) || []).map(normalizeCommissionTier),
});

const normalizeStatistics = (raw: RawApiData): CommissionStatistics => ({
  ...(raw as unknown as CommissionStatistics),
  totalCommissions: (raw['totalCommissions'] ?? 0) as number,
  totalPaid: (raw['totalPaid'] ?? 0) as number,
  totalPending: (raw['totalPending'] ?? 0) as number,
  totalRecords: (raw['totalRecords'] ?? 0) as number,
  pendingApprovals: (raw['pendingApprovals'] ?? 0) as number,
  averageCommission: (raw['averageCommission'] ?? 0) as number,
  activePlans: (raw['activePlans'] ?? 0) as number,
  commissionsByPlan: (raw['commissionsByPlan'] ?? {}) as Record<string, number>,
});

const normalizeLeaderboard = (raw: RawApiData): CommissionLeaderboard => ({
  ...(raw as unknown as CommissionLeaderboard),
  rank: (raw['rank'] ?? 0) as number,
  totalEarned: (raw['totalEarned'] ?? 0) as number,
  dealCount: (raw['dealCount'] ?? 0) as number,
  averageDealSize: (raw['averageDealSize'] ?? 0) as number,
});

const normalizeCommissionSummary = (raw: RawApiData): CommissionSummary => ({
  ...(raw as unknown as CommissionSummary),
  totalEarned: (raw['totalEarned'] ?? 0) as number,
  totalPaid: (raw['totalPaid'] ?? 0) as number,
  totalPending: (raw['totalPending'] ?? 0) as number,
  totalClawedBack: (raw['totalClawedBack'] ?? 0) as number,
  dealCount: (raw['dealCount'] ?? 0) as number,
  averageCommission: (raw['averageCommission'] ?? 0) as number,
  commissions: ((raw['commissions'] as RawApiData[] | undefined) || []).map(normalizeCommission),
});

const normalizeForecast = (raw: RawApiData): CommissionForecast => ({
  ...(raw as unknown as CommissionForecast),
  currentEarned: (raw['currentEarned'] ?? 0) as number,
  forecastedEarnings: (raw['forecastedEarnings'] ?? 0) as number,
  pipelineValue: (raw['pipelineValue'] ?? 0) as number,
  expectedFromPipeline: (raw['expectedFromPipeline'] ?? 0) as number,
  quotaProgress: (raw['quotaProgress'] ?? 0) as number,
  projectedQuotaAttainment: (raw['projectedQuotaAttainment'] ?? 0) as number,
  forecastedDeals: ((raw['forecastedDeals'] as RawApiData[] | undefined) || []).map((d) => ({
    opportunityId: (d['opportunityId'] ?? 0) as number,
    opportunityName: (d['opportunityName'] ?? '') as string,
    dealValue: (d['dealValue'] ?? 0) as number,
    expectedCommission: (d['expectedCommission'] ?? 0) as number,
    probability: (d['probability'] ?? 0) as number,
    expectedCloseDate: d['expectedCloseDate'] as string | undefined,
  })),
});

const normalizeCalculation = (raw: RawApiData): CommissionCalculation => ({
  ...(raw as unknown as CommissionCalculation),
  baseAmount: (raw['baseAmount'] ?? 0) as number,
  commissionRate: (raw['commissionRate'] ?? 0) as number,
  calculatedAmount: (raw['calculatedAmount'] ?? 0) as number,
  accelerator: (raw['accelerator'] ?? 1) as number,
  finalAmount: (raw['finalAmount'] ?? 0) as number,
  tierLevel: (raw['tierLevel'] ?? 0) as number,
  breakdown: ((raw['breakdown'] as RawApiData[] | undefined) || []).map((b) => ({
    description: (b['description'] ?? '') as string,
    amount: (b['amount'] ?? 0) as number,
    rate: (b['rate'] ?? 0) as number,
    result: (b['result'] ?? 0) as number,
  })),
});

// ============================================================================
// Enums
// ============================================================================

export enum CommissionType {
  FlatPercentage = 0,
  TieredPercentage = 1,
  FixedAmount = 2,
  TieredAmount = 3,
  MarginBased = 4,
  Custom = 5,
}

export enum CommissionTrigger {
  OnClose = 0,
  OnOrder = 1,
  OnInvoice = 2,
  OnPayment = 3,
  OnSubscriptionStart = 4,
  OnSignature = 5,
  Monthly = 6,
}

export enum CommissionStatus {
  Pending = 0,
  Approved = 1,
  Held = 2,
  Paid = 3,
  ClawedBack = 4,
  Adjusted = 5,
  Cancelled = 6,
}

export enum CommissionPlanStatus {
  Draft = 0,
  Active = 1,
  Inactive = 2,
  Archived = 3,
}

export enum CommissionStatementStatus {
  Draft = 0,
  PendingReview = 1,
  Finalized = 2,
  Paid = 3,
  // Aliases used by UI components
  PendingApproval = 1,
  Approved = 2,
  Disputed = 4,
  Voided = 5,
}

// ============================================================================
// Interfaces
// ============================================================================

export interface Commission {
  id: number;
  userId: number;
  user?: {
    id: number;
    username?: string;
    firstName?: string;
    lastName?: string;
    email?: string;
  };
  commissionPlanId?: number;
  commissionPlan?: CommissionPlan;
  opportunityId?: number;
  opportunity?: {
    id: number;
    name: string;
    amount?: number;
    stage?: string;
  };
  orderId?: number;
  invoiceId?: number;
  subscriptionId?: number;
  dealAmount: number;
  commissionableAmount: number;
  commissionRate: number;
  commissionAmount: number;
  splitPercent: number;
  finalCommissionAmount: number;
  currencyCode?: string;
  status: CommissionStatus;
  tierLevel?: number;
  tierName?: string;
  quotaAttainmentAtTime?: number;
  quotaAmount?: number;
  quotaPeriod?: string;
  approvedById?: number;
  approvedAt?: string;
  paidAt?: string;
  clawbackDate?: string;
  clawbackReason?: string;
  adjustmentReason?: string;
  originalAmount?: number;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CommissionPlan {
  id: number;
  name: string;
  code?: string;
  description?: string;
  status: CommissionPlanStatus;
  effectiveStartDate?: string;
  effectiveEndDate?: string;
  fiscalYear?: number;
  commissionType: CommissionType;
  baseRate: number;
  trigger: CommissionTrigger;
  clawbackPeriodDays?: number;
  minDealSize?: number;
  maxCommissionPerDeal?: number;
  maxCommissionPerPeriod?: number;
  allowSplits: boolean;
  defaultOverlayPercent?: number;
  tiers?: CommissionTier[];
  createdAt?: string;
  updatedAt?: string;
}

export interface CommissionTier {
  id: number;
  commissionPlanId: number;
  name?: string;
  tierOrder: number;
  minValue?: number;
  maxValue?: number;
  minAttainmentPercent?: number;
  maxAttainmentPercent?: number;
  commissionRate?: number;
  fixedAmount?: number;
  multiplier: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CommissionStatement {
  id: number;
  userId: number;
  user?: {
    id: number;
    username?: string;
    firstName?: string;
    lastName?: string;
    email?: string;
  };
  statementNumber?: string;
  period?: string;
  periodStartDate: string;
  periodEndDate: string;
  // Aliases used by UI
  periodStart?: string;
  periodEnd?: string;
  totalEarned: number;
  totalAdjustments: number;
  totalClawbacks: number;
  netPayout: number;
  // Aliases used by UI
  totalAmount?: number;
  adjustments?: number;
  netAmount?: number;
  userName?: string;
  status: CommissionStatementStatus;
  finalizedAt?: string;
  finalizedBy?: number;
  paidAt?: string;
  isPaid: boolean;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CommissionCalculation {
  userId: number;
  opportunityId?: number;
  orderId?: number;
  planId?: number;
  planName?: string;
  baseAmount: number;
  commissionRate: number;
  calculatedAmount: number;
  accelerator?: number;
  finalAmount: number;
  tierLevel?: number;
  tierName?: string;
  breakdown: CommissionBreakdown[];
}

export interface CommissionBreakdown {
  description: string;
  amount: number;
  rate: number;
  result: number;
}

export interface CommissionSummary {
  userId: number;
  fromDate: string;
  toDate: string;
  totalEarned: number;
  totalPaid: number;
  totalPending: number;
  totalClawedBack: number;
  dealCount: number;
  averageCommission: number;
  commissions: Commission[];
}

export interface CommissionStatistics {
  totalCommissions: number;
  totalPaid: number;
  totalPending: number;
  totalRecords: number;
  pendingApprovals: number;
  averageCommission: number;
  activePlans: number;
  commissionsByPlan: Record<string, number>;
}

export interface CommissionLeaderboard {
  rank: number;
  userId: number;
  userName: string;
  totalEarned: number;
  dealCount: number;
  averageDealSize: number;
}

export interface CommissionForecast {
  userId: number;
  currentEarned: number;
  forecastedEarnings: number;
  pipelineValue: number;
  expectedFromPipeline: number;
  quotaProgress: number;
  projectedQuotaAttainment: number;
  forecastedDeals: ForecastedDeal[];
}

export interface ForecastedDeal {
  opportunityId: number;
  opportunityName: string;
  dealValue: number;
  expectedCommission: number;
  probability: number;
  expectedCloseDate?: string;
}

// ============================================================================
// Request Types
// ============================================================================

export interface CommissionCreateRequest {
  userId: number;
  commissionPlanId?: number;
  opportunityId?: number;
  orderId?: number;
  invoiceId?: number;
  subscriptionId?: number;
  dealAmount: number;
  commissionableAmount?: number;
  commissionRate: number;
  commissionAmount: number;
  splitPercent?: number;
  finalCommissionAmount?: number;
  currencyCode?: string;
  notes?: string;
}

export interface CommissionUpdateRequest {
  dealAmount?: number;
  commissionableAmount?: number;
  commissionRate?: number;
  commissionAmount?: number;
  splitPercent?: number;
  finalCommissionAmount?: number;
  notes?: string;
}

export interface CommissionPlanCreateRequest {
  name: string;
  code?: string;
  description?: string;
  effectiveStartDate?: string;
  effectiveEndDate?: string;
  fiscalYear?: number;
  commissionType?: CommissionType;
  baseRate?: number;
  trigger?: CommissionTrigger;
  clawbackPeriodDays?: number;
  minDealSize?: number;
  maxCommissionPerDeal?: number;
  maxCommissionPerPeriod?: number;
  allowSplits?: boolean;
  defaultOverlayPercent?: number;
}

export interface CommissionPlanUpdateRequest {
  name?: string;
  code?: string;
  description?: string;
  status?: CommissionPlanStatus;
  effectiveStartDate?: string;
  effectiveEndDate?: string;
  fiscalYear?: number;
  commissionType?: CommissionType;
  baseRate?: number;
  trigger?: CommissionTrigger;
  clawbackPeriodDays?: number;
  minDealSize?: number;
  maxCommissionPerDeal?: number;
  maxCommissionPerPeriod?: number;
  allowSplits?: boolean;
  defaultOverlayPercent?: number;
}

export interface CommissionTierCreateRequest {
  commissionPlanId?: number;
  name?: string;
  tierOrder?: number;
  minValue?: number;
  maxValue?: number;
  minAttainmentPercent?: number;
  maxAttainmentPercent?: number;
  commissionRate?: number;
  fixedAmount?: number;
  multiplier?: number;
}

export interface CommissionTierUpdateRequest {
  planId?: number;
  name?: string;
  tierOrder?: number;
  minValue?: number;
  maxValue?: number;
  minAttainmentPercent?: number;
  maxAttainmentPercent?: number;
  commissionRate?: number;
  fixedAmount?: number;
  multiplier?: number;
}

export interface CommissionStatementGenerateRequest {
  userId: number;
  fromDate?: string;
  toDate?: string;
  periodStart?: string;
  periodEnd?: string;
}

// ============================================================================
// Commission API
// ============================================================================

const API_BASE = '/commissions';

/**
 * Get all commissions with optional filtering.
 */
export const getCommissions = async (
  userId?: number,
  status?: CommissionStatus
): Promise<Commission[]> => {
  const params = new URLSearchParams();
  if (userId !== undefined) params.append('userId', userId.toString());
  if (status !== undefined) params.append('status', status.toString());
  
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}?${params}`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get a commission by ID.
 */
export const getCommissionById = async (id: number): Promise<Commission> => {
  const response = await apiClient.get<RawApiData>(`${API_BASE}/${id}`);
  return normalizeCommission(response.data);
};

/**
 * Create a new commission.
 */
export const createCommission = async (
  request: CommissionCreateRequest
): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(API_BASE, request);
  return normalizeCommission(response.data);
};

/**
 * Update a commission.
 */
export const updateCommission = async (
  id: number,
  request: CommissionUpdateRequest
): Promise<Commission> => {
  const response = await apiClient.put<RawApiData>(`${API_BASE}/${id}`, request);
  return normalizeCommission(response.data);
};

/**
 * Delete a commission.
 */
export const deleteCommission = async (id: number): Promise<void> => {
  await apiClient.delete(`${API_BASE}/${id}`);
};

// ============================================================================
// Status Management
// ============================================================================

/**
 * Update commission status.
 */
export const updateCommissionStatus = async (
  id: number,
  status: CommissionStatus
): Promise<Commission> => {
  const response = await apiClient.patch<RawApiData>(`${API_BASE}/${id}/status`, { status });
  return normalizeCommission(response.data);
};

/**
 * Approve a commission for payout.
 */
export const approveCommission = async (
  id: number,
  approvedById: number
): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/${id}/approve`, { approvedById });
  return normalizeCommission(response.data);
};

/**
 * Reject a commission.
 */
export const rejectCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/${id}/reject`, { reason });
  return normalizeCommission(response.data);
};

/**
 * Mark a commission as paid.
 */
export const markCommissionPaid = async (
  id: number,
  paidDate?: string
): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/${id}/mark-paid`, { paidDate });
  return normalizeCommission(response.data);
};

/**
 * Claw back a paid commission.
 */
export const clawbackCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/${id}/clawback`, { reason });
  return normalizeCommission(response.data);
};

// ============================================================================
// Commission Calculation
// ============================================================================

/**
 * Calculate commission for a deal/opportunity.
 */
export const calculateForDeal = async (opportunityId: number): Promise<CommissionCalculation> => {
  const response = await apiClient.get<RawApiData>(
    `${API_BASE}/calculate/deal/${opportunityId}`
  );
  return normalizeCalculation(response.data);
};

/**
 * Calculate commission for an order.
 */
export const calculateForOrder = async (orderId: number): Promise<CommissionCalculation> => {
  const response = await apiClient.get<RawApiData>(
    `${API_BASE}/calculate/order/${orderId}`
  );
  return normalizeCalculation(response.data);
};

/**
 * Recalculate a commission after changes.
 */
export const recalculateCommission = async (id: number): Promise<Commission> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/${id}/recalculate`);
  return normalizeCommission(response.data);
};

// ============================================================================
// Queries
// ============================================================================

/**
 * Get commissions for a specific user.
 */
export const getCommissionsByUser = async (
  userId: number,
  fromDate?: string,
  toDate?: string
): Promise<Commission[]> => {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/user/${userId}?${params}`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get pending commissions awaiting approval.
 */
export const getPendingApprovals = async (): Promise<Commission[]> => {
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/pending-approvals`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get commissions ready for payout.
 */
export const getReadyForPayout = async (): Promise<Commission[]> => {
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/ready-for-payout`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get commission statistics.
 */
export const getStatistics = async (
  fromDate?: string,
  toDate?: string
): Promise<CommissionStatistics> => {
  const params = new URLSearchParams();
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  
  const response = await apiClient.get<RawApiData>(`${API_BASE}/statistics?${params}`);
  return normalizeStatistics(response.data);
};

/**
 * Get commission leaderboard.
 */
export const getLeaderboard = async (
  topN: number = 10,
  fromDate?: string,
  toDate?: string
): Promise<CommissionLeaderboard[]> => {
  const params = new URLSearchParams();
  params.append('topN', topN.toString());
  if (fromDate) params.append('fromDate', fromDate);
  if (toDate) params.append('toDate', toDate);
  
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/leaderboard?${params}`);
  return (response.data || []).map(normalizeLeaderboard);
};

/**
 * Get commission forecast for a user.
 */
export const getForecast = async (
  userId: number,
  asOfDate?: string
): Promise<CommissionForecast> => {
  const params = new URLSearchParams();
  if (asOfDate) params.append('asOfDate', asOfDate);
  
  const response = await apiClient.get<RawApiData>(`${API_BASE}/forecast/${userId}?${params}`);
  return normalizeForecast(response.data);
};

/**
 * Get commission summary for a period.
 */
export const getPeriodSummary = async (
  userId: number,
  fromDate: string,
  toDate: string
): Promise<CommissionSummary> => {
  const params = new URLSearchParams();
  params.append('fromDate', fromDate);
  params.append('toDate', toDate);
  
  const response = await apiClient.get<RawApiData>(`${API_BASE}/summary/${userId}?${params}`);
  return normalizeCommissionSummary(response.data);
};

// ============================================================================
// Commission Plans
// ============================================================================

/**
 * Get all commission plans.
 */
export const getPlans = async (isActive?: boolean): Promise<CommissionPlan[]> => {
  const params = new URLSearchParams();
  if (isActive !== undefined) params.append('isActive', isActive.toString());
  
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/plans?${params}`);
  return (response.data || []).map(normalizePlan);
};

/**
 * Get a commission plan by ID.
 */
export const getPlanById = async (planId: number): Promise<CommissionPlan> => {
  const response = await apiClient.get<RawApiData>(`${API_BASE}/plans/${planId}`);
  return normalizePlan(response.data);
};

/**
 * Create a commission plan.
 */
export const createPlan = async (request: CommissionPlanCreateRequest): Promise<CommissionPlan> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/plans`, request);
  return normalizePlan(response.data);
};

/**
 * Update a commission plan.
 */
export const updatePlan = async (
  planId: number,
  request: CommissionPlanUpdateRequest
): Promise<CommissionPlan> => {
  const response = await apiClient.put<RawApiData>(`${API_BASE}/plans/${planId}`, request);
  return normalizePlan(response.data);
};

/**
 * Delete a commission plan.
 */
export const deletePlan = async (planId: number): Promise<void> => {
  await apiClient.delete(`${API_BASE}/plans/${planId}`);
};

/**
 * Assign a plan to a user.
 */
export const assignPlanToUser = async (
  planId: number,
  userId: number,
  effectiveDate?: string
): Promise<{ message: string }> => {
  const response = await apiClient.post<{ message: string }>(
    `${API_BASE}/plans/${planId}/assign`,
    { userId, effectiveDate }
  );
  return response.data;
};

/**
 * Get the active plan for a user.
 */
export const getUserPlan = async (userId: number): Promise<CommissionPlan> => {
  const response = await apiClient.get<RawApiData>(`${API_BASE}/plans/user/${userId}`);
  return normalizePlan(response.data);
};

// ============================================================================
// Commission Tiers
// ============================================================================

/**
 * Get tiers for a commission plan.
 */
export const getTiers = async (planId: number): Promise<CommissionTier[]> => {
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/plans/${planId}/tiers`);
  return (response.data || []).map(normalizeCommissionTier);
};

/**
 * Add a tier to a plan.
 */
export const addTier = async (
  planId: number,
  request: CommissionTierCreateRequest
): Promise<CommissionTier> => {
  const response = await apiClient.post<RawApiData>(
    `${API_BASE}/plans/${planId}/tiers`,
    request
  );
  return normalizeCommissionTier(response.data);
};

/**
 * Update a tier.
 */
export const updateTier = async (
  tierId: number,
  request: CommissionTierUpdateRequest
): Promise<CommissionTier> => {
  const response = await apiClient.put<RawApiData>(`${API_BASE}/tiers/${tierId}`, request);
  return normalizeCommissionTier(response.data);
};

/**
 * Remove a tier from a plan.
 */
export const removeTier = async (tierId: number): Promise<void> => {
  await apiClient.delete(`${API_BASE}/tiers/${tierId}`);
};

// ============================================================================
// Commission Statements
// ============================================================================

/**
 * Generate a commission statement for a user.
 */
export const generateStatement = async (
  request: CommissionStatementGenerateRequest
): Promise<CommissionStatement> => {
  const response = await apiClient.post<RawApiData>(
    `${API_BASE}/statements/generate`,
    request
  );
  return normalizeStatement(response.data);
};

/**
 * Get statements, optionally filtered by user.
 */
export const getStatements = async (userId?: number): Promise<CommissionStatement[]> => {
  const url = userId ? `${API_BASE}/statements/user/${userId}` : `${API_BASE}/statements`;
  const response = await apiClient.get<RawApiData[]>(url);
  return (response.data || []).map(normalizeStatement);
};

/**
 * Get a statement by ID.
 */
export const getStatementById = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.get<RawApiData>(
    `${API_BASE}/statements/${statementId}`
  );
  return normalizeStatement(response.data);
};

/**
 * Finalize a statement for payout.
 */
export const finalizeStatement = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.post<RawApiData>(
    `${API_BASE}/statements/${statementId}/finalize`
  );
  return normalizeStatement(response.data);
};

/**
 * Clone a commission plan.
 */
export const clonePlan = async (planId: number): Promise<CommissionPlan> => {
  const response = await apiClient.post<RawApiData>(`${API_BASE}/plans/${planId}/clone`);
  return normalizePlan(response.data);
};

/**
 * Update plan status.
 */
export const updatePlanStatus = async (planId: number, status: CommissionPlanStatus): Promise<CommissionPlan> => {
  const response = await apiClient.patch<RawApiData>(`${API_BASE}/plans/${planId}/status`, { status });
  return normalizePlan(response.data);
};

/**
 * Create a tier for a plan (alias for addTier).
 */
export const createTier = async (planId: number, request: CommissionTierCreateRequest): Promise<CommissionTier> => {
  return addTier(planId, request);
};

/**
 * Delete a tier (alias for removeTier).
 */
export const deleteTier = async (_planId: number, tierId: number): Promise<void> => {
  return removeTier(tierId);
};

/**
 * Get commissions for a specific statement.
 */
export const getCommissionsForStatement = async (statementId: number): Promise<Commission[]> => {
  const response = await apiClient.get<RawApiData[]>(`${API_BASE}/statements/${statementId}/commissions`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Update statement status.
 */
export const updateStatementStatus = async (
  statementId: number,
  status: CommissionStatementStatus
): Promise<CommissionStatement> => {
  const response = await apiClient.patch<RawApiData>(
    `${API_BASE}/statements/${statementId}/status`,
    { status }
  );
  return normalizeStatement(response.data);
};

/**
 * Download statement as PDF.
 */
export const downloadStatementPdf = async (statementId: number): Promise<Blob> => {
  const response = await apiClient.get(`${API_BASE}/statements/${statementId}/pdf`, {
    responseType: 'blob',
  });
  return response.data as Blob;
};

// ============================================================================
// Export grouped service object for convenience
// ============================================================================

const commissionService = {
  // Commission CRUD
  getCommissions,
  getCommissionById,
  createCommission,
  updateCommission,
  deleteCommission,
  
  // Status management
  updateCommissionStatus,
  approveCommission,
  rejectCommission,
  markCommissionPaid,
  clawbackCommission,
  
  // Calculation
  calculateForDeal,
  calculateForOrder,
  recalculateCommission,
  
  // Queries
  getCommissionsByUser,
  getPendingApprovals,
  getReadyForPayout,
  getStatistics,
  getLeaderboard,
  getForecast,
  getPeriodSummary,
  
  // Plans
  getPlans,
  getPlanById,
  createPlan,
  updatePlan,
  deletePlan,
  assignPlanToUser,
  getUserPlan,
  
  // Tiers
  getTiers,
  addTier,
  updateTier,
  removeTier,
  createTier,
  deleteTier,
  
  // Statements
  generateStatement,
  getStatements,
  getStatementById,
  finalizeStatement,
  getCommissionsForStatement,
  updateStatementStatus,
  downloadStatementPdf,
  
  // Plan management
  clonePlan,
  updatePlanStatus,
};

export default commissionService;

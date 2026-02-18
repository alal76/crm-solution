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

// ============================================================================
// DATA NORMALIZATION
// ============================================================================
// Backend returns different property names. These helpers ensure safe data access.

const normalizeCommission = (c: any): Commission => ({
  ...c,
  quotaAttainmentAtTime: c.quotaAttainmentAtTime ?? c.attainmentPercent ?? 0,
  tierLevel: c.tierLevel ?? 0,
  quotaPeriod: c.quotaPeriod ?? '',
  originalAmount: c.originalAmount ?? 0,
  commissionRate: c.commissionRate ?? 0,
  commissionAmount: c.commissionAmount ?? 0,
  dealAmount: c.dealAmount ?? 0,
  commissionableAmount: c.commissionableAmount ?? 0,
  finalCommissionAmount: c.finalCommissionAmount ?? 0,
  splitPercent: c.splitPercent ?? 100,
});

const normalizeCommissionTier = (t: any): CommissionTier => ({
  ...t,
  maxValue: t.maxValue ?? t.maxAttainmentPercent ?? 100,
  minValue: t.minValue ?? t.minAttainmentPercent ?? 0,
  commissionRate: t.commissionRate ?? 0,
  fixedAmount: t.fixedAmount ?? 0,
  multiplier: t.multiplier ?? 1,
  tierOrder: t.tierOrder ?? 0,
});

const normalizeStatement = (s: any): CommissionStatement => ({
  ...s,
  finalizedBy: s.finalizedBy ?? null,
  notes: s.notes ?? '',
  totalEarned: s.totalEarned ?? 0,
  totalAmount: s.totalAmount ?? s.totalEarned ?? 0,
  totalAdjustments: s.totalAdjustments ?? 0,
  totalClawbacks: s.totalClawbacks ?? 0,
  netPayout: s.netPayout ?? 0,
});

const normalizePlan = (p: any): CommissionPlan => ({
  ...p,
  baseRate: p.baseRate ?? 0,
  tiers: (p.tiers || []).map(normalizeCommissionTier),
});

const normalizeStatistics = (s: any): CommissionStatistics => ({
  ...s,
  totalCommissions: s.totalCommissions ?? 0,
  totalPaid: s.totalPaid ?? 0,
  totalPending: s.totalPending ?? 0,
  totalRecords: s.totalRecords ?? 0,
  pendingApprovals: s.pendingApprovals ?? 0,
  averageCommission: s.averageCommission ?? 0,
  activePlans: s.activePlans ?? 0,
  commissionsByPlan: s.commissionsByPlan ?? {},
});

const normalizeLeaderboard = (l: any): CommissionLeaderboard => ({
  ...l,
  rank: l.rank ?? 0,
  totalEarned: l.totalEarned ?? 0,
  dealCount: l.dealCount ?? 0,
  averageDealSize: l.averageDealSize ?? 0,
});

const normalizeCommissionSummary = (s: any): CommissionSummary => ({
  ...s,
  totalEarned: s.totalEarned ?? 0,
  totalPaid: s.totalPaid ?? 0,
  totalPending: s.totalPending ?? 0,
  totalClawedBack: s.totalClawedBack ?? 0,
  dealCount: s.dealCount ?? 0,
  averageCommission: s.averageCommission ?? 0,
  commissions: (s.commissions || []).map(normalizeCommission),
});

const normalizeForecast = (f: any): CommissionForecast => ({
  ...f,
  currentEarned: f.currentEarned ?? 0,
  forecastedEarnings: f.forecastedEarnings ?? 0,
  pipelineValue: f.pipelineValue ?? 0,
  expectedFromPipeline: f.expectedFromPipeline ?? 0,
  quotaProgress: f.quotaProgress ?? 0,
  projectedQuotaAttainment: f.projectedQuotaAttainment ?? 0,
  forecastedDeals: (f.forecastedDeals || []).map((d: any) => ({
    ...d,
    dealValue: d.dealValue ?? 0,
    expectedCommission: d.expectedCommission ?? 0,
    probability: d.probability ?? 0,
  })),
});

const normalizeCalculation = (c: any): CommissionCalculation => ({
  ...c,
  baseAmount: c.baseAmount ?? 0,
  commissionRate: c.commissionRate ?? 0,
  calculatedAmount: c.calculatedAmount ?? 0,
  accelerator: c.accelerator ?? 1,
  finalAmount: c.finalAmount ?? 0,
  tierLevel: c.tierLevel ?? 0,
  breakdown: (c.breakdown || []).map((b: any) => ({
    ...b,
    amount: b.amount ?? 0,
    rate: b.rate ?? 0,
    result: b.result ?? 0,
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
  totalEarned: number;
  totalAdjustments: number;
  totalClawbacks: number;
  netPayout: number;
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
  fromDate: string;
  toDate: string;
}

// ============================================================================
// Commission API
// ============================================================================

const API_BASE = '/api/commissions';

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
  
  const response = await apiClient.get<Commission[]>(`${API_BASE}?${params}`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get a commission by ID.
 */
export const getCommissionById = async (id: number): Promise<Commission> => {
  const response = await apiClient.get<Commission>(`${API_BASE}/${id}`);
  return normalizeCommission(response.data);
};

/**
 * Create a new commission.
 */
export const createCommission = async (
  request: CommissionCreateRequest
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(API_BASE, request);
  return normalizeCommission(response.data);
};

/**
 * Update a commission.
 */
export const updateCommission = async (
  id: number,
  request: CommissionUpdateRequest
): Promise<Commission> => {
  const response = await apiClient.put<Commission>(`${API_BASE}/${id}`, request);
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
  const response = await apiClient.patch<Commission>(`${API_BASE}/${id}/status`, { status });
  return normalizeCommission(response.data);
};

/**
 * Approve a commission for payout.
 */
export const approveCommission = async (
  id: number,
  approvedById: number
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/approve`, { approvedById });
  return normalizeCommission(response.data);
};

/**
 * Reject a commission.
 */
export const rejectCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/reject`, { reason });
  return normalizeCommission(response.data);
};

/**
 * Mark a commission as paid.
 */
export const markCommissionPaid = async (
  id: number,
  paidDate?: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/mark-paid`, { paidDate });
  return normalizeCommission(response.data);
};

/**
 * Claw back a paid commission.
 */
export const clawbackCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/clawback`, { reason });
  return normalizeCommission(response.data);
};

// ============================================================================
// Commission Calculation
// ============================================================================

/**
 * Calculate commission for a deal/opportunity.
 */
export const calculateForDeal = async (opportunityId: number): Promise<CommissionCalculation> => {
  const response = await apiClient.get<CommissionCalculation>(
    `${API_BASE}/calculate/deal/${opportunityId}`
  );
  return normalizeCalculation(response.data);
};

/**
 * Calculate commission for an order.
 */
export const calculateForOrder = async (orderId: number): Promise<CommissionCalculation> => {
  const response = await apiClient.get<CommissionCalculation>(
    `${API_BASE}/calculate/order/${orderId}`
  );
  return normalizeCalculation(response.data);
};

/**
 * Recalculate a commission after changes.
 */
export const recalculateCommission = async (id: number): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/recalculate`);
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
  
  const response = await apiClient.get<Commission[]>(`${API_BASE}/user/${userId}?${params}`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get pending commissions awaiting approval.
 */
export const getPendingApprovals = async (): Promise<Commission[]> => {
  const response = await apiClient.get<Commission[]>(`${API_BASE}/pending-approvals`);
  return (response.data || []).map(normalizeCommission);
};

/**
 * Get commissions ready for payout.
 */
export const getReadyForPayout = async (): Promise<Commission[]> => {
  const response = await apiClient.get<Commission[]>(`${API_BASE}/ready-for-payout`);
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
  
  const response = await apiClient.get<CommissionStatistics>(`${API_BASE}/statistics?${params}`);
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
  
  const response = await apiClient.get<CommissionLeaderboard[]>(`${API_BASE}/leaderboard?${params}`);
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
  
  const response = await apiClient.get<CommissionForecast>(`${API_BASE}/forecast/${userId}?${params}`);
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
  
  const response = await apiClient.get<CommissionSummary>(`${API_BASE}/summary/${userId}?${params}`);
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
  
  const response = await apiClient.get<CommissionPlan[]>(`${API_BASE}/plans?${params}`);
  return (response.data || []).map(normalizePlan);
};

/**
 * Get a commission plan by ID.
 */
export const getPlanById = async (planId: number): Promise<CommissionPlan> => {
  const response = await apiClient.get<CommissionPlan>(`${API_BASE}/plans/${planId}`);
  return normalizePlan(response.data);
};

/**
 * Create a commission plan.
 */
export const createPlan = async (request: CommissionPlanCreateRequest): Promise<CommissionPlan> => {
  const response = await apiClient.post<CommissionPlan>(`${API_BASE}/plans`, request);
  return normalizePlan(response.data);
};

/**
 * Update a commission plan.
 */
export const updatePlan = async (
  planId: number,
  request: CommissionPlanUpdateRequest
): Promise<CommissionPlan> => {
  const response = await apiClient.put<CommissionPlan>(`${API_BASE}/plans/${planId}`, request);
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
  const response = await apiClient.get<CommissionPlan>(`${API_BASE}/plans/user/${userId}`);
  return normalizePlan(response.data);
};

// ============================================================================
// Commission Tiers
// ============================================================================

/**
 * Get tiers for a commission plan.
 */
export const getTiers = async (planId: number): Promise<CommissionTier[]> => {
  const response = await apiClient.get<CommissionTier[]>(`${API_BASE}/plans/${planId}/tiers`);
  return (response.data || []).map(normalizeCommissionTier);
};

/**
 * Add a tier to a plan.
 */
export const addTier = async (
  planId: number,
  request: CommissionTierCreateRequest
): Promise<CommissionTier> => {
  const response = await apiClient.post<CommissionTier>(
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
  const response = await apiClient.put<CommissionTier>(`${API_BASE}/tiers/${tierId}`, request);
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
  const response = await apiClient.post<CommissionStatement>(
    `${API_BASE}/statements/generate`,
    request
  );
  return normalizeStatement(response.data);
};

/**
 * Get statements for a user.
 */
export const getStatements = async (userId: number): Promise<CommissionStatement[]> => {
  const response = await apiClient.get<CommissionStatement[]>(
    `${API_BASE}/statements/user/${userId}`
  );
  return (response.data || []).map(normalizeStatement);
};

/**
 * Get a statement by ID.
 */
export const getStatementById = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.get<CommissionStatement>(
    `${API_BASE}/statements/${statementId}`
  );
  return normalizeStatement(response.data);
};

/**
 * Finalize a statement for payout.
 */
export const finalizeStatement = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.post<CommissionStatement>(
    `${API_BASE}/statements/${statementId}/finalize`
  );
  return normalizeStatement(response.data);
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
  
  // Statements
  generateStatement,
  getStatements,
  getStatementById,
  finalizeStatement,
};

export default commissionService;

import api from 'src/services/api';

type Params = Record<string, any>;

export const getCommissions = (params?: Params) =>
  api.get('/api/commissions', { params }).then((res) => res.data);

export const getCommission = (id: number) => api.get(`/api/commissions/${id}`).then((res) => res.data);

export const createCommission = (payload: any) => api.post('/api/commissions', payload).then((res) => res.data);

export const updateCommission = (id: number, payload: any) =>
  api.put(`/api/commissions/${id}`, payload).then((res) => res.data);

export const deleteCommission = (id: number) => api.delete(`/api/commissions/${id}`).then((res) => res.data);

export const calculateForDeal = (opportunityId: number) =>
  api.get(`/api/commissions/calculate/deal/${opportunityId}`).then((res) => res.data);

export const calculateForOrder = (orderId: number) =>
  api.get(`/api/commissions/calculate/order/${orderId}`).then((res) => res.data);

export const getPlans = (params?: Params) => api.get('/api/commission-plans', { params }).then((res) => res.data);

export const getPlan = (id: number) => api.get(`/api/commission-plans/${id}`).then((res) => res.data);

export const createPlan = (payload: any) => api.post('/api/commission-plans', payload).then((res) => res.data);

export const updatePlan = (id: number, payload: any) => api.put(`/api/commission-plans/${id}`, payload).then((res) => res.data);

export const assignPlanToUser = (planId: number, userId: number, effectiveDate?: string) =>
  api.post(`/api/commission-plans/${planId}/assign`, { userId, effectiveDate }).then((res) => res.data);

export const getUserPlan = (userId: number) => api.get(`/api/commission-plans/user/${userId}`).then((res) => res.data);

export const generateStatement = (userId: number, fromDate: string, toDate: string) =>
  api.post(`/api/commissions/statements/generate`, { userId, fromDate, toDate }).then((res) => res.data);

export const getStatements = (userId: number) => api.get(`/api/commissions/statements/${userId}`).then((res) => res.data);

export const finalizeStatement = (statementId: number) =>
  api.post(`/api/commissions/statements/${statementId}/finalize`).then((res) => res.data);

export const getLeaderboard = (topN = 10) => api.get('/api/commissions/leaderboard', { params: { topN } }).then((res) => res.data);

export default {
  getCommissions,
  getCommission,
  createCommission,
  updateCommission,
  deleteCommission,
  calculateForDeal,
  calculateForOrder,
  getPlans,
  getPlan,
  createPlan,
  updatePlan,
  assignPlanToUser,
  getUserPlan,
  generateStatement,
  getStatements,
  finalizeStatement,
  getLeaderboard,
};
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
  return response.data;
};

/**
 * Get a commission by ID.
 */
export const getCommissionById = async (id: number): Promise<Commission> => {
  const response = await apiClient.get<Commission>(`${API_BASE}/${id}`);
  return response.data;
};

/**
 * Create a new commission.
 */
export const createCommission = async (
  request: CommissionCreateRequest
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(API_BASE, request);
  return response.data;
};

/**
 * Update a commission.
 */
export const updateCommission = async (
  id: number,
  request: CommissionUpdateRequest
): Promise<Commission> => {
  const response = await apiClient.put<Commission>(`${API_BASE}/${id}`, request);
  return response.data;
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
  return response.data;
};

/**
 * Approve a commission for payout.
 */
export const approveCommission = async (
  id: number,
  approvedById: number
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/approve`, { approvedById });
  return response.data;
};

/**
 * Reject a commission.
 */
export const rejectCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/reject`, { reason });
  return response.data;
};

/**
 * Mark a commission as paid.
 */
export const markCommissionPaid = async (
  id: number,
  paidDate?: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/mark-paid`, { paidDate });
  return response.data;
};

/**
 * Claw back a paid commission.
 */
export const clawbackCommission = async (
  id: number,
  reason: string
): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/clawback`, { reason });
  return response.data;
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
  return response.data;
};

/**
 * Calculate commission for an order.
 */
export const calculateForOrder = async (orderId: number): Promise<CommissionCalculation> => {
  const response = await apiClient.get<CommissionCalculation>(
    `${API_BASE}/calculate/order/${orderId}`
  );
  return response.data;
};

/**
 * Recalculate a commission after changes.
 */
export const recalculateCommission = async (id: number): Promise<Commission> => {
  const response = await apiClient.post<Commission>(`${API_BASE}/${id}/recalculate`);
  return response.data;
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
  return response.data;
};

/**
 * Get pending commissions awaiting approval.
 */
export const getPendingApprovals = async (): Promise<Commission[]> => {
  const response = await apiClient.get<Commission[]>(`${API_BASE}/pending-approvals`);
  return response.data;
};

/**
 * Get commissions ready for payout.
 */
export const getReadyForPayout = async (): Promise<Commission[]> => {
  const response = await apiClient.get<Commission[]>(`${API_BASE}/ready-for-payout`);
  return response.data;
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
  return response.data;
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
  return response.data;
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
  return response.data;
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
  return response.data;
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
  return response.data;
};

/**
 * Get a commission plan by ID.
 */
export const getPlanById = async (planId: number): Promise<CommissionPlan> => {
  const response = await apiClient.get<CommissionPlan>(`${API_BASE}/plans/${planId}`);
  return response.data;
};

/**
 * Create a commission plan.
 */
export const createPlan = async (request: CommissionPlanCreateRequest): Promise<CommissionPlan> => {
  const response = await apiClient.post<CommissionPlan>(`${API_BASE}/plans`, request);
  return response.data;
};

/**
 * Update a commission plan.
 */
export const updatePlan = async (
  planId: number,
  request: CommissionPlanUpdateRequest
): Promise<CommissionPlan> => {
  const response = await apiClient.put<CommissionPlan>(`${API_BASE}/plans/${planId}`, request);
  return response.data;
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
  return response.data;
};

// ============================================================================
// Commission Tiers
// ============================================================================

/**
 * Get tiers for a commission plan.
 */
export const getTiers = async (planId: number): Promise<CommissionTier[]> => {
  const response = await apiClient.get<CommissionTier[]>(`${API_BASE}/plans/${planId}/tiers`);
  return response.data;
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
  return response.data;
};

/**
 * Update a tier.
 */
export const updateTier = async (
  tierId: number,
  request: CommissionTierUpdateRequest
): Promise<CommissionTier> => {
  const response = await apiClient.put<CommissionTier>(`${API_BASE}/tiers/${tierId}`, request);
  return response.data;
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
  return response.data;
};

/**
 * Get statements for a user.
 */
export const getStatements = async (userId: number): Promise<CommissionStatement[]> => {
  const response = await apiClient.get<CommissionStatement[]>(
    `${API_BASE}/statements/user/${userId}`
  );
  return response.data;
};

/**
 * Get a statement by ID.
 */
export const getStatementById = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.get<CommissionStatement>(
    `${API_BASE}/statements/${statementId}`
  );
  return response.data;
};

/**
 * Finalize a statement for payout.
 */
export const finalizeStatement = async (statementId: number): Promise<CommissionStatement> => {
  const response = await apiClient.post<CommissionStatement>(
    `${API_BASE}/statements/${statementId}/finalize`
  );
  return response.data;
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

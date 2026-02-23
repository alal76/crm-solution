/**
 * Sales Configuration Service
 *
 * Provides API methods for managing sales-related configuration:
 * - Commission plans (CRUD, activate/deactivate, tiers, assignments)
 * - Commission calculations (deal, order, period, validation)
 *
 * Backend controllers:
 * - CommissionPlansController:      api/commissionplans
 * - CommissionCalculationsController: api/commissioncalculations
 */

import apiClient from './apiClient';

// ── Types ────────────────────────────────────────────────────────────────────

export interface CommissionPlanDto {
  id: number;
  name: string;
  description?: string;
  commissionType: number;
  baseRate: number;
  isActive: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCommissionPlanDto {
  name: string;
  description?: string;
  commissionType: number;
  baseRate: number;
  isActive?: boolean;
  effectiveFrom?: string;
  effectiveTo?: string;
}

export interface UpdateCommissionPlanDto extends CreateCommissionPlanDto {
  id: number;
}

export interface CommissionTierDto {
  id: number;
  commissionPlanId: number;
  minAmount: number;
  maxAmount?: number;
  rate: number;
  flatBonus?: number;
  sortOrder: number;
}

export interface CreateCommissionTierDto {
  minAmount: number;
  maxAmount?: number;
  rate: number;
  flatBonus?: number;
  sortOrder?: number;
}

export interface UpdateCommissionTierDto extends CreateCommissionTierDto {
  id: number;
}

export interface CommissionDealCalculationDto {
  opportunityId: number;
  userId: number;
  planId?: number;
}

export interface CommissionOrderCalculationDto {
  orderId: number;
  userId: number;
  planId?: number;
}

export interface CommissionPeriodCalculationDto {
  userId: number;
  planId?: number;
  startDate: string;
  endDate: string;
}

export interface CommissionValidationDto {
  planId: number;
  amount: number;
}

export interface CommissionCalculationResultDto {
  baseAmount: number;
  commissionAmount: number;
  commissionRate: number;
  tierApplied?: string;
  bonusAmount?: number;
  totalCommission: number;
  planName?: string;
  calculatedAt: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Service ──────────────────────────────────────────────────────────────────

export const salesConfigService = {
  // ── Commission Plans ────────────────────────────────────────────────────

  /**
   * Get all commission plans (paginated).
   */
  getCommissionPlans: async (
    page: number = 1,
    pageSize: number = 20
  ): Promise<PaginatedResponse<CommissionPlanDto>> => {
    const response = await apiClient.get<PaginatedResponse<CommissionPlanDto>>(
      '/commissionplans',
      { params: { page, pageSize } }
    );
    return response.data;
  },

  /**
   * Get a single commission plan by ID.
   */
  getCommissionPlanById: async (id: number): Promise<CommissionPlanDto> => {
    const response = await apiClient.get<CommissionPlanDto>(`/commissionplans/${id}`);
    return response.data;
  },

  /**
   * Create a new commission plan.
   */
  createCommissionPlan: async (dto: CreateCommissionPlanDto): Promise<CommissionPlanDto> => {
    const response = await apiClient.post<CommissionPlanDto>('/commissionplans', dto);
    return response.data;
  },

  /**
   * Update an existing commission plan.
   */
  updateCommissionPlan: async (
    id: number,
    dto: UpdateCommissionPlanDto
  ): Promise<CommissionPlanDto> => {
    const response = await apiClient.put<CommissionPlanDto>(`/commissionplans/${id}`, dto);
    return response.data;
  },

  /**
   * Delete a commission plan (soft delete).
   */
  deleteCommissionPlan: async (id: number): Promise<void> => {
    await apiClient.delete(`/commissionplans/${id}`);
  },

  /**
   * Activate a commission plan.
   */
  activateCommissionPlan: async (id: number): Promise<void> => {
    await apiClient.post(`/commissionplans/${id}/activate`);
  },

  /**
   * Deactivate a commission plan.
   */
  deactivateCommissionPlan: async (id: number): Promise<void> => {
    await apiClient.post(`/commissionplans/${id}/deactivate`);
  },

  /**
   * Assign a commission plan to a user.
   */
  assignPlanToUser: async (planId: number, userId: number): Promise<void> => {
    await apiClient.post(`/commissionplans/${planId}/assign/${userId}`);
  },

  /**
   * Unassign a commission plan from a user.
   */
  unassignPlanFromUser: async (planId: number, userId: number): Promise<void> => {
    await apiClient.delete(`/commissionplans/${planId}/assign/${userId}`);
  },

  // ── Commission Tiers ───────────────────────────────────────────────────

  /**
   * Get tiers for a commission plan.
   */
  getCommissionTiers: async (planId: number): Promise<CommissionTierDto[]> => {
    const response = await apiClient.get<CommissionTierDto[]>(
      `/commissionplans/${planId}/tiers`
    );
    return response.data;
  },

  /**
   * Create a new tier for a commission plan.
   */
  createCommissionTier: async (
    planId: number,
    dto: CreateCommissionTierDto
  ): Promise<CommissionTierDto> => {
    const response = await apiClient.post<CommissionTierDto>(
      `/commissionplans/${planId}/tiers`,
      dto
    );
    return response.data;
  },

  /**
   * Update an existing tier.
   */
  updateCommissionTier: async (
    planId: number,
    tierId: number,
    dto: UpdateCommissionTierDto
  ): Promise<CommissionTierDto> => {
    const response = await apiClient.put<CommissionTierDto>(
      `/commissionplans/${planId}/tiers/${tierId}`,
      dto
    );
    return response.data;
  },

  // ── Commission Calculations ────────────────────────────────────────────

  /**
   * Calculate commission for a deal (opportunity).
   */
  calculateForDeal: async (
    dto: CommissionDealCalculationDto
  ): Promise<CommissionCalculationResultDto> => {
    const response = await apiClient.post<CommissionCalculationResultDto>(
      '/commissioncalculations/deal',
      dto
    );
    return response.data;
  },

  /**
   * Calculate commission for an order.
   */
  calculateForOrder: async (
    dto: CommissionOrderCalculationDto
  ): Promise<CommissionCalculationResultDto> => {
    const response = await apiClient.post<CommissionCalculationResultDto>(
      '/commissioncalculations/order',
      dto
    );
    return response.data;
  },

  /**
   * Calculate commission for a period.
   */
  calculateForPeriod: async (
    dto: CommissionPeriodCalculationDto
  ): Promise<CommissionCalculationResultDto> => {
    const response = await apiClient.post<CommissionCalculationResultDto>(
      '/commissioncalculations/period',
      dto
    );
    return response.data;
  },

  /**
   * Validate a commission calculation.
   */
  validateCommission: async (
    dto: CommissionValidationDto
  ): Promise<CommissionCalculationResultDto> => {
    const response = await apiClient.post<CommissionCalculationResultDto>(
      '/commissioncalculations/validate',
      dto
    );
    return response.data;
  },
};

export default salesConfigService;

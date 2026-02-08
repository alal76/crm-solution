/**
 * Territory Service
 * 
 * Provides API operations for territory management including:
 * - Territory CRUD operations
 * - Account assignments to territories
 * - Quota management
 * - Territory hierarchy
 * - Statistics and performance
 */
import apiClient from './apiClient';

// ============================================================================
// Types and Interfaces
// ============================================================================

export interface Territory {
  id?: number;
  name: string;
  code: string;
  description?: string;
  parentTerritoryId?: number;
  parentTerritoryName?: string;
  managerId?: number;
  managerName?: string;
  region?: string;
  country?: string;
  states?: string[];
  postalCodes?: string[];
  naicsIndustries?: string[];
  minRevenue?: number;
  maxRevenue?: number;
  minEmployees?: number;
  maxEmployees?: number;
  isActive: boolean;
  color?: string;
  geoJson?: string;
  accountCount?: number;
  opportunityCount?: number;
  pipelineValue?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface TerritoryAssignment {
  id?: number;
  territoryId: number;
  territoryName?: string;
  accountId: number;
  accountName?: string;
  assignedById?: number;
  assignedByName?: string;
  assignedAt?: string;
  reason?: string;
  isPrimary: boolean;
}

export interface TerritoryQuota {
  id?: number;
  territoryId: number;
  territoryName?: string;
  year: number;
  quarter?: number;
  month?: number;
  revenueTarget: number;
  dealCountTarget?: number;
  newAccountTarget?: number;
  actualRevenue?: number;
  actualDealCount?: number;
  actualNewAccounts?: number;
  attainmentPercentage?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface TerritoryStatistics {
  territoryId: number;
  territoryName: string;
  totalAccounts: number;
  activeOpportunities: number;
  totalPipelineValue: number;
  closedWonValue: number;
  closedLostValue: number;
  avgDealSize: number;
  winRate: number;
  quotaAttainment: number;
  teamMemberCount: number;
  accountsBySegment: SegmentCount[];
  monthlyTrend: MonthlyMetrics[];
}

export interface SegmentCount {
  segment: string;
  count: number;
  percentage: number;
}

export interface MonthlyMetrics {
  month: string;
  revenue: number;
  dealsClosed: number;
  newAccounts: number;
}

export interface TerritoryHierarchy {
  territory: Territory;
  children: TerritoryHierarchy[];
  level: number;
  totalAccounts: number;
  totalPipelineValue: number;
}

export interface TerritoryRule {
  id?: number;
  territoryId: number;
  ruleType: 'include' | 'exclude';
  field: string;
  operator: 'equals' | 'contains' | 'startsWith' | 'endsWith' | 'in' | 'range';
  value: string;
  priority: number;
  isActive: boolean;
}

export interface AutoAssignResult {
  totalProcessed: number;
  assigned: number;
  skipped: number;
  errors: number;
  assignments: TerritoryAssignment[];
  errorDetails?: AssignmentError[];
}

export interface AssignmentError {
  accountId: number;
  accountName: string;
  error: string;
}

export interface CreateTerritoryDto {
  name: string;
  code: string;
  description?: string;
  parentTerritoryId?: number;
  managerId?: number;
  region?: string;
  country?: string;
  states?: string[];
  postalCodes?: string[];
  naicsIndustries?: string[];
  minRevenue?: number;
  maxRevenue?: number;
  minEmployees?: number;
  maxEmployees?: number;
  color?: string;
}

export interface UpdateTerritoryDto extends Partial<CreateTerritoryDto> {
  isActive?: boolean;
  geoJson?: string;
}

// ============================================================================
// Territory Service
// ============================================================================

const territoryService = {
  // === Territory CRUD ===
  
  /**
   * Get all territories
   */
  getAll: (includeInactive: boolean = false) =>
    apiClient.get<Territory[]>(`/territories?includeInactive=${includeInactive}`),

  /**
   * Get territories by region
   */
  getByRegion: (region: string) =>
    apiClient.get<Territory[]>(`/territories/region/${encodeURIComponent(region)}`),

  /**
   * Get territory by ID
   */
  getById: (id: number) =>
    apiClient.get<Territory>(`/territories/${id}`),

  /**
   * Get territory by code
   */
  getByCode: (code: string) =>
    apiClient.get<Territory>(`/territories/code/${encodeURIComponent(code)}`),

  /**
   * Create a new territory
   */
  create: (data: CreateTerritoryDto) =>
    apiClient.post<Territory>('/territories', data),

  /**
   * Update a territory
   */
  update: (id: number, data: UpdateTerritoryDto) =>
    apiClient.put<Territory>(`/territories/${id}`, data),

  /**
   * Delete a territory
   */
  delete: (id: number) =>
    apiClient.delete(`/territories/${id}`),

  /**
   * Activate a territory
   */
  activate: (id: number) =>
    apiClient.post<Territory>(`/territories/${id}/activate`),

  /**
   * Deactivate a territory
   */
  deactivate: (id: number) =>
    apiClient.post<Territory>(`/territories/${id}/deactivate`),

  // === Territory Hierarchy ===

  /**
   * Get territory hierarchy (tree view)
   */
  getHierarchy: () =>
    apiClient.get<TerritoryHierarchy[]>('/territories/hierarchy'),

  /**
   * Get child territories
   */
  getChildren: (parentId: number) =>
    apiClient.get<Territory[]>(`/territories/${parentId}/children`),

  /**
   * Move territory under new parent
   */
  moveTerritory: (id: number, newParentId: number | null) =>
    apiClient.post<Territory>(`/territories/${id}/move`, { newParentId }),

  // === Account Assignments ===

  /**
   * Get accounts assigned to a territory
   */
  getAssignedAccounts: (territoryId: number, page: number = 1, pageSize: number = 50) =>
    apiClient.get<{ items: TerritoryAssignment[]; totalCount: number }>(
      `/territories/${territoryId}/accounts?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Assign account to territory
   */
  assignAccount: (territoryId: number, accountId: number, isPrimary: boolean = true, reason?: string) =>
    apiClient.post<TerritoryAssignment>(`/territories/${territoryId}/accounts`, {
      accountId,
      isPrimary,
      reason,
    }),

  /**
   * Remove account from territory
   */
  removeAccount: (territoryId: number, accountId: number) =>
    apiClient.delete(`/territories/${territoryId}/accounts/${accountId}`),

  /**
   * Bulk assign accounts to territory
   */
  bulkAssignAccounts: (territoryId: number, accountIds: number[], reason?: string) =>
    apiClient.post<{ assignedCount: number; failedCount: number }>(
      `/territories/${territoryId}/accounts/bulk-assign`,
      { accountIds, reason }
    ),

  /**
   * Get territory for an account
   */
  getTerritoryForAccount: (accountId: number) =>
    apiClient.get<Territory | null>(`/territories/by-account/${accountId}`),

  /**
   * Auto-assign unassigned accounts based on rules
   */
  autoAssignAccounts: (territoryId?: number) => {
    const url = territoryId
      ? `/territories/${territoryId}/auto-assign`
      : '/territories/auto-assign';
    return apiClient.post<AutoAssignResult>(url);
  },

  // === Territory Rules ===

  /**
   * Get rules for a territory
   */
  getRules: (territoryId: number) =>
    apiClient.get<TerritoryRule[]>(`/territories/${territoryId}/rules`),

  /**
   * Add a rule to a territory
   */
  addRule: (territoryId: number, rule: Omit<TerritoryRule, 'id' | 'territoryId'>) =>
    apiClient.post<TerritoryRule>(`/territories/${territoryId}/rules`, rule),

  /**
   * Update a territory rule
   */
  updateRule: (territoryId: number, ruleId: number, rule: Partial<TerritoryRule>) =>
    apiClient.put<TerritoryRule>(`/territories/${territoryId}/rules/${ruleId}`, rule),

  /**
   * Delete a territory rule
   */
  deleteRule: (territoryId: number, ruleId: number) =>
    apiClient.delete(`/territories/${territoryId}/rules/${ruleId}`),

  // === Quota Management ===

  /**
   * Get quotas for a territory
   */
  getQuotas: (territoryId: number, year?: number) => {
    let url = `/territories/${territoryId}/quotas`;
    if (year) url += `?year=${year}`;
    return apiClient.get<TerritoryQuota[]>(url);
  },

  /**
   * Set quota for a territory
   */
  setQuota: (territoryId: number, quota: Omit<TerritoryQuota, 'id' | 'territoryId' | 'territoryName'>) =>
    apiClient.post<TerritoryQuota>(`/territories/${territoryId}/quotas`, quota),

  /**
   * Update a quota
   */
  updateQuota: (territoryId: number, quotaId: number, quota: Partial<TerritoryQuota>) =>
    apiClient.put<TerritoryQuota>(`/territories/${territoryId}/quotas/${quotaId}`, quota),

  /**
   * Delete a quota
   */
  deleteQuota: (territoryId: number, quotaId: number) =>
    apiClient.delete(`/territories/${territoryId}/quotas/${quotaId}`),

  /**
   * Get quota attainment
   */
  getQuotaAttainment: (territoryId: number, year: number, quarter?: number) => {
    let url = `/territories/${territoryId}/quota-attainment?year=${year}`;
    if (quarter) url += `&quarter=${quarter}`;
    return apiClient.get<{ quota: TerritoryQuota; attainment: number }>(url);
  },

  // === Statistics ===

  /**
   * Get territory statistics
   */
  getStatistics: (territoryId: number) =>
    apiClient.get<TerritoryStatistics>(`/territories/${territoryId}/statistics`),

  /**
   * Get all territories statistics summary
   */
  getAllStatistics: () =>
    apiClient.get<TerritoryStatistics[]>('/territories/statistics'),

  /**
   * Get territory leaderboard
   */
  getLeaderboard: (metric: 'revenue' | 'deals' | 'pipeline' | 'winRate' = 'revenue', limit: number = 10) =>
    apiClient.get<TerritoryStatistics[]>(`/territories/leaderboard?metric=${metric}&limit=${limit}`),

  // === Team ===

  /**
   * Get team members for a territory
   */
  getTeamMembers: (territoryId: number) =>
    apiClient.get<{ userId: number; userName: string; role: string }[]>(
      `/territories/${territoryId}/team`
    ),

  /**
   * Add team member to territory
   */
  addTeamMember: (territoryId: number, userId: number, role: string) =>
    apiClient.post(`/territories/${territoryId}/team`, { userId, role }),

  /**
   * Remove team member from territory
   */
  removeTeamMember: (territoryId: number, userId: number) =>
    apiClient.delete(`/territories/${territoryId}/team/${userId}`),

  // === Geographic ===

  /**
   * Get territories for a location (by postal code or coordinates)
   */
  findByLocation: (postalCode?: string, latitude?: number, longitude?: number) => {
    const params = new URLSearchParams();
    if (postalCode) params.append('postalCode', postalCode);
    if (latitude !== undefined) params.append('latitude', latitude.toString());
    if (longitude !== undefined) params.append('longitude', longitude.toString());
    return apiClient.get<Territory[]>(`/territories/by-location?${params.toString()}`);
  },

  /**
   * Update territory geographic boundaries
   */
  updateGeoJson: (id: number, geoJson: string) =>
    apiClient.put<Territory>(`/territories/${id}/geo`, { geoJson }),

  // === Export ===

  /**
   * Export territories to CSV
   */
  exportToCsv: () =>
    apiClient.get('/territories/export', { responseType: 'blob' }),

  /**
   * Export territory accounts to CSV
   */
  exportAccountsToCsv: (territoryId: number) =>
    apiClient.get(`/territories/${territoryId}/accounts/export`, { responseType: 'blob' }),
};

export default territoryService;

/**
 * Lead Routing Service
 * 
 * Provides API operations for lead routing functionality including:
 * - Routing rules CRUD
 * - Lead assignment and routing
 * - Routing history and logs
 * - Statistics and analytics
 */
import apiClient from './apiClient';

// ============================================================================
// Types and Interfaces
// ============================================================================

export interface LeadRoutingRule {
  id?: number;
  name: string;
  description?: string;
  isActive: boolean;
  priority: number;
  routingType: RoutingType;
  criteria: RoutingCriteria[];
  targetType: TargetType;
  targetId?: number;
  targetName?: string;
  targetIds?: number[];
  roundRobinIndex?: number;
  maxLeadsPerDay?: number;
  maxLeadsPerWeek?: number;
  workingHoursOnly: boolean;
  workingHoursStart?: string;
  workingHoursEnd?: string;
  workingDays?: number[];
  fallbackRuleId?: number;
  fallbackRuleName?: string;
  createdAt?: string;
  updatedAt?: string;
  lastTriggeredAt?: string;
  triggerCount?: number;
}

export interface RoutingCriteria {
  id?: number;
  field: string;
  fieldLabel?: string;
  operator: CriteriaOperator;
  value: string;
  values?: string[];
  logicalOperator?: 'and' | 'or';
}

export enum RoutingType {
  DirectAssignment = 'direct',
  RoundRobin = 'roundRobin',
  WeightedDistribution = 'weighted',
  LeastLoaded = 'leastLoaded',
  Geography = 'geography',
  Skill = 'skill',
  Queue = 'queue',
}

export enum TargetType {
  User = 'user',
  Team = 'team',
  Queue = 'queue',
  Territory = 'territory',
}

export enum CriteriaOperator {
  Equals = 'equals',
  NotEquals = 'notEquals',
  Contains = 'contains',
  NotContains = 'notContains',
  StartsWith = 'startsWith',
  EndsWith = 'endsWith',
  GreaterThan = 'greaterThan',
  LessThan = 'lessThan',
  GreaterOrEqual = 'greaterOrEqual',
  LessOrEqual = 'lessOrEqual',
  In = 'in',
  NotIn = 'notIn',
  IsEmpty = 'isEmpty',
  IsNotEmpty = 'isNotEmpty',
  Between = 'between',
}

export interface RoutingLog {
  id?: number;
  leadId: number;
  leadName?: string;
  leadEmail?: string;
  ruleId?: number;
  ruleName?: string;
  previousOwnerId?: number;
  previousOwnerName?: string;
  newOwnerId: number;
  newOwnerName?: string;
  routingType: RoutingType;
  reason: string;
  processingTimeMs?: number;
  matchedCriteria?: string;
  routedAt: string;
  success: boolean;
  errorMessage?: string;
}

export interface RoutingStatistics {
  totalRules: number;
  activeRules: number;
  totalRoutedLeads: number;
  routedLast24Hours: number;
  routedLast7Days: number;
  routedLast30Days: number;
  avgProcessingTimeMs: number;
  successRate: number;
  routingByType: RoutingTypeStats[];
  routingByUser: UserRoutingStats[];
  routingByRule: RuleRoutingStats[];
  dailyTrend: DailyRoutingStats[];
}

export interface RoutingTypeStats {
  type: RoutingType;
  count: number;
  percentage: number;
}

export interface UserRoutingStats {
  userId: number;
  userName: string;
  leadsReceived: number;
  percentage: number;
}

export interface RuleRoutingStats {
  ruleId: number;
  ruleName: string;
  triggerCount: number;
  successRate: number;
  avgProcessingTimeMs: number;
}

export interface DailyRoutingStats {
  date: string;
  count: number;
  successCount: number;
  failureCount: number;
}

export interface RouteLeadResult {
  success: boolean;
  leadId: number;
  assignedToId?: number;
  assignedToName?: string;
  ruleId?: number;
  ruleName?: string;
  reason: string;
  processingTimeMs: number;
}

export interface CreateRoutingRuleDto {
  name: string;
  description?: string;
  priority?: number;
  routingType: RoutingType;
  criteria: Omit<RoutingCriteria, 'id'>[];
  targetType: TargetType;
  targetId?: number;
  targetIds?: number[];
  maxLeadsPerDay?: number;
  maxLeadsPerWeek?: number;
  workingHoursOnly?: boolean;
  workingHoursStart?: string;
  workingHoursEnd?: string;
  workingDays?: number[];
  fallbackRuleId?: number;
}

export interface UpdateRoutingRuleDto extends Partial<CreateRoutingRuleDto> {
  isActive?: boolean;
}

export interface QueueEntry {
  id?: number;
  queueId: number;
  queueName?: string;
  leadId: number;
  leadName?: string;
  priority: number;
  addedAt: string;
  status: 'pending' | 'assigned' | 'expired';
  expiresAt?: string;
}

// ============================================================================
// Lead Routing Service
// ============================================================================

const leadRoutingService = {
  // === Routing Rules CRUD ===
  
  /**
   * Get all routing rules
   */
  getAllRules: (includeInactive: boolean = false) =>
    apiClient.get<LeadRoutingRule[]>(`/lead-routing/rules?includeInactive=${includeInactive}`),

  /**
   * Get active routing rules only
   */
  getActiveRules: () =>
    apiClient.get<LeadRoutingRule[]>('/lead-routing/rules/active'),

  /**
   * Get routing rule by ID
   */
  getRuleById: (id: number) =>
    apiClient.get<LeadRoutingRule>(`/lead-routing/rules/${id}`),

  /**
   * Create a new routing rule
   */
  createRule: (data: CreateRoutingRuleDto) =>
    apiClient.post<LeadRoutingRule>('/lead-routing/rules', data),

  /**
   * Update a routing rule
   */
  updateRule: (id: number, data: UpdateRoutingRuleDto) =>
    apiClient.put<LeadRoutingRule>(`/lead-routing/rules/${id}`, data),

  /**
   * Delete a routing rule
   */
  deleteRule: (id: number) =>
    apiClient.delete(`/lead-routing/rules/${id}`),

  /**
   * Enable a routing rule
   */
  enableRule: (id: number) =>
    apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/enable`),

  /**
   * Disable a routing rule
   */
  disableRule: (id: number) =>
    apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/disable`),

  /**
   * Reorder routing rules (by priority)
   */
  reorderRules: (ruleIds: number[]) =>
    apiClient.post('/lead-routing/rules/reorder', { ruleIds }),

  /**
   * Clone a routing rule
   */
  cloneRule: (id: number, newName: string) =>
    apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/clone`, { newName }),

  // === Lead Routing Operations ===

  /**
   * Route a specific lead
   */
  routeLead: (leadId: number, forceRuleId?: number) =>
    apiClient.post<RouteLeadResult>(`/lead-routing/route/${leadId}`, { forceRuleId }),

  /**
   * Route multiple leads
   */
  routeLeads: (leadIds: number[]) =>
    apiClient.post<{ results: RouteLeadResult[]; successCount: number; failureCount: number }>(
      '/lead-routing/route/bulk',
      { leadIds }
    ),

  /**
   * Route all unassigned leads
   */
  routeUnassignedLeads: () =>
    apiClient.post<{ results: RouteLeadResult[]; successCount: number; failureCount: number }>(
      '/lead-routing/route/unassigned'
    ),

  /**
   * Preview routing for a lead (dry run)
   */
  previewRouting: (leadId: number) =>
    apiClient.get<RouteLeadResult>(`/lead-routing/preview/${leadId}`),

  /**
   * Manually assign a lead
   */
  manualAssign: (leadId: number, userId: number, reason?: string) =>
    apiClient.post<RouteLeadResult>(`/lead-routing/assign`, { leadId, userId, reason }),

  /**
   * Reassign a lead
   */
  reassign: (leadId: number, newUserId: number, reason?: string) =>
    apiClient.post<RouteLeadResult>(`/lead-routing/reassign`, { leadId, newUserId, reason }),

  // === Routing History ===

  /**
   * Get routing history for a lead
   */
  getLeadHistory: (leadId: number) =>
    apiClient.get<RoutingLog[]>(`/lead-routing/history/lead/${leadId}`),

  /**
   * Get routing history for a user
   */
  getUserHistory: (userId: number, page: number = 1, pageSize: number = 50) =>
    apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
      `/lead-routing/history/user/${userId}?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Get routing history for a rule
   */
  getRuleHistory: (ruleId: number, page: number = 1, pageSize: number = 50) =>
    apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
      `/lead-routing/history/rule/${ruleId}?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Get all routing history
   */
  getAllHistory: (
    page: number = 1,
    pageSize: number = 50,
    fromDate?: string,
    toDate?: string,
    success?: boolean
  ) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (success !== undefined) params.append('success', success.toString());
    return apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
      `/lead-routing/history?${params.toString()}`
    );
  },

  // === Statistics ===

  /**
   * Get routing statistics
   */
  getStatistics: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<RoutingStatistics>(`/lead-routing/statistics${query ? `?${query}` : ''}`);
  },

  /**
   * Get rule performance statistics
   */
  getRuleStatistics: (ruleId: number, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get<RuleRoutingStats>(
      `/lead-routing/rules/${ruleId}/statistics${query ? `?${query}` : ''}`
    );
  },

  /**
   * Get user workload statistics
   */
  getUserWorkload: (userId: number) =>
    apiClient.get<{
      pendingLeads: number;
      leadsToday: number;
      leadsThisWeek: number;
      capacity: number;
      utilizationPercentage: number;
    }>(`/lead-routing/workload/${userId}`),

  /**
   * Get all users workload for balancing
   */
  getAllUsersWorkload: () =>
    apiClient.get<{
      userId: number;
      userName: string;
      pendingLeads: number;
      leadsToday: number;
      capacity: number;
      utilizationPercentage: number;
    }[]>('/lead-routing/workload'),

  // === Queues ===

  /**
   * Get lead queues
   */
  getQueues: () =>
    apiClient.get<{ id: number; name: string; leadCount: number }[]>('/lead-routing/queues'),

  /**
   * Get queue entries
   */
  getQueueEntries: (queueId: number, page: number = 1, pageSize: number = 50) =>
    apiClient.get<{ items: QueueEntry[]; totalCount: number }>(
      `/lead-routing/queues/${queueId}/entries?page=${page}&pageSize=${pageSize}`
    ),

  /**
   * Add lead to queue
   */
  addToQueue: (queueId: number, leadId: number, priority?: number) =>
    apiClient.post<QueueEntry>(`/lead-routing/queues/${queueId}/entries`, { leadId, priority }),

  /**
   * Remove lead from queue
   */
  removeFromQueue: (queueId: number, leadId: number) =>
    apiClient.delete(`/lead-routing/queues/${queueId}/entries/${leadId}`),

  /**
   * Claim next lead from queue
   */
  claimFromQueue: (queueId: number) =>
    apiClient.post<RouteLeadResult>(`/lead-routing/queues/${queueId}/claim`),

  // === Rule Testing ===

  /**
   * Test a routing rule against sample leads
   */
  testRule: (ruleId: number, sampleSize: number = 10) =>
    apiClient.post<{ matches: number; total: number; matchedLeads: { id: number; name: string }[] }>(
      `/lead-routing/rules/${ruleId}/test`,
      { sampleSize }
    ),

  /**
   * Validate rule criteria syntax
   */
  validateCriteria: (criteria: RoutingCriteria[]) =>
    apiClient.post<{ isValid: boolean; errors: string[] }>(
      '/lead-routing/validate-criteria',
      { criteria }
    ),

  // === Available Fields ===

  /**
   * Get available fields for routing criteria
   */
  getAvailableFields: () =>
    apiClient.get<{
      name: string;
      label: string;
      type: string;
      operators: CriteriaOperator[];
      options?: { value: string; label: string }[];
    }[]>('/lead-routing/fields'),

  // === Export ===

  /**
   * Export routing history to CSV
   */
  exportHistory: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    return apiClient.get(`/lead-routing/history/export${query ? `?${query}` : ''}`, {
      responseType: 'blob',
    });
  },
};

export default leadRoutingService;

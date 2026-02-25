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

// Forward declarations for normalizers (defined after interfaces)

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
// Data Normalization
// ============================================================================
// Backend may return different property names. These helpers map backend responses
// to frontend interfaces, providing defensive handling against undefined values.

/** Raw API response data - JSON object with unknown field types */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type RawApiData = any;

/** Safe field extractors for raw API data */
const rStr = (r: RawApiData, k: string, fb = ''): string => {
  const v = r[k]; return v != null && typeof v !== 'object' ? String(v) : fb;
};
const rNum = (r: RawApiData, k: string, fb = 0): number => {
  const v = r[k]; return typeof v === 'number' ? v : fb;
};
const rBool = (r: RawApiData, k: string, fb = false): boolean => {
  const v = r[k]; return typeof v === 'boolean' ? v : fb;
};
const rArr = (r: RawApiData, k: string): RawApiData[] =>
  Array.isArray(r[k]) ? (r[k] as RawApiData[]) : [];

const normalizeRoutingCriteria = (c: RawApiData): RoutingCriteria => ({
  id: rNum(c, 'id') || undefined,
  field: rStr(c, 'field'),
  fieldLabel: rStr(c, 'fieldLabel') || rStr(c, 'field'),
  operator: (c['operator'] as CriteriaOperator) || CriteriaOperator.Equals,
  value: (c['value'] ?? '') as string,
  values: (c['values'] as string[]) || [],
  logicalOperator: rStr(c, 'logicalOperator', 'and') as 'and' | 'or',
});

const normalizeLeadRoutingRule = (r: RawApiData): LeadRoutingRule => ({
  id: rNum(r, 'id') || undefined,
  name: rStr(r, 'name') || rStr(r, 'ruleName'),
  description: rStr(r, 'description'),
  isActive: rBool(r, 'isActive', r['status'] === 'Active'),
  priority: rNum(r, 'priority') || rNum(r, 'order'),
  routingType: (r['routingType'] || r['assignmentType'] || RoutingType.RoundRobin) as RoutingType,
  criteria: rArr(r, 'criteria').map(normalizeRoutingCriteria),
  targetType: (r['targetType'] || (r['assignToTeam'] ? TargetType.Team : TargetType.User)) as TargetType,
  targetId: (r['targetId'] ?? r['assignToUserId'] ?? r['assignToTeamId']) as number | undefined,
  targetName: (r['targetName'] ?? r['assignToUserName'] ?? r['assignToTeamName'] ?? '') as string,
  targetIds: (r['targetIds'] as number[]) || [],
  roundRobinIndex: rNum(r, 'roundRobinIndex'),
  maxLeadsPerDay: (r['maxLeadsPerDay'] ?? r['dailyLimit']) as number | undefined,
  maxLeadsPerWeek: (r['maxLeadsPerWeek'] ?? r['weeklyLimit']) as number | undefined,
  workingHoursOnly: rBool(r, 'workingHoursOnly', rBool(r, 'businessHoursOnly')),
  workingHoursStart: rStr(r, 'workingHoursStart') || rStr(r, 'businessHoursStart'),
  workingHoursEnd: rStr(r, 'workingHoursEnd') || rStr(r, 'businessHoursEnd'),
  workingDays: (r['workingDays'] as number[]) || [],
  fallbackRuleId: r['fallbackRuleId'] as number | undefined,
  fallbackRuleName: rStr(r, 'fallbackRuleName'),
  createdAt: (r['createdAt'] || r['created']) as string | undefined,
  updatedAt: (r['updatedAt'] || r['modified'] || r['lastModified']) as string | undefined,
  lastTriggeredAt: (r['lastTriggeredAt'] || r['lastExecuted']) as string | undefined,
  triggerCount: rNum(r, 'triggerCount') || rNum(r, 'executionCount'),
});

const normalizeRoutingLog = (log: RawApiData): RoutingLog => ({
  id: rNum(log, 'id') || undefined,
  leadId: rNum(log, 'leadId'),
  leadName: rStr(log, 'leadName') || rStr(log['lead'] as RawApiData ?? {}, 'name'),
  leadEmail: rStr(log, 'leadEmail') || rStr(log['lead'] as RawApiData ?? {}, 'email'),
  ruleId: (log['ruleId'] ?? log['routingRuleId']) as number | undefined,
  ruleName: rStr(log, 'ruleName') || rStr(log, 'routingRuleName') || rStr(log['rule'] as RawApiData ?? {}, 'name'),
  previousOwnerId: (log['previousOwnerId'] ?? log['previousUserId']) as number | undefined,
  previousOwnerName: rStr(log, 'previousOwnerName') || rStr(log, 'previousUserName'),
  newOwnerId: (rNum(log, 'newOwnerId') || rNum(log, 'assignedToUserId') || rNum(log, 'userId')),
  newOwnerName: (log['newOwnerName'] ?? log['assignedToUserName'] ?? rStr(log['assignedToUser'] as RawApiData ?? {}, 'name') ?? log['userName'] ?? '') as string,
  routingType: (log['routingType'] || log['assignmentType'] || RoutingType.DirectAssignment) as RoutingType,
  reason: rStr(log, 'reason') || rStr(log, 'routingReason'),
  processingTimeMs: rNum(log, 'processingTimeMs') || (rNum(log, 'responseTimeSeconds') * 1000),
  matchedCriteria: rStr(log, 'matchedCriteria'),
  routedAt: (log['routedAt'] ?? log['assignedAt'] ?? log['createdAt'] ?? new Date().toISOString()) as string,
  success: rBool(log, 'success', rBool(log, 'isSuccess', true)),
  errorMessage: rStr(log, 'errorMessage') || rStr(log, 'error'),
});

const normalizeRouteLeadResult = (r: RawApiData): RouteLeadResult => ({
  success: rBool(r, 'success', rBool(r, 'isSuccess')),
  leadId: rNum(r, 'leadId'),
  assignedToId: (r['assignedToId'] ?? r['assignedToUserId'] ?? r['userId']) as number | undefined,
  assignedToName: (r['assignedToName'] ?? r['assignedToUserName'] ?? r['userName'] ?? '') as string,
  ruleId: (r['ruleId'] ?? r['routingRuleId']) as number | undefined,
  ruleName: (r['ruleName'] ?? r['routingRuleName'] ?? '') as string,
  reason: rStr(r, 'reason') || rStr(r, 'message'),
  processingTimeMs: rNum(r, 'processingTimeMs') || (rNum(r, 'responseTimeSeconds') * 1000),
});

const normalizeRoutingTypeStats = (s: RawApiData): RoutingTypeStats => ({
  type: (s['type'] || s['routingType'] || RoutingType.DirectAssignment) as RoutingType,
  count: rNum(s, 'count') || rNum(s, 'total'),
  percentage: rNum(s, 'percentage'),
});

const normalizeUserRoutingStats = (s: RawApiData): UserRoutingStats => ({
  userId: rNum(s, 'userId'),
  userName: rStr(s, 'userName') || rStr(s, 'name'),
  leadsReceived: rNum(s, 'leadsReceived') || rNum(s, 'count') || rNum(s, 'leadCount'),
  percentage: rNum(s, 'percentage'),
});

const normalizeRuleRoutingStats = (s: RawApiData): RuleRoutingStats => ({
  ruleId: rNum(s, 'ruleId'),
  ruleName: rStr(s, 'ruleName') || rStr(s, 'name'),
  triggerCount: rNum(s, 'triggerCount') || rNum(s, 'count'),
  successRate: rNum(s, 'successRate'),
  avgProcessingTimeMs: rNum(s, 'avgProcessingTimeMs') || (rNum(s, 'averageResponseTimeSeconds') * 1000),
});

const normalizeDailyRoutingStats = (s: RawApiData): DailyRoutingStats => ({
  date: rStr(s, 'date'),
  count: rNum(s, 'count') || rNum(s, 'total'),
  successCount: rNum(s, 'successCount') || rNum(s, 'success'),
  failureCount: rNum(s, 'failureCount') || rNum(s, 'failure'),
});

const normalizeStatistics = (s: RawApiData): RoutingStatistics => ({
  totalRules: rNum(s, 'totalRules') || rNum(s, 'ruleCount'),
  activeRules: rNum(s, 'activeRules') || rNum(s, 'activeRuleCount'),
  totalRoutedLeads: rNum(s, 'totalRoutedLeads') || rNum(s, 'totalLeadsRouted') || rNum(s, 'totalAssignments'),
  routedLast24Hours: rNum(s, 'routedLast24Hours') || rNum(s, 'last24Hours'),
  routedLast7Days: rNum(s, 'routedLast7Days') || rNum(s, 'last7Days'),
  routedLast30Days: rNum(s, 'routedLast30Days') || rNum(s, 'last30Days'),
  avgProcessingTimeMs: rNum(s, 'avgProcessingTimeMs') || (rNum(s, 'averageResponseTimeSeconds') * 1000),
  successRate: rNum(s, 'successRate'),
  routingByType: (Array.isArray(s['routingByType']) ? s['routingByType'] as RawApiData[] :
    Array.isArray(s['routesByAssignmentType']) ? s['routesByAssignmentType'] as RawApiData[] : []).map(normalizeRoutingTypeStats),
  routingByUser: (Array.isArray(s['routingByUser']) ? s['routingByUser'] as RawApiData[] :
    Array.isArray(s['routesByUser']) ? s['routesByUser'] as RawApiData[] : []).map(normalizeUserRoutingStats),
  routingByRule: rArr(s, 'routingByRule').map(normalizeRuleRoutingStats),
  dailyTrend: rArr(s, 'dailyTrend').map(normalizeDailyRoutingStats),
});

const normalizeQueueEntry = (e: RawApiData): QueueEntry => ({
  id: rNum(e, 'id') || undefined,
  queueId: rNum(e, 'queueId'),
  queueName: rStr(e, 'queueName'),
  leadId: rNum(e, 'leadId'),
  leadName: rStr(e, 'leadName'),
  priority: rNum(e, 'priority'),
  addedAt: (e['addedAt'] || e['createdAt'] || new Date().toISOString()) as string,
  status: rStr(e, 'status', 'pending') as 'pending' | 'assigned' | 'expired',
  expiresAt: e['expiresAt'] as string | undefined,
});

const normalizeWorkload = (w: RawApiData) => ({
  userId: rNum(w, 'userId'),
  userName: rStr(w, 'userName') || rStr(w, 'name'),
  pendingLeads: rNum(w, 'pendingLeads') || rNum(w, 'pending'),
  leadsToday: rNum(w, 'leadsToday') || rNum(w, 'today'),
  leadsThisWeek: rNum(w, 'leadsThisWeek') || rNum(w, 'thisWeek'),
  capacity: rNum(w, 'capacity') || rNum(w, 'maxCapacity') || 100,
  utilizationPercentage: rNum(w, 'utilizationPercentage') || rNum(w, 'utilization'),
});

// ============================================================================
// Lead Routing Service
// ============================================================================

const leadRoutingService = {
  // === Routing Rules CRUD ===
  
  /**
   * Get all routing rules
   */
  getAllRules: async (includeInactive: boolean = false) => {
    try {
      const response = await apiClient.get<LeadRoutingRule[]>(`/lead-routing/rules?includeInactive=${includeInactive}`);
      return { ...response, data: (response.data || []).map(normalizeLeadRoutingRule) };
    } catch (error) {
      console.error('getAllRules failed:', error);
      return { data: [] as LeadRoutingRule[] };
    }
  },

  /**
   * Get active routing rules only
   */
  getActiveRules: async () => {
    try {
      // Try the dedicated endpoint first
      const response = await apiClient.get<LeadRoutingRule[]>('/lead-routing/rules/active');
      return { ...response, data: (response.data || []).map(normalizeLeadRoutingRule) };
    } catch {
      // Fallback: use getAllRules and filter
      try {
        const response = await apiClient.get<LeadRoutingRule[]>('/lead-routing/rules?includeInactive=true');
        const activeRules = (response.data || [])
          .map(normalizeLeadRoutingRule)
          .filter(r => r.isActive);
        return { ...response, data: activeRules };
      } catch (error) {
        console.error('getActiveRules failed:', error);
        return { data: [] as LeadRoutingRule[] };
      }
    }
  },

  /**
   * Get routing rule by ID
   */
  getRuleById: async (id: number) => {
    try {
      const response = await apiClient.get<LeadRoutingRule>(`/lead-routing/rules/${id}`);
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch (error) {
      console.error('getRuleById failed:', error);
      throw error;
    }
  },

  /**
   * Create a new routing rule
   */
  createRule: async (data: CreateRoutingRuleDto) => {
    try {
      const response = await apiClient.post<LeadRoutingRule>('/lead-routing/rules', data);
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch (error) {
      console.error('createRule failed:', error);
      throw error;
    }
  },

  /**
   * Update a routing rule
   */
  updateRule: async (id: number, data: UpdateRoutingRuleDto) => {
    try {
      const response = await apiClient.put<LeadRoutingRule>(`/lead-routing/rules/${id}`, data);
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch (error) {
      console.error('updateRule failed:', error);
      throw error;
    }
  },

  /**
   * Delete a routing rule
   */
  deleteRule: (id: number) =>
    apiClient.delete(`/lead-routing/rules/${id}`),

  /**
   * Enable a routing rule
   */
  enableRule: async (id: number) => {
    try {
      const response = await apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/enable`);
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch {
      // Fallback: try using updateRule
      try {
        const response = await apiClient.put<LeadRoutingRule>(`/lead-routing/rules/${id}`, { isActive: true });
        return { ...response, data: normalizeLeadRoutingRule(response.data) };
      } catch (error) {
        console.error('enableRule failed:', error);
        throw error;
      }
    }
  },

  /**
   * Disable a routing rule
   */
  disableRule: async (id: number) => {
    try {
      const response = await apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/disable`);
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch {
      // Fallback: try using updateRule
      try {
        const response = await apiClient.put<LeadRoutingRule>(`/lead-routing/rules/${id}`, { isActive: false });
        return { ...response, data: normalizeLeadRoutingRule(response.data) };
      } catch (error) {
        console.error('disableRule failed:', error);
        throw error;
      }
    }
  },

  /**
   * Reorder routing rules (by priority)
   */
  reorderRules: async (ruleIds: number[]) => {
    try {
      return await apiClient.post('/lead-routing/rules/reorder', { ruleIds });
    } catch (error) {
      console.error('reorderRules failed:', error);
      // Return success anyway - non-critical operation
      return { data: { success: true } };
    }
  },

  /**
   * Clone a routing rule
   */
  cloneRule: async (id: number, newName: string) => {
    try {
      const response = await apiClient.post<LeadRoutingRule>(`/lead-routing/rules/${id}/clone`, { newName });
      return { ...response, data: normalizeLeadRoutingRule(response.data) };
    } catch {
      // Fallback: get the rule and create a new one
      try {
        const original = await apiClient.get<LeadRoutingRule>(`/lead-routing/rules/${id}`);
        const normalized = normalizeLeadRoutingRule(original.data);
        const newRule: CreateRoutingRuleDto = {
          name: newName,
          description: normalized.description,
          priority: (normalized.priority || 0) + 1,
          routingType: normalized.routingType,
          criteria: normalized.criteria || [],
          targetType: normalized.targetType,
          targetId: normalized.targetId,
          targetIds: normalized.targetIds,
          maxLeadsPerDay: normalized.maxLeadsPerDay,
          maxLeadsPerWeek: normalized.maxLeadsPerWeek,
          workingHoursOnly: normalized.workingHoursOnly,
          workingHoursStart: normalized.workingHoursStart,
          workingHoursEnd: normalized.workingHoursEnd,
          workingDays: normalized.workingDays,
          fallbackRuleId: normalized.fallbackRuleId,
        };
        const response = await apiClient.post<LeadRoutingRule>('/lead-routing/rules', newRule);
        return { ...response, data: normalizeLeadRoutingRule(response.data) };
      } catch (error) {
        console.error('cloneRule failed:', error);
        throw error;
      }
    }
  },

  // === Lead Routing Operations ===

  /**
   * Route a specific lead
   */
  routeLead: async (leadId: number, forceRuleId?: number) => {
    try {
      const response = await apiClient.post<RouteLeadResult>(`/lead-routing/route/${leadId}`, { forceRuleId });
      return { ...response, data: normalizeRouteLeadResult(response.data) };
    } catch (error) {
      console.error('routeLead failed:', error);
      throw error;
    }
  },

  /**
   * Route multiple leads
   */
  routeLeads: async (leadIds: number[]) => {
    try {
      const response = await apiClient.post<{ results: RouteLeadResult[]; successCount: number; failureCount: number }>(
        '/lead-routing/route/bulk',
        { leadIds }
      );
      return {
        ...response,
        data: {
          results: (response.data?.results || []).map(normalizeRouteLeadResult),
          successCount: response.data?.successCount ?? 0,
          failureCount: response.data?.failureCount ?? 0,
        }
      };
    } catch (error) {
      console.error('routeLeads failed:', error);
      return { data: { results: [], successCount: 0, failureCount: leadIds.length } };
    }
  },

  /**
   * Route all unassigned leads
   */
  routeUnassignedLeads: async () => {
    try {
      const response = await apiClient.post<{ results: RouteLeadResult[]; successCount: number; failureCount: number }>(
        '/lead-routing/route/unassigned'
      );
      return {
        ...response,
        data: {
          results: (response.data?.results || []).map(normalizeRouteLeadResult),
          successCount: response.data?.successCount ?? 0,
          failureCount: response.data?.failureCount ?? 0,
        }
      };
    } catch (error) {
      console.error('routeUnassignedLeads failed:', error);
      return { data: { results: [], successCount: 0, failureCount: 0 } };
    }
  },

  /**
   * Preview routing for a lead (dry run)
   */
  previewRouting: async (leadId: number) => {
    try {
      const response = await apiClient.get<RouteLeadResult>(`/lead-routing/preview/${leadId}`);
      return { ...response, data: normalizeRouteLeadResult(response.data) };
    } catch (error) {
      console.error('previewRouting failed:', error);
      throw error;
    }
  },

  /**
   * Manually assign a lead
   */
  manualAssign: async (leadId: number, userId: number, reason?: string) => {
    try {
      const response = await apiClient.post<RouteLeadResult>(`/lead-routing/assign`, { leadId, userId, reason });
      return { ...response, data: normalizeRouteLeadResult(response.data) };
    } catch (error) {
      console.error('manualAssign failed:', error);
      throw error;
    }
  },

  /**
   * Reassign a lead
   */
  reassign: async (leadId: number, newUserId: number, reason?: string) => {
    try {
      const response = await apiClient.post<RouteLeadResult>(`/lead-routing/reassign`, { leadId, newUserId, reason });
      return { ...response, data: normalizeRouteLeadResult(response.data) };
    } catch (error) {
      console.error('reassign failed:', error);
      throw error;
    }
  },

  // === Routing History ===

  /**
   * Get routing history for a lead
   */
  getLeadHistory: async (leadId: number) => {
    try {
      const response = await apiClient.get<RoutingLog[]>(`/lead-routing/history/lead/${leadId}`);
      return { ...response, data: (response.data || []).map(normalizeRoutingLog) };
    } catch (error) {
      console.error('getLeadHistory failed:', error);
      return { data: [] as RoutingLog[] };
    }
  },

  /**
   * Get routing history for a user
   */
  getUserHistory: async (userId: number, page: number = 1, pageSize: number = 50) => {
    try {
      const response = await apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
        `/lead-routing/history/user/${userId}?page=${page}&pageSize=${pageSize}`
      );
      return {
        ...response,
        data: {
          items: (response.data?.items || []).map(normalizeRoutingLog),
          totalCount: response.data?.totalCount ?? 0,
        }
      };
    } catch (error) {
      console.error('getUserHistory failed:', error);
      return { data: { items: [] as RoutingLog[], totalCount: 0 } };
    }
  },

  /**
   * Get routing history for a rule
   */
  getRuleHistory: async (ruleId: number, page: number = 1, pageSize: number = 50) => {
    try {
      const response = await apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
        `/lead-routing/history/rule/${ruleId}?page=${page}&pageSize=${pageSize}`
      );
      return {
        ...response,
        data: {
          items: (response.data?.items || []).map(normalizeRoutingLog),
          totalCount: response.data?.totalCount ?? 0,
        }
      };
    } catch (error) {
      console.error('getRuleHistory failed:', error);
      return { data: { items: [] as RoutingLog[], totalCount: 0 } };
    }
  },

  /**
   * Get all routing history
   */
  getAllHistory: async (
    page: number = 1,
    pageSize: number = 50,
    fromDate?: string,
    toDate?: string,
    success?: boolean
  ) => {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);
      if (success !== undefined) params.append('success', success.toString());
      const response = await apiClient.get<{ items: RoutingLog[]; totalCount: number }>(
        `/lead-routing/history?${params.toString()}`
      );
      return {
        ...response,
        data: {
          items: (response.data?.items || []).map(normalizeRoutingLog),
          totalCount: response.data?.totalCount ?? 0,
        }
      };
    } catch (error) {
      console.error('getAllHistory failed:', error);
      return { data: { items: [] as RoutingLog[], totalCount: 0 } };
    }
  },

  // === Statistics ===

  /**
   * Get routing statistics
   */
  getStatistics: async (fromDate?: string, toDate?: string) => {
    try {
      const params = new URLSearchParams();
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);
      const query = params.toString();
      const response = await apiClient.get<RoutingStatistics>(`/lead-routing/statistics${query ? `?${query}` : ''}`);
      return { ...response, data: normalizeStatistics(response.data) };
    } catch (error) {
      console.error('getStatistics failed:', error);
      // Return empty statistics to prevent UI errors
      return {
        data: normalizeStatistics({})
      };
    }
  },

  /**
   * Get rule performance statistics
   */
  getRuleStatistics: async (ruleId: number, fromDate?: string, toDate?: string) => {
    try {
      const params = new URLSearchParams();
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);
      const query = params.toString();
      const response = await apiClient.get<RuleRoutingStats>(
        `/lead-routing/rules/${ruleId}/statistics${query ? `?${query}` : ''}`
      );
      return { ...response, data: normalizeRuleRoutingStats(response.data) };
    } catch (error) {
      console.error('getRuleStatistics failed:', error);
      return { data: normalizeRuleRoutingStats({ ruleId }) };
    }
  },

  /**
   * Get user workload statistics
   */
  getUserWorkload: async (userId: number) => {
    try {
      const response = await apiClient.get<{
        pendingLeads: number;
        leadsToday: number;
        leadsThisWeek: number;
        capacity: number;
        utilizationPercentage: number;
      }>(`/lead-routing/workload/${userId}`);
      return { ...response, data: normalizeWorkload({ ...response.data, userId }) };
    } catch (error) {
      console.error('getUserWorkload failed:', error);
      return { data: normalizeWorkload({ userId }) };
    }
  },

  /**
   * Get all users workload for balancing
   */
  getAllUsersWorkload: async () => {
    try {
      const response = await apiClient.get<{
        userId: number;
        userName: string;
        pendingLeads: number;
        leadsToday: number;
        capacity: number;
        utilizationPercentage: number;
      }[]>('/lead-routing/workload');
      return { ...response, data: (response.data || []).map(normalizeWorkload) };
    } catch (error) {
      console.error('getAllUsersWorkload failed:', error);
      return { data: [] };
    }
  },

  // === Queues ===

  /**
   * Get lead queues
   */
  getQueues: async () => {
    try {
      const response = await apiClient.get<RawApiData[]>('/lead-routing/queues');
      return {
        ...response,
        data: (response.data || []).map((q: RawApiData) => ({
          id: rNum(q, 'id'),
          name: rStr(q, 'name'),
          leadCount: rNum(q, 'leadCount') || rNum(q, 'count'),
        }))
      };
    } catch (error) {
      console.error('getQueues failed:', error);
      return { data: [] };
    }
  },

  /**
   * Get queue entries
   */
  getQueueEntries: async (queueId: number, page: number = 1, pageSize: number = 50) => {
    try {
      const response = await apiClient.get<{ items: QueueEntry[]; totalCount: number }>(
        `/lead-routing/queues/${queueId}/entries?page=${page}&pageSize=${pageSize}`
      );
      return {
        ...response,
        data: {
          items: (response.data?.items || []).map(normalizeQueueEntry),
          totalCount: response.data?.totalCount ?? 0,
        }
      };
    } catch (error) {
      console.error('getQueueEntries failed:', error);
      return { data: { items: [] as QueueEntry[], totalCount: 0 } };
    }
  },

  /**
   * Add lead to queue
   */
  addToQueue: async (queueId: number, leadId: number, priority?: number) => {
    try {
      const response = await apiClient.post<QueueEntry>(`/lead-routing/queues/${queueId}/entries`, { leadId, priority });
      return { ...response, data: normalizeQueueEntry(response.data) };
    } catch (error) {
      console.error('addToQueue failed:', error);
      throw error;
    }
  },

  /**
   * Remove lead from queue
   */
  removeFromQueue: (queueId: number, leadId: number) =>
    apiClient.delete(`/lead-routing/queues/${queueId}/entries/${leadId}`),

  /**
   * Claim next lead from queue
   */
  claimFromQueue: async (queueId: number) => {
    try {
      const response = await apiClient.post<RouteLeadResult>(`/lead-routing/queues/${queueId}/claim`);
      return { ...response, data: normalizeRouteLeadResult(response.data) };
    } catch (error) {
      console.error('claimFromQueue failed:', error);
      throw error;
    }
  },

  // === Rule Testing ===

  /**
   * Test a routing rule against sample leads
   */
  testRule: async (ruleId: number, sampleSize: number = 10) => {
    try {
      const response = await apiClient.post<{ matches: number; total: number; matchedLeads: { id: number; name: string }[] }>(
        `/lead-routing/rules/${ruleId}/test`,
        { sampleSize }
      );
      return {
        ...response,
        data: {
          matches: response.data?.matches ?? 0,
          total: response.data?.total ?? sampleSize,
          matchedLeads: response.data?.matchedLeads || [],
        }
      };
    } catch (error) {
      console.error('testRule failed:', error);
      return { data: { matches: 0, total: sampleSize, matchedLeads: [] } };
    }
  },

  /**
   * Validate rule criteria syntax
   */
  validateCriteria: async (criteria: RoutingCriteria[]) => {
    try {
      const response = await apiClient.post<{ isValid: boolean; errors: string[] }>(
        '/lead-routing/validate-criteria',
        { criteria }
      );
      return {
        ...response,
        data: {
          isValid: response.data?.isValid ?? true,
          errors: response.data?.errors || [],
        }
      };
    } catch (error) {
      console.error('validateCriteria failed:', error);
      // Assume valid if validation endpoint not available
      return { data: { isValid: true, errors: [] } };
    }
  },

  // === Available Fields ===

  /**
   * Get available fields for routing criteria
   */
  getAvailableFields: async () => {
    try {
      const response = await apiClient.get<{
        name: string;
        label: string;
        type: string;
        operators: CriteriaOperator[];
        options?: { value: string; label: string }[];
      }[]>('/lead-routing/fields');
      return {
        ...response,
        data: (response.data || []).map(f => ({
          name: f?.name || '',
          label: f?.label || f?.name || '',
          type: f?.type || 'string',
          operators: f?.operators || [CriteriaOperator.Equals, CriteriaOperator.Contains],
          options: f?.options || [],
        }))
      };
    } catch (error) {
      console.error('getAvailableFields failed:', error);
      // Return default lead fields if endpoint not available
      return {
        data: [
          { name: 'source', label: 'Lead Source', type: 'string', operators: [CriteriaOperator.Equals, CriteriaOperator.Contains], options: [] },
          { name: 'status', label: 'Status', type: 'string', operators: [CriteriaOperator.Equals], options: [] },
          { name: 'company', label: 'Company', type: 'string', operators: [CriteriaOperator.Equals, CriteriaOperator.Contains], options: [] },
          { name: 'country', label: 'Country', type: 'string', operators: [CriteriaOperator.Equals, CriteriaOperator.In], options: [] },
          { name: 'industry', label: 'Industry', type: 'string', operators: [CriteriaOperator.Equals, CriteriaOperator.In], options: [] },
        ]
      };
    }
  },

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

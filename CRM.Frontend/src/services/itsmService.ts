/**
 * ITSM Service - Centralized API service for IT Service Management module
 * Replaces raw axios calls in ITSM pages with typed, reusable methods
 */
import apiClient from './apiClient';

// ============================================================================
// Types
// ============================================================================

export interface Incident {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  impact: string;
  urgency: string;
  category?: string;
  subcategory?: string;
  assignedToId?: number;
  assignedToName?: string;
  reportedById?: number;
  reportedByName?: string;
  resolvedAt?: string;
  closedAt?: string;
  resolutionNotes?: string;
  slaBreached?: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Problem {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  impact: string;
  category?: string;
  rootCause?: string;
  workaround?: string;
  knownError: boolean;
  assignedToId?: number;
  assignedToName?: string;
  relatedIncidentIds?: number[];
  createdAt: string;
  updatedAt: string;
}

export interface ChangeRequest {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  type: string;
  impact: string;
  risk: string;
  category?: string;
  requestedById?: number;
  requestedByName?: string;
  assignedToId?: number;
  assignedToName?: string;
  scheduledStartDate?: string;
  scheduledEndDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  rollbackPlan?: string;
  approvalStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface KnowledgeArticle {
  id: number;
  title: string;
  content: string;
  summary?: string;
  status: string;
  category?: string;
  tags?: string[];
  authorId?: number;
  authorName?: string;
  viewCount: number;
  helpfulCount: number;
  notHelpfulCount: number;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ConfigurationItem {
  id: number;
  name: string;
  type: string;
  status: string;
  environment?: string;
  owner?: string;
  description?: string;
  serialNumber?: string;
  location?: string;
  manufacturer?: string;
  model?: string;
  version?: string;
  ipAddress?: string;
  relationships?: CIRelationship[];
  createdAt: string;
  updatedAt: string;
}

export interface CIRelationship {
  id: number;
  sourceId: number;
  targetId: number;
  type: string;
  sourceName?: string;
  targetName?: string;
}

export interface ServiceCatalogItem {
  id: number;
  name: string;
  description: string;
  category: string;
  status: string;
  slaId?: number;
  approvalRequired: boolean;
  estimatedDeliveryDays?: number;
  cost?: number;
  iconUrl?: string;
  formDefinitionId?: number;
  createdAt: string;
  updatedAt: string;
}

export interface ServiceCatalogRequest {
  id: number;
  catalogItemId: number;
  catalogItemName?: string;
  requestedById: number;
  requestedByName?: string;
  status: string;
  priority: string;
  description?: string;
  formData?: Record<string, unknown>;
  assignedToId?: number;
  assignedToName?: string;
  approvalStatus?: string;
  createdAt: string;
  updatedAt: string;
}

export interface SLAPolicy {
  id: number;
  name: string;
  description?: string;
  targetResponseTime: number;
  targetResolutionTime: number;
  priority: string;
  isActive: boolean;
  escalationRules?: EscalationRule[];
  createdAt: string;
  updatedAt: string;
}

export interface SLAInstance {
  id: number;
  policyId: number;
  policyName?: string;
  entityType: string;
  entityId: number;
  status: string;
  responseDeadline?: string;
  resolutionDeadline?: string;
  respondedAt?: string;
  resolvedAt?: string;
  isResponseBreached: boolean;
  isResolutionBreached: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface EscalationRule {
  id: number;
  name: string;
  condition: string;
  action: string;
  targetUserId?: number;
  targetGroupId?: number;
  escalationMinutes: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ITSMDashboardMetrics {
  openIncidents: number;
  criticalIncidents: number;
  openProblems: number;
  pendingChanges: number;
  slaComplianceRate: number;
  averageResolutionTime: number;
  openServiceRequests: number;
  knowledgeArticleCount: number;
}

// ============================================================================
// Incidents
// ============================================================================

export const incidentService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<Incident>>('/api/itsm/incidents', { params }),

  getById: (id: number) =>
    apiClient.get<Incident>(`/api/itsm/incidents/${id}`),

  create: (data: Partial<Incident>) =>
    apiClient.post<Incident>('/api/itsm/incidents', data),

  update: (id: number, data: Partial<Incident>) =>
    apiClient.put<Incident>(`/api/itsm/incidents/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/incidents/${id}`),

  updateStatus: (id: number, status: string) =>
    apiClient.patch(`/api/itsm/incidents/${id}/status`, { status }),

  assign: (id: number, userId: number) =>
    apiClient.patch(`/api/itsm/incidents/${id}/assign`, { assignedToId: userId }),

  getTimeline: (id: number) =>
    apiClient.get(`/api/itsm/incidents/${id}/timeline`),

  getRelated: (id: number) =>
    apiClient.get<Incident[]>(`/api/itsm/incidents/${id}/related`),

  linkToProblem: (id: number, problemId: number) =>
    apiClient.post(`/api/itsm/incidents/${id}/problems/${problemId}`),
};

// ============================================================================
// Problems
// ============================================================================

export const problemService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<Problem>>('/api/itsm/problems', { params }),

  getById: (id: number) =>
    apiClient.get<Problem>(`/api/itsm/problems/${id}`),

  create: (data: Partial<Problem>) =>
    apiClient.post<Problem>('/api/itsm/problems', data),

  update: (id: number, data: Partial<Problem>) =>
    apiClient.put<Problem>(`/api/itsm/problems/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/problems/${id}`),

  updateStatus: (id: number, status: string) =>
    apiClient.patch(`/api/itsm/problems/${id}/status`, { status }),

  getRelatedIncidents: (id: number) =>
    apiClient.get<Incident[]>(`/api/itsm/problems/${id}/incidents`),

  addRootCause: (id: number, rootCause: string) =>
    apiClient.patch(`/api/itsm/problems/${id}/root-cause`, { rootCause }),

  markAsKnownError: (id: number, workaround: string) =>
    apiClient.patch(`/api/itsm/problems/${id}/known-error`, { workaround }),
};

// ============================================================================
// Changes
// ============================================================================

export const changeService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<ChangeRequest>>('/api/itsm/changes', { params }),

  getById: (id: number) =>
    apiClient.get<ChangeRequest>(`/api/itsm/changes/${id}`),

  create: (data: Partial<ChangeRequest>) =>
    apiClient.post<ChangeRequest>('/api/itsm/changes', data),

  update: (id: number, data: Partial<ChangeRequest>) =>
    apiClient.put<ChangeRequest>(`/api/itsm/changes/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/changes/${id}`),

  updateStatus: (id: number, status: string) =>
    apiClient.patch(`/api/itsm/changes/${id}/status`, { status }),

  approve: (id: number, comment?: string) =>
    apiClient.post(`/api/itsm/changes/${id}/approve`, { comment }),

  reject: (id: number, reason: string) =>
    apiClient.post(`/api/itsm/changes/${id}/reject`, { reason }),

  getCalendar: (params?: { startDate?: string; endDate?: string }) =>
    apiClient.get<ChangeRequest[]>('/api/itsm/changes/calendar', { params }),

  getConflicts: (id: number) =>
    apiClient.get(`/api/itsm/changes/${id}/conflicts`),
};

// ============================================================================
// Knowledge Base
// ============================================================================

export const knowledgeService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<KnowledgeArticle>>('/api/itsm/knowledge', { params }),

  getById: (id: number) =>
    apiClient.get<KnowledgeArticle>(`/api/itsm/knowledge/${id}`),

  create: (data: Partial<KnowledgeArticle>) =>
    apiClient.post<KnowledgeArticle>('/api/itsm/knowledge', data),

  update: (id: number, data: Partial<KnowledgeArticle>) =>
    apiClient.put<KnowledgeArticle>(`/api/itsm/knowledge/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/knowledge/${id}`),

  publish: (id: number) =>
    apiClient.patch(`/api/itsm/knowledge/${id}/publish`),

  archive: (id: number) =>
    apiClient.patch(`/api/itsm/knowledge/${id}/archive`),

  submitFeedback: (id: number, helpful: boolean, comment?: string) =>
    apiClient.post(`/api/itsm/knowledge/${id}/feedback`, { helpful, comment }),

  search: (query: string) =>
    apiClient.get<KnowledgeArticle[]>('/api/itsm/knowledge/search', { params: { query } }),

  getSuggestions: (context: string) =>
    apiClient.get<KnowledgeArticle[]>('/api/itsm/knowledge/suggestions', { params: { context } }),
};

// ============================================================================
// CMDB (Configuration Management Database)
// ============================================================================

export const cmdbService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<ConfigurationItem>>('/api/itsm/cmdb', { params }),

  getById: (id: number) =>
    apiClient.get<ConfigurationItem>(`/api/itsm/cmdb/${id}`),

  create: (data: Partial<ConfigurationItem>) =>
    apiClient.post<ConfigurationItem>('/api/itsm/cmdb', data),

  update: (id: number, data: Partial<ConfigurationItem>) =>
    apiClient.put<ConfigurationItem>(`/api/itsm/cmdb/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/cmdb/${id}`),

  getRelationships: (id: number) =>
    apiClient.get<CIRelationship[]>(`/api/itsm/cmdb/${id}/relationships`),

  addRelationship: (sourceId: number, targetId: number, type: string) =>
    apiClient.post(`/api/itsm/cmdb/${sourceId}/relationships`, { targetId, type }),

  removeRelationship: (sourceId: number, relationshipId: number) =>
    apiClient.delete(`/api/itsm/cmdb/${sourceId}/relationships/${relationshipId}`),

  getImpactAnalysis: (id: number) =>
    apiClient.get(`/api/itsm/cmdb/${id}/impact-analysis`),

  getServiceMap: () =>
    apiClient.get('/api/itsm/cmdb/service-map'),
};

// ============================================================================
// Service Catalog
// ============================================================================

export const serviceCatalogService = {
  getAll: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<ServiceCatalogItem>>('/api/itsm/catalog', { params }),

  getById: (id: number) =>
    apiClient.get<ServiceCatalogItem>(`/api/itsm/catalog/${id}`),

  create: (data: Partial<ServiceCatalogItem>) =>
    apiClient.post<ServiceCatalogItem>('/api/itsm/catalog', data),

  update: (id: number, data: Partial<ServiceCatalogItem>) =>
    apiClient.put<ServiceCatalogItem>(`/api/itsm/catalog/${id}`, data),

  delete: (id: number) =>
    apiClient.delete(`/api/itsm/catalog/${id}`),

  getCategories: () =>
    apiClient.get<string[]>('/api/itsm/catalog/categories'),

  // Requests
  getRequests: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<ServiceCatalogRequest>>('/api/itsm/catalog/requests', { params }),

  getRequestById: (id: number) =>
    apiClient.get<ServiceCatalogRequest>(`/api/itsm/catalog/requests/${id}`),

  createRequest: (data: Partial<ServiceCatalogRequest>) =>
    apiClient.post<ServiceCatalogRequest>('/api/itsm/catalog/requests', data),

  updateRequestStatus: (id: number, status: string) =>
    apiClient.patch(`/api/itsm/catalog/requests/${id}/status`, { status }),
};

// ============================================================================
// SLA Management
// ============================================================================

export const slaService = {
  // Policies
  getPolicies: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<SLAPolicy>>('/api/itsm/sla/policies', { params }),

  getPolicyById: (id: number) =>
    apiClient.get<SLAPolicy>(`/api/itsm/sla/policies/${id}`),

  createPolicy: (data: Partial<SLAPolicy>) =>
    apiClient.post<SLAPolicy>('/api/itsm/sla/policies', data),

  updatePolicy: (id: number, data: Partial<SLAPolicy>) =>
    apiClient.put<SLAPolicy>(`/api/itsm/sla/policies/${id}`, data),

  deletePolicy: (id: number) =>
    apiClient.delete(`/api/itsm/sla/policies/${id}`),

  // Instances
  getInstances: (params?: Record<string, unknown>) =>
    apiClient.get<PaginatedResponse<SLAInstance>>('/api/itsm/sla/instances', { params }),

  getInstanceById: (id: number) =>
    apiClient.get<SLAInstance>(`/api/itsm/sla/instances/${id}`),

  // Dashboard
  getDashboard: () =>
    apiClient.get<ITSMDashboardMetrics>('/api/itsm/dashboard'),
};

// ============================================================================
// ITSM Dashboard / Metrics
// ============================================================================

export const itsmDashboardService = {
  getMetrics: () =>
    apiClient.get<ITSMDashboardMetrics>('/api/itsm/dashboard'),

  getOverview: () =>
    apiClient.get('/api/itsm/overview'),
};

// ============================================================================
// Default export combining all services
// ============================================================================

const itsmService = {
  incidents: incidentService,
  problems: problemService,
  changes: changeService,
  knowledge: knowledgeService,
  cmdb: cmdbService,
  catalog: serviceCatalogService,
  sla: slaService,
  dashboard: itsmDashboardService,
};

export default itsmService;

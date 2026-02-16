/**
 * ITSM Service - Incidents, Problems, Changes, SLA Management
 * Fully typed service layer for all ITSM operations
 */

import apiClient from './apiClient';
import {
  Incident,
  Problem,
  Change,
  ChangeApproval,
  CABVote,
  SLAPolicy,
  SLAStatus,
  ConfigurationItem,
  KnowledgeArticle,
  EscalationRule,
  WorkflowDefinition,
  ServiceRequest,
  CreateIncidentDto,
  UpdateIncidentDto,
  CreateProblemDto,
  UpdateProblemDto,
  CreateChangeDto,
  UpdateChangeDto,
  CreateServiceRequestDto,
  PaginatedResponse,
} from '../types/itsm';

/**
 * ITSM Service offering CRUD operations for all ITSM entities
 */
const itsmService = {
  // =========================================================================
  // INCIDENTS / SERVICE REQUESTS
  // =========================================================================

  /**
   * Get all incidents with pagination and filtering
   */
  getIncidents: async (
    page: number = 1,
    pageSize: number = 20,
    filters?: Record<string, string | number>
  ) => {
    return apiClient.get<PaginatedResponse<Incident>>('/servicerequests', {
      params: { page, pageSize, ...filters }
    });
  },

  /**
   * Get single incident by ID
   */
  getIncidentById: async (id: number) => {
    return apiClient.get<Incident>(`/servicerequests/${id}`);
  },

  /**
   * Search incidents
   */
  searchIncidents: async (query: string) => {
    return apiClient.get<Incident[]>('/servicerequests/search', {
      params: { q: query }
    });
  },

  /**
   * Create new incident
   */
  createIncident: async (data: CreateIncidentDto) => {
    return apiClient.post<Incident>('/servicerequests', data);
  },

  /**
   * Update existing incident
   */
  updateIncident: async (id: number, data: UpdateIncidentDto) => {
    return apiClient.patch<Incident>(`/servicerequests/${id}`, data);
  },

  /**
   * Get incidents by status
   */
  getIncidentsByStatus: async (status: string) => {
    return apiClient.get<Incident[]>(`/servicerequests/status/${status}`);
  },

  /**
   * Get incidents assigned to user
   */
  getMyIncidents: async () => {
    return apiClient.get<Incident[]>('/servicerequests/assigned-to-me');
  },

  /**
   * Assign incident to user
   */
  assignIncident: async (incidentId: number, userId: number) => {
    return apiClient.patch<Incident>(`/servicerequests/${incidentId}/assign`, {
      assignedTo: userId
    });
  },

  /**
   * Resolve incident
   */
  resolveIncident: async (incidentId: number, resolution: string) => {
    return apiClient.patch<Incident>(`/servicerequests/${incidentId}/resolve`, {
      resolution
    });
  },

  /**
   * Close incident
   */
  closeIncident: async (incidentId: number) => {
    return apiClient.patch<Incident>(`/servicerequests/${incidentId}/close`, {});
  },

  /**
   * Reopen incident
   */
  reopenIncident: async (incidentId: number) => {
    return apiClient.patch<Incident>(`/servicerequests/${incidentId}/reopen`, {});
  },

  /**
   * Get incident timeline/history
   */
  getIncidentTimeline: async (incidentId: number) => {
    return apiClient.get(`/servicerequests/${incidentId}/timeline`);
  },

  /**
   * Add note to incident
   */
  addIncidentNote: async (incidentId: number, content: string) => {
    return apiClient.post(`/servicerequests/${incidentId}/notes`, { content });
  },

  // =========================================================================
  // PROBLEMS
  // =========================================================================

  /**
   * Get all problems
   */
  getProblems: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Problem>>('/problems', {
      params: { page, pageSize }
    });
  },

  /**
   * Get problem by ID
   */
  getProblemById: async (id: number) => {
    return apiClient.get<Problem>(`/problems/${id}`);
  },

  /**
   * Create problem
   */
  createProblem: async (data: CreateProblemDto) => {
    return apiClient.post<Problem>('/problems', data);
  },

  /**
   * Update problem
   */
  updateProblem: async (id: number, data: UpdateProblemDto) => {
    return apiClient.patch<Problem>(`/problems/${id}`, data);
  },

  /**
   * Get problems by status
   */
  getProblemsByStatus: async (status: string) => {
    return apiClient.get<Problem[]>(`/problems/status/${status}`);
  },

  // =========================================================================
  // CHANGES
  // =========================================================================

  /**
   * Get all changes
   */
  getChanges: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<Change>>('/changes', {
      params: { page, pageSize }
    });
  },

  /**
   * Get change by ID
   */
  getChangeById: async (id: number) => {
    return apiClient.get<Change>(`/changes/${id}`);
  },

  /**
   * Create change
   */
  createChange: async (data: CreateChangeDto) => {
    return apiClient.post<Change>('/changes', data);
  },

  /**
   * Update change
   */
  updateChange: async (id: number, data: UpdateChangeDto) => {
    return apiClient.patch<Change>(`/changes/${id}`, data);
  },

  /**
   * Submit change for approval
   */
  submitChangeForApproval: async (changeId: number) => {
    return apiClient.post(`/changes/${changeId}/submit-for-approval`, {});
  },

  /**
   * Get pending approvals for change
   */
  getChangeApprovals: async (changeId: number) => {
    return apiClient.get<ChangeApproval[]>(`/changes/${changeId}/approvals`);
  },

  /**
   * Approve change
   */
  approveChange: async (changeId: number, approverId: number, comments?: string) => {
    return apiClient.post<ChangeApproval>(
      `/changes/${changeId}/approvals`,
      { approverId, comments, status: 'approved' }
    );
  },

  /**
   * Reject change
   */
  rejectChange: async (changeId: number, approverId: number, comments: string) => {
    return apiClient.post<ChangeApproval>(
      `/changes/${changeId}/approvals`,
      { approverId, comments, status: 'rejected' }
    );
  },

  /**
   * Get CAB votes for change
   */
  getCABVotes: async (changeId: number) => {
    return apiClient.get<CABVote[]>(`/changes/${changeId}/cab-votes`);
  },

  /**
   * Cast CAB vote
   */
  castCABVote: async (
    changeId: number,
    voterId: number,
    vote: 'approve' | 'reject' | 'abstain',
    comments?: string
  ) => {
    return apiClient.post<CABVote>(`/changes/${changeId}/cab-votes`, {
      voterId,
      vote,
      comments
    });
  },

  /**
   * Schedule change implementation
   */
  scheduleChange: async (changeId: number, startDate: string, endDate: string) => {
    return apiClient.patch<Change>(`/changes/${changeId}`, {
      status: 'scheduled',
      startDate,
      endDate
    });
  },

  /**
   * Implement change (mark as in progress)
   */
  implementChange: async (changeId: number) => {
    return apiClient.post(`/changes/${changeId}/implement`, {});
  },

  /**
   * Complete change
   */
  completeChange: async (changeId: number) => {
    return apiClient.post(`/changes/${changeId}/complete`, {});
  },

  /**
   * Rollback change
   */
  rollbackChange: async (changeId: number, reason?: string) => {
    return apiClient.post(`/changes/${changeId}/rollback`, { reason });
  },

  // =========================================================================
  // SLA MANAGEMENT
  // =========================================================================

  /**
   * Get all SLA policies
   */
  getSLAPolicies: async () => {
    return apiClient.get<SLAPolicy[]>('/sla-policies');
  },

  /**
   * Get SLA status for incident
   */
  getIncidentSLAStatus: async (incidentId: number) => {
    return apiClient.get<SLAStatus>(`/servicerequests/${incidentId}/sla-status`);
  },

  // =========================================================================
  // CONFIGURATION ITEMS
  // =========================================================================

  /**
   * Get all configuration items
   */
  getConfigurationItems: async () => {
    return apiClient.get<ConfigurationItem[]>('/cmdb');
  },

  /**
   * Get CI by ID
   */
  getConfigurationItemById: async (id: number) => {
    return apiClient.get<ConfigurationItem>(`/cmdb/${id}`);
  },

  // =========================================================================
  // KNOWLEDGE BASE
  // =========================================================================

  /**
   * Get all knowledge articles
   */
  getKnowledgeArticles: async (page: number = 1, pageSize: number = 20) => {
    return apiClient.get<PaginatedResponse<KnowledgeArticle>>('/knowledge-articles', {
      params: { page, pageSize }
    });
  },

  /**
   * Search knowledge articles
   */
  searchKnowledgeArticles: async (query: string) => {
    return apiClient.get<KnowledgeArticle[]>('/knowledge-articles/search', {
      params: { q: query }
    });
  },

  /**
   * Get article by ID
   */
  getKnowledgeArticleById: async (id: number) => {
    return apiClient.get<KnowledgeArticle>(`/knowledge-articles/${id}`);
  },

  // =========================================================================
  // ESCALATION RULES
  // =========================================================================

  /**
   * Get escalation rules
   */
  getEscalationRules: async () => {
    return apiClient.get<EscalationRule[]>('/escalation-rules');
  },

  /**
   * Get escalation rule by ID
   */
  getEscalationRuleById: async (id: number) => {
    return apiClient.get<EscalationRule>(`/escalation-rules/${id}`);
  },

  // =========================================================================
  // WORKFLOWS
  // =========================================================================

  /**
   * Get workflow definitions
   */
  getWorkflowDefinitions: async () => {
    return apiClient.get<WorkflowDefinition[]>('/workflows');
  },

  /**
   * Get workflow by ID
   */
  getWorkflowById: async (id: number) => {
    return apiClient.get<WorkflowDefinition>(`/workflows/${id}`);
  },

  // =========================================================================
  // ANALYTICS & REPORTING
  // =========================================================================

  /**
   * Get ITSM dashboard metrics
   */
  getITSMMetrics: async () => {
    return apiClient.get('/itsm/metrics');
  },

  /**
   * Get incident statistics
   */
  getIncidentStatistics: async () => {
    return apiClient.get('/servicerequests/statistics');
  },

  /**
   * Get SLA compliance report
   */
  getSLAComplianceReport: async (startDate: string, endDate: string) => {
    return apiClient.get('/itsm/sla-compliance', {
      params: { startDate, endDate }
    });
  }
};

export default itsmService;

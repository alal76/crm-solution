/**
 * Change Service - Manages ITSM changes and CAB approvals
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum ChangeStatus {
  Draft = 0,
  SubmittedForApproval = 1,
  ApprovalInProgress = 2,
  Approved = 3,
  Rejected = 4,
  Scheduled = 5,
  InProgress = 6,
  OnHold = 7,
  Completed = 8,
  Rolled_Back = 9,
  Cancelled = 10,
}

export enum ChangePriority {
  Critical = 0,
  High = 1,
  Medium = 2,
  Low = 3,
  Planning = 4,
}

export enum ChangeRiskLevel {
  Low = 0,
  Medium = 1,
  High = 2,
  VeryHigh = 3,
}

export enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Deferred = 3,
}

// ============================================================================
// Interfaces
// ============================================================================

export interface Change {
  id: number;
  number: string;
  title: string;
  description: string;
  status: ChangeStatus;
  priority: ChangePriority;
  riskLevel: ChangeRiskLevel;
  createdById: number;
  createdByName?: string;
  implementedById?: number;
  implementedByName?: string;
  startDate: string;
  endDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  impactAnalysis: string;
  rollbackPlan: string;
  testingPlan?: string;
  communicationPlan?: string;
  affectedServices?: string[];
  relatedIncidents?: number[];
  relatedProblems?: number[];
  relatedAssets?: number[];
  approvals?: ChangeApproval[];
  createdAt: string;
  updatedAt: string;
}

export interface ChangeApproval {
  id: number;
  changeId: number;
  approverId: number;
  approverName?: string;
  status: ApprovalStatus;
  comments?: string;
  createdAt: string;
  respondedAt?: string;
}

export interface ChangeActivity {
  id: number;
  changeId: number;
  type: 'status_change' | 'comment' | 'approval' | 'attachment';
  userId: number;
  userName?: string;
  content: string;
  timestamp: string;
}

export interface CreateChangeRequest {
  title: string;
  description: string;
  priority: ChangePriority;
  riskLevel: ChangeRiskLevel;
  startDate: string;
  endDate: string;
  impactAnalysis: string;
  rollbackPlan: string;
}

export interface UpdateChangeRequest {
  title?: string;
  description?: string;
  priority?: ChangePriority;
  riskLevel?: ChangeRiskLevel;
  status?: ChangeStatus;
  startDate?: string;
  endDate?: string;
  impactAnalysis?: string;
  rollbackPlan?: string;
  testingPlan?: string;
  communicationPlan?: string;
}

export interface PagedChangeResult {
  items: Change[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ChangeCalendarEvent {
  id: number;
  title: string;
  start: string;
  end: string;
  status: ChangeStatus;
  priority: ChangePriority;
  riskLevel: ChangeRiskLevel;
}

export interface ChangeStatistics {
  totalChanges: number;
  approvedChanges: number;
  scheduledChanges: number;
  completedChanges: number;
  successRate: number;
  avgApprovalTime: number;
}

// ============================================================================
// Service
// ============================================================================

const changeService = {
  /**
   * Get all changes with pagination and filtering
   */
  getChanges: async (
    page: number = 1,
    pageSize: number = 20,
    filters?: {
      status?: ChangeStatus;
      priority?: ChangePriority;
      riskLevel?: ChangeRiskLevel;
      implementedById?: number;
      search?: string;
      startDate?: string;
      endDate?: string;
    }
  ): Promise<PagedChangeResult> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (filters) {
      if (filters.status !== undefined) params.append('status', filters.status.toString());
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.riskLevel !== undefined) params.append('riskLevel', filters.riskLevel.toString());
      if (filters.implementedById) params.append('implementedById', filters.implementedById.toString());
      if (filters.search) params.append('search', filters.search);
      if (filters.startDate) params.append('startDate', filters.startDate);
      if (filters.endDate) params.append('endDate', filters.endDate);
    }

    const response = await apiClient.get(`/api/changes?${params}`);
    return response.data;
  },

  /**
   * Get change by ID
   */
  getChange: async (id: number): Promise<Change> => {
    const response = await apiClient.get(`/api/changes/${id}`);
    return response.data;
  },

  /**
   * Create new change
   */
  createChange: async (data: CreateChangeRequest): Promise<Change> => {
    const response = await apiClient.post('/api/changes', data);
    return response.data;
  },

  /**
   * Update change
   */
  updateChange: async (id: number, data: UpdateChangeRequest): Promise<Change> => {
    const response = await apiClient.put(`/api/changes/${id}`, data);
    return response.data;
  },

  /**
   * Change status
   */
  changeStatus: async (id: number, status: ChangeStatus): Promise<Change> => {
    const response = await apiClient.patch(`/api/changes/${id}/status`, { status });
    return response.data;
  },

  /**
   * Submit for approval
   */
  submitForApproval: async (id: number, approvers: number[]): Promise<Change> => {
    const response = await apiClient.post(`/api/changes/${id}/submit-approval`, { approvers });
    return response.data;
  },

  /**
   * Approve change
   */
  approveChange: async (id: number, comments?: string): Promise<ChangeApproval> => {
    const response = await apiClient.post(`/api/changes/${id}/approve`, { comments });
    return response.data;
  },

  /**
   * Reject change
   */
  rejectChange: async (id: number, reason: string): Promise<ChangeApproval> => {
    const response = await apiClient.post(`/api/changes/${id}/reject`, { reason });
    return response.data;
  },

  /**
   * Schedule change
   */
  scheduleChange: async (id: number, startDate: string, endDate: string): Promise<Change> => {
    const response = await apiClient.post(`/api/changes/${id}/schedule`, { startDate, endDate });
    return response.data;
  },

  /**
   * Start change implementation
   */
  startImplementation: async (id: number): Promise<Change> => {
    const response = await apiClient.post(`/api/changes/${id}/start`, {});
    return response.data;
  },

  /**
   * Complete change
   */
  completeChange: async (id: number, comments?: string): Promise<Change> => {
    const response = await apiClient.post(`/api/changes/${id}/complete`, { comments });
    return response.data;
  },

  /**
   * Rollback change
   */
  rollbackChange: async (id: number, reason: string): Promise<Change> => {
    const response = await apiClient.post(`/api/changes/${id}/rollback`, { reason });
    return response.data;
  },

  /**
   * Get change activity timeline
   */
  getActivity: async (changeId: number): Promise<ChangeActivity[]> => {
    const response = await apiClient.get(`/api/changes/${changeId}/activity`);
    return response.data;
  },

  /**
   * Add comment to change
   */
  addComment: async (changeId: number, content: string): Promise<ChangeActivity> => {
    const response = await apiClient.post(`/api/changes/${changeId}/comments`, { content });
    return response.data;
  },

  /**
   * Get change calendar events
   */
  getCalendarEvents: async (startDate: string, endDate: string): Promise<ChangeCalendarEvent[]> => {
    const params = new URLSearchParams({
      startDate,
      endDate,
    });
    const response = await apiClient.get(`/api/changes/calendar?${params}`);
    return response.data;
  },

  /**
   * Get related incidents
   */
  getRelatedIncidents: async (changeId: number): Promise<any[]> => {
    const response = await apiClient.get(`/api/changes/${changeId}/related-incidents`);
    return response.data;
  },

  /**
   * Get related problems
   */
  getRelatedProblems: async (changeId: number): Promise<any[]> => {
    const response = await apiClient.get(`/api/changes/${changeId}/related-problems`);
    return response.data;
  },

  /**
   * Get change statistics
   */
  getStatistics: async (): Promise<ChangeStatistics> => {
    const response = await apiClient.get('/api/changes/statistics');
    return response.data;
  },

  /**
   * Detect conflicting changes
   */
  detectConflicts: async (id: number): Promise<Change[]> => {
    const response = await apiClient.get(`/api/changes/${id}/conflicts`);
    return response.data;
  },

  /**
   * Delete change
   */
  deleteChange: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/changes/${id}`);
  },
};

export default changeService;

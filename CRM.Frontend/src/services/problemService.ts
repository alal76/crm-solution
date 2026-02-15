/**
 * Problem Service - Manages ITSM problems
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum ProblemStatus {
  Draft = 0,
  Open = 1,
  InProgress = 2,
  OnHold = 3,
  Resolved = 4,
  Closed = 5,
  Cancelled = 6,
}

export enum ProblemPriority {
  Critical = 0,
  High = 1,
  Medium = 2,
  Low = 3,
  Planning = 4,
}

export enum ProblemCategory {
  Hardware = 0,
  Software = 1,
  Network = 2,
  Database = 3,
  Application = 4,
  Infrastructure = 5,
  Other = 6,
}

// ============================================================================
// Interfaces
// ============================================================================

export interface Problem {
  id: number;
  number: string;
  title: string;
  description: string;
  status: ProblemStatus;
  priority: ProblemPriority;
  category: ProblemCategory;
  createdById: number;
  createdByName?: string;
  assignedToId?: number;
  assignedToName?: string;
  rootCauseDescription?: string;
  rootCauseAnalysis?: string;
  workaround?: string;
  relatedIncidentCount: number;
  relatedIncidents?: number[];
  relatedChanges?: number[];
  knowledgeArticleId?: number;
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string;
  closedAt?: string;
}

export interface ProblemActivity {
  id: number;
  problemId: number;
  type: 'comment' | 'status_change' | 'attachment' | 'root_cause_added';
  userId: number;
  userName?: string;
  content: string;
  timestamp: string;
}

export interface CreateProblemRequest {
  title: string;
  description: string;
  priority: ProblemPriority;
  category: ProblemCategory;
}

export interface UpdateProblemRequest {
  title?: string;
  description?: string;
  status?: ProblemStatus;
  priority?: ProblemPriority;
  category?: ProblemCategory;
  assignedToId?: number;
  rootCauseDescription?: string;
  rootCauseAnalysis?: string;
  workaround?: string;
}

export interface PagedProblemResult {
  items: Problem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ProblemStatistics {
  totalProblems: number;
  openProblems: number;
  avgResolutionTime: number;
  criticalProblems: number;
  highProblems: number;
}

// ============================================================================
// Service
// ============================================================================

const problemService = {
  /**
   * Get all problems with pagination and filtering
   */
  getProblems: async (
    page: number = 1,
    pageSize: number = 20,
    filters?: {
      status?: ProblemStatus;
      priority?: ProblemPriority;
      category?: ProblemCategory;
      assignedToId?: number;
      search?: string;
    }
  ): Promise<PagedProblemResult> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (filters) {
      if (filters.status !== undefined) params.append('status', filters.status.toString());
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.category !== undefined) params.append('category', filters.category.toString());
      if (filters.assignedToId) params.append('assignedToId', filters.assignedToId.toString());
      if (filters.search) params.append('search', filters.search);
    }

    const response = await apiClient.get(`/api/problems?${params}`);
    return response.data;
  },

  /**
   * Get problem by ID
   */
  getProblem: async (id: number): Promise<Problem> => {
    const response = await apiClient.get(`/api/problems/${id}`);
    return response.data;
  },

  /**
   * Create new problem
   */
  createProblem: async (data: CreateProblemRequest): Promise<Problem> => {
    const response = await apiClient.post('/api/problems', data);
    return response.data;
  },

  /**
   * Update problem
   */
  updateProblem: async (id: number, data: UpdateProblemRequest): Promise<Problem> => {
    const response = await apiClient.put(`/api/problems/${id}`, data);
    return response.data;
  },

  /**
   * Change problem status
   */
  changeStatus: async (id: number, status: ProblemStatus): Promise<Problem> => {
    const response = await apiClient.patch(`/api/problems/${id}/status`, { status });
    return response.data;
  },

  /**
   * Assign problem to user
   */
  assignToUser: async (id: number, userId: number): Promise<Problem> => {
    const response = await apiClient.patch(`/api/problems/${id}/assign`, { assignedToId: userId });
    return response.data;
  },

  /**
   * Get problem activity timeline
   */
  getActivity: async (problemId: number): Promise<ProblemActivity[]> => {
    const response = await apiClient.get(`/api/problems/${problemId}/activity`);
    return response.data;
  },

  /**
   * Add comment to problem
   */
  addComment: async (problemId: number, content: string): Promise<ProblemActivity> => {
    const response = await apiClient.post(`/api/problems/${problemId}/comments`, { content });
    return response.data;
  },

  /**
   * Get related incidents for a problem
   */
  getRelatedIncidents: async (problemId: number): Promise<any[]> => {
    const response = await apiClient.get(`/api/problems/${problemId}/related-incidents`);
    return response.data;
  },

  /**
   * Link incident to problem
   */
  linkIncident: async (problemId: number, incidentId: number): Promise<void> => {
    await apiClient.post(`/api/problems/${problemId}/incidents/${incidentId}`);
  },

  /**
   * Unlink incident from problem
   */
  unlinkIncident: async (problemId: number, incidentId: number): Promise<void> => {
    await apiClient.delete(`/api/problems/${problemId}/incidents/${incidentId}`);
  },

  /**
   * Add root cause analysis
   */
  addRootCauseAnalysis: async (
    id: number,
    analysis: string,
    description: string,
    workaround?: string
  ): Promise<Problem> => {
    const response = await apiClient.post(`/api/problems/${id}/root-cause`, {
      rootCauseAnalysis: analysis,
      rootCauseDescription: description,
      workaround,
    });
    return response.data;
  },

  /**
   * Resolve problem
   */
  resolve: async (id: number, resolutionNotes?: string): Promise<Problem> => {
    const response = await apiClient.post(`/api/problems/${id}/resolve`, { resolutionNotes });
    return response.data;
  },

  /**
   * Close problem
   */
  close: async (id: number): Promise<Problem> => {
    const response = await apiClient.post(`/api/problems/${id}/close`, {});
    return response.data;
  },

  /**
   * Get problem statistics
   */
  getStatistics: async (): Promise<ProblemStatistics> => {
    const response = await apiClient.get('/api/problems/statistics');
    return response.data;
  },

  /**
   * Delete problem
   */
  deleteProblem: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/problems/${id}`);
  },
};

export default problemService;

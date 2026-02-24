/**
 * Incident Service - Manages ITSM incidents
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum IncidentStatus {
  New = 0,
  InProgress = 1,
  OnHold = 2,
  Resolved = 3,
  Closed = 4,
  Cancelled = 5,
  Reopened = 6,
}

export enum IncidentPriority {
  Critical = 0,
  High = 1,
  Medium = 2,
  Low = 3,
  Planning = 4,
}

export enum IncidentCategory {
  Hardware = 0,
  Software = 1,
  Network = 2,
  Telecom = 3,
  Other = 4,
}

// ============================================================================
// Interfaces
// ============================================================================

export interface Incident {
  id: number;
  number: string;
  title: string;
  description: string;
  status: IncidentStatus;
  priority: IncidentPriority;
  category: IncidentCategory;
  callerId: number;
  callerName?: string;
  callerEmail?: string;
  assignedToId?: number;
  assignedToName?: string;
  teamId?: number;
  teamName?: string;
  slaInstanceId?: number;
  resolutionNotes?: string;
  rootCauseId?: number;
  rootCauseName?: string;
  relatedProblems?: number[];
  relatedChanges?: number[];
  attachments?: Attachment[];
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string;
  closedAt?: string;
  customFields?: Record<string, any>;
}

export interface IncidentActivity {
  id: number;
  incidentId: number;
  type: 'comment' | 'status_change' | 'assignment' | 'attachment' | 'assignment_group';
  userId: number;
  userName?: string;
  userEmail?: string;
  content: string;
  timestamp: string;
  metadata?: Record<string, any>;
}

export interface IncidentSLA {
  id: number;
  incidentId: number;
  slaName: string;
  responseTime: number; // minutes
  resolutionTime: number; // minutes
  responseDeadline: string;
  resolutionDeadline: string;
  responseBreached: boolean;
  resolutionBreached: boolean;
  responsePercentComplete: number;
  resolutionPercentComplete: number;
}

export interface Attachment {
  id: number;
  fileUrl: string;
  fileName: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: string;
}

export interface CreateIncidentRequest {
  title: string;
  description: string;
  priority: IncidentPriority;
  category: IncidentCategory;
  callerId: number;
  assignedToId?: number;
}

export interface UpdateIncidentRequest {
  title?: string;
  description?: string;
  status?: IncidentStatus;
  priority?: IncidentPriority;
  category?: IncidentCategory;
  assignedToId?: number;
  resolutionNotes?: string;
}

export interface PagedIncidentResult {
  items: Incident[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================================
// Service
// ============================================================================

const incidentService = {
  /**
   * Get all incidents with pagination and filtering
   */
  getIncidents: async (
    page: number = 1,
    pageSize: number = 20,
    filters?: {
      status?: IncidentStatus;
      priority?: IncidentPriority;
      category?: IncidentCategory;
      assignedToId?: number;
      search?: string;
    }
  ): Promise<PagedIncidentResult> => {
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

    const response = await apiClient.get(`/incidents?${params}`);
    return response.data;
  },

  /**
   * Get incident by ID
   */
  getIncident: async (id: number): Promise<Incident> => {
    const response = await apiClient.get(`/incidents/${id}`);
    return response.data;
  },

  /**
   * Create new incident
   */
  createIncident: async (data: CreateIncidentRequest): Promise<Incident> => {
    const response = await apiClient.post('/incidents', data);
    return response.data;
  },

  /**
   * Update incident
   */
  updateIncident: async (id: number, data: UpdateIncidentRequest): Promise<Incident> => {
    const response = await apiClient.put(`/incidents/${id}`, data);
    return response.data;
  },

  /**
   * Change incident status
   */
  changeStatus: async (id: number, status: IncidentStatus): Promise<Incident> => {
    const response = await apiClient.patch(`/incidents/${id}/status`, { status });
    return response.data;
  },

  /**
   * Assign incident to user
   */
  assignToUser: async (id: number, userId: number): Promise<Incident> => {
    const response = await apiClient.patch(`/incidents/${id}/assign`, { assignedToId: userId });
    return response.data;
  },

  /**
   * Assign incident to group
   */
  assignToGroup: async (id: number, groupId: number): Promise<Incident> => {
    const response = await apiClient.patch(`/incidents/${id}/assign-group`, { teamId: groupId });
    return response.data;
  },

  /**
   * Get incident activity timeline
   */
  getActivity: async (incidentId: number): Promise<IncidentActivity[]> => {
    const response = await apiClient.get(`/incidents/${incidentId}/activity`);
    return response.data;
  },

  /**
   * Add comment to incident
   */
  addComment: async (incidentId: number, content: string): Promise<IncidentActivity> => {
    const response = await apiClient.post(`/incidents/${incidentId}/comments`, { content });
    return response.data;
  },

  /**
   * Get SLA information for incident
   */
  getSLA: async (incidentId: number): Promise<IncidentSLA[]> => {
    const response = await apiClient.get(`/incidents/${incidentId}/sla`);
    return response.data;
  },

  /**
   * Get related incidents
   */
  getRelatedIncidents: async (incidentId: number): Promise<Incident[]> => {
    const response = await apiClient.get(`/incidents/${incidentId}/related`);
    return response.data;
  },

  /**
   * Bulk update incidents status
   */
  bulkUpdateStatus: async (incidentIds: number[], status: IncidentStatus): Promise<void> => {
    await apiClient.patch('/incidents/bulk/status', { incidentIds, status });
  },

  /**
   * Bulk assign incidents
   */
  bulkAssign: async (incidentIds: number[], userId: number): Promise<void> => {
    await apiClient.patch('/incidents/bulk/assign', { incidentIds, assignedToId: userId });
  },

  /**
   * Delete incident
   */
  deleteIncident: async (id: number): Promise<void> => {
    await apiClient.delete(`/incidents/${id}`);
  },

  /**
   * Escalate incident
   */
  escalate: async (id: number, reason?: string): Promise<Incident> => {
    const response = await apiClient.post(`/incidents/${id}/escalate`, { reason });
    return response.data;
  },

  /**
   * Resolve incident
   */
  resolve: async (id: number, resolutionNotes: string): Promise<Incident> => {
    const response = await apiClient.post(`/incidents/${id}/resolve`, { resolutionNotes });
    return response.data;
  },

  /**
   * Close incident
   */
  close: async (id: number): Promise<Incident> => {
    const response = await apiClient.post(`/incidents/${id}/close`, {});
    return response.data;
  },

  /**
   * Reopen incident
   */
  reopen: async (id: number, reason: string): Promise<Incident> => {
    const response = await apiClient.post(`/incidents/${id}/reopen`, { reason });
    return response.data;
  },
};

export default incidentService;

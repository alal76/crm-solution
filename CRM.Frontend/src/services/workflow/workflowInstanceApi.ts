/**
 * CRM Solution - Workflow Instance API
 * 
 * This file contains API functions for workflow instances, tasks, and execution management.
 * Extracted from workflowService.ts for better modularity.
 */

import apiClient from '../apiClient';
import type {
  WorkflowInstance,
  WorkflowInstanceDetail,
  WorkflowLog,
  HumanTask,
  InstanceStatistics,
  StartWorkflowDto,
  PaginatedResult,
  AuditLogEntry,
  ExecutionTimeline,
  SimulationResult
} from './types';

// ============================================================================
// Workflow Instance API
// ============================================================================

export const workflowInstanceService = {
  // ============================================================================
  // Instance Management
  // ============================================================================

  /**
   * Get instances with filtering
   */
  async getInstances(params?: {
    workflowDefinitionId?: number;
    entityType?: string;
    entityId?: number;
    status?: string;
    fromDate?: string;
    toDate?: string;
    skip?: number;
    take?: number;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PaginatedResult<WorkflowInstance>> {
    const response = await apiClient.get('/workflow-instances', { params });
    // Handle both array and paginated response formats
    if (Array.isArray(response.data)) {
      return {
        items: response.data,
        totalCount: response.data.length,
        pageNumber: 1,
        pageSize: response.data.length,
        totalPages: 1
      };
    }
    return response.data;
  },

  /**
   * Get a specific instance with details
   */
  async getInstance(id: number): Promise<WorkflowInstanceDetail> {
    const response = await apiClient.get(`/workflow-instances/${id}`);
    return response.data;
  },

  /**
   * Get instances for an entity
   */
  async getInstancesForEntity(entityType: string, entityId: number): Promise<WorkflowInstance[]> {
    const response = await apiClient.get(`/workflow-instances/entity/${entityType}/${entityId}`);
    return response.data;
  },

  /**
   * Start a new workflow instance
   */
  async startWorkflow(dto: StartWorkflowDto): Promise<{ id: number; correlationId: string }> {
    const response = await apiClient.post('/workflow-instances', dto);
    return response.data;
  },

  /**
   * Cancel an instance
   */
  async cancelInstance(id: number, reason: string): Promise<void> {
    await apiClient.post(`/workflow-instances/${id}/cancel`, { reason });
  },

  /**
   * Pause an instance
   */
  async pauseInstance(id: number): Promise<void> {
    await apiClient.post(`/workflow-instances/${id}/pause`);
  },

  /**
   * Resume an instance
   */
  async resumeInstance(id: number): Promise<void> {
    await apiClient.post(`/workflow-instances/${id}/resume`);
  },

  /**
   * Retry a failed instance
   */
  async retryInstance(id: number): Promise<void> {
    await apiClient.post(`/workflow-instances/${id}/retry`);
  },

  /**
   * Skip a node
   */
  async skipNode(instanceId: number, nodeId: number, reason: string): Promise<void> {
    await apiClient.post(`/workflow-instances/${instanceId}/skip-node/${nodeId}`, { reason });
  },

  // ============================================================================
  // Task Management
  // ============================================================================

  /**
   * Get my human tasks
   */
  async getMyTasks(): Promise<HumanTask[]> {
    const response = await apiClient.get('/workflow-instances/my-tasks');
    return response.data;
  },

  /**
   * Claim a task
   */
  async claimTask(taskId: number): Promise<void> {
    await apiClient.post(`/workflow-instances/tasks/${taskId}/claim`);
  },

  /**
   * Complete a task
   */
  async completeTask(taskId: number, formData?: string, outputData?: string): Promise<void> {
    await apiClient.post(`/workflow-instances/tasks/${taskId}/complete`, { formData, outputData });
  },

  // ============================================================================
  // Logging & Statistics
  // ============================================================================

  /**
   * Get logs for an instance
   */
  async getLogs(instanceId: number, params?: {
    minLevel?: string;
    category?: string;
    skip?: number;
    take?: number;
  }): Promise<WorkflowLog[]> {
    const response = await apiClient.get(`/workflow-instances/${instanceId}/logs`, { params });
    return response.data;
  },

  /**
   * Alias for getLogs
   */
  async getInstanceLogs(instanceId: number): Promise<WorkflowLog[]> {
    return this.getLogs(instanceId);
  },

  /**
   * Get instance statistics
   */
  async getStatistics(params?: {
    workflowDefinitionId?: number;
    fromDate?: string;
    toDate?: string;
  }): Promise<InstanceStatistics> {
    const response = await apiClient.get('/workflow-instances/statistics', { params });
    return response.data;
  },

  // ============================================================================
  // Audit Log & Timeline
  // ============================================================================

  /**
   * Get audit log for a workflow definition
   */
  async getAuditLog(definitionId: number, params?: {
    eventType?: string;
    eventCategory?: string;
    fromDate?: string;
    toDate?: string;
    skip?: number;
    take?: number;
  }): Promise<{ items: AuditLogEntry[]; hasMore: boolean }> {
    const response = await apiClient.get(`/workflow-instances/definitions/${definitionId}/audit-log`, { params });
    return response.data;
  },

  /**
   * Export audit log as CSV
   */
  async exportAuditLog(definitionId: number, params?: {
    fromDate?: string;
    toDate?: string;
  }): Promise<Blob> {
    const response = await apiClient.get(
      `/workflow-instances/definitions/${definitionId}/audit-log/export`, 
      { params, responseType: 'blob' }
    );
    return response.data;
  },

  /**
   * Get execution timeline for an instance
   */
  async getExecutionTimeline(instanceId: number): Promise<ExecutionTimeline> {
    const response = await apiClient.get(`/workflow-instances/${instanceId}/timeline`);
    return response.data;
  },

  /**
   * Simulate workflow execution (dry run)
   */
  async simulateWorkflow(workflowId: number, sampleData: object): Promise<SimulationResult> {
    const response = await apiClient.post(`/workflows/${workflowId}/simulate`, { sampleData });
    return response.data;
  }
};

export default workflowInstanceService;

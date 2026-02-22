/**
 * CRM Solution - Workflow Definition API
 * 
 * This file contains API functions for workflow definitions, versions, nodes, and transitions.
 * Extracted from workflowService.ts for better modularity.
 */

import apiClient from '../apiClient';
import type {
  WorkflowDefinition,
  WorkflowDefinitionDetail,
  WorkflowVersionDetail,
  WorkflowStatistics,
  WorkflowConfig,
  CreateWorkflowDto,
  UpdateWorkflowDto,
  CreateNodeDto,
  UpdateNodeDto,
  NodePositionDto,
  CreateTransitionDto,
  UpdateTransitionDto,
  EntityTypeOption,
  NodeTypeOption,
  ActionTypeOption,
  TriggerTypeOption,
  OperatorOption,
  StatusOption,
  LLMProviderOption,
  LLMModelOption,
  EventTypeOption
} from './types';

// ============================================================================
// Node Type Metadata - Default Fallback Values
// ============================================================================

const defaultNodeTypeInfo: Record<string, { icon: string; color: string; label: string }> = {
  Trigger: { icon: 'PlayCircle', color: '#4CAF50', label: 'Trigger' },
  Condition: { icon: 'CallSplit', color: '#FF9800', label: 'Condition' },
  Action: { icon: 'FlashOn', color: '#2196F3', label: 'Action' },
  HumanTask: { icon: 'Person', color: '#9C27B0', label: 'Human Task' },
  Wait: { icon: 'Schedule', color: '#607D8B', label: 'Wait/Timer' },
  ParallelGateway: { icon: 'CallSplit', color: '#FF5722', label: 'Parallel Split' },
  JoinGateway: { icon: 'CallMerge', color: '#FF5722', label: 'Parallel Join' },
  Subprocess: { icon: 'AccountTree', color: '#795548', label: 'Subprocess' },
  LLMAction: { icon: 'Psychology', color: '#E91E63', label: 'AI/LLM Action' },
  End: { icon: 'StopCircle', color: '#F44336', label: 'End' },
  // AI-Enhanced Node Types
  AIDecision: { icon: 'Route', color: '#00BCD4', label: 'AI Decision' },
  AIAgent: { icon: 'SmartToy', color: '#673AB7', label: 'AI Agent' },
  AIContentGenerator: { icon: 'AutoAwesome', color: '#3F51B5', label: 'AI Content Generator' },
  AIDataExtractor: { icon: 'DataObject', color: '#009688', label: 'AI Data Extractor' },
  AIClassifier: { icon: 'Category', color: '#8BC34A', label: 'AI Classifier' },
  AISentimentAnalyzer: { icon: 'SentimentSatisfied', color: '#FFEB3B', label: 'AI Sentiment Analyzer' },
  HumanReview: { icon: 'RateReview', color: '#FF5722', label: 'Human Review' }
};

const defaultStatusColors: Record<string, string> = {
  Draft: '#9E9E9E',
  Active: '#4CAF50',
  Paused: '#FF9800',
  Archived: '#607D8B',
  Deprecated: '#F44336',
  Pending: '#2196F3',
  Running: '#4CAF50',
  Waiting: '#FF9800',
  Completed: '#4CAF50',
  Failed: '#F44336',
  Cancelled: '#9E9E9E',
  TimedOut: '#FF5722',
  Suspended: '#9C27B0'
};

// ============================================================================
// Configuration Cache
// ============================================================================

let cachedConfig: WorkflowConfig | null = null;
let configLoadPromise: Promise<WorkflowConfig> | null = null;

// Dynamic getters that use cached config or fall back to defaults
export const nodeTypeInfo: Record<string, { icon: string; color: string; label: string }> = new Proxy(defaultNodeTypeInfo, {
  get(target, prop) {
    if (cachedConfig) {
      const nodeType = cachedConfig.nodeTypes.find(n => n.value === String(prop));
      if (nodeType) {
        return { icon: nodeType.icon, color: nodeType.color, label: nodeType.label };
      }
    }
    return target[String(prop)] || { icon: 'Circle', color: '#6750A4', label: String(prop) };
  }
});

export const statusColors: Record<string, string> = new Proxy(defaultStatusColors, {
  get(target, prop) {
    if (cachedConfig) {
      const status = cachedConfig.statusOptions.find(s => s.value === String(prop));
      if (status) {
        return status.color;
      }
    }
    return target[String(prop)] || '#9E9E9E';
  }
});

// ============================================================================
// Workflow Definition API
// ============================================================================

export const workflowService = {
  // ============================================================================
  // Workflow Definitions
  // ============================================================================

  /**
   * Get all workflows with filtering
   */
  async getWorkflows(params?: {
    entityType?: string;
    status?: string;
    category?: string;
    search?: string;
    skip?: number;
    take?: number;
  }): Promise<WorkflowDefinition[]> {
    const response = await apiClient.get('/workflows', { params });
    return response.data;
  },

  /**
   * Get a single workflow with versions
   */
  async getWorkflow(id: number): Promise<WorkflowDefinitionDetail> {
    const response = await apiClient.get(`/workflows/${id}`);
    return response.data;
  },

  /**
   * Create a new workflow
   */
  async createWorkflow(workflow: CreateWorkflowDto): Promise<{ id: number }> {
    const response = await apiClient.post('/workflows', workflow);
    return response.data;
  },

  /**
   * Update a workflow
   */
  async updateWorkflow(id: number, workflow: UpdateWorkflowDto): Promise<void> {
    await apiClient.put(`/workflows/${id}`, workflow);
  },

  /**
   * Delete a workflow
   */
  async deleteWorkflow(id: number): Promise<void> {
    await apiClient.delete(`/workflows/${id}`);
  },

  /**
   * Activate a workflow version
   */
  async activateWorkflow(workflowId: number, versionId: number): Promise<void> {
    await apiClient.post(`/workflows/${workflowId}/activate/${versionId}`);
  },

  /**
   * Pause a workflow
   */
  async pauseWorkflow(id: number): Promise<void> {
    await apiClient.post(`/workflows/${id}/pause`);
  },

  /**
   * Clone an entire workflow definition including its active/latest version, nodes, and transitions.
   * Returns the cloned workflow's ID, key, name, and status.
   */
  async cloneWorkflow(id: number, newName?: string): Promise<{ id: number; workflowKey: string; name: string; status: string }> {
    const response = await apiClient.post(`/workflows/${id}/clone`, newName ? { newName } : {});
    return response.data;
  },

  /**
   * Get workflow statistics
   */
  async getStatistics(): Promise<WorkflowStatistics> {
    const response = await apiClient.get('/workflows/statistics');
    return response.data;
  },

  // ============================================================================
  // Configuration
  // ============================================================================

  /**
   * Get comprehensive workflow configuration (cached)
   */
  async getConfig(forceRefresh = false): Promise<WorkflowConfig> {
    if (cachedConfig && !forceRefresh) {
      return cachedConfig;
    }
    
    if (configLoadPromise && !forceRefresh) {
      return configLoadPromise;
    }

    configLoadPromise = apiClient.get('/workflows/config')
      .then(response => {
        cachedConfig = response.data;
        configLoadPromise = null;
        return cachedConfig as WorkflowConfig;
      })
      .catch(error => {
        configLoadPromise = null;
        throw error;
      });

    return configLoadPromise;
  },

  /**
   * Clear the cached configuration
   */
  clearConfigCache(): void {
    cachedConfig = null;
    configLoadPromise = null;
  },

  /**
   * Get available entity types (uses cached config)
   */
  async getEntityTypes(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.entityTypes;
  },

  /**
   * Get available node types (uses cached config)
   */
  async getNodeTypes(): Promise<NodeTypeOption[]> {
    const config = await this.getConfig();
    return config.nodeTypes;
  },

  /**
   * Get workflow categories (uses cached config)
   */
  async getCategories(): Promise<string[]> {
    const config = await this.getConfig();
    return config.categories;
  },

  /**
   * Get action types
   */
  async getActionTypes(): Promise<ActionTypeOption[]> {
    const config = await this.getConfig();
    return config.actionTypes;
  },

  /**
   * Get trigger types
   */
  async getTriggerTypes(): Promise<TriggerTypeOption[]> {
    const config = await this.getConfig();
    return config.triggerTypes;
  },

  /**
   * Get LLM providers (only configured ones)
   */
  async getLLMProviders(): Promise<LLMProviderOption[]> {
    const config = await this.getConfig();
    return config.llmProviders.filter(p => p.isConfigured);
  },

  /**
   * Get all LLM models (from configured providers)
   */
  async getLLMModels(): Promise<LLMModelOption[]> {
    const config = await this.getConfig();
    return config.llmModels;
  },

  /**
   * Get roles
   */
  async getRoles(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.roles;
  },

  /**
   * Get status options
   */
  async getStatusOptions(): Promise<StatusOption[]> {
    const config = await this.getConfig();
    return config.statusOptions;
  },

  /**
   * Get condition operators
   */
  async getConditionOperators(): Promise<OperatorOption[]> {
    const config = await this.getConfig();
    return config.conditionOperators;
  },

  /**
   * Get fallback actions
   */
  async getFallbackActions(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.fallbackActions;
  },

  /**
   * Get icon options
   */
  async getIconOptions(): Promise<string[]> {
    const config = await this.getConfig();
    return config.iconOptions;
  },

  /**
   * Get color options
   */
  async getColorOptions(): Promise<string[]> {
    const config = await this.getConfig();
    return config.colorOptions;
  },

  /**
   * Get event types (for audit logs)
   */
  async getEventTypes(): Promise<EventTypeOption[]> {
    const config = await this.getConfig();
    return config.eventTypes;
  },

  // ============================================================================
  // Versions
  // ============================================================================

  /**
   * Get a specific version with full graph
   */
  async getVersion(versionId: number): Promise<WorkflowVersionDetail> {
    const response = await apiClient.get(`/workflows/versions/${versionId}`);
    return response.data;
  },

  /**
   * Create a new version
   */
  async createVersion(workflowId: number, sourceVersionId?: number): Promise<{ id: number; versionNumber: number }> {
    const response = await apiClient.post(`/workflows/${workflowId}/versions`, { sourceVersionId });
    return response.data;
  },

  /**
   * Save canvas layout
   */
  async saveCanvasLayout(versionId: number, canvasLayout: string): Promise<void> {
    await apiClient.put(`/workflows/versions/${versionId}/layout`, { canvasLayout });
  },

  // ============================================================================
  // Nodes
  // ============================================================================

  /**
   * Add a node
   */
  async addNode(versionId: number, node: CreateNodeDto): Promise<{ id: number; nodeKey: string }> {
    const response = await apiClient.post(`/workflows/versions/${versionId}/nodes`, node);
    return response.data;
  },

  /**
   * Update a node
   */
  async updateNode(nodeId: number, node: UpdateNodeDto): Promise<void> {
    await apiClient.put(`/workflows/nodes/${nodeId}`, node);
  },

  /**
   * Delete a node
   */
  async deleteNode(nodeId: number): Promise<void> {
    await apiClient.delete(`/workflows/nodes/${nodeId}`);
  },

  /**
   * Update node positions (bulk)
   */
  async updateNodePositions(versionId: number, positions: NodePositionDto[]): Promise<void> {
    await apiClient.put(`/workflows/versions/${versionId}/nodes/positions`, positions);
  },

  // ============================================================================
  // Transitions
  // ============================================================================

  /**
   * Add a transition
   */
  async addTransition(versionId: number, transition: CreateTransitionDto): Promise<{ id: number }> {
    const response = await apiClient.post(`/workflows/versions/${versionId}/transitions`, transition);
    return response.data;
  },

  /**
   * Update a transition
   */
  async updateTransition(transitionId: number, transition: UpdateTransitionDto): Promise<void> {
    await apiClient.put(`/workflows/transitions/${transitionId}`, transition);
  },

  /**
   * Delete a transition
   */
  async deleteTransition(transitionId: number): Promise<void> {
    await apiClient.delete(`/workflows/transitions/${transitionId}`);
  }
};

export default workflowService;

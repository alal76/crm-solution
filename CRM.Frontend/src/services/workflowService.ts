/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

import apiClient from './apiClient';

// ============================================================================
// Enums
// ============================================================================

export enum WorkflowStatus {
  Draft = 'Draft',
  Active = 'Active',
  Paused = 'Paused',
  Archived = 'Archived',
  Deprecated = 'Deprecated'
}

export enum WorkflowVersionStatus {
  Draft = 'Draft',
  Active = 'Active',
  Deprecated = 'Deprecated'
}

export enum WorkflowNodeType {
  Trigger = 'Trigger',
  Condition = 'Condition',
  Action = 'Action',
  HumanTask = 'HumanTask',
  Wait = 'Wait',
  ParallelGateway = 'ParallelGateway',
  JoinGateway = 'JoinGateway',
  Subprocess = 'Subprocess',
  LLMAction = 'LLMAction',
  End = 'End'
}

export enum TransitionConditionType {
  Always = 'Always',
  Expression = 'Expression',
  FieldMatch = 'FieldMatch',
  Any = 'Any',
  All = 'All',
  UserChoice = 'UserChoice'
}

export enum WorkflowInstanceStatus {
  Pending = 'Pending',
  Running = 'Running',
  Waiting = 'Waiting',
  Paused = 'Paused',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  TimedOut = 'TimedOut',
  Suspended = 'Suspended'
}

export enum WorkflowTaskType {
  Automated = 'Automated',
  Human = 'Human',
  Timer = 'Timer',
  Event = 'Event',
  LLM = 'LLM'
}

export enum WorkflowTaskStatus {
  Pending = 'Pending',
  Locked = 'Locked',
  Running = 'Running',
  Waiting = 'Waiting',
  Completed = 'Completed',
  Failed = 'Failed',
  Retrying = 'Retrying',
  Cancelled = 'Cancelled',
  Skipped = 'Skipped',
  DeadLetter = 'DeadLetter'
}

export enum WorkflowLogLevel {
  Debug = 'Debug',
  Info = 'Info',
  Warning = 'Warning',
  Error = 'Error',
  Critical = 'Critical'
}

// ============================================================================
// Types - Workflow Definitions
// ============================================================================

export interface WorkflowDefinition {
  id: number;
  workflowKey: string;
  name: string;
  description?: string;
  category?: string;
  entityType: string;
  status: string;
  currentVersion: number;
  iconName?: string;
  color?: string;
  isSystem: boolean;
  priority: number;
  maxConcurrentInstances: number;
  defaultTimeoutHours: number;
  ownerId?: number;
  ownerName?: string;
  tags?: string[];
  createdAt: string;
  updatedAt?: string;
}

export interface WorkflowDefinitionDetail extends WorkflowDefinition {
  metadata?: string;
  versions: WorkflowVersionSummary[];
}

export interface WorkflowVersionSummary {
  id: number;
  versionNumber: number;
  label?: string;
  status: string;
  publishedAt?: string;
  createdAt: string;
}

export interface WorkflowVersionDetail extends WorkflowVersionSummary {
  workflowDefinitionId: number;
  workflowName: string;
  changeLog?: string;
  publishedByName?: string;
  canvasLayout?: string;
  updatedAt?: string;
  nodes: WorkflowNode[];
  transitions: WorkflowTransition[];
}

export interface WorkflowNode {
  id: number;
  nodeKey: string;
  name: string;
  description?: string;
  nodeType: string;
  nodeSubType?: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
  iconName?: string;
  color?: string;
  isStartNode: boolean;
  isEndNode: boolean;
  configuration?: string;
  timeoutMinutes: number;
  retryCount: number;
  executionOrder: number;
}

export interface WorkflowTransition {
  id: number;
  sourceNodeId: number;
  targetNodeId: number;
  transitionKey?: string;
  label?: string;
  conditionType: string;
  conditionExpression?: string;
  isDefault: boolean;
  priority: number;
  sourceHandle: string;
  targetHandle: string;
  lineStyle: string;
  color: string;
  animationStyle: string;
}

// ============================================================================
// Types - Workflow Instances
// ============================================================================

export interface WorkflowInstance {
  id: number;
  correlationId: string;
  workflowDefinitionId: number;
  workflowName: string;
  workflowVersionId: number;
  versionNumber: number;
  entityType: string;
  entityId: number;
  status: string;
  currentNodeId?: number;
  currentNodeName?: string;
  triggerEvent?: string;
  triggeredByName?: string;
  startedAt?: string;
  completedAt?: string;
  scheduledAt?: string;
  priority: number;
  retryCount: number;
  errorMessage?: string;
  isCancelled: boolean;
  createdAt: string;
}

export interface WorkflowInstanceDetail extends WorkflowInstance {
  triggeredById?: number;
  inputData?: string;
  stateData?: string;
  outputData?: string;
  timeoutAt?: string;
  maxRetries: number;
  nextRetryAt?: string;
  errorStackTrace?: string;
  cancellationReason?: string;
  parentInstanceId?: number;
  updatedAt?: string;
  nodes: WorkflowNode[];
  transitions: WorkflowTransition[];
  nodeInstances: WorkflowNodeInstance[];
  tasks: WorkflowTask[];
  recentLogs: WorkflowLog[];
}

export interface WorkflowNodeInstance {
  id: number;
  nodeId: number;
  nodeName: string;
  status: string;
  startedAt?: string;
  completedAt?: string;
  durationMs?: number;
  retryCount: number;
  errorMessage?: string;
  isSkipped: boolean;
  skipReason?: string;
  executionSequence: number;
  workerId?: string;
}

export interface WorkflowTask {
  id: number;
  nodeId: number;
  nodeName: string;
  taskType: string;
  name: string;
  status: string;
  priority: number;
  dueAt?: string;
  assignedToId?: number;
  assignedToRole?: string;
  retryCount: number;
  isDeadLetter: boolean;
  createdAt: string;
}

export interface WorkflowLog {
  id: number;
  level: string;
  category: string;
  message: string;
  details?: string;
  nodeName?: string;
  userName?: string;
  workerId?: string;
  timestamp: string;
  durationMs?: number;
}

export interface HumanTask {
  id: number;
  workflowInstanceId: number;
  workflowName: string;
  nodeId: number;
  nodeName: string;
  name: string;
  description?: string;
  priority: number;
  dueAt?: string;
  formSchema?: string;
  entityType: string;
  entityId: number;
  createdAt: string;
}

// ============================================================================
// Types - Statistics
// ============================================================================

export interface WorkflowStatistics {
  totalWorkflows: number;
  activeWorkflows: number;
  draftWorkflows: number;
  totalInstances: number;
  runningInstances: number;
  completedInstances: number;
  failedInstances: number;
  pendingTasks: number;
  deadLetterTasks: number;
  workflowsByCategory: Record<string, number>;
  workflowsByEntityType: Record<string, number>;
}

export interface InstanceStatistics {
  total: number;
  pending: number;
  running: number;
  waiting: number;
  completed: number;
  failed: number;
  cancelled: number;
  timedOut: number;
  averageCompletionTimeMinutes: number;
  byWorkflow: {
    workflowId: number;
    workflowName: string;
    total: number;
    completed: number;
    failed: number;
  }[];
}

// ============================================================================
// Types - Create/Update DTOs
// ============================================================================

export interface CreateWorkflowDto {
  workflowKey: string;
  name: string;
  description?: string;
  category?: string;
  entityType: string;
  iconName?: string;
  color?: string;
  priority?: number;
  maxConcurrentInstances?: number;
  defaultTimeoutHours?: number;
  tags?: string[];
  metadata?: string;
}

export interface UpdateWorkflowDto {
  name?: string;
  description?: string;
  category?: string;
  entityType?: string;
  iconName?: string;
  color?: string;
  priority?: number;
  maxConcurrentInstances?: number;
  defaultTimeoutHours?: number;
  tags?: string[];
  metadata?: string;
}

export interface CreateNodeDto {
  nodeKey?: string;
  name: string;
  description?: string;
  nodeType: string;
  nodeSubType?: string;
  positionX: number;
  positionY: number;
  width?: number;
  height?: number;
  iconName?: string;
  color?: string;
  isStartNode?: boolean;
  isEndNode?: boolean;
  configuration?: string;
  timeoutMinutes?: number;
  retryCount?: number;
  retryDelaySeconds?: number;
  useExponentialBackoff?: boolean;
  executionOrder?: number;
}

export interface UpdateNodeDto {
  name?: string;
  description?: string;
  nodeType?: string;
  nodeSubType?: string;
  positionX?: number;
  positionY?: number;
  width?: number;
  height?: number;
  iconName?: string;
  color?: string;
  isStartNode?: boolean;
  isEndNode?: boolean;
  configuration?: string;
  timeoutMinutes?: number;
  retryCount?: number;
  retryDelaySeconds?: number;
  useExponentialBackoff?: boolean;
  executionOrder?: number;
}

export interface NodePositionDto {
  nodeId: number;
  x: number;
  y: number;
}

export interface CreateTransitionDto {
  sourceNodeId: number;
  targetNodeId: number;
  transitionKey?: string;
  label?: string;
  description?: string;
  conditionType?: string;
  conditionExpression?: string;
  isDefault?: boolean;
  priority?: number;
  sourceHandle?: string;
  targetHandle?: string;
  lineStyle?: string;
  color?: string;
  animationStyle?: string;
}

export interface UpdateTransitionDto {
  label?: string;
  description?: string;
  conditionType?: string;
  conditionExpression?: string;
  isDefault?: boolean;
  priority?: number;
  sourceHandle?: string;
  targetHandle?: string;
  lineStyle?: string;
  color?: string;
  animationStyle?: string;
}

export interface StartWorkflowDto {
  workflowDefinitionId: number;
  entityType: string;
  entityId: number;
  triggerEvent?: string;
  inputData?: object;
  scheduledAt?: string;
}

export interface EntityTypeOption {
  value: string;
  label: string;
}

export interface NodeTypeOption {
  value: string;
  label: string;
  icon: string;
  color: string;
}

// ============================================================================
// Node type metadata
// ============================================================================

export const nodeTypeInfo: Record<string, { icon: string; color: string; label: string }> = {
  Trigger: { icon: 'PlayCircle', color: '#4CAF50', label: 'Trigger' },
  Condition: { icon: 'CallSplit', color: '#FF9800', label: 'Condition' },
  Action: { icon: 'FlashOn', color: '#2196F3', label: 'Action' },
  HumanTask: { icon: 'Person', color: '#9C27B0', label: 'Human Task' },
  Wait: { icon: 'Schedule', color: '#607D8B', label: 'Wait/Timer' },
  ParallelGateway: { icon: 'CallSplit', color: '#FF5722', label: 'Parallel Split' },
  JoinGateway: { icon: 'CallMerge', color: '#FF5722', label: 'Parallel Join' },
  Subprocess: { icon: 'AccountTree', color: '#795548', label: 'Subprocess' },
  LLMAction: { icon: 'Psychology', color: '#E91E63', label: 'AI/LLM Action' },
  End: { icon: 'StopCircle', color: '#F44336', label: 'End' }
};

export const statusColors: Record<string, string> = {
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
// API Functions - Workflow Definitions
// ============================================================================

export const workflowService = {
  // Get all workflows with filtering
  async getWorkflows(params?: {
    entityType?: string;
    status?: string;
    category?: string;
    search?: string;
    skip?: number;
    take?: number;
  }): Promise<WorkflowDefinition[]> {
    const response = await apiClient.get('/api/workflows', { params });
    return response.data;
  },

  // Get a single workflow with versions
  async getWorkflow(id: number): Promise<WorkflowDefinitionDetail> {
    const response = await apiClient.get(`/api/workflows/${id}`);
    return response.data;
  },

  // Create a new workflow
  async createWorkflow(workflow: CreateWorkflowDto): Promise<{ id: number }> {
    const response = await apiClient.post('/api/workflows', workflow);
    return response.data;
  },

  // Update a workflow
  async updateWorkflow(id: number, workflow: UpdateWorkflowDto): Promise<void> {
    await apiClient.put(`/api/workflows/${id}`, workflow);
  },

  // Delete a workflow
  async deleteWorkflow(id: number): Promise<void> {
    await apiClient.delete(`/api/workflows/${id}`);
  },

  // Activate a workflow version
  async activateWorkflow(workflowId: number, versionId: number): Promise<void> {
    await apiClient.post(`/api/workflows/${workflowId}/activate/${versionId}`);
  },

  // Pause a workflow
  async pauseWorkflow(id: number): Promise<void> {
    await apiClient.post(`/api/workflows/${id}/pause`);
  },

  // Get workflow statistics
  async getStatistics(): Promise<WorkflowStatistics> {
    const response = await apiClient.get('/api/workflows/statistics');
    return response.data;
  },

  // Get available entity types
  async getEntityTypes(): Promise<EntityTypeOption[]> {
    const response = await apiClient.get('/api/workflows/entity-types');
    return response.data;
  },

  // Get available node types
  async getNodeTypes(): Promise<NodeTypeOption[]> {
    const response = await apiClient.get('/api/workflows/node-types');
    return response.data;
  },

  // Get workflow categories
  async getCategories(): Promise<string[]> {
    const response = await apiClient.get('/api/workflows/categories');
    return response.data;
  },

  // ============================================================================
  // Versions
  // ============================================================================

  // Get a specific version with full graph
  async getVersion(versionId: number): Promise<WorkflowVersionDetail> {
    const response = await apiClient.get(`/api/workflows/versions/${versionId}`);
    return response.data;
  },

  // Create a new version
  async createVersion(workflowId: number, sourceVersionId?: number): Promise<{ id: number; versionNumber: number }> {
    const response = await apiClient.post(`/api/workflows/${workflowId}/versions`, { sourceVersionId });
    return response.data;
  },

  // Save canvas layout
  async saveCanvasLayout(versionId: number, canvasLayout: string): Promise<void> {
    await apiClient.put(`/api/workflows/versions/${versionId}/layout`, { canvasLayout });
  },

  // ============================================================================
  // Nodes
  // ============================================================================

  // Add a node
  async addNode(versionId: number, node: CreateNodeDto): Promise<{ id: number; nodeKey: string }> {
    const response = await apiClient.post(`/api/workflows/versions/${versionId}/nodes`, node);
    return response.data;
  },

  // Update a node
  async updateNode(nodeId: number, node: UpdateNodeDto): Promise<void> {
    await apiClient.put(`/api/workflows/nodes/${nodeId}`, node);
  },

  // Delete a node
  async deleteNode(nodeId: number): Promise<void> {
    await apiClient.delete(`/api/workflows/nodes/${nodeId}`);
  },

  // Update node positions (bulk)
  async updateNodePositions(versionId: number, positions: NodePositionDto[]): Promise<void> {
    await apiClient.put(`/api/workflows/versions/${versionId}/nodes/positions`, positions);
  },

  // ============================================================================
  // Transitions
  // ============================================================================

  // Add a transition
  async addTransition(versionId: number, transition: CreateTransitionDto): Promise<{ id: number }> {
    const response = await apiClient.post(`/api/workflows/versions/${versionId}/transitions`, transition);
    return response.data;
  },

  // Update a transition
  async updateTransition(transitionId: number, transition: UpdateTransitionDto): Promise<void> {
    await apiClient.put(`/api/workflows/transitions/${transitionId}`, transition);
  },

  // Delete a transition
  async deleteTransition(transitionId: number): Promise<void> {
    await apiClient.delete(`/api/workflows/transitions/${transitionId}`);
  }
};

// ============================================================================
// API Functions - Workflow Instances
// ============================================================================

// Paginated result interface
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export const workflowInstanceService = {
  // Get instances with filtering
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
    const response = await apiClient.get('/api/workflow-instances', { params });
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

  // Get a specific instance with details
  async getInstance(id: number): Promise<WorkflowInstanceDetail> {
    const response = await apiClient.get(`/api/workflow-instances/${id}`);
    return response.data;
  },

  // Get instances for an entity
  async getInstancesForEntity(entityType: string, entityId: number): Promise<WorkflowInstance[]> {
    const response = await apiClient.get(`/api/workflow-instances/entity/${entityType}/${entityId}`);
    return response.data;
  },

  // Start a new workflow instance
  async startWorkflow(dto: StartWorkflowDto): Promise<{ id: number; correlationId: string }> {
    const response = await apiClient.post('/api/workflow-instances', dto);
    return response.data;
  },

  // Cancel an instance
  async cancelInstance(id: number, reason: string): Promise<void> {
    await apiClient.post(`/api/workflow-instances/${id}/cancel`, { reason });
  },

  // Pause an instance
  async pauseInstance(id: number): Promise<void> {
    await apiClient.post(`/api/workflow-instances/${id}/pause`);
  },

  // Resume an instance
  async resumeInstance(id: number): Promise<void> {
    await apiClient.post(`/api/workflow-instances/${id}/resume`);
  },

  // Retry a failed instance
  async retryInstance(id: number): Promise<void> {
    await apiClient.post(`/api/workflow-instances/${id}/retry`);
  },

  // Skip a node
  async skipNode(instanceId: number, nodeId: number, reason: string): Promise<void> {
    await apiClient.post(`/api/workflow-instances/${instanceId}/skip-node/${nodeId}`, { reason });
  },

  // Get my human tasks
  async getMyTasks(): Promise<HumanTask[]> {
    const response = await apiClient.get('/api/workflow-instances/my-tasks');
    return response.data;
  },

  // Claim a task
  async claimTask(taskId: number): Promise<void> {
    await apiClient.post(`/api/workflow-instances/tasks/${taskId}/claim`);
  },

  // Complete a task
  async completeTask(taskId: number, formData?: string, outputData?: string): Promise<void> {
    await apiClient.post(`/api/workflow-instances/tasks/${taskId}/complete`, { formData, outputData });
  },

  // Get logs for an instance
  async getLogs(instanceId: number, params?: {
    minLevel?: string;
    category?: string;
    skip?: number;
    take?: number;
  }): Promise<WorkflowLog[]> {
    const response = await apiClient.get(`/api/workflow-instances/${instanceId}/logs`, { params });
    return response.data;
  },

  // Alias for getLogs
  async getInstanceLogs(instanceId: number): Promise<WorkflowLog[]> {
    return this.getLogs(instanceId);
  },

  // Get instance statistics
  async getStatistics(params?: {
    workflowDefinitionId?: number;
    fromDate?: string;
    toDate?: string;
  }): Promise<InstanceStatistics> {
    const response = await apiClient.get('/api/workflow-instances/statistics', { params });
    return response.data;
  }
};

export default workflowService;

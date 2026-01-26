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
  End = 'End',
  // AI-Enhanced Node Types
  AIDecision = 'AIDecision',
  AIAgent = 'AIAgent',
  AIContentGenerator = 'AIContentGenerator',
  AIDataExtractor = 'AIDataExtractor',
  AIClassifier = 'AIClassifier',
  AISentimentAnalyzer = 'AISentimentAnalyzer',
  HumanReview = 'HumanReview'
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
  description?: string;
}

export interface ActionTypeOption {
  value: string;
  label: string;
  category: string;
  icon: string;
}

export interface TriggerTypeOption {
  value: string;
  label: string;
  description: string;
  icon: string;
}

export interface OperatorOption {
  value: string;
  label: string;
  appliesTo: string[];
}

export interface StatusOption {
  value: string;
  label: string;
  color: string;
  bgColor: string;
  icon: string;
}

export interface LLMProviderOption {
  value: string;
  label: string;
  isConfigured: boolean;
  models: LLMModelOption[];
}

export interface LLMModelOption {
  value: string;
  label: string;
  provider: string;
  isDefault: boolean;
}

export interface EventTypeOption {
  value: string;
  label: string;
  color: string;
  category: string;
}

export interface WorkflowConfig {
  entityTypes: EntityTypeOption[];
  nodeTypes: NodeTypeOption[];
  actionTypes: ActionTypeOption[];
  triggerTypes: TriggerTypeOption[];
  conditionOperators: OperatorOption[];
  statusOptions: StatusOption[];
  llmProviders: LLMProviderOption[];
  llmModels: LLMModelOption[];
  roles: EntityTypeOption[];
  categories: string[];
  iconOptions: string[];
  colorOptions: string[];
  fallbackActions: EntityTypeOption[];
  eventTypes: EventTypeOption[];
}

// ============================================================================
// AI-Enhanced Workflow Node Configuration Types
// ============================================================================

/**
 * AI Tool definition for AI Agent nodes
 */
export interface AIToolDefinition {
  id: string;
  name: string;
  description: string;
  category: 'crm' | 'communication' | 'data' | 'external' | 'custom';
  icon: string;
  parameters: AIToolParameter[];
  requiresApproval?: boolean;
  costPerCall?: number;
}

export interface AIToolParameter {
  name: string;
  type: 'string' | 'number' | 'boolean' | 'object' | 'array';
  description: string;
  required: boolean;
  default?: any;
}

/**
 * Prompt template for reuse across AI nodes
 */
export interface PromptTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  template: string;
  variables: string[];
  exampleOutput?: string;
  tags: string[];
}

/**
 * AI Decision Node Configuration
 * Routes workflow based on AI analysis of content
 */
export interface AIDecisionConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Decision prompt
  systemPrompt: string;
  userPromptTemplate: string;
  
  // Decision options
  decisionOptions: AIDecisionOption[];
  defaultOption?: string;
  confidenceThreshold: number;
  
  // Input/Output
  inputVariables: string[];
  outputVariable: string;
  
  // Error handling
  fallbackOption?: string;
  retryOnError: boolean;
}

export interface AIDecisionOption {
  id: string;
  label: string;
  description: string;
  criteria: string;
  outputHandle?: string;
}

/**
 * AI Agent Node Configuration
 * Autonomous agent that can use tools
 */
export interface AIAgentConfig {
  // Agent identity
  agentName: string;
  agentDescription: string;
  systemPrompt: string;
  
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  maxIterations: number;
  
  // Tool access
  availableTools: string[];
  toolApprovalRequired: boolean;
  
  // Autonomy settings
  autonomyLevel: 'low' | 'medium' | 'high' | 'full';
  canModifyData: boolean;
  canSendCommunications: boolean;
  
  // Memory settings
  enableMemory: boolean;
  memoryType: 'conversation' | 'summary' | 'vector';
  maxMemoryItems: number;
  
  // Goals and constraints
  primaryGoal: string;
  constraints: string[];
  stopConditions: string[];
  
  // Budget controls
  maxCost?: number;
  maxApiCalls?: number;
  
  // Output
  outputVariable: string;
  outputSchema?: object;
}

/**
 * AI Content Generator Node Configuration
 * Generates emails, summaries, reports, etc.
 */
export interface AIContentGeneratorConfig {
  // Content type
  contentType: 'email' | 'summary' | 'report' | 'response' | 'document' | 'custom';
  
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Prompt configuration
  systemPrompt: string;
  userPromptTemplate: string;
  
  // Content settings
  tone: 'professional' | 'friendly' | 'formal' | 'casual' | 'empathetic';
  language: string;
  maxLength?: number;
  
  // Template settings
  useTemplate: boolean;
  templateId?: string;
  
  // Input variables
  inputVariables: string[];
  contextFields: string[];
  
  // Output
  outputVariable: string;
  outputFormat: 'text' | 'html' | 'markdown' | 'json';
  
  // Review settings
  requiresReview: boolean;
  reviewerRole?: string;
}

/**
 * AI Data Extractor Node Configuration
 * Extracts structured data from unstructured text
 */
export interface AIDataExtractorConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Extraction settings
  inputVariable: string;
  extractionSchema: AIExtractionField[];
  
  // Prompt customization
  systemPrompt?: string;
  additionalInstructions?: string;
  
  // Output
  outputVariable: string;
  outputFormat: 'object' | 'array';
  
  // Validation
  validateOutput: boolean;
  validationRules?: AIValidationRule[];
  
  // Error handling
  onExtractionFailure: 'error' | 'skip' | 'default';
  defaultValues?: Record<string, any>;
}

export interface AIExtractionField {
  name: string;
  type: 'string' | 'number' | 'boolean' | 'date' | 'email' | 'phone' | 'address' | 'array' | 'object';
  description: string;
  required: boolean;
  format?: string;
  examples?: string[];
  arrayItemType?: string;
  objectSchema?: AIExtractionField[];
}

export interface AIValidationRule {
  field: string;
  rule: 'required' | 'minLength' | 'maxLength' | 'pattern' | 'min' | 'max' | 'enum';
  value: any;
  errorMessage: string;
}

/**
 * AI Classifier Node Configuration
 * Categorizes and tags content
 */
export interface AIClassifierConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Classification type
  classificationType: 'single' | 'multi';
  
  // Categories
  categories: AIClassifierCategory[];
  allowCustomCategories: boolean;
  maxCategories?: number;
  
  // Input
  inputVariable: string;
  contextVariables: string[];
  
  // Prompt customization
  systemPrompt?: string;
  classificationCriteria?: string;
  
  // Output
  outputVariable: string;
  includeConfidence: boolean;
  includeReasoning: boolean;
  confidenceThreshold: number;
}

export interface AIClassifierCategory {
  id: string;
  name: string;
  description: string;
  keywords?: string[];
  examples?: string[];
  color?: string;
}

/**
 * AI Sentiment Analyzer Node Configuration
 * Analyzes sentiment and emotion
 */
export interface AISentimentAnalyzerConfig {
  // Model settings
  model: string;
  temperature: number;
  maxTokens: number;
  
  // Analysis type
  analysisType: 'basic' | 'detailed' | 'emotional';
  
  // Input
  inputVariable: string;
  contextVariables: string[];
  
  // Output settings
  outputVariable: string;
  includeScore: boolean;
  includeEmotions: boolean;
  includeKeyPhrases: boolean;
  includeSuggestions: boolean;
  
  // Thresholds for routing
  sentimentThresholds?: {
    positive: number;
    neutral: number;
    negative: number;
  };
  
  // Custom prompts
  systemPrompt?: string;
  analysisInstructions?: string;
}

export interface AISentimentResult {
  sentiment: 'positive' | 'neutral' | 'negative' | 'mixed';
  score: number;
  confidence: number;
  emotions?: {
    joy: number;
    anger: number;
    sadness: number;
    fear: number;
    surprise: number;
  };
  keyPhrases?: string[];
  suggestions?: string[];
}

/**
 * Human Review Node Configuration
 * Human-in-the-loop review for AI outputs
 */
export interface HumanReviewConfig {
  // Task settings
  taskTitle: string;
  taskDescription: string;
  
  // Assignment
  assignToRole?: string;
  assignToUser?: string;
  escalationRules?: AIEscalationRule[];
  
  // Review content
  reviewVariable: string;
  contextVariables: string[];
  showOriginalInput: boolean;
  
  // Review options
  reviewOptions: HumanReviewOption[];
  allowEdit: boolean;
  requireComments: boolean;
  
  // SLA
  dueInMinutes: number;
  escalateAfterMinutes?: number;
  
  // Output
  outputVariable: string;
  captureReviewerFeedback: boolean;
}

export interface HumanReviewOption {
  id: string;
  label: string;
  description: string;
  action: 'approve' | 'reject' | 'modify' | 'escalate';
  requiresComment: boolean;
  nextHandle?: string;
}

export interface AIEscalationRule {
  condition: 'timeout' | 'rejection' | 'uncertainty';
  escalateTo: string;
  notifyOriginal: boolean;
}

/**
 * AI Analytics types for cost and performance tracking
 */
export interface AINodeExecution {
  nodeId: number;
  nodeType: string;
  model: string;
  inputTokens: number;
  outputTokens: number;
  totalTokens: number;
  cost: number;
  latencyMs: number;
  success: boolean;
  errorMessage?: string;
  timestamp: string;
}

export interface AIAnalyticsSummary {
  period: string;
  totalCost: number;
  totalTokens: number;
  totalExecutions: number;
  successRate: number;
  averageLatencyMs: number;
  byModel: {
    model: string;
    cost: number;
    tokens: number;
    executions: number;
  }[];
  byNodeType: {
    nodeType: string;
    cost: number;
    tokens: number;
    executions: number;
  }[];
}

// ============================================================================
// Node type metadata - Default fallback values (overridden by config API)
// ============================================================================

// Default values used before config is loaded
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

// Cached configuration from the server
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

  // Get comprehensive workflow configuration (cached)
  async getConfig(forceRefresh = false): Promise<WorkflowConfig> {
    if (cachedConfig && !forceRefresh) {
      return cachedConfig;
    }
    
    if (configLoadPromise && !forceRefresh) {
      return configLoadPromise;
    }

    configLoadPromise = apiClient.get('/api/workflows/config')
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

  // Clear the cached configuration
  clearConfigCache(): void {
    cachedConfig = null;
    configLoadPromise = null;
  },

  // Get available entity types (uses cached config)
  async getEntityTypes(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.entityTypes;
  },

  // Get available node types (uses cached config)
  async getNodeTypes(): Promise<NodeTypeOption[]> {
    const config = await this.getConfig();
    return config.nodeTypes;
  },

  // Get workflow categories (uses cached config)
  async getCategories(): Promise<string[]> {
    const config = await this.getConfig();
    return config.categories;
  },

  // Get action types
  async getActionTypes(): Promise<ActionTypeOption[]> {
    const config = await this.getConfig();
    return config.actionTypes;
  },

  // Get trigger types
  async getTriggerTypes(): Promise<TriggerTypeOption[]> {
    const config = await this.getConfig();
    return config.triggerTypes;
  },

  // Get LLM providers (only configured ones)
  async getLLMProviders(): Promise<LLMProviderOption[]> {
    const config = await this.getConfig();
    return config.llmProviders.filter(p => p.isConfigured);
  },

  // Get all LLM models (from configured providers)
  async getLLMModels(): Promise<LLMModelOption[]> {
    const config = await this.getConfig();
    return config.llmModels;
  },

  // Get roles
  async getRoles(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.roles;
  },

  // Get status options
  async getStatusOptions(): Promise<StatusOption[]> {
    const config = await this.getConfig();
    return config.statusOptions;
  },

  // Get condition operators
  async getConditionOperators(): Promise<OperatorOption[]> {
    const config = await this.getConfig();
    return config.conditionOperators;
  },

  // Get fallback actions
  async getFallbackActions(): Promise<EntityTypeOption[]> {
    const config = await this.getConfig();
    return config.fallbackActions;
  },

  // Get icon options
  async getIconOptions(): Promise<string[]> {
    const config = await this.getConfig();
    return config.iconOptions;
  },

  // Get color options
  async getColorOptions(): Promise<string[]> {
    const config = await this.getConfig();
    return config.colorOptions;
  },

  // Get event types (for audit logs)
  async getEventTypes(): Promise<EventTypeOption[]> {
    const config = await this.getConfig();
    return config.eventTypes;
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
  },

  // ============================================================================
  // Audit Log & Timeline
  // ============================================================================

  // Get audit log for a workflow definition
  async getAuditLog(definitionId: number, params?: {
    eventType?: string;
    eventCategory?: string;
    fromDate?: string;
    toDate?: string;
    skip?: number;
    take?: number;
  }): Promise<{ items: AuditLogEntry[]; hasMore: boolean }> {
    const response = await apiClient.get(`/api/workflow-instances/definitions/${definitionId}/audit-log`, { params });
    return response.data;
  },

  // Export audit log as CSV
  async exportAuditLog(definitionId: number, params?: {
    fromDate?: string;
    toDate?: string;
  }): Promise<Blob> {
    const response = await apiClient.get(
      `/api/workflow-instances/definitions/${definitionId}/audit-log/export`, 
      { params, responseType: 'blob' }
    );
    return response.data;
  },

  // Get execution timeline for an instance
  async getExecutionTimeline(instanceId: number): Promise<ExecutionTimeline> {
    const response = await apiClient.get(`/api/workflow-instances/${instanceId}/timeline`);
    return response.data;
  },

  // Simulate workflow execution (dry run)
  async simulateWorkflow(workflowId: number, sampleData: object): Promise<SimulationResult> {
    const response = await apiClient.post(`/api/workflows/${workflowId}/simulate`, { sampleData });
    return response.data;
  }
};

// ============================================================================
// New Type Definitions
// ============================================================================

export interface AuditLogEntry {
  id: number;
  eventType: string;
  eventCategory: string;
  message: string;
  details?: string;
  actorName?: string;
  nodeName?: string;
  workerId?: string;
  durationMs?: number;
  timestamp: string;
}

export interface ExecutionTimeline {
  instanceId: number;
  status: string;
  startedAt?: string;
  completedAt?: string;
  totalDurationMs?: number;
  entries: TimelineEntry[];
}

export interface TimelineEntry {
  id: number;
  type: 'node' | 'task';
  name: string;
  nodeType: string;
  status: string;
  startedAt?: string;
  completedAt?: string;
  durationMs?: number;
  isSkipped: boolean;
  errorMessage?: string;
  sequence?: number;
  assignedTo?: string;
}

export interface SimulationResult {
  success: boolean;
  steps: SimulationStep[];
  finalState?: object;
  errors?: string[];
}

export interface SimulationStep {
  nodeId: number;
  nodeName: string;
  nodeType: string;
  action: string;
  result: 'executed' | 'skipped' | 'error';
  message?: string;
  duration?: number;
}

export default workflowService;

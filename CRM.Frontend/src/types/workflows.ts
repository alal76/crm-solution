/**
 * Workflow State Management Types
 * Frontend types for workflow state, context, and UI state management
 * 
 * PHASE 2: Frontend Types for New DTOs
 * This file provides TypeScript types for workflow-related state management
 */

import { BaseEntity } from './common';

// ============================================================================
// WORKFLOW STATE AND CONTEXT
// ============================================================================

/**
 * Global workflow context state
 */
export interface WorkflowContextState {
  currentWorkflow: WorkflowUIState | null;
  workflows: WorkflowUIState[];
  loading: boolean;
  error: string | null;
  selectedNodeId: string | null;
  selectedTransitionId: string | null;
  canvasZoom: number;
  canvasPan: { x: number; y: number };
}

/**
 * UI-specific workflow state (extends backend model)
 */
export interface WorkflowUIState {
  id: number;
  name: string;
  description?: string;
  entityType: string;
  status: 'Draft' | 'Active' | 'Paused' | 'Archived' | 'Deprecated';
  nodes: WorkflowNodeUI[];
  transitions: WorkflowTransitionUI[];
  isEditing: boolean;
  isDirty: boolean;
  validationErrors: ValidationError[];
  metadata?: WorkflowMetadata;
}

/**
 * UI state for a workflow node
 */
export interface WorkflowNodeUI {
  id: string; // Unique identifier (can be string for unsaved nodes)
  parentId?: number; // Database ID if saved
  name: string;
  type: WorkflowNodeType;
  description?: string;
  position: { x: number; y: number };
  size: { width: number; height: number };
  config: Record<string, unknown>; // Node-specific configuration
  isSelected: boolean;
  hasError: boolean;
  errorMessage?: string;
  incomingTransitions: string[]; // IDs of source transitions
  outgoingTransitions: string[]; // IDs of target transitions
}

/**
 * UI state for a workflow transition
 */
export interface WorkflowTransitionUI {
  id: string;
  parentId?: number;
  sourceNodeId: string;
  targetNodeId: string;
  label?: string;
  condition?: TransitionConditionUI;
  isSelected: boolean;
  hasError: boolean;
  errorMessage?: string;
  animated: boolean;
  lineStyle: 'solid' | 'dashed' | 'dotted';
  color: string;
}

/**
 * Transition condition UI state
 */
export interface TransitionConditionUI {
  type: 'Always' | 'Expression' | 'FieldValue' | 'StatusChange' | 'UserDecision' | 'AIDecision';
  expression?: string;
  fieldName?: string;
  operator?: 'equals' | 'notEquals' | 'greaterThan' | 'lessThan' | 'contains' | 'in';
  value?: unknown;
  description?: string;
}

// ============================================================================
// WORKFLOW TYPES
// ============================================================================

export enum WorkflowNodeType {
  Start = 'Start',
  End = 'End',
  Action = 'Action',
  Condition = 'Condition',
  Wait = 'Wait',
  HumanTask = 'HumanTask',
  Approval = 'Approval',
  Notification = 'Notification',
  SubWorkflow = 'SubWorkflow',
  Parallel = 'Parallel',
  Join = 'Join',
  Loop = 'Loop',
  AIDecision = 'AIDecision',
  AIAgent = 'AIAgent',
  DataMapper = 'DataMapper',
  SystemIntegration = 'SystemIntegration',
  Custom = 'Custom'
}

export enum WorkflowActionType {
  SendEmail = 'sendEmail',
  SendSMS = 'sendSMS',
  UpdateEntity = 'updateEntity',
  CreateEntity = 'createEntity',
  CallWebhook = 'callWebhook',
  AssignTask = 'assignTask',
  RequestApproval = 'requestApproval',
  ExecuteScript = 'executeScript',
  LogEvent = 'logEvent',
  ChangeStatus = 'changeStatus',
  NotifyUser = 'notifyUser',
  ScheduleJob = 'scheduleJob'
}

export enum WorkflowTriggerType {
  Manual = 'manual',
  Scheduled = 'scheduled',
  EventBased = 'eventBased',
  TimeoutBased = 'timeoutBased',
  ExternalAPI = 'externalAPI',
  User = 'user',
  System = 'system'
}

// ============================================================================
// WORKFLOW CONFIGURATION AND METADATA
// ============================================================================

/**
 * Metadata about the workflow
 */
export interface WorkflowMetadata {
  category?: string;
  tags?: string[];
  owner?: string;
  createdBy?: string;
  lastModifiedBy?: string;
  version: number;
  isSystem: boolean;
  documentation?: string;
  estimatedDurationMinutes?: number;
}

/**
 * Workflow execution context
 */
export interface WorkflowExecutionContext {
  instanceId: number;
  correlationId: string;
  entityType: string;
  entityId: number;
  currentNodeId: string;
  variables: Record<string, unknown>;
  executionHistory: ExecutionHistoryEntry[];
  startTime: Date;
  estimatedCompletionTime?: Date;
  status: ExecutionStatus;
  error?: {
    nodeId: string;
    message: string;
    code: string;
    timestamp: Date;
  };
}

export enum ExecutionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Waiting = 'Waiting',
  Paused = 'Paused',
  Completed = 'Completed',
  Failed = 'Failed',
  Rolled Back = 'Rolled Back',
  Cancelled = 'Cancelled'
}

/**
 * Execution history entry
 */
export interface ExecutionHistoryEntry {
  timestamp: Date;
  nodeId: string;
  nodeName: string;
  status: ExecutionStatus;
  durationMs: number;
  output?: Record<string, unknown>;
  error?: string;
  variables: Record<string, unknown>;
}

// ============================================================================
// WORKFLOW VALIDATION
// ============================================================================

/**
 * Validation error
 */
export interface ValidationError {
  id: string; // Node or transition ID
  level: 'error' | 'warning' | 'info';
  message: string;
  code: string;
  suggestion?: string;
  relatedTo: 'node' | 'transition' | 'workflow';
}

/**
 * Workflow validation result
 */
export interface WorkflowValidationResult {
  isValid: boolean;
  errors: ValidationError[];
  warnings: ValidationError[];
  missingConnections: string[]; // Node IDs that don't have proper connections
  unreachableNodes: string[]; // Node IDs that can't be reached
  recommendations: string[];
}

// ============================================================================
// WORKFLOW TEMPLATES AND PRESETS
// ============================================================================

/**
 * Workflow template for quick creation
 */
export interface WorkflowTemplate {
  id: number;
  name: string;
  description?: string;
  category: string;
  entityType: string;
  thumbnail?: string;
  nodes: WorkflowNodeUI[];
  transitions: WorkflowTransitionUI[];
  estimatedCompletionTime?: number;
  isPublic: boolean;
  tags?: string[];
  usageCount: number;
}

/**
 * Quick action template for common actions
 */
export interface ActionTemplate {
  id: string;
  name: string;
  description?: string;
  icon?: string;
  actionType: WorkflowActionType;
  defaultConfig: Record<string, unknown>;
  category: string;
}

// ============================================================================
// WORKFLOW MONITORING AND ANALYTICS
// ============================================================================

/**
 * Workflow execution statistics
 */
export interface WorkflowExecutionStats {
  workflowId: number;
  totalExecutions: number;
  successfulExecutions: number;
  failedExecutions: number;
  averageExecutionTimeMs: number;
  medianExecutionTimeMs: number;
  maxExecutionTimeMs: number;
  minExecutionTimeMs: number;
  successRate: number; // Percentage
  failureRate: number; // Percentage
  lastExecutedAt?: Date;
  executions30Days: ExecutionSummary[];
}

/**
 * Summary of workflow execution
 */
export interface ExecutionSummary {
  date: Date;
  count: number;
  successful: number;
  failed: number;
  averageTimeMs: number;
}

/**
 * Node execution analytics
 */
export interface NodeExecutionAnalytics {
  nodeId: string;
  nodeName: string;
  totalExecutions: number;
  successfulExecutions: number;
  failedExecutions: number;
  averageExecutionTimeMs: number;
  errorRate: number;
  mostCommonError?: string;
  upstreamNode?: string;
  downstreamNodes: string[];
}

// ============================================================================
// WORKFLOW DESIGNER ACTIONS
// ============================================================================

/**
 * Action payload for workflow designer state management
 */
export type WorkflowDesignerAction =
  | { type: 'SET_WORKFLOW'; payload: WorkflowUIState }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'ADD_NODE'; payload: WorkflowNodeUI }
  | { type: 'UPDATE_NODE'; payload: { id: string; updates: Partial<WorkflowNodeUI> } }
  | { type: 'DELETE_NODE'; payload: string }
  | { type: 'SELECT_NODE'; payload: string | null }
  | { type: 'ADD_TRANSITION'; payload: WorkflowTransitionUI }
  | { type: 'UPDATE_TRANSITION'; payload: { id: string; updates: Partial<WorkflowTransitionUI> } }
  | { type: 'DELETE_TRANSITION'; payload: string }
  | { type: 'SELECT_TRANSITION'; payload: string | null }
  | { type: 'SET_ZOOM'; payload: number }
  | { type: 'SET_PAN'; payload: { x: number; y: number } }
  | { type: 'SET_DIRTY'; payload: boolean }
  | { type: 'SET_VALIDATION_ERRORS'; payload: ValidationError[] }
  | { type: 'RESET' };

// ============================================================================
// COMMON WORKFLOW PATTERNS
// ============================================================================

/**
 * Pattern for approval workflow
 */
export interface ApprovalWorkflowPattern {
  id: string;
  name: string;
  approvers: ApprovalLevel[];
  escalationPath?: string[];
  requireAllApprovals: boolean;
  timeoutDays?: number;
  notificationTemplate?: string;
}

/**
 * Approval level in multi-level approval
 */
export interface ApprovalLevel {
  level: number;
  approverIds?: number[];
  approverGroupId?: number;
  approverRole?: string;
  requiredCount?: number; // Number of approvals needed at this level
}

/**
 * Pattern for notification workflow
 */
export interface NotificationWorkflowPattern {
  id: string;
  name: string;
  recipients: NotificationRecipient[];
  channels: NotificationChannel[];
  template?: string;
  variables?: Record<string, string>;
}

/**
 * Notification recipient
 */
export interface NotificationRecipient {
  type: 'user' | 'group' | 'role' | 'email' | 'phone';
  id?: string;
  value?: string; // Email or phone
}

/**
 * Notification channel
 */
export type NotificationChannel = 'email' | 'sms' | 'push' | 'slack' | 'teams' | 'webhook';

// ============================================================================
// WORKFLOW SIMULATION AND TESTING
// ============================================================================

/**
 * Workflow simulation parameters
 */
export interface WorkflowSimulationParams {
  workflowId: number;
  entityType: string;
  entityId: number;
  inputVariables: Record<string, unknown>;
  stopAtNode?: string; // For step-by-step simulation
  maxSteps?: number;
}

/**
 * Workflow simulation result
 */
export interface WorkflowSimulationResult {
  success: boolean;
  executionPath: string[]; // Node IDs in execution order
  executionHistory: ExecutionHistoryEntry[];
  finalVariables: Record<string, unknown>;
  error?: {
    nodeId: string;
    message: string;
    code: string;
  };
  warnings: string[];
  estimatedDurationMs: number;
}

// ============================================================================
// WORKFLOW HOOKS AND LISTENERS
// ============================================================================

/**
 * Workflow lifecycle hook
 */
export interface WorkflowHook {
  type: 'before' | 'after';
  event: 'start' | 'nodeEnter' | 'nodeExit' | 'end' | 'error';
  handler: (context: WorkflowExecutionContext) => Promise<void>;
  priority: number;
}

/**
 * Event listener for workflow changes
 */
export interface WorkflowChangeListener {
  onNodeAdded?: (node: WorkflowNodeUI) => void;
  onNodeUpdated?: (node: WorkflowNodeUI) => void;
  onNodeDeleted?: (nodeId: string) => void;
  onTransitionAdded?: (transition: WorkflowTransitionUI) => void;
  onTransitionUpdated?: (transition: WorkflowTransitionUI) => void;
  onTransitionDeleted?: (transitionId: string) => void;
  onWorkflowSaved?: (workflow: WorkflowUIState) => void;
  onValidationChanged?: (result: WorkflowValidationResult) => void;
}

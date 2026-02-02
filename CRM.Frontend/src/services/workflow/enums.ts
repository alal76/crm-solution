/**
 * CRM Solution - Workflow Enums
 * 
 * This file contains all enumeration types used in the workflow system.
 * Extracted from workflowService.ts for better modularity.
 */

// ============================================================================
// Workflow Status Enums
// ============================================================================

/**
 * Status of a workflow definition
 */
export enum WorkflowStatus {
  Draft = 'Draft',
  Active = 'Active',
  Paused = 'Paused',
  Archived = 'Archived',
  Deprecated = 'Deprecated'
}

/**
 * Status of a workflow version
 */
export enum WorkflowVersionStatus {
  Draft = 'Draft',
  Published = 'Published',
  Retired = 'Retired'
}

// ============================================================================
// Node and Transition Enums
// ============================================================================

/**
 * Types of nodes available in the workflow designer
 */
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

/**
 * Types of conditions for workflow transitions
 */
export enum TransitionConditionType {
  Always = 'Always',
  Expression = 'Expression',
  FieldValue = 'FieldValue',
  StatusChange = 'StatusChange',
  UserDecision = 'UserDecision',
  AIDecision = 'AIDecision'
}

// ============================================================================
// Instance and Task Enums
// ============================================================================

/**
 * Status of a running workflow instance
 */
export enum WorkflowInstanceStatus {
  Pending = 'Pending',
  Running = 'Running',
  Waiting = 'Waiting',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  TimedOut = 'TimedOut',
  Suspended = 'Suspended',
  Paused = 'Paused'
}

/**
 * Types of workflow tasks
 */
export enum WorkflowTaskType {
  HumanTask = 'HumanTask',
  Approval = 'Approval',
  Review = 'Review',
  DataEntry = 'DataEntry',
  AIReview = 'AIReview'
}

/**
 * Status of a workflow task
 */
export enum WorkflowTaskStatus {
  Pending = 'Pending',
  Queued = 'Queued',
  Assigned = 'Assigned',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Delegated = 'Delegated',
  Escalated = 'Escalated',
  DeadLetter = 'DeadLetter'
}

// ============================================================================
// Logging Enums
// ============================================================================

/**
 * Log levels for workflow execution logs
 */
export enum WorkflowLogLevel {
  Trace = 'Trace',
  Debug = 'Debug',
  Information = 'Information',
  Warning = 'Warning',
  Error = 'Error',
  Critical = 'Critical'
}

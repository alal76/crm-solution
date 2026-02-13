/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Workflow Components - Export all workflow-related components
 */

// Rule Builder
export { 
  RuleBuilder, 
  createDefaultRuleGroup, 
  ruleGroupToExpression,
  type ConditionRule,
  type ConditionGroup,
  type ConditionOperator,
  type LogicalOperator,
  type FieldDefinition,
  type VariableDefinition,
  type RuleBuilderProps,
} from './RuleBuilder';

// Condition Builder (wrapper)
export { default as ConditionBuilder } from './ConditionBuilder';

// Version Diff Viewer
export { VersionDiffViewer } from './VersionDiffViewer';

// Execution Timeline
export { 
  ExecutionTimeline,
  type TimelineStep,
} from './ExecutionTimeline';

// Instance Timeline (wrapper)
export { default as InstanceTimeline } from './InstanceTimeline';

// Audit Log Viewer
export { 
  AuditLogViewer,
  type AuditLogEntry,
} from './AuditLogViewer';

// Workflow Simulator
export { WorkflowSimulator } from './WorkflowSimulator';

// Workflow Viewer (read-only)
export { default as WorkflowViewer } from './WorkflowViewer';

// Enhanced Properties Panel
export { 
  EnhancedPropertiesPanel,
  type NodeConfiguration,
} from './EnhancedPropertiesPanel';

// AI Properties Panel - AI-enhanced workflow node configurations
export { AIPropertiesPanel } from './AIPropertiesPanel';

// Trigger Properties Panel - Trigger node configuration with field conditions
export { 
  TriggerPropertiesPanel,
  type TriggerConfiguration,
} from './TriggerPropertiesPanel';

// Workflow Trigger Editor (wrapper)
export { default as WorkflowTriggerEditor } from './WorkflowTriggerEditor';

// Action Properties Panel - Action node configuration with field updates
export { 
  ActionPropertiesPanel,
  type ActionConfiguration,
} from './ActionPropertiesPanel';

// Action Config Panel (wrapper)
export { default as ActionConfigPanel } from './ActionConfigPanel';

// Workflow Version History
export { default as WorkflowVersionHistory } from './WorkflowVersionHistory';

// Task components
export { default as TaskCard } from './TaskCard';
export { default as TaskList } from './TaskList';
export { default as TaskApprovalDialog } from './TaskApprovalDialog';

// AI Analytics Dashboard - Cost tracking and performance monitoring
export { AIAnalyticsDashboard } from './AIAnalyticsDashboard';
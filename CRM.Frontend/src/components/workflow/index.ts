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

// Version Diff Viewer
export { VersionDiffViewer } from './VersionDiffViewer';

// Execution Timeline
export { 
  ExecutionTimeline,
  type TimelineStep,
} from './ExecutionTimeline';

// Audit Log Viewer
export { 
  AuditLogViewer,
  type AuditLogEntry,
} from './AuditLogViewer';

// Workflow Simulator
export { WorkflowSimulator } from './WorkflowSimulator';

// Enhanced Properties Panel
export { 
  EnhancedPropertiesPanel,
  type NodeConfiguration,
} from './EnhancedPropertiesPanel';

// AI Properties Panel - AI-enhanced workflow node configurations
export { AIPropertiesPanel } from './AIPropertiesPanel';

// AI Analytics Dashboard - Cost tracking and performance monitoring
export { AIAnalyticsDashboard } from './AIAnalyticsDashboard';
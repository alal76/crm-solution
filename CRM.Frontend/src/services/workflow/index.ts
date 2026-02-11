/**
 * CRM Solution - Workflow Module
 * 
 * Barrel export file for the workflow service module.
 * This provides a clean public API for consuming the workflow functionality.
 * 
 * Usage:
 *   import { workflowService, WorkflowDefinition, WorkflowStatus } from '@/services/workflow';
 * 
 * The module is organized as follows:
 * - enums.ts: Workflow status, node type, and task enums
 * - types.ts: Core workflow interfaces and DTOs
 * - aiTypes.ts: AI-enhanced workflow node configuration types
 * - workflowDefinitionApi.ts: API for workflow definitions, versions, nodes, transitions
 * - workflowInstanceApi.ts: API for workflow instances, tasks, logs
 */

// ============================================================================
// Enums
// ============================================================================

export {
  WorkflowStatus,
  WorkflowVersionStatus,
  WorkflowNodeType,
  TransitionConditionType,
  WorkflowInstanceStatus,
  WorkflowTaskType,
  WorkflowTaskStatus,
  WorkflowLogLevel
} from './enums';

// ============================================================================
// Core Types
// ============================================================================

export type {
  // Workflow Definitions
  WorkflowDefinition,
  WorkflowDefinitionDetail,
  WorkflowVersionSummary,
  WorkflowVersionDetail,
  WorkflowNode,
  WorkflowTransition,
  
  // Workflow Instances
  WorkflowInstance,
  WorkflowInstanceDetail,
  WorkflowNodeInstance,
  WorkflowTask,
  WorkflowLog,
  HumanTask,
  
  // Statistics
  WorkflowStatistics,
  InstanceStatistics,
  
  // Create/Update DTOs
  CreateWorkflowDto,
  UpdateWorkflowDto,
  CreateNodeDto,
  UpdateNodeDto,
  NodePositionDto,
  CreateTransitionDto,
  UpdateTransitionDto,
  StartWorkflowDto,
  
  // Configuration Options
  EntityTypeOption,
  NodeTypeOption,
  ActionTypeOption,
  TriggerTypeOption,
  OperatorOption,
  StatusOption,
  LLMProviderOption,
  LLMModelOption,
  EventTypeOption,
  EntityFieldConfig,
  RelatedEntityConfig,
  WorkflowConfig,
  
  // Audit Log & Timeline
  AuditLogEntry,
  ExecutionTimeline,
  TimelineEntry,
  SimulationResult,
  SimulationStep,
  
  // Paginated Results
  PaginatedResult
} from './types';

// ============================================================================
// AI Types
// ============================================================================

export type {
  // AI Tool Definitions
  AIToolDefinition,
  AIToolParameter,
  PromptTemplate,
  
  // AI Node Configurations
  AIDecisionConfig,
  AIDecisionOption,
  AIAgentConfig,
  AIContentGeneratorConfig,
  AIDataExtractorConfig,
  AIExtractionField,
  AIValidationRule,
  AIClassifierConfig,
  AIClassifierCategory,
  AISentimentAnalyzerConfig,
  AISentimentResult,
  
  // Human Review
  HumanReviewConfig,
  HumanReviewOption,
  AIEscalationRule,
  
  // AI Analytics
  AINodeExecution,
  AIAnalyticsSummary
} from './aiTypes';

// ============================================================================
// API Services
// ============================================================================

export { workflowService, nodeTypeInfo, statusColors, default } from './workflowDefinitionApi';
export { workflowInstanceService } from './workflowInstanceApi';

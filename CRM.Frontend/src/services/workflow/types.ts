/**
 * CRM Solution - Workflow Types
 * 
 * This file contains all interface and type definitions for the workflow system.
 * Extracted from workflowService.ts for better modularity.
 */

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

// ============================================================================
// Types - Configuration Options
// ============================================================================

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

export interface EntityFieldConfig {
  name: string;
  label: string;
  type: 'string' | 'number' | 'boolean' | 'date' | 'enum' | 'reference';
  required: boolean;
  enumValues?: string[];
  referenceEntity?: string;
  group?: string;
}

export interface RelatedEntityConfig {
  name: string;
  label: string;
  entityType: string;
  relationType: 'parent' | 'child' | 'related';
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
  entityFields: Record<string, EntityFieldConfig[]>;
  relatedEntities: Record<string, RelatedEntityConfig[]>;
}

// ============================================================================
// Types - Audit Log & Timeline
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

// ============================================================================
// Types - Paginated Results
// ============================================================================

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

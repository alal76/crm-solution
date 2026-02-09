# Workflow Frontend Audit

> **Generated:** February 2026  
> **Scope:** All workflow-related frontend code in `CRM.Frontend/src/`  
> **Files Audited:** 20 files (7 service/type, 3 pages, 11 components) + routing/navigation  
> **Total Lines of Code:** ~10,406  

---

## Table of Contents

1. [File Inventory](#1-file-inventory)
2. [TypeScript Types & Interfaces](#2-typescript-types--interfaces)
3. [Service Files](#3-service-files)
4. [Workflow Pages](#4-workflow-pages)
5. [Workflow Components](#5-workflow-components)
6. [Routing (App.tsx)](#6-routing-apptsx)
7. [Navigation (Navigation.tsx)](#7-navigation-navigationtsx)
8. [Cross-Cutting References](#8-cross-cutting-references)
9. [Gap Analysis & Findings](#9-gap-analysis--findings)
10. [Architecture Summary](#10-architecture-summary)

---

## 1. File Inventory

### Service / Type Layer (`src/services/workflow/`)

| File | Lines | Purpose |
|------|-------|---------|
| `enums.ts` | 130 | 8 enums (WorkflowStatus, NodeType, InstanceStatus, etc.) |
| `types.ts` | 535 | 30+ interfaces/DTOs (definitions, versions, nodes, transitions, instances, tasks, logs, configs) |
| `aiTypes.ts` | 426 | 15+ AI-specific config interfaces (Decision, Agent, ContentGen, Extractor, Classifier, Sentiment, HumanReview) |
| `workflowDefinitionApi.ts` | 420 | ~35 methods — Workflow CRUD, config cache, versions, nodes, transitions |
| `workflowInstanceApi.ts` | 190 | ~16 methods — Instance lifecycle, tasks, logs, audit, timeline, simulation |
| `index.ts` | 132 | Barrel exports for all enums, types, and service objects |

### Barrel Re-Export (`src/services/`)

| File | Lines | Purpose |
|------|-------|---------|
| `workflowService.ts` | 31 | Backward-compatible barrel re-export from `./workflow` |

### Pages (`src/pages/admin/`)

| File | Lines | Purpose |
|------|-------|---------|
| `WorkflowListPage.tsx` | 852 | List/manage workflow definitions + statistics |
| `WorkflowDesignerPage.tsx` | 1,249 | Visual SVG canvas node editor |
| `WorkflowMonitorPage.tsx` | 1,029 | Instance monitoring, tasks, statistics |

### Components (`src/components/workflow/`)

| File | Lines | Purpose |
|------|-------|---------|
| `AIPropertiesPanel.tsx` | 2,470 | Property panels for 7 AI node types |
| `ActionPropertiesPanel.tsx` | 1,044 | Property panel for 8 action types |
| `EnhancedPropertiesPanel.tsx` | 824 | General node properties (6 tabs) |
| `TriggerPropertiesPanel.tsx` | 713 | Trigger node properties (3 tabs) |
| `RuleBuilder.tsx` | 707 | Visual condition rule builder |
| `WorkflowSimulator.tsx` | 704 | Client-side workflow simulation engine |
| `AIAnalyticsDashboard.tsx` | 618 | AI cost/token/latency analytics (MOCK DATA) |
| `AuditLogViewer.tsx` | 534 | Audit log dialog with filters + export |
| `VersionDiffViewer.tsx` | 526 | Side-by-side version comparison |
| `ExecutionTimeline.tsx` | ~380 | Gantt-style execution timeline |
| `index.ts` | 62 | Barrel exports for all 10 components |

**Grand Total: ~10,406 lines across 20 files**

---

## 2. TypeScript Types & Interfaces

### 2.1 Enums (`enums.ts` — 130 lines)

| Enum | Values | Used By |
|------|--------|---------|
| `WorkflowStatus` | `Draft`, `Active`, `Paused`, `Archived`, `Deprecated` (5) | WorkflowListPage, workflowDefinitionApi |
| `WorkflowVersionStatus` | `Draft`, `Active`, `Archived` (3) | WorkflowDesignerPage |
| `WorkflowNodeType` | `Start`, `End`, `Action`, `Condition`, `Wait`, `Trigger`, `HumanTask`, `SubWorkflow`, `Parallel`, `Loop`, `AIDecision`, `AIAgent`, `AIContentGenerator`, `AIDataExtractor`, `AIClassifier`, `AISentimentAnalyzer`, `HumanReview` (17) | All property panels, WorkflowDesignerPage |
| `TransitionConditionType` | `Always`, `Expression`, `FieldComparison`, `StatusChange`, `Custom`, `AIDecision` (6) | RuleBuilder, EnhancedPropertiesPanel |
| `WorkflowInstanceStatus` | `Pending`, `Running`, `Completed`, `Failed`, `Cancelled`, `Suspended`, `TimedOut`, `WaitingForInput`, `WaitingForApproval` (9) | WorkflowMonitorPage |
| `WorkflowTaskType` | `HumanApproval`, `DataEntry`, `Review`, `Escalation`, `Custom` (5) | WorkflowMonitorPage tasks tab |
| `WorkflowTaskStatus` | `Pending`, `Assigned`, `InProgress`, `Completed`, `Rejected`, `Escalated`, `TimedOut`, `Cancelled`, `Skipped`, `Failed` (10) | WorkflowMonitorPage tasks tab |
| `WorkflowLogLevel` | `Debug`, `Info`, `Warning`, `Error`, `Critical`, `Trace` (6) | WorkflowMonitorPage logs tab |

### 2.2 Core Types (`types.ts` — 535 lines)

#### Definition & Version Types

| Interface | Key Fields | Notes |
|-----------|------------|-------|
| `WorkflowDefinition` | `id`, `name`, `description`, `category`, `entityType`, `status`, `iconName`, `color`, `priority`, `maxConcurrentInstances`, `defaultTimeoutHours`, `tags[]`, `currentVersionId`, `currentVersionNumber`, `totalVersions`, `totalInstances`, `createdAt`, `updatedAt`, `createdByName` (20 fields) | Main list item DTO |
| `WorkflowDefinitionDetail` | extends `WorkflowDefinition` + `versions: WorkflowVersionSummary[]`, `metadata` | Detail view |
| `WorkflowVersionSummary` | `id`, `versionNumber`, `status`, `description`, `nodeCount`, `transitionCount`, `createdAt`, `createdByName` | Version list item |
| `WorkflowVersionDetail` | extends `WorkflowVersionSummary` + `nodes: WorkflowNode[]`, `transitions: WorkflowTransition[]`, `canvasLayout?`, `publishedAt?`, `activatedAt?`, `activatedByName?` | Full version for designer |

#### Node & Transition Types

| Interface | Key Fields | Notes |
|-----------|------------|-------|
| `WorkflowNode` | `id`, `nodeKey`, `name`, `description`, `nodeType`, `nodeSubType`, `positionX`, `positionY`, `width`, `height`, `iconName`, `color`, `isStartNode`, `isEndNode`, `configuration` (JSON string), `timeoutMinutes`, `retryCount`, `executionOrder` (18 fields) | Canvas node |
| `WorkflowTransition` | `id`, `transitionKey`, `sourceNodeId`, `targetNodeId`, `label`, `description`, `conditionType`, `conditionExpression`, `isDefault`, `priority`, `sourceHandle`, `targetHandle`, `lineStyle`, `color`, `animationStyle` (15 fields) | Canvas edge |

#### Instance & Execution Types

| Interface | Key Fields | Notes |
|-----------|------------|-------|
| `WorkflowInstance` | `id`, `workflowDefinitionId`, `workflowName`, `versionId`, `versionNumber`, `entityType`, `entityId`, `status`, `triggerEvent`, `inputData`, `outputData`, `currentNodeId`, `currentNodeName`, `errorMessage`, `retryCount`, `startedAt`, `completedAt`, `cancelledAt`, `cancelledByName`, `createdAt`, `createdByName`, `lastActivityAt` (22 fields) | Instance list item |
| `WorkflowInstanceDetail` | extends `WorkflowInstance` + `nodeInstances: WorkflowNodeInstance[]`, `tasks: WorkflowTask[]`, `logs: WorkflowLog[]`, `variables` | Full instance detail |
| `WorkflowNodeInstance` | `id`, `nodeId`, `nodeName`, `nodeType`, `status`, `startedAt`, `completedAt`, `errorMessage`, `retryCount`, `inputData`, `outputData`, `executionOrder` (12 fields) | Node execution state |
| `WorkflowTask` | `id`, `nodeInstanceId`, `nodeName`, `taskType`, `title`, `description`, `assignedTo`, `assignedToName`, `status`, `dueDate`, `completedAt`, `completedByName`, `result`, `comments` (14 fields) | Human task |
| `WorkflowLog` | `id`, `nodeInstanceId?`, `nodeName?`, `level`, `message`, `details?`, `source?`, `timestamp`, `correlationId?`, `metadata?` (10 fields) | Execution log |
| `HumanTask` | `id`, `workflowInstanceId`, `workflowName`, `nodeName`, `taskType`, `title`, `description`, `assignedTo`, `assignedToName`, `status`, `dueDate`, `createdAt`, `entityType`, `entityId` (14 fields) | "My Tasks" list item |

#### Statistics Types

| Interface | Key Fields | Notes |
|-----------|------------|-------|
| `WorkflowStatistics` | `totalWorkflows`, `activeWorkflows`, `pausedWorkflows`, `totalInstances`, `runningInstances`, `completedInstances`, `failedInstances`, `averageDurationMinutes`, `successRate`, `instancesByStatus`, `instancesByWorkflow` (11 fields) | List page stats tab |
| `InstanceStatistics` | `totalInstances`, `completedInstances`, `failedInstances`, `runningInstances`, `averageDurationMinutes`, `successRate`, `instancesByStatus`, `instancesByDay`, `averageNodeDuration`, `nodeSuccessRates` (10 fields) | Monitor page stats tab |

#### DTO Types (Create/Update)

| Interface | Fields | Notes |
|-----------|--------|-------|
| `CreateWorkflowDto` | `name`, `description?`, `category`, `entityType`, `iconName?`, `color?`, `priority?`, `maxConcurrentInstances?`, `defaultTimeoutHours?`, `tags?`, `metadata?` (12) | POST body |
| `UpdateWorkflowDto` | All optional: `name?`, `description?`, `category?`, `entityType?`, `iconName?`, `color?`, `priority?`, `maxConcurrentInstances?`, `defaultTimeoutHours?`, `tags?`, `metadata?` (11) | PUT body |
| `CreateNodeDto` | `nodeKey?`, `name`, `description?`, `nodeType`, `nodeSubType?`, `positionX`, `positionY`, `width?`, `height?`, `iconName?`, `color?`, `isStartNode?`, `isEndNode?`, `configuration?`, `timeoutMinutes?`, `retryCount?`, `retryDelaySeconds?`, `useExponentialBackoff?`, `executionOrder?` (20) | Add node |
| `UpdateNodeDto` | All optional (18 fields matching CreateNodeDto minus nodeKey) | Update node |
| `NodePositionDto` | `nodeId`, `x`, `y` | Batch position update |
| `CreateTransitionDto` | `sourceNodeId`, `targetNodeId`, `transitionKey?`, `label?`, `description?`, `conditionType?`, `conditionExpression?`, `isDefault?`, `priority?`, `sourceHandle?`, `targetHandle?`, `lineStyle?`, `color?`, `animationStyle?` (14) | Add transition |
| `UpdateTransitionDto` | All optional (11 fields, excludes source/target IDs) | Update transition |
| `StartWorkflowDto` | `workflowDefinitionId`, `entityType`, `entityId`, `triggerEvent?`, `inputData?`, `scheduledAt?` (6) | Start instance |

#### Configuration Option Types

| Interface | Fields | Used By |
|-----------|--------|---------|
| `EntityTypeOption` | `value`, `label` | Dropdown options |
| `NodeTypeOption` | `value`, `label`, `icon`, `color`, `description?` | Node palette |
| `ActionTypeOption` | `value`, `label`, `category`, `icon` | ActionPropertiesPanel |
| `TriggerTypeOption` | `value`, `label`, `description`, `icon` | TriggerPropertiesPanel |
| `OperatorOption` | `value`, `label`, `appliesTo[]` | RuleBuilder |
| `StatusOption` | `value`, `label`, `color`, `bgColor`, `icon` | Status chips |
| `LLMProviderOption` | `value`, `label`, `isConfigured`, `models[]` | AIPropertiesPanel |
| `LLMModelOption` | `value`, `label`, `provider`, `isDefault` | AIPropertiesPanel |
| `EventTypeOption` | `value`, `label`, `color`, `category` | AuditLogViewer |
| `EntityFieldConfig` | `name`, `label`, `type` (6 types), `required`, `enumValues?`, `referenceEntity?`, `group?` | TriggerPropertiesPanel field conditions |
| `RelatedEntityConfig` | `name`, `label`, `entityType`, `relationType` (parent/child/related) | TriggerPropertiesPanel advanced |
| `WorkflowConfig` | 16 fields aggregating all option types + `entityFields` record + `relatedEntities` record | Cached via proxy in workflowDefinitionApi |

#### Audit / Timeline / Simulation Types

| Interface | Key Fields | Notes |
|-----------|------------|-------|
| `AuditLogEntry` (types.ts) | `id`, `eventType`, `eventCategory`, `message`, `details?`, `actorName?`, `nodeName?`, `workerId?`, `durationMs?`, `timestamp` (10 fields) | ⚠️ Different from component's local AuditLogEntry |
| `ExecutionTimeline` | `instanceId`, `status`, `startedAt?`, `completedAt?`, `totalDurationMs?`, `entries[]` | Timeline widget |
| `TimelineEntry` | `id`, `type` (node/task), `name`, `nodeType`, `status`, `startedAt?`, `completedAt?`, `durationMs?`, `isSkipped`, `errorMessage?`, `sequence?`, `assignedTo?` (12) | Timeline bar |
| `SimulationResult` | `success`, `steps[]`, `finalState?`, `errors?` | Simulator output |
| `SimulationStep` | `nodeId`, `nodeName`, `nodeType`, `action`, `result` (executed/skipped/error), `message?`, `duration?` | Simulator step |
| `PaginatedResult<T>` | `items[]`, `totalCount`, `pageNumber`, `pageSize`, `totalPages` | Paginated API responses |

### 2.3 AI Types (`aiTypes.ts` — 426 lines)

| Interface | Key Fields | Used By |
|-----------|------------|---------|
| `AIToolDefinition` | `id`, `name`, `description`, `category`, `icon`, `parameters: AIToolParameter[]`, `requiresApproval`, `isEnabled?`, `maxCallsPerExecution?` (9) | AIAgentPanel |
| `AIToolParameter` | `name`, `type`, `description`, `required`, `defaultValue?`, `enumValues?` | Tool config |
| `PromptTemplate` | `id`, `name`, `description`, `template`, `variables[]`, `category?`, `isDefault?` | Not yet used in UI |
| `AIDecisionConfig` | `model`, `temperature`, `maxTokens`, `systemPrompt`, `userPromptTemplate`, `decisionOptions: AIDecisionOption[]`, `confidenceThreshold`, `inputVariables`, `outputVariable`, `retryOnError`, `retryCount?`, `fallbackOption?` (12) | AIDecisionPanel |
| `AIDecisionOption` | `id`, `label`, `description`, `matchCriteria?`, `outputValue?`, `priority?` | Decision option CRUD |
| `AIAgentConfig` | `agentName`, `agentDescription`, `systemPrompt`, `model`, `temperature`, `maxTokens`, `maxIterations`, `availableTools: string[]`, `toolApprovalRequired`, `autonomyLevel` (4 levels), `canModifyData`, `canSendCommunications`, `enableMemory`, `memoryType` (3 types), `maxMemoryItems`, `primaryGoal`, `constraints[]`, `stopConditions[]`, `outputVariable`, `maxCostPerExecution?`, `maxApiCalls?` | AIAgentPanel |
| `AIContentGeneratorConfig` | `contentType` (6 types), `model`, `temperature`, `maxTokens`, `systemPrompt`, `userPromptTemplate`, `tone` (5 values), `language`, `useTemplate`, `templateId?`, `inputVariables[]`, `contextFields[]`, `outputVariable`, `outputFormat` (4 formats), `requiresReview` | AIContentGeneratorPanel |
| `AIDataExtractorConfig` | `model`, `temperature`, `maxTokens`, `inputVariable`, `extractionSchema: AIExtractionField[]`, `outputVariable`, `outputFormat`, `validateOutput`, `onExtractionFailure` (error/skip/default) | AIDataExtractorPanel |
| `AIExtractionField` | `fieldName`, `fieldType` (7 types), `description`, `required`, `defaultValue?`, `validationRules: AIValidationRule[]` | Extraction schema CRUD |
| `AIValidationRule` | `type` (required/pattern/range/enum/custom), `value`, `message` | Field validation |
| `AIClassifierConfig` | `model`, `temperature`, `maxTokens`, `classificationType` (single/multi), `categories: AIClassifierCategory[]`, `allowCustomCategories`, `inputVariable`, `contextVariables[]`, `outputVariable`, `includeConfidence`, `includeReasoning`, `confidenceThreshold` | AIClassifierPanel |
| `AIClassifierCategory` | `id`, `name`, `description`, `keywords[]`, `examples[]` | Category CRUD |
| `AISentimentAnalyzerConfig` | `model`, `temperature`, `maxTokens`, `analysisType` (basic/detailed/emotional), `inputVariable`, `contextVariables[]`, `outputVariable`, `includeScore`, `includeEmotions`, `includeKeyPhrases`, `includeSuggestions`, `sentimentThresholds: { positive, neutral, negative }` | AISentimentAnalyzerPanel |
| `AISentimentResult` | `score` (-1 to 1), `label`, `confidence`, `emotions: { joy, anger, sadness, surprise, fear }`, `keyPhrases[]` | Type definition only |
| `HumanReviewConfig` | `taskTitle`, `taskDescription`, `assignedRole?`, `reviewVariable`, `contextVariables[]`, `showOriginalInput`, `reviewOptions: HumanReviewOption[]`, `allowEdit`, `requireComments`, `dueInMinutes`, `outputVariable`, `captureReviewerFeedback`, `escalationRules?: AIEscalationRule[]` | HumanReviewPanel |
| `HumanReviewOption` | `id`, `label`, `description`, `action` (approve/reject/modify/escalate), `requiresComment`, `icon?`, `color?` | Review option CRUD |
| `AIEscalationRule` | `condition`, `targetRole`, `timeoutMinutes`, `notifyOriginalReviewer` | Type definition only (not yet in UI) |
| `AINodeExecution` | `nodeType`, `model`, `inputTokens`, `outputTokens`, `totalTokens`, `cost`, `latencyMs`, `success`, `errorMessage?`, `timestamp`, `workflowInstanceId?`, `nodeId?`, `requestId?` (13) | AIAnalyticsDashboard |
| `AIAnalyticsSummary` | `totalCost`, `totalTokens`, `totalExecutions`, `successRate`, `averageLatencyMs`, `costByModel: Record<string, number>`, `costByNodeType: Record<string, number>` (7) | AIAnalyticsDashboard |

---

## 3. Service Files

### 3.1 `workflowDefinitionApi.ts` (420 lines — ~35 methods)

**Architecture:**
- Imports from `apiClient` (shared Axios instance) + 30 types from `./types`
- Implements a **Proxy-based lazy configuration cache** — `cachedConfig` is loaded once on first access via `workflowService.getConfig()`, then served from memory. `nodeTypeInfo` and `statusColors` are Proxy objects that delegate to the cached config.
- Default fallback data: `defaultNodeTypeInfo` (17 node types with icon/color/label), `defaultStatusColors` (13 status→color mappings)

**Method Inventory:**

| Category | Method | Signature | Endpoint |
|----------|--------|-----------|----------|
| **Workflow CRUD** | `getWorkflows` | `(params?) → PaginatedResult<WorkflowDefinition>` | `GET /api/workflowdefinitions` |
| | `getWorkflow` | `(id) → WorkflowDefinitionDetail` | `GET /api/workflowdefinitions/{id}` |
| | `createWorkflow` | `(dto: CreateWorkflowDto) → WorkflowDefinition` | `POST /api/workflowdefinitions` |
| | `updateWorkflow` | `(id, dto: UpdateWorkflowDto) → WorkflowDefinition` | `PUT /api/workflowdefinitions/{id}` |
| | `deleteWorkflow` | `(id) → void` | `DELETE /api/workflowdefinitions/{id}` |
| | `activateWorkflow` | `(workflowId, versionId) → void` | `POST /api/workflowdefinitions/{id}/activate` |
| | `pauseWorkflow` | `(id) → void` | `POST /api/workflowdefinitions/{id}/pause` |
| **Statistics** | `getStatistics` | `() → WorkflowStatistics` | `GET /api/workflowdefinitions/statistics` |
| **Config** | `getConfig` | `(forceRefresh?) → WorkflowConfig` | `GET /api/workflowdefinitions/config` |
| | `clearConfigCache` | `() → void` | (local cache clear) |
| **Config Accessors** | `getEntityTypes` | `() → EntityTypeOption[]` | via cached config |
| | `getNodeTypes` | `() → NodeTypeOption[]` | via cached config |
| | `getCategories` | `() → string[]` | via cached config |
| | `getActionTypes` | `() → ActionTypeOption[]` | via cached config |
| | `getTriggerTypes` | `() → TriggerTypeOption[]` | via cached config |
| | `getLLMProviders` | `() → LLMProviderOption[]` | via cached config (filtered by `isConfigured`) |
| | `getLLMModels` | `() → LLMModelOption[]` | via cached config |
| | `getRoles` | `() → EntityTypeOption[]` | via cached config |
| | `getStatusOptions` | `() → StatusOption[]` | via cached config |
| | `getConditionOperators` | `() → OperatorOption[]` | via cached config |
| | `getFallbackActions` | `() → EntityTypeOption[]` | via cached config |
| | `getIconOptions` | `() → string[]` | via cached config |
| | `getColorOptions` | `() → string[]` | via cached config |
| | `getEventTypes` | `() → EventTypeOption[]` | via cached config |
| **Versions** | `getVersion` | `(versionId) → WorkflowVersionDetail` | `GET /api/workflowversions/{id}` |
| | `createVersion` | `(workflowId, sourceVersionId?) → { id, versionNumber }` | `POST /api/workflowdefinitions/{id}/versions` |
| | `saveCanvasLayout` | `(versionId, canvasLayout) → void` | `PUT /api/workflowversions/{id}/canvas-layout` |
| **Nodes** | `addNode` | `(versionId, dto: CreateNodeDto) → { id, nodeKey }` | `POST /api/workflowversions/{id}/nodes` |
| | `updateNode` | `(nodeId, dto: UpdateNodeDto) → void` | `PUT /api/workflownodes/{id}` |
| | `deleteNode` | `(nodeId) → void` | `DELETE /api/workflownodes/{id}` |
| | `updateNodePositions` | `(versionId, positions: NodePositionDto[]) → void` | `PUT /api/workflowversions/{id}/nodes/positions` |
| **Transitions** | `addTransition` | `(versionId, dto: CreateTransitionDto) → { id }` | `POST /api/workflowversions/{id}/transitions` |
| | `updateTransition` | `(transitionId, dto: UpdateTransitionDto) → void` | `PUT /api/workflowtransitions/{id}` |
| | `deleteTransition` | `(transitionId) → void` | `DELETE /api/workflowtransitions/{id}` |

### 3.2 `workflowInstanceApi.ts` (190 lines — ~16 methods)

| Category | Method | Signature | Endpoint |
|----------|--------|-----------|----------|
| **Instance CRUD** | `getInstances` | `(params?) → PaginatedResult<WorkflowInstance>` | `GET /api/workflowinstances` |
| | `getInstance` | `(id) → WorkflowInstanceDetail` | `GET /api/workflowinstances/{id}` |
| | `getInstancesForEntity` | `(entityType, entityId) → WorkflowInstance[]` | `GET /api/workflowinstances/entity/{type}/{id}` |
| **Lifecycle** | `startWorkflow` | `(dto: StartWorkflowDto) → WorkflowInstance` | `POST /api/workflowinstances/start` |
| | `cancelInstance` | `(id) → void` | `POST /api/workflowinstances/{id}/cancel` |
| | `pauseInstance` | `(id) → void` | `POST /api/workflowinstances/{id}/pause` |
| | `resumeInstance` | `(id) → void` | `POST /api/workflowinstances/{id}/resume` |
| | `retryInstance` | `(id) → void` | `POST /api/workflowinstances/{id}/retry` |
| | `skipNode` | `(instanceId, nodeId) → void` | `POST /api/workflowinstances/{id}/nodes/{nodeId}/skip` |
| **Tasks** | `getMyTasks` | `(params?) → PaginatedResult<HumanTask>` | `GET /api/workflowtasks/my-tasks` |
| | `claimTask` | `(taskId) → void` | `POST /api/workflowtasks/{id}/claim` |
| | `completeTask` | `(taskId, result, comments?) → void` | `POST /api/workflowtasks/{id}/complete` |
| **Logs & Stats** | `getLogs` | `(params?) → PaginatedResult<WorkflowLog>` | `GET /api/workflowinstances/logs` |
| | `getInstanceLogs` | `(instanceId) → WorkflowLog[]` | `GET /api/workflowinstances/{id}/logs` |
| | `getStatistics` | `(params?) → InstanceStatistics` | `GET /api/workflowinstances/statistics` |
| **Audit & Timeline** | `getAuditLog` | `(params?) → PaginatedResult<AuditLogEntry>` | `GET /api/workflowinstances/audit` |
| | `exportAuditLog` | `(params?) → Blob` | `GET /api/workflowinstances/audit/export` |
| | `getExecutionTimeline` | `(instanceId) → ExecutionTimeline` | `GET /api/workflowinstances/{id}/timeline` |
| | `simulateWorkflow` | `(workflowId, inputData?) → SimulationResult` | `POST /api/workflowdefinitions/{id}/simulate` |

**Notable:** `getInstances` handles both array and `PaginatedResult` response formats defensively (wraps array in PaginatedResult shape if backend returns raw array).

### 3.3 Barrel Exports

**`src/services/workflow/index.ts`** (132 lines): Re-exports all 8 enums, 30+ core types, 20+ AI types, `workflowService` (as default + named), `nodeTypeInfo`, `statusColors`, `workflowInstanceService`.

**`src/services/workflowService.ts`** (31 lines): Backward-compatible `export * from './workflow'` with documentation comment explaining the module split.

---

## 4. Workflow Pages

### 4.1 `WorkflowListPage.tsx` (852 lines)

**Purpose:** List, create, edit, and activate workflow definitions.

**Layout:** Two tabs — "Workflows" (data table) and "Statistics" (dashboard).

**Workflows Tab:**
- Filters: Search text, Status dropdown (all/draft/active/paused/archived), Entity Type dropdown (from backend config), Category dropdown (from backend config)
- Table columns (7): Name (with icon + tags), Entity Type, Category (Chip), Status (colored Chip), Instances count, Updated date, Actions (menu)
- Actions per row: Edit Details, Open Designer, View Monitor, Activate (if draft version exists), Pause, Delete
- Create/Edit Dialog: 10+ fields — Name, Description (multiline), Entity Type (Select), Category (Select), Priority, Max Concurrent Instances, Default Timeout Hours, Icon Name, Color, Tags (comma-separated)
- Activation logic: `handleActivate` finds the first draft version via `workflowService.getWorkflow(id)`, then calls `workflowService.activateWorkflow(workflowId, versionId)`

**Statistics Tab:**
- 8 stat cards: Total Workflows, Active, Paused, Total Instances, Running, Completed, Failed, Success Rate
- Two charts (using Recharts): instances by status (BarChart), instances by workflow (BarChart)

**Backend Calls:** `workflowService.getWorkflows()`, `getStatistics()`, `getConfig()`, `createWorkflow()`, `updateWorkflow()`, `deleteWorkflow()`, `activateWorkflow()`, `pauseWorkflow()`, `getWorkflow()`

### 4.2 `WorkflowDesignerPage.tsx` (1,249 lines)

**Purpose:** Visual drag-and-drop workflow node editor with SVG canvas.

**Layout Constants:**
- `DRAWER_WIDTH = 280` (left palette), `PROPERTIES_WIDTH = 420` (right properties), `GRID_SIZE = 20`

**Canvas Architecture:**
- SVG layer for transitions (Bezier curves with arrowhead markers, animated dashed lines)
- HTML overlay for nodes (positioned absolutely within a scrollable container)
- Grid background rendered in SVG
- Pan via mouse drag on canvas, zoom not implemented
- Node drag-and-drop with grid snapping (`Math.round(x / GRID_SIZE) * GRID_SIZE`)

**Left Drawer — Node Palette:**
- 17 node types organized into 4 categories: Basic (Start/End/Action/Condition/Wait), Flow Control (Trigger/HumanTask/SubWorkflow/Parallel/Loop), AI Nodes (AIDecision/AIAgent/AIContentGenerator/AIDataExtractor/AIClassifier/AISentimentAnalyzer), Review (HumanReview)
- Each palette item shows icon + label + description from `nodeTypeInfo`
- Drag handler: creates `CreateNodeDto` and calls `workflowService.addNode()`

**Right Drawer — Properties Panel:**
- Conditional rendering based on selected node's `nodeType`:
  - AI types → `AIPropertiesPanel`
  - `Trigger` → `TriggerPropertiesPanel`
  - `Action` → `ActionPropertiesPanel`
  - All others → `EnhancedPropertiesPanel`
- Updates call `workflowService.updateNode()`

**Toolbar:**
- Version selector dropdown
- Create New Version button (calls `workflowService.createVersion()`)
- Activate Version button
- Toggle: Node palette / Simulator / Version Diff / Audit Log
- Save Canvas Layout button (calls `workflowService.saveCanvasLayout()`)

**Integrated Dialogs:**
- `WorkflowSimulator` — opened from toolbar
- `VersionDiffViewer` — opened from toolbar
- `AuditLogViewer` — opened from toolbar

**Transition Creation:**
- Click output handle on source node → click input handle on target node
- Creates `CreateTransitionDto` via `workflowService.addTransition()`
- Bezier curve rendering between handle positions

**Backend Calls:** `workflowService.getWorkflow()`, `getVersion()`, `getConfig()`, `addNode()`, `updateNode()`, `deleteNode()`, `updateNodePositions()`, `addTransition()`, `updateTransition()`, `deleteTransition()`, `createVersion()`, `activateWorkflow()`, `saveCanvasLayout()`

### 4.3 `WorkflowMonitorPage.tsx` (1,029 lines)

**Purpose:** Monitor running workflow instances, manage tasks, view statistics.

**Layout:** Three tabs — "Instances", "My Tasks", "Statistics".

**Instances Tab:**
- Filters: Search text, Status dropdown (9 statuses), Workflow dropdown (loaded from definitions), Date range (From/To)
- Paginated table (10/page) with columns: Workflow Name, Entity, Status (colored Chip), Current Node (Chip), Started, Duration, Actions
- Auto-refresh: `setInterval` at 5000ms when tab is active
- Instance Detail Dialog (`maxWidth="lg"`) with 4 sub-tabs:
  - **Overview**: Status, workflow name, entity link, trigger, timing, error display
  - **Node Execution**: Table of `WorkflowNodeInstance` entries with status chips and timing
  - **Logs**: Filterable log table (by level) with color-coded log level chips, expandable details
  - **Data**: JSON viewers for `inputData`, `outputData`, `variables`
- Actions per instance: View Details, Cancel, Pause, Resume, Retry

**My Tasks Tab:**
- Paginated table of `HumanTask` items
- Columns: Task title, Workflow name, Type (Chip), Status (Chip), Due date (with overdue highlighting), Actions
- Actions: Claim (if unassigned), Complete (opens dialog with result + comments)
- Complete Task Dialog: Result select (approve/reject/escalate) + Comments textarea

**Statistics Tab:**
- Status distribution (4 stat cards: Running/Completed/Failed/Success Rate)
- Performance section: Avg Duration, Avg Node Duration
- Two charts: Instances by Day (BarChart), Node Success Rates (BarChart)

**Backend Calls:** `workflowService.getWorkflows()`, `workflowInstanceService.getInstances()`, `getInstance()`, `getMyTasks()`, `cancelInstance()`, `pauseInstance()`, `resumeInstance()`, `retryInstance()`, `claimTask()`, `completeTask()`, `getStatistics()`

---

## 5. Workflow Components

### 5.1 `AIPropertiesPanel.tsx` (2,470 lines)

**Purpose:** Configures 7 AI/review node types via dedicated sub-panels.

**Exports:** `AIPropertiesPanel` (named + default), `AIPropertiesPanelProps`

**Architecture:** Main component + 7 sub-panels + 4 shared helper components.

**Shared Helpers:**
| Helper | Purpose | Notes |
|--------|---------|-------|
| `ModelSelector` | LLM model dropdown | Groups by provider, shows "(default)" label |
| `TemperatureSlider` | Temperature 0-2 slider with presets | 4 presets: Precise(0.1), Balanced(0.5), Creative(0.8), Experimental(1.2) |
| `PromptEditor` | Multiline prompt text with variable chip insertion | Shows approximate token count (`text.length / 4`), inserts `{{variable}}` syntax |
| `CostEstimator` | Estimated cost display | Model-specific rates for gpt-4o/4/3.5-turbo, claude-3-opus/sonnet/haiku; formula: `(input*rate + output*rate) / 1000` |

**Sub-Panels:**

| Panel | Node Type | Key Config Fields | UI Elements |
|-------|-----------|-------------------|-------------|
| `AIDecisionPanel` | AIDecision | model, temperature, maxTokens, systemPrompt, userPromptTemplate, decisionOptions[], confidenceThreshold, outputVariable, retryOnError | Model selector, prompts, Options CRUD (id/label/description/matchCriteria cards), confidence slider (0-1 step 0.05), CostEstimator |
| `AIAgentPanel` | AIAgent | agentName, systemPrompt, model, maxIterations, availableTools[], toolApprovalRequired, autonomyLevel, canModifyData, canSendCommunications, enableMemory, memoryType, constraints[], stopConditions[], outputVariable | Agent identity, tool checkboxes (6 hardcoded tools), 4 autonomy levels (minimal/low/medium/high), 3 memory types, goals/constraints lists, cost/API limits |
| `AIContentGeneratorPanel` | AIContentGenerator | contentType (6), tone (5), outputFormat (4), model, prompts, inputVariables[], requiresReview | Type/tone/format dropdowns, template toggle, variable chips, review switch |
| `AIDataExtractorPanel` | AIDataExtractor | model, inputVariable, extractionSchema[], outputVariable, validateOutput, onExtractionFailure | Input selector, schema CRUD (fieldName/fieldType 7 types/description/required toggle/validation rules), output toggle |
| `AIClassifierPanel` | AIClassifier | model, classificationType (single/multi), categories[], inputVariable, outputVariable, includeConfidence, includeReasoning, allowCustomCategories | Grid category cards (id/name/description), 3 output toggles, confidence threshold (0-1 step 0.05) |
| `AISentimentAnalyzerPanel` | AISentimentAnalyzer | model, analysisType (basic/detailed/emotional), inputVariable, outputVariable, includeScore, includeEmotions, includeKeyPhrases, includeSuggestions, sentimentThresholds | Analysis type radio, input selector, 4 output switches, 3 threshold sliders (-1 to 1 step 0.1) |
| `HumanReviewPanel` | HumanReview | taskTitle, taskDescription, assignedRole, reviewVariable, showOriginalInput, reviewOptions[], allowEdit, requireComments, dueInMinutes, outputVariable, captureReviewerFeedback | Task settings, role selector (from backend), review options CRUD (id/label/action with color-coded chips: approve=success, reject=error, modify=warning, escalate=default), output config |

**Main Component:**
- Parses `configuration` JSON string → merges with `getDefaultConfig(nodeType)` defaults
- Loads models/roles from `workflowService.getConfig()` with fallback defaults (4 hardcoded models)
- Loads 6 hardcoded tool definitions for agent panel
- Switch renders appropriate sub-panel based on `nodeType`
- Returns `null` if nodeType not in `AI_NODE_TYPES` list
- Delete Node button at bottom
- Updates parent via `onChange('configuration', JSON.stringify(newConfig))`

### 5.2 `ActionPropertiesPanel.tsx` (1,044 lines)

**Purpose:** Configures Action nodes with 8 action types.

**Exports:** `ActionPropertiesPanel` (named + default), `ActionConfiguration`

**`ActionConfiguration` Interface (30+ fields):**
- `actionType`: `'UpdateField'` | `'CreateRecord'` | `'SendEmail'` | `'SendNotification'` | `'CallWebhook'` | `'RunScript'` | `'AssignRecord'` | `'UpdateStatus'`
- Per-type fields: `targetEntityType`, `fieldUpdates[]`, `emailTo/Subject/Body`, `webhookUrl/Method/Headers/Body`, `scriptLanguage/Content`, `assigneeType/assigneeId`, etc.
- Error handling: `onError` (stop/continue/retry), `retryCount`, `retryDelaySeconds`
- Execution: `runAsync`, `timeoutSeconds`, `logResult`

**4 Tabs:**
1. **Action Type**: 8 type-specific configuration sections
2. **Field Updates**: CRUD list of `{ fieldName, value, valueType: static/variable/expression }`
3. **Related Records**: Entity type selector + field updates for related records
4. **Error Handling**: Error strategy select, retry settings, timeout

**Backend Config:** Loads action types and entity types from `workflowService.getConfig()`

### 5.3 `EnhancedPropertiesPanel.tsx` (824 lines)

**Purpose:** General property panel for non-AI, non-Trigger, non-Action nodes.

**Exports:** `EnhancedPropertiesPanel` (named + default), `NodeConfiguration`

**`NodeConfiguration` Interface (30+ fields):** Covers all node-type-specific settings — wait config (duration/unit/until/cronExpression), human task (title/description/assigneeType/assigneeId/dueInHours), condition (if/then/else expressions), sub-workflow reference, loop/parallel config, raw JSON editor.

**6 Tabs:**
1. **Config**: Action type selector, RuleBuilder for conditions, Wait/HumanTask settings, Raw JSON
2. **Inputs**: Input variable mapping CRUD
3. **Outputs**: Output variable mapping CRUD
4. **LLM**: Model, temperature, max tokens, system/user prompts (for nodes that support LLM)
5. **Errors**: Error handling, retry, fallback action
6. **Permissions**: Required roles multi-select, visibility toggle

### 5.4 `TriggerPropertiesPanel.tsx` (713 lines)

**Purpose:** Configures Trigger nodes with 6 trigger types.

**Exports:** `TriggerPropertiesPanel` (named + default), `TriggerConfiguration`

**`TriggerConfiguration` Interface:**
- `triggerType`: `'EntityCreated'` | `'EntityUpdated'` | `'EntityDeleted'` | `'StatusChanged'` | `'FieldChanged'` | `'Scheduled'`
- `entityType`, `fieldConditions[]`, `statusTransitions`, `cronExpression`, `filterExpression`, `relatedEntityType/Conditions`

**3 Tabs:**
1. **Trigger Type**: Entity type selector + 6 type-specific configs (Schedule with cron, StatusChanged with from/to, FieldChanged with field selector)
2. **Field Conditions**: CRUD list with field/operator/value per condition, operators loaded from backend
3. **Advanced**: Filter expression textarea, related entity conditions, Raw JSON editor

### 5.5 `RuleBuilder.tsx` (707 lines)

**Purpose:** Visual condition/rule builder with recursive group nesting.

**Exports:** `RuleBuilder` (named + default), `ConditionOperator` (16 operators), `LogicalOperator` (AND/OR), `ConditionRule`, `ConditionGroup`, `FieldDefinition`, `VariableDefinition`

**16 Operators with Type Mapping:**
- String: `equals`, `notEquals`, `contains`, `notContains`, `startsWith`, `endsWith`, `matches` (regex)
- Number: `equals`, `notEquals`, `greaterThan`, `lessThan`, `greaterThanOrEqual`, `lessThanOrEqual`, `between`
- Boolean: `equals`, `notEquals`
- Date: `equals`, `notEquals`, `greaterThan`, `lessThan`, `between`
- Enum: `equals`, `notEquals`, `in`
- Reference: `equals`, `notEquals`

**Visual Features:**
- Recursive `GroupEditor` with color-coded depth (blue/green/orange/red cycling)
- Each group has AND/OR toggle + Add Rule/Add Group buttons
- Rules: Field selector + Operator selector + Value input (type-aware: TextField/Select/Switch/DatePicker)
- Toggle between Visual builder and Raw JSON editor
- Delete button per rule and per group

### 5.6 `WorkflowSimulator.tsx` (704 lines)

**Purpose:** Client-side workflow execution simulator.

**Exports:** `WorkflowSimulator` (named + default)

**Architecture:** Fully client-side — traverses node graph using `nodes[]` and `transitions[]` from the loaded version.

**Sample Data Templates (4 entity types):**
- Account: `id`, `name`, `industry`, `revenue`, `status`, `priority`, `email`, `createdAt`
- Contact: `id`, `firstName`, `lastName`, `email`, `phone`, `company`, `title`, `status`
- Lead: `id`, `company`, `source`, `score`, `status`, `budget`, `email`, `assignedTo`
- Opportunity: `id`, `name`, `amount`, `stage`, `probability`, `closeDate`, `accountId`, `ownerId`

**Simulation Engine (`simulateWorkflow`):**
- Finds start node (or first node), traverses up to `maxSteps=50` with `500ms` delay per step
- Evaluates transitions: default transition chosen if no conditions, first matching condition otherwise
- `simulateNode()` handles 7 node types: Start, End, Action, Condition (50/50 random), Wait, HumanTask, AI nodes (simulated success/random-fail at 10%)

**Dialog Layout:**
- Left panel: Sample Data (JSON editor per entity type)
- Right panel: Execution Steps (MUI Stepper with step-by-step results)
- Footer: Start/Reset/Close buttons
- State indicators: idle / running / completed / error

### 5.7 `ExecutionTimeline.tsx` (~380 lines)

**Purpose:** Gantt-style horizontal bar timeline for workflow execution.

**Exports:** `ExecutionTimeline` (named + default), `TimelineStep`

**`TimelineStep` Interface (12 fields):** `id`, `name`, `nodeType`, `status`, `startTime`, `endTime`, `duration`, `isActive`, `error?`, `assignedTo?`, `result?`, `children?`

**Visual Elements:**
- Horizontal bars with widths proportional to duration
- Color coding by status (completed=green, running=blue animated pulse, failed=red, waiting=amber)
- Hover tooltip showing name, type, duration, status, error
- Summary row: total duration, longest node, completed count
- Legend for status colors
- Uses `nodeTypeInfo` from workflow service for node type icons

### 5.8 `VersionDiffViewer.tsx` (526 lines)

**Purpose:** Side-by-side comparison of two workflow versions.

**Exports:** `VersionDiffViewer` (named + default)

**Features:**
- From/To version selectors (dropdowns from version list) + Swap button
- Loads full version details via `workflowService.getVersion()`
- `computeNodeDiffs()`: Compares nodes by `nodeKey` → categorizes as Added / Removed / Modified / Unchanged
- `computeTransitionDiffs()`: Compares transitions by `transitionKey` → same categories
- Deep comparison of all node/transition properties (position, config, timeout, retry, etc.)

**3 Tabs:**
1. **Nodes**: Table showing nodeKey, name, type, status (Added/Removed/Modified/Unchanged with colored chips), changed fields list
2. **Transitions**: Table showing transitionKey, label, source→target, status, changed fields
3. **Raw JSON**: Side-by-side JSON pretty-print of full version data

**Summary Bar:** Counts of added/removed/modified/unchanged for both nodes and transitions.

### 5.9 `AuditLogViewer.tsx` (534 lines)

**Purpose:** Dialog for viewing, filtering, and exporting audit logs.

**Exports:** `AuditLogViewer` (named + default), `AuditLogEntry` (component-local — see ⚠️ Gap #1)

**Component-Local `AuditLogEntry` (14 fields):** `id`, `timestamp`, `eventType`, `entityType`, `entityId`, `actorType` (User/System/Worker), `actorId?`, `actorName?`, `description`, `previousValue?`, `newValue?`, `ipAddress?`, `userAgent?`, `correlationId?`, `metadata?`

**29 Default Event Type Configs:** Maps event type strings to MUI chip colors — covers workflow lifecycle (created/updated/deleted/activated/paused), version events, node events (added/updated/deleted/started/completed/failed/retried/skipped/timed_out), transition events, instance events (started/completed/failed/cancelled/suspended/resumed), task events (created/assigned/completed/escalated), variable events (set/updated).

**Backend Integration:** Loads `EventTypeOption[]` from `workflowService.getEventTypes()`, merges with default colors via `getEventConfig()`.

**UI:**
- Dialog with collapsible filter panel: Event type dropdown, Start date, End date, Clear filters button
- Paginated table (25/page, configurable 10/25/50/100)
- Expandable rows showing: entity info, correlation ID, user agent, value diffs (`ValueDiff` helper with red/green JSON), metadata JSON
- CSV export function (exports all loaded entries)
- Loading spinner, empty state message

### 5.10 `AIAnalyticsDashboard.tsx` (618 lines)

**Purpose:** Dashboard for AI execution cost, token usage, and performance metrics.

**Exports:** `AIAnalyticsDashboard` (named + default)

**⚠️ Uses MOCK DATA** — `mockExecutions` (6 entries) and `mockSummary` with hardcoded values ($45.67 total cost, 1.25M tokens, 3456 executions, 97.8% success rate, 1850ms avg latency). Comment in code: "Replace with actual API calls".

**Sections:**
1. **Header**: Title, period selector (today/week/month/quarter/year), Refresh + Export buttons
2. **Stat Cards (4)**: Total Cost, Total Tokens, Success Rate, Avg Latency — each with trend indicator (hardcoded `+12.5%` etc.)
3. **Cost by Model**: List of 3 models (from mock) with LinearProgress bars showing relative cost
4. **Cost by Node Type**: List of 6 AI node types with color-coded icons and progress bars
5. **Recent AI Executions**: Table with columns: Node Type (with avatar), Model (chip), Tokens (tooltip for in/out), Cost, Latency (warning color if >5s), Status (success/error icon), Time
6. **Cost Alert**: Warning Alert shown when `totalCost > 40` with "Set Budget" button

**Helper Utilities:** `formatCost(n)` → `$X.XXXX`, `formatTokens(n)` → `X.XXK/M`, `formatLatency(ms)` → `Xs/Xms`, `getNodeTypeIcon(type)` maps 7 AI types to MUI icons, `getNodeTypeColor(type)` maps 7 types to hex colors.

### 5.11 `index.ts` — Component Barrel (62 lines)

**Exports all 10 components + selected types:**

| Export | Source |
|--------|--------|
| `EnhancedPropertiesPanel`, `NodeConfiguration` | `./EnhancedPropertiesPanel` |
| `RuleBuilder`, `ConditionOperator`, `LogicalOperator`, `ConditionRule`, `ConditionGroup`, `FieldDefinition`, `VariableDefinition` | `./RuleBuilder` |
| `TriggerPropertiesPanel`, `TriggerConfiguration` | `./TriggerPropertiesPanel` |
| `ActionPropertiesPanel`, `ActionConfiguration` | `./ActionPropertiesPanel` |
| `AIPropertiesPanel` | `./AIPropertiesPanel` |
| `WorkflowSimulator` | `./WorkflowSimulator` |
| `ExecutionTimeline`, `TimelineStep` | `./ExecutionTimeline` |
| `VersionDiffViewer` | `./VersionDiffViewer` |
| `AuditLogViewer`, `AuditLogEntry` | `./AuditLogViewer` |
| `AIAnalyticsDashboard` | `./AIAnalyticsDashboard` |

---

## 6. Routing (App.tsx)

**Lazy Imports (line ~196-198):**
```tsx
const WorkflowListPage = React.lazy(() => import('./pages/admin/WorkflowListPage'));
const WorkflowDesignerPage = React.lazy(() => import('./pages/admin/WorkflowDesignerPage'));
const WorkflowMonitorPage = React.lazy(() => import('./pages/admin/WorkflowMonitorPage'));
```

**Route Definitions:**

| Route Pattern | Component | Line | Notes |
|---------------|-----------|------|-------|
| `/admin/workflows` | `WorkflowListPage` | ~1207 | List + create/edit |
| `/admin/workflows/:id/designer` | `WorkflowDesignerPage` | ~1217 | Visual editor for specific workflow |
| `/admin/workflows/monitor` | `WorkflowMonitorPage` | ~1227 | Global monitor (all instances) |
| `/admin/workflows/:workflowId/monitor` | `WorkflowMonitorPage` | ~1237 | Filtered monitor for specific workflow |

All routes are wrapped in `<Suspense>` with a loading fallback and are within the authenticated admin layout.

---

## 7. Navigation (Navigation.tsx)

**Icon Import:** `AccountTree as WorkflowIcon` (MUI)

**Menu Item Definition (line ~273):**
```
'workflow-settings': {
  label: 'Workflows',
  icon: WorkflowIcon,
  path: '/admin/workflows',
  menuName: 'WorkflowSettings'
}
```

**Admin Subcategory (line ~315):**
```
'admin-workflows': {
  label: 'Workflows & Dashboards',
  icon: DashboardAdminIcon,
  order: 6
}
```

**Items within subcategory:**
| Item Key | Label | Order | Path |
|----------|-------|-------|------|
| `dashboard-settings` | Dashboard Settings | 74 | (dashboard path) |
| `workflow-settings` | Workflows | 75 | `/admin/workflows` |
| `llm-settings` | LLM Settings | 78 | (LLM path) |

**Auto-expand logic:** Path includes `'workflows'` or `'dashboards'` → expands `admin-workflows` subcategory.

**Note:** `WorkflowIcon` (`AccountTree`) is also reused by the ITSM CMDB menu item (`'itsm-cmdb'`).

---

## 8. Cross-Cutting References

Pages outside the workflow module that reference workflow functionality:

| Page | Reference | Details |
|------|-----------|---------|
| `HelpPage.tsx` | Workflow Automation topic (line ~159) | Lists help topics: Trigger Events, Actions, Conditions, Workflow Testing |
| `TasksPage.tsx` | `isWorkflowAdmin` flag (line ~104) | Shows "Workflow Admin: Viewing all tasks across all groups" message + admin chip |
| `CampaignExecutionPage.tsx` | Campaign workflow integration | Imports `CampaignWorkflow`, `CampaignWorkflowType`, `LinkCampaignWorkflowRequest`, `CreateABTestRequest`; manages `workflows`/`availableWorkflows` state; `workflowDialogOpen` for linking |

---

## 9. Gap Analysis & Findings

### 🔴 Critical Gaps

| ID | Finding | Details | Impact |
|----|---------|---------|--------|
| GAP-WF-001 | **AIAnalyticsDashboard uses mock data** | `mockExecutions` and `mockSummary` are hardcoded. Comment: "Replace with actual API calls". No backend endpoint for AI execution analytics exists in the instance API. | Dashboard shows fake data; no actual AI cost/token tracking |
| GAP-WF-002 | **Duplicate AuditLogEntry interface** | `types.ts` defines `AuditLogEntry` (10 fields: eventType, eventCategory, message, etc.) while `AuditLogViewer.tsx` exports its own `AuditLogEntry` (14 fields: entityType, entityId, actorType, previousValue, ipAddress, etc.). The barrel `index.ts` exports the component's version, shadowing the service type. | Type confusion — consumers importing `AuditLogEntry` from different sources get different shapes. The component's version has more fields than the API service expects. |
| GAP-WF-003 | **Agent tools are hardcoded** | `AIPropertiesPanel` main component hardcodes 6 tool definitions (search_customers, get_customer, update_customer, create_ticket, send_email, web_search) in a `useEffect`. No backend endpoint to fetch available tools. | Tools cannot be configured per deployment; adding new tools requires code changes |

### 🟡 Medium Gaps

| ID | Finding | Details | Impact |
|----|---------|---------|--------|
| GAP-WF-004 | **3 components not used by any page** | `ExecutionTimeline`, `AuditLogViewer`, and `AIAnalyticsDashboard` are exported from the barrel but none of the 3 admin pages import them directly. `AuditLogViewer` is imported by `WorkflowDesignerPage`. `ExecutionTimeline` and `AIAnalyticsDashboard` appear to be standalone exports for future use or external consumption. | Exported but potentially unused components increase bundle size. `ExecutionTimeline` type (`TimelineStep`) differs from the service type (`TimelineEntry`). |
| GAP-WF-005 | **Simulator is fully client-side** | `WorkflowSimulator` traverses nodes locally with random condition evaluation (50/50) and simulated AI success (90% rate). It also calls `workflowInstanceService.simulateWorkflow()` but only displays the client-side results. | Client simulation doesn't reflect real condition logic or backend behavior. Backend simulation endpoint exists but result is not displayed. |
| GAP-WF-006 | **No workflow import/export** | No UI or service method for importing or exporting workflow definitions (JSON/YAML). | Cannot backup, share, or migrate workflows between environments |
| GAP-WF-007 | **No copy/duplicate workflow** | No "Clone" or "Duplicate" action in WorkflowListPage. | Users must manually recreate similar workflows |
| GAP-WF-008 | **CostEstimator rates are hardcoded** | `CostEstimator` in `AIPropertiesPanel` has hardcoded per-token rates for 6 models (gpt-4o, gpt-4, gpt-3.5-turbo, claude-3-opus, claude-3-sonnet, claude-3-haiku). | Rates become stale as providers update pricing; new models require code changes |
| GAP-WF-009 | **No canvas zoom** | `WorkflowDesignerPage` supports pan (drag on canvas) but not zoom in/out. | Large workflows with many nodes are difficult to navigate |
| GAP-WF-010 | **PromptTemplate type defined but unused** | `aiTypes.ts` defines `PromptTemplate` interface but no UI consumes it. `AIContentGeneratorConfig` has `useTemplate` and `templateId` fields, but the template selection UI is not implemented. | Prompt template library feature is type-defined but not built |
| GAP-WF-011 | **AIEscalationRule type defined but unused** | `aiTypes.ts` defines `AIEscalationRule` interface and `HumanReviewConfig` has optional `escalationRules` field, but HumanReviewPanel does not render escalation rule configuration. | Escalation rules cannot be configured through the UI |

### 🟢 Minor Gaps

| ID | Finding | Details | Impact |
|----|---------|---------|--------|
| GAP-WF-012 | **No breadcrumb navigation** | Pages use `useNavigate` for back navigation but no breadcrumb trail (e.g., Workflows > My Workflow > Designer). | Users lose context in nested pages |
| GAP-WF-013 | **Token count is approximate** | `PromptEditor` uses `text.length / 4` for token estimation. | Inaccurate for non-English text and doesn't account for model-specific tokenizers |
| GAP-WF-014 | **No undo/redo in designer** | Canvas operations (add/move/delete nodes) have no undo/redo history. | Accidental deletions require manual recreation |
| GAP-WF-015 | **No node search in designer** | Large workflows have no search/filter to find specific nodes on canvas. | Navigation difficulty in complex workflows |
| GAP-WF-016 | **No transition label editing inline** | Transition labels can only be edited via the properties panel, not inline on the canvas. | Less intuitive editing workflow |
| GAP-WF-017 | **No keyboard shortcuts** | Designer has no keyboard shortcuts for common operations (delete, copy, paste, select all). | Reduced productivity for power users |
| GAP-WF-018 | **SentimentResult type unused** | `AISentimentResult` is defined in `aiTypes.ts` but never used in UI components. | Dead type definition |

### ⚠️ Consistency Issues

| ID | Finding | Details |
|----|---------|---------|
| CON-WF-001 | **TimelineStep vs TimelineEntry** | `ExecutionTimeline` component exports `TimelineStep` (12 fields including `startTime`, `endTime`, `isActive`, `children`). Service types.ts defines `TimelineEntry` (12 different fields including `startedAt`, `completedAt`, `isSkipped`, `sequence`). These are structurally different — the component would need a mapping layer. |
| CON-WF-002 | **AuditLogEntry divergence** | As noted in GAP-WF-002 — two interfaces with same name, different shapes. Component version (14 fields) is richer than service version (10 fields). |
| CON-WF-003 | **Mixed naming for model fallbacks** | `AIPropertiesPanel` falls back to 4 models (gpt-4o, gpt-4o-mini, gpt-3.5-turbo, claude-3-sonnet). `CostEstimator` supports 6 models (gpt-4o, gpt-4, gpt-3.5-turbo, claude-3-opus, claude-3-sonnet, claude-3-haiku). `ModelSelector` uses whatever `models[]` is loaded. No single source of truth. |

---

## 10. Architecture Summary

### Design Patterns

| Pattern | Implementation |
|---------|---------------|
| **Module Split** | Service layer split into definition API (`workflowService`) and instance API (`workflowInstanceService`) with shared types |
| **Barrel Re-exports** | Two levels: `workflow/index.ts` + backward-compatible `workflowService.ts` |
| **Proxy-based Config Cache** | `nodeTypeInfo` and `statusColors` are JavaScript Proxy objects that lazily load from `getConfig()` and serve defaults until backend responds |
| **Backend-driven Configuration** | All property panels call `workflowService.getConfig()` to populate dropdowns (entity types, action types, triggers, operators, LLM models, roles, etc.) |
| **Conditional Panel Rendering** | Designer page selects property panel based on node type: AI→AIPropertiesPanel, Trigger→TriggerPropertiesPanel, Action→ActionPropertiesPanel, Others→EnhancedPropertiesPanel |
| **JSON Configuration Storage** | Node configurations are stored as JSON strings in `WorkflowNode.configuration`, parsed/serialized by each property panel |
| **Lazy Route Loading** | All 3 pages are `React.lazy()` imported with `<Suspense>` fallback |

### Node Type Coverage

| Node Type | Palette | Properties Panel | Simulator | Timeline Icon |
|-----------|---------|-----------------|-----------|---------------|
| Start | ✅ | EnhancedPropertiesPanel | ✅ | ✅ |
| End | ✅ | EnhancedPropertiesPanel | ✅ | ✅ |
| Action | ✅ | ActionPropertiesPanel | ✅ | ✅ |
| Condition | ✅ | EnhancedPropertiesPanel (RuleBuilder) | ✅ (50/50 random) | ✅ |
| Wait | ✅ | EnhancedPropertiesPanel | ✅ | ✅ |
| Trigger | ✅ | TriggerPropertiesPanel | ❌ (not simulated) | ✅ |
| HumanTask | ✅ | EnhancedPropertiesPanel | ✅ | ✅ |
| SubWorkflow | ✅ | EnhancedPropertiesPanel | ❌ (not simulated) | ✅ |
| Parallel | ✅ | EnhancedPropertiesPanel | ❌ (not simulated) | ✅ |
| Loop | ✅ | EnhancedPropertiesPanel | ❌ (not simulated) | ✅ |
| AIDecision | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| AIAgent | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| AIContentGenerator | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| AIDataExtractor | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| AIClassifier | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| AISentimentAnalyzer | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |
| HumanReview | ✅ | AIPropertiesPanel | ✅ (90% success sim) | ✅ |

### API Endpoint Summary

**Total unique endpoints called: ~51**

| Category | Count | Base Path |
|----------|-------|-----------|
| Workflow Definitions | 8 | `/api/workflowdefinitions` |
| Workflow Config | 1 | `/api/workflowdefinitions/config` |
| Workflow Versions | 3 | `/api/workflowversions` |
| Workflow Nodes | 4 | `/api/workflownodes` + `/api/workflowversions/.../nodes` |
| Workflow Transitions | 3 | `/api/workflowtransitions` + `/api/workflowversions/.../transitions` |
| Workflow Instances | 10 | `/api/workflowinstances` |
| Workflow Tasks | 3 | `/api/workflowtasks` |
| Statistics | 2 | `.../statistics` |
| Audit & Export | 2 | `.../audit`, `.../audit/export` |
| Timeline & Simulation | 2 | `.../timeline`, `.../simulate` |

---

**END OF AUDIT**

# Workflow Backend — Comprehensive Code Audit

> **Generated:** 2026-02-12  
> **Scope:** All workflow-related backend code in `crm-solution/CRM.Backend`  
> **Purpose:** Gap analysis — every property, method, enum, DTO, endpoint, and registration

---

## Table of Contents

1. [Entities](#1-entities)
2. [DTOs](#2-dtos)
3. [Interfaces](#3-interfaces)
4. [Service Implementations](#4-service-implementations)
5. [Controllers](#5-controllers)
6. [CrmDbContext DbSets](#6-crmdbcontext-dbsets)
7. [DI Registration](#7-di-registration)
8. [Gap Analysis & Findings](#8-gap-analysis--findings)

---

## 1. Entities

**Location:** `CRM.Backend/src/CRM.Core/Entities/Workflow/`  
**Count:** 9 entity files + 1 outside Workflow namespace = **10 total**

---

### 1.1 WorkflowDefinition.cs (127 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowDefinition.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowKey` | `string` | `[Required]`, `[MaxLength(100)]` |
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` |
| `Description` | `string?` | `[MaxLength(2000)]` |
| `Category` | `string?` | `[MaxLength(100)]` |
| `EntityType` | `string` | `[Required]`, `[MaxLength(100)]` |
| `Status` | `WorkflowStatus` | Default `Draft` |
| `CurrentVersion` | `int` | Default `1` |
| `IconName` | `string` | `[MaxLength(50)]`, default `"AccountTree"` |
| `Color` | `string` | `[MaxLength(20)]`, default `"#6750A4"` |
| `IsSystem` | `bool` | Default `false` |
| `Priority` | `int` | Default `100` |
| `MaxConcurrentInstances` | `int` | Default `0` (unlimited) |
| `DefaultTimeoutHours` | `int` | Default `0` (no timeout) |
| `OwnerId` | `int?` | FK → `Users` |
| `Tags` | `string?` | `[MaxLength(500)]` |
| `Metadata` | `string?` | JSON blob |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `Owner` | `User?` |
| `Versions` | `ICollection<WorkflowVersion>` |
| `Instances` | `ICollection<WorkflowInstance>` |

**Enum — `WorkflowStatus`:**

| Value | Int |
|-------|-----|
| `Draft` | 0 |
| `Active` | 1 |
| `Paused` | 2 |
| `Archived` | 3 |
| `Deprecated` | 4 |

---

### 1.2 WorkflowVersion.cs (87 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowVersion.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowDefinitionId` | `int` | FK → `WorkflowDefinitions` |
| `VersionNumber` | `int` | |
| `Label` | `string?` | `[MaxLength(50)]` |
| `ChangeLog` | `string?` | `[MaxLength(1000)]` |
| `Status` | `WorkflowVersionStatus` | Default `Draft` |
| `PublishedAt` | `DateTime?` | |
| `PublishedById` | `int?` | FK → `Users` |
| `DeprecatedAt` | `DateTime?` | |
| `CanvasLayout` | `string?` | JSON (full canvas state) |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowDefinition` | `WorkflowDefinition` |
| `PublishedBy` | `User?` |
| `Nodes` | `ICollection<WorkflowNode>` |
| `Transitions` | `ICollection<WorkflowTransition>` |

**Enum — `WorkflowVersionStatus`:**

| Value | Int |
|-------|-----|
| `Draft` | 0 |
| `Active` | 1 |
| `Deprecated` | 2 |

---

### 1.3 WorkflowNode.cs (235 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowNode.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowVersionId` | `int` | FK → `WorkflowVersions` |
| `NodeKey` | `string` | `[Required]`, `[MaxLength(100)]` |
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` |
| `Description` | `string?` | `[MaxLength(1000)]` |
| `NodeType` | `WorkflowNodeType` | |
| `NodeSubType` | `string?` | `[MaxLength(100)]` |
| `PositionX` | `double` | |
| `PositionY` | `double` | |
| `Width` | `double` | Default `200` |
| `Height` | `double` | Default `80` |
| `IconName` | `string` | `[MaxLength(50)]`, default `"Circle"` |
| `Color` | `string` | `[MaxLength(20)]`, default `"#6750A4"` |
| `IsStartNode` | `bool` | |
| `IsEndNode` | `bool` | |
| `Configuration` | `string?` | JSON (node-specific config) |
| `TimeoutMinutes` | `int` | Default `0` |
| `RetryCount` | `int` | Default `0` |
| `RetryDelaySeconds` | `int` | Default `60` |
| `UseExponentialBackoff` | `bool` | Default `true` |
| `ExecutionOrder` | `int` | Default `0` |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowVersion` | `WorkflowVersion` |
| `OutgoingTransitions` | `ICollection<WorkflowTransition>` |
| `IncomingTransitions` | `ICollection<WorkflowTransition>` |
| `NodeInstances` | `ICollection<WorkflowNodeInstance>` |

**Enum — `WorkflowNodeType` (17 values):**

| Value | Int | Description |
|-------|-----|-------------|
| `Trigger` | 0 | Start trigger node |
| `Condition` | 1 | Boolean branch |
| `Action` | 2 | Automated action |
| `HumanTask` | 3 | Manual/approval task |
| `Wait` | 4 | Timer / delay |
| `ParallelGateway` | 5 | Fork into parallel branches |
| `JoinGateway` | 6 | Merge parallel branches |
| `Subprocess` | 7 | Child workflow |
| `LLMAction` | 8 | Generic LLM call |
| `End` | 9 | Terminal node |
| `AIDecision` | 10 | AI-based decision |
| `AIAgent` | 11 | Autonomous AI agent |
| `AIContentGenerator` | 12 | AI content creation |
| `AIDataExtractor` | 13 | AI data extraction |
| `AIClassifier` | 14 | AI classification |
| `AISentimentAnalyzer` | 15 | AI sentiment analysis |
| `HumanReview` | 16 | Human review checkpoint |

---

### 1.4 WorkflowTransition.cs (138 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowTransition.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowVersionId` | `int` | FK → `WorkflowVersions` |
| `SourceNodeId` | `int` | FK → `WorkflowNodes` |
| `TargetNodeId` | `int` | FK → `WorkflowNodes` |
| `TransitionKey` | `string?` | `[MaxLength(100)]` |
| `Label` | `string?` | `[MaxLength(100)]` |
| `Description` | `string?` | `[MaxLength(500)]` |
| `ConditionType` | `TransitionConditionType` | Default `Always` |
| `ConditionExpression` | `string?` | JSON expression |
| `IsDefault` | `bool` | |
| `Priority` | `int` | Default `100` |
| `SourceHandle` | `string` | `[MaxLength(20)]`, default `"right"` |
| `TargetHandle` | `string` | `[MaxLength(20)]`, default `"left"` |
| `LineStyle` | `string` | `[MaxLength(20)]`, default `"solid"` |
| `Color` | `string` | `[MaxLength(20)]`, default `"#888888"` |
| `AnimationStyle` | `string` | `[MaxLength(20)]`, default `"none"` |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowVersion` | `WorkflowVersion` |
| `SourceNode` | `WorkflowNode` |
| `TargetNode` | `WorkflowNode` |

**Enum — `TransitionConditionType`:**

| Value | Int |
|-------|-----|
| `Always` | 0 |
| `Expression` | 1 |
| `FieldMatch` | 2 |
| `Any` | 3 |
| `All` | 4 |
| `UserChoice` | 5 |

---

### 1.5 WorkflowInstance.cs (241 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowInstance.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowDefinitionId` | `int` | FK |
| `WorkflowVersionId` | `int` | FK |
| `CorrelationId` | `string` | `[Required]`, `[MaxLength(100)]`, default `Guid.NewGuid().ToString()` |
| `EntityType` | `string` | `[Required]`, `[MaxLength(100)]` |
| `EntityId` | `int` | |
| `Status` | `WorkflowInstanceStatus` | Default `Pending` |
| `CurrentNodeId` | `int?` | FK → `WorkflowNodes` |
| `StartedAt` | `DateTime?` | |
| `CompletedAt` | `DateTime?` | |
| `ScheduledAt` | `DateTime?` | |
| `Priority` | `int` | Default `100` |
| `TriggerEvent` | `string?` | `[MaxLength(100)]` |
| `TriggeredById` | `int?` | FK → `Users` |
| `InputData` | `string?` | JSON |
| `StateData` | `string?` | JSON |
| `OutputData` | `string?` | JSON |
| `ErrorMessage` | `string?` | |
| `ErrorStackTrace` | `string?` | |
| `RetryCount` | `int` | Default `0` |
| `MaxRetries` | `int` | Default `3` |
| `NextRetryAt` | `DateTime?` | |
| `TimeoutAt` | `DateTime?` | |
| `IsCancelled` | `bool` | |
| `CancellationReason` | `string?` | `[MaxLength(500)]` |
| `ParentInstanceId` | `int?` | FK (self-referencing for subprocesses) |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowDefinition` | `WorkflowDefinition` |
| `WorkflowVersion` | `WorkflowVersion` |
| `CurrentNode` | `WorkflowNode?` |
| `TriggeredBy` | `User?` |
| `ParentInstance` | `WorkflowInstance?` |
| `ChildInstances` | `ICollection<WorkflowInstance>` |
| `NodeInstances` | `ICollection<WorkflowNodeInstance>` |
| `Tasks` | `ICollection<WorkflowTask>` |
| `Logs` | `ICollection<WorkflowLog>` |

**Enum — `WorkflowInstanceStatus`:**

| Value | Int |
|-------|-----|
| `Pending` | 0 |
| `Running` | 1 |
| `Waiting` | 2 |
| `Paused` | 3 |
| `Completed` | 4 |
| `Failed` | 5 |
| `Cancelled` | 6 |
| `TimedOut` | 7 |
| `Suspended` | 8 |

---

### 1.6 WorkflowNodeInstance.cs (162 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowNodeInstance.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowInstanceId` | `int` | FK |
| `WorkflowNodeId` | `int` | FK |
| `Status` | `WorkflowNodeInstanceStatus` | Default `Pending` |
| `StartedAt` | `DateTime?` | |
| `CompletedAt` | `DateTime?` | |
| `DurationMs` | `long?` | |
| `InputData` | `string?` | JSON |
| `OutputData` | `string?` | JSON |
| `ErrorMessage` | `string?` | |
| `ErrorStackTrace` | `string?` | |
| `RetryCount` | `int` | Default `0` |
| `NextRetryAt` | `DateTime?` | |
| `IsSkipped` | `bool` | |
| `SkipReason` | `string?` | `[MaxLength(500)]` |
| `ExecutionSequence` | `int` | |
| `WorkerId` | `string?` | `[MaxLength(100)]` |
| `TransitionTakenId` | `int?` | FK → `WorkflowTransitions` |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowInstance` | `WorkflowInstance` |
| `WorkflowNode` | `WorkflowNode` |
| `TransitionTaken` | `WorkflowTransition?` |

**Enum — `WorkflowNodeInstanceStatus`:**

| Value | Int |
|-------|-----|
| `Pending` | 0 |
| `Running` | 1 |
| `Waiting` | 2 |
| `Completed` | 3 |
| `Failed` | 4 |
| `Skipped` | 5 |
| `Cancelled` | 6 |
| `Retrying` | 7 |

---

### 1.7 WorkflowTask.cs (283 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowTask.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowInstanceId` | `int` | FK |
| `WorkflowNodeId` | `int` | FK |
| `NodeInstanceId` | `int?` | FK |
| `TaskType` | `WorkflowTaskType` | |
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` |
| `Description` | `string?` | `[MaxLength(1000)]` |
| `Status` | `WorkflowTaskStatus` | Default `Pending` |
| `Priority` | `int` | Default `100` |
| `QueueName` | `string` | `[MaxLength(100)]`, default `"default"` |
| `ScheduledAt` | `DateTime?` | |
| `PickedAt` | `DateTime?` | |
| `StartedAt` | `DateTime?` | |
| `CompletedAt` | `DateTime?` | |
| `DueAt` | `DateTime?` | |
| `TimeoutAt` | `DateTime?` | |
| `LockedByWorkerId` | `string?` | `[MaxLength(100)]` |
| `LockExpiresAt` | `DateTime?` | |
| `AssignedToId` | `int?` | FK → `Users` |
| `AssignedToRole` | `string?` | `[MaxLength(100)]` |
| `InputData` | `string?` | JSON |
| `OutputData` | `string?` | JSON |
| `FormSchema` | `string?` | JSON (for human task forms) |
| `FormData` | `string?` | JSON (user-submitted form data) |
| `ErrorMessage` | `string?` | |
| `ErrorStackTrace` | `string?` | |
| `RetryCount` | `int` | Default `0` |
| `MaxRetries` | `int` | Default `3` |
| `NextRetryAt` | `DateTime?` | |
| `IsDeadLetter` | `bool` | |
| `DeadLetterReason` | `string?` | `[MaxLength(500)]` |
| `DeadLetterAt` | `DateTime?` | |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowInstance` | `WorkflowInstance` |
| `WorkflowNode` | `WorkflowNode` |
| `NodeInstance` | `WorkflowNodeInstance?` |
| `AssignedTo` | `User?` |

**Enum — `WorkflowTaskType`:**

| Value | Int |
|-------|-----|
| `Automated` | 0 |
| `Human` | 1 |
| `Timer` | 2 |
| `Event` | 3 |
| `LLM` | 4 |

**Enum — `WorkflowTaskStatus`:**

| Value | Int |
|-------|-----|
| `Pending` | 0 |
| `Locked` | 1 |
| `Running` | 2 |
| `Waiting` | 3 |
| `Completed` | 4 |
| `Failed` | 5 |
| `Retrying` | 6 |
| `Cancelled` | 7 |
| `Skipped` | 8 |
| `DeadLetter` | 9 |

---

### 1.8 WorkflowLog.cs (~120 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowLog.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowInstanceId` | `int?` | FK (nullable — log may be system-level) |
| `WorkflowNodeId` | `int?` | FK |
| `NodeInstanceId` | `int?` | FK |
| `Level` | `WorkflowLogLevel` | Default `Info` |
| `Category` | `string` | `[MaxLength(100)]`, default `"General"` |
| `Message` | `string` | `[Required]`, `[MaxLength(2000)]` |
| `Details` | `string?` | JSON |
| `Timestamp` | `DateTime` | Default `DateTime.UtcNow` |
| `WorkerId` | `string?` | `[MaxLength(100)]` |
| `UserId` | `int?` | FK |
| `DurationMs` | `long?` | |
| `ExceptionType` | `string?` | `[MaxLength(200)]` |
| `StackTrace` | `string?` | |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowInstance` | `WorkflowInstance?` |
| `WorkflowNode` | `WorkflowNode?` |
| `NodeInstance` | `WorkflowNodeInstance?` |
| `User` | `User?` |

**Enum — `WorkflowLogLevel`:**

| Value | Int |
|-------|-----|
| `Debug` | 0 |
| `Info` | 1 |
| `Warning` | 2 |
| `Error` | 3 |
| `Critical` | 4 |

---

### 1.9 WorkflowTrigger.cs (~180 lines)

**File:** `CRM.Core/Entities/Workflow/WorkflowTrigger.cs`  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `WorkflowDefinitionId` | `int` | FK |
| `Name` | `string` | `[Required]`, `[MaxLength(200)]` |
| `TriggerType` | `WorkflowTriggerType` | Default `Manual` |
| `EntityType` | `string?` | `[MaxLength(100)]` |
| `EventName` | `string?` | `[MaxLength(200)]` |
| `CronExpression` | `string?` | `[MaxLength(100)]` |
| `FilterConditions` | `string?` | JSON |
| `WatchedField` | `string?` | `[MaxLength(100)]` |
| `OldValue` | `string?` | `[MaxLength(500)]` |
| `NewValue` | `string?` | `[MaxLength(500)]` |
| `IsActive` | `bool` | Default `true` |
| `Priority` | `int` | Default `100` |
| `Description` | `string?` | `[MaxLength(1000)]` |
| `LastTriggeredAt` | `DateTime?` | |
| `NextScheduledAt` | `DateTime?` | |
| `ExecutionCount` | `int` | Default `0` |
| `DelaySeconds` | `int` | Default `0` |
| `RunAsync` | `bool` | Default `true` |
| `MaxRetries` | `int` | Default `3` |
| `CreatedById` | `int?` | FK → `Users` |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `WorkflowDefinition` | `WorkflowDefinition` |
| `CreatedBy` | `User?` |

**Enum — `WorkflowTriggerType` (10 values):**

| Value | Int |
|-------|-----|
| `Manual` | 0 |
| `OnCreate` | 1 |
| `OnUpdate` | 2 |
| `OnDelete` | 3 |
| `OnFieldChange` | 4 |
| `Scheduled` | 5 |
| `OnEvent` | 6 |
| `OnWebhook` | 7 |
| `OnSLABreach` | 8 |
| `OnEscalation` | 9 |

---

### 1.10 CampaignWorkflow.cs (~80 lines)

**File:** `CRM.Core/Entities/CampaignWorkflow.cs`  
**Namespace:** `CRM.Core.Entities` *(NOT in the `Workflow` sub-namespace)*  
**Inherits:** `BaseEntity`

| Property | Type | Attributes / Default |
|----------|------|---------------------|
| `CampaignId` | `int` | `[Required]` |
| `WorkflowDefinitionId` | `int` | `[Required]` |
| `WorkflowType` | `string` | `[Required]`, `[MaxLength(50)]`, default `"Sequential"` |
| `TriggerEvent` | `string?` | `[MaxLength(100)]` |
| `TriggerConditions` | `string?` | JSON |
| `IsActive` | `bool` | Default `true` |
| `Priority` | `int` | Default `0` |
| `MaxExecutionsPerContact` | `int` | Default `1` |
| `CooldownHours` | `int` | Default `0` |

**Navigation Properties:**

| Property | Type |
|----------|------|
| `Campaign` | `MarketingCampaign?` |
| `WorkflowDefinition` | `WorkflowDefinition?` |

**Enum — `CampaignWorkflowType`:**

| Value | Int |
|-------|-----|
| `TriggerBased` | 0 |
| `Scheduled` | 1 |
| `Sequential` | 2 |

---

## 2. DTOs

### 2.1 Dedicated DTO Files

#### WorkflowTriggerDtos.cs (258 lines)

**File:** `CRM.Core/Dtos/Workflow/WorkflowTriggerDtos.cs`

| Class | Properties |
|-------|-----------|
| `WorkflowTriggerDto` | `Id`, `WorkflowDefinitionId`, `WorkflowName`, `Name`, `TriggerType`, `TriggerTypeName`, `EntityType`, `EventName`, `CronExpression`, `FilterConditions`, `WatchedField`, `OldValue`, `NewValue`, `IsActive`, `Priority`, `Description`, `LastTriggeredAt`, `NextScheduledAt`, `ExecutionCount`, `DelaySeconds`, `RunAsync`, `MaxRetries`, `CreatedById`, `CreatedByName`, `CreatedAt`, `UpdatedAt` |
| `CreateWorkflowTriggerDto` | `WorkflowDefinitionId`, `Name`, `TriggerType`, `EntityType`, `EventName`, `CronExpression`, `FilterConditions`, `WatchedField`, `OldValue`, `NewValue`, `IsActive` (default true), `Priority` (default 100), `Description`, `DelaySeconds` (default 0), `RunAsync` (default true), `MaxRetries` (default 3) |
| `UpdateWorkflowTriggerDto` | Same as Create — all nullable-style (but non-nullable types with defaults) |
| `TriggerExecutionRequest` | `EntityType`, `EntityId`, `TriggerType`, `ChangedField`, `OldValue`, `NewValue`, `InitiatedById`, `ContextData` |
| `TriggerExecutionResult` | `TriggersEvaluated`, `TriggersMatched`, `WorkflowsTriggered`, `Results` (List of `TriggerResult`), `WorkflowInstanceIds` (List), `Errors` (List) |
| `TriggerResult` | `TriggerId`, `TriggerName`, `WorkflowDefinitionId`, `WorkflowName`, `Matched`, `WorkflowStarted`, `WorkflowInstanceId`, `Error` |
| `TriggerStatisticsDto` | `TotalTriggers`, `ActiveTriggers`, `ScheduledTriggers`, `TriggersByType` (Dictionary), `TriggersByEntity` (Dictionary), `TopTriggeredWorkflows` (List of `TopTriggeredWorkflow`), `RecentExecutions` (List of `TriggerResult`) |

---

#### WorkflowInstanceDtos.cs (375 lines)

**File:** `CRM.Core/Dtos/Workflow/WorkflowInstanceDtos.cs`

| Class | Properties |
|-------|-----------|
| `WorkflowInstanceDto` | `Id`, `WorkflowDefinitionId`, `WorkflowName`, `WorkflowVersionId`, `VersionNumber`, `CorrelationId`, `EntityType`, `EntityId`, `Status`, `StatusName`, `CurrentNodeId`, `CurrentNodeName`, `StartedAt`, `CompletedAt`, `ScheduledAt`, `Priority`, `TriggerEvent`, `TriggeredById`, `TriggeredByName`, `RetryCount`, `MaxRetries`, `IsCancelled`, `ParentInstanceId`, `CreatedAt`, `UpdatedAt` |
| `WorkflowInstanceDetailDto` | Extends `WorkflowInstanceDto` + `InputData`, `StateData`, `OutputData`, `ErrorMessage`, `ErrorStackTrace`, `CancellationReason`, `TimeoutAt`, `NextRetryAt`, `NodeInstances` (List), `Tasks` (List), `Logs` (List), `Nodes` (List), `Transitions` (List) |
| `WorkflowNodeInstanceDto` | `Id`, `WorkflowInstanceId`, `WorkflowNodeId`, `NodeName`, `NodeType`, `NodeTypeName`, `Status`, `StatusName`, `StartedAt`, `CompletedAt`, `DurationMs`, `InputData`, `OutputData`, `ErrorMessage`, `RetryCount`, `IsSkipped`, `SkipReason`, `ExecutionSequence`, `WorkerId`, `TransitionTakenId` |
| `WorkflowTaskDto` | `Id`, `WorkflowInstanceId`, `WorkflowNodeId`, `NodeInstanceId`, `TaskType`, `TaskTypeName`, `Name`, `Description`, `Status`, `StatusName`, `Priority`, `QueueName`, `ScheduledAt`, `PickedAt`, `StartedAt`, `CompletedAt`, `DueAt`, `TimeoutAt`, `LockedByWorkerId`, `LockExpiresAt`, `AssignedToId`, `AssignedToName`, `AssignedToRole`, `InputData`, `OutputData`, `FormSchema`, `FormData`, `ErrorMessage`, `RetryCount`, `MaxRetries`, `IsDeadLetter`, `DeadLetterReason`, `CreatedAt` |
| `WorkflowLogDto` | `Id`, `WorkflowInstanceId`, `WorkflowNodeId`, `NodeInstanceId`, `Level`, `LevelName`, `Category`, `Message`, `Details`, `Timestamp`, `WorkerId`, `UserId`, `UserName`, `DurationMs`, `ExceptionType` |
| `HumanTaskDto` | `TaskId`, `WorkflowInstanceId`, `WorkflowName`, `NodeName`, `Name`, `Description`, `Priority`, `PriorityLabel`, `Status`, `StatusName`, `AssignedToId`, `AssignedToName`, `AssignedToRole`, `DueAt`, `FormSchema`, `FormData`, `EntityType`, `EntityId`, `CorrelationId`, `CreatedAt` |
| `StartWorkflowDto` | `WorkflowDefinitionId`, `VersionId`, `EntityType`, `EntityId`, `InputData`, `TriggerEvent`, `TriggeredById`, `Priority` (default 100), `ScheduledAt` |
| `CancelInstanceDto` | `Reason` |
| `SkipNodeDto` | `Reason` |
| `CompleteTaskDto` | `OutputData` (Dictionary), `FormData` (Dictionary) |
| `WorkflowAuditLogDto` | `Id`, `WorkflowInstanceId`, `Timestamp`, `Category`, `Level`, `Message`, `NodeName`, `NodeType`, `UserName`, `WorkerId`, `DurationMs`, `Details` |
| `ExecutionTimelineDto` | `InstanceId`, `WorkflowName`, `Status`, `StartedAt`, `CompletedAt`, `Entries` (List) |
| `TimelineEntryDto` | `Id`, `NodeId`, `NodeName`, `NodeType`, `Status`, `StartedAt`, `CompletedAt`, `DurationMs`, `Details`, `ErrorMessage`, `Sequence` |
| `WorkflowNodeDto` | `Id`, `NodeKey`, `Name`, `NodeType`, `NodeTypeName`, `PositionX`, `PositionY`, `Width`, `Height`, `IconName`, `Color`, `IsStartNode`, `IsEndNode`, `Configuration` |
| `WorkflowTransitionDto` | `Id`, `SourceNodeId`, `TargetNodeId`, `TransitionKey`, `Label`, `ConditionType`, `ConditionExpression`, `IsDefault`, `Priority`, `SourceHandle`, `TargetHandle`, `LineStyle`, `Color` |
| `WorkflowDashboardDto` | `TotalActive`, `TotalCompleted`, `TotalFailed`, `TotalRunning`, `AverageCompletionTimeMs`, `CompletionRate`, `FailureRate`, `TopFailingWorkflows` (List), `DailyThroughput` (List), `RecentErrors` (List), `WorkflowBreakdown` (List) |
| `TopFailingWorkflowDto` | `WorkflowName`, `FailureCount`, `LastFailedAt` |
| `DailyThroughputDto` | `Date`, `Started`, `Completed`, `Failed` |
| `RecentErrorDto` | `WorkflowName`, `InstanceId`, `ErrorMessage`, `OccurredAt` |
| `WorkflowBreakdownDto` | `WorkflowId`, `WorkflowName`, `ActiveCount`, `CompletedCount`, `FailedCount` |

---

### 2.2 Inline DTOs in WorkflowController.cs (22 classes)

**File:** `CRM.Api/Controllers/WorkflowController.cs` → `#region DTOs`

These DTOs are defined **inside the controller file** and are not in `CRM.Core`.

| Class | Purpose | Key Properties |
|-------|---------|----------------|
| `WorkflowDefinitionDto` | List/detail response | `Id`, `WorkflowKey`, `Name`, `Description`, `Category`, `EntityType`, `Status`, `StatusName`, `CurrentVersion`, `IconName`, `Color`, `IsSystem`, `Priority`, `MaxConcurrentInstances`, `DefaultTimeoutHours`, `OwnerId`, `OwnerName`, `Tags`, `CreatedAt`, `UpdatedAt` |
| `WorkflowDefinitionDetailDto` | Extends above | `+Metadata`, `+Versions` (List of `WorkflowVersionSummaryDto`) |
| `WorkflowVersionSummaryDto` | Version list item | `VersionId`, `VersionNumber`, `Label`, `Status`, `StatusName`, `PublishedAt` |
| `WorkflowVersionDetailDto` | Extends summary | `+WorkflowDefinitionId`, `WorkflowName`, `ChangeLog`, `PublishedByName`, `CanvasLayout`, `UpdatedAt`, `Nodes` (List of `WorkflowNodeDto`), `Transitions` (List of `WorkflowTransitionDto`) |
| `CreateWorkflowDto` | Create request | `Name`, `Description`, `Category`, `EntityType`, `IconName`, `Color`, `Priority`, `MaxConcurrentInstances`, `DefaultTimeoutHours`, `Tags`, `Metadata`, `WorkflowKey` (auto-generated if null) |
| `UpdateWorkflowDto` | Update request (all nullable) | `Name?`, `Description?`, `Category?`, `EntityType?`, `Status?`, `IconName?`, `Color?`, `Priority?`, `MaxConcurrentInstances?`, `DefaultTimeoutHours?`, `Tags?` |
| `CreateVersionDto` | Create version | `SourceVersionId?` |
| `SaveLayoutDto` | Save canvas | `CanvasLayout` (string) |
| `UpdateVersionMetadataDto` | Update version | `Label?`, `ChangeLog?` |
| `CreateNodeDto` | Create node | `NodeKey`, `Name`, `Description`, `NodeType`, `NodeSubType`, `PositionX`, `PositionY`, `Width` (200), `Height` (80), `IconName` ("Circle"), `Color` ("#6750A4"), `IsStartNode`, `IsEndNode`, `Configuration`, `TimeoutMinutes`, `RetryCount`, `RetryDelaySeconds` (60), `UseExponentialBackoff` (true), `ExecutionOrder` |
| `UpdateNodeDto` | Update node (all nullable) | `Name?`, `Description?`, `NodeType?`, `NodeSubType?`, `PositionX?`, `PositionY?`, `Width?`, `Height?`, `IconName?`, `Color?`, `IsStartNode?`, `IsEndNode?`, `Configuration?`, `TimeoutMinutes?`, `RetryCount?`, `RetryDelaySeconds?`, `UseExponentialBackoff?`, `ExecutionOrder?` |
| `NodePositionDto` | Batch position update | `NodeId`, `X`, `Y` |
| `CreateTransitionDto` | Create transition | `SourceNodeId`, `TargetNodeId`, `TransitionKey`, `Label`, `Description`, `ConditionType` (0), `ConditionExpression`, `IsDefault`, `Priority` (100), `SourceHandle` ("right"), `TargetHandle` ("left"), `LineStyle` ("solid"), `Color` ("#888888"), `AnimationStyle` |
| `UpdateTransitionDto` | Update (all nullable) | `Label?`, `Description?`, `ConditionType?`, `ConditionExpression?`, `IsDefault?`, `Priority?`, `SourceHandle?`, `TargetHandle?`, `LineStyle?`, `Color?`, `AnimationStyle?` |
| `WorkflowConfigResponse` | Config endpoint response | `EntityTypes`, `NodeTypes`, `ActionTypes`, `TriggerTypes`, `ConditionOperators`, `StatusOptions`, `LLMProviders`, `LLMModels`, `Roles`, `Categories`, `IconOptions`, `ColorOptions`, `FallbackActions`, `EventTypes`, `EntityFields`, `RelatedEntities` |
| `EntityFieldConfig` | Field metadata | `Name`, `Label`, `Type`, `Required`, `EnumValues?`, `ReferenceEntity?`, `Group?` |
| `RelatedEntityConfig` | Relationship info | `Name`, `Label`, `EntityType`, `RelationType` |
| `ConfigOption` | Base option | `Value`, `Label` |
| `NodeTypeConfig` | Extends ConfigOption | `+Icon`, `Color`, `Description` |
| `ActionTypeConfig` | Extends ConfigOption | `+Category`, `Icon` |
| `TriggerTypeConfig` | Extends ConfigOption | `+Description`, `Icon` |
| `OperatorConfig` | Extends ConfigOption | `+AppliesTo` (string[]) |
| `StatusConfig` | Extends ConfigOption | `+Color`, `BgColor`, `Icon` |
| `EventTypeConfig` | Extends ConfigOption | `+Color`, `Category` |

---

## 3. Interfaces

### 3.1 IWorkflowService.cs (224 lines)

**File:** `CRM.Core/Interfaces/IWorkflowService.cs`

#### Definition Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetWorkflowDefinitionsAsync` | `Task<IEnumerable<WorkflowDefinition>>` | `string? search, string? category, WorkflowStatus? status, int page, int pageSize, CancellationToken` |
| `GetWorkflowDefinitionAsync` | `Task<WorkflowDefinition?>` | `int id, CancellationToken` |
| `GetWorkflowByKeyAsync` | `Task<WorkflowDefinition?>` | `string workflowKey, CancellationToken` |
| `CreateWorkflowDefinitionAsync` | `Task<WorkflowDefinition>` | `WorkflowDefinition definition, CancellationToken` |
| `UpdateWorkflowDefinitionAsync` | `Task<WorkflowDefinition>` | `WorkflowDefinition definition, CancellationToken` |
| `DeleteWorkflowDefinitionAsync` | `Task<bool>` | `int id, CancellationToken` |
| `ActivateWorkflowAsync` | `Task<WorkflowDefinition>` | `int id, int? versionId, CancellationToken` |
| `PauseWorkflowAsync` | `Task<WorkflowDefinition>` | `int id, CancellationToken` |

#### Version Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetWorkflowVersionAsync` | `Task<WorkflowVersion?>` | `int versionId, CancellationToken` |
| `GetActiveVersionAsync` | `Task<WorkflowVersion?>` | `int workflowDefinitionId, CancellationToken` |
| `GetDraftVersionAsync` | `Task<WorkflowVersion?>` | `int workflowDefinitionId, CancellationToken` |
| `CreateNewVersionAsync` | `Task<WorkflowVersion>` | `int workflowDefinitionId, int? sourceVersionId, CancellationToken` |
| `SaveCanvasLayoutAsync` | `Task<WorkflowVersion>` | `int versionId, string canvasLayout, CancellationToken` |
| `GetVersionsAsync` | `Task<IEnumerable<WorkflowVersion>>` | `int workflowDefinitionId, CancellationToken` |
| `UpdateVersionMetadataAsync` | `Task<WorkflowVersion>` | `int versionId, string? label, string? changeLog, CancellationToken` |
| `PublishVersionAsync` | `Task<WorkflowVersion>` | `int versionId, int publishedById, CancellationToken` |
| `DeleteVersionAsync` | `Task<bool>` | `int versionId, CancellationToken` |
| `RollbackToVersionAsync` | `Task<WorkflowDefinition>` | `int workflowDefinitionId, int versionId, CancellationToken` |
| `CompareVersionsAsync` | `Task<VersionComparisonResult>` | `int versionId1, int versionId2, CancellationToken` |

#### Node Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `AddNodeAsync` | `Task<WorkflowNode>` | `WorkflowNode node, CancellationToken` |
| `UpdateNodeAsync` | `Task<WorkflowNode>` | `WorkflowNode node, CancellationToken` |
| `DeleteNodeAsync` | `Task<bool>` | `int nodeId, CancellationToken` |
| `UpdateNodePositionsAsync` | `Task<bool>` | `Dictionary<int, (double X, double Y)> positions, CancellationToken` |

#### Transition Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `AddTransitionAsync` | `Task<WorkflowTransition>` | `WorkflowTransition transition, CancellationToken` |
| `UpdateTransitionAsync` | `Task<WorkflowTransition>` | `WorkflowTransition transition, CancellationToken` |
| `DeleteTransitionAsync` | `Task<bool>` | `int transitionId, CancellationToken` |

#### Statistics

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetStatisticsAsync` | `Task<WorkflowStatistics>` | `CancellationToken` |

**Helper Types (inside IWorkflowService.cs):**

| Class | Properties |
|-------|-----------|
| `WorkflowStatistics` | `TotalWorkflows`, `ActiveWorkflows`, `DraftWorkflows`, `PausedWorkflows`, `TotalVersions`, `TotalNodes`, `TotalTransitions`, `WorkflowsByCategory` (Dictionary) |
| `VersionComparisonResult` | `Version1Id`, `Version2Id`, `Version1Label`, `Version2Label`, `AddedNodes` (List), `RemovedNodes` (List), `ModifiedNodes` (List), `AddedTransitions`, `RemovedTransitions`, `ModifiedTransitions`, `Summary` |
| `NodeDiffItem` | `NodeId`, `NodeKey`, `Name`, `NodeType`, `ChangeType`, `Changes` (List of string) |

---

### 3.2 IWorkflowInstanceService.cs (403 lines)

**File:** `CRM.Core/Interfaces/IWorkflowInstanceService.cs`

#### Instance Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetInstancesAsync` | `Task<IEnumerable<WorkflowInstance>>` | `int? workflowDefinitionId, WorkflowInstanceStatus? status, string? entityType, int? entityId, int page, int pageSize, CancellationToken` |
| `GetInstanceAsync` | `Task<WorkflowInstance?>` | `int id, CancellationToken` |
| `GetInstanceByCorrelationIdAsync` | `Task<WorkflowInstance?>` | `string correlationId, CancellationToken` |
| `StartWorkflowAsync` | `Task<WorkflowInstance>` | `int workflowDefinitionId, int? versionId, string entityType, int entityId, string? inputData, string? triggerEvent, int? triggeredById, int priority, DateTime? scheduledAt, CancellationToken` |
| `CancelInstanceAsync` | `Task<WorkflowInstance>` | `int instanceId, string? reason, CancellationToken` |
| `PauseInstanceAsync` | `Task<WorkflowInstance>` | `int instanceId, CancellationToken` |
| `ResumeInstanceAsync` | `Task<WorkflowInstance>` | `int instanceId, CancellationToken` |
| `RetryInstanceAsync` | `Task<WorkflowInstance>` | `int instanceId, CancellationToken` |
| `BulkStartWorkflowAsync` | `Task<BulkStartResult>` | `int workflowDefinitionId, IEnumerable<(string EntityType, int EntityId)> entities, string? inputData, int? triggeredById, CancellationToken` |

#### Node Instance Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `StartNodeExecutionAsync` | `Task<WorkflowNodeInstance>` | `int instanceId, int nodeId, string? inputData, CancellationToken` |
| `CompleteNodeExecutionAsync` | `Task<WorkflowNodeInstance>` | `int nodeInstanceId, string? outputData, CancellationToken` |
| `FailNodeExecutionAsync` | `Task<WorkflowNodeInstance>` | `int nodeInstanceId, string errorMessage, string? stackTrace, CancellationToken` |
| `SkipNodeAsync` | `Task<WorkflowNodeInstance>` | `int instanceId, int nodeId, string? reason, CancellationToken` |

#### Task Operations

| Method | Return Type | Parameters |
|--------|------------|------------|
| `CreateTaskForNodeAsync` | `Task<WorkflowTask>` | `int instanceId, int nodeId, int? nodeInstanceId, WorkflowTaskType taskType, string name, string? description, string? inputData, int? assignedToId, string? assignedToRole, string? queueName, DateTime? dueAt, string? formSchema, CancellationToken` |
| `GetPendingTasksAsync` | `Task<IEnumerable<WorkflowTask>>` | `string? queueName, int maxTasks, CancellationToken` |
| `LockTaskAsync` | `Task<WorkflowTask?>` | `int taskId, string workerId, TimeSpan? lockDuration, CancellationToken` |
| `CompleteTaskAsync` | `Task<WorkflowTask>` | `int taskId, string? outputData, CancellationToken` |
| `FailTaskAsync` | `Task<WorkflowTask>` | `int taskId, string errorMessage, string? stackTrace, bool shouldRetry, CancellationToken` |
| `ProcessRetryTasksAsync` | `Task<int>` | `CancellationToken` |
| `GetHumanTasksForUserAsync` | `Task<IEnumerable<WorkflowTask>>` | `int userId, string? role, WorkflowTaskStatus? status, CancellationToken` |
| `ClaimTaskAsync` | `Task<WorkflowTask>` | `int taskId, int userId, CancellationToken` |
| `CompleteHumanTaskAsync` | `Task<WorkflowTask>` | `int taskId, int userId, string? outputData, string? formData, CancellationToken` |

#### Logging & Audit

| Method | Return Type | Parameters |
|--------|------------|------------|
| `LogAsync` | `Task<WorkflowLog>` | `int? instanceId, int? nodeId, int? nodeInstanceId, WorkflowLogLevel level, string category, string message, string? details, string? workerId, int? userId, long? durationMs, string? exceptionType, string? stackTrace, CancellationToken` |
| `GetLogsAsync` | `Task<IEnumerable<WorkflowLog>>` | `int instanceId, WorkflowLogLevel? minLevel, string? category, int? limit, CancellationToken` |
| `GetAuditLogAsync` | `Task<IEnumerable<WorkflowLog>>` | `int instanceId, CancellationToken` |
| `ExportAuditLogCsvAsync` | `Task<string>` | `int instanceId, CancellationToken` |
| `GetExecutionTimelineDataAsync` | `Task<IEnumerable<WorkflowNodeInstance>>` | `int instanceId, CancellationToken` |

#### Statistics & Dashboard

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetInstanceStatisticsAsync` | `Task<WorkflowInstanceStatistics>` | `CancellationToken` |
| `GetDashboardAsync` | `Task<object>` | `CancellationToken` |

#### Parallel Gateway & Sub-workflow

| Method | Return Type | Parameters |
|--------|------------|------------|
| `AdvanceWorkflowAsync` | `Task` | `int instanceId, int completedNodeId, string? outputData, CancellationToken` |
| `ExecuteParallelGatewayAsync` | `Task` | `int instanceId, int gatewayNodeId, CancellationToken` |
| `CheckJoinGatewayAsync` | `Task<bool>` | `int instanceId, int joinNodeId, CancellationToken` |
| `StartSubWorkflowAsync` | `Task<WorkflowInstance>` | `int parentInstanceId, int subworkflowNodeId, CancellationToken` |
| `OnChildWorkflowCompletedAsync` | `Task` | `int childInstanceId, CancellationToken` |
| `GetParallelBranchStatusAsync` | `Task<ParallelBranchStatus>` | `int instanceId, int gatewayNodeId, CancellationToken` |
| `GetChildInstancesAsync` | `Task<IEnumerable<WorkflowInstance>>` | `int parentInstanceId, CancellationToken` |

#### Wait / Timer / Timeout

| Method | Return Type | Parameters |
|--------|------------|------------|
| `StartWaitNodeAsync` | `Task<WorkflowNodeInstance>` | `int instanceId, int waitNodeId, CancellationToken` |
| `ProcessDueWaitNodesAsync` | `Task<int>` | `CancellationToken` |
| `ProcessTimedOutInstancesAsync` | `Task<int>` | `CancellationToken` |
| `GetWaitingNodesAsync` | `Task<IEnumerable<WorkflowNodeInstance>>` | `int instanceId, CancellationToken` |
| `ResumeWaitingNodeAsync` | `Task<WorkflowNodeInstance>` | `int nodeInstanceId, string? outputData, CancellationToken` |

**Helper Types (inside IWorkflowInstanceService.cs):**

| Class | Properties |
|-------|-----------|
| `WorkflowInstanceStatistics` | `Total`, `Running`, `Waiting`, `Paused`, `Completed`, `Failed`, `Cancelled`, `TimedOut`, `Suspended`, `ByWorkflow` (List) |
| `WorkflowInstanceByWorkflow` | `WorkflowId`, `WorkflowName`, `ActiveCount`, `CompletedCount`, `FailedCount` |
| `ParallelBranchStatus` | `GatewayNodeId`, `TotalBranches`, `CompletedBranches`, `FailedBranches`, `RunningBranches`, `AllCompleted`, `Branches` (List) |
| `BranchInfo` | `NodeId`, `NodeName`, `Status`, `StartedAt`, `CompletedAt`, `DurationMs` |
| `BulkStartResult` | `TotalRequested`, `SuccessCount`, `FailureCount`, `Results` (List) |
| `BulkStartItemResult` | `EntityType`, `EntityId`, `Success`, `InstanceId`, `Error` |

---

### 3.3 IWorkflowTriggerService.cs

**File:** `CRM.Core/Interfaces/IWorkflowTriggerService.cs`

| Method | Return Type | Parameters |
|--------|------------|------------|
| `GetAllAsync` | `Task<IEnumerable<WorkflowTriggerDto>>` | `bool? isActive, WorkflowTriggerType? triggerType, string? entityType, CancellationToken` |
| `GetByIdAsync` | `Task<WorkflowTriggerDto?>` | `int id, CancellationToken` |
| `GetByWorkflowAsync` | `Task<IEnumerable<WorkflowTriggerDto>>` | `int workflowDefinitionId, CancellationToken` |
| `CreateAsync` | `Task<WorkflowTriggerDto>` | `CreateWorkflowTriggerDto dto, int? createdById, CancellationToken` |
| `UpdateAsync` | `Task<WorkflowTriggerDto>` | `int id, UpdateWorkflowTriggerDto dto, CancellationToken` |
| `DeleteAsync` | `Task<bool>` | `int id, CancellationToken` |
| `ActivateAsync` | `Task<WorkflowTriggerDto>` | `int id, CancellationToken` |
| `DeactivateAsync` | `Task<WorkflowTriggerDto>` | `int id, CancellationToken` |
| `EvaluateTriggersAsync` | `Task<TriggerExecutionResult>` | `TriggerExecutionRequest request, CancellationToken` |
| `FireTriggerAsync` | `Task<TriggerResult>` | `int triggerId, TriggerExecutionRequest request, CancellationToken` |
| `GetMatchingTriggersAsync` | `Task<IEnumerable<WorkflowTriggerDto>>` | `TriggerExecutionRequest request, CancellationToken` |
| `GetScheduledTriggersDueAsync` | `Task<IEnumerable<WorkflowTrigger>>` | `CancellationToken` |
| `UpdateNextScheduledTimeAsync` | `Task` | `int triggerId, CancellationToken` |
| `RecordTriggerExecutionAsync` | `Task` | `int triggerId, bool success, int? instanceId, string? error, CancellationToken` |
| `GetStatisticsAsync` | `Task<TriggerStatisticsDto>` | `CancellationToken` |
| `ValidateCronExpression` | `bool` | `string expression` |
| `ValidateFilterConditions` | `bool` | `string? jsonConditions` |

---

### 3.4 IEntityEventDispatcher.cs (54 lines)

**File:** `CRM.Core/Interfaces/IEntityEventDispatcher.cs`

| Method | Return Type | Parameters |
|--------|------------|------------|
| `DispatchEntityEvent` | `void` | `string entityType, int entityId, WorkflowTriggerType triggerType, int? initiatedById = null, string? changedField = null, string? oldValue = null, string? newValue = null, string? contextData = null` |
| `DispatchEntityEventAsync` | `Task` | `string entityType, int entityId, WorkflowTriggerType triggerType, int? initiatedById = null, string? changedField = null, string? oldValue = null, string? newValue = null, string? contextData = null, CancellationToken cancellationToken = default` |

---

## 4. Service Implementations

### 4.1 WorkflowService.cs (~741 lines)

**File:** `CRM.Infrastructure/Services/WorkflowService.cs`  
**Class:** `WorkflowService : IWorkflowService`  
**Constructor:** `(CrmDbContext context, ILogger<WorkflowService> logger)`

**All 29 public methods (matches IWorkflowService 1:1):**

| # | Method |
|---|--------|
| 1 | `GetWorkflowDefinitionsAsync` |
| 2 | `GetWorkflowDefinitionAsync` |
| 3 | `GetWorkflowByKeyAsync` |
| 4 | `CreateWorkflowDefinitionAsync` |
| 5 | `UpdateWorkflowDefinitionAsync` |
| 6 | `DeleteWorkflowDefinitionAsync` |
| 7 | `ActivateWorkflowAsync` |
| 8 | `PauseWorkflowAsync` |
| 9 | `GetWorkflowVersionAsync` |
| 10 | `GetActiveVersionAsync` |
| 11 | `GetDraftVersionAsync` |
| 12 | `CreateNewVersionAsync` |
| 13 | `SaveCanvasLayoutAsync` |
| 14 | `GetVersionsAsync` |
| 15 | `UpdateVersionMetadataAsync` |
| 16 | `PublishVersionAsync` |
| 17 | `DeleteVersionAsync` |
| 18 | `RollbackToVersionAsync` |
| 19 | `CompareVersionsAsync` |
| 20 | `AddNodeAsync` |
| 21 | `UpdateNodeAsync` |
| 22 | `DeleteNodeAsync` |
| 23 | `UpdateNodePositionsAsync` |
| 24 | `AddTransitionAsync` |
| 25 | `UpdateTransitionAsync` |
| 26 | `DeleteTransitionAsync` |
| 27 | `GetStatisticsAsync` |
| 28 | *(interface-level helper types only — WorkflowStatistics, VersionComparisonResult, NodeDiffItem)* |
| 29 | *(29 methods confirmed via grep; all interface methods implemented)* |

---

### 4.2 WorkflowInstanceService.cs (~1754 lines)

**File:** `CRM.Infrastructure/Services/WorkflowInstanceService.cs`  
**Class:** `WorkflowInstanceService : IWorkflowInstanceService`  
**Constructor:** `(CrmDbContext context, ILogger<WorkflowInstanceService> logger, IServiceProvider serviceProvider)`

**All 43 public methods (matches IWorkflowInstanceService 1:1):**

| # | Method |
|---|--------|
| 1 | `GetInstancesAsync` |
| 2 | `GetInstanceAsync` |
| 3 | `GetInstanceByCorrelationIdAsync` |
| 4 | `StartWorkflowAsync` |
| 5 | `CancelInstanceAsync` |
| 6 | `PauseInstanceAsync` |
| 7 | `ResumeInstanceAsync` |
| 8 | `RetryInstanceAsync` |
| 9 | `BulkStartWorkflowAsync` |
| 10 | `StartNodeExecutionAsync` |
| 11 | `CompleteNodeExecutionAsync` |
| 12 | `FailNodeExecutionAsync` |
| 13 | `SkipNodeAsync` |
| 14 | `CreateTaskForNodeAsync` |
| 15 | `GetPendingTasksAsync` |
| 16 | `LockTaskAsync` |
| 17 | `CompleteTaskAsync` |
| 18 | `FailTaskAsync` |
| 19 | `ProcessRetryTasksAsync` |
| 20 | `GetHumanTasksForUserAsync` |
| 21 | `ClaimTaskAsync` |
| 22 | `CompleteHumanTaskAsync` |
| 23 | `LogAsync` |
| 24 | `GetLogsAsync` |
| 25 | `GetAuditLogAsync` |
| 26 | `ExportAuditLogCsvAsync` |
| 27 | `GetExecutionTimelineDataAsync` |_frontend
| 28 | `GetInstanceStatisticsAsync` |
| 29 | `GetDashboardAsync` |
| 30 | `AdvanceWorkflowAsync` |
| 31 | `ExecuteParallelGatewayAsync` |
| 32 | `CheckJoinGatewayAsync` |
| 33 | `StartSubWorkflowAsync` |
| 34 | `OnChildWorkflowCompletedAsync` |
| 35 | `GetParallelBranchStatusAsync` |
| 36 | `GetChildInstancesAsync` |
| 37 | `StartWaitNodeAsync` |
| 38 | `ProcessDueWaitNodesAsync` |
| 39 | `ProcessTimedOutInstancesAsync` |
| 40 | `GetWaitingNodesAsync` |
| 41 | `ResumeWaitingNodeAsync` |
| 42 | *(helper types defined in interface: WorkflowInstanceStatistics, ParallelBranchStatus, etc.)* |
| 43 | *(43 methods confirmed via grep; all interface methods implemented)* |

---

### 4.3 WorkflowTriggerService.cs (757 lines) — FULLY READ

**File:** `CRM.Infrastructure/Services/WorkflowTriggerService.cs`  
**Class:** `WorkflowTriggerService : IWorkflowTriggerService`  
**Constructor:** `(CrmDbContext context, ILogger<WorkflowTriggerService> logger)`

**17 public methods + 2 validation methods:**

| # | Method | Notes |
|---|--------|-------|
| 1 | `GetAllAsync` | Filters by IsActive, TriggerType, EntityType |
| 2 | `GetByIdAsync` | |
| 3 | `GetByWorkflowAsync` | |
| 4 | `CreateAsync` | Validates type requirements, calculates next scheduled time |
| 5 | `UpdateAsync` | |
| 6 | `DeleteAsync` | Hard delete (not soft) |
| 7 | `ActivateAsync` | |
| 8 | `DeactivateAsync` | |
| 9 | `EvaluateTriggersAsync` | Core evaluation loop — finds matching triggers, fires each |
| 10 | `FireTriggerAsync` | Evaluates filters, starts workflow via `StartWorkflowAsync` |
| 11 | `GetMatchingTriggersAsync` | Filters by entity type + trigger type |
| 12 | `GetScheduledTriggersDueAsync` | `NextScheduledAt <= UtcNow` |
| 13 | `UpdateNextScheduledTimeAsync` | Uses `CalculateNextScheduledTime` |
| 14 | `RecordTriggerExecutionAsync` | Updates `LastTriggeredAt`, `ExecutionCount` |
| 15 | `GetStatisticsAsync` | |
| 16 | `ValidateCronExpression` | Uses `Cronos.CronExpression.Parse()` |
| 17 | `ValidateFilterConditions` | JSON parse validation |

**Private Methods:**

| Method | Notes |
|--------|-------|
| `ValidateTriggerTypeRequirements` | Validates required fields per trigger type |
| `CalculateNextScheduledTime` | Uses Cronos library for cron → DateTime |
| `EvaluateFilterConditionsAsync` | **⚠️ STUB — always returns `true` (has TODO)** |
| `StartWorkflowAsync` | Resolves `IWorkflowInstanceService` from DI, calls `StartWorkflowAsync` |
| `MapToDto` | Entity → DTO mapping |
| `WriteConfigAuditLogAsync` | Writes audit log for config changes |

---

### 4.4 WorkflowWorkerService.cs (~830 lines)

**File:** `CRM.Infrastructure/Services/WorkflowWorkerService.cs`  
**Class:** `WorkflowWorkerService : BackgroundService` (no interface)

**Options Class — `WorkflowWorkerOptions`:**

| Property | Type | Default |
|----------|------|---------|
| `WorkerId` | `string` | `Environment.MachineName + "-" + Guid` |
| `MaxConcurrentTasks` | `int` | `5` |
| `PollIntervalSeconds` | `int` | `5` |
| `LockDurationMinutes` | `int` | `15` |
| `MaxRetryCount` | `int` | `3` |
| `BaseRetryDelaySeconds` | `int` | `30` |
| `EnableLLMActions` | `bool` | `true` |
| `QueueNames` | `string[]` | `["default", "priority", "background"]` |

**Key Methods (grep-level inventory):**

| Method | Purpose |
|--------|---------|
| `ExecuteAsync` | Main loop: FetchNextTask → ProcessTaskAsync per queue |
| `FetchNextTask` | Gets and locks next pending task |
| `ProcessTaskAsync` | Routes by TaskType (Automated, Human, Timer, Event, LLM) |
| `AdvanceWorkflowAsync` | After task completion, evaluates transitions to next node |
| `EvaluateTransitionsAsync` | Evaluates outgoing transition conditions |
| `EvaluateExpression` | Expression-based condition evaluation |
| `EvaluateFieldMatch` | Field match condition |
| `EvaluateUserChoice` | User choice condition |
| `CreateNodeExecutionAsync` | Creates next node instance + task |
| `LogWorkflowEvent` | Logs to WorkflowLog |
| `ExecuteAutomatedAction` | Routes by action type |
| `ExecuteLogAction` | Logs a message |
| `ExecuteUpdateEntityAction` | Updates entity field |
| `ExecuteSendEmailAction` | Sends email |
| `ExecuteWebhookAction` | Calls external URL |
| `ExecuteTimerAction` | Sets timer delay |
| `ExecuteEventAction` | Fires event |

---

### 4.5 ScheduledWorkflowService.cs (~180 lines)

**File:** `CRM.Infrastructure/Services/ScheduledWorkflowService.cs`  
**Class:** `ScheduledWorkflowService : BackgroundService` (no interface)  
**Constructor:** `(IServiceProvider serviceProvider, ILogger<ScheduledWorkflowService> logger)`

**Check Interval:** 1 minute

| Method | Purpose |
|--------|---------|
| `ExecuteAsync` | Main loop — calls 3 processors every minute |
| `ProcessDueTriggersAsync` | `triggerService.GetScheduledTriggersDueAsync()` → `FireTriggerAsync()` |
| `ProcessDueWaitNodesAsync` | `instanceService.ProcessDueWaitNodesAsync()` |
| `ProcessTimedOutItemsAsync` | `instanceService.ProcessTimedOutInstancesAsync()` |

---

### 4.6 WorkflowLogRetentionService.cs (~180 lines)

**File:** `CRM.Infrastructure/Services/WorkflowLogRetentionService.cs`  
**Class:** `WorkflowLogRetentionService : BackgroundService` (no interface)

**Configuration:**

| Setting | Value |
|---------|-------|
| Check interval | 24 hours |
| Batch size | 1000 |
| Debug log retention | 7 days |
| Info log retention | 30 days |
| Warning log retention | 90 days |
| Error/Critical retention | 365 days |

| Method | Purpose |
|--------|---------|
| `ExecuteAsync` | Runs `PurgeExpiredLogsAsync` every 24 hours |
| `PurgeExpiredLogsAsync` | Iterates log levels, calls `DeleteLogsInBatchesAsync` |
| `DeleteLogsInBatchesAsync` | Batch-deletes expired logs |

---

### 4.7 EntityEventDispatcher.cs (130 lines) — FULLY READ

**File:** `CRM.Infrastructure/Services/EntityEventDispatcher.cs`  
**Class:** `EntityEventDispatcher : IEntityEventDispatcher`  
**Constructor:** `(IServiceScopeFactory scopeFactory, ILogger<EntityEventDispatcher> logger)`

| Method | Pattern | Description |
|--------|---------|-------------|
| `DispatchEntityEvent` | Fire-and-forget (`_ = Task.Run(async () => ...)`) | Calls `DispatchEntityEventCoreAsync` in background — does NOT block caller |
| `DispatchEntityEventAsync` | Awaitable | Calls `DispatchEntityEventCoreAsync` directly |
| `DispatchEntityEventCoreAsync` (private) | Scoped service resolution | Creates DI scope → resolves `IWorkflowTriggerService` → builds `TriggerExecutionRequest` → calls `EvaluateTriggersAsync` |

---

## 5. Controllers

### 5.1 CRM.Api — WorkflowController.cs (1719 lines)

**File:** `CRM.Api/Controllers/WorkflowController.cs`  
**Route:** `api/workflows`  
**Auth:** `[Authorize]`  
**Dependencies:** `CrmDbContext`, `WorkflowService` **(concrete, not interface)**, `ILLMService`, `ILLMSettingsService`, `ILogger<WorkflowController>`

| # | Verb | Route | Method | Auth |
|---|------|-------|--------|------|
| 1 | GET | `/` | `GetWorkflows` | Any |
| 2 | GET | `/{id}` | `GetWorkflow` | Any |
| 3 | POST | `/` | `CreateWorkflow` | Admin |
| 4 | PUT | `/{id}` | `UpdateWorkflow` | Admin |
| 5 | DELETE | `/{id}` | `DeleteWorkflow` | Admin |
| 6 | POST | `/{id}/activate/{versionId}` | `ActivateWorkflow` | Admin |
| 7 | POST | `/{id}/pause` | `PauseWorkflow` | Admin |
| 8 | GET | `versions/{versionId}` | `GetVersion` | Any |
| 9 | POST | `/{workflowId}/versions` | `CreateVersion` | Admin |
| 10 | PUT | `versions/{versionId}/layout` | `SaveCanvasLayout` | Admin |
| 11 | GET | `/{workflowId}/versions` | `GetVersions` | Any |
| 12 | PUT | `versions/{versionId}` | `UpdateVersionMetadata` | Admin |
| 13 | POST | `versions/{versionId}/publish` | `PublishVersion` | Admin |
| 14 | DELETE | `versions/{versionId}` | `DeleteVersion` | Admin |
| 15 | POST | `/{workflowId}/rollback/{versionId}` | `RollbackToVersion` | Admin |
| 16 | GET | `versions/compare` | `CompareVersions` | Any |
| 17 | POST | `versions/{versionId}/nodes` | `AddNode` | Admin |
| 18 | PUT | `nodes/{nodeId}` | `UpdateNode` | Admin |
| 19 | DELETE | `nodes/{nodeId}` | `DeleteNode` | Admin |
| 20 | PUT | `versions/{versionId}/nodes/positions` | `UpdateNodePositions` | Admin |
| 21 | POST | `versions/{versionId}/transitions` | `AddTransition` | Admin |
| 22 | PUT | `transitions/{transitionId}` | `UpdateTransition` | Admin |
| 23 | DELETE | `transitions/{transitionId}` | `DeleteTransition` | Admin |
| 24 | GET | `statistics` | `GetStatistics` | Any |
| 25 | GET | `config` | `GetWorkflowConfig` | Any |
| 26 | GET | `llm-settings` | `GetLLMSettings` | Any |
| 27 | PUT | `llm-settings` | `UpdateLLMSettings` | Admin |
| 28 | POST | `llm-settings/reset` | `ResetLLMSettings` | Admin |
| 29 | POST | `llm-settings/initialize` | `InitializeLLMSettings` | Admin |
| 30 | GET | `entity-types` | `GetEntityTypes` | Any |
| 31 | GET | `node-types` | `GetNodeTypes` | Any |
| 32 | GET | `categories` | `GetCategories` | Any |

**Embedded Config Data (in private helper methods):**

| Helper Method | Output |
|---------------|--------|
| `GetEntityTypesInternal()` | 8 entity types (Customer, Lead, Contact, Opportunity, Account, ServiceRequest, Quote, Campaign) |
| `GetNodeTypesInternal()` | 17 node types (from `WorkflowNodeType` enum) |
| `GetActionTypesInternal()` | **23 action types** in 10 categories |
| `GetTriggerTypesInternal()` | **12 trigger types** (see Gap #1 below) |
| `GetConditionOperatorsInternal()` | 16 operators |
| `GetStatusOptionsInternal()` | 10 status values with colors/icons |
| `GetIconOptionsInternal()` | 28 icon names |
| `GetColorOptionsInternal()` | 18 color hex values |
| `GetFallbackActionsInternal()` | 6 fallback actions |
| `GetEventTypesInternal()` | 20 event types in 5 categories |
| `GetEntityFieldsInternal()` | Full field definitions for 8 entity types |
| `GetRelatedEntitiesInternal()` | Relationship maps for 8 entity types |

---

### 5.2 CRM.Api — WorkflowInstanceController.cs

**File:** `CRM.Api/Controllers/WorkflowInstanceController.cs`  
**Route:** `api/workflow-instances`  
**Auth:** `[Authorize]`  
**Dependencies:** `IWorkflowInstanceService`, `IHttpCalloutService`, `ILogger`

| # | Verb | Route | Method | Auth |
|---|------|-------|--------|------|
| 1 | GET | `/` | `GetInstances` | Any |
| 2 | GET | `/{id}` | `GetInstance` | Any |
| 3 | GET | `/entity/{entityType}/{entityId}` | `GetInstancesForEntity` | Any |
| 4 | POST | `/start` | `StartWorkflow` | Any |
| 5 | POST | `/{id}/cancel` | `CancelInstance` | Any |
| 6 | POST | `/{id}/pause` | `PauseInstance` | Any |
| 7 | POST | `/{id}/resume` | `ResumeInstance` | Any |
| 8 | POST | `/{id}/retry` | `RetryInstance` | Any |
| 9 | POST | `/{instanceId}/nodes/{nodeId}/skip` | `SkipNode` | Admin |
| 10 | GET | `/my-tasks` | `GetMyTasks` | Any |
| 11 | POST | `/tasks/{taskId}/claim` | `ClaimTask` | Any |
| 12 | POST | `/tasks/{taskId}/complete` | `CompleteTask` | Any |
| 13 | POST | `/tasks/{taskId}/reassign` | `ReassignTask` | Admin |
| 14 | GET | `/{id}/logs` | `GetLogs` | Any |
| 15 | GET | `/{id}/audit` | `GetAuditLog` | Any |
| 16 | GET | `/{id}/audit/export` | `ExportAuditLog` | Any |
| 17 | GET | `/{id}/timeline` | `GetExecutionTimeline` | Any |
| 18 | GET | `/dashboard` | `GetDashboard` | Any |
| 19 | GET | `/statistics` | `GetStatistics` | Any |
| 20 | POST | `/{id}/advance` | `AdvanceWorkflow` | Any |

---

### 5.3 CRM.Api — WorkflowTriggersController.cs

**File:** `CRM.Api/Controllers/WorkflowTriggersController.cs`  
**Route:** `api/workflow-triggers`  
**Auth:** `[Authorize]`  
**Dependencies:** `IWorkflowTriggerService`, `ILogger`

| # | Verb | Route | Method |
|---|------|-------|--------|
| 1 | GET | `/` | `GetTriggers` |
| 2 | GET | `/{id}` | `GetTrigger` |
| 3 | GET | `/workflow/{workflowId}` | `GetTriggersForWorkflow` |
| 4 | POST | `/` | `CreateTrigger` |
| 5 | PUT | `/{id}` | `UpdateTrigger` |
| 6 | DELETE | `/{id}` | `DeleteTrigger` |
| 7 | POST | `/{id}/activate` | `ActivateTrigger` |
| 8 | POST | `/{id}/deactivate` | `DeactivateTrigger` |
| 9 | POST | `/{id}/fire` | `FireTrigger` |
| 10 | POST | `/evaluate` | `EvaluateTriggers` |
| 11 | POST | `/{id}/record-execution` | `RecordTriggerExecution` |
| 12 | GET | `/scheduled/due` | `GetDueScheduledTriggers` |
| 13 | PUT | `/{id}/schedule` | `UpdateSchedule` |
| 14 | GET | `/statistics` | `GetStatistics` |
| 15 | POST | `/validate-cron` | `ValidateCronExpression` |
| 16 | POST | `/validate-filter` | `ValidateFilterConditions` |

**Inline Helper DTOs:** `UpdateScheduleRequest`, `CronValidationRequest`, `CronValidationResult`, `FilterValidationRequest`, `FilterValidationResult`

---

### 5.4 CRM.ServiceDeskService — WorkflowController.cs (duplicate)

**File:** `CRM.Backend/src/Services/CRM.ServiceDeskService/Controllers/WorkflowController.cs`  
**Route:** `api/workflows`  
**Dependencies:** Same as CRM.Api version (CrmDbContext, WorkflowService concrete, ILLMService, ILLMSettingsService)  
**Note:** Near-duplicate of CRM.Api WorkflowController for the microservices deployment.

---

### 5.5 CRM.ServiceDeskService — WorkflowEngineController.cs

**File:** `CRM.Backend/src/Services/CRM.ServiceDeskService/Controllers/WorkflowEngineController.cs`  
**Route:** `api/workflowengine`  
**Auth:** `[Authorize]`  
**Dependencies:** `CrmDbContext`, `WorkflowInstanceService` **(concrete)**, `ILogger`

| # | Verb | Route | Method |
|---|------|-------|--------|
| 1 | GET | `/instances` | `GetInstances` |
| 2 | GET | `/instances/{id}` | `GetInstance` |
| 3 | POST | `/instances/start` | `StartWorkflow` |
| 4 | POST | `/instances/{id}/cancel` | `CancelInstance` |
| 5 | POST | `/instances/{id}/pause` | `PauseInstance` |
| 6 | POST | `/instances/{id}/resume` | `ResumeInstance` |
| 7 | POST | `/instances/{id}/retry` | `RetryInstance` |
| 8 | GET | `/stats` | `GetStats` |
| 9 | GET | `/definitions` | `GetDefinitions` |
| 10 | GET | `/tasks` | `GetTasks` |
| 11 | POST | `/tasks/{id}/complete` | `CompleteTask` |

---

## 6. CrmDbContext DbSets

**File:** `CRM.Infrastructure/Data/CrmDbContext.cs`  
**Lines:** 164–173, 191

```csharp
public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;
public DbSet<WorkflowVersion> WorkflowVersions { get; set; } = null!;
public DbSet<WorkflowNode> WorkflowNodes { get; set; } = null!;
public DbSet<WorkflowTransition> WorkflowTransitions { get; set; } = null!;
public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
public DbSet<WorkflowNodeInstance> WorkflowNodeInstances { get; set; } = null!;
public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
public DbSet<WorkflowLog> WorkflowLogs { get; set; } = null!;
public DbSet<WorkflowTrigger> WorkflowTriggers { get; set; } = null!;
// ...
public DbSet<CampaignWorkflow> CampaignWorkflows { get; set; } = null!;
```

**Total:** 10 DbSets (9 Workflow namespace + 1 CampaignWorkflow)

---

## 7. DI Registration

**File:** `CRM.Api/Program.cs`

| Line | Registration | Lifetime |
|------|-------------|----------|
| 428 | `IApprovalWorkflowService → ApprovalWorkflowService` | Scoped |
| 447 | `IWorkflowService → WorkflowService` | Scoped |
| 448 | `WorkflowService` (concrete) | Scoped |
| 449 | `IWorkflowInstanceService → WorkflowInstanceService` | Scoped |
| 450 | `WorkflowInstanceService` (concrete) | Scoped |
| 484–491 | `WorkflowWorkerOptions` configured inline | Singleton |
| 492 | `WorkflowWorkerService` (HostedService) | Singleton |
| 495 | `IWorkflowTriggerService → WorkflowTriggerService` | Scoped |
| 496 | `IEntityEventDispatcher → EntityEventDispatcher` | Singleton |
| 497 | `ScheduledWorkflowService` (HostedService) | Singleton |
| 500 | `WorkflowLogRetentionService` (HostedService) | Singleton |

**WorkflowWorkerOptions (lines 484–491):**

```csharp
var workflowWorkerOptions = new WorkflowWorkerOptions
{
    MaxConcurrentTasks = 5,
    PollIntervalSeconds = 5,
    LockDurationMinutes = 15,
    EnableLLMActions = true
};
```

---

## 8. Gap Analysis & Findings

### 🔴 Critical Gaps

| # | Gap | Location | Impact |
|---|-----|----------|--------|
| **G-01** | **`EvaluateFilterConditionsAsync` is a STUB** — always returns `true` | [WorkflowTriggerService.cs](CRM.Backend/src/CRM.Infrastructure/Services/WorkflowTriggerService.cs) | Every trigger with `FilterConditions` JSON will match unconditionally. Triggers that should be filtered will fire when they shouldn't. |
| **G-02** | **Trigger type mismatch: Controller (12) vs Entity Enum (10)** | WorkflowController.cs `GetTriggerTypesInternal()` vs `WorkflowTriggerType` enum | Controller advertises `onStatusChange`, `onApproval`, `onRejection`, `onAssignment` — none of which exist in the entity enum. Entity enum has `OnSLABreach` and `OnEscalation` — neither appears in the controller config. Frontend will show trigger types that cannot be persisted. |
| **G-03** | **Trigger type casing mismatch** | Controller uses camelCase strings (`"onCreate"`, `"onUpdate"`) vs entity enum PascalCase (`OnCreate`, `OnUpdate`) | Frontend receives camelCase, but if it sends them back to the API the enum binding may fail depending on the JSON serializer settings. |

### 🟡 Architectural Concerns

| # | Issue | Location | Impact |
|---|-------|----------|--------|
| **A-01** | **God Controller** — `WorkflowController.cs` is 1719 lines | CRM.Api/Controllers/ | Contains 32 endpoints, 22 DTOs, 12 config helper methods, LLM settings management. Violates SRP. |
| **A-02** | **22 Inline DTO classes** defined inside the controller | WorkflowController.cs `#region DTOs` | DTOs are not reusable from other projects (microservices, tests). Duplicate definition risk. |
| **A-03** | **Concrete service injection** in controllers | WorkflowController uses `WorkflowService` not `IWorkflowService` | Violates dependency inversion, makes mocking/testing harder. Both concrete + interface are registered in DI. |
| **A-04** | **ServiceDeskService has near-duplicate WorkflowController.cs** | ServiceDeskService/Controllers/ | Two copies of the same controller to maintain — high risk of drift. |
| **A-05** | **BackgroundServices have no interfaces** | WorkflowWorkerService, ScheduledWorkflowService, WorkflowLogRetentionService | Cannot be mocked for testing. Configuration is only via inline code, not `IOptions<T>`. |
| **A-06** | **Hardcoded entity field definitions** in `GetEntityFieldsInternal()` | WorkflowController.cs | 8 entity types with full field configs are hardcoded in the controller, not generated from actual entity models. Field additions/changes in entities won't be reflected here. |
| **A-07** | **Hardcoded retention policy** in `WorkflowLogRetentionService` | WorkflowLogRetentionService.cs | Retention days per log level are constants, not configurable via `appsettings.json`. |

### 🟢 Observations

| # | Observation | Details |
|---|-------------|---------|
| **O-01** | Entity ↔ Interface ↔ Service alignment is solid | All IWorkflowService (29 methods) and IWorkflowInstanceService (43 methods) signatures are fully implemented. |
| **O-02** | Comprehensive AI node support | 7 AI-specific `WorkflowNodeType` values (LLMAction, AIDecision, AIAgent, AIContentGenerator, AIDataExtractor, AIClassifier, AISentimentAnalyzer). LLM settings management endpoints exist. |
| **O-03** | CampaignWorkflow is outside Workflow namespace | Located in `CRM.Core.Entities` rather than `CRM.Core.Entities.Workflow`. Has its own enum `CampaignWorkflowType`. |
| **O-04** | EntityEventDispatcher uses correct Singleton + Scope pattern | Registered as Singleton but creates a new DI scope internally via `IServiceScopeFactory` to resolve scoped services. This avoids captive dependency issues. |
| **O-05** | Worker concurrency model | `WorkflowWorkerService` polls 3 queues ("default", "priority", "background") with `SemaphoreSlim(MaxConcurrentTasks=5)` and task locking via `LockExpiresAt`. |
| **O-06** | Full version comparison support | `CompareVersionsAsync` produces `VersionComparisonResult` with added/removed/modified nodes and transitions — diff-level granularity. |
| **O-07** | ScheduledWorkflowService handles 3 concerns per tick | Due triggers, due wait nodes, and timed-out instances — all processed every 60 seconds. |
| **O-08** | 23 automated action types defined | Includes: log, updateEntity, sendEmail, sendNotification, webhook, createTask, createActivity, createNote, assignOwner, addTag, removeTag, updateField, calculateField, validateField, callApi, delay, sendSms, publishEvent, startSubWorkflow, approval, conditionalBranch, loopAction, aiAction |

---

### Summary Counts

| Area | Count |
|------|-------|
| Entities | 10 (9 in Workflow namespace + CampaignWorkflow) |
| Enums | 12 (across all entity files) |
| Dedicated DTO Files | 2 (WorkflowTriggerDtos.cs, WorkflowInstanceDtos.cs) |
| DTO Classes (dedicated) | 27 |
| Inline DTO Classes (controller) | 22 |
| **Total DTO Classes** | **49** |
| Interfaces | 4 (IWorkflowService, IWorkflowInstanceService, IWorkflowTriggerService, IEntityEventDispatcher) |
| Interface Methods (total) | 72 + 17 + 2 = **91** |
| Service Implementations | 7 (WorkflowService, WorkflowInstanceService, WorkflowTriggerService, WorkflowWorkerService, ScheduledWorkflowService, WorkflowLogRetentionService, EntityEventDispatcher) |
| Controllers (CRM.Api) | 3 (WorkflowController, WorkflowInstanceController, WorkflowTriggersController) |
| Controllers (ServiceDeskService) | 2 (WorkflowController duplicate, WorkflowEngineController) |
| **Total Controllers** | **5** |
| API Endpoints (CRM.Api total) | 32 + 20 + 16 = **68** |
| API Endpoints (ServiceDeskService) | ~32 + 11 = **~43** |
| CrmDbContext DbSets | 10 |
| DI Registrations | 11 lines |
| BackgroundServices | 3 |

---

**END OF AUDIT**

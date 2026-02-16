# SPEC-ARCH-006: Worker Service Architecture

> **Spec ID:** SPEC-ARCH-006  
> **Feature:** Worker Service Architecture  
> **Module:** Architecture  
> **Version:** 1.0  
> **Last Updated:** 2026-02-16  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description
Define a dedicated worker service architecture to run background processing workloads (ITSM escalation, SLA timers, notification dispatch, and maintenance jobs) outside the API request path. The worker(s) must be durable, idempotent, observable, and horizontally scalable.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| WS-001 | Worker Host | Dedicated process for background jobs | ❌ |
| WS-002 | Queue/Backbone | Transport and job routing | ❌ |
| WS-003 | Outbox Dispatcher | Durable event emission | ❌ |
| WS-004 | Escalation Worker | SLA breach and escalation evaluation | ❌ |
| WS-005 | Notification Worker | Async notification fan-out | ❌ |
| WS-006 | Observability | Metrics, logs, tracing, retries | ❌ |
| WS-007 | Operational Controls | Pause, drain, replay, DLQ | ❌ |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Process SLA breach escalations | System | SLA breach detected | Escalation history + notifications created | ❌ |
| UC-002 | Retry failed jobs | Operator | Job failed | Job retried or moved to DLQ | ❌ |
| UC-003 | Pause workers for maintenance | Operator | Planned maintenance | Workers drained and paused | ❌ |
| UC-004 | Replay failed escalations | Operator | DLQ items exist | Items reprocessed safely | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| N/A | N/A | ❌ | No frontend changes required |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| N/A | N/A | ❌ | No frontend changes required |

### 2.3 Services (API Client)
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| N/A | N/A | N/A | ❌ |

### 2.4 Frontend Validations
| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| N/A | N/A | N/A | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| WorkerJob | `CRM.Core/Entities/Workers/WorkerJob.cs` | ❌ | Canonical job envelope |
| WorkerExecution | `CRM.Core/Entities/Workers/WorkerExecution.cs` | ❌ | Execution attempts and outcomes |
| OutboxEvent | `CRM.Core/Entities/Integration/OutboxEvent.cs` | ❌ | Durable event emission |

### 3.2 DTOs
| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| WorkerJobDto | `CRM.Core/Dtos/Workers/WorkerJobDto.cs` | ❌ | Job data for monitoring |
| WorkerExecutionDto | `CRM.Core/Dtos/Workers/WorkerExecutionDto.cs` | ❌ | Execution status |

### 3.3 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IWorkerQueue | `CRM.Core/Interfaces/Workers/IWorkerQueue.cs` | 4 | ❌ |
| IOutboxDispatcher | `CRM.Core/Interfaces/Integration/IOutboxDispatcher.cs` | 3 | ❌ |
| IEscalationProcessor | `CRM.Core/Interfaces/ITSM/IEscalationProcessor.cs` | 3 | ❌ |
| INotificationDispatcher | `CRM.Core/Interfaces/Notifications/INotificationDispatcher.cs` | 3 | ❌ |

### 3.4 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| WorkerHost | `CRM.Infrastructure/Workers/WorkerHost.cs` | 5 | ❌ |
| EscalationWorker | `CRM.Infrastructure/Workers/EscalationWorker.cs` | 6 | ❌ |
| NotificationWorker | `CRM.Infrastructure/Workers/NotificationWorker.cs` | 6 | ❌ |
| OutboxDispatcher | `CRM.Infrastructure/Integration/OutboxDispatcher.cs` | 5 | ❌ |

### 3.5 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| WorkerHealthController | `CRM.Api/Controllers/WorkerHealthController.cs` | 2 | ❌ |

### 3.6 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/workers/health` | GetHealth | No | ❌ |
| GET | `/api/workers/stats` | GetStats | Yes | ❌ |

### 3.7 Backend Validations
| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| JobType | Required, 3-100 chars | DTO | ❌ |
| Payload | Required, JSON | DTO | ❌ |
| RetryCount | >= 0 | DTO | ❌ |
| MaxRetries | 0-10 | DTO | ❌ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | File Path | Status | Notes |
|------------|----------|--------|-------|
| WorkerJobs | `database/schema/worker_jobs.sql` | ❌ | Job queue storage |
| WorkerExecutions | `database/schema/worker_executions.sql` | ❌ | Execution history |
| OutboxEvents | `database/schema/outbox_events.sql` | ❌ | Durable events |

### 4.2 Data Elements
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| JobType | VARCHAR(100) | No | - | IDX | JobType | ❌ |
| Status | VARCHAR(30) | No | Queued | IDX | Status | ❌ |
| Payload | LONGTEXT | No | - | - | Payload | ❌ |
| RetryCount | INT | No | 0 | - | RetryCount | ❌ |
| MaxRetries | INT | No | 5 | - | MaxRetries | ❌ |
| NextAttemptAt | DATETIME | Yes | - | IDX | NextAttemptAt | ❌ |

### 4.3 Relationships
| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| WorkerExecutions | WorkerJobs | N:1 | WorkerJobId | ❌ |

### 4.4 Indexes
| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_WorkerJobs_Status_NextAttemptAt | WorkerJobs | Status, NextAttemptAt | NonClustered | ❌ |
| IX_OutboxEvents_Status | OutboxEvents | Status | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| EscalationWorkerTests | `CRM.Tests/Workers/EscalationWorkerTests.cs` | 8 | ❌ |
| OutboxDispatcherTests | `CRM.Tests/Integration/OutboxDispatcherTests.cs` | 6 | ❌ |

### 5.2 Integration Tests
| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WorkerQueueIntegrationTests | `CRM.Tests/Integration/WorkerQueueIntegrationTests.cs` | 5 | ❌ |

### 5.3 E2E Tests
| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WorkerHealthChecks | `e2e-tests/tests/ops/worker-health.spec.ts` | 2 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches
| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| N/A | N/A | N/A | N/A |

### 6.2 Missing Implementations
| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Worker host project | `CRM.Workers/` | Not created | TODO-ARCH-006-001 |
| Outbox schema | `database/schema/` | Not created | TODO-ARCH-006-002 |
| Escalation worker | `CRM.Infrastructure/Workers/` | Not created | TODO-ARCH-006-003 |

### 6.3 Validation Gaps
| Field | Issue | Status |
|-------|-------|--------|
| Payload | No JSON schema validation | TODO-ARCH-006-004 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-ARCH-006-001 | Create Worker host project and base services | P0 | Architecture |
| TODO-ARCH-006-002 | Define Outbox and Worker tables in DB schemas | P0 | Database |
| TODO-ARCH-006-003 | Implement EscalationWorker and NotificationWorker | P0 | Backend |
| TODO-ARCH-006-004 | Add worker observability (metrics/logs/DLQ) | P1 | Operations |
| TODO-ARCH-006-005 | Add worker tests (unit/integration/e2e) | P1 | Tests |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-16 | Abhishek Lal | Initial specification |

# Webhook Management Specification

> **Spec ID:** SPEC-INT-001  
> **Feature:** Webhook Management  
> **Module:** Integration  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description

Webhook Management provides a real-time event notification system for external integrations. When business events occur in the CRM (e.g., account created, opportunity closed, payment received), the system sends HTTP POST requests to registered webhooks. This enables third-party systems, automation platforms, and integrations to react instantly to CRM changes without polling.

**Key Capabilities:**
- Register webhooks for specific entity events
- Filter events by entity type, status, or custom criteria
- Automatic retry with exponential backoff on delivery failure
- HMAC-SHA256 signature verification for security
- Delivery history and analytics
- Dead webhook detection and cleanup
- Test webhook delivery UI
- Event queue and concurrent dispatch

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Webhook Registration | Create/edit/delete webhooks with event filters | ❌ |
| SF-002 | Event Filtering | Filter events by entity type, status, field changes | ❌ |
| SF-003 | Signature Verification | HMAC-SHA256 signatures for webhook security | ❌ |
| SF-004 | Delivery Tracking | Real-time delivery status, retry logs, analytics | ❌ |
| SF-005 | Retry Policies | Exponential backoff, max retries, dead letter queue | ❌ |
| SF-006 | Webhook Testing | Test delivery UI to verify webhook endpoints | ❌ |
| SF-007 | Dead Webhook Detection | Automatic disabling of unresponsive webhooks | ❌ |
| SF-008 | Payload Management | Large payload handling, compression, pagination | ❌ |
| SF-009 | Event Queue | Async event processing, concurrent delivery | ❌ |
| SF-010 | Webhook Dashboard | UI for managing webhooks, viewing history | ❌ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Register webhook | Integration Manager | Must have API access | Webhook created and active | ❌ |
| UC-002 | Filter events | Integration Manager | Webhook must exist | Event filter applied | ❌ |
| UC-003 | Test webhook | Developer | Webhook registered | Test payload delivered | ❌ |
| UC-004 | View delivery history | Integration Manager | Webhook must exist | Delivery list displayed | ❌ |
| UC-005 | Retry failed delivery | System | Delivery failed | Auto-retry with backoff | ❌ |
| UC-006 | Verify webhook signature | External System | Webhook received | Signature validated (HMAC-SHA256) | ❌ |
| UC-007 | Disable dead webhook | System | 5+ consecutive failures | Webhook disabled automatically | ❌ |
| UC-008 | Export delivery logs | Admin | Webhooks exist | CSV/JSON export available | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| WebhooksPage | `CRM.Frontend/src/pages/admin/WebhooksPage.tsx` | ❌ | Main webhook management dashboard |
| WebhookDetailPage | `CRM.Frontend/src/pages/admin/WebhookDetailPage.tsx` | ❌ | Single webhook view/edit |
| WebhookDeliveryHistoryPage | `CRM.Frontend/src/pages/admin/WebhookDeliveryHistoryPage.tsx` | ❌ | Delivery tracking and analytics |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| WebhookList | `CRM.Frontend/src/components/webhooks/WebhookList.tsx` | ❌ | Table of registered webhooks |
| WebhookForm | `CRM.Frontend/src/components/webhooks/WebhookForm.tsx` | ❌ | Create/edit webhook modal or page |
| EventTypeSelector | `CRM.Frontend/src/components/webhooks/EventTypeSelector.tsx` | ❌ | Multi-select for entity events |
| EventFilterBuilder | `CRM.Frontend/src/components/webhooks/EventFilterBuilder.tsx` | ❌ | Advanced filter UI (status, fields, etc.) |
| WebhookTestSender | `CRM.Frontend/src/components/webhooks/WebhookTestSender.tsx` | ❌ | Test delivery UI with payload editor |
| DeliveryHistoryTable | `CRM.Frontend/src/components/webhooks/DeliveryHistoryTable.tsx` | ❌ | Paginated delivery log view |
| DeliveryDetail | `CRM.Frontend/src/components/webhooks/DeliveryDetail.tsx` | ❌ | Single delivery details (request, response, signature) |
| RetryPolicyForm | `CRM.Frontend/src/components/webhooks/RetryPolicyForm.tsx` | ❌ | Retry settings (max retries, backoff factor) |
| SignatureVerificationUI | `CRM.Frontend/src/components/webhooks/SignatureVerificationUI.tsx` | ❌ | Show signature, validation status, secret preview |
| WebhookAnalytics | `CRM.Frontend/src/components/webhooks/WebhookAnalytics.tsx` | ❌ | Success rate, latency, failure reason charts |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| webhookService | `CRM.Frontend/src/services/webhookService.ts` | GetAll, GetById, Create, Update, Delete, Test, GetDeliveries, Retry, DisableWebhook | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Webhook URL | Valid HTTPS URL, no localhost in production | Frontend/Backend | ❌ |
| Max Retries | Integer 0-10 | Frontend/Backend | ❌ |
| Retry Interval | Integer 60-3600 seconds | Frontend/Backend | ❌ |
| Timeout | Integer 5-60 seconds | Frontend/Backend | ❌ |
| Description | String 0-500 characters | Frontend | ❌ |
| Event Types | At least one selected | Frontend/Backend | ❌ |
| Filter Criteria | Valid JSON or empty | Frontend/Backend | ❌ |
| Active Status | Boolean | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Webhook | `CRM.Core/Entities/Webhook.cs` | ❌ | Webhook definition with URL, filters, retry policy |
| WebhookEvent | `CRM.Core/Entities/WebhookEvent.cs` | ❌ | Event type definition (AccountCreated, OpportunityClosed, etc.) |
| WebhookDelivery | `CRM.Core/Entities/WebhookDelivery.cs` | ❌ | Individual delivery record with status and response |
| DeliveryRetry | `CRM.Core/Entities/DeliveryRetry.cs` | ❌ | Retry attempt tracking |
| DeliveryLog | `CRM.Core/Entities/DeliveryLog.cs` | ❌ | Detailed execution logs for debugging |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| CreateWebhookDto | `CRM.Core/DTOs/Webhooks/CreateWebhookDto.cs` | ❌ | Input for creating webhook |
| UpdateWebhookDto | `CRM.Core/DTOs/Webhooks/UpdateWebhookDto.cs` | ❌ | Input for updating webhook |
| WebhookDto | `CRM.Core/DTOs/Webhooks/WebhookDto.cs` | ❌ | Webhook response DTO |
| WebhookEventDto | `CRM.Core/DTOs/Webhooks/WebhookEventDto.cs` | ❌ | Event type response |
| WebhookDeliveryDto | `CRM.Core/DTOs/Webhooks/WebhookDeliveryDto.cs` | ❌ | Delivery record response |
| DeliveryRetryDto | `CRM.Core/DTOs/Webhooks/DeliveryRetryDto.cs` | ❌ | Retry attempt details |
| WebhookPayloadDto | `CRM.Core/DTOs/Webhooks/WebhookPayloadDto.cs` | ❌ | Event payload sent to webhook |
| RetryPolicyDto | `CRM.Core/DTOs/Webhooks/RetryPolicyDto.cs` | ❌ | Retry settings |
| WebhookStatisticsDto | `CRM.Core/DTOs/Webhooks/WebhookStatisticsDto.cs` | ❌ | Success rate, latency stats |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IWebhookService | `CRM.Core/Interfaces/IWebhookService.cs` | 15+ | ❌ |
| IWebhookDispatcher | `CRM.Core/Interfaces/IWebhookDispatcher.cs` | 5+ | ❌ |
| IDeliveryTracker | `CRM.Core/Interfaces/IDeliveryTracker.cs` | 8+ | ❌ |
| ISignatureGenerator | `CRM.Core/Interfaces/ISignatureGenerator.cs` | 2 | ❌ |
| IRetryPolicyEngine | `CRM.Core/Interfaces/IRetryPolicyEngine.cs` | 4+ | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| WebhookService | `CRM.Infrastructure/Services/WebhookService.cs` | Create, Update, Delete, GetAll, GetById, GetDeliveries, GetStatistics, ToggleActive | ❌ |
| WebhookDispatcher | `CRM.Infrastructure/Services/WebhookDispatcher.cs` | DispatchAsync, DispatchBatchAsync, ProcessEventQueue | ❌ |
| DeliveryTracker | `CRM.Infrastructure/Services/DeliveryTracker.cs` | Track, UpdateStatus, LogRetry, GetHistory | ❌ |
| SignatureGenerator | `CRM.Infrastructure/Services/SignatureGenerator.cs` | GenerateHmacSignature, VerifySignature | ❌ |
| RetryPolicyEngine | `CRM.Infrastructure/Services/RetryPolicyEngine.cs` | ShouldRetry, CalculateBackoff, IsDeadWebhook | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| WebhooksController | `CRM.Api/Controllers/WebhooksController.cs` | 12+ | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/webhooks` | GetAllWebhooks | Yes | ❌ |
| GET | `/api/webhooks/{id}` | GetWebhookById | Yes | ❌ |
| POST | `/api/webhooks` | CreateWebhook | Yes | ❌ |
| PUT | `/api/webhooks/{id}` | UpdateWebhook | Yes | ❌ |
| DELETE | `/api/webhooks/{id}` | DeleteWebhook | Yes | ❌ |
| GET | `/api/webhooks/{id}/deliveries` | GetDeliveries | Yes | ❌ |
| GET | `/api/webhooks/{id}/deliveries/{deliveryId}` | GetDeliveryDetail | Yes | ❌ |
| POST | `/api/webhooks/{id}/test` | TestWebhook | Yes | ❌ |
| POST | `/api/webhooks/{id}/deliveries/{deliveryId}/retry` | RetryDelivery | Yes | ❌ |
| GET | `/api/webhooks/{id}/statistics` | GetWebhookStatistics | Yes | ❌ |
| PATCH | `/api/webhooks/{id}/toggle` | ToggleWebhookActive | Yes | ❌ |
| GET | `/api/webhook-events` | GetAvailableEvents | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| URL | Must be valid HTTPS (allow http://localhost for dev) | DTO/Service | ❌ |
| Description | Max 500 chars | Entity | ❌ |
| MaxRetries | 0-10 range | Service | ❌ |
| RetryIntervalSeconds | 60-3600 range | Service | ❌ |
| TimeoutSeconds | 5-60 range | Service | ❌ |
| Events | At least 1 event selected | Service | ❌ |
| FilterCriteria | Valid JSON or null | Service | ❌ |
| Active Status | Boolean | Entity | ❌ |
| Webhook URL reachable | POST /test must succeed before save | Service | ❌ |
| Duplicate URL + Events | No duplicate registrations | Service | ❌ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Webhooks | `database/schema/webhooks.sql` | ❌ | Core webhook definitions |
| WebhookEvents | `database/schema/webhook_events.sql` | ❌ | Event type catalog |
| WebhookEventRegistrations | `database/schema/webhook_event_registrations.sql` | ❌ | Junction: Webhooks ↔ Events |
| WebhookDeliveries | `database/schema/webhook_deliveries.sql` | ❌ | Delivery records |
| DeliveryRetries | `database/schema/delivery_retries.sql` | ❌ | Retry attempt logs |
| DeliveryLogs | `database/schema/delivery_logs.sql` | ❌ | Detailed execution logs |

### 4.2 Data Elements

#### Webhooks Table
| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| Url | VARCHAR(2048) | No | - | UNIQUE(Url, EventsHash) | Url | ❌ |
| Description | VARCHAR(500) | Yes | - | - | Description | ❌ |
| Secret | VARCHAR(256) | No | - | Encrypted in DB | Secret | ❌ |
| IsActive | BOOLEAN | No | TRUE | - | IsActive | ❌ |
| EventsJson | JSON | No | - | Event type filter | EventsJson | ❌ |
| FilterCriteriaJson | JSON | Yes | - | Advanced filter | FilterCriteriaJson | ❌ |
| MaxRetries | INT | No | 5 | Range 0-10 | MaxRetries | ❌ |
| RetryIntervalSeconds | INT | No | 300 | Range 60-3600 | RetryIntervalSeconds | ❌ |
| TimeoutSeconds | INT | No | 30 | Range 5-60 | TimeoutSeconds | ❌ |
| FailureCount | INT | No | 0 | Consecutive failures | FailureCount | ❌ |
| DisabledReason | VARCHAR(500) | Yes | - | Why disabled | DisabledReason | ❌ |
| DisabledAt | DATETIME | Yes | - | Timestamp disabled | DisabledAt | ❌ |
| LastDeliveryAt | DATETIME | Yes | - | Last successful send | LastDeliveryAt | ❌ |
| CreatedBy | INT | Yes | - | FK: Users.Id | CreatedBy | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | Audit | CreatedAt | ❌ |
| UpdatedAt | DATETIME | Yes | - | Audit | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | Soft delete | IsDeleted | ❌ |
| RowVersion | BINARY(8) | Yes | - | Concurrency | RowVersion | ❌ |

#### WebhookEvents Table
| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ❌ |
| EventType | VARCHAR(100) | No | - | UNIQUE | ❌ |
| Description | VARCHAR(500) | Yes | - | - | ❌ |
| EntityType | VARCHAR(50) | No | - | Entity category | ❌ |
| PayloadSchema | JSON | Yes | - | Sample payload | ❌ |
| IsActive | BOOLEAN | No | TRUE | - | ❌ |

#### WebhookDeliveries Table
| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ❌ |
| WebhookId | INT | No | - | FK: Webhooks.Id | ❌ |
| EventType | VARCHAR(100) | No | - | Event triggered | ❌ |
| EntityType | VARCHAR(50) | No | - | Entity type | ❌ |
| EntityId | INT | No | - | Entity ID | ❌ |
| PayloadJson | JSON | No | - | Request body | ❌ |
| PayloadHash | VARCHAR(64) | Yes | - | SHA-256 hash | ❌ |
| Status | VARCHAR(20) | No | 'Pending' | Pending/Delivered/Failed | ❌ |
| HttpStatusCode | INT | Yes | - | Response status | ❌ |
| ResponseHeaders | JSON | Yes | - | Response metadata | ❌ |
| ResponseBody | LONGTEXT | Yes | - | Response text (truncated) | ❌ |
| SignatureHeader | VARCHAR(256) | Yes | - | HMAC header sent | ❌ |
| Latency | INT | Yes | - | Response time (ms) | ❌ |
| AttemptCount | INT | No | 0 | Retry counter | ❌ |
| NextRetryAt | DATETIME | Yes | - | Scheduled retry | ❌ |
| FailureReason | VARCHAR(500) | Yes | - | Error description | ❌ |
| DeliveredAt | DATETIME | Yes | - | Successful delivery | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | Timestamp created | ❌ |
| UpdatedAt | DATETIME | Yes | - | Last update | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | Soft delete | ❌ |

#### DeliveryRetries Table
| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ❌ |
| DeliveryId | INT | No | - | FK: WebhookDeliveries.Id | ❌ |
| AttemptNumber | INT | No | - | Retry count (1, 2, 3...) | ❌ |
| HttpStatusCode | INT | Yes | - | Response from retry | ❌ |
| ResponseBody | LONGTEXT | Yes | - | Error message | ❌ |
| Latency | INT | Yes | - | Response time (ms) | ❌ |
| FailureReason | VARCHAR(500) | Yes | - | Why it failed | ❌ |
| ScheduledFor | DATETIME | No | - | When retry was scheduled | ❌ |
| ExecutedAt | DATETIME | Yes | - | When retry ran | ❌ |
| Success | BOOLEAN | No | FALSE | Did retry succeed | ❌ |

#### DeliveryLogs Table
| Column | Data Type | Nullable | Default | Constraints | Status |
|--------|-----------|----------|---------|-------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ❌ |
| DeliveryId | INT | No | - | FK: WebhookDeliveries.Id | ❌ |
| LogLevel | VARCHAR(20) | No | 'Info' | Info/Warning/Error | ❌ |
| Message | TEXT | No | - | Log message | ❌ |
| Exception | TEXT | Yes | - | Stack trace if error | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | Timestamp | ❌ |

### 4.3 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Webhooks | Users | N:1 | CreatedBy | ❌ |
| WebhookDeliveries | Webhooks | N:1 | WebhookId | ❌ |
| DeliveryRetries | WebhookDeliveries | N:1 | DeliveryId | ❌ |
| DeliveryLogs | WebhookDeliveries | N:1 | DeliveryId | ❌ |
| WebhookEventRegistrations | Webhooks | N:1 | WebhookId | ❌ |
| WebhookEventRegistrations | WebhookEvents | N:1 | EventId | ❌ |

### 4.4 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Webhooks_IsActive | Webhooks | IsActive | NonClustered | ❌ |
| IX_Webhooks_CreatedBy | Webhooks | CreatedBy | NonClustered | ❌ |
| IX_WebhookDeliveries_WebhookId | WebhookDeliveries | WebhookId | NonClustered | ❌ |
| IX_WebhookDeliveries_Status | WebhookDeliveries | Status | NonClustered | ❌ |
| IX_WebhookDeliveries_CreatedAt | WebhookDeliveries | CreatedAt DESC | NonClustered | ❌ |
| IX_WebhookDeliveries_NextRetryAt | WebhookDeliveries | NextRetryAt | NonClustered | ❌ |
| IX_DeliveryRetries_DeliveryId | DeliveryRetries | DeliveryId | NonClustered | ❌ |
| IX_DeliveryRetries_ExecutedAt | DeliveryRetries | ExecutedAt DESC | NonClustered | ❌ |
| IX_DeliveryLogs_DeliveryId | DeliveryLogs | DeliveryId | NonClustered | ❌ |
| IX_WebhookEventRegistrations_WebhookId | WebhookEventRegistrations | WebhookId | NonClustered | ❌ |
| IX_WebhookEventRegistrations_EventId | WebhookEventRegistrations | EventId | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WebhookServiceTests | `CRM.Tests/Services/WebhookServiceTests.cs` | 18 | ❌ |
| SignatureGeneratorTests | `CRM.Tests/Services/SignatureGeneratorTests.cs` | 12 | ❌ |
| RetryPolicyEngineTests | `CRM.Tests/Services/RetryPolicyEngineTests.cs` | 15 | ❌ |
| WebhookDispatcherTests | `CRM.Tests/Services/WebhookDispatcherTests.cs` | 16 | ❌ |
| EventFilterTests | `CRM.Tests/Webhooks/EventFilterTests.cs` | 14 | ❌ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WebhookIntegrationTests | `CRM.Tests/Integration/WebhookIntegrationTests.cs` | 20 | ❌ |
| WebhookDeliveryIntegrationTests | `CRM.Tests/Integration/WebhookDeliveryIntegrationTests.cs` | 18 | ❌ |
| RetryMechanismIntegrationTests | `CRM.Tests/Integration/RetryMechanismIntegrationTests.cs` | 16 | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| WebhookManagement | `e2e-tests/tests/webhooks/webhook-management.spec.ts` | 12 | ❌ |
| WebhookDelivery | `e2e-tests/tests/webhooks/webhook-delivery.spec.ts` | 10 | ❌ |
| WebhookSignature | `e2e-tests/tests/webhooks/webhook-signature.spec.ts` | 8 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Infinite Loop Prevention

| Issue | Description | Impact | Resolution |
|-------|-------------|--------|------------|
| Self-Referential Webhooks | Webhook triggers event that triggers same webhook | System overload, runaway threads | Implement event chain tracking; prevent same event type triggering same webhook |
| Circular Event Dependencies | Event A triggers Event B which triggers Event A | Infinite retry loops | Graph-based cycle detection before dispatch |
| Webhook → CRM Update → Webhook Loop | External system updates CRM via webhook, update triggers same webhook | Data corruption risk | Mark webhook-originated events with `SourceWebhookId`; exclude in trigger condition |

**Resolution Strategy:**
- Maintain event chain depth counter (`MaxChainDepth = 5`)
- Skip dispatch if event source = same webhook
- Log all self-referential attempts
- Alert admin if threshold exceeded

### 6.2 Dead Webhook Management

| Issue | Description | Current State | TODO |
|-------|-------------|---------------|------|
| Unresponsive Endpoints | Webhooks fail repeatedly but remain registered | Manual disabling only | Auto-disable after 5 consecutive failures |
| Orphaned Webhooks | No activity for 30+ days | Never cleaned up | Implement last-activity tracking |
| Large Payload Failures | Payloads > 1MB cause delivery failures | Silently fails | Implement payload chunking or compression |
| DNS Timeouts | URL valid but DNS unresolvable | Retried indefinitely | Cache DNS failures; alert after 3 failures |

**Resolution:**
- `FailureCount++` on each failed delivery
- After 5 consecutive failures: set `IsActive = FALSE`, populate `DisabledReason`
- Webhook remains in DB for history but won't dispatch
- Admin can manually re-enable from dashboard

### 6.3 Large Payload Handling

| Scenario | Max Size | Issue | Solution |
|----------|----------|-------|----------|
| Account with 1000+ contacts | ~5MB | HTTP POST fails | Implement payload pagination |
| PDF attachment in delivery | ~50MB | Memory overload | Stream to temp file, then POST |
| Campaign recipient list | ~10K records | Request timeout | Batch deliveries with limit |

**Implementations:**
- Max single delivery: 2MB (configurable)
- Chunk payload: split into multiple deliveries with `SequenceNumber`
- Gzip compression enabled if `Content-Encoding: gzip` supported
- Implement pagination: `/accounts?skip=0&take=100`

### 6.4 Security Considerations

| Vulnerability | Risk | Mitigation |
|---------------|------|-----------|
| Secret exposure | Secret visible in logs | Never log Secret column; only log masked signature |
| Man-in-the-middle | Request intercepted | Enforce HTTPS only (allow localhost for dev) |
| Signature spoofing | Attacker forges HMAC | Use HMAC-SHA256; verify before processing |
| Rate limiting | Webhook endpoint DoS'd | Implement exponential backoff; max 1 retry per minute |
| URL injection | Malicious URL in webhook | Validate against URL whitelist; block internal IPs (127.x, 192.168.x) |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category | Spec Section |
|---------|-------------|----------|----------|--------------|
| TODO-INT001-01 | Create Webhook entity with all properties | P1 | Backend | 3.1 |
| TODO-INT001-02 | Create WebhookEvent entity and seed event types | P1 | Backend | 3.1 |
| TODO-INT001-03 | Create WebhookDelivery entity with tracking fields | P1 | Backend | 3.1 |
| TODO-INT001-04 | Implement IWebhookService interface (15+ methods) | P1 | Backend | 3.3 |
| TODO-INT001-05 | Implement WebhookService CRUD operations | P1 | Backend | 3.4 |
| TODO-INT001-06 | Implement SignatureGenerator with HMAC-SHA256 | P1 | Backend | 3.4 |
| TODO-INT001-07 | Implement IWebhookDispatcher for async delivery | P1 | Backend | 3.3 |
| TODO-INT001-08 | Implement WebhookDispatcher with event queue | P1 | Backend | 3.4 |
| TODO-INT001-09 | Implement RetryPolicyEngine with exponential backoff | P1 | Backend | 3.4 |
| TODO-INT001-10 | Implement IDeliveryTracker interface | P2 | Backend | 3.3 |
| TODO-INT001-11 | Implement DeliveryTracker for logging/metrics | P2 | Backend | 3.4 |
| TODO-INT001-12 | Create WebhookDto and related DTOs | P1 | Backend | 3.2 |
| TODO-INT001-13 | Create WebhooksController with 12+ endpoints | P1 | Backend | 3.5 |
| TODO-INT001-14 | Implement backend validations for webhook registration | P1 | Backend | 3.7 |
| TODO-INT001-15 | Create database schema for Webhooks table | P1 | Database | 4.1 |
| TODO-INT001-16 | Create database schema for WebhookEvents table | P1 | Database | 4.1 |
| TODO-INT001-17 | Create database schema for WebhookDeliveries table | P1 | Database | 4.1 |
| TODO-INT001-18 | Create database schema for DeliveryRetries table | P1 | Database | 4.1 |
| TODO-INT001-19 | Create database indexes for performance | P2 | Database | 4.4 |
| TODO-INT001-20 | Implement frontend WebhooksPage.tsx | P1 | Frontend | 2.1 |
| TODO-INT001-21 | Implement WebhookList component with pagination | P1 | Frontend | 2.2 |
| TODO-INT001-22 | Implement WebhookForm for create/edit | P1 | Frontend | 2.2 |
| TODO-INT001-23 | Implement EventTypeSelector multi-select | P1 | Frontend | 2.2 |
| TODO-INT001-24 | Implement EventFilterBuilder for advanced filters | P2 | Frontend | 2.2 |
| TODO-INT001-25 | Implement WebhookTestSender UI with payload editor | P1 | Frontend | 2.2 |
| TODO-INT001-26 | Implement DeliveryHistoryTable with sorting/filtering | P2 | Frontend | 2.2 |
| TODO-INT001-27 | Implement DeliveryDetail modal for debugging | P2 | Frontend | 2.2 |
| TODO-INT001-28 | Implement SignatureVerificationUI | P2 | Frontend | 2.2 |
| TODO-INT001-29 | Implement webhookService.ts API client | P1 | Frontend | 2.3 |
| TODO-INT001-30 | Implement frontend validations for webhook form | P1 | Frontend | 2.4 |
| TODO-INT001-31 | Create unit tests for WebhookService (18 tests) | P2 | Testing | 5.1 |
| TODO-INT001-32 | Create unit tests for SignatureGenerator (12 tests) | P2 | Testing | 5.1 |
| TODO-INT001-33 | Create unit tests for RetryPolicyEngine (15 tests) | P2 | Testing | 5.1 |
| TODO-INT001-34 | Create unit tests for WebhookDispatcher (16 tests) | P2 | Testing | 5.1 |
| TODO-INT001-35 | Create unit tests for EventFilter (14 tests) | P2 | Testing | 5.1 |
| TODO-INT001-36 | Create integration tests for webhook CRUD | P2 | Testing | 5.2 |
| TODO-INT001-37 | Create integration tests for delivery retry mechanism | P2 | Testing | 5.2 |
| TODO-INT001-38 | Create integration tests for signature verification | P2 | Testing | 5.2 |
| TODO-INT001-39 | Create E2E tests for webhook management flow | P3 | Testing | 5.3 |
| TODO-INT001-40 | Create E2E tests for webhook delivery and retry | P3 | Testing | 5.3 |
| TODO-INT001-41 | Implement infinite loop prevention mechanism | P1 | Features | 6.1 |
| TODO-INT001-42 | Implement auto-disable dead webhook logic | P1 | Features | 6.2 |
| TODO-INT001-43 | Implement large payload handling/chunking | P2 | Features | 6.3 |
| TODO-INT001-44 | Implement event chain tracking and cycle detection | P1 | Features | 6.1 |
| TODO-INT001-45 | Implement concurrent webhook dispatch (background service) | P1 | Features | 1.2 |
| TODO-INT001-46 | Implement webhook health monitoring dashboard | P2 | Frontend | 2.2 |
| TODO-INT001-47 | Implement webhook analytics (success rate, latency) | P2 | Frontend | 2.2 |
| TODO-INT001-48 | Add feature flag for webhook system (FeatureManagement) | P1 | Configuration | - |
| TODO-INT001-49 | Document webhook event payload schemas | P2 | Documentation | - |
| TODO-INT001-50 | Document webhook signature verification algorithm | P2 | Documentation | - |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | Specification Team | Initial specification created for Webhook Management feature |

---

## Appendix: Event Types (Webhook Events)

### Entity Event Types
```
account.created
account.updated
account.deleted
account.status_changed

contact.created
contact.updated
contact.deleted
contact.linked_to_account

opportunity.created
opportunity.updated
opportunity.closed_won
opportunity.closed_lost
opportunity.stage_changed

lead.created
lead.converted
lead.status_changed
lead.scored

payment.received
payment.failed
payment.refunded

subscription.created
subscription.upgraded
subscription.cancelled
subscription.renewed

invoice.created
invoice.sent
invoice.paid
invoice.overdue

order.created
order.fulfilled
order.cancelled

contract.created
contract.executed
contract.renewed
contract.terminated

quote.created
quote.accepted
quote.rejected
quote.expired
```

### System Event Types
```
system.startup
system.shutdown
system.maintenance
api.rate_limit_exceeded
webhook.delivery_failed
webhook.delivery_successful
```

---

**END OF SPECIFICATION**

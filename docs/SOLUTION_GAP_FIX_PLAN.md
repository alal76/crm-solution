# CRM Solution Gap Fix Plan

> **Created:** February 7, 2026  
> **Status:** Active Implementation  
> **Reference:** Load this file at the start of gap fix sessions

---

## Executive Summary

This plan addresses **12 missing service implementations**, **16 infrastructure port stubs**, and associated test coverage gaps identified in the CRM Solution codebase.

**Current State:**
- 2854 unit tests passing (CRM.Tests.Unit.Core)
- 82% core interface coverage (41/50 implemented)
- 100% ITSM interface coverage
- 100% pluggable provider coverage

**Target State:**
- 100% core interface coverage
- Full entity service coverage
- 80%+ unit test coverage across all modules

---

## Phase 1: Critical Service Implementations (Week 1)

### Batch 1.1: Note & Task Services (Priority: CRITICAL)

These are core CRM functionality used throughout the application.

#### 1.1.1 NoteService Implementation

**Interface:** `CRM.Core/Interfaces/INoteService.cs`

```
Methods to implement:
- GetNotesAsync(entityType, entityId) 
- GetNoteByIdAsync(id)
- CreateNoteAsync(noteDto)
- UpdateNoteAsync(id, noteDto)
- DeleteNoteAsync(id)
- SearchNotesAsync(searchTerm)
```

**Implementation Location:** `CRM.Infrastructure/Services/NoteService.cs`

**Dependencies:**
- `IRepository<Note>`
- `ICurrentUserService`
- `ICrmDbContext`

**Tests Required:**
- Unit: NoteServiceTests.cs (~15 tests)
- Integration: NoteServiceIntegrationTests.cs (~8 tests)

#### 1.1.2 TaskService Implementation

**Interface:** `CRM.Core/Interfaces/ITaskService.cs`

```
Methods to implement:
- GetTasksAsync(userId, filters)
- GetTaskByIdAsync(id)
- CreateTaskAsync(taskDto)
- UpdateTaskAsync(id, taskDto)
- DeleteTaskAsync(id)
- AssignTaskAsync(taskId, userId)
- CompleteTaskAsync(taskId)
- GetOverdueTasksAsync()
```

**Implementation Location:** `CRM.Infrastructure/Services/TaskService.cs`

**Dependencies:**
- `IRepository<CrmTask>`
- `ICurrentUserService`
- `INotificationPort` (for task notifications)

**Tests Required:**
- Unit: TaskServiceTests.cs (~20 tests)
- Integration: TaskServiceIntegrationTests.cs (~10 tests)

### Batch 1.2: Quote Service (Priority: CRITICAL)

**Interface:** `CRM.Core/Interfaces/IQuoteService.cs`

```
Methods to implement:
- GetQuotesAsync(filters)
- GetQuoteByIdAsync(id)
- CreateQuoteAsync(quoteDto)
- UpdateQuoteAsync(id, quoteDto)
- DeleteQuoteAsync(id)
- AddLineItemAsync(quoteId, lineItemDto)
- RemoveLineItemAsync(quoteId, lineItemId)
- CalculateTotalsAsync(quoteId)
- ConvertToOrderAsync(quoteId)
- CloneQuoteAsync(quoteId)
- SendForApprovalAsync(quoteId)
```

**Implementation Location:** `CRM.Infrastructure/Services/QuoteService.cs`

**Dependencies:**
- `IRepository<Quote>`
- `IRepository<QuoteLineItem>`
- `IProductService`
- `IAccountService`
- `ISignaturePort` (for e-signatures)

**Tests Required:**
- Unit: QuoteServiceTests.cs (~25 tests)
- Integration: QuoteServiceIntegrationTests.cs (~12 tests)

---

## Phase 2: High Priority Services (Week 2)

### Batch 2.1: Dashboard & Pipeline Services

#### 2.1.1 DashboardService Implementation

**Interface:** `CRM.Core/Interfaces/IDashboardService.cs`

```
Methods to implement:
- GetDashboardDataAsync(userId)
- GetSalesPipelineMetricsAsync()
- GetAccountHealthMetricsAsync()
- GetActivitySummaryAsync(dateRange)
- GetLeadConversionMetricsAsync()
- GetRevenueMetricsAsync(dateRange)
- GetTaskSummaryAsync(userId)
```

**Implementation Location:** `CRM.Infrastructure/Services/DashboardService.cs`

**Tests Required:**
- Unit: DashboardServiceTests.cs (~18 tests)

#### 2.1.2 PipelineService Implementation

**Interface:** `CRM.Core/Interfaces/IPipelineService.cs`

```
Methods to implement:
- GetPipelinesAsync()
- GetPipelineByIdAsync(id)
- CreatePipelineAsync(pipelineDto)
- UpdatePipelineAsync(id, pipelineDto)
- DeletePipelineAsync(id)
- GetStagesAsync(pipelineId)
- AddStageAsync(pipelineId, stageDto)
- ReorderStagesAsync(pipelineId, stageOrder)
```

**Implementation Location:** `CRM.Infrastructure/Services/PipelineService.cs`

**Tests Required:**
- Unit: PipelineServiceTests.cs (~15 tests)

### Batch 2.2: Interaction Service

**Interface:** `CRM.Core/Interfaces/IInteractionService.cs`

```
Methods to implement:
- LogInteractionAsync(interactionDto)
- GetInteractionsAsync(entityType, entityId)
- GetRecentInteractionsAsync(limit)
- GetInteractionTimelineAsync(accountId)
- UpdateInteractionAsync(id, interactionDto)
- DeleteInteractionAsync(id)
```

**Implementation Location:** `CRM.Infrastructure/Services/InteractionService.cs`

**Tests Required:**
- Unit: InteractionServiceTests.cs (~12 tests)

---

## Phase 3: Medium Priority Services (Week 3)

### Batch 3.1: Communication & Import/Export

#### 3.1.1 CommunicationService Implementation

**Interface:** `CRM.Core/Interfaces/ICommunicationService.cs`

```
Methods to implement:
- SendEmailAsync(emailDto)
- SendSmsAsync(smsDto)
- GetCommunicationHistoryAsync(contactId)
- LogCommunicationAsync(communicationDto)
- ScheduleCommunicationAsync(scheduledDto)
- GetPendingCommunicationsAsync()
```

**Implementation Location:** `CRM.Infrastructure/Services/CommunicationService.cs`

**Dependencies:**
- `INotificationPort`
- `IChatPort`

**Tests Required:**
- Unit: CommunicationServiceTests.cs (~15 tests)

#### 3.1.2 ImportExportService Implementation

**Interface:** `CRM.Core/Interfaces/IImportExportService.cs`

```
Methods to implement:
- ImportDataAsync(entityType, fileStream, format)
- ExportDataAsync(entityType, filters, format)
- ValidateImportAsync(entityType, fileStream)
- GetImportTemplateAsync(entityType, format)
- GetImportStatusAsync(jobId)
- CancelImportAsync(jobId)
```

**Implementation Location:** `CRM.Infrastructure/Services/ImportExportService.cs`

**Tests Required:**
- Unit: ImportExportServiceTests.cs (~18 tests)

### Batch 3.2: Webhook Service

**Interface:** `CRM.Core/Interfaces/IWebhookService.cs`

```
Methods to implement:
- RegisterWebhookAsync(webhookDto)
- GetWebhooksAsync()
- UpdateWebhookAsync(id, webhookDto)
- DeleteWebhookAsync(id)
- TriggerWebhookAsync(eventType, payload)
- GetWebhookLogsAsync(webhookId)
```

**Implementation Location:** `CRM.Infrastructure/Services/WebhookService.cs`

**Dependencies:**
- `IIntegrationPort`

**Tests Required:**
- Unit: WebhookServiceTests.cs (~12 tests)

---

## Phase 4: Entity Services (Week 4)

### Batch 4.1: Financial Entity Services

| Service | Interface | Implementation |
|---------|-----------|----------------|
| InvoiceService | Create IInvoiceService | InvoiceService.cs |
| PaymentService | Create IPaymentService | PaymentService.cs |
| OrderService | Create IOrderService | OrderService.cs |
| ContractService | Create IContractService | ContractService.cs |
| SubscriptionService | Create ISubscriptionService | SubscriptionService.cs |

**Tests Required per service:** ~12-15 unit tests

### Batch 4.2: Supporting Entity Services

| Service | Interface | Implementation |
|---------|-----------|----------------|
| TeamService | Create ITeamService | TeamService.cs |
| CommissionService | Create ICommissionService | CommissionService.cs |
| EmailTemplateService | Create IEmailTemplateService | EmailTemplateService.cs |

---

## Phase 5: Test Coverage (Ongoing)

### Unit Tests by Service

| Service | Test File | Est. Tests | Priority |
|---------|-----------|------------|----------|
| NoteService | NoteServiceTests.cs | 15 | P1 |
| TaskService | TaskServiceTests.cs | 20 | P1 |
| QuoteService | QuoteServiceTests.cs | 25 | P1 |
| DashboardService | DashboardServiceTests.cs | 18 | P2 |
| PipelineService | PipelineServiceTests.cs | 15 | P2 |
| InteractionService | InteractionServiceTests.cs | 12 | P2 |
| CommunicationService | CommunicationServiceTests.cs | 15 | P3 |
| ImportExportService | ImportExportServiceTests.cs | 18 | P3 |
| WebhookService | WebhookServiceTests.cs | 12 | P3 |
| Financial Services (5) | Various | 60 | P4 |

**Total New Unit Tests:** ~210

### Integration Tests

| Test Suite | Coverage | Est. Tests |
|------------|----------|------------|
| NoteServiceIntegrationTests | CRUD + Search | 8 |
| TaskServiceIntegrationTests | Lifecycle + Notifications | 10 |
| QuoteServiceIntegrationTests | Full workflow | 12 |
| DashboardServiceIntegrationTests | Data aggregation | 6 |
| ImportExportIntegrationTests | File handling | 10 |

**Total New Integration Tests:** ~46

### E2E Tests

| Test Suite | Coverage |
|------------|----------|
| notes.spec.ts | Create/Edit/Delete notes |
| tasks.spec.ts | Task management workflow |
| quotes.spec.ts | Quote to order conversion |
| dashboard.spec.ts | Dashboard widgets |
| import-export.spec.ts | Data import/export |

**Total New E2E Tests:** ~25

---

## Implementation Checklist

### Phase 1 (Week 1) - Critical
- [ ] 1.1.1 Create NoteService.cs
- [ ] 1.1.1 Register NoteService in DI
- [ ] 1.1.1 Create NoteServiceTests.cs
- [ ] 1.1.2 Create TaskService.cs
- [ ] 1.1.2 Register TaskService in DI
- [ ] 1.1.2 Create TaskServiceTests.cs
- [ ] 1.2 Create QuoteService.cs
- [ ] 1.2 Register QuoteService in DI
- [ ] 1.2 Create QuoteServiceTests.cs
- [ ] 1.x Commit batch 1 with tests

### Phase 2 (Week 2) - High Priority
- [ ] 2.1.1 Create DashboardService.cs
- [ ] 2.1.1 Create DashboardServiceTests.cs
- [ ] 2.1.2 Create PipelineService.cs
- [ ] 2.1.2 Create PipelineServiceTests.cs
- [ ] 2.2 Create InteractionService.cs
- [ ] 2.2 Create InteractionServiceTests.cs
- [ ] 2.x Commit batch 2 with tests

### Phase 3 (Week 3) - Medium Priority
- [ ] 3.1.1 Create CommunicationService.cs
- [ ] 3.1.1 Create CommunicationServiceTests.cs
- [ ] 3.1.2 Create ImportExportService.cs
- [ ] 3.1.2 Create ImportExportServiceTests.cs
- [ ] 3.2 Create WebhookService.cs
- [ ] 3.2 Create WebhookServiceTests.cs
- [ ] 3.x Commit batch 3 with tests

### Phase 4 (Week 4) - Entity Services
- [ ] 4.1 Create IInvoiceService + InvoiceService
- [ ] 4.1 Create IPaymentService + PaymentService
- [ ] 4.1 Create IOrderService + OrderService
- [ ] 4.1 Create IContractService + ContractService
- [ ] 4.1 Create ISubscriptionService + SubscriptionService
- [ ] 4.2 Create ITeamService + TeamService
- [ ] 4.2 Create ICommissionService + CommissionService
- [ ] 4.2 Create IEmailTemplateService + EmailTemplateService
- [ ] 4.x Unit tests for all Phase 4 services
- [ ] 4.x Commit batch 4 with tests

### Phase 5 (Week 5) - Integration & E2E
- [ ] 5.1 Integration tests for Phase 1-2 services
- [ ] 5.2 Integration tests for Phase 3-4 services
- [ ] 5.3 E2E tests for critical paths
- [ ] 5.4 Frontend component tests
- [ ] 5.5 Update TESTING_SUMMARY.md
- [ ] 5.x Final commit with all tests

---

## DI Registration Template

Add to `Program.cs`:

```csharp
// Phase 1: Critical Services
services.AddScoped<INoteService, NoteService>();
services.AddScoped<ITaskService, TaskService>();
services.AddScoped<IQuoteService, QuoteService>();

// Phase 2: High Priority Services
services.AddScoped<IDashboardService, DashboardService>();
services.AddScoped<IPipelineService, PipelineService>();
services.AddScoped<IInteractionService, InteractionService>();

// Phase 3: Medium Priority Services
services.AddScoped<ICommunicationService, CommunicationService>();
services.AddScoped<IImportExportService, ImportExportService>();
services.AddScoped<IWebhookService, WebhookService>();

// Phase 4: Entity Services
services.AddScoped<IInvoiceService, InvoiceService>();
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IContractService, ContractService>();
services.AddScoped<ISubscriptionService, SubscriptionService>();
services.AddScoped<ITeamService, TeamService>();
services.AddScoped<ICommissionService, CommissionService>();
services.AddScoped<IEmailTemplateService, EmailTemplateService>();
```

---

## Success Metrics

| Metric | Current | Target | Timeline |
|--------|---------|--------|----------|
| Interface Coverage | 82% | 100% | Week 4 |
| Unit Test Count | 2854 | 3100+ | Week 5 |
| Service Implementation | 41 | 50+ | Week 4 |
| Build Errors | 0 | 0 | Always |

---

## Reference Documents

- [SOLUTION_CONTEXT.md](../SOLUTION_CONTEXT.md) - Technical context
- [ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md) - Architecture patterns
- [TESTING_SUMMARY.md](TESTING_SUMMARY.md) - Test documentation
- [copilot-instructions.md](../.github/copilot-instructions.md) - Development standards

---

**END OF GAP FIX PLAN**

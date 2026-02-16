# ITSM Module - Architecture Overview

> **Last Updated:** February 2026  
> **Module Status:** Core services implemented, Advanced services deferred  
> **ADR Reference:** N/A (built as extension of Service Desk module)

---

## 1. Overview

The ITSM (IT Service Management) module extends the CRM's Service Desk capabilities with ITIL-aligned processes. It provides Incident, Problem, Change, Configuration (CMDB), Knowledge, Service Catalog, and SLA management — all integrated with the core CRM entities (Accounts, Contacts, Users).

### Module Scope

| Process Area | ITIL Practice | Implementation Status |
|--------------|--------------|----------------------|
| Incident Management | Service Operation | ✅ Core Complete |
| Problem Management | Service Operation | ✅ Core Complete |
| Change Management | Service Transition | ✅ Core Complete |
| CMDB | Service Transition | ✅ Core Complete |
| Knowledge Management | Service Transition | ✅ Core Complete |
| Service Catalog | Service Design | ✅ Core Complete |
| SLA Management | Service Design | ✅ Core Complete |
| Asset Lifecycle | Service Transition | ❌ Deferred (ITSM_ADVANCED) |
| Capacity Management | Service Design | ❌ Deferred (ITSM_ADVANCED) |
| Release Management | Service Transition | ❌ Deferred (ITSM_ADVANCED) |

---

## 2. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ITSM Module                                      │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                      API Controllers                              │   │
│  │  ┌──────────────┐ ┌───────────────┐ ┌──────────────────────────┐ │   │
│  │  │ Incidents    │ │ ITSMControllers│ │ KnowledgeAndCatalog     │ │   │
│  │  │ Controller   │ │ (Problems,    │ │ Controllers (Knowledge, │ │   │
│  │  │ (.cs)        │ │  CMDB,Changes)│ │  Catalog, SLA)          │ │   │
│  │  └──────────────┘ └───────────────┘ └──────────────────────────┘ │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                │                                         │
│                                ▼                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Service Interfaces                              │   │
│  │  CRM.Core/Interfaces/ITSM/                                       │   │
│  │  ├── IIncidentService          ├── IServiceCatalogService        │   │
│  │  ├── IProblemService           ├── ISLAService                   │   │
│  │  ├── IChangeManagementService  ├── IKnowledgeManagementService   │   │
│  │  └── ICMDBService                                                 │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                │                                         │
│                                ▼                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                  Service Implementations                          │   │
│  │  CRM.Infrastructure/Services/ITSM/                                │   │
│  │  ├── IncidentService.cs        ├── ServiceCatalogService.cs      │   │
│  │  ├── ProblemService.cs         ├── SLAService.cs                 │   │
│  │  ├── ChangeManagementService.cs├── KnowledgeManagementService.cs │   │
│  │  ├── CMDBService.cs            ├── BusinessHoursCalculator.cs    │   │
│  │  └── SLAEnforcementHostedService.cs (background)                 │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                │                                         │
│                                ▼                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Entity Model (CRM.Core/Entities/ITSM/)         │   │
│  │  ├── Incident           ├── CatalogItem         ├── SLAPolicy    │   │
│  │  ├── Problem            ├── CatalogRequest      ├── SLATarget    │   │
│  │  ├── Change             ├── KnowledgeArticle    ├── SLAInstance  │   │
│  │  ├── ConfigurationItem  ├── ArticleRelationship ├── BusinessHrs  │   │
│  │  ├── CIRelationship     ├── BlackoutPeriod      └───────────────│   │
│  │  └── ...                                                          │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                │                                         │
│                                ▼                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Database (CrmDbContext)                         │   │
│  │  DbSets: Incidents, Problems, Changes, ConfigurationItems,       │   │
│  │  CIRelationships, KnowledgeArticles, ArticleRelationships,       │   │
│  │  CatalogItems, CatalogRequests, SLAPolicies, SLATargets,        │   │
│  │  SLAInstances, BlackoutPeriods, BusinessHoursConfigs             │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Service Responsibilities

### 3.1 IncidentService

Manages the lifecycle of IT incidents from creation to resolution.

| Operation | Description |
|-----------|-------------|
| `CreateIncidentAsync` | Create incident, auto-assign SLA policy |
| `GetIncidentsAsync` | Filtered/paginated list (status, priority, assignee) |
| `UpdateIncidentAsync` | Update incident details |
| `AssignIncidentAsync` | Assign to user/team |
| `EscalateIncidentAsync` | Escalate priority with reason |
| `ResolveIncidentAsync` | Mark resolved with resolution notes |
| `CloseIncidentAsync` | Close after confirmation |
| `ReopenIncidentAsync` | Reopen with reason |
| `LinkToProblemsAsync` | Link incident to root-cause problem |

**SLA Integration:** On creation, the service auto-attaches an SLA policy based on priority/category. SLA timers start immediately.

### 3.2 ProblemService

Tracks root causes and known errors across multiple incidents.

| Operation | Description |
|-----------|-------------|
| `CreateProblemAsync` | Create problem record |
| `GetProblemsAsync` | Filtered list |
| `UpdateProblemAsync` | Update details |
| `AssignProblemAsync` | Assign investigator |
| `AddRootCauseAsync` | Record root cause analysis |
| `MarkAsKnownErrorAsync` | Promote to known error with workaround |
| `ResolveProblemAsync` | Mark resolved |
| `LinkIncidentAsync` | Link related incidents |
| `GetLinkedIncidentsAsync` | Get all incidents caused by this problem |

### 3.3 ChangeManagementService

Controls the lifecycle of IT changes with approval workflows.

| Operation | Description |
|-----------|-------------|
| `CreateChangeAsync` | Create change request (Normal, Standard, Emergency) |
| `GetChangesAsync` | Filtered/paginated list |
| `SubmitForApprovalAsync` | Submit to CAB for review |
| `ApproveChangeAsync` | Record CAB approval |
| `RejectChangeAsync` | Record CAB rejection with reason |
| `ScheduleChangeAsync` | Set implementation window |
| `ImplementChangeAsync` | Begin implementation |
| `CompleteChangeAsync` | Mark implementation complete |
| `RollbackChangeAsync` | Rollback failed change |
| `GetImpactedCIsAsync` | CIs affected by this change |
| `CheckConflictsAsync` | Check scheduling conflicts |
| `GetBlackoutPeriodsAsync` | Periods when changes are forbidden |
| `GetChangeCalendar` | Combined view of changes + blackouts |

### 3.4 CMDBService

Maintains the Configuration Management Database of IT assets and their relationships.

| Operation | Description |
|-----------|-------------|
| `GetConfigurationItemsAsync` | Filtered CI list (type, status, environment) |
| `GetConfigurationItemByIdAsync` | CI details with relationships |
| `CreateConfigurationItemAsync` | Register new CI |
| `UpdateConfigurationItemAsync` | Update CI details |
| `DeleteConfigurationItemAsync` | Decommission CI (soft delete) |
| `GetRelationshipsAsync` | Get CI dependency graph |
| `AddRelationshipAsync` | Link CIs (depends_on, runs_on, etc.) |
| `RemoveRelationshipAsync` | Remove CI link |
| `GetDependenciesAsync` | Upstream dependencies |
| `GetDependentsAsync` | Downstream dependents |
| `GetImpactAnalysisAsync` | Impact analysis for a CI failure |

### 3.5 KnowledgeManagementService

Manages knowledge articles for self-service and agent assistance.

| Operation | Description |
|-----------|-------------|
| `GetArticlesAsync` | Filtered/paginated article list |
| `GetArticleByIdAsync` | Article with metadata |
| `CreateArticleAsync` | Create article (draft status) |
| `UpdateArticleAsync` | Update content |
| `PublishArticleAsync` | Publish for consumption |
| `ArchiveArticleAsync` | Archive outdated article |
| `SearchArticlesAsync` | Full-text search across articles |
| `AddFeedbackAsync` | Record helpful/unhelpful feedback |
| `GetPopularArticlesAsync` | Most viewed/helpful articles |
| `GetRelatedArticlesAsync` | Articles linked to a topic |

**AI Integration (TODO):** Semantic search via embeddings is planned but not yet implemented. Currently uses SQL `LIKE` matching.

### 3.6 ServiceCatalogService

Provides a self-service portal for requesting IT services.

| Operation | Description |
|-----------|-------------|
| `GetCatalogItemsAsync` | Browse items, optionally by category |
| `GetCatalogItemByIdAsync` | Item details with form schema |
| `CreateCatalogRequestAsync` | Submit a request for an item |
| `CreateCatalogRequestForOthersAsync` | Submit on behalf of another user |
| `GetMyRequestsAsync` | User's submitted requests |
| `GetRequestByIdAsync` | Request details |
| `CancelRequestAsync` | Cancel pending request |
| `SearchCatalogAsync` | Search items by term |
| `GetCategoriesAsync` | List categories with counts |

### 3.7 SLAService + SLAEnforcementHostedService

Tracks SLA compliance with automatic breach detection.

| Operation | Description |
|-----------|-------------|
| `CreateSLAPolicyAsync` | Define SLA policy with targets |
| `GetSLAPoliciesAsync` | List policies (by target type) |
| `GetSLAInstanceAsync` | Active SLA for a target |
| `CheckSLABreachesAsync` | Scan for breached SLAs |
| `PauseSLAAsync` | Pause timer (awaiting customer) |
| `ResumeSLAAsync` | Resume timer |
| `GetBreachedSLAsAsync` | All currently breached SLAs |
| `GetAtRiskSLAsAsync` | SLAs nearing breach threshold |
| `GetSLADashboardAsync` | Summary dashboard data |
| `GetSLAMetricsAsync` | Compliance metrics by date range |

**Background Service:** `SLAEnforcementHostedService` runs as a hosted background service, periodically scanning for SLA breaches and sending notifications.

**Business Hours:** `BusinessHoursCalculator` computes elapsed business-hours time, excluding weekends and holidays. Currently uses a default schedule; TODO: load custom schedules from database.

---

## 4. Entity Model

### Core ITSM Entities

```
Incident
├── Id, Title, Description
├── Priority (1-Critical, 2-High, 3-Medium, 4-Low)
├── Status (New, InProgress, OnHold, Resolved, Closed)
├── Category, Subcategory
├── ReportedById (→ Users.Id)
├── AssignedToId (→ Users.Id)
├── AccountId (→ Customers.Id)
├── SLAPolicyId (→ SLAPolicies.Id)
├── ResolutionNotes, EscalationReason
└── CreatedAt, UpdatedAt, ResolvedAt, ClosedAt

Problem
├── Id, Title, Description
├── Priority, Status (New, Investigating, KnownError, Resolved)
├── RootCause, Workaround
├── AssignedToId, AccountId
├── LinkedIncidents (via ProblemIncidentLinks)
└── CreatedAt, UpdatedAt, ResolvedAt

Change
├── Id, Title, Description
├── ChangeType (Normal, Standard, Emergency)
├── Priority, Status (Draft → Submitted → Approved → Scheduled → Implementing → Completed)
├── Risk, Impact
├── PlannedStart, PlannedEnd, ActualStart, ActualEnd
├── ImplementationPlan, BackoutPlan, TestPlan
├── RequestedById, AssignedToId, ApprovedById
├── ImpactedCIs (via ChangeImpactedCIs)
└── CreatedAt, UpdatedAt

ConfigurationItem (CI)
├── Id, Name, Type (Server, Application, Network, Database, Service, etc.)
├── Status (Active, Inactive, Maintenance, Retired)
├── Environment (Production, Staging, Development, Testing)
├── Owner, Location, Criticality
├── Relationships (via CIRelationship: depends_on, runs_on, connects_to, etc.)
└── CreatedAt, UpdatedAt

KnowledgeArticle
├── Id, Title, Content (HTML)
├── Category, Tags
├── Status (Draft, Published, Archived)
├── ViewCount, HelpfulCount, NotHelpfulCount
├── AuthorId, PublishedAt
├── RelatedArticles (via ArticleRelationship)
└── CreatedAt, UpdatedAt

SLAPolicy
├── Id, Name, Description
├── TargetType (Incident, ServiceRequest)
├── Targets (via SLATarget: response time, resolution time by priority)
└── IsActive, CreatedAt

SLAInstance
├── Id, SLAPolicyId
├── TargetId, TargetType
├── ResponseDeadline, ResolutionDeadline
├── ResponseBreached, ResolutionBreached
├── PausedAt, TotalPausedMinutes
└── CreatedAt, UpdatedAt
```

### Relationship Diagram

```
Users ─────┐
           ├──▶ Incident ──▶ SLAInstance ──▶ SLAPolicy
Customers ─┘       │                            │
                    │                            ▼
                    ▼                        SLATarget
                Problem
                    │
                    ▼
                Change ──▶ ConfigurationItem
                               │
                               ▼
                          CIRelationship
                          (depends_on,
                           runs_on, etc.)

KnowledgeArticle ──▶ ArticleRelationship
                     (related_to, supersedes, etc.)

CatalogItem ──▶ CatalogRequest ──▶ Users (RequestedById, RequestedForId)
```

---

## 5. API Routes

All ITSM endpoints are prefixed with `/api/itsm/`.

| Controller | Route Prefix | Methods |
|------------|-------------|---------|
| IncidentsController | `/api/itsm/incidents` | GET (list), GET /{id}, POST, PUT /{id}, POST /{id}/assign, POST /{id}/escalate, POST /{id}/resolve, POST /{id}/close, POST /{id}/reopen, POST /{id}/comments, GET /{id}/comments |
| ProblemsController | `/api/itsm/problems` | GET (list), GET /{id}, POST, PUT /{id}, POST /{id}/assign, POST /{id}/root-cause, POST /{id}/known-error, POST /{id}/resolve, POST /{id}/link-incident, GET /{id}/incidents |
| CMDBController | `/api/itsm/cmdb` | GET /cis (list), GET /cis/{id}, POST /cis, PUT /cis/{id}, DELETE /cis/{id}, GET /cis/{id}/relationships, POST /cis/{id}/relationships, DELETE /cis/relationships/{id}, GET /cis/{id}/dependencies, GET /cis/{id}/dependents, GET /cis/{id}/impact |
| ChangesController | `/api/itsm/changes` | GET (list), GET /{id}, POST, PUT /{id}, POST /{id}/submit, POST /{id}/approve, POST /{id}/reject, POST /{id}/schedule, POST /{id}/implement, POST /{id}/complete, POST /{id}/rollback, GET /{id}/impacted-cis, POST /{id}/check-conflicts, GET /blackouts, POST /blackouts, GET /calendar |
| KnowledgeController | `/api/itsm/knowledge` | GET /articles (list), GET /articles/{id}, POST /articles, PUT /articles/{id}, POST /articles/{id}/publish, POST /articles/{id}/archive, GET /articles/search, POST /articles/{id}/feedback, GET /articles/popular, GET /articles/{id}/related, GET /categories |
| CatalogController | `/api/itsm/catalog` | GET /items, GET /items/{id}, POST /requests, GET /my-requests, GET /search, GET /featured, GET /categories, POST /requests/for-others, GET /requests/{requestId}, PATCH /requests/{requestId}/cancel |
| SLAController | `/api/itsm/sla` | POST /policies, GET /policies, GET /policies/{id}, GET /instances/{targetId}/{targetType}, GET /breached, POST /check-breaches, POST /instances/{targetId}/{targetType}/pause, POST /instances/{targetId}/{targetType}/resume, GET /dashboard, GET /at-risk, GET /metrics |

---

## 6. Integration with Core CRM

### Account/Contact Linking

- Incidents and Problems link to `Customers` (Accounts) via `AccountId`
- Incidents link to `Contacts` via `ContactId`
- Catalog requests are created by `Users` and optionally on behalf of other users

### Activity Timeline

- Incident status changes, SLA breaches, and chat messages create `Activity` records
- Activities appear in the Account and Contact timeline views

### SLA & Notifications

- SLA breaches trigger notifications via `INotificationPort` (BuiltIn, Novu, Twilio, SendGrid)
- Escalation rules can auto-reassign incidents

### Feature Flags

The ITSM module is gated by the `EnableITSM` feature flag in `FeatureManagement`:

```json
{
  "FeatureManagement": {
    "EnableITSM": true
  }
}
```

---

## 7. Frontend Architecture

### Pages (React + MUI)

| Page | Route | Description |
|------|-------|-------------|
| IncidentListPage | `/itsm/incidents` | Paginated list with filters |
| IncidentDetailPage | `/itsm/incidents/:id` | Detail view with timeline |
| ProblemListPage | `/itsm/problems` | Problem records |
| ProblemDetailPage | `/itsm/problems/:id` | Root cause analysis view |
| ChangeListPage | `/itsm/changes` | Change requests |
| ChangeDetailPage | `/itsm/changes/:id` | Approval workflow view |
| CMDBListPage | `/itsm/cmdb` | Configuration items |
| CMDBDetailPage | `/itsm/cmdb/:id` | CI relationships graph |
| KnowledgeListPage | `/itsm/knowledge` | Article library |
| KnowledgeArticleDetailPage | `/itsm/knowledge/:id` | Article with feedback |
| ServiceCatalogPage | `/itsm/catalog` | Self-service catalog |
| ServiceCatalogRequestCreatePage | `/itsm/catalog/request/:id` | Request form |
| SLADashboardPage | `/itsm/sla` | SLA compliance dashboard |
| IncidentFormPage | `/itsm/incidents/new` | Create incident form |

### API Service Layer

All ITSM API calls are centralized in `CRM.Frontend/src/services/itsmService.ts`, which provides 8 typed service objects:

- `incidentService` — CRUD + lifecycle operations
- `problemService` — CRUD + root cause + known errors
- `changeService` — CRUD + approval workflow + calendar
- `cmdbService` — CRUD + relationships + impact analysis
- `knowledgeService` — CRUD + search + feedback
- `catalogService` — Browse + request + track
- `slaService` — Policies + instances + dashboard + metrics
- `itsmDashboardService` — Aggregated dashboard data

### Reusable Components

| Component | Purpose |
|-----------|---------|
| SLACountdownWidget | Real-time SLA timer display |
| ImpactUrgencyMatrix | Priority matrix selector |
| ApprovalWorkflowPanel | Change approval status |
| RelationshipDiagram | CI dependency graph |
| ChangeCalendar | Calendar view of changes |
| KnowledgeSearchBar | Article search |
| ArticleFeedbackWidget | Helpful/not helpful buttons |
| ServiceCatalogBrowser | Category-based catalog browsing |

### Note on Styling

Currently, 31 ITSM pages use Tailwind CSS classes. A migration to MUI components is tracked in TODO-AUDIT-07.

---

## 8. Database Schema

ITSM tables are created by migration `010_itsm_module.sql` with seed data in `011_itsm_seed_data.sql`.

Key tables: `Incidents`, `Problems`, `Changes`, `ConfigurationItems`, `CIRelationships`, `KnowledgeArticles`, `ArticleRelationships`, `CatalogItems`, `CatalogRequests`, `SLAPolicies`, `SLATargets`, `SLAInstances`, `BlackoutPeriods`, `BusinessHoursConfigs`.

See [DATABASE_SCHEMA.md](../../database/DATABASE_SCHEMA.md) for complete column definitions.

---

## 9. Test Coverage

| Test File | Tests | Coverage |
|-----------|-------|----------|
| IncidentServiceTests.cs | 14 | Core CRUD + lifecycle |
| ProblemServiceTests.cs | 12 | CRUD + root cause + known errors |
| ChangeManagementServiceTests.cs | 16 | CRUD + approval workflow |
| CMDBServiceTests.cs | 12 | CRUD + relationships |
| KnowledgeManagementServiceTests.cs | 13 | CRUD + search + feedback |
| ServiceCatalogServiceTests.cs | 14 | CRUD + requests |
| SLAServiceTests.cs | 14 | Policies + breach detection |
| SLAEnforcementHostedServiceTests.cs | 16 | Background processing |
| ITSMIncidentsControllerTests.cs | 12 | Controller integration |
| ITSMProblemsControllerTests.cs | 12 | Controller integration |
| ITSMChangesControllerTests.cs | 14 | Controller integration |
| ITSMCMDBControllerTests.cs | 14 | Controller integration |
| ITSMKnowledgeControllerTests.cs | 14 | Controller integration |
| ITSMCatalogControllerTests.cs | 14 | Controller integration |
| ITSMSLAControllerTests.cs | 14 | Controller integration |
| **Total** | **195** | |

---

## 10. Deferred Work (ITSM_ADVANCED)

28 advanced ITSM services are defined but have 460+ build errors due to entity model mismatches. They are excluded from the build via `Directory.Build.props` (commented out). These include:

- AssetLifecycleService, CapacityManagementService, ReleaseManagementService
- KCSWorkflowService, ImpactAnalysisService, CABWorkflowService
- AutomatedCategorizationService, MajorIncidentService
- ServiceDependencyMapService, and more

See TODO-ITSM-01 through TODO-ITSM-02 in [MASTER_TODO_LIST.md](../MASTER_TODO_LIST.md).

---

## Related Documentation

- [ARCHITECTURE_OVERVIEW.md](../development/ARCHITECTURE_OVERVIEW.md) — Solution-wide architecture
- [ITSM_IMPLEMENTATION_STATUS.md](../status/ITSM_IMPLEMENTATION_STATUS.md) — Detailed status
- [ITSM_USER_GUIDE.md](../ITSM_USER_GUIDE.md) — End-user guide
- [SOLUTION_GAPS_REMEDIATION_PLAN.md](docs/development/SOLUTION_GAPS_REMEDIATION_PLAN.md) — Remaining work

---

**END OF ITSM ARCHITECTURE OVERVIEW**

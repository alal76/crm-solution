# ITSM Problem Management Specification

> **Spec ID:** SPEC-ITSM-002  
> **Feature:** Problem Management & Root Cause Analysis  
> **Module:** ITSM  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description

Problem Management is the ITSM process for managing the lifecycle of problems—from incident investigation through root cause analysis (RCA) to permanent solution implementation and prevention. Problem Management links related incidents to reduce duplication, identifies known errors, and implements permanent fixes to prevent incident recurrence.

**Key Differences from Incident Management:**
- **Incident:** Individual occurrence requiring immediate restoration of service
- **Problem:** Underlying cause of one or more incidents requiring long-term resolution
- **Known Error:** Documented problem with a permanent solution pending implementation

**Core Activities:**
1. **Root Cause Analysis (RCA):** Systematic investigation of incident causes
2. **Known Error Creation:** Documenting problems with identified solutions
3. **Incident Linking:** Associating incidents to problems and trend tracking
4. **Change Planning:** Coordinating fixes through Change Management process
5. **Prevention:** Proactive problem identification and trend analysis

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Problem Lifecycle | Create, investigate, resolve, close problems with status tracking | ❌ |
| SF-002 | Root Cause Analysis | RCA conductor workflow with investigation tree, evidence collection, 5-Whys framework | ❌ |
| SF-003 | Known Errors | Known error registry with documented solutions and incident matching | ❌ |
| SF-004 | Incident-Problem Linking | Link/unlink incidents to problems with automatic suggestion matching | ❌ |
| SF-005 | Trend Analysis | Trend identification, cause-based metrics, recurrence prediction | ❌ |
| SF-006 | Change Integration | Link problems to change requests, track implementation status | ❌ |
| SF-007 | Problem Reports | Analytics dashboard, trend charts, top problems, resolution metrics | ❌ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Initiate RCA | Support Manager | Incident received | RCA session created with initial investigation | ❌ |
| UC-002 | Conduct Root Cause Analysis | Support Engineer | RCA session active | Root cause identified with supporting evidence | ❌ |
| UC-003 | Create Known Error | Support Manager | RCA complete with solution identified | Known error documented and published | ❌ |
| UC-004 | Link Incidents to Problem | Support Engineer | New incident received | Incident associated to existing problem; similar incidents auto-suggested | ❌ |
| UC-005 | Review Problem Trends | Manager | Problems in system | Trend dashboard shows top causes, recurrence, resolution time | ❌ |
| UC-006 | Request Change for Fix | Support Manager | Known error with permanent solution | Change request created and linked to problem | ❌ |
| UC-007 | Resolve Problem | Support Engineer | Known error implemented via change | Problem status updated to Resolved/Closed | ❌ |
| UC-008 | Prevent Recurrence | Proactive Manager | Historical problems analyzed | Preventive measures implemented; future incidents reduced | ❌ |

### 1.4 Business Rules

| Rule | Description | Enforcement |
|------|-------------|------------|
| BR-001 | Problem must have root cause documented before closure | Service validation on close |
| BR-002 | Known error must have solution documented and priority assigned | UI required field on creation |
| BR-003 | Problems can only be resolved after linked change is implemented | Status transition validation |
| BR-004 | RCA must include at least 3 "Why" levels in investigation tree | Conductor workflow enforcer |
| BR-005 | Known errors auto-link to incidents within 2 hours of error creation | Background job scheduling |
| BR-006 | Problem trend requires minimum 3 incidents in 30-day window | Analytics calculation rule |
| BR-007 | Duplicate problems must be merged with single RCA documented | Problem deduplication service |
| BR-008 | Management approval required for problems with workaround-only solutions | Approval workflow gate |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Problem List | `CRM.Frontend/src/pages/itsm/ProblemsPage.tsx` | ❌ | Filter by status, priority, trend; bulk actions |
| Problem Details | `CRM.Frontend/src/pages/itsm/ProblemDetailsPage.tsx` | ❌ | Full problem record with RCA, incidents, changes, timeline |
| RCA Workspace | `CRM.Frontend/src/pages/itsm/RCAWorkspacePage.tsx` | ❌ | Interactive investigation tree, 5-Whys conductor, evidence collection |
| Known Errors | `CRM.Frontend/src/pages/itsm/KnownErrorsBrowserPage.tsx` | ❌ | Known error registry with search, filter by product/component |
| Incident Linking | `CRM.Frontend/src/pages/itsm/ProblemIncidentLinkingPage.tsx` | ❌ | Link/unlink incidents, view suggested matches via AI |
| Trend Dashboard | `CRM.Frontend/src/pages/itsm/ProblemTrendDashboardPage.tsx` | ❌ | Top problems, trend charts, recurrence patterns, resolution metrics |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ProblemForm | `CRM.Frontend/src/components/itsm/ProblemForm.tsx` | ❌ | Create/edit problem with category, priority, impact, urgency fields |
| RCAConductor | `CRM.Frontend/src/components/itsm/RCAConductor.tsx` | ❌ | Interactive 5-Whys tree builder with guided investigation |
| RCAEvidence | `CRM.Frontend/src/components/itsm/RCAEvidence.tsx` | ❌ | Collect logs, screenshots, system metrics as evidence |
| RCATimeline | `CRM.Frontend/src/components/itsm/RCATimeline.tsx` | ❌ | Visual timeline of incident progression with annotation capability |
| KnownErrorCard | `CRM.Frontend/src/components/itsm/KnownErrorCard.tsx` | ❌ | Displays known error summary with workaround, permanent solution |
| IncidentProblemMatrix | `CRM.Frontend/src/components/itsm/IncidentProblemMatrix.tsx` | ❌ | Similarity matrix with drag-drop incident linking |
| SuggestedProblems | `CRM.Frontend/src/components/itsm/SuggestedProblems.tsx` | ❌ | AI-powered suggestions for problem matching based on incident history |
| TrendChart | `CRM.Frontend/src/components/itsm/TrendChart.tsx` | ❌ | Render trend line charts (line, bar, pie) with drill-down capability |
| ProblemTimeline | `CRM.Frontend/src/components/itsm/ProblemTimeline.tsx` | ❌ | Display problem lifecycle: Created → RCA → KE → Change → Resolved → Closed |
| ChangeIntegration | `CRM.Frontend/src/components/itsm/ChangeIntegration.tsx` | ❌ | Show linked changes with implementation status and rollback option |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| problemService | `CRM.Frontend/src/services/itsm/problemService.ts` | GetAll, GetById, Create, Update, Delete, UpdateStatus | ❌ |
| rcaService | `CRM.Frontend/src/services/itsm/rcaService.ts` | StartRCA, SaveInvestigation, SubmitRCA, GetInvestigationTree | ❌ |
| knownErrorService | `CRM.Frontend/src/services/itsm/knownErrorService.ts` | GetAll, GetById, Create, Update, Search, GetByProblem | ❌ |
| incidentLinkingService | `CRM.Frontend/src/services/itsm/incidentLinkingService.ts` | LinkIncident, UnlinkIncident, GetLinkedIncidents, GetSuggestedProblems | ❌ |
| trendAnalysisService | `CRM.Frontend/src/services/itsm/trendAnalysisService.ts` | GetTopProblems, GetTrends, GetMetrics, GetRecurrencePattern | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Problem Title | Required, max 500 chars, no special chars | Frontend/Backend | ❌ |
| Root Cause | Required before status changes to "RCA Complete" | Frontend/Backend | ❌ |
| Solution | Required before creating Known Error | Frontend/Backend | ❌ |
| RCA Evidence | At least 1 evidence item required for RCA submission | Frontend/Backend | ❌ |
| Impact/Urgency | Valid mapping to Priority matrix (4x4 grid) | Frontend/Backend | ❌ |
| Incident Count | Minimum 1 incident must be linked to classify as Trend | Frontend/Backend | ❌ |
| Known Error Title | Required, unique per component/version | Frontend/Backend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Problem | `CRM.Core/Entities/Problem.cs` | ❌ | ProblemId, Title, Description, Status, Priority, Impact, Urgency, RootCause, Workaround, PermanentSolution, CreatedDate, TargetResolutionDate, ResolvedDate, ClosedDate |
| RootCauseAnalysis | `CRM.Core/Entities/RootCauseAnalysis.cs` | ❌ | RCAId, ProblemId, Status, InvestigationTree (nested), StartDate, CompletionDate, ConductedBy, Evidence (collection) |
| RCAEvidence | `CRM.Core/Entities/RCAEvidence.cs` | ❌ | EvidenceId, RCAId, EvidenceType (Log/Screenshot/Metric/Other), Content, FilePath, UploadedBy, UploadedAt |
| RCAInvestigationNode | `CRM.Core/Entities/RCAInvestigationNode.cs` | ❌ | NodeId, RCAId, Level (1-5 for 5-Whys), Question, Answer, Evidence (FK), ParentNodeId, CreatedAt |
| KnownError | `CRM.Core/Entities/KnownError.cs` | ❌ | KnownErrorId, ProblemId, Title, Description, Component, AffectedVersions, Workaround, PermanentSolution, ImpactedSystems, Status (Published/Internal/Deprecated) |
| ProblemIncidentLink | `CRM.Core/Entities/ProblemIncidentLink.cs` | ❌ | LinkId, ProblemId, IncidentId, LinkType (Related/Duplicate/Trend), ConfidenceScore, CreatedDate, ConfirmedBy |
| ProblemTrend | `CRM.Core/Entities/ProblemTrend.cs` | ❌ | TrendId, ProblemId, IncidentCount (30d), IncidentCountPreviousPeriod, TrendPercentage, RecurrencePattern, PredictedOccurrences |
| ProblemMetrics | `CRM.Core/Entities/ProblemMetrics.cs` | ❌ | MetricsId, ProblemId, TimeToRCA (hours), TimeToSolution (days), ResolutionRate, PreventionEffectiveness |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ProblemDto | `CRM.Core/DTOs/ProblemDto.cs` | ❌ | Flat problem record for API responses |
| CreateProblemDto | `CRM.Core/DTOs/CreateProblemDto.cs` | ❌ | Title, Category, Priority, Impact, Urgency, Description |
| UpdateProblemDto | `CRM.Core/DTOs/UpdateProblemDto.cs` | ❌ | Can update any field; status transitions validated |
| ProblemDetailsDto | `CRM.Core/DTOs/ProblemDetailsDto.cs` | ❌ | Full problem with nested RCA, KnownError, LinkedIncidents, Changes |
| RCASessionDto | `CRM.Core/DTOs/RCASessionDto.cs` | ❌ | RCA session state: ProblemId, InvestigationTree, Evidence, Status, ConductedBy |
| RCAResultDto | `CRM.Core/DTOs/RCAResultDto.cs` | ❌ | RCA output: RootCause, ConfidenceLevel, FinishedTree, RecommendedActions |
| KnownErrorDto | `CRM.Core/DTOs/KnownErrorDto.cs` | ❌ | KnownErrorId, Title, Component, Workaround, PermanentSolution, Status, LinkedProblems |
| CreateKnownErrorDto | `CRM.Core/DTOs/CreateKnownErrorDto.cs` | ❌ | ProblemId, Title, Component, Solution, AffectedVersions |
| IncidentLinkDto | `CRM.Core/DTOs/IncidentLinkDto.cs` | ❌ | IncidentId, ProblemId, LinkType, ConfidenceScore, LinkedAt |
| TrendMetricsDto | `CRM.Core/DTOs/TrendMetricsDto.cs` | ❌ | IncidentCount, TrendPercentage, RecurrencePattern, Prediction |
| ProblemSearchResultDto | `CRM.Core/DTOs/ProblemSearchResultDto.cs` | ❌ | Lightweight problem record for list views and search results |
| SuggestedProblemDto | `CRM.Core/DTOs/SuggestedProblemDto.cs` | ❌ | ProblemId, Title, MatchScore (0-100), Reason (keywords/category/symptom match) |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IProblemService | `CRM.Core/Interfaces/IProblemService.cs` | GetAll, GetById, Create, Update, UpdateStatus, Delete, GetByTrend, Search, GetProblemDetails | ❌ |
| IRCAConductor | `CRM.Core/Interfaces/IRCAConductor.cs` | StartRCA, SaveInvestigationNode, SubmitRCA, GetInvestigationTree, ValidateRCA | ❌ |
| IRCAEvidenceService | `CRM.Core/Interfaces/IRCAEvidenceService.cs` | CollectEvidence, AttachLog, AttachScreenshot, AttachMetrics, GetEvidence, DeleteEvidence | ❌ |
| IKnownErrorService | `CRM.Core/Interfaces/IKnownErrorService.cs` | GetAll, GetById, Create, Update, Publish, Deprecate, Search, MatchIncidentToKnownError | ❌ |
| IIncidentProblemLinker | `CRM.Core/Interfaces/IIncidentProblemLinker.cs` | LinkIncident, UnlinkIncident, GetLinkedIncidents, GetSuggestedProblems, CalculateMatchScore | ❌ |
| ITrendAnalyzer | `CRM.Core/Interfaces/ITrendAnalyzer.cs` | IdentifyTrend, CalculateTrendMetrics, PredictRecurrence, GetTopProblems, AnalyzeHistoricalTrends | ❌ |
| IProblemChangeIntegration | `CRM.Core/Interfaces/IProblemChangeIntegration.cs` | LinkChange, UnlinkChange, GetLinkedChanges, UpdateChangeStatus, CheckChangeImplementation | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ProblemService | `CRM.Infrastructure/Services/ProblemService.cs` | GetAll, GetById, Create, Update, UpdateStatus, Delete, GetByTrend, Search, GetProblemDetails, MergeDuplicates | ❌ |
| RCAConductor | `CRM.Infrastructure/Services/RCAConductor.cs` | StartRCA, SaveInvestigationNode, SubmitRCA, GetInvestigationTree, ValidateRCA, CalculateConfidenceScore, GenerateReport | ❌ |
| RCAEvidenceCollector | `CRM.Infrastructure/Services/RCAEvidenceCollector.cs` | CollectEvidence, AttachLog, AttachScreenshot, AttachMetrics, GetEvidence, DeleteEvidence, ValidateEvidenceIntegrity | ❌ |
| KnownErrorService | `CRM.Infrastructure/Services/KnownErrorService.cs` | GetAll, GetById, Create, Update, Publish, Deprecate, Search, MatchIncidentToKnownError, ApplySolution | ❌ |
| IncidentProblemLinker | `CRM.Infrastructure/Services/IncidentProblemLinker.cs` | LinkIncident, UnlinkIncident, GetLinkedIncidents, GetSuggestedProblems, CalculateMatchScore, AutoLinkKnownErrors | ❌ |
| TrendAnalyzer | `CRM.Infrastructure/Services/TrendAnalyzer.cs` | IdentifyTrend, CalculateTrendMetrics, PredictRecurrence, GetTopProblems, AnalyzeHistoricalTrends, GenerateTrendReport | ❌ |
| ProblemChangeIntegration | `CRM.Infrastructure/Services/ProblemChangeIntegration.cs` | LinkChange, UnlinkChange, GetLinkedChanges, UpdateChangeStatus, CheckChangeImplementation, CoordinateChangeExecution | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ProblemsController | `CRM.Api/Controllers/itsm/ProblemsController.cs` | 12 | ❌ |
| RCAController | `CRM.Api/Controllers/itsm/RCAController.cs` | 8 | ❌ |
| KnownErrorsController | `CRM.Api/Controllers/itsm/KnownErrorsController.cs` | 10 | ❌ |
| TrendAnalysisController | `CRM.Api/Controllers/itsm/TrendAnalysisController.cs` | 6 | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/itsm/problems` | GetAll | Yes | ❌ |
| GET | `/api/itsm/problems/{id}` | GetById | Yes | ❌ |
| GET | `/api/itsm/problems/{id}/details` | GetProblemDetails | Yes | ❌ |
| POST | `/api/itsm/problems` | Create | Yes | ❌ |
| PUT | `/api/itsm/problems/{id}` | Update | Yes | ❌ |
| PATCH | `/api/itsm/problems/{id}/status` | UpdateStatus | Yes | ❌ |
| DELETE | `/api/itsm/problems/{id}` | Delete | Yes | ❌ |
| GET | `/api/itsm/problems/search` | Search | Yes | ❌ |
| POST | `/api/itsm/problems/{id}/merge/{otherId}` | MergeDuplicates | Yes | ❌ |
| GET | `/api/itsm/rca/session/{problemId}` | GetRCASession | Yes | ❌ |
| POST | `/api/itsm/rca/start/{problemId}` | StartRCA | Yes | ❌ |
| POST | `/api/itsm/rca/{rcaId}/node` | SaveInvestigationNode | Yes | ❌ |
| POST | `/api/itsm/rca/{rcaId}/submit` | SubmitRCA | Yes | ❌ |
| POST | `/api/itsm/rca/{rcaId}/evidence` | AttachEvidence | Yes | ❌ |
| GET | `/api/itsm/known-errors` | GetKnownErrors | Yes | ❌ |
| GET | `/api/itsm/known-errors/{id}` | GetKnownErrorById | Yes | ❌ |
| POST | `/api/itsm/known-errors` | CreateKnownError | Yes | ❌ |
| PUT | `/api/itsm/known-errors/{id}` | UpdateKnownError | Yes | ❌ |
| PATCH | `/api/itsm/known-errors/{id}/publish` | PublishKnownError | Yes | ❌ |
| GET | `/api/itsm/incidents/{incidentId}/suggest-problems` | GetSuggestedProblems | Yes | ❌ |
| POST | `/api/itsm/incidents/{incidentId}/link-problem/{problemId}` | LinkIncidentToProblem | Yes | ❌ |
| DELETE | `/api/itsm/incidents/{incidentId}/unlink-problem/{problemId}` | UnlinkIncidentFromProblem | Yes | ❌ |
| GET | `/api/itsm/trends/top-problems` | GetTopProblems | Yes | ❌ |
| GET | `/api/itsm/trends/metrics` | GetTrendMetrics | Yes | ❌ |
| POST | `/api/itsm/trends/analyze` | AnalyzeTrends | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Problem Title | Required, 10-500 chars, no SQL injection | Entity/DTO/Service | ❌ |
| Root Cause | Required before RCA submission; min 50 chars | Service validation | ❌ |
| RCA Status Transition | Only "RCA Complete" after evidence validated | Service state machine | ❌ |
| Solution Permanence | Permanent solution required for Known Error creation | Service workflow | ❌ |
| Investigation Tree Depth | Minimum 3 levels required for 5-Whys; max 7 levels | RCAConductor | ❌ |
| Incident Link Count | Minimum 1 incident to link; max 1000 per problem | Service validation | ❌ |
| Trend Threshold | Minimum 3 incidents in 30 days to identify trend | TrendAnalyzer calculation | ❌ |
| Match Confidence Score | 0-100 range; only auto-link if > 75 | IncidentProblemLinker | ❌ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Problems | `database/schema/itsm/001_problems_table.sql` | ❌ | Main problem records with status, priority, RCA tracking |
| RootCauseAnalysis | `database/schema/itsm/002_rca_table.sql` | ❌ | RCA sessions with investigation tree storage (JSON) |
| RCAEvidence | `database/schema/itsm/003_rca_evidence_table.sql` | ❌ | Evidence items (logs, screenshots, metrics) with file references |
| RCAInvestigationNodes | `database/schema/itsm/004_rca_nodes_table.sql` | ❌ | 5-Whys investigation tree nodes with hierarchical structure |
| KnownErrors | `database/schema/itsm/005_known_errors_table.sql` | ❌ | Known error registry with solutions and affected systems |
| ProblemIncidentLinks | `database/schema/itsm/006_problem_incident_links.sql` | ❌ | Junction: Problems ↔ Incidents with match confidence |
| ProblemTrends | `database/schema/itsm/007_problem_trends_table.sql` | ❌ | Trend metrics: incident count, recurrence, prediction |
| ProblemMetrics | `database/schema/itsm/008_problem_metrics_table.sql` | ❌ | Performance metrics: RCA time, resolution rate, effectiveness |
| ProblemChangeLinks | `database/schema/itsm/009_problem_change_links.sql` | ❌ | Junction: Problems ↔ Changes for fix coordination |

### 4.2 Data Elements - Problems Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | ProblemId | ✅ |
| Title | VARCHAR(500) | No | | UNIQUE(Title, CreatedDate) | Title | ❌ |
| Description | TEXT | Yes | | | Description | ❌ |
| Category | VARCHAR(100) | Yes | | FK → ServiceRequestCategories | Category | ❌ |
| Priority | INT | No | 3 | 1-4 (Critical to Low) | Priority | ❌ |
| Impact | INT | No | 3 | 1-4 (Business Down to Minimal) | Impact | ❌ |
| Urgency | INT | No | 3 | 1-4 (Immediate to Low) | Urgency | ❌ |
| Status | VARCHAR(50) | No | New | New/InvestRCA/RCAComplete/KnownError/ReadyForChange/ChangeScheduled/Resolved/Closed | Status | ❌ |
| RootCause | TEXT | Yes | | Populated after RCA completion | RootCause | ❌ |
| Workaround | TEXT | Yes | | Temporary solution while permanent fix pending | Workaround | ❌ |
| PermanentSolution | TEXT | Yes | | Final solution description | PermanentSolution | ❌ |
| IncidentCount (30d) | INT | No | 0 | Calculated; triggers trend detection | IncidentCount | ❌ |
| CreatedDate | DATETIME | No | CURRENT_TIMESTAMP | | CreatedDate | ✅ |
| CreatedBy | INT | No | | FK → Users | CreatedBy | ❌ |
| UpdatedDate | DATETIME | Yes | | On update | UpdatedDate | ✅ |
| TargetResolutionDate | DATETIME | Yes | | SLA-driven based on priority | TargetResolutionDate | ❌ |
| ResolvedDate | DATETIME | Yes | | Populated on resolution | ResolvedDate | ❌ |
| ClosedDate | DATETIME | Yes | | Populated on closure | ClosedDate | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | Soft delete | IsDeleted | ✅ |
| RowVersion | BINARY(8) | No | | Optimistic concurrency | RowVersion | ✅ |

### 4.3 Data Elements - RootCauseAnalysis Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | RCAId | ❌ |
| ProblemId | INT | No | | FK → Problems | ProblemId | ❌ |
| Status | VARCHAR(50) | No | InProgress | InProgress/Submitted/Approved/Rejected | Status | ❌ |
| InvestigationTree | JSON | No | | Hierarchical 5-Whys tree as nested JSON | InvestigationTree | ❌ |
| ConfidenceLevel | INT | No | 0 | 0-100 (% confidence in root cause) | ConfidenceLevel | ❌ |
| RootCauseStatement | TEXT | Yes | | Final RCA conclusion | RootCauseStatement | ❌ |
| RecommendedActions | JSON | No | [] | Array of action items with responsibility | RecommendedActions | ❌ |
| StartDate | DATETIME | No | CURRENT_TIMESTAMP | | StartDate | ❌ |
| CompletionDate | DATETIME | Yes | | Populated on submission | CompletionDate | ❌ |
| ConductedBy | INT | No | | FK → Users | ConductedBy | ❌ |
| ApprovedBy | INT | Yes | | FK → Users (null until approved) | ApprovedBy | ❌ |
| Notes | TEXT | Yes | | Investigation notes and discoveries | Notes | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | | | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | | IsDeleted | ✅ |

### 4.4 Data Elements - RCAEvidence Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | EvidenceId | ❌ |
| RCAId | INT | No | | FK → RootCauseAnalysis | RCAId | ❌ |
| EvidenceType | VARCHAR(50) | No | | Log/Screenshot/Metric/Document/SystemState | EvidenceType | ❌ |
| Title | VARCHAR(255) | No | | | Title | ❌ |
| Description | TEXT | Yes | | | Description | ❌ |
| Content | LONGTEXT | Yes | | Raw data (logs, metrics) | Content | ❌ |
| FilePath | VARCHAR(500) | Yes | | Path to uploaded file (screenshot, doc) | FilePath | ❌ |
| MimeType | VARCHAR(100) | Yes | | application/json, image/png, etc. | MimeType | ❌ |
| FileSize | BIGINT | Yes | | Size in bytes | FileSize | ❌ |
| Timestamp | DATETIME | Yes | | When evidence was collected/occurred | Timestamp | ❌ |
| UploadedBy | INT | No | | FK → Users | UploadedBy | ❌ |
| UploadedAt | DATETIME | No | CURRENT_TIMESTAMP | | UploadedAt | ✅ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | | CreatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | | IsDeleted | ✅ |

### 4.5 Data Elements - RCAInvestigationNodes Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | NodeId | ❌ |
| RCAId | INT | No | | FK → RootCauseAnalysis | RCAId | ❌ |
| WhyLevel | INT | No | 1 | 1-7 (Why layers in analysis) | WhyLevel | ❌ |
| Question | VARCHAR(500) | No | | The "Why" question asked | Question | ❌ |
| Answer | TEXT | Yes | | Response/investigation finding | Answer | ❌ |
| EvidenceIds | JSON | No | [] | Array of evidence FK references | EvidenceReferences | ❌ |
| ParentNodeId | INT | Yes | | FK → RCAInvestigationNodes (self-reference for tree) | ParentNodeId | ❌ |
| Status | VARCHAR(50) | No | Open | Open/Analyzed/Verified | Status | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | | | UpdatedAt | ✅ |

### 4.6 Data Elements - KnownErrors Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | KnownErrorId | ❌ |
| ProblemId | INT | No | | FK → Problems | ProblemId | ❌ |
| Title | VARCHAR(500) | No | | Descriptive title of known error | Title | ❌ |
| Description | TEXT | Yes | | Full error description and symptoms | Description | ❌ |
| Component | VARCHAR(255) | No | | Affected component/module/system | Component | ❌ |
| AffectedVersions | JSON | No | [] | Array of software versions affected | AffectedVersions | ❌ |
| Workaround | TEXT | No | | Temporary mitigation while fix pending | Workaround | ❌ |
| PermanentSolution | TEXT | No | | Final resolution steps | PermanentSolution | ❌ |
| SolutionImplementationDate | DATETIME | Yes | | When permanent solution will/was implemented | SolutionDate | ❌ |
| ImpactedSystems | JSON | No | [] | Array of affected systems/services | ImpactedSystems | ❌ |
| Status | VARCHAR(50) | No | Internal | Internal/Published/Deprecated | Status | ❌ |
| Priority | INT | No | 2 | 1-4 (Critical to Low) | Priority | ❌ |
| DocumentedBy | INT | No | | FK → Users | DocumentedBy | ❌ |
| PublishedDate | DATETIME | Yes | | When error published to public KB | PublishedDate | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | | | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | | IsDeleted | ✅ |

### 4.7 Data Elements - ProblemIncidentLinks Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | LinkId | ❌ |
| ProblemId | INT | No | | FK → Problems | ProblemId | ❌ |
| IncidentId | INT | No | | FK → ServiceRequests (Incident type) | IncidentId | ❌ |
| LinkType | VARCHAR(50) | No | Related | Related/Duplicate/Trend/Cause | LinkType | ❌ |
| MatchScore | INT | No | 0 | 0-100 (confidence of matching) | MatchScore | ❌ |
| MatchReason | JSON | No | {} | {keywords: [], category: [], symptoms: []} | MatchReason | ❌ |
| IsAutoLinked | BOOLEAN | No | FALSE | TRUE if linked by system suggestion | IsAutoLinked | ❌ |
| CreatedDate | DATETIME | No | CURRENT_TIMESTAMP | | CreatedDate | ✅ |
| CreatedBy | INT | No | | FK → Users | CreatedBy | ❌ |
| ConfirmedBy | INT | Yes | | FK → Users (null if auto) | ConfirmedBy | ❌ |
| ConfirmedDate | DATETIME | Yes | | When human confirmed the link | ConfirmedDate | ❌ |
| UNIQUE | | | | (ProblemId, IncidentId) | | ❌ |

### 4.8 Data Elements - ProblemTrends Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | TrendId | ❌ |
| ProblemId | INT | No | | FK → Problems | ProblemId | ❌ |
| PeriodStart | DATETIME | No | | Start of trend analysis period (30 days) | PeriodStart | ❌ |
| PeriodEnd | DATETIME | No | | End of trend analysis period | PeriodEnd | ❌ |
| IncidentCount | INT | No | 0 | Number of incidents in period | IncidentCount | ❌ |
| IncidentCountPrevious | INT | No | 0 | Previous period count for comparison | IncidentCountPrevious | ❌ |
| TrendPercentage | DECIMAL(5,2) | No | 0.00 | Growth % from previous period | TrendPercentage | ❌ |
| RecurrencePattern | VARCHAR(50) | No | Unknown | Daily/Weekly/Monthly/Random/Unknown | RecurrencePattern | ❌ |
| PredictedNextOccurrence | DATETIME | Yes | | Forecasted next incident date | PredictedNextOccurrence | ❌ |
| ConfidenceInPrediction | INT | No | 0 | 0-100 | ConfidenceInPrediction | ❌ |
| SeverityTrend | VARCHAR(50) | No | Stable | Increasing/Decreasing/Stable | SeverityTrend | ❌ |
| AnalyzedAt | DATETIME | No | CURRENT_TIMESTAMP | | AnalyzedAt | ✅ |
| UpdatedAt | DATETIME | Yes | | | UpdatedAt | ✅ |

### 4.9 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| RootCauseAnalysis | Problems | N:1 | ProblemId | ❌ |
| RCAEvidence | RootCauseAnalysis | N:1 | RCAId | ❌ |
| RCAInvestigationNodes | RootCauseAnalysis | N:1 | RCAId | ❌ |
| RCAInvestigationNodes | RCAInvestigationNodes | Self (parent) | ParentNodeId | ❌ |
| KnownErrors | Problems | N:1 | ProblemId | ❌ |
| ProblemIncidentLinks | Problems | N:1 | ProblemId | ❌ |
| ProblemIncidentLinks | ServiceRequests (Incidents) | N:1 | IncidentId | ❌ |
| ProblemTrends | Problems | N:1 | ProblemId | ❌ |
| ProblemMetrics | Problems | N:1 | ProblemId | ❌ |
| Problems | Users (Owner) | N:1 | CreatedBy | ❌ |
| RootCauseAnalysis | Users (Conductor) | N:1 | ConductedBy | ❌ |

### 4.10 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Problems_Status | Problems | Status | NonClustered | ❌ |
| IX_Problems_Priority | Problems | Priority | NonClustered | ❌ |
| IX_Problems_CreatedDate | Problems | CreatedDate | NonClustered | ❌ |
| IX_Problems_IncidentCount | Problems | IncidentCount | NonClustered | ❌ |
| IX_RCA_ProblemId | RootCauseAnalysis | ProblemId | NonClustered | ❌ |
| IX_RCAEvidence_RCAId | RCAEvidence | RCAId | NonClustered | ❌ |
| IX_KnownErrors_Component | KnownErrors | Component | NonClustered | ❌ |
| IX_KnownErrors_Status | KnownErrors | Status | NonClustered | ❌ |
| IX_ProblemIncidentLinks_ProblemId | ProblemIncidentLinks | ProblemId | NonClustered | ❌ |
| IX_ProblemIncidentLinks_IncidentId | ProblemIncidentLinks | IncidentId | NonClustered | ❌ |
| IX_ProblemTrends_ProblemId_PeriodStart | ProblemTrends | ProblemId, PeriodStart | NonClustered | ❌ |
| IX_ProblemTrends_IncidentCount | ProblemTrends | IncidentCount DESC | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ProblemServiceTests | `CRM.Tests/Services/ProblemServiceTests.cs` | 18 | ❌ |
| RCAConductorTests | `CRM.Tests/Services/RCAConductorTests.cs` | 16 | ❌ |
| RCAEvidenceCollectorTests | `CRM.Tests/Services/RCAEvidenceCollectorTests.cs` | 12 | ❌ |
| KnownErrorServiceTests | `CRM.Tests/Services/KnownErrorServiceTests.cs` | 14 | ❌ |
| IncidentProblemLinkerTests | `CRM.Tests/Services/IncidentProblemLinkerTests.cs` | 15 | ❌ |
| TrendAnalyzerTests | `CRM.Tests/Services/TrendAnalyzerTests.cs` | 12 | ❌ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ProblemServiceIntegrationTests | `CRM.Tests/Integration/ProblemServiceIntegrationTests.cs` | 10 | ❌ |
| RCAWorkflowIntegrationTests | `CRM.Tests/Integration/RCAWorkflowIntegrationTests.cs` | 8 | ❌ |
| IncidentProblemLinkingIntegrationTests | `CRM.Tests/Integration/IncidentProblemLinkingIntegrationTests.cs` | 6 | ❌ |
| TrendAnalysisIntegrationTests | `CRM.Tests/Integration/TrendAnalysisIntegrationTests.cs` | 7 | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Problem Management E2E | `e2e-tests/tests/itsm/problem-management.spec.ts` | 12 | ❌ |
| RCA Workflow E2E | `e2e-tests/tests/itsm/rca-workflow.spec.ts` | 10 | ❌ |
| Known Errors E2E | `e2e-tests/tests/itsm/known-errors.spec.ts` | 8 | ❌ |
| Incident Linking E2E | `e2e-tests/tests/itsm/incident-linking.spec.ts` | 6 | ❌ |
| Trend Analysis E2E | `e2e-tests/tests/itsm/trend-analysis.spec.ts` | 5 | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| RCAInvestigationNode.Question (VARCHAR 500) | RCAInvestigationNode input form | User might submit very long investigation questions | Increase field to TEXT or truncate at API boundary |
| Problem.Priority (INT 1-4) | Known Error.Priority (INT 1-4) | Should map through common priority enum | Create shared PriorityEnum in Core; use throughout |
| ProblemIncidentLinks.MatchScore (INT 0-100) | Frontend SuggestedProblems display (0-100%) | Percentage display should round consistently | Implement percentage formatter in service layer |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| AI-powered incident matching | IncidentProblemLinker service | Requires ML model training on historical incidents | TODO-ITSM002-01 |
| RCA auto-suggestion engine | RCAConductor service | Next "Why" suggestions based on problem type/history | TODO-ITSM002-02 |
| Predictive recurrence modeling | TrendAnalyzer service | Requires time-series analysis or ML integration | TODO-ITSM002-03 |
| Evidence encryption for sensitive logs | RCAEvidenceCollector service | PII/passwords in evidence need masking/encryption | TODO-ITSM002-04 |
| Change integration workflows | ProblemChangeIntegration service | Orchestration with Change Management module | TODO-ITSM002-05 |
| Problem impact calculator | ProblemService service | Calculate business impact from linked incidents | TODO-ITSM002-06 |
| RCA report PDF generation | RCAConductor service | Export RCA investigation as formatted PDF | TODO-ITSM002-07 |
| Known error auto-publication rules | KnownErrorService service | Rule engine for auto-publish when solution ready | TODO-ITSM002-08 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| Problem.Title uniqueness | No check for duplicate titles in system | TODO-ITSM002-09 |
| RCA Evidence file size limits | No max file size validation on upload | TODO-ITSM002-10 |
| Investigation tree depth validation | No enforcement of min 3 / max 7 Why levels | TODO-ITSM002-11 |
| Incident link confidence threshold | No configurable threshold for auto-linking | TODO-ITSM002-12 |
| RCA completion criteria | No validation that RCA is "ready to submit" | TODO-ITSM002-13 |
| Known error component validation | No validation that component exists in system | TODO-ITSM002-14 |
| Solution permanence criteria | No validation distinguishing workaround vs. permanent | TODO-ITSM002-15 |

---

## 7. TODOs (Master List)

### Business Logic Implementation
- **TODO-ITSM002-01**: Design and implement AI-powered incident matching algorithm for incident-to-problem linking based on symptom/keyword/category similarity
- **TODO-ITSM002-02**: Implement RCA auto-suggestion engine that suggests next "Why" questions based on problem type, industry, historical data
- **TODO-ITSM002-03**: Integrate time-series forecasting (ARIMA/Prophet) for predictive recurrence modeling in TrendAnalyzer
- **TODO-ITSM002-04**: Implement PII detection and masking in RCA evidence collector for sensitive logs (passwords, credit cards, etc.)
- **TODO-ITSM002-05**: Create orchestration layer for Problem-Change-Implementation workflow with approval gates
- **TODO-ITSM002-06**: Implement business impact calculator: derives impact from linked incidents' SLA breaches and affected users
- **TODO-ITSM002-07**: Create RCA report PDF generator with formatted investigation tree, evidence summary, recommendations
- **TODO-ITSM002-08**: Build rule engine for auto-publication of known errors when solution implementation date reached

### Validation Implementation
- **TODO-ITSM002-09**: Add database unique constraint check for Problem.Title duplicates; warn user before creating duplicate
- **TODO-ITSM002-10**: Add max file size validation (50MB) for RCA evidence uploads; truncate large logs at API boundary
- **TODO-ITSM002-11**: Implement RCAConductor validation enforcing 3-7 levels in investigation tree before submission
- **TODO-ITSM002-12**: Create configurable threshold setting for incident-to-problem match confidence (default 75%); allow admin tuning
- **TODO-ITSM002-13**: Define RCA completion checklist: root cause, evidence, confidence score, actions; validate before submit
- **TODO-ITSM002-14**: Add component validation against master component catalog in system settings
- **TODO-ITSM002-15**: Implement categorization of solution types (Workaround vs. Permanent) with distinct workflows

### Database & Performance
- **TODO-ITSM002-16**: Create database indexes for trend analysis queries: Problems(IncidentCount), ProblemTrends(PeriodStart DESC)
- **TODO-ITSM002-17**: Partition ProblemTrends table by PeriodStart to optimize historical trend queries
- **TODO-ITSM002-18**: Implement caching layer for frequently-accessed known errors (Redis with 1-hour TTL)
- **TODO-ITSM002-19**: Create scheduled job to calculate and update ProblemTrends table daily at 2 AM UTC
- **TODO-ITSM002-20**: Add database maintenance job to archive closed problems > 1 year old to separate table

### Frontend Implementation
- **TODO-ITSM002-21**: Create interactive 5-Whys investigation tree UI component with drag-drop node reordering
- **TODO-ITSM002-22**: Implement evidence attachment workflow (multi-file upload with progress, drag-drop support)
- **TODO-ITSM002-23**: Create incident-problem similarity matrix visualization (heatmap) for bulk linking operations
- **TODO-ITSM002-24**: Build trend dashboard with line/bar charts, drill-down to incident details
- **TODO-ITSM002-25**: Create RCA progress indicator showing completion % and required steps

### Integrations
- **TODO-ITSM002-26**: Integrate with Change Management: auto-create change request from problem with pre-filled solution
- **TODO-ITSM002-27**: Integrate with Incident Management: auto-link closed incidents to problems within 24 hours
- **TODO-ITSM002-28**: Implement webhook notification when problem status changes (for Change/Incident/Ticket integrations)
- **TODO-ITSM002-29**: Create ServiceNow/BMC integration for bi-directional problem sync (future roadmap)

### Testing
- **TODO-ITSM002-30**: Create unit tests for RCA confidence score calculation across various evidence types and quantities
- **TODO-ITSM002-31**: Create integration tests verifying incident-to-problem matching accuracy with 95%+ precision target
- **TODO-ITSM002-32**: Create unit tests for trend calculation: incident counting, recurrence pattern detection, prediction accuracy
- **TODO-ITSM002-33**: Create E2E tests for complete RCA workflow from incident to known error publication
- **TODO-ITSM002-34**: Create performance tests for trend analysis with 10,000+ problems in system

### Documentation
- **TODO-ITSM002-35**: Write RCA best practices guide (min/max Why levels, evidence collection tips, confidence scoring)
- **TODO-ITSM002-36**: Create admin guide for configuring problem categories, components, known error auto-publication rules
- **TODO-ITSM002-37**: Create user training documentation for RCA workflow with examples and common pitfalls

---

## 8. Functional Requirements Summary

### Problem Lifecycle States
```
New → InvestRCA → RCAComplete → KnownError → ReadyForChange → ChangeScheduled → Resolved → Closed
```

### RCA Workflow
1. Initiate RCA from incident or manually
2. Ask "Why" questions and record answers (min 3 levels)
3. Attach supporting evidence (logs, metrics, screenshots)
4. Calculate confidence score based on evidence
5. Submit for approval
6. Create known error (if applicable)
7. Link to change request for implementation

### Known Error Auto-Linking
- When known error created, system searches for similar incidents (keywords, category, component)
- Calculates match confidence score
- Auto-links incidents > 75% confidence (configurable)
- Manual review available for borderline matches

### Trend Detection Algorithm
- Monitor incident count per problem in 30-day rolling window
- Calculate trend % vs. previous period
- If ≥ 3 incidents in 30d → classified as trend
- Analyze recurrence pattern (daily/weekly/monthly/random)
- Predict next occurrence using time-series model
- Alert if trend is increasing

---

## 9. Dependencies

**Depends On:**
- SPEC-ITSM-001 (Incident Management) - for incident linking and incident data
- SPEC-SYS-004 (Feature Flags) - for experimental RCA features/AI suggestions

**Feeds To:**
- Change Management (creates changes from problems)
- Reporting/Analytics (problem metrics in dashboards)
- Knowledge Management (publishes known errors to KB)

---

## 10. Success Criteria

✅ **Acceptance Criteria:**
1. RCA can be completed in < 30 minutes for 80% of incidents
2. Incident-to-problem matching accuracy ≥ 90%
3. Trend detection identifies recurrence patterns within 2 days
4. Known error workaround reduces incident resolution time by 50%
5. Problem closure rate ≥ 70% within 60 days of creation
6. User satisfaction with RCA workflow ≥ 4/5 stars

---

**END OF SPECIFICATION**

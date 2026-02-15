# Feature Specification: ITSM Incident Management

> **Spec ID:** SPEC-ITSM-001  
> **Feature:** Incident Management  
> **Module:** ITSM (IT Service Management)  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ⏳ Pending Implementation

---

## 1. Business Context

### 1.1 Feature Description

Incident Management is the core operational process in ITSM that handles the detection, logging, triage, diagnosis, resolution, and closure of unplanned interruptions to IT services. This module provides comprehensive incident lifecycle management including severity/priority classification, impact analysis, intelligent assignment, escalation workflows, SLA compliance tracking, and root cause analysis.

**Business Goals:**
- Minimize Mean Time To Resolution (MTTR)
- Maintain Service Level Agreement (SLA) compliance above 95%
- Reduce incident volume through proactive problem management
- Track business impact and cost of incidents
- Provide audit trail for compliance and learning

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Incident Creation & Logging | Automated and manual incident creation with classification | ⏳ |
| SF-002 | Severity & Priority Assessment | AI-powered and manual severity/priority calculation | ⏳ |
| SF-003 | Impact Analysis | Business impact calculation and affected CI relationships | ⏳ |
| SF-004 | Intelligent Assignment | Rule-based and skill-based incident assignment | ⏳ |
| SF-005 | Escalation Management | Time-based and priority-based escalation workflows | ⏳ |
| SF-006 | SLA Compliance Tracking | Real-time SLA breach detection and notifications | ⏳ |
| SF-007 | Incident Lifecycle Management | States: New, Assigned, InProgress, OnHold, Resolved, Closed | ⏳ |
| SF-008 | Knowledge Base Integration | Link related KB articles for resolution guidance | ⏳ |
| SF-009 | Incident Communication | Multi-channel status updates and stakeholder notifications | ⏳ |
| SF-010 | Workaround Management | Track temporary solutions pending permanent resolution | ⏳ |
| SF-011 | Related Incidents | Link duplicate or related incidents (incident grouping) | ⏳ |
| SF-012 | Timeline & Audit Trail | Complete audit history with timestamps and actor tracking | ⏳ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Report New IT Issue | End User | Service is impaired | Incident created with Auto ID | ⏳ |
| UC-002 | Auto-Detect Critical Alert | Monitoring System | Alert threshold exceeded | Incident auto-created with severity | ⏳ |
| UC-003 | Assess Incident Impact | Support Agent | Incident assigned to agent | Business impact calculated, related CIs linked | ⏳ |
| UC-004 | Escalate Stalled Incident | System Timer | SLA threshold reached 80% | Incident escalated to L2/L3 support | ⏳ |
| UC-005 | Assign Work to Technician | L1 Support Agent | Incident classified & impact known | Incident assigned to available technician | ⏳ |
| UC-006 | Apply Workaround | L2 Support Agent | Root cause identified but permanent fix pending | Workaround documented, incident state = OnHold | ⏳ |
| UC-007 | Resolve & Close Incident | Support Agent | Incident solution applied and verified | Incident closed with resolution notes | ⏳ |
| UC-008 | Link Related Incidents | Support Manager | Duplicate incidents exist | Incidents linked, priority consolidated | ⏳ |
| UC-009 | View Incident Dashboard | Manager | Logged in to system | Real-time incident metrics displayed | ⏳ |
| UC-010 | Generate Incident Report | Service Delivery Manager | Date range selected | Report exported with SLA metrics | ⏳ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| Incident List | `CRM.Frontend/src/pages/itsm/IncidentsPage.tsx` | ❌ | Table view with filters, search, sorting |
| Incident Detail | `CRM.Frontend/src/pages/itsm/IncidentDetailPage.tsx` | ❌ | Full incident view with timeline, assignments, SLA |
| Incident Creation Wizard | `CRM.Frontend/src/pages/itsm/IncidentCreateWizardPage.tsx` | ❌ | Multi-step guided creation with classification |
| Assignment Dashboard | `CRM.Frontend/src/pages/itsm/AssignmentDashboardPage.tsx` | ❌ | Workload distribution, pending assignments |
| Escalation Panel | `CRM.Frontend/src/pages/itsm/EscalationPanelPage.tsx` | ❌ | Real-time escalation queue and actions |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| IncidentSummaryCard | `CRM.Frontend/src/components/itsm/IncidentSummaryCard.tsx` | ❌ | Key incident metrics |
| SeverityBadge | `CRM.Frontend/src/components/itsm/SeverityBadge.tsx` | ❌ | Visual severity indicator (Critical/High/Medium/Low) |
| PriorityBadge | `CRM.Frontend/src/components/itsm/PriorityBadge.tsx` | ❌ | Priority level indicator |
| ImpactAnalysisPanel | `CRM.Frontend/src/components/itsm/ImpactAnalysisPanel.tsx` | ❌ | Affected CIs and business impact visualization |
| EscalationIndicator | `CRM.Frontend/src/components/itsm/EscalationIndicator.tsx` | ❌ | SLA time remaining and escalation status |
| SLAMeter | `CRM.Frontend/src/components/itsm/SLAMeter.tsx` | ❌ | Response/Resolution SLA progress bar |
| IncidentTimeline | `CRM.Frontend/src/components/itsm/IncidentTimeline.tsx` | ❌ | Complete incident lifecycle timeline with state changes |
| AssignmentForm | `CRM.Frontend/src/components/itsm/AssignmentForm.tsx` | ❌ | Assign incident to technician with conflict check |
| EscalationForm | `CRM.Frontend/src/components/itsm/EscalationForm.tsx` | ❌ | Manual escalation with reason and target level |
| KnowledgeBaseWidget | `CRM.Frontend/src/components/itsm/KnowledgeBaseWidget.tsx` | ❌ | Suggested KB articles based on incident classification |
| IncidentCommentForm | `CRM.Frontend/src/components/itsm/IncidentCommentForm.tsx` | ❌ | Add updates and internal/external notes |
| RelatedIncidentsPanel | `CRM.Frontend/src/components/itsm/RelatedIncidentsPanel.tsx` | ❌ | Display linked incidents and grouping options |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| incidentService | `CRM.Frontend/src/services/itsm/incidentService.ts` | GetAll, GetById, Create, Update, Delete, Search, BulkAction | ❌ |
| incidentAssignmentService | `CRM.Frontend/src/services/itsm/incidentAssignmentService.ts` | Assign, Reassign, Unassign, GetWorkload, SuggestAssignee | ❌ |
| incidentEscalationService | `CRM.Frontend/src/services/itsm/incidentEscalationService.ts` | Escalate, GetEscalationQueue, UpdatePriority | ❌ |
| impactAnalysisService | `CRM.Frontend/src/services/itsm/impactAnalysisService.ts` | AnalyzeImpact, GetAffectedCIs, CalculateBusinessImpact | ❌ |
| slaService | `CRM.Frontend/src/services/itsm/slaService.ts` | GetSLAStatus, CheckBreachRisk, GetTimeRemaining | ❌ |
| incidentSearchService | `CRM.Frontend/src/services/itsm/incidentSearchService.ts` | Search, GetSuggestionsForClassification | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Title/Summary | Min 10 chars, max 500 chars, no SQL injection | Frontend/Backend | ❌ |
| Description | Min 20 chars, max 5000 chars | Frontend/Backend | ❌ |
| Incident Type | Must be in configured types list | Backend | ❌ |
| Category | Must be in category list | Backend | ❌ |
| Urgency | Must be 1-5 (Critical to Low) | Frontend/Backend | ❌ |
| Impact | Must be 1-5 (Extensive to Minimal) | Frontend/Backend | ❌ |
| Affected CI | Must exist in CMDB | Backend | ❌ |
| Assigned User | If assigned, must have required skills | Backend | ❌ |
| Status Transition | Must follow valid state machine | Backend | ❌ |
| SLA Policy | Must be associated with valid SLA | Backend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Incident | `CRM.Core/Entities/Incident.cs` | ❌ | Core incident entity with lifecycle |
| IncidentTimeline | `CRM.Core/Entities/IncidentTimeline.cs` | ❌ | Audit trail with state changes |
| ImpactAnalysis | `CRM.Core/Entities/ImpactAnalysis.cs` | ❌ | Business impact calculation |
| IncidentAssignment | `CRM.Core/Entities/IncidentAssignment.cs` | ❌ | Assignment history and chain |
| EscalationRule | `CRM.Core/Entities/EscalationRule.cs` | ❌ | Rule engine for auto-escalation |
| IncidentComment | `CRM.Core/Entities/IncidentComment.cs` | ❌ | Updates and notes (internal/external) |
| RelatedIncident | `CRM.Core/Entities/RelatedIncident.cs` | ❌ | Junction for incident relationships |
| IncidentConfiguration | `CRM.Core/Entities/IncidentConfiguration.cs` | ❌ | Module-level configuration |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| IncidentDto | `CRM.Core/DTOs/Incident/IncidentDto.cs` | ❌ | Full incident view for responses |
| CreateIncidentDto | `CRM.Core/DTOs/Incident/CreateIncidentDto.cs` | ❌ | Create incident input |
| UpdateIncidentDto | `CRM.Core/DTOs/Incident/UpdateIncidentDto.cs` | ❌ | Partial update input |
| IncidentListItemDto | `CRM.Core/DTOs/Incident/IncidentListItemDto.cs` | ❌ | List view summary |
| AssignIncidentDto | `CRM.Core/DTOs/Incident/AssignIncidentDto.cs` | ❌ | Assignment request |
| EscalateIncidentDto | `CRM.Core/DTOs/Incident/EscalateIncidentDto.cs` | ❌ | Escalation request |
| ImpactAnalysisDto | `CRM.Core/DTOs/Incident/ImpactAnalysisDto.cs` | ❌ | Impact assessment result |
| IncidentFilterDto | `CRM.Core/DTOs/Incident/IncidentFilterDto.cs` | ❌ | Search and filter criteria |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IIncidentService | `CRM.Core/Interfaces/IIncidentService.cs` | 25+ (CRUD, lifecycle, assignment, escalation, impact) | ❌ |
| IIncidentAssignmentService | `CRM.Core/Interfaces/IIncidentAssignmentService.cs` | 8+ (Assign, reassign, unassign, workload, suggestions) | ❌ |
| IEscalationService | `CRM.Core/Interfaces/IEscalationService.cs` | 6+ (Escalate, check breach, notify) | ❌ |
| IImpactAnalysisService | `CRM.Core/Interfaces/IImpactAnalysisService.cs` | 5+ (Analyze, calculate impact, get affected CIs) | ❌ |
| ISLAService | `CRM.Core/Interfaces/ISLAService.cs` | 6+ (Check compliance, calculate metrics, alert) | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| IncidentService | `CRM.Infrastructure/Services/IncidentService.cs` | 25+ (GetAll, GetById, Create, Update, Delete, Search, Lifecycle) | ❌ |
| IncidentAssignmentService | `CRM.Infrastructure/Services/IncidentAssignmentService.cs` | 8+ (Assign, Reassign, Unassign, SuggestAssignee) | ❌ |
| EscalationService | `CRM.Infrastructure/Services/EscalationService.cs` | 6+ (EvaluateEscalationRules, Escalate, CheckBreaches) | ❌ |
| ImpactAnalysisService | `CRM.Infrastructure/Services/ImpactAnalysisService.cs` | 5+ (AnalyzeImpact, CalculateBusinessImpact, GetAffectedCIs) | ❌ |
| SLAService | `CRM.Infrastructure/Services/SLAService.cs` | 6+ (CheckSLACompliance, CalculateMetrics, NotifyOnBreach) | ❌ |
| IncidentSeverityCalculator | `CRM.Infrastructure/Services/IncidentSeverityCalculator.cs` | 3+ (CalculateSeverity, CalculatePriority, ApplyCorrectionFactors) | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| IncidentsController | `CRM.Api/Controllers/IncidentsController.cs` | 15+ | ❌ |
| IncidentAssignmentController | `CRM.Api/Controllers/IncidentAssignmentController.cs` | 5+ | ❌ |
| IncidentEscalationController | `CRM.Api/Controllers/IncidentEscalationController.cs` | 5+ | ❌ |
| IncidentAnalysisController | `CRM.Api/Controllers/IncidentAnalysisController.cs` | 5+ | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/itsm/incidents` | GetAllAsync | Yes | ❌ |
| GET | `/api/itsm/incidents/{id}` | GetByIdAsync | Yes | ❌ |
| GET | `/api/itsm/incidents/{id}/timeline` | GetTimelineAsync | Yes | ❌ |
| POST | `/api/itsm/incidents` | CreateAsync | Yes | ❌ |
| PUT | `/api/itsm/incidents/{id}` | UpdateAsync | Yes | ❌ |
| PATCH | `/api/itsm/incidents/{id}/status` | UpdateStatusAsync | Yes | ❌ |
| DELETE | `/api/itsm/incidents/{id}` | DeleteAsync | Yes | ❌ |
| GET | `/api/itsm/incidents/search` | SearchAsync | Yes | ❌ |
| GET | `/api/itsm/incidents/{id}/impact` | GetImpactAnalysisAsync | Yes | ❌ |
| POST | `/api/itsm/incidents/{id}/impact/analyze` | AnalyzeImpactAsync | Yes | ❌ |
| POST | `/api/itsm/incidents/{id}/assign` | AssignAsync | Yes | ❌ |
| POST | `/api/itsm/incidents/{id}/escalate` | EscalateAsync | Yes | ❌ |
| POST | `/api/itsm/incidents/{id}/workaround` | SetWorkaroundAsync | Yes | ❌ |
| GET | `/api/itsm/incidents/{id}/related` | GetRelatedIncidentsAsync | Yes | ❌ |
| POST | `/api/itsm/incidents/{id}/link/{relatedId}` | LinkIncidentAsync | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Title | Min 10 chars, max 500, no SQL | IncidentValidator | ❌ |
| Description | Min 20 chars, max 5000 | IncidentValidator | ❌ |
| Category | Must exist in ServiceRequestCategories | IncidentValidator | ❌ |
| Impact Rating | 1-5, validated against history | SeverityCalculator | ❌ |
| Urgency Rating | 1-5, validated against SLA | SeverityCalculator | ❌ |
| Affected CI | Must exist in CMDB | ImpactAnalysisValidator | ❌ |
| Assigned User | Must have required skills and availability | AssignmentValidator | ❌ |
| Status Transition | Must follow state machine (New→Assigned→InProgress→OnHold/Resolved→Closed) | IncidentService | ❌ |
| SLA Policy | Must have valid time targets | SLAValidator | ❌ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Incidents | `database/schema/itsm/001_incidents.sql` | ❌ | Core incident records |
| IncidentTimeline | `database/schema/itsm/002_incident_timeline.sql` | ❌ | Audit trail with state changes |
| ImpactAnalysis | `database/schema/itsm/003_impact_analysis.sql` | ❌ | Business impact calculations |
| IncidentAssignments | `database/schema/itsm/004_incident_assignments.sql` | ❌ | Assignment history chain |
| EscalationRules | `database/schema/itsm/005_escalation_rules.sql` | ❌ | Auto-escalation rule definitions |
| IncidentComments | `database/schema/itsm/006_incident_comments.sql` | ❌ | Updates and internal/external notes |
| RelatedIncidents | `database/schema/itsm/007_related_incidents.sql` | ❌ | Junction for incident relationships |

### 4.2 Data Elements (Incidents Table)

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| IncidentNumber | VARCHAR(50) | No | AUTO | UK | IncidentNumber | ❌ |
| Title | VARCHAR(500) | No | - | - | Title | ❌ |
| Description | TEXT | No | - | - | Description | ❌ |
| Status | VARCHAR(50) | No | 'New' | Check(New,Assigned,InProgress,OnHold,Resolved,Closed) | Status | ❌ |
| Severity | INT | No | 3 | Check(1-5), 1=Critical, 5=Low | Severity | ❌ |
| Priority | INT | No | 3 | Check(1-5), calculated from Severity+Impact | Priority | ❌ |
| Impact | INT | No | 3 | Check(1-5), 1=Extensive, 5=Minimal | Impact | ❌ |
| Urgency | INT | No | 3 | Check(1-5), 1=Critical, 5=Low | Urgency | ❌ |
| Category | VARCHAR(100) | No | - | FK → ServiceRequestCategories | CategoryId | ❌ |
| SubCategory | VARCHAR(100) | Yes | - | FK → ServiceRequestSubcategories | SubCategoryId | ❌ |
| Type | VARCHAR(100) | Yes | - | Incident/Service Request/Change/Problem | IncidentType | ❌ |
| AffectedCI | INT | Yes | - | FK → CMDB ConfigurationItems | AffectedCIId | ❌ |
| ReportedBy | INT | No | - | FK → Users | ReportedById | ❌ |
| AssignedTo | INT | Yes | - | FK → Users, Nullable until assigned | AssignedToId | ❌ |
| AssignedTeam | INT | Yes | - | FK → Teams | AssignedTeamId | ❌ |
| Manager | INT | Yes | - | FK → Users, escalation manager | ManagerId | ❌ |
| SLAPolicyId | INT | Yes | - | FK → SLAPolicies | SLAPolicyId | ❌ |
| ResponseDueAt | DATETIME | Yes | - | Calculated from SLA policy | ResponseDueAt | ❌ |
| ResolutionDueAt | DATETIME | Yes | - | Calculated from SLA policy | ResolutionDueAt | ❌ |
| ResponseGivenAt | DATETIME | Yes | - | Timestamp of first response | ResponseGivenAt | ❌ |
| ResolvedAt | DATETIME | Yes | - | When marked resolved | ResolvedAt | ❌ |
| ClosedAt | DATETIME | Yes | - | When closed | ClosedAt | ❌ |
| Workaround | TEXT | Yes | - | Temporary solution description | Workaround | ❌ |
| WorkaroundProvidedAt | DATETIME | Yes | - | When workaround applied | WorkaroundProvidedAt | ❌ |
| RootCause | TEXT | Yes | - | Root cause analysis | RootCause | ❌ |
| Resolution | TEXT | Yes | - | How incident was resolved | Resolution | ❌ |
| SLABreached | BOOLEAN | No | FALSE | Trigger if due date passed | SLABreached | ❌ |
| ResolutionSLABreached | BOOLEAN | No | FALSE | For resolution targets | ResolutionSLABreached | ❌ |
| EscalationCount | INT | No | 0 | Count of escalations | EscalationCount | ❌ |
| LastEscalatedAt | DATETIME | Yes | - | When last escalated | LastEscalatedAt | ❌ |
| CustomerSatisfaction | INT | Yes | - | 1-5 rating post-resolution | CustomerSatisfaction | ❌ |
| KnowledgeArticleId | INT | Yes | - | FK → KnowledgeArticles, linked resolution | KnowledgeArticleId | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | Audit | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | - | Audit | UpdatedAt | ✅ |
| IsDeleted | BOOLEAN | No | FALSE | Soft delete | IsDeleted | ✅ |
| RowVersion | BINARY(8) | No | - | Optimistic concurrency | RowVersion | ✅ |

### 4.3 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Incidents | Users (Reporter) | N:1 | ReportedById | ❌ |
| Incidents | Users (Assigned) | N:1 | AssignedToId | ❌ |
| Incidents | Teams | N:1 | AssignedTeamId | ❌ |
| Incidents | ServiceRequestCategories | N:1 | CategoryId | ❌ |
| Incidents | SLAPolicies | N:1 | SLAPolicyId | ❌ |
| Incidents | KnowledgeArticles | N:1 | KnowledgeArticleId | ❌ |
| IncidentTimeline | Incidents | N:1 | IncidentId | ❌ |
| ImpactAnalysis | Incidents | N:1 | IncidentId | ❌ |
| IncidentAssignments | Incidents | N:1 | IncidentId | ❌ |
| IncidentAssignments | Users | N:1 | AssignedToId | ❌ |
| EscalationRules | ServiceRequestCategories | N:1 | CategoryId | ❌ |
| RelatedIncidents | Incidents | N:1 | IncidentId (from) | ❌ |
| RelatedIncidents | Incidents | N:1 | RelatedIncidentId (to) | ❌ |

### 4.4 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Incidents_IncidentNumber | Incidents | IncidentNumber | NonClustered | ❌ |
| IX_Incidents_Status | Incidents | Status | NonClustered | ❌ |
| IX_Incidents_Priority | Incidents | Priority, Status | NonClustered | ❌ |
| IX_Incidents_AssignedTo | Incidents | AssignedToId | NonClustered | ❌ |
| IX_Incidents_CreatedAt | Incidents | CreatedAt DESC | NonClustered | ❌ |
| IX_Incidents_ResolutionDueAt | Incidents | ResolutionDueAt, SLABreached | NonClustered | ❌ |
| IX_Incidents_SLAPolicy | Incidents | SLAPolicyId | NonClustered | ❌ |
| IX_IncidentTimeline_IncidentId | IncidentTimeline | IncidentId, CreatedAt DESC | NonClustered | ❌ |
| IX_EscalationRules_Active | EscalationRules | IsActive, CategoryId | NonClustered | ❌ |
| IX_RelatedIncidents_Bidirectional | RelatedIncidents | IncidentId, RelatedIncidentId | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| IncidentServiceTests | `CRM.Backend/tests/CRM.Tests/Services/IncidentServiceTests.cs` | 25+ | ❌ |
| SeverityCalculatorTests | `CRM.Backend/tests/CRM.Tests/Services/SeverityCalculatorTests.cs` | 12+ | ❌ |
| ImpactAnalysisServiceTests | `CRM.Backend/tests/CRM.Tests/Services/ImpactAnalysisServiceTests.cs` | 15+ | ❌ |
| IncidentAssignmentServiceTests | `CRM.Backend/tests/CRM.Tests/Services/IncidentAssignmentServiceTests.cs` | 10+ | ❌ |
| EscalationServiceTests | `CRM.Backend/tests/CRM.Tests/Services/EscalationServiceTests.cs` | 15+ | ❌ |
| SLAServiceTests | `CRM.Backend/tests/CRM.Tests/Services/SLAServiceTests.cs` | 12+ | ❌ |

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| IncidentLifecycleIntegrationTests | `CRM.Backend/tests/CRM.Tests/Integration/IncidentLifecycleIntegrationTests.cs` | 10+ | ❌ |
| IncidentEscalationIntegrationTests | `CRM.Backend/tests/CRM.Tests/Integration/IncidentEscalationIntegrationTests.cs` | 8+ | ❌ |
| IncidentSLAIntegrationTests | `CRM.Backend/tests/CRM.Tests/Integration/IncidentSLAIntegrationTests.cs` | 8+ | ❌ |
| IncidentImpactIntegrationTests | `CRM.Backend/tests/CRM.Tests/Integration/IncidentImpactIntegrationTests.cs` | 6+ | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| Incident Creation | `e2e-tests/tests/itsm/incidents.spec.ts` | 5+ | ❌ |
| Incident Assignment | `e2e-tests/tests/itsm/incident-assignment.spec.ts` | 5+ | ❌ |
| Incident Escalation | `e2e-tests/tests/itsm/incident-escalation.spec.ts` | 5+ | ❌ |
| SLA Compliance | `e2e-tests/tests/itsm/sla-compliance.spec.ts` | 4+ | ❌ |

### 5.4 Critical Test Scenarios

**Severity Calculation Accuracy:**
- [ ] Verify severity score formula: Severity = (Urgency × 0.6 + Impact × 0.4) rounds to integer 1-5
- [ ] Test correction factors: P1 issues +1 severity, CEO-reported +1 severity
- [ ] Validate severity never exceeds 5 or goes below 1
- [ ] Test edge cases: All Critical (5), All Low (1), Mixed urgency/impact

**SLA Breach Detection:**
- [ ] Incident created without SLA policy: no SLA targets
- [ ] Incident with SLA policy: ResponseDueAt = now + ResponseTime, ResolutionDueAt = now + ResolutionTime
- [ ] Test 80% threshold: Escalation triggered when 80% of time consumed
- [ ] Test breach flag: SLABreached set to true when time expires
- [ ] Test breach reset: Status change clears escalation flag

**Escalation Triggering:**
- [ ] Time-based: Escalate when SLA time reaches 80%
- [ ] Priority-based: Auto-escalate P1/P2 if assigned >24hrs without response
- [ ] Manual escalation: User can force escalation with reason
- [ ] Chain of command: Escalate respects team hierarchy
- [ ] Duplicate prevention: Don't escalate already escalated incidents

**Impact Propagation:**
- [ ] Single affected CI: Impact = direct CI impact
- [ ] Multiple CIs: Impact = MAX(CI impacts)
- [ ] Service dependencies: Propagate up stack (DB down = App down = Service down)
- [ ] Circular dependencies: Detect and alert (do not cascade infinitely)
- [ ] No circular escalation: Prevent incident affecting same CI twice

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| IIncidentService (returns Severity:int 1-5) | SeverityCalculator (calculates float 1-5) | Type conversion needed | Convert to int, round 0.5 up |
| Incident.AssignedTo (nullable User FK) | Assignment logic (must be assigned) | Status transition allows unassigned | Enforce non-null for InProgress state |
| SLA.ResolutionTime (minutes int) | SLA calculation (uses TimeSpan) | Unit mismatch | Store as minutes, convert to TimeSpan in service |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| CMDB Integration | ImpactAnalysisService.GetAffectedCIs | Depends on CMDB module (deferred) | TODO-ITSM-001-15 |
| Webhook Integration | IncidentEventPublisher | Real-time external system sync (Phase 2) | TODO-ITSM-001-16 |
| Predictive Escalation | AI-based escalation rules | ML model training needed (Phase 3) | TODO-ITSM-001-17 |
| Multi-language Support | UI text and templates | Requires i18n framework | TODO-ITSM-001-18 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| SLA Policy Selection | No validation that SLA time targets are realistic | TODO-ITSM-001-01 |
| Circular Dependencies in Impact Analysis | No cycle detection in related CIs | TODO-ITSM-001-02 |
| Escalation Loop Prevention | No max escalation depth limit | TODO-ITSM-001-03 |
| Assignment Skill Matching | No skill-level enforcement for L1/L2/L3 tiers | TODO-ITSM-001-04 |
| Status Transition Rules | State machine not fully validated on each transition | TODO-ITSM-001-05 |
| Comment Author Validation | No verification that assignee or reporter can comment | TODO-ITSM-001-06 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-ITSM-001-01 | Implement SLA policy validation to ensure realistic time targets | P1 | Validation |
| TODO-ITSM-001-02 | Add cycle detection algorithm to ImpactAnalysisService to prevent circular dependencies | P1 | Performance |
| TODO-ITSM-001-03 | Implement max escalation depth limit (suggest 5 levels) to prevent escalation loops | P1 | Validation |
| TODO-ITSM-001-04 | Add skill-level matching in IncidentAssignmentService for L1/L2/L3 tier enforcement | P2 | Business Logic |
| TODO-ITSM-001-05 | Create IncidentStateValidator to enforce state machine on all status transitions | P1 | Validation |
| TODO-ITSM-001-06 | Add comment permission checks: assignee, reporter, manager, or support team | P2 | Security |
| TODO-ITSM-001-07 | Implement IncidentNumberGenerator with format: INC-{YY}{MM}-{sequentialID} | P2 | Data Generation |
| TODO-ITSM-001-08 | Create escalation rule builder UI for administrators (drag-drop or form-based) | P2 | Frontend |
| TODO-ITSM-001-09 | Implement incident bulk actions: bulk assign, bulk status change, bulk delete | P2 | Batch Processing |
| TODO-ITSM-001-10 | Add SLA compliance dashboard showing breaches and at-risk incidents in real-time | P1 | Dashboard |
| TODO-ITSM-001-11 | Implement email notifications for incident creation, assignment, escalation, resolution | P2 | Communication |
| TODO-ITSM-001-12 | Create incident metrics calculation service (MTTR, MTTA, SLA compliance %) | P2 | Analytics |
| TODO-ITSM-001-13 | Build incident search with Meilisearch/Algolia full-text indexing | P2 | Search |
| TODO-ITSM-001-14 | Implement incident categorization ML model using historical data | P3 | AI/ML |
| TODO-ITSM-001-15 | Integrate with CMDB module for affected CI tracking and impact propagation | P2 | Integration |
| TODO-ITSM-001-16 | Add webhook event publishing for incident state changes (incident.created, incident.escalated, etc.) | P3 | Integration |
| TODO-ITSM-001-17 | Create predictive escalation model using AI to anticipate escalation needs | P3 | AI/ML |
| TODO-ITSM-001-18 | Add multi-language support with i18n for incident UI and notifications | P3 | Localization |
| TODO-ITSM-001-19 | Implement incident archive mechanism for incidents >90 days closed | P3 | Maintenance |
| TODO-ITSM-001-20 | Create incident SLA reporting with export to PDF/Excel (14 standard reports) | P2 | Reporting |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | Copilot | Initial ITSM-001 Incident Management specification |

---

## Related Documents

- [SPEC-ITSM-002-ProblemManagement.md](SPEC-ITSM-002-ProblemManagement.md) (Pending)
- [SPEC-ITSM-003-ChangeManagement.md](SPEC-ITSM-003-ChangeManagement.md) (Pending)
- [SPEC-ITSM-004-CMDB.md](SPEC-ITSM-004-CMDB.md) (Pending)
- [specifications/INDEX.md](INDEX.md) - Specification index
- [docs/MASTER_TODO_LIST.md](../MASTER_TODO_LIST.md) - Consolidated TODO list

# ITSM Change Management Specification

> **Spec ID:** SPEC-ITSM-003  
> **Feature:** Change Management  
> **Module:** ITSM (IT Service Management)  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description

Change Management is a core ITSM process that controls the lifecycle of IT changes. It ensures that proposed changes are properly assessed for impact, scheduled during appropriate windows, approved by the Change Advisory Board (CAB), and executed with rollback procedures in place. The system prevents conflicting changes, manages blackout windows, and tracks change-related incidents.

**Key Objectives:**
- Minimize unplanned downtime and failed changes
- Ensure proper authorization before deployment
- Track change history and impact for auditing
- Enable rapid rollback if issues occur
- Prevent concurrent conflicting changes

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Change Request Submission | Users submit change requests with description, affected CIs, implementation steps | ❌ Not Implemented |
| SF-002 | CAB Approval Workflow | Change Advisory Board votes on requests; can approve, request info, or reject | ❌ Not Implemented |
| SF-003 | Change Scheduling | Assign implementation date/time; detect blackout windows and conflicts | ❌ Not Implemented |
| SF-004 | Impact Assessment | Calculate impact radius: affected CIs, dependent services, estimated downtime | ❌ Not Implemented |
| SF-005 | Change Calendar | Visual calendar showing scheduled changes, blackout windows, and availability | ❌ Not Implemented |
| SF-006 | Rollback Procedures | Define and document rollback steps; track rollback execution history | ❌ Not Implemented |
| SF-007 | Blackout Windows | Define periods when no changes allowed (maintenance windows, holidays, etc.) | ❌ Not Implemented |
| SF-008 | Change Automation Integration | Trigger workflows/runbooks upon change approval and execution | ❌ Not Implemented |
| SF-009 | Change History & Audit Trail | Complete audit log of all change events with timestamps and actors | ❌ Not Implemented |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Submit Emergency Change | Change Manager | Change urgency detected | Change marked EMERGENCY, expedited review | ❌ |
| UC-002 | CAB Votes on Change | CAB Members | Change in SUBMITTED state | Change moves to APPROVED/REJECTED/INFO_REQUESTED | ❌ |
| UC-003 | Schedule Change Execution | Change Manager | Change APPROVED | Conflict check performed, blackout window warning shown | ❌ |
| UC-004 | View Impact Analysis | Change Manager | Change SCHEDULED | Impact dashboard shows affected CIs, services, teams | ❌ |
| UC-005 | Execute Change | Implementation Team | Change SCHEDULED, outside blackout window | Change state → IMPLEMENTING, tickets created for incident tracking | ❌ |
| UC-006 | Rollback Failed Change | Implementation Team | Change in ERROR state | Rollback plan executed, change → ROLLED_BACK, incidents created | ❌ |
| UC-007 | View Change Calendar | Operations Team | System online | Calendar shows all changes, blackout windows, team assignments | ❌ |
| UC-008 | Define Blackout Window | Admin | System online | Recurring or one-time blackout created, affects future scheduling | ❌ |
| UC-009 | Detect Concurrent Changes | Change Scheduler | Multiple changes scheduled same time | Warning displayed, alternative times suggested | ❌ |
| UC-010 | Change Risk Assessment | Risk Analyst | Change SUBMITTED | Risk score calculated (HIGH/MEDIUM/LOW) based on impact | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ChangeRequestListPage | `CRM.Frontend/src/pages/itsm/ChangeRequestListPage.tsx` | ❌ Not Implemented | List all change requests with filters by status, priority, date |
| ChangeRequestDetailPage | `CRM.Frontend/src/pages/itsm/ChangeRequestDetailPage.tsx` | ❌ Not Implemented | View/edit change request details, impact analysis, audit trail |
| ChangeRequestFormPage | `CRM.Frontend/src/pages/itsm/ChangeRequestFormPage.tsx` | ❌ Not Implemented | Form to create/edit change requests |
| CABApprovalPage | `CRM.Frontend/src/pages/itsm/CABApprovalPage.tsx` | ❌ Not Implemented | Vote on pending changes, comment, view impact summary |
| ChangeCalendarPage | `CRM.Frontend/src/pages/itsm/ChangeCalendarPage.tsx` | ❌ Not Implemented | Calendar view of scheduled changes and blackout windows |
| ChangeSchedulingPage | `CRM.Frontend/src/pages/itsm/ChangeSchedulingPage.tsx` | ❌ Not Implemented | Schedule approved changes; show conflicts and alternatives |
| ImpactAnalysisPage | `CRM.Frontend/src/pages/itsm/ImpactAnalysisPage.tsx` | ❌ Not Implemented | Network diagram of affected CIs, services, dependencies |
| RollbackProceduresPage | `CRM.Frontend/src/pages/itsm/RollbackProceduresPage.tsx` | ❌ Not Implemented | Define, test, and execute rollback procedures |
| BlackoutWindowsPage | `CRM.Frontend/src/pages/itsm/BlackoutWindowsPage.tsx` | ❌ Not Implemented | Manage maintenance windows and no-change periods |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ChangeRequestForm | `CRM.Frontend/src/components/itsm/ChangeRequestForm.tsx` | ❌ | Form fields: title, description, affected CIs, implementation plan |
| CABVotingPanel | `CRM.Frontend/src/components/itsm/CABVotingPanel.tsx` | ❌ | Vote buttons: Approve/Reject/MoreInfo; shows voting status |
| ChangeCalendarWidget | `CRM.Frontend/src/components/itsm/ChangeCalendarWidget.tsx` | ❌ | React Calendar showing scheduled changes and blackout windows |
| ImpactNetworkDiagram | `CRM.Frontend/src/components/itsm/ImpactNetworkDiagram.tsx` | ❌ | D3/Cytoscape visualization of CI dependencies |
| RollbackStepList | `CRM.Frontend/src/components/itsm/RollbackStepList.tsx` | ❌ | Ordered list of rollback procedures with execution tracking |
| ConflictWarning | `CRM.Frontend/src/components/itsm/ConflictWarning.tsx` | ❌ | Alert showing conflicting changes and suggestions |
| ChangeStatusBadge | `CRM.Frontend/src/components/itsm/ChangeStatusBadge.tsx` | ❌ | Status badge with color coding (SUBMITTED/APPROVED/REJECTED/etc.) |
| BlackoutWindowSelector | `CRM.Frontend/src/components/itsm/BlackoutWindowSelector.tsx` | ❌ | Date/time range picker with recurring options |
| ChangeTimelineView | `CRM.Frontend/src/components/itsm/ChangeTimelineView.tsx` | ❌ | Timeline showing change history, approvals, scheduling, execution |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| changeService | `CRM.Frontend/src/services/itsm/changeService.ts` | getAll, getById, create, update, delete, submit, schedule, execute, rollback | ❌ |
| cabApprovalService | `CRM.Frontend/src/services/itsm/cabApprovalService.ts` | getPendingChanges, vote, addComment, getVotingHistory | ❌ |
| changeSchedulerService | `CRM.Frontend/src/services/itsm/changeSchedulerService.ts` | checkConflicts, suggestAlternativeTimes, detectBlackouts | ❌ |
| impactAnalysisService | `CRM.Frontend/src/services/itsm/impactAnalysisService.ts` | calculateImpact, getAffectedCIs, getDependencies, estimateDowntime | ❌ |
| rollbackService | `CRM.Frontend/src/services/itsm/rollbackService.ts` | getPlan, executePlan, trackRollback, getSupportingInfo | ❌ |
| blackoutWindowService | `CRM.Frontend/src/services/itsm/blackoutWindowService.ts` | getAll, create, update, delete, checkBlackout | ❌ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Change Title | Required, 10-200 chars | Frontend/Backend | ❌ Not Implemented |
| Description | Required, 50-5000 chars | Frontend/Backend | ❌ |
| Affected CIs | Min 1 CI selected | Frontend/Backend | ❌ |
| Implementation Plan | Required, clear steps | Frontend/Backend | ❌ |
| Scheduled Date/Time | Must be in future, outside blackout window | Frontend/Backend | ❌ |
| Risk Assessment | Auto-calculated from impact | Backend | ❌ |
| CAB Voting | Minimum votes required for decision | Backend | ❌ |
| Rollback Plan | Required for MEDIUM/HIGH risk changes | Frontend/Backend | ❌ |
| Implementation Steps | Should include verification steps | Frontend/Backend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| Change | `CRM.Core/Entities/ITSM/Change.cs` | ❌ Not Implemented | Change request aggregate root |
| ChangeStatus enum | `CRM.Core/Entities/ITSM/ChangeStatus.cs` | ❌ | SUBMITTED, APPROVED, REJECTED, INFO_REQUESTED, SCHEDULED, IMPLEMENTING, COMPLETED, ROLLED_BACK, CANCELLED, ERROR |
| ChangeType enum | `CRM.Core/Entities/ITSM/ChangeType.cs` | ❌ | NORMAL, STANDARD, EMERGENCY |
| ChangeRiskLevel enum | `CRM.Core/Entities/ITSM/ChangeRiskLevel.cs` | ❌ | LOW, MEDIUM, HIGH, CRITICAL |
| CABVote | `CRM.Core/Entities/ITSM/CABVote.cs` | ❌ | CAB member vote with comment |
| CABVoteDecision enum | `CRM.Core/Entities/ITSM/CABVoteDecision.cs` | ❌ | APPROVE, REJECT, MORE_INFO, ABSTAIN |
| ChangeSchedule | `CRM.Core/Entities/ITSM/ChangeSchedule.cs` | ❌ | Scheduled execution date/time |
| ChangeImpact | `CRM.Core/Entities/ITSM/ChangeImpact.cs` | ❌ | Calculated impact metrics |
| ChangeImplementation | `CRM.Core/Entities/ITSM/ChangeImplementation.cs` | ❌ | Implementation step tracking |
| RollbackPlan | `CRM.Core/Entities/ITSM/RollbackPlan.cs` | ❌ | Rollback procedure and execution history |
| RollbackStep | `CRM.Core/Entities/ITSM/RollbackStep.cs` | ❌ | Individual rollback step |
| BlackoutWindow | `CRM.Core/Entities/ITSM/BlackoutWindow.cs` | ❌ | Period when no changes allowed |
| BlackoutWindowRecurrence enum | `CRM.Core/Entities/ITSM/BlackoutWindowRecurrence.cs` | ❌ | ONCE, DAILY, WEEKLY, MONTHLY, YEARLY |
| ChangeConflict | `CRM.Core/Entities/ITSM/ChangeConflict.cs` | ❌ | Detected conflict between changes |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ChangeDto | `CRM.Core/DTOs/ITSM/ChangeDto.cs` | ❌ Not Implemented | Standard change view DTO |
| CreateChangeDto | `CRM.Core/DTOs/ITSM/CreateChangeDto.cs` | ❌ | Creation payload with validation |
| UpdateChangeDto | `CRM.Core/DTOs/ITSM/UpdateChangeDto.cs` | ❌ | Update payload |
| ChangeDetailDto | `CRM.Core/DTOs/ITSM/ChangeDetailDto.cs` | ❌ | Extended DTO with all related data |
| CABVoteDto | `CRM.Core/DTOs/ITSM/CABVoteDto.cs` | ❌ | Vote submission DTO |
| ChangeImpactDto | `CRM.Core/DTOs/ITSM/ChangeImpactDto.cs` | ❌ | Impact analysis results |
| ChangeConflictDto | `CRM.Core/DTOs/ITSM/ChangeConflictDto.cs` | ❌ | Conflict detection results |
| RollbackPlanDto | `CRM.Core/DTOs/ITSM/RollbackPlanDto.cs` | ❌ | Rollback procedure DTO |
| SchedulingConflictDto | `CRM.Core/DTOs/ITSM/SchedulingConflictDto.cs` | ❌ | Conflict check results with suggestions |
| BlackoutWindowDto | `CRM.Core/DTOs/ITSM/BlackoutWindowDto.cs` | ❌ | Blackout window DTO |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IChangeService | `CRM.Core/Interfaces/ITSM/IChangeService.cs` | GetAll, GetById, Create, Update, Delete, SubmitForApproval, Approve, Reject, RequestMoreInfo, Schedule, Execute, CompleteChange, CreateIncident, Rollback, Cancel, GetHistory | ❌ Not Implemented |
| ICABApprovalEngine | `CRM.Core/Interfaces/ITSM/ICABApprovalEngine.cs` | SubmitForVoting, RecordVote, GetCABMembers, EvaluateApproval, GetVotingStatus, GetApprovalHistory | ❌ |
| IChangeScheduler | `CRM.Core/Interfaces/ITSM/IChangeScheduler.cs` | ScheduleChange, CheckConflicts, DetectBlackouts, SuggestAlternativeTimes, GetSchedule, RescheduleChange, UpdateSchedule | ❌ |
| IImpactCalculator | `CRM.Core/Interfaces/ITSM/IImpactCalculator.cs` | CalculateImpact, GetAffectedCIs, GetDependencies, EstimateDowntime, CalculateRiskScore, GetImpactReport | ❌ |
| IRollbackCoordinator | `CRM.Core/Interfaces/ITSM/IRollbackCoordinator.cs` | CreateRollbackPlan, ExecuteRollback, TrackRollbackProgress, ValidateRollbackCompleteness, GetRollbackHistory, TestRollback | ❌ |
| IBlackoutWindowService | `CRM.Core/Interfaces/ITSM/IBlackoutWindowService.cs` | GetAll, Create, Update, Delete, IsBlackoutTime, GetNextAvailableWindow | ❌ |
| IChangeConflictDetector | `CRM.Core/Interfaces/ITSM/IChangeConflictDetector.cs` | DetectConflicts, CheckCIConflicts, CheckDependencyConflicts, GetConflictResolution | ❌ |

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ChangeService | `CRM.Infrastructure/Services/ITSM/ChangeService.cs` | Full IChangeService implementation | ❌ Not Implemented |
| CABApprovalEngine | `CRM.Infrastructure/Services/ITSM/CABApprovalEngine.cs` | Full ICABApprovalEngine implementation | ❌ |
| ChangeScheduler | `CRM.Infrastructure/Services/ITSM/ChangeScheduler.cs` | Full IChangeScheduler implementation | ❌ |
| ImpactCalculator | `CRM.Infrastructure/Services/ITSM/ImpactCalculator.cs` | Full IImpactCalculator implementation | ❌ |
| RollbackCoordinator | `CRM.Infrastructure/Services/ITSM/RollbackCoordinator.cs` | Full IRollbackCoordinator implementation | ❌ |
| BlackoutWindowService | `CRM.Infrastructure/Services/ITSM/BlackoutWindowService.cs` | Full IBlackoutWindowService implementation | ❌ |
| ChangeConflictDetector | `CRM.Infrastructure/Services/ITSM/ChangeConflictDetector.cs` | Full IChangeConflictDetector implementation | ❌ |

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ChangesController | `CRM.Api/Controllers/ITSM/ChangesController.cs` | 25 endpoints (CRUD, approval, scheduling, rollback) | ❌ Not Implemented |
| CABApprovalController | `CRM.Api/Controllers/ITSM/CABApprovalController.cs` | 8 endpoints (voting, history, status) | ❌ |
| ChangeSchedulingController | `CRM.Api/Controllers/ITSM/ChangeSchedulingController.cs` | 6 endpoints (schedule, conflicts, alternatives) | ❌ |
| ImpactAnalysisController | `CRM.Api/Controllers/ITSM/ImpactAnalysisController.cs` | 6 endpoints (impact calculation, CI dependencies) | ❌ |
| RollbackController | `CRM.Api/Controllers/ITSM/RollbackController.cs` | 8 endpoints (plan, execute, track, validate) | ❌ |
| BlackoutWindowsController | `CRM.Api/Controllers/ITSM/BlackoutWindowsController.cs` | 6 endpoints (CRUD, availability check) | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/itsm/changes` | GetAll | Yes | ❌ |
| POST | `/api/itsm/changes` | Create | Yes | ❌ |
| GET | `/api/itsm/changes/{id}` | GetById | Yes | ❌ |
| PUT | `/api/itsm/changes/{id}` | Update | Yes | ❌ |
| DELETE | `/api/itsm/changes/{id}` | Delete | Yes | ❌ |
| POST | `/api/itsm/changes/{id}/submit` | SubmitForApproval | Yes | ❌ |
| POST | `/api/itsm/changes/{id}/schedule` | Schedule | Yes | ❌ |
| GET | `/api/itsm/changes/{id}/schedule/conflicts` | CheckConflicts | Yes | ❌ |
| GET | `/api/itsm/changes/{id}/schedule/alternatives` | GetAlternatives | Yes | ❌ |
| GET | `/api/itsm/changes/{id}/impact` | GetImpactAnalysis | Yes | ❌ |
| POST | `/api/itsm/changes/{id}/execute` | ExecuteChange | Yes | ❌ |
| POST | `/api/itsm/changes/{id}/rollback` | RollbackChange | Yes | ❌ |
| GET | `/api/itsm/cab/pending` | GetPendingChanges | Yes | ❌ |
| POST | `/api/itsm/cab/{changeId}/vote` | VoteOnChange | Yes | ❌ |
| GET | `/api/itsm/cab/{changeId}/votes` | GetVotes | Yes | ❌ |
| POST | `/api/itsm/rollback/{changeId}/plan` | CreateRollbackPlan | Yes | ❌ |
| POST | `/api/itsm/rollback/{changeId}/execute` | ExecuteRollbackPlan | Yes | ❌ |
| GET | `/api/itsm/rollback/{changeId}/progress` | GetRollbackProgress | Yes | ❌ |
| GET | `/api/itsm/blackout-windows` | GetAll | Yes | ❌ |
| POST | `/api/itsm/blackout-windows` | Create | Yes | ❌ |
| GET | `/api/itsm/calendar` | GetChangeCalendar | Yes | ❌ |
| GET | `/api/itsm/changes/{id}/history` | GetHistory | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| Change.Title | Required, 10-200 chars | Entity/DTO | ❌ |
| Change.Description | Required, 50-5000 chars | Entity/DTO | ❌ |
| Change.AffectedCIs | Min 1, max 100 CIs | Entity/Service | ❌ |
| Change.ImplementationSteps | Required, ordered, min 1 | Entity/DTO | ❌ |
| Change.ScheduledDate | Must be in future, outside blackout | Service/Scheduler | ❌ |
| Change.RollbackPlan | Required for MEDIUM/HIGH risk | Entity/Service | ❌ |
| CABVote.Decision | Valid enum value (APPROVE/REJECT/MORE_INFO) | DTO/Service | ❌ |
| CABVote.Comment | Required, 1-1000 chars when REJECT/MORE_INFO | DTO/Service | ❌ |
| RollbackStep.EstimatedDuration | Positive integer (seconds) | Entity | ❌ |
| BlackoutWindow.EndTime | Must be after StartTime | Entity | ❌ |

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| Changes | `database/schema/itsm/changes.sql` | ❌ Not Implemented | Main change request table |
| CABVotes | `database/schema/itsm/cab_votes.sql` | ❌ | CAB member voting records |
| ChangeSchedules | `database/schema/itsm/change_schedules.sql` | ❌ | Scheduling information per change |
| ChangeImpacts | `database/schema/itsm/change_impacts.sql` | ❌ | Calculated impact metrics |
| ChangeImplementations | `database/schema/itsm/change_implementations.sql` | ❌ | Implementation step tracking |
| RollbackPlans | `database/schema/itsm/rollback_plans.sql` | ❌ | Rollback procedure definitions |
| RollbackSteps | `database/schema/itsm/rollback_steps.sql` | ❌ | Individual rollback steps |
| RollbackExecutions | `database/schema/itsm/rollback_executions.sql` | ❌ | Execution history of rollbacks |
| BlackoutWindows | `database/schema/itsm/blackout_windows.sql` | ❌ | Maintenance window definitions |
| ChangeConflicts | `database/schema/itsm/change_conflicts.sql` | ❌ | Detected conflicts between changes |
| ChangeAuditLog | `database/schema/itsm/change_audit_log.sql` | ❌ | Complete change audit trail |

### 4.2 Data Elements (Changes Table)

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ✅ |
| ChangeNumber | VARCHAR(50) | No | | UK | ChangeNumber | ❌ |
| Title | VARCHAR(255) | No | | | Title | ❌ |
| Description | TEXT | No | | | Description | ❌ |
| ChangeType | INT | No | 0 (NORMAL) | FK: ChangeType | ChangeType | ❌ |
| Status | INT | No | 0 (SUBMITTED) | FK: ChangeStatus | Status | ❌ |
| RiskLevel | INT | No | 1 (MEDIUM) | FK: ChangeRiskLevel | RiskLevel | ❌ |
| RequesterId | INT | No | | FK: Users | RequesterId | ❌ |
| ImplementerId | INT | Yes | NULL | FK: Users | ImplementerId | ❌ |
| ApprovedById | INT | Yes | NULL | FK: Users | ApprovedById | ❌ |
| SubmittedAt | DATETIME | No | | | SubmittedAt | ❌ |
| ScheduledStartTime | DATETIME | Yes | NULL | | ScheduledStartTime | ❌ |
| ScheduledEndTime | DATETIME | Yes | NULL | | ScheduledEndTime | ❌ |
| ActualStartTime | DATETIME | Yes | NULL | | ActualStartTime | ❌ |
| ActualEndTime | DATETIME | Yes | NULL | | ActualEndTime | ❌ |
| EstimatedDowntime | INT | Yes | NULL | | EstimatedDowntimeSeconds | ❌ |
| BackoutPlan | TEXT | Yes | NULL | | BackoutPlan | ❌ |
| TestResult | VARCHAR(50) | Yes | NULL | | TestResult | ❌ |
| ImpactRadius | TEXT | Yes | NULL | JSON: affected CIs | ImpactRadius | ❌ |
| CABApprovalRequired | BOOLEAN | No | FALSE | | CABApprovalRequired | ❌ |
| CABApprovalDate | DATETIME | Yes | NULL | | CABApprovalDate | ❌ |
| RollbackExecuted | BOOLEAN | No | FALSE | | RollbackExecuted | ❌ |
| RollbackReason | VARCHAR(500) | Yes | NULL | | RollbackReason | ❌ |
| ParentChangeId | INT | Yes | NULL | FK: Changes (self) | ParentChangeId | ❌ |
| RelatedIncidentIds | TEXT | Yes | NULL | JSON array | RelatedIncidentIds | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | | CreatedAt | ✅ |
| UpdatedAt | DATETIME | Yes | NULL | | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | | IsDeleted | ❌ |
| RowVersion | BINARY(8) | Yes | NULL | | RowVersion | ❌ |

### 4.3 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| Changes | Users (Requester) | N:1 | RequesterId | ❌ |
| Changes | Users (Implementer) | N:1 | ImplementerId | ❌ |
| Changes | Users (Approver) | N:1 | ApprovedById | ❌ |
| Changes | Changes (Parent) | N:1 (self) | ParentChangeId | ❌ |
| CABVotes | Changes | N:1 | ChangeId | ❌ |
| CABVotes | Users (Voter) | N:1 | VoterId | ❌ |
| ChangeSchedules | Changes | 1:1 | ChangeId | ❌ |
| ChangeImpacts | Changes | 1:1 | ChangeId | ❌ |
| ChangeImplementations | Changes | 1:N | ChangeId | ❌ |
| RollbackPlans | Changes | 1:1 | ChangeId | ❌ |
| RollbackSteps | RollbackPlans | 1:N | RollbackPlanId | ❌ |
| RollbackExecutions | RollbackPlans | 1:N | RollbackPlanId | ❌ |
| BlackoutWindows | (standalone) | - | - | ❌ |
| ChangeConflicts | Changes (Change1) | N:1 | ChangeId1 | ❌ |
| ChangeConflicts | Changes (Change2) | N:1 | ChangeId2 | ❌ |

### 4.4 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_Changes_Status | Changes | Status | NonClustered | ❌ |
| IX_Changes_RiskLevel | Changes | RiskLevel | NonClustered | ❌ |
| IX_Changes_ScheduledStartTime | Changes | ScheduledStartTime | NonClustered | ❌ |
| IX_Changes_RequesterId | Changes | RequesterId | NonClustered | ❌ |
| IX_Changes_IsDeleted | Changes | IsDeleted | NonClustered | ❌ |
| IX_CABVotes_ChangeId | CABVotes | ChangeId | NonClustered | ❌ |
| IX_CABVotes_VoterId | CABVotes | VoterId | NonClustered | ❌ |
| IX_BlackoutWindows_StartTime | BlackoutWindows | StartTime | NonClustered | ❌ |
| IX_ChangeConflicts_ChangeId1 | ChangeConflicts | ChangeId1, ChangeId2 | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ChangeServiceTests | `CRM.Tests/ITSM/Services/ChangeServiceTests.cs` | 35+ tests | ❌ Not Implemented |
| CABApprovalEngineTests | `CRM.Tests/ITSM/Services/CABApprovalEngineTests.cs` | 25+ tests | ❌ |
| ChangeSchedulerTests | `CRM.Tests/ITSM/Services/ChangeSchedulerTests.cs` | 20+ tests | ❌ |
| ImpactCalculatorTests | `CRM.Tests/ITSM/Services/ImpactCalculatorTests.cs` | 25+ tests | ❌ |
| RollbackCoordinatorTests | `CRM.Tests/ITSM/Services/RollbackCoordinatorTests.cs` | 20+ tests | ❌ |
| BlackoutWindowServiceTests | `CRM.Tests/ITSM/Services/BlackoutWindowServiceTests.cs` | 15+ tests | ❌ |
| ChangeConflictDetectorTests | `CRM.Tests/ITSM/Services/ChangeConflictDetectorTests.cs` | 20+ tests | ❌ |

**Key Test Scenarios:**

**ChangeServiceTests:**
- Create change with valid/invalid data
- Submit change for approval
- Track change through lifecycle
- Cancel/reject changes
- Get change history and audit trail
- Soft delete handling
- Concurrent change handling

**CABApprovalEngineTests:**
- CAB voting state machine (SUBMITTED → APPROVED/REJECTED/INFO_REQUESTED)
- Minimum votes requirement enforcement
- Vote counting and decision logic
- Comment validation on rejection
- Voting timeout scenarios
- Change of vote scenarios

**ChangeSchedulerTests:**
- Schedule change outside blackout window
- Detect concurrent changes
- Suggest alternative times
- Reschedule already-scheduled changes
- Blackout window detection
- Conflict resolution logic

**ImpactCalculatorTests:**
- Calculate impact radius from change
- Cascade impact through CI dependencies
- Estimate downtime
- Risk score calculation (LOW/MEDIUM/HIGH/CRITICAL)
- Multiple affected services
- Rollback complexity scoring

**RollbackCoordinatorTests:**
- Create rollback plan from change
- Execute rollback in reverse order
- Track rollback progress
- Validate rollback completeness
- Rollback failure scenarios
- Rollback history tracking

**BlackoutWindowServiceTests:**
- Create recurring blackout windows
- Check if time is within blackout
- Get next available time slot
- Conflict detection between windows
- Timezone handling

**ChangeConflictDetectorTests:**
- Detect same-CI conflicts
- Detect dependency conflicts
- Get conflict resolution suggestions
- Alternative scheduling

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ChangeServiceIntegrationTests | `CRM.Tests/Integration/ITSM/ChangeServiceIntegrationTests.cs` | 20+ tests | ❌ Not Implemented |
| CABWorkflowIntegrationTests | `CRM.Tests/Integration/ITSM/CABWorkflowIntegrationTests.cs` | 15+ tests | ❌ |
| ChangeSchedulingIntegrationTests | `CRM.Tests/Integration/ITSM/ChangeSchedulingIntegrationTests.cs` | 15+ tests | ❌ |
| ImpactCascadeIntegrationTests | `CRM.Tests/Integration/ITSM/ImpactCascadeIntegrationTests.cs` | 15+ tests | ❌ |
| EndToEndChangeLifecycleTests | `CRM.Tests/Integration/ITSM/EndToEndChangeLifecycleTests.cs` | 10+ tests | ❌ |

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ChangeManagement.spec.ts | `e2e-tests/tests/itsm/change-management.spec.ts` | 20+ tests | ❌ Not Implemented |
| CABApprovalWorkflow.spec.ts | `e2e-tests/tests/itsm/cab-approval-workflow.spec.ts` | 15+ tests | ❌ |
| ChangeScheduling.spec.ts | `e2e-tests/tests/itsm/change-scheduling.spec.ts` | 15+ tests | ❌ |
| RollbackProcedures.spec.ts | `e2e-tests/tests/itsm/rollback-procedures.spec.ts` | 10+ tests | ❌ |

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| Change.ScheduledStartTime (DATETIME) | Change.ScheduledEndTime (DATETIME) | Start/end relationship unclear | Ensure EndTime > StartTime in validation |
| Change.EstimatedDowntime (INT seconds) | RollbackStep.EstimatedDuration (INT seconds) | Unit consistency | Document units consistently in DTO |
| ImpactRadius (TEXT/JSON) | ChangeImpact table | Duplication of impact data | Store structured data in ChangeImpact, reference from Changes |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| ImpactNetworkDiagram component | CRM.Frontend/components | Dependency visualization not yet built | TODO-ITSM003-015 |
| CAB Voting quorum enforcement | CABApprovalEngine | Minimum voting requirements not defined | TODO-ITSM003-008 |
| Automated rollback triggering | ChangeService | Auto-rollback on error not implemented | TODO-ITSM003-020 |
| Change template library | ChangeService | Reusable change templates missing | TODO-ITSM003-021 |
| Change impact simulation | ImpactCalculator | What-if analysis not implemented | TODO-ITSM003-022 |
| Change communication/notifications | ChangeService | Stakeholder notifications not triggered | TODO-ITSM003-023 |
| Change dashboard widgets | Pages | Real-time dashboard missing | TODO-ITSM003-024 |
| Historical trend analysis | ImpactAnalysisService | Success rate trends not calculated | TODO-ITSM003-025 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| Change.ImplementationSteps | No minimum quality standards defined | TODO-ITSM003-011 |
| CAB member absence | No quorum rules when CAB members unavailable | TODO-ITSM003-012 |
| Rollback failure recovery | What to do if rollback fails | TODO-ITSM003-013 |
| Emergency change bypass | Who can bypass CAB approval and under what conditions | TODO-ITSM003-014 |
| CI dependency depth | How deep to cascade impact calculation | TODO-ITSM003-026 |
| Change blast radius limit | Should there be a maximum impact threshold | TODO-ITSM003-027 |

### 6.4 Concurrent Change Conflicts

| Scenario | Current Behavior | Expected Behavior | TODO ID |
|----------|------------------|-------------------|---------|
| Two changes scheduled for same CI at overlapping times | Detected but user choice unclear | Reject second or suggest alternative | TODO-ITSM003-028 |
| Change A depends on Change B not completing | No dependency tracking | Add prerequisite change tracking | TODO-ITSM003-029 |
| Rollback during concurrent change execution | Undefined behavior | Queue rollback or reject conflicting change | TODO-ITSM003-030 |

### 6.5 Rollback Completeness Validation

| Scenario | Current Behavior | Expected Behavior | TODO ID |
|----------|------------------|-------------------|---------|
| Partial rollback (some steps fail) | Unclear state | Track which steps completed/failed | TODO-ITSM003-031 |
| Rollback verification steps | Not defined | Add pre- and post-rollback checks | TODO-ITSM003-032 |
| Rollback timeout handling | No timeout defined | Define and enforce rollback timeout | TODO-ITSM003-033 |

---

## 7. TODO Items (→ Master TODO)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-ITSM003-001 | Implement IChangeService with full change lifecycle management | P1 | Backend Implementation |
| TODO-ITSM003-002 | Implement CABApprovalEngine with voting state machine | P1 | Backend Implementation |
| TODO-ITSM003-003 | Implement ChangeScheduler with blackout window detection | P1 | Backend Implementation |
| TODO-ITSM003-004 | Implement ImpactCalculator with CI dependency cascading | P1 | Backend Implementation |
| TODO-ITSM003-005 | Implement RollbackCoordinator with rollback execution tracking | P1 | Backend Implementation |
| TODO-ITSM003-006 | Implement ChangesController with 25 REST endpoints | P1 | API Implementation |
| TODO-ITSM003-007 | Implement CABApprovalController with voting endpoints | P1 | API Implementation |
| TODO-ITSM003-008 | Define CAB voting quorum rules and enforcement logic | P1 | Requirements |
| TODO-ITSM003-009 | Create database schema: Changes, CABVotes, ChangeSchedules, RollbackPlans, BlackoutWindows tables | P1 | Database |
| TODO-ITSM003-010 | Create database seed data: status enums, risk levels, change types | P1 | Database |
| TODO-ITSM003-011 | Define implementation step quality standards and validation rules | P1 | Requirements |
| TODO-ITSM003-012 | Define CAB quorum rules when members unavailable | P1 | Requirements |
| TODO-ITSM003-013 | Document rollback failure recovery procedures | P1 | Requirements |
| TODO-ITSM003-014 | Define emergency change bypass criteria and approvers | P1 | Requirements |
| TODO-ITSM003-015 | Build ImpactNetworkDiagram component for CI dependency visualization | P2 | Frontend |
| TODO-ITSM003-016 | Build ChangeRequestForm component with validation | P2 | Frontend |
| TODO-ITSM003-017 | Build CABVotingPanel component | P2 | Frontend |
| TODO-ITSM003-018 | Build ChangeCalendarPage with conflict visualization | P2 | Frontend |
| TODO-ITSM003-019 | Build RollbackProceduresPage with execution tracking | P2 | Frontend |
| TODO-ITSM003-020 | Implement automated rollback triggering on change error | P2 | Backend Enhancement |
| TODO-ITSM003-021 | Create change template library for reusable change definitions | P2 | Backend Enhancement |
| TODO-ITSM003-022 | Implement change impact simulation (what-if analysis) | P2 | Backend Enhancement |
| TODO-ITSM003-023 | Implement change communication/notification system | P2 | Backend Enhancement |
| TODO-ITSM003-024 | Create change management dashboard widgets | P2 | Frontend |
| TODO-ITSM003-025 | Implement historical trend analysis (success rates, avg duration) | P3 | Analytics |
| TODO-ITSM003-026 | Define CI dependency depth limits for impact calculation | P3 | Requirements |
| TODO-ITSM003-027 | Define change blast radius threshold | P3 | Requirements |
| TODO-ITSM003-028 | Implement concurrent change conflict resolution strategy | P2 | Backend Enhancement |
| TODO-ITSM003-029 | Add prerequisite change tracking and dependency validation | P2 | Backend Enhancement |
| TODO-ITSM003-030 | Handle rollback during concurrent change execution scenarios | P2 | Backend Enhancement |
| TODO-ITSM003-031 | Track partial rollback scenarios with completion status | P2 | Backend Enhancement |
| TODO-ITSM003-032 | Add pre- and post-rollback verification steps | P2 | Backend Enhancement |
| TODO-ITSM003-033 | Define and enforce rollback timeout with escalation | P2 | Backend Enhancement |
| TODO-ITSM003-034 | Write 35+ ChangeServiceTests unit tests | P2 | Testing |
| TODO-ITSM003-035 | Write 25+ CABApprovalEngineTests unit tests | P2 | Testing |
| TODO-ITSM003-036 | Write 20+ ChangeSchedulerTests unit tests | P2 | Testing |
| TODO-ITSM003-037 | Write 25+ ImpactCalculatorTests unit tests | P2 | Testing |
| TODO-ITSM003-038 | Write 20+ RollbackCoordinatorTests unit tests | P2 | Testing |
| TODO-ITSM003-039 | Write 20+ integration tests for end-to-end change lifecycle | P2 | Testing |
| TODO-ITSM003-040 | Write 20+ E2E tests with Playwright for change workflows | P2 | Testing |
| TODO-ITSM003-041 | Create operator documentation: Change Management User Guide | P3 | Documentation |
| TODO-ITSM003-042 | Create operator documentation: CAB Approval Process | P3 | Documentation |
| TODO-ITSM003-043 | Create operator documentation: Rollback Procedures | P3 | Documentation |
| TODO-ITSM003-044 | Create developer documentation: API Reference | P3 | Documentation |
| TODO-ITSM003-045 | Add change management to ITSM dashboard | P3 | Frontend |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | February 14, 2026 | AI | Initial specification - comprehensive change management workflow covering CAB approvals, scheduling, impact analysis, and rollback procedures |

---

**END OF SPECIFICATION**

# P0/P1 Critical Backend Services Implementation Plan
> **Status:** In Progress  
> **Date Started:** February 16, 2026  
> **Estimated Duration:** 192 hours (8-10 day sprint)  
> **Branch:** feature/p0-p1-architecture-specs-2026-02-16

## Executive Summary

This document tracks the implementation of 7 critical backend service suites for the CRM solution:
1. **ITSM Tier-1 Re-enablement** (8 hours)
2. **Admin Configuration Services** (24 hours)  
3. **ITSM Problem Management** (35 hours)
4. **ITSM Change Management** (50 hours)
5. **Commission Rules Engine** (20 hours)
6. **Subscription Billing Services** (25 hours)
7. **Email Sequence Service Enhancements** (20 hours)

---

## Phase 1: ITSM Tier-1 Re-enablement (8 hours)

### Services to Re-enable
- ✅ **BusinessHoursCalculator** - Exists, needs re-enable
- ✅ **IncidentService** - Exists (431 lines), needs re-enable  
- ✅ **SLAService** - Exists (484 lines), needs re-enable
- ✅ **ServiceQueueService** - Exists, needs re-enable

### Task List
- [ ] Re-enable BusinessHoursCalculator in Program.cs
- [ ] Re-enable IIncidentService in Program.cs
- [ ] Re-enable ISLAService in Program.cs  
- [ ] Verify IncidentService compiles and imports correctly
- [ ] Verify SLAService compiles and imports correctly
- [ ] Add integration tests for ITSM services
- [ ] Build verification (full solution)
- [ ] Smoke tests with sample data

### Status: ⏳ Pending

---

## Phase 2: Admin Configuration Services (24 hours)

### Services to Implement/Re-enable

#### 2.1 CommissionRuleService
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/CommissionRuleService.cs`
- **Status:** Disabled (.disabled exists with 219 lines)
- **Action:** Re-enable from disabled file, verify completeness
- **Methods:** Create, Update, GetById, GetAll, Delete, GetApplicableRules, CalculateCommission
- **DI:** Enable in Program.cs

#### 2.2 DiscountRuleService  
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/DiscountRuleService.cs`
- **Status:** Disabled (.disabled exists)
- **Action:** Re-enable from disabled file, verify completeness
- **Methods:** Create, Update, GetById, GetAll, Delete, GetApplicableRules, CalculateDiscount
- **DI:** Enable in Program.cs

#### 2.3 SLAPolicyService (Admin)
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/SLAPolicyAdminService.cs`
- **Status:** Disabled (.disabled exists)
- **Action:** Re-enable and complete
- **Methods:** CRUD operations for SLA policies + admin configuration
- **DI:** Enable in Program.cs

#### 2.4 EscalationRuleService  
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/EscalationRuleAdminService.cs`
- **Status:** Disabled (.disabled exists)
- **Action:** Re-enable and complete
- **Methods:** CRUD + rule evaluation + ordering
- **DI:** Enable in Program.cs

#### 2.5 ServiceQueueService
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ServiceQueueService.cs`
- **Status:** Exists, may be disabled
- **Action:** Verify and enable
- **Methods:** Queue management + routing rules
- **DI:** Enable in Program.cs

### Task List
- [ ] Re-enable CommissionRuleService from .disabled
- [ ] Re-enable DiscountRuleService from .disabled
- [ ] Re-enable SLAPolicyAdminService from .disabled
- [ ] Re-enable EscalationRuleAdminService from .disabled
- [ ] Verify ServiceQueueService
- [ ] Register all 5 services in Program.cs
- [ ] Verify all services compile
- [ ] Unit tests for each service (5-10 tests each)
- [ ] Integration tests with sample data

### Status: ⏳ Pending

---

## Phase 3: ITSM Problem Management (35 hours)

### Service Details
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs`
- **Status:** Disabled (.disabled exists)
- **Interface:** IProblemService in IITSMServices.cs
- **Methods to Implement:** 25+ methods

### Core Methods
1. **CRUD Operations** (5 methods)
   - CreateProblemAsync
   - UpdateProblemAsync
   - DeleteProblemAsync
   - GetProblemByIdAsync
   - ListProblemsAsync

2. **Workflow Operations** (8 methods)
   - RelateProblemToIncidentsAsync
   - ResolveProblemAsync
   - CloseProblemAsync
   - ReopenProblemAsync
   - LinkIncidentAsync
   - MarkAsKnownErrorAsync
   - GetRelatedIncidentsAsync

3. **Analysis & Documentation** (7 methods)
   - AnalyzeIncidentsAsync
   - DetermineCauseAsync
   - IdentifyTemporaryWorkaroundAsync
   - DocumentProblemAsync
   - CreateProblemRecordAsync
   - TrackProblemResolutionAsync
   - UpdateRootCauseAnalysisAsync

4. **Query Operations** (5 methods)
   - GetByStatusAsync
   - GetPriorityProblemsAsync
   - SearchProblemsAsync
   - GetProblemsForKCAAsync

### Dependencies
- IIncidentService
- ISLAService
- INotificationService
- ILogger<ProblemService>
- ICrmDbContext

### Database Requirements
- Problem entity
- ProblemIncident join entity
- ProblemResolution entity
- Problem workflow states

### Task List
- [ ] Re-enable ProblemService from .disabled
- [ ] Implement all 25+ methods
- [ ] Add database entities if missing
- [ ] Register in Program.cs
- [ ] Unit tests (20-25 tests)
- [ ] Integration tests with incident linkage
- [ ] Verification build

### Status: ⏳ Pending

---

## Phase 4: ITSM Change Management (50 hours)

### Service Details
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs`
- **Status:** Disabled (.disabled exists)
- **Interface:** IChangeManagementService in IITSMServices.cs
- **Methods to Implement:** 40+ methods

### Method Categories

1. **Change Request Management** (8 methods)
   - CreateChangeAsync
   - UpdateChangeAsync
   - ScheduleChangeAsync
   - CancelChangeAsync
   - GetChangeByIdAsync
   - ListChangesAsync
   - SearchChangesAsync

2. **Change Advisory Board (CAB)** (8 methods)
   - CreateCABAsync
   - ScheduleCABMeetingAsync
   - RecordCABDecisionAsync
   - UpdateCABApprovalStatusAsync
   - GetCABMembersAsync
   - AddCABMemberAsync
   - RemoveCABMemberAsync

3. **Workflow Management** (8 methods)
   - RequestChangeApprovalAsync
   - ApproveChangeAsync
   - RejectChangeAsync
   - AssignChangeImplementationAsync
   - TrackChangeProgressAsync
   - CompleteChangeAsync

4. **Impact Analysis** (8 methods)
   - AnalyzeChangeImpactAsync
   - DocumentChangeImpactAsync
   - IdentifyAffectedServicesAsync
   - GetImpactedItemsAsync
   - ValidateChangeConflictsAsync
   - CheckBlackoutPeriodsAsync

5. **Rollback Management** (5 methods)
   - CreateRollbackPlanAsync
   - ExecuteRollbackAsync
   - ValidateRollbackSuccessAsync
   - GetRollbackHistoryAsync

6. **Utilities** (3 methods)
   - GetChangeStatusAsync
   - GenerateChangeReportAsync
   - LinkRelatedChangesAsync

### Database Requirements
- Change entity
- CAB entity
- CABMember entity
- ChangeImpact entity
- ChangeImplementation entity
- RollbackPlan entity

### Task List
- [ ] Re-enable ChangeManagementService from .disabled
- [ ] Implement all 40+ methods
- [ ] Add/verify database entities
- [ ] Add CAB workflow validation
- [ ] Register in Program.cs
- [ ] Unit tests (30-35 tests)
- [ ] Integration tests with impact analysis
- [ ] E2E tests for approval workflow
- [ ] Build verification

### Status: ⏳ Pending

---

## Phase 5: Commission Rules Engine (20 hours)

### Service Details
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/CommissionRuleService.cs`
- **Status:** Re-enabled from .disabled
- **Existing:** Flat rate implementation
- **Action:** Add advanced features

### Features to Implement

1. **Tiered Commission Calculation** (3 methods)
   - GetTiersAsync(ruleId)
   - CalculateTieredCommissionAsync(amount, ruleId)
   - ApplyCommissionTierAsync(tier, amount)

2. **Commission Caps & Minimums** (2 methods)
   - ApplyCommissionCapsAsync(baseCommission, rule)
   - ValidateCommissionLimitsAsync(commission, rule)

3. **Trigger-Based Rules** (3 methods)
   - EvaluateCommissionTriggerAsync(trigger, target)
   - ExecuteTriggeredRuleAsync(ruleId, target)
   - GetApplicableTriggersAsync(dealId)

4. **Split Commission** (3 methods)
   - ProcessSplitCommissionAsync(deal, reps, percentages)
   - AllocateCommissionAsync(deal, allocation)
   - GetSplitAllocationAsync(dealId)

5. **Commission Clawback** (3 methods)
   - CreateClawbackAsync(dealId, reason, amount)
   - ProcessClawbackAsync(clawbackId)
   - ValidateClawbackAsync(commission, rule)

### Database Requirements
- CommissionRule entity enhancements
- CommissionTier entity
- CommissionTrigger entity
- CommissionAllocation entity
- CommissionClawback entity

### Task List
- [ ] Re-enable CommissionRuleService
- [ ] Implement tiered calculations (3 methods)
- [ ] Implement trigger-based rules (3 methods)
- [ ] Implement split commission (3 methods)
- [ ] Implement clawback logic (3 methods)
- [ ] Unit tests (15-18 tests)
- [ ] Integration tests with deal data
- [ ] Build verification

### Status: ⏳ Pending

---

## Phase 6: Subscription Billing Services (25 hours)

### Service 6.1: RecurringBillingEngine
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/RecurringBillingEngine.cs`
- **Status:** Disabled (.disabled exists with 398 lines)
- **Methods:** ProcessBillingCyclesAsync, BillSubscriptionAsync, CalculateBillingAmountAsync
- **Schedule:** Daily job, processes up to 1000 subscriptions per batch

### Service 6.2: DunningManager
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/DunningManager.cs`
- **Status:** Disabled (.disabled exists)
- **Methods:** RetryFailedPayment, SendDunningEmail, CancelSubscriptionOnExhaustion
- **Logic:** Exponential backoff + escalation sequence

### Service 6.3: ProrateCalculator
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/ProrateCalculator.cs`
- **Status:** Disabled (.disabled exists)
- **Algorithms:** 4 proration methods
  - ProrateCreditMethod (full month credit)
  - ProrateDaysMethod (days used basis)
  - ProrateCycleMethod (billing cycle basis)
  - ProrateIntervalMethod (interval basis)

### Service 6.4: SubscriptionMetricsAggregator
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/SubscriptionMetricsAggregator.cs`
- **Status:** Disabled (.disabled exists)
- **Methods:**
  - CalculateMRRAsync (Monthly Recurring Revenue)
  - CalculateARRAsync (Annual Recurring Revenue)
  - CalculateChurnRateAsync
  - GetCohortMetricsAsync

### Task List
- [ ] Re-enable RecurringBillingEngine from .disabled
- [ ] Implement billing cycle processor (batch processing)
- [ ] Re-enable DunningManager from .disabled
- [ ] Implement retry logic with exponential backoff
- [ ] Re-enable ProrateCalculator from .disabled
- [ ] Implement 4 proration algorithms
- [ ] Re-enable SubscriptionMetricsAggregator from .disabled
- [ ] Implement MRR, ARR, churn calculations
- [ ] Register all 4 services in Program.cs
- [ ] Unit tests (20-25 tests)
- [ ] Integration tests with subscription data
- [ ] Build verification

### Status: ⏳ Pending

---

## Phase 7: Email Sequence Service Enhancements (20 hours)

### Service Details
- **File:** `CRM.Backend/src/CRM.Infrastructure/Services/Marketing/EmailSequenceService.cs`
- **Status:** Exists but may need enhancements
- **Interface:** IEmailSequenceService 
- **Existing File:** EmailSequenceManagementService.cs also exists

### Core Methods (20 methods)

1. **CRUD Operations** (5 methods)
   - CreateSequenceAsync
   - UpdateSequenceAsync
   - DeleteSequenceAsync
   - GetSequenceAsync
   - ListSequencesAsync

2. **Execution & Triggering** (5 methods)
   - TriggerSequenceAsync
   - ExecuteSequenceStepAsync
   - EvaluateConditionAsync
   - SkipStepAsync
   - RetryStepAsync

3. **State Management** (5 methods)
   - TrackRecipientProgressAsync
   - RecordStepDeliveryAsync
   - RecordOpeningAsync
   - RecordClickAsync
   - GetRecipientProgressAsync

4. **Termination & Control** (5 methods)
   - PauseSequenceAsync
   - ResumeSequenceAsync
   - CompleteSequenceAsync
   - UnsubscribeRecipientAsync
   - BounceHandlingAsync

### Condition Engine
- EmailOpened condition
- EmailClicked condition
- TimeDelay condition
- CustomField matching
- Boolean logic (AND/OR combinations)

### Database Requirements
- EmailSequence entity
- EmailStep entity
- SequenceCondition entity
- RecipientProgress entity
- SequenceExecution entity

### Task List
- [ ] Review existing EmailSequenceService
- [ ] Implement/complete CRUD operations
- [ ] Implement execution engine
- [ ] Implement condition evaluation
- [ ] Implement state tracking
- [ ] Implement termination logic
- [ ] Register in Program.cs (verify not already registered)
- [ ] Unit tests (18-20 tests)
- [ ] Integration tests with email templates
- [ ] Build verification

### Status: ⏳ Pending

---

## Development Guidelines

### Architecture Compliance
✅ **Follow SPEC-ARCH-002:** Error Handling (throw proper exceptions)  
✅ **Follow SPEC-ARCH-003:** DI injection, use interfaces  
✅ **Follow SPEC-ARCH-004:** Cache where appropriate  
✅ **Follow SPEC-ARCH-005:** Validate inputs with FluentValidation  

### Code Quality Standards
- [ ] All services interface-driven (IServiceName in Ports/)
- [ ] Repository pattern for all data access
- [ ] Async/await for all I/O operations
- [ ] Comprehensive logging via ILogger<T>
- [ ] Unit testable (no static dependencies)
- [ ] No existing services modified - only extensions
- [ ] All new services pass unit tests
- [ ] Integration tests vs real/test database
- [ ] Full solution builds: 0 errors, 0 warnings

### Testing Requirements

#### Unit Tests
- Minimum 100+ unit tests total across all services
- Test happy path, error cases, boundary conditions
- Mock dependencies (IRepository, ILogger, etc.)
- Use xUnit framework
- Test naming: `{Method}_Should{Expectation}_When{Condition}`

#### Integration Tests  
- Test with real database context
- Test data relationships and constraints
- Test transaction handling
- Test soft delete behavior
- Minimum 50+ integration tests

### Database
- [ ] Verify all required entities in CrmDbContext
- [ ] Create EF Core configurations
- [ ] Add indexes on frequently queried columns
- [ ] Ensure soft-delete support (IsDeleted field)
- [ ] Create migrations for new entities

### DI Registration
- [ ] Add to Program.cs AddServiceServices() extension
- [ ] Register interfaces + implementations
- [ ] Use Singleton for stateless services
- [ ] Use Scoped for DbContext-dependent services

---

## Success Criteria

✅ All 7 service suites implemented with complete methods  
✅ 0 compilation errors across entire solution  
✅ 0 compilation warnings  
✅ 100+ unit tests written and passing  
✅ 50+ integration tests passing  
✅ Services follow SPEC-ARCH patterns  
✅ Database migrations created for new entities  
✅ DI registration complete in Program.cs  
✅ All API controllers calling services still compile  
✅ No regression in existing services  

---

## Progress Tracking

| Phase | Service | Status | Progress | Tests | Build |
|-------|---------|--------|----------|-------|-------|
| 1 | BusinessHoursCalculator | ⏳ | 0% | 0 | ❌ |
| 1 | IncidentService | ⏳ | 0% | 0 | ❌ |
| 1 | SLAService | ⏳ | 0% | 0 | ❌ |
| 1 | ServiceQueueService | ⏳ | 0% | 0 | ❌ |
| 2 | CommissionRuleService | ⏳ | 0% | 0 | ❌ |
| 2 | DiscountRuleService | ⏳ | 0% | 0 | ❌ |
| 2 | SLAPolicyAdminService | ⏳ | 0% | 0 | ❌ |
| 2 | EscalationRuleAdminService | ⏳ | 0% | 0 | ❌ |
| 2 | ServiceQueueService | ⏳ | 0% | 0 | ❌ |
| 3 | ProblemManagementService | ⏳ | 0% | 0 | ❌ |
| 4 | ChangeManagementService | ⏳ | 0% | 0 | ❌ |
| 5 | CommissionRuleService (Adv) | ⏳ | 0% | 0 | ❌ |
| 6 | RecurringBillingEngine | ⏳ | 0% | 0 | ❌ |
| 6 | DunningManager | ⏳ | 0% | 0 | ❌ |
| 6 | ProrateCalculator | ⏳ | 0% | 0 | ❌ |
| 6 | SubscriptionMetricsAggregator | ⏳ | 0% | 0 | ❌ |
| 7 | EmailSequenceService | ⏳ | 0% | 0 | ❌ |

---

## Notes

- All services follow the existing ServiceCollection pattern in Program.cs
- Services use ICrmDbContext for data access
- All services implement ILogger<T> for logging
- Repository pattern used for all data access
- FluentValidation used for input validation
- Unit tests use Moq for dependency injection
- Integration tests use real database context with test data seeding

---

**Last Updated:** February 16, 2026
**Next Review:** After Phase 1 completion

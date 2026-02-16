# P0/P1 Critical Services - Quick Reference Guide & Next Steps

> **Updated:** February 16, 2026  
> **Phase Status:** Phase 1-2 ✅ COMPLETE | Phase 3-7 ⏳ PENDING

---

## Summary: What Was Completed

### Services Re-enabled/Created (9 total)
✅ **BusinessHoursCalculator** - ITSM business hours calculation  
✅ **IncidentService** - ITSM incident lifecycle management  
✅ **SLAService** - ITSM SLA policy tracking and breach detection  
✅ **CommissionRuleService** - Sales commission rule definitions and calculations  
✅ **DiscountRuleService** - Sales discount rule definitions and calculations  
✅ **SLAPolicyAdminService** - Admin interface for SLA policy management  
✅ **EscalationRuleAdminService** - Admin interface for escalation rule management  
✅ **ServiceQueueService** - ITSM service queue and routing management  

### DI Registrations Updated
✅ Program.cs lines 536-542 (Phase 1)  
✅ Program.cs lines 623-627 (Phase 2)  

---

## Next Steps: Phase 3-7 (192 Hours Remaining)

### Phase 3: ITSM Problem Management (35 hours)
File: `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs`

**Action Items:**
1. Copy content from `ProblemManagementService.cs.disabled` → `ProblemManagementService.cs`
2. Verify all 25+ methods are implemented:
   - CRUD: CreateAsync, UpdateAsync, DeleteAsync, GetByIdAsync, ListAsync
   - Workflow: ResolveAsync, CloseAsync, ReopenAsync, LinkIncidentAsync
   - Analysis: AnalyzeAsync, DetermineAsync, DocumentAsync
3. Add DI registration in Program.cs (~line 545)
4. Create unit tests (20-25 tests)
5. Create integration tests

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs
```

---

### Phase 4: ITSM Change Management (50 hours)
File: `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs`

**Action Items:**
1. Copy content from `ChangeManagementService.cs.disabled` → `ChangeManagementService.cs`
2. Implement all 40+ methods across 5 categories:
   - Change Request Management (8 methods)
   - Change Advisory Board (8 methods)
   - Workflow Management (8 methods)
   - Impact Analysis (8 methods)
   - Rollback Management (5 methods)
3. Verify database entities exist: Change, CAB, ChangeImpact, ChangeImplementation, RollbackPlan
4. Add EF Core configurations for relationships
5. Add DI registration in Program.cs (~line 547)
6. Create unit tests (30-35 tests)
7. Create integration tests (E2E workflow tests)

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs
```

---

### Phase 5: Commission Rules Advanced Features (20 hours)
File: `CRM.Backend/src/CRM.Infrastructure/Services/CommissionRuleService.cs` (ENHANCE)

**Action Items:**
1. Open existing `CommissionRuleService.cs` (already enabled)
2. Add 14 new methods:
   - Tiered calculations: GetTiersAsync, CalculateTieredAsync, ApplyTierAsync
   - Commission caps: ApplyCommissionCapsAsync, ValidateLimitsAsync
   - Trigger-based: EvaluateTriggersAsync, ExecuteTriggeredAsync, GetTriggersAsync
   - Split commission: ProcessSplitAsync, AllocateAsync, GetAllocationAsync
   - Clawback: CreateClawbackAsync, ProcessClawbackAsync, ValidateAsync
3. Create CommissionTier, CommissionTrigger, CommissionAllocation, CommissionClawback entities
4. Create EF Core configurations
5. Update DI if needed (already registered)
6. Add unit tests (15-18 tests)
7. Add integration tests

---

### Phase 6: Subscription Billing Services (25 hours - 4 Services)

#### 6.1 RecurringBillingEngine
File: `CRM.Backend/src/CRM.Infrastructure/Services/RecurringBillingEngine.cs`

**Key Methods:**
- `ProcessBillingCyclesAsync()` - Batch process subscriptions (1000 at a time)
- `BillSubscriptionAsync()` - Bill single subscription
- `CalculateBillingAmountAsync()` - Calculate amount including usage

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/RecurringBillingEngine.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/RecurringBillingEngine.cs
```

#### 6.2 DunningManager
File: `CRM.Backend/src/CRM.Infrastructure/Services/DunningManager.cs`

**Key Methods:**
- `RetryFailedPayment()` - Exponential backoff retry logic
- `SendDunningEmail()` - Escalation sequence (3 emails)
- `CancelSubscriptionOnExhaustion()` - After 3 retries

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/DunningManager.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/DunningManager.cs
```

#### 6.3 ProrateCalculator
File: `CRM.Backend/src/CRM.Infrastructure/Services/ProrateCalculator.cs`

**4 Algorithms to Implement:**
- ProrateCreditMethod - Full month credit
- ProrateDaysMethod - Days used basis
- ProrateCycleMethod - Billing cycle basis
- ProrateIntervalMethod - Interval basis

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/ProrateCalculator.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/ProrateCalculator.cs
```

#### 6.4 SubscriptionMetricsAggregator
File: `CRM.Backend/src/CRM.Infrastructure/Services/SubscriptionMetricsAggregator.cs`

**Key Methods:**
- `CalculateMRRAsync()` - Monthly recurring revenue
- `CalculateARRAsync()` - Annual recurring revenue
- `CalculateChurnRateAsync()` - Churned subscription rate
- `GetCohortMetricsAsync()` - Cohort analysis

**Command:**
```bash
cp CRM.Backend/src/CRM.Infrastructure/Services/SubscriptionMetricsAggregator.cs.disabled \
   CRM.Backend/src/CRM.Infrastructure/Services/SubscriptionMetricsAggregator.cs
```

**DI Registration Block (add to Program.cs):**
```csharp
// Subscription Billing Services (Phase 6)
builder.Services.AddScoped<IRecurringBillingEngine, RecurringBillingEngine>();
builder.Services.AddScoped<IDunningManager, DunningManager>();
builder.Services.AddScoped<IProrateCalculator, ProrateCalculator>();
builder.Services.AddScoped<ISubscriptionMetricsAggregator, SubscriptionMetricsAggregator>();
builder.Services.AddHostedService<RecurringBillingHostedService>(); // Daily job
```

---

### Phase 7: Email Sequence Service Enhancements (20 hours)
File: `CRM.Backend/src/CRM.Infrastructure/Services/Marketing/EmailSequenceService.cs`

**Verify & Enhance:**
1. Check existing EmailSequenceService and EmailSequenceManagementService
2. Implement 20 total methods if missing:
   - CRUD: CreateAsync, UpdateAsync, DeleteAsync, GetAsync, ListAsync
   - Execution: TriggerAsync, ExecuteStepAsync, EvaluateConditionAsync
   - State: TrackProgressAsync, RecordDeliveryAsync, RecordOpeningAsync
   - Control: PauseAsync, ResumeAsync, CompleteAsync, UnsubscribeAsync
3. Build condition engine: EmailOpened, EmailClicked, TimeDelay, CustomField
4. Support Boolean logic: AND/OR combinations
5. Create/verify entities: EmailSequence, EmailStep, SequenceCondition, RecipientProgress
6. Add DI registration (check if already registered)
7. Create unit tests (18-20 tests)
8. Create integration tests

---

## Testing Requirements

### Unit Tests Needed
- **Phase 3:** 20-25 tests
- **Phase 4:** 30-35 tests
- **Phase 5:** 15-18 tests
- **Phase 6:** 20-25 tests (5-6 per service)
- **Phase 7:** 18-20 tests
- **TOTAL:** 120-150 unit tests

### Integration Tests Needed
- **Phase 3:** 5-10 tests (incident relationships)
- **Phase 4:** 10-15 tests (change workflow, CAB)
- **Phase 5:** 5-10 tests (commission calculations)
- **Phase 6:** 10-15 tests (billing cycles, proration)
- **Phase 7:** 5-10 tests (sequence execution)
- **TOTAL:** 40-60 integration tests

### Test Naming Convention
```csharp
[Fact]
public async Task CreateAsync_ShouldReturnDto_WhenInputIsValid()
{
    // Arrange
    var dto = new CreateServiceDto { Name = "Test" };
    
    // Act
    var result = await _service.CreateAsync(dto);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}

[Fact]
public async Task CreateAsync_ShouldThrowException_WhenNameIsEmpty()
{
    var dto = new CreateServiceDto { Name = "" };
    await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
}
```

---

## Build & Verification Commands

### Current Status
```bash
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend
dotnet build CRM.sln
# Expected: 0 errors (after Phase 1-2 complete)
```

### After Each Phase
```bash
# Build
dotnet build CRM.sln

# Run tests
dotnet test

# Test specific service
dotnet test --filter "ServiceName"
```

---

## Database Considerations

### Entities to Verify/Create
**Phase 3:**
- Problem
- ProblemIncident
- ProblemResolution

**Phase 4:**
- Change
- CAB
- CABMember
- ChangeImpact
- ChangeImplementation
- RollbackPlan
- BlackoutPeriod

**Phase 5:**
- CommissionTier
- CommissionTrigger
- CommissionAllocation
- CommissionClawback

**Phase 6:**
- Subscription (likely exists)
- BillingCycle
- DunningAttempt
- ProrationHistory

**Phase 7:**
- EmailSequence (likely exists)
- EmailStep
- SequenceCondition
- RecipientProgress

### EF Core Migrations
After adding new entities:
```bash
cd CRM.Backend/src/CRM.Api
dotnet ef migrations add Phase3_ProblemManagement
dotnet ef migrations add Phase4_ChangeManagement
# etc.

dotnet ef database update
```

---

## File Organization Reference

### Service Implementation Folders
```
CRM.Backend/src/CRM.Infrastructure/Services/
├── ITSM/                          # Phase 1-4 services
│   ├── BusinessHoursCalculator.cs     ✅
│   ├── IncidentService.cs             ✅
│   ├── SLAService.cs                  ✅
│   ├── SLAPolicyAdminService.cs       ✅
│   ├── EscalationRuleAdminService.cs  ✅
│   ├── ServiceQueueService.cs         ✅
│   ├── ProblemManagementService.cs    ⏳
│   └── ChangeManagementService.cs     ⏳
├── Sales/                         # Phase 5
│   └── CommissionRuleService.cs       ✅ (enhance)
├── Billing/                       # Phase 6
│   ├── RecurringBillingEngine.cs      ⏳
│   ├── DunningManager.cs              ⏳
│   ├── ProrateCalculator.cs           ⏳
│   └── SubscriptionMetricsAggregator.cs⏳
├── Marketing/                     # Phase 7
│   └── EmailSequenceService.cs        ✅ (enhance)
├── CommissionRuleService.cs           ✅
└── DiscountRuleService.cs             ✅
```

### Interface Folders
```
CRM.Backend/src/CRM.Core/Interfaces/
├── ITSM/
│   ├── IITSMServices.cs           (Contains all ITSM interfaces)
│   ├── IServiceQueueService.cs
│   ├── ISLAPolicyAdminService.cs
│   └── IEscalationRuleAdminService.cs
├── ICommissionRuleService.cs      ✅
├── IDiscountRuleService.cs        ✅
└── (Others as needed for Phase 6-7)
```

---

## Critical Paths & Dependencies

### Dependency Map
```
Phase 3: ProblemManagementService
  └─ DependsOn: IncidentService ✅, SLAService ✅

Phase 4: ChangeManagementService
  └─ DependsOn: SLAService ✅, NotificationService

Phase 5: CommissionRuleService (Enhanced)
  └─ Standalone (but related to Phase 6)

Phase 6: RecurringBillingEngine
  └─ DependsOn: SubscriptionService ✅, ProrateCalculator

Phase 6: DunningManager
  └─ DependsOn: PaymentService ✅, EmailService ✅

Phase 7: EmailSequenceService
  └─ DependsOn: EmailTemplateService ✅
```

---

## Estimated Timeline

| Phase | Hours | Days | Status |
|-------|-------|------|--------|
| 1-2 | 32 | 1.0 | ✅ Complete |
| 3 | 35 | 1.3 | ⏳ Next |
| 4 | 50 | 1.9 | ⏳ After 3 |
| 5 | 20 | 0.8 | ⏳ Parallel with 4 |
| 6 | 25 | 1.0 | ⏳ After 4-5 |
| 7 | 20 | 0.8 | ⏳ Final |
| **Total** | **192** | **7.8** | **In Progress** |

---

## Git Workflow

```bash
# Current branch
git branch
# feature/p0-p1-architecture-specs-2026-02-16 ✅

# Status
git status

# Commit after Phase 3
git add -A
git commit -m "feat: Implement ITSM Problem Management (Phase 3)"

# Commit after Phase 4
git commit -m "feat: Implement ITSM Change Management (Phase 4)"

# Push when ready
git push origin feature/p0-p1-architecture-specs-2026-02-16
```

---

## Success Criteria Checklist

Before moving to next phase, ensure:
- [ ] All service files created/re-enabled (no .disabled files in use)
- [ ] All services registered in Program.cs with logging
- [ ] `dotnet build` returns 0 errors, 0 warnings
- [ ] All method signatures match interface contracts
- [ ] All services have proper error handling and logging
- [ ] All services use async/await for I/O
- [ ] Unit tests passing (minimum 5-10 per service)
- [ ] Integration tests passing
- [ ] Database migrations created and applied
- [ ] Documentation updated

---

## Resources & References

📁 **Implementation Plan:** P0_P1_CRITICAL_SERVICES_IMPLEMENTATION_PLAN.md  
📁 **Phase 1-2 Completion:** docs/legacy/summary/P0_P1_PHASE1_PHASE2_COMPLETION_REPORT.md  
📁 **Specifications:** docs/11-specifications/SPEC-ITSM-*.md, SPEC-SALES-*.md  
📁 **Architecture:** docs/SOLUTION_CONTEXT.md, ARCHITECTURE_OVERVIEW.md  

---

## Contact Points

**For Questions:**
- Check specification files for business logic
- Review existing service implementations for patterns
- Check interface definitions for method contracts
- Review entity definitions in CrmDbContext

**For Blockers:**
- Verify database entities exist before implementing services
- Check interface definitions match expected methods
- Ensure required services are registered in DI container

---

**Last Updated:** February 16, 2026  
**Next Review:** After Phase 3 completion

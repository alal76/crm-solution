# PHASE 3-7 CONTINUATION GUIDE

**Session Date:** February 16, 2026  
**Status:** Token Budget Exhausted - Preparing for Next Session  
**Current Progress:** Phase 2 Complete (DTO Foundation Ready)

---

## WHERE WE LEFT OFF

### Phase 2: ✅ COMPLETE
- Deleted duplicate DTO stub files (ColorPaletteDtos.cs, ChangeDtos.cs)
- Resolved StandardEnums.cs namespace pollution (18 ambiguous reference errors)
- Fixed PaymentDto enum qualification
- **Result:** CRM.Core compiles clean (0 DTO errors), ready for Phase 3-4

### Next: Phase 3-4 ITSM Services (85 hours)
- **BLOCKED BY:** Copilot token rate limit
- **STATUS:** Ready to start (all foundation work complete)
- **DELIVERABLE:** Problem Management Service (40h) + Change Management Service (45h)

---

## IMMEDIATE NEXT STEPS FOR NEW SESSION

### Step 1: Verify Phase 2 Completion
```bash
cd '/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend'
dotnet build CRM.sln --configuration Release
# SHOULD SHOW: CRM.Core with 0 errors
# Some service errors OK (Phase 3-7 work not started)
```

### Step 2: Deploy Phase 3 Sub-Agent
Use the prompt below (prepared and ready):

**PHASE 3 SUB-AGENT PROMPT:**
```
## PHASE 3: Problem Management Service Implementation (40 hours)

[FULL PROMPT READY - see PHASE_3_SUBAGENT_PROMPT.md in repo root]

Key Methods: 25 total (CRUD, lifecycle, RCA, relationships, metrics, bulk ops)
DTOs: Problem-specific DTOs already exist in ITSM/ProblemManagementDtos.cs
Location: /CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs
Success: dotnet build → 0 errors + 20 unit tests + zero breaking changes
```

### Step 3: Deploy Phase 4 Sub-Agent (sequential to Phase 3)
Once Phase 3 completes:
- Change Management Service (45 hours, 40 methods)
- CAB workflow, impact analysis, rollback planning
- Location: /CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs

---

## CRITICAL FILES TO KNOW

### Phase 2 Completion Reports
- `/docs/legacy/summary/PHASE_2_COMPLETE.md` - What was accomplished, what issues were fixed
- `/docs/legacy/summary/PHASE_2_EXECUTION_REPORT.md` - Detailed technical analysis
- `/docs/legacy/summary/PHASE_2_DTO_CONSOLIDATION_PLAN.md` - Consolidation strategy

### Key Architecture Documents
- `/docs/11-11-11-specifications/SPEC-ARCH-001.md` - DTO standardization (MUST READ)
- `/docs/11-11-11-specifications/SPEC-ARCH-002.md` - Error handling
- `/docs/11-11-11-specifications/SPEC-ARCH-003.md` - Dependency injection
- `/docs/11-11-11-specifications/SPEC-ARCH-004.md` - Caching
- `/docs/11-11-11-specifications/SPEC-ARCH-005.md` - Validation

### Solution Status
- `/docs/legacy/summary/VICTORY_DECLARATION_P0P1_PHASE1.md` - Phase 1 achievement summary 
- `/SOLUTION_GAPS_REMEDIATION_PLAN.md` - Master remediation tracking
- Root `.github/copilot-instructions.md` - CRM architecture guide

---

## PHASE 3 PREREQUISITES (VERIFIED ✅)

### Infrastructure Ready
- ✅ BaseDtoInterfaces.cs (base classes defined: ReadResponseDtoBase, CreateRequestDtoBase, etc.)
- ✅ ITSMDtos.cs (consolidated, no duplication)
- ✅ ProblemManagementDtos.cs (20 DTOs available)
- ✅ CRM.Core.Dtos namespace clean (StandardEnums.cs.backup disabled)
- ✅ No ambiguous references

### Entity Layer
- ✅ Problem entity exists (CRM.Core.Entities.ITSM.Problem)
- ✅ Incident entity exists (CRM.Core.Entities.ITSM.Incident)
- ✅ Problem-Incident relationship configured in DbContext
- ✅ Entity enums available (ProblemStatus, ProblemPriority, etc.)

### Service Layer Patterns
- ✅ ILogger<T> for logging (use Microsoft.Extensions.Logging)
- ✅ ICrmDbContext for data access (soft-delete pattern established)
- ✅ IEmailServicePort / INotificationPort for notifications
- ✅ ICacheService for caching (3-layer pattern defined in SPEC-ARCH-004)
- ✅ Error handling with ApiResponse<T> (defined in BaseDtoInterfaces.cs)

### Database Ready
- ✅ Migrations already applied (problem management 100% complete)
- ✅ All problem/incident tables created
- ✅ Indexes configured for performance
- ✅ Foreign key relationships established

---

## PHASE 3 IMPLEMENTATION CHECKLIST

When implementing Problem Management Service, ensure:

- [ ] **Service Class Created:**
  - Location: `/CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ProblemManagementService.cs`
  - Implements: `IProblemManagementService`
  - Dependencies injected: ILogger<T>, ICrmDbContext, INotificationPort, ICacheService

- [ ] **CRUD Operations (5 methods):**
  - GetByIdAsync - with soft-delete check
  - GetAllAsync - with filtering & pagination
  - CreateAsync - with validation
  - UpdateAsync - with optimistic locking (RowVersion)
  - DeleteAsync - soft delete (IsDeleted = true)

- [ ] **Problem Lifecycle (5 methods):**
  - AssignAsync - assign to investigator
  - InvestigateAsync - record findings
  - ResolveAsync - mark resolved
  - CloseAsync - complete problem
  - ReopenAsync - reopen if needed

- [ ] **Root Cause Analysis (2 methods):**
  - PerformRCAAsync - analyze patterns
  - GetRCAAsync - retrieve RCA

- [ ] **Relationships (4 methods):**
  - LinkIncidentsAsync - create incident-problem links
  - UnlinkIncidentsAsync - remove links
  - GetLinkedIncidentsAsync - retrieve linked incidents
  - GetLinkedIncidentCountAsync - count

- [ ] **Metrics (4 methods):**
  - GetMetricsAsync - MTTR, MTBF, resolution rates
  - GetOpenProblemsCountAsync - summary
  - GetProblemsNeedingRCAAsync - incomplete RCAs
  - GetTrendsAsync - trend analysis

- [ ] **Bulk & Notifications (5 methods):**
  - BulkAssignAsync - assign multiple
  - BulkCloseAsync - close multiple
  - NotifyStakeholdersAsync - send notifications
  - EscalateAsync - escalate high-priority
  - (One more for balance)

- [ ] **Error Handling:**
  - ArgumentNullException for null inputs
  - InvalidOperationException for invalid state transitions
  - EntityNotFoundException for missing entities
  - ApiResponse<T> wrapper for all returns

- [ ] **Unit Tests (20+ tests):**
  - CRUD tests (5)
  - Lifecycle tests (5)
  - RCA tests (3)
  - Relationship tests (3)
  - Metrics tests (2)
  - Error handling (2)
  - Integration tests (5)

- [ ] **Build Verification:**
  - `dotnet build CRM.sln --configuration Release` → **0 errors**
  - No regression in existing tests

- [ ] **DI Registration:**
  - Added to Program.cs: `.AddScoped<IProblemManagementService, ProblemManagementService>()`

---

## PHASE 3 DETAILED SPECIFICATION

### Method 1-5: CRUD Operations

```csharp
// GetByIdAsync(int problemId, CancellationToken)
// Returns: ProblemDto with related incidents
// Errors: EntityNotFoundException if not found or deleted

// GetAllAsync(ProblemFilterDto, CancellationToken) 
// Returns: PagedResultDto<ProblemListDto>
// Filters: Status, Priority, Investigator, DateRange, SearchText
// Default Pagination: page=1, pageSize=20

// CreateAsync(CreateProblemDto, CancellationToken)
// Validates: Title required, max 255 chars; Description optional
// Defaults: Status=Open, CreatedAt=UtcNow, AssignedToId=null initially
// Returns: Created ProblemDto with full audit trail

// UpdateAsync(int, UpdateProblemDto, CancellationToken)
// All fields optional (partial update support)
// RowVersion required (optimistic locking)
// Throws: ConcurrencyException if RowVersion mismatch

// DeleteAsync(int problemId, CancellationToken)
// Soft delete: Sets IsDeleted=true
// Returns: true if successful, false if already deleted
```

### Method 6-10: Problem Lifecycle

```csharp
// AssignAsync(int problemId, int investigatorUserId, CancellationToken)
// Validates: User exists in system
// Updates: AssignedToId, AssignedDate=UtcNow, Status stays same
// Notifies: Investigator that problem assigned

// InvestigateAsync(int problemId, ProblemInvestigationDto, CancellationToken)
// Input: Findings, Workaround, ExecutionNotes
// Status: Remains Open but records investigation progress
// Records: Investigation history for audit trail

// ResolveAsync(int problemId, ResolveProblemDto, CancellationToken)
// Validates: Root cause must be identified (optional implementation plan)
// Status: Open → Resolved (must be investigated first)
// Notifies: Stakeholders of resolution
// Links: Linked incidents marked as "resolved via problem X"

// CloseAsync(int problemId, CloseProblemDto, CancellationToken)
// Validates: Resolution accepted, no new related incidents
// Status: Resolved → Closed
// Clean up: Remove temporary data, finalize metrics
// Returns: Problem with final status

// ReopenAsync(int problemId, string reason, CancellationToken)
// Validates: Problem must be Closed or Resolved
// Status: Back to Open
// Records: Reason for reopening in history
```

### Method 11-12: Root Cause Analysis

```csharp
// PerformRCAAsync(int problemId, RCADataDto, CancellationToken)
// Input: RCADataDto contains symptoms, environment, timeline
// Analysis: Implement 5-Why method or fishbone diagram
// Output: ProblemRootCauseAnalysisDto with root cause + 3+ recommendations
// Calculates: MTTR, MTBF based on problem history

// GetRCAAsync(int problemId, CancellationToken)
// Retrieves: Existing RCA for problem (may be null if not started)
// Returns: ProblemRootCauseAnalysisDto or null
```

### Method 13-16: Relationships

```csharp
// LinkIncidentsAsync(int problemId, int[] incidentIds, CancellationToken)
// Creates: Problem-Incident links (may resolve multiple incidents)
// Validates: All incidents exist
// Returns: List of LinkProblemToIncidentDto with link metadata

// UnlinkIncidentsAsync(int problemId, int[] incidentIds, CancellationToken)
// Removes: Links between problem and incidents
// Validates: Cannot unlink if incident status depends on it
// Returns: Array of unlinked incident IDs

// GetLinkedIncidentsAsync(int problemId, CancellationToken)
// Retrieves: All IncidentDto objects linked to problem
// Ordering: By incident creation date, most recent first
// Returns: List<IncidentDto>

// GetLinkedIncidentCountAsync(int problemId, CancellationToken)
// Returns: int count of linked incidents
// Caching: Cache this metric for 1 hour
```

### Method 17-20: Metrics & Analytics

```csharp
// GetMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken)
// Calculates:
//   - TotalProblems: Count in date range
//   - ResolvedProblems: Count resolved
//   - OpenProblems: Count still open
//   - AverageMTTR: Seconds from open to resolved
//   - AverageMTBF: Seconds between problem recurrences
//   - ResolutionRate: % resolved / total
// Returns: ProblemMetricsDto

// GetOpenProblemsCountAsync(CancellationToken)
// Returns: int count of all open problems
// Caching: Cache for 5 minutes

// GetProblemsNeedingRCAAsync(CancellationToken)
// Returns: int count of problems in Resolved status without RCA
// These: Ready to be moved to Closed after RCA

// GetTrendsAsync(int days = 30, CancellationToken)
// Returns: List<ProblemTrendDto> with daily/weekly trend data
// Shows: How problem volume trending (up/down/stable)
// Caching: Cache for 1 day
```

### Method 21-25: Bulk Operations & Notifications

```csharp
// BulkAssignAsync(int[] problemIds, int investigatorUserId, CancellationToken)
// Assigns: All problems to same investigator
// Validates: User exists; problems are assignable
// Returns: BulkOperationResultDto with success/fail counts

// BulkCloseAsync(int[] problemIds, string reason, CancellationToken)
// Closes: Multiple problems with same reason
// Validates: All must be Resolved (not Open)
// Returns: BulkOperationResultDto with success/fail counts
// Notifies: All investigators of bulk closure

// NotifyStakeholdersAsync(int problemId, string message, CancellationToken)
// Sends: Notification message to:
//   - Problem investigator
//   - All linked incident assignees
//   - Problem reporter
// Returns: bool success

// EscalateAsync(int problemId, string reason, CancellationToken)
// Escalates: High-priority problems to management
// Creates: Escalation record and notification
// Returns: bool success or throws if already escalated

// [5th method - suggest additional operation like: ApproveRCAAsync or ScheduleImplementationAsync]
```

---

## BUILD & TEST VERIFICATION

### Required Build Success
```bash
cd '/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend'
dotnet build CRM.sln --configuration Release
# Expected: 0 errors (CRM.Core, CRM.Infrastructure all compile)
```

### Unit Test Framework
Use xUnit with Moq:
```csharp
public class ProblemManagementServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ProblemManagementService>> _mockLogger;
    private readonly ProblemManagementService _service;
    
    public ProblemManagementServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ProblemManagementService>>();
        _service = new ProblemManagementService(_mockDbContext.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnProblem_WhenProblemExists()
    {
        // Arrange
        var problemId = 1;
        var problem = new Problem { Id = problemId, Title = "Test Problem", Status = ProblemStatus.Open };
        _mockDbContext.Setup(x => x.Problems.FindAsync(problemId, default)).ReturnsAsync(problem);
        
        // Act
        var result = await _service.GetByIdAsync(problemId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Problem", result.Title);
    }
}
```

---

## REGRESSION PREVENTION

**Before Declaring Phase 3 Complete:**

1. Run full test suite:
   ```bash
   cd CRM.Backend && dotnet test --configuration Release
   ```
   Must show: All existing tests pass (baseline 5,300+ tests)

2. Verify zero breaking changes:
   - No changes to IProblemManagementService signatures
   - No changes to ProblemDto structure
   - No changes to existing service interfaces

3. Git verification:
   - Only NEW files and MODIFIED service/test files
   - No deletion of existing production code
   - Clear commit message: "feat(itsm): implement ProblemManagementService (Phase 3)"

---

## PHASE 4 PREPARATION

Once Phase 3 complete:

**Phase 4: Change Management Service (45 hours)**
- Similar structure to Phase 3 but more complex
- 40+ methods (CRUD + CAB workflows + impact analysis + rollback)
- Location: `/CRM.Backend/src/CRM.Infrastructure/Services/ITSM/ChangeManagementService.cs`
- DTOs: ChangeManagementDtos.cs already created (CABDto, ChangeApprovalDto, etc.)
- Tests: 30+ unit tests

---

## KNOW YOUR ARCHITECTURE

### Service Injection Pattern (REQUIRED for Phase 3)
```csharp
// In Program.cs
services.AddScoped<IProblemManagementService, ProblemManagementService>();
services.AddScoped<CrmDbContext>();
services.AddLogging();
services.AddScoped(typeof(ICacheService), typeof(DistributedCacheService));
```

### Data Access Pattern (SOFT DELETE)
```csharp
// Always filter deleted records
var problems = await _dbContext.Problems
    .Where(p => !p.IsDeleted)
    .AsNoTracking()
    .ToListAsync();
```

### Error Handling Pattern (REQUIRED)
```csharp
// Never throw generic exceptions
if (problem == null || problem.IsDeleted)
    throw new EntityNotFoundException("Problem not found", $"Problem ID {id}");

// Always return standardized response
return ApiResponse.SuccessResponse(dto, "Problem created successfully");
```

---

## TOKENS & RESOURCES

**Next Session Requirements:**
- Budget: ~100k tokens for Phase 3 (40h service implementation)
- Plus: ~20k tokens for Phase 4 prep
- Total: ~150k tokens for Phase 3-4 combined

**Parallel Execution Ready:**
- Database migrations staged (Phase 3-4 don't need DB changes)
- Frontend components can proceed in parallel (Phase 5)
- API controllers can start after services stabilize (Phase 6)

---

## SUCCESS DEFINITION FOR PHASE 3

✅ **Deliverables:**
1. ProblemManagementService.cs (700+ lines, 25 methods)
2. 20+ unit tests (95%+ code coverage)
3. Zero breaking changes
4. `dotnet build` → 0 errors
5. All tests pass (existing + new)

✅ **Ready for Phase 4** (ChangeManagementService)

---

**Prepared by:** GitHub Copilot  
**Date:** February 16, 2026  
**Status:** READY FOR NEXT SESSION  
**Next Action:** Deploy Phase 3 sub-agent with Problem Management Service prompt


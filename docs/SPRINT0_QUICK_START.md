# SPRINT 0 QUICK START GUIDE
## Emergency Response & Build Fix (Day 1 - Week 1)

> **For:** Immediate action  
> **Duration:** 1 week  
> **Team:** 2-3 Backend developers

---

## DAY 1: EMERGENCY RESPONSE

### Morning (Standup + Planning)

**8:00 AM - Standup**
- Review: We have 188 build errors, tests blocked, no Go/No-Go yet
- Plan: Fix build, restore tests, prepare weekly plan
- Decision: All other work paused until build green

**8:30 AM - Build Error Audit**
```bash
# Get the full error list
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend
dotnet build 2>&1 | grep "error " | wc -l

# Copy error detail to file
dotnet build 2>&1 > build-errors.txt

# Categorize errors
grep "error " build-errors.txt | sort | uniq -c | sort -rn
```

**Expected output categories:**
1. Missing DTOs (likely 40-50 errors)
2. Missing using statements (likely 20-30 errors)
3. Type ambiguities (likely 5-10 errors)
4. Method not found (likely 30-50 errors)

**9:00 AM - Error Categorization**
- [ ] Create SPRINT0_BUILD_ERRORS.md documenting all 188
- [ ] Group by file and error type
- [ ] Prioritize by severity (blocking vs warnings)
- [ ] Assign tasks to developers

### Afternoon (Build Fix Sprint)

**1:00 PM - Parallel Fix Stream**

**Dev 1: DTOs & Types**
- [ ] Identify missing DTO files
- [ ] Create DTO templates based on entity definitions
- [ ] Add to appropriate namespaces
- [ ] Wire into services

**Dev 2: Using Statements & Namespaces**
- [ ] Fix import statements
- [ ] Add missing using declarations
- [ ] Resolve namespace ambiguities
- [ ] Verify IntelliSense happy

**Dev 3: Method Implementations**
- [ ] Find AdminConfigurationService (46 missing methods)
- [ ] Create method stubs
- [ ] Add to interfaces
- [ ] Don't need full implementation—just compile-able

**5:00 PM - First Build Attempt**
```bash
dotnet build

# If still errors, fix errors < 20
# If errors > 100, escalate (may need more info)
```

**5:30 PM - Daily Summary**
- Document progress
- Update error count
- Plan next day (continue or escalate)

---

## DAY 2-3: SYSTEMATIC FIX

### Prioritized Fix Order

**Priority 1: Quick Wins** (Est: 2-3 hours)
- [ ] Add missing `using` statements (auto-importable)
- [ ] Fix type ambiguities (rename or fully qualify)
- [ ] Create missing DTOs in right namespace

**Commands:**
```bash
# Find all "CS0103: name does not exist" errors
dotnet build 2>&1 | grep "CS0103"

# Find all "missing namespace/using" errors
dotnet build 2>&1 | grep "using"

# Try auto-fix (Roslyn analyzers)
dotnet build --no-incremental
```

**Priority 2: Structure Fixes** (Est: 4-6 hours)
- [ ] AdminConfigurationService method stubs
- [ ] CrmDbContext ambiguities
- [ ] Service registration issues

**Priority 3: Validation** (Est: 2-3 hours)
- [ ] Full build test
- [ ] Warning cleanup
- [ ] Documentation

### Example: Fix AdminConfigurationService

**Error:** 46 missing methods  
**Fix Pattern:**

```csharp
// File: CRM.Infrastructure/Services/AdminConfigurationService.cs

public class AdminConfigurationService : IAdminConfigurationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<AdminConfigurationService> _logger;
    
    public AdminConfigurationService(
        ICrmDbContext dbContext,
        ILogger<AdminConfigurationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    // Missing method stubs - add like this:
    
    public async Task<BrandingSettingsDto> GetBrandingSettingsAsync(CancellationToken ct = default)
    {
        try
        {
            // TODO: Implement full logic later
            return new BrandingSettingsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branding settings");
            throw;
        }
    }
    
    public async Task UpdateBrandingSettingsAsync(BrandingSettingsDto dto, CancellationToken ct = default)
    {
        // TODO: Implement
    }
    
    // Repeat for all 46 methods...
}
```

---

## DAY 4-5: TEST EXECUTION

### Step 1: Verify Clean Build

```bash
cd CRM.Backend
dotnet clean
dotnet build

# Should output: Build succeeded
# Error count should be: 0
```

### Step 2: Run Test Suite

```bash
# Run all tests
dotnet test

# Run System module tests only (the critical ones)
dotnet test --filter "Category=SystemModule"

# Run with output
dotnet test -v normal
```

**Expected Result:**
- Most tests pass (some may have pre-existing failures)
- No NEW errors from build fixes
- Test count > 1000
- Duration < 5 minutes for quick test
- Full suite < 15 minutes

### Step 3: Document Test Status

Create: `SPRINT0_TEST_STATUS.md`

```markdown
# Test Execution Status

Build Status: ✅ PASS (0 errors)
Test Count: 1087 tests
Test Result: ✅ PASS (98% passing)
Duration: 245 seconds
Coverage: (calculated in Sprint 8)

Failing Tests:
- [List any pre-existing failures]

Next Action: Proceed to Sprint 1
```

---

## DAY 5-7: PLAN & PREPARE

### Task 1: Create Sprint Execution Blueprint

**Jira Setup:**
- [ ] Create Epic: "Sprint 1: Foundation Layer"
- [ ] Create 8 Sprints (backlog items grouped)
- [ ] Assign P0/P1 items to Sprint 1
- [ ] Set story points (1 point = 3 hours)

**Sprints to Create:**
```
Sprint 0: Build Fix (THIS WEEK)
Sprint 1: Database & Core Services (Week 1-2 of project)
Sprint 2: Backend Completeness (Week 3-4)
Sprint 3: Frontend Foundation (Week 5-6)
Sprint 4: ITSM Tier-2 (Week 7-8)
Sprint 5: Campaign Module (Week 9-10)
Sprint 6: Integration (Week 11-12)
Sprint 7: Polish (Week 13-14)
Sprint 8: Production Ready (Week 15+)
```

### Task 2: Database Migration Prep

**Files to Prepare:**
- [ ] Create `migrations/001_AddITSMTables.sql`
- [ ] Create `migrations/002_AddWebhookTables.sql`
- [ ] Create `migrations/003_UpdateSLATracking.sql`
- [ ] Create `migrations/004_AddIntegrationTables.sql`

**Use template:**
```sql
-- Migration: AddProblems
-- Purpose: Create Problem entity and supporting tables
-- Rollback: DROP TABLE Problems;

BEGIN TRANSACTION;

CREATE TABLE Problems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ProblemNumber VARCHAR(50) NOT NULL UNIQUE,
    Title VARCHAR(500) NOT NULL,
    Description LONGTEXT,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Severity INT NOT NULL DEFAULT 3,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,
    IsDeleted BIT DEFAULT 0,
    RowVersion TIMESTAMP,
    INDEX IX_Problems_Status (Status),
    INDEX IX_Problems_CreatedAt (CreatedAt DESC)
);

-- Add foreign keys after dependent tables
-- Add seed data last

COMMIT;
```

### Task 3: Team Training

**Create Training Materials:**
- [ ] Hexagonal architecture overview (30 min video/doc)
- [ ] Codebase walkthrough (60 min)
- [ ] Development environment setup guide
- [ ] Git workflow & code review guidelines
- [ ] Testing standards & examples

**Schedule:**
- Wednesday: Architecture training
- Thursday: Codebase walkthrough
- Friday: Development workflow

### Task 4: Communication Plan

**Daily:**
- [ ] 10 AM Standup (15 min) - Progress, blockers
- [ ] Slack #crm-dev channel for updates

**Weekly:**
- [ ] Sprint planning (Mondays, 1 hour)
- [ ] Sprint review (Fridays, 1 hour)
- [ ] Architecture review (Fridays, 30 min)

**Stakeholder Briefings:**
- [ ] Weekly exec update (status, blockers, forecast)
- [ ] Bi-weekly feature demo (what shipped)
- [ ] Monthly retrospective (process improvement)

---

## SUCCESS CRITERIA (Week 1)

### Build Status
- [ ] 0 compilation errors
- [ ] 0 build warnings
- [ ] Build time < 2 minutes

### Test Status
- [ ] Tests executable (no startup failures)
- [ ] 95%+ tests passing (pre-existing failures documented)
- [ ] Test plan documented (Sprint1+)

### Planning Status
- [ ] All 8 sprints planned in Jira
- [ ] All 327 gaps assigned to sprints
- [ ] Story points estimated (±20%)
- [ ] Team trained on architecture & patterns

### Go-Live Criteria Met
- [ ] Executive approval for 15-week timeline
- [ ] Team agrees plan is achievable
- [ ] Resources committed
- [ ] Sprint 1 starts Monday

---

## SPRINT 0 CHECKLIST

### Day 1 (Today)
- [ ] Hold kick-off meeting
- [ ] Review 188 build errors
- [ ] Assign developer tasks
- [ ] Categorize errors

### Day 2-3
- [ ] Fix Priority 1 errors (using statements)
- [ ] Fix Priority 2 errors (methods/DTOs)
- [ ] Attempt build fix

### Day 4-5
- [ ] Clean build passes
- [ ] Tests execute successfully
- [ ] Document test status

### Day 5-7
- [ ] Create Sprint backlog (Jira)
- [ ] Database migrations prepared
- [ ] Team trained
- [ ] Communication plan active

### Week 1 Exit Criteria
- [ ] Build: GREEN ✅
- [ ] Tests: EXECUTABLE ✅
- [ ] Plan: APPROVED ✅
- [ ] Sprint 1: READY ✅

---

## ESCALATION PATH

**Build still failing after Day 3:**
1. **Option A:** Rebuild individual projects
   ```bash
   # Rebuild just CRM.Infrastructure
   dotnet build CRM.Backend/src/CRM.Infrastructure/CRM.Infrastructure.csproj
   ```

2. **Option B:** Clean slate
   ```bash
   dotnet clean
   rm -rf ~/.nuget/packages/crm* (if local packages)
   dotnet build
   ```

3. **Option C:** Emergency remediation
   - Identify root cause error
   - Create isolated fix PR
   - Escalate to architecture team
   - May need manual code review

**Tests won't execute after build green:**
- Check .NET version: `dotnet --version` (need 8.0+)
- Check test runner: `dotnet test --list-tests`
- Run specific test file: `dotnet test path/to/test.csproj`
- Check test framework: `Microsoft.NET.Test.Sdk` version in .csproj

**Performance issues during build:**
- Add `--no-restore` flag (faster)
- Add `--no-incremental` only when needed
- Run on local machine (avoid network drives)
- Close other apps (VS Code, etc.)

---

## RESOURCES & REFERENCES

- [UNIFIED_REMEDIATION_PLAN.md](./UNIFIED_REMEDIATION_PLAN.md) — Full 8-sprint plan
- [EXECUTIVE_BRIEFING_REMEDIATION.md](./EXECUTIVE_BRIEFING_REMEDIATION.md) — For stakeholders
- [Build Error Tracking](./SPRINT0_BUILD_ERRORS.md) — To be created Day 1
- Copilot Instructions: `/Users/alal/Code/Git CRM Solution/crm-solution/.github/copilot-instructions.md`

---

**Report Date:** February 15, 2026  
**Status:** Ready for immediate execution  
**Next Update:** Daily during Sprint 0

# ITSM Services: Quick Fix Reference Guide

**Purpose:** Line-by-line fixes for re-enabling ITSM services  
**Last Updated:** February 15, 2026  
**Time Estimate:** 2-3 hours to apply all fixes

---

## Fix #1: IDbContextResolver Pattern Refactor

### Services Affected (9 total)
- BusinessHoursCalculator.cs.disabled
- IncidentService.cs.disabled  
- ProblemService.cs.disabled
- CMDBService.cs.disabled
- ChangeManagementService.cs.disabled
- KnowledgeManagementService.cs.disabled
- ServiceCatalogService.cs.disabled
- SLAService.cs.disabled
- AutoCloseHostedService.cs.disabled

### Template: Before → After

**BEFORE (Current - WRONG):**
```csharp
public class IncidentService : IIncidentService
{
    private readonly IDbContextResolver _dbContextResolver;
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(
        IDbContextResolver dbContextResolver,
        ILogger<IncidentService> logger)
    {
        _dbContextResolver = dbContextResolver;
        _logger = logger;
    }

    public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, int createdById)
    {
        var context = _dbContextResolver.ResolveContext();  // ❌ REMOVE THIS LINE
        
        var incident = new Incident { ... };
        context.Incidents.Add(incident);
        await context.SaveChangesAsync();
        // ...
    }
}
```

**AFTER (Correct):**
```csharp
public class IncidentService : IIncidentService
{
    private readonly ICrmDbContext _context;  // ✅ CHANGE THIS
    private readonly ILogger<IncidentService> _logger;

    public IncidentService(
        ICrmDbContext context,  // ✅ CHANGE THIS
        ILogger<IncidentService> logger)
    {
        _context = context;  // ✅ CHANGE THIS
        _logger = logger;
    }

    public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto, int createdById)
    {
        // ✅ DELETE: var context = _dbContextResolver.ResolveContext();
        
        var incident = new Incident { ... };
        _context.Incidents.Add(incident);  // ✅ CHANGE THIS
        await _context.SaveChangesAsync();  // ✅ CHANGE THIS
        // ...
    }
}
```

### Global Replace Pattern
For each affected file, perform these replacements:

**Find & Replace #1:**
```
Find:    private readonly IDbContextResolver _dbContextResolver;
Replace: private readonly ICrmDbContext _context;
```

**Find & Replace #2:**
```
Find:    IDbContextResolver dbContextResolver,
Replace: ICrmDbContext context,
```

**Find & Replace #3:**
```
Find:    _dbContextResolver = dbContextResolver;
Replace: _context = context;
```

**Find & Replace #4:**
```
Find:    var context = _dbContextResolver.ResolveContext();
Replace: // Removed - use _context directly
```

**Find & Replace #5:**
```
Find:    context.Incidents
Replace: _context.Incidents
```
(Repeat for all DbSets: Problems, Changes, CatalogItems, etc.)

**Find & Replace #6:**
```
Find:    context.SaveChangesAsync()
Replace: _context.SaveChangesAsync()
```

---

## Fix #2: DTO Namespace Typo

### Services Affected (2 total)
- EscalationRuleAdminService.cs.disabled
- EscalationPolicyService.cs.disabled

### Simple Find & Replace

**Find:**
```csharp
using CRM.Core.Dtos.ITSM;
```

**Replace:**
```csharp
using CRM.Core.DTOs.ITSM;
```

### Verification
After replacement, these types should be found:
- `EscalationRuleDto` ✅
- `CreateEscalationRuleDto` ✅
- `UpdateEscalationRuleDto` ✅
- `EscalationPolicyDto` ✅

---

## Fix #3: Extract Inline Interfaces

### Services Affected (8 total)

### Example: BusinessHoursCalculator.cs

#### Step 1: Create New File
**New File:** `Core/Interfaces/ITSM/IBusinessHoursCalculator.cs`

```csharp
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// [License headers...]

using System;
using System.Threading.Tasks;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Interface for business hours calculations used in SLA management.
/// Supports multiple time zones, holidays, and custom schedules.
/// </summary>
public interface IBusinessHoursCalculator
{
    /// <summary>
    /// Calculate the due date by adding business minutes to a start time.
    /// </summary>
    Task<DateTime> AddBusinessMinutesAsync(DateTime startTime, int businessMinutes, int? scheduleId = null);

    /// <summary>
    /// Calculate elapsed business minutes between two dates.
    /// </summary>
    Task<int> GetElapsedBusinessMinutesAsync(DateTime startTime, DateTime endTime, int? scheduleId = null);

    /// <summary>
    /// Check if a given time is within business hours.
    /// </summary>
    Task<bool> IsBusinessTimeAsync(DateTime dateTime, int? scheduleId = null);

    /// <summary>
    /// Get the next business day start time from a given date.
    /// </summary>
    Task<DateTime> GetNextBusinessStartAsync(DateTime fromDate, int? scheduleId = null);

    /// <summary>
    /// Check if a date is a holiday.
    /// </summary>
    Task<bool> IsHolidayAsync(DateTime date, int? scheduleId = null);
}
```

#### Step 2: Update Service File
**File:** `Infrastructure/Services/ITSM/BusinessHoursCalculator.cs.disabled`

**Remove from file:**
```csharp
// DELETE THIS ENTIRE BLOCK:
public interface IBusinessHoursCalculator
{
    // ... interface definition
}
```

**Keep in file:**
```csharp
public class BusinessDay
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
}

public class Holiday
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecurringYearly { get; set; }
}

public class BusinessSchedule
{
    public int ScheduleId { get; set; }
    public string Name { get; set; } = "Default";
    // ... rest of class
}
```

**Important:** Move supporting classes to a shared DTO or Models file if needed, or leave as nested types if used only in this service.

#### Step 3: Update Using Statements
**Add to file:**
```csharp
using CRM.Core.Interfaces.ITSM;
```

---

### Other Interfaces to Extract (Similar Process)

#### ApplicableInterface: IAssignmentRulesEngine
**From:** AssignmentRulesEngine.cs.disabled (lines ~50-150)  
**To:** `Core/Interfaces/ITSM/IAssignmentRulesEngine.cs`

**Supporting Types to Keep:**
```csharp
public class AssignmentResult { ... }
public class RuleEvaluation { ... }
public class AssignmentRule { ... }
public class RuleCondition { ... }
public enum ConditionOperator { ... }
public enum LogicalOperator { ... }
```
Keep these in AssignmentRulesEngine.cs or move to shared file.

#### Application Interface: IArticleRecommendationService
**From:** ArticleRecommendationService.cs.disabled (lines ~21-80)  
**To:** `Core/Interfaces/ITSM/IArticleRecommendationService.cs`

**Supporting Types:**
```csharp
public class ArticleRecommendation { ... }
public enum ArticleFeedbackType { ... }
public class TrendingArticle { ... }
public class RecommendationStats { ... }
```

#### And so on for remaining 5 interfaces...
(IImpactAnalysisService, IDiscoveryService, ICatalogApprovalService, ICatalogFulfillmentService, ICABWorkflowService)

---

## Fix #4: Add Missing DTOs to ITSMDtos.cs

### File Location
`Core/DTOs/ITSM/ITSMDtos.cs`

### Code to Add (at end of file, before final closing brace)

```csharp
// ============================================================================
// Filter DTOs (Add after existing DTOs)
// ============================================================================

public class IncidentFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public IncidentState? State { get; set; }
    public int? Priority { get; set; }
    public int? AssignedToId { get; set; }
    public int? AssignmentGroupId { get; set; }
    public bool? SLABreached { get; set; }
    public bool? MajorIncident { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public class ProblemFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ProblemState? State { get; set; }
    public ProblemPriority? Priority { get; set; }
    public bool? KnownError { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public class ChangeFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ChangeState? State { get; set; }
    public ChangeType? Type { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public DateTime? PlannedStartFrom { get; set; }
    public DateTime? PlannedStartTo { get; set; }
}

public class EscalationRuleFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
}

// ============================================================================
// Escalation Rule DTOs
// ============================================================================

public class CreateEscalationRuleDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string Priority { get; set; } = string.Empty;

    public string? Category { get; set; }
    public string? Queue { get; set; }

    [Range(1, int.MaxValue)]
    public int AgeInMinutes { get; set; } = 60;

    [Required]
    public string TargetType { get; set; } = string.Empty;

    public int? TargetId { get; set; }
    public string? TargetName { get; set; }

    [Range(1, 10)]
    public int MaxAttempts { get; set; } = 3;

    [Range(1, 1440)]
    public int RetryIntervalMinutes { get; set; } = 15;

    public bool IsActive { get; set; } = true;
}

public class UpdateEscalationRuleDto
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Queue { get; set; }

    [Range(1, int.MaxValue)]
    public int? AgeInMinutes { get; set; }

    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }

    [Range(1, 10)]
    public int? MaxAttempts { get; set; }

    [Range(1, 1440)]
    public int? RetryIntervalMinutes { get; set; }

    public bool? IsActive { get; set; }
}
```

---

## Fix #5: DI Registration

### File to Modify
`CRM.Backend/src/CRM.Api/Program.cs` or  
`CRM.Backend/src/CRM.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

### Add These Registrations

```csharp
// Add after existing service registrations

// ITSM Services - Phase 1
services.AddScoped<IBusinessHoursCalculator, BusinessHoursCalculator>();
services.AddScoped<IEscalationRuleService, EscalationRuleAdminService>();

// ITSM Services - Phase 2 (Core Services)
services.AddScoped<IIncidentService, IncidentService>();
services.AddScoped<IProblemService, ProblemService>();
services.AddScoped<ICMDBService, CMDBService>();
services.AddScoped<IChangeManagementService, ChangeManagementService>();
services.AddScoped<IKnowledgeManagementService, KnowledgeManagementService>();
services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
services.AddScoped<ISLAService, SLAService>();

// ITSM Services - Admin
services.AddScoped<IEscalationRuleAdminService, EscalationRuleAdminService>();
services.AddScoped<IEscalationPolicyService, EscalationPolicyService>();
services.AddScoped<ISLAPolicyAdminService, SLAPolicyAdminService>();

// ITSM Services - Hosted (Background Tasks)
services.AddHostedService<SLAEnforcementHostedService>();
services.AddHostedService<AutoCloseHostedService>();
services.AddHostedService<EscalationHostedService>();

// ITSM Services - Advanced (Phase 4 - optional)
// services.AddScoped<IAssignmentRulesEngine, AssignmentRulesEngine>();
// services.AddScoped<IImpactAnalysisService, ImpactAnalysisService>();
// services.AddScoped<IDiscoveryService, DiscoveryService>();
```

---

## Fix #6: Entity Models - Add Missing DbSets

### File to Modify
`CRM.Backend/src/CRM.Core/Interfaces/ICrmDbContext.cs`

### Code to Add

Find the section with ITSM entity definitions:
```csharp
DbSet<CRM.Core.Entities.ITSM.Incident> Incidents { get; }
DbSet<CRM.Core.Entities.ITSM.Problem> Problems { get; }
DbSet<CRM.Core.Entities.ITSM.ProblemIncident> ProblemIncidents { get; }
```

**Add after these:**
```csharp
    // NEW - Add these DbSets
    DbSet<CRM.Core.Entities.ITSM.SLAInstance> ITSMSLAInstances { get; }
    DbSet<CRM.Core.Entities.ITSM.SLABreachHistory> ITSMSLABreachHistories { get; }
    DbSet<CRM.Core.Entities.ITSM.CIRelationship> ConfigurationItemRelationships { get; }
    DbSet<CRM.Core.Entities.ITSM.ChangeImpactedCI> ChangeImpactedCIs { get; }
    DbSet<CRM.Core.Entities.ITSM.BusinessDay> BusinessDays { get; }
    DbSet<CRM.Core.Entities.ITSM.ApprovalWorkflow> CatalogApprovalWorkflows { get; }
    DbSet<CRM.Core.Entities.ITSM.ApprovalStage> CatalogApprovalStages { get; }
    DbSet<CRM.Core.Entities.ITSM.ApprovalAction> CatalogApprovalActions { get; }
```

---

## Fix #7: Remove .disabled Extension

Once all fixes are applied per service, remove the `.disabled` extension:

```bash
# Example for IncidentService:
mv IncidentService.cs.disabled IncidentService.cs

# Or bulk rename all fixed services:
cd CRM.Backend/src/CRM.Infrastructure/Services/ITSM
for f in *.disabled; do 
    if [[ "$f" == "IncidentService"* ]] || [[ "$f" == "ProblemService"* ]]; then
        mv "$f" "${f%.disabled}"
    fi
done
```

---

## Fix #8: Update EscalationHostedService

**Note:** This file is ENABLED but should be updated per pattern

### Changes Needed:
```csharp
// BEFORE:
private readonly IDbContextResolver _dbContextResolver;
using CRM.Infrastructure.Data;

// AFTER:
private readonly ICrmDbContext _context;
using CRM.Core.Interfaces;  // Remove Infrastructure.Data reference
```

Replace all `_dbContextResolver.ResolveContext()` calls with `_context`.

---

## Verification Checklist

After applying all fixes, verify:

### Compilation
- [ ] Solution builds without errors: `dotnet build`
- [ ] No 'CS0246' errors (missing namespaces)
- [ ] No 'CS0103' errors (undefined variables)

### Namespace Imports
- [ ] All `IDbContextResolver` references removed
- [ ] All `CRM.Core.DTOs.ITSM` imports correct

### DI Registration
- [ ] All services registered in Program.cs
- [ ] No duplicate registrations
- [ ] No circular dependencies
- [ ] DI container builds: `services.BuildServiceProvider()`

### Database
- [ ] New DbSets added to ICrmDbContext (8 new DbSets)
- [ ] Migration created: `dotnet ef migrations add ...`
- [ ] Database schema updated: `dotnet ef database update`

### File Extensions
- [ ] Correct .disabled files renamed to .cs (remove extension)
- [ ] Leave advanced services as .disabled for now

### Project Structure
```
✅ Core/Interfaces/ITSM/
   ├─ IBusinessHoursCalculator.cs (MOVED from service)
   ├─ IIncidentService.cs (EXISTS - in IITSMServices.cs)
   ├─ IProblemService.cs (EXISTS - in IITSMServices.cs)
   └─ ... other extracted interfaces

✅ Core/Entities/ITSM/
   ├─ Incident.cs ✅
   ├─ Problem.cs ✅
   ├─ Change.cs ✅
   ├─ SLAInstance.cs (NEW)
   ├─ CIRelationship.cs (NEW)
   └─ ChangeImpactedCI.cs (NEW)

✅ Core/DTOs/ITSM/
   ├─ ITSMDtos.cs (with new filter DTOs)

✅ Infrastructure/Services/ITSM/
   ├─ BusinessHoursCalculator.cs (renamed from .disabled)
   ├─ IncidentService.cs (renamed from .disabled)
   ├─ ... other enabled services
```

---

## Testing After Fixes

### Quick Test Command
```bash
# Navigate to backend
cd CRM.Backend

# Clean build
dotnet clean
dotnet build

# Run tests
dotnet test

# Entity Framework validation
dotnet ef dbcontext validate
```

### Expected Results
- ✅ 0 build errors
- ✅ 0 build warnings
- ✅ All unit tests pass
- ✅ EF context validates successfully
- ✅ No migration errors

---

**END OF QUICK REFERENCE GUIDE**

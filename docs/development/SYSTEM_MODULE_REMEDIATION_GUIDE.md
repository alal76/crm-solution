# System Module Test Blocker - Remediation Implementation Guide

**Date:** February 15, 2026  
**Priority:** CRITICAL  
**Estimated Duration:** 4-6 hours

---

## Overview

The System Module (SYS-001 through SYS-012) test suite cannot execute because the backend code has **188 compilation errors**. This guide provides step-by-step instructions to remediate each blockerinto three phases.

---

## Phase 1: Critical Build Blockers (Hours 1-3)

### Phase 1.1: Create Missing DTOs in Core
**Location:** `src/CRM.Core/Dtos/ITSM/`  
**Priority:** CRITICAL (unblocks 40+ CS0246 errors)

#### Task 1.1.1: Create CommissionRuleDtos.cs
```csharp
File: src/CRM.Core/Dtos/CommissionRuleDto.cs

namespace CRM.Core.Dtos;

public class CommissionRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal CommissionPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCommissionRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CommissionPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCommissionRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool? IsActive { get; set; }
}
```

#### Task 1.1.2: Create DiscountRuleDtos.cs
```csharp
File: src/CRM.Core/Dtos/DiscountRuleDto.cs

namespace CRM.Core.Dtos;

public class DiscountRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDiscountRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDiscountRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public bool? IsActive { get; set; }
}
```

#### Task 1.1.3: Create SLAPolicyDtos.cs
```csharp
File: src/CRM.Core/Dtos/ITSM/SLAPolicyDto.cs

namespace CRM.Core.Dtos.ITSM;

public class SLAPolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSLAPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ResponseTimeMinutes { get; set; }
    public int ResolutionTimeMinutes { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSLAPolicyDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? ResponseTimeMinutes { get; set; }
    public int? ResolutionTimeMinutes { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
}
```

#### Task 1.1.4: Create EscalationRuleDtos.cs
```csharp
File: src/CRM.Core/Dtos/ITSM/EscalationRuleDto.cs

namespace CRM.Core.Dtos.ITSM;

public class EscalationRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int EscalationTimeoutMinutes { get; set; }
    public string? AssignTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateEscalationRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EscalationTimeoutMinutes { get; set; }
    public string? AssignTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateEscalationRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? EscalationTimeoutMinutes { get; set; }
    public string? AssignTo { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateEscalationPolicyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EscalationTimeoutMinutes { get; set; }
    public string? AssignTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class EscalationPolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int EscalationTimeoutMinutes { get; set; }
    public string? AssignTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Task 1.1.5: Create ServiceQueueDtos.cs
```csharp
File: src/CRM.Core/Dtos/ITSM/ServiceQueueDto.cs

namespace CRM.Core.Dtos.ITSM;

public class ServiceQueueDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int AssigneeId { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateServiceQueueDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AssigneeId { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateServiceQueueDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? AssigneeId { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
}
```

---

### Phase 1.2: Fix CrmDbContext.cs Ambiguous References
**Location:** `src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Priority:** CRITICAL (unblocks CS0104 errors - 2 instances)  
**Lines:** 355, 359

**Current Code (Lines 350-365):**
```csharp
// Remove duplicate SLAPolicy and EscalationRule definitions
// and use fully qualified names or consolidate to one location
```

**Remediation Option A: Use Fully Qualified Names**
```csharp
// Line 355 - Change from:
DbSet<SLAPolicy>

// To:
DbSet<CRM.Core.Entities.SLAPolicy> // Explicitly reference the non-KB version

// Line 359 - Change from:
DbSet<EscalationRule>

// To:
DbSet<CRM.Core.Entities.EscalationRule> // Explicitly reference the non-KB version
```

**Remediation Option B (Preferred): Remove KnowledgeBase Duplicates**

1. Delete `src/CRM.Core/Entities/KnowledgeBase/SLAPolicy.cs`
2. Delete `src/CRM.Core/Entities/KnowledgeBase/EscalationRule.cs`
3. Consolidate into main namespace:
   - Move "SLAPolicy" to `src/CRM.Core/Entities/SLAPolicy.cs`
   - Move "EscalationRule" to `src/CRM.Core/Entities/EscalationRule.cs`
4. Update all references to use the main namespace version
5. Add any KB-specific properties to the main entity

**Recommended:** Use Option A (fully qualified names) for immediate fix, then do Option B refactoring in next iteration.

---

### Phase 1.3: Implement AdminConfigurationService Missing Methods
**Location:** `src/CRM.Infrastructure/Services/AdminConfigurationService.cs`  
**Priority:** CRITICAL (unblocks 46+ CS0535/CS0738 errors)  
**Files:** AdminConfigurationService.cs + IAdminConfigurationService.cs

#### Step 1: Review Interface
Check `src/CRM.Core/Interfaces/IAdminConfigurationService.cs` for required method signatures.

#### Step 2: Implement Commission Rule Methods
```csharp
public async Task<CommissionRuleDto> GetCommissionRuleByIdAsync(int id, CancellationToken cancellationToken = default)
{
    var rule = await _dbContext.Set<CommissionRule>()
        .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    return rule == null ? null : MapToDto(rule);
}

public async Task<CommissionRuleDto> CreateCommissionRuleAsync(
    CreateCommissionRuleDto dto, 
    int? userId = null, 
    CancellationToken cancellationToken = default)
{
    var rule = new CommissionRule
    {
        Name = dto.Name,
        Description = dto.Description,
        CommissionPercentage = dto.CommissionPercentage,
        MinAmount = dto.MinAmount,
        MaxAmount = dto.MaxAmount,
        IsActive = dto.IsActive,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    _dbContext.Set<CommissionRule>().Add(rule);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return MapToDto(rule);
}

public async Task<CommissionRuleDto> UpdateCommissionRuleAsync(
    int id, 
    UpdateCommissionRuleDto dto, 
    int? userId = null, 
    CancellationToken cancellationToken = default)
{
    var rule = await _dbContext.Set<CommissionRule>()
        .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    if (rule == null) return null;
    
    if (!string.IsNullOrEmpty(dto.Name)) rule.Name = dto.Name;
    if (!string.IsNullOrEmpty(dto.Description)) rule.Description = dto.Description;
    if (dto.CommissionPercentage.HasValue) rule.CommissionPercentage = dto.CommissionPercentage.Value;
    if (dto.MinAmount.HasValue) rule.MinAmount = dto.MinAmount;
    if (dto.MaxAmount.HasValue) rule.MaxAmount = dto.MaxAmount;
    if (dto.IsActive.HasValue) rule.IsActive = dto.IsActive.Value;
    
    rule.UpdatedAt = DateTime.UtcNow;
    _dbContext.Set<CommissionRule>().Update(rule);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return MapToDto(rule);
}

public async Task<bool> DeleteCommissionRuleAsync(
    int id, 
    int? userId = null, 
    CancellationToken cancellationToken = default)
{
    var rule = await _dbContext.Set<CommissionRule>()
        .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    if (rule == null) return false;
    
    rule.IsDeleted = true;
    rule.UpdatedAt = DateTime.UtcNow;
    _dbContext.Set<CommissionRule>().Update(rule);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return true;
}
```

#### Step 3: Repeat for Discount Rules, SLA Policies, Escalation Rules, and Service Queues
Follow the same pattern for:
- Discount Rule methods (4 methods)
- SLA Policy methods (4 methods)
- Escalation Rule methods (4 methods)
- Service Queue methods (4 methods)

**Total methods to implement:** 20 methods

#### Step 4: Fix Return Types
Change all `DeleteX` methods from:
```csharp
public async Task DeleteXAsync(...) // WRONG
```

To:
```csharp
public async Task<bool> DeleteXAsync(...) // CORRECT
```

---

## Phase 2: Fix Missing References (Hour 3-4)

### Phase 2.1: Add Missing Using Statements

#### File: PerformanceOptimizationService.cs
**Location:** `src/CRM.Infrastructure/Services/PerformanceOptimizationService.cs`  
**Add these lines at the top:**
```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
```

#### File: FeatureFlagManagementService.cs
**Location:** `src/CRM.Infrastructure/Services/FeatureFlagManagementService.cs`  
**Add these lines at the top:**
```csharp
using Microsoft.Extensions.Logging;
```

#### File: UserInterfaceService.cs
**Location:** `src/CRM.Infrastructure/Services/UserInterfaceService.cs`  
**Add these lines at the top:**
```csharp
using Microsoft.Extensions.Logging;
```

---

## Phase 3: Verification & Testing (Hour 5-6)

### Phase 3.1: Clean and Rebuild
```bash
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend

# Clean previous builds
dotnet clean

# Rebuild Infrastructure project
dotnet build src/CRM.Infrastructure/CRM.Infrastructure.csproj

# If successful, rebuild all
dotnet build
```

### Phase 3.2: Verify Compilation Success
```bash
# Should show 0 errors
dotnet build 2>&1 | grep "error CS" | wc -l
```

### Phase 3.3: Build Test Project
```bash
dotnet build tests/CRM.Tests.csproj
```

### Phase 3.4: List Tests
```bash
dotnet test tests/CRM.Tests.csproj --list-tests 2>&1 | head -50
```

---

## Implementation Checklist

### Phase 1
- [ ] Create CommissionRuleDtos.cs
- [ ] Create DiscountRuleDtos.cs
- [ ] Create SLAPolicyDtos.cs (in ITSM folder)
- [ ] Create EscalationRuleDtos.cs (in ITSM folder)
- [ ] Create ServiceQueueDtos.cs (in ITSM folder)
- [ ] Fix CrmDbContext.cs line 355 (SLAPolicy reference)
- [ ] Fix CrmDbContext.cs line 359 (EscalationRule reference)
- [ ] Implement 20 methods in AdminConfigurationService
- [ ] Fix return types for all 5 Delete methods

### Phase 2
- [ ] Add using statements to PerformanceOptimizationService.cs
- [ ] Add using statements to FeatureFlagManagementService.cs
- [ ] Add using statements to UserInterfaceService.cs

### Phase 3
- [ ] Clean and rebuild solution
- [ ] Verify 0 compilation errors
- [ ] Test project builds successfully
- [ ] Prepare for test execution

---

## Success Criteria

✅ **Phase 1 Complete:** All 188 errors eliminated from Infrastructure build  
✅ **Phase 2 Complete:** All Services build with correct dependencies  
✅ **Phase 3 Complete:** Test project builds and tests can be discovered  

Once this is completed, proceed to **System Module Test Execution** as documented in:  
[SYSTEM_MODULE_TEST_EXECUTION_REPORT.md](docs/test/SYSTEM_MODULE_TEST_EXECUTION_REPORT.md)

---

## Time Estimate Breakdown

| Phase | Task | Duration |
|-------|------|----------|
| 1 | Create DTOs | 45 min |
| 1 | Fix CrmDbContext | 15 min |
| 1 | Implement AdminConfigurationService | 90 min |
| 2 | Add using statements | 15 min |
| 3 | Clean, rebuild, verify | 30 min |
| **TOTAL** | | **~3 hours** |

---

**Report Generated:** February 15, 2026  
**Next Step:** Execute Phase 1 immediately


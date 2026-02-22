# Commission Plans - Implementation Fixes

## Quick Fix Summary

**Root Cause:** DTO and controller routing misalignment
**Impact:** Users cannot create commission plans
**Effort:** ~2-4 hours developer time

---

## Fix #1: Consolidate DTOs (CRITICAL)

### Step 1: Add missing request types to CommissionManagementDtos.cs

**File:** `CRM.Backend/src/CRM.Core/Dtos/CommissionManagementDtos.cs`

**Add at the end of the file (before namespace closing brace):**

```csharp
/// <summary>
/// Request to create a commission plan.
/// </summary>
public class CommissionPlanCreateRequest
{
    [Required(ErrorMessage = "Plan name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    
    public int? CommissionType { get; set; }  // 0=FlatPercentage, 1=TieredPercentage, etc.
    
    [Range(0, 100)]
    public decimal? BaseRate { get; set; }

    public int? Trigger { get; set; }  // 0=OnClose, 1=OnOrder, etc.
    
    public int? ClawbackPeriodDays { get; set; }
    public decimal? MinDealSize { get; set; }
    public decimal? MaxCommissionPerDeal { get; set; }
    public decimal? MaxCommissionPerPeriod { get; set; }
    public bool? AllowSplits { get; set; }
    public decimal? DefaultOverlayPercent { get; set; }
}

/// <summary>
/// Request to update a commission plan.
/// </summary>
public class CommissionPlanUpdateRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? Status { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public int? FiscalYear { get; set; }
    public int? CommissionType { get; set; }

    [Range(0, 100)]
    public decimal? BaseRate { get; set; }

    public int? Trigger { get; set; }
    public int? ClawbackPeriodDays { get; set; }
    public decimal? MinDealSize { get; set; }
    public decimal? MaxCommissionPerDeal { get; set; }
    public decimal? MaxCommissionPerPeriod { get; set; }
    public bool? AllowSplits { get; set; }
    public decimal? DefaultOverlayPercent { get; set; }
}
```

### Step 2: Remove inline types from CommissionsController.cs

**File:** `CRM.Backend/src/CRM.Api/Controllers/CommissionsController.cs`

**Remove these lines (around 1098-1160):**
```csharp
/// <summary>
/// Request to create a commission plan.
/// </summary>
public class CommissionPlanCreateRequest
{
    ...entire class...
}

/// <summary>
/// Request to update a commission plan.
/// </summary>
public class CommissionPlanUpdateRequest
{
    ...entire class...
}
```

### Step 3: Update CommissionsController using statements

**File:** `CRM.Backend/src/CRM.Api/Controllers/CommissionsController.cs`

**Ensure using statement at top:**
```csharp
using CRM.Core.Dtos;  // Add this if not present
```

### Step 4: Delete empty CommissionPlanDtos.cs

**File:** `CRM.Backend/src/CRM.Core/Dtos/CommissionPlanDtos.cs`

**Action:** DELETE this file entirely - it's just an empty header

---

## Fix #2: Route Consolidation (CRITICAL)

### Option A: Fix CommissionPlansController route (RECOMMENDED)

**File:** `CRM.Backend/src/CRM.Api/Controllers/CommissionPlansController.cs`

**Change line 20 from:**
```csharp
[Route("api/[controller]")]  // Results in /api/commissionplans
```

**To:**
```csharp
[Route("api/commissions/plans")]  // Results in /api/commissions/plans
```

**Updated full header:**
```csharp
namespace CRM.Api.Controllers;

/// <summary>
/// API Controller for managing commission plans.
/// Provides endpoints for CRUD operations and plan management.
/// </summary>
[ApiController]
[Route("api/commissions/plans")]  // ← CHANGED
[Authorize]
public class CommissionPlansController : ControllerBase
{
    // ... rest of class
}
```

### Option B: Delete CommissionPlansController (ALTERNATIVE)

**If Option A is chosen:** Skip this section

**If preferring Option B:** Delete entire file and use CommissionsController exclusively

**File to delete:** `CRM.Backend/src/CRM.Api/Controllers/CommissionPlansController.cs`

Then remove from Program.cs registration if present.

---

## Fix #3: Update DTO Usage in CommissionPlansController

**File:** `CRM.Backend/src/CRM.Api/Controllers/CommissionPlansController.cs`

**Change Create method signature from:**
```csharp
public async Task<IActionResult> Create(
    [FromBody] CreateCommissionPlanDto dto,
    CancellationToken cancellationToken = default)
```

**To:**
```csharp
public async Task<IActionResult> Create(
    [FromBody] CommissionPlanCreateRequest dto,
    CancellationToken cancellationToken = default)
```

**Update method body to handle the mapping:**
```csharp
[HttpPost]
[ProducesResponseType(typeof(CommissionPlanDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Create(
    [FromBody] CommissionPlanCreateRequest dto,
    CancellationToken cancellationToken = default)
{
    try
    {
        _logger.LogInformation("Creating new commission plan: {PlanName}", dto.Name);
        var result = await _service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating commission plan");
        return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
    }
}
```

**Similarly update Update method:**
```csharp
[HttpPut("{id}")]
[ProducesResponseType(typeof(CommissionPlanDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Update(
    int id,
    [FromBody] CommissionPlanUpdateRequest dto,  // ← Changed from UpdateCommissionPlanDto
    CancellationToken cancellationToken = default)
{
    // ... rest of method
}
```

---

## Fix #4: Update CommissionPlanService CreateAsync

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/CommissionPlanService.cs`

**Update method signature to use the request type:**
```csharp
// OLD
public async Task<CommissionPlanDto> CreateAsync(CreateCommissionPlanDto dto, CancellationToken cancellationToken = default)

// NEW
public async Task<CommissionPlanDto> CreateAsync(CommissionPlanCreateRequest dto, CancellationToken cancellationToken = default)
```

**Update interface too:**
**File:** `CRM.Backend/src/CRM.Core/Interfaces/ICommissionPlanService.cs`

```csharp
// OLD
Task<CommissionPlanDto> CreateAsync(CreateCommissionPlanDto dto, CancellationToken cancellationToken = default);

// NEW
Task<CommissionPlanDto> CreateAsync(CommissionPlanCreateRequest dto, CancellationToken cancellationToken = default);
```

**Do the same for UpdateAsync:**
```csharp
// OLD
Task<CommissionPlanDto> UpdateAsync(int id, UpdateCommissionPlanDto dto, CancellationToken cancellationToken = default);

// NEW
Task<CommissionPlanDto> UpdateAsync(int id, CommissionPlanUpdateRequest dto, CancellationToken cancellationToken = default);
```

---

## Fix #5: Verify Database Schema

**Run this command to check for schema issues:**
```bash
cd CRM.Backend
dotnet ef migrations add CommissionPlanSchemaVerification --project src/CRM.Infrastructure --startup-project src/CRM.Api
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

**Expected CommissionPlan table columns:**
```sql
CREATE TABLE `CommissionPlans` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  `Code` varchar(50) NULL,
  `Description` longtext NULL,
  `CommissionType` int NOT NULL,
  `Trigger` int NOT NULL,
  `BaseRate` decimal(18,2) NOT NULL,
  `Status` int NOT NULL DEFAULT 0,
  `EffectiveStartDate` datetime(6) NULL,
  `EffectiveEndDate` datetime(6) NULL,
  `FiscalYear` int NULL,
  `ClawbackPeriodDays` int NULL,
  `MinDealSize` decimal(18,2) NULL,
  `MaxCommissionPerDeal` decimal(18,2) NULL,
  `MaxCommissionPerPeriod` decimal(18,2) NULL,
  `AllowSplits` bit NOT NULL,
  `DefaultOverlayPercent` decimal(18,2) NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `IsDeleted` bit NOT NULL,
  `RowVersion` longblob NULL,
  PRIMARY KEY (`Id`)
);
```

---

## Fix #6: Clean Up StyleCop Warnings

### Add headers to test files

**File:** `CRM.Backend/tests/Integration/Controllers/CommissionPlansControllerTests.cs`

**Add at very top (before any code):**
```csharp
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

```

**Add newline at end of file (after closing brace)**

**Do the same for:** `CRM.Backend/tests/Integration/Controllers/CommissionsControllerTests.cs`

---

## Fix #7: Fix Null Reference Warnings in CommissionPlanService

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/CommissionPlanService.cs`

**Around line 239-247, add null checks:**

```csharp
// OLD (lines 235-250)
var tiers = JsonConvert.DeserializeObject<List<CommissionTier>>(plan.TiersJson!);
tiers.Add(new CommissionTier
{
    Name = tier.Name,
    TierOrder = tier.TierOrder,
    MinimumAmount = tier.MinValue,
    MaximumAmount = tier.MaxValue,
});

// NEW - Add null safety
if (string.IsNullOrEmpty(plan.TiersJson))
{
    plan.TiersJson = JsonConvert.SerializeObject(new List<CommissionTier>());
}

var tiers = JsonConvert.DeserializeObject<List<CommissionTier>>(plan.TiersJson);
if (tiers == null)
{
    tiers = new List<CommissionTier>();
}

tiers.Add(new CommissionTier
{
    Name = tier.Name,
    TierOrder = tier.TierOrder,
    MinimumAmount = tier.MinValue,
    MaximumAmount = tier.MaxValue,
});

plan.TiersJson = JsonConvert.SerializeObject(tiers);
```

---

## Fix #8: Build and Test

**After making all changes above:**

```bash
# Build solution
cd CRM.Backend
dotnet build CRM.sln

# Run tests
dotnet test tests/CRM.Tests

# Verify no warnings
dotnet build CRM.sln /p:TreatWarningsAsErrors=true
```

---

## Verification Checklist

After implementing fixes, verify:

- [ ] No compilation errors
- [ ] No StyleCop warnings
- [ ] No null reference warnings
- [ ] CommissionPlanDtos.cs deleted
- [ ] CommissionPlanCreateRequest moved to CommissionManagementDtos.cs
- [ ] CommissionsController updated to use types from Dtos
- [ ] CommissionPlansController route adjusted to `/api/commissions/plans`
- [ ] Database schema includes all required columns
- [ ] Services properly implement CreateAsync and UpdateAsync
- [ ] Unit tests compile and run
- [ ] Frontend can create a plan via POST /api/commissions/plans
- [ ] Frontend can list plans via GET /api/commissions/plans
- [ ] Frontend can update plan via PUT /api/commissions/plans/{id}
- [ ] Frontend can delete plan via DELETE /api/commissions/plans/{id}

---

## Testing Commission Plan Creation

**Manual test after fixes:**

1. **Start the application:**
   ```bash
   cd CRM.Backend/src/CRM.Api
   dotnet run
   ```

2. **Navigate to:** `http://localhost:5000/commissions` in browser

3. **Create a test plan:**
   - Click "Add Plan" button
   - Fill in form:
     - Name: "Test Plan"
     - Commission Type: "Flat Percentage"
     - Trigger: "On Close"
     - Base Rate: "5"
   - Click "Save"

4. **Expected result:**
   - Plan appears in list below
   - No error messages
   - Plan properties match input

5. **Curl test (if manual doesn't work):**
   ```bash
   curl -X POST http://localhost:5000/api/commissions/plans \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{
       "name": "Test Plan",
       "commissionType": 0,
       "trigger": 0,
       "baseRate": 0.05,
       "allowSplits": true
     }'
   ```

   Expected response:
   ```json
   {
     "id": 1,
     "name": "Test Plan",
     "commissionType": 0,
     "trigger": 0,
     "baseRate": 0.05,
     "status": 0,
     "createdAt": "2026-02-22T...",
     "updatedAt": "2026-02-22T..."
   }
   ```

---

## Reference: Complete File Changes Summary

| File | Change | Lines to modify |
|------|--------|-----------------|
| CommissionManagementDtos.cs | Add CommissionPlanCreateRequest, CommissionPlanUpdateRequest | End of file |
| CommissionsController.cs | Remove CommissionPlanCreateRequest, CommissionPlanUpdateRequest | ~1098-1160 |
| CommissionPlansController.cs | Change route to "api/commissions/plans" | Line 20 |
| CommissionPlanService.cs | Update method signatures | CreateAsync, UpdateAsync signatures |
| ICommissionPlanService.cs | Update interface signatures | CreateAsync, UpdateAsync declarations |
| CommissionPlanDtos.cs | DELETE entire file | - |
| Program.cs | No changes needed | - |
| CommissionsPage.tsx | No changes needed | - |
| commissionService.ts | No changes needed | - |

---

End of Implementation Fixes

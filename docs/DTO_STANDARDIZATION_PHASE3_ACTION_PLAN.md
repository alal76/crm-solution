# DTO Standardization - Phase 3 Action Plan

**Date:** February 16, 2026  
**Status:** 🔴 PENDING EXECUTION  
**Estimated Duration:** 4-6 hours  
**Priority:** 🔴 CRITICAL - Blocks production stability/consistency

---

## PHASE 3 EXECUTIVE SUMMARY

Phase 3 involves refactoring and consolidating existing problematic DTOs. This phase is divided into **4 priority tiers** for surgical, risk-mitigated execution:

| Tier | Priority | Items | Est. Time | Risk |
|------|----------|-------|-----------|------|
| **Tier 1** | 🔴 CRITICAL | 5 items | 1-2 hrs | LOW |
| **Tier 2** | 🔴 HIGH | 4 items | 2-3 hrs | LOW |
| **Tier 3** | 🟡 MEDIUM | 4 items | 2-3 hrs | MEDIUM |
| **Tier 4** | 🟢 LOW | 4+ items | 1-2 hrs | LOW |

---

## TIER 1: CRITICAL (Execute First - 1-2 Hours)

### T1.1: DELETE ColorPaletteDtos.cs (FILE COMPLETELY EMPTY)
**File:** `CRM.Backend/src/CRM.Core/Dtos/ColorPaletteDtos.cs`  
**Status:** 🔴 DUPLICATE/DEAD CODE  
**Action:** DELETE  
**Reason:** File contains only comment, actual class is in ColorPaletteDto.cs  
**Impact:** None (file is not imported anywhere)  
**Steps:**
```bash
# Step 1: Verify no usages exist
grep -r "ColorPaletteDtos" /path/to/codebase

# Step 2: Delete the file
rm CRM.Backend/src/CRM.Core/Dtos/ColorPaletteDtos.cs

# Step 3: Verify build still passes
dotnet build
```

**Risk Level:** ✅ ZERO - File is dead code with no imports

---

### T1.2: Consolidate PasswordReset DTOs (3 Files → 1)
**Files:**
- `CRM.Backend/src/CRM.Core/Dtos/PasswordResetRequest.cs`
- `CRM.Backend/src/CRM.Core/Dtos/PasswordResetConfirm.cs`
- `CRM.Backend/src/CRM.Core/Dtos/SetPasswordRequest.cs`

**Status:** 🔴 DUPLICATE FUNCTIONALITY  
**Action:** Consolidate into single file  
**Reason:** All three handle password reset but defined separately  

**Current Structure (BEFORE):**
```csharp
// PasswordResetRequest.cs
public class PasswordResetRequest { public string Email { get; set; } }

// PasswordResetConfirm.cs
public class PasswordResetConfirm { public string Code { get; set; } public string NewPassword { get; set; } }

// SetPasswordRequest.cs
public class SetPasswordRequest { public string UserId { get; set; } public string Password { get; set; } }
```

**Target Structure (AFTER):**
Create new file: `PasswordManagementDtos.cs`
```csharp
namespace CRM.Core.Dtos;

/// <summary>
/// Request to initiate a password reset (step 1).
/// User provides their email, system sends reset link.
/// </summary>
public class RequestPasswordResetDto : CreateRequestDtoBase
{
    /// <summary>Gets or sets the email address of the account.</summary>
    [Required] [EmailAddress]
    public string Email { get; set; } = "";
}

/// <summary>
/// Request to confirm password reset with code (step 2).
/// User provides reset code and new password.
/// </summary>
public class ConfirmPasswordResetDto : CreateRequestDtoBase
{
    /// <summary>Gets or sets the reset code sent via email.</summary>
    [Required] [StringLength(256)]
    public string ResetCode { get; set; } = "";

    /// <summary>Gets or sets the new password.</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*])")]
    public string NewPassword { get; set; } = "";

    /// <summary>Gets or sets the password confirmation (must match NewPassword).</summary>
    [Required]
    [Compare("NewPassword", ErrorMessage = ValidationMessages.Password.ConfirmationMismatch)]
    public string ConfirmPassword { get; set; } = "";
}

/// <summary>
/// Admin-initiated password set request (no reset code needed).
/// Admin sets password directly for a user.
/// </summary>
public class AdminSetPasswordDto : CreateRequestDtoBase
{
    /// <summary>Gets or sets the user ID to set password for.</summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>Gets or sets the new password.</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = "";
}

/// <summary>
/// Response to password reset request with reset link info.
/// </summary>
public class PasswordResetResponseDto : ReadResponseDtoBase
{
    /// <summary>Gets or sets the email that reset link was sent to.</summary>
    public string Email { get; set; } = "";

    /// <summary>Gets or sets when the reset code expires.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets or sets the message to display to user.</summary>
    public string Message { get; set; } = "";
}
```

**Migration Steps:**
1. Create `PasswordManagementDtos.cs` with new structure
2. Update all imports in 5-10 files currently using old DTOs
3. Keep old files but mark as `[Obsolete]` for 2-week deprecation
4. Run tests to verify
5. Delete old files in next sprint

**Files to Update Imports:**
```
Controllers/AuthController.cs (3 usages)
Services/AuthService.cs (2 usages)
CRM.Api/Middleware/AuthMiddleware.cs (1 usage)
Tests/*AuthTests.cs (4 usages)
Frontend (if any - API clients)
```

**Risk Level:** ✅ LOW - Old files kept with deprecation warnings

---

### T1.3: Fix Financial DTOs - Add Validation (3 Files)
**Critical Issue:** Financial DTOs using `double` instead of `decimal`, missing validations

#### T1.3.1: InvoiceDto.cs
**Current Issues:**
```csharp
// ❌ WRONG - Using double, no validation
public double Amount { get; set; }
public double TaxAmount { get; set; }
```

**Target Fix:**
```csharp
// ✅ CORRECT - Decimal with validation
[Required(ErrorMessage = ValidationMessages.Invoice.AmountRequired)]
[Range(0.01, 999999999.99, ErrorMessage = ValidationMessages.Currency.InvalidAmount)]
[DecimalPrecision(18, 4)]
public decimal Amount { get; set; }

[Range(0, 999999999.99, ErrorMessage = ValidationMessages.Currency.CannotBeNegative)]
[DecimalPrecision(18, 4)]
public decimal TaxAmount { get; set; }

// New validation: Tax cannot exceed total
[CustomValidation(typeof(InvoiceValidator), nameof(InvoiceValidator.ValidateTaxAmount))]
public decimal TotalDue { get; set; }  // Amount + Tax
```

**Validation Class to Add:**
```csharp
namespace CRM.Core.Dtos.Validators;

public static class InvoiceValidator
{
    public static ValidationResult ValidateTaxAmount(decimal totalDue, ValidationContext context)
    {
        var invoice = (InvoiceDto)context.ObjectInstance;
        if (invoice.TaxAmount > invoice.Amount)
            return new ValidationResult(ValidationMessages.Currency.RefundExceedsPayment);
        return ValidationResult.Success;
    }
}
```

#### T1.3.2: PaymentDto.cs
**Current Issues:**
```csharp
// ❌ WRONG
public double Amount { get; set; }  // No validation, could be <= 0
public double RefundAmount { get; set; }  // No check against Amount
```

**Target Fix:**
```csharp
[Required(ErrorMessage = ValidationMessages.Currency.MustBePositive)]
[Range(0.01, 999999999.99)]
[DecimalPrecision(18, 4)]
public decimal Amount { get; set; }

[Range(0, 999999999.99)]
[DecimalPrecision(18, 4)]
[CustomValidation(typeof(PaymentValidator), nameof(PaymentValidator.ValidateRefundAmount))]
public decimal RefundAmount { get; set; }

public PaymentStatus Status { get; set; }  // Change from int to enum
```

#### T1.3.3: SubscriptionDtos.cs
**Current Issues:**
```csharp
// ❌ WRONG
public int Status { get; set; }  // Should be enum
public double Price { get; set; }  // Should be decimal
```

**Target Fix:**
```csharp
[Range(0.01, 999999999.99)]
[DecimalPrecision(18, 2)]
public decimal Price { get; set; }

public SubscriptionStatus Status { get; set; }  // Use enum from StandardEnums.cs

public FrequencyInterval BillingFrequency { get; set; }  // New: Use enum instead of int

// Add renewal date validation
[DateRange(minDaysFromToday: 1)]  // Renewal must be in future
public DateTime? RenewalDate { get; set; }
```

**Files to Create/Update:**
- Create: `CRM.Backend/src/CRM.Core/Dtos/Validators/FinancialValidators.cs` (utility validators)
- Update: `InvoiceDto.cs`, `PaymentDto.cs`, `SubscriptionDtos.cs`
- Update: Controllers and tests that use these DTOs

**Risk Level:** ✅ LOW - Adding validations (stricter but safe if existing data valid)

---

### T1.4: Consolidate ITSM Duplicates (Root → Delete)
**Issue:** Same DTOs defined in 2 places (root and ITSM subdirectory)

**Duplicates to Remove (Delete Root Versions):**
1. `/Dtos/SLAPolicyDto.cs` → Keep `/Dtos/ITSM/ITSMDtos.cs` version
2. `/Dtos/ServiceQueueDto.cs` → Keep `/Dtos/ITSM/ITSMDtos.cs` version
3. `/Dtos/EscalationRuleDto.cs` → Keep `/Dtos/ITSM/ITSMDtos.cs` version
4. `/Dtos/WebhookManagementDtos.cs` → Keep `/Dtos/ITSM/WebhookDtos.cs` version

**Actions:**
1. Merge ITSM/ITSMDtos.cs into separate files by type
   ```
   ITSM/ITSMDtos.cs → Split into:
   - ITSM/SLAPolicyDtos.cs
   - ITSM/ServiceQueueDtos.cs
   - ITSM/EscalationPolicyDtos.cs
   ```

2. Delete root versions (they're redundant)
3. Search and replace imports throughout codebase:
   ```csharp
   // OLD
   using CRM.Core.Dtos;
   var dto = new SLAPolicyDto();
   
   // NEW
   using CRM.Core.Dtos.ServiceDesk;
   var dto = new SLAPolicyDto();
   ```

4. Update ~15 files that import from root ITSM DTOs

**Expected Files to Update:**
```
Controllers/SLAPolicyController.cs
Controllers/ServiceQueueController.cs
Controllers/EscalationController.cs
Services/SLAPolicyService.cs
Services/ServiceQueueService.cs
Services/EscalationService.cs
(+ 8 test files)
```

**Risk Level:** ✅ LOW - Just namespace reorganization

---

### T1.5: Add Response Wrappers (Priority List)
**Action:** Update controller return types to use `ApiResponse<T>` wrapper

**Priority Controllers to Update (30+ DTOs affected):**
1. AccountsController.cs (5 endpoints)
2. ContactsController.cs (5 endpoints)
3. OpportunitiesController.cs (5 endpoints)
4. InvoicesController.cs (5 endpoints)
5. PaymentsController.cs (3 endpoints)
6. CampaignsController.cs (3 endpoints)
7. TicketsController.cs (5 endpoints)
8. UsersController.cs (5 endpoints)

**Pattern to Apply:**
```csharp
// BEFORE
[HttpGet("{id}")]
public async Task<ActionResult<AccountDto>> GetById(int id)
{
    var account = await _service.GetByIdAsync(id);
    return Ok(account);
}

// AFTER
[HttpGet("{id}")]
public async Task<ActionResult<ApiResponse<AccountDto>>> GetById(int id)
{
    var account = await _service.GetByIdAsync(id);
    if (account == null)
        return NotFound(ApiResponse<AccountDto>.NotFoundResponse("Account not found"));
    
    return Ok(ApiResponse<AccountDto>.SuccessResponse(account));
}
```

**Implementation Steps:**
1. Add using statement: `using CRM.Core.Dtos;`
2. Update return type: `ActionResult<ApiResponse<T>>`
3. Wrap successful responses: `ApiResponse<T>.SuccessResponse()`
4. Wrap errors: `ApiResponse<T>.ErrorResponse()`, `.NotFoundResponse()`, etc.
5. For validation errors: `ApiResponse<T>.ValidationErrorResponse()`
6. Update tests to expect wrapped responses

**Risk Level:** ✅ LOW - Additive (doesn't break existing data, just wraps it)

---

## TIER 2: HIGH (2-3 Hours)

### T2.1: Consolidate TwoFactor DTOs (4 Files → 1)
**Files to Consolidate:**
```
TotpDtos.cs
TwoFactorVerification.cs
TwoFactorEnableRequest.cs
TwoFactorSetupResponse.cs
```

**Target:** `TwoFactorAuthDtos.cs` with clear step flow

### T2.2: Add Create/Update/List Patterns to Account/Contact
**Add DTO Variants:**
- `CreateAccountDto` (POST request)
- `UpdateAccountDto` (PATCH request)
- `AccountListDto` (list item with minimal fields)
- `CreateContactDto`, `UpdateContactDto`, `ContactListDto` (same pattern)

### T2.3: Consolidate OAuthDtos
**Consolidate:**
- `OAuthDtos.cs` + `OAuthLoginRequest.cs` → `OAuthProviderDtos.cs`
- Add provider types enum
- Add validation for OAuth flow

### T2.4: Fix Commission/Subscription Type Safety
**Replace int with Enums:**
- Commission Status: `int` → `CommissionStatus` enum
- Subscription Status: ✅ Already in T1.3
- Discount Type: `int` → `DiscountType` enum

---

## TIER 3: MEDIUM (2-3 Hours)

### T3.1: Consolidate Campaign DTOs
**Current:** `CampaignDtos.cs` (aggregates multiple concerns)
**Target:** Split into:
- `CampaignDtos.cs` (main entity DTOs)
- `CampaignRecipientDtos.cs` (recipient targeting)
- `CampaignMetricsDtos.cs` (performance data)
- `EmailTemplateDtos.cs` (email-specific)

### T3.2: Consolidate UI Configuration DTOs
**Consolidate 3 files into 2:**
- `UICustomizationDto.cs` + `DashboardCustomizationDto.cs` → `UIPreferenceDtos.cs`
- `ModuleUIConfigDto.cs` + `ModuleFieldConfigurationDto.cs` → `ModuleConfigurationDtos.cs`

### T3.3: Add Validation to Admin Config DTOs
**Files:**
- `SLAPolicyDto.cs` - Add response time validation
- `ServiceQueueDto.cs` - Add priority enum, assign rules
- `EscalationRuleDto.cs` - Add condition validation
- `PasswordPolicyDto.cs` - Add min/max length validation

### T3.4: Consolidate RBAC DTOs
**Consolidate 2 files:**
- `RBACAndAdminDtos.cs` (currently too aggregated)
- Split into: `RoleDtos.cs`, `PermissionDtos.cs`, `AccessControlDtos.cs`

---

## TIER 4: LOW (1-2 Hours)

### T4.1: Add XML Documentation
**Target:** All DTOs must have `///` comments on all public properties

### T4.2: Add Email/Phone Validation
**Apply to all contact-related DTOs:**
```csharp
[EmailAddress(ErrorMessage = ValidationMessages.Email.Invalid)]
public string Email { get; set; }

[PhoneNumber]  // Custom validation
public string Phone { get; set; }
```

### T4.3: Consolidate Remaining Overlaps
**Examples:**
- PreferencesDto + UIPreferenceDto
- OptionalAuditLoggingDtos + AuditLogDtos
- RelationshipDto (if unused, delete)

### T4.4: Add Missing Validation Ranges
**Example problematic DTOs:**
- PerformanceMetricsDto - add percentage ranges
- BrandingConfigDto - add image URL/size validation
- EmailConfigDto - add SMTP server validation

---

## IMPLEMENTATION SEQUENCE

### Day 1 Morning (1-2 Hours)
```
T1.1: Delete ColorPaletteDtos.cs
      ↓
T1.2: Create PasswordManagementDtos.cs + update imports
      ↓
T1.3: Update financial DTOs (InvoiceDto, PaymentDto, SubscriptionDtos)
      ↓
RUN TESTS after T1.1-T1.3
```

### Day 1 Afternoon (1-2 Hours)
```
T1.4: Reorganize ITSM DTOs + delete root versions + update imports
      ↓
T1.5: Add ApiResponse<T> wrappers to controllers (sample 3-5 controllers)
      ↓
RUN TESTS after T1.4-T1.5
```

### Day 2 Morning (2-3 Hours)
```
T2.1: Consolidate TwoFactor DTOs
      ↓
T2.2: Add Account/Contact variants
      ↓
T2.3: Consolidate OAuth + T2.4: Fix type safety
      ↓
RUN TESTS
```

### Remaining (2-3 Hours)
```
T3.x: Medium priority consolidations
      ↓
T4.x: Low priority polish (comments, validation)
```

---

## TESTING STRATEGY FOR PHASE 3

### After Each Tier:
1. **Compilation Check**
   ```bash
   dotnet build --configuration Debug
   ```

2. **Unit Test Run**
   ```bash
   dotnet test --filter "DTO|Validation"
   ```

3. **Import Verification**
   ```bash
   grep -r "using CRM.Core.Dtos" CRM.Backend/src --include="*.cs" | wc -l
   # Should decrease as consolidation happens
   ```

### Final Verification:
1. All 80+ existing DTOs compile
2. No broken imports
3. Controllers return `ApiResponse<T>`
4. Financial DTOs use decimal + validation
5. Enums used instead of int for status fields
6. All DTOs inherit from base classes

---

## ROLLBACK STRATEGY

### If Issues Found:
1. **Minor Issues:** Revert specific file, retest
2. **Import Breakage:** Use find/replace to fix all usages
3. **Compilation Failure:** Comment out failing DTO, continue rest
4. **Test Failure:** Roll back consolidation, try different approach

### Never:
- Delete old DTOs without tracing all usages first
- Skip tests between changes
- Combine Tier 1 + Tier 2 + Tier 3 changes in one commit

---

## PHASE 3 COMPLETION CHECKLIST

- [ ] Tier 1 (T1.1-T1.5) complete and tested
- [ ] Tier 2 (T2.1-T2.4) complete and tested
- [ ] Tier 3 (T3.1-T3.4) complete and tested
- [ ] Tier 4 (T4.1-T4.4) complete
- [ ] Zero compilation errors
- [ ] All imports updated
- [ ] All tests passing
- [ ] Code review approved
- [ ] Documentation updated (DTO usage guide)
- [ ] Team notified of breaking changes (if any)

---

## MEASURING SUCCESS

After Phase 3, the CRM DTO architecture should have:

| Metric | Before | After | Target |
|--------|--------|-------|--------|
| DTO Files | 85 | 80-90 (consolidated better) | 90-100 |
| Duplicate Files | 6-8 | 0 | 0 |
| Files using int for status | ~15 | 0 | 0 |
| Files missing validation | ~25 | 0 | 0 |
| Response wrapper usage | 20% | 60%+ | 100% |
| API consistency | Low | High | Very High |

---

## PHASE 4 READINESS

After Phase 3 completes:
- ✅ Base architecture in place (Phase 2)
- ✅ Existing DTOs cleaned up (Phase 3)
- ✅ Ready to create 50+ new DTOs (Phase 4) following standard patterns

**Estimated Phase 4 Duration:** 6-8 hours (50+ new DTOs per spec)

---

**Next Step:** Execute Phase 3 Tier 1, verify success with tests, then proceed to Tier 2.


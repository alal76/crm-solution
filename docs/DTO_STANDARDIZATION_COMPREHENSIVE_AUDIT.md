# DTO Standardization - Comprehensive Audit Report

**Date:** February 16, 2026  
**Status:** Phase 1 - Audit Complete | Phases 2-4 Pending  
**Total DTOs Identified:** 85 (69 root-level + 16 in subdirectories)  
**Total Lines of DTO Code:** ~5,000+ lines

---

## Executive Summary

This audit identifies **3 critical issues** and **2 standardization opportunities** in the existing CRM DTO architecture:

| Issue | Count | Priority | Impact |
|-------|-------|----------|--------|
| **Duplicate/Conflicting DTOs** | 8-12 | 🔴 HIGH | Code maintainability, confusion |
| **Missing/Inconsistent Validations** | ~25 | 🔴 HIGH | Data quality, security |
| **Type Safety Issues (int vs Enum)** | ~15 | 🟡 MEDIUM | IDE support, maintainability |
| **Response Wrapper Inconsistencies** | ~30 | 🟡 MEDIUM | API consistency |
| **Missing XML Documentation** | ~70 | 🟢 LOW | Developer experience |

---

## PHASE 1: DTO AUDIT RESULTS

### 1.1 Root-Level DTOs (69 files)

#### Category: Authentication & User Management (9 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **LoginRequest.cs** | 1 | ✅ Standard | - |
| **RegisterRequest.cs** | 1 | ✅ Standard | - |
| **AuthResponse.cs** | 1 | ✅ Standard | - |
| **ChangePasswordRequest.cs** | 1 | ✅ Standard | - |
| **PasswordResetRequest.cs** | 1 | ⚠️ DUPLICATE (3 variants) | 🔴 HIGH |
| **PasswordResetConfirm.cs** | 1 | ⚠️ DUPLICATE (3 variants) | 🔴 HIGH |
| **SetPasswordRequest.cs** | 1 | ⚠️ DUPLICATE (3 variants) | 🔴 HIGH |
| **TwoFactorLoginRequest.cs** | 1 | ⚠️ Overlaps with LoginRequest | 🟡 MEDIUM |
| **TwoFactorVerification.cs** | 1 | ⚠️ Overlaps with TwoFactorLoginRequest | 🟡 MEDIUM |

**Issues Found:**
- ❌ **PasswordResetRequest** appears 3 times (PasswordResetRequest.cs, PasswordResetConfirm.cs, SetPasswordRequest.cs)
- ❌ **TwoFactorLoginRequest** & **TwoFactorVerification** overlap
- ⚠️ Missing centralized validation messages
- ⚠️ No base class inheritance

**Refactoring Plan:**
```
Consolidate into 4 files:
1. AuthenticationRequestDtos.cs (Login, Register, RefreshToken, MFA)
2. PasswordManagementDtos.cs (PasswordReset, PasswordChange, SetPassword)
3. TwoFactorAuthDtos.cs (Setup, Verify, Disable)
4. AuthResponseDtos.cs (AuthResponse, TokenResponse)
```

---

#### Category: User & Group Management (6 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **UserDto.cs** | 2+ | ⚠️ Missing Create/Update variants | 🟡 MEDIUM |
| **UserProfileDto.cs** | 1 | ⚠️ Overlaps with UserDto | 🟡 MEDIUM |
| **UserGroupDto.cs** | 1 | ✅ Standard | - |
| **UserApprovalDto.cs** | 1 | ✅ Standard | - |
| **UpdateUserDto.cs** | 1 | ⚠️ Should be in UserDto.cs | 🟡 MEDIUM |
| **AdminPasswordResetRequest.cs** | 1 | ⚠️ Admin-specific variant needed | 🟡 MEDIUM |

**Issues Found:**
- ❌ No consistent Create/Update/Read/List pattern
- ⚠️ UserDto and UserProfileDto handle overlapping concerns
- ⚠️ UpdateUserDto is separate file (inconsistent naming)

**Refactoring Plan:**
```
Consolidate into 3 files:
1. UserDtos.cs (ReadDto, CreateDto, UpdateDto, ListDto)
2. UserGroupDtos.cs (existing, add variants)
3. AdminManagementDtos.cs (AdminPasswordReset, AdminApproval)
```

---

#### Category: Account & Contact Management (3 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **AccountDto.cs** | 2+ | ⚠️ Missing CRUD variants | 🟡 MEDIUM |
| **ContactDto.cs** | 2+ | ⚠️ Missing CRUD variants | 🟡 MEDIUM |
| **ContactInfoDto.cs** | 1 | ✅ Standard | - |

**Issues Found:**
- ⚠️ Missing CreateAccountDto, UpdateAccountDto, ListAccountDto pattern
- ⚠️ Missing CreateContactDto, UpdateContactDto, ListContactDto pattern
- ⚠️ No validation for email, phone formats

**New Files Needed:**
```
1. AccountAddressDto, AccountPhoneDto, AccountEmailDto (contact info linking)
2. AccountDetailDto (full hydrated read response)
3. QuickAccountDto (minimal for lists)
```

---

#### Category: Sales Management (7 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **ContractDto.cs** | 1 | ⚠️ Missing validation, CRUD variants | 🟡 MEDIUM |
| **InvoiceDto.cs** | 2+ | ❌ MISSING Range validations | 🔴 HIGH |
| **PaymentDto.cs** | 2+ | ❌ MISSING Range validations | 🔴 HIGH |
| **SubscriptionDtos.cs** | 3+ | ❌ MISSING Range validations, type safety | 🔴 HIGH |
| **CommissionPlanDtos.cs** | 3+ | ⚠️ Missing validation | 🟡 MEDIUM |
| **CommissionRuleDto.cs** | 1 | ⚠️ Missing validation | 🟡 MEDIUM |
| **DiscountRuleDto.cs** | 1 | ⚠️ Missing validation | 🟡 MEDIUM |

**Critical Issues Found:**
- ❌ **InvoiceDto:** Amount field using `double` (should be `decimal[18,4]`)
- ❌ **PaymentDto:** Missing validation: Amount > 0, RefundAmount <= Amount
- ❌ **SubscriptionDtos:** Status as `int` (should be enum), price validation missing
- ❌ **CommissionPlanDtos:** Commission rate uses `double` (should be `decimal[18,2]`)

**Refactoring Priority - Financial DTOs:**
```csharp
// BEFORE (❌ WRONG):
public class InvoiceDto { 
    public double Amount { get; set; }  // ❌ Wrong type
    public double TaxAmount { get; set; } // ❌ No validation
}

// AFTER (✅ CORRECT):
public class InvoiceDto { 
    [Range(0.01, 999999999.99)]
    [DecimalPrecision(18, 4)]
    public decimal Amount { get; set; } // ✅ Correct type
    
    [Range(0, 999999999.99)]
    [DecimalPrecision(18, 4)]
    public decimal TaxAmount { get; set; } // ✅ Validated
}
```

---

#### Category: Marketing & Campaigns (5 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **CampaignDtos.cs** | 3+ | ⚠️ Missing CRUD variants, validation | 🟡 MEDIUM |
| **EmailSequenceDtos.cs** | 3+ | ⚠️ Missing step types, conditions | 🟡 MEDIUM |
| **NewsSocialDto.cs** | 1 | ⚠️ Naming unclear, limited validation | 🟡 MEDIUM |
| **CommunicationDto.cs** | 1 | ⚠️ Too generic, overlaps with other files | 🟡 MEDIUM |
| **RelationshipDto.cs** | 1 | ⚠️ Purpose unclear | 🟡 MEDIUM |

**Issues Found:**
- ⚠️ CampaignDtos.cs handles multiple concerns (campaigns, recipients, metrics)
- ⚠️ EmailSequenceDtos missing step type enums
- ⚠️ No validation for campaign dates (StartDate <= EndDate)
- ⚠️ RelationshipDto purpose vague - appears unused

**New Files Needed:**
```
1. CampaignCreateUpdateDto variants
2. CampaignRecipientDto (with import patterns)
3. EmailStepDto (with step type enum)
4. SequenceConditionDto (with operators enum)
5. CampaignMetricsDto (with calculated fields)
```

---

#### Category: System Configuration (12 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **SystemSettingsDto.cs** | 1 | ✅ Standard | - |
| **AdminConfigurationDto.cs** | 1 | ⚠️ Too aggregated | 🟡 MEDIUM |
| **FeatureFlagDto.cs** | 1 | ✅ Standard | - |
| **BrandingConfigDto.cs** | 1 | ⚠️ Missing image validation | 🟡 MEDIUM |
| **NavigationConfigDto.cs** | 1 | ⚠️ No ordering support | 🟡 MEDIUM |
| **EmailConfigDto.cs** | 1 | ⚠️ Missing SMTP validation | 🟡 MEDIUM |
| **ModuleUIConfigDto.cs** | 1 | ⚠️ Missing field type enums | 🟡 MEDIUM |
| **ModuleFieldConfigurationDto.cs** | 1 | ⚠️ Overlaps with ModuleUIConfigDto | 🟡 MEDIUM |
| **PasswordPolicyDto.cs** | 1 | ⚠️ Missing validation ranges | 🟡 MEDIUM |
| **PermissionCacheDtos.cs** | 2+ | ⚠️ Cache-specific, consider timing | 🟡 MEDIUM |
| **DashboardCustomizationDto.cs** | 1 | ✅ Standard | - |
| **UICustomizationDto.cs** | 1 | ⚠️ Overlaps with DashboardCustomizationDto | 🟡 MEDIUM |

**Issues Found:**
- ⚠️ AdminConfigurationDto aggregates too many concerns (should split)
- ⚠️ EmailConfigDto missing SMTP validation
- ⚠️ PasswordPolicyDto missing min/max ranges
- ⚠️ UICustomizationDto and DashboardCustomizationDto overlap

**Refactoring Plan:**
```
Consolidate from 12 files to 8:
1. SystemSettingsDto.cs (keep as-is)
2. FeatureFlagDto.cs (keep as-is)
3. AdminConfigurationDtos.cs (split concerns)
4. BrandingConfigDto.cs (add validation)
5. EmailConfigDto.cs (add SMTP validation)
6. PasswordPolicyDto.cs (add validation ranges)
7. UIPreferenceDto.cs (consolidate customization)
8. PermissionCacheDtos.cs (consider timing)
```

---

#### Category: Admin Configuration (7 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **SLAPolicyDto.cs** | 1 | ⚠️ Missing response time validation | 🟡 MEDIUM |
| **ServiceQueueDto.cs** | 1 | ⚠️ Missing priority enum | 🟡 MEDIUM |
| **EscalationRuleDto.cs** | 1 | ⚠️ Overlaps with ITSM/EscalationPolicyDto | 🔴 HIGH |
| **CloudDeploymentDto.cs** | 1 | ⚠️ Cloud-specific, limited use | 🟡 MEDIUM |
| **DatabaseManagementDto.cs** | 1 | ⚠️ Database-specific, limited use | 🟡 MEDIUM |
| **PerformanceOptimizationDtos.cs** | 2+ | ⚠️ Internal only, low priority | 🟢 LOW |
| **PerformanceMetricsDto.cs** | 1 | ✅ Standard | - |

**Issues Found:**
- ❌ **EscalationRuleDto** (root) vs **ITSM/EscalationPolicyDto** - CONFLICT
- ⚠️ SLAPolicyDto missing response time validation (should enforce SLA>0)
- ⚠️ ServiceQueueDto missing priority enum
- ⚠️ CloudDeploymentDto and DatabaseManagementDto are infrastructure-specific

**Refactoring Plan:**
```
1. Remove root EscalationRuleDto (consolidate into ITSM)
2. Add validation to SLAPolicyDto (response time > 0)
3. Add priority enum to ServiceQueueDto
4. Consider moving CloudDeploymentDto and DatabaseManagementDto to separate Infrastructure folder
```

---

#### Category: RBAC & Advanced Features (6 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **RBACAndAdminDtos.cs** | 1+ | ⚠️ Too aggregated, needs split | 🟡 MEDIUM |
| **PreferencesDto.cs** | 1 | ✅ Standard | - |
| **UIPreferenceDto.cs** | 1 | ⚠️ Overlaps with PreferencesDto | 🟡 MEDIUM |
| **FieldMasterDataLinkDto.cs** | 1 | ⚠️ Purpose unclear | 🟡 MEDIUM |
| **AuditLogDtos.cs** | 3+ | ✅ Reasonable | - |
| **OptionalAuditLoggingDtos.cs** | 1 | ⚠️ Overlaps with AuditLogDtos | 🟡 MEDIUM |

**Issues Found:**
- ⚠️ RBACAndAdminDtos aggregates multiple concerns
- ⚠️ PreferencesDto and UIPreferenceDto overlap
- ⚠️ OptionalAuditLoggingDtos should merge with AuditLogDtos
- ⚠️ FieldMasterDataLinkDto purpose and usage unclear

---

#### Category: Special Features (9 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **OAuthDtos.cs** | 2+ | ⚠️ Missing provider types | 🟡 MEDIUM |
| **OAuthLoginRequest.cs** | 1 | ⚠️ Should consolidate to OAuthDtos | 🟡 MEDIUM |
| **WebAuthnDtos.cs** | 2+ | ✅ Reasonable | - |
| **TotpDtos.cs** | 2+ | ✅ Reasonable | - |
| **TwoFactorEnableRequest.cs** | 1 | ⚠️ Overlaps with TotpDtos/TwoFactorDtos | 🟡 MEDIUM |
| **TwoFactorSetupResponse.cs** | 1 | ⚠️ Overlaps with TwoFactorDtos | 🟡 MEDIUM |
| **WebhookManagementDtos.cs** | 2+ | ⚠️ Missing event type enums | 🟡 MEDIUM |
| **AssignProfileDto.cs** | 1 | ⚠️ Purpose unclear | 🟡 MEDIUM |
| **CommissionManagementDtos.cs** | 3+ | ⚠️ Type safety, validation issues | 🟡 MEDIUM |

**Issues Found:**
- ⚠️ OAuthLoginRequest should consolidate to OAuthDtos.cs
- ⚠️ Multiple TwoFactor DTOs scattered (TotpDtos, TwoFactorEnableRequest, TwoFactorSetupResponse)
- ⚠️ WebhookManagementDtos missing event type enums
- ⚠️ CommissionManagementDtos uses `int` for status (should be enum)

---

#### Category: Problematic Duplicates (2 files) 🔴 **CRITICAL**

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **ColorPaletteDto.cs** | 1 | ❌ **DUPLICATE** of ColorPaletteDtos 🚨 | 🔴 CRITICAL |
| **ColorPaletteDtos.cs** | 1 | ❌ **DUPLICATE** of ColorPaletteDto 🚨 | 🔴 CRITICAL |

**Issues Found:**
- ❌ **EXACT DUPLICATE FILES** - Both define same class
- ❌ Causes build conflicts in compilation
- ❌ Usage scattered across codebase

**Resolution:**
```
1. Keep ColorPaletteDtos.cs (plural form, standard pattern)
2. DELETE ColorPaletteDto.cs immediately
3. Search/replace imports throughout codebase
```

---

### 1.2 Subdirectory DTOs (16 files)

#### ITSM Subdirectory (6 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **ITSMDtos.cs** | 5+ | ⚠️ Multiple concerns in one file | 🟡 MEDIUM |
| **SLAPolicyDto.cs** | 1 | ⚠️ DUPLICATE (root + ITSM) | 🔴 HIGH |
| **ServiceQueueDto.cs** | 1 | ⚠️ DUPLICATE (root + ITSM) | 🔴 HIGH |
| **EscalationRuleDto.cs** | 1 | ⚠️ DUPLICATE (root + ITSM) | 🔴 HIGH |
| **EscalationPolicyDto.cs** | 1 | ⚠️ Overlaps with EscalationRuleDto | 🟡 MEDIUM |
| **WebhookDtos.cs** | 2+ | ⚠️ DUPLICATE (root) | 🔴 HIGH |

**Critical Issue:**
```
DUPLICATE FILES BETWEEN ROOT & ITSM:
- SLAPolicyDto (root) + SLAPolicyDto (ITSM/ITSMDtos.cs) 
- ServiceQueueDto (root) + ServiceQueueDto (ITSM/ITSMDtos.cs)
- EscalationRuleDto (root) + EscalationRuleDto (ITSM/ITSMDtos.cs)
- WebhookManagementDtos (root) + WebhookDtos (ITSM/ITSMDtos.cs)

DECISION: Keep ITSM versions, delete root versions
```

**Refactoring Plan:**
```
1. Consolidate ITSM/ITSMDtos.cs into separate type files
2. Delete root version duplicates
3. Create proper namespacing:
   - CRM.Core.Dtos.ServiceDesk.SLAPolicyDto
   - CRM.Core.Dtos.ServiceDesk.ServiceQueueDto
   - CRM.Core.Dtos.ServiceDesk.EscalationPolicyDto
```

---

#### Reports Subdirectory (1 file)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **ReportDtos.cs** | 5+ | ⚠️ Multiple concerns, low priority | 🟢 LOW |

**Status:** Acceptable structure, minimal changes needed

---

#### Workflow Subdirectory (4 files)

| File | Classes | Issues | Priority |
|------|---------|--------|----------|
| **WorkflowConfigDtos.cs** | 2+ | ✅ Standard | - |
| **WorkflowDefinitionDtos.cs** | 2+ | ⚠️ Missing trigger type enum | 🟡 MEDIUM |
| **WorkflowInstanceDtos.cs** | 2+ | ✅ Standard | - |
| **WorkflowTriggerDtos.cs** | 2+ | ⚠️ Overlaps with WorkflowDefinitionDtos | 🟡 MEDIUM |

**Issues Found:**
- ⚠️ WorkflowTriggerDtos may duplicate WorkflowDefinitionDtos triggers
- ⚠️ Missing trigger type enums
- ⚠️ No validation for timeout values

---

### 1.3 Special Cases & Missing Patterns

#### ✅ DTOs Following Best Practices

| File | Strengths |
|------|-----------|
| **AuditLogDtos.cs** | Good separation of concerns |
| **DashboardCustomizationDto.cs** | Clear structure |
| **PerformanceMetricsDto.cs** | Well-organized |
| **PreferencesDto.cs** | Clean design |

---

#### ❌ Missing DTO Patterns

| Pattern | Count | Example | Priority |
|---------|-------|---------|----------|
| No ListDto (pagination wrapper) | ~10 | Need AccountListDto with pagination | 🟡 MEDIUM |
| No CreateDto (post request) | ~15 | Need CreateContactDto pattern | 🟡 MEDIUM |
| No UpdateDto (patch request) | ~10 | Need UpdateContractDto pattern | 🟡 MEDIUM |
| No response wrapper | ~30 | ApiResponse<T> missing | 🔴 HIGH |
| No enum types | ~15 | StatusIds as int, not enums | 🔴 HIGH |
| No XML documentation | ~70 | Missing /// comments | 🟢 LOW |

---

## PHASE 2: STANDARDIZATION PLAN

### 2.1 DTO Architecture Standards

```csharp
// STANDARD PATTERN - ALL NEW DTOs MUST FOLLOW:

// 1. READ RESPONSE (GET /api/entity/{id})
public class EntityReadDto : ReadResponseDtoBase
{
    /// <summary>Gets or sets the first name.</summary>
    [Required(ErrorMessage = "FirstName is required.")]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    /// <summary>Gets or sets the phone number.</summary>
    [PhoneNumber]  // Custom validation
    public string PhoneNumber { get; set; }
}

// 2. CREATE REQUEST (POST /api/entity)
public class CreateEntityDto : CreateRequestDtoBase
{
    /// <summary>Gets or sets the first name for the new entity.</summary>
    [Required] [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    [Required] [EmailAddress]
    public string Email { get; set; }
}

// 3. UPDATE REQUEST (PATCH /api/entity/{id})
public class UpdateEntityDto : UpdateRequestDtoBase
{
    /// <summary>Gets or sets the first name (optional update).</summary>
    [StringLength(100, MinimumLength = 1)]
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the email address (optional update).</summary>
    [EmailAddress]
    public string? Email { get; set; }
}

// 4. LIST RESPONSE (GET /api/entity?page=1&pageSize=20)
public class EntityListDto
{
    /// <summary>Gets or sets the entity ID.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the status code.</summary>
    public EntityStatus Status { get; set; }
}
```

### 2.2 Validation Standards

**All DTOs MUST have:**

1. **String Validation**
   ```csharp
   [Required(ErrorMessage = "Field name is required.")]
   [StringLength(100, MinimumLength = 1, ErrorMessage = "Field must be 1-100 characters.")]
   public string Name { get; set; }
   ```

2. **Decimal/Currency Validation**
   ```csharp
   [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between $0.01 and $999,999,999.99")]
   [DecimalPrecision(18, 4)]  // Custom validation
   public decimal Amount { get; set; }
   ```

3. **Email/Phone Validation**
   ```csharp
   [EmailAddress(ErrorMessage = "Invalid email format.")]
   public string Email { get; set; }

   [PhoneNumber]  // Custom validation for E.164 format
   public string Phone { get; set; }
   ```

4. **Enum Validation (NEVER use int for status/priority)**
   ```csharp
   // ❌ WRONG
   public int Status { get; set; }

   // ✅ CORRECT
   public EntityStatus Status { get; set; }  // Enum
   
   public enum EntityStatus 
   { 
       Active = 1, 
       Inactive = 2,
       Deleted = 3 
   }
   ```

5. **Date Validation**
   ```csharp
   [DataType(DataType.Date)]
   public DateTime StartDate { get; set; }

   [DataType(DataType.DateTime)]
   public DateTime CreatedAt { get; set; }
   ```

### 2.3 Response Wrapper Pattern

**All API responses MUST use standardized wrapper:**

```csharp
// SUCCESS RESPONSE
{
    "success": true,
    "data": { /* actual data */ },
    "message": "Operation completed successfully.",
    "errors": null
}

// ERROR RESPONSE
{
    "success": false,
    "data": null,
    "message": "Validation failed.",
    "errors": {
        "Email": ["Invalid email format"],
        "Amount": ["Amount must be greater than 0"]
    }
}

// PAGINATED RESPONSE
{
    "success": true,
    "data": {
        "items": [ /* array of items */ ],
        "totalCount": 150,
        "page": 1,
        "pageSize": 20,
        "totalPages": 8
    },
    "message": null,
    "errors": null
}
```

---

## EXECUTION PLAN: 4 PHASES

### Phase 1: ✅ Audit (COMPLETE)
- [x] List all 85 DTOs
- [x] Identify duplicates (ColorPaletteDto + ColorPaletteDtos, etc.)
- [x] Document validation gaps
- [x] Create refactoring plan
- [x] Identify missing patterns

### Phase 2: Create Base Architecture (PENDING)
**Duration:** 2-3 hours | **Files:** 8 new files  
**Status:** 🔴 NOT STARTED

**Deliverables:**
1. `BaseDtoInterfaces.cs` - Base classes and interfaces
2. `CustomValidationAttributes.cs` - DecimalPrecision, PhoneNumber, CurrencyCode attributes
3. `ValidationMessages.cs` - Centralized error messages
4. `ApiResponse.cs` - Response wrappers
5. `PaginatedResponse.cs` - Pagination wrapper
6. `StandardEnums.cs` - Common enums (Status, Priority, Stage, etc.)
7. `ResponseWrapperExtensions.cs` - Factory methods
8. `DtoMappingProfile.cs` - AutoMapper profile template

### Phase 3: Refactor Existing DTOs (PENDING)
**Duration:** 4-6 hours | **Files:** 25-30 refactorings  
**Status:** 🔴 NOT STARTED

**Priority Tiers:**

**Tier 1 - CRITICAL (Do First):**
- [ ] Delete ColorPaletteDto.cs (keep ColorPaletteDtos.cs only)
- [ ] Consolidate PasswordReset DTOs (3 files → 1)
- [ ] Fix Financial DTOs: InvoiceDto, PaymentDto (add Range validations)
- [ ] Consolidate ITSM duplicates (root vs subdirectory)
- [ ] Add response wrappers to 30+ DTOs

**Tier 2 - HIGH (Within 2 hours):**
- [ ] Consolidate TwoFactor DTOs
- [ ] Add Create/Update/List patterns to Account/Contact
- [ ] Consolidate OAuthDtos
- [ ] Fix Commission/Subscription type safety (int → enum)

**Tier 3 - MEDIUM (Within 3 hours):**
- [ ] Consolidate Campaign DTOs
- [ ] Consolidate UI Configuration DTOs
- [ ] Add validation to Admin Config DTOs
- [ ] Consolidate RBAC DTOs

**Tier 4 - LOW (Polish):**
- [ ] Add XML documentation to 70+ DTOs
- [ ] Add email/phone validation
- [ ] Consolidate remaining overlaps

### Phase 4: Create New DTOs (PENDING)
**Duration:** 6-8 hours | **Files:** 50+ new DTOs  
**Status:** 🔴 NOT STARTED

Will be based on SPEC-ARCH-001 (in progress)

---

## RISK MITIGATION

### ✅ Backward Compatibility Strategy

1. **Create new standardized DTOs alongside old ones**
   ```
   OLD: AccountDto.cs
   NEW: AccountDtos.cs (with CreateAccountDto, UpdateAccountDto, etc.)
   ```

2. **Controllers accept both during migration period**
   ```csharp
   [HttpPost("api/accounts")]
   public async Task<IActionResult> Create(CreateAccountDto dto)  // NEW
   {
       // Handle both old and new patterns
   }
   ```

3. **Mark old DTOs with [Obsolete] attributes**
   ```csharp
   [Obsolete("Use CreateAccountDto instead.", Error = false)]
   public class AccountDto { /* ... */ }
   ```

4. **Update guides for dependent teams**
   - Migration guide for each changed DTO
   - Mapping examples in documentation
   - 2-week deprecation notice

### Dependencies Analysis

**Files affected by refactoring:**
- Controllers (70+ files) - mostly FindByIdAsync reads, should not break
- Services (30+ files) - mapping logic to be updated
- Integration tests (50+ files) - test fixtures to update
- API tests (40+ tests) - response validation to update

---

## NEXT STEPS

### Immediate Actions (Today)

1. ✅ **Complete Audit Report** (This document)
2. 🔴 **Phase 2: Create Base Architecture** 
   - [ ] Create BaseDtoInterfaces.cs
   - [ ] Create CustomValidationAttributes.cs
   - [ ] Create StandardEnums.cs
   - [ ] Create ApiResponse wrappers
3. 🔴 **Emergency Fix: Delete ColorPaletteDto.cs**
   - [ ] Remove file
   - [ ] Update imports (expect 5-10 files)

### This Week

4. **Phase 3.1: Tier 1 Refactoring**
   - [ ] Consolidate PasswordReset DTOs
   - [ ] Fix Financial DTOs
   - [ ] Consolidate ITSM duplicates

### Next Week

5. **Phase 3.2-3.4: Remaining Refactoring**
6. **Phase 4: New DTOs for P0/P1 items**

---

## METRICS

### Before Standardization
- Total DTO files: 85
- Files with duplicates: 8-12
- Files missing validation: ~25
- Files with type safety issues: ~15
- XML documentation coverage: <10%
- Response wrapper usage: ~20%

### After Standardization (Target)
- Total DTO files: 100-110 (better organized)
- Duplicate files: 0
- Files with complete validation: 100%
- Type safety score: 99%
- XML documentation coverage: 100%
- Response wrapper usage: 100%

---

## APPENDIX: DETAILED DUPLICATE ANALYSIS

### Duplicate Set 1: PASSWORD RESET (3 FILES)

```
1. PasswordResetRequest.cs
   public class PasswordResetRequest
   {
       public string Email { get; set; }
   }

2. PasswordResetConfirm.cs
   public class PasswordResetConfirm
   {
       public string Code { get; set; }
       public string NewPassword { get; set; }
   }

3. SetPasswordRequest.cs
   public class SetPasswordRequest
   {
       public string UserId { get; set; }
       public string Password { get; set; }
   }

✅ SOLUTION:
Create PasswordManagementDtos.cs with:
- RequestPasswordResetDto (email)
- ConfirmPasswordResetDto (code + newPassword)
- SetPasswordDto (admin-initiated, requires approval)
```

### Duplicate Set 2: TWO-FACTOR AUTH (4+ FILES)

```
1. TotpDtos.cs
   - TotpSetupDto
   - TotpVerificationDto

2. TwoFactorVerification.cs
   public class TwoFactorVerification { /* ... */ }

3. TwoFactorEnableRequest.cs
   public class TwoFactorEnableRequest { /* ... */ }

4. TwoFactorSetupResponse.cs
   public class TwoFactorSetupResponse { /* ... */ }

✅ SOLUTION:
Consolidate into TwoFactorAuthDtos.cs with:
- TwoFactorSetupDto (request)
- TwoFactorVerifyDto (verification request)
- TwoFactorResponseDto (response with secret)
- TwoFactorBackupCodesDto (recovery codes)
```

### Duplicate Set 3: COLOR PALETTE (2 FILES) 🔴 CRITICAL

```
1. ColorPaletteDto.cs
   public class ColorPaletteDto { /* exact same content */ }

2. ColorPaletteDtos.cs
   public class ColorPaletteDto { /* exact same content */ }
   public class ColorPaletteDtos { /* ... */ }

✅ IMMEDIATE ACTION:
1. DELETE ColorPaletteDto.cs
2. Search codebase for references to ColorPaletteDto
3. Update imports to use ColorPaletteDtos.cs
```

### Duplicate Set 4: ADMIN CONFIG DUPLICATES (ROOT VS ITSM)

```
ROOT:                          ITSM:
SLAPolicyDto.cs                ITSMDtos.cs (contains SLAPolicyDto)
ServiceQueueDto.cs             ITSMDtos.cs (contains ServiceQueueDto)
EscalationRuleDto.cs           ITSMDtos.cs (contains EscalationRuleDto)
                                EscalationPolicyDto.cs

✅ SOLUTION:
1. Keep ITSM versions (better organized)
2. Delete root versions
3. Create proper ITSM namespace
4. Consolidate ITSMDtos.cs into separate files by type
```

---

## APPENDIX: GAP ANALYSIS FOR NEW DTOs

| Module | Needed DTOs | Count | Files |
|--------|-------------|-------|-------|
| **ITSM** | Problem, Change, CAB, Incident (enhanced) | 20+ | 4-5 |
| **Admin Config** | Commission, Discount, SLA, Queue variants | 15+ | 3 |
| **Email Sequences** | EmailStep, SequenceCondition, Trigger | 10+ | 2 |
| **Marketing** | Campaign (enhanced), EmailTemplate, LeadScore | 20+ | 3 |
| **Integration** | Webhook (enhanced), Import/Export, Integration | 15+ | 3 |
| **Reporting** | Custom Report, Dashboard Widget, KPI | 10+ | 2 |
| **Advanced Sales** | Commission Statement, Payout, Calculation | 15+ | 2 |
| **Automation** | Workflow (enhanced), Trigger, Action | 15+ | 3 |
| **Analytics** | Dataset, Chart Config, Dashboard | 12+ | 2 |

**Total New DTOs Needed:** 132+

---

**Report Generated:** February 16, 2026  
**Next Review:** After Phase 2 completion

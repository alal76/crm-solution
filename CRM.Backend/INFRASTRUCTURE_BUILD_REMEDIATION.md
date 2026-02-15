# CRM Infrastructure Build Remediation Guide

**Objective:** Fix 119 compilation errors in CRM.Infrastructure to enable System Module test execution  
**Scope:** CRM.Infrastructure project only  
**Severity:** 🔴 CRITICAL - Blocks all downstream testing

---

## Error Category Breakdown

### Category 1: Missing Entity Properties (45 errors)

#### `User.LastLoginDate` Missing (8 errors)
**Affected Services:**
- AdminDashboardService
- PerformanceMonitoringService
- UICustomizationService
- RBACService (multiple references)

**Fix Required:**
Add property to `CRM.Core/Entities/User.cs`:
```csharp
public DateTime? LastLoginDate { get; set; }
```

OR if property is actually `LastLoginAt`:
```csharp
public DateTime? LastLoginAt { get; set; }
```

Then update all service references from `LastLoginDate` to `LastLoginAt` (or vice versa for consistency)

**Services to Update:**
- [ ] AdminDashboardService.cs
- [ ] PerformanceMonitoringService.cs  
- [ ] UICustomizationService.cs
- [ ] RBACService.cs

---

#### `SLAPolicy` Entity Properties Missing (6 errors)

**Properties Missing:**
- `CustomerSegmentsJson`
- `ProductsJson`
- `CaseTypesJson`
- `CustomerTiersJson`
- `MatchConditionsJson`
- `BusinessHours`

**Location:** `CRM.Core/Entities/ITSM/SLAPolicy.cs`

**Action Required:**
Either add these properties to the entity OR comment out/remove references in services using them.

**Affected Locations:**
- `SLAService.cs` (multiple references)
- `SLAPolicyConfigurationService.cs`

---

#### `EscalationRule` Entity Properties Missing (5 errors)

**Properties Missing:**
- `EmailRecipientsJson`
- `WebhookUrl`
- `ActionConfigJson`
- `SLAPolicyId` (navigation property)
- `TriggerMetric`

**Location:** `CRM.Core/Entities/ITSM/EscalationRule.cs`

**Action Required:**
Add missing properties OR update services that reference them

**Affected Locations:**
- `EscalationRuleService.cs` (multiple references)

---

#### `Contact.IsDeleted` Missing (3 errors)

**Property Missing:**
- `IsDeleted` on Contact entity (soft-delete support)

**Location:** `CRM.Core/Entities/Contact.cs`

**Fix:**
```csharp
public bool IsDeleted { get; set; }
```

**Affected Services:**
- ContactService.cs
- Likely multiple data access patterns

---

#### `Invoice.Amount` Missing (4 errors)

**Property Missing:**
- `Amount` on Invoice entity

**Location:** `CRM.Core/Entities/Invoice.cs`

**Fix:**
```csharp
public decimal Amount { get; set; }
```

**Affected Services:**
- InvoiceService.cs
- BillingService.cs

---

#### `ServiceRequest` Properties Missing (4 errors)

**Properties Missing:**
- `DueDate`
- `StatusCode`

**Location:** `CRM.Core/Entities/ITSM/ServiceRequest.cs`

**Fix:**
```csharp
public DateTime? DueDate { get; set; }
public string? StatusCode { get; set; }
```

**Affected Services:**
- ServiceRequestService.cs
- SLACalculationService.cs

---

#### `ServiceQueue.DisplayOrder` Missing (3 errors)

**Property Missing:**
- `DisplayOrder` on ServiceQueue entity

**Location:** `CRM.Core/Entities/ITSM/ServiceQueue.cs`

**Fix:**
```csharp
public int DisplayOrder { get; set; }
```

**Affected Services:**
- ServiceQueueService.cs

---

#### `UserApprovalRequest.IsApproved` Missing (2 errors)

**Property Missing:**
- `IsApproved` on UserApprovalRequest entity

**Location:** `CRM.Core/Entities/UserApprovalRequest.cs`

**Fix:**
```csharp
public bool IsApproved { get; set; }
```

**Affected Services:**
- UserApprovalService.cs

---

#### `ModuleStatusDto` Property Mismatches (15+ errors)

**Affected Locations:**
- SystemSettingsService.cs (multiple property assignments)
- AdminDashboardService.cs
- HealthCheckService.cs

**Action Required:**
Review `CRM.Core/Dtos/ModuleStatusDto.cs` and ensure all properties used in services exist:
- [ ] Module (string)
- [ ] IsEnabled (bool)
- [ ] Status (enum or string)
- [ ] ErrorMessage (string?)
- [ ] HealthStatus (enum or string)
- [ ] LastCheckedAt (DateTime)
- [ ] ConfigurationStatus (enum or string)

**Verify All Property Names Match** in:
- [ ] SystemSettingsService.cs
- [ ] AdminDashboardService.cs
- [ ] HealthCheckService.cs

---

### Category 2: Type Ambiguities (8 errors)

#### Ambiguous `SLAPolicy` Reference
```
error CS0104: 'SLAPolicy' is an ambiguous reference between 
'CRM.Core.Entities.SLAPolicy' and 'CRM.Core.Entities.ITSM.SLAPolicy'
```

**Root Cause:** SLAPolicy defined in TWO locations

**Fix:**
1. Check if `CRM.Core/Entities/SLAPolicy.cs` exists (should not)
2. If it exists, delete it OR rename to distinguish purpose
3. Update all using statements to be fully qualified:
   ```csharp
   using CRM.Core.Entities.ITSM; // Then use SLAPolicy
   // OR
   using CRM.Core.Entities.ITSM.SLAPolicy; // Fully qualified
   ```

**Affected Files:** Any file importing SLAPolicy without namespace qualification

---

#### Ambiguous `EscalationRule` Reference
```
error CS0104: 'EscalationRule' is an ambiguous reference between 
'CRM.Core.Entities.EscalationRule' and 'CRM.Core.Entities.ITSM.EscalationRule'
```

**Root Cause:** Same as SLAPolicy - defined in TWO locations

**Fix:** Same as SLAPolicy remediation above

---

#### Ambiguous `UserRole` Reference
```
error CS0104: 'UserRole' is an ambiguous reference
```

**Root Cause:** UserRole defined as both Entity and Enum

**Fix:**
1. Determine intended use (is it an entity or enum?)
2. Delete or rename the unused version
3. Or use fully qualified names throughout

---

### Category 3: Missing DbContext Properties (4 errors)

#### `ITSMSLAInstances` Missing from DbContext
```
error CS1061: 'ICrmDbContext' does not contain a definition for 'ITSMSLAInstances'
```

**Location:** `CRM.Core/Interfaces/ICrmDbContext.cs` and `CRM.Infrastructure/Data/CrmDbContext.cs`

**Fix:**
Add DbSet property to both files:
```csharp
DbSet<SLAInstance> ITSMSLAInstances { get; set; }
```

**Verify in:**
- [ ] ICrmDbContext.cs interface
- [ ] CrmDbContext.cs implementation

---

#### `UserRoles` Missing from DbContext
```
error CS1061: 'ICrmDbContext' does not contain a definition for 'UserRoles'
```

**Location:** `CRM.Core/Interfaces/ICrmDbContext.cs` and `CRM.Infrastructure/Data/CrmDbContext.cs`

**Fix:**
```csharp
DbSet<UserRole> UserRoles { get; set; }
```

**Verify that UserRole is the correct entity type and not being confused with role strings**

---

### Category 4: Repository Method Signature Mismatches (9 errors)

#### Pattern: `AddAsync(entity, cancellationToken)` - Wrong Signature
```
error CS1501: No overload for method 'AddAsync' takes 2 arguments
```

**Affected Services:**
- CommissionRuleService.cs
- DiscountRuleService.cs
- SubscriptionService.cs
- BillingService.cs

**Root Cause:** Services calling `AddAsync(entity, cancellationToken)` but repository method is `AddAsync(entity)` or vice versa

**Fix Options:**
1. **If using EF Core DbSet directly:**
   ```csharp
   await dbContext.CommissionRules.AddAsync(rule);
   await dbContext.SaveChangesAsync(cancellationToken);
   ```

2. **If using Repository pattern:**
   ```csharp
   // Repository should be:
   public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
   ```

**Audit Services:**
- [ ] CommissionRuleService.cs
- [ ] DiscountRuleService.cs
- [ ] SubscriptionService.cs
- [ ] BillingService.cs

---

#### Pattern: `GetByIdAsync(id, id2)` - Wrong Overload
```
error CS1501: No overload for method 'GetByIdAsync' takes 2 arguments
```

**Affected Services:**
- ProrateCalculator.cs
- RecurringBillingEngine.cs
- SubscriptionMetricsAggregator.cs

**Root Cause:** Calling `GetByIdAsync(id1, id2)` but repository only has `GetByIdAsync(id)`

**Fix:** 
Either:
1. Repository supports composite key: Add overload accepting 2 parameters
2. Service should only pass single ID
3. Use different method for composite lookups (e.g., `GetByCompositeKeyAsync`)

---

#### Pattern: `GetAllAsync()` - Wrong Parameter Count
```
error CS1501: No overload for method 'GetAllAsync' takes 1 arguments
```

**Likely Issue:** Services calling `GetAllAsync(cancellationToken)` but repository expects 0 parameters

**Fix:**
- Repository: `public Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)`
- OR remove CancellationToken from service call if not used

---

### Category 5: Type System Errors (18 errors)

#### Decimal vs Nullable Type Mismatch
```
error CS0019: Operator '??' cannot be applied to operands of type 'decimal' and 'int'
```

**Affected Services:**
- SubscriptionMetricsAggregator.cs
- ProrateCalculator.cs
- RecurringBillingEngine.cs

**Example Issue:**
```csharp
decimal amount = someDictionary["amount"] ?? 0; // ERROR: wrong type on right
```

**Fix:**
```csharp
decimal amount = (decimal)(someDictionary["amount"] ?? 0);
// OR
decimal amount = someDictionary.TryGetValue("amount", out var val) ? (decimal)val : 0m;
```

**Audit These Files:**
- [ ] SubscriptionMetricsAggregator.cs
- [ ] ProrateCalculator.cs
- [ ] RecurringBillingEngine.cs

---

#### Type Parameter Constraint Error
```
error CS0452: The type 'UserRole' must be a reference type in order to use it as parameter 'TEntity'
```

**Root Cause:** Generic constraint `where TEntity : class` but UserRole is a struct or value type

**Fix:**
1. Check if UserRole should be a class (reference type)
2. If it's an enum/struct, add constraint `where TEntity : struct` or remove constraint
3. Or convert struct to class

---

#### Implicit Conversion Error
```
error CS0266: Cannot implicitly convert type 'System.Collections.Generic.IDictionary<string, CRM.Core.Dtos.ModuleStatusDto>' 
to 'System.Collections.Generic.Dictionary<string, CRM.Core.Dtos.ModuleStatusDto>'
```

**Affected Locations:**
- AdminDashboardService.cs (likely in GetModuleStatus method)

**Fix:**
Change return type or cast explicitly:
```csharp
// Option 1: Keep as IDictionary
IDictionary<string, ModuleStatusDto> moduleStatus = new Dictionary<string, ModuleStatusDto>();

// Option 2: Explicitly cast
Dictionary<string, ModuleStatusDto> moduleStatus = 
    (systemSettings.ModuleStatus as Dictionary<string, ModuleStatusDto>) 
    ?? new Dictionary<string, ModuleStatusDto>();

// Option 3: Use .ToDictionary()
var moduleStatus = systemSettings.ModuleStatus.ToDictionary(x => x.Key, x => x.Value);
```

---

#### Enum/Integer Type Mismatch
```
error CS0019: Operator '!=' cannot be applied to operands of type 'int' and 'ProviderHealthStatus'
```

**Fix:**
Compare same types:
```csharp
// WRONG
if (status != (int)ProviderHealthStatus.Unknown) { }

// CORRECT
if (status != ProviderHealthStatus.Unknown) { }
// OR
if (statusInt != (int)ProviderHealthStatus.Unknown) { }
```

---

### Category 6: Context Usage Issues (6 errors)

#### Missing Using Statements
Various errors may stem from missing `using` directives

**Verify All These Exist:**
- [ ] `using CRM.Core.Entities;`
- [ ] `using CRM.Core.Entities.ITSM;`
- [ ] `using CRM.Core.Dtos;`
- [ ] `using CRM.Core.Interfaces;`
- [ ] `using CRM.Infrastructure.Data;`

---

## Remediation Checklist

### Phase 1: Entity Properties (2 hours)
- [ ] Add `User.LastLoginDate` (or `LastLoginAt`)
- [ ] Add `Contact.IsDeleted`
- [ ] Add `Invoice.Amount`
- [ ] Add `ServiceRequest.DueDate` and `StatusCode`
- [ ] Add `ServiceQueue.DisplayOrder`
- [ ] Add `UserApprovalRequest.IsApproved`
- [ ] Verify `SLAPolicy` properties exist
- [ ] Verify `EscalationRule` properties exist
- [ ] Verify `ModuleStatusDto` property names match usage

### Phase 2: Type Ambiguities (30 minutes)
- [ ] Remove duplicate entity definitions (SLAPolicy, EscalationRule, UserRole)
- [ ] Add fully qualified using statements where needed

### Phase 3: DbContext (30 minutes)
- [ ] Add `ITSMSLAInstances` to ICrmDbContext and CrmDbContext
- [ ] Add `UserRoles` to ICrmDbContext and CrmDbContext
- [ ] Add migrations if using EF Core

### Phase 4: Service Method Signatures (1 hour)
- [ ] Fix AddAsync signature mismatches
- [ ] Fix GetByIdAsync signature mismatches
- [ ] Fix GetAllAsync signature mismatches
- [ ] Test: `dotnet build CRM.Infrastructure/CRM.Infrastructure.csproj`

### Phase 5: Type System Fixes (1 hour)
- [ ] Fix decimal/nullable coercion issues
- [ ] Fix generic type constraint violations
- [ ] Fix implicit conversion errors
- [ ] Fix enum/integer mismatches

### Phase 6: Final Verification (30 minutes)
- [ ] `dotnet build` entire solution
- [ ] `dotnet test tests/CRM.SystemModule.Tests/`
- [ ] Verify 77 tests are reached (at minimum)

---

## Estimated Timeline

| Phase | Duration | Status |
|-------|----------|--------|
| Phase 1: Entity Properties | 120 min | ⏳ Pending |
| Phase 2: Type Ambiguities | 30 min | ⏳ Pending |
| Phase 3: DbContext | 30 min | ⏳ Pending |
| Phase 4: Service Signatures | 60 min | ⏳ Pending |
| Phase 5: Type Systems | 60 min | ⏳ Pending |
| Phase 6: Verification | 30 min | ⏳ Pending |
| **TOTAL** | **~5 hours** | |

---

## Quick Command to Validate Progress

```bash
cd "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend"

# Count current errors
dotnet build CRM.Infrastructure/CRM.Infrastructure.csproj 2>&1 | grep "error CS"  | wc -l

# Build clean confirmation
dotnet build CRM.Infrastructure/CRM.Infrastructure.csproj --no-restore
```

---

## Success Criteria

✅ Infrastructure builds cleanly
```
Build succeeded. 0 Failed, 0 Passed in X seconds
```

✅ System Module tests can execute
```bash
dotnet test tests/CRM.SystemModule.Tests/ --verbosity normal
```

✅ Target:
- 77 tests discovered
- 77 tests passed  
- 0 tests failed

---

## Post-Remediation Commands

Once infrastructure is fixed:

```bash
# Run the tests
cd "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend"
dotnet test tests/CRM.SystemModule.Tests/CRM.SystemModule.Tests.csproj --verbosity detailed

# Generate coverage
dotnet test tests/CRM.SystemModule.Tests/ --collect:"XPlat Code Coverage"

# View specific test results
dotnet test tests/CRM.SystemModule.Tests/CRM.SystemModule.Tests.csproj -v detailed --logger:"console;verbosity=normal"
```

---

## Support

If you encounter errors not in this guide:
1. Note the exact error code (e.g., CS1061)
2. Check which service/file is generating the error
3. Search for that error code in Category sections above
4. If not found, examine the error message for patterns

**Key Error Code Reference:**
- CS0104 = Ambiguous type reference
- CS0452 = Generic type constraint violation  
- CS1061 = Missing member
- CS1501 = No matching method overload
- CS0266 = Implicit conversion error
- CS0019 = Operator not supported for types
- CS0117 = Type doesn't contain member

---

**Generated:** February 15, 2026  
**Status:** READY FOR REMEDIATION

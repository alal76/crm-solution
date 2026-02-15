# ITSM Disabled Services Root Cause Analysis

**Last Updated:** February 15, 2026  
**Analysis Scope:** 31 disabled ITSM services in `CRM.Backend/src/CRM.Infrastructure/Services/ITSM/`

---

## Executive Summary

All 31 ITSM services are disabled due to a **combination of missing abstract infrastructure** and **design pattern inconsistency**. The core issue is NOT missing individual DTOs, entities, or interfaces—those are mostly defined. Rather:

1. **IDbContextResolver Pattern Mismatch** - Services inject `IDbContextResolver` (multi-tenancy resolver) instead of `ICrmDbContext` directly
2. **Incomplete Helper Services** - `IBusinessHoursCalculator` has inline interface definition without proper architecture
3. **Circular Dependency Chains** - Services reference each other without clear dependency resolution
4. **Missing Hosted Service Infrastructure** - Background services reference undefined utility methods
5. **Inconsistent DTO Naming** - Mixed usage of `CRM.Core.DTOs.ITSM` vs `CRM.Core.Dtos.ITSM` namespaces

---

## 📊 Service Classification Summary

| Category | Count | Size Range | Status |
|----------|-------|-----------|--------|
| **Core ITSM Services** | 8 | 200-500 lines | All disabled |
| **Advanced/Analytics** | 6 | 700-900 lines | All disabled (requires ITSM_ADVANCED flag) |
| **Background/Hosted Services** | 4 | 170-530 lines | All disabled |
| **Admin/Integration Services** | 9 | 200-800 lines | All disabled |
| **Supporting Services** | 4 | 200-300 lines | All disabled |

**Total Lines of Code (ITSM services):** ~15,000+ lines

---

## 🔴 Root Causes - Categorized

### Category 1: Infrastructure Pattern Mismatch (9 services affected)

**Problem:** Services use `IDbContextResolver` pattern instead of injecting `ICrmDbContext` directly.

**Services Affected:**
- BusinessHoursCalculator.cs.disabled
- IncidentService.cs.disabled  
- ProblemService.cs.disabled
- CMDBService.cs.disabled
- ChangeManagementService.cs.disabled
- KnowledgeManagementService.cs.disabled
- ServiceCatalogService.cs.disabled
- SLAEnforcementHostedService.cs.disabled
- AutoCloseHostedService.cs.disabled

**Details:**
```csharp
// CURRENT (PATTERN MISMATCH):
private readonly IDbContextResolver _dbContextResolver;
public IncidentService(IDbContextResolver dbContextResolver, ...)
{
    var context = _dbContextResolver.ResolveContext();  // ❌ Not injectable
}

// SHOULD BE:
private readonly ICrmDbContext _context;
public IncidentService(ICrmDbContext context, ...)
{
    // Use _context directly
}
```

**Root Cause:** `IDbContextResolver` is defined in `CRM.Infrastructure.Data.DynamicDbContextResolver.cs`, not in `Core.Interfaces`. This breaks the dependency injection pattern and makes services invalid for standard DI registration.

**Fix Approach:**
1. Remove `IDbContextResolver` dependency injection pattern
2. Inject `ICrmDbContext` directly (standard ASP.NET pattern)
3. Update all `_dbContextResolver.ResolveContext()` calls to `_context` references
4. If multi-tenancy context switching is needed, create a proper `ITenantContext` interface in Core

---

### Category 2: Circular Service Dependencies (3 services)

**Problem:** Services depend on each other, creating circular reference chains.

**Services Affected:**
- IncidentService → ISLAService → IBusinessHoursCalculator
- ChangeManagementService → ICMDBService (bidirectional)
- SLAService → IBusinessHoursCalculator → ISLAService

**Dependency Chain:**
```
IncidentService
  └─ ISLAService
      └─ IBusinessHoursCalculator
          └─ ICrmDbContext

ChangeManagementService
  └─ ICMDBService
      └─ ICrmDbContext

SLAService
  └─ IBusinessHoursCalculator
```

**Fix Approach:**
1. Inject `IBusinessHoursCalculator` as an optional dependency (nullable `IBusinessHoursCalculator?`)
2. Use service locator pattern for late-binding if absolutely necessary (not recommended)
3. Refactor to eliminate circular dependency by extracting shared logic into separate utility class

---

### Category 3: Hosted Service Missing Utilities (4 services)

**Problem:** Background services call undefined helper methods without implementation context.

**Services Affected:**
- SLAEnforcementHostedService.cs.disabled
- AutoCloseHostedService.cs.disabled
- EscalationHostedService.cs (currently ENABLED but marked ITSM_ADVANCED)
- MonitoringIntegrationService.cs.disabled

**Examples of Missing Methods:**
```csharp
// In SLAEnforcementHostedService:
await escalationRuleService.TriggerEscalationAsync(...);  // ✅ Exists

// In AutoCloseHostedService:
await AutoCloseIncidentsAsync(context, now);  // ❌ Method referenced but not fully implemented
await AutoCloseServiceRequestsAsync(context, now);  // ❌ Incomplete

// In EscalationHostedService:
private List<EscalationRule> GetEscalationRules() { ... }  // ❌ Not implemented
```

**Fix Approach:**
1. Complete method implementations in hosted services
2. Create `IHostedServiceHelper` interface with utility methods
3. Implement helper service with actual business logic

---

### Category 4: DTO Namespace Inconsistency (2 services)

**Problem:** Mixed usage of DTO namespaces causes type mismatch errors.

**Services Affected:**
- EscalationRuleAdminService.cs.disabled - uses `CRM.Core.Dtos.ITSM` (wrong)
- EscalationPolicyService.cs.disabled - uses `CRM.Core.Dtos.ITSM` (wrong)

**Details:**
```csharp
// Wrong namespace (missing 'T' in Dtos):
using CRM.Core.Dtos.ITSM;  // ❌ Should be DTOs

// Correct:
using CRM.Core.DTOs.ITSM;  // ✅ Defined in code
```

**Root Cause:** Copy-paste error during code generation. Namespace should be `CRM.Core.DTOs.ITSM` (plural DTOs).

**Fix Approach:**
1. Global search/replace `CRM.Core.Dtos.ITSM` → `CRM.Core.DTOs.ITSM`
2. Update using statements in all affected services
3. Verify DTOs are importable after namespace correction

---

### Category 5: Missing or Incomplete Entity Relationships (2 services)

**Problem:** Services reference entities with incomplete relationship definitions.

**Services Affected:**
- ImpactAnalysisService.cs.disabled - references undefined entity properties
- DiscoveryService.cs.disabled - references undefined CMDB reconciliation structures

**Example:**
```csharp
// In ImpactAnalysisService:
public class IncidentImpactAnalysis
{
    public List<AffectedCI> AffectedCIs { get; set; }  // Nested type defined locally
    public List<PotentialRootCause> PotentialRootCauses { get; set; }  // Not in entity
}

// In DiscoveryService:
public class DiscoveryScanResult  // Local definition, not in DB entities
{
    // Incomplete implementation
}
```

**Fix Approach:**
1. Create entity models for `AffectedCI`, `PotentialRootCause`, `DiscoveryScanResult`
2. Add DbSets to `ICrmDbContext`
3. Create database migration for new tables
4. Update services to use EF entities instead of local classes

---

### Category 6: Interface Inconsistency with Service Implementations (3 services)

**Problem:** Service implementations don't match interface signatures exactly.

**Services Affected:**
- SLAService.cs.disabled - `StartSLAAsync` returns Task (interface expects no return)
- AssignmentRulesEngine.cs.disabled - Locally defines `IAssignmentRulesEngine` interface
- ArticleRecommendationService.cs.disabled - Locally defines `IArticleRecommendationService` interface

**Example:**
```csharp
// Interface definition (in IITSMServices.cs):
Task StartSLAAsync(int targetId, SLATargetType targetType, int priority);

// Implementation (in SLAService.cs):
async Task ISLAService.StartSLAAsync(...)
{
    await StartSLAInternalAsync(...);  // Returns Task<SLAInstanceDto?>
}
```

**Fix Approach:**
1. Move locally-defined interfaces to `CRM.Core.Interfaces.ITSM/`
2. Create separate files: `IAssignmentRulesEngine.cs`, `IArticleRecommendationService.cs`
3. Ensure implementation method signatures exactly match interface definitions
4. Remove local interface definitions from service files

---

## 📋 Complete Service-by-Service Analysis

### **CORE ITSM SERVICES (must be fixed first)**

#### 1. BusinessHoursCalculator.cs.disabled
- **Lines:** 537 | **Status:** Core dependency
- **Root Cause:** IDbContextResolver pattern mismatch
- **Dependencies:** IDbContextResolver, ICrmDbContext
- **Interfaces:** IBusinessHoursCalculator (defined inline) ✅
- **DTOs:** None needed
- **Entities:** None directly referenced
- **Fix Priority:** 🔴 HIGH (blocker for SLAService, IncidentService)
- **Effort:** Medium (refactor DI pattern, extract interface)

#### 2. IncidentService.cs.disabled
- **Lines:** 431 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern + circular dependency on ISLAService
- **Dependencies:** IDbContextResolver, ISLAService, ILogger
- **Interfaces Needed:** IIncidentService ✅ (defined in IITSMServices.cs)
- **DTOs Needed:**
  - IncidentDto ✅
  - CreateIncidentDto ✅
  - UpdateIncidentDto ✅
  - IncidentFilterDto ✅
  - ResolveIncidentDto ✅
- **Entities Needed:**
  - Incident ✅
  - IncidentComment ✅
  - User ✅
  - UserGroup ✅
  - ServiceRequestCategory ✅
  - ServiceRequestSubcategory ✅
- **Fix Priority:** 🔴 HIGH (core service for ITSM module)
- **Effort:** Medium (refactor DI, verify methods exist)

#### 3. ProblemService.cs.disabled
- **Lines:** 291 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern mismatch
- **Dependencies:** IDbContextResolver, ILogger
- **Interfaces Needed:** IProblemService ✅
- **DTOs Needed:**
  - ProblemDto ✅
  - CreateProblemDto ✅
  - UpdateProblemDto ✅
  - ProblemFilterDto ✅
- **Entities Needed:**
  - Problem ✅
  - ProblemComment ✅
  - ProblemIncident ✅ (junction table)
  - Incident ✅
- **Fix Priority:** 🔴 HIGH
- **Effort:** Medium

#### 4. CMDBService.cs.disabled
- **Lines:** 255 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern mismatch
- **Dependencies:** IDbContextResolver, ILogger
- **Interfaces Needed:** ICMDBService ✅
- **DTOs Needed:**
  - ConfigurationItemDto ✅
  - CreateCIDto ✅
- **Entities Needed:**
  - ConfigurationItem ✅
  - User ✅
  - CIRelationship ❌ (may need creation)
- **Fix Priority:** 🔴 HIGH (referenced by ChangeManagementService, ImpactAnalysisService)
- **Effort:** Medium

#### 5. ChangeManagementService.cs.disabled
- **Lines:** 426 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern + ICMDBService dependency
- **Dependencies:** IDbContextResolver, ICMDBService, ILogger
- **Interfaces Needed:** IChangeManagementService ✅
- **DTOs Needed:**
  - ChangeDto ✅
  - CreateChangeDto ✅
  - UpdateChangeDto ✅
  - ChangeFilterDto ✅
  - BlackoutPeriodInfo ✅ (defined in interface)
  - CreateBlackoutPeriodInfo ✅
- **Entities Needed:**
  - Change ✅
  - ChangeComment ✅
  - User ✅
  - ChangeImpactedCI ❌ (may need creation)
- **Fix Priority:** 🔴 HIGH
- **Effort:** Medium-High (complex filtering, relationship management)

#### 6. KnowledgeManagementService.cs.disabled
- **Lines:** 333 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern mismatch
- **Dependencies:** IDbContextResolver, ILogger
- **Interfaces Needed:** IKnowledgeManagementService ✅
- **DTOs Needed:**
  - KnowledgeArticleDto ✅
  - CreateKnowledgeArticleDto ✅
- **Entities Needed:**
  - KnowledgeArticle ✅
  - User ✅
  - ServiceRequestCategory ✅
- **Fix Priority:** 🟡 MEDIUM (non-critical path)
- **Effort:** Medium

#### 7. ServiceCatalogService.cs.disabled
- **Lines:** 207 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern mismatch
- **Dependencies:** IDbContextResolver, ILogger
- **Interfaces Needed:** IServiceCatalogService ✅
- **DTOs Needed:**
  - CatalogItemDto ✅
  - CreateCatalogRequestDto ✅
  - CatalogCategoryInfo ✅ (defined in interface)
  - CreateCatalogRequestForOthersDto ✅
- **Entities Needed:**
  - CatalogItem ✅
  - CatalogRequest ✅
  - CatalogRequestComment ✅
  - User ✅
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** Medium

#### 8. SLAService.cs.disabled
- **Lines:** 503 | **Status:** Core service
- **Root Cause:** IDbContextResolver pattern + IBusinessHoursCalculator circular dependency
- **Dependencies:** IDbContextResolver, IBusinessHoursCalculator, ILogger
- **Interfaces Needed:** ISLAService ✅
- **DTOs Needed:**
  - SLAPolicyDto ✅
  - SLAInstanceDto ✅
  - SLADashboardInfo ✅ (defined in interface)
  - SLAMetricsInfo ✅ (defined in interface)
- **Entities Needed:**
  - SLAPolicy ✅
  - SLAInstance ❌ (referenced but may not exist)
  - Incident ✅
  - Problem ✅
  - Change ✅
  - ServiceRequest ✅
- **Fix Priority:** 🔴 HIGH (blocker for incident escalations)
- **Effort:** Medium-High (complex time calculations)
- **Note:** Check if `ITSMSLAInstances` DbSet exists in ICrmDbContext

---

### **ADVANCED/ANALYTICS SERVICES (700+ lines, ITSM_ADVANCED flag)**

#### 9. ImpactAnalysisService.cs.disabled
- **Lines:** 866 | **Status:** Advanced feature
- **Root Cause:** Incomplete entity model for impact analysis data
- **Interfaces Needed:** IImpactAnalysisService (defined inline) ❌
- **Supporting Types:** 
  - IncidentImpactAnalysis (local)
  - AffectedCI (local)
  - AffectedService (local)
  - BusinessImpactScore (local)
  - PotentialRootCause (local)
  - All need to be → entities or VMs
- **Fix Priority:** 🟢 LOW (optional feature)
- **Effort:** High (new entity models + migrations)

#### 10. DiscoveryService.cs.disabled
- **Lines:** 738 | **Status:** Advanced feature
- **Root Cause:** Complex CMDB discovery model not fully defined
- **Interfaces Needed:** IDiscoveryService (defined inline) ❌
- **Supporting Types:** DiscoveryScanRequest, DiscoveryScanResult, etc. (all local)
- **Fix Priority:** 🟢 LOW (optional CMDB auto-discovery)
- **Effort:** High

#### 11. ArticleRecommendationService.cs.disabled
- **Lines:** 575 | **Status:** Advanced feature
- **Root Cause:** AI/ML recommendation model not implemented
- **Interfaces Needed:** IArticleRecommendationService (defined inline) ❌
- **Dependencies:** Would need access to Semantic Kernel or similar
- **Fix Priority:** 🟢 LOW (optional ML feature)
- **Effort:** Very High

#### 12. CatalogApprovalService.cs.disabled
- **Lines:** 726 | **Status:** Advanced workflow
- **Root Cause:** Approval workflow entity model incomplete
- **Interfaces Needed:** ICatalogApprovalService (defined inline) ❌
- **Supporting Types:** ApprovalWorkflow, ApprovalStage, etc. (local)
- **Fix Priority:** 🟡 MEDIUM (quality of life feature)
- **Effort:** High

#### 13. CatalogFulfillmentService.cs.disabled
- **Lines:** 500+ | **Status:** Advanced service
- **Root Cause:** Fulfillment workflow integration incomplete
- **Interfaces Needed:** ICatalogFulfillmentService (defined inline) ❌
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** High

#### 14. CABWorkflowService.cs.disabled
- **Lines:** 400+ | **Status:** Advanced workflow (CAB = Change Advisory Board)
- **Root Cause:** CAB review entity model not defined
- **Interfaces Needed:** ICABWorkflowService (defined inline) ❌
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** High

---

### **BACKGROUND/HOSTED SERVICES**

#### 15. SLAEnforcementHostedService.cs.disabled
- **Lines:** 170 | **Status:** Background service
- **Root Cause:** Incomplete method implementations
- **Dependencies:** ICrmDbContext, IEscalationRuleService
- **Issues:**
  - References `SLAStatus` enum (lives in KnowledgeBase, not ITSM)
  - References `SLAInstance` entity (need to verify DbSet exists)
  - Incomplete `CheckSLABreachesAsync` method
- **Fix Priority:** 🔴 HIGH (enables SLA enforcement)
- **Effort:** Medium

#### 16. AutoCloseHostedService.cs.disabled
- **Lines:** 293 | **Status:** Background service
- **Root Cause:** Incomplete method implementations
- **Dependencies:** IDbContextResolver, ILogger
- **Issues:**
  - Has multiple async methods that are partial implementations
  - References IncidentComment, ServiceRequest (need entity verification)
- **Fix Priority:** 🟡 MEDIUM (auto-close feature)
- **Effort:** Medium

#### 17. EscalationHostedService.cs (ENABLED)
- **Lines:** 531 | **Status:** ENABLED but marked ITSM_ADVANCED
- **Root Cause:** Uses IDbContextResolver pattern
- **Issue:** Currently ENABLED but should probably be disabled until pattern is refactored
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** Low (already implemented, just needs DI refactor)

#### 18. MonitoringIntegrationService.cs.disabled
- **Lines:** 400+ | **Status:** Advanced integration
- **Root Cause:** Monitoring platform integration incomplete
- **Interfaces Needed:** IMonitoringIntegrationService (defined in Interfaces/ITSM) ✅
- **Fix Priority:** 🟢 LOW (optional monitoring integration)
- **Effort:** High (external API integration)

---

### **ADMIN/INTEGRATION SERVICES**

#### 19. EscalationRuleAdminService.cs.disabled
- **Lines:** 201 | **Status:** Admin service
- **Root Cause:** DTO namespace typo (`Dtos` instead of `DTOs`)
- **Issues:**
  - `using CRM.Core.Dtos.ITSM` ❌ (should be DTOs)
  - Uses `IRepository<T>` pattern (should verify DI registration)
  - Uses non-standard `ICrmDbContext` injection
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** Low (just namespace fix)

#### 20. EscalationPolicyService.cs.disabled
- **Lines:** 453 | **Status:** Admin service  
- **Root Cause:** DTO namespace typo (`Dtos` instead of `DTOs`)
- **Issues:**
  - `using CRM.Core.Dtos.ITSM` ❌
  - References `EscalationPolicy` and `EscalationLevel` entities
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** Low

#### 21. SLAPolicyAdminService.cs.disabled
- **Lines:** 300+ | **Status:** Admin service
- **Root Cause:** Similar to other admin services
- **Interfaces Needed:** ISLAPolicyAdminService ✅ (exists in Interfaces/ITSM)
- **Fix Priority:** 🟡 MEDIUM
- **Effort:** Medium

#### 22. EscalationRuleAdminService.cs.disabled (duplicate entry 19)
- See above

#### 23. EmailToTicketService.cs.disabled
- **Lines:** 400+ | **Status:** Integration service
- **Root Cause:** Email parsing + incident creation integration incomplete
- **Interfaces Needed:** IEmailToTicketService ✅
- **Fix Priority:** 🟢 LOW (nice-to-have integration)
- **Effort:** High (email processing logic)

#### 24-27. Other Integration Services
- AssignmentRulesEngine.cs.disabled
- KCSWorkflowService.cs.disabled
- CICDIntegrationService.cs.disabled
- SelfServiceChatbotService.cs.disabled

All follow similar pattern: locally-defined interfaces, incomplete integration logic.

---

### **SUPPORTING SERVICES**

#### 28-31. Asset/Webhook Services
- AssetLifecycleService.cs.disabled - Asset lifecycle management
- ChangeCalendarService.cs.disabled - Blackout period calendar
- ChangeImpactService.cs.disabled - Impact analysis for changes
- WebhookNotificationService.cs.disabled - Webhook integration
- ITSMDashboardService.cs.disabled - Dashboard metrics
- SelfServiceChatbotService.cs.disabled - Customer-facing chatbot

These are mostly missing integration points or incomplete specific features.

---

## 📝 Summary Table: Missing Components

### Missing Interfaces (need to move from local to Core/Interfaces/ITSM/)

| Interface | Location | Status |
|-----------|----------|--------|
| IBusinessHoursCalculator | BusinessHoursCalculator.cs | ✅ Defined inline, needs extraction |
| IAssignmentRulesEngine | AssignmentRulesEngine.cs | ❌ Defined inline only |
| IArticleRecommendationService | ArticleRecommendationService.cs | ❌ Defined inline only |
| IImpactAnalysisService | ImpactAnalysisService.cs | ❌ Defined inline only |
| IDiscoveryService | DiscoveryService.cs | ❌ Defined inline only |
| ICatalogApprovalService | CatalogApprovalService.cs | ❌ Defined inline only |
| ICatalogFulfillmentService | CatalogFulfillmentService.cs | ❌ Defined inline only |
| ICABWorkflowService | CABWorkflowService.cs | ❌ Defined inline only |

### Missing Entity Models (need DB entities, not just VMs)

| Entity | Purpose | Where Referenced |
|--------|---------|------------------|
| SLAInstance | SLA tracking | SLAService, SLAEnforcementHostedService |
| CIRelationship | CMDB CI dependencies | CMDBService, ImpactAnalysisService |
| ChangeImpactedCI | Changed CIs | ChangeManagementService |
| ApprovalWorkflow | Catalog approval | CatalogApprovalService |
| ApprovalStage | Approval stages | CatalogApprovalService |
| DiscoveryScanResult | Asset discovery | DiscoveryService |
| DiscoveredAsset | Pending CMDB import | DiscoveryService |
| AssetLifecycle | Asset tracking | AssetLifecycleService |
| ChangeCalendar | Blackout periods | ChangeManagementService, ChangeCalendarService |

### Missing/Incomplete DTO Definitions

| DTO | Status | Location |
|-----|--------|----------|
| IncidentFilterDto | ❌ Missing | ITSMDtos.cs |
| ProblemFilterDto | ❌ Missing | ITSMDtos.cs |
| ChangeFilterDto | ❌ Missing | ITSMDtos.cs |
| EscalationRuleFilterDto | ❌ Missing | EscalationRuleDto.cs |
| CreateEscalationRuleDto | ❓ Check | EscalationRuleDto.cs |
| UpdateEscalationRuleDto | ❓ Check | EscalationRuleDto.cs |

---

## 🛠️ Recommended Fix Priority & Approach

### **Phase 1: Infrastructure Fix (Must Do First)**

1. **Refactor DI Pattern** (affects 9 services)
   - Remove `IDbContextResolver` from all ITSM services
   - Inject `ICrmDbContext` directly
   - Update all context resolution calls
   - **Effort:** 2-3 days
   - **Impact:** Unblocks 9 core services

2. **Extract Inline Interfaces** (affects 8 services)
   - Create new files in `Core/Interfaces/ITSM/`:
     - `IBusinessHoursCalculator.cs` (move from BusinessHoursCalculator.cs)
     - `IAssignmentRulesEngine.cs` (from AssignmentRulesEngine.cs)
     - `IArticleRecommendationService.cs` (from ArticleRecommendationService.cs)
     - `IImpactAnalysisService.cs` (from ImpactAnalysisService.cs)
     - `IDiscoveryService.cs` (from DiscoveryService.cs)
     - `ICatalogApprovalService.cs` (from CatalogApprovalService.cs)
     - `ICatalogFulfillmentService.cs` (from CatalogFulfillmentService.cs)
     - `ICABWorkflowService.cs` (from CABWorkflowService.cs)
   - **Effort:** 1 day
   - **Impact:** Properly interfaces 8 services

3. **Fix DTO Namespace Typos** (affects 2 services)
   - Global replace: `using CRM.Core.Dtos.ITSM` → `using CRM.Core.DTOs.ITSM`
   - Files: EscalationRuleAdminService.cs, EscalationPolicyService.cs
   - **Effort:** 30 minutes
   - **Impact:** Fixes compilation errors

### **Phase 2: Entity Model Completion**

4. **Create Missing Entities** (affects 5+ services)
   - `SLAInstance` - for tracking active SLA agreements
   - `CIRelationship` - for CMDB CI dependencies
   - `ChangeImpactedCI` - junction table for change→CI relationships
   - `ApprovalWorkflow`, `ApprovalStage` - for catalog approvals
   - `DiscoverySchedule`, `DiscoveredAsset` - for CMDB discovery
   - **Effort:** 3-4 days
   - **Impact:** Enables data persistence for advanced features

5. **Create Missing DTOs** (affects 7+ services)
   - `IncidentFilterDto`, `ProblemFilterDto`, `ChangeFilterDto`
   - `EscalationRuleFilterDto`, `CreateEscalationRuleDto`, `UpdateEscalationRuleDto`
   - Filtering support DTOs
   - **Effort:** 1-2 days
   - **Impact:** Enables service filtering/search

### **Phase 3: Service Completion**

6. **Complete Core Services** (IncidentService, ProblemService, etc.)
   - Implement pending helper methods
   - Verify all referenced entities exist in DbContext
   - Test CRUD operations
   - **Effort:** 5-7 days
   - **Impact:** Core ITSM functionality

7. **Complete Hosted Services**
   - SLAEnforcementHostedService - finish SLA breach detection
   - AutoCloseHostedService - finish auto-close logic
   - EscalationHostedService - refactor DI pattern
   - **Effort:** 2-3 days
   - **Impact:** Automated ITSM processes

### **Phase 4: Advanced Features** (optional, can defer)

8. **Advanced/Analytics Services** (ImpactAnalysis, Discovery, etc.)
   - These require substantial additional development
   - Can be deferred to later release
   - **Priority:** 🟢 LOW
   - **Estimated Effort:** 10+ days per service

---

## 📚 Supporting Type Definitions & Method Signatures

### Required DTO Definitions (to add to DTOs/ITSM/ITSMDtos.cs)

```csharp
// Incident filtering
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

// Problem filtering
public class ProblemFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ProblemState? State { get; set; }
    public int? Priority { get; set; }
    public bool? KnownError { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

// Change filtering
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

// Escalation rule filtering
public class EscalationRuleFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
}

// Escalation rule creation/update
public class CreateEscalationRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Queue { get; set; }
    public int AgeInMinutes { get; set; } = 60;
    public string TargetType { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public int RetryIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
}

public class UpdateEscalationRuleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Queue { get; set; }
    public int? AgeInMinutes { get; set; }
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string? TargetName { get; set; }
    public int? MaxAttempts { get; set; }
    public int? RetryIntervalMinutes { get; set; }
    public bool? IsActive { get; set; }
}
```

---

## 🔍 File Count Summary by Category

| Category | Count | Total Lines | Action |
|----------|-------|-------------|--------|
| Core ITSM Services | 8 | ~3,500 | Enable with Phase 1-2 fixes |
| Advanced Services | 6 | ~5,000 | Defer or implement Phase 4 |
| Hosted Services | 4 | ~1,200 | Fix Phase 1-3 |
| Admin Services | 9 | ~3,000 | Fix Phase 1-2 |
| Supporting Services | 4 | ~1,500 | Varies |
| **TOTAL** | **31** | **~14,200** | ~2-3 weeks effort |

---

## ✅ Verification Checklist

Before re-enabling each service, verify:

- [ ] All `using` statements compile (no missing namespaces)
- [ ] All interfaces are defined in `Core/Interfaces/`
- [ ] All entities exist in `Core/Entities/`
- [ ] All DTOs exist in `Core/DTOs/`
- [ ] No `IDbContextResolver` pattern usage (use `ICrmDbContext` directly)
- [ ] No circular dependencies (use dependency interfaces, not concrete)
- [ ] All `context.DbSet<T>` references have corresponding `DbSet<T>` in `ICrmDbContext`
- [ ] Helper methods are fully implemented (not stubbed)
- [ ] No local interface definitions (all in Core/Interfaces/)
- [ ] Unit tests exist or can be written

---

## 📖 Related Documentation

- [IITSMServices.cs](../../../CRM.Backend/src/CRM.Core/Interfaces/ITSM/IITSMServices.cs) - Core interface definitions
- [ITSMDtos.cs](../../../CRM.Backend/src/CRM.Core/DTOs/ITSM/ITSMDtos.cs) - DTO definitions
- [Incident.cs](../../../CRM.Backend/src/CRM.Core/Entities/ITSM/Incident.cs) - Entity models
- [DynamicDbContextResolver.cs](../../../CRM.Backend/src/CRM.Infrastructure/Data/DynamicDbContextResolver.cs) - Current multi-tenancy pattern
- [ICrmDbContext.cs](../../../CRM.Backend/src/CRM.Core/Interfaces/ICrmDbContext.cs) - Database context interface

---

**END OF ANALYSIS DOCUMENT**

# ITSM Services Implementation Roadmap & Dependency Graph

**Document Type:** Implementation Guide  
**Status:** Ready for implementation  
**Last Updated:** February 15, 2026

---

## 📊 Complete Dependency Graph

### Legend
- 🟢 **Implemented** - Code exists and compiles
- 🟡 **Partial** - Code exists but incomplete
- 🔴 **Missing** - Code doesn't exist
- ⚪ **Blocked** - Depends on other services

### Level 1: Foundation (No Dependencies)

```
├─ 🔴 IDbContextResolver interface
│  └─ BLOCKED: All services depend on this pattern
│
├─ 🟢 ICrmDbContext (injection target)
│  ├─ DbSet<Incident> ✅
│  ├─ DbSet<Problem> ✅
│  ├─ DbSet<Change> ✅
│  ├─ DbSet<ConfigurationItem> ✅
│  ├─ DbSet<KnowledgeArticle> ✅
│  ├─ DbSet<ServiceRequest> ✅
│  ├─ DbSet<CatalogItem> ✅
│  ├─ DbSet<SLAPolicy> ✅
│  ├─ DbSet<SLAInstance> ❌ (MISSING)
│  ├─ DbSet<EscalationRule> ✅
│  └─ DbSet<IncidentComment> ✅
│
└─ 🟡 Enums (mostly complete)
   ├─ IncidentState ✅
   ├─ IncidentImpact ✅
   ├─ IncidentUrgency ✅
   ├─ ProblemState ✅
   ├─ ChangeState ✅
   ├─ SLATargetType ✅
   ├─ SLAState ✅
   └─ EscalationTargetType ✅
```

### Level 2: Core Services (Direct DB Access)

```
BusinessHoursCalculator (IDbContextResolver)
└─ 🔴 Pattern refactor needed
   ├─ Requires: ICrmDbContext
   ├─ Provides: IBusinessHoursCalculator
   └─ Used by: SLAService, SLAEnforcementHostedService

IncidentService (IDbContextResolver, ISLAService)
└─ ⚪ BLOCKED: ISLAService dependency
   ├─ Requires: ICrmDbContext, ISLAService
   ├─ Provides: IIncidentService
   ├─ DTOs: IncidentDto, CreateIncidentDto, UpdateIncidentDto ✅
   └─ Entities: Incident ✅, IncidentComment ✅

ProblemService (IDbContextResolver)
└─ 🔴 Pattern refactor needed
   ├─ Requires: ICrmDbContext
   ├─ Provides: IProblemService
   ├─ DTOs: ProblemDto, CreateProblemDto ✅
   └─ Entities: Problem ✅, ProblemComment ✅

CMDBService (IDbContextResolver)
└─ 🔴 Pattern refactor needed
   ├─ Requires: ICrmDbContext
   ├─ Provides: ICMDBService
   ├─ DTOs: ConfigurationItemDto, CreateCIDto ✅
   └─ Entities: ConfigurationItem ✅

ChangeManagementService (IDbContextResolver, ICMDBService)
└─ ⚪ BLOCKED: ICMDBService dependency
   ├─ Requires: ICrmDbContext, ICMDBService
   ├─ Provides: IChangeManagementService
   ├─ DTOs: ChangeDto, CreateChangeDto ✅
   └─ Entities: Change ✅, ChangeComment ✅

KnowledgeManagementService (IDbContextResolver)
└─ 🔴 Pattern refactor needed
   ├─ Requires: ICrmDbContext
   ├─ Provides: IKnowledgeManagementService
   ├─ DTOs: KnowledgeArticleDto, CreateKnowledgeArticleDto ✅
   └─ Entities: KnowledgeArticle ✅

ServiceCatalogService (IDbContextResolver)
└─ 🔴 Pattern refactor needed
   ├─ Requires: ICrmDbContext
   ├─ Provides: IServiceCatalogService
   ├─ DTOs: CatalogItemDto, CreateCatalogRequestDto ✅
   └─ Entities: CatalogItem ✅, CatalogRequest ✅

SLAService (IDbContextResolver, IBusinessHoursCalculator)
└─ ⚪ BLOCKED: IBusinessHoursCalculator dependency
   ├─ Requires: ICrmDbContext, IBusinessHoursCalculator
   ├─ Provides: ISLAService
   ├─ DTOs: SLAPolicyDto, SLAInstanceDto ✅
   └─ Entities: SLAPolicy ✅, SLAInstance ❌
```

### Level 3: Admin/Utility Services

```
EscalationRuleAdminService (IRepository<EscalationRule>, ICrmDbContext)
└─ 🔴 Namespace typo + pattern inconsistency
   ├─ Uses: CRM.Core.Dtos.ITSM (WRONG - typo)
   ├─ Correct: CRM.Core.DTOs.ITSM
   ├─ Provides: IEscalationRuleAdminService ✅
   └─ Entities: EscalationRule ✅

EscalationPolicyService (ICrmDbContext)
└─ 🔴 Namespace typo
   ├─ Uses: CRM.Core.Dtos.ITSM (WRONG - typo)
   ├─ Provides: IEscalationPolicyService ✅
   └─ Entities: EscalationPolicy ✅, EscalationLevel ✅

SLAPolicyAdminService (ISLAService)
└─ ⚪ BLOCKED: ISLAService dependency
   └─ Provides: ISLAPolicyAdminService ✅
```

### Level 4: Hosted Services (Background Jobs)

```
SLAEnforcementHostedService (IServiceProvider, ILogger)
├─ 🟡 Partially implemented (ITSM_ADVANCED flag)
├─ Requires: ICrmDbContext, IEscalationRuleService
├─ Incomplete: CheckSLABreachesAsync method
└─ Blocks: Automated SLA enforcement

AutoCloseHostedService (IServiceProvider, ILogger)
├─ 🟡 Partially implemented (#if ITSM_ADVANCED)
├─ Requires: IDbContextResolver (should be ICrmDbContext)
├─ Incomplete: Multiple async methods
└─ Blocks: Auto-closure of resolved tickets

EscalationHostedService (IServiceProvider, ILogger)
├─ 🟢 ENABLED (but shouldn't be without Phase 1 fixes)
├─ Requires: IDbContextResolver
├─ Uses (#if ITSM_ADVANCED flag)
└─ Blocks: Automatic escalation routing
```

### Level 5: Advanced Services (Complex Features)

```
AssignmentRulesEngine (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline (needs extraction)
├─ 777 lines - most complex service
├─ Requires: Rule evaluation engine
└─ Provides: IAssignmentRulesEngine ❌

ImpactAnalysisService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ 866 lines - complex graph analysis
├─ Requires: CMDB relationship models
├─ Depends on: CMDBService (implicit)
└─ Provides: IImpactAnalysisService ❌

DiscoveryService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ 738 lines - network/CMDB scanning
├─ Requires: External API integrations
└─ Provides: IDiscoveryService ❌

ArticleRecommendationService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ 575 lines - AI/ML recommendations
├─ Requires: Semantic Kernel or similar
└─ Provides: IArticleRecommendationService ❌

CatalogApprovalService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ 726 lines - workflow engine
├─ Requires: Approval workflow models
└─ Provides: ICatalogApprovalService ❌

CatalogFulfillmentService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ Requires: External fulfillment APIs
└─ Provides: ICatalogFulfillmentService ❌

CABWorkflowService (ICrmDbContext, ILogger)
├─ 🔴 Interface defined inline
├─ Requires: CAB (Change Advisory Board) model
└─ Provides: ICABWorkflowService ❌
```

---

## 🔗 Dependency Matrix

| Service | Depends On | Provided To | Enable When |
|---------|-----------|------------|------------|
| **BusinessHoursCalculator** | ICrmDbContext | SLAService | Phase 1 |
| **IncidentService** | ICrmDbContext, ISLAService | Controllers, AutoCloseHostedService | Phase 2 |
| **ProblemService** | ICrmDbContext | IncidentService (implicit) | Phase 2 |
| **CMDBService** | ICrmDbContext | ChangeManagementService, ImpactAnalysisService | Phase 2 |
| **ChangeManagementService** | ICrmDbContext, ICMDBService | Controllers | Phase 2 |
| **KnowledgeManagementService** | ICrmDbContext | ImpactAnalysisService, ArticleRecommendation | Phase 2 |
| **ServiceCatalogService** | ICrmDbContext | CatalogApprovalService, Controllers | Phase 2 |
| **SLAService** | ICrmDbContext, IBusinessHoursCalculator | IncidentService, SLAEnforcementHostedService | Phase 2 |
| **EscalationRuleAdminService** | ICrmDbContext | SLAEnforcementHostedService | Phase 1 |
| **EscalationPolicyService** | ICrmDbContext | Controllers | Phase 1 |
| **SLAPolicyAdminService** | ICrmDbContext, ISLAService | Controllers | Phase 2 |
| **SLAEnforcementHostedService** | ICrmDbContext, IEscalationRuleService | (background task) | Phase 3 |
| **AutoCloseHostedService** | ICrmDbContext | (background task) | Phase 3 |
| **EscalationHostedService** | ICrmDbContext | (background task) | Phase 3 |
| **MonitoringIntegrationService** | External APIs | Controllers | Phase 4 |
| **AssignmentRulesEngine** | ICrmDbContext | IncidentService | Phase 4 |
| **ImpactAnalysisService** | ICrmDbContext, CMDBService | ChangeManagementService | Phase 4 |
| **DiscoveryService** | External APIs, ICrmDbContext | Controllers | Phase 4 |
| **ArticleRecommendationService** | Semantic Kernel, ICrmDbContext | IncidentService | Phase 4 |
| **CatalogApprovalService** | ICrmDbContext | ServiceCatalogService | Phase 4 |
| **CatalogFulfillmentService** | External APIs, ICrmDbContext | CatalogApprovalService | Phase 4 |

---

## 🎯 Implementation Phases Detailed

### ✅ Phase 1: Infrastructure & Fix-ups (2-3 days)

**Objective:** Fix compilation errors and architectural issues

#### Step 1.1: Refactor IDbContextResolver Pattern
**Effort:** 4-6 hours  
**Files to Modify:**
- BusinessHoursCalculator.cs.disabled
- IncidentService.cs.disabled
- ProblemService.cs.disabled
- CMDBService.cs.disabled
- ChangeManagementService.cs.disabled
- KnowledgeManagementService.cs.disabled
- ServiceCatalogService.cs.disabled
- SLAService.cs.disabled
- AutoCloseHostedService.cs.disabled

**Pattern Change:**
```csharp
// BEFORE:
private readonly IDbContextResolver _dbContextResolver;
public Service(IDbContextResolver dbContextResolver)
{ 
    _dbContextResolver = dbContextResolver;
}
var context = _dbContextResolver.ResolveContext();

// AFTER:
private readonly ICrmDbContext _context;
public Service(ICrmDbContext context)
{
    _context = context;
}
// Use _context directly instead of ResolveContext()
```

**Checklist:**
- [ ] Update constructor parameters
- [ ] Remove `IDbContextResolver` field
- [ ] Add `ICrmDbContext` field
- [ ] Replace all `_dbContextResolver.ResolveContext()` calls
- [ ] Verify compilation

#### Step 1.2: Fix DTO Namespace Typos
**Effort:** 30 minutes  
**Files:**
- EscalationRuleAdminService.cs.disabled
- EscalationPolicyService.cs.disabled

**Change:** `using CRM.Core.Dtos.ITSM;` → `using CRM.Core.DTOs.ITSM;`

**Checklist:**
- [ ] Global find/replace completed
- [ ] Files compile without namespace errors
- [ ] No other Dtos.ITSM references remain

#### Step 1.3: Extract Inline Interfaces
**Effort:** 8 hours  
**New Files to Create in `Core/Interfaces/ITSM/`:**
- `IBusinessHoursCalculator.cs`
- `IAssignmentRulesEngine.cs`
- `IArticleRecommendationService.cs`
- `IImpactAnalysisService.cs`
- `IDiscoveryService.cs`
- `ICatalogApprovalService.cs`
- `ICatalogFulfillmentService.cs`
- `ICABWorkflowService.cs`

**Process per Service:**
```
1. Copy interface definition from .disabled file
2. Create new dedicated .cs file in Core/Interfaces/ITSM/
3. Add proper namespaces: using CRM.Core.Entities.ITSM;
4. Remove supporting types that are DTOs/VMs from interface file
5. Create separate supporting type classes in DTOs or Entities
6. Update service file to use extracted interface
7. Remove interface definition from service file
```

**Checklist:**
- [ ] All 8 interface files created
- [ ] Interfaces properly namespaced
- [ ] Supporting types extracted and placed correctly
- [ ] Service files updated to reference interfaces
- [ ] No duplicate definitions remain

#### Step 1.4: Add Missing DTOs
**Effort:** 2-3 hours  
**Target File:** `Core/DTOs/ITSM/ITSMDtos.cs`

**DTOs to Add:**
```csharp
// Filters
public class IncidentFilterDto { ... }
public class ProblemFilterDto { ... }
public class ChangeFilterDto { ... }
public class EscalationRuleFilterDto { ... }

// Creation/Update
public class CreateEscalationRuleDto { ... }
public class UpdateEscalationRuleDto { ... }
```

**Checklist:**
- [ ] All filters added to ITSMDtos.cs
- [ ] All CRUD DTOs added
- [ ] Validation attributes applied ([Required], [StringLength], etc.)
- [ ] No duplicate definitions across files

### ⏳ Phase 2: Entity Models & Database (3-4 days)

**Objective:** Create missing entity models and database migrations

#### Step 2.1: Create Missing Entities
**Effort:** 2-3 days  
**Entities to Create in `Core/Entities/ITSM/`:**

1. **SLAInstance.cs** (critical)
```csharp
public class SLAInstance : BaseEntity
{
    public int SLAPolicyId { get; set; }
    public int TargetId { get; set; }
    public SLATargetType TargetType { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public DateTime? ResponseActualAt { get; set; }
    public DateTime? ResolutionActualAt { get; set; }
    
    public SLAState State { get; set; }
    public bool BreachedOnResponse { get; set; }
    public bool BreachedOnResolution { get; set; }
    
    public int? BusinessScheduleId { get; set; }
    public int? EscalationRuleId { get; set; }
    
    // Navigation
    public SLAPolicy? Policy { get; set; }
    public ICollection<SLABreachHistory>? BreachHistory { get; set; }
}

public class SLABreachHistory : BaseEntity
{
    public int SLAInstanceId { get; set; }
    public SLABreachType BreachType { get; set; }
    public DateTime BreachedAt { get; set; }
    public string? Notes { get; set; }
    
    public SLAInstance? Instance { get; set; }
}

public enum SLABreachType { Response, Resolution }
```

2. **CIRelationship.cs** (CMDB relationships)
```csharp
public class CIRelationship : BaseEntity
{
    public int ParentCIId { get; set; }
    public int ChildCIId { get; set; }
    public RelationshipType Type { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DateTime ValidFromDate { get; set; } = DateTime.UtcNow;
    public DateTime? ValidToDate { get; set; }
    
    public ConfigurationItem? ParentCI { get; set; }
    public ConfigurationItem? ChildCI { get; set; }
}

public enum RelationshipType
{
    DependsOn,
    SupportsService,
    PartOf,
    ConnectedTo,
    ReplacedBy,
    Duplicates
}
```

3. **ChangeImpactedCI.cs**
```csharp
public class ChangeImpactedCI : BaseEntity
{
    public int ChangeId { get; set; }
    public int ConfigurationItemId { get; set; }
    public ImpactLevel Impact { get; set; }
    public string? Notes { get; set; }
    
    public Change? Change { get; set; }
    public ConfigurationItem? ConfigurationItem { get; set; }
}

public enum ImpactLevel { High, Medium, Low }
```

4. **ApprovalWorkflow.cs** (Catalog approvals)
5. **DiscoverySchedule.cs** (Asset discovery)
6. **AssetLifecycleStatus.cs** (Asset tracking)

**Checklist:**
- [ ] All entities created with BaseEntity inheritance
- [ ] Navigation properties correctly defined
- [ ] Keys configured properly
- [ ] Foreign keys defined
- [ ] Enums created where needed
- [ ] Data annotations for constraints

#### Step 2.2: Update ICrmDbContext
**Effort:** 1 hour  
**File:** `Core/Interfaces/ICrmDbContext.cs`

**Add DbSets:**
```csharp
// In ICrmDbContext interface
DbSet<SLAInstance> ITSMSLAInstances { get; }
DbSet<SLABreachHistory> ITSMSLABreachHistories { get; }
DbSet<CIRelationship> ConfigurationItemRelationships { get; }
DbSet<ChangeImpactedCI> ChangeImpactedCIs { get; }
DbSet<ApprovalWorkflow> CatalogApprovalWorkflows { get; }
DbSet<ApprovalStage> CatalogApprovalStages { get; }
DbSet<ApprovalAction> CatalogApprovalActions { get; }
DbSet<DiscoverySchedule> CMDBDiscoverySchedules { get; }
```

**Checklist:**
- [ ] All DbSets added
- [ ] Naming convention consistent
- [ ] Updated in EF DbContext implementation

#### Step 2.3: Create Database Migration
**Effort:** 2 hours  
**Command:**
```bash
cd CRM.Backend
dotnet ef migrations add AddITSMServiceEntities
dotnet ef database update
```

**Checklist:**
- [ ] Migration created successfully
- [ ] No syntax errors in migration
- [ ] Database updated without errors
- [ ] Tables created with correct schema
- [ ] Foreign keys properly created
- [ ] Indexes created for performance

### 🔨 Phase 3: Service Implementation (5-7 days)

**Objective:** Complete core ITSM services

#### Step 3.1: Enable Core Services
**Files to Enable (remove .disabled):**
1. BusinessHoursCalculator.cs
2. IncidentService.cs
3. ProblemService.cs
4. CMDBService.cs
5. ChangeManagementService.cs
6. KnowledgeManagementService.cs
7. ServiceCatalogService.cs
8. SLAService.cs

**Per Service:**
```
1. Remove .disabled extension
2. Verify all using statements
3. Verify all method signatures
4. Implement any stub methods
5. Add to DI container in Program.cs or ServiceCollectionExtensions
6. Run unit tests
7. Verify compilation
```

#### Step 3.2: Register in Dependency Injection
**File:** `CRM.Backend/src/CRM.Api/Program.cs` or DI registration file

```csharp
// Add to service registration
services.AddScoped<IBusinessHoursCalculator, BusinessHoursCalculator>();
services.AddScoped<IIncidentService, IncidentService>();
services.AddScoped<IProblemService, ProblemService>();
services.AddScoped<ICMDBService, CMDBService>();
services.AddScoped<IChangeManagementService, ChangeManagementService>();
services.AddScoped<IKnowledgeManagementService, KnowledgeManagementService>();
services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
services.AddScoped<ISLAService, SLAService>();
```

**Checklist:**
- [ ] All 8 services registered
- [ ] Correct lifetimes (Scoped)
- [ ] No circular registration
- [ ] DI container builds successfully

#### Step 3.3: Complete Method Implementations
**Review Each Service For:**

1. **Number Generation Methods**
   - `GenerateIncidentNumberAsync()`
   - `GenerateProblemNumberAsync()`
   - `GenerateChangeNumberAsync()`
   - `GenerateCINumberAsync()`
   - **Ensure:** Unique sequences, format compliance

2. **DTO Mapping Methods**
   - `MapToDto(entity)` implementations
   - **Ensure:** Include all required properties, handle nulls

3. **Filter Application**
   - Using correct filter DTOs
   - Optional filter properties handled
   - Pagination implemented

4. **Relationship Management**
   - Junction tables (e.g., ProblemIncident)
   - Cascade behaviors correct
   - Orphaned records handled

**Checklist per Service:**
- [ ] All public methods have bodies (not just `throw new NotImplementedException()`)
- [ ] Helper methods completely implemented
- [ ] Error cases handled appropriately
- [ ] Unit tests pass

#### Step 3.4: Enable Hosted Services
**Files to Enable:**
1. SLAEnforcementHostedService.cs
2. AutoCloseHostedService.cs

**Per Service:**
1. Complete pending method implementations
2. Register as hosted service:
```csharp
services.AddHostedService<SLAEnforcementHostedService>();
services.AddHostedService<AutoCloseHostedService>();
```
3. Configure intervals/timings in appsettings.json
4. Add logging for monitoring

**Checklist:**
- [ ] All async methods complete
- [ ] Error handling robust
- [ ] Logging comprehensive
- [ ] Hosted service registration correct

### 🎓 Phase 4: Advanced Features (10+ days, OPTIONAL)

**Objective:** Implement optional advanced services

#### Services in Phase 4:
1. AssignmentRulesEngine (complex rule evaluation)
2. ImpactAnalysisService (graph analysis, prediction)
3. DiscoveryService (CMDB auto-discovery)
4. ArticleRecommendationService (ML/AI recommendations)
5. CatalogApprovalService (approval workflows)
6. CatalogFulfillmentService (external integration)
7. CABWorkflowService (change approval board)

**These can be deferred** to a later release if timeline is tight.

---

## 📋 Testing Strategy

### Unit Tests (per service)
```csharp
[TestFixture]
public class IncidentServiceTests
{
    private Mock<ICrmDbContext> _mockContext;
    private Mock<ISLAService> _mockSLAService;
    private IncidentService _service;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockSLAService = new Mock<ISLAService>();
        _service = new IncidentService(_mockContext.Object, _mockSLAService.Object, Mock.Of<ILogger<IncidentService>>());
    }

    [Test]
    public async Task CreateIncidentAsync_WithValidData_ReturnsIncidentDto()
    {
        // Arrange
        var createDto = new CreateIncidentDto { ... };
        var mockDbSet = new Mock<DbSet<Incident>>();
        _mockContext.Setup(c => c.Incidents).Returns(mockDbSet.Object);

        // Act
        var result = await _service.CreateIncidentAsync(createDto, 1);

        // Assert
        Assert.NotNull(result);
        Assert.That(result.ShortDescription, Is.EqualTo(createDto.ShortDescription));
    }

    [Test]
    public async Task GetIncidentsAsync_WithFilter_AppliesCorrectly()
    {
        // Test filter application
    }
}
```

### Integration Tests
```csharp
[TestFixture]
public class IncidentServiceIntegrationTests : IntegrationTestBase
{
    private IIncidentService _service;

    [SetUp]
    public void Setup()
    {
        _service = ServiceProvider.GetRequiredService<IIncidentService>();
    }

    [Test]
    public async Task CreateAndRetrieveIncident_WorksCorrectly()
    {
        // Test full flow using real DbContext
    }
}
```

---

## 🚀 Deployment Considerations

### Feature Flags
Add to `appsettings.json`:
```json
{
  "FeatureManagement": {
    "EnableITSMIncidents": true,
    "EnableITSMProblems": true,
    "EnableITSMChanges": true,
    "EnableITSMServiceCatalog": true,
    "EnableITSMSLA": true,
    "EnableAutoCloseTick ets": true,
    "EnableSLAEnforcement": true,
    "EnableAssignmentRules": false,
    "EnableImpactAnalysis": false,
    "EnableDiscovery": false,
    "EnableArticleRecommendation": false,
    "EnableCatalogApproval": false
  }
}
```

### Rollout Strategy
1. **Day 1-3:** Phase 1 fixes (infra)
2. **Day 4-7:** Phase 2 entities + Phase 3 core services
3. **Production:** Deploy with advanced features disabled initially
4. **Day 8+:** Phase 4 features can be enabled as they're completed

---

## 📞 Questions & Decisions

**Q: Should we use IDbContextResolver pattern?**
A: No. Use standard ASP.NET pattern with direct `ICrmDbContext` injection.

**Q: How to handle multi-tenancy?**
A: Create separate `ITenantContext` interface in Core if needed later, don't use DbContextResolver pattern.

**Q: Timeline for Phase 4 services?**
A: These are optional. De-scope from initial release and deliver in subsequent sprints.

**Q: Do we need all 31 services?**
A: No. Core 8 services (Phase 1-3) cover 80% of ITSM functionality. Advanced services (Phase 4) are incremental improvements.

---

**END OF ROADMAP DOCUMENT**

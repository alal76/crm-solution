# ADR-005: Large File Refactoring Strategy

## Status
Proposed

## Date
2026-02-02

## Context

During comprehensive architecture review, we identified several files exceeding recommended size thresholds (500 lines). Large files impact:
- Code readability and navigation
- Test maintainability
- Code review effectiveness
- Merge conflict frequency
- IDE performance

### Files Identified for Refactoring

| File | Lines | Priority | Status |
|------|-------|----------|--------|
| DeploymentSettingsTab.tsx | 3,608 | Critical | Pending |
| CrmDbContext.cs | 2,924 | Critical | Pending |
| DatabaseController.cs | 2,580 | Critical | Pending |
| AIPropertiesPanel.tsx | 2,468 | High | Pending |
| DatabaseSettingsTab.tsx | 2,367 | High | Pending |
| SampleDataSeederService.cs | 1,762 | Medium | Pending |
| MarketingCampaign.cs | 1,658 | High | Pending |
| Product.cs | 1,608 | High | Pending |
| ServiceRequestsPage.tsx | 1,627 | Medium | Pending |
| workflowService.ts | 1,479 | High | ✅ Completed |

## Decision

We will refactor large files using domain-driven decomposition patterns appropriate for each technology:

### 1. TypeScript/React Pattern: Module Folders with Barrel Exports

**Applied to:** workflowService.ts ✅

Split monolithic service files into focused modules:
```
services/workflow/
├── enums.ts              # Enumeration types
├── types.ts              # Core interfaces and DTOs
├── aiTypes.ts            # AI-specific types
├── workflowDefinitionApi.ts  # Definition CRUD operations
├── workflowInstanceApi.ts    # Instance management
└── index.ts              # Barrel export for public API
```

**Benefits:**
- Tree-shaking: Only import what you need
- Clear separation of concerns
- Easier testing of individual modules
- Backward compatibility via re-exports

### 2. Entity Framework Pattern: IEntityTypeConfiguration

**For:** CrmDbContext.cs

Extract entity configurations into separate files:
```
Data/Configurations/
├── Core/
│   ├── AccountConfiguration.cs
│   ├── ContactConfiguration.cs
│   └── ProductConfiguration.cs
├── Sales/
│   ├── OpportunityConfiguration.cs
│   └── QuoteConfiguration.cs
├── Marketing/
│   └── CampaignConfiguration.cs
└── ServiceDesk/
    └── ServiceRequestConfiguration.cs
```

**Implementation:**
```csharp
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(a => a.AccountNumber).IsUnique();
        builder.HasMany(a => a.Contacts).WithOne(c => c.Account);
        // ... configuration extracted from CrmDbContext
    }
}
```

**In CrmDbContext:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
}
```

### 3. React Component Pattern: Sub-Components with Hooks

**For:** DeploymentSettingsTab.tsx, AIPropertiesPanel.tsx, DatabaseSettingsTab.tsx

Split into focused components:
```
components/settings/deployment/
├── DeploymentSettingsTab.tsx      # Main container
├── ProviderSelector.tsx           # Cloud provider selection
├── ResourceConfiguration.tsx      # Resource settings
├── NetworkSettings.tsx            # Networking config
├── SecuritySettings.tsx           # Security options
├── CostEstimation.tsx            # Cost calculations
├── DeploymentPreview.tsx         # Preview/validation
├── hooks/
│   ├── useDeploymentConfig.ts    # Configuration state
│   └── useDeploymentValidation.ts # Validation logic
└── types.ts                       # Component-specific types
```

### 4. Domain Entity Pattern: Owned Types

**For:** MarketingCampaign.cs, Product.cs

Use EF Core owned types to decompose large entities:
```csharp
public class MarketingCampaign
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Owned types for logical grouping
    public CampaignSchedule Schedule { get; set; }
    public CampaignBudget Budget { get; set; }
    public CampaignTargeting Targeting { get; set; }
    public CampaignAnalytics Analytics { get; set; }
}

[Owned]
public class CampaignSchedule
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TimeZone { get; set; }
    public RecurrencePattern Recurrence { get; set; }
}
```

### 5. Controller Pattern: Area/Feature Controllers

**For:** DatabaseController.cs

Split by functionality:
```
Controllers/Database/
├── DatabaseSchemaController.cs   # Schema operations
├── DatabaseMigrationController.cs # Migrations
├── DatabaseBackupController.cs   # Backup/restore
└── DatabaseHealthController.cs   # Health checks
```

## Consequences

### Positive
- Improved code maintainability
- Better separation of concerns
- Easier onboarding for new developers
- Reduced merge conflicts
- Faster IDE navigation
- More focused unit tests

### Negative
- Initial refactoring effort required
- Need to update existing imports
- Potential for over-engineering if taken too far
- More files to manage

### Neutral
- Build times may slightly increase due to more files
- May require tooling updates (path aliases, barrel exports)

## Implementation Progress

### Phase 1: Frontend Services (Completed)
- [x] workflowService.ts → workflow/ module (1,479 → 6 files)

### Phase 2: Backend Data Layer (Planned)
- [ ] CrmDbContext entity configurations
- [ ] Large entity decomposition

### Phase 3: Frontend Components (Planned)
- [ ] DeploymentSettingsTab component splitting
- [ ] AIPropertiesPanel component splitting
- [ ] DatabaseSettingsTab component splitting

### Phase 4: Backend Controllers (Planned)
- [ ] DatabaseController splitting
- [ ] Service layer extraction

## Guidelines

### When to Split a File
- Exceeds 500 lines of non-generated code
- Contains multiple unrelated concerns
- Has complex conditional logic that could be separated
- Multiple developers frequently edit simultaneously

### When NOT to Split
- File is primarily generated code
- Splitting would create circular dependencies
- File represents a single cohesive domain concept
- Splitting would reduce code clarity

### Recommended Sizes
- TypeScript/React components: 200-400 lines max
- TypeScript services: 300-500 lines max
- C# controllers: 300-500 lines max
- C# entities: 200-400 lines max (excluding owned types)
- C# services: 400-600 lines max

## References
- [ADR-001: Initial Architecture Decisions](./ADR-001-initial-architecture-decisions.md)
- [ADR-004: Security and Architecture Improvements](./ADR-004-security-architecture-improvements.md)
- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans

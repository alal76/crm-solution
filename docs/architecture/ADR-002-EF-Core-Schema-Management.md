# ADR-002: Unified EF Core Schema Management

## Architecture Decision Record

| Field | Value |
|-------|-------|
| **ADR ID** | ADR-002 |
| **Title** | Unified EF Core Schema Management — Eliminate Dual-Path Database Initialization |
| **Status** | ACCEPTED |
| **Date** | 2026-02-18 |
| **Decision Makers** | Abhishek Lal (Owner) |
| **Supersedes** | Informal raw-SQL migration approach |

---

## 1. Context

### 1.1 Current State

The CRM solution's database lifecycle is managed by **two independent, competing systems** running in sequence at startup:

```
Program.cs startup sequence (current):
─────────────────────────────────────
  1. EnsureCreatedAsync()          ← EF Core creates tables from C# model
  2. Raw SQL migration loop        ← 22 .sql files re-CREATE / ALTER the same tables
  3. DbSeed.SeedAsync()            ← C# code seeds admin user, departments, lookups
  4. MasterDataSeederService       ← C# code seeds ZIP codes, color palettes
  5. SampleDataSeederService       ← Optional sample data
```

Additionally, a parallel set of **11 DDL files** in `database/schema/` and **13 seed files** in `database/seed/` exist for manual database provisioning via shell scripts (`setup-database.sh`, `deploy.sh`).

### 1.2 Inventory of SQL Artifacts

| Location | Files | Lines (approx) | Purpose |
|----------|-------|-----------------|---------|
| `CRM.Backend/migrations/` | 22 SQL files (007–022) | ~9,400 | DDL: CREATE TABLE, ALTER TABLE, CREATE INDEX, some INSERT |
| `database/schema/` | 11 SQL files (000–009) | ~4,000 | DDL: baseline schema, tables, indexes |
| `database/seed/` | 13 SQL files (000–012) + data-sets/ | ~2,500 | DML: core seed, settings, lookups, workflows, ITSM |
| `database/master_data/` | 3 files | ~500 | ZIP codes (sample), timezones, conversion script |
| `database/setup-database.sh` | 1 file | 842 | Interactive shell setup |
| `database/deploy.sh` | 1 file | 266 | Deployment helper |

**Total:** ~50 SQL-related files comprising ~17,500 lines.

### 1.3 Problems Identified

1. **Schema Collision**: `EnsureCreatedAsync()` creates tables from the EF Core model, then raw SQL files attempt to CREATE or ALTER the same tables, causing silent failures or redundant operations.

2. **Divergent Truth Sources**: The C# entity model (`CrmDbContext` with 200+ DbSets) and the SQL DDL files define overlapping but not identical schemas. Seven entities exist only in SQL and have no C# representation:
   - `WorkflowSchedules`, `WorkflowJobs`, `WorkflowContextVariables`
   - `workflow_audit_log`, `workflow_metrics`, `workflow_llm_usage`, `workflow_circuit_breaker_state`

3. **Non-Idempotent Migrations**: Raw SQL files lack state tracking. Every restart re-executes all 22 files, relying on `try/catch` to swallow "table already exists" errors.

4. **Seed Data Duplication**: Admin user, departments, lookup data, and system settings are defined in both `DbSeed.cs` (C#) and `database/seed/*.sql` files — with divergent values.

5. **EnsureCreated Blocks Migrations**: Once `EnsureCreatedAsync()` creates the schema, EF Core's `__EFMigrationsHistory` table is never populated. This makes it impossible to use `MigrateAsync()` for future incremental changes without manual intervention.

6. **No Rollback Capability**: Raw SQL files have no down-migration support. Schema changes are one-way and untestable.

### 1.4 Risk Assessment

| Risk | Severity | Description |
|------|----------|-------------|
| Silent schema drift | 🔴 High | EF model diverges from actual DB over time |
| Startup latency | 🟡 Medium | 22 SQL files executed every restart (~2–5s wasted) |
| Multi-provider fragility | 🔴 High | SQL files use MySQL syntax; PostgreSQL/SQL Server break |
| Orphan entities | 🟡 Medium | 7 tables exist in SQL but not in C# — invisible to EF |
| Blocked incremental migration | 🔴 High | Cannot use `dotnet ef migrations add` without baseline |
| Untestable seeding | 🟡 Medium | SQL seed files can't be covered by unit tests |

---

## 2. Decision

### 2.1 Core Decision

**Entity Framework Core will be the single, authoritative owner of all database schema management.** All raw SQL migration files, DDL scripts, seed scripts, and shell-based database tools will be retired.

### 2.2 Specific Decisions

| # | Decision | Rationale |
|---|----------|-----------|
| D1 | Replace `EnsureCreatedAsync()` with `MigrateAsync()` for **all** database providers | Enables incremental migration tracking via `__EFMigrationsHistory` |
| D2 | Create one baseline EF Core migration capturing the full current model | Establishes a clean starting point; data loss is acceptable in dev |
| D3 | Delete all 22 files in `CRM.Backend/migrations/` | Eliminates the raw SQL migration loop entirely |
| D4 | Archive all files in `database/schema/`, `database/seed/`, `database/master_data/` | These become documentation-only references |
| D5 | Remove `database/setup-database.sh` and `database/deploy.sh` | Shell-based DB setup is replaced by `dotnet ef database update` |
| D6 | Add missing orphan entities to `CrmDbContext` before baseline migration | Ensures 100% schema coverage in the EF model |
| D7 | Seed the SysAdmin user with `PasswordNeverSet = true` and no password hash | User sets password on first login via existing `/api/auths/setup-password` flow |
| D8 | Move all startup seeding (master data, lookups, settings) to API-triggered endpoints | Application starts with an empty database; seeding is an explicit post-deploy step |
| D9 | Retain `DbSeed.SeedAsync()` only for the minimal SysAdmin bootstrap (group + user) | This is the one piece that MUST exist for the system to be usable at all |
| D10 | All future schema changes use `dotnet ef migrations add` / `dotnet ef database update` | Standard EF Core workflow; no SQL scripts ever |

### 2.3 Constraints Acknowledged

- **Data Loss Acceptable**: System is in development. All existing database instances will be dropped and recreated.
- **One-Shot Execution**: This is not a phased migration. All changes land in a single commit.
- **No SQL Scripts Going Forward**: The team will not author raw SQL for schema or seed operations.

---

## 3. Implementation Plan

### 3.1 Overview

```
Phase 1: Model Completion          → Add orphan entities to CrmDbContext
Phase 2: Baseline Migration        → dotnet ef migrations add InitialCreate
Phase 3: Startup Restructure       → Replace EnsureCreated + SQL loop with MigrateAsync
Phase 4: Seed Data Refactor        → SysAdmin-only startup seed; everything else via API
Phase 5: Artifact Retirement       → Delete/archive SQL files, shell scripts
Phase 6: Documentation Update      → Update DATABASE_SCHEMA.md, README, copilot-instructions
Phase 7: Verification              → Drop DB, restart, validate clean startup
```

### 3.2 Phase 1 — Model Completion

**Goal:** Ensure every table that was previously created by raw SQL has a corresponding EF Core entity and DbSet.

**Orphan Entities to Add:**

| SQL Table | Proposed Entity | DbSet Name | Source File |
|-----------|-----------------|------------|-------------|
| `WorkflowSchedules` | `WorkflowSchedule` | `WorkflowSchedules` | 009_create_workflow_engine_tables.sql |
| `WorkflowJobs` | `WorkflowJob` | `WorkflowJobs` | 009_create_workflow_engine_tables.sql |
| `WorkflowContextVariables` | `WorkflowContextVariable` | `WorkflowContextVariables` | 009_create_workflow_engine_tables.sql |
| `workflow_audit_log` | `WorkflowAuditLog` | `WorkflowAuditLogs` | 010_workflow_enhancements.sql |
| `workflow_metrics` | `WorkflowMetric` | `WorkflowMetrics` | 010_workflow_enhancements.sql |
| `workflow_llm_usage` | `WorkflowLlmUsage` | `WorkflowLlmUsages` | 010_workflow_enhancements.sql |
| `workflow_circuit_breaker_state` | `WorkflowCircuitBreakerState` | `WorkflowCircuitBreakerStates` | 010_workflow_enhancements.sql |

**Actions:**

1. Cross-reference SQL DDL with existing entities in `CRM.Core/Entities/` to verify these 7 are truly missing
2. Create entity classes following existing conventions (`BaseEntity` inheritance, PascalCase, etc.)
3. Add DbSet declarations to `CrmDbContext.cs`
4. Add any necessary `OnModelCreating` configuration (indexes, relationships, column types)
5. Build and verify: `dotnet build CRM.Backend/CRM.sln`

### 3.3 Phase 2 — Baseline EF Core Migration

**Goal:** Generate a single migration that represents the complete current model state.

**Prerequisite:** Phase 1 complete (all entities in model).

**Commands:**

```bash
cd CRM.Backend

# Generate the initial migration
dotnet ef migrations add InitialCreate \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api \
  -- --DatabaseProvider mariadb

# Verify the generated migration file looks correct
# The Up() method should contain CREATE TABLE for all 200+ entities
# The Down() method should DROP all tables
```

**Validation:**
- Migration file exists in `CRM.Infrastructure/Migrations/`
- `ModelSnapshot.cs` is generated
- Build succeeds with no errors

**Important:** Do NOT run `dotnet ef database update` yet — that happens after Phase 3 restructures startup.

### 3.4 Phase 3 — Startup Restructure

**Goal:** Replace the 6-stage initialization in `Program.cs` with a clean `MigrateAsync()` call.

**Current Code** (Program.cs lines ~676–795):
```csharp
// REMOVE: EnsureCreatedAsync for non-SQLite
// REMOVE: Fallback EnsureCreated for SQLite
// REMOVE: Raw SQL migration file loop
// KEEP (modified): DbSeed.SeedAsync — but only SysAdmin bootstrap
// REMOVE: MasterDataSeederService.SeedIfEmptyAsync() from startup
// REMOVE: SampleDataSeederService auto-seed from startup
```

**New Code:**

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    try
    {
        Log.Information("Applying EF Core migrations for {Provider}...", databaseProvider);
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");

        // Minimal bootstrap: SysAdmin group + admin user only
        await DbSeed.SeedAsync(db);
        Log.Information("Bootstrap seed completed");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database initialization failed");
        throw;
    }
}
```

**Key Changes:**
- `EnsureCreatedAsync()` → removed entirely
- SQLite special-case → removed (SQLite also uses `MigrateAsync()`)
- Raw SQL loop → removed
- `MasterDataSeederService.SeedIfEmptyAsync()` → removed from startup (stays as API)
- `SampleDataSeederService` auto-seed → removed from startup (stays as API)

### 3.5 Phase 4 — Seed Data Refactor

**Goal:** Restructure seeding so that only the absolute minimum runs at startup, and everything else is triggered via API.

#### 4A. Slim Down `DbSeed.SeedAsync()`

The static `DbSeed.SeedAsync()` method will be reduced to seed **only**:

1. **SysAdmin UserGroup** — with full permissions (existing code, no change)
2. **Admin User** — with `PasswordNeverSet = true`, no password hash

**Admin User Changes:**

```csharp
// BEFORE (current):
adminUser = new User
{
    Username = adminUsername,
    Email = adminEmail,
    PasswordHash = HashPassword(adminPassword),  // "Admin@123"
    Role = (int)UserRole.Admin,
    IsActive = true,
    EmailVerified = true,
    PrimaryGroupId = sysAdminGroup.Id
};

// AFTER:
adminUser = new User
{
    Username = adminUsername,
    Email = adminEmail,
    PasswordHash = "",                  // No password
    PasswordNeverSet = true,            // Forces password setup on first login
    Role = (int)UserRole.Admin,
    IsActive = true,
    EmailVerified = true,
    PrimaryGroupId = sysAdminGroup.Id
};
```

This leverages the existing `PasswordNeverSet` flow in `AuthenticationService.cs` (line 205):
- Login with any password → returns `RequiresPasswordSetup = true` + setup token
- Frontend redirects to `/setup-password`
- `POST /api/auths/setup-password` sets the real password

**Remove from `DbSeed.SeedAsync()`:**
- ❌ Departments (15 departments) → move to seed API
- ❌ Sample Accounts (John Doe, Jane Smith) → move to sample data API
- ❌ Sample Products → move to sample data API
- ❌ LookupCategories (Currency 163 items, BillingCycle, ContactMethod) → move to seed API
- ❌ Additional master data → move to seed API
- ❌ Sample Contacts → move to sample data API

#### 4B. Create Core Seed API Endpoint

Create a new controller (or extend existing) that seeds the "core" data that every deployment needs but isn't strictly required for first login:

**`POST /api/admin/seed/core`** (requires SysAdmin role)

Seeds:
- Departments (15)
- LookupCategories + Items (Currency, BillingCycle, PreferredContactMethod)
- SystemSettings defaults
- Service request categories/types
- Default workflow definitions

**`POST /api/admin/seed/master-data`** (requires SysAdmin role)

Seeds (already exists via `MasterDataController`):
- ZIP codes
- Color palettes
- Timezones

**`POST /api/admin/seed/sample-data`** (requires SysAdmin role)

Seeds (already exists via `SampleDataController`):
- Sample accounts, contacts, opportunities, products

#### 4C. Create `CoreDataSeederService`

A new service that consolidates the data currently split between `DbSeed.cs` and `database/seed/*.sql`:

```csharp
public interface ICoreDataSeederService
{
    Task SeedDepartmentsAsync(CancellationToken ct = default);
    Task SeedLookupDataAsync(CancellationToken ct = default);
    Task SeedSystemSettingsAsync(CancellationToken ct = default);
    Task SeedServiceRequestTypesAsync(CancellationToken ct = default);
    Task SeedWorkflowDefinitionsAsync(CancellationToken ct = default);
    Task SeedAllCoreDataAsync(CancellationToken ct = default);
    Task<CoreDataStats> GetStatsAsync(CancellationToken ct = default);
}
```

**Data Sources Mapping:**

| Data | Current Source | New Source |
|------|---------------|-----------|
| Departments (15) | `DbSeed.cs` lines 196–215 | `CoreDataSeederService.SeedDepartmentsAsync()` |
| Currency (163 items) | `DbSeed.cs` lines 300–480 | `CoreDataSeederService.SeedLookupDataAsync()` |
| BillingCycle (3 items) | `DbSeed.cs` lines ~485–495 | `CoreDataSeederService.SeedLookupDataAsync()` |
| ContactMethod (3 items) | `DbSeed.cs` lines ~500–510 | `CoreDataSeederService.SeedLookupDataAsync()` |
| SystemSettings | `database/seed/003_system_settings.sql` | `CoreDataSeederService.SeedSystemSettingsAsync()` |
| Service request types | `database/seed/004_service_request_types.sql` | `CoreDataSeederService.SeedServiceRequestTypesAsync()` |
| Workflow definitions | `database/seed/008_workflow_definitions.sql` | `CoreDataSeederService.SeedWorkflowDefinitionsAsync()` |

### 3.6 Phase 5 — Artifact Retirement

**Goal:** Remove all SQL-based database management artifacts.

#### 5A. Delete (tracked in git history)

| Path | Files | Reason |
|------|-------|--------|
| `CRM.Backend/migrations/*.sql` | 22 files | Replaced by EF Core migrations |
| `database/schema/*.sql` | 11 files | Schema owned by CrmDbContext |
| `database/seed/*.sql` + `data-sets/` | 13+ files | Seed via API |
| `database/master_data/*.sql` + `*.py` | 3 files | Master data via API |
| `database/setup-database.sh` | 1 file | Replaced by `dotnet ef database update` |
| `database/deploy.sh` | 1 file | Replaced by `dotnet ef database update` |

#### 5B. Retain (modified)

| Path | Change |
|------|--------|
| `database/DATABASE_SCHEMA.md` | Update to reference EF Core as source of truth |
| `database/README.md` | Rewrite with new workflow |

### 3.7 Phase 6 — Documentation Update

**Files to Update:**

| Document | Changes |
|----------|---------|
| `database/DATABASE_SCHEMA.md` | Remove SQL references; state EF Core is authoritative |
| `database/README.md` | New content: `dotnet ef` commands, seed API instructions |
| `docs/architecture/DATABASE_CONFIGURATION.md` | Update initialization flow description |
| `.github/copilot-instructions.md` | Update Section 9 (Build & Deploy), Section 4 (Database), Section 12 (Important Notes) |
| `SOLUTION_CONTEXT.md` | Update Section 8 (Database Startup & Seeding Process) |
| `ARCHITECTURE_OVERVIEW.md` | Update Data Layer section |

### 3.8 Phase 7 — Verification

**Clean-Room Test:**

```bash
# 1. Drop the existing database
docker exec -it crm-mariadb mariadb -u root -pRootPass@Dev2024 -e "DROP DATABASE IF EXISTS crm_db; CREATE DATABASE crm_db;"

# 2. Rebuild the API
cd CRM.Backend && dotnet build

# 3. Start the API (MigrateAsync runs automatically)
cd src/CRM.Api && dotnet run

# 4. Verify:
#    ✅ Migrations apply without errors
#    ✅ __EFMigrationsHistory table has one row (InitialCreate)
#    ✅ All 200+ tables exist
#    ✅ SysAdmin group exists
#    ✅ Admin user exists with PasswordNeverSet = true
#    ✅ No other seed data present
#    ✅ Health endpoint returns healthy: curl http://localhost:5000/health

# 5. Seed core data via API:
TOKEN=$(curl -s -X POST http://localhost:5000/api/auths/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"any"}' | jq -r '.passwordSetupToken')

# (First: set admin password via setup-password flow)
# Then authenticate and call seed endpoints:
curl -X POST http://localhost:5000/api/admin/seed/core \
  -H "Authorization: Bearer $JWT"

curl -X POST http://localhost:5000/api/admin/seed/master-data \
  -H "Authorization: Bearer $JWT"

# 6. Verify seeded data:
#    ✅ 15 departments
#    ✅ 163+ lookup items
#    ✅ ZIP codes populated
#    ✅ Color palettes populated
#    ✅ System settings configured
```

---

## 4. Post-Migration Workflow

### 4.1 Adding a New Entity (Going Forward)

```bash
# 1. Create entity class in CRM.Core/Entities/
# 2. Add DbSet to CrmDbContext.cs
# 3. Add any OnModelCreating configuration
# 4. Generate migration:
dotnet ef migrations add AddNewEntity \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# 5. Review generated migration
# 6. Apply:
dotnet ef database update \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# 7. Commit migration files to git
```

### 4.2 Modifying an Existing Entity

```bash
# 1. Edit entity class (add/remove/change properties)
# 2. Generate migration:
dotnet ef migrations add ModifyEntityName \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# 3. Review generated migration (verify Up/Down methods)
# 4. Apply locally, test, commit
```

### 4.3 Deployment Pipeline

```
Build → Test → dotnet ef database update → Deploy API → Seed via API (if first deploy)
```

For CI/CD, migrations are applied using:
```bash
dotnet ef database update \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api \
  --connection "Server=...;Database=crm_db;..."
```

Or at application startup (current approach, retained):
```csharp
await db.Database.MigrateAsync();
```

### 4.4 First-Time Deployment Checklist

1. ✅ Deploy API (MigrateAsync creates schema + seeds SysAdmin)
2. ✅ Navigate to login page
3. ✅ Login as `admin@crm.local` (any password) → redirected to password setup
4. ✅ Set admin password
5. ✅ Call `POST /api/admin/seed/core` to load departments, lookups, settings
6. ✅ Call `POST /api/admin/seed/master-data` to load ZIP codes, palettes
7. ✅ (Optional) Call `POST /api/admin/seed/sample-data` to load demo data

---

## 5. Consequences

### 5.1 Positive

| Consequence | Impact |
|-------------|--------|
| **Single source of truth** | C# model is the only schema definition |
| **Incremental migrations** | `dotnet ef migrations add` tracks every change |
| **Multi-provider safe** | EF Core generates provider-specific DDL automatically |
| **Testable seeding** | All seed logic in C# services, coverable by unit tests |
| **Faster startup** | No more executing 22 SQL files on every restart |
| **Rollback capability** | Every migration has an `Up()` and `Down()` method |
| **CI/CD friendly** | `dotnet ef database update` works in pipelines |
| **Secure bootstrap** | No hardcoded passwords; admin sets password on first login |
| **Explicit seeding** | Core data loaded via API — visible, auditable, repeatable |

### 5.2 Negative

| Consequence | Mitigation |
|-------------|------------|
| **All existing data is lost** | Acceptable in dev; production doesn't exist yet |
| **Large initial migration file** | One-time cost; future migrations are small |
| **Post-deploy seed step required** | Documented in checklist; can be scripted |
| **Learning curve for SQL-only contributors** | EF Core is industry standard; documentation provided |

### 5.3 Neutral

| Consequence | Note |
|-------------|------|
| Migration files are committed to git | Standard EF Core practice |
| `__EFMigrationsHistory` table added to database | Standard EF Core tracking table |
| SQL files remain in git history | Available for archaeological reference |

---

## 6. Files Changed (Complete Manifest)

### 6.1 Modified

| File | Change |
|------|--------|
| `CRM.Backend/src/CRM.Api/Program.cs` | Replace 6-stage init with `MigrateAsync()` + minimal seed |
| `CRM.Backend/src/CRM.Infrastructure/Data/DbSeed.cs` | Strip to SysAdmin group + passwordless admin user only |
| `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` | Add ~7 orphan entity DbSets + configurations |
| `database/DATABASE_SCHEMA.md` | Update to reference EF Core |
| `database/README.md` | Rewrite with new workflow |
| `SOLUTION_CONTEXT.md` | Update database sections |
| `.github/copilot-instructions.md` | Update database and build sections |

### 6.2 Created

| File | Purpose |
|------|---------|
| `CRM.Core/Entities/WorkflowSchedule.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowJob.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowContextVariable.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowAuditLog.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowMetric.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowLlmUsage.cs` | Orphan entity |
| `CRM.Core/Entities/WorkflowCircuitBreakerState.cs` | Orphan entity |
| `CRM.Infrastructure/Migrations/*_InitialCreate.cs` | Baseline migration |
| `CRM.Infrastructure/Migrations/*_InitialCreate.Designer.cs` | Migration metadata |
| `CRM.Infrastructure/Migrations/CrmDbContextModelSnapshot.cs` | Model snapshot |
| `CRM.Infrastructure/Services/CoreDataSeederService.cs` | Core data seed service |
| `CRM.Core/Interfaces/ICoreDataSeederService.cs` | Interface for core seeder |
| `CRM.Api/Controllers/AdminSeedController.cs` | Seed API endpoints |

### 6.3 Deleted

| Path | Count | Description |
|------|-------|-------------|
| `CRM.Backend/migrations/*.sql` | 22 | Raw SQL migration files |
| `database/schema/*.sql` | 11 | DDL schema files |
| `database/seed/*.sql` + `data-sets/` | 13+ | Seed data SQL files |
| `database/master_data/*.sql` + `*.py` | 3 | Master data files |
| `database/setup-database.sh` | 1 | Shell setup script |
| `database/deploy.sh` | 1 | Shell deploy script |

---

## 7. Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Initial migration is very large | Certain | Low | One-time; future migrations are incremental |
| Orphan entities have wrong column types | Medium | Medium | Cross-reference SQL DDL when creating entities |
| MigrateAsync fails on existing non-empty DB | Low | Low | Drop and recreate — data loss is accepted |
| Seed API not called after deploy | Medium | Medium | Health check can report "unseeded" state |
| Multi-provider migration compatibility | Low | Medium | Test with MariaDB (primary) + SQLite; PostgreSQL/SQL Server tested when needed |

---

## 8. Decision Rationale Summary

> The CRM solution is in active development with no production deployments. The current dual-path database management (EF Core + raw SQL) creates schema drift, blocks incremental migrations, wastes startup time, and makes multi-database support fragile. Since data loss is acceptable, a clean one-shot cutover to pure EF Core management eliminates all six identified risks, establishes a maintainable migration workflow, and makes seed data testable and API-driven.
>
> The existing `PasswordNeverSet` authentication flow makes passwordless admin seeding safe and user-friendly — the admin simply sets their password on first login.

---

## References

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [ADR-001: Pluggable Architecture Strategy](ADR-001-Pluggable-Architecture-Strategy.md)
- [DATABASE_CONFIGURATION.md](DATABASE_CONFIGURATION.md)
- [SOLUTION_CONTEXT.md](../../SOLUTION_CONTEXT.md) — Section 8

---

**END OF ADR-002**

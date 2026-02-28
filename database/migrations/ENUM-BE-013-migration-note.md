# ENUM-BE-013: EF Core Migration – AddConfigurableEnumSupport

## Status
- ⚠️ Migration NOT yet generated/applied on dev server
- SQL schema script exists at: `database/migrations/SYS-008-ConfigurableEnums.sql`

## Tables Created By This Migration

| Table | Purpose |
|-------|---------|
| `EnumCategories` | New dedicated table for configurable enum categories |
| `EnumValues`     | New dedicated table for configurable enum values |
| `EnumTransitions`| State-transition rules (note: `IF NOT EXISTS` guards duplication) |

## Commands To Run

Execute from the repository root (`CRM.Backend/` directory):

```bash
# 1. Generate the EF Core migration
dotnet ef migrations add AddConfigurableEnumSupport \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api

# 2. Apply the migration to the database
dotnet ef database update \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api
```

Or using the MariaDB-specific connection string override:

```bash
dotnet ef database update \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api \
    -- --ConnectionStrings__DefaultConnection="Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;"
```

## Notes

- The migration **must** be run before the new `api/enummanagement` endpoints will function.
- After `dotnet ef migrations add`, verify the generated `.cs` file contains `CreateTable` calls for `EnumCategories`, `EnumValues`, and `EnumTransitions`.
- Seed data (from `SYS-008-ConfigurableEnums.sql`) should be applied manually or integrated into a seeder after the migration runs.
- The existing `LookupCategories` / `LookupItems` tables and their EF entities (`LookupCategory`, `LookupItem`) are NOT affected by this migration.

## Related Items

| Item | File |
|------|------|
| Entities | `CRM.Core/Entities/EnumCategory.cs`, `EnumValue.cs`, `EnumTransition.cs` |
| DTOs | `CRM.Core/DTOs/EnumDtos.cs` |
| Service interface | `CRM.Core/Interfaces/IEnumManagementService.cs` |
| Service impl | `CRM.Infrastructure/Services/EnumManagementService.cs` |
| Controller | `CRM.Api/Controllers/EnumManagementServiceController.cs` |
| SQL seed | `database/migrations/SYS-008-ConfigurableEnums.sql` |
| DI registration | `CRM.Api/Program.cs` (AddScoped line ~1095) |

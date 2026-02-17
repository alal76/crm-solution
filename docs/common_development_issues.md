# Common Development Issues
# Common Development Issues

## Database Schema Drift from EF Core Model

 - **Symptom:** Database tables, columns, or constraints do not match the current Entity Framework (EF) Core model. Examples: missing tables, extra columns, mismatched data types, or failed migrations.
 - **Root Cause:**
	 - Legacy use of both raw SQL schema files and EF Core migrations led to two sources of truth. Some tables/entities existed only in SQL, others only in C#.
	 - Use of `EnsureCreated()` in development or startup scripts created schemas without tracking migration history, blocking future use of `MigrateAsync()` and causing silent drift.
	 - Manual changes to the database (via SQL or admin tools) were not reflected in the EF model or migrations.
	 - Incomplete or missing `OnModelCreating` configuration for new entities, or missing `DbSet` declarations in `CrmDbContext`.
	 - Not following the specification process: changes made directly to the database or code without updating the feature spec and running migrations.
 - **Fix:**
	 1. **EF Core is the single source of truth for schema.**
		 - All tables, columns, and constraints must be defined in the C# model and `OnModelCreating`.
		 - Do not edit the database schema directly or use raw SQL files for schema changes.
	 2. **Always use EF Core migrations for schema changes.**
		 - Add or update entities in code, then run:
		   ```bash
		   dotnet ef migrations add <MigrationName> --project src/CRM.Infrastructure --startup-project src/CRM.Api
		   dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
		   ```
		 - Never use `EnsureCreated()` except for initial dev DBs (and only if you will never migrate that DB).
	 3. **If schema drift is detected:**
		 - Drop and recreate the database from scratch using migrations, or manually align the schema to match the EF model, then reapply migrations.
		 - Remove any orphan tables or columns not present in the EF model.
	 4. **Follow the feature specification process:**
		 - All schema changes must be documented in the relevant `docs/11-specifications/SPEC-*.md` file before implementation.
		 - Update the spec and mark as implemented when complete.
	 5. **Troubleshooting:**
		 - If `dotnet ef database update` fails, check for pending model changes, missing migrations, or use of `EnsureCreated()` in the past (see ADR-002 for recovery steps).
		 - If you see `__EFMigrationsHistory` missing, the DB was not created by migrations—drop and recreate.
		 - For multi-provider support, always validate migrations on all supported DBs (MariaDB, SQL Server, PostgreSQL).

 **References:**
 - See `docs/architecture/ADR-002-EF-Core-Schema-Management.md` for the unified schema management policy and recovery steps.
 - See `docs/development/DATABASE_EF_CORE_GAP_ANALYSIS.md` for validation checklists and gap analysis.

 ---

## API enum binding requires numeric values

- Symptom: 400 validation errors such as `The JSON value could not be converted to CRM.Core.Entities.AccountCategory` and `The dto field is required.`
- Cause: API uses default JSON enum handling (numeric only). String enum values fail model binding.
- Fix: Send numeric enum values (example: `AccountCategory.Organization` = `1`, `AccountType.Enterprise` = `3`).
- Where seen: `POST /api/accounts` during test data load.

## Orders tables missing in MariaDB

- Symptom: `Table 'crm_db.Orders' doesn't exist` or `Table 'crm_db.Quotes' doesn't exist` when seeding sales data.
- Cause: Schema deployments before 2026-02-17 did not include Orders/OrderLineItems tables; Quotes may be missing if schema files were not applied.
- Fix: Apply database schema updates (schema files include `010_sales_orders.sql`). For existing databases, run the schema deploy or apply the file directly.
- Where seen: `POST /api/orders/*` and `POST /api/quotes/*/lineitems` during test data load.

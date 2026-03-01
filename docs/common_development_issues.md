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

---

## MariaDB migration fails with "Table already exists"

- **Symptom:** `dotnet ef database update` or `MigrateAsync()` fails with `MySqlException: Table 'X' already exists`. Re-running still fails because some tables were created but migration history was not recorded.
- **Root Cause:** MariaDB/MySQL DDL statements (CREATE TABLE, ALTER TABLE, DROP TABLE) are **auto-committed and non-transactional**. If EF Core's `MigrateAsync()` fails partway through a migration, previously executed CREATE TABLE statements persist (auto-committed) but the migration record in `__EFMigrationsHistory` is NOT inserted (rolled back with the transaction). On retry, the first already-existing table causes an immediate failure.
- **Additional causes found (v0.560.4):**
  1. `Program.cs` had an `EnsureCreated()` fallback path that created ALL tables without recording ANY migration history
  2. `ExecuteSqlRawAsync` had a string interpolation bug (`$"...'{{{table}}}'..."`) causing `FormatException` at startup
  3. `docker-compose.app.yml` defaulted `DatabaseProvider` to `sqlserver` while connection string pointed to MariaDB
  4. `appsettings.Development.json` connection string pointed to remote server (192.168.0.9), not local Docker, causing `dotnet ef` from host to target wrong DB
- **Fix:**
  1. **Do NOT use `EnsureCreated()` for any database that will use migrations** — this was removed from Program.cs
  2. **Use the idempotent migration script for MariaDB** (safe retries):
    ```bash
    ./scripts/apply-migrations.sh --local --reset   # Full reset + apply
    ./scripts/apply-migrations.sh --remote           # Apply to remote
    ```
  3. **If tables exist without migration history**, drop and recreate the database:
    ```bash
    docker exec -i crm-mariadb mysql -u root -pRootPass@Dev2024 -e "DROP DATABASE IF EXISTS crm_db; CREATE DATABASE crm_db; GRANT ALL ON crm_db.* TO 'crm_user'@'%';"
    ```
  4. **Verify connection string target** before running `dotnet ef` — check `appsettings.Development.json` to confirm it points to the intended database server
- **Helper script:** `scripts/apply-migrations.sh` generates idempotent SQL via `dotnet ef migrations script --idempotent` and applies via MariaDB CLI, avoiding the transactional DDL problem.
- **Where seen:** Fresh Docker deployments, database resets, CI/CD pipeline failures

---

## Docker DatabaseProvider mismatch causes startup crash

- **Symptom:** API container fails to start or connects to wrong database. Logs may show connection errors or provider-specific SQL syntax errors.
- **Root Cause:** `docker-compose.app.yml` had `DatabaseProvider` defaulting to `sqlserver` while the connection string and dependent containers were MariaDB. This caused EF Core to use the wrong SQL dialect.
- **Fix:** Ensure `DatabaseProvider` environment variable matches the actual database:
  ```yaml
  DatabaseProvider: ${DB_PROVIDER:-mariadb}
  DB_HOST: ${DB_HOST:-crm-mariadb}
  DB_PORT: ${DB_PORT:-3306}
  ```
- **Where seen:** docker-compose.app.yml, docker-compose.unified.yml

---

## Multi-arch Docker build required for Mac → Linux deployment

- **Symptom:** Docker image built on Mac (arm64) crashes or fails on Linux server (amd64) with exec format errors.
- **Fix:** Always use multi-arch build for cross-platform deployment:
  ```bash
  docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend .
  ```
- **Transfer to remote:**
  ```bash
  docker save crm-api:latest | ssh root@192.168.0.9 "docker load"
  ```
- **Where seen:** Deploying from Mac development machine to `192.168.0.9` Linux server

---

## Frontend /admin/features page crashes: "Cannot read properties of undefined (reading 'isActive')"

- **Symptom:** TypeError at `/admin/features` admin page: `Cannot read properties of undefined (reading 'isActive')`. Page fails to render.
- **Root Cause (v0.560.7):** The `FeatureManagementTab` component expected `databaseStatus.demoDatabase` and `features.systemSettings` to always be present. After the demo database feature was deprecated (single database policy), the API no longer returns `demoDatabase` in the `/api/systemsettings/database/status` response.
- **Fix:**
  1. Made `demoDatabase` optional in the `DatabaseStatus` TypeScript interface
  2. Made `systemSettings` optional in the `FeatureConfiguration` interface  
  3. Added conditional rendering: `{databaseStatus.demoDatabase && (...)}`
  4. Used optional chaining: `features.systemSettings?.useDemoDatabase ?? false`
  5. Removed the Demo Mode toggle UI (deprecated feature)
- **File:** `CRM.Frontend/src/components/settings/FeatureManagementTab.tsx`
- **Related:** Demo database deprecation per single database policy in copilot-instructions.md
- **Where seen:** Admin settings → Features page after v0.560.7 demo database removal

---

## Superset shows no columns in datasets / "missing columns in datasource"

**Symptoms:**
- Superset dataset editor shows 0 columns for CRM tables
- Charts fail with "column not found" or show empty results
- Reports do not render despite data existing in MariaDB

**Root Causes (4 compounding issues — all present simultaneously):**

### 1. `fetch_metadata()` never called after dataset creation
The original `setup-superset-crm.py` script created datasets but never called `fetch_metadata()`, so Superset had zero column definitions from day one.

### 2. Malformed SQLAlchemy URI — `@` in password breaks host parsing
Password `CrmPass@Dev2024` contains `@`. Plain string SQLAlchemy URI `mysql://crm_user:CrmPass@Dev2024@crm-mariadb` is parsed as host = `Dev2024@crm-mariadb`, causing connection failure. Fix: URL-encode with `urllib.parse.quote_plus()`, which converts `@` to `%40`.

### 3. Wrong MySQL driver in URI
`mysql+pymysql://` fails because `pymysql` is not installed in `apache/superset:3.1.0`. Use `mysql+mysqldb://` (backed by `mysqlclient 2.1.0` which is installed).

### 4. Superset and MariaDB on different Docker networks
`crm-superset` → `crm-solution_crm-network`; `crm-mariadb` → `crm_crm-network`. Fix: connect Superset to the DB network. Applied permanently in `docker/docker-compose.providers.yml` via external network `crm-db-network`.

### 5. Wrong table names registered in Superset (after EF Core migrations)

| Old Name (wrong) | Correct Name |
|-----------------|--------------|
| `Customers` | `Accounts` |
| `SubscriptionUsage` | `SubscriptionUsages` |
| `SalesForecastHistory` | `ForecastHistories` |
| `EscalationRules` | `ITSMEscalationRules` |

**Fix — run the refresh script (handles all issues automatically):**
```bash
scp scripts/superset-refresh-datasources.py root@192.168.0.9:/tmp/
ssh root@192.168.0.9 "docker cp /tmp/superset-refresh-datasources.py crm-superset:/tmp/ && docker exec crm-superset python /tmp/superset-refresh-datasources.py"
```

**Run after every EF Core migration** to keep Superset column definitions in sync.

- **Files modified:** `scripts/superset-refresh-datasources.py` (new helper), `scripts/setup-superset-crm.py` (added `fetch_metadata()`), `docker/docker-compose.providers.yml` (network + driver fix)
- **Where seen:** v0.581.0, Feb 24 2026 — initial Superset setup via `setup-superset-crm.py`

---

## Category: Database / EF Core Migrations

### 6. MariaDB partial migration — "Table already exists" prevents API startup

**Symptom:** API container exits with code 139 (or an unhandled `MySqlException`) immediately at startup. Logs show:
```
[ERR] Failed executing DbCommand: CREATE TABLE `AIAgents` (...)
MySqlConnector.MySqlException: Table 'AIAgents' already exists
[FTL] Error during database setup. Startup aborted.
```
EF Core reports one pending migration (e.g. `20260225200552_InitialCreate`) even though the tables were already created.

**Root cause:** MariaDB/MySQL DDL statements (`CREATE TABLE`, `ALTER TABLE`, etc.) are **auto-committed** and cannot be rolled back. If a previous migration run failed partway through, some tables were created but the migration was never inserted into `__EFMigrationsHistory`. On the next startup EF tries to re-run the same `CREATE TABLE` statements and they fail.

**Fix — mark the migration as applied:**
```bash
docker exec crm-mariadb mariadb -u crm_user -pCrmPass@Dev2024 crm_db \
  -e "INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) \
      VALUES ('20260225200552_InitialCreate', '9.0.1');"
```
Replace `20260225200552_InitialCreate` and `9.0.1` with the pending migration ID and the .NET SDK version reported in the existing history rows. Then restart the API container.

**Helper script:** `scripts/fix-migration-state.sh` (created to automate this)

**Prevention:** The startup logs include an explicit warning:
> _"For MariaDB/MySQL, use scripts/apply-migrations.sh to generate and apply idempotent SQL."_
Use `dotnet ef migrations script --idempotent` to generate `IF NOT EXISTS`-style SQL rather than relying on `MigrateAsync()` in production.

- **Where seen:** v0.593.5, Feb 25 2026 — API crashed after MariaDB container was first started with schema already populated from a prior session.

---

## Category: SignalR / WebSockets

### 7. SignalR WebSocket fails: "connection not found on server"

**Symptom:** Browser console shows:
```
Error: Failed to start the transport 'WebSockets': Error: WebSocket failed to connect.
The connection could not be found on the server, either the endpoint may not be a
SignalR endpoint, the connection ID is not present on the server, or there is a
proxy blocking WebSockets.
```

**Root causes (three separate issues — all must be fixed):**

| # | Cause | Fix |
|---|-------|-----|
| 1 | `app.UseWebSockets()` missing from the middleware pipeline — Kestrel won't accept the 101 Switching Protocols handshake | Add `app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(15) })` **before** `app.UseRouting()` in `Program.cs` |
| 2 | `SecurityHeadersMiddleware` adds response headers to all requests, including the WebSocket 101 upgrade, corrupting the handshake | Add early return in `InvokeAsync` when `context.WebSockets.IsWebSocketRequest` or path starts with `/hubs` |
| 3 | nginx `/hubs/` location has no `proxy_buffering off` — nginx buffers prevent real-time WebSocket frame streaming | Add `proxy_buffering off; proxy_no_cache 1; proxy_cache_bypass 1;` to the `/hubs/` location in `nginx-frontend.conf` |

**Files modified:** `CRM.Backend/src/CRM.Api/Program.cs`, `CRM.Backend/src/CRM.Api/Middleware/SecurityHeadersMiddleware.cs`, `nginx-frontend.conf`

**After nginx config change, reload without downtime:**
```bash
docker exec crm-frontend nginx -s reload
```

- **Where seen:** v0.593.5, Feb 25 2026 — reported as browser console error after deployment to dev server.

---

## Category: Database / Migrations

### 8. Schema drift from short-circuited InitialCreate migration

**Symptom:** API endpoints return HTTP 400 (or 500) with generic error messages. Real error in container logs is:
```
MySqlException: Unknown column 'o.ClosedDate' in 'SELECT'
MySqlException: Table 'crm_db.AuthAuditLogs' doesn't exist
MySqlException: Table 'crm_db.UserSessions' doesn't exist
[WRN] Archive operation failed: Unknown column 'a.ArchivedAt' in 'SELECT'
```

**Root cause:** The `InitialCreate` migration was generated against an empty DB but the DB already had tables from earlier migrations. On startup it crashed with `Table 'AIAgents' already exists` and was manually marked as applied in `__EFMigrationsHistory` without executing. This left new columns and tables defined in the entity model missing from the live database:

| Missing Object | Table/Column | Type |
|---|---|---|
| `Opportunities.ClosedDate` | `Opportunities` | Column (plus 5 others) |
| `Competitors` | new table | Table |
| `OpportunityCompetitors` | new table | Table |
| `AuditLogs.ArchivedAt` | `AuditLogs` | Column |
| `AuthAuditLogs` | new table | Table (in unapplied `AddAuthAuditLogs` migration) |
| `UserSessions` | new table | Table |

**Fix — apply schema directly to DB:**
```bash
# 1. Create the fix SQL locally, scp to server, cp into container, execute
scp /tmp/fix-schema.sql root@192.168.0.9:/tmp/
ssh root@192.168.0.9 'docker cp /tmp/fix-schema.sql crm-mariadb:/tmp/ && docker exec crm-mariadb mariadb -u crm_user -pCrmPass@Dev2024 crm_db -e "source /tmp/fix-schema.sql"'
```

Fix SQL pattern:
```sql
-- For missing columns:
ALTER TABLE Opportunities ADD COLUMN IF NOT EXISTS ClosedDate datetime(6) NULL;
ALTER TABLE AuditLogs ADD COLUMN IF NOT EXISTS ArchivedAt datetime(6) NULL;

-- For missing tables:
CREATE TABLE IF NOT EXISTS AuthAuditLogs ( Id int NOT NULL AUTO_INCREMENT, ... );
CREATE TABLE IF NOT EXISTS UserSessions ( Id int NOT NULL AUTO_INCREMENT, ... );

-- For FKs (MariaDB does NOT support ADD CONSTRAINT IF NOT EXISTS — must use standard):
ALTER TABLE Opportunities ADD CONSTRAINT FK_Opportunities_Competitors
    FOREIGN KEY (CompetitorWinnerId) REFERENCES Competitors(Id);

-- Register unapplied migration:
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260225222952_AddAuthAuditLogs', '9.0.1');
```

**Key MariaDB DDL quirks:**
- `ALTER TABLE ... ADD COLUMN IF NOT EXISTS` — ✅ supported (MariaDB 10.x+)
- `CREATE TABLE IF NOT EXISTS` — ✅ supported
- `CREATE INDEX IF NOT EXISTS` — ✅ supported (MariaDB 10.1.4+)
- `ADD CONSTRAINT IF NOT EXISTS` — ❌ NOT supported (MySQL 8.0+ only); use plain `ADD CONSTRAINT` after checking existence manually

**Prevention:** When a migration crashes on startup with `Table X already exists`, DO NOT just mark it applied in `__EFMigrationsHistory`. Instead:
1. Drop the partially-created tables EF made before the crash
2. Re-run `dotnet ef database update` so the full migration executes cleanly
   
**Where seen:** v0.593.5-0.593.6, Feb 25-26 2026 — `/api/tasks/my-queue` returned 400; audit archive warnings logged.

---

### Issue 1b: MariaDB "Duplicate column name" on EF Migrations (partial apply from prior crash)

- **Symptom:** `crm-api` restart-loops after a fresh deploy; `docker logs crm-api` shows `Duplicate column name 'ColumnName'` inside EF Core migration runner.
- **Root cause:** A previous deploy crashed mid-migration. Because MariaDB auto-commits every DDL statement (no transaction rollback), some `ADD COLUMN` statements from the migration executed and committed before the crash. The migration is NOT recorded in `__EFMigrationsHistory`, so EF tries to re-run it from the beginning, failing on the already-existing column.
- **Fix (idempotent repair SQL):**
  ```sql
  SET FOREIGN_KEY_CHECKS = 0;

  -- Re-apply each AddColumn idempotently
  ALTER TABLE `AffectedTable` ADD COLUMN IF NOT EXISTS `ColumnName` int NULL;
  -- ... repeat for all columns in the failing migration ...

  -- Re-create any new tables with IF NOT EXISTS
  CREATE TABLE IF NOT EXISTS `NewTable` ( ... ) CHARACTER SET utf8mb4;

  -- Mark the migration as applied so EF skips it
  INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
  VALUES ('20260228053352_AddScriptRegistryEnhancements', '10.0.0');

  SET FOREIGN_KEY_CHECKS = 1;
  ```
  Apply: `docker exec -i crm-mariadb mariadb -u crm_user -pCrmPass@Dev2024 crm_db < /tmp/repair.sql`  
  Then `docker restart crm-api` and let EF apply any remaining clean migrations.
- **Important:** Only mark the FAILING migration as applied. Any subsequent unapplied clean migrations (e.g., the Marketing migration) should be left for EF to apply normally.
- **Helper script:** `/tmp/gen_repair_sql.py` (parses C# migration files and generates idempotent SQL automatically)
- **Prevention:** Ensure the deploy script uses `--force-recreate` on containers and that all new migrations are smoke-tested locally against a fresh MariaDB volume before pushing to the dev server.
- **Where seen:** v0.610.1, Feb 28 2026 — `AddScriptRegistryEnhancements` partially pre-applied `PriorityId` column; repair SQL fixed + Marketing migration applied cleanly.

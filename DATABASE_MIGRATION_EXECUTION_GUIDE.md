# Database Migration Execution Guide

## QUICK START: Create and Apply Migrations for SYS-004, SYS-010, SYS-011

### One-Command Migration (Automated)

```bash
cd CRM.Backend

# Create migration with automatic naming
dotnet ef migrations add "AddSystemFeatureEntities" \
  --context CrmDbContext \
  --output-dir "Migrations" \
  --verbose

# Apply migration to database
dotnet ef database update --verbose
```

---

## Step-by-Step Execution

### Step 1: Generate Migration

```bash
cd CRM.Backend

# Generate the migration code
dotnet ef migrations add "AddSystemFeatureEntities" --context CrmDbContext

# Output should show:
# To undo this action, use 'ef migrations remove'
```

**What this does:**
- Creates new migration file: `Migrations/YYYYMMDDHHMMSS_AddSystemFeatureEntities.cs`
- Generates `Up()` method with CREATE TABLE statements for 5 new entities
- Generates `Down()` method for rollback capability
- Updates `CrmDbContextModelSnapshot.cs` with schema state

### Step 2: Verify Migration File

```bash
# List all migrations
dotnet ef migrations list

# Output should include: AddSystemFeatureEntities (Pending)

# Show migration code
cat Migrations/[TIMESTAMP]_AddSystemFeatureEntities.cs | head -100
```

**Verify these table creations are in the migration:**
- ✅ CreateTable("FeatureFlagAuditLogs")
- ✅ CreateTable("UIPreferences")
- ✅ CreateTable("UICustomizations")
- ✅ CreateTable("DashboardCustomizations")
- ✅ CreateTable("PerformanceMetrics")

**Verify these indexes are created:**
- ✅ IX_FeatureFlagAuditLogs_FlagName
- ✅ IX_FeatureFlagAuditLogs_ChangedAt
- ✅ IX_UIPreferences_UserId
- ✅ IX_UICustomizations_UserId_ModuleName_PageName
- ✅ IX_DashboardCustomizations_IsDefault
- ✅ IX_PerformanceMetrics_EndpointName
- ✅ IX_PerformanceMetrics_RequestTime

### Step 3: Backup Database (IMPORTANT!)

```bash
# MariaDB backup
mysqldump -u crm_user -p crm_db > crm_db_backup_$(date +%Y%m%d_%H%M%S).sql

# SQL Server backup
sqlcmd -S localhost -U crm_user -P [password] -Q "BACKUP DATABASE [crm_db] TO DISK='C:\Backups\crm_db_$(Get-Date -Format yyyyMMdd_HHmmss).bak'"

# PostgreSQL backup
pg_dump -U crm_user crm_db > crm_db_backup_$(date +%Y%m%d_%H%M%S).sql
```

### Step 4: Apply Migration

```bash
# Apply to development database
dotnet ef database update --context CrmDbContext

# For specific target migration (if needed)
dotnet ef database update "AddSystemFeatureEntities" --context CrmDbContext

# With verbose output to see SQL being executed
dotnet ef database update --verbose
```

**Output should show:**
```
Applying migration '[TIMESTAMP]_AddSystemFeatureEntities'.
Done.
```

### Step 5: Verify Tables Created

#### MariaDB Verification
```sql
-- Connect to database
mysql -u crm_user -p crm_db

-- Show all tables
SHOW TABLES LIKE '%Audit%' OR TABLES LIKE '%Preference%' OR TABLES LIKE '%Performance%';

-- Verify table structure
DESCRIBE FeatureFlagAuditLogs;
DESCRIBE UIPreferences;
DESCRIBE UICustomizations;
DESCRIBE DashboardCustomizations;
DESCRIBE PerformanceMetrics;

-- Check indexes
SHOW INDEX FROM FeatureFlagAuditLogs;
SHOW INDEX FROM PerformanceMetrics;

-- Verify column data types
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'FeatureFlagAuditLogs' AND TABLE_SCHEMA = 'crm_db';
```

#### SQL Server Verification
```sql
-- Connect to database
sqlcmd -S localhost -U crm_user -P [password] -d crm_db

-- Verify tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('FeatureFlagAuditLogs', 'UIPreferences', 'UICustomizations', 
                      'DashboardCustomizations', 'PerformanceMetrics');

-- Show table schema
EXEC sp_help 'dbo.FeatureFlagAuditLogs';
EXEC sp_help 'dbo.UIPreferences';
EXEC sp_help 'dbo.PerformanceMetrics';

-- Verify indexes
SELECT name, type_desc FROM sys.indexes 
WHERE object_id = OBJECT_ID('dbo.PerformanceMetrics');
```

#### PostgreSQL Verification
```sql
-- Connect to database
psql -U crm_user -d crm_db

-- List all tables
\dt

-- Show table structure
\d FeatureFlagAuditLogs
\d UIPreferences
\d UICustomizations
\d DashboardCustomizations
\d PerformanceMetrics

-- List indexes
\di

-- Show indexes for specific table
SELECT indexname FROM pg_indexes WHERE tablename = 'PerformanceMetrics';
```

### Step 6: Run Tests

```bash
# Run all backend tests
cd CRM.Backend
dotnet test

# Run only service layer tests (to verify entity integration)
dotnet test --filter "namespace=CRM.Tests.Services"

# Expected output:
# 8 FeatureFlagManagementServiceTests passed ✓
# 5 UserInterfaceServiceTests passed ✓
# 4 PerformanceOptimizationServiceTests passed ✓
```

### Step 7: Verify API Connectivity

```bash
# Start the API
cd CRM.Backend/src/CRM.Api
dotnet run

# In another terminal, test endpoints (after authentication with JWT token)

# Test Feature Flag endpoint
curl -X GET http://localhost:5000/api/feature-flags \
  -H "Authorization: Bearer [JWT_TOKEN]"

# Test UI Preferences endpoint
curl -X GET http://localhost:5000/api/ui-preferences \
  -H "Authorization: Bearer [JWT_TOKEN]"

# Test Performance endpoint
curl -X GET http://localhost:5000/api/performance/dashboard \
  -H "Authorization: Bearer [JWT_TOKEN]"

# Expected responses:
# GET /api/feature-flags: 200 OK with flag array
# GET /api/ui-preferences: 404 Not Found (no preferences yet - normal)
# GET /api/performance/dashboard: 200 OK with empty/zero dashboard
```

---

## Troubleshooting Migration Issues

### Issue: "No database provider has been configured"

**Cause:** EF Core doesn't know which database provider to use

**Solution:**
```bash
# Verify appsettings.json has ConnectionStrings
cat appsettings.Development.json | grep -A 5 "ConnectionStrings"

# Verify Program.cs registers DbContext with provider
grep -A 3 "AddDbContext" src/CRM.Api/Program.cs

# Should show one of:
#   UseSqlServer
#   UseNpgsql (PostgreSQL)
#   UseMySql (MariaDB)
```

### Issue: "There are pending model changes"

**Cause:** Entity definitions changed but migration not created

**Solution:**
```bash
# Remove last migration if not yet applied
dotnet ef migrations remove

# Recreate migration
dotnet ef migrations add "AddSystemFeatureEntities"

# Or view pending changes
dotnet ef migrations list
```

### Issue: "Foreign key constraint fails"

**Cause:** User table not defined before adding FK to it

**Solution:**
```bash
# Verify User table exists
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users';

# If not, user table migration must run first
# Check migration history
dotnet ef migrations list

# Apply all migrations in order
dotnet ef database update
```

### Issue: "Migration already applied"

**Cause:** Migration file exists but database shows "Applied"

**Solution:**
```bash
# Show migration status
dotnet ef migrations list

# If already applied to database, skip to Step 7 (verify tests)

# If stuck in "Pending" state, check database __EFMigrationsHistory table
SELECT * FROM dbo.__EFMigrationsHistory ORDER BY AppliedOn DESC;

# Manually record migration (only if absolutely necessary)
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260217120000_AddSystemFeatureEntities', '10.0.0');
```

### Issue: "Timeout expired"

**Cause:** Database migration taking too long (large tables)

**Solution:**
```bash
# Increase timeout (default is 30 seconds)
dotnet ef database update --command-timeout 120

# Or in code:
# var options = new DbContextOptionsBuilder<CrmDbContext>()
#     .UseSqlServer(connection, x => x.CommandTimeout(120))
#     .Options;
```

### Issue: "Permission denied" on Linux/Mac

**Cause:** EF Core executable not executable

**Solution:**
```bash
# Add execute permission
chmod +x $HOME/.dotnet/tools/dotnet-ef

# Or use full path
~/.dotnet/tools/dotnet-ef migrations add "AddSystemFeatureEntities"
```

---

## Rollback Procedure

If migration needs to be undone:

### Option A: Remove Last Migration (Before Applied)

```bash
# If migration not yet applied to database
dotnet ef migrations remove

# This will:
# - Delete the migration file
# - Update Migration snapshot
# - Leave database unchanged
```

### Option B: Revert Migration on Database

```bash
# Revert to previous migration
dotnet ef database update [PREVIOUS_MIGRATION_NAME]

# Example: if previous migration was 20260210120000_UpdateUserEntity
dotnet ef database update 20260210120000_UpdateUserEntity

# This will:
# - Execute Down() method of removed migration
# - Drop new tables (FeatureFlagAuditLogs, UIPreferences, etc.)
# - Restore database to previous state
```

### Option C: Emergency Rollback

```bash
# Restore from backup
mysql -u crm_user -p crm_db < crm_db_backup_20260217_120000.sql

# Verify database state
SELECT * FROM dbo.__EFMigrationsHistory ORDER BY AppliedOn DESC;

# Update EF Core state to match database
dotnet ef database update [MATCHING_MIGRATION]
```

---

## Performance Tuning After Migration

### Check Index Usage

```sql
-- MariaDB
SELECT * FROM INFORMATION_SCHEMA.STATISTICS 
WHERE TABLE_SCHEMA = 'crm_db' 
AND TABLE_NAME IN ('FeatureFlagAuditLogs', 'PerformanceMetrics');

-- SQL Server
SELECT name, type_desc, is_unique FROM sys.indexes 
WHERE object_id IN (OBJECT_ID('dbo.FeatureFlagAuditLogs'), 
                     OBJECT_ID('dbo.PerformanceMetrics'));

-- Make sure IX_PerformanceMetrics_RequestTime DESC exists (for fast time-based queries)
```

### Monitor Table Growth

```sql
-- Check row count per table
SELECT 
    TABLE_NAME,
    TABLE_ROWS as 'Row Count',
    ROUND((DATA_LENGTH+INDEX_LENGTH)/1024/1024, 2) as 'Size MB'
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'crm_db'
AND TABLE_NAME IN ('FeatureFlagAuditLogs', 'UIPreferences', 'UICustomizations', 
                    'DashboardCustomizations', 'PerformanceMetrics')
ORDER BY TABLE_ROWS DESC;
```

### Enable Query Metrics Collection

Set up background job to auto-purge old PerformanceMetrics:

```csharp
// In Program.cs after services registered:
using var scope = app.Services.CreateScope();
var performanceService = scope.ServiceProvider.GetRequiredService<IPerformanceOptimizationService>();

// Schedule daily purge task
_ = Task.Run(async () => {
    while (true) {
        try {
            await performanceService.PurgeOldMetricsAsync(30); // Keep 30 days
            await Task.Delay(TimeSpan.FromHours(24)); // Run once per day
        } catch (Exception ex) {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error in metrics purge job");
        }
    }
});
```

---

## Deployment to Production

### Pre-Deployment Checklist
- [ ] Migration tested on staging database
- [ ] Database backup created
- [ ] All unit tests passing
- [ ] Rollback procedure documented and tested
- [ ] Team notified of planned deployment
- [ ] Maintenance window scheduled (if needed)
- [ ] Monitoring alerts setup for new tables

### Deployment Steps
1. Create backup: `mysqldump -u crm_user -p crm_db > backup_predeployment.sql`
2. Generate migration locally and verify
3. Deploy API code changes
4. Apply migration: `dotnet ef database update`
5. Run health checks
6. Monitor error logs for 1 hour
7. Verify telemetry data flowing to PerformanceMetrics table

### Post-Deployment Validation
```bash
# Verify all tables accessible
curl -X GET https://yourapiendpoint/api/feature-flags \
  -H "Authorization: Bearer [PROD_JWT_TOKEN]"

# Check for errors in application logs
tail -f /var/log/crm-api/app.log | grep -i "error\|exception"

# Verify metrics are being recorded
SELECT COUNT(*) FROM PerformanceMetrics WHERE CreatedAt > DATE_SUB(NOW(), INTERVAL 5 MINUTE);
# Should return: > 0 (meaning metrics are being recorded)

# Monitor for slowness
SELECT AVG(ResponseTimeMs) as 'Avg Response Time (ms)' 
FROM PerformanceMetrics 
WHERE CreatedAt > DATE_SUB(NOW(), INTERVAL 1 HOUR);
# Should return: < 500ms (per SLA target)
```

---

## Next Steps After Migration

1. ✅ Database migration applied
2. 📱 Deploy frontend components
3. 🧪 Run frontend integration tests  
4. 📊 Monitor production metrics for 24 hours
5. 🎓 Update documentation and release notes
6. 📢 Notify stakeholders of feature availability
7. 🔄 Schedule Phase 2 enhancements (Q2 2026)

---

*Migration Guide for SYS-004, SYS-010, SYS-011 Implementation*
*Generated: February 17, 2026*

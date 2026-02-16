# System Module Database Migration - Execution Checklist

**Date:** February 15, 2026  
**Migration ID:** 20260215T160000  
**Tables:** 11 New Tables, 25 Indexes, 14 Foreign Keys

---

## ✅ Pre-Execution Requirements

### Code Changes Completed
- [x] Migration file created: `20260215T160000_AddSystemModuleEntities.cs`
- [x] Migration designer file created: `20260215T160000_AddSystemModuleEntities.Designer.cs`  
- [x] Entity class created: `FeatureFlag.cs` (includes FeatureFlagVariant)
- [x] DbContext updated with new DbSet properties:
  - [ ] DbSet<FeatureFlag>
  - [ ] DbSet<FeatureFlagVariant>

### Report Generated
- [x] `SYSTEM_MODULE_MIGRATIONS_REPORT.md` - Complete migration documentation

---

## ⚠️ BLOCKING ISSUES - MUST FIX FIRST

### Compilation Errors: **101 Total Errors**

```
Error Summary:
- Missing 'using Microsoft.Extensions.Logging;' directives (8 files)
- Ambiguous type references: EscalationRule, SLAPolicy, ServiceQueue (7 instances)
- Missing DTO classes: EscalationLevelDto, EscalationHistoryDto, EscalationRuleFilterDto
- Unimplemented interface members (12 services)
```

### Files Requiring Fixes
- [ ] `src/CRM.Infrastructure/Services/ITSM/EscalationPolicyService.cs` - Add ILogger using, implement interface methods
- [ ] `src/CRM.Infrastructure/Services/ITSM/EscalationRuleService.cs` - Add ILogger using, implement interface methods
- [ ] `src/CRM.Infrastructure/Services/ITSM/EscalationRuleAdminService.cs` - Fix ambiguous references
- [ ] `src/CRM.Infrastructure/Services/ITSM/SLAPolicyAdminService.cs` - Fix ambiguous references, ILogger
- [ ] `src/CRM.Infrastructure/Services/ITSM/SLAService.cs` - Fix ambiguous references
- [ ] `src/CRM.Infrastructure/Services/ITSM/ServiceQueueService.cs` - Fix ambiguous references
- [ ] `src/CRM.Infrastructure/Services/UserInterfaceService.cs` - Add ILogger using
- [ ] `src/CRM.Core/Dtos/` - Create missing DTO classes

### Quick Fix Script
```bash
# Step 1: Add missing using directives
cd CRM.Backend/src

# Edit these files and add:
# using Microsoft.Extensions.Logging;

# Step 2: Fix ambiguous references using full namespace qualification
# Change: EscalationRule → ITSM.EscalationRule (or CRM.Core.Entities.EscalationRule)
# Change: SLAPolicy → ITSM.SLAPolicy  
# Change: ServiceQueue → ITSM.ServiceQueue

# Step 3: Verify build
dotnet build CRM.sln
```

---

## 📋 Database Application Sequence

### Phase 1️⃣: Pre-Migration Verification
```bash
# Navigate to CRM.Backend directory
cd "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend"

# Verify database connection string configured in appsettings.json
cat src/CRM.Api/appsettings.json | grep -A 5 "ConnectionStrings"

# Expected output:
# "ConnectionStrings": {
#   "DefaultConnection": "Server=...;Database=crm_db;..."
# }
```

### Phase 2️⃣: Clean Build
```bash
# Clean previous build artifacts
dotnet clean CRM.sln

# Restore packages
dotnet restore

# Build solution (MUST SUCCEED - no errors allowed)
dotnet build CRM.sln --configuration Debug
```

**Expected Result:** ✅ Build succeeded

### Phase 3️⃣: Apply Migrations
```bash
# List pending migrations
dotnet ef migrations list --project src/CRM.Infrastructure

# Apply all pending migrations
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api

# Or apply specific migration
dotnet ef database update AddSystemModuleEntities --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

**Expected Result:**
```
Apply migration '20260215T160000_AddSystemModuleEntities'
Done.
```

---

## 🔍 Post-Migration Verification

### Step 1️⃣: Verify Table Creation
```sql
-- SQL Server / T-SQL
SELECT COUNT(*) as TableCount 
FROM information_schema.tables 
WHERE table_name IN (
  'Roles', 'Permissions', 'RolePermissions', 'UserRoleAssignments',
  'UserGroups', 'UserGroupMembers', 'FeatureFlags', 'FeatureFlagVariants',
  'UIPreferences', 'UICustomizations', 'PerformanceMetrics'
);

-- Expected Result: 11 rows
```

### Step 2️⃣: Verify Schema Structure
```sql
-- Check Roles table
EXEC sp_help 'Roles';

-- Expected columns:
-- Id (int, PK)
-- Name (nvarchar(256))
-- Description (nvarchar(500))
-- HierarchyLevel (int)
-- IsSystemDefined (bit)
-- IsActive (bit)
-- CreatedAt (datetime2)
-- UpdatedAt (datetime2)
-- IsDeleted (bit)
-- RowVersion (timestamp)
```

### Step 3️⃣: Verify Indexes
```sql
-- List all indexes on new tables
SELECT object_name(i.object_id) as TableName, i.name as IndexName, i.type_desc
FROM sys.indexes i
WHERE object_name(i.object_id) IN ('Roles', 'Permissions', 'RolePermissions', 
  'UserRoleAssignments', 'UserGroups', 'UserGroupMembers', 'FeatureFlags', 
  'FeatureFlagVariants', 'UIPreferences', 'UICustomizations', 'PerformanceMetrics')
ORDER BY object_name(i.object_id), i.name;

-- Expected Result: 25 indexes total
```

### Step 4️⃣: Verify Foreign Keys
```sql
-- List all foreign keys
SELECT constraint_name, table_name, referenced_table_name
FROM information_schema.referential_constraints
WHERE table_name IN (
  'RolePermissions', 'UserRoleAssignments', 'UserGroupMembers',
  'UIPreferences', 'UICustomizations', 'FeatureFlagVariants', 'PerformanceMetrics'
)
ORDER BY table_name, constraint_name;

-- Expected Result: 14 foreign keys total
```

### Step 5️⃣: Verify Unique Constraints
```sql
-- Check unique constraints
SELECT object_name(object_id) as TableName, name as ConstraintName
FROM sys.key_constraints
WHERE type = 'UQ'
AND object_name(object_id) IN ('Roles', 'Permissions', 'RolePermissions',
  'UserRoleAssignments', 'UserGroups', 'UIPreferences', 'UICustomizations',
  'FeatureFlags', 'FeatureFlagVariants')
ORDER BY object_name(object_id);

-- Expected Result: 8 unique constraints/indexes
```

### Step 6️⃣: Test Soft Delete (IsDeleted)
```sql
-- Verify all tables have IsDeleted column
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
  'Roles', 'Permissions', 'RolePermissions', 'UserRoleAssignments',
  'UserGroups', 'UserGroupMembers', 'FeatureFlags', 'FeatureFlagVariants',
  'UIPreferences', 'UICustomizations', 'PerformanceMetrics'
)
AND COLUMN_NAME = 'IsDeleted'
ORDER BY TABLE_NAME;

-- Expected Result: 11 rows (one for each table)
```

### Step 7️⃣: Test Data Integrity
```sql
-- Verify cascade delete configuration
SELECT name, OBJECT_NAME(parent_object_id) as TableName
FROM sys.foreign_keys
WHERE OBJECT_NAME(parent_object_id) IN ('RolePermissions', 'UserRoleAssignments', etc.)
AND delete_referential_action_desc = 'CASCADE';

-- Test cascade delete (in transaction - rollback)
BEGIN TRANSACTION;
  DELETE FROM Roles WHERE Id = [test_id];
  -- Verify RolePermissions records were deleted
  SELECT COUNT(*) FROM RolePermissions WHERE RoleId = [test_id];
ROLLBACK;
```

---

## 🚨 Troubleshooting Guide

### Issue: "Build failed with 101 errors"
**Solution:**
1. Fix all compilation errors (see Blocking Issues section)
2. Rebuild: `dotnet build CRM.sln`
3. Verify no remaining errors: `dotnet build CRM.sln 2>&1 | grep error`

### Issue: "No database provider matches..."
**Solution:**
1. Verify `DatabaseProvider` in appsettings.json
2. Verify connection string is correct
3. Check database server is running:
   ```bash
   # For SQL Server (local)
   sqlcmd -S (local) -E

   # For MariaDB
   mysql -h crm-mariadb -u crm_user -p
   ```

### Issue: "Pending model changes..."
**Solution:**
```bash
# Add new migration to capture model changes
# Note: Skip this if applying pre-created migration
dotnet ef migrations add MigrationName --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

### Issue: "Foreign key referential action conflict"
**Solution:**
1. Verify all related tables exist before migration
2. Check proper table creation order (Parents before children)
3. Review migration Up() and Down() methods

### Issue: "Unique constraint violation"
**Solution:**
1. Verify no duplicate data in source tables
2. Check unique indexes created correctly
3. Run verification query: 
```sql
SELECT DISTINCT Column FROM Table HAVING COUNT(*) > 1
```

---

## 📊 Rollback Plan

### If migration fails (reverting to previous state):

```bash
# Option 1: Rollback to previous migration
dotnet ef database update 20260215_AddSubscriptionBillingEntities --project src/CRM.Infrastructure

# Option 2: Remove migration from identity
# (Only if NOT applied to database yet)
dotnet ef migrations remove --project src/CRM.Infrastructure

# Option 3: Manual rollback (Emergency)
# Execute Down() script manually in SQL Management Studio
```

### Database Snapshot (Pre-Migration)
**Recommended:** Create a database backup before running migration:
```sql
-- SQL Server
BACKUP DATABASE [crm_db] TO DISK = N'C:\Backups\crm_db_20260215_pre_migration.bak'

-- MariaDB
mysqldump -h crm-mariadb -u crm_user -p crm_db > crm_db_20260215_pre_migration.sql
```

---

##✅ Final Checklist

Before executing the migration, verify:

- [ ] All 101 compilation errors fixed
- [ ] Solution builds successfully: `dotnet build CRM.sln`
- [ ] Database connection tested and working
- [ ] Database backup created (if production)
- [ ] Migration files exist:
  - [ ] `20260215T160000_AddSystemModuleEntities.cs` (30KB+)
  - [ ] `20260215T160000_AddSystemModuleEntities.Designer.cs` (1KB+)
- [ ] Entity classes created:
  - [ ] `FeatureFlag.cs` (in CRM.Core/Entities)
- [ ] DbContext updated:
  - [ ] DbSet<FeatureFlag> added
  - [ ] DbSet<FeatureFlagVariant> added

**Migration Ready:** ✅ YES / ❌ NO

---

## 📝 Execution Log

**Start Time:** _______________  
**Completion Time:** _______________  
**Duration:** _______________  

**Build Result:** ✅ SUCCESS / ❌ FAILED  
**Migration Result:** ✅ SUCCESS / ❌ FAILED  
**Verification Result:** ✅ PASSED / ❌ FAILED  

**Notes/Issues:**
```
[Space for notes]
```

**Executed By:** _______________  
**Date:** _______________  

---

## 📞 Support Contacts

- **Database Admin:** [Database Administrator Contact]
- **Backend Team Lead:** [Backend Lead Contact]
- **DevOps/Deployment:** [DevOps Contact]

---

**Document Version:** 1.0  
**Last Updated:** February 15, 2026  
**Status:** READY FOR EXECUTION (pending compilation fixes)

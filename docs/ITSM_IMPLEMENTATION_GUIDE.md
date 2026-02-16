# ITSM Database Implementation Guide

> **Created:** February 15, 2026  
> **Version:** 1.0  
> **Last Updated:** February 15, 2026

## Overview

This guide provides step-by-step instructions for deploying the ITSM, Marketing, and Integration database schema to production environments. It covers all three supported databases (MariaDB, SQL Server, PostgreSQL) and includes validation, rollback, and troubleshooting procedures.

**Deliverables Covered:**
- 21 new database tables (13 ITSM + 8 Marketing/Integration)
- 51 optimized indexes for query performance
- 35 data integrity constraints  
- 3 complete database schema scripts (one per supported database)
- 1 EF Core migration file supporting all three databases

---

## Table of Contents

1. [Pre-Deployment Checklist](#pre-deployment-checklist)
2. [Deployment Methods](#deployment-methods)
3. [Deployment Procedures](#deployment-procedures)
4. [Post-Deployment Validation](#post-deployment-validation)
5. [Troubleshooting](#troubleshooting)
6. [Rollback Procedures](#rollback-procedures)
7. [Performance Optimization](#performance-optimization)
8. [FAQ](#faq)

---

## Pre-Deployment Checklist

Before deploying any schema changes, complete the following:

### Backup & Recovery
- [ ] **Full database backup taken** - Create complete backup before any schema changes
  ```bash
  # MariaDB
  mysqldump -h crm-mariadb -u crm_user -p crm_db > crm_db_backup_$(date +%Y%m%d_%H%M%S).sql
  
  # SQL Server (PowerShell)
  Backup-SqlDatabase -ServerInstance "sql-server" -Database "crm_db" -BackupFile "C:\Backups\crm_db_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
  
  # PostgreSQL
  pg_dump -h crm-postgresql -U crm_user -d crm_db > crm_db_backup_$(date +%Y%m%d_%H%M%S).sql
  ```

- [ ] **Backup stored in secure location** - Verify backup file exists and is accessible
- [ ] **Recovery procedure tested** - Confirm backup can be restored successfully
- [ ] **Database size noted** - Document current database size for impact assessment

### Environment Verification
- [ ] **Database connection verified** - Can establish connection to target database
- [ ] **Sufficient disk space** - At least 2x current database size available
- [ ] **No active locks on tables** - Check for long-running transactions
  ```sql
  -- MariaDB
  SHOW OPEN TABLES WHERE In_use > 0;
  
  -- SQL Server
  EXEC sp_whoisactive;
  
  -- PostgreSQL
  SELECT * FROM pg_stat_activity WHERE state != 'idle';
  ```

- [ ] **Application deployment halted** - No new transactions during migration
- [ ] **All existing migrations applied** - Database is at latest schema version
  ```bash
  cd CRM.Backend && dotnet ef database update --verbose
  ```

### Permission Verification
- [ ] **User has DDL privileges** - CREATE TABLE, CREATE INDEX, ALTER permissions
- [ ] **Foreign key constraints can be created** - Prerequisite tables exist
- [ ] **Test environment deployed first** - Migration tested in non-production
- [ ] **Team notified** - Stakeholders aware of maintenance window

---

## Deployment Methods

Choose one of the following deployment methods based on your environment:

### Method 1: Entity Framework Core Migration (Recommended)

**Best for:** Development, Staging, and Production environments with CI/CD pipelines  
**Advantages:** Database-agnostic, reversible, version-controlled, part of application code  
**Time complexity:** Low (automated)

```bash
# Apply migration
cd CRM.Backend
dotnet ef database update

# Verify migration applied
dotnet ef migrations list

# Show pending migrations
dotnet ef migrations pending
```

### Method 2: Direct SQL Execution

**Best for:** Legacy systems, manual deployments, database-only updates  
**Advantages:** Direct control, no .NET dependency  
**Database-specific:** Requires choosing correct SQL script

- **MariaDB:** Use `itsm_marketing_integration_mariadb.sql`
- **SQL Server:** Use `itsm_marketing_integration_sqlserver.sql`
- **PostgreSQL:** Use `itsm_marketing_integration_postgresql.sql`

### Method 3: CI/CD Pipeline

**Best for:** Production deployments with automated testing  
**Advantages:** Integrated with build process, enforces compliance  
**Tools supported:** GitHub Actions, Azure DevOps, GitLab CI

---

## Deployment Procedures

### Procedure 1: EF Core Migration Deployment

**Step 1: Build and test locally**
```bash
# Navigate to backend directory
cd CRM.Backend

# Build solution
dotnet build

# Run unit tests
dotnet test tests/CRM.Tests

# Run integration tests
dotnet test tests/CRM.Tests --filter "Category=Integration"
```

**Step 2: Test migration (dry-run)**
```bash
# Generate SQL without executing
dotnet ef migrations script 0 20260215T180000_AddITSMMarketingIntegrationTables --output migration_preview.sql

# Review generated SQL
cat migration_preview.sql | head -50
```

**Step 3: Apply migration to development database**
```bash
# Set environment to Development
export ASPNETCORE_ENVIRONMENT=Development

# Apply migration
dotnet ef database update --verbose
```

**Step 4: Validate in development**
```bash
# Run smoke tests
dotnet test tests/CRM.Tests --filter "Category=SmokeTest"

# Check schema integrity
# (See validation section for detailed queries)
```

**Step 5: Deploy to staging environment**
```bash
# Update connection string for staging
export ConnectionStrings__DefaultConnection="Server=staging-db;Port=3306;Database=crm_db_staging;User=crm_user;Password=XXX;"

# Apply migration
dotnet ef database update

# Run full test suite
dotnet test tests/CRM.Tests --verbose
```

**Step 6: Schedule production deployment**
```bash
# Create maintenance window
# - Notify users of downtime (24+ hours notice)
# - Document rollback plan with team
# - Schedule post-deployment validation

# Set environment to Production
export ASPNETCORE_ENVIRONMENT=Production

# Apply migration
dotnet ef database update --verbose

# Verify successful deployment
dotnet ef migrations list  # Should show migration as applied
```

---

### Procedure 2: Direct SQL Deployment

#### For MariaDB

**Step 1: Connect to database**
```bash
mysql -h crm-mariadb -u crm_user -p crm_db
```

**Step 2: Review schema additions**
```sql
-- Check if tables exist (should show 0 results before deployment)
SHOW TABLES LIKE 'ITSM_%';

-- Check for existing Problems table
SELECT COUNT(*) as Table_Count FROM information_schema.TABLES 
  WHERE TABLE_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%';
```

**Step 3: Execute schema script**
```bash
# Source the SQL file
mysql -h crm-mariadb -u crm_user -p crm_db < database/schema/itsm_marketing_integration_mariadb.sql

# Monitor execution (for large scripts)
pv database/schema/itsm_marketing_integration_mariadb.sql | mysql -h crm-mariadb -u crm_user -p crm_db
```

**Step 4: Verify tables created**
```sql
-- Should show 13 ITSM tables + indexes on webhooks
SHOW TABLES LIKE 'ITSM_%';

-- Count indexes created
SELECT COUNT(*) FROM information_schema.STATISTICS 
  WHERE TABLE_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%';

-- Expected: 51 indexes
```

#### For SQL Server

**Step 1: Connect using SSMS or sqlcmd**
```powershell
sqlcmd -S sql-server -U crm_user -P "password" -d crm_db
```

**Step 2: Review schema additions**
```sql
-- Check if tables exist (should show 0 results before deployment)
SELECT COUNT(*) FROM information_schema.TABLES 
  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME LIKE 'ITSM_%';

-- Check for existing indexes
SELECT COUNT(*) FROM sys.indexes 
  WHERE object_id IN (
    SELECT object_id FROM sys.tables 
    WHERE name LIKE 'ITSM_%'
  );
```

**Step 3: Execute schema script**
```powershell
# Using sqlcmd
sqlcmd -S sql-server -U crm_user -P "password" -d crm_db -i database/schema/itsm_marketing_integration_sqlserver.sql

# Or in SSMS
# File → Open → itsm_marketing_integration_sqlserver.sql
# Execute script (Ctrl+Shift+E)
```

**Step 4: Verify tables created**
```sql
-- Should show 13 ITSM tables
SELECT name FROM sys.tables WHERE name LIKE 'ITSM_%' ORDER BY name;

-- Count indexes created
SELECT COUNT(*) FROM sys.indexes 
  WHERE object_id IN (
    SELECT object_id FROM sys.tables WHERE name LIKE 'ITSM_%'
  );
```

#### For PostgreSQL

**Step 1: Connect to database**
```bash
psql -h crm-postgresql -U crm_user -d crm_db
```

**Step 2: Review schema additions**
```sql
-- Check if tables exist (should show 0 results before deployment)
SELECT COUNT(*) FROM information_schema.tables 
  WHERE table_schema = 'public' AND table_name LIKE 'ITSM_%';

-- Check existing indexes
SELECT COUNT(*) FROM information_schema.tables 
  WHERE table_schema = 'public' AND table_name LIKE 'IX_%';
```

**Step 3: Execute schema script**
```bash
# Using psql
psql -h crm-postgresql -U crm_user -d crm_db -f database/schema/itsm_marketing_integration_postgresql.sql

# Or with file input in interactive mode
psql -h crm-postgresql -U crm_user -d crm_db
\i database/schema/itsm_marketing_integration_postgresql.sql
```

**Step 4: Verify tables created**
```sql
-- Should show 13 ITSM tables
SELECT tablename FROM pg_tables 
  WHERE tablename LIKE 'ITSM_%' ORDER BY tablename;

-- Count indexes
SELECT COUNT(*) FROM pg_indexes 
  WHERE tablename LIKE 'ITSM_%';
```

---

## Post-Deployment Validation

After deployment, execute these validation checks:

### 1. Table Creation Verification

```sql
-- MariaDB / SQL Server / PostgreSQL combo (adjust table names as needed)

-- Verify all Problem Management tables exist
SELECT COUNT(*) as Problem_Tables FROM information_schema.TABLES 
  WHERE TABLE_NAME IN ('ITSM_Problems', 'ITSM_ProblemIncidents', 'ITSM_ProblemTasks', 
                       'ITSM_ProblemComments', 'ITSM_ProblemAttachments')
  AND TABLE_SCHEMA = 'crm_db';
-- Expected: 5

-- Verify all Change Management tables exist
SELECT COUNT(*) as Change_Tables FROM information_schema.TABLES 
  WHERE TABLE_NAME IN ('ITSM_Changes', 'ITSM_ChangeApprovals', 'ITSM_ChangeBlackouts',
                       'ITSM_ChangeImpactedCIs', 'ITSM_ChangeTasks', 'ITSM_ChangeComments',
                       'ITSM_ChangeAttachments')
  AND TABLE_SCHEMA = 'crm_db';
-- Expected: 7

-- Verify CMDB table exists
SELECT COUNT(*) as CMDB_Tables FROM information_schema.TABLES 
  WHERE TABLE_NAME = 'ITSM_CIRelationships' AND TABLE_SCHEMA = 'crm_db';
-- Expected: 1
```

### 2. Column Verification

```sql
-- Verify Problem table structure
DESCRIBE ITSM_Problems;  -- MariaDB
-- OR
EXEC sp_columns 'ITSM_Problems';  -- SQL Server
-- OR
\d ITSM_Problems  -- PostgreSQL

-- Expected columns (25 total):
-- ProblemId (PK), Number, ShortDescription, Description, CategoryId, SubcategoryId,
-- ConfigurationItemId, Priority, Symptoms, RootCause, Workaround, KnownError,
-- State, CreatedAt, UpdatedAt, CreatedByUserId, AssignedToUserId, TargetResolutionDate,
-- ResolvedDate, ClosedDate, IsDeleted, RowVersion
```

### 3. Constraint Verification

```sql
-- Verify foreign key constraints exist
SELECT CONSTRAINT_NAME, TABLE_NAME, REFERENCED_TABLE_NAME
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%';
-- Expected: 35+ foreign key constraints

-- Verify check constraints
SELECT CONSTRAINT_NAME, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = 'crm_db' AND CONSTRAINT_TYPE = 'CHECK';
```

### 4. Index Verification

```sql
-- Count indexes per table
SELECT TABLE_NAME, COUNT(*) as Index_Count
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%'
GROUP BY TABLE_NAME
ORDER BY Table_Name;

-- Expected sample:
-- ITSM_Problems: 10 indexes
-- ITSM_Changes: 10 indexes
-- ITSM_ChangeApprovals: 4 indexes
-- ITSM_ChangeBlackouts: 2 indexes
-- ITSM_ChangeImpactedCIs: 3 indexes
-- ITSM_ChangeTasks: 3 indexes
-- ITSM_ChangeComments: 2 indexes
-- ITSM_ChangeAttachments: 2 indexes
-- ITSM_ProblemIncidents: 5 indexes
-- ITSM_ProblemTasks: 4 indexes
-- ITSM_ProblemComments: 3 indexes
-- ITSM_ProblemAttachments: 2 indexes
-- ITSM_CIRelationships: 4 indexes
-- Total expected: 55 indexes (51 new + 4 on webhooks)
```

### 5. Data Integrity Test

```sql
-- Test insert on Problem table (should succeed)
BEGIN TRANSACTION;
INSERT INTO ITSM_Problems (Number, ShortDescription, Priority, State, CreatedAt, CreatedByUserId)
VALUES ('TEST-001', 'Test Problem', 3, 1, NOW(), 1);

-- Verify insert succeeded
SELECT COUNT(*) FROM ITSM_Problems WHERE Number = 'TEST-001';

-- Rollback test data
ROLLBACK;

-- Verify row was removed
SELECT COUNT(*) FROM ITSM_Problems WHERE Number = 'TEST-001';
-- Expected: 0
```

### 6. Application Integration Test

```bash
# Start the CRM API application
cd CRM.Backend/src/CRM.Api && dotnet run

# Test API connectivity
curl -X GET http://localhost:5000/health
# Expected: 200 OK

# Test Problem Management API
curl -X GET http://localhost:5000/api/problems
# Expected: 200 OK (empty array initially)

# Test API endpoint health
curl -X GET http://localhost:5000/api/health/providers
# Expected: All providers showing healthy status
```

---

## Troubleshooting

### Issue: Foreign Key Constraint Fails

**Symptom:** Error like "Cannot add or update a row: a foreign key constraint fails"

**Root Cause:** Referenced table doesn't exist or has incomplete data

**Resolution:**
```sql
-- Check if prerequisite tables exist
SELECT COUNT(*) FROM information_schema.TABLES 
  WHERE TABLE_NAME IN ('Users', 'ConfigurationItems', 'Services', 'UserGroups')
  AND TABLE_SCHEMA = 'crm_db';

-- If missing, apply migrations for those modules first
-- Then re-apply ITSM migrations

-- Verify referenced table has data
SELECT COUNT(*) FROM Users;
```

### Issue: Index Creation Fails

**Symptom:** Error like "Duplicate key name 'IX_Problems_State_CreatedAt'"

**Root Cause:** Index already exists from partial deployment or previous attempt

**Resolution:**
```sql
-- Check existing indexes
SELECT INDEX_NAME, TABLE_NAME FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = 'crm_db' AND INDEX_NAME LIKE 'IX_%';

-- Drop duplicate index if safe to do so
DROP INDEX `IX_Problems_State_CreatedAt` ON ITSM_Problems;

-- Re-run schema script
```

### Issue: Migration Fails with "Metadata token conflict"

**Symptom:** EF Core error about metadata token when applying migration

**Root Cause:** Assembly version mismatch or partial build failure

**Resolution:**
```bash
# Clean build
cd CRM.Backend
dotnet clean
dotnet build

# Try migration again
dotnet ef database update --verbose
```

### Issue: Script Execution Timeout

**Symptom:** Deployment script hangs or times out

**Root Cause:** Large result sets, locked tables, or slow network

**Resolution:**
```bash
# Increase timeout (example for MySQL)
mysql --connect-timeout=300 -h crm-mariadb -u crm_user -p crm_db < schema_script.sql

# Or execute sections individually
# Split schema script and execute in parts

# Check for locked tables
SHOW OPEN TABLES WHERE In_use > 0;  -- MariaDB
SELECT * FROM pg_stat_activity WHERE state != 'idle';  -- PostgreSQL
```

### Issue: "Table already exists" Error

**Symptom:** Error like "Table 'crm_db.ITSM_Problems' already exists"

**Root Cause:** Schema script is not idempotent or table exists from failed previous attempt

**Resolution:**
```sql
-- Option 1: Use provided idempotent DROP statements
-- Schema scripts include "DROP TABLE IF EXISTS" statements
-- Re-run with complete script

-- Option 2: Manual cleanup (if desired)
DROP TABLE IF EXISTS ITSM_ProblemAttachments;
DROP TABLE IF EXISTS ITSM_ProblemComments;
DROP TABLE IF EXISTS ITSM_ProblemTasks;
DROP TABLE IF EXISTS ITSM_ProblemIncidents;
DROP TABLE IF EXISTS ITSM_Problems;

-- Then re-run schema script
```

---

## Rollback Procedures

### Rollback Using EF Core Migration

```bash
# List all applied migrations
cd CRM.Backend && dotnet ef migrations list

# Rollback to previous migration (removes all ITSM tables)
dotnet ef database update <PreviousMigrationName> --verbose

# Example (rollback to before ITSM additions)
dotnet ef database update 20260214T120000_PreviousMigration --verbose

# Verify rollback
SELECT COUNT(*) FROM information_schema.TABLES 
  WHERE TABLE_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%';
-- Expected: 0 (all tables removed)
```

### Rollback Using Direct SQL

```bash
# Create rollback script by extracting DROP statements

# MariaDB / SQL Server
DROP TABLE IF EXISTS ITSM_ChangeAttachments;
DROP TABLE IF EXISTS ITSM_ChangeComments;
DROP TABLE IF EXISTS ITSM_ChangeTasks;
DROP TABLE IF EXISTS ITSM_ChangeImpactedCIs;
DROP TABLE IF EXISTS ITSM_ChangeBlackouts;
DROP TABLE IF EXISTS ITSM_ChangeApprovals;
DROP TABLE IF EXISTS ITSM_Changes;
DROP TABLE IF EXISTS ITSM_ProblemAttachments;
DROP TABLE IF EXISTS ITSM_ProblemComments;
DROP TABLE IF EXISTS ITSM_ProblemTasks;
DROP TABLE IF EXISTS ITSM_ProblemIncidents;
DROP TABLE IF EXISTS ITSM_Problems;
DROP TABLE IF EXISTS ITSM_CIRelationships;

# Execute rollback
mysql -h crm-mariadb -u crm_user -p crm_db < rollback_script.sql
```

### Rollback From Database Backup

```bash
# Use full backup created before deployment
# (see Pre-Deployment Checklist for backup commands)

# MariaDB restore
mysql -h crm-mariadb -u crm_user -p crm_db < crm_db_backup_20260215_100000.sql

# SQL Server restore (PowerShell)
Restore-SqlDatabase -ServerInstance "sql-server" -Database "crm_db" `
  -BackupFile "C:\Backups\crm_db_20260215_100000.bak" -ReplaceDatabase

# PostgreSQL restore
psql -h crm-postgresql -U crm_user -d crm_db < crm_db_backup_20260215_100000.sql
```

**Note:** Backup restore will revert all changes since backup creation. Use only if complete rollback needed.

---

## Performance Optimization

### Post-Deployment Index Maintenance

After deployment, rebuild indexes for optimal performance:

```sql
-- MariaDB
ANALYZE TABLE ITSM_Problems;
ANALYZE TABLE ITSM_Changes;
ANALYZE TABLE ITSM_ChangeApprovals;

-- SQL Server
ALTER INDEX ALL ON ITSM_Problems REBUILD;
ALTER INDEX ALL ON ITSM_Changes REBUILD;
UPDATE STATISTICS ITSM_Problems;
UPDATE STATISTICS ITSM_Changes;

-- PostgreSQL
ANALYZE ITSM_Problems;
ANALYZE ITSM_Changes;
ANALYZE ITSM_ChangeApprovals;
REINDEX TABLE ITSM_Problems;
```

### Query Performance Baseline

Capture baseline performance metrics after deployment:

```sql
-- Test query performance
EXPLAIN ANALYZE
SELECT p.* FROM ITSM_Problems p
WHERE p.State = 1 AND p.Priority = 1
ORDER BY p.CreatedAt DESC
LIMIT 100;

-- Expected index usage: IX_Problems_State_CreatedAt
```

### Monitoring Recommendations

- Monitor table row counts weekly to identify growth patterns
- Track slow query log for index optimization opportunities
- Review execution plans for queries using newly created tables
- Set up alerts for index fragmentation (SQL Server) or bloat (PostgreSQL)

---

## FAQ

### Q: How long does the deployment take?

**A:** EF Core migration typically completes in < 5 minutes for the 13 ITSM tables. Direct SQL execution depends on database size and system load (typically 2-10 minutes).

### Q: Can I deploy without downtime?

**A:** This schema deployment does not support zero-downtime deployment because:
1. New tables must be created before data insertion
2. Foreign key constraints prevent partial deployment
3. Soft-delete column (IsDeleted) affects all records

Recommended approach: Schedule maintenance window (1-2 hours) and notify users in advance.

### Q: What if deployment fails halfway through?

**A:** The migration is transactional:
- **Using EF Core:** Automatically rolls back on failure
- **Using direct SQL:** Transactions must be managed manually or use rollback script
- **Safest approach:** Apply entire migration as one transaction and verify success before commitment

### Q: Can I skip the webhook index additions?

**A:** webhook indexes are optional performance enhancements. You can deploy ITSM tables without webhook index additions:
1. Modify schema script to remove webhook index section
2. Webhook tables will continue to function without new indexes
3. Recommended: Add indexes later during maintenance window for better query performance

### Q: Are there any breaking changes?

**A:** No breaking changes:
- Additions only (new tables and indexes)
- No modifications to existing tables
- No columns added to existing tables
- Foreign keys point to existing tables (no validation issues)
- Applications continue to work unchanged

### Q: Can multiple databases be updated simultaneously?

**A:** EF Core migration is single-database:
1. Update MariaDB using EF migration
2. Update SQL Server using T-SQL script
3. Update PostgreSQL using psql script

Alternatively, use three separate deployment pipelines running in parallel.

### Q: What about seed data?

**A:** Seed data (default SLA policies, problem categories, etc.) is optional:
1. Covered in separate `*_seed_data.sql` files (not yet generated)
2. Can be deployed immediately after schema creation
3. Can also be loaded through application UI during setup

---

## Success Criteria

Deployment is successful when:

- [ ] **All 13 ITSM tables created** ✅
- [ ] **51 indexes created** ✅
- [ ] **35 constraints enforced** ✅
- [ ] **Foreign key relationships valid** ✅
- [ ] **Sample insert/query operations succeed** ✅
- [ ] **Application starts without errors** ✅
- [ ] **API endpoints responding** ✅
- [ ] **No warnings in application logs** ✅
- [ ] **Database backups verified restorable** ✅

---

## Rollback Decision Tree

**Deployment success?**
- ✅ YES → Proceed with application testing and user acceptance testing
- ❌ NO → See [Rollback Procedures](#rollback-procedures)

**Query performance acceptable?**
- ✅ YES → Deployment complete, promote to next environment
- ❌ NO → Check [Performance Optimization](#performance-optimization) and [Troubleshooting](#troubleshooting)

**All tests passing?**
- ✅ YES → Schedule go-live and communicate to stakeholders
- ❌ NO → Review failures, apply fixes, re-run testing cycle

---

## Contact & Support

- **Technical Questions:** Reference [SOLUTION_CONTEXT.md](docs/development/SOLUTION_CONTEXT.md)
- **Schema Details:** See [DATABASE_SCHEMA_ADDITIONS.md](./DATABASE_SCHEMA_ADDITIONS.md)
- **Migration Code:** See [20260215T180000_AddITSMMarketingIntegrationTables.cs](../../CRM.Backend/src/CRM.Infrastructure/Migrations/)
- **Entity Definitions:** Check [Problem.cs](../../CRM.Backend/src/CRM.Core/Entities/ITSM/), [Change.cs](../../CRM.Backend/src/CRM.Core/Entities/ITSM/)

---

**END OF IMPLEMENTATION GUIDE**

# Configurable Enums Database Migration

**Date:** February 27, 2026  
**Status:** ✅ **COMPLETED** on dev server (192.168.0.9:3306/crm_db)  
**Impact:** Database-driven enum management with backward compatibility

## Summary

Migrated hard-coded C# enums (LeadStatus, OpportunityStage, ServiceRequestStatus/Priority) to database-driven configurable enums stored in `LookupCategories` and `LookupItems` tables. This enables runtime customization without code changes.

## What Was Changed

### Database Schema
1. **Enhanced LookupCategories table** - Added `EntityType`, `PropertyName`, `IsSystemManaged`, `AllowCustomValues`, `ValidationSchema`
2. **Enhanced LookupItems table** - Added `IsDefault`, `IsSystemValue`, `Color`, `Icon`, `ValidationRules`
3. **Created EnumTransitions table** - State machine rules for allowed transitions, role-based restrictions, approval workflows
4. **Added FK columns to entities**:
   - `Leads.StatusId` → FK to LookupItems
   - `Opportunities.StageId` → FK to LookupItems
   - `ServiceRequests.StatusId` → FK to LookupItems
   - `ServiceRequests.PriorityId` → FK to LookupItems

### Data Migration
- ✅ **231 Leads** migrated from `Status` (enum int) to `StatusId` (FK)
- ✅ **230 Opportunities** migrated from `Stage` (enum int) to `StageId` (FK)
- ✅ **187 ServiceRequests** migrated from `Status`/`Priority` (enum ints) to `StatusId`/`PriorityId` (FKs)

### Backward Compatibility
- Old enum columns (`Status`, `Stage`, `Priority`) remain unchanged
- Existing code continues to work without modification
- Future enhancement: Mark old columns as `[Obsolete]` in C# entities

## Migration Files

| File | Purpose | Status |
|------|---------|--------|
| `20260227_enum_schema_enhancements.sql` | Step 1: Enhance schema, create EnumTransitions | ✅ Applied |
| `20260227_servicerequest_categories.sql` | Step 2: Create ServiceRequest enum categories | ✅ Applied |
| `20260227_entity_fk_migration.sql` | Step 3: Add FK columns, migrate data | ✅ Applied |

## Applying to Other Environments

### Prerequisites
1. Backup database before applying migrations
2. Ensure `006_lookup_data.sql` seed file has been run (creates base enum categories)
3. Database user needs ALTER TABLE, CREATE TABLE, CREATE INDEX permissions

### Execution Order
```bash
# Connect to target database
mysql -u <user> -p<password> -h <host> <database>

# Step 1: Enhance schema (if not already applied)
source /path/to/migrations/20260227_enum_schema_enhancements.sql

# Step 2: Create ServiceRequest categories (if not already exist)
source /path/to/migrations/20260227_servicerequest_categories.sql

# Step 3: Migrate entity FKs
source /path/to/migrations/20260227_entity_fk_migration.sql

# Verify data migration
SELECT 'Leads' AS Entity, COUNT(*) AS NullCount FROM Leads WHERE StatusId IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) FROM Opportunities WHERE StageId IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) FROM ServiceRequests WHERE StatusId IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) FROM ServiceRequests WHERE PriorityId IS NULL;

-- Expected: All counts should be 0
```

### Docker Deployment
```bash
# Copy migration files to docker volume
docker cp 20260227_*.sql crm-mariadb:/tmp/

# Execute migrations
docker exec -i crm-mariadb mariadb -u crm_user -p<password> crm_db < /tmp/20260227_enum_schema_enhancements.sql
docker exec -i crm-mariadb mariadb -u crm_user -p<password> crm_db < /tmp/20260227_servicerequest_categories.sql
docker exec -i crm-mariadb mariadb -u crm_user -p<password> crm_db < /tmp/20260227_entity_fk_migration.sql
```

## Verification Queries

### Check schema changes
```sql
-- Verify new columns exist
SHOW COLUMNS FROM LookupCategories;
SHOW COLUMNS FROM LookupItems;
SHOW COLUMNS FROM Leads WHERE Field LIKE '%StatusId%';
SHOW COLUMNS FROM Opportunities WHERE Field LIKE '%StageId%';
SHOW COLUMNS FROM ServiceRequests WHERE Field LIKE '%Id%';

-- Verify EnumTransitions table exists
SHOW TABLES LIKE 'EnumTransitions';
```

### Check data migration
```sql
-- Verify no NULL FK values
SELECT 'Leads' AS Entity, COUNT(*) AS NullCount FROM Leads WHERE StatusId IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) FROM Opportunities WHERE StageId IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) FROM ServiceRequests WHERE StatusId IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) FROM ServiceRequests WHERE PriorityId IS NULL;
-- Expected: 0, 0, 0, 0

-- Check data distribution
SELECT li.Key, li.Value, COUNT(*) as Count
FROM Leads l
INNER JOIN LookupItems li ON l.StatusId = li.Id
GROUP BY li.Key, li.Value
ORDER BY COUNT(*) DESC;
```

### Check constraints
```sql
-- Verify FK constraints exist
SELECT CONSTRAINT_NAME, TABLE_NAME, REFERENCED_TABLE_NAME
FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'crm_db'
  AND CONSTRAINT_NAME LIKE 'FK_%StatusValue%'
   OR CONSTRAINT_NAME LIKE 'FK_%StageValue%'
   OR CONSTRAINT_NAME LIKE 'FK_%PriorityValue%';
-- Expected: 4 constraints
```

## Rollback Procedure (NOT RECOMMENDED)

⚠️ **WARNING:** Rolling back will cause data loss for any customizations made to enum values after migration.

```sql
-- Remove FK constraints
ALTER TABLE Leads DROP FOREIGN KEY FK_Leads_StatusValue;
ALTER TABLE Opportunities DROP FOREIGN KEY FK_Opportunities_StageValue;
ALTER TABLE ServiceRequests DROP FOREIGN KEY FK_ServiceRequests_StatusValue;
ALTER TABLE ServiceRequests DROP FOREIGN KEY FK_ServiceRequests_PriorityValue;

-- Drop FK columns
ALTER TABLE Leads DROP COLUMN StatusId;
ALTER TABLE Opportunities DROP COLUMN StageId;
ALTER TABLE ServiceRequests DROP COLUMN StatusId, DROP COLUMN PriorityId;

-- Remove new columns from LookupCategories/LookupItems
ALTER TABLE LookupCategories 
    DROP COLUMN EntityType,
    DROP COLUMN PropertyName,
    DROP COLUMN IsSystemManaged,
    DROP COLUMN AllowCustomValues,
    DROP COLUMN ValidationSchema;

ALTER TABLE LookupItems
    DROP COLUMN IsDefault,
    DROP COLUMN IsSystemValue,
    DROP COLUMN Color,
    DROP COLUMN Icon,
    DROP COLUMN ValidationRules;

-- Drop EnumTransitions table
DROP TABLE IF EXISTS EnumTransitions;
```

## Future Enhancements

These migrations are **Phase 1** of the configurable enums feature. Future phases will include:

| Phase | Description | Status |
|-------|-------------|--------|
| **Phase 1** | Database schema + data migration | ✅ Complete |
| **Phase 2** | Backend API (EnumManagement service/controller) | ⏳ Planned |
| **Phase 3** | Frontend Admin UI (Enum Management page) | ⏳ Planned |
| **Phase 4** | State machine transitions enforcement | ⏳ Planned |
| **Phase 5** | Role-based transition rules | ⏳ Planned |

## Troubleshooting

**Issue:** Migration fails with "Unknown column 'EntityType'"  
**Solution:** Run `20260227_enum_schema_enhancements.sql` first

**Issue:** Migration fails with "Cannot add foreign key constraint"  
**Solution:** Ensure `LookupItems` table has the referenced Id values (run seed data 006_lookup_data.sql)

**Issue:** NULL FK values after migration  
**Solution:** Check if `ServiceRequestStatus` and `ServiceRequestPriority` categories exist (run `20260227_servicerequest_categories.sql`)

**Issue:** "Duplicate entry" error when inserting enum categories  
**Solution:** Categories already exist - use `ON DUPLICATE KEY UPDATE` or check existing data first

## Support

- **Documentation:** `docs/11-specifications/SPEC-GEN-002-ConfigurableEnums.md`
- **Contact:** Development Team
- **Slack:** #crm-development

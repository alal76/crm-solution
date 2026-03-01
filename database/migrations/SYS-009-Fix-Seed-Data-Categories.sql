-- =============================================================================
-- SYS-009: Fix Seed Data Category Mismatch
-- =============================================================================
-- Purpose: Fix 69 timezone entries that were wrongly assigned to LeadStatus (ID=2)
--          instead of the Timezones category. Then re-migrate Lead StatusId.
-- Root Cause: Multiple seed files with conflicting hardcoded category IDs caused
--             timezone items to land in the wrong category bucket.
-- Applied to: crm_db (192.168.0.9) - 2026-02-27
-- =============================================================================

-- DIAGNOSTIC: Show current state of LeadStatus category items

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

SELECT 'DIAGNOSTIC: Items in LeadStatus (ID=2) before fix' AS Step;
SELECT Id, `Key`, Value, LookupCategoryId FROM LookupItems
WHERE LookupCategoryId = 2
ORDER BY Id
LIMIT 30;

-- Count timezone items in LeadStatus
SELECT 'DIAGNOSTIC: Timezone items wrongly in LeadStatus' AS Step;
SELECT COUNT(*) AS TimezoneItemsInLeadStatus
FROM LookupItems
WHERE LookupCategoryId = 2
AND (`Key` LIKE 'America/%' OR `Key` LIKE 'Europe/%' OR `Key` LIKE 'Asia/%'
     OR `Key` LIKE 'Pacific/%' OR `Key` LIKE 'Africa/%' OR `Key` LIKE 'Atlantic/%'
     OR `Key` LIKE 'Australia/%' OR `Key` = 'UTC');

-- =============================================================================
-- FIX: Move timezone items from LeadStatus (2) to correct Timezones category
-- =============================================================================
SELECT 'FIX: Moving timezone items to correct category' AS Step;

SET @timezone_cat_id = (SELECT Id FROM LookupCategories WHERE Name = 'Timezones' LIMIT 1);

SELECT @timezone_cat_id AS TimezonesCategoryId;

UPDATE LookupItems
SET LookupCategoryId = @timezone_cat_id
WHERE LookupCategoryId = 2
AND (`Key` LIKE 'America/%' OR `Key` LIKE 'Europe/%' OR `Key` LIKE 'Asia/%'
     OR `Key` LIKE 'Pacific/%' OR `Key` LIKE 'Africa/%' OR `Key` LIKE 'Atlantic/%'
     OR `Key` LIKE 'Australia/%' OR `Key` = 'UTC');

SELECT ROW_COUNT() AS TimezonesItemsMoved;

-- =============================================================================
-- VERIFY: Show remaining items in LeadStatus (should only be status values)
-- =============================================================================
SELECT 'VERIFY: Items remaining in LeadStatus after fix' AS Step;
SELECT Id, `Key`, Value, SortOrder FROM LookupItems
WHERE LookupCategoryId = 2
ORDER BY SortOrder;

-- =============================================================================
-- RE-MIGRATE: Lead StatusId using Key-based mapping (after timezone fix)
-- =============================================================================
SELECT 'RE-MIGRATE: Leads.StatusId with Key-based mapping' AS Step;

UPDATE Leads l
INNER JOIN LookupCategories lc ON lc.Name = 'LeadStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET l.StatusId = li.Id
WHERE li.Key = CASE l.Status
    WHEN 0 THEN 'NEW'
    WHEN 1 THEN 'CONTACT'
    WHEN 2 THEN 'WORK'
    WHEN 3 THEN 'NURTURE'
    WHEN 4 THEN 'QUAL'
    WHEN 5 THEN 'UNQUAL'
    WHEN 6 THEN 'CONV'
    WHEN 7 THEN 'LOST'
    ELSE NULL
END;

SELECT ROW_COUNT() AS LeadsMigrated;

-- =============================================================================
-- FINAL VERIFICATION
-- =============================================================================
SELECT 'FINAL: Lead Status Distribution (should show actual status names)' AS Step;
SELECT li.`Key` AS StatusKey, li.Value AS StatusValue, lc.Name AS CategoryName, COUNT(*) AS Count
FROM Leads l
INNER JOIN LookupItems li ON l.StatusId = li.Id
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
GROUP BY li.`Key`, li.Value, lc.Name
ORDER BY Count DESC;

SELECT 'FINAL: NULL FK Check (all should be 0)' AS Step;
SELECT 'Leads' AS Entity, COUNT(*) AS NullCount FROM Leads WHERE StatusId IS NULL
UNION ALL SELECT 'Opportunities', COUNT(*) FROM Opportunities WHERE StageId IS NULL
UNION ALL SELECT 'ServiceRequests(Status)', COUNT(*) FROM ServiceRequests WHERE StatusId IS NULL
UNION ALL SELECT 'ServiceRequests(Priority)', COUNT(*) FROM ServiceRequests WHERE PriorityId IS NULL;
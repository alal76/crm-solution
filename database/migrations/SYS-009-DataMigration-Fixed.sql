-- =============================================================================
-- SYS-009: Entity FK Data Migration (Fixed Version)
-- =============================================================================
-- Purpose: Migrate existing enum integer values to FK references in LookupItems
-- Applies to: Leads (StatusId), Opportunities (StageId)
-- Note: ServiceRequests handled in SYS-009-ServiceRequest-Fix.sql
-- Applied to: crm_db (192.168.0.9) - 2026-02-27
-- =============================================================================

-- PRE-CHECK: Verify categories exist

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

SELECT 'PRE-CHECK: LookupCategories' AS Step;
SELECT Id, Name, EntityType, PropertyName FROM LookupCategories ORDER BY Id;

-- PRE-CHECK: Distribution before migration
SELECT 'PRE-CHECK: Lead Status distribution (enum values)' AS Step;
SELECT Status, COUNT(*) AS Count FROM Leads GROUP BY Status ORDER BY Status;

SELECT 'PRE-CHECK: Opportunity Stage distribution (enum values)' AS Step;
SELECT Stage, COUNT(*) AS Count FROM Opportunities GROUP BY Stage ORDER BY Stage;

-- =============================================================================
-- MIGRATE LEADS.StatusId
-- Maps Lead.Status (0-7) to LeadStatus LookupItem using SortOrder-1 offset
-- =============================================================================
SELECT 'MIGRATING: Leads.StatusId' AS Step;

UPDATE Leads l
INNER JOIN LookupCategories lc ON lc.Name = 'LeadStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET l.StatusId = li.Id
WHERE (li.SortOrder - 1) = l.Status;

SELECT 'RESULT: Leads migration' AS Step;
SELECT
    COUNT(CASE WHEN StatusId IS NULL THEN 1 END) AS NullCount,
    COUNT(CASE WHEN StatusId IS NOT NULL THEN 1 END) AS MigratedCount,
    COUNT(*) AS TotalCount
FROM Leads;

-- =============================================================================
-- MIGRATE OPPORTUNITIES.StageId
-- Maps Opportunity.Stage (0-7) to OpportunityStage LookupItem using Key matching
-- =============================================================================
SELECT 'MIGRATING: Opportunities.StageId' AS Step;

UPDATE Opportunities o
INNER JOIN LookupCategories lc ON lc.Name = 'OpportunityStage'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET o.StageId = li.Id
WHERE li.Key = CASE o.Stage
    WHEN 0 THEN 'PROSP'
    WHEN 1 THEN 'QUAL'
    WHEN 2 THEN 'NEEDS'
    WHEN 3 THEN 'VALUE'
    WHEN 4 THEN 'PERC'
    WHEN 5 THEN 'PROP'
    WHEN 6 THEN 'NEG'
    WHEN 7 THEN 'CLOSED'
    ELSE NULL
END;

SELECT 'RESULT: Opportunities migration' AS Step;
SELECT
    COUNT(CASE WHEN StageId IS NULL THEN 1 END) AS NullCount,
    COUNT(CASE WHEN StageId IS NOT NULL THEN 1 END) AS MigratedCount,
    COUNT(*) AS TotalCount
FROM Opportunities;

-- =============================================================================
-- POST-CHECK: Distribution after migration
-- =============================================================================
SELECT 'POST-CHECK: Lead Status Distribution' AS Step;
SELECT li.Value AS Status, lc.Name AS Category, COUNT(*) AS Count
FROM Leads l
INNER JOIN LookupItems li ON l.StatusId = li.Id
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
GROUP BY li.Value, lc.Name
ORDER BY Count DESC;

SELECT 'POST-CHECK: Opportunity Stage Distribution' AS Step;
SELECT li.Value AS Stage, lc.Name AS Category, COUNT(*) AS Count
FROM Opportunities o
INNER JOIN LookupItems li ON o.StageId = li.Id
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
GROUP BY li.Value, lc.Name
ORDER BY Count DESC;
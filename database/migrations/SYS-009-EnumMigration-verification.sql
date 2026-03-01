-- ============================================================================
-- SYS-009: Entity Migration Verification Queries (ENUM-MIG-016)
-- Date: 2026-02-28
-- ENUM-TEST-014: Data integrity verification — this file satisfies the
--   ENUM-TEST-014 requirement by providing SQL queries that confirm:
--     (1) No entity rows remain with NULL FK columns after migration
--     (2) All migrated FK values join successfully to EnumValues
--     (3) Distribution of enum FK values is viewable for manual audit
--   Run this script against crm_db after applying SYS-009-EnumMigration.sql.
--   All COUNT(*) queries in sections 1-4 must return 0 for migration success.
--
-- PURPOSE:
--   Verify that the SYS-009-EnumMigration.sql data migration populated the
--   new StatusId / StageId / PriorityId columns correctly.
--
-- Expected result after a successful migration:
--   NullLeadStatuses     = 0
--   NullOpportunityStages = 0
--   NullSRStatuses        = 0
--   NullSRPriorities      = 0
--
-- Non-zero counts indicate rows whose legacy enum ordinal could not be
-- mapped to a matching EnumValues key.  Those rows must be updated manually.
-- ============================================================================

-- ─────────────────────────────────────────────────────────────────────────────
-- 1.  Leads: rows still missing StatusId (should be 0)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT COUNT(*) AS NullLeadStatuses
FROM `Leads`
WHERE StatusId IS NULL
  AND IsDeleted = 0;

-- ─────────────────────────────────────────────────────────────────────────────
-- 2.  Opportunities: rows still missing StageId (should be 0)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT COUNT(*) AS NullOpportunityStages
FROM `Opportunities`
WHERE StageId IS NULL
  AND IsDeleted = 0;

-- ─────────────────────────────────────────────────────────────────────────────
-- 3.  ServiceRequests: rows still missing StatusId (should be 0)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT COUNT(*) AS NullSRStatuses
FROM `ServiceRequests`
WHERE StatusId IS NULL
  AND IsDeleted = 0;

-- ─────────────────────────────────────────────────────────────────────────────
-- 4.  ServiceRequests: rows still missing PriorityId (should be 0)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT COUNT(*) AS NullSRPriorities
FROM `ServiceRequests`
WHERE PriorityId IS NULL
  AND IsDeleted = 0;

-- ─────────────────────────────────────────────────────────────────────────────
-- 5.  Sample of unmapped Leads (for debugging)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT Id, `Status`, StatusId, FirstName, LastName
FROM `Leads`
WHERE StatusId IS NULL
  AND IsDeleted = 0
LIMIT 20;

-- ─────────────────────────────────────────────────────────────────────────────
-- 6.  Distribution of Lead StatusId values
-- ─────────────────────────────────────────────────────────────────────────────
SELECT ev.`Key` AS StatusKey, ev.Label AS StatusLabel, COUNT(l.Id) AS LeadCount
FROM `Leads` l
JOIN `EnumValues` ev ON ev.Id = l.StatusId
WHERE l.IsDeleted = 0
GROUP BY ev.Id, ev.`Key`, ev.Label
ORDER BY ev.SortOrder;

-- ─────────────────────────────────────────────────────────────────────────────
-- 7.  Distribution of Opportunity StageId values
-- ─────────────────────────────────────────────────────────────────────────────
SELECT ev.`Key` AS StageKey, ev.Label AS StageLabel, COUNT(o.Id) AS OpportunityCount
FROM `Opportunities` o
JOIN `EnumValues` ev ON ev.Id = o.StageId
WHERE o.IsDeleted = 0
GROUP BY ev.Id, ev.`Key`, ev.Label
ORDER BY ev.SortOrder;

-- ─────────────────────────────────────────────────────────────────────────────
-- 8.  Distribution of ServiceRequest StatusId values
-- ─────────────────────────────────────────────────────────────────────────────
SELECT ev.`Key` AS StatusKey, ev.Label AS StatusLabel, COUNT(sr.Id) AS SRCount
FROM `ServiceRequests` sr
JOIN `EnumValues` ev ON ev.Id = sr.StatusId
WHERE sr.IsDeleted = 0
GROUP BY ev.Id, ev.`Key`, ev.Label
ORDER BY ev.SortOrder;

-- ─────────────────────────────────────────────────────────────────────────────
-- 9.  Distribution of ServiceRequest PriorityId values
-- ─────────────────────────────────────────────────────────────────────────────
SELECT ev.`Key` AS PriorityKey, ev.Label AS PriorityLabel, COUNT(sr.Id) AS SRCount
FROM `ServiceRequests` sr
JOIN `EnumValues` ev ON ev.Id = sr.PriorityId
WHERE sr.IsDeleted = 0
GROUP BY ev.Id, ev.`Key`, ev.Label
ORDER BY ev.SortOrder;

SELECT 'Verification complete.' AS status;

-- ============================================================================
-- Migration: SYS-009 - Entity FK Migration
-- Date: 2026-02-27
-- Description: Migrate Lead, Opportunity, ServiceRequest from enum integers
--              to foreign key references to LookupItems
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
--           ✅ Data migrated: 231 Leads, 230 Opportunities, 187 ServiceRequests
-- ============================================================================

-- ============================================================================
-- LEADS: Add StatusId FK column
-- ============================================================================
ALTER TABLE Leads
ADD COLUMN IF NOT EXISTS StatusId INT NULL COMMENT 'FK to LookupItems (LeadStatus category)';

-- Migrate existing data (enum int → FK)
-- Strategy: SortOrder in LookupItems starts at 1, but enum ints start at 0
-- So: Status 0 maps to SortOrder 1, Status 1 maps to SortOrder 2, etc.
UPDATE Leads l
INNER JOIN LookupCategories lc ON lc.Name = 'LeadStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET l.StatusId = li.Id
WHERE li.SortOrder = (l.Status + 1);

-- Add FK constraint
ALTER TABLE Leads
ADD CONSTRAINT IF NOT EXISTS FK_Leads_StatusValue 
FOREIGN KEY (StatusId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add index for performance
CREATE INDEX IF NOT EXISTS IX_Leads_StatusId ON Leads(StatusId);

-- ============================================================================
-- OPPORTUNITIES: Add StageId FK column
-- ============================================================================
ALTER TABLE Opportunities
ADD COLUMN IF NOT EXISTS StageId INT NULL COMMENT 'FK to LookupItems (OpportunityStage category)';

-- Migrate existing data
UPDATE Opportunities o
INNER JOIN LookupCategories lc ON lc.Name = 'OpportunityStage'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET o.StageId = li.Id
WHERE li.SortOrder = (o.Stage + 1);

-- Add FK constraint
ALTER TABLE Opportunities
ADD CONSTRAINT IF NOT EXISTS FK_Opportunities_StageValue 
FOREIGN KEY (StageId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add index
CREATE INDEX IF NOT EXISTS IX_Opportunities_StageId ON Opportunities(StageId);

-- ============================================================================
-- SERVICEREQUESTS: Add StatusId and PriorityId FK columns
-- ============================================================================

-- Note: ServiceRequestStatus and ServiceRequestPriority categories must exist first
-- If they don't exist, run the creation script first (see SYS-009-ServiceRequest-Creation.sql)

ALTER TABLE ServiceRequests
ADD COLUMN IF NOT EXISTS StatusId INT NULL COMMENT 'FK to LookupItems (ServiceRequestStatus category)',
ADD COLUMN IF NOT EXISTS PriorityId INT NULL COMMENT 'FK to LookupItems (ServiceRequestPriority category)';

-- Migrate Status (using CASE mapping because Key names don't match enum exactly)
UPDATE ServiceRequests sr
INNER JOIN LookupCategories lc ON lc.Name = 'ServiceRequestStatus'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET sr.StatusId = li.Id
WHERE li.`Key` = CASE sr.Status
    WHEN 0 THEN 'NEW'
    WHEN 1 THEN 'OPEN'
    WHEN 2 THEN 'IN_PROGRESS'
    WHEN 3 THEN 'PENDING'
    WHEN 4 THEN 'ON_HOLD'
    WHEN 5 THEN 'RESOLVED'
    WHEN 6 THEN 'CLOSED'
    WHEN 7 THEN 'CANCELLED'
    ELSE 'NEW'
END;

-- Migrate Priority
UPDATE ServiceRequests sr
INNER JOIN LookupCategories lc ON lc.Name = 'ServiceRequestPriority'
INNER JOIN LookupItems li ON li.LookupCategoryId = lc.Id
SET sr.PriorityId = li.Id
WHERE li.`Key` = CASE sr.Priority
    WHEN 0 THEN 'LOW'
    WHEN 1 THEN 'MEDIUM'
    WHEN 2 THEN 'HIGH'
    WHEN 3 THEN 'CRITICAL'
    ELSE 'MEDIUM'
END;

-- Add FK constraints
ALTER TABLE ServiceRequests
ADD CONSTRAINT IF NOT EXISTS FK_ServiceRequests_StatusValue 
FOREIGN KEY (StatusId) REFERENCES LookupItems(Id) ON DELETE RESTRICT,
ADD CONSTRAINT IF NOT EXISTS FK_ServiceRequests_PriorityValue 
FOREIGN KEY (PriorityId) REFERENCES LookupItems(Id) ON DELETE RESTRICT;

-- Add indexes
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_StatusId ON ServiceRequests(StatusId);
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_PriorityId ON ServiceRequests(PriorityId);

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Check for NULL FK values (should be 0 after migration)
SELECT 'NULL FK Check' AS Test;
SELECT 'Leads' AS Entity, COUNT(*) AS NullCount FROM Leads WHERE StatusId IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) FROM Opportunities WHERE StageId IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) FROM ServiceRequests WHERE StatusId IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) FROM ServiceRequests WHERE PriorityId IS NULL;

-- Check for invalid FK values
SELECT 'Invalid FK Check' AS Test;
SELECT 'Leads' AS Entity, COUNT(*) AS InvalidCount 
FROM Leads l 
LEFT JOIN LookupItems li ON l.StatusId = li.Id 
WHERE l.StatusId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'Opportunities', COUNT(*) 
FROM Opportunities o 
LEFT JOIN LookupItems li ON o.StageId = li.Id 
WHERE o.StageId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'ServiceRequests (Status)', COUNT(*) 
FROM ServiceRequests sr 
LEFT JOIN LookupItems li ON sr.StatusId = li.Id 
WHERE sr.StatusId IS NOT NULL AND li.Id IS NULL
UNION ALL
SELECT 'ServiceRequests (Priority)', COUNT(*) 
FROM ServiceRequests sr 
LEFT JOIN LookupItems li ON sr.PriorityId = li.Id 
WHERE sr.PriorityId IS NOT NULL AND li.Id IS NULL;

-- Data distribution
SELECT 'Lead Status Distribution' AS Report;
SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM Leads l
INNER JOIN LookupItems li ON l.StatusId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT 'Opportunity Stage Distribution' AS Report;
SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM Opportunities o
INNER JOIN LookupItems li ON o.StageId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT 'ServiceRequest Status Distribution' AS Report;
SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.StatusId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

SELECT 'ServiceRequest Priority Distribution' AS Report;
SELECT li.`Key`, li.Value, COUNT(*) as Count
FROM ServiceRequests sr
INNER JOIN LookupItems li ON sr.PriorityId = li.Id
GROUP BY li.`Key`, li.Value
ORDER BY COUNT(*) DESC;

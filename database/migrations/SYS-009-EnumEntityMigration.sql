-- =============================================================================
-- SYS-009: Enum Entity Migration
-- Batch 3 Phase 2: ENUM-MIG-001 to ENUM-MIG-016
-- Purpose: Add nullable StatusId/StageId/PriorityId FK columns alongside
--          existing enum int columns for Lead, Opportunity, and ServiceRequest.
--          Enables gradual migration to database-driven (configurable) enum values.
-- Prerequisite: SYS-008-ConfigurableEnums.sql must have been applied first.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- LEADS: Add StatusId (ENUM-MIG-001 to ENUM-MIG-004)
-- -----------------------------------------------------------------------------

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

ALTER TABLE Leads
    ADD COLUMN IF NOT EXISTS StatusId INT NULL AFTER `Status`;

-- Add FK constraint (safe to run even if column was already added)
SET @constraint_exists = (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Leads'
      AND CONSTRAINT_NAME = 'FK_Leads_StatusId'
);

SET @sql = IF(@constraint_exists = 0,
    'ALTER TABLE Leads ADD CONSTRAINT FK_Leads_StatusId FOREIGN KEY (StatusId) REFERENCES EnumValues(Id) ON DELETE SET NULL',
    'SELECT ''FK_Leads_StatusId already exists, skipping'' AS msg'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE INDEX IF NOT EXISTS IX_Leads_StatusId ON Leads (StatusId);

-- -----------------------------------------------------------------------------
-- OPPORTUNITIES: Add StageId (ENUM-MIG-005 to ENUM-MIG-008)
-- -----------------------------------------------------------------------------
ALTER TABLE Opportunities
    ADD COLUMN IF NOT EXISTS StageId INT NULL AFTER `Stage`;

SET @constraint_exists = (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Opportunities'
      AND CONSTRAINT_NAME = 'FK_Opportunities_StageId'
);

SET @sql = IF(@constraint_exists = 0,
    'ALTER TABLE Opportunities ADD CONSTRAINT FK_Opportunities_StageId FOREIGN KEY (StageId) REFERENCES EnumValues(Id) ON DELETE SET NULL',
    'SELECT ''FK_Opportunities_StageId already exists, skipping'' AS msg'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE INDEX IF NOT EXISTS IX_Opportunities_StageId ON Opportunities (StageId);

-- -----------------------------------------------------------------------------
-- SERVICE REQUESTS: Add StatusId and PriorityId (ENUM-MIG-009 to ENUM-MIG-016)
-- -----------------------------------------------------------------------------
ALTER TABLE ServiceRequests
    ADD COLUMN IF NOT EXISTS StatusId INT NULL,
    ADD COLUMN IF NOT EXISTS PriorityId INT NULL;

SET @constraint_exists = (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'ServiceRequests'
      AND CONSTRAINT_NAME = 'FK_ServiceRequests_StatusId'
);

SET @sql = IF(@constraint_exists = 0,
    'ALTER TABLE ServiceRequests ADD CONSTRAINT FK_ServiceRequests_StatusId FOREIGN KEY (StatusId) REFERENCES EnumValues(Id) ON DELETE SET NULL',
    'SELECT ''FK_ServiceRequests_StatusId already exists, skipping'' AS msg'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @constraint_exists = (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'ServiceRequests'
      AND CONSTRAINT_NAME = 'FK_ServiceRequests_PriorityId'
);

SET @sql = IF(@constraint_exists = 0,
    'ALTER TABLE ServiceRequests ADD CONSTRAINT FK_ServiceRequests_PriorityId FOREIGN KEY (PriorityId) REFERENCES EnumValues(Id) ON DELETE SET NULL',
    'SELECT ''FK_ServiceRequests_PriorityId already exists, skipping'' AS msg'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE INDEX IF NOT EXISTS IX_ServiceRequests_StatusId   ON ServiceRequests (StatusId);
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_PriorityId ON ServiceRequests (PriorityId);

-- =============================================================================
-- DATA MIGRATION (OPTIONAL - run after EnumValues seeding is confirmed)
-- Uncomment the UPDATE blocks below once LookupItems / EnumValues rows exist.
-- =============================================================================

-- Leads: populate StatusId from Status integer + EnumCategory 'LeadStatus'
-- UPDATE Leads l
-- INNER JOIN EnumValues ev ON ev.Key = CASE l.Status
--     WHEN 0 THEN 'new'
--     WHEN 1 THEN 'working'
--     WHEN 2 THEN 'nurturing'
--     WHEN 3 THEN 'qualified'
--     WHEN 4 THEN 'disqualified'
--     WHEN 5 THEN 'converted'
-- END
-- INNER JOIN EnumCategories ec ON ev.CategoryId = ec.Id AND ec.EntityType = 'Lead' AND ec.FieldName = 'Status'
-- SET l.StatusId = ev.Id
-- WHERE l.IsDeleted = 0 AND l.StatusId IS NULL;

-- Opportunities: populate StageId from Stage integer + EnumCategory 'OpportunityStage'
-- UPDATE Opportunities o
-- INNER JOIN EnumValues ev ON ev.Key = CASE o.Stage
--     WHEN 0 THEN 'discovery'
--     WHEN 1 THEN 'qualification'
--     WHEN 2 THEN 'proposal'
--     WHEN 3 THEN 'negotiation'
--     WHEN 4 THEN 'closed_won'
--     WHEN 5 THEN 'closed_lost'
-- END
-- INNER JOIN EnumCategories ec ON ev.CategoryId = ec.Id AND ec.EntityType = 'Opportunity' AND ec.FieldName = 'Stage'
-- SET o.StageId = ev.Id
-- WHERE o.IsDeleted = 0 AND o.StageId IS NULL;

-- ServiceRequests: populate StatusId
-- UPDATE ServiceRequests sr
-- INNER JOIN EnumValues ev ON ev.Key = CASE sr.Status
--     WHEN 0 THEN 'new'
--     WHEN 1 THEN 'open'
--     WHEN 2 THEN 'in_progress'
--     WHEN 3 THEN 'pending_customer'
--     WHEN 4 THEN 'pending_internal'
--     WHEN 5 THEN 'escalated'
--     WHEN 6 THEN 'resolved'
--     WHEN 7 THEN 'closed'
--     WHEN 8 THEN 'cancelled'
--     WHEN 9 THEN 'on_hold'
--     WHEN 10 THEN 'reopened'
-- END
-- INNER JOIN EnumCategories ec ON ev.CategoryId = ec.Id AND ec.EntityType = 'ServiceRequest' AND ec.FieldName = 'Status'
-- SET sr.StatusId = ev.Id
-- WHERE sr.IsDeleted = 0 AND sr.StatusId IS NULL;

-- ServiceRequests: populate PriorityId
-- UPDATE ServiceRequests sr
-- INNER JOIN EnumValues ev ON ev.Key = CASE sr.Priority
--     WHEN 0 THEN 'low'
--     WHEN 1 THEN 'medium'
--     WHEN 2 THEN 'high'
--     WHEN 3 THEN 'critical'
--     WHEN 4 THEN 'urgent'
-- END
-- INNER JOIN EnumCategories ec ON ev.CategoryId = ec.Id AND ec.EntityType = 'ServiceRequest' AND ec.FieldName = 'Priority'
-- SET sr.PriorityId = ev.Id
-- WHERE sr.IsDeleted = 0 AND sr.PriorityId IS NULL;

-- =============================================================================
-- VERIFICATION
-- =============================================================================
SELECT 'Leads missing StatusId'          AS check_name, COUNT(*) AS affected_count
FROM Leads
WHERE StatusId IS NULL AND IsDeleted = 0
UNION ALL
SELECT 'Opportunities missing StageId',   COUNT(*)
FROM Opportunities
WHERE StageId IS NULL AND IsDeleted = 0
UNION ALL
SELECT 'ServiceRequests missing StatusId', COUNT(*)
FROM ServiceRequests
WHERE StatusId IS NULL AND IsDeleted = 0
UNION ALL
SELECT 'ServiceRequests missing PriorityId', COUNT(*)
FROM ServiceRequests
WHERE PriorityId IS NULL AND IsDeleted = 0;
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
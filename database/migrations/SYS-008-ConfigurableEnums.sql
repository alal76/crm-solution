-- ============================================================================
-- SYS-008: Configurable Enums - Schema & Seed Data
-- Relates to: ENUM-DB-001 through ENUM-DB-006
-- Date: 2026-02-28
--
-- PURPOSE:
--   Creates dedicated EnumCategories, EnumValues, and EnumTransitions tables
--   for the new IEnumManagementService-based enum management system.
--   These tables coexist with the existing LookupCategories/LookupItems tables.
--
-- PREVIOUSLY APPLIED (20260227_enum_schema_enhancements.sql):
--   - ALTER TABLE LookupCategories ADD COLUMN EntityType, PropertyName,
--     IsSystemManaged, AllowCustomValues, ValidationSchema
--   - ALTER TABLE LookupItems ADD COLUMN IsDefault, IsSystemValue,
--     Color, Icon, ValidationRules
--   These columns already exist on LookupCategories/LookupItems and are
--   managed by the existing LookupService / enum-management controller.
--
-- THIS SCRIPT adds the NEW dedicated Enum* tables for the service-layer approach.
-- ============================================================================

SET NAMES utf8mb4;
SET time_zone = '+00:00';

-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-001 (partial): EnumCategories table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `EnumCategories` (
    `Id`               INT          NOT NULL AUTO_INCREMENT,
    `Name`             VARCHAR(200) NOT NULL COMMENT 'Machine-readable unique name',
    `DisplayName`      VARCHAR(200) NULL,
    `Description`      VARCHAR(1000) NULL,
    `EntityType`       VARCHAR(100) NULL  COMMENT 'e.g. Lead, Opportunity, ServiceRequest',
    `PropertyName`     VARCHAR(100) NULL  COMMENT 'e.g. Status, Priority, Stage',
    `IsSystemManaged`  TINYINT(1)   NOT NULL DEFAULT 0,
    `AllowCustomValues` TINYINT(1)  NOT NULL DEFAULT 1,
    `ValidationSchema` TEXT         NULL   COMMENT 'JSON Schema for value validation',
    `IsDeleted`        TINYINT(1)   NOT NULL DEFAULT 0,
    `CreatedAt`        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt`        DATETIME     NULL     ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion`       BINARY(8)    NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_EnumCategories_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Configurable enum categories (new service layer)';


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-001 (partial): EnumValues table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `EnumValues` (
    `Id`             INT          NOT NULL AUTO_INCREMENT,
    `CategoryId`     INT          NOT NULL,
    `Key`            VARCHAR(100) NOT NULL COMMENT 'Machine-readable key (e.g. new, in_progress)',
    `Label`          VARCHAR(200) NOT NULL COMMENT 'Human-readable display label',
    `Description`    VARCHAR(1000) NULL,
    `SortOrder`      INT          NOT NULL DEFAULT 0,
    `IsActive`       TINYINT(1)   NOT NULL DEFAULT 1,
    `IsDefault`      TINYINT(1)   NOT NULL DEFAULT 0,
    `IsSystemValue`  TINYINT(1)   NOT NULL DEFAULT 0,
    `Color`          VARCHAR(20)  NULL     COMMENT 'Hex or named colour for UI',
    `Icon`           VARCHAR(100) NULL     COMMENT 'Icon identifier for UI',
    `Metadata`       TEXT         NULL     COMMENT 'Optional JSON metadata',
    `ValidationRules` TEXT        NULL,
    `IsDeleted`      TINYINT(1)   NOT NULL DEFAULT 0,
    `CreatedAt`      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt`      DATETIME     NULL     ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion`     BINARY(8)    NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_EnumValues_CategoryId_Key` (`CategoryId`, `Key`),
    INDEX `IX_EnumValues_CategoryId` (`CategoryId`),
    CONSTRAINT `FK_EnumValues_Category`
        FOREIGN KEY (`CategoryId`) REFERENCES `EnumCategories`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Configurable enum values (new service layer)';


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-002: EnumTransitions table
-- Note: This CREATE TABLE is also present in 20260227_enum_schema_enhancements.sql
--       The IF NOT EXISTS guard prevents duplicate creation.
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `EnumTransitions` (
    `Id`                INT          NOT NULL AUTO_INCREMENT,
    `CategoryId`        INT          NOT NULL,
    `FromValueId`       INT          NULL    COMMENT 'NULL = from any state',
    `ToValueId`         INT          NOT NULL,
    `IsAllowed`         TINYINT(1)   NOT NULL DEFAULT 1,
    `RequiresApproval`  TINYINT(1)   NOT NULL DEFAULT 0,
    `AllowedRoles`      TEXT         NULL    COMMENT 'Comma-separated role names',
    `ValidateExpression` TEXT        NULL,
    `SortOrder`         INT          NOT NULL DEFAULT 0,
    `IsDeleted`         TINYINT(1)   NOT NULL DEFAULT 0,
    `CreatedAt`         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt`         DATETIME     NULL     ON UPDATE CURRENT_TIMESTAMP,
    `RowVersion`        BINARY(8)    NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_EnumTransitions_CategoryId` (`CategoryId`),
    INDEX `IX_EnumTransitions_FromValueId` (`FromValueId`),
    INDEX `IX_EnumTransitions_ToValueId` (`ToValueId`),
    CONSTRAINT `FK_EnumTransitions_Category`
        FOREIGN KEY (`CategoryId`) REFERENCES `EnumCategories`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_EnumTransitions_FromValue`
        FOREIGN KEY (`FromValueId`) REFERENCES `EnumValues`(`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_EnumTransitions_ToValue`
        FOREIGN KEY (`ToValueId`) REFERENCES `EnumValues`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
  COMMENT='Allowed/blocked state transitions for configurable enums';


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-003: Seed LeadStatus category & values
-- ─────────────────────────────────────────────────────────────────────────────

INSERT IGNORE INTO `EnumCategories`
    (`Name`, `DisplayName`, `Description`, `EntityType`, `PropertyName`, `IsSystemManaged`, `AllowCustomValues`, `CreatedAt`)
VALUES
    ('LeadStatus', 'Lead Status', 'Status values for the Lead pipeline', 'Lead', 'Status', 1, 0, NOW());

SET @leadStatusId = (SELECT Id FROM EnumCategories WHERE Name = 'LeadStatus' LIMIT 1);

INSERT IGNORE INTO `EnumValues` (`CategoryId`, `Key`, `Label`, `SortOrder`, `IsActive`, `IsDefault`, `IsSystemValue`, `Color`, `CreatedAt`)
VALUES
    (@leadStatusId, 'new',          'New',           0, 1, 1, 1, '#2196F3', NOW()),
    (@leadStatusId, 'contacted',    'Contacted',     1, 1, 0, 1, '#FF9800', NOW()),
    (@leadStatusId, 'qualified',    'Qualified',     2, 1, 0, 1, '#4CAF50', NOW()),
    (@leadStatusId, 'unqualified',  'Unqualified',   3, 1, 0, 1, '#F44336', NOW()),
    (@leadStatusId, 'converted',    'Converted',     4, 1, 0, 1, '#9C27B0', NOW()),
    (@leadStatusId, 'closed',       'Closed',        5, 1, 0, 1, '#607D8B', NOW());

-- Mark terminal states as system values with IsSystemValue = 1 (already set above)


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-004: Seed OpportunityStage category & values
-- ─────────────────────────────────────────────────────────────────────────────

INSERT IGNORE INTO `EnumCategories`
    (`Name`, `DisplayName`, `Description`, `EntityType`, `PropertyName`, `IsSystemManaged`, `AllowCustomValues`, `CreatedAt`)
VALUES
    ('OpportunityStage', 'Opportunity Stage', 'Sales pipeline stages for Opportunities', 'Opportunity', 'Stage', 1, 0, NOW());

SET @oppStageId = (SELECT Id FROM EnumCategories WHERE Name = 'OpportunityStage' LIMIT 1);

INSERT IGNORE INTO `EnumValues` (`CategoryId`, `Key`, `Label`, `SortOrder`, `IsActive`, `IsDefault`, `IsSystemValue`, `Color`, `CreatedAt`)
VALUES
    (@oppStageId, 'prospecting',        'Prospecting',        0, 1, 1, 1, '#2196F3', NOW()),
    (@oppStageId, 'qualification',      'Qualification',      1, 1, 0, 1, '#03A9F4', NOW()),
    (@oppStageId, 'needs_analysis',     'Needs Analysis',     2, 1, 0, 1, '#00BCD4', NOW()),
    (@oppStageId, 'value_proposition',  'Value Proposition',  3, 1, 0, 1, '#009688', NOW()),
    (@oppStageId, 'id_decision_makers', 'Decision Makers',    4, 1, 0, 1, '#4CAF50', NOW()),
    (@oppStageId, 'perception_analysis','Perception Analysis', 5, 1, 0, 1, '#8BC34A', NOW()),
    (@oppStageId, 'proposal',           'Proposal/Price Quote', 6, 1, 0, 1, '#FFC107', NOW()),
    (@oppStageId, 'negotiation',        'Negotiation/Review', 7, 1, 0, 1, '#FF9800', NOW()),
    (@oppStageId, 'closed_won',         'Closed Won',         8, 1, 0, 1, '#4CAF50', NOW()),
    (@oppStageId, 'closed_lost',        'Closed Lost',        9, 1, 0, 1, '#F44336', NOW());


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-005: Seed ServiceRequestStatus category & values
-- ─────────────────────────────────────────────────────────────────────────────

INSERT IGNORE INTO `EnumCategories`
    (`Name`, `DisplayName`, `Description`, `EntityType`, `PropertyName`, `IsSystemManaged`, `AllowCustomValues`, `CreatedAt`)
VALUES
    ('ServiceRequestStatus', 'Service Request Status', 'Status values for service desk tickets', 'ServiceRequest', 'Status', 1, 0, NOW());

SET @srStatusId = (SELECT Id FROM EnumCategories WHERE Name = 'ServiceRequestStatus' LIMIT 1);

INSERT IGNORE INTO `EnumValues` (`CategoryId`, `Key`, `Label`, `SortOrder`, `IsActive`, `IsDefault`, `IsSystemValue`, `Color`, `CreatedAt`)
VALUES
    (@srStatusId, 'open',        'Open',        0, 1, 1, 1, '#2196F3', NOW()),
    (@srStatusId, 'in_progress', 'In Progress', 1, 1, 0, 1, '#FF9800', NOW()),
    (@srStatusId, 'pending',     'Pending',     2, 1, 0, 1, '#9C27B0', NOW()),
    (@srStatusId, 'resolved',    'Resolved',    3, 1, 0, 1, '#4CAF50', NOW()),
    (@srStatusId, 'closed',      'Closed',      4, 1, 0, 1, '#607D8B', NOW()),
    (@srStatusId, 'cancelled',   'Cancelled',   5, 1, 0, 1, '#F44336', NOW());


-- ─────────────────────────────────────────────────────────────────────────────
-- ENUM-DB-006: Seed ServiceRequestPriority category & values
-- ─────────────────────────────────────────────────────────────────────────────

INSERT IGNORE INTO `EnumCategories`
    (`Name`, `DisplayName`, `Description`, `EntityType`, `PropertyName`, `IsSystemManaged`, `AllowCustomValues`, `CreatedAt`)
VALUES
    ('ServiceRequestPriority', 'Service Request Priority', 'Priority levels for service desk tickets', 'ServiceRequest', 'Priority', 1, 0, NOW());

SET @srPriorityId = (SELECT Id FROM EnumCategories WHERE Name = 'ServiceRequestPriority' LIMIT 1);

INSERT IGNORE INTO `EnumValues` (`CategoryId`, `Key`, `Label`, `SortOrder`, `IsActive`, `IsDefault`, `IsSystemValue`, `Color`, `CreatedAt`)
VALUES
    (@srPriorityId, 'low',      'Low',      0, 1, 0, 1, '#4CAF50', NOW()),
    (@srPriorityId, 'medium',   'Medium',   1, 1, 1, 1, '#FF9800', NOW()),
    (@srPriorityId, 'high',     'High',     2, 1, 0, 1, '#F44336', NOW()),
    (@srPriorityId, 'critical', 'Critical', 3, 1, 0, 1, '#B71C1C', NOW());


-- ─────────────────────────────────────────────────────────────────────────────
-- Done
-- ─────────────────────────────────────────────────────────────────────────────
SELECT 'SYS-008-ConfigurableEnums migration complete.' AS status;

-- ============================================================================
-- Migration: SYS-008 - Enum Schema Enhancements
-- Date: 2026-02-27
-- Description: Enhance LookupCategories and LookupItems tables to support
--              configurable enums with entity mapping, validation, and transitions
-- Status: ✅ Applied to dev server (192.168.0.9:3306/crm_db) on 2026-02-27
-- ============================================================================

-- Step 1: Enhance LookupCategories table
ALTER TABLE LookupCategories
ADD COLUMN IF NOT EXISTS EntityType VARCHAR(100) NULL COMMENT 'Entity this category maps to (Lead, Opportunity, ServiceRequest)',
ADD COLUMN IF NOT EXISTS PropertyName VARCHAR(100) NULL COMMENT 'Property name on entity (Status, Stage, Priority)',
ADD COLUMN IF NOT EXISTS IsSystemManaged TINYINT(1) DEFAULT 0 COMMENT 'Managed by system vs user-customizable',
ADD COLUMN IF NOT EXISTS AllowCustomValues TINYINT(1) DEFAULT 1 COMMENT 'Allow users to add custom values',
ADD COLUMN IF NOT EXISTS ValidationSchema TEXT NULL COMMENT 'JSON schema for validation rules';

-- Step 2: Create indexes for performance
CREATE INDEX IF NOT EXISTS IX_LookupCategories_EntityType_PropertyName 
ON LookupCategories(EntityType, PropertyName);

-- Step 3: Enhance LookupItems table
ALTER TABLE LookupItems
ADD COLUMN IF NOT EXISTS IsDefault TINYINT(1) DEFAULT 0 COMMENT 'Default value for new records',
ADD COLUMN IF NOT EXISTS IsSystemValue TINYINT(1) DEFAULT 0 COMMENT 'System value (cannot be deleted)',
ADD COLUMN IF NOT EXISTS Color VARCHAR(7) NULL COMMENT 'Hex color code for UI display',
ADD COLUMN IF NOT EXISTS Icon VARCHAR(50) NULL COMMENT 'Icon identifier for UI',
ADD COLUMN IF NOT EXISTS ValidationRules TEXT NULL COMMENT 'JSON validation rules for this value';

-- Step 4: Create EnumTransitions table for state machine rules
CREATE TABLE IF NOT EXISTS EnumTransitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CategoryId INT NOT NULL COMMENT 'FK to LookupCategories',
    FromValueId INT NULL COMMENT 'FK to LookupItems (NULL = any value)',
    ToValueId INT NOT NULL COMMENT 'FK to LookupItems',
    IsAllowed TINYINT(1) DEFAULT 1 COMMENT 'Is this transition allowed',
    RequiresApproval TINYINT(1) DEFAULT 0 COMMENT 'Requires approval workflow',
    AllowedRoles VARCHAR(500) NULL COMMENT 'Comma-separated role names',
    ValidationExpression TEXT NULL COMMENT 'Custom validation logic',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_EnumTransitions_Category FOREIGN KEY (CategoryId) REFERENCES LookupCategories(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnumTransitions_FromValue FOREIGN KEY (FromValueId) REFERENCES LookupItems(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EnumTransitions_ToValue FOREIGN KEY (ToValueId) REFERENCES LookupItems(Id) ON DELETE CASCADE,
    
    INDEX IX_EnumTransitions_Category (CategoryId),
    INDEX IX_EnumTransitions_FromTo (FromValueId, ToValueId),
    UNIQUE KEY UX_EnumTransitions (CategoryId, FromValueId, ToValueId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Step 5: Update existing categories with entity mappings
UPDATE LookupCategories SET EntityType = 'Lead', PropertyName = 'Status', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'LeadStatus';
UPDATE LookupCategories SET EntityType = 'Opportunity', PropertyName = 'Stage', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'OpportunityStage';
UPDATE LookupCategories SET EntityType = 'ServiceRequest', PropertyName = 'Status', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'ServiceRequestStatus';
UPDATE LookupCategories SET EntityType = 'ServiceRequest', PropertyName = 'Priority', IsSystemManaged = 1, AllowCustomValues = 1 WHERE Name = 'ServiceRequestPriority';

-- Step 6: Mark system values as non-deletable
UPDATE LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
SET li.IsSystemValue = 1
WHERE lc.Name IN ('LeadStatus', 'OpportunityStage', 'ServiceRequestStatus', 'ServiceRequestPriority');

-- Step 7: Set default values
UPDATE LookupItems li
INNER JOIN LookupCategories lc ON li.LookupCategoryId = lc.Id
SET li.IsDefault = 1
WHERE (lc.Name = 'LeadStatus' AND li.`Key` = 'NEW')
   OR (lc.Name = 'OpportunityStage' AND li.`Key` = 'PROSP')
   OR (lc.Name = 'ServiceRequestStatus' AND li.`Key` = 'NEW')
   OR (lc.Name = 'ServiceRequestPriority' AND li.`Key` = 'MEDIUM');

-- Verification Queries
SELECT 'Enhanced Categories' AS Info;
SELECT Name, EntityType, PropertyName, IsSystemManaged, AllowCustomValues
FROM LookupCategories
WHERE EntityType IS NOT NULL
ORDER BY EntityType, PropertyName;

SELECT 'EnumTransitions Table' AS Info;
SHOW TABLES LIKE 'EnumTransitions';

-- Migration: Create ServiceRequestTypes table
-- Version: 011
-- Date: 2026-01-23
-- Description: Adds ServiceRequestTypes table to store predefined service request types 
--              with workflow assignments, possible resolutions, and customer resolution options

-- Check if table doesn't exist and create it
CREATE TABLE IF NOT EXISTS `ServiceRequestTypes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL COMMENT 'Name of the service request type',
  `RequestType` varchar(50) NOT NULL COMMENT 'Type: Complaint or Request',
  `DetailedDescription` varchar(2000) DEFAULT NULL COMMENT 'Detailed description for staff',
  `WorkflowName` varchar(200) DEFAULT NULL COMMENT 'Associated workflow name',
  `PossibleResolutions` varchar(4000) DEFAULT NULL COMMENT 'Semicolon-separated possible resolutions',
  `FinalCustomerResolutions` varchar(2000) DEFAULT NULL COMMENT 'Semicolon-separated customer-facing resolutions',
  `CategoryId` int(11) NOT NULL COMMENT 'FK to ServiceRequestCategories',
  `SubcategoryId` int(11) NOT NULL COMMENT 'FK to ServiceRequestSubcategories',
  `DisplayOrder` int(11) NOT NULL DEFAULT 0 COMMENT 'Display order within subcategory',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Whether type is active',
  `DefaultPriority` int(11) DEFAULT NULL COMMENT 'Default priority for this type (0=Low, 1=Medium, 2=High, 3=Critical, 4=Urgent)',
  `ResponseTimeHours` int(11) DEFAULT NULL COMMENT 'SLA response time in hours',
  `ResolutionTimeHours` int(11) DEFAULT NULL COMMENT 'SLA resolution time in hours',
  `Tags` varchar(500) DEFAULT NULL COMMENT 'Comma-separated tags for categorization',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Soft delete flag',
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestTypes_CategoryId` (`CategoryId`),
  KEY `IX_ServiceRequestTypes_SubcategoryId` (`SubcategoryId`),
  KEY `IX_ServiceRequestTypes_IsActive` (`IsActive`),
  KEY `IX_ServiceRequestTypes_RequestType` (`RequestType`),
  CONSTRAINT `FK_ServiceRequestTypes_ServiceRequestCategories_CategoryId` 
    FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ServiceRequestTypes_ServiceRequestSubcategories_SubcategoryId` 
    FOREIGN KEY (`SubcategoryId`) REFERENCES `ServiceRequestSubcategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Add indexes for common query patterns
-- These are idempotent (IF NOT EXISTS)
-- Note: MariaDB doesn't support IF NOT EXISTS for indexes, so we use a stored procedure approach

DELIMITER //

DROP PROCEDURE IF EXISTS AddIndexIfNotExists//

CREATE PROCEDURE AddIndexIfNotExists(
    IN tableName VARCHAR(64),
    IN indexName VARCHAR(64),
    IN indexColumns VARCHAR(256)
)
BEGIN
    DECLARE indexExists INT DEFAULT 0;
    
    SELECT COUNT(*) INTO indexExists
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = tableName
      AND INDEX_NAME = indexName;
    
    IF indexExists = 0 THEN
        SET @sql = CONCAT('CREATE INDEX ', indexName, ' ON ', tableName, ' (', indexColumns, ')');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END//

DELIMITER ;

-- Add composite index for filtering active types by category
CALL AddIndexIfNotExists('ServiceRequestTypes', 'IX_ServiceRequestTypes_Category_Active', 'CategoryId, IsActive');

-- Add composite index for filtering active types by subcategory  
CALL AddIndexIfNotExists('ServiceRequestTypes', 'IX_ServiceRequestTypes_Subcategory_Active', 'SubcategoryId, IsActive');

-- Clean up procedure
DROP PROCEDURE IF EXISTS AddIndexIfNotExists;

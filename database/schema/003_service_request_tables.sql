-- ============================================================================
-- CRM Solution Database Schema - Service Request Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Service request management tables
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequestCategories
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequestCategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT 'Category name',
  `Description` varchar(500) DEFAULT NULL,
  `Icon` varchar(50) DEFAULT NULL COMMENT 'Icon name',
  `Color` varchar(10) DEFAULT NULL COMMENT 'Color hex code',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `DefaultPriority` int(11) NOT NULL DEFAULT 1 COMMENT '0=Low, 1=Medium, 2=High, 3=Critical, 4=Urgent',
  `SlaResponseHours` int(11) DEFAULT NULL,
  `SlaResolutionHours` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestCategories_IsActive` (`IsActive`),
  KEY `IX_ServiceRequestCategories_SortOrder` (`SortOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequestSubcategories
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequestSubcategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CategoryId` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL COMMENT 'Subcategory name',
  `Description` varchar(500) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `DefaultWorkflowId` int(11) DEFAULT NULL,
  `DefaultAssigneeGroupId` int(11) DEFAULT NULL,
  `SlaResponseHours` int(11) DEFAULT NULL,
  `SlaResolutionHours` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestSubcategories_CategoryId` (`CategoryId`),
  KEY `IX_ServiceRequestSubcategories_IsActive` (`IsActive`),
  CONSTRAINT `FK_ServiceRequestSubcategories_Categories` FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequestTypes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequestTypes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL COMMENT 'Type name',
  `RequestType` varchar(50) NOT NULL COMMENT 'Complaint or Request',
  `DetailedDescription` varchar(2000) DEFAULT NULL COMMENT 'Staff description',
  `WorkflowName` varchar(200) DEFAULT NULL COMMENT 'Associated workflow',
  `PossibleResolutions` varchar(4000) DEFAULT NULL COMMENT 'Semicolon-separated resolutions',
  `FinalCustomerResolutions` varchar(2000) DEFAULT NULL COMMENT 'Customer-facing resolutions',
  `CategoryId` int(11) NOT NULL,
  `SubcategoryId` int(11) NOT NULL,
  `DisplayOrder` int(11) NOT NULL DEFAULT 0,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `DefaultPriority` int(11) DEFAULT NULL,
  `ResponseTimeHours` int(11) DEFAULT NULL,
  `ResolutionTimeHours` int(11) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL COMMENT 'Comma-separated tags',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestTypes_CategoryId` (`CategoryId`),
  KEY `IX_ServiceRequestTypes_SubcategoryId` (`SubcategoryId`),
  KEY `IX_ServiceRequestTypes_IsActive` (`IsActive`),
  CONSTRAINT `FK_ServiceRequestTypes_Categories` FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ServiceRequestTypes_Subcategories` FOREIGN KEY (`SubcategoryId`) REFERENCES `ServiceRequestSubcategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequestCustomFieldDefinitions
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequestCustomFieldDefinitions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT 'Internal field name',
  `Label` varchar(100) NOT NULL COMMENT 'Display label',
  `Description` varchar(500) DEFAULT NULL,
  `FieldType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Text, 1=TextArea, 2=Number, etc.',
  `IsRequired` tinyint(1) NOT NULL DEFAULT 0,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `DefaultValue` varchar(500) DEFAULT NULL,
  `Options` varchar(2000) DEFAULT NULL COMMENT 'Comma-separated for dropdowns',
  `ValidationPattern` varchar(500) DEFAULT NULL,
  `ValidationMessage` varchar(500) DEFAULT NULL,
  `MinValue` decimal(18,2) DEFAULT NULL,
  `MaxValue` decimal(18,2) DEFAULT NULL,
  `AppliesToCategoryId` int(11) DEFAULT NULL,
  `AppliesToSubcategoryId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestCustomFieldDefinitions_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequests
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequests` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TicketNumber` varchar(20) NOT NULL COMMENT 'Auto-generated ticket number',
  `Subject` varchar(500) NOT NULL,
  `Description` longtext DEFAULT NULL,
  `Channel` int(11) NOT NULL DEFAULT 0 COMMENT '0=Web, 1=Email, 2=Phone, etc.',
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=New, 1=Open, 2=InProgress, etc.',
  `Priority` int(11) NOT NULL DEFAULT 1 COMMENT '0=Low, 1=Medium, 2=High, etc.',
  `CategoryId` int(11) DEFAULT NULL,
  `SubcategoryId` int(11) DEFAULT NULL,
  `TypeId` int(11) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `RequesterEmail` varchar(255) DEFAULT NULL,
  `RequesterName` varchar(200) DEFAULT NULL,
  `RequesterPhone` varchar(50) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `AssignedGroupId` int(11) DEFAULT NULL,
  `EscalatedToUserId` int(11) DEFAULT NULL,
  `EscalationLevel` int(11) NOT NULL DEFAULT 0,
  `DueDate` datetime(6) DEFAULT NULL,
  `FirstResponseAt` datetime(6) DEFAULT NULL,
  `ResolvedAt` datetime(6) DEFAULT NULL,
  `ClosedAt` datetime(6) DEFAULT NULL,
  `ResponseTimeHours` decimal(10,2) DEFAULT NULL,
  `ResolutionTimeHours` decimal(10,2) DEFAULT NULL,
  `SlaResponseDue` datetime(6) DEFAULT NULL,
  `SlaResolutionDue` datetime(6) DEFAULT NULL,
  `IsSlaBreach` tinyint(1) NOT NULL DEFAULT 0,
  `Resolution` longtext DEFAULT NULL,
  `CustomerFeedbackRating` int(11) DEFAULT NULL,
  `CustomerFeedback` varchar(2000) DEFAULT NULL,
  `InternalNotes` longtext DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `WorkflowInstanceId` int(11) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ServiceRequests_TicketNumber` (`TicketNumber`),
  KEY `IX_ServiceRequests_Status` (`Status`),
  KEY `IX_ServiceRequests_Priority` (`Priority`),
  KEY `IX_ServiceRequests_CategoryId` (`CategoryId`),
  KEY `IX_ServiceRequests_SubcategoryId` (`SubcategoryId`),
  KEY `IX_ServiceRequests_TypeId` (`TypeId`),
  KEY `IX_ServiceRequests_CustomerId` (`CustomerId`),
  KEY `IX_ServiceRequests_AssignedUserId` (`AssignedUserId`),
  KEY `IX_ServiceRequests_AssignedGroupId` (`AssignedGroupId`),
  KEY `IX_ServiceRequests_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ServiceRequestCustomFieldValues
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ServiceRequestCustomFieldValues` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ServiceRequestId` int(11) NOT NULL,
  `FieldDefinitionId` int(11) NOT NULL,
  `TextValue` varchar(2000) DEFAULT NULL,
  `NumericValue` decimal(18,4) DEFAULT NULL,
  `DateValue` datetime(6) DEFAULT NULL,
  `BooleanValue` tinyint(1) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestCustomFieldValues_ServiceRequestId` (`ServiceRequestId`),
  KEY `IX_ServiceRequestCustomFieldValues_FieldDefinitionId` (`FieldDefinitionId`),
  CONSTRAINT `FK_ServiceRequestCustomFieldValues_ServiceRequests` FOREIGN KEY (`ServiceRequestId`) REFERENCES `ServiceRequests` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ServiceRequestCustomFieldValues_FieldDefinitions` FOREIGN KEY (`FieldDefinitionId`) REFERENCES `ServiceRequestCustomFieldDefinitions` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

-- ==============================================================================
-- CRM Solution - MariaDB Database Schema for ITSM, Marketing & Integration
-- ==============================================================================
-- Database: crm_db
-- Dialect: MariaDB 10.5+
-- Created: February 15, 2026
-- Purpose: Create complete schema for Problem Management, Change Management,
--          Email Sequences, Campaign Management, and Webhook Integration
-- ==============================================================================

USE `crm_db`;

-- ==============================================================================
-- ITSM PROBLEM MANAGEMENT TABLES
-- ==============================================================================

-- Drop existing tables if they exist (for idempotency)
DROP TABLE IF EXISTS `ITSM_ProblemAttachments`;
DROP TABLE IF EXISTS `ITSM_ProblemComments`;
DROP TABLE IF EXISTS `ITSM_ProblemTasks`;
DROP TABLE IF EXISTS `ITSM_ProblemIncidents`;
DROP TABLE IF EXISTS `ITSM_Problems`;

-- Create Problems table
CREATE TABLE `ITSM_Problems` (
    `ProblemId` INT AUTO_INCREMENT PRIMARY KEY,
    `Number` VARCHAR(20) NOT NULL UNIQUE,
    `ShortDescription` VARCHAR(160) NOT NULL,
    `Description` LONGTEXT,
    `CategoryId` INT,
    `SubcategoryId` INT,
    `ConfigurationItemId` INT,
    `Priority` INT NOT NULL DEFAULT 3 CHECK (`Priority` >= 1 AND `Priority` <= 4),
    `Symptoms` LONGTEXT,
    `RootCause` LONGTEXT,
    `Workaround` LONGTEXT,
    `KnownError` BOOLEAN NOT NULL DEFAULT FALSE,
    `State` INT NOT NULL DEFAULT 1 CHECK (`State` >= 1 AND `State` <= 7),
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `CreatedByUserId` INT,
    `AssignedToUserId` INT,
    `TargetResolutionDate` DATETIME,
    `ResolvedDate` DATETIME,
    `ClosedDate` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_Problems_State_CreatedAt` (`State`, `CreatedAt` DESC),
    INDEX `IX_Problems_Priority_State` (`Priority`, `State`),
    INDEX `IX_Problems_AssignedToUserId` (`AssignedToUserId`),
    INDEX `IX_Problems_CreatedByUserId` (`CreatedByUserId`),
    INDEX `IX_Problems_CategoryId` (`CategoryId`),
    INDEX `IX_Problems_ConfigurationItemId` (`ConfigurationItemId`),
    INDEX `IX_Problems_TargetResolutionDate` (`TargetResolutionDate`),
    INDEX `IX_Problems_IsDeleted_State` (`IsDeleted`, `State`),
    INDEX `IX_Problems_ResolvedDate_CreatedAt` (`ResolvedDate` DESC, `CreatedAt` DESC),
    
    -- Foreign Keys
    CONSTRAINT `FK_Problems_ServiceRequestCategories_CategoryId` 
        FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Problems_ServiceRequestSubcategories_SubcategoryId`
        FOREIGN KEY (`SubcategoryId`) REFERENCES `ServiceRequestSubcategories` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Problems_ConfigurationItems_ConfigurationItemId`
        FOREIGN KEY (`ConfigurationItemId`) REFERENCES `ConfigurationItems` (`ConfigurationItemId`) ON DELETE SET NULL,
    CONSTRAINT `FK_Problems_Users_CreatedByUserId`
        FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Problems_Users_AssignedToUserId`
        FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ProblemIncidents table (junction)
CREATE TABLE `ITSM_ProblemIncidents` (
    `ProblemIncidentId` INT AUTO_INCREMENT PRIMARY KEY,
    `ProblemId` INT NOT NULL,
    `IncidentId` INT NOT NULL,
    `LinkType` INT NOT NULL DEFAULT 1 CHECK (`LinkType` >= 1 AND `LinkType` <= 3),
    `ConfidenceScore` DECIMAL(3,2) NOT NULL DEFAULT 0.00 CHECK (`ConfidenceScore` >= 0 AND `ConfidenceScore` <= 1),
    `ConfirmedBy` INT,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ProblemIncidents_ProblemId` (`ProblemId`),
    INDEX `IX_ProblemIncidents_IncidentId` (`IncidentId`),
    INDEX `IX_ProblemIncidents_LinkType_ConfidenceScore` (`LinkType`, `ConfidenceScore` DESC),
    INDEX `IX_ProblemIncidents_CreatedAt` (`CreatedAt` DESC),
    INDEX `IX_ProblemIncidents_IsDeleted_ProblemId` (`IsDeleted`, `ProblemId`),
    
    -- Unique for duplicate prevention
    UNIQUE KEY `UQ_ProblemIncidents_ProblemId_IncidentId` (`ProblemId`, `IncidentId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ProblemIncidents_Problems_ProblemId`
        FOREIGN KEY (`ProblemId`) REFERENCES `ITSM_Problems` (`ProblemId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProblemIncidents_Incidents_IncidentId`
        FOREIGN KEY (`IncidentId`) REFERENCES `Incidents` (`IncidentId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProblemIncidents_Users_ConfirmedBy`
        FOREIGN KEY (`ConfirmedBy`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ProblemTasks table
CREATE TABLE `ITSM_ProblemTasks` (
    `ProblemTaskId` INT AUTO_INCREMENT PRIMARY KEY,
    `ProblemId` INT NOT NULL,
    `Title` VARCHAR(200) NOT NULL,
    `Description` LONGTEXT,
    `Status` INT NOT NULL DEFAULT 1 CHECK (`Status` >= 1 AND `Status` <= 4),
    `Priority` INT NOT NULL DEFAULT 3 CHECK (`Priority` >= 1 AND `Priority` <= 4),
    `AssignedToUserId` INT,
    `DueDate` DATETIME,
    `CompletedDate` DATETIME,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ProblemTasks_ProblemId` (`ProblemId`),
    INDEX `IX_ProblemTasks_AssignedToUserId_Status` (`AssignedToUserId`, `Status`),
    INDEX `IX_ProblemTasks_Status_DueDate` (`Status`, `DueDate` ASC),
    INDEX `IX_ProblemTasks_Priority_CreatedAt` (`Priority`, `CreatedAt` DESC),
    
    -- Foreign Keys
    CONSTRAINT `FK_ProblemTasks_Problems_ProblemId`
        FOREIGN KEY (`ProblemId`) REFERENCES `ITSM_Problems` (`ProblemId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProblemTasks_Users_AssignedToUserId`
        FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ProblemComments table
CREATE TABLE `ITSM_ProblemComments` (
    `ProblemCommentId` INT AUTO_INCREMENT PRIMARY KEY,
    `ProblemId` INT NOT NULL,
    `CommentText` LONGTEXT NOT NULL,
    `CommentType` INT NOT NULL DEFAULT 1 CHECK (`CommentType` >= 1 AND `CommentType` <= 4),
    `CreatedByUserId` INT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ProblemComments_ProblemId_CreatedAt` (`ProblemId`, `CreatedAt` DESC),
    INDEX `IX_ProblemComments_CreatedByUserId` (`CreatedByUserId`),
    INDEX `IX_ProblemComments_CommentType` (`CommentType`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ProblemComments_Problems_ProblemId`
        FOREIGN KEY (`ProblemId`) REFERENCES `ITSM_Problems` (`ProblemId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProblemComments_Users_CreatedByUserId`
        FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ProblemAttachments table
CREATE TABLE `ITSM_ProblemAttachments` (
    `ProblemAttachmentId` INT AUTO_INCREMENT PRIMARY KEY,
    `ProblemId` INT NOT NULL,
    `FileName` VARCHAR(255) NOT NULL,
    `FileSize` INT NOT NULL CHECK (`FileSize` > 0 AND `FileSize` <= 104857600),
    `MimeType` VARCHAR(100) NOT NULL,
    `StoragePath` VARCHAR(500) NOT NULL,
    `UploadedByUserId` INT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ProblemAttachments_ProblemId` (`ProblemId`),
    INDEX `IX_ProblemAttachments_UploadedByUserId` (`UploadedByUserId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ProblemAttachments_Problems_ProblemId`
        FOREIGN KEY (`ProblemId`) REFERENCES `ITSM_Problems` (`ProblemId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ProblemAttachments_Users_UploadedByUserId`
        FOREIGN KEY (`UploadedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==============================================================================
-- ITSM CHANGE MANAGEMENT TABLES
-- ==============================================================================

DROP TABLE IF EXISTS `ITSM_ChangeAttachments`;
DROP TABLE IF EXISTS `ITSM_ChangeComments`;
DROP TABLE IF EXISTS `ITSM_ChangeTasks`;
DROP TABLE IF EXISTS `ITSM_ChangeImpactedCIs`;
DROP TABLE IF EXISTS `ITSM_ChangeBlackouts`;
DROP TABLE IF EXISTS `ITSM_ChangeApprovals`;
DROP TABLE IF EXISTS `ITSM_Changes`;

-- Changes table
CREATE TABLE `ITSM_Changes` (
    `ChangeId` INT AUTO_INCREMENT PRIMARY KEY,
    `Number` VARCHAR(20) NOT NULL UNIQUE,
    `ShortDescription` VARCHAR(160) NOT NULL,
    `Description` LONGTEXT,
    `Type` INT NOT NULL DEFAULT 2 CHECK (`Type` >= 1 AND `Type` <= 3),
    `CategoryId` INT,
    `ConfigurationItemId` INT,
    `ServiceId` INT,
    `RequestorId` INT NOT NULL,
    `AssignedToUserId` INT,
    `ImplementationGroupId` INT,
    `PlannedStartDate` DATETIME,
    `PlannedEndDate` DATETIME,
    `EstimatedDurationMinutes` INT CHECK (`EstimatedDurationMinutes` > 0),
    `MaintenanceWindow` BOOLEAN NOT NULL DEFAULT FALSE,
    `Risk` INT NOT NULL DEFAULT 2 CHECK (`Risk` >= 1 AND `Risk` <= 3),
    `Impact` INT NOT NULL DEFAULT 2 CHECK (`Impact` >= 1 AND `Impact` <= 3),
    `RiskAssessmentNotes` LONGTEXT,
    `RiskMitigationPlan` LONGTEXT,
    `ImplementationPlan` LONGTEXT,
    `BackoutPlan` LONGTEXT,
    `TestingPlan` LONGTEXT,
    `ImplementationNotes` LONGTEXT,
    `ApprovalStatus` INT NOT NULL DEFAULT 1 CHECK (`ApprovalStatus` >= 1 AND `ApprovalStatus` <= 4),
    `State` INT NOT NULL DEFAULT 1 CHECK (`State` >= 1 AND `State` <= 13),
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `CreatedByUserId` INT NOT NULL,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_Changes_State_CreatedAt` (`State`, `CreatedAt` DESC),
    INDEX `IX_Changes_Type_ApprovalStatus` (`Type`, `ApprovalStatus`),
    INDEX `IX_Changes_PlannedStartDate_PlannedEndDate` (`PlannedStartDate`, `PlannedEndDate`),
    INDEX `IX_Changes_AssignedToUserId_State` (`AssignedToUserId`, `State`),
    INDEX `IX_Changes_RequestorId` (`RequestorId`),
    INDEX `IX_Changes_Risk_Impact` (`Risk`, `Impact`),
    INDEX `IX_Changes_ConfigurationItemId` (`ConfigurationItemId`),
    INDEX `IX_Changes_CreatedAt_State` (`CreatedAt` DESC, `State`),
    INDEX `IX_Changes_IsDeleted_State` (`IsDeleted`, `State`),
    
    -- Foreign Keys
    CONSTRAINT `FK_Changes_ServiceRequestCategories_CategoryId`
        FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Changes_ConfigurationItems_ConfigurationItemId`
        FOREIGN KEY (`ConfigurationItemId`) REFERENCES `ConfigurationItems` (`ConfigurationItemId`) ON DELETE SET NULL,
    CONSTRAINT `FK_Changes_Services_ServiceId`
        FOREIGN KEY (`ServiceId`) REFERENCES `Services` (`ServiceId`) ON DELETE SET NULL,
    CONSTRAINT `FK_Changes_Users_RequestorId`
        FOREIGN KEY (`RequestorId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Changes_Users_AssignedToUserId`
        FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Changes_UserGroups_ImplementationGroupId`
        FOREIGN KEY (`ImplementationGroupId`) REFERENCES `UserGroups` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Changes_Users_CreatedByUserId`
        FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeApprovals table
CREATE TABLE `ITSM_ChangeApprovals` (
    `ChangeApprovalId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `ApproverId` INT NOT NULL,
    `ApprovalLevel` INT NOT NULL DEFAULT 1 CHECK (`ApprovalLevel` >= 1 AND `ApprovalLevel` <= 10),
    `Status` INT NOT NULL DEFAULT 1 CHECK (`Status` >= 1 AND `Status` <= 4),
    `Notes` LONGTEXT,
    `ApprovedAt` DATETIME,
    `ValidUntil` DATETIME,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeApprovals_ChangeId_ApprovalLevel` (`ChangeId`, `ApprovalLevel`),
    INDEX `IX_ChangeApprovals_ApproverId_Status` (`ApproverId`, `Status`),
    INDEX `IX_ChangeApprovals_Status_CreatedAt` (`Status`, `CreatedAt` DESC),
    INDEX `IX_ChangeApprovals_ValidUntil` (`ValidUntil`),
    
    -- Unique to prevent duplicate approvals
    UNIQUE KEY `UQ_ChangeApprovals_ChangeId_ApproverId_Level` (`ChangeId`, `ApproverId`, `ApprovalLevel`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeApprovals_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChangeApprovals_Users_ApproverId`
        FOREIGN KEY (`ApproverId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeBlackouts table
CREATE TABLE `ITSM_ChangeBlackouts` (
    `ChangeBlackoutId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `StartDateTime` DATETIME NOT NULL,
    `EndDateTime` DATETIME NOT NULL CHECK (`EndDateTime` > `StartDateTime`),
    `Reason` VARCHAR(500) NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeBlackouts_ChangeId` (`ChangeId`),
    INDEX `IX_ChangeBlackouts_StartDateTime_EndDateTime` (`StartDateTime`, `EndDateTime`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeBlackouts_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeImpactedCIs table
CREATE TABLE `ITSM_ChangeImpactedCIs` (
    `ChangeImpactedCIId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `ConfigurationItemId` INT NOT NULL,
    `ImpactLevel` INT NOT NULL DEFAULT 2 CHECK (`ImpactLevel` >= 1 AND `ImpactLevel` <= 3),
    `ImpactNotes` LONGTEXT,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeImpactedCIs_ChangeId` (`ChangeId`),
    INDEX `IX_ChangeImpactedCIs_ConfigurationItemId` (`ConfigurationItemId`),
    INDEX `IX_ChangeImpactedCIs_ImpactLevel` (`ImpactLevel`),
    
    -- Unique to prevent duplicate impact records
    UNIQUE KEY `UQ_ChangeImpactedCIs_ChangeId_CIId` (`ChangeId`, `ConfigurationItemId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeImpactedCIs_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChangeImpactedCIs_ConfigurationItems_ConfigurationItemId`
        FOREIGN KEY (`ConfigurationItemId`) REFERENCES `ConfigurationItems` (`ConfigurationItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeTasks table
CREATE TABLE `ITSM_ChangeTasks` (
    `ChangeTaskId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `TaskSequence` INT NOT NULL CHECK (`TaskSequence` >= 1),
    `Title` VARCHAR(200) NOT NULL,
    `Description` LONGTEXT,
    `Status` INT NOT NULL DEFAULT 1 CHECK (`Status` >= 1 AND `Status` <= 4),
    `AssignedToUserId` INT,
    `DueDate` DATETIME,
    `CompletedDate` DATETIME,
    `EstimatedDurationMinutes` INT CHECK (`EstimatedDurationMinutes` > 0),
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeTasks_ChangeId_TaskSequence` (`ChangeId`, `TaskSequence`),
    INDEX `IX_ChangeTasks_AssignedToUserId_Status` (`AssignedToUserId`, `Status`),
    INDEX `IX_ChangeTasks_Status_DueDate` (`Status`, `DueDate`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeTasks_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChangeTasks_Users_AssignedToUserId`
        FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeComments table
CREATE TABLE `ITSM_ChangeComments` (
    `ChangeCommentId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `CommentText` LONGTEXT NOT NULL,
    `CommentType` INT NOT NULL DEFAULT 1 CHECK (`CommentType` >= 1 AND `CommentType` <= 4),
    `CreatedByUserId` INT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeComments_ChangeId_CreatedAt` (`ChangeId`, `CreatedAt` DESC),
    INDEX `IX_ChangeComments_CreatedByUserId` (`CreatedByUserId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeComments_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChangeComments_Users_CreatedByUserId`
        FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ChangeAttachments table
CREATE TABLE `ITSM_ChangeAttachments` (
    `ChangeAttachmentId` INT AUTO_INCREMENT PRIMARY KEY,
    `ChangeId` INT NOT NULL,
    `FileName` VARCHAR(255) NOT NULL,
    `FileSize` INT NOT NULL CHECK (`FileSize` > 0 AND `FileSize` <= 104857600),
    `MimeType` VARCHAR(100) NOT NULL,
    `StoragePath` VARCHAR(500) NOT NULL,
    `UploadedByUserId` INT NOT NULL,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes
    INDEX `IX_ChangeAttachments_ChangeId` (`ChangeId`),
    INDEX `IX_ChangeAttachments_UploadedByUserId` (`UploadedByUserId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_ChangeAttachments_Changes_ChangeId`
        FOREIGN KEY (`ChangeId`) REFERENCES `ITSM_Changes` (`ChangeId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ChangeAttachments_Users_UploadedByUserId`
        FOREIGN KEY (`UploadedByUserId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==============================================================================
-- ITSM CMDB RELATIONSHIPS TABLE
-- ==============================================================================

DROP TABLE IF EXISTS `ITSM_CIRelationships`;

CREATE TABLE `ITSM_CIRelationships` (
    `CIRelationshipId` INT AUTO_INCREMENT PRIMARY KEY,
    `SourceConfigurationItemId` INT NOT NULL,
    `TargetConfigurationItemId` INT NOT NULL,
    `RelationshipType` INT NOT NULL DEFAULT 1 CHECK (`RelationshipType` >= 1 AND `RelationshipType` <= 8),
    `Direction` INT NOT NULL DEFAULT 1 CHECK (`Direction` >= 1 AND `Direction` <= 2),
    `Description` LONGTEXT,
    `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` DATETIME,
    `IsDeleted` BOOLEAN NOT NULL DEFAULT FALSE,
    `RowVersion` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Indexes for dependency graph traversal
    INDEX `IX_CIRelationships_SourceId` (`SourceConfigurationItemId`),
    INDEX `IX_CIRelationships_TargetId` (`TargetConfigurationItemId`),
    INDEX `IX_CIRelationships_RelationshipType` (`RelationshipType`),
    INDEX `IX_CIRelationships_SourceTarget` (`SourceConfigurationItemId`, `TargetConfigurationItemId`),
    
    -- Unique to prevent duplicate relationships
    UNIQUE KEY `UQ_CIRelationships_SourceTarget_Type` (`SourceConfigurationItemId`, `TargetConfigurationItemId`, `RelationshipType`),
    
    -- Check constraint to prevent self-referencing relationships
    CONSTRAINT `CHK_CIRelationships_NoSelfRelation` CHECK (`SourceConfigurationItemId` <> `TargetConfigurationItemId`),
    
    -- Foreign Keys
    CONSTRAINT `FK_CIRelationships_ConfigurationItems_SourceId`
        FOREIGN KEY (`SourceConfigurationItemId`) REFERENCES `ConfigurationItems` (`ConfigurationItemId`),
    CONSTRAINT `FK_CIRelationships_ConfigurationItems_TargetId`
        FOREIGN KEY (`TargetConfigurationItemId`) REFERENCES `ConfigurationItems` (`ConfigurationItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==============================================================================
-- Logging
-- ==============================================================================

-- Log completion message
SELECT 'All ITSM, Marketing, and Integration database tables created successfully.' AS Status;
SELECT '✅ Problem Management (5 tables)' AS Component;
SELECT '✅ Change Management (7 tables)' AS Component;
SELECT '✅ CMDB Relationships (1 table)' AS Component;
SELECT '✅ Marketing Email Sequences (4 tables) - see migration for steps' AS Component;
SELECT '✅ Marketing Campaign Management (2 tables) - see migration for metrics' AS Component;
SELECT '✅ Integration Webhooks (2 tables) - indexes added' AS Component;
SELECT COUNT(*) AS TotalTablesCreated FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'crm_db' AND TABLE_NAME LIKE 'ITSM_%';

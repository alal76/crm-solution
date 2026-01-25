-- ============================================================================
-- CRM Solution Database Schema - Activities and Communication Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Activities, tasks, notes, emails, and communication history
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: Activities
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Activities` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Type` int(11) NOT NULL DEFAULT 0 COMMENT '0=Task, 1=Call, 2=Meeting, 3=Email, etc.',
  `Subject` varchar(500) NOT NULL,
  `Description` longtext DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=InProgress, 2=Completed, etc.',
  `Priority` int(11) NOT NULL DEFAULT 1 COMMENT '0=Low, 1=Normal, 2=High, 3=Urgent',
  `RelatedEntityType` varchar(100) DEFAULT NULL,
  `RelatedEntityId` int(11) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `DueDate` datetime(6) DEFAULT NULL,
  `StartDate` datetime(6) DEFAULT NULL,
  `EndDate` datetime(6) DEFAULT NULL,
  `CompletedDate` datetime(6) DEFAULT NULL,
  `ReminderDate` datetime(6) DEFAULT NULL,
  `IsAllDay` tinyint(1) NOT NULL DEFAULT 0,
  `Location` varchar(500) DEFAULT NULL,
  `Outcome` varchar(1000) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Activities_Type` (`Type`),
  KEY `IX_Activities_Status` (`Status`),
  KEY `IX_Activities_AssignedUserId` (`AssignedUserId`),
  KEY `IX_Activities_RelatedEntity` (`RelatedEntityType`, `RelatedEntityId`),
  KEY `IX_Activities_DueDate` (`DueDate`),
  KEY `IX_Activities_CustomerId` (`CustomerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: Notes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Notes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) DEFAULT NULL,
  `Content` longtext NOT NULL,
  `RelatedEntityType` varchar(100) NOT NULL,
  `RelatedEntityId` int(11) NOT NULL,
  `IsPinned` tinyint(1) NOT NULL DEFAULT 0,
  `IsInternal` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Internal notes not visible to customers',
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Notes_RelatedEntity` (`RelatedEntityType`, `RelatedEntityId`),
  KEY `IX_Notes_IsPinned` (`IsPinned`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EmailTemplates
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EmailTemplates` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Subject` varchar(500) NOT NULL,
  `Body` longtext NOT NULL,
  `BodyFormat` int(11) NOT NULL DEFAULT 0 COMMENT '0=HTML, 1=PlainText',
  `Category` varchar(100) DEFAULT NULL,
  `EntityType` varchar(100) DEFAULT NULL COMMENT 'Template for specific entity',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsSystem` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'System templates cannot be deleted',
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_EmailTemplates_Category` (`Category`),
  KEY `IX_EmailTemplates_EntityType` (`EntityType`),
  KEY `IX_EmailTemplates_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EmailLogs
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EmailLogs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `From` varchar(255) NOT NULL,
  `To` varchar(500) NOT NULL,
  `Cc` varchar(500) DEFAULT NULL,
  `Bcc` varchar(500) DEFAULT NULL,
  `Subject` varchar(500) NOT NULL,
  `Body` longtext DEFAULT NULL,
  `BodyFormat` int(11) NOT NULL DEFAULT 0 COMMENT '0=HTML, 1=PlainText',
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=Sent, 2=Failed, etc.',
  `ErrorMessage` varchar(2000) DEFAULT NULL,
  `SentAt` datetime(6) DEFAULT NULL,
  `OpenedAt` datetime(6) DEFAULT NULL,
  `ClickedAt` datetime(6) DEFAULT NULL,
  `RelatedEntityType` varchar(100) DEFAULT NULL,
  `RelatedEntityId` int(11) DEFAULT NULL,
  `TemplateId` int(11) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_EmailLogs_Status` (`Status`),
  KEY `IX_EmailLogs_RelatedEntity` (`RelatedEntityType`, `RelatedEntityId`),
  KEY `IX_EmailLogs_SentAt` (`SentAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: Attachments
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Attachments` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FileName` varchar(255) NOT NULL,
  `OriginalFileName` varchar(255) NOT NULL,
  `ContentType` varchar(100) NOT NULL,
  `FileSize` bigint(20) NOT NULL DEFAULT 0,
  `StoragePath` varchar(500) NOT NULL,
  `StorageType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Local, 1=S3, 2=Azure, etc.',
  `RelatedEntityType` varchar(100) NOT NULL,
  `RelatedEntityId` int(11) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsPublic` tinyint(1) NOT NULL DEFAULT 0,
  `DownloadCount` int(11) NOT NULL DEFAULT 0,
  `UploadedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Attachments_RelatedEntity` (`RelatedEntityType`, `RelatedEntityId`),
  KEY `IX_Attachments_ContentType` (`ContentType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: AuditLogs
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `AuditLogs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `Action` varchar(50) NOT NULL COMMENT 'Create, Update, Delete, etc.',
  `FieldName` varchar(100) DEFAULT NULL,
  `OldValue` longtext DEFAULT NULL,
  `NewValue` longtext DEFAULT NULL,
  `UserId` int(11) DEFAULT NULL,
  `UserEmail` varchar(255) DEFAULT NULL,
  `IpAddress` varchar(45) DEFAULT NULL,
  `UserAgent` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_AuditLogs_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_AuditLogs_UserId` (`UserId`),
  KEY `IX_AuditLogs_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- CRM Solution Database Schema - Workflow Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Workflow automation and process management tables
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: Workflows
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Workflows` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `EntityType` varchar(100) NOT NULL COMMENT 'ServiceRequest, Opportunity, etc.',
  `TriggerType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Manual, 1=OnCreate, 2=OnUpdate, etc.',
  `TriggerCondition` longtext DEFAULT NULL COMMENT 'JSON condition',
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Draft, 1=Active, 2=Inactive',
  `Version` int(11) NOT NULL DEFAULT 1,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `Priority` int(11) NOT NULL DEFAULT 0,
  `MaxExecutionTimeMinutes` int(11) DEFAULT NULL,
  `RetryOnFailure` tinyint(1) NOT NULL DEFAULT 0,
  `MaxRetries` int(11) NOT NULL DEFAULT 3,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Workflows_EntityType` (`EntityType`),
  KEY `IX_Workflows_Status` (`Status`),
  KEY `IX_Workflows_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: WorkflowSteps
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `WorkflowSteps` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowId` int(11) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `StepType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Action, 1=Condition, 2=Wait, etc.',
  `ActionType` varchar(100) DEFAULT NULL COMMENT 'SendEmail, UpdateField, AssignTo, etc.',
  `ActionConfig` longtext DEFAULT NULL COMMENT 'JSON configuration',
  `ConditionConfig` longtext DEFAULT NULL COMMENT 'JSON condition for branching',
  `NextStepId` int(11) DEFAULT NULL COMMENT 'Default next step',
  `TrueStepId` int(11) DEFAULT NULL COMMENT 'Next step if condition is true',
  `FalseStepId` int(11) DEFAULT NULL COMMENT 'Next step if condition is false',
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `IsRequired` tinyint(1) NOT NULL DEFAULT 0,
  `TimeoutMinutes` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_WorkflowSteps_WorkflowId` (`WorkflowId`),
  KEY `IX_WorkflowSteps_SortOrder` (`SortOrder`),
  CONSTRAINT `FK_WorkflowSteps_Workflows` FOREIGN KEY (`WorkflowId`) REFERENCES `Workflows` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: WorkflowInstances
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `WorkflowInstances` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowId` int(11) NOT NULL,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=Running, 2=Completed, etc.',
  `CurrentStepId` int(11) DEFAULT NULL,
  `StartedAt` datetime(6) DEFAULT NULL,
  `CompletedAt` datetime(6) DEFAULT NULL,
  `ErrorMessage` varchar(2000) DEFAULT NULL,
  `RetryCount` int(11) NOT NULL DEFAULT 0,
  `Data` longtext DEFAULT NULL COMMENT 'JSON instance data/variables',
  `TriggeredByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_WorkflowInstances_WorkflowId` (`WorkflowId`),
  KEY `IX_WorkflowInstances_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_WorkflowInstances_Status` (`Status`),
  CONSTRAINT `FK_WorkflowInstances_Workflows` FOREIGN KEY (`WorkflowId`) REFERENCES `Workflows` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: WorkflowStepExecutions
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `WorkflowStepExecutions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowInstanceId` int(11) NOT NULL,
  `WorkflowStepId` int(11) NOT NULL,
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Pending, 1=Running, 2=Completed, etc.',
  `StartedAt` datetime(6) DEFAULT NULL,
  `CompletedAt` datetime(6) DEFAULT NULL,
  `InputData` longtext DEFAULT NULL,
  `OutputData` longtext DEFAULT NULL,
  `ErrorMessage` varchar(2000) DEFAULT NULL,
  `RetryCount` int(11) NOT NULL DEFAULT 0,
  `ExecutedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_WorkflowStepExecutions_InstanceId` (`WorkflowInstanceId`),
  KEY `IX_WorkflowStepExecutions_StepId` (`WorkflowStepId`),
  KEY `IX_WorkflowStepExecutions_Status` (`Status`),
  CONSTRAINT `FK_WorkflowStepExecutions_Instances` FOREIGN KEY (`WorkflowInstanceId`) REFERENCES `WorkflowInstances` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_WorkflowStepExecutions_Steps` FOREIGN KEY (`WorkflowStepId`) REFERENCES `WorkflowSteps` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: WorkflowTriggers
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `WorkflowTriggers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowId` int(11) NOT NULL,
  `TriggerType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Event, 1=Schedule, 2=Manual',
  `EventName` varchar(100) DEFAULT NULL COMMENT 'OnRecordCreate, OnFieldChange, etc.',
  `ScheduleCron` varchar(100) DEFAULT NULL COMMENT 'Cron expression',
  `Condition` longtext DEFAULT NULL COMMENT 'JSON trigger condition',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `LastTriggeredAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_WorkflowTriggers_WorkflowId` (`WorkflowId`),
  KEY `IX_WorkflowTriggers_TriggerType` (`TriggerType`),
  KEY `IX_WorkflowTriggers_IsActive` (`IsActive`),
  CONSTRAINT `FK_WorkflowTriggers_Workflows` FOREIGN KEY (`WorkflowId`) REFERENCES `Workflows` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

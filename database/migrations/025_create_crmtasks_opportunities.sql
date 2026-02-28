-- ============================================================================
-- Migration 025: Create CrmTasks and Opportunities tables
-- Date: 2026-02-17
-- Description: Creates the CrmTasks table (task queue) and Opportunities table
--              that were missing from the database schema. Also adds shadow FK
--              columns (ProductId, SubscriptionId, MarketingCampaignId) that
--              EF Core generates from navigation properties on related entities.
-- ============================================================================

-- CrmTasks Table (matches CRM.Core.Entities.CrmTask : BaseEntity)

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

CREATE TABLE IF NOT EXISTS `CrmTasks` (
    `Id`                INT NOT NULL AUTO_INCREMENT,
    `Subject`           VARCHAR(500) NOT NULL,
    `Description`       TEXT NULL,
    `TaskType`          INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskType enum: 0=Call,1=Email,2=Meeting,3=FollowUp,4=Demo,5=Proposal,6=Contract,7=Research,8=Other',
    `Status`            INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskStatus enum: 0=NotStarted,1=InProgress,2=Completed,3=Deferred,4=Waiting,5=Cancelled',
    `Priority`          INT NOT NULL DEFAULT 0       COMMENT 'CrmTaskPriority enum: 0=Low,1=Normal,2=High,3=Urgent',
    `DueDate`           DATETIME NULL,
    `StartDate`         DATETIME NULL,
    `CompletedDate`     DATETIME NULL,
    `ReminderDate`      DATETIME NULL,
    `HasReminder`       TINYINT(1) NOT NULL DEFAULT 0,
    `PercentComplete`   INT NOT NULL DEFAULT 0,
    `EstimatedMinutes`  INT NULL,
    `ActualMinutes`     INT NULL,
    `IsRecurring`       TINYINT(1) NOT NULL DEFAULT 0,
    `RecurrencePattern` VARCHAR(500) NULL,
    `RecurrenceEndDate` DATETIME NULL,
    `ParentTaskId`      INT NULL,
    `AccountId`         INT NULL               COMMENT 'FK to Customers table (Account entity)',
    `ContactId`         INT NULL,
    `OpportunityId`     INT NULL,
    `CampaignId`        INT NULL               COMMENT 'FK to MarketingCampaigns',
    `AssignedToUserId`  INT NULL,
    `AssignedToGroupId` INT NULL               COMMENT 'FK to UserGroups for workflow queue',
    `CreatedByUserId`   INT NULL,
    `Tags`              VARCHAR(500) NULL,
    `Category`          VARCHAR(100) NULL,
    `Attachments`       VARCHAR(5000) NULL,
    `CustomFields`      TEXT NULL,
    `IsDeleted`         TINYINT(1) NOT NULL DEFAULT 0,
    `RowVersion`        TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `CreatedAt`         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt`         DATETIME NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_CrmTasks_DueDate` (`DueDate`),
    INDEX `IX_CrmTasks_Status` (`Status`),
    INDEX `IX_CrmTasks_AssignedToUserId` (`AssignedToUserId`),
    INDEX `IX_CrmTasks_AssignedToGroupId` (`AssignedToGroupId`),
    INDEX `IX_CrmTasks_AccountId` (`AccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Opportunities Table (matches CRM.Core.Entities.Opportunity : BaseEntity)
CREATE TABLE IF NOT EXISTS `Opportunities` (
    `Id`                    INT NOT NULL AUTO_INCREMENT,
    `CreatedAt`             DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt`             DATETIME(6) NULL DEFAULT NULL,
    `IsDeleted`             TINYINT(1) NOT NULL DEFAULT 0,
    `RowVersion`            TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `Name`                  VARCHAR(255) NOT NULL,
    `Stage`                 INT NOT NULL DEFAULT 0       COMMENT 'OpportunityStage enum: 0=Discovery,1=Qualification,2=Proposal,3=Negotiation,4=ClosedWon,5=ClosedLost',
    `Probability`           INT NOT NULL DEFAULT 10      COMMENT 'Win probability 0-100%',
    `Amount`                DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `Currency`              VARCHAR(3) NOT NULL DEFAULT 'USD',
    `ExpectedCloseDate`     DATETIME(6) NULL DEFAULT NULL,
    `PricingModel`          INT NOT NULL DEFAULT 0       COMMENT 'OpportunityPricingModel enum: 0=Subscription,1=OneTime,2=UsageBased,3=Hybrid',
    `TermLengthMonths`      INT NOT NULL DEFAULT 12,
    `SolutionNotes`         VARCHAR(4000) NULL DEFAULT NULL,
    `QualificationReason`   INT NULL DEFAULT NULL        COMMENT 'QualificationReason enum: 0=Budget,1=Need,2=Timing,3=Authority,4=Fit',
    `QualificationNotes`    VARCHAR(4000) NULL DEFAULT NULL,
    `Region`                VARCHAR(100) NULL DEFAULT NULL,
    `AccountId`             INT NOT NULL                 COMMENT 'FK to Customers table (Account entity)',
    `PrimaryContactId`      INT NULL DEFAULT NULL,
    `SalesOwnerId`          INT NULL DEFAULT NULL,
    `LeadId`                INT NULL DEFAULT NULL,
    `MarketingCampaignId`   INT NULL DEFAULT NULL        COMMENT 'Shadow FK from MarketingCampaign.Opportunities collection',
    `ProductId`             INT NULL DEFAULT NULL        COMMENT 'Shadow FK from Product.Opportunities collection',
    `SubscriptionId`        INT NULL DEFAULT NULL        COMMENT 'Shadow FK from Subscription.Opportunities collection',
    PRIMARY KEY (`Id`),
    KEY `IX_Opportunities_Stage` (`Stage`),
    KEY `IX_Opportunities_ExpectedCloseDate` (`ExpectedCloseDate`),
    KEY `IX_Opportunities_AccountId` (`AccountId`),
    KEY `IX_Opportunities_SalesOwnerId` (`SalesOwnerId`),
    KEY `IX_Opportunities_LeadId` (`LeadId`),
    KEY `IX_Opportunities_MarketingCampaignId` (`MarketingCampaignId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK constraints (optional, may fail if referenced tables don't exist)
-- CrmTasks FKs
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_Parent`
    FOREIGN KEY (`ParentTaskId`) REFERENCES `CrmTasks`(`Id`);
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_Account`
    FOREIGN KEY (`AccountId`) REFERENCES `Customers`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_AssignedTo`
    FOREIGN KEY (`AssignedToUserId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_AssignedToGroup`
    FOREIGN KEY (`AssignedToGroupId`) REFERENCES `UserGroups`(`Id`) ON DELETE SET NULL;
ALTER TABLE `CrmTasks` ADD CONSTRAINT `FK_CrmTasks_CreatedBy`
    FOREIGN KEY (`CreatedByUserId`) REFERENCES `Users`(`Id`);

-- Opportunities FKs
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Customers_AccountId`
    FOREIGN KEY (`AccountId`) REFERENCES `Customers`(`Id`) ON DELETE CASCADE;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Contacts_PrimaryContactId`
    FOREIGN KEY (`PrimaryContactId`) REFERENCES `Contacts`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Users_SalesOwnerId`
    FOREIGN KEY (`SalesOwnerId`) REFERENCES `Users`(`Id`) ON DELETE SET NULL;
ALTER TABLE `Opportunities` ADD CONSTRAINT `FK_Opportunities_Leads_LeadId`
    FOREIGN KEY (`LeadId`) REFERENCES `Leads`(`Id`) ON DELETE SET NULL;
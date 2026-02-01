-- ============================================================================
-- CRM Solution - Junction Table Improvements Migration
-- Version: 2.0
-- Date: 2026-02-01
-- Description: Creates Tags/EntityTags tables and adds missing indexes, 
--              columns, and constraints to junction tables
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- 0. Tags and EntityTags Tables (Create if not exist)
-- ============================================================================

-- Create Tags table if not exists
CREATE TABLE IF NOT EXISTS `Tags` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Color` varchar(20) DEFAULT NULL COMMENT 'Hex color code for UI display',
  `Description` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Tags_Name` (`Name`),
  KEY `IX_Tags_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Create EntityTags table if not exists
CREATE TABLE IF NOT EXISTS `EntityTags` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL COMMENT 'Account, Contact, Lead, Opportunity, etc.',
  `EntityId` int(11) NOT NULL,
  `TagId` int(11) NOT NULL,
  `TagName` varchar(200) DEFAULT NULL COMMENT 'Denormalized tag name for display',
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `CreatedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityTags_EntityType_EntityId_TagId` (`EntityType`, `EntityId`, `TagId`),
  KEY `IX_EntityTags_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_EntityTags_TagId` (`TagId`),
  CONSTRAINT `FK_EntityTags_Tags` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 1. EntitySocialMediaLink Improvements - Add ValidFrom, ValidTo, DoNotContact
-- ============================================================================

-- Add ValidFrom column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntitySocialMediaLinks' AND COLUMN_NAME = 'ValidFrom');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE EntitySocialMediaLinks ADD COLUMN ValidFrom datetime(6) NULL', 
    'SELECT ''ValidFrom column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add ValidTo column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntitySocialMediaLinks' AND COLUMN_NAME = 'ValidTo');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE EntitySocialMediaLinks ADD COLUMN ValidTo datetime(6) NULL', 
    'SELECT ''ValidTo column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add DoNotContact column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntitySocialMediaLinks' AND COLUMN_NAME = 'DoNotContact');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE EntitySocialMediaLinks ADD COLUMN DoNotContact tinyint(1) NOT NULL DEFAULT 0', 
    'SELECT ''DoNotContact column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 2. Tags Table Improvements - Add Color, Description, unique index on Name
-- ============================================================================

-- Add Color column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tags' AND COLUMN_NAME = 'Color');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE Tags ADD COLUMN Color varchar(20) NULL', 
    'SELECT ''Color column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add Description column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tags' AND COLUMN_NAME = 'Description');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE Tags ADD COLUMN Description varchar(500) NULL', 
    'SELECT ''Description column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add unique index on Name if not exists
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tags' AND INDEX_NAME = 'IX_Tags_Name');
SET @query := IF(@idx_exists = 0, 
    'CREATE UNIQUE INDEX IX_Tags_Name ON Tags(Name)', 
    'SELECT ''IX_Tags_Name index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index on IsDeleted if not exists
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tags' AND INDEX_NAME = 'IX_Tags_IsDeleted');
SET @query := IF(@idx_exists = 0, 
    'CREATE INDEX IX_Tags_IsDeleted ON Tags(IsDeleted)', 
    'SELECT ''IX_Tags_IsDeleted index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 3. EntityTags Table Improvements
-- ============================================================================

-- Rename Tag column to TagName if exists (for clarity)
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntityTags' AND COLUMN_NAME = 'Tag');
SET @query := IF(@col_exists > 0, 
    'ALTER TABLE EntityTags CHANGE COLUMN Tag TagName varchar(200) NULL', 
    'SELECT ''Tag column already renamed or does not exist'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add SortOrder column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntityTags' AND COLUMN_NAME = 'SortOrder');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE EntityTags ADD COLUMN SortOrder int NOT NULL DEFAULT 0', 
    'SELECT ''SortOrder column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add CreatedBy column if not exists
SET @col_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntityTags' AND COLUMN_NAME = 'CreatedBy');
SET @query := IF(@col_exists = 0, 
    'ALTER TABLE EntityTags ADD COLUMN CreatedBy int NULL', 
    'SELECT ''CreatedBy column already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add unique composite index on EntityType, EntityId, TagId if not exists
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EntityTags' AND INDEX_NAME = 'IX_EntityTags_EntityType_EntityId_TagId');
SET @query := IF(@idx_exists = 0, 
    'CREATE UNIQUE INDEX IX_EntityTags_EntityType_EntityId_TagId ON EntityTags(EntityType, EntityId, TagId)', 
    'SELECT ''IX_EntityTags_EntityType_EntityId_TagId index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 4. UserGroupMembers Table Improvements - Add unique constraint
-- ============================================================================

-- Add unique composite index on UserId, UserGroupId if not exists
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserGroupMembers' AND INDEX_NAME = 'IX_UserGroupMembers_UserId_UserGroupId');
SET @query := IF(@idx_exists = 0, 
    'CREATE UNIQUE INDEX IX_UserGroupMembers_UserId_UserGroupId ON UserGroupMembers(UserId, UserGroupId)', 
    'SELECT ''IX_UserGroupMembers_UserId_UserGroupId index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 5. OpportunityProducts - Ensure composite primary key exists
-- ============================================================================

-- Add index on CreatedAt for audit queries
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'OpportunityProducts' AND INDEX_NAME = 'IX_OpportunityProducts_CreatedAt');
SET @query := IF(@idx_exists = 0, 
    'CREATE INDEX IX_OpportunityProducts_CreatedAt ON OpportunityProducts(CreatedAt)', 
    'SELECT ''IX_OpportunityProducts_CreatedAt index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 6. LeadProductInterests - Ensure composite primary key exists
-- ============================================================================

-- Add index on CreatedAt for audit queries
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LeadProductInterests' AND INDEX_NAME = 'IX_LeadProductInterests_CreatedAt');
SET @query := IF(@idx_exists = 0, 
    'CREATE INDEX IX_LeadProductInterests_CreatedAt ON LeadProductInterests(CreatedAt)', 
    'SELECT ''IX_LeadProductInterests_CreatedAt index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 7. AccountContacts - Ensure proper indexes
-- ============================================================================

-- Add index on Role for filtering
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AccountContacts' AND INDEX_NAME = 'IX_AccountContacts_Role');
SET @query := IF(@idx_exists = 0, 
    'CREATE INDEX IX_AccountContacts_Role ON AccountContacts(Role)', 
    'SELECT ''IX_AccountContacts_Role index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index on IsPrimaryContact for quick primary contact lookup
SET @idx_exists := (SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AccountContacts' AND INDEX_NAME = 'IX_AccountContacts_IsPrimaryContact');
SET @query := IF(@idx_exists = 0, 
    'CREATE INDEX IX_AccountContacts_IsPrimaryContact ON AccountContacts(AccountId, IsPrimaryContact)', 
    'SELECT ''IX_AccountContacts_IsPrimaryContact index already exists'' AS message');
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================================
-- Migration Complete
-- ============================================================================

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- 8. CustomFields Table (Create if not exist)
-- ============================================================================

CREATE TABLE IF NOT EXISTS `CustomFields` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL COMMENT 'Account, Contact, Lead, Opportunity, etc.',
  `EntityId` int(11) NOT NULL,
  `Key` varchar(200) NOT NULL COMMENT 'Field name/key',
  `Value` text DEFAULT NULL COMMENT 'Field value',
  `DataType` varchar(50) DEFAULT 'string' COMMENT 'string, number, date, boolean',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_CustomFields_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_CustomFields_Key` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 9. LLMProviderSettings Table (Create if not exist)
-- ============================================================================

CREATE TABLE IF NOT EXISTS `llm_provider_settings` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `provider_name` varchar(50) NOT NULL COMMENT 'openai, anthropic, local, etc.',
  `model_name` varchar(100) DEFAULT NULL,
  `api_key` varchar(500) DEFAULT NULL COMMENT 'Encrypted API key',
  `api_base_url` varchar(500) DEFAULT NULL,
  `max_tokens` int(11) DEFAULT 2000,
  `temperature` decimal(3,2) DEFAULT 0.70,
  `is_enabled` tinyint(1) NOT NULL DEFAULT 1,
  `is_default` tinyint(1) NOT NULL DEFAULT 0,
  `priority` int(11) NOT NULL DEFAULT 0,
  `fallback_order` text DEFAULT NULL COMMENT 'JSON array of provider names',
  `effective_fallback_order` text DEFAULT NULL COMMENT 'Computed fallback order',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `updated_at` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `is_deleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_LLMProviderSettings_ProviderName` (`provider_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 10. SystemSettings Table (Create if not exist)
-- ============================================================================

CREATE TABLE IF NOT EXISTS `SystemSettings` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CompanyName` varchar(255) DEFAULT NULL,
  `CompanyLogoUrl` varchar(1000) DEFAULT NULL,
  `PrimaryColor` varchar(20) DEFAULT '#6750A4',
  `SecondaryColor` varchar(20) DEFAULT '#958DA5',
  `DateFormat` varchar(50) DEFAULT 'MM/DD/YYYY',
  `TimeFormat` varchar(50) DEFAULT 'h:mm A',
  `DefaultCurrency` varchar(10) DEFAULT 'USD',
  `DefaultTimezone` varchar(100) DEFAULT 'UTC',
  `DefaultLanguage` varchar(10) DEFAULT 'en',
  `IsMultiCurrency` tinyint(1) NOT NULL DEFAULT 0,
  `FiscalYearStart` int(11) DEFAULT 1 COMMENT 'Month 1-12',
  `EnableTwoFactor` tinyint(1) NOT NULL DEFAULT 0,
  `RequireTwoFactor` tinyint(1) NOT NULL DEFAULT 0,
  `PasswordMinLength` int(11) DEFAULT 8,
  `PasswordRequireSpecial` tinyint(1) NOT NULL DEFAULT 1,
  `PasswordExpiryDays` int(11) DEFAULT NULL,
  `SessionTimeoutMinutes` int(11) DEFAULT 60,
  `AllowUserRegistration` tinyint(1) NOT NULL DEFAULT 0,
  `RequireEmailVerification` tinyint(1) NOT NULL DEFAULT 1,
  `RequireAdminApproval` tinyint(1) NOT NULL DEFAULT 0,
  `SmtpHost` varchar(255) DEFAULT NULL,
  `SmtpPort` int(11) DEFAULT 587,
  `SmtpUser` varchar(255) DEFAULT NULL,
  `SmtpPassword` varchar(500) DEFAULT NULL,
  `SmtpFromEmail` varchar(255) DEFAULT NULL,
  `SmtpFromName` varchar(255) DEFAULT NULL,
  `SmtpUseSsl` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SELECT 'Junction table improvements migration completed successfully' AS status;

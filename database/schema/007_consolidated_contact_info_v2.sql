-- Consolidated Contact Information Migration v2
-- Adapted for existing schema
-- Run on: crm_db and crm_demodb

-- ============================================================
-- 1. Enhance existing Addresses table with new columns
-- ============================================================

ALTER TABLE `Addresses` 
ADD COLUMN IF NOT EXISTS `Line3` VARCHAR(500) NULL AFTER `Line2`,
ADD COLUMN IF NOT EXISTS `County` VARCHAR(100) NULL AFTER `PostalCode`,
ADD COLUMN IF NOT EXISTS `CountryCode` VARCHAR(3) NULL DEFAULT 'US' AFTER `Country`,
ADD COLUMN IF NOT EXISTS `Latitude` DECIMAL(10,7) NULL,
ADD COLUMN IF NOT EXISTS `Longitude` DECIMAL(10,7) NULL,
ADD COLUMN IF NOT EXISTS `GeocodeAccuracy` VARCHAR(50) NULL,
ADD COLUMN IF NOT EXISTS `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS `VerifiedDate` DATETIME(6) NULL,
ADD COLUMN IF NOT EXISTS `VerificationSource` VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS `IsResidential` TINYINT(1) NULL,
ADD COLUMN IF NOT EXISTS `DeliveryInstructions` TEXT NULL,
ADD COLUMN IF NOT EXISTS `AccessHours` VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS `SiteContactName` VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS `SiteContactPhone` VARCHAR(50) NULL,
ADD COLUMN IF NOT EXISTS `CreatedBy` INT NULL,
ADD COLUMN IF NOT EXISTS `UpdatedBy` INT NULL;

-- ============================================================
-- 2. Create PhoneNumbers table
-- ============================================================

CREATE TABLE IF NOT EXISTS `PhoneNumbers` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Label` VARCHAR(100) NULL,
    `CountryCode` VARCHAR(10) NOT NULL DEFAULT '+1',
    `AreaCode` VARCHAR(10) NULL,
    `Number` VARCHAR(30) NOT NULL,
    `Extension` VARCHAR(20) NULL,
    `FormattedNumber` VARCHAR(50) NULL,
    `CanSMS` TINYINT(1) NOT NULL DEFAULT 0,
    `CanWhatsApp` TINYINT(1) NOT NULL DEFAULT 0,
    `CanFax` TINYINT(1) NOT NULL DEFAULT 0,
    `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `VerifiedDate` DATETIME(6) NULL,
    `BestTimeToCall` VARCHAR(100) NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_PhoneNumbers_Number` (`Number`),
    INDEX `IX_PhoneNumbers_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 3. Create EmailAddresses table
-- ============================================================

CREATE TABLE IF NOT EXISTS `EmailAddresses` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Label` VARCHAR(100) NULL,
    `Email` VARCHAR(320) NOT NULL,
    `DisplayName` VARCHAR(200) NULL,
    `IsVerified` TINYINT(1) NOT NULL DEFAULT 0,
    `VerifiedDate` DATETIME(6) NULL,
    `BounceCount` INT NOT NULL DEFAULT 0,
    `LastBounceDate` DATETIME(6) NULL,
    `HardBounce` TINYINT(1) NOT NULL DEFAULT 0,
    `LastEmailSent` DATETIME(6) NULL,
    `LastEmailOpened` DATETIME(6) NULL,
    `EmailEngagementScore` INT NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_EmailAddresses_Email` (`Email`),
    INDEX `IX_EmailAddresses_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 4. Create SocialMediaAccounts table
-- ============================================================

CREATE TABLE IF NOT EXISTS `SocialMediaAccounts` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Platform` VARCHAR(50) NOT NULL,
    `PlatformOther` VARCHAR(100) NULL,
    `AccountType` VARCHAR(50) NOT NULL DEFAULT 'Personal',
    `HandleOrUsername` VARCHAR(200) NOT NULL,
    `ProfileUrl` VARCHAR(500) NULL,
    `DisplayName` VARCHAR(200) NULL,
    `FollowerCount` INT NULL,
    `FollowingCount` INT NULL,
    `IsVerifiedAccount` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `LastActivityDate` DATETIME(6) NULL,
    `EngagementLevel` VARCHAR(20) NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_SocialMediaAccounts_Platform` (`Platform`),
    INDEX `IX_SocialMediaAccounts_HandleOrUsername` (`HandleOrUsername`),
    INDEX `IX_SocialMediaAccounts_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 5. Create EntityAddressLinks junction table
-- ============================================================

CREATE TABLE IF NOT EXISTS `EntityAddressLinks` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `AddressId` INT NOT NULL,
    `EntityType` VARCHAR(50) NOT NULL,
    `EntityId` INT NOT NULL,
    `AddressType` VARCHAR(50) NOT NULL DEFAULT 'Primary',
    `IsPrimary` TINYINT(1) NOT NULL DEFAULT 0,
    `ValidFrom` DATETIME(6) NULL,
    `ValidTo` DATETIME(6) NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_EntityAddressLinks_AddressId` (`AddressId`),
    INDEX `IX_EntityAddressLinks_Entity` (`EntityType`, `EntityId`),
    INDEX `IX_EntityAddressLinks_Primary` (`EntityType`, `EntityId`, `IsPrimary`),
    INDEX `IX_EntityAddressLinks_IsDeleted` (`IsDeleted`),
    CONSTRAINT `FK_EntityAddressLinks_Addresses` FOREIGN KEY (`AddressId`) REFERENCES `Addresses` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 6. Create EntityPhoneLinks junction table
-- ============================================================

CREATE TABLE IF NOT EXISTS `EntityPhoneLinks` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `PhoneId` INT NOT NULL,
    `EntityType` VARCHAR(50) NOT NULL,
    `EntityId` INT NOT NULL,
    `PhoneType` VARCHAR(50) NOT NULL DEFAULT 'Office',
    `IsPrimary` TINYINT(1) NOT NULL DEFAULT 0,
    `DoNotCall` TINYINT(1) NOT NULL DEFAULT 0,
    `ValidFrom` DATETIME(6) NULL,
    `ValidTo` DATETIME(6) NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_EntityPhoneLinks_PhoneId` (`PhoneId`),
    INDEX `IX_EntityPhoneLinks_Entity` (`EntityType`, `EntityId`),
    INDEX `IX_EntityPhoneLinks_Primary` (`EntityType`, `EntityId`, `IsPrimary`),
    INDEX `IX_EntityPhoneLinks_IsDeleted` (`IsDeleted`),
    CONSTRAINT `FK_EntityPhoneLinks_PhoneNumbers` FOREIGN KEY (`PhoneId`) REFERENCES `PhoneNumbers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 7. Create EntityEmailLinks junction table
-- ============================================================

CREATE TABLE IF NOT EXISTS `EntityEmailLinks` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `EmailId` INT NOT NULL,
    `EntityType` VARCHAR(50) NOT NULL,
    `EntityId` INT NOT NULL,
    `EmailType` VARCHAR(50) NOT NULL DEFAULT 'Work',
    `IsPrimary` TINYINT(1) NOT NULL DEFAULT 0,
    `DoNotEmail` TINYINT(1) NOT NULL DEFAULT 0,
    `UnsubscribedDate` DATETIME(6) NULL,
    `MarketingOptIn` TINYINT(1) NOT NULL DEFAULT 1,
    `TransactionalOnly` TINYINT(1) NOT NULL DEFAULT 0,
    `ValidFrom` DATETIME(6) NULL,
    `ValidTo` DATETIME(6) NULL,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_EntityEmailLinks_EmailId` (`EmailId`),
    INDEX `IX_EntityEmailLinks_Entity` (`EntityType`, `EntityId`),
    INDEX `IX_EntityEmailLinks_Primary` (`EntityType`, `EntityId`, `IsPrimary`),
    INDEX `IX_EntityEmailLinks_IsDeleted` (`IsDeleted`),
    CONSTRAINT `FK_EntityEmailLinks_EmailAddresses` FOREIGN KEY (`EmailId`) REFERENCES `EmailAddresses` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 8. Create EntitySocialMediaLinks junction table
-- ============================================================

CREATE TABLE IF NOT EXISTS `EntitySocialMediaLinks` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `SocialMediaAccountId` INT NOT NULL,
    `EntityType` VARCHAR(50) NOT NULL,
    `EntityId` INT NOT NULL,
    `IsPrimary` TINYINT(1) NOT NULL DEFAULT 0,
    `PreferredForContact` TINYINT(1) NOT NULL DEFAULT 0,
    `Notes` TEXT NULL,
    `CreatedBy` INT NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedBy` INT NULL,
    `UpdatedAt` DATETIME(6) NULL,
    `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
    INDEX `IX_EntitySocialMediaLinks_SocialMediaAccountId` (`SocialMediaAccountId`),
    INDEX `IX_EntitySocialMediaLinks_Entity` (`EntityType`, `EntityId`),
    INDEX `IX_EntitySocialMediaLinks_Primary` (`EntityType`, `EntityId`, `IsPrimary`),
    INDEX `IX_EntitySocialMediaLinks_IsDeleted` (`IsDeleted`),
    CONSTRAINT `FK_EntitySocialMediaLinks_SocialMediaAccounts` FOREIGN KEY (`SocialMediaAccountId`) REFERENCES `SocialMediaAccounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 9. Migrate existing customer/contact phone numbers
-- ============================================================

-- Migrate Customer phone numbers
INSERT INTO `PhoneNumbers` (`Label`, `CountryCode`, `Number`, `CreatedAt`, `IsDeleted`)
SELECT 'Primary', '+1', c.`Phone`, c.`CreatedAt`, 0
FROM `Customers` c
WHERE c.`Phone` IS NOT NULL AND c.`Phone` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- Link customer phones
INSERT INTO `EntityPhoneLinks` (`PhoneId`, `EntityType`, `EntityId`, `PhoneType`, `IsPrimary`, `CreatedAt`, `IsDeleted`)
SELECT p.`Id`, 'Customer', c.`Id`, 'Office', 1, NOW(6), 0
FROM `Customers` c
INNER JOIN `PhoneNumbers` p ON p.`Number` = c.`Phone`
WHERE c.`Phone` IS NOT NULL AND c.`Phone` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- ============================================================
-- 10. Migrate existing customer/contact emails
-- ============================================================

-- Migrate Customer emails
INSERT INTO `EmailAddresses` (`Label`, `Email`, `CreatedAt`, `IsDeleted`)
SELECT 'Primary', c.`Email`, c.`CreatedAt`, 0
FROM `Customers` c
WHERE c.`Email` IS NOT NULL AND c.`Email` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- Link customer emails
INSERT INTO `EntityEmailLinks` (`EmailId`, `EntityType`, `EntityId`, `EmailType`, `IsPrimary`, `CreatedAt`, `IsDeleted`)
SELECT e.`Id`, 'Customer', c.`Id`, 'Work', 1, NOW(6), 0
FROM `Customers` c
INNER JOIN `EmailAddresses` e ON e.`Email` = c.`Email`
WHERE c.`Email` IS NOT NULL AND c.`Email` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- Migrate Contact emails if Contacts table exists
INSERT INTO `EmailAddresses` (`Label`, `Email`, `CreatedAt`, `IsDeleted`)
SELECT 'Primary', c.`Email`, COALESCE(c.`CreatedAt`, NOW(6)), 0
FROM `Contacts` c
WHERE c.`Email` IS NOT NULL AND c.`Email` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- Link contact emails
INSERT INTO `EntityEmailLinks` (`EmailId`, `EntityType`, `EntityId`, `EmailType`, `IsPrimary`, `CreatedAt`, `IsDeleted`)
SELECT e.`Id`, 'Contact', c.`Id`, 'Work', 1, NOW(6), 0
FROM `Contacts` c
INNER JOIN `EmailAddresses` e ON e.`Email` = c.`Email`
WHERE c.`Email` IS NOT NULL AND c.`Email` != '' AND c.`IsDeleted` = 0
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- ============================================================
-- 11. Insert sample data for demonstration
-- ============================================================

-- Sample Social Media for demo customer (ID 1 if exists)
INSERT INTO `SocialMediaAccounts` (`Platform`, `AccountType`, `HandleOrUsername`, `ProfileUrl`, `DisplayName`, `IsVerifiedAccount`, `CreatedAt`, `IsDeleted`)
VALUES 
    ('LinkedIn', 'Business', 'acme-corp', 'https://linkedin.com/company/acme-corp', 'ACME Corporation', 1, NOW(6), 0),
    ('Twitter', 'Business', '@acmecorp', 'https://twitter.com/acmecorp', 'ACME Corp', 1, NOW(6), 0)
ON DUPLICATE KEY UPDATE `Id` = `Id`;

-- Log migration completion
SELECT 'Migration completed successfully' AS status, 
       (SELECT COUNT(*) FROM `PhoneNumbers`) AS phones_count,
       (SELECT COUNT(*) FROM `EmailAddresses`) AS emails_count,
       (SELECT COUNT(*) FROM `SocialMediaAccounts`) AS social_count,
       (SELECT COUNT(*) FROM `EntityAddressLinks`) AS address_links,
       (SELECT COUNT(*) FROM `EntityPhoneLinks`) AS phone_links,
       (SELECT COUNT(*) FROM `EntityEmailLinks`) AS email_links;

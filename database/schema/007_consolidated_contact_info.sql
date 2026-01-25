-- ============================================================================
-- CRM Solution Database Schema - Consolidated Contact Information Tables
-- Version: 2.0
-- Date: 2026-01-24
-- Description: Comprehensive contact information tables shared between 
--              Customers, Contacts, Leads, and Accounts with full 3NF normalization
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- MASTER TABLES (Shared Contact Information)
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Table: Addresses (Enhanced Master Table)
-- Description: Master table for all physical addresses, shared between entities
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `Addresses_New`;
CREATE TABLE `Addresses_New` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  
  -- Basic Address Fields
  `Label` varchar(100) DEFAULT 'Primary' COMMENT 'Descriptive name (e.g., Main Office, Home)',
  `Line1` varchar(255) NOT NULL COMMENT 'Street address line 1',
  `Line2` varchar(255) DEFAULT NULL COMMENT 'Street address line 2',
  `Line3` varchar(255) DEFAULT NULL COMMENT 'Additional address info',
  `City` varchar(100) NOT NULL COMMENT 'City or locality',
  `State` varchar(100) DEFAULT NULL COMMENT 'State, province, or region',
  `PostalCode` varchar(20) DEFAULT NULL COMMENT 'Postal or ZIP code',
  `County` varchar(100) DEFAULT NULL COMMENT 'County (for US addresses)',
  `CountryCode` char(2) DEFAULT 'US' COMMENT 'ISO 3166-1 alpha-2 country code',
  `Country` varchar(100) NOT NULL DEFAULT 'United States' COMMENT 'Country full name',
  
  -- Geocoding
  `Latitude` decimal(10,8) DEFAULT NULL COMMENT 'GPS latitude',
  `Longitude` decimal(11,8) DEFAULT NULL COMMENT 'GPS longitude',
  `GeocodeAccuracy` varchar(50) DEFAULT NULL COMMENT 'Accuracy level (ROOFTOP, RANGE, etc.)',
  
  -- Verification
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Address verified flag',
  `VerifiedDate` datetime(6) DEFAULT NULL COMMENT 'When address was verified',
  `VerificationSource` varchar(100) DEFAULT NULL COMMENT 'How verified (Google Maps API, etc.)',
  
  -- Additional Info
  `IsResidential` tinyint(1) DEFAULT NULL COMMENT 'Residential vs commercial',
  `DeliveryInstructions` text DEFAULT NULL COMMENT 'Special delivery notes',
  `AccessHours` varchar(200) DEFAULT NULL COMMENT 'When location is accessible',
  `SiteContactName` varchar(200) DEFAULT NULL COMMENT 'Site contact person',
  `SiteContactPhone` varchar(30) DEFAULT NULL COMMENT 'Site contact phone',
  `Notes` text DEFAULT NULL COMMENT 'Additional notes',
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL COMMENT 'FK to Users',
  `UpdatedBy` int(11) DEFAULT NULL COMMENT 'FK to Users',
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  KEY `IX_Addresses_PostalCode` (`PostalCode`),
  KEY `IX_Addresses_City_State` (`City`, `State`),
  KEY `IX_Addresses_CountryCode` (`CountryCode`),
  KEY `IX_Addresses_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: PhoneNumbers (New Master Table - replaces ContactDetail for phones)
-- Description: Master table for all phone numbers, shared between entities
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `PhoneNumbers`;
CREATE TABLE `PhoneNumbers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  
  -- Phone Details
  `Label` varchar(100) DEFAULT NULL COMMENT 'Descriptive label (Main Office Line)',
  `CountryCode` varchar(5) NOT NULL DEFAULT '+1' COMMENT 'International dialing code',
  `AreaCode` varchar(10) DEFAULT NULL COMMENT 'Area or city code',
  `Number` varchar(30) NOT NULL COMMENT 'Phone number',
  `Extension` varchar(10) DEFAULT NULL COMMENT 'Phone extension',
  `FormattedNumber` varchar(50) DEFAULT NULL COMMENT 'Display formatted number',
  
  -- Capabilities
  `CanSMS` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Supports SMS',
  `CanWhatsApp` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'WhatsApp enabled',
  `CanFax` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Fax capable',
  
  -- Verification
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Number verified',
  `VerifiedDate` datetime(6) DEFAULT NULL COMMENT 'When verified',
  
  -- Preferences
  `BestTimeToCall` varchar(100) DEFAULT NULL COMMENT 'Preferred calling hours',
  `Notes` text DEFAULT NULL COMMENT 'Phone notes',
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  KEY `IX_PhoneNumbers_Number` (`Number`),
  KEY `IX_PhoneNumbers_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EmailAddresses (New Master Table - replaces ContactDetail for emails)
-- Description: Master table for all email addresses, shared between entities
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `EmailAddresses`;
CREATE TABLE `EmailAddresses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  
  -- Email Details
  `Label` varchar(100) DEFAULT NULL COMMENT 'Descriptive label (Accounts Payable)',
  `Email` varchar(255) NOT NULL COMMENT 'Email address',
  `DisplayName` varchar(200) DEFAULT NULL COMMENT 'Display name for email',
  
  -- Verification
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Email verified',
  `VerifiedDate` datetime(6) DEFAULT NULL COMMENT 'When verified',
  
  -- Deliverability
  `BounceCount` int(11) NOT NULL DEFAULT 0 COMMENT 'Number of bounces',
  `LastBounceDate` datetime(6) DEFAULT NULL COMMENT 'Last bounce occurred',
  `HardBounce` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Permanent delivery failure',
  
  -- Engagement Tracking
  `LastEmailSent` datetime(6) DEFAULT NULL COMMENT 'Last email sent date',
  `LastEmailOpened` datetime(6) DEFAULT NULL COMMENT 'Last email opened',
  `EmailEngagementScore` decimal(3,2) DEFAULT NULL COMMENT 'Engagement rating 0-1',
  
  -- Notes
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EmailAddresses_Email` (`Email`),
  KEY `IX_EmailAddresses_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: SocialMediaAccounts (Enhanced Master Table)
-- Description: Master table for all social media accounts, shared between entities
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `SocialMediaAccounts`;
CREATE TABLE `SocialMediaAccounts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  
  -- Platform Details
  `Platform` varchar(50) NOT NULL COMMENT 'Social platform name',
  `PlatformOther` varchar(100) DEFAULT NULL COMMENT 'Other platform name if Platform=Other',
  `AccountType` varchar(50) DEFAULT 'Personal' COMMENT 'Company Page, Personal Profile, etc.',
  `HandleOrUsername` varchar(100) NOT NULL COMMENT 'Social media handle',
  `ProfileUrl` varchar(500) DEFAULT NULL COMMENT 'Full profile URL',
  `DisplayName` varchar(200) DEFAULT NULL COMMENT 'Display name on platform',
  
  -- Metrics
  `FollowerCount` int(11) DEFAULT NULL COMMENT 'Number of followers',
  `FollowingCount` int(11) DEFAULT NULL COMMENT 'Number following',
  
  -- Status
  `IsVerifiedAccount` tinyint(1) DEFAULT 0 COMMENT 'Platform verification badge',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Account actively used',
  `LastActivityDate` date DEFAULT NULL COMMENT 'Last post or activity',
  `EngagementLevel` varchar(20) DEFAULT NULL COMMENT 'High, Medium, Low, Inactive',
  
  -- Notes
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  KEY `IX_SocialMediaAccounts_Platform` (`Platform`),
  KEY `IX_SocialMediaAccounts_HandleOrUsername` (`HandleOrUsername`),
  KEY `IX_SocialMediaAccounts_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- JUNCTION TABLES (Link Contact Info to Entities)
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Table: EntityAddressLinks
-- Description: Links addresses to customers, contacts, leads, accounts
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `EntityAddressLinks`;
CREATE TABLE `EntityAddressLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AddressId` int(11) NOT NULL COMMENT 'FK to Addresses',
  
  -- Polymorphic Link
  `EntityType` varchar(50) NOT NULL COMMENT 'Customer, Contact, Lead, Account',
  `EntityId` int(11) NOT NULL COMMENT 'ID of the linked entity',
  
  -- Link Properties
  `AddressType` varchar(50) NOT NULL DEFAULT 'Primary' COMMENT 'Billing, Shipping, Physical, Mailing, Work, Home, etc.',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Primary address for this type',
  `ValidFrom` date DEFAULT NULL COMMENT 'Address effective start date',
  `ValidTo` date DEFAULT NULL COMMENT 'Address effective end date',
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityAddressLinks_Unique` (`EntityType`, `EntityId`, `AddressId`, `AddressType`),
  KEY `IX_EntityAddressLinks_AddressId` (`AddressId`),
  KEY `IX_EntityAddressLinks_Entity` (`EntityType`, `EntityId`),
  KEY `IX_EntityAddressLinks_IsPrimary` (`IsPrimary`),
  CONSTRAINT `FK_EntityAddressLinks_Addresses` FOREIGN KEY (`AddressId`) REFERENCES `Addresses_New` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EntityPhoneLinks
-- Description: Links phone numbers to customers, contacts, leads, accounts
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `EntityPhoneLinks`;
CREATE TABLE `EntityPhoneLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PhoneId` int(11) NOT NULL COMMENT 'FK to PhoneNumbers',
  
  -- Polymorphic Link
  `EntityType` varchar(50) NOT NULL COMMENT 'Customer, Contact, Lead, Account',
  `EntityId` int(11) NOT NULL COMMENT 'ID of the linked entity',
  
  -- Link Properties
  `PhoneType` varchar(50) NOT NULL DEFAULT 'Office' COMMENT 'Office, Mobile, Fax, Home, Direct, Toll-Free, Emergency, Other',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Primary phone for entity',
  `DoNotCall` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Do not call flag',
  `ValidFrom` date DEFAULT NULL,
  `ValidTo` date DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityPhoneLinks_Unique` (`EntityType`, `EntityId`, `PhoneId`, `PhoneType`),
  KEY `IX_EntityPhoneLinks_PhoneId` (`PhoneId`),
  KEY `IX_EntityPhoneLinks_Entity` (`EntityType`, `EntityId`),
  KEY `IX_EntityPhoneLinks_IsPrimary` (`IsPrimary`),
  CONSTRAINT `FK_EntityPhoneLinks_PhoneNumbers` FOREIGN KEY (`PhoneId`) REFERENCES `PhoneNumbers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EntityEmailLinks
-- Description: Links email addresses to customers, contacts, leads, accounts
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `EntityEmailLinks`;
CREATE TABLE `EntityEmailLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EmailId` int(11) NOT NULL COMMENT 'FK to EmailAddresses',
  
  -- Polymorphic Link
  `EntityType` varchar(50) NOT NULL COMMENT 'Customer, Contact, Lead, Account',
  `EntityId` int(11) NOT NULL COMMENT 'ID of the linked entity',
  
  -- Link Properties
  `EmailType` varchar(50) NOT NULL DEFAULT 'General' COMMENT 'General, Billing, Support, Orders, Marketing, Technical, Executive, Work, Personal, Other',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Primary email for entity',
  `DoNotEmail` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Email opt-out',
  `UnsubscribedDate` datetime(6) DEFAULT NULL COMMENT 'When unsubscribed',
  `MarketingOptIn` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Marketing email consent',
  `TransactionalOnly` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Only transactional emails',
  `ValidFrom` date DEFAULT NULL,
  `ValidTo` date DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityEmailLinks_Unique` (`EntityType`, `EntityId`, `EmailId`, `EmailType`),
  KEY `IX_EntityEmailLinks_EmailId` (`EmailId`),
  KEY `IX_EntityEmailLinks_Entity` (`EntityType`, `EntityId`),
  KEY `IX_EntityEmailLinks_IsPrimary` (`IsPrimary`),
  CONSTRAINT `FK_EntityEmailLinks_EmailAddresses` FOREIGN KEY (`EmailId`) REFERENCES `EmailAddresses` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: EntitySocialMediaLinks
-- Description: Links social media accounts to customers, contacts, leads, accounts
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS `EntitySocialMediaLinks`;
CREATE TABLE `EntitySocialMediaLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SocialMediaAccountId` int(11) NOT NULL COMMENT 'FK to SocialMediaAccounts',
  
  -- Polymorphic Link
  `EntityType` varchar(50) NOT NULL COMMENT 'Customer, Contact, Lead, Account',
  `EntityId` int(11) NOT NULL COMMENT 'ID of the linked entity',
  
  -- Link Properties
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Primary account for platform per entity',
  `PreferredForContact` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Use for customer contact',
  `Notes` text DEFAULT NULL,
  
  -- Audit Fields
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntitySocialMediaLinks_Unique` (`EntityType`, `EntityId`, `SocialMediaAccountId`),
  KEY `IX_EntitySocialMediaLinks_SocialMediaAccountId` (`SocialMediaAccountId`),
  KEY `IX_EntitySocialMediaLinks_Entity` (`EntityType`, `EntityId`),
  KEY `IX_EntitySocialMediaLinks_IsPrimary` (`IsPrimary`),
  CONSTRAINT `FK_EntitySocialMediaLinks_SocialMediaAccounts` FOREIGN KEY (`SocialMediaAccountId`) REFERENCES `SocialMediaAccounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- DATA MIGRATION FROM EXISTING TABLES
-- ============================================================================

-- Migrate existing Addresses to new structure
INSERT INTO `Addresses_New` (`Id`, `Label`, `Line1`, `Line2`, `City`, `State`, `PostalCode`, `Country`, `IsVerified`, `Notes`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
SELECT 
    `Id`,
    COALESCE(NULLIF(`AddressType`, ''), 'Primary') as `Label`,
    COALESCE(`Street`, '') as `Line1`,
    `Street2` as `Line2`,
    COALESCE(`City`, '') as `City`,
    `State`,
    `PostalCode`,
    COALESCE(`Country`, 'United States') as `Country`,
    0 as `IsVerified`,
    NULL as `Notes`,
    `CreatedAt`,
    `UpdatedAt`,
    `IsDeleted`
FROM `Addresses`
WHERE `IsDeleted` = 0;

-- Create EntityAddressLinks from existing Addresses (using EntityType/EntityId columns)
INSERT INTO `EntityAddressLinks` (`AddressId`, `EntityType`, `EntityId`, `AddressType`, `IsPrimary`, `CreatedAt`, `IsDeleted`)
SELECT 
    a.`Id` as `AddressId`,
    a.`EntityType`,
    a.`EntityId`,
    COALESCE(a.`AddressType`, 'Primary') as `AddressType`,
    a.`IsPrimary`,
    a.`CreatedAt`,
    0
FROM `Addresses` a
INNER JOIN `Addresses_New` an ON a.`Id` = an.`Id`
WHERE a.`IsDeleted` = 0 AND a.`EntityType` IS NOT NULL;

-- Migrate ContactDetails (Email type) to EmailAddresses
INSERT INTO `EmailAddresses` (`Label`, `Email`, `IsVerified`, `Notes`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
SELECT 
    COALESCE(`Label`, 'Primary'),
    `Value`,
    0,
    `Notes`,
    `CreatedAt`,
    `UpdatedAt`,
    `IsDeleted`
FROM `ContactDetails`
WHERE `DetailType` = 0 AND `IsDeleted` = 0 AND `Value` IS NOT NULL AND `Value` != ''
ON DUPLICATE KEY UPDATE `Label` = VALUES(`Label`);

-- Migrate ContactDetails (Phone/Fax types) to PhoneNumbers
INSERT INTO `PhoneNumbers` (`Label`, `CountryCode`, `Number`, `CanFax`, `Notes`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
SELECT 
    COALESCE(`Label`, 'Primary'),
    '+1',
    `Value`,
    CASE WHEN `DetailType` = 2 THEN 1 ELSE 0 END,
    `Notes`,
    `CreatedAt`,
    `UpdatedAt`,
    `IsDeleted`
FROM `ContactDetails`
WHERE `DetailType` IN (1, 2) AND `IsDeleted` = 0 AND `Value` IS NOT NULL AND `Value` != '';

-- Migrate SocialAccounts to SocialMediaAccounts
INSERT INTO `SocialMediaAccounts` (`Platform`, `HandleOrUsername`, `ProfileUrl`, `Notes`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
SELECT 
    CASE 
        WHEN `Network` = 1 THEN 'LinkedIn'
        WHEN `Network` = 2 THEN 'Twitter'
        WHEN `Network` = 3 THEN 'Facebook'
        WHEN `Network` = 4 THEN 'Instagram'
        WHEN `Network` = 5 THEN 'YouTube'
        ELSE 'Other'
    END as `Platform`,
    COALESCE(`HandleOrUrl`, '') as `HandleOrUsername`,
    CASE 
        WHEN `HandleOrUrl` LIKE 'http%' THEN `HandleOrUrl`
        ELSE NULL 
    END as `ProfileUrl`,
    `Notes`,
    `CreatedAt`,
    `UpdatedAt`,
    `IsDeleted`
FROM `SocialAccounts`
WHERE `IsDeleted` = 0 AND `HandleOrUrl` IS NOT NULL AND `HandleOrUrl` != '';

-- ============================================================================
-- VIEWS FOR BACKWARD COMPATIBILITY
-- ============================================================================

-- View: CustomerAddresses
CREATE OR REPLACE VIEW `vw_CustomerAddresses` AS
SELECT 
    eal.`Id` as `LinkId`,
    a.`Id` as `AddressId`,
    eal.`EntityId` as `CustomerId`,
    eal.`AddressType`,
    eal.`IsPrimary`,
    a.`Label`,
    a.`Line1`,
    a.`Line2`,
    a.`Line3`,
    a.`City`,
    a.`State`,
    a.`PostalCode`,
    a.`County`,
    a.`CountryCode`,
    a.`Country`,
    a.`Latitude`,
    a.`Longitude`,
    a.`IsVerified`,
    a.`IsResidential`,
    a.`DeliveryInstructions`,
    eal.`ValidFrom`,
    eal.`ValidTo`,
    a.`CreatedAt`,
    a.`UpdatedAt`
FROM `EntityAddressLinks` eal
INNER JOIN `Addresses_New` a ON eal.`AddressId` = a.`Id`
WHERE eal.`EntityType` = 'Customer' AND eal.`IsDeleted` = 0 AND a.`IsDeleted` = 0;

-- View: ContactAddresses
CREATE OR REPLACE VIEW `vw_ContactAddresses` AS
SELECT 
    eal.`Id` as `LinkId`,
    a.`Id` as `AddressId`,
    eal.`EntityId` as `ContactId`,
    eal.`AddressType`,
    eal.`IsPrimary`,
    a.`Label`,
    a.`Line1`,
    a.`Line2`,
    a.`Line3`,
    a.`City`,
    a.`State`,
    a.`PostalCode`,
    a.`County`,
    a.`CountryCode`,
    a.`Country`,
    a.`Latitude`,
    a.`Longitude`,
    a.`IsVerified`,
    eal.`ValidFrom`,
    eal.`ValidTo`,
    a.`CreatedAt`,
    a.`UpdatedAt`
FROM `EntityAddressLinks` eal
INNER JOIN `Addresses_New` a ON eal.`AddressId` = a.`Id`
WHERE eal.`EntityType` = 'Contact' AND eal.`IsDeleted` = 0 AND a.`IsDeleted` = 0;

-- View: EntityPhoneNumbers (unified view for all entities)
CREATE OR REPLACE VIEW `vw_EntityPhoneNumbers` AS
SELECT 
    epl.`Id` as `LinkId`,
    p.`Id` as `PhoneId`,
    epl.`EntityType`,
    epl.`EntityId`,
    epl.`PhoneType`,
    epl.`IsPrimary`,
    epl.`DoNotCall`,
    p.`Label`,
    p.`CountryCode`,
    p.`AreaCode`,
    p.`Number`,
    p.`Extension`,
    p.`FormattedNumber`,
    p.`CanSMS`,
    p.`CanWhatsApp`,
    p.`CanFax`,
    p.`IsVerified`,
    p.`BestTimeToCall`,
    p.`CreatedAt`,
    p.`UpdatedAt`
FROM `EntityPhoneLinks` epl
INNER JOIN `PhoneNumbers` p ON epl.`PhoneId` = p.`Id`
WHERE epl.`IsDeleted` = 0 AND p.`IsDeleted` = 0;

-- View: EntityEmailAddresses (unified view for all entities)
CREATE OR REPLACE VIEW `vw_EntityEmailAddresses` AS
SELECT 
    eel.`Id` as `LinkId`,
    e.`Id` as `EmailId`,
    eel.`EntityType`,
    eel.`EntityId`,
    eel.`EmailType`,
    eel.`IsPrimary`,
    eel.`DoNotEmail`,
    eel.`MarketingOptIn`,
    eel.`TransactionalOnly`,
    e.`Label`,
    e.`Email`,
    e.`DisplayName`,
    e.`IsVerified`,
    e.`BounceCount`,
    e.`HardBounce`,
    e.`LastEmailSent`,
    e.`LastEmailOpened`,
    e.`EmailEngagementScore`,
    e.`CreatedAt`,
    e.`UpdatedAt`
FROM `EntityEmailLinks` eel
INNER JOIN `EmailAddresses` e ON eel.`EmailId` = e.`Id`
WHERE eel.`IsDeleted` = 0 AND e.`IsDeleted` = 0;

-- View: EntitySocialMedia (unified view for all entities)
CREATE OR REPLACE VIEW `vw_EntitySocialMedia` AS
SELECT 
    esml.`Id` as `LinkId`,
    s.`Id` as `SocialMediaAccountId`,
    esml.`EntityType`,
    esml.`EntityId`,
    esml.`IsPrimary`,
    esml.`PreferredForContact`,
    s.`Platform`,
    s.`PlatformOther`,
    s.`AccountType`,
    s.`HandleOrUsername`,
    s.`ProfileUrl`,
    s.`DisplayName`,
    s.`FollowerCount`,
    s.`FollowingCount`,
    s.`IsVerifiedAccount`,
    s.`IsActive`,
    s.`LastActivityDate`,
    s.`EngagementLevel`,
    s.`CreatedAt`,
    s.`UpdatedAt`
FROM `EntitySocialMediaLinks` esml
INNER JOIN `SocialMediaAccounts` s ON esml.`SocialMediaAccountId` = s.`Id`
WHERE esml.`IsDeleted` = 0 AND s.`IsDeleted` = 0;

-- ============================================================================
-- STORED PROCEDURES FOR COMMON OPERATIONS
-- ============================================================================

DELIMITER //

-- Procedure: Add or link an address to an entity
CREATE PROCEDURE `sp_LinkAddressToEntity`(
    IN p_EntityType VARCHAR(50),
    IN p_EntityId INT,
    IN p_AddressType VARCHAR(50),
    IN p_IsPrimary TINYINT,
    IN p_Line1 VARCHAR(255),
    IN p_Line2 VARCHAR(255),
    IN p_City VARCHAR(100),
    IN p_State VARCHAR(100),
    IN p_PostalCode VARCHAR(20),
    IN p_Country VARCHAR(100),
    IN p_CreatedBy INT,
    OUT p_AddressId INT,
    OUT p_LinkId INT
)
BEGIN
    -- Check if identical address already exists
    SELECT `Id` INTO p_AddressId
    FROM `Addresses_New`
    WHERE `Line1` = p_Line1 
      AND COALESCE(`Line2`, '') = COALESCE(p_Line2, '')
      AND `City` = p_City 
      AND COALESCE(`State`, '') = COALESCE(p_State, '')
      AND COALESCE(`PostalCode`, '') = COALESCE(p_PostalCode, '')
      AND `Country` = p_Country
      AND `IsDeleted` = 0
    LIMIT 1;
    
    -- If no existing address, create new one
    IF p_AddressId IS NULL THEN
        INSERT INTO `Addresses_New` (`Label`, `Line1`, `Line2`, `City`, `State`, `PostalCode`, `Country`, `CreatedBy`)
        VALUES (p_AddressType, p_Line1, p_Line2, p_City, p_State, p_PostalCode, p_Country, p_CreatedBy);
        SET p_AddressId = LAST_INSERT_ID();
    END IF;
    
    -- If setting as primary, unset other primaries of same type
    IF p_IsPrimary = 1 THEN
        UPDATE `EntityAddressLinks` 
        SET `IsPrimary` = 0 
        WHERE `EntityType` = p_EntityType 
          AND `EntityId` = p_EntityId 
          AND `AddressType` = p_AddressType;
    END IF;
    
    -- Create the link
    INSERT INTO `EntityAddressLinks` (`AddressId`, `EntityType`, `EntityId`, `AddressType`, `IsPrimary`, `CreatedBy`)
    VALUES (p_AddressId, p_EntityType, p_EntityId, p_AddressType, p_IsPrimary, p_CreatedBy)
    ON DUPLICATE KEY UPDATE `IsPrimary` = p_IsPrimary, `IsDeleted` = 0;
    
    SET p_LinkId = LAST_INSERT_ID();
END //

-- Procedure: Share address between entities
CREATE PROCEDURE `sp_ShareAddressBetweenEntities`(
    IN p_AddressId INT,
    IN p_SourceEntityType VARCHAR(50),
    IN p_SourceEntityId INT,
    IN p_TargetEntityType VARCHAR(50),
    IN p_TargetEntityId INT,
    IN p_AddressType VARCHAR(50),
    IN p_IsPrimary TINYINT,
    IN p_CreatedBy INT,
    OUT p_LinkId INT
)
BEGIN
    -- Verify address exists and is linked to source
    IF EXISTS (
        SELECT 1 FROM `EntityAddressLinks` 
        WHERE `AddressId` = p_AddressId 
          AND `EntityType` = p_SourceEntityType 
          AND `EntityId` = p_SourceEntityId
          AND `IsDeleted` = 0
    ) THEN
        -- If setting as primary, unset other primaries
        IF p_IsPrimary = 1 THEN
            UPDATE `EntityAddressLinks` 
            SET `IsPrimary` = 0 
            WHERE `EntityType` = p_TargetEntityType 
              AND `EntityId` = p_TargetEntityId 
              AND `AddressType` = p_AddressType;
        END IF;
        
        -- Create link to target entity
        INSERT INTO `EntityAddressLinks` (`AddressId`, `EntityType`, `EntityId`, `AddressType`, `IsPrimary`, `CreatedBy`)
        VALUES (p_AddressId, p_TargetEntityType, p_TargetEntityId, p_AddressType, p_IsPrimary, p_CreatedBy)
        ON DUPLICATE KEY UPDATE `IsPrimary` = p_IsPrimary, `IsDeleted` = 0;
        
        SET p_LinkId = LAST_INSERT_ID();
    ELSE
        SET p_LinkId = NULL;
    END IF;
END //

-- Procedure: Get all contact info for an entity
CREATE PROCEDURE `sp_GetEntityContactInfo`(
    IN p_EntityType VARCHAR(50),
    IN p_EntityId INT
)
BEGIN
    -- Addresses
    SELECT 'Address' as InfoType, a.*, eal.`AddressType`, eal.`IsPrimary`, eal.`ValidFrom`, eal.`ValidTo`
    FROM `EntityAddressLinks` eal
    INNER JOIN `Addresses_New` a ON eal.`AddressId` = a.`Id`
    WHERE eal.`EntityType` = p_EntityType AND eal.`EntityId` = p_EntityId 
      AND eal.`IsDeleted` = 0 AND a.`IsDeleted` = 0;
    
    -- Phone Numbers
    SELECT 'Phone' as InfoType, p.*, epl.`PhoneType`, epl.`IsPrimary`, epl.`DoNotCall`, epl.`ValidFrom`, epl.`ValidTo`
    FROM `EntityPhoneLinks` epl
    INNER JOIN `PhoneNumbers` p ON epl.`PhoneId` = p.`Id`
    WHERE epl.`EntityType` = p_EntityType AND epl.`EntityId` = p_EntityId 
      AND epl.`IsDeleted` = 0 AND p.`IsDeleted` = 0;
    
    -- Email Addresses
    SELECT 'Email' as InfoType, e.*, eel.`EmailType`, eel.`IsPrimary`, eel.`DoNotEmail`, eel.`MarketingOptIn`, eel.`ValidFrom`, eel.`ValidTo`
    FROM `EntityEmailLinks` eel
    INNER JOIN `EmailAddresses` e ON eel.`EmailId` = e.`Id`
    WHERE eel.`EntityType` = p_EntityType AND eel.`EntityId` = p_EntityId 
      AND eel.`IsDeleted` = 0 AND e.`IsDeleted` = 0;
    
    -- Social Media Accounts
    SELECT 'SocialMedia' as InfoType, s.*, esml.`IsPrimary`, esml.`PreferredForContact`
    FROM `EntitySocialMediaLinks` esml
    INNER JOIN `SocialMediaAccounts` s ON esml.`SocialMediaAccountId` = s.`Id`
    WHERE esml.`EntityType` = p_EntityType AND esml.`EntityId` = p_EntityId 
      AND esml.`IsDeleted` = 0 AND s.`IsDeleted` = 0;
END //

DELIMITER ;

SET FOREIGN_KEY_CHECKS = 1;

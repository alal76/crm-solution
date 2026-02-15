-- ============================================================================
-- CRM Solution - Complete Baseline Schema
-- Version: 2.0
-- Date: 2026-02-01
-- Description: Consolidated baseline schema for fresh deployments.
--              Run this file ONLY on a new database. For existing databases,
--              use the incremental migration files (001-009).
--
-- Order of table creation follows dependency graph:
--   1. Master/Lookup tables (no dependencies)
--   2. User management tables
--   3. Core entity tables (Accounts, Contacts, Leads, Products)
--   4. Junction tables (relationships between entities)
--   5. Activity and communication tables
--   6. Workflow and automation tables
--   7. Service desk tables
--   8. System configuration tables
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- SECTION 1: MASTER AND LOOKUP TABLES
-- ============================================================================

-- ZipCodes - Master data for postal code lookup
CREATE TABLE IF NOT EXISTS `ZipCodes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Country` varchar(100) NOT NULL,
  `CountryCode` varchar(10) DEFAULT NULL,
  `PostalCode` varchar(20) NOT NULL,
  `City` varchar(200) NOT NULL,
  `State` varchar(200) DEFAULT NULL,
  `StateCode` varchar(10) DEFAULT NULL,
  `County` varchar(200) DEFAULT NULL,
  `CountyCode` varchar(10) DEFAULT NULL,
  `Community` varchar(200) DEFAULT NULL,
  `CommunityCode` varchar(10) DEFAULT NULL,
  `Latitude` decimal(10,7) DEFAULT NULL,
  `Longitude` decimal(10,7) DEFAULT NULL,
  `Accuracy` int(11) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_ZipCodes_PostalCode` (`PostalCode`),
  KEY `IX_ZipCodes_Country_PostalCode` (`Country`, `PostalCode`),
  KEY `IX_ZipCodes_City` (`City`),
  FULLTEXT KEY `FT_ZipCodes_City` (`City`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ColorPalettes - System and user-defined color palettes
CREATE TABLE IF NOT EXISTS `ColorPalettes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Category` varchar(100) DEFAULT NULL,
  `Color1` varchar(10) NOT NULL,
  `Color2` varchar(10) NOT NULL,
  `Color3` varchar(10) NOT NULL,
  `Color4` varchar(10) NOT NULL,
  `Color5` varchar(10) NOT NULL,
  `IsUserDefined` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ColorPalettes_Category` (`Category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- LookupCategories - Categories for lookup/dropdown values
CREATE TABLE IF NOT EXISTS `LookupCategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsSystem` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_LookupCategories_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- LookupItems - Lookup/dropdown values
CREATE TABLE IF NOT EXISTS `LookupItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CategoryId` int(11) NOT NULL,
  `Value` varchar(200) NOT NULL,
  `Code` varchar(50) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ParentItemId` int(11) DEFAULT NULL,
  `DisplayOrder` int(11) NOT NULL DEFAULT 0,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsDefault` tinyint(1) NOT NULL DEFAULT 0,
  `Metadata` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_LookupItems_CategoryId` (`CategoryId`),
  CONSTRAINT `FK_LookupItems_LookupCategories` FOREIGN KEY (`CategoryId`) REFERENCES `LookupCategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Tags - Tag definitions for entity categorization
CREATE TABLE IF NOT EXISTS `Tags` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Color` varchar(20) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Tags_Name` (`Name`),
  KEY `IX_Tags_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 2: USER MANAGEMENT
-- ============================================================================

-- Departments
CREATE TABLE IF NOT EXISTS `Departments` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `DepartmentCode` varchar(20) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ManagerId` int(11) DEFAULT NULL,
  `ParentDepartmentId` int(11) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Departments_ManagerId` (`ManagerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- UserProfiles
CREATE TABLE IF NOT EXISTS `UserProfiles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL DEFAULT 0,
  `Permissions` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- UserGroups
CREATE TABLE IF NOT EXISTS `UserGroups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsDefault` tinyint(1) NOT NULL DEFAULT 0,
  `DisplayOrder` int(11) NOT NULL DEFAULT 0,
  `HeaderColor` varchar(10) DEFAULT '#6750A4',
  `IsSystemAdmin` tinyint(1) NOT NULL DEFAULT 0,
  `AccessibleMenuItems` text DEFAULT '[]',
  `CanAccessDashboard` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessAccounts` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessContacts` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessLeads` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessOpportunities` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessProducts` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessServices` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessCampaigns` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessQuotes` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessTasks` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessActivities` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessNotes` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessWorkflows` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessServiceRequests` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessReports` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessSettings` tinyint(1) NOT NULL DEFAULT 0,
  `CanAccessUserManagement` tinyint(1) NOT NULL DEFAULT 0,
  `CanCreateAccounts` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditAccounts` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteAccounts` tinyint(1) NOT NULL DEFAULT 0,
  `CanViewAllAccounts` tinyint(1) NOT NULL DEFAULT 1,
  `CanCreateContacts` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditContacts` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteContacts` tinyint(1) NOT NULL DEFAULT 0,
  `CanCreateLeads` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditLeads` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteLeads` tinyint(1) NOT NULL DEFAULT 0,
  `CanConvertLeads` tinyint(1) NOT NULL DEFAULT 1,
  `CanCreateOpportunities` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditOpportunities` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteOpportunities` tinyint(1) NOT NULL DEFAULT 0,
  `CanCloseOpportunities` tinyint(1) NOT NULL DEFAULT 1,
  `CanCreateProducts` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditProducts` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteProducts` tinyint(1) NOT NULL DEFAULT 0,
  `CanManagePricing` tinyint(1) NOT NULL DEFAULT 0,
  `CanCreateCampaigns` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditCampaigns` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteCampaigns` tinyint(1) NOT NULL DEFAULT 0,
  `CanLaunchCampaigns` tinyint(1) NOT NULL DEFAULT 0,
  `CanCreateQuotes` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditQuotes` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteQuotes` tinyint(1) NOT NULL DEFAULT 0,
  `CanApproveQuotes` tinyint(1) NOT NULL DEFAULT 0,
  `CanCreateTasks` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditTasks` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteTasks` tinyint(1) NOT NULL DEFAULT 0,
  `CanAssignTasks` tinyint(1) NOT NULL DEFAULT 1,
  `CanCreateWorkflows` tinyint(1) NOT NULL DEFAULT 0,
  `CanEditWorkflows` tinyint(1) NOT NULL DEFAULT 0,
  `CanDeleteWorkflows` tinyint(1) NOT NULL DEFAULT 0,
  `CanActivateWorkflows` tinyint(1) NOT NULL DEFAULT 0,
  `DataAccessScope` varchar(50) NOT NULL DEFAULT 'own',
  `CanExportData` tinyint(1) NOT NULL DEFAULT 0,
  `CanImportData` tinyint(1) NOT NULL DEFAULT 0,
  `CanBulkEdit` tinyint(1) NOT NULL DEFAULT 0,
  `CanBulkDelete` tinyint(1) NOT NULL DEFAULT 0,
  `PasswordExpirationDays` int(11) DEFAULT NULL,
  `PasswordExpirationPolicy` int(11) NOT NULL DEFAULT 0,
  `PasswordExpirationWarningDays` int(11) DEFAULT 7,
  `RequireTwoFactor` tinyint(1) NOT NULL DEFAULT 0,
  `EnforceTwoFactor` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UserGroups_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Users
CREATE TABLE IF NOT EXISTS `Users` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(100) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `PasswordHash` varchar(512) NOT NULL,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Role` varchar(50) NOT NULL DEFAULT 'User',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsEmailVerified` tinyint(1) NOT NULL DEFAULT 0,
  `LastLoginAt` datetime(6) DEFAULT NULL,
  `FailedLoginAttempts` int(11) NOT NULL DEFAULT 0,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `RefreshToken` varchar(512) DEFAULT NULL,
  `RefreshTokenExpiryTime` datetime(6) DEFAULT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL DEFAULT 0,
  `TwoFactorSecret` varchar(255) DEFAULT NULL,
  `BackupCodes` text DEFAULT NULL,
  `PasswordLastChangedAt` datetime(6) DEFAULT NULL,
  `MustResetPassword` tinyint(1) NOT NULL DEFAULT 0,
  `PasswordNeverSet` tinyint(1) NOT NULL DEFAULT 0,
  `PasswordResetToken` varchar(512) DEFAULT NULL,
  `PasswordResetTokenExpiry` datetime(6) DEFAULT NULL,
  `HeaderColor` varchar(10) DEFAULT NULL,
  `PhotoUrl` varchar(500) DEFAULT NULL,
  `ThemePreference` varchar(20) DEFAULT 'system',
  `DepartmentId` int(11) DEFAULT NULL,
  `UserProfileId` int(11) DEFAULT NULL,
  `PrimaryGroupId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Email` (`Email`),
  UNIQUE KEY `IX_Users_Username` (`Username`),
  KEY `IX_Users_DepartmentId` (`DepartmentId`),
  KEY `IX_Users_UserProfileId` (`UserProfileId`),
  KEY `IX_Users_PrimaryGroupId` (`PrimaryGroupId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- UserGroupMembers (Junction Table)
CREATE TABLE IF NOT EXISTS `UserGroupMembers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `GroupId` int(11) NOT NULL,
  `JoinedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UserGroupMembers_UserId_GroupId` (`UserId`, `GroupId`),
  KEY `IX_UserGroupMembers_GroupId` (`GroupId`),
  CONSTRAINT `FK_UserGroupMembers_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_UserGroupMembers_UserGroups` FOREIGN KEY (`GroupId`) REFERENCES `UserGroups` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Teams
CREATE TABLE IF NOT EXISTS `Teams` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `LeaderId` int(11) DEFAULT NULL,
  `DepartmentId` int(11) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Teams_LeaderId` (`LeaderId`),
  KEY `IX_Teams_DepartmentId` (`DepartmentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- TeamMembers (Junction Table)
CREATE TABLE IF NOT EXISTS `TeamMembers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TeamId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `Role` varchar(50) DEFAULT 'Member',
  `JoinedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_TeamMembers_TeamId_UserId` (`TeamId`, `UserId`),
  KEY `IX_TeamMembers_UserId` (`UserId`),
  CONSTRAINT `FK_TeamMembers_Teams` FOREIGN KEY (`TeamId`) REFERENCES `Teams` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_TeamMembers_Users` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 3: CONTACT INFORMATION TABLES
-- ============================================================================

-- Addresses (Master Table)
CREATE TABLE IF NOT EXISTS `Addresses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(50) DEFAULT NULL,
  `EntityId` int(11) DEFAULT NULL,
  `AddressType` varchar(50) NOT NULL DEFAULT 'Primary',
  `Label` varchar(100) DEFAULT 'Primary',
  `Line1` varchar(255) DEFAULT NULL,
  `Line2` varchar(255) DEFAULT NULL,
  `Line3` varchar(500) DEFAULT NULL,
  `Street` varchar(255) DEFAULT NULL,
  `Street2` varchar(255) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `PostalCode` varchar(20) DEFAULT NULL,
  `County` varchar(100) DEFAULT NULL,
  `Country` varchar(100) DEFAULT 'United States',
  `CountryCode` varchar(3) DEFAULT 'US',
  `Latitude` decimal(10,7) DEFAULT NULL,
  `Longitude` decimal(10,7) DEFAULT NULL,
  `GeocodeAccuracy` varchar(50) DEFAULT NULL,
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0,
  `VerifiedDate` datetime(6) DEFAULT NULL,
  `VerificationSource` varchar(100) DEFAULT NULL,
  `IsResidential` tinyint(1) DEFAULT NULL,
  `DeliveryInstructions` text DEFAULT NULL,
  `AccessHours` varchar(200) DEFAULT NULL,
  `SiteContactName` varchar(200) DEFAULT NULL,
  `SiteContactPhone` varchar(50) DEFAULT NULL,
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `Notes` text DEFAULT NULL,
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Addresses_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_Addresses_PostalCode` (`PostalCode`),
  KEY `IX_Addresses_City_State` (`City`, `State`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- PhoneNumbers (Master Table)
CREATE TABLE IF NOT EXISTS `PhoneNumbers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Label` varchar(100) DEFAULT NULL,
  `CountryCode` varchar(10) NOT NULL DEFAULT '+1',
  `AreaCode` varchar(10) DEFAULT NULL,
  `Number` varchar(30) NOT NULL,
  `Extension` varchar(20) DEFAULT NULL,
  `FormattedNumber` varchar(50) DEFAULT NULL,
  `CanSMS` tinyint(1) NOT NULL DEFAULT 0,
  `CanWhatsApp` tinyint(1) NOT NULL DEFAULT 0,
  `CanFax` tinyint(1) NOT NULL DEFAULT 0,
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0,
  `VerifiedDate` datetime(6) DEFAULT NULL,
  `BestTimeToCall` varchar(100) DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_PhoneNumbers_Number` (`Number`),
  KEY `IX_PhoneNumbers_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EmailAddresses (Master Table)
CREATE TABLE IF NOT EXISTS `EmailAddresses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Label` varchar(100) DEFAULT NULL,
  `Email` varchar(320) NOT NULL,
  `DisplayName` varchar(200) DEFAULT NULL,
  `IsVerified` tinyint(1) NOT NULL DEFAULT 0,
  `VerifiedDate` datetime(6) DEFAULT NULL,
  `BounceCount` int(11) NOT NULL DEFAULT 0,
  `LastBounceDate` datetime(6) DEFAULT NULL,
  `HardBounce` tinyint(1) NOT NULL DEFAULT 0,
  `LastEmailSent` datetime(6) DEFAULT NULL,
  `LastEmailOpened` datetime(6) DEFAULT NULL,
  `EmailEngagementScore` int(11) DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_EmailAddresses_Email` (`Email`),
  KEY `IX_EmailAddresses_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- SocialMediaAccounts (Master Table)
CREATE TABLE IF NOT EXISTS `SocialMediaAccounts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Platform` varchar(50) NOT NULL,
  `PlatformOther` varchar(100) DEFAULT NULL,
  `AccountType` varchar(50) NOT NULL DEFAULT 'Personal',
  `HandleOrUsername` varchar(200) NOT NULL,
  `ProfileUrl` varchar(500) DEFAULT NULL,
  `DisplayName` varchar(200) DEFAULT NULL,
  `FollowerCount` int(11) DEFAULT NULL,
  `FollowingCount` int(11) DEFAULT NULL,
  `IsVerifiedAccount` tinyint(1) NOT NULL DEFAULT 0,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `LastActivityDate` datetime(6) DEFAULT NULL,
  `EngagementLevel` varchar(20) DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedBy` int(11) DEFAULT NULL,
  `UpdatedBy` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_SocialMediaAccounts_Platform` (`Platform`),
  KEY `IX_SocialMediaAccounts_HandleOrUsername` (`HandleOrUsername`),
  KEY `IX_SocialMediaAccounts_IsDeleted` (`IsDeleted`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 4: CORE ENTITY TABLES
-- ============================================================================

-- Accounts
CREATE TABLE IF NOT EXISTS `Accounts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Category` int(11) NOT NULL DEFAULT 0 COMMENT '0=Individual, 1=Organization',
  `FirstName` varchar(100) DEFAULT NULL,
  `LastName` varchar(100) DEFAULT NULL,
  `Company` varchar(255) DEFAULT NULL,
  `LegalName` varchar(500) DEFAULT NULL,
  `DbaName` varchar(255) DEFAULT NULL,
  `TaxId` varchar(50) DEFAULT NULL,
  `RegistrationNumber` varchar(100) DEFAULT NULL,
  `Salutation` varchar(20) DEFAULT NULL,
  `Suffix` varchar(20) DEFAULT NULL,
  `Gender` varchar(20) DEFAULT NULL,
  `Name` varchar(200) DEFAULT NULL,
  `Email` varchar(255) NOT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Website` varchar(255) DEFAULT NULL,
  `Industry` varchar(100) DEFAULT NULL,
  `AccountType` int(11) NOT NULL DEFAULT 0,
  `LifecycleStage` int(11) NOT NULL DEFAULT 0,
  `Priority` int(11) NOT NULL DEFAULT 1,
  `Status` varchar(50) DEFAULT 'Active',
  `Rating` varchar(20) DEFAULT NULL,
  `Source` varchar(100) DEFAULT NULL,
  `LeadSource` varchar(100) DEFAULT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `Address` varchar(500) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `ZipCode` varchar(20) DEFAULT NULL,
  `Country` varchar(100) DEFAULT NULL,
  `AnnualRevenue` decimal(18,2) DEFAULT NULL,
  `NumberOfEmployees` int(11) DEFAULT NULL,
  `LogoUrl` varchar(500) DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `ParentAccountId` int(11) DEFAULT NULL,
  `CustomerHealthScore` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Accounts_Email` (`Email`),
  KEY `IX_Accounts_OwnerId` (`OwnerId`),
  KEY `IX_Accounts_AssignedUserId` (`AssignedUserId`),
  KEY `IX_Accounts_Status` (`Status`),
  KEY `IX_Accounts_Category` (`Category`),
  KEY `IX_Accounts_LifecycleStage` (`LifecycleStage`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Contacts
CREATE TABLE IF NOT EXISTS `Contacts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `EmailPrimary` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `PhonePrimary` varchar(50) DEFAULT NULL,
  `Mobile` varchar(50) DEFAULT NULL,
  `Fax` varchar(50) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `JobTitle` varchar(100) DEFAULT NULL,
  `Department` varchar(100) DEFAULT NULL,
  `Type` int(11) NOT NULL DEFAULT 0,
  `Status` int(11) NOT NULL DEFAULT 0,
  `DoNotCall` tinyint(1) NOT NULL DEFAULT 0,
  `DoNotEmail` tinyint(1) NOT NULL DEFAULT 0,
  `Description` varchar(2000) DEFAULT NULL,
  `PhotoUrl` varchar(500) DEFAULT NULL,
  `LeadSource` varchar(100) DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `CampaignId` int(11) DEFAULT NULL,
  `ReportsToContactId` int(11) DEFAULT NULL,
  `PreferredContactMethodLookupId` int(11) DEFAULT NULL,
  `LastActivityDate` datetime(6) DEFAULT NULL,
  `LastContactedDate` datetime(6) DEFAULT NULL,
  `NextFollowUpDate` datetime(6) DEFAULT NULL,
  `TotalInteractions` int(11) DEFAULT NULL,
  `EmailsReceived` int(11) DEFAULT NULL,
  `EmailsOpened` int(11) DEFAULT NULL,
  `LinksClicked` int(11) DEFAULT NULL,
  `DateAdded` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `LastModified` datetime(6) DEFAULT NULL,
  `ModifiedBy` varchar(200) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CustomFields` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Contacts_OwnerId` (`OwnerId`),
  KEY `IX_Contacts_AccountId` (`AccountId`),
  KEY `IX_Contacts_CustomerId` (`CustomerId`),
  KEY `IX_Contacts_Email` (`Email`),
  KEY `IX_Contacts_ReportsToContactId` (`ReportsToContactId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Leads
CREATE TABLE IF NOT EXISTS `Leads` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Email` varchar(255) NOT NULL,
  `Phone` varchar(30) DEFAULT NULL,
  `CompanyName` varchar(255) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Website` varchar(255) DEFAULT NULL,
  `Industry` varchar(100) DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0,
  `Source` int(11) NOT NULL DEFAULT 0,
  `Score` int(11) DEFAULT NULL,
  `Rating` varchar(20) DEFAULT NULL,
  `Region` varchar(100) DEFAULT NULL,
  `QualificationNotes` text DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `ConvertedAccountId` int(11) DEFAULT NULL,
  `ConvertedContactId` int(11) DEFAULT NULL,
  `ConvertedOpportunityId` int(11) DEFAULT NULL,
  `ConvertedAt` datetime(6) DEFAULT NULL,
  `CampaignId` int(11) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Leads_Email` (`Email`),
  KEY `IX_Leads_Status` (`Status`),
  KEY `IX_Leads_Source` (`Source`),
  KEY `IX_Leads_OwnerId` (`OwnerId`),
  KEY `IX_Leads_CampaignId` (`CampaignId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Products
CREATE TABLE IF NOT EXISTS `Products` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `Category` varchar(100) DEFAULT NULL,
  `Type` int(11) NOT NULL DEFAULT 0,
  `UnitPrice` decimal(18,4) NOT NULL DEFAULT 0,
  `CostPrice` decimal(18,4) DEFAULT NULL,
  `Currency` varchar(3) NOT NULL DEFAULT 'USD',
  `UnitOfMeasure` varchar(50) DEFAULT NULL,
  `QuantityInStock` decimal(18,4) DEFAULT NULL,
  `ReorderLevel` decimal(18,4) DEFAULT NULL,
  `TaxRate` decimal(5,2) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `ImageUrl` varchar(500) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Products_Code` (`Code`),
  KEY `IX_Products_Category` (`Category`),
  KEY `IX_Products_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Opportunities
CREATE TABLE IF NOT EXISTS `Opportunities` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `Stage` int(11) NOT NULL DEFAULT 0,
  `Status` int(11) NOT NULL DEFAULT 0,
  `Probability` int(11) NOT NULL DEFAULT 0,
  `Amount` decimal(18,4) NOT NULL DEFAULT 0,
  `Currency` varchar(3) NOT NULL DEFAULT 'USD',
  `ExpectedCloseDate` datetime(6) DEFAULT NULL,
  `ActualCloseDate` datetime(6) DEFAULT NULL,
  `Source` varchar(100) DEFAULT NULL,
  `Campaign` varchar(100) DEFAULT NULL,
  `Competitor` varchar(200) DEFAULT NULL,
  `LossReason` varchar(500) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `NextSteps` varchar(1000) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Opportunities_AccountId` (`AccountId`),
  KEY `IX_Opportunities_CustomerId` (`CustomerId`),
  KEY `IX_Opportunities_Stage` (`Stage`),
  KEY `IX_Opportunities_Status` (`Status`),
  KEY `IX_Opportunities_AssignedUserId` (`AssignedUserId`),
  KEY `IX_Opportunities_ExpectedCloseDate` (`ExpectedCloseDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 5: JUNCTION TABLES (Polymorphic Links)
-- ============================================================================

-- EntityAddressLinks
CREATE TABLE IF NOT EXISTS `EntityAddressLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AddressId` int(11) NOT NULL,
  `EntityType` varchar(50) NOT NULL COMMENT 'Account, Contact, Lead',
  `EntityId` int(11) NOT NULL,
  `AddressType` varchar(50) NOT NULL DEFAULT 'Primary',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `ValidFrom` date DEFAULT NULL,
  `ValidTo` date DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityAddressLinks_Unique` (`EntityType`, `EntityId`, `AddressId`, `AddressType`),
  KEY `IX_EntityAddressLinks_AddressId` (`AddressId`),
  KEY `IX_EntityAddressLinks_Entity` (`EntityType`, `EntityId`),
  CONSTRAINT `FK_EntityAddressLinks_Addresses` FOREIGN KEY (`AddressId`) REFERENCES `Addresses` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EntityPhoneLinks
CREATE TABLE IF NOT EXISTS `EntityPhoneLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PhoneId` int(11) NOT NULL,
  `EntityType` varchar(50) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `PhoneType` varchar(50) NOT NULL DEFAULT 'Office',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `DoNotCall` tinyint(1) NOT NULL DEFAULT 0,
  `ValidFrom` date DEFAULT NULL,
  `ValidTo` date DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityPhoneLinks_Unique` (`EntityType`, `EntityId`, `PhoneId`, `PhoneType`),
  KEY `IX_EntityPhoneLinks_PhoneId` (`PhoneId`),
  KEY `IX_EntityPhoneLinks_Entity` (`EntityType`, `EntityId`),
  CONSTRAINT `FK_EntityPhoneLinks_PhoneNumbers` FOREIGN KEY (`PhoneId`) REFERENCES `PhoneNumbers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EntityEmailLinks
CREATE TABLE IF NOT EXISTS `EntityEmailLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EmailId` int(11) NOT NULL,
  `EntityType` varchar(50) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `EmailType` varchar(50) NOT NULL DEFAULT 'General',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `DoNotEmail` tinyint(1) NOT NULL DEFAULT 0,
  `UnsubscribedDate` datetime(6) DEFAULT NULL,
  `MarketingOptIn` tinyint(1) NOT NULL DEFAULT 1,
  `TransactionalOnly` tinyint(1) NOT NULL DEFAULT 0,
  `ValidFrom` date DEFAULT NULL,
  `ValidTo` date DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntityEmailLinks_Unique` (`EntityType`, `EntityId`, `EmailId`, `EmailType`),
  KEY `IX_EntityEmailLinks_EmailId` (`EmailId`),
  KEY `IX_EntityEmailLinks_Entity` (`EntityType`, `EntityId`),
  CONSTRAINT `FK_EntityEmailLinks_EmailAddresses` FOREIGN KEY (`EmailId`) REFERENCES `EmailAddresses` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EntitySocialMediaLinks
CREATE TABLE IF NOT EXISTS `EntitySocialMediaLinks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SocialMediaAccountId` int(11) NOT NULL,
  `EntityType` varchar(50) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `PreferredForContact` tinyint(1) NOT NULL DEFAULT 0,
  `DoNotContact` tinyint(1) NOT NULL DEFAULT 0,
  `ValidFrom` datetime(6) DEFAULT NULL,
  `ValidTo` datetime(6) DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CreatedBy` int(11) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_EntitySocialMediaLinks_Unique` (`EntityType`, `EntityId`, `SocialMediaAccountId`),
  KEY `IX_EntitySocialMediaLinks_SocialMediaAccountId` (`SocialMediaAccountId`),
  KEY `IX_EntitySocialMediaLinks_Entity` (`EntityType`, `EntityId`),
  CONSTRAINT `FK_EntitySocialMediaLinks_SocialMediaAccounts` FOREIGN KEY (`SocialMediaAccountId`) REFERENCES `SocialMediaAccounts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EntityTags (Polymorphic tagging)
CREATE TABLE IF NOT EXISTS `EntityTags` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `TagId` int(11) NOT NULL,
  `TagName` varchar(200) DEFAULT NULL,
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

-- AccountContacts (Traditional Junction Table)
CREATE TABLE IF NOT EXISTS `AccountContacts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `ContactId` int(11) NOT NULL,
  `Role` varchar(100) DEFAULT NULL,
  `IsPrimaryContact` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_AccountContacts_AccountId_ContactId` (`AccountId`, `ContactId`),
  KEY `IX_AccountContacts_ContactId` (`ContactId`),
  KEY `IX_AccountContacts_Role` (`Role`),
  KEY `IX_AccountContacts_IsPrimaryContact` (`AccountId`, `IsPrimaryContact`),
  CONSTRAINT `FK_AccountContacts_Accounts` FOREIGN KEY (`AccountId`) REFERENCES `Accounts` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AccountContacts_Contacts` FOREIGN KEY (`ContactId`) REFERENCES `Contacts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- OpportunityProducts (Traditional Junction Table)
CREATE TABLE IF NOT EXISTS `OpportunityProducts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `OpportunityId` int(11) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `Quantity` decimal(18,4) NOT NULL DEFAULT 1,
  `UnitPrice` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0,
  `LineTotal` decimal(18,4) NOT NULL DEFAULT 0,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `Notes` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_OpportunityProducts_OpportunityId` (`OpportunityId`),
  KEY `IX_OpportunityProducts_ProductId` (`ProductId`),
  KEY `IX_OpportunityProducts_CreatedAt` (`CreatedAt`),
  CONSTRAINT `FK_OpportunityProducts_Opportunities` FOREIGN KEY (`OpportunityId`) REFERENCES `Opportunities` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_OpportunityProducts_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- LeadProductInterests (Traditional Junction Table)
CREATE TABLE IF NOT EXISTS `LeadProductInterests` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `LeadId` int(11) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `InterestLevel` varchar(50) DEFAULT 'Medium',
  `Notes` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_LeadProductInterests_LeadId` (`LeadId`),
  KEY `IX_LeadProductInterests_ProductId` (`ProductId`),
  KEY `IX_LeadProductInterests_CreatedAt` (`CreatedAt`),
  CONSTRAINT `FK_LeadProductInterests_Leads` FOREIGN KEY (`LeadId`) REFERENCES `Leads` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_LeadProductInterests_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 6: QUOTES
-- ============================================================================

-- Quotes
CREATE TABLE IF NOT EXISTS `Quotes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `QuoteNumber` varchar(20) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `OpportunityId` int(11) DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0,
  `SubTotal` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0,
  `Tax` decimal(18,4) NOT NULL DEFAULT 0,
  `Shipping` decimal(18,4) NOT NULL DEFAULT 0,
  `Total` decimal(18,4) NOT NULL DEFAULT 0,
  `Currency` varchar(3) NOT NULL DEFAULT 'USD',
  `ValidUntil` datetime(6) DEFAULT NULL,
  `TermsAndConditions` longtext DEFAULT NULL,
  `Notes` varchar(2000) DEFAULT NULL,
  `BillingAddressId` int(11) DEFAULT NULL,
  `ShippingAddressId` int(11) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `SentAt` datetime(6) DEFAULT NULL,
  `AcceptedAt` datetime(6) DEFAULT NULL,
  `RejectedAt` datetime(6) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Quotes_QuoteNumber` (`QuoteNumber`),
  KEY `IX_Quotes_AccountId` (`AccountId`),
  KEY `IX_Quotes_CustomerId` (`CustomerId`),
  KEY `IX_Quotes_OpportunityId` (`OpportunityId`),
  KEY `IX_Quotes_Status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- QuoteLineItems
CREATE TABLE IF NOT EXISTS `QuoteLineItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `QuoteId` int(11) NOT NULL,
  `ProductId` int(11) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Quantity` decimal(18,4) NOT NULL DEFAULT 1,
  `UnitPrice` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0,
  `TaxRate` decimal(5,2) DEFAULT NULL,
  `LineTotal` decimal(18,4) NOT NULL DEFAULT 0,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_QuoteLineItems_QuoteId` (`QuoteId`),
  KEY `IX_QuoteLineItems_ProductId` (`ProductId`),
  CONSTRAINT `FK_QuoteLineItems_Quotes` FOREIGN KEY (`QuoteId`) REFERENCES `Quotes` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_QuoteLineItems_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 7: ACTIVITIES AND COMMUNICATION
-- ============================================================================

-- Activities
CREATE TABLE IF NOT EXISTS `Activities` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Type` int(11) NOT NULL DEFAULT 0,
  `Subject` varchar(500) NOT NULL,
  `Description` longtext DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0,
  `Priority` int(11) NOT NULL DEFAULT 1,
  `RelatedEntityType` varchar(100) DEFAULT NULL,
  `RelatedEntityId` int(11) DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
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
  KEY `IX_Activities_AccountId` (`AccountId`),
  KEY `IX_Activities_CustomerId` (`CustomerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Notes
CREATE TABLE IF NOT EXISTS `Notes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) DEFAULT NULL,
  `Content` longtext NOT NULL,
  `RelatedEntityType` varchar(100) NOT NULL,
  `RelatedEntityId` int(11) NOT NULL,
  `IsPinned` tinyint(1) NOT NULL DEFAULT 0,
  `IsInternal` tinyint(1) NOT NULL DEFAULT 0,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Notes_RelatedEntity` (`RelatedEntityType`, `RelatedEntityId`),
  KEY `IX_Notes_IsPinned` (`IsPinned`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Interactions
CREATE TABLE IF NOT EXISTS `Interactions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `InteractionType` varchar(50) NOT NULL,
  `Direction` varchar(20) DEFAULT 'Inbound',
  `Subject` varchar(500) DEFAULT NULL,
  `Content` longtext DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `LeadId` int(11) DEFAULT NULL,
  `EmailAddress` varchar(255) DEFAULT NULL,
  `PhoneNumber` varchar(50) DEFAULT NULL,
  `Channel` varchar(50) DEFAULT NULL,
  `Status` varchar(50) DEFAULT 'New',
  `Priority` varchar(20) DEFAULT 'Normal',
  `AssignedUserId` int(11) DEFAULT NULL,
  `RespondedAt` datetime(6) DEFAULT NULL,
  `ResolvedAt` datetime(6) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `Metadata` longtext DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Interactions_AccountId` (`AccountId`),
  KEY `IX_Interactions_ContactId` (`ContactId`),
  KEY `IX_Interactions_LeadId` (`LeadId`),
  KEY `IX_Interactions_InteractionType` (`InteractionType`),
  KEY `IX_Interactions_Status` (`Status`),
  KEY `IX_Interactions_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EmailTemplates
CREATE TABLE IF NOT EXISTS `EmailTemplates` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Subject` varchar(500) NOT NULL,
  `Body` longtext NOT NULL,
  `BodyFormat` int(11) NOT NULL DEFAULT 0,
  `Category` varchar(100) DEFAULT NULL,
  `EntityType` varchar(100) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsSystem` tinyint(1) NOT NULL DEFAULT 0,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_EmailTemplates_Category` (`Category`),
  KEY `IX_EmailTemplates_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- EmailLogs
CREATE TABLE IF NOT EXISTS `EmailLogs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `From` varchar(255) NOT NULL,
  `To` varchar(500) NOT NULL,
  `Cc` varchar(500) DEFAULT NULL,
  `Bcc` varchar(500) DEFAULT NULL,
  `Subject` varchar(500) NOT NULL,
  `Body` longtext DEFAULT NULL,
  `BodyFormat` int(11) NOT NULL DEFAULT 0,
  `Status` int(11) NOT NULL DEFAULT 0,
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

-- Attachments
CREATE TABLE IF NOT EXISTS `Attachments` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FileName` varchar(255) NOT NULL,
  `OriginalFileName` varchar(255) NOT NULL,
  `ContentType` varchar(100) NOT NULL,
  `FileSize` bigint(20) NOT NULL DEFAULT 0,
  `StoragePath` varchar(500) NOT NULL,
  `StorageType` int(11) NOT NULL DEFAULT 0,
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

-- AuditLogs
CREATE TABLE IF NOT EXISTS `AuditLogs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `Action` varchar(50) NOT NULL,
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

-- ============================================================================
-- SECTION 8: SERVICE DESK
-- ============================================================================

-- ServiceRequestCategories
CREATE TABLE IF NOT EXISTS `ServiceRequestCategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Icon` varchar(50) DEFAULT NULL,
  `Color` varchar(10) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `DefaultPriority` int(11) NOT NULL DEFAULT 1,
  `SlaResponseHours` int(11) DEFAULT NULL,
  `SlaResolutionHours` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ServiceRequestCategories_IsActive` (`IsActive`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ServiceRequestSubcategories
CREATE TABLE IF NOT EXISTS `ServiceRequestSubcategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CategoryId` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL,
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
  CONSTRAINT `FK_ServiceRequestSubcategories_Categories` FOREIGN KEY (`CategoryId`) REFERENCES `ServiceRequestCategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ServiceRequests
CREATE TABLE IF NOT EXISTS `ServiceRequests` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TicketNumber` varchar(20) NOT NULL,
  `Subject` varchar(500) NOT NULL,
  `Description` longtext DEFAULT NULL,
  `Channel` int(11) NOT NULL DEFAULT 0,
  `Status` int(11) NOT NULL DEFAULT 0,
  `Priority` int(11) NOT NULL DEFAULT 1,
  `CategoryId` int(11) DEFAULT NULL,
  `SubcategoryId` int(11) DEFAULT NULL,
  `TypeId` int(11) DEFAULT NULL,
  `AccountId` int(11) DEFAULT NULL,
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
  KEY `IX_ServiceRequests_AccountId` (`AccountId`),
  KEY `IX_ServiceRequests_CustomerId` (`CustomerId`),
  KEY `IX_ServiceRequests_AssignedUserId` (`AssignedUserId`),
  KEY `IX_ServiceRequests_CreatedAt` (`CreatedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 9: WORKFLOWS
-- ============================================================================

-- Workflows
CREATE TABLE IF NOT EXISTS `Workflows` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `EntityType` varchar(100) NOT NULL,
  `TriggerType` int(11) NOT NULL DEFAULT 0,
  `TriggerCondition` longtext DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0,
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

-- WorkflowSteps
CREATE TABLE IF NOT EXISTS `WorkflowSteps` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowId` int(11) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `StepType` int(11) NOT NULL DEFAULT 0,
  `ActionType` varchar(100) DEFAULT NULL,
  `ActionConfig` longtext DEFAULT NULL,
  `ConditionConfig` longtext DEFAULT NULL,
  `NextStepId` int(11) DEFAULT NULL,
  `TrueStepId` int(11) DEFAULT NULL,
  `FalseStepId` int(11) DEFAULT NULL,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `IsRequired` tinyint(1) NOT NULL DEFAULT 0,
  `TimeoutMinutes` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_WorkflowSteps_WorkflowId` (`WorkflowId`),
  CONSTRAINT `FK_WorkflowSteps_Workflows` FOREIGN KEY (`WorkflowId`) REFERENCES `Workflows` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- WorkflowInstances
CREATE TABLE IF NOT EXISTS `WorkflowInstances` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `WorkflowId` int(11) NOT NULL,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `Status` int(11) NOT NULL DEFAULT 0,
  `CurrentStepId` int(11) DEFAULT NULL,
  `StartedAt` datetime(6) DEFAULT NULL,
  `CompletedAt` datetime(6) DEFAULT NULL,
  `ErrorMessage` varchar(2000) DEFAULT NULL,
  `RetryCount` int(11) NOT NULL DEFAULT 0,
  `Data` longtext DEFAULT NULL,
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

-- ============================================================================
-- SECTION 10: SYSTEM CONFIGURATION
-- ============================================================================

-- SystemSettings
CREATE TABLE IF NOT EXISTS `SystemSettings` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SettingKey` varchar(100) DEFAULT NULL,
  `SettingValue` longtext DEFAULT NULL,
  `SettingType` varchar(50) DEFAULT 'String',
  `Category` varchar(100) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsEditable` tinyint(1) NOT NULL DEFAULT 1,
  `CompanyName` varchar(255) DEFAULT NULL,
  `CompanyLogoUrl` varchar(1000) DEFAULT NULL,
  `PrimaryColor` varchar(20) DEFAULT '#6750A4',
  `SecondaryColor` varchar(20) DEFAULT '#958DA5',
  `SelectedPaletteId` int(11) DEFAULT NULL,
  `DateFormat` varchar(50) DEFAULT 'MM/DD/YYYY',
  `TimeFormat` varchar(50) DEFAULT 'h:mm A',
  `DefaultCurrency` varchar(10) DEFAULT 'USD',
  `DefaultTimezone` varchar(100) DEFAULT 'UTC',
  `DefaultLanguage` varchar(10) DEFAULT 'en',
  `IsMultiCurrency` tinyint(1) NOT NULL DEFAULT 0,
  `FiscalYearStart` int(11) DEFAULT 1,
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
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_SystemSettings_SettingKey` (`SettingKey`),
  KEY `IX_SystemSettings_Category` (`Category`),
  KEY `IX_SystemSettings_SelectedPaletteId` (`SelectedPaletteId`),
  CONSTRAINT `FK_SystemSettings_SelectedPaletteId` FOREIGN KEY (`SelectedPaletteId`) REFERENCES `ColorPalettes` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- BrandingConfigs
CREATE TABLE IF NOT EXISTS `BrandingConfigs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SolutionName` varchar(100) NOT NULL DEFAULT 'CRM Solution',
  `CustomLogoPath` varchar(500) DEFAULT NULL,
  `CustomLogoFileName` varchar(255) DEFAULT NULL,
  `FaviconPath` varchar(500) DEFAULT NULL,
  `FaviconFileName` varchar(255) DEFAULT NULL,
  `SoftwareLogoPath` varchar(500) NOT NULL DEFAULT '/assets/logo.png',
  `IsCustomBrandingEnabled` tinyint(1) NOT NULL DEFAULT 1,
  `FaviconDataUrl` longtext DEFAULT NULL,
  `LastLogoUploadedAt` datetime(6) DEFAULT NULL,
  `LastLogoUploadedById` int(11) DEFAULT NULL,
  `LastFaviconUploadedAt` datetime(6) DEFAULT NULL,
  `LastFaviconUploadedById` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  `RowVersion` binary(8) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ModuleUIConfigs
CREATE TABLE IF NOT EXISTS `ModuleUIConfigs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ModuleName` varchar(100) NOT NULL,
  `IsEnabled` tinyint(1) NOT NULL DEFAULT 1,
  `DisplayName` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IconName` varchar(100) NOT NULL,
  `DisplayOrder` int(11) NOT NULL DEFAULT 0,
  `TabsConfig` longtext DEFAULT NULL,
  `LinkedEntitiesConfig` longtext DEFAULT NULL,
  `ListViewConfig` longtext DEFAULT NULL,
  `DetailViewConfig` longtext DEFAULT NULL,
  `QuickCreateConfig` longtext DEFAULT NULL,
  `SearchFilterConfig` longtext DEFAULT NULL,
  `ModuleSettings` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ModuleUIConfigs_ModuleName` (`ModuleName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- CustomFields
CREATE TABLE IF NOT EXISTS `CustomFields` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `Key` varchar(200) NOT NULL,
  `Value` text DEFAULT NULL,
  `DataType` varchar(50) DEFAULT 'string',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_CustomFields_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_CustomFields_Key` (`Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- LLMProviderSettings
CREATE TABLE IF NOT EXISTS `llm_provider_settings` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `provider_name` varchar(50) NOT NULL,
  `model_name` varchar(100) DEFAULT NULL,
  `api_key` varchar(500) DEFAULT NULL,
  `api_base_url` varchar(500) DEFAULT NULL,
  `max_tokens` int(11) DEFAULT 2000,
  `temperature` decimal(3,2) DEFAULT 0.70,
  `is_enabled` tinyint(1) NOT NULL DEFAULT 1,
  `is_default` tinyint(1) NOT NULL DEFAULT 0,
  `priority` int(11) NOT NULL DEFAULT 0,
  `fallback_order` text DEFAULT NULL,
  `effective_fallback_order` text DEFAULT NULL,
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `updated_at` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `is_deleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_LLMProviderSettings_ProviderName` (`provider_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- SECTION 11: BACKWARD COMPATIBILITY - Customers alias removed
-- ============================================================================

-- Customers view removed; Accounts is the canonical table.

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- END OF BASELINE SCHEMA
-- ============================================================================

SELECT 'CRM Baseline Schema v2.0 deployed successfully' AS status;

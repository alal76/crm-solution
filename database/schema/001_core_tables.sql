-- ============================================================================
-- CRM Solution Database Schema - Core Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Core database tables for the CRM system
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: Users
-- ----------------------------------------------------------------------------
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
  `HeaderColor` varchar(10) DEFAULT NULL,
  `PhotoUrl` varchar(500) DEFAULT NULL,
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

-- ----------------------------------------------------------------------------
-- Table: UserGroups
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `UserGroups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsSystemAdmin` tinyint(1) NOT NULL DEFAULT 0,
  `CanAccessDashboard` tinyint(1) NOT NULL DEFAULT 1,
  `CanAccessCustomers` tinyint(1) NOT NULL DEFAULT 1,
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
  `CanCreateCustomers` tinyint(1) NOT NULL DEFAULT 1,
  `CanEditCustomers` tinyint(1) NOT NULL DEFAULT 1,
  `CanDeleteCustomers` tinyint(1) NOT NULL DEFAULT 0,
  `CanViewAllCustomers` tinyint(1) NOT NULL DEFAULT 1,
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
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UserGroups_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: UserGroupMembers
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- Table: Departments
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Departments` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `ManagerId` int(11) DEFAULT NULL,
  `ParentDepartmentId` int(11) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Departments_ManagerId` (`ManagerId`),
  KEY `IX_Departments_ParentDepartmentId` (`ParentDepartmentId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: UserProfiles
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- Table: Customers
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Customers` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Type` varchar(50) NOT NULL DEFAULT 'Business',
  `Industry` varchar(100) DEFAULT NULL,
  `Website` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `AnnualRevenue` decimal(18,2) DEFAULT NULL,
  `NumberOfEmployees` int(11) DEFAULT NULL,
  `Rating` varchar(20) DEFAULT NULL,
  `Status` varchar(50) NOT NULL DEFAULT 'Active',
  `Source` varchar(100) DEFAULT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `LogoUrl` varchar(500) DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `ParentCustomerId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Customers_OwnerId` (`OwnerId`),
  KEY `IX_Customers_AssignedUserId` (`AssignedUserId`),
  KEY `IX_Customers_ParentCustomerId` (`ParentCustomerId`),
  KEY `IX_Customers_Status` (`Status`),
  KEY `IX_Customers_Type` (`Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: Contacts
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Contacts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `Email` varchar(255) DEFAULT NULL,
  `Phone` varchar(50) DEFAULT NULL,
  `Mobile` varchar(50) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Department` varchar(100) DEFAULT NULL,
  `DoNotCall` tinyint(1) NOT NULL DEFAULT 0,
  `DoNotEmail` tinyint(1) NOT NULL DEFAULT 0,
  `Description` varchar(2000) DEFAULT NULL,
  `PhotoUrl` varchar(500) DEFAULT NULL,
  `LeadSource` varchar(100) DEFAULT NULL,
  `OwnerId` int(11) DEFAULT NULL,
  `ReportsToContactId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Contacts_OwnerId` (`OwnerId`),
  KEY `IX_Contacts_ReportsToContactId` (`ReportsToContactId`),
  KEY `IX_Contacts_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: CustomerContacts (Junction table)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `CustomerContacts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CustomerId` int(11) NOT NULL,
  `ContactId` int(11) NOT NULL,
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `Role` varchar(100) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_CustomerContacts_CustomerId_ContactId` (`CustomerId`, `ContactId`),
  KEY `IX_CustomerContacts_ContactId` (`ContactId`),
  CONSTRAINT `FK_CustomerContacts_Customers` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_CustomerContacts_Contacts` FOREIGN KEY (`ContactId`) REFERENCES `Contacts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: Addresses
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Addresses` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(50) NOT NULL,
  `EntityId` int(11) NOT NULL,
  `AddressType` varchar(50) NOT NULL DEFAULT 'Primary',
  `Street` varchar(255) DEFAULT NULL,
  `Street2` varchar(255) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `PostalCode` varchar(20) DEFAULT NULL,
  `Country` varchar(100) DEFAULT NULL,
  `County` varchar(100) DEFAULT NULL,
  `Latitude` decimal(10,7) DEFAULT NULL,
  `Longitude` decimal(10,7) DEFAULT NULL,
  `IsPrimary` tinyint(1) NOT NULL DEFAULT 0,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Addresses_EntityType_EntityId` (`EntityType`, `EntityId`),
  KEY `IX_Addresses_PostalCode` (`PostalCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

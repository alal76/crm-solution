-- ============================================================================
-- CRM Solution Database Schema - Master Data Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Master data tables including ZipCodes, ColorPalettes, etc.
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: ZipCodes - Master data for postal code lookup
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ZipCodes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Country` varchar(100) NOT NULL COMMENT 'Country name',
  `CountryCode` varchar(10) DEFAULT NULL COMMENT 'ISO country code',
  `PostalCode` varchar(20) NOT NULL COMMENT 'Postal/ZIP code',
  `City` varchar(200) NOT NULL COMMENT 'City name',
  `State` varchar(200) DEFAULT NULL COMMENT 'State/Province name',
  `StateCode` varchar(10) DEFAULT NULL COMMENT 'State abbreviation',
  `County` varchar(200) DEFAULT NULL COMMENT 'County name',
  `CountyCode` varchar(10) DEFAULT NULL COMMENT 'County code',
  `Community` varchar(200) DEFAULT NULL COMMENT 'Community/District name',
  `CommunityCode` varchar(10) DEFAULT NULL COMMENT 'Community code',
  `Latitude` decimal(10,7) DEFAULT NULL COMMENT 'GPS latitude',
  `Longitude` decimal(10,7) DEFAULT NULL COMMENT 'GPS longitude',
  `Accuracy` int(11) DEFAULT NULL COMMENT 'Accuracy level 1-6',
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `IX_ZipCodes_PostalCode` (`PostalCode`),
  KEY `IX_ZipCodes_Country` (`Country`),
  KEY `IX_ZipCodes_Country_PostalCode` (`Country`, `PostalCode`),
  KEY `IX_ZipCodes_City` (`City`),
  KEY `IX_ZipCodes_State` (`State`),
  FULLTEXT KEY `FT_ZipCodes_City` (`City`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ColorPalettes - System and user-defined color palettes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ColorPalettes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL COMMENT 'Palette name',
  `Category` varchar(100) DEFAULT NULL COMMENT 'Category: Professional, Vibrant, Nature, etc.',
  `Color1` varchar(10) NOT NULL COMMENT 'Primary color hex',
  `Color2` varchar(10) NOT NULL COMMENT 'Secondary color hex',
  `Color3` varchar(10) NOT NULL COMMENT 'Tertiary color hex',
  `Color4` varchar(10) NOT NULL COMMENT 'Quaternary color hex',
  `Color5` varchar(10) NOT NULL COMMENT 'Quinary color hex',
  `IsUserDefined` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'False for system palettes',
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_ColorPalettes_Category` (`Category`),
  KEY `IX_ColorPalettes_IsUserDefined` (`IsUserDefined`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: SystemSettings - Application-wide settings
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `SystemSettings` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SettingKey` varchar(100) NOT NULL COMMENT 'Unique setting key',
  `SettingValue` longtext DEFAULT NULL COMMENT 'Setting value (can be JSON)',
  `SettingType` varchar(50) NOT NULL DEFAULT 'String' COMMENT 'Type: String, Number, Boolean, JSON',
  `Category` varchar(100) DEFAULT NULL COMMENT 'Settings category',
  `Description` varchar(500) DEFAULT NULL COMMENT 'Setting description',
  `IsEditable` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Can be edited by admin',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_SystemSettings_SettingKey` (`SettingKey`),
  KEY `IX_SystemSettings_Category` (`Category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ModuleUIConfigs - UI configuration for each module/navigation
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ModuleUIConfigs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ModuleName` varchar(100) NOT NULL COMMENT 'Module identifier',
  `IsEnabled` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Module enabled in navigation',
  `DisplayName` varchar(100) NOT NULL COMMENT 'Display name in UI',
  `Description` varchar(500) DEFAULT NULL,
  `IconName` varchar(100) NOT NULL COMMENT 'Material UI icon name',
  `DisplayOrder` int(11) NOT NULL DEFAULT 0 COMMENT 'Sort order in navigation',
  `TabsConfig` longtext DEFAULT NULL COMMENT 'JSON config for detail view tabs',
  `LinkedEntitiesConfig` longtext DEFAULT NULL COMMENT 'JSON config for linked entities',
  `ListViewConfig` longtext DEFAULT NULL COMMENT 'JSON config for list view columns',
  `DetailViewConfig` longtext DEFAULT NULL COMMENT 'JSON config for detail view layout',
  `QuickCreateConfig` longtext DEFAULT NULL COMMENT 'JSON config for quick create form',
  `SearchFilterConfig` longtext DEFAULT NULL COMMENT 'JSON config for search filters',
  `ModuleSettings` longtext DEFAULT NULL COMMENT 'Additional module-specific settings',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ModuleUIConfigs_ModuleName` (`ModuleName`),
  KEY `IX_ModuleUIConfigs_IsEnabled` (`IsEnabled`),
  KEY `IX_ModuleUIConfigs_DisplayOrder` (`DisplayOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: ModuleFieldConfigurations - Field-level configuration per module
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `ModuleFieldConfigurations` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ModuleName` varchar(100) NOT NULL COMMENT 'Module identifier',
  `FieldName` varchar(100) NOT NULL COMMENT 'Field name/key',
  `FieldLabel` varchar(100) NOT NULL COMMENT 'Display label',
  `FieldType` varchar(50) NOT NULL COMMENT 'Field type: text, number, select, etc.',
  `TabIndex` int(11) NOT NULL DEFAULT 0 COMMENT 'Tab index for this field',
  `TabName` varchar(100) NOT NULL DEFAULT 'General' COMMENT 'Tab name',
  `DisplayOrder` int(11) NOT NULL DEFAULT 0 COMMENT 'Field order within tab',
  `IsEnabled` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Field visible in UI',
  `IsRequired` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Field is required',
  `GridSize` int(11) NOT NULL DEFAULT 6 COMMENT 'Grid column width 1-12',
  `Placeholder` varchar(200) DEFAULT NULL,
  `HelpText` varchar(500) DEFAULT NULL,
  `Options` longtext DEFAULT NULL COMMENT 'JSON options for select/multiselect',
  `ParentField` varchar(100) DEFAULT NULL COMMENT 'Parent field for dependencies',
  `ParentFieldValue` varchar(255) DEFAULT NULL COMMENT 'Show when parent has this value',
  `IsReorderable` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Can be reordered',
  `IsRequiredConfigurable` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Required flag can be changed',
  `IsHideable` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Can be hidden',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ModuleFieldConfigs_Module_Field` (`ModuleName`, `FieldName`),
  KEY `IX_ModuleFieldConfigs_ModuleName` (`ModuleName`),
  KEY `IX_ModuleFieldConfigs_TabIndex` (`TabIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: LookupCategories - Categories for lookup/dropdown values
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `LookupCategories` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL COMMENT 'Category name',
  `Description` varchar(500) DEFAULT NULL,
  `IsSystem` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'System category cannot be deleted',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_LookupCategories_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: LookupItems - Lookup/dropdown values
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `LookupItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CategoryId` int(11) NOT NULL,
  `Value` varchar(200) NOT NULL COMMENT 'Display value',
  `Code` varchar(50) DEFAULT NULL COMMENT 'Short code',
  `Description` varchar(500) DEFAULT NULL,
  `ParentItemId` int(11) DEFAULT NULL COMMENT 'For hierarchical lookups',
  `DisplayOrder` int(11) NOT NULL DEFAULT 0,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `IsDefault` tinyint(1) NOT NULL DEFAULT 0,
  `Metadata` longtext DEFAULT NULL COMMENT 'Additional JSON metadata',
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_LookupItems_CategoryId` (`CategoryId`),
  KEY `IX_LookupItems_ParentItemId` (`ParentItemId`),
  CONSTRAINT `FK_LookupItems_LookupCategories` FOREIGN KEY (`CategoryId`) REFERENCES `LookupCategories` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

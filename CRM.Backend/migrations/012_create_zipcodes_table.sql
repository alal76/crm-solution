-- ============================================================================
-- Migration: Add ZipCodes table for address auto-population
-- Version: 012
-- Date: 2026-01-23
-- Description: Creates the ZipCodes master data table for postal code lookups
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- Create ZipCodes table if not exists
CREATE TABLE IF NOT EXISTS `ZipCodes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Country` varchar(100) NOT NULL,
  `CountryCode` varchar(10) NOT NULL,
  `PostalCode` varchar(20) NOT NULL,
  `City` varchar(200) NOT NULL,
  `State` varchar(100) DEFAULT NULL,
  `StateCode` varchar(10) DEFAULT NULL,
  `County` varchar(100) DEFAULT NULL,
  `CountyCode` varchar(20) DEFAULT NULL,
  `Community` varchar(100) DEFAULT NULL,
  `CommunityCode` varchar(20) DEFAULT NULL,
  `Latitude` decimal(10,6) DEFAULT NULL,
  `Longitude` decimal(10,6) DEFAULT NULL,
  `Accuracy` int(11) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`Id`),
  KEY `IX_ZipCodes_PostalCode` (`PostalCode`),
  KEY `IX_ZipCodes_CountryCode` (`CountryCode`),
  KEY `IX_ZipCodes_Country_PostalCode` (`CountryCode`, `PostalCode`),
  KEY `IX_ZipCodes_City` (`City`),
  KEY `IX_ZipCodes_State` (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

SET FOREIGN_KEY_CHECKS = 1;

-- Migration complete
SELECT 'Migration 012: ZipCodes table created successfully' AS Status;

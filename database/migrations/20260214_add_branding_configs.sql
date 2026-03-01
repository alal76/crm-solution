-- ============================================================================
-- Migration: Add BrandingConfigs table
-- Date: 2026-02-14
-- ============================================================================


SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
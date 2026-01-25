-- ============================================================================
-- CRM Solution Database Schema - Products, Opportunities, Quotes Tables
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Sales pipeline and product management tables
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------
-- Table: Products
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Products` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL COMMENT 'Product SKU/Code',
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `Category` varchar(100) DEFAULT NULL,
  `Type` int(11) NOT NULL DEFAULT 0 COMMENT '0=Product, 1=Service, 2=Bundle',
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

-- ----------------------------------------------------------------------------
-- Table: Opportunities
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Opportunities` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `Stage` int(11) NOT NULL DEFAULT 0 COMMENT '0=Qualification, 1=Proposal, etc.',
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Open, 1=Won, 2=Lost',
  `Probability` int(11) NOT NULL DEFAULT 0 COMMENT 'Win probability %',
  `Amount` decimal(18,4) NOT NULL DEFAULT 0,
  `Currency` varchar(3) NOT NULL DEFAULT 'USD',
  `ExpectedCloseDate` datetime(6) DEFAULT NULL,
  `ActualCloseDate` datetime(6) DEFAULT NULL,
  `Source` varchar(100) DEFAULT NULL COMMENT 'Lead source',
  `Campaign` varchar(100) DEFAULT NULL,
  `Competitor` varchar(200) DEFAULT NULL,
  `LossReason` varchar(500) DEFAULT NULL,
  `AssignedUserId` int(11) DEFAULT NULL,
  `NextSteps` varchar(1000) DEFAULT NULL,
  `Tags` varchar(500) DEFAULT NULL,
  `CreatedByUserId` int(11) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_Opportunities_CustomerId` (`CustomerId`),
  KEY `IX_Opportunities_Stage` (`Stage`),
  KEY `IX_Opportunities_Status` (`Status`),
  KEY `IX_Opportunities_AssignedUserId` (`AssignedUserId`),
  KEY `IX_Opportunities_ExpectedCloseDate` (`ExpectedCloseDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: OpportunityProducts
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `OpportunityProducts` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `OpportunityId` int(11) NOT NULL,
  `ProductId` int(11) NOT NULL,
  `Quantity` decimal(18,4) NOT NULL DEFAULT 1,
  `UnitPrice` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Amount, 1=Percentage',
  `LineTotal` decimal(18,4) NOT NULL DEFAULT 0,
  `SortOrder` int(11) NOT NULL DEFAULT 0,
  `Notes` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `IX_OpportunityProducts_OpportunityId` (`OpportunityId`),
  KEY `IX_OpportunityProducts_ProductId` (`ProductId`),
  CONSTRAINT `FK_OpportunityProducts_Opportunities` FOREIGN KEY (`OpportunityId`) REFERENCES `Opportunities` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_OpportunityProducts_Products` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: Quotes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Quotes` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `QuoteNumber` varchar(20) NOT NULL COMMENT 'Auto-generated quote number',
  `Name` varchar(200) NOT NULL,
  `Description` varchar(2000) DEFAULT NULL,
  `CustomerId` int(11) DEFAULT NULL,
  `ContactId` int(11) DEFAULT NULL,
  `OpportunityId` int(11) DEFAULT NULL,
  `Status` int(11) NOT NULL DEFAULT 0 COMMENT '0=Draft, 1=Sent, 2=Accepted, etc.',
  `SubTotal` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Amount, 1=Percentage',
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
  KEY `IX_Quotes_CustomerId` (`CustomerId`),
  KEY `IX_Quotes_OpportunityId` (`OpportunityId`),
  KEY `IX_Quotes_Status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------------------------------------------------------
-- Table: QuoteLineItems
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `QuoteLineItems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `QuoteId` int(11) NOT NULL,
  `ProductId` int(11) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Quantity` decimal(18,4) NOT NULL DEFAULT 1,
  `UnitPrice` decimal(18,4) NOT NULL DEFAULT 0,
  `Discount` decimal(18,4) NOT NULL DEFAULT 0,
  `DiscountType` int(11) NOT NULL DEFAULT 0 COMMENT '0=Amount, 1=Percentage',
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

SET FOREIGN_KEY_CHECKS = 1;

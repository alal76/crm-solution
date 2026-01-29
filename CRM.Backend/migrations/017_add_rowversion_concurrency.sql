-- Migration: 017_add_rowversion_concurrency.sql
-- Purpose: Add RowVersion columns to all entity tables for optimistic concurrency control
-- This enables multi-user conflict detection when concurrent updates occur

-- Note: MariaDB/MySQL doesn't have native ROWVERSION like SQL Server
-- We use BINARY(8) to store a version identifier that changes on each update
-- The application layer will handle version generation and comparison

-- =====================================================
-- Add RowVersion to Core Entity Tables
-- =====================================================

-- Customers table
ALTER TABLE Customers 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Contacts table
ALTER TABLE Contacts 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Leads table
ALTER TABLE Leads 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Opportunities table
ALTER TABLE Opportunities 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Products table
ALTER TABLE Products 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Quotes table
ALTER TABLE Quotes 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- QuoteLineItems table
ALTER TABLE QuoteLineItems 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- MarketingCampaigns table
ALTER TABLE MarketingCampaigns 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- ServiceRequests table
ALTER TABLE ServiceRequests 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Notes table
ALTER TABLE Notes 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Users table
ALTER TABLE Users 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- =====================================================
-- Add RowVersion to Supporting Entity Tables
-- =====================================================

-- Interactions table
ALTER TABLE Interactions 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- CrmTasks table  
ALTER TABLE CrmTasks 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Activities table
ALTER TABLE Activities 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- CustomerContacts (junction table)
ALTER TABLE CustomerContacts 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Addresses table
ALTER TABLE Addresses 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- PhoneNumbers table
ALTER TABLE PhoneNumbers 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- EmailAddresses table
ALTER TABLE EmailAddresses 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- SocialMediaAccounts table
ALTER TABLE SocialMediaAccounts 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- ServiceRequestCategories table
ALTER TABLE ServiceRequestCategories 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- ServiceRequestSubcategories table
ALTER TABLE ServiceRequestSubcategories 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- CampaignMetrics table
ALTER TABLE CampaignMetrics 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- =====================================================
-- Add RowVersion to Configuration Tables
-- =====================================================

-- LookupCategories table
ALTER TABLE LookupCategories 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- LookupItems table
ALTER TABLE LookupItems 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- UserGroups table
ALTER TABLE UserGroups 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- UserProfiles table
ALTER TABLE UserProfiles 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- Departments table
ALTER TABLE Departments 
ADD COLUMN IF NOT EXISTS RowVersion BINARY(8) NULL DEFAULT NULL;

-- =====================================================
-- Create trigger function to auto-update RowVersion
-- =====================================================

-- Drop existing triggers if they exist
DROP TRIGGER IF EXISTS trg_Customers_RowVersion;
DROP TRIGGER IF EXISTS trg_Contacts_RowVersion;
DROP TRIGGER IF EXISTS trg_Leads_RowVersion;
DROP TRIGGER IF EXISTS trg_Opportunities_RowVersion;
DROP TRIGGER IF EXISTS trg_Products_RowVersion;
DROP TRIGGER IF EXISTS trg_Quotes_RowVersion;
DROP TRIGGER IF EXISTS trg_ServiceRequests_RowVersion;
DROP TRIGGER IF EXISTS trg_Notes_RowVersion;
DROP TRIGGER IF EXISTS trg_MarketingCampaigns_RowVersion;

-- Customers trigger
DELIMITER //
CREATE TRIGGER trg_Customers_RowVersion
BEFORE UPDATE ON Customers
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Contacts trigger
DELIMITER //
CREATE TRIGGER trg_Contacts_RowVersion
BEFORE UPDATE ON Contacts
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Leads trigger
DELIMITER //
CREATE TRIGGER trg_Leads_RowVersion
BEFORE UPDATE ON Leads
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Opportunities trigger
DELIMITER //
CREATE TRIGGER trg_Opportunities_RowVersion
BEFORE UPDATE ON Opportunities
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Products trigger
DELIMITER //
CREATE TRIGGER trg_Products_RowVersion
BEFORE UPDATE ON Products
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Quotes trigger
DELIMITER //
CREATE TRIGGER trg_Quotes_RowVersion
BEFORE UPDATE ON Quotes
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- ServiceRequests trigger
DELIMITER //
CREATE TRIGGER trg_ServiceRequests_RowVersion
BEFORE UPDATE ON ServiceRequests
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- Notes trigger
DELIMITER //
CREATE TRIGGER trg_Notes_RowVersion
BEFORE UPDATE ON Notes
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- MarketingCampaigns trigger
DELIMITER //
CREATE TRIGGER trg_MarketingCampaigns_RowVersion
BEFORE UPDATE ON MarketingCampaigns
FOR EACH ROW
BEGIN
    SET NEW.RowVersion = UNHEX(LPAD(HEX(COALESCE(CONV(HEX(OLD.RowVersion), 16, 10), 0) + 1), 16, '0'));
END//
DELIMITER ;

-- =====================================================
-- Initialize RowVersion for existing records
-- =====================================================

UPDATE Customers SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Contacts SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Leads SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Opportunities SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Products SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Quotes SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE ServiceRequests SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Notes SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE MarketingCampaigns SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Users SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Interactions SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE CrmTasks SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;
UPDATE Activities SET RowVersion = UNHEX('0000000000000001') WHERE RowVersion IS NULL;

-- Record migration
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
SELECT '20260129_AddRowVersionConcurrency', '8.0.0'
WHERE NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260129_AddRowVersionConcurrency');

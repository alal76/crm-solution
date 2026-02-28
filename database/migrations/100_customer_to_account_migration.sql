-- =============================================================================
-- CRM Solution: Customer to Account Migration Script
-- =============================================================================
-- Purpose: Migrate all Customer references to Account for consistency
-- Author: CRM Development Team
-- Date: 2026-01-31
-- Version: 1.0
-- =============================================================================
-- IMPORTANT: Run this script in a transaction and test in staging first!
-- =============================================================================

-- Start transaction

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

START TRANSACTION;

-- =============================================================================
-- PHASE 1: Rename main Customers table to Accounts
-- =============================================================================

-- Check if Accounts table already exists (from microservices hybrid schema)
-- If both exist, we need to merge or decide which is canonical

-- First, let's see what we have:
-- SELECT COUNT(*) as customer_count FROM Customers;
-- SELECT COUNT(*) as account_count FROM Accounts; -- May not exist

-- Option A: If Customers is the canonical table, rename it
-- RENAME TABLE Customers TO Accounts;

-- For now, we'll ADD AccountId columns and migrate data, keeping both temporarily

-- =============================================================================
-- PHASE 2: Add AccountId columns where missing (parallel to CustomerId)
-- =============================================================================

-- Opportunities table (already has AccountId, but may have CustomerId too)
-- ALTER TABLE Opportunities ADD COLUMN IF NOT EXISTS AccountId INT NULL;
-- UPDATE Opportunities SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Quotes table
ALTER TABLE Quotes ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Quotes SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- ServiceRequests table  
ALTER TABLE ServiceRequests ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE ServiceRequests SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Notes table
ALTER TABLE Notes ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Notes SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Activities table
ALTER TABLE Activities ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Activities SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CrmTasks table
ALTER TABLE CrmTasks ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CrmTasks SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Interactions table
ALTER TABLE Interactions ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Interactions SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- Conversations table
ALTER TABLE Conversations ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE Conversations SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CommunicationMessages table
ALTER TABLE CommunicationMessages ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CommunicationMessages SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CampaignConversions table
ALTER TABLE CampaignConversions ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CampaignConversions SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- CampaignRecipients table
ALTER TABLE CampaignRecipients ADD COLUMN IF NOT EXISTS AccountId INT NULL;
UPDATE CampaignRecipients SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 3: Rename junction/relationship tables
-- =============================================================================

-- CustomerContacts -> AccountContacts
-- Check if table exists first
-- RENAME TABLE CustomerContacts TO AccountContacts;
-- ALTER TABLE AccountContacts CHANGE COLUMN CustomerId AccountId INT NOT NULL;
-- ALTER TABLE AccountContacts CHANGE COLUMN DepartmentAtCustomer DepartmentAtAccount VARCHAR(255);
-- ALTER TABLE AccountContacts CHANGE COLUMN PositionAtCustomer PositionAtAccount VARCHAR(255);

-- CustomerTerritoryAssignments -> AccountTerritoryAssignments
-- RENAME TABLE CustomerTerritoryAssignments TO AccountTerritoryAssignments;
-- ALTER TABLE AccountTerritoryAssignments CHANGE COLUMN CustomerId AccountId INT NOT NULL;

-- =============================================================================
-- PHASE 4: Update Contacts table
-- =============================================================================

-- Contacts already has AccountId, ensure it's populated
UPDATE Contacts SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 5: Update Leads table
-- =============================================================================

-- Leads already has AccountId, ensure it's populated  
UPDATE Leads SET AccountId = CustomerId WHERE AccountId IS NULL AND CustomerId IS NOT NULL;

-- =============================================================================
-- PHASE 6: Create AccountId indexes (for performance)
-- =============================================================================

-- Add indexes on new AccountId columns
CREATE INDEX IF NOT EXISTS IX_Quotes_AccountId ON Quotes(AccountId);
CREATE INDEX IF NOT EXISTS IX_ServiceRequests_AccountId ON ServiceRequests(AccountId);
CREATE INDEX IF NOT EXISTS IX_Notes_AccountId ON Notes(AccountId);
CREATE INDEX IF NOT EXISTS IX_Activities_AccountId ON Activities(AccountId);
CREATE INDEX IF NOT EXISTS IX_CrmTasks_AccountId ON CrmTasks(AccountId);

-- =============================================================================
-- PHASE 7: Rename Customers table columns that should stay
-- =============================================================================

-- These columns are about the Customer entity itself, so they become Account entity:
-- CustomerType -> AccountType (but this is already the category like Individual/Organization)
-- CustomerHealthScore -> AccountHealthScore
-- ParentCustomerId -> ParentAccountId
-- ReferredByCustomerId -> ReferredByAccountId

-- We'll handle these in the entity rename phase

-- =============================================================================
-- PHASE 8: Update UserGroups permission column names (cosmetic, low priority)
-- =============================================================================

-- These are permission flags, naming can stay as-is for backward compatibility
-- CanAccessCustomers, CanCreateCustomers, CanDeleteCustomers
-- Could rename to CanAccessAccounts, etc. but not critical

-- =============================================================================
-- PHASE 9: Clean up duplicate/orphan columns
-- =============================================================================

-- Remove duplicate AccountId columns in Opportunities
-- ALTER TABLE Opportunities DROP COLUMN IF EXISTS AccountId1;

-- =============================================================================
-- VERIFICATION QUERIES
-- =============================================================================

-- Verify all CustomerId values were copied to AccountId
SELECT 'Quotes' as TableName, 
       COUNT(*) as TotalRows,
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END) as WithAccountId,
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END) as MissingAccountId
FROM Quotes
UNION ALL
SELECT 'ServiceRequests', COUNT(*), 
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM ServiceRequests
UNION ALL
SELECT 'Notes', COUNT(*),
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM Notes
UNION ALL
SELECT 'Activities', COUNT(*),
       SUM(CASE WHEN AccountId IS NOT NULL THEN 1 ELSE 0 END),
       SUM(CASE WHEN CustomerId IS NOT NULL AND AccountId IS NULL THEN 1 ELSE 0 END)
FROM Activities;

-- =============================================================================
-- COMMIT OR ROLLBACK
-- =============================================================================

-- If all looks good:
COMMIT;

-- If there are issues:
-- ROLLBACK;

-- =============================================================================
-- POST-MIGRATION: Drop old CustomerId columns (RUN AFTER CODE CHANGES DEPLOYED)
-- =============================================================================

-- DANGER: Only run this after all code has been updated to use AccountId
-- 
-- ALTER TABLE Quotes DROP COLUMN CustomerId;
-- ALTER TABLE ServiceRequests DROP COLUMN CustomerId;
-- ALTER TABLE Notes DROP COLUMN CustomerId;
-- ALTER TABLE Activities DROP COLUMN CustomerId;
-- ALTER TABLE CrmTasks DROP COLUMN CustomerId;
-- ALTER TABLE Contacts DROP COLUMN CustomerId;
-- ALTER TABLE Leads DROP COLUMN CustomerId;
-- ALTER TABLE Interactions DROP COLUMN CustomerId;
-- ALTER TABLE Conversations DROP COLUMN CustomerId;
-- ALTER TABLE CommunicationMessages DROP COLUMN CustomerId;
-- ALTER TABLE CampaignConversions DROP COLUMN CustomerId;
-- ALTER TABLE CampaignRecipients DROP COLUMN CustomerId;
-- ALTER TABLE Opportunities DROP COLUMN CustomerId;

-- =============================================================================
-- END OF MIGRATION SCRIPT
-- =============================================================================
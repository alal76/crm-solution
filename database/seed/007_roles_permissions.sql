-- ============================================================================
-- CRM Solution Database Seed Data - User Groups with Permissions
-- Version: 1.0
-- Date: 2026-01-28
-- Description: Enhanced user groups with full permission configurations
-- Note: SysAdmin (Id=1) is created during initial setup, this adds additional roles
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- UserGroups - Role definitions with all permissions
-- DataAccessScope: 'all' = All records, 'own' = Own records only, 'team' = Team records
-- Starts at Id=2 to not conflict with existing SysAdmin
-- ============================================================================

-- Clean up any existing non-system groups
DELETE FROM UserGroups WHERE Id > 1;

INSERT INTO UserGroups (
  Id, Name, Description, IsActive, IsDefault, DisplayOrder, HeaderColor, AccessibleMenuItems,
  IsSystemAdmin,
  CanAccessDashboard, CanAccessCustomers, CanAccessContacts, CanAccessLeads,
  CanAccessOpportunities, CanAccessProducts, CanAccessServices, CanAccessCampaigns,
  CanAccessQuotes, CanAccessTasks, CanAccessActivities, CanAccessNotes,
  CanAccessWorkflows, CanAccessServiceRequests, CanAccessReports, CanAccessSettings,
  CanAccessUserManagement,
  CanCreateCustomers, CanEditCustomers, CanDeleteCustomers, CanViewAllCustomers,
  CanCreateContacts, CanEditContacts, CanDeleteContacts,
  CanCreateLeads, CanEditLeads, CanDeleteLeads, CanConvertLeads,
  CanCreateOpportunities, CanEditOpportunities, CanDeleteOpportunities, CanCloseOpportunities,
  CanCreateProducts, CanEditProducts, CanDeleteProducts, CanManagePricing,
  CanCreateCampaigns, CanEditCampaigns, CanDeleteCampaigns, CanLaunchCampaigns,
  CanCreateQuotes, CanEditQuotes, CanDeleteQuotes, CanApproveQuotes,
  CanCreateTasks, CanEditTasks, CanDeleteTasks, CanAssignTasks,
  CanCreateWorkflows, CanEditWorkflows, CanDeleteWorkflows, CanActivateWorkflows,
  DataAccessScope, CanExportData, CanImportData, CanBulkEdit, CanBulkDelete,
  CreatedAt, IsDeleted
) VALUES

-- 2. Sales Manager
(2, 'Sales Manager', 'Manages sales team with full access to sales modules',
 1, 0, 2, '#2e7d32', '[]', 0,
 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0,
 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
 'team', 1, 0, 1, 0, NOW(), 0),

-- 3. Sales Representative
(3, 'Sales Representative', 'Standard sales user with access to leads, opportunities, and quotes',
 1, 1, 3, '#1976d2', '[]', 0,
 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0,
 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0,
 'own', 0, 0, 0, 0, NOW(), 0),

-- 4. Marketing Manager
(4, 'Marketing Manager', 'Manages marketing campaigns with full marketing access',
 1, 0, 4, '#7b1fa2', '[]', 0,
 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0,
 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0,
 'all', 1, 1, 1, 0, NOW(), 0),

-- 5. Marketing Specialist
(5, 'Marketing Specialist', 'Standard marketing user with campaign execution access',
 1, 0, 5, '#9c27b0', '[]', 0,
 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0,
 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0,
 'own', 0, 0, 0, 0, NOW(), 0),

-- 6. Customer Service Manager
(6, 'Customer Service Manager', 'Manages service desk with full access to service modules',
 1, 0, 6, '#00796b', '[]', 0,
 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0,
 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0,
 'team', 1, 0, 1, 0, NOW(), 0),

-- 7. Customer Service Agent
(7, 'Customer Service Agent', 'Standard service agent handling tickets and service requests',
 1, 0, 7, '#26a69a', '[]', 0,
 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0,
 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0,
 'own', 0, 0, 0, 0, NOW(), 0),

-- 8. Account Manager
(8, 'Account Manager', 'Manages customer accounts and relationships',
 1, 0, 8, '#f57c00', '[]', 0,
 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 0,
 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0,
 'own', 1, 0, 0, 0, NOW(), 0),

-- 9. Read Only User
(9, 'Read Only User', 'View-only access to all non-sensitive data',
 1, 0, 9, '#757575', '[]', 0,
 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0,
 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 'all', 0, 0, 0, 0, NOW(), 0),

-- 10. Executive
(10, 'Executive', 'Executive-level access with dashboards and reports',
 1, 0, 10, '#37474f', '[]', 0,
 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0,
 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
 'all', 1, 0, 0, 0, NOW(), 0);

-- Update existing SysAdmin to ensure it has all permissions
UPDATE UserGroups SET
  Description = 'Full system access with all administrative privileges',
  IsActive = 1,
  IsDefault = 0,
  DisplayOrder = 0,
  HeaderColor = '#1a237e',
  AccessibleMenuItems = '[]',
  IsSystemAdmin = 1,
  CanAccessDashboard = 1, CanAccessCustomers = 1, CanAccessContacts = 1, CanAccessLeads = 1,
  CanAccessOpportunities = 1, CanAccessProducts = 1, CanAccessServices = 1, CanAccessCampaigns = 1,
  CanAccessQuotes = 1, CanAccessTasks = 1, CanAccessActivities = 1, CanAccessNotes = 1,
  CanAccessWorkflows = 1, CanAccessServiceRequests = 1, CanAccessReports = 1, CanAccessSettings = 1,
  CanAccessUserManagement = 1,
  CanCreateCustomers = 1, CanEditCustomers = 1, CanDeleteCustomers = 1, CanViewAllCustomers = 1,
  CanCreateContacts = 1, CanEditContacts = 1, CanDeleteContacts = 1,
  CanCreateLeads = 1, CanEditLeads = 1, CanDeleteLeads = 1, CanConvertLeads = 1,
  CanCreateOpportunities = 1, CanEditOpportunities = 1, CanDeleteOpportunities = 1, CanCloseOpportunities = 1,
  CanCreateProducts = 1, CanEditProducts = 1, CanDeleteProducts = 1, CanManagePricing = 1,
  CanCreateCampaigns = 1, CanEditCampaigns = 1, CanDeleteCampaigns = 1, CanLaunchCampaigns = 1,
  CanCreateQuotes = 1, CanEditQuotes = 1, CanDeleteQuotes = 1, CanApproveQuotes = 1,
  CanCreateTasks = 1, CanEditTasks = 1, CanDeleteTasks = 1, CanAssignTasks = 1,
  CanCreateWorkflows = 1, CanEditWorkflows = 1, CanDeleteWorkflows = 1, CanActivateWorkflows = 1,
  DataAccessScope = 'all', CanExportData = 1, CanImportData = 1, CanBulkEdit = 1, CanBulkDelete = 1
WHERE Id = 1;

SET FOREIGN_KEY_CHECKS = 1;

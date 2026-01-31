-- ============================================================================
-- CRM Solution Database Seed Data - ESSENTIAL
-- Version: 2.0
-- Description: Minimum required data for system to function
-- Copyright (C) 2024-2026 Abhishek Lal
-- Licensed under the GNU Affero General Public License v3.0
-- ============================================================================
-- This file contains data WITHOUT WHICH THE SYSTEM CANNOT FUNCTION:
-- - System configuration
-- - User groups and roles
-- - Lookup categories and values
-- - Default departments
-- - Required workflow definitions
-- ============================================================================

-- Use PostgreSQL syntax
SET client_encoding = 'UTF8';

-- ============================================================================
-- SECTION 1: SYSTEM SETTINGS
-- Essential configuration values for CRM operation
-- ============================================================================

INSERT INTO "SystemSettings" ("Id", "Category", "Key", "Value", "DataType", "Description", "IsReadOnly", "CreatedAt", "IsDeleted")
VALUES 
    (1, 'General', 'ApplicationName', 'CRM Solution', 'String', 'Name of the application', false, NOW(), false),
    (2, 'General', 'DefaultCurrency', 'USD', 'String', 'Default currency for transactions', false, NOW(), false),
    (3, 'General', 'DefaultTimezone', 'America/New_York', 'String', 'Default timezone', false, NOW(), false),
    (4, 'General', 'DateFormat', 'yyyy-MM-dd', 'String', 'Date format for display', false, NOW(), false),
    (5, 'General', 'TimeFormat', 'HH:mm:ss', 'String', 'Time format for display', false, NOW(), false),
    (6, 'Email', 'FromAddress', 'noreply@crm.local', 'String', 'Default from email address', false, NOW(), false),
    (7, 'Email', 'FromName', 'CRM System', 'String', 'Default from name', false, NOW(), false),
    (8, 'Security', 'PasswordMinLength', '8', 'Integer', 'Minimum password length', false, NOW(), false),
    (9, 'Security', 'SessionTimeoutMinutes', '60', 'Integer', 'Session timeout in minutes', false, NOW(), false),
    (10, 'Security', 'MaxLoginAttempts', '5', 'Integer', 'Max failed login attempts before lockout', false, NOW(), false),
    (11, 'Leads', 'DefaultOwnerAssignment', 'RoundRobin', 'String', 'Lead assignment method', false, NOW(), false),
    (12, 'Opportunities', 'DefaultCurrency', 'USD', 'String', 'Default opportunity currency', false, NOW(), false),
    (13, 'ServiceDesk', 'AutoAssign', 'true', 'Boolean', 'Auto-assign service requests', false, NOW(), false),
    (14, 'ServiceDesk', 'SLAEnabled', 'true', 'Boolean', 'Enable SLA tracking', false, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================================
-- SECTION 2: DEPARTMENTS
-- Essential organizational units
-- ============================================================================

INSERT INTO "Departments" ("Id", "Name", "Code", "Description", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    (1, 'Sales', 'SALES', 'Sales and business development team', true, NOW(), false),
    (2, 'Marketing', 'MKT', 'Marketing and demand generation team', true, NOW(), false),
    (3, 'Customer Support', 'SUP', 'Customer support and service team', true, NOW(), false),
    (4, 'Engineering', 'ENG', 'Engineering and development team', true, NOW(), false),
    (5, 'Operations', 'OPS', 'Operations and administration team', true, NOW(), false),
    (6, 'Finance', 'FIN', 'Finance and accounting team', true, NOW(), false),
    (7, 'Human Resources', 'HR', 'Human resources team', true, NOW(), false),
    (8, 'Executive', 'EXEC', 'Executive leadership team', true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Departments_Id_seq"', (SELECT MAX("Id") FROM "Departments"));

-- ============================================================================
-- SECTION 3: USER GROUPS (Roles with Permissions)
-- ============================================================================

INSERT INTO "UserGroups" ("Id", "Name", "Description", "IsActive", "CreatedAt", "IsDeleted",
    "CanAccessDashboard", "CanAccessLeads", "CanAccessContacts", "CanAccessOpportunities",
    "CanAccessCustomers", "CanAccessCampaigns", "CanAccessReports", "CanAccessAdmin",
    "CanAccessServiceDesk", "CanAccessProducts",
    "CanCreateLeads", "CanEditLeads", "CanDeleteLeads",
    "CanCreateContacts", "CanEditContacts", "CanDeleteContacts",
    "CanCreateOpportunities", "CanEditOpportunities", "CanDeleteOpportunities",
    "CanCreateCustomers", "CanEditCustomers", "CanDeleteCustomers",
    "CanCreateCampaigns", "CanEditCampaigns", "CanDeleteCampaigns",
    "CanCreateServiceRequests", "CanEditServiceRequests", "CanDeleteServiceRequests",
    "CanCreateProducts", "CanEditProducts", "CanDeleteProducts",
    "DataAccessScope")
VALUES
    -- System Administrator (Id=1 reserved for SysAdmin)
    (1, 'System Administrator', 'Full system access with all permissions', true, NOW(), false,
     true, true, true, true, true, true, true, true, true, true,
     true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
     'all'),
    
    -- Sales Manager
    (2, 'Sales Manager', 'Sales team management with full sales module access', true, NOW(), false,
     true, true, true, true, true, true, true, false, true, true,
     true, true, true, true, true, true, true, true, true, true, true, false, true, true, false, true, true, false, true, true, false,
     'team'),
    
    -- Sales Representative
    (3, 'Sales Representative', 'Individual contributor for sales activities', true, NOW(), false,
     true, true, true, true, true, false, true, false, false, true,
     true, true, false, true, true, false, true, true, false, true, true, false, false, false, false, false, false, false, false, false, false,
     'own'),
    
    -- Marketing Manager
    (4, 'Marketing Manager', 'Marketing team management with campaign access', true, NOW(), false,
     true, true, true, false, true, true, true, false, false, false,
     true, true, true, true, true, true, false, false, false, true, true, false, true, true, true, false, false, false, false, false, false,
     'team'),
    
    -- Marketing Specialist
    (5, 'Marketing Specialist', 'Marketing team member', true, NOW(), false,
     true, true, true, false, true, true, true, false, false, false,
     true, true, false, true, true, false, false, false, false, false, false, false, true, true, false, false, false, false, false, false, false,
     'own'),
    
    -- Support Manager
    (6, 'Support Manager', 'Customer support team management', true, NOW(), false,
     true, false, true, false, true, false, true, false, true, true,
     false, false, false, true, true, false, false, false, false, true, true, false, false, false, false, true, true, true, false, false, false,
     'team'),
    
    -- Support Agent
    (7, 'Support Agent', 'Customer support agent', true, NOW(), false,
     true, false, true, false, true, false, false, false, true, true,
     false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, true, true, false, false, false, false,
     'own'),
    
    -- Read Only
    (8, 'Read Only', 'View-only access to all modules', true, NOW(), false,
     true, true, true, true, true, true, true, false, true, true,
     false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
     'all')
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"UserGroups_Id_seq"', (SELECT MAX("Id") FROM "UserGroups"));

-- ============================================================================
-- SECTION 4: LOOKUP CATEGORIES
-- Master categories for dropdown/picklist values
-- ============================================================================

INSERT INTO "LookupCategories" ("Id", "Name", "Description", "IsActive", "CreatedAt", "IsDeleted")
VALUES
    (1, 'LeadSource', 'Source channels for leads', true, NOW(), false),
    (2, 'LeadStatus', 'Lead lifecycle statuses', true, NOW(), false),
    (3, 'OpportunityStage', 'Sales opportunity pipeline stages', true, NOW(), false),
    (4, 'ContactSource', 'How contacts were acquired', true, NOW(), false),
    (5, 'Industry', 'Industry classification', true, NOW(), false),
    (6, 'TaskType', 'Types of tasks/activities', true, NOW(), false),
    (7, 'TaskStatus', 'Task completion statuses', true, NOW(), false),
    (8, 'Priority', 'Priority levels', true, NOW(), false),
    (9, 'QuoteStatus', 'Quote/proposal statuses', true, NOW(), false),
    (10, 'CampaignType', 'Marketing campaign types', true, NOW(), false),
    (11, 'CampaignStatus', 'Campaign lifecycle statuses', true, NOW(), false),
    (12, 'ProductCategory', 'Product/service categories', true, NOW(), false),
    (13, 'Currency', 'Supported currencies', true, NOW(), false),
    (14, 'Country', 'Countries for addresses', true, NOW(), false),
    (15, 'ContactRole', 'Role of contact within customer', true, NOW(), false),
    (16, 'PaymentTerms', 'Payment term options', true, NOW(), false),
    (17, 'CustomerLifecycleStage', 'Customer lifecycle stages', true, NOW(), false),
    (18, 'Timezone', 'IANA timezone identifiers', true, NOW(), false),
    (19, 'ServiceRequestType', 'Types of service requests', true, NOW(), false),
    (20, 'ServiceRequestStatus', 'Service request statuses', true, NOW(), false),
    (21, 'ServiceRequestPriority', 'Service request priority levels', true, NOW(), false),
    (22, 'CustomerType', 'Customer classification types', true, NOW(), false),
    (23, 'CustomerCategory', 'Individual vs Organization', true, NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"LookupCategories_Id_seq"', (SELECT MAX("Id") FROM "LookupCategories"));

-- ============================================================================
-- SECTION 5: LOOKUP ITEMS
-- Values for each lookup category
-- ============================================================================

-- Lead Source (Category 1)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(1, 'WEB', 'Website', '{"default":true}', 1, true, NOW(), false),
(1, 'REF', 'Referral', NULL, 2, true, NOW(), false),
(1, 'SOC', 'Social Media', NULL, 3, true, NOW(), false),
(1, 'EMAIL', 'Email Campaign', NULL, 4, true, NOW(), false),
(1, 'TRADE', 'Trade Show', NULL, 5, true, NOW(), false),
(1, 'COLD', 'Cold Call', NULL, 6, true, NOW(), false),
(1, 'PARTNER', 'Partner', NULL, 7, true, NOW(), false),
(1, 'PPC', 'Paid Search', NULL, 8, true, NOW(), false),
(1, 'SEO', 'Organic Search', NULL, 9, true, NOW(), false),
(1, 'WEBINAR', 'Webinar', NULL, 10, true, NOW(), false),
(1, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Lead Status (Category 2)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(2, 'NEW', 'New', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(2, 'CONTACT', 'Contacted', '{"color":"#2196f3"}', 2, true, NOW(), false),
(2, 'WORKING', 'Working', '{"color":"#03a9f4"}', 3, true, NOW(), false),
(2, 'NURTURE', 'Nurturing', '{"color":"#00bcd4"}', 4, true, NOW(), false),
(2, 'QUAL', 'Qualified', '{"color":"#4caf50"}', 5, true, NOW(), false),
(2, 'UNQUAL', 'Unqualified', '{"color":"#ff5722"}', 6, true, NOW(), false),
(2, 'CONV', 'Converted', '{"color":"#9c27b0"}', 7, true, NOW(), false),
(2, 'LOST', 'Lost', '{"color":"#f44336"}', 8, true, NOW(), false);

-- Opportunity Stage (Category 3)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(3, 'PROSPECT', 'Prospecting', '{"probability":10,"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(3, 'QUALIFY', 'Qualification', '{"probability":20,"color":"#2196f3"}', 2, true, NOW(), false),
(3, 'NEEDS', 'Needs Analysis', '{"probability":40,"color":"#03a9f4"}', 3, true, NOW(), false),
(3, 'PROPOSAL', 'Proposal', '{"probability":60,"color":"#ff9800"}', 4, true, NOW(), false),
(3, 'NEGOTIATE', 'Negotiation', '{"probability":75,"color":"#ff5722"}', 5, true, NOW(), false),
(3, 'WON', 'Closed Won', '{"probability":100,"color":"#4caf50","isClosed":true}', 6, true, NOW(), false),
(3, 'LOST', 'Closed Lost', '{"probability":0,"color":"#f44336","isClosed":true}', 7, true, NOW(), false);

-- Contact Source (Category 4)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(4, 'WEB', 'Website Form', '{"default":true}', 1, true, NOW(), false),
(4, 'IMPORT', 'Data Import', NULL, 2, true, NOW(), false),
(4, 'MANUAL', 'Manual Entry', NULL, 3, true, NOW(), false),
(4, 'API', 'API Integration', NULL, 4, true, NOW(), false),
(4, 'LEAD', 'Lead Conversion', NULL, 5, true, NOW(), false),
(4, 'REF', 'Referral', NULL, 6, true, NOW(), false),
(4, 'EVENT', 'Event', NULL, 7, true, NOW(), false);

-- Industry (Category 5)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(5, 'TECH', 'Technology', NULL, 1, true, NOW(), false),
(5, 'FIN', 'Finance & Banking', NULL, 2, true, NOW(), false),
(5, 'HEALTH', 'Healthcare', NULL, 3, true, NOW(), false),
(5, 'MFG', 'Manufacturing', NULL, 4, true, NOW(), false),
(5, 'RETAIL', 'Retail', NULL, 5, true, NOW(), false),
(5, 'EDU', 'Education', NULL, 6, true, NOW(), false),
(5, 'GOV', 'Government', NULL, 7, true, NOW(), false),
(5, 'ENERGY', 'Energy & Utilities', NULL, 8, true, NOW(), false),
(5, 'MEDIA', 'Media & Entertainment', NULL, 9, true, NOW(), false),
(5, 'TRANSP', 'Transportation & Logistics', NULL, 10, true, NOW(), false),
(5, 'REAL', 'Real Estate', NULL, 11, true, NOW(), false),
(5, 'HOSP', 'Hospitality', NULL, 12, true, NOW(), false),
(5, 'CONSTR', 'Construction', NULL, 13, true, NOW(), false),
(5, 'AGRI', 'Agriculture', NULL, 14, true, NOW(), false),
(5, 'PROF', 'Professional Services', NULL, 15, true, NOW(), false),
(5, 'NONP', 'Non-Profit', NULL, 16, true, NOW(), false),
(5, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Task Type (Category 6)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(6, 'CALL', 'Phone Call', '{"icon":"phone"}', 1, true, NOW(), false),
(6, 'EMAIL', 'Email', '{"icon":"email"}', 2, true, NOW(), false),
(6, 'MEETING', 'Meeting', '{"icon":"calendar"}', 3, true, NOW(), false),
(6, 'DEMO', 'Demo', '{"icon":"presentation"}', 4, true, NOW(), false),
(6, 'FOLLOWUP', 'Follow Up', '{"icon":"arrow-right"}', 5, true, NOW(), false),
(6, 'TODO', 'To Do', '{"icon":"check","default":true}', 6, true, NOW(), false),
(6, 'OTHER', 'Other', '{"icon":"dots"}', 99, true, NOW(), false);

-- Task Status (Category 7)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(7, 'NOT_STARTED', 'Not Started', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(7, 'IN_PROGRESS', 'In Progress', '{"color":"#2196f3"}', 2, true, NOW(), false),
(7, 'WAITING', 'Waiting on Someone Else', '{"color":"#ff9800"}', 3, true, NOW(), false),
(7, 'DEFERRED', 'Deferred', '{"color":"#795548"}', 4, true, NOW(), false),
(7, 'COMPLETED', 'Completed', '{"color":"#4caf50"}', 5, true, NOW(), false);

-- Priority (Category 8)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(8, 'LOW', 'Low', '{"color":"#4caf50"}', 1, true, NOW(), false),
(8, 'MEDIUM', 'Medium', '{"color":"#ff9800","default":true}', 2, true, NOW(), false),
(8, 'HIGH', 'High', '{"color":"#f44336"}', 3, true, NOW(), false),
(8, 'CRITICAL', 'Critical', '{"color":"#9c27b0"}', 4, true, NOW(), false);

-- Quote Status (Category 9)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(9, 'DRAFT', 'Draft', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(9, 'PENDING', 'Pending Review', '{"color":"#ff9800"}', 2, true, NOW(), false),
(9, 'APPROVED', 'Approved', '{"color":"#2196f3"}', 3, true, NOW(), false),
(9, 'SENT', 'Sent to Customer', '{"color":"#03a9f4"}', 4, true, NOW(), false),
(9, 'ACCEPTED', 'Accepted', '{"color":"#4caf50"}', 5, true, NOW(), false),
(9, 'REJECTED', 'Rejected', '{"color":"#f44336"}', 6, true, NOW(), false),
(9, 'EXPIRED', 'Expired', '{"color":"#795548"}', 7, true, NOW(), false);

-- Campaign Type (Category 10)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(10, 'EMAIL', 'Email Campaign', NULL, 1, true, NOW(), false),
(10, 'EVENT', 'Event', NULL, 2, true, NOW(), false),
(10, 'WEBINAR', 'Webinar', NULL, 3, true, NOW(), false),
(10, 'SOCIAL', 'Social Media', NULL, 4, true, NOW(), false),
(10, 'PPC', 'Paid Search', NULL, 5, true, NOW(), false),
(10, 'CONTENT', 'Content Marketing', NULL, 6, true, NOW(), false),
(10, 'DIRECT', 'Direct Mail', NULL, 7, true, NOW(), false),
(10, 'PARTNER', 'Partner Marketing', NULL, 8, true, NOW(), false),
(10, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Campaign Status (Category 11)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(11, 'PLANNED', 'Planned', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(11, 'ACTIVE', 'Active', '{"color":"#4caf50"}', 2, true, NOW(), false),
(11, 'PAUSED', 'Paused', '{"color":"#ff9800"}', 3, true, NOW(), false),
(11, 'COMPLETED', 'Completed', '{"color":"#2196f3"}', 4, true, NOW(), false),
(11, 'CANCELLED', 'Cancelled', '{"color":"#f44336"}', 5, true, NOW(), false);

-- Product Category (Category 12)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(12, 'SOFTWARE', 'Software', NULL, 1, true, NOW(), false),
(12, 'HARDWARE', 'Hardware', NULL, 2, true, NOW(), false),
(12, 'SERVICE', 'Professional Services', NULL, 3, true, NOW(), false),
(12, 'SUBSCRIPTION', 'Subscription', NULL, 4, true, NOW(), false),
(12, 'SUPPORT', 'Support & Maintenance', NULL, 5, true, NOW(), false),
(12, 'TRAINING', 'Training', NULL, 6, true, NOW(), false),
(12, 'CONSULTING', 'Consulting', NULL, 7, true, NOW(), false),
(12, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Currency (Category 13)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(13, 'USD', 'US Dollar', '{"symbol":"$","default":true}', 1, true, NOW(), false),
(13, 'EUR', 'Euro', '{"symbol":"€"}', 2, true, NOW(), false),
(13, 'GBP', 'British Pound', '{"symbol":"£"}', 3, true, NOW(), false),
(13, 'CAD', 'Canadian Dollar', '{"symbol":"CA$"}', 4, true, NOW(), false),
(13, 'AUD', 'Australian Dollar', '{"symbol":"A$"}', 5, true, NOW(), false),
(13, 'JPY', 'Japanese Yen', '{"symbol":"¥"}', 6, true, NOW(), false),
(13, 'CHF', 'Swiss Franc', '{"symbol":"CHF"}', 7, true, NOW(), false),
(13, 'INR', 'Indian Rupee', '{"symbol":"₹"}', 8, true, NOW(), false);

-- Country (Category 14) - Top 20 countries by GDP
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(14, 'US', 'United States', '{"default":true}', 1, true, NOW(), false),
(14, 'CN', 'China', NULL, 2, true, NOW(), false),
(14, 'JP', 'Japan', NULL, 3, true, NOW(), false),
(14, 'DE', 'Germany', NULL, 4, true, NOW(), false),
(14, 'GB', 'United Kingdom', NULL, 5, true, NOW(), false),
(14, 'IN', 'India', NULL, 6, true, NOW(), false),
(14, 'FR', 'France', NULL, 7, true, NOW(), false),
(14, 'IT', 'Italy', NULL, 8, true, NOW(), false),
(14, 'CA', 'Canada', NULL, 9, true, NOW(), false),
(14, 'KR', 'South Korea', NULL, 10, true, NOW(), false),
(14, 'AU', 'Australia', NULL, 11, true, NOW(), false),
(14, 'BR', 'Brazil', NULL, 12, true, NOW(), false),
(14, 'ES', 'Spain', NULL, 13, true, NOW(), false),
(14, 'MX', 'Mexico', NULL, 14, true, NOW(), false),
(14, 'NL', 'Netherlands', NULL, 15, true, NOW(), false),
(14, 'CH', 'Switzerland', NULL, 16, true, NOW(), false),
(14, 'SG', 'Singapore', NULL, 17, true, NOW(), false),
(14, 'AE', 'United Arab Emirates', NULL, 18, true, NOW(), false);

-- Contact Role (Category 15)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(15, 'DM', 'Decision Maker', '{"default":true}', 1, true, NOW(), false),
(15, 'INF', 'Influencer', NULL, 2, true, NOW(), false),
(15, 'TECH', 'Technical Evaluator', NULL, 3, true, NOW(), false),
(15, 'FIN', 'Financial Evaluator', NULL, 4, true, NOW(), false),
(15, 'USER', 'End User', NULL, 5, true, NOW(), false),
(15, 'EXEC', 'Executive Sponsor', NULL, 6, true, NOW(), false),
(15, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Payment Terms (Category 16)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(16, 'NET15', 'Net 15', '{"days":15}', 1, true, NOW(), false),
(16, 'NET30', 'Net 30', '{"days":30,"default":true}', 2, true, NOW(), false),
(16, 'NET45', 'Net 45', '{"days":45}', 3, true, NOW(), false),
(16, 'NET60', 'Net 60', '{"days":60}', 4, true, NOW(), false),
(16, 'NET90', 'Net 90', '{"days":90}', 5, true, NOW(), false),
(16, 'DUE_RECEIPT', 'Due on Receipt', '{"days":0}', 6, true, NOW(), false),
(16, 'PREPAY', 'Prepayment Required', '{"days":-1}', 7, true, NOW(), false);

-- Customer Lifecycle Stage (Category 17)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(17, 'PROSPECT', 'Prospect', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(17, 'LEAD', 'Lead', '{"color":"#2196f3"}', 2, true, NOW(), false),
(17, 'OPPORTUNITY', 'Opportunity', '{"color":"#ff9800"}', 3, true, NOW(), false),
(17, 'CUSTOMER', 'Customer', '{"color":"#4caf50"}', 4, true, NOW(), false),
(17, 'CHURNED', 'Churned', '{"color":"#f44336"}', 5, true, NOW(), false),
(17, 'REACTIVATED', 'Reactivated', '{"color":"#9c27b0"}', 6, true, NOW(), false);

-- Service Request Type (Category 19)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(19, 'SUPPORT', 'Technical Support', '{"default":true}', 1, true, NOW(), false),
(19, 'BILLING', 'Billing Inquiry', NULL, 2, true, NOW(), false),
(19, 'FEATURE', 'Feature Request', NULL, 3, true, NOW(), false),
(19, 'BUG', 'Bug Report', NULL, 4, true, NOW(), false),
(19, 'QUESTION', 'General Question', NULL, 5, true, NOW(), false),
(19, 'ACCESS', 'Access Request', NULL, 6, true, NOW(), false),
(19, 'CHANGE', 'Change Request', NULL, 7, true, NOW(), false),
(19, 'INCIDENT', 'Incident Report', NULL, 8, true, NOW(), false),
(19, 'OTHER', 'Other', NULL, 99, true, NOW(), false);

-- Service Request Status (Category 20)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(20, 'NEW', 'New', '{"color":"#9e9e9e","default":true}', 1, true, NOW(), false),
(20, 'OPEN', 'Open', '{"color":"#2196f3"}', 2, true, NOW(), false),
(20, 'PROGRESS', 'In Progress', '{"color":"#03a9f4"}', 3, true, NOW(), false),
(20, 'PENDING', 'Pending Customer', '{"color":"#ff9800"}', 4, true, NOW(), false),
(20, 'ESCALATED', 'Escalated', '{"color":"#f44336"}', 5, true, NOW(), false),
(20, 'RESOLVED', 'Resolved', '{"color":"#4caf50"}', 6, true, NOW(), false),
(20, 'CLOSED', 'Closed', '{"color":"#9c27b0"}', 7, true, NOW(), false);

-- Service Request Priority (Category 21)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(21, 'LOW', 'Low', '{"color":"#4caf50","slaHours":72}', 1, true, NOW(), false),
(21, 'MEDIUM', 'Medium', '{"color":"#ff9800","slaHours":24,"default":true}', 2, true, NOW(), false),
(21, 'HIGH', 'High', '{"color":"#f44336","slaHours":8}', 3, true, NOW(), false),
(21, 'CRITICAL', 'Critical', '{"color":"#9c27b0","slaHours":4}', 4, true, NOW(), false);

-- Customer Type (Category 22)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(22, 'PROSPECT', 'Prospect', '{"default":true}', 1, true, NOW(), false),
(22, 'SMALL', 'Small Business', NULL, 2, true, NOW(), false),
(22, 'MID', 'Mid-Market', NULL, 3, true, NOW(), false),
(22, 'ENTERPRISE', 'Enterprise', NULL, 4, true, NOW(), false),
(22, 'STRATEGIC', 'Strategic Account', NULL, 5, true, NOW(), false);

-- Customer Category (Category 23)
INSERT INTO "LookupItems" ("LookupCategoryId", "Key", "Value", "Metadata", "SortOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(23, 'INDIVIDUAL', 'Individual', NULL, 1, true, NOW(), false),
(23, 'ORGANIZATION', 'Organization', '{"default":true}', 2, true, NOW(), false);

-- ============================================================================
-- SECTION 6: DEFAULT ADMIN USER
-- Created with secure password (change after first login)
-- ============================================================================

-- Note: Password is 'Admin@123' hashed with BCrypt
-- In production, generate a new hash with: BCrypt.HashPassword("YourSecurePassword")
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FirstName", "LastName", 
    "IsActive", "UserGroupId", "DepartmentId", "CreatedAt", "IsDeleted", "EmailConfirmed", "MustChangePassword")
VALUES 
    (1, 'admin', 'admin@crm.local', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOexJujLDl1CZa4GlX2NqwONNlZwGLGHC', 
     'System', 'Administrator', true, 1, 8, NOW(), false, true, true)
ON CONFLICT ("Id") DO NOTHING;

-- Reset sequence
SELECT setval('"Users_Id_seq"', (SELECT MAX("Id") FROM "Users"));

-- ============================================================================
-- VERIFICATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Essential data verification:';
    RAISE NOTICE '  - SystemSettings: % rows', (SELECT COUNT(*) FROM "SystemSettings");
    RAISE NOTICE '  - Departments: % rows', (SELECT COUNT(*) FROM "Departments");
    RAISE NOTICE '  - UserGroups: % rows', (SELECT COUNT(*) FROM "UserGroups");
    RAISE NOTICE '  - LookupCategories: % rows', (SELECT COUNT(*) FROM "LookupCategories");
    RAISE NOTICE '  - LookupItems: % rows', (SELECT COUNT(*) FROM "LookupItems");
    RAISE NOTICE '  - Users: % rows', (SELECT COUNT(*) FROM "Users");
    RAISE NOTICE 'Essential data loading complete!';
END $$;

-- ============================================================================
-- CRM Solution - Core Seed Data (Cross-Platform)
-- ============================================================================
-- Version: 1.0
-- Date: February 1, 2026
-- Description: Essential seed data required for CRM to function
-- 
-- This file contains:
--   1. SysAdmin User Group (with full permissions)
--   2. Standard User Groups (Manager, User, etc.)
--   3. Departments
--   4. System Settings
--   5. Color Palettes
--   6. Lookup Categories and Items
--   7. Service Request Types
--   8. Module UI Configurations
--
-- Compatible with: MariaDB, MySQL, PostgreSQL, SQL Server
-- ============================================================================

-- ============================================================================
-- 1. USER GROUPS (Permission-based access control)
-- ============================================================================

-- SysAdmin Group - Full system access
INSERT INTO UserGroups (
    Name, Description, IsActive, IsDefault, IsSystemAdmin, DisplayOrder, HeaderColor,
    -- Menu Access
    CanAccessDashboard, CanAccessCustomers, CanAccessContacts, CanAccessLeads,
    CanAccessOpportunities, CanAccessProducts, CanAccessServices, CanAccessCampaigns,
    CanAccessQuotes, CanAccessTasks, CanAccessActivities, CanAccessNotes,
    CanAccessWorkflows, CanAccessServiceRequests, CanAccessReports, CanAccessSettings,
    CanAccessUserManagement,
    -- Customer Permissions
    CanCreateCustomers, CanEditCustomers, CanDeleteCustomers, CanViewAllCustomers,
    -- Contact Permissions
    CanCreateContacts, CanEditContacts, CanDeleteContacts,
    -- Lead Permissions
    CanCreateLeads, CanEditLeads, CanDeleteLeads, CanConvertLeads,
    -- Opportunity Permissions
    CanCreateOpportunities, CanEditOpportunities, CanDeleteOpportunities, CanCloseOpportunities,
    -- Product Permissions
    CanCreateProducts, CanEditProducts, CanDeleteProducts, CanManagePricing,
    -- Campaign Permissions
    CanCreateCampaigns, CanEditCampaigns, CanDeleteCampaigns, CanLaunchCampaigns,
    -- Quote Permissions
    CanCreateQuotes, CanEditQuotes, CanDeleteQuotes, CanApproveQuotes,
    -- Task Permissions
    CanCreateTasks, CanEditTasks, CanDeleteTasks, CanAssignTasks,
    -- Workflow Permissions
    CanCreateWorkflows, CanEditWorkflows, CanDeleteWorkflows, CanActivateWorkflows,
    -- Data Permissions
    DataAccessScope, CanExportData, CanImportData, CanBulkEdit, CanBulkDelete,
    AccessibleMenuItems, CreatedAt, IsDeleted
) VALUES (
    'SysAdmin', 'System Administrators with full access to all features and settings',
    1, 0, 1, 0, '#DC2626',
    -- Menu Access (all enabled)
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    -- All CRUD permissions
    1, 1, 1, 1,  -- Customers
    1, 1, 1,     -- Contacts
    1, 1, 1, 1,  -- Leads
    1, 1, 1, 1,  -- Opportunities
    1, 1, 1, 1,  -- Products
    1, 1, 1, 1,  -- Campaigns
    1, 1, 1, 1,  -- Quotes
    1, 1, 1, 1,  -- Tasks
    1, 1, 1, 1,  -- Workflows
    -- Data access
    'all', 1, 1, 1, 1,
    '["Dashboard","Customers","Contacts","Leads","Opportunities","Products","Services","Campaigns","Quotes","Tasks","Activities","Notes","Workflows","ServiceRequests","Reports","Settings","UserManagement","Admin"]',
    CURRENT_TIMESTAMP, 0
);

-- Manager Group
INSERT INTO UserGroups (
    Name, Description, IsActive, IsDefault, IsSystemAdmin, DisplayOrder, HeaderColor,
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
) VALUES (
    'Manager', 'Team managers with oversight and approval permissions',
    1, 0, 0, 1, '#2563EB',
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
    1, 1, 0, 1,
    1, 1, 0,
    1, 1, 0, 1,
    1, 1, 0, 1,
    1, 1, 0, 0,
    1, 1, 0, 1,
    1, 1, 0, 1,
    1, 1, 0, 1,
    1, 1, 0, 1,
    'team', 1, 0, 1, 0,
    CURRENT_TIMESTAMP, 0
);

-- Sales Group
INSERT INTO UserGroups (
    Name, Description, IsActive, IsDefault, IsSystemAdmin, DisplayOrder, HeaderColor,
    CanAccessDashboard, CanAccessCustomers, CanAccessContacts, CanAccessLeads,
    CanAccessOpportunities, CanAccessProducts, CanAccessServices, CanAccessCampaigns,
    CanAccessQuotes, CanAccessTasks, CanAccessActivities, CanAccessNotes,
    CanAccessWorkflows, CanAccessServiceRequests, CanAccessReports, CanAccessSettings,
    CanAccessUserManagement,
    CanCreateCustomers, CanEditCustomers, CanDeleteCustomers, CanViewAllCustomers,
    CanCreateContacts, CanEditContacts, CanDeleteContacts,
    CanCreateLeads, CanEditLeads, CanDeleteLeads, CanConvertLeads,
    CanCreateOpportunities, CanEditOpportunities, CanDeleteOpportunities, CanCloseOpportunities,
    DataAccessScope, CanExportData, CanImportData, CanBulkEdit, CanBulkDelete,
    CreatedAt, IsDeleted
) VALUES (
    'Sales', 'Sales team members',
    1, 1, 0, 2, '#059669',
    1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0,
    1, 1, 0, 0,
    1, 1, 0,
    1, 1, 0, 1,
    1, 1, 0, 1,
    'own', 1, 0, 0, 0,
    CURRENT_TIMESTAMP, 0
);

-- Support Group
INSERT INTO UserGroups (
    Name, Description, IsActive, IsDefault, IsSystemAdmin, DisplayOrder, HeaderColor,
    CanAccessDashboard, CanAccessCustomers, CanAccessContacts, CanAccessLeads,
    CanAccessOpportunities, CanAccessProducts, CanAccessServices, CanAccessCampaigns,
    CanAccessQuotes, CanAccessTasks, CanAccessActivities, CanAccessNotes,
    CanAccessWorkflows, CanAccessServiceRequests, CanAccessReports, CanAccessSettings,
    CanAccessUserManagement,
    DataAccessScope, CreatedAt, IsDeleted
) VALUES (
    'Support', 'Customer support team',
    1, 0, 0, 3, '#7C3AED',
    1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0,
    'all', CURRENT_TIMESTAMP, 0
);

-- Marketing Group
INSERT INTO UserGroups (
    Name, Description, IsActive, IsDefault, IsSystemAdmin, DisplayOrder, HeaderColor,
    CanAccessDashboard, CanAccessCustomers, CanAccessContacts, CanAccessLeads,
    CanAccessOpportunities, CanAccessProducts, CanAccessServices, CanAccessCampaigns,
    CanAccessQuotes, CanAccessTasks, CanAccessActivities, CanAccessNotes,
    CanAccessWorkflows, CanAccessServiceRequests, CanAccessReports, CanAccessSettings,
    CanAccessUserManagement,
    CanCreateCampaigns, CanEditCampaigns, CanDeleteCampaigns, CanLaunchCampaigns,
    DataAccessScope, CreatedAt, IsDeleted
) VALUES (
    'Marketing', 'Marketing team members',
    1, 0, 0, 4, '#DB2777',
    1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0,
    1, 1, 0, 1,
    'own', CURRENT_TIMESTAMP, 0
);

-- ============================================================================
-- 2. DEPARTMENTS
-- ============================================================================

INSERT INTO Departments (Name, Description, DepartmentCode, IsActive, CreatedAt, IsDeleted) VALUES
('Executive', 'Executive leadership and C-suite', 'EXEC', 1, CURRENT_TIMESTAMP, 0),
('Sales', 'Sales and business development', 'SALES', 1, CURRENT_TIMESTAMP, 0),
('Marketing', 'Marketing, branding, and campaigns', 'MKTG', 1, CURRENT_TIMESTAMP, 0),
('Customer Support', 'Customer support and service desk', 'SUPPORT', 1, CURRENT_TIMESTAMP, 0),
('Customer Success', 'Customer success and account management', 'CS', 1, CURRENT_TIMESTAMP, 0),
('Engineering', 'Product development and engineering', 'ENG', 1, CURRENT_TIMESTAMP, 0),
('Product', 'Product management and strategy', 'PROD', 1, CURRENT_TIMESTAMP, 0),
('Finance', 'Finance, accounting, and billing', 'FIN', 1, CURRENT_TIMESTAMP, 0),
('Human Resources', 'HR, recruiting, and people operations', 'HR', 1, CURRENT_TIMESTAMP, 0),
('Legal', 'Legal and compliance', 'LEGAL', 1, CURRENT_TIMESTAMP, 0),
('Operations', 'Business operations and logistics', 'OPS', 1, CURRENT_TIMESTAMP, 0),
('IT', 'Information technology and infrastructure', 'IT', 1, CURRENT_TIMESTAMP, 0),
('Quality Assurance', 'QA and testing', 'QA', 1, CURRENT_TIMESTAMP, 0),
('Research & Development', 'R&D and innovation', 'RD', 1, CURRENT_TIMESTAMP, 0),
('Procurement', 'Procurement and vendor management', 'PROC', 1, CURRENT_TIMESTAMP, 0);

-- ============================================================================
-- 3. SYSTEM SETTINGS
-- ============================================================================

INSERT INTO SystemSettings (
    Id,
    -- Module Visibility
    CustomersEnabled, ContactsEnabled, LeadsEnabled, OpportunitiesEnabled,
    ProductsEnabled, ServicesEnabled, CampaignsEnabled, QuotesEnabled,
    TasksEnabled, ActivitiesEnabled, NotesEnabled, WorkflowsEnabled,
    ReportsEnabled, DashboardEnabled, EmailEnabled, WhatsAppEnabled,
    SocialMediaEnabled, CommunicationsEnabled,
    -- Company
    CompanyName,
    -- Theme
    PrimaryColor, SecondaryColor, TertiaryColor, SurfaceColor, BackgroundColor,
    UseGroupHeaderColor, SelectedPaletteId, SelectedPaletteName,
    -- Security
    RequireTwoFactor, MinPasswordLength, SessionTimeoutMinutes,
    AllowUserRegistration, RequireApprovalForNewUsers,
    -- Features
    ShowDemoData, ApiAccessEnabled, EmailNotificationsEnabled, AuditLoggingEnabled,
    -- Localization
    DateFormat, TimeFormat, DefaultCurrency, DefaultTimezone, DefaultLanguage,
    -- SSL
    HttpsEnabled, ForceHttpsRedirect,
    -- DEPRECATED: Demo database columns removed — single database policy
    -- UseDemoDatabase, DemoDataSeeded,
    -- Statistics
    StatisticsRefreshEnabled, StatisticsRefreshIntervalMinutes,
    -- Timestamps
    CreatedAt, UpdatedAt, IsDeleted
) VALUES (
    1,
    -- All modules enabled
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    -- Company
    'CRM System',
    -- Theme (Material Purple)
    '#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#FFFBFE',
    0, 1, 'Material Purple',
    -- Security
    0, 8, 60, 1, 1,
    -- Features
    0, 1, 1, 1,
    -- Localization
    'MM/dd/yyyy', '12h', 'USD', 'America/New_York', 'en',
    -- SSL
    1, 0,
    -- DEPRECATED: Demo database values removed — single database policy
    -- 0, 0,
    -- Statistics
    1, 60,
    -- Timestamps
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 0
);

-- ============================================================================
-- 4. COLOR PALETTES
-- ============================================================================

INSERT INTO ColorPalettes (Name, Description, PrimaryColor, SecondaryColor, TertiaryColor, SurfaceColor, BackgroundColor, ErrorColor, WarningColor, SuccessColor, InfoColor, IsDefault, IsActive, DisplayOrder, CreatedAt, IsDeleted) VALUES
('Material Purple', 'Default Material Design 3 purple palette', '#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#FFFBFE', '#B3261E', '#F9A825', '#2E7D32', '#1565C0', 1, 1, 0, CURRENT_TIMESTAMP, 0),
('Ocean Blue', 'Professional blue ocean theme', '#1565C0', '#0D47A1', '#01579B', '#F5F5F5', '#FFFFFF', '#D32F2F', '#FFA000', '#388E3C', '#0288D1', 0, 1, 1, CURRENT_TIMESTAMP, 0),
('Forest Green', 'Natural green theme', '#2E7D32', '#1B5E20', '#004D40', '#F1F8E9', '#FFFFFF', '#C62828', '#F57F17', '#1565C0', '#00838F', 0, 1, 2, CURRENT_TIMESTAMP, 0),
('Sunset Orange', 'Warm orange sunset theme', '#E65100', '#BF360C', '#D84315', '#FFF3E0', '#FFFFFF', '#B71C1C', '#FBC02D', '#388E3C', '#0277BD', 0, 1, 3, CURRENT_TIMESTAMP, 0),
('Midnight Dark', 'Dark mode friendly palette', '#BB86FC', '#03DAC6', '#CF6679', '#121212', '#1E1E1E', '#CF6679', '#FFB74D', '#81C784', '#64B5F6', 0, 1, 4, CURRENT_TIMESTAMP, 0);

-- ============================================================================
-- 5. LOOKUP CATEGORIES AND ITEMS
-- ============================================================================

-- Categories
INSERT INTO LookupCategories (Name, Description, IsSystem, IsActive, DisplayOrder, CreatedAt, IsDeleted) VALUES
('Industry', 'Industry classifications for accounts', 1, 1, 1, CURRENT_TIMESTAMP, 0),
('LeadSource', 'Sources for lead generation', 1, 1, 2, CURRENT_TIMESTAMP, 0),
('AccountType', 'Types of customer accounts', 1, 1, 3, CURRENT_TIMESTAMP, 0),
('OpportunityStage', 'Sales opportunity stages', 1, 1, 4, CURRENT_TIMESTAMP, 0),
('Priority', 'Priority levels', 1, 1, 5, CURRENT_TIMESTAMP, 0),
('ContactRole', 'Contact roles within organizations', 1, 1, 6, CURRENT_TIMESTAMP, 0),
('Country', 'Countries', 1, 1, 7, CURRENT_TIMESTAMP, 0);

-- Industry Items
INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'TECH', 'Technology', 'Technology and software companies', 1, 1, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'FINANCE', 'Finance', 'Financial services and banking', 1, 2, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'HEALTHCARE', 'Healthcare', 'Healthcare and medical', 1, 3, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'RETAIL', 'Retail', 'Retail and e-commerce', 1, 4, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'MANUFACTURING', 'Manufacturing', 'Manufacturing and industrial', 1, 5, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'EDUCATION', 'Education', 'Education and training', 1, 6, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'GOVERNMENT', 'Government', 'Government and public sector', 1, 7, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'NONPROFIT', 'Non-Profit', 'Non-profit organizations', 1, 8, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Industry';

-- Lead Source Items
INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'WEB', 'Website', 'Company website', 1, 1, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'REFERRAL', 'Referral', 'Customer or partner referral', 1, 2, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'TRADE_SHOW', 'Trade Show', 'Trade show or conference', 1, 3, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'COLD_CALL', 'Cold Call', 'Outbound cold calling', 1, 4, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'LINKEDIN', 'LinkedIn', 'LinkedIn outreach', 1, 5, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'ADVERTISING', 'Advertising', 'Paid advertising', 1, 6, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'LeadSource';

-- Account Type Items
INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'PROSPECT', 'Prospect', 'Potential customer', 1, 1, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'AccountType';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'CUSTOMER', 'Customer', 'Active customer', 1, 2, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'AccountType';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'PARTNER', 'Partner', 'Business partner', 1, 3, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'AccountType';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'VENDOR', 'Vendor', 'Vendor or supplier', 1, 4, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'AccountType';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'COMPETITOR', 'Competitor', 'Competitor tracking', 1, 5, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'AccountType';

-- Opportunity Stage Items
INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'QUALIFICATION', 'Qualification', 'Initial qualification', 1, 1, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'NEEDS_ANALYSIS', 'Needs Analysis', 'Understanding requirements', 1, 2, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'PROPOSAL', 'Proposal', 'Proposal submitted', 1, 3, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'NEGOTIATION', 'Negotiation', 'Contract negotiation', 1, 4, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'CLOSED_WON', 'Closed Won', 'Deal won', 1, 5, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'CLOSED_LOST', 'Closed Lost', 'Deal lost', 1, 6, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'OpportunityStage';

-- Priority Items
INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'LOW', 'Low', 'Low priority', 1, 1, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Priority';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'MEDIUM', 'Medium', 'Medium priority', 1, 2, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Priority';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'HIGH', 'High', 'High priority', 1, 3, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Priority';

INSERT INTO LookupItems (CategoryId, Code, Name, Description, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT Id, 'CRITICAL', 'Critical', 'Critical priority', 1, 4, CURRENT_TIMESTAMP, 0
FROM LookupCategories WHERE Name = 'Priority';

-- ============================================================================
-- 6. SERVICE REQUEST CATEGORIES
-- ============================================================================

INSERT INTO ServiceRequestCategories (Name, Description, IconName, IsActive, DisplayOrder, CreatedAt, IsDeleted) VALUES
('Technical Support', 'Technical issues and troubleshooting', 'build', 1, 1, CURRENT_TIMESTAMP, 0),
('Billing', 'Billing and payment inquiries', 'payment', 1, 2, CURRENT_TIMESTAMP, 0),
('Account Management', 'Account-related requests', 'account_circle', 1, 3, CURRENT_TIMESTAMP, 0),
('Feature Request', 'New feature requests', 'lightbulb', 1, 4, CURRENT_TIMESTAMP, 0),
('General Inquiry', 'General questions and information', 'help', 1, 5, CURRENT_TIMESTAMP, 0);

-- ============================================================================
-- 7. SERVICE REQUEST TYPES
-- ============================================================================

INSERT INTO ServiceRequestTypes (Name, Description, CategoryId, DefaultPriority, DefaultSlaHours, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT 'Bug Report', 'Software bug or defect', Id, 'High', 24, 1, 1, CURRENT_TIMESTAMP, 0
FROM ServiceRequestCategories WHERE Name = 'Technical Support';

INSERT INTO ServiceRequestTypes (Name, Description, CategoryId, DefaultPriority, DefaultSlaHours, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT 'How-To Question', 'Usage questions', Id, 'Medium', 48, 1, 2, CURRENT_TIMESTAMP, 0
FROM ServiceRequestCategories WHERE Name = 'Technical Support';

INSERT INTO ServiceRequestTypes (Name, Description, CategoryId, DefaultPriority, DefaultSlaHours, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT 'Invoice Question', 'Questions about invoices', Id, 'Medium', 48, 1, 1, CURRENT_TIMESTAMP, 0
FROM ServiceRequestCategories WHERE Name = 'Billing';

INSERT INTO ServiceRequestTypes (Name, Description, CategoryId, DefaultPriority, DefaultSlaHours, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT 'Payment Issue', 'Payment processing problems', Id, 'High', 24, 1, 2, CURRENT_TIMESTAMP, 0
FROM ServiceRequestCategories WHERE Name = 'Billing';

INSERT INTO ServiceRequestTypes (Name, Description, CategoryId, DefaultPriority, DefaultSlaHours, IsActive, DisplayOrder, CreatedAt, IsDeleted)
SELECT 'Enhancement Request', 'Improvement suggestions', Id, 'Low', 168, 1, 1, CURRENT_TIMESTAMP, 0
FROM ServiceRequestCategories WHERE Name = 'Feature Request';

-- ============================================================================
-- END OF CORE SEED DATA
-- ============================================================================

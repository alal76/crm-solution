-- Core Seed Data Migration for CRM System
-- Database: SQL Server (MSSQL)
-- This script sets up the base system state with:
-- 1. SystemSettings with default branding
-- 2. Sysadmin UserGroup
-- 3. Admin user with Admin@123 password
-- 4. Core lookup categories and items

SET NOCOUNT ON;

-- =============================================
-- 1. System Settings (singleton)
-- =============================================

IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE Id = 1)
BEGIN
    INSERT INTO SystemSettings (
        Id,
        CreatedAt,
        UpdatedAt,
        IsDeleted,
        CompanyName,
        CompanyFullName,
        PrimaryColor,
        SecondaryColor,
        TertiaryColor,
        SurfaceColor,
        BackgroundColor,
        QuickAdminLoginEnabled,
        AllowUserRegistration,
        RequireApprovalForNewUsers,
        MinPasswordLength,
        SessionTimeoutMinutes,
        DefaultCurrency,
        DefaultTimezone,
        DefaultLanguage,
        DateFormat,
        TimeFormat,
        QuoteValidityDays,
        QuoteNumberPrefix,
        QuoteNumberSequence,
        DefaultTaxRate,
        -- Enable all modules by default
        CustomersEnabled,
        ContactsEnabled,
        LeadsEnabled,
        OpportunitiesEnabled,
        ProductsEnabled,
        ServicesEnabled,
        CampaignsEnabled,
        QuotesEnabled,
        TasksEnabled,
        ActivitiesEnabled,
        NotesEnabled,
        WorkflowsEnabled,
        ReportsEnabled,
        DashboardEnabled,
        EmailEnabled,
        CommunicationsEnabled,
        InteractionsEnabled
    ) VALUES (
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        0,
        'CRM System',
        'Customer Relationship Management System',
        '#6750A4',
        '#625B71',
        '#7D5260',
        '#FFFBFE',
        '#FFFBFE',
        1,  -- QuickAdminLoginEnabled (dev convenience)
        1,  -- AllowUserRegistration
        1,  -- RequireApprovalForNewUsers
        8,  -- MinPasswordLength
        60, -- SessionTimeoutMinutes
        'USD',
        'America/New_York',
        'en',
        'MM/dd/yyyy',
        '12h',
        30, -- QuoteValidityDays
        'QT-',
        1000,
        0,  -- DefaultTaxRate
        -- All modules enabled
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
    );
    PRINT 'Created default SystemSettings';
END
ELSE
BEGIN
    PRINT 'SystemSettings already exists';
END

-- =============================================
-- 2. Sysadmin UserGroup
-- =============================================

DECLARE @SysadminGroupId INT;

IF NOT EXISTS (SELECT 1 FROM UserGroups WHERE Name = 'SystemAdmin')
BEGIN
    INSERT INTO UserGroups (
        CreatedAt,
        UpdatedAt,
        IsDeleted,
        Name,
        Description,
        IsActive,
        IsSystemGroup,
        -- Full permissions
        CanManageUsers,
        CanManageSettings,
        CanManageModules,
        CanViewReports,
        CanManageWorkflows,
        CanExportData,
        CanImportData,
        CanManageLookups,
        CanManageTemplates,
        CanManageIntegrations,
        CanAccessApi,
        CanManageSecurity,
        CanManageBackups,
        CanApproveUsers
    ) VALUES (
        GETUTCDATE(),
        GETUTCDATE(),
        0,
        'SystemAdmin',
        'System Administrators with full access to all CRM features',
        1,
        1,  -- IsSystemGroup - cannot be deleted
        -- Full permissions
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
    );
    SET @SysadminGroupId = SCOPE_IDENTITY();
    PRINT 'Created SystemAdmin UserGroup';
END
ELSE
BEGIN
    SELECT @SysadminGroupId = Id FROM UserGroups WHERE Name = 'SystemAdmin';
    PRINT 'SystemAdmin UserGroup already exists';
END

-- =============================================
-- 3. Admin User (password: Admin@123)
-- =============================================

-- Password hash for 'Admin@123' using BCrypt
-- Note: This is a pre-computed BCrypt hash. In production, use proper password hashing.
DECLARE @AdminPasswordHash NVARCHAR(500) = '$2a$11$rBFZjpWqJqGFqh5QwKXKk.VhWH5XCqFB7dS4vqxLWQx7iG4bwH2Nu';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin' OR Email = 'admin@crm.local')
BEGIN
    INSERT INTO Users (
        CreatedAt,
        UpdatedAt,
        IsDeleted,
        Username,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        IsActive,
        IsApproved,
        EmailConfirmed,
        UserGroupId,
        ThemePreference,
        LanguagePreference,
        TimezonePreference,
        DateFormatPreference,
        TimeFormatPreference,
        RowsPerPagePreference,
        NotificationsEnabled,
        CompactModeEnabled
    ) VALUES (
        GETUTCDATE(),
        GETUTCDATE(),
        0,
        'admin',
        'admin@crm.local',
        @AdminPasswordHash,
        'System',
        'Administrator',
        1,
        1,
        1,
        @SysadminGroupId,
        'system',
        'en',
        'America/New_York',
        'MM/dd/yyyy',
        '12h',
        25,
        1,
        0
    );
    PRINT 'Created admin user (password: Admin@123)';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- =============================================
-- 4. Core Lookup Categories
-- =============================================

-- Contact Types
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'ContactType')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'ContactType', 'Types of contacts', 1, 1);
    
    DECLARE @ContactTypeCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Employee', 'Employee', 1, 1, 1),
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Customer', 'Customer', 2, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Partner', 'Partner', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Lead', 'Lead', 4, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Vendor', 'Vendor', 5, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @ContactTypeCatId, 'Other', 'Other', 6, 1, 0);
    PRINT 'Created ContactType lookup';
END

-- Lead Status
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'LeadStatus')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'LeadStatus', 'Lead pipeline stages', 1, 1);
    
    DECLARE @LeadStatusCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'New', 'New', 1, 1, 1),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Contacted', 'Contacted', 2, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Qualified', 'Qualified', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Proposal', 'Proposal Sent', 4, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Negotiation', 'Negotiation', 5, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Won', 'Converted (Won)', 6, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @LeadStatusCatId, 'Lost', 'Lost', 7, 1, 0);
    PRINT 'Created LeadStatus lookup';
END

-- Opportunity Stage
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'OpportunityStage')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'OpportunityStage', 'Opportunity pipeline stages', 1, 1);
    
    DECLARE @OppStageCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'Prospecting', 'Prospecting', 1, 1, 1),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'Qualification', 'Qualification', 2, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'NeedsAnalysis', 'Needs Analysis', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'ValueProposition', 'Value Proposition', 4, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'DecisionMakers', 'Decision Makers', 5, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'Proposal', 'Proposal/Quote', 6, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'Negotiation', 'Negotiation', 7, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'ClosedWon', 'Closed Won', 8, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @OppStageCatId, 'ClosedLost', 'Closed Lost', 9, 1, 0);
    PRINT 'Created OpportunityStage lookup';
END

-- Customer Category
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'CustomerCategory')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'CustomerCategory', 'Customer classification', 1, 1);
    
    DECLARE @CustCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'Enterprise', 'Enterprise', 1, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'SMB', 'Small/Medium Business', 2, 1, 1),
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'Startup', 'Startup', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'Government', 'Government', 4, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'NonProfit', 'Non-Profit', 5, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @CustCatId, 'Individual', 'Individual', 6, 1, 0);
    PRINT 'Created CustomerCategory lookup';
END

-- Service Request Priority
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'ServiceRequestPriority')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'ServiceRequestPriority', 'Service request priority levels', 1, 1);
    
    DECLARE @SRPriorityCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @SRPriorityCatId, 'Low', 'Low', 1, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @SRPriorityCatId, 'Medium', 'Medium', 2, 1, 1),
        (GETUTCDATE(), GETUTCDATE(), 0, @SRPriorityCatId, 'High', 'High', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @SRPriorityCatId, 'Critical', 'Critical', 4, 1, 0);
    PRINT 'Created ServiceRequestPriority lookup';
END

-- Industries
IF NOT EXISTS (SELECT 1 FROM LookupCategories WHERE Name = 'Industry')
BEGIN
    INSERT INTO LookupCategories (CreatedAt, UpdatedAt, IsDeleted, Name, Description, IsSystem, IsActive)
    VALUES (GETUTCDATE(), GETUTCDATE(), 0, 'Industry', 'Business industries', 1, 1);
    
    DECLARE @IndustryCatId INT = SCOPE_IDENTITY();
    
    INSERT INTO LookupItems (CreatedAt, UpdatedAt, IsDeleted, CategoryId, Value, DisplayText, SortOrder, IsActive, IsDefault)
    VALUES 
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Technology', 'Technology', 1, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Healthcare', 'Healthcare', 2, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Finance', 'Finance & Banking', 3, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Retail', 'Retail', 4, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Manufacturing', 'Manufacturing', 5, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Education', 'Education', 6, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'RealEstate', 'Real Estate', 7, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Legal', 'Legal Services', 8, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Consulting', 'Consulting', 9, 1, 0),
        (GETUTCDATE(), GETUTCDATE(), 0, @IndustryCatId, 'Other', 'Other', 10, 1, 1);
    PRINT 'Created Industry lookup';
END

PRINT '';
PRINT '===========================================';
PRINT 'Core seed data migration complete!';
PRINT 'Admin login: admin / Admin@123';
PRINT '===========================================';
GO

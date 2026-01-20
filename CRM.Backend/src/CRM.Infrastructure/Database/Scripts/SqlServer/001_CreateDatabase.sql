-- ============================================
-- CRM Database Schema for SQL Server
-- Version: 1.0.0
-- Generated: 2026-01-20
-- ============================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CrmDatabase')
BEGIN
    CREATE DATABASE CrmDatabase;
END
GO

USE CrmDatabase;
GO

-- ============================================
-- Core Tables
-- ============================================

-- Departments Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
CREATE TABLE Departments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    DepartmentCode NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    ParentDepartmentId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Departments_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id) ON DELETE NO ACTION
);
GO

-- User Profiles Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserProfiles' AND xtype='U')
CREATE TABLE UserProfiles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    DepartmentId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    AccessiblePages NVARCHAR(MAX) DEFAULT '[]',
    CanCreateCustomers BIT DEFAULT 0,
    CanEditCustomers BIT DEFAULT 0,
    CanDeleteCustomers BIT DEFAULT 0,
    CanCreateOpportunities BIT DEFAULT 0,
    CanEditOpportunities BIT DEFAULT 0,
    CanDeleteOpportunities BIT DEFAULT 0,
    CanCreateProducts BIT DEFAULT 0,
    CanEditProducts BIT DEFAULT 0,
    CanDeleteProducts BIT DEFAULT 0,
    CanManageCampaigns BIT DEFAULT 0,
    CanViewReports BIT DEFAULT 0,
    CanManageUsers BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_UserProfiles_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE CASCADE
);
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) DEFAULT '',
    LastName NVARCHAR(100) DEFAULT '',
    PasswordHash NVARCHAR(500) NOT NULL,
    Role INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    LastLoginDate DATETIME2,
    TwoFactorEnabled BIT DEFAULT 0,
    TwoFactorSecret NVARCHAR(500),
    BackupCodes NVARCHAR(MAX),
    PasswordResetToken NVARCHAR(500),
    PasswordResetTokenExpiry DATETIME2,
    EmailVerified BIT DEFAULT 0,
    EmailVerificationToken NVARCHAR(500),
    DepartmentId INT,
    UserProfileId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT UK_Users_Username UNIQUE (Username),
    CONSTRAINT UK_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Users_UserProfile FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id) ON DELETE SET NULL
);
GO

-- User Groups Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserGroups' AND xtype='U')
CREATE TABLE UserGroups (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
);
GO

-- User Group Members Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserGroupMembers' AND xtype='U')
CREATE TABLE UserGroupMembers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserGroupId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_UserGroupMembers_Group FOREIGN KEY (UserGroupId) REFERENCES UserGroups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserGroupMembers_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- User Approval Requests Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserApprovalRequests' AND xtype='U')
CREATE TABLE UserApprovalRequests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RequestedRole INT NOT NULL,
    RequestedDepartmentId INT,
    Status INT DEFAULT 0,
    RequestedAt DATETIME2 DEFAULT GETUTCDATE(),
    ReviewedAt DATETIME2,
    ReviewedByUserId INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_ApprovalRequests_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ApprovalRequests_Department FOREIGN KEY (RequestedDepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL,
    CONSTRAINT FK_ApprovalRequests_Reviewer FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- OAuth Tokens Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OAuthTokens' AND xtype='U')
CREATE TABLE OAuthTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Provider NVARCHAR(50) NOT NULL,
    AccessToken NVARCHAR(MAX) NOT NULL,
    RefreshToken NVARCHAR(MAX),
    ExpiresAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_OAuthTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- Database Backups Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DatabaseBackups' AND xtype='U')
CREATE TABLE DatabaseBackups (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT,
    BackupType NVARCHAR(50) DEFAULT 'Full',
    Status NVARCHAR(50) DEFAULT 'Completed',
    CreatedByUserId INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_DatabaseBackups_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

-- ============================================
-- CRM Entity Tables
-- ============================================

-- Customers Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    SecondaryEmail NVARCHAR(255),
    Phone NVARCHAR(20) DEFAULT '',
    MobilePhone NVARCHAR(20),
    Company NVARCHAR(255) DEFAULT '',
    JobTitle NVARCHAR(100),
    Website NVARCHAR(500),
    -- Address Information
    Address NVARCHAR(500) DEFAULT '',
    Address2 NVARCHAR(500),
    City NVARCHAR(100) DEFAULT '',
    State NVARCHAR(100) DEFAULT '',
    ZipCode NVARCHAR(20) DEFAULT '',
    Country NVARCHAR(100) DEFAULT '',
    -- Business Information
    Industry NVARCHAR(100),
    NumberOfEmployees INT,
    AnnualRevenue DECIMAL(18,2) DEFAULT 0,
    CustomerType INT DEFAULT 0,
    Priority INT DEFAULT 1,
    -- Lifecycle & Status
    LifecycleStage INT DEFAULT 0,
    LeadSource NVARCHAR(100),
    FirstContactDate DATETIME2,
    ConversionDate DATETIME2,
    LastActivityDate DATETIME2,
    NextFollowUpDate DATETIME2,
    -- Financial
    TotalPurchases DECIMAL(18,2) DEFAULT 0,
    AccountBalance DECIMAL(18,2) DEFAULT 0,
    CreditLimit DECIMAL(18,2) DEFAULT 0,
    PaymentTerms NVARCHAR(50),
    PreferredPaymentMethod NVARCHAR(50),
    -- Scoring & Rating
    LeadScore INT DEFAULT 0,
    CustomerHealthScore INT DEFAULT 50,
    NpsScore INT DEFAULT 0,
    SatisfactionRating FLOAT DEFAULT 0,
    -- Social & Communication
    LinkedInUrl NVARCHAR(500),
    TwitterHandle NVARCHAR(100),
    FacebookUrl NVARCHAR(500),
    OptInEmail BIT DEFAULT 1,
    OptInSms BIT DEFAULT 0,
    OptInPhone BIT DEFAULT 1,
    PreferredContactMethod NVARCHAR(50),
    PreferredContactTime NVARCHAR(50),
    Timezone NVARCHAR(50),
    -- Assignment & Ownership
    AssignedToUserId INT,
    AccountManagerId INT,
    Territory NVARCHAR(100),
    -- Classification
    Tags NVARCHAR(MAX),
    Segment NVARCHAR(100),
    ReferralSource NVARCHAR(100),
    ReferredByCustomerId INT,
    -- Documentation
    Notes NVARCHAR(MAX) DEFAULT '',
    InternalNotes NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT UK_Customers_Email UNIQUE (Email),
    CONSTRAINT FK_Customers_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Customers_AccountManager FOREIGN KEY (AccountManagerId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Customers_ReferredBy FOREIGN KEY (ReferredByCustomerId) REFERENCES Customers(Id) ON DELETE NO ACTION
);
GO

-- Products Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    ShortDescription NVARCHAR(500),
    SKU NVARCHAR(100) NOT NULL,
    Barcode NVARCHAR(100),
    ExternalId NVARCHAR(100),
    -- Classification
    ProductType INT DEFAULT 0,
    Status INT DEFAULT 1,
    Category NVARCHAR(100) DEFAULT '',
    SubCategory NVARCHAR(100),
    Brand NVARCHAR(100),
    Manufacturer NVARCHAR(100),
    Tags NVARCHAR(MAX),
    -- Pricing
    Price DECIMAL(18,2) DEFAULT 0,
    ListPrice DECIMAL(18,2),
    Cost DECIMAL(18,2) DEFAULT 0,
    MinimumPrice DECIMAL(18,2),
    WholesalePrice DECIMAL(18,2),
    Margin DECIMAL(18,2) DEFAULT 0,
    CurrencyCode NVARCHAR(3) DEFAULT 'USD',
    IsTaxable BIT DEFAULT 1,
    TaxRate DECIMAL(5,2),
    -- Subscription Pricing
    BillingFrequency INT DEFAULT 0,
    RecurringPrice DECIMAL(18,2),
    SetupFee DECIMAL(18,2),
    TrialPeriodDays INT,
    ContractLengthMonths INT,
    -- Inventory
    Quantity INT DEFAULT 0,
    ReorderLevel INT,
    ReorderQuantity INT,
    MaxQuantity INT,
    ReservedQuantity INT,
    AvailableQuantity INT,
    WarehouseLocation NVARCHAR(100),
    TrackInventory BIT DEFAULT 1,
    AllowBackorder BIT DEFAULT 0,
    -- Physical Attributes
    Weight DECIMAL(10,2),
    WeightUnit NVARCHAR(10) DEFAULT 'kg',
    Length DECIMAL(10,2),
    Width DECIMAL(10,2),
    Height DECIMAL(10,2),
    DimensionUnit NVARCHAR(10) DEFAULT 'cm',
    -- Media
    ImageUrl NVARCHAR(500) DEFAULT '',
    ThumbnailUrl NVARCHAR(500),
    AdditionalImages NVARCHAR(MAX),
    VideoUrl NVARCHAR(500),
    DocumentUrls NVARCHAR(MAX),
    -- SEO & Marketing
    MetaTitle NVARCHAR(100),
    MetaDescription NVARCHAR(500),
    MetaKeywords NVARCHAR(500),
    Slug NVARCHAR(200),
    -- Relationships
    ParentProductId INT,
    VendorId INT,
    ProductFamilyId INT,
    -- Features & Specifications
    Features NVARCHAR(MAX),
    Specifications NVARCHAR(MAX),
    Warranty NVARCHAR(500),
    SupportInfo NVARCHAR(MAX),
    -- Sales Information
    TotalSold INT DEFAULT 0,
    TotalRevenue DECIMAL(18,2) DEFAULT 0,
    AverageRating FLOAT DEFAULT 0,
    ReviewCount INT DEFAULT 0,
    IsFeatured BIT DEFAULT 0,
    IsBestSeller BIT DEFAULT 0,
    -- Dates
    AvailableFrom DATETIME2,
    AvailableTo DATETIME2,
    DiscontinuedDate DATETIME2,
    -- Legacy
    IsActive BIT DEFAULT 1,
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT UK_Products_SKU UNIQUE (SKU),
    CONSTRAINT FK_Products_Parent FOREIGN KEY (ParentProductId) REFERENCES Products(Id) ON DELETE NO ACTION
);
GO

-- Marketing Campaigns Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MarketingCampaigns' AND xtype='U')
CREATE TABLE MarketingCampaigns (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    Objective NVARCHAR(MAX),
    CampaignType INT DEFAULT 0,
    Status INT DEFAULT 0,
    Priority INT DEFAULT 1,
    Type NVARCHAR(50) DEFAULT '',
    -- Dates
    StartDate DATETIME2,
    EndDate DATETIME2,
    ActualStartDate DATETIME2,
    ActualEndDate DATETIME2,
    -- Budget & Costs
    Budget DECIMAL(18,2) DEFAULT 0,
    ActualCost DECIMAL(18,2) DEFAULT 0,
    ExpectedRevenue DECIMAL(18,2),
    ActualRevenue DECIMAL(18,2),
    CostPerLead DECIMAL(18,2),
    CostPerAcquisition DECIMAL(18,2),
    CurrencyCode NVARCHAR(3) DEFAULT 'USD',
    -- Target Audience
    TargetAudience INT DEFAULT 0,
    TargetAudienceDescription NVARCHAR(MAX),
    TargetDemographics NVARCHAR(MAX),
    TargetGeography NVARCHAR(500),
    TargetIndustries NVARCHAR(500),
    TargetSegments NVARCHAR(500),
    ExclusionCriteria NVARCHAR(MAX),
    -- Performance Metrics
    ConversionRate FLOAT DEFAULT 0,
    Impressions INT DEFAULT 0,
    Clicks INT DEFAULT 0,
    ClickThroughRate FLOAT DEFAULT 0,
    LeadsGenerated INT DEFAULT 0,
    OpportunitiesCreated INT DEFAULT 0,
    CustomersAcquired INT DEFAULT 0,
    ROI FLOAT DEFAULT 0,
    EmailsSent INT DEFAULT 0,
    EmailsOpened INT DEFAULT 0,
    OpenRate FLOAT DEFAULT 0,
    Bounces INT DEFAULT 0,
    Unsubscribes INT DEFAULT 0,
    -- Social Media Metrics
    SocialReach INT DEFAULT 0,
    SocialEngagement INT DEFAULT 0,
    SocialShares INT DEFAULT 0,
    SocialComments INT DEFAULT 0,
    SocialLikes INT DEFAULT 0,
    NewFollowers INT DEFAULT 0,
    -- Content
    MessageSubject NVARCHAR(500),
    MessageBody NVARCHAR(MAX),
    CallToAction NVARCHAR(500),
    LandingPageUrl NVARCHAR(500),
    TrackingUrl NVARCHAR(500),
    UtmSource NVARCHAR(100),
    UtmMedium NVARCHAR(100),
    UtmCampaign NVARCHAR(100),
    UtmContent NVARCHAR(100),
    -- A/B Testing
    IsABTest BIT DEFAULT 0,
    ABTestVariants NVARCHAR(MAX),
    WinningVariant NVARCHAR(100),
    -- Channels & Platforms
    Channels NVARCHAR(MAX),
    Platforms NVARCHAR(MAX),
    -- Assignment
    OwnerId INT,
    AssignedToUserId INT,
    TeamMembers NVARCHAR(MAX),
    -- Related
    ParentCampaignId INT,
    RelatedCampaigns NVARCHAR(MAX),
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    -- Documentation
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    Attachments NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Campaigns_Owner FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Campaigns_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Campaigns_Parent FOREIGN KEY (ParentCampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE NO ACTION
);
GO

-- Opportunities Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Opportunities' AND xtype='U')
CREATE TABLE Opportunities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    OpportunityType INT DEFAULT 0,
    Priority INT DEFAULT 1,
    -- Financial
    Amount DECIMAL(18,2) DEFAULT 0,
    ExpectedRevenue DECIMAL(18,2) DEFAULT 0,
    RecurringRevenue DECIMAL(18,2) DEFAULT 0,
    OneTimeRevenue DECIMAL(18,2) DEFAULT 0,
    Discount DECIMAL(18,2) DEFAULT 0,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    CurrencyCode NVARCHAR(3) DEFAULT 'USD',
    -- Stage & Probability
    Stage INT DEFAULT 0,
    Probability FLOAT DEFAULT 0,
    ForecastCategory INT DEFAULT 0,
    -- Dates
    CloseDate DATETIME2,
    ExpectedCloseDate DATETIME2,
    ActualCloseDate DATETIME2,
    NextStepDate DATETIME2,
    LastActivityDate DATETIME2,
    DaysInCurrentStage INT,
    TotalDaysOpen INT,
    -- Sales Process
    NextStep NVARCHAR(500),
    LossReason NVARCHAR(500),
    WinReason NVARCHAR(500),
    CompetitorName NVARCHAR(200),
    CompetitorStrengths NVARCHAR(MAX),
    CompetitorWeaknesses NVARCHAR(MAX),
    -- Decision Making
    DecisionMakers NVARCHAR(MAX),
    BudgetStatus NVARCHAR(50),
    DecisionProcess NVARCHAR(MAX),
    PainPoints NVARCHAR(MAX),
    ProposedSolution NVARCHAR(MAX),
    -- Source & Campaign
    LeadSource NVARCHAR(100),
    CampaignId INT,
    OriginalLeadId INT,
    -- Relationships
    CustomerId INT NOT NULL,
    PrimaryContactId INT,
    AssignedToUserId INT,
    ProductId INT,
    QuoteId INT,
    -- Classification
    Tags NVARCHAR(MAX),
    Territory NVARCHAR(100),
    Region NVARCHAR(100),
    -- Documentation
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Opportunities_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Opportunities_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Opportunities_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Opportunities_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL
);
GO

-- Campaign Metrics Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CampaignMetrics' AND xtype='U')
CREATE TABLE CampaignMetrics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CampaignId INT NOT NULL,
    MetricName NVARCHAR(100) NOT NULL,
    MetricValue FLOAT DEFAULT 0,
    RecordedDate DATETIME2 DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_CampaignMetrics_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE
);
GO

-- Contacts Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Contacts' AND xtype='U')
CREATE TABLE Contacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Type & Status
    ContactType INT DEFAULT 0,
    Status INT DEFAULT 0,
    LeadStatus INT,
    -- Personal Information
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100),
    Salutation NVARCHAR(20),
    Suffix NVARCHAR(20),
    Nickname NVARCHAR(50),
    Gender NVARCHAR(20),
    DateOfBirth DATETIME2,
    -- Contact Information
    EmailPrimary NVARCHAR(255),
    EmailSecondary NVARCHAR(255),
    EmailWork NVARCHAR(255),
    PhonePrimary NVARCHAR(20),
    PhoneSecondary NVARCHAR(20),
    PhoneMobile NVARCHAR(20),
    PhoneWork NVARCHAR(20),
    PhoneFax NVARCHAR(20),
    -- Address - Primary
    Address NVARCHAR(500),
    Address2 NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Country NVARCHAR(100),
    ZipCode NVARCHAR(20),
    -- Address - Mailing
    MailingAddress NVARCHAR(500),
    MailingCity NVARCHAR(100),
    MailingState NVARCHAR(100),
    MailingCountry NVARCHAR(100),
    MailingZipCode NVARCHAR(20),
    -- Professional Information
    JobTitle NVARCHAR(100),
    Department NVARCHAR(100),
    Company NVARCHAR(200),
    ReportsTo NVARCHAR(100),
    AssistantName NVARCHAR(100),
    AssistantPhone NVARCHAR(20),
    -- Lead Information
    LeadScore INT DEFAULT 0,
    LeadSource NVARCHAR(100),
    LeadRating NVARCHAR(20),
    ConvertedDate DATETIME2,
    ConvertedToCustomerId INT,
    -- Communication Preferences
    PreferredContactMethod INT DEFAULT 5,
    PreferredLanguage NVARCHAR(10) DEFAULT 'en',
    DoNotCall BIT DEFAULT 0,
    DoNotEmail BIT DEFAULT 0,
    DoNotMail BIT DEFAULT 0,
    DoNotSMS BIT DEFAULT 0,
    -- Activity Tracking
    LastContactDate DATETIME2,
    LastActivityDate DATETIME2,
    NextFollowUpDate DATETIME2,
    TotalInteractions INT DEFAULT 0,
    -- Relationships
    CustomerId INT,
    AccountId INT,
    OwnerUserId INT,
    CreatedByUserId INT,
    -- Classification
    Tags NVARCHAR(MAX),
    Categories NVARCHAR(MAX),
    -- Notes
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    DateAdded DATETIME2 DEFAULT GETUTCDATE(),
    LastModified DATETIME2,
    ModifiedBy NVARCHAR(100)
);
GO

-- Social Media Links Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SocialMediaLinks' AND xtype='U')
CREATE TABLE SocialMediaLinks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ContactId INT NOT NULL,
    Platform INT DEFAULT 0,
    Url NVARCHAR(500) NOT NULL,
    Handle NVARCHAR(100),
    DateAdded DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SocialMediaLinks_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE
);
GO

-- Interactions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Interactions' AND xtype='U')
CREATE TABLE Interactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    InteractionType INT DEFAULT 9,
    Type NVARCHAR(50) DEFAULT '',
    Direction INT DEFAULT 1,
    Subject NVARCHAR(500) DEFAULT '',
    Description NVARCHAR(MAX) DEFAULT '',
    -- Timing
    InteractionDate DATETIME2 NOT NULL,
    EndTime DATETIME2,
    DurationMinutes INT,
    ScheduledDate DATETIME2,
    CompletedDate DATETIME2,
    -- Status & Outcome
    Outcome INT DEFAULT 0,
    Sentiment INT DEFAULT 2,
    IsCompleted BIT DEFAULT 0,
    IsPrivate BIT DEFAULT 0,
    Priority INT DEFAULT 1,
    -- Communication Details
    PhoneNumber NVARCHAR(20),
    EmailAddress NVARCHAR(255),
    Location NVARCHAR(500),
    MeetingLink NVARCHAR(500),
    RecordingUrl NVARCHAR(500),
    -- Email Specific
    EmailCc NVARCHAR(MAX),
    EmailBcc NVARCHAR(MAX),
    EmailOpened BIT,
    EmailOpenedDate DATETIME2,
    EmailClicked BIT,
    EmailClickedDate DATETIME2,
    -- Relationships
    CustomerId INT NOT NULL,
    ContactId INT,
    OpportunityId INT,
    CampaignId INT,
    AssignedToUserId INT,
    CreatedByUserId INT,
    -- Follow-up
    FollowUpRequired BIT DEFAULT 0,
    FollowUpDate DATETIME2,
    FollowUpNotes NVARCHAR(MAX),
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    -- Documentation
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    Attachments NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Interactions_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Interactions_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Interactions_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Interactions_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Interactions_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Interactions_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- CRM Tasks Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CrmTasks' AND xtype='U')
CREATE TABLE CrmTasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Basic Information
    Subject NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    TaskType INT DEFAULT 8,
    Status INT DEFAULT 0,
    Priority INT DEFAULT 1,
    -- Dates
    DueDate DATETIME2,
    StartDate DATETIME2,
    CompletedDate DATETIME2,
    ReminderDate DATETIME2,
    HasReminder BIT DEFAULT 0,
    -- Progress
    PercentComplete INT DEFAULT 0,
    EstimatedMinutes INT,
    ActualMinutes INT,
    -- Recurrence
    IsRecurring BIT DEFAULT 0,
    RecurrencePattern NVARCHAR(MAX),
    RecurrenceEndDate DATETIME2,
    ParentTaskId INT,
    -- Relationships
    CustomerId INT,
    ContactId INT,
    OpportunityId INT,
    CampaignId INT,
    AssignedToUserId INT,
    CreatedByUserId INT,
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    -- Attachments
    Attachments NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_CrmTasks_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CrmTasks_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CrmTasks_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CrmTasks_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CrmTasks_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CrmTasks_Parent FOREIGN KEY (ParentTaskId) REFERENCES CrmTasks(Id) ON DELETE NO ACTION
);
GO

-- Notes Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notes' AND xtype='U')
CREATE TABLE Notes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Content
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Summary NVARCHAR(500),
    -- Classification
    NoteType INT DEFAULT 0,
    Visibility INT DEFAULT 1,
    IsPinned BIT DEFAULT 0,
    IsImportant BIT DEFAULT 0,
    -- Relationships
    CustomerId INT,
    ContactId INT,
    OpportunityId INT,
    CampaignId INT,
    ProductId INT,
    TaskId INT,
    InteractionId INT,
    -- Authorship
    CreatedByUserId INT,
    LastModifiedByUserId INT,
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    -- Attachments
    Attachments NVARCHAR(MAX),
    MentionedUsers NVARCHAR(MAX),
    RelatedNotes NVARCHAR(MAX),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Notes_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notes_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Notes_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Notes_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Notes_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Notes_ModifiedBy FOREIGN KEY (LastModifiedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- Quotes Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Quotes' AND xtype='U')
CREATE TABLE Quotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Identification
    QuoteNumber NVARCHAR(50) NOT NULL,
    ExternalQuoteId NVARCHAR(100),
    Version INT DEFAULT 1,
    -- Basic Information
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Status INT DEFAULT 0,
    -- Dates
    QuoteDate DATETIME2 DEFAULT GETUTCDATE(),
    ExpirationDate DATETIME2,
    SentDate DATETIME2,
    ViewedDate DATETIME2,
    AcceptedDate DATETIME2,
    RejectedDate DATETIME2,
    -- Pricing
    Subtotal DECIMAL(18,2) DEFAULT 0,
    Discount DECIMAL(18,2) DEFAULT 0,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    DiscountReason NVARCHAR(500),
    Tax DECIMAL(18,2) DEFAULT 0,
    TaxRate DECIMAL(5,2) DEFAULT 0,
    ShippingCost DECIMAL(18,2) DEFAULT 0,
    Total DECIMAL(18,2) DEFAULT 0,
    CurrencyCode NVARCHAR(3) DEFAULT 'USD',
    -- Terms
    PaymentTerms NVARCHAR(100),
    DeliveryTerms NVARCHAR(100),
    TermsAndConditions NVARCHAR(MAX),
    Warranty NVARCHAR(500),
    ValidityDays INT DEFAULT 30,
    -- Line Items
    LineItems NVARCHAR(MAX),
    -- Billing Address
    BillingName NVARCHAR(200),
    BillingAddress NVARCHAR(500),
    BillingCity NVARCHAR(100),
    BillingState NVARCHAR(100),
    BillingZipCode NVARCHAR(20),
    BillingCountry NVARCHAR(100),
    -- Shipping Address
    ShippingName NVARCHAR(200),
    ShippingAddress NVARCHAR(500),
    ShippingCity NVARCHAR(100),
    ShippingState NVARCHAR(100),
    ShippingZipCode NVARCHAR(20),
    ShippingCountry NVARCHAR(100),
    -- Contact Information
    ContactName NVARCHAR(200),
    ContactEmail NVARCHAR(255),
    ContactPhone NVARCHAR(20),
    -- Relationships
    CustomerId INT,
    ContactId INT,
    OpportunityId INT,
    AssignedToUserId INT,
    CreatedByUserId INT,
    ApprovedByUserId INT,
    ParentQuoteId INT,
    -- Approval
    RequiresApproval BIT DEFAULT 0,
    IsApproved BIT DEFAULT 0,
    ApprovalDate DATETIME2,
    ApprovalNotes NVARCHAR(MAX),
    -- Signature
    IsSigned BIT DEFAULT 0,
    SignedDate DATETIME2,
    SignedBy NVARCHAR(200),
    SignatureUrl NVARCHAR(500),
    -- Documentation
    Notes NVARCHAR(MAX),
    InternalNotes NVARCHAR(MAX),
    Attachments NVARCHAR(MAX),
    QuotePdfUrl NVARCHAR(500),
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT UK_Quotes_Number UNIQUE (QuoteNumber),
    CONSTRAINT FK_Quotes_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Quotes_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Quotes_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Quotes_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Quotes_ApprovedBy FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Quotes_Parent FOREIGN KEY (ParentQuoteId) REFERENCES Quotes(Id) ON DELETE NO ACTION
);
GO

-- Activities Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Activities' AND xtype='U')
CREATE TABLE Activities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    -- Activity Information
    ActivityType INT DEFAULT 99,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Details NVARCHAR(MAX),
    -- Timing
    ActivityDate DATETIME2 DEFAULT GETUTCDATE(),
    DurationMinutes INT,
    -- Actor
    UserId INT,
    UserName NVARCHAR(200),
    UserEmail NVARCHAR(255),
    -- Related Entity
    EntityType NVARCHAR(50),
    EntityId INT,
    EntityName NVARCHAR(255),
    -- Related Records
    CustomerId INT,
    ContactId INT,
    OpportunityId INT,
    CampaignId INT,
    ProductId INT,
    -- Metadata
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    DeviceType NVARCHAR(50),
    Source NVARCHAR(100),
    -- Classification
    Tags NVARCHAR(MAX),
    Category NVARCHAR(100),
    IsSystemGenerated BIT DEFAULT 0,
    IsVisible BIT DEFAULT 1,
    CustomFields NVARCHAR(MAX),
    -- Audit
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Activities_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Activities_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Activities_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE NO ACTION
);
GO

-- Workflows Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Workflows' AND xtype='U')
CREATE TABLE Workflows (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    EntityType NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    Priority INT DEFAULT 100,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
);
GO

-- Workflow Rules Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkflowRules' AND xtype='U')
CREATE TABLE WorkflowRules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) DEFAULT '',
    ConditionLogic NVARCHAR(10) DEFAULT 'AND',
    ActionType NVARCHAR(50) NOT NULL,
    ActionConfig NVARCHAR(MAX),
    TargetUserGroupId INT,
    IsActive BIT DEFAULT 1,
    Priority INT DEFAULT 100,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_WorkflowRules_Workflow FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowRules_TargetGroup FOREIGN KEY (TargetUserGroupId) REFERENCES UserGroups(Id) ON DELETE NO ACTION
);
GO

-- Workflow Rule Conditions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkflowRuleConditions' AND xtype='U')
CREATE TABLE WorkflowRuleConditions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowRuleId INT NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    Operator NVARCHAR(20) NOT NULL,
    Value NVARCHAR(MAX),
    ValueType NVARCHAR(50) DEFAULT 'String',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_WorkflowRuleConditions_Rule FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id) ON DELETE CASCADE
);
GO

-- Workflow Executions Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkflowExecutions' AND xtype='U')
CREATE TABLE WorkflowExecutions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    WorkflowRuleId INT,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Success',
    Message NVARCHAR(MAX),
    SourceUserGroupId INT,
    TargetUserGroupId INT,
    ExecutedByUserId INT,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_WorkflowExecutions_Workflow FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowExecutions_Rule FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WorkflowExecutions_SourceGroup FOREIGN KEY (SourceUserGroupId) REFERENCES UserGroups(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WorkflowExecutions_TargetGroup FOREIGN KEY (TargetUserGroupId) REFERENCES UserGroups(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WorkflowExecutions_User FOREIGN KEY (ExecutedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
);
GO

-- ============================================
-- Indexes for Performance
-- ============================================

CREATE INDEX idx_customers_email ON Customers(Email);
CREATE INDEX idx_customers_company ON Customers(Company);
CREATE INDEX idx_customers_lifecycle ON Customers(LifecycleStage);
CREATE INDEX idx_customers_assigned ON Customers(AssignedToUserId);

CREATE INDEX idx_products_sku ON Products(SKU);
CREATE INDEX idx_products_category ON Products(Category);
CREATE INDEX idx_products_status ON Products(Status);

CREATE INDEX idx_opportunities_customer ON Opportunities(CustomerId);
CREATE INDEX idx_opportunities_stage ON Opportunities(Stage);
CREATE INDEX idx_opportunities_close_date ON Opportunities(CloseDate);
CREATE INDEX idx_opportunities_assigned ON Opportunities(AssignedToUserId);

CREATE INDEX idx_campaigns_status ON MarketingCampaigns(Status);
CREATE INDEX idx_campaigns_type ON MarketingCampaigns(CampaignType);
CREATE INDEX idx_campaigns_dates ON MarketingCampaigns(StartDate, EndDate);

CREATE INDEX idx_interactions_customer ON Interactions(CustomerId);
CREATE INDEX idx_interactions_date ON Interactions(InteractionDate);
CREATE INDEX idx_interactions_type ON Interactions(InteractionType);

CREATE INDEX idx_contacts_type ON Contacts(ContactType);
CREATE INDEX idx_contacts_company ON Contacts(Company);
CREATE INDEX idx_contacts_email ON Contacts(EmailPrimary);

CREATE INDEX idx_tasks_due_date ON CrmTasks(DueDate);
CREATE INDEX idx_tasks_status ON CrmTasks(Status);
CREATE INDEX idx_tasks_assigned ON CrmTasks(AssignedToUserId);

CREATE INDEX idx_notes_pinned ON Notes(IsPinned);
CREATE INDEX idx_notes_customer ON Notes(CustomerId);

CREATE INDEX idx_quotes_number ON Quotes(QuoteNumber);
CREATE INDEX idx_quotes_status ON Quotes(Status);
CREATE INDEX idx_quotes_customer ON Quotes(CustomerId);

CREATE INDEX idx_activities_date ON Activities(ActivityDate);
CREATE INDEX idx_activities_type ON Activities(ActivityType);
CREATE INDEX idx_activities_entity ON Activities(EntityType, EntityId);

CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_department ON Users(DepartmentId);

CREATE INDEX idx_workflow_executions_workflow ON WorkflowExecutions(WorkflowId, CreatedAt);
CREATE INDEX idx_workflow_executions_entity ON WorkflowExecutions(EntityType, EntityId);
GO

-- ============================================
-- Schema complete
-- ============================================
PRINT 'CRM Database schema created successfully for SQL Server';
GO

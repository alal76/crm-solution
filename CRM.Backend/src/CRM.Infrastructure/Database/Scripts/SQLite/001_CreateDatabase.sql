-- ============================================
-- CRM Database Schema for SQLite
-- Version: 1.0.0
-- Generated: 2026-01-20
-- ============================================

-- Enable foreign keys
PRAGMA foreign_keys = ON;

-- ============================================
-- Core Tables
-- ============================================

-- Departments Table
CREATE TABLE IF NOT EXISTS Departments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    DepartmentCode TEXT,
    IsActive INTEGER DEFAULT 1,
    ParentDepartmentId INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL
);

-- User Profiles Table
CREATE TABLE IF NOT EXISTS UserProfiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    DepartmentId INTEGER NOT NULL,
    IsActive INTEGER DEFAULT 1,
    AccessiblePages TEXT DEFAULT '[]',
    CanCreateCustomers INTEGER DEFAULT 0,
    CanEditCustomers INTEGER DEFAULT 0,
    CanDeleteCustomers INTEGER DEFAULT 0,
    CanCreateOpportunities INTEGER DEFAULT 0,
    CanEditOpportunities INTEGER DEFAULT 0,
    CanDeleteOpportunities INTEGER DEFAULT 0,
    CanCreateProducts INTEGER DEFAULT 0,
    CanEditProducts INTEGER DEFAULT 0,
    CanDeleteProducts INTEGER DEFAULT 0,
    CanManageCampaigns INTEGER DEFAULT 0,
    CanViewReports INTEGER DEFAULT 0,
    CanManageUsers INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE CASCADE
);

-- Users Table
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    FirstName TEXT DEFAULT '',
    LastName TEXT DEFAULT '',
    PasswordHash TEXT NOT NULL,
    Role INTEGER DEFAULT 0,
    IsActive INTEGER DEFAULT 1,
    LastLoginDate TEXT,
    TwoFactorEnabled INTEGER DEFAULT 0,
    TwoFactorSecret TEXT,
    BackupCodes TEXT,
    PasswordResetToken TEXT,
    PasswordResetTokenExpiry TEXT,
    EmailVerified INTEGER DEFAULT 0,
    EmailVerificationToken TEXT,
    DepartmentId INTEGER,
    UserProfileId INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL,
    FOREIGN KEY (UserProfileId) REFERENCES UserProfiles(Id) ON DELETE SET NULL
);

-- User Groups Table
CREATE TABLE IF NOT EXISTS UserGroups (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0
);

-- User Group Members Table
CREATE TABLE IF NOT EXISTS UserGroupMembers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserGroupId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (UserGroupId) REFERENCES UserGroups(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- User Approval Requests Table
CREATE TABLE IF NOT EXISTS UserApprovalRequests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    RequestedRole INTEGER NOT NULL,
    RequestedDepartmentId INTEGER,
    Status INTEGER DEFAULT 0,
    RequestedAt TEXT DEFAULT (datetime('now')),
    ReviewedAt TEXT,
    ReviewedByUserId INTEGER,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RequestedDepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL,
    FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- OAuth Tokens Table
CREATE TABLE IF NOT EXISTS OAuthTokens (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    Provider TEXT NOT NULL,
    AccessToken TEXT NOT NULL,
    RefreshToken TEXT,
    ExpiresAt TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Database Backups Table
CREATE TABLE IF NOT EXISTS DatabaseBackups (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    FileSize INTEGER,
    BackupType TEXT DEFAULT 'Full',
    Status TEXT DEFAULT 'Completed',
    CreatedByUserId INTEGER,
    Notes TEXT,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ============================================
-- CRM Entity Tables
-- ============================================

-- Customers Table
CREATE TABLE IF NOT EXISTS Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    SecondaryEmail TEXT,
    Phone TEXT DEFAULT '',
    MobilePhone TEXT,
    Company TEXT DEFAULT '',
    JobTitle TEXT,
    Website TEXT,
    -- Address Information
    Address TEXT DEFAULT '',
    Address2 TEXT,
    City TEXT DEFAULT '',
    State TEXT DEFAULT '',
    ZipCode TEXT DEFAULT '',
    Country TEXT DEFAULT '',
    -- Business Information
    Industry TEXT,
    NumberOfEmployees INTEGER,
    AnnualRevenue REAL DEFAULT 0,
    CustomerType INTEGER DEFAULT 0,
    Priority INTEGER DEFAULT 1,
    -- Lifecycle & Status
    LifecycleStage INTEGER DEFAULT 0,
    LeadSource TEXT,
    FirstContactDate TEXT,
    ConversionDate TEXT,
    LastActivityDate TEXT,
    NextFollowUpDate TEXT,
    -- Financial
    TotalPurchases REAL DEFAULT 0,
    AccountBalance REAL DEFAULT 0,
    CreditLimit REAL DEFAULT 0,
    PaymentTerms TEXT,
    PreferredPaymentMethod TEXT,
    -- Scoring & Rating
    LeadScore INTEGER DEFAULT 0,
    CustomerHealthScore INTEGER DEFAULT 50,
    NpsScore INTEGER DEFAULT 0,
    SatisfactionRating REAL DEFAULT 0,
    -- Social & Communication
    LinkedInUrl TEXT,
    TwitterHandle TEXT,
    FacebookUrl TEXT,
    OptInEmail INTEGER DEFAULT 1,
    OptInSms INTEGER DEFAULT 0,
    OptInPhone INTEGER DEFAULT 1,
    PreferredContactMethod TEXT,
    PreferredContactTime TEXT,
    Timezone TEXT,
    -- Assignment & Ownership
    AssignedToUserId INTEGER,
    AccountManagerId INTEGER,
    Territory TEXT,
    -- Classification
    Tags TEXT,
    Segment TEXT,
    ReferralSource TEXT,
    ReferredByCustomerId INTEGER,
    -- Documentation
    Notes TEXT DEFAULT '',
    InternalNotes TEXT,
    Description TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (AccountManagerId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ReferredByCustomerId) REFERENCES Customers(Id) ON DELETE SET NULL
);

-- Products Table
CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    ShortDescription TEXT,
    SKU TEXT NOT NULL UNIQUE,
    Barcode TEXT,
    ExternalId TEXT,
    -- Classification
    ProductType INTEGER DEFAULT 0,
    Status INTEGER DEFAULT 1,
    Category TEXT DEFAULT '',
    SubCategory TEXT,
    Brand TEXT,
    Manufacturer TEXT,
    Tags TEXT,
    -- Pricing
    Price REAL DEFAULT 0,
    ListPrice REAL,
    Cost REAL DEFAULT 0,
    MinimumPrice REAL,
    WholesalePrice REAL,
    Margin REAL DEFAULT 0,
    CurrencyCode TEXT DEFAULT 'USD',
    IsTaxable INTEGER DEFAULT 1,
    TaxRate REAL,
    -- Subscription Pricing
    BillingFrequency INTEGER DEFAULT 0,
    RecurringPrice REAL,
    SetupFee REAL,
    TrialPeriodDays INTEGER,
    ContractLengthMonths INTEGER,
    -- Inventory
    Quantity INTEGER DEFAULT 0,
    ReorderLevel INTEGER,
    ReorderQuantity INTEGER,
    MaxQuantity INTEGER,
    ReservedQuantity INTEGER,
    AvailableQuantity INTEGER,
    WarehouseLocation TEXT,
    TrackInventory INTEGER DEFAULT 1,
    AllowBackorder INTEGER DEFAULT 0,
    -- Physical Attributes
    Weight REAL,
    WeightUnit TEXT DEFAULT 'kg',
    Length REAL,
    Width REAL,
    Height REAL,
    DimensionUnit TEXT DEFAULT 'cm',
    -- Media
    ImageUrl TEXT DEFAULT '',
    ThumbnailUrl TEXT,
    AdditionalImages TEXT,
    VideoUrl TEXT,
    DocumentUrls TEXT,
    -- SEO & Marketing
    MetaTitle TEXT,
    MetaDescription TEXT,
    MetaKeywords TEXT,
    Slug TEXT,
    -- Relationships
    ParentProductId INTEGER,
    VendorId INTEGER,
    ProductFamilyId INTEGER,
    -- Features & Specifications
    Features TEXT,
    Specifications TEXT,
    Warranty TEXT,
    SupportInfo TEXT,
    -- Sales Information
    TotalSold INTEGER DEFAULT 0,
    TotalRevenue REAL DEFAULT 0,
    AverageRating REAL DEFAULT 0,
    ReviewCount INTEGER DEFAULT 0,
    IsFeatured INTEGER DEFAULT 0,
    IsBestSeller INTEGER DEFAULT 0,
    -- Dates
    AvailableFrom TEXT,
    AvailableTo TEXT,
    DiscontinuedDate TEXT,
    -- Legacy
    IsActive INTEGER DEFAULT 1,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (ParentProductId) REFERENCES Products(Id) ON DELETE SET NULL
);

-- Opportunities Table
CREATE TABLE IF NOT EXISTS Opportunities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    OpportunityType INTEGER DEFAULT 0,
    Priority INTEGER DEFAULT 1,
    -- Financial
    Amount REAL DEFAULT 0,
    ExpectedRevenue REAL DEFAULT 0,
    RecurringRevenue REAL DEFAULT 0,
    OneTimeRevenue REAL DEFAULT 0,
    Discount REAL DEFAULT 0,
    DiscountPercent REAL DEFAULT 0,
    CurrencyCode TEXT DEFAULT 'USD',
    -- Stage & Probability
    Stage INTEGER DEFAULT 0,
    Probability REAL DEFAULT 0,
    ForecastCategory INTEGER DEFAULT 0,
    -- Dates
    CloseDate TEXT,
    ExpectedCloseDate TEXT,
    ActualCloseDate TEXT,
    NextStepDate TEXT,
    LastActivityDate TEXT,
    DaysInCurrentStage INTEGER,
    TotalDaysOpen INTEGER,
    -- Sales Process
    NextStep TEXT,
    LossReason TEXT,
    WinReason TEXT,
    CompetitorName TEXT,
    CompetitorStrengths TEXT,
    CompetitorWeaknesses TEXT,
    -- Decision Making
    DecisionMakers TEXT,
    BudgetStatus TEXT,
    DecisionProcess TEXT,
    PainPoints TEXT,
    ProposedSolution TEXT,
    -- Source & Campaign
    LeadSource TEXT,
    CampaignId INTEGER,
    OriginalLeadId INTEGER,
    -- Relationships
    CustomerId INTEGER NOT NULL,
    PrimaryContactId INTEGER,
    AssignedToUserId INTEGER,
    ProductId INTEGER,
    QuoteId INTEGER,
    -- Classification
    Tags TEXT,
    Territory TEXT,
    Region TEXT,
    -- Documentation
    Notes TEXT,
    InternalNotes TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL
);

-- Marketing Campaigns Table
CREATE TABLE IF NOT EXISTS MarketingCampaigns (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    Objective TEXT,
    CampaignType INTEGER DEFAULT 0,
    Status INTEGER DEFAULT 0,
    Priority INTEGER DEFAULT 1,
    Type TEXT DEFAULT '',
    -- Dates
    StartDate TEXT,
    EndDate TEXT,
    ActualStartDate TEXT,
    ActualEndDate TEXT,
    -- Budget & Costs
    Budget REAL DEFAULT 0,
    ActualCost REAL DEFAULT 0,
    ExpectedRevenue REAL,
    ActualRevenue REAL,
    CostPerLead REAL,
    CostPerAcquisition REAL,
    CurrencyCode TEXT DEFAULT 'USD',
    -- Target Audience
    TargetAudience INTEGER DEFAULT 0,
    TargetAudienceDescription TEXT,
    TargetDemographics TEXT,
    TargetGeography TEXT,
    TargetIndustries TEXT,
    TargetSegments TEXT,
    ExclusionCriteria TEXT,
    -- Performance Metrics
    ConversionRate REAL DEFAULT 0,
    Impressions INTEGER DEFAULT 0,
    Clicks INTEGER DEFAULT 0,
    ClickThroughRate REAL DEFAULT 0,
    LeadsGenerated INTEGER DEFAULT 0,
    OpportunitiesCreated INTEGER DEFAULT 0,
    CustomersAcquired INTEGER DEFAULT 0,
    ROI REAL DEFAULT 0,
    EmailsSent INTEGER DEFAULT 0,
    EmailsOpened INTEGER DEFAULT 0,
    OpenRate REAL DEFAULT 0,
    Bounces INTEGER DEFAULT 0,
    Unsubscribes INTEGER DEFAULT 0,
    -- Social Media Metrics
    SocialReach INTEGER DEFAULT 0,
    SocialEngagement INTEGER DEFAULT 0,
    SocialShares INTEGER DEFAULT 0,
    SocialComments INTEGER DEFAULT 0,
    SocialLikes INTEGER DEFAULT 0,
    NewFollowers INTEGER DEFAULT 0,
    -- Content
    MessageSubject TEXT,
    MessageBody TEXT,
    CallToAction TEXT,
    LandingPageUrl TEXT,
    TrackingUrl TEXT,
    UtmSource TEXT,
    UtmMedium TEXT,
    UtmCampaign TEXT,
    UtmContent TEXT,
    -- A/B Testing
    IsABTest INTEGER DEFAULT 0,
    ABTestVariants TEXT,
    WinningVariant TEXT,
    -- Channels & Platforms
    Channels TEXT,
    Platforms TEXT,
    -- Assignment
    OwnerId INTEGER,
    AssignedToUserId INTEGER,
    TeamMembers TEXT,
    -- Related
    ParentCampaignId INTEGER,
    RelatedCampaigns TEXT,
    -- Classification
    Tags TEXT,
    Category TEXT,
    -- Documentation
    Notes TEXT,
    InternalNotes TEXT,
    Attachments TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ParentCampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL
);

-- Campaign Metrics Table
CREATE TABLE IF NOT EXISTS CampaignMetrics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CampaignId INTEGER NOT NULL,
    MetricName TEXT NOT NULL,
    MetricValue REAL DEFAULT 0,
    RecordedDate TEXT DEFAULT (datetime('now')),
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE
);

-- Interactions Table
CREATE TABLE IF NOT EXISTS Interactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    InteractionType INTEGER DEFAULT 9,
    Type TEXT DEFAULT '',
    Direction INTEGER DEFAULT 1,
    Subject TEXT DEFAULT '',
    Description TEXT DEFAULT '',
    -- Timing
    InteractionDate TEXT NOT NULL,
    EndTime TEXT,
    DurationMinutes INTEGER,
    ScheduledDate TEXT,
    CompletedDate TEXT,
    -- Status & Outcome
    Outcome INTEGER DEFAULT 0,
    Sentiment INTEGER DEFAULT 2,
    IsCompleted INTEGER DEFAULT 0,
    IsPrivate INTEGER DEFAULT 0,
    Priority INTEGER DEFAULT 1,
    -- Communication Details
    PhoneNumber TEXT,
    EmailAddress TEXT,
    Location TEXT,
    MeetingLink TEXT,
    RecordingUrl TEXT,
    -- Email Specific
    EmailCc TEXT,
    EmailBcc TEXT,
    EmailOpened INTEGER,
    EmailOpenedDate TEXT,
    EmailClicked INTEGER,
    EmailClickedDate TEXT,
    -- Relationships
    CustomerId INTEGER NOT NULL,
    ContactId INTEGER,
    OpportunityId INTEGER,
    CampaignId INTEGER,
    AssignedToUserId INTEGER,
    CreatedByUserId INTEGER,
    -- Follow-up
    FollowUpRequired INTEGER DEFAULT 0,
    FollowUpDate TEXT,
    FollowUpNotes TEXT,
    -- Classification
    Tags TEXT,
    Category TEXT,
    -- Documentation
    Notes TEXT,
    InternalNotes TEXT,
    Attachments TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE SET NULL,
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE SET NULL,
    FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Contacts Table
CREATE TABLE IF NOT EXISTS Contacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Type & Status
    ContactType INTEGER DEFAULT 0,
    Status INTEGER DEFAULT 0,
    LeadStatus INTEGER,
    -- Personal Information
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    MiddleName TEXT,
    Salutation TEXT,
    Suffix TEXT,
    Nickname TEXT,
    Gender TEXT,
    DateOfBirth TEXT,
    -- Contact Information
    EmailPrimary TEXT,
    EmailSecondary TEXT,
    EmailWork TEXT,
    PhonePrimary TEXT,
    PhoneSecondary TEXT,
    PhoneMobile TEXT,
    PhoneWork TEXT,
    PhoneFax TEXT,
    -- Address - Primary
    Address TEXT,
    Address2 TEXT,
    City TEXT,
    State TEXT,
    Country TEXT,
    ZipCode TEXT,
    -- Address - Mailing
    MailingAddress TEXT,
    MailingCity TEXT,
    MailingState TEXT,
    MailingCountry TEXT,
    MailingZipCode TEXT,
    -- Professional Information
    JobTitle TEXT,
    Department TEXT,
    Company TEXT,
    ReportsTo TEXT,
    AssistantName TEXT,
    AssistantPhone TEXT,
    -- Lead Information
    LeadScore INTEGER DEFAULT 0,
    LeadSource TEXT,
    LeadRating TEXT,
    ConvertedDate TEXT,
    ConvertedToCustomerId INTEGER,
    -- Communication Preferences
    PreferredContactMethod INTEGER DEFAULT 5,
    PreferredLanguage TEXT DEFAULT 'en',
    DoNotCall INTEGER DEFAULT 0,
    DoNotEmail INTEGER DEFAULT 0,
    DoNotMail INTEGER DEFAULT 0,
    DoNotSMS INTEGER DEFAULT 0,
    -- Activity Tracking
    LastContactDate TEXT,
    LastActivityDate TEXT,
    NextFollowUpDate TEXT,
    TotalInteractions INTEGER DEFAULT 0,
    -- Relationships
    CustomerId INTEGER,
    AccountId INTEGER,
    OwnerUserId INTEGER,
    CreatedByUserId INTEGER,
    -- Classification
    Tags TEXT,
    Categories TEXT,
    -- Notes
    Notes TEXT,
    InternalNotes TEXT,
    CustomFields TEXT,
    -- Audit
    DateAdded TEXT DEFAULT (datetime('now')),
    LastModified TEXT,
    ModifiedBy TEXT
);

-- Social Media Links Table
CREATE TABLE IF NOT EXISTS SocialMediaLinks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ContactId INTEGER NOT NULL,
    Platform INTEGER DEFAULT 0,
    Url TEXT NOT NULL,
    Handle TEXT,
    DateAdded TEXT DEFAULT (datetime('now')),
    FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE
);

-- CRM Tasks Table
CREATE TABLE IF NOT EXISTS CrmTasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Basic Information
    Subject TEXT NOT NULL,
    Description TEXT,
    TaskType INTEGER DEFAULT 8,
    Status INTEGER DEFAULT 0,
    Priority INTEGER DEFAULT 1,
    -- Dates
    DueDate TEXT,
    StartDate TEXT,
    CompletedDate TEXT,
    ReminderDate TEXT,
    HasReminder INTEGER DEFAULT 0,
    -- Progress
    PercentComplete INTEGER DEFAULT 0,
    EstimatedMinutes INTEGER,
    ActualMinutes INTEGER,
    -- Recurrence
    IsRecurring INTEGER DEFAULT 0,
    RecurrencePattern TEXT,
    RecurrenceEndDate TEXT,
    ParentTaskId INTEGER,
    -- Relationships
    CustomerId INTEGER,
    ContactId INTEGER,
    OpportunityId INTEGER,
    CampaignId INTEGER,
    AssignedToUserId INTEGER,
    CreatedByUserId INTEGER,
    -- Classification
    Tags TEXT,
    Category TEXT,
    -- Attachments
    Attachments TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE SET NULL,
    FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ParentTaskId) REFERENCES CrmTasks(Id) ON DELETE SET NULL
);

-- Notes Table
CREATE TABLE IF NOT EXISTS Notes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Content
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    Summary TEXT,
    -- Classification
    NoteType INTEGER DEFAULT 0,
    Visibility INTEGER DEFAULT 1,
    IsPinned INTEGER DEFAULT 0,
    IsImportant INTEGER DEFAULT 0,
    -- Relationships
    CustomerId INTEGER,
    ContactId INTEGER,
    OpportunityId INTEGER,
    CampaignId INTEGER,
    ProductId INTEGER,
    TaskId INTEGER,
    InteractionId INTEGER,
    -- Authorship
    CreatedByUserId INTEGER,
    LastModifiedByUserId INTEGER,
    -- Classification
    Tags TEXT,
    Category TEXT,
    -- Attachments
    Attachments TEXT,
    MentionedUsers TEXT,
    RelatedNotes TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE CASCADE,
    FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (LastModifiedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Quotes Table
CREATE TABLE IF NOT EXISTS Quotes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Identification
    QuoteNumber TEXT NOT NULL UNIQUE,
    ExternalQuoteId TEXT,
    Version INTEGER DEFAULT 1,
    -- Basic Information
    Name TEXT NOT NULL,
    Description TEXT,
    Status INTEGER DEFAULT 0,
    -- Dates
    QuoteDate TEXT DEFAULT (datetime('now')),
    ExpirationDate TEXT,
    SentDate TEXT,
    ViewedDate TEXT,
    AcceptedDate TEXT,
    RejectedDate TEXT,
    -- Pricing
    Subtotal REAL DEFAULT 0,
    Discount REAL DEFAULT 0,
    DiscountPercent REAL DEFAULT 0,
    DiscountReason TEXT,
    Tax REAL DEFAULT 0,
    TaxRate REAL DEFAULT 0,
    ShippingCost REAL DEFAULT 0,
    Total REAL DEFAULT 0,
    CurrencyCode TEXT DEFAULT 'USD',
    -- Terms
    PaymentTerms TEXT,
    DeliveryTerms TEXT,
    TermsAndConditions TEXT,
    Warranty TEXT,
    ValidityDays INTEGER DEFAULT 30,
    -- Line Items
    LineItems TEXT,
    -- Billing Address
    BillingName TEXT,
    BillingAddress TEXT,
    BillingCity TEXT,
    BillingState TEXT,
    BillingZipCode TEXT,
    BillingCountry TEXT,
    -- Shipping Address
    ShippingName TEXT,
    ShippingAddress TEXT,
    ShippingCity TEXT,
    ShippingState TEXT,
    ShippingZipCode TEXT,
    ShippingCountry TEXT,
    -- Contact Information
    ContactName TEXT,
    ContactEmail TEXT,
    ContactPhone TEXT,
    -- Relationships
    CustomerId INTEGER,
    ContactId INTEGER,
    OpportunityId INTEGER,
    AssignedToUserId INTEGER,
    CreatedByUserId INTEGER,
    ApprovedByUserId INTEGER,
    ParentQuoteId INTEGER,
    -- Approval
    RequiresApproval INTEGER DEFAULT 0,
    IsApproved INTEGER DEFAULT 0,
    ApprovalDate TEXT,
    ApprovalNotes TEXT,
    -- Signature
    IsSigned INTEGER DEFAULT 0,
    SignedDate TEXT,
    SignedBy TEXT,
    SignatureUrl TEXT,
    -- Documentation
    Notes TEXT,
    InternalNotes TEXT,
    Attachments TEXT,
    QuotePdfUrl TEXT,
    -- Classification
    Tags TEXT,
    Category TEXT,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ParentQuoteId) REFERENCES Quotes(Id) ON DELETE SET NULL
);

-- Activities Table
CREATE TABLE IF NOT EXISTS Activities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    -- Activity Information
    ActivityType INTEGER DEFAULT 99,
    Title TEXT NOT NULL,
    Description TEXT,
    Details TEXT,
    -- Timing
    ActivityDate TEXT DEFAULT (datetime('now')),
    DurationMinutes INTEGER,
    -- Actor
    UserId INTEGER,
    UserName TEXT,
    UserEmail TEXT,
    -- Related Entity
    EntityType TEXT,
    EntityId INTEGER,
    EntityName TEXT,
    -- Related Records
    CustomerId INTEGER,
    ContactId INTEGER,
    OpportunityId INTEGER,
    CampaignId INTEGER,
    ProductId INTEGER,
    -- Metadata
    IpAddress TEXT,
    UserAgent TEXT,
    DeviceType TEXT,
    Source TEXT,
    -- Classification
    Tags TEXT,
    Category TEXT,
    IsSystemGenerated INTEGER DEFAULT 0,
    IsVisible INTEGER DEFAULT 1,
    CustomFields TEXT,
    -- Audit
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id) ON DELETE CASCADE
);

-- Workflows Table
CREATE TABLE IF NOT EXISTS Workflows (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    EntityType TEXT NOT NULL,
    IsActive INTEGER DEFAULT 1,
    Priority INTEGER DEFAULT 100,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0
);

-- Workflow Rules Table
CREATE TABLE IF NOT EXISTS WorkflowRules (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkflowId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT DEFAULT '',
    ConditionLogic TEXT DEFAULT 'AND',
    ActionType TEXT NOT NULL,
    ActionConfig TEXT,
    TargetUserGroupId INTEGER,
    IsActive INTEGER DEFAULT 1,
    Priority INTEGER DEFAULT 100,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    FOREIGN KEY (TargetUserGroupId) REFERENCES UserGroups(Id) ON DELETE SET NULL
);

-- Workflow Rule Conditions Table
CREATE TABLE IF NOT EXISTS WorkflowRuleConditions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkflowRuleId INTEGER NOT NULL,
    FieldName TEXT NOT NULL,
    Operator TEXT NOT NULL,
    Value TEXT,
    ValueType TEXT DEFAULT 'String',
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id) ON DELETE CASCADE
);

-- Workflow Executions Table
CREATE TABLE IF NOT EXISTS WorkflowExecutions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkflowId INTEGER NOT NULL,
    WorkflowRuleId INTEGER,
    EntityType TEXT NOT NULL,
    EntityId INTEGER NOT NULL,
    Status TEXT DEFAULT 'Success',
    Message TEXT,
    SourceUserGroupId INTEGER,
    TargetUserGroupId INTEGER,
    ExecutedByUserId INTEGER,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id) ON DELETE SET NULL,
    FOREIGN KEY (SourceUserGroupId) REFERENCES UserGroups(Id) ON DELETE SET NULL,
    FOREIGN KEY (TargetUserGroupId) REFERENCES UserGroups(Id) ON DELETE SET NULL,
    FOREIGN KEY (ExecutedByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ============================================
-- Indexes for Performance
-- ============================================

CREATE INDEX IF NOT EXISTS idx_customers_email ON Customers(Email);
CREATE INDEX IF NOT EXISTS idx_customers_company ON Customers(Company);
CREATE INDEX IF NOT EXISTS idx_customers_lifecycle ON Customers(LifecycleStage);
CREATE INDEX IF NOT EXISTS idx_customers_assigned ON Customers(AssignedToUserId);

CREATE INDEX IF NOT EXISTS idx_products_sku ON Products(SKU);
CREATE INDEX IF NOT EXISTS idx_products_category ON Products(Category);
CREATE INDEX IF NOT EXISTS idx_products_status ON Products(Status);

CREATE INDEX IF NOT EXISTS idx_opportunities_customer ON Opportunities(CustomerId);
CREATE INDEX IF NOT EXISTS idx_opportunities_stage ON Opportunities(Stage);
CREATE INDEX IF NOT EXISTS idx_opportunities_close_date ON Opportunities(CloseDate);
CREATE INDEX IF NOT EXISTS idx_opportunities_assigned ON Opportunities(AssignedToUserId);

CREATE INDEX IF NOT EXISTS idx_campaigns_status ON MarketingCampaigns(Status);
CREATE INDEX IF NOT EXISTS idx_campaigns_type ON MarketingCampaigns(CampaignType);
CREATE INDEX IF NOT EXISTS idx_campaigns_dates ON MarketingCampaigns(StartDate, EndDate);

CREATE INDEX IF NOT EXISTS idx_interactions_customer ON Interactions(CustomerId);
CREATE INDEX IF NOT EXISTS idx_interactions_date ON Interactions(InteractionDate);
CREATE INDEX IF NOT EXISTS idx_interactions_type ON Interactions(InteractionType);

CREATE INDEX IF NOT EXISTS idx_contacts_type ON Contacts(ContactType);
CREATE INDEX IF NOT EXISTS idx_contacts_company ON Contacts(Company);
CREATE INDEX IF NOT EXISTS idx_contacts_email ON Contacts(EmailPrimary);

CREATE INDEX IF NOT EXISTS idx_tasks_due_date ON CrmTasks(DueDate);
CREATE INDEX IF NOT EXISTS idx_tasks_status ON CrmTasks(Status);
CREATE INDEX IF NOT EXISTS idx_tasks_assigned ON CrmTasks(AssignedToUserId);

CREATE INDEX IF NOT EXISTS idx_notes_pinned ON Notes(IsPinned);
CREATE INDEX IF NOT EXISTS idx_notes_customer ON Notes(CustomerId);

CREATE INDEX IF NOT EXISTS idx_quotes_number ON Quotes(QuoteNumber);
CREATE INDEX IF NOT EXISTS idx_quotes_status ON Quotes(Status);
CREATE INDEX IF NOT EXISTS idx_quotes_customer ON Quotes(CustomerId);

CREATE INDEX IF NOT EXISTS idx_activities_date ON Activities(ActivityDate);
CREATE INDEX IF NOT EXISTS idx_activities_type ON Activities(ActivityType);
CREATE INDEX IF NOT EXISTS idx_activities_entity ON Activities(EntityType, EntityId);

CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_department ON Users(DepartmentId);

CREATE INDEX IF NOT EXISTS idx_workflow_executions_workflow ON WorkflowExecutions(WorkflowId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_workflow_executions_entity ON WorkflowExecutions(EntityType, EntityId);

-- ============================================
-- Schema complete
-- ============================================

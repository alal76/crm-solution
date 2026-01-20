-- =============================================
-- CRM Database Creation Script - Oracle Database
-- Version: 1.0.0
-- Generated: 2024
-- =============================================

-- Note: Oracle requires running as a privileged user to create tablespaces/users
-- Uncomment and modify the following section if needed:
/*
CREATE TABLESPACE crm_data
    DATAFILE 'crm_data01.dbf' SIZE 500M
    AUTOEXTEND ON NEXT 100M MAXSIZE UNLIMITED;

CREATE USER crm_user IDENTIFIED BY crm_password
    DEFAULT TABLESPACE crm_data
    QUOTA UNLIMITED ON crm_data;

GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE VIEW, CREATE PROCEDURE TO crm_user;
*/

-- =============================================
-- Sequences for Auto-Increment
-- =============================================

CREATE SEQUENCE seq_departments START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_userprofiles START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_users START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_usergroups START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_usergroupmembers START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_userapprovalrequests START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_oauthtokens START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_databasebackups START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_customers START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_products START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_marketingcampaigns START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_campaignmetrics START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_opportunities START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_contacts START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_socialmedialinks START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_interactions START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_crmtasks START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_notes START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_quotes START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_activities START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_workflows START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_workflowrules START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_workflowruleconditions START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_workflowexecutions START WITH 1 INCREMENT BY 1;

-- =============================================
-- Core Tables
-- =============================================

-- Departments Table
CREATE TABLE Departments (
    Id NUMBER(10) DEFAULT seq_departments.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(100) NOT NULL,
    Description VARCHAR2(500),
    DepartmentCode VARCHAR2(20),
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    ParentDepartmentId NUMBER(10),
    ManagerId NUMBER(10),
    Budget NUMBER(18,2),
    CostCenter VARCHAR2(50),
    Location VARCHAR2(200),
    PhoneExtension VARCHAR2(20),
    Email VARCHAR2(255),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Departments_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
);

-- UserProfiles Table
CREATE TABLE UserProfiles (
    Id NUMBER(10) DEFAULT seq_userprofiles.NEXTVAL PRIMARY KEY,
    UserId NUMBER(10) NOT NULL,
    Bio CLOB,
    AvatarUrl VARCHAR2(500),
    PhoneNumber VARCHAR2(50),
    JobTitle VARCHAR2(100),
    Department VARCHAR2(100),
    Location VARCHAR2(200),
    Timezone VARCHAR2(100),
    Language VARCHAR2(20),
    DateFormat VARCHAR2(50),
    Theme VARCHAR2(50),
    NotificationPreferences CLOB,
    SocialLinks CLOB,
    Skills CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);

-- Users Table
CREATE TABLE Users (
    Id NUMBER(10) DEFAULT seq_users.NEXTVAL PRIMARY KEY,
    Username VARCHAR2(50) NOT NULL,
    Email VARCHAR2(255) NOT NULL,
    PasswordHash VARCHAR2(500) NOT NULL,
    FirstName VARCHAR2(100),
    LastName VARCHAR2(100),
    MiddleName VARCHAR2(100),
    Role NUMBER(10) DEFAULT 0 NOT NULL,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    EmailVerified NUMBER(1) DEFAULT 0 NOT NULL,
    EmailVerificationToken VARCHAR2(500),
    PasswordResetToken VARCHAR2(500),
    PasswordResetExpires TIMESTAMP,
    TwoFactorEnabled NUMBER(1) DEFAULT 0 NOT NULL,
    TwoFactorSecret VARCHAR2(500),
    LastLoginAt TIMESTAMP,
    LastLoginIp VARCHAR2(50),
    LoginAttempts NUMBER(10) DEFAULT 0 NOT NULL,
    LockoutEnd TIMESTAMP,
    DepartmentId NUMBER(10),
    ManagerId NUMBER(10),
    ProfileId NUMBER(10),
    RefreshToken VARCHAR2(500),
    RefreshTokenExpiry TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    CONSTRAINT FK_Users_Manager FOREIGN KEY (ManagerId) REFERENCES Users(Id),
    CONSTRAINT FK_Users_Profile FOREIGN KEY (ProfileId) REFERENCES UserProfiles(Id)
);

-- UserGroups Table
CREATE TABLE UserGroups (
    Id NUMBER(10) DEFAULT seq_usergroups.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(100) NOT NULL,
    Description VARCHAR2(500),
    GroupType NUMBER(10) DEFAULT 0 NOT NULL,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    ParentGroupId NUMBER(10),
    Permissions CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT UQ_UserGroups_Name UNIQUE (Name),
    CONSTRAINT FK_UserGroups_Parent FOREIGN KEY (ParentGroupId) REFERENCES UserGroups(Id)
);

-- UserGroupMembers Table
CREATE TABLE UserGroupMembers (
    Id NUMBER(10) DEFAULT seq_usergroupmembers.NEXTVAL PRIMARY KEY,
    UserId NUMBER(10) NOT NULL,
    GroupId NUMBER(10) NOT NULL,
    Role VARCHAR2(50),
    JoinedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    AddedByUserId NUMBER(10),
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CONSTRAINT FK_UGM_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UGM_Group FOREIGN KEY (GroupId) REFERENCES UserGroups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UGM_AddedBy FOREIGN KEY (AddedByUserId) REFERENCES Users(Id)
);

-- UserApprovalRequests Table
CREATE TABLE UserApprovalRequests (
    Id NUMBER(10) DEFAULT seq_userapprovalrequests.NEXTVAL PRIMARY KEY,
    UserId NUMBER(10) NOT NULL,
    Email VARCHAR2(255) NOT NULL,
    FirstName VARCHAR2(100),
    LastName VARCHAR2(100),
    RequestedRole NUMBER(10) NOT NULL,
    RequestedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    ReviewedByUserId NUMBER(10),
    ReviewedAt TIMESTAMP,
    ReviewNotes VARCHAR2(500),
    RejectionReason VARCHAR2(500),
    CONSTRAINT FK_UAR_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UAR_ReviewedBy FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id)
);

-- OAuthTokens Table
CREATE TABLE OAuthTokens (
    Id NUMBER(10) DEFAULT seq_oauthtokens.NEXTVAL PRIMARY KEY,
    UserId NUMBER(10) NOT NULL,
    Provider VARCHAR2(50) NOT NULL,
    AccessToken CLOB NOT NULL,
    RefreshToken CLOB,
    TokenType VARCHAR2(50),
    ExpiresAt TIMESTAMP,
    Scope VARCHAR2(500),
    ProviderUserId VARCHAR2(255),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_OAuthTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- DatabaseBackups Table
CREATE TABLE DatabaseBackups (
    Id NUMBER(10) DEFAULT seq_databasebackups.NEXTVAL PRIMARY KEY,
    FileName VARCHAR2(500) NOT NULL,
    FilePath VARCHAR2(1000) NOT NULL,
    FileSize NUMBER(19) NOT NULL,
    BackupType VARCHAR2(50) NOT NULL,
    Status VARCHAR2(50) NOT NULL,
    StartedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP,
    ErrorMessage CLOB,
    CreatedByUserId NUMBER(10),
    Notes VARCHAR2(500),
    RetentionDays NUMBER(10) DEFAULT 30 NOT NULL,
    IsEncrypted NUMBER(1) DEFAULT 0 NOT NULL,
    ChecksumMD5 VARCHAR2(100),
    CONSTRAINT FK_DBBackups_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- =============================================
-- CRM Tables
-- =============================================

-- Customers Table
CREATE TABLE Customers (
    Id NUMBER(10) DEFAULT seq_customers.NEXTVAL PRIMARY KEY,
    FirstName VARCHAR2(100) NOT NULL,
    LastName VARCHAR2(100) NOT NULL,
    MiddleName VARCHAR2(100),
    Prefix VARCHAR2(20),
    Suffix VARCHAR2(20),
    FullName VARCHAR2(300),
    Email VARCHAR2(255) NOT NULL,
    SecondaryEmail VARCHAR2(255),
    Phone VARCHAR2(50),
    MobilePhone VARCHAR2(50),
    WorkPhone VARCHAR2(50),
    Fax VARCHAR2(50),
    Company VARCHAR2(200),
    LegalName VARCHAR2(200),
    DBA VARCHAR2(200),
    TaxId VARCHAR2(50),
    DunsNumber VARCHAR2(20),
    Industry VARCHAR2(100),
    SubIndustry VARCHAR2(100),
    SICCode VARCHAR2(20),
    NAICSCode VARCHAR2(20),
    NumberOfEmployees NUMBER(10),
    AnnualRevenue NUMBER(18,2),
    OwnershipType VARCHAR2(50),
    YearFounded NUMBER(10),
    FiscalYearEnd VARCHAR2(20),
    JobTitle VARCHAR2(100),
    Department VARCHAR2(100),
    ReportsTo VARCHAR2(100),
    Website VARCHAR2(500),
    LinkedIn VARCHAR2(500),
    Twitter VARCHAR2(500),
    Facebook VARCHAR2(500),
    Address VARCHAR2(500),
    AddressLine2 VARCHAR2(500),
    City VARCHAR2(100),
    State VARCHAR2(100),
    ZipCode VARCHAR2(20),
    Country VARCHAR2(100),
    ShippingAddress VARCHAR2(500),
    ShippingAddressLine2 VARCHAR2(500),
    ShippingCity VARCHAR2(100),
    ShippingState VARCHAR2(100),
    ShippingZipCode VARCHAR2(20),
    ShippingCountry VARCHAR2(100),
    CustomerType NUMBER(10) DEFAULT 0 NOT NULL,
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    LifecycleStage NUMBER(10) DEFAULT 0 NOT NULL,
    LeadSource VARCHAR2(100),
    FirstContactDate TIMESTAMP,
    LastContactDate TIMESTAMP,
    LeadScore NUMBER(10),
    CustomerHealthScore NUMBER(10),
    PreferredContactMethod NUMBER(10) DEFAULT 0 NOT NULL,
    PreferredLanguage VARCHAR2(20),
    Timezone VARCHAR2(100),
    CurrencyCode VARCHAR2(10),
    PaymentTerms VARCHAR2(100),
    CreditLimit NUMBER(18,2),
    AccountBalance NUMBER(18,2),
    LifetimeValue NUMBER(18,2),
    TotalPurchases NUMBER(10),
    AverageOrderValue NUMBER(18,2),
    LastPurchaseDate TIMESTAMP,
    ReferralSource VARCHAR2(200),
    ReferredBy VARCHAR2(200),
    AssignedToUserId NUMBER(10),
    ParentAccountId NUMBER(10),
    DoNotCall NUMBER(1) DEFAULT 0 NOT NULL,
    DoNotEmail NUMBER(1) DEFAULT 0 NOT NULL,
    DoNotMail NUMBER(1) DEFAULT 0 NOT NULL,
    OptInEmail NUMBER(1) DEFAULT 1 NOT NULL,
    OptInPhone NUMBER(1) DEFAULT 1 NOT NULL,
    OptInSMS NUMBER(1) DEFAULT 0 NOT NULL,
    GDPRConsent NUMBER(1) DEFAULT 0 NOT NULL,
    GDPRConsentDate TIMESTAMP,
    Tags CLOB,
    Notes CLOB,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT UQ_Customers_Email UNIQUE (Email),
    CONSTRAINT FK_Customers_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Customers_Parent FOREIGN KEY (ParentAccountId) REFERENCES Customers(Id)
);

-- Products Table
CREATE TABLE Products (
    Id NUMBER(10) DEFAULT seq_products.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(200) NOT NULL,
    Description CLOB,
    ShortDescription VARCHAR2(500),
    SKU VARCHAR2(100),
    Barcode VARCHAR2(100),
    GTIN VARCHAR2(50),
    MPN VARCHAR2(100),
    ProductType NUMBER(10) DEFAULT 0 NOT NULL,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    Category VARCHAR2(100),
    SubCategory VARCHAR2(100),
    Brand VARCHAR2(100),
    Manufacturer VARCHAR2(200),
    Vendor VARCHAR2(200),
    VendorPartNumber VARCHAR2(100),
    Price NUMBER(18,2) NOT NULL,
    ListPrice NUMBER(18,2),
    Cost NUMBER(18,2),
    CompareAtPrice NUMBER(18,2),
    Margin NUMBER(18,2),
    MarkupPercent NUMBER(18,4),
    MinPrice NUMBER(18,2),
    MaxDiscount NUMBER(18,4),
    IsTaxable NUMBER(1) DEFAULT 1 NOT NULL,
    TaxRate NUMBER(18,4),
    TaxCode VARCHAR2(50),
    Quantity NUMBER(10) DEFAULT 0 NOT NULL,
    ReservedQuantity NUMBER(10) DEFAULT 0 NOT NULL,
    AvailableQuantity NUMBER(10) DEFAULT 0 NOT NULL,
    MinQuantity NUMBER(10),
    MaxQuantity NUMBER(10),
    ReorderLevel NUMBER(10),
    ReorderQuantity NUMBER(10),
    LeadTimeDays NUMBER(10),
    WarehouseLocation VARCHAR2(100),
    BinNumber VARCHAR2(50),
    TrackInventory NUMBER(1) DEFAULT 1 NOT NULL,
    AllowBackorder NUMBER(1) DEFAULT 0 NOT NULL,
    Weight NUMBER(18,4),
    WeightUnit VARCHAR2(20),
    Length NUMBER(18,4),
    Width NUMBER(18,4),
    Height NUMBER(18,4),
    DimensionUnit VARCHAR2(20),
    ShippingClass VARCHAR2(50),
    RequiresShipping NUMBER(1) DEFAULT 1 NOT NULL,
    ImageUrl VARCHAR2(1000),
    ThumbnailUrl VARCHAR2(1000),
    MediaGallery CLOB,
    VideoUrl VARCHAR2(1000),
    DocumentUrl VARCHAR2(1000),
    MetaTitle VARCHAR2(200),
    MetaDescription VARCHAR2(500),
    MetaKeywords VARCHAR2(500),
    Slug VARCHAR2(300),
    TotalSold NUMBER(10) DEFAULT 0 NOT NULL,
    TotalRevenue NUMBER(18,2) DEFAULT 0 NOT NULL,
    ViewCount NUMBER(10) DEFAULT 0 NOT NULL,
    AverageRating NUMBER,
    ReviewCount NUMBER(10) DEFAULT 0 NOT NULL,
    IsFeatured NUMBER(1) DEFAULT 0 NOT NULL,
    IsNew NUMBER(1) DEFAULT 0 NOT NULL,
    IsBestSeller NUMBER(1) DEFAULT 0 NOT NULL,
    IsOnSale NUMBER(1) DEFAULT 0 NOT NULL,
    SaleStartDate TIMESTAMP,
    SaleEndDate TIMESTAMP,
    WarrantyPeriod VARCHAR2(50),
    ReturnPolicy VARCHAR2(500),
    SupportUrl VARCHAR2(500),
    CustomField1 VARCHAR2(500),
    CustomField2 VARCHAR2(500),
    CustomField3 VARCHAR2(500),
    CustomField4 VARCHAR2(500),
    CustomField5 VARCHAR2(500),
    Tags CLOB,
    Notes CLOB,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP
);

-- MarketingCampaigns Table
CREATE TABLE MarketingCampaigns (
    Id NUMBER(10) DEFAULT seq_marketingcampaigns.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(200) NOT NULL,
    Description CLOB,
    Objective VARCHAR2(500),
    CampaignType NUMBER(10) DEFAULT 0 NOT NULL,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    StartDate TIMESTAMP,
    EndDate TIMESTAMP,
    ActualStartDate TIMESTAMP,
    ActualEndDate TIMESTAMP,
    Budget NUMBER(18,2),
    ActualCost NUMBER(18,2),
    ExpectedRevenue NUMBER(18,2),
    ActualRevenue NUMBER(18,2),
    ROI NUMBER,
    ROAS NUMBER,
    TargetAudience NUMBER(10),
    ReachActual NUMBER(10),
    Impressions NUMBER(10),
    Clicks NUMBER(10),
    CTR NUMBER,
    Conversions NUMBER(10),
    ConversionRate NUMBER,
    CostPerClick NUMBER(18,2),
    CostPerConversion NUMBER(18,2),
    CostPerLead NUMBER(18,2),
    CostPerAcquisition NUMBER(18,2),
    LeadsGenerated NUMBER(10),
    OpportunitiesCreated NUMBER(10),
    CustomersAcquired NUMBER(10),
    EmailsSent NUMBER(10),
    EmailsDelivered NUMBER(10),
    EmailsOpened NUMBER(10),
    EmailOpenRate NUMBER,
    EmailsClicked NUMBER(10),
    EmailClickRate NUMBER,
    EmailsBounced NUMBER(10),
    EmailBounceRate NUMBER,
    Unsubscribes NUMBER(10),
    UnsubscribeRate NUMBER,
    SocialShares NUMBER(10),
    SocialLikes NUMBER(10),
    SocialComments NUMBER(10),
    SocialReach NUMBER(10),
    WebsiteVisits NUMBER(10),
    PageViews NUMBER(10),
    BounceRate NUMBER,
    AvgSessionDuration NUMBER,
    OwnerId NUMBER(10),
    CreatedByUserId NUMBER(10),
    Channel VARCHAR2(100),
    SubChannel VARCHAR2(100),
    TargetRegion VARCHAR2(200),
    TargetIndustry VARCHAR2(200),
    TargetJobTitles VARCHAR2(500),
    TargetCompanySize VARCHAR2(200),
    ABTestVariant VARCHAR2(50),
    ControlGroup NUMBER(1) DEFAULT 0 NOT NULL,
    ParentCampaignId NUMBER(10),
    LandingPageUrl VARCHAR2(1000),
    TrackingCode VARCHAR2(200),
    UTMSource VARCHAR2(100),
    UTMCampaign VARCHAR2(100),
    UTMContent VARCHAR2(100),
    Tags CLOB,
    Notes CLOB,
    IsArchived NUMBER(1) DEFAULT 0 NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_MC_Owner FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT FK_MC_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_MC_Parent FOREIGN KEY (ParentCampaignId) REFERENCES MarketingCampaigns(Id)
);

-- CampaignMetrics Table
CREATE TABLE CampaignMetrics (
    Id NUMBER(10) DEFAULT seq_campaignmetrics.NEXTVAL PRIMARY KEY,
    CampaignId NUMBER(10) NOT NULL,
    MetricDate DATE NOT NULL,
    Impressions NUMBER(10) DEFAULT 0 NOT NULL,
    Clicks NUMBER(10) DEFAULT 0 NOT NULL,
    Conversions NUMBER(10) DEFAULT 0 NOT NULL,
    Cost NUMBER(18,2) DEFAULT 0 NOT NULL,
    Revenue NUMBER(18,2) DEFAULT 0 NOT NULL,
    Leads NUMBER(10) DEFAULT 0 NOT NULL,
    EmailsSent NUMBER(10) DEFAULT 0 NOT NULL,
    EmailsOpened NUMBER(10) DEFAULT 0 NOT NULL,
    EmailsClicked NUMBER(10) DEFAULT 0 NOT NULL,
    Unsubscribes NUMBER(10) DEFAULT 0 NOT NULL,
    Channel VARCHAR2(100),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT FK_CM_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE
);

-- Opportunities Table
CREATE TABLE Opportunities (
    Id NUMBER(10) DEFAULT seq_opportunities.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(300) NOT NULL,
    Description CLOB,
    OpportunityType NUMBER(10) DEFAULT 0 NOT NULL,
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    Amount NUMBER(18,2),
    ExpectedRevenue NUMBER(18,2),
    ActualRevenue NUMBER(18,2),
    GrossProfit NUMBER(18,2),
    Commission NUMBER(18,2),
    CurrencyCode VARCHAR2(10),
    Stage NUMBER(10) DEFAULT 0 NOT NULL,
    StageChangedAt TIMESTAMP,
    DaysInStage NUMBER(10) DEFAULT 0 NOT NULL,
    Probability NUMBER DEFAULT 0 NOT NULL,
    ForecastCategory NUMBER(10) DEFAULT 0 NOT NULL,
    CloseDate TIMESTAMP,
    ExpectedCloseDate TIMESTAMP,
    ActualCloseDate TIMESTAMP,
    NextStep VARCHAR2(500),
    NextStepDate TIMESTAMP,
    LeadSource VARCHAR2(200),
    CustomerId NUMBER(10),
    ProductId NUMBER(10),
    PrimaryContactId NUMBER(10),
    AssignedToUserId NUMBER(10),
    TeamId NUMBER(10),
    CampaignId NUMBER(10),
    QuoteId NUMBER(10),
    CompetitorName VARCHAR2(200),
    CompetitorStrengths VARCHAR2(500),
    CompetitorWeaknesses VARCHAR2(500),
    WinLossReason VARCHAR2(500),
    LostToCompetitor VARCHAR2(200),
    DecisionMakers VARCHAR2(500),
    BudgetStatus VARCHAR2(100),
    Authority VARCHAR2(100),
    Need VARCHAR2(100),
    Timeline VARCHAR2(100),
    BANTScore NUMBER(10),
    RiskLevel NUMBER(10) DEFAULT 0 NOT NULL,
    RiskNotes VARCHAR2(500),
    LastActivityDate TIMESTAMP,
    LastActivityType VARCHAR2(100),
    TotalActivities NUMBER(10) DEFAULT 0 NOT NULL,
    Tags CLOB,
    Notes CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Opp_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Opp_Product FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_Opp_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Opp_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id)
);

-- Contacts Table
CREATE TABLE Contacts (
    Id NUMBER(10) DEFAULT seq_contacts.NEXTVAL PRIMARY KEY,
    ContactType NUMBER(10) DEFAULT 0 NOT NULL,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    FirstName VARCHAR2(100) NOT NULL,
    LastName VARCHAR2(100) NOT NULL,
    MiddleName VARCHAR2(100),
    Prefix VARCHAR2(20),
    Suffix VARCHAR2(20),
    FullName VARCHAR2(300),
    Nickname VARCHAR2(100),
    EmailPrimary VARCHAR2(255),
    EmailSecondary VARCHAR2(255),
    EmailWork VARCHAR2(255),
    PhonePrimary VARCHAR2(50),
    PhoneMobile VARCHAR2(50),
    PhoneWork VARCHAR2(50),
    PhoneHome VARCHAR2(50),
    Fax VARCHAR2(50),
    JobTitle VARCHAR2(150),
    Department VARCHAR2(100),
    Company VARCHAR2(200),
    ReportsTo VARCHAR2(100),
    AssistantName VARCHAR2(100),
    AssistantPhone VARCHAR2(50),
    Address VARCHAR2(500),
    AddressLine2 VARCHAR2(500),
    City VARCHAR2(100),
    State VARCHAR2(100),
    ZipCode VARCHAR2(20),
    Country VARCHAR2(100),
    Region VARCHAR2(100),
    Timezone VARCHAR2(100),
    PreferredLanguage VARCHAR2(20),
    PreferredContactMethod NUMBER(10) DEFAULT 0 NOT NULL,
    LeadScore NUMBER(10),
    LeadSource VARCHAR2(100),
    LeadStatus NUMBER(10),
    ConversionDate TIMESTAMP,
    IsKeyContact NUMBER(1) DEFAULT 0 NOT NULL,
    IsDecisionMaker NUMBER(1) DEFAULT 0 NOT NULL,
    Influence VARCHAR2(50),
    BudgetAuthority NUMBER(1) DEFAULT 0 NOT NULL,
    DoNotCall NUMBER(1) DEFAULT 0 NOT NULL,
    DoNotEmail NUMBER(1) DEFAULT 0 NOT NULL,
    HasOptedOut NUMBER(1) DEFAULT 0 NOT NULL,
    OptOutDate TIMESTAMP,
    BirthDate DATE,
    Anniversary DATE,
    CustomerId NUMBER(10),
    AccountId NUMBER(10),
    OwnerId NUMBER(10),
    LastContactedDate TIMESTAMP,
    TotalInteractions NUMBER(10) DEFAULT 0 NOT NULL,
    Tags CLOB,
    Notes CLOB,
    DateAdded TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    DateModified TIMESTAMP,
    CONSTRAINT FK_Contacts_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Contacts_Owner FOREIGN KEY (OwnerId) REFERENCES Users(Id)
);

-- SocialMediaLinks Table
CREATE TABLE SocialMediaLinks (
    Id NUMBER(10) DEFAULT seq_socialmedialinks.NEXTVAL PRIMARY KEY,
    ContactId NUMBER(10) NOT NULL,
    Platform VARCHAR2(50) NOT NULL,
    ProfileUrl VARCHAR2(500) NOT NULL,
    Username VARCHAR2(100),
    IsVerified NUMBER(1) DEFAULT 0 NOT NULL,
    FollowerCount NUMBER(10),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT FK_SML_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE
);

-- Interactions Table
CREATE TABLE Interactions (
    Id NUMBER(10) DEFAULT seq_interactions.NEXTVAL PRIMARY KEY,
    InteractionType NUMBER(10) DEFAULT 0 NOT NULL,
    Type VARCHAR2(50) NOT NULL,
    Direction NUMBER(10) DEFAULT 0 NOT NULL,
    Subject VARCHAR2(300),
    Description CLOB,
    InteractionDate TIMESTAMP NOT NULL,
    StartTime TIMESTAMP,
    EndTime TIMESTAMP,
    DurationMinutes NUMBER(10),
    Outcome NUMBER(10) DEFAULT 0 NOT NULL,
    OutcomeNotes VARCHAR2(500),
    Sentiment NUMBER(10) DEFAULT 0 NOT NULL,
    SentimentScore NUMBER,
    IsCompleted NUMBER(1) DEFAULT 0 NOT NULL,
    CompletedAt TIMESTAMP,
    CustomerId NUMBER(10),
    ContactId NUMBER(10),
    OpportunityId NUMBER(10),
    CampaignId NUMBER(10),
    CaseId NUMBER(10),
    AssignedToUserId NUMBER(10),
    CreatedByUserId NUMBER(10),
    FollowUpRequired NUMBER(1) DEFAULT 0 NOT NULL,
    FollowUpDate TIMESTAMP,
    FollowUpNotes VARCHAR2(500),
    FollowUpCompleted NUMBER(1) DEFAULT 0 NOT NULL,
    Location VARCHAR2(200),
    MeetingUrl VARCHAR2(500),
    MeetingId VARCHAR2(100),
    Attendees CLOB,
    AttachmentCount NUMBER(10) DEFAULT 0 NOT NULL,
    AttachmentUrls CLOB,
    EmailMessageId VARCHAR2(200),
    CallRecordingUrl VARCHAR2(500),
    CallDisposition VARCHAR2(100),
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    Tags CLOB,
    Notes CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Int_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Int_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id),
    CONSTRAINT FK_Int_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id),
    CONSTRAINT FK_Int_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id),
    CONSTRAINT FK_Int_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Int_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- CrmTasks Table
CREATE TABLE CrmTasks (
    Id NUMBER(10) DEFAULT seq_crmtasks.NEXTVAL PRIMARY KEY,
    Subject VARCHAR2(300) NOT NULL,
    Description CLOB,
    TaskType NUMBER(10) DEFAULT 0 NOT NULL,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    DueDate TIMESTAMP,
    StartDate TIMESTAMP,
    CompletedDate TIMESTAMP,
    ReminderDate TIMESTAMP,
    ReminderSent NUMBER(1) DEFAULT 0 NOT NULL,
    PercentComplete NUMBER(10) DEFAULT 0 NOT NULL,
    EstimatedMinutes NUMBER(10),
    ActualMinutes NUMBER(10),
    RecurrencePattern VARCHAR2(100),
    RecurrenceEndDate TIMESTAMP,
    ParentTaskId NUMBER(10),
    IsAllDay NUMBER(1) DEFAULT 0 NOT NULL,
    IsBillable NUMBER(1) DEFAULT 0 NOT NULL,
    BillingRate NUMBER(18,2),
    CustomerId NUMBER(10),
    ContactId NUMBER(10),
    OpportunityId NUMBER(10),
    CampaignId NUMBER(10),
    AssignedToUserId NUMBER(10),
    CreatedByUserId NUMBER(10),
    CompletedByUserId NUMBER(10),
    Tags CLOB,
    Notes CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Task_Parent FOREIGN KEY (ParentTaskId) REFERENCES CrmTasks(Id),
    CONSTRAINT FK_Task_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Task_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id),
    CONSTRAINT FK_Task_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id),
    CONSTRAINT FK_Task_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id),
    CONSTRAINT FK_Task_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Task_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Task_CompletedBy FOREIGN KEY (CompletedByUserId) REFERENCES Users(Id)
);

-- Notes Table
CREATE TABLE Notes (
    Id NUMBER(10) DEFAULT seq_notes.NEXTVAL PRIMARY KEY,
    Title VARCHAR2(300),
    Content CLOB,
    Summary VARCHAR2(500),
    NoteType NUMBER(10) DEFAULT 0 NOT NULL,
    Visibility NUMBER(10) DEFAULT 0 NOT NULL,
    IsPinned NUMBER(1) DEFAULT 0 NOT NULL,
    IsImportant NUMBER(1) DEFAULT 0 NOT NULL,
    IsArchived NUMBER(1) DEFAULT 0 NOT NULL,
    ParentNoteId NUMBER(10),
    CustomerId NUMBER(10),
    ContactId NUMBER(10),
    OpportunityId NUMBER(10),
    CampaignId NUMBER(10),
    TaskId NUMBER(10),
    InteractionId NUMBER(10),
    QuoteId NUMBER(10),
    CreatedByUserId NUMBER(10),
    LastModifiedByUserId NUMBER(10),
    AttachmentCount NUMBER(10) DEFAULT 0 NOT NULL,
    AttachmentUrls CLOB,
    Tags CLOB,
    Metadata CLOB,
    ViewCount NUMBER(10) DEFAULT 0 NOT NULL,
    LastViewedAt TIMESTAMP,
    LastViewedByUserId NUMBER(10),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Notes_Parent FOREIGN KEY (ParentNoteId) REFERENCES Notes(Id),
    CONSTRAINT FK_Notes_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Notes_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id),
    CONSTRAINT FK_Notes_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id),
    CONSTRAINT FK_Notes_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id),
    CONSTRAINT FK_Notes_Task FOREIGN KEY (TaskId) REFERENCES CrmTasks(Id),
    CONSTRAINT FK_Notes_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- Quotes Table
CREATE TABLE Quotes (
    Id NUMBER(10) DEFAULT seq_quotes.NEXTVAL PRIMARY KEY,
    QuoteNumber VARCHAR2(50) NOT NULL,
    Name VARCHAR2(300),
    Description CLOB,
    Status NUMBER(10) DEFAULT 0 NOT NULL,
    QuoteDate TIMESTAMP NOT NULL,
    ExpirationDate TIMESTAMP,
    AcceptedDate TIMESTAMP,
    RejectedDate TIMESTAMP,
    Subtotal NUMBER(18,2) DEFAULT 0 NOT NULL,
    Discount NUMBER(18,2) DEFAULT 0 NOT NULL,
    DiscountPercent NUMBER(18,4),
    DiscountReason VARCHAR2(200),
    Tax NUMBER(18,2) DEFAULT 0 NOT NULL,
    TaxRate NUMBER(18,4),
    Shipping NUMBER(18,2) DEFAULT 0 NOT NULL,
    Total NUMBER(18,2) DEFAULT 0 NOT NULL,
    CurrencyCode VARCHAR2(10),
    PaymentTerms VARCHAR2(200),
    DeliveryTerms VARCHAR2(200),
    ValidityDays NUMBER(10),
    TermsAndConditions CLOB,
    BillingName VARCHAR2(200),
    BillingAddress VARCHAR2(500),
    BillingCity VARCHAR2(100),
    BillingState VARCHAR2(100),
    BillingZipCode VARCHAR2(20),
    BillingCountry VARCHAR2(100),
    ShippingName VARCHAR2(200),
    ShippingAddress VARCHAR2(500),
    ShippingCity VARCHAR2(100),
    ShippingState VARCHAR2(100),
    ShippingZipCode VARCHAR2(20),
    ShippingCountry VARCHAR2(100),
    CustomerId NUMBER(10),
    ContactId NUMBER(10),
    OpportunityId NUMBER(10),
    AssignedToUserId NUMBER(10),
    CreatedByUserId NUMBER(10),
    ApprovedByUserId NUMBER(10),
    ApprovedAt TIMESTAMP,
    SentAt TIMESTAMP,
    SentToEmail VARCHAR2(255),
    ViewedAt TIMESTAMP,
    VersionNumber NUMBER(10) DEFAULT 1 NOT NULL,
    ParentQuoteId NUMBER(10),
    PdfUrl VARCHAR2(500),
    Tags CLOB,
    Notes CLOB,
    InternalNotes CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT UQ_Quotes_Number UNIQUE (QuoteNumber),
    CONSTRAINT FK_Quote_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Quote_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id),
    CONSTRAINT FK_Quote_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id),
    CONSTRAINT FK_Quote_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Quote_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Quote_ApprovedBy FOREIGN KEY (ApprovedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Quote_Parent FOREIGN KEY (ParentQuoteId) REFERENCES Quotes(Id)
);

-- Activities Table
CREATE TABLE Activities (
    Id NUMBER(10) DEFAULT seq_activities.NEXTVAL PRIMARY KEY,
    ActivityType NUMBER(10) DEFAULT 0 NOT NULL,
    Title VARCHAR2(300) NOT NULL,
    Description CLOB,
    ActivityDate TIMESTAMP NOT NULL,
    StartTime TIMESTAMP,
    EndTime TIMESTAMP,
    DurationMinutes NUMBER(10),
    UserId NUMBER(10),
    UserName VARCHAR2(200),
    UserEmail VARCHAR2(255),
    EntityType VARCHAR2(100),
    EntityId NUMBER(10),
    EntityName VARCHAR2(300),
    CustomerId NUMBER(10),
    ContactId NUMBER(10),
    OpportunityId NUMBER(10),
    CampaignId NUMBER(10),
    TaskId NUMBER(10),
    InteractionId NUMBER(10),
    OldValue CLOB,
    NewValue CLOB,
    FieldChanged VARCHAR2(100),
    IpAddress VARCHAR2(50),
    UserAgent VARCHAR2(500),
    SessionId VARCHAR2(200),
    IsSystemGenerated NUMBER(1) DEFAULT 0 NOT NULL,
    IsVisible NUMBER(1) DEFAULT 1 NOT NULL,
    Importance NUMBER(10) DEFAULT 0 NOT NULL,
    Tags CLOB,
    Metadata CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT FK_Act_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Act_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_Act_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id),
    CONSTRAINT FK_Act_Opportunity FOREIGN KEY (OpportunityId) REFERENCES Opportunities(Id),
    CONSTRAINT FK_Act_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id),
    CONSTRAINT FK_Act_Task FOREIGN KEY (TaskId) REFERENCES CrmTasks(Id)
);

-- =============================================
-- Workflow Tables
-- =============================================

-- Workflows Table
CREATE TABLE Workflows (
    Id NUMBER(10) DEFAULT seq_workflows.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(200) NOT NULL,
    Description CLOB,
    WorkflowType VARCHAR2(50) NOT NULL,
    TriggerType VARCHAR2(50) NOT NULL,
    TriggerEntity VARCHAR2(100),
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    Priority NUMBER(10) DEFAULT 0 NOT NULL,
    Version NUMBER(10) DEFAULT 1 NOT NULL,
    ExecutionOrder NUMBER(10) DEFAULT 0 NOT NULL,
    MaxExecutions NUMBER(10),
    CurrentExecutions NUMBER(10) DEFAULT 0 NOT NULL,
    StartDate TIMESTAMP,
    EndDate TIMESTAMP,
    LastExecutedAt TIMESTAMP,
    NextExecutionAt TIMESTAMP,
    CreatedByUserId NUMBER(10),
    ModifiedByUserId NUMBER(10),
    Configuration CLOB,
    ErrorHandling VARCHAR2(500),
    NotifyOnError NUMBER(1) DEFAULT 1 NOT NULL,
    NotifyEmail VARCHAR2(255),
    Tags CLOB,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_WF_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_WF_ModifiedBy FOREIGN KEY (ModifiedByUserId) REFERENCES Users(Id)
);

-- WorkflowRules Table
CREATE TABLE WorkflowRules (
    Id NUMBER(10) DEFAULT seq_workflowrules.NEXTVAL PRIMARY KEY,
    WorkflowId NUMBER(10) NOT NULL,
    Name VARCHAR2(200) NOT NULL,
    Description VARCHAR2(500),
    RuleOrder NUMBER(10) DEFAULT 0 NOT NULL,
    ActionType VARCHAR2(100) NOT NULL,
    ActionConfiguration CLOB,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    StopOnSuccess NUMBER(1) DEFAULT 0 NOT NULL,
    StopOnFailure NUMBER(1) DEFAULT 0 NOT NULL,
    RetryCount NUMBER(10) DEFAULT 0 NOT NULL,
    RetryInterval NUMBER(10) DEFAULT 60 NOT NULL,
    TimeoutSeconds NUMBER(10),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_WFR_Workflow FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE
);

-- WorkflowRuleConditions Table
CREATE TABLE WorkflowRuleConditions (
    Id NUMBER(10) DEFAULT seq_workflowruleconditions.NEXTVAL PRIMARY KEY,
    WorkflowRuleId NUMBER(10) NOT NULL,
    ConditionOrder NUMBER(10) DEFAULT 0 NOT NULL,
    FieldName VARCHAR2(100) NOT NULL,
    Operator VARCHAR2(50) NOT NULL,
    Value VARCHAR2(500),
    ValueType VARCHAR2(50),
    LogicalOperator VARCHAR2(10) DEFAULT 'AND' NOT NULL,
    ParentConditionId NUMBER(10),
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT FK_WFRC_Rule FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WFRC_Parent FOREIGN KEY (ParentConditionId) REFERENCES WorkflowRuleConditions(Id)
);

-- WorkflowExecutions Table
CREATE TABLE WorkflowExecutions (
    Id NUMBER(10) DEFAULT seq_workflowexecutions.NEXTVAL PRIMARY KEY,
    WorkflowId NUMBER(10) NOT NULL,
    WorkflowRuleId NUMBER(10),
    EntityType VARCHAR2(100) NOT NULL,
    EntityId NUMBER(10) NOT NULL,
    Status VARCHAR2(50) NOT NULL,
    StartedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP,
    Duration NUMBER(10),
    ErrorMessage CLOB,
    ErrorStack CLOB,
    RetryCount NUMBER(10) DEFAULT 0 NOT NULL,
    InputData CLOB,
    OutputData CLOB,
    ExecutedByUserId NUMBER(10),
    IsManual NUMBER(1) DEFAULT 0 NOT NULL,
    ParentExecutionId NUMBER(10),
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    CONSTRAINT FK_WFE_Workflow FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id),
    CONSTRAINT FK_WFE_Rule FOREIGN KEY (WorkflowRuleId) REFERENCES WorkflowRules(Id),
    CONSTRAINT FK_WFE_User FOREIGN KEY (ExecutedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_WFE_Parent FOREIGN KEY (ParentExecutionId) REFERENCES WorkflowExecutions(Id)
);

-- =============================================
-- Performance Indexes
-- =============================================

CREATE INDEX IX_Users_IsActive ON Users (IsActive);
CREATE INDEX IX_Users_Role ON Users (Role);
CREATE INDEX IX_Customers_Email ON Customers (Email);
CREATE INDEX IX_Customers_Company ON Customers (Company);
CREATE INDEX IX_Customers_LifecycleStage ON Customers (LifecycleStage);
CREATE INDEX IX_Customers_AssignedTo ON Customers (AssignedToUserId);
CREATE INDEX IX_Products_SKU ON Products (SKU);
CREATE INDEX IX_Products_Category ON Products (Category);
CREATE INDEX IX_Products_Status ON Products (Status);
CREATE INDEX IX_MC_Status ON MarketingCampaigns (Status);
CREATE INDEX IX_MC_Type ON MarketingCampaigns (CampaignType);
CREATE INDEX IX_CM_Date ON CampaignMetrics (MetricDate);
CREATE INDEX IX_Opp_Stage ON Opportunities (Stage);
CREATE INDEX IX_Opp_CloseDate ON Opportunities (CloseDate);
CREATE INDEX IX_Opp_CustomerId ON Opportunities (CustomerId);
CREATE INDEX IX_Contacts_Email ON Contacts (EmailPrimary);
CREATE INDEX IX_Contacts_Company ON Contacts (Company);
CREATE INDEX IX_Int_Date ON Interactions (InteractionDate);
CREATE INDEX IX_Int_CustomerId ON Interactions (CustomerId);
CREATE INDEX IX_Task_DueDate ON CrmTasks (DueDate);
CREATE INDEX IX_Task_Status ON CrmTasks (Status);
CREATE INDEX IX_Notes_CustomerId ON Notes (CustomerId);
CREATE INDEX IX_Notes_CreatedAt ON Notes (CreatedAt);
CREATE INDEX IX_Quotes_Number ON Quotes (QuoteNumber);
CREATE INDEX IX_Quotes_Status ON Quotes (Status);
CREATE INDEX IX_Act_Date ON Activities (ActivityDate);
CREATE INDEX IX_Act_UserId ON Activities (UserId);
CREATE INDEX IX_Act_Entity ON Activities (EntityType, EntityId);
CREATE INDEX IX_WF_IsActive ON Workflows (IsActive);
CREATE INDEX IX_WF_TriggerType ON Workflows (TriggerType);
CREATE INDEX IX_WFE_Status ON WorkflowExecutions (Status);
CREATE INDEX IX_WFE_StartedAt ON WorkflowExecutions (StartedAt);

-- =============================================
-- Initial Data
-- =============================================

-- Note: Admin user and sample data should be created using the CRM.DatabaseSeeder application

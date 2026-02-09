-- Migration: 019_create_missing_entity_tables.sql
-- Created: 2026-02-12
-- Description: Creates 27 tables for entities that have DbSets in CrmDbContext
--              but are missing from the database (EnsureCreated is a no-op when DB exists).
-- Groups: ITSM (11), Calendar Integration (3), Email Integration (3),
--         Email Template Versioning (3), Landing Pages (3), Events (1),
--         Lead Scoring (1), Subscription (1), Workflow (1)

-- ============================================================================
-- GROUP 1: ITSM MODULE TABLES (custom primary keys, no RowVersion)
-- ============================================================================

-- 1.1 ITSM Knowledge Articles (separate from KB KnowledgeArticles)
CREATE TABLE IF NOT EXISTS ITSMKnowledgeArticles (
    ArticleId INT AUTO_INCREMENT PRIMARY KEY,
    Number VARCHAR(20) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500) NULL,
    ArticleBody LONGTEXT NOT NULL,
    ArticleType INT NOT NULL DEFAULT 1,
    CategoryId INT NULL,
    SubcategoryId INT NULL,
    AuthorId INT NOT NULL,
    OwnerId INT NOT NULL,
    PublishingState INT NOT NULL DEFAULT 1,
    PublishedDate DATETIME NULL,
    PublishedById INT NULL,
    ReviewDate DATETIME NULL,
    ExpirationDate DATETIME NULL,
    Version INT NOT NULL DEFAULT 1,
    IsInternal TINYINT(1) NOT NULL DEFAULT 1,
    IsExternal TINYINT(1) NOT NULL DEFAULT 0,
    IsPublic TINYINT(1) NOT NULL DEFAULT 0,
    Tags TEXT NULL,
    ViewCount INT NOT NULL DEFAULT 0,
    HelpfulCount INT NOT NULL DEFAULT 0,
    NotHelpfulCount INT NOT NULL DEFAULT 0,
    AttachedToIncidentCount INT NOT NULL DEFAULT 0,
    LastViewedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL,
    ModifiedById INT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_ITSMKnowledgeArticles_Number (Number),
    INDEX IX_ITSMKnowledgeArticles_PublishingState (PublishingState),
    INDEX IX_ITSMKnowledgeArticles_ArticleType (ArticleType),
    INDEX IX_ITSMKnowledgeArticles_AuthorId (AuthorId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.2 ITSM SLA Policies (separate from KB SLAPolicies)
CREATE TABLE IF NOT EXISTS ITSMSLAPolicies (
    SLAPolicyId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    TargetType INT NOT NULL DEFAULT 1,
    P1ResponseMinutes INT NULL DEFAULT 15,
    P2ResponseMinutes INT NULL DEFAULT 30,
    P3ResponseMinutes INT NULL DEFAULT 120,
    P4ResponseMinutes INT NULL DEFAULT 480,
    P1ResolutionMinutes INT NULL DEFAULT 240,
    P2ResolutionMinutes INT NULL DEFAULT 480,
    P3ResolutionMinutes INT NULL DEFAULT 1440,
    P4ResolutionMinutes INT NULL DEFAULT 7200,
    UseBusinessHours TINYINT(1) NOT NULL DEFAULT 1,
    BusinessHoursScheduleId INT NULL,
    Conditions TEXT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT NULL,
    ModifiedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_ITSMSLAPolicies_TargetType (TargetType),
    INDEX IX_ITSMSLAPolicies_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.3 ITSM SLA Instances
CREATE TABLE IF NOT EXISTS ITSMSLAInstances (
    SLAInstanceId INT AUTO_INCREMENT PRIMARY KEY,
    TargetId INT NOT NULL,
    TargetType INT NOT NULL DEFAULT 1,
    SLAPolicyId INT NOT NULL,
    ResponseDueAt DATETIME NULL,
    ResponseActualAt DATETIME NULL,
    ResponseBreached TINYINT(1) NOT NULL DEFAULT 0,
    ResponseBusinessMinutes INT NULL,
    ResolutionDueAt DATETIME NULL,
    ResolutionActualAt DATETIME NULL,
    ResolutionBreached TINYINT(1) NOT NULL DEFAULT 0,
    ResolutionBusinessMinutes INT NULL,
    State INT NOT NULL DEFAULT 1,
    PausedAt DATETIME NULL,
    PausedMinutes INT NOT NULL DEFAULT 0,
    PauseReason VARCHAR(500) NULL,
    ResponseWarning50Sent TINYINT(1) NOT NULL DEFAULT 0,
    ResponseWarning75Sent TINYINT(1) NOT NULL DEFAULT 0,
    ResponseBreachedSent TINYINT(1) NOT NULL DEFAULT 0,
    ResolutionWarning50Sent TINYINT(1) NOT NULL DEFAULT 0,
    ResolutionWarning75Sent TINYINT(1) NOT NULL DEFAULT 0,
    ResolutionBreachedSent TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL,
    INDEX IX_ITSMSLAInstances_TargetId_TargetType (TargetId, TargetType),
    INDEX IX_ITSMSLAInstances_SLAPolicyId (SLAPolicyId),
    INDEX IX_ITSMSLAInstances_State (State)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.4 Business Hours Schedules
CREATE TABLE IF NOT EXISTS BusinessHoursSchedules (
    ScheduleId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    TimeZone VARCHAR(100) NULL DEFAULT 'UTC',
    BusinessHours TEXT NULL,
    Holidays TEXT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_BusinessHoursSchedules_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.5 Catalog Categories
CREATE TABLE IF NOT EXISTS CatalogCategories (
    CategoryId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    IconName VARCHAR(100) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_CatalogCategories_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.6 Catalog Variables
CREATE TABLE IF NOT EXISTS CatalogVariables (
    VariableId INT AUTO_INCREMENT PRIMARY KEY,
    CatalogItemId INT NOT NULL,
    VariableName VARCHAR(100) NOT NULL,
    VariableLabel VARCHAR(200) NOT NULL,
    VariableType INT NOT NULL DEFAULT 1,
    IsRequired TINYINT(1) NOT NULL DEFAULT 0,
    ValidationRegex VARCHAR(500) NULL,
    ValidationMessage VARCHAR(500) NULL,
    MinLength INT NULL,
    MaxLength INT NULL,
    `MinValue` DECIMAL(18,2) NULL,
    `MaxValue` DECIMAL(18,2) NULL,
    Options TEXT NULL,
    DefaultValue VARCHAR(500) NULL,
    ShowWhen TEXT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    HelpText VARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_CatalogVariables_CatalogItemId (CatalogItemId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.7 Catalog Request Approvals
CREATE TABLE IF NOT EXISTS CatalogRequestApprovals (
    ApprovalId INT AUTO_INCREMENT PRIMARY KEY,
    CatalogRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    ApprovalDate DATETIME NULL,
    Comments TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_CatalogRequestApprovals_CatalogRequestId (CatalogRequestId),
    INDEX IX_CatalogRequestApprovals_ApproverId (ApproverId),
    INDEX IX_CatalogRequestApprovals_Status (ApprovalStatus)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.8 Catalog Request Comments
CREATE TABLE IF NOT EXISTS CatalogRequestComments (
    CommentId INT AUTO_INCREMENT PRIMARY KEY,
    CatalogRequestId INT NOT NULL,
    `Comment` TEXT NOT NULL,
    IsInternal TINYINT(1) NOT NULL DEFAULT 0,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_CatalogRequestComments_CatalogRequestId (CatalogRequestId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.9 Change Attachments
CREATE TABLE IF NOT EXISTS ChangeAttachments (
    AttachmentId INT AUTO_INCREMENT PRIMARY KEY,
    ChangeId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100) NULL,
    FileSize BIGINT NOT NULL DEFAULT 0,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_ChangeAttachments_ChangeId (ChangeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.10 ITSM Article Feedback
CREATE TABLE IF NOT EXISTS ITSMArticleFeedback (
    FeedbackId INT AUTO_INCREMENT PRIMARY KEY,
    ArticleId INT NOT NULL,
    UserId INT NULL,
    IsHelpful TINYINT(1) NOT NULL,
    `Comment` TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_ITSMArticleFeedback_ArticleId (ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.11 Article Incidents (junction: KnowledgeArticle ↔ Incident)
CREATE TABLE IF NOT EXISTS ArticleIncidents (
    ArticleIncidentId INT AUTO_INCREMENT PRIMARY KEY,
    ArticleId INT NOT NULL,
    IncidentId INT NOT NULL,
    UsedToResolve TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_ArticleIncidents_ArticleId (ArticleId),
    INDEX IX_ArticleIncidents_IncidentId (IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 2: CALENDAR INTEGRATION TABLES (BaseEntity pattern)
-- ============================================================================

-- 2.1 Calendar Integrations
CREATE TABLE IF NOT EXISTS CalendarIntegrations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Provider INT NOT NULL DEFAULT 0,
    AccessToken TEXT NOT NULL,
    RefreshToken TEXT NOT NULL,
    TokenExpiresAt DATETIME NOT NULL,
    CalendarId VARCHAR(500) NULL,
    CalendarName VARCHAR(200) NULL,
    ExternalEmail VARCHAR(255) NULL,
    SyncDirection INT NOT NULL DEFAULT 2,
    LastSyncAt DATETIME NULL,
    LastSyncStatus INT NOT NULL DEFAULT 3,
    LastSyncError VARCHAR(2000) NULL,
    NextSyncAt DATETIME NULL,
    SyncIntervalMinutes INT NOT NULL DEFAULT 15,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    SyncToken VARCHAR(500) NULL,
    LastSyncEventsCount INT NULL,
    TotalEventsSynced INT NOT NULL DEFAULT 0,
    SettingsJson TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_CalendarIntegrations_UserId (UserId),
    INDEX IX_CalendarIntegrations_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2.2 Calendar Sync Logs
CREATE TABLE IF NOT EXISTS CalendarSyncLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CalendarIntegrationId INT NOT NULL,
    StartedAt DATETIME NOT NULL,
    CompletedAt DATETIME NULL,
    Status INT NOT NULL DEFAULT 3,
    EventsCreated INT NOT NULL DEFAULT 0,
    EventsUpdated INT NOT NULL DEFAULT 0,
    EventsDeleted INT NOT NULL DEFAULT 0,
    ConflictsResolved INT NOT NULL DEFAULT 0,
    ErrorMessage VARCHAR(2000) NULL,
    ErrorStackTrace TEXT NULL,
    Direction INT NOT NULL DEFAULT 2,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_CalendarSyncLogs_IntegrationId (CalendarIntegrationId),
    INDEX IX_CalendarSyncLogs_Status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2.3 Calendar Event Mappings
CREATE TABLE IF NOT EXISTS CalendarEventMappings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ActivityId INT NOT NULL,
    CalendarIntegrationId INT NOT NULL,
    ExternalEventId VARCHAR(500) NOT NULL,
    ExternalEventUid VARCHAR(500) NULL,
    ExternalETag VARCHAR(200) NULL,
    LastSyncedAt DATETIME NOT NULL,
    ExternalLastModified DATETIME NULL,
    CrmLastModified DATETIME NULL,
    CreatedFromExternal TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_CalendarEventMappings_ActivityId (ActivityId),
    INDEX IX_CalendarEventMappings_IntegrationId (CalendarIntegrationId),
    INDEX IX_CalendarEventMappings_ExternalEventId (ExternalEventId(255))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 3: EMAIL INTEGRATION TABLES (BaseEntity pattern)
-- ============================================================================

-- 3.1 Email Integrations
CREATE TABLE IF NOT EXISTS EmailIntegrations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Provider INT NOT NULL DEFAULT 0,
    EmailAddress VARCHAR(255) NOT NULL,
    AccessToken TEXT NULL,
    RefreshToken TEXT NULL,
    TokenExpiresAt DATETIME NULL,
    ImapServer VARCHAR(255) NULL,
    ImapPort INT NULL,
    ImapUsername VARCHAR(255) NULL,
    ImapPassword TEXT NULL,
    UseSsl TINYINT(1) NOT NULL DEFAULT 1,
    LastSyncAt DATETIME NULL,
    LastSyncStatus INT NOT NULL DEFAULT 3,
    LastSyncError VARCHAR(2000) NULL,
    NextSyncAt DATETIME NULL,
    SyncIntervalMinutes INT NOT NULL DEFAULT 15,
    LastSyncToken VARCHAR(500) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    TotalEmailsSynced INT NOT NULL DEFAULT 0,
    SettingsJson TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EmailIntegrations_UserId (UserId),
    INDEX IX_EmailIntegrations_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3.2 Email Sync Logs
CREATE TABLE IF NOT EXISTS EmailSyncLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EmailIntegrationId INT NOT NULL,
    StartedAt DATETIME NOT NULL,
    CompletedAt DATETIME NULL,
    Status INT NOT NULL DEFAULT 3,
    EmailsCreated INT NOT NULL DEFAULT 0,
    EmailsUpdated INT NOT NULL DEFAULT 0,
    EmailsSkipped INT NOT NULL DEFAULT 0,
    ErrorMessage VARCHAR(2000) NULL,
    ErrorStackTrace TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EmailSyncLogs_IntegrationId (EmailIntegrationId),
    INDEX IX_EmailSyncLogs_Status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3.3 Email Message Mappings
CREATE TABLE IF NOT EXISTS EmailMessageMappings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CommunicationMessageId INT NOT NULL,
    EmailIntegrationId INT NOT NULL,
    ExternalMessageId VARCHAR(500) NOT NULL,
    ExternalThreadId VARCHAR(500) NULL,
    ExternalChangeKey VARCHAR(500) NULL,
    LastSyncedAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EmailMessageMappings_MessageId (CommunicationMessageId),
    INDEX IX_EmailMessageMappings_IntegrationId (EmailIntegrationId),
    INDEX IX_EmailMessageMappings_ExternalId (ExternalMessageId(255))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 4: EMAIL TEMPLATE VERSIONING TABLES
-- ============================================================================

-- 4.1 Email Template History Entries (BaseEntity)
CREATE TABLE IF NOT EXISTS EmailTemplateHistoryEntries (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TemplateId INT NOT NULL,
    Version INT NOT NULL,
    Subject TEXT NOT NULL,
    HtmlBody LONGTEXT NOT NULL,
    TextBody TEXT NULL,
    ChangeDescription TEXT NULL,
    CreatedById INT NULL,
    CreatedByName VARCHAR(200) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EmailTemplateHistoryEntries_TemplateId (TemplateId),
    INDEX IX_EmailTemplateHistoryEntries_Version (TemplateId, Version)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4.2 Email Template Usages (BaseEntity)
CREATE TABLE IF NOT EXISTS EmailTemplateUsages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TemplateId INT NOT NULL,
    UserId INT NULL,
    Context VARCHAR(500) NULL,
    UsedAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EmailTemplateUsages_TemplateId (TemplateId),
    INDEX IX_EmailTemplateUsages_UsedAt (UsedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4.3 Email Template Versions (standalone, defined in IEmailTemplateService)
CREATE TABLE IF NOT EXISTS EmailTemplateVersions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TemplateId INT NOT NULL,
    Version INT NOT NULL,
    Subject TEXT NOT NULL,
    HtmlBody LONGTEXT NOT NULL,
    TextBody TEXT NULL,
    ChangeDescription TEXT NULL,
    CreatedById INT NULL,
    CreatedByName VARCHAR(200) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IX_EmailTemplateVersions_TemplateId (TemplateId),
    INDEX IX_EmailTemplateVersions_TemplateVersion (TemplateId, Version)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 5: LANDING PAGE TABLES (BaseEntity pattern)
-- ============================================================================

-- 5.1 Landing Pages
CREATE TABLE IF NOT EXISTS LandingPages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Slug VARCHAR(300) NOT NULL,
    Title VARCHAR(300) NULL,
    MetaDescription VARCHAR(500) NULL,
    MetaKeywords VARCHAR(500) NULL,
    Template INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    ContentJson LONGTEXT NULL,
    HtmlContent LONGTEXT NULL,
    CustomCss TEXT NULL,
    CustomJs TEXT NULL,
    FeaturedImageUrl VARCHAR(500) NULL,
    FacebookPixelId VARCHAR(100) NULL,
    GoogleAnalyticsId VARCHAR(100) NULL,
    TrackingCode TEXT NULL,
    FormDefinitionId INT NULL,
    CampaignId INT NULL,
    ThankYouPageId INT NULL,
    RedirectUrl VARCHAR(500) NULL,
    CreatedByUserId INT NOT NULL,
    PublishedAt DATETIME NULL,
    ScheduledPublishAt DATETIME NULL,
    ScheduledUnpublishAt DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    ABTestVariant VARCHAR(50) NULL,
    OriginalPageId INT NULL,
    ABTestTrafficPercentage INT NULL,
    PageViews INT NOT NULL DEFAULT 0,
    UniqueVisitors INT NOT NULL DEFAULT 0,
    Conversions INT NOT NULL DEFAULT 0,
    ConversionRate DOUBLE NOT NULL DEFAULT 0,
    AverageTimeOnPage DOUBLE NOT NULL DEFAULT 0,
    BounceRate DECIMAL(18,2) NOT NULL DEFAULT 0,
    SettingsJson TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_LandingPages_Slug (Slug(255)),
    INDEX IX_LandingPages_Status (Status),
    INDEX IX_LandingPages_CampaignId (CampaignId),
    INDEX IX_LandingPages_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5.2 Landing Page Blocks
CREATE TABLE IF NOT EXISTS LandingPageBlocks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LandingPageId INT NOT NULL,
    BlockType INT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    ContentJson LONGTEXT NULL,
    StyleJson TEXT NULL,
    VisibilityCondition VARCHAR(500) NULL,
    IsVisible TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_LandingPageBlocks_PageId (LandingPageId),
    INDEX IX_LandingPageBlocks_SortOrder (LandingPageId, SortOrder)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5.3 Landing Page Visits
CREATE TABLE IF NOT EXISTS LandingPageVisits (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LandingPageId INT NOT NULL,
    VisitorId VARCHAR(200) NULL,
    IpAddressHash VARCHAR(100) NULL,
    UserAgent VARCHAR(500) NULL,
    Referrer VARCHAR(1000) NULL,
    UtmSource VARCHAR(200) NULL,
    UtmMedium VARCHAR(200) NULL,
    UtmCampaign VARCHAR(200) NULL,
    UtmTerm VARCHAR(200) NULL,
    UtmContent VARCHAR(200) NULL,
    VisitedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TimeOnPageSeconds INT NULL,
    Converted TINYINT(1) NOT NULL DEFAULT 0,
    ConvertedAt DATETIME NULL,
    LeadId INT NULL,
    DeviceType VARCHAR(50) NULL,
    Browser VARCHAR(100) NULL,
    OperatingSystem VARCHAR(100) NULL,
    Country VARCHAR(100) NULL,
    City VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_LandingPageVisits_PageId (LandingPageId),
    INDEX IX_LandingPageVisits_VisitedAt (VisitedAt),
    INDEX IX_LandingPageVisits_Converted (Converted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 6: EVENT ATTENDEES (BaseEntity pattern)
-- ============================================================================

CREATE TABLE IF NOT EXISTS EventAttendees (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ActivityId INT NOT NULL,
    AttendeeType INT NOT NULL DEFAULT 0,
    AttendeeId INT NOT NULL,
    AttendeeEmail VARCHAR(255) NULL,
    AttendeeName VARCHAR(200) NULL,
    ResponseStatus INT NOT NULL DEFAULT 0,
    RespondedAt DATETIME NULL,
    ResponseComment VARCHAR(500) NULL,
    IsOrganizer TINYINT(1) NOT NULL DEFAULT 0,
    IsRequired TINYINT(1) NOT NULL DEFAULT 1,
    Role VARCHAR(100) NULL,
    DidAttend TINYINT(1) NULL,
    AttendanceDurationMinutes INT NULL,
    AttendanceNotes VARCHAR(1000) NULL,
    ExternalCalendarEventId VARCHAR(500) NULL,
    InvitationSentAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_EventAttendees_ActivityId (ActivityId),
    INDEX IX_EventAttendees_AttendeeId (AttendeeType, AttendeeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 7: LEAD SCORE RULES (BaseEntity pattern)
-- ============================================================================

CREATE TABLE IF NOT EXISTS LeadScoreRules (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    RuleType INT NOT NULL DEFAULT 0,
    FieldName VARCHAR(200) NULL,
    `Operator` INT NOT NULL DEFAULT 0,
    Value VARCHAR(500) NULL,
    ConditionsJson TEXT NULL,
    ScoreImpact INT NOT NULL DEFAULT 10,
    MaxApplications INT NULL,
    DecayDaysThreshold INT NULL,
    DecayPointsPerPeriod INT NULL,
    DecayPeriodDays INT NULL DEFAULT 7,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 100,
    Category VARCHAR(100) NULL,
    ActionType VARCHAR(100) NULL,
    ActionIdentifier VARCHAR(200) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_LeadScoreRules_RuleType (RuleType),
    INDEX IX_LeadScoreRules_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 8: SUBSCRIPTION TABLE (BaseEntity pattern)
-- Note: The Subscription entity has [Table("Accounts")] which is incorrect.
-- Creating as "Subscriptions" to match the DbSet name. A code fix for the
-- [Table] attribute mapping will be applied separately.
-- ============================================================================

CREATE TABLE IF NOT EXISTS Subscriptions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SubscriptionNumber VARCHAR(50) NULL,
    AccountId INT NOT NULL,
    ProductId INT NULL,
    SubscriptionStatus INT NOT NULL DEFAULT 0,
    MRR DECIMAL(18,2) NULL,
    ARR DECIMAL(18,2) NULL,
    OneTimeFee DECIMAL(18,2) NULL,
    Currency VARCHAR(10) NULL,
    CurrencyLookupId INT NULL,
    BillingCycle VARCHAR(50) NULL,
    BillingPeriod VARCHAR(50) NULL,
    BillingStartDate DATETIME NULL,
    BillingEndDate DATETIME NULL,
    ContractReference VARCHAR(100) NULL,
    ContractStartDate DATETIME NULL,
    ContractEndDate DATETIME NULL,
    TermCategory INT NULL,
    ServiceTier INT NULL,
    SLA VARCHAR(100) NULL,
    ContractNotes TEXT NULL,
    BillingAddress VARCHAR(500) NULL,
    BillingCity VARCHAR(100) NULL,
    BillingState VARCHAR(100) NULL,
    BillingZip VARCHAR(20) NULL,
    BillingCountry VARCHAR(100) NULL,
    BillingContactName VARCHAR(200) NULL,
    BillingContactEmail VARCHAR(255) NULL,
    BillingContactPhone VARCHAR(50) NULL,
    ContractFileName VARCHAR(255) NULL,
    ContractFilePath VARCHAR(500) NULL,
    ContractContentType VARCHAR(100) NULL,
    ContractFileSize BIGINT NULL,
    IsAutoRenew TINYINT(1) NOT NULL DEFAULT 0,
    RenewalDate DATETIME NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    SubscriptionOwner VARCHAR(200) NULL,
    SubscriptionManagerId INT NULL,
    Tags VARCHAR(500) NULL,
    ExternalReference VARCHAR(200) NULL,
    OrderId INT NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartDate DATETIME NULL,
    EndDate DATETIME NULL,
    NextBillingDate DATETIME NULL,
    CurrentPeriodStart DATETIME NULL,
    CurrentPeriodEnd DATETIME NULL,
    CancelledAt DATETIME NULL,
    CancellationReason TEXT NULL,
    CancelAtPeriodEnd TINYINT(1) NOT NULL DEFAULT 0,
    PausedAt DATETIME NULL,
    PauseReason TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_Subscriptions_AccountId (AccountId),
    INDEX IX_Subscriptions_Status (SubscriptionStatus),
    INDEX IX_Subscriptions_SubscriptionNumber (SubscriptionNumber),
    INDEX IX_Subscriptions_IsActive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 9: SUBSCRIPTION USAGE LIMITS (BaseEntity pattern)
-- ============================================================================

CREATE TABLE IF NOT EXISTS SubscriptionUsageLimits (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SubscriptionId INT NOT NULL,
    MetricName VARCHAR(200) NOT NULL,
    `Limit` DECIMAL(18,2) NOT NULL DEFAULT 0,
    Unit VARCHAR(50) NULL,
    EnforceCap TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_SubscriptionUsageLimits_SubscriptionId (SubscriptionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- GROUP 10: WORKFLOW TRIGGERS (BaseEntity pattern)
-- ============================================================================

CREATE TABLE IF NOT EXISTS WorkflowTriggers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowDefinitionId INT NOT NULL,
    Name VARCHAR(200) NOT NULL,
    TriggerType INT NOT NULL DEFAULT 0,
    EntityType VARCHAR(100) NULL,
    EventName VARCHAR(200) NULL,
    CronExpression VARCHAR(100) NULL,
    FilterConditions TEXT NULL,
    WatchedField VARCHAR(200) NULL,
    OldValue VARCHAR(500) NULL,
    NewValue VARCHAR(500) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 100,
    Description VARCHAR(500) NULL,
    LastTriggeredAt DATETIME NULL,
    NextScheduledAt DATETIME NULL,
    ExecutionCount INT NOT NULL DEFAULT 0,
    DelaySeconds INT NOT NULL DEFAULT 0,
    RunAsync TINYINT(1) NOT NULL DEFAULT 1,
    MaxRetries INT NOT NULL DEFAULT 3,
    CreatedById INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    RowVersion VARBINARY(8) NULL,
    INDEX IX_WorkflowTriggers_DefinitionId (WorkflowDefinitionId),
    INDEX IX_WorkflowTriggers_TriggerType (TriggerType),
    INDEX IX_WorkflowTriggers_IsActive (IsActive),
    INDEX IX_WorkflowTriggers_EntityType (EntityType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- SUMMARY: 27 tables created
-- ============================================================================
-- ITSM (11): ITSMKnowledgeArticles, ITSMSLAPolicies, ITSMSLAInstances,
--            BusinessHoursSchedules, CatalogCategories, CatalogVariables,
--            CatalogRequestApprovals, CatalogRequestComments, ChangeAttachments,
--            ITSMArticleFeedback, ArticleIncidents
-- Calendar (3): CalendarIntegrations, CalendarSyncLogs, CalendarEventMappings
-- Email (3): EmailIntegrations, EmailSyncLogs, EmailMessageMappings
-- Email Template (3): EmailTemplateHistoryEntries, EmailTemplateUsages, EmailTemplateVersions
-- Landing Pages (3): LandingPages, LandingPageBlocks, LandingPageVisits
-- Events (1): EventAttendees
-- Lead Scoring (1): LeadScoreRules
-- Subscription (2): Subscriptions, SubscriptionUsageLimits
-- Workflow (1): WorkflowTriggers

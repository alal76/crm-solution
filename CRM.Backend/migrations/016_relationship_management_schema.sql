-- CRM Solution - Relationship Management Schema
-- Migration: 016_relationship_management_schema.sql
-- Created: 2026-01-28
-- Purpose: Add comprehensive B2B/B2C relationship tracking system

-- ===================================================================
-- RELATIONSHIP MANAGEMENT SCHEMA
-- This is a completely separate feature from contact management,
-- designed to track complex business relationships, account hierarchies,
-- partnerships, and multi-stakeholder interactions.
-- ===================================================================

-- Relationship types and mapping
-- Defines the types of relationships that can exist between accounts
CREATE TABLE IF NOT EXISTS RelationshipTypes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TypeName VARCHAR(100) NOT NULL UNIQUE,
    TypeCategory VARCHAR(50) NULL, -- business, partnership, hierarchy, dependency
    Description TEXT NULL,
    IsBidirectional TINYINT(1) DEFAULT 0,
    ReverseTypeName VARCHAR(100) NULL, -- For bidirectional relationships
    Icon VARCHAR(50) NULL,
    Color VARCHAR(20) NULL,
    IsActive TINYINT(1) DEFAULT 1,
    IsSystem TINYINT(1) DEFAULT 0, -- System-defined types cannot be deleted
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    CONSTRAINT FK_RelationshipTypes_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX idx_relationship_types_category (TypeCategory),
    INDEX idx_relationship_types_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert default relationship types
INSERT IGNORE INTO RelationshipTypes (TypeName, TypeCategory, IsBidirectional, ReverseTypeName, Icon, Color, IsSystem, DisplayOrder, Description) VALUES
    ('Parent Company', 'hierarchy', 1, 'Subsidiary', 'Business', '#1976D2', 1, 1, 'Parent-subsidiary corporate relationship'),
    ('Subsidiary', 'hierarchy', 1, 'Parent Company', 'AccountTree', '#1976D2', 1, 2, 'Subsidiary of a parent company'),
    ('Strategic Partner', 'partnership', 1, 'Strategic Partner', 'Handshake', '#4CAF50', 1, 3, 'Strategic business partnership'),
    ('Technology Partner', 'partnership', 1, 'Technology Partner', 'IntegrationInstructions', '#9C27B0', 1, 4, 'Technology integration partnership'),
    ('Integration Partner', 'partnership', 1, 'Integration Partner', 'Cable', '#673AB7', 1, 5, 'System integration partnership'),
    ('Vendor', 'business', 1, 'Customer', 'Store', '#FF9800', 1, 6, 'Vendor/supplier relationship'),
    ('Customer', 'business', 1, 'Vendor', 'Person', '#2196F3', 1, 7, 'Customer relationship'),
    ('Reseller', 'business', 0, NULL, 'Storefront', '#E91E63', 1, 8, 'Reseller/distributor relationship'),
    ('Referral Source', 'business', 0, NULL, 'Share', '#00BCD4', 1, 9, 'Referral or lead source'),
    ('Competitor', 'business', 1, 'Competitor', 'CompareArrows', '#F44336', 1, 10, 'Competitive relationship'),
    ('Service Provider', 'business', 1, 'Client', 'Support', '#795548', 1, 11, 'Service provider relationship'),
    ('Client', 'business', 1, 'Service Provider', 'PersonOutline', '#607D8B', 1, 12, 'Client of a service provider'),
    ('Influencer', 'business', 0, NULL, 'TrendingUp', '#FFEB3B', 1, 13, 'Industry influencer'),
    ('Affiliate', 'business', 0, NULL, 'Link', '#8BC34A', 1, 14, 'Affiliate marketing relationship'),
    ('Consultant', 'business', 0, NULL, 'School', '#3F51B5', 1, 15, 'Consulting relationship');

-- Account-to-Account relationships
-- Links two accounts together with a specific relationship type
CREATE TABLE IF NOT EXISTS AccountRelationships (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SourceCustomerId INT NOT NULL, -- The "from" account
    TargetCustomerId INT NOT NULL, -- The "to" account
    RelationshipTypeId INT NOT NULL,
    
    -- Relationship strength and status
    Status VARCHAR(50) DEFAULT 'Active', -- Active, Inactive, Pending, Terminated
    StrengthScore INT DEFAULT 50, -- 0-100
    StrategicImportance VARCHAR(50) DEFAULT 'Medium', -- Critical, High, Medium, Low
    
    -- Dates
    RelationshipStartDate DATE NULL,
    RelationshipEndDate DATE NULL,
    LastReviewedDate DATE NULL,
    NextReviewDate DATE NULL,
    
    -- Financial impact
    AnnualRevenueImpact DECIMAL(15,2) NULL,
    CostSavings DECIMAL(15,2) NULL,
    
    -- Details
    Description TEXT NULL,
    Notes TEXT NULL,
    TermsConditions TEXT NULL,
    
    -- Metadata
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    -- Prevent self-relationships
    CONSTRAINT chk_no_self_relationship CHECK (SourceCustomerId != TargetCustomerId),
    -- Prevent duplicate relationships of same type
    CONSTRAINT uk_account_relationship UNIQUE (SourceCustomerId, TargetCustomerId, RelationshipTypeId),
    
    CONSTRAINT FK_AccountRelationships_Source FOREIGN KEY (SourceCustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountRelationships_Target FOREIGN KEY (TargetCustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountRelationships_Type FOREIGN KEY (RelationshipTypeId) REFERENCES RelationshipTypes(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_AccountRelationships_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_AccountRelationships_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_relationships_source (SourceCustomerId),
    INDEX idx_relationships_target (TargetCustomerId),
    INDEX idx_relationships_type (RelationshipTypeId),
    INDEX idx_relationships_status (Status),
    INDEX idx_relationships_importance (StrategicImportance)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Relationship interactions/touchpoints
-- Tracks interactions and activities within a relationship
CREATE TABLE IF NOT EXISTS RelationshipInteractions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    AccountRelationshipId INT NOT NULL,
    InteractionType VARCHAR(100) NOT NULL, -- meeting, call, email, event, contract_renewal, issue_resolution
    Subject VARCHAR(255) NULL,
    Description TEXT NULL,
    InteractionDate DATETIME NOT NULL,
    DurationMinutes INT NULL,
    
    -- Participants (JSON arrays of IDs)
    ParticipantContactIds TEXT NULL, -- JSON array of contact IDs involved
    ParticipantUserIds TEXT NULL, -- JSON array of internal user IDs
    
    -- Outcomes
    Outcome VARCHAR(100) NULL, -- successful, needs_followup, escalated, resolved
    ActionItems TEXT NULL,
    NextSteps TEXT NULL,
    FollowUpDate DATE NULL,
    
    -- Sentiment and health
    SentimentScore INT DEFAULT 0, -- -100 to +100
    HealthImpact VARCHAR(50) DEFAULT 'Neutral', -- Positive, Neutral, Negative, Critical
    
    -- Location and context
    Location VARCHAR(255) NULL,
    MeetingLink VARCHAR(500) NULL,
    
    -- Metadata (JSON for flexible additional data)
    Metadata TEXT NULL,
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_RelationshipInteractions_Relationship FOREIGN KEY (AccountRelationshipId) REFERENCES AccountRelationships(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RelationshipInteractions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_rel_interactions_relationship (AccountRelationshipId),
    INDEX idx_rel_interactions_date (InteractionDate),
    INDEX idx_rel_interactions_type (InteractionType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Account health tracking over time
-- Periodic snapshots of account health metrics
CREATE TABLE IF NOT EXISTS AccountHealthSnapshots (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CustomerId INT NOT NULL,
    SnapshotDate DATE NOT NULL,
    
    -- Health metrics (0-100 scale)
    OverallHealthScore INT DEFAULT 0,
    EngagementScore INT DEFAULT 0,
    ProductAdoptionScore INT DEFAULT 0,
    SupportSatisfactionScore INT DEFAULT 0,
    FinancialHealthScore INT DEFAULT 0,
    RelationshipScore INT DEFAULT 0,
    
    -- Key indicators
    ActiveUsersCount INT NULL,
    FeatureAdoptionRate DECIMAL(5,2) NULL,
    SupportTicketsCount INT NULL,
    SupportTicketsResolved INT NULL,
    AverageResponseTimeHours DECIMAL(8,2) NULL,
    NPSScore INT NULL, -- Net Promoter Score (-100 to 100)
    
    -- Risk factors (JSON arrays)
    RiskFactors TEXT NULL,
    WarningSignals TEXT NULL,
    GrowthIndicators TEXT NULL,
    
    -- Notes
    AnalystNotes TEXT NULL,
    
    -- Trend comparison
    PreviousHealthScore INT NULL,
    HealthTrend VARCHAR(20) DEFAULT 'Stable', -- Improving, Stable, Declining, Critical
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT uk_health_snapshot UNIQUE (CustomerId, SnapshotDate),
    CONSTRAINT FK_AccountHealthSnapshots_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AccountHealthSnapshots_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_health_snapshots_customer (CustomerId),
    INDEX idx_health_snapshots_date (SnapshotDate),
    INDEX idx_health_snapshots_score (OverallHealthScore)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Relationship maps for visualization
-- Saved relationship map configurations
CREATE TABLE IF NOT EXISTS RelationshipMaps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    MapName VARCHAR(255) NOT NULL,
    Description TEXT NULL,
    
    -- Focus
    CentralCustomerId INT NULL, -- The account at center of the map
    RelationshipDepth INT DEFAULT 2, -- How many levels to show
    
    -- Filters (JSON arrays of IDs/values)
    IncludeRelationshipTypeIds TEXT NULL,
    ExcludeRelationshipTypeIds TEXT NULL,
    MinRelationshipStrength INT DEFAULT 0,
    IncludeStatuses TEXT NULL, -- JSON array of statuses to include
    
    -- Date range
    DateRangeStart DATE NULL,
    DateRangeEnd DATE NULL,
    
    -- Layout and visualization (JSON)
    LayoutConfig TEXT NULL, -- Node positions, colors, etc.
    ViewSettings TEXT NULL, -- Zoom level, pan position, etc.
    
    -- Sharing
    IsPublic TINYINT(1) DEFAULT 0,
    SharedWithUserIds TEXT NULL, -- JSON array of user IDs
    SharedWithGroupIds TEXT NULL, -- JSON array of group IDs
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_RelationshipMaps_Central FOREIGN KEY (CentralCustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_RelationshipMaps_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_relationship_maps_creator (CreatedBy),
    INDEX idx_relationship_maps_public (IsPublic)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Territory assignments for account management
CREATE TABLE IF NOT EXISTS AccountTerritories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TerritoryName VARCHAR(255) NOT NULL,
    TerritoryCode VARCHAR(50) NULL UNIQUE,
    Description TEXT NULL,
    
    -- Geographic (JSON arrays)
    Countries TEXT NULL,
    Regions TEXT NULL,
    States TEXT NULL,
    Cities TEXT NULL,
    
    -- Segmentation (JSON arrays)
    Industries TEXT NULL,
    CustomerTypes TEXT NULL,
    RevenueRangeMin DECIMAL(15,2) NULL,
    RevenueRangeMax DECIMAL(15,2) NULL,
    
    -- Assignment
    PrimaryOwnerId INT NULL, -- Primary owner user
    TeamMemberIds TEXT NULL, -- JSON array of user IDs
    
    -- Quota and targets
    AnnualQuota DECIMAL(15,2) NULL,
    QuotaCurrency VARCHAR(10) DEFAULT 'USD',
    TargetAccountCount INT NULL,
    
    -- Status
    IsActive TINYINT(1) DEFAULT 1,
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_AccountTerritories_Owner FOREIGN KEY (PrimaryOwnerId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_AccountTerritories_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_territories_owner (PrimaryOwnerId),
    INDEX idx_territories_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Territory to customer assignments (many-to-many)
CREATE TABLE IF NOT EXISTS CustomerTerritoryAssignments (
    CustomerId INT NOT NULL,
    TerritoryId INT NOT NULL,
    AssignedDate DATE DEFAULT (CURRENT_DATE),
    IsPrimary TINYINT(1) DEFAULT 1, -- Is this the primary territory for the customer?
    AssignedBy INT NULL,
    Notes VARCHAR(500) NULL,
    
    PRIMARY KEY (CustomerId, TerritoryId),
    
    CONSTRAINT FK_CTA_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CTA_Territory FOREIGN KEY (TerritoryId) REFERENCES AccountTerritories(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CTA_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_cta_territory (TerritoryId),
    INDEX idx_cta_primary (IsPrimary)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================================================================
-- CAMPAIGN EXECUTION TABLES
-- Enhanced campaign management with workflow integration
-- ===================================================================

-- Campaign recipients/members
CREATE TABLE IF NOT EXISTS CampaignRecipients (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampaignId INT NOT NULL,
    ContactId INT NULL, -- Link to Contact if applicable
    CustomerId INT NULL, -- Link to Customer if applicable
    
    -- Recipient info (denormalized for performance)
    Email VARCHAR(255) NULL,
    FirstName VARCHAR(100) NULL,
    LastName VARCHAR(100) NULL,
    Company VARCHAR(255) NULL,
    
    -- Status
    Status VARCHAR(50) DEFAULT 'Pending', -- Pending, Sent, Delivered, Failed, Bounced
    SendScheduledTime DATETIME NULL,
    SendActualTime DATETIME NULL,
    
    -- Engagement tracking
    DeliveredAt DATETIME NULL,
    FirstOpenedAt DATETIME NULL,
    LastOpenedAt DATETIME NULL,
    OpenCount INT DEFAULT 0,
    FirstClickedAt DATETIME NULL,
    LastClickedAt DATETIME NULL,
    ClickCount INT DEFAULT 0,
    ConvertedAt DATETIME NULL,
    ConversionValue DECIMAL(12,2) NULL,
    UnsubscribedAt DATETIME NULL,
    
    -- Delivery details
    BounceType VARCHAR(50) NULL, -- Hard, Soft, Technical
    BounceReason TEXT NULL,
    ErrorMessage TEXT NULL,
    
    -- Personalization data snapshot (JSON)
    PersonalizationData TEXT NULL,
    
    -- A/B Test variant
    ABTestVariant VARCHAR(10) NULL, -- A, B, C, etc.
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_CampaignRecipients_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignRecipients_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CampaignRecipients_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    
    INDEX idx_recipients_campaign (CampaignId),
    INDEX idx_recipients_contact (ContactId),
    INDEX idx_recipients_customer (CustomerId),
    INDEX idx_recipients_status (Status),
    INDEX idx_recipients_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Campaign link click tracking
CREATE TABLE IF NOT EXISTS CampaignLinkClicks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampaignRecipientId INT NOT NULL,
    CampaignId INT NOT NULL,
    
    LinkUrl TEXT NOT NULL,
    LinkLabel VARCHAR(255) NULL,
    ClickedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Device/browser info
    UserAgent TEXT NULL,
    IpAddress VARCHAR(50) NULL,
    DeviceType VARCHAR(50) NULL, -- Desktop, Mobile, Tablet
    Browser VARCHAR(100) NULL,
    OperatingSystem VARCHAR(100) NULL,
    
    -- Location data (JSON)
    LocationData TEXT NULL,
    
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_CampaignLinkClicks_Recipient FOREIGN KEY (CampaignRecipientId) REFERENCES CampaignRecipients(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignLinkClicks_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    
    INDEX idx_clicks_recipient (CampaignRecipientId),
    INDEX idx_clicks_campaign (CampaignId),
    INDEX idx_clicks_time (ClickedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Campaign A/B tests
CREATE TABLE IF NOT EXISTS CampaignABTests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampaignId INT NOT NULL,
    TestName VARCHAR(255) NOT NULL,
    TestType VARCHAR(50) NOT NULL, -- SubjectLine, FromName, Content, SendTime
    TestMetric VARCHAR(50) NOT NULL, -- OpenRate, ClickRate, ConversionRate
    
    -- Traffic split (JSON: {"A": 50, "B": 50})
    TrafficSplit TEXT NULL,
    SampleSize INT NULL,
    SamplePercentage DECIMAL(5,2) NULL,
    
    -- Variants configuration (JSON)
    VariantConfigs TEXT NULL,
    
    -- Results
    WinnerVariant VARCHAR(10) NULL,
    WinningCriteria TEXT NULL,
    ConfidenceLevel DECIMAL(5,2) NULL, -- Statistical confidence %
    
    -- Timing
    TestStartedAt DATETIME NULL,
    TestCompletedAt DATETIME NULL,
    WinnerDeployedAt DATETIME NULL,
    
    -- Auto-winner settings
    AutoSelectWinner TINYINT(1) DEFAULT 0,
    AutoWinnerAfterHours INT NULL,
    
    Status VARCHAR(50) DEFAULT 'Draft', -- Draft, Running, Completed, Cancelled
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_CampaignABTests_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignABTests_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_abtests_campaign (CampaignId),
    INDEX idx_abtests_status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Campaign conversions tracking
CREATE TABLE IF NOT EXISTS CampaignConversions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampaignId INT NOT NULL,
    CampaignRecipientId INT NULL,
    ContactId INT NULL,
    CustomerId INT NULL,
    
    ConversionType VARCHAR(100) NOT NULL, -- Purchase, Signup, Download, FormSubmit, Demo, Trial
    ConversionValue DECIMAL(12,2) NULL,
    ConversionCurrency VARCHAR(10) DEFAULT 'USD',
    
    -- Attribution
    AttributionModel VARCHAR(50) DEFAULT 'LastTouch', -- FirstTouch, LastTouch, Linear, TimeDecay, Custom
    AttributionPercentage DECIMAL(5,2) DEFAULT 100,
    
    -- Conversion data (JSON)
    ConversionData TEXT NULL,
    
    ConvertedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- External references
    ExternalOrderId VARCHAR(100) NULL,
    ExternalTransactionId VARCHAR(100) NULL,
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_CampaignConversions_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignConversions_Recipient FOREIGN KEY (CampaignRecipientId) REFERENCES CampaignRecipients(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CampaignConversions_Contact FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE SET NULL,
    CONSTRAINT FK_CampaignConversions_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
    
    INDEX idx_conversions_campaign (CampaignId),
    INDEX idx_conversions_recipient (CampaignRecipientId),
    INDEX idx_conversions_type (ConversionType),
    INDEX idx_conversions_time (ConvertedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Campaign workflow association
-- Links campaigns to workflow definitions for automated execution
CREATE TABLE IF NOT EXISTS CampaignWorkflows (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CampaignId INT NOT NULL,
    WorkflowDefinitionId INT NOT NULL,
    
    WorkflowType VARCHAR(50) NOT NULL, -- TriggerBased, Scheduled, Sequential
    TriggerEvent VARCHAR(100) NULL, -- ContactCreated, FormSubmitted, EmailOpened, etc.
    TriggerConditions TEXT NULL, -- JSON conditions
    
    IsActive TINYINT(1) DEFAULT 1,
    Priority INT DEFAULT 0,
    
    -- Execution settings
    MaxExecutionsPerContact INT DEFAULT 1,
    CooldownHours INT DEFAULT 0, -- Minimum hours between executions for same contact
    
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    CreatedBy INT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    
    CONSTRAINT FK_CampaignWorkflows_Campaign FOREIGN KEY (CampaignId) REFERENCES MarketingCampaigns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignWorkflows_Workflow FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CampaignWorkflows_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    INDEX idx_campaign_workflows_campaign (CampaignId),
    INDEX idx_campaign_workflows_workflow (WorkflowDefinitionId),
    INDEX idx_campaign_workflows_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ===================================================================
-- Add fields to existing Customers table for relationship tracking
-- ===================================================================

-- Add relationship-focused fields to Customer entity
ALTER TABLE Customers 
    ADD COLUMN IF NOT EXISTS AccountHealthScore INT DEFAULT 0,
    ADD COLUMN IF NOT EXISTS RiskLevel VARCHAR(50) DEFAULT 'Low',
    ADD COLUMN IF NOT EXISTS LastInteractionDate DATETIME NULL,
    ADD COLUMN IF NOT EXISTS LastInteractionType VARCHAR(100) NULL,
    ADD COLUMN IF NOT EXISTS InteractionFrequency VARCHAR(50) NULL,
    ADD COLUMN IF NOT EXISTS PreferredContactMethod VARCHAR(50) NULL,
    ADD COLUMN IF NOT EXISTS AccountTier VARCHAR(50) NULL,
    ADD COLUMN IF NOT EXISTS TotalContractValue DECIMAL(15,2) NULL,
    ADD COLUMN IF NOT EXISTS NextRenewalDate DATE NULL,
    ADD COLUMN IF NOT EXISTS AccountNotes TEXT NULL;

-- Add index for health score queries
CREATE INDEX IF NOT EXISTS idx_customers_health_score ON Customers(AccountHealthScore);
CREATE INDEX IF NOT EXISTS idx_customers_risk_level ON Customers(RiskLevel);
CREATE INDEX IF NOT EXISTS idx_customers_tier ON Customers(AccountTier);

-- ===================================================================
-- END OF RELATIONSHIP MANAGEMENT SCHEMA
-- ===================================================================

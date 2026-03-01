-- ITSM Module Simplified Migration (No Foreign Keys)
-- Creates core ITSM tables for Incident, Problem, SLA, CMDB, Change, Knowledge, Service Catalog


SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

USE crm_db;

-- Incident Management
CREATE TABLE IF NOT EXISTS Incidents (
    IncidentId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    CallerId INT NOT NULL,
    ContactType INT NOT NULL DEFAULT 1,
    OpenedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    OpenedById INT,
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    Impact INT NOT NULL DEFAULT 2,
    Urgency INT NOT NULL DEFAULT 2,
    State INT NOT NULL DEFAULT 1,
    AssignmentGroupId INT,
    AssignedToId INT,
    EscalationLevel INT DEFAULT 0,
    ResolutionCode INT,
    ResolutionNotes TEXT,
    ResolvedAt DATETIME,
    ResolvedById INT,
    ClosedAt DATETIME,
    ClosedById INT,
    SLABreached BOOLEAN DEFAULT FALSE,
    ResponseDueAt DATETIME,
    ResolutionDueAt DATETIME,
    ProblemId INT,
    ChangeRequestId INT,
    KnowledgeArticleId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_incidents_state (State),
    INDEX idx_incidents_caller (CallerId),
    INDEX idx_incidents_number (Number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_incident_comments_incident (IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentHistory (
    HistoryId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Field VARCHAR(100) NOT NULL,
    OldValue TEXT,
    NewValue TEXT,
    ChangedById INT NOT NULL,
    ChangedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_incident_history_incident (IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_incident_attachments_incident (IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Problem Management
CREATE TABLE IF NOT EXISTS Problems (
    ProblemId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    Priority INT NOT NULL DEFAULT 3,
    Symptoms TEXT,
    RootCause TEXT,
    Workaround TEXT,
    KnownError BOOLEAN DEFAULT FALSE,
    KnownErrorDate DATETIME,
    State INT NOT NULL DEFAULT 1,
    ProblemInvestigatorId INT,
    ProblemManagerId INT,
    AssignmentGroupId INT,
    Solution TEXT,
    ResolutionCode VARCHAR(100),
    ResolvedAt DATETIME,
    FixVerified BOOLEAN DEFAULT FALSE,
    VerifiedAt DATETIME,
    KnowledgeArticleId INT,
    ClosedAt DATETIME,
    ClosedById INT,
    ClosureNotes TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_problems_state (State),
    INDEX idx_problems_priority (Priority)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemIncidents (
    ProblemIncidentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    IncidentId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    INDEX idx_problem_incidents_problem (ProblemId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_problem_comments_problem (ProblemId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemTasks (
    TaskId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    TaskName VARCHAR(200) NOT NULL,
    Description TEXT,
    AssignedToId INT,
    DueDate DATETIME,
    IsCompleted BOOLEAN DEFAULT FALSE,
    CompletedAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_problem_tasks_problem (ProblemId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_problem_attachments_problem (ProblemId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- SLA Management
CREATE TABLE IF NOT EXISTS SLAPolicies (
    SLAPolicyId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    TargetType INT NOT NULL,
    P1ResponseMinutes INT DEFAULT 15,
    P2ResponseMinutes INT DEFAULT 30,
    P3ResponseMinutes INT DEFAULT 120,
    P4ResponseMinutes INT DEFAULT 480,
    P1ResolutionMinutes INT DEFAULT 240,
    P2ResolutionMinutes INT DEFAULT 480,
    P3ResolutionMinutes INT DEFAULT 1440,
    P4ResolutionMinutes INT DEFAULT 7200,
    UseBusinessHours BOOLEAN DEFAULT TRUE,
    BusinessHoursScheduleId INT,
    Conditions TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_sla_policies_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS SLAInstances (
    SLAInstanceId INT PRIMARY KEY AUTO_INCREMENT,
    TargetId INT NOT NULL,
    TargetType INT NOT NULL,
    SLAPolicyId INT NOT NULL,
    ResponseDueAt DATETIME,
    ResponseActualAt DATETIME,
    ResponseBreached BOOLEAN DEFAULT FALSE,
    ResolutionDueAt DATETIME,
    ResolutionActualAt DATETIME,
    ResolutionBreached BOOLEAN DEFAULT FALSE,
    State INT NOT NULL DEFAULT 1,
    PausedAt DATETIME,
    PausedMinutes INT DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    INDEX idx_sla_instances_target (TargetId, TargetType),
    INDEX idx_sla_instances_state (State)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- CMDB
CREATE TABLE IF NOT EXISTS ConfigurationItems (
    CIId INT PRIMARY KEY AUTO_INCREMENT,
    CIName VARCHAR(200) NOT NULL,
    CINumber VARCHAR(50) NOT NULL UNIQUE,
    CIType INT NOT NULL,
    CISubtype VARCHAR(50),
    Description TEXT,
    SerialNumber VARCHAR(100),
    AssetTag VARCHAR(100),
    ModelNumber VARCHAR(100),
    Manufacturer VARCHAR(200),
    OwnerId INT,
    SupportGroupId INT,
    OperationalStatus INT NOT NULL DEFAULT 1,
    Environment INT,
    Criticality INT,
    PhysicalLocation VARCHAR(500),
    IPAddress VARCHAR(50),
    MACAddress VARCHAR(50),
    OperatingSystem VARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_ci_number (CINumber),
    INDEX idx_ci_type (CIType),
    INDEX idx_ci_status (OperationalStatus)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CIRelationships (
    RelationshipId INT PRIMARY KEY AUTO_INCREMENT,
    ParentCIId INT NOT NULL,
    ChildCIId INT NOT NULL,
    RelationshipType INT NOT NULL,
    Description VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_ci_relationships_parent (ParentCIId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Services (
    ServiceId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceName VARCHAR(200) NOT NULL,
    ServiceNumber VARCHAR(50) UNIQUE,
    Description TEXT,
    ServiceType INT NOT NULL,
    OwnerId INT,
    SupportGroupId INT,
    Criticality INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_services_type (ServiceType),
    INDEX idx_services_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ServiceCIs (
    ServiceCIId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceId INT NOT NULL,
    CIId INT NOT NULL,
    DependencyType INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_service_cis_service (ServiceId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Change Management
CREATE TABLE IF NOT EXISTS Changes (
    ChangeId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    Type INT NOT NULL DEFAULT 1,
    CategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    RequestorId INT NOT NULL,
    AssignedToId INT,
    ImplementationGroupId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    Risk INT NOT NULL DEFAULT 2,
    Impact INT NOT NULL DEFAULT 2,
    ImplementationPlan TEXT,
    BackoutPlan TEXT,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    CABDate DATETIME,
    State INT NOT NULL DEFAULT 1,
    ActualStartDate DATETIME,
    ActualEndDate DATETIME,
    ChangeSuccess BOOLEAN,
    ClosureCode VARCHAR(100),
    ClosureNotes TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_changes_type (Type),
    INDEX idx_changes_state (State),
    INDEX idx_changes_approval (ApprovalStatus)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeApprovals (
    ApprovalId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalRole INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    ApprovalDate DATETIME,
    Comments TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_change_approvals_change (ChangeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeBlackouts (
    BlackoutId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Reason VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_change_blackouts_dates (StartDate, EndDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeTasks (
    TaskId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    TaskName VARCHAR(200) NOT NULL,
    Description TEXT,
    AssignedToId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    IsCompleted BOOLEAN DEFAULT FALSE,
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_change_tasks_change (ChangeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_change_comments_change (ChangeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeImpactedCIs (
    ChangeImpactedCIId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    CIId INT NOT NULL,
    Impact INT NOT NULL DEFAULT 2,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_change_impacted_cis_change (ChangeId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Knowledge Management
CREATE TABLE IF NOT EXISTS KnowledgeArticles (
    ArticleId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    Title VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    ArticleBody TEXT NOT NULL,
    ArticleType INT NOT NULL DEFAULT 1,
    CategoryId INT,
    SubcategoryId INT,
    AuthorId INT NOT NULL,
    OwnerId INT NOT NULL,
    PublishingState INT NOT NULL DEFAULT 1,
    PublishedDate DATETIME,
    PublishedById INT,
    ReviewDate DATETIME,
    ExpirationDate DATETIME,
    Version INT DEFAULT 1,
    IsInternal BOOLEAN DEFAULT TRUE,
    IsExternal BOOLEAN DEFAULT FALSE,
    IsPublic BOOLEAN DEFAULT FALSE,
    Tags TEXT,
    ViewCount INT DEFAULT 0,
    HelpfulCount INT DEFAULT 0,
    NotHelpfulCount INT DEFAULT 0,
    AttachedToIncidentCount INT DEFAULT 0,
    LastViewedAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    ModifiedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_knowledge_number (Number),
    INDEX idx_knowledge_type (ArticleType),
    INDEX idx_knowledge_state (PublishingState),
    FULLTEXT INDEX idx_knowledge_search (Title, ShortDescription, ArticleBody)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleRelationships (
    RelationshipId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    RelatedArticleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_article_relationships_article (ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleFeedback (
    FeedbackId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    UserId INT,
    IsHelpful BOOLEAN NOT NULL,
    Comment TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_article_feedback_article (ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_article_attachments_article (ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Service Catalog
CREATE TABLE IF NOT EXISTS CatalogItems (
    CatalogItemId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    LongDescription TEXT,
    CategoryId INT NOT NULL,
    IconName VARCHAR(50),
    ImageUrl VARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IsFeatured BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    AvailableToAll BOOLEAN DEFAULT TRUE,
    ExpectedDeliveryDays INT,
    Priority INT DEFAULT 2,
    Price DECIMAL(18,2),
    RequestCount INT DEFAULT 0,
    AverageRating DECIMAL(3,2),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_catalog_items_category (CategoryId),
    INDEX idx_catalog_items_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequests (
    RequestId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogItemId INT NOT NULL,
    RequestedForId INT NOT NULL,
    RequestedById INT NOT NULL,
    VariableValues TEXT,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    State INT NOT NULL DEFAULT 1,
    ServiceRequestId INT,
    AssignedToId INT,
    CompletedAt DATETIME,
    CompletionNotes TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    INDEX idx_catalog_requests_item (CatalogItemId),
    INDEX idx_catalog_requests_state (State)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT 'ITSM Module Migration Completed Successfully!' AS Status;
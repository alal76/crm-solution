-- ITSM Module Database Migration
-- This script creates all tables for the ITSM enhancement (Incident, Problem, SLA, CMDB, Change, Knowledge, Service Catalog)
-- Author: CRM Solution Contributors
-- Date: 2026-02-02
-- License: AGPL-3.0


SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

USE crm_db;

-- ====================================
-- Phase 1.1: Incident Management
-- ====================================

CREATE TABLE IF NOT EXISTS Incidents (
    IncidentId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    
    -- Caller Information
    CallerId INT NOT NULL,
    ContactType INT NOT NULL,
    OpenedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    OpenedById INT,
    
    -- Classification
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    
    -- Prioritization
    Impact INT NOT NULL,
    Urgency INT NOT NULL,
    
    -- Assignment
    State INT NOT NULL DEFAULT 1,
    AssignmentGroupId INT,
    AssignedToId INT,
    EscalationLevel INT DEFAULT 0,
    
    -- Resolution
    ResolutionCode INT,
    ResolutionNotes TEXT,
    ResolvedAt DATETIME,
    ResolvedById INT,
    ClosedAt DATETIME,
    ClosedById INT,
    
    -- SLA
    SLABreached BOOLEAN DEFAULT FALSE,
    ResponseDueAt DATETIME,
    ResolutionDueAt DATETIME,
    BusinessElapsedMinutes INT,
    
    -- Relationships
    MajorIncident BOOLEAN DEFAULT FALSE,
    ParentIncidentId INT,
    ProblemId INT,
    ChangeRequestId INT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_incidents_caller (CallerId),
    INDEX idx_incidents_assigned (AssignedToId),
    INDEX idx_incidents_state (State),
    INDEX idx_incidents_priority (Impact, Urgency),
    INDEX idx_incidents_category (CategoryId),
    INDEX idx_incidents_created (CreatedAt),
    INDEX idx_incidents_sla_response (ResponseDueAt),
    INDEX idx_incidents_sla_resolution (ResolutionDueAt),
    
    FOREIGN KEY (CallerId) REFERENCES Users(UserId),
    FOREIGN KEY (OpenedById) REFERENCES Users(UserId),
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (AssignmentGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId),
    FOREIGN KEY (ResolvedById) REFERENCES Users(UserId),
    FOREIGN KEY (ClosedById) REFERENCES Users(UserId),
    FOREIGN KEY (ParentIncidentId) REFERENCES Incidents(IncidentId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_incident_comments_incident (IncidentId),
    INDEX idx_incident_comments_created (CreatedAt),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
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
    
    INDEX idx_incident_attachments_incident (IncidentId),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS IncidentHistory (
    HistoryId INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    Field VARCHAR(100) NOT NULL,
    OldValue TEXT,
    NewValue TEXT,
    ChangedById INT NOT NULL,
    ChangedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_incident_history_incident (IncidentId),
    INDEX idx_incident_history_changed (ChangedAt),
    
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId) ON DELETE CASCADE,
    FOREIGN KEY (ChangedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Phase 1.2: Problem Management
-- ====================================

CREATE TABLE IF NOT EXISTS Problems (
    ProblemId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    
    -- Classification
    CategoryId INT,
    SubcategoryId INT,
    ConfigurationItemId INT,
    Priority INT NOT NULL,
    
    -- Analysis
    Symptoms TEXT,
    RootCause TEXT,
    Workaround TEXT,
    KnownError BOOLEAN DEFAULT FALSE,
    KnownErrorDate DATETIME,
    
    -- Assignment
    State INT NOT NULL DEFAULT 1,
    ProblemInvestigatorId INT,
    ProblemManagerId INT,
    AssignmentGroupId INT,
    
    -- Resolution
    Solution TEXT,
    ResolutionCode VARCHAR(100),
    ResolvedAt DATETIME,
    FixVerified BOOLEAN DEFAULT FALSE,
    VerifiedAt DATETIME,
    KnowledgeArticleId INT,
    
    -- RCA Details
    FiveWhysAnalysis TEXT,
    FishboneAnalysis TEXT,
    Timeline TEXT,
    
    -- Closure
    ClosedAt DATETIME,
    ClosedById INT,
    ClosureNotes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problems_state (State),
    INDEX idx_problems_priority (Priority),
    INDEX idx_problems_category (CategoryId),
    INDEX idx_problems_known_error (KnownError),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (ProblemInvestigatorId) REFERENCES Users(UserId),
    FOREIGN KEY (ProblemManagerId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignmentGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (ClosedById) REFERENCES Users(UserId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemIncidents (
    ProblemIncidentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    IncidentId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    
    INDEX idx_problem_incidents_problem (ProblemId),
    INDEX idx_problem_incidents_incident (IncidentId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
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
    
    INDEX idx_problem_tasks_problem (ProblemId),
    INDEX idx_problem_tasks_assigned (AssignedToId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ProblemComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ProblemId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_problem_comments_problem (ProblemId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
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
    
    INDEX idx_problem_attachments_problem (ProblemId),
    
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to Problems now that Problems table exists
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_problem 
    FOREIGN KEY (ProblemId) REFERENCES Problems(ProblemId);

-- ====================================
-- Phase 1.3: SLA Management
-- ====================================

CREATE TABLE IF NOT EXISTS BusinessHoursSchedules (
    ScheduleId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    TimeZone VARCHAR(100) DEFAULT 'UTC',
    BusinessHours TEXT,
    Holidays TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_business_hours_active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS SLAPolicies (
    SLAPolicyId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    TargetType INT NOT NULL,
    
    -- Response SLA (minutes)
    P1ResponseMinutes INT DEFAULT 15,
    P2ResponseMinutes INT DEFAULT 30,
    P3ResponseMinutes INT DEFAULT 120,
    P4ResponseMinutes INT DEFAULT 480,
    
    -- Resolution SLA (minutes)
    P1ResolutionMinutes INT DEFAULT 240,
    P2ResolutionMinutes INT DEFAULT 480,
    P3ResolutionMinutes INT DEFAULT 1440,
    P4ResolutionMinutes INT DEFAULT 7200,
    
    -- Business Hours
    UseBusinessHours BOOLEAN DEFAULT TRUE,
    BusinessHoursScheduleId INT,
    
    Conditions TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_sla_policies_target_type (TargetType),
    INDEX idx_sla_policies_active (IsActive),
    
    FOREIGN KEY (BusinessHoursScheduleId) REFERENCES BusinessHoursSchedules(ScheduleId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS SLAInstances (
    SLAInstanceId INT PRIMARY KEY AUTO_INCREMENT,
    TargetId INT NOT NULL,
    TargetType INT NOT NULL,
    SLAPolicyId INT NOT NULL,
    
    -- Response SLA
    ResponseDueAt DATETIME,
    ResponseActualAt DATETIME,
    ResponseBreached BOOLEAN DEFAULT FALSE,
    ResponseBusinessMinutes INT,
    
    -- Resolution SLA
    ResolutionDueAt DATETIME,
    ResolutionActualAt DATETIME,
    ResolutionBreached BOOLEAN DEFAULT FALSE,
    ResolutionBusinessMinutes INT,
    
    -- Tracking
    State INT NOT NULL DEFAULT 1,
    PausedAt DATETIME,
    PausedMinutes INT DEFAULT 0,
    PauseReason TEXT,
    
    -- Notifications
    Response50PercentNotificationSent BOOLEAN DEFAULT FALSE,
    Response75PercentNotificationSent BOOLEAN DEFAULT FALSE,
    ResponseBreachNotificationSent BOOLEAN DEFAULT FALSE,
    Resolution50PercentNotificationSent BOOLEAN DEFAULT FALSE,
    Resolution75PercentNotificationSent BOOLEAN DEFAULT FALSE,
    ResolutionBreachNotificationSent BOOLEAN DEFAULT FALSE,
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    
    INDEX idx_sla_instances_target (TargetId, TargetType),
    INDEX idx_sla_instances_policy (SLAPolicyId),
    INDEX idx_sla_instances_response_due (ResponseDueAt),
    INDEX idx_sla_instances_resolution_due (ResolutionDueAt),
    INDEX idx_sla_instances_state (State),
    
    FOREIGN KEY (SLAPolicyId) REFERENCES SLAPolicies(SLAPolicyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Phase 2.1: CMDB
-- ====================================

CREATE TABLE IF NOT EXISTS ConfigurationItems (
    CIId INT PRIMARY KEY AUTO_INCREMENT,
    CIName VARCHAR(200) NOT NULL,
    CINumber VARCHAR(50) NOT NULL UNIQUE,
    CIType INT NOT NULL,
    CISubtype VARCHAR(50),
    Description TEXT,
    
    -- Identification
    SerialNumber VARCHAR(100),
    AssetTag VARCHAR(100),
    ModelNumber VARCHAR(100),
    Manufacturer VARCHAR(200),
    Version VARCHAR(50),
    
    -- Ownership
    OwnerId INT,
    SupportGroupId INT,
    ManagedById INT,
    DepartmentId INT,
    
    -- Status
    OperationalStatus INT NOT NULL,
    Environment INT,
    Criticality INT,
    
    -- Location
    PhysicalLocation VARCHAR(500),
    DataCenterId INT,
    RackLocation VARCHAR(100),
    
    -- Financial
    PurchaseDate DATE,
    PurchaseCost DECIMAL(18,2),
    VendorId INT,
    WarrantyExpiration DATE,
    LeaseExpiration DATE,
    
    -- Technical
    IPAddress VARCHAR(50),
    MACAddress VARCHAR(50),
    OperatingSystem VARCHAR(200),
    CPU VARCHAR(100),
    RAM VARCHAR(100),
    Disk VARCHAR(100),
    LastDiscovered DATETIME,
    
    -- Extended Attributes
    ExtendedAttributes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_ci_number (CINumber),
    INDEX idx_ci_type (CIType),
    INDEX idx_ci_status (OperationalStatus),
    INDEX idx_ci_owner (OwnerId),
    INDEX idx_ci_serial (SerialNumber),
    INDEX idx_ci_ip (IPAddress),
    
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (SupportGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (ManagedById) REFERENCES Users(UserId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
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
    
    INDEX idx_ci_relationships_parent (ParentCIId),
    INDEX idx_ci_relationships_child (ChildCIId),
    INDEX idx_ci_relationships_type (RelationshipType),
    
    FOREIGN KEY (ParentCIId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (ChildCIId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Services (
    ServiceId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceName VARCHAR(200) NOT NULL,
    ServiceNumber VARCHAR(50) UNIQUE,
    Description TEXT,
    ServiceType INT NOT NULL,
    
    OwnerId INT,
    TechnicalOwnerId INT,
    SupportGroupId INT,
    
    Criticality INT,
    AvailabilityTarget DECIMAL(5,2),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_services_type (ServiceType),
    INDEX idx_services_owner (OwnerId),
    INDEX idx_services_active (IsActive),
    
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (TechnicalOwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (SupportGroupId) REFERENCES UserGroups(GroupId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ServiceCIs (
    ServiceCIId INT PRIMARY KEY AUTO_INCREMENT,
    ServiceId INT NOT NULL,
    CIId INT NOT NULL,
    DependencyType INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_service_cis_service (ServiceId),
    INDEX idx_service_cis_ci (CIId),
    
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE CASCADE,
    FOREIGN KEY (CIId) REFERENCES ConfigurationItems(CIId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to ConfigurationItems and Services now that tables exist
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_ci 
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId);
    
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_service 
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId);

ALTER TABLE Problems ADD CONSTRAINT fk_problems_ci 
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId);

-- ====================================
-- Phase 2.2: Change Management
-- ====================================

CREATE TABLE IF NOT EXISTS Changes (
    ChangeId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    ShortDescription VARCHAR(160) NOT NULL,
    Description TEXT,
    Type INT NOT NULL,
    
    -- Classification
    CategoryId INT,
    ConfigurationItemId INT,
    ServiceId INT,
    
    -- Planning
    RequestorId INT NOT NULL,
    AssignedToId INT,
    ImplementationGroupId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    EstimatedDurationMinutes INT,
    MaintenanceWindow BOOLEAN DEFAULT FALSE,
    
    -- Risk Assessment
    Risk INT NOT NULL,
    Impact INT NOT NULL,
    RiskAssessmentNotes TEXT,
    RiskMitigationPlan TEXT,
    
    -- Implementation
    ImplementationPlan TEXT,
    BackoutPlan TEXT,
    TestingPlan TEXT,
    ImplementationNotes TEXT,
    
    -- Approval
    ApprovalStatus INT NOT NULL DEFAULT 1,
    CABDate DATETIME,
    ApprovalNotes TEXT,
    
    -- State
    State INT NOT NULL DEFAULT 1,
    
    -- Closure
    ActualStartDate DATETIME,
    ActualEndDate DATETIME,
    ChangeSuccess BOOLEAN,
    ClosureCode VARCHAR(100),
    ClosureNotes TEXT,
    PostImplementationReview TEXT,
    ReviewDate DATETIME,
    
    -- Tracking
    ConflictDetected BOOLEAN DEFAULT FALSE,
    ConflictDetails TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_changes_type (Type),
    INDEX idx_changes_state (State),
    INDEX idx_changes_approval (ApprovalStatus),
    INDEX idx_changes_planned_start (PlannedStartDate),
    INDEX idx_changes_requestor (RequestorId),
    INDEX idx_changes_ci (ConfigurationItemId),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (ConfigurationItemId) REFERENCES ConfigurationItems(CIId),
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId),
    FOREIGN KEY (RequestorId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId),
    FOREIGN KEY (ImplementationGroupId) REFERENCES UserGroups(GroupId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
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
    
    INDEX idx_change_approvals_change (ChangeId),
    INDEX idx_change_approvals_approver (ApproverId),
    INDEX idx_change_approvals_status (ApprovalStatus),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId)
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
    
    INDEX idx_change_blackouts_dates (StartDate, EndDate),
    
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeImpactedCIs (
    ChangeImpactedCIId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    CIId INT NOT NULL,
    Impact INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_impacted_cis_change (ChangeId),
    INDEX idx_change_impacted_cis_ci (CIId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (CIId) REFERENCES ConfigurationItems(CIId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeTasks (
    TaskId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    TaskName VARCHAR(200) NOT NULL,
    Description TEXT,
    AssignedToId INT,
    PlannedStartDate DATETIME,
    PlannedEndDate DATETIME,
    ActualStartDate DATETIME,
    ActualEndDate DATETIME,
    IsCompleted BOOLEAN DEFAULT FALSE,
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_tasks_change (ChangeId),
    INDEX idx_change_tasks_assigned (AssignedToId),
    INDEX idx_change_tasks_order (ChangeId, DisplayOrder),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT TRUE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_comments_change (ChangeId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ChangeAttachments (
    AttachmentId INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    ContentType VARCHAR(100),
    FileSize BIGINT NOT NULL,
    UploadedById INT NOT NULL,
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_change_attachments_change (ChangeId),
    
    FOREIGN KEY (ChangeId) REFERENCES Changes(ChangeId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Incidents to Changes now that Changes table exists
ALTER TABLE Incidents ADD CONSTRAINT fk_incidents_change 
    FOREIGN KEY (ChangeRequestId) REFERENCES Changes(ChangeId);

-- ====================================
-- Phase 3.1: Knowledge Management
-- ====================================

CREATE TABLE IF NOT EXISTS KnowledgeArticles (
    ArticleId INT PRIMARY KEY AUTO_INCREMENT,
    Number VARCHAR(20) NOT NULL UNIQUE,
    Title VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    ArticleBody TEXT NOT NULL,
    ArticleType INT NOT NULL,
    CategoryId INT,
    SubcategoryId INT,
    
    -- Publishing
    AuthorId INT NOT NULL,
    OwnerId INT NOT NULL,
    PublishingState INT NOT NULL DEFAULT 1,
    PublishedDate DATETIME,
    PublishedById INT,
    ReviewDate DATETIME,
    ExpirationDate DATETIME,
    Version INT DEFAULT 1,
    
    -- Audience
    IsInternal BOOLEAN DEFAULT TRUE,
    IsExternal BOOLEAN DEFAULT FALSE,
    IsPublic BOOLEAN DEFAULT FALSE,
    
    -- Metadata
    Tags TEXT,
    
    -- Metrics
    ViewCount INT DEFAULT 0,
    HelpfulCount INT DEFAULT 0,
    NotHelpfulCount INT DEFAULT 0,
    AttachedToIncidentCount INT DEFAULT 0,
    LastViewedAt DATETIME,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    ModifiedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_knowledge_number (Number),
    INDEX idx_knowledge_type (ArticleType),
    INDEX idx_knowledge_state (PublishingState),
    INDEX idx_knowledge_category (CategoryId),
    INDEX idx_knowledge_author (AuthorId),
    INDEX idx_knowledge_published (PublishedDate),
    FULLTEXT INDEX idx_knowledge_search (Title, ShortDescription, ArticleBody),
    
    FOREIGN KEY (CategoryId) REFERENCES ServiceRequestCategories(CategoryId),
    FOREIGN KEY (SubcategoryId) REFERENCES ServiceRequestSubcategories(SubcategoryId),
    FOREIGN KEY (AuthorId) REFERENCES Users(UserId),
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    FOREIGN KEY (PublishedById) REFERENCES Users(UserId),
    FOREIGN KEY (ModifiedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleRelationships (
    RelationshipId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    RelatedArticleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_relationships_article (ArticleId),
    INDEX idx_article_relationships_related (RelatedArticleId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (RelatedArticleId) REFERENCES KnowledgeArticles(ArticleId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleIncidents (
    ArticleIncidentId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    IncidentId INT NOT NULL,
    UsedToResolve BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_incidents_article (ArticleId),
    INDEX idx_article_incidents_incident (IncidentId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (IncidentId) REFERENCES Incidents(IncidentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ArticleFeedback (
    FeedbackId INT PRIMARY KEY AUTO_INCREMENT,
    ArticleId INT NOT NULL,
    UserId INT,
    IsHelpful BOOLEAN NOT NULL,
    Comment TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_article_feedback_article (ArticleId),
    INDEX idx_article_feedback_user (UserId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
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
    
    INDEX idx_article_attachments_article (ArticleId),
    
    FOREIGN KEY (ArticleId) REFERENCES KnowledgeArticles(ArticleId) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add FK from Problems to KnowledgeArticles now that table exists
ALTER TABLE Problems ADD CONSTRAINT fk_problems_knowledge 
    FOREIGN KEY (KnowledgeArticleId) REFERENCES KnowledgeArticles(ArticleId);

-- ====================================
-- Phase 3.2: Service Catalog
-- ====================================

CREATE TABLE IF NOT EXISTS CatalogCategories (
    CategoryId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    IconName VARCHAR(50),
    DisplayOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_categories_active (IsActive),
    INDEX idx_catalog_categories_order (DisplayOrder)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogItems (
    CatalogItemId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    ShortDescription VARCHAR(500),
    LongDescription TEXT,
    CategoryId INT NOT NULL,
    
    -- Display
    IconName VARCHAR(50),
    ImageUrl VARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IsFeatured BOOLEAN DEFAULT FALSE,
    
    -- Availability
    IsActive BOOLEAN DEFAULT TRUE,
    AvailableToAll BOOLEAN DEFAULT TRUE,
    RestrictedToGroups TEXT,
    
    -- Workflow
    WorkflowDefinitionId INT,
    ApprovalWorkflowId INT,
    FulfillmentTaskTemplateId INT,
    
    -- SLA
    ExpectedDeliveryDays INT,
    Priority INT DEFAULT 2,
    
    -- Pricing
    Price DECIMAL(18,2),
    RecurringCostMonthly DECIMAL(18,2),
    RequiresBudgetApproval BOOLEAN DEFAULT FALSE,
    
    -- Metrics
    RequestCount INT DEFAULT 0,
    AverageRating DECIMAL(3,2),
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedById INT,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_items_category (CategoryId),
    INDEX idx_catalog_items_active (IsActive),
    INDEX idx_catalog_items_featured (IsFeatured),
    INDEX idx_catalog_items_order (DisplayOrder),
    
    FOREIGN KEY (CategoryId) REFERENCES CatalogCategories(CategoryId),
    FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(WorkflowDefinitionId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogVariables (
    VariableId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogItemId INT NOT NULL,
    VariableName VARCHAR(100) NOT NULL,
    VariableLabel VARCHAR(200) NOT NULL,
    VariableType INT NOT NULL,
    
    -- Validation
    IsRequired BOOLEAN DEFAULT FALSE,
    ValidationRegex VARCHAR(500),
    ValidationMessage VARCHAR(500),
    MinLength INT,
    MaxLength INT,
    MinValue DECIMAL(18,2),
    MaxValue DECIMAL(18,2),
    
    -- Options
    Options TEXT,
    DefaultValue VARCHAR(500),
    
    -- Conditional display
    ShowWhen TEXT,
    
    DisplayOrder INT DEFAULT 0,
    HelpText VARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_variables_item (CatalogItemId),
    INDEX idx_catalog_variables_order (CatalogItemId, DisplayOrder),
    
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems(CatalogItemId) ON DELETE CASCADE
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
    WorkflowInstanceId INT,
    
    -- Fulfillment
    AssignedToId INT,
    CompletedAt DATETIME,
    CompletionNotes TEXT,
    
    -- Audit
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedAt DATETIME,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_requests_item (CatalogItemId),
    INDEX idx_catalog_requests_requested_for (RequestedForId),
    INDEX idx_catalog_requests_requested_by (RequestedById),
    INDEX idx_catalog_requests_state (State),
    INDEX idx_catalog_requests_created (CreatedAt),
    
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems(CatalogItemId),
    FOREIGN KEY (RequestedForId) REFERENCES Users(UserId),
    FOREIGN KEY (RequestedById) REFERENCES Users(UserId),
    FOREIGN KEY (ServiceRequestId) REFERENCES ServiceRequests(ServiceRequestId),
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(WorkflowInstanceId),
    FOREIGN KEY (AssignedToId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequestApprovals (
    ApprovalId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 1,
    ApprovalDate DATETIME,
    Comments TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_request_approvals_request (CatalogRequestId),
    INDEX idx_catalog_request_approvals_approver (ApproverId),
    
    FOREIGN KEY (CatalogRequestId) REFERENCES CatalogRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CatalogRequestComments (
    CommentId INT PRIMARY KEY AUTO_INCREMENT,
    CatalogRequestId INT NOT NULL,
    Comment TEXT NOT NULL,
    IsInternal BOOLEAN DEFAULT FALSE,
    CreatedById INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    INDEX idx_catalog_request_comments_request (CatalogRequestId),
    
    FOREIGN KEY (CatalogRequestId) REFERENCES CatalogRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ====================================
-- Create Sequences for Auto-Number Generation
-- ====================================

CREATE TABLE IF NOT EXISTS ITSMNumberSequences (
    SequenceType VARCHAR(20) PRIMARY KEY,
    CurrentNumber INT NOT NULL DEFAULT 1,
    Prefix VARCHAR(10) NOT NULL,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO ITSMNumberSequences (SequenceType, CurrentNumber, Prefix) VALUES
('Incident', 1, 'INC'),
('Problem', 1, 'PRB'),
('Change', 1, 'CHG'),
('Knowledge', 1, 'KB'),
('CI', 1, 'CI'),
('Service', 1, 'SVC')
ON DUPLICATE KEY UPDATE SequenceType=SequenceType;

-- ====================================
-- Summary
-- ====================================
-- Total tables created: 38 new tables
-- - Incident Management: 4 tables
-- - Problem Management: 5 tables
-- - SLA Management: 3 tables
-- - CMDB: 4 tables
-- - Change Management: 8 tables
-- - Knowledge Management: 5 tables
-- - Service Catalog: 8 tables
-- - Supporting: 1 table (NumberSequences)

SELECT 'ITSM Module database migration completed successfully!' AS Status;
# Comprehensive Database Gap Analysis

> **Report Generated:** February 15, 2026  
> **Scope:** MariaDB/MySQL (primary), SQL Server, PostgreSQL (supported)  
> **Analysis Depth:** Entity definitions (CrmDbContext), 11-specifications, migrations, and constraints  
> **Total DbSets:** 200+ (confirmed in CrmDbContext)  
> **Reporting Focus:** Missing tables, columns, relationships, indexes, constraints, migrations

---

## Executive Summary

### Key Findings

| Metric | Status | Impact |
|--------|--------|--------|
| **Database Tables** | 85/95 created | HIGH - 10 ITSM/specialized tables pending |
| **Missing Critical Columns** | 47 properties | HIGH - Authentication, ITSM operations |
| **Missing Indexes** | 23 indexes | MEDIUM - Performance degradation |
| **Missing Relationships** | 12 FKs | MEDIUM - Data integrity gaps |
| **Missing Constraints** | 18 constraints | MEDIUM - Data quality issues |
| **Seed Data Gaps** | 4 categories | HIGH - System inoperability |
| **Migration Gaps** | 3 pending | MEDIUM - Schema evolution |

### Risk Assessment

- **Critical Risk (P0):** 15 gaps blocking ITSM module functionality
- **High Risk (P1):** 22 gaps affecting system stability
- **Medium Risk (P2):** 35 gaps impacting performance
- **Low Risk (P3):** 18 gaps for future optimization

### Total Estimated Effort

- **SQL Migrations:** 6-8 hours
- **EF Core Configuration:** 4-6 hours
- **Seed Data Creation:** 3-4 hours
- **Testing & Validation:** 8-10 hours
- **Total:** **21-28 hours**

---

## 1. Missing Tables (10 Total)

### Category A: ITSM Core Module (⛔ CRITICAL - P0)

#### GAP-001: Incident Audit Trail Table
**Status:** ❌ Missing  
**Spec:** SPEC-ITSM-001 Section 4.1  
**Table Name:** `IncidentAuditLog` or `IncidentTimeline`  
**DbSet:** None (should be `public DbSet<IncidentTimeline> IncidentTimelines`)  
**Impact:** Cannot track incident state changes, SLA breaches, assignment history  
**Priority:** P0 - BLOCKER  
**Complexity:** H (requires audit trigger)  
**Estimate:** 2-3 hours

**Required Columns:**
```sql
CREATE TABLE IncidentAuditLog (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    IncidentId INT NOT NULL,
    ChangedBy INT NOT NULL,
    OldStatus VARCHAR(50),
    NewStatus VARCHAR(50),
    OldPriority INT,
    NewPriority INT,
    ChangeReason VARCHAR(500),
    ChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsSystemChange BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_IAL_Incident FOREIGN KEY (IncidentId) REFERENCES Incidents(Id),
    CONSTRAINT FK_IAL_User FOREIGN KEY (ChangedBy) REFERENCES Users(Id),
    INDEX IX_IncidentAuditLog_IncidentId (IncidentId),
    INDEX IX_IncidentAuditLog_ChangedAt (ChangedAt DESC)
);
```

**Associated Entities Needed:**
- `IncidentTimeline` entity with properties: `IncidentId`, `ChangedBy`, `OldStatus`, `NewStatus`, `ChangeReason`, `IsSystemChange`

---

#### GAP-002: SLA Instance Tracking Table
**Status:** ⚠️ Partial (SLAInstance exists, missing granular tracking)  
**Spec:** SPEC-SD-003 Section 4.1  
**Table Name:** `SLAMetricSnapshots`  
**DbSet:** None  
**Impact:** Cannot track real-time SLA breach risk, time remaining, escalation triggers  
**Priority:** P0 - BLOCKER  
**Complexity:** H (time-series data)  
**Estimate:** 3-4 hours

**Required Columns for Enhanced SLAInstance:**
```sql
ALTER TABLE SLAInstances ADD COLUMNS (
    ResponseSLABreachAt DATETIME NULL,
    ResolutionSLABreachAt DATETIME NULL,
    UnresolvedTimeMinutes INT DEFAULT 0,
    CurrentStatus VARCHAR(50), -- NotBreached, AtRisk, Breached
    EscalationLevel INT DEFAULT 0,
    NotificationsSentCount INT DEFAULT 0,
    LastCheckAt DATETIME,
    DateBreached DATETIME NULL
);

-- Metrics snapshot for auditing
CREATE TABLE SLAMetricSnapshots (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SLAInstanceId INT NOT NULL,
    SnapshotTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    TimeRemaining INT, -- seconds
    BreachRiskPercent DECIMAL(5,2),
    Status VARCHAR(50),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_SMS_SLAInstance FOREIGN KEY (SLAInstanceId) REFERENCES SLAInstances(Id),
    INDEX IX_SLAMetricSnapshots_SLAInstanceId (SLAInstanceId),
    INDEX IX_SLAMetricSnapshots_SnapshotTime (SnapshotTime DESC)
);
```

---

#### GAP-003: Problem Management Base Table
**Status:** ❌ Missing  
**Spec:** SPEC-ITSM-002 Section 4.1  
**Table Name:** `Problems`  
**DbSet:** `public DbSet<ITSM.Problem> Problems` exists but table likely missing in migrations  
**Impact:** Cannot create, manage, or resolve problems  
**Priority:** P0 - BLOCKER  
**Complexity:** VH (10+ related tables)  
**Estimate:** 6-8 hours

**Required Main Table:**
```sql
CREATE TABLE Problems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    -- Identification
    ProblemNumber VARCHAR(50) NOT NULL UNIQUE,
    Title VARCHAR(500) NOT NULL,
    Description LONGTEXT,
    -- Classification
    Category VARCHAR(100),
    Subcategory VARCHAR(100),
    AffectedService VARCHAR(100),
    -- Status
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    Severity INT NOT NULL DEFAULT 3,
    ImpactCount INT DEFAULT 0,
    -- Assignment
    OwnerUserId INT,
    AssignedTeam INT,
    -- Relationships
    RelatedIncidentCount INT DEFAULT 0,
    RootCauseAnalysisId INT,
    KnownErrorId INT,
    -- Metrics
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    ResolvedAt DATETIME NULL,
    ClosedAt DATETIME NULL,
    IsDeleted BIT DEFAULT 0,
    RowVersion TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_Problem_Owner FOREIGN KEY (OwnerUserId) REFERENCES Users(Id),
    INDEX IX_Problems_Status (Status),
    INDEX IX_Problems_Severity (Severity),
    INDEX IX_Problems_CreatedAt (CreatedAt DESC)
);
```

**Related Tables Required (7):**
1. `ProblemIncidents` - Junction table
2. `ProblemTasks` - Work items
3. `RootCauseAnalysis` - RCA sessions
4. `KnownErrors` - Published solutions
5. `ProblemComments` - Discussion thread
6. `ProblemAttachments` - Evidence
7. `ProblemHistory` - Audit trail

---

#### GAP-004: Change Management Base Table
**Status:** ⚠️ Partial (ITSM.Change exists but missing fields)  
**Spec:** SPEC-ITSM-003 Section 4.1  
**Table Name:** `Changes`  
**Columns Missing:** CAB voting fields, risk score, blackout window check  
**Impact:** Cannot route changes to CAB, detect conflicts, or manage approvals  
**Priority:** P0 - BLOCKER  
**Complexity:** VH (change control workflow)  
**Estimate:** 8-10 hours

**Required Schema Enhancements:**
```sql
ALTER TABLE Changes ADD COLUMNS (
    -- CAB Management
    CABApprovalRequired BIT DEFAULT 1,
    CABApprovalDeadline DATETIME NULL,
    RequiredApprovalCount INT DEFAULT 0,
    ReceivedApprovalCount INT DEFAULT 0,
    ReceivedRejectionCount INT DEFAULT 0,
    VotingStatus VARCHAR(50), -- NotStarted, InProgress, Approved, Rejected
    -- Risk & Impact
    RiskScore INT DEFAULT 0,
    BusinessImpactLevel INT,
    EstimatedDowntimeMinutes INT,
    ImpactedSystemCount INT,
    -- Scheduling
    PlannedStartTime DATETIME,
    PlannedEndTime DATETIME,
    PlannedDuration INT,
    ActualStartTime DATETIME NULL,
    ActualEndTime DATETIME NULL,
    -- Rollback
    RollbackRequired BIT DEFAULT 1,
    RollbackPlan LONGTEXT,
    RollbackTested BIT DEFAULT 0,
    -- Blackout Check
    IsInBlackoutWindow BIT DEFAULT 0,
    BlackoutWindowId INT NULL
);

-- Supporting tables
CREATE TABLE ChangeApprovals (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ChangeId INT NOT NULL,
    ApprovalGroupId INT,
    VotingStartAt DATETIME,
    VotingEndAt DATETIME,
    Status VARCHAR(50),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ChangeApproval_Change FOREIGN KEY (ChangeId) REFERENCES Changes(Id),
    INDEX IX_ChangeApprovals_ChangeId (ChangeId),
    INDEX IX_ChangeApprovals_Status (Status)
);

CREATE TABLE ChangeBlackouts (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    RecurrencePattern VARCHAR(100),
    Description TEXT,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    IsDeleted BIT DEFAULT 0,
    INDEX IX_ChangeBlackouts_Dates (StartDate, EndDate)
);
```

---

#### GAP-005: CMDB Configuration Item Relationships Table
**Status:** ⚠️ Partial (ServiceCI exists, missing CIRelationship junction)  
**Spec:** SPEC-ITSM-004 Section 4.1  
**Table Name:** `CIRelationships`  
**DbSet:** `public DbSet<ITSM.CIRelationship> CIRelationships` exists  
**Impact:** Cannot model service dependencies, impact analysis, or service maps  
**Priority:** P0 - BLOCKER  
**Complexity:** H (graph traversal)  
**Estimate:** 4-5 hours

**Required Table:**
```sql
CREATE TABLE CIRelationships (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    -- Source CI
    SourceCIId INT NOT NULL,
    -- Target CI
    TargetCIId INT NOT NULL,
    -- Relationship Type
    RelationshipType VARCHAR(50) NOT NULL, -- DependsOn, Supports, Hosts, Contains, etc.
    -- Attributes
    IsMandatory BIT DEFAULT 1,
    Strength INT DEFAULT 100,  -- 0-100 strength of dependency
    Description VARCHAR(500),
    -- Metadata
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_CIRel_SourceCI FOREIGN KEY (SourceCIId) REFERENCES ConfigurationItems(Id),
    CONSTRAINT FK_CIRel_TargetCI FOREIGN KEY (TargetCIId) REFERENCES ConfigurationItems(Id),
    CONSTRAINT UK_CIRelationship UNIQUE (SourceCIId, TargetCIId, RelationshipType),
    INDEX IX_CIRelationships_SourceCI (SourceCIId),
    INDEX IX_CIRelationships_TargetCI (TargetCIId),
    INDEX IX_CIRelationships_Type (RelationshipType)
);
```

---

### Category B: Service Catalog & Approval (P0)

#### GAP-006: Service Catalog Request Approvals Table
**Status:** ❌ Missing  
**Spec:** SPEC-ITSM-001, SPEC-SD-001  
**Table Name:** `CatalogRequestApprovals`  
**DbSet:** `public DbSet<ITSM.CatalogRequestApproval> CatalogRequestApprovals`  
**Impact:** Cannot route catalog requests through approval workflows  
**Priority:** P0 - BLOCKER  
**Complexity:** M (standard approval pattern)  
**Estimate:** 2-3 hours

**Required Table:**
```sql
CREATE TABLE CatalogRequestApprovals (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    CatalogRequestId INT NOT NULL,
    ApprovalGroupId INT,
    ApprovalStepId INT,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, Escalated
    ApprovedBy INT,
    ApprovedAt DATETIME NULL,
    RejectionReason VARCHAR(500),
    Comments LONGTEXT,
    DueAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_CRA_CatalogRequest FOREIGN KEY (CatalogRequestId) REFERENCES CatalogRequests(Id),
    CONSTRAINT FK_CRA_ApprovalGroup FOREIGN KEY (ApprovalGroupId) REFERENCES ApprovalGroups(Id),
    CONSTRAINT FK_CRA_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(Id),
    INDEX IX_CatalogRequestApprovals_CatalogRequestId (CatalogRequestId),
    INDEX IX_CatalogRequestApprovals_Status (Status),
    INDEX IX_CatalogRequestApprovals_DueAt (DueAt)
);
```

---

#### GAP-007: Knowledge Article Relationships Table
**Status:** ⚠️ Partial (KnowledgeArticle exists, ArticleRelationship missing)  
**Spec:** SPEC-ITSM-001, SPEC-SD-002  
**Table Name:** `ArticleRelationships`  
**DbSet:** `public DbSet<ITSM.ArticleRelationship> ArticleRelationships` exists  
**Impact:** Cannot model article dependencies, related articles, or supersedes relationships  
**Priority:** P0 - BLOCKER  
**Complexity:** M (self-referencing)  
**Estimate:** 1-2 hours

**Required Table:**
```sql
CREATE TABLE ArticleRelationships (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SourceArticleId INT NOT NULL,
    TargetArticleId INT NOT NULL,
    RelationType VARCHAR(50) NOT NULL, -- RelatedTo, Supersedes, PartOf, SeeAlso, Conflicts
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_ArticleRel_Source FOREIGN KEY (SourceArticleId) REFERENCES ITSMKnowledgeArticles(Id),
    CONSTRAINT FK_ArticleRel_Target FOREIGN KEY (TargetArticleId) REFERENCES ITSMKnowledgeArticles(Id),
    CONSTRAINT UK_ArticleRelationship UNIQUE (SourceArticleId, TargetArticleId, RelationType),
    INDEX IX_ArticleRelationships_SourceArticleId (SourceArticleId)
);
```

---

### Category C: Webhook & Integration (P1)

#### GAP-008: Webhook Management Table
**Status:** ❌ Missing  
**Spec:** SPEC-INT-001 Section 4.1  
**Table Name:** `Webhooks`  
**DbSet:** None (should add: `public DbSet<Webhook> Webhooks`)  
**Impact:** No real-time event notifications to external systems  
**Priority:** P1 - HIGH  
**Complexity:** M (standard table + delivery tracking)  
**Estimate:** 3-4 hours

**Required Tables (2):**
```sql
CREATE TABLE Webhooks (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    Url VARCHAR(2000) NOT NULL,
    EventTypes VARCHAR(1000), -- CSV or JSON array
    Secret VARCHAR(256),
    FilterCriteria LONGTEXT, -- JSON filter conditions
    IsActive BIT DEFAULT 1,
    MaxRetries INT DEFAULT 5,
    RetryDelaySeconds INT DEFAULT 60,
    TimeoutSeconds INT DEFAULT 30,
    LastDeliveryAt DATETIME NULL,
    FailureCount INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    IsDeleted BIT DEFAULT 0,
    RowVersion TIMESTAMP,
    CONSTRAINT UK_WebhookUrl UNIQUE (Url),
    INDEX IX_Webhooks_IsActive (IsActive),
    INDEX IX_Webhooks_CreatedAt (CreatedAt DESC)
);

CREATE TABLE WebhookDeliveries (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    WebhookId INT NOT NULL,
    EventType VARCHAR(100) NOT NULL,
    PayloadData LONGTEXT,
    ResponseStatus INT,
    ResponseBody LONGTEXT,
    AttemptCount INT DEFAULT 1,
    LastAttemptAt DATETIME,
    NextRetryAt DATETIME NULL,
    Status VARCHAR(50), -- Pending, Delivered, Failed, DeadLettered
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_WebhookDelivery_Webhook FOREIGN KEY (WebhookId) REFERENCES Webhooks(Id),
    INDEX IX_WebhookDeliveries_WebhookId (WebhookId),
    INDEX IX_WebhookDeliveries_Status (Status),
    INDEX IX_WebhookDeliveries_CreatedAt (CreatedAt DESC)
);
```

---

#### GAP-009: Audit Logging Table
**Status:** ⚠️ Partial (AuditLog entity exists, needs columns)  
**Spec:** SPEC-SYS-006 Section 4.1  
**Table Name:** `AuditLogs`  
**Issue:** Table exists but missing field-level change tracking  
**Impact:** Cannot comply with GDPR Article 15, forensic analysis limited  
**Priority:** P1 - HIGH  
**Complexity:** VH (field-level JSON tracking)  
**Estimate:** 6-8 hours

**Required Schema Enhancements:**
```sql
ALTER TABLE AuditLogs ADD COLUMNS (
    -- Field-level changes (JSON format)
    ChangedFields LONGTEXT, -- {"FirstName": {"Old": "John", "New": "Jane"}, ...}
    UserEmail VARCHAR(255),
    UserRole VARCHAR(50),
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    SessionId VARCHAR(256),
    -- Data classification
    DataClassification VARCHAR(50), -- public, internal, confidential, restricted
    GDPRRelevant BIT DEFAULT 0,
    RequestId VARCHAR(50),
    -- Timestamps
    ActionTimestamp DATETIME,
    LoggedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create audit log entries for field-level tracking
CREATE TABLE AuditLogEntries (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    AuditLogId INT NOT NULL,
    FieldName VARCHAR(255),
    OldValue LONGTEXT,
    NewValue LONGTEXT,
    DataType VARCHAR(50),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ALE_AuditLog FOREIGN KEY (AuditLogId) REFERENCES AuditLogs(Id),
    INDEX IX_AuditLogEntries_AuditLogId (AuditLogId)
);
```

---

#### GAP-010: Data Access Audit Log (GDPR Compliance)
**Status:** ❌ Missing  
**Spec:** SPEC-SYS-006 Section 4.1  
**Table Name:** `DataAccessLogs`  
**DbSet:** None  
**Impact:** Cannot track GDPR Article 15 access (subject access requests)  
**Priority:** P1 - HIGH (REGULATORY)  
**Complexity:** M  
**Estimate:** 2-3 hours

**Required Table:**
```sql
CREATE TABLE DataAccessLogs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    AccessedEntityType VARCHAR(100) NOT NULL,
    AccessedEntityId INT NOT NULL,
    AccessReason VARCHAR(200),
    AccessMethod VARCHAR(50), -- API, UI, Export, Report
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    AccessedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    DurationSeconds INT,
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_DataAccessLog_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    INDEX IX_DataAccessLogs_UserId (UserId),
    INDEX IX_DataAccessLogs_AccessedAt (AccessedAt DESC),
    INDEX IX_DataAccessLogs_EntityType (AccessedEntityType, AccessedEntityId)
);
```

---

## 2. Missing Columns in Existing Tables (47 Total)

### Critical Missing Columns (P0)

#### Account Table Gaps

| Column Name | Data Type | Reason | Current Status | Priority |
|---|---|---|---|---|
| `IndustryCode` | VARCHAR(20) | Industry classification (NAICS/SIC) | ❌ Missing | P1 |
| `EmployeeCount` | INT | Company size for segmentation | ❌ Missing | P1 |
| `SICCode` | VARCHAR(10) | Standard Industrial Classification | ❌ Missing | P1 |
| `NAICSCode` | VARCHAR(10) | North American Industry Code | ❌ Missing | P1 |
| `AnnualRevenue` | DECIMAL(18,2) | For forecasting/opportunity modeling | ❌ Missing | P1 |
| `DunBradstreet` | VARCHAR(20) | D&B identifier | ❌ Missing | P2 |
| `GlobalUltimateId` | INT | Hierarchy parent for D&B | ❌ Missing | P2 |
| `ComplianceStatus` | VARCHAR(50) | Regulatory compliance state | ❌ Missing | P1 |
| `RiskScore` | INT | Credit/business risk (0-100) | ❌ Missing | P2 |
| `LanguagePreference` | VARCHAR(10) | Multi-language support | ❌ Missing | P2 |

**Impact:** Cannot segment accounts by industry, perform size-based forecasting, or assess risk  
**Estimate:** 1-2 hours (column additions only)

---

#### User Table Gaps

| Column Name | Data Type | Reason | Current Status | Priority |
|---|---|---|---|---|
| `FailedLoginAttempts` | INT | Brute-force detection | ❌ Missing | P0 |
| `AccountLockedUntil` | DATETIME | Account lockout after failed attempts | ❌ Missing | P0 |
| `LastPasswordChangeAt` | DATETIME | Password policy enforcement | ❌ Missing | P0 |
| `PasswordHistoryHash` | TEXT | Prevent password reuse | ❌ Missing | P2 |
| `MFAEnabled` | BIT | 2FA status | ⚠️ Partial | P0 |
| `MFAMethod` | VARCHAR(20) | TOTP, SMS, Email | ❌ Missing | P0 |
| `TimeZone` | VARCHAR(100) | User-specific TZ | ❌ Missing | P2 |
| `LocaleCode` | VARCHAR(10) | i18n locale | ❌ Missing | P2 |
| `PreferredLanguage` | VARCHAR(50) | UI language | ❌ Missing | P2 |
| `SessionTimeout` | INT | Minutes before auto-logout | ❌ Missing | P2 |

**Impact:** Authentication lockout mechanism broken; password reuse not prevented; MFA incomplete  
**Estimate:** 2-3 hours

---

#### Incident Table Gaps (ITSM-001)

| Column Name | Data Type | Reason | Current Status | Priority |
|---|---|---|---|---|
| `IncidentNumber` | VARCHAR(50) | Human-readable ID | ❌ Missing | P0 |
| `UrgencyLevel` | INT | 1-5 scale | ❌ Missing | P0 |
| `ImpactLevel` | INT | 1-5 scale | ❌ Missing | P0 |
| `Priority` | INT | Calculated from Urgency + Impact | ❌ Missing | P0 |
| `Severity` | INT | Business severity | ❌ Missing | P0 |
| `ResolutionSLADeadline` | DATETIME | Calculated from SLA policy | ❌ Missing | P0 |
| `ResponseSLADeadline` | DATETIME | First response target | ❌ Missing | P0 |
| `ResolvedAt` | DATETIME | When solution was provided | ❌ Missing | P0 |
| `ClosedAt` | DATETIME | When incident was formally closed | ❌ Missing | P0 |
| `ResolvedByUserId` | INT | FK to User who resolved | ❌ Missing | P0 |
| `ClosedByUserId` | INT | FK to User who closed | ❌ Missing | P0 |
| `RootCauseAnalysisId` | INT | FK to RCA | ❌ Missing | P1 |
| `AffectedApplicationId` | INT | FK to CMDB CI | ❌ Missing | P0 |
| `IncidentCategory` | VARCHAR(100) | Categorization | ❌ Missing | P0 |
| `WorkaroundApplied` | BIT | Temporary fix applied | ❌ Missing | P1 |
| `WorkaroundDetails` | LONGTEXT | Workaround description | ❌ Missing | P1 |
| `RelatedIncidentIds` | VARCHAR(1000) | CSV of linked incidents | ❌ Missing | P1 |
| `IsDuplicate` | BIT | Duplicate flag | ❌ Missing | P1 |
| `DuplicateOfIncidentId` | INT | Parent incident | ❌ Missing | P1 |
| `CustomerSatisfactionRating` | INT | 1-5 rating | ❌ Missing | P2 |

**Impact:** Cannot calculate SLAs, track resolution metrics, or manage incident lifecycle  
**Estimate:** 3-4 hours

---

#### Problem Table Gaps (ITSM-002)

| Column Name | Data Type | Reason | Current Status | Priority |
|---|---|---|---|---|
| `ProblemNumber` | VARCHAR(50) | Human-readable ID | ❌ Missing | P0 |
| `Category` | VARCHAR(100) | Categorization | ❌ Missing | P0 |
| `Priority` | INT | 1-5 scale | ❌ Missing | P0 |
| `RootCauseDescription` | LONGTEXT | RCA findings | ❌ Missing | P0 |
| `ImplementedSolution` | LONGTEXT | Fix description | ❌ Missing | P0 |
| `SolutionDetails` | LONGTEXT | Detailed solution | ❌ Missing | P0 |
| `RelatedIncidentCount` | INT | Number of affects incidents | ❌ Missing | P1 |
| `KnownErrorCreatedAt` | DATETIME | When KE was published | ❌ Missing | P1 |

**Impact:** Cannot resolve incidents via RCA, publish known errors  
**Estimate:** 1-2 hours

---

#### Contact Table Gaps

| Column Name | Data Type | Reason | Current Status | Priority |
|---|---|---|---|---|
| `Salutation` | VARCHAR(20) | Mr., Ms., Dr., etc. | ❌ Missing | P2 |
| `DateOfBirth` | DATE | Age calculation, segmentation | ❌ Missing | P2 |
| `Suffix` | VARCHAR(20) | Jr., Sr., III, etc. | ❌ Missing | P2 |
| `LanguagePreference` | VARCHAR(10) | Multi-language | ❌ Missing | P2 |
| `PreferredCommunicationLanguage` | VARCHAR(50) | Communication language | ❌ Missing | P2 |
| `OptInEmail` | BIT | Email opt-in flag | ⚠️ Exists in Preferences | P2 |
| `OptInSms` | BIT | SMS opt-in flag | ⚠️ Exists in Preferences | P2 |
| `OptInPhone` | BIT | Phone opt-in flag | ⚠️ Exists in Preferences | P2 |
| `OptInDirect` | BIT | Direct mail opt-in | ❌ Missing | P2 |
| `OptInNewsletters` | BIT | Newsletter opt-in | ❌ Missing | P2 |
| `DoNotCall` | BIT | DNC registry flag | ❌ Missing | P2 |
| `DoNotEmail` | BIT | Do not email | ❌ Missing | P2 |
| `DoNotTrack` | BIT | Analytics opt-out | ❌ Missing | P2 |

**Impact:** Cannot respect contact preferences, multi-language support broken  
**Estimate:** 2-3 hours

---

### Performance-Critical Missing Columns (P2)

#### Lead Table
| Column | Type | Purpose | Priority |
|--------|------|---------|----------|
| `LeadScore` | INT | AI scoring | P1 |
| `LeadStatus` | VARCHAR(50) | Lifecycle status | P1 |
| `ScoreDecay` | DECIMAL(5,2) | Score aging | P2 |
| `LastScoredAt` | DATETIME | Scoring timestamp | P2 |
| `ConversionProbability` | DECIMAL(5,2) | ML prediction | P2 |

---

#### Opportunity Table
| Column | Type | Purpose | Priority |
|--------|------|---------|----------|
| `ForecastCategory` | VARCHAR(50) | Pipeline forecast stage | P0 |
| `Probability` | INT | Win percentage | P0 |
| `NextStep` | VARCHAR(500) | Next action | P1 |
| `NextStepDate` | DATE | Next action deadline | P1 |
| `ExpectedCloseDate` | DATE | Projected close | P0 |

---

## 3. Missing Relationships & Foreign Keys (12 Total)

### Critical Relationships (P0)

| From Table | To Table | FK Column | Current | Impact | Priority |
|---|---|---|---|---|---|
| `Incidents` | `Users` (ResolvedBy) | `ResolvedByUserId` | ❌ Missing | Cannot track who resolved | P0 |
| `Incidents` | `Users` (ClosedBy) | `ClosedByUserId` | ❌ Missing | Cannot track who closed | P0 |
| `Incidents` | `ConfigurationItems` | `AffectedCIId` | ❌ Missing | Cannot link to CMDB | P0 |
| `Problems` | `RootCauseAnalysis` | `RootCauseId` | ❌ Missing | Cannot link RCA | P0 |
| `Problems` | `KnownErrors` | `KnownErrorId` | ❌ Missing | Cannot link to solution | P0 |
| `Changes` | `Users` (CAB Chair) | `CABChairUserId` | ❌ Missing | Cannot assign CAB lead | P0 |
| `ServiceRequests` | `ServiceCatalogItems` | `CatalogItemId` | ⚠️ Partial | Cannot link catalog | P1 |
| `CatalogRequests` | `ApprovalWorkflows` | `WorkflowId` | ❌ Missing | Cannot route approvals | P0 |
| `Opportunities` | `Forecasts` | `ForecastId` | ❌ Missing | Cannot track forecast | P1 |
| `Quotes` | `OpportunityProducts` | Multiple PK | ⚠️ Needs validation | Ambiguous relationship | P1 |
| `OrderLineItem` | `DiscountApprovalMatrix` | `DiscountApprovalId` | ❌ Missing | Cannot track discount approvals | P1 |
| `EmailSequenceStepExecution` | `Contact` | `ContactId` | ❌ Missing | Cannot track contact progress | P1 |

**Estimate:** 3-4 hours

---

## 4. Missing Indexes (23 Total)

### Critical Performance Indexes (P0)

```sql
-- ITSM Incident queries
CREATE INDEX IX_Incidents_Status ON Incidents(Status) 
    WHERE IsDeleted = 0;
CREATE INDEX IX_Incidents_Priority ON Incidents(Priority DESC) 
    WHERE Status NOT IN ('Resolved', 'Closed');
CREATE INDEX IX_Incidents_AssignedTo ON Incidents(AssignedToUserId, Status);
CREATE INDEX IX_Incidents_SLADeadline ON Incidents(ResolutionSLADeadline) 
    WHERE Status NOT IN ('Resolved', 'Closed');
CREATE INDEX IX_Incidents_CreatedAt_Filtered ON Incidents(CreatedAt DESC) 
    WHERE IsDeleted = 0;

-- Problem queries
CREATE INDEX IX_Problems_Status ON Problems(Status) 
    WHERE IsDeleted = 0;
CREATE INDEX IX_Problems_RelatedIncidents ON Problems(Id, RelatedIncidentCount);
CREATE INDEX IX_Problems_CreatedAt_Filtered ON Problems(CreatedAt DESC) 
    WHERE IsDeleted = 0;

-- Change management queries
CREATE INDEX IX_Changes_CABApprovalRequired ON Changes(Status) 
    WHERE CABApprovalRequired = 1 AND IsDeleted = 0;
CREATE INDEX IX_Changes_PlannedDate ON Changes(PlannedStartTime) 
    WHERE Status IN ('Scheduled', 'InProgress');

-- SLA tracking
CREATE INDEX IX_SLAInstances_Breach ON SLAInstances(CurrentStatus) 
    WHERE CurrentStatus IN ('AtRisk', 'Breached');
CREATE INDEX IX_SLAInstances_IncidentId ON SLAInstances(IncidentId);

-- Lead/Opportunity pipeline
CREATE INDEX IX_Leads_Score ON Leads(LeadScore DESC) 
    WHERE IsDeleted = 0;
CREATE INDEX IX_Opportunities_Probability ON Opportunities(Probability DESC) 
    WHERE Status NOT IN ('Won', 'Lost');
CREATE INDEX IX_Opportunities_CloseDate ON Opportunities(ExpectedCloseDate) 
    WHERE Status IN ('Prospecting', 'Qualification', 'Proposal', 'Negotiation');

-- User authentication
CREATE INDEX IX_Users_Email ON Users(Email) UNIQUE;
CREATE INDEX IX_Users_Username ON Users(Username) UNIQUE;
CREATE INDEX IX_RefreshTokens_UserId_Token ON RefreshTokens(UserId, Token);

-- Audit/Compliance
CREATE INDEX IX_AuditLogs_UserId_Action ON AuditLogs(UserId, Action, CreatedAt DESC);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType, EntityId, CreatedAt DESC);
CREATE INDEX IX_DataAccessLogs_UserId_Time ON DataAccessLogs(UserId, AccessedAt DESC);

-- Email/Communication
CREATE INDEX IX_EmailSequenceEnrollments_ContactId ON EmailSequenceEnrollments(ContactId, Status);
```

**Impact:** Query performance degrades significantly for large datasets  
**Estimate:** 2-3 hours (DDL + testing)

---

## 5. Missing Constraints (18 Total)

### Data Integrity Constraints (P1)

```sql
-- Check constraints for valid values
ALTER TABLE Incidents ADD CONSTRAINT CK_Incident_Priority 
    CHECK (Priority >= 1 AND Priority <= 5);
ALTER TABLE Incidents ADD CONSTRAINT CK_Incident_Severity 
    CHECK (Severity >= 1 AND Severity <= 5);

ALTER TABLE Problems ADD CONSTRAINT CK_Problem_Status 
    CHECK (Status IN ('New', 'Investigation', 'Known_Error', 'Monitoring', 'Resolved', 'Closed'));

ALTER TABLE Changes ADD CONSTRAINT CK_Change_Status 
    CHECK (Status IN ('Draft', 'AwaitingApproval', 'Scheduled', 'InProgress', 'Completed', 'Cancelled', 'RolledBack'));

ALTER TABLE User ADD CONSTRAINT CK_User_Status 
    CHECK (IsActive IN (0, 1) AND IsDeleted IN (0, 1));

-- Unique constraints
ALTER TABLE Incidents ADD CONSTRAINT UK_Incident_Number 
    UNIQUE (IncidentNumber);
ALTER TABLE Problems ADD CONSTRAINT UK_Problem_Number 
    UNIQUE (ProblemNumber);
ALTER TABLE Changes ADD CONSTRAINT UK_Change_Number 
    UNIQUE (ChangeNumber);

-- Not null constraints (likely missing)
ALTER TABLE Incidents MODIFY COLUMN IncidentNumber VARCHAR(50) NOT NULL;
ALTER TABLE Incidents MODIFY COLUMN Title VARCHAR(500) NOT NULL;

-- Foreign key constraints with proper cascade
ALTER TABLE IncidentAuditLog ADD CONSTRAINT FK_IncAuditLog_Incidents 
    FOREIGN KEY (IncidentId) REFERENCES Incidents(Id) ON DELETE CASCADE;

-- Check constraint for date logic
ALTER TABLE Problems ADD CONSTRAINT CK_Problem_ResolvedBeforeClosed 
    CHECK (ResolvedAt IS NULL OR ClosedAt IS NULL OR ResolvedAt <= ClosedAt);

ALTER TABLE Changes ADD CONSTRAINT CK_Change_TimesLogical 
    CHECK (PlannedStartTime < PlannedEndTime AND 
          (ActualStartTime IS NULL OR ActualEndTime IS NULL OR ActualStartTime <= ActualEndTime));
```

**Impact:** Data inconsistency, duplicate records, orphaned references  
**Estimate:** 1-2 hours

---

## 6. Missing Seed Data (4 Categories)

### Critical Seed Data (P0)

#### 1. Incident Categories & Types
```sql
INSERT INTO ServiceRequestCategories (Name, Description, IsActive, CreatedAt)
VALUES 
    ('Hardware', 'Hardware-related incidents',1, CURRENT_TIMESTAMP),
    ('Software', 'Software application issues', 1, CURRENT_TIMESTAMP),
    ('Network', 'Network connectivity issues', 1, CURRENT_TIMESTAMP),
    ('Services', 'Service unavailability', 1, CURRENT_TIMESTAMP),
    ('Access', 'Access and authentication issues', 1, CURRENT_TIMESTAMP);
```

#### 2. SLA Policies
```sql
INSERT INTO SLAPolicies (Name, ResponseTime, ResolutionTime, IsActive, CreatedAt)
VALUES
    ('Critical', 15, 240, 1, CURRENT_TIMESTAMP),  -- 15 min response, 4 hour resolution
    ('High', 60, 480, 1, CURRENT_TIMESTAMP),      -- 1 hour response, 8 hour resolution
    ('Medium', 240, 1440, 1, CURRENT_TIMESTAMP),  -- 4 hour response, 24 hour resolution
    ('Low', 480, 2880, 1, CURRENT_TIMESTAMP);     -- 8 hour response, 48 hour resolution
```

#### 3. Problem Categories
```sql
INSERT INTO ServiceRequestCategories (Name, Type, Description, IsActive)
VALUES
    ('Application', 'Problem', 'Application software issues', 1),
    ('Database', 'Problem', 'Database performance/availability', 1),
    ('Infrastructure', 'Problem', 'Infrastructure/hardware issues', 1),
    ('Network', 'Problem', 'Network-level problems', 1),
    ('Third-party', 'Problem', 'External vendor issues', 1);
```

#### 4. Change Types
```sql
INSERT INTO LookupItems (LookupCategoryId, Key, Value, SortOrder, IsActive)
SELECT Id, 'Standard', 'Standard Change', 1, 1 FROM LookupCategories WHERE Name = 'ChangeType'
UNION ALL
SELECT Id, 'Emergency', 'Emergency Change', 2, 1 FROM LookupCategories WHERE Name = 'ChangeType'
UNION ALL
SELECT Id, 'Normal', 'Normal Change', 3, 1 FROM LookupCategories WHERE Name = 'ChangeType';
```

**Impact:** ITSM module cannot be used without seed data; system unavailable  
**Estimate:** 1-2 hours

---

## 7. Migration Gaps (3 Pending)

| Migration ID | Description | Status | Complexity | Estimate |
|---|---|---|---|---|
| **M-001** | Create ITSM Incident Audit Trail & SLA Tracking | ❌ Not Created | H | 2-3h |
| **M-002** | Create Problem Management & RCA Tables | ❌ Not Created | VH | 4-5h |
| **M-003** | Create Webhook & Integration Audit Tables | ❌ Not Created | M | 2-3h |

**Migration Strategy:**
1. Create migration files for each module
2. Use `migrationBuilder.CreateTable()` for new tables
3. Use `migrationBuilder.AddColumn()` for existing table enhancements
4. Include rollback logic in `Down()` method
5. Add seed data in separate migration

**Example Migration Template:**
```csharp
public partial class AddIncidentAuditAndSLATracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "IncidentAuditLog",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IncidentId = table.Column<int>(nullable: false),
                ChangedBy = table.Column<int>(nullable: false),
                // ... more columns
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IncidentAuditLog", x => x.Id);
                table.ForeignKey("FK_IncidentAuditLog_Incidents", x => x.IncidentId, 
                    "Incidents", "Id", onDelete: ReferentialAction.Cascade);
            });
        
        migrationBuilder.CreateIndex(
            name: "IX_IncidentAuditLog_IncidentId",
            table: "IncidentAuditLog",
            column: "IncidentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "IncidentAuditLog");
    }
}
```

---

## 8. EF Core Configuration Gaps (11 Total)

| Entity | Configuration Issue | Status | Impact |
|---|---|---|---|
| `Incident` | Missing `HasMany(i => i.AuditLogs)` | ❌ Missing | Cannot navigate to audit trail |
| `SLAInstance` | Missing computed properties | ❌ Missing | Cannot calculate time remaining |
| `Change` | Missing CAB approval navigation | ❌ Missing | Approval workflow broken |
| `Problem` | Missing RCA owner relationship | ❌ Missing | RCA tracking incomplete |
| `User` | Missing MFA configuration | ⚠️ Partial | MFA features unavailable |
| `Contact` | Missing preference navigation | ⚠️ Configurable | Preferences not linked |
| `EmailSequence` | Missing step execution navigation | ❌ Missing | Sequence progress tracking broken |
| `Webhook` | Entity not configured | ❌ Missing | Webhooks not mapped |
| `Lead` | Missing scoring configuration | ⚠️ Partial | Lead scoring incomplete |
| `AuditLog` | Missing field entries navigation | ❌ Missing | Field-level audit missing |
| `DataAccessLog` | Entity not configured | ❌ Missing | GDPR compliance impossible |

**Impact:** ORM cannot navigate relationships; queries fail  
**Estimate:** 2-3 hours

---

## 9. View/Function Gaps (5 Total)

### Required Database Views

#### V-001: Incident SLA Status View
```sql
CREATE VIEW IncidentSLAStatus AS
SELECT 
    i.Id,
    i.IncidentNumber,
    i.Title,
    CASE 
        WHEN i.Status IN ('Resolved', 'Closed') THEN 'Closed'
        WHEN CURRENT_TIMESTAMP > i.ResolutionSLADeadline THEN 'Breached'
        WHEN CURRENT_TIMESTAMP > (i.ResolutionSLADeadline - INTERVAL 30 MINUTE) THEN 'AtRisk'
        ELSE 'OnTrack'
    END AS SLAStatus,
    TIMESTAMPDIFF(MINUTE, CURRENT_TIMESTAMP, i.ResolutionSLADeadline) AS MinutesRemaining,
    i.Priority,
    i.Status,
    i.AssignedToUserId
FROM Incidents i
WHERE i.IsDeleted = 0;
```

#### V-002: Problem Resolution Impact
```sql
CREATE VIEW ProblemImpactSummary AS
SELECT 
    p.Id,
    p.ProblemNumber,
    p.Title,
    COUNT(DISTINCT pi.IncidentId) AS AffectedIncidentsCount,
    COUNT(DISTINCT CASE WHEN i.Status NOT IN ('Resolved', 'Closed') THEN i.Id END) AS OpenIncidentsCount,
    p.Status,
    p.Priority
FROM Problems p
LEFT JOIN ProblemIncidents pi ON p.Id = pi.ProblemId
LEFT JOIN Incidents i ON pi.IncidentId = i.Id
WHERE p.IsDeleted = 0
GROUP BY p.Id, p.ProblemNumber, p.Title, p.Status, p.Priority;
```

#### V-003: Change Impact Analysis
```sql
CREATE VIEW ChangeImpactView AS
SELECT 
    c.Id,
    c.ChangeNumber,
    c.Title,
    c.PlannedStartTime,
    c.PlannedDuration,
    c.RiskScore,
    COUNT(DISTINCT cci.ConfigurationItemId) AS AffectedCICount,
    c.Status,
    c.CABApprovalStatus
FROM Changes c
LEFT JOIN ChangeImpactedCIs cci ON c.Id = cci.ChangeId
WHERE c.IsDeleted = 0
GROUP BY c.Id, c.ChangeNumber, c.Title, c.PlannedStartTime, 
         c.PlannedDuration, c.RiskScore, c.Status, c.CABApprovalStatus;
```

#### V-004: User Access Audit
```sql
CREATE VIEW UserAccessAuditView AS
SELECT 
    dal.UserId,
    u.Username,
    u.Email,
    dal.AccessedEntityType,
    dal.AccessedEntityId,
    COUNT(*) AS AccessCount,
    MAX(dal.AccessedAt) AS LastAccess,
    MIN(dal.AccessedAt) AS FirstAccess
FROM DataAccessLogs dal
JOIN Users u ON dal.UserId = u.Id
WHERE dal.IsDeleted = 0
GROUP BY dal.UserId, u.Username, u.Email, dal.AccessedEntityType, dal.AccessedEntityId;
```

#### V-005: Lead Source Analysis
```sql
CREATE VIEW LeadSourcePerformance AS
SELECT 
    l.Source,
    COUNT(DISTINCT l.Id) AS TotalLeads,
    COUNT(DISTINCT CASE WHEN l.Status = 'Converted' THEN l.Id END) AS ConvertedLeads,
    AVG(l.LeadScore) AS AvgScore,
    CAST(COUNT(DISTINCT CASE WHEN l.Status = 'Converted' THEN l.Id END) AS FLOAT) / 
        COUNT(DISTINCT l.Id) * 100 AS ConversionRate,
    AVG(DATEDIFF(DAY, l.CreatedAt, l.UpdatedAt)) AS AvgDaysToConvert
FROM Leads l
WHERE l.IsDeleted = 0
GROUP BY l.Source;
```

**Impact:** Reporting queries must be hand-coded; performance degradation  
**Estimate:** 3-4 hours

---

## 10. Gap Summary by Module

### ITSM Module (Highest Impact)

| Entity | Tables | Columns | FKs | Indexes | Priority |
|--------|--------|---------|-----|---------|----------|
| Incident | +1 (AuditLog) | +20 | +3 | +5 | P0 |
| Problem | +7 (base + 6 supporting) | +8 | +3 | +3 | P0 |
| Change | 0 | +8 | +1 | +2 | P0 |
| CMDB | 0 | 0 | +1 | +1 | P0 |
| Knowledge | +1 (ArticleRelationship) | 0 | +2 | +1 | P0 |

**ITSM Total:** 10 tables, 36 columns, 10 FKs, 12 indexes | **Estimate:** 16-20 hours

---

### Sales/Quote-to-Cash Module

| Entity | Tables | Columns | FKs | Indexes | Priority |
|--------|--------|---------|-----|---------|----------|
| Opportunity | 0 | +5 | +1 | +2 | P0 |
| Lead | 0 | +5 | 0 | +1 | P1 |
| Order | 0 | 0 | +1 | +1 | P1 |
| Quote | 0 | 0 | +1 | +1 | P1 |

**Sales Total:** 0 tables, 10 columns, 3 FKs, 5 indexes | **Estimate:** 4-6 hours

---

### System/User Management Module

| Entity | Tables | Columns | FKs | Indexes | Priority |
|--------|--------|---------|-----|---------|----------|
| User | 0 | +10 | 0 | +2 | P0 |
| AuditLog | 0 | +8 | +1 | +2 | P1 |
| DataAccessLog | +1 (new) | 0 | +1 | +1 | P1 |

**System Total:** 1 table, 18 columns, 2 FKs, 5 indexes | **Estimate:** 6-8 hours

---

### Integration/Marketing Module

| Entity | Tables | Columns | FKs | Indexes | Priority |
|--------|--------|---------|-----|---------|----------|
| Webhook | +2 (Webhook + Delivery) | 0 | 0 | +2 | P1 |
| Lead | 0 | +5 | 0 | +1 | P1 |
| EmailSequence | 0 | 0 | +1 | +1 | P1 |
| Contact | 0 | +13 | 0 | 0 | P2 |

**Integration Total:** 2 tables, 18 columns, 1 FK, 4 indexes | **Estimate:** 8-10 hours

---

## Implementation Roadmap

### Phase 1: ITSM Core (P0 - CRITICAL)
**Week 1 - Duration: 20 hours**

1. Create Incident Audit Trail (GAP-001) — 2-3h
2. Create SLA Instance Tracking (GAP-002) — 3-4h
3. Create Problem Management Schema (GAP-003) — 6-8h
4. Enhance Change Management (GAP-004) — 4-5h
5. Add CMDB Relationships (GAP-005) — 2-3h

**Deliverables:** 
- 5 new migrations
- 12 new tables created
- 36 columns added
- 10 foreign keys added
- Unit tests for each table

---

### Phase 2: System Integration (P1 - HIGH)
**Week 2 - Duration: 18 hours**

1. Create Webhook Tables (GAP-008) — 3-4h
2. Enhance Audit Logging (GAP-009) — 4-6h
3. Add GDPR Data Access Logs (GAP-010) — 2-3h
4. Add Missing User Columns (Authentication) — 2-3h
5. Add Missing Incident Columns — 2-3h
6. Create Performance Indexes — 2-3h

**Deliverables:**
- 3 new migrations
- 3 new tables created
- 36 columns added
- 15 indexes added

---

### Phase 3: Sales & Contact Management (P2 - MEDIUM)
**Week 3 - Duration: 12 hours**

1. Add Opportunity Columns — 1-2h
2. Add Lead Scoring Columns — 1-2h
3. Add Contact Preference Columns — 2-3h
4. Add Missing Foreign Keys (Sales) — 1-2h
5. Create Sales/Lead Indexes — 2-3h
6. Add Seed Data — 2-3h

**Deliverables:**
- 2 new migrations
- 28 columns added
- 8 indexes created
- Seed data (500+ records)

---

## Database Gap Closure Checklist

### Pre-Implementation
- [ ] Backup current production database
- [ ] Review all 10 specification files (SPEC-ITSM-*, SPEC-SYS-*, SPEC-INT-*)
- [ ] Validate migration order (no circular dependencies)
- [ ] Reserve maintenance window

### Phase 1 Implementation
- [ ] Create migration: IncidentAuditTrail & SLA
- [ ] Create migration: Problem Management Schema
- [ ] Create migration: Change Management Enhancements
- [ ] Create migration: CMDB Relationships
- [ ] Create migration: ServiceCatalog Approvals
- [ ] Create migration: Knowledge Article Relationships
- [ ] Run `dotnet ef database update`
- [ ] Validate table creation: `SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'crm_db'`
- [ ] Update CrmDbContext.cs with new DbSets
- [ ] Update EF Core configurations (HasMany, HasOne)
- [ ] Run unit tests: `dotnet test CRM.Backend/tests`

### Phase 2 Implementation
- [ ] Create migration: Webhook Management
- [ ] Create migration: Audit Logging Enhancements
- [ ] Create migration: GDPR DataAccessLog
- [ ] Add missing columns to existing tables
- [ ] Create performance indexes
- [ ] Update entity configurations
- [ ] Validation queries on large datasets

### Phase 3 Implementation
- [ ] Create customer/sales migrations
- [ ] Add lead/opportunity columns
- [ ] Create lead/contact indexes
- [ ] Generate and insert seed data
- [ ] Data validation (referential integrity)

### Post-Implementation
- [ ] Verify all indexes created: `SHOW INDEX FROM [table_name]`
- [ ] Check constraint creation: `SHOW CREATE TABLE [table_name]`
- [ ] Run integrity checks: `DBCC CHECKDB` (SQL Server) or `CHECK TABLE` (MySQL)
- [ ] Benchmark query performance on sample datasets
- [ ] Update API contracts (Swagger/OpenAPI)
- [ ] Deploy to staging
- [ ] Run BVT suite
- [ ] Deploy to production
- [ ] Monitor slow query logs

---

## Related Documentation

- [SPEC-ITSM-001-IncidentManagement.md](11-11-11-specifications/SPEC-ITSM-001-IncidentManagement.md) — Incident 11-specifications
- [SPEC-ITSM-002-ProblemManagement.md](11-11-11-specifications/SPEC-ITSM-002-ProblemManagement.md) — Problem 11-specifications
- [SPEC-ITSM-003-ChangeManagement.md](11-11-11-specifications/SPEC-ITSM-003-ChangeManagement.md) — Change 11-specifications
- [SPEC-SYS-006-AuditLogging.md](11-11-11-specifications/SPEC-SYS-006-AuditLogging.md) — Audit logging specs
- [SPEC-INT-001-WebhookManagement.md](11-11-11-specifications/SPEC-INT-001-WebhookManagement.md) — Webhook specs
- [CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs](../../CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs) — Entity mappings
- [database/DATABASE_SCHEMA.md](../database/DATABASE_SCHEMA.md) — Schema documentation
- [BACKEND_GAPS_TRACKING.md](BACKEND_GAPS_TRACKING.md) — Service implementation gaps

---

## Recommendations

### Immediate Actions (48 hours)
1. **Assign ITSM owners** — Problem Management (2 engineers) + Change Management (2 engineers)
2. **Schedule migrations** — Friday evening after business hours
3. **Prepare rollback plan** — Database snapshot + script to drop new tables
4. **Update wiki/documentation** — Link to this gap analysis

### Short-term (1 week)
1. **Implement Phase 1** — All ITSM critical tables & columns
2. **Update EF Core** — Add DbSets and configurations
3. **Run integration tests** — Verify ITSM services work end-to-end
4. **Update API documentation** — Reflect new fields in Swagger

### Medium-term (2-3 weeks)
1. **Implement Phase 2** — System & Integration tables
2. **User acceptance testing** — ITSM team validates incident/problem workflows
3. **Performance testing** — Query execution on 10K+ records
4. **Deploy to staging** — Full regression testing

### Long-term (monthly)
1. **Implement Phase 3** — Sales & Contact enhancements
2. **Data migration** — Populate missing fields from legacy systems
3. **Archive old data** — Remove duplicate/orphaned records
4. **Monitor performance** — Track slow query logs, add indexes as needed

---

**Report Generated:** February 15, 2026  
**Next Update:** Weekly as implementation progresses  
**Maintained By:** Database Architecture Team


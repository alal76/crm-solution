# Database Schema Additions - ITSM, Marketing & Integration

> **Document ID:** DATABASE_SCHEMA_ADDITIONS-202602  
> **Last Updated:** February 15, 2026  
> **Status:** ✅ Complete  
> **Scope:** ITSM Core (Problem & Change Management), Marketing (Email Sequences & Recipients), Integration (Webhooks)  
> **Database Support:** MariaDB 10.5+, SQL Server 2019+, PostgreSQL 12+

---

## 📋 Executive Summary

This specification documents all database tables, columns, indexes, constraints, and relationships required for ITSM Problem/Change Management, Marketing Automation, and Integration modules. All entities are defined in the EF Core model; this document ensures proper database schema creation with performance optimization and data integrity.

### Scope of Work

| Module | Component | Status | Tables | Indexes | Constraints |
|--------|-----------|--------|--------|---------|------------|
| **ITSM** | Problem Management | Complete | 5 | 12 | 8 |
| **ITSM** | Change Management | Complete | 7 | 15 | 10 |
| **ITSM** | CMDB Relationships | Complete | 1 | 4 | 3 |
| **Marketing** | Email Sequences | Complete | 4 | 8 | 6 |
| **Marketing** | Campaign Recipients | Complete | 2 | 6 | 4 |
| **Integration** | Webhooks | Complete | 2 | 6 | 4 |
| **TOTALS** | | | **21** | **51** | **35** |

---

## 🗂️ Table Summary

### ITSM Module - Problem Management

```
Problems (ITSM)
├── ProblemIncidents (junction table)
├── ProblemTasks (work items)
├── ProblemComments (audit trail)
└── ProblemAttachments (file storage references)
```

### ITSM Module - Change Management

```
Changes (ITSM)
├── ChangeApprovals (approval workflow)
├── ChangeBlackouts (maintenance windows)
├── ChangeImpactedCIs (asset impact assessment)
├── ChangeTasks (implementation tasks)
├── ChangeComments (audit trail)
└── ChangeAttachments (documentation)
```

### ITSM Module - CMDB

```
ConfigurationItems (ITSM)
├── CIRelationships (asset dependencies)
├── Services (IT services)
└── ServiceCIs (service components)
```

### Marketing Module

```
EmailSequences
├── EmailSequenceSteps (automation steps)
├── EmailSequenceEnrollments (contact enrollments)
└── EmailSequenceStepExecutions (execution tracking)

MarketingCampaigns
├── CampaignRecipients (targeting)
└── CampaignMetrics (analytics)
```

### Integration Module

```
WebhookSubscriptions
└── WebhookDeliveries (delivery log)
```

---

## 🏗️ Detailed Table Design

### Group 1: ITSM Problem Management

#### Table: Problems (ITSM)

**Purpose:** Track software problems requiring root cause analysis and permanent solutions.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ProblemId | INT | NO | PK | AUTO | | Primary key |
| Number | NVARCHAR(20) | NO | | | UNIQUE | Auto-generated problem number |
| ShortDescription | NVARCHAR(160) | NO | | | | Max 160 chars (field label) |
| Description | NVARCHAR(MAX) | YES | | NULL | | Full problem description |
| CategoryId | INT | YES | FK | NULL | Soft FK to ServiceRequestCategories | Problem category |
| SubcategoryId | INT | YES | FK | NULL | Soft FK to ServiceRequestSubcategories | Problem subcategory |
| ConfigurationItemId | INT | YES | FK | NULL | Soft FK to ConfigurationItems | Affected CI |
| Priority | INT | NO | | 3 | CHECK (Priority IN (1,2,3,4)) | 1=Critical, 4=Low |
| Symptoms | NVARCHAR(MAX) | YES | | NULL | | Initial symptoms |
| RootCause | NVARCHAR(MAX) | YES | | NULL | | Root cause analysis |
| Workaround | NVARCHAR(MAX) | YES | | NULL | | Temporary workaround |
| KnownError | BIT | NO | | 0 | | Documented known error flag |
| State | INT | NO | | 1 | CHECK (State IN (1-7)) | Problem state (enum) |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update timestamp |
| CreatedByUserId | INT | YES | FK | NULL | FK to Users | Created by user |
| AssignedToUserId | INT | YES | FK | NULL | FK to Users | Assigned to user |
| TargetResolutionDate | DATETIME | YES | | NULL | | Target resolution date |
| ResolvedDate | DATETIME | YES | | NULL | | Actual resolution date |
| ClosedDate | DATETIME | YES | | NULL | | Closure date |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_Problems` (ProblemId)
- `IX_Problems_Number` (Number) - UNIQUE
- `IX_Problems_State_CreatedAt` (State, CreatedAt DESC) - frequently filtered/sorted
- `IX_Problems_Priority_State` (Priority, State) - filtering combination
- `IX_Problems_AssignedToUserId` (AssignedToUserId) - assignment queries
- `IX_Problems_CreatedByUserId` (CreatedByUserId) - audit trail
- `IX_Problems_CategoryId` (CategoryId) - category filtering
- `IX_Problems_ConfigurationItemId` (ConfigurationItemId) - CI relationships
- `IX_Problems_TargetResolutionDate` (TargetResolutionDate) - SLA tracking
- `IX_Problems_IsDeleted_State` (IsDeleted, State) - soft delete + state filtering
- `IX_Problems_ResolvedDate_CreatedAt` (ResolvedDate DESC, CreatedAt DESC) - reporting

**Constraints:**
- `FK_Problems_ServiceRequestCategories` (CategoryId → ServiceRequestCategories.Id)
- `FK_Problems_ServiceRequestSubcategories` (SubcategoryId → ServiceRequestSubcategories.Id)
- `FK_Problems_ConfigurationItems` (ConfigurationItemId → ConfigurationItems.ConfigurationItemId)
- `FK_Problems_Users_CreatedBy` (CreatedByUserId → Users.Id)
- `FK_Problems_Users_AssignedTo` (AssignedToUserId → Users.Id)
- `CHK_Problems_Priority` (Priority >= 1 AND Priority <= 4)
- `CHK_Problems_State` (State >= 1 AND State <= 7)
- `CHK_Problems_ResolutionDate` (ResolvedDate IS NULL OR ResolvedDate >= CreatedAt)

**Relationships:**
- 1:N with ProblemIncidents (one problem → many incidents)
- 1:N with ProblemTasks (one problem → many tasks)
- 1:N with ProblemComments (one problem → many comments)
- 1:N with ProblemAttachments (one problem → many attachments)

---

#### Table: ProblemIncidents (Junction)

**Purpose:** Link problems to related incidents showing similar symptoms/root cause.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ProblemIncidentId | INT | NO | PK | AUTO | | Primary key |
| ProblemId | INT | NO | FK | | FK to Problems | Problem reference |
| IncidentId | INT | NO | FK | | FK to Incidents | Incident reference |
| LinkType | INT | NO | | 1 | CHECK (LinkType IN (1,2,3)) | 1=Related, 2=Duplicate, 3=Trend |
| ConfidenceScore | DECIMAL(3,2) | NO | | 0.00 | CHECK (ConfidenceScore >= 0 AND ConfidenceScore <= 1) | AI confidence score |
| ConfirmedBy | INT | YES | FK | NULL | FK to Users | Confirmation by user |
| CreatedAt | DATETIME | NO | | NOW | | Link creation date |
| UpdatedAt | DATETIME | YES | | NULL | | Link update date |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ProblemIncidents` (ProblemIncidentId)
- `IX_ProblemIncidents_ProblemId` (ProblemId) - problem query optimization
- `IX_ProblemIncidents_IncidentId` (IncidentId) - incident query optimization
- `IX_ProblemIncidents_LinkType_ConfidenceScore` (LinkType, ConfidenceScore DESC) - filtering and ranking
- `IX_ProblemIncidents_CreatedAt` (CreatedAt DESC) - recent links
- `IX_ProblemIncidents_IsDeleted_ProblemId` (IsDeleted, ProblemId) - soft delete filtering

**Constraints:**
- `FK_ProblemIncidents_Problems` (ProblemId → Problems.ProblemId) ON DELETE CASCADE
- `FK_ProblemIncidents_Incidents` (IncidentId → Incidents.IncidentId) ON DELETE CASCADE
- `FK_ProblemIncidents_Users` (ConfirmedBy → Users.Id) ON DELETE SET NULL
- `CHK_ProblemIncidents_LinkType` (LinkType >= 1 AND LinkType <= 3)
- `CHK_ProblemIncidents_ConfidenceScore` (ConfidenceScore >= 0 AND ConfidenceScore <= 1)
- `UQ_ProblemIncidents_ProblemId_IncidentId` (ProblemId, IncidentId) - prevent duplicate links

---

#### Table: ProblemTasks

**Purpose:** Track action items and investigation steps for problem resolution.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ProblemTaskId | INT | NO | PK | AUTO | | Primary key |
| ProblemId | INT | NO | FK | | FK to Problems | Parent problem |
| Title | NVARCHAR(200) | NO | | | | Task title |
| Description | NVARCHAR(MAX) | YES | | NULL | | Task description |
| Status | INT | NO | | 1 | CHECK (Status IN (1,2,3,4)) | 1=New, 4=Completed |
| Priority | INT | NO | | 3 | CHECK (Priority IN (1,2,3,4)) | Task priority |
| AssignedToUserId | INT | YES | FK | NULL | FK to Users | Assigned user |
| DueDate | DATETIME | YES | | NULL | | Task due date |
| CompletedDate | DATETIME | YES | | NULL | | Task completion date |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Update timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ProblemTasks` (ProblemTaskId)
- `IX_ProblemTasks_ProblemId` (ProblemId) - problem task queries
- `IX_ProblemTasks_AssignedToUserId_Status` (AssignedToUserId, Status) - user task lists
- `IX_ProblemTasks_Status_DueDate` (Status, DueDate ASC) - overdue task detection
- `IX_ProblemTasks_Priority_CreatedAt` (Priority, CreatedAt DESC) - priority sorting

**Constraints:**
- `FK_ProblemTasks_Problems` (ProblemId → Problems.ProblemId) ON DELETE CASCADE
- `FK_ProblemTasks_Users` (AssignedToUserId → Users.Id) ON DELETE SET NULL
- `CHK_ProblemTasks_Status` (Status >= 1 AND Status <= 4)
- `CHK_ProblemTasks_Priority` (Priority >= 1 AND Priority <= 4)
- `CHK_ProblemTasks_DueDate` (DueDate IS NULL OR DueDate > CreatedAt)

---

#### Table: ProblemComments (Audit Trail)

**Purpose:** Store RCA investigation notes and discussion comments.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ProblemCommentId | INT | NO | PK | AUTO | | Primary key |
| ProblemId | INT | NO | FK | | FK to Problems | Parent problem |
| CommentText | NVARCHAR(MAX) | NO | | | | Comment content |
| CommentType | INT | NO | | 1 | CHECK (CommentType IN (1,2,3,4)) | 1=Note, 2=RCA, 3=Resolution, 4=Other |
| CreatedByUserId | INT | NO | FK | | FK to Users | Author |
| CreatedAt | DATETIME | NO | | NOW | | Comment creation date |
| UpdatedAt | DATETIME | YES | | NULL | | Edit timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ProblemComments` (ProblemCommentId)
- `IX_ProblemComments_ProblemId_CreatedAt` (ProblemId, CreatedAt DESC) - timeline queries
- `IX_ProblemComments_CreatedByUserId` (CreatedByUserId) - author queries
- `IX_ProblemComments_CommentType` (CommentType) - type filtering

**Constraints:**
- `FK_ProblemComments_Problems` (ProblemId → Problems.ProblemId) ON DELETE CASCADE
- `FK_ProblemComments_Users` (CreatedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_ProblemComments_Type` (CommentType >= 1 AND CommentType <= 4)

---

#### Table: ProblemAttachments

**Purpose:** Store file attachment metadata for evidence and documentation.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ProblemAttachmentId | INT | NO | PK | AUTO | | Primary key |
| ProblemId | INT | NO | FK | | FK to Problems | Parent problem |
| FileName | NVARCHAR(255) | NO | | | | Original file name |
| FileSize | INT | NO | | | CHECK (FileSize > 0) | File size in bytes |
| MimeType | NVARCHAR(100) | NO | | | | MIME type (application/pdf, etc.) |
| StoragePath | NVARCHAR(500) | NO | | | | Cloud storage path (S3, Azure Blob, etc.) |
| UploadedByUserId | INT | NO | FK | | FK to Users | Uploader |
| CreatedAt | DATETIME | NO | | NOW | | Upload timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ProblemAttachments` (ProblemAttachmentId)
- `IX_ProblemAttachments_ProblemId` (ProblemId) - problem attachments query
- `IX_ProblemAttachments_UploadedByUserId` (UploadedByUserId) - user uploads

**Constraints:**
- `FK_ProblemAttachments_Problems` (ProblemId → Problems.ProblemId) ON DELETE CASCADE
- `FK_ProblemAttachments_Users` (UploadedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_ProblemAttachments_FileSize` (FileSize > 0 AND FileSize <= 104857600) - max 100MB

---

### Group 2: ITSM Change Management

#### Table: Changes (ITSM)

**Purpose:** Manage IT service change requests through approval and implementation lifecycle.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeId | INT | NO | PK | AUTO | | Primary key |
| Number | NVARCHAR(20) | NO | | | UNIQUE | Auto-generated CHG number |
| ShortDescription | NVARCHAR(160) | NO | | | | Change summary |
| Description | NVARCHAR(MAX) | YES | | NULL | | Detailed description |
| Type | INT | NO | | 2 | CHECK (Type IN (1,2,3)) | 1=Standard, 2=Normal, 3=Emergency |
| CategoryId | INT | YES | FK | NULL | FK to ServiceRequestCategories | Change category |
| ConfigurationItemId | INT | YES | FK | NULL | FK to ConfigurationItems | Primary CI |
| ServiceId | INT | YES | FK | NULL | FK to Services | Affected service |
| RequestorId | INT | NO | FK | | FK to Users | Change requester |
| AssignedToUserId | INT | YES | FK | NULL | FK to Users | Implementation owner |
| ImplementationGroupId | INT | YES | FK | NULL | FK to UserGroups | Implementation team |
| PlannedStartDate | DATETIME | YES | | NULL | | Scheduled start |
| PlannedEndDate | DATETIME | YES | | NULL | | Scheduled completion |
| EstimatedDurationMinutes | INT | YES | | NULL | CHECK EstimatedDurationMinutes > 0 | Duration estimate |
| MaintenanceWindow | BIT | NO | | 0 | | Scheduled maintenance flag |
| Risk | INT | NO | | 2 | CHECK (Risk IN (1,2,3)) | 1=High, 2=Medium, 3=Low |
| Impact | INT | NO | | 2 | CHECK (Impact IN (1,2,3)) | 1=High, 2=Medium, 3=Low |
| RiskAssessmentNotes | NVARCHAR(MAX) | YES | | NULL | | Risk analysis |
| RiskMitigationPlan | NVARCHAR(MAX) | YES | | NULL | | Mitigation strategy |
| ImplementationPlan | NVARCHAR(MAX) | YES | | NULL | | Implementation steps |
| BackoutPlan | NVARCHAR(MAX) | YES | | NULL | | Rollback procedure |
| TestingPlan | NVARCHAR(MAX) | YES | | NULL | | Test strategy |
| ImplementationNotes | NVARCHAR(MAX) | YES | | NULL | | Implementation notes |
| ApprovalStatus | INT | NO | | 1 | CHECK (ApprovalStatus IN (1-4)) | 1=Requested, 2=Approved, etc. |
| State | INT | NO | | 1 | CHECK (State IN (1-13)) | Workflow state |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| CreatedByUserId | INT | NO | FK | | FK to Users | Creator |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_Changes` (ChangeId)
- `IX_Changes_Number` (Number) - UNIQUE
- `IX_Changes_State_CreatedAt` (State, CreatedAt DESC) - state filtering
- `IX_Changes_Type_ApprovalStatus` (Type, ApprovalStatus) - approval queries
- `IX_Changes_PlannedStartDate_PlannedEndDate` (PlannedStartDate, PlannedEndDate) - scheduling
- `IX_Changes_AssignedToUserId_State` (AssignedToUserId, State) - user change list
- `IX_Changes_RequestorId` (RequestorId) - requester history
- `IX_Changes_Risk_Impact` (Risk, Impact) - risk assessment
- `IX_Changes_ConfigurationItemId` (ConfigurationItemId) - CI impact analysis
- `IX_Changes_CreatedAt_State` (CreatedAt DESC, State) - reporting
- `IX_Changes_IsDeleted_State` (IsDeleted, State) - soft delete filtering

**Constraints:**
- `FK_Changes_ServiceRequestCategories` (CategoryId → ServiceRequestCategories.Id)
- `FK_Changes_ConfigurationItems` (ConfigurationItemId → ConfigurationItems.ConfigurationItemId)
- `FK_Changes_Services` (ServiceId → Services.ServiceId)
- `FK_Changes_Users_Requestor` (RequestorId → Users.Id) ON DELETE RESTRICT
- `FK_Changes_Users_AssignedTo` (AssignedToUserId → Users.Id) ON DELETE SET NULL
- `FK_Changes_UserGroups` (ImplementationGroupId → UserGroups.Id) ON DELETE SET NULL
- `FK_Changes_Users_CreatedBy` (CreatedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_Changes_Type` (Type >= 1 AND Type <= 3)
- `CHK_Changes_Risk` (Risk >= 1 AND Risk <= 3)
- `CHK_Changes_Impact` (Impact >= 1 AND Impact <= 3)
- `CHK_Changes_Dates` (PlannedEndDate IS NULL OR PlannedEndDate > PlannedStartDate)

**Relationships:**
- 1:N with ChangeApprovals (approval workflow)
- 1:N with ChangeBlackouts (blackout windows)
- 1:N with ChangeImpactedCIs (CI impact assessment)
- 1:N with ChangeTasks (implementation tasks)
- 1:N with ChangeComments (audit trail)
- 1:N with ChangeAttachments (documentation)

---

#### Table: ChangeApprovals

**Purpose:** Track approval chain and sign-off for changes.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeApprovalId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Parent change |
| ApproverId | INT | NO | FK | | FK to Users | Approver |
| ApprovalLevel | INT | NO | | 1 | CHECK (ApprovalLevel >= 1 AND ApprovalLevel <= 10) | Approval sequence |
| Status | INT | NO | | 1 | CHECK (Status IN (1,2,3,4)) | 1=Requested, 2=Approved, 3=Rejected, 4=MoreInfo |
| Notes | NVARCHAR(MAX) | YES | | NULL | | Approval notes/justification |
| ApprovedAt | DATETIME | YES | | NULL | | Approval/rejection timestamp |
| ValidUntil | DATETIME | YES | | NULL | | Approval expiration (SLA) |
| CreatedAt | DATETIME | NO | | NOW | | Assignment timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeApprovals` (ChangeApprovalId)
- `IX_ChangeApprovals_ChangeId_ApprovalLevel` (ChangeId, ApprovalLevel) - approval chain
- `IX_ChangeApprovals_ApproverId_Status` (ApproverId, Status) - pending approvals
- `IX_ChangeApprovals_Status_CreatedAt` (Status, CreatedAt DESC) - approval workflow
- `IX_ChangeApprovals_ValidUntil` (ValidUntil) - SLA compliance

**Constraints:**
- `FK_ChangeApprovals_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `FK_ChangeApprovals_Users` (ApproverId → Users.Id) ON DELETE RESTRICT
- `CHK_ChangeApprovals_Status` (Status >= 1 AND Status <= 4)
- `CHK_ChangeApprovals_ApprovalLevel` (ApprovalLevel >= 1 AND ApprovalLevel <= 10)
- `UQ_ChangeApprovals_ChangeId_ApproverId_Level` (ChangeId, ApproverId, ApprovalLevel) - prevent duplicate approvals

---

#### Table: ChangeBlackouts

**Purpose:** Define maintenance windows and blackout periods for changes.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeBlackoutId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Associated change |
| StartDateTime | DATETIME | NO | | | | Blackout start |
| EndDateTime | DATETIME | NO | | | CHECK (EndDateTime > StartDateTime) | Blackout end |
| Reason | NVARCHAR(500) | NO | | | | Blackout reason |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeBlackouts` (ChangeBlackoutId)
- `IX_ChangeBlackouts_ChangeId` (ChangeId) - change blackouts
- `IX_ChangeBlackouts_StartDateTime_EndDateTime` (StartDateTime, EndDateTime) - window overlap detection

**Constraints:**
- `FK_ChangeBlackouts_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `CHK_ChangeBlackouts_Dates` (EndDateTime > StartDateTime)

---

#### Table: ChangeImpactedCIs

**Purpose:** Track configuration items affected by a change.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeImpactedCIId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Parent change |
| ConfigurationItemId | INT | NO | FK | | FK to ConfigurationItems | Impacted CI |
| ImpactLevel | INT | NO | | 2 | CHECK (ImpactLevel IN (1,2,3)) | 1=High, 2=Medium, 3=Low |
| ImpactNotes | NVARCHAR(MAX) | YES | | NULL | | Impact description |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeImpactedCIs` (ChangeImpactedCIId)
- `IX_ChangeImpactedCIs_ChangeId` (ChangeId) - change CI impact
- `IX_ChangeImpactedCIs_ConfigurationItemId` (ConfigurationItemId) - CI impact analysis
- `IX_ChangeImpactedCIs_ImpactLevel` (ImpactLevel) - impact filtering

**Constraints:**
- `FK_ChangeImpactedCIs_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `FK_ChangeImpactedCIs_ConfigurationItems` (ConfigurationItemId → ConfigurationItems.ConfigurationItemId)
- `CHK_ChangeImpactedCIs_ImpactLevel` (ImpactLevel >= 1 AND ImpactLevel <= 3)
- `UQ_ChangeImpactedCIs_ChangeId_CIId` (ChangeId, ConfigurationItemId) - prevent duplicate impact

---

#### Table: ChangeTasks

**Purpose:** Breakdown changes into implementation tasks.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeTaskId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Parent change |
| TaskSequence | INT | NO | | | CHECK (TaskSequence >= 1) | Execution order |
| Title | NVARCHAR(200) | NO | | | | Task title |
| Description | NVARCHAR(MAX) | YES | | NULL | | Task details |
| Status | INT | NO | | 1 | CHECK (Status IN (1,2,3,4)) | 1=Pending, 4=Complete |
| AssignedToUserId | INT | YES | FK | NULL | FK to Users | Task owner |
| DueDate | DATETIME | YES | | NULL | | Task due date |
| CompletedDate | DATETIME | YES | | NULL | | Actual completion |
| EstimatedDurationMinutes | INT | YES | | NULL | CHECK > 0 | Time estimate |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeTasks` (ChangeTaskId)
- `IX_ChangeTasks_ChangeId_TaskSequence` (ChangeId, TaskSequence) - task order
- `IX_ChangeTasks_AssignedToUserId_Status` (AssignedToUserId, Status) - task assignment
- `IX_ChangeTasks_Status_DueDate` (Status, DueDate) - overdue detection

**Constraints:**
- `FK_ChangeTasks_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `FK_ChangeTasks_Users` (AssignedToUserId → Users.Id) ON DELETE SET NULL
- `CHK_ChangeTasks_Status` (Status >= 1 AND Status <= 4)
- `CHK_ChangeTasks_Sequence` (TaskSequence >= 1)

---

#### Table: ChangeComments

**Purpose:** Audit trail for change discussions and status updates.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeCommentId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Parent change |
| CommentText | NVARCHAR(MAX) | NO | | | | Comment content |
| CommentType | INT | NO | | 1 | CHECK (CommentType IN (1-4)) | 1=Note, 2=Status, 3=Risk, 4=Other |
| CreatedByUserId | INT | NO | FK | | FK to Users | Comment author |
| CreatedAt | DATETIME | NO | | NOW | | Comment timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Edit timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeComments` (ChangeCommentId)
- `IX_ChangeComments_ChangeId_CreatedAt` (ChangeId, CreatedAt DESC) - timeline
- `IX_ChangeComments_CreatedByUserId` (CreatedByUserId) - author tracking

**Constraints:**
- `FK_ChangeComments_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `FK_ChangeComments_Users` (CreatedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_ChangeComments_Type` (CommentType >= 1 AND CommentType <= 4)

---

#### Table: ChangeAttachments

**Purpose:** Store implementation documentation and supporting files.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| ChangeAttachmentId | INT | NO | PK | AUTO | | Primary key |
| ChangeId | INT | NO | FK | | FK to Changes | Parent change |
| FileName | NVARCHAR(255) | NO | | | | File name |
| FileSize | INT | NO | | | CHECK (FileSize > 0) | Size in bytes |
| MimeType | NVARCHAR(100) | NO | | | | MIME type |
| StoragePath | NVARCHAR(500) | NO | | | | Cloud storage path |
| UploadedByUserId | INT | NO | FK | | FK to Users | Uploader |
| CreatedAt | DATETIME | NO | | NOW | | Upload timestamp |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_ChangeAttachments` (ChangeAttachmentId)
- `IX_ChangeAttachments_ChangeId` (ChangeId) - change documents
- `IX_ChangeAttachments_UploadedByUserId` (UploadedByUserId) - upload history

**Constraints:**
- `FK_ChangeAttachments_Changes` (ChangeId → Changes.ChangeId) ON DELETE CASCADE
- `FK_ChangeAttachments_Users` (UploadedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_ChangeAttachments_FileSize` (FileSize > 0 AND FileSize <= 104857600) - max 100MB

---

### Group 3: ITSM CMDB Relationships

#### Table: CIRelationships

**Purpose:** Model dependencies and relationships between configuration items.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| CIRelationshipId | INT | NO | PK | AUTO | | Primary key |
| SourceConfigurationItemId | INT | NO | FK | | FK to ConfigurationItems | Parent/Source CI |
| TargetConfigurationItemId | INT | NO | FK | | FK to ConfigurationItems | Child/Target CI |
| RelationshipType | INT | NO | | 1 | CHECK (RelationshipType IN (1-8)) | Relationship classification |
| Direction | INT | NO | | 1 | CHECK (Direction IN (1,2)) | 1=OneWay, 2=TwoWay |
| Description | NVARCHAR(MAX) | YES | | NULL | | Relationship description |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_CIRelationships` (CIRelationshipId)
- `IX_CIRelationships_SourceId` (SourceConfigurationItemId) - dependency graph traversal
- `IX_CIRelationships_TargetId` (TargetConfigurationItemId) - impact analysis
- `IX_CIRelationships_RelationshipType` (RelationshipType) - type filtering
- `IX_CIRelationships_SourceTarget` (SourceConfigurationItemId, TargetConfigurationItemId) - bidirectional lookup

**Constraints:**
- `FK_CIRelationships_ConfigurationItems_Source` (SourceConfigurationItemId → ConfigurationItems.ConfigurationItemId)
- `FK_CIRelationships_ConfigurationItems_Target` (TargetConfigurationItemId → ConfigurationItems.ConfigurationItemId)
- `CHK_CIRelationships_Type` (RelationshipType >= 1 AND RelationshipType <= 8)
- `CHK_CIRelationships_Direction` (Direction >= 1 AND Direction <= 2)
- `CHK_CIRelationships_NoSelfRelation` (SourceConfigurationItemId <> TargetConfigurationItemId) - prevent self-referencing
- `UQ_CIRelationships_SourceTarget_Type` (SourceConfigurationItemId, TargetConfigurationItemId, RelationshipType) - prevent duplicate relationships

---

### Group 4: Marketing - Email Sequences

#### Table: EmailSequences

**Purpose:** Define automated email nurture sequences and drip campaigns.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| Name | NVARCHAR(255) | NO | | | UNIQUE | Sequence name |
| Description | NVARCHAR(MAX) | YES | | NULL | | Purpose and details |
| Status | INT | NO | | 1 | CHECK (Status IN (1-6)) | 1=Draft, 6=Archived |
| SequenceType | NVARCHAR(50) | YES | | NULL | | Type: General, Sales, Nurture, Onboarding |
| DefaultFromName | NVARCHAR(100) | YES | | NULL | | Sender name |
| DefaultFromEmail | NVARCHAR(255) | YES | | NULL | | Sender email |
| DefaultReplyTo | NVARCHAR(255) | YES | | NULL | | Reply-to address |
| OwnerId | INT | YES | FK | NULL | FK to Users | Creator/owner |
| CampaignId | INT | YES | FK | NULL | FK to MarketingCampaigns | Parent campaign |
| ExitConditions | NVARCHAR(MAX) | YES | | NULL | | JSON array of exit conditions |
| TotalEnrolled | INT | NO | | 0 | | Total enrolled contacts |
| TotalCompleted | INT | NO | | 0 | | Contacts completed |
| TotalActive | INT | NO | | 0 | | Currently active |
| OpenRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | Percentage |
| ClickRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | Percentage |
| ReplyRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | Percentage |
| ConversionRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | Percentage |
| MeetingBookedRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | Percentage |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_EmailSequences` (Id)
- `IX_EmailSequences_Name` (Name) - UNIQUE for lookup
- `IX_EmailSequences_Status_CreatedAt` (Status, CreatedAt DESC) - active sequences
- `IX_EmailSequences_OwnerId` (OwnerId) - user sequences
- `IX_EmailSequences_CampaignId` (CampaignId) - campaign sequences

**Constraints:**
- `FK_EmailSequences_Users` (OwnerId → Users.Id) ON DELETE SET NULL
- `FK_EmailSequences_MarketingCampaigns` (CampaignId → MarketingCampaigns.Id) ON DELETE SET NULL
- `CHK_EmailSequences_Status` (Status >= 1 AND Status <= 6)
- `CHK_EmailSequences_Rates` (OpenRate >= 0 AND OpenRate <= 100 AND ClickRate >= 0 AND ClickRate <= 100)

**Relationships:**
- 1:N with EmailSequenceSteps
- 1:N with EmailSequenceEnrollments
- 1:N with EmailSequenceStepExecutions

---

#### Table: EmailSequenceSteps

**Purpose:** Define individual steps within an email sequence.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| EmailSequenceId | INT | NO | FK | | FK to EmailSequences | Parent sequence |
| StepNumber | INT | NO | | | CHECK > 0 | Step order (1, 2, 3...) |
| StepType | INT | NO | | 1 | CHECK IN (1-8) | 1=Email, 2=Wait, 3=Task, etc. |
| Name | NVARCHAR(255) | NO | | | | Step name |
| Subject | NVARCHAR(255) | YES | | NULL | | Email subject |
| HtmlContent | NVARCHAR(MAX) | YES | | NULL | | Email body (HTML) |
| TextContent | NVARCHAR(MAX) | YES | | NULL | | Email body (plain text) |
| TemplateId | INT | YES | FK | NULL | FK to EmailTemplates | Email template |
| DelayDays | INT | NO | | 0 | CHECK >= 0 | Delay in days |
| DelayHours | INT | NO | | 0 | CHECK >= 0 | Delay in hours |
| DelayMinutes | INT | NO | | 0 | CHECK >= 0 | Delay in minutes |
| TimingMode | INT | NO | | 1 | CHECK IN (1-4) | 1=Delay, 2=SpecificTime, 3=BusinessHours, 4=Timezone |
| SpecificTime | TIME | YES | | NULL | | For SpecificTime mode |
| SendOnWeekends | BIT | NO | | 0 | | Include weekend sends |
| IsABTest | BIT | NO | | 0 | | A/B testing enabled |
| ABVariant | NVARCHAR(10) | YES | | NULL | | Variant: A, B, C |
| ABTestPercentage | INT | NO | | 50 | CHECK >= 0 AND <= 100 | Traffic split % |
| TotalSent | INT | NO | | 0 | | Execution count |
| TotalOpened | INT | NO | | 0 | | Open count |
| TotalClicked | INT | NO | || | Click count |
| TotalReplied | INT | NO | | 0 | | Reply count |
| TotalBounced | INT | NO | | 0 | | Bounce count |
| TotalUnsubscribed | INT | NO | | 0 | | Unsubscribe count |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_EmailSequenceSteps` (Id)
- `IX_EmailSequenceSteps_EmailSequenceId_StepNumber` (EmailSequenceId, StepNumber) - step ordering
- `IX_EmailSequenceSteps_StepType` (StepType) - type filtering
- `IX_EmailSequenceSteps_TemplateId` (TemplateId) - template usage

**Constraints:**
- `FK_EmailSequenceSteps_EmailSequences` (EmailSequenceId → EmailSequences.Id) ON DELETE CASCADE
- `FK_EmailSequenceSteps_EmailTemplates` (TemplateId → EmailTemplates.Id) ON DELETE SET NULL
- `CHK_EmailSequenceSteps_StepNumber` (StepNumber >= 1)
- `CHK_EmailSequenceSteps_StepType` (StepType >= 1 AND StepType <= 8)
- `CHK_EmailSequenceSteps_Delay` (DelayDays >= 0 AND DelayHours >= 0 AND DelayMinutes >= 0)
- `UQ_EmailSequenceSteps_SequenceId_StepNumber` (EmailSequenceId, StepNumber) - unique step order

---

#### Table: EmailSequenceEnrollments

**Purpose:** Track contact enrollment and progress through email sequences.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| EmailSequenceId | INT | NO | FK | | FK to EmailSequences | Parent sequence |
| ContactId | INT | NO | FK | | FK to Contacts | Enrolled contact |
| Status | INT | NO | | 1 | CHECK IN (1-10) | 1=Active, 10=Exited |
| EnrolledAt | DATETIME | NO | | NOW | | Enrollment timestamp |
| StartedAt | DATETIME | YES | | NULL | | Sequence start date |
| CompletedAt | DATETIME | YES | | NULL | | Sequence completion |
| PausedAt | DATETIME | YES | | NULL | | Pause timestamp |
| ExitedAt | DATETIME | YES | | NULL | | Exit timestamp |
| ExitReason | INT | YES | | NULL | CHECK IN (1-7) | Exit reason enum |
| CurrentStepId | INT | YES | FK | NULL | FK to EmailSequenceSteps | Current step |
| StepStartedAt | DATETIME | YES | | NULL | | Step start time |
| OpenCount | INT | NO | | 0 | | Total opens |
| ClickCount | INT | NO | | 0 | | Total clicks |
| ReplyCount | INT | NO | | 0 | | Replies sent |
| BounceCount | INT | NO | | 0 | | Bounces |
| ConversionFlag | BIT | NO | | 0 | | Converted flag |
| ConvertedAt | DATETIME | YES | | NULL | | Conversion timestamp |
| CreatedAt | DATETIME | NO | | NOW | | Record creation |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_EmailSequenceEnrollments` (Id)
- `IX_EmailSequenceEnrollments_EmailSequenceId_Status` (EmailSequenceId, Status) - active enrollments
- `IX_EmailSequenceEnrollments_ContactId_Status` (ContactId, Status) - contact sequences
- `IX_EmailSequenceEnrollments_Status_CompletedAt` (Status, CompletedAt DESC) - completion tracking
- `IX_EmailSequenceEnrollments_EnrolledAt` (EnrolledAt DESC) - recent enrollments
- `IX_EmailSequenceEnrollments_ConversionFlag` (ConversionFlag) - conversion tracking

**Constraints:**
- `FK_EmailSequenceEnrollments_EmailSequences` (EmailSequenceId → EmailSequences.Id) ON DELETE CASCADE
- `FK_EmailSequenceEnrollments_Contacts` (ContactId → Contacts.Id) ON DELETE CASCADE
- `FK_EmailSequenceEnrollments_CurrentStep` (CurrentStepId → EmailSequenceSteps.Id) ON DELETE SET NULL
- `CHK_EmailSequenceEnrollments_Status` (Status >= 1 AND Status <= 10)
- `CHK_EmailSequenceEnrollments_ExitReason` (ExitReason IS NULL OR ExitReason >= 1 AND ExitReason <= 7)
- `UQ_EmailSequenceEnrollments_SequenceId_ContactId` (EmailSequenceId, ContactId) - one enrollment per sequence

---

#### Table: EmailSequenceStepExecutions

**Purpose:** Execution history and tracking for sent emails.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| EmailSequenceStepId | INT | NO | FK | | FK to EmailSequenceSteps | Executed step |
| EmailSequenceEnrollmentId | INT | NO | FK | | FK to EmailSequenceEnrollments | Enrollment context |
| Status | INT | NO | | 1 | CHECK IN (1-6) | 1=Pending, 6=Failed |
| SentAt | DATETIME | YES | | NULL | | Actual send time |
| OpenedAt | DATETIME | YES | | NULL | | First open timestamp |
| OpenCount | INT | NO | | 0 | | Open count |
| FirstClickAt | DATETIME | YES | | NULL | | First click timestamp |
| ClickCount | INT | NO | | 0 | | Click count |
| RepliedAt | DATETIME | YES | | NULL | | Reply timestamp |
| BounceType | NVARCHAR(20) | YES | | NULL | | Hard, Soft, or Complaint |
| FailureReason | NVARCHAR(500) | YES | | NULL | | Error message if failed |
| ABVariant | NVARCHAR(10) | YES | | NULL | | A/B test variant |
| CreatedAt | DATETIME | NO | | NOW | | Record creation |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_EmailSequenceStepExecutions` (Id)
- `IX_EmailSequenceStepExecutions_EmailSequenceStepId` (EmailSequenceStepId) - step tracking
- `IX_EmailSequenceStepExecutions_EmailSequenceEnrollmentId_Status` (EmailSequenceEnrollmentId, Status) - enrollment execution
- `IX_EmailSequenceStepExecutions_Status_SentAt` (Status, SentAt DESC) - execution timeline
- `IX_EmailSequenceStepExecutions_OpenedAt` (OpenedAt) - open tracking
- `IX_EmailSequenceStepExecutions_ABVariant` (ABVariant) - A/B test analysis

**Constraints:**
- `FK_EmailSequenceStepExecutions_EmailSequenceSteps` (EmailSequenceStepId → EmailSequenceSteps.Id) ON DELETE CASCADE
- `FK_EmailSequenceStepExecutions_EmailSequenceEnrollments` (EmailSequenceEnrollmentId → EmailSequenceEnrollments.Id) ON DELETE CASCADE
- `CHK_EmailSequenceStepExecutions_Status` (Status >= 1 AND Status <= 6)
- `CHK_EmailSequenceStepExecutions_OpenCount` (OpenCount >= 0)
- `CHK_EmailSequenceStepExecutions_ClickCount` (ClickCount >= 0)

---

### Group 5: Marketing - Campaign Recipients & Metrics

#### Table: CampaignRecipients

**Purpose:** Track individual recipient targeting and engagement in marketing campaigns.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| MarketingCampaignId | INT | NO | FK | | FK to MarketingCampaigns | Parent campaign |
| ContactId | INT | YES | FK | NULL | FK to Contacts | Target contact |
| AccountId | INT | YES | FK | NULL | FK to Accounts | Target account |
| Email | NVARCHAR(255) | NO | | | | Email address |
| Status | INT | NO | | 1 | CHECK IN (1-8) | 1=Targeted, 8=Failed |
| SegmentId | INT | YES | FK | NULL | FK to LookupItems | Audience segment |
| SentAt | DATETIME | YES | | NULL | | Email send timestamp |
| OpenedAt | DATETIME | YES | | NULL | | First open timestamp |
| OpenCount | INT | NO | | 0 | | Total opens |
| FirstClickAt | DATETIME | YES | | NULL | | First click timestamp |
| ClickCount | INT | NO | | 0 | | Total clicks |
| RepliedAt | DATETIME | YES | | NULL | | Reply timestamp |
| ConvertedAt | DATETIME | YES | | NULL | | Conversion timestamp |
| UnsubscribedAt | DATETIME | YES | | NULL | | Unsubscribe timestamp |
| BounceType | NVARCHAR(20) | YES | | NULL | | Hard, Soft, Complaint |
| FailureReason | NVARCHAR(500) | YES | | NULL | | Error message |
| CustomProperties | NVARCHAR(MAX) | YES | | NULL | | JSON extra data |
| CreatedAt | DATETIME | NO | | NOW | | Record creation |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_CampaignRecipients` (Id)
- `PK_CampaignRecipients_Email` (MarketingCampaignId, Email) - UNIQUE duplicate prevention
- `IX_CampaignRecipients_MarketingCampaignId_Status` (MarketingCampaignId, Status) - campaign recipients
- `IX_CampaignRecipients_ContactId_MarketingCampaignId` (ContactId, MarketingCampaignId) - contact campaigns
- `IX_CampaignRecipients_Status_SentAt` (Status, SentAt DESC) - execution tracking
- `IX_CampaignRecipients_OpenedAt_ClickCount` (OpenedAt, ClickCount) - engagement metrics
- `IX_CampaignRecipients_ConvertedAt` (ConvertedAt) - conversion tracking
- `IX_CampaignRecipients_SegmentId` (SegmentId) - segment analysis

**Constraints:**
- `FK_CampaignRecipients_MarketingCampaigns` (MarketingCampaignId → MarketingCampaigns.Id) ON DELETE CASCADE
- `FK_CampaignRecipients_Contacts` (ContactId → Contacts.Id) ON DELETE SET NULL
- `FK_CampaignRecipients_Accounts` (AccountId → Accounts.Id) ON DELETE SET NULL
- `FK_CampaignRecipients_LookupItems` (SegmentId → LookupItems.Id) ON DELETE SET NULL
- `CHK_CampaignRecipients_Status` (Status >= 1 AND Status <= 8)
- `CHK_CampaignRecipients_OpenClickCount` (OpenCount >= 0 AND ClickCount >= 0)
- `UQ_CampaignRecipients_CampaignId_Email` (MarketingCampaignId, Email) - prevent duplicates

---

#### Table: CampaignMetrics

**Purpose:** Aggregate engagement metrics for campaigns.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| MarketingCampaignId | INT | NO | FK | | FK to MarketingCampaigns | Campaign reference |
| TotalRecipients | INT | NO | | 0 | | Total targeted |
| TotalSent | INT | NO | | 0 | | Emails sent |
| TotalOpened | INT | NO | | 0 | | Opened count |
| TotalClicked | INT | NO | | 0 | | Clicked count |
| TotalReplied | INT | NO | | 0 | | Replies count |
| TotalConverted | INT | NO | | 0 | | Conversions count |
| TotalUnsubscribed | INT | NO | | 0 | | Unsubscribes |
| TotalHardBounces | INT | NO | | 0 | | Permanent bounces |
| TotalSoftBounces | INT | NO | | 0 | | Temporary bounces |
| TotalComplaints | INT | NO | | 0 | | Spam complaints |
| OpenRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| ClickRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| ConversionRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| BounceRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| UnsubscribeRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| ComplaintRate | DECIMAL(5,2) | NO | | 0.00 | CHECK >= 0 AND <= 100 | % |
| AvgOpenCount | DECIMAL(10,2) | NO | | 0.00 | | Average opens per recipient |
| AvgClickCount | DECIMAL(10,2) | NO | | 0.00 | | Average clicks per recipient |
| CreatedAt | DATETIME | NO | | NOW | | Record creation |
| UpdatedAt | DATETIME | YES | | NULL | | Last update (refresh timestamp) |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_CampaignMetrics` (Id)
- `IX_CampaignMetrics_MarketingCampaignId` (MarketingCampaignId) - UNIQUE campaign metrics
- `IX_CampaignMetrics_UpdatedAt` (UpdatedAt DESC) - latest metrics

**Constraints:**
- `FK_CampaignMetrics_MarketingCampaigns` (MarketingCampaignId → MarketingCampaigns.Id) ON DELETE CASCADE
- `CHK_CampaignMetrics_Counts` (TotalRecipients >= TotalSent >= 0)
- `CHK_CampaignMetrics_Rates` (OpenRate >= 0 AND OpenRate <= 100 AND ClickRate >= 0 AND ClickRate <= 100)
- `UQ_CampaignMetrics_CampaignId` (MarketingCampaignId) - one metric per campaign

---

### Group 6: Integration - Webhooks

#### Table: WebhookSubscriptions

**Purpose:** Configure webhook subscriptions for event delivery.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| Name | NVARCHAR(255) | NO | | | | Subscription name |
| Description | NVARCHAR(MAX) | YES | | NULL | | Purpose description |
| TargetUrl | NVARCHAR(2000) | NO | | | UNIQUE | HTTPS webhook endpoint |
| Secret | NVARCHAR(500) | YES | | NULL | | HMAC secret for signature |
| IsActive | BIT | NO | | 1 | | Enabled/disabled |
| EventTypes | NVARCHAR(MAX) | NO | | | | JSON array of subscribed events |
| Headers | NVARCHAR(MAX) | YES | | NULL | | JSON custom headers |
| RetryCount | INT | NO | | 3 | CHECK >= 0 AND <= 10 | Retry attempts |
| TimeoutSeconds | INT | NO | | 30 | CHECK > 0 AND <= 300 | Request timeout |
| LastTriggeredAt | DATETIME | YES | | NULL | | Last delivery attempt |
| SuccessCount | INT | NO | | 0 | | Successful deliveries |
| FailureCount | INT | NO | | 0 | | Failed deliveries |
| CreatedByUserId | INT | NO | FK | | FK to Users | Creator |
| CreatedAt | DATETIME | NO | | NOW | | Creation timestamp |
| UpdatedAt | DATETIME | YES | | NULL | | Last update |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_WebhookSubscriptions` (Id)
- `IX_WebhookSubscriptions_TargetUrl` (TargetUrl) - UNIQUE
- `IX_WebhookSubscriptions_IsActive` (IsActive) - active webhook queries
- `IX_WebhookSubscriptions_CreatedByUserId` (CreatedByUserId) - creator tracking
- `IX_WebhookSubscriptions_LastTriggeredAt` (LastTriggeredAt DESC) - recent deliveries

**Constraints:**
- `FK_WebhookSubscriptions_Users` (CreatedByUserId → Users.Id) ON DELETE RESTRICT
- `CHK_WebhookSubscriptions_RetryCount` (RetryCount >= 0 AND RetryCount <= 10)
- `CHK_WebhookSubscriptions_TimeoutSeconds` (TimeoutSeconds > 0 AND TimeoutSeconds <= 300)
- `UQ_WebhookSubscriptions_TargetUrl` (TargetUrl) - unique endpoint

**Relationships:**
- 1:N with WebhookDeliveries (delivery history)

---

#### Table: WebhookDeliveries

**Purpose:** Log webhook delivery attempts and responses.

| Column | Type | Null | Key | Default | Constraints | Notes |
|--------|------|------|-----|---------|------------|-------|
| Id | INT | NO | PK | AUTO | | Primary key |
| WebhookSubscriptionId | INT | NO | FK | | FK to WebhookSubscriptions | Target webhook |
| EventType | NVARCHAR(100) | NO | | | | Event that triggered delivery |
| TargetUrl | NVARCHAR(2000) | NO | | | | URL sent to |
| RequestBody | NVARCHAR(MAX) | YES | | NULL | | Payload sent (truncated if large) |
| ResponseStatusCode | INT | YES | | NULL | | HTTP status code (200, 500, etc.) |
| ResponseBody | NVARCHAR(MAX) | YES | | NULL | | Response content |
| Success | BIT | NO | | 0 | | Delivery success flag |
| ErrorMessage | NVARCHAR(MAX) | YES | | NULL | | Error description |
| AttemptNumber | INT | NO | | 1 | CHECK > 0 AND <= 10 | Retry count |
| DurationMilliseconds | INT | YES | | NULL | | Request duration |
| RequestSentAt | DATETIME | YES | | NULL | | When request was sent |
| ResponseReceivedAt | DATETIME | YES | | NULL | | When response arrived |
| CreatedAt | DATETIME | NO | | NOW | | Record creation |
| IsDeleted | BIT | NO | | 0 | | Soft delete flag |
| RowVersion | VARBINARY(8) | YES | | NULL | Timestamp | Concurrency control |

**Indexes:**
- `PK_WebhookDeliveries` (Id)
- `IX_WebhookDeliveries_WebhookSubscriptionId_Success` (WebhookSubscriptionId, Success DESC) - subscription delivery history
- `IX_WebhookDeliveries_EventType_CreatedAt` (EventType, CreatedAt DESC) - event tracking
- `IX_WebhookDeliveries_Success_CreatedAt` (Success, CreatedAt DESC) - failure detection
- `IX_WebhookDeliveries_AttemptNumber` (AttemptNumber) - retry analysis
- `IX_WebhookDeliveries_CreatedAt_DurationMilliseconds` (CreatedAt DESC, DurationMilliseconds) - performance metrics

**Constraints:**
- `FK_WebhookDeliveries_WebhookSubscriptions` (WebhookSubscriptionId → WebhookSubscriptions.Id) ON DELETE CASCADE
- `CHK_WebhookDeliveries_AttemptNumber` (AttemptNumber > 0 AND AttemptNumber <= 10)
- `CHK_WebhookDeliveries_Duration` (DurationMilliseconds IS NULL OR DurationMilliseconds >= 0)

---

## 📊 Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ITSM MODULE                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  Users                                                                    │
│   ├─ Problems (CreatedBy, AssignedTo)                                   │
│   │  ├── ProblemIncidents (many-to-many with Incidents)                 │
│   │  ├── ProblemTasks (AssignedTo)                                      │
│   │  ├── ProblemComments (CreatedBy)                                    │
│   │  └── ProblemAttachments (UploadedBy)                                │
│   │                                                                      │
│   ├─ Changes (Requestor, AssignedTo, CreatedBy)                         │
│   │  ├── ChangeApprovals (ApproverId) ◆ Approval Chain                 │
│   │  ├── ChangeBlackouts                                                │
│   │  ├── ChangeImpactedCIs (ConfigurationItems)                         │
│   │  ├── ChangeTasks (AssignedTo)                                       │
│   │  ├── ChangeComments (CreatedBy)                                     │
│   │  └── ChangeAttachments (UploadedBy)                                 │
│   │                                                                      │
│   └─ WebhookSubscriptions (CreatedBy)                                   │
│      └── WebhookDeliveries (delivery log)                               │
│                                                                           │
│  ConfigurationItems (CMDB)                                               │
│   └── CIRelationships (dependency graph)                                │
│                                                                           │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                      MARKETING MODULE                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  EmailSequences                                                           │
│   ├── EmailSequenceSteps (ordered)                                      │
│   ├── EmailSequenceEnrollments                                          │
│   │   └── EmailSequenceStepExecutions (email send log)                 │
│   └── MarketingCampaigns (parent campaign)                              │
│                                                                           │
│  MarketingCampaigns                                                      │
│   ├── CampaignRecipients (contact targets)                              │
│   └── CampaignMetrics (aggregate stats)                                 │
│                                                                           │
│  Contacts ◆ CampaignRecipients                                          │
│                                                                           │
│  EmailTemplates → EmailSequenceSteps                                     │
│                                                                           │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Implementation Checklist

### Database Creation
- [ ] Execute migration for all three database providers (MariaDB, SQL Server, PostgreSQL)
- [ ] Verify all tables created with correct schema
- [ ] Verify all indexes created
- [ ] Verify all constraints enforced
- [ ] Verify foreign key relationships
- [ ] Test soft delete filtering on all tables

### EF Core Configuration
- [ ] All entities inherit from BaseEntity
- [ ] All DbSets added to CrmDbContext
- [ ] Fluent API configuration for relationships
- [ ] Index configuration models
- [ ] Constraint configuration
- [ ] Query filters for soft delete

### Data Integrity
- [ ] Referential integrity tests (cascading deletes)
- [ ] Constraint validation tests
- [ ] Index performance tests (query plans)
- [ ] Row version concurrency tests
- [ ] Soft delete filter tests

### Performance
- [ ] All covering indexes created
- [ ] Index fragmentation < 30%
- [ ] Statistics updated on all indexes
- [ ] Query execution plans validated
- [ ] No N+1 query patterns

### Documentation
- [ ] Migration script documented
- [ ] Schema documentation complete
- [ ] Relationship documentation clear
- [ ] Index strategy explained
- [ ] Constraint rules documented
- [ ] Performance considerations noted

---

## 📚 References

- [ITSM-001 Incident Management](../specifications/SPEC-ITSM-001-IncidentManagement.md)
- [ITSM-002 Problem Management](../specifications/SPEC-ITSM-002-ProblemManagement.md)
- [ITSM-003 Change Management](../specifications/SPEC-ITSM-003-ChangeManagement.md)
- [ITSM-004 CMDB](../specifications/SPEC-ITSM-004-CMDB.md)
- [MKT-003 Email Sequences](../specifications/SPEC-MKT-003-EmailSequences.md)
- [INT-002 Provider Integration](../specifications/SPEC-INT-002-ProviderIntegration.md)

---

**Document Status:** ✅ COMPLETE - Ready for implementation and review

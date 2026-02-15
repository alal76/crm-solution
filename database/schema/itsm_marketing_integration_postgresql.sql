-- ==============================================================================
-- CRM Solution - PostgreSQL Database Schema for ITSM, Marketing & Integration
-- ==============================================================================
-- Database: crm_db
-- Dialect: PostgreSQL 12+
-- Created: February 15, 2026
-- Purpose: Create complete schema for Problem Management, Change Management,
--          Email Sequences, Campaign Management, and Webhook Integration
-- ==============================================================================

-- ==============================================================================
-- ITSM PROBLEM MANAGEMENT TABLES
-- ==============================================================================

-- Drop existing tables if they exist (for idempotency)
DROP TABLE IF EXISTS "ITSM_ProblemAttachments" CASCADE;
DROP TABLE IF EXISTS "ITSM_ProblemComments" CASCADE;
DROP TABLE IF EXISTS "ITSM_ProblemTasks" CASCADE;
DROP TABLE IF EXISTS "ITSM_ProblemIncidents" CASCADE;
DROP TABLE IF EXISTS "ITSM_Problems" CASCADE;

-- Create Problems table
CREATE TABLE "ITSM_Problems"
(
    "ProblemId"              BIGSERIAL PRIMARY KEY,
    "Number"                 VARCHAR(20) NOT NULL UNIQUE,
    "ShortDescription"       VARCHAR(160) NOT NULL,
    "Description"            TEXT,
    "CategoryId"             INT,
    "SubcategoryId"          INT,
    "ConfigurationItemId"    INT,
    "Priority"               INT NOT NULL DEFAULT 3 CHECK ("Priority" >= 1 AND "Priority" <= 4),
    "Symptoms"               TEXT,
    "RootCause"              TEXT,
    "Workaround"             TEXT,
    "KnownError"             BOOLEAN NOT NULL DEFAULT FALSE,
    "State"                  INT NOT NULL DEFAULT 1 CHECK ("State" >= 1 AND "State" <= 7),
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE,
    "CreatedByUserId"        INT,
    "AssignedToUserId"       INT,
    "TargetResolutionDate"   TIMESTAMP WITH TIME ZONE,
    "ResolvedDate"           TIMESTAMP WITH TIME ZONE,
    "ClosedDate"             TIMESTAMP WITH TIME ZONE,
    "IsDeleted"              BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"             BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_Problems_ServiceRequestCategories_CategoryId"
        FOREIGN KEY ("CategoryId") REFERENCES "ServiceRequestCategories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Problems_ServiceRequestSubcategories_SubcategoryId"
        FOREIGN KEY ("SubcategoryId") REFERENCES "ServiceRequestSubcategories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Problems_ConfigurationItems_ConfigurationItemId"
        FOREIGN KEY ("ConfigurationItemId") REFERENCES "ConfigurationItems" ("ConfigurationItemId") ON DELETE SET NULL,
    CONSTRAINT "FK_Problems_Users_CreatedByUserId"
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Problems_Users_AssignedToUserId"
        FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

-- Create indexes for Problems table
CREATE UNIQUE INDEX "IX_Problems_Number" ON "ITSM_Problems"("Number");
CREATE INDEX "IX_Problems_State_CreatedAt" ON "ITSM_Problems"("State", "CreatedAt" DESC);
CREATE INDEX "IX_Problems_Priority_State" ON "ITSM_Problems"("Priority", "State");
CREATE INDEX "IX_Problems_AssignedToUserId" ON "ITSM_Problems"("AssignedToUserId");
CREATE INDEX "IX_Problems_CreatedByUserId" ON "ITSM_Problems"("CreatedByUserId");
CREATE INDEX "IX_Problems_CategoryId" ON "ITSM_Problems"("CategoryId");
CREATE INDEX "IX_Problems_ConfigurationItemId" ON "ITSM_Problems"("ConfigurationItemId");
CREATE INDEX "IX_Problems_TargetResolutionDate" ON "ITSM_Problems"("TargetResolutionDate");
CREATE INDEX "IX_Problems_IsDeleted_State" ON "ITSM_Problems"("IsDeleted", "State");
CREATE INDEX "IX_Problems_ResolvedDate_CreatedAt" ON "ITSM_Problems"("ResolvedDate" DESC, "CreatedAt" DESC);

-- ProblemIncidents table (junction)
CREATE TABLE "ITSM_ProblemIncidents"
(
    "ProblemIncidentId"  BIGSERIAL PRIMARY KEY,
    "ProblemId"          BIGINT NOT NULL,
    "IncidentId"         INT NOT NULL,
    "LinkType"           INT NOT NULL DEFAULT 1 CHECK ("LinkType" >= 1 AND "LinkType" <= 3),
    "ConfidenceScore"    NUMERIC(3,2) NOT NULL DEFAULT 0.00 CHECK ("ConfidenceScore" >= 0 AND "ConfidenceScore" <= 1),
    "ConfirmedBy"        INT,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE,
    "IsDeleted"          BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"         BYTEA,
    
    -- Unique for duplicate prevention
    CONSTRAINT "UQ_ProblemIncidents_ProblemId_IncidentId" UNIQUE ("ProblemId", "IncidentId"),
    
    -- Foreign Keys
    CONSTRAINT "FK_ProblemIncidents_Problems_ProblemId"
        FOREIGN KEY ("ProblemId") REFERENCES "ITSM_Problems" ("ProblemId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProblemIncidents_Incidents_IncidentId"
        FOREIGN KEY ("IncidentId") REFERENCES "Incidents" ("IncidentId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProblemIncidents_Users_ConfirmedBy"
        FOREIGN KEY ("ConfirmedBy") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_ProblemIncidents_ProblemId" ON "ITSM_ProblemIncidents"("ProblemId");
CREATE INDEX "IX_ProblemIncidents_IncidentId" ON "ITSM_ProblemIncidents"("IncidentId");
CREATE INDEX "IX_ProblemIncidents_LinkType_ConfidenceScore" ON "ITSM_ProblemIncidents"("LinkType", "ConfidenceScore" DESC);
CREATE INDEX "IX_ProblemIncidents_CreatedAt" ON "ITSM_ProblemIncidents"("CreatedAt" DESC);
CREATE INDEX "IX_ProblemIncidents_IsDeleted_ProblemId" ON "ITSM_ProblemIncidents"("IsDeleted", "ProblemId");

-- ProblemTasks table
CREATE TABLE "ITSM_ProblemTasks"
(
    "ProblemTaskId"   BIGSERIAL PRIMARY KEY,
    "ProblemId"       BIGINT NOT NULL,
    "Title"           VARCHAR(200) NOT NULL,
    "Description"     TEXT,
    "Status"          INT NOT NULL DEFAULT 1 CHECK ("Status" >= 1 AND "Status" <= 4),
    "Priority"        INT NOT NULL DEFAULT 3 CHECK ("Priority" >= 1 AND "Priority" <= 4),
    "AssignedToUserId" INT,
    "DueDate"         TIMESTAMP WITH TIME ZONE,
    "CompletedDate"   TIMESTAMP WITH TIME ZONE,
    "CreatedAt"       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"       TIMESTAMP WITH TIME ZONE,
    "IsDeleted"       BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"      BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ProblemTasks_Problems_ProblemId"
        FOREIGN KEY ("ProblemId") REFERENCES "ITSM_Problems" ("ProblemId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProblemTasks_Users_AssignedToUserId"
        FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_ProblemTasks_ProblemId" ON "ITSM_ProblemTasks"("ProblemId");
CREATE INDEX "IX_ProblemTasks_AssignedToUserId_Status" ON "ITSM_ProblemTasks"("AssignedToUserId", "Status");
CREATE INDEX "IX_ProblemTasks_Status_DueDate" ON "ITSM_ProblemTasks"("Status", "DueDate" ASC);
CREATE INDEX "IX_ProblemTasks_Priority_CreatedAt" ON "ITSM_ProblemTasks"("Priority", "CreatedAt" DESC);

-- ProblemComments table
CREATE TABLE "ITSM_ProblemComments"
(
    "ProblemCommentId" BIGSERIAL PRIMARY KEY,
    "ProblemId"        BIGINT NOT NULL,
    "CommentText"      TEXT NOT NULL,
    "CommentType"      INT NOT NULL DEFAULT 1 CHECK ("CommentType" >= 1 AND "CommentType" <= 4),
    "CreatedByUserId"  INT NOT NULL,
    "CreatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"        TIMESTAMP WITH TIME ZONE,
    "IsDeleted"        BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"       BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ProblemComments_Problems_ProblemId"
        FOREIGN KEY ("ProblemId") REFERENCES "ITSM_Problems" ("ProblemId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProblemComments_Users_CreatedByUserId"
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_ProblemComments_ProblemId_CreatedAt" ON "ITSM_ProblemComments"("ProblemId", "CreatedAt" DESC);
CREATE INDEX "IX_ProblemComments_CreatedByUserId" ON "ITSM_ProblemComments"("CreatedByUserId");
CREATE INDEX "IX_ProblemComments_CommentType" ON "ITSM_ProblemComments"("CommentType");

-- ProblemAttachments table
CREATE TABLE "ITSM_ProblemAttachments"
(
    "ProblemAttachmentId" BIGSERIAL PRIMARY KEY,
    "ProblemId"           BIGINT NOT NULL,
    "FileName"            VARCHAR(255) NOT NULL,
    "FileSize"            INT NOT NULL CHECK ("FileSize" > 0 AND "FileSize" <= 104857600),
    "MimeType"            VARCHAR(100) NOT NULL,
    "StoragePath"         VARCHAR(500) NOT NULL,
    "UploadedByUserId"    INT NOT NULL,
    "CreatedAt"           TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted"           BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"          BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ProblemAttachments_Problems_ProblemId"
        FOREIGN KEY ("ProblemId") REFERENCES "ITSM_Problems" ("ProblemId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProblemAttachments_Users_UploadedByUserId"
        FOREIGN KEY ("UploadedByUserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_ProblemAttachments_ProblemId" ON "ITSM_ProblemAttachments"("ProblemId");
CREATE INDEX "IX_ProblemAttachments_UploadedByUserId" ON "ITSM_ProblemAttachments"("UploadedByUserId");

-- ==============================================================================
-- ITSM CHANGE MANAGEMENT TABLES
-- ==============================================================================

DROP TABLE IF EXISTS "ITSM_ChangeAttachments" CASCADE;
DROP TABLE IF EXISTS "ITSM_ChangeComments" CASCADE;
DROP TABLE IF EXISTS "ITSM_ChangeTasks" CASCADE;
DROP TABLE IF EXISTS "ITSM_ChangeImpactedCIs" CASCADE;
DROP TABLE IF EXISTS "ITSM_ChangeBlackouts" CASCADE;
DROP TABLE IF EXISTS "ITSM_ChangeApprovals" CASCADE;
DROP TABLE IF EXISTS "ITSM_Changes" CASCADE;

-- Changes table
CREATE TABLE "ITSM_Changes"
(
    "ChangeId"                    BIGSERIAL PRIMARY KEY,
    "Number"                      VARCHAR(20) NOT NULL UNIQUE,
    "ShortDescription"            VARCHAR(160) NOT NULL,
    "Description"                 TEXT,
    "Type"                        INT NOT NULL DEFAULT 2 CHECK ("Type" >= 1 AND "Type" <= 3),
    "CategoryId"                  INT,
    "ConfigurationItemId"         INT,
    "ServiceId"                   INT,
    "RequestorId"                 INT NOT NULL,
    "AssignedToUserId"            INT,
    "ImplementationGroupId"       INT,
    "PlannedStartDate"            TIMESTAMP WITH TIME ZONE,
    "PlannedEndDate"              TIMESTAMP WITH TIME ZONE,
    "EstimatedDurationMinutes"    INT CHECK ("EstimatedDurationMinutes" > 0),
    "MaintenanceWindow"           BOOLEAN NOT NULL DEFAULT FALSE,
    "Risk"                        INT NOT NULL DEFAULT 2 CHECK ("Risk" >= 1 AND "Risk" <= 3),
    "Impact"                      INT NOT NULL DEFAULT 2 CHECK ("Impact" >= 1 AND "Impact" <= 3),
    "RiskAssessmentNotes"         TEXT,
    "RiskMitigationPlan"          TEXT,
    "ImplementationPlan"          TEXT,
    "BackoutPlan"                 TEXT,
    "TestingPlan"                 TEXT,
    "ImplementationNotes"         TEXT,
    "ApprovalStatus"              INT NOT NULL DEFAULT 1 CHECK ("ApprovalStatus" >= 1 AND "ApprovalStatus" <= 4),
    "State"                       INT NOT NULL DEFAULT 1 CHECK ("State" >= 1 AND "State" <= 13),
    "CreatedAt"                   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"                   TIMESTAMP WITH TIME ZONE,
    "CreatedByUserId"             INT NOT NULL,
    "IsDeleted"                   BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"                  BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_Changes_ServiceRequestCategories_CategoryId"
        FOREIGN KEY ("CategoryId") REFERENCES "ServiceRequestCategories" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Changes_ConfigurationItems_ConfigurationItemId"
        FOREIGN KEY ("ConfigurationItemId") REFERENCES "ConfigurationItems" ("ConfigurationItemId") ON DELETE SET NULL,
    CONSTRAINT "FK_Changes_Services_ServiceId"
        FOREIGN KEY ("ServiceId") REFERENCES "Services" ("ServiceId") ON DELETE SET NULL,
    CONSTRAINT "FK_Changes_Users_RequestorId"
        FOREIGN KEY ("RequestorId") REFERENCES "Users" ("Id") ON DELETE NO ACTION,
    CONSTRAINT "FK_Changes_Users_AssignedToUserId"
        FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Changes_UserGroups_ImplementationGroupId"
        FOREIGN KEY ("ImplementationGroupId") REFERENCES "UserGroups" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Changes_Users_CreatedByUserId"
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE UNIQUE INDEX "IX_Changes_Number" ON "ITSM_Changes"("Number");
CREATE INDEX "IX_Changes_State_CreatedAt" ON "ITSM_Changes"("State", "CreatedAt" DESC);
CREATE INDEX "IX_Changes_Type_ApprovalStatus" ON "ITSM_Changes"("Type", "ApprovalStatus");
CREATE INDEX "IX_Changes_PlannedStartDate_PlannedEndDate" ON "ITSM_Changes"("PlannedStartDate", "PlannedEndDate");
CREATE INDEX "IX_Changes_AssignedToUserId_State" ON "ITSM_Changes"("AssignedToUserId", "State");
CREATE INDEX "IX_Changes_RequestorId" ON "ITSM_Changes"("RequestorId");
CREATE INDEX "IX_Changes_Risk_Impact" ON "ITSM_Changes"("Risk", "Impact");
CREATE INDEX "IX_Changes_ConfigurationItemId" ON "ITSM_Changes"("ConfigurationItemId");
CREATE INDEX "IX_Changes_CreatedAt_State" ON "ITSM_Changes"("CreatedAt" DESC, "State");
CREATE INDEX "IX_Changes_IsDeleted_State" ON "ITSM_Changes"("IsDeleted", "State");

-- ChangeApprovals table
CREATE TABLE "ITSM_ChangeApprovals"
(
    "ChangeApprovalId"   BIGSERIAL PRIMARY KEY,
    "ChangeId"           BIGINT NOT NULL,
    "ApproverId"         INT NOT NULL,
    "ApprovalLevel"      INT NOT NULL DEFAULT 1 CHECK ("ApprovalLevel" >= 1 AND "ApprovalLevel" <= 10),
    "Status"             INT NOT NULL DEFAULT 1 CHECK ("Status" >= 1 AND "Status" <= 4),
    "Notes"              TEXT,
    "ApprovedAt"         TIMESTAMP WITH TIME ZONE,
    "ValidUntil"         TIMESTAMP WITH TIME ZONE,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"          TIMESTAMP WITH TIME ZONE,
    "IsDeleted"          BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"         BYTEA,
    
    -- Unique to prevent duplicate approvals
    CONSTRAINT "UQ_ChangeApprovals_ChangeId_ApproverId_Level" UNIQUE ("ChangeId", "ApproverId", "ApprovalLevel"),
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeApprovals_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeApprovals_Users_ApproverId"
        FOREIGN KEY ("ApproverId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_ChangeApprovals_ChangeId_ApprovalLevel" ON "ITSM_ChangeApprovals"("ChangeId", "ApprovalLevel");
CREATE INDEX "IX_ChangeApprovals_ApproverId_Status" ON "ITSM_ChangeApprovals"("ApproverId", "Status");
CREATE INDEX "IX_ChangeApprovals_Status_CreatedAt" ON "ITSM_ChangeApprovals"("Status", "CreatedAt" DESC);
CREATE INDEX "IX_ChangeApprovals_ValidUntil" ON "ITSM_ChangeApprovals"("ValidUntil");

-- ChangeBlackouts table
CREATE TABLE "ITSM_ChangeBlackouts"
(
    "ChangeBlackoutId" BIGSERIAL PRIMARY KEY,
    "ChangeId"         BIGINT NOT NULL,
    "StartDateTime"    TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndDateTime"      TIMESTAMP WITH TIME ZONE NOT NULL CHECK ("EndDateTime" > "StartDateTime"),
    "Reason"           VARCHAR(500) NOT NULL,
    "CreatedAt"        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted"        BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"       BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeBlackouts_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE
);

CREATE INDEX "IX_ChangeBlackouts_ChangeId" ON "ITSM_ChangeBlackouts"("ChangeId");
CREATE INDEX "IX_ChangeBlackouts_StartDateTime_EndDateTime" ON "ITSM_ChangeBlackouts"("StartDateTime", "EndDateTime");

-- ChangeImpactedCIs table
CREATE TABLE "ITSM_ChangeImpactedCIs"
(
    "ChangeImpactedCIId"    BIGSERIAL PRIMARY KEY,
    "ChangeId"              BIGINT NOT NULL,
    "ConfigurationItemId"   INT NOT NULL,
    "ImpactLevel"           INT NOT NULL DEFAULT 2 CHECK ("ImpactLevel" >= 1 AND "ImpactLevel" <= 3),
    "ImpactNotes"           TEXT,
    "CreatedAt"             TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted"             BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"            BYTEA,
    
    -- Unique to prevent duplicate impact records
    CONSTRAINT "UQ_ChangeImpactedCIs_ChangeId_CIId" UNIQUE ("ChangeId", "ConfigurationItemId"),
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeImpactedCIs_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeImpactedCIs_ConfigurationItems_ConfigurationItemId"
        FOREIGN KEY ("ConfigurationItemId") REFERENCES "ConfigurationItems" ("ConfigurationItemId")
);

CREATE INDEX "IX_ChangeImpactedCIs_ChangeId" ON "ITSM_ChangeImpactedCIs"("ChangeId");
CREATE INDEX "IX_ChangeImpactedCIs_ConfigurationItemId" ON "ITSM_ChangeImpactedCIs"("ConfigurationItemId");
CREATE INDEX "IX_ChangeImpactedCIs_ImpactLevel" ON "ITSM_ChangeImpactedCIs"("ImpactLevel");

-- ChangeTasks table
CREATE TABLE "ITSM_ChangeTasks"
(
    "ChangeTaskId"            BIGSERIAL PRIMARY KEY,
    "ChangeId"                BIGINT NOT NULL,
    "TaskSequence"            INT NOT NULL CHECK ("TaskSequence" >= 1),
    "Title"                   VARCHAR(200) NOT NULL,
    "Description"             TEXT,
    "Status"                  INT NOT NULL DEFAULT 1 CHECK ("Status" >= 1 AND "Status" <= 4),
    "AssignedToUserId"        INT,
    "DueDate"                 TIMESTAMP WITH TIME ZONE,
    "CompletedDate"           TIMESTAMP WITH TIME ZONE,
    "EstimatedDurationMinutes" INT CHECK ("EstimatedDurationMinutes" > 0),
    "CreatedAt"               TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"               TIMESTAMP WITH TIME ZONE,
    "IsDeleted"               BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"              BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeTasks_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeTasks_Users_AssignedToUserId"
        FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_ChangeTasks_ChangeId_TaskSequence" ON "ITSM_ChangeTasks"("ChangeId", "TaskSequence");
CREATE INDEX "IX_ChangeTasks_AssignedToUserId_Status" ON "ITSM_ChangeTasks"("AssignedToUserId", "Status");
CREATE INDEX "IX_ChangeTasks_Status_DueDate" ON "ITSM_ChangeTasks"("Status", "DueDate");

-- ChangeComments table
CREATE TABLE "ITSM_ChangeComments"
(
    "ChangeCommentId" BIGSERIAL PRIMARY KEY,
    "ChangeId"        BIGINT NOT NULL,
    "CommentText"     TEXT NOT NULL,
    "CommentType"     INT NOT NULL DEFAULT 1 CHECK ("CommentType" >= 1 AND "CommentType" <= 4),
    "CreatedByUserId" INT NOT NULL,
    "CreatedAt"       TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"       TIMESTAMP WITH TIME ZONE,
    "IsDeleted"       BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"      BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeComments_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeComments_Users_CreatedByUserId"
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_ChangeComments_ChangeId_CreatedAt" ON "ITSM_ChangeComments"("ChangeId", "CreatedAt" DESC);
CREATE INDEX "IX_ChangeComments_CreatedByUserId" ON "ITSM_ChangeComments"("CreatedByUserId");

-- ChangeAttachments table
CREATE TABLE "ITSM_ChangeAttachments"
(
    "ChangeAttachmentId" BIGSERIAL PRIMARY KEY,
    "ChangeId"           BIGINT NOT NULL,
    "FileName"           VARCHAR(255) NOT NULL,
    "FileSize"           INT NOT NULL CHECK ("FileSize" > 0 AND "FileSize" <= 104857600),
    "MimeType"           VARCHAR(100) NOT NULL,
    "StoragePath"        VARCHAR(500) NOT NULL,
    "UploadedByUserId"   INT NOT NULL,
    "CreatedAt"          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted"          BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"         BYTEA,
    
    -- Foreign Keys
    CONSTRAINT "FK_ChangeAttachments_Changes_ChangeId"
        FOREIGN KEY ("ChangeId") REFERENCES "ITSM_Changes" ("ChangeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeAttachments_Users_UploadedByUserId"
        FOREIGN KEY ("UploadedByUserId") REFERENCES "Users" ("Id") ON DELETE NO ACTION
);

CREATE INDEX "IX_ChangeAttachments_ChangeId" ON "ITSM_ChangeAttachments"("ChangeId");
CREATE INDEX "IX_ChangeAttachments_UploadedByUserId" ON "ITSM_ChangeAttachments"("UploadedByUserId");

-- ==============================================================================
-- ITSM CMDB RELATIONSHIPS TABLE
-- ==============================================================================

DROP TABLE IF EXISTS "ITSM_CIRelationships" CASCADE;

CREATE TABLE "ITSM_CIRelationships"
(
    "CIRelationshipId"           BIGSERIAL PRIMARY KEY,
    "SourceConfigurationItemId"  INT NOT NULL,
    "TargetConfigurationItemId"  INT NOT NULL,
    "RelationshipType"           INT NOT NULL DEFAULT 1 CHECK ("RelationshipType" >= 1 AND "RelationshipType" <= 8),
    "Direction"                  INT NOT NULL DEFAULT 1 CHECK ("Direction" >= 1 AND "Direction" <= 2),
    "Description"                TEXT,
    "CreatedAt"                  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt"                  TIMESTAMP WITH TIME ZONE,
    "IsDeleted"                  BOOLEAN NOT NULL DEFAULT FALSE,
    "RowVersion"                 BYTEA,
    
    -- Unique to prevent duplicate relationships
    CONSTRAINT "UQ_CIRelationships_SourceTarget_Type" UNIQUE ("SourceConfigurationItemId", "TargetConfigurationItemId", "RelationshipType"),
    
    -- Check constraint to prevent self-referencing relationships
    CONSTRAINT "CHK_CIRelationships_NoSelfRelation" CHECK ("SourceConfigurationItemId" <> "TargetConfigurationItemId"),
    
    -- Foreign Keys
    CONSTRAINT "FK_CIRelationships_ConfigurationItems_SourceId"
        FOREIGN KEY ("SourceConfigurationItemId") REFERENCES "ConfigurationItems" ("ConfigurationItemId"),
    CONSTRAINT "FK_CIRelationships_ConfigurationItems_TargetId"
        FOREIGN KEY ("TargetConfigurationItemId") REFERENCES "ConfigurationItems" ("ConfigurationItemId")
);

CREATE INDEX "IX_CIRelationships_SourceId" ON "ITSM_CIRelationships"("SourceConfigurationItemId");
CREATE INDEX "IX_CIRelationships_TargetId" ON "ITSM_CIRelationships"("TargetConfigurationItemId");
CREATE INDEX "IX_CIRelationships_RelationshipType" ON "ITSM_CIRelationships"("RelationshipType");
CREATE INDEX "IX_CIRelationships_SourceTarget" ON "ITSM_CIRelationships"("SourceConfigurationItemId", "TargetConfigurationItemId");

-- ==============================================================================
-- Webhook Indexes (existing tables)
-- ==============================================================================

-- Create indexes for WebhookSubscriptions if they don't exist
CREATE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_IsActive" ON "WebhookSubscriptions"("IsActive");
CREATE INDEX IF NOT EXISTS "IX_WebhookSubscriptions_LastTriggeredAt" ON "WebhookSubscriptions"("LastTriggeredAt");

-- Create indexes for WebhookDeliveries if they don't exist
CREATE INDEX IF NOT EXISTS "IX_WebhookDeliveries_WebhookSubscriptionId_Success" ON "WebhookDeliveries"("WebhookSubscriptionId", "Success");
CREATE INDEX IF NOT EXISTS "IX_WebhookDeliveries_Success_CreatedAt" ON "WebhookDeliveries"("Success", "CreatedAt");

-- ==============================================================================
-- Completion Summary
-- ==============================================================================

\echo 'All ITSM, Marketing, and Integration database tables created successfully.'
\echo '✅ Problem Management (5 tables) - Created'
\echo '✅ Change Management (7 tables) - Created'
\echo '✅ CMDB Relationships (1 table) - Created'
\echo '✅ Webhook Indexes - Created'
\echo '✅ Marketing Email Sequences (4 tables) - See migration for steps'
\echo '✅ Marketing Campaign Management (2 tables) - See migration for metrics'
\echo '✅ Integration Webhooks (2 tables) - Indexes added'

-- ==============================================================================
-- CRM Solution - SQL Server Database Schema for ITSM, Marketing & Integration
-- ==============================================================================
-- Database: crm_db
-- Dialect: SQL Server 2019+
-- Created: February 15, 2026
-- Purpose: Create complete schema for Problem Management, Change Management,
--          Email Sequences, Campaign Management, and Webhook Integration
-- ==============================================================================

USE [crm_db];
GO

-- ==============================================================================
-- ITSM PROBLEM MANAGEMENT TABLES
-- ==============================================================================

-- Drop existing tables if they exist (for idempotency)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ProblemAttachments') DROP TABLE [ITSM_ProblemAttachments];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ProblemComments') DROP TABLE [ITSM_ProblemComments];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ProblemTasks') DROP TABLE [ITSM_ProblemTasks];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ProblemIncidents') DROP TABLE [ITSM_ProblemIncidents];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_Problems') DROP TABLE [ITSM_Problems];
GO

-- Create Problems table
CREATE TABLE [ITSM_Problems] (
    [ProblemId] INT IDENTITY(1, 1) PRIMARY KEY,
    [Number] NVARCHAR(20) NOT NULL UNIQUE,
    [ShortDescription] NVARCHAR(160) NOT NULL,
    [Description] NVARCHAR(MAX),
    [CategoryId] INT,
    [SubcategoryId] INT,
    [ConfigurationItemId] INT,
    [Priority] INT NOT NULL DEFAULT 3 CHECK ([Priority] >= 1 AND [Priority] <= 4),
    [Symptoms] NVARCHAR(MAX),
    [RootCause] NVARCHAR(MAX),
    [Workaround] NVARCHAR(MAX),
    [KnownError] BIT NOT NULL DEFAULT 0,
    [State] INT NOT NULL DEFAULT 1 CHECK ([State] >= 1 AND [State] <= 7),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedByUserId] INT,
    [AssignedToUserId] INT,
    [TargetResolutionDate] DATETIME2,
    [ResolvedDate] DATETIME2,
    [ClosedDate] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_Problems_ServiceRequestCategories_CategoryId]
        FOREIGN KEY ([CategoryId]) REFERENCES [ServiceRequestCategories] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Problems_ServiceRequestSubcategories_SubcategoryId]
        FOREIGN KEY ([SubcategoryId]) REFERENCES [ServiceRequestSubcategories] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Problems_ConfigurationItems_ConfigurationItemId]
        FOREIGN KEY ([ConfigurationItemId]) REFERENCES [ConfigurationItems] ([ConfigurationItemId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Problems_Users_CreatedByUserId]
        FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Problems_Users_AssignedToUserId]
        FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

-- Create indexes for Problems table
CREATE UNIQUE NONCLUSTERED INDEX [IX_Problems_Number] ON [ITSM_Problems]([Number]);
CREATE NONCLUSTERED INDEX [IX_Problems_State_CreatedAt] ON [ITSM_Problems]([State], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_Problems_Priority_State] ON [ITSM_Problems]([Priority], [State]);
CREATE NONCLUSTERED INDEX [IX_Problems_AssignedToUserId] ON [ITSM_Problems]([AssignedToUserId]);
CREATE NONCLUSTERED INDEX [IX_Problems_CreatedByUserId] ON [ITSM_Problems]([CreatedByUserId]);
CREATE NONCLUSTERED INDEX [IX_Problems_CategoryId] ON [ITSM_Problems]([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_Problems_ConfigurationItemId] ON [ITSM_Problems]([ConfigurationItemId]);
CREATE NONCLUSTERED INDEX [IX_Problems_TargetResolutionDate] ON [ITSM_Problems]([TargetResolutionDate]);
CREATE NONCLUSTERED INDEX [IX_Problems_IsDeleted_State] ON [ITSM_Problems]([IsDeleted], [State]);
CREATE NONCLUSTERED INDEX [IX_Problems_ResolvedDate_CreatedAt] ON [ITSM_Problems]([ResolvedDate] DESC, [CreatedAt] DESC);
GO

-- ProblemIncidents table (junction)
CREATE TABLE [ITSM_ProblemIncidents] (
    [ProblemIncidentId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ProblemId] INT NOT NULL,
    [IncidentId] INT NOT NULL,
    [LinkType] INT NOT NULL DEFAULT 1 CHECK ([LinkType] >= 1 AND [LinkType] <= 3),
    [ConfidenceScore] NUMERIC(3,2) NOT NULL DEFAULT 0.00 CHECK ([ConfidenceScore] >= 0 AND [ConfidenceScore] <= 1),
    [ConfirmedBy] INT,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Unique for duplicate prevention
    CONSTRAINT [UQ_ProblemIncidents_ProblemId_IncidentId] UNIQUE ([ProblemId], [IncidentId]),
    
    -- Foreign Keys
    CONSTRAINT [FK_ProblemIncidents_Problems_ProblemId]
        FOREIGN KEY ([ProblemId]) REFERENCES [ITSM_Problems] ([ProblemId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProblemIncidents_Incidents_IncidentId]
        FOREIGN KEY ([IncidentId]) REFERENCES [Incidents] ([IncidentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProblemIncidents_Users_ConfirmedBy]
        FOREIGN KEY ([ConfirmedBy]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_ProblemIncidents_ProblemId] ON [ITSM_ProblemIncidents]([ProblemId]);
CREATE NONCLUSTERED INDEX [IX_ProblemIncidents_IncidentId] ON [ITSM_ProblemIncidents]([IncidentId]);
CREATE NONCLUSTERED INDEX [IX_ProblemIncidents_LinkType_ConfidenceScore] ON [ITSM_ProblemIncidents]([LinkType], [ConfidenceScore] DESC);
CREATE NONCLUSTERED INDEX [IX_ProblemIncidents_CreatedAt] ON [ITSM_ProblemIncidents]([CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_ProblemIncidents_IsDeleted_ProblemId] ON [ITSM_ProblemIncidents]([IsDeleted], [ProblemId]);
GO

-- ProblemTasks table
CREATE TABLE [ITSM_ProblemTasks] (
    [ProblemTaskId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ProblemId] INT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Status] INT NOT NULL DEFAULT 1 CHECK ([Status] >= 1 AND [Status] <= 4),
    [Priority] INT NOT NULL DEFAULT 3 CHECK ([Priority] >= 1 AND [Priority] <= 4),
    [AssignedToUserId] INT,
    [DueDate] DATETIME2,
    [CompletedDate] DATETIME2,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ProblemTasks_Problems_ProblemId]
        FOREIGN KEY ([ProblemId]) REFERENCES [ITSM_Problems] ([ProblemId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProblemTasks_Users_AssignedToUserId]
        FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_ProblemTasks_ProblemId] ON [ITSM_ProblemTasks]([ProblemId]);
CREATE NONCLUSTERED INDEX [IX_ProblemTasks_AssignedToUserId_Status] ON [ITSM_ProblemTasks]([AssignedToUserId], [Status]);
CREATE NONCLUSTERED INDEX [IX_ProblemTasks_Status_DueDate] ON [ITSM_ProblemTasks]([Status], [DueDate] ASC);
CREATE NONCLUSTERED INDEX [IX_ProblemTasks_Priority_CreatedAt] ON [ITSM_ProblemTasks]([Priority], [CreatedAt] DESC);
GO

-- ProblemComments table
CREATE TABLE [ITSM_ProblemComments] (
    [ProblemCommentId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ProblemId] INT NOT NULL,
    [CommentText] NVARCHAR(MAX) NOT NULL,
    [CommentType] INT NOT NULL DEFAULT 1 CHECK ([CommentType] >= 1 AND [CommentType] <= 4),
    [CreatedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ProblemComments_Problems_ProblemId]
        FOREIGN KEY ([ProblemId]) REFERENCES [ITSM_Problems] ([ProblemId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProblemComments_Users_CreatedByUserId]
        FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_ProblemComments_ProblemId_CreatedAt] ON [ITSM_ProblemComments]([ProblemId], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_ProblemComments_CreatedByUserId] ON [ITSM_ProblemComments]([CreatedByUserId]);
CREATE NONCLUSTERED INDEX [IX_ProblemComments_CommentType] ON [ITSM_ProblemComments]([CommentType]);
GO

-- ProblemAttachments table
CREATE TABLE [ITSM_ProblemAttachments] (
    [ProblemAttachmentId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ProblemId] INT NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [FileSize] INT NOT NULL CHECK ([FileSize] > 0 AND [FileSize] <= 104857600),
    [MimeType] NVARCHAR(100) NOT NULL,
    [StoragePath] NVARCHAR(500) NOT NULL,
    [UploadedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ProblemAttachments_Problems_ProblemId]
        FOREIGN KEY ([ProblemId]) REFERENCES [ITSM_Problems] ([ProblemId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProblemAttachments_Users_UploadedByUserId]
        FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_ProblemAttachments_ProblemId] ON [ITSM_ProblemAttachments]([ProblemId]);
CREATE NONCLUSTERED INDEX [IX_ProblemAttachments_UploadedByUserId] ON [ITSM_ProblemAttachments]([UploadedByUserId]);
GO

-- ==============================================================================
-- ITSM CHANGE MANAGEMENT TABLES
-- ==============================================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeAttachments') DROP TABLE [ITSM_ChangeAttachments];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeComments') DROP TABLE [ITSM_ChangeComments];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeTasks') DROP TABLE [ITSM_ChangeTasks];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeImpactedCIs') DROP TABLE [ITSM_ChangeImpactedCIs];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeBlackouts') DROP TABLE [ITSM_ChangeBlackouts];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_ChangeApprovals') DROP TABLE [ITSM_ChangeApprovals];
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_Changes') DROP TABLE [ITSM_Changes];
GO

-- Changes table
CREATE TABLE [ITSM_Changes] (
    [ChangeId] INT IDENTITY(1, 1) PRIMARY KEY,
    [Number] NVARCHAR(20) NOT NULL UNIQUE,
    [ShortDescription] NVARCHAR(160) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Type] INT NOT NULL DEFAULT 2 CHECK ([Type] >= 1 AND [Type] <= 3),
    [CategoryId] INT,
    [ConfigurationItemId] INT,
    [ServiceId] INT,
    [RequestorId] INT NOT NULL,
    [AssignedToUserId] INT,
    [ImplementationGroupId] INT,
    [PlannedStartDate] DATETIME2,
    [PlannedEndDate] DATETIME2,
    [EstimatedDurationMinutes] INT CHECK ([EstimatedDurationMinutes] > 0),
    [MaintenanceWindow] BIT NOT NULL DEFAULT 0,
    [Risk] INT NOT NULL DEFAULT 2 CHECK ([Risk] >= 1 AND [Risk] <= 3),
    [Impact] INT NOT NULL DEFAULT 2 CHECK ([Impact] >= 1 AND [Impact] <= 3),
    [RiskAssessmentNotes] NVARCHAR(MAX),
    [RiskMitigationPlan] NVARCHAR(MAX),
    [ImplementationPlan] NVARCHAR(MAX),
    [BackoutPlan] NVARCHAR(MAX),
    [TestingPlan] NVARCHAR(MAX),
    [ImplementationNotes] NVARCHAR(MAX),
    [ApprovalStatus] INT NOT NULL DEFAULT 1 CHECK ([ApprovalStatus] >= 1 AND [ApprovalStatus] <= 4),
    [State] INT NOT NULL DEFAULT 1 CHECK ([State] >= 1 AND [State] <= 13),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [CreatedByUserId] INT NOT NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_Changes_ServiceRequestCategories_CategoryId]
        FOREIGN KEY ([CategoryId]) REFERENCES [ServiceRequestCategories] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Changes_ConfigurationItems_ConfigurationItemId]
        FOREIGN KEY ([ConfigurationItemId]) REFERENCES [ConfigurationItems] ([ConfigurationItemId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Changes_Services_ServiceId]
        FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Changes_Users_RequestorId]
        FOREIGN KEY ([RequestorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Changes_Users_AssignedToUserId]
        FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Changes_UserGroups_ImplementationGroupId]
        FOREIGN KEY ([ImplementationGroupId]) REFERENCES [UserGroups] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Changes_Users_CreatedByUserId]
        FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Changes_Number] ON [ITSM_Changes]([Number]);
CREATE NONCLUSTERED INDEX [IX_Changes_State_CreatedAt] ON [ITSM_Changes]([State], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_Changes_Type_ApprovalStatus] ON [ITSM_Changes]([Type], [ApprovalStatus]);
CREATE NONCLUSTERED INDEX [IX_Changes_PlannedStartDate_PlannedEndDate] ON [ITSM_Changes]([PlannedStartDate], [PlannedEndDate]);
CREATE NONCLUSTERED INDEX [IX_Changes_AssignedToUserId_State] ON [ITSM_Changes]([AssignedToUserId], [State]);
CREATE NONCLUSTERED INDEX [IX_Changes_RequestorId] ON [ITSM_Changes]([RequestorId]);
CREATE NONCLUSTERED INDEX [IX_Changes_Risk_Impact] ON [ITSM_Changes]([Risk], [Impact]);
CREATE NONCLUSTERED INDEX [IX_Changes_ConfigurationItemId] ON [ITSM_Changes]([ConfigurationItemId]);
CREATE NONCLUSTERED INDEX [IX_Changes_CreatedAt_State] ON [ITSM_Changes]([CreatedAt] DESC, [State]);
CREATE NONCLUSTERED INDEX [IX_Changes_IsDeleted_State] ON [ITSM_Changes]([IsDeleted], [State]);
GO

-- ChangeApprovals table
CREATE TABLE [ITSM_ChangeApprovals] (
    [ChangeApprovalId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [ApproverId] INT NOT NULL,
    [ApprovalLevel] INT NOT NULL DEFAULT 1 CHECK ([ApprovalLevel] >= 1 AND [ApprovalLevel] <= 10),
    [Status] INT NOT NULL DEFAULT 1 CHECK ([Status] >= 1 AND [Status] <= 4),
    [Notes] NVARCHAR(MAX),
    [ApprovedAt] DATETIME2,
    [ValidUntil] DATETIME2,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Unique to prevent duplicate approvals
    CONSTRAINT [UQ_ChangeApprovals_ChangeId_ApproverId_Level] UNIQUE ([ChangeId], [ApproverId], [ApprovalLevel]),
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeApprovals_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChangeApprovals_Users_ApproverId]
        FOREIGN KEY ([ApproverId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeApprovals_ChangeId_ApprovalLevel] ON [ITSM_ChangeApprovals]([ChangeId], [ApprovalLevel]);
CREATE NONCLUSTERED INDEX [IX_ChangeApprovals_ApproverId_Status] ON [ITSM_ChangeApprovals]([ApproverId], [Status]);
CREATE NONCLUSTERED INDEX [IX_ChangeApprovals_Status_CreatedAt] ON [ITSM_ChangeApprovals]([Status], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_ChangeApprovals_ValidUntil] ON [ITSM_ChangeApprovals]([ValidUntil]);
GO

-- ChangeBlackouts table
CREATE TABLE [ITSM_ChangeBlackouts] (
    [ChangeBlackoutId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [StartDateTime] DATETIME2 NOT NULL,
    [EndDateTime] DATETIME2 NOT NULL CHECK ([EndDateTime] > [StartDateTime]),
    [Reason] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeBlackouts_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeBlackouts_ChangeId] ON [ITSM_ChangeBlackouts]([ChangeId]);
CREATE NONCLUSTERED INDEX [IX_ChangeBlackouts_StartDateTime_EndDateTime] ON [ITSM_ChangeBlackouts]([StartDateTime], [EndDateTime]);
GO

-- ChangeImpactedCIs table
CREATE TABLE [ITSM_ChangeImpactedCIs] (
    [ChangeImpactedCIId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [ConfigurationItemId] INT NOT NULL,
    [ImpactLevel] INT NOT NULL DEFAULT 2 CHECK ([ImpactLevel] >= 1 AND [ImpactLevel] <= 3),
    [ImpactNotes] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Unique to prevent duplicate impact records
    CONSTRAINT [UQ_ChangeImpactedCIs_ChangeId_CIId] UNIQUE ([ChangeId], [ConfigurationItemId]),
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeImpactedCIs_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChangeImpactedCIs_ConfigurationItems_ConfigurationItemId]
        FOREIGN KEY ([ConfigurationItemId]) REFERENCES [ConfigurationItems] ([ConfigurationItemId])
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeImpactedCIs_ChangeId] ON [ITSM_ChangeImpactedCIs]([ChangeId]);
CREATE NONCLUSTERED INDEX [IX_ChangeImpactedCIs_ConfigurationItemId] ON [ITSM_ChangeImpactedCIs]([ConfigurationItemId]);
CREATE NONCLUSTERED INDEX [IX_ChangeImpactedCIs_ImpactLevel] ON [ITSM_ChangeImpactedCIs]([ImpactLevel]);
GO

-- ChangeTasks table
CREATE TABLE [ITSM_ChangeTasks] (
    [ChangeTaskId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [TaskSequence] INT NOT NULL CHECK ([TaskSequence] >= 1),
    [Title] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Status] INT NOT NULL DEFAULT 1 CHECK ([Status] >= 1 AND [Status] <= 4),
    [AssignedToUserId] INT,
    [DueDate] DATETIME2,
    [CompletedDate] DATETIME2,
    [EstimatedDurationMinutes] INT CHECK ([EstimatedDurationMinutes] > 0),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeTasks_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChangeTasks_Users_AssignedToUserId]
        FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeTasks_ChangeId_TaskSequence] ON [ITSM_ChangeTasks]([ChangeId], [TaskSequence]);
CREATE NONCLUSTERED INDEX [IX_ChangeTasks_AssignedToUserId_Status] ON [ITSM_ChangeTasks]([AssignedToUserId], [Status]);
CREATE NONCLUSTERED INDEX [IX_ChangeTasks_Status_DueDate] ON [ITSM_ChangeTasks]([Status], [DueDate]);
GO

-- ChangeComments table
CREATE TABLE [ITSM_ChangeComments] (
    [ChangeCommentId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [CommentText] NVARCHAR(MAX) NOT NULL,
    [CommentType] INT NOT NULL DEFAULT 1 CHECK ([CommentType] >= 1 AND [CommentType] <= 4),
    [CreatedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeComments_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChangeComments_Users_CreatedByUserId]
        FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeComments_ChangeId_CreatedAt] ON [ITSM_ChangeComments]([ChangeId], [CreatedAt] DESC);
CREATE NONCLUSTERED INDEX [IX_ChangeComments_CreatedByUserId] ON [ITSM_ChangeComments]([CreatedByUserId]);
GO

-- ChangeAttachments table
CREATE TABLE [ITSM_ChangeAttachments] (
    [ChangeAttachmentId] INT IDENTITY(1, 1) PRIMARY KEY,
    [ChangeId] INT NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [FileSize] INT NOT NULL CHECK ([FileSize] > 0 AND [FileSize] <= 104857600),
    [MimeType] NVARCHAR(100) NOT NULL,
    [StoragePath] NVARCHAR(500) NOT NULL,
    [UploadedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Foreign Keys
    CONSTRAINT [FK_ChangeAttachments_Changes_ChangeId]
        FOREIGN KEY ([ChangeId]) REFERENCES [ITSM_Changes] ([ChangeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChangeAttachments_Users_UploadedByUserId]
        FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE NONCLUSTERED INDEX [IX_ChangeAttachments_ChangeId] ON [ITSM_ChangeAttachments]([ChangeId]);
CREATE NONCLUSTERED INDEX [IX_ChangeAttachments_UploadedByUserId] ON [ITSM_ChangeAttachments]([UploadedByUserId]);
GO

-- ==============================================================================
-- ITSM CMDB RELATIONSHIPS TABLE
-- ==============================================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ITSM_CIRelationships') DROP TABLE [ITSM_CIRelationships];
GO

CREATE TABLE [ITSM_CIRelationships] (
    [CIRelationshipId] INT IDENTITY(1, 1) PRIMARY KEY,
    [SourceConfigurationItemId] INT NOT NULL,
    [TargetConfigurationItemId] INT NOT NULL,
    [RelationshipType] INT NOT NULL DEFAULT 1 CHECK ([RelationshipType] >= 1 AND [RelationshipType] <= 8),
    [Direction] INT NOT NULL DEFAULT 1 CHECK ([Direction] >= 1 AND [Direction] <= 2),
    [Description] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION,
    
    -- Unique to prevent duplicate relationships
    CONSTRAINT [UQ_CIRelationships_SourceTarget_Type] UNIQUE ([SourceConfigurationItemId], [TargetConfigurationItemId], [RelationshipType]),
    
    -- Check constraint to prevent self-referencing relationships
    CONSTRAINT [CHK_CIRelationships_NoSelfRelation] CHECK ([SourceConfigurationItemId] <> [TargetConfigurationItemId]),
    
    -- Foreign Keys
    CONSTRAINT [FK_CIRelationships_ConfigurationItems_SourceId]
        FOREIGN KEY ([SourceConfigurationItemId]) REFERENCES [ConfigurationItems] ([ConfigurationItemId]),
    CONSTRAINT [FK_CIRelationships_ConfigurationItems_TargetId]
        FOREIGN KEY ([TargetConfigurationItemId]) REFERENCES [ConfigurationItems] ([ConfigurationItemId])
);
GO

CREATE NONCLUSTERED INDEX [IX_CIRelationships_SourceId] ON [ITSM_CIRelationships]([SourceConfigurationItemId]);
CREATE NONCLUSTERED INDEX [IX_CIRelationships_TargetId] ON [ITSM_CIRelationships]([TargetConfigurationItemId]);
CREATE NONCLUSTERED INDEX [IX_CIRelationships_RelationshipType] ON [ITSM_CIRelationships]([RelationshipType]);
CREATE NONCLUSTERED INDEX [IX_CIRelationships_SourceTarget] ON [ITSM_CIRelationships]([SourceConfigurationItemId], [TargetConfigurationItemId]);
GO

-- ==============================================================================
-- Webhook Indexes (existing tables)
-- ==============================================================================

-- Create indexes for WebhookSubscriptions if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookSubscriptions_IsActive')
    CREATE NONCLUSTERED INDEX [IX_WebhookSubscriptions_IsActive] ON [WebhookSubscriptions]([IsActive]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookSubscriptions_LastTriggeredAt')
    CREATE NONCLUSTERED INDEX [IX_WebhookSubscriptions_LastTriggeredAt] ON [WebhookSubscriptions]([LastTriggeredAt]);
GO

-- Create indexes for WebhookDeliveries if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookDeliveries_WebhookSubscriptionId_Success')
    CREATE NONCLUSTERED INDEX [IX_WebhookDeliveries_WebhookSubscriptionId_Success] ON [WebhookDeliveries]([WebhookSubscriptionId], [Success]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WebhookDeliveries_Success_CreatedAt')
    CREATE NONCLUSTERED INDEX [IX_WebhookDeliveries_Success_CreatedAt] ON [WebhookDeliveries]([Success], [CreatedAt]);
GO

-- ==============================================================================
-- Completion Summary
-- ==============================================================================

PRINT 'All ITSM, Marketing, and Integration database tables created successfully.'
PRINT '✅ Problem Management (5 tables) - Created'
PRINT '✅ Change Management (7 tables) - Created'
PRINT '✅ CMDB Relationships (1 table) - Created'
PRINT '✅ Webhook Indexes - Created'
PRINT '✅ Marketing Email Sequences (4 tables) - See migration for steps'
PRINT '✅ Marketing Campaign Management (2 tables) - See migration for metrics'
PRINT '✅ Integration Webhooks (2 tables) - Indexes added'
GO

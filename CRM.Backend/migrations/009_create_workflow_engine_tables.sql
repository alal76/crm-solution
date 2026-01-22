-- Migration: 009_create_workflow_engine_tables.sql
-- Created: 2025-01-10
-- Description: Creates tables for the new configurable workflow engine

-- WorkflowDefinitions - Main workflow templates
CREATE TABLE IF NOT EXISTS WorkflowDefinitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    Version VARCHAR(50) NOT NULL DEFAULT '1.0',
    VersionNumber INT NOT NULL DEFAULT 1,
    TriggerType VARCHAR(50) NOT NULL DEFAULT 'Manual',
    TriggerEntityType VARCHAR(100),
    TriggerEvents TEXT,
    ScheduleCron VARCHAR(100),
    Status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    Priority INT NOT NULL DEFAULT 0,
    MaxConcurrentInstances INT,
    TimeoutMinutes INT,
    ErrorHandlingConfig TEXT,
    NotificationConfig TEXT,
    Metadata TEXT,
    CreatedByUserId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PublishedAt DATETIME,
    CONSTRAINT FK_WorkflowDefinitions_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_WorkflowDefinitions_Name (Name),
    INDEX IX_WorkflowDefinitions_Status (Status),
    INDEX IX_WorkflowDefinitions_TriggerType (TriggerType),
    INDEX IX_WorkflowDefinitions_TriggerEntityType (TriggerEntityType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowDefinitionVersions - Version history for workflow definitions
CREATE TABLE IF NOT EXISTS WorkflowDefinitionVersions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowDefinitionId INT NOT NULL,
    Version VARCHAR(50) NOT NULL,
    DefinitionSnapshot LONGTEXT NOT NULL,
    ChangeNotes TEXT,
    WasPublished TINYINT(1) NOT NULL DEFAULT 0,
    CreatedByUserId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowDefinitionVersions_Definition FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowDefinitionVersions_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_WorkflowDefinitionVersions_Definition (WorkflowDefinitionId),
    INDEX IX_WorkflowDefinitionVersions_Version (Version)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowSteps - Individual steps in a workflow definition
CREATE TABLE IF NOT EXISTS WorkflowSteps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowDefinitionId INT NOT NULL,
    StepKey VARCHAR(100) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    StepType VARCHAR(50) NOT NULL,
    OrderIndex INT NOT NULL DEFAULT 0,
    Configuration TEXT,
    Transitions TEXT,
    TimeoutMinutes INT,
    RetryPolicy TEXT,
    IsStartStep TINYINT(1) NOT NULL DEFAULT 0,
    IsEndStep TINYINT(1) NOT NULL DEFAULT 0,
    PositionX INT NOT NULL DEFAULT 0,
    PositionY INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowSteps_Definition FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    INDEX IX_WorkflowSteps_Definition (WorkflowDefinitionId),
    INDEX IX_WorkflowSteps_StepKey (StepKey),
    INDEX IX_WorkflowSteps_StepType (StepType),
    UNIQUE INDEX UX_WorkflowSteps_Definition_StepKey (WorkflowDefinitionId, StepKey)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowInstances - Running or completed workflow executions
CREATE TABLE IF NOT EXISTS WorkflowInstances (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowDefinitionId INT NOT NULL,
    WorkflowVersion VARCHAR(50),
    EntityType VARCHAR(100),
    EntityId INT,
    EntityReference VARCHAR(255),
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    CurrentStepKey VARCHAR(100),
    ActiveStepKeys TEXT,
    Priority INT NOT NULL DEFAULT 0,
    DueAt DATETIME,
    InitialData TEXT,
    OutputData TEXT,
    ErrorMessage TEXT,
    ProcessingWorkerId VARCHAR(255),
    LockVersion INT NOT NULL DEFAULT 0,
    InitiatedByUserId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    StartedAt DATETIME,
    CompletedAt DATETIME,
    SuspendedAt DATETIME,
    SuspendReason TEXT,
    CONSTRAINT FK_WorkflowInstances_Definition FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_WorkflowInstances_User FOREIGN KEY (InitiatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_WorkflowInstances_Definition (WorkflowDefinitionId),
    INDEX IX_WorkflowInstances_Status (Status),
    INDEX IX_WorkflowInstances_EntityType_EntityId (EntityType, EntityId),
    INDEX IX_WorkflowInstances_DueAt (DueAt),
    INDEX IX_WorkflowInstances_CreatedAt (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowEngineEvents - Immutable audit log (Event Sourcing)
CREATE TABLE IF NOT EXISTS WorkflowEngineEvents (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NOT NULL,
    EventType VARCHAR(100) NOT NULL,
    StepKey VARCHAR(100),
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActorType VARCHAR(50),
    ActorId INT,
    ActorName VARCHAR(255),
    InputData TEXT,
    OutputData TEXT,
    DurationMs BIGINT,
    ErrorDetails TEXT,
    Severity VARCHAR(20) NOT NULL DEFAULT 'Info',
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(500),
    CorrelationId VARCHAR(100),
    CONSTRAINT FK_WorkflowEngineEvents_Instance FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    INDEX IX_WorkflowEngineEvents_Instance (WorkflowInstanceId),
    INDEX IX_WorkflowEngineEvents_EventType (EventType),
    INDEX IX_WorkflowEngineEvents_StepKey (StepKey),
    INDEX IX_WorkflowEngineEvents_Timestamp (Timestamp),
    INDEX IX_WorkflowEngineEvents_CorrelationId (CorrelationId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowEngineTasks - Pending user action tasks
CREATE TABLE IF NOT EXISTS WorkflowEngineTasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NOT NULL,
    StepKey VARCHAR(100) NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    Instructions TEXT,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    AssignmentType VARCHAR(50) NOT NULL DEFAULT 'User',
    AssignedToUserId INT,
    AssignedToGroupId INT,
    AssignedToRole VARCHAR(100),
    Priority INT NOT NULL DEFAULT 0,
    DueAt DATETIME,
    EscalationRules TEXT,
    FormSchema TEXT,
    FormData TEXT,
    AvailableActions TEXT,
    ActionTaken VARCHAR(100),
    ClaimedByUserId INT,
    ClaimedAt DATETIME,
    CompletedByUserId INT,
    CompletedAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowEngineTasks_Instance FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowEngineTasks_AssignedUser FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_WorkflowEngineTasks_AssignedGroup FOREIGN KEY (AssignedToGroupId) REFERENCES UserGroups(Id) ON DELETE SET NULL,
    CONSTRAINT FK_WorkflowEngineTasks_ClaimedUser FOREIGN KEY (ClaimedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_WorkflowEngineTasks_CompletedUser FOREIGN KEY (CompletedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_WorkflowEngineTasks_Instance (WorkflowInstanceId),
    INDEX IX_WorkflowEngineTasks_Status (Status),
    INDEX IX_WorkflowEngineTasks_AssignedUser (AssignedToUserId),
    INDEX IX_WorkflowEngineTasks_AssignedGroup (AssignedToGroupId),
    INDEX IX_WorkflowEngineTasks_AssignedRole (AssignedToRole),
    INDEX IX_WorkflowEngineTasks_DueAt (DueAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowContextVariables - Key-value store for instance variables
CREATE TABLE IF NOT EXISTS WorkflowContextVariables (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NOT NULL,
    `Key` VARCHAR(255) NOT NULL,
    `Value` TEXT,
    ValueType VARCHAR(50) NOT NULL DEFAULT 'String',
    SetByStepKey VARCHAR(100),
    IsEncrypted TINYINT(1) NOT NULL DEFAULT 0,
    IsSystemVariable TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowContextVariables_Instance FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    INDEX IX_WorkflowContextVariables_Instance (WorkflowInstanceId),
    UNIQUE INDEX UX_WorkflowContextVariables_Instance_Key (WorkflowInstanceId, `Key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowSchedules - Scheduled workflow triggers
CREATE TABLE IF NOT EXISTS WorkflowSchedules (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowDefinitionId INT NOT NULL,
    CronExpression VARCHAR(100) NOT NULL,
    TimeZone VARCHAR(100) NOT NULL DEFAULT 'UTC',
    IsEnabled TINYINT(1) NOT NULL DEFAULT 1,
    InitialVariables TEXT,
    LastTriggeredAt DATETIME,
    NextTriggerAt DATETIME,
    ExecutionCount INT NOT NULL DEFAULT 0,
    ValidFrom DATETIME,
    ValidUntil DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowSchedules_Definition FOREIGN KEY (WorkflowDefinitionId) REFERENCES WorkflowDefinitions(Id) ON DELETE CASCADE,
    INDEX IX_WorkflowSchedules_Definition (WorkflowDefinitionId),
    INDEX IX_WorkflowSchedules_Enabled (IsEnabled),
    INDEX IX_WorkflowSchedules_NextTrigger (NextTriggerAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowJobs - Background job queue
CREATE TABLE IF NOT EXISTS WorkflowJobs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NOT NULL,
    JobType VARCHAR(100) NOT NULL,
    Payload TEXT,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    Priority INT NOT NULL DEFAULT 0,
    ScheduledAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    StartedAt DATETIME,
    CompletedAt DATETIME,
    ProcessingWorkerId VARCHAR(255),
    AttemptCount INT NOT NULL DEFAULT 0,
    MaxAttempts INT NOT NULL DEFAULT 3,
    LastError TEXT,
    Result TEXT,
    VisibilityTimeoutAt DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowJobs_Instance FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    INDEX IX_WorkflowJobs_Instance (WorkflowInstanceId),
    INDEX IX_WorkflowJobs_Status (Status),
    INDEX IX_WorkflowJobs_Priority_ScheduledAt (Priority, ScheduledAt),
    INDEX IX_WorkflowJobs_VisibilityTimeout (VisibilityTimeoutAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- WorkflowApiCredentials - Secure storage for API credentials
CREATE TABLE IF NOT EXISTS WorkflowApiCredentials (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    AuthenticationType VARCHAR(50) NOT NULL DEFAULT 'None',
    CredentialData TEXT,
    BaseUrl VARCHAR(500),
    DefaultHeaders TEXT,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    LastUsedAt DATETIME,
    CreatedByUserId INT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_WorkflowApiCredentials_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    INDEX IX_WorkflowApiCredentials_Name (Name),
    INDEX IX_WorkflowApiCredentials_Active (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Sample workflow definition for Service Request processing
INSERT INTO WorkflowDefinitions (Name, Description, Version, TriggerType, TriggerEntityType, Status, Priority)
VALUES (
    'Service Request Processing',
    'Standard workflow for processing customer service requests with approval and assignment',
    '1.0',
    'Event',
    'ServiceRequest',
    'Draft',
    0
);

-- Get the inserted workflow ID
SET @workflow_id = LAST_INSERT_ID();

-- Add sample steps
INSERT INTO WorkflowSteps (WorkflowDefinitionId, StepKey, Name, Description, StepType, OrderIndex, IsStartStep, Configuration, Transitions, PositionX, PositionY)
VALUES 
(@workflow_id, 'start', 'Start', 'Workflow start point', 'Start', 0, 1, NULL, '[{"nextStepKey":"triage","label":"Begin"}]', 100, 100),
(@workflow_id, 'triage', 'Triage Request', 'Review and categorize the service request', 'UserAction', 1, 0, 
    '{"title":"Review Service Request","description":"Review the incoming request and assign priority and category","assignmentType":"Role","assignedRole":"SupportAgent","availableActions":["Approve","Reject","Escalate"],"formFields":[{"name":"priority","label":"Priority","type":"select","options":["Low","Medium","High","Critical"]},{"name":"notes","label":"Triage Notes","type":"textarea"}]}',
    '[{"nextStepKey":"assign","condition":"action == \\"Approve\\""},{"nextStepKey":"escalate","condition":"action == \\"Escalate\\""},{"nextStepKey":"reject","condition":"action == \\"Reject\\""}]', 
    300, 100),
(@workflow_id, 'assign', 'Assign to Agent', 'Assign the request to an available support agent', 'UserAction', 2, 0,
    '{"title":"Assign Request","description":"Assign this request to a support agent for resolution","assignmentType":"Role","assignedRole":"SupportManager","availableActions":["Assigned"],"formFields":[{"name":"assignedAgent","label":"Assign To","type":"user"},{"name":"dueDate","label":"Due Date","type":"date"}]}',
    '[{"nextStepKey":"work","label":"Proceed to Work"}]',
    500, 100),
(@workflow_id, 'work', 'Work on Request', 'Agent works on resolving the service request', 'UserAction', 3, 0,
    '{"title":"Resolve Request","description":"Work on and resolve the customer request","assignmentType":"FromVariable","assignedToVariable":"assignedAgent","availableActions":["Resolved","NeedInfo","Escalate"],"formFields":[{"name":"resolution","label":"Resolution Notes","type":"textarea"},{"name":"category","label":"Resolution Category","type":"select","options":["Fixed","Workaround","NoAction","Duplicate"]}]}',
    '[{"nextStepKey":"notify","condition":"action == \\"Resolved\\""},{"nextStepKey":"triage","condition":"action == \\"NeedInfo\\""},{"nextStepKey":"escalate","condition":"action == \\"Escalate\\""}]',
    700, 100),
(@workflow_id, 'escalate', 'Escalate Request', 'Handle escalated requests', 'UserAction', 4, 0,
    '{"title":"Handle Escalation","description":"Review and handle the escalated request","assignmentType":"Role","assignedRole":"SupportManager","availableActions":["Resolved","Reassign"]}',
    '[{"nextStepKey":"notify","condition":"action == \\"Resolved\\""},{"nextStepKey":"assign","condition":"action == \\"Reassign\\""}]',
    500, 250),
(@workflow_id, 'reject', 'Reject Request', 'Handle rejected requests', 'Notification', 5, 0,
    '{"email":{"toVariable":"customer.email","subject":"Your request has been closed","body":"Your service request #{{entityReference}} has been reviewed and closed. Reason: {{rejectionReason}}"},"inApp":{"userIdVariable":"customer.userId","title":"Request Closed","message":"Your request has been closed"}}',
    '[{"nextStepKey":"end"}]',
    300, 250),
(@workflow_id, 'notify', 'Notify Customer', 'Send resolution notification to customer', 'Notification', 6, 0,
    '{"email":{"toVariable":"customer.email","subject":"Your request has been resolved","body":"Your service request #{{entityReference}} has been resolved. Resolution: {{resolution}}"},"inApp":{"userIdVariable":"customer.userId","title":"Request Resolved","message":"Your request has been resolved"}}',
    '[{"nextStepKey":"end"}]',
    900, 100),
(@workflow_id, 'end', 'End', 'Workflow end point', 'End', 7, 0, NULL, NULL, 1100, 175);

SELECT 'Workflow Engine tables created successfully' AS Result;

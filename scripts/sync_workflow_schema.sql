-- Workflow Module Database Schema Sync Script
-- This script recreates workflow tables to match entity definitions

-- Backup and drop existing tables that will be recreated
SET FOREIGN_KEY_CHECKS = 0;

-- Rename existing tables to backup
RENAME TABLE WorkflowNodes TO WorkflowNodes_backup;
RENAME TABLE WorkflowTransitions TO WorkflowTransitions_backup;
RENAME TABLE WorkflowNodeInstances TO WorkflowNodeInstances_backup;
RENAME TABLE WorkflowLogs TO WorkflowLogs_backup;
RENAME TABLE WorkflowTasks TO WorkflowTasks_backup;

-- Create WorkflowNodes table
CREATE TABLE WorkflowNodes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowVersionId INT NOT NULL,
    NodeKey VARCHAR(100) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(1000) NULL,
    NodeType INT NOT NULL DEFAULT 0,
    NodeSubType VARCHAR(100) NULL,
    PositionX DOUBLE NOT NULL DEFAULT 0,
    PositionY DOUBLE NOT NULL DEFAULT 0,
    Width DOUBLE NOT NULL DEFAULT 200,
    Height DOUBLE NOT NULL DEFAULT 80,
    IconName VARCHAR(50) NOT NULL DEFAULT 'Circle',
    Color VARCHAR(20) NOT NULL DEFAULT '#6750A4',
    IsStartNode TINYINT(1) NOT NULL DEFAULT 0,
    IsEndNode TINYINT(1) NOT NULL DEFAULT 0,
    Configuration LONGTEXT NULL,
    TimeoutMinutes INT NOT NULL DEFAULT 0,
    RetryCount INT NOT NULL DEFAULT 0,
    RetryDelaySeconds INT NOT NULL DEFAULT 60,
    UseExponentialBackoff TINYINT(1) NOT NULL DEFAULT 1,
    ExecutionOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_WorkflowNodes_WorkflowVersionId (WorkflowVersionId),
    INDEX IX_WorkflowNodes_NodeType (NodeType),
    UNIQUE INDEX UX_WorkflowNodes_VersionId_NodeKey (WorkflowVersionId, NodeKey),
    FOREIGN KEY (WorkflowVersionId) REFERENCES WorkflowVersions(Id) ON DELETE CASCADE
);

-- Create WorkflowTransitions table
CREATE TABLE WorkflowTransitions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowVersionId INT NOT NULL,
    TransitionKey VARCHAR(100) NOT NULL,
    Name VARCHAR(200) NULL,
    Description VARCHAR(1000) NULL,
    SourceNodeId INT NOT NULL,
    TargetNodeId INT NOT NULL,
    ConditionExpression LONGTEXT NULL,
    ConditionType INT NOT NULL DEFAULT 0,
    Priority INT NOT NULL DEFAULT 0,
    IsDefault TINYINT(1) NOT NULL DEFAULT 0,
    Label VARCHAR(100) NULL,
    LabelPositionX DOUBLE NULL,
    LabelPositionY DOUBLE NULL,
    PathData LONGTEXT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_WorkflowTransitions_WorkflowVersionId (WorkflowVersionId),
    INDEX IX_WorkflowTransitions_SourceNodeId (SourceNodeId),
    INDEX IX_WorkflowTransitions_TargetNodeId (TargetNodeId),
    FOREIGN KEY (WorkflowVersionId) REFERENCES WorkflowVersions(Id) ON DELETE CASCADE,
    FOREIGN KEY (SourceNodeId) REFERENCES WorkflowNodes(Id) ON DELETE CASCADE,
    FOREIGN KEY (TargetNodeId) REFERENCES WorkflowNodes(Id) ON DELETE CASCADE
);

-- Create WorkflowNodeInstances table
CREATE TABLE WorkflowNodeInstances (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NOT NULL,
    WorkflowNodeId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    EnteredAt DATETIME(6) NULL,
    StartedAt DATETIME(6) NULL,
    CompletedAt DATETIME(6) NULL,
    InputData LONGTEXT NULL,
    OutputData LONGTEXT NULL,
    ErrorMessage LONGTEXT NULL,
    ErrorStackTrace LONGTEXT NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    NextRetryAt DATETIME(6) NULL,
    TimeoutAt DATETIME(6) NULL,
    ExecutedById INT NULL,
    SelectedTransitionId INT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_WorkflowNodeInstances_WorkflowInstanceId (WorkflowInstanceId),
    INDEX IX_WorkflowNodeInstances_WorkflowNodeId (WorkflowNodeId),
    INDEX IX_WorkflowNodeInstances_Status (Status),
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkflowNodeId) REFERENCES WorkflowNodes(Id) ON DELETE CASCADE,
    FOREIGN KEY (ExecutedById) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (SelectedTransitionId) REFERENCES WorkflowTransitions(Id) ON DELETE SET NULL
);

-- Create WorkflowTasks table
CREATE TABLE WorkflowTasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NULL,
    WorkflowNodeId INT NULL,
    NodeInstanceId INT NULL,
    Name VARCHAR(200) NOT NULL,
    Description VARCHAR(2000) NULL,
    TaskType INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    Priority INT NOT NULL DEFAULT 100,
    QueueName VARCHAR(100) NOT NULL DEFAULT 'default',
    InputData LONGTEXT NULL,
    OutputData LONGTEXT NULL,
    FormSchema LONGTEXT NULL,
    FormData LONGTEXT NULL,
    AssignedToId INT NULL,
    AssignedToRole VARCHAR(100) NULL,
    DueAt DATETIME(6) NULL,
    ScheduledAt DATETIME(6) NULL,
    PickedAt DATETIME(6) NULL,
    StartedAt DATETIME(6) NULL,
    CompletedAt DATETIME(6) NULL,
    TimeoutAt DATETIME(6) NULL,
    LockedByWorkerId VARCHAR(100) NULL,
    LockExpiresAt DATETIME(6) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 3,
    NextRetryAt DATETIME(6) NULL,
    ErrorMessage LONGTEXT NULL,
    ErrorStackTrace LONGTEXT NULL,
    IsDeadLetter TINYINT(1) NOT NULL DEFAULT 0,
    DeadLetterAt DATETIME(6) NULL,
    DeadLetterReason VARCHAR(1000) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_WorkflowTasks_WorkflowInstanceId (WorkflowInstanceId),
    INDEX IX_WorkflowTasks_WorkflowNodeId (WorkflowNodeId),
    INDEX IX_WorkflowTasks_NodeInstanceId (NodeInstanceId),
    INDEX IX_WorkflowTasks_Status (Status),
    INDEX IX_WorkflowTasks_QueueName (QueueName),
    INDEX IX_WorkflowTasks_AssignedToId (AssignedToId),
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE SET NULL,
    FOREIGN KEY (WorkflowNodeId) REFERENCES WorkflowNodes(Id) ON DELETE SET NULL,
    FOREIGN KEY (NodeInstanceId) REFERENCES WorkflowNodeInstances(Id) ON DELETE SET NULL,
    FOREIGN KEY (AssignedToId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Create WorkflowLogs table
CREATE TABLE WorkflowLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkflowInstanceId INT NULL,
    WorkflowNodeId INT NULL,
    NodeInstanceId INT NULL,
    Level INT NOT NULL DEFAULT 0,
    Category VARCHAR(100) NOT NULL,
    Message VARCHAR(4000) NOT NULL,
    Details LONGTEXT NULL,
    Timestamp DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UserId INT NULL,
    CorrelationId VARCHAR(100) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_WorkflowLogs_WorkflowInstanceId (WorkflowInstanceId),
    INDEX IX_WorkflowLogs_NodeInstanceId (NodeInstanceId),
    INDEX IX_WorkflowLogs_Level (Level),
    INDEX IX_WorkflowLogs_Timestamp (Timestamp),
    FOREIGN KEY (WorkflowInstanceId) REFERENCES WorkflowInstances(Id) ON DELETE CASCADE,
    FOREIGN KEY (WorkflowNodeId) REFERENCES WorkflowNodes(Id) ON DELETE SET NULL,
    FOREIGN KEY (NodeInstanceId) REFERENCES WorkflowNodeInstances(Id) ON DELETE SET NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

SET FOREIGN_KEY_CHECKS = 1;

-- Verify tables were created
SHOW TABLES LIKE 'Workflow%';

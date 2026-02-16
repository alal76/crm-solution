-- Worker architecture tables

CREATE TABLE IF NOT EXISTS WorkerJobs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    JobType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    NextAttemptAt DATETIME NULL,
    CompletedAt DATETIME NULL,
    LastError TEXT NULL,
    CorrelationId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerJobs_Status_NextAttemptAt (Status, NextAttemptAt),
    INDEX IX_WorkerJobs_JobType (JobType)
);

CREATE TABLE IF NOT EXISTS OutboxEvents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EventType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    OccurredAt DATETIME NOT NULL,
    ProcessedAt DATETIME NULL,
    CorrelationId VARCHAR(100) NULL,
    IdempotencyKey VARCHAR(100) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    LastError TEXT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_OutboxEvents_Status (Status),
    INDEX IX_OutboxEvents_OccurredAt (OccurredAt)
);

CREATE TABLE IF NOT EXISTS WorkerExecutions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkerJobId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    StartedAt DATETIME NOT NULL,
    FinishedAt DATETIME NULL,
    ErrorMessage TEXT NULL,
    NodeId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerExecutions_WorkerJobId (WorkerJobId),
    CONSTRAINT FK_WorkerExecutions_WorkerJobs FOREIGN KEY (WorkerJobId) REFERENCES WorkerJobs(Id)
);

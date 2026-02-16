CREATE TABLE IF NOT EXISTS WorkerJobs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    JobType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Queued',
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    NextAttemptAt DATETIME NULL,
    CompletedAt DATETIME NULL,
    LastError TEXT NULL,
    CorrelationId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerJobs_Status_NextAttemptAt (Status, NextAttemptAt),
    INDEX IX_WorkerJobs_JobType (JobType)
);

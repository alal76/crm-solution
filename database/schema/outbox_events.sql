CREATE TABLE IF NOT EXISTS OutboxEvents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EventType VARCHAR(100) NOT NULL,
    Payload LONGTEXT NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    OccurredAt DATETIME NOT NULL,
    ProcessedAt DATETIME NULL,
    CorrelationId VARCHAR(100) NULL,
    IdempotencyKey VARCHAR(100) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    MaxRetries INT NOT NULL DEFAULT 5,
    LastError TEXT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_OutboxEvents_Status (Status),
    INDEX IX_OutboxEvents_OccurredAt (OccurredAt)
);

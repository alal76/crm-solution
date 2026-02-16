CREATE TABLE IF NOT EXISTS WorkerExecutions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    WorkerJobId INT NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Started',
    StartedAt DATETIME NOT NULL,
    FinishedAt DATETIME NULL,
    ErrorMessage TEXT NULL,
    NodeId VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion BLOB NULL,
    INDEX IX_WorkerExecutions_WorkerJobId (WorkerJobId),
    CONSTRAINT FK_WorkerExecutions_WorkerJobs FOREIGN KEY (WorkerJobId) REFERENCES WorkerJobs(Id)
);

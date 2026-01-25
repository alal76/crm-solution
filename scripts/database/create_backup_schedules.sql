-- Create BackupSchedules table
CREATE TABLE IF NOT EXISTS BackupSchedules (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    IsEnabled TINYINT(1) DEFAULT 1,
    BackupType VARCHAR(50) DEFAULT 'Full',
    CronExpression VARCHAR(100) DEFAULT '0 2 * * *',
    BackupPath VARCHAR(500) DEFAULT 'DatabaseBackups',
    RetentionDays INT DEFAULT 30,
    MaxBackupsToKeep INT DEFAULT 10,
    CompressBackups TINYINT(1) DEFAULT 1,
    LastBackupAt DATETIME NULL,
    NextBackupAt DATETIME NULL,
    LastError TEXT NULL,
    SuccessfulBackups INT DEFAULT 0,
    FailedBackups INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    IsDeleted TINYINT(1) DEFAULT 0
);

-- Insert default daily full backup schedule
INSERT INTO BackupSchedules (Name, IsEnabled, BackupType, CronExpression, BackupPath, RetentionDays, MaxBackupsToKeep, CompressBackups, NextBackupAt)
SELECT 'Daily Full Backup', 1, 'Full', '0 2 * * *', 'DatabaseBackups', 30, 10, 1, DATE_ADD(CURDATE(), INTERVAL 1 DAY) + INTERVAL 2 HOUR
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM BackupSchedules WHERE Name = 'Daily Full Backup');

-- Insert weekly incremental backup schedule
INSERT INTO BackupSchedules (Name, IsEnabled, BackupType, CronExpression, BackupPath, RetentionDays, MaxBackupsToKeep, CompressBackups, NextBackupAt)
SELECT 'Weekly Incremental', 0, 'Incremental', '0 3 * * 0', 'DatabaseBackups/incremental', 90, 12, 1, NULL
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM BackupSchedules WHERE Name = 'Weekly Incremental');

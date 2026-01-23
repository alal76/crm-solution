namespace CRM.Core.Entities;

/// <summary>
/// Database backup schedule configuration
/// </summary>
public class BackupSchedule : BaseEntity
{
    /// <summary>
    /// Name of the schedule
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the schedule is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Type of backup: Full, Incremental, Differential
    /// </summary>
    public string BackupType { get; set; } = "Full";

    /// <summary>
    /// Cron expression for the schedule (e.g., "0 2 * * *" for daily at 2 AM)
    /// </summary>
    public string CronExpression { get; set; } = "0 2 * * *";

    /// <summary>
    /// Path to store backups (absolute or relative)
    /// </summary>
    public string BackupPath { get; set; } = "DatabaseBackups";

    /// <summary>
    /// Retention period in days (0 = keep forever)
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Maximum number of backups to keep (0 = unlimited)
    /// </summary>
    public int MaxBackupsToKeep { get; set; } = 10;

    /// <summary>
    /// Whether to compress backups
    /// </summary>
    public bool CompressBackups { get; set; } = true;

    /// <summary>
    /// Last time a backup was created by this schedule
    /// </summary>
    public DateTime? LastBackupAt { get; set; }

    /// <summary>
    /// Next scheduled backup time
    /// </summary>
    public DateTime? NextBackupAt { get; set; }

    /// <summary>
    /// Last error message if any
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Number of successful backups created by this schedule
    /// </summary>
    public int SuccessfulBackups { get; set; } = 0;

    /// <summary>
    /// Number of failed backup attempts
    /// </summary>
    public int FailedBackups { get; set; } = 0;
}

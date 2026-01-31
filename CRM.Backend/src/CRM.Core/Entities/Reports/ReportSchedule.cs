namespace CRM.Core.Entities.Reports;

#region Schedule Enumerations

/// <summary>
/// Report schedule frequency.
/// </summary>
public enum ScheduleFrequency
{
    /// <summary>Run once</summary>
    Once = 0,
    
    /// <summary>Hourly</summary>
    Hourly = 1,
    
    /// <summary>Daily</summary>
    Daily = 2,
    
    /// <summary>Weekly</summary>
    Weekly = 3,
    
    /// <summary>Bi-weekly</summary>
    BiWeekly = 4,
    
    /// <summary>Monthly</summary>
    Monthly = 5,
    
    /// <summary>Quarterly</summary>
    Quarterly = 6,
    
    /// <summary>Yearly</summary>
    Yearly = 7,
    
    /// <summary>Custom cron expression</summary>
    Custom = 99
}

/// <summary>
/// Report output format.
/// </summary>
public enum ReportOutputFormat
{
    /// <summary>PDF document</summary>
    PDF = 0,
    
    /// <summary>Excel spreadsheet</summary>
    Excel = 1,
    
    /// <summary>CSV file</summary>
    CSV = 2,
    
    /// <summary>HTML</summary>
    HTML = 3,
    
    /// <summary>PNG image</summary>
    PNG = 4,
    
    /// <summary>JSON data</summary>
    JSON = 5
}

/// <summary>
/// Report schedule status.
/// </summary>
public enum ScheduleStatus
{
    /// <summary>Schedule is active</summary>
    Active = 0,
    
    /// <summary>Schedule is paused</summary>
    Paused = 1,
    
    /// <summary>Schedule has completed (one-time)</summary>
    Completed = 2,
    
    /// <summary>Schedule has errors</summary>
    Error = 3,
    
    /// <summary>Schedule is disabled</summary>
    Disabled = 4
}

/// <summary>
/// Report execution status.
/// </summary>
public enum ReportExecutionStatus
{
    /// <summary>Queued for execution</summary>
    Queued = 0,
    
    /// <summary>Currently running</summary>
    Running = 1,
    
    /// <summary>Completed successfully</summary>
    Completed = 2,
    
    /// <summary>Failed with error</summary>
    Failed = 3,
    
    /// <summary>Cancelled</summary>
    Cancelled = 4,
    
    /// <summary>Timed out</summary>
    TimedOut = 5
}

#endregion

/// <summary>
/// Schedule for automated report delivery.
/// </summary>
public class ReportSchedule : BaseEntity
{
    #region Identification
    
    /// <summary>Schedule name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Schedule description</summary>
    public string? Description { get; set; }
    
    /// <summary>Report ID</summary>
    public int ReportDefinitionId { get; set; }
    
    /// <summary>Navigation to report</summary>
    public ReportDefinition? ReportDefinition { get; set; }
    
    #endregion
    
    #region Schedule Configuration
    
    /// <summary>Schedule status</summary>
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;
    
    /// <summary>Frequency</summary>
    public ScheduleFrequency Frequency { get; set; }
    
    /// <summary>Custom cron expression</summary>
    public string? CronExpression { get; set; }
    
    /// <summary>Time of day to run (for daily+)</summary>
    public TimeSpan? TimeOfDay { get; set; }
    
    /// <summary>Day of week (for weekly)</summary>
    public DayOfWeek? DayOfWeek { get; set; }
    
    /// <summary>Day of month (for monthly)</summary>
    public int? DayOfMonth { get; set; }
    
    /// <summary>Timezone for scheduling</summary>
    public string Timezone { get; set; } = "UTC";
    
    #endregion
    
    #region Date Range
    
    /// <summary>Schedule start date</summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Schedule end date (null = no end)</summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>Next scheduled run</summary>
    public DateTime? NextRunAt { get; set; }
    
    /// <summary>Last run time</summary>
    public DateTime? LastRunAt { get; set; }
    
    #endregion
    
    #region Output Configuration
    
    /// <summary>Output format</summary>
    public ReportOutputFormat OutputFormat { get; set; } = ReportOutputFormat.PDF;
    
    /// <summary>Include data in email body</summary>
    public bool IncludeInEmailBody { get; set; } = false;
    
    /// <summary>Attach file to email</summary>
    public bool AttachFile { get; set; } = true;
    
    /// <summary>Custom file name pattern</summary>
    public string? FileNamePattern { get; set; }
    
    /// <summary>Compress output file</summary>
    public bool CompressOutput { get; set; } = false;
    
    #endregion
    
    #region Delivery Configuration
    
    /// <summary>Send via email</summary>
    public bool SendEmail { get; set; } = true;
    
    /// <summary>Email recipients (JSON array)</summary>
    public string? EmailRecipientsJson { get; set; }
    
    /// <summary>CC recipients (JSON array)</summary>
    public string? EmailCcJson { get; set; }
    
    /// <summary>Email subject template</summary>
    public string? EmailSubject { get; set; }
    
    /// <summary>Email body template</summary>
    public string? EmailBody { get; set; }
    
    /// <summary>Save to storage</summary>
    public bool SaveToStorage { get; set; } = false;
    
    /// <summary>Storage path</summary>
    public string? StoragePath { get; set; }
    
    /// <summary>Post to webhook</summary>
    public bool PostToWebhook { get; set; } = false;
    
    /// <summary>Webhook URL</summary>
    public string? WebhookUrl { get; set; }
    
    #endregion
    
    #region Conditions
    
    /// <summary>Only send if data changed</summary>
    public bool OnlyIfDataChanged { get; set; } = false;
    
    /// <summary>Only send if has data</summary>
    public bool OnlyIfHasData { get; set; } = false;
    
    /// <summary>Minimum rows threshold</summary>
    public int? MinRowsThreshold { get; set; }
    
    /// <summary>Data hash from last run (for change detection)</summary>
    public string? LastDataHash { get; set; }
    
    #endregion
    
    #region Ownership
    
    /// <summary>Creator user ID</summary>
    public int CreatedByUserId { get; set; }
    
    /// <summary>Navigation to creator</summary>
    public User? CreatedByUser { get; set; }
    
    #endregion
    
    #region Statistics
    
    /// <summary>Total run count</summary>
    public int RunCount { get; set; } = 0;
    
    /// <summary>Success count</summary>
    public int SuccessCount { get; set; } = 0;
    
    /// <summary>Failure count</summary>
    public int FailureCount { get; set; } = 0;
    
    /// <summary>Last error message</summary>
    public string? LastError { get; set; }
    
    /// <summary>Average execution time (seconds)</summary>
    public decimal? AvgExecutionTimeSeconds { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Execution history</summary>
    public ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
    
    #endregion
}

/// <summary>
/// Record of a report execution.
/// </summary>
public class ReportExecution : BaseEntity
{
    #region References
    
    /// <summary>Schedule ID (null if ad-hoc)</summary>
    public int? ReportScheduleId { get; set; }
    
    /// <summary>Navigation to schedule</summary>
    public ReportSchedule? ReportSchedule { get; set; }
    
    /// <summary>Report definition ID</summary>
    public int ReportDefinitionId { get; set; }
    
    /// <summary>Navigation to report</summary>
    public ReportDefinition? ReportDefinition { get; set; }
    
    #endregion
    
    #region Execution Details
    
    /// <summary>Execution status</summary>
    public ReportExecutionStatus Status { get; set; }
    
    /// <summary>Started at</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>Completed at</summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>Execution time in seconds</summary>
    public decimal? ExecutionTimeSeconds { get; set; }
    
    /// <summary>Parameters used (JSON)</summary>
    public string? ParametersJson { get; set; }
    
    #endregion
    
    #region Results
    
    /// <summary>Row count returned</summary>
    public int? RowCount { get; set; }
    
    /// <summary>Output file path</summary>
    public string? OutputFilePath { get; set; }
    
    /// <summary>Output file size in bytes</summary>
    public long? OutputFileSize { get; set; }
    
    /// <summary>Output format used</summary>
    public ReportOutputFormat? OutputFormat { get; set; }
    
    /// <summary>Data hash for change detection</summary>
    public string? DataHash { get; set; }
    
    #endregion
    
    #region Delivery
    
    /// <summary>Email sent successfully</summary>
    public bool? EmailSent { get; set; }
    
    /// <summary>Recipients who received email</summary>
    public string? DeliveredToJson { get; set; }
    
    /// <summary>Webhook delivered</summary>
    public bool? WebhookDelivered { get; set; }
    
    #endregion
    
    #region Error Handling
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Error stack trace</summary>
    public string? ErrorStackTrace { get; set; }
    
    /// <summary>Retry count</summary>
    public int RetryCount { get; set; } = 0;
    
    #endregion
    
    #region User Context
    
    /// <summary>User who triggered execution (null if scheduled)</summary>
    public int? TriggeredByUserId { get; set; }
    
    /// <summary>Navigation to user</summary>
    public User? TriggeredByUser { get; set; }
    
    #endregion
}

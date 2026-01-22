namespace CRM.Core.Entities;

/// <summary>
/// CRM Task status enumeration
/// </summary>
public enum CrmTaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Deferred = 3,
    Waiting = 4,
    Cancelled = 5
}

/// <summary>
/// CRM Task priority enumeration
/// </summary>
public enum CrmTaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// CRM Task type enumeration
/// </summary>
public enum CrmTaskType
{
    Call = 0,
    Email = 1,
    Meeting = 2,
    FollowUp = 3,
    Demo = 4,
    Proposal = 5,
    Contract = 6,
    Research = 7,
    Other = 8
}

/// <summary>
/// CRM Task entity for managing to-do items and follow-ups
/// </summary>
public class CrmTask : BaseEntity
{
    // Basic Information
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CrmTaskType TaskType { get; set; } = CrmTaskType.Other;
    public CrmTaskStatus Status { get; set; } = CrmTaskStatus.NotStarted;
    public CrmTaskPriority Priority { get; set; } = CrmTaskPriority.Normal;
    
    // Dates
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public bool HasReminder { get; set; } = false;
    
    // Progress
    public int PercentComplete { get; set; } = 0;
    public int? EstimatedMinutes { get; set; }
    public int? ActualMinutes { get; set; }
    
    // Recurrence
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; } // JSON: daily, weekly, monthly, etc.
    public DateTime? RecurrenceEndDate { get; set; }
    public int? ParentTaskId { get; set; }
    
    // Relationships
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? CampaignId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? AssignedToGroupId { get; set; } // Group assignment for workflow queue
    public int? CreatedByUserId { get; set; }
    
    // Classification
    public string? Tags { get; set; }
    public string? Category { get; set; }
    
    // Attachments
    public string? Attachments { get; set; } // JSON array of file URLs
    
    // Custom Fields
    public string? CustomFields { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
    public MarketingCampaign? Campaign { get; set; }
    public User? AssignedToUser { get; set; }
    public UserGroup? AssignedToGroup { get; set; }
    public User? CreatedByUser { get; set; }
    public CrmTask? ParentTask { get; set; }
    public ICollection<CrmTask>? SubTasks { get; set; }
}

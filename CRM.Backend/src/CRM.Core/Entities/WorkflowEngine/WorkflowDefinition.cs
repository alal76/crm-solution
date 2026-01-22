using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Workflow definition entity - stores workflow templates with versioning support
/// </summary>
public class WorkflowDefinition : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Current version of this workflow (semantic versioning)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// Version number for optimistic locking
    /// </summary>
    public int VersionNumber { get; set; } = 1;
    
    /// <summary>
    /// How workflow is triggered: Manual, Scheduled, Event
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TriggerType { get; set; } = "Manual";
    
    /// <summary>
    /// For event triggers - which entity type triggers this workflow
    /// </summary>
    [MaxLength(100)]
    public string? TriggerEntityType { get; set; }
    
    /// <summary>
    /// For event triggers - which events trigger (Created, Updated, StatusChanged, etc.)
    /// </summary>
    [MaxLength(500)]
    public string? TriggerEvents { get; set; }
    
    /// <summary>
    /// For scheduled triggers - cron expression
    /// </summary>
    [MaxLength(100)]
    public string? ScheduleCron { get; set; }
    
    /// <summary>
    /// Workflow status: Draft, Published, Archived
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";
    
    /// <summary>
    /// Priority for execution ordering (higher = more priority)
    /// </summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>
    /// JSON configuration for error handling
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ErrorHandlingConfig { get; set; }
    
    /// <summary>
    /// JSON configuration for notification channels
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? NotificationConfig { get; set; }
    
    /// <summary>
    /// ID of user who created this workflow
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    /// <summary>
    /// ID of user who last modified this workflow
    /// </summary>
    public int? LastModifiedByUserId { get; set; }
    
    /// <summary>
    /// When workflow was published (if published)
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? LastModifiedByUser { get; set; }
    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
    public virtual ICollection<WorkflowDefinitionVersion> Versions { get; set; } = new List<WorkflowDefinitionVersion>();
}

/// <summary>
/// Trigger types for workflows
/// </summary>
public static class WorkflowTriggerTypes
{
    public const string Manual = "Manual";
    public const string Scheduled = "Scheduled";
    public const string Event = "Event";
}

/// <summary>
/// Workflow status values
/// </summary>
public static class WorkflowDefinitionStatus
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Archived = "Archived";
}

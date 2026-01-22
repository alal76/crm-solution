using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Scheduled workflow triggers
/// </summary>
public class WorkflowSchedule : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Cron expression for schedule
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CronExpression { get; set; } = string.Empty;
    
    /// <summary>
    /// Timezone for schedule evaluation
    /// </summary>
    [MaxLength(100)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// Whether schedule is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Last time workflow was triggered
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }
    
    /// <summary>
    /// Next scheduled trigger time
    /// </summary>
    public DateTime? NextTriggerAt { get; set; }
    
    /// <summary>
    /// Count of successful executions
    /// </summary>
    public int ExecutionCount { get; set; } = 0;
    
    /// <summary>
    /// Context data JSON to pass to workflow
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? ContextData { get; set; }
    
    /// <summary>
    /// Start date for schedule validity
    /// </summary>
    public DateTime? ValidFrom { get; set; }
    
    /// <summary>
    /// End date for schedule validity
    /// </summary>
    public DateTime? ValidUntil { get; set; }
    
    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
}

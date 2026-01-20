namespace CRM.Core.Entities;

/// <summary>
/// Workflow execution history - tracks workflow executions for audit trail
/// </summary>
public class WorkflowExecution : BaseEntity
{
    public int WorkflowId { get; set; }
    public int? WorkflowRuleId { get; set; } // The rule that matched
    public string EntityType { get; set; } = string.Empty; // "Customer", "Opportunity", etc.
    public int EntityId { get; set; } // The ID of the entity being processed
    public int SourceUserGroupId { get; set; } // The group the entity was transferred from
    public int TargetUserGroupId { get; set; } // The group the entity was transferred to
    public string Status { get; set; } = "Success"; // "Success", "Failed", "Pending"
    public string? ErrorMessage { get; set; }
    public string? EntitySnapshotJson { get; set; } // Store entity data as JSON at time of execution

    // Navigation properties
    public virtual Workflow Workflow { get; set; } = null!;
    public virtual WorkflowRule? WorkflowRule { get; set; }
    public virtual UserGroup SourceUserGroup { get; set; } = null!;
    public virtual UserGroup TargetUserGroup { get; set; } = null!;
}

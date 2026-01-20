namespace CRM.Core.Entities;

/// <summary>
/// Workflow rule entity - defines conditions and actions for a workflow
/// </summary>
public class WorkflowRule : BaseEntity
{
    public int WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetUserGroupId { get; set; } // The group to transfer the entity to
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 100; // Higher priority rules execute first

    // Logical operator for conditions: AND or OR
    public string ConditionLogic { get; set; } = "AND"; // AND | OR

    // Navigation properties
    public virtual Workflow Workflow { get; set; } = null!;
    public virtual UserGroup TargetUserGroup { get; set; } = null!;
    public virtual ICollection<WorkflowRuleCondition> Conditions { get; set; } = new List<WorkflowRuleCondition>();
}

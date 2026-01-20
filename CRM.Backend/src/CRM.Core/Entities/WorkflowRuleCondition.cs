namespace CRM.Core.Entities;

/// <summary>
/// Workflow rule condition - defines conditions for rule evaluation
/// </summary>
public class WorkflowRuleCondition : BaseEntity
{
    public int WorkflowRuleId { get; set; }
    public string FieldName { get; set; } = string.Empty; // e.g., "LifecycleStage", "AnnualRevenue", "Company"
    public string Operator { get; set; } = string.Empty; // "Equals", "NotEquals", "GreaterThan", "LessThan", "Contains", "In", "Between"
    public string Value { get; set; } = string.Empty; // The value to compare against
    public string? ValueTwo { get; set; } // For "Between" operator
    public int Priority { get; set; } = 100; // Execution order of conditions

    // Navigation properties
    public virtual WorkflowRule WorkflowRule { get; set; } = null!;
}

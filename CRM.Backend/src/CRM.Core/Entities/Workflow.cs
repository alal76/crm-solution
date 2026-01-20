namespace CRM.Core.Entities;

/// <summary>
/// Workflow entity for managing entity transfer rules
/// </summary>
public class Workflow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // "Customer", "Opportunity", "Product", etc.
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 100; // Higher priority workflows execute first

    // Navigation properties
    public virtual ICollection<WorkflowRule> Rules { get; set; } = new List<WorkflowRule>();
    public virtual ICollection<WorkflowExecution> Executions { get; set; } = new List<WorkflowExecution>();
}

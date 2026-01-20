namespace CRM.Core.Dtos;

/// <summary>
/// DTO for creating/updating a workflow
/// </summary>
public class WorkflowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 100;
    public List<WorkflowRuleDto> Rules { get; set; } = new();
}

/// <summary>
/// DTO for creating/updating a workflow rule
/// </summary>
public class WorkflowRuleDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetUserGroupId { get; set; }
    public string TargetUserGroupName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 100;
    public string ConditionLogic { get; set; } = "AND";
    public List<WorkflowRuleConditionDto> Conditions { get; set; } = new();
}

/// <summary>
/// DTO for workflow rule condition
/// </summary>
public class WorkflowRuleConditionDto
{
    public int Id { get; set; }
    public int WorkflowRuleId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? ValueTwo { get; set; }
    public int Priority { get; set; } = 100;
}

/// <summary>
/// DTO for workflow execution history
/// </summary>
public class WorkflowExecutionDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int? WorkflowRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int SourceUserGroupId { get; set; }
    public string SourceUserGroupName { get; set; } = string.Empty;
    public int TargetUserGroupId { get; set; }
    public string TargetUserGroupName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? EntitySnapshotJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for executing a workflow manually
/// </summary>
public class ExecuteWorkflowRequestDto
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public Dictionary<string, object> EntityData { get; set; } = new();
}

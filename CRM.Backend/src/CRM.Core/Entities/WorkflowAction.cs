using System;

namespace CRM.Core.Entities;

/// <summary>
/// Reusable workflow action definition (send email, update field, create task, etc.).
/// </summary>
public class WorkflowAction : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? ConfigurationJson { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
    public string? Icon { get; set; }
}

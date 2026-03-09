using System;

namespace CRM.Core.Entities;

/// <summary>
/// CRM automation rule for triggering actions based on entity events and conditions.
/// </summary>
public class AutomationRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string TriggerEvent { get; set; } = string.Empty;
    public string? ConditionsJson { get; set; }
    public string? ActionsJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
    public int? CreatedById { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int TriggerCount { get; set; }
}

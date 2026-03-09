// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

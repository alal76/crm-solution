// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities;

/// <summary>
/// Template for generating notifications across channels (email, push, in-app).
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Channel { get; set; } = "InApp";
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? EventTrigger { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Variables { get; set; }
}

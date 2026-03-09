// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities;

/// <summary>
/// User notification entity for the in-app notification inbox.
/// </summary>
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public virtual User? User { get; set; }
}

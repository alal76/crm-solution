// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Stores per-user column visibility / ordering preferences for list views.
/// </summary>
public class UserListViewPreference : BaseEntity
{
    public int UserId { get; set; }

    /// <summary>Entity type whose list view this preference applies to.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>JSON array of column definitions: [{field, visible, order, width}].</summary>
    public string ColumnsJson { get; set; } = "[]";
}

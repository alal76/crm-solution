// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities;

/// <summary>
/// Saved list/grid view per entity type for user customization.
/// </summary>
public class SavedView : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsShared { get; set; }
    public string? ColumnsJson { get; set; }
    public string? FiltersJson { get; set; }
    public string? SortJson { get; set; }
    public int? PageSize { get; set; }
    public virtual User? User { get; set; }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persisted page layout configuration for entity list/detail views.
/// </summary>
public class PageLayout : BaseEntity
{
    /// <summary>Target entity type: Account, Contact, Lead, Opportunity, etc.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Layout name (e.g. "Default", "Compact", "Sales Team").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>JSON-serialized layout definition (columns, sections, tab order, etc.).</summary>
    public string LayoutJson { get; set; } = "{}";

    /// <summary>When true this layout is applied to all users unless overridden.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Optional: restrict layout to a specific user group ID.</summary>
    public int? UserGroupId { get; set; }
}

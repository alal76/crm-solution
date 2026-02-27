// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

public class LookupCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Entity type this category is bound to (e.g. "Lead", "ServiceRequest")</summary>
    public string? EntityType { get; set; }

    /// <summary>Property name on the entity (e.g. "Status", "Priority")</summary>
    public string? PropertyName { get; set; }

    /// <summary>System-managed categories cannot be deleted</summary>
    public bool IsSystemManaged { get; set; } = false;

    /// <summary>Whether end-users may add custom values to this category</summary>
    public bool AllowCustomValues { get; set; } = true;

    /// <summary>Optional JSON schema for value validation</summary>
    public string? ValidationSchema { get; set; }

    public ICollection<LookupItem> Items { get; set; } = new List<LookupItem>();
}

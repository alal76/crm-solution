// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Represents a configurable enumeration category (ENUM-BE-001).
/// Maps to the EnumCategories table, providing a dedicated, service-managed
/// alternative to LookupCategories for typed enum management.
/// </summary>
public class EnumCategory : BaseEntity
{
    /// <summary>Machine-readable unique name (e.g. "LeadStatus")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable display name</summary>
    public string? DisplayName { get; set; }

    /// <summary>Description of what this category represents</summary>
    public string? Description { get; set; }

    /// <summary>Entity type this category is bound to (e.g. "Lead", "ServiceRequest")</summary>
    public string? EntityType { get; set; }

    /// <summary>Property name on the entity (e.g. "Status", "Priority")</summary>
    public string? PropertyName { get; set; }

    /// <summary>System-managed categories cannot be deleted by end-users</summary>
    public bool IsSystemManaged { get; set; } = false;

    /// <summary>Whether end-users may add custom values to this category</summary>
    public bool AllowCustomValues { get; set; } = true;

    /// <summary>Optional JSON schema for value validation</summary>
    public string? ValidationSchema { get; set; }

    /// <summary>Values belonging to this category</summary>
    public ICollection<EnumValue> Values { get; set; } = new List<EnumValue>();

    /// <summary>Transition rules defined for this category</summary>
    public ICollection<EnumTransition> Transitions { get; set; } = new List<EnumTransition>();
}

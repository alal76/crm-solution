// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Persisted navigation configuration item for RBAC normalization.
/// Used by RbacNormalizationService to sync group permission flags with navigation filtering rules.
/// TODO-SYS012-002
/// </summary>
[Table("NavigationConfigs")]
public class NavigationConfigEntity : BaseEntity
{
    /// <summary>Navigation item key (e.g., "dashboard", "accounts", "contacts")</summary>
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>Display label</summary>
    [MaxLength(200)]
    public string? Label { get; set; }

    /// <summary>Icon name</summary>
    [MaxLength(100)]
    public string? Icon { get; set; }

    /// <summary>Route/path</summary>
    [MaxLength(500)]
    public string? Route { get; set; }

    /// <summary>Display order</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Whether this navigation item is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Comma-separated list of required roles (e.g., "Admin,Manager")</summary>
    [MaxLength(500)]
    public string? RequiredRoles { get; set; }

    /// <summary>Parent navigation config ID (for nested menus)</summary>
    public int? ParentId { get; set; }

    /// <summary>Navigation to parent</summary>
    [ForeignKey("ParentId")]
    public virtual NavigationConfigEntity? Parent { get; set; }
}

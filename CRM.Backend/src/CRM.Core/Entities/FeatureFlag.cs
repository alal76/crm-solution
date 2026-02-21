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
/// Feature flag entity for SYS-004: Feature Flags & Toggles
/// Represents a toggleable feature that can be enabled/disabled.
/// </summary>
[Table("FeatureFlags")]
public class FeatureFlag : BaseEntity
{
    /// <summary>
    /// Unique key identifier for the feature flag (e.g., "EnableUserPortal")
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// displayable name of the feature
    /// </summary>
    [Required]
    [StringLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this feature flag controls
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the feature is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Type of feature flag (Toggle, Percentage, Variant)
    /// 0 = Toggle, 1 = Percentage, 2 = Variant
    /// </summary>
    public int FeatureType { get; set; } = 0;

    /// <summary>
    /// For string/config based features
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// JSON metadata for the feature
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Whether this is a system flag (can't be deleted)
    /// </summary>
    public bool IsSystemFlag { get; set; } = false;

    /// <summary>
    /// Navigation property for variants
    /// </summary>
    public virtual ICollection<FeatureFlagVariant> Variants { get; set; } = new List<FeatureFlagVariant>();

    /// <summary>
    /// Navigation property for audit logs
    /// </summary>
    public virtual ICollection<FeatureFlagAuditLog> AuditLogs { get; set; } = new List<FeatureFlagAuditLog>();
}

/// <summary>
/// Feature flag variant entity for SYS-004
/// Used for A/B testing and variant feature flags
/// </summary>
[Table("FeatureFlagVariants")]
public class FeatureFlagVariant : BaseEntity
{
    /// <summary>
    /// Foreign key to FeatureFlag
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Navigation property to parent FeatureFlag
    /// </summary>
    public virtual FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Unique key for this variant (e.g., "ControlGroup", "VariantA")
    /// </summary>
    [Required]
    [StringLength(256)]
    public string VariantKey { get; set; } = string.Empty;

    /// <summary>
    /// The value/content for this variant
    /// </summary>
    [Required]
    public string VariantValue { get; set; } = string.Empty;

    /// <summary>
    /// Description of this variant
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Weight/percentage for this variant (0-100)
    /// </summary>
    public int Weight { get; set; } = 0;
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Audit log entity for feature flag changes
/// Tracks all modifications to feature flags including who changed it and when
/// </summary>
[Table("FeatureFlagAuditLogs")]
public class FeatureFlagAuditLog : BaseEntity
{
    /// <summary>
    /// Name of the feature flag
    /// </summary>
    public string FlagName { get; set; } = string.Empty;

    /// <summary>
    /// Previous value of the flag
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value of the flag
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Type of change (Enable, Disable, SetRollout, SetProvider, etc.)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// User ID who made the change
    /// </summary>
    public int ChangedById { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User? ChangedBy { get; set; }

    /// <summary>
    /// Timestamp of the change
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional reason for the change
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Affected targeting (user IDs, roles, organizations)
    /// </summary>
    public string? TargetingInfo { get; set; }
}

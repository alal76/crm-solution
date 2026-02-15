// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

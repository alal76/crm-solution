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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents an audit log entry tracking user actions and entity changes.
/// Used for compliance, troubleshooting, and security monitoring.
/// </summary>
[Table("AuditLogs")]
public class AuditLog : BaseEntity
{
    /// <summary>
    /// The ID of the user who performed the action.
    /// Nullable for system-generated events.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// The action performed (e.g., Login, Create, Update, Delete, Close).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The type of entity affected (e.g., User, Quote, ServiceRequest, Campaign).
    /// Also referred to as "Entity" in seed data.
    /// </summary>
    [MaxLength(100)]
    public string? EntityType { get; set; }

    /// <summary>
    /// The ID of the entity affected by this action.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Optional display name for the affected entity.
    /// </summary>
    [MaxLength(500)]
    public string? EntityName { get; set; }

    /// <summary>
    /// JSON-serialized dictionary of old property values (before change).
    /// Used for update and delete operations.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON-serialized dictionary of new property values (after change).
    /// Used for create and update operations.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? NewValues { get; set; }

    /// <summary>
    /// Comma-separated list of properties that changed.
    /// </summary>
    [MaxLength(2000)]
    public string? ChangedProperties { get; set; }

    /// <summary>
    /// IP address of the user who performed the action.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the client request.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp when the action occurred.
    /// Uses CreatedAt from BaseEntity but kept for explicit clarity.
    /// </summary>
    [NotMapped]
    public DateTime Timestamp => CreatedAt;

    /// <summary>
    /// Additional details or context as JSON string.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? Details { get; set; }

    // Navigation properties

    /// <summary>
    /// Reference to the user who performed the action.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}

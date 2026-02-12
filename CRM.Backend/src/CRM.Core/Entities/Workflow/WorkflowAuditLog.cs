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

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Represents an audit log entry for workflow-related actions (design-time and runtime).
/// </summary>
public class WorkflowAuditLog : BaseEntity
{
    /// <summary>
    /// Foreign key to the workflow instance this audit entry relates to.
    /// </summary>
    public int WorkflowInstanceId { get; set; }

    /// <summary>
    /// Navigation property to the workflow instance.
    /// </summary>
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// Action that was performed (e.g., "Started", "Approved", "Cancelled", "Escalated").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the actor who performed the action.
    /// </summary>
    [MaxLength(200)]
    public string? ActorId { get; set; }

    /// <summary>
    /// Display name of the actor who performed the action.
    /// </summary>
    [MaxLength(200)]
    public string? ActorName { get; set; }

    /// <summary>
    /// JSON blob with additional details about the action.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the actor at the time of the action.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent header of the actor's client.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Exact timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

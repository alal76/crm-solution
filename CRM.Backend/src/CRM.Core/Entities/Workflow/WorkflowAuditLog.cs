// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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

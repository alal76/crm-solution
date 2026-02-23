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
/// Represents a webhook event that occurred in the system.
/// Events are created when CRM entities are created, updated, or deleted.
/// </summary>
[Table("WebhookEvents")]
public class WebhookEvent : BaseEntity
{
    /// <summary>
    /// Event type identifier, e.g. "account.created", "contact.updated", "opportunity.deleted"
    /// </summary>
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The type of entity that triggered the event, e.g. "Account", "Contact"
    /// </summary>
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    /// <summary>
    /// JSON payload containing the event data.
    /// </summary>
    public string Payload { get; set; } = "{}";

    public DateTime OccurredAt { get; set; }

    public int? TriggeredByUserId { get; set; }

    /// <summary>
    /// Correlation ID for tracking chains of related events.
    /// </summary>
    [StringLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Parent event ID for cycle detection in cascading events.
    /// </summary>
    [StringLength(100)]
    public string? ParentEventId { get; set; }
}

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
/// Tracks individual delivery attempts for a general-purpose webhook event.
/// Each delivery represents one attempt to send an event payload to a WebhookEndpoint.
/// Named WebhookDeliveryGeneral to avoid conflict with ITSM.WebhookDelivery.
/// </summary>
[Table("WebhookDeliveriesGeneral")]
public class WebhookDeliveryGeneral : BaseEntity
{
    public int WebhookEndpointId { get; set; }

    public int WebhookEventId { get; set; }

    public int AttemptNumber { get; set; }

    /// <summary>
    /// Status of the delivery: Pending, Success, Failed, Retrying
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public int? HttpStatusCode { get; set; }

    [StringLength(2000)]
    public string? ResponseBody { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public long? DurationMs { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Parent event ID for event chain tracking (TODO-INT001-48).
    /// Null for root events (events not triggered by another event).
    /// </summary>
    public int? ParentEventId { get; set; }

    /// <summary>
    /// Depth in the event chain (TODO-INT001-48).
    /// 0 for root events, increments for each triggered event in the chain.
    /// </summary>
    public int ChainDepth { get; set; } = 0;

    /// <summary>
    /// Correlation ID for tracking across distributed systems.
    /// </summary>
    [StringLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Entity type that triggered this delivery (e.g., "Account", "Lead").
    /// </summary>
    [StringLength(100)]
    public string? EntityType { get; set; }

    /// <summary>
    /// Entity ID that triggered this delivery.
    /// </summary>
    public int? EntityId { get; set; }

    // Navigation properties
    [ForeignKey("WebhookEndpointId")]
    public virtual WebhookEndpoint? WebhookEndpoint { get; set; }

    [ForeignKey("WebhookEventId")]
    public virtual WebhookEvent? WebhookEvent { get; set; }

    /// <summary>
    /// Navigation to parent event for chain tracking.
    /// </summary>
    [ForeignKey("ParentEventId")]
    public virtual WebhookEvent? ParentEvent { get; set; }
}

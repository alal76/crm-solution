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

    // Navigation properties
    [ForeignKey("WebhookEndpointId")]
    public virtual WebhookEndpoint? WebhookEndpoint { get; set; }

    [ForeignKey("WebhookEventId")]
    public virtual WebhookEvent? WebhookEvent { get; set; }
}

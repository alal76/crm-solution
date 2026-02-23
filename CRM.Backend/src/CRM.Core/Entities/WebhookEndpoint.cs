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
/// General-purpose webhook endpoint registration (not ITSM-scoped).
/// Represents a URL that receives webhook notifications for subscribed event types.
/// </summary>
[Table("WebhookEndpoints")]
public class WebhookEndpoint : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [StringLength(255)]
    public string Secret { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of subscribed event types, e.g. ["account.created","contact.updated"]
    /// </summary>
    public string EventTypes { get; set; } = "[]";

    /// <summary>
    /// JSON object of custom headers to include in webhook requests.
    /// </summary>
    public string? Headers { get; set; }

    public int MaxRetries { get; set; } = 5;

    public int TimeoutSeconds { get; set; } = 30;

    [StringLength(500)]
    public string? Description { get; set; }

    public int ConsecutiveFailures { get; set; }

    public DateTime? LastSuccessAt { get; set; }

    public DateTime? LastFailureAt { get; set; }

    public DateTime? DisabledAt { get; set; }

    [StringLength(500)]
    public string? DisabledReason { get; set; }

    public int CreatedByUserId { get; set; }
}

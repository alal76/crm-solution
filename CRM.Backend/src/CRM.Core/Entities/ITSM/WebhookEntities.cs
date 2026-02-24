// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Entities.ITSM;

/// <summary>
/// Webhook subscription for receiving ITSM event notifications.
/// </summary>
public class WebhookSubscription : BaseEntity
{
    public int WebhookSubscriptionId { get; set; }

    /// <summary>
    /// Friendly name for the webhook subscription.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the webhook's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Target URL to send webhook payloads to.
    /// </summary>
    public string TargetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for HMAC signature verification.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Whether this subscription is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON array of event types this subscription listens to.
    /// </summary>
    public string EventTypes { get; set; } = "[]";

    /// <summary>
    /// JSON object of custom headers to include in webhook requests.
    /// </summary>
    public string Headers { get; set; } = "{}";

    /// <summary>
    /// Number of retry attempts for failed deliveries.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for webhook requests.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Last time a webhook was triggered for this subscription.
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Count of successful deliveries.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Count of failed deliveries.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// User who created this subscription.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>
    /// Delivery history for this subscription.
    /// </summary>
    public virtual ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

/// <summary>
/// Record of a webhook delivery attempt.
/// </summary>
public class WebhookDelivery : BaseEntity
{
    public int WebhookDeliveryId { get; set; }

    /// <summary>
    /// Associated subscription.
    /// </summary>
    public int WebhookSubscriptionId { get; set; }
    public virtual WebhookSubscription? Subscription { get; set; }

    /// <summary>
    /// Event type that triggered this delivery.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Target URL the webhook was sent to.
    /// </summary>
    public string TargetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request body that was sent.
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// HTTP response status code.
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// Response body received.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Whether the delivery was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Attempt number (1 = first attempt, 2+ = retries).
    /// </summary>
    public int AttemptNumber { get; set; } = 1;

    /// <summary>
    /// When the delivery was completed (success or final failure).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration in milliseconds.
    /// </summary>
    public double? DurationMs { get; set; }

    /// <summary>
    /// Correlation ID for tracking webhook events across systems.
    /// Used to correlate original event with webhook delivery.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Parent event ID for event chain tracking.
    /// Used to detect and prevent infinite event loops.
    /// </summary>
    public string? ParentEventId { get; set; }

    /// <summary>
    /// When the next retry is scheduled.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Payload size in bytes before chunking.
    /// </summary>
    public int? PayloadSizeBytes { get; set; }

    /// <summary>
    /// Chunk number if payload was split (1-based).
    /// </summary>
    public int? ChunkNumber { get; set; }

    /// <summary>
    /// Total number of chunks if payload was split.
    /// </summary>
    public int? TotalChunks { get; set; }

    /// <summary>
    /// Continuation token for retrieving subsequent chunks.
    /// </summary>
    public string? ContinuationToken { get; set; }
}
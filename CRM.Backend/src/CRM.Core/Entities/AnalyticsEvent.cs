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
/// Represents an analytics event for tracking user actions and system events.
/// Used for business intelligence, user behavior analysis, and audit purposes.
/// </summary>
public class AnalyticsEvent : BaseEntity
{
    /// <summary>
    /// Name of the event (e.g., "QuoteCreated", "OrderSubmitted", "PaymentReceived").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity the event relates to (e.g., "Quote", "Order", "Invoice").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the related entity.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// ID of the user who triggered the event.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON metadata containing additional event-specific information.
    /// </summary>
    [Column(TypeName = "json")]
    public string? Metadata { get; set; }
}

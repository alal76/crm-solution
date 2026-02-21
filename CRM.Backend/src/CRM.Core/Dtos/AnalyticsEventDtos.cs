// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// Data transfer object for reading analytics event data.
/// </summary>
public class AnalyticsEventDto
{
    /// <summary>
    /// Unique identifier for the analytics event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the event (e.g., "QuoteCreated", "OrderSubmitted").
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity the event relates to (e.g., "Quote", "Order").
    /// </summary>
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
    /// Username of the user who triggered the event.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// JSON metadata containing additional event-specific information.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new analytics event.
/// </summary>
public class CreateAnalyticsEventDto
{
    /// <summary>
    /// Name of the event (e.g., "QuoteCreated", "OrderSubmitted").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity the event relates to (e.g., "Quote", "Order").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the related entity.
    /// </summary>
    [Required]
    public int EntityId { get; set; }

    /// <summary>
    /// ID of the user who triggered the event (optional, defaults to current user).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Timestamp when the event occurred (optional, defaults to current time).
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// JSON metadata containing additional event-specific information.
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Data transfer object for filtering analytics events.
/// </summary>
public class AnalyticsEventFilterDto
{
    /// <summary>
    /// Filter by event name.
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// Filter by entity type.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter by entity ID.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Filter by user ID.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Filter events from this date.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter events until this date.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size for pagination.
    /// </summary>
    public int PageSize { get; set; } = 50;
}

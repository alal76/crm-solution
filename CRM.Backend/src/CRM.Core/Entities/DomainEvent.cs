// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persisted domain event for event-sourcing / audit purposes.
/// Complements the in-memory IEventBus with a durable backing store.
/// </summary>
public class DomainEvent : BaseEntity
{
    /// <summary>Globally unique event identifier.</summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON-serialized event payload.</summary>
    public string Payload { get; set; } = "{}";

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Version / sequence number within the aggregate stream.</summary>
    public long Version { get; set; }

    /// <summary>Optional correlation ID linking related events.</summary>
    public string? CorrelationId { get; set; }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.Integration;

public enum OutboxEventStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

[Table("OutboxEvents")]
public class OutboxEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? CorrelationId { get; set; }
    public string? IdempotencyKey { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 5;
    public string? LastError { get; set; }
}

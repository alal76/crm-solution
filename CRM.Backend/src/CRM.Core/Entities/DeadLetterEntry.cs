// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persisted dead-letter queue entry for a message that could not be processed.
/// </summary>
public class DeadLetterEntry : BaseEntity
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public string Topic { get; set; } = string.Empty;

    /// <summary>JSON-serialized original message payload.</summary>
    public string Payload { get; set; } = "{}";

    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastRetryAt { get; set; }

    /// <summary>When true an admin has acknowledged / resolved this entry.</summary>
    public bool IsResolved { get; set; }

    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Source service or component that published the original message.</summary>
    public string? Source { get; set; }
}

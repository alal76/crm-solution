// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Persisted state for a running or completed saga instance.
/// </summary>
public class SagaInstance : BaseEntity
{
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public string SagaType { get; set; } = string.Empty;

    /// <summary>Name of the current step being executed.</summary>
    public string CurrentStep { get; set; } = string.Empty;

    /// <summary>JSON-serialized saga context / data bag.</summary>
    public string StateJson { get; set; } = "{}";

    /// <summary>Status: Running, Completed, Failed, Compensating, Compensated.</summary>
    public string Status { get; set; } = "Running";

    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    /// <summary>Optional correlation ID (e.g. order ID) to find this saga quickly.</summary>
    public string? CorrelationId { get; set; }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities.Workflow;

/// <summary>
/// Persists circuit breaker state for external services called by workflow nodes.
/// </summary>
public class WorkflowCircuitBreakerState : BaseEntity
{
    /// <summary>
    /// Unique name of the external service (e.g., "EmailProvider", "PaymentGateway").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the circuit breaker (e.g., "Closed", "Open", "HalfOpen").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string State { get; set; } = "Closed";

    /// <summary>
    /// Number of consecutive failures recorded.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Number of consecutive successes recorded (used in HalfOpen state).
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Timestamp of the most recent failure.
    /// </summary>
    public DateTime? LastFailureAt { get; set; }

    /// <summary>
    /// Timestamp of the most recent success.
    /// </summary>
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>
    /// When the circuit breaker will transition from Open to HalfOpen.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Number of failures required to trip the circuit breaker open.
    /// </summary>
    public int Threshold { get; set; } = 5;

    /// <summary>
    /// Seconds to wait before attempting recovery (Open → HalfOpen).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of test calls allowed in HalfOpen state.
    /// </summary>
    public int HalfOpenMaxCalls { get; set; } = 1;
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

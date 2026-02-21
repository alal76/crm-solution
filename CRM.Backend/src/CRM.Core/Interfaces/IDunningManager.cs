// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Dunning Manager - Payment failure recovery workflow with intelligent retry escalation.
///
/// Dunning Process (3-retry escalation):
/// - Retry 1 (Day 3): Soft decline, request payment
/// - Retry 2 (Day 7): Escalate, account warning
/// - Retry 3 (Day 10): Final notice, subscription paused
/// - Retry >3: Cancel subscription, archive customer
///
/// Automatically handles payment retries, customer notifications, and subscription lifecycle.
/// Designed to maximize revenue recovery while preventing payment churn.
///
/// SPEC: PHASE 6 - Subscription Billing Services (25 hours)
/// </summary>
public interface IDunningManager
{
    /// <summary>
    /// Process all pending dunning records (background job, runs twice daily).
    /// Finds records with NextRetryDate <= Today and attempts payment retry or escalation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dunning cycle results with retry/escalation counts</returns>
    Task<DunningCycleResultDto> ProcessDunningAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retry a failed payment with dunning escalation.
    /// Increments retry counter, escalates if needed, schedules next retry.
    /// </summary>
    /// <param name="paymentId">Payment ID that failed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retry result with escalation status if applicable</returns>
    Task<DunningRetryResultDto> RetryFailedPaymentAsync(
        int paymentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send dunning email to customer requesting payment.
    /// Escalates message severity based on retry attempt number.
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="attemptNumber">Retry attempt (1=soft, 2=escalate, 3=final, >3=cancel)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Email sending result</returns>
    Task<DunningEmailResultDto> SendDunningEmailAsync(
        int paymentId,
        int attemptNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Pause subscription due to payment failure (on 3rd dunning attempt).
    /// Prevents service delivery while allowing customer time to update payment method.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to pause</param>
    /// <param name="reason">Reason for pause</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pause result</returns>
    Task<SubscriptionPauseDto> PauseSubscriptionAsync(
        int subscriptionId,
        string reason,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cancel subscription due to exhausted dunning retries.
    /// Final action after 3+ failed retries. Archives customer if configured.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID to cancel</param>
    /// <param name="archiveCustomer">Whether to soft-delete the account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancellation result</returns>
    Task<SubscriptionCancellationDto> CancelSubscriptionOnExhaustionAsync(
        int subscriptionId,
        bool archiveCustomer = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dunning history for a subscription.
    /// Shows all retry attempts, escalations, and outcomes.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dunning records for subscription</returns>
    Task<List<DunningRecordDto>> GetDunningHistoryAsync(
        int subscriptionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get dunning status for a subscription.
    /// Shows current retry stage, next retry date, escalation level.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current dunning status</returns>
    Task<DunningStatusDto> GetDunningStatusAsync(
        int subscriptionId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Result of a complete dunning cycle run.
/// </summary>
public class DunningCycleResultDto
{
    public int ProcessedCount { get; set; }
    public int SuccessfulRetries { get; set; }
    public int EscalatedCount { get; set; }
    public int PausedSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of a single dunning retry attempt.
/// </summary>
public class DunningRetryResultDto
{
    public int PaymentId { get; set; }
    public int AttemptNumber { get; set; }
    public bool PaymentSucceeded { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? NextRetryDate { get; set; }
    public DunningEscalationLevel EscalationLevel { get; set; }
    public bool IsExhausted { get; set; }
}

/// <summary>
/// Result of dunning email sending.
/// </summary>
public class DunningEmailResultDto
{
    public int PaymentId { get; set; }
    public bool Sent { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

/// <summary>
/// Result of subscription pause.
/// </summary>
public class SubscriptionPauseDto
{
    public int SubscriptionId { get; set; }
    public bool Success { get; set; }
    public DateTime PausedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of subscription cancellation.
/// </summary>
public class SubscriptionCancellationDto
{
    public int SubscriptionId { get; set; }
    public bool Success { get; set; }
    public DateTime CancelledAt { get; set; }
    public bool CustomerArchived { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Dunning record tracking retry attempts.
/// </summary>
public class DunningRecordDto
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int RetryAttempt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? NextRetryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Current dunning status for a subscription.
/// </summary>
public class DunningStatusDto
{
    public int SubscriptionId { get; set; }
    public int CurrentAttempt { get; set; }
    public DunningEscalationLevel EscalationLevel { get; set; }
    public DateTime? NextRetryDate { get; set; }
    public bool IsPaused { get; set; }
    public bool IsExhausted { get; set; }
}

/// <summary>
/// Dunning escalation levels (severity of message).
/// </summary>
public enum DunningEscalationLevel
{
    Soft = 0,        // Day 3: Soft request
    Escalated = 1,   // Day 7: Warning
    Final = 2,       // Day 10: Final notice
    Exhausted = 3 // Day >10: Cancel
}

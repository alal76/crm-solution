// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Dunning Email Scheduler Service - Automates dunning email sequences based on retry attempts.
/// TODO-SALES003-012: Automated dunning email sequence scheduler.
///
/// Email Sequence by Retry Attempt:
/// - Attempt 1 (Day 3): "Payment Failed" - Soft reminder
/// - Attempt 2 (Day 7): "Action Required" - Warning tone
/// - Attempt 3 (Day 10): "Final Notice" - Urgent, subscription pause warning
/// - Attempt 4+: "Subscription Suspended" - Account actions required
///
/// The scheduler runs periodically to queue emails based on payment status and retry dates.
/// </summary>
public interface IDunningSchedulerService
{
    /// <summary>
    /// Schedules dunning emails for all subscriptions with failed payments.
    /// This is typically called by a background job (e.g., twice daily).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with counts of scheduled emails.</returns>
    Task<DunningScheduleResultDto> ScheduleDunningEmailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the email template type for a given dunning attempt number.
    /// </summary>
    /// <param name="attemptNumber">The retry attempt number (1-4+).</param>
    /// <returns>The email template type to use.</returns>
    DunningEmailTemplateType GetEmailTemplateForAttempt(int attemptNumber);

    /// <summary>
    /// Queues a dunning email for a specific payment.
    /// </summary>
    /// <param name="paymentId">Payment ID.</param>
    /// <param name="attemptNumber">Retry attempt number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was queued successfully.</returns>
    Task<bool> QueueDunningEmailAsync(int paymentId, int attemptNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next retry date based on attempt number and dunning configuration.
    /// </summary>
    /// <param name="attemptNumber">Current attempt number.</param>
    /// <param name="gracePeriodDays">Grace period in days (default from subscription).</param>
    /// <returns>The next scheduled retry/email date.</returns>
    DateTime GetNextRetryDate(int attemptNumber, int gracePeriodDays = 3);

    /// <summary>
    /// Sends an escalation email for critical dunning situations.
    /// TODO-SALES006-025: Dunning escalation email support.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID.</param>
    /// <param name="escalationLevel">Escalation level.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if escalation email was sent.</returns>
    Task<bool> SendEscalationEmailAsync(int subscriptionId, DunningEscalationLevel escalationLevel, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from dunning email scheduling run.
/// </summary>
public class DunningScheduleResultDto
{
    /// <summary>Total payments processed.</summary>
    public int PaymentsProcessed { get; set; }

    /// <summary>Emails scheduled for soft reminder (Attempt 1).</summary>
    public int SoftReminderEmailsScheduled { get; set; }

    /// <summary>Emails scheduled for warning (Attempt 2).</summary>
    public int WarningEmailsScheduled { get; set; }

    /// <summary>Emails scheduled for final notice (Attempt 3).</summary>
    public int FinalNoticeEmailsScheduled { get; set; }

    /// <summary>Escalation emails scheduled (Attempt 4+).</summary>
    public int EscalationEmailsScheduled { get; set; }

    /// <summary>Total emails scheduled.</summary>
    public int TotalEmailsScheduled =>
        SoftReminderEmailsScheduled + WarningEmailsScheduled + FinalNoticeEmailsScheduled + EscalationEmailsScheduled;

    /// <summary>Any errors encountered during processing.</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Processing timestamp.</summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Email template types for dunning sequences.
/// </summary>
public enum DunningEmailTemplateType
{
    /// <summary>Soft reminder - first failed payment notice.</summary>
    SoftReminder = 0,

    /// <summary>Warning - second attempt, more urgent tone.</summary>
    Warning = 1,

    /// <summary>Final notice - subscription will be paused.</summary>
    FinalNotice = 2,

    /// <summary>Suspended - subscription has been paused/cancelled.</summary>
    Suspended = 3,

    /// <summary>Recovery - payment recovered after suspension.</summary>
    Recovery = 4
}

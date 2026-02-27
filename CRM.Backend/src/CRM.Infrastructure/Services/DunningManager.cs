// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dunning Manager - Payment failure recovery with intelligent retry escalation.
///
/// Dunning Process:
/// - Retry 1 (Day 3): Soft request
/// - Retry 2 (Day 7): Escalated warning
/// - Retry 3 (Day 10): Final notice, subscription paused
/// - Retry >3: Cancel subscription
///
/// Runs twice daily via background job processor.
///
/// PHASE 6: Subscription Billing Services (25 hours)
/// SPEC: SPEC-SALES-006
/// </summary>
public class DunningManager : IDunningManager
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DunningManager> _logger;

    public DunningManager(
        ICrmDbContext context,
        ILogger<DunningManager> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DunningCycleResultDto> ProcessDunningAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting dunning cycle process");

        var result = new DunningCycleResultDto();

        try
        {
            // Find payment records due for dunning
            var today = DateTime.UtcNow.Date;
            var duePayments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == PaymentStatus.Failed && p.ScheduledDate <= today && !p.IsDeleted)
                .Take(1000)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} payments due for dunning", duePayments.Count);

            foreach (var payment in duePayments)
            {
                try
                {
                    var retryResult = await RetryFailedPaymentAsync(payment.Id, cancellationToken);
                    result.ProcessedCount++;

                    // TODO: Track successful retries when payment gateway is integrated
                    // (retryResult.PaymentSucceeded is always false until then)
                    if (retryResult.EscalationLevel == DunningEscalationLevel.Escalated)
                        result.EscalatedCount++;
                    else if (retryResult.EscalationLevel == DunningEscalationLevel.Final)
                        result.PausedSubscriptions++;
                    else if (retryResult.IsExhausted)
                        result.CancelledSubscriptions++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process dunning for payment {PaymentId}", payment.Id);
                    result.Errors.Add($"Payment {payment.Id}: {ex.Message}");
                }
            }

            _logger.LogInformation("Dunning cycle complete: {Success} succeeded, {Failed} failed",
                result.SuccessfulRetries, result.Errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in dunning cycle");
            result.Errors.Add($"Critical error: {ex.Message}");
        }

        return result;
    }

    public async Task<DunningRetryResultDto> RetryFailedPaymentAsync(
        int paymentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying failed payment {PaymentId}", paymentId);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted, cancellationToken);

        if (payment == null)
            throw new InvalidOperationException($"Payment {paymentId} not found");

        // --- TODO-SALES006-025: Load invoice + subscription early for grace period check ---
        Invoice? linkedInvoice = null;
        Subscription? linkedSubscription = null;

        if (payment.InvoiceId.HasValue)
        {
            linkedInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId, cancellationToken);

            if (linkedInvoice?.SubscriptionId.HasValue == true)
            {
                linkedSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.Id == linkedInvoice.SubscriptionId && !s.IsDeleted, cancellationToken);
            }
        }

        // --- TODO-SALES006-025: Grace Period Check ---
        // If the subscription has an active grace period, skip dunning until it expires.
        if (linkedSubscription != null && linkedInvoice != null && linkedSubscription.DunningGracePeriodDays > 0)
        {
            var gracePeriodEnd = linkedInvoice.DueDate.AddDays(linkedSubscription.DunningGracePeriodDays);
            if (DateTime.UtcNow.Date <= gracePeriodEnd.Date)
            {
                _logger.LogInformation(
                    "Dunning skipped for payment {PaymentId} — subscription {SubId} is within grace period until {GraceEnd:yyyy-MM-dd}",
                    paymentId, linkedSubscription.Id, gracePeriodEnd);

                return new DunningRetryResultDto
                {
                    PaymentId = paymentId,
                    AttemptNumber = 0,
                    PaymentSucceeded = false,
                    Status = "GracePeriod",
                    Message = $"Dunning skipped. Grace period expires {gracePeriodEnd:yyyy-MM-dd}.",
                    EscalationLevel = DunningEscalationLevel.Soft,
                    SkippedDueToGracePeriod = true
                };
            }
        }

        var attemptNumber = payment.RetryCount + 1;
        var escalationLevel = attemptNumber switch
        {
            1 => DunningEscalationLevel.Soft,
            2 => DunningEscalationLevel.Escalated,
            3 => DunningEscalationLevel.Final,
            _ => DunningEscalationLevel.Exhausted
        };

        // TODO: Integrate real payment gateway — paymentSucceeded is currently always false
        // When payment gateway is integrated, replace 'false' literals below with actual result

        // Schedule next retry (payment gateway integration pending)
        {
            var nextDays = attemptNumber switch
            {
                1 => 3,  // First retry: 3 days
                2 => 7,  // Second retry: 7 days
                3 => 14, // Third retry: 14 days
                _ => 30 // Beyond third: 30 days
            };

            payment.RetryCount = attemptNumber;
            payment.ScheduledDate = DateTime.UtcNow.AddDays(nextDays);

            // Handle escalation — reuse already-loaded subscription where available
            if (escalationLevel == DunningEscalationLevel.Final)
            {
                var sub = linkedSubscription ?? await ResolveSubscriptionFromPaymentAsync(payment, cancellationToken);
                if (sub != null)
                {
                    sub.SubscriptionStatus = SubscriptionStatus.Paused;
                    _context.Subscriptions.Update(sub);
                    linkedSubscription = sub;
                    _logger.LogInformation("Subscription {SubId} paused due to dunning", sub.Id);
                }
            }
            else if (escalationLevel == DunningEscalationLevel.Exhausted)
            {
                var sub = linkedSubscription ?? await ResolveSubscriptionFromPaymentAsync(payment, cancellationToken);
                if (sub != null)
                {
                    sub.SubscriptionStatus = SubscriptionStatus.Cancelled;
                    sub.EndDate = DateTime.UtcNow;
                    _context.Subscriptions.Update(sub);
                    linkedSubscription = sub;
                    _logger.LogInformation("Subscription {SubId} cancelled due to exhausted dunning", sub.Id);
                }
            }

            // --- TODO-SALES006-025: Track dunning attempt on subscription ---
            if (linkedSubscription != null)
            {
                linkedSubscription.LastDunningDate = DateTime.UtcNow;
                linkedSubscription.DunningAttemptCount = attemptNumber;
                _context.Subscriptions.Update(linkedSubscription);
            }
        }

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);

        // --- TODO-SALES006-025: Send dunning escalation email after each failed retry ---
        // Payment retry always fails until payment gateway is integrated
        if (linkedSubscription?.SendDunningEscalationEmails == true)
        {
            try
            {
                await SendDunningEmailAsync(paymentId, attemptNumber, cancellationToken);
                _logger.LogInformation(
                    "Dunning escalation email sent for payment {PaymentId}, attempt {Attempt}",
                    paymentId, attemptNumber);
            }
            catch (Exception emailEx)
            {
                // Email failure must not block the dunning workflow
                _logger.LogWarning(emailEx, "Failed to send dunning escalation email for payment {PaymentId}", paymentId);
            }
        }

        var nextRetryDate = payment.ScheduledDate;
        if (payment.RetryCount > 3)
            nextRetryDate = null;

        return new DunningRetryResultDto
        {
            PaymentId = paymentId,
            AttemptNumber = attemptNumber,
            PaymentSucceeded = false, // TODO: Set from payment gateway result when integrated
            Status = "Scheduled",
            Message = $"Retry scheduled for {nextRetryDate:yyyy-MM-dd}",
            NextRetryDate = nextRetryDate,
            EscalationLevel = escalationLevel,
            IsExhausted = attemptNumber > 3
        };
    }

    /// <summary>
    /// Helper: resolves the subscription linked to a payment via its invoice.
    /// Used as a fallback when the subscription was not pre-loaded (e.g. no invoice link).
    /// </summary>
    private async Task<Subscription?> ResolveSubscriptionFromPaymentAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        if (!payment.InvoiceId.HasValue)
            return null;

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId, cancellationToken);

        if (invoice?.SubscriptionId.HasValue != true)
            return null;

        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == invoice.SubscriptionId && !s.IsDeleted, cancellationToken);
    }

    public async Task<DunningEmailResultDto> SendDunningEmailAsync(
        int paymentId,
        int attemptNumber,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending dunning email for payment {PaymentId}, attempt {Attempt}",
            paymentId, attemptNumber);

        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted, cancellationToken);

        if (payment == null)
            throw new InvalidOperationException($"Payment {paymentId} not found");

        var customer = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == payment.AccountId && !c.IsDeleted, cancellationToken);

        var email = customer?.Email ?? string.Empty;

        // TODO: Replace with real email sending in production
        var sent = true;

        return new DunningEmailResultDto
        {
            PaymentId = paymentId,
            Sent = sent,
            Message = "Email sent",
            RecipientEmail = email,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<SubscriptionPauseDto> PauseSubscriptionAsync(
        int subscriptionId,
        string reason,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Pausing subscription {SubId}, reason: {Reason}",
            subscriptionId, reason);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (subscription == null)
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");

        subscription.SubscriptionStatus = SubscriptionStatus.Paused;
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionPauseDto
        {
            SubscriptionId = subscriptionId,
            Success = true,
            PausedAt = DateTime.UtcNow,
            Reason = reason
        };
    }

    public async Task<SubscriptionCancellationDto> CancelSubscriptionOnExhaustionAsync(
        int subscriptionId,
        bool archiveCustomer = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling subscription {SubId}, archiveCustomer: {Archive}",
            subscriptionId, archiveCustomer);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (subscription == null)
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");

        subscription.SubscriptionStatus = SubscriptionStatus.Cancelled;
        subscription.EndDate = DateTime.UtcNow;
        _context.Subscriptions.Update(subscription);

        var customerArchived = false;
        if (archiveCustomer && subscription.AccountId > 0)
        {
            var customer = await _context.Accounts
                .FirstOrDefaultAsync(c => c.Id == subscription.AccountId, cancellationToken);

            if (customer != null)
            {
                customer.IsDeleted = true;
                _context.Accounts.Update(customer);
                customerArchived = true;
                _logger.LogInformation("Customer {CustomerId} archived", customer.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SubscriptionCancellationDto
        {
            SubscriptionId = subscriptionId,
            Success = true,
            CancelledAt = DateTime.UtcNow,
            CustomerArchived = customerArchived,
            Message = "Subscription cancelled due to exhausted dunning retries"
        };
    }

    public async Task<List<DunningRecordDto>> GetDunningHistoryAsync(
        int subscriptionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dunning history for subscription {SubId}", subscriptionId);

        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SubscriptionId == subscriptionId && !i.IsDeleted, cancellationToken);

        if (invoice == null)
            return new List<DunningRecordDto>();

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoice.Id && p.Status == PaymentStatus.Failed && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = payments.Select((p, idx) => new DunningRecordDto
        {
            Id = p.Id,
            PaymentId = p.Id,
            RetryAttempt = p.RetryCount,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            NextRetryDate = p.ScheduledDate,
            Notes = ""
        }).ToList();

        _logger.LogInformation("Retrieved {Count} dunning records", result.Count);
        return result;
    }

    public async Task<DunningStatusDto> GetDunningStatusAsync(
        int subscriptionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dunning status for subscription {SubId}", subscriptionId);

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (subscription == null)
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");

        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.SubscriptionId == subscriptionId && !i.IsDeleted, cancellationToken);

        var failedPayment = invoice != null
            ? await _context.Payments
                .AsNoTracking()
                .Where(p => p.InvoiceId == invoice.Id && p.Status == PaymentStatus.Failed && !p.IsDeleted)
                .OrderByDescending(p => p.RetryCount)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var attempt = failedPayment?.RetryCount ?? 0;
        var escalationLevel = attempt switch
        {
            0 => DunningEscalationLevel.Soft,
            1 => DunningEscalationLevel.Soft,
            2 => DunningEscalationLevel.Escalated,
            3 => DunningEscalationLevel.Final,
            _ => DunningEscalationLevel.Exhausted
        };

        return new DunningStatusDto
        {
            SubscriptionId = subscriptionId,
            CurrentAttempt = attempt,
            EscalationLevel = escalationLevel,
            NextRetryDate = failedPayment?.ScheduledDate,
            IsPaused = subscription.SubscriptionStatus == SubscriptionStatus.Paused,
            IsExhausted = attempt > 3
        };
    }
}

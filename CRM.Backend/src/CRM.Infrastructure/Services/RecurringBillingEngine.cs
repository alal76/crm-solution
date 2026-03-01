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
/// Recurring Billing Engine - Background job for subscription invoice generation and payment processing.
///
/// Processes:
/// - Generate renewal invoices for subscriptions due for billing
/// - Process automatic payments via stored payment methods
/// - Retry failed payments with exponential backoff
///
/// Designed to run hourly via background job processor (Hangfire).
///
/// PHASE 6: Subscription Billing Services (25 hours)
/// SPEC: SPEC-SALES-006
/// </summary>
public class RecurringBillingEngine : IRecurringBillingEngine
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<RecurringBillingEngine> _logger;
    private readonly IBillingTimezoneService _timezoneService;
    private const int BatchSize = 1000;

    public RecurringBillingEngine(
        ICrmDbContext context,
        ILogger<RecurringBillingEngine> logger,
        IBillingTimezoneService timezoneService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timezoneService = timezoneService ?? throw new ArgumentNullException(nameof(timezoneService));
    }

    public async Task<BillingCycleResultDto> ProcessBillingCyclesAsync(
        DateTime? billingDate = null,
        CancellationToken cancellationToken = default)
    {
        var date = billingDate?.Date ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Starting billing cycle process for date {BillingDate}", date);

        var result = new BillingCycleResultDto
        {
            BillingDate = date
        };

        try
        {
            // Find subscriptions due for billing
            var dueSubs = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.NextBillingDate <= date && s.SubscriptionStatus == SubscriptionStatus.Active && !s.IsDeleted)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} subscriptions due for billing", dueSubs.Count);

            foreach (var sub in dueSubs)
            {
                try
                {
                    var invoice = await GenerateRenewalInvoiceAsync(sub.Id, cancellationToken);
                    result.SuccessCount++;
                    result.TotalAmountBilled += invoice.Amount;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to bill subscription {SubId}", sub.Id);
                    result.FailureCount++;
                    result.Errors.Add($"Subscription {sub.Id}: {ex.Message}");
                }
            }

            result.ProcessedCount = dueSubs.Count;
            _logger.LogInformation("Billing cycle complete: {Success} succeeded, {Failure} failed",
                result.SuccessCount, result.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in billing cycle process");
            result.Errors.Add($"Critical error: {ex.Message}");
        }

        return result;
    }

    public async Task<BillingResultDto> GenerateRenewalInvoiceAsync(
        int subscriptionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating renewal invoice for subscription {SubId}", subscriptionId);

        var sub = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (sub == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        // Calculate billing amount
        var billingAmount = await CalculateBillingAmountAsync(
            subscriptionId,
            sub.NextBillingDate ?? DateTime.UtcNow,
            sub.NextBillingDate?.AddMonths(1) ?? DateTime.UtcNow.AddMonths(1),
            cancellationToken);

        // Create invoice
        var invoice = new Invoice
        {
            AccountId = sub.AccountId,
            SubscriptionId = sub.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{sub.Id}",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Subtotal = billingAmount.BaseAmount,
            TaxAmount = billingAmount.TaxAmount,
            TotalAmount = billingAmount.TotalAmount,
            Status = InvoiceStatus.Draft,
            InvoiceType = InvoiceType.Recurring,
            Description = $"Subscription renewal for subscription {sub.Id}",
            CurrencyCode = sub.Currency ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);

        // Add line item
        invoice.LineItems.Add(new InvoiceLineItem
        {
            LineNumber = 1,
            Name = "Subscription Service",
            Description = $"Subscription renewal - {sub.SubscriptionNumber}",
            ProductId = sub.ProductId,
            SubscriptionId = sub.Id,
            Quantity = 1,
            UnitPrice = billingAmount.BaseAmount,
            ExtendedAmount = billingAmount.BaseAmount,
            TotalAmount = billingAmount.BaseAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Update subscription's next billing date (TODO-SALES006-023: timezone-aware)
        var billingTimezone = sub.BillingTimezone ?? "UTC";
        sub.NextBillingDate = _timezoneService.GetNextBillingDate(
            sub.NextBillingDate ?? DateTime.UtcNow,
            billingTimezone,
            sub.BillingPeriod);
        _context.Subscriptions.Update(sub);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} created for subscription {SubId}",
            invoice.InvoiceNumber, subscriptionId);

        return new BillingResultDto
        {
            InvoiceId = invoice.Id,
            SubscriptionId = subscriptionId,
            Amount = invoice.Amount,
            BillingDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString()
        };
    }

    public async Task<List<PaymentProcessingResultDto>> ProcessAutoPaymentsAsync(
        List<int> invoiceIds,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing auto-payments for {Count} invoices", invoiceIds.Count);

        var results = new List<PaymentProcessingResultDto>();

        foreach (var invoiceId in invoiceIds)
        {
            try
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);

                if (invoice == null)
                {
                    results.Add(new PaymentProcessingResultDto
                    {
                        InvoiceId = invoiceId,
                        Success = false,
                        Status = "Failed",
                        Message = "Invoice not found",
                        ProcessedAt = DateTime.UtcNow
                    });
                    continue;
                }

                // Simulate payment processing
                // In production, integrate with payment gateway (Stripe, etc)
                var payment = new Payment
                {
                    InvoiceId = invoiceId,
                    AccountId = invoice.AccountId,
                    Amount = invoice.Amount,
                    Status = PaymentStatus.Completed,
                    PaymentMethod = PaymentMethod.CreditCard,
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = $"TXN-{Guid.NewGuid():N}".Substring(0, 20),
                    ProcessedDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);

                // Update invoice status
                invoice.Status = InvoiceStatus.Paid;
                _context.Invoices.Update(invoice);

                await _context.SaveChangesAsync(cancellationToken);

                results.Add(new PaymentProcessingResultDto
                {
                    InvoiceId = invoiceId,
                    Success = true,
                    Status = "Processed",
                    Amount = invoice.Amount,
                    ProcessedAt = DateTime.UtcNow,
                    PaymentMethodLast4 = "****"
                });

                _logger.LogInformation("Payment processed for invoice {InvoiceId}", invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment for invoice {InvoiceId}", invoiceId);
                results.Add(new PaymentProcessingResultDto
                {
                    InvoiceId = invoiceId,
                    Success = false,
                    Status = "Failed",
                    Message = ex.Message,
                    ProcessedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }

    public async Task<PaymentRetryDto> RetryFailedPaymentAsync(
        int invoiceId,
        int retryAttempt,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying payment for invoice {InvoiceId}, attempt {Attempt}",
            invoiceId, retryAttempt);

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        // Calculate next retry date with exponential backoff
        var backoffDays = retryAttempt switch
        {
            1 => 3,    // First retry: 3 days
            2 => 7,    // Second retry: 7 days
            3 => 14,   // Third retry: 14 days
            _ => 30 // After third: 30 days
        };

        var nextRetryDate = DateTime.UtcNow.AddDays(backoffDays);
        var isExhausted = retryAttempt > 3;

        _logger.LogInformation("Payment retry scheduled for {RetryDate}, attempt {Attempt}, exhausted {IsExhausted}",
            nextRetryDate, retryAttempt, isExhausted);

        return new PaymentRetryDto
        {
            InvoiceId = invoiceId,
            RetryAttempt = retryAttempt,
            Success = false,
            Message = $"Retry scheduled for {nextRetryDate:yyyy-MM-dd}",
            NextRetryDate = isExhausted ? null : nextRetryDate,
            IsExhausted = isExhausted
        };
    }

    public async Task<BillingCalculationDto> CalculateBillingAmountAsync(
        int subscriptionId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating billing amount for subscription {SubId}, period {Start} to {End}",
            subscriptionId, startDate, endDate);

        var sub = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, cancellationToken);

        if (sub == null)
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} not found");
        }

        var baseAmount = sub.Amount;

        var result = new BillingCalculationDto
        {
            SubscriptionId = subscriptionId,
            StartDate = startDate,
            EndDate = endDate,
            BaseAmount = baseAmount,
            UsageAmount = 0m,
            AdjustmentAmount = 0m,
            CreditAmount = 0m,
            TaxAmount = baseAmount * 0.1m, // 10% tax
            TotalAmount = baseAmount + (baseAmount * 0.1m)
        };

        _logger.LogInformation("Billing calculation complete: {Total}", result.TotalAmount);
        return result;
    }

    public async Task<List<BillingHistoryDto>> GetBillingHistoryAsync(
        int subscriptionId,
        int limit = 12,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting billing history for subscription {SubId}, limit {Limit}",
            subscriptionId, limit);

        var history = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.SubscriptionId == subscriptionId && !i.IsDeleted)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(limit)
            .Select(i => new BillingHistoryDto
            {
                InvoiceId = i.Id,
                BillingDate = i.InvoiceDate,
                Amount = i.Amount,
                Status = i.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} billing records", history.Count);
        return history;
    }
}

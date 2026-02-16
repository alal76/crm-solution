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

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Recurring Billing Engine - Background job for subscription invoice generation and payment processing.
/// 
/// Responsibilities:
/// - Generate renewal invoices for subscriptions due for billing
/// - Process automatic payments via stored payment methods
/// - Retry failed payments with exponential backoff (3 attempts)
/// - Track billing cycles and payment status
/// 
/// Runs as hourly background job (via Hangfire or similar background job processor).
/// 
/// SPEC: PHASE 6 - Subscription Billing Services (25 hours)
/// </summary>
public interface IRecurringBillingEngine
{
    /// <summary>
    /// Process all subscriptions due for billing (background job, runs hourly).
    /// Finds subscriptions with NextBillingDate <= Today and generates renewal invoices.
    /// Batch processes up to 1000 subscriptions at a time to prevent memory spikes.
    /// </summary>
    /// <param name="billingDate">Date to bill for (default: Today UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Billing cycle results with success/failure counts</returns>
    Task<BillingCycleResultDto> ProcessBillingCyclesAsync(
        DateTime? billingDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate renewal invoice for a subscription immediately.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated invoice details</returns>
    Task<BillingResultDto> GenerateRenewalInvoiceAsync(
        int subscriptionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Process automatic payment for list of invoices.
    /// Uses stored payment method on account (credit card, bank account, etc).
    /// </summary>
    /// <param name="invoiceIds">List of invoice IDs to charge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment processing results per invoice</returns>
    Task<List<PaymentProcessingResultDto>> ProcessAutoPaymentsAsync(
        List<int> invoiceIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retry a failed payment with exponential backoff.
    /// Tracks retry attempt number and schedules next retry if needed.
    /// </summary>
    /// <param name="invoiceId">Invoice ID with failed payment</param>
    /// <param name="retryAttempt">Retry attempt number (1=first, 2=second, 3=final)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Retry result with next retry schedule if applicable</returns>
    Task<PaymentRetryDto> RetryFailedPaymentAsync(
        int invoiceId,
        int retryAttempt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Calculate billing amount for a subscription in a given period.
    /// Includes base charge + usage-based fees + adjustments - credits.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="startDate">Billing period start</param>
    /// <param name="endDate">Billing period end</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Calculated billing amount with breakdown</returns>
    Task<BillingCalculationDto> CalculateBillingAmountAsync(
        int subscriptionId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get billing history for a subscription.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <param name="limit">Maximum records to return (default: 12 months)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of billing transactions</returns>
    Task<List<BillingHistoryDto>> GetBillingHistoryAsync(
        int subscriptionId,
        int limit = 12,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a complete billing cycle run.
/// </summary>
public class BillingCycleResultDto
{
    public DateTime BillingDate { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public decimal TotalAmountBilled { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of billing a single subscription.
/// </summary>
public class BillingResultDto
{
    public int InvoiceId { get; set; }
    public int SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime BillingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Generated";
}

/// <summary>
/// Result of payment processing attempt.
/// </summary>
public class PaymentProcessingResultDto
{
    public int InvoiceId { get; set; }
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string? PaymentMethodLast4 { get; set; }
}

/// <summary>
/// Result of a payment retry attempt.
/// </summary>
public class PaymentRetryDto
{
    public int InvoiceId { get; set; }
    public int RetryAttempt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? NextRetryDate { get; set; }
    public bool IsExhausted { get; set; }
}

/// <summary>
/// Billing amount calculation breakdown.
/// </summary>
public class BillingCalculationDto
{
    public int SubscriptionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal UsageAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Single billing transaction in history.
/// </summary>
public class BillingHistoryDto
{
    public int InvoiceId { get; set; }
    public DateTime BillingDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

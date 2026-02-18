// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for payment processing operations.
/// Handles payment lifecycle from initiation to reconciliation.
/// </summary>
public interface IPaymentService
{
    #region CRUD Operations

    /// <summary>Gets all payments with optional filtering.</summary>
    Task<IEnumerable<Payment>> GetAllAsync(
        int? accountId = null,
        int? invoiceId = null,
        PaymentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a payment by ID.</summary>
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a payment by transaction ID.</summary>
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new payment record.</summary>
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing payment.</summary>
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);

    /// <summary>Deletes a payment (soft delete).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    #endregion

    #region Payment Processing

    /// <summary>Processes a payment for an invoice.</summary>
    Task<PaymentResult> ProcessPaymentAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method,
        PaymentDetails details,
        CancellationToken cancellationToken = default);

    /// <summary>Processes a refund for a payment.</summary>
    Task<PaymentResult> ProcessRefundAsync(
        int paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>Voids a pending payment.</summary>
    Task<PaymentResult> VoidPaymentAsync(int paymentId, string reason, CancellationToken cancellationToken = default);

    /// <summary>Captures a pre-authorized payment.</summary>
    Task<PaymentResult> CapturePaymentAsync(int paymentId, decimal? amount = null, CancellationToken cancellationToken = default);

    #endregion

    #region Status Management

    /// <summary>Updates payment status.</summary>
    Task<Payment> UpdateStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default);

    /// <summary>Marks a payment as completed.</summary>
    Task<Payment> MarkAsCompletedAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>Marks a payment as failed.</summary>
    Task<Payment> MarkAsFailedAsync(int paymentId, string failureReason, CancellationToken cancellationToken = default);

    #endregion

    #region Queries

    /// <summary>Gets payments within a date range.</summary>
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>Gets pending payments requiring action.</summary>
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets failed payments for retry.</summary>
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync(int maxRetries = 3, CancellationToken cancellationToken = default);

    /// <summary>Gets payment statistics.</summary>
    Task<PaymentStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets payment history for an account.</summary>
    Task<IEnumerable<Payment>> GetAccountPaymentHistoryAsync(int accountId, CancellationToken cancellationToken = default);

    #endregion

    #region Reconciliation

    /// <summary>Reconciles a payment with bank records.</summary>
    Task<bool> ReconcilePaymentAsync(int paymentId, string bankReference, CancellationToken cancellationToken = default);

    /// <summary>Gets unreconciled payments.</summary>
    Task<IEnumerable<Payment>> GetUnreconciledPaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Applies a payment to multiple invoices.</summary>
    Task<IEnumerable<PaymentAllocation>> ApplyPaymentToInvoicesAsync(
        int paymentId,
        IEnumerable<PaymentAllocation> allocations,
        CancellationToken cancellationToken = default);

    #endregion

    #region Retry & Recovery

    /// <summary>Retries a failed payment.</summary>
    Task<PaymentResult> RetryPaymentAsync(int paymentId, CancellationToken cancellationToken = default);

    /// <summary>Schedules a payment for future processing.</summary>
    Task<Payment> SchedulePaymentAsync(Payment payment, DateTime scheduledDate, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Result of a payment operation.
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Payment? Payment { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Payment details for processing.
/// </summary>
public class PaymentDetails
{
    public string? CardNumber { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardholderName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? PaymentToken { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingZip { get; set; }
    public string? BillingCountry { get; set; }
}

/// <summary>
/// Payment allocation to an invoice.
/// </summary>
public class PaymentAllocation
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Payment statistics for reporting.
/// </summary>
public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public double SuccessRate { get; set; }
    public double AveragePaymentAmount { get; set; }
    public Dictionary<PaymentMethod, int> PaymentsByMethod { get; set; } = new();
}

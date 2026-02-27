// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IPaymentService for payment processing operations.
/// Handles payment lifecycle from initiation to reconciliation.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ICrmDbContext context, ILogger<PaymentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetAllAsync(
        int? accountId = null,
        int? invoiceId = null,
        PaymentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Account)
            .Where(p => !p.IsDeleted);

        if (accountId.HasValue)
        {
            query = query.Where(p => p.AccountId == accountId.Value);
        }

        if (invoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == invoiceId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Account)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Account)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId && !p.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(payment.PaymentNumber))
        {
            payment.PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken);
        }

        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created payment {PaymentNumber} for {Amount}", payment.PaymentNumber, payment.Amount);
        return payment;
    }

    /// <inheritdoc />
    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Payments.FindAsync(new object[] { payment.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Payment {payment.Id} not found");
        }

        payment.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated payment {PaymentNumber}", payment.PaymentNumber);
        return payment;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
        if (payment == null || payment.IsDeleted)
        {
            return false;
        }

        payment.IsDeleted = true;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted payment {PaymentNumber}", payment.PaymentNumber);
        return true;
    }

    #endregion

    #region Payment Processing

    /// <inheritdoc />
    public async Task<PaymentResult> ProcessPaymentAsync(
        int invoiceId,
        decimal amount,
        PaymentMethod method,
        PaymentDetails details,
        CancellationToken cancellationToken = default)
    {
        // Validate payment amount
        if (amount <= 0)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "INVALID_AMOUNT",
                ErrorMessage = "Payment amount must be greater than zero"
            };
        }

        var invoice = await _context.Invoices.FindAsync(new object[] { invoiceId }, cancellationToken);
        if (invoice == null || invoice.IsDeleted)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "INVOICE_NOT_FOUND",
                ErrorMessage = $"Invoice {invoiceId} not found"
            };
        }

        // Prevent overpayment beyond the outstanding balance
        if (invoice.BalanceDue > 0 && amount > invoice.BalanceDue)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "AMOUNT_EXCEEDS_BALANCE",
                ErrorMessage = $"Payment amount {amount:F2} exceeds outstanding balance {invoice.BalanceDue:F2}"
            };
        }

        var payment = new Payment
        {
            PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken),
            InvoiceId = invoiceId,
            AccountId = invoice.AccountId,
            Amount = amount,
            PaymentMethod = method,
            Status = PaymentStatus.Processing,
            PaymentDate = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Store masked card info if credit card payment (null-safe on details)
        if (method == PaymentMethod.CreditCard && details != null && !string.IsNullOrEmpty(details.CardNumber))
        {
            payment.CardLast4 = details.CardNumber.Length >= 4
                ? details.CardNumber[^4..]
                : details.CardNumber;
            payment.CardholderName = details.CardholderName;
        }

        try
        {
            // BuiltIn payment processing: records the payment and updates invoice balances.
            // For external gateway integration, swap to an IPaymentGateway provider.
            payment.Status = PaymentStatus.Completed;
            payment.ProcessedDate = DateTime.UtcNow;
            payment.AuthorizationCode = GenerateAuthCode();

            _context.Payments.Add(payment);

            // Update invoice paid amount and status
            invoice.AmountPaid += amount;
            if (invoice.BalanceDue <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Processed payment {PaymentNumber} of {Amount:F2} for invoice {InvoiceId} via {Method}",
                payment.PaymentNumber, amount, invoiceId, method);

            return new PaymentResult
            {
                Success = true,
                TransactionId = payment.TransactionId,
                AuthorizationCode = payment.AuthorizationCode,
                Payment = payment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed for invoice {InvoiceId}, amount {Amount:F2}", invoiceId, amount);
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "PROCESSING_ERROR",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<PaymentResult> ProcessRefundAsync(
        int paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "PAYMENT_NOT_FOUND",
                ErrorMessage = $"Payment {paymentId} not found"
            };
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "INVALID_STATUS",
                ErrorMessage = $"Cannot refund payment in status {payment.Status}"
            };
        }

        if (amount > payment.Amount - payment.RefundedAmount)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "INVALID_AMOUNT",
                ErrorMessage = "Refund amount exceeds available amount"
            };
        }

        // Create refund payment
        var refund = new Payment
        {
            PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken),
            InvoiceId = payment.InvoiceId,
            AccountId = payment.AccountId,
            Amount = -amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentType = PaymentType.Refund,
            Status = PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            ProcessedDate = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
            Description = reason,
            OriginalPaymentId = paymentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(refund);

        // Update original payment
        payment.RefundedAmount += amount;
        payment.Status = payment.RefundedAmount >= payment.Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;
        payment.RefundDate = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        // Update invoice
        if (payment.InvoiceId.HasValue)
        {
            var invoice = await _context.Invoices.FindAsync(new object[] { payment.InvoiceId.Value }, cancellationToken);
            if (invoice != null)
            {
                invoice.AmountPaid -= amount;
                invoice.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Processed refund of {Amount} for payment {PaymentNumber}", amount, payment.PaymentNumber);

        return new PaymentResult
        {
            Success = true,
            TransactionId = refund.TransactionId,
            Payment = refund
        };
    }

    /// <inheritdoc />
    public async Task<PaymentResult> VoidPaymentAsync(int paymentId, string reason, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "PAYMENT_NOT_FOUND",
                ErrorMessage = $"Payment {paymentId} not found"
            };
        }

        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "CANNOT_VOID",
                ErrorMessage = $"Cannot void payment in status {payment.Status}"
            };
        }

        payment.Status = PaymentStatus.Voided;
        payment.Description = $"Voided: {reason}";
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Voided payment {PaymentNumber}: {Reason}", payment.PaymentNumber, reason);

        return new PaymentResult
        {
            Success = true,
            TransactionId = payment.TransactionId,
            Payment = payment
        };
    }

    /// <inheritdoc />
    public async Task<PaymentResult> CapturePaymentAsync(int paymentId, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "PAYMENT_NOT_FOUND",
                ErrorMessage = $"Payment {paymentId} not found"
            };
        }

        if (payment.PaymentType != PaymentType.Authorization)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "NOT_AUTHORIZATION",
                ErrorMessage = "Payment is not an authorization"
            };
        }

        var captureAmount = amount ?? payment.Amount;

        payment.PaymentType = PaymentType.Capture;
        payment.Status = PaymentStatus.Completed;
        payment.Amount = captureAmount;
        payment.ProcessedDate = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Captured payment {PaymentNumber} for {Amount}", payment.PaymentNumber, captureAmount);

        return new PaymentResult
        {
            Success = true,
            TransactionId = payment.TransactionId,
            Payment = payment
        };
    }

    #endregion

    #region Status Management

    /// <inheritdoc />
    public async Task<Payment> UpdateStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {paymentId} not found");
        }

        payment.Status = status;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} status updated to {Status}", payment.PaymentNumber, status);
        return payment;
    }

    /// <inheritdoc />
    public async Task<Payment> MarkAsCompletedAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {paymentId} not found");
        }

        payment.Status = PaymentStatus.Completed;
        payment.ProcessedDate = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} marked as completed", payment.PaymentNumber);
        return payment;
    }

    /// <inheritdoc />
    public async Task<Payment> MarkAsFailedAsync(int paymentId, string failureReason, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {paymentId} not found");
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = failureReason;
        payment.RetryCount = payment.RetryCount + 1;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Payment {PaymentNumber} failed: {Reason}", payment.PaymentNumber, failureReason);
        return payment;
    }

    #endregion

    #region Queries

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => !p.IsDeleted && p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => !p.IsDeleted && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing))
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Failed && p.RetryCount < maxRetries)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaymentStatistics> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Where(p => !p.IsDeleted && p.PaymentType == PaymentType.Payment);

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        }

        var payments = await query.ToListAsync(cancellationToken);
        var successfulPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var failedPayments = payments.Where(p => p.Status == PaymentStatus.Failed || p.Status == PaymentStatus.Declined).ToList();
        var pendingPayments = payments.Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing).ToList();

        var refunds = await _context.Payments
            .Where(p => !p.IsDeleted && p.PaymentType == PaymentType.Refund)
            .SumAsync(p => p.Amount, cancellationToken);

        return new PaymentStatistics
        {
            TotalPayments = payments.Count,
            SuccessfulPayments = successfulPayments.Count,
            FailedPayments = failedPayments.Count,
            PendingPayments = pendingPayments.Count,
            TotalAmount = successfulPayments.Sum(p => p.Amount),
            SuccessfulAmount = successfulPayments.Sum(p => p.Amount),
            RefundedAmount = Math.Abs(refunds),
            SuccessRate = payments.Count > 0 ? (double)successfulPayments.Count / payments.Count * 100 : 0,
            AveragePaymentAmount = successfulPayments.Count > 0 ? (double)successfulPayments.Average(p => p.Amount) : 0,
            PaymentsByMethod = successfulPayments
                .GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetAccountPaymentHistoryAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Invoice)
            .Where(p => p.AccountId == accountId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Reconciliation

    /// <inheritdoc />
    public async Task<bool> ReconcilePaymentAsync(int paymentId, string bankReference, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return false;
        }

        payment.BankReference = bankReference;
        payment.IsReconciled = true;
        payment.ReconciledDate = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} reconciled with bank reference {BankReference}", payment.PaymentNumber, bankReference);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetUnreconciledPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => !p.IsDeleted && p.Status == PaymentStatus.Completed && !p.IsReconciled)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PaymentAllocation>> ApplyPaymentToInvoicesAsync(
        int paymentId,
        IEnumerable<PaymentAllocation> allocations,
        CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            throw new InvalidOperationException($"Payment {paymentId} not found");
        }

        var totalAllocated = allocations.Sum(a => a.Amount);
        if (totalAllocated > payment.Amount)
        {
            throw new InvalidOperationException("Total allocation exceeds payment amount");
        }

        var appliedAllocations = new List<PaymentAllocation>();

        foreach (var allocation in allocations)
        {
            var invoice = await _context.Invoices.FindAsync(new object[] { allocation.InvoiceId }, cancellationToken);
            if (invoice == null || invoice.IsDeleted)
            {
                continue;
            }

            invoice.AmountPaid += allocation.Amount;
            if (invoice.BalanceDue <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            invoice.UpdatedAt = DateTime.UtcNow;

            appliedAllocations.Add(allocation);
        }

        payment.AmountApplied = appliedAllocations.Sum(a => a.Amount);
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Applied payment {PaymentNumber} to {Count} invoices", payment.PaymentNumber, appliedAllocations.Count);

        return appliedAllocations;
    }

    #endregion

    #region Retry & Recovery

    /// <inheritdoc />
    public async Task<PaymentResult> RetryPaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "PAYMENT_NOT_FOUND",
                ErrorMessage = $"Payment {paymentId} not found"
            };
        }

        if (payment.Status != PaymentStatus.Failed)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorCode = "INVALID_STATUS",
                ErrorMessage = "Only failed payments can be retried"
            };
        }

        // Retry payment
        payment.Status = PaymentStatus.Processing;
        payment.RetryCount = payment.RetryCount + 1;
        payment.UpdatedAt = DateTime.UtcNow;

        // Simulate retry success (in production, call actual gateway)
        payment.Status = PaymentStatus.Completed;
        payment.ProcessedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retried payment {PaymentNumber}, attempt {Attempt}", payment.PaymentNumber, payment.RetryCount);

        return new PaymentResult
        {
            Success = true,
            TransactionId = payment.TransactionId,
            Payment = payment
        };
    }

    /// <inheritdoc />
    public async Task<Payment> SchedulePaymentAsync(Payment payment, DateTime scheduledDate, CancellationToken cancellationToken = default)
    {
        payment.ScheduledDate = scheduledDate;
        payment.Status = PaymentStatus.Pending;
        payment.PaymentNumber = await GeneratePaymentNumberAsync(cancellationToken);
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Scheduled payment {PaymentNumber} for {ScheduledDate}", payment.PaymentNumber, scheduledDate);

        return payment;
    }

    #endregion

    #region Private Helpers

    private async Task<string> GeneratePaymentNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = "PAY";
        var year = DateTime.UtcNow.ToString("yy");
        var month = DateTime.UtcNow.ToString("MM");

        var lastPayment = await _context.Payments
            .Where(p => p.PaymentNumber.StartsWith($"{prefix}-{year}{month}"))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastPayment != null)
        {
            var parts = lastPayment.PaymentNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}-{year}{month}-{sequence:D4}";
    }

    private static string GenerateAuthCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }

    #endregion
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0. See LICENSE for details.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IInvoiceService for invoice management.
/// Handles invoice lifecycle from creation to payment reconciliation.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ICrmDbContext context, ILogger<InvoiceService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <inheritdoc />
    public async Task<IEnumerable<Invoice>> GetAllAsync(
        int? customerId = null,
        InvoiceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .Include(i => i.Account)
            .Include(i => i.LineItems)
            .Where(i => !i.IsDeleted);

        if (customerId.HasValue)
        {
            query = query.Where(i => i.AccountId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        return await query.OrderByDescending(i => i.InvoiceDate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Account)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Account)
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && !i.IsDeleted, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken);
        }

        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created invoice {InvoiceNumber} for account {AccountId}", invoice.InvoiceNumber, invoice.AccountId);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Invoices.FindAsync(new object[] { invoice.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Invoice {invoice.Id} not found");
        }

        invoice.UpdatedAt = DateTime.UtcNow;
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated invoice {InvoiceNumber}", invoice.InvoiceNumber);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FindAsync(new object[] { id }, cancellationToken);
        if (invoice == null || invoice.IsDeleted)
        {
            return false;
        }

        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted invoice {InvoiceNumber}", invoice.InvoiceNumber);
        return true;
    }

    #endregion

    #region Invoice Operations

    /// <inheritdoc />
    public async Task<Invoice> CreateFromOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.LineItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken),
            AccountId = order.AccountId,
            OrderId = orderId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            CurrencyCode = order.CurrencyCode ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Copy line items
        foreach (var orderLine in order.LineItems.Where(l => !l.IsDeleted))
        {
            invoice.LineItems.Add(new InvoiceLineItem
            {
                LineNumber = orderLine.LineNumber,
                ProductId = orderLine.ProductId,
                Description = orderLine.Description,
                Quantity = orderLine.Quantity,
                UnitPrice = orderLine.UnitPrice,
                DiscountAmount = orderLine.DiscountAmount,
                TaxAmount = orderLine.TaxAmount,
                TotalAmount = orderLine.TotalAmount,
                OrderLineItemId = orderLine.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created invoice {InvoiceNumber} from order {OrderId}", invoice.InvoiceNumber, orderId);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> CreateFromQuoteAsync(int quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _context.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId && !q.IsDeleted, cancellationToken);

        if (quote == null)
        {
            throw new InvalidOperationException($"Quote {quoteId} not found");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = await GenerateInvoiceNumberAsync(cancellationToken),
            AccountId = quote.AccountId ?? 0,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft,
            Subtotal = quote.Subtotal,
            TaxAmount = quote.TaxAmount,
            DiscountAmount = quote.DiscountAmount,
            TotalAmount = quote.TotalAmount,
            CurrencyCode = quote.CurrencyCode ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Copy line items from quote
        int lineNumber = 1;
        foreach (var quoteLine in (quote.QuoteLineItems ?? Enumerable.Empty<QuoteLineItem>()).Where(l => !l.IsDeleted))
        {
            invoice.LineItems.Add(new InvoiceLineItem
            {
                LineNumber = lineNumber++,
                ProductId = quoteLine.ProductId,
                Description = quoteLine.Description ?? string.Empty,
                Quantity = quoteLine.Quantity,
                UnitPrice = quoteLine.UnitPrice,
                DiscountAmount = quoteLine.TotalDiscount,
                TotalAmount = quoteLine.Total,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created invoice {InvoiceNumber} from quote {QuoteId}", invoice.InvoiceNumber, quoteId);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = "INV";
        var year = DateTime.UtcNow.ToString("yy");
        var month = DateTime.UtcNow.ToString("MM");

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"{prefix}-{year}{month}"))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequence = 1;
        if (lastInvoice != null)
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"{prefix}-{year}{month}-{sequence:D4}";
    }

    /// <inheritdoc />
    public async Task<bool> SendInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} marked as sent", invoice.InvoiceNumber);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsViewedAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            return false;
        }

        if (invoice.ViewedDate == null)
        {
            invoice.ViewedDate = DateTime.UtcNow;
            if (invoice.Status == InvoiceStatus.Sent)
            {
                invoice.Status = InvoiceStatus.Viewed;
            }
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Invoice {InvoiceNumber} marked as viewed", invoice.InvoiceNumber);
        }

        return true;
    }

    #endregion

    #region Status Management

    /// <inheritdoc />
    public async Task<Invoice> UpdateStatusAsync(int invoiceId, InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} status updated to {Status}", invoice.InvoiceNumber, status);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> ApproveAsync(int invoiceId, int approvedById, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.PendingApproval)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} cannot be approved in status {invoice.Status}");
        }

        invoice.Status = InvoiceStatus.Approved;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} approved by user {UserId}", invoice.InvoiceNumber, approvedById);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> VoidAsync(int invoiceId, string reason, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot void a paid invoice");
        }

        invoice.Status = InvoiceStatus.Voided;
        invoice.VoidedDate = DateTime.UtcNow;
        invoice.VoidReason = reason;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} voided: {Reason}", invoice.InvoiceNumber, reason);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> MarkAsPaidAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceNumber} marked as paid", invoice.InvoiceNumber);
        return invoice;
    }

    #endregion

    #region Payment Operations

    /// <inheritdoc />
    public async Task<Invoice> RecordPaymentAsync(int invoiceId, decimal amount, PaymentMethod method, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            AccountId = invoice.AccountId,
            Amount = amount,
            PaymentMethod = method,
            Status = PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            TransactionId = Guid.NewGuid().ToString("N")[..16].ToUpper(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

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

        _logger.LogInformation("Recorded payment of {Amount} for invoice {InvoiceNumber}", amount, invoice.InvoiceNumber);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<decimal> GetOutstandingBalanceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        return invoice?.BalanceDue ?? 0;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Payment>> GetPaymentsAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Queries

    /// <inheritdoc />
    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync(int? daysPastDue = null, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.Invoices
            .Include(i => i.Account)
            .Where(i => !i.IsDeleted
                && i.Status != InvoiceStatus.Paid
                && i.Status != InvoiceStatus.Voided
                && i.DueDate < today);

        if (daysPastDue.HasValue)
        {
            var minDate = today.AddDays(-daysPastDue.Value);
            query = query.Where(i => i.DueDate >= minDate);
        }

        return await query.OrderBy(i => i.DueDate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Invoice>> GetInvoicesDueAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Account)
            .Where(i => !i.IsDeleted
                && i.Status != InvoiceStatus.Paid
                && i.Status != InvoiceStatus.Voided
                && i.DueDate >= fromDate
                && i.DueDate <= toDate)
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InvoiceStatistics> GetCustomerStatisticsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Where(i => i.AccountId == customerId && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();

        return new InvoiceStatistics
        {
            TotalInvoices = invoices.Count,
            PaidInvoices = paidInvoices.Count,
            OverdueInvoices = invoices.Count(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Voided && i.DueDate < today),
            DraftInvoices = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            TotalInvoiced = invoices.Sum(i => i.TotalAmount),
            TotalPaid = invoices.Sum(i => i.AmountPaid),
            TotalOutstanding = invoices.Sum(i => i.BalanceDue),
            AverageInvoiceAmount = invoices.Count > 0 ? invoices.Average(i => i.TotalAmount) : 0,
            AverageDaysToPayment = paidInvoices.Count > 0
                ? paidInvoices.Where(i => i.PaidDate.HasValue)
                    .Average(i => (i.PaidDate!.Value - i.InvoiceDate).TotalDays)
                : 0
        };
    }

    #endregion

    #region Line Items

    /// <inheritdoc />
    public async Task<InvoiceLineItem> AddLineItemAsync(int invoiceId, InvoiceLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        lineItem.InvoiceId = invoiceId;
        lineItem.CreatedAt = DateTime.UtcNow;
        lineItem.UpdatedAt = DateTime.UtcNow;

        if (lineItem.LineNumber == 0)
        {
            lineItem.LineNumber = (invoice.LineItems.Max(l => (int?)l.LineNumber) ?? 0) + 1;
        }

        _context.InvoiceLineItems.Add(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate totals
        await RecalculateTotalsAsync(invoiceId, cancellationToken);

        return lineItem;
    }

    /// <inheritdoc />
    public async Task<InvoiceLineItem> UpdateLineItemAsync(InvoiceLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var existing = await _context.InvoiceLineItems.FindAsync(new object[] { lineItem.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
        {
            throw new InvalidOperationException($"Line item {lineItem.Id} not found");
        }

        lineItem.UpdatedAt = DateTime.UtcNow;
        _context.InvoiceLineItems.Update(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate totals
        await RecalculateTotalsAsync(lineItem.InvoiceId, cancellationToken);

        return lineItem;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default)
    {
        var lineItem = await _context.InvoiceLineItems.FindAsync(new object[] { lineItemId }, cancellationToken);
        if (lineItem == null || lineItem.IsDeleted)
        {
            return false;
        }

        var invoiceId = lineItem.InvoiceId;
        lineItem.IsDeleted = true;
        lineItem.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate totals
        await RecalculateTotalsAsync(invoiceId, cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InvoiceLineItem>> GetLineItemsAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceLineItems
            .Where(l => l.InvoiceId == invoiceId && !l.IsDeleted)
            .OrderBy(l => l.LineNumber)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Calculations

    /// <inheritdoc />
    public async Task<Invoice> RecalculateTotalsAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);

        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        var activeLines = invoice.LineItems.Where(l => !l.IsDeleted).ToList();

        invoice.Subtotal = activeLines.Sum(l => l.Quantity * l.UnitPrice);
        invoice.DiscountAmount = activeLines.Sum(l => l.DiscountAmount);
        invoice.TaxAmount = activeLines.Sum(l => l.TaxAmount);
        invoice.TotalAmount = invoice.Subtotal - invoice.DiscountAmount + invoice.TaxAmount + invoice.ShippingAmount + invoice.FeesAmount;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recalculated totals for invoice {InvoiceNumber}", invoice.InvoiceNumber);
        return invoice;
    }

    /// <inheritdoc />
    public async Task<Invoice> ApplyDiscountAsync(int invoiceId, decimal discountAmount, string? discountCode = null, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice {invoiceId} not found");
        }

        invoice.DiscountAmount += discountAmount;
        invoice.TotalAmount = invoice.Subtotal - invoice.DiscountAmount + invoice.TaxAmount + invoice.ShippingAmount + invoice.FeesAmount;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Applied discount of {Amount} to invoice {InvoiceNumber}", discountAmount, invoice.InvoiceNumber);
        return invoice;
    }

    #endregion
}

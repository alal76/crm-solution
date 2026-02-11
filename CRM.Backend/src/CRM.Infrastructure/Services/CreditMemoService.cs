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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICreditMemoService for credit memo management.
/// </summary>
public class CreditMemoService : ICreditMemoService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CreditMemoService> _logger;

    public CreditMemoService(ICrmDbContext context, ILogger<CreditMemoService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CreditMemo>> GetAllAsync(int? accountId = null, CreditMemoStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CreditMemos
            .Include(c => c.Account)
            .Include(c => c.LineItems)
            .Where(c => !c.IsDeleted);

        if (accountId.HasValue) query = query.Where(c => c.AccountId == accountId.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);

        return await query.OrderByDescending(c => c.CreditMemoDate).ToListAsync(cancellationToken);
    }

    public async Task<CreditMemo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CreditMemos
            .Include(c => c.Account)
            .Include(c => c.LineItems)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<CreditMemo?> GetByCreditMemoNumberAsync(string creditMemoNumber, CancellationToken cancellationToken = default)
    {
        return await _context.CreditMemos
            .Include(c => c.Account)
            .Include(c => c.LineItems)
            .FirstOrDefaultAsync(c => c.CreditMemoNumber == creditMemoNumber && !c.IsDeleted, cancellationToken);
    }

    public async Task<CreditMemo> CreateAsync(CreditMemo creditMemo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(creditMemo.CreditMemoNumber))
        {
            creditMemo.CreditMemoNumber = await GenerateCreditMemoNumberAsync(cancellationToken);
        }

        creditMemo.CreatedAt = DateTime.UtcNow;
        creditMemo.UpdatedAt = DateTime.UtcNow;

        _context.CreditMemos.Add(creditMemo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created credit memo {CreditMemoNumber} for account {AccountId}", creditMemo.CreditMemoNumber, creditMemo.AccountId);
        return creditMemo;
    }

    public async Task<CreditMemo> UpdateAsync(CreditMemo creditMemo, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CreditMemos.FindAsync(new object[] { creditMemo.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted)
            throw new InvalidOperationException($"Credit memo {creditMemo.Id} not found");

        creditMemo.UpdatedAt = DateTime.UtcNow;
        _context.CreditMemos.Update(creditMemo);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated credit memo {CreditMemoNumber}", creditMemo.CreditMemoNumber);
        return creditMemo;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cm = await _context.CreditMemos.FindAsync(new object[] { id }, cancellationToken);
        if (cm == null || cm.IsDeleted) return false;

        cm.IsDeleted = true;
        cm.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted credit memo {CreditMemoNumber}", cm.CreditMemoNumber);
        return true;
    }

    public async Task<CreditMemo> CreateFromInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {invoiceId} not found");

        var cm = new CreditMemo
        {
            CreditMemoNumber = await GenerateCreditMemoNumberAsync(cancellationToken),
            AccountId = invoice.AccountId,
            SourceInvoiceId = invoiceId,
            CreditMemoDate = DateTime.UtcNow,
            Status = CreditMemoStatus.Draft,
            Amount = invoice.TotalAmount * -1, // default negative value
            CurrencyCode = invoice.CurrencyCode ?? "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // copy limited line item info
        int line = 1;
        foreach (var li in invoice.LineItems.Where(l => !l.IsDeleted))
        {
            cm.LineItems.Add(new CreditMemoLineItem
            {
                LineNumber = line++,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                Amount = li.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.CreditMemos.Add(cm);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created credit memo {CreditMemoNumber} from invoice {InvoiceId}", cm.CreditMemoNumber, invoiceId);
        return cm;
    }

    public async Task<string> GenerateCreditMemoNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = "CM";
        var now = DateTime.UtcNow;
        var count = await _context.CreditMemos.CountAsync(cancellationToken);
        return $"{prefix}-{now:yyyyMMdd}-{count + 1:D4}";
    }

    public async Task<CreditMemo> ApplyAsync(int creditMemoId, int invoiceId, CancellationToken cancellationToken = default)
    {
        var cm = await _context.CreditMemos.FindAsync(new object[] { creditMemoId }, cancellationToken);
        if (cm == null || cm.IsDeleted) throw new InvalidOperationException($"Credit memo {creditMemoId} not found");

        var invoice = await _context.Invoices.FindAsync(new object[] { invoiceId }, cancellationToken);
        if (invoice == null || invoice.IsDeleted) throw new InvalidOperationException($"Invoice {invoiceId} not found");

        // simplistic application: reduce invoice balance and mark credit memo applied
        var applyAmount = Math.Min(Math.Abs(cm.BalanceRemaining), invoice.TotalAmount - (invoice.Payments?.Sum(p => p.Amount) ?? 0));
        if (applyAmount <= 0) throw new InvalidOperationException("Nothing to apply");

        cm.AmountApplied += applyAmount;
        cm.Status = CreditMemoStatus.PartiallyApplied;
        if (Math.Abs(cm.BalanceRemaining) < 0.01m) cm.Status = CreditMemoStatus.Applied;
        cm.AppliedDate = DateTime.UtcNow;

        invoice.UpdatedAt = DateTime.UtcNow;
        _context.CreditMemos.Update(cm);
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Applied {Amount} from credit memo {CreditMemoId} to invoice {InvoiceId}", applyAmount, creditMemoId, invoiceId);
        return cm;
    }

    public async Task<CreditMemo> UnapplyAsync(int creditMemoId, CancellationToken cancellationToken = default)
    {
        var cm = await _context.CreditMemos.FindAsync(new object[] { creditMemoId }, cancellationToken);
        if (cm == null || cm.IsDeleted) throw new InvalidOperationException($"Credit memo {creditMemoId} not found");

        cm.AmountApplied = 0m;
        cm.Status = CreditMemoStatus.Approved;
        cm.UpdatedAt = DateTime.UtcNow;

        _context.CreditMemos.Update(cm);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Unapplied credit memo {CreditMemoId}", creditMemoId);
        return cm;
    }

    public async Task<CreditMemo> RefundAsync(int creditMemoId, CancellationToken cancellationToken = default)
    {
        var cm = await _context.CreditMemos.FindAsync(new object[] { creditMemoId }, cancellationToken);
        if (cm == null || cm.IsDeleted) throw new InvalidOperationException($"Credit memo {creditMemoId} not found");

        cm.Status = CreditMemoStatus.Refunded;
        cm.RefundedDate = DateTime.UtcNow;
        cm.UpdatedAt = DateTime.UtcNow;

        _context.CreditMemos.Update(cm);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refunded credit memo {CreditMemoId}", creditMemoId);
        return cm;
    }

    public async Task<CreditMemoLineItem> AddLineItemAsync(int creditMemoId, CreditMemoLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var cm = await _context.CreditMemos.FindAsync(new object[] { creditMemoId }, cancellationToken);
        if (cm == null || cm.IsDeleted) throw new InvalidOperationException($"Credit memo {creditMemoId} not found");

        lineItem.CreatedAt = DateTime.UtcNow;
        lineItem.UpdatedAt = DateTime.UtcNow;
        lineItem.CreditMemoId = creditMemoId;
        _context.CreditMemoLineItems.Add(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added line item {LineItemId} to credit memo {CreditMemoId}", lineItem.Id, creditMemoId);
        return lineItem;
    }

    public async Task<CreditMemoLineItem> UpdateLineItemAsync(CreditMemoLineItem lineItem, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CreditMemoLineItems.FindAsync(new object[] { lineItem.Id }, cancellationToken);
        if (existing == null || existing.IsDeleted) throw new InvalidOperationException($"Credit memo line item {lineItem.Id} not found");

        lineItem.UpdatedAt = DateTime.UtcNow;
        _context.CreditMemoLineItems.Update(lineItem);
        await _context.SaveChangesAsync(cancellationToken);

        return lineItem;
    }

    public async Task<bool> RemoveLineItemAsync(int lineItemId, CancellationToken cancellationToken = default)
    {
        var li = await _context.CreditMemoLineItems.FindAsync(new object[] { lineItemId }, cancellationToken);
        if (li == null || li.IsDeleted) return false;

        li.IsDeleted = true;
        li.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<CreditMemoLineItem>> GetLineItemsAsync(int creditMemoId, CancellationToken cancellationToken = default)
    {
        return await _context.CreditMemoLineItems
            .Where(li => li.CreditMemoId == creditMemoId && !li.IsDeleted)
            .OrderBy(li => li.LineNumber)
            .ToListAsync(cancellationToken);
    }
}

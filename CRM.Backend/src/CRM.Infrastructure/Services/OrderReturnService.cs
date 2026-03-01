// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IOrderReturnService for order return management.
/// Handles the full lifecycle of product returns and refunds.
/// </summary>
public class OrderReturnService : IOrderReturnService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<OrderReturnService> _logger;
    private const string OrderReturnNotFoundMessage = "Order return {0} not found";

    public OrderReturnService(ICrmDbContext context, ILogger<OrderReturnService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    public async Task<IEnumerable<OrderReturn>> GetAllAsync(
        int? orderId = null,
        int? accountId = null,
        OrderReturnStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.OrderReturns
            .Include(r => r.Order)
            .Include(r => r.Account)
            .Include(r => r.InitiatedBy)
            .Where(r => !r.IsDeleted);

        if (orderId.HasValue)
        {
            query = query.Where(r => r.OrderId == orderId.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(r => r.AccountId == accountId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<OrderReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.OrderReturns
            .Include(r => r.Order)
            .Include(r => r.Account)
            .Include(r => r.InitiatedBy)
            .Include(r => r.ProcessedBy)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<OrderReturn?> GetByReturnNumberAsync(string returnNumber, CancellationToken cancellationToken = default)
    {
        return await _context.OrderReturns
            .Include(r => r.Order)
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber && !r.IsDeleted, cancellationToken);
    }

    public async Task<OrderReturn> CreateAsync(CreateOrderReturnDto dto, int initiatedById, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {dto.OrderId} not found");
        }

        var orderReturn = new OrderReturn
        {
            ReturnNumber = await GenerateReturnNumberAsync(cancellationToken),
            OrderId = dto.OrderId,
            AccountId = order.AccountId,
            InitiatedById = initiatedById,
            Status = OrderReturnStatus.Pending,
            Reason = (OrderReturnReason)dto.Reason,
            ReasonDescription = dto.ReasonDescription,
            Notes = dto.Notes,
            OriginalAmount = order.TotalAmount,
            RefundAmount = dto.RefundAmount,
            RestockingFee = dto.RestockingFee,
            ShippingRefund = dto.ShippingRefund,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.LineItems != null && dto.LineItems.Any())
        {
            orderReturn.LineItemsJson = JsonSerializer.Serialize(dto.LineItems);
        }

        _context.OrderReturns.Add(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return created: {ReturnNumber} for order {OrderId}", 
            orderReturn.ReturnNumber, dto.OrderId);

        return orderReturn;
    }

    public async Task<OrderReturn> UpdateAsync(int id, UpdateOrderReturnDto dto, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = (OrderReturnStatus)dto.Status;
        
        if (!string.IsNullOrEmpty(dto.Notes))
        {
            orderReturn.Notes = dto.Notes;
        }
        if (dto.RefundAmount.HasValue)
        {
            orderReturn.RefundAmount = dto.RefundAmount.Value;
        }
        if (dto.RestockingFee.HasValue)
        {
            orderReturn.RestockingFee = dto.RestockingFee.Value;
        }
        if (dto.ShippingRefund.HasValue)
        {
            orderReturn.ShippingRefund = dto.ShippingRefund.Value;
        }
        if (!string.IsNullOrEmpty(dto.ReturnTrackingNumber))
        {
            orderReturn.ReturnTrackingNumber = dto.ReturnTrackingNumber;
        }
        if (!string.IsNullOrEmpty(dto.ReturnCarrier))
        {
            orderReturn.ReturnCarrier = dto.ReturnCarrier;
        }
        if (!string.IsNullOrEmpty(dto.RefundTransactionId))
        {
            orderReturn.RefundTransactionId = dto.RefundTransactionId;
        }

        orderReturn.UpdatedAt = DateTime.UtcNow;
        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return updated: {Id}", id);
        return orderReturn;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderReturn = await _context.OrderReturns.FindAsync(new object[] { id }, cancellationToken);
        if (orderReturn == null)
        {
            return false;
        }

        orderReturn.IsDeleted = true;
        orderReturn.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return deleted: {Id}", id);
        return true;
    }

    #endregion

    #region Workflow Operations

    public async Task<OrderReturn> ApproveAsync(int id, int approvedById, string? notes = null, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        if (orderReturn.Status != OrderReturnStatus.Pending)
        {
            throw new InvalidOperationException($"Order return {id} cannot be approved - current status is {orderReturn.Status}");
        }

        orderReturn.Status = OrderReturnStatus.Approved;
        orderReturn.ProcessedById = approvedById;
        orderReturn.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
        {
            orderReturn.Notes = (orderReturn.Notes ?? "") + $"\nApproval note: {notes}";
        }
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return approved: {Id} by user {UserId}", id, approvedById);
        return orderReturn;
    }

    public async Task<OrderReturn> RejectAsync(int id, int rejectedById, string reason, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = OrderReturnStatus.Rejected;
        orderReturn.ProcessedById = rejectedById;
        orderReturn.Notes = (orderReturn.Notes ?? "") + $"\nRejection reason: {reason}";
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return rejected: {Id} by user {UserId}", id, rejectedById);
        return orderReturn;
    }

    public async Task<OrderReturn> MarkReceivedAsync(int id, string? trackingNumber = null, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = OrderReturnStatus.Received;
        orderReturn.ReceivedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(trackingNumber))
        {
            orderReturn.ReturnTrackingNumber = trackingNumber;
        }
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return items received: {Id}", id);
        return orderReturn;
    }

    public async Task<OrderReturn> ProcessRefundAsync(int id, string transactionId, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = OrderReturnStatus.Refunded;
        orderReturn.RefundedAt = DateTime.UtcNow;
        orderReturn.RefundTransactionId = transactionId;
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return refund processed: {Id}, Transaction: {TransactionId}", id, transactionId);
        return orderReturn;
    }

    public async Task<OrderReturn> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = OrderReturnStatus.Completed;
        orderReturn.CompletedAt = DateTime.UtcNow;
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return completed: {Id}", id);
        return orderReturn;
    }

    public async Task<OrderReturn> CancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var orderReturn = await GetByIdAsync(id, cancellationToken);
        if (orderReturn == null)
        {
            throw new InvalidOperationException(string.Format(OrderReturnNotFoundMessage, id));
        }

        orderReturn.Status = OrderReturnStatus.Cancelled;
        orderReturn.Notes = (orderReturn.Notes ?? "") + $"\nCancellation reason: {reason}";
        orderReturn.UpdatedAt = DateTime.UtcNow;

        _context.OrderReturns.Update(orderReturn);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order return cancelled: {Id}", id);
        return orderReturn;
    }

    #endregion

    #region Queries

    public async Task<IEnumerable<OrderReturn>> GetByOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderReturns
            .Include(r => r.Order)
            .Where(r => r.OrderId == orderId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderReturn>> GetByAccountAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderReturns
            .Include(r => r.Order)
            .Where(r => r.AccountId == accountId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderReturn>> GetPendingReturnsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrderReturns
            .Include(r => r.Order)
            .Include(r => r.Account)
            .Where(r => !r.IsDeleted && r.Status == OrderReturnStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderReturnStatisticsDto> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.OrderReturns.Where(r => !r.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= toDate.Value);
        }

        var returns = await query.ToListAsync(cancellationToken);
        var totalOrders = await _context.Orders.CountAsync(o => !o.IsDeleted, cancellationToken);

        return new OrderReturnStatisticsDto
        {
            TotalReturns = returns.Count,
            PendingReturns = returns.Count(r => r.Status == OrderReturnStatus.Pending),
            ApprovedReturns = returns.Count(r => r.Status == OrderReturnStatus.Approved),
            CompletedReturns = returns.Count(r => r.Status == OrderReturnStatus.Completed),
            RejectedReturns = returns.Count(r => r.Status == OrderReturnStatus.Rejected),
            TotalRefundedAmount = returns.Where(r => r.Status == OrderReturnStatus.Refunded || r.Status == OrderReturnStatus.Completed).Sum(r => r.NetRefundAmount),
            AverageRefundAmount = returns.Any() ? returns.Average(r => r.RefundAmount) : 0,
            ReturnRate = totalOrders > 0 ? (double)returns.Count / totalOrders * 100 : 0,
            ReturnsByReason = returns.GroupBy(r => (int)r.Reason).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<string> GenerateReturnNumberAsync(CancellationToken cancellationToken = default)
    {
        var prefix = $"RET-{DateTime.UtcNow:yyMM}-";
        var lastReturn = await _context.OrderReturns
            .Where(r => r.ReturnNumber.StartsWith(prefix))
            .OrderByDescending(r => r.ReturnNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastReturn != null)
        {
            var lastNum = lastReturn.ReturnNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastNum, out var num))
            {
                sequence = num + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }

    #endregion

    #region Credit Notes

    /// <inheritdoc />
    public async Task<CreditNoteDto> IssueCreditNoteAsync(
        int returnId,
        CancellationToken cancellationToken = default)
    {
        var orderReturn = await _context.OrderReturns
            .FirstOrDefaultAsync(r => r.Id == returnId && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException(string.Format(OrderReturnNotFoundMessage, returnId));

        if (orderReturn.Status != OrderReturnStatus.Approved)
        {
            throw new InvalidOperationException(
                $"Cannot issue a credit note for return {returnId}: status is '{orderReturn.Status}' (must be Approved).");
        }

        if (orderReturn.CreditNoteId.HasValue)
        {
            throw new InvalidOperationException(
                $"Credit note already issued for return {returnId} (CreditNoteId={orderReturn.CreditNoteId}).");
        }

        // Persist a placeholder first to obtain the Id for number generation.
        var creditNote = new CreditNote
        {
            OrderId = orderReturn.OrderId,
            Amount = orderReturn.RefundAmount,
            Reason = orderReturn.Notes ?? "Order return refund",
            IssuedAt = DateTime.UtcNow,
            IsApplied = false,
            CreditNoteNumber = string.Empty, // placeholder — replaced below
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CreditNotes.Add(creditNote);
        await _context.SaveChangesAsync(cancellationToken);

        // Now that we have the Id, generate the human-readable number.
        creditNote.CreditNoteNumber = $"CN-{DateTime.UtcNow.Year}-{creditNote.Id:D5}";
        orderReturn.CreditNoteId = creditNote.Id;
        orderReturn.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreditNote {CreditNoteNumber} (Id={CreditNoteId}) issued for OrderReturn {ReturnId}.",
            creditNote.CreditNoteNumber, creditNote.Id, returnId);

        return MapToCreditNoteDto(creditNote);
    }

    private static CreditNoteDto MapToCreditNoteDto(CreditNote cn) =>
        new()
        {
            Id = cn.Id,
            CreditNoteNumber = cn.CreditNoteNumber,
            OrderId = cn.OrderId,
            InvoiceId = cn.InvoiceId,
            Amount = cn.Amount,
            Reason = cn.Reason,
            IssuedAt = cn.IssuedAt,
            IsApplied = cn.IsApplied,
            AppliedAt = cn.AppliedAt,
            CreatedAt = cn.CreatedAt,
            UpdatedAt = cn.UpdatedAt.GetValueOrDefault(cn.CreatedAt)
        };

    #endregion
}

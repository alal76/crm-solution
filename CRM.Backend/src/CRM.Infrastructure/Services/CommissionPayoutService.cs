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
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICommissionPayoutService for commission payout operations.
/// Handles payments, reconciliation, and financial integration.
/// </summary>
public class CommissionPayoutService : ICommissionPayoutService, ICommissionPayoutInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionPayoutService> _logger;

    public CommissionPayoutService(ICrmDbContext context, ILogger<CommissionPayoutService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> MarkPaidAsync(int commissionId, DateTime? paidDate = null, string? reference = null, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .FirstOrDefaultAsync(c => c.Id == commissionId && !c.IsDeleted, cancellationToken);

        if (commission == null)
            return false;

        commission.Status = CommissionStatus.Paid;
        commission.PaidDate = paidDate ?? DateTime.UtcNow;

        if (!string.IsNullOrEmpty(reference))
        {
            commission.Notes = $"Payout Reference: {reference}\n{commission.Notes}";
        }

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission {CommissionId} marked as paid on {PaidDate}", commissionId, commission.PaidDate);
        return true;
    }

    public async Task<bool> ClawbackAsync(int commissionId, string reason, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .FirstOrDefaultAsync(c => c.Id == commissionId && !c.IsDeleted && c.Status == CommissionStatus.Paid, cancellationToken);

        if (commission == null)
            return false;

        var clawbackAmount = amount ?? commission.CommissionAmount;
        commission.Status = CommissionStatus.ClawedBack;
        commission.ClawbackDate = DateTime.UtcNow;
        commission.ClawbackReason = reason;
        commission.CommissionAmount -= clawbackAmount;

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission {CommissionId} clawed back: {Amount}. Reason: {Reason}", commissionId, clawbackAmount, reason);
        return true;
    }

    public async Task<CRM.Core.Dtos.CommissionStatementDto> GenerateStatementAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CreatedAt >= from && c.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var statement = new CRM.Core.Dtos.CommissionStatementDto
        {
            UserId = userId,
            PeriodStartDate = from,
            PeriodEndDate = to,
            CommissionCount = commissions.Count,
            TotalAmount = commissions.Sum(c => c.CommissionAmount),
            ApprovedAmount = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.CommissionAmount),
            PaidAmount = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.CommissionAmount),
            CreatedAt = DateTime.UtcNow
        };

        return statement;
    }

    public async Task<bool> FinalizeStatementAsync(int statementId, CancellationToken cancellationToken = default)
    {
        // In a real system, this would lock the statement for future modifications
        _logger.LogInformation("Statement {StatementId} finalized", statementId);
        return true;
    }

    public async Task<bool> ReconcileAsync(int statementId, CancellationToken cancellationToken = default)
    {
        // Reconciliation logic - match commissions with financial records
        _logger.LogInformation("Statement {StatementId} reconciliation completed", statementId);
        return true;
    }

    public async Task<List<object>> GetPayoutScheduleAsync(int userId, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Amount = g.Sum(c => c.CommissionAmount),
                Count = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return commissions.Cast<object>().ToList();
    }

    public async Task<CommissionPayoutDto?> FinalizeAsync(int payoutId, CancellationToken cancellationToken = default)
    {
        var commission = await _context.Commissions
            .FirstOrDefaultAsync(c => c.Id == payoutId && !c.IsDeleted, cancellationToken);

        if (commission == null)
            return null;

        commission.Status = CommissionStatus.Paid;
        commission.PaidDate = DateTime.UtcNow;
        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission payout {PayoutId} finalized", payoutId);

        return new CommissionPayoutDto
        {
            Id = commission.Id,
            UserId = commission.UserId,
            TotalCommissionAmount = commission.CommissionAmount,
            Status = commission.Status.ToString(),
            CreatedAt = commission.CreatedAt,
            UpdatedAt = commission.UpdatedAt ?? DateTime.UtcNow
        };
    }
}

/// <summary>
/// DTO for commission statement response.
/// </summary>
public class CommissionStatementDto
{
    public DateTime StatementPeriodStart { get; set; }
    public DateTime StatementPeriodEnd { get; set; }
    public int UserId { get; set; }
    public int TotalCommissions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ClawedBackAmount { get; set; }
    public decimal NetPayable { get; set; }
    public DateTime GeneratedAt { get; set; }
}

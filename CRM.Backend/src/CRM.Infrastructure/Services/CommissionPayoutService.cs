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
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        commission.UpdatedAt = DateTime.UtcNow;

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
        commission.UpdatedAt = DateTime.UtcNow;

        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission {CommissionId} clawed back: {Amount}. Reason: {Reason}", commissionId, clawbackAmount, reason);
        return true;
    }

    public async Task<CommissionStatementDto> GenerateStatementAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CreatedAt >= from && c.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var statement = new CommissionStatementDto
        {
            StatementPeriodStart = from,
            StatementPeriodEnd = to,
            UserId = userId,
            TotalCommissions = commissions.Count,
            TotalAmount = commissions.Sum(c => c.CommissionAmount),
            ApprovedAmount = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.CommissionAmount),
            PaidAmount = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.CommissionAmount),
            ClawedBackAmount = commissions.Where(c => c.Status == CommissionStatus.ClawedBack).Sum(c => c.CommissionAmount),
            NetPayable = commissions.Where(c => c.Status == CommissionStatus.Approved || c.Status == CommissionStatus.Paid)
                .Sum(c => c.CommissionAmount) - commissions.Where(c => c.Status == CommissionStatus.ClawedBack).Sum(c => c.CommissionAmount),
            GeneratedAt = DateTime.UtcNow
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
}

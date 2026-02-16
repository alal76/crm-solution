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
/// Implementation of ICommissionCalculationService for complex commission calculations.
/// Handles tier-based calculations, accelerators, and split commissions.
/// </summary>
public class CommissionCalculationService : ICommissionCalculationService, ICommissionCalculationInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionCalculationService> _logger;

    public CommissionCalculationService(ICrmDbContext context, ILogger<CommissionCalculationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommissionCalculationResultDto> CalculateDealAsync(int opportunityId, int? planId = null, CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");

        var plan = planId.HasValue
            ? await _context.CommissionPlans.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken)
            : await GetDefaultPlanAsync(opportunity.OwnerId, cancellationToken);

        if (plan == null)
            throw new InvalidOperationException("No commission plan available for calculation");

        var result = new CommissionCalculationResultDto
        {
            OpportunityId = opportunityId,
            Amount = opportunity.Amount ?? 0,
            CommissionPlanId = plan.Id,
            BaseCommissionRate = plan.BaseRate,
            BaseCommissionAmount = (opportunity.Amount ?? 0) * plan.BaseRate / 100,
            CreatedAt = DateTime.UtcNow
        };

        // Apply tier-based rate
        var tierRate = await GetApplicableTierRateAsync(plan.Id, result.Amount, cancellationToken);
        if (tierRate > 0 && tierRate != plan.BaseRate)
        {
            result.TierCommissionAmount = result.Amount * tierRate / 100;
            result.TierCommissionRate = tierRate;
            result.FinalCommissionAmount = result.TierCommissionAmount;
        }
        else
        {
            result.FinalCommissionAmount = result.BaseCommissionAmount;
        }

        _logger.LogInformation("Commission calculated for opportunity {OpportunityId}: {Amount}", opportunityId, result.FinalCommissionAmount);
        return result;
    }

    public async Task<CommissionCalculationResultDto> CalculateOrderAsync(int orderId, int? planId = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        var plan = planId.HasValue
            ? await _context.CommissionPlans.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken)
            : await GetDefaultPlanAsync(order.CreatedById, cancellationToken);

        if (plan == null)
            throw new InvalidOperationException("No commission plan available for calculation");

        var result = new CommissionCalculationResultDto
        {
            OrderId = orderId,
            Amount = order.TotalAmount ?? 0,
            CommissionPlanId = plan.Id,
            BaseCommissionRate = plan.BaseRate,
            BaseCommissionAmount = (order.TotalAmount ?? 0) * plan.BaseRate / 100,
            CreatedAt = DateTime.UtcNow
        };

        result.FinalCommissionAmount = result.BaseCommissionAmount;

        _logger.LogInformation("Commission calculated for order {OrderId}: {Amount}", orderId, result.FinalCommissionAmount);
        return result;
    }

    public async Task<CommissionStatisticsDto> CalculatePeriodAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CreatedAt >= from && c.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var stats = new CommissionStatisticsDto
        {
            UserId = userId,
            PeriodStart = from,
            PeriodEnd = to,
            TotalCommissions = commissions.Count,
            TotalAmount = commissions.Sum(c => c.CommissionAmount),
            AverageAmount = commissions.Any() ? commissions.Average(c => c.CommissionAmount) : 0,
            ApprovedAmount = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.CommissionAmount),
            PaidAmount = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.CommissionAmount),
            PendingAmount = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.CommissionAmount)
        };

        return stats;
    }

    public async Task<decimal> ApplyTierAsync(int planId, decimal amount, CancellationToken cancellationToken = default)
    {
        var rate = await GetApplicableTierRateAsync(planId, amount, cancellationToken);
        return amount * rate / 100;
    }

    public async Task<decimal> ApplyAcceleratorAsync(int planId, decimal baseAmount, decimal achievementPercent, CancellationToken cancellationToken = default)
    {
        // Accelerator: if achievement > 100%, add bonus rate
        if (achievementPercent <= 100)
            return baseAmount;

        var bonusPercentage = (achievementPercent - 100) * 0.1m; // 0.1% additional commission per 1% over target
        var bonusAmount = baseAmount * bonusPercentage / 100;

        _logger.LogInformation("Accelerator bonus applied: {Achievement}% -> +{Bonus}%", achievementPercent, bonusPercentage);
        return baseAmount + bonusAmount;
    }

    public async Task<bool> ValidateAsync(CommissionCalculationResultDto calculation, CancellationToken cancellationToken = default)
    {
        if (calculation == null)
            return await Task.FromResult(false);

        if (calculation.FinalCommissionAmount < 0)
            return await Task.FromResult(false);

        if (calculation.CommissionPlanId <= 0)
            return await Task.FromResult(false);

        var plan = await _context.CommissionPlans
            .FirstOrDefaultAsync(p => p.Id == calculation.CommissionPlanId && !p.IsDeleted, cancellationToken);

        return await Task.FromResult(plan != null);
    }

    private async Task<CommissionPlan?> GetDefaultPlanAsync(int userId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.CommissionPlanAssignments
            .Include(a => a.CommissionPlan)
            .FirstOrDefaultAsync(a => a.UserId == userId && !a.IsDeleted && a.EffectiveDate <= DateTime.UtcNow, cancellationToken);

        return assignment?.CommissionPlan;
    }

    private async Task<decimal> GetApplicableTierRateAsync(int planId, decimal amount, CancellationToken cancellationToken = default)
    {
        var tier = await _context.CommissionTiers
            .Where(t => t.CommissionPlanId == planId && !t.IsDeleted
                && t.MinimumAmount <= amount
                && (t.MaximumAmount == null || t.MaximumAmount >= amount))
            .OrderBy(t => t.Sequence)
            .FirstOrDefaultAsync(cancellationToken);

        return tier?.CommissionRate ?? 0;
    }
}

/// <summary>
/// DTO for commission calculation result.
/// </summary>
public class CommissionCalculationResultDto
{
    public int? OpportunityId { get; set; }
    public int? OrderId { get; set; }
    public decimal Amount { get; set; }
    public int CommissionPlanId { get; set; }
    public decimal BaseCommissionRate { get; set; }
    public decimal BaseCommissionAmount { get; set; }
    public decimal? TierCommissionRate { get; set; }
    public decimal? TierCommissionAmount { get; set; }
    public decimal FinalCommissionAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for commission statistics.
/// </summary>
public class CommissionStatisticsDto
{
    public int UserId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalCommissions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}

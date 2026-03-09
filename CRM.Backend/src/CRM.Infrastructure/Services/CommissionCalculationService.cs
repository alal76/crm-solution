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
        {
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");
        }

        var plan = planId.HasValue
            ? await _context.CommissionPlans.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken)
            : await GetDefaultPlanAsync(opportunity.SalesOwnerId ?? 0, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException("No commission plan available for calculation");
        }

        var amount = opportunity.Amount;
        var baseCommissionAmount = amount * plan.BaseRate / 100m;
        var tierRate = await GetApplicableTierRateAsync(plan.Id, amount, cancellationToken);
        var tierCommissionAmount = tierRate > 0 && tierRate != plan.BaseRate ? (decimal?)(amount * tierRate / 100m) : null;
        var finalAmount = tierCommissionAmount ?? baseCommissionAmount;

        var result = new CRM.Core.Dtos.CommissionCalculationResultDto
        {
            UserId = opportunity.SalesOwnerId ?? 0,
            OpportunityId = opportunityId,
            Amount = amount,
            CommissionPlanId = plan.Id,
            PlanName = plan.Name,
            DealAmount = amount,
            BaseCommissionAmount = baseCommissionAmount,
            BaseCommissionRate = plan.BaseRate,
            TierCommissionAmount = tierCommissionAmount,
            TierCommissionRate = tierRate > 0 ? tierRate : null,
            FinalCommissionAmount = finalAmount,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Commission calculated for opportunity {OpportunityId}: {Amount}", opportunityId, result.FinalCommissionAmount);
        return result;
    }

    public async Task<CommissionCalculationResultDto> CalculateOrderAsync(int orderId, int? planId = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var plan = planId.HasValue
            ? await _context.CommissionPlans.FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken)
            : await GetDefaultPlanAsync(order.UserId ?? 0, cancellationToken);

        if (plan == null)
        {
            throw new InvalidOperationException("No commission plan available for calculation");
        }

        var amount = order.TotalAmount;
        var baseCommissionAmount = amount * plan.BaseRate / 100;

        var result = new CRM.Core.Dtos.CommissionCalculationResultDto
        {
            UserId = order.UserId ?? 0,
            OrderId = orderId,
            Amount = amount,
            CommissionPlanId = plan.Id,
            PlanName = plan.Name,
            DealAmount = amount,
            BaseCommissionAmount = baseCommissionAmount,
            BaseCommissionRate = plan.BaseRate,
            FinalCommissionAmount = baseCommissionAmount,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Commission calculated for order {OrderId}: {Amount}", orderId, result.FinalCommissionAmount);
        return result;
    }

    public async Task<CommissionStatisticsDto> CalculatePeriodAsync(int userId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var commissions = await _context.Commissions
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CreatedAt >= from && c.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var approvedCommissions = commissions.Where(c => c.Status == CommissionStatus.Approved).ToList();
        var paidCommissions = commissions.Where(c => c.Status == CommissionStatus.Paid).ToList();
        var pendingCommissions = commissions.Where(c => c.Status == CommissionStatus.Pending).ToList();
        var clawedBackCommissions = commissions.Where(c => c.Status == CommissionStatus.Rejected).ToList();

        var stats = new CommissionStatisticsDto
        {
            TotalCommissions = commissions.Count,
            TotalAmount = commissions.Sum(c => c.CommissionAmount),
            ApprovedAmount = approvedCommissions.Sum(c => c.CommissionAmount),
            PaidAmount = paidCommissions.Sum(c => c.CommissionAmount),
            AverageCommission = commissions.Any() ? commissions.Average(c => c.CommissionAmount) : 0,
            MaxCommission = commissions.Any() ? commissions.Max(c => c.CommissionAmount) : 0,
            MinCommission = commissions.Any() ? commissions.Min(c => c.CommissionAmount) : 0,
            PendingCount = pendingCommissions.Count,
            ApprovedCount = approvedCommissions.Count,
            PaidCount = paidCommissions.Count,
            ClawedBackCount = clawedBackCommissions.Count,
            StartDate = from,
            EndDate = to
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
        {
            return baseAmount;
        }

        var bonusPercentage = (achievementPercent - 100) * 0.1m; // 0.1% additional commission per 1% over target
        var bonusAmount = baseAmount * bonusPercentage / 100;

        _logger.LogInformation("Accelerator bonus applied: {Achievement}% -> +{Bonus}%", achievementPercent, bonusPercentage);
        return baseAmount + bonusAmount;
    }

    public async Task<bool> ValidateAsync(CommissionCalculationResultDto calculation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (calculation == null)
            {
                _logger.LogWarning("ValidateAsync: Calculation is null");
                return false;
            }

            if (calculation.Amount < 0)
            {
                _logger.LogWarning("ValidateAsync: Amount {Amount} is negative", calculation.Amount);
                return false;
            }

            if (calculation.CommissionPlanId <= 0)
            {
                _logger.LogWarning("ValidateAsync: Invalid PlanId {PlanId}", calculation.CommissionPlanId);
                return false;
            }

            // Verify commission plan exists
            var plan = await _context.CommissionPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == calculation.CommissionPlanId && !p.IsDeleted, cancellationToken);

            if (plan == null)
            {
                _logger.LogWarning("ValidateAsync: Plan {PlanId} not found", calculation.CommissionPlanId);
                return false;
            }

            // Verify final amount is greater than or equal to base amount
            if (calculation.FinalCommissionAmount < calculation.BaseCommissionAmount)
            {
                _logger.LogWarning("ValidateAsync: Final amount {Final} less than base {Base}",
                    calculation.FinalCommissionAmount, calculation.BaseCommissionAmount);
                return false;
            }

            _logger.LogInformation("ValidateAsync: Commission calculation valid");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateAsync: Error during validation");
            return false;
        }
    }

    public async Task<CommissionCalculationResultDto> CalculateForDealAsync(CommissionDealCalculationDto dto, CancellationToken cancellationToken = default)
    {
        // Delegate to existing method if OpportunityId is set, otherwise create basic result
        if (dto.OpportunityId > 0)
        {
            return await CalculateDealAsync(dto.OpportunityId, null, cancellationToken);
        }

        // Return basic result with provided data
        return new CommissionCalculationResultDto
        {
            UserId = dto.UserId,
            DealAmount = dto.DealAmount,
            CommissionRate = dto.CommissionRate,
            TierName = dto.CommissionTier,
            FinalCommissionAmount = dto.Commission,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<CommissionCalculationResultDto> CalculateForOrderAsync(CommissionOrderCalculationDto dto, CancellationToken cancellationToken = default)
    {
        // Delegate to existing method if OrderId is set, otherwise create basic result
        if (dto.OrderId > 0)
        {
            return await CalculateOrderAsync(dto.OrderId, null, cancellationToken);
        }

        // Return basic result with provided data
        return new CommissionCalculationResultDto
        {
            UserId = dto.UserId,
            OrderId = dto.OrderId,
            DealAmount = dto.OrderAmount,
            CommissionRate = 0,
            FinalCommissionAmount = dto.Commission,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<CommissionPeriodCalculationResultDto> CalculateForPeriodAsync(CommissionPeriodCalculationDto dto, CancellationToken cancellationToken = default)
    {
        // Calculate total deal amount and commission for the period
        var stats = await CalculatePeriodAsync(dto.UserId, dto.StartDate, dto.EndDate, cancellationToken);

        return new CommissionPeriodCalculationResultDto
        {
            UserId = dto.UserId,
            UserName = string.Empty,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDealAmount = stats?.TotalAmount ?? 0,
            TotalCommission = stats?.ApprovedAmount ?? 0,
            DealCount = stats?.ApprovedCount ?? 0
        };
    }

    public async Task<CommissionValidationResultDto> ValidateAsync(CommissionCalculationValidationDto validation, CancellationToken cancellationToken = default)
    {
        var result = new CommissionValidationResultDto
        {
            IsValid = true,
            CalculatedCommission = 0,
            ValidationErrors = new()
        };

        // Validate required fields
        if (validation.RuleId <= 0)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("Rule ID is required and must be greater than 0");
        }

        if (validation.DealAmount <= 0)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("Deal amount must be greater than 0");
        }

        if (validation.UserId <= 0)
        {
            result.IsValid = false;
            result.ValidationErrors.Add("User ID is required and must be greater than 0");
        }

        // If basic validation passed, check if rule exists
        if (result.IsValid && validation.RuleId > 0)
        {
            var rule = await _context.CommissionRules
                .FirstOrDefaultAsync(r => r.Id == validation.RuleId && !r.IsDeleted, cancellationToken);

            if (rule == null)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Commission rule {validation.RuleId} not found");
            }
            else
            {
                // Calculate simple commission
                result.CalculatedCommission = validation.DealAmount * rule.BaseRate / 100;
            }
        }

        result.ValidationMessage = result.IsValid ? "Commission calculation is valid" : "Commission calculation has validation errors";
        return result;
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

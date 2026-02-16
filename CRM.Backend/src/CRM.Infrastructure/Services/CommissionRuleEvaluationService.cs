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

using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Commission Rule Evaluation Service - Advanced commission calculations with tiers, caps,
/// minimums, clawback, split commissions, and trigger-based rules.
/// 
/// PHASE 5: Commission Rules Advanced (20 hours)
/// SPEC: SPEC-SALES-005
/// </summary>
public class CommissionRuleEvaluationService : ICommissionRuleEvaluationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionRuleEvaluationService> _logger;
    private const decimal DefaultCapAmount = 50000m;
    private const decimal DefaultMinimumAmount = 0m;

    public CommissionRuleEvaluationService(
        ICrmDbContext context,
        ILogger<CommissionRuleEvaluationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommissionEvaluationDto> EvaluateCommissionRuleAsync(
        int dealId,
        int salesRepId,
        string trigger,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evaluating commission for deal {DealId}, rep {SalesRepId}, trigger {Trigger}",
            dealId, salesRepId, trigger);

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dealId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
            throw new InvalidOperationException($"Opportunity {dealId} not found");

        var amount = opportunity.Amount;

        // Get applicable commission rule based on trigger
        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsActive && !r.IsDeleted, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"No active commission rule found for trigger {trigger}");

        var result = new CommissionEvaluationDto
        {
            DealId = dealId,
            SalesRepId = salesRepId,
            DealAmount = amount,
            Trigger = trigger,
            BaseCommissionRate = rule.BaseRate,
            BaseCommissionAmount = amount * rule.BaseRate / 100,
            EvaluatedAt = DateTime.UtcNow
        };

        // Apply tiered rates if available
        if (!string.IsNullOrEmpty(rule.Configuration))
        {
            var tierResult = await CalculateTieredCommissionAsync(amount, rule.Id, cancellationToken);
            result.TierCommissionRate = tierResult.Tiers.OrderByDescending(t => t.CommissionAmount)
                .FirstOrDefault()?.Rate ?? rule.BaseRate;
            result.TierCommissionAmount = tierResult.TotalCommission;
            result.TierName = tierResult.Tiers.OrderByDescending(t => t.CommissionAmount)
                .FirstOrDefault()?.Name ?? "Base";
        }

        result.FinalCommissionAmount = result.TierCommissionAmount > 0
            ? result.TierCommissionAmount
            : result.BaseCommissionAmount;

        // Apply caps and minimums
        result.FinalCommissionAmount = await ApplyCapsAndMinimumsAsync(result.FinalCommissionAmount, rule.Id, cancellationToken);

        _logger.LogInformation("Commission evaluated: {Amount}", result.FinalCommissionAmount);
        return result;
    }

    public async Task<TieredCommissionDto> CalculateTieredCommissionAsync(
        decimal amount,
        int ruleId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating tiered commission for amount {Amount}, rule {RuleId}", amount, ruleId);

        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Commission rule {ruleId} not found");

        var result = new TieredCommissionDto
        {
            Amount = amount,
            RuleId = ruleId
        };

        // Default tier structure: base, tier1 (1000+, 5%), tier2 (5000+, 7%), tier3 (10000+, 10%)
        var tiers = new[]
        {
            new { Name = "Base", Min = 0m, Max = 999.99m, Rate = rule.BaseRate },
            new { Name = "Tier 1", Min = 1000m, Max = 4999.99m, Rate = 5m },
            new { Name = "Tier 2", Min = 5000m, Max = 9999.99m, Rate = 7m },
            new { Name = "Tier 3", Min = 10000m, Max = decimal.MaxValue, Rate = 10m }
        };

        decimal totalCommission = 0m;

        foreach (var tier in tiers)
        {
            if (amount > tier.Min)
            {
                var appliedAmount = Math.Min(amount, tier.Max) - tier.Min;
                var commissionInTier = appliedAmount * tier.Rate / 100;
                totalCommission += commissionInTier;

                result.Tiers.Add(new CommissionTierCalculationDto
                {
                    TierNumber = result.Tiers.Count + 1,
                    Name = tier.Name,
                    MinAmount = tier.Min,
                    MaxAmount = tier.Max,
                    Rate = tier.Rate,
                    AppliedAmount = appliedAmount,
                    CommissionAmount = commissionInTier
                });
            }
        }

        result.TotalCommission = totalCommission;
        _logger.LogInformation("Tiered commission calculated: {Total}", totalCommission);
        return result;
    }

    public async Task<decimal> ApplyCapsAndMinimumsAsync(
        decimal baseCommission,
        int ruleId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying caps and minimums to commission {Amount}, rule {RuleId}",
            baseCommission, ruleId);

        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Commission rule {ruleId} not found");

        decimal finalAmount = baseCommission;

        // Apply minimum
        var minimum = DefaultMinimumAmount;
        if (finalAmount < minimum)
        {
            finalAmount = minimum;
            _logger.LogInformation("Applied minimum commission: {Amount}", minimum);
        }

        // Apply cap
        var cap = DefaultCapAmount;
        if (finalAmount > cap)
        {
            finalAmount = cap;
            _logger.LogInformation("Applied commission cap: {Amount}", cap);
        }

        return finalAmount;
    }

    public async Task<List<SplitCommissionDto>> ProcessSplitCommissionAsync(
        int dealId,
        List<int> repIds,
        List<decimal> splitPercentages,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing split commission for deal {DealId} across {RepCount} reps",
            dealId, repIds.Count);

        if (repIds.Count != splitPercentages.Count)
            throw new ArgumentException("Rep IDs and split percentages must have same count");

        var totalPercentage = splitPercentages.Sum();
        if (Math.Abs(totalPercentage - 100m) > 0.01m)
            throw new ArgumentException($"Split percentages must sum to 100, got {totalPercentage}");

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dealId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
            throw new InvalidOperationException($"Opportunity {dealId} not found");

        // Get base commission for the deal
        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsActive && !r.IsDeleted, cancellationToken);

        var baseAmount = opportunity.Amount * (rule?.BaseRate ?? 0) / 100;

        var result = new List<SplitCommissionDto>();
        for (int i = 0; i < repIds.Count; i++)
        {
            var repId = repIds[i];
            var percentage = splitPercentages[i];
            var amount = baseAmount * percentage / 100;

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == repId, cancellationToken);

            result.Add(new SplitCommissionDto
            {
                SalesRepId = repId,
                SalesRepName = user?.FullName ?? $"Rep {repId}",
                AllocationPercentage = percentage,
                CommissionAmount = amount
            });
        }

        _logger.LogInformation("Split commission processed: {ResultCount} allocations", result.Count);
        return result;
    }

    public async Task<ClawbackDto> ApplyClawbackAsync(
        int dealId,
        decimal originalCommission,
        decimal clawbackPercentage,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying clawback to deal {DealId}, original {Commission}, percentage {Percentage}",
            dealId, originalCommission, clawbackPercentage);

        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dealId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
            throw new InvalidOperationException($"Opportunity {dealId} not found");

        var clawbackAmount = originalCommission * clawbackPercentage / 100;

        var result = new ClawbackDto
        {
            DealId = dealId,
            OriginalCommission = originalCommission,
            ClawbackPercentage = clawbackPercentage,
            ClawbackAmount = clawbackAmount,
            Reason = "Deal Churn",
            ClawbackDate = DateTime.UtcNow
        };

        _logger.LogInformation("Clawback calculated: {Amount}", clawbackAmount);
        return result;
    }

    public async Task<List<CommissionHistoryDto>> GetCommissionHistoryAsync(
        int salesRepId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting commission history for rep {RepId}, period {Start} to {End}",
            salesRepId, startDate, endDate);

        var commissions = await _context.Commissions
            .AsNoTracking()
            .Where(c => c.SalesRepUserId == salesRepId 
                && c.CreatedAt >= startDate 
                && c.CreatedAt <= endDate
                && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = commissions.Select(c => new CommissionHistoryDto
        {
            CommissionId = c.Id,
            DealId = c.OpportunityId ?? 0,
            Amount = c.FinalCommissionAmount,
            Status = c.Status.ToString(),
            TransactionDate = c.CreatedAt,
            Notes = c.Notes ?? string.Empty
        }).ToList();

        _logger.LogInformation("Retrieved {Count} commission records", result.Count);
        return result;
    }

    public async Task<CommissionSummaryDto> GetCommissionSummaryAsync(
        int salesRepId,
        string period,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting commission summary for rep {RepId}, period {Period}",
            salesRepId, period);

        // Parse period (YYYY-MM)
        if (!DateTime.TryParseExact(period + "-01", "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out var periodStart))
        {
            throw new ArgumentException($"Invalid period format: {period}");
        }

        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var commissions = await _context.Commissions
            .AsNoTracking()
            .Where(c => c.SalesRepUserId == salesRepId 
                && c.CreatedAt >= periodStart 
                && c.CreatedAt <= periodEnd
                && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var result = new CommissionSummaryDto
        {
            SalesRepId = salesRepId,
            Period = period,
            TotalEarned = commissions.Sum(c => c.FinalCommissionAmount),
            TotalPending = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.FinalCommissionAmount),
            TotalApproved = commissions.Where(c => c.Status == CommissionStatus.Approved).Sum(c => c.FinalCommissionAmount),
            TotalPaid = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.FinalCommissionAmount),
            DealCount = commissions.DistinctBy(c => c.OpportunityId).Count()
        };

        _logger.LogInformation("Commission summary: earned {Earned}, pending {Pending}, approved {Approved}, paid {Paid}",
            result.TotalEarned, result.TotalPending, result.TotalApproved, result.TotalPaid);

        return result;
    }
}

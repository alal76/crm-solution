// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CommissionTierDto = CRM.Core.Interfaces.CommissionTierDto;
using CommissionTriggerEvent = CRM.Core.Enums.CommissionTriggerEvent;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of ICommissionRulesEngine for complex commission calculations.
/// Handles caps, splits, tiered rates, and trigger-based commission logic.
/// </summary>
public class CommissionRulesEngine : ICommissionRulesEngine
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CommissionRulesEngine> _logger;

    public CommissionRulesEngine(ICrmDbContext context, ILogger<CommissionRulesEngine> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CommissionCalculationResultDto> CalculateCommissionAsync(
        int opportunityId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
        {
            throw new InvalidOperationException($"Opportunity {opportunityId} not found");
        }

        var dealAmount = opportunity.Amount;
        var productIds = await GetOpportunityProductIdsAsync(opportunityId, cancellationToken);

        // Get applicable rules
        var rules = await GetApplicableRulesAsync(userId, dealAmount, productIds, cancellationToken);
        var rule = rules.FirstOrDefault();

        if (rule == null)
        {
            _logger.LogWarning("No applicable commission rules found for user {UserId}, deal amount {Amount}",
                userId, dealAmount);
            return new CommissionCalculationResultDto
            {
                UserId = userId,
                OpportunityId = opportunityId,
                DealAmount = dealAmount,
                FinalCommissionAmount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Calculate base commission
        decimal baseCommission = CalculateBaseCommission(dealAmount, rule);

        // Apply tiered calculation if applicable
        if (rule.RuleType == CommissionRuleType.Tiered)
        {
            var tiers = await GetTiersForRuleAsync(rule.Id, cancellationToken);
            baseCommission = CalculateTieredCommission(dealAmount, tiers);
        }

        // Apply cap if configured
        var maxCommission = GetMaxCommissionFromRule(rule);
        var cappedCommission = ApplyCap(baseCommission, maxCommission);

        // Get team members for split calculation
        var teamSplits = await GetTeamSplitsAsync(opportunityId, userId, rule, cancellationToken);
        var userCommission = cappedCommission;
        if (teamSplits.Count > 1)
        {
            var splits = CalculateSplit(cappedCommission, teamSplits);
            userCommission = splits.TryGetValue(userId, out var amount) ? amount : cappedCommission;
        }

        _logger.LogInformation(
            "Commission calculated for opportunity {OpportunityId}, user {UserId}: " +
            "Base={BaseCommission}, Capped={CappedCommission}, UserShare={UserCommission}",
            opportunityId, userId, baseCommission, cappedCommission, userCommission);

        return new CommissionCalculationResultDto
        {
            UserId = userId,
            OpportunityId = opportunityId,
            DealAmount = dealAmount,
            BaseCommissionAmount = baseCommission,
            BaseCommissionRate = rule.Rate,
            FinalCommissionAmount = userCommission,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<CommissionCalculationResultDto> CalculateWithRuleAsync(
        int ruleId,
        decimal dealAmount,
        int userId,
        int[]? teamMemberIds = null,
        CancellationToken cancellationToken = default)
    {
        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId && !r.IsDeleted, cancellationToken);

        if (rule == null)
        {
            throw new InvalidOperationException($"Commission rule {ruleId} not found");
        }

        // Calculate base commission
        decimal baseCommission = CalculateBaseCommission(dealAmount, rule);

        // Apply tiered calculation if applicable
        if (rule.RuleType == CommissionRuleType.Tiered)
        {
            var tiers = await GetTiersForRuleAsync(rule.Id, cancellationToken);
            baseCommission = CalculateTieredCommission(dealAmount, tiers);
        }

        // Apply cap
        var maxCommission = GetMaxCommissionFromRule(rule);
        var cappedCommission = ApplyCap(baseCommission, maxCommission);

        // Apply splits if team members provided
        var userCommission = cappedCommission;
        if (teamMemberIds != null && teamMemberIds.Length > 0)
        {
            var splitPercentage = 100m / (teamMemberIds.Length + 1);
            var splits = new Dictionary<int, decimal> { { userId, splitPercentage } };
            foreach (var memberId in teamMemberIds)
            {
                splits[memberId] = splitPercentage;
            }
            var calculatedSplits = CalculateSplit(cappedCommission, splits);
            userCommission = calculatedSplits.TryGetValue(userId, out var amount) ? amount : cappedCommission;
        }

        return new CommissionCalculationResultDto
        {
            UserId = userId,
            DealAmount = dealAmount,
            BaseCommissionAmount = baseCommission,
            BaseCommissionRate = rule.Rate,
            FinalCommissionAmount = userCommission,
            CreatedAt = DateTime.UtcNow
        };
    }

    public decimal ApplyCap(decimal calculatedAmount, decimal? maxCommissionAmount)
    {
        if (!maxCommissionAmount.HasValue || maxCommissionAmount.Value <= 0)
        {
            return calculatedAmount;
        }

        var capped = Math.Min(calculatedAmount, maxCommissionAmount.Value);
        if (capped < calculatedAmount)
        {
            _logger.LogInformation("Commission capped: Original={Original}, Capped={Capped}, Max={Max}",
                calculatedAmount, capped, maxCommissionAmount.Value);
        }

        return capped;
    }

    public Dictionary<int, decimal> CalculateSplit(decimal totalCommission, Dictionary<int, decimal> splitPercentages)
    {
        var result = new Dictionary<int, decimal>();
        var totalPercentage = splitPercentages.Values.Sum();

        if (totalPercentage <= 0)
        {
            _logger.LogWarning("Invalid split percentages (total <= 0)");
            return result;
        }

        foreach (var (userId, percentage) in splitPercentages)
        {
            // Normalize percentages if they don't sum to 100
            var normalizedPercentage = (percentage / totalPercentage) * 100m;
            var amount = Math.Round(totalCommission * normalizedPercentage / 100m, 2);
            result[userId] = amount;
        }

        // Adjust for rounding errors - give remainder to first user
        var totalDistributed = result.Values.Sum();
        var remainder = totalCommission - totalDistributed;
        if (remainder != 0 && result.Any())
        {
            var firstUserId = result.Keys.First();
            result[firstUserId] += remainder;
        }

        _logger.LogInformation("Commission split calculated: Total={Total}, Splits={Splits}",
            totalCommission, string.Join(", ", result.Select(kvp => $"{kvp.Key}:{kvp.Value}")));

        return result;
    }

    public async Task<bool> CheckTriggerConditionsAsync(
        int opportunityId,
        CommissionTriggerEvent trigger,
        CancellationToken cancellationToken = default)
    {
        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted, cancellationToken);

        if (opportunity == null)
        {
            return false;
        }

        return trigger switch
        {
            CommissionTriggerEvent.OnClose => opportunity.Stage == OpportunityStage.ClosedWon,
            CommissionTriggerEvent.OnOrder => await CheckOrderExistsAsync(opportunityId, cancellationToken),
            CommissionTriggerEvent.OnInvoice => await CheckInvoiceExistsAsync(opportunityId, cancellationToken),
            CommissionTriggerEvent.OnPayment => await CheckPaymentReceivedAsync(opportunityId, cancellationToken),
            CommissionTriggerEvent.OnSignature => await CheckContractSignedAsync(opportunityId, cancellationToken),
            CommissionTriggerEvent.OnSubscriptionStart => await CheckSubscriptionActiveAsync(opportunityId, cancellationToken),
            CommissionTriggerEvent.Monthly => true, // Always eligible for monthly calculations
            _ => false
        };
    }

    public async Task<IEnumerable<CommissionRule>> GetApplicableRulesAsync(
        int userId,
        decimal dealAmount,
        int[]? productIds = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var rules = await _context.CommissionRules
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.IsActive)
            .Where(r => r.EffectiveDate <= now)
            .Where(r => !r.ExpiryDate.HasValue || r.ExpiryDate.Value >= now)
            .ToListAsync(cancellationToken);

        // Filter by applicable users
        var applicableRules = rules.Where(r =>
        {
            if (string.IsNullOrEmpty(r.ApplicableUserIds))
                return true;

            try
            {
                var userIds = JsonSerializer.Deserialize<int[]>(r.ApplicableUserIds);
                return userIds == null || userIds.Contains(userId);
            }
            catch
            {
                return true;
            }
        });

        // Filter by applicable products
        if (productIds != null && productIds.Length > 0)
        {
            applicableRules = applicableRules.Where(r =>
            {
                if (string.IsNullOrEmpty(r.ApplicableProductIds))
                    return true;

                try
                {
                    var ruleProductIds = JsonSerializer.Deserialize<int[]>(r.ApplicableProductIds);
                    return ruleProductIds == null || ruleProductIds.Intersect(productIds).Any();
                }
                catch
                {
                    return true;
                }
            });
        }

        // Filter by amount range for tiered rules
        applicableRules = applicableRules.Where(r =>
        {
            if (r.RuleType != CommissionRuleType.Tiered)
                return true;

            var minOk = !r.MinAmount.HasValue || dealAmount >= r.MinAmount.Value;
            var maxOk = !r.MaxAmount.HasValue || dealAmount <= r.MaxAmount.Value;
            return minOk && maxOk;
        });

        return applicableRules.OrderByDescending(r => r.EffectiveDate).ToList();
    }

    public decimal CalculateTieredCommission(decimal dealAmount, IEnumerable<CommissionTierDto> tiers)
    {
        var orderedTiers = tiers.OrderBy(t => t.MinAmount).ToList();
        decimal totalCommission = 0;
        decimal remainingAmount = dealAmount;

        foreach (var tier in orderedTiers)
        {
            if (remainingAmount <= 0)
                break;

            var tierMax = tier.MaxAmount ?? decimal.MaxValue;
            var tierRange = tierMax - tier.MinAmount;
            var amountInTier = Math.Min(remainingAmount, tierRange);

            if (amountInTier <= 0)
                continue;

            // Calculate commission for this tier
            var tierCommission = tier.FixedAmount.HasValue && tier.FixedAmount.Value > 0
                ? tier.FixedAmount.Value
                : amountInTier * tier.Rate / 100m;

            totalCommission += tierCommission;
            remainingAmount -= amountInTier;
        }

        return Math.Round(totalCommission, 2);
    }

    #region Private Helpers

    private decimal CalculateBaseCommission(decimal dealAmount, CommissionRule rule)
    {
        return rule.RuleType switch
        {
            CommissionRuleType.Percentage => dealAmount * rule.Rate / 100m,
            CommissionRuleType.Flat => rule.Rate,
            CommissionRuleType.Tiered => dealAmount * rule.Rate / 100m, // Placeholder, actual tiered calc done separately
            _ => dealAmount * rule.Rate / 100m
        };
    }

    private decimal? GetMaxCommissionFromRule(CommissionRule rule)
    {
        // Try to parse max commission from Configuration JSON
        if (string.IsNullOrEmpty(rule.Configuration))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(rule.Configuration);
            if (doc.RootElement.TryGetProperty("maxCommissionAmount", out var maxProp))
            {
                return maxProp.GetDecimal();
            }
            if (doc.RootElement.TryGetProperty("cap", out var capProp))
            {
                return capProp.GetDecimal();
            }
        }
        catch
        {
            // Configuration is not valid JSON or doesn't contain cap
        }

        return null;
    }

    private async Task<int[]> GetOpportunityProductIdsAsync(int opportunityId, CancellationToken cancellationToken)
    {
        var products = await _context.OpportunityProducts
            .AsNoTracking()
            .Where(op => op.OpportunityId == opportunityId && !op.IsDeleted)
            .Select(op => op.ProductId)
            .ToArrayAsync(cancellationToken);

        return products;
    }

    private async Task<Dictionary<int, decimal>> GetTeamSplitsAsync(
        int opportunityId,
        int primaryUserId,
        CommissionRule rule,
        CancellationToken cancellationToken)
    {
        var splits = new Dictionary<int, decimal>();

        // Try to get split configuration from rule
        if (!string.IsNullOrEmpty(rule.Configuration))
        {
            try
            {
                using var doc = JsonDocument.Parse(rule.Configuration);
                if (doc.RootElement.TryGetProperty("splitPercentage", out var splitProp))
                {
                    // If split percentage is configured, use it for the primary user
                    splits[primaryUserId] = splitProp.GetDecimal();
                }
                if (doc.RootElement.TryGetProperty("teamSplits", out var teamSplitsProp))
                {
                    foreach (var split in teamSplitsProp.EnumerateObject())
                    {
                        if (int.TryParse(split.Name, out var userId))
                        {
                            splits[userId] = split.Value.GetDecimal();
                        }
                    }
                }
            }
            catch
            {
                // Invalid configuration, use default
            }
        }

        // If no splits configured, assign 100% to primary user
        if (!splits.Any())
        {
            splits[primaryUserId] = 100m;
        }

        return splits;
    }

    private async Task<IEnumerable<CommissionTierDto>> GetTiersForRuleAsync(int ruleId, CancellationToken cancellationToken)
    {
        // For now, return based on rule configuration
        var rule = await _context.CommissionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);

        if (rule != null && !string.IsNullOrEmpty(rule.Configuration))
        {
            try
            {
                using var doc = JsonDocument.Parse(rule.Configuration);
                if (doc.RootElement.TryGetProperty("tiers", out var tiersProp))
                {
                    return JsonSerializer.Deserialize<List<CommissionTierDto>>(tiersProp.GetRawText()) ?? new List<CommissionTierDto>();
                }
            }
            catch
            {
                // Invalid configuration
            }
        }

        // Default tier based on rule
        return new List<CommissionTierDto>
        {
            new CommissionTierDto
            {
                MinAmount = rule?.MinAmount ?? 0,
                MaxAmount = rule?.MaxAmount,
                Rate = rule?.Rate ?? 0
            }
        };
    }

    private async Task<bool> CheckOrderExistsAsync(int opportunityId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AnyAsync(o => o.OpportunityId == opportunityId && !o.IsDeleted, cancellationToken);
    }

    private async Task<bool> CheckInvoiceExistsAsync(int opportunityId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OpportunityId == opportunityId && !o.IsDeleted, cancellationToken);
        if (order == null) return false;

        return await _context.Invoices
            .AnyAsync(i => i.OrderId == order.Id && !i.IsDeleted, cancellationToken);
    }

    private async Task<bool> CheckPaymentReceivedAsync(int opportunityId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OpportunityId == opportunityId && !o.IsDeleted, cancellationToken);
        if (order == null) return false;

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.OrderId == order.Id && !i.IsDeleted, cancellationToken);
        if (invoice == null) return false;

        return await _context.Payments
            .AnyAsync(p => p.InvoiceId == invoice.Id && !p.IsDeleted && p.Status == PaymentStatus.Completed, cancellationToken);
    }

    private async Task<bool> CheckContractSignedAsync(int opportunityId, CancellationToken cancellationToken)
    {
        return await _context.Contracts
            .AnyAsync(c => c.OpportunityId == opportunityId && !c.IsDeleted && c.IsSigned, cancellationToken);
    }

    private async Task<bool> CheckSubscriptionActiveAsync(int opportunityId, CancellationToken cancellationToken)
    {
        // Subscription links to Account, not Opportunity — find the opportunity's account first
        var opportunity = await _context.Opportunities
            .Where(o => o.Id == opportunityId)
            .Select(o => new { o.AccountId })
            .FirstOrDefaultAsync(cancellationToken);

        if (opportunity == null) return false;

        return await _context.Subscriptions
            .AnyAsync(s => s.AccountId == opportunity.AccountId && !s.IsDeleted && s.SubscriptionStatus == SubscriptionStatus.Active, cancellationToken);
    }

    #endregion
}

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
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dynamic Pricing Engine (TODO-GAP-07)
/// Calculates prices based on pricing rules, volume discounts, and customer segments.
/// </summary>
public class DynamicPricingEngine : IDynamicPricingEngine
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<DynamicPricingEngine> _logger;

    public DynamicPricingEngine(ICrmDbContext dbContext, ILogger<DynamicPricingEngine> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PriceCalculationResult> CalculatePriceAsync(
        int productId,
        int quantity,
        int? accountId = null,
        int? priceBookId = null,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (product == null)
        {
            return new PriceCalculationResult
            {
                ProductId = productId,
                Quantity = quantity,
                Success = false,
                ErrorMessage = $"Product {productId} not found"
            };
        }

        var listPrice = product.Price;
        var result = new PriceCalculationResult
        {
            ProductId = productId,
            Quantity = quantity,
            ListPrice = listPrice,
            UnitPrice = listPrice,
            Success = true
        };

        // Get applicable rules
        var rules = await GetApplicableRulesAsync(productId, accountId, cancellationToken);
        var rulesList = rules.OrderBy(r => r.Priority).ToList();

        decimal totalDiscount = 0;
        foreach (var rule in rulesList)
        {
            if (!rule.IsActive) continue;

            var discountAmount = CalculateRuleDiscount(rule, listPrice, quantity);
            if (discountAmount > 0)
            {
                totalDiscount += discountAmount;
                result.AppliedRules.Add(new AppliedPricingRule
                {
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    RuleType = rule.RuleType.ToString(),
                    DiscountAmount = discountAmount,
                    Priority = rule.Priority
                });
            }
        }

        // Apply cap if configured
        var maxDiscount = rulesList.FirstOrDefault()?.MaxDiscountAmount;
        if (maxDiscount.HasValue && totalDiscount > maxDiscount.Value)
        {
            totalDiscount = maxDiscount.Value;
        }

        result.DiscountAmount = totalDiscount;
        result.DiscountPercent = listPrice > 0 ? Math.Round(totalDiscount / listPrice * 100, 2) : 0;
        result.UnitPrice = listPrice - totalDiscount;
        result.ExtendedPrice = result.UnitPrice * quantity;

        _logger.LogDebug("Calculated price for product {ProductId}: List={ListPrice}, Final={UnitPrice}, Rules={RuleCount}",
            productId, listPrice, result.UnitPrice, result.AppliedRules.Count);

        return result;
    }

    private decimal CalculateRuleDiscount(PricingRule rule, decimal listPrice, int quantity)
    {
        // Check quantity threshold
        if (rule.MinQuantity.HasValue && quantity < rule.MinQuantity.Value)
            return 0;

        // Check volume tiers
        if (!string.IsNullOrEmpty(rule.VolumeTiers))
        {
            try
            {
                var tiers = JsonSerializer.Deserialize<List<VolumeTier>>(rule.VolumeTiers);
                var applicableTier = tiers?
                    .Where(t => quantity >= t.MinQty && (t.MaxQty == null || quantity <= t.MaxQty))
                    .OrderByDescending(t => t.MinQty)
                    .FirstOrDefault();

                if (applicableTier != null)
                {
                    return listPrice * applicableTier.Discount / 100;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse volume tiers for rule {RuleId}", rule.Id);
            }
        }

        // Apply standard discount
        switch (rule.DiscountMethod)
        {
            case DiscountMethod.PercentOff:
                if (rule.DiscountValue.HasValue)
                    return listPrice * rule.DiscountValue.Value / 100;
                break;

            case DiscountMethod.AmountOff:
                if (rule.DiscountValue.HasValue)
                    return rule.DiscountValue.Value;
                break;

            case DiscountMethod.FixedPrice:
                if (rule.FixedPrice.HasValue && rule.FixedPrice.Value < listPrice)
                    return listPrice - rule.FixedPrice.Value;
                break;
        }

        return 0;
    }

    public async Task<List<PriceCalculationResult>> CalculateBatchPricesAsync(
        IEnumerable<PriceLineItem> lineItems,
        int? accountId = null,
        int? priceBookId = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<PriceCalculationResult>();
        foreach (var item in lineItems)
        {
            var result = await CalculatePriceAsync(item.ProductId, item.Quantity, accountId, priceBookId, cancellationToken);
            
            // Override base price if specified
            if (item.BasePrice.HasValue)
            {
                result.ListPrice = item.BasePrice.Value;
                result.UnitPrice = item.BasePrice.Value - result.DiscountAmount;
                result.ExtendedPrice = result.UnitPrice * item.Quantity;
            }
            
            results.Add(result);
        }
        return results;
    }

    public async Task<IEnumerable<PricingRule>> GetApplicableRulesAsync(
        int productId, 
        int? accountId = null, 
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var productIdStr = productId.ToString();
        var accountIdStr = accountId?.ToString() ?? string.Empty;

        var rules = await _dbContext.PricingRules
            .Where(r => !r.IsDeleted && r.IsActive)
            .Where(r => r.EffectiveStartDate == null || r.EffectiveStartDate <= now)
            .Where(r => r.EffectiveEndDate == null || r.EffectiveEndDate >= now)
            .ToListAsync(cancellationToken);

        // Filter by product applicability
        return rules.Where(r =>
            r.AppliesToAllProducts ||
            (!string.IsNullOrEmpty(r.ProductIds) && r.ProductIds.Contains(productIdStr))
        ).Where(r =>
            string.IsNullOrEmpty(r.AccountIds) ||
            r.AccountIds.Contains(accountIdStr)
        ).OrderBy(r => r.Priority);
    }

    private class VolumeTier
    {
        public int MinQty { get; set; }
        public int? MaxQty { get; set; }
        public decimal Discount { get; set; }
    }
}

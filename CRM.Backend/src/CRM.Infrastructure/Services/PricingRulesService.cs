// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Dynamic Pricing Rules Service implementation (TODO-GAP-07).
/// Orchestrates volume discounts, customer-specific pricing, promotional codes
/// and time-based rules via the <see cref="IDynamicPricingEngine"/>.
/// </summary>
public class PricingRulesService : IPricingRulesService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IDynamicPricingEngine _pricingEngine;
    private readonly ILogger<PricingRulesService> _logger;

    public PricingRulesService(
        ICrmDbContext dbContext,
        IDynamicPricingEngine pricingEngine,
        ILogger<PricingRulesService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<decimal> CalculatePriceAsync(
        int productId,
        int quantity,
        int? customerId,
        string? promoCode,
        CancellationToken ct = default)
    {
        var breakdown = await GetPriceBreakdownAsync(
            new PriceCalculationRequest
            {
                ProductId = productId,
                Quantity = quantity,
                CustomerId = customerId,
                PromoCode = promoCode
            },
            ct);

        return breakdown.FinalPrice;
    }

    /// <inheritdoc/>
    public async Task<PriceBreakdownDto> GetPriceBreakdownAsync(
        PriceCalculationRequest request,
        CancellationToken ct = default)
    {
        // 1. Core calculation — volume discounts, customer-specific pricing, time-based rules
        var engineResult = await _pricingEngine.CalculatePriceAsync(
            request.ProductId,
            request.Quantity,
            request.CustomerId,
            request.PriceBookId,
            ct);

        var breakdown = new PriceBreakdownDto
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            BasePrice = engineResult.ListPrice,
            UnitPrice = engineResult.UnitPrice,
            DiscountAmount = engineResult.DiscountAmount,
            DiscountPercent = engineResult.DiscountPercent,
            DiscountType = engineResult.AppliedRules.FirstOrDefault()?.RuleType ?? string.Empty,
            FinalPrice = engineResult.UnitPrice,
            ExtendedPrice = engineResult.ExtendedPrice,
            AppliedRules = engineResult.AppliedRules.Select(r => new AppliedRuleSummary
            {
                RuleId = r.RuleId,
                RuleName = r.RuleName,
                RuleType = r.RuleType,
                DiscountAmount = r.DiscountAmount,
                Description = $"Rule '{r.RuleName}' applied {r.DiscountAmount:C} discount"
            }).ToList()
        };

        // 2. Promotional code — look for a Promotional rule whose Conditions JSON matches
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var promoDiscount = await ApplyPromoCodeAsync(
                request.PromoCode.Trim().ToUpperInvariant(),
                engineResult.ListPrice,
                breakdown,
                ct);

            if (promoDiscount > 0)
            {
                breakdown.DiscountAmount += promoDiscount;
                breakdown.FinalPrice = Math.Max(0, breakdown.BasePrice - breakdown.DiscountAmount);
                breakdown.ExtendedPrice = breakdown.FinalPrice * request.Quantity;
                breakdown.DiscountPercent = breakdown.BasePrice > 0
                    ? Math.Round(breakdown.DiscountAmount / breakdown.BasePrice * 100, 2)
                    : 0;
                breakdown.PromoCodeApplied = request.PromoCode.Trim().ToUpperInvariant();
                breakdown.DiscountType = "Promotional";
            }
            else
            {
                _logger.LogDebug("Promo code '{PromoCode}' not valid or not applicable", request.PromoCode);
            }
        }

        return breakdown;
    }

    /// <inheritdoc/>
    public async Task<List<PricingRule>> GetActiveRulesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.PricingRules
            .Where(r => !r.IsDeleted && r.IsActive)
            .Where(r => r.EffectiveStartDate == null || r.EffectiveStartDate <= now)
            .Where(r => r.EffectiveEndDate == null || r.EffectiveEndDate >= now)
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PricingRule> CreateRuleAsync(CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        var rule = MapFromCreateDto(dto);
        rule.CreatedAt = DateTime.UtcNow;
        rule.UpdatedAt = DateTime.UtcNow;

        _dbContext.PricingRules.Add(rule);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created pricing rule '{Name}' (Id={Id})", rule.Name, rule.Id);
        return rule;
    }

    /// <inheritdoc/>
    public async Task<PricingRule?> UpdateRuleAsync(int id, UpdatePricingRuleDto dto, CancellationToken ct = default)
    {
        var rule = await _dbContext.PricingRules
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        if (rule == null)
        {
            _logger.LogWarning("Pricing rule {Id} not found for update", id);
            return null;
        }

        MapUpdateDtoToEntity(dto, rule);
        rule.UpdatedAt = DateTime.UtcNow;

        _dbContext.PricingRules.Update(rule);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Updated pricing rule '{Name}' (Id={Id})", rule.Name, rule.Id);
        return rule;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteRuleAsync(int id, CancellationToken ct = default)
    {
        var rule = await _dbContext.PricingRules
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        if (rule == null)
        {
            return false;
        }

        rule.IsDeleted = true;
        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;

        _dbContext.PricingRules.Update(rule);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted pricing rule {Id}", id);
        return true;
    }

    // ─── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Looks up a valid Promotional pricing rule whose Conditions JSON contains
    /// the promo code (e.g. {"promoCode":"SUMMER20"}) and returns the
    /// discount amount to apply.
    /// </summary>
    private async Task<decimal> ApplyPromoCodeAsync(
        string normalizedCode,
        decimal basePrice,
        PriceBreakdownDto breakdown,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Fetch all active Promotional rules that might have the code
        var promoRules = await _dbContext.PricingRules
            .Where(r => !r.IsDeleted
                     && r.IsActive
                     && r.RuleType == PricingRuleType.Promotional
                     && r.Conditions != null
                     && (r.EffectiveStartDate == null || r.EffectiveStartDate <= now)
                     && (r.EffectiveEndDate == null || r.EffectiveEndDate >= now)
                     && (r.UsageLimit == null || r.UsageCount < r.UsageLimit))
            .ToListAsync(ct);

        foreach (var rule in promoRules.OrderBy(r => r.Priority))
        {
            if (!TryMatchPromoCode(rule.Conditions!, normalizedCode))
                continue;

            var discount = CalculatePromoDiscount(rule, basePrice);
            if (discount <= 0)
                continue;

            // Respect MaxDiscountAmount cap
            if (rule.MaxDiscountAmount.HasValue)
                discount = Math.Min(discount, rule.MaxDiscountAmount.Value);

            breakdown.AppliedRules.Add(new AppliedRuleSummary
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleType = "Promotional",
                DiscountAmount = discount,
                Description = $"Promo code '{normalizedCode}' applied"
            });

            _logger.LogDebug("Promo code '{Code}' matched rule {RuleId}, discount={Discount}",
                normalizedCode, rule.Id, discount);

            // If rule doesn't combine with others, stop here
            if (!rule.CombineWithOtherRules)
                break;
        }

        return breakdown.AppliedRules
            .Where(r => r.RuleType == "Promotional")
            .Sum(r => r.DiscountAmount);
    }

    private static bool TryMatchPromoCode(string conditionsJson, string normalizedCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionsJson);
            if (doc.RootElement.TryGetProperty("promoCode", out var prop))
            {
                var code = prop.GetString()?.ToUpperInvariant();
                return code == normalizedCode;
            }
        }
        catch (JsonException)
        {
            // Not valid JSON — ignore
        }

        return false;
    }

    private static decimal CalculatePromoDiscount(PricingRule rule, decimal basePrice)
    {
        return rule.DiscountMethod switch
        {
            DiscountMethod.PercentOff when rule.DiscountValue.HasValue =>
                basePrice * rule.DiscountValue.Value / 100m,
            DiscountMethod.AmountOff when rule.DiscountValue.HasValue =>
                rule.DiscountValue.Value,
            DiscountMethod.FixedPrice when rule.FixedPrice.HasValue && rule.FixedPrice.Value < basePrice =>
                basePrice - rule.FixedPrice.Value,
            _ => 0m
        };
    }

    private static PricingRule MapFromCreateDto(CreatePricingRuleDto dto) =>
        new PricingRule
        {
            Name = dto.Name,
            Description = dto.Description,
            RuleType = (PricingRuleType)dto.RuleType,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            AppliesToAllProducts = dto.AppliesToAllProducts,
            ProductIds = dto.ProductIds,
            ProductCategories = dto.ProductCategories,
            AccountIds = dto.AccountIds,
            CustomerSegments = dto.CustomerSegments,
            DiscountMethod = (DiscountMethod)dto.DiscountMethod,
            DiscountValue = dto.DiscountValue,
            FixedPrice = dto.FixedPrice,
            MinQuantity = dto.MinQuantity,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            VolumeTiers = dto.VolumeTiers,
            EffectiveStartDate = dto.EffectiveStartDate,
            EffectiveEndDate = dto.EffectiveEndDate,
            UsageLimit = dto.UsageLimit,
            Conditions = dto.Conditions,
            CombineWithOtherRules = dto.CombineWithOtherRules
        };

    private static void MapUpdateDtoToEntity(UpdatePricingRuleDto dto, PricingRule rule)
    {
        rule.Name = dto.Name;
        rule.Description = dto.Description;
        rule.RuleType = (PricingRuleType)dto.RuleType;
        rule.IsActive = dto.IsActive;
        rule.Priority = dto.Priority;
        rule.AppliesToAllProducts = dto.AppliesToAllProducts;
        rule.ProductIds = dto.ProductIds;
        rule.ProductCategories = dto.ProductCategories;
        rule.AccountIds = dto.AccountIds;
        rule.CustomerSegments = dto.CustomerSegments;
        rule.DiscountMethod = (DiscountMethod)dto.DiscountMethod;
        rule.DiscountValue = dto.DiscountValue;
        rule.FixedPrice = dto.FixedPrice;
        rule.MinQuantity = dto.MinQuantity;
        rule.MaxDiscountAmount = dto.MaxDiscountAmount;
        rule.VolumeTiers = dto.VolumeTiers;
        rule.EffectiveStartDate = dto.EffectiveStartDate;
        rule.EffectiveEndDate = dto.EffectiveEndDate;
        rule.UsageLimit = dto.UsageLimit;
        rule.Conditions = dto.Conditions;
        rule.CombineWithOtherRules = dto.CombineWithOtherRules;
    }
}

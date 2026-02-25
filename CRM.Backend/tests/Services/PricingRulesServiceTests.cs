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
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PricingRulesService"/> (TODO-GAP-07).
/// Uses EF Core InMemory database for real DbContext and a Moq for IDynamicPricingEngine.
/// </summary>
public class PricingRulesServiceTests
{
    // ─── Helpers ────────────────────────────────────────────────────────────

    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"PricingRulesTests_{Guid.NewGuid()}")
            .Options;

        var config = new ConfigurationBuilder().Build();
        return new CrmDbContext(options, config);
    }

    private static (PricingRulesService service, Mock<IDynamicPricingEngine> engineMock)
        CreateService(CrmDbContext context)
    {
        var engineMock = new Mock<IDynamicPricingEngine>();
        var service = new PricingRulesService(
            context,
            engineMock.Object,
            Mock.Of<ILogger<PricingRulesService>>());

        return (service, engineMock);
    }

    private static PriceCalculationResult DefaultEngineResult(int productId, int quantity, decimal listPrice) =>
        new PriceCalculationResult
        {
            ProductId = productId,
            Quantity = quantity,
            ListPrice = listPrice,
            UnitPrice = listPrice,
            DiscountAmount = 0,
            DiscountPercent = 0,
            ExtendedPrice = listPrice * quantity,
            Success = true
        };

    // ─── Test 1: GetActiveRulesAsync returns only active, non-deleted rules ─

    [Fact]
    public async Task GetActiveRulesAsync_ReturnsOnlyActiveNonDeletedRules()
    {
        await using var ctx = CreateContext();
        ctx.PricingRules.AddRange(
            new PricingRule { Name = "Active Rule", IsActive = true, IsDeleted = false, Priority = 10 },
            new PricingRule { Name = "Inactive Rule", IsActive = false, IsDeleted = false, Priority = 20 },
            new PricingRule { Name = "Deleted Rule", IsActive = true, IsDeleted = true, Priority = 30 }
        );
        await ctx.SaveChangesAsync();

        var (service, _) = CreateService(ctx);

        var rules = await service.GetActiveRulesAsync();

        Assert.Single(rules);
        Assert.Equal("Active Rule", rules[0].Name);
    }

    // ─── Test 2: GetActiveRulesAsync excludes expired rules ─────────────────

    [Fact]
    public async Task GetActiveRulesAsync_ExcludesExpiredAndFutureRules()
    {
        await using var ctx = CreateContext();
        var now = DateTime.UtcNow;
        ctx.PricingRules.AddRange(
            new PricingRule { Name = "Valid", IsActive = true, IsDeleted = false,
                EffectiveStartDate = now.AddDays(-1), EffectiveEndDate = now.AddDays(1) },
            new PricingRule { Name = "Expired", IsActive = true, IsDeleted = false,
                EffectiveEndDate = now.AddDays(-1) },
            new PricingRule { Name = "Future", IsActive = true, IsDeleted = false,
                EffectiveStartDate = now.AddDays(1) }
        );
        await ctx.SaveChangesAsync();

        var (service, _) = CreateService(ctx);

        var rules = await service.GetActiveRulesAsync();

        Assert.Single(rules);
        Assert.Equal("Valid", rules[0].Name);
    }

    // ─── Test 3: CreateRuleAsync persists the new rule ───────────────────────

    [Fact]
    public async Task CreateRuleAsync_PersistsRuleWithCorrectFields()
    {
        await using var ctx = CreateContext();
        var (service, _) = CreateService(ctx);

        var dto = new CreatePricingRuleDto
        {
            Name = "Summer Sale",
            RuleType = (int)PricingRuleType.Promotional,
            DiscountMethod = (int)DiscountMethod.PercentOff,
            DiscountValue = 15m,
            IsActive = true,
            Priority = 50,
            AppliesToAllProducts = true,
            CombineWithOtherRules = true
        };

        var created = await service.CreateRuleAsync(dto);

        Assert.True(created.Id > 0);
        Assert.Equal("Summer Sale", created.Name);
        Assert.Equal(15m, created.DiscountValue);
        Assert.Equal(PricingRuleType.Promotional, created.RuleType);
        Assert.True(created.IsActive);
    }

    // ─── Test 4: UpdateRuleAsync updates an existing rule ───────────────────

    [Fact]
    public async Task UpdateRuleAsync_UpdatesFieldsAndReturnsUpdatedRule()
    {
        await using var ctx = CreateContext();
        ctx.PricingRules.Add(new PricingRule
        {
            Name = "Old Name",
            IsActive = true,
            IsDeleted = false,
            Priority = 100,
            DiscountMethod = DiscountMethod.PercentOff,
            DiscountValue = 5m,
            RuleType = PricingRuleType.VolumeDiscount
        });
        await ctx.SaveChangesAsync();
        var ruleId = ctx.PricingRules.First().Id;

        var (service, _) = CreateService(ctx);

        var dto = new UpdatePricingRuleDto
        {
            Id = ruleId,
            Name = "Updated Name",
            RuleType = (int)PricingRuleType.VolumeDiscount,
            DiscountMethod = (int)DiscountMethod.PercentOff,
            DiscountValue = 10m,
            IsActive = true,
            Priority = 50,
            CombineWithOtherRules = true
        };

        var updated = await service.UpdateRuleAsync(ruleId, dto);

        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal(10m, updated.DiscountValue);
        Assert.Equal(50, updated.Priority);
    }

    // ─── Test 5: DeleteRuleAsync soft-deletes the rule ───────────────────────

    [Fact]
    public async Task DeleteRuleAsync_SoftDeletesRule()
    {
        await using var ctx = CreateContext();
        ctx.PricingRules.Add(new PricingRule
        {
            Name = "To Delete",
            IsActive = true,
            IsDeleted = false
        });
        await ctx.SaveChangesAsync();
        var ruleId = ctx.PricingRules.First().Id;

        var (service, _) = CreateService(ctx);

        var result = await service.DeleteRuleAsync(ruleId);

        Assert.True(result);
        var dbRule = ctx.PricingRules.Single(r => r.Id == ruleId);
        Assert.True(dbRule.IsDeleted);
        Assert.False(dbRule.IsActive);
    }

    // ─── Test 6: GetPriceBreakdownAsync applies promo code discount ──────────

    [Fact]
    public async Task GetPriceBreakdownAsync_AppliesPromoCodeDiscount()
    {
        await using var ctx = CreateContext();

        // Seed a Promotional rule matching promo code "SAVE10"
        ctx.PricingRules.Add(new PricingRule
        {
            Name = "SAVE10 Promo",
            IsActive = true,
            IsDeleted = false,
            RuleType = PricingRuleType.Promotional,
            DiscountMethod = DiscountMethod.PercentOff,
            DiscountValue = 10m,
            Conditions = "{\"promoCode\":\"SAVE10\"}",
            CombineWithOtherRules = true
        });
        await ctx.SaveChangesAsync();

        var (service, engineMock) = CreateService(ctx);

        // Engine returns list price 100, no discount from rules
        engineMock
            .Setup(e => e.CalculatePriceAsync(1, 2, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultEngineResult(1, 2, 100m));

        var breakdown = await service.GetPriceBreakdownAsync(
            new PriceCalculationRequest { ProductId = 1, Quantity = 2, PromoCode = "SAVE10" });

        Assert.Equal(100m, breakdown.BasePrice);
        Assert.Equal(10m, breakdown.DiscountAmount);           // 10% of 100
        Assert.Equal(90m, breakdown.FinalPrice);
        Assert.Equal(180m, breakdown.ExtendedPrice);           // 90 × 2
        Assert.Equal("SAVE10", breakdown.PromoCodeApplied);
        Assert.Contains(breakdown.AppliedRules, r => r.RuleType == "Promotional");
    }

    // ─── Test 7: GetPriceBreakdownAsync ignores invalid/unknown promo code ───

    [Fact]
    public async Task GetPriceBreakdownAsync_DoesNotApplyUnknownPromoCode()
    {
        await using var ctx = CreateContext();
        // No promo rule seeded

        var (service, engineMock) = CreateService(ctx);

        engineMock
            .Setup(e => e.CalculatePriceAsync(1, 1, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultEngineResult(1, 1, 200m));

        var breakdown = await service.GetPriceBreakdownAsync(
            new PriceCalculationRequest { ProductId = 1, Quantity = 1, PromoCode = "INVALID" });

        Assert.Null(breakdown.PromoCodeApplied);
        Assert.Equal(200m, breakdown.FinalPrice);
    }

    // ─── Test 8: DeleteRuleAsync returns false for non-existent rule ─────────

    [Fact]
    public async Task DeleteRuleAsync_ReturnsFalseWhenRuleNotFound()
    {
        await using var ctx = CreateContext();
        var (service, _) = CreateService(ctx);

        var result = await service.DeleteRuleAsync(999);

        Assert.False(result);
    }

    // ─── Test 9: UpdateRuleAsync returns null for non-existent rule ──────────

    [Fact]
    public async Task UpdateRuleAsync_ReturnsNullWhenRuleNotFound()
    {
        await using var ctx = CreateContext();
        var (service, _) = CreateService(ctx);

        var result = await service.UpdateRuleAsync(999, new UpdatePricingRuleDto { Id = 999, Name = "X" });

        Assert.Null(result);
    }

    // ─── Test 10: CalculatePriceAsync delegates to GetPriceBreakdownAsync ────

    [Fact]
    public async Task CalculatePriceAsync_ReturnsFinalPriceFromBreakdown()
    {
        await using var ctx = CreateContext();
        var (service, engineMock) = CreateService(ctx);

        engineMock
            .Setup(e => e.CalculatePriceAsync(5, 3, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceCalculationResult
            {
                ProductId = 5,
                Quantity = 3,
                ListPrice = 50m,
                UnitPrice = 45m,
                DiscountAmount = 5m,
                DiscountPercent = 10m,
                ExtendedPrice = 135m,
                Success = true
            });

        var price = await service.CalculatePriceAsync(5, 3, null, null);

        Assert.Equal(45m, price);
    }
}

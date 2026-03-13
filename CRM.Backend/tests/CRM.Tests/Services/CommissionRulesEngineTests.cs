// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Exceptions;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CommissionRulesEngine.
/// Pure-arithmetic methods (ApplyCap, CalculateSplit) are tested directly.
/// Methods that require DB queries use InMemory EF Core.
/// </summary>
public class CommissionRulesEngineTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"CommRules_{Guid.NewGuid()}")
                .Options,
            null!);

    private static CommissionRulesEngine CreateEngine(CrmDbContext db) =>
        new CommissionRulesEngine(db, new Mock<ILogger<CommissionRulesEngine>>().Object);

    // ── ApplyCap (pure method) ───────────────────────────────────────────────

    // xUnit InlineData cannot convert int → decimal? — use int? and convert manually.
    [Theory]
    [InlineData(1000, 500, 500)]   // capped at 500
    [InlineData(300, 500, 300)]    // below cap — returned as-is
    [InlineData(1000, null, 1000)] // no cap — returned as-is
    [InlineData(1000, 0, 1000)]    // zero cap treated as no cap
    public void ApplyCap_ShouldReturnExpectedAmount(int calculated, int? maxInt, int expected)
    {
        using var db = CreateDb();
        var engine = CreateEngine(db);
        decimal? max = maxInt.HasValue ? (decimal?)maxInt.Value : null;

        var result = engine.ApplyCap(calculated, max);

        result.Should().Be(expected);
    }

    // ── CalculateSplit (pure method) ─────────────────────────────────────────

    [Fact]
    public void CalculateSplit_ShouldDistributeEquallyWhenTwoUsersWithEqualPercentages()
    {
        using var db = CreateDb();
        var engine = CreateEngine(db);

        var splits = new Dictionary<int, decimal> { { 1, 50 }, { 2, 50 } };
        var result = engine.CalculateSplit(100m, splits);

        result[1].Should().Be(50m);
        result[2].Should().Be(50m);
        result.Values.Sum().Should().Be(100m);
    }

    [Fact]
    public void CalculateSplit_ShouldNormalizeWhenPercentagesDontSumTo100()
    {
        using var db = CreateDb();
        var engine = CreateEngine(db);

        // 30 + 70 = 100 but represented as 3 and 7
        var splits = new Dictionary<int, decimal> { { 1, 3 }, { 2, 7 } };
        var result = engine.CalculateSplit(100m, splits);

        result.Values.Sum().Should().Be(100m);
        result[2].Should().BeGreaterThan(result[1]);
    }

    [Fact]
    public void CalculateSplit_ShouldReturnEmptyWhenTotalPercentageIsZero()
    {
        using var db = CreateDb();
        var engine = CreateEngine(db);

        var splits = new Dictionary<int, decimal> { { 1, 0 }, { 2, 0 } };
        var result = engine.CalculateSplit(100m, splits);

        result.Should().BeEmpty();
    }

    // ── GetApplicableRulesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetApplicableRulesAsync_ShouldReturnActiveNonExpiredRules()
    {
        using var db = CreateDb();

        db.CommissionRules.AddRange(
            new CommissionRule
            {
                Id = 1, Name = "Active Rule", SaleType = "Direct",
                RuleType = CommissionRuleType.Percentage, Rate = 0.10m,
                IsActive = true, IsDeleted = false,
                EffectiveDate = DateTime.UtcNow.AddDays(-30)
            },
            new CommissionRule
            {
                Id = 2, Name = "Inactive Rule", SaleType = "Direct",
                RuleType = CommissionRuleType.Percentage, Rate = 0.05m,
                IsActive = false, IsDeleted = false,
                EffectiveDate = DateTime.UtcNow.AddDays(-30)
            },
            new CommissionRule
            {
                Id = 3, Name = "Expired Rule", SaleType = "Direct",
                RuleType = CommissionRuleType.Percentage, Rate = 0.08m,
                IsActive = true, IsDeleted = false,
                EffectiveDate = DateTime.UtcNow.AddDays(-60),
                ExpiryDate = DateTime.UtcNow.AddDays(-1)
            }
        );
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        var rules = await engine.GetApplicableRulesAsync(userId: 1, dealAmount: 5000m);

        rules.Should().HaveCount(1);
        rules.First().Name.Should().Be("Active Rule");
    }

    // ── CalculateWithRuleAsync ───────────────────────────────────────────────

    [Fact]
    public async Task CalculateWithRuleAsync_ShouldThrow_WhenRuleNotFound()
    {
        using var db = CreateDb();
        var engine = CreateEngine(db);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => engine.CalculateWithRuleAsync(999, 5000m, 1));
    }

    [Fact]
    public async Task CalculateWithRuleAsync_ShouldCalculatePercentageCommission()
    {
        using var db = CreateDb();
        db.CommissionRules.Add(new CommissionRule
        {
            Id = 10, Name = "10% Rule", SaleType = "Direct",
            RuleType = CommissionRuleType.Percentage, Rate = 10m,  // stored as %; formula: dealAmount * rate / 100
            IsActive = true, IsDeleted = false,
            EffectiveDate = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        var result = await engine.CalculateWithRuleAsync(10, 10000m, userId: 1);

        result.DealAmount.Should().Be(10000m);
        result.BaseCommissionAmount.Should().Be(1000m); // 10% of 10000 = 10000 * 10 / 100
        result.FinalCommissionAmount.Should().Be(1000m);
        result.UserId.Should().Be(1);
    }

    [Fact]
    public async Task CalculateWithRuleAsync_ShouldApplySplitWhenTeamMembersProvided()
    {
        using var db = CreateDb();
        db.CommissionRules.Add(new CommissionRule
        {
            Id = 20, Name = "10% Rule Split", SaleType = "Direct",
            RuleType = CommissionRuleType.Percentage, Rate = 10m,  // 10% stored as integer %
            IsActive = true, IsDeleted = false,
            EffectiveDate = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        // 2 team members + current user = 3 ways; 9000 * 10/100 = 900; 900/3 = 300
        var result = await engine.CalculateWithRuleAsync(20, 9000m, userId: 1, teamMemberIds: new[] { 2, 3 });

        result.FinalCommissionAmount.Should().Be(300m);
    }
}

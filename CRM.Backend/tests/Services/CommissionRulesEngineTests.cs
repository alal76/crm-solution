// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Linq;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CommissionRulesEngine.
/// Tests cap enforcement, split calculations, tiered rates, and trigger conditions.
/// Covers TODO-SYS008-014.
/// </summary>
public class CommissionRulesEngineTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CommissionRulesEngine>> _mockLogger;
    private readonly CommissionRulesEngine _engine;

    public CommissionRulesEngineTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionRulesEngine>>();
        _engine = new CommissionRulesEngine(_mockContext.Object, _mockLogger.Object);
    }

    #region ApplyCap Tests

    [Fact]
    public void ApplyCap_WithNullCap_ShouldReturnOriginalAmount()
    {
        // Arrange
        decimal calculatedAmount = 1000m;

        // Act
        var result = _engine.ApplyCap(calculatedAmount, null);

        // Assert
        result.Should().Be(1000m);
    }

    [Fact]
    public void ApplyCap_WithZeroCap_ShouldReturnOriginalAmount()
    {
        // Arrange — zero cap treated as "no cap"
        decimal calculatedAmount = 1000m;

        // Act
        var result = _engine.ApplyCap(calculatedAmount, 0m);

        // Assert
        result.Should().Be(1000m);
    }

    [Fact]
    public void ApplyCap_WhenCommissionAboveCap_ShouldReturnCapAmount()
    {
        // Arrange
        decimal calculatedAmount = 5000m;
        decimal cap = 2000m;

        // Act
        var result = _engine.ApplyCap(calculatedAmount, cap);

        // Assert
        result.Should().Be(2000m, "commission above cap must be truncated to cap amount");
    }

    [Fact]
    public void ApplyCap_WhenCommissionBelowCap_ShouldReturnOriginalAmount()
    {
        // Arrange
        decimal calculatedAmount = 500m;
        decimal cap = 2000m;

        // Act
        var result = _engine.ApplyCap(calculatedAmount, cap);

        // Assert
        result.Should().Be(500m, "commission below cap should not be affected");
    }

    [Fact]
    public void ApplyCap_WhenCommissionExactlyAtCap_ShouldReturnCapAmount()
    {
        // Arrange
        decimal calculatedAmount = 3000m;
        decimal cap = 3000m;

        // Act
        var result = _engine.ApplyCap(calculatedAmount, cap);

        // Assert
        result.Should().Be(3000m);
    }

    #endregion

    #region CalculateSplit Tests

    [Fact]
    public void CalculateSplit_WithEqualPercentages_ShouldSplitEvenly()
    {
        // Arrange
        decimal totalCommission = 1000m;
        var splits = new Dictionary<int, decimal>
        {
            { 1, 50m },
            { 2, 50m }
        };

        // Act
        var result = _engine.CalculateSplit(totalCommission, splits);

        // Assert
        result.Should().ContainKey(1);
        result.Should().ContainKey(2);
        result[1].Should().Be(500m);
        result[2].Should().Be(500m);
        result.Values.Sum().Should().Be(totalCommission, "split amounts must sum to total commission");
    }

    [Fact]
    public void CalculateSplit_WithUnequalPercentages_ShouldAllocateProportionally()
    {
        // Arrange — rep 1 gets 70%, rep 2 gets 30%
        decimal totalCommission = 1000m;
        var splits = new Dictionary<int, decimal>
        {
            { 1, 70m },
            { 2, 30m }
        };

        // Act
        var result = _engine.CalculateSplit(totalCommission, splits);

        // Assert
        result[1].Should().Be(700m);
        result[2].Should().Be(300m);
        result.Values.Sum().Should().Be(1000m);
    }

    [Fact]
    public void CalculateSplit_WithThreeWaySplit_ShouldDistributeAll()
    {
        // Arrange — three reps with equal share
        decimal totalCommission = 300m;
        var splits = new Dictionary<int, decimal>
        {
            { 1, 33.33m },
            { 2, 33.33m },
            { 3, 33.34m }
        };

        // Act
        var result = _engine.CalculateSplit(totalCommission, splits);

        // Assert
        result.Values.Sum().Should().Be(300m, "rounding adjustments must preserve total commission");
    }

    [Fact]
    public void CalculateSplit_WithZeroOrNegativeTotal_ShouldReturnEmptyDictionary()
    {
        // Arrange — invalid percentages (total <= 0)
        decimal totalCommission = 500m;
        var splits = new Dictionary<int, decimal>
        {
            { 1, 0m },
            { 2, 0m }
        };

        // Act
        var result = _engine.CalculateSplit(totalCommission, splits);

        // Assert
        result.Should().BeEmpty("zero percentages produce empty result");
    }

    [Fact]
    public void CalculateSplit_WithSingleRecipient_ShouldReceiveFullAmount()
    {
        // Arrange
        decimal totalCommission = 800m;
        var splits = new Dictionary<int, decimal> { { 1, 100m } };

        // Act
        var result = _engine.CalculateSplit(totalCommission, splits);

        // Assert
        result[1].Should().Be(800m);
    }

    #endregion

    #region CalculateTieredCommission Tests

    [Fact]
    public void CalculateTieredCommission_SingleTier_ShouldCalculateCorrectly()
    {
        // Arrange — $10,000 deal with 5% rate on entire amount
        decimal dealAmount = 10_000m;
        var tiers = new List<CommissionTierDto>
        {
            new CommissionTierDto { MinAmount = 0m, MaxAmount = null, Rate = 5m }
        };

        // Act
        var result = _engine.CalculateTieredCommission(dealAmount, tiers);

        // Assert
        result.Should().Be(500m, "5% of $10,000 = $500");
    }

    [Fact]
    public void CalculateTieredCommission_MultipleTiers_ShouldApplyCorrectRatePerBracket()
    {
        // Arrange — $10,000 deal:
        //   Tier 1: $0–$5,000 at 5%    = $250
        //   Tier 2: $5,000–$10,000 at 10% = $500
        //   Total = $750
        decimal dealAmount = 10_000m;
        var tiers = new List<CommissionTierDto>
        {
            new CommissionTierDto { MinAmount = 0m, MaxAmount = 5_000m, Rate = 5m },
            new CommissionTierDto { MinAmount = 5_000m, MaxAmount = 10_000m, Rate = 10m }
        };

        // Act
        var result = _engine.CalculateTieredCommission(dealAmount, tiers);

        // Assert
        result.Should().Be(750m, "Tier1($250) + Tier2($500) = $750");
    }

    [Fact]
    public void CalculateTieredCommission_AmountInFirstTierOnly_ShouldNotApplyHigherTiers()
    {
        // Arrange — $3,000 deal only reaches tier 1 ($0–$5,000 at 5%) = $150
        decimal dealAmount = 3_000m;
        var tiers = new List<CommissionTierDto>
        {
            new CommissionTierDto { MinAmount = 0m, MaxAmount = 5_000m, Rate = 5m },
            new CommissionTierDto { MinAmount = 5_000m, MaxAmount = 15_000m, Rate = 12m }
        };

        // Act
        var result = _engine.CalculateTieredCommission(dealAmount, tiers);

        // Assert
        result.Should().Be(150m, "$3,000 × 5% = $150; higher tier not reached");
    }

    [Fact]
    public void CalculateTieredCommission_WithFixedAmountTier_ShouldUseFixedNotRate()
    {
        // Arrange — fixed $200 per tier entry (regardless of rate)
        decimal dealAmount = 5_000m;
        var tiers = new List<CommissionTierDto>
        {
            new CommissionTierDto { MinAmount = 0m, MaxAmount = 5_000m, Rate = 10m, FixedAmount = 200m }
        };

        // Act
        var result = _engine.CalculateTieredCommission(dealAmount, tiers);

        // Assert
        result.Should().Be(200m, "FixedAmount takes precedence over Rate when configured");
    }

    [Fact]
    public void CalculateTieredCommission_EmptyTiers_ShouldReturnZero()
    {
        // Arrange
        decimal dealAmount = 10_000m;
        var tiers = new List<CommissionTierDto>();

        // Act
        var result = _engine.CalculateTieredCommission(dealAmount, tiers);

        // Assert
        result.Should().Be(0m);
    }

    #endregion

    #region ApplyCap + CalculateSplit Interaction Tests

    [Fact]
    public void ApplyCap_ThenSplit_ShouldCapBeforeDistributing()
    {
        // Arrange — high commission capped then split 50/50
        decimal rawCommission = 10_000m;
        decimal cap = 4_000m;
        var splits = new Dictionary<int, decimal> { { 1, 50m }, { 2, 50m } };

        // Act
        var capped = _engine.ApplyCap(rawCommission, cap);
        var result = _engine.CalculateSplit(capped, splits);

        // Assert
        capped.Should().Be(4_000m);
        result[1].Should().Be(2_000m);
        result[2].Should().Be(2_000m);
    }

    #endregion
}

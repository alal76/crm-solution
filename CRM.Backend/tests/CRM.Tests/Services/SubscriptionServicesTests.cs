// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive Unit Tests for ProrateCalculator.
/// 
/// Tests all 4 proration algorithms with edge cases:
/// - Pro-Rata: Time-based calculation
/// - Full Price: No adjustment
/// - One Month: Full month always
/// - None: Difference only
/// 
/// Edge cases covered:
/// - Leap year (29 Feb)
/// - Month-end transitions
/// - Single-day billing
/// - Mid-cycle changes
/// </summary>
public class ProrateCalculatorTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ProrateCalculator>> _mockLogger;
    private readonly ProrateCalculator _calculator;

    public ProrateCalculatorTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ProrateCalculator>>();
        _calculator = new ProrateCalculator(_mockContext.Object, _mockLogger.Object);
    }

    #region Pro-Rata Algorithm Tests

    [Fact]
    public void CalculateProRata_Day10Of30_Returns33Percent()
    {
        // Arrange: $100/month, used 10 days out of 30
        var amount = 100m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2026, 1, 10);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: 10 days / 30 days * $100 = $33.33
        Assert.Equal(33.33m, result);
    }

    [Fact]
    public void CalculateProRata_Day1Of30_ReturnsSmallAmount()
    {
        // Arrange: $100/month,used 1 day out of 30
        var amount = 100m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2026, 1, 1);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: 1 day / 30 days * $100 = $3.33
        Assert.Equal(3.33m, result);
    }

    [Fact]
    public void CalculateProRata_LeapYear_Feb29()
    {
        // Arrange: $100/month for Feb in leap year (29 days)
        var amount = 100m;
        var cycleStart = new DateTime(2024, 2, 1);
        var cycleEnd = new DateTime(2024, 2, 29);
        var changeDate = new DateTime(2024, 2, 15);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: ~15 days / 29 days * $100 ≈ $51.72
        Assert.InRange(result, 51.70m, 51.75m);
    }

    [Fact]
    public void CalculateProRata_ChangeDateAfterCycleEnd_ReturnsFull()
    {
        // Arrange: changeDate after cycle end
        var amount = 100m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2026, 2, 1); // After end

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: Should cap at full amount
        Assert.Equal(100.00m, result);
    }

    [Fact]
    public void CalculateProRata_MonthWithVaryingDays()
    {
        // Arrange: April (30 days)
        var amount = 150m;
        var cycleStart = new DateTime(2026, 4, 1);
        var cycleEnd = new DateTime(2026, 4, 30);
        var changeDate = new DateTime(2026, 4, 20);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: 20 days / 30 days * $150 = $100
        Assert.Equal(100.00m, result);
    }

    #endregion

    #region Full Price Algorithm Tests

    [Fact]
    public void CalculateFullPrice_AlwaysReturnsFull()
    {
        // Arrange
        var amount = 250m;

        // Act
        var result = _calculator.CalculateFullPrice(amount);

        // Assert
        Assert.Equal(250m, result);
    }

    [Fact]
    public void CalculateFullPrice_ZeroAmount()
    {
        // Arrange
        var amount = 0m;

        // Act
        var result = _calculator.CalculateFullPrice(amount);

        // Assert
        Assert.Equal(0m, result);
    }

    #endregion

    #region One Month Algorithm Tests

    [Fact]
    public void CalculateOneMonth_AlwaysReturnsFull()
    {
        // Arrange
        var monthlyAmount = 99.99m;

        // Act
        var result = _calculator.CalculateOneMonth(monthlyAmount);

        // Assert
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public void CalculateOneMonth_LargeAmount()
    {
        // Arrange
        var monthlyAmount = 5000m;

        // Act
        var result = _calculator.CalculateOneMonth(monthlyAmount);

        // Assert
        Assert.Equal(5000m, result);
    }

    #endregion

    #region None Algorithm Tests

    [Fact]
    public void CalculateNone_Upgrade_ChargesDifference()
    {
        // Arrange: Upgrade from $50 to $100
        var oldAmount = 50m;
        var newAmount = 100m;

        // Act
        var result = _calculator.CalculateNone(oldAmount, newAmount);

        // Assert: $100 - $50 = $50 charge
        Assert.Equal(50m, result);
    }

    [Fact]
    public void CalculateNone_Downgrade_NoCharge()
    {
        // Arrange: Downgrade from $100 to $50
        var oldAmount = 100m;
        var newAmount = 50m;

        // Act
        var result = _calculator.CalculateNone(oldAmount, newAmount);

        // Assert: Downgrade → credit (but method returns 0 for negative)
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateNone_SameAmount_NoCharge()
    {
        // Arrange
        var amount = 75m;

        // Act
        var result = _calculator.CalculateNone(amount, amount);

        // Assert
        Assert.Equal(0m, result);
    }

    #endregion

    #region Precision Tests

    [Fact]
    public void CalculateProRata_HighPrecision_RoundsCorrectly()
    {
        // Arrange: Test floating point precision
        var amount = 123.45m;
        var cycleStart = new DateTime(2026, 2, 1);
        var cycleEnd = new DateTime(2026, 2, 28);
        var changeDate = new DateTime(2026, 2, 14);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: Result should be rounded to 2 decimal places
        Assert.Equal(result, Math.Round(result, 2));
    }

    [Fact]
    public void CalculateProRata_VerySmallAmount()
    {
        // Arrange: $0.01/month for 1 day
        var amount = 0.01m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2026, 1, 1);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: Should still calculate correctly
        Assert.True(result >= 0);
        Assert.True(result <= amount);
    }

    [Fact]
    public void CalculateProRata_LargeAmount()
    {
        // Arrange: $10,000/month
        var amount = 10000m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2026, 1, 15);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: ~15/30 * 10000 = ~5000
        Assert.InRange(result, 4900m, 5100m);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void CalculateProRata_BeforeCycleStart_ReturnsZero()
    {
        // Arrange: changeDate before cycle start
        var amount = 100m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 31);
        var changeDate = new DateTime(2025, 12, 31);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateProRata_SingleDayCycle()
    {
        // Arrange: 1-day cycle
        var amount = 100m;
        var cycleStart = new DateTime(2026, 1, 1);
        var cycleEnd = new DateTime(2026, 1, 1);
        var changeDate = new DateTime(2026, 1, 1);

        // Act
        var result = _calculator.CalculateProRata(amount, cycleStart, cycleEnd, changeDate);

        // Assert: 1 day / 1 day = full amount
        Assert.Equal(100m, result);
    }

    #endregion
}

/// <summary>
/// Comprehensive Unit Tests for SubscriptionMetricsAggregator.
/// 
/// Tests calculations for:
/// - MRR (Monthly Recurring Revenue)
/// - ARR (Annual Recurring Revenue)
/// - Churn Rate (monthly cancellations)
/// - NRR (Net Revenue Retention)
/// - LTV (Customer Lifetime Value)
/// </summary>
public class SubscriptionMetricsAggregatorTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionMetricsAggregator>> _mockLogger;
    private readonly SubscriptionMetricsAggregator _aggregator;

    public SubscriptionMetricsAggregatorTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SubscriptionMetricsAggregator>>();
        _aggregator = new SubscriptionMetricsAggregator(_mockContext.Object, _mockLogger.Object);
    }

    #region MRR Calculation Tests

    [Fact]
    public void NormalizeToMonthly_Monthly_ReturnsSame()
    {
        // Arrange
        var amount = 100m;

        // Act: Reflect and invoke private method
        var method = typeof(SubscriptionMetricsAggregator).GetMethod("NormalizeToMonthly", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (decimal)method.Invoke(null, new object[] { amount, "Monthly" });

        // Assert
        Assert.Equal(100m, result);
    }

    [Fact]
    public void NormalizeToMonthly_Quarterly_DividesBy3()
    {
        // Arrange
        var amount = 300m; // $300/quarter

        // Act
        var method = typeof(SubscriptionMetricsAggregator).GetMethod("NormalizeToMonthly",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (decimal)method.Invoke(null, new object[] { amount, "Quarterly" });

        // Assert: $300/3 = $100/month
        Assert.Equal(100m, result);
    }

    [Fact]
    public void NormalizeToMonthly_Yearly_DividesBy12()
    {
        // Arrange
        var amount = 1200m; // $1200/year

        // Act
        var method = typeof(SubscriptionMetricsAggregator).GetMethod("NormalizeToMonthly",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (decimal)method.Invoke(null, new object[] { amount, "Yearly" });

        // Assert: $1200/12 = $100/month
        Assert.Equal(100m, result);
    }

    [Fact]
    public void NormalizeToMonthly_Weekly_MultipliesBy52Div12()
    {
        // Arrange
        var amount = 100m; // $100/week

        // Act
        var method = typeof(SubscriptionMetricsAggregator).GetMethod("NormalizeToMonthly",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (decimal)method.Invoke(null, new object[] { amount, "Weekly" });

        // Assert: $100 * 52 / 12 ≈ $433.33/month
        var expected = Math.Round(100 * 52m / 12, 4);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        // Arrange & Act
        var context = new Mock<ICrmDbContext>();
        var logger = new Mock<ILogger<SubscriptionMetricsAggregator>>();

        // Assert: No exception
        var aggregator = new SubscriptionMetricsAggregator(context.Object, logger.Object);
        Assert.NotNull(aggregator);
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<SubscriptionMetricsAggregator>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new SubscriptionMetricsAggregator(null!, logger.Object));
    }

    #endregion
}

/// <summary>
/// Integration Tests for Billing and Dunning Services.
/// These would require a test database context and are marked for integration testing.
/// </summary>
public class SubscriptionBillingIntegrationTests
{
    [Fact(Skip = "Requires test database context")]
    public void RecurringBillingEngine_ProcessBillingCycles_CreatesInvoices()
    {
        // This test would require:
        // 1. In-memory database with test subscriptions
        // 2. Invoice service mock/integration
        // 3. Verification of invoice creation
    }

    [Fact(Skip = "Requires test database context")]
    public void DunningManager_HandlePaymentFailure_CreatesInitialRecord()
    {
        // This test would require:
        // 1. In-memory database
        // 2. Invoice and subscription fixtures
        // 3. Verification of DunningRecord creation
    }
}

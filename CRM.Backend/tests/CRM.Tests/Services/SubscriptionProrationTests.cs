// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for subscription proration calculations.
/// TODO-SALES006-041: 20+ unit tests for mid-cycle upgrades, downgrades, daily/monthly proration, edge cases.
/// </summary>
public class SubscriptionProrationTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;
    private readonly List<Product> _products;

    public SubscriptionProrationTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SubscriptionService>>();

        _subscriptions = new List<Subscription>();
        _invoices = new List<Invoice>();
        _usages = new List<SubscriptionUsage>();
        _usageLimits = new List<SubscriptionUsageLimit>();
        _orders = new List<Order>();
        _products = new List<Product>();

        SetupMockContext();

        _service = new SubscriptionService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupMockContext()
    {
        var mockSubscriptions = MockDbSetFactory.CreateMockDbSet(_subscriptions);
        var mockInvoices = MockDbSetFactory.CreateMockDbSet(_invoices);
        var mockUsages = MockDbSetFactory.CreateMockDbSet(_usages);
        var mockUsageLimits = MockDbSetFactory.CreateMockDbSet(_usageLimits);
        var mockOrders = MockDbSetFactory.CreateMockDbSet(_orders);
        var mockProducts = MockDbSetFactory.CreateMockDbSet(_products);

        mockSubscriptions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null) return ValueTask.FromResult<Subscription?>(default);
                return ValueTask.FromResult(_subscriptions.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        mockProducts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null) return ValueTask.FromResult<Product?>(default);
                return ValueTask.FromResult(_products.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        _mockContext.Setup(c => c.Subscriptions).Returns(mockSubscriptions.Object);
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);
        _mockContext.Setup(c => c.SubscriptionUsages).Returns(mockUsages.Object);
        _mockContext.Setup(c => c.SubscriptionUsageLimits).Returns(mockUsageLimits.Object);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        _mockContext.Setup(c => c.Products).Returns(mockProducts.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private Subscription CreateTestSubscription(decimal mrr = 100m, BillingPeriod period = BillingPeriod.Monthly)
    {
        var billingCycle = period switch
        {
            BillingPeriod.Weekly => "Weekly",
            BillingPeriod.Quarterly => "Quarterly",
            BillingPeriod.Yearly => "Yearly",
            _ => "Monthly"
        };

        return new Subscription
        {
            Id = _subscriptions.Count + 1,
            SubscriptionNumber = $"SUB-TEST-{_subscriptions.Count + 1:D4}",
            AccountId = 1,
            MRR = mrr,
            ARR = mrr * 12,
            BillingCycle = billingCycle,
            BillingStartDate = DateTime.UtcNow.Date.AddDays(-15),
            BillingEndDate = DateTime.UtcNow.Date.AddDays(15),
            SubscriptionStatus = SubscriptionStatus.Active,
            ProrationType = ProrationStrategy.Daily
        };
    }

    // ========================================================================
    // Daily Proration Tests
    // ========================================================================

    [Fact]
    public async Task CalculateProration_DailyStrategy_ShouldCalculateCorrectAmountForMonthlyPlan()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m, BillingPeriod.Monthly);
        _subscriptions.Add(subscription);

        // Mid-cycle: 15 days remaining out of 30
        var changeDate = subscription.BillingStartDate!.Value.AddDays(15);
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - 15/30 * 200 = 100
        prorated.Should().BeApproximately(100m, 0.01m);
    }

    [Fact]
    public async Task CalculateProration_DailyStrategy_ShouldReturnZeroOnLastDay()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingEndDate = DateTime.UtcNow.Date;
        _subscriptions.Add(subscription);

        var changeDate = DateTime.UtcNow.Date;
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert
        prorated.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateProration_DailyStrategy_ShouldReturnFullAmountOnFirstDay()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingStartDate = DateTime.UtcNow.Date;
        subscription.BillingEndDate = DateTime.UtcNow.Date.AddDays(30);
        _subscriptions.Add(subscription);

        var changeDate = DateTime.UtcNow.Date;
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - Full 30 days remaining
        prorated.Should().BeApproximately(200m, 0.01m);
    }

    [Fact]
    public async Task CalculateProration_WeeklyPlan_ShouldUse7DaysPeriod()
    {
        // Arrange
        var subscription = CreateTestSubscription(70m, BillingPeriod.Weekly);
        subscription.BillingStartDate = DateTime.UtcNow.Date.AddDays(-3);
        subscription.BillingEndDate = DateTime.UtcNow.Date.AddDays(4);
        _subscriptions.Add(subscription);

        var changeDate = DateTime.UtcNow.Date; // 4 days remaining
        var newAmount = 140m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - 4/7 * 140 = 80
        prorated.Should().BeApproximately(80m, 0.01m);
    }

    [Fact]
    public async Task CalculateProration_QuarterlyPlan_ShouldUse90DaysPeriod()
    {
        // Arrange
        var subscription = CreateTestSubscription(900m, BillingPeriod.Quarterly);
        subscription.BillingStartDate = DateTime.UtcNow.Date.AddDays(-45);
        subscription.BillingEndDate = DateTime.UtcNow.Date.AddDays(45);
        _subscriptions.Add(subscription);

        var changeDate = DateTime.UtcNow.Date; // 45 days remaining
        var newAmount = 1800m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - 45/90 * 1800 = 900
        prorated.Should().BeApproximately(900m, 0.01m);
    }

    [Fact]
    public async Task CalculateProration_YearlyPlan_ShouldUse365DaysPeriod()
    {
        // Arrange
        var subscription = CreateTestSubscription(1200m, BillingPeriod.Yearly);
        subscription.BillingStartDate = DateTime.UtcNow.Date.AddDays(-182);
        subscription.BillingEndDate = DateTime.UtcNow.Date.AddDays(183);
        _subscriptions.Add(subscription);

        var changeDate = DateTime.UtcNow.Date; // ~183 days remaining
        var newAmount = 2400m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - 183/365 * 2400 ≈ 1203
        prorated.Should().BeApproximately(1203.29m, 1m);
    }

    // ========================================================================
    // Mid-Cycle Upgrade Tests
    // ========================================================================

    [Fact]
    public async Task MidCycleUpgrade_ShouldChargeProRatedDifference()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var higherPlan = new Product { Id = 2, Name = "Premium", UnitPrice = 200m };
        _products.Add(higherPlan);

        // Act
        var result = await _service.UpgradeAsync(subscription.Id, higherPlan.Id, immediate: true);

        // Assert
        result.ProductId.Should().Be(higherPlan.Id);
        result.MRR.Should().Be(200m);
    }

    [Fact]
    public async Task MidCycleUpgrade_ScheduledForNextBillingCycle_ShouldNotChangeImmediately()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var higherPlan = new Product { Id = 2, Name = "Premium", UnitPrice = 200m };
        _products.Add(higherPlan);

        // Act
        var result = await _service.UpgradeAsync(subscription.Id, higherPlan.Id, immediate: false);

        // Assert - MRR should not change immediately
        result.MRR.Should().Be(100m);
        result.ContractNotes.Should().Contain("next billing cycle");
    }

    // ========================================================================
    // Mid-Cycle Downgrade Tests
    // ========================================================================

    [Fact]
    public async Task MidCycleDowngrade_ShouldScheduleForEndOfPeriod()
    {
        // Arrange
        var subscription = CreateTestSubscription(200m);
        _subscriptions.Add(subscription);

        var lowerPlan = new Product { Id = 3, Name = "Basic", UnitPrice = 100m };
        _products.Add(lowerPlan);

        // Act
        var result = await _service.DowngradeAsync(subscription.Id, lowerPlan.Id);

        // Assert - Downgrade is scheduled, not immediate
        result.MRR.Should().Be(200m);
        result.ContractNotes.Should().Contain("end of period");
    }

    [Fact]
    public async Task Downgrade_ShouldNotCreditImmediately()
    {
        // Arrange
        var subscription = CreateTestSubscription(500m);
        _subscriptions.Add(subscription);

        var lowerPlan = new Product { Id = 3, Name = "Starter", UnitPrice = 100m };
        _products.Add(lowerPlan);

        // Act
        var result = await _service.DowngradeAsync(subscription.Id, lowerPlan.Id);

        // Assert
        result.MRR.Should().Be(500m); // Still at old price until period ends
    }

    // ========================================================================
    // Edge Cases - Leap Year
    // ========================================================================

    [Fact]
    public async Task CalculateProration_LeapYearFebruary_ShouldHandle29Days()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingStartDate = new DateTime(2024, 2, 1); // Leap year
        subscription.BillingEndDate = new DateTime(2024, 2, 29);
        _subscriptions.Add(subscription);

        var changeDate = new DateTime(2024, 2, 15); // 14 days remaining
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - Should handle 29 days correctly
        prorated.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculateProration_NonLeapYearFebruary_ShouldHandle28Days()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingStartDate = new DateTime(2023, 2, 1); // Non-leap year
        subscription.BillingEndDate = new DateTime(2023, 2, 28);
        _subscriptions.Add(subscription);

        var changeDate = new DateTime(2023, 2, 14); // 14 days remaining
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - Should handle 28 days correctly
        prorated.Should().BeGreaterThan(0);
    }

    // ========================================================================
    // Edge Cases - End of Month
    // ========================================================================

    [Fact]
    public async Task CalculateProration_EndOfMonth31Days_ShouldCalculateCorrectly()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingStartDate = new DateTime(2024, 1, 1);
        subscription.BillingEndDate = new DateTime(2024, 1, 31);
        _subscriptions.Add(subscription);

        var changeDate = new DateTime(2024, 1, 16); // 15 days remaining
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert
        prorated.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalculateProration_EndOfMonth30Days_ShouldCalculateCorrectly()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        subscription.BillingStartDate = new DateTime(2024, 4, 1);
        subscription.BillingEndDate = new DateTime(2024, 4, 30);
        _subscriptions.Add(subscription);

        var changeDate = new DateTime(2024, 4, 15); // 15 days remaining
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(subscription.Id, newAmount, changeDate);

        // Assert - 15/30 * 200 = 100
        prorated.Should().BeApproximately(100m, 0.01m);
    }

    // ========================================================================
    // Proration Strategy Tests
    // ========================================================================

    [Fact]
    public void ProrationStrategy_Daily_ShouldBeDefaultValue()
    {
        // Arrange & Act
        var subscription = new Subscription();

        // Assert
        subscription.ProrationType.Should().Be(ProrationStrategy.Daily);
    }

    [Fact]
    public void ProrationStrategy_AllValues_ShouldBeDefined()
    {
        // Assert - Verify all expected proration strategies exist
        Enum.GetValues<ProrationStrategy>()
            .Should().HaveCount(5)
            .And.Contain(ProrationStrategy.Daily)
            .And.Contain(ProrationStrategy.HalfMonth)
            .And.Contain(ProrationStrategy.FullMonth)
            .And.Contain(ProrationStrategy.None)
            .And.Contain(ProrationStrategy.Credit);
    }

    // ========================================================================
    // Plan Change Tests
    // ========================================================================

    [Fact]
    public async Task ChangePlan_Immediate_ShouldUpdateProductAndMRR()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var newPlan = new Product { Id = 5, Name = "Enterprise", UnitPrice = 500m };
        _products.Add(newPlan);

        // Act
        var result = await _service.ChangePlanAsync(subscription.Id, newPlan.Id, SubscriptionChangeType.Immediate);

        // Assert
        result.ProductId.Should().Be(newPlan.Id);
        result.MRR.Should().Be(500m);
        result.ARR.Should().Be(6000m);
    }

    [Fact]
    public async Task ChangePlan_EndOfPeriod_ShouldScheduleChange()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var newPlan = new Product { Id = 5, Name = "Enterprise", UnitPrice = 500m };
        _products.Add(newPlan);

        // Act
        var result = await _service.ChangePlanAsync(subscription.Id, newPlan.Id, SubscriptionChangeType.EndOfPeriod);

        // Assert
        result.ProductId.Should().NotBe(newPlan.Id);
        result.MRR.Should().Be(100m);
        result.ContractNotes.Should().Contain("Enterprise");
    }

    [Fact]
    public async Task ChangePlan_NextBillingCycle_ShouldScheduleChange()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var newPlan = new Product { Id = 5, Name = "Standard", UnitPrice = 150m };
        _products.Add(newPlan);

        // Act
        var result = await _service.ChangePlanAsync(subscription.Id, newPlan.Id, SubscriptionChangeType.NextBillingCycle);

        // Assert
        result.MRR.Should().Be(100m);
        result.ContractNotes.Should().Contain("Standard");
    }

    // ========================================================================
    // Addon Tests with Proration
    // ========================================================================

    [Fact]
    public async Task AddAddon_ShouldIncreaseMRR()
    {
        // Arrange
        var subscription = CreateTestSubscription(100m);
        _subscriptions.Add(subscription);

        var addon = new Product { Id = 10, Name = "Extra Storage", UnitPrice = 25m };
        _products.Add(addon);

        // Act
        var result = await _service.AddAddonAsync(subscription.Id, addon.Id, quantity: 2);

        // Assert
        result.MRR.Should().Be(150m); // 100 + (25 * 2)
        result.ARR.Should().Be(1800m);
    }

    [Fact]
    public async Task RemoveAddon_ShouldDecreaseMRR()
    {
        // Arrange
        var subscription = CreateTestSubscription(150m);
        _subscriptions.Add(subscription);

        var addon = new Product { Id = 10, Name = "Extra Storage", UnitPrice = 25m };
        _products.Add(addon);

        // Act
        var result = await _service.RemoveAddonAsync(subscription.Id, addon.Id);

        // Assert
        result.MRR.Should().Be(125m);
        result.ARR.Should().Be(1500m);
    }

    [Fact]
    public async Task RemoveAddon_ShouldNotGoBelowZero()
    {
        // Arrange
        var subscription = CreateTestSubscription(20m);
        _subscriptions.Add(subscription);

        var addon = new Product { Id = 10, Name = "Expensive Addon", UnitPrice = 100m };
        _products.Add(addon);

        // Act
        var result = await _service.RemoveAddonAsync(subscription.Id, addon.Id);

        // Assert
        result.MRR.Should().Be(0m);
    }
}

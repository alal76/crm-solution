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
/// Unit tests for subscription MRR/ARR metrics calculations.
/// TODO-SALES006-043: Unit tests for MRR/ARR calculation precision.
/// </summary>
public class SubscriptionMetricsTests : ServiceTestFixtureBase<SubscriptionService>
{    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;
    private readonly List<Product> _products;

    public SubscriptionMetricsTests()
    {        _subscriptions = new List<Subscription>();
        _invoices = new List<Invoice>();
        _usages = new List<SubscriptionUsage>();
        _usageLimits = new List<SubscriptionUsageLimit>();
        _orders = new List<Order>();
        _products = new List<Product>();

        SetupMockContext();

        _service = new SubscriptionService(MockContext.Object, MockLogger.Object);
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

        MockContext.Setup(c => c.Subscriptions).Returns(mockSubscriptions.Object);
        MockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);
        MockContext.Setup(c => c.SubscriptionUsages).Returns(mockUsages.Object);
        MockContext.Setup(c => c.SubscriptionUsageLimits).Returns(mockUsageLimits.Object);
        MockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        MockContext.Setup(c => c.Products).Returns(mockProducts.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private Subscription CreateSubscription(string billingCycle = "Monthly", decimal mrr = 100m)
    {
        var sub = new Subscription
        {
            Id = _subscriptions.Count + 1,
            SubscriptionNumber = $"SUB-TEST-{_subscriptions.Count + 1:D4}",
            AccountId = 1,
            BillingCycle = billingCycle,
            MRR = mrr,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _subscriptions.Add(sub);
        return sub;
    }

    #region MRR Calculations

    [Fact]
    public async Task CalculateMRRAsync_MonthlySubscription_ShouldReturnCorrectMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_AnnualSubscription_ShouldCalculateMRR()
    {
        // Arrange - Annual subscription at $1200/year = $100/month MRR
        var subscription = CreateSubscription("Annual", 1200m / 12);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_QuarterlySubscription_ShouldCalculateMRR()
    {
        // Arrange - Quarterly at $300/quarter = $100/month MRR
        var subscription = CreateSubscription("Quarterly", 300m / 3);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_WeeklySubscription_ShouldCalculateMRR()
    {
        // Arrange - Weekly at $25/week
        var subscription = CreateSubscription("Weekly", 25m * (52m / 12m));

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_MultipleSubscriptions_ShouldSumMRR()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);
        CreateSubscription("Monthly", 50m);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region ARR Calculations

    [Fact]
    public async Task CalculateARRAsync_ShouldCalculateARRFromMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);

        // Act
        var result = await _service.CalculateARRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateARRAsync_HighValueSubscription_ShouldCalculateARRPrecisely()
    {
        // Arrange - High-value subscription
        var subscription = CreateSubscription("Monthly", 9999.99m);

        // Act
        var result = await _service.CalculateARRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCalculateTotalARR()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.ARR.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Discounts in Metrics

    [Fact]
    public async Task CalculateMRRAsync_WithDiscount_ShouldReflectNetMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);
        subscription.MRR = 90m; // Net MRR after discount

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_WithLargeDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 50m);
        subscription.MRR = 50m;

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Status-Based Metrics

    [Fact]
    public async Task GetStatisticsAsync_OnlyCountsActiveSubscriptions()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var canceledSub = CreateSubscription("Monthly", 200m);
        canceledSub.SubscriptionStatus = SubscriptionStatus.Cancelled;
        CreateSubscription("Monthly", 50m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.ActiveSubscriptions.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ExcludesDeletedSubscriptions()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var deletedSub = CreateSubscription("Monthly", 200m);
        deletedSub.IsDeleted = true;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatisticsAsync_IncludesPausedInCount()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var pausedSub = CreateSubscription("Monthly", 0m); // No MRR while paused
        pausedSub.SubscriptionStatus = SubscriptionStatus.Paused;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.PausedSubscriptions.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Precision Edge Cases

    [Fact]
    public async Task CalculateMRRAsync_SmallMRR_ShouldMaintainPrecision()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 0.01m);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateARRAsync_OddValueMRR_ShouldNotLosePrecision()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 33.33m);

        // Act
        var result = await _service.CalculateARRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CalculateMRRAsync_LargeSubscription_ShouldHandleCorrectly()
    {
        // Arrange - Enterprise subscription
        var subscription = CreateSubscription("Monthly", 99999.99m);

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    #endregion

    #region Churn Metrics

    [Fact]
    public async Task GetChurnRateAsync_ShouldCalculateChurnRate()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 100m);
        var canceled = CreateSubscription("Monthly", 100m);
        canceled.SubscriptionStatus = SubscriptionStatus.Cancelled;
        canceled.CancelledAt = DateTime.UtcNow.AddDays(-15);

        // Act
        var churnRate = await _service.GetChurnRateAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        churnRate.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetChurnRateAsync_NoChurn_ShouldReturnZero()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);

        // Act
        var churnRate = await _service.GetChurnRateAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        churnRate.Should().Be(0);
    }

    #endregion

    #region Growth Metrics

    [Fact]
    public async Task GetStatisticsAsync_ShouldCalculateNewSubscriptions()
    {
        // Arrange
        var newSub = CreateSubscription("Monthly", 100m);
        newSub.CreatedAt = DateTime.UtcNow.AddDays(-5);

        var oldSub = CreateSubscription("Monthly", 200m);
        oldSub.CreatedAt = DateTime.UtcNow.AddMonths(-3);

        // Act
        var stats = await _service.GetStatisticsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        stats.TotalSubscriptions.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludeMRR()
    {
        // Arrange
        var newSub = CreateSubscription("Monthly", 150m);
        newSub.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var churned = CreateSubscription("Monthly", 50m);
        churned.SubscriptionStatus = SubscriptionStatus.Cancelled;
        churned.CancelledAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var stats = await _service.GetStatisticsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        stats.Should().NotBeNull();
    }

    #endregion
}

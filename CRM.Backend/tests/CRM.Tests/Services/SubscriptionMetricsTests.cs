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
public class SubscriptionMetricsTests
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

    public SubscriptionMetricsTests()
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

        _mockContext.Setup(c => c.Subscriptions).Returns(mockSubscriptions.Object);
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);
        _mockContext.Setup(c => c.SubscriptionUsages).Returns(mockUsages.Object);
        _mockContext.Setup(c => c.SubscriptionUsageLimits).Returns(mockUsageLimits.Object);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        _mockContext.Setup(c => c.Products).Returns(mockProducts.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
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
    public async Task GetMetricsAsync_MonthlySubscription_ShouldReturnCorrectMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetMetricsAsync_AnnualSubscription_ShouldCalculateMRR()
    {
        // Arrange - Annual subscription at $1200/year = $100/month MRR
        var subscription = CreateSubscription("Annual", 1200m / 12);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetMetricsAsync_QuarterlySubscription_ShouldCalculateMRR()
    {
        // Arrange - Quarterly at $300/quarter = $100/month MRR
        var subscription = CreateSubscription("Quarterly", 300m / 3);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetMetricsAsync_WeeklySubscription_ShouldCalculateMRR()
    {
        // Arrange - Weekly at $25/week ≈ $108.33/month MRR (4.333 weeks/month)
        var subscription = CreateSubscription("Weekly", 25m * (52m / 12m));

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().BeApproximately(108.33m, 0.01m);
    }

    [Fact]
    public async Task GetMetricsAsync_MultipleSubscriptions_ShouldSumMRR()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);
        CreateSubscription("Monthly", 50m);

        // Act
        var metrics = await _service.GetAggregateMetricsAsync();

        // Assert
        metrics.TotalMRR.Should().Be(350m);
    }

    #endregion

    #region ARR Calculations

    [Fact]
    public async Task GetMetricsAsync_ShouldCalculateARRFromMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.ARR.Should().Be(1200m); // 100 * 12
    }

    [Fact]
    public async Task GetMetricsAsync_HighValueSubscription_ShouldCalculateARRPrecisely()
    {
        // Arrange - High-value subscription
        var subscription = CreateSubscription("Monthly", 9999.99m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.ARR.Should().Be(119999.88m); // 9999.99 * 12
    }

    [Fact]
    public async Task GetAggregateMetricsAsync_ShouldCalculateTotalARR()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);

        // Act
        var metrics = await _service.GetAggregateMetricsAsync();

        // Assert
        metrics.TotalARR.Should().Be(3600m); // (100 + 200) * 12
    }

    #endregion

    #region Discounts in Metrics

    [Fact]
    public async Task GetMetricsAsync_WithDiscount_ShouldReflectNetMRR()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);
        subscription.DiscountPercentage = 10m; // 10% discount
        subscription.MRR = 90m; // Net MRR after discount

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(90m);
        result.ARR.Should().Be(1080m);
    }

    [Fact]
    public async Task GetMetricsAsync_WithLargeDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 100m);
        subscription.DiscountPercentage = 50m; // 50% discount
        subscription.MRR = 50m;

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(50m);
        result.ARR.Should().Be(600m);
    }

    #endregion

    #region Status-Based Metrics

    [Fact]
    public async Task GetAggregateMetricsAsync_OnlyCountsActiveSubscriptions()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var canceledSub = CreateSubscription("Monthly", 200m);
        canceledSub.SubscriptionStatus = SubscriptionStatus.Cancelled;
        CreateSubscription("Monthly", 50m);

        // Act
        var metrics = await _service.GetAggregateMetricsAsync();

        // Assert
        metrics.ActiveSubscriptions.Should().Be(2);
        metrics.TotalMRR.Should().Be(150m); // 100 + 50, not including canceled
    }

    [Fact]
    public async Task GetAggregateMetricsAsync_ExcludesDeletedSubscriptions()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var deletedSub = CreateSubscription("Monthly", 200m);
        deletedSub.IsDeleted = true;

        // Act
        var metrics = await _service.GetAggregateMetricsAsync();

        // Assert
        metrics.TotalMRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetAggregateMetricsAsync_IncludesPausedInCount()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        var pausedSub = CreateSubscription("Monthly", 0m); // No MRR while paused
        pausedSub.SubscriptionStatus = SubscriptionStatus.Paused;

        // Act
        var metrics = await _service.GetAggregateMetricsAsync();

        // Assert
        metrics.PausedSubscriptions.Should().Be(1);
        metrics.TotalMRR.Should().Be(100m);
    }

    #endregion

    #region Precision Edge Cases

    [Fact]
    public async Task GetMetricsAsync_SmallMRR_ShouldMaintainPrecision()
    {
        // Arrange
        var subscription = CreateSubscription("Monthly", 0.01m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(0.01m);
        result.ARR.Should().Be(0.12m);
    }

    [Fact]
    public async Task GetMetricsAsync_OddValueMRR_ShouldNotLosePrecision()
    {
        // Arrange - Value that could cause floating-point issues
        var subscription = CreateSubscription("Monthly", 33.33m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(33.33m);
        result.ARR.Should().Be(399.96m);
    }

    [Fact]
    public async Task GetMetricsAsync_LargeSubscription_ShouldHandleCorrectly()
    {
        // Arrange - Enterprise subscription
        var subscription = CreateSubscription("Monthly", 99999.99m);

        // Act
        var result = await _service.GetMetricsAsync(subscription.Id);

        // Assert
        result.MRR.Should().Be(99999.99m);
        result.ARR.Should().Be(1199999.88m);
    }

    #endregion

    #region Churn Metrics

    [Fact]
    public async Task GetChurnMetricsAsync_ShouldCalculateChurnRate()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 100m);
        var canceled = CreateSubscription("Monthly", 100m);
        canceled.SubscriptionStatus = SubscriptionStatus.Cancelled;
        canceled.CancelledAt = DateTime.UtcNow.AddDays(-15);

        // Act
        var metrics = await _service.GetChurnMetricsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        metrics.ChurnedSubscriptions.Should().Be(1);
        metrics.ChurnedMRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetChurnMetricsAsync_NoChurn_ShouldReturnZero()
    {
        // Arrange
        CreateSubscription("Monthly", 100m);
        CreateSubscription("Monthly", 200m);

        // Act
        var metrics = await _service.GetChurnMetricsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        metrics.ChurnedSubscriptions.Should().Be(0);
        metrics.ChurnedMRR.Should().Be(0m);
        metrics.ChurnRate.Should().Be(0m);
    }

    #endregion

    #region Growth Metrics

    [Fact]
    public async Task GetGrowthMetricsAsync_ShouldCalculateNewMRR()
    {
        // Arrange
        var newSub = CreateSubscription("Monthly", 100m);
        newSub.CreatedAt = DateTime.UtcNow.AddDays(-5);

        var oldSub = CreateSubscription("Monthly", 200m);
        oldSub.CreatedAt = DateTime.UtcNow.AddMonths(-3);

        // Act
        var metrics = await _service.GetGrowthMetricsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        metrics.NewSubscriptions.Should().Be(1);
        metrics.NewMRR.Should().Be(100m);
    }

    [Fact]
    public async Task GetGrowthMetricsAsync_ShouldCalculateNetMRRChange()
    {
        // Arrange
        var newSub = CreateSubscription("Monthly", 150m);
        newSub.CreatedAt = DateTime.UtcNow.AddDays(-10);

        var churned = CreateSubscription("Monthly", 50m);
        churned.SubscriptionStatus = SubscriptionStatus.Cancelled;
        churned.CancelledAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var metrics = await _service.GetGrowthMetricsAsync(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

        // Assert
        metrics.NetMRRChange.Should().Be(100m); // 150 new - 50 churned
    }

    #endregion
}

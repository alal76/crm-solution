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
/// Unit tests for subscription usage billing calculations.
/// TODO-SALES006-042: 15+ unit tests for tiered usage, overage, metered billing.
/// </summary>
public class SubscriptionUsageBillingTests : ServiceTestFixtureBase<SubscriptionService>
{    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;
    private readonly List<Product> _products;

    public SubscriptionUsageBillingTests()
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

    private Subscription CreateSubscription()
    {
        var sub = new Subscription
        {
            Id = _subscriptions.Count + 1,
            SubscriptionNumber = $"SUB-TEST-{_subscriptions.Count + 1:D4}",
            AccountId = 1,
            BillingCycle = "Monthly",
            MRR = 100m,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _subscriptions.Add(sub);
        return sub;
    }

    #region Recording Usage

    [Fact]
    public async Task RecordUsageAsync_ValidUsage_ShouldSucceed()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 100m);

        // Assert
        result.Should().BeTrue();
        _usages.Should().HaveCount(1);
        _usages.First().Quantity.Should().Be(100m);
    }

    [Fact]
    public async Task RecordUsageAsync_MultipleRecords_ShouldAccumulate()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 50m);
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 75m);
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 25m);

        // Assert
        _usages.Should().HaveCount(3);
        _usages.Sum(u => u.Quantity).Should().Be(150m);
    }

    [Fact]
    public async Task RecordUsageAsync_DifferentMetrics_ShouldTrackSeparately()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 100m);
        await _service.RecordUsageAsync(subscription.Id, "storage_gb", 50m);
        await _service.RecordUsageAsync(subscription.Id, "bandwidth_gb", 200m);

        // Assert
        _usages.Should().HaveCount(3);
        _usages.Where(u => u.MetricName == "api_calls").Sum(u => u.Quantity).Should().Be(100m);
        _usages.Where(u => u.MetricName == "storage_gb").Sum(u => u.Quantity).Should().Be(50m);
        _usages.Where(u => u.MetricName == "bandwidth_gb").Sum(u => u.Quantity).Should().Be(200m);
    }

    [Fact]
    public async Task RecordUsageAsync_ZeroQuantity_ShouldSucceed()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 0m);

        // Assert
        result.Should().BeTrue();
        _usages.First().Quantity.Should().Be(0m);
    }

    [Fact]
    public async Task RecordUsageAsync_WithTimestamp_ShouldUseProvidedTime()
    {
        // Arrange
        var subscription = CreateSubscription();
        var customTime = new DateTime(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 100m, customTime);

        // Assert
        _usages.First().Timestamp.Should().Be(customTime);
    }

    [Fact]
    public async Task RecordUsageAsync_WithoutTimestamp_ShouldUseCurrentTime()
    {
        // Arrange
        var subscription = CreateSubscription();
        var beforeTime = DateTime.UtcNow;

        // Act
        await _service.RecordUsageAsync(subscription.Id, "api_calls", 100m);

        // Assert
        _usages.First().Timestamp.Should().BeOnOrAfter(beforeTime);
    }

    [Fact]
    public async Task RecordUsageAsync_InvalidSubscription_ShouldThrow()
    {
        // Act
        var act = async () => await _service.RecordUsageAsync(999, "api_calls", 100m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Usage Limits

    [Fact]
    public async Task RecordUsageAsync_WithinLimit_ShouldSucceed()
    {
        // Arrange
        var subscription = CreateSubscription();
        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 1000m,
            EnforceCap = true,
            IsDeleted = false
        });

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 500m);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsageAsync_ExceedingEnforcedLimit_ShouldThrow()
    {
        // Arrange
        var subscription = CreateSubscription();
        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 100m,
            EnforceCap = true,
            IsDeleted = false
        });

        // Existing usage at 90
        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 90m,
            CreatedAt = DateTime.UtcNow
        });

        // Act - Try to add 20 more (would exceed 100 limit)
        var act = async () => await _service.RecordUsageAsync(subscription.Id, "api_calls", 20m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds*limit*");
    }

    [Fact]
    public async Task RecordUsageAsync_ExceedingNonEnforcedLimit_ShouldSucceed()
    {
        // Arrange
        var subscription = CreateSubscription();
        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 100m,
            EnforceCap = false, // Not enforced
            IsDeleted = false
        });

        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 90m,
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 20m);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Getting Usage Data

    [Fact]
    public async Task GetUsageAsync_WithRecords_ShouldReturnAggregatedData()
    {
        // Arrange
        var subscription = CreateSubscription();
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);

        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 100m,
            Timestamp = new DateTime(2026, 1, 10),
            CreatedAt = DateTime.UtcNow
        });
        _usages.Add(new SubscriptionUsage
        {
            Id = 2,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 50m,
            Timestamp = new DateTime(2026, 1, 20),
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.SubscriptionId.Should().Be(subscription.Id);
        result.Metrics.Should().HaveCount(1);
        result.Metrics.First().TotalUsage.Should().Be(150m);
    }

    [Fact]
    public async Task GetUsageAsync_NoRecords_ShouldReturnEmptyMetrics()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        // Assert
        result.Should().NotBeNull();
        result.Metrics.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsageAsync_MultipleMetrics_ShouldGroupCorrectly()
    {
        // Arrange
        var subscription = CreateSubscription();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        _usages.Add(new SubscriptionUsage { Id = 1, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 100m, Timestamp = new DateTime(2026, 1, 15), CreatedAt = DateTime.UtcNow });
        _usages.Add(new SubscriptionUsage { Id = 2, SubscriptionId = subscription.Id, MetricName = "storage_gb", Quantity = 50m, Timestamp = new DateTime(2026, 1, 15), CreatedAt = DateTime.UtcNow });
        _usages.Add(new SubscriptionUsage { Id = 3, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 200m, Timestamp = new DateTime(2026, 1, 20), CreatedAt = DateTime.UtcNow });

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, start, end);

        // Assert
        result.Metrics.Should().HaveCount(2);
        result.Metrics.First(m => m.MetricName == "api_calls").TotalUsage.Should().Be(300m);
        result.Metrics.First(m => m.MetricName == "storage_gb").TotalUsage.Should().Be(50m);
    }

    #endregion

    #region Usage Limits Query

    [Fact]
    public async Task GetUsageLimitsAsync_WithLimitsAndUsage_ShouldCalculateRemaining()
    {
        // Arrange
        var subscription = CreateSubscription();

        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 1000m,
            IsDeleted = false
        });

        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 300m,
            CreatedAt = DateTime.UtcNow
        });

        // Act
        var result = await _service.GetUsageLimitsAsync(subscription.Id);

        // Assert
        var limits = result.ToList();
        limits.Should().HaveCount(1);
        limits.First().Limit.Should().Be(1000m);
        limits.First().Used.Should().Be(300m);
        limits.First().Remaining.Should().Be(700m);
        limits.First().UsagePercentage.Should().BeApproximately(30, 0.1);
    }

    [Fact]
    public async Task GetUsageLimitsAsync_NoLimits_ShouldReturnEmpty()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        var result = await _service.GetUsageLimitsAsync(subscription.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region High Precision Usage Values

    [Fact]
    public async Task RecordUsageAsync_HighPrecisionValue_ShouldPreserve()
    {
        // Arrange
        var subscription = CreateSubscription();
        var preciseQuantity = 123.4567m; // 4 decimal places (precision 18,4)

        // Act
        await _service.RecordUsageAsync(subscription.Id, "cpu_hours", preciseQuantity);

        // Assert
        _usages.First().Quantity.Should().Be(123.4567m);
    }

    [Fact]
    public async Task RecordUsageAsync_VerySmallValue_ShouldHandleCorrectly()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        await _service.RecordUsageAsync(subscription.Id, "micro_transactions", 0.0001m);

        // Assert
        _usages.First().Quantity.Should().Be(0.0001m);
    }

    #endregion
}

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
/// Unit tests for MRR/ARR calculation precision.
/// TODO-SALES006-043: Verify CalculateMRRAsync, CalculateARRAsync, and GetStatisticsAsync
/// produce correct values under various subscription mix scenarios.
/// </summary>
public class SubscriptionMrrArrTests : ServiceTestFixtureBase<SubscriptionService>
{    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;
    private readonly List<Product> _products;

    public SubscriptionMrrArrTests()
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

    private Subscription CreateActiveSubscription(decimal mrr, string billingCycle = "Monthly", int accountId = 1, string? productName = null)
    {
        var sub = new Subscription
        {
            Id = _subscriptions.Count + 1,
            SubscriptionNumber = $"SUB-MRR-{_subscriptions.Count + 1:D4}",
            AccountId = accountId,
            MRR = mrr,
            ARR = mrr * 12,
            BillingCycle = billingCycle,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Product = productName != null ? new Product { Name = productName } : null
        };
        _subscriptions.Add(sub);
        return sub;
    }

    // ========================================================================
    // CalculateMRRAsync Tests
    // ========================================================================

    [Fact]
    public async Task CalculateMRR_NoSubscriptions_ShouldReturnZero()
    {
        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateMRR_SingleActiveSubscription_ShouldReturnMRR()
    {
        // Arrange
        CreateActiveSubscription(100m);

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateMRR_MultipleActiveSubscriptions_ShouldReturnSum()
    {
        // Arrange
        CreateActiveSubscription(100m);
        CreateActiveSubscription(200m);
        CreateActiveSubscription(350m);

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(650m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludeCancelledSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var cancelled = CreateActiveSubscription(200m);
        cancelled.SubscriptionStatus = SubscriptionStatus.Cancelled;

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludePausedSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(500m);
        var paused = CreateActiveSubscription(250m);
        paused.SubscriptionStatus = SubscriptionStatus.Paused;

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(500m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludeDeletedSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var deleted = CreateActiveSubscription(999m);
        deleted.IsDeleted = true;

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateMRR_NullMRR_ShouldTreatAsZero()
    {
        // Arrange
        var sub = CreateActiveSubscription(100m);
        var subNullMrr = CreateActiveSubscription(0m);
        subNullMrr.MRR = null;

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(100m);
    }

    [Fact]
    public async Task CalculateMRR_HighPrecisionValues_ShouldPreserve()
    {
        // Arrange
        CreateActiveSubscription(99.99m);
        CreateActiveSubscription(149.95m);
        CreateActiveSubscription(0.01m);

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(249.95m);
    }

    [Fact]
    public async Task CalculateMRR_LargeAmounts_ShouldHandleCorrectly()
    {
        // Arrange
        CreateActiveSubscription(50000m);
        CreateActiveSubscription(100000m);

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(150000m);
    }

    // ========================================================================
    // CalculateARRAsync Tests
    // ========================================================================

    [Fact]
    public async Task CalculateARR_ShouldBe12TimesMRR()
    {
        // Arrange
        CreateActiveSubscription(100m);
        CreateActiveSubscription(200m);

        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(3600m); // 300 * 12
    }

    [Fact]
    public async Task CalculateARR_NoSubscriptions_ShouldReturnZero()
    {
        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateARR_SingleSubscription_ShouldBeExact()
    {
        // Arrange
        CreateActiveSubscription(49.99m);

        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(599.88m);
    }

    // ========================================================================
    // GetStatisticsAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectMRRAndARR()
    {
        // Arrange
        CreateActiveSubscription(100m);
        CreateActiveSubscription(200m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.MRR.Should().Be(300m);
        stats.ARR.Should().Be(3600m);
    }

    [Fact]
    public async Task GetStatistics_ShouldCountActiveSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(100m);
        CreateActiveSubscription(200m);
        var cancelled = CreateActiveSubscription(50m);
        cancelled.SubscriptionStatus = SubscriptionStatus.Cancelled;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalSubscriptions.Should().Be(3);
        stats.ActiveSubscriptions.Should().Be(2);
        stats.CancelledSubscriptions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_ShouldCalculateAverageRevenuePerUser()
    {
        // Arrange
        CreateActiveSubscription(100m);
        CreateActiveSubscription(200m);
        CreateActiveSubscription(300m);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.AverageRevenuePerUser.Should().Be(200m); // 600 / 3
    }

    [Fact]
    public async Task GetStatistics_NoActiveSubscriptions_ShouldReturnZeroARPU()
    {
        // Arrange
        var cancelled = CreateActiveSubscription(100m);
        cancelled.SubscriptionStatus = SubscriptionStatus.Cancelled;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.ActiveSubscriptions.Should().Be(0);
        stats.AverageRevenuePerUser.Should().Be(0m);
    }

    [Fact]
    public async Task GetStatistics_ShouldCountTrialSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var trial = CreateActiveSubscription(0m);
        trial.SubscriptionStatus = SubscriptionStatus.Trial;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TrialSubscriptions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_ShouldCountPausedSubscriptions()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var paused = CreateActiveSubscription(50m);
        paused.SubscriptionStatus = SubscriptionStatus.Paused;

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.PausedSubscriptions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_ShouldGroupByPlan()
    {
        // Arrange
        CreateActiveSubscription(100m, productName: "Basic");
        CreateActiveSubscription(100m, productName: "Basic");
        CreateActiveSubscription(200m, productName: "Premium");

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.SubscriptionsByPlan.Should().ContainKey("Basic").WhoseValue.Should().Be(2);
        stats.SubscriptionsByPlan.Should().ContainKey("Premium").WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_WithDateRange_ShouldFilterSubscriptions()
    {
        // Arrange
        var old = CreateActiveSubscription(100m);
        old.CreatedAt = DateTime.UtcNow.AddMonths(-3);

        var recent = CreateActiveSubscription(200m);
        recent.CreatedAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var stats = await _service.GetStatisticsAsync(
            fromDate: DateTime.UtcNow.AddDays(-7),
            toDate: DateTime.UtcNow);

        // Assert
        stats.TotalSubscriptions.Should().Be(1);
    }

    // ========================================================================
    // Churn Rate Tests
    // ========================================================================

    [Fact]
    public async Task GetChurnRate_NoPriorSubscriptions_ShouldReturnZero()
    {
        // Act
        var churn = await _service.GetChurnRateAsync(
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow);

        // Assert
        churn.Should().Be(0);
    }

    [Fact]
    public async Task GetChurnRate_WithChurnedSubscriptions_ShouldCalculatePercentage()
    {
        // Arrange - 4 active subscriptions created 2 months ago
        for (int i = 0; i < 4; i++)
        {
            var sub = CreateActiveSubscription(100m);
            sub.CreatedAt = DateTime.UtcNow.AddMonths(-2);
        }

        // 1 cancelled this month
        var cancelled = CreateActiveSubscription(100m);
        cancelled.CreatedAt = DateTime.UtcNow.AddMonths(-2);
        cancelled.SubscriptionStatus = SubscriptionStatus.Cancelled;
        cancelled.UpdatedAt = DateTime.UtcNow.AddDays(-5);

        // Act
        var churn = await _service.GetChurnRateAsync(
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow);

        // Assert - 1 churned out of 5 = 20%
        churn.Should().BeApproximately(20, 0.1);
    }

    // ========================================================================
    // MRR Consistency After Operations
    // ========================================================================

    [Fact]
    public async Task AddAddon_ShouldIncreaseMRRCorrectly()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var sub = CreateActiveSubscription(200m);
        var addon = new Product { Id = 10, Name = "Addon", UnitPrice = 50m };
        _products.Add(addon);

        // Act
        await _service.AddAddonAsync(sub.Id, addon.Id, quantity: 1);
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(350m); // 100 + 250 (200 + 50)
    }

    [Fact]
    public async Task RemoveAddon_ShouldDecreaseMRRCorrectly()
    {
        // Arrange
        CreateActiveSubscription(100m);
        var sub = CreateActiveSubscription(250m);
        var addon = new Product { Id = 11, Name = "Addon", UnitPrice = 50m };
        _products.Add(addon);

        // Act
        await _service.RemoveAddonAsync(sub.Id, addon.Id);
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(300m); // 100 + 200 (250 - 50)
    }
}

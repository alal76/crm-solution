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
/// Unit tests for MRR (Monthly Recurring Revenue) and ARR (Annual Recurring Revenue) calculations.
/// TODO-SALES006-043: Tests for calculation precision, aggregation, and edge cases.
/// </summary>
public class MrrArrCalculationTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;

    public MrrArrCalculationTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SubscriptionService>>();

        _subscriptions = new List<Subscription>();
        _invoices = new List<Invoice>();
        _usages = new List<SubscriptionUsage>();
        _usageLimits = new List<SubscriptionUsageLimit>();
        _orders = new List<Order>();

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
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private Subscription CreateSubscription(int id, decimal mrr, SubscriptionStatus status = SubscriptionStatus.Active)
    {
        return new Subscription
        {
            Id = id,
            SubscriptionNumber = $"SUB-MRR-{id:D4}",
            AccountId = id,
            BillingCycle = "Monthly",
            MRR = mrr,
            ARR = mrr * 12,
            SubscriptionStatus = status,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
    }

    // ========================================================================
    // MRR Calculation Tests
    // ========================================================================

    [Fact]
    public async Task CalculateMRR_ShouldSumActiveSubscriptions()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Active),
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(600m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludeCancelledSubscriptions()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Cancelled),
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(400m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludePausedSubscriptions()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Paused),
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(400m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldExcludeDeletedSubscriptions()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            new Subscription { Id = 2, MRR = 500m, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = true, AccountId = 2, BillingCycle = "Monthly" },
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(400m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldReturnZero_WhenNoActiveSubscriptions()
    {
        // Arrange - No subscriptions

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateMRR_ShouldHandleNullMRRValues()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            new Subscription { Id = 2, MRR = null, SubscriptionStatus = SubscriptionStatus.Active, AccountId = 2, BillingCycle = "Monthly" },
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(400m); // null treated as 0
    }

    [Fact]
    public async Task CalculateMRR_ShouldHandlePrecisionCorrectly()
    {
        // Arrange - Testing decimal precision (18,4)
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 99.9999m, SubscriptionStatus.Active),
            CreateSubscription(2, 100.0001m, SubscriptionStatus.Active)
        });

        // Act
        var mrr = await _service.CalculateMRRAsync();

        // Assert
        mrr.Should().Be(200m);
    }

    // ========================================================================
    // ARR Calculation Tests
    // ========================================================================

    [Fact]
    public async Task CalculateARR_ShouldBe12TimesMRR()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Active)
        });

        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(3600m); // 300 * 12
    }

    [Fact]
    public async Task CalculateARR_ShouldReturnZero_WhenNoActiveSubscriptions()
    {
        // Arrange - No subscriptions

        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(0m);
    }

    [Fact]
    public async Task CalculateARR_ShouldHandleLargeValues()
    {
        // Arrange - Large MRR values
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 1000000m, SubscriptionStatus.Active),
            CreateSubscription(2, 2000000m, SubscriptionStatus.Active)
        });

        // Act
        var arr = await _service.CalculateARRAsync();

        // Assert
        arr.Should().Be(36000000m); // 3,000,000 * 12
    }

    // ========================================================================
    // Statistics Tests
    // ========================================================================

    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectCounts()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Active),
            CreateSubscription(3, 300m, SubscriptionStatus.Cancelled),
            CreateSubscription(4, 150m, SubscriptionStatus.Paused),
            CreateSubscription(5, 50m, SubscriptionStatus.Trial)
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalSubscriptions.Should().Be(5);
        stats.ActiveSubscriptions.Should().Be(2);
        stats.CancelledSubscriptions.Should().Be(1);
        stats.PausedSubscriptions.Should().Be(1);
        stats.TrialSubscriptions.Should().Be(1);
    }

    [Fact]
    public async Task GetStatistics_ShouldCalculateMRRAndARR()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Active)
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.MRR.Should().Be(300m);
        stats.ARR.Should().Be(3600m);
    }

    [Fact]
    public async Task GetStatistics_ShouldCalculateAverageRevenuePerUser()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            CreateSubscription(1, 100m, SubscriptionStatus.Active),
            CreateSubscription(2, 200m, SubscriptionStatus.Active),
            CreateSubscription(3, 300m, SubscriptionStatus.Active)
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.AverageRevenuePerUser.Should().Be(200m); // 600/3
    }

    [Fact]
    public async Task GetStatistics_ShouldGroupByPlan()
    {
        // Arrange
        var product1 = new Product { Id = 1, Name = "Basic" };
        var product2 = new Product { Id = 2, Name = "Premium" };

        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, MRR = 50m, ProductId = 1, Product = product1, SubscriptionStatus = SubscriptionStatus.Active, AccountId = 1, BillingCycle = "Monthly" },
            new Subscription { Id = 2, MRR = 50m, ProductId = 1, Product = product1, SubscriptionStatus = SubscriptionStatus.Active, AccountId = 2, BillingCycle = "Monthly" },
            new Subscription { Id = 3, MRR = 150m, ProductId = 2, Product = product2, SubscriptionStatus = SubscriptionStatus.Active, AccountId = 3, BillingCycle = "Monthly" }
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.SubscriptionsByPlan.Should().ContainKey("Basic").WhoseValue.Should().Be(2);
        stats.SubscriptionsByPlan.Should().ContainKey("Premium").WhoseValue.Should().Be(1);
    }

    // ========================================================================
    // Churn Rate Tests
    // ========================================================================

    [Fact]
    public async Task GetChurnRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        // 10 active at start of period
        for (int i = 1; i <= 10; i++)
        {
            var sub = CreateSubscription(i, 100m, SubscriptionStatus.Active);
            sub.CreatedAt = startDate.AddMonths(-3); // Created before period
            _subscriptions.Add(sub);
        }

        // 2 churned during period
        _subscriptions[0].SubscriptionStatus = SubscriptionStatus.Cancelled;
        _subscriptions[0].UpdatedAt = DateTime.UtcNow.AddDays(-5);
        _subscriptions[1].SubscriptionStatus = SubscriptionStatus.Cancelled;
        _subscriptions[1].UpdatedAt = DateTime.UtcNow.AddDays(-10);

        // Act
        var churnRate = await _service.GetChurnRateAsync(startDate, endDate);

        // Assert
        churnRate.Should().BeApproximately(20.0, 0.1); // 2/10 = 20%
    }

    [Fact]
    public async Task GetChurnRate_ShouldReturnZero_WhenNoChurn()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        for (int i = 1; i <= 5; i++)
        {
            var sub = CreateSubscription(i, 100m, SubscriptionStatus.Active);
            sub.CreatedAt = startDate.AddMonths(-3);
            _subscriptions.Add(sub);
        }

        // Act
        var churnRate = await _service.GetChurnRateAsync(startDate, endDate);

        // Assert
        churnRate.Should().Be(0);
    }

    [Fact]
    public async Task GetChurnRate_ShouldReturnZero_WhenNoStartingSubscriptions()
    {
        // Arrange - No subscriptions at start of period
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var churnRate = await _service.GetChurnRateAsync(startDate, endDate);

        // Assert
        churnRate.Should().Be(0);
    }

    // ========================================================================
    // New Subscriptions Count Tests
    // ========================================================================

    [Fact]
    public async Task GetStatistics_ShouldCountNewSubscriptionsThisMonth()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-5), AccountId = 1, BillingCycle = "Monthly" },
            new Subscription { Id = 2, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-10), AccountId = 2, BillingCycle = "Monthly" },
            new Subscription { Id = 3, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Active, CreatedAt = DateTime.UtcNow.AddMonths(-2), AccountId = 3, BillingCycle = "Monthly" }
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.NewSubscriptionsThisMonth.Should().Be(2);
    }

    [Fact]
    public async Task GetStatistics_ShouldCountCancellationsThisMonth()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Cancelled, UpdatedAt = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow.AddMonths(-6), AccountId = 1, BillingCycle = "Monthly" },
            new Subscription { Id = 2, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Cancelled, UpdatedAt = DateTime.UtcNow.AddDays(-10), CreatedAt = DateTime.UtcNow.AddMonths(-6), AccountId = 2, BillingCycle = "Monthly" },
            new Subscription { Id = 3, MRR = 100m, SubscriptionStatus = SubscriptionStatus.Cancelled, UpdatedAt = DateTime.UtcNow.AddMonths(-2), CreatedAt = DateTime.UtcNow.AddMonths(-6), AccountId = 3, BillingCycle = "Monthly" }
        });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.CancellationsThisMonth.Should().Be(2);
    }
}

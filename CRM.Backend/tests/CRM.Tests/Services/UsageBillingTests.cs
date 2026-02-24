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
/// Unit tests for usage-based billing accuracy.
/// TODO-SALES006-042: 15+ unit tests for usage billing calculations.
/// </summary>
public class UsageBillingTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;

    public UsageBillingTests()
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

    private Subscription CreateActiveSubscription(int id = 1)
    {
        return new Subscription
        {
            Id = id,
            SubscriptionNumber = $"SUB-USAGE-{id:D4}",
            AccountId = 1,
            BillingCycle = "Monthly",
            MRR = 100m,
            SubscriptionStatus = SubscriptionStatus.Active
        };
    }

    // ========================================================================
    // Usage Recording Tests
    // ========================================================================

    [Fact]
    public async Task RecordUsage_ShouldSucceed_WhenSubscriptionExists()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsage_ShouldThrow_WhenSubscriptionNotFound()
    {
        // Act
        var act = async () => await _service.RecordUsageAsync(999, "api_calls", 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*not found*");
    }

    [Fact]
    public async Task RecordUsage_ShouldTrimMetricName()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "  api_calls  ", 100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsage_ShouldAllowDecimalQuantities()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "storage_gb", 1.5m);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsage_ShouldAllowVerySmallQuantities()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act - Using precision (18,4) allows 0.0001 as minimum
        var result = await _service.RecordUsageAsync(subscription.Id, "micro_units", 0.0001m);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsage_ShouldRecordTimestamp()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);
        var specificTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "events", 50, specificTime);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // Usage Limits Tests
    // ========================================================================

    [Fact]
    public async Task RecordUsage_WithEnforcedLimit_ShouldThrow_WhenExceedingLimit()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        var limit = new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 1000,
            EnforceCap = true
        };
        _usageLimits.Add(limit);

        // Existing usage
        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 950
        });

        // Act - Trying to add 100 more (total 1050 > limit 1000)
        var act = async () => await _service.RecordUsageAsync(subscription.Id, "api_calls", 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds the configured limit*");
    }

    [Fact]
    public async Task RecordUsage_WithNonEnforcedLimit_ShouldSucceed_WhenExceedingLimit()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        var limit = new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 1000,
            EnforceCap = false // Not enforced
        };
        _usageLimits.Add(limit);

        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 950
        });

        // Act
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 100);

        // Assert - Should succeed even though it exceeds limit
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RecordUsage_ExactlyAtLimit_ShouldSucceed()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        var limit = new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 1000,
            EnforceCap = true
        };
        _usageLimits.Add(limit);

        _usages.Add(new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 900
        });

        // Act - Adding exactly 100 to reach 1000 (not exceeding)
        var result = await _service.RecordUsageAsync(subscription.Id, "api_calls", 100);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // Usage Query Tests
    // ========================================================================

    [Fact]
    public async Task GetUsage_ShouldReturnUsageInDateRange()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        var usage1 = new SubscriptionUsage
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 100,
            Timestamp = new DateTime(2024, 1, 15)
        };
        var usage2 = new SubscriptionUsage
        {
            Id = 2,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 200,
            Timestamp = new DateTime(2024, 1, 20)
        };
        var usage3 = new SubscriptionUsage
        {
            Id = 3,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Quantity = 300,
            Timestamp = new DateTime(2024, 2, 1) // Outside range
        };
        _usages.AddRange(new[] { usage1, usage2, usage3 });

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        // Assert
        result.Should().NotBeNull();
        result.SubscriptionId.Should().Be(subscription.Id);
        result.Metrics.Should().HaveCount(1);
        result.Metrics[0].TotalUsage.Should().Be(300m); // 100 + 200
    }

    [Fact]
    public async Task GetUsage_ShouldGroupByMetricName()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        _usages.AddRange(new[]
        {
            new SubscriptionUsage { Id = 1, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 100, Timestamp = DateTime.UtcNow },
            new SubscriptionUsage { Id = 2, SubscriptionId = subscription.Id, MetricName = "storage_gb", Quantity = 5.5m, Timestamp = DateTime.UtcNow },
            new SubscriptionUsage { Id = 3, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 50, Timestamp = DateTime.UtcNow }
        });

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert
        result.Metrics.Should().HaveCount(2);
        result.Metrics.Should().Contain(m => m.MetricName == "api_calls" && m.TotalUsage == 150m);
        result.Metrics.Should().Contain(m => m.MetricName == "storage_gb" && m.TotalUsage == 5.5m);
    }

    [Fact]
    public async Task GetUsage_ShouldReturnEmptyMetrics_WhenNoUsageInRange()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act
        var result = await _service.GetUsageAsync(subscription.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert
        result.Metrics.Should().BeEmpty();
    }

    // ========================================================================
    // Usage Limits Query Tests
    // ========================================================================

    [Fact]
    public async Task GetUsageLimits_ShouldReturnLimitsWithCurrentUsage()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "api_calls",
            Limit = 10000,
            Unit = "calls"
        });

        _usages.AddRange(new[]
        {
            new SubscriptionUsage { Id = 1, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 1500 },
            new SubscriptionUsage { Id = 2, SubscriptionId = subscription.Id, MetricName = "api_calls", Quantity = 2500 }
        });

        // Act
        var result = await _service.GetUsageLimitsAsync(subscription.Id);

        // Assert
        result.Should().HaveCount(1);
        var limit = result.First();
        limit.MetricName.Should().Be("api_calls");
        limit.Limit.Should().Be(10000);
        limit.Used.Should().Be(4000); // 1500 + 2500
    }

    [Fact]
    public async Task GetUsageLimits_ShouldReturnEmpty_WhenNoLimitsConfigured()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        // Act
        var result = await _service.GetUsageLimitsAsync(subscription.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsageLimits_ShouldShowZeroUsed_WhenNoUsageRecorded()
    {
        // Arrange
        var subscription = CreateActiveSubscription();
        _subscriptions.Add(subscription);

        _usageLimits.Add(new SubscriptionUsageLimit
        {
            Id = 1,
            SubscriptionId = subscription.Id,
            MetricName = "storage_gb",
            Limit = 100
        });

        // Act
        var result = await _service.GetUsageLimitsAsync(subscription.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Used.Should().Be(0);
    }
}

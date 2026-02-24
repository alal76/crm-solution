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
/// Unit tests for Subscription validation logic in SubscriptionService.
/// Tests SubscriptionNumber, Amount, BillingCycle, and AutoRenew+Cancelled validations.
/// </summary>
public class SubscriptionValidationTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;

    public SubscriptionValidationTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SubscriptionService>>();

        _subscriptions = new List<Subscription>();
        _invoices = new List<Invoice>();
        _usages = new List<SubscriptionUsage>();
        _usageLimits = new List<SubscriptionUsageLimit>();
        _orders = new List<Order>();

        var mockSubscriptions = MockDbSetFactory.CreateMockDbSet(_subscriptions);
        var mockInvoices = MockDbSetFactory.CreateMockDbSet(_invoices);
        var mockUsages = MockDbSetFactory.CreateMockDbSet(_usages);
        var mockUsageLimits = MockDbSetFactory.CreateMockDbSet(_usageLimits);
        var mockOrders = MockDbSetFactory.CreateMockDbSet(_orders);

        // Add FindAsync(object[], CancellationToken) overload
        mockSubscriptions.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<Subscription?>(default);
                return ValueTask.FromResult(_subscriptions.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        _mockContext.Setup(c => c.Subscriptions).Returns(mockSubscriptions.Object);
        _mockContext.Setup(c => c.Invoices).Returns(mockInvoices.Object);
        _mockContext.Setup(c => c.SubscriptionUsages).Returns(mockUsages.Object);
        _mockContext.Setup(c => c.SubscriptionUsageLimits).Returns(mockUsageLimits.Object);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new SubscriptionService(_mockContext.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Helper to create a valid subscription for testing. Override specific fields as needed.
    /// </summary>
    private static Subscription CreateValidSubscription(int accountId = 1, decimal amount = 100m, string billingCycle = "Monthly")
    {
        return new Subscription
        {
            AccountId = accountId,
            Amount = amount,
            BillingCycle = billingCycle
        };
    }

    // ========================================================================
    // CreateAsync - Amount Validation
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenAmountNegative()
    {
        // Arrange
        var subscription = CreateValidSubscription(amount: -50m);

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Amount must be greater than or equal to zero*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenAmountIsZero()
    {
        // Arrange
        var subscription = CreateValidSubscription(amount: 0m);

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(0m);
    }

    // ========================================================================
    // CreateAsync - BillingCycle Validation
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenBillingCycleEmpty()
    {
        // Arrange
        var subscription = CreateValidSubscription(billingCycle: "");

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*BillingCycle is required*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenBillingCycleWhitespace()
    {
        // Arrange
        var subscription = CreateValidSubscription(billingCycle: "   ");

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*BillingCycle is required*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenBillingCycleIsMonthly()
    {
        // Arrange
        var subscription = CreateValidSubscription(billingCycle: "Monthly");

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.BillingCycle.Should().Be("Monthly");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenBillingCycleIsInvalid()
    {
        // Arrange
        var subscription = CreateValidSubscription(billingCycle: "Biweekly");

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ========================================================================
    // CreateAsync - AutoRenew + Cancelled Validation
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenAutoRenewAndCancelled()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.IsAutoRenew = true;
        subscription.SubscriptionStatus = SubscriptionStatus.Cancelled;

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot set AutoRenew on a cancelled subscription*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenAutoRenewAndActive()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.IsAutoRenew = true;
        subscription.SubscriptionStatus = SubscriptionStatus.Active;

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.IsAutoRenew.Should().BeTrue();
    }

    // ========================================================================
    // UpdateAsync - SubscriptionNumber Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenSubscriptionNumberEmpty()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "",
            BillingCycle = "Monthly",
            Amount = 100m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SubscriptionNumber is required*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenSubscriptionNumberWhitespace()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "   ",
            BillingCycle = "Monthly",
            Amount = 100m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SubscriptionNumber is required*");
    }

    // ========================================================================
    // UpdateAsync - BillingCycle Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenBillingCycleEmpty()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "",
            Amount = 100m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*BillingCycle is required*");
    }

    // ========================================================================
    // UpdateAsync - AutoRenew + Cancelled Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenAutoRenewAndCancelled()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsAutoRenew = true,
            SubscriptionStatus = SubscriptionStatus.Cancelled
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot set AutoRenew on a cancelled subscription*");
    }

    // ========================================================================
    // UpdateAsync - Amount Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenAmountNegative()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = -10m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Amount must be greater than or equal to zero*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldSucceed_WhenAllFieldsValid()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Monthly",
            Amount = 100m,
            IsDeleted = false
        });

        var updated = new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionNumber = "SUB-2602-0001",
            BillingCycle = "Yearly",
            Amount = 200m,
            IsAutoRenew = false,
            SubscriptionStatus = SubscriptionStatus.Active
        };

        // Act
        var result = await _service.UpdateAsync(updated);

        // Assert
        result.Should().NotBeNull();
        result.BillingCycle.Should().Be("Yearly");
        result.Amount.Should().Be(200m);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ========================================================================
    // Trial Date Validation (TODO-SALES006-019)
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTrialEndDateBeforeTrialStartDate()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.TrialStartDate = DateTime.UtcNow.AddDays(10);
        subscription.TrialEndDate = DateTime.UtcNow; // Before start

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialEndDate must be greater than TrialStartDate*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTrialEndDateEqualsTrialStartDate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var subscription = CreateValidSubscription();
        subscription.TrialStartDate = now;
        subscription.TrialEndDate = now; // Equal

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialEndDate must be greater than TrialStartDate*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTrialEndDateSetWithoutStartDate()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.TrialStartDate = null;
        subscription.TrialEndDate = DateTime.UtcNow.AddDays(14);

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialStartDate is required when TrialEndDate is set*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenTrialDatesValid()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.TrialStartDate = DateTime.UtcNow;
        subscription.TrialEndDate = DateTime.UtcNow.AddDays(14);

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.TrialStartDate.Should().NotBeNull();
        result.TrialEndDate.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenNoTrialDates()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.TrialStartDate = null;
        subscription.TrialEndDate = null;

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // Proration Type Validation (TODO-SALES006-019)
    // ========================================================================

    [Fact]
    public void ProrationStrategy_Default_ShouldBeDaily()
    {
        // Arrange
        var subscription = new Subscription();

        // Assert
        subscription.ProrationType.Should().Be(ProrationStrategy.Daily);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WithAllProrationTypes()
    {
        // Arrange & Act & Assert
        foreach (var strategy in Enum.GetValues<ProrationStrategy>())
        {
            var subscription = CreateValidSubscription();
            subscription.ProrationType = strategy;

            var result = await _service.CreateAsync(subscription);
            result.Should().NotBeNull();
            result.ProrationType.Should().Be(strategy);

            // Clean up for next iteration
            _subscriptions.Clear();
        }
    }

    // ========================================================================
    // Dunning Configuration Validation (TODO-SALES006-025)
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDunningGracePeriodNegative()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.DunningGracePeriodDays = -1;

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*DunningGracePeriodDays must be greater than or equal to zero*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenDunningGracePeriodZero()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.DunningGracePeriodDays = 0;

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.DunningGracePeriodDays.Should().Be(0);
    }

    [Fact]
    public void DunningGracePeriodDays_Default_ShouldBeThree()
    {
        // Arrange
        var subscription = new Subscription();

        // Assert
        subscription.DunningGracePeriodDays.Should().Be(3);
    }

    [Fact]
    public void SendDunningEscalationEmails_Default_ShouldBeTrue()
    {
        // Arrange
        var subscription = new Subscription();

        // Assert
        subscription.SendDunningEscalationEmails.Should().BeTrue();
    }

    // ========================================================================
    // Timezone Validation (TODO-SALES006-023)
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldAcceptValidTimezone()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.BillingTimezone = "America/New_York";

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.BillingTimezone.Should().Be("America/New_York");
    }

    [Fact]
    public async Task CreateAsync_ShouldAcceptNullTimezone()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.BillingTimezone = null;

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.BillingTimezone.Should().BeNull();
    }

    // ========================================================================
    // RowVersion Optimistic Locking (TODO-SALES006-022)
    // ========================================================================

    [Fact]
    public void Subscription_ShouldHaveRowVersionProperty()
    {
        // Arrange & Act
        var subscription = new Subscription();

        // Assert
        var property = typeof(Subscription).GetProperty("RowVersion");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(byte[]));
    }
}


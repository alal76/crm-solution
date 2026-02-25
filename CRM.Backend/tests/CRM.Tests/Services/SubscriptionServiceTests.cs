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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SubscriptionService.
/// </summary>
public class SubscriptionServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _service;

    private readonly List<Subscription> _subscriptions;
    private readonly List<Invoice> _invoices;
    private readonly List<SubscriptionUsage> _usages;
    private readonly List<SubscriptionUsageLimit> _usageLimits;
    private readonly List<Order> _orders;

    public SubscriptionServiceTests()
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

        // Add FindAsync(object[], CancellationToken) overload - MockDbSetFactory only sets up FindAsync(object[])
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

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedSubscriptions()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, SubscriptionNumber = "SUB-000001", AccountId = 1, IsDeleted = false },
            new Subscription { Id = 2, SubscriptionNumber = "SUB-000002", AccountId = 2, IsDeleted = false },
            new Subscription { Id = 3, SubscriptionNumber = "SUB-000003", AccountId = 1, IsDeleted = true }
        });

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByCustomerId()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, AccountId = 10, IsDeleted = false },
            new Subscription { Id = 2, AccountId = 20, IsDeleted = false },
            new Subscription { Id = 3, AccountId = 10, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(accountId: 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.AccountId == 10);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false },
            new Subscription { Id = 2, SubscriptionStatus = SubscriptionStatus.Paused, IsDeleted = false },
            new Subscription { Id = 3, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false }
        });

        // Act
        var result = await _service.GetAllAsync(status: SubscriptionStatus.Active);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.SubscriptionStatus == SubscriptionStatus.Active);
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnSubscription_WhenExists()
    {
        // Arrange
        _subscriptions.Add(new Subscription { Id = 1, SubscriptionNumber = "SUB-000001", AccountId = 5, IsDeleted = false });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        // Arrange
        _subscriptions.Add(new Subscription { Id = 1, IsDeleted = true });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetBySubscriptionNumberAsync
    // ========================================================================
    [Fact]
    public async Task GetBySubscriptionNumberAsync_ShouldReturnMatchingSubscription()
    {
        // Arrange
        _subscriptions.Add(new Subscription { Id = 1, SubscriptionNumber = "SUB-000001", IsDeleted = false });

        // Act
        var result = await _service.GetBySubscriptionNumberAsync("SUB-000001");

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionNumber.Should().Be("SUB-000001");
    }

    // ========================================================================
    // CreateAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldSetTimestampsAndSubscriptionNumber()
    {
        // Arrange
        var subscription = new Subscription { AccountId = 1, Amount = 100m, BillingCycle = "Monthly" };

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.SubscriptionNumber.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _subscriptions.Should().Contain(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenAccountIdIsZero()
    {
        // Arrange
        var subscription = new Subscription { AccountId = 0, Amount = 100m, BillingCycle = "Monthly" };

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ========================================================================
    // DeleteAsync
    // ========================================================================
    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        // Arrange
        _subscriptions.Add(new Subscription { Id = 1, AccountId = 1, IsDeleted = false });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _subscriptions.First(s => s.Id == 1).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ========================================================================
    // ActivateAsync / PauseAsync / ResumeAsync / CancelAsync
    // ========================================================================
    [Fact]
    public async Task ActivateAsync_ShouldSetStatusToActive()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Trial,
            IsDeleted = false
        });

        // Act
        var result = await _service.ActivateAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task PauseAsync_ShouldSetStatusToPaused()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false
        });

        // Act
        var result = await _service.PauseAsync(1, "Customer requested");

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionStatus.Should().Be(SubscriptionStatus.Paused);
    }

    [Fact]
    public async Task ResumeAsync_ShouldThrow_WhenNotPaused()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false
        });

        // Act
        var act = async () => await _service.ResumeAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not paused*");
    }

    [Fact]
    public async Task ResumeAsync_ShouldSetActiveStatus_WhenPaused()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Paused,
            IsDeleted = false
        });

        // Act
        var result = await _service.ResumeAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task CancelAsync_Immediate_ShouldSetCancelledStatus()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false
        });

        // Act
        var result = await _service.CancelAsync(1, "No longer needed", immediate: true);

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_NotImmediate_ShouldSetPendingCancellation()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 1,
            AccountId = 1,
            SubscriptionStatus = SubscriptionStatus.Active,
            IsDeleted = false
        });

        // Act
        var result = await _service.CancelAsync(1, "Switching provider", immediate: false);

        // Assert
        result.Should().NotBeNull();
        result!.SubscriptionStatus.Should().Be(SubscriptionStatus.PendingCancellation);
    }

    // ========================================================================
    // CalculateMRRAsync / CalculateARRAsync
    // ========================================================================
    [Fact]
    public async Task CalculateMRRAsync_ShouldSumActiveSubscriptionMRR()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, SubscriptionStatus = SubscriptionStatus.Active, MRR = 100m, IsDeleted = false },
            new Subscription { Id = 2, SubscriptionStatus = SubscriptionStatus.Active, MRR = 200m, IsDeleted = false },
            new Subscription { Id = 3, SubscriptionStatus = SubscriptionStatus.Cancelled, MRR = 50m, IsDeleted = false }
        });

        // Act
        var result = await _service.CalculateMRRAsync();

        // Assert
        result.Should().Be(300m);
    }

    [Fact]
    public async Task CalculateARRAsync_ShouldBeMRRTimes12()
    {
        // Arrange
        _subscriptions.AddRange(new[]
        {
            new Subscription { Id = 1, SubscriptionStatus = SubscriptionStatus.Active, MRR = 100m, IsDeleted = false },
            new Subscription { Id = 2, SubscriptionStatus = SubscriptionStatus.Active, MRR = 200m, IsDeleted = false }
        });

        // Act
        var result = await _service.CalculateARRAsync();

        // Assert
        result.Should().Be(3600m); // (100 + 200) * 12
    }

    // ========================================================================
    // SetAutoRenewalAsync
    // ========================================================================
    [Fact]
    public async Task SetAutoRenewalAsync_ShouldUpdateContractNotes()
    {
        // Arrange
        _subscriptions.Add(new Subscription { Id = 1, AccountId = 1, IsDeleted = false });

        // Act
        var result = await _service.SetAutoRenewalAsync(1, true);

        // Assert
        result.Should().NotBeNull();
        result!.ContractNotes.Should().NotBeNullOrEmpty();
    }

    // ========================================================================
    // TODO-SALES006-019: Trial Date Validation Tests
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTrialEndDateIsBeforeTrialStartDate()
    {
        // Arrange
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            TrialEndDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc)  // Before start
        };

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
        var trialDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = trialDate,
            TrialEndDate = trialDate  // Same as start — not strictly greater
        };

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialEndDate must be greater than TrialStartDate*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenTrialEndDateSetWithoutTrialStartDate()
    {
        // Arrange
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = null,
            TrialEndDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialStartDate is required when TrialEndDate is set*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenValidTrialDatesProvided()
    {
        // Arrange
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            TrialEndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.TrialStartDate.Should().Be(subscription.TrialStartDate);
        result.TrialEndDate.Should().Be(subscription.TrialEndDate);
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenBothTrialDatesAreNull()
    {
        // Arrange — no trial dates, should pass validation
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 50m,
            BillingCycle = "Monthly",
            TrialStartDate = null,
            TrialEndDate = null
        };

        // Act
        var result = await _service.CreateAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.TrialStartDate.Should().BeNull();
        result.TrialEndDate.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTrialEndDateIsBeforeTrialStartDate()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 10,
            AccountId = 1,
            SubscriptionNumber = "SUB-010",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        var update = new Subscription
        {
            Id = 10,
            AccountId = 1,
            SubscriptionNumber = "SUB-010",
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            TrialEndDate = new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc)  // Before start
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialEndDate must be greater than TrialStartDate*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenTrialEndDateSetWithoutTrialStartDate()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 11,
            AccountId = 1,
            SubscriptionNumber = "SUB-011",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        var update = new Subscription
        {
            Id = 11,
            AccountId = 1,
            SubscriptionNumber = "SUB-011",
            Amount = 100m,
            BillingCycle = "Monthly",
            TrialStartDate = null,
            TrialEndDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*TrialStartDate is required when TrialEndDate is set*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDunningGracePeriodDaysIsNegative()
    {
        // Arrange
        var subscription = new Subscription
        {
            AccountId = 1,
            Amount = 100m,
            BillingCycle = "Monthly",
            DunningGracePeriodDays = -1
        };

        // Act
        var act = async () => await _service.CreateAsync(subscription);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*DunningGracePeriodDays must be greater than or equal to zero*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDunningGracePeriodDaysIsNegative()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 12,
            AccountId = 1,
            SubscriptionNumber = "SUB-012",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        var update = new Subscription
        {
            Id = 12,
            AccountId = 1,
            SubscriptionNumber = "SUB-012",
            Amount = 100m,
            BillingCycle = "Monthly",
            DunningGracePeriodDays = -5
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*DunningGracePeriodDays must be greater than or equal to zero*");
    }

    // ========================================================================
    // TODO-SALES006-022: Optimistic Concurrency Tests
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 20,
            AccountId = 1,
            SubscriptionNumber = "SUB-020",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        // Make SaveChangesAsync throw DbUpdateConcurrencyException
        _mockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Simulated concurrency conflict"));

        var update = new Subscription
        {
            Id = 20,
            AccountId = 1,
            SubscriptionNumber = "SUB-020",
            Amount = 200m,
            BillingCycle = "Monthly"
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldIncludeSubscriptionIdInMessage_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 21,
            AccountId = 1,
            SubscriptionNumber = "SUB-021",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        _mockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Conflict"));

        var update = new Subscription
        {
            Id = 21,
            AccountId = 1,
            SubscriptionNumber = "SUB-021",
            Amount = 200m,
            BillingCycle = "Monthly"
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("21");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveOriginalException_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        _subscriptions.Add(new Subscription
        {
            Id = 22,
            AccountId = 1,
            SubscriptionNumber = "SUB-022",
            BillingCycle = "Monthly",
            IsDeleted = false
        });

        var dbException = new DbUpdateConcurrencyException("Root concurrency cause");
        _mockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        var update = new Subscription
        {
            Id = 22,
            AccountId = 1,
            SubscriptionNumber = "SUB-022",
            Amount = 200m,
            BillingCycle = "Monthly"
        };

        // Act
        var act = async () => await _service.UpdateAsync(update);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();
    }
}

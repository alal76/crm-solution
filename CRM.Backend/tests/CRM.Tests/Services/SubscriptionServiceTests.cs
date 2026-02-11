// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
                if (id == null) return ValueTask.FromResult<Subscription?>(default);
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
        var result = await _service.GetAllAsync(customerId: 10);

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
        var subscription = new Subscription { AccountId = 1, Amount = 100m };

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
        var subscription = new Subscription { AccountId = 0, Amount = 100m };

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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Trial, IsDeleted = false
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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false
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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false
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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Paused, IsDeleted = false
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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false
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
            Id = 1, AccountId = 1, SubscriptionStatus = SubscriptionStatus.Active, IsDeleted = false
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
}

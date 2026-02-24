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

namespace CRM.Tests.Integration;

/// <summary>
/// Unit tests for subscription workflow scenarios (converted from integration tests).
/// Tests auto-renewal, dunning/suspension, and plan change workflows using
/// actual SubscriptionService with mocked ICrmDbContext.
/// TODO-SALES006-045: Auto-renewal workflow
/// TODO-SALES006-046: Dunning retry + cancellation
/// TODO-SALES006-047: Plan change with proration
/// </summary>
public class SubscriptionWorkflowIntegrationTests
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

    public SubscriptionWorkflowIntegrationTests()
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

    private Subscription CreateTestSubscription(
        bool isAutoRenew = true,
        DateTime? contractEndDate = null,
        SubscriptionStatus status = SubscriptionStatus.Active,
        decimal mrr = 100m,
        string billingCycle = "Monthly",
        DateTime? billingStartDate = null)
    {
        var sub = new Subscription
        {
            Id = _subscriptions.Count + 1,
            SubscriptionNumber = $"SUB-WF-{_subscriptions.Count + 1:D4}",
            AccountId = 1,
            ProductId = 1,
            MRR = mrr,
            ARR = mrr * 12,
            BillingCycle = billingCycle,
            SubscriptionStatus = status,
            IsAutoRenew = isAutoRenew,
            IsActive = true,
            BillingStartDate = billingStartDate ?? DateTime.UtcNow.AddDays(-5),
            BillingEndDate = (billingStartDate ?? DateTime.UtcNow.AddDays(-5)).AddMonths(1),
            ContractStartDate = DateTime.UtcNow.AddDays(-5),
            ContractEndDate = contractEndDate ?? DateTime.UtcNow.AddMonths(1),
            BillingTimezone = "UTC",
            DunningGracePeriodDays = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _subscriptions.Add(sub);
        return sub;
    }

    private int CreateTestProduct(decimal price)
    {
        var product = new Product
        {
            Id = _products.Count + 10,
            Name = $"Test Product {_products.Count + 1}",
            UnitPrice = price,
            RecurringPrice = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _products.Add(product);
        return product.Id;
    }

    #region Auto-Renewal Workflow (TODO-SALES006-045)

    [Fact]
    public async Task RenewAsync_ActiveSubscription_ShouldExtendContract()
    {
        // Arrange
        var subscription = CreateTestSubscription(
            isAutoRenew: true,
            contractEndDate: DateTime.UtcNow.AddDays(5)
        );

        // Act
        var result = await _service.RenewAsync(subscription.Id);

        // Assert
        result.Should().NotBeNull();
        result.ContractEndDate.Should().BeAfter(subscription.ContractEndDate!.Value);
    }

    [Fact]
    public async Task RenewAsync_PausedSubscription_ShouldThrow()
    {
        // Arrange
        var subscription = CreateTestSubscription(
            isAutoRenew: true,
            contractEndDate: DateTime.UtcNow.AddDays(-1),
            status: SubscriptionStatus.Paused
        );

        // Act & Assert
        var act = async () => await _service.RenewAsync(subscription.Id);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetAutoRenewalAsync_ShouldToggleAutoRenew()
    {
        // Arrange
        var subscription = CreateTestSubscription(isAutoRenew: false);

        // Act
        var result = await _service.SetAutoRenewalAsync(subscription.Id, true);

        // Assert
        result.IsAutoRenew.Should().BeTrue();
        result.ContractNotes.Should().Contain("Auto-renewal");
    }

    [Fact]
    public async Task GetDueForRenewalAsync_ShouldReturnUpcomingRenewals()
    {
        // Arrange
        var soon = CreateTestSubscription(
            contractEndDate: DateTime.UtcNow.AddDays(5)
        );
        var later = CreateTestSubscription(
            contractEndDate: DateTime.UtcNow.AddDays(45)
        );

        // Act
        var dueForRenewal = await _service.GetDueForRenewalAsync(withinDays: 30);

        // Assert
        dueForRenewal.Should().Contain(s => s.Id == soon.Id);
    }

    #endregion

    #region Dunning / Suspension Workflow (TODO-SALES006-046)

    [Fact]
    public async Task SuspendAsync_ShouldSetStatusToSuspended()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);

        // Act
        var result = await _service.SuspendAsync(subscription.Id, "Payment failed after retries");

        // Assert
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);
        result.ContractNotes.Should().Contain("Suspended");
    }

    [Fact]
    public async Task CancelAsync_AfterSuspension_ShouldSetStatusToCancelled()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);
        subscription.DunningAttemptCount = 3;
        subscription.DunningGracePeriodDays = 0;

        // Suspend first
        await _service.SuspendAsync(subscription.Id, "Payment failed");

        // Act
        var result = await _service.CancelAsync(subscription.Id, "Dunning exhausted - auto-cancellation", immediate: true);

        // Assert
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
        result.ContractNotes.Should().Contain("Dunning exhausted");
    }

    [Fact]
    public async Task ReactivateAsync_SuspendedSubscription_ShouldRestore()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);
        subscription.DunningAttemptCount = 2;

        // Suspend
        await _service.SuspendAsync(subscription.Id, "Payment failed");

        // Act - Customer resolves payment issue
        var result = await _service.ReactivateAsync(subscription.Id);

        // Assert
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task SuspendAsync_ShouldPreserveDunningMetadata()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);
        subscription.DunningAttemptCount = 3;
        subscription.LastDunningDate = DateTime.UtcNow.AddDays(-3);

        // Act
        var result = await _service.SuspendAsync(subscription.Id, "Max dunning retries reached");

        // Assert
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);
        result.DunningAttemptCount.Should().Be(3);
    }

    #endregion

    #region Plan Change with Proration (TODO-SALES006-047)

    [Fact]
    public async Task CalculateProratedAmountAsync_Upgrade_ShouldReturnProratedAmount()
    {
        // Arrange
        var subscription = CreateTestSubscription(mrr: 100m, billingCycle: "Monthly");
        subscription.BillingStartDate = DateTime.UtcNow.Date.AddDays(-15);
        subscription.BillingEndDate = DateTime.UtcNow.Date.AddDays(15);

        var changeDate = DateTime.UtcNow.Date;
        var newAmount = 200m;

        // Act
        var prorated = await _service.CalculateProratedAmountAsync(
            subscription.Id, changeDate, newAmount);

        // Assert
        prorated.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ChangePlanAsync_Immediate_ShouldUpdateSubscription()
    {
        // Arrange
        var subscription = CreateTestSubscription(mrr: 100m);
        var newProductId = CreateTestProduct(200m);

        // Act
        var result = await _service.ChangePlanAsync(
            subscription.Id, newProductId, SubscriptionChangeType.Immediate);

        // Assert
        result.ProductId.Should().Be(newProductId);
        result.MRR.Should().Be(200m);
    }

    [Fact]
    public async Task ChangePlanAsync_NextBillingCycle_ShouldScheduleChange()
    {
        // Arrange
        var subscription = CreateTestSubscription(mrr: 100m);
        var newProductId = CreateTestProduct(200m);

        // Act
        var result = await _service.ChangePlanAsync(
            subscription.Id, newProductId, SubscriptionChangeType.NextBillingCycle);

        // Assert - Should not change immediately
        result.MRR.Should().Be(100m);
        result.ContractNotes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpgradeAsync_Immediate_ShouldUpdateProductAndMRR()
    {
        // Arrange
        var subscription = CreateTestSubscription(mrr: 100m);
        var newProductId = CreateTestProduct(200m);

        // Act
        var result = await _service.UpgradeAsync(subscription.Id, newProductId, immediate: true);

        // Assert
        result.ProductId.Should().Be(newProductId);
        result.MRR.Should().Be(200m);
    }

    [Fact]
    public async Task DowngradeAsync_ShouldScheduleForEndOfPeriod()
    {
        // Arrange
        var subscription = CreateTestSubscription(mrr: 200m);
        var newProductId = CreateTestProduct(100m);

        // Act
        var result = await _service.DowngradeAsync(subscription.Id, newProductId);

        // Assert - Downgrade deferred
        result.MRR.Should().Be(200m);
        result.ContractNotes.Should().Contain("end of period");
    }

    #endregion

    #region Full Lifecycle Workflow

    [Fact]
    public async Task FullLifecycle_Activate_Pause_Resume()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);

        // Act Step 1: Pause
        var paused = await _service.PauseAsync(subscription.Id, "Customer request");
        paused.SubscriptionStatus.Should().Be(SubscriptionStatus.Paused);

        // Act Step 2: Resume
        var resumed = await _service.ResumeAsync(subscription.Id);
        resumed.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task FullLifecycle_Active_Suspend_Cancel()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);

        // Act Step 1: Suspend for payment failure
        var suspended = await _service.SuspendAsync(subscription.Id, "Payment failed");
        suspended.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);

        // Act Step 2: Cancel after exhausting dunning retries
        var cancelled = await _service.CancelAsync(subscription.Id, "All retries exhausted", immediate: true);
        cancelled.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task FullLifecycle_Active_Suspend_Reactivate()
    {
        // Arrange
        var subscription = CreateTestSubscription(status: SubscriptionStatus.Active);

        // Suspend
        await _service.SuspendAsync(subscription.Id, "Payment failed");

        // Reactivate
        var reactivated = await _service.ReactivateAsync(subscription.Id);

        // Assert
        reactivated.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        reactivated.MRR.Should().Be(100m); // MRR preserved
    }

    #endregion
}

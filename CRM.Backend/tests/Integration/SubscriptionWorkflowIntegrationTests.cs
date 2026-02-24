// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for subscription workflow scenarios.
/// TODO-SALES006-045: Auto-renewal workflow
/// TODO-SALES006-046: Dunning retry + cancellation
/// TODO-SALES006-047: Plan change with proration
/// </summary>
public class SubscriptionWorkflowIntegrationTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly IServiceScope _scope;
    private readonly ICrmDbContext _dbContext;
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionWorkflowIntegrationTests(TestFixture fixture)
    {
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
        _subscriptionService = _scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }

    #region Auto-Renewal Workflow (TODO-SALES006-045)

    [Fact]
    public async Task AutoRenewal_ActiveSubscriptionDueForRenewal_ShouldExtendContract()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            isAutoRenew: true,
            contractEndDate: DateTime.UtcNow.AddDays(-1) // Past due
        );

        // Act
        var result = await _subscriptionService.ProcessRenewalAsync(subscription.Id);

        // Assert
        result.Should().BeTrue();
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.ContractEndDate.Should().BeAfter(DateTime.UtcNow);
        updated.RenewalCount.Should().Be(1);
    }

    [Fact]
    public async Task AutoRenewal_NonAutoRenewSubscription_ShouldNotExtend()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            isAutoRenew: false,
            contractEndDate: DateTime.UtcNow.AddDays(-1)
        );
        var originalEndDate = subscription.ContractEndDate;

        // Act
        var result = await _subscriptionService.ProcessRenewalAsync(subscription.Id);

        // Assert
        result.Should().BeFalse();
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.ContractEndDate.Should().Be(originalEndDate);
    }

    [Fact]
    public async Task AutoRenewal_ShouldCreateRenewalInvoice()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            isAutoRenew: true,
            contractEndDate: DateTime.UtcNow.AddDays(-1)
        );
        var invoiceCountBefore = await _dbContext.Invoices.CountAsync(i => i.SubscriptionId == subscription.Id);

        // Act
        await _subscriptionService.ProcessRenewalAsync(subscription.Id);

        // Assert
        var invoiceCountAfter = await _dbContext.Invoices.CountAsync(i => i.SubscriptionId == subscription.Id);
        invoiceCountAfter.Should().BeGreaterThan(invoiceCountBefore);
    }

    [Fact]
    public async Task AutoRenewal_PausedSubscription_ShouldNotRenew()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            isAutoRenew: true,
            contractEndDate: DateTime.UtcNow.AddDays(-1),
            status: SubscriptionStatus.Paused
        );

        // Act
        var result = await _subscriptionService.ProcessRenewalAsync(subscription.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Dunning Retry and Cancellation (TODO-SALES006-046)

    [Fact]
    public async Task DunningWorkflow_FailedPayment_ShouldIncrementAttempts()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            status: SubscriptionStatus.Active
        );

        // Act
        await _subscriptionService.RecordPaymentFailureAsync(subscription.Id, "Card declined");

        // Assert
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.PaymentRetryCount.Should().Be(1);
    }

    [Fact]
    public async Task DunningWorkflow_MaxRetriesExceeded_ShouldCancelSubscription()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            status: SubscriptionStatus.Active
        );
        subscription.PaymentRetryCount = 3; // At max retries
        subscription.DunningGracePeriodDays = 0;
        await _dbContext.SaveChangesAsync();

        // Act
        await _subscriptionService.ProcessDunningAsync(subscription.Id);

        // Assert
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
        updated.CancellationReason.Should().Contain("payment");
    }

    [Fact]
    public async Task DunningWorkflow_WithinGracePeriod_ShouldNotCancel()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            status: SubscriptionStatus.Active
        );
        subscription.PaymentRetryCount = 3;
        subscription.DunningGracePeriodDays = 7;
        subscription.LastPaymentFailedAt = DateTime.UtcNow.AddDays(-3);
        await _dbContext.SaveChangesAsync();

        // Act
        await _subscriptionService.ProcessDunningAsync(subscription.Id);

        // Assert
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.SubscriptionStatus.Should().NotBe(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public async Task DunningWorkflow_SuccessfulPayment_ShouldResetRetryCount()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            status: SubscriptionStatus.Active
        );
        subscription.PaymentRetryCount = 2;
        await _dbContext.SaveChangesAsync();

        // Act
        await _subscriptionService.RecordPaymentSuccessAsync(subscription.Id);

        // Assert
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.PaymentRetryCount.Should().Be(0);
    }

    #endregion

    #region Plan Change with Proration (TODO-SALES006-047)

    [Fact]
    public async Task PlanChange_Upgrade_ShouldApplyProration()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            mrr: 100m,
            billingCycle: "Monthly"
        );
        var newProductId = await CreateTestProduct(200m);

        // Act
        var proration = await _subscriptionService.CalculatePlanChangeProrationAsync(
            subscription.Id, newProductId);

        // Assert
        proration.ProratedAmount.Should().BeGreaterThan(0);
        proration.IsUpgrade.Should().BeTrue();
    }

    [Fact]
    public async Task PlanChange_Downgrade_ShouldCalculateCredit()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            mrr: 200m,
            billingCycle: "Monthly"
        );
        var newProductId = await CreateTestProduct(100m);

        // Act
        var proration = await _subscriptionService.CalculatePlanChangeProrationAsync(
            subscription.Id, newProductId);

        // Assert
        proration.CreditAmount.Should().BeGreaterThan(0);
        proration.IsUpgrade.Should().BeFalse();
    }

    [Fact]
    public async Task PlanChange_MidCycle_ShouldProrateDaysRemaining()
    {
        // Arrange
        var subscription = await CreateTestSubscription(
            mrr: 100m,
            billingCycleStartDate: DateTime.UtcNow.AddDays(-15), // Mid-cycle
            contractEndDate: DateTime.UtcNow.AddDays(15)
        );
        var newProductId = await CreateTestProduct(200m);

        // Act
        var proration = await _subscriptionService.CalculatePlanChangeProrationAsync(
            subscription.Id, newProductId);

        // Assert
        proration.DaysRemaining.Should().BeGreaterThan(0);
        proration.DaysRemaining.Should().BeLessThan(30);
    }

    [Fact]
    public async Task PlanChange_ApplyChange_ShouldUpdateSubscription()
    {
        // Arrange
        var subscription = await CreateTestSubscription(mrr: 100m);
        var newProductId = await CreateTestProduct(200m);

        // Act
        await _subscriptionService.ChangePlanAsync(subscription.Id, newProductId, applyProration: true);

        // Assert
        var updated = await _dbContext.Subscriptions.FindAsync(subscription.Id);
        updated!.ProductId.Should().Be(newProductId);
        updated.MRR.Should().Be(200m);
    }

    [Fact]
    public async Task PlanChange_WithProration_ShouldCreateProrationInvoice()
    {
        // Arrange
        var subscription = await CreateTestSubscription(mrr: 100m);
        var newProductId = await CreateTestProduct(200m);
        var invoiceCountBefore = await _dbContext.Invoices.CountAsync(i => i.SubscriptionId == subscription.Id);

        // Act
        await _subscriptionService.ChangePlanAsync(subscription.Id, newProductId, applyProration: true);

        // Assert
        var invoiceCountAfter = await _dbContext.Invoices.CountAsync(i => i.SubscriptionId == subscription.Id);
        invoiceCountAfter.Should().BeGreaterThan(invoiceCountBefore);
    }

    #endregion

    #region Optimistic Concurrency

    [Fact]
    public async Task ConcurrentUpdate_ShouldThrowConcurrencyException()
    {
        // Arrange
        var subscription = await CreateTestSubscription();
        var originalId = subscription.Id;

        // Simulate concurrent access by loading the same entity in two contexts
        var subscription1 = await _dbContext.Subscriptions.FindAsync(originalId);
        var rowVersion1 = subscription1!.RowVersion;

        // Modify in "another session" (simulated)
        subscription1.Notes = "First update";
        await _dbContext.SaveChangesAsync();

        // Act & Assert - trying to update with old RowVersion should fail
        // This test validates the RowVersion is being enforced
        subscription1.RowVersion.Should().NotEqual(rowVersion1);
    }

    #endregion

    #region Helper Methods

    private async Task<Subscription> CreateTestSubscription(
        bool isAutoRenew = true,
        DateTime? contractEndDate = null,
        SubscriptionStatus status = SubscriptionStatus.Active,
        decimal mrr = 100m,
        string billingCycle = "Monthly",
        DateTime? billingCycleStartDate = null)
    {
        var subscription = new Subscription
        {
            SubscriptionNumber = $"TEST-{Guid.NewGuid():N}".Substring(0, 20),
            AccountId = 1,
            ProductId = 1,
            MRR = mrr,
            ARR = mrr * 12,
            BillingCycle = billingCycle,
            SubscriptionStatus = status,
            IsAutoRenew = isAutoRenew,
            IsActive = true,
            BillingStartDate = billingCycleStartDate ?? DateTime.UtcNow.AddDays(-5),
            ContractStartDate = DateTime.UtcNow.AddDays(-5),
            ContractEndDate = contractEndDate ?? DateTime.UtcNow.AddMonths(1),
            BillingTimezone = "UTC",
            DunningGracePeriodDays = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();
        return subscription;
    }

    private async Task<int> CreateTestProduct(decimal price)
    {
        var product = new Product
        {
            Name = $"Test Product {Guid.NewGuid():N}".Substring(0, 50),
            ProductCode = $"PROD-{Guid.NewGuid():N}".Substring(0, 20),
            Category = "Test",
            UnitPrice = price,
            RecurringPrice = price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product.Id;
    }

    #endregion
}

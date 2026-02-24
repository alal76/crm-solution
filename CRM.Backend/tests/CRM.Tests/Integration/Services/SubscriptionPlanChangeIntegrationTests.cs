// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Services;

/// <summary>
/// Integration tests for plan change with proration.
/// TODO-SALES006-047: Test plan changes with proper proration calculations.
/// </summary>
public class SubscriptionPlanChangeIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionPlanChangeIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CrmTestDb_PlanChange_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options);

        var logger = new Mock<ILogger<SubscriptionService>>();
        _subscriptionService = new SubscriptionService(_context, logger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test account
        var account = new Account
        {
            Id = 1,
            Name = "Plan Change Test Company",
            Email = "test@planchange.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Customers.Add(account);

        // Create products/plans
        var basicPlan = new Product
        {
            Id = 1,
            Name = "Basic Plan",
            UnitPrice = 50m,
            ProductType = "Subscription",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var standardPlan = new Product
        {
            Id = 2,
            Name = "Standard Plan",
            UnitPrice = 100m,
            ProductType = "Subscription",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var premiumPlan = new Product
        {
            Id = 3,
            Name = "Premium Plan",
            UnitPrice = 200m,
            ProductType = "Subscription",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var enterprisePlan = new Product
        {
            Id = 4,
            Name = "Enterprise Plan",
            UnitPrice = 500m,
            ProductType = "Subscription",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.AddRange(basicPlan, standardPlan, premiumPlan, enterprisePlan);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ImmediateUpgrade_ShouldUpdateProductAndMRRImmediately()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-UPGRADE-0001",
            AccountId = 1,
            ProductId = 1, // Basic
            MRR = 50m,
            ARR = 600m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            BillingStartDate = DateTime.UtcNow.AddDays(-15),
            BillingEndDate = DateTime.UtcNow.AddDays(15),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act - Upgrade to Premium
        var upgraded = await _subscriptionService.UpgradeAsync(subscription.Id, 3, immediate: true);

        // Assert
        upgraded.ProductId.Should().Be(3);
        upgraded.MRR.Should().Be(200m);
        upgraded.ARR.Should().Be(2400m);
    }

    [Fact]
    public async Task ScheduledUpgrade_ShouldNotChangeProductImmediately()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-UPGRADE-0002",
            AccountId = 1,
            ProductId = 1,
            MRR = 50m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionService.UpgradeAsync(subscription.Id, 2, immediate: false);

        // Assert - Product and MRR unchanged
        result.ProductId.Should().Be(1);
        result.MRR.Should().Be(50m);
        result.ContractNotes.Should().Contain("next billing cycle");
    }

    [Fact]
    public async Task Downgrade_ShouldScheduleForEndOfPeriod()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-DOWNGRADE-0001",
            AccountId = 1,
            ProductId = 3, // Premium
            MRR = 200m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act - Downgrade to Basic
        var downgraded = await _subscriptionService.DowngradeAsync(subscription.Id, 1);

        // Assert - Not changed immediately
        downgraded.ProductId.Should().Be(3);
        downgraded.MRR.Should().Be(200m);
        downgraded.ContractNotes.Should().Contain("end of period");
    }

    [Fact]
    public async Task ChangePlan_Immediate_ShouldRecalculateARR()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-CHANGE-0001",
            AccountId = 1,
            ProductId = 1,
            MRR = 50m,
            ARR = 600m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act - Change to Enterprise
        var changed = await _subscriptionService.ChangePlanAsync(
            subscription.Id, 4, SubscriptionChangeType.Immediate);

        // Assert
        changed.MRR.Should().Be(500m);
        changed.ARR.Should().Be(6000m);
    }

    [Fact]
    public async Task ChangePlan_NextBillingCycle_ShouldScheduleChange()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-CHANGE-0002",
            AccountId = 1,
            ProductId = 2,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionService.ChangePlanAsync(
            subscription.Id, 3, SubscriptionChangeType.NextBillingCycle);

        // Assert
        result.ProductId.Should().Be(2);
        result.MRR.Should().Be(100m);
        result.ContractNotes.Should().Contain("Premium Plan");
    }

    [Fact]
    public async Task ChangePlan_EndOfPeriod_ShouldScheduleChange()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-CHANGE-0003",
            AccountId = 1,
            ProductId = 2,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionService.ChangePlanAsync(
            subscription.Id, 1, SubscriptionChangeType.EndOfPeriod);

        // Assert
        result.ContractNotes.Should().Contain("end of period");
    }

    [Fact]
    public async Task AddAddon_ShouldIncreaseMRRImmediately()
    {
        // Arrange
        var addon = new Product
        {
            Id = 10,
            Name = "Extra Storage",
            UnitPrice = 25m,
            ProductType = "Addon",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Products.Add(addon);

        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-ADDON-0001",
            AccountId = 1,
            ProductId = 2,
            MRR = 100m,
            ARR = 1200m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionService.AddAddonAsync(subscription.Id, addon.Id, quantity: 2);

        // Assert
        result.MRR.Should().Be(150m); // 100 + (25 * 2)
        result.ARR.Should().Be(1800m);
    }

    [Fact]
    public async Task RemoveAddon_ShouldDecreaseMRR()
    {
        // Arrange
        var addon = new Product
        {
            Id = 11,
            Name = "Support Package",
            UnitPrice = 30m,
            ProductType = "Addon",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Products.Add(addon);

        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-ADDON-0002",
            AccountId = 1,
            ProductId = 2,
            MRR = 130m, // Base 100 + addon 30
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionService.RemoveAddonAsync(subscription.Id, addon.Id);

        // Assert
        result.MRR.Should().Be(100m);
    }

    [Fact]
    public async Task ChangePlan_InvalidPlan_ShouldThrow()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-INVALID-0001",
            AccountId = 1,
            ProductId = 1,
            MRR = 50m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var act = async () => await _subscriptionService.ChangePlanAsync(
            subscription.Id, 9999, SubscriptionChangeType.Immediate);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*9999*not found*");
    }

    [Fact]
    public async Task ChangePlan_WithProration_ShouldCalculateProrated()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-PRORATE-0001",
            AccountId = 1,
            ProductId = 1,
            MRR = 50m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            BillingStartDate = DateTime.UtcNow.AddDays(-15),
            BillingEndDate = DateTime.UtcNow.AddDays(15),
            ProrationType = ProrationStrategy.Daily,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var proratedAmount = await _subscriptionService.CalculateProratedAmountAsync(
            subscription.Id, 200m, DateTime.UtcNow);

        // Assert - 15 days remaining of 30, new price 200
        proratedAmount.Should().BeApproximately(100m, 1m);
    }
}

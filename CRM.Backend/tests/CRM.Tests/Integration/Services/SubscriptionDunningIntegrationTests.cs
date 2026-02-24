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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Services;

/// <summary>
/// Integration tests for subscription dunning retry + cancellation workflow.
/// TODO-SALES006-046: Test dunning retry logic, grace periods, and eventual cancellation.
/// </summary>
public class SubscriptionDunningIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly SubscriptionService _service;

    public SubscriptionDunningIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CrmTestDb_Dunning_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options, new Mock<IConfiguration>().Object);

        var logger = new Mock<ILogger<SubscriptionService>>();
        _service = new SubscriptionService(_context, logger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var account = new Account
        {
            Id = 1,
            LegalName = "Dunning Test Company",
            Email = "billing@dunningtestco.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ========================================================================
    // Suspend on Payment Failure
    // ========================================================================

    [Fact]
    public async Task SuspendForNonPayment_ShouldSetStatusToSuspended()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-DUNNING-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningGracePeriodDays = 3,
            SendDunningEscalationEmails = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var suspended = await _service.SuspendAsync(subscription.Id, "Payment failed after 3 retries");

        // Assert
        suspended.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);
        suspended.ContractNotes.Should().Contain("Suspended");
        suspended.ContractNotes.Should().Contain("Payment failed");
    }

    [Fact]
    public async Task CancelAfterDunningExhausted_ShouldSetStatusToCancelled()
    {
        // Arrange - Subscription already suspended from dunning
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-DUNNING-0002",
            AccountId = 1,
            MRR = 200m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Suspended,
            DunningAttemptCount = 3,
            LastDunningDate = DateTime.UtcNow.AddDays(-7),
            DunningGracePeriodDays = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var cancelled = await _service.CancelAsync(subscription.Id, "Dunning exhausted - auto-cancellation", immediate: true);

        // Assert
        cancelled.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
        cancelled.ContractEndDate.Should().NotBeNull();
        cancelled.ContractNotes.Should().Contain("Dunning exhausted");
    }

    [Fact]
    public async Task ReactivateAfterPaymentRecovery_ShouldResetToActive()
    {
        // Arrange - Subscription suspended due to payment failure
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-DUNNING-0003",
            AccountId = 1,
            MRR = 150m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Suspended,
            DunningAttemptCount = 2,
            LastDunningDate = DateTime.UtcNow.AddDays(-2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act - Customer resolves payment issue
        var reactivated = await _service.ReactivateAsync(subscription.Id);

        // Assert
        reactivated.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    // ========================================================================
    // Grace Period Scenarios
    // ========================================================================

    [Fact]
    public async Task DunningGracePeriod_ShouldBeConfigurable()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-GRACE-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningGracePeriodDays = 7, // Extended grace period
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var loaded = await _service.GetByIdAsync(subscription.Id);
        loaded.Should().NotBeNull();
        loaded!.DunningGracePeriodDays.Should().Be(7);
    }

    [Fact]
    public async Task DunningGracePeriod_Default_ShouldBeThreeDays()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-GRACE-0002",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var loaded = await _service.GetByIdAsync(subscription.Id);
        loaded.Should().NotBeNull();
        loaded!.DunningGracePeriodDays.Should().Be(3);
    }

    // ========================================================================
    // Dunning Retry Counting
    // ========================================================================

    [Fact]
    public async Task DunningAttemptCount_ShouldPersistAcrossUpdates()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-RETRY-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningAttemptCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act - Simulate dunning attempts by updating directly
        subscription.DunningAttemptCount = 1;
        subscription.LastDunningDate = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        subscription.DunningAttemptCount = 2;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var loaded = await _service.GetByIdAsync(subscription.Id);
        loaded.Should().NotBeNull();
        loaded!.DunningAttemptCount.Should().Be(2);
        loaded.LastDunningDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DunningSuspension_ShouldTrackAttemptCount()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-RETRY-0002",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningAttemptCount = 3,
            LastDunningDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var suspended = await _service.SuspendAsync(subscription.Id, "Max retry attempts reached");

        // Assert
        suspended.DunningAttemptCount.Should().Be(3);
        suspended.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);
    }

    // ========================================================================
    // Full Dunning Lifecycle
    // ========================================================================

    [Fact]
    public async Task FullDunningLifecycle_Active_Suspend_Cancel()
    {
        // Arrange - Start with active subscription
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-LIFECYCLE-0001",
            AccountId = 1,
            MRR = 250m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningGracePeriodDays = 3,
            SendDunningEscalationEmails = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act Step 1: Suspend for payment failure
        var suspended = await _service.SuspendAsync(subscription.Id, "Payment failed - retry 1");
        suspended.SubscriptionStatus.Should().Be(SubscriptionStatus.Suspended);

        // Act Step 2: Cancel after exhausting dunning retries
        var cancelled = await _service.CancelAsync(subscription.Id, "All dunning retries exhausted", immediate: true);
        cancelled.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
        cancelled.ContractEndDate.Should().NotBeNull();
    }

    [Fact]
    public async Task FullDunningLifecycle_Active_Suspend_Reactivate()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-LIFECYCLE-0002",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act Step 1: Suspend
        await _service.SuspendAsync(subscription.Id, "Payment failed");

        // Act Step 2: Reactivate after payment recovered
        var reactivated = await _service.ReactivateAsync(subscription.Id);

        // Assert
        reactivated.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        reactivated.MRR.Should().Be(100m); // MRR preserved
    }

    // ========================================================================
    // Dunning Email Configuration
    // ========================================================================

    [Fact]
    public async Task DunningNotificationEmails_ShouldBePersistable()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-EMAIL-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            DunningNotificationEmails = "billing@company.com,finance@company.com",
            SendDunningEscalationEmails = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Assert
        var loaded = await _service.GetByIdAsync(subscription.Id);
        loaded.Should().NotBeNull();
        loaded!.DunningNotificationEmails.Should().Be("billing@company.com,finance@company.com");
        loaded.SendDunningEscalationEmails.Should().BeTrue();
    }
}

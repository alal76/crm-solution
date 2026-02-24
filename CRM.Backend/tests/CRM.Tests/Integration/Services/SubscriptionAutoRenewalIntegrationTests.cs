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
/// Integration tests for subscription auto-renewal workflow.
/// TODO-SALES006-045: Test auto-renewal logic end-to-end with database.
/// </summary>
public class SubscriptionAutoRenewalIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly SubscriptionService _service;

    public SubscriptionAutoRenewalIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CrmTestDb_AutoRenewal_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options);

        var logger = new Mock<ILogger<SubscriptionService>>();
        _service = new SubscriptionService(_context, logger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test account
        var account = new Account
        {
            Id = 1,
            Name = "Test Company",
            Email = "test@company.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Customers.Add(account);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task FullRenewalWorkflow_ShouldCreateNewPeriod()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-RENEW-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            ContractStartDate = DateTime.UtcNow.AddMonths(-1),
            ContractEndDate = DateTime.UtcNow.AddDays(5),
            IsAutoRenew = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var renewed = await _service.RenewAsync(subscription.Id);

        // Assert
        renewed.ContractStartDate.Should().Be(subscription.ContractEndDate);
        renewed.ContractEndDate.Should().BeAfter(renewed.ContractStartDate!.Value);
        renewed.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task GetDueForRenewal_ShouldReturnUpcomingRenewals()
    {
        // Arrange
        var soon = new Subscription
        {
            SubscriptionNumber = "SUB-RENEW-0002",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            ContractEndDate = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var later = new Subscription
        {
            SubscriptionNumber = "SUB-RENEW-0003",
            AccountId = 1,
            MRR = 200m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            ContractEndDate = DateTime.UtcNow.AddDays(45),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.AddRange(soon, later);
        await _context.SaveChangesAsync();

        // Act
        var dueForRenewal = await _service.GetDueForRenewalAsync(withinDays: 30);

        // Assert
        dueForRenewal.Should().HaveCount(1);
        dueForRenewal.First().SubscriptionNumber.Should().Be("SUB-RENEW-0002");
    }

    [Fact]
    public async Task RenewalWithYearlyPlan_ShouldExtendBy365Days()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-ANNUAL-0001",
            AccountId = 1,
            MRR = 1000m,
            BillingCycle = "Yearly",
            SubscriptionStatus = SubscriptionStatus.Active,
            ContractStartDate = DateTime.UtcNow.AddYears(-1),
            ContractEndDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var renewed = await _service.RenewAsync(subscription.Id);

        // Assert
        var extendedDays = (renewed.ContractEndDate - renewed.ContractStartDate)!.Value.Days;
        extendedDays.Should().BeApproximately(365, 1);
    }

    [Fact]
    public async Task SetAutoRenewal_ShouldUpdateSubscription()
    {
        // Arrange
        var subscription = new Subscription
        {
            SubscriptionNumber = "SUB-AUTO-0001",
            AccountId = 1,
            MRR = 100m,
            BillingCycle = "Monthly",
            SubscriptionStatus = SubscriptionStatus.Active,
            IsAutoRenew = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // Act
        var updated = await _service.SetAutoRenewalAsync(subscription.Id, true);

        // Assert
        updated.ContractNotes.Should().Contain("Auto-renewal enabled");
    }
}

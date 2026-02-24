// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
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
/// Integration tests for dunning retry and cancellation flow.
/// TODO-SALES006-046: Test dunning retry logic with escalation and eventual cancellation.
/// </summary>
public class DunningRetryIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly DunningManager _dunningManager;

    public DunningRetryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CrmTestDb_Dunning_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options, new Mock<IConfiguration>().Object);

        var logger = new Mock<ILogger<DunningManager>>();
        _dunningManager = new DunningManager(_context, logger.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test account
        var account = new Account
        {
            Id = 1,
            LegalName = "Dunning Test Company",
            Email = "test@dunning.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(account);

        // Create test subscription
        var subscription = new Subscription
        {
            Id = 1,
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

        // Create test invoice
        var invoice = new Invoice
        {
            Id = 1,
            SubscriptionId = 1,
            InvoiceNumber = "INV-DUNNING-0001",
            InvoiceDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(-7),
            TotalAmount = 100m,
            Status = InvoiceStatus.Overdue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ProcessDunning_ShouldFindFailedPayments()
    {
        // Arrange
        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            Amount = 100m,
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dunningManager.ProcessDunningAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProcessedCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task RetryFailedPayment_FirstAttempt_ShouldScheduleNextRetryIn3Days()
    {
        // Arrange
        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            Amount = 100m,
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dunningManager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EscalationLevel.Should().Be(DunningEscalationLevel.Soft);

        // Verify payment was updated
        var updatedPayment = await _context.Payments.FindAsync(failedPayment.Id);
        updatedPayment!.RetryCount.Should().Be(1);
        updatedPayment.ScheduledDate!.Value.Date.Should().Be(DateTime.UtcNow.AddDays(3).Date);
    }

    [Fact]
    public async Task RetryFailedPayment_SecondAttempt_ShouldEscalate()
    {
        // Arrange
        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            Amount = 100m,
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 1, // Already tried once
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dunningManager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert
        result.EscalationLevel.Should().Be(DunningEscalationLevel.Escalated);

        var updatedPayment = await _context.Payments.FindAsync(failedPayment.Id);
        updatedPayment!.RetryCount.Should().Be(2);
    }

    [Fact]
    public async Task RetryFailedPayment_ThirdAttempt_ShouldBeFinalAndPauseSubscription()
    {
        // Arrange
        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            Amount = 100m,
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 2, // Already tried twice
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dunningManager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert
        result.EscalationLevel.Should().Be(DunningEscalationLevel.Final);

        var subscription = await _context.Subscriptions.FindAsync(1);
        subscription!.SubscriptionStatus.Should().Be(SubscriptionStatus.Paused);
    }

    [Fact]
    public async Task DunningCycle_ShouldReportMetrics()
    {
        // Arrange - Add multiple failed payments
        _context.Payments.AddRange(new[]
        {
            new Payment { InvoiceId = 1, SubscriptionId = 1, Amount = 100m, Status = PaymentStatus.Failed, ScheduledDate = DateTime.UtcNow.AddDays(-1), RetryCount = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Payment { InvoiceId = 1, SubscriptionId = 1, Amount = 50m, Status = PaymentStatus.Failed, ScheduledDate = DateTime.UtcNow.AddDays(-2), RetryCount = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _dunningManager.ProcessDunningAsync(CancellationToken.None);

        // Assert
        result.ProcessedCount.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task RetryFailedPayment_NotFound_ShouldThrow()
    {
        // Act
        var act = async () => await _dunningManager.RetryFailedPaymentAsync(9999, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task DunningCycle_ShouldHandleEmptyQueue()
    {
        // Arrange - No failed payments

        // Act
        var result = await _dunningManager.ProcessDunningAsync(CancellationToken.None);

        // Assert
        result.ProcessedCount.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }
}

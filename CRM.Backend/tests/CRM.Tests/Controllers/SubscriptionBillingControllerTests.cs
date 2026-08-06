// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for SubscriptionBillingController's dunning retry endpoint.
/// Verifies that POST retry-dunning delegates to IDunningManager.RetryFailedPaymentAsync
/// instead of directly bumping DunningRecord fields, and returns the manager's actual result.
/// SubscriptionBillingController uses ICrmDbContext directly, so EF InMemory (via CrmDbContext)
/// is used as the real ICrmDbContext implementation, matching the pattern in TasksControllerTests.
/// </summary>
public class SubscriptionBillingControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<IInvoiceService> _mockInvoiceService;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IDunningManager> _mockDunningManager;
    private readonly SubscriptionBillingController _controller;

    public SubscriptionBillingControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SubscriptionBillingTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockInvoiceService = new Mock<IInvoiceService>();
        _mockPaymentService = new Mock<IPaymentService>();
        _mockDunningManager = new Mock<IDunningManager>();
        var logger = new Mock<ILogger<SubscriptionBillingController>>();

        _controller = new SubscriptionBillingController(
            _mockSubscriptionService.Object,
            _mockInvoiceService.Object,
            _mockPaymentService.Object,
            _mockDunningManager.Object,
            _dbContext,
            logger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private static Subscription MakeSubscription(int id = 1) => new()
    {
        Id = id,
        SubscriptionNumber = $"SUB-{id:0000}",
        AccountId = 1,
        BillingCycle = "Monthly",
        SubscriptionStatus = SubscriptionStatus.Active,
        Amount = 100m,
        StartDate = DateTime.UtcNow.AddMonths(-1),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private async Task<(DunningRecord dunning, Payment payment)> SeedActiveDunningWithFailedPaymentAsync(int subscriptionId)
    {
        var invoice = new Invoice
        {
            SubscriptionId = subscriptionId,
            AccountId = 1,
            InvoiceNumber = $"INV-{subscriptionId:0000}",
            InvoiceDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(-7),
            TotalAmount = 100m,
            Status = InvoiceStatus.Overdue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            SubscriptionId = subscriptionId,
            AccountId = 1,
            Amount = 100m,
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Payments.Add(payment);

        var dunning = new DunningRecord
        {
            SubscriptionId = subscriptionId,
            InvoiceId = invoice.Id,
            RetryAttempt = 1,
            NextRetryDate = DateTime.UtcNow.AddDays(-1),
            Status = DunningStatus.Active,
            InitialFailureDate = DateTime.UtcNow.AddDays(-10),
            OutstandingAmount = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.DunningRecords.Add(dunning);

        await _dbContext.SaveChangesAsync();

        return (dunning, payment);
    }

    [Fact]
    public async Task RetryDunning_ShouldCallDunningManager_NotBumpDbRecordDirectly()
    {
        var subscriptionId = 1;
        _mockSubscriptionService
            .Setup(s => s.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSubscription(subscriptionId));

        var (dunning, payment) = await SeedActiveDunningWithFailedPaymentAsync(subscriptionId);
        var originalRetryAttempt = dunning.RetryAttempt;
        var originalNextRetryDate = dunning.NextRetryDate;

        var managerResult = new DunningRetryResultDto
        {
            PaymentId = payment.Id,
            AttemptNumber = 2,
            PaymentSucceeded = false,
            Status = "Scheduled",
            Message = "Retry scheduled",
            NextRetryDate = DateTime.UtcNow.AddDays(7),
            EscalationLevel = DunningEscalationLevel.Escalated,
            IsExhausted = false
        };

        _mockDunningManager
            .Setup(m => m.RetryFailedPaymentAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerResult);

        var result = await _controller.RetryDunning(
            subscriptionId,
            new SubscriptionBillingController.RetryDunningRequest(),
            CancellationToken.None);

        // The controller must delegate to IDunningManager rather than mutating the DB directly.
        _mockDunningManager.Verify(
            m => m.RetryFailedPaymentAsync(payment.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        result.Result.Should().BeOfType<AcceptedResult>();
        var accepted = (AcceptedResult)result.Result!;
        var response = accepted.Value.Should().BeOfType<SubscriptionBillingController.RetryDunningResponse>().Subject;

        // Response must reflect the manager's actual result, not fabricated/incremented values.
        response.DunningRecordId.Should().Be(dunning.Id);
        response.SubscriptionId.Should().Be(subscriptionId);
        response.RetryAttempt.Should().Be(managerResult.AttemptNumber);
        response.NextRetryDate.Should().Be(managerResult.NextRetryDate);
        response.Status.Should().Be(managerResult.Status);

        // The DunningRecord row itself must NOT have been bumped directly by the controller
        // (that was the old buggy behavior being replaced).
        var persisted = await _dbContext.DunningRecords.FindAsync(dunning.Id);
        persisted!.RetryAttempt.Should().Be(originalRetryAttempt);
        persisted.NextRetryDate.Should().Be(originalNextRetryDate);
    }

    [Fact]
    public async Task RetryDunning_ShouldReturnNotFound_WhenSubscriptionMissing()
    {
        _mockSubscriptionService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var result = await _controller.RetryDunning(
            999,
            new SubscriptionBillingController.RetryDunningRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockDunningManager.Verify(
            m => m.RetryFailedPaymentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RetryDunning_ShouldReturnNotFound_WhenNoActiveDunningRecord()
    {
        var subscriptionId = 2;
        _mockSubscriptionService
            .Setup(s => s.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSubscription(subscriptionId));

        var result = await _controller.RetryDunning(
            subscriptionId,
            new SubscriptionBillingController.RetryDunningRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockDunningManager.Verify(
            m => m.RetryFailedPaymentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RetryDunning_ShouldReturnNotFound_WhenNoFailedPaymentBacksDunningRecord()
    {
        var subscriptionId = 3;
        _mockSubscriptionService
            .Setup(s => s.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSubscription(subscriptionId));

        var invoice = new Invoice
        {
            SubscriptionId = subscriptionId,
            AccountId = 1,
            InvoiceNumber = "INV-0003",
            InvoiceDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(-7),
            TotalAmount = 100m,
            Status = InvoiceStatus.Overdue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        var dunning = new DunningRecord
        {
            SubscriptionId = subscriptionId,
            InvoiceId = invoice.Id,
            RetryAttempt = 1,
            NextRetryDate = DateTime.UtcNow.AddDays(-1),
            Status = DunningStatus.Active,
            InitialFailureDate = DateTime.UtcNow.AddDays(-10),
            OutstandingAmount = 100m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.DunningRecords.Add(dunning);
        await _dbContext.SaveChangesAsync();
        // No failed Payment seeded for this invoice.

        var result = await _controller.RetryDunning(
            subscriptionId,
            new SubscriptionBillingController.RetryDunningRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockDunningManager.Verify(
            m => m.RetryFailedPaymentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RetryDunning_ShouldReturnNullNextRetryDate_WhenManagerReportsExhausted()
    {
        var subscriptionId = 4;
        _mockSubscriptionService
            .Setup(s => s.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSubscription(subscriptionId));

        var (dunning, payment) = await SeedActiveDunningWithFailedPaymentAsync(subscriptionId);

        var managerResult = new DunningRetryResultDto
        {
            PaymentId = payment.Id,
            AttemptNumber = 4,
            PaymentSucceeded = false,
            Status = "Exhausted",
            Message = "Dunning exhausted, subscription cancelled",
            NextRetryDate = null,
            EscalationLevel = DunningEscalationLevel.Exhausted,
            IsExhausted = true
        };

        _mockDunningManager
            .Setup(m => m.RetryFailedPaymentAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerResult);

        var result = await _controller.RetryDunning(
            subscriptionId,
            new SubscriptionBillingController.RetryDunningRequest(),
            CancellationToken.None);

        result.Result.Should().BeOfType<AcceptedResult>();
        var accepted = (AcceptedResult)result.Result!;
        var response = accepted.Value.Should().BeOfType<SubscriptionBillingController.RetryDunningResponse>().Subject;

        response.NextRetryDate.Should().BeNull();
        response.Status.Should().Be("Exhausted");
        response.RetryAttempt.Should().Be(4);

        _ = dunning; // dunning record identity already asserted via DunningRecordId in prior test
    }
}

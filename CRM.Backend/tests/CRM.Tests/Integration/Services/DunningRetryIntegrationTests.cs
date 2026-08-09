// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http;
using System.Text;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.Stripe;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Services;

/// <summary>
/// Integration tests for dunning retry and cancellation flow.
/// TODO-SALES006-046: Test dunning retry logic with escalation and eventual cancellation.
/// REV-STUB-012/013: Also covers the real Stripe charge attempt path (success + decline) and
/// the "no payment method on file" fallback path, using a fake HttpMessageHandler
/// (<see cref="CRM.Tests.Services.StripeMockHandler"/>) so StripeIntegrationService never
/// makes a real network call — mirrors the pattern in StripeIntegrationServiceTests.
/// </summary>
public class DunningRetryIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly DunningManager _dunningManager;
    private readonly CRM.Tests.Services.StripeMockHandler _defaultStripeHandler;

    public DunningRetryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CrmTestDb_Dunning_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(options, new Mock<IConfiguration>().Object);

        var logger = new Mock<ILogger<DunningManager>>();
        // Default: Stripe is never actually reached in most tests below because seeded
        // subscriptions have no StripeCustomerId/StripePaymentMethodId. The response body here
        // is unused unless a test opts in via CreateManagerWithStripeResponse.
        var stripeService = CreateStripeService(HttpStatusCode.OK, "{}", out _defaultStripeHandler);
        _dunningManager = new DunningManager(_context, logger.Object, stripeService);

        SeedTestData();
    }

    // ── Stripe test helpers (mirrors StripeIntegrationServiceTests' StripeMockHandler pattern) ──

    private static StripeConfiguration ValidStripeConfig() => new()
    {
        SecretKey = "sk_test_fake",
        PublishableKey = "pk_test_fake",
        WebhookSecret = "whsec_test_fake",
        WebhookToleranceSeconds = 300,
        ApiVersion = "2024-06-20"
    };

    private static StripeIntegrationService CreateStripeService(
        HttpStatusCode responseStatus,
        string responseBody,
        out CRM.Tests.Services.StripeMockHandler handler)
    {
        handler = new CRM.Tests.Services.StripeMockHandler(responseStatus, responseBody);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(ValidStripeConfig());
        var logger = new Mock<ILogger<StripeIntegrationService>>();
        return new StripeIntegrationService(options, httpClient, logger.Object);
    }

    /// <summary>
    /// Builds a DunningManager wired to a StripeIntegrationService whose HTTP layer is faked to
    /// return the given canned response — used by tests that need a real (successful or
    /// declined) charge attempt.
    /// </summary>
    private DunningManager CreateManagerWithStripeResponse(HttpStatusCode status, string body)
    {
        var stripeService = CreateStripeService(status, body, out _);
        var logger = new Mock<ILogger<DunningManager>>();
        return new DunningManager(_context, logger.Object, stripeService);
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

    // ========================================================================
    // TODO-SALES006-025: Grace Period Tests
    // ========================================================================

    [Fact]
    public async Task RetryFailedPayment_ShouldSkipDunning_WhenWithinGracePeriod()
    {
        // Arrange — invoice due date is today, grace period is 3 days → still within grace
        var invoice = await _context.Invoices.FindAsync(1);
        invoice!.DueDate = DateTime.UtcNow.Date;  // Due today → grace expires today+3
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
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
        result.SkippedDueToGracePeriod.Should().BeTrue();
        result.Status.Should().Be("GracePeriod");
        result.AttemptNumber.Should().Be(0);  // No attempt was made

        // Verify payment was NOT modified
        var payment = await _context.Payments.FindAsync(failedPayment.Id);
        payment!.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task RetryFailedPayment_ShouldProcessDunning_WhenGracePeriodExpired()
    {
        // Arrange — invoice due 10 days ago, grace period is 3 days → expired 7 days ago
        var invoice = await _context.Invoices.FindAsync(1);
        invoice!.DueDate = DateTime.UtcNow.AddDays(-10);
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
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
        result.SkippedDueToGracePeriod.Should().BeFalse();
        result.AttemptNumber.Should().Be(1);  // First actual attempt
        result.EscalationLevel.Should().Be(DunningEscalationLevel.Soft);
    }

    [Fact]
    public async Task RetryFailedPayment_ShouldSkipDunning_WhenGracePeriodEndsExactlyToday()
    {
        // Arrange — due date + grace period = today → still within period (boundary inclusive)
        var invoice = await _context.Invoices.FindAsync(1);
        invoice!.DueDate = DateTime.UtcNow.AddDays(-3);  // 3 days ago + 3 grace = today
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
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

        // Assert — grace period inclusive of boundary date
        result.SkippedDueToGracePeriod.Should().BeTrue();
    }

    [Fact]
    public async Task RetryFailedPayment_ShouldUpdateDunningTracking_AfterSuccessfulAttempt()
    {
        // Arrange — invoice overdue so grace period is expired
        var invoice = await _context.Invoices.FindAsync(1);
        invoice!.DueDate = DateTime.UtcNow.AddDays(-20);
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
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
        await _dunningManager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert — subscription tracking fields are updated
        var subscription = await _context.Subscriptions.FindAsync(1);
        subscription!.LastDunningDate.Should().NotBeNull();
        subscription.LastDunningDate!.Value.Date.Should().Be(DateTime.UtcNow.Date);
        subscription.DunningAttemptCount.Should().Be(1);
    }

    // ========================================================================
    // REV-STUB-012/013: Real Stripe charge attempt on dunning retry
    // ========================================================================

    [Fact]
    public async Task RetryFailedPayment_ShouldAttemptRealCharge_AndSucceed_WhenStripePaymentMethodOnFile()
    {
        // Arrange — subscription has a saved Stripe customer + payment method on file, so the
        // retry should attempt a real off-session PaymentIntent charge instead of just scheduling.
        var subscription = await _context.Subscriptions.FindAsync(1);
        subscription!.StripeCustomerId = "cus_test_123";
        subscription.StripePaymentMethodId = "pm_test_visa";
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
            Amount = 100m,
            CurrencyCode = "USD",
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        const string succeededJson = """
        {
          "id": "pi_dunning_success_1",
          "object": "payment_intent",
          "amount": 10000,
          "currency": "usd",
          "status": "succeeded",
          "client_secret": "pi_dunning_success_1_secret"
        }
        """;
        var manager = CreateManagerWithStripeResponse(HttpStatusCode.OK, succeededJson);

        // Act
        var result = await manager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert — a real charge was attempted and succeeded
        result.PaymentSucceeded.Should().BeTrue();
        result.Status.Should().Be("Succeeded");
        result.Message.Should().Contain("pi_dunning_success_1");
        result.NextRetryDate.Should().BeNull();
        result.IsExhausted.Should().BeFalse();

        var updatedPayment = await _context.Payments.FindAsync(failedPayment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Completed);
        updatedPayment.GatewayTransactionId.Should().Be("pi_dunning_success_1");
        updatedPayment.Gateway.Should().Be("Stripe");
        updatedPayment.ScheduledDate.Should().BeNull();
        updatedPayment.ProcessedDate.Should().NotBeNull();

        var updatedSubscription = await _context.Subscriptions.FindAsync(1);
        updatedSubscription!.DunningAttemptCount.Should().Be(1);
        updatedSubscription.LastDunningDate.Should().NotBeNull();
        // A successful real charge must not pause/cancel the subscription.
        updatedSubscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task RetryFailedPayment_ShouldAttemptRealCharge_AndReportDecline_WhenStripePaymentMethodOnFile()
    {
        // Arrange — saved payment method on file, but Stripe declines the charge.
        var subscription = await _context.Subscriptions.FindAsync(1);
        subscription!.StripeCustomerId = "cus_test_456";
        subscription.StripePaymentMethodId = "pm_test_declined";
        await _context.SaveChangesAsync();

        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
            Amount = 75m,
            CurrencyCode = "USD",
            Status = PaymentStatus.Failed,
            ScheduledDate = DateTime.UtcNow.AddDays(-1),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(failedPayment);
        await _context.SaveChangesAsync();

        const string declinedJson = """
        {
          "error": {
            "type": "card_error",
            "code": "card_declined",
            "decline_code": "generic_decline",
            "message": "Your card was declined."
          }
        }
        """;
        var manager = CreateManagerWithStripeResponse(HttpStatusCode.PaymentRequired, declinedJson);

        // Act
        var result = await manager.RetryFailedPaymentAsync(failedPayment.Id, CancellationToken.None);

        // Assert — this is "we tried and Stripe declined it", distinct from "no payment method"
        result.PaymentSucceeded.Should().BeFalse();
        result.Status.Should().Be("Declined");
        result.Status.Should().NotBe("NoPaymentMethodOnFile");
        result.Message.Should().Contain("declined");
        result.NextRetryDate.Should().NotBeNull();
        result.EscalationLevel.Should().Be(DunningEscalationLevel.Soft);

        var updatedPayment = await _context.Payments.FindAsync(failedPayment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Failed);
        updatedPayment.RetryCount.Should().Be(1);
        updatedPayment.ScheduledDate.Should().NotBeNull();
        updatedPayment.FailureReason.Should().Contain("declined");
    }

    [Fact]
    public async Task RetryFailedPayment_ShouldReportNoPaymentMethodOnFile_WhenSubscriptionHasNoStripeReference()
    {
        // Arrange — subscription (seeded in SeedTestData) has no StripeCustomerId/StripePaymentMethodId.
        var failedPayment = new Payment
        {
            InvoiceId = 1,
            SubscriptionId = 1,
            AccountId = 1,
            Amount = 50m,
            CurrencyCode = "USD",
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

        // Assert — distinguishable from the "Declined" case above via Status/Message
        result.PaymentSucceeded.Should().BeFalse();
        result.Status.Should().Be("NoPaymentMethodOnFile");
        result.Message.Should().Contain("No payment method on file");
        result.NextRetryDate.Should().NotBeNull();

        // Confirm Stripe was never actually called — the branch is skipped locally, not attempted and failed.
        _defaultStripeHandler.Requests.Should().BeEmpty();
    }
}

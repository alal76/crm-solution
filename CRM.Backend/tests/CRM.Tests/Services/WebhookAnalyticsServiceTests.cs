// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Threading.Tasks;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="WebhookAnalyticsService"/>.
/// Uses an InMemory <see cref="CrmDbContext"/> to avoid real database dependencies.
/// </summary>
public class WebhookAnalyticsServiceTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WebhookAnalytics_{Guid.NewGuid()}")
            .Options;
        return new CrmDbContext(options, null!);
    }

    private static WebhookAnalyticsService BuildService(CrmDbContext context)
        => new(context, Mock.Of<ILogger<WebhookAnalyticsService>>());

    // ------------------------------------------------------------------
    // GetSuccessRateAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetSuccessRateAsync_WhenNoDeliveries_ReturnsZero()
    {
        // Arrange
        await using var context = CreateContext();
        var service = BuildService(context);
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        // Act
        var rate = await service.GetSuccessRateAsync(null, start, end);

        // Assert
        rate.Should().Be(0);
    }

    [Fact]
    public async Task GetSuccessRateAsync_AllSuccessful_Returns100()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.WebhookDeliveries.AddRange(
            new WebhookDelivery { WebhookSubscriptionId = 1, TargetUrl = "http://test", Success = true, EventType = "e1", CreatedAt = now },
            new WebhookDelivery { WebhookSubscriptionId = 1, TargetUrl = "http://test", Success = true, EventType = "e2", CreatedAt = now });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var rate = await service.GetSuccessRateAsync(null, now.AddHours(-1), now.AddHours(1));

        // Assert
        rate.Should().Be(100.0);
    }

    [Fact]
    public async Task GetSuccessRateAsync_HalfSuccessful_Returns50()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.WebhookDeliveries.AddRange(
            new WebhookDelivery { WebhookSubscriptionId = 2, TargetUrl = "http://test", Success = true, EventType = "e1", CreatedAt = now },
            new WebhookDelivery { WebhookSubscriptionId = 2, TargetUrl = "http://test", Success = false, EventType = "e2", CreatedAt = now });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var rate = await service.GetSuccessRateAsync(null, now.AddHours(-1), now.AddHours(1));

        // Assert
        rate.Should().BeApproximately(50.0, 0.001);
    }

    [Fact]
    public async Task GetSuccessRateAsync_WithWebhookIdFilter_OnlyCountsMatchingWebhook()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        // webhook 10: 2 success
        context.WebhookDeliveries.AddRange(
            new WebhookDelivery { WebhookSubscriptionId = 10, TargetUrl = "http://a", Success = true, EventType = "e", CreatedAt = now },
            new WebhookDelivery { WebhookSubscriptionId = 10, TargetUrl = "http://a", Success = true, EventType = "e", CreatedAt = now },
            // webhook 20: 1 failure
            new WebhookDelivery { WebhookSubscriptionId = 20, TargetUrl = "http://b", Success = false, EventType = "e", CreatedAt = now });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var rate = await service.GetSuccessRateAsync(10, now.AddHours(-1), now.AddHours(1));

        // Assert
        rate.Should().Be(100.0);
    }

    // ------------------------------------------------------------------
    // GetAverageLatencyAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAverageLatencyAsync_NullDurationEntriesExcluded_ReturnsOnlyNonNullAverage()
    {
        // Arrange - one delivery with DurationMs set, one without; only the non-null should be averaged
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.WebhookDeliveries.AddRange(
            new WebhookDelivery { WebhookSubscriptionId = 60, TargetUrl = "http://t", Success = true, EventType = "e", DurationMs = 400, CreatedAt = now },
            new WebhookDelivery { WebhookSubscriptionId = 60, TargetUrl = "http://t", Success = false, EventType = "e", DurationMs = null, CreatedAt = now });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var latency = await service.GetAverageLatencyAsync(null, now.AddHours(-1), now.AddHours(1));

        // Assert
        latency.Should().BeApproximately(400.0, 0.001);
    }

    [Fact]
    public async Task GetAverageLatencyAsync_WithKnownDurations_ReturnsCorrectAverage()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;
        context.WebhookDeliveries.AddRange(
            new WebhookDelivery { WebhookSubscriptionId = 5, TargetUrl = "http://t", Success = true, EventType = "e", DurationMs = 100, CreatedAt = now },
            new WebhookDelivery { WebhookSubscriptionId = 5, TargetUrl = "http://t", Success = true, EventType = "e", DurationMs = 200, CreatedAt = now });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var latency = await service.GetAverageLatencyAsync(null, now.AddHours(-1), now.AddHours(1));

        // Assert
        latency.Should().BeApproximately(150.0, 0.001);
    }

    // ------------------------------------------------------------------
    // GetTopFailuresAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetTopFailuresAsync_WhenNoFailures_ReturnsEmpty()
    {
        // Arrange
        await using var context = CreateContext();
        var service = BuildService(context);

        // Act
        var failures = await service.GetTopFailuresAsync(5, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Assert
        failures.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopFailuresAsync_ReturnsTopNByFailureCount()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        // Seed subscriptions so Include(d => d.Subscription) resolves (required FK = inner join semantics in InMemory)
        context.WebhookSubscriptions.AddRange(
            new WebhookSubscription { WebhookSubscriptionId = 30, Name = "Sub30", TargetUrl = "http://fail1" },
            new WebhookSubscription { WebhookSubscriptionId = 31, Name = "Sub31", TargetUrl = "http://fail2" });

        // webhook 30: 3 failures; webhook 31: 1 failure
        for (int i = 0; i < 3; i++)
        {
            context.WebhookDeliveries.Add(new WebhookDelivery
            {
                WebhookSubscriptionId = 30, TargetUrl = "http://fail1", Success = false,
                EventType = "e", ErrorMessage = "timeout", CreatedAt = now
            });
        }
        context.WebhookDeliveries.Add(new WebhookDelivery
        {
            WebhookSubscriptionId = 31, TargetUrl = "http://fail2", Success = false,
            EventType = "e", ErrorMessage = "404", CreatedAt = now
        });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var failures = await service.GetTopFailuresAsync(5, now.AddHours(-1), now.AddHours(1));

        // Assert
        failures.Should().HaveCount(2);
        var list = new System.Collections.Generic.List<WebhookFailureInfo>(failures);
        list[0].WebhookId.Should().Be(30); // highest failure count first
        list[0].FailureCount.Should().Be(3);
    }
}

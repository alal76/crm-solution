// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SALES006-024: Unit tests for usage record batch buffer and hosted service.

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Billing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Tests for the usage record batching infrastructure (TODO-SALES006-024):
/// <see cref="UsageRecordBatchBuffer"/> and <see cref="UsageRecordBatchHostedService"/>.
/// </summary>
public class UsageRecordBatchTests
{
    private static UsageRecordBatchBuffer MakeBuffer() =>
        new(NullLogger<UsageRecordBatchBuffer>.Instance);

    private static UsageRecordBatchDto MakeRecord(int subscriptionId = 1, string metric = "api_calls", decimal qty = 1) =>
        new() { SubscriptionId = subscriptionId, MetricName = metric, Quantity = qty };

    // ──────────────────────────────────────────────────────────────────────
    // UsageRecordBatchBuffer
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void Buffer_Enqueue_IncreasesQueuedCount()
    {
        var buffer = MakeBuffer();

        buffer.Enqueue(MakeRecord());
        buffer.Enqueue(MakeRecord());

        buffer.QueuedCount.Should().Be(2);
    }

    [Fact]
    public void Buffer_Drain_ReturnsEnqueuedItems()
    {
        var buffer = MakeBuffer();
        var r1 = MakeRecord(subscriptionId: 1, metric: "api_calls");
        var r2 = MakeRecord(subscriptionId: 2, metric: "storage_gb");

        buffer.Enqueue(r1);
        buffer.Enqueue(r2);

        var drained = buffer.Drain(100);

        drained.Should().HaveCount(2);
        drained.Should().Contain(r => r.SubscriptionId == 1 && r.MetricName == "api_calls");
        drained.Should().Contain(r => r.SubscriptionId == 2 && r.MetricName == "storage_gb");
    }

    [Fact]
    public void Buffer_Drain_RespectsMaxCount()
    {
        var buffer = MakeBuffer();
        for (var i = 0; i < 10; i++) buffer.Enqueue(MakeRecord(subscriptionId: i));

        var drained = buffer.Drain(3);

        drained.Should().HaveCount(3);
        buffer.QueuedCount.Should().Be(7);
    }

    [Fact]
    public void Buffer_Drain_EmptiesQueue_WhenMaxGreaterThanContents()
    {
        var buffer = MakeBuffer();
        buffer.Enqueue(MakeRecord());

        var drained = buffer.Drain(50);

        drained.Should().HaveCount(1);
        buffer.QueuedCount.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────────────────
    // UsageRecordBatchHostedService.FlushBatchAsync
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HostedService_FlushBatchAsync_CallsRecordUsageBatchAsync()
    {
        var buffer = MakeBuffer();
        buffer.Enqueue(MakeRecord(subscriptionId: 1));
        buffer.Enqueue(MakeRecord(subscriptionId: 2));

        var mockSubscriptionService = new Mock<ISubscriptionService>();
        mockSubscriptionService
            .Setup(s => s.RecordUsageBatchAsync(It.IsAny<List<UsageRecordBatchDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var serviceProvider = BuildServiceProvider(mockSubscriptionService.Object);

        var hosted = new UsageRecordBatchHostedService(
            buffer,
            serviceProvider,
            NullLogger<UsageRecordBatchHostedService>.Instance);

        await hosted.FlushBatchAsync(CancellationToken.None);

        mockSubscriptionService.Verify(
            s => s.RecordUsageBatchAsync(
                It.Is<List<UsageRecordBatchDto>>(l => l.Count == 2),
                CancellationToken.None),
            Times.Once);

        buffer.QueuedCount.Should().Be(0);
    }

    [Fact]
    public async Task HostedService_FlushBatchAsync_ReQueuesItems_OnPersistenceFailure()
    {
        var buffer = MakeBuffer();
        buffer.Enqueue(MakeRecord(subscriptionId: 10));
        buffer.Enqueue(MakeRecord(subscriptionId: 11));

        var mockSubscriptionService = new Mock<ISubscriptionService>();
        mockSubscriptionService
            .Setup(s => s.RecordUsageBatchAsync(It.IsAny<List<UsageRecordBatchDto>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB unavailable"));

        var serviceProvider = BuildServiceProvider(mockSubscriptionService.Object);

        var hosted = new UsageRecordBatchHostedService(
            buffer,
            serviceProvider,
            NullLogger<UsageRecordBatchHostedService>.Instance);

        await hosted.FlushBatchAsync(CancellationToken.None);

        // Records must be re-queued so no data is lost
        buffer.QueuedCount.Should().Be(2, "records should be re-queued when persistence fails");
    }

    private static IServiceProvider BuildServiceProvider(ISubscriptionService subscriptionService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(subscriptionService);
        return services.BuildServiceProvider();
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Threading.Tasks;
using CRM.Infrastructure.Services.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.Messaging;

/// <summary>
/// Unit tests for <see cref="DeadLetterQueueService"/>.
/// All tests use <c>redis = null</c> to exercise the in-memory fallback path
/// (no real Redis instance required).
///
/// NOTE: <c>_inMemoryQueue</c> is a static field in the production class,
/// so tests use unique SourceStream values per test to avoid cross-test interference.
/// </summary>
public class DeadLetterQueueServiceTests
{
    // Use a unique prefix per test class run so static state doesn't carry over
    private static readonly string RunId = Guid.NewGuid().ToString("N")[..8];

    private static DeadLetterQueueService BuildService()
        => new(
            redis: null,           // triggers in-memory fallback
            streamService: Mock.Of<IRedisStreamService>(),
            logger: Mock.Of<ILogger<DeadLetterQueueService>>());

    private static string UniqueSource(string testName)
        => $"test.{RunId}.{testName}";

    // ------------------------------------------------------------------
    // EnqueueAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task EnqueueAsync_WhenRedisNull_StoresMessageInMemory()
    {
        // Arrange
        var service = BuildService();
        var source = UniqueSource(nameof(EnqueueAsync_WhenRedisNull_StoresMessageInMemory));
        var message = new DeadLetterMessage
        {
            SourceStream = source,
            OriginalMessageId = "msg-1",
            EventType = "order.created",
            Payload = "{\"id\":1}",
            ErrorMessage = "processing failed"
        };

        // Act
        await service.EnqueueAsync(message);

        // Assert — retrieve by source stream
        var stored = await service.GetBySourceAsync(source);
        stored.Should().ContainSingle(m => m.OriginalMessageId == "msg-1");
    }

    [Fact]
    public async Task EnqueueAsync_SetsFailedAtTimestamp()
    {
        // Arrange
        var service = BuildService();
        var source = UniqueSource(nameof(EnqueueAsync_SetsFailedAtTimestamp));
        var before = DateTime.UtcNow;
        var message = new DeadLetterMessage { SourceStream = source, EventType = "test", Payload = "{}" };

        // Act
        await service.EnqueueAsync(message);

        // Assert
        message.FailedAt.Should().BeOnOrAfter(before);
    }

    // ------------------------------------------------------------------
    // GetMessagesAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetMessagesAsync_WhenRedisNull_ReturnsPreviouslyEnqueuedMessages()
    {
        // Arrange
        var service = BuildService();
        var source = UniqueSource(nameof(GetMessagesAsync_WhenRedisNull_ReturnsPreviouslyEnqueuedMessages));

        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = source, EventType = "a", Payload = "1" });
        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = source, EventType = "b", Payload = "2" });

        // Act
        var messages = await service.GetBySourceAsync(source);

        // Assert
        messages.Should().HaveCount(2);
    }

    // ------------------------------------------------------------------
    // GetCountAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetCountAsync_WhenRedisNull_ReturnsCorrectCount()
    {
        // Arrange
        var service = BuildService();
        var source = UniqueSource(nameof(GetCountAsync_WhenRedisNull_ReturnsCorrectCount));

        // Capture baseline count (static queue may have messages from other tests)
        var beforeCount = await service.GetCountAsync();

        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = source, EventType = "x", Payload = "p" });
        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = source, EventType = "y", Payload = "q" });

        // Act
        var afterCount = await service.GetCountAsync();

        // Assert — exactly 2 more messages added
        (afterCount - beforeCount).Should().Be(2);
    }

    // ------------------------------------------------------------------
    // GetBySourceAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetBySourceAsync_FiltersBySourceStream()
    {
        // Arrange
        var service = BuildService();
        var sourceA = UniqueSource("srcA_" + nameof(GetBySourceAsync_FiltersBySourceStream));
        var sourceB = UniqueSource("srcB_" + nameof(GetBySourceAsync_FiltersBySourceStream));

        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = sourceA, EventType = "e", Payload = "a" });
        await service.EnqueueAsync(new DeadLetterMessage { SourceStream = sourceB, EventType = "e", Payload = "b" });

        // Act
        var fromA = await service.GetBySourceAsync(sourceA);
        var fromB = await service.GetBySourceAsync(sourceB);

        // Assert
        fromA.Should().HaveCount(1).And.OnlyContain(m => m.SourceStream == sourceA);
        fromB.Should().HaveCount(1).And.OnlyContain(m => m.SourceStream == sourceB);
    }

    // ------------------------------------------------------------------
    // RetryAsync (in-memory path returns false — no Redis)
    // ------------------------------------------------------------------

    [Fact]
    public async Task RetryAsync_WhenRedisNull_ReturnsFalse()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.RetryAsync("non-existent-id");

        // Assert — in-memory path always returns false for retry
        result.Should().BeFalse();
    }
}

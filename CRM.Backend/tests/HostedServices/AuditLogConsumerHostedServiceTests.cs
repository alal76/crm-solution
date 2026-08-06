// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.Messaging;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for AuditLogConsumerHostedService (FLAG-005).
///
/// Constructor: (IServiceScopeFactory scopeFactory, IConnectionMultiplexer? redis, ILogger&lt;AuditLogConsumerHostedService&gt; logger)
/// Internal helpers: ParseStreamEntry (static), FlushBatchAsync, EnsureConsumerGroupAsync.
/// </summary>
public class AuditLogConsumerHostedServiceTests : ServiceTestFixtureBase<AuditLogConsumerHostedService>
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceProvider> _mockScopedProvider;
    private readonly Mock<IConnectionMultiplexer> _mockMultiplexer;
    private readonly Mock<IDatabase> _mockDatabase;

    public AuditLogConsumerHostedServiceTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockScopedProvider = new Mock<IServiceProvider>();
        _mockMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();

        // Wire scope factory → scope → provider → ICrmDbContext
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockScopedProvider.Object);
        _mockScopedProvider.Setup(p => p.GetService(typeof(ICrmDbContext))).Returns(MockContext.Object);

        // Wire multiplexer → database
        _mockMultiplexer.Setup(m => m.IsConnected).Returns(true);
        _mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object?>())).Returns(_mockDatabase.Object);
    }

    private AuditLogConsumerHostedService CreateService(
        IConnectionMultiplexer? redis = null,
        int batchSize = 100,
        TimeSpan? flushInterval = null)
        => new AuditLogConsumerHostedService(
            _mockScopeFactory.Object,
            redis ?? _mockMultiplexer.Object,
            MockLogger.Object,
            batchSize,
            flushInterval);

    // ── ParseStreamEntry ─────────────────────────────────────────────────────

    [Fact]
    public void ParseStreamEntry_WithValidFields_ShouldReturnAuditLog()
    {
        // Arrange
        var entry = BuildStreamEntry(
            ("action", "Create"),
            ("userId", "42"),
            ("entityType", "Account"),
            ("entityId", "7"),
            ("oldValues", "{\"Name\":\"Old\"}"),
            ("newValues", "{\"Name\":\"New\"}"),
            ("reason", "System update"),
            ("ipAddress", "10.0.0.1"),
            ("userAgent", "TestAgent/1.0"),
            ("timestamp", "2026-03-09T12:00:00.0000000Z"));

        // Act
        var result = AuditLogConsumerHostedService.ParseStreamEntry(entry);

        // Assert
        result.Should().NotBeNull();
        result!.Action.Should().Be("Create");
        result.UserId.Should().Be(42);
        result.EntityType.Should().Be("Account");
        result.EntityId.Should().Be(7);
        result.OldValues.Should().Be("{\"Name\":\"Old\"}");
        result.NewValues.Should().Be("{\"Name\":\"New\"}");
        result.Details.Should().Be("System update");
        result.IpAddress.Should().Be("10.0.0.1");
        result.UserAgent.Should().Be("TestAgent/1.0");
        result.CreatedAt.Should().Be(new DateTime(2026, 3, 9, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ParseStreamEntry_MissingActionField_ShouldReturnNull()
    {
        // Arrange — no "action" field
        var entry = BuildStreamEntry(
            ("userId", "1"),
            ("entityType", "Contact"));

        // Act
        var result = AuditLogConsumerHostedService.ParseStreamEntry(entry);

        // Assert
        result.Should().BeNull("an entry without 'action' is malformed and must be discarded");
    }

    [Fact]
    public void ParseStreamEntry_EmptyActionField_ShouldReturnNull()
    {
        var entry = BuildStreamEntry(("action", ""), ("userId", "1"));
        AuditLogConsumerHostedService.ParseStreamEntry(entry).Should().BeNull();
    }

    [Fact]
    public void ParseStreamEntry_OptionalFieldsAbsent_ShouldReturnAuditLogWithNulls()
    {
        // Arrange — only the mandatory field
        var entry = BuildStreamEntry(("action", "Delete"));

        // Act
        var result = AuditLogConsumerHostedService.ParseStreamEntry(entry);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().BeNull();
        result.EntityType.Should().BeNull();
        result.EntityId.Should().BeNull();
        result.OldValues.Should().BeNull();
        result.NewValues.Should().BeNull();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithNullRedis_ShouldCreateServiceSuccessfully()
    {
        // Act & Assert — should not throw
        var sut = new AuditLogConsumerHostedService(
            _mockScopeFactory.Object,
            redis: null,
            MockLogger.Object);

        sut.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithConnectedRedis_ShouldCreateServiceSuccessfully()
    {
        var sut = CreateService(_mockMultiplexer.Object);
        sut.Should().NotBeNull();
    }

    // ── PublishAuditEventAsync (via OptionalAuditLoggingService) ──────────────

    [Fact]
    public async Task PublishAuditEventAsync_ShouldEnqueueToRedisStream_WhenFlagEnabled()
    {
        // Arrange
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockFeatureManager
            .Setup(fm => fm.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockStreamService = new Mock<IRedisStreamService>();
        mockStreamService
            .Setup(s => s.PublishAsync(
                OptionalAuditLoggingService.StreamName,
                "AuditEvent",
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("1234567890123-0"); // Non-empty = queued successfully

        var service = new OptionalAuditLoggingService(
            mockFeatureManager.Object,
            MockContext.Object,
            new Mock<ILogger<OptionalAuditLoggingService>>().Object,
            mockStreamService.Object);

        var auditEvent = new AuditEvent
        {
            UserId = 5,
            Action = "Create",
            EntityType = "Lead",
            EntityId = 123,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await service.PublishAuditEventAsync(auditEvent);

        // Assert
        mockStreamService.Verify(
            s => s.PublishAsync(
                OptionalAuditLoggingService.StreamName,
                "AuditEvent",
                It.Is<Dictionary<string, string>>(d =>
                    d["action"] == "Create" &&
                    d["userId"] == "5" &&
                    d["entityType"] == "Lead" &&
                    d["entityId"] == "123"),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "PublishAsync must be called exactly once with the correct audit event fields");
    }

    [Fact]
    public async Task PublishAuditEventAsync_ShouldFallBackToDirectDbWrite_WhenRedisUnavailable()
    {
        // Arrange
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        mockFeatureManager.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        mockFeatureManager.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var mockStreamService = new Mock<IRedisStreamService>();
        mockStreamService
            .Setup(s => s.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty); // Empty string = Redis unavailable

        var auditLogs = new List<AuditLog>();
        var mockAuditSet = SetupDbSet(auditLogs);
        mockAuditSet.Setup(s => s.Add(It.IsAny<AuditLog>())).Callback<AuditLog>(auditLogs.Add);
        MockContext.Setup(c => c.AuditLogs).Returns(mockAuditSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new OptionalAuditLoggingService(
            mockFeatureManager.Object,
            MockContext.Object,
            new Mock<ILogger<OptionalAuditLoggingService>>().Object,
            mockStreamService.Object);

        var auditEvent = new AuditEvent { Action = "Update", EntityType = "Opportunity", EntityId = 99 };

        // Act
        await service.PublishAuditEventAsync(auditEvent);

        // Assert — direct DB write fallback should have been triggered
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once,
            "SaveChangesAsync must be called as a fallback when Redis returns empty message ID");
    }

    // ── AuditLogConsumer_ShouldBatchWriteToDb_WhenEventsQueued ───────────────

    [Fact]
    public async Task AuditLogConsumer_ShouldBatchWriteToDb_WhenEventsQueued()
    {
        // Arrange — use mock ICrmDbContext so we can verify AddRange + SaveChangesAsync calls
        var capturedEntries = new List<AuditLog>();
        var mockAuditSet = SetupDbSet(capturedEntries);
        mockAuditSet
            .Setup(s => s.AddRange(It.IsAny<IEnumerable<AuditLog>>()))
            .Callback<IEnumerable<AuditLog>>(items => capturedEntries.AddRange(items));
        MockContext.Setup(c => c.AuditLogs).Returns(mockAuditSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockScopedProvider.Setup(p => p.GetService(typeof(ICrmDbContext))).Returns(MockContext.Object);

        // Two entries; batchSize=2 forces flush after accumulating both (no timer wait needed)
        var entry1 = BuildStreamEntry("1680000000000-0",
            ("action", "Create"), ("entityType", "Account"), ("entityId", "1"));
        var entry2 = BuildStreamEntry("1680000000000-1",
            ("action", "Delete"), ("entityType", "Contact"), ("entityId", "2"), ("userId", "10"));

        _mockDatabase
            .SetupSequence(d => d.StreamReadGroupAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                AuditLogConsumerHostedService.ConsumerName,
                ">",
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(new[] { entry1, entry2 })
            .ReturnsAsync(Array.Empty<StreamEntry>());

        _mockDatabase
            .Setup(d => d.StreamAcknowledgeAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1L);

        _mockDatabase
            .Setup(d => d.StreamCreateConsumerGroupAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue?>(), It.IsAny<bool>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // batchSize=2 so flush is triggered as soon as 2 entries arrive
        var sut = CreateService(batchSize: 2, flushInterval: TimeSpan.FromSeconds(30));

        // Act — 800 ms is enough for the batch-full flush to complete
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await sut.StartAsync(cts.Token); await Task.Delay(800, cts.Token); } catch (OperationCanceledException) { }
        await sut.StopAsync(CancellationToken.None);

        // Assert — SaveChangesAsync must have been called (batch written to DB)
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "SaveChangesAsync must be called to persist the audit batch to the DB");

        // And the entries passed to AddRange must represent both queued events
        capturedEntries.Should().HaveCountGreaterOrEqualTo(2,
            "both audit events must be persisted in a single batch write");
        capturedEntries.Should().Contain(e => e.Action == "Create" && e.EntityType == "Account");
        capturedEntries.Should().Contain(e => e.Action == "Delete" && e.EntityType == "Contact");
    }

    [Fact]
    public async Task AuditLogConsumer_ShouldAcknowledge_WhenWriteSucceeds()
    {
        // Arrange
        var entry = BuildStreamEntry("1680000000001-0",
            ("action", "Update"), ("entityType", "Lead"), ("entityId", "33"));

        _mockDatabase
            .SetupSequence(d => d.StreamReadGroupAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                AuditLogConsumerHostedService.ConsumerName,
                ">",
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(new[] { entry })
            .ReturnsAsync(Array.Empty<StreamEntry>());

        _mockDatabase
            .Setup(d => d.StreamAcknowledgeAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1L);

        _mockDatabase
            .Setup(d => d.StreamCreateConsumerGroupAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue?>(), It.IsAny<bool>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        MockContext.Setup(c => c.AuditLogs).Returns(SetupDbSet(new List<AuditLog>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockScopedProvider.Setup(p => p.GetService(typeof(ICrmDbContext))).Returns(MockContext.Object);

        // batchSize=1 triggers flush immediately after the first entry is buffered
        var sut = CreateService(batchSize: 1, flushInterval: TimeSpan.FromSeconds(30));

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await sut.StartAsync(cts.Token); await Task.Delay(800, cts.Token); } catch (OperationCanceledException) { }
        await sut.StopAsync(CancellationToken.None);

        // Assert — message acknowledged after successful write
        _mockDatabase.Verify(
            d => d.StreamAcknowledgeAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()),
            Times.AtLeastOnce,
            "XACK must be called after a successful DB write");
    }

    [Fact]
    public async Task AuditLogConsumer_ShouldNotAcknowledge_WhenDbWriteFails()
    {
        // Arrange
        var entry = BuildStreamEntry("1680000000002-0",
            ("action", "Create"), ("entityType", "Quote"), ("entityId", "5"));

        _mockDatabase
            .SetupSequence(d => d.StreamReadGroupAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                AuditLogConsumerHostedService.ConsumerName,
                ">",
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(new[] { entry })
            .ReturnsAsync(Array.Empty<StreamEntry>());

        _mockDatabase
            .Setup(d => d.StreamCreateConsumerGroupAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<RedisValue?>(), It.IsAny<bool>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // DB write throws
        MockContext.Setup(c => c.AuditLogs).Returns(SetupDbSet(new List<AuditLog>()).Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated DB failure"));
        _mockScopedProvider.Setup(p => p.GetService(typeof(ICrmDbContext))).Returns(MockContext.Object);

        // batchSize=1 triggers flush immediately so the DB failure occurs before StopAsync
        var sut = CreateService(batchSize: 1, flushInterval: TimeSpan.FromSeconds(30));

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await sut.StartAsync(cts.Token); await Task.Delay(800, cts.Token); } catch (OperationCanceledException) { }
        await sut.StopAsync(CancellationToken.None);

        // Assert — XACK should NOT be called when the DB write fails
        _mockDatabase.Verify(
            d => d.StreamAcknowledgeAsync(
                AuditLogConsumerHostedService.StreamName,
                AuditLogConsumerHostedService.GroupName,
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()),
            Times.Never,
            "XACK must NOT be called when the DB write fails so the event is retried");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="StreamEntry"/> with the given entry ID and name/value pairs.
    /// </summary>
    private static StreamEntry BuildStreamEntry(string id, params (string Name, string Value)[] fields)
    {
        var nameValueEntries = fields
            .Select(f => new NameValueEntry(f.Name, f.Value))
            .ToArray();

        return new StreamEntry(id, nameValueEntries);
    }

    /// <summary>Overload that uses a default ID for single-entry tests.</summary>
    private static StreamEntry BuildStreamEntry(params (string Name, string Value)[] fields)
        => BuildStreamEntry("1680000000000-0", fields);
}

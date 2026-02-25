// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.EventSourcing;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EventStore.
/// Verifies event append (version assignment) and event retrieval.
/// </summary>
public class EventStoreTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<EventStore>> _mockLogger;

    public EventStoreTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EventStore>>();
    }

    private EventStore BuildStore(List<AuditLog> existingLogs)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(existingLogs);
        _mockContext.Setup(c => c.AuditLogs).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new EventStore(_mockContext.Object, _mockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – first event for an aggregate gets version 1
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AppendEventAsync_ShouldReturnVersionOne_WhenNoExistingEvents()
    {
        // Arrange — empty audit log (no prior events for this aggregate)
        var store = BuildStore(new List<AuditLog>());

        var ev = new StoredEvent
        {
            AggregateType = "Account",
            AggregateId = "42",
            EventType = "AccountCreated",
            EventData = @"{""Name"":""Acme Corp""}"
        };

        // Act
        var version = await store.AppendEventAsync(ev);

        // Assert
        version.Should().Be(1);
        ev.Version.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – GetEventsAsync returns mapped StoredEvents for an aggregate
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEvents_WhenAuditLogsExistForAggregate()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                EntityType = "Contact",
                EntityId = 10,
                Action = "ContactCreated",
                NewValues = @"{""Email"":""a@example.com""}",
                Details = "v1",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = 2,
                EntityType = "Contact",
                EntityId = 10,
                Action = "ContactUpdated",
                NewValues = @"{""Email"":""b@example.com""}",
                Details = "v2",
                CreatedAt = DateTime.UtcNow
            },
            // Different entity — should NOT appear
            new()
            {
                Id = 3,
                EntityType = "Contact",
                EntityId = 99,
                Action = "ContactCreated",
                NewValues = @"{}",
                Details = "v1",
                CreatedAt = DateTime.UtcNow
            }
        };

        var store = BuildStore(logs);

        // Act
        var events = await store.GetEventsAsync("Contact", "10");

        // Assert
        var list = new List<StoredEvent>(events);
        list.Should().HaveCount(2);
        list.Should().Contain(e => e.EventType == "ContactCreated");
        list.Should().Contain(e => e.EventType == "ContactUpdated");
    }
}

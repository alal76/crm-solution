// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Ports.Output.Events;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

// ────────────────────────────────────────────────────────────────────────────
// Test helpers
// ────────────────────────────────────────────────────────────────────────────

public record TestDomainEvent : DomainEventBase
{
    public string Payload { get; init; } = string.Empty;
}

public record AnotherDomainEvent : DomainEventBase
{
    public int EntityId { get; init; }
}

/// <summary>
/// Simple entity stub that tracks / clears domain events for testing
/// <see cref="DomainEventPublisher.PublishAndClearAsync"/>.
/// </summary>
internal sealed class FakeEntity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _events.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _events.Remove(domainEvent);
    public void ClearDomainEvents() => _events.Clear();
}

// ────────────────────────────────────────────────────────────────────────────
// Tests
// ────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Unit tests for <see cref="DomainEventPublisher"/>.
/// Uses a real <see cref="ServiceProvider"/> to avoid brittle mock-of-mock
/// setups for <c>IServiceProvider.GetServices&lt;T&gt;()</c>.
/// </summary>
public class DomainEventPublisherTests
{
    private static DomainEventPublisher BuildPublisher(IServiceProvider sp)
        => new(sp);

    // ------------------------------------------------------------------
    // PublishAsync<TEvent>
    // ------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_SingleEvent_DispatchesToAllRegisteredHandlers()
    {
        // Arrange
        var handler1 = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var handler2 = new Mock<IDomainEventHandler<TestDomainEvent>>();
        handler1.Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        handler2.Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler1.Object);
        services.AddSingleton(handler2.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);
        var domainEvent = new TestDomainEvent { Payload = "hello" };

        // Act
        await publisher.PublishAsync(domainEvent);

        // Assert
        handler1.Verify(h => h.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_NoHandlersRegistered_CompletesWithoutException()
    {
        // Arrange: empty container — no handlers
        var sp = new ServiceCollection().BuildServiceProvider();
        var publisher = BuildPublisher(sp);
        var domainEvent = new TestDomainEvent { Payload = "no handlers" };

        // Act & Assert: must not throw
        var act = async () => await publisher.PublishAsync(domainEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_PassesCancellationTokenToHandler()
    {
        // Arrange
        CancellationToken capturedToken = default;
        var handler = new Mock<IDomainEventHandler<TestDomainEvent>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
               .Callback<TestDomainEvent, CancellationToken>((_, ct) => capturedToken = ct)
               .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);
        using var cts = new CancellationTokenSource();

        // Act
        await publisher.PublishAsync(new TestDomainEvent(), cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    // ------------------------------------------------------------------
    // PublishAsync(IEnumerable<IDomainEvent>) — collection overload
    // ------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_Collection_DispatchesEachEventToMatchingHandler()
    {
        // Arrange
        var testHandler = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var anotherHandler = new Mock<IDomainEventHandler<AnotherDomainEvent>>();
        testHandler.Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
        anotherHandler.Setup(h => h.HandleAsync(It.IsAny<AnotherDomainEvent>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(testHandler.Object);
        services.AddSingleton(anotherHandler.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent { Payload = "one" },
            new AnotherDomainEvent { EntityId = 42 },
        };

        // Act
        await publisher.PublishAsync(events);

        // Assert
        testHandler.Verify(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        anotherHandler.Verify(h => h.HandleAsync(It.IsAny<AnotherDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_EmptyCollection_CompletesWithoutCallingAnyHandler()
    {
        // Arrange
        var handler = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);

        // Act
        await publisher.PublishAsync(new List<IDomainEvent>());

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ------------------------------------------------------------------
    // PublishAndClearAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task PublishAndClearAsync_ClearsEntityEventsAfterDispatch()
    {
        // Arrange
        var handler = new Mock<IDomainEventHandler<TestDomainEvent>>();
        handler.Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(handler.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);

        var entity = new FakeEntity();
        entity.AddDomainEvent(new TestDomainEvent { Payload = "clear me" });
        entity.DomainEvents.Should().HaveCount(1);

        // Act
        await publisher.PublishAndClearAsync(entity);

        // Assert — entity events cleared
        entity.DomainEvents.Should().BeEmpty();
        handler.Verify(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAndClearAsync_EmptyEntity_DoesNotCallAnyHandler()
    {
        // Arrange
        var handler = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var services = new ServiceCollection();
        services.AddSingleton(handler.Object);
        var sp = services.BuildServiceProvider();

        var publisher = BuildPublisher(sp);
        var entity = new FakeEntity(); // no events added

        // Act
        await publisher.PublishAndClearAsync(entity);

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        entity.DomainEvents.Should().BeEmpty();
    }
}

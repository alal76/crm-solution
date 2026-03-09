// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;
using CRM.Infrastructure.Data.Interceptors;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CRM.Tests.Unit.Infrastructure;

#region Test helpers

/// <summary>Simple concrete domain event for testing.</summary>
public sealed record TestDomainEvent(string Message) : DomainEventBase;

/// <summary>A second event type to verify multi-type dispatch.</summary>
public sealed record AnotherTestDomainEvent(int Code) : DomainEventBase;

/// <summary>Minimal entity implementing <see cref="IHasDomainEvents"/>.</summary>
public sealed class TestEntity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

#endregion

/// <summary>
/// INFRA-001: Unit tests for <see cref="DomainEventPublisher"/>.
/// Verifies generic publish, reflection-based dispatch, and entity event clearing.
/// </summary>
public sealed class DomainEventPublisherTests
{
    private readonly Mock<IDomainEventHandler<TestDomainEvent>> _handlerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly DomainEventPublisher _sut;

    public DomainEventPublisherTests()
    {
        _handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _serviceProviderMock = new Mock<IServiceProvider>();

        // Set up GetServices<IDomainEventHandler<TestDomainEvent>>() —
        // the extension calls sp.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>))
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
            .Returns(new[] { _handlerMock.Object });

        _sut = new DomainEventPublisher(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeRegisteredHandler()
    {
        var evt = new TestDomainEvent("hello");

        await _sut.PublishAsync(evt);

        _handlerMock.Verify(
            h => h.HandleAsync(evt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeMultipleHandlers()
    {
        var handler2 = new Mock<IDomainEventHandler<TestDomainEvent>>();
        handler2
            .Setup(h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
          .Returns(new[] { _handlerMock.Object, handler2.Object });

        var publisher = new DomainEventPublisher(sp.Object);
        var evt = new TestDomainEvent("multi");

        await publisher.PublishAsync(evt);

        _handlerMock.Verify(h => h.HandleAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldDoNothing_WhenNoHandlersRegistered()
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IEnumerable<IDomainEventHandler<TestDomainEvent>>)))
          .Returns(Array.Empty<IDomainEventHandler<TestDomainEvent>>());

        var publisher = new DomainEventPublisher(sp.Object);

        var act = () => publisher.PublishAsync(new TestDomainEvent("no-handler"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_IEnumerable_ShouldDispatchAllEvents()
    {
        // The IEnumerable<IDomainEvent> overload uses reflection-based DispatchAsync,
        // which calls sp.GetServices(handlerType) — same underlying call.
        IEnumerable<IDomainEvent> events = new IDomainEvent[]
        {
            new TestDomainEvent("one"),
            new TestDomainEvent("two"),
            new TestDomainEvent("three"),
        };

        await _sut.PublishAsync(events);

        _handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task PublishAndClearAsync_ShouldPublishAllEventsAndClearEntity()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent("a"));
        entity.AddDomainEvent(new TestDomainEvent("b"));

        entity.DomainEvents.Should().HaveCount(2);

        await _sut.PublishAndClearAsync(entity);

        entity.DomainEvents.Should().BeEmpty("events should be cleared after publish");
        _handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task PublishAndClearAsync_ShouldDoNothing_WhenEntityHasNoEvents()
    {
        var entity = new TestEntity();
        entity.DomainEvents.Should().BeEmpty();

        var act = () => _sut.PublishAndClearAsync(entity);

        await act.Should().NotThrowAsync();
        _handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

/// <summary>
/// INFRA-001: Unit tests for <see cref="DomainEventDispatchInterceptor"/>.
/// Verifies the EF Core SaveChanges interceptor dispatches domain events
/// from tracked entities after a successful save.
/// </summary>
public sealed class DomainEventDispatchInterceptorTests
{
    private readonly Mock<IDomainEventPublisher> _publisherMock;
    private readonly DomainEventDispatchInterceptor _sut;

    public DomainEventDispatchInterceptorTests()
    {
        _publisherMock = new Mock<IDomainEventPublisher>();
        _publisherMock
            .Setup(p => p.PublishAndClearAsync(It.IsAny<IHasDomainEvents>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new DomainEventDispatchInterceptor(_publisherMock.Object);
    }

    [Fact]
    public async Task SavedChangesAsync_ShouldReturnResult_WhenContextIsNull()
    {
        var eventData = new SaveChangesCompletedEventData(
            eventDefinition: null!,
            messageGenerator: (_, _) => string.Empty,
            context: null!,
            entitiesSavedCount: 0);

        var result = await _sut.SavedChangesAsync(eventData, 42);

        result.Should().Be(42);
        _publisherMock.Verify(
            p => p.PublishAndClearAsync(It.IsAny<IHasDomainEvents>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void SavedChanges_Sync_ShouldPassThrough()
    {
        var eventData = new SaveChangesCompletedEventData(
            eventDefinition: null!,
            messageGenerator: (_, _) => string.Empty,
            context: null!,
            entitiesSavedCount: 0);

        var result = _sut.SavedChanges(eventData, 7);

        result.Should().Be(7);
    }
}

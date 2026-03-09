// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Reflection;
using CRM.Core.Ports.Output.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Services;

/// <summary>
/// AP-059: In-process implementation of <see cref="IDomainEventPublisher"/>.
/// Resolves <see cref="IDomainEventHandler{TEvent}"/> registrations from the DI
/// container and dispatches domain events to them.
/// </summary>
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task PublishAndClearAsync(IHasDomainEvents entity, CancellationToken cancellationToken = default)
    {
        var events = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();
        await PublishAsync(events, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dispatches a single domain event to its registered handlers using reflection
    /// to resolve the correct generic handler type at runtime.
    /// </summary>
    private async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

        foreach (var handler in _serviceProvider.GetServices(handlerType))
        {
            if (handler is null) continue;
            await ((Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!).ConfigureAwait(false);
        }
    }
}

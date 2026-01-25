using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Ports.Output.Events;

/// <summary>
/// Marker interface for domain events.
/// All domain events should implement this interface.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the type name of the event.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Gets the version of the event schema.
    /// </summary>
    int Version { get; }
}

/// <summary>
/// Base implementation of a domain event.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public virtual string EventType => GetType().Name;
    public virtual int Version => 1;
}

/// <summary>
/// Event that is associated with a specific aggregate/entity.
/// </summary>
public interface IEntityDomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the type of the entity.
    /// </summary>
    string EntityType { get; }

    /// <summary>
    /// Gets the ID of the entity.
    /// </summary>
    int EntityId { get; }
}

/// <summary>
/// Base implementation of an entity domain event.
/// </summary>
public abstract record EntityDomainEventBase : DomainEventBase, IEntityDomainEvent
{
    public abstract string EntityType { get; init; }
    public int EntityId { get; init; }
}

/// <summary>
/// Interface for entities that can raise domain events.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events that have been raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Removes a domain event from the entity.
    /// </summary>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from the entity.
    /// </summary>
    void ClearDomainEvents();
}

/// <summary>
/// Handler interface for domain events.
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Port for publishing domain events following hexagonal architecture.
/// This is the primary interface for event publishing in the domain layer.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes a single domain event.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;

    /// <summary>
    /// Publishes multiple domain events.
    /// </summary>
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes all domain events from an entity and clears them.
    /// </summary>
    Task PublishAndClearAsync(IHasDomainEvents entity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Port for event subscription and handling.
/// </summary>
public interface IDomainEventSubscriber
{
    /// <summary>
    /// Subscribes a handler to a specific event type.
    /// </summary>
    void Subscribe<TEvent, THandler>() 
        where TEvent : IDomainEvent 
        where THandler : IDomainEventHandler<TEvent>;

    /// <summary>
    /// Subscribes a handler instance to a specific event type.
    /// </summary>
    void Subscribe<TEvent>(IDomainEventHandler<TEvent> handler) where TEvent : IDomainEvent;

    /// <summary>
    /// Unsubscribes a handler from a specific event type.
    /// </summary>
    void Unsubscribe<TEvent, THandler>() 
        where TEvent : IDomainEvent 
        where THandler : IDomainEventHandler<TEvent>;

    /// <summary>
    /// Gets all handlers for a specific event type.
    /// </summary>
    IEnumerable<IDomainEventHandler<TEvent>> GetHandlers<TEvent>() where TEvent : IDomainEvent;
}

/// <summary>
/// Extended event bus interface that combines publishing and subscribing.
/// This is the full event bus port for hexagonal architecture.
/// </summary>
public interface IEventBus : IDomainEventPublisher, IDomainEventSubscriber, IAsyncDisposable
{
    /// <summary>
    /// Gets whether the event bus is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the event bus.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the event bus.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check on the event bus.
    /// </summary>
    Task<EventBusHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for publishing events.
/// </summary>
public record EventPublishOptions
{
    /// <summary>
    /// Whether to wait for all handlers to complete.
    /// </summary>
    public bool WaitForHandlers { get; init; } = true;

    /// <summary>
    /// Timeout for handler execution.
    /// </summary>
    public TimeSpan? HandlerTimeout { get; init; }

    /// <summary>
    /// Whether to continue if a handler fails.
    /// </summary>
    public bool ContinueOnError { get; init; } = true;

    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of an event bus health check.
/// </summary>
public record EventBusHealthResult(
    bool IsHealthy,
    string Status,
    int SubscribedHandlerCount,
    int PendingEventCount,
    string? ErrorMessage = null
);

#region Common Domain Events

/// <summary>
/// Event raised when an entity is created.
/// </summary>
public record EntityCreatedEvent : EntityDomainEventBase
{
    public override string EntityType { get; init; } = string.Empty;
    public object? EntityData { get; init; }
    public int? CreatedByUserId { get; init; }
}

/// <summary>
/// Event raised when an entity is updated.
/// </summary>
public record EntityUpdatedEvent : EntityDomainEventBase
{
    public override string EntityType { get; init; } = string.Empty;
    public IDictionary<string, (object? OldValue, object? NewValue)>? Changes { get; init; }
    public int? UpdatedByUserId { get; init; }
}

/// <summary>
/// Event raised when an entity is deleted.
/// </summary>
public record EntityDeletedEvent : EntityDomainEventBase
{
    public override string EntityType { get; init; } = string.Empty;
    public int? DeletedByUserId { get; init; }
    public bool IsSoftDelete { get; init; }
}

/// <summary>
/// Event raised when an entity's status changes.
/// </summary>
public record EntityStatusChangedEvent : EntityDomainEventBase
{
    public override string EntityType { get; init; } = string.Empty;
    public string? OldStatus { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public int? ChangedByUserId { get; init; }
}

#endregion

#region Integration Events

/// <summary>
/// Marker interface for integration events (cross-service communication).
/// </summary>
public interface IIntegrationEvent : IDomainEvent
{
    /// <summary>
    /// Gets the source service that raised this event.
    /// </summary>
    string SourceService { get; }
}

/// <summary>
/// Base implementation of an integration event.
/// </summary>
public abstract record IntegrationEventBase : DomainEventBase, IIntegrationEvent
{
    public virtual string SourceService => "CRM";
}

/// <summary>
/// Port for publishing integration events to external systems.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publishes an integration event to the event bus/message broker.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, IntegrationEventOptions? options = null, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;

    /// <summary>
    /// Publishes multiple integration events.
    /// </summary>
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> integrationEvents, IntegrationEventOptions? options = null, CancellationToken cancellationToken = default) where TEvent : IIntegrationEvent;
}

/// <summary>
/// Options for publishing integration events.
/// </summary>
public record IntegrationEventOptions
{
    /// <summary>
    /// The exchange/topic to publish to.
    /// </summary>
    public string? RoutingKey { get; init; }

    /// <summary>
    /// Whether to persist the event.
    /// </summary>
    public bool Persistent { get; init; } = true;

    /// <summary>
    /// Message time-to-live.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Delay before the event is available.
    /// </summary>
    public TimeSpan? Delay { get; init; }

    /// <summary>
    /// Custom headers.
    /// </summary>
    public IDictionary<string, object>? Headers { get; init; }
}

#endregion

#region Event Store (Event Sourcing Support)

/// <summary>
/// Stored event representation.
/// </summary>
public record StoredEvent(
    Guid EventId,
    string EventType,
    string AggregateType,
    string AggregateId,
    int Version,
    string Data,
    string? Metadata,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Port for event store operations (Event Sourcing).
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends events to an aggregate's event stream.
    /// </summary>
    Task AppendEventsAsync(string aggregateType, string aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion = -1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events for an aggregate.
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsAsync(string aggregateType, string aggregateId, int fromVersion = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events of a specific type.
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(string eventType, DateTimeOffset? fromDate = null, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current version of an aggregate.
    /// </summary>
    Task<int> GetCurrentVersionAsync(string aggregateType, string aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a snapshot for an aggregate.
    /// </summary>
    Task SaveSnapshotAsync(string aggregateType, string aggregateId, int version, object state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest snapshot for an aggregate.
    /// </summary>
    Task<(int Version, object? State)?> GetSnapshotAsync(string aggregateType, string aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to all events from the event store.
    /// </summary>
    Task<IAsyncDisposable> SubscribeToAllAsync(Func<StoredEvent, Task> handler, string? fromPosition = null, CancellationToken cancellationToken = default);
}

#endregion

#region Outbox Pattern Support

/// <summary>
/// Outbox message for reliable event publishing.
/// </summary>
public record OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string? Destination { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; init; }
    public int RetryCount { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Port for the outbox pattern.
/// </summary>
public interface IOutboxPort
{
    /// <summary>
    /// Adds an event to the outbox.
    /// </summary>
    Task AddAsync(IDomainEvent domainEvent, string? destination = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed outbox messages.
    /// </summary>
    Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed.
    /// </summary>
    Task MarkAsFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old processed messages.
    /// </summary>
    Task CleanupAsync(TimeSpan retentionPeriod, CancellationToken cancellationToken = default);
}

#endregion

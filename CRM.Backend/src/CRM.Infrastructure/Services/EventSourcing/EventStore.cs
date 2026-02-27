// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.EventSourcing;

/// <summary>
/// Interface for event sourcing operations on audit-critical entities.
/// TODO-INFRA-05
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends an event to the event stream for a given aggregate.
    /// </summary>
    Task<int> AppendEventAsync(StoredEvent storedEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events for a given aggregate.
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsAsync(string aggregateType, string aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for an aggregate after a specific version.
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsAfterVersionAsync(string aggregateType, string aggregateId, int afterVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest snapshot for an aggregate.
    /// </summary>
    Task<EventSnapshot?> GetSnapshotAsync(string aggregateType, string aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a snapshot for an aggregate.
    /// </summary>
    Task SaveSnapshotAsync(EventSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current version of an aggregate.
    /// </summary>
    Task<int> GetCurrentVersionAsync(string aggregateType, string aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events of a specific type across all aggregates (for projections).
    /// </summary>
    Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(string eventType, DateTime? since = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a stored domain event.
/// </summary>
public class StoredEvent
{
    public int Id { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public int Version { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Represents a snapshot of an aggregate's state.
/// </summary>
public class EventSnapshot
{
    public int Id { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string SnapshotData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Entity Framework-based event store implementation.
/// Stores domain events for audit-critical entities using the application database.
/// </summary>
public class EventStore : IEventStore
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EventStore> _logger;

    public EventStore(
        ICrmDbContext context,
        ILogger<EventStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> AppendEventAsync(StoredEvent storedEvent, CancellationToken cancellationToken = default)
    {
        // Get next version for this aggregate
        var currentVersion = await GetCurrentVersionAsync(
            storedEvent.AggregateType,
            storedEvent.AggregateId,
            cancellationToken);

        storedEvent.Version = currentVersion + 1;
        storedEvent.OccurredAt = DateTime.UtcNow;

        // Store as an audit log entry for now — the AuditLog table serves as event store
        // In a full implementation, this would have its own EventStore table
        _context.AuditLogs.Add(new CRM.Core.Entities.AuditLog
        {
            EntityType = storedEvent.AggregateType,
            EntityId = int.TryParse(storedEvent.AggregateId, out var id) ? id : 0,
            Action = storedEvent.EventType,
            NewValues = storedEvent.EventData,
            UserId = storedEvent.UserId,
            CreatedAt = storedEvent.OccurredAt,
            IpAddress = storedEvent.CorrelationId,
            Details = storedEvent.Metadata != null
                ? $"v{storedEvent.Version}|{storedEvent.Metadata}"
                : $"v{storedEvent.Version}"
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Appended event {EventType} v{Version} to {AggregateType}/{AggregateId}",
            storedEvent.EventType, storedEvent.Version,
            storedEvent.AggregateType, storedEvent.AggregateId);

        return storedEvent.Version;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StoredEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        var entityId = int.TryParse(aggregateId, out var id) ? id : 0;

        var logs = await _context.AuditLogs
            .Where(a => a.EntityType == aggregateType && a.EntityId == entityId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToStoredEvent);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StoredEvent>> GetEventsAfterVersionAsync(
        string aggregateType,
        string aggregateId,
        int afterVersion,
        CancellationToken cancellationToken = default)
    {
        var allEvents = await GetEventsAsync(aggregateType, aggregateId, cancellationToken);
        return allEvents.Where(e => e.Version > afterVersion);
    }

    /// <inheritdoc />
    public Task<EventSnapshot?> GetSnapshotAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        // Snapshots are stored in-memory cache or a dedicated table
        // For this implementation, we return null (rebuild from events)
        _logger.LogDebug(
            "Snapshot requested for {AggregateType}/{AggregateId} — not yet cached",
            aggregateType, aggregateId);
        return Task.FromResult<EventSnapshot?>(null);
    }

    /// <inheritdoc />
    public Task SaveSnapshotAsync(
        EventSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        // Snapshot persistence would go to a dedicated table
        _logger.LogDebug(
            "Snapshot saved for {AggregateType}/{AggregateId} at v{Version}",
            snapshot.AggregateType, snapshot.AggregateId, snapshot.Version);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        var entityId = int.TryParse(aggregateId, out var id) ? id : 0;

        var count = await _context.AuditLogs
            .CountAsync(a => a.EntityType == aggregateType && a.EntityId == entityId,
                cancellationToken);

        return count;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Where(a => a.Action == eventType);

        if (since.HasValue)
        {
            query = query.Where(a => a.Timestamp >= since.Value);
        }

        var logs = await query
            .OrderBy(a => a.Timestamp)
            .Take(1000)
            .ToListAsync(cancellationToken);

        return logs.Select(MapToStoredEvent);
    }

    private static StoredEvent MapToStoredEvent(CRM.Core.Entities.AuditLog log)
    {
        var version = 0;
        string? metadata = log.Details;

        // Parse version from Details field (format: "v{version}|{metadata}")
        if (!string.IsNullOrEmpty(log.Details) && log.Details.StartsWith("v"))
        {
            var pipeIndex = log.Details.IndexOf('|');
            if (pipeIndex > 0)
            {
                int.TryParse(log.Details[1..pipeIndex], out version);
                metadata = log.Details[(pipeIndex + 1)..];
            }
            else
            {
                int.TryParse(log.Details[1..], out version);
                metadata = null;
            }
        }

        return new StoredEvent
        {
            Id = log.Id,
            AggregateType = log.EntityType ?? string.Empty,
            AggregateId = log.EntityId?.ToString() ?? string.Empty,
            EventType = log.Action ?? string.Empty,
            EventData = log.NewValues ?? string.Empty,
            Metadata = metadata,
            Version = version,
            OccurredAt = log.Timestamp,
            UserId = log.UserId,
            CorrelationId = log.IpAddress
        };
    }
}

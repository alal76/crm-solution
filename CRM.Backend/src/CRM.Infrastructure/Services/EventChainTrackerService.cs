// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for tracking webhook event chains and detecting cycles.
/// Implements TODO-INT001-48: Event chain tracking with cycle detection.
/// </summary>
public class EventChainTrackerService : IEventChainTracker
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EventChainTrackerService> _logger;

    /// <summary>
    /// Maximum allowed chain depth before rejecting events.
    /// Default is 10 to prevent infinite loops.
    /// </summary>
    public int MaxChainDepth { get; } = 10;

    public EventChainTrackerService(ICrmDbContext context, ILogger<EventChainTrackerService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EventChainContext> StartChainAsync(int eventId, string eventType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting new event chain for event {EventId} of type {EventType}", eventId, eventType);

        var context = new EventChainContext
        {
            RootEventId = eventId,
            CurrentEventId = eventId,
            Depth = 0,
            StartedAt = DateTime.UtcNow,
            ChainId = $"chain_{eventId}_{Guid.NewGuid():N}"
        };

        return await Task.FromResult(context);
    }

    /// <inheritdoc />
    public async Task<ChainAddResult> AddToChainAsync(
        int parentEventId,
        int childEventId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        // Get current depth
        var currentDepth = await GetChainDepthAsync(parentEventId, cancellationToken);
        var newDepth = currentDepth + 1;

        // Check max depth
        if (newDepth > MaxChainDepth)
        {
            _logger.LogWarning(
                "Event chain max depth exceeded. Parent event {ParentEventId}, attempted child {ChildEventId}, depth {NewDepth}",
                parentEventId, childEventId, newDepth);
            return ChainAddResult.MaxDepthFailed(MaxChainDepth);
        }

        // Check for cycles - look for the same event type in the chain
        var cycleResult = await DetectCycleAsync(parentEventId, eventType, string.Empty, 0, cancellationToken);
        if (cycleResult.CycleDetected)
        {
            _logger.LogWarning(
                "Cycle detected in event chain. Parent event {ParentEventId}, child event type {EventType}",
                parentEventId, eventType);
            return ChainAddResult.CycleFailed(cycleResult.CycleDescription ?? "Cycle detected");
        }

        _logger.LogDebug(
            "Added event {ChildEventId} to chain under parent {ParentEventId} at depth {NewDepth}",
            childEventId, parentEventId, newDepth);

        return ChainAddResult.Succeeded(newDepth);
    }

    /// <inheritdoc />
    public async Task<CycleDetectionResult> DetectCycleAsync(
        int parentEventId,
        string eventType,
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        // Get the full chain from the parent back to root
        var chain = await GetChainAsync(parentEventId, cancellationToken);

        // Check if the same event type appears in the chain more than twice (potential loop)
        var eventTypeCount = chain.Count(n => n.EventType == eventType);
        if (eventTypeCount >= 2)
        {
            var cycleEventIds = chain.Where(n => n.EventType == eventType).Select(n => n.EventId).ToList();
            _logger.LogWarning(
                "Potential cycle detected: event type {EventType} appears {Count} times in chain",
                eventType, eventTypeCount);
            return CycleDetectionResult.Detected(
                $"Event type '{eventType}' appears {eventTypeCount} times in chain",
                cycleEventIds);
        }

        // Check if the same entity was already processed in the chain
        if (!string.IsNullOrEmpty(entityType) && entityId > 0)
        {
            var sameEntity = chain.FirstOrDefault(n => n.EntityType == entityType && n.EntityId == entityId);
            if (sameEntity != null)
            {
                _logger.LogWarning(
                    "Potential cycle detected: entity {EntityType}:{EntityId} already in chain at event {EventId}",
                    entityType, entityId, sameEntity.EventId);
                return new CycleDetectionResult
                {
                    CycleDetected = true,
                    CycleDescription = $"Entity {entityType}:{entityId} already processed in chain",
                    CyclicEntityType = entityType,
                    CyclicEntityId = entityId,
                    CycleEventIds = new[] { sameEntity.EventId }
                };
            }
        }

        return CycleDetectionResult.NoCycle();
    }

    /// <inheritdoc />
    public async Task<int> GetChainDepthAsync(int eventId, CancellationToken cancellationToken = default)
    {
        // Try to get depth from general webhook deliveries
        var delivery = await _context.WebhookDeliveriesGeneral
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.WebhookEventId == eventId && !d.IsDeleted, cancellationToken);

        if (delivery != null)
        {
            return delivery.ChainDepth;
        }

        // If not found in general, try to trace back through parent references
        var chain = await GetChainAsync(eventId, cancellationToken);
        return chain.Count - 1; // Depth is 0-indexed
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventChainNode>> GetChainAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var chain = new List<EventChainNode>();
        var visited = new HashSet<int>();
        var currentEventId = eventId;
        int depth = 0;

        // Walk up the chain to find the root
        while (currentEventId > 0 && !visited.Contains(currentEventId))
        {
            visited.Add(currentEventId);

            var webEvent = await _context.WebhookEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == currentEventId && !e.IsDeleted, cancellationToken);

            if (webEvent == null)
                break;

            // Find delivery for this event to get parent info
            var delivery = await _context.WebhookDeliveriesGeneral
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.WebhookEventId == currentEventId && !d.IsDeleted, cancellationToken);

            var node = new EventChainNode
            {
                EventId = currentEventId,
                ParentEventId = delivery?.ParentEventId,
                Depth = depth,
                EventType = webEvent.EventType,
                EntityType = delivery?.EntityType,
                EntityId = delivery?.EntityId,
                CreatedAt = webEvent.CreatedAt
            };

            chain.Insert(0, node); // Insert at beginning since we're walking backwards
            depth++;

            // Move to parent
            if (delivery?.ParentEventId.HasValue == true)
            {
                currentEventId = delivery.ParentEventId.Value;
            }
            else
            {
                break; // No parent - this is the root
            }

            // Safety check to prevent infinite loops
            if (depth > MaxChainDepth + 5)
            {
                _logger.LogError("Chain depth exceeded safety limit while traversing chain for event {EventId}", eventId);
                break;
            }
        }

        // Recalculate depths now that we have the full chain
        for (int i = 0; i < chain.Count; i++)
        {
            chain[i] = chain[i] with { Depth = i };
        }

        return chain;
    }

    /// <inheritdoc />
    public async Task<bool> CanProcessAsync(int? parentEventId, CancellationToken cancellationToken = default)
    {
        if (!parentEventId.HasValue)
        {
            // Root event - always allowed
            return true;
        }

        var currentDepth = await GetChainDepthAsync(parentEventId.Value, cancellationToken);

        if (currentDepth >= MaxChainDepth)
        {
            _logger.LogWarning(
                "Processing blocked: event chain depth {Depth} would exceed max depth {MaxDepth}",
                currentDepth + 1, MaxChainDepth);
            return false;
        }

        return true;
    }
}

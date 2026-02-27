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

namespace CRM.Infrastructure.Services.Webhooks;

/// <summary>
/// Implementation of IEventChainTracker for tracking webhook event chains and detecting cycles.
/// Prevents infinite webhook loops by tracking parent-child event relationships and enforcing
/// a maximum chain depth.
/// Implements TODO-INT001-48.
/// </summary>
public class EventChainTracker : IEventChainTracker
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EventChainTracker> _logger;
    private const int DefaultMaxChainDepth = 10;

    public EventChainTracker(ICrmDbContext context, ILogger<EventChainTracker> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int MaxChainDepth => DefaultMaxChainDepth;

    /// <inheritdoc />
    public async Task<EventChainContext> StartChainAsync(int eventId, string eventType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting event chain for event {EventId} of type {EventType}", eventId, eventType);

        var correlationId = Guid.NewGuid().ToString("N");

        // Update the event with chain metadata
        var webhookEvent = await _context.WebhookEvents
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (webhookEvent != null)
        {
            webhookEvent.CorrelationId = correlationId;
            webhookEvent.ParentEventId = null; // Root event
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new EventChainContext
        {
            RootEventId = eventId,
            CurrentEventId = eventId,
            Depth = 0,
            StartedAt = DateTime.UtcNow,
            ChainId = correlationId
        };
    }

    /// <inheritdoc />
    public async Task<ChainAddResult> AddToChainAsync(
        int parentEventId,
        int childEventId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Adding child event {ChildEventId} ({EventType}) to parent {ParentEventId}",
            childEventId, eventType, parentEventId);

        // Get parent event to determine chain depth and correlation
        var parentEvent = await _context.WebhookEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == parentEventId, cancellationToken);

        if (parentEvent == null)
        {
            _logger.LogWarning("Parent event {ParentEventId} not found", parentEventId);
            return ChainAddResult.CycleFailed($"Parent event {parentEventId} not found");
        }

        // Calculate new depth
        var parentDepth = await GetChainDepthAsync(parentEventId, cancellationToken);
        var newDepth = parentDepth + 1;

        // Check max depth
        if (newDepth > MaxChainDepth)
        {
            _logger.LogWarning(
                "Max chain depth ({MaxDepth}) exceeded for event {EventId} at depth {Depth}",
                MaxChainDepth, childEventId, newDepth);
            return ChainAddResult.MaxDepthFailed(newDepth);
        }

        // Check for cycle: does adding this event create a cycle?
        var childEvent = await _context.WebhookEvents
            .FirstOrDefaultAsync(e => e.Id == childEventId, cancellationToken);

        if (childEvent != null)
        {
            // Detect cycles by checking if the child event's entity already appears in the chain
            var cycleCheck = await DetectCycleAsync(
                parentEventId,
                eventType,
                childEvent.EntityType,
                childEvent.EntityId,
                cancellationToken);

            if (cycleCheck.CycleDetected)
            {
                _logger.LogWarning(
                    "Cycle detected when adding event {ChildEventId} to chain: {Description}",
                    childEventId, cycleCheck.CycleDescription);
                return ChainAddResult.CycleFailed(cycleCheck.CycleDescription ?? "Cycle detected");
            }

            // Update child event with chain metadata
            childEvent.ParentEventId = parentEventId.ToString();
            childEvent.CorrelationId = parentEvent.CorrelationId;
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Event {ChildEventId} added to chain at depth {Depth}. CorrelationId={CorrelationId}",
            childEventId, newDepth, parentEvent.CorrelationId);

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
        // Walk up the chain from parent to root, checking for the same entity+eventType
        var chain = await GetChainAsync(parentEventId, cancellationToken);
        var cycleEventIds = new List<int>();

        foreach (var node in chain)
        {
            if (node.EntityType == entityType &&
                node.EntityId == entityId &&
                node.EventType == eventType)
            {
                cycleEventIds.Add(node.EventId);
            }
        }

        if (cycleEventIds.Count > 0)
        {
            var description = $"Entity {entityType}#{entityId} with event type '{eventType}' already exists in the chain at event(s): {string.Join(", ", cycleEventIds)}";
            return CycleDetectionResult.Detected(description, cycleEventIds);
        }

        return CycleDetectionResult.NoCycle();
    }

    /// <inheritdoc />
    public async Task<int> GetChainDepthAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var depth = 0;
        var currentId = eventId;
        var visited = new HashSet<int>();

        while (true)
        {
            if (!visited.Add(currentId))
            {
                // We've detected an internal cycle - break to prevent infinite loop
                _logger.LogWarning("Cycle detected while computing depth for event {EventId}", eventId);
                break;
            }

            var evt = await _context.WebhookEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == currentId, cancellationToken);

            if (evt?.ParentEventId == null || !int.TryParse(evt.ParentEventId, out var parentId))
                break;

            depth++;
            currentId = parentId;
        }

        return depth;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EventChainNode>> GetChainAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var chain = new List<EventChainNode>();
        var currentId = eventId;
        var visited = new HashSet<int>();

        // Walk up to root first
        var ancestors = new List<EventChainNode>();
        while (true)
        {
            if (!visited.Add(currentId))
                break;

            var evt = await _context.WebhookEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == currentId, cancellationToken);

            if (evt == null)
                break;

            ancestors.Add(new EventChainNode
            {
                EventId = evt.Id,
                ParentEventId = evt.ParentEventId != null && int.TryParse(evt.ParentEventId, out var pid) ? pid : null,
                Depth = 0, // Will be recalculated
                EventType = evt.EventType,
                EntityType = evt.EntityType,
                EntityId = evt.EntityId,
                CreatedAt = evt.CreatedAt
            });

            if (evt.ParentEventId == null || !int.TryParse(evt.ParentEventId, out var parentId))
                break;

            currentId = parentId;
        }

        // Reverse so root is first, then assign depths
        ancestors.Reverse();
        for (var i = 0; i < ancestors.Count; i++)
        {
            chain.Add(ancestors[i] with { Depth = i });
        }

        return chain;
    }

    /// <inheritdoc />
    public async Task<bool> CanProcessAsync(int? parentEventId, CancellationToken cancellationToken = default)
    {
        if (!parentEventId.HasValue)
            return true; // Root events are always allowed

        var depth = await GetChainDepthAsync(parentEventId.Value, cancellationToken);
        var nextDepth = depth + 1;

        if (nextDepth > MaxChainDepth)
        {
            _logger.LogWarning(
                "Processing blocked: chain depth {Depth} would exceed max {MaxDepth} for parent event {ParentEventId}",
                nextDepth, MaxChainDepth, parentEventId.Value);
            return false;
        }

        return true;
    }
}

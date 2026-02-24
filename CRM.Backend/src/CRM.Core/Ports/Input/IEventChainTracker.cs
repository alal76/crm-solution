// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for tracking webhook event chains and detecting cycles.
/// Implements TODO-INT001-48: Event chain tracking with cycle detection.
/// </summary>
public interface IEventChainTracker
{
    /// <summary>
    /// Maximum allowed chain depth before rejecting events.
    /// </summary>
    int MaxChainDepth { get; }

    /// <summary>
    /// Starts tracking a new event chain.
    /// </summary>
    /// <param name="eventId">The root event ID.</param>
    /// <param name="eventType">The event type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chain tracking context.</returns>
    Task<EventChainContext> StartChainAsync(int eventId, string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a child event to an existing chain.
    /// </summary>
    /// <param name="parentEventId">The parent event ID.</param>
    /// <param name="childEventId">The new child event ID.</param>
    /// <param name="eventType">The child event type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure with reason.</returns>
    Task<ChainAddResult> AddToChainAsync(
        int parentEventId,
        int childEventId,
        string eventType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if adding an event to a chain would cause a cycle.
    /// </summary>
    /// <param name="parentEventId">The potential parent event ID.</param>
    /// <param name="eventType">The event type being added.</param>
    /// <param name="entityType">The entity type involved.</param>
    /// <param name="entityId">The entity ID involved.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with cycle detection details.</returns>
    Task<CycleDetectionResult> DetectCycleAsync(
        int parentEventId,
        string eventType,
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current depth of an event chain.
    /// </summary>
    /// <param name="eventId">The event ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The depth of the event in its chain (0 for root events).</returns>
    Task<int> GetChainDepthAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full chain for an event (from root to current).
    /// </summary>
    /// <param name="eventId">The event ID to trace.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of events in the chain from root to the specified event.</returns>
    Task<IReadOnlyList<EventChainNode>> GetChainAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if processing should be allowed based on chain depth.
    /// </summary>
    /// <param name="parentEventId">The parent event ID (null for root events).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if processing is allowed, false if max depth exceeded.</returns>
    Task<bool> CanProcessAsync(int? parentEventId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for tracking an event chain.
/// </summary>
public record EventChainContext
{
    /// <summary>The root event ID of the chain.</summary>
    public int RootEventId { get; init; }

    /// <summary>The current event ID.</summary>
    public int CurrentEventId { get; init; }

    /// <summary>Current depth in the chain (0 for root).</summary>
    public int Depth { get; init; }

    /// <summary>When the chain was started.</summary>
    public DateTime StartedAt { get; init; }

    /// <summary>Unique identifier for this chain.</summary>
    public string ChainId { get; init; } = string.Empty;
}

/// <summary>
/// Result of adding an event to a chain.
/// </summary>
public record ChainAddResult
{
    /// <summary>Whether the add was successful.</summary>
    public bool Success { get; init; }

    /// <summary>Reason for failure if not successful.</summary>
    public string? FailureReason { get; init; }

    /// <summary>The new chain depth after adding.</summary>
    public int NewDepth { get; init; }

    /// <summary>Whether max depth was exceeded.</summary>
    public bool MaxDepthExceeded { get; init; }

    /// <summary>Whether a cycle was detected.</summary>
    public bool CycleDetected { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static ChainAddResult Succeeded(int newDepth) =>
        new() { Success = true, NewDepth = newDepth };

    /// <summary>Creates a failure result due to max depth exceeded.</summary>
    public static ChainAddResult MaxDepthFailed(int depth) =>
        new() { Success = false, FailureReason = $"Max chain depth ({depth}) exceeded", MaxDepthExceeded = true, NewDepth = depth };

    /// <summary>Creates a failure result due to cycle detected.</summary>
    public static ChainAddResult CycleFailed(string details) =>
        new() { Success = false, FailureReason = $"Cycle detected: {details}", CycleDetected = true };
}

/// <summary>
/// Result of cycle detection check.
/// </summary>
public record CycleDetectionResult
{
    /// <summary>Whether a cycle was detected.</summary>
    public bool CycleDetected { get; init; }

    /// <summary>Description of the detected cycle.</summary>
    public string? CycleDescription { get; init; }

    /// <summary>Event IDs involved in the cycle.</summary>
    public IReadOnlyList<int> CycleEventIds { get; init; } = Array.Empty<int>();

    /// <summary>The entity type that would cause the cycle.</summary>
    public string? CyclicEntityType { get; init; }

    /// <summary>The entity ID that would cause the cycle.</summary>
    public int? CyclicEntityId { get; init; }

    /// <summary>Creates a result indicating no cycle.</summary>
    public static CycleDetectionResult NoCycle() => new() { CycleDetected = false };

    /// <summary>Creates a result indicating a cycle was detected.</summary>
    public static CycleDetectionResult Detected(string description, IReadOnlyList<int>? eventIds = null) =>
        new() { CycleDetected = true, CycleDescription = description, CycleEventIds = eventIds ?? Array.Empty<int>() };
}

/// <summary>
/// Represents a node in an event chain.
/// </summary>
public record EventChainNode
{
    /// <summary>The event ID.</summary>
    public int EventId { get; init; }

    /// <summary>The parent event ID (null for root).</summary>
    public int? ParentEventId { get; init; }

    /// <summary>Depth in the chain (0 for root).</summary>
    public int Depth { get; init; }

    /// <summary>The event type.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>The entity type this event relates to.</summary>
    public string? EntityType { get; init; }

    /// <summary>The entity ID this event relates to.</summary>
    public int? EntityId { get; init; }

    /// <summary>When the event was created.</summary>
    public DateTime CreatedAt { get; init; }
}

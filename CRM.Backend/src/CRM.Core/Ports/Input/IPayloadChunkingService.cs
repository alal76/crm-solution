// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for splitting large webhook payloads into manageable chunks.
/// Implements TODO-INT001-47: Large payload handling/chunking.
/// </summary>
public interface IPayloadChunkingService
{
    /// <summary>
    /// Maximum payload size in bytes before chunking is applied.
    /// </summary>
    int MaxPayloadSizeBytes { get; }

    /// <summary>
    /// Determines if a payload needs to be chunked.
    /// </summary>
    /// <param name="payload">The payload string.</param>
    /// <returns>True if the payload exceeds the maximum size.</returns>
    bool NeedsChunking(string payload);

    /// <summary>
    /// Splits a payload into chunks with sequence metadata.
    /// </summary>
    /// <param name="payload">The full payload string.</param>
    /// <param name="webhookId">The webhook ID for correlation.</param>
    /// <param name="eventId">The event ID for correlation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of chunked payloads with sequence info.</returns>
    Task<IReadOnlyList<PayloadChunk>> ChunkPayloadAsync(
        string payload,
        int webhookId,
        int eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reassembles a chunked payload from its parts.
    /// </summary>
    /// <param name="chunks">The individual chunks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reassembled payload.</returns>
    Task<string> ReassembleAsync(
        IReadOnlyList<PayloadChunk> chunks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all chunks for a given batch have been received.
    /// </summary>
    /// <param name="batchId">The batch identifier.</param>
    /// <param name="receivedChunks">The chunks received so far.</param>
    /// <returns>Validation result indicating completeness.</returns>
    ChunkValidationResult ValidateChunks(string batchId, IReadOnlyList<PayloadChunk> receivedChunks);
}

/// <summary>
/// Represents a single chunk of a larger payload.
/// </summary>
public record PayloadChunk
{
    /// <summary>The unique batch identifier linking all chunks of the same payload.</summary>
    public string BatchId { get; init; } = string.Empty;

    /// <summary>The sequence number of this chunk (1-based).</summary>
    public int SequenceNumber { get; init; }

    /// <summary>The total number of chunks in the batch.</summary>
    public int TotalChunks { get; init; }

    /// <summary>The webhook ID this chunk belongs to.</summary>
    public int WebhookId { get; init; }

    /// <summary>The event ID this chunk relates to.</summary>
    public int EventId { get; init; }

    /// <summary>The chunk payload data.</summary>
    public string Data { get; init; } = string.Empty;

    /// <summary>SHA-256 checksum of the chunk data for integrity verification.</summary>
    public string Checksum { get; init; } = string.Empty;

    /// <summary>Size of this chunk in bytes.</summary>
    public int SizeBytes { get; init; }

    /// <summary>Timestamp when the chunk was created.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of chunk validation.
/// </summary>
public record ChunkValidationResult
{
    /// <summary>Whether all chunks are present and valid.</summary>
    public bool IsComplete { get; init; }

    /// <summary>Whether all checksums pass verification.</summary>
    public bool ChecksumsValid { get; init; }

    /// <summary>Sequence numbers of missing chunks.</summary>
    public IReadOnlyList<int> MissingSequences { get; init; } = Array.Empty<int>();

    /// <summary>Sequence numbers of corrupted chunks.</summary>
    public IReadOnlyList<int> CorruptedSequences { get; init; } = Array.Empty<int>();

    /// <summary>Expected total chunks.</summary>
    public int ExpectedTotalChunks { get; init; }

    /// <summary>Received chunk count.</summary>
    public int ReceivedChunks { get; init; }

    /// <summary>Error message if validation failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Creates a successful validation result.</summary>
    public static ChunkValidationResult Complete(int totalChunks) =>
        new() { IsComplete = true, ChecksumsValid = true, ExpectedTotalChunks = totalChunks, ReceivedChunks = totalChunks };

    /// <summary>Creates an incomplete validation result.</summary>
    public static ChunkValidationResult Incomplete(int expected, int received, IReadOnlyList<int> missing) =>
        new() { IsComplete = false, ChecksumsValid = true, ExpectedTotalChunks = expected, ReceivedChunks = received, MissingSequences = missing };
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services.Webhooks;

/// <summary>
/// Implementation of IPayloadChunkingService for splitting large webhook payloads.
/// Splits payloads that exceed a configurable size limit into sequenced chunks
/// with checksums for integrity verification.
/// Implements TODO-INT001-47.
/// </summary>
public class PayloadChunkingService : IPayloadChunkingService
{
    private readonly ILogger<PayloadChunkingService> _logger;
    private const int DefaultMaxPayloadSizeBytes = 256 * 1024; // 256 KB default limit

    public PayloadChunkingService(ILogger<PayloadChunkingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int MaxPayloadSizeBytes => DefaultMaxPayloadSizeBytes;

    /// <inheritdoc />
    public bool NeedsChunking(string payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return false;
        }

        return Encoding.UTF8.GetByteCount(payload) > MaxPayloadSizeBytes;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PayloadChunk>> ChunkPayloadAsync(
        string payload,
        int webhookId,
        int eventId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return Task.FromResult<IReadOnlyList<PayloadChunk>>(new List<PayloadChunk>
            {
                CreateChunk(payload ?? string.Empty, Guid.NewGuid().ToString("N"), webhookId, eventId, 1, 1)
            });
        }

        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        if (payloadBytes.Length <= MaxPayloadSizeBytes)
        {
            // No chunking needed — single chunk
            var batchId = Guid.NewGuid().ToString("N");
            return Task.FromResult<IReadOnlyList<PayloadChunk>>(new List<PayloadChunk>
            {
                CreateChunk(payload, batchId, webhookId, eventId, 1, 1)
            });
        }

        var chunks = new List<PayloadChunk>();
        var batch = Guid.NewGuid().ToString("N");

        // Try to chunk at JSON boundaries if possible
        if (TryChunkJson(payload, batch, webhookId, eventId, out var jsonChunks))
        {
            chunks = jsonChunks;
        }
        else
        {
            // Fall back to byte-level chunking
            chunks = ChunkByBytes(payloadBytes, batch, webhookId, eventId);
        }

        _logger.LogInformation(
            "Payload for webhook {WebhookId} event {EventId} split into {ChunkCount} chunks (batch {BatchId}). Total size: {Size} bytes",
            webhookId, eventId, chunks.Count, batch, payloadBytes.Length);

        return Task.FromResult<IReadOnlyList<PayloadChunk>>(chunks);
    }

    /// <inheritdoc />
    public Task<string> ReassembleAsync(IReadOnlyList<PayloadChunk> chunks, CancellationToken cancellationToken = default)
    {
        if (chunks == null || chunks.Count == 0)
        {
            return Task.FromResult(string.Empty);
        }

        var ordered = chunks.OrderBy(c => c.SequenceNumber).ToList();

        // Validate checksums
        foreach (var chunk in ordered)
        {
            var expectedChecksum = ComputeChecksum(chunk.Data);
            if (chunk.Checksum != expectedChecksum)
            {
                throw new InvalidOperationException(
                    $"Checksum mismatch for chunk {chunk.SequenceNumber}/{chunk.TotalChunks} in batch {chunk.BatchId}");
            }
        }

        var reassembled = string.Concat(ordered.Select(c => c.Data));
        return Task.FromResult(reassembled);
    }

    /// <inheritdoc />
    public ChunkValidationResult ValidateChunks(string batchId, IReadOnlyList<PayloadChunk> receivedChunks)
    {
        if (receivedChunks == null || receivedChunks.Count == 0)
        {
            return new ChunkValidationResult
            {
                IsComplete = false,
                ChecksumsValid = false,
                ErrorMessage = "No chunks received"
            };
        }

        var expectedTotal = receivedChunks.Max(c => c.TotalChunks);
        var receivedSequences = receivedChunks.Select(c => c.SequenceNumber).ToHashSet();
        var missing = Enumerable.Range(1, expectedTotal).Where(i => !receivedSequences.Contains(i)).ToList();

        var corrupted = new List<int>();
        foreach (var chunk in receivedChunks)
        {
            var expected = ComputeChecksum(chunk.Data);
            if (chunk.Checksum != expected)
            {
                corrupted.Add(chunk.SequenceNumber);
            }
        }

        if (missing.Count == 0 && corrupted.Count == 0)
        {
            return ChunkValidationResult.Complete(expectedTotal);
        }

        if (missing.Count > 0)
        {
            return ChunkValidationResult.Incomplete(expectedTotal, receivedChunks.Count, missing);
        }

        return new ChunkValidationResult
        {
            IsComplete = false,
            ChecksumsValid = false,
            ExpectedTotalChunks = expectedTotal,
            ReceivedChunks = receivedChunks.Count,
            CorruptedSequences = corrupted,
            ErrorMessage = $"{corrupted.Count} chunk(s) failed checksum verification"
        };
    }

    private bool TryChunkJson(string payload, string batchId, int webhookId, int eventId, out List<PayloadChunk> chunks)
    {
        chunks = new List<PayloadChunk>();

        try
        {
            // Try to parse as JSON array and split by items
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = new List<string>();
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    items.Add(element.GetRawText());
                }

                // NOSONAR - array with 0 or 1 items cannot be split into chunks
                if (items.Count <= 1)
                {
                    return false;
                }

                // Group items into chunks that fit the size limit
                var currentItems = new List<string>();
                var currentSize = 2; // for '[]' wrapper

                foreach (var item in items)
                {
                    var itemSize = Encoding.UTF8.GetByteCount(item) + 1; // +1 for comma
                    if (currentSize + itemSize > MaxPayloadSizeBytes && currentItems.Count > 0)
                    {
                        chunks.Add(CreateChunk(
                            "[" + string.Join(",", currentItems) + "]",
                            batchId, webhookId, eventId, chunks.Count + 1, 0));
                        currentItems.Clear();
                        currentSize = 2;
                    }

                    currentItems.Add(item);
                    currentSize += itemSize;
                }

                if (currentItems.Count > 0)
                {
                    chunks.Add(CreateChunk(
                        "[" + string.Join(",", currentItems) + "]",
                        batchId, webhookId, eventId, chunks.Count + 1, 0));
                }

                // Update total chunks
                var total = chunks.Count;
                chunks = chunks.Select(c => c with { TotalChunks = total }).ToList();
                return true;
            }
        }
        catch (JsonException)
        {
            // Not valid JSON, fall back to byte chunking
        }

        return false;
    }

    private List<PayloadChunk> ChunkByBytes(byte[] payloadBytes, string batchId, int webhookId, int eventId)
    {
        var chunks = new List<PayloadChunk>();
        var totalChunks = (int)Math.Ceiling((double)payloadBytes.Length / MaxPayloadSizeBytes);

        for (var i = 0; i < totalChunks; i++)
        {
            var offset = i * MaxPayloadSizeBytes;
            var length = Math.Min(MaxPayloadSizeBytes, payloadBytes.Length - offset);
            var chunkData = Encoding.UTF8.GetString(payloadBytes, offset, length);

            chunks.Add(CreateChunk(chunkData, batchId, webhookId, eventId, i + 1, totalChunks));
        }

        return chunks;
    }

    private static PayloadChunk CreateChunk(string data, string batchId, int webhookId, int eventId, int sequence, int total)
    {
        return new PayloadChunk
        {
            BatchId = batchId,
            SequenceNumber = sequence,
            TotalChunks = total,
            WebhookId = webhookId,
            EventId = eventId,
            Data = data,
            Checksum = ComputeChecksum(data),
            SizeBytes = Encoding.UTF8.GetByteCount(data),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string ComputeChecksum(string data)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

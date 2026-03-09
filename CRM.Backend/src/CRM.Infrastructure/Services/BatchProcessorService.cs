// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Processes large item collections in configurable mini-batches with concurrency
/// control, in-memory progress tracking, and async progress streaming.
/// </summary>
/// <typeparam name="T">Type of item to process.</typeparam>
public sealed class BatchProcessorService<T> : IBatchProcessor<T>
{
    private readonly ILogger<BatchProcessorService<T>> _logger;

    // In-memory store of live BatchProgress snapshots keyed by batchId
    private readonly ConcurrentDictionary<string, BatchProgress> _progressStore = new();

    public BatchProcessorService(ILogger<BatchProcessorService<T>> logger)
    {
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // IBatchProcessor<T> implementation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<BatchResult> ProcessAsync(
        IEnumerable<T> items,
        Func<T, CancellationToken, Task<bool>> processor,
        BatchOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new BatchOptions();
        var batchId = Guid.NewGuid().ToString("N");

        var itemList = items is IList<T> list ? list : [.. items];
        var total = itemList.Count;

        var progress = new BatchProgress { BatchId = batchId, Total = total };
        _progressStore[batchId] = progress;

        _logger.LogInformation(
            "BatchProcessor [{BatchId}] starting: {Total} items, batchSize={BatchSize}, maxConcurrency={MaxConcurrency}",
            batchId, total, options.BatchSize, options.MaxConcurrency);

        var startTime = DateTime.UtcNow;
        var errors = new List<string>();
        var succeeded = 0;
        var failed = 0;
        var aborted = false;

        using var semaphore = new SemaphoreSlim(options.MaxConcurrency);

        // Process in mini-batches
        for (var offset = 0; offset < total && !aborted; offset += options.BatchSize)
        {
            ct.ThrowIfCancellationRequested();

            var batchItems = itemList
                .Skip(offset)
                .Take(options.BatchSize)
                .ToList();

            var tasks = batchItems.Select(async item =>
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    bool success;
                    try
                    {
                        success = await processor(item, ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        var message = $"Item processing error: {ex.Message}";
                        _logger.LogWarning(ex, "BatchProcessor [{BatchId}] item error", batchId);
                        lock (errors)
                        {
                            // cap to avoid memory blow-up
                            if (errors.Count < 500)
                            {
                                errors.Add(message);
                            }
                        }
                    }

                    if (success)
                    {
                        Interlocked.Increment(ref succeeded);
                    }
                    else
                    {
                        Interlocked.Increment(ref failed);
                    }
                }
                finally
                {
                    semaphore.Release();

                    // Update progress snapshot
                    var processed = Interlocked.Add(ref succeeded, 0) + Interlocked.Add(ref failed, 0);
                    progress = new BatchProgress
                    {
                        BatchId = batchId,
                        Total = total,
                        Processed = processed,
                        Failed = Interlocked.Add(ref failed, 0),
                    };
                    _progressStore[batchId] = progress;
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (options.StopOnError && failed > 0)
            {
                _logger.LogWarning(
                    "BatchProcessor [{BatchId}] stopping early due to failure (StopOnError=true)",
                    batchId);
                aborted = true;
            }
        }

        var finalProgress = new BatchProgress
        {
            BatchId = batchId,
            Total = total,
            Processed = succeeded + failed,
            Failed = failed,
        };
        _progressStore[batchId] = finalProgress;

        var result = new BatchResult
        {
            Total = total,
            Succeeded = succeeded,
            Failed = failed,
            Duration = DateTime.UtcNow - startTime,
            Errors = errors,
        };

        _logger.LogInformation(
            "BatchProcessor [{BatchId}] finished: {Succeeded} succeeded, {Failed} failed, duration={Duration}",
            batchId, succeeded, failed, result.Duration);

        // Evict after a short TTL so progress streams can drain
        _ = Task.Delay(TimeSpan.FromMinutes(5))
              .ContinueWith(completedTask => _progressStore.TryRemove(batchId, out _));

        return result;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<BatchProgress> GetProgressStream(string batchId)
    {
        // Poll the in-memory store every 500 ms and yield the current snapshot
        while (true)
        {
            if (_progressStore.TryGetValue(batchId, out var snapshot))
            {
                yield return snapshot;

                // Stop streaming once all items are accounted for
                if (snapshot.Total > 0 && snapshot.Processed >= snapshot.Total)
                {
                    break;
                }
            }
            else
            {
                // Batch not found (hasn't started or already evicted); yield empty and break
                yield return new BatchProgress { BatchId = batchId };
                break;
            }

            await Task.Delay(500).ConfigureAwait(false);
        }
    }
}

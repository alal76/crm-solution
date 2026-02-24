// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Concurrent;

namespace CRM.Core.Ports.Input;

// ============================================================================
// Supporting types
// ============================================================================

/// <summary>
/// Options controlling how a batch is processed.
/// </summary>
public class BatchOptions
{
    /// <summary>Number of items processed per mini-batch.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Maximum degree of parallelism within a batch.</summary>
    public int MaxConcurrency { get; set; } = 4;

    /// <summary>
    /// When true, processing stops on the first item failure.
    /// When false (default), all items are attempted and failures are recorded.
    /// </summary>
    public bool StopOnError { get; set; } = false;
}

/// <summary>
/// Aggregate result returned after the entire batch completes (or is aborted).
/// </summary>
public class BatchResult
{
    /// <summary>Total number of items submitted.</summary>
    public int Total { get; set; }

    /// <summary>Items that were processed without error.</summary>
    public int Succeeded { get; set; }

    /// <summary>Items that raised an exception or returned <c>false</c>.</summary>
    public int Failed { get; set; }

    /// <summary>Wall-clock time from start to finish.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Error messages collected during processing (up to 500 to avoid memory blow-up).</summary>
    public List<string> Errors { get; set; } = [];
}

/// <summary>
/// A progress snapshot published to <see cref="IBatchProcessor{T}.GetProgressStream"/>.
/// </summary>
public class BatchProgress
{
    /// <summary>Unique identifier for this batch run.</summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>Items processed so far (includes both successes and failures).</summary>
    public int Processed { get; set; }

    /// <summary>Total items in the batch.</summary>
    public int Total { get; set; }

    /// <summary>Failed items so far.</summary>
    public int Failed { get; set; }
}

// ============================================================================
// Port interface
// ============================================================================

/// <summary>
/// Input port for processing large, arbitrary collections of items in
/// configurable mini-batches with concurrency control and progress streaming.
/// </summary>
/// <typeparam name="T">The type of items to process.</typeparam>
public interface IBatchProcessor<T>
{
    /// <summary>
    /// Processes all <paramref name="items"/> by invoking <paramref name="processor"/>
    /// on each one, respecting <see cref="BatchOptions.BatchSize"/> and
    /// <see cref="BatchOptions.MaxConcurrency"/>.
    /// </summary>
    /// <param name="items">Items to process.</param>
    /// <param name="processor">
    /// Work function for a single item. Return <c>true</c> on success, <c>false</c> to
    /// count the item as failed without raising an exception.
    /// </param>
    /// <param name="options">Optional tuning parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregate result once all items have been processed.</returns>
    Task<BatchResult> ProcessAsync(
        IEnumerable<T> items,
        Func<T, CancellationToken, Task<bool>> processor,
        BatchOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns an async stream of <see cref="BatchProgress"/> snapshots for the
    /// batch identified by <paramref name="batchId"/>.
    /// Yields until the batch completes or the consumer cancels the enumeration.
    /// </summary>
    IAsyncEnumerable<BatchProgress> GetProgressStream(string batchId);
}

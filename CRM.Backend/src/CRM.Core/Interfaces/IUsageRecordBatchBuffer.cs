// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// In-memory buffer for usage records that accumulates items and flushes them
/// in batches to the subscription service.
/// TODO-SALES006-024: Usage record batching implementation.
/// </summary>
public interface IUsageRecordBatchBuffer
{
    /// <summary>Enqueue a usage record for async batch processing.</summary>
    void Enqueue(UsageRecordBatchDto record);

    /// <summary>Number of records currently in the queue.</summary>
    int QueuedCount { get; }

    /// <summary>
    /// Dequeue up to <paramref name="maxCount"/> records and return them.
    /// Non-blocking: returns any available records immediately.
    /// </summary>
    IReadOnlyList<UsageRecordBatchDto> Drain(int maxCount);
}

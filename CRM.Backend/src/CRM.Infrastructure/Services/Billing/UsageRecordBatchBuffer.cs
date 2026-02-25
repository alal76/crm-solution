// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Concurrent;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Billing;

/// <summary>
/// Thread-safe in-memory queue for usage records.
/// Items are enqueued by application code and drained by <see cref="UsageRecordBatchHostedService"/>.
/// TODO-SALES006-024: Usage record batching.
/// </summary>
public sealed class UsageRecordBatchBuffer : IUsageRecordBatchBuffer
{
    private readonly ConcurrentQueue<UsageRecordBatchDto> _queue = new();
    private readonly ILogger<UsageRecordBatchBuffer> _logger;

    public UsageRecordBatchBuffer(ILogger<UsageRecordBatchBuffer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public int QueuedCount => _queue.Count;

    /// <inheritdoc/>
    public void Enqueue(UsageRecordBatchDto record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _queue.Enqueue(record);
        _logger.LogDebug(
            "Usage record enqueued: subscription={SubscriptionId} metric={Metric} qty={Qty}. Queue depth={Depth}",
            record.SubscriptionId,
            record.MetricName,
            record.Quantity,
            _queue.Count);
    }

    /// <inheritdoc/>
    public IReadOnlyList<UsageRecordBatchDto> Drain(int maxCount)
    {
        var batch = new List<UsageRecordBatchDto>(maxCount);
        while (batch.Count < maxCount && _queue.TryDequeue(out var item))
        {
            batch.Add(item);
        }
        return batch;
    }
}

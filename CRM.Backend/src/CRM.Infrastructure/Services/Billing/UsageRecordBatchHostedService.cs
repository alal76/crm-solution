// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Billing;

/// <summary>
/// Background service that drains the <see cref="IUsageRecordBatchBuffer"/> and
/// persists records via <see cref="ISubscriptionService.RecordUsageBatchAsync"/>.
/// Flushes every 30 seconds or when the queue reaches 100 records, whichever comes first.
/// TODO-SALES006-024: Usage record batching with hosted service.
/// </summary>
public sealed class UsageRecordBatchHostedService : BackgroundService
{
    /// <summary>Maximum records to flush in a single batch.</summary>
    public const int BatchSize = 100;

    /// <summary>How often to flush regardless of queue depth.</summary>
    public static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(30);

    private readonly IUsageRecordBatchBuffer _buffer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsageRecordBatchHostedService> _logger;

    public UsageRecordBatchHostedService(
        IUsageRecordBatchBuffer buffer,
        IServiceProvider serviceProvider,
        ILogger<UsageRecordBatchHostedService> logger)
    {
        _buffer = buffer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UsageRecordBatchHostedService started (batchSize={BatchSize}, interval={Interval}s)",
            BatchSize,
            FlushInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Flush immediately when the queue is at capacity to avoid unbounded growth
                if (_buffer.QueuedCount >= BatchSize)
                {
                    await FlushBatchAsync(stoppingToken);
                }
                else
                {
                    // Otherwise wait for the flush interval (or until batch fills)
                    await Task.Delay(FlushInterval, stoppingToken);
                    if (_buffer.QueuedCount > 0)
                    {
                        await FlushBatchAsync(stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UsageRecordBatchHostedService flush loop");
                // Back off briefly to avoid tight error loops
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        // Drain any remaining records on graceful shutdown
        if (_buffer.QueuedCount > 0)
        {
            _logger.LogInformation("Flushing remaining {Count} usage records on shutdown", _buffer.QueuedCount);
            await FlushBatchAsync(CancellationToken.None);
        }

        _logger.LogInformation("UsageRecordBatchHostedService stopped");
    }

    /// <summary>
    /// Drains up to <see cref="BatchSize"/> records from the buffer and persists them.
    /// </summary>
    internal async Task FlushBatchAsync(CancellationToken ct)
    {
        var batch = _buffer.Drain(BatchSize);
        if (batch.Count == 0)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

        try
        {
            var saved = await subscriptionService.RecordUsageBatchAsync(batch.ToList(), ct);
            _logger.LogInformation(
                "Usage record batch flushed: {Requested} submitted, {Saved} persisted",
                batch.Count,
                saved);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Re-queue the batch so records are not lost
            foreach (var item in batch)
            {
                _buffer.Enqueue(item);
            }
            _logger.LogError(
                ex,
                "Failed to persist usage record batch ({Count} records re-queued)",
                batch.Count);
        }
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities.Workflow;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that periodically purges old workflow log entries
/// based on configurable retention periods per log level.
/// </summary>
public class WorkflowLogRetentionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowLogRetentionService> _logger;

    /// <summary>
    /// Maximum number of rows to delete per batch to avoid long-running transactions.
    /// </summary>
    private const int BatchSize = 1000;

    /// <summary>
    /// How often the retention check runs (default: once per day).
    /// </summary>
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    /// <summary>
    /// Retention periods per log level (in days). Logs older than these thresholds are purged.
    /// </summary>
    private static readonly Dictionary<WorkflowLogLevel, int> DefaultRetentionDays = new()
    {
        { WorkflowLogLevel.Debug, 30 },
        { WorkflowLogLevel.Info, 90 },
        { WorkflowLogLevel.Warning, 180 },
        { WorkflowLogLevel.Error, 365 },
        { WorkflowLogLevel.Critical, 730 },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowLogRetentionService"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for creating scoped services.</param>
    /// <param name="logger">Logger instance.</param>
    public WorkflowLogRetentionService(
        IServiceProvider serviceProvider,
        ILogger<WorkflowLogRetentionService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "WorkflowLogRetentionService started. Retention check runs every {Hours} hour(s)",
            _checkInterval.TotalHours);

        // Delay startup to let the application finish initialization
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredLogsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("WorkflowLogRetentionService stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in WorkflowLogRetentionService loop");
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("WorkflowLogRetentionService stopped");
    }

    /// <summary>
    /// Purges workflow log entries that exceed their retention period.
    /// Processes each log level independently, deleting in batches.
    /// </summary>
    private async Task PurgeExpiredLogsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();

        var totalDeleted = 0;

        foreach (var (level, retentionDays) in DefaultRetentionDays)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            try
            {
                var deletedForLevel = await DeleteLogsInBatchesAsync(context, level, cutoffDate, stoppingToken);
                if (deletedForLevel > 0)
                {
                    totalDeleted += deletedForLevel;
                    _logger.LogInformation(
                        "Purged {Count} {Level} workflow log entries older than {CutoffDate:yyyy-MM-dd}",
                        deletedForLevel, level, cutoffDate);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Error purging {Level} workflow logs (cutoff: {CutoffDate:yyyy-MM-dd})", level, cutoffDate);
            }
        }

        if (totalDeleted > 0)
        {
            _logger.LogInformation("WorkflowLogRetentionService purged {TotalCount} expired log entries", totalDeleted);
        }
        else
        {
            _logger.LogDebug("WorkflowLogRetentionService: no expired log entries found");
        }
    }

    /// <summary>
    /// Deletes log entries in batches to avoid long-running transactions and excessive memory usage.
    /// </summary>
    private static async Task<int> DeleteLogsInBatchesAsync(
        CrmDbContext context,
        WorkflowLogLevel level,
        DateTime cutoffDate,
        CancellationToken stoppingToken)
    {
        var totalDeleted = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var batch = await context.WorkflowLogs
                .Where(l => l.Level == level && l.Timestamp < cutoffDate)
                .OrderBy(l => l.Timestamp)
                .Take(BatchSize)
                .ToListAsync(stoppingToken);

            if (batch.Count == 0)
            {
                break;
            }

            context.WorkflowLogs.RemoveRange(batch);
            await context.SaveChangesAsync(stoppingToken);
            totalDeleted += batch.Count;

            // If we got fewer than a full batch, we're done
            if (batch.Count < BatchSize)
            {
                break;
            }
        }

        return totalDeleted;
    }
}

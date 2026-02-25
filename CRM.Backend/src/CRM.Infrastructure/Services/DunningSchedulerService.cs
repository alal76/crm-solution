// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that schedules and executes automated dunning processes.
/// Uses the existing DunningManager to process failed payments and send escalation emails.
///
/// TODO-SALES003-012: DunningSchedulerService implementation.
///
/// Scheduling:
/// - Runs every 4 hours (six times daily) by default
/// - Processes all due dunning records
/// - Sends escalation emails based on retry attempt level
///
/// Exponential back-off (managed inside DunningManager.RetryFailedPaymentAsync):
///   Attempt 1: same day   | Attempt 2: +3 days
///   Attempt 3: +7 days    | Attempt 4+: +14 days
///
/// Uses IServiceScopeFactory to resolve IDunningManager per cycle,
/// avoiding the captive-dependency problem (scoped inside singleton).
///
/// SPEC: PHASE 6 - Subscription Billing Services
/// </summary>
public class DunningSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DunningSchedulerService> _logger;
    private readonly TimeSpan _runInterval;
    private readonly TimeSpan[] _scheduledTimes;

    /// <summary>
    /// Creates a new DunningSchedulerService (production constructor — uses IServiceScopeFactory).
    /// </summary>
    /// <param name="scopeFactory">Service scope factory to resolve scoped IDunningManager per cycle</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="runIntervalHours">Hours between runs (default: 4h = six times daily)</param>
    public DunningSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<DunningSchedulerService> logger,
        int runIntervalHours = 4)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _runInterval = TimeSpan.FromHours(runIntervalHours);

        // Default scheduled times: every 4 hours (6 runs/day) UTC
        _scheduledTimes = new[]
        {
            TimeSpan.FromHours(0),  //  0:00 UTC (midnight)
            TimeSpan.FromHours(4),  //  4:00 UTC
            TimeSpan.FromHours(8),  //  8:00 UTC
            TimeSpan.FromHours(12), // 12:00 UTC (noon)
            TimeSpan.FromHours(16), // 16:00 UTC
            TimeSpan.FromHours(20), // 20:00 UTC
        };
    }

    /// <summary>
    /// Executes the dunning scheduler background loop.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DunningSchedulerService started. Running every {Interval} hours", _runInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextRunTime = GetNextScheduledTime();
                var delay = nextRunTime - DateTime.UtcNow;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogDebug("Next dunning run scheduled for {NextRun}", nextRunTime);
                    await Task.Delay(delay, stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunDunningCycleAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Service is stopping, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in dunning scheduler loop. Will retry at next scheduled time.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("DunningSchedulerService stopped");
    }

    /// <summary>
    /// Runs a dunning cycle: resolves IDunningManager in a fresh scope, processes all due payments,
    /// and sends escalation emails.  Each cycle gets its own EF Core DbContext (via scoped IDunningManager).
    /// </summary>
    public async Task<DunningCycleResultDto> RunDunningCycleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scheduled dunning cycle at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var dunningManager = scope.ServiceProvider.GetRequiredService<IDunningManager>();

        var result = await dunningManager.ProcessDunningAsync(cancellationToken);

        _logger.LogInformation(
            "Dunning cycle completed: {Processed} processed, {Success} succeeded, {Escalated} escalated, {Paused} paused, {Cancelled} cancelled",
            result.ProcessedCount,
            result.SuccessfulRetries,
            result.EscalatedCount,
            result.PausedSubscriptions,
            result.CancelledSubscriptions);

        if (result.Errors.Any())
        {
            _logger.LogWarning("Dunning cycle had {ErrorCount} errors: {Errors}",
                result.Errors.Count, string.Join("; ", result.Errors.Take(5)));
        }

        return result;
    }

    /// <summary>
    /// Calculates the next scheduled run time based on configured schedule.
    /// </summary>
    private DateTime GetNextScheduledTime()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Find the next scheduled time today or tomorrow
        foreach (var time in _scheduledTimes.OrderBy(t => t))
        {
            var scheduledTime = today.Add(time);
            if (scheduledTime > now)
            {
                return scheduledTime;
            }
        }

        // No more times today, use first time tomorrow
        return today.AddDays(1).Add(_scheduledTimes.Min());
    }

    /// <summary>
    /// Manually trigger a dunning cycle (for testing or manual intervention).
    /// </summary>
    public async Task<DunningCycleResultDto> TriggerManualCycleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual dunning cycle triggered");
        return await RunDunningCycleAsync(cancellationToken);
    }

    /// <summary>
    /// Gets information about the next scheduled run.
    /// </summary>
    public SchedulerStatusDto GetSchedulerStatus()
    {
        return new SchedulerStatusDto
        {
            NextScheduledRun = GetNextScheduledTime(),
            RunIntervalHours = (int)_runInterval.TotalHours,
            ScheduledTimes = _scheduledTimes.Select(t => t.ToString(@"hh\:mm")).ToList(),
            IsRunning = true
        };
    }
}

/// <summary>
/// Status information for the dunning scheduler.
/// </summary>
public class SchedulerStatusDto
{
    public DateTime NextScheduledRun { get; set; }
    public int RunIntervalHours { get; set; }
    public List<string> ScheduledTimes { get; set; } = new();
    public bool IsRunning { get; set; }
}

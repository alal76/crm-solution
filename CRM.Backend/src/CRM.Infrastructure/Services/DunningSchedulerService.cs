// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
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
/// - Runs twice daily (default: 6 AM and 6 PM UTC)
/// - Processes all due dunning records
/// - Sends escalation emails based on retry attempt level
///
/// SPEC: PHASE 6 - Subscription Billing Services
/// </summary>
public class DunningSchedulerService : BackgroundService
{
    private readonly IDunningManager _dunningManager;
    private readonly ILogger<DunningSchedulerService> _logger;
    private readonly TimeSpan _runInterval;
    private readonly TimeSpan[] _scheduledTimes;

    /// <summary>
    /// Creates a new DunningSchedulerService.
    /// </summary>
    /// <param name="dunningManager">Dunning manager for processing payments</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="runIntervalHours">Hours between runs (default: 12h = twice daily)</param>
    public DunningSchedulerService(
        IDunningManager dunningManager,
        ILogger<DunningSchedulerService> logger,
        int runIntervalHours = 12)
    {
        _dunningManager = dunningManager ?? throw new ArgumentNullException(nameof(dunningManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _runInterval = TimeSpan.FromHours(runIntervalHours);

        // Default scheduled times: 6 AM and 6 PM UTC
        _scheduledTimes = new[]
        {
            TimeSpan.FromHours(6),  // 6:00 AM UTC
            TimeSpan.FromHours(18)  // 6:00 PM UTC
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
    /// Runs a dunning cycle: processes all due payments and sends escalation emails.
    /// </summary>
    public async Task<DunningCycleResultDto> RunDunningCycleAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scheduled dunning cycle at {Time}", DateTime.UtcNow);

        var result = await _dunningManager.ProcessDunningAsync(cancellationToken);

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

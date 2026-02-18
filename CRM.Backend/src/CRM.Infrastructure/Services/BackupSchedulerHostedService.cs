// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service that runs scheduled database backups
/// </summary>
public class BackupSchedulerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupSchedulerHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public BackupSchedulerHostedService(
        IServiceProvider serviceProvider,
        ILogger<BackupSchedulerHostedService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Backup Scheduler Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedulesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in backup scheduler service");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Backup Scheduler Service stopping");
    }

    private async Task ProcessDueSchedulesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ICrmDbContext>();
        var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();

        var now = DateTime.UtcNow;

        // Find enabled schedules that are due
        var dueSchedules = await context.BackupSchedules
            .Where(s => s.IsEnabled && !s.IsDeleted && s.NextBackupAt != null && s.NextBackupAt <= now)
            .ToListAsync(stoppingToken);

        foreach (var schedule in dueSchedules)
        {
            if (stoppingToken.IsCancellationRequested) break;

            _logger.LogInformation("Running due scheduled backup: {ScheduleName} (ID: {ScheduleId})", schedule.Name, schedule.Id);

            try
            {
                await backupService.RunScheduledBackupAsync(schedule.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run scheduled backup: {ScheduleName}", schedule.Name);
            }
        }
    }
}

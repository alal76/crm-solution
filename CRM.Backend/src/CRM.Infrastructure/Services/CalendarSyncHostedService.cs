// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Background service to periodically sync all due calendar integrations.
/// Part of Marketing &amp; Sales gap analysis implementation (G4).
/// </summary>
public class CalendarSyncHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CalendarSyncHostedService> _logger;

    public CalendarSyncHostedService(
        IServiceProvider serviceProvider,
        ILogger<CalendarSyncHostedService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CalendarSyncHostedService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ICalendarSyncService>();

                await syncService.SyncAllDueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running calendar sync job");
            }

            // Default interval: 5 minutes (individual integrations can have their own NextSyncAt)
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // ignore cancellation
            }
        }

        _logger.LogInformation("CalendarSyncHostedService stopped");
    }
}

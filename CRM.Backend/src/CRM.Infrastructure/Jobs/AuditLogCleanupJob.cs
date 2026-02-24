// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Jobs;

/// <summary>
/// Background job for periodic audit log cleanup and archival.
/// Runs on a configurable schedule to maintain audit log table performance.
/// TODO-SYS006-007
/// </summary>
public class AuditLogCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupJob> _logger;
    private readonly AuditLogCleanupOptions _options;

    public AuditLogCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<AuditLogCleanupJob> logger,
        IOptions<AuditLogCleanupOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Audit log cleanup job is disabled");
            return;
        }

        _logger.LogInformation(
            "Audit log cleanup job started. Schedule: every {Hours} hours, Archive after {ArchiveDays} days, Purge after {PurgeDays} days",
            _options.IntervalHours, _options.ArchiveAfterDays, _options.PurgeAfterDays);

        // Initial delay to allow system to stabilize
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during audit log cleanup job execution");
            }

            // Wait for next execution
            await Task.Delay(TimeSpan.FromHours(_options.IntervalHours), stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting audit log cleanup cycle");

        using var scope = _serviceProvider.CreateScope();
        var retentionService = scope.ServiceProvider.GetRequiredService<IAuditRetentionService>();

        // Step 1: Archive old logs
        if (_options.ArchiveAfterDays > 0)
        {
            _logger.LogDebug("Archiving audit logs older than {Days} days", _options.ArchiveAfterDays);
            var archiveResult = await retentionService.ArchiveOldLogsAsync(
                _options.ArchiveAfterDays, cancellationToken);

            if (archiveResult.Success)
            {
                _logger.LogInformation(
                    "Archive completed: {Archived} logs archived out of {Total} found",
                    archiveResult.LogsArchived, archiveResult.TotalLogsFound);
            }
            else
            {
                _logger.LogWarning("Archive operation failed: {Error}", archiveResult.ErrorMessage);
            }
        }

        // Step 2: Purge archived logs
        if (_options.PurgeAfterDays > 0)
        {
            _logger.LogDebug("Purging archived audit logs older than {Days} days", _options.PurgeAfterDays);
            var purgeResult = await retentionService.PurgeArchivedLogsAsync(
                _options.PurgeAfterDays, cancellationToken);

            if (purgeResult.Success)
            {
                _logger.LogInformation(
                    "Purge completed: {Purged} logs purged out of {Total} found",
                    purgeResult.LogsPurged, purgeResult.TotalLogsFound);
            }
            else
            {
                _logger.LogWarning("Purge operation failed: {Error}", purgeResult.ErrorMessage);
            }
        }

        // Log retention stats
        var stats = await retentionService.GetRetentionStatsAsync(cancellationToken);
        _logger.LogInformation(
            "Audit log stats - Total: {Total}, Active: {Active}, Archived: {Archived}, Last 30 days: {Recent}",
            stats.TotalLogs, stats.ActiveLogs, stats.ArchivedLogs, stats.LogsLast30Days);
    }
}

/// <summary>
/// Configuration options for the audit log cleanup job.
/// </summary>
public class AuditLogCleanupOptions
{
    public const string SectionName = "AuditLogCleanup";

    /// <summary>
    /// Whether the cleanup job is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// How often to run the cleanup job in hours.
    /// </summary>
    public int IntervalHours { get; set; } = 24;

    /// <summary>
    /// Archive audit logs older than this many days.
    /// Set to 0 to disable archiving.
    /// </summary>
    public int ArchiveAfterDays { get; set; } = 90;

    /// <summary>
    /// Purge archived logs older than this many days.
    /// Set to 0 to disable purging.
    /// </summary>
    public int PurgeAfterDays { get; set; } = 365;
}

/// <summary>
/// Extension methods for registering the audit log cleanup job.
/// </summary>
public static class AuditLogCleanupJobExtensions
{
    /// <summary>
    /// Adds the audit log cleanup background job to the service collection.
    /// </summary>
    public static IServiceCollection AddAuditLogCleanupJob(
        this IServiceCollection services,
        Action<AuditLogCleanupOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<AuditLogCleanupOptions>(_ => { });
        }

        services.AddHostedService<AuditLogCleanupJob>();

        return services;
    }
}

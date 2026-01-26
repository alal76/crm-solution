/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration options for ZIP code import scheduling
/// </summary>
public class ZipCodeImportOptions
{
    public const string SectionName = "ZipCodeImport";
    
    /// <summary>
    /// Enable automatic scheduled imports
    /// </summary>
    public bool EnableScheduledImport { get; set; } = false;
    
    /// <summary>
    /// Cron expression for scheduled imports (e.g., "0 0 1 * *" for monthly)
    /// </summary>
    public string CronExpression { get; set; } = "0 0 1 * *"; // Monthly at midnight on the 1st
    
    /// <summary>
    /// Import source: "GeoNames", "GitHub", or custom URL
    /// </summary>
    public string ImportSource { get; set; } = "GeoNames";
    
    /// <summary>
    /// Custom GitHub URL for ZIP code data
    /// </summary>
    public string? GitHubUrl { get; set; }
    
    /// <summary>
    /// List of country codes to import (empty = all countries)
    /// </summary>
    public List<string> CountryCodes { get; set; } = new() { "US" };
    
    /// <summary>
    /// Auto-import on startup if table is empty
    /// </summary>
    public bool ImportOnStartupIfEmpty { get; set; } = true;
    
    /// <summary>
    /// Minimum hours between imports
    /// </summary>
    public int MinimumHoursBetweenImports { get; set; } = 168; // 1 week
}

/// <summary>
/// Background service for scheduled ZIP code imports from GeoNames/GitHub
/// </summary>
public class ZipCodeImportHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZipCodeImportHostedService> _logger;
    private readonly ZipCodeImportOptions _options;
    private DateTime? _lastImportTime;
    private bool _initialImportDone = false;

    public ZipCodeImportHostedService(
        IServiceProvider serviceProvider,
        ILogger<ZipCodeImportHostedService> logger,
        IOptions<ZipCodeImportOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZIP Code Import Service starting...");
        
        // Initial delay to let the application start up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        
        // Check if we should import on startup
        if (_options.ImportOnStartupIfEmpty && !_initialImportDone)
        {
            await CheckAndImportIfEmptyAsync(stoppingToken);
            _initialImportDone = true;
        }
        
        // Main scheduling loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_options.EnableScheduledImport)
                {
                    await CheckAndRunScheduledImportAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ZIP code import scheduler");
            }

            // Check every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("ZIP Code Import Service stopping...");
    }

    private async Task CheckAndImportIfEmptyAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CRM.Infrastructure.Data.CrmDbContext>();
        
        var count = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .CountAsync(context.ZipCodes, cancellationToken);

        if (count == 0)
        {
            _logger.LogInformation("ZipCodes table is empty. Starting automatic import...");
            await RunImportAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("ZipCodes table has {Count:N0} records. Skipping startup import.", count);
        }
    }

    private async Task CheckAndRunScheduledImportAsync(CancellationToken cancellationToken)
    {
        // Simple scheduling: check if enough time has passed since last import
        if (_lastImportTime.HasValue)
        {
            var hoursSinceLastImport = (DateTime.UtcNow - _lastImportTime.Value).TotalHours;
            if (hoursSinceLastImport < _options.MinimumHoursBetweenImports)
            {
                return;
            }
        }

        // Check if current time matches schedule (simplified cron check)
        if (ShouldRunNow())
        {
            _logger.LogInformation("Running scheduled ZIP code import...");
            await RunImportAsync(cancellationToken);
        }
    }

    private bool ShouldRunNow()
    {
        // Simple cron-like check for monthly on the 1st at midnight
        var now = DateTime.UtcNow;
        
        // Parse simple cron expression (minute hour dayOfMonth month dayOfWeek)
        var parts = _options.CronExpression.Split(' ');
        if (parts.Length < 5) return false;

        // Check day of month
        if (parts[2] != "*" && !parts[2].Split(',').Contains(now.Day.ToString()))
            return false;

        // Check hour
        if (parts[1] != "*" && !parts[1].Split(',').Contains(now.Hour.ToString()))
            return false;

        // Check minute (within a 2-hour window since we check every hour)
        if (parts[0] != "*")
        {
            if (int.TryParse(parts[0], out var scheduledMinute))
            {
                if (Math.Abs(now.Minute - scheduledMinute) > 30)
                    return false;
            }
        }

        return true;
    }

    private async Task RunImportAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IZipCodeImportService>();

        ZipCodeImportResult result;

        if (_options.ImportSource.Equals("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            result = await importService.ImportFromGitHubAsync(_options.GitHubUrl, cancellationToken);
        }
        else if (_options.CountryCodes.Count == 1)
        {
            // Import single country
            result = await importService.ImportCountryFromGeoNamesAsync(_options.CountryCodes[0], cancellationToken);
        }
        else if (_options.CountryCodes.Count > 1)
        {
            // Import multiple countries
            var totalImported = 0;
            var totalSkipped = 0;
            var errors = new List<string>();
            
            foreach (var countryCode in _options.CountryCodes)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                var countryResult = await importService.ImportCountryFromGeoNamesAsync(countryCode, cancellationToken);
                totalImported += countryResult.RecordsImported;
                totalSkipped += countryResult.RecordsSkipped;
                
                if (!countryResult.Success && !string.IsNullOrEmpty(countryResult.ErrorMessage))
                {
                    errors.Add($"{countryCode}: {countryResult.ErrorMessage}");
                }
            }
            
            result = new ZipCodeImportResult
            {
                Success = errors.Count == 0,
                RecordsImported = totalImported,
                RecordsSkipped = totalSkipped,
                ErrorMessage = errors.Count > 0 ? string.Join("; ", errors) : null,
                Source = $"GeoNames ({string.Join(", ", _options.CountryCodes)})"
            };
        }
        else
        {
            // Import all countries
            result = await importService.ImportFromGeoNamesAsync(cancellationToken);
        }

        _lastImportTime = DateTime.UtcNow;

        if (result.Success)
        {
            _logger.LogInformation("Scheduled import completed: {Imported:N0} imported, {Skipped:N0} skipped",
                result.RecordsImported, result.RecordsSkipped);
        }
        else
        {
            _logger.LogError("Scheduled import failed: {Error}", result.ErrorMessage);
        }
    }
}

/// <summary>
/// Background job for one-time ZIP code import triggered via workflow or API
/// </summary>
public class ZipCodeImportJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZipCodeImportJob> _logger;

    public ZipCodeImportJob(
        IServiceProvider serviceProvider,
        ILogger<ZipCodeImportJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Execute the import job
    /// </summary>
    public async Task<ZipCodeImportResult> ExecuteAsync(
        string source = "GeoNames",
        string? countryCode = null,
        string? gitHubUrl = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IZipCodeImportService>();

        _logger.LogInformation("Executing ZIP code import job. Source: {Source}, Country: {Country}", 
            source, countryCode ?? "All");

        if (source.Equals("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            return await importService.ImportFromGitHubAsync(gitHubUrl, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(countryCode))
        {
            return await importService.ImportCountryFromGeoNamesAsync(countryCode, cancellationToken);
        }
        else
        {
            return await importService.ImportFromGeoNamesAsync(cancellationToken);
        }
    }
}

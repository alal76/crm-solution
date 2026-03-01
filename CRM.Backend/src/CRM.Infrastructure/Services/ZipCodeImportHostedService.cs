// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

// ---------------------------------------------------------------------------
// On-demand import queue — lets the controller fire-and-forget without waiting
// for a potentially 30-minute GeoNames download.
// ---------------------------------------------------------------------------

/// <summary>
/// Represents an on-demand ZIP code import request queued by the API layer.
/// </summary>
public record ZipCodeImportRequest(
    /// <summary>"GeoNames", "GeoNames-Country", "GitHub", or "CsvUrl"</summary>
    string Source,
    /// <summary>ISO 2-letter country code – only used for Source=GeoNames-Country</summary>
    string? CountryCode = null,
    /// <summary>URL for GitHub JSON or CSV download sources</summary>
    string? Url = null,
    /// <summary>Friendly name for logging/status</summary>
    string? RequestedBy = null);

/// <summary>
/// Singleton queue for on-demand import requests. The background worker drains
/// this queue between its schedule ticks.
/// </summary>
public interface IZipCodeImportQueue
{
    /// <summary>Enqueue an import request. Returns false if the queue is full.</summary>
    bool TryEnqueue(ZipCodeImportRequest request);

    /// <summary>True if there is at least one pending request.</summary>
    bool HasPendingRequests { get; }

    /// <summary>Read the next request (resolves when one is available or token is cancelled).</summary>
    ValueTask<ZipCodeImportRequest> DequeueAsync(CancellationToken cancellationToken);

    /// <summary>Wait until at least one request is available without consuming it. Returns true if an item is ready.</summary>
    Task<bool> WaitToReadAsync(CancellationToken cancellationToken);
}

/// <summary>Default implementation backed by a bounded <see cref="Channel{T}"/>.</summary>
public sealed class ZipCodeImportQueue : IZipCodeImportQueue
{
    // Capacity=10 — more than enough; concurrent imports are guarded by _isRunning.
    private readonly Channel<ZipCodeImportRequest> _channel =
        Channel.CreateBounded<ZipCodeImportRequest>(new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
        });

    public bool TryEnqueue(ZipCodeImportRequest request) =>
        _channel.Writer.TryWrite(request);

    public bool HasPendingRequests => _channel.Reader.Count > 0;

    public ValueTask<ZipCodeImportRequest> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);

    public Task<bool> WaitToReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.WaitToReadAsync(cancellationToken).AsTask();
}

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
    /// Import source for scheduled/startup imports: "GeoNames", "GitHub", or "CsvUrl"
    /// </summary>
    public string ImportSource { get; set; } = "GeoNames";

    /// <summary>
    /// Custom GitHub URL for ZIP code data (used when ImportSource = "GitHub")
    /// </summary>
    public string? GitHubUrl { get; set; }

    /// <summary>
    /// Direct HTTP(S) URL to a CSV file to download (used when ImportSource = "CsvUrl").
    /// The file must be a directly accessible URL (no authentication redirects).
    /// Supports the Zeeshanahmad4 CSV format and any header-based CSV/TSV.
    /// </summary>
    public string? CsvDownloadUrl { get; set; }

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
/// Background service for scheduled and on-demand ZIP code imports.
/// Drains <see cref="IZipCodeImportQueue"/> for API-triggered requests and also
/// runs a cron-based scheduled import when <see cref="ZipCodeImportOptions.EnableScheduledImport"/> is true.
/// </summary>
public class ZipCodeImportHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZipCodeImportHostedService> _logger;
    private readonly ZipCodeImportOptions _options;
    private readonly IZipCodeImportQueue _queue;
    private DateTime? _lastImportTime;
    private bool _initialImportDone = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipCodeImportHostedService"/> class.
    /// </summary>
    public ZipCodeImportHostedService(
        IServiceProvider serviceProvider,
        ILogger<ZipCodeImportHostedService> logger,
        IOptions<ZipCodeImportOptions> options,
        IZipCodeImportQueue queue)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ZIP Code Import Service starting...");

        // Initial delay to let the application start up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        // Check if we should import on startup
        if (_options.ImportOnStartupIfEmpty && !_initialImportDone)
        {
            try
            {
                await CheckAndImportIfEmptyAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during startup ZIP code import. Service will continue to run. Scheduled imports may retry later.");
            }
            _initialImportDone = true;
        }

        // Main loop — drain on-demand queue and run scheduled imports
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // --- On-demand queue: process all pending requests first ---
                while (_queue.HasPendingRequests && !stoppingToken.IsCancellationRequested)
                {
                    ZipCodeImportRequest request;
                    try
                    {
                        using var shortCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        shortCts.CancelAfter(TimeSpan.FromSeconds(5));
                        request = await _queue.DequeueAsync(shortCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    _logger.LogInformation(
                        "Processing on-demand ZIP import (source={Source}, country={Country}, requestedBy={By})",
                        request.Source, request.CountryCode ?? "all", request.RequestedBy ?? "admin");
                    try
                    {
                        await RunImportFromRequestAsync(request, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "On-demand ZIP import failed (source={Source})", request.Source);
                    }
                }

                // --- Scheduled import ---
                if (_options.EnableScheduledImport)
                {
                    await CheckAndRunScheduledImportAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ZIP code import worker loop");
            }

            // Wait up to 60 s, but wake immediately when a request is queued
            try
            {
                using var wakeCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                wakeCts.CancelAfter(TimeSpan.FromMinutes(1));
                await _queue.WaitToReadAsync(wakeCts.Token);
                // Item is available — loop will pick it up at the top of the while.
            }
            catch (OperationCanceledException)
            {
                // Timeout or service stopping — both are normal
            }
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
        if (parts.Length < 5)
            return false;

        // Check day of month
        if (parts[2] != "*" && !parts[2].Split(',').Contains(now.Day.ToString()))
            return false;

        // Check hour
        if (parts[1] != "*" && !parts[1].Split(',').Contains(now.Hour.ToString()))
            return false;

        // Check minute (within a 2-hour window since we check every hour)
        if (parts[0] != "*"
            && int.TryParse(parts[0], out var scheduledMinute)
            && Math.Abs(now.Minute - scheduledMinute) > 30)
        {
            return false;
        }

        return true;
    }

    // Runs the scheduled import using options-configured source/countries.
    private Task RunImportAsync(CancellationToken cancellationToken)
    {
        var request = _options.ImportSource.Equals("CsvUrl", StringComparison.OrdinalIgnoreCase)
            ? new ZipCodeImportRequest("CsvUrl", Url: _options.CsvDownloadUrl, RequestedBy: "scheduler")
            : _options.ImportSource.Equals("GitHub", StringComparison.OrdinalIgnoreCase)
                ? new ZipCodeImportRequest("GitHub", Url: _options.GitHubUrl, RequestedBy: "scheduler")
                : _options.CountryCodes.Count == 1
                    ? new ZipCodeImportRequest("GeoNames-Country", CountryCode: _options.CountryCodes[0], RequestedBy: "scheduler")
                    : _options.CountryCodes.Count > 1
                        ? new ZipCodeImportRequest("GeoNames-Multi", RequestedBy: "scheduler")
                        : new ZipCodeImportRequest("GeoNames", RequestedBy: "scheduler");

        return RunImportFromRequestAsync(request, cancellationToken);
    }

    // Core dispatcher — resolves the right import method for any request.
    private async Task RunImportFromRequestAsync(ZipCodeImportRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IZipCodeImportService>();

        ZipCodeImportResult result;

        switch (request.Source.ToUpperInvariant())
        {
            case "GITHUB":
                result = await importService.ImportFromGitHubAsync(request.Url, cancellationToken);
                break;

            case "CSVURL":
                result = await DownloadAndImportCsvAsync(importService, request.Url, cancellationToken);
                break;

            case "GEONAMES-COUNTRY":
                if (string.IsNullOrEmpty(request.CountryCode))
                    goto default;
                result = await importService.ImportCountryFromGeoNamesAsync(request.CountryCode, cancellationToken);
                break;

            case "GEONAMES-MULTI":
                // Multi-country scheduled import driven by options.CountryCodes
                var totalImported = 0;
                var totalSkipped = 0;
                var errors = new List<string>();
                foreach (var cc in _options.CountryCodes)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var cr = await importService.ImportCountryFromGeoNamesAsync(cc, cancellationToken);
                    totalImported += cr.RecordsImported;
                    totalSkipped  += cr.RecordsSkipped;
                    if (!cr.Success && !string.IsNullOrEmpty(cr.ErrorMessage))
                        errors.Add($"{cc}: {cr.ErrorMessage}");
                }
                result = new ZipCodeImportResult
                {
                    Success          = errors.Count == 0,
                    RecordsImported  = totalImported,
                    RecordsSkipped   = totalSkipped,
                    ErrorMessage     = errors.Count > 0 ? string.Join("; ", errors) : null,
                    Source           = $"GeoNames ({string.Join(", ", _options.CountryCodes)})"
                };
                break;

            default:
                result = await importService.ImportFromGeoNamesAsync(cancellationToken);
                break;
        }

        _lastImportTime = DateTime.UtcNow;

        if (result.Success)
        {
            _logger.LogInformation(
                "ZIP import completed (source={Source}): {Imported:N0} imported, {Skipped:N0} skipped",
                request.Source, result.RecordsImported, result.RecordsSkipped);
        }
        else
        {
            _logger.LogError("ZIP import failed (source={Source}): {Error}", request.Source, result.ErrorMessage);
        }
    }

    // Downloads a CSV file from an arbitrary URL and pipes it into ImportFromCsvStreamAsync.
    private async Task<ZipCodeImportResult> DownloadAndImportCsvAsync(
        IZipCodeImportService importService,
        string? url,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = "CsvDownloadUrl is not configured. Set ZipCodeImport:CsvDownloadUrl in appsettings."
            };
        }

        _logger.LogInformation("Downloading ZIP CSV from {Url}", url);

        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromMinutes(30);
        http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("CRM-ZipImporter", "1.0"));

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Derive a friendly source name from the URL filename
        var fileName = System.IO.Path.GetFileName(new Uri(url).AbsolutePath);
        var sourceName = $"CSV auto-download ({fileName})";

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await importService.ImportFromCsvStreamAsync(stream, sourceName, cancellationToken);
    }
}

/// <summary>
/// Background job for one-time ZIP code import triggered via workflow or API
/// </summary>
public class ZipCodeImportJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZipCodeImportJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipCodeImportJob"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger instance.</param>
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

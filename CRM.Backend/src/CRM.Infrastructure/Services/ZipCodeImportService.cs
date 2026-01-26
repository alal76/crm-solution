/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.IO.Compression;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for importing ZIP code data from GeoNames and GitHub
/// </summary>
public interface IZipCodeImportService
{
    /// <summary>
    /// Import ZIP codes from GeoNames all countries file
    /// </summary>
    Task<ZipCodeImportResult> ImportFromGeoNamesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Import ZIP codes for a specific country from GeoNames
    /// </summary>
    Task<ZipCodeImportResult> ImportCountryFromGeoNamesAsync(string countryCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Import ZIP codes from a GitHub repository
    /// </summary>
    Task<ZipCodeImportResult> ImportFromGitHubAsync(string? repositoryUrl = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get import status and statistics
    /// </summary>
    Task<ZipCodeImportStatus> GetImportStatusAsync();
    
    /// <summary>
    /// Check if an import is currently running
    /// </summary>
    bool IsImportRunning { get; }
}

public class ZipCodeImportResult
{
    public bool Success { get; set; }
    public int RecordsImported { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
}

public class ZipCodeImportStatus
{
    public bool IsRunning { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public double ProgressPercent => TotalRecords > 0 ? (ProcessedRecords * 100.0 / TotalRecords) : 0;
    public DateTime? LastImportAt { get; set; }
    public ZipCodeImportResult? LastResult { get; set; }
    public string? CurrentSource { get; set; }
    public string? CurrentCountry { get; set; }
}

public class ZipCodeImportService : IZipCodeImportService
{
    private readonly CrmDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ZipCodeImportService> _logger;
    
    // GeoNames URLs
    private const string GEONAMES_ALL_COUNTRIES_URL = "https://download.geonames.org/export/zip/allCountries.zip";
    private const string GEONAMES_COUNTRY_URL_TEMPLATE = "https://download.geonames.org/export/zip/{0}.zip";
    
    // Default GitHub URL for CRM ZIP code data
    private const string DEFAULT_GITHUB_URL = "https://raw.githubusercontent.com/abhilal/crm-zipcode-data/main/data/zipcodes.json";
    
    // Import state
    private bool _isRunning = false;
    private int _totalRecords = 0;
    private int _processedRecords = 0;
    private string? _currentSource;
    private string? _currentCountry;
    private ZipCodeImportResult? _lastResult;
    
    // Country code to name mapping
    private static readonly Dictionary<string, string> CountryNames = new()
    {
        { "US", "United States" }, { "CA", "Canada" }, { "GB", "United Kingdom" },
        { "AU", "Australia" }, { "DE", "Germany" }, { "FR", "France" }, { "IT", "Italy" },
        { "ES", "Spain" }, { "NL", "Netherlands" }, { "BE", "Belgium" }, { "CH", "Switzerland" },
        { "AT", "Austria" }, { "IE", "Ireland" }, { "NZ", "New Zealand" }, { "MX", "Mexico" },
        { "BR", "Brazil" }, { "AR", "Argentina" }, { "IN", "India" }, { "JP", "Japan" },
        { "CN", "China" }, { "KR", "South Korea" }, { "SG", "Singapore" }, { "MY", "Malaysia" },
        { "PH", "Philippines" }, { "TH", "Thailand" }, { "VN", "Vietnam" }, { "ID", "Indonesia" },
        { "ZA", "South Africa" }, { "NG", "Nigeria" }, { "EG", "Egypt" }, { "AE", "United Arab Emirates" },
        { "SA", "Saudi Arabia" }, { "IL", "Israel" }, { "TR", "Turkey" }, { "RU", "Russia" },
        { "PL", "Poland" }, { "CZ", "Czech Republic" }, { "HU", "Hungary" }, { "RO", "Romania" },
        { "SE", "Sweden" }, { "NO", "Norway" }, { "DK", "Denmark" }, { "FI", "Finland" },
        { "PT", "Portugal" }, { "GR", "Greece" }, { "HR", "Croatia" }, { "SI", "Slovenia" },
        { "SK", "Slovakia" }, { "BG", "Bulgaria" }, { "LT", "Lithuania" }, { "LV", "Latvia" },
        { "EE", "Estonia" }, { "CY", "Cyprus" }, { "MT", "Malta" }, { "LU", "Luxembourg" }
    };

    public ZipCodeImportService(
        CrmDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<ZipCodeImportService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsImportRunning => _isRunning;

    public async Task<ZipCodeImportStatus> GetImportStatusAsync()
    {
        return await Task.FromResult(new ZipCodeImportStatus
        {
            IsRunning = _isRunning,
            TotalRecords = _totalRecords,
            ProcessedRecords = _processedRecords,
            CurrentSource = _currentSource,
            CurrentCountry = _currentCountry,
            LastResult = _lastResult,
            LastImportAt = _lastResult?.CompletedAt
        });
    }

    public async Task<ZipCodeImportResult> ImportFromGeoNamesAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = "An import is already in progress"
            };
        }

        var startTime = DateTime.UtcNow;
        _isRunning = true;
        _currentSource = "GeoNames (All Countries)";
        _totalRecords = 0;
        _processedRecords = 0;

        try
        {
            _logger.LogInformation("Starting ZIP code import from GeoNames (all countries)...");

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(30);
            
            // Download the ZIP file
            _logger.LogInformation("Downloading {Url}...", GEONAMES_ALL_COUNTRIES_URL);
            using var response = await client.GetAsync(GEONAMES_ALL_COUNTRIES_URL, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var records = await ParseGeoNamesZipAsync(stream, cancellationToken);

            _totalRecords = records.Count;
            _logger.LogInformation("Downloaded {Count:N0} ZIP code records", _totalRecords);

            var result = await ImportRecordsAsync(records, cancellationToken);
            result.Source = "GeoNames (allCountries.zip)";
            result.Duration = DateTime.UtcNow - startTime;
            
            _lastResult = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing ZIP codes from GeoNames");
            _lastResult = new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime,
                Source = "GeoNames (allCountries.zip)"
            };
            return _lastResult;
        }
        finally
        {
            _isRunning = false;
            _currentSource = null;
            _currentCountry = null;
        }
    }

    public async Task<ZipCodeImportResult> ImportCountryFromGeoNamesAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = "An import is already in progress"
            };
        }

        var startTime = DateTime.UtcNow;
        _isRunning = true;
        _currentSource = $"GeoNames ({countryCode})";
        _currentCountry = countryCode.ToUpperInvariant();
        _totalRecords = 0;
        _processedRecords = 0;

        try
        {
            var url = string.Format(GEONAMES_COUNTRY_URL_TEMPLATE, countryCode.ToUpperInvariant());
            _logger.LogInformation("Starting ZIP code import from GeoNames for {Country}...", countryCode);

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            
            _logger.LogInformation("Downloading {Url}...", url);
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var records = await ParseGeoNamesZipAsync(stream, cancellationToken);

            _totalRecords = records.Count;
            _logger.LogInformation("Downloaded {Count:N0} ZIP code records for {Country}", _totalRecords, countryCode);

            var result = await ImportRecordsAsync(records, cancellationToken);
            result.Source = $"GeoNames ({countryCode}.zip)";
            result.Duration = DateTime.UtcNow - startTime;
            
            _lastResult = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing ZIP codes from GeoNames for {Country}", countryCode);
            _lastResult = new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime,
                Source = $"GeoNames ({countryCode}.zip)"
            };
            return _lastResult;
        }
        finally
        {
            _isRunning = false;
            _currentSource = null;
            _currentCountry = null;
        }
    }

    public async Task<ZipCodeImportResult> ImportFromGitHubAsync(string? repositoryUrl = null, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = "An import is already in progress"
            };
        }

        var startTime = DateTime.UtcNow;
        _isRunning = true;
        var url = repositoryUrl ?? DEFAULT_GITHUB_URL;
        _currentSource = $"GitHub ({url})";
        _totalRecords = 0;
        _processedRecords = 0;

        try
        {
            _logger.LogInformation("Starting ZIP code import from GitHub: {Url}", url);

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var records = JsonSerializer.Deserialize<List<GitHubZipCodeRecord>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (records == null || records.Count == 0)
            {
                return new ZipCodeImportResult
                {
                    Success = false,
                    ErrorMessage = "No records found in GitHub response",
                    Source = $"GitHub ({url})"
                };
            }

            _totalRecords = records.Count;
            _logger.LogInformation("Downloaded {Count:N0} ZIP code records from GitHub", _totalRecords);

            // Convert to ZipCode entities
            var zipCodes = records.Select(r => new ZipCode
            {
                Country = r.Country ?? GetCountryName(r.CountryCode),
                CountryCode = r.CountryCode ?? "US",
                PostalCode = r.PostalCode ?? r.ZipCode ?? "",
                City = r.City ?? r.PlaceName ?? "",
                State = r.State ?? r.AdminName1,
                StateCode = r.StateCode ?? r.AdminCode1,
                County = r.County ?? r.AdminName2,
                CountyCode = r.CountyCode ?? r.AdminCode2,
                Community = r.Community ?? r.AdminName3,
                CommunityCode = r.CommunityCode ?? r.AdminCode3,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Accuracy = r.Accuracy ?? 4,
                IsActive = true
            }).Where(z => !string.IsNullOrEmpty(z.PostalCode) && !string.IsNullOrEmpty(z.City)).ToList();

            var result = await ImportRecordsAsync(zipCodes, cancellationToken);
            result.Source = $"GitHub ({url})";
            result.Duration = DateTime.UtcNow - startTime;
            
            _lastResult = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing ZIP codes from GitHub");
            _lastResult = new ZipCodeImportResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = DateTime.UtcNow - startTime,
                Source = $"GitHub ({url})"
            };
            return _lastResult;
        }
        finally
        {
            _isRunning = false;
            _currentSource = null;
        }
    }

    private async Task<List<ZipCode>> ParseGeoNamesZipAsync(Stream zipStream, CancellationToken cancellationToken)
    {
        var records = new List<ZipCode>();
        
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            if (!entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                continue;

            using var reader = new StreamReader(entry.Open());
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                var parts = line.Split('\t');
                if (parts.Length < 10) continue;

                var countryCode = parts[0];
                var postalCode = parts[1];
                var city = parts[2];

                if (string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(city))
                    continue;

                decimal.TryParse(parts.Length > 9 ? parts[9] : "0", out var latitude);
                decimal.TryParse(parts.Length > 10 ? parts[10] : "0", out var longitude);
                int.TryParse(parts.Length > 11 ? parts[11] : "1", out var accuracy);

                records.Add(new ZipCode
                {
                    Country = GetCountryName(countryCode),
                    CountryCode = countryCode,
                    PostalCode = postalCode,
                    City = city,
                    State = parts.Length > 3 ? parts[3] : null,
                    StateCode = parts.Length > 4 ? parts[4] : null,
                    County = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null,
                    CountyCode = parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6] : null,
                    Community = parts.Length > 7 && !string.IsNullOrWhiteSpace(parts[7]) ? parts[7] : null,
                    CommunityCode = parts.Length > 8 && !string.IsNullOrWhiteSpace(parts[8]) ? parts[8] : null,
                    Latitude = latitude,
                    Longitude = longitude,
                    Accuracy = accuracy > 0 ? accuracy : 1,
                    IsActive = true
                });
            }
        }

        return records;
    }

    private async Task<ZipCodeImportResult> ImportRecordsAsync(List<ZipCode> records, CancellationToken cancellationToken)
    {
        var result = new ZipCodeImportResult { Success = true };
        const int batchSize = 5000;
        
        try
        {
            // Get existing postal codes for deduplication
            _logger.LogInformation("Loading existing ZIP codes for deduplication...");
            var existingCodes = await _context.ZipCodes
                .Select(z => new { z.CountryCode, z.PostalCode })
                .ToListAsync(cancellationToken);
            
            var existingSet = existingCodes
                .Select(x => $"{x.CountryCode}:{x.PostalCode}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            _logger.LogInformation("Found {Count:N0} existing ZIP codes", existingSet.Count);

            // Filter to only new records
            var newRecords = records
                .Where(r => !existingSet.Contains($"{r.CountryCode}:{r.PostalCode}"))
                .ToList();

            result.RecordsSkipped = records.Count - newRecords.Count;
            _logger.LogInformation("Importing {New:N0} new records, skipping {Skip:N0} existing", 
                newRecords.Count, result.RecordsSkipped);

            // Batch insert
            for (int i = 0; i < newRecords.Count; i += batchSize)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var batch = newRecords.Skip(i).Take(batchSize).ToList();
                _context.ZipCodes.AddRange(batch);
                await _context.SaveChangesAsync(cancellationToken);

                _processedRecords = i + batch.Count;
                result.RecordsImported += batch.Count;

                _currentCountry = batch.LastOrDefault()?.CountryCode;
                
                if ((i / batchSize) % 10 == 0)
                {
                    _logger.LogInformation("Import progress: {Processed:N0}/{Total:N0} ({Percent:F1}%)", 
                        _processedRecords, newRecords.Count, 
                        newRecords.Count > 0 ? (_processedRecords * 100.0 / newRecords.Count) : 100);
                }
            }

            // Update system settings with last import timestamp
            var settings = await _context.SystemSettings.FirstOrDefaultAsync(cancellationToken);
            if (settings != null)
            {
                settings.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("ZIP code import completed: {Imported:N0} imported, {Skipped:N0} skipped, {Failed:N0} failed",
                result.RecordsImported, result.RecordsSkipped, result.RecordsFailed);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch import");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private static string GetCountryName(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode)) return "Unknown";
        return CountryNames.TryGetValue(countryCode.ToUpperInvariant(), out var name) 
            ? name 
            : countryCode;
    }

    // DTO for GitHub JSON format
    private class GitHubZipCodeRecord
    {
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? PostalCode { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? PlaceName { get; set; }
        public string? State { get; set; }
        public string? StateCode { get; set; }
        public string? AdminName1 { get; set; }
        public string? AdminCode1 { get; set; }
        public string? County { get; set; }
        public string? CountyCode { get; set; }
        public string? AdminName2 { get; set; }
        public string? AdminCode2 { get; set; }
        public string? Community { get; set; }
        public string? CommunityCode { get; set; }
        public string? AdminName3 { get; set; }
        public string? AdminCode3 { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Accuracy { get; set; }
    }
}

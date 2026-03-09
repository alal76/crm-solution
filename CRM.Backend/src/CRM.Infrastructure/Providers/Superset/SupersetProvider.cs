// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Providers.Superset;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Apache Superset analytics provider implementation.
/// Implements IAnalyticsPort for embedded dashboards and charts.
/// </summary>
public class SupersetProvider : IAnalyticsPort
{
    private readonly HttpClient _httpClient;
    private readonly SupersetConfiguration _config;
    private readonly ILogger<SupersetProvider> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public SupersetProvider(
        HttpClient httpClient,
        IOptions<SupersetConfiguration> config,
        ILogger<SupersetProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient.BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    /// <inheritdoc />
    public string ProviderName => "Superset";

    /// <inheritdoc />
    public bool SupportsEmbedding => true;

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Superset provider is not available");
            return false;
        }
    }

    #region Authentication

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt)
        {
            return;
        }

        await AuthenticateAsync(cancellationToken);
    }

    private async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Authenticating with Superset API");

        // Superset v3+ requires direct login (CSRF token endpoint itself requires auth).
        // We authenticate directly with credentials, then fetch a CSRF token afterwards
        // for state-changing requests if needed.
        var loginRequest = new SupersetLoginRequest
        {
            Username = _config.Username,
            Password = _config.Password,
            Provider = _config.Provider,
            Refresh = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/security/login");
        request.Content = new StringContent(
            JsonSerializer.Serialize(loginRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Superset authentication failed: {error}");
        }

        var loginResult = await response.Content.ReadFromJsonAsync<SupersetLoginResponse>(JsonOptions, cancellationToken);

        if (string.IsNullOrEmpty(loginResult?.AccessToken))
        {
            throw new InvalidOperationException("Superset authentication failed: No access token returned");
        }

        _accessToken = loginResult.AccessToken;
        _tokenExpiresAt = DateTime.UtcNow.AddMinutes(_config.TokenRefreshIntervalMinutes);

        // Set default auth header for subsequent requests
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        _logger.LogDebug("Successfully authenticated with Superset");
    }

    #endregion

    #region Dashboard Operations

    /// <inheritdoc />
    public async Task<IEnumerable<DashboardInfo>> GetDashboardsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync("api/v1/dashboard/", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SupersetListResponse<SupersetDashboard>>(JsonOptions, cancellationToken);

        return result?.Result?.Select(MapToDashboardInfo) ?? Enumerable.Empty<DashboardInfo>();
    }

    /// <inheritdoc />
    public async Task<DashboardInfo?> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        if (!int.TryParse(dashboardId, out var id))
        {
            _logger.LogWarning("Invalid dashboard ID format: {DashboardId}", dashboardId);
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/v1/dashboard/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<SupersetSingleResponse<SupersetDashboard>>(JsonOptions, cancellationToken);
            return result?.Result != null ? MapToDashboardInfo(result.Result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard {DashboardId}", dashboardId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DashboardInfo>> GetDashboardsForUserAsync(
        int userId,
        IEnumerable<string>? roles = null,
        CancellationToken cancellationToken = default)
    {
        // Superset has its own permission model - we return all accessible dashboards
        // The guest token will enforce RLS based on the user context
        var dashboards = await GetDashboardsAsync(cancellationToken);

        // Filter based on dashboard tags matching roles (convention-based)
        if (roles?.Any() == true)
        {
            var roleSet = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
            dashboards = dashboards.Where(d =>
                d.Tags == null ||
                !d.Tags.Any() ||
                d.Tags.Any(t => roleSet.Contains(t)));
        }

        return dashboards;
    }

    #endregion

    #region Embedding Operations

    /// <inheritdoc />
    public async Task<EmbedResult> GetEmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        if (!int.TryParse(request.ResourceId, out var dashboardId))
        {
            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object>
                {
                    ["error"] = $"Invalid dashboard ID: {request.ResourceId}"
                }
            };
        }

        try
        {
            // Build RLS filters
            var rlsFilters = new List<object>();
            if (request.Filters?.Any() == true)
            {
                foreach (var filter in request.Filters)
                {
                    rlsFilters.Add(new
                    {
                        clause = $"{filter.Key} = '{filter.Value}'"
                    });
                }
            }

            // Apply default RLS filters with user substitution
            foreach (var defaultFilter in _config.DefaultRlsFilters)
            {
                var clause = defaultFilter.Value
                    .Replace("{userId}", request.UserId?.ToString() ?? "0")
                    .Replace("{userEmail}", request.UserEmail ?? "");

                rlsFilters.Add(new { clause });
            }

            // Create guest token request
            var guestTokenRequest = new
            {
                user = new
                {
                    username = request.UserEmail ?? $"user_{request.UserId}",
                    first_name = "CRM",
                    last_name = "User"
                },
                resources = new[]
                {
                    new { type = "dashboard", id = dashboardId }
                },
                rls = rlsFilters
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(guestTokenRequest, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("api/v1/security/guest_token/", requestContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get guest token: {Error}", error);

                return new EmbedResult
                {
                    EmbedType = "error",
                    Config = new Dictionary<string, object>
                    {
                        ["error"] = "Failed to generate embed token",
                        ["details"] = error
                    }
                };
            }

            var result = await response.Content.ReadFromJsonAsync<SupersetGuestTokenResponse>(JsonOptions, cancellationToken);

            // Build embed URL
            var embedUrl = $"{_config.BaseUrl}/superset/dashboard/{dashboardId}/" +
                           $"?standalone=true" +
                           $"&guest_token={result?.Token}";

            if (request.HideHeader)
            {
                embedUrl += "&show_filters=0";
            }

            return new EmbedResult
            {
                EmbedType = "iframe",
                EmbedUrl = embedUrl,
                Token = result?.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_config.GuestToken.DefaultExpirationMinutes),
                Height = 800,
                Width = null, // Use 100%
                Config = new Dictionary<string, object>
                {
                    ["dashboardId"] = dashboardId,
                    ["standalone"] = true,
                    ["allowFullscreen"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embed for dashboard {DashboardId}", dashboardId);

            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    /// <inheritdoc />
    public async Task<string> GetGuestTokenAsync(
        string dashboardId,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        var embedResult = await GetEmbedAsync(new EmbedRequest
        {
            ResourceId = dashboardId,
            Filters = filters,
            HideHeader = true
        }, cancellationToken);

        if (embedResult.EmbedType == "error")
        {
            var errorMsg = embedResult.Config?.GetValueOrDefault("error")?.ToString() ?? "Unknown error";
            throw new InvalidOperationException($"Failed to generate guest token: {errorMsg}");
        }

        return embedResult.Token ?? throw new InvalidOperationException("No token in embed result");
    }

    #endregion

    #region Chart Operations

    /// <inheritdoc />
    public async Task<IEnumerable<ChartInfo>> GetChartsAsync(string? dashboardId = null, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = "api/v1/chart/";
        if (!string.IsNullOrEmpty(dashboardId) && int.TryParse(dashboardId, out var id))
        {
            url += $"?q=(filters:!((col:dashboards,opr:rel_m_m,value:{id})))";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SupersetListResponse<SupersetChart>>(JsonOptions, cancellationToken);

        return result?.Result?.Select(MapToChartInfo) ?? Enumerable.Empty<ChartInfo>();
    }

    /// <inheritdoc />
    public async Task<EmbedResult> GetChartEmbedAsync(
        string chartId,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        if (!int.TryParse(chartId, out var id))
        {
            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object>
                {
                    ["error"] = $"Invalid chart ID: {chartId}"
                }
            };
        }

        try
        {
            // Build standalone chart URL
            var chartUrl = $"{_config.BaseUrl}/superset/explore/?form_data_key=&slice_id={id}&standalone=true";

            if (filters?.Any() == true)
            {
                var filterParam = Uri.EscapeDataString(JsonSerializer.Serialize(filters, JsonOptions));
                chartUrl += $"&extra_filters={filterParam}";
            }

            return new EmbedResult
            {
                EmbedType = "iframe",
                EmbedUrl = chartUrl,
                Height = 400,
                Width = 600,
                Config = new Dictionary<string, object>
                {
                    ["chartId"] = id,
                    ["standalone"] = true
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart embed for {ChartId}", chartId);

            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    #endregion

    #region Query Operations

    /// <inheritdoc />
    public async Task<ReportResult> ExecuteReportAsync(
        string reportId,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        // Superset charts can be executed as "reports" via the chart data API
        await EnsureAuthenticatedAsync(cancellationToken);

        if (!int.TryParse(reportId, out var chartId))
        {
            return new ReportResult
            {
                ReportId = reportId,
                Success = false,
                Error = $"Invalid report/chart ID: {reportId}",
                ExecutedAt = DateTime.UtcNow
            };
        }

        try
        {
            var startTime = DateTime.UtcNow;

            // Get chart data
            var response = await _httpClient.GetAsync($"api/v1/chart/{chartId}/data/", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ReportResult
                {
                    ReportId = reportId,
                    Success = false,
                    Error = error,
                    ExecutedAt = DateTime.UtcNow
                };
            }

            var result = await response.Content.ReadFromJsonAsync<SupersetChartDataResponse>(JsonOptions, cancellationToken);
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (result?.Result == null || !result.Result.Any())
            {
                return new ReportResult
                {
                    ReportId = reportId,
                    Success = true,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutionTimeMs = (long)executionTime,
                    Columns = new List<string>(),
                    Rows = new List<Dictionary<string, object>>(),
                    RowCount = 0
                };
            }

            var firstResult = result.Result.First();
            var columns = firstResult.Colnames ?? new List<string>();
            var rows = new List<Dictionary<string, object>>();

            if (firstResult.Data != null)
            {
                foreach (var row in firstResult.Data)
                {
                    var rowDict = new Dictionary<string, object>();
                    for (int i = 0; i < columns.Count && i < row.Count; i++)
                    {
                        rowDict[columns[i]] = row[i];
                    }
                    rows.Add(rowDict);
                }
            }

            return new ReportResult
            {
                ReportId = reportId,
                Success = true,
                ExecutedAt = DateTime.UtcNow,
                ExecutionTimeMs = (long)executionTime,
                Columns = columns,
                Rows = rows,
                RowCount = rows.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing report {ReportId}", reportId);

            return new ReportResult
            {
                ReportId = reportId,
                Success = false,
                Error = ex.Message,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportInfo>> GetReportsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        // In Superset, "reports" are typically saved charts
        var charts = await GetChartsAsync(null, cancellationToken);

        var reports = charts.Select(c => new ReportInfo
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Category = c.DashboardId ?? "Uncategorized",
            Parameters = new List<ReportParameter>(),
            OutputFormats = new List<string> { "json", "csv" }
        });

        if (!string.IsNullOrEmpty(category))
        {
            reports = reports.Where(r => r.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
        }

        return reports;
    }

    #endregion

    #region Data Source Operations

    /// <inheritdoc />
    public async Task<IEnumerable<DataSourceInfo>> GetDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync("api/v1/database/", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SupersetListResponse<SupersetDatabase>>(JsonOptions, cancellationToken);

        return result?.Result?.Select(db => new DataSourceInfo
        {
            Id = db.Id.ToString(),
            Name = db.DatabaseName ?? db.Id.ToString(),
            Type = db.Backend,
            Status = db.AllowRunAsync == true ? "active" : "read-only",
            LastRefreshed = null // Superset doesn't track this at database level
        }) ?? Enumerable.Empty<DataSourceInfo>();
    }

    /// <inheritdoc />
    public async Task<RefreshJobStatus> RefreshDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        if (!int.TryParse(dataSourceId, out var id))
        {
            return new RefreshJobStatus
            {
                JobId = Guid.NewGuid().ToString(),
                Status = "failed",
                Error = $"Invalid data source ID: {dataSourceId}",
                StartedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Superset doesn't have a direct "refresh" endpoint for databases
            // Instead, we can refresh the schema metadata
            var response = await _httpClient.PostAsync(
                $"api/v1/database/{id}/schemas/?force=true",
                null,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new RefreshJobStatus
                {
                    JobId = Guid.NewGuid().ToString(),
                    Status = "completed",
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return new RefreshJobStatus
                {
                    JobId = Guid.NewGuid().ToString(),
                    Status = "failed",
                    Error = error,
                    StartedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing data source {DataSourceId}", dataSourceId);

            return new RefreshJobStatus
            {
                JobId = Guid.NewGuid().ToString(),
                Status = "failed",
                Error = ex.Message,
                StartedAt = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region Health Check

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = await IsAvailableAsync(cancellationToken);

            if (!isAvailable)
            {
                return new ProviderHealthResult
                {
                    IsHealthy = false,
                    ProviderName = ProviderName,
                    Message = "Failed to authenticate with Superset",
                    Details = new Dictionary<string, object>
                    {
                        ["baseUrl"] = _config.BaseUrl
                    }
                };
            }

            // Get counts for health details
            var dashboards = await GetDashboardsAsync(cancellationToken);
            var charts = await GetChartsAsync(null, cancellationToken);
            var dataSources = await GetDataSourcesAsync(cancellationToken);

            return new ProviderHealthResult
            {
                IsHealthy = true,
                ProviderName = ProviderName,
                Message = "Superset is healthy and connected",
                Details = new Dictionary<string, object>
                {
                    ["baseUrl"] = _config.BaseUrl,
                    ["dashboardCount"] = dashboards.Count(),
                    ["chartCount"] = charts.Count(),
                    ["dataSourceCount"] = dataSources.Count(),
                    ["supportsEmbedding"] = true,
                    ["tokenExpiresAt"] = _tokenExpiresAt.ToString("O")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Superset health check failed");

            return new ProviderHealthResult
            {
                IsHealthy = false,
                ProviderName = ProviderName,
                Message = $"Superset health check failed: {ex.Message}",
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["baseUrl"] = _config.BaseUrl
                }
            };
        }
    }

    #endregion

    #region Mapping Helpers

    private static DashboardInfo MapToDashboardInfo(SupersetDashboard dashboard) => new()
    {
        Id = dashboard.Id.ToString(),
        Name = dashboard.DashboardTitle ?? $"Dashboard {dashboard.Id}",
        Description = dashboard.Description,
        Category = dashboard.Slug,
        ThumbnailUrl = dashboard.ThumbnailUrl,
        Url = dashboard.Url,
        CanEmbed = true,
        Owner = dashboard.Owners?.FirstOrDefault()?.Username,
        CreatedAt = dashboard.CreatedOn,
        ModifiedAt = dashboard.ChangedOn,
        Tags = dashboard.Tags?.Select(t => t.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList(),
        ChartCount = dashboard.Charts?.Count
    };

    private static ChartInfo MapToChartInfo(SupersetChart chart) => new()
    {
        Id = chart.Id.ToString(),
        Name = chart.SliceName ?? $"Chart {chart.Id}",
        Description = chart.Description,
        ChartType = chart.VizType,
        DashboardId = chart.Dashboards?.FirstOrDefault()?.Id.ToString(),
        CanEmbed = true,
        Width = chart.Width,
        Height = chart.Height
    };

    #endregion
}

#region Superset API DTOs

internal class SupersetCsrfResponse
{
    public string? Result { get; set; }
}

internal class SupersetLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Provider { get; set; } = "db";
    public bool Refresh { get; set; } = true;
}

internal class SupersetLoginResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

internal class SupersetGuestTokenResponse
{
    public string? Token { get; set; }
}

internal class SupersetListResponse<T>
{
    public int Count { get; set; }
    public List<T>? Result { get; set; }
}

internal class SupersetSingleResponse<T>
{
    public T? Result { get; set; }
}

internal class SupersetDashboard
{
    public int Id { get; set; }
    public string? DashboardTitle { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Url { get; set; }
    public bool Published { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ChangedOn { get; set; }
    public List<SupersetOwner>? Owners { get; set; }
    public List<SupersetTag>? Tags { get; set; }
    public List<SupersetChart>? Charts { get; set; }
}

internal class SupersetChart
{
    public int Id { get; set; }
    public string? SliceName { get; set; }
    public string? Description { get; set; }
    public string? VizType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public List<SupersetDashboardRef>? Dashboards { get; set; }
}

internal class SupersetDashboardRef
{
    public int Id { get; set; }
    public string? DashboardTitle { get; set; }
}

internal class SupersetOwner
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

internal class SupersetTag
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

internal class SupersetDatabase
{
    public int Id { get; set; }
    public string? DatabaseName { get; set; }
    public string? Backend { get; set; }
    public bool? AllowRunAsync { get; set; }
}

internal class SupersetChartDataResponse
{
    public List<SupersetChartDataResult>? Result { get; set; }
}

internal class SupersetChartDataResult
{
    public List<string>? Colnames { get; set; }
    public List<List<object>>? Data { get; set; }
}

#endregion

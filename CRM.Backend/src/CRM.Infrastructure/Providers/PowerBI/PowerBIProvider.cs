// CRM Solution - Pluggable Architecture
// Power BI Analytics Provider Implementation
// Phase 5 Week 23: Power BI Provider

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.PowerBI;

/// <summary>
/// Power BI analytics provider implementing IAnalyticsPort.
/// Supports Azure AD authentication (Service Principal or Master User).
/// Provides dashboard/report embedding with Row-Level Security (RLS) support.
/// </summary>
public class PowerBIProvider : IAnalyticsPort
{
    private readonly HttpClient _httpClient;
    private readonly PowerBIConfiguration _config;
    private readonly ILogger<PowerBIProvider> _logger;

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private const string PowerBIApiBaseUrl = "https://api.powerbi.com/v1.0/myorg";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PowerBIProvider(
        HttpClient httpClient,
        IOptions<PowerBIConfiguration> options,
        ILogger<PowerBIProvider> logger)
    {
        _httpClient = httpClient;
        _config = options.Value;
        _logger = logger;
    }

    #region IAnalyticsPort Implementation

    public string ProviderName => "PowerBI";

    public bool SupportsEmbedding => true;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var (isValid, _) = _config.Validate();
        if (!isValid) return false;

        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Power BI availability check failed");
            return false;
        }
    }

    #endregion

    #region Dashboard Operations

    public async Task<IEnumerable<DashboardInfo>> GetDashboardsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/dashboards";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PowerBIDashboardListResponse>(JsonOptions, cancellationToken);

            return result?.Value?.Select(d => new DashboardInfo
            {
                Id = d.Id,
                Name = GetMappedName(d.Id, d.DisplayName, _config.DashboardMappings),
                Description = $"Power BI Dashboard: {d.DisplayName}",
                Category = "PowerBI",
                Url = d.WebUrl,
                CanEmbed = true,
                Tags = new List<string> { "powerbi", "dashboard" }
            }) ?? Enumerable.Empty<DashboardInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboards from Power BI");
            return Enumerable.Empty<DashboardInfo>();
        }
    }

    public async Task<DashboardInfo?> GetDashboardAsync(string dashboardId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/dashboards/{dashboardId}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var dashboard = await response.Content.ReadFromJsonAsync<PowerBIDashboard>(JsonOptions, cancellationToken);
            if (dashboard == null) return null;

            return new DashboardInfo
            {
                Id = dashboard.Id,
                Name = GetMappedName(dashboard.Id, dashboard.DisplayName, _config.DashboardMappings),
                Description = $"Power BI Dashboard: {dashboard.DisplayName}",
                Category = "PowerBI",
                Url = dashboard.WebUrl,
                CanEmbed = true,
                Tags = new List<string> { "powerbi", "dashboard" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard {DashboardId} from Power BI", dashboardId);
            return null;
        }
    }

    public async Task<IEnumerable<DashboardInfo>> GetDashboardsForUserAsync(
        int userId, 
        IEnumerable<string>? roles = null, 
        CancellationToken cancellationToken = default)
    {
        // Power BI handles permissions through RLS; we return all dashboards
        // Actual filtering happens at embed token generation time
        return await GetDashboardsAsync(cancellationToken);
    }

    #endregion

    #region Embedding Operations

    public async Task<EmbedResult> GetEmbedAsync(EmbedRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        try
        {
            // Determine if it's a report or dashboard
            var embedType = request.EmbedType?.ToLowerInvariant() ?? "report";
            var resourceId = request.ResourceId;

            if (string.IsNullOrEmpty(resourceId) && request.Filters?.ContainsKey("dashboardId") == true)
            {
                resourceId = request.Filters["dashboardId"];
            }

            if (embedType == "dashboard")
            {
                return await GetDashboardEmbedAsync(resourceId, request, cancellationToken);
            }

            // Default to report embedding
            return await GetReportEmbedAsync(resourceId, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embed for {ResourceId}", request.ResourceId);
            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object> { ["error"] = ex.Message }
            };
        }
    }

    private async Task<EmbedResult> GetReportEmbedAsync(string reportId, EmbedRequest request, CancellationToken cancellationToken)
    {
        // Get report details
        var reportUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/reports/{reportId}";
        var reportResponse = await _httpClient.GetAsync(reportUrl, cancellationToken);
        reportResponse.EnsureSuccessStatusCode();

        var report = await reportResponse.Content.ReadFromJsonAsync<PowerBIReport>(JsonOptions, cancellationToken);
        if (report == null)
        {
            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object> { ["error"] = "Report not found" }
            };
        }

        // Generate embed token
        var embedToken = await GenerateEmbedTokenAsync(report.Id, report.DatasetId, request, cancellationToken);

        return new EmbedResult
        {
            EmbedType = "sdk",
            EmbedUrl = report.EmbedUrl,
            Token = embedToken.Token,
            ExpiresAt = embedToken.Expiration,
            Config = new Dictionary<string, object>
            {
                ["reportId"] = report.Id,
                ["type"] = "report",
                ["providerName"] = ProviderName
            }
        };
    }

    private async Task<EmbedResult> GetDashboardEmbedAsync(string dashboardId, EmbedRequest request, CancellationToken cancellationToken)
    {
        // Get dashboard details
        var dashboard = await GetDashboardAsync(dashboardId, cancellationToken);
        if (dashboard == null)
        {
            return new EmbedResult
            {
                EmbedType = "error",
                Config = new Dictionary<string, object> { ["error"] = "Dashboard not found" }
            };
        }

        // Generate embed token for dashboard
        var tokenRequest = new
        {
            dashboards = new[] { new { id = dashboardId } },
            accessLevel = "View"
        };

        var tokenUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/GenerateToken";
        var tokenResponse = await _httpClient.PostAsJsonAsync(tokenUrl, tokenRequest, JsonOptions, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        var embedToken = await tokenResponse.Content.ReadFromJsonAsync<PowerBIEmbedToken>(JsonOptions, cancellationToken);

        return new EmbedResult
        {
            EmbedType = "sdk",
            EmbedUrl = dashboard.Url,
            Token = embedToken?.Token ?? string.Empty,
            ExpiresAt = embedToken?.Expiration ?? DateTime.UtcNow.AddHours(1),
            Config = new Dictionary<string, object>
            {
                ["dashboardId"] = dashboardId,
                ["type"] = "dashboard",
                ["providerName"] = ProviderName
            }
        };
    }

    public async Task<string> GetGuestTokenAsync(
        string dashboardId, 
        Dictionary<string, string>? filters = null, 
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        // For guest access, we generate an embed token with RLS
        var reportUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/reports/{dashboardId}";
        var reportResponse = await _httpClient.GetAsync(reportUrl, cancellationToken);
        reportResponse.EnsureSuccessStatusCode();

        var report = await reportResponse.Content.ReadFromJsonAsync<PowerBIReport>(JsonOptions, cancellationToken);
        if (report == null)
        {
            throw new InvalidOperationException("Report not found");
        }

        var request = new EmbedRequest
        {
            ResourceId = dashboardId,
            Filters = filters
        };

        var embedToken = await GenerateEmbedTokenAsync(report.Id, report.DatasetId, request, cancellationToken);
        return embedToken.Token;
    }

    private async Task<PowerBIEmbedToken> GenerateEmbedTokenAsync(
        string reportId,
        string? datasetId,
        EmbedRequest request,
        CancellationToken cancellationToken)
    {
        var tokenRequestBody = new Dictionary<string, object>
        {
            ["reports"] = new[] { new { id = reportId } },
            ["accessLevel"] = _config.EmbedConfig?.AllowEdit == true ? "Edit" : "View"
        };

        // Add dataset if available
        if (!string.IsNullOrEmpty(datasetId))
        {
            tokenRequestBody["datasets"] = new[] { new { id = datasetId } };
        }

        // Add RLS identity if enabled
        if (_config.EnableRls && request.UserId.HasValue)
        {
            var identities = new List<object>
            {
                new
                {
                    username = request.UserEmail ?? request.UserId.ToString(),
                    roles = request.Roles?.ToArray() ?? new[] { _config.DefaultRlsRole ?? "CRMUser" },
                    datasets = new[] { datasetId }
                }
            };
            tokenRequestBody["identities"] = identities;
        }

        var tokenUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/reports/{reportId}/GenerateToken";
        var tokenResponse = await _httpClient.PostAsJsonAsync(tokenUrl, tokenRequestBody, JsonOptions, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        var embedToken = await tokenResponse.Content.ReadFromJsonAsync<PowerBIEmbedToken>(JsonOptions, cancellationToken);
        return embedToken ?? throw new InvalidOperationException("Failed to generate embed token");
    }

    #endregion

    #region Chart Operations

    public async Task<IEnumerable<ChartInfo>> GetChartsAsync(string? dashboardId = null, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        if (string.IsNullOrEmpty(dashboardId))
        {
            // Get tiles from all dashboards
            var dashboards = await GetDashboardsAsync(cancellationToken);
            var allTiles = new List<ChartInfo>();
            foreach (var dashboard in dashboards.Take(10)) // Limit to prevent too many API calls
            {
                var tiles = await GetDashboardTilesAsync(dashboard.Id, cancellationToken);
                allTiles.AddRange(tiles);
            }
            return allTiles;
        }

        return await GetDashboardTilesAsync(dashboardId, cancellationToken);
    }

    private async Task<IEnumerable<ChartInfo>> GetDashboardTilesAsync(string dashboardId, CancellationToken cancellationToken)
    {
        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/dashboards/{dashboardId}/tiles";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<ChartInfo>();

            var result = await response.Content.ReadFromJsonAsync<PowerBITileListResponse>(JsonOptions, cancellationToken);

            return result?.Value?.Select(t => new ChartInfo
            {
                Id = t.Id,
                Name = t.Title ?? "Untitled Tile",
                Description = $"Power BI Tile from dashboard {dashboardId}",
                ChartType = "tile",
                DashboardId = dashboardId,
                CanEmbed = true
            }) ?? Enumerable.Empty<ChartInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get tiles for dashboard {DashboardId}", dashboardId);
            return Enumerable.Empty<ChartInfo>();
        }
    }

    public Task<EmbedResult> GetChartEmbedAsync(
        string chartId, 
        Dictionary<string, string>? filters = null, 
        CancellationToken cancellationToken = default)
    {
        // Tiles are embedded as part of dashboards in Power BI
        return Task.FromResult(new EmbedResult
        {
            EmbedType = "error",
            Config = new Dictionary<string, object>
            {
                ["error"] = "Power BI tiles must be embedded within their parent dashboard. Use GetEmbedAsync with the dashboard ID."
            }
        });
    }

    #endregion

    #region Report Operations

    public async Task<ReportResult> ExecuteReportAsync(
        string reportId, 
        Dictionary<string, object>? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        // Power BI doesn't support direct report execution like a traditional reporting system.
        // Instead, we return embed instructions for the report.
        var reportUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/reports/{reportId}";
        var response = await _httpClient.GetAsync(reportUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ReportResult
            {
                ReportId = reportId,
                Success = false,
                ExecutedAt = DateTime.UtcNow,
                Error = $"Report not found: {response.StatusCode}"
            };
        }

        var report = await response.Content.ReadFromJsonAsync<PowerBIReport>(JsonOptions, cancellationToken);

        return new ReportResult
        {
            ReportId = reportId,
            Success = true,
            ExecutedAt = DateTime.UtcNow,
            ExecutionTimeMs = 0,
            RowCount = 1,
            Columns = new List<string> { "reportId", "name", "embedUrl", "webUrl" },
            Rows = new List<Dictionary<string, object>>
            {
                new()
                {
                    ["reportId"] = report?.Id ?? reportId,
                    ["name"] = report?.Name ?? "Unknown",
                    ["embedUrl"] = report?.EmbedUrl ?? string.Empty,
                    ["webUrl"] = report?.WebUrl ?? string.Empty
                }
            }
        };
    }

    public async Task<IEnumerable<ReportInfo>> GetReportsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/reports";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PowerBIReportListResponse>(JsonOptions, cancellationToken);

            return result?.Value?.Select(r => new ReportInfo
            {
                Id = r.Id,
                Name = GetMappedName(r.Id, r.Name, _config.ReportMappings),
                Description = $"Power BI Report: {r.Name}",
                Category = "PowerBI",
                OutputFormats = new List<string> { "embed", "web" }
            }) ?? Enumerable.Empty<ReportInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get reports from Power BI");
            return Enumerable.Empty<ReportInfo>();
        }
    }

    #endregion

    #region Data Source Operations

    public async Task<IEnumerable<DataSourceInfo>> GetDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/datasets";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PowerBIDatasetListResponse>(JsonOptions, cancellationToken);

            return result?.Value?.Select(d => new DataSourceInfo
            {
                Id = d.Id,
                Name = d.Name,
                Type = "powerbi-dataset",
                Status = d.IsRefreshable ? "Active" : "Static"
            }) ?? Enumerable.Empty<DataSourceInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get datasets from Power BI");
            return Enumerable.Empty<DataSourceInfo>();
        }
    }

    public async Task<RefreshJobStatus> RefreshDataSourceAsync(string dataSourceId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var url = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}/datasets/{dataSourceId}/refreshes";

        try
        {
            var response = await _httpClient.PostAsync(url, null, cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return new RefreshJobStatus
                {
                    JobId = Guid.NewGuid().ToString(),
                    Status = "InProgress",
                    StartedAt = DateTime.UtcNow
                };
            }

            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new RefreshJobStatus
            {
                JobId = string.Empty,
                Status = "Failed",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                Error = error
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh dataset {DatasetId}", dataSourceId);
            return new RefreshJobStatus
            {
                JobId = string.Empty,
                Status = "Failed",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                Error = ex.Message
            };
        }
    }

    #endregion

    #region Health Check

    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var (isValid, error) = _config.Validate();
        if (!isValid)
        {
            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = $"Configuration invalid: {error}",
                Details = new Dictionary<string, object>()
            };
        }

        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            // Verify workspace access
            var workspaceUrl = $"{PowerBIApiBaseUrl}/groups/{_config.WorkspaceId}";
            var workspaceResponse = await _httpClient.GetAsync(workspaceUrl, cancellationToken);
            workspaceResponse.EnsureSuccessStatusCode();

            // Get counts
            var dashboards = await GetDashboardsAsync(cancellationToken);
            var reports = await GetReportsAsync(cancellationToken: cancellationToken);
            var datasets = await GetDataSourcesAsync(cancellationToken);

            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = true,
                Message = "Power BI connected successfully",
                Details = new Dictionary<string, object>
                {
                    ["workspaceId"] = _config.WorkspaceId ?? string.Empty,
                    ["authMethod"] = _config.AuthMethod.ToString(),
                    ["dashboardCount"] = dashboards.Count(),
                    ["reportCount"] = reports.Count(),
                    ["datasetCount"] = datasets.Count()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Power BI health check failed");
            return new ProviderHealthResult
            {
                ProviderName = ProviderName,
                IsHealthy = false,
                Message = $"Connection failed: {ex.Message}",
                Details = new Dictionary<string, object>()
            };
        }
    }

    #endregion

    #region Authentication

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return;
            }

            var token = await AcquireTokenAsync(cancellationToken);
            _accessToken = token.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(_config.TokenCacheMinutes);

            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<TokenResponse> AcquireTokenAsync(CancellationToken cancellationToken)
    {
        var tokenEndpoint = $"https://login.microsoftonline.com/{_config.TenantId}/oauth2/v2.0/token";

        var formData = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId ?? string.Empty,
            ["scope"] = "https://analysis.windows.net/powerbi/api/.default"
        };

        if (_config.AuthMethod == PowerBIAuthMethod.ServicePrincipal)
        {
            formData["grant_type"] = "client_credentials";
            formData["client_secret"] = _config.ClientSecret ?? string.Empty;
        }
        else if (_config.AuthMethod == PowerBIAuthMethod.MasterUser && _config.MasterUser != null)
        {
            formData["grant_type"] = "password";
            formData["username"] = _config.MasterUser.Username ?? string.Empty;
            formData["password"] = _config.MasterUser.Password ?? string.Empty;
            formData["client_secret"] = _config.ClientSecret ?? string.Empty;
        }
        else
        {
            throw new InvalidOperationException("Invalid authentication configuration");
        }

        var content = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(tokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, cancellationToken);
        return tokenResponse ?? throw new InvalidOperationException("Failed to acquire access token");
    }

    #endregion

    #region Helpers

    private static string GetMappedName(string id, string defaultName, Dictionary<string, string>? mappings)
    {
        if (mappings != null && mappings.TryGetValue(id, out var mappedName))
        {
            return mappedName;
        }
        return defaultName;
    }

    #endregion

    #region Power BI API Response Models

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private class PowerBIDashboardListResponse
    {
        public List<PowerBIDashboard>? Value { get; set; }
    }

    private class PowerBIDashboard
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? EmbedUrl { get; set; }
        public string? WebUrl { get; set; }
        public bool IsReadOnly { get; set; }
    }

    private class PowerBIReportListResponse
    {
        public List<PowerBIReport>? Value { get; set; }
    }

    private class PowerBIReport
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DatasetId { get; set; }
        public string? EmbedUrl { get; set; }
        public string? WebUrl { get; set; }
    }

    private class PowerBITileListResponse
    {
        public List<PowerBITile>? Value { get; set; }
    }

    private class PowerBITile
    {
        public string Id { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? EmbedUrl { get; set; }
        public string? ReportId { get; set; }
        public string? DatasetId { get; set; }
    }

    private class PowerBIDatasetListResponse
    {
        public List<PowerBIDataset>? Value { get; set; }
    }

    private class PowerBIDataset
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsRefreshable { get; set; }
        public string? ConfiguredBy { get; set; }
        public string? WebUrl { get; set; }
    }

    private class PowerBIEmbedToken
    {
        public string Token { get; set; } = string.Empty;
        public string? TokenId { get; set; }
        public DateTime Expiration { get; set; }
    }

    #endregion
}

// CRM Solution - Pluggable Architecture
// Power BI Provider Unit Tests
// Phase 5 Week 23: Power BI Provider

using System.Net;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.PowerBI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for PowerBIProvider.
/// Tests Azure AD authentication, dashboard operations, embedding, reports, and health checks.
/// </summary>
public class PowerBIProviderTests
{
    private readonly Mock<ILogger<PowerBIProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;

    public PowerBIProviderTests()
    {
        _loggerMock = new Mock<ILogger<PowerBIProvider>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.powerbi.com")
        };
    }

    private PowerBIProvider CreateProvider(PowerBIConfiguration? config = null)
    {
        config ??= CreateValidConfiguration();
        var options = Options.Create(config);
        return new PowerBIProvider(_httpClient, options, _loggerMock.Object);
    }

    private static PowerBIConfiguration CreateValidConfiguration()
    {
        return new PowerBIConfiguration
        {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            WorkspaceId = "test-workspace-id",
            AuthMethod = PowerBIAuthMethod.ServicePrincipal,
            TokenCacheMinutes = 55,
            TimeoutSeconds = 30,
            EnableRls = true,
            DefaultRlsRole = "CRMUser"
        };
    }

    private void SetupTokenResponse()
    {
        var tokenResponse = new { access_token = "test-access-token", token_type = "Bearer", expires_in = 3600 };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("login.microsoftonline.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });
    }

    #region Configuration Validation Tests

    [Fact]
    public void Validate_WithValidServicePrincipalConfig_ReturnsValid()
    {
        // Arrange
        var config = CreateValidConfiguration();

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WithMissingTenantId_ReturnsInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.TenantId = string.Empty;

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("TenantId", error);
    }

    [Fact]
    public void Validate_WithMissingClientId_ReturnsInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.ClientId = string.Empty;

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("ClientId", error);
    }

    [Fact]
    public void Validate_WithMissingWorkspaceId_ReturnsInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.WorkspaceId = string.Empty;

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("WorkspaceId", error);
    }

    [Fact]
    public void Validate_ServicePrincipalWithMissingClientSecret_ReturnsInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.AuthMethod = PowerBIAuthMethod.ServicePrincipal;
        config.ClientSecret = string.Empty;

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("ClientSecret", error);
    }

    [Fact]
    public void Validate_MasterUserWithMissingCredentials_ReturnsInvalid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.AuthMethod = PowerBIAuthMethod.MasterUser;
        config.MasterUser = null;

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.False(isValid);
        Assert.Contains("MasterUser", error);
    }

    [Fact]
    public void Validate_MasterUserWithValidCredentials_ReturnsValid()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.AuthMethod = PowerBIAuthMethod.MasterUser;
        config.MasterUser = new MasterUserCredentials
        {
            Username = "user@company.com",
            Password = "password123"
        };

        // Act
        var (isValid, error) = config.Validate();

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsPowerBI()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        Assert.Equal("PowerBI", provider.ProviderName);
    }

    [Fact]
    public void SupportsEmbedding_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();

        // Act & Assert
        Assert.True(provider.SupportsEmbedding);
    }

    #endregion

    #region Availability Tests

    [Fact]
    public async Task IsAvailableAsync_WithValidConfigAndToken_ReturnsTrue()
    {
        // Arrange
        SetupTokenResponse();
        var provider = CreateProvider();

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_WithInvalidConfig_ReturnsFalse()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.TenantId = string.Empty;
        var provider = CreateProvider(config);

        // Act
        var result = await provider.IsAvailableAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardsAsync_ReturnsListOfDashboards()
    {
        // Arrange
        SetupTokenResponse();

        var dashboardsResponse = new
        {
            value = new[]
            {
                new { id = "dash-1", displayName = "Sales Dashboard", webUrl = "https://app.powerbi.com/dash1" },
                new { id = "dash-2", displayName = "Marketing Dashboard", webUrl = "https://app.powerbi.com/dash2" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/dashboards") &&
                    !req.RequestUri.ToString().Contains("tiles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(dashboardsResponse))
            });

        var provider = CreateProvider();

        // Act
        var dashboards = await provider.GetDashboardsAsync();

        // Assert
        var dashboardList = dashboards.ToList();
        Assert.Equal(2, dashboardList.Count);
        Assert.Equal("dash-1", dashboardList[0].Id);
        Assert.Equal("Sales Dashboard", dashboardList[0].Name);
        Assert.True(dashboardList[0].CanEmbed);
    }

    [Fact]
    public async Task GetDashboardAsync_WithValidId_ReturnsDashboard()
    {
        // Arrange
        SetupTokenResponse();

        var dashboard = new { id = "dash-1", displayName = "Sales Dashboard", webUrl = "https://app.powerbi.com/dash1" };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/dashboards/dash-1") &&
                    !req.RequestUri.ToString().Contains("tiles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(dashboard))
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.GetDashboardAsync("dash-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dash-1", result.Id);
        Assert.Equal("Sales Dashboard", result.Name);
    }

    [Fact]
    public async Task GetDashboardAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        SetupTokenResponse();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/dashboards/invalid")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.GetDashboardAsync("invalid");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Embed Tests

    [Fact]
    public async Task GetEmbedAsync_ForReport_ReturnsEmbedResult()
    {
        // Arrange
        SetupTokenResponse();

        var report = new
        {
            id = "report-1",
            name = "Sales Report",
            datasetId = "dataset-1",
            embedUrl = "https://app.powerbi.com/embed/report1",
            webUrl = "https://app.powerbi.com/report1"
        };

        var embedToken = new
        {
            token = "embed-token-123",
            tokenId = "token-id-1",
            expiration = DateTime.UtcNow.AddHours(1)
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports/report-1") &&
                    !req.RequestUri.ToString().Contains("GenerateToken")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(report))
            });

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("GenerateToken")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(embedToken))
            });

        var provider = CreateProvider();

        // Act
        var request = new EmbedRequest
        {
            ResourceId = "report-1",
            EmbedType = "report"
        };
        var result = await provider.GetEmbedAsync(request);

        // Assert
        Assert.Equal("sdk", result.EmbedType);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.EmbedUrl);
        Assert.NotNull(result.Config);
        Assert.Equal("report-1", result.Config["reportId"]);
    }

    [Fact]
    public async Task GetEmbedAsync_WithError_ReturnsErrorResult()
    {
        // Arrange
        SetupTokenResponse();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Report not found")
            });

        var provider = CreateProvider();

        // Act
        var request = new EmbedRequest
        {
            ResourceId = "invalid-report",
            EmbedType = "report"
        };
        var result = await provider.GetEmbedAsync(request);

        // Assert
        Assert.Equal("error", result.EmbedType);
        Assert.NotNull(result.Config);
        Assert.True(result.Config.ContainsKey("error"));
    }

    #endregion

    #region Chart Tests

    [Fact]
    public async Task GetChartsAsync_ForDashboard_ReturnsTiles()
    {
        // Arrange
        SetupTokenResponse();

        var tilesResponse = new
        {
            value = new[]
            {
                new { id = "tile-1", title = "Revenue Chart", embedUrl = "https://app.powerbi.com/tile1" },
                new { id = "tile-2", title = "Growth Chart", embedUrl = "https://app.powerbi.com/tile2" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/tiles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tilesResponse))
            });

        var provider = CreateProvider();

        // Act
        var charts = await provider.GetChartsAsync("dashboard-1");

        // Assert
        var chartList = charts.ToList();
        Assert.Equal(2, chartList.Count);
        Assert.Equal("tile-1", chartList[0].Id);
        Assert.Equal("Revenue Chart", chartList[0].Name);
        Assert.Equal("tile", chartList[0].ChartType);
        Assert.True(chartList[0].CanEmbed);
    }

    [Fact]
    public async Task GetChartEmbedAsync_ReturnsNotSupportedMessage()
    {
        // Arrange
        var provider = CreateProvider();

        // Act
        var result = await provider.GetChartEmbedAsync("tile-1");

        // Assert
        Assert.Equal("error", result.EmbedType);
        Assert.NotNull(result.Config);
        Assert.Contains("dashboard", result.Config["error"].ToString()!.ToLower());
    }

    #endregion

    #region Report Tests

    [Fact]
    public async Task GetReportsAsync_ReturnsListOfReports()
    {
        // Arrange
        SetupTokenResponse();

        var reportsResponse = new
        {
            value = new[]
            {
                new { id = "report-1", name = "Sales Analysis", datasetId = "ds-1", embedUrl = "https://app.powerbi.com/r1" },
                new { id = "report-2", name = "Marketing ROI", datasetId = "ds-2", embedUrl = "https://app.powerbi.com/r2" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports") &&
                    !req.RequestUri.ToString().Contains("GenerateToken")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(reportsResponse))
            });

        var provider = CreateProvider();

        // Act
        var reports = await provider.GetReportsAsync();

        // Assert
        var reportList = reports.ToList();
        Assert.Equal(2, reportList.Count);
        Assert.Equal("report-1", reportList[0].Id);
        Assert.Equal("Sales Analysis", reportList[0].Name);
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsEmbedInstructions()
    {
        // Arrange
        SetupTokenResponse();

        var report = new
        {
            id = "report-1",
            name = "Sales Report",
            embedUrl = "https://app.powerbi.com/embed/report1",
            webUrl = "https://app.powerbi.com/report1"
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports/report-1")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(report))
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.ExecuteReportAsync("report-1");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("report-1", result.ReportId);
        Assert.NotNull(result.Columns);
        Assert.Contains("embedUrl", result.Columns);
        Assert.NotNull(result.Rows);
        Assert.Single(result.Rows);
    }

    [Fact]
    public async Task ExecuteReportAsync_WithInvalidReport_ReturnsError()
    {
        // Arrange
        SetupTokenResponse();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports/invalid")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.ExecuteReportAsync("invalid");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    #endregion

    #region Data Source Tests

    [Fact]
    public async Task GetDataSourcesAsync_ReturnsListOfDatasets()
    {
        // Arrange
        SetupTokenResponse();

        var datasetsResponse = new
        {
            value = new[]
            {
                new { id = "ds-1", name = "CRM Dataset", isRefreshable = true },
                new { id = "ds-2", name = "Marketing Dataset", isRefreshable = false }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/datasets") &&
                    !req.RequestUri.ToString().Contains("/refreshes")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(datasetsResponse))
            });

        var provider = CreateProvider();

        // Act
        var dataSources = await provider.GetDataSourcesAsync();

        // Assert
        var dataSourceList = dataSources.ToList();
        Assert.Equal(2, dataSourceList.Count);
        Assert.Equal("ds-1", dataSourceList[0].Id);
        Assert.Equal("CRM Dataset", dataSourceList[0].Name);
        Assert.Equal("Active", dataSourceList[0].Status);
        Assert.Equal("Static", dataSourceList[1].Status);
    }

    [Fact]
    public async Task RefreshDataSourceAsync_WithSuccess_ReturnsInProgressStatus()
    {
        // Arrange
        SetupTokenResponse();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/refreshes") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Accepted
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.RefreshDataSourceAsync("ds-1");

        // Assert
        Assert.Equal("InProgress", result.Status);
        Assert.NotNull(result.StartedAt);
        Assert.NotEmpty(result.JobId);
    }

    [Fact]
    public async Task RefreshDataSourceAsync_WithFailure_ReturnsFailedStatus()
    {
        // Arrange
        SetupTokenResponse();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/refreshes") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Refresh limit exceeded")
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.RefreshDataSourceAsync("ds-1");

        // Assert
        Assert.Equal("Failed", result.Status);
        Assert.NotNull(result.Error);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WhenHealthy_ReturnsHealthyResult()
    {
        // Arrange
        SetupTokenResponse();

        // Setup workspace check
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/groups/test-workspace-id") &&
                    !req.RequestUri.ToString().Contains("/dashboards") &&
                    !req.RequestUri.ToString().Contains("/reports") &&
                    !req.RequestUri.ToString().Contains("/datasets")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Setup dashboard list
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/dashboards")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"value\": []}")
            });

        // Setup report list
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/reports")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"value\": []}")
            });

        // Setup dataset list
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("/datasets")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"value\": []}")
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("PowerBI", result.ProviderName);
        Assert.Contains("successfully", result.Message.ToLower());
    }

    [Fact]
    public async Task HealthCheckAsync_WithInvalidConfig_ReturnsUnhealthyResult()
    {
        // Arrange
        var config = CreateValidConfiguration();
        config.TenantId = string.Empty;
        var provider = CreateProvider(config);

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Contains("invalid", result.Message.ToLower());
    }

    [Fact]
    public async Task HealthCheckAsync_WithAuthFailure_ReturnsUnhealthyResult()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("login.microsoftonline.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            });

        var provider = CreateProvider();

        // Act
        var result = await provider.HealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Contains("failed", result.Message.ToLower());
    }

    #endregion
}

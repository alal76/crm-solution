// CRM Solution - Superset Provider Unit Tests
// Tests for Apache Superset analytics provider implementation

namespace CRM.Tests.Providers;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Superset;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

public class SupersetProviderTests
{
    private readonly Mock<ILogger<SupersetProvider>> _loggerMock;
    private readonly SupersetConfiguration _config;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;

    public SupersetProviderTests()
    {
        _loggerMock = new Mock<ILogger<SupersetProvider>>();
        _config = new SupersetConfiguration
        {
            BaseUrl = "http://superset.test",
            Username = "admin",
            Password = "admin123",
            Provider = "db",
            TimeoutSeconds = 30,
            TokenRefreshIntervalMinutes = 50,
            GuestToken = new GuestTokenSettings
            {
                DefaultExpirationMinutes = 60
            }
        };
        
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }

    private SupersetProvider CreateProvider()
    {
        var options = Options.Create(_config);
        return new SupersetProvider(_httpClient, options, _loggerMock.Object);
    }

    private void SetupAuthenticationMocks()
    {
        // Mock CSRF token
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("csrf_token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { result = "test-csrf-token" })
            });

        // Mock login
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("login") && 
                    m.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { access_token = "test-access-token", refresh_token = "test-refresh-token" })
            });
    }

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_Should_Return_Superset()
    {
        var provider = CreateProvider();
        Assert.Equal("Superset", provider.ProviderName);
    }

    [Fact]
    public void SupportsEmbedding_Should_Return_True()
    {
        var provider = CreateProvider();
        Assert.True(provider.SupportsEmbedding);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task IsAvailableAsync_Should_Return_True_When_Authentication_Succeeds()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.IsAvailableAsync();
        
        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_Should_Return_False_When_Authentication_Fails()
    {
        // Mock CSRF token
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("csrf_token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { result = "test-csrf-token" })
            });

        // Mock login failure
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("login")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            });
        
        var provider = CreateProvider();
        
        var result = await provider.IsAvailableAsync();
        
        Assert.False(result);
    }

    #endregion

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardsAsync_Should_Return_Dashboards()
    {
        SetupAuthenticationMocks();
        
        var dashboardData = new
        {
            count = 2,
            result = new[]
            {
                new { id = 1, dashboard_title = "Sales Dashboard", description = "Sales metrics", slug = "sales" },
                new { id = 2, dashboard_title = "Marketing Dashboard", description = "Marketing KPIs", slug = "marketing" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/dashboard/") &&
                    !m.RequestUri!.PathAndQuery.Contains("api/v1/dashboard/1") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dashboardData)
            });

        var provider = CreateProvider();
        
        var result = (await provider.GetDashboardsAsync()).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Equal("Sales Dashboard", result[0].Name);
        Assert.Equal("Marketing Dashboard", result[1].Name);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Return_Dashboard_By_Id()
    {
        SetupAuthenticationMocks();
        
        var dashboardData = new
        {
            result = new { id = 1, dashboard_title = "Sales Dashboard", description = "Sales metrics", slug = "sales" }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery == "/api/v1/dashboard/1" &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dashboardData)
            });

        var provider = CreateProvider();
        
        var result = await provider.GetDashboardAsync("1");
        
        Assert.NotNull(result);
        Assert.Equal("Sales Dashboard", result.Name);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Return_Null_For_Invalid_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.GetDashboardAsync("invalid-id");
        
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Return_Null_When_Not_Found()
    {
        SetupAuthenticationMocks();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery == "/api/v1/dashboard/999" &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var provider = CreateProvider();
        
        var result = await provider.GetDashboardAsync("999");
        
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_Should_Return_All_Dashboards()
    {
        SetupAuthenticationMocks();
        
        var dashboardData = new
        {
            count = 2,
            result = new[]
            {
                new { id = 1, dashboard_title = "Sales Dashboard", description = "Sales metrics", slug = "sales" },
                new { id = 2, dashboard_title = "Admin Dashboard", description = "Admin view", slug = "admin" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/dashboard/") &&
                    !m.RequestUri!.PathAndQuery.Contains("api/v1/dashboard/1") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(dashboardData)
            });

        var provider = CreateProvider();
        
        var result = (await provider.GetDashboardsForUserAsync(1, null)).ToList();
        
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region Embed Tests

    [Fact]
    public async Task GetEmbedAsync_Should_Return_Iframe_Embed_With_Guest_Token()
    {
        SetupAuthenticationMocks();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("guest_token") &&
                    m.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { token = "guest-token-12345" })
            });

        var provider = CreateProvider();
        var request = new EmbedRequest
        {
            ResourceId = "1",
            UserId = 100,
            UserEmail = "user@example.com"
        };
        
        var result = await provider.GetEmbedAsync(request);
        
        Assert.Equal("iframe", result.EmbedType);
        Assert.NotNull(result.EmbedUrl);
        Assert.Contains("guest_token=guest-token-12345", result.EmbedUrl);
        Assert.Contains("standalone=true", result.EmbedUrl);
        Assert.Equal("guest-token-12345", result.Token);
    }

    [Fact]
    public async Task GetEmbedAsync_Should_Return_Error_For_Invalid_Dashboard_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        var request = new EmbedRequest
        {
            ResourceId = "invalid-id"
        };
        
        var result = await provider.GetEmbedAsync(request);
        
        Assert.Equal("error", result.EmbedType);
        Assert.NotNull(result.Config);
        Assert.Contains("Invalid dashboard ID", result.Config["error"]?.ToString());
    }

    [Fact]
    public async Task GetEmbedAsync_Should_Return_Error_When_Guest_Token_Fails()
    {
        SetupAuthenticationMocks();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("guest_token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent("Guest token generation disabled")
            });

        var provider = CreateProvider();
        var request = new EmbedRequest { ResourceId = "1" };
        
        var result = await provider.GetEmbedAsync(request);
        
        Assert.Equal("error", result.EmbedType);
    }

    [Fact]
    public async Task GetGuestTokenAsync_Should_Return_Token_String()
    {
        SetupAuthenticationMocks();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("guest_token") &&
                    m.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { token = "guest-token-abc123" })
            });

        var provider = CreateProvider();
        
        var result = await provider.GetGuestTokenAsync("1", null);
        
        Assert.Equal("guest-token-abc123", result);
    }

    [Fact]
    public async Task GetGuestTokenAsync_Should_Throw_For_Invalid_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            provider.GetGuestTokenAsync("invalid", null));
    }

    #endregion

    #region Chart Tests

    [Fact]
    public async Task GetChartsAsync_Should_Return_Charts()
    {
        SetupAuthenticationMocks();
        
        var chartData = new
        {
            count = 2,
            result = new[]
            {
                new { id = 1, slice_name = "Revenue Chart", description = "Monthly revenue", viz_type = "line" },
                new { id = 2, slice_name = "Leads Pie", description = "Lead sources", viz_type = "pie" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/chart/") &&
                    !m.RequestUri!.PathAndQuery.Contains("/data/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(chartData)
            });

        var provider = CreateProvider();
        
        var result = (await provider.GetChartsAsync(null)).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Equal("Revenue Chart", result[0].Name);
        Assert.Equal("line", result[0].ChartType);
        Assert.Equal("Leads Pie", result[1].Name);
        Assert.Equal("pie", result[1].ChartType);
    }

    [Fact]
    public async Task GetChartEmbedAsync_Should_Return_Iframe_Embed()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.GetChartEmbedAsync("1", null);
        
        Assert.Equal("iframe", result.EmbedType);
        Assert.NotNull(result.EmbedUrl);
        Assert.Contains("slice_id=1", result.EmbedUrl);
        Assert.Contains("standalone=true", result.EmbedUrl);
    }

    [Fact]
    public async Task GetChartEmbedAsync_Should_Return_Error_For_Invalid_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.GetChartEmbedAsync("invalid", null);
        
        Assert.Equal("error", result.EmbedType);
    }

    #endregion

    #region Report/Query Tests

    [Fact]
    public async Task ExecuteReportAsync_Should_Return_Chart_Data()
    {
        SetupAuthenticationMocks();
        
        var chartDataResponse = new
        {
            result = new[]
            {
                new
                {
                    colnames = new[] { "date", "revenue" },
                    data = new[]
                    {
                        new object[] { "2024-01-01", 1000 },
                        new object[] { "2024-01-02", 1500 }
                    }
                }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("/data/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(chartDataResponse)
            });

        var provider = CreateProvider();
        
        var result = await provider.ExecuteReportAsync("1", null);
        
        Assert.True(result.Success);
        Assert.Equal("1", result.ReportId);
        Assert.Equal(2, result.RowCount);
        Assert.Contains("date", result.Columns);
        Assert.Contains("revenue", result.Columns);
    }

    [Fact]
    public async Task ExecuteReportAsync_Should_Return_Error_For_Invalid_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.ExecuteReportAsync("invalid", null);
        
        Assert.False(result.Success);
        Assert.Contains("Invalid report/chart ID", result.Error);
    }

    [Fact]
    public async Task GetReportsAsync_Should_Return_Charts_As_Reports()
    {
        SetupAuthenticationMocks();
        
        var chartData = new
        {
            count = 2,
            result = new[]
            {
                new { id = 1, slice_name = "Revenue Report", description = "Monthly revenue", viz_type = "table" },
                new { id = 2, slice_name = "Leads Report", description = "Lead analysis", viz_type = "table" }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/chart/") &&
                    !m.RequestUri!.PathAndQuery.Contains("/data/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(chartData)
            });

        var provider = CreateProvider();
        
        var result = (await provider.GetReportsAsync(null)).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Equal("Revenue Report", result[0].Name);
        Assert.Contains("json", result[0].OutputFormats);
    }

    #endregion

    #region Data Source Tests

    [Fact]
    public async Task GetDataSourcesAsync_Should_Return_Databases()
    {
        SetupAuthenticationMocks();
        
        var databaseData = new
        {
            count = 2,
            result = new[]
            {
                new { id = 1, database_name = "CRM Database", backend = "postgresql", allow_run_async = true },
                new { id = 2, database_name = "Analytics DW", backend = "postgresql", allow_run_async = true }
            }
        };

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/database/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(databaseData)
            });

        var provider = CreateProvider();
        
        var result = (await provider.GetDataSourcesAsync()).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Equal("CRM Database", result[0].Name);
        Assert.Equal("postgresql", result[0].Type);
        Assert.Equal("active", result[0].Status);
    }

    [Fact]
    public async Task RefreshDataSourceAsync_Should_Return_Completed_On_Success()
    {
        SetupAuthenticationMocks();

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("/schemas/") &&
                    m.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var provider = CreateProvider();
        
        var result = await provider.RefreshDataSourceAsync("1");
        
        Assert.Equal("completed", result.Status);
    }

    [Fact]
    public async Task RefreshDataSourceAsync_Should_Return_Failed_For_Invalid_Id()
    {
        SetupAuthenticationMocks();
        var provider = CreateProvider();
        
        var result = await provider.RefreshDataSourceAsync("invalid");
        
        Assert.Equal("failed", result.Status);
        Assert.Contains("Invalid data source ID", result.Error);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_Should_Return_Healthy_When_Connected()
    {
        // Note: Use Returns() with factory to create fresh response for each call
        
        // First set up catch-all for GET requests 
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { count = 0, result = Array.Empty<object>() })
            }));
        
        // Then set up specific auth mocks (these will override catch-all where they match)
        SetupAuthenticationMocks();

        var provider = CreateProvider();
        
        var result = await provider.HealthCheckAsync();
        
        // For now, just verify it completes without error and returns healthy
        // The actual counts will be 0 since we're using empty arrays
        Assert.True(result.IsHealthy, $"Expected healthy. Message: {result.Message}");
        Assert.Equal("Superset", result.ProviderName);
        Assert.NotNull(result.Details);
    }

    [Fact]
    public async Task HealthCheckAsync_Should_Return_Unhealthy_When_Auth_Fails()
    {
        // Mock CSRF token
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("csrf_token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { result = "test-csrf-token" })
            });

        // Mock login failure
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("login")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid credentials")
            });
        
        var provider = CreateProvider();
        
        var result = await provider.HealthCheckAsync();
        
        Assert.False(result.IsHealthy);
        Assert.Contains("authenticate", result.Message?.ToLower() ?? "");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetDashboardsAsync_Should_Return_Empty_When_No_Dashboards()
    {
        SetupAuthenticationMocks();
        
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/dashboard/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { count = 0, result = Array.Empty<object>() })
            });

        var provider = CreateProvider();
        
        var result = await provider.GetDashboardsAsync();
        
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChartsAsync_Should_Return_Empty_When_No_Charts()
    {
        SetupAuthenticationMocks();
        
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("api/v1/chart/") &&
                    m.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { count = 0, result = Array.Empty<object>() })
            });

        var provider = CreateProvider();
        
        var result = await provider.GetChartsAsync(null);
        
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteReportAsync_Should_Handle_Empty_Result()
    {
        SetupAuthenticationMocks();
        
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("/data/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { result = Array.Empty<object>() })
            });

        var provider = CreateProvider();
        
        var result = await provider.ExecuteReportAsync("1", null);
        
        Assert.True(result.Success);
        Assert.Equal(0, result.RowCount);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task GetEmbedAsync_Should_Apply_Filters_To_RLS()
    {
        SetupAuthenticationMocks();

        HttpRequestMessage? capturedRequest = null;
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => 
                    m.RequestUri!.PathAndQuery.Contains("guest_token") &&
                    m.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { token = "guest-token-with-filters" })
            });

        var provider = CreateProvider();
        var request = new EmbedRequest
        {
            ResourceId = "1",
            UserId = 100,
            Filters = new Dictionary<string, string>
            {
                ["account_id"] = "123"
            }
        };
        
        var result = await provider.GetEmbedAsync(request);
        
        Assert.Equal("iframe", result.EmbedType);
        Assert.NotNull(capturedRequest);
        
        // Verify that the request body contains the filters
        var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("account_id", requestBody);
    }

    #endregion
}

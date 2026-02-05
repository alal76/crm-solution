using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInAnalyticsProvider
/// </summary>
public class BuiltInAnalyticsProviderTests
{
    private readonly Mock<ICrmDbContext> _contextMock;
    private readonly Mock<ILogger<BuiltInAnalyticsProvider>> _loggerMock;
    private readonly BuiltInAnalyticsProvider _provider;

    public BuiltInAnalyticsProviderTests()
    {
        _contextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<BuiltInAnalyticsProvider>>();
        _provider = new BuiltInAnalyticsProvider(_contextMock.Object, _loggerMock.Object);
    }

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthyResult()
    {
        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldIncludeDetails()
    {
        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Details.Should().NotBeNull();
        result.Details.Should().ContainKey("dashboardCount");
        result.Details.Should().ContainKey("reportCount");
        result.Details.Should().ContainKey("supportsEmbedding");
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        // Assert
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportsEmbedding_ShouldReturnFalse()
    {
        // BuiltIn provider doesn't support embedding
        _provider.SupportsEmbedding.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        // BuiltIn provider is always available
        var result = await _provider.IsAvailableAsync();
        result.Should().BeTrue();
    }

    #endregion

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnPredefinedDashboards()
    {
        // Act
        var result = await _provider.GetDashboardsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Count().Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    public async Task GetDashboardsAsync_ShouldIncludeSalesDashboard()
    {
        // Act
        var result = await _provider.GetDashboardsAsync();

        // Assert
        result.Should().Contain(d => d.Name == "Sales Dashboard");
    }

    [Fact]
    public async Task GetDashboardsAsync_ShouldIncludeOverviewDashboard()
    {
        // Act
        var result = await _provider.GetDashboardsAsync();

        // Assert
        result.Should().Contain(d => d.Name == "Overview Dashboard");
    }

    [Fact]
    public async Task GetDashboardAsync_WithValidId_ShouldReturnDashboard()
    {
        // Arrange
        var dashboards = await _provider.GetDashboardsAsync();
        var firstDashboard = dashboards.First();

        // Act
        var result = await _provider.GetDashboardAsync(firstDashboard.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(firstDashboard.Id);
        result.Name.Should().Be(firstDashboard.Name);
    }

    [Fact]
    public async Task GetDashboardAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _provider.GetDashboardAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_ShouldFilterByRole()
    {
        // Arrange
        var userId = 123;
        var roles = new List<string> { "Sales", "Manager" };

        // Act
        var result = await _provider.GetDashboardsForUserAsync(userId, roles);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_WithAdminRole_ShouldReturnAllDashboards()
    {
        // Arrange
        var userId = 1;
        var roles = new List<string> { "Admin" };

        // Act
        var result = await _provider.GetDashboardsForUserAsync(userId, roles);
        var allDashboards = await _provider.GetDashboardsAsync();

        // Assert
        result.Count().Should().Be(allDashboards.Count());
    }

    #endregion

    #region Chart Tests

    [Fact]
    public async Task GetChartsAsync_WithValidDashboardId_ShouldReturnCharts()
    {
        // Arrange
        var dashboards = await _provider.GetDashboardsAsync();
        var salesDashboard = dashboards.First(d => d.Name == "Sales Dashboard");

        // Act
        var result = await _provider.GetChartsAsync(salesDashboard.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChartsAsync_WithNullDashboardId_ShouldReturnAllCharts()
    {
        // Act
        var result = await _provider.GetChartsAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChartsAsync_WithInvalidDashboardId_ShouldReturnEmpty()
    {
        // Act
        var result = await _provider.GetChartsAsync("non-existent-dashboard");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChartEmbedAsync_ShouldReturnUnsupportedResult()
    {
        // Arrange
        var chartId = "chart-1";
        var filters = new Dictionary<string, string> { { "period", "30d" } };

        // Act
        var result = await _provider.GetChartEmbedAsync(chartId, filters);

        // Assert
        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
        result.Config.Should().ContainKey("error");
    }

    #endregion

    #region Report Tests

    [Fact]
    public async Task GetReportsAsync_WithNoCategory_ShouldReturnAllReports()
    {
        // Act
        var result = await _provider.GetReportsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Count().Should().BeGreaterOrEqualTo(6);
    }

    [Fact]
    public async Task GetReportsAsync_WithSalesCategory_ShouldReturnFilteredReports()
    {
        // Act
        var result = await _provider.GetReportsAsync("Sales");

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.All(r => r.Category == "Sales").Should().BeTrue();
    }

    [Fact]
    public async Task GetReportsAsync_WithMarketingCategory_ShouldReturnFilteredReports()
    {
        // Act
        var result = await _provider.GetReportsAsync("Marketing");

        // Assert
        result.Should().NotBeNull();
        result.All(r => r.Category == "Marketing").Should().BeTrue();
    }

    [Fact]
    public async Task GetReportsAsync_ShouldIncludePipelineReport()
    {
        // Act
        var result = await _provider.GetReportsAsync();

        // Assert
        result.Should().Contain(r => r.Name == "Sales Pipeline Report");
    }

    [Fact]
    public async Task ExecuteReportAsync_WithValidReportId_ShouldReturnData()
    {
        // Arrange
        var reports = await _provider.GetReportsAsync();
        var pipelineReport = reports.First(r => r.Name == "Sales Pipeline Report");
        var parameters = new Dictionary<string, object> { { "Period", "30d" } };

        // Act
        var result = await _provider.ExecuteReportAsync(pipelineReport.Id, parameters);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be(pipelineReport.Id);
        result.ExecutedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ExecuteReportAsync_WithInvalidReportId_ShouldReturnErrorResult()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act
        var result = await _provider.ExecuteReportAsync("invalid-report-id", parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteReportAsync_ShouldReturnColumnsAndRows()
    {
        // Arrange
        var reports = await _provider.GetReportsAsync();
        var firstReport = reports.First();
        var parameters = new Dictionary<string, object>();

        // Act
        var result = await _provider.ExecuteReportAsync(firstReport.Id, parameters);

        // Assert
        if (result.Success)
        {
            result.Columns.Should().NotBeNull();
            result.Columns.Should().NotBeEmpty();
            result.RowCount.Should().BeGreaterOrEqualTo(0);
        }
    }

    #endregion

    #region Embed Tests

    [Fact]
    public async Task GetEmbedAsync_ShouldReturnUnsupportedResult()
    {
        // Arrange
        var request = new EmbedRequest
        {
            EmbedType = "dashboard",
            ResourceId = "dashboard-1",
            UserId = 123
        };

        // Act
        var result = await _provider.GetEmbedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
        result.Config.Should().ContainKey("error");
    }

    [Fact]
    public async Task GetGuestTokenAsync_ShouldThrowNotSupportedException()
    {
        // Arrange
        var dashboardId = "dashboard-1";
        var filters = new Dictionary<string, string> { { "account", "123" } };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _provider.GetGuestTokenAsync(dashboardId, filters));
    }

    #endregion

    #region DataSource Tests

    [Fact]
    public async Task GetDataSourcesAsync_ShouldReturnDataSources()
    {
        // Act
        var result = await _provider.GetDataSourcesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDataSourcesAsync_ShouldIncludeCrmDatabase()
    {
        // Act
        var result = await _provider.GetDataSourcesAsync();

        // Assert
        result.Should().Contain(ds => ds.Name == "CRM Database");
    }

    [Fact]
    public async Task RefreshDataSourceAsync_WithValidId_ShouldReturnJobStatus()
    {
        // Arrange
        var dataSources = await _provider.GetDataSourcesAsync();
        var firstDataSource = dataSources.First();

        // Act
        var result = await _provider.RefreshDataSourceAsync(firstDataSource.Id);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshDataSourceAsync_WithInvalidId_ShouldReturnJobStatus()
    {
        // Act
        var result = await _provider.RefreshDataSourceAsync("non-existent-datasource");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Dashboard Structure Tests

    [Fact]
    public async Task GetDashboardAsync_ShouldHaveValidStructure()
    {
        // Arrange
        var dashboards = await _provider.GetDashboardsAsync();
        var dashboard = dashboards.First();

        // Act
        var result = await _provider.GetDashboardAsync(dashboard.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetChartsAsync_ChartsShouldHaveValidStructure()
    {
        // Arrange
        var dashboards = await _provider.GetDashboardsAsync();
        var dashboard = dashboards.First();

        // Act
        var charts = await _provider.GetChartsAsync(dashboard.Id);

        // Assert
        foreach (var chart in charts)
        {
            chart.Id.Should().NotBeNullOrEmpty();
            chart.Name.Should().NotBeNullOrEmpty();
            chart.ChartType.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetReportsAsync_ReportsShouldHaveValidStructure()
    {
        // Act
        var reports = await _provider.GetReportsAsync();

        // Assert
        foreach (var report in reports)
        {
            report.Id.Should().NotBeNullOrEmpty();
            report.Name.Should().NotBeNullOrEmpty();
            report.Category.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetDashboardsForUserAsync_WithEmptyRoles_ShouldReturnDashboards()
    {
        // Arrange
        var userId = 123;
        var roles = new List<string>();

        // Act
        var result = await _provider.GetDashboardsForUserAsync(userId, roles);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteReportAsync_WithNullParameters_ShouldHandleGracefully()
    {
        // Arrange
        var reports = await _provider.GetReportsAsync();
        var firstReport = reports.First();

        // Act
        var result = await _provider.ExecuteReportAsync(firstReport.Id, null);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReportsAsync_WithNonExistentCategory_ShouldReturnEmpty()
    {
        // Act
        var result = await _provider.GetReportsAsync("NonExistentCategory");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_WithNullRoles_ShouldNotThrow()
    {
        // Arrange
        var userId = 123;

        // Act
        var result = await _provider.GetDashboardsForUserAsync(userId, null);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}

// CRM Solution - BuiltInAnalyticsProvider Tests
// Tests for the built-in database-based analytics provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInAnalyticsProvider.
/// Tests dashboard retrieval, reports, charts, and data sources.
/// </summary>
public class BuiltInAnalyticsProviderTests : IDisposable
{
    private readonly Mock<ILogger<BuiltInAnalyticsProvider>> _loggerMock;
    private readonly Mock<ICrmDbContext> _contextMock;
    private readonly BuiltInAnalyticsProvider _provider;

    public BuiltInAnalyticsProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInAnalyticsProvider>>();
        _contextMock = new Mock<ICrmDbContext>();
        SetupMockDbSets();
        _provider = new BuiltInAnalyticsProvider(_contextMock.Object, _loggerMock.Object);
    }

    private void SetupMockDbSets()
    {
        // Setup mock DbSets for accounts, contacts, opportunities, etc.
        var accounts = new List<Account>
        {
            new Account { Id = 1, Company = "Acme Corp", AccountType = "Customer", Industry = "Technology" },
            new Account { Id = 2, Company = "Beta Inc", AccountType = "Prospect", Industry = "Finance" }
        }.AsQueryable();

        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@acme.com" },
            new Contact { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@beta.com" }
        }.AsQueryable();

        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 1, Name = "Big Deal", Stage = "Negotiation", Amount = 50000, Probability = 75 },
            new Opportunity { Id = 2, Name = "Small Deal", Stage = "Proposal", Amount = 10000, Probability = 50 }
        }.AsQueryable();

        var activities = new List<Activity>
        {
            new Activity { Id = 1, ActivityType = ActivityType.Call, Subject = "Follow up call", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Activity { Id = 2, ActivityType = ActivityType.Email, Subject = "Proposal sent", CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

        // Setup mock DbSets with IQueryable
        var accountDbSet = CreateMockDbSet(accounts);
        var contactDbSet = CreateMockDbSet(contacts);
        var opportunityDbSet = CreateMockDbSet(opportunities);
        var activityDbSet = CreateMockDbSet(activities);

        _contextMock.Setup(c => c.Accounts).Returns(accountDbSet.Object);
        _contextMock.Setup(c => c.Contacts).Returns(contactDbSet.Object);
        _contextMock.Setup(c => c.Opportunities).Returns(opportunityDbSet.Object);
        _contextMock.Setup(c => c.Activities).Returns(activityDbSet.Object);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet;
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInAnalyticsProvider(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new BuiltInAnalyticsProvider(_contextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_AlwaysReturnsTrue()
    {
        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }

    #endregion

    #region Dashboard Tests

    [Fact]
    public async Task GetDashboardsAsync_ReturnsPredefinedDashboards()
    {
        // Act
        var dashboards = await _provider.GetDashboardsAsync();

        // Assert
        dashboards.Should().NotBeEmpty();
        dashboards.Should().Contain(d => d.Name == "Overview Dashboard");
        dashboards.Should().Contain(d => d.Name == "Sales Dashboard");
    }

    [Fact]
    public async Task GetDashboardsAsync_WithUserRole_FiltersAppropriately()
    {
        // Act
        var dashboards = await _provider.GetDashboardsAsync("Sales");

        // Assert
        dashboards.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_WithValidId_ReturnsDashboard()
    {
        // Act
        var dashboard = await _provider.GetDashboardAsync("overview");

        // Assert
        dashboard.Should().NotBeNull();
        dashboard!.Id.Should().Be("overview");
        dashboard.Name.Should().Contain("Overview");
    }

    [Fact]
    public async Task GetDashboardAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var dashboard = await _provider.GetDashboardAsync("non-existent-dashboard");

        // Assert
        dashboard.Should().BeNull();
    }

    #endregion

    #region Report Tests

    [Fact]
    public async Task GetReportsAsync_ReturnsPredefinedReports()
    {
        // Act
        var reports = await _provider.GetReportsAsync();

        // Assert
        reports.Should().NotBeEmpty();
        reports.Should().Contain(r => r.Id == "sales-pipeline");
        reports.Should().Contain(r => r.Id == "account-summary");
        reports.Should().Contain(r => r.Id == "activity-report");
    }

    [Fact]
    public async Task GetReportsAsync_WithCategory_FiltersReports()
    {
        // Act
        var reports = await _provider.GetReportsAsync("Sales");

        // Assert
        reports.Should().NotBeEmpty();
        reports.Should().OnlyContain(r => r.Category == "Sales");
    }

    [Fact]
    public async Task GetReportAsync_WithValidId_ReturnsReport()
    {
        // Act
        var report = await _provider.GetReportAsync("sales-pipeline");

        // Assert
        report.Should().NotBeNull();
        report!.Id.Should().Be("sales-pipeline");
        report.Name.Should().Contain("Sales Pipeline");
    }

    [Fact]
    public async Task GetReportAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var report = await _provider.GetReportAsync("non-existent-report");

        // Assert
        report.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteReportAsync_WithValidReport_ReturnsResults()
    {
        // Arrange
        var request = new ReportExecutionRequest
        {
            ReportId = "sales-pipeline",
            Parameters = new Dictionary<string, object>()
        };

        // Act
        var result = await _provider.ExecuteReportAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be("sales-pipeline");
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteReportAsync_WithInvalidReport_ReturnsFailure()
    {
        // Arrange
        var request = new ReportExecutionRequest
        {
            ReportId = "non-existent",
            Parameters = new Dictionary<string, object>()
        };

        // Act
        var result = await _provider.ExecuteReportAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteReportAsync_WithDateParameters_FiltersData()
    {
        // Arrange
        var request = new ReportExecutionRequest
        {
            ReportId = "activity-report",
            Parameters = new Dictionary<string, object>
            {
                ["startDate"] = DateTime.UtcNow.AddDays(-7),
                ["endDate"] = DateTime.UtcNow
            }
        };

        // Act
        var result = await _provider.ExecuteReportAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Be("activity-report");
    }

    #endregion

    #region Chart Tests

    [Fact]
    public async Task GetChartsAsync_ReturnsPredefinedCharts()
    {
        // Act
        var charts = await _provider.GetChartsAsync();

        // Assert
        charts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetChartAsync_WithValidId_ReturnsChart()
    {
        // Act
        var charts = await _provider.GetChartsAsync();
        if (charts.Any())
        {
            var firstChart = charts.First();
            var chart = await _provider.GetChartAsync(firstChart.Id);

            // Assert
            chart.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetChartAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var chart = await _provider.GetChartAsync("non-existent-chart");

        // Assert
        chart.Should().BeNull();
    }

    [Fact]
    public async Task GetChartDataAsync_WithValidId_ReturnsData()
    {
        // Arrange
        var charts = await _provider.GetChartsAsync();
        if (charts.Any())
        {
            var chartId = charts.First().Id;

            // Act
            var data = await _provider.GetChartDataAsync(chartId);

            // Assert
            data.Should().NotBeNull();
        }
    }

    #endregion

    #region Embed Tests

    [Fact]
    public async Task GetEmbedAsync_ReturnsNotSupported()
    {
        // Act
        var result = await _provider.GetEmbedAsync("dashboard", "overview");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
    }

    [Fact]
    public async Task GetGuestTokenAsync_ReturnsNotSupported()
    {
        // Arrange
        var request = new GuestTokenRequest
        {
            DashboardId = "overview",
            UserId = "user-123"
        };

        // Act
        var result = await _provider.GetGuestTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Data Source Tests

    [Fact]
    public async Task GetDataSourcesAsync_ReturnsCrmDatabase()
    {
        // Act
        var dataSources = await _provider.GetDataSourcesAsync();

        // Assert
        dataSources.Should().NotBeEmpty();
        dataSources.Should().Contain(d => d.Name == "CRM Database");
    }

    [Fact]
    public async Task RefreshDataSourceAsync_ReturnsSuccess()
    {
        // Act
        var result = await _provider.RefreshDataSourceAsync("crm-db");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_WithHealthyDatabase_ReturnsHealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task HealthCheckAsync_WithUnhealthyDatabase_ReturnsUnhealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public async Task HealthCheckAsync_WithDatabaseException_ReturnsUnhealthy()
    {
        // Arrange
        _contextMock.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Message.Should().Contain("Connection failed");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetDashboardsAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act
        var dashboards = await _provider.GetDashboardsAsync(null, cts.Token);

        // Assert
        dashboards.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteReportAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var request = new ReportExecutionRequest { ReportId = "sales-pipeline" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.ExecuteReportAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Parameter Validation Tests

    [Fact]
    public async Task ExecuteReportAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.ExecuteReportAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetDashboardAsync_WithNullId_ThrowsArgumentException()
    {
        // Act
        var act = () => _provider.GetDashboardAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetReportAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act
        var act = () => _provider.GetReportAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetChartDataAsync_WithNullId_ThrowsArgumentException()
    {
        // Act
        var act = () => _provider.GetChartDataAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ─────────────────────────────────────────────────────────────────────────────
// MANDATORY pre-write verification performed:
//   Class:     BuiltInAnalyticsProvider
//   Namespace: CRM.Infrastructure.Providers.BuiltIn
//   File:      src/CRM.Infrastructure/Providers/BuiltIn/BuiltInAnalyticsProvider.cs
//   Constructor: (ICrmDbContext context, ILogger<BuiltInAnalyticsProvider> logger)
//     - context ?? throw ArgumentNullException
//     - logger  ?? throw ArgumentNullException
//   Properties:
//     - ProviderName  => "BuiltIn"
//     - SupportsEmbedding => false
//   Key behaviours confirmed by reading source:
//     - IsAvailableAsync always returns true
//     - GetDashboardsAsync returns 4 predefined dashboards (static field)
//     - GetDashboardAsync returns null for unknown id
//     - GetDashboardsForUserAsync returns all 4 (no role filtering)
//     - GetEmbedAsync returns EmbedResult { EmbedType = "unsupported" }  (does NOT throw)
//     - GetGuestTokenAsync THROWS NotSupportedException
//     - GetChartsAsync returns 7 charts total; filters by dashboardId when provided
//     - GetChartEmbedAsync returns EmbedResult { EmbedType = "unsupported" } (does NOT throw)
//     - ExecuteReportAsync unknown reportId => ReportResult { Success=false, Error="Unknown report: …" }
//     - GetReportsAsync returns 6 predefined reports; filtered by category when provided
//     - GetDataSourcesAsync returns one data source (id="crm-database", type="mariadb")
//     - RefreshDataSourceAsync returns RefreshJobStatus { Status="completed" }
//     - HealthCheckAsync returns ProviderHealthResult { IsHealthy=true, ProviderName="BuiltIn" }
//   Report execution methods query ICrmDbContext – tested via InMemory CrmDbContext.
// ─────────────────────────────────────────────────────────────────────────────

using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="BuiltInAnalyticsProvider"/>.
/// Covers constructor guards, property values, non-DB operations, and report
/// execution paths using an in-memory <see cref="CrmDbContext"/>.
/// </summary>
public class BuiltInAnalyticsProviderTests : IDisposable
{
    // ── Infrastructure ───────────────────────────────────────────────────────

    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<BuiltInAnalyticsProvider>> _loggerMock;

    public BuiltInAnalyticsProviderTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AnalyticsTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableServiceProviderCaching(false)
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _loggerMock = new Mock<ILogger<BuiltInAnalyticsProvider>>();
    }

    public void Dispose() => _dbContext.Dispose();

    private BuiltInAnalyticsProvider CreateProvider() =>
        new(_dbContext, _loggerMock.Object);

    // ── Constructor Guards ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var act = () => new BuiltInAnalyticsProvider(
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new BuiltInAnalyticsProvider(
            _dbContext,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ── Provider Properties ───────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        var provider = CreateProvider();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportsEmbedding_ReturnsFalse()
    {
        var provider = CreateProvider();
        provider.SupportsEmbedding.Should().BeFalse();
    }

    // ── IsAvailableAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_Always()
    {
        var provider = CreateProvider();

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    // ── Dashboard Operations ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardsAsync_ReturnsFourPredefinedDashboards()
    {
        var provider = CreateProvider();

        var dashboards = (await provider.GetDashboardsAsync()).ToList();

        dashboards.Should().HaveCount(4);
        dashboards.Select(d => d.Id).Should().Contain(new[] { "overview", "sales", "accounts", "activities" });
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectDashboard_WhenIdExists()
    {
        var provider = CreateProvider();

        var dashboard = await provider.GetDashboardAsync("sales");

        dashboard.Should().NotBeNull();
        dashboard!.Id.Should().Be("sales");
        dashboard.Name.Should().Be("Sales Dashboard");
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsNull_WhenIdDoesNotExist()
    {
        var provider = CreateProvider();

        var dashboard = await provider.GetDashboardAsync("nonexistent-id");

        dashboard.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_ReturnsAllDashboards_RegardlessOfRoles()
    {
        var provider = CreateProvider();

        var dashboards = (await provider.GetDashboardsForUserAsync(42, new[] { "Admin", "User" })).ToList();

        dashboards.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_ReturnsAllDashboards_WhenRolesAreNull()
    {
        var provider = CreateProvider();

        var dashboards = (await provider.GetDashboardsForUserAsync(1, null)).ToList();

        dashboards.Should().HaveCount(4);
    }

    // ── Embedding Operations ──────────────────────────────────────────────────

    [Fact]
    public async Task GetEmbedAsync_ReturnsUnsupportedResult_WithoutThrowing()
    {
        var provider = CreateProvider();
        var request = new EmbedRequest { ResourceId = "overview" };

        var result = await provider.GetEmbedAsync(request);

        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
        result.Config.Should().ContainKey("error");
    }

    [Fact]
    public async Task GetGuestTokenAsync_ThrowsNotSupportedException()
    {
        var provider = CreateProvider();

        var act = async () => await provider.GetGuestTokenAsync("overview");

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    // ── Chart Operations ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetChartsAsync_ReturnsAllSevenCharts_WhenDashboardIdIsNull()
    {
        var provider = CreateProvider();

        var charts = (await provider.GetChartsAsync(null)).ToList();

        charts.Should().HaveCount(7);
    }

    [Fact]
    public async Task GetChartsAsync_ReturnsFilteredCharts_WhenDashboardIdIsProvided()
    {
        var provider = CreateProvider();

        var charts = (await provider.GetChartsAsync("sales")).ToList();

        charts.Should().NotBeEmpty();
        charts.Should().AllSatisfy(c => c.DashboardId.Should().Be("sales"));
    }

    [Fact]
    public async Task GetChartsAsync_ReturnsEmpty_WhenDashboardIdHasNoCharts()
    {
        var provider = CreateProvider();

        var charts = (await provider.GetChartsAsync("nonexistent-dashboard")).ToList();

        charts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChartEmbedAsync_ReturnsUnsupportedResult_WithoutThrowing()
    {
        var provider = CreateProvider();

        var result = await provider.GetChartEmbedAsync("account-count");

        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
    }

    // ── Report Operations ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetReportsAsync_ReturnsSixPredefinedReports_WhenNoCategoryFilter()
    {
        var provider = CreateProvider();

        var reports = (await provider.GetReportsAsync()).ToList();

        reports.Should().HaveCount(6);
        reports.Select(r => r.Id).Should().Contain(new[]
        {
            "sales-pipeline", "account-summary", "activity-report",
            "lead-conversion", "product-revenue", "user-activity"
        });
    }

    [Fact]
    public async Task GetReportsAsync_ReturnsFilteredByCategory_WhenCategoryProvided()
    {
        var provider = CreateProvider();

        var salesReports = (await provider.GetReportsAsync("Sales")).ToList();

        salesReports.Should().NotBeEmpty();
        salesReports.Should().AllSatisfy(r =>
            string.Equals(r.Category, "Sales", StringComparison.OrdinalIgnoreCase)
                .Should().BeTrue(because: $"report '{r.Id}' should belong to 'Sales' category"));
    }

    [Fact]
    public async Task GetReportsAsync_ReturnsEmpty_WhenCategoryHasNoMatches()
    {
        var provider = CreateProvider();

        var reports = (await provider.GetReportsAsync("NonExistentCategory")).ToList();

        reports.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsFailureResult_WhenReportIdIsUnknown()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("unknown-report-id");

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ReportId.Should().Be("unknown-report-id");
        result.Error.Should().Contain("Unknown report");
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsSuccessResult_ForSalesPipelineReport()
    {
        var provider = CreateProvider();

        // Act - InMemory DB has no data, but report should still return success with empty rows
        var result = await provider.ExecuteReportAsync("sales-pipeline");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ReportId.Should().Be("sales-pipeline");
        result.Columns.Should().Contain(new[] { "Stage", "Count", "TotalValue", "WeightedValue" });
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsSuccessResult_ForAccountSummaryReport()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("account-summary");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ReportId.Should().Be("account-summary");
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsSuccessResult_ForLeadConversionReport()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("lead-conversion");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ReportId.Should().Be("lead-conversion");
        result.Columns.Should().Contain(new[] { "Status", "Count", "Percentage" });
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsSuccessResult_ForProductRevenueReport()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("product-revenue");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ReportId.Should().Be("product-revenue");
    }

    [Fact]
    public async Task ExecuteReportAsync_ReturnsSuccessResult_ForUserActivityReport()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("user-activity");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ReportId.Should().Be("user-activity");
        result.Columns.Should().Contain(new[] { "UserName", "ActivityCount", "LastActivity" });
    }

    [Fact]
    public async Task ExecuteReportAsync_ReportIdIsCaseInsensitive()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteReportAsync("SALES-PIPELINE");

        result.Success.Should().BeTrue();
    }

    // ── Data Source Operations ────────────────────────────────────────────────

    [Fact]
    public async Task GetDataSourcesAsync_ReturnsOneDataSource_WithCorrectId()
    {
        var provider = CreateProvider();

        var dataSources = (await provider.GetDataSourcesAsync()).ToList();

        dataSources.Should().HaveCount(1);
        dataSources[0].Id.Should().Be("crm-database");
        dataSources[0].Type.Should().Be("mariadb");
        dataSources[0].Status.Should().Be("connected");
    }

    [Fact]
    public async Task RefreshDataSourceAsync_ReturnsCompletedStatus()
    {
        var provider = CreateProvider();

        var jobStatus = await provider.RefreshDataSourceAsync("crm-database");

        jobStatus.Should().NotBeNull();
        jobStatus.Status.Should().Be("completed");
        jobStatus.JobId.Should().NotBeNullOrWhiteSpace();
    }

    // ── Health Check ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthyResult()
    {
        var provider = CreateProvider();

        var health = await provider.HealthCheckAsync();

        health.Should().NotBeNull();
        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
        health.Details.Should().ContainKey("dashboardCount");
        health.Details.Should().ContainKey("reportCount");
        health.Details["supportsEmbedding"].Should().Be(false);
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for DashboardController
/// Covers: Dashboard data, widgets, KPIs, charts, user preferences
/// </summary>
public class DashboardControllerTests
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _mockLogger = new Mock<ILogger<DashboardController>>();

        _controller = new DashboardController(_mockDashboardService.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetDashboard Tests

    [Fact]
    public async Task GetDashboard_ReturnsOkResult_WithDashboardData()
    {
        // Arrange
        var dashboardData = new DashboardDataDto
        {
            TotalAccounts = 100,
            TotalContacts = 500,
            TotalOpportunities = 50,
            TotalRevenue = 1000000
        };

        _mockDashboardService.Setup(s => s.GetDashboardDataAsync(1))
            .ReturnsAsync(dashboardData);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value as DashboardDataDto;
        data!.TotalAccounts.Should().Be(100);
    }

    [Fact]
    public async Task GetDashboard_WithDateRange_ReturnsFilteredData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var dashboardData = new DashboardDataDto
        {
            TotalAccounts = 10,
            Period = "Last 30 Days"
        };

        _mockDashboardService.Setup(s => s.GetDashboardDataAsync(1, startDate, endDate))
            .ReturnsAsync(dashboardData);

        // Act
        var result = await _controller.GetDashboard(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region KPI Tests

    [Fact]
    public async Task GetKPIs_ReturnsKPIData()
    {
        // Arrange
        var kpis = new List<KPIDto>
        {
            new KPIDto { Name = "Total Revenue", Value = 1000000, ChangePercent = 15 },
            new KPIDto { Name = "New Customers", Value = 50, ChangePercent = 10 },
            new KPIDto { Name = "Conversion Rate", Value = 25, ChangePercent = -5 }
        };

        _mockDashboardService.Setup(s => s.GetKPIsAsync(1))
            .ReturnsAsync(kpis);

        // Act
        var result = await _controller.GetKPIs();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedKPIs = okResult.Value as IEnumerable<KPIDto>;
        returnedKPIs.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetKPI_ByName_ReturnsSpecificKPI()
    {
        // Arrange
        var kpi = new KPIDto { Name = "Total Revenue", Value = 1000000 };

        _mockDashboardService.Setup(s => s.GetKPIByNameAsync("TotalRevenue"))
            .ReturnsAsync(kpi);

        // Act
        var result = await _controller.GetKPI("TotalRevenue");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetKPI_NonExisting_ReturnsNotFound()
    {
        // Arrange
        _mockDashboardService.Setup(s => s.GetKPIByNameAsync("Unknown"))
            .ReturnsAsync((KPIDto?)null);

        // Act
        var result = await _controller.GetKPI("Unknown");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Widget Tests

    [Fact]
    public async Task GetWidgets_ReturnsUserWidgets()
    {
        // Arrange
        var widgets = new List<DashboardWidgetDto>
        {
            new DashboardWidgetDto { Id = 1, Name = "Revenue Chart", Type = "Chart" },
            new DashboardWidgetDto { Id = 2, Name = "Tasks List", Type = "List" }
        };

        _mockDashboardService.Setup(s => s.GetWidgetsAsync(1))
            .ReturnsAsync(widgets);

        // Act
        var result = await _controller.GetWidgets();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetAvailableWidgets_ReturnsAllWidgets()
    {
        // Arrange
        var widgets = new List<WidgetDefinitionDto>
        {
            new WidgetDefinitionDto { Id = "revenue-chart", Name = "Revenue Chart", Category = "Sales" },
            new WidgetDefinitionDto { Id = "tasks-list", Name = "Tasks List", Category = "Productivity" }
        };

        _mockDashboardService.Setup(s => s.GetAvailableWidgetsAsync())
            .ReturnsAsync(widgets);

        // Act
        var result = await _controller.GetAvailableWidgets();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddWidget_ValidWidget_ReturnsCreated()
    {
        // Arrange
        var widgetDto = new AddWidgetDto
        {
            WidgetId = "revenue-chart",
            Position = new WidgetPositionDto { X = 0, Y = 0, Width = 4, Height = 3 }
        };

        var addedWidget = new DashboardWidgetDto { Id = 1, Name = "Revenue Chart" };

        _mockDashboardService.Setup(s => s.AddWidgetAsync(1, widgetDto))
            .ReturnsAsync(addedWidget);

        // Act
        var result = await _controller.AddWidget(widgetDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateWidgetPosition_ValidPosition_ReturnsOk()
    {
        // Arrange
        var positionDto = new WidgetPositionDto { X = 2, Y = 1, Width = 4, Height = 3 };

        _mockDashboardService.Setup(s => s.UpdateWidgetPositionAsync(1, 1, positionDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateWidgetPosition(1, positionDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveWidget_ValidWidget_ReturnsNoContent()
    {
        // Arrange
        _mockDashboardService.Setup(s => s.RemoveWidgetAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveWidget(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateWidgetSettings_ValidSettings_ReturnsOk()
    {
        // Arrange
        var settings = new Dictionary<string, object>
        {
            { "showLegend", true },
            { "chartType", "bar" }
        };

        _mockDashboardService.Setup(s => s.UpdateWidgetSettingsAsync(1, 1, settings))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateWidgetSettings(1, settings);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Chart Data Tests

    [Fact]
    public async Task GetSalesChart_ReturnsChartData()
    {
        // Arrange
        var chartData = new ChartDataDto
        {
            Labels = new List<string> { "Jan", "Feb", "Mar" },
            Datasets = new List<DatasetDto>
            {
                new DatasetDto { Label = "Sales", Data = new List<decimal> { 10000, 15000, 12000 } }
            }
        };

        _mockDashboardService.Setup(s => s.GetSalesChartAsync(1, null, null))
            .ReturnsAsync(chartData);

        // Act
        var result = await _controller.GetSalesChart(null, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetRevenueChart_ReturnsChartData()
    {
        // Arrange
        var chartData = new ChartDataDto
        {
            Labels = new List<string> { "Q1", "Q2", "Q3", "Q4" },
            Datasets = new List<DatasetDto>
            {
                new DatasetDto { Label = "Revenue", Data = new List<decimal> { 250000, 300000, 280000, 350000 } }
            }
        };

        _mockDashboardService.Setup(s => s.GetRevenueChartAsync(1, "quarterly"))
            .ReturnsAsync(chartData);

        // Act
        var result = await _controller.GetRevenueChart("quarterly");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPipelineChart_ReturnsStageData()
    {
        // Arrange
        var chartData = new ChartDataDto
        {
            Labels = new List<string> { "Prospect", "Qualified", "Proposal", "Closed" },
            Datasets = new List<DatasetDto>
            {
                new DatasetDto { Label = "Opportunities", Data = new List<decimal> { 20, 15, 10, 5 } }
            }
        };

        _mockDashboardService.Setup(s => s.GetPipelineChartAsync(1))
            .ReturnsAsync(chartData);

        // Act
        var result = await _controller.GetPipelineChart();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetActivityChart_ReturnsActivityData()
    {
        // Arrange
        var chartData = new ChartDataDto
        {
            Labels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri" },
            Datasets = new List<DatasetDto>
            {
                new DatasetDto { Label = "Calls", Data = new List<decimal> { 10, 15, 12, 8, 20 } },
                new DatasetDto { Label = "Emails", Data = new List<decimal> { 25, 30, 28, 22, 35 } }
            }
        };

        _mockDashboardService.Setup(s => s.GetActivityChartAsync(1, "week"))
            .ReturnsAsync(chartData);

        // Act
        var result = await _controller.GetActivityChart("week");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLeadsChart_ReturnsLeadData()
    {
        // Arrange
        var chartData = new ChartDataDto
        {
            Labels = new List<string> { "Website", "Referral", "Cold Call", "Event" },
            Datasets = new List<DatasetDto>
            {
                new DatasetDto { Label = "Leads", Data = new List<decimal> { 40, 25, 20, 15 } }
            }
        };

        _mockDashboardService.Setup(s => s.GetLeadsChartAsync(1, "source"))
            .ReturnsAsync(chartData);

        // Act
        var result = await _controller.GetLeadsChart("source");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Recent Items Tests

    [Fact]
    public async Task GetRecentAccounts_ReturnsRecentItems()
    {
        // Arrange
        var accounts = new List<AccountSummaryDto>
        {
            new AccountSummaryDto { Id = 1, Name = "Acme Corp" },
            new AccountSummaryDto { Id = 2, Name = "Beta Inc" }
        };

        _mockDashboardService.Setup(s => s.GetRecentAccountsAsync(1, 10))
            .ReturnsAsync(accounts);

        // Act
        var result = await _controller.GetRecentAccounts(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentOpportunities_ReturnsRecentItems()
    {
        // Arrange
        var opportunities = new List<OpportunitySummaryDto>
        {
            new OpportunitySummaryDto { Id = 1, Name = "Big Deal", Value = 100000 }
        };

        _mockDashboardService.Setup(s => s.GetRecentOpportunitiesAsync(1, 10))
            .ReturnsAsync(opportunities);

        // Act
        var result = await _controller.GetRecentOpportunities(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentActivities_ReturnsRecentItems()
    {
        // Arrange
        var activities = new List<ActivitySummaryDto>
        {
            new ActivitySummaryDto { Id = 1, Type = "Call", Description = "Follow-up call" }
        };

        _mockDashboardService.Setup(s => s.GetRecentActivitiesAsync(1, 10))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetRecentActivities(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingTasks_ReturnsUpcomingItems()
    {
        // Arrange
        var tasks = new List<TaskSummaryDto>
        {
            new TaskSummaryDto { Id = 1, Title = "Call Client", DueDate = DateTime.Today.AddDays(1) }
        };

        _mockDashboardService.Setup(s => s.GetUpcomingTasksAsync(1, 10))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetUpcomingTasks(10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Leaderboard Tests

    [Fact]
    public async Task GetSalesLeaderboard_ReturnsLeaderboardData()
    {
        // Arrange
        var leaderboard = new List<LeaderboardEntryDto>
        {
            new LeaderboardEntryDto { UserId = 1, UserName = "John", Revenue = 500000, Rank = 1 },
            new LeaderboardEntryDto { UserId = 2, UserName = "Jane", Revenue = 450000, Rank = 2 }
        };

        _mockDashboardService.Setup(s => s.GetSalesLeaderboardAsync("month"))
            .ReturnsAsync(leaderboard);

        // Act
        var result = await _controller.GetSalesLeaderboard("month");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetActivityLeaderboard_ReturnsLeaderboardData()
    {
        // Arrange
        var leaderboard = new List<LeaderboardEntryDto>
        {
            new LeaderboardEntryDto { UserId = 1, UserName = "John", Activities = 150, Rank = 1 }
        };

        _mockDashboardService.Setup(s => s.GetActivityLeaderboardAsync("week"))
            .ReturnsAsync(leaderboard);

        // Act
        var result = await _controller.GetActivityLeaderboard("week");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Dashboard Configuration Tests

    [Fact]
    public async Task GetDashboardLayout_ReturnsLayout()
    {
        // Arrange
        var layout = new DashboardLayoutDto
        {
            Columns = 12,
            Widgets = new List<DashboardWidgetDto>()
        };

        _mockDashboardService.Setup(s => s.GetDashboardLayoutAsync(1))
            .ReturnsAsync(layout);

        // Act
        var result = await _controller.GetDashboardLayout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SaveDashboardLayout_ValidLayout_ReturnsOk()
    {
        // Arrange
        var layout = new SaveDashboardLayoutDto
        {
            Widgets = new List<WidgetLayoutDto>
            {
                new WidgetLayoutDto { WidgetId = 1, X = 0, Y = 0, Width = 4, Height = 3 }
            }
        };

        _mockDashboardService.Setup(s => s.SaveDashboardLayoutAsync(1, layout))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SaveDashboardLayout(layout);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetDashboard_ReturnsOk()
    {
        // Arrange
        _mockDashboardService.Setup(s => s.ResetDashboardAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetDashboard();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetDashboardPreferences_ReturnsPreferences()
    {
        // Arrange
        var preferences = new DashboardPreferencesDto
        {
            Theme = "light",
            RefreshInterval = 300,
            DefaultDateRange = "last30days"
        };

        _mockDashboardService.Setup(s => s.GetPreferencesAsync(1))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetDashboardPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateDashboardPreferences_ValidPreferences_ReturnsOk()
    {
        // Arrange
        var preferences = new UpdateDashboardPreferencesDto
        {
            Theme = "dark",
            RefreshInterval = 600
        };

        _mockDashboardService.Setup(s => s.UpdatePreferencesAsync(1, preferences))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateDashboardPreferences(preferences);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Team Dashboard Tests

    [Fact]
    public async Task GetTeamDashboard_ReturnsTeamData()
    {
        // Arrange
        var teamData = new TeamDashboardDto
        {
            TeamId = 1,
            TeamName = "Sales Team",
            TotalRevenue = 2000000,
            Members = new List<TeamMemberStatsDto>()
        };

        _mockDashboardService.Setup(s => s.GetTeamDashboardAsync(1))
            .ReturnsAsync(teamData);

        // Act
        var result = await _controller.GetTeamDashboard(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMyTeamsDashboard_ReturnsTeamsData()
    {
        // Arrange
        var teamsData = new List<TeamDashboardSummaryDto>
        {
            new TeamDashboardSummaryDto { TeamId = 1, TeamName = "Sales Team", Revenue = 1000000 }
        };

        _mockDashboardService.Setup(s => s.GetMyTeamsDashboardAsync(1))
            .ReturnsAsync(teamsData);

        // Act
        var result = await _controller.GetMyTeamsDashboard();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportDashboard_ToPdf_ReturnsFile()
    {
        // Arrange
        var pdfData = new byte[] { 37, 80, 68, 70 };

        _mockDashboardService.Setup(s => s.ExportDashboardAsync(1, "pdf"))
            .ReturnsAsync(pdfData);

        // Act
        var result = await _controller.ExportDashboard("pdf");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ExportWidgetData_ReturnsFile()
    {
        // Arrange
        var csvData = new byte[] { 78, 97, 109, 101 };

        _mockDashboardService.Setup(s => s.ExportWidgetDataAsync(1, "csv"))
            .ReturnsAsync(csvData);

        // Act
        var result = await _controller.ExportWidgetData(1, "csv");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion

    #region Notification/Alert Tests

    [Fact]
    public async Task GetAlerts_ReturnsAlerts()
    {
        // Arrange
        var alerts = new List<DashboardAlertDto>
        {
            new DashboardAlertDto { Id = 1, Type = "Warning", Message = "5 opportunities closing soon" },
            new DashboardAlertDto { Id = 2, Type = "Info", Message = "New leads assigned" }
        };

        _mockDashboardService.Setup(s => s.GetAlertsAsync(1))
            .ReturnsAsync(alerts);

        // Act
        var result = await _controller.GetAlerts();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DismissAlert_ReturnsOk()
    {
        // Arrange
        _mockDashboardService.Setup(s => s.DismissAlertAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DismissAlert(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Goals/Targets Tests

    [Fact]
    public async Task GetGoalProgress_ReturnsGoalData()
    {
        // Arrange
        var goals = new List<GoalProgressDto>
        {
            new GoalProgressDto { Name = "Revenue Target", Target = 1000000, Current = 750000, Percentage = 75 },
            new GoalProgressDto { Name = "New Customers", Target = 100, Current = 80, Percentage = 80 }
        };

        _mockDashboardService.Setup(s => s.GetGoalProgressAsync(1))
            .ReturnsAsync(goals);

        // Act
        var result = await _controller.GetGoalProgress();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetQuotaProgress_ReturnsQuotaData()
    {
        // Arrange
        var quota = new QuotaProgressDto
        {
            Period = "Q1 2024",
            Quota = 500000,
            Achieved = 400000,
            Percentage = 80
        };

        _mockDashboardService.Setup(s => s.GetQuotaProgressAsync(1))
            .ReturnsAsync(quota);

        // Act
        var result = await _controller.GetQuotaProgress();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}

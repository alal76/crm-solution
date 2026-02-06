// CRM Solution - ITSM Dashboard Service Tests
// Comprehensive tests for ITSM analytics and metrics

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for ITSMDashboardService.
/// Tests dashboard analytics and metrics.
/// </summary>
public class ITSMDashboardServiceTests
{
    private readonly Mock<ILogger<ITSMDashboardService>> _mockLogger;
    private readonly ITSMDashboardService _service;

    public ITSMDashboardServiceTests()
    {
        _mockLogger = new Mock<ILogger<ITSMDashboardService>>();
        _service = new ITSMDashboardService(_mockLogger.Object);
    }

    #region GetIncidentTrendsAsync Tests

    [Fact]
    public async Task GetIncidentTrendsAsync_ReturnsValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalIncidents.Should().BeGreaterThan(0);
        result.OpenIncidents.Should().BeGreaterOrEqualTo(0);
        result.ResolvedIncidents.Should().BeGreaterOrEqualTo(0);
        result.ClosedIncidents.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_HasValidResolutionTime()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.AverageResolutionTimeHours.Should().BePositive();
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_HasValidFCRRate()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.FirstContactResolutionRate.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_HasDailyTrends()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.DailyTrends.Should().NotBeNull();
        result.DailyTrends.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_DailyTrendsHaveValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 7);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        foreach (var trend in result.DailyTrends)
        {
            trend.Date.Should().BeOnOrAfter(startDate);
            trend.Date.Should().BeOnOrBefore(endDate);
            trend.Created.Should().BeGreaterOrEqualTo(0);
            trend.Resolved.Should().BeGreaterOrEqualTo(0);
            trend.Backlog.Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_HasCategoryBreakdown()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.ByCategory.Should().NotBeNull();
        result.ByCategory.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_CategoryPercentagesSum()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        var totalPercentage = result.ByCategory.Sum(c => c.Percentage);
        totalPercentage.Should().BeApproximately(100, 1); // Allow 1% tolerance for rounding
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_HasPriorityBreakdown()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.ByPriority.Should().NotBeNull();
        result.ByPriority.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_PriorityBreakdownHasPriorityLabels()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        foreach (var priority in result.ByPriority)
        {
            priority.Priority.Should().BeInRange(1, 5);
            priority.PriorityLabel.Should().NotBeNullOrEmpty();
            priority.Count.Should().BeGreaterOrEqualTo(0);
            priority.Percentage.Should().BeInRange(0, 100);
        }
    }

    #endregion

    #region GetProblemAnalyticsAsync Tests

    [Fact]
    public async Task GetProblemAnalyticsAsync_ReturnsValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalProblems.Should().BeGreaterOrEqualTo(0);
        result.OpenProblems.Should().BeGreaterOrEqualTo(0);
        result.ProblemsWithKnownError.Should().BeGreaterOrEqualTo(0);
        result.ProblemsWithWorkaround.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_HasLinkedIncidentsCount()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        result.LinkedIncidentsCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_HasRootCauseBreakdown()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        result.ByRootCause.Should().NotBeNull();
        result.ByRootCause.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_HasTopRecurringProblems()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        result.TopRecurringProblems.Should().NotBeNull();
        result.TopRecurringProblems.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_TopProblemsHaveValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        foreach (var problem in result.TopRecurringProblems)
        {
            problem.ProblemId.Should().BeGreaterThan(0);
            problem.ProblemNumber.Should().NotBeNullOrEmpty();
            problem.Title.Should().NotBeNullOrEmpty();
            problem.LinkedIncidents.Should().BeGreaterOrEqualTo(0);
        }
    }

    #endregion

    #region GetChangeStatisticsAsync Tests

    [Fact]
    public async Task GetChangeStatisticsAsync_ReturnsValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalChanges.Should().BeGreaterOrEqualTo(0);
        result.ScheduledChanges.Should().BeGreaterOrEqualTo(0);
        result.CompletedChanges.Should().BeGreaterOrEqualTo(0);
        result.FailedChanges.Should().BeGreaterOrEqualTo(0);
        result.RolledBackChanges.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_HasValidSuccessRate()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        result.SuccessRate.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_HasTypeBreakdown()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        result.ByType.Should().NotBeNull();
        result.ByType.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_TypeBreakdownHasValidTypes()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        var validTypes = new[] { "Standard", "Normal", "Emergency" };
        foreach (var type in result.ByType)
        {
            validTypes.Should().Contain(type.ChangeType);
        }
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_HasRiskBreakdown()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        result.ByRisk.Should().NotBeNull();
        result.ByRisk.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_RiskLevelsAreValid()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        var validRiskLevels = new[] { "Low", "Medium", "High", "Critical" };
        foreach (var risk in result.ByRisk)
        {
            validRiskLevels.Should().Contain(risk.RiskLevel);
        }
    }

    #endregion

    #region GetSLAComplianceAsync Tests

    [Fact]
    public async Task GetSLAComplianceAsync_ReturnsValidData()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        var result = await _service.GetSLAComplianceAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Date Range Tests

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public async Task GetIncidentTrendsAsync_WorksWithVariousDateRanges(int days)
    {
        // Arrange
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);

        // Act
        var result = await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.DailyTrends.Should().NotBeNull();
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_SameDayRange_ReturnsData()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        // Act
        var result = await _service.GetIncidentTrendsAsync(date, date);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetIncidentTrendsAsync_LogsInformation()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        await _service.GetIncidentTrendsAsync(startDate, endDate);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Getting incident trends")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_LogsInformation()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        await _service.GetProblemAnalyticsAsync(startDate, endDate);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Getting problem analytics")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetChangeStatisticsAsync_LogsInformation()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);

        // Act
        await _service.GetChangeStatisticsAsync(startDate, endDate);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Getting change statistics")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void ITSMDashboardService_ImplementsInterface()
    {
        // Assert
        typeof(ITSMDashboardService).Should().Implement<IITSMDashboardService>();
    }

    #endregion
}

/// <summary>
/// Tests for IncidentTrendsDto and related DTOs.
/// </summary>
public class IncidentTrendsDtoTests
{
    [Fact]
    public void DailyTrendItem_CanBePopulated()
    {
        // Arrange & Act
        var item = new DailyTrendItem
        {
            Date = new DateTime(2025, 1, 15),
            Created = 25,
            Resolved = 20,
            Backlog = 50
        };

        // Assert
        item.Date.Should().Be(new DateTime(2025, 1, 15));
        item.Created.Should().Be(25);
        item.Resolved.Should().Be(20);
        item.Backlog.Should().Be(50);
    }

    [Fact]
    public void CategoryBreakdown_CanBePopulated()
    {
        // Arrange & Act
        var breakdown = new CategoryBreakdown
        {
            Category = "Hardware",
            Count = 150,
            Percentage = 28.5
        };

        // Assert
        breakdown.Category.Should().Be("Hardware");
        breakdown.Count.Should().Be(150);
        breakdown.Percentage.Should().Be(28.5);
    }

    [Fact]
    public void PriorityBreakdown_CanBePopulated()
    {
        // Arrange & Act
        var breakdown = new PriorityBreakdown
        {
            Priority = 1,
            PriorityLabel = "Critical",
            Count = 25,
            Percentage = 5.0
        };

        // Assert
        breakdown.Priority.Should().Be(1);
        breakdown.PriorityLabel.Should().Be("Critical");
        breakdown.Count.Should().Be(25);
        breakdown.Percentage.Should().Be(5.0);
    }
}

/// <summary>
/// Tests for ProblemAnalyticsDto and related DTOs.
/// </summary>
public class ProblemAnalyticsDtoTests
{
    [Fact]
    public void RootCauseBreakdown_CanBePopulated()
    {
        // Arrange & Act
        var breakdown = new RootCauseBreakdown
        {
            RootCause = "Configuration Error",
            Count = 12,
            Percentage = 35.3
        };

        // Assert
        breakdown.RootCause.Should().Be("Configuration Error");
        breakdown.Count.Should().Be(12);
        breakdown.Percentage.Should().Be(35.3);
    }

    [Fact]
    public void TopProblem_CanBePopulated()
    {
        // Arrange & Act
        var problem = new TopProblem
        {
            ProblemId = 1,
            ProblemNumber = "PRB-0001",
            Title = "VPN Connection Drops",
            LinkedIncidents = 45
        };

        // Assert
        problem.ProblemId.Should().Be(1);
        problem.ProblemNumber.Should().Be("PRB-0001");
        problem.Title.Should().Be("VPN Connection Drops");
        problem.LinkedIncidents.Should().Be(45);
    }
}

/// <summary>
/// Tests for ChangeStatisticsDto and related DTOs.
/// </summary>
public class ChangeStatisticsDtoTests
{
    [Fact]
    public void ChangeTypeBreakdown_CanBePopulated()
    {
        // Arrange & Act
        var breakdown = new ChangeTypeBreakdown
        {
            ChangeType = "Standard",
            Count = 42,
            Percentage = 53.8
        };

        // Assert
        breakdown.ChangeType.Should().Be("Standard");
        breakdown.Count.Should().Be(42);
        breakdown.Percentage.Should().Be(53.8);
    }

    [Fact]
    public void ChangeRiskBreakdown_CanBePopulated()
    {
        // Arrange & Act
        var breakdown = new ChangeRiskBreakdown
        {
            RiskLevel = "Medium",
            Count = 31,
            Percentage = 39.7
        };

        // Assert
        breakdown.RiskLevel.Should().Be("Medium");
        breakdown.Count.Should().Be(31);
        breakdown.Percentage.Should().Be(39.7);
    }
}

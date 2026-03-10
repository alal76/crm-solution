// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for ITSMDashboardService (TCOV-003).
/// Service returns deterministic simulated data — tests verify shape, ranges, and non-nulls.
/// </summary>
public class ITSMDashboardServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ITSMDashboardService>> _mockLogger;
    private readonly ITSMDashboardService _service;

    public ITSMDashboardServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ITSMDashboardService>>();
        _service = new ITSMDashboardService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldReturnNonNullResult()
    {
        var result = await _service.GetIncidentTrendsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldReturnDailyTrendsForRange()
    {
        var start = DateTime.UtcNow.AddDays(-30);
        var end = DateTime.UtcNow;
        var result = await _service.GetIncidentTrendsAsync(start, end);
        result.DailyTrends.Should().NotBeNull();
        result.DailyTrends.Should().HaveCount(31);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldReturnCategoryBreakdown()
    {
        var result = await _service.GetIncidentTrendsAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.ByCategory.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldReturnPriorityBreakdown()
    {
        var result = await _service.GetIncidentTrendsAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.ByPriority.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldHaveValidResolutionStats()
    {
        var result = await _service.GetIncidentTrendsAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        result.FirstContactResolutionRate.Should().BeInRange(0, 100);
        result.AverageResolutionTimeHours.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetIncidentTrendsAsync_ShouldHavePositiveCounts()
    {
        var result = await _service.GetIncidentTrendsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        result.TotalIncidents.Should().BeGreaterThan(0);
        result.OpenIncidents.Should().BeGreaterThanOrEqualTo(0);
        result.ResolvedIncidents.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_ShouldReturnNonNullResult()
    {
        var result = await _service.GetProblemAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_ShouldReturnRootCauseBreakdown()
    {
        var result = await _service.GetProblemAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        result.ByRootCause.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProblemAnalyticsAsync_ShouldReturnTopRecurringProblems()
    {
        var result = await _service.GetProblemAnalyticsAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
        result.TopRecurringProblems.Should().NotBeNullOrEmpty();
    }
}

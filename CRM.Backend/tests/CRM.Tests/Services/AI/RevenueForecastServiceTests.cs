// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for RevenueForecastService (TODO-AI-10).
/// Covers: empty db → zero forecast; ClosedWon this month → in ClosedRevenue; pipeline weighted correctly.
/// </summary>
public class RevenueForecastServiceTests : ServiceTestFixtureBase<RevenueForecastService>
{    private readonly RevenueForecastService _sut;

    public RevenueForecastServiceTests()
    {        _sut = new RevenueForecastService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public async Task ForecastRevenueAsync_ShouldReturnZeroForecast_WhenNoPipelineExists()
    {
        // Arrange
        MockContext.Setup(c => c.Opportunities)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Opportunity>()).Object);

        // Act
        var result = await _sut.ForecastRevenueAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Months.Should().HaveCount(3);
        result.Months.All(m => m.ForecastedRevenue == 0).Should().BeTrue();
        result.TotalForecastedRevenue.Should().Be(0);
    }

    [Fact]
    public async Task ForecastRevenueAsync_ShouldIncludeClosedRevenue_WhenOpportunityIsClosedWonThisMonth()
    {
        // Arrange
        var thisMonthClose = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 15);
        var opps = new List<Opportunity>
        {
            new Opportunity
            {
                Id = 1,
                IsDeleted = false,
                Stage = OpportunityStage.ClosedWon,
                Probability = 100,
                Amount = 10000m,
                ExpectedCloseDate = thisMonthClose
            }
        };
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opps).Object);

        // Act
        var result = await _sut.ForecastRevenueAsync(6);

        // Assert
        var thisMonthKey = thisMonthClose.ToString("yyyy-MM");
        var thisMonthForecast = result.Months.FirstOrDefault(m => m.Month == thisMonthKey);
        thisMonthForecast.Should().NotBeNull();
        thisMonthForecast!.ClosedRevenue.Should().Be(10000m);
    }

    [Fact]
    public async Task ForecastRevenueAsync_ShouldApplyWeightedProbability_ForPipelineOpportunities()
    {
        // Arrange – Proposal-stage opp (50% stage × 60% CRM prob → blended 55%)
        var futureMonth = DateTime.UtcNow.AddMonths(2);
        var closeDate = new DateTime(futureMonth.Year, futureMonth.Month, 15);
        var opps = new List<Opportunity>
        {
            new Opportunity
            {
                Id = 2,
                IsDeleted = false,
                Stage = OpportunityStage.Proposal,
                Probability = 60,
                Amount = 20000m,
                ExpectedCloseDate = closeDate
            }
        };
        MockContext.Setup(c => c.Opportunities).Returns(MockDbSetFactory.CreateMockDbSet(opps).Object);

        // Act
        var result = await _sut.ForecastRevenueAsync(6);

        // Assert
        var monthKey = closeDate.ToString("yyyy-MM");
        var monthForecast = result.Months.FirstOrDefault(m => m.Month == monthKey);
        monthForecast.Should().NotBeNull();
        monthForecast!.ForecastedRevenue.Should().BeGreaterThan(0, because: "pipeline opp contributes weighted revenue");
        monthForecast.ForecastedRevenue.Should().BeLessThan(20000m, because: "weighting < 100% reduces forecast below full amount");
    }
}

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
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WinLossAnalysisService (TODO-CRM003-05).
/// </summary>
public class WinLossAnalysisServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly WinLossAnalysisService _service;
    private readonly List<Opportunity> _opportunities;

    public WinLossAnalysisServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _opportunities = new List<Opportunity>();

        SetupMockDbSets();

        _service = new WinLossAnalysisService(
            _mockContext.Object,
            Mock.Of<ILogger<WinLossAnalysisService>>());
    }

    private void SetupMockDbSets()
    {
        var mockOpps = MockDbSetFactory.CreateMockDbSet(_opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpps.Object);
    }

    private void Refresh()
    {
        var mockOpps = MockDbSetFactory.CreateMockDbSet(_opportunities);
        _mockContext.Setup(c => c.Opportunities).Returns(mockOpps.Object);
    }

    // ========================================================================
    // GetSummaryAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCorrectWinRate_WhenMixedOpportunities()
    {
        // Arrange — 2 won, 1 lost, 1 open (within default date range)
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, Stage = OpportunityStage.ClosedWon,  Amount = 10000m, Probability = 100, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, Stage = OpportunityStage.ClosedWon,  Amount = 20000m, Probability = 100, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 3, Stage = OpportunityStage.ClosedLost, Amount = 15000m, Probability = 0,   IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 4, Stage = OpportunityStage.Discovery,  Amount = 5000m,  Probability = 10,  IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        Refresh();

        // Act
        var summary = await _service.GetSummaryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert
        summary.Should().NotBeNull();
        summary.TotalOpportunities.Should().Be(4);
        summary.TotalWins.Should().Be(2);
        summary.TotalLosses.Should().Be(1);
        summary.WinRate.Should().BeApproximately(66.67m, 0.02m);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnZeroWinRate_WhenNoClosedDeals()
    {
        // Arrange — only open opportunities
        _opportunities.Add(new Opportunity
        {
            Id = 1,
            Stage = OpportunityStage.Discovery,
            Amount = 5000m,
            Probability = 10,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        Refresh();

        // Act
        var summary = await _service.GetSummaryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert
        summary.WinRate.Should().Be(0m);
        summary.TotalWins.Should().Be(0);
        summary.TotalLosses.Should().Be(0);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldExcludeDeletedOpportunities()
    {
        // Arrange
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, Stage = OpportunityStage.ClosedWon, Amount = 10000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, Stage = OpportunityStage.ClosedWon, Amount = 50000m, IsDeleted = true,  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        Refresh();

        // Act
        var summary = await _service.GetSummaryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert: only the non-deleted opportunity counts
        summary.TotalOpportunities.Should().Be(1);
        summary.TotalWins.Should().Be(1);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldIncludeCorrectAverageAmounts()
    {
        // Arrange
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, Stage = OpportunityStage.ClosedWon,  Amount = 10000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, Stage = OpportunityStage.ClosedWon,  Amount = 30000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 3, Stage = OpportunityStage.ClosedLost, Amount = 20000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        Refresh();

        // Act
        var summary = await _service.GetSummaryAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        // Assert
        summary.AverageWonDealSize.Should().BeApproximately(20000m, 0.01m);
        summary.AverageLostDealSize.Should().BeApproximately(20000m, 0.01m);
    }

    // ========================================================================
    // GetByReasonAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetByReasonAsync_ShouldGroupLossesByReason()
    {
        // Arrange
        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, Stage = OpportunityStage.ClosedLost, LossReasonCategory = LossReasonCategory.Price,       Amount = 10000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 2, Stage = OpportunityStage.ClosedLost, LossReasonCategory = LossReasonCategory.Price,       Amount = 15000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Opportunity { Id = 3, Stage = OpportunityStage.ClosedLost, LossReasonCategory = LossReasonCategory.Competition, Amount = 20000m, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        });
        Refresh();

        // Act
        var byReason = (await _service.GetByReasonAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1))).ToList();

        // Assert
        byReason.Should().NotBeEmpty();
        var priceReason = byReason.FirstOrDefault(r => r.ReasonCategory == LossReasonCategory.Price);
        priceReason.Should().NotBeNull();
        priceReason!.Count.Should().Be(2);
    }

    // ========================================================================
    // GetWinRateTrendsAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetWinRateTrendsAsync_ShouldReturnTrendsInRange()
    {
        // Arrange — spread across two months
        var month1 = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);
        var month2 = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        _opportunities.AddRange(new[]
        {
            new Opportunity { Id = 1, Stage = OpportunityStage.ClosedWon,  Amount = 10000m, IsDeleted = false, CreatedAt = month1.AddDays(5), UpdatedAt = month1.AddDays(5) },
            new Opportunity { Id = 2, Stage = OpportunityStage.ClosedLost, Amount = 5000m,  IsDeleted = false, CreatedAt = month1.AddDays(6), UpdatedAt = month1.AddDays(6) },
            new Opportunity { Id = 3, Stage = OpportunityStage.ClosedWon,  Amount = 20000m, IsDeleted = false, CreatedAt = month2.AddDays(5), UpdatedAt = month2.AddDays(5) }
        });
        Refresh();

        // Act
        var trends = (await _service.GetWinRateTrendsAsync(month1.AddDays(-1), month2.AddMonths(1), "month")).ToList();

        // Assert
        trends.Should().NotBeEmpty();
    }
}

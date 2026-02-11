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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Opportunity Repository
/// Covers: Opportunity-specific queries, pipeline, forecasting
/// </summary>
public class OpportunityRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<OpportunityEntity>> _mockDbSet;
    private readonly Mock<ILogger<OpportunityRepository>> _mockLogger;
    private readonly OpportunityRepository _repository;

    public OpportunityRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<OpportunityEntity>>();
        _mockLogger = new Mock<ILogger<OpportunityRepository>>();

        _mockContext.Setup(c => c.Set<OpportunityEntity>()).Returns(_mockDbSet.Object);
        _repository = new OpportunityRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStage Tests

    [Fact]
    public async Task GetByStageAsync_HasMatches_ReturnsOpportunities()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Qualification" },
            new OpportunityEntity { Id = 2, Stage = "Qualification" },
            new OpportunityEntity { Id = 3, Stage = "Negotiation" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetByStageAsync("Qualification");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStagesAsync_MultipleStages_ReturnsAll()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Qualification" },
            new OpportunityEntity { Id = 2, Stage = "Proposal" },
            new OpportunityEntity { Id = 3, Stage = "Closed Won" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        var stages = new[] { "Qualification", "Proposal" };

        // Act
        var result = await _repository.GetByStagesAsync(stages);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasOpportunities_ReturnsAccountOpportunities()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, AccountId = 1 },
            new OpportunityEntity { Id = 2, AccountId = 1 },
            new OpportunityEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOpenByAccountAsync_ReturnsOpenOnly()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, AccountId = 1, IsClosed = false },
            new OpportunityEntity { Id = 2, AccountId = 1, IsClosed = true },
            new OpportunityEntity { Id = 3, AccountId = 1, IsClosed = false }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetOpenByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByOwner Tests

    [Fact]
    public async Task GetByOwnerAsync_HasOpportunities_ReturnsOwnerOpportunities()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, OwnerId = 1 },
            new OpportunityEntity { Id = 2, OwnerId = 1 },
            new OpportunityEntity { Id = 3, OwnerId = 2 }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Pipeline Tests

    [Fact]
    public async Task GetPipelineValueAsync_CalculatesTotal()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Amount = 100000, IsClosed = false },
            new OpportunityEntity { Id = 2, Amount = 200000, IsClosed = false },
            new OpportunityEntity { Id = 3, Amount = 50000, IsClosed = true }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetPipelineValueAsync();

        // Assert
        result.Should().Be(300000);
    }

    [Fact]
    public async Task GetPipelineByStageAsync_ReturnsStageTotals()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Qualification", Amount = 100000 },
            new OpportunityEntity { Id = 2, Stage = "Qualification", Amount = 50000 },
            new OpportunityEntity { Id = 3, Stage = "Proposal", Amount = 200000 }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetPipelineByStageAsync();

        // Assert
        result.Should().ContainKey("Qualification");
        result["Qualification"].Should().Be(150000);
    }

    [Fact]
    public async Task GetWeightedPipelineAsync_AppliesProbability()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Amount = 100000, Probability = 50, IsClosed = false },
            new OpportunityEntity { Id = 2, Amount = 200000, Probability = 75, IsClosed = false }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetWeightedPipelineAsync();

        // Assert
        // (100000 * 0.5) + (200000 * 0.75) = 50000 + 150000 = 200000
        result.Should().Be(200000);
    }

    #endregion

    #region Closing Tests

    [Fact]
    public async Task GetClosingThisMonthAsync_ReturnsClosingOpportunities()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, CloseDate = now.AddDays(5), IsClosed = false },
            new OpportunityEntity { Id = 2, CloseDate = now.AddDays(-5), IsClosed = false },
            new OpportunityEntity { Id = 3, CloseDate = now.AddMonths(2), IsClosed = false }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetClosingThisMonthAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetOverdueAsync_ReturnsOverdueOpportunities()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, CloseDate = now.AddDays(-10), IsClosed = false },
            new OpportunityEntity { Id = 2, CloseDate = now.AddDays(-5), IsClosed = false },
            new OpportunityEntity { Id = 3, CloseDate = now.AddDays(10), IsClosed = false }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetOverdueAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetWinRateAsync_CalculatesRate()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Closed Won", IsClosed = true },
            new OpportunityEntity { Id = 2, Stage = "Closed Won", IsClosed = true },
            new OpportunityEntity { Id = 3, Stage = "Closed Lost", IsClosed = true },
            new OpportunityEntity { Id = 4, Stage = "Qualification", IsClosed = false }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetWinRateAsync();

        // Assert
        // 2 won out of 3 closed = 66.67%
        result.Should().BeApproximately(66.67m, 1);
    }

    [Fact]
    public async Task GetAverageDealSizeAsync_CalculatesAverage()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Amount = 100000, Stage = "Closed Won" },
            new OpportunityEntity { Id = 2, Amount = 200000, Stage = "Closed Won" },
            new OpportunityEntity { Id = 3, Amount = 150000, Stage = "Closed Won" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetAverageDealSizeAsync();

        // Assert
        result.Should().Be(150000);
    }

    [Fact]
    public async Task GetAverageSalesCycleAsync_CalculatesAverageDays()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-30), ClosedAt = DateTime.UtcNow, Stage = "Closed Won" },
            new OpportunityEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-60), ClosedAt = DateTime.UtcNow, Stage = "Closed Won" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetAverageSalesCycleAsync();

        // Assert
        result.Should().Be(45); // Average of 30 and 60 days
    }

    [Fact]
    public async Task GetCountByStageAsync_ReturnsStageCounts()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Qualification" },
            new OpportunityEntity { Id = 2, Stage = "Qualification" },
            new OpportunityEntity { Id = 3, Stage = "Proposal" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetCountByStageAsync();

        // Assert
        result["Qualification"].Should().Be(2);
        result["Proposal"].Should().Be(1);
    }

    #endregion

    #region Forecasting Tests

    [Fact]
    public async Task GetForecastAsync_ReturnsForecastData()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Amount = 100000, Probability = 90, CloseDate = DateTime.UtcNow.AddDays(15) },
            new OpportunityEntity { Id = 2, Amount = 200000, Probability = 50, CloseDate = DateTime.UtcNow.AddDays(30) }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetForecastAsync(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetQuarterlyForecastAsync_ReturnsQuarterData()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Amount = 100000, CloseDate = DateTime.UtcNow }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetQuarterlyForecastAsync(2026, 1);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Name = "Enterprise Deal" },
            new OpportunityEntity { Id = 2, Name = "Small Business" },
            new OpportunityEntity { Id = 3, Name = "Enterprise Expansion" }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.SearchAsync("Enterprise");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new OpportunityEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new OpportunityEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyWonAsync_ReturnsRecentWins()
    {
        // Arrange
        var opportunities = new List<OpportunityEntity>
        {
            new OpportunityEntity { Id = 1, Stage = "Closed Won", ClosedAt = DateTime.UtcNow.AddDays(-5) },
            new OpportunityEntity { Id = 2, Stage = "Closed Won", ClosedAt = DateTime.UtcNow.AddDays(-20) },
            new OpportunityEntity { Id = 3, Stage = "Closed Lost", ClosedAt = DateTime.UtcNow.AddDays(-5) }
        }.AsQueryable();

        SetupMockDbSet(opportunities);

        // Act
        var result = await _repository.GetRecentlyWonAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<OpportunityEntity> data)
    {
        _mockDbSet.As<IQueryable<OpportunityEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<OpportunityEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<OpportunityEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<OpportunityEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class OpportunityEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Probability { get; set; }
    public int? AccountId { get; set; }
    public int? OwnerId { get; set; }
    public DateTime CloseDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

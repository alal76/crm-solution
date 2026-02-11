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
/// Unit tests for Campaign Repository
/// Covers: Campaign-specific queries, metrics, scheduling
/// </summary>
public class CampaignRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<CampaignEntity>> _mockDbSet;
    private readonly Mock<ILogger<CampaignRepository>> _mockLogger;
    private readonly CampaignRepository _repository;

    public CampaignRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<CampaignEntity>>();
        _mockLogger = new Mock<ILogger<CampaignRepository>>();

        _mockContext.Setup(c => c.Set<CampaignEntity>()).Returns(_mockDbSet.Object);
        _repository = new CampaignRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Status = "Active" },
            new CampaignEntity { Id = 2, Status = "Active" },
            new CampaignEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetByStatusAsync("Active");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Status = "Active" },
            new CampaignEntity { Id = 2, Status = "Active" },
            new CampaignEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDraftsAsync_ReturnsDraftCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Status = "Draft" },
            new CampaignEntity { Id = 2, Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetDraftsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region GetByType Tests

    [Fact]
    public async Task GetByTypeAsync_HasMatches_ReturnsCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Type = "Email" },
            new CampaignEntity { Id = 2, Type = "Email" },
            new CampaignEntity { Id = 3, Type = "Social" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetByTypeAsync("Email");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmailCampaignsAsync_ReturnsEmailCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Type = "Email" },
            new CampaignEntity { Id = 2, Type = "Email" },
            new CampaignEntity { Id = 3, Type = "SMS" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetEmailCampaignsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByOwner Tests

    [Fact]
    public async Task GetByOwnerAsync_HasCampaigns_ReturnsOwnerCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, OwnerId = 1 },
            new CampaignEntity { Id = 2, OwnerId = 1 },
            new CampaignEntity { Id = 3, OwnerId = 2 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Scheduling Tests

    [Fact]
    public async Task GetScheduledAsync_ReturnsScheduledCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, StartDate = DateTime.UtcNow.AddDays(1), Status = "Scheduled" },
            new CampaignEntity { Id = 2, StartDate = DateTime.UtcNow.AddDays(5), Status = "Scheduled" },
            new CampaignEntity { Id = 3, Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetScheduledAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStartingTodayAsync_ReturnsTodayCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, StartDate = DateTime.UtcNow.Date, Status = "Scheduled" },
            new CampaignEntity { Id = 2, StartDate = DateTime.UtcNow.Date, Status = "Scheduled" },
            new CampaignEntity { Id = 3, StartDate = DateTime.UtcNow.AddDays(5), Status = "Scheduled" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetStartingTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEndingTodayAsync_ReturnsTodayEndingCampaigns()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, EndDate = DateTime.UtcNow.Date, Status = "Active" },
            new CampaignEntity { Id = 2, EndDate = DateTime.UtcNow.Date, Status = "Active" },
            new CampaignEntity { Id = 3, EndDate = DateTime.UtcNow.AddDays(5), Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetEndingTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Budget Tests

    [Fact]
    public async Task GetByBudgetRangeAsync_ReturnsInRange()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Budget = 5000 },
            new CampaignEntity { Id = 2, Budget = 10000 },
            new CampaignEntity { Id = 3, Budget = 50000 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetByBudgetRangeAsync(5000, 20000);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOverBudgetAsync_ReturnsCampaignsOverBudget()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Budget = 10000, ActualCost = 12000, Status = "Active" },
            new CampaignEntity { Id = 2, Budget = 10000, ActualCost = 15000, Status = "Active" },
            new CampaignEntity { Id = 3, Budget = 10000, ActualCost = 5000, Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetOverBudgetAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Name = "Summer Sale 2025" },
            new CampaignEntity { Id = 2, Name = "Winter Sale 2025" },
            new CampaignEntity { Id = 3, Name = "Product Launch" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.SearchAsync("Sale");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Metrics Tests

    [Fact]
    public async Task GetTotalRecipientsAsync_CalculatesTotal()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, TotalRecipients = 1000 },
            new CampaignEntity { Id = 2, TotalRecipients = 2000 },
            new CampaignEntity { Id = 3, TotalRecipients = 3000 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTotalRecipientsAsync();

        // Assert
        result.Should().Be(6000);
    }

    [Fact]
    public async Task GetTotalSentAsync_CalculatesTotal()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, TotalSent = 950 },
            new CampaignEntity { Id = 2, TotalSent = 1900 },
            new CampaignEntity { Id = 3, TotalSent = 2850 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTotalSentAsync();

        // Assert
        result.Should().Be(5700);
    }

    [Fact]
    public async Task GetAverageOpenRateAsync_CalculatesAverage()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, OpenRate = 25 },
            new CampaignEntity { Id = 2, OpenRate = 30 },
            new CampaignEntity { Id = 3, OpenRate = 35 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetAverageOpenRateAsync();

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public async Task GetAverageClickRateAsync_CalculatesAverage()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, ClickRate = 5 },
            new CampaignEntity { Id = 2, ClickRate = 10 },
            new CampaignEntity { Id = 3, ClickRate = 15 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetAverageClickRateAsync();

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task GetAverageConversionRateAsync_CalculatesAverage()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, ConversionRate = 2 },
            new CampaignEntity { Id = 2, ConversionRate = 3 },
            new CampaignEntity { Id = 3, ConversionRate = 4 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetAverageConversionRateAsync();

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Status = "Active" },
            new CampaignEntity { Id = 2, Status = "Active" },
            new CampaignEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Active"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByTypeAsync_ReturnsTypeCounts()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Type = "Email" },
            new CampaignEntity { Id = 2, Type = "Email" },
            new CampaignEntity { Id = 3, Type = "Social" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetCountByTypeAsync();

        // Assert
        result["Email"].Should().Be(2);
    }

    [Fact]
    public async Task GetTotalBudgetAsync_CalculatesTotalBudget()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, Budget = 10000 },
            new CampaignEntity { Id = 2, Budget = 20000 },
            new CampaignEntity { Id = 3, Budget = 30000 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTotalBudgetAsync();

        // Assert
        result.Should().Be(60000);
    }

    [Fact]
    public async Task GetTotalSpendAsync_CalculatesTotalSpend()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, ActualCost = 8000 },
            new CampaignEntity { Id = 2, ActualCost = 18000 },
            new CampaignEntity { Id = 3, ActualCost = 25000 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTotalSpendAsync();

        // Assert
        result.Should().Be(51000);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new CampaignEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new CampaignEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyCompletedAsync_ReturnsRecentlyCompleted()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, CompletedAt = DateTime.UtcNow.AddDays(-5), Status = "Completed" },
            new CampaignEntity { Id = 2, CompletedAt = DateTime.UtcNow.AddDays(-15), Status = "Completed" },
            new CampaignEntity { Id = 3, Status = "Active" }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetRecentlyCompletedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Top Performers Tests

    [Fact]
    public async Task GetTopByOpenRateAsync_ReturnsTopPerformers()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, OpenRate = 40 },
            new CampaignEntity { Id = 2, OpenRate = 35 },
            new CampaignEntity { Id = 3, OpenRate = 25 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTopByOpenRateAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().OpenRate.Should().Be(40);
    }

    [Fact]
    public async Task GetTopByConversionRateAsync_ReturnsTopPerformers()
    {
        // Arrange
        var campaigns = new List<CampaignEntity>
        {
            new CampaignEntity { Id = 1, ConversionRate = 8 },
            new CampaignEntity { Id = 2, ConversionRate = 5 },
            new CampaignEntity { Id = 3, ConversionRate = 3 }
        }.AsQueryable();

        SetupMockDbSet(campaigns);

        // Act
        var result = await _repository.GetTopByConversionRateAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result.First().ConversionRate.Should().Be(8);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<CampaignEntity> data)
    {
        _mockDbSet.As<IQueryable<CampaignEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<CampaignEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<CampaignEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<CampaignEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class CampaignEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string Type { get; set; } = "Email";
    public int? OwnerId { get; set; }
    public decimal Budget { get; set; }
    public decimal ActualCost { get; set; }
    public int TotalRecipients { get; set; }
    public int TotalSent { get; set; }
    public decimal OpenRate { get; set; }
    public decimal ClickRate { get; set; }
    public decimal ConversionRate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

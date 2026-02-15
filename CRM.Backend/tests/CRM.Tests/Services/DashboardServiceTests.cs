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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DashboardService
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<DashboardService>> _mockLogger;
    private readonly HybridCache _cache;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DashboardService>>();
        _cache = CreateHybridCache();

        SetupMockDbSets();

        _service = new DashboardService(_mockDbContext.Object, _mockLogger.Object, _cache);
    }

    private static HybridCache CreateHybridCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(5),
                LocalCacheExpiration = TimeSpan.FromSeconds(1)
            };
        });

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<HybridCache>();
    }

    private void SetupMockDbSets()
    {
        // Setup empty DbSets
        _mockDbContext.Setup(x => x.Accounts).Returns(CreateMockDbSet(new List<Account>()).Object);
        _mockDbContext.Setup(x => x.Contacts).Returns(CreateMockDbSet(new List<Contact>()).Object);
        _mockDbContext.Setup(x => x.Opportunities).Returns(CreateMockDbSet(new List<Opportunity>()).Object);
        _mockDbContext.Setup(x => x.Products).Returns(CreateMockDbSet(new List<Product>()).Object);
        _mockDbContext.Setup(x => x.CrmTasks).Returns(CreateMockDbSet(new List<CrmTask>()).Object);
        _mockDbContext.Setup(x => x.Users).Returns(CreateMockDbSet(new List<User>()).Object);
        _mockDbContext.Setup(x => x.Leads).Returns(CreateMockDbSet(new List<Lead>()).Object);
        _mockDbContext.Setup(x => x.Activities).Returns(CreateMockDbSet(new List<Activity>()).Object);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        return mockSet;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDbContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DashboardService(null!, _mockLogger.Object, _cache));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DashboardService(_mockDbContext.Object, null!, _cache));
    }

    [Fact]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Arrange & Act
        var service = new DashboardService(_mockDbContext.Object, _mockLogger.Object, _cache);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_WithEmptyDatabase_ReturnsZeroCounts()
    {
        // Arrange is done in constructor with empty sets

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Accounts.Total);
        Assert.Equal(0, result.Contacts.Total);
        Assert.Equal(0, result.Products.Total);
        Assert.Equal(0, result.Opportunities.Total);
        Assert.Equal(0, result.Tasks.Total);
    }

    [Fact]
    public async Task GetStatsAsync_WithData_ReturnsCorrectCounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "test1@example.com", IsDeleted = false },
            new Account { Id = 2, Email = "test2@example.com", IsDeleted = false },
            new Account { Id = 3, Email = "deleted@example.com", IsDeleted = true }
        };
        _mockDbContext.Setup(x => x.Accounts).Returns(CreateMockDbSet(accounts).Object);

        var contacts = new List<Contact>
        {
            new Contact { Id = 1, Status = ContactStatus.Active },
            new Contact { Id = 2, Status = ContactStatus.Active },
            new Contact { Id = 3, Status = ContactStatus.Inactive }
        };
        _mockDbContext.Setup(x => x.Contacts).Returns(CreateMockDbSet(contacts).Object);

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        Assert.Equal(2, result.Accounts.Total); // Excludes deleted
        Assert.Equal(2, result.Contacts.Total);  // Only active contacts
    }

    [Fact]
    public async Task GetStatsAsync_WithOpportunities_CalculatesCorrectValues()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 1, Name = "Opp1", Amount = 10000, Stage = OpportunityStage.Proposal, IsDeleted = false },
            new Opportunity { Id = 2, Name = "Opp2", Amount = 20000, Stage = OpportunityStage.ClosedWon, IsDeleted = false },
            new Opportunity { Id = 3, Name = "Opp3", Amount = 5000, Stage = OpportunityStage.ClosedLost, IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.Opportunities).Returns(CreateMockDbSet(opportunities).Object);

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        Assert.Equal(3, result.Opportunities.Total);
        Assert.Equal(10000, result.Opportunities.OpenValue); // Only open stages
        Assert.Equal(20000, result.Opportunities.WonValue);  // Only won
    }

    [Fact]
    public async Task GetStatsAsync_WithTasks_ReturnsCorrectTaskStats()
    {
        // Arrange
        var tasks = new List<CrmTask>
        {
            new CrmTask { Id = 1, Subject = "Task1", Status = CrmTaskStatus.NotStarted, IsDeleted = false },
            new CrmTask { Id = 2, Subject = "Task2", Status = CrmTaskStatus.InProgress, IsDeleted = false },
            new CrmTask { Id = 3, Subject = "Task3", Status = CrmTaskStatus.Completed, IsDeleted = false },
            new CrmTask { Id = 4, Subject = "Task4", Status = CrmTaskStatus.Cancelled, IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.CrmTasks).Returns(CreateMockDbSet(tasks).Object);

        // Act
        var result = await _service.GetStatsAsync();

        // Assert
        Assert.Equal(4, result.Tasks.Total);
        Assert.Equal(2, result.Tasks.Pending); // NotStarted + InProgress
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsCurrentTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = await _service.GetStatsAsync();

        var after = DateTime.UtcNow;

        // Assert
        Assert.True(result.Timestamp >= before);
        Assert.True(result.Timestamp <= after);
    }

    #endregion

    #region GetPipelineSummaryAsync Tests

    [Fact]
    public async Task GetPipelineSummaryAsync_WithEmptyDatabase_ReturnsEmptyPipeline()
    {
        // Act
        var result = await _service.GetPipelineSummaryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Stages);
        Assert.Equal(0, result.Summary.TotalValue);
        Assert.Equal(0, result.Summary.OpportunityCount);
    }

    [Fact]
    public async Task GetPipelineSummaryAsync_WithOpportunities_GroupsByStage()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 1, Name = "Opp1", Amount = 10000, Stage = OpportunityStage.Qualification, Probability = 20, IsDeleted = false },
            new Opportunity { Id = 2, Name = "Opp2", Amount = 20000, Stage = OpportunityStage.Qualification, Probability = 20, IsDeleted = false },
            new Opportunity { Id = 3, Name = "Opp3", Amount = 30000, Stage = OpportunityStage.Proposal, Probability = 50, IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.Opportunities).Returns(CreateMockDbSet(opportunities).Object);

        // Act
        var result = await _service.GetPipelineSummaryAsync();

        // Assert
        Assert.Equal(2, result.Stages.Count());
        Assert.Equal(3, result.Summary.OpportunityCount);
        Assert.Equal(60000, result.Summary.TotalValue);
    }

    [Fact]
    public async Task GetPipelineSummaryAsync_CalculatesWeightedValue()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new Opportunity { Id = 1, Name = "Opp1", Amount = 100000, Stage = OpportunityStage.Proposal, Probability = 50, IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.Opportunities).Returns(CreateMockDbSet(opportunities).Object);

        // Act
        var result = await _service.GetPipelineSummaryAsync();

        // Assert
        Assert.Equal(50000m, result.Summary.WeightedValue); // 100000 * 50%
    }

    #endregion

    #region GetRecentActivitiesAsync Tests

    [Fact]
    public async Task GetRecentActivitiesAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetRecentActivitiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_WithData_ReturnsRecentItems()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "test@example.com", Company = "Test Corp", CreatedAt = DateTime.UtcNow.AddHours(-1), IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.Accounts).Returns(CreateMockDbSet(accounts).Object);

        // Act
        var result = await _service.GetRecentActivitiesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("AccountCreated", result.First().Type);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_RespectsCountParameter()
    {
        // Arrange
        var accounts = Enumerable.Range(1, 20).Select(i => new Account
        {
            Id = i,
            Email = $"test{i}@example.com",
            Company = $"Company {i}",
            CreatedAt = DateTime.UtcNow.AddHours(-i),
            IsDeleted = false
        }).ToList();
        _mockDbContext.Setup(x => x.Accounts).Returns(CreateMockDbSet(accounts).Object);

        // Act
        var result = await _service.GetRecentActivitiesAsync(5);

        // Assert
        Assert.True(result.Count() <= 5);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_SortsByDateDescending()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 1, Email = "old@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7), IsDeleted = false },
            new Account { Id = 2, Email = "new@example.com", CreatedAt = DateTime.UtcNow.AddHours(-1), IsDeleted = false }
        };
        _mockDbContext.Setup(x => x.Accounts).Returns(CreateMockDbSet(accounts).Object);

        // Act
        var result = await _service.GetRecentActivitiesAsync();

        // Assert
        var resultList = result.ToList();
        Assert.True(resultList.First().ActivityDate >= resultList.Last().ActivityDate);
    }

    #endregion
}

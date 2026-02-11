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
/// Unit tests for Lead Repository
/// Covers: Lead-specific queries, scoring, conversion tracking
/// </summary>
public class LeadRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<LeadEntity>> _mockDbSet;
    private readonly Mock<ILogger<LeadRepository>> _mockLogger;
    private readonly LeadRepository _repository;

    public LeadRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<LeadEntity>>();
        _mockLogger = new Mock<ILogger<LeadRepository>>();

        _mockContext.Setup(c => c.Set<LeadEntity>()).Returns(_mockDbSet.Object);
        _repository = new LeadRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsLeads()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Status = "New" },
            new LeadEntity { Id = 2, Status = "New" },
            new LeadEntity { Id = 3, Status = "Qualified" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetByStatusAsync("New");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnassignedAsync_ReturnsUnassigned()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, OwnerId = null },
            new LeadEntity { Id = 2, OwnerId = null },
            new LeadEntity { Id = 3, OwnerId = 1 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetUnassignedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetBySource Tests

    [Fact]
    public async Task GetBySourceAsync_HasMatches_ReturnsLeads()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Source = "Website" },
            new LeadEntity { Id = 2, Source = "Website" },
            new LeadEntity { Id = 3, Source = "Referral" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetBySourceAsync("Website");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByOwner Tests

    [Fact]
    public async Task GetByOwnerAsync_HasLeads_ReturnsOwnerLeads()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, OwnerId = 1 },
            new LeadEntity { Id = 2, OwnerId = 1 },
            new LeadEntity { Id = 3, OwnerId = 2 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Scoring Tests

    [Fact]
    public async Task GetByScoreRangeAsync_ReturnsLeadsInRange()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Score = 85 },
            new LeadEntity { Id = 2, Score = 70 },
            new LeadEntity { Id = 3, Score = 50 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetByScoreRangeAsync(60, 90);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHotLeadsAsync_ReturnsHighScoreLeads()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Score = 90 },
            new LeadEntity { Id = 2, Score = 85 },
            new LeadEntity { Id = 3, Score = 50 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetHotLeadsAsync(80);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetColdLeadsAsync_ReturnsLowScoreLeads()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Score = 20 },
            new LeadEntity { Id = 2, Score = 30 },
            new LeadEntity { Id = 3, Score = 70 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetColdLeadsAsync(40);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, FirstName = "John", LastName = "Doe" },
            new LeadEntity { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new LeadEntity { Id = 3, FirstName = "Bob", LastName = "Smith" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.SearchAsync("Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ByCompany_ReturnsMatches()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Company = "Acme Corp" },
            new LeadEntity { Id = 2, Company = "Acme Industries" },
            new LeadEntity { Id = 3, Company = "Beta LLC" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.SearchAsync("Acme");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsLead()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Email = "john@example.com" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Status = "New" },
            new LeadEntity { Id = 2, Status = "New" },
            new LeadEntity { Id = 3, Status = "Qualified" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["New"].Should().Be(2);
        result["Qualified"].Should().Be(1);
    }

    [Fact]
    public async Task GetCountBySourceAsync_ReturnsSourceCounts()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Source = "Website" },
            new LeadEntity { Id = 2, Source = "Website" },
            new LeadEntity { Id = 3, Source = "Referral" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetCountBySourceAsync();

        // Assert
        result["Website"].Should().Be(2);
    }

    [Fact]
    public async Task GetConversionRateAsync_CalculatesRate()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Status = "Converted" },
            new LeadEntity { Id = 2, Status = "Converted" },
            new LeadEntity { Id = 3, Status = "Lost" },
            new LeadEntity { Id = 4, Status = "New" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetConversionRateAsync();

        // Assert
        // 2 converted out of 4 total = 50%
        result.Should().Be(50);
    }

    [Fact]
    public async Task GetAverageScoreAsync_CalculatesAverage()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Score = 60 },
            new LeadEntity { Id = 2, Score = 80 },
            new LeadEntity { Id = 3, Score = 70 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetAverageScoreAsync();

        // Assert
        result.Should().Be(70);
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public async Task GetConvertedLeadsAsync_ReturnsConverted()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Status = "Converted", ConvertedAt = DateTime.UtcNow.AddDays(-5) },
            new LeadEntity { Id = 2, Status = "Converted", ConvertedAt = DateTime.UtcNow.AddDays(-10) },
            new LeadEntity { Id = 3, Status = "New" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetConvertedLeadsAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAverageTimeToConvertAsync_CalculatesAverageDays()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-10), ConvertedAt = DateTime.UtcNow, Status = "Converted" },
            new LeadEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-20), ConvertedAt = DateTime.UtcNow, Status = "Converted" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetAverageTimeToConvertAsync();

        // Assert
        result.Should().Be(15); // Average of 10 and 20 days
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new LeadEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new LeadEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStaleLeadsAsync_ReturnsStale()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, LastActivityAt = DateTime.UtcNow.AddDays(-40), Status = "New" },
            new LeadEntity { Id = 2, LastActivityAt = DateTime.UtcNow.AddDays(-35), Status = "New" },
            new LeadEntity { Id = 3, LastActivityAt = DateTime.UtcNow.AddDays(-5), Status = "New" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetStaleLeadsAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Duplicate Detection Tests

    [Fact]
    public async Task FindDuplicatesAsync_ByEmail_ReturnsDuplicates()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, Email = "same@example.com" },
            new LeadEntity { Id = 2, Email = "same@example.com" }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.FindDuplicatesAsync("same@example.com");

        // Assert
        result.Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region Campaign Tests

    [Fact]
    public async Task GetByCampaignAsync_ReturnsLeadsFromCampaign()
    {
        // Arrange
        var leads = new List<LeadEntity>
        {
            new LeadEntity { Id = 1, CampaignId = 1 },
            new LeadEntity { Id = 2, CampaignId = 1 },
            new LeadEntity { Id = 3, CampaignId = 2 }
        }.AsQueryable();

        SetupMockDbSet(leads);

        // Act
        var result = await _repository.GetByCampaignAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkAssignAsync_AssignsLeads()
    {
        // Arrange
        var leadIds = new[] { 1, 2, 3 };
        var ownerId = 10;

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkAssignAsync(leadIds, ownerId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_UpdatesStatus()
    {
        // Arrange
        var leadIds = new[] { 1, 2, 3 };
        var newStatus = "Qualified";

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdateStatusAsync(leadIds, newStatus);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<LeadEntity> data)
    {
        _mockDbSet.As<IQueryable<LeadEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<LeadEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<LeadEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<LeadEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class LeadEntity
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string Status { get; set; } = "New";
    public string? Source { get; set; }
    public int Score { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsDeleted { get; set; }
}

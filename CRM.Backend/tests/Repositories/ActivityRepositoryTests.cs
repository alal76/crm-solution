// CRM Solution - Customer Relationship Management System
// Activity Repository Unit Tests

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
/// Unit tests for Activity Repository
/// Covers: Activity-specific queries, timeline, metrics
/// </summary>
public class ActivityRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<ActivityEntity>> _mockDbSet;
    private readonly Mock<ILogger<ActivityRepository>> _mockLogger;
    private readonly ActivityRepository _repository;

    public ActivityRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<ActivityEntity>>();
        _mockLogger = new Mock<ILogger<ActivityRepository>>();

        _mockContext.Setup(c => c.Set<ActivityEntity>()).Returns(_mockDbSet.Object);
        _repository = new ActivityRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByType Tests

    [Fact]
    public async Task GetByTypeAsync_HasMatches_ReturnsActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityType = "Call" },
            new ActivityEntity { Id = 2, ActivityType = "Call" },
            new ActivityEntity { Id = 3, ActivityType = "Email" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByTypeAsync("Call");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCallActivitiesAsync_ReturnsCallActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityType = "Call" },
            new ActivityEntity { Id = 2, ActivityType = "Call" },
            new ActivityEntity { Id = 3, ActivityType = "Meeting" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetCallActivitiesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMeetingActivitiesAsync_ReturnsMeetingActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityType = "Meeting" },
            new ActivityEntity { Id = 2, ActivityType = "Meeting" },
            new ActivityEntity { Id = 3, ActivityType = "Call" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetMeetingActivitiesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmailActivitiesAsync_ReturnsEmailActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityType = "Email" },
            new ActivityEntity { Id = 2, ActivityType = "Email" },
            new ActivityEntity { Id = 3, ActivityType = "Call" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetEmailActivitiesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByUser Tests

    [Fact]
    public async Task GetByUserAsync_HasActivities_ReturnsUserActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, UserId = 1 },
            new ActivityEntity { Id = 2, UserId = 1 },
            new ActivityEntity { Id = 3, UserId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByUserAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Entity Relationship Tests

    [Fact]
    public async Task GetByAccountAsync_ReturnsAccountActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, AccountId = 1 },
            new ActivityEntity { Id = 2, AccountId = 1 },
            new ActivityEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByContactAsync_ReturnsContactActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ContactId = 1 },
            new ActivityEntity { Id = 2, ContactId = 1 },
            new ActivityEntity { Id = 3, ContactId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByContactAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOpportunityAsync_ReturnsOpportunityActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, OpportunityId = 1 },
            new ActivityEntity { Id = 2, OpportunityId = 1 },
            new ActivityEntity { Id = 3, OpportunityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByOpportunityAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByLeadAsync_ReturnsLeadActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, LeadId = 1 },
            new ActivityEntity { Id = 2, LeadId = 1 },
            new ActivityEntity { Id = 3, LeadId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByLeadAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Timeline Tests

    [Fact]
    public async Task GetTimelineAsync_ReturnsChronologicalActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityDate = DateTime.UtcNow.AddDays(-5) },
            new ActivityEntity { Id = 2, ActivityDate = DateTime.UtcNow.AddDays(-10) },
            new ActivityEntity { Id = 3, ActivityDate = DateTime.UtcNow.AddDays(-1) }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetTimelineAsync(1, 20); // Account ID 1

        // Assert
        result.Should().BeInDescendingOrder(a => a.ActivityDate);
    }

    [Fact]
    public async Task GetAccountTimelineAsync_ReturnsAccountTimeline()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, AccountId = 1, ActivityDate = DateTime.UtcNow.AddDays(-1) },
            new ActivityEntity { Id = 2, AccountId = 1, ActivityDate = DateTime.UtcNow.AddDays(-5) },
            new ActivityEntity { Id = 3, AccountId = 2, ActivityDate = DateTime.UtcNow }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetAccountTimelineAsync(1, 20);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Date Range Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsActivitiesInRange()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityDate = DateTime.UtcNow.AddDays(-5) },
            new ActivityEntity { Id = 2, ActivityDate = DateTime.UtcNow.AddDays(-15) },
            new ActivityEntity { Id = 3, ActivityDate = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-20), 
            DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRecentActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityDate = DateTime.UtcNow.AddDays(-1) },
            new ActivityEntity { Id = 2, ActivityDate = DateTime.UtcNow.AddDays(-5) },
            new ActivityEntity { Id = 3, ActivityDate = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetRecentAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodayAsync_ReturnsTodayActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityDate = DateTime.UtcNow.Date },
            new ActivityEntity { Id = 2, ActivityDate = DateTime.UtcNow.Date },
            new ActivityEntity { Id = 3, ActivityDate = DateTime.UtcNow.AddDays(-5) }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_BySubject_ReturnsMatches()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, Subject = "Call with client" },
            new ActivityEntity { Id = 2, Subject = "Client follow up" },
            new ActivityEntity { Id = 3, Subject = "Internal meeting" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.SearchAsync("client");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByTypeAsync_ReturnsTypeCounts()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityType = "Call" },
            new ActivityEntity { Id = 2, ActivityType = "Call" },
            new ActivityEntity { Id = 3, ActivityType = "Email" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetCountByTypeAsync();

        // Assert
        result["Call"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByUserAsync_ReturnsUserCounts()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, UserId = 1 },
            new ActivityEntity { Id = 2, UserId = 1 },
            new ActivityEntity { Id = 3, UserId = 2 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetCountByUserAsync();

        // Assert
        result[1].Should().Be(2);
    }

    [Fact]
    public async Task GetDailyCountAsync_ReturnsDailyCounts()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ActivityDate = DateTime.UtcNow.Date },
            new ActivityEntity { Id = 2, ActivityDate = DateTime.UtcNow.Date },
            new ActivityEntity { Id = 3, ActivityDate = DateTime.UtcNow.AddDays(-1).Date }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetDailyCountAsync(7);

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAverageDurationAsync_CalculatesAverage()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, DurationMinutes = 30 },
            new ActivityEntity { Id = 2, DurationMinutes = 60 },
            new ActivityEntity { Id = 3, DurationMinutes = 90 }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetAverageDurationAsync();

        // Assert
        result.Should().Be(60); // Average of 30, 60, 90
    }

    #endregion

    #region Outcome Tests

    [Fact]
    public async Task GetByOutcomeAsync_ReturnsActivitiesByOutcome()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, Outcome = "Successful" },
            new ActivityEntity { Id = 2, Outcome = "Successful" },
            new ActivityEntity { Id = 3, Outcome = "No Answer" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetByOutcomeAsync("Successful");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSuccessfulAsync_ReturnsSuccessfulActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, Outcome = "Successful" },
            new ActivityEntity { Id = 2, Outcome = "Completed" },
            new ActivityEntity { Id = 3, Outcome = "No Answer" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetSuccessfulAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Direction Tests

    [Fact]
    public async Task GetOutboundAsync_ReturnsOutboundActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, Direction = "Outbound" },
            new ActivityEntity { Id = 2, Direction = "Outbound" },
            new ActivityEntity { Id = 3, Direction = "Inbound" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetOutboundAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInboundAsync_ReturnsInboundActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, Direction = "Inbound" },
            new ActivityEntity { Id = 2, Direction = "Inbound" },
            new ActivityEntity { Id = 3, Direction = "Outbound" }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetInboundAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Scheduled Activities Tests

    [Fact]
    public async Task GetScheduledAsync_ReturnsScheduledActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, IsScheduled = true, ScheduledDate = DateTime.UtcNow.AddDays(1) },
            new ActivityEntity { Id = 2, IsScheduled = true, ScheduledDate = DateTime.UtcNow.AddDays(5) },
            new ActivityEntity { Id = 3, IsScheduled = false }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetScheduledAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUpcomingAsync_ReturnsUpcomingActivities()
    {
        // Arrange
        var activities = new List<ActivityEntity>
        {
            new ActivityEntity { Id = 1, ScheduledDate = DateTime.UtcNow.AddDays(1) },
            new ActivityEntity { Id = 2, ScheduledDate = DateTime.UtcNow.AddDays(3) },
            new ActivityEntity { Id = 3, ScheduledDate = DateTime.UtcNow.AddDays(-5) }
        }.AsQueryable();

        SetupMockDbSet(activities);

        // Act
        var result = await _repository.GetUpcomingAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDeleteAsync_DeletesActivities()
    {
        // Arrange
        var activityIds = new[] { 1, 2, 3 };
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkDeleteAsync(activityIds);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<ActivityEntity> data)
    {
        _mockDbSet.As<IQueryable<ActivityEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<ActivityEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<ActivityEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<ActivityEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class ActivityEntity
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ActivityType { get; set; } = "Call";
    public string? Outcome { get; set; }
    public string? Direction { get; set; }
    public int? UserId { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? LeadId { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public bool IsScheduled { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsDeleted { get; set; }
}

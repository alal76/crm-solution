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
/// Unit tests for Task Repository
/// Covers: Task-specific queries, assignment, scheduling
/// </summary>
public class TaskRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<TaskEntity>> _mockDbSet;
    private readonly Mock<ILogger<TaskRepository>> _mockLogger;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<TaskEntity>>();
        _mockLogger = new Mock<ILogger<TaskRepository>>();

        _mockContext.Setup(c => c.Set<TaskEntity>()).Returns(_mockDbSet.Object);
        _repository = new TaskRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Status = "Open" },
            new TaskEntity { Id = 2, Status = "Open" },
            new TaskEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByStatusAsync("Open");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOpenAsync_ReturnsOpenTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Status = "Open" },
            new TaskEntity { Id = 2, Status = "In Progress" },
            new TaskEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetOpenAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetCompletedAsync_ReturnsCompletedTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Status = "Completed" },
            new TaskEntity { Id = 2, Status = "Completed" },
            new TaskEntity { Id = 3, Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetCompletedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByPriority Tests

    [Fact]
    public async Task GetByPriorityAsync_HasMatches_ReturnsTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Priority = "High" },
            new TaskEntity { Id = 2, Priority = "High" },
            new TaskEntity { Id = 3, Priority = "Low" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByPriorityAsync("High");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHighPriorityAsync_ReturnsHighPriorityTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Priority = "High", Status = "Open" },
            new TaskEntity { Id = 2, Priority = "Critical", Status = "Open" },
            new TaskEntity { Id = 3, Priority = "Low", Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetHighPriorityAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region GetByAssignee Tests

    [Fact]
    public async Task GetByAssigneeAsync_HasTasks_ReturnsAssigneeTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, AssignedToId = 1 },
            new TaskEntity { Id = 2, AssignedToId = 1 },
            new TaskEntity { Id = 3, AssignedToId = 2 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByAssigneeAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUnassignedAsync_ReturnsUnassignedTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, AssignedToId = null },
            new TaskEntity { Id = 2, AssignedToId = null },
            new TaskEntity { Id = 3, AssignedToId = 1 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetUnassignedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Due Date Tests

    [Fact]
    public async Task GetOverdueAsync_ReturnsOverdueTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(-2), Status = "Open" },
            new TaskEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(-1), Status = "Open" },
            new TaskEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(5), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetOverdueAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDueTodayAsync_ReturnsTodayTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, DueDate = DateTime.UtcNow.Date, Status = "Open" },
            new TaskEntity { Id = 2, DueDate = DateTime.UtcNow.Date, Status = "Open" },
            new TaskEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(5), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetDueTodayAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDueThisWeekAsync_ReturnsThisWeekTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(1), Status = "Open" },
            new TaskEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(3), Status = "Open" },
            new TaskEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(30), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetDueThisWeekAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDueByDateAsync_ReturnsTasksDueByDate()
    {
        // Arrange
        var targetDate = DateTime.UtcNow.AddDays(7);
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(3), Status = "Open" },
            new TaskEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(5), Status = "Open" },
            new TaskEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(15), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetDueByDateAsync(targetDate);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Entity Relationship Tests

    [Fact]
    public async Task GetByAccountAsync_ReturnsAccountTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, AccountId = 1 },
            new TaskEntity { Id = 2, AccountId = 1 },
            new TaskEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByContactAsync_ReturnsContactTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, ContactId = 1 },
            new TaskEntity { Id = 2, ContactId = 1 },
            new TaskEntity { Id = 3, ContactId = 2 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByContactAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOpportunityAsync_ReturnsOpportunityTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, OpportunityId = 1 },
            new TaskEntity { Id = 2, OpportunityId = 1 },
            new TaskEntity { Id = 3, OpportunityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByOpportunityAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_BySubject_ReturnsMatches()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Subject = "Follow up with client" },
            new TaskEntity { Id = 2, Subject = "Client meeting prep" },
            new TaskEntity { Id = 3, Subject = "Send proposal" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.SearchAsync("client");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Status = "Open" },
            new TaskEntity { Id = 2, Status = "Open" },
            new TaskEntity { Id = 3, Status = "Completed" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Open"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByPriorityAsync_ReturnsPriorityCounts()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Priority = "High" },
            new TaskEntity { Id = 2, Priority = "High" },
            new TaskEntity { Id = 3, Priority = "Low" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetCountByPriorityAsync();

        // Assert
        result["High"].Should().Be(2);
    }

    [Fact]
    public async Task GetCountByAssigneeAsync_ReturnsAssigneeCounts()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, AssignedToId = 1 },
            new TaskEntity { Id = 2, AssignedToId = 1 },
            new TaskEntity { Id = 3, AssignedToId = 2 }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetCountByAssigneeAsync();

        // Assert
        result[1].Should().Be(2);
    }

    [Fact]
    public async Task GetOverdueCountAsync_ReturnsOverdueCount()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(-2), Status = "Open" },
            new TaskEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(-1), Status = "Open" },
            new TaskEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(5), Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetOverdueCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCompletionRateAsync_CalculatesRate()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, Status = "Completed" },
            new TaskEntity { Id = 2, Status = "Completed" },
            new TaskEntity { Id = 3, Status = "Open" },
            new TaskEntity { Id = 4, Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetCompletionRateAsync();

        // Assert
        result.Should().Be(50); // 2 out of 4 = 50%
    }

    #endregion

    #region Type Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsTasksByType()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, TaskType = "Call" },
            new TaskEntity { Id = 2, TaskType = "Call" },
            new TaskEntity { Id = 3, TaskType = "Email" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetByTypeAsync("Call");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Recurring Tasks Tests

    [Fact]
    public async Task GetRecurringAsync_ReturnsRecurringTasks()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, IsRecurring = true },
            new TaskEntity { Id = 2, IsRecurring = true },
            new TaskEntity { Id = 3, IsRecurring = false }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetRecurringAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new TaskEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new TaskEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-15) }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyCompletedAsync_ReturnsRecentlyCompleted()
    {
        // Arrange
        var tasks = new List<TaskEntity>
        {
            new TaskEntity { Id = 1, CompletedAt = DateTime.UtcNow.AddDays(-1), Status = "Completed" },
            new TaskEntity { Id = 2, CompletedAt = DateTime.UtcNow.AddDays(-5), Status = "Completed" },
            new TaskEntity { Id = 3, Status = "Open" }
        }.AsQueryable();

        SetupMockDbSet(tasks);

        // Act
        var result = await _repository.GetRecentlyCompletedAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkAssignAsync_AssignsTasks()
    {
        // Arrange
        var taskIds = new[] { 1, 2, 3 };
        var assigneeId = 10;

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkAssignAsync(taskIds, assigneeId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_UpdatesStatus()
    {
        // Arrange
        var taskIds = new[] { 1, 2, 3 };
        var newStatus = "Completed";

        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(3);

        // Act
        var result = await _repository.BulkUpdateStatusAsync(taskIds, newStatus);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<TaskEntity> data)
    {
        _mockDbSet.As<IQueryable<TaskEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<TaskEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<TaskEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<TaskEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class TaskEntity
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public string? TaskType { get; set; }
    public int? AssignedToId { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TaskService using InMemory database.
/// Tests CRUD operations, filtering, completion, overdue detection, and statistics.
/// </summary>
public class TaskServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TaskServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<TaskService>>();
        _service = new TaskService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private CrmTask CreateTestTask(
        string subject = "Test Task",
        CrmTaskStatus status = CrmTaskStatus.NotStarted,
        CrmTaskPriority priority = CrmTaskPriority.Normal,
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        DateTime? dueDate = null)
    {
        return new CrmTask
        {
            Subject = subject,
            Description = $"Description for {subject}",
            Status = status,
            Priority = priority,
            AccountId = accountId,
            OpportunityId = opportunityId,
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            TaskType = CrmTaskType.Other,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<CrmTask> SeedTaskAsync(
        string subject = "Seeded Task",
        CrmTaskStatus status = CrmTaskStatus.NotStarted,
        CrmTaskPriority priority = CrmTaskPriority.Normal,
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        DateTime? dueDate = null,
        bool isDeleted = false)
    {
        var task = CreateTestTask(subject, status, priority, accountId, opportunityId, assignedToUserId, dueDate);
        task.IsDeleted = isDeleted;
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();
        return task;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        var act = () => new TaskService(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new TaskService(_dbContext, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateTask_WhenValidTask()
    {
        // Arrange
        var task = CreateTestTask("New Task");

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Subject.Should().Be("New Task");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultStatus_WhenStatusIsDefault()
    {
        // Arrange
        var task = new CrmTask { Subject = "Default Status Task" };

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.Status.Should().Be(CrmTaskStatus.NotStarted);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultPriority_WhenPriorityIsDefault()
    {
        // Arrange
        var task = new CrmTask { Subject = "Default Priority Task" };

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.Priority.Should().Be(CrmTaskPriority.Normal);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveExplicitStatus_WhenStatusIsSet()
    {
        // Arrange
        var task = CreateTestTask(status: CrmTaskStatus.InProgress);

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.Status.Should().Be(CrmTaskStatus.InProgress);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveExplicitPriority_WhenPriorityIsSet()
    {
        // Arrange
        var task = CreateTestTask(priority: CrmTaskPriority.Urgent);

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.Priority.Should().Be(CrmTaskPriority.Urgent);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenTaskIsNull()
    {
        // Act
        var act = () => _service.CreateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTask_InDatabase()
    {
        // Arrange
        var task = CreateTestTask("Persisted Task");

        // Act
        await _service.CreateAsync(task);

        // Assert
        var dbTask = await _dbContext.CrmTasks.FirstOrDefaultAsync(t => t.Subject == "Persisted Task");
        dbTask.Should().NotBeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Find Me");

        // Act
        var result = await _service.GetByIdAsync(seeded.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Subject.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskIsDeleted()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Deleted Task", isDeleted: true);

        // Act
        var result = await _service.GetByIdAsync(seeded.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Original Subject");
        var update = CreateTestTask("Updated Subject");

        // Act
        var result = await _service.UpdateAsync(seeded.Id, update);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProperties_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Original Subject");
        var update = CreateTestTask("Updated Subject", CrmTaskStatus.InProgress, CrmTaskPriority.High);

        // Act
        await _service.UpdateAsync(seeded.Id, update);

        // Assert
        var updated = await _dbContext.CrmTasks.FindAsync(seeded.Id);
        updated!.Subject.Should().Be("Updated Subject");
        updated.Status.Should().Be(CrmTaskStatus.InProgress);
        updated.Priority.Should().Be(CrmTaskPriority.High);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetCompletedDate_WhenStatusChangedToCompleted()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Task to Complete");
        var update = CreateTestTask(status: CrmTaskStatus.Completed);

        // Act
        await _service.UpdateAsync(seeded.Id, update);

        // Assert
        var updated = await _dbContext.CrmTasks.FindAsync(seeded.Id);
        updated!.CompletedDate.Should().NotBeNull();
        updated.CompletedDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        var update = CreateTestTask("Nonexistent");

        // Act
        var result = await _service.UpdateAsync(999, update);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTaskIsDeleted()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Deleted Task", isDeleted: true);
        var update = CreateTestTask("Update Attempt");

        // Act
        var result = await _service.UpdateAsync(seeded.Id, update);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Timestamp Test");
        var originalUpdatedAt = seeded.UpdatedAt;
        await Task.Delay(10); // Ensure time difference
        var update = CreateTestTask("Updated Timestamp");

        // Act
        await _service.UpdateAsync(seeded.Id, update);

        // Assert
        var updated = await _dbContext.CrmTasks.FindAsync(seeded.Id);
        updated!.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt!.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenTaskIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(1, null!));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Delete Me");

        // Act
        var result = await _service.DeleteAsync(seeded.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Soft Delete Me");

        // Act
        await _service.DeleteAsync(seeded.Id);

        // Assert
        var deleted = await _dbContext.CrmTasks.FindAsync(seeded.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTaskAlreadyDeleted()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Already Deleted", isDeleted: true);

        // Act
        var result = await _service.DeleteAsync(seeded.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CompleteAsync Tests

    [Fact]
    public async Task CompleteAsync_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Complete Me");

        // Act
        var result = await _service.CompleteAsync(seeded.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ShouldSetStatusAndCompletedDate()
    {
        // Arrange
        var seeded = await SeedTaskAsync("Complete Me");

        // Act
        await _service.CompleteAsync(seeded.Id);

        // Assert
        var completed = await _dbContext.CrmTasks.FindAsync(seeded.Id);
        completed!.Status.Should().Be(CrmTaskStatus.Completed);
        completed.CompletedDate.Should().NotBeNull();
        completed.CompletedDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        var result = await _service.CompleteAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnFalse_WhenTaskIsDeleted()
    {
        var seeded = await SeedTaskAsync("Deleted Complete", isDeleted: true);
        var result = await _service.CompleteAsync(seeded.Id);
        result.Should().BeFalse();
    }

    #endregion

    #region GetTasksAsync Filtering Tests

    [Fact]
    public async Task GetTasksAsync_ShouldReturnAllNonDeletedTasks_WhenNoFilters()
    {
        // Arrange
        await SeedTaskAsync("Task 1");
        await SeedTaskAsync("Task 2");
        await SeedTaskAsync("Deleted Task", isDeleted: true);

        // Act
        var result = await _service.GetTasksAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByAccountId()
    {
        // Arrange
        await SeedTaskAsync("Account 1 Task", accountId: 1);
        await SeedTaskAsync("Account 2 Task", accountId: 2);

        // Act
        var result = await _service.GetTasksAsync(accountId: 1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Account 1 Task");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByOpportunityId()
    {
        await SeedTaskAsync("Opp 1 Task", opportunityId: 10);
        await SeedTaskAsync("Opp 2 Task", opportunityId: 20);

        var result = await _service.GetTasksAsync(opportunityId: 10);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Opp 1 Task");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByAssignedToUserId()
    {
        await SeedTaskAsync("User 1 Task", assignedToUserId: 100);
        await SeedTaskAsync("User 2 Task", assignedToUserId: 200);

        var result = await _service.GetTasksAsync(assignedToUserId: 100);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("User 1 Task");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByStatus()
    {
        await SeedTaskAsync("Not Started", status: CrmTaskStatus.NotStarted);
        await SeedTaskAsync("In Progress", status: CrmTaskStatus.InProgress);
        await SeedTaskAsync("Completed", status: CrmTaskStatus.Completed);

        var result = await _service.GetTasksAsync(status: CrmTaskStatus.InProgress);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("In Progress");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByPriority()
    {
        await SeedTaskAsync("Low Priority", priority: CrmTaskPriority.Low);
        await SeedTaskAsync("High Priority", priority: CrmTaskPriority.High);

        var result = await _service.GetTasksAsync(priority: CrmTaskPriority.High);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("High Priority");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterOverdue_WhenOverdueIsTrue()
    {
        await SeedTaskAsync("Overdue Task", dueDate: DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync("Future Task", dueDate: DateTime.UtcNow.AddDays(1));
        await SeedTaskAsync("Completed Overdue", status: CrmTaskStatus.Completed, dueDate: DateTime.UtcNow.AddDays(-1));

        var result = await _service.GetTasksAsync(overdue: true);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Overdue Task");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldCombineMultipleFilters()
    {
        await SeedTaskAsync("Match", status: CrmTaskStatus.InProgress, accountId: 1);
        await SeedTaskAsync("No Match Status", status: CrmTaskStatus.NotStarted, accountId: 1);
        await SeedTaskAsync("No Match Account", status: CrmTaskStatus.InProgress, accountId: 2);

        var result = await _service.GetTasksAsync(accountId: 1, status: CrmTaskStatus.InProgress);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Match");
    }

    #endregion

    #region GetOverdueTasksAsync Tests

    [Fact]
    public async Task GetOverdueTasksAsync_ShouldReturnOverdueTasks()
    {
        await SeedTaskAsync("Overdue 1", dueDate: DateTime.UtcNow.AddDays(-2));
        await SeedTaskAsync("Overdue 2", dueDate: DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync("Not Due", dueDate: DateTime.UtcNow.AddDays(1));

        var result = await _service.GetOverdueTasksAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ShouldExcludeCompleted()
    {
        await SeedTaskAsync("Overdue Active", dueDate: DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync("Overdue Completed", status: CrmTaskStatus.Completed, dueDate: DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync("Overdue Cancelled", status: CrmTaskStatus.Cancelled, dueDate: DateTime.UtcNow.AddDays(-1));

        var result = await _service.GetOverdueTasksAsync();

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Overdue Active");
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ShouldExcludeTasksWithoutDueDate()
    {
        await SeedTaskAsync("No Due Date", dueDate: null);
        await SeedTaskAsync("Overdue", dueDate: DateTime.UtcNow.AddDays(-1));

        var result = await _service.GetOverdueTasksAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ShouldExcludeDeletedTasks()
    {
        await SeedTaskAsync("Deleted Overdue", dueDate: DateTime.UtcNow.AddDays(-1), isDeleted: true);

        var result = await _service.GetOverdueTasksAsync();

        result.Should().BeEmpty();
    }

    #endregion

    #region GetTasksDueTodayAsync Tests

    [Fact]
    public async Task GetTasksDueTodayAsync_ShouldReturnTasksDueToday()
    {
        var today = DateTime.UtcNow.Date;
        await SeedTaskAsync("Due Today", dueDate: today.AddHours(10));
        await SeedTaskAsync("Due Tomorrow", dueDate: today.AddDays(1).AddHours(10));
        await SeedTaskAsync("Due Yesterday", dueDate: today.AddDays(-1));

        var result = await _service.GetTasksDueTodayAsync();

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Due Today");
    }

    [Fact]
    public async Task GetTasksDueTodayAsync_ShouldFilterByUserId()
    {
        var today = DateTime.UtcNow.Date;
        await SeedTaskAsync("User 1 Today", dueDate: today.AddHours(10), assignedToUserId: 1);
        await SeedTaskAsync("User 2 Today", dueDate: today.AddHours(10), assignedToUserId: 2);

        var result = await _service.GetTasksDueTodayAsync(userId: 1);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("User 1 Today");
    }

    [Fact]
    public async Task GetTasksDueTodayAsync_ShouldExcludeCompletedAndCancelled()
    {
        var today = DateTime.UtcNow.Date;
        await SeedTaskAsync("Active Today", dueDate: today.AddHours(10));
        await SeedTaskAsync("Completed Today", status: CrmTaskStatus.Completed, dueDate: today.AddHours(10));
        await SeedTaskAsync("Cancelled Today", status: CrmTaskStatus.Cancelled, dueDate: today.AddHours(10));

        var result = await _service.GetTasksDueTodayAsync();

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Active Today");
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        await SeedTaskAsync("NS1", status: CrmTaskStatus.NotStarted);
        await SeedTaskAsync("NS2", status: CrmTaskStatus.NotStarted);
        await SeedTaskAsync("IP1", status: CrmTaskStatus.InProgress);
        await SeedTaskAsync("C1", status: CrmTaskStatus.Completed);
        await SeedTaskAsync("Deleted", status: CrmTaskStatus.NotStarted, isDeleted: true);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.Total.Should().Be(4);
        stats.NotStarted.Should().Be(2);
        stats.InProgress.Should().Be(1);
        stats.Completed.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountOverdueTasks()
    {
        await SeedTaskAsync("Overdue", dueDate: DateTime.UtcNow.AddDays(-1));
        await SeedTaskAsync("Not Overdue", dueDate: DateTime.UtcNow.AddDays(1));
        await SeedTaskAsync("Completed Overdue", status: CrmTaskStatus.Completed, dueDate: DateTime.UtcNow.AddDays(-1));

        var stats = await _service.GetStatisticsAsync();

        stats.Overdue.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountDueToday()
    {
        var today = DateTime.UtcNow.Date;
        await SeedTaskAsync("Due Today", dueDate: today.AddHours(10));
        await SeedTaskAsync("Due Tomorrow", dueDate: today.AddDays(1).AddHours(10));

        var stats = await _service.GetStatisticsAsync();

        stats.DueToday.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldCountDueThisWeek()
    {
        var today = DateTime.UtcNow.Date;
        await SeedTaskAsync("Due Today", dueDate: today.AddHours(10));
        await SeedTaskAsync("Due In 3 Days", dueDate: today.AddDays(3));
        await SeedTaskAsync("Due In 10 Days", dueDate: today.AddDays(10));

        var stats = await _service.GetStatisticsAsync();

        stats.DueThisWeek.Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldFilterByUserId()
    {
        await SeedTaskAsync("User 1 Task", assignedToUserId: 1);
        await SeedTaskAsync("User 2 Task", assignedToUserId: 2);

        var stats = await _service.GetStatisticsAsync(userId: 1);

        stats.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnZeros_WhenNoTasks()
    {
        var stats = await _service.GetStatisticsAsync();

        stats.Total.Should().Be(0);
        stats.NotStarted.Should().Be(0);
        stats.InProgress.Should().Be(0);
        stats.Completed.Should().Be(0);
        stats.Overdue.Should().Be(0);
        stats.DueToday.Should().Be(0);
        stats.DueThisWeek.Should().Be(0);
    }

    #endregion

    #region New Field Round-Trip Tests (OpportunityCrmTaskDto gap remediation)

    [Fact]
    public async Task CreateAsync_ShouldPersistNewFields_TaskTypeStartDateEstimatedMinutesAccountIdOpportunityId()
    {
        // Arrange
        var task = new CrmTask
        {
            Subject = "New Field Task",
            TaskType = CrmTaskType.Demo,
            StartDate = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            EstimatedMinutes = 90,
            AccountId = 42,
            OpportunityId = 77
        };

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.TaskType.Should().Be(CrmTaskType.Demo);
        result.StartDate.Should().Be(new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc));
        result.EstimatedMinutes.Should().Be(90);
        result.AccountId.Should().Be(42);
        result.OpportunityId.Should().Be(77);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNewFields_AfterRoundTripThroughDatabase()
    {
        // Arrange
        var task = new CrmTask
        {
            Subject = "Round Trip Task",
            TaskType = CrmTaskType.Proposal,
            StartDate = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            EstimatedMinutes = 240,
            AccountId = 5,
            OpportunityId = 9
        };
        await _service.CreateAsync(task);

        // Act
        var result = await _service.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TaskType.Should().Be(CrmTaskType.Proposal);
        result.StartDate.Should().Be(new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc));
        result.EstimatedMinutes.Should().Be(240);
        result.AccountId.Should().Be(5);
        result.OpportunityId.Should().Be(9);
    }

    [Fact]
    public async Task CreateAsync_ShouldDefaultTaskTypeToOther_WhenNotSpecified()
    {
        // Arrange
        var task = new CrmTask { Subject = "Default TaskType" };

        // Act
        var result = await _service.CreateAsync(task);

        // Assert
        result.TaskType.Should().Be(CrmTaskType.Other);
    }

    #endregion
}

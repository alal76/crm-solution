// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for TaskService (TCOV Wave-A).</summary>
public class TaskServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<TaskService>> _logger;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        _logger = new Mock<ILogger<TaskService>>();
        _service = new TaskService(_context, _logger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── GetTasksAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasksAsync_ShouldReturnAllNonDeleted_WhenNoFilters()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 1, Subject = "Task A", IsDeleted = false });
        _context.CrmTasks.Add(new CrmTask { Id = 2, Subject = "Task B", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetTasksAsync();

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Task A");
    }

    [Fact]
    public async Task GetTasksAsync_ShouldReturnEmpty_WhenNoTasks()
    {
        var result = await _service.GetTasksAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasksAsync_ShouldFilterByAssignedUserId()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 1, Subject = "Mine", AssignedToUserId = 42, IsDeleted = false });
        _context.CrmTasks.Add(new CrmTask { Id = 2, Subject = "Other", AssignedToUserId = 99, IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetTasksAsync(assignedToUserId: 42);

        result.Should().HaveCount(1);
        result.First().Subject.Should().Be("Mine");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 5, Subject = "Find Me", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskIsDeleted()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 3, Subject = "Deleted", IsDeleted = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(3);

        result.Should().BeNull();
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateTask_WithDefaultStatusAndPriority()
    {
        var task = new CrmTask { Subject = "New Task" };

        var created = await _service.CreateAsync(task);

        created.Should().NotBeNull();
        created.Subject.Should().Be("New Task");
        created.Status.Should().Be(CrmTaskStatus.NotStarted);
        created.Priority.Should().Be(CrmTaskPriority.Normal);
        created.IsDeleted.Should().BeFalse();
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenTaskIsNull()
    {
        Func<Task> act = () => _service.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenTaskExists()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 10, Subject = "Old Subject", IsDeleted = false });
        await _context.SaveChangesAsync();

        var updated = new CrmTask { Subject = "New Subject", Status = CrmTaskStatus.InProgress, Priority = CrmTaskPriority.High };
        var result = await _service.UpdateAsync(10, updated);

        result.Should().BeTrue();
        var inDb = await _context.CrmTasks.FindAsync(10);
        inDb!.Subject.Should().Be("New Subject");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTaskNotFound()
    {
        var result = await _service.UpdateAsync(999, new CrmTask { Subject = "Ghost" });
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetCompletedDate_WhenStatusChangedToCompleted()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 20, Subject = "Go Complete", Status = CrmTaskStatus.InProgress, IsDeleted = false });
        await _context.SaveChangesAsync();

        await _service.UpdateAsync(20, new CrmTask { Subject = "Go Complete", Status = CrmTaskStatus.Completed });

        var inDb = await _context.CrmTasks.FindAsync(20);
        inDb!.CompletedDate.Should().NotBeNull();
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_AndSoftDelete_WhenTaskExists()
    {
        _context.CrmTasks.Add(new CrmTask { Id = 30, Subject = "To Delete", IsDeleted = false });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(30);

        result.Should().BeTrue();
        var inDb = await _context.CrmTasks.FindAsync(30);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTaskNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D05 — TasksController unit tests
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for TasksController (TCOV2-D05).
/// TasksController delegates CRUD to ITaskService (a real TaskService instance backed by the
/// same EF InMemory CrmDbContext, so seeding data directly into _dbContext is visible through
/// the service layer); only GetMyQueue still queries CrmDbContext directly for its richer joins.
/// NormalizationService (constructor-arg) is created with a mock ICrmDbContext.
/// [Authorize] attribute present; not exercised here.
/// </summary>
public class TasksControllerTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"TasksTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        var mockCrmDb = new Mock<ICrmDbContext>();
        var normalization = new NormalizationService(mockCrmDb.Object);
        var logger = new Mock<ILogger<TasksController>>();
        var taskServiceLogger = new Mock<ILogger<TaskService>>();
        var taskService = new TaskService(_dbContext, taskServiceLogger.Object);

        _controller = new TasksController(taskService, _dbContext, logger.Object, normalization);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "TestAuth"))
            }
        };
    }

    public void Dispose() => _dbContext.Dispose();

    private static CrmTask MakeTask(int id = 0) => new()
    {
        Subject = $"Task {(id > 0 ? id.ToString() : "New")}",
        Status = CrmTaskStatus.NotStarted,
        Priority = CrmTaskPriority.Normal,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // ── GetTasks ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_ShouldReturnOk_WhenNoTasks()
    {
        var result = await _controller.GetTasks();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTasks_ShouldReturnOk_WithTasksInDatabase()
    {
        // Arrange
        _dbContext.CrmTasks.Add(MakeTask());
        _dbContext.CrmTasks.Add(MakeTask());
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
    }

    // ── GetTask ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var result = await _controller.GetTask(9999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTask_ShouldReturnOk_WhenTaskExists()
    {
        // Arrange
        var task = MakeTask();
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetTask(task.Id);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetTasks with filter ─────────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_ShouldReturnOk_FilteredByStatus()
    {
        var task = MakeTask();
        task.Status = CrmTaskStatus.InProgress;
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetTasks(status: CrmTaskStatus.InProgress);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ── MapToDto — new fields (CrmTaskDto gap remediation) ────────────────────

    [Fact]
    public async Task GetTask_ShouldMapNewFields_TaskTypeStartDateEstimatedMinutesAccountIdOpportunityId()
    {
        // Arrange
        var task = MakeTask();
        task.TaskType = CrmTaskType.Meeting;
        task.StartDate = new DateTime(2026, 6, 10, 8, 30, 0, DateTimeKind.Utc);
        task.EstimatedMinutes = 45;
        task.AccountId = 21;
        task.OpportunityId = 34;
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetTask(task.Id);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<CrmTaskDto>().Subject;
        dto.TaskType.Should().Be((int)CrmTaskType.Meeting);
        dto.StartDate.Should().Be(task.StartDate.Value.ToString("o"));
        dto.EstimatedMinutes.Should().Be(45);
        dto.AccountId.Should().Be(21);
        dto.OpportunityId.Should().Be(34);
    }

    [Fact]
    public async Task GetTask_ShouldMapNullNewFields_WhenNotSet()
    {
        // Arrange
        var task = MakeTask();
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetTask(task.Id);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<CrmTaskDto>().Subject;
        dto.TaskType.Should().Be((int)CrmTaskType.Other);
        dto.StartDate.Should().BeNull();
        dto.EstimatedMinutes.Should().BeNull();
        dto.AccountId.Should().BeNull();
        dto.OpportunityId.Should().BeNull();
    }

    // ── DeleteTask — soft-delete regression (REM-BUG-010) ──────────────────────
    // Prior to this fix, DeleteTask hard-removed the row via _context.CrmTasks.Remove(task),
    // disagreeing with TaskService's soft-delete convention used everywhere else in the app.

    [Fact]
    public async Task DeleteTask_ShouldSoftDelete_NotHardDeleteRow()
    {
        // Arrange
        var task = MakeTask();
        _dbContext.CrmTasks.Add(task);
        await _dbContext.SaveChangesAsync();
        var taskId = task.Id;

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        var rowStillExists = await _dbContext.CrmTasks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == taskId);
        rowStillExists.Should().NotBeNull("delete must be a soft-delete, not a hard row removal");
        rowStillExists!.IsDeleted.Should().BeTrue();

        // And it should no longer be visible through the normal (non-deleted) query path.
        var getResult = await _controller.GetTask(taskId);
        getResult.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var result = await _controller.DeleteTask(9999);

        result.Should().BeOfType<NotFoundResult>();
    }
}

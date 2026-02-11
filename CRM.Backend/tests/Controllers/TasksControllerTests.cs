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
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for TasksController
/// Covers: CRUD operations, assignment, priority, status, reminders, recurring
/// </summary>
public class TasksControllerTests
{
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockTaskService = new Mock<ITaskService>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new TasksController(_mockTaskService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, Subject = "Call customer", DueDate = DateTime.Today.AddDays(1) },
            new CrmTaskDto { Id = 2, Subject = "Send proposal", DueDate = DateTime.Today.AddDays(2) }
        };

        _mockTaskService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value as IEnumerable<CrmTaskDto>;
        returnedTasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyTasks_ReturnsUserTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, AssignedToId = 1 }
        };

        _mockTaskService.Setup(s => s.GetByAssigneeAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetMyTasks();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByStatus_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, Status = "InProgress" }
        };

        _mockTaskService.Setup(s => s.GetByStatusAsync("InProgress"))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByStatus("InProgress");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByPriority_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, Priority = "High" }
        };

        _mockTaskService.Setup(s => s.GetByPriorityAsync("High"))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByPriority("High");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetOverdue_ReturnsOverdueTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, DueDate = DateTime.Today.AddDays(-1) }
        };

        _mockTaskService.Setup(s => s.GetOverdueAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetOverdue();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetDueToday_ReturnsTodayTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, DueDate = DateTime.Today }
        };

        _mockTaskService.Setup(s => s.GetDueTodayAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetDueToday();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetDueThisWeek_ReturnsWeekTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, DueDate = DateTime.Today.AddDays(3) }
        };

        _mockTaskService.Setup(s => s.GetDueThisWeekAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetDueThisWeek();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingTask_ReturnsOkWithTask()
    {
        // Arrange
        var task = new CrmTaskDto { Id = 1, Subject = "Call customer" };

        _mockTaskService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value as CrmTaskDto;
        returnedTask!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        _mockTaskService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((CrmTaskDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidTask_ReturnsCreatedWithTask()
    {
        // Arrange
        var createDto = new CreateCrmTaskDto
        {
            Subject = "Follow up call",
            Description = "Call customer about proposal",
            DueDate = DateTime.Today.AddDays(1),
            Priority = "High"
        };

        var createdTask = new CrmTaskDto
        {
            Id = 1,
            Subject = createDto.Subject,
            Status = "NotStarted"
        };

        _mockTaskService.Setup(s => s.CreateAsync(It.IsAny<CreateCrmTaskDto>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedTask = createdResult.Value as CrmTaskDto;
        returnedTask!.Status.Should().Be("NotStarted");
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_MissingSubject_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCrmTaskDto { Description = "No subject" };

        _mockTaskService.Setup(s => s.CreateAsync(It.IsAny<CreateCrmTaskDto>()))
            .ThrowsAsync(new ArgumentException("Subject is required"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_PastDueDate_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCrmTaskDto
        {
            Subject = "Test",
            DueDate = DateTime.Today.AddDays(-1)
        };

        _mockTaskService.Setup(s => s.CreateAsync(It.IsAny<CreateCrmTaskDto>()))
            .ThrowsAsync(new ArgumentException("Due date cannot be in the past"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidTask_ReturnsOkWithUpdatedTask()
    {
        // Arrange
        var updateDto = new UpdateCrmTaskDto
        {
            Id = 1,
            Subject = "Updated subject",
            Priority = "Medium"
        };

        var updatedTask = new CrmTaskDto
        {
            Id = 1,
            Subject = "Updated subject",
            Priority = "Medium"
        };

        _mockTaskService.Setup(s => s.UpdateAsync(It.IsAny<UpdateCrmTaskDto>()))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateCrmTaskDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateCrmTaskDto { Id = 999 };

        _mockTaskService.Setup(s => s.UpdateAsync(It.IsAny<UpdateCrmTaskDto>()))
            .ReturnsAsync((CrmTaskDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task UpdateStatus_ValidStatus_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.UpdateStatusAsync(1, "InProgress"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateStatus(1, "InProgress");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Complete_ValidTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.CompleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Complete(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Complete_AlreadyCompleted_ReturnsConflict()
    {
        // Arrange
        _mockTaskService.Setup(s => s.CompleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Task is already completed"));

        // Act
        var result = await _controller.Complete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Reopen_CompletedTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ReopenAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reopen(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Cancel_ValidTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.CancelAsync(1, "No longer needed"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Cancel(1, "No longer needed");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public async Task Assign_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.AssignAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Assign(1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Unassign_ValidTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.UnassignAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Unassign(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetByAssignee_ReturnsAssigneeTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, AssignedToId = 2 }
        };

        _mockTaskService.Setup(s => s.GetByAssigneeAsync(2))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByAssignee(2);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Reassign_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ReassignAsync(1, 2, 3))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reassign(1, 3);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Priority Tests

    [Fact]
    public async Task UpdatePriority_ValidPriority_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.UpdatePriorityAsync(1, "Critical"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdatePriority(1, "Critical");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdatePriority_InvalidPriority_ReturnsBadRequest()
    {
        // Arrange
        _mockTaskService.Setup(s => s.UpdatePriorityAsync(1, "Invalid"))
            .ThrowsAsync(new ArgumentException("Invalid priority"));

        // Act
        var result = await _controller.UpdatePriority(1, "Invalid");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Due Date Tests

    [Fact]
    public async Task UpdateDueDate_ValidDate_ReturnsOk()
    {
        // Arrange
        var newDate = DateTime.Today.AddDays(5);

        _mockTaskService.Setup(s => s.UpdateDueDateAsync(1, newDate))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateDueDate(1, newDate);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Snooze_ValidDuration_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.SnoozeAsync(1, TimeSpan.FromDays(1)))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Snooze(1, TimeSpan.FromDays(1));

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetByDateRange_ReturnsTasksInRange()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(7);

        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, DueDate = DateTime.Today.AddDays(3) }
        };

        _mockTaskService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Reminder Tests

    [Fact]
    public async Task SetReminder_ValidRequest_ReturnsOk()
    {
        // Arrange
        var reminderTime = DateTime.Now.AddHours(2);

        _mockTaskService.Setup(s => s.SetReminderAsync(1, reminderTime))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetReminder(1, reminderTime);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ClearReminder_ValidTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.ClearReminderAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ClearReminder(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetUpcomingReminders_ReturnsReminders()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, ReminderTime = DateTime.Now.AddMinutes(30) }
        };

        _mockTaskService.Setup(s => s.GetUpcomingRemindersAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetUpcomingReminders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Recurring Tasks Tests

    [Fact]
    public async Task CreateRecurring_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateRecurringTaskDto
        {
            Subject = "Weekly review",
            RecurrencePattern = "Weekly",
            StartDate = DateTime.Today
        };

        var createdTask = new CrmTaskDto { Id = 1, Subject = createDto.Subject, IsRecurring = true };

        _mockTaskService.Setup(s => s.CreateRecurringAsync(createDto))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateRecurring(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateRecurrence_ValidRequest_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateRecurrenceDto
        {
            RecurrencePattern = "Monthly"
        };

        _mockTaskService.Setup(s => s.UpdateRecurrenceAsync(1, updateDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateRecurrence(1, updateDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task StopRecurrence_ValidTask_ReturnsOk()
    {
        // Arrange
        _mockTaskService.Setup(s => s.StopRecurrenceAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.StopRecurrence(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Entity Association Tests

    [Fact]
    public async Task GetByAccount_ReturnsAccountTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, RelatedAccountId = 1 }
        };

        _mockTaskService.Setup(s => s.GetByAccountAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByContact_ReturnsContactTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, RelatedContactId = 1 }
        };

        _mockTaskService.Setup(s => s.GetByContactAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByContact(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByOpportunity_ReturnsOpportunityTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, RelatedOpportunityId = 1 }
        };

        _mockTaskService.Setup(s => s.GetByOpportunityAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetByOpportunity(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Subtask Tests

    [Fact]
    public async Task AddSubtask_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var subtaskDto = new CreateSubtaskDto
        {
            Subject = "Subtask 1"
        };

        var createdSubtask = new CrmTaskDto { Id = 2, Subject = subtaskDto.Subject, ParentTaskId = 1 };

        _mockTaskService.Setup(s => s.AddSubtaskAsync(1, subtaskDto))
            .ReturnsAsync(createdSubtask);

        // Act
        var result = await _controller.AddSubtask(1, subtaskDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetSubtasks_ReturnsSubtasks()
    {
        // Arrange
        var subtasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 2, ParentTaskId = 1 },
            new CrmTaskDto { Id = 3, ParentTaskId = 1 }
        };

        _mockTaskService.Setup(s => s.GetSubtasksAsync(1))
            .ReturnsAsync(subtasks);

        // Act
        var result = await _controller.GetSubtasks(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkComplete_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockTaskService.Setup(s => s.BulkCompleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkComplete(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkAssign_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkAssignTasksRequest
        {
            TaskIds = new List<int> { 1, 2, 3 },
            AssigneeId = 1
        };

        _mockTaskService.Setup(s => s.BulkAssignAsync(request.TaskIds, request.AssigneeId))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAssign(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockTaskService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkUpdatePriority_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkUpdatePriorityRequest
        {
            TaskIds = new List<int> { 1, 2, 3 },
            Priority = "High"
        };

        _mockTaskService.Setup(s => s.BulkUpdatePriorityAsync(request.TaskIds, request.Priority))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkUpdatePriority(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingTasks()
    {
        // Arrange
        var tasks = new List<CrmTaskDto>
        {
            new CrmTaskDto { Id = 1, Subject = "Call customer" }
        };

        _mockTaskService.Setup(s => s.SearchAsync("call"))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.Search("call");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_CompletedTask_ReturnsConflict()
    {
        // Arrange
        _mockTaskService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete completed tasks"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new TaskStatisticsDto
        {
            TotalTasks = 100,
            CompletedTasks = 60,
            OverdueTasks = 10
        };

        _mockTaskService.Setup(s => s.GetStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion
}

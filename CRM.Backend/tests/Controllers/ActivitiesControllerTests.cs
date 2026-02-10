// CRM Solution - Customer Relationship Management System
// Activities Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ActivitiesController
/// Covers: Activity CRUD, timeline, types, filtering
/// </summary>
public class ActivitiesControllerTests
{
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<ActivitiesController>> _mockLogger;
    private readonly ActivitiesController _controller;

    public ActivitiesControllerTests()
    {
        _mockActivityService = new Mock<IActivityService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<ActivitiesController>>();

        _controller = new ActivitiesController(
            _mockActivityService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Type = "Call", Subject = "Follow-up call" },
            new ActivityDto { Id = 2, Type = "Email", Subject = "Introduction email" }
        };

        _mockActivityService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        returnedActivities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithTypeFilter_ReturnsFilteredActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Type = "Call" }
        };

        _mockActivityService.Setup(s => s.GetByTypeAsync("Call"))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetByType("Call");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        returnedActivities.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByEntity_ReturnsEntityActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, EntityType = "Account", EntityId = 1 }
        };

        _mockActivityService.Setup(s => s.GetByEntityAsync("Account", 1))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetByEntity("Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        returnedActivities.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByDateRange_ReturnsActivitiesInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, CreatedAt = DateTime.Today.AddDays(-3) }
        };

        _mockActivityService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        returnedActivities.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUser_ReturnsUserActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, UserId = 5 }
        };

        _mockActivityService.Setup(s => s.GetByUserAsync(5))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetByUser(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingActivity_ReturnsOk()
    {
        // Arrange
        var activity = new ActivityDto { Id = 1, Type = "Call", Subject = "Test call" };

        _mockActivityService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(activity);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivity = okResult.Value.Should().BeOfType<ActivityDto>().Subject;
        returnedActivity.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingActivity_ReturnsNotFound()
    {
        // Arrange
        _mockActivityService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((ActivityDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidActivity_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Type = "Call",
            Subject = "Follow-up call",
            EntityType = "Account",
            EntityId = 1
        };

        var createdActivity = new ActivityDto
        {
            Id = 1,
            Type = "Call",
            Subject = "Follow-up call",
            EntityType = "Account",
            EntityId = 1
        };

        _mockActivityService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdActivity);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
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
    public async Task Create_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Type", "Type is required");

        // Act
        var result = await _controller.Create(new CreateActivityDto());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CallActivity_SetsCorrectType()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Type = "Call",
            Subject = "Sales call",
            EntityType = "Account",
            EntityId = 1,
            Duration = 30
        };

        _mockActivityService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new ActivityDto { Id = 1, Type = "Call" });
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_EmailActivity_SetsCorrectType()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Type = "Email",
            Subject = "Introduction email",
            EntityType = "Contact",
            EntityId = 5
        };

        _mockActivityService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new ActivityDto { Id = 1, Type = "Email" });
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_MeetingActivity_SetsCorrectType()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Type = "Meeting",
            Subject = "Quarterly review",
            EntityType = "Account",
            EntityId = 1,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        _mockActivityService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new ActivityDto { Id = 1, Type = "Meeting" });
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidActivity_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateActivityDto
        {
            Id = 1,
            Subject = "Updated call",
            Description = "Updated description"
        };

        var updatedActivity = new ActivityDto
        {
            Id = 1,
            Subject = "Updated call",
            Description = "Updated description"
        };

        _mockActivityService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedActivity);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivity = okResult.Value.Should().BeOfType<ActivityDto>().Subject;
        returnedActivity.Subject.Should().Be("Updated call");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateActivityDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingActivity_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateActivityDto { Id = 999 };

        _mockActivityService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((ActivityDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingActivity_ReturnsNoContent()
    {
        // Arrange
        _mockActivityService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingActivity_ReturnsNotFound()
    {
        // Arrange
        _mockActivityService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Timeline Tests

    [Fact]
    public async Task GetTimeline_ReturnsTimelineActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, CreatedAt = DateTime.Now.AddDays(-1) },
            new ActivityDto { Id = 2, CreatedAt = DateTime.Now }
        };

        _mockActivityService.Setup(s => s.GetTimelineAsync("Account", 1))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetTimeline("Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var timeline = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        timeline.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentActivities_ReturnsRecentItems()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, CreatedAt = DateTime.Now.AddHours(-1) },
            new ActivityDto { Id = 2, CreatedAt = DateTime.Now.AddMinutes(-30) }
        };

        _mockActivityService.Setup(s => s.GetRecentAsync(10))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetRecent(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var recent = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        recent.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUpcoming_ReturnsUpcomingActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, StartTime = DateTime.Now.AddDays(1) },
            new ActivityDto { Id = 2, StartTime = DateTime.Now.AddDays(2) }
        };

        _mockActivityService.Setup(s => s.GetUpcomingAsync(7))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetUpcoming(7);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var upcoming = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        upcoming.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOverdue_ReturnsOverdueActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, DueDate = DateTime.Now.AddDays(-1), IsCompleted = false }
        };

        _mockActivityService.Setup(s => s.GetOverdueAsync())
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetOverdue();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var overdue = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        overdue.Should().HaveCount(1);
    }

    #endregion

    #region Activity Type Tests

    [Fact]
    public async Task GetActivityTypes_ReturnsAvailableTypes()
    {
        // Arrange
        var types = new List<string> { "Call", "Email", "Meeting", "Task", "Note" };

        _mockActivityService.Setup(s => s.GetActivityTypesAsync())
            .ReturnsAsync(types);

        // Act
        var result = await _controller.GetActivityTypes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTypes = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedTypes.Should().Contain("Call");
    }

    [Fact]
    public async Task GetActivityStats_ReturnsStatistics()
    {
        // Arrange
        var stats = new ActivityStatsDto
        {
            TotalActivities = 100,
            CallsToday = 5,
            EmailsToday = 10,
            MeetingsToday = 2
        };

        _mockActivityService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<ActivityStatsDto>().Subject;
        returnedStats.TotalActivities.Should().Be(100);
    }

    #endregion

    #region Complete/Mark Tests

    [Fact]
    public async Task MarkComplete_ValidActivity_ReturnsOk()
    {
        // Arrange
        _mockActivityService.Setup(s => s.MarkCompleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkComplete(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarkComplete_NonExistingActivity_ReturnsNotFound()
    {
        // Arrange
        _mockActivityService.Setup(s => s.MarkCompleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkComplete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MarkIncomplete_ValidActivity_ReturnsOk()
    {
        // Arrange
        _mockActivityService.Setup(s => s.MarkIncompleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkIncomplete(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockActivityService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    [Fact]
    public async Task BulkComplete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockActivityService.Setup(s => s.BulkMarkCompleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkComplete(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { CompletedCount = 3 });
    }

    #endregion

    #region Assign Tests

    [Fact]
    public async Task AssignActivity_ValidAssignment_ReturnsOk()
    {
        // Arrange
        var assignRequest = new AssignActivityDto { ActivityId = 1, UserId = 5 };

        _mockActivityService.Setup(s => s.AssignAsync(1, 5))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Assign(assignRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AssignActivity_NonExistingActivity_ReturnsNotFound()
    {
        // Arrange
        var assignRequest = new AssignActivityDto { ActivityId = 999, UserId = 5 };

        _mockActivityService.Setup(s => s.AssignAsync(999, 5))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Assign(assignRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Subject = "Sales call with John" }
        };

        _mockActivityService.Setup(s => s.SearchAsync("John"))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.Search("John");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<IEnumerable<ActivityDto>>().Subject;
        searchResults.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}

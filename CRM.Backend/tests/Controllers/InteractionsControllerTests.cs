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
/// Unit tests for InteractionsController
/// Covers: Interaction CRUD, types, entity linking, timeline
/// </summary>
public class InteractionsControllerTests
{
    private readonly Mock<IInteractionService> _mockInteractionService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<InteractionsController>> _mockLogger;
    private readonly InteractionsController _controller;

    public InteractionsControllerTests()
    {
        _mockInteractionService = new Mock<IInteractionService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<InteractionsController>>();

        _controller = new InteractionsController(
            _mockInteractionService.Object,
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
    public async Task GetAll_ReturnsOkWithInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, Type = "Call", Subject = "Sales call" },
            new InteractionDto { Id = 2, Type = "Email", Subject = "Follow-up" }
        };

        _mockInteractionService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInteractions = okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>().Subject;
        returnedInteractions.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByType_ReturnsFilteredInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, Type = "Call" }
        };

        _mockInteractionService.Setup(s => s.GetByTypeAsync("Call"))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetByType("Call");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInteractions = okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>().Subject;
        returnedInteractions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByAccount_ReturnsAccountInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, AccountId = 1 }
        };

        _mockInteractionService.Setup(s => s.GetByAccountAsync(1))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetByAccount(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>();
    }

    [Fact]
    public async Task GetByContact_ReturnsContactInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, ContactId = 5 }
        };

        _mockInteractionService.Setup(s => s.GetByContactAsync(5))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetByContact(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>();
    }

    [Fact]
    public async Task GetByOpportunity_ReturnsOpportunityInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, OpportunityId = 10 }
        };

        _mockInteractionService.Setup(s => s.GetByOpportunityAsync(10))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetByOpportunity(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>();
    }

    [Fact]
    public async Task GetByDateRange_ReturnsInteractionsInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, InteractionDate = DateTime.Today.AddDays(-10) }
        };

        _mockInteractionService.Setup(s => s.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetByDateRange(startDate, endDate);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingInteraction_ReturnsOk()
    {
        // Arrange
        var interaction = new InteractionDto
        {
            Id = 1,
            Type = "Call",
            Subject = "Initial contact"
        };

        _mockInteractionService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(interaction);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInteraction = okResult.Value.Should().BeOfType<InteractionDto>().Subject;
        returnedInteraction.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingInteraction_ReturnsNotFound()
    {
        // Arrange
        _mockInteractionService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((InteractionDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidInteraction_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateInteractionDto
        {
            Type = "Call",
            Subject = "Discovery call",
            AccountId = 1,
            Duration = 30
        };

        var createdInteraction = new InteractionDto
        {
            Id = 1,
            Type = "Call",
            Subject = "Discovery call"
        };

        _mockInteractionService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdInteraction);
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
    public async Task Create_CallInteraction_SetsTypeCorrectly()
    {
        // Arrange
        var createDto = new CreateInteractionDto
        {
            Type = "Call",
            Subject = "Phone call",
            AccountId = 1,
            Direction = "Outbound",
            Duration = 15
        };

        _mockInteractionService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new InteractionDto { Id = 1, Type = "Call" });
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_EmailInteraction_SetsTypeCorrectly()
    {
        // Arrange
        var createDto = new CreateInteractionDto
        {
            Type = "Email",
            Subject = "Follow-up email",
            ContactId = 5,
            Direction = "Outbound"
        };

        _mockInteractionService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new InteractionDto { Id = 1, Type = "Email" });
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_MeetingInteraction_SetsTypeCorrectly()
    {
        // Arrange
        var createDto = new CreateInteractionDto
        {
            Type = "Meeting",
            Subject = "Product demo",
            OpportunityId = 10,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(1)
        };

        _mockInteractionService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new InteractionDto { Id = 1, Type = "Meeting" });
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
    public async Task Update_ValidInteraction_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateInteractionDto
        {
            Id = 1,
            Subject = "Updated subject",
            Notes = "Additional notes"
        };

        var updatedInteraction = new InteractionDto
        {
            Id = 1,
            Subject = "Updated subject",
            Notes = "Additional notes"
        };

        _mockInteractionService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedInteraction);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedInteraction = okResult.Value.Should().BeOfType<InteractionDto>().Subject;
        returnedInteraction.Subject.Should().Be("Updated subject");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateInteractionDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingInteraction_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateInteractionDto { Id = 999 };

        _mockInteractionService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((InteractionDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingInteraction_ReturnsNoContent()
    {
        // Arrange
        _mockInteractionService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingInteraction_ReturnsNotFound()
    {
        // Arrange
        _mockInteractionService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Interaction Types Tests

    [Fact]
    public async Task GetInteractionTypes_ReturnsAvailableTypes()
    {
        // Arrange
        var types = new List<string>
        {
            "Call", "Email", "Meeting", "Note", "Task", "Demo", "Proposal"
        };

        _mockInteractionService.Setup(s => s.GetTypesAsync())
            .ReturnsAsync(types);

        // Act
        var result = await _controller.GetTypes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTypes = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedTypes.Should().Contain("Call");
    }

    [Fact]
    public async Task GetDirections_ReturnsDirectionOptions()
    {
        // Arrange
        var directions = new List<string> { "Inbound", "Outbound", "Internal" };

        _mockInteractionService.Setup(s => s.GetDirectionsAsync())
            .ReturnsAsync(directions);

        // Act
        var result = await _controller.GetDirections();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDirections = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedDirections.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOutcomes_ReturnsOutcomeOptions()
    {
        // Arrange
        var outcomes = new List<string>
        {
            "Positive", "Negative", "Neutral", "Follow-up Required", "No Answer"
        };

        _mockInteractionService.Setup(s => s.GetOutcomesAsync())
            .ReturnsAsync(outcomes);

        // Act
        var result = await _controller.GetOutcomes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOutcomes = okResult.Value.Should().BeAssignableTo<IEnumerable<string>>().Subject;
        returnedOutcomes.Should().Contain("Positive");
    }

    #endregion

    #region Timeline Tests

    [Fact]
    public async Task GetAccountTimeline_ReturnsTimelineItems()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, AccountId = 1, InteractionDate = DateTime.Now.AddDays(-1) },
            new InteractionDto { Id = 2, AccountId = 1, InteractionDate = DateTime.Now }
        };

        _mockInteractionService.Setup(s => s.GetTimelineAsync("Account", 1))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetTimeline("Account", 1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var timeline = okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>().Subject;
        timeline.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentInteractions_ReturnsRecentItems()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, InteractionDate = DateTime.Now.AddHours(-1) }
        };

        _mockInteractionService.Setup(s => s.GetRecentAsync(10))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.GetRecent(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsInteractionStats()
    {
        // Arrange
        var stats = new InteractionStatsDto
        {
            TotalInteractions = 500,
            CallsToday = 15,
            EmailsToday = 25,
            MeetingsThisWeek = 8,
            AverageCallDuration = 12.5
        };

        _mockInteractionService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<InteractionStatsDto>().Subject;
        returnedStats.TotalInteractions.Should().Be(500);
    }

    [Fact]
    public async Task GetStatsByType_ReturnsGroupedStats()
    {
        // Arrange
        var stats = new Dictionary<string, int>
        {
            { "Call", 200 },
            { "Email", 250 },
            { "Meeting", 50 }
        };

        _mockInteractionService.Setup(s => s.GetStatsByTypeAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatsByType();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeAssignableTo<Dictionary<string, int>>().Subject;
        returnedStats["Call"].Should().Be(200);
    }

    [Fact]
    public async Task GetUserStats_ReturnsUserInteractionStats()
    {
        // Arrange
        var stats = new UserInteractionStatsDto
        {
            UserId = 1,
            TotalInteractions = 150,
            CallsThisMonth = 50,
            EmailsThisMonth = 75
        };

        _mockInteractionService.Setup(s => s.GetUserStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetUserStats(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<UserInteractionStatsDto>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingInteractions()
    {
        // Arrange
        var interactions = new List<InteractionDto>
        {
            new InteractionDto { Id = 1, Subject = "Product demo discussion" }
        };

        _mockInteractionService.Setup(s => s.SearchAsync("demo"))
            .ReturnsAsync(interactions);

        // Act
        var result = await _controller.Search("demo");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<IEnumerable<InteractionDto>>().Subject;
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

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockInteractionService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    [Fact]
    public async Task LogBulkCall_ValidCalls_ReturnsOkWithCount()
    {
        // Arrange
        var callsToLog = new List<BulkCallLogDto>
        {
            new BulkCallLogDto { ContactId = 1, Duration = 10, Outcome = "Positive" },
            new BulkCallLogDto { ContactId = 2, Duration = 5, Outcome = "No Answer" }
        };

        _mockInteractionService.Setup(s => s.LogBulkCallsAsync(callsToLog))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.LogBulkCalls(callsToLog);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { LoggedCount = 2 });
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task Export_ReturnsFileResult()
    {
        // Arrange
        var exportData = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // Excel file header

        _mockInteractionService.Setup(s => s.ExportAsync("xlsx", null, null))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.Export("xlsx");

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task Export_WithDateRange_ReturnsFilteredData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var exportData = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

        _mockInteractionService.Setup(s => s.ExportAsync("csv", startDate, endDate))
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.Export("csv", startDate, endDate);

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion
}

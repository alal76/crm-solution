// CRM Solution - Customer Relationship Management System
// Stages Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for StagesController
/// Covers: Stage CRUD, ordering, probability, transitions
/// </summary>
public class StagesControllerTests
{
    private readonly Mock<IStageService> _mockStageService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<StagesController>> _mockLogger;
    private readonly StagesController _controller;

    public StagesControllerTests()
    {
        _mockStageService = new Mock<IStageService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<StagesController>>();

        _controller = new StagesController(
            _mockStageService.Object,
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
    public async Task GetAll_ReturnsOkWithStages()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Qualification", Order = 1, Probability = 10 },
            new StageDto { Id = 2, Name = "Proposal", Order = 2, Probability = 40 }
        };

        _mockStageService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStages = okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>().Subject;
        returnedStages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByPipeline_ReturnsStagesForPipeline()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Lead", PipelineId = 1 },
            new StageDto { Id = 2, Name = "Qualified", PipelineId = 1 }
        };

        _mockStageService.Setup(s => s.GetByPipelineAsync(1))
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetByPipeline(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStages = okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>().Subject;
        returnedStages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActive_ReturnsActiveStages()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Active Stage", IsActive = true }
        };

        _mockStageService.Setup(s => s.GetActiveAsync())
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>();
    }

    [Fact]
    public async Task GetClosedWonStages_ReturnsClosedWonStages()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Closed Won", IsClosedWon = true, Probability = 100 }
        };

        _mockStageService.Setup(s => s.GetClosedWonStagesAsync())
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetClosedWon();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>();
    }

    [Fact]
    public async Task GetClosedLostStages_ReturnsClosedLostStages()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Closed Lost", IsClosedLost = true, Probability = 0 }
        };

        _mockStageService.Setup(s => s.GetClosedLostStagesAsync())
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetClosedLost();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingStage_ReturnsOk()
    {
        // Arrange
        var stage = new StageDto
        {
            Id = 1,
            Name = "Qualification",
            Order = 1,
            Probability = 10,
            PipelineId = 1
        };

        _mockStageService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(stage);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStage = okResult.Value.Should().BeOfType<StageDto>().Subject;
        returnedStage.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingStage_ReturnsNotFound()
    {
        // Arrange
        _mockStageService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((StageDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidStage_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateStageDto
        {
            Name = "New Stage",
            PipelineId = 1,
            Order = 3,
            Probability = 50
        };

        var createdStage = new StageDto
        {
            Id = 3,
            Name = "New Stage",
            PipelineId = 1,
            Order = 3,
            Probability = 50
        };

        _mockStageService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdStage);
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
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateStageDto
        {
            Name = "Existing Stage",
            PipelineId = 1
        };

        _mockStageService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Stage with this name already exists in the pipeline"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidProbability_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Probability", "Probability must be between 0 and 100");

        // Act
        var result = await _controller.Create(new CreateStageDto { Probability = 150 });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ClosedWonStage_SetsProbabilityTo100()
    {
        // Arrange
        var createDto = new CreateStageDto
        {
            Name = "Closed Won",
            PipelineId = 1,
            IsClosedWon = true
        };

        var createdStage = new StageDto
        {
            Id = 1,
            Name = "Closed Won",
            IsClosedWon = true,
            Probability = 100
        };

        _mockStageService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdStage);
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
    public async Task Update_ValidStage_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateStageDto
        {
            Id = 1,
            Name = "Updated Stage",
            Probability = 60
        };

        var updatedStage = new StageDto
        {
            Id = 1,
            Name = "Updated Stage",
            Probability = 60
        };

        _mockStageService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedStage);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStage = okResult.Value.Should().BeOfType<StageDto>().Subject;
        returnedStage.Name.Should().Be("Updated Stage");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateStageDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingStage_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateStageDto { Id = 999 };

        _mockStageService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((StageDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingStage_ReturnsNoContent()
    {
        // Arrange
        _mockStageService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingStage_ReturnsNotFound()
    {
        // Arrange
        _mockStageService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_StageWithOpportunities_ReturnsConflict()
    {
        // Arrange
        _mockStageService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Stage has active opportunities"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task Reorder_ValidOrder_ReturnsOk()
    {
        // Arrange
        var reorderRequest = new ReorderStagesDto
        {
            StageIds = new List<int> { 3, 1, 2 }
        };

        _mockStageService.Setup(s => s.ReorderAsync(1, reorderRequest.StageIds))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reorder(1, reorderRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Reorder_InvalidStageIds_ReturnsBadRequest()
    {
        // Arrange
        var reorderRequest = new ReorderStagesDto
        {
            StageIds = new List<int> { 999, 998 }
        };

        _mockStageService.Setup(s => s.ReorderAsync(1, reorderRequest.StageIds))
            .ThrowsAsync(new ArgumentException("Invalid stage IDs provided"));

        // Act
        var result = await _controller.Reorder(1, reorderRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MoveUp_ValidStage_ReturnsOk()
    {
        // Arrange
        _mockStageService.Setup(s => s.MoveUpAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MoveUp(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MoveDown_ValidStage_ReturnsOk()
    {
        // Arrange
        _mockStageService.Setup(s => s.MoveDownAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MoveDown(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MoveUp_FirstStage_ReturnsBadRequest()
    {
        // Arrange
        _mockStageService.Setup(s => s.MoveUpAsync(1))
            .ThrowsAsync(new InvalidOperationException("Stage is already at the top"));

        // Act
        var result = await _controller.MoveUp(1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Probability Tests

    [Fact]
    public async Task UpdateProbability_ValidProbability_ReturnsOk()
    {
        // Arrange
        var probabilityRequest = new UpdateStageProbabilityDto
        {
            StageId = 1,
            Probability = 75
        };

        _mockStageService.Setup(s => s.UpdateProbabilityAsync(1, 75))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProbability(probabilityRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdateProbability_InvalidProbability_ReturnsBadRequest()
    {
        // Arrange
        var probabilityRequest = new UpdateStageProbabilityDto
        {
            StageId = 1,
            Probability = 150
        };

        _mockStageService.Setup(s => s.UpdateProbabilityAsync(1, 150))
            .ThrowsAsync(new ArgumentException("Probability must be between 0 and 100"));

        // Act
        var result = await _controller.UpdateProbability(probabilityRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Stage Transitions Tests

    [Fact]
    public async Task GetAllowedTransitions_ReturnsAllowedNextStages()
    {
        // Arrange
        var allowedStages = new List<StageDto>
        {
            new StageDto { Id = 2, Name = "Proposal", Order = 2 },
            new StageDto { Id = 3, Name = "Negotiation", Order = 3 }
        };

        _mockStageService.Setup(s => s.GetAllowedTransitionsAsync(1))
            .ReturnsAsync(allowedStages);

        // Act
        var result = await _controller.GetAllowedTransitions(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var transitions = okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>().Subject;
        transitions.Should().HaveCount(2);
    }

    [Fact]
    public async Task SetAllowedTransitions_ValidTransitions_ReturnsOk()
    {
        // Arrange
        var transitionRequest = new SetStageTransitionsDto
        {
            StageId = 1,
            AllowedTransitionIds = new List<int> { 2, 3 }
        };

        _mockStageService.Setup(s => s.SetAllowedTransitionsAsync(1, transitionRequest.AllowedTransitionIds))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetAllowedTransitions(transitionRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Stage Statistics Tests

    [Fact]
    public async Task GetStageStats_ReturnsStats()
    {
        // Arrange
        var stats = new StageStatsDto
        {
            StageId = 1,
            StageName = "Qualification",
            OpportunityCount = 25,
            TotalValue = 500000,
            AverageDaysInStage = 12.5
        };

        _mockStageService.Setup(s => s.GetStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<StageStatsDto>().Subject;
        returnedStats.OpportunityCount.Should().Be(25);
    }

    [Fact]
    public async Task GetConversionRate_ReturnsRate()
    {
        // Arrange
        var conversionRate = 75.5;

        _mockStageService.Setup(s => s.GetConversionRateAsync(1))
            .ReturnsAsync(conversionRate);

        // Act
        var result = await _controller.GetConversionRate(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { ConversionRate = 75.5 });
    }

    [Fact]
    public async Task GetOpportunitiesInStage_ReturnsOpportunities()
    {
        // Arrange
        var opportunities = new List<OpportunityDto>
        {
            new OpportunityDto { Id = 1, Name = "Deal 1", StageId = 1 },
            new OpportunityDto { Id = 2, Name = "Deal 2", StageId = 1 }
        };

        _mockStageService.Setup(s => s.GetOpportunitiesInStageAsync(1))
            .ReturnsAsync(opportunities);

        // Act
        var result = await _controller.GetOpportunities(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOpps = okResult.Value.Should().BeAssignableTo<IEnumerable<OpportunityDto>>().Subject;
        returnedOpps.Should().HaveCount(2);
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task Activate_ValidStage_ReturnsOk()
    {
        // Arrange
        _mockStageService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ValidStage_ReturnsOk()
    {
        // Arrange
        _mockStageService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_StageWithOpportunities_ReturnsConflict()
    {
        // Arrange
        _mockStageService.Setup(s => s.DeactivateAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot deactivate stage with active opportunities"));

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion
}

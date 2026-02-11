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
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for PipelinesController
/// Covers: Pipeline CRUD, stages, ordering, assignment
/// </summary>
public class PipelinesControllerTests
{
    private readonly Mock<IPipelineService> _mockPipelineService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<PipelinesController>> _mockLogger;
    private readonly PipelinesController _controller;

    public PipelinesControllerTests()
    {
        _mockPipelineService = new Mock<IPipelineService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<PipelinesController>>();

        _controller = new PipelinesController(
            _mockPipelineService.Object,
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
    public async Task GetAll_ReturnsOkWithPipelines()
    {
        // Arrange
        var pipelines = new List<PipelineDto>
        {
            new PipelineDto { Id = 1, Name = "Sales Pipeline", IsActive = true },
            new PipelineDto { Id = 2, Name = "Enterprise Pipeline", IsActive = true }
        };

        _mockPipelineService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(pipelines);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPipelines = okResult.Value.Should().BeAssignableTo<IEnumerable<PipelineDto>>().Subject;
        returnedPipelines.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActive_ReturnsActivePipelines()
    {
        // Arrange
        var pipelines = new List<PipelineDto>
        {
            new PipelineDto { Id = 1, Name = "Sales Pipeline", IsActive = true }
        };

        _mockPipelineService.Setup(s => s.GetActiveAsync())
            .ReturnsAsync(pipelines);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPipelines = okResult.Value.Should().BeAssignableTo<IEnumerable<PipelineDto>>().Subject;
        returnedPipelines.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDefault_ReturnsDefaultPipeline()
    {
        // Arrange
        var pipeline = new PipelineDto { Id = 1, Name = "Sales Pipeline", IsDefault = true };

        _mockPipelineService.Setup(s => s.GetDefaultAsync())
            .ReturnsAsync(pipeline);

        // Act
        var result = await _controller.GetDefault();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPipeline = okResult.Value.Should().BeOfType<PipelineDto>().Subject;
        returnedPipeline.IsDefault.Should().BeTrue();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingPipeline_ReturnsOk()
    {
        // Arrange
        var pipeline = new PipelineDto
        {
            Id = 1,
            Name = "Sales Pipeline",
            Stages = new List<StageDto>
            {
                new StageDto { Id = 1, Name = "Qualification", Order = 1 },
                new StageDto { Id = 2, Name = "Proposal", Order = 2 }
            }
        };

        _mockPipelineService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(pipeline);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPipeline = okResult.Value.Should().BeOfType<PipelineDto>().Subject;
        returnedPipeline.Id.Should().Be(1);
        returnedPipeline.Stages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_NonExistingPipeline_ReturnsNotFound()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((PipelineDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidPipeline_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreatePipelineDto
        {
            Name = "New Pipeline",
            Description = "New sales pipeline"
        };

        var createdPipeline = new PipelineDto
        {
            Id = 1,
            Name = "New Pipeline",
            Description = "New sales pipeline",
            IsActive = true
        };

        _mockPipelineService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdPipeline);
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
        var createDto = new CreatePipelineDto { Name = "Existing Pipeline" };

        _mockPipelineService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Pipeline with this name already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_WithStages_ReturnsCreatedWithStages()
    {
        // Arrange
        var createDto = new CreatePipelineDto
        {
            Name = "New Pipeline",
            Stages = new List<CreateStageDto>
            {
                new CreateStageDto { Name = "Lead", Order = 1 },
                new CreateStageDto { Name = "Qualified", Order = 2 }
            }
        };

        var createdPipeline = new PipelineDto
        {
            Id = 1,
            Name = "New Pipeline",
            Stages = new List<StageDto>
            {
                new StageDto { Id = 1, Name = "Lead", Order = 1 },
                new StageDto { Id = 2, Name = "Qualified", Order = 2 }
            }
        };

        _mockPipelineService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdPipeline);
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
    public async Task Update_ValidPipeline_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdatePipelineDto
        {
            Id = 1,
            Name = "Updated Pipeline",
            Description = "Updated description"
        };

        var updatedPipeline = new PipelineDto
        {
            Id = 1,
            Name = "Updated Pipeline",
            Description = "Updated description"
        };

        _mockPipelineService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedPipeline);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPipeline = okResult.Value.Should().BeOfType<PipelineDto>().Subject;
        returnedPipeline.Name.Should().Be("Updated Pipeline");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdatePipelineDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingPipeline_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdatePipelineDto { Id = 999 };

        _mockPipelineService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((PipelineDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingPipeline_ReturnsNoContent()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingPipeline_ReturnsNotFound()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_PipelineWithOpportunities_ReturnsConflict()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Pipeline has active opportunities"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Stage Management Tests

    [Fact]
    public async Task GetStages_ReturnsStagesForPipeline()
    {
        // Arrange
        var stages = new List<StageDto>
        {
            new StageDto { Id = 1, Name = "Qualification", Order = 1 },
            new StageDto { Id = 2, Name = "Proposal", Order = 2 },
            new StageDto { Id = 3, Name = "Negotiation", Order = 3 }
        };

        _mockPipelineService.Setup(s => s.GetStagesAsync(1))
            .ReturnsAsync(stages);

        // Act
        var result = await _controller.GetStages(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStages = okResult.Value.Should().BeAssignableTo<IEnumerable<StageDto>>().Subject;
        returnedStages.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddStage_ValidStage_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateStageDto
        {
            Name = "New Stage",
            Order = 4,
            Probability = 50
        };

        var createdStage = new StageDto
        {
            Id = 4,
            Name = "New Stage",
            Order = 4,
            Probability = 50
        };

        _mockPipelineService.Setup(s => s.AddStageAsync(1, createDto))
            .ReturnsAsync(createdStage);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddStage(1, createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateStage_ValidStage_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateStageDto
        {
            Id = 1,
            Name = "Updated Stage",
            Probability = 75
        };

        var updatedStage = new StageDto
        {
            Id = 1,
            Name = "Updated Stage",
            Probability = 75
        };

        _mockPipelineService.Setup(s => s.UpdateStageAsync(1, updateDto))
            .ReturnsAsync(updatedStage);

        // Act
        var result = await _controller.UpdateStage(1, 1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StageDto>();
    }

    [Fact]
    public async Task DeleteStage_ExistingStage_ReturnsNoContent()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeleteStageAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteStage(1, 1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStage_StageWithOpportunities_ReturnsConflict()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeleteStageAsync(1, 1))
            .ThrowsAsync(new InvalidOperationException("Stage has active opportunities"));

        // Act
        var result = await _controller.DeleteStage(1, 1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task ReorderStages_ValidOrder_ReturnsOk()
    {
        // Arrange
        var reorderRequest = new ReorderStagesDto
        {
            StageIds = new List<int> { 3, 1, 2 } // New order
        };

        _mockPipelineService.Setup(s => s.ReorderStagesAsync(1, reorderRequest.StageIds))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReorderStages(1, reorderRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Pipeline Configuration Tests

    [Fact]
    public async Task SetDefault_ValidPipeline_ReturnsOk()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.SetDefaultAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetDefault(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetDefault_NonExistingPipeline_ReturnsNotFound()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.SetDefaultAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SetDefault(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Activate_ValidPipeline_ReturnsOk()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ValidPipeline_ReturnsOk()
    {
        // Arrange
        _mockPipelineService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetPipelineStats_ReturnsStats()
    {
        // Arrange
        var stats = new PipelineStatsDto
        {
            PipelineId = 1,
            TotalOpportunities = 50,
            TotalValue = 500000,
            ByStage = new Dictionary<string, int>
            {
                { "Qualification", 20 },
                { "Proposal", 15 },
                { "Negotiation", 10 },
                { "Closed Won", 5 }
            }
        };

        _mockPipelineService.Setup(s => s.GetStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<PipelineStatsDto>().Subject;
        returnedStats.TotalOpportunities.Should().Be(50);
    }

    [Fact]
    public async Task GetConversionRates_ReturnsRates()
    {
        // Arrange
        var rates = new List<StageConversionRateDto>
        {
            new StageConversionRateDto { StageName = "Qualification", ConversionRate = 75 },
            new StageConversionRateDto { StageName = "Proposal", ConversionRate = 60 }
        };

        _mockPipelineService.Setup(s => s.GetConversionRatesAsync(1))
            .ReturnsAsync(rates);

        // Act
        var result = await _controller.GetConversionRates(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedRates = okResult.Value.Should().BeAssignableTo<IEnumerable<StageConversionRateDto>>().Subject;
        returnedRates.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetVelocity_ReturnsPipelineVelocity()
    {
        // Arrange
        var velocity = new PipelineVelocityDto
        {
            AverageDaysInPipeline = 45,
            AverageDaysPerStage = new Dictionary<string, double>
            {
                { "Qualification", 10 },
                { "Proposal", 15 },
                { "Negotiation", 20 }
            }
        };

        _mockPipelineService.Setup(s => s.GetVelocityAsync(1))
            .ReturnsAsync(velocity);

        // Act
        var result = await _controller.GetVelocity(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PipelineVelocityDto>();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task Clone_ExistingPipeline_ReturnsCreatedAtAction()
    {
        // Arrange
        var cloneRequest = new ClonePipelineDto
        {
            SourcePipelineId = 1,
            NewName = "Cloned Pipeline"
        };

        var clonedPipeline = new PipelineDto
        {
            Id = 2,
            Name = "Cloned Pipeline"
        };

        _mockPipelineService.Setup(s => s.CloneAsync(1, "Cloned Pipeline"))
            .ReturnsAsync(clonedPipeline);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Clone_NonExistingSource_ReturnsNotFound()
    {
        // Arrange
        var cloneRequest = new ClonePipelineDto
        {
            SourcePipelineId = 999,
            NewName = "Cloned Pipeline"
        };

        _mockPipelineService.Setup(s => s.CloneAsync(999, "Cloned Pipeline"))
            .ReturnsAsync((PipelineDto?)null);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Team Assignment Tests

    [Fact]
    public async Task AssignToTeam_ValidAssignment_ReturnsOk()
    {
        // Arrange
        var assignRequest = new AssignPipelineToTeamDto
        {
            PipelineId = 1,
            TeamId = 5
        };

        _mockPipelineService.Setup(s => s.AssignToTeamAsync(1, 5))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignToTeam(assignRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetTeamPipelines_ReturnsTeamPipelines()
    {
        // Arrange
        var pipelines = new List<PipelineDto>
        {
            new PipelineDto { Id = 1, Name = "Team Pipeline" }
        };

        _mockPipelineService.Setup(s => s.GetByTeamAsync(5))
            .ReturnsAsync(pipelines);

        // Act
        var result = await _controller.GetByTeam(5);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<PipelineDto>>();
    }

    #endregion
}

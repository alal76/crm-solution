// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos.Workflow;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for WorkflowController (TCOV-039).
/// </summary>
public class WorkflowControllerTests : IDisposable
{
    private readonly Mock<IWorkflowService> _mockWorkflowService;
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<IWorkflowFieldSchemaService> _mockFieldSchemaService;
    private readonly CrmDbContext _dbContext;
    private readonly WorkflowController _controller;

    public WorkflowControllerTests()
    {
        _mockWorkflowService = new Mock<IWorkflowService>();
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockFieldSchemaService = new Mock<IWorkflowFieldSchemaService>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"WorkflowTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new WorkflowController(
            _dbContext,
            _mockWorkflowService.Object,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockFieldSchemaService.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetWorkflows_ShouldReturnOk_WhenCalled()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionsAsync(null, null, null, null, 0, 50))
            .ReturnsAsync(new List<WorkflowDefinition>());

        var result = await _controller.GetWorkflows();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWorkflows_ShouldReturnOk_WithFilterParams()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionsAsync("Account", null, "Sales", null, 0, 50))
            .ReturnsAsync(new List<WorkflowDefinition>());

        var result = await _controller.GetWorkflows(entityType: "Account", category: "Sales");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetWorkflow_ShouldReturnNotFound_WhenWorkflowDoesNotExist()
    {
        _mockWorkflowService
            .Setup(s => s.GetWorkflowDefinitionAsync(999))
            .ReturnsAsync((WorkflowDefinition?)null);

        var result = await _controller.GetWorkflow(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWorkflow_ShouldReturnOk_WhenWorkflowExists()
    {
        var workflow = new WorkflowDefinition
        {
            Id = 1,
            WorkflowKey = "test-key",
            Name = "Test Workflow",
            Versions = new List<WorkflowVersion>()
        };
        _mockWorkflowService.Setup(s => s.GetWorkflowDefinitionAsync(1)).ReturnsAsync(workflow);

        var result = await _controller.GetWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnBadRequest_WhenWorkflowKeyIsEmpty()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "", Name = "Test" };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "key-1", Name = "" };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnConflict_WhenKeyAlreadyExists()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "existing-key", Name = "Test Workflow" };
        _mockWorkflowService
            .Setup(s => s.GetWorkflowByKeyAsync("existing-key"))
            .ReturnsAsync(new WorkflowDefinition { WorkflowKey = "existing-key" });

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldReturnCreated_WhenValid()
    {
        var dto = new CreateWorkflowDto { WorkflowKey = "new-key", Name = "New Workflow" };
        _mockWorkflowService
            .Setup(s => s.GetWorkflowByKeyAsync("new-key"))
            .ReturnsAsync((WorkflowDefinition?)null);
        _mockWorkflowService
            .Setup(s => s.CreateWorkflowDefinitionAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync(new WorkflowDefinition { Id = 1 });

        // WorkflowController calls GetCurrentUserId() which reads User.FindFirst()
        // Set up a minimal claims principal so it doesn't throw NullReferenceException
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1") }))
            }
        };

        var result = await _controller.CreateWorkflow(dto);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateWorkflow_ShouldReturnNotFound_WhenWorkflowDoesNotExist()
    {
        var dto = new UpdateWorkflowDto { Name = "Updated" };

        var result = await _controller.UpdateWorkflow(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteWorkflow_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.DeleteWorkflowDefinitionAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteWorkflow_ShouldReturnBadRequest_WhenNotFound()
    {
        _mockWorkflowService.Setup(s => s.DeleteWorkflowDefinitionAsync(999)).ReturnsAsync(false);

        var result = await _controller.DeleteWorkflow(999);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PauseWorkflow_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.PauseWorkflowAsync(1)).ReturnsAsync(true);

        var result = await _controller.PauseWorkflow(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PauseWorkflow_ShouldReturnBadRequest_WhenNotFound()
    {
        _mockWorkflowService.Setup(s => s.PauseWorkflowAsync(99)).ReturnsAsync(false);

        var result = await _controller.PauseWorkflow(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private void SetAuthenticatedUser(string userId = "1")
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId) }))
            }
        };
    }

    #region ActivateWorkflow

    [Fact]
    public async Task ActivateWorkflow_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.ActivateWorkflowAsync(1, 2)).ReturnsAsync(true);

        var result = await _controller.ActivateWorkflow(1, 2);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ActivateWorkflow_ShouldReturnBadRequest_WhenServiceFails()
    {
        _mockWorkflowService.Setup(s => s.ActivateWorkflowAsync(1, 99)).ReturnsAsync(false);

        var result = await _controller.ActivateWorkflow(1, 99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region CloneWorkflow

    [Fact]
    public async Task CloneWorkflow_ShouldReturnCreated_WhenSuccess()
    {
        SetAuthenticatedUser();
        var cloned = new WorkflowDefinition
        {
            Id = 5,
            WorkflowKey = "clone-key",
            Name = "Original (Copy)",
            Status = WorkflowStatus.Draft
        };
        _mockWorkflowService
            .Setup(s => s.CloneWorkflowAsync(1, null, 1))
            .ReturnsAsync(cloned);

        var result = await _controller.CloneWorkflow(1);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CloneWorkflow_ShouldReturnNotFound_WhenSourceMissing()
    {
        SetAuthenticatedUser();
        _mockWorkflowService
            .Setup(s => s.CloneWorkflowAsync(999, null, 1))
            .ThrowsAsync(new KeyNotFoundException("Workflow not found"));

        var result = await _controller.CloneWorkflow(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetVersion

    [Fact]
    public async Task GetVersion_ShouldReturnNotFound_WhenVersionDoesNotExist()
    {
        _mockWorkflowService.Setup(s => s.GetWorkflowVersionAsync(999)).ReturnsAsync((WorkflowVersion?)null);

        var result = await _controller.GetVersion(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetVersion_ShouldReturnOk_WhenVersionExists()
    {
        var version = new WorkflowVersion
        {
            Id = 10,
            WorkflowDefinitionId = 1,
            WorkflowDefinition = new WorkflowDefinition { Id = 1, Name = "Test Workflow" },
            VersionNumber = 1,
            Status = WorkflowVersionStatus.Draft,
            Nodes = new List<WorkflowNode>(),
            Transitions = new List<WorkflowTransition>()
        };
        _mockWorkflowService.Setup(s => s.GetWorkflowVersionAsync(10)).ReturnsAsync(version);

        var result = await _controller.GetVersion(10);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region CreateVersion

    [Fact]
    public async Task CreateVersion_ShouldReturnOk_WhenSuccess()
    {
        var version = new WorkflowVersion { Id = 3, VersionNumber = 2 };
        _mockWorkflowService
            .Setup(s => s.CreateNewVersionAsync(1, null))
            .ReturnsAsync(version);

        var result = await _controller.CreateVersion(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateVersion_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _mockWorkflowService
            .Setup(s => s.CreateNewVersionAsync(999, null))
            .ThrowsAsync(new ArgumentException("Workflow not found"));

        var result = await _controller.CreateVersion(999);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region SaveCanvasLayout

    [Fact]
    public async Task SaveCanvasLayout_ShouldReturnOk_WhenSuccess()
    {
        var dto = new SaveLayoutDto { CanvasLayout = "{}" };
        _mockWorkflowService.Setup(s => s.SaveCanvasLayoutAsync(1, "{}")).ReturnsAsync(true);

        var result = await _controller.SaveCanvasLayout(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SaveCanvasLayout_ShouldReturnBadRequest_WhenServiceFails()
    {
        var dto = new SaveLayoutDto { CanvasLayout = "{}" };
        _mockWorkflowService.Setup(s => s.SaveCanvasLayoutAsync(99, "{}")).ReturnsAsync(false);

        var result = await _controller.SaveCanvasLayout(99, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetVersions

    [Fact]
    public async Task GetVersions_ShouldReturnOk_WhenCalled()
    {
        _mockWorkflowService
            .Setup(s => s.GetVersionsAsync(1))
            .ReturnsAsync(new List<WorkflowVersion>
            {
                new() { Id = 1, VersionNumber = 1, Status = WorkflowVersionStatus.Active }
            });

        var result = await _controller.GetVersions(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region UpdateVersionMetadata

    [Fact]
    public async Task UpdateVersionMetadata_ShouldReturnOk_WhenSuccess()
    {
        var dto = new UpdateVersionMetadataDto { Label = "v2", ChangeLog = "changes" };
        _mockWorkflowService
            .Setup(s => s.UpdateVersionMetadataAsync(1, "v2", "changes"))
            .ReturnsAsync(new WorkflowVersion { Id = 1, Label = "v2", ChangeLog = "changes" });

        var result = await _controller.UpdateVersionMetadata(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateVersionMetadata_ShouldReturnBadRequest_WhenNotDraft()
    {
        var dto = new UpdateVersionMetadataDto { Label = "v2" };
        _mockWorkflowService
            .Setup(s => s.UpdateVersionMetadataAsync(99, "v2", null))
            .ReturnsAsync((WorkflowVersion?)null);

        var result = await _controller.UpdateVersionMetadata(99, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region PublishVersion

    [Fact]
    public async Task PublishVersion_ShouldReturnOk_WhenSuccess()
    {
        SetAuthenticatedUser();
        _mockWorkflowService.Setup(s => s.PublishVersionAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.PublishVersion(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PublishVersion_ShouldReturnBadRequest_WhenNotDraft()
    {
        SetAuthenticatedUser();
        _mockWorkflowService.Setup(s => s.PublishVersionAsync(99, 1)).ReturnsAsync(false);

        var result = await _controller.PublishVersion(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteVersion

    [Fact]
    public async Task DeleteVersion_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.DeleteVersionAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteVersion(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteVersion_ShouldReturnBadRequest_WhenNotDraft()
    {
        _mockWorkflowService.Setup(s => s.DeleteVersionAsync(99)).ReturnsAsync(false);

        var result = await _controller.DeleteVersion(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region RollbackToVersion

    [Fact]
    public async Task RollbackToVersion_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService
            .Setup(s => s.RollbackToVersionAsync(1, 2))
            .ReturnsAsync(new WorkflowVersion { Id = 3, VersionNumber = 3, Label = "Rollback" });

        var result = await _controller.RollbackToVersion(1, 2);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RollbackToVersion_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _mockWorkflowService
            .Setup(s => s.RollbackToVersionAsync(1, 999))
            .ThrowsAsync(new ArgumentException("Version not found"));

        var result = await _controller.RollbackToVersion(1, 999);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region CompareVersions

    [Fact]
    public async Task CompareVersions_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService
            .Setup(s => s.CompareVersionsAsync(1, 2))
            .ReturnsAsync(new VersionComparisonResult { Version1Id = 1, Version2Id = 2, TotalChanges = 3 });

        var result = await _controller.CompareVersions(1, 2);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CompareVersions_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _mockWorkflowService
            .Setup(s => s.CompareVersionsAsync(1, 999))
            .ThrowsAsync(new ArgumentException("Version not found"));

        var result = await _controller.CompareVersions(1, 999);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region AddNode

    [Fact]
    public async Task AddNode_ShouldReturnOk_WhenValid()
    {
        var dto = new CreateNodeDto { Name = "Start", NodeType = "Trigger", PositionX = 0, PositionY = 0 };
        _mockWorkflowService
            .Setup(s => s.AddNodeAsync(It.IsAny<WorkflowNode>()))
            .ReturnsAsync(new WorkflowNode { Id = 1, NodeKey = "abc123", Name = "Start" });

        var result = await _controller.AddNode(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddNode_ShouldReturnBadRequest_WhenNodeTypeInvalid()
    {
        var dto = new CreateNodeDto { Name = "Start", NodeType = "NotARealType" };

        var result = await _controller.AddNode(1, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddNode_ShouldReturnBadRequest_WhenServiceThrows()
    {
        var dto = new CreateNodeDto { Name = "Start", NodeType = "Trigger" };
        _mockWorkflowService
            .Setup(s => s.AddNodeAsync(It.IsAny<WorkflowNode>()))
            .ThrowsAsync(new InvalidOperationException("Version not editable"));

        var result = await _controller.AddNode(1, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateNode

    [Fact]
    public async Task UpdateNode_ShouldReturnNotFound_WhenNodeDoesNotExist()
    {
        var dto = new UpdateNodeDto { Name = "Renamed" };

        var result = await _controller.UpdateNode(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateNode_ShouldReturnOk_WhenSuccess()
    {
        _dbContext.WorkflowNodes.Add(new WorkflowNode { Id = 1, NodeKey = "n1", Name = "Old Name", WorkflowVersionId = 1 });
        await _dbContext.SaveChangesAsync();

        var dto = new UpdateNodeDto { Name = "New Name" };
        _mockWorkflowService
            .Setup(s => s.UpdateNodeAsync(1, It.IsAny<WorkflowNode>()))
            .ReturnsAsync(new WorkflowNode { Id = 1, NodeKey = "n1", Name = "New Name" });

        var result = await _controller.UpdateNode(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateNode_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _dbContext.WorkflowNodes.Add(new WorkflowNode { Id = 2, NodeKey = "n2", Name = "Old Name", WorkflowVersionId = 1 });
        await _dbContext.SaveChangesAsync();

        var dto = new UpdateNodeDto { Name = "New Name" };
        _mockWorkflowService
            .Setup(s => s.UpdateNodeAsync(2, It.IsAny<WorkflowNode>()))
            .ThrowsAsync(new InvalidOperationException("Cannot update published version"));

        var result = await _controller.UpdateNode(2, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteNode

    [Fact]
    public async Task DeleteNode_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.DeleteNodeAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteNode(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteNode_ShouldReturnBadRequest_WhenServiceReturnsFalse()
    {
        _mockWorkflowService.Setup(s => s.DeleteNodeAsync(99)).ReturnsAsync(false);

        var result = await _controller.DeleteNode(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteNode_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _mockWorkflowService
            .Setup(s => s.DeleteNodeAsync(2))
            .ThrowsAsync(new InvalidOperationException("Node has dependent transitions"));

        var result = await _controller.DeleteNode(2);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateNodePositions

    [Fact]
    public async Task UpdateNodePositions_ShouldReturnOk_WhenCalled()
    {
        var positions = new List<NodePositionDto>
        {
            new() { NodeId = 1, X = 10, Y = 20 },
            new() { NodeId = 2, X = 30, Y = 40 }
        };
        _mockWorkflowService
            .Setup(s => s.UpdateNodePositionsAsync(It.IsAny<Dictionary<int, (double x, double y)>>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateNodePositions(1, positions);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region AddTransition

    [Fact]
    public async Task AddTransition_ShouldReturnOk_WhenValid()
    {
        var dto = new CreateTransitionDto { SourceNodeId = 1, TargetNodeId = 2 };
        _mockWorkflowService
            .Setup(s => s.AddTransitionAsync(It.IsAny<WorkflowTransition>()))
            .ReturnsAsync(new WorkflowTransition { Id = 1, SourceNodeId = 1, TargetNodeId = 2 });

        var result = await _controller.AddTransition(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddTransition_ShouldReturnBadRequest_WhenServiceThrows()
    {
        var dto = new CreateTransitionDto { SourceNodeId = 1, TargetNodeId = 999 };
        _mockWorkflowService
            .Setup(s => s.AddTransitionAsync(It.IsAny<WorkflowTransition>()))
            .ThrowsAsync(new InvalidOperationException("Target node not found"));

        var result = await _controller.AddTransition(1, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region UpdateTransition

    [Fact]
    public async Task UpdateTransition_ShouldReturnNotFound_WhenTransitionDoesNotExist()
    {
        var dto = new UpdateTransitionDto { Label = "Updated" };

        var result = await _controller.UpdateTransition(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateTransition_ShouldReturnOk_WhenSuccess()
    {
        _dbContext.WorkflowTransitions.Add(new WorkflowTransition { Id = 1, SourceNodeId = 1, TargetNodeId = 2, WorkflowVersionId = 1 });
        await _dbContext.SaveChangesAsync();

        var dto = new UpdateTransitionDto { Label = "Updated" };
        _mockWorkflowService
            .Setup(s => s.UpdateTransitionAsync(1, It.IsAny<WorkflowTransition>()))
            .ReturnsAsync(new WorkflowTransition { Id = 1, Label = "Updated" });

        var result = await _controller.UpdateTransition(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateTransition_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _dbContext.WorkflowTransitions.Add(new WorkflowTransition { Id = 2, SourceNodeId = 1, TargetNodeId = 2, WorkflowVersionId = 1 });
        await _dbContext.SaveChangesAsync();

        var dto = new UpdateTransitionDto { Label = "Updated" };
        _mockWorkflowService
            .Setup(s => s.UpdateTransitionAsync(2, It.IsAny<WorkflowTransition>()))
            .ThrowsAsync(new InvalidOperationException("Cannot update this transition"));

        var result = await _controller.UpdateTransition(2, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteTransition

    [Fact]
    public async Task DeleteTransition_ShouldReturnOk_WhenSuccess()
    {
        _mockWorkflowService.Setup(s => s.DeleteTransitionAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteTransition(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteTransition_ShouldReturnBadRequest_WhenServiceReturnsFalse()
    {
        _mockWorkflowService.Setup(s => s.DeleteTransitionAsync(99)).ReturnsAsync(false);

        var result = await _controller.DeleteTransition(99);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteTransition_ShouldReturnBadRequest_WhenServiceThrows()
    {
        _mockWorkflowService
            .Setup(s => s.DeleteTransitionAsync(2))
            .ThrowsAsync(new InvalidOperationException("Transition in use"));

        var result = await _controller.DeleteTransition(2);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Statistics

    [Fact]
    public async Task GetStatistics_ShouldReturnOk_WhenCalled()
    {
        _mockWorkflowService
            .Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(new WorkflowStatistics { TotalWorkflows = 5, ActiveWorkflows = 2 });

        var result = await _controller.GetStatistics();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetExecutionStats_ShouldReturnOk_WhenCalled()
    {
        _mockWorkflowService
            .Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(new WorkflowStatistics { TotalWorkflows = 5, RunningInstances = 1 });

        var result = await _controller.GetExecutionStats();

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetWorkflowConfig

    [Fact]
    public async Task GetWorkflowConfig_ShouldReturnOk_WhenCalled()
    {
        _mockFieldSchemaService
            .Setup(s => s.GetAllFieldSchemas())
            .Returns(new Dictionary<string, List<EntityFieldConfig>>());
        _mockFieldSchemaService
            .Setup(s => s.GetAllRelatedEntitySchemas())
            .Returns(new Dictionary<string, List<RelatedEntityConfig>>());
        _mockLLMService
            .Setup(s => s.GetAvailableProviders())
            .Returns(new List<LLMProviderInfo>());
        _mockLLMService
            .Setup(s => s.GetAvailableModels())
            .Returns(new List<LLMModelInfo>());

        var result = await _controller.GetWorkflowConfig();

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region LLM Settings

    [Fact]
    public async Task GetLLMSettings_ShouldReturnOk_WhenCalled()
    {
        _mockLLMSettingsService
            .Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto());

        var result = await _controller.GetLLMSettings();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateLLMSettings_ShouldReturnOk_WhenCalled()
    {
        var request = new UpdateLLMSettingsRequest { DefaultProvider = "openai" };
        _mockLLMSettingsService
            .Setup(s => s.UpdateSettingsAsync(request))
            .ReturnsAsync(new LLMSettingsDto { DefaultProvider = "openai" });

        var result = await _controller.UpdateLLMSettings(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetLLMSettings_ShouldReturnOk_WhenCalled()
    {
        _mockLLMSettingsService.Setup(s => s.ResetToDefaultsAsync()).Returns(Task.CompletedTask);
        _mockLLMSettingsService
            .Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto());

        var result = await _controller.ResetLLMSettings();

        result.Should().BeOfType<OkObjectResult>();
        _mockLLMSettingsService.Verify(s => s.ResetToDefaultsAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeLLMSettings_ShouldReturnOk_WhenCalled()
    {
        _mockLLMSettingsService.Setup(s => s.InitializeDefaultSettingsAsync()).Returns(Task.CompletedTask);
        _mockLLMSettingsService
            .Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto());

        var result = await _controller.InitializeLLMSettings();

        result.Should().BeOfType<OkObjectResult>();
        _mockLLMSettingsService.Verify(s => s.InitializeDefaultSettingsAsync(), Times.Once);
    }

    [Fact]
    public async Task TestLLMProviderConnection_ShouldReturnOk_WhenSuccess()
    {
        _mockLLMSettingsService
            .Setup(s => s.TestProviderConnectionAsync("openai"))
            .ReturnsAsync((true, "Connected successfully"));

        var result = await _controller.TestLLMProviderConnection("openai");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TestLLMProviderConnection_ShouldReturnOk_WhenFailure()
    {
        _mockLLMSettingsService
            .Setup(s => s.TestProviderConnectionAsync("badprovider"))
            .ReturnsAsync((false, "Provider not configured"));

        var result = await _controller.TestLLMProviderConnection("badprovider");

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Lookups

    [Fact]
    public void GetEntityTypes_ShouldReturnOk_WhenCalled()
    {
        var result = _controller.GetEntityTypes();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetNodeTypes_ShouldReturnOk_WhenCalled()
    {
        var result = _controller.GetNodeTypes();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WhenCalled()
    {
        var result = await _controller.GetCategories();

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}

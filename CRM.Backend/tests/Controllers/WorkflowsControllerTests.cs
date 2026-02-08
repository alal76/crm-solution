// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Workflow Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
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
/// Comprehensive unit tests for WorkflowsController
/// Covers: Workflow definitions, instances, nodes, transitions, tasks, automation
/// </summary>
public class WorkflowsControllerTests
{
    private readonly Mock<IWorkflowService> _mockWorkflowService;
    private readonly Mock<ILogger<WorkflowsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly WorkflowsController _controller;

    public WorkflowsControllerTests()
    {
        _mockWorkflowService = new Mock<IWorkflowService>();
        _mockLogger = new Mock<ILogger<WorkflowsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new WorkflowsController(_mockWorkflowService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region Workflow Definitions Tests

    [Fact]
    public async Task GetDefinitions_ReturnsOkResult_WithDefinitions()
    {
        // Arrange
        var definitions = new List<WorkflowDefinitionDto>
        {
            new WorkflowDefinitionDto { Id = 1, Name = "Lead Qualification", IsActive = true },
            new WorkflowDefinitionDto { Id = 2, Name = "Quote Approval", IsActive = true }
        };

        _mockWorkflowService.Setup(s => s.GetDefinitionsAsync())
            .ReturnsAsync(definitions);

        // Act
        var result = await _controller.GetDefinitions();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDefinitions = okResult.Value as IEnumerable<WorkflowDefinitionDto>;
        returnedDefinitions.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveDefinitions_ReturnsOnlyActive()
    {
        // Arrange
        var definitions = new List<WorkflowDefinitionDto>
        {
            new WorkflowDefinitionDto { Id = 1, IsActive = true }
        };

        _mockWorkflowService.Setup(s => s.GetActiveDefinitionsAsync())
            .ReturnsAsync(definitions);

        // Act
        var result = await _controller.GetActiveDefinitions();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetDefinitionById_ExistingDefinition_ReturnsOk()
    {
        // Arrange
        var definition = new WorkflowDefinitionDto { Id = 1, Name = "Test Workflow" };

        _mockWorkflowService.Setup(s => s.GetDefinitionByIdAsync(1))
            .ReturnsAsync(definition);

        // Act
        var result = await _controller.GetDefinitionById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetDefinitionById_NonExisting_ReturnsNotFound()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.GetDefinitionByIdAsync(999))
            .ReturnsAsync((WorkflowDefinitionDto?)null);

        // Act
        var result = await _controller.GetDefinitionById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateDefinition_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateWorkflowDefinitionDto
        {
            Name = "New Workflow",
            Description = "Test workflow",
            EntityType = "Lead"
        };

        var createdDefinition = new WorkflowDefinitionDto
        {
            Id = 1,
            Name = "New Workflow",
            IsActive = false
        };

        _mockWorkflowService.Setup(s => s.CreateDefinitionAsync(createDto))
            .ReturnsAsync(createdDefinition);

        // Act
        var result = await _controller.CreateDefinition(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    }

    [Fact]
    public async Task CreateDefinition_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateDefinition(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateDefinition_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateWorkflowDefinitionDto { Name = "Existing Workflow" };

        _mockWorkflowService.Setup(s => s.CreateDefinitionAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Workflow name already exists"));

        // Act
        var result = await _controller.CreateDefinition(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UpdateDefinition_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateWorkflowDefinitionDto
        {
            Id = 1,
            Name = "Updated Workflow"
        };

        var updatedDefinition = new WorkflowDefinitionDto { Id = 1, Name = "Updated Workflow" };

        _mockWorkflowService.Setup(s => s.UpdateDefinitionAsync(updateDto))
            .ReturnsAsync(updatedDefinition);

        // Act
        var result = await _controller.UpdateDefinition(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task ActivateDefinition_ValidDefinition_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.ActivateDefinitionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateDefinition(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeactivateDefinition_ValidDefinition_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeactivateDefinitionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateDefinition(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeleteDefinition_ExistingDefinition_ReturnsNoContent()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeleteDefinitionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteDefinition(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteDefinition_HasActiveInstances_ReturnsConflict()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeleteDefinitionAsync(1))
            .ThrowsAsync(new InvalidOperationException("Workflow has active instances"));

        // Act
        var result = await _controller.DeleteDefinition(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Workflow Nodes Tests

    [Fact]
    public async Task GetNodes_ValidDefinition_ReturnsNodes()
    {
        // Arrange
        var nodes = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto { Id = 1, Name = "Start", NodeType = "Start" },
            new WorkflowNodeDto { Id = 2, Name = "Approval", NodeType = "Approval" }
        };

        _mockWorkflowService.Setup(s => s.GetNodesAsync(1))
            .ReturnsAsync(nodes);

        // Act
        var result = await _controller.GetNodes(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateNode_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateWorkflowNodeDto
        {
            WorkflowDefinitionId = 1,
            Name = "New Node",
            NodeType = "Task"
        };

        var createdNode = new WorkflowNodeDto { Id = 1, Name = "New Node" };

        _mockWorkflowService.Setup(s => s.CreateNodeAsync(createDto))
            .ReturnsAsync(createdNode);

        // Act
        var result = await _controller.CreateNode(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateNode_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateWorkflowNodeDto
        {
            Id = 1,
            Name = "Updated Node"
        };

        var updatedNode = new WorkflowNodeDto { Id = 1, Name = "Updated Node" };

        _mockWorkflowService.Setup(s => s.UpdateNodeAsync(updateDto))
            .ReturnsAsync(updatedNode);

        // Act
        var result = await _controller.UpdateNode(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task DeleteNode_ExistingNode_ReturnsNoContent()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeleteNodeAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteNode(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Workflow Transitions Tests

    [Fact]
    public async Task GetTransitions_ValidDefinition_ReturnsTransitions()
    {
        // Arrange
        var transitions = new List<WorkflowTransitionDto>
        {
            new WorkflowTransitionDto { Id = 1, FromNodeId = 1, ToNodeId = 2 }
        };

        _mockWorkflowService.Setup(s => s.GetTransitionsAsync(1))
            .ReturnsAsync(transitions);

        // Act
        var result = await _controller.GetTransitions(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateTransition_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateWorkflowTransitionDto
        {
            WorkflowDefinitionId = 1,
            FromNodeId = 1,
            ToNodeId = 2,
            Condition = "approved == true"
        };

        var createdTransition = new WorkflowTransitionDto { Id = 1 };

        _mockWorkflowService.Setup(s => s.CreateTransitionAsync(createDto))
            .ReturnsAsync(createdTransition);

        // Act
        var result = await _controller.CreateTransition(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateTransition_CircularReference_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateWorkflowTransitionDto
        {
            FromNodeId = 1,
            ToNodeId = 1
        };

        _mockWorkflowService.Setup(s => s.CreateTransitionAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Circular reference detected"));

        // Act
        var result = await _controller.CreateTransition(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task DeleteTransition_ExistingTransition_ReturnsNoContent()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeleteTransitionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTransition(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Workflow Instances Tests

    [Fact]
    public async Task GetInstances_ReturnsOkResult_WithInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstanceDto>
        {
            new WorkflowInstanceDto { Id = 1, Status = "Running" },
            new WorkflowInstanceDto { Id = 2, Status = "Completed" }
        };

        _mockWorkflowService.Setup(s => s.GetInstancesAsync())
            .ReturnsAsync(instances);

        // Act
        var result = await _controller.GetInstances();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetInstancesByDefinition_ReturnsFilteredInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstanceDto>
        {
            new WorkflowInstanceDto { Id = 1, WorkflowDefinitionId = 1 }
        };

        _mockWorkflowService.Setup(s => s.GetInstancesByDefinitionAsync(1))
            .ReturnsAsync(instances);

        // Act
        var result = await _controller.GetInstancesByDefinition(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetActiveInstances_ReturnsActiveOnly()
    {
        // Arrange
        var instances = new List<WorkflowInstanceDto>
        {
            new WorkflowInstanceDto { Id = 1, Status = "Running" }
        };

        _mockWorkflowService.Setup(s => s.GetActiveInstancesAsync())
            .ReturnsAsync(instances);

        // Act
        var result = await _controller.GetActiveInstances();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetInstanceById_ExistingInstance_ReturnsOk()
    {
        // Arrange
        var instance = new WorkflowInstanceDto { Id = 1 };

        _mockWorkflowService.Setup(s => s.GetInstanceByIdAsync(1))
            .ReturnsAsync(instance);

        // Act
        var result = await _controller.GetInstanceById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task StartInstance_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var startDto = new StartWorkflowDto
        {
            WorkflowDefinitionId = 1,
            EntityType = "Lead",
            EntityId = 1
        };

        var instance = new WorkflowInstanceDto { Id = 1, Status = "Running" };

        _mockWorkflowService.Setup(s => s.StartInstanceAsync(startDto))
            .ReturnsAsync(instance);

        // Act
        var result = await _controller.StartInstance(startDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task SuspendInstance_ValidInstance_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.SuspendInstanceAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SuspendInstance(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResumeInstance_ValidInstance_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.ResumeInstanceAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResumeInstance(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelInstance_ValidInstance_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.CancelInstanceAsync(1, "No longer needed"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelInstance(1, "No longer needed");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RetryInstance_FailedInstance_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.RetryInstanceAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RetryInstance(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Workflow Tasks Tests

    [Fact]
    public async Task GetTasks_ReturnsOkResult_WithTasks()
    {
        // Arrange
        var tasks = new List<WorkflowTaskDto>
        {
            new WorkflowTaskDto { Id = 1, Name = "Review document" },
            new WorkflowTaskDto { Id = 2, Name = "Approve request" }
        };

        _mockWorkflowService.Setup(s => s.GetTasksAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetMyTasks_ReturnsCurrentUserTasks()
    {
        // Arrange
        var tasks = new List<WorkflowTaskDto>
        {
            new WorkflowTaskDto { Id = 1, AssignedToId = 1 }
        };

        _mockWorkflowService.Setup(s => s.GetTasksByAssigneeAsync(1))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetMyTasks();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetPendingTasks_ReturnsPendingOnly()
    {
        // Arrange
        var tasks = new List<WorkflowTaskDto>
        {
            new WorkflowTaskDto { Id = 1, Status = "Pending" }
        };

        _mockWorkflowService.Setup(s => s.GetPendingTasksAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetPendingTasks();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsOk()
    {
        // Arrange
        var task = new WorkflowTaskDto { Id = 1, Name = "Review document" };

        _mockWorkflowService.Setup(s => s.GetTaskByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CompleteTask_ValidData_ReturnsOk()
    {
        // Arrange
        var completeDto = new CompleteWorkflowTaskDto
        {
            TaskId = 1,
            Outcome = "Approved",
            Comments = "Looks good"
        };

        _mockWorkflowService.Setup(s => s.CompleteTaskAsync(completeDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CompleteTask(1, completeDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CompleteTask_AlreadyCompleted_ReturnsConflict()
    {
        // Arrange
        var completeDto = new CompleteWorkflowTaskDto { TaskId = 1 };

        _mockWorkflowService.Setup(s => s.CompleteTaskAsync(completeDto))
            .ThrowsAsync(new InvalidOperationException("Task is already completed"));

        // Act
        var result = await _controller.CompleteTask(1, completeDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task AssignTask_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.AssignTaskAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignTask(1, 2);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ReassignTask_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.ReassignTaskAsync(1, 3, "Out of office"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReassignTask(1, 3, "Out of office");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DelegateTask_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DelegateTaskAsync(1, 4))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DelegateTask(1, 4);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Workflow Execution History Tests

    [Fact]
    public async Task GetExecutionHistory_ValidInstance_ReturnsHistory()
    {
        // Arrange
        var history = new List<WorkflowExecutionLogDto>
        {
            new WorkflowExecutionLogDto { Id = 1, Action = "Started" },
            new WorkflowExecutionLogDto { Id = 2, Action = "Task Completed" }
        };

        _mockWorkflowService.Setup(s => s.GetExecutionHistoryAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetExecutionHistory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetCurrentNode_ValidInstance_ReturnsNode()
    {
        // Arrange
        var node = new WorkflowNodeDto { Id = 2, Name = "Approval" };

        _mockWorkflowService.Setup(s => s.GetCurrentNodeAsync(1))
            .ReturnsAsync(node);

        // Act
        var result = await _controller.GetCurrentNode(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Workflow Version Tests

    [Fact]
    public async Task GetVersions_ValidDefinition_ReturnsVersions()
    {
        // Arrange
        var versions = new List<WorkflowVersionDto>
        {
            new WorkflowVersionDto { Version = 1, CreatedAt = DateTime.Today.AddDays(-7) },
            new WorkflowVersionDto { Version = 2, CreatedAt = DateTime.Today }
        };

        _mockWorkflowService.Setup(s => s.GetVersionsAsync(1))
            .ReturnsAsync(versions);

        // Act
        var result = await _controller.GetVersions(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task PublishVersion_ValidDefinition_ReturnsOk()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.PublishVersionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PublishVersion(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Workflow Triggers Tests

    [Fact]
    public async Task GetTriggers_ValidDefinition_ReturnsTriggers()
    {
        // Arrange
        var triggers = new List<WorkflowTriggerDto>
        {
            new WorkflowTriggerDto { Id = 1, TriggerType = "OnCreate" }
        };

        _mockWorkflowService.Setup(s => s.GetTriggersAsync(1))
            .ReturnsAsync(triggers);

        // Act
        var result = await _controller.GetTriggers(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateTrigger_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateWorkflowTriggerDto
        {
            WorkflowDefinitionId = 1,
            TriggerType = "OnUpdate",
            Conditions = "status == 'Approved'"
        };

        var createdTrigger = new WorkflowTriggerDto { Id = 1 };

        _mockWorkflowService.Setup(s => s.CreateTriggerAsync(createDto))
            .ReturnsAsync(createdTrigger);

        // Act
        var result = await _controller.CreateTrigger(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task DeleteTrigger_ExistingTrigger_ReturnsNoContent()
    {
        // Arrange
        _mockWorkflowService.Setup(s => s.DeleteTriggerAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTrigger(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatistics_ReturnsWorkflowStats()
    {
        // Arrange
        var stats = new WorkflowStatisticsDto
        {
            TotalDefinitions = 10,
            ActiveInstances = 50,
            CompletedInstances = 500,
            PendingTasks = 25
        };

        _mockWorkflowService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion
}

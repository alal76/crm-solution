// CRM Solution - Customer Relationship Management System
// Workflow Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WorkflowService
/// Covers: Workflow definitions, instances, tasks, execution
/// </summary>
public class WorkflowServiceTests
{
    private readonly Mock<IRepository<WorkflowDefinition>> _mockWorkflowRepository;
    private readonly Mock<IRepository<WorkflowInstance>> _mockInstanceRepository;
    private readonly Mock<IRepository<WorkflowTask>> _mockTaskRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<WorkflowService>> _mockLogger;
    private readonly WorkflowService _service;

    public WorkflowServiceTests()
    {
        _mockWorkflowRepository = new Mock<IRepository<WorkflowDefinition>>();
        _mockInstanceRepository = new Mock<IRepository<WorkflowInstance>>();
        _mockTaskRepository = new Mock<IRepository<WorkflowTask>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<WorkflowService>>();

        _service = new WorkflowService(
            _mockWorkflowRepository.Object,
            _mockInstanceRepository.Object,
            _mockTaskRepository.Object,
            _mockDbContext.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    #region Workflow Definition Tests

    [Fact]
    public async Task GetAllDefinitionsAsync_ReturnsAllWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowDefinition>
        {
            new WorkflowDefinition { Id = 1, Name = "Lead Approval", IsActive = true },
            new WorkflowDefinition { Id = 2, Name = "Quote Approval", IsActive = true }
        };

        _mockWorkflowRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(workflows);

        // Act
        var result = await _service.GetAllDefinitionsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDefinitionByIdAsync_ExistingWorkflow_ReturnsWorkflow()
    {
        // Arrange
        var workflow = new WorkflowDefinition
        {
            Id = 1,
            Name = "Lead Approval",
            Description = "Approval workflow for leads",
            IsActive = true
        };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(workflow);

        // Act
        var result = await _service.GetDefinitionByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Lead Approval");
    }

    [Fact]
    public async Task GetDefinitionByIdAsync_NonExistingWorkflow_ReturnsNull()
    {
        // Arrange
        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((WorkflowDefinition?)null);

        // Act
        var result = await _service.GetDefinitionByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateDefinitionAsync_ValidWorkflow_ReturnsCreatedWorkflow()
    {
        // Arrange
        var createDto = new CreateWorkflowDefinitionDto
        {
            Name = "New Workflow",
            Description = "Test workflow",
            EntityType = "Lead",
            TriggerType = "OnCreate"
        };

        _mockWorkflowRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync((WorkflowDefinition w) => { w.Id = 1; return w; });

        // Act
        var result = await _service.CreateDefinitionAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Workflow");
    }

    [Fact]
    public async Task CreateDefinitionAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var createDto = new CreateWorkflowDefinitionDto { Name = "Existing Workflow" };

        _mockWorkflowRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>()))
            .ReturnsAsync(new List<WorkflowDefinition> { new WorkflowDefinition { Name = "Existing Workflow" } });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateDefinitionAsync(createDto));
    }

    [Fact]
    public async Task UpdateDefinitionAsync_ValidWorkflow_ReturnsUpdatedWorkflow()
    {
        // Arrange
        var existingWorkflow = new WorkflowDefinition { Id = 1, Name = "Old Name" };
        var updateDto = new UpdateWorkflowDefinitionDto { Id = 1, Name = "New Name" };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingWorkflow);

        _mockWorkflowRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync((WorkflowDefinition w) => w);

        // Act
        var result = await _service.UpdateDefinitionAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteDefinitionAsync_ExistingWorkflow_ReturnsTrue()
    {
        // Arrange
        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new WorkflowDefinition { Id = 1 });

        _mockWorkflowRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteDefinitionAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateDefinitionAsync_ValidWorkflow_ReturnsTrue()
    {
        // Arrange
        var workflow = new WorkflowDefinition { Id = 1, IsActive = false };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(workflow);

        _mockWorkflowRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync((WorkflowDefinition w) => w);

        // Act
        var result = await _service.ActivateDefinitionAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateDefinitionAsync_ValidWorkflow_ReturnsTrue()
    {
        // Arrange
        var workflow = new WorkflowDefinition { Id = 1, IsActive = true };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(workflow);

        _mockWorkflowRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowDefinition>()))
            .ReturnsAsync((WorkflowDefinition w) => w);

        // Act
        var result = await _service.DeactivateDefinitionAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Workflow Instance Tests

    [Fact]
    public async Task StartWorkflowAsync_ValidWorkflow_ReturnsInstance()
    {
        // Arrange
        var workflow = new WorkflowDefinition { Id = 1, Name = "Test", IsActive = true };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(workflow);

        _mockInstanceRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Id = 1; return i; });

        // Act
        var result = await _service.StartWorkflowAsync(1, "Lead", 100);

        // Assert
        result.Should().NotBeNull();
        result.WorkflowDefinitionId.Should().Be(1);
    }

    [Fact]
    public async Task StartWorkflowAsync_InactiveWorkflow_ThrowsException()
    {
        // Arrange
        var workflow = new WorkflowDefinition { Id = 1, IsActive = false };

        _mockWorkflowRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(workflow);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.StartWorkflowAsync(1, "Lead", 100));
    }

    [Fact]
    public async Task GetInstanceByIdAsync_ExistingInstance_ReturnsInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            WorkflowDefinitionId = 1,
            Status = "Running"
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        // Act
        var result = await _service.GetInstanceByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Running");
    }

    [Fact]
    public async Task GetInstancesByEntityAsync_ReturnsInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, EntityType = "Lead", EntityId = 100 },
            new WorkflowInstance { Id = 2, EntityType = "Lead", EntityId = 100 }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetInstancesByEntityAsync("Lead", 100);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CancelInstanceAsync_RunningInstance_ReturnsCancelled()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = "Running" };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => i);

        // Act
        var result = await _service.CancelInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelInstanceAsync_CompletedInstance_ReturnsFalse()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = "Completed" };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        // Act
        var result = await _service.CancelInstanceAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteInstanceAsync_RunningInstance_ReturnsCompleted()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = "Running" };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = "Completed"; return i; });

        // Act
        var result = await _service.CompleteInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Workflow Task Tests

    [Fact]
    public async Task GetTaskByIdAsync_ExistingTask_ReturnsTask()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            Title = "Approve Lead",
            Status = "Pending"
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _service.GetTaskByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Approve Lead");
    }

    [Fact]
    public async Task GetTasksByAssigneeAsync_ReturnsTasks()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, AssignedToId = 1, Title = "Task 1" },
            new WorkflowTask { Id = 2, AssignedToId = 1, Title = "Task 2" }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetTasksByAssigneeAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPendingTasksAsync_ReturnsPendingTasks()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, Status = "Pending" }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetPendingTasksAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task CompleteTaskAsync_ValidTask_ReturnsTrue()
    {
        // Arrange
        var task = new WorkflowTask { Id = 1, Status = "Pending" };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.CompleteTaskAsync(1, "Approved");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_AlreadyCompleted_ReturnsFalse()
    {
        // Arrange
        var task = new WorkflowTask { Id = 1, Status = "Completed" };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _service.CompleteTaskAsync(1, "Approved");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReassignTaskAsync_ValidTask_ReturnsTrue()
    {
        // Arrange
        var task = new WorkflowTask { Id = 1, AssignedToId = 1, Status = "Pending" };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.ReassignTaskAsync(1, 2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddCommentToTaskAsync_ValidTask_ReturnsTrue()
    {
        // Arrange
        var task = new WorkflowTask { Id = 1 };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.AddCommentToTaskAsync(1, "This is a comment", 1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetWorkflowStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var definitions = new List<WorkflowDefinition>
        {
            new WorkflowDefinition { Id = 1, IsActive = true },
            new WorkflowDefinition { Id = 2, IsActive = false }
        };

        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = "Running" },
            new WorkflowInstance { Id = 2, Status = "Completed" },
            new WorkflowInstance { Id = 3, Status = "Running" }
        };

        _mockWorkflowRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(definitions);

        _mockInstanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalDefinitions.Should().Be(2);
        result.ActiveDefinitions.Should().Be(1);
        result.RunningInstances.Should().Be(2);
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_ReturnsTaskStats()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, Status = "Pending" },
            new WorkflowTask { Id = 2, Status = "Completed" },
            new WorkflowTask { Id = 3, Status = "Pending" }
        };

        _mockTaskRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetTaskStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalTasks.Should().Be(3);
        result.PendingTasks.Should().Be(2);
    }

    #endregion

    #region Trigger Tests

    [Fact]
    public async Task GetTriggersForEntityAsync_ReturnsMatchingWorkflows()
    {
        // Arrange
        var workflows = new List<WorkflowDefinition>
        {
            new WorkflowDefinition { Id = 1, EntityType = "Lead", TriggerType = "OnCreate", IsActive = true }
        };

        _mockWorkflowRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>()))
            .ReturnsAsync(workflows);

        // Act
        var result = await _service.GetTriggersForEntityAsync("Lead", "OnCreate");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task TriggerWorkflowsAsync_MatchingWorkflows_StartsInstances()
    {
        // Arrange
        var workflows = new List<WorkflowDefinition>
        {
            new WorkflowDefinition { Id = 1, EntityType = "Lead", TriggerType = "OnCreate", IsActive = true }
        };

        _mockWorkflowRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>()))
            .ReturnsAsync(workflows);

        _mockInstanceRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Id = 1; return i; });

        // Act
        var result = await _service.TriggerWorkflowsAsync("Lead", "OnCreate", 100);

        // Assert
        result.Should().Be(1);
    }

    #endregion
}

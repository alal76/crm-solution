// CRM Solution - Customer Relationship Management System
// Workflow Instance Service Unit Tests

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
/// Unit tests for WorkflowInstanceService
/// Covers: Instance creation, execution, status tracking, completion
/// </summary>
public class WorkflowInstanceServiceTests
{
    private readonly Mock<IRepository<WorkflowInstance>> _mockInstanceRepository;
    private readonly Mock<IRepository<WorkflowDefinition>> _mockDefinitionRepository;
    private readonly Mock<IRepository<WorkflowNodeInstance>> _mockNodeInstanceRepository;
    private readonly Mock<IRepository<WorkflowTask>> _mockTaskRepository;
    private readonly Mock<IRepository<WorkflowLog>> _mockLogRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<WorkflowInstanceService>> _mockLogger;
    private readonly WorkflowInstanceService _service;

    public WorkflowInstanceServiceTests()
    {
        _mockInstanceRepository = new Mock<IRepository<WorkflowInstance>>();
        _mockDefinitionRepository = new Mock<IRepository<WorkflowDefinition>>();
        _mockNodeInstanceRepository = new Mock<IRepository<WorkflowNodeInstance>>();
        _mockTaskRepository = new Mock<IRepository<WorkflowTask>>();
        _mockLogRepository = new Mock<IRepository<WorkflowLog>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<WorkflowInstanceService>>();

        _service = new WorkflowInstanceService(
            _mockInstanceRepository.Object,
            _mockDefinitionRepository.Object,
            _mockNodeInstanceRepository.Object,
            _mockTaskRepository.Object,
            _mockLogRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Create Instance Tests

    [Fact]
    public async Task CreateInstanceAsync_ValidDefinition_ReturnsInstance()
    {
        // Arrange
        var definition = new WorkflowDefinition
        {
            Id = 1,
            Name = "Approval Workflow",
            IsActive = true
        };

        var request = new CreateWorkflowInstanceRequest
        {
            DefinitionId = 1,
            EntityType = "Quote",
            EntityId = 100,
            InitiatedBy = 1
        };

        _mockDefinitionRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(definition);

        _mockInstanceRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Id = 1; return i; });

        // Act
        var result = await _service.CreateInstanceAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Status.Should().Be(WorkflowInstanceStatus.Pending);
    }

    [Fact]
    public async Task CreateInstanceAsync_InactiveDefinition_ThrowsException()
    {
        // Arrange
        var definition = new WorkflowDefinition
        {
            Id = 1,
            Name = "Inactive Workflow",
            IsActive = false
        };

        var request = new CreateWorkflowInstanceRequest
        {
            DefinitionId = 1,
            EntityType = "Quote",
            EntityId = 100
        };

        _mockDefinitionRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(definition);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateInstanceAsync(request));
    }

    [Fact]
    public async Task CreateInstanceAsync_NonExistingDefinition_ThrowsException()
    {
        // Arrange
        var request = new CreateWorkflowInstanceRequest
        {
            DefinitionId = 999
        };

        _mockDefinitionRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((WorkflowDefinition?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateInstanceAsync(request));
    }

    [Fact]
    public async Task CreateInstanceAsync_WithInitialData_SetsData()
    {
        // Arrange
        var definition = new WorkflowDefinition { Id = 1, IsActive = true };
        var request = new CreateWorkflowInstanceRequest
        {
            DefinitionId = 1,
            EntityType = "Quote",
            EntityId = 100,
            InitialData = new Dictionary<string, object>
            {
                { "Amount", 50000 },
                { "Priority", "High" }
            }
        };

        _mockDefinitionRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(definition);

        _mockInstanceRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Id = 1; return i; });

        // Act
        var result = await _service.CreateInstanceAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Get Instance Tests

    [Fact]
    public async Task GetInstanceByIdAsync_ExistingInstance_ReturnsInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            DefinitionId = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        // Act
        var result = await _service.GetInstanceByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(WorkflowInstanceStatus.Running);
    }

    [Fact]
    public async Task GetInstanceByIdAsync_NonExistingInstance_ReturnsNull()
    {
        // Arrange
        _mockInstanceRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((WorkflowInstance?)null);

        // Act
        var result = await _service.GetInstanceByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInstancesByEntityAsync_ReturnsEntityInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, EntityType = "Quote", EntityId = 100 },
            new WorkflowInstance { Id = 2, EntityType = "Quote", EntityId = 100 }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetInstancesByEntityAsync("Quote", 100);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveInstancesAsync_ReturnsRunningInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Running },
            new WorkflowInstance { Id = 2, Status = WorkflowInstanceStatus.Running }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetActiveInstancesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Start/Stop Instance Tests

    [Fact]
    public async Task StartInstanceAsync_PendingInstance_StartsExecution()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Pending,
            DefinitionId = 1
        };

        var definition = new WorkflowDefinition
        {
            Id = 1,
            StartNodeId = 10
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockDefinitionRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(definition);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Running; return i; });

        // Act
        var result = await _service.StartInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task StartInstanceAsync_AlreadyRunning_ReturnsFalse()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        // Act
        var result = await _service.StartInstanceAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PauseInstanceAsync_RunningInstance_PausesInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Paused; return i; });

        // Act
        var result = await _service.PauseInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResumeInstanceAsync_PausedInstance_ResumesInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Paused
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Running; return i; });

        // Act
        var result = await _service.ResumeInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CancelInstanceAsync_RunningInstance_CancelsInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Cancelled; return i; });

        // Act
        var result = await _service.CancelInstanceAsync(1, "Cancelled by user");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Complete Instance Tests

    [Fact]
    public async Task CompleteInstanceAsync_ValidInstance_CompletesInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Completed; return i; });

        // Act
        var result = await _service.CompleteInstanceAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FailInstanceAsync_ValidInstance_FailsInstance()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Id = 1,
            Status = WorkflowInstanceStatus.Running
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => { i.Status = WorkflowInstanceStatus.Failed; return i; });

        // Act
        var result = await _service.FailInstanceAsync(1, "An error occurred");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Node Instance Tests

    [Fact]
    public async Task GetCurrentNodeAsync_RunningInstance_ReturnsCurrentNode()
    {
        // Arrange
        var nodeInstance = new WorkflowNodeInstance
        {
            Id = 1,
            InstanceId = 1,
            Status = NodeInstanceStatus.Active
        };

        _mockNodeInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowNodeInstance, bool>>>()))
            .ReturnsAsync(new List<WorkflowNodeInstance> { nodeInstance });

        // Act
        var result = await _service.GetCurrentNodeAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(NodeInstanceStatus.Active);
    }

    [Fact]
    public async Task AdvanceToNextNodeAsync_ValidTransition_AdvancesNode()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Running };
        var currentNode = new WorkflowNodeInstance
        {
            Id = 1,
            InstanceId = 1,
            NodeId = 10,
            Status = NodeInstanceStatus.Active
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockNodeInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowNodeInstance, bool>>>()))
            .ReturnsAsync(new List<WorkflowNodeInstance> { currentNode });

        _mockNodeInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowNodeInstance>()))
            .ReturnsAsync((WorkflowNodeInstance n) => n);

        _mockNodeInstanceRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowNodeInstance>()))
            .ReturnsAsync((WorkflowNodeInstance n) => { n.Id = 2; return n; });

        // Act
        var result = await _service.AdvanceToNextNodeAsync(1, 20);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetNodeHistoryAsync_ReturnsAllNodeInstances()
    {
        // Arrange
        var nodeInstances = new List<WorkflowNodeInstance>
        {
            new WorkflowNodeInstance { Id = 1, InstanceId = 1, Status = NodeInstanceStatus.Completed },
            new WorkflowNodeInstance { Id = 2, InstanceId = 1, Status = NodeInstanceStatus.Active }
        };

        _mockNodeInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowNodeInstance, bool>>>()))
            .ReturnsAsync(nodeInstances);

        // Act
        var result = await _service.GetNodeHistoryAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Task Tests

    [Fact]
    public async Task GetPendingTasksAsync_ReturnsPendingTasks()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, Status = TaskStatus.Pending, AssignedToId = 1 },
            new WorkflowTask { Id = 2, Status = TaskStatus.Pending, AssignedToId = 1 }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetPendingTasksAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompleteTaskAsync_ValidTask_CompletesTask()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            InstanceId = 1,
            Status = TaskStatus.Pending
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => { t.Status = TaskStatus.Completed; return t; });

        // Act
        var result = await _service.CompleteTaskAsync(1, new TaskCompletionData { Outcome = "Approved" });

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetInstanceLogsAsync_ReturnsLogs()
    {
        // Arrange
        var logs = new List<WorkflowLog>
        {
            new WorkflowLog { Id = 1, InstanceId = 1, Message = "Started" },
            new WorkflowLog { Id = 2, InstanceId = 1, Message = "Completed" }
        };

        _mockLogRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowLog, bool>>>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetInstanceLogsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddLogAsync_ValidLog_AddsLog()
    {
        // Arrange
        _mockLogRepository.Setup(r => r.AddAsync(It.IsAny<WorkflowLog>()))
            .ReturnsAsync((WorkflowLog l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.AddLogAsync(1, "Node completed", LogLevel.Info);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetInstanceStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Completed },
            new WorkflowInstance { Id = 2, Status = WorkflowInstanceStatus.Running },
            new WorkflowInstance { Id = 3, Status = WorkflowInstanceStatus.Failed }
        };

        _mockInstanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetInstanceStatisticsAsync();

        // Assert
        result.TotalInstances.Should().Be(3);
        result.CompletedCount.Should().Be(1);
        result.FailedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetInstancesByStatusAsync_ReturnsFilteredInstances()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Completed }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        // Act
        var result = await _service.GetInstancesByStatusAsync(WorkflowInstanceStatus.Completed);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion
}

// Supporting classes for tests
public enum WorkflowInstanceStatus
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public enum NodeInstanceStatus
{
    Pending,
    Active,
    Completed,
    Skipped,
    Failed
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public class CreateWorkflowInstanceRequest
{
    public int DefinitionId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? InitiatedBy { get; set; }
    public Dictionary<string, object>? InitialData { get; set; }
}

public class TaskCompletionData
{
    public string Outcome { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

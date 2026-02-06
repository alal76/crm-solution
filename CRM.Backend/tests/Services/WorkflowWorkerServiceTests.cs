// CRM Solution - Customer Relationship Management System
// Workflow Worker Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WorkflowWorkerService
/// Covers: Background workflow processing, task execution, scheduling
/// </summary>
public class WorkflowWorkerServiceTests
{
    private readonly Mock<IRepository<WorkflowInstance>> _mockInstanceRepository;
    private readonly Mock<IRepository<WorkflowDefinition>> _mockDefinitionRepository;
    private readonly Mock<IRepository<WorkflowTask>> _mockTaskRepository;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<WorkflowWorkerService>> _mockLogger;
    private readonly WorkflowWorkerService _service;

    public WorkflowWorkerServiceTests()
    {
        _mockInstanceRepository = new Mock<IRepository<WorkflowInstance>>();
        _mockDefinitionRepository = new Mock<IRepository<WorkflowDefinition>>();
        _mockTaskRepository = new Mock<IRepository<WorkflowTask>>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<WorkflowWorkerService>>();

        // Setup scope factory
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        _service = new WorkflowWorkerService(
            _mockInstanceRepository.Object,
            _mockDefinitionRepository.Object,
            _mockTaskRepository.Object,
            _mockScopeFactory.Object,
            _mockLogger.Object);
    }

    #region Process Pending Tests

    [Fact]
    public async Task ProcessPendingInstancesAsync_PendingInstances_ProcessesAll()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Pending },
            new WorkflowInstance { Id = 2, Status = WorkflowInstanceStatus.Pending }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => i);

        // Act
        var result = await _service.ProcessPendingInstancesAsync();

        // Assert
        result.ProcessedCount.Should().Be(2);
    }

    [Fact]
    public async Task ProcessPendingInstancesAsync_NoInstances_ReturnsZero()
    {
        // Arrange
        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(new List<WorkflowInstance>());

        // Act
        var result = await _service.ProcessPendingInstancesAsync();

        // Assert
        result.ProcessedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessPendingInstancesAsync_WithBatchSize_LimitsBatch()
    {
        // Arrange
        var instances = Enumerable.Range(1, 100).Select(i => new WorkflowInstance
        {
            Id = i,
            Status = WorkflowInstanceStatus.Pending
        }).ToList();

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances.Take(10).ToList());

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => i);

        // Act
        var result = await _service.ProcessPendingInstancesAsync(batchSize: 10);

        // Assert
        result.ProcessedCount.Should().BeLessThanOrEqualTo(10);
    }

    #endregion

    #region Execute Task Tests

    [Fact]
    public async Task ExecuteTaskAsync_ValidTask_ExecutesTask()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            WorkflowInstanceId = 1,
            TaskType = "SendEmail",
            Status = WorkflowTaskStatus.Pending
        };

        var instance = new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Running };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.ExecuteTaskAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteTaskAsync_TaskNotFound_ReturnsFalse()
    {
        // Arrange
        _mockTaskRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((WorkflowTask?)null);

        // Act
        var result = await _service.ExecuteTaskAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteTaskAsync_TaskFails_MarksAsFailed()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            WorkflowInstanceId = 1,
            TaskType = "InvalidType",
            Status = WorkflowTaskStatus.Pending
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        await _service.ExecuteTaskAsync(1);

        // Assert
        _mockTaskRepository.Verify(r => r.UpdateAsync(It.Is<WorkflowTask>(
            t => t.Status == WorkflowTaskStatus.Failed)), Times.AtLeastOnce);
    }

    #endregion

    #region Scheduled Tasks Tests

    [Fact]
    public async Task ProcessScheduledTasksAsync_DueTasks_ProcessesAll()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, ScheduledAt = DateTime.UtcNow.AddMinutes(-5), Status = WorkflowTaskStatus.Scheduled },
            new WorkflowTask { Id = 2, ScheduledAt = DateTime.UtcNow.AddMinutes(-1), Status = WorkflowTaskStatus.Scheduled }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.ProcessScheduledTasksAsync();

        // Assert
        result.ProcessedCount.Should().Be(2);
    }

    [Fact]
    public async Task ScheduleTaskAsync_ValidTask_SchedulesTask()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            Status = WorkflowTaskStatus.Pending
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(task);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.ScheduleTaskAsync(1, DateTime.UtcNow.AddHours(1));

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Retry Tests

    [Fact]
    public async Task RetryFailedTasksAsync_FailedTasks_RetriesAll()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, Status = WorkflowTaskStatus.Failed, RetryCount = 1, MaxRetries = 3 },
            new WorkflowTask { Id = 2, Status = WorkflowTaskStatus.Failed, RetryCount = 2, MaxRetries = 3 }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.RetryFailedTasksAsync();

        // Assert
        result.RetriedCount.Should().Be(2);
    }

    [Fact]
    public async Task RetryFailedTasksAsync_MaxRetriesExceeded_SkipsTask()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Id = 1, Status = WorkflowTaskStatus.Failed, RetryCount = 3, MaxRetries = 3 }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(new List<WorkflowTask>()); // No eligible tasks

        // Act
        var result = await _service.RetryFailedTasksAsync();

        // Assert
        result.RetriedCount.Should().Be(0);
    }

    #endregion

    #region Timeout Tests

    [Fact]
    public async Task ProcessTimeoutTasksAsync_TimedOutTasks_MarksAsFailed()
    {
        // Arrange
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask
            {
                Id = 1,
                Status = WorkflowTaskStatus.Running,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                TimeoutMinutes = 15
            }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        _mockTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowTask>()))
            .ReturnsAsync((WorkflowTask t) => t);

        // Act
        var result = await _service.ProcessTimeoutTasksAsync();

        // Assert
        result.TimedOutCount.Should().Be(1);
    }

    #endregion

    #region Instance Completion Tests

    [Fact]
    public async Task CheckInstanceCompletionAsync_AllTasksComplete_CompletesInstance()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Running };
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { WorkflowInstanceId = 1, Status = WorkflowTaskStatus.Completed },
            new WorkflowTask { WorkflowInstanceId = 1, Status = WorkflowTaskStatus.Completed }
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        _mockInstanceRepository.Setup(r => r.UpdateAsync(It.IsAny<WorkflowInstance>()))
            .ReturnsAsync((WorkflowInstance i) => i);

        // Act
        var result = await _service.CheckInstanceCompletionAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckInstanceCompletionAsync_TasksPending_DoesNotComplete()
    {
        // Arrange
        var instance = new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Running };
        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { WorkflowInstanceId = 1, Status = WorkflowTaskStatus.Completed },
            new WorkflowTask { WorkflowInstanceId = 1, Status = WorkflowTaskStatus.Pending }
        };

        _mockInstanceRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(instance);

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.CheckInstanceCompletionAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public async Task CleanupCompletedInstancesAsync_OldInstances_CleansUp()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Id = 1, Status = WorkflowInstanceStatus.Completed, CompletedAt = DateTime.UtcNow.AddDays(-100) },
            new WorkflowInstance { Id = 2, Status = WorkflowInstanceStatus.Completed, CompletedAt = DateTime.UtcNow.AddDays(-95) }
        };

        _mockInstanceRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowInstance, bool>>>()))
            .ReturnsAsync(instances);

        _mockInstanceRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CleanupCompletedInstancesAsync(90); // older than 90 days

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var instances = new List<WorkflowInstance>
        {
            new WorkflowInstance { Status = WorkflowInstanceStatus.Running },
            new WorkflowInstance { Status = WorkflowInstanceStatus.Completed },
            new WorkflowInstance { Status = WorkflowInstanceStatus.Pending }
        };

        var tasks = new List<WorkflowTask>
        {
            new WorkflowTask { Status = WorkflowTaskStatus.Running },
            new WorkflowTask { Status = WorkflowTaskStatus.Completed },
            new WorkflowTask { Status = WorkflowTaskStatus.Failed }
        };

        _mockInstanceRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(instances);

        _mockTaskRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.RunningInstances.Should().Be(1);
        result.PendingInstances.Should().Be(1);
        result.CompletedInstances.Should().Be(1);
    }

    [Fact]
    public async Task GetQueueStatusAsync_ReturnsQueueInfo()
    {
        // Arrange
        var pendingTasks = new List<WorkflowTask>
        {
            new WorkflowTask { Status = WorkflowTaskStatus.Pending },
            new WorkflowTask { Status = WorkflowTaskStatus.Pending },
            new WorkflowTask { Status = WorkflowTaskStatus.Pending }
        };

        _mockTaskRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowTask, bool>>>()))
            .ReturnsAsync(pendingTasks);

        // Act
        var result = await _service.GetQueueStatusAsync();

        // Assert
        result.PendingTaskCount.Should().Be(3);
    }

    #endregion
}

// Supporting classes for tests
public enum WorkflowInstanceStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum WorkflowTaskStatus
{
    Pending,
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled
}

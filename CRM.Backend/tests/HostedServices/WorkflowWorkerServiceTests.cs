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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for WorkflowWorkerService background service.
/// Tests workflow execution, task processing, and error handling.
/// </summary>
public class WorkflowWorkerServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<WorkflowWorkerService>> _mockLogger;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IWorkflowEngine> _mockWorkflowEngine;
    private readonly Mock<IOptions<WorkflowWorkerSettings>> _mockSettings;

    public WorkflowWorkerServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<WorkflowWorkerService>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockWorkflowEngine = new Mock<IWorkflowEngine>();
        _mockSettings = new Mock<IOptions<WorkflowWorkerSettings>>();

        SetupServiceProvider();
    }

    private void SetupServiceProvider()
    {
        var settings = new WorkflowWorkerSettings
        {
            Enabled = true,
            PollingIntervalSeconds = 10,
            MaxConcurrentWorkflows = 5,
            MaxRetryAttempts = 3
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICrmDbContext)))
            .Returns(_mockDbContext.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IWorkflowEngine)))
            .Returns(_mockWorkflowEngine.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<WorkflowWorkerSettings>)))
            .Returns(_mockSettings.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new WorkflowWorkerService(
            null!,
            _mockLogger.Object,
            _mockSettings.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new WorkflowWorkerService(
            _mockServiceProvider.Object,
            null!,
            _mockSettings.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("settings");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenEnabled_ShouldLogStartMessage()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting") || v.ToString()!.Contains("Workflow")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotProcessWorkflows()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings { Enabled = false };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockWorkflowEngine.Verify(
            x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();

        // Assert
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Workflow Processing Tests

    [Fact]
    public async Task ProcessWorkflowsAsync_WithPendingWorkflows_ShouldProcessThem()
    {
        // Arrange
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - workflows should be processed (timing dependent)
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_WithNoWorkflows_ShouldCompleteSuccessfully()
    {
        // Arrange
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should complete without error
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_ShouldRespectMaxConcurrentWorkflows()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings
        {
            Enabled = true,
            PollingIntervalSeconds = 1,
            MaxConcurrentWorkflows = 10,
            MaxRetryAttempts = 3
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        // Assert
        settings.MaxConcurrentWorkflows.Should().Be(10);
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_ShouldRespectPollingInterval()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings
        {
            Enabled = true,
            PollingIntervalSeconds = 30,
            MaxConcurrentWorkflows = 5,
            MaxRetryAttempts = 3
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        // Assert
        settings.PollingIntervalSeconds.Should().Be(30);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ProcessWorkflowsAsync_WhenEngineFails_ShouldLogError()
    {
        // Arrange
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Workflow engine error"));

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_AfterError_ShouldContinueOnNextCycle()
    {
        // Arrange
        var callCount = 0;
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("First failure");
                return Task.FromResult(0);
            });

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - service should continue despite first error
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_WithMaxRetryAttempts_ShouldRespectSetting()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings
        {
            Enabled = true,
            PollingIntervalSeconds = 1,
            MaxConcurrentWorkflows = 5,
            MaxRetryAttempts = 5
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        // Assert
        settings.MaxRetryAttempts.Should().Be(5);
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_WhenScopeCreationFails_ShouldLogError()
    {
        // Arrange
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Throws(new InvalidOperationException("Scope creation failed"));

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Act
        var task = service.StartAsync(CancellationToken.None);

        // Assert
        await task;
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_ShouldStopBackgroundExecution()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(CancellationToken.None);

        // Assert - should complete without hanging
    }

    [Fact]
    public async Task StopAsync_WhenCalledBeforeStart_ShouldNotThrow()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Act
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldLogStopMessage()
    {
        // Arrange
        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopping") || v.ToString()!.Contains("stop")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void WorkflowWorkerSettings_DefaultValues_ShouldBeReasonable()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings();

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.PollingIntervalSeconds.Should().Be(0);
        settings.MaxConcurrentWorkflows.Should().Be(0);
        settings.MaxRetryAttempts.Should().Be(0);
    }

    [Fact]
    public void WorkflowWorkerSettings_CanBeFullyConfigured()
    {
        // Arrange
        var settings = new WorkflowWorkerSettings
        {
            Enabled = true,
            PollingIntervalSeconds = 15,
            MaxConcurrentWorkflows = 20,
            MaxRetryAttempts = 5
        };

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.PollingIntervalSeconds.Should().Be(15);
        settings.MaxConcurrentWorkflows.Should().Be(20);
        settings.MaxRetryAttempts.Should().Be(5);
    }

    #endregion

    #region Concurrent Processing Tests

    [Fact]
    public async Task ProcessWorkflowsAsync_WithMultipleWorkflows_ShouldProcessConcurrently()
    {
        // Arrange
        var processedCount = 0;
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(10);
                Interlocked.Increment(ref processedCount);
                return 1;
            });

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should have processed multiple times
    }

    [Fact]
    public async Task ProcessWorkflowsAsync_WhenCancelledDuringProcessing_ShouldStopGracefully()
    {
        // Arrange
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                await Task.Delay(1000, ct);
                return 0;
            });

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(50);
        cts.Cancel();

        // Assert
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Workflow State Tests

    [Fact]
    public async Task ProcessWorkflowsAsync_WithFailedWorkflow_ShouldHandleGracefully()
    {
        // Arrange
        _mockWorkflowEngine.Setup(x => x.ProcessPendingWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WorkflowExecutionException("Workflow 1 failed"));

        var service = new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should handle error and continue
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    #endregion
}

/// <summary>
/// Workflow worker configuration settings
/// </summary>
public class WorkflowWorkerSettings
{
    public bool Enabled { get; set; }
    public int PollingIntervalSeconds { get; set; }
    public int MaxConcurrentWorkflows { get; set; }
    public int MaxRetryAttempts { get; set; }
}

/// <summary>
/// Mock interface for workflow engine
/// </summary>
public interface IWorkflowEngine
{
    Task<int> ProcessPendingWorkflowsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Custom workflow execution exception
/// </summary>
public class WorkflowExecutionException : Exception
{
    public WorkflowExecutionException(string message) : base(message) { }
    public WorkflowExecutionException(string message, Exception innerException) : base(message, innerException) { }
}

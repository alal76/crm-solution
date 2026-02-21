// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Tests for WorkflowWorkerService.
/// Real constructor: (IServiceProvider serviceProvider, ILogger&lt;WorkflowWorkerService&gt; logger, IHttpClientFactory httpClientFactory, WorkflowWorkerOptions? options = null)
/// Extends BackgroundService. Uses SemaphoreSlim for concurrency, action handlers dictionary.
/// </summary>
public class WorkflowWorkerServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<WorkflowWorkerService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public WorkflowWorkerServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<WorkflowWorkerService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // HttpClientFactory should return a usable client
        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());
    }

    private WorkflowWorkerService CreateService(WorkflowWorkerOptions? options = null)
    {
        return new WorkflowWorkerService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockHttpClientFactory.Object,
            options);
    }

    [Fact]
    public void Constructor_WithDefaultOptions_ShouldCreateService()
    {
        // Act
        var service = CreateService();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomOptions_ShouldCreateService()
    {
        // Arrange
        var options = new WorkflowWorkerOptions
        {
            WorkerId = "test-worker-001",
            MaxConcurrentTasks = 10,
            PollIntervalSeconds = 2,
            LockDurationMinutes = 30,
            MaxRetryCount = 5,
            BaseRetryDelaySeconds = 60,
            EnableLLMActions = false,
            QueueNames = new[] { "priority", "default" }
        };

        // Act
        var service = CreateService(options);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaults()
    {
        // Act - null options should use defaults
        var service = CreateService(null);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void WorkflowWorkerOptions_Defaults_ShouldBeCorrect()
    {
        // Arrange & Act
        var options = new WorkflowWorkerOptions();

        // Assert
        options.WorkerId.Should().NotBeNullOrEmpty();
        options.MaxConcurrentTasks.Should().Be(5);
        options.PollIntervalSeconds.Should().Be(5);
        options.LockDurationMinutes.Should().Be(15);
        options.MaxRetryCount.Should().Be(3);
        options.BaseRetryDelaySeconds.Should().Be(30);
        options.EnableLLMActions.Should().BeTrue();
        options.QueueNames.Should().Contain("default");
        options.QueueNames.Should().Contain("priority");
        options.QueueNames.Should().Contain("background");
    }

    [Fact]
    public void WorkflowWorkerOptions_WorkerId_ShouldIncludeMachineName()
    {
        // Arrange & Act
        var options = new WorkflowWorkerOptions();

        // Assert
        options.WorkerId.Should().StartWith(Environment.MachineName);
    }

    [Fact]
    public void WorkflowWorkerOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new WorkflowWorkerOptions
        {
            WorkerId = "custom-worker",
            MaxConcurrentTasks = 20,
            PollIntervalSeconds = 1,
            LockDurationMinutes = 60,
            MaxRetryCount = 10,
            BaseRetryDelaySeconds = 5,
            EnableLLMActions = false,
            QueueNames = new[] { "urgent" }
        };

        // Assert
        options.WorkerId.Should().Be("custom-worker");
        options.MaxConcurrentTasks.Should().Be(20);
        options.PollIntervalSeconds.Should().Be(1);
        options.LockDurationMinutes.Should().Be(60);
        options.MaxRetryCount.Should().Be(10);
        options.BaseRetryDelaySeconds.Should().Be(5);
        options.EnableLLMActions.Should().BeFalse();
        options.QueueNames.Should().HaveCount(1);
        options.QueueNames.Should().Contain("urgent");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = CreateService();
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteWithoutError()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);
        var act = () => service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenScopeFactoryNotResolved_ShouldHandleGracefully()
    {
        // Arrange - service provider returns null for scope factory
        _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
            .Returns(null!);

        var service = CreateService();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithMinimalPollInterval_ShouldStillWork()
    {
        // Arrange
        var options = new WorkflowWorkerOptions
        {
            PollIntervalSeconds = 1,
            MaxConcurrentTasks = 1
        };
        var service = CreateService(options);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        cts.Cancel();
        await Task.Delay(200, CancellationToken.None);

        // Assert
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}

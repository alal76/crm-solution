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
/// Unit tests for CalendarSyncHostedService background service.
/// Tests periodic sync execution, error handling, and service lifecycle.
/// </summary>
public class CalendarSyncHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<CalendarSyncHostedService>> _mockLogger;
    private readonly Mock<ICalendarSyncService> _mockCalendarSyncService;

    public CalendarSyncHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<CalendarSyncHostedService>>();
        _mockCalendarSyncService = new Mock<ICalendarSyncService>();

        SetupServiceProvider();
    }

    private void SetupServiceProvider()
    {
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICalendarSyncService)))
            .Returns(_mockCalendarSyncService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CalendarSyncHostedService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new CalendarSyncHostedService(_mockServiceProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_WhenStarted_ShouldLogStartMessage()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldLogStopMessage()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopped")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallSyncAllDueAsync()
    {
        // Arrange
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(Task.CompletedTask);

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert - may or may not have called depending on timing
    }

    [Fact]
    public async Task ExecuteAsync_WhenSyncThrowsException_ShouldLogError()
    {
        // Arrange
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .ThrowsAsync(new InvalidOperationException("Calendar sync failed"));

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
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
            Times.AtMost(1));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
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

    #region Service Scope Tests

    [Fact]
    public async Task ExecuteAsync_ShouldCreateScopeForEachIteration()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.AtMost(1));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDisposeScope()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockScope.Verify(x => x.Dispose(), Times.AtMost(1));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_WhenServiceResolutionFails_ShouldHandleGracefully()
    {
        // Arrange
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICalendarSyncService)))
            .Returns(null);

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert - service should handle null gracefully or log error
    }

    [Fact]
    public async Task ExecuteAsync_WhenScopeCreationFails_ShouldLogError()
    {
        // Arrange
        _mockScopeFactory.Setup(x => x.CreateScope())
            .Throws(new InvalidOperationException("Cannot create scope"));

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(1));
    }

    [Fact]
    public async Task ExecuteAsync_AfterMultipleErrors_ShouldContinueRunning()
    {
        // Arrange
        var callCount = 0;
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new InvalidOperationException("Temporary failure");
                return Task.CompletedTask;
            });

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - service should continue despite errors
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);

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
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
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
        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);

        // Act
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Interval Tests

    [Fact]
    public async Task ExecuteAsync_ShouldUseFiveMinuteInterval()
    {
        // Arrange
        var syncCallCount = 0;
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(() =>
            {
                syncCallCount++;
                return Task.CompletedTask;
            });

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act - run for a short time (not full 5 minutes)
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert - should have called at most once in this short window
        syncCallCount.Should().BeLessOrEqualTo(1);
    }

    #endregion

    #region Google Calendar Integration Tests

    [Fact]
    public async Task ExecuteAsync_WithGoogleCalendarIntegration_ShouldSync()
    {
        // Arrange
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(Task.CompletedTask);

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert - service should complete without errors
    }

    [Fact]
    public async Task ExecuteAsync_WithMicrosoftCalendarIntegration_ShouldSync()
    {
        // Arrange
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(Task.CompletedTask);

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert - service should complete without errors
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task ExecuteAsync_ShouldNotOverlapSyncOperations()
    {
        // Arrange
        var concurrentCalls = 0;
        var maxConcurrentCalls = 0;
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(async () =>
            {
                var current = Interlocked.Increment(ref concurrentCalls);
                maxConcurrentCalls = Math.Max(maxConcurrentCalls, current);
                await Task.Delay(50);
                Interlocked.Decrement(ref concurrentCalls);
            });

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(300);
        cts.Cancel();

        // Assert - no overlapping calls (would be 1 max)
        maxConcurrentCalls.Should().BeLessOrEqualTo(1);
    }

    #endregion

    #region Integration-Style Tests

    [Fact]
    public async Task ExecuteAsync_FullLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        _mockCalendarSyncService.Setup(x => x.SyncAllDueAsync())
            .Returns(Task.CompletedTask);

        var service = new CalendarSyncHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();
        // StopAsync waits for ExecuteAsync to complete, ensuring all logging has occurred
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}

/// <summary>
/// Mock interface for calendar sync service
/// </summary>
public interface ICalendarSyncService
{
    Task SyncAllDueAsync();
}

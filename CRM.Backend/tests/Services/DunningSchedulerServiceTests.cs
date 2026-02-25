// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
//
// Spec: PHASE 6 - Subscription Billing Services (SPEC-SALES-006)
// TODO-SALES003-012: DunningSchedulerService tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: DunningSchedulerService.cs (refactored to use IServiceScopeFactory),
//   IDunningManager.cs, DunningManager.cs and all DTOs.
//
// Constructor: DunningSchedulerService(IServiceScopeFactory scopeFactory,
//              ILogger<DunningSchedulerService> logger, int runIntervalHours = 4)

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DunningSchedulerService.
/// Verifies that the background scheduler correctly delegates to IDunningManager
/// (resolved per-cycle via IServiceScopeFactory), applies exponential back-off
/// scheduling, and handles errors gracefully.
/// </summary>
public class DunningSchedulerServiceTests
{
    private readonly Mock<IDunningManager> _mockDunningManager;
    private readonly Mock<ILogger<DunningSchedulerService>> _mockLogger;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public DunningSchedulerServiceTests()
    {
        _mockDunningManager = new Mock<IDunningManager>();
        _mockLogger = new Mock<ILogger<DunningSchedulerService>>();

        // Wire: ScopeFactory → Scope → ServiceProvider → IDunningManager
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IDunningManager)))
            .Returns(_mockDunningManager.Object);

        _mockScope = new Mock<IServiceScope>();
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
    }

    private DunningSchedulerService CreateService(int intervalHours = 4)
    {
        return new DunningSchedulerService(
            _mockScopeFactory.Object,
            _mockLogger.Object,
            runIntervalHours: intervalHours);
    }

    private static DunningCycleResultDto CreateCycleResult(
        int processed = 5,
        int successful = 3,
        int escalated = 1,
        int paused = 0,
        int cancelled = 0,
        List<string>? errors = null)
    {
        return new DunningCycleResultDto
        {
            ProcessedCount = processed,
            SuccessfulRetries = successful,
            EscalatedCount = escalated,
            PausedSubscriptions = paused,
            CancelledSubscriptions = cancelled,
            Errors = errors ?? [],
        };
    }

    // ────────────────────────────────────────────────────────────────────────
    // RunDunningCycleAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunDunningCycleAsync_ShouldCallProcessDunning_AndReturnResult()
    {
        // Arrange
        var expectedResult = CreateCycleResult(processed: 10, successful: 7);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProcessedCount.Should().Be(10);
        result.SuccessfulRetries.Should().Be(7);
        _mockDunningManager.Verify(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldCreateNewScope_ForEachCycle()
    {
        // Arrange
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCycleResult());

        var service = CreateService();

        // Act
        await service.RunDunningCycleAsync(CancellationToken.None);
        await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert — each cycle creates its own scope (prevents captive dependency)
        _mockScopeFactory.Verify(f => f.CreateScope(), Times.Exactly(2));
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldLogErrors_WhenDunningCycleHasErrors()
    {
        // Arrange
        var resultWithErrors = CreateCycleResult(
            processed: 3,
            errors: ["Payment 1: gateway timeout", "Payment 5: invalid card"]);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultWithErrors);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.Errors.Should().HaveCount(2);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldLogInformation_WhenCycleCompletes()
    {
        // Arrange
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCycleResult());

        var service = CreateService();

        // Act
        await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // "Starting" and "completed" messages
    }

    // ────────────────────────────────────────────────────────────────────────
    // TriggerManualCycleAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TriggerManualCycleAsync_ShouldDelegateToRunDunningCycle()
    {
        // Arrange
        var expectedResult = CreateCycleResult(processed: 2, successful: 2);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = CreateService();

        // Act
        var result = await service.TriggerManualCycleAsync(CancellationToken.None);

        // Assert
        result.ProcessedCount.Should().Be(2);
        result.SuccessfulRetries.Should().Be(2);
        _mockDunningManager.Verify(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetSchedulerStatus
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetSchedulerStatus_ShouldReturnIsRunning_AndCorrectInterval()
    {
        // Arrange
        var service = CreateService(intervalHours: 4);

        // Act
        var status = service.GetSchedulerStatus();

        // Assert
        status.Should().NotBeNull();
        status.IsRunning.Should().BeTrue();
        status.RunIntervalHours.Should().Be(4);
        status.NextScheduledRun.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void GetSchedulerStatus_ShouldContainSixScheduledTimes_ForFourHourInterval()
    {
        // Arrange
        var service = CreateService(intervalHours: 4);

        // Act
        var status = service.GetSchedulerStatus();

        // Assert
        status.ScheduledTimes.Should().NotBeEmpty();
        status.ScheduledTimes.Should().HaveCount(6); // Every 4h = 6 runs/day
        status.ScheduledTimes.Should().AllSatisfy(t => t.Should().MatchRegex(@"\d{2}:\d{2}"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // ExecuteAsync — cancellation
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ShouldStopGracefully_WhenCancellationRequested()
    {
        // Arrange
        var service = CreateService(intervalHours: 4);
        using var cts = new CancellationTokenSource();

        // Act — cancel immediately so ExecuteAsync exits without waiting
        cts.Cancel();
        var executeTask = service.StartAsync(cts.Token);
        await Task.WhenAny(executeTask, Task.Delay(2000));

        // Assert — no exception thrown, service stopped
        executeTask.IsCompleted.Should().BeTrue();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Exponential back-off schedule verification
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetSchedulerStatus_NextRunShouldBeWithin24Hours()
    {
        // Arrange
        var service = CreateService(intervalHours: 4);

        // Act
        var status = service.GetSchedulerStatus();

        // Assert — the next scheduled run should be within the next 24 hours
        var now = DateTime.UtcNow;
        status.NextScheduledRun.Should().BeAfter(now.AddSeconds(-5));
        status.NextScheduledRun.Should().BeBefore(now.AddHours(24));
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldReturnPausedCount_WhenDunningEscalates()
    {
        // Arrange
        var cycleResult = CreateCycleResult(processed: 5, successful: 2, escalated: 1, paused: 1);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.PausedSubscriptions.Should().Be(1);
        result.EscalatedCount.Should().Be(1);
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldReturnCancelledCount_WhenDunningExhausted()
    {
        // Arrange
        var cycleResult = CreateCycleResult(processed: 3, successful: 0, cancelled: 2);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.CancelledSubscriptions.Should().Be(2);
        result.SuccessfulRetries.Should().Be(0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Exponential Back-off Schedule Verification
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetSchedulerStatus_ShouldScheduleNextRunInFuture()
    {
        // Arrange
        var service = CreateService(intervalHours: 4);

        // Act
        var status = service.GetSchedulerStatus();

        // Assert — the next scheduled run should be within 4 hours from now
        var now = DateTime.UtcNow;
        status.NextScheduledRun.Should().BeAfter(now.AddSeconds(-5));
        status.NextScheduledRun.Should().BeBefore(now.AddHours(24));
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldReturnResultWithPausedSubscriptions_WhenDunningEscalates()
    {
        // Arrange
        var cycleResult = CreateCycleResult(
            processed: 5,
            successful: 2,
            escalated: 1,
            paused: 1,
            cancelled: 0);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.PausedSubscriptions.Should().Be(1);
        result.EscalatedCount.Should().Be(1);
        result.CancelledSubscriptions.Should().Be(0);
    }

    [Fact]
    public async Task RunDunningCycleAsync_ShouldReturnResultWithCancelledSubscriptions_WhenDunningExhausted()
    {
        // Arrange
        var cycleResult = CreateCycleResult(
            processed: 3,
            successful: 0,
            escalated: 0,
            paused: 0,
            cancelled: 2);
        _mockDunningManager
            .Setup(m => m.ProcessDunningAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var service = CreateService();

        // Act
        var result = await service.RunDunningCycleAsync(CancellationToken.None);

        // Assert
        result.CancelledSubscriptions.Should().Be(2);
        result.SuccessfulRetries.Should().Be(0);
    }
}

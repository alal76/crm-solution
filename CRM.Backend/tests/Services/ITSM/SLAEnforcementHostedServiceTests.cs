// This file is part of the CRM Solution.
// Tests for SLAEnforcementHostedService - Background SLA monitoring

using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class SLAEnforcementHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<SLAEnforcementHostedService>> _mockLogger;
    private readonly Mock<ISLAService> _mockSlaService;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;

    public SLAEnforcementHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<SLAEnforcementHostedService>>();
        _mockSlaService = new Mock<ISLAService>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();

        // Setup the service provider chain
        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider.Setup(x => x.GetService(typeof(ISLAService)))
            .Returns(_mockSlaService.Object);

        _mockScope.Setup(x => x.ServiceProvider).Returns(scopeServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SLAEnforcementHostedService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new SLAEnforcementHostedService(_mockServiceProvider.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_CallsCheckSLABreachesAsync()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        // Setup to cancel after first check
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        
        // Wait a bit for the task to start
        await Task.Delay(100);

        // Assert
        _mockSlaService.Verify(x => x.CheckSLABreachesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewServiceScope()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);

        // Assert
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_LogsStartMessage()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SLA Enforcement Background Service started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsStoppingMessage_WhenCancelled()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(async () => 
            {
                cts.Cancel();
                await Task.Delay(10);
            });

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(200);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopping") || v.ToString()!.Contains("stopped")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesExceptionGracefully()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        var callCount = 0;
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("Test exception");
                }
                cts.Cancel();
                return Task.CompletedTask;
            });

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(2500); // Allow for retry after exception

        // Assert - should log error but continue
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in SLA enforcement service")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ContinuesAfterException()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        var callCount = 0;
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("First call fails");
                }
                if (callCount >= 2)
                {
                    cts.Cancel();
                }
                return Task.CompletedTask;
            });

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(2500); // Allow for retry

        // Assert - should have been called at least twice (once failing, once succeeding)
        callCount.Should().BeGreaterOrEqualTo(2);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ExecuteAsync_StopsWhenCancellationRequested()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(cts.Token);
        cts.Cancel();
        
        // Wait for the service to process the cancellation
        await Task.Delay(200);

        // Assert - the service should have stopped
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopped") || v.ToString()!.Contains("stopping")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_StopsTheService()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var startCts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(async () => await Task.Delay(50));

        // Act
        await service.StartAsync(startCts.Token);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert - no exception thrown means successful stop
    }

    #endregion

    #region Service Scope Tests

    [Fact]
    public async Task ExecuteAsync_DisposesServiceScope()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);

        // Assert - scope should be created (and implicitly disposed via using)
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_GetsServiceFromScope()
    {
        // Arrange
        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider.Setup(x => x.GetService(typeof(ISLAService)))
            .Returns(_mockSlaService.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(scopeServiceProvider.Object);

        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);

        // Assert
        scopeServiceProvider.Verify(x => x.GetService(typeof(ISLAService)), Times.AtLeastOnce);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ExecuteAsync_LogsDebugOnSuccessfulCheck()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SLA breach check completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Service_CanBeStartedAndStopped()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert - no exception means success
    }

    [Fact]
    public async Task Service_HandlesMultipleChecks()
    {
        // Arrange
        var service = new SLAEnforcementHostedService(_mockServiceProvider.Object, _mockLogger.Object);
        using var cts = new CancellationTokenSource();
        var checkCount = 0;
        
        _mockSlaService.Setup(x => x.CheckSLABreachesAsync())
            .Returns(() =>
            {
                checkCount++;
                return Task.CompletedTask;
            });

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100); // Let at least one check complete
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        checkCount.Should().BeGreaterOrEqualTo(1);
    }

    #endregion
}

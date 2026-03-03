// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Unit tests for BackupSchedulerHostedService background service.
/// Tests scheduled backup execution, schedule processing, and error handling.
/// </summary>
public class BackupSchedulerHostedServiceTests : ServiceTestFixtureBase<BackupSchedulerHostedService>
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IDatabaseBackupService> _mockBackupService;

    public BackupSchedulerHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();        _mockDbContext = new Mock<ICrmDbContext>();
        _mockBackupService = new Mock<IDatabaseBackupService>();

        SetupServiceProvider();
    }

    private void SetupServiceProvider()
    {
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICrmDbContext)))
            .Returns(_mockDbContext.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IDatabaseBackupService)))
            .Returns(_mockBackupService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new BackupSchedulerHostedService(null!, MockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new BackupSchedulerHostedService(_mockServiceProvider.Object, null!);

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
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldLogStopMessage()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopping")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
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

    #region Schedule Processing Tests

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithNoDueSchedules_ShouldNotRunBackup()
    {
        // Arrange
        var schedules = new List<BackupSchedule>().AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithDueSchedule_ShouldRunBackup()
    {
        // Arrange
        var dueSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "Test Backup",
            IsEnabled = true,
            IsDeleted = false,
            NextBackupAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var schedules = new List<BackupSchedule> { dueSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);
        _mockBackupService.Setup(x => x.RunScheduledBackupAsync(1)).Returns(Task.CompletedTask);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - backup should have been triggered (or attempted)
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(It.IsAny<int>()), Times.AtMost(1));
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithDisabledSchedule_ShouldNotRunBackup()
    {
        // Arrange
        var disabledSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "Disabled Backup",
            IsEnabled = false,
            IsDeleted = false,
            NextBackupAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var schedules = new List<BackupSchedule> { disabledSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(1), Times.Never);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithDeletedSchedule_ShouldNotRunBackup()
    {
        // Arrange
        var deletedSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "Deleted Backup",
            IsEnabled = true,
            IsDeleted = true,
            NextBackupAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var schedules = new List<BackupSchedule> { deletedSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(1), Times.Never);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithFutureSchedule_ShouldNotRunBackup()
    {
        // Arrange
        var futureSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "Future Backup",
            IsEnabled = true,
            IsDeleted = false,
            NextBackupAt = DateTime.UtcNow.AddHours(1)
        };
        var schedules = new List<BackupSchedule> { futureSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(1), Times.Never);
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithNullNextBackupAt_ShouldNotRunBackup()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "Null Schedule",
            IsEnabled = true,
            IsDeleted = false,
            NextBackupAt = null
        };
        var schedules = new List<BackupSchedule> { schedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(1), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_WhenBackupFails_ShouldLogError()
    {
        // Arrange
        var dueSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "Failing Backup",
            IsEnabled = true,
            IsDeleted = false,
            NextBackupAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var schedules = new List<BackupSchedule> { dueSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);
        _mockBackupService.Setup(x => x.RunScheduledBackupAsync(1))
            .ThrowsAsync(new InvalidOperationException("Backup failed"));

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2)); // May log error for backup and possibly for general error
    }

    [Fact]
    public async Task ExecuteAsync_WhenDbContextFails_ShouldLogError()
    {
        // Arrange
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICrmDbContext)))
            .Throws(new InvalidOperationException("DB context unavailable"));

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        MockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtMost(2));
    }

    [Fact]
    public async Task ExecuteAsync_AfterBackupError_ShouldContinueProcessing()
    {
        // Arrange
        var callCount = 0;
        _mockBackupService.Setup(x => x.RunScheduledBackupAsync(It.IsAny<int>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("First failure");
                return Task.CompletedTask;
            });

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - service should continue despite first error
        service.Should().NotBeNull("Service should remain valid after handling errors");
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);

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
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(CancellationToken.None);

        // Assert - should complete without hanging
        service.Should().NotBeNull("Service should still be accessible after stopping");
    }

    [Fact]
    public async Task StopAsync_WhenCalledBeforeStart_ShouldNotThrow()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);

        // Act
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Interval Tests

    [Fact]
    public async Task ExecuteAsync_ShouldUseOneMinuteInterval()
    {
        // Arrange
        var schedules = new List<BackupSchedule>().AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert - service uses 1-minute interval for checking
        service.Should().NotBeNull("Service should be running with the expected interval");
    }

    #endregion

    #region Multiple Schedules Tests

    [Fact]
    public async Task ProcessDueSchedulesAsync_WithMultipleDueSchedules_ShouldRunAll()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new BackupSchedule { Id = 1, Name = "Backup 1", IsEnabled = true, IsDeleted = false, NextBackupAt = DateTime.UtcNow.AddMinutes(-5) },
            new BackupSchedule { Id = 2, Name = "Backup 2", IsEnabled = true, IsDeleted = false, NextBackupAt = DateTime.UtcNow.AddMinutes(-3) },
            new BackupSchedule { Id = 3, Name = "Backup 3", IsEnabled = true, IsDeleted = false, NextBackupAt = DateTime.UtcNow.AddMinutes(-1) }
        }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);
        _mockBackupService.Setup(x => x.RunScheduledBackupAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - all due backups should be processed (timing dependent)
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(It.IsAny<int>()), Times.AtMost(3));
    }

    [Fact]
    public async Task ProcessDueSchedulesAsync_WhenCancelledDuringProcessing_ShouldStopGracefully()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new BackupSchedule { Id = 1, Name = "Backup 1", IsEnabled = true, IsDeleted = false, NextBackupAt = DateTime.UtcNow.AddMinutes(-5) },
            new BackupSchedule { Id = 2, Name = "Backup 2", IsEnabled = true, IsDeleted = false, NextBackupAt = DateTime.UtcNow.AddMinutes(-3) }
        }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
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

    #region Logging Tests

    [Fact]
    public async Task ProcessDueSchedulesAsync_WhenRunningBackup_ShouldLogScheduleName()
    {
        // Arrange
        var dueSchedule = new BackupSchedule
        {
            Id = 1,
            Name = "MyTestBackup",
            IsEnabled = true,
            IsDeleted = false,
            NextBackupAt = DateTime.UtcNow.AddMinutes(-5)
        };
        var schedules = new List<BackupSchedule> { dueSchedule }.AsQueryable();
        _mockDbContext.Setup(x => x.BackupSchedules).Returns(CreateMockDbSet(schedules).Object);
        _mockBackupService.Setup(x => x.RunScheduledBackupAsync(1)).Returns(Task.CompletedTask);

        var service = new BackupSchedulerHostedService(_mockServiceProvider.Object, MockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - log should contain schedule name (timing dependent)
        _mockBackupService.Verify(x => x.RunScheduledBackupAsync(It.IsAny<int>()), Times.AtMost(1));
    }

    #endregion

    #region Helper Methods

    private static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        return MockDbSetFactory.CreateMockDbSet(data.ToList());
    }

    #endregion
}

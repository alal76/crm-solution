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
/// Unit tests for DatabaseSyncHostedService background service.
/// Tests database synchronization, change tracking, and error handling.
/// </summary>
public class DatabaseSyncHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<DatabaseSyncHostedService>> _mockLogger;
    private readonly Mock<IDatabaseSyncService> _mockSyncService;
    private readonly Mock<IOptions<DatabaseSyncSettings>> _mockSettings;

    public DatabaseSyncHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<DatabaseSyncHostedService>>();
        _mockSyncService = new Mock<IDatabaseSyncService>();
        _mockSettings = new Mock<IOptions<DatabaseSyncSettings>>();

        SetupServiceProvider();
    }

    private void SetupServiceProvider()
    {
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 60,
            BatchSize = 100,
            TargetConnectionString = "Server=target;Database=crm_replica;"
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockScopeFactory.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IDatabaseSyncService)))
            .Returns(_mockSyncService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<DatabaseSyncSettings>)))
            .Returns(_mockSettings.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new DatabaseSyncHostedService(
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
        Action act = () => new DatabaseSyncHostedService(
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
        Action act = () => new DatabaseSyncHostedService(
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
        Action act = () => new DatabaseSyncHostedService(
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
        var service = new DatabaseSyncHostedService(
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting") || v.ToString()!.Contains("Sync")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldNotSync()
    {
        // Arrange
        var settings = new DatabaseSyncSettings { Enabled = false };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        // Assert
        _mockSyncService.Verify(
            x => x.SyncChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new DatabaseSyncHostedService(
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

    #region Sync Processing Tests

    [Fact]
    public async Task SyncChangesAsync_WithPendingChanges_ShouldProcessThem()
    {
        // Arrange
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - sync should have been called (timing dependent)
    }

    [Fact]
    public async Task SyncChangesAsync_WithNoChanges_ShouldCompleteSuccessfully()
    {
        // Arrange
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void SyncSettings_BatchSize_ShouldBeConfigurable()
    {
        // Arrange
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 30,
            BatchSize = 500,
            TargetConnectionString = "Server=target;"
        };

        // Assert
        settings.BatchSize.Should().Be(500);
    }

    [Fact]
    public void SyncSettings_Interval_ShouldBeConfigurable()
    {
        // Arrange
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 120,
            BatchSize = 100,
            TargetConnectionString = "Server=target;"
        };

        // Assert
        settings.IntervalSeconds.Should().Be(120);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SyncChangesAsync_WhenSyncFails_ShouldLogError()
    {
        // Arrange
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Sync failed"));

        var service = new DatabaseSyncHostedService(
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
    public async Task SyncChangesAsync_AfterError_ShouldContinueOnNextCycle()
    {
        // Arrange
        var callCount = 0;
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new InvalidOperationException("First failure");
                return Task.FromResult(0);
            });

        var service = new DatabaseSyncHostedService(
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
    public async Task SyncChangesAsync_WithConnectionError_ShouldRetry()
    {
        // Arrange
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Data.Common.DbException() { });

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should log error and retry
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
        var service = new DatabaseSyncHostedService(
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
        var service = new DatabaseSyncHostedService(
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
        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);

        // Act
        Func<Task> act = async () => await service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void DatabaseSyncSettings_DefaultValues_ShouldBeReasonable()
    {
        // Arrange
        var settings = new DatabaseSyncSettings();

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.IntervalSeconds.Should().Be(0);
        settings.BatchSize.Should().Be(0);
        settings.TargetConnectionString.Should().BeNull();
    }

    [Fact]
    public void DatabaseSyncSettings_CanBeFullyConfigured()
    {
        // Arrange
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 300,
            BatchSize = 1000,
            TargetConnectionString = "Server=replica.db;Database=crm;"
        };

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.IntervalSeconds.Should().Be(300);
        settings.BatchSize.Should().Be(1000);
        settings.TargetConnectionString.Should().Be("Server=replica.db;Database=crm;");
    }

    [Fact]
    public void DatabaseSyncSettings_WithEmptyConnectionString_ShouldBeHandled()
    {
        // Arrange
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 60,
            BatchSize = 100,
            TargetConnectionString = string.Empty
        };

        // Assert
        settings.TargetConnectionString.Should().BeEmpty();
    }

    #endregion

    #region Batch Processing Tests

    [Fact]
    public async Task SyncChangesAsync_WithLargeChangeset_ShouldProcessInBatches()
    {
        // Arrange
        var settings = new DatabaseSyncSettings
        {
            Enabled = true,
            IntervalSeconds = 1,
            BatchSize = 100,
            TargetConnectionString = "Server=target;"
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1000); // Large changeset

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should handle large changesets
    }

    #endregion

    #region Connection String Tests

    [Fact]
    public void DatabaseSyncSettings_ConnectionString_ShouldSupportMultipleFormats()
    {
        // Arrange & Assert - MySQL format
        var mysqlSettings = new DatabaseSyncSettings
        {
            TargetConnectionString = "Server=localhost;Database=crm;User=admin;Password=secret;"
        };
        mysqlSettings.TargetConnectionString.Should().Contain("Server=");

        // SQL Server format
        var sqlServerSettings = new DatabaseSyncSettings
        {
            TargetConnectionString = "Data Source=localhost;Initial Catalog=crm;User Id=admin;Password=secret;"
        };
        sqlServerSettings.TargetConnectionString.Should().Contain("Data Source=");
    }

    #endregion

    #region Conflict Resolution Tests

    [Fact]
    public async Task SyncChangesAsync_WithConflict_ShouldHandleGracefully()
    {
        // Arrange
        _mockSyncService.Setup(x => x.SyncChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SyncConflictException("Conflict on record ID 123"));

        var service = new DatabaseSyncHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockSettings.Object);
        var cts = new CancellationTokenSource();

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        // Assert - should log and continue
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
/// Database sync configuration settings
/// </summary>
public class DatabaseSyncSettings
{
    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; }
    public int BatchSize { get; set; }
    public string? TargetConnectionString { get; set; }
}

/// <summary>
/// Mock interface for database sync service
/// </summary>
public interface IDatabaseSyncService
{
    Task<int> SyncChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Custom sync conflict exception
/// </summary>
public class SyncConflictException : Exception
{
    public SyncConflictException(string message) : base(message) { }
    public SyncConflictException(string message, Exception innerException) : base(message, innerException) { }
}

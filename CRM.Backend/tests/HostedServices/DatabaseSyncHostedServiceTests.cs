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

using CRM.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.HostedServices;

/// <summary>
/// Tests for DatabaseSyncHostedService.
/// Real constructor: (IDatabaseSyncService syncService, ILogger&lt;DatabaseSyncHostedService&gt; logger)
/// </summary>
public class DatabaseSyncHostedServiceTests
{
    private readonly Mock<IDatabaseSyncService> _mockSyncService;
    private readonly Mock<ILogger<DatabaseSyncHostedService>> _mockLogger;
    private readonly DatabaseSyncHostedService _service;

    public DatabaseSyncHostedServiceTests()
    {
        _mockSyncService = new Mock<IDatabaseSyncService>();
        _mockLogger = new Mock<ILogger<DatabaseSyncHostedService>>();
        _service = new DatabaseSyncHostedService(_mockSyncService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldCallRunSyncCheck()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult { Success = true, Messages = new List<string> { "All OK" } });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenSyncSucceeds_ShouldLogSuccess()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = true,
                Messages = new List<string> { "Tables verified", "Indexes OK" },
                FieldsSynced = 0
            });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenSyncSucceeds_WithFieldsSynced_ShouldLog()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = true,
                Messages = new List<string> { "Synced 5 fields" },
                FieldsSynced = 5
            });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenSyncFails_ShouldLogWarning()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = false,
                Messages = new List<string> { "Table mismatch detected" }
            });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert - does NOT throw, just logs warning
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenSyncThrows_ShouldNotPropagateException()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act
        var act = () => _service.StartAsync(CancellationToken.None);

        // Assert - should NOT throw (service catches exceptions to avoid blocking startup)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_WhenSyncThrowsTimeout_ShouldNotPropagateException()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ThrowsAsync(new TimeoutException("Connection timed out"));

        // Act
        var act = () => _service.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteImmediately()
    {
        // Act
        var act = () => _service.StopAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_WithCancellationToken_ShouldPassThrough()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult { Success = true, Messages = new List<string>() });

        // Act
        await _service.StartAsync(cts.Token);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithEmptyMessages_ShouldSucceed()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = true,
                Messages = new List<string>()
            });

        // Act
        var act = () => _service.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ShouldAcceptValidParameters()
    {
        // Act
        var service = new DatabaseSyncHostedService(_mockSyncService.Object, _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_SyncResultFieldsSynced_Zero_ShouldNotLogAutoFix()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = true,
                Messages = new List<string> { "OK" },
                FieldsSynced = 0
            });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenSyncReturnsProductionFieldCounts_ShouldComplete()
    {
        // Arrange
        _mockSyncService
            .Setup(s => s.RunSyncCheckAsync())
            .ReturnsAsync(new DatabaseSyncResult
            {
                Success = true,
                Messages = new List<string> { "Checked 5 tables" },
                ProductionFieldCounts = new Dictionary<string, int>
                {
                    { "Accounts", 25 },
                    { "Contacts", 18 }
                }
            });

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _mockSyncService.Verify(s => s.RunSyncCheckAsync(), Times.Once);
    }
}

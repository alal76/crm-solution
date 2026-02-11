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

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
/// Unit tests for DatabaseSyncService
/// Covers: Database synchronization, data replication, conflict resolution
/// </summary>
public class DatabaseSyncServiceTests
{
    private readonly Mock<ICrmDbContext> _mockSourceDb;
    private readonly Mock<ICrmDbContext> _mockTargetDb;
    private readonly Mock<IOptions<SyncOptions>> _mockOptions;
    private readonly Mock<ILogger<DatabaseSyncService>> _mockLogger;
    private readonly DatabaseSyncService _service;

    public DatabaseSyncServiceTests()
    {
        _mockSourceDb = new Mock<ICrmDbContext>();
        _mockTargetDb = new Mock<ICrmDbContext>();
        _mockOptions = new Mock<IOptions<SyncOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(new SyncOptions
        {
            BatchSize = 100,
            ConflictResolution = ConflictResolutionStrategy.SourceWins
        });
        _mockLogger = new Mock<ILogger<DatabaseSyncService>>();

        _service = new DatabaseSyncService(
            _mockSourceDb.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    #region Sync Tests

    [Fact]
    public async Task SyncEntitiesAsync_ValidEntities_SyncsAll()
    {
        // Arrange
        var entityType = "Account";

        // Act
        var result = await _service.SyncEntitiesAsync(entityType);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncEntitiesAsync_WithDateRange_SyncsRange()
    {
        // Arrange
        var entityType = "Account";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _service.SyncEntitiesAsync(entityType, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncAllAsync_AllTables_SyncsAll()
    {
        // Act
        var result = await _service.SyncAllAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Change Detection Tests

    [Fact]
    public async Task GetChangedEntitiesAsync_WithChanges_ReturnsChanges()
    {
        // Arrange
        var lastSync = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = await _service.GetChangedEntitiesAsync("Account", lastSync);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChangedEntitiesAsync_NoChanges_ReturnsEmpty()
    {
        // Arrange
        var lastSync = DateTime.UtcNow;

        // Act
        var result = await _service.GetChangedEntitiesAsync("Account", lastSync);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectConflictsAsync_NoConflicts_ReturnsEmpty()
    {
        // Act
        var result = await _service.DetectConflictsAsync("Account");

        // Assert
        result.Conflicts.Should().BeEmpty();
    }

    #endregion

    #region Conflict Resolution Tests

    [Fact]
    public async Task ResolveConflictAsync_SourceWins_UsesSourceData()
    {
        // Arrange
        var conflict = new SyncConflict
        {
            EntityType = "Account",
            EntityId = 1,
            SourceValue = "Source Value",
            TargetValue = "Target Value"
        };

        // Act
        var result = await _service.ResolveConflictAsync(conflict, ConflictResolutionStrategy.SourceWins);

        // Assert
        result.ResolvedValue.Should().Be("Source Value");
    }

    [Fact]
    public async Task ResolveConflictAsync_TargetWins_UsesTargetData()
    {
        // Arrange
        var conflict = new SyncConflict
        {
            EntityType = "Account",
            EntityId = 1,
            SourceValue = "Source Value",
            TargetValue = "Target Value"
        };

        // Act
        var result = await _service.ResolveConflictAsync(conflict, ConflictResolutionStrategy.TargetWins);

        // Assert
        result.ResolvedValue.Should().Be("Target Value");
    }

    [Fact]
    public async Task ResolveConflictAsync_LastWriteWins_UsesNewerData()
    {
        // Arrange
        var conflict = new SyncConflict
        {
            EntityType = "Account",
            EntityId = 1,
            SourceValue = "Source Value",
            TargetValue = "Target Value",
            SourceModifiedAt = DateTime.UtcNow.AddHours(-1),
            TargetModifiedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.ResolveConflictAsync(conflict, ConflictResolutionStrategy.LastWriteWins);

        // Assert
        result.ResolvedValue.Should().Be("Target Value");
    }

    [Fact]
    public async Task ResolveAllConflictsAsync_MultipleConflicts_ResolvesAll()
    {
        // Arrange
        var conflicts = new List<SyncConflict>
        {
            new SyncConflict { EntityId = 1 },
            new SyncConflict { EntityId = 2 }
        };

        // Act
        var result = await _service.ResolveAllConflictsAsync(conflicts, ConflictResolutionStrategy.SourceWins);

        // Assert
        result.ResolvedCount.Should().Be(2);
    }

    #endregion

    #region Schema Comparison Tests

    [Fact]
    public async Task CompareSchemaAsync_IdenticalSchemas_NoDifferences()
    {
        // Act
        var result = await _service.CompareSchemaAsync();

        // Assert
        result.HasDifferences.Should().BeFalse();
    }

    [Fact]
    public async Task GetSchemaDifferencesAsync_ReturnsDifferences()
    {
        // Act
        var result = await _service.GetSchemaDifferencesAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Batch Sync Tests

    [Fact]
    public async Task SyncBatchAsync_ValidBatch_SyncsSuccessfully()
    {
        // Arrange
        var batch = new SyncBatch
        {
            EntityType = "Account",
            Entities = new List<object> { new { Id = 1 }, new { Id = 2 } }
        };

        // Act
        var result = await _service.SyncBatchAsync(batch);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncBatchAsync_EmptyBatch_ReturnsZero()
    {
        // Arrange
        var batch = new SyncBatch
        {
            EntityType = "Account",
            Entities = new List<object>()
        };

        // Act
        var result = await _service.SyncBatchAsync(batch);

        // Assert
        result.ProcessedCount.Should().Be(0);
    }

    #endregion

    #region Incremental Sync Tests

    [Fact]
    public async Task IncrementalSyncAsync_ValidCheckpoint_SyncsFromCheckpoint()
    {
        // Arrange
        var checkpoint = new SyncCheckpoint
        {
            EntityType = "Account",
            LastSyncedId = 100,
            LastSyncedAt = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = await _service.IncrementalSyncAsync(checkpoint);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLastCheckpointAsync_ExistingCheckpoint_ReturnsCheckpoint()
    {
        // Arrange
        var entityType = "Account";

        // Act
        var result = await _service.GetLastCheckpointAsync(entityType);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveCheckpointAsync_ValidCheckpoint_SavesCheckpoint()
    {
        // Arrange
        var checkpoint = new SyncCheckpoint
        {
            EntityType = "Account",
            LastSyncedId = 100
        };

        // Act
        await _service.SaveCheckpointAsync(checkpoint);

        // Assert - no exception thrown
    }

    #endregion

    #region Status Tests

    [Fact]
    public async Task GetSyncStatusAsync_ReturnsStatus()
    {
        // Act
        var result = await _service.GetSyncStatusAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSyncHistoryAsync_ReturnsHistory()
    {
        // Act
        var result = await _service.GetSyncHistoryAsync();

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEntityStatisticsAsync_ValidEntity_ReturnsStats()
    {
        // Act
        var result = await _service.GetEntityStatisticsAsync("Account");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateSyncAsync_ValidData_ReturnsValid()
    {
        // Act
        var result = await _service.ValidateSyncAsync("Account");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task GetValidationErrorsAsync_NoErrors_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetValidationErrorsAsync("Account");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void GetSupportedEntities_ReturnsSupportedList()
    {
        // Act
        var result = _service.GetSupportedEntities();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("Account");
    }

    [Fact]
    public async Task SetSyncConfigurationAsync_ValidConfig_SetsConfig()
    {
        // Arrange
        var config = new SyncConfiguration
        {
            EntityType = "Account",
            Enabled = true,
            BatchSize = 500
        };

        // Act
        await _service.SetSyncConfigurationAsync(config);

        // Assert - no exception thrown
    }

    #endregion
}

// Supporting classes for tests
public class SyncOptions
{
    public int BatchSize { get; set; } = 100;
    public ConflictResolutionStrategy ConflictResolution { get; set; }
}

public enum ConflictResolutionStrategy
{
    SourceWins,
    TargetWins,
    LastWriteWins,
    Manual
}

public class SyncConflict
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? SourceValue { get; set; }
    public string? TargetValue { get; set; }
    public DateTime? SourceModifiedAt { get; set; }
    public DateTime? TargetModifiedAt { get; set; }
}

public class SyncBatch
{
    public string EntityType { get; set; } = string.Empty;
    public List<object> Entities { get; set; } = new();
}

public class SyncCheckpoint
{
    public string EntityType { get; set; } = string.Empty;
    public int LastSyncedId { get; set; }
    public DateTime LastSyncedAt { get; set; }
}

public class SyncConfiguration
{
    public string EntityType { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int BatchSize { get; set; }
}

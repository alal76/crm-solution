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
using System.IO;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DatabaseBackupService
/// Covers: Backup creation, restore, scheduling, retention
/// </summary>
public class DatabaseBackupServiceTests
{
    private readonly Mock<IRepository<DatabaseBackup>> _mockBackupRepository;
    private readonly Mock<IRepository<BackupSchedule>> _mockScheduleRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<DatabaseBackupService>> _mockLogger;
    private readonly Mock<IOptions<BackupSettings>> _mockOptions;
    private readonly DatabaseBackupService _service;

    public DatabaseBackupServiceTests()
    {
        _mockBackupRepository = new Mock<IRepository<DatabaseBackup>>();
        _mockScheduleRepository = new Mock<IRepository<BackupSchedule>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DatabaseBackupService>>();
        _mockOptions = new Mock<IOptions<BackupSettings>>();

        _mockOptions.Setup(o => o.Value).Returns(new BackupSettings
        {
            BackupPath = "/backups",
            RetentionDays = 30,
            MaxBackups = 100
        });

        _service = new DatabaseBackupService(
            _mockBackupRepository.Object,
            _mockScheduleRepository.Object,
            _mockDbContext.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    #region Create Backup Tests

    [Fact]
    public async Task CreateBackupAsync_ValidRequest_ReturnsBackup()
    {
        // Arrange
        var request = new CreateBackupRequest
        {
            Name = "Daily Backup",
            Description = "Automated daily backup",
            BackupType = BackupType.Full
        };

        _mockBackupRepository.Setup(r => r.AddAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => { b.Id = 1; return b; });

        // Act
        var result = await _service.CreateBackupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Daily Backup");
    }

    [Fact]
    public async Task CreateBackupAsync_FullBackup_SetsFullType()
    {
        // Arrange
        var request = new CreateBackupRequest
        {
            Name = "Full Backup",
            BackupType = BackupType.Full
        };

        _mockBackupRepository.Setup(r => r.AddAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => { b.Id = 1; return b; });

        // Act
        var result = await _service.CreateBackupAsync(request);

        // Assert
        result!.BackupType.Should().Be(BackupType.Full);
    }

    [Fact]
    public async Task CreateBackupAsync_IncrementalBackup_SetsIncrementalType()
    {
        // Arrange
        var request = new CreateBackupRequest
        {
            Name = "Incremental Backup",
            BackupType = BackupType.Incremental
        };

        _mockBackupRepository.Setup(r => r.AddAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => { b.Id = 1; return b; });

        // Act
        var result = await _service.CreateBackupAsync(request);

        // Assert
        result!.BackupType.Should().Be(BackupType.Incremental);
    }

    [Fact]
    public async Task CreateBackupAsync_NullRequest_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateBackupAsync(null!));
    }

    [Fact]
    public async Task CreateBackupAsync_GeneratesFileName()
    {
        // Arrange
        var request = new CreateBackupRequest
        {
            Name = "Test Backup"
        };

        _mockBackupRepository.Setup(r => r.AddAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => { b.Id = 1; return b; });

        // Act
        var result = await _service.CreateBackupAsync(request);

        // Assert
        result!.FileName.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Get Backup Tests

    [Fact]
    public async Task GetBackupsAsync_ReturnsAllBackups()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, Name = "Backup 1" },
            new DatabaseBackup { Id = 2, Name = "Backup 2" }
        };

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBackupByIdAsync_ExistingBackup_ReturnsBackup()
    {
        // Arrange
        var backup = new DatabaseBackup { Id = 1, Name = "Test Backup" };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        // Act
        var result = await _service.GetBackupByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Backup");
    }

    [Fact]
    public async Task GetBackupByIdAsync_NonExistingBackup_ReturnsNull()
    {
        // Arrange
        _mockBackupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((DatabaseBackup?)null);

        // Act
        var result = await _service.GetBackupByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestBackupAsync_ReturnsLatest()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new DatabaseBackup { Id = 2, CreatedAt = DateTime.UtcNow }
        };

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetLatestBackupAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetBackupsByTypeAsync_ReturnsFilteredBackups()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, BackupType = BackupType.Full }
        };

        _mockBackupRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DatabaseBackup, bool>>>()))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsByTypeAsync(BackupType.Full);

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Restore Backup Tests

    [Fact]
    public async Task RestoreBackupAsync_ValidBackup_ReturnsTrue()
    {
        // Arrange
        var backup = new DatabaseBackup
        {
            Id = 1,
            Name = "Test Backup",
            FilePath = "/backups/test.sql",
            Status = BackupStatus.Completed
        };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        _mockBackupRepository.Setup(r => r.UpdateAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => b);

        // Act
        var result = await _service.RestoreBackupAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RestoreBackupAsync_NonExistingBackup_ReturnsFalse()
    {
        // Arrange
        _mockBackupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((DatabaseBackup?)null);

        // Act
        var result = await _service.RestoreBackupAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreBackupAsync_FailedBackup_ThrowsException()
    {
        // Arrange
        var backup = new DatabaseBackup
        {
            Id = 1,
            Status = BackupStatus.Failed
        };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RestoreBackupAsync(1));
    }

    [Fact]
    public async Task RestoreBackupAsync_CreatesRestorePoint()
    {
        // Arrange
        var backup = new DatabaseBackup
        {
            Id = 1,
            FilePath = "/backups/test.sql",
            Status = BackupStatus.Completed
        };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        _mockBackupRepository.Setup(r => r.AddAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => { b.Id = 2; return b; });

        _mockBackupRepository.Setup(r => r.UpdateAsync(It.IsAny<DatabaseBackup>()))
            .ReturnsAsync((DatabaseBackup b) => b);

        // Act
        await _service.RestoreBackupAsync(1, createRestorePoint: true);

        // Assert
        _mockBackupRepository.Verify(r => r.AddAsync(It.Is<DatabaseBackup>(b =>
            b.Name.Contains("Pre-restore"))), Times.Once);
    }

    #endregion

    #region Delete Backup Tests

    [Fact]
    public async Task DeleteBackupAsync_ExistingBackup_ReturnsTrue()
    {
        // Arrange
        var backup = new DatabaseBackup { Id = 1, FilePath = "/backups/test.sql" };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        _mockBackupRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteBackupAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBackupAsync_NonExistingBackup_ReturnsFalse()
    {
        // Arrange
        _mockBackupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((DatabaseBackup?)null);

        // Act
        var result = await _service.DeleteBackupAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Schedule Tests

    [Fact]
    public async Task CreateScheduleAsync_ValidSchedule_ReturnsSchedule()
    {
        // Arrange
        var request = new CreateBackupScheduleRequest
        {
            Name = "Daily Backup",
            CronExpression = "0 0 * * *",
            BackupType = BackupType.Full,
            IsEnabled = true
        };

        _mockScheduleRepository.Setup(r => r.AddAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.CreateScheduleAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetSchedulesAsync_ReturnsAllSchedules()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new BackupSchedule { Id = 1, Name = "Daily" },
            new BackupSchedule { Id = 2, Name = "Weekly" }
        };

        _mockScheduleRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetSchedulesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task EnableScheduleAsync_DisabledSchedule_EnablesSchedule()
    {
        // Arrange
        var schedule = new BackupSchedule { Id = 1, IsEnabled = false };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);

        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => { s.IsEnabled = true; return s; });

        // Act
        var result = await _service.EnableScheduleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DisableScheduleAsync_EnabledSchedule_DisablesSchedule()
    {
        // Arrange
        var schedule = new BackupSchedule { Id = 1, IsEnabled = true };

        _mockScheduleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);

        _mockScheduleRepository.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => { s.IsEnabled = false; return s; });

        // Act
        var result = await _service.DisableScheduleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteScheduleAsync_ExistingSchedule_ReturnsTrue()
    {
        // Arrange
        _mockScheduleRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new BackupSchedule { Id = 1 });

        _mockScheduleRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteScheduleAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Retention Tests

    [Fact]
    public async Task ApplyRetentionPolicyAsync_RemovesOldBackups()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new DatabaseBackup { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        };

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        _mockBackupRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var deletedCount = await _service.ApplyRetentionPolicyAsync();

        // Assert
        deletedCount.Should().Be(1);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_RespectsMaxBackups()
    {
        // Arrange
        var backups = Enumerable.Range(1, 150).Select(i => new DatabaseBackup
        {
            Id = i,
            CreatedAt = DateTime.UtcNow.AddHours(-i)
        }).ToList();

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        _mockBackupRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var deletedCount = await _service.ApplyRetentionPolicyAsync();

        // Assert
        deletedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRetentionStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, FileSize = 1024 * 1024 },
            new DatabaseBackup { Id = 2, FileSize = 2048 * 1024 }
        };

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetRetentionStatisticsAsync();

        // Assert
        result.TotalBackups.Should().Be(2);
        result.TotalSize.Should().Be(3 * 1024 * 1024);
    }

    #endregion

    #region Verification Tests

    [Fact]
    public async Task VerifyBackupAsync_ValidBackup_ReturnsTrue()
    {
        // Arrange
        var backup = new DatabaseBackup
        {
            Id = 1,
            FilePath = "/backups/test.sql",
            Status = BackupStatus.Completed,
            Checksum = "abc123"
        };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        // Act
        var result = await _service.VerifyBackupAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyBackupAsync_CorruptBackup_ReturnsFalse()
    {
        // Arrange
        var backup = new DatabaseBackup
        {
            Id = 1,
            FilePath = "/backups/test.sql",
            Status = BackupStatus.Completed,
            IsCorrupt = true
        };

        _mockBackupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(backup);

        // Act
        var result = await _service.VerifyBackupAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetBackupStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var backups = new List<DatabaseBackup>
        {
            new DatabaseBackup { Id = 1, Status = BackupStatus.Completed, BackupType = BackupType.Full },
            new DatabaseBackup { Id = 2, Status = BackupStatus.Completed, BackupType = BackupType.Incremental },
            new DatabaseBackup { Id = 3, Status = BackupStatus.Failed, BackupType = BackupType.Full }
        };

        _mockBackupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupStatisticsAsync();

        // Assert
        result.TotalBackups.Should().Be(3);
        result.SuccessfulBackups.Should().Be(2);
        result.FailedBackups.Should().Be(1);
    }

    #endregion
}

// Supporting classes for tests
public class BackupSettings
{
    public string BackupPath { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public int MaxBackups { get; set; }
}

public enum BackupType
{
    Full,
    Incremental,
    Differential
}

public enum BackupStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public class CreateBackupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BackupType BackupType { get; set; }
}

public class CreateBackupScheduleRequest
{
    public string Name { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public BackupType BackupType { get; set; }
    public bool IsEnabled { get; set; }
}

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
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for DatabaseBackupService (TCOV-012).
/// The service creates a backup directory in the constructor; we redirect to a temp path.
/// </summary>
public class DatabaseBackupServiceTests : ServiceTestFixtureBase<DatabaseBackupService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly string _tempBackupDir;
    private readonly DatabaseBackupService _service;

    public DatabaseBackupServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockConfig = new Mock<IConfiguration>();
        _tempBackupDir = Path.Combine(Path.GetTempPath(), $"crm_backup_test_{Guid.NewGuid()}");
        _mockConfig.Setup(c => c["Backup:DefaultPath"]).Returns(_tempBackupDir);
        _service = new DatabaseBackupService(_mockDbContext.Object, MockLogger.Object, _mockConfig.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_AndBackupDirectory()
    {
        _service.Should().NotBeNull();
        Directory.Exists(_tempBackupDir).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllBackupsAsync_ShouldReturnEmpty_WhenNoBackupsExist()
    {
        _mockDbContext.Setup(c => c.DatabaseBackups)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DatabaseBackup>()).Object);

        var result = await _service.GetAllBackupsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBackupByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _mockDbContext.Setup(c => c.DatabaseBackups)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DatabaseBackup>()).Object);

        var result = await _service.GetBackupByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBackupSettingsAsync_ShouldReturnResult()
    {
        _mockDbContext.Setup(c => c.DatabaseBackups)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<DatabaseBackup>()).Object);
        _mockDbContext.Setup(c => c.BackupSchedules)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<BackupSchedule>()).Object);

        var result = await _service.GetBackupSettingsAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateBackupPathAsync_ShouldComplete_WhenPathIsValid()
    {
        var newPath = Path.Combine(Path.GetTempPath(), $"crm_backup_{Guid.NewGuid()}");

        var act = async () => await _service.UpdateBackupPathAsync(newPath);

        await act.Should().NotThrowAsync();
        if (Directory.Exists(newPath)) Directory.Delete(newPath);
    }
}

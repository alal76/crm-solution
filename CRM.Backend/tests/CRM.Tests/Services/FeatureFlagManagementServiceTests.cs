// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for FeatureFlagManagementService, covering REV-FE-005: the admin
/// Feature Flags panel save action must actually persist toggles (not just log
/// an audit entry) so that a subsequent read reflects the change.
/// </summary>
public class FeatureFlagManagementServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<FeatureFlagManagementService>> _mockLogger;
    private readonly List<FeatureFlag> _featureFlags;
    private readonly List<FeatureFlagAuditLog> _auditLogs;
    private readonly FeatureFlagManagementService _service;

    public FeatureFlagManagementServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<FeatureFlagManagementService>>();

        _featureFlags = new List<FeatureFlag>();
        _auditLogs = new List<FeatureFlagAuditLog>();

        var featureFlagSet = MockDbSetFactory.CreateMockDbSet(_featureFlags);
        var auditLogSet = MockDbSetFactory.CreateMockDbSet(_auditLogs);

        _mockDbContext.Setup(c => c.FeatureFlags).Returns(featureFlagSet.Object);
        _mockDbContext.Setup(c => c.FeatureFlagAuditLogs).Returns(auditLogSet.Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Default: appsettings-backed flags are all disabled unless overridden by test.
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _service = new FeatureFlagManagementService(
            _mockDbContext.Object,
            _mockFeatureManager.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateFlagAsync_NewFlag_PersistsToFeatureFlagsTable()
    {
        // Act
        var result = await _service.UpdateFlagAsync(
            "EnableITSM",
            new UpdateFeatureFlagDto { Name = "EnableITSM", Enabled = true },
            updatedById: 1);

        // Assert
        result.Should().BeTrue();
        _featureFlags.Should().ContainSingle(f => f.Key == "EnableITSM" && f.IsEnabled);
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFlagAsync_ExistingFlag_UpdatesInPlaceRatherThanDuplicating()
    {
        // Arrange
        _featureFlags.Add(new FeatureFlag { Id = 1, Key = "EnableITSM", DisplayName = "ITSM Module", IsEnabled = false });

        // Act
        var result = await _service.UpdateFlagAsync(
            "EnableITSM",
            new UpdateFeatureFlagDto { Name = "EnableITSM", Enabled = true },
            updatedById: 1);

        // Assert
        result.Should().BeTrue();
        _featureFlags.Should().ContainSingle(f => f.Key == "EnableITSM");
        _featureFlags.Single(f => f.Key == "EnableITSM").IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFlagAsync_WritesAuditLogEntry()
    {
        // Act
        await _service.UpdateFlagAsync(
            "UseExternalSearch",
            new UpdateFeatureFlagDto { Name = "UseExternalSearch", Enabled = true, Reason = "Testing" },
            updatedById: 42);

        // Assert
        _auditLogs.Should().ContainSingle(a =>
            a.FlagName == "UseExternalSearch" &&
            a.NewValue == "True" &&
            a.ChangedById == 42 &&
            a.Reason == "Testing");
    }

    [Fact]
    public async Task GetAllFlagsAsync_ReflectsPersistedOverride_AfterUpdateFlagAsync()
    {
        // Arrange: appsettings default for EnableITSM is disabled (see constructor setup).

        // Act: toggle it on via the same path the admin panel's Save button uses.
        await _service.UpdateFlagAsync(
            "EnableITSM",
            new UpdateFeatureFlagDto { Name = "EnableITSM", Enabled = true },
            updatedById: 1);

        var flags = await _service.GetAllFlagsAsync();

        // Assert: the read path must reflect the persisted override, not the stale
        // appsettings-backed IFeatureManager default.
        flags.Should().ContainSingle(f => f.Name == "EnableITSM" && f.Enabled);
    }

    [Fact]
    public async Task GetAllFlagsAsync_FallsBackToFeatureManager_WhenNoPersistedOverrideExists()
    {
        // Arrange
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync("EnableMarketing")).ReturnsAsync(true);

        // Act
        var flags = await _service.GetAllFlagsAsync();

        // Assert
        flags.Should().ContainSingle(f => f.Name == "EnableMarketing" && f.Enabled);
    }
}

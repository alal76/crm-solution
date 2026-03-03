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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserGroupService — covering ValidateAndNormalizeGroupPermissionsAsync
/// (TODO-SYS012-002) and audit-log integration on update (TODO-SYS012-003).
/// </summary>
public class UserGroupServiceTests : ServiceTestFixtureBase<UserGroupService>
{    private readonly Mock<IAuditLogService> _mockAuditLog;
    private readonly List<UserGroup> _groups;

    public UserGroupServiceTests()
    {        _mockAuditLog = new Mock<IAuditLogService>();
        _groups = new List<UserGroup>();

        MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        MockContext
            .Setup(c => c.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    private UserGroupService CreateService(bool withAuditLog = false)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(_groups);
        mockSet.Setup(s => s.Update(It.IsAny<UserGroup>()));
        MockContext.Setup(c => c.UserGroups).Returns(mockSet.Object);
        return new UserGroupService(
            MockContext.Object,
            MockLogger.Object,
            withAuditLog ? _mockAuditLog.Object : null);
    }

    private static UserGroup MakeGroup(int id, string menuJson = "[]", bool isDefault = false) =>
        new()
        {
            Id = id,
            Name = $"Group-{id}",
            Description = "Test group",
            IsActive = true,
            IsDefault = isDefault,
            IsDeleted = false,
            IsSystemAdmin = false,
            AccessibleMenuItems = menuJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    // ── TODO-SYS012-002 : ValidateAndNormalizeGroupPermissionsAsync ─────────────

    /// <summary>
    /// All recognised menu keys should be preserved in the output.
    /// </summary>
    [Fact]
    public async Task ValidateAndNormalizeGroupPermissionsAsync_ShouldKeepValidMenuItems()
    {
        // Arrange
        var group = MakeGroup(1, menuJson: "[\"Dashboard\",\"Accounts\",\"Contacts\"]");
        _groups.Add(group);
        var svc = CreateService();

        // Act
        var result = await svc.ValidateAndNormalizeGroupPermissionsAsync(1);

        // Assert
        result.Should().NotBeNull();
        var savedJson = group.AccessibleMenuItems;
        savedJson.Should().Contain("Dashboard");
        savedJson.Should().Contain("Accounts");
        savedJson.Should().Contain("Contacts");
    }

    /// <summary>
    /// Unrecognised menu keys should be stripped from the stored value.
    /// </summary>
    [Fact]
    public async Task ValidateAndNormalizeGroupPermissionsAsync_ShouldRemoveInvalidMenuKeys()
    {
        // Arrange — "UnknownModule" is not a recognised nav key
        var group = MakeGroup(1, menuJson: "[\"Dashboard\",\"UnknownModule\"]");
        _groups.Add(group);
        var svc = CreateService();

        // Act
        await svc.ValidateAndNormalizeGroupPermissionsAsync(1);

        // Assert
        group.AccessibleMenuItems.Should().Contain("Dashboard");
        group.AccessibleMenuItems.Should().NotContain("UnknownModule");
    }

    /// <summary>
    /// When the stored AccessibleMenuItems JSON is malformed the field should be
    /// reset to an empty JSON array rather than propagating an exception.
    /// </summary>
    [Fact]
    public async Task ValidateAndNormalizeGroupPermissionsAsync_ShouldHandleMalformedJson()
    {
        // Arrange — broken JSON that cannot be parsed
        var group = MakeGroup(1, menuJson: "NOT_VALID_JSON");
        _groups.Add(group);
        var svc = CreateService();

        // Act
        var act = () => svc.ValidateAndNormalizeGroupPermissionsAsync(1);

        // Assert — no exception; field normalised to empty list
        await act.Should().NotThrowAsync();
        group.AccessibleMenuItems.Should().Be("[]");
    }

    // ── TODO-SYS012-003 : Audit logging on group update ─────────────────────────

    /// <summary>
    /// UpdateGroupAsync should call IAuditLogService.LogUpdateAsync once when
    /// the audit service is injected.
    /// </summary>
    [Fact]
    public async Task UpdateGroupAsync_ShouldCallLogUpdateAsync_WhenAuditServiceIsProvided()
    {
        // Arrange
        var group = MakeGroup(10, isDefault: false);
        _groups.Add(group);
        _mockAuditLog
            .Setup(a => a.LogUpdateAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<List<string>>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var svc = CreateService(withAuditLog: true);

        var request = new CreateUserGroupRequest
        {
            Name = "Updated Group",
            Description = "desc",
            IsDefault = false,
            IsActive = true
        };

        // Act
        var result = await svc.UpdateGroupAsync(10, request);

        // Assert
        result.Should().NotBeNull();
        _mockAuditLog.Verify(
            a => a.LogUpdateAsync(
                "UserGroup",
                10,
                It.IsAny<string>(),
                null,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<List<string>>(),
                null, null,
                It.IsAny<CancellationToken>()),
            Times.Once,
            "LogUpdateAsync must be called exactly once for a UserGroup update");
    }

    /// <summary>
    /// UpdateGroupAsync should complete successfully and NOT throw when
    /// no IAuditLogService is injected (null).
    /// </summary>
    [Fact]
    public async Task UpdateGroupAsync_ShouldNotThrow_WhenAuditServiceIsNull()
    {
        // Arrange
        var group = MakeGroup(20, isDefault: false);
        _groups.Add(group);

        // Service created WITHOUT an audit log service
        var svc = CreateService(withAuditLog: false);

        var request = new CreateUserGroupRequest
        {
            Name = "No-Audit Group",
            Description = "desc",
            IsDefault = false,
            IsActive = true
        };

        // Act & Assert
        var act = () => svc.UpdateGroupAsync(20, request);
        await act.Should().NotThrowAsync("UpdateGroupAsync must be resilient when audit service is absent");
    }
}

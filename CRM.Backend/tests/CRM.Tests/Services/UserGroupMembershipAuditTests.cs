// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SYS003-003: Unit tests for UserGroup membership audit log integration.

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
/// Tests that <see cref="UserGroupService"/> emits audit log entries when group membership
/// changes (add/remove), per TODO-SYS003-003.
/// </summary>
public class UserGroupMembershipAuditTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<UserGroupService>> _mockLogger;
    private readonly Mock<IAuditLogService> _mockAuditLog;

    public UserGroupMembershipAuditTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<UserGroupService>>();
        _mockAuditLog = new Mock<IAuditLogService>();

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);
    }

    private UserGroupService CreateService(bool withAudit = true) =>
        new(_mockContext.Object, _mockLogger.Object, withAudit ? _mockAuditLog.Object : null);

    // ──────────────────────────────────────────────────────────────────────
    // AddUserToGroupAsync
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddUserToGroupAsync_CallsLogActionAsync_WhenAuditServiceProvided()
    {
        const int groupId = 1;
        const int userId = 10;

        // Arrange: existing group + user, no pre-existing membership
        var groups = new List<UserGroup> { new() { Id = groupId, Name = "Engineering" } };
        var users = new List<User> { new() { Id = userId } };
        var members = new List<UserGroupMember>();

        _mockContext.Setup(c => c.UserGroups).Returns(MockDbSetFactory.CreateMockDbSet(groups).Object);
        _mockContext.Setup(c => c.Users).Returns(MockDbSetFactory.CreateMockDbSet(users).Object);
        _mockContext.Setup(c => c.UserGroupMembers).Returns(MockDbSetFactory.CreateMockDbSet(members).Object);

        _mockAuditLog
            .Setup(a => a.LogActionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // Act
        await service.AddUserToGroupAsync(groupId, userId);

        // Assert
        _mockAuditLog.Verify(a => a.LogActionAsync(
            "UserAddedToGroup",
            "UserGroup",
            groupId,
            userId,
            It.Is<string>(s => s.Contains(userId.ToString()) && s.Contains(groupId.ToString())),
            null, null, default),
            Times.Once);
    }

    [Fact]
    public async Task AddUserToGroupAsync_DoesNotThrow_WhenAuditServiceIsNull()
    {
        const int groupId = 2;
        const int userId = 20;

        var groups = new List<UserGroup> { new() { Id = groupId, Name = "Support" } };
        var users = new List<User> { new() { Id = userId } };
        var members = new List<UserGroupMember>();

        _mockContext.Setup(c => c.UserGroups).Returns(MockDbSetFactory.CreateMockDbSet(groups).Object);
        _mockContext.Setup(c => c.Users).Returns(MockDbSetFactory.CreateMockDbSet(users).Object);
        _mockContext.Setup(c => c.UserGroupMembers).Returns(MockDbSetFactory.CreateMockDbSet(members).Object);

        var service = CreateService(withAudit: false);

        // Should complete without throwing even when audit service is null
        var act = async () => await service.AddUserToGroupAsync(groupId, userId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddUserToGroupAsync_Throws_WhenGroupNotFound()
    {
        var groups = new List<UserGroup>(); // no groups
        var users = new List<User> { new() { Id = 5 } };
        var members = new List<UserGroupMember>();

        _mockContext.Setup(c => c.UserGroups).Returns(MockDbSetFactory.CreateMockDbSet(groups).Object);
        _mockContext.Setup(c => c.Users).Returns(MockDbSetFactory.CreateMockDbSet(users).Object);
        _mockContext.Setup(c => c.UserGroupMembers).Returns(MockDbSetFactory.CreateMockDbSet(members).Object);

        var service = CreateService();

        await service.Invoking(s => s.AddUserToGroupAsync(999, 5))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ──────────────────────────────────────────────────────────────────────
    // RemoveUserFromGroupAsync
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveUserFromGroupAsync_CallsLogActionAsync_WhenAuditServiceProvided()
    {
        const int groupId = 3;
        const int userId = 30;

        var members = new List<UserGroupMember>
        {
            new() { Id = 1, UserGroupId = groupId, UserId = userId }
        };
        var groups = new List<UserGroup> { new() { Id = groupId, Name = "Sales" } };

        _mockContext.Setup(c => c.UserGroupMembers).Returns(MockDbSetFactory.CreateMockDbSet(members).Object);
        _mockContext.Setup(c => c.UserGroups).Returns(MockDbSetFactory.CreateMockDbSet(groups).Object);

        _mockAuditLog
            .Setup(a => a.LogActionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        await service.RemoveUserFromGroupAsync(groupId, userId);

        _mockAuditLog.Verify(a => a.LogActionAsync(
            "UserRemovedFromGroup",
            "UserGroup",
            groupId,
            userId,
            It.IsAny<string>(),
            null, null, default),
            Times.Once);
    }

    [Fact]
    public async Task RemoveUserFromGroupAsync_Throws_WhenMemberNotFound()
    {
        var members = new List<UserGroupMember>(); // no members
        _mockContext.Setup(c => c.UserGroupMembers).Returns(MockDbSetFactory.CreateMockDbSet(members).Object);

        var service = CreateService();

        await service.Invoking(s => s.RemoveUserFromGroupAsync(1, 99))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}

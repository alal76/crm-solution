// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.SystemModule.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Controllers;

/// <summary>
/// Unit tests for UserGroups controller functionality.
/// Tests user group API endpoints behavior at the service level.
/// </summary>
public class UserGroupsControllerTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<UserGroupService>> _loggerMock;
    private readonly UserGroupService _userGroupService;

    public UserGroupsControllerTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<UserGroupService>>();
        _userGroupService = new UserGroupService(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllGroups_ReturnsAllUserGroups()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UserGroup { Id = 2, Name = "Viewers", Description = "Viewer role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = groups.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroups).Returns(mockDbSet.Object);

        // Act
        var result = await _userGroupService.GetAllGroupsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGroup_WithValidId_ReturnsGroup()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = groups.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroups).Returns(mockDbSet.Object);

        // Act
        var result = await _userGroupService.GetGroupByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Managers", result.Name);
    }

    [Fact]
    public async Task GetGroupMembers_WithValidGroupId_ReturnsMembers()
    {
        // Arrange
        var groupId = 1;
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { Id = 1, UserId = 1, UserGroupId = groupId, CreatedAt = DateTime.UtcNow },
            new UserGroupMember { Id = 2, UserId = 2, UserGroupId = groupId, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = members.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(mockDbSet.Object);

        // Act
        var result = await _userGroupService.GetGroupMembersAsync(groupId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}

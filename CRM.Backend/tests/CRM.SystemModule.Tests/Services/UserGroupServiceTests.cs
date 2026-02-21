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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for UserGroupService.
/// Tests user group management functionality.
/// </summary>
public class UserGroupServiceTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<UserGroupService>> _loggerMock;
    private readonly UserGroupService _service;

    public UserGroupServiceTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<UserGroupService>>();
        _service = new UserGroupService(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetGroupByIdAsync_WithValidId_ReturnsGroup()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = groups.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroups).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetGroupByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Managers", result.Name);
    }

    [Fact]
    public async Task GetGroupByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = groups.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroups).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetGroupByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllGroupsAsync_ReturnsAllGroups()
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
        var result = await _service.GetAllGroupsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGroupMembersAsync_ReturnsGroupMembers()
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
        var result = await _service.GetGroupMembersAsync(groupId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task IsUserInGroupAsync_WithMember_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var groupId = 1;

        var members = new List<UserGroupMember>
        {
            new UserGroupMember { Id = 1, UserId = userId, UserGroupId = groupId, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = members.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(mockDbSet.Object);

        // Act
        var result = await _service.IsUserInGroupAsync(userId, groupId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserInGroupAsync_WithNonMember_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var groupId = 1;

        var members = new List<UserGroupMember>
        {
            new UserGroupMember { Id = 1, UserId = 1, UserGroupId = groupId, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = members.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(mockDbSet.Object);

        // Act
        var result = await _service.IsUserInGroupAsync(userId, groupId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetActiveGroupsAsync_ReturnsOnlyActiveGroups()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow },
            new UserGroup { Id = 2, Name = "Inactive", Description = "Inactive role", IsActive = false, CreatedAt = DateTime.UtcNow }
        };

        var mockDbSet = groups.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserGroups).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetActiveGroupsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Managers", result.First().Name);
    }
}

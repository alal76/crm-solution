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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.SystemModule.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for RBACService.
/// Tests role-based access control functionality.
/// </summary>
public class RBACServiceTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<IPermissionCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<RBACService>> _loggerMock;
    private readonly RBACService _service;

    public RBACServiceTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _cacheServiceMock = new Mock<IPermissionCacheService>();
        _loggerMock = new Mock<ILogger<RBACService>>();
        _service = new RBACService(_dbContextMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CheckPermissionAsync_WithPermittedUser_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "View.Accounts";

        // Mock cache to return permissions
        var cachedPermissions = new HashSet<string> { "View.Accounts", "Edit.Accounts" };
        _cacheServiceMock.Setup(x => x.GetUserPermissionsFromCacheAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _service.CheckPermissionAsync(userId, permission);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckPermissionAsync_WithDeniedUser_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var permission = "Delete.Accounts";

        // Mock cache to return empty permissions
        var cachedPermissions = new HashSet<string>();
        _cacheServiceMock.Setup(x => x.GetUserPermissionsFromCacheAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Setup Users mock for database fallback
        var users = new List<User>
        {
            new User 
            { 
                Id = userId, 
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        var userMock = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(userMock.Object);

        // Act
        var result = await _service.CheckPermissionAsync(userId, permission);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithCachedPermissions_ReturnsFromCache()
    {
        // Arrange
        var userId = 1;
        var cachedPermissions = new HashSet<string> { "View.Accounts", "Edit.Accounts" };
        _cacheServiceMock.Setup(x => x.GetUserPermissionsFromCacheAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _service.GetUserPermissionsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("View.Accounts", result);
    }

    [Fact]
    public async Task GetUserRolesAsync_ReturnsAllUserRoles()
    {
        // Arrange
        var userId = 1;

        var users = new List<User>
        {
            new User 
            { 
                Id = userId, 
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var userGroups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role" }
        };

        var groupMembers = new List<UserGroupMember>
        {
            new UserGroupMember { UserId = userId, UserGroupId = 1 }
        };

        var userMock = users.CreateMockDbSet();
        var userGroupMock = userGroups.CreateMockDbSet();
        var groupMemberMock = groupMembers.CreateMockDbSet();

        _dbContextMock.Setup(x => x.Users).Returns(userMock.Object);
        _dbContextMock.Setup(x => x.UserGroups).Returns(userGroupMock.Object);
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(groupMemberMock.Object);

        // Act
        var result = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

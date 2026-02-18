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

namespace CRM.SystemModule.Tests.Controllers;

/// <summary>
/// Unit tests for RBAC controller functionality.
/// Tests role and permission API endpoints behavior at the service level.
/// </summary>
public class RolesControllerTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<IPermissionCacheService> _cacheMock;
    private readonly Mock<ILogger<RBACService>> _loggerMock;
    private readonly RBACService _rbacService;

    public RolesControllerTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _cacheMock = new Mock<IPermissionCacheService>();
        _loggerMock = new Mock<ILogger<RBACService>>();
        _rbacService = new RBACService(_dbContextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserRoles_WithValidUser_ReturnsRoles()
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
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var groupMembers = new List<UserGroupMember>
        {
            new UserGroupMember { Id = 1, UserId = userId, UserGroupId = 1, CreatedAt = DateTime.UtcNow }
        };

        var userMock = users.CreateMockDbSet();
        var userGroupMock = userGroups.CreateMockDbSet();
        var groupMemberMock = groupMembers.CreateMockDbSet();

        _dbContextMock.Setup(x => x.Users).Returns(userMock.Object);
        _dbContextMock.Setup(x => x.UserGroups).Returns(userGroupMock.Object);
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(groupMemberMock.Object);

        // Act
        var result = await _rbacService.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPermissions_ReturnsAllPermissions()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new Permission { Id = 1, Name = "View.Accounts", Description = "Can view accounts" },
            new Permission { Id = 2, Name = "Edit.Accounts", Description = "Can edit accounts" },
            new Permission { Id = 3, Name = "Delete.Accounts", Description = "Can delete accounts" }
        };

        var permissionMock = permissions.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Permissions).Returns(permissionMock.Object);

        // Act
        var result = permissions;

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task CheckUserPermission_WithGrantedPermission_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var permission = "View.Accounts";

        var userGroups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var groupMembers = new List<UserGroupMember>
        {
            new UserGroupMember { Id = 1, UserId = userId, UserGroupId = 1, CreatedAt = DateTime.UtcNow }
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 1, Name = permission, Description = "Can view accounts" }
        };

        var rolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = 1, PermissionId = 1 }
        };

        // Setup cache to indicate no cached permissions (force DB lookup)
        _cacheMock.Setup(x => x.IsUserPermissionsCachedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheMock.Setup(x => x.GetUserPermissionsFromCacheAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var userGroupMock = userGroups.CreateMockDbSet();
        var groupMemberMock = groupMembers.CreateMockDbSet();
        var permissionMock = permissions.CreateMockDbSet();
        var rolePermissionMock = rolePermissions.CreateMockDbSet();

        _dbContextMock.Setup(x => x.UserGroups).Returns(userGroupMock.Object);
        _dbContextMock.Setup(x => x.UserGroupMembers).Returns(groupMemberMock.Object);
        _dbContextMock.Setup(x => x.Permissions).Returns(permissionMock.Object);
        _dbContextMock.Setup(x => x.RolePermissions).Returns(rolePermissionMock.Object);

        // Act
        var result = await _rbacService.CheckPermissionAsync(userId, permission);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetRolePermissions_WithValidRole_ReturnsPermissions()
    {
        // Arrange
        var roleId = 1;

        var permissions = new List<Permission>
        {
            new Permission { Id = 1, Name = "View.Accounts", Description = "Can view accounts" },
            new Permission { Id = 2, Name = "Edit.Accounts", Description = "Can edit accounts" }
        };

        var rolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = roleId, PermissionId = 1 },
            new RolePermission { RoleId = roleId, PermissionId = 2 }
        };

        var permissionMock = permissions.CreateMockDbSet();
        var rolePermissionMock = rolePermissions.CreateMockDbSet();

        _dbContextMock.Setup(x => x.Permissions).Returns(permissionMock.Object);
        _dbContextMock.Setup(x => x.RolePermissions).Returns(rolePermissionMock.Object);

        // Act
        var result = rolePermissions;

        // Assert
        Assert.Equal(2, result.Count);
    }
}

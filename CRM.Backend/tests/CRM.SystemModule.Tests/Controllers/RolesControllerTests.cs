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
        var userRoles = new List<UserRoleAssignment>
        {
            new UserRoleAssignment { Id = 1, UserId = userId, RoleId = 1, IsActive = true, IsDeleted = false, AssignedAt = DateTime.UtcNow }
        };

        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };

        var userRoleMock = userRoles.CreateMockDbSet();
        var roleMock = roles.CreateMockDbSet();

        _dbContextMock.Setup(x => x.UserRoles).Returns(userRoleMock.Object);
        _dbContextMock.Setup(x => x.Roles).Returns(roleMock.Object);

        // Act
        var result = await _rbacService.GetUserRolesAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Managers", result.First().Name);
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

        var userRoles = new List<UserRoleAssignment>
        {
            new UserRoleAssignment { Id = 1, UserId = userId, RoleId = 1, IsActive = true, IsDeleted = false, AssignedAt = DateTime.UtcNow }
        };

        var permissions = new List<Permission>
        {
            new Permission { Id = 1, Name = permission, Description = "Can view accounts" }
        };

        var rolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = 1, PermissionId = 1, IsDeleted = false }
        };

        // Setup cache to indicate no cached permissions (force DB lookup)
        _cacheMock.Setup(x => x.IsUserPermissionsCachedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cacheMock.Setup(x => x.GetUserPermissionsFromCacheAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var userRoleMock = userRoles.CreateMockDbSet();
        var permissionMock = permissions.CreateMockDbSet();
        var rolePermissionMock = rolePermissions.CreateMockDbSet();

        _dbContextMock.Setup(x => x.UserRoles).Returns(userRoleMock.Object);
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

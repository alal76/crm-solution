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

        var userRoleAssignments = new List<UserRoleAssignment>
        {
            new UserRoleAssignment { Id = 1, UserId = userId, RoleId = 1, AssignedAt = DateTime.UtcNow, EffectiveFrom = DateTime.UtcNow.AddDays(-1), EffectiveTo = null, Role = new Role { Id = 1, Name = "Managers", Description = "Manager role", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow } }
        };

        var userRoleMock = userRoleAssignments.CreateMockDbSet();
        _dbContextMock.Setup(x => x.UserRoles).Returns(userRoleMock.Object);

        // Act
        var result = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

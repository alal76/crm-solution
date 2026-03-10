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
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for RBACService (TCOV-008).
/// </summary>
public class RBACServiceTests : ServiceTestFixtureBase<RBACService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IPermissionCacheService> _mockCacheService;
    private readonly RBACService _service;

    public RBACServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockCacheService = new Mock<IPermissionCacheService>();
        _service = new RBACService(_mockDbContext.Object, _mockCacheService.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckPermissionAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        _mockCacheService.Setup(c => c.GetUserPermissionsFromCacheAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<string>)new HashSet<string>());
        _mockDbContext.Setup(c => c.Users)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<User>()).Object);

        // Act
        var result = await _service.CheckPermissionAsync(999, "can_read");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ShouldReturnFromCache_WhenCacheHasData()
    {
        // Arrange
        ISet<string> cachedPermissions = new HashSet<string> { "can_read", "can_write" };
        _mockCacheService.Setup(c => c.GetUserPermissionsFromCacheAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPermissions);

        // Act
        var result = await _service.GetUserPermissionsAsync(1);

        // Assert
        result.Should().BeEquivalentTo(cachedPermissions);
        _mockDbContext.Verify(c => c.Users, Times.Never);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ShouldReturnEmpty_WhenUserNotFound()
    {
        // Arrange
        _mockCacheService.Setup(c => c.GetUserPermissionsFromCacheAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<string>)new HashSet<string>());
        _mockDbContext.Setup(c => c.Users)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<User>()).Object);

        // Act
        var result = await _service.GetUserPermissionsAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnEmpty_WhenNoRolesExist()
    {
        // Arrange
        _mockDbContext.Setup(c => c.Roles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Role>()).Object);

        // Act
        var result = await _service.GetAllRolesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnEmpty_WhenNoRolesExist()
    {
        _mockDbContext.Setup(c => c.Roles)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Role>()).Object);

        var result = await _service.GetAllRolesAsync();

        result.Should().BeEmpty();
    }
}

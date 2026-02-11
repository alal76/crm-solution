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

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserGroupService
/// Covers: Group CRUD, permissions, membership, system admin groups
/// </summary>
public class UserGroupServiceTests
{
    private readonly Mock<IRepository<UserGroup>> _mockGroupRepository;
    private readonly Mock<IRepository<UserGroupMember>> _mockMemberRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserGroupService>> _mockLogger;
    private readonly UserGroupService _service;

    public UserGroupServiceTests()
    {
        _mockGroupRepository = new Mock<IRepository<UserGroup>>();
        _mockMemberRepository = new Mock<IRepository<UserGroupMember>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<UserGroupService>>();

        _service = new UserGroupService(
            _mockGroupRepository.Object,
            _mockMemberRepository.Object,
            _mockUserRepository.Object,
            _mockDbContext.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllGroups()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Administrators" },
            new UserGroup { Id = 2, Name = "Sales Team" }
        };

        _mockGroupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(groups);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _mockGroupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<UserGroup>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveGroupsAsync_ReturnsOnlyActiveGroups()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, Name = "Active", IsActive = true },
            new UserGroup { Id = 2, Name = "Inactive", IsActive = false }
        };

        _mockGroupRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroup, bool>>>()))
            .ReturnsAsync(groups.Where(g => g.IsActive).ToList());

        // Act
        var result = await _service.GetActiveGroupsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_ExistingGroup_ReturnsGroup()
    {
        // Arrange
        var group = new UserGroup
        {
            Id = 1,
            Name = "Administrators",
            Description = "System administrators"
        };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Administrators");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingGroup_ReturnsNull()
    {
        // Arrange
        _mockGroupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((UserGroup?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithMembersAsync_ReturnsGroupWithMembers()
    {
        // Arrange
        var group = new UserGroup
        {
            Id = 1,
            Name = "Sales Team",
            UserGroupMembers = new List<UserGroupMember>
            {
                new UserGroupMember { UserId = 1, User = new User { Username = "user1" } },
                new UserGroupMember { UserId = 2, User = new User { Username = "user2" } }
            }
        };

        _mockGroupRepository.Setup(r => r.GetByIdWithIncludesAsync(1, It.IsAny<string[]>()))
            .ReturnsAsync(group);

        // Act
        var result = await _service.GetByIdWithMembersAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.UserGroupMembers.Should().HaveCount(2);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidGroup_ReturnsCreatedGroup()
    {
        // Arrange
        var createDto = new CreateUserGroupDto
        {
            Name = "New Group",
            Description = "Test group"
        };

        _mockGroupRepository.Setup(r => r.AddAsync(It.IsAny<UserGroup>()))
            .ReturnsAsync((UserGroup g) => { g.Id = 1; return g; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Group");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var createDto = new CreateUserGroupDto { Name = "Existing Group" };

        _mockGroupRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroup, bool>>>()))
            .ReturnsAsync(new List<UserGroup> { new UserGroup { Name = "Existing Group" } });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_SystemAdminGroup_SetsSystemAdminFlag()
    {
        // Arrange
        var createDto = new CreateUserGroupDto
        {
            Name = "SysAdmin",
            IsSystemAdmin = true
        };

        _mockGroupRepository.Setup(r => r.AddAsync(It.IsAny<UserGroup>()))
            .ReturnsAsync((UserGroup g) => { g.Id = 1; return g; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.IsSystemAdmin.Should().BeTrue();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidGroup_ReturnsUpdatedGroup()
    {
        // Arrange
        var existingGroup = new UserGroup { Id = 1, Name = "Old Name" };
        var updateDto = new UpdateUserGroupDto { Id = 1, Name = "New Name" };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingGroup);

        _mockGroupRepository.Setup(r => r.UpdateAsync(It.IsAny<UserGroup>()))
            .ReturnsAsync((UserGroup g) => g);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingGroup_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateUserGroupDto { Id = 999 };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((UserGroup?)null);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePermissionsAsync_ValidGroup_UpdatesPermissions()
    {
        // Arrange
        var group = new UserGroup { Id = 1, CanCreateCustomers = false };
        var permissions = new UpdateGroupPermissionsDto
        {
            CanCreateCustomers = true,
            CanEditCustomers = true
        };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        _mockGroupRepository.Setup(r => r.UpdateAsync(It.IsAny<UserGroup>()))
            .ReturnsAsync((UserGroup g) => g);

        // Act
        var result = await _service.UpdatePermissionsAsync(1, permissions);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingGroup_ReturnsTrue()
    {
        // Arrange
        var group = new UserGroup { Id = 1, IsSystemAdmin = false };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        _mockGroupRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingGroup_ReturnsFalse()
    {
        // Arrange
        _mockGroupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((UserGroup?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SystemAdminGroup_ThrowsException()
    {
        // Arrange
        var group = new UserGroup { Id = 1, IsSystemAdmin = true };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_DefaultGroup_ThrowsException()
    {
        // Arrange
        var group = new UserGroup { Id = 1, IsDefault = true };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(1));
    }

    #endregion

    #region Membership Tests

    [Fact]
    public async Task AddMemberAsync_ValidIds_AddsMember()
    {
        // Arrange
        var group = new UserGroup { Id = 1, Name = "Sales" };
        var user = new User { Id = 1, Username = "user1" };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);
        _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(new List<UserGroupMember>());

        _mockMemberRepository.Setup(r => r.AddAsync(It.IsAny<UserGroupMember>()))
            .ReturnsAsync((UserGroupMember m) => { m.Id = 1; return m; });

        // Act
        var result = await _service.AddMemberAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AddMemberAsync_AlreadyMember_ReturnsFalse()
    {
        // Arrange
        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(new List<UserGroupMember> { new UserGroupMember { UserGroupId = 1, UserId = 1 } });

        // Act
        var result = await _service.AddMemberAsync(1, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveMemberAsync_ExistingMember_RemovesMember()
    {
        // Arrange
        var member = new UserGroupMember { Id = 1, UserGroupId = 1, UserId = 1 };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(new List<UserGroupMember> { member });

        _mockMemberRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveMemberAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveMemberAsync_NotAMember_ReturnsFalse()
    {
        // Arrange
        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(new List<UserGroupMember>());

        // Act
        var result = await _service.RemoveMemberAsync(1, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsGroupMembers()
    {
        // Arrange
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { UserId = 1, User = new User { Username = "user1" } },
            new UserGroupMember { UserId = 2, User = new User { Username = "user2" } }
        };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.GetMembersAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserGroupsAsync_ReturnsUserGroups()
    {
        // Arrange
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { UserGroup = new UserGroup { Name = "Sales" } },
            new UserGroupMember { UserGroup = new UserGroup { Name = "Marketing" } }
        };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.GetUserGroupsAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task BulkAddMembersAsync_ValidIds_AddsAllMembers()
    {
        // Arrange
        var group = new UserGroup { Id = 1 };
        var userIds = new List<int> { 1, 2, 3 };

        _mockGroupRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(new List<UserGroupMember>());

        _mockMemberRepository.Setup(r => r.AddAsync(It.IsAny<UserGroupMember>()))
            .ReturnsAsync((UserGroupMember m) => m);

        // Act
        var result = await _service.BulkAddMembersAsync(1, userIds);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Permission Check Tests

    [Fact]
    public async Task CanUserAccessAsync_SystemAdmin_ReturnsTrue()
    {
        // Arrange
        var group = new UserGroup { Id = 1, IsSystemAdmin = true };
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { UserId = 1, UserGroup = group }
        };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.CanUserAccessAsync(1, "AnyPermission");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserAccessAsync_HasPermission_ReturnsTrue()
    {
        // Arrange
        var group = new UserGroup
        {
            Id = 1,
            IsSystemAdmin = false,
            CanCreateCustomers = true
        };
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { UserId = 1, UserGroup = group }
        };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.CanUserAccessAsync(1, "CanCreateCustomers");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserAccessAsync_NoPermission_ReturnsFalse()
    {
        // Arrange
        var group = new UserGroup
        {
            Id = 1,
            IsSystemAdmin = false,
            CanCreateCustomers = false
        };
        var members = new List<UserGroupMember>
        {
            new UserGroupMember { UserId = 1, UserGroup = group }
        };

        _mockMemberRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserGroupMember, bool>>>()))
            .ReturnsAsync(members);

        // Act
        var result = await _service.CanUserAccessAsync(1, "CanCreateCustomers");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var groups = new List<UserGroup>
        {
            new UserGroup { Id = 1, IsActive = true },
            new UserGroup { Id = 2, IsActive = false },
            new UserGroup { Id = 3, IsActive = true }
        };

        _mockGroupRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(groups);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.TotalGroups.Should().Be(3);
        result.ActiveGroups.Should().Be(2);
    }

    #endregion
}

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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserGroupService.
/// Uses ICrmDbContext mock pattern with MockDbSetFactory.
/// </summary>
public class UserGroupServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<UserGroupService>> _mockLogger;
    private readonly UserGroupService _service;

    // Backing lists for mock DbSets
    private readonly List<UserGroup> _groups;
    private readonly List<UserGroupMember> _members;
    private readonly List<User> _users;

    public UserGroupServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<UserGroupService>>();

        _groups = new List<UserGroup>();
        _members = new List<UserGroupMember>();
        _users = new List<User>();

        SetupDbSets();

        _service = new UserGroupService(_mockContext.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Rebuilds the mock DbSets from the current backing lists.
    /// Call this after modifying the backing lists within a test.
    /// </summary>
    private void SetupDbSets()
    {
        var mockGroupSet = MockDbSetFactory.CreateMockDbSet(_groups);
        mockGroupSet.Setup(m => m.Remove(It.IsAny<UserGroup>()))
            .Callback<UserGroup>(g => _groups.Remove(g));

        var mockMemberSet = MockDbSetFactory.CreateMockDbSet(_members);
        mockMemberSet.Setup(m => m.Remove(It.IsAny<UserGroupMember>()))
            .Callback<UserGroupMember>(m => _members.Remove(m));

        var mockUserSet = MockDbSetFactory.CreateMockDbSet(_users);

        _mockContext.Setup(c => c.UserGroups).Returns(mockGroupSet.Object);
        _mockContext.Setup(c => c.UserGroupMembers).Returns(mockMemberSet.Object);
        _mockContext.Setup(c => c.Users).Returns(mockUserSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #region Helpers

    private static UserGroup CreateTestGroup(int id, string name = "Test Group", bool isActive = true)
    {
        return new UserGroup
        {
            Id = id,
            Name = name,
            Description = $"Description for {name}",
            IsActive = isActive,
            IsDefault = false,
            IsSystemAdmin = false,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow,
            Members = new List<UserGroupMember>()
        };
    }

    private static User CreateTestUser(int id, string firstName = "Test", string lastName = "User")
    {
        return new User
        {
            Id = id,
            Username = $"{firstName.ToLower()}.{lastName.ToLower()}",
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@test.com",
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = "hashed",
            IsActive = true,
            Role = (int)UserRole.Sales
        };
    }

    private static CreateUserGroupRequest CreateTestRequest(string name = "New Group")
    {
        return new CreateUserGroupRequest
        {
            Name = name,
            Description = $"Description for {name}",
            IsActive = true,
            IsDefault = false,
            IsSystemAdmin = false,
            CanAccessDashboard = true,
            CanAccessAccounts = true,
            CanAccessCustomers = true, // alias should mirror Accounts
            CanAccessContacts = true
        };
    }

    #endregion

    #region GetAllGroupsAsync

    [Fact]
    public async Task GetAllGroupsAsync_ShouldReturnOnlyActiveGroups()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "Active Group", isActive: true));
        _groups.Add(CreateTestGroup(2, "Inactive Group", isActive: false));
        _groups.Add(CreateTestGroup(3, "Another Active", isActive: true));
        SetupDbSets();

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        var groups = result.ToList();
        groups.Should().HaveCount(2);
        groups.Should().OnlyContain(g => g.IsActive);
    }

    [Fact]
    public async Task GetAllGroupsAsync_ShouldReturnEmptyList_WhenNoActiveGroups()
    {
        // Arrange - list is empty by default

        // Act
        var result = await _service.GetAllGroupsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllGroupsAsync_ShouldReturnDtoWithCorrectProperties()
    {
        // Arrange
        var group = CreateTestGroup(1, "Sales Team");
        group.IsSystemAdmin = true;
        group.HeaderColor = "#FF0000";
        _groups.Add(group);
        SetupDbSets();

        // Act
        var result = (await _service.GetAllGroupsAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.Name.Should().Be("Sales Team");
        dto.IsSystemAdmin.Should().BeTrue();
        dto.HeaderColor.Should().Be("#FF0000");
    }

    #endregion

    #region GetGroupByIdAsync

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnGroup_WhenExists()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "Sales"));
        _groups.Add(CreateTestGroup(2, "Marketing"));
        SetupDbSets();

        // Act
        var result = await _service.GetGroupByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sales");
    }

    [Fact]
    public async Task GetGroupByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange - empty list

        // Act
        var result = await _service.GetGroupByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateGroupAsync

    [Fact]
    public async Task CreateGroupAsync_ShouldCreateGroup_WhenNameIsUnique()
    {
        // Arrange
        var request = CreateTestRequest("New Group");

        // Act
        var result = await _service.CreateGroupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Group");
        _groups.Should().HaveCount(1);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldThrowInvalidOperation_WhenNameExists()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "Existing Group"));
        SetupDbSets();
        var request = CreateTestRequest("Existing Group");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateGroupAsync(request));
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldSetPermissionsFromRequest()
    {
        // Arrange
        var request = CreateTestRequest("Permissions Group");
        request.CanAccessDashboard = true;
        request.CanAccessCustomers = false;
        request.IsSystemAdmin = true;

        // Act
        var result = await _service.CreateGroupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CanAccessDashboard.Should().BeTrue();
        result.CanAccessAccounts.Should().BeFalse();
        result.CanAccessCustomers.Should().BeFalse();
        result.IsSystemAdmin.Should().BeTrue();
    }

    #endregion

    #region UpdateGroupAsync

    [Fact]
    public async Task UpdateGroupAsync_ShouldUpdateGroup_WhenExists()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "Old Name"));
        SetupDbSets();
        var request = CreateTestRequest("Updated Name");

        // Act
        var result = await _service.UpdateGroupAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupAsync_ShouldThrowKeyNotFound_WhenGroupDoesNotExist()
    {
        // Arrange
        var request = CreateTestRequest("Doesn't Matter");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateGroupAsync(999, request));
    }

    #endregion

    #region DeleteGroupAsync

    [Fact]
    public async Task DeleteGroupAsync_ShouldDeleteGroup_WhenExists()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "To Delete"));
        SetupDbSets();

        // Act
        await _service.DeleteGroupAsync(1);

        // Assert
        _groups.Should().BeEmpty();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupAsync_ShouldThrowKeyNotFound_WhenGroupDoesNotExist()
    {
        // Arrange - empty list

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteGroupAsync(999));
    }

    #endregion

    #region GetGroupMembersAsync

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnMembers_ForGivenGroup()
    {
        // Arrange
        var user1 = CreateTestUser(1, "Alice", "Smith");
        var user2 = CreateTestUser(2, "Bob", "Jones");
        _users.AddRange(new[] { user1, user2 });

        _members.Add(new UserGroupMember { Id = 1, UserGroupId = 10, UserId = 1, AddedAt = DateTime.UtcNow, User = user1 });
        _members.Add(new UserGroupMember { Id = 2, UserGroupId = 10, UserId = 2, AddedAt = DateTime.UtcNow, User = user2 });
        _members.Add(new UserGroupMember { Id = 3, UserGroupId = 20, UserId = 1, AddedAt = DateTime.UtcNow, User = user1 }); // different group
        SetupDbSets();

        // Act
        var result = (await _service.GetGroupMembersAsync(10)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Email == "alice.smith@test.com");
        result.Should().Contain(m => m.Email == "bob.jones@test.com");
    }

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnEmpty_WhenNoMembers()
    {
        // Arrange - no members

        // Act
        var result = await _service.GetGroupMembersAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddUserToGroupAsync

    [Fact]
    public async Task AddUserToGroupAsync_ShouldAddMember_WhenValid()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1, "Target Group"));
        _users.Add(CreateTestUser(10, "New", "Member"));
        SetupDbSets();

        // Act
        await _service.AddUserToGroupAsync(1, 10);

        // Assert
        _members.Should().HaveCount(1);
        _members.First().UserGroupId.Should().Be(1);
        _members.First().UserId.Should().Be(10);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowKeyNotFound_WhenGroupNotFound()
    {
        // Arrange
        _users.Add(CreateTestUser(10));
        SetupDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddUserToGroupAsync(999, 10));
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowKeyNotFound_WhenUserNotFound()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1));
        SetupDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddUserToGroupAsync(1, 999));
    }

    [Fact]
    public async Task AddUserToGroupAsync_ShouldThrowInvalidOperation_WhenAlreadyMember()
    {
        // Arrange
        _groups.Add(CreateTestGroup(1));
        _users.Add(CreateTestUser(10));
        _members.Add(new UserGroupMember { Id = 1, UserGroupId = 1, UserId = 10, AddedAt = DateTime.UtcNow });
        SetupDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddUserToGroupAsync(1, 10));
    }

    #endregion

    #region RemoveUserFromGroupAsync

    [Fact]
    public async Task RemoveUserFromGroupAsync_ShouldRemoveMember_WhenExists()
    {
        // Arrange
        _members.Add(new UserGroupMember { Id = 1, UserGroupId = 1, UserId = 10, AddedAt = DateTime.UtcNow });
        SetupDbSets();

        // Act
        await _service.RemoveUserFromGroupAsync(1, 10);

        // Assert
        _members.Should().BeEmpty();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUserFromGroupAsync_ShouldThrowKeyNotFound_WhenNotMember()
    {
        // Arrange - no members

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RemoveUserFromGroupAsync(1, 999));
    }

    #endregion
}

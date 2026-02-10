// CRM Solution - Customer Relationship Management System
// User Groups Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Api.Hubs;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for UserGroupsController
/// Covers: Group CRUD, permissions, members, roles
/// </summary>
public class UserGroupsControllerTests
{
    private readonly Mock<IUserGroupService> _mockUserGroupService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserGroupsController>> _mockLogger;
    private readonly UserGroupsController _controller;

    public UserGroupsControllerTests()
    {
        _mockUserGroupService = new Mock<IUserGroupService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<UserGroupsController>>();

        _controller = new UserGroupsController(
            _mockUserGroupService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new UserGroupDto { Id = 1, Name = "Administrators", IsActive = true },
            new UserGroupDto { Id = 2, Name = "Sales Team", IsActive = true }
        };

        _mockUserGroupService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroups = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>().Subject;
        returnedGroups.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActive_ReturnsActiveGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new UserGroupDto { Id = 1, Name = "Active Group", IsActive = true }
        };

        _mockUserGroupService.Setup(s => s.GetActiveAsync())
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>();
    }

    [Fact]
    public async Task GetSystemAdminGroups_ReturnsSystemAdminGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new UserGroupDto { Id = 1, Name = "Administrators", IsSystemAdmin = true }
        };

        _mockUserGroupService.Setup(s => s.GetSystemAdminGroupsAsync())
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetSystemAdminGroups();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroups = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>().Subject;
        returnedGroups.Should().HaveCount(1);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingGroup_ReturnsOk()
    {
        // Arrange
        var group = new UserGroupDto
        {
            Id = 1,
            Name = "Administrators",
            Description = "System administrators",
            IsActive = true,
            IsSystemAdmin = true
        };

        _mockUserGroupService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(group);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroup = okResult.Value.Should().BeOfType<UserGroupDto>().Subject;
        returnedGroup.Id.Should().Be(1);
        returnedGroup.Name.Should().Be("Administrators");
    }

    [Fact]
    public async Task GetById_NonExistingGroup_ReturnsNotFound()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserGroupDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidGroup_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateUserGroupDto
        {
            Name = "New Group",
            Description = "A new user group"
        };

        var createdGroup = new UserGroupDto
        {
            Id = 3,
            Name = "New Group",
            Description = "A new user group",
            IsActive = true
        };

        _mockUserGroupService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdGroup);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateUserGroupDto { Name = "Existing Group" };

        _mockUserGroupService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Group with this name already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_WithPermissions_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateUserGroupDto
        {
            Name = "Sales Team",
            CanAccessCustomers = true,
            CanAccessContacts = true,
            CanAccessOpportunities = true,
            CanCreateCustomers = true,
            CanEditCustomers = true
        };

        var createdGroup = new UserGroupDto
        {
            Id = 1,
            Name = "Sales Team",
            CanAccessCustomers = true,
            CanAccessContacts = true
        };

        _mockUserGroupService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdGroup);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidGroup_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateUserGroupDto
        {
            Id = 1,
            Name = "Updated Group",
            Description = "Updated description"
        };

        var updatedGroup = new UserGroupDto
        {
            Id = 1,
            Name = "Updated Group",
            Description = "Updated description"
        };

        _mockUserGroupService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedGroup);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroup = okResult.Value.Should().BeOfType<UserGroupDto>().Subject;
        returnedGroup.Name.Should().Be("Updated Group");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateUserGroupDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingGroup_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateUserGroupDto { Id = 999 };

        _mockUserGroupService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((UserGroupDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingGroup_ReturnsNoContent()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingGroup_ReturnsNotFound()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_GroupWithMembers_ReturnsConflict()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete group with active members"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_SystemAdminGroup_ReturnsForbidden()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot delete system admin group"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Member Management Tests

    [Fact]
    public async Task GetMembers_ReturnsGroupMembers()
    {
        // Arrange
        var members = new List<UserDto>
        {
            new UserDto { Id = 1, Email = "user1@example.com" },
            new UserDto { Id = 2, Email = "user2@example.com" }
        };

        _mockUserGroupService.Setup(s => s.GetMembersAsync(1))
            .ReturnsAsync(members);

        // Act
        var result = await _controller.GetMembers(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedMembers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedMembers.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddMember_ValidMember_ReturnsOk()
    {
        // Arrange
        var addRequest = new AddGroupMemberDto
        {
            GroupId = 1,
            UserId = 5
        };

        _mockUserGroupService.Setup(s => s.AddMemberAsync(1, 5))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddMember(addRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AddMember_AlreadyMember_ReturnsConflict()
    {
        // Arrange
        var addRequest = new AddGroupMemberDto
        {
            GroupId = 1,
            UserId = 5
        };

        _mockUserGroupService.Setup(s => s.AddMemberAsync(1, 5))
            .ThrowsAsync(new InvalidOperationException("User is already a member of this group"));

        // Act
        var result = await _controller.AddMember(addRequest);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task RemoveMember_ValidMember_ReturnsNoContent()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.RemoveMemberAsync(1, 5))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveMember(1, 5);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveMember_NotMember_ReturnsNotFound()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.RemoveMemberAsync(1, 999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveMember(1, 999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task BulkAddMembers_ValidMembers_ReturnsOkWithCount()
    {
        // Arrange
        var userIds = new List<int> { 1, 2, 3 };

        _mockUserGroupService.Setup(s => s.BulkAddMembersAsync(1, userIds))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAddMembers(1, userIds);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { AddedCount = 3 });
    }

    [Fact]
    public async Task BulkRemoveMembers_ValidMembers_ReturnsOkWithCount()
    {
        // Arrange
        var userIds = new List<int> { 1, 2 };

        _mockUserGroupService.Setup(s => s.BulkRemoveMembersAsync(1, userIds))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkRemoveMembers(1, userIds);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { RemovedCount = 2 });
    }

    #endregion

    #region Permissions Tests

    [Fact]
    public async Task GetPermissions_ReturnsGroupPermissions()
    {
        // Arrange
        var permissions = new GroupPermissionsDto
        {
            GroupId = 1,
            CanAccessCustomers = true,
            CanAccessContacts = true,
            CanCreateCustomers = true,
            CanEditCustomers = true,
            CanDeleteCustomers = false
        };

        _mockUserGroupService.Setup(s => s.GetPermissionsAsync(1))
            .ReturnsAsync(permissions);

        // Act
        var result = await _controller.GetPermissions(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPermissions = okResult.Value.Should().BeOfType<GroupPermissionsDto>().Subject;
        returnedPermissions.CanAccessCustomers.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePermissions_ValidPermissions_ReturnsOk()
    {
        // Arrange
        var permissions = new UpdateGroupPermissionsDto
        {
            GroupId = 1,
            CanAccessCustomers = true,
            CanCreateCustomers = true,
            CanDeleteCustomers = false
        };

        _mockUserGroupService.Setup(s => s.UpdatePermissionsAsync(permissions))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePermissions(1, permissions);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetAvailablePermissions_ReturnsAllPermissions()
    {
        // Arrange
        var permissions = new List<PermissionDefinitionDto>
        {
            new PermissionDefinitionDto { Name = "CanAccessCustomers", Category = "Customers" },
            new PermissionDefinitionDto { Name = "CanCreateCustomers", Category = "Customers" }
        };

        _mockUserGroupService.Setup(s => s.GetAvailablePermissionsAsync())
            .ReturnsAsync(permissions);

        // Act
        var result = await _controller.GetAvailablePermissions();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<PermissionDefinitionDto>>();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task Activate_ValidGroup_ReturnsOk()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ValidGroup_ReturnsOk()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_SystemAdminGroup_ReturnsForbidden()
    {
        // Arrange
        _mockUserGroupService.Setup(s => s.DeactivateAsync(1))
            .ThrowsAsync(new UnauthorizedAccessException("Cannot deactivate system admin group"));

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task Clone_ExistingGroup_ReturnsCreatedAtAction()
    {
        // Arrange
        var cloneRequest = new CloneGroupDto
        {
            SourceGroupId = 1,
            NewName = "Cloned Group"
        };

        var clonedGroup = new UserGroupDto
        {
            Id = 2,
            Name = "Cloned Group"
        };

        _mockUserGroupService.Setup(s => s.CloneAsync(1, "Cloned Group"))
            .ReturnsAsync(clonedGroup);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Clone_NonExistingSource_ReturnsNotFound()
    {
        // Arrange
        var cloneRequest = new CloneGroupDto
        {
            SourceGroupId = 999,
            NewName = "Cloned Group"
        };

        _mockUserGroupService.Setup(s => s.CloneAsync(999, "Cloned Group"))
            .ReturnsAsync((UserGroupDto?)null);

        // Act
        var result = await _controller.Clone(cloneRequest);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new UserGroupDto { Id = 1, Name = "Sales Team" }
        };

        _mockUserGroupService.Setup(s => s.SearchAsync("Sales"))
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.Search("Sales");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var searchResults = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>().Subject;
        searchResults.Should().HaveCount(1);
    }

    #endregion
}

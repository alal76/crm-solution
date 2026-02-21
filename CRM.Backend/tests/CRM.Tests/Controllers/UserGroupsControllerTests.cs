// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for UserGroupsController.
/// Tests all CRUD operations and member management endpoints.
/// </summary>
public class UserGroupsControllerTests
{
    private readonly Mock<IUserGroupService> _mockGroupService;
    private readonly Mock<ILogger<UserGroupsController>> _mockLogger;
    private readonly UserGroupsController _controller;

    public UserGroupsControllerTests()
    {
        _mockGroupService = new Mock<IUserGroupService>();
        _mockLogger = new Mock<ILogger<UserGroupsController>>();
        _controller = new UserGroupsController(_mockGroupService.Object, _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithExistingGroups_ReturnsOkWithGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new() { Id = 1, Name = "Admins", Description = "Admin group", IsActive = true, MemberCount = 2 },
            new() { Id = 2, Name = "Sales", Description = "Sales group", IsActive = true, MemberCount = 5 }
        };
        _mockGroupService.Setup(s => s.GetAllGroupsAsync()).ReturnsAsync(groups);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroups = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>().Subject;
        returnedGroups.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoGroups_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockGroupService.Setup(s => s.GetAllGroupsAsync()).ReturnsAsync(new List<UserGroupDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroups = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupDto>>().Subject;
        returnedGroups.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockGroupService.Setup(s => s.GetAllGroupsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithGroup()
    {
        // Arrange
        var group = new UserGroupDto { Id = 1, Name = "Admins", Description = "Admin group", IsActive = true };
        _mockGroupService.Setup(s => s.GetGroupByIdAsync(1)).ReturnsAsync(group);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroup = okResult.Value.Should().BeOfType<UserGroupDto>().Subject;
        returnedGroup.Id.Should().Be(1);
        returnedGroup.Name.Should().Be("Admins");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockGroupService.Setup(s => s.GetGroupByIdAsync(999)).ReturnsAsync((UserGroupDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateUserGroupRequest { Name = "Sales Team", Description = "Sales group" };
        var createdGroup = new UserGroupDto { Id = 1, Name = "Sales Team", Description = "Sales group", IsActive = true };
        _mockGroupService.Setup(s => s.CreateGroupAsync(request)).ReturnsAsync(createdGroup);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UserGroupsController.GetById));
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateUserGroupRequest { Name = "", Description = "Empty name" };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ReturnsOkWithUpdatedGroup()
    {
        // Arrange
        var request = new CreateUserGroupRequest { Name = "Updated Sales", Description = "Updated description" };
        var updatedGroup = new UserGroupDto { Id = 1, Name = "Updated Sales", Description = "Updated description", IsActive = true };
        _mockGroupService.Setup(s => s.UpdateGroupAsync(1, request)).ReturnsAsync(updatedGroup);

        // Act
        var result = await _controller.Update(1, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedGroup = okResult.Value.Should().BeOfType<UserGroupDto>().Subject;
        returnedGroup.Name.Should().Be("Updated Sales");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateUserGroupRequest { Name = "Sales", Description = "Description" };
        _mockGroupService.Setup(s => s.UpdateGroupAsync(999, request)).ReturnsAsync((UserGroupDto?)null);

        // Act
        var result = await _controller.Update(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        _mockGroupService.Setup(s => s.DeleteGroupAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region GetMembers Tests

    [Fact]
    public async Task GetMembers_WithValidGroupId_ReturnsMembers()
    {
        // Arrange
        var members = new List<UserGroupMemberDto>
        {
            new() { Id = 1, UserId = 1, UserEmail = "user1@example.com", GroupId = 1 },
            new() { Id = 2, UserId = 2, UserEmail = "user2@example.com", GroupId = 1 }
        };
        _mockGroupService.Setup(s => s.GetGroupMembersAsync(1)).ReturnsAsync(members);

        // Act
        var result = await _controller.GetMembers(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedMembers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserGroupMemberDto>>().Subject;
        returnedMembers.Should().HaveCount(2);
    }

    #endregion

    #region AddMember Tests

    [Fact]
    public async Task AddMember_WithValidIds_ReturnsOk()
    {
        // Arrange
        _mockGroupService.Setup(s => s.AddUserToGroupAsync(1, 1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddMember(1, 1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region RemoveMember Tests

    [Fact]
    public async Task RemoveMember_WithValidIds_ReturnsOk()
    {
        // Arrange
        _mockGroupService.Setup(s => s.RemoveUserFromGroupAsync(1, 1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveMember(1, 1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}

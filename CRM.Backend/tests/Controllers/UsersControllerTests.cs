// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Users Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for UsersController
/// Covers: CRUD operations, profiles, permissions, groups, password management
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, Username = "john", Email = "john@example.com" },
            new UserDto { Id = 2, Username = "jane", Email = "jane@example.com" }
        };

        _mockUserService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value as IEnumerable<UserDto>;
        returnedUsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithDepartmentFilter_ReturnsFilteredUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, DepartmentId = 1 }
        };

        _mockUserService.Setup(s => s.GetByDepartmentAsync(1))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetByDepartment(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetAll_WithGroupFilter_ReturnsFilteredUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = 1 }
        };

        _mockUserService.Setup(s => s.GetByGroupAsync(1))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetByGroup(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetActiveUsers_ReturnsOnlyActive()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, IsActive = true }
        };

        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetActiveUsers();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingUser_ReturnsOkWithUser()
    {
        // Arrange
        var user = new UserDto { Id = 1, Username = "john", Email = "john@example.com" };

        _mockUserService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value as UserDto;
        returnedUser!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByEmail_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new UserDto { Id = 1, Email = "john@example.com" };

        _mockUserService.Setup(s => s.GetByEmailAsync("john@example.com"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetByEmail("john@example.com");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetByUsername_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new UserDto { Id = 1, Username = "john" };

        _mockUserService.Setup(s => s.GetByUsernameAsync("john"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetByUsername("john");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidUser_ReturnsCreatedWithUser()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "Password123!"
        };

        var createdUser = new UserDto
        {
            Id = 1,
            Username = createDto.Username,
            Email = createDto.Email,
            IsActive = true
        };

        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedUser = createdResult.Value as UserDto;
        returnedUser!.IsActive.Should().BeTrue();
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
    public async Task Create_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "existing@example.com"
        };

        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_DuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "existing",
            Email = "new@example.com"
        };

        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>()))
            .ThrowsAsync(new InvalidOperationException("Username already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "123"
        };

        _mockUserService.Setup(s => s.CreateAsync(It.IsAny<CreateUserDto>()))
            .ThrowsAsync(new ArgumentException("Password does not meet requirements"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidUser_ReturnsOkWithUpdatedUser()
    {
        // Arrange
        var updateDto = new UpdateUserDto
        {
            Id = 1,
            FirstName = "Updated",
            LastName = "User"
        };

        var updatedUser = new UserDto
        {
            Id = 1,
            FirstName = "Updated",
            LastName = "User"
        };

        _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserDto>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateUserDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateUserDto { Id = 999 };

        _mockUserService.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserDto>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task Activate_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_OwnAccount_ReturnsConflict()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeactivateAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot deactivate own account"));

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Lock_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.LockAsync(1, DateTime.Now.AddDays(1)))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Lock(1, DateTime.Now.AddDays(1));

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Unlock_LockedUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.UnlockAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Unlock(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Password Management Tests

    [Fact]
    public async Task ResetPassword_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.AdminResetPasswordAsync(1, "NewPassword123!"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPassword(1, new ResetPasswordRequest { NewPassword = "NewPassword123!" });

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ForcePasswordReset_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.ForcePasswordResetAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForcePasswordReset(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SetPasswordNeverExpires_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetPasswordNeverExpiresAsync(1, true))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetPasswordNeverExpires(1, true);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Group Management Tests

    [Fact]
    public async Task AddToGroup_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.AddToGroupAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddToGroup(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RemoveFromGroup_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.RemoveFromGroupAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFromGroup(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetGroups_ValidUser_ReturnsGroups()
    {
        // Arrange
        var groups = new List<UserGroupDto>
        {
            new UserGroupDto { Id = 1, Name = "Admins" },
            new UserGroupDto { Id = 2, Name = "Sales" }
        };

        _mockUserService.Setup(s => s.GetUserGroupsAsync(1))
            .ReturnsAsync(groups);

        // Act
        var result = await _controller.GetGroups(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task SetPrimaryGroup_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetPrimaryGroupAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetPrimaryGroup(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetProfile_ValidUser_ReturnsProfile()
    {
        // Arrange
        var profile = new UserProfileDto
        {
            UserId = 1,
            Theme = "dark",
            Language = "en"
        };

        _mockUserService.Setup(s => s.GetProfileAsync(1))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.GetProfile(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateUserProfileDto
        {
            Theme = "light",
            Language = "es"
        };

        _mockUserService.Setup(s => s.UpdateProfileAsync(1, updateDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProfile(1, updateDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task UploadPhoto_ValidFile_ReturnsOk()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1000);
        file.Setup(f => f.ContentType).Returns("image/jpeg");

        _mockUserService.Setup(s => s.UploadPhotoAsync(1, It.IsAny<byte[]>()))
            .ReturnsAsync("https://example.com/photo.jpg");

        // Act
        var result = await _controller.UploadPhoto(1, file.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeletePhoto_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeletePhotoAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePhoto(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Two-Factor Authentication Tests

    [Fact]
    public async Task Enable2FA_ValidUser_ReturnsSetupInfo()
    {
        // Arrange
        var setupInfo = new TwoFactorSetupDto
        {
            Secret = "ABCD1234",
            QrCodeUrl = "otpauth://..."
        };

        _mockUserService.Setup(s => s.Enable2FAAsync(1))
            .ReturnsAsync(setupInfo);

        // Act
        var result = await _controller.Enable2FA(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Disable2FA_ValidUser_ReturnsOk()
    {
        // Arrange
        _mockUserService.Setup(s => s.Disable2FAAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Disable2FA(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GenerateBackupCodes_ValidUser_ReturnsCodes()
    {
        // Arrange
        var codes = new List<string> { "CODE1", "CODE2", "CODE3" };

        _mockUserService.Setup(s => s.GenerateBackupCodesAsync(1))
            .ReturnsAsync(codes);

        // Act
        var result = await _controller.GenerateBackupCodes(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Activity & Audit Tests

    [Fact]
    public async Task GetLoginHistory_ValidUser_ReturnsHistory()
    {
        // Arrange
        var history = new List<LoginHistoryDto>
        {
            new LoginHistoryDto { LoginTime = DateTime.Today, IPAddress = "192.168.1.1" }
        };

        _mockUserService.Setup(s => s.GetLoginHistoryAsync(1))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetLoginHistory(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetActivityLog_ValidUser_ReturnsActivities()
    {
        // Arrange
        var activities = new List<UserActivityDto>
        {
            new UserActivityDto { Action = "Login", Timestamp = DateTime.Now }
        };

        _mockUserService.Setup(s => s.GetActivityLogAsync(1))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetActivityLog(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { Id = 1, Username = "john" }
        };

        _mockUserService.Setup(s => s.SearchAsync("john"))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.Search("john");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkActivate_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockUserService.Setup(s => s.BulkActivateAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkActivate(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkDeactivate_ValidIds_ReturnsCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockUserService.Setup(s => s.BulkDeactivateAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDeactivate(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkAddToGroup_ValidRequest_ReturnsCount()
    {
        // Arrange
        var request = new BulkAddToGroupRequest
        {
            UserIds = new List<int> { 1, 2, 3 },
            GroupId = 1
        };

        _mockUserService.Setup(s => s.BulkAddToGroupAsync(request.UserIds, request.GroupId))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkAddToGroup(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingUser_ReturnsNoContent()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_OwnAccount_ReturnsConflict()
    {
        // Arrange
        _mockUserService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Cannot delete own account"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion
}

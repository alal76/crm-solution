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
using BCrypt.Net;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserService.
/// Tests all CRUD operations, password management, and profile operations.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _service;
    private readonly List<User> _users;

    public UserServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _users = new List<User>();
        _service = new UserService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupMockDbSet()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(_users);
        _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
    }

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act
        var result = await _service.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.Role.Should().Be("Sales");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        SetupMockDbSet();

        // Act
        var result = await _service.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Jane",
            LastName = "Smith",
            PasswordHash = "hash",
            Role = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act
        var result = await _service.GetUserByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        SetupMockDbSet();

        // Act
        var result = await _service.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_WithMultipleUsers_ReturnsAllUsers()
    {
        // Arrange
        _users.AddRange(new[]
        {
            new User { Id = 1, Email = "user1@example.com", Username = "user1", FirstName = "User", LastName = "One", Role = 2, IsActive = true, PasswordHash = "hash", CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "user2@example.com", Username = "user2", FirstName = "User", LastName = "Two", Role = 2, IsActive = true, PasswordHash = "hash", CreatedAt = DateTime.UtcNow },
            new User { Id = 3, Email = "user3@example.com", Username = "user3", FirstName = "User", LastName = "Three", Role = 2, IsActive = true, PasswordHash = "hash", CreatedAt = DateTime.UtcNow }
        });
        SetupMockDbSet();

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Email == "user1@example.com");
        result.Should().Contain(u => u.Email == "user2@example.com");
        result.Should().Contain(u => u.Email == "user3@example.com");
    }

    [Fact]
    public async Task GetAllUsersAsync_WithNoUsers_ReturnsEmptyList()
    {
        // Arrange
        SetupMockDbSet();

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        SetupMockDbSet();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateUserAsync("newuser@example.com", "New", "User", "Password@123");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("User");
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "existing@example.com",
            Username = "existing",
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(existingUser);
        SetupMockDbSet();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateUserAsync("existing@example.com", "New", "User", "Password@123"));
    }

    #endregion

    #region CreateUserWithoutPasswordAsync Tests

    [Fact]
    public async Task CreateUserWithoutPasswordAsync_WithValidData_CreatesUserWithoutPassword()
    {
        // Arrange
        SetupMockDbSet();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateUserWithoutPasswordAsync("newuser@example.com", "New", "User");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var updateDto = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Jane",
            LastName = "Smith",
            Role = "Manager",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.UpdateUserAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithValidId_SoftDeletesUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.DeleteUserAsync(1);

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    #endregion

    #region VerifyPasswordAsync Tests

    [Fact]
    public async Task VerifyPasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "CorrectPassword@123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hash,
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act
        var result = await _service.VerifyPasswordAsync(1, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "CorrectPassword@123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hash,
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act
        var result = await _service.VerifyPasswordAsync(1, "WrongPassword@123");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ChangesPassword()
    {
        // Arrange
        var currentPassword = "OldPassword@123";
        var newPassword = "NewPassword@456";
        var hash = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hash,
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.ChangePasswordAsync(1, currentPassword, newPassword);

        // Assert
        user.PasswordHash.Should().NotBe(hash);
        BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ThrowsException()
    {
        // Arrange
        var currentPassword = "OldPassword@123";
        var hash = BCrypt.Net.BCrypt.HashPassword(currentPassword);
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hash,
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.ChangePasswordAsync(1, "WrongPassword@123", "NewPassword@456"));
    }

    #endregion

    #region GetUserEntityByIdAsync Tests

    [Fact]
    public async Task GetUserEntityByIdAsync_WithValidId_ReturnsUserEntity()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash",
            Role = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(user);
        SetupMockDbSet();

        // Act
        var result = await _service.GetUserEntityByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Email.Should().Be("test@example.com");
    }

    #endregion
}

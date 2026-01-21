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
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserService
/// Tests cover user CRUD operations, profile management, and department assignment
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IRepository<User>> _mockUserRepo;
    private readonly Mock<IRepository<Department>> _mockDepartmentRepo;
    private readonly Mock<IRepository<UserProfile>> _mockUserProfileRepo;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IRepository<User>>();
        _mockDepartmentRepo = new Mock<IRepository<Department>>();
        _mockUserProfileRepo = new Mock<IRepository<UserProfile>>();
        
        _service = new UserService(
            _mockUserRepo.Object,
            _mockDepartmentRepo.Object,
            _mockUserProfileRepo.Object);
    }

    #region GetUserByIdAsync Tests

    /// <summary>
    /// Verifies that GetUserByIdAsync returns user when found
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Username.Should().Be("testuser");
    }

    /// <summary>
    /// Verifies that GetUserByIdAsync returns null for non-existent user
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotExists_ReturnsNull()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllUsersAsync Tests

    /// <summary>
    /// Verifies that GetAllUsersAsync returns all active users
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllActiveUsers()
    {
        // Arrange
        var deletedUser = CreateTestUser(3, "user3");
        deletedUser.IsDeleted = true;
        
        var users = new List<User>
        {
            CreateTestUser(1, "user1"),
            CreateTestUser(2, "user2"),
            deletedUser
        };
        
        _mockUserRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(u => !u.IsDeleted).Should().BeTrue();
    }

    #endregion

    #region GetUserByUsernameAsync Tests

    /// <summary>
    /// Verifies that GetUserByUsernameAsync returns user by username
    /// </summary>
    [Fact]
    public async Task GetUserByUsernameAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _service.GetUserByUsernameAsync("testuser");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    #endregion

    #region CreateUserAsync Tests

    /// <summary>
    /// Verifies that CreateUserAsync creates a new user with hashed password
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var user = CreateTestUser(0, "newuser");
        user.PasswordHash = "plainpassword";

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync(new List<User>());
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask)
            .Callback<User>(u => u.Id = 1);
        _mockUserRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    /// <summary>
    /// Verifies that CreateUserAsync rejects duplicate username
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithDuplicateUsername_ReturnsNull()
    {
        // Arrange
        var existingUser = CreateTestUser(1, "existinguser");
        var newUser = CreateTestUser(0, "existinguser");

        _mockUserRepo.Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync(new List<User> { existingUser });

        // Act
        var result = await _service.CreateUserAsync(newUser);

        // Assert
        result.Should().BeNull();
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region UpdateUserAsync Tests

    /// <summary>
    /// Verifies that UpdateUserAsync updates user correctly
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        var existingUser = CreateTestUser(1, "testuser");
        var updatedUser = CreateTestUser(1, "testuser");
        updatedUser.Email = "newemail@example.com";
        updatedUser.FirstName = "UpdatedFirst";

        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingUser);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateUserAsync(updatedUser);

        // Assert
        result.Should().NotBeNull();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Email == "newemail@example.com")), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    /// <summary>
    /// Verifies that DeleteUserAsync performs soft delete
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_PerformsSoftDelete()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        
        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteUserAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.IsDeleted == true)), Times.Once);
    }

    /// <summary>
    /// Verifies that DeleteUserAsync returns false for non-existent user
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_WhenUserNotExists_ReturnsFalse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.DeleteUserAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Department Assignment Tests

    /// <summary>
    /// Verifies that AssignToDepartmentAsync assigns user to department
    /// </summary>
    [Fact]
    public async Task AssignToDepartmentAsync_WithValidData_AssignsUser()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        var department = new Department { Id = 5, Name = "Engineering" };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockDepartmentRepo.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(department);
        _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AssignToDepartmentAsync(1, 5);

        // Assert
        result.Should().BeTrue();
        _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.DepartmentId == 5)), Times.Once);
    }

    #endregion

    #region Password Management Tests

    /// <summary>
    /// Verifies that ValidatePasswordAsync validates password correctly
    /// </summary>
    [Fact]
    public async Task ValidatePasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        // In real implementation, this would be a BCrypt hash
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");

        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ValidatePasswordAsync(1, "correctpassword");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ValidatePasswordAsync rejects wrong password
    /// </summary>
    [Fact]
    public async Task ValidatePasswordAsync_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");

        _mockUserRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.ValidatePasswordAsync(1, "wrongpassword");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private User CreateTestUser(int id, string username)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = $"{username}@example.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Role = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}

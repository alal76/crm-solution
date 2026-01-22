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
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for UserService
/// Tests cover user CRUD operations using ICrmDbContext and ILogger
/// </summary>
public class UserServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _service;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<UserService>>();
        
        _service = new UserService(_dbContext, _mockLogger.Object);
    }

    #region GetUserByIdAsync Tests

    /// <summary>
    /// Verifies that GetUserByIdAsync returns user when found
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("testuser@example.com");
    }

    /// <summary>
    /// Verifies that GetUserByIdAsync returns null for non-existent user
    /// </summary>
    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserByEmailAsync Tests

    /// <summary>
    /// Verifies that GetUserByEmailAsync returns user when found
    /// </summary>
    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetUserByEmailAsync("testuser@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("testuser@example.com");
        result.Username.Should().Be("testuser");
    }

    /// <summary>
    /// Verifies that GetUserByEmailAsync returns null for non-existent email
    /// </summary>
    [Fact]
    public async Task GetUserByEmailAsync_WhenUserNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllUsersAsync Tests

    /// <summary>
    /// Verifies that GetAllUsersAsync returns all users
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        await _dbContext.Users.AddRangeAsync(
            CreateTestUser(1, "user1"),
            CreateTestUser(2, "user2"),
            CreateTestUser(3, "user3")
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies that GetAllUsersAsync returns empty collection when no users
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsers_ReturnsEmptyCollection()
    {
        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateUserAsync Tests

    /// <summary>
    /// Verifies that CreateUserAsync creates a new user with hashed password
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Act
        var result = await _service.CreateUserAsync(
            "newuser@example.com",
            "New",
            "User",
            "SecurePassword123!");

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("newuser@example.com");
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("User");
        result.IsActive.Should().BeTrue();
        
        // Verify user was created in database
        var createdUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
        createdUser.Should().NotBeNull();
        createdUser!.PasswordHash.Should().NotBe("SecurePassword123!"); // Should be hashed
    }

    /// <summary>
    /// Verifies that CreateUserAsync rejects duplicate email
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        var existingUser = CreateTestUser(1, "existing");
        existingUser.Email = "duplicate@example.com";
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _service.CreateUserAsync(
            "duplicate@example.com",
            "New",
            "User",
            "password");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    /// <summary>
    /// Verifies that CreateUserAsync assigns specified role
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_WithSpecifiedRole_AssignsRole()
    {
        // Act
        var result = await _service.CreateUserAsync(
            "admin@example.com",
            "Admin",
            "User",
            "password",
            roleId: (int)UserRole.Admin);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Admin");
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
        var user = CreateTestUser(1, "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        var updateDto = new UserDto
        {
            Id = 1,
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast",
            IsActive = true,
            Role = "Sales"
        };

        // Act
        var result = await _service.UpdateUserAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("UpdatedFirst");
        result.LastName.Should().Be("UpdatedLast");
    }

    /// <summary>
    /// Verifies that UpdateUserAsync throws when user not found
    /// </summary>
    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var updateDto = new UserDto { FirstName = "Test", LastName = "User", Role = "Sales" };

        // Act
        Func<Task> act = async () => await _service.UpdateUserAsync(999, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteUserAsync Tests

    /// <summary>
    /// Verifies that DeleteUserAsync removes user
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_RemovesUser()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.DeleteUserAsync(1);

        // Assert
        var deletedUser = await _dbContext.Users.FindAsync(1);
        deletedUser.Should().BeNull();
    }

    /// <summary>
    /// Verifies that DeleteUserAsync throws for non-existent user
    /// </summary>
    [Fact]
    public async Task DeleteUserAsync_WhenUserNotExists_ThrowsException()
    {
        // Act
        Func<Task> act = async () => await _service.DeleteUserAsync(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region VerifyPasswordAsync Tests

    /// <summary>
    /// Verifies that VerifyPasswordAsync validates correct password
    /// </summary>
    [Fact]
    public async Task VerifyPasswordAsync_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.VerifyPasswordAsync(1, "correctpassword");

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that VerifyPasswordAsync rejects wrong password
    /// </summary>
    [Fact]
    public async Task VerifyPasswordAsync_WithWrongPassword_ReturnsFalse()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.VerifyPasswordAsync(1, "wrongpassword");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that VerifyPasswordAsync returns false for non-existent user
    /// </summary>
    [Fact]
    public async Task VerifyPasswordAsync_WhenUserNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.VerifyPasswordAsync(999, "anypassword");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ChangePasswordAsync Tests

    /// <summary>
    /// Verifies that ChangePasswordAsync changes password when current password is correct
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ChangesPassword()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("currentpassword");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.ChangePasswordAsync(1, "currentpassword", "newpassword");

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(1);
        BCrypt.Net.BCrypt.Verify("newpassword", updatedUser!.PasswordHash).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ChangePasswordAsync throws when current password is wrong
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithWrongCurrentPassword_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _service.ChangePasswordAsync(1, "wrongpassword", "newpassword");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*incorrect*");
    }

    #endregion

    #region GetUserEntityByIdAsync Tests

    /// <summary>
    /// Verifies that GetUserEntityByIdAsync returns full user entity
    /// </summary>
    [Fact]
    public async Task GetUserEntityByIdAsync_WhenUserExists_ReturnsUserEntity()
    {
        // Arrange
        var user = CreateTestUser(1, "testuser");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetUserEntityByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.PasswordHash.Should().NotBeNullOrEmpty(); // Entity includes password hash
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
            Role = (int)UserRole.Sales,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #endregion
}

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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for UserService.
/// Tests basic CRUD operations for users.
/// </summary>
public class UserServiceTests : ServiceTestFixtureBase<UserService>
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _service = new UserService(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetUserByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test1@example.com",
                Username = "user1",
                FirstName = "Test",
                LastName = "One",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Email = "test2@example.com",
                Username = "user2",
                FirstName = "Test",
                LastName = "Two",
                Role = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task IsUserActiveAsync_WithActiveUser_ReturnsTrue()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.IsUserActiveAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserActiveAsync_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Role = 1,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _service.IsUserActiveAsync(1);

        // Assert
        Assert.False(result);
    }
}

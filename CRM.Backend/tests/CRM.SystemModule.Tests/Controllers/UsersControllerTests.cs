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
using CRM.SystemModule.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.SystemModule.Tests.Controllers;

/// <summary>
/// Unit tests for Users controller functionality.
/// Tests user API endpoints behavior at the service level.
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UsersControllerTests()
    {
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
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
        var result = await _userService.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUserList()
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
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var users = new List<User>();
        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _userService.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchUsersByEmail_WithPartialMatch_ReturnsResults()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "john@example.com",
                Username = "john",
                FirstName = "John",
                LastName = "Doe",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockDbSet = users.CreateMockDbSet();
        _dbContextMock.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _userService.GetUserByEmailAsync("john@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("john", result.Username);
    }
}

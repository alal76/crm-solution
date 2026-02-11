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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for User Repository
/// Covers: User-specific queries, authentication, groups
/// </summary>
public class UserRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<UserEntity>> _mockDbSet;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<UserEntity>>();
        _mockLogger = new Mock<ILogger<UserRepository>>();

        _mockContext.Setup(c => c.Set<UserEntity>()).Returns(_mockDbSet.Object);
        _repository = new UserRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByEmail Tests

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Email = "john@example.com", Username = "john" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("john");
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Email = "John@Example.com", Username = "john" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var users = new List<UserEntity>().AsQueryable();
        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByEmailAsync("notfound@example.com");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByUsername Tests

    [Fact]
    public async Task GetByUsernameAsync_ExistingUsername_ReturnsUser()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Email = "john@example.com", Username = "johndoe" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByUsernameAsync("johndoe");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExisting_ReturnsNull()
    {
        // Arrange
        var users = new List<UserEntity>().AsQueryable();
        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByUsernameAsync("unknown");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Active Users Tests

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsActiveOnly()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, IsActive = true },
            new UserEntity { Id = 2, IsActive = true },
            new UserEntity { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInactiveUsersAsync_ReturnsInactiveOnly()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, IsActive = true },
            new UserEntity { Id = 2, IsActive = false },
            new UserEntity { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetInactiveUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByRole Tests

    [Fact]
    public async Task GetByRoleAsync_HasMatches_ReturnsUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Role = "Admin" },
            new UserEntity { Id = 2, Role = "Admin" },
            new UserEntity { Id = 3, Role = "User" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByRoleAsync("Admin");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdminsAsync_ReturnsAdminUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Role = "Admin" },
            new UserEntity { Id = 2, Role = "SystemAdmin" },
            new UserEntity { Id = 3, Role = "User" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetAdminsAsync();

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region GetByDepartment Tests

    [Fact]
    public async Task GetByDepartmentAsync_HasMatches_ReturnsUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, DepartmentId = 1 },
            new UserEntity { Id = 2, DepartmentId = 1 },
            new UserEntity { Id = 3, DepartmentId = 2 }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByDepartmentAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, FirstName = "John", LastName = "Doe" },
            new UserEntity { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new UserEntity { Id = 3, FirstName = "Bob", LastName = "Smith" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.SearchAsync("Doe");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_ByEmail_ReturnsMatches()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Email = "john@acme.com", FirstName = "John" },
            new UserEntity { Id = 2, Email = "jane@beta.com", FirstName = "Jane" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.SearchAsync("acme");

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task UpdateLastLoginAsync_UpdatesTimestamp()
    {
        // Arrange
        var user = new UserEntity { Id = 1, LastLoginAt = null };
        var users = new List<UserEntity> { user }.AsQueryable();

        SetupMockDbSet(users);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.UpdateLastLoginAsync(1);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task IncrementFailedLoginAsync_IncrementsCounter()
    {
        // Arrange
        var user = new UserEntity { Id = 1, FailedLoginAttempts = 0 };
        var users = new List<UserEntity> { user }.AsQueryable();

        SetupMockDbSet(users);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.IncrementFailedLoginAsync(1);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ResetFailedLoginAsync_ResetsCounter()
    {
        // Arrange
        var user = new UserEntity { Id = 1, FailedLoginAttempts = 5 };
        var users = new List<UserEntity> { user }.AsQueryable();

        SetupMockDbSet(users);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.ResetFailedLoginAsync(1);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetLockedOutUsersAsync_ReturnsLockedUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, LockoutEnd = DateTime.UtcNow.AddMinutes(30) },
            new UserEntity { Id = 2, LockoutEnd = DateTime.UtcNow.AddMinutes(15) },
            new UserEntity { Id = 3, LockoutEnd = null }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetLockedOutUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task GetByRefreshTokenAsync_ExistingToken_ReturnsUser()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, RefreshToken = "valid-token-123" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByRefreshTokenAsync("valid-token-123");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_UpdatesToken()
    {
        // Arrange
        var user = new UserEntity { Id = 1, RefreshToken = "old-token" };
        var users = new List<UserEntity> { user }.AsQueryable();

        SetupMockDbSet(users);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.UpdateRefreshTokenAsync(1, "new-token", DateTime.UtcNow.AddDays(7));

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByRoleAsync_ReturnsRoleCounts()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, Role = "Admin" },
            new UserEntity { Id = 2, Role = "Admin" },
            new UserEntity { Id = 3, Role = "User" }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetCountByRoleAsync();

        // Assert
        result["Admin"].Should().Be(2);
    }

    [Fact]
    public async Task GetActiveUserCountAsync_ReturnsCount()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, IsActive = true },
            new UserEntity { Id = 2, IsActive = true },
            new UserEntity { Id = 3, IsActive = false }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetActiveUserCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetRecentlyActiveAsync_ReturnsRecentlyActive()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, LastLoginAt = DateTime.UtcNow.AddDays(-1) },
            new UserEntity { Id = 2, LastLoginAt = DateTime.UtcNow.AddDays(-5) },
            new UserEntity { Id = 3, LastLoginAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetRecentlyActiveAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Two-Factor Authentication Tests

    [Fact]
    public async Task GetUsersWithTwoFactorAsync_ReturnsTwoFactorEnabled()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, TwoFactorEnabled = true },
            new UserEntity { Id = 2, TwoFactorEnabled = true },
            new UserEntity { Id = 3, TwoFactorEnabled = false }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetUsersWithTwoFactorAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task EnableTwoFactorAsync_EnablesForUser()
    {
        // Arrange
        var user = new UserEntity { Id = 1, TwoFactorEnabled = false };
        var users = new List<UserEntity> { user }.AsQueryable();

        SetupMockDbSet(users);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Act
        await _repository.EnableTwoFactorAsync(1, "SECRET123");

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    #endregion

    #region Group Tests

    [Fact]
    public async Task GetByGroupAsync_ReturnsGroupMembers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, PrimaryGroupId = 1 },
            new UserEntity { Id = 2, PrimaryGroupId = 1 },
            new UserEntity { Id = 3, PrimaryGroupId = 2 }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetByGroupAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Email Verification Tests

    [Fact]
    public async Task GetUnverifiedUsersAsync_ReturnsUnverified()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new UserEntity { Id = 1, EmailVerified = false },
            new UserEntity { Id = 2, EmailVerified = false },
            new UserEntity { Id = 3, EmailVerified = true }
        }.AsQueryable();

        SetupMockDbSet(users);

        // Act
        var result = await _repository.GetUnverifiedUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<UserEntity> data)
    {
        _mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class UserEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }
    public int? DepartmentId { get; set; }
    public int? PrimaryGroupId { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public bool IsDeleted { get; set; }
}

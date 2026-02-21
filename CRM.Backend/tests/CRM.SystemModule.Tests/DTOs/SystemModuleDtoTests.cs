// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using Xunit;

namespace CRM.SystemModule.Tests.DTOs;

/// <summary>
/// Unit tests for System Module DTOs.
/// Tests data transfer object validation and structure.
/// </summary>
public class SystemModuleDtoTests
{
    [Fact]
    public void UserDto_Creation_IsValid()
    {
        // Arrange & Act
        var userDto = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("test@example.com", userDto.Email);
        Assert.Equal("testuser", userDto.Username);
    }

    [Fact]
    public void UserDto_WithAllProperties_IsValid()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Role = "Admin",
            IsActive = true,
            LastLoginDate = DateTime.UtcNow,
            DepartmentId = 1,
            UserProfileId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        Assert.NotNull(userDto);
        Assert.True(userDto.IsActive);
        Assert.NotNull(userDto.LastLoginDate);
    }

    [Fact]
    public void CreateUserDto_WithValidData_IsValid()
    {
        // Arrange & Act
        var createUserDto = new CreateUserDto
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "SecurePassword123",
            FirstName = "New",
            LastName = "User",
            Role = "User"
        };

        // Assert
        Assert.NotNull(createUserDto);
        Assert.NotEmpty(createUserDto.Email);
        Assert.NotEmpty(createUserDto.Password);
    }

    [Fact]
    public void UpdateUserDto_WithValidData_IsValid()
    {
        // Arrange & Act
        var updateUserDto = new UpdateUserDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            IsActive = true
        };

        // Assert
        Assert.NotNull(updateUserDto);
        Assert.Equal("Updated", updateUserDto.FirstName);
    }

    [Fact]
    public void UserGroupDto_Creation_IsValid()
    {
        // Arrange & Act
        var groupDto = new UserGroupDto
        {
            Id = 1,
            Name = "Managers",
            Description = "Manager role",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(groupDto);
        Assert.Equal("Managers", groupDto.Name);
    }

    [Fact]
    public void PermissionDto_Creation_IsValid()
    {
        // Arrange & Act
        var permissionDto = new PermissionDto
        {
            Id = 1,
            Name = "View.Accounts",
            Description = "Can view accounts",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(permissionDto);
        Assert.Equal("View.Accounts", permissionDto.Name);
    }

    [Fact]
    public void LoginRequest_WithValidCredentials_IsValid()
    {
        // Arrange & Act
        var loginDto = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123"
        };

        // Assert
        Assert.NotNull(loginDto);
        Assert.NotEmpty(loginDto.Email);
        Assert.NotEmpty(loginDto.Password);
    }

    [Fact]
    public void AuthResponse_WithValidToken_IsValid()
    {
        // Arrange & Act
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var authResponse = new AuthResponse
        {
            AccessToken = token,
            RefreshToken = "refresh-token-value",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = 1,
            Email = "test@example.com",
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Role = "Admin"
        };

        // Assert
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.AccessToken);
        Assert.Equal("test@example.com", authResponse.Email);
    }

    [Fact]
    public void RefreshTokenRequest_WithValidToken_IsValid()
    {
        // Arrange & Act
        var refreshDto = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        };

        // Assert
        Assert.NotNull(refreshDto);
        Assert.NotEmpty(refreshDto.RefreshToken);
    }

    [Fact]
    public void SystemSettingsDto_Creation_IsValid()
    {
        // Arrange & Act
        var settingsDto = new SystemSettingsDto
        {
            Id = 1,
            CompanyName = "Test Company",
            PrimaryColor = "#6750A4",
            AccountsEnabled = true,
            ContactsEnabled = true,
            LeadsEnabled = true
        };

        // Assert
        Assert.NotNull(settingsDto);
        Assert.Equal("Test Company", settingsDto.CompanyName);
        Assert.True(settingsDto.AccountsEnabled);
    }

    [Fact]
    public void AdminDashboardDto_Creation_IsValid()
    {
        // Arrange & Act
        var dashboardDto = new AdminDashboardDto
        {
            SystemStatistics = new SystemStatisticsDto
            {
                TotalUsers = 100,
                ActiveUsers = 85,
                TotalAccounts = 50,
                TotalContacts = 200
            },
            RefreshedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotNull(dashboardDto);
        Assert.Equal(100, dashboardDto.SystemStatistics.TotalUsers);
        Assert.Equal(85, dashboardDto.SystemStatistics.ActiveUsers);
    }
}

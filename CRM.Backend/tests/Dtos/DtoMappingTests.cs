// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// DTO Mapping and Validation Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Dtos;

/// <summary>
/// Comprehensive unit tests for DTOs and mapping
/// </summary>
public class DtoMappingTests
{
    #region CustomerDto Tests

    [Fact]
    public void CustomerDto_CreateFromEntity_MapsCorrectly()
    {
        // Arrange
        var entity = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            Company = "Doe Inc",
            Category = CustomerCategory.Individual
        };

        // Act
        var dto = new CustomerDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            Company = entity.Company
        };

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.FirstName.Should().Be(entity.FirstName);
        dto.Email.Should().Be(entity.Email);
    }

    [Fact]
    public void CustomerDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new CustomerDto();

        // Assert
        dto.Id.Should().Be(0);
    }

    [Fact]
    public void CustomerDto_WithAllFields_SetsCorrectly()
    {
        // Arrange & Act
        var dto = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            Company = "Acme Corp"
        };

        // Assert
        dto.Company.Should().Be("Acme Corp");
    }

    #endregion

    #region LoginRequest Tests

    [Fact]
    public void LoginRequest_Creation()
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Email = "user@company.com",
            Password = "SecurePassword123"
        };

        // Assert
        request.Email.Should().Be("user@company.com");
        request.Password.Should().Be("SecurePassword123");
    }

    [Fact]
    public void LoginRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new LoginRequest();

        // Assert
        request.Email.Should().BeNullOrEmpty();
        request.Password.Should().BeNullOrEmpty();
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void UserDto_Creation()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = 1,
            Username = "jdoe",
            Email = "john@company.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin",
            IsActive = true
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Username.Should().Be("jdoe");
        dto.Role.Should().Be("Admin");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserDto_FromEntity_MapsCorrectly()
    {
        // Arrange
        var entity = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@company.com",
            FirstName = "Admin",
            LastName = "User",
            Role = (int)UserRole.Admin,
            IsActive = true
        };

        // Act
        var dto = new UserDto
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Role = ((UserRole)entity.Role).ToString(),
            IsActive = entity.IsActive
        };

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.Username.Should().Be(entity.Username);
        dto.Role.Should().Be("Admin");
    }

    #endregion

    #region ContactDto Tests

    [Fact]
    public void ContactDto_Creation()
    {
        // Arrange & Act
        var dto = new ContactDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            EmailPrimary = "jane@company.com"
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("Jane");
    }

    [Fact]
    public void ContactDto_DefaultValues()
    {
        // Arrange & Act
        var dto = new ContactDto();

        // Assert
        dto.Id.Should().Be(0);
    }

    #endregion

    #region DepartmentDto Tests

    [Fact]
    public void DepartmentDto_Creation()
    {
        // Arrange & Act
        var dto = new DepartmentDto
        {
            Id = 1,
            Name = "Sales",
            Description = "Sales Department"
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Sales");
        dto.Description.Should().Be("Sales Department");
    }

    [Fact]
    public void DepartmentDto_FromEntity_MapsCorrectly()
    {
        // Arrange
        var entity = new Department
        {
            Id = 1,
            Name = "Marketing",
            Description = "Marketing and Communications"
        };

        // Act
        var dto = new DepartmentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.Name.Should().Be("Marketing");
    }

    #endregion

    #region SystemSettingsDto Tests

    [Fact]
    public void SystemSettingsDto_Creation()
    {
        // Arrange & Act
        var dto = new SystemSettingsDto
        {
            Id = 1,
            CustomersEnabled = true,
            LeadsEnabled = true,
            OpportunitiesEnabled = true,
            ProductsEnabled = true,
            ContactsEnabled = true
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.CustomersEnabled.Should().BeTrue();
        dto.LeadsEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettingsDto_FromEntity_MapsCorrectly()
    {
        // Arrange
        var entity = new SystemSettings
        {
            Id = 1,
            CustomersEnabled = true,
            LeadsEnabled = false,
            OpportunitiesEnabled = true
        };

        // Act
        var dto = new SystemSettingsDto
        {
            Id = entity.Id,
            CustomersEnabled = entity.CustomersEnabled,
            LeadsEnabled = entity.LeadsEnabled,
            OpportunitiesEnabled = entity.OpportunitiesEnabled
        };

        // Assert
        dto.CustomersEnabled.Should().BeTrue();
        dto.LeadsEnabled.Should().BeFalse();
        dto.OpportunitiesEnabled.Should().BeTrue();
    }

    #endregion

    #region Collection Mapping Tests

    [Fact]
    public void CustomerList_ToDto_MapsCorrectly()
    {
        // Arrange
        var entities = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith" },
            new() { Id = 3, FirstName = "Bob", LastName = "Wilson" }
        };

        // Act
        var dtos = entities.Select(e => new CustomerDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName
        }).ToList();

        // Assert
        dtos.Should().HaveCount(3);
        dtos[0].FirstName.Should().Be("John");
        dtos[2].LastName.Should().Be("Wilson");
    }

    #endregion

    #region CreateAccountRequest Tests

    [Fact]
    public void CreateAccountRequest_Creation()
    {
        // Arrange & Act
        var request = new CreateAccountRequest
        {
            Name = "Acme Corp",
            Email = "info@acme.com",
            Phone = "555-1234",
            Website = "https://acme.com"
        };

        // Assert
        request.Name.Should().Be("Acme Corp");
        request.Email.Should().Be("info@acme.com");
        request.Website.Should().Be("https://acme.com");
    }

    [Fact]
    public void CreateAccountRequest_WithAddress()
    {
        // Arrange & Act
        var request = new CreateAccountRequest
        {
            Name = "Test Corp",
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA"
        };

        // Assert
        request.Address.Should().Be("123 Main St");
        request.City.Should().Be("New York");
        request.ZipCode.Should().Be("10001");
    }

    #endregion

    #region UpdateAccountRequest Tests

    [Fact]
    public void UpdateAccountRequest_Creation()
    {
        // Arrange & Act
        var request = new UpdateAccountRequest
        {
            Name = "Updated Corp",
            Email = "new@company.com"
        };

        // Assert
        request.Name.Should().Be("Updated Corp");
        request.Email.Should().Be("new@company.com");
    }

    #endregion

    #region AuthResponse Tests

    [Fact]
    public void AuthResponse_Creation()
    {
        // Arrange & Act
        var response = new AuthResponse
        {
            AccessToken = "jwt_token_here",
            Username = "testuser"
        };

        // Assert
        response.AccessToken.Should().Be("jwt_token_here");
        response.Username.Should().Be("testuser");
    }

    #endregion

    #region RegisterRequest Tests

    [Fact]
    public void RegisterRequest_Creation()
    {
        // Arrange & Act
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@company.com",
            Password = "SecurePass123!"
        };

        // Assert
        request.Username.Should().Be("newuser");
        request.Email.Should().Be("newuser@company.com");
        request.Password.Should().Be("SecurePass123!");
    }

    #endregion
}

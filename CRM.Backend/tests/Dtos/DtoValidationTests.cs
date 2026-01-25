// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CRM.Tests.Dtos;

/// <summary>
/// Comprehensive unit tests for DTOs and Validation
/// 
/// FUNCTIONAL VIEW:
/// - Tests DTO validation rules
/// - Tests required fields and constraints
/// - Tests data transformation
/// 
/// TECHNICAL VIEW:
/// - Uses DataAnnotations validation
/// - Tests model binding scenarios
/// </summary>
public class DtoValidationTests
{
    #region LoginRequest Validation Tests

    [Fact]
    public void LoginRequest_ValidData_PassesValidation()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@example.com",
            Password = "Admin@123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void LoginRequest_EmptyEmail_FailsValidation()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = "Admin@123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginRequest_InvalidEmailFormat_FailsValidation()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "not-an-email",
            Password = "Admin@123"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginRequest_EmptyPassword_FailsValidation()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@example.com",
            Password = ""
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Password"));
    }

    #endregion

    #region CreateCustomerRequest Validation Tests

    [Fact]
    public void CreateCustomerRequest_Individual_ValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Category = "Individual",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateCustomerRequest_Organization_ValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Category = "Organization",
            CompanyName = "Acme Corp",
            Email = "contact@acme.com"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateCustomerRequest_MissingCategory_FailsValidation()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Category = null!,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Category"));
    }

    [Fact]
    public void CreateCustomerRequest_FirstNameTooLong_FailsValidation()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Category = "Individual",
            FirstName = new string('A', 101), // Over 100 char limit
            LastName = "Doe"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("FirstName"));
    }

    #endregion

    #region CreateOpportunityRequest Validation Tests

    [Fact]
    public void CreateOpportunityRequest_ValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateOpportunityRequest
        {
            Name = "Enterprise Deal",
            CustomerId = 1,
            Amount = 50000,
            Stage = "Prospecting",
            CloseDate = DateTime.UtcNow.AddMonths(3)
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateOpportunityRequest_NegativeAmount_FailsValidation()
    {
        // Arrange
        var request = new CreateOpportunityRequest
        {
            Name = "Bad Deal",
            CustomerId = 1,
            Amount = -1000, // Negative not allowed
            Stage = "Prospecting"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Amount"));
    }

    [Fact]
    public void CreateOpportunityRequest_InvalidProbability_FailsValidation()
    {
        // Arrange
        var request = new CreateOpportunityRequest
        {
            Name = "Invalid Prob",
            CustomerId = 1,
            Amount = 10000,
            Probability = 150, // Over 100 not allowed
            Stage = "Prospecting"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Probability"));
    }

    #endregion

    #region CreateProductRequest Validation Tests

    [Fact]
    public void CreateProductRequest_ValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Enterprise License",
            SKU = "ENT-001",
            Price = 999.99m,
            IsActive = true
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateProductRequest_EmptyName_FailsValidation()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            SKU = "TEST-001",
            Price = 100
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Name"));
    }

    [Fact]
    public void CreateProductRequest_NegativePrice_FailsValidation()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Invalid Product",
            SKU = "NEG-001",
            Price = -50
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Price"));
    }

    #endregion

    #region CreateUserRequest Validation Tests

    [Fact]
    public void CreateUserRequest_ValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "SecurePass@123",
            FirstName = "New",
            LastName = "User",
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateUserRequest_ShortPassword_FailsValidation()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "short", // Less than 8 chars
            FirstName = "Test",
            LastName = "User",
            Role = "User"
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("Password"));
    }

    [Fact]
    public void CreateUserRequest_DuplicateEmail_UsernameAllowed()
    {
        // Arrange - Business rule: email must be unique (tested at service level)
        var request1 = new CreateUserRequest
        {
            Email = "same@example.com",
            Username = "user1",
            Password = "Password@123",
            FirstName = "User",
            LastName = "One",
            Role = "User"
        };

        var request2 = new CreateUserRequest
        {
            Email = "different@example.com",
            Username = "user1", // Same username
            Password = "Password@123",
            FirstName = "User",
            LastName = "Two",
            Role = "User"
        };

        // Act - Both pass DTO validation (uniqueness checked at service)
        var results1 = ValidateModel(request1);
        var results2 = ValidateModel(request2);

        // Assert
        results1.Should().BeEmpty();
        results2.Should().BeEmpty();
    }

    #endregion

    #region AddressDto Validation Tests

    [Fact]
    public void AddressDto_ValidUSAddress_PassesValidation()
    {
        // Arrange
        var address = new AddressDto
        {
            Street1 = "123 Main Street",
            City = "Beverly Hills",
            State = "CA",
            PostalCode = "90210",
            Country = "US"
        };

        // Act
        var validationResults = ValidateModel(address);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void AddressDto_MissingRequiredFields_FailsValidation()
    {
        // Arrange
        var address = new AddressDto
        {
            Street1 = "", // Required
            City = "", // Required
            Country = "US"
        };

        // Act
        var validationResults = ValidateModel(address);

        // Assert
        validationResults.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region SystemSettingsDto Validation Tests

    [Fact]
    public void UpdateSystemSettingsRequest_ValidData_PassesValidation()
    {
        // Arrange
        var request = new UpdateSystemSettingsRequest
        {
            CompanyName = "Acme Corporation",
            PrimaryColor = "#1976d2",
            MinPasswordLength = 8,
            MaxLoginAttempts = 5
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void UpdateSystemSettingsRequest_PasswordLengthTooShort_FailsValidation()
    {
        // Arrange
        var request = new UpdateSystemSettingsRequest
        {
            MinPasswordLength = 3 // Too short, minimum should be 6
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("MinPasswordLength"));
    }

    [Fact]
    public void UpdateSystemSettingsRequest_InvalidColorFormat_FailsValidation()
    {
        // Arrange
        var request = new UpdateSystemSettingsRequest
        {
            PrimaryColor = "not-a-color" // Should be hex format
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().ContainSingle(v => v.MemberNames.Contains("PrimaryColor"));
    }

    #endregion

    #region Helper Methods

    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}

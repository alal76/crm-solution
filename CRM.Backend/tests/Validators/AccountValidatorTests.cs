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
using CRM.Core.DTOs;
using CRM.Core.Interfaces;
using CRM.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CRM.Tests.Validators;

/// <summary>
/// Unit tests for Account Validator
/// Covers: Name validation, email validation, business rules
/// </summary>
public class AccountValidatorTests
{
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly AccountValidator _validator;

    public AccountValidatorTests()
    {
        _mockAccountService = new Mock<IAccountService>();
        _validator = new AccountValidator(_mockAccountService.Object);
    }

    #region Name Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidAccount_ReturnsNoErrors()
    {
        // Arrange
        var dto = CreateValidAccountDto();

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_NullName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Company = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Company");
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Company = "";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Company"].Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_WhitespaceName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Company = "   ";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_NameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Company = new string('A', 256);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Company"].Should().Contain(e => e.Contains("255"));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task ValidateAsync_NameTooShort_ReturnsError(string name)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Company = name;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Company"].Should().Contain(e => e.Contains("3 characters"));
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidEmail_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Email = "contact@company.com";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.com")]
    [InlineData("invalid@.com")]
    public async Task ValidateAsync_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Email = email;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task ValidateAsync_NullEmail_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Email = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert - Email is optional
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Email = "existing@company.com";
        _mockAccountService.Setup(s => s.EmailExistsAsync(dto.Email, null))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Email"].Should().Contain(e => e.Contains("already exists"));
    }

    [Fact]
    public async Task ValidateAsync_SameEmailOnUpdate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Id = 1;
        dto.Email = "existing@company.com";
        _mockAccountService.Setup(s => s.EmailExistsAsync(dto.Email, dto.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("+44 20 7946 0958")]
    public async Task ValidateAsync_ValidPhone_ReturnsNoError(string phone)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Phone = phone;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("phone")]
    public async Task ValidateAsync_InvalidPhone_ReturnsError(string phone)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Phone = phone;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Phone");
    }

    #endregion

    #region Website Validation Tests

    [Theory]
    [InlineData("https://www.company.com")]
    [InlineData("http://company.com")]
    [InlineData("https://company.co.uk")]
    public async Task ValidateAsync_ValidWebsite_ReturnsNoError(string website)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Website = website;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://company.com")]
    [InlineData("company.com")]
    public async Task ValidateAsync_InvalidWebsite_ReturnsError(string website)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Website = website;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Website");
    }

    #endregion

    #region Industry Validation Tests

    [Theory]
    [InlineData("Technology")]
    [InlineData("Healthcare")]
    [InlineData("Finance")]
    [InlineData("Manufacturing")]
    public async Task ValidateAsync_ValidIndustry_ReturnsNoError(string industry)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Industry = industry;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidIndustry_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Industry = "InvalidIndustry";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Industry");
    }

    #endregion

    #region Account Type Validation Tests

    [Theory]
    [InlineData("Customer")]
    [InlineData("Prospect")]
    [InlineData("Partner")]
    [InlineData("Vendor")]
    public async Task ValidateAsync_ValidAccountType_ReturnsNoError(string accountType)
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.AccountType = accountType;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Revenue Validation Tests

    [Fact]
    public async Task ValidateAsync_NegativeRevenue_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.AnnualRevenue = -1000;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("AnnualRevenue");
    }

    [Fact]
    public async Task ValidateAsync_ZeroRevenue_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.AnnualRevenue = 0;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Employee Count Validation Tests

    [Fact]
    public async Task ValidateAsync_NegativeEmployeeCount_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.EmployeeCount = -5;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("EmployeeCount");
    }

    #endregion

    #region Owner Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidOwner_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.OwnerId = 1;
        _mockAccountService.Setup(s => s.OwnerExistsAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidOwner_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.OwnerId = 999;
        _mockAccountService.Setup(s => s.OwnerExistsAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("OwnerId");
    }

    #endregion

    #region Parent Account Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidParentAccount_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.ParentAccountId = 2;
        _mockAccountService.Setup(s => s.GetByIdAsync(2))
            .ReturnsAsync(new Account { Id = 2 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_SelfAsParent_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.Id = 1;
        dto.ParentAccountId = 1;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["ParentAccountId"].Should().Contain(e => e.Contains("cannot be its own parent"));
    }

    [Fact]
    public async Task ValidateAsync_NonExistentParent_ReturnsError()
    {
        // Arrange
        var dto = CreateValidAccountDto();
        dto.ParentAccountId = 999;
        _mockAccountService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("ParentAccountId");
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new CreateAccountDto
        {
            Company = "",
            Email = "invalid",
            Phone = "abc",
            Website = "not-url",
            AnnualRevenue = -100
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(4);
    }

    #endregion

    #region Helper Methods

    private CreateAccountDto CreateValidAccountDto()
    {
        return new CreateAccountDto
        {
            Company = "Test Company Inc",
            Email = "test@company.com",
            Phone = "+1-555-123-4567",
            Website = "https://company.com",
            Industry = "Technology",
            AccountType = "Customer",
            AnnualRevenue = 1000000,
            EmployeeCount = 50
        };
    }

    #endregion
}

// Supporting classes
public class AccountValidator
{
    private readonly IAccountService _accountService;
    private static readonly HashSet<string> ValidIndustries = new(StringComparer.OrdinalIgnoreCase)
    {
        "Technology", "Healthcare", "Finance", "Manufacturing", "Retail",
        "Education", "Government", "Energy", "Transportation", "Other"
    };

    public AccountValidator(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<ValidationResult> ValidateAsync(CreateAccountDto dto)
    {
        var result = new ValidationResult();

        // Company name validation
        if (string.IsNullOrWhiteSpace(dto.Company))
        {
            result.AddError("Company", "Company name is required");
        }
        else if (dto.Company.Length < 3)
        {
            result.AddError("Company", "Company name must be at least 3 characters");
        }
        else if (dto.Company.Length > 255)
        {
            result.AddError("Company", "Company name cannot exceed 255 characters");
        }

        // Email validation
        if (!string.IsNullOrEmpty(dto.Email))
        {
            if (!IsValidEmail(dto.Email))
            {
                result.AddError("Email", "Invalid email format");
            }
            else if (await _accountService.EmailExistsAsync(dto.Email, dto.Id))
            {
                result.AddError("Email", "An account with this email already exists");
            }
        }

        // Phone validation
        if (!string.IsNullOrEmpty(dto.Phone) && !IsValidPhone(dto.Phone))
        {
            result.AddError("Phone", "Invalid phone number format");
        }

        // Website validation
        if (!string.IsNullOrEmpty(dto.Website) && !IsValidWebsite(dto.Website))
        {
            result.AddError("Website", "Invalid website URL. Must start with http:// or https://");
        }

        // Industry validation
        if (!string.IsNullOrEmpty(dto.Industry) && !ValidIndustries.Contains(dto.Industry))
        {
            result.AddError("Industry", "Invalid industry value");
        }

        // Revenue validation
        if (dto.AnnualRevenue.HasValue && dto.AnnualRevenue < 0)
        {
            result.AddError("AnnualRevenue", "Annual revenue cannot be negative");
        }

        // Employee count validation
        if (dto.EmployeeCount.HasValue && dto.EmployeeCount < 0)
        {
            result.AddError("EmployeeCount", "Employee count cannot be negative");
        }

        // Owner validation
        if (dto.OwnerId.HasValue && !await _accountService.OwnerExistsAsync(dto.OwnerId.Value))
        {
            result.AddError("OwnerId", "Owner does not exist");
        }

        // Parent account validation
        if (dto.ParentAccountId.HasValue)
        {
            if (dto.Id.HasValue && dto.ParentAccountId == dto.Id)
            {
                result.AddError("ParentAccountId", "Account cannot be its own parent");
            }
            else if (await _accountService.GetByIdAsync(dto.ParentAccountId.Value) == null)
            {
                result.AddError("ParentAccountId", "Parent account does not exist");
            }
        }

        return result;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains(".");
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length >= 7 && digits.Length <= 15;
    }

    private bool IsValidWebsite(string website)
    {
        return Uri.TryCreate(website, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public class CreateAccountDto
{
    public int? Id { get; set; }
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? AccountType { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? EmployeeCount { get; set; }
    public int? OwnerId { get; set; }
    public int? ParentAccountId { get; set; }
}

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, List<string>> Errors { get; } = new();

    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = new List<string>();
        }
        Errors[field].Add(message);
    }
}

public interface IAccountService
{
    Task<bool> EmailExistsAsync(string email, int? excludeId);
    Task<bool> OwnerExistsAsync(int ownerId);
    Task<Account?> GetByIdAsync(int id);
}

public class Account
{
    public int Id { get; set; }
    public string? Company { get; set; }
}

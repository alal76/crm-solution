// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Validators.ContactValidation;

/// <summary>
/// Unit tests for Contact Validator
/// Covers: Name validation, email validation, relationship validation
/// </summary>
public class ContactValidatorTests
{
    private readonly Mock<IContactService> _mockContactService;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly ContactValidator _validator;

    public ContactValidatorTests()
    {
        _mockContactService = new Mock<IContactService>();
        _mockAccountService = new Mock<IAccountService>();
        _validator = new ContactValidator(_mockContactService.Object, _mockAccountService.Object);
    }

    #region First Name Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidContact_ReturnsNoErrors()
    {
        // Arrange
        var dto = CreateValidContactDto();

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_NullFirstName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.FirstName = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("FirstName");
    }

    [Fact]
    public async Task ValidateAsync_EmptyFirstName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.FirstName = "";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["FirstName"].Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_FirstNameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.FirstName = new string('A', 101);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["FirstName"].Should().Contain(e => e.Contains("100"));
    }

    #endregion

    #region Last Name Validation Tests

    [Fact]
    public async Task ValidateAsync_NullLastName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.LastName = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("LastName");
    }

    [Fact]
    public async Task ValidateAsync_LastNameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.LastName = new string('B', 101);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidEmail_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Email = "john.doe@company.com";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@.com")]
    public async Task ValidateAsync_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Email = email;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task ValidateAsync_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Email = "existing@company.com";
        _mockContactService.Setup(s => s.EmailExistsAsync(dto.Email, null))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Email"].Should().Contain(e => e.Contains("exists"));
    }

    [Fact]
    public async Task ValidateAsync_SameEmailOnUpdate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Id = 1;
        dto.Email = "john@company.com";
        _mockContactService.Setup(s => s.EmailExistsAsync(dto.Email, dto.Id))
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
        var dto = CreateValidContactDto();
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
        var dto = CreateValidContactDto();
        dto.Phone = phone;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Phone");
    }

    [Fact]
    public async Task ValidateAsync_NullPhone_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Phone = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Account Relationship Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidAccountId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.AccountId = 1;
        _mockAccountService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new AccountDto { Id = 1 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentAccount_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.AccountId = 999;
        _mockAccountService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("AccountId");
    }

    [Fact]
    public async Task ValidateAsync_NullAccountId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.AccountId = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Job Title Validation Tests

    [Fact]
    public async Task ValidateAsync_JobTitleTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.JobTitle = new string('X', 151);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("JobTitle");
    }

    #endregion

    #region Department Validation Tests

    [Fact]
    public async Task ValidateAsync_DepartmentTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Department = new string('D', 101);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Department");
    }

    #endregion

    #region LinkedIn URL Validation Tests

    [Theory]
    [InlineData("https://www.linkedin.com/in/johndoe")]
    [InlineData("https://linkedin.com/in/johndoe")]
    public async Task ValidateAsync_ValidLinkedIn_ReturnsNoError(string url)
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.LinkedInUrl = url;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://facebook.com/johndoe")]
    [InlineData("not-a-url")]
    [InlineData("https://linked.in/johndoe")]
    public async Task ValidateAsync_InvalidLinkedIn_ReturnsError(string url)
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.LinkedInUrl = url;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("LinkedInUrl");
    }

    #endregion

    #region Reports To Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidReportsTo_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Id = 1;
        dto.ReportsToId = 2;
        _mockContactService.Setup(s => s.GetByIdAsync(2))
            .ReturnsAsync(new ContactDto { Id = 2 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ReportsToSelf_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Id = 1;
        dto.ReportsToId = 1;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["ReportsToId"].Should().Contain(e => e.Contains("cannot report to themselves"));
    }

    [Fact]
    public async Task ValidateAsync_NonExistentReportsTo_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.ReportsToId = 999;
        _mockContactService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((ContactDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("ReportsToId");
    }

    #endregion

    #region Salutation Validation Tests

    [Theory]
    [InlineData("Mr.")]
    [InlineData("Ms.")]
    [InlineData("Mrs.")]
    [InlineData("Dr.")]
    [InlineData("Prof.")]
    public async Task ValidateAsync_ValidSalutation_ReturnsNoError(string salutation)
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.Salutation = salutation;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Birth Date Validation Tests

    [Fact]
    public async Task ValidateAsync_FutureBirthDate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.BirthDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("BirthDate");
    }

    [Fact]
    public async Task ValidateAsync_VeryOldBirthDate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.BirthDate = DateTime.UtcNow.AddYears(-200);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("BirthDate");
    }

    [Fact]
    public async Task ValidateAsync_ValidBirthDate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidContactDto();
        dto.BirthDate = DateTime.UtcNow.AddYears(-30);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new CreateContactDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid",
            Phone = "abc"
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(3);
    }

    #endregion

    #region Helper Methods

    private CreateContactDto CreateValidContactDto()
    {
        return new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@company.com",
            Phone = "+1-555-123-4567",
            JobTitle = "Software Engineer",
            Department = "Engineering"
        };
    }

    #endregion
}

// Supporting classes
public class ContactValidator
{
    private readonly IContactService _contactService;
    private readonly IAccountService _accountService;

    public ContactValidator(IContactService contactService, IAccountService accountService)
    {
        _contactService = contactService;
        _accountService = accountService;
    }

    public async Task<ValidationResult> ValidateAsync(CreateContactDto dto)
    {
        var result = new ValidationResult();

        // First name validation
        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            result.AddError("FirstName", "First name is required");
        }
        else if (dto.FirstName.Length > 100)
        {
            result.AddError("FirstName", "First name cannot exceed 100 characters");
        }

        // Last name validation
        if (string.IsNullOrWhiteSpace(dto.LastName))
        {
            result.AddError("LastName", "Last name is required");
        }
        else if (dto.LastName.Length > 100)
        {
            result.AddError("LastName", "Last name cannot exceed 100 characters");
        }

        // Email validation
        if (!string.IsNullOrEmpty(dto.Email))
        {
            if (!IsValidEmail(dto.Email))
            {
                result.AddError("Email", "Invalid email format");
            }
            else if (await _contactService.EmailExistsAsync(dto.Email, dto.Id))
            {
                result.AddError("Email", "A contact with this email already exists");
            }
        }

        // Phone validation
        if (!string.IsNullOrEmpty(dto.Phone) && !IsValidPhone(dto.Phone))
        {
            result.AddError("Phone", "Invalid phone number format");
        }

        // Account validation
        if (dto.AccountId.HasValue)
        {
            var account = await _accountService.GetByIdAsync(dto.AccountId.Value);
            if (account == null)
            {
                result.AddError("AccountId", "Account does not exist");
            }
        }

        // Job title validation
        if (!string.IsNullOrEmpty(dto.JobTitle) && dto.JobTitle.Length > 150)
        {
            result.AddError("JobTitle", "Job title cannot exceed 150 characters");
        }

        // Department validation
        if (!string.IsNullOrEmpty(dto.Department) && dto.Department.Length > 100)
        {
            result.AddError("Department", "Department cannot exceed 100 characters");
        }

        // LinkedIn validation
        if (!string.IsNullOrEmpty(dto.LinkedInUrl) && !IsValidLinkedInUrl(dto.LinkedInUrl))
        {
            result.AddError("LinkedInUrl", "Invalid LinkedIn URL");
        }

        // Reports to validation
        if (dto.ReportsToId.HasValue)
        {
            if (dto.Id.HasValue && dto.ReportsToId == dto.Id)
            {
                result.AddError("ReportsToId", "Contact cannot report to themselves");
            }
            else
            {
                var reportsTo = await _contactService.GetByIdAsync(dto.ReportsToId.Value);
                if (reportsTo == null)
                {
                    result.AddError("ReportsToId", "Reports to contact does not exist");
                }
            }
        }

        // Birth date validation
        if (dto.BirthDate.HasValue)
        {
            if (dto.BirthDate > DateTime.UtcNow)
            {
                result.AddError("BirthDate", "Birth date cannot be in the future");
            }
            else if (dto.BirthDate < DateTime.UtcNow.AddYears(-150))
            {
                result.AddError("BirthDate", "Birth date is too far in the past");
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

    private bool IsValidLinkedInUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Host.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase);
    }
}

public class CreateContactDto
{
    public int? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? AccountId { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? LinkedInUrl { get; set; }
    public int? ReportsToId { get; set; }
    public string? Salutation { get; set; }
    public DateTime? BirthDate { get; set; }
}

public interface IContactService
{
    Task<bool> EmailExistsAsync(string email, int? excludeId);
    Task<ContactDto?> GetByIdAsync(int id);
}

public class ContactDto
{
    public int Id { get; set; }
}

public class AccountDto
{
    public int Id { get; set; }
}

public interface IAccountService
{
    Task<AccountDto?> GetByIdAsync(int id);
}

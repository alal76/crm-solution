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

namespace CRM.Tests.Validators.UserValidation;

/// <summary>
/// Unit tests for User Validator
/// Covers: Username validation, password validation, email uniqueness
/// </summary>
public class UserValidatorTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IUserGroupService> _mockGroupService;
    private readonly UserValidator _validator;
    private readonly PasswordPolicySettings _passwordPolicy;

    public UserValidatorTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockGroupService = new Mock<IUserGroupService>();
        _passwordPolicy = new PasswordPolicySettings
        {
            MinLength = 8,
            MaxLength = 128,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialChar = true,
            MinSpecialChars = 1,
            DisallowCommonPasswords = true
        };
        _validator = new UserValidator(_mockUserService.Object, _mockGroupService.Object, _passwordPolicy);
    }

    #region Username Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidUser_ReturnsNoErrors()
    {
        // Arrange
        var dto = CreateValidUserDto();

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_NullUsername_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Username");
    }

    [Fact]
    public async Task ValidateAsync_UsernameTooShort_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = "ab";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Username"].Should().Contain(e => e.Contains("3 characters"));
    }

    [Fact]
    public async Task ValidateAsync_UsernameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = new string('a', 51);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Username"].Should().Contain(e => e.Contains("50"));
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user@name")]
    [InlineData("user#name")]
    [InlineData("user$name")]
    public async Task ValidateAsync_UsernameWithInvalidChars_ReturnsError(string username)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = username;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Username");
    }

    [Theory]
    [InlineData("john_doe")]
    [InlineData("john.doe")]
    [InlineData("john-doe")]
    [InlineData("johndoe123")]
    public async Task ValidateAsync_UsernameWithValidChars_ReturnsNoError(string username)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = username;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_DuplicateUsername_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Username = "existinguser";
        _mockUserService.Setup(s => s.UsernameExistsAsync("existinguser", null))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Username"].Should().Contain(e => e.Contains("already taken"));
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidEmail_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidUserDto();
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
        var dto = CreateValidUserDto();
        dto.Email = email;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task ValidateAsync_NullEmail_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Email = null;

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
        var dto = CreateValidUserDto();
        dto.Email = "existing@company.com";
        _mockUserService.Setup(s => s.EmailExistsAsync("existing@company.com", null))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Email"].Should().Contain(e => e.Contains("already registered"));
    }

    #endregion

    #region Password Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidPassword_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "SecureP@ss123";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_PasswordTooShort_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "Pass@1";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("8 characters"));
    }

    [Fact]
    public async Task ValidateAsync_PasswordTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = new string('A', 130) + "@1a";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("128"));
    }

    [Fact]
    public async Task ValidateAsync_PasswordWithoutUppercase_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "securep@ss123";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("uppercase"));
    }

    [Fact]
    public async Task ValidateAsync_PasswordWithoutLowercase_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "SECUREP@SS123";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("lowercase"));
    }

    [Fact]
    public async Task ValidateAsync_PasswordWithoutDigit_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "SecureP@ssword";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public async Task ValidateAsync_PasswordWithoutSpecialChar_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = "SecurePass123";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("special character"));
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("123456789")]
    [InlineData("qwerty")]
    [InlineData("admin")]
    public async Task ValidateAsync_CommonPassword_ReturnsError(string password)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = password + "A@"; // Add complexity to pass other rules

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Password"].Should().Contain(e => e.Contains("common"));
    }

    [Fact]
    public async Task ValidateAsync_NullPasswordOnCreate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Password = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Password");
    }

    [Fact]
    public async Task ValidateAsync_NullPasswordOnUpdate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Id = 1; // Update mode
        dto.Password = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region First/Last Name Validation Tests

    [Fact]
    public async Task ValidateAsync_NullFirstName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.FirstName = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("FirstName");
    }

    [Fact]
    public async Task ValidateAsync_FirstNameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.FirstName = new string('A', 101);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["FirstName"].Should().Contain(e => e.Contains("100"));
    }

    [Fact]
    public async Task ValidateAsync_NullLastName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.LastName = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("LastName");
    }

    #endregion

    #region Role Validation Tests

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("ReadOnly")]
    public async Task ValidateAsync_ValidRole_ReturnsNoError(string role)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Role = role;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidRole_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Role = "InvalidRole";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Role");
    }

    #endregion

    #region Group Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidGroupIds_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.GroupIds = new[] { 1, 2 };
        _mockGroupService.Setup(s => s.GroupExistsAsync(1)).ReturnsAsync(true);
        _mockGroupService.Setup(s => s.GroupExistsAsync(2)).ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentGroup_ReturnsError()
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.GroupIds = new[] { 1, 999 };
        _mockGroupService.Setup(s => s.GroupExistsAsync(1)).ReturnsAsync(true);
        _mockGroupService.Setup(s => s.GroupExistsAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("GroupIds");
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("+1-555-123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("+44 20 7946 0958")]
    public async Task ValidateAsync_ValidPhone_ReturnsNoError(string phone)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Phone = phone;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public async Task ValidateAsync_InvalidPhone_ReturnsError(string phone)
    {
        // Arrange
        var dto = CreateValidUserDto();
        dto.Phone = phone;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Phone");
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "a",
            Email = "invalid",
            Password = "weak",
            FirstName = "",
            LastName = ""
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(4);
    }

    #endregion

    #region Helper Methods

    private CreateUserDto CreateValidUserDto()
    {
        return new CreateUserDto
        {
            Username = "johndoe",
            Email = "john.doe@company.com",
            Password = "SecureP@ss123",
            FirstName = "John",
            LastName = "Doe",
            Role = "User"
        };
    }

    #endregion
}

// Supporting classes
public class UserValidator
{
    private readonly IUserService _userService;
    private readonly IUserGroupService _groupService;
    private readonly PasswordPolicySettings _passwordPolicy;

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "User", "Manager", "ReadOnly"
    };

    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "123456", "qwerty", "admin", "letmein", "welcome",
        "monkey", "dragon", "master", "login"
    };

    public UserValidator(IUserService userService, IUserGroupService groupService, PasswordPolicySettings passwordPolicy)
    {
        _userService = userService;
        _groupService = groupService;
        _passwordPolicy = passwordPolicy;
    }

    public async Task<ValidationResult> ValidateAsync(CreateUserDto dto)
    {
        var result = new ValidationResult();
        var isUpdate = dto.Id.HasValue;

        // Username validation
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            result.AddError("Username", "Username is required");
        }
        else
        {
            if (dto.Username.Length < 3)
                result.AddError("Username", "Username must be at least 3 characters");
            else if (dto.Username.Length > 50)
                result.AddError("Username", "Username cannot exceed 50 characters");

            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Username, @"^[a-zA-Z0-9._-]+$"))
                result.AddError("Username", "Username can only contain letters, numbers, dots, underscores, and hyphens");

            if (await _userService.UsernameExistsAsync(dto.Username, dto.Id))
                result.AddError("Username", "Username is already taken");
        }

        // Email validation
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            result.AddError("Email", "Email is required");
        }
        else
        {
            if (!IsValidEmail(dto.Email))
                result.AddError("Email", "Invalid email format");
            else if (await _userService.EmailExistsAsync(dto.Email, dto.Id))
                result.AddError("Email", "Email is already registered");
        }

        // Password validation (required on create, optional on update)
        if (!isUpdate || !string.IsNullOrEmpty(dto.Password))
        {
            ValidatePassword(dto.Password, result, isUpdate);
        }

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

        // Role validation
        if (!string.IsNullOrEmpty(dto.Role) && !ValidRoles.Contains(dto.Role))
        {
            result.AddError("Role", "Invalid role value");
        }

        // Phone validation
        if (!string.IsNullOrEmpty(dto.Phone) && !IsValidPhone(dto.Phone))
        {
            result.AddError("Phone", "Invalid phone number format");
        }

        // Group validation
        if (dto.GroupIds != null)
        {
            foreach (var groupId in dto.GroupIds)
            {
                if (!await _groupService.GroupExistsAsync(groupId))
                {
                    result.AddError("GroupIds", $"Group with ID {groupId} does not exist");
                    break;
                }
            }
        }

        return result;
    }

    private void ValidatePassword(string? password, ValidationResult result, bool isUpdate)
    {
        if (string.IsNullOrEmpty(password))
        {
            if (!isUpdate)
                result.AddError("Password", "Password is required");
            return;
        }

        if (password.Length < _passwordPolicy.MinLength)
            result.AddError("Password", $"Password must be at least {_passwordPolicy.MinLength} characters");

        if (password.Length > _passwordPolicy.MaxLength)
            result.AddError("Password", $"Password cannot exceed {_passwordPolicy.MaxLength} characters");

        if (_passwordPolicy.RequireUppercase && !password.Any(char.IsUpper))
            result.AddError("Password", "Password must contain at least one uppercase letter");

        if (_passwordPolicy.RequireLowercase && !password.Any(char.IsLower))
            result.AddError("Password", "Password must contain at least one lowercase letter");

        if (_passwordPolicy.RequireDigit && !password.Any(char.IsDigit))
            result.AddError("Password", "Password must contain at least one digit");

        if (_passwordPolicy.RequireSpecialChar)
        {
            var specialChars = password.Count(c => !char.IsLetterOrDigit(c));
            if (specialChars < _passwordPolicy.MinSpecialChars)
                result.AddError("Password", $"Password must contain at least {_passwordPolicy.MinSpecialChars} special character(s)");
        }

        if (_passwordPolicy.DisallowCommonPasswords)
        {
            var lowerPassword = password.ToLower();
            if (CommonPasswords.Any(cp => lowerPassword.Contains(cp)))
                result.AddError("Password", "Password contains a common word or pattern");
        }
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
}

public class PasswordPolicySettings
{
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialChar { get; set; }
    public int MinSpecialChars { get; set; }
    public bool DisallowCommonPasswords { get; set; }
}

public class CreateUserDto
{
    public int? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
    public string? Phone { get; set; }
    public int[]? GroupIds { get; set; }
}

public interface IUserGroupService
{
    Task<bool> GroupExistsAsync(int groupId);
}

public interface IUserService
{
    Task<bool> UsernameExistsAsync(string username, int? excludeId);
    Task<bool> EmailExistsAsync(string email, int? excludeId);
}

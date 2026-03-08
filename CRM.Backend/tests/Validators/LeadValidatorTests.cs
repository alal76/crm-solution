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

namespace CRM.Tests.Validators.LeadValidation;

/// <summary>
/// Unit tests for Lead Validator
/// Covers: Lead info validation, source validation, scoring rules
/// </summary>
public class LeadValidatorTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ICampaignService> _mockCampaignService;
    private readonly LeadValidator _validator;

    public LeadValidatorTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _mockUserService = new Mock<IUserService>();
        _mockCampaignService = new Mock<ICampaignService>();
        _validator = new LeadValidator(
            _mockLeadService.Object,
            _mockUserService.Object,
            _mockCampaignService.Object);
    }

    #region Name Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidLead_ReturnsNoErrors()
    {
        // Arrange
        var dto = CreateValidLeadDto();

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
        var dto = CreateValidLeadDto();
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
        var dto = CreateValidLeadDto();
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
        var dto = CreateValidLeadDto();
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
        var dto = CreateValidLeadDto();
        dto.LastName = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("LastName");
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidEmail_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Email = "lead@company.com";

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
    public async Task ValidateAsync_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var dto = CreateValidLeadDto();
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
        var dto = CreateValidLeadDto();
        dto.Email = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Email"].Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Email = "existing@company.com";
        _mockLeadService.Setup(s => s.EmailExistsAsync(dto.Email, null))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Email"].Should().Contain(e => e.Contains("exists"));
    }

    #endregion

    #region Status Validation Tests

    [Theory]
    [InlineData("New")]
    [InlineData("Contacted")]
    [InlineData("Qualified")]
    [InlineData("Converted")]
    [InlineData("Disqualified")]
    public async Task ValidateAsync_ValidStatus_ReturnsNoError(string status)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = status;

        // Provide required fields for terminal statuses
        if (status == "Converted")
            dto.ConvertedDate = DateTime.UtcNow;
        if (status == "Disqualified")
            dto.DisqualificationReason = "Not a good fit";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidStatus_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = "InvalidStatus";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Status");
    }

    [Fact]
    public async Task ValidateAsync_NullStatus_DefaultsToNew()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue(); // Should accept null as "New" default
    }

    #endregion

    #region Source Validation Tests

    [Theory]
    [InlineData("Web")]
    [InlineData("Referral")]
    [InlineData("Trade Show")]
    [InlineData("Cold Call")]
    [InlineData("Advertisement")]
    [InlineData("Partner")]
    [InlineData("Social Media")]
    public async Task ValidateAsync_ValidSource_ReturnsNoError(string source)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Source = source;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidSource_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Source = "InvalidSource";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Source");
    }

    #endregion

    #region Rating Validation Tests

    [Theory]
    [InlineData("Hot")]
    [InlineData("Warm")]
    [InlineData("Cold")]
    public async Task ValidateAsync_ValidRating_ReturnsNoError(string rating)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Rating = rating;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidRating_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Rating = "SuperHot";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Rating");
    }

    #endregion

    #region Score Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ValidateAsync_ValidScore_ReturnsNoError(int score)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Score = score;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task ValidateAsync_NegativeScore_ReturnsError(int score)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Score = score;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Score");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    public async Task ValidateAsync_ScoreOver100_ReturnsError(int score)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Score = score;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Score"].Should().Contain(e => e.Contains("100"));
    }

    #endregion

    #region Owner Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidOwnerId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.OwnerId = 1;
        _mockUserService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new UserResultDto { Id = 1 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentOwner_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.OwnerId = 999;
        _mockUserService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserResultDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("OwnerId");
    }

    #endregion

    #region Campaign Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidCampaignId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.CampaignId = 1;
        _mockCampaignService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new CampaignResultDto { Id = 1 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentCampaign_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.CampaignId = 999;
        _mockCampaignService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((CampaignResultDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("CampaignId");
    }

    #endregion

    #region Company Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidCompany_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Company = "Acme Corporation";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_CompanyTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Company = new string('A', 256);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Company");
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
        var dto = CreateValidLeadDto();
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
        var dto = CreateValidLeadDto();
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
    public async Task ValidateAsync_ValidWebsite_ReturnsNoError(string website)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Website = website;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("company.com")]
    public async Task ValidateAsync_InvalidWebsite_ReturnsError(string website)
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Website = website;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Website");
    }

    #endregion

    #region Conversion Validation Tests

    [Fact]
    public async Task ValidateAsync_ConvertedLeadRequiresConversionDate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = "Converted";
        dto.ConvertedDate = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("ConvertedDate");
    }

    [Fact]
    public async Task ValidateAsync_ConvertedLeadWithValidDate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = "Converted";
        dto.ConvertedDate = DateTime.UtcNow;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_DisqualifiedLeadRequiresReason_ReturnsError()
    {
        // Arrange
        var dto = CreateValidLeadDto();
        dto.Status = "Disqualified";
        dto.DisqualificationReason = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("DisqualificationReason");
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new CreateLeadDto
        {
            FirstName = "",
            LastName = "",
            Email = "invalid",
            Status = "InvalidStatus",
            Score = -5
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(4);
    }

    #endregion

    #region Helper Methods

    private CreateLeadDto CreateValidLeadDto()
    {
        return new CreateLeadDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@company.com",
            Status = "New",
            Source = "Web",
            Rating = "Warm",
            Score = 50
        };
    }

    #endregion
}

// Supporting classes
public class LeadValidator
{
    private readonly ILeadService _leadService;
    private readonly IUserService _userService;
    private readonly ICampaignService _campaignService;

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "New", "Contacted", "Qualified", "Converted", "Disqualified"
    };

    private static readonly HashSet<string> ValidSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "Web", "Referral", "Trade Show", "Cold Call", "Advertisement",
        "Partner", "Social Media", "Email Campaign", "Other"
    };

    private static readonly HashSet<string> ValidRatings = new(StringComparer.OrdinalIgnoreCase)
    {
        "Hot", "Warm", "Cold"
    };

    public LeadValidator(ILeadService leadService, IUserService userService, ICampaignService campaignService)
    {
        _leadService = leadService;
        _userService = userService;
        _campaignService = campaignService;
    }

    public async Task<ValidationResult> ValidateAsync(CreateLeadDto dto)
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
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            result.AddError("Email", "Email is required");
        }
        else
        {
            if (!IsValidEmail(dto.Email))
            {
                result.AddError("Email", "Invalid email format");
            }
            else if (await _leadService.EmailExistsAsync(dto.Email, dto.Id))
            {
                result.AddError("Email", "A lead with this email already exists");
            }
        }

        // Status validation
        if (!string.IsNullOrEmpty(dto.Status) && !ValidStatuses.Contains(dto.Status))
        {
            result.AddError("Status", "Invalid status value");
        }

        // Source validation
        if (!string.IsNullOrEmpty(dto.Source) && !ValidSources.Contains(dto.Source))
        {
            result.AddError("Source", "Invalid source value");
        }

        // Rating validation
        if (!string.IsNullOrEmpty(dto.Rating) && !ValidRatings.Contains(dto.Rating))
        {
            result.AddError("Rating", "Invalid rating value");
        }

        // Score validation
        if (dto.Score.HasValue)
        {
            if (dto.Score < 0)
                result.AddError("Score", "Score cannot be negative");
            else if (dto.Score > 100)
                result.AddError("Score", "Score cannot exceed 100");
        }

        // Company validation
        if (!string.IsNullOrEmpty(dto.Company) && dto.Company.Length > 255)
        {
            result.AddError("Company", "Company name cannot exceed 255 characters");
        }

        // Phone validation
        if (!string.IsNullOrEmpty(dto.Phone) && !IsValidPhone(dto.Phone))
        {
            result.AddError("Phone", "Invalid phone number format");
        }

        // Website validation
        if (!string.IsNullOrEmpty(dto.Website) && !IsValidWebsite(dto.Website))
        {
            result.AddError("Website", "Invalid website URL");
        }

        // Owner validation
        if (dto.OwnerId.HasValue)
        {
            var owner = await _userService.GetByIdAsync(dto.OwnerId.Value);
            if (owner == null)
            {
                result.AddError("OwnerId", "Owner does not exist");
            }
        }

        // Campaign validation
        if (dto.CampaignId.HasValue)
        {
            var campaign = await _campaignService.GetByIdAsync(dto.CampaignId.Value);
            if (campaign == null)
            {
                result.AddError("CampaignId", "Campaign does not exist");
            }
        }

        // Status-specific validation
        if (dto.Status?.Equals("Converted", StringComparison.OrdinalIgnoreCase) == true && !dto.ConvertedDate.HasValue)
        {
            result.AddError("ConvertedDate", "Conversion date is required for converted leads");
        }

        if (dto.Status?.Equals("Disqualified", StringComparison.OrdinalIgnoreCase) == true &&
            string.IsNullOrWhiteSpace(dto.DisqualificationReason))
        {
            result.AddError("DisqualificationReason", "Disqualification reason is required");
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

public class CreateLeadDto
{
    public int? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Rating { get; set; }
    public int? Score { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
    public string? Website { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public string? DisqualificationReason { get; set; }
}

public class UserResultDto
{
    public int Id { get; set; }
}

public class CampaignResultDto
{
    public int Id { get; set; }
}

public interface ILeadService
{
    Task<bool> EmailExistsAsync(string email, int? excludeId);
}

public interface ICampaignService
{
    Task<CampaignResultDto?> GetByIdAsync(int id);
}

public interface IUserService
{
    Task<UserResultDto?> GetByIdAsync(int id);
}

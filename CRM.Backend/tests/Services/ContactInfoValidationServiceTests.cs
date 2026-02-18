// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ContactInfoValidationService
/// </summary>
public class ContactInfoValidationServiceTests
{
    private readonly Mock<ILogger<ContactInfoValidationService>> _mockLogger;
    private readonly ContactInfoValidationService _service;

    public ContactInfoValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ContactInfoValidationService>>();
        _service = new ContactInfoValidationService(_mockLogger.Object);
    }

    #region Email Validation Tests

    [Fact]
    public async Task ValidateEmailAsync_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeTrue();
        result.NormalizedValue.Should().Be("test@example.com");
    }

    [Fact]
    public async Task ValidateEmailAsync_WithEmptyEmail_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidateEmailAsync("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task ValidateEmailAsync_WithNullEmail_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidateEmailAsync(null!);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test@@example.com")]
    [InlineData("test @example.com")]
    public async Task ValidateEmailAsync_WithInvalidFormat_ReturnsFailure(string email)
    {
        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task ValidateEmailAsync_WithTooLongEmail_ReturnsFailure()
    {
        // Arrange
        var longLocal = new string('a', 65); // > 64 chars
        var email = $"{longLocal}@example.com";

        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("too long");
    }

    [Theory]
    [InlineData("test@gmial.com", "gmail.com")]
    [InlineData("test@gmal.com", "gmail.com")]
    [InlineData("test@hotmal.com", "hotmail.com")]
    [InlineData("test@yaho.com", "yahoo.com")]
    public async Task ValidateEmailAsync_WithCommonTypo_SuggestsCorrection(string email, string correctDomain)
    {
        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain(correctDomain);
    }

    [Theory]
    [InlineData("test@mailinator.com")]
    [InlineData("test@guerrillamail.com")]
    [InlineData("test@tempmail.com")]
    [InlineData("test@10minutemail.com")]
    public async Task ValidateEmailAsync_WithDisposableEmail_ReturnsFailure(string email)
    {
        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Disposable");
    }

    [Theory]
    [InlineData("john.doe+test@gmail.com")]
    [InlineData("first.last@subdomain.example.com")]
    [InlineData("user123@company.co.uk")]
    public async Task ValidateEmailAsync_WithComplexValidEmail_ReturnsSuccess(string email)
    {
        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEmailAsync_NormalizesEmailToLowercase()
    {
        // Arrange
        var email = "TEST@EXAMPLE.COM";

        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeTrue();
        result.NormalizedValue.Should().Be("test@example.com");
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("555-123-4567", "US")]
    [InlineData("(555) 123-4567", "US")]
    [InlineData("+1 555 123 4567", "US")]
    [InlineData("5551234567", "US")]
    public async Task ValidatePhoneNumberAsync_WithValidUSPhone_ReturnsSuccess(string phone, string countryCode)
    {
        // Act
        var result = await _service.ValidatePhoneNumberAsync(phone, countryCode);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePhoneNumberAsync_WithEmptyPhone_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidatePhoneNumberAsync("", "US");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Theory]
    [InlineData("123", "US")]
    [InlineData("abcdefghij", "US")]
    public async Task ValidatePhoneNumberAsync_WithInvalidPhone_ReturnsFailure(string phone, string countryCode)
    {
        // Act
        var result = await _service.ValidatePhoneNumberAsync(phone, countryCode);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("9876543210", "IN")]
    [InlineData("+91 98765 43210", "IN")]
    public async Task ValidatePhoneNumberAsync_WithValidIndianPhone_ReturnsSuccess(string phone, string countryCode)
    {
        // Act
        var result = await _service.ValidatePhoneNumberAsync(phone, countryCode);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePhoneNumberAsync_WithUnknownCountry_UsesGenericValidation()
    {
        // Arrange
        var phone = "+9991234567890";
        var countryCode = "ZZ"; // Unknown country

        // Act
        var result = await _service.ValidatePhoneNumberAsync(phone, countryCode);

        // Assert - should use generic validation for unknown countries
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Phone Formatting Tests

    [Theory]
    [InlineData("5551234567", "US", "+1 (555) 123-4567")]
    [InlineData("15551234567", "US", "+1 (555) 123-4567")]
    [InlineData("+1 (555) 123-4567", "US", "+1 (555) 123-4567")]
    public void FormatPhoneNumber_WithUSPhone_FormatsCorrectly(string input, string countryCode, string expected)
    {
        // Act
        var result = _service.FormatPhoneNumber(input, countryCode);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("9876543210", "IN", "+91 98765 43210")]
    public void FormatPhoneNumber_WithIndianPhone_FormatsCorrectly(string input, string countryCode, string expected)
    {
        // Act
        var result = _service.FormatPhoneNumber(input, countryCode);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Social Media Validation Tests

    [Theory]
    [InlineData("https://linkedin.com/in/johndoe", SocialMediaPlatform.LinkedIn)]
    [InlineData("https://www.linkedin.com/in/john-doe", SocialMediaPlatform.LinkedIn)]
    [InlineData("johndoe", SocialMediaPlatform.LinkedIn)]
    public async Task ValidateSocialMediaAccountAsync_WithValidLinkedIn_ReturnsSuccess(string handle, SocialMediaPlatform platform)
    {
        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(handle, platform);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://twitter.com/username", SocialMediaPlatform.Twitter)]
    [InlineData("https://x.com/username", SocialMediaPlatform.Twitter)]
    [InlineData("@username", SocialMediaPlatform.Twitter)]
    [InlineData("username", SocialMediaPlatform.Twitter)]
    public async Task ValidateSocialMediaAccountAsync_WithValidTwitter_ReturnsSuccess(string handle, SocialMediaPlatform platform)
    {
        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(handle, platform);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://instagram.com/user.name", SocialMediaPlatform.Instagram)]
    [InlineData("user_name", SocialMediaPlatform.Instagram)]
    public async Task ValidateSocialMediaAccountAsync_WithValidInstagram_ReturnsSuccess(string handle, SocialMediaPlatform platform)
    {
        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(handle, platform);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSocialMediaAccountAsync_WithEmptyHandle_ReturnsFailure()
    {
        // Act
        var result = await _service.ValidateSocialMediaAccountAsync("", SocialMediaPlatform.LinkedIn);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task ValidateSocialMediaAccountAsync_WithOtherPlatform_AcceptsAnyReasonableInput()
    {
        // Arrange
        var handle = "my-custom-handle";

        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(handle, SocialMediaPlatform.Other);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSocialMediaAccountAsync_ExtractsHandleFromUrl()
    {
        // Arrange
        var url = "https://twitter.com/testuser";

        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(url, SocialMediaPlatform.Twitter);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Details.Should().ContainKey("Handle");
        result.Details["Handle"].Should().Be("testuser");
    }

    [Fact]
    public async Task ValidateSocialMediaAccountAsync_GeneratesProfileUrl()
    {
        // Arrange
        var handle = "testuser";

        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(handle, SocialMediaPlatform.Twitter);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Details.Should().ContainKey("ProfileUrl");
        result.Details["ProfileUrl"].Should().Contain("x.com");
    }

    #endregion

    #region Handle Extraction Tests

    [Theory]
    [InlineData("https://linkedin.com/in/john-doe", SocialMediaPlatform.LinkedIn, "john-doe")]
    [InlineData("https://twitter.com/johndoe", SocialMediaPlatform.Twitter, "johndoe")]
    [InlineData("https://instagram.com/john.doe", SocialMediaPlatform.Instagram, "john.doe")]
    [InlineData("https://facebook.com/johndoe", SocialMediaPlatform.Facebook, "johndoe")]
    public void ExtractSocialMediaHandle_FromUrl_ReturnsHandle(string url, SocialMediaPlatform platform, string expectedHandle)
    {
        // Act
        var result = _service.ExtractSocialMediaHandle(url, platform);

        // Assert
        result.Should().Be(expectedHandle);
    }

    [Fact]
    public void ExtractSocialMediaHandle_WithNullUrl_ReturnsNull()
    {
        // Act
        var result = _service.ExtractSocialMediaHandle(null!, SocialMediaPlatform.LinkedIn);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("@johndoe", SocialMediaPlatform.Twitter, "johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.Instagram, "johndoe")]
    public void ExtractSocialMediaHandle_FromPlainHandle_ReturnsHandle(string input, SocialMediaPlatform platform, string expectedHandle)
    {
        // Act
        var result = _service.ExtractSocialMediaHandle(input, platform);

        // Assert
        result.Should().Be(expectedHandle);
    }

    #endregion

    #region Profile URL Generation Tests

    [Theory]
    [InlineData("johndoe", SocialMediaPlatform.LinkedIn, "https://linkedin.com/in/johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.Twitter, "https://x.com/johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.Instagram, "https://instagram.com/johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.Facebook, "https://facebook.com/johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.TikTok, "https://tiktok.com/@johndoe")]
    [InlineData("johndoe", SocialMediaPlatform.Telegram, "https://t.me/johndoe")]
    public void GenerateProfileUrl_WithValidHandle_ReturnsUrl(string handle, SocialMediaPlatform platform, string expectedUrl)
    {
        // Act
        var result = _service.GenerateProfileUrl(handle, platform);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public void GenerateProfileUrl_WithNullHandle_ReturnsNull()
    {
        // Act
        var result = _service.GenerateProfileUrl(null!, SocialMediaPlatform.LinkedIn);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenerateProfileUrl_WithEmptyHandle_ReturnsNull()
    {
        // Act
        var result = _service.GenerateProfileUrl("", SocialMediaPlatform.LinkedIn);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenerateProfileUrl_StripsLeadingAtSymbol()
    {
        // Act
        var result = _service.GenerateProfileUrl("@johndoe", SocialMediaPlatform.Twitter);

        // Assert
        result.Should().Be("https://x.com/johndoe");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ValidateEmailAsync_TrimsWhitespace()
    {
        // Arrange
        var email = "  test@example.com  ";

        // Act
        var result = await _service.ValidateEmailAsync(email);

        // Assert
        result.IsValid.Should().BeTrue();
        result.NormalizedValue.Should().Be("test@example.com");
    }

    [Fact]
    public async Task ValidatePhoneNumberAsync_HandlesFormattedInput()
    {
        // Arrange
        var phone = "(555) 123-4567";

        // Act
        var result = await _service.ValidatePhoneNumberAsync(phone, "US");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateSocialMediaAccountAsync_WithVeryLongHandle_ReturnsAppropriateResult()
    {
        // Arrange
        var longHandle = new string('a', 250);

        // Act
        var result = await _service.ValidateSocialMediaAccountAsync(longHandle, SocialMediaPlatform.Other);

        // Assert
        result.IsValid.Should().BeFalse(); // > 200 chars for Other platform
    }

    #endregion
}

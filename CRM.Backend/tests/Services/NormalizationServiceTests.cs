// CRM Solution - Customer Relationship Management System
// Normalization Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for NormalizationService
/// Covers: Data normalization, formatting, standardization
/// </summary>
public class NormalizationServiceTests
{
    private readonly Mock<ILogger<NormalizationService>> _mockLogger;
    private readonly NormalizationService _service;

    public NormalizationServiceTests()
    {
        _mockLogger = new Mock<ILogger<NormalizationService>>();
        _service = new NormalizationService(_mockLogger.Object);
    }

    #region Phone Number Tests

    [Theory]
    [InlineData("1234567890", "+1 (123) 456-7890")]
    [InlineData("123-456-7890", "+1 (123) 456-7890")]
    [InlineData("(123) 456-7890", "+1 (123) 456-7890")]
    [InlineData("+1 123 456 7890", "+1 (123) 456-7890")]
    public void NormalizePhoneNumber_USFormats_ReturnsStandardFormat(string input, string expected)
    {
        // Act
        var result = _service.NormalizePhoneNumber(input, "US");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizePhoneNumber_InvalidNumber_ReturnsOriginal()
    {
        // Arrange
        var input = "invalid";

        // Act
        var result = _service.NormalizePhoneNumber(input, "US");

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void NormalizePhoneNumber_EmptyInput_ReturnsEmpty()
    {
        // Act
        var result = _service.NormalizePhoneNumber("", "US");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("+44 20 7946 0958", "UK")]
    [InlineData("+49 30 12345678", "DE")]
    public void NormalizePhoneNumber_InternationalFormats_ReturnsNormalized(string input, string country)
    {
        // Act
        var result = _service.NormalizePhoneNumber(input, country);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Email Tests

    [Theory]
    [InlineData("Test@Example.COM", "test@example.com")]
    [InlineData("  user@domain.com  ", "user@domain.com")]
    [InlineData("USER@DOMAIN.ORG", "user@domain.org")]
    public void NormalizeEmail_ValidEmails_ReturnsLowercase(string input, string expected)
    {
        // Act
        var result = _service.NormalizeEmail(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeEmail_WithSpaces_TrimsSpaces()
    {
        // Arrange
        var input = "  test@example.com  ";

        // Act
        var result = _service.NormalizeEmail(input);

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void NormalizeEmail_EmptyInput_ReturnsEmpty()
    {
        // Act
        var result = _service.NormalizeEmail("");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Name Tests

    [Theory]
    [InlineData("john doe", "John Doe")]
    [InlineData("JOHN DOE", "John Doe")]
    [InlineData("jOHN dOE", "John Doe")]
    public void NormalizeName_VariousFormats_ReturnsTitleCase(string input, string expected)
    {
        // Act
        var result = _service.NormalizeName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("mcdonald", "McDonald")]
    [InlineData("o'brien", "O'Brien")]
    [InlineData("van der berg", "Van Der Berg")]
    public void NormalizeName_SpecialCases_HandlesCorrectly(string input, string expected)
    {
        // Act
        var result = _service.NormalizeName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeName_WithExtraSpaces_TrimsSpaces()
    {
        // Arrange
        var input = "  John   Doe  ";

        // Act
        var result = _service.NormalizeName(input);

        // Assert
        result.Should().Be("John Doe");
    }

    #endregion

    #region Address Tests

    [Fact]
    public void NormalizeAddress_ValidAddress_NormalizesComponents()
    {
        // Arrange
        var address = new AddressInput
        {
            Street = "123 main street",
            City = "new york",
            State = "ny",
            ZipCode = "10001",
            Country = "usa"
        };

        // Act
        var result = _service.NormalizeAddress(address);

        // Assert
        result.Street.Should().Be("123 Main Street");
        result.City.Should().Be("New York");
        result.State.Should().Be("NY");
        result.Country.Should().Be("USA");
    }

    [Fact]
    public void NormalizeAddress_WithAbbreviations_ExpandsAbbreviations()
    {
        // Arrange
        var address = new AddressInput
        {
            Street = "123 main st",
            City = "los angeles"
        };

        // Act
        var result = _service.NormalizeAddress(address);

        // Assert
        result.Street.Should().Contain("Street");
    }

    [Fact]
    public void NormalizeAddress_EmptyComponents_HandlesGracefully()
    {
        // Arrange
        var address = new AddressInput
        {
            Street = "",
            City = "",
            State = "",
            ZipCode = ""
        };

        // Act
        var result = _service.NormalizeAddress(address);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Company Name Tests

    [Theory]
    [InlineData("acme corp", "Acme Corp")]
    [InlineData("ACME CORPORATION", "Acme Corporation")]
    [InlineData("ibm", "IBM")]
    public void NormalizeCompanyName_VariousFormats_ReturnsNormalized(string input, string expected)
    {
        // Act
        var result = _service.NormalizeCompanyName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("acme, inc.", "Acme, Inc.")]
    [InlineData("acme llc", "Acme LLC")]
    [InlineData("acme ltd", "Acme Ltd")]
    public void NormalizeCompanyName_WithSuffix_PreservesSuffix(string input, string expected)
    {
        // Act
        var result = _service.NormalizeCompanyName(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region URL Tests

    [Theory]
    [InlineData("example.com", "https://example.com")]
    [InlineData("http://example.com", "https://example.com")]
    [InlineData("HTTPS://EXAMPLE.COM", "https://example.com")]
    public void NormalizeUrl_VariousFormats_ReturnsHttps(string input, string expected)
    {
        // Act
        var result = _service.NormalizeUrl(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeUrl_WithTrailingSlash_RemovesSlash()
    {
        // Arrange
        var input = "https://example.com/";

        // Act
        var result = _service.NormalizeUrl(input);

        // Assert
        result.Should().Be("https://example.com");
    }

    [Fact]
    public void NormalizeUrl_EmptyInput_ReturnsEmpty()
    {
        // Act
        var result = _service.NormalizeUrl("");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Currency Tests

    [Theory]
    [InlineData("$1,234.56", 1234.56)]
    [InlineData("1234.56", 1234.56)]
    [InlineData("€1.234,56", 1234.56)]
    public void NormalizeCurrency_VariousFormats_ReturnsDecimal(string input, decimal expected)
    {
        // Act
        var result = _service.NormalizeCurrency(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeCurrency_InvalidInput_ReturnsZero()
    {
        // Act
        var result = _service.NormalizeCurrency("invalid");

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Date Tests

    [Theory]
    [InlineData("01/15/2024", "2024-01-15")]
    [InlineData("15-01-2024", "2024-01-15")]
    [InlineData("2024/01/15", "2024-01-15")]
    public void NormalizeDate_VariousFormats_ReturnsISOFormat(string input, string expected)
    {
        // Act
        var result = _service.NormalizeDate(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizeDate_InvalidDate_ReturnsNull()
    {
        // Act
        var result = _service.NormalizeDate("invalid");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Batch Normalization Tests

    [Fact]
    public async Task NormalizeBatchAsync_ValidRecords_NormalizesAll()
    {
        // Arrange
        var records = new List<NormalizationRecord>
        {
            new NormalizationRecord { Type = "phone", Value = "1234567890" },
            new NormalizationRecord { Type = "email", Value = "TEST@EXAMPLE.COM" }
        };

        // Act
        var result = await _service.NormalizeBatchAsync(records);

        // Assert
        result.ProcessedCount.Should().Be(2);
    }

    #endregion

    #region Text Cleanup Tests

    [Fact]
    public void CleanText_WithSpecialCharacters_RemovesSpecialChars()
    {
        // Arrange
        var input = "Hello<script>alert('test')</script>World";

        // Act
        var result = _service.CleanText(input);

        // Assert
        result.Should().NotContain("<script>");
    }

    [Fact]
    public void CleanText_WithMultipleSpaces_NormalizesSpaces()
    {
        // Arrange
        var input = "Hello    World";

        // Act
        var result = _service.CleanText(input);

        // Assert
        result.Should().Be("Hello World");
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void GetNormalizationRules_ReturnsRules()
    {
        // Act
        var result = _service.GetNormalizationRules();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainKey("phone");
        result.Should().ContainKey("email");
    }

    #endregion
}

// Supporting classes for tests
public class AddressInput
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class NormalizationRecord
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

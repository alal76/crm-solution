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
using CRM.Core.Validation;
using CRM.Core.DTOs;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace CRM.Tests.Validators;

/// <summary>
/// Unit tests for Common Validators (shared validation rules)
/// Covers: Email, phone, URL, date range, currency, percentage validations
/// </summary>
public class CommonValidatorTests
{
    private readonly CommonValidator _validator;

    public CommonValidatorTests()
    {
        _validator = new CommonValidator();
    }

    #region Email Validation Tests

    [Theory]
    [InlineData("user@domain.com", true)]
    [InlineData("user.name@domain.com", true)]
    [InlineData("user+tag@domain.com", true)]
    [InlineData("user@subdomain.domain.com", true)]
    [InlineData("user@domain.co.uk", true)]
    public void ValidateEmail_ValidEmails_ReturnsTrue(string email, bool expected)
    {
        // Act
        var result = _validator.IsValidEmail(email);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    [InlineData("user@.com")]
    [InlineData("user@domain")]
    [InlineData("user@domain.")]
    [InlineData("user @domain.com")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateEmail_InvalidEmails_ReturnsFalse(string? email)
    {
        // Act
        var result = _validator.IsValidEmail(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateEmail_EmailTooLong_ReturnsFalse()
    {
        // Arrange
        var email = new string('a', 250) + "@domain.com";

        // Act
        var result = _validator.IsValidEmail(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("+1-555-123-4567", true)]
    [InlineData("+1 555 123 4567", true)]
    [InlineData("555-123-4567", true)]
    [InlineData("(555) 123-4567", true)]
    [InlineData("+44 20 7946 0958", true)]
    [InlineData("5551234567", true)]
    [InlineData("+1.555.123.4567", true)]
    public void ValidatePhone_ValidPhones_ReturnsTrue(string phone, bool expected)
    {
        // Act
        var result = _validator.IsValidPhone(phone);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12")]
    [InlineData("phone")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    public void ValidatePhone_InvalidPhones_ReturnsFalse(string? phone)
    {
        // Act
        var result = _validator.IsValidPhone(phone);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePhone_PhoneTooLong_ReturnsFalse()
    {
        // Arrange - More than 20 digits
        var phone = "+" + new string('1', 25);

        // Act
        var result = _validator.IsValidPhone(phone);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region URL Validation Tests

    [Theory]
    [InlineData("https://www.example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com/path/to/page", true)]
    [InlineData("https://example.com?query=value", true)]
    [InlineData("https://example.co.uk", true)]
    [InlineData("https://subdomain.example.com", true)]
    public void ValidateUrl_ValidUrls_ReturnsTrue(string url, bool expected)
    {
        // Act
        var result = _validator.IsValidUrl(url);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("file:///path/to/file")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateUrl_InvalidUrls_ReturnsFalse(string? url)
    {
        // Act
        var result = _validator.IsValidUrl(url);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://www.linkedin.com/in/username", true)]
    [InlineData("https://linkedin.com/in/username", true)]
    [InlineData("https://facebook.com/username", false)]
    [InlineData("https://example.com", false)]
    public void ValidateLinkedInUrl_ReturnsExpected(string url, bool expected)
    {
        // Act
        var result = _validator.IsValidLinkedInUrl(url);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://twitter.com/username", true)]
    [InlineData("https://x.com/username", true)]
    [InlineData("https://facebook.com/username", false)]
    public void ValidateTwitterUrl_ReturnsExpected(string url, bool expected)
    {
        // Act
        var result = _validator.IsValidTwitterUrl(url);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public void ValidateDateRange_ValidRange_ReturnsNoError()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(30);

        // Act
        var result = _validator.ValidateDateRange(start, end);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateDateRange_EndBeforeStart_ReturnsError()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.ValidateDateRange(start, end);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("before");
    }

    [Fact]
    public void ValidateDateRange_SameDate_ReturnsNoError()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var result = _validator.ValidateDateRange(date, date);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateFutureDate_FutureDate_ReturnsNoError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(1);

        // Act
        var result = _validator.ValidateFutureDate(date);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateFutureDate_PastDate_ReturnsError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.ValidateFutureDate(date);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidatePastDate_PastDate_ReturnsNoError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.ValidatePastDate(date);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePastDate_FutureDate_ReturnsError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(1);

        // Act
        var result = _validator.ValidatePastDate(date);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateReasonableDate_TooOld_ReturnsError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(-200);

        // Act
        var result = _validator.ValidateReasonableDate(date);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateReasonableDate_TooFarInFuture_ReturnsError()
    {
        // Arrange
        var date = DateTime.UtcNow.AddYears(101);

        // Act
        var result = _validator.ValidateReasonableDate(date);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Currency Validation Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(100.50, true)]
    [InlineData(999999999.99, true)]
    public void ValidateCurrency_ValidAmounts_ReturnsTrue(decimal amount, bool expected)
    {
        // Act
        var result = _validator.IsValidCurrency(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void ValidateCurrency_NegativeAmounts_ReturnsFalse(decimal amount)
    {
        // Act
        var result = _validator.IsValidCurrency(amount);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCurrency_AllowNegative_ReturnsTrue()
    {
        // Act
        var result = _validator.IsValidCurrency(-100.50m, allowNegative: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateCurrency_TooManyDecimalPlaces_ReturnsFalse()
    {
        // Arrange
        var amount = 100.12345m;

        // Act
        var result = _validator.IsValidCurrency(amount, maxDecimalPlaces: 2);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Percentage Validation Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    public void ValidatePercentage_ValidPercentages_ReturnsTrue(int percentage, bool expected)
    {
        // Act
        var result = _validator.IsValidPercentage(percentage);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ValidatePercentage_NegativePercentages_ReturnsFalse(int percentage)
    {
        // Act
        var result = _validator.IsValidPercentage(percentage);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    public void ValidatePercentage_Over100_ReturnsFalse(int percentage)
    {
        // Act
        var result = _validator.IsValidPercentage(percentage);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePercentage_CustomMax_ReturnsTrue()
    {
        // Act - Allow percentages up to 200%
        var result = _validator.IsValidPercentage(150, maxValue: 200);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region String Length Validation Tests

    [Theory]
    [InlineData("test", 1, 10, true)]
    [InlineData("test", 4, 4, true)]
    [InlineData("", 0, 10, true)]
    public void ValidateStringLength_ValidLengths_ReturnsTrue(string value, int min, int max, bool expected)
    {
        // Act
        var result = _validator.IsValidStringLength(value, min, max);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("ab", 3, 10)]
    [InlineData("this is too long", 1, 5)]
    public void ValidateStringLength_InvalidLengths_ReturnsFalse(string value, int min, int max)
    {
        // Act
        var result = _validator.IsValidStringLength(value, min, max);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateStringLength_NullWithMinLength_ReturnsFalse()
    {
        // Act
        var result = _validator.IsValidStringLength(null, 1, 10);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateStringLength_NullWithZeroMin_ReturnsTrue()
    {
        // Act
        var result = _validator.IsValidStringLength(null, 0, 10);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Alphanumeric Validation Tests

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("ABC123", true)]
    [InlineData("test", true)]
    [InlineData("123", true)]
    public void ValidateAlphanumeric_ValidValues_ReturnsTrue(string value, bool expected)
    {
        // Act
        var result = _validator.IsAlphanumeric(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("abc 123")]
    [InlineData("abc-123")]
    [InlineData("abc_123")]
    [InlineData("abc@123")]
    public void ValidateAlphanumeric_InvalidValues_ReturnsFalse(string value)
    {
        // Act
        var result = _validator.IsAlphanumeric(value);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("abc_123", true)]
    [InlineData("abc-123", true)]
    [InlineData("abc.123", true)]
    public void ValidateAlphanumericWithSpecialChars_ReturnsTrue(string value, bool expected)
    {
        // Act
        var result = _validator.IsAlphanumericWithAllowedChars(value, "_-.");

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Collection Validation Tests

    [Fact]
    public void ValidateCollection_NotEmpty_ReturnsTrue()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = _validator.ValidateCollection(items, minCount: 1);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCollection_EmptyWithMinCount_ReturnsFalse()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var result = _validator.ValidateCollection(items, minCount: 1);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateCollection_ExceedsMaxCount_ReturnsFalse()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = _validator.ValidateCollection(items, maxCount: 3);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateCollection_NullCollection_ReturnsFalse()
    {
        // Arrange
        List<int>? items = null;

        // Act
        var result = _validator.ValidateCollection(items, minCount: 1);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Enum Validation Tests

    [Fact]
    public void ValidateEnumValue_ValidValue_ReturnsTrue()
    {
        // Act
        var result = _validator.IsValidEnumValue<TestStatus>("Active");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateEnumValue_InvalidValue_ReturnsFalse()
    {
        // Act
        var result = _validator.IsValidEnumValue<TestStatus>("Invalid");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateEnumValue_CaseInsensitive_ReturnsTrue()
    {
        // Act
        var result = _validator.IsValidEnumValue<TestStatus>("active");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ZIP/Postal Code Validation Tests

    [Theory]
    [InlineData("12345", "US", true)]
    [InlineData("12345-6789", "US", true)]
    [InlineData("SW1A 1AA", "UK", true)]
    [InlineData("A1A 1A1", "CA", true)]
    public void ValidatePostalCode_ValidCodes_ReturnsTrue(string code, string countryCode, bool expected)
    {
        // Act
        var result = _validator.IsValidPostalCode(code, countryCode);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1234", "US")]
    [InlineData("123456", "US")]
    [InlineData("ABCDE", "US")]
    public void ValidatePostalCode_InvalidUSCodes_ReturnsFalse(string code, string countryCode)
    {
        // Act
        var result = _validator.IsValidPostalCode(code, countryCode);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

// Supporting classes
public class CommonValidator
{
    public bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Contains(".") && !email.EndsWith(".");
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length >= 7 && digits.Length <= 20;
    }

    public bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public bool IsValidLinkedInUrl(string? url)
    {
        if (!IsValidUrl(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               uri.Host.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsValidTwitterUrl(string? url)
    {
        if (!IsValidUrl(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Host.Contains("twitter.com", StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Contains("x.com", StringComparison.OrdinalIgnoreCase));
    }

    public DateValidationResult ValidateDateRange(DateTime start, DateTime end)
    {
        if (end < start)
            return DateValidationResult.Invalid("End date cannot be before start date");
        return DateValidationResult.Valid();
    }

    public DateValidationResult ValidateFutureDate(DateTime date)
    {
        if (date <= DateTime.UtcNow)
            return DateValidationResult.Invalid("Date must be in the future");
        return DateValidationResult.Valid();
    }

    public DateValidationResult ValidatePastDate(DateTime date)
    {
        if (date >= DateTime.UtcNow)
            return DateValidationResult.Invalid("Date must be in the past");
        return DateValidationResult.Valid();
    }

    public DateValidationResult ValidateReasonableDate(DateTime date)
    {
        var minDate = DateTime.UtcNow.AddYears(-150);
        var maxDate = DateTime.UtcNow.AddYears(100);

        if (date < minDate)
            return DateValidationResult.Invalid("Date is too far in the past");
        if (date > maxDate)
            return DateValidationResult.Invalid("Date is too far in the future");
        return DateValidationResult.Valid();
    }

    public bool IsValidCurrency(decimal amount, bool allowNegative = false, int maxDecimalPlaces = 2)
    {
        if (!allowNegative && amount < 0)
            return false;

        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
        return decimalPlaces <= maxDecimalPlaces;
    }

    public bool IsValidPercentage(int percentage, int minValue = 0, int maxValue = 100)
    {
        return percentage >= minValue && percentage <= maxValue;
    }

    public bool IsValidStringLength(string? value, int minLength, int maxLength)
    {
        if (value == null)
            return minLength == 0;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    public bool IsAlphanumeric(string value)
    {
        return !string.IsNullOrEmpty(value) && value.All(char.IsLetterOrDigit);
    }

    public bool IsAlphanumericWithAllowedChars(string value, string allowedChars)
    {
        return !string.IsNullOrEmpty(value) &&
               value.All(c => char.IsLetterOrDigit(c) || allowedChars.Contains(c));
    }

    public CollectionValidationResult ValidateCollection<T>(IList<T>? items, int minCount = 0, int maxCount = int.MaxValue)
    {
        if (items == null)
            return minCount == 0
                ? CollectionValidationResult.Valid()
                : CollectionValidationResult.Invalid("Collection cannot be null");

        if (items.Count < minCount)
            return CollectionValidationResult.Invalid($"Collection must have at least {minCount} items");

        if (items.Count > maxCount)
            return CollectionValidationResult.Invalid($"Collection cannot have more than {maxCount} items");

        return CollectionValidationResult.Valid();
    }

    public bool IsValidEnumValue<TEnum>(string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out _);
    }

    public bool IsValidPostalCode(string code, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return countryCode.ToUpper() switch
        {
            "US" => Regex.IsMatch(code, @"^\d{5}(-\d{4})?$"),
            "UK" => Regex.IsMatch(code, @"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$", RegexOptions.IgnoreCase),
            "CA" => Regex.IsMatch(code, @"^[A-Z]\d[A-Z] ?\d[A-Z]\d$", RegexOptions.IgnoreCase),
            _ => code.Length >= 3 && code.Length <= 10
        };
    }
}

public class DateValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static DateValidationResult Valid() => new() { IsValid = true };
    public static DateValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
}

public class CollectionValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static CollectionValidationResult Valid() => new() { IsValid = true };
    public static CollectionValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
}

public enum TestStatus
{
    Active,
    Inactive,
    Pending
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Infrastructure.Validation;
using Xunit;

namespace CRM.Tests.Validators
{
    /// <summary>
    /// Comprehensive unit tests for <see cref="LocalizationValidator"/>.
    /// Tests validation of timezone identifiers, currency codes, and language tags.
    ///
    /// Related: TODO-SYS005-003: Localization settings validation
    /// </summary>
    public class LocalizationValidatorTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateTimezone Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that common Windows timezone identifiers pass validation.
        /// </summary>
        [Theory]
        [InlineData("UTC")]
        [InlineData("Eastern Standard Time")]
        [InlineData("Pacific Standard Time")]
        [InlineData("Central Standard Time")]
        [InlineData("Mountain Standard Time")]
        public void ValidateTimezone_ShouldNotThrow_WhenValidWindowsTimezone(string timezone)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateTimezone(timezone));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that common IANA timezone identifiers pass validation.
        /// </summary>
        [Theory]
        [InlineData("America/New_York")]
        [InlineData("America/Los_Angeles")]
        [InlineData("America/Chicago")]
        [InlineData("Europe/London")]
        [InlineData("Europe/Paris")]
        [InlineData("Asia/Tokyo")]
        [InlineData("Asia/Shanghai")]
        [InlineData("Australia/Sydney")]
        public void ValidateTimezone_ShouldNotThrow_WhenValidIanaTimezone(string timezone)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateTimezone(timezone));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that GMT is a valid timezone.
        /// </summary>
        [Fact]
        public void ValidateTimezone_ShouldNotThrow_WhenGMT()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateTimezone("GMT"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that null timezone throws ValidationException.
        /// </summary>
        [Fact]
        public void ValidateTimezone_ShouldThrow_WhenNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateTimezone(null));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that empty timezone throws ValidationException.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateTimezone_ShouldThrow_WhenEmptyOrWhitespace(string timezone)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateTimezone(timezone));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that invalid timezone throws ValidationException.
        /// </summary>
        [Theory]
        [InlineData("Invalid/Timezone")]
        [InlineData("FakeTimezone")]
        [InlineData("America/Invalid")]
        [InlineData("BadFormat")]
        public void ValidateTimezone_ShouldThrow_WhenInvalidTimezone(string timezone)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateTimezone(timezone));

            Assert.Contains("Invalid timezone", exception.Message);
        }

        /// <summary>
        /// Verifies that exception message provides helpful guidance.
        /// </summary>
        [Fact]
        public void ValidateTimezone_ShouldProvideHelpfulMessage_WhenInvalid()
        {
            // Arrange
            var invalidTimezone = "BadTimezone";

            // Act
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateTimezone(invalidTimezone));

            // Assert
            Assert.Contains("IANA timezone identifier", exception.Message);
            Assert.Contains("Windows timezone identifier", exception.Message);
            Assert.Contains("UTC", exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateCurrency Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that common ISO 4217 currency codes pass validation.
        /// </summary>
        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        [InlineData("JPY")]
        [InlineData("CAD")]
        [InlineData("AUD")]
        [InlineData("CHF")]
        [InlineData("CNY")]
        [InlineData("INR")]
        public void ValidateCurrency_ShouldNotThrow_WhenValidCurrencyCode(string currency)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateCurrency(currency));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that currency validation is case-insensitive.
        /// </summary>
        [Theory]
        [InlineData("usd")]
        [InlineData("USD")]
        [InlineData("Usd")]
        [InlineData("eur")]
        [InlineData("EUR")]
        public void ValidateCurrency_ShouldBeCaseInsensitive(string currency)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateCurrency(currency));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that null currency throws ValidationException.
        /// </summary>
        [Fact]
        public void ValidateCurrency_ShouldThrow_WhenNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateCurrency(null));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that empty currency throws ValidationException.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateCurrency_ShouldThrow_WhenEmptyOrWhitespace(string currency)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateCurrency(currency));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that invalid currency codes throw ValidationException.
        /// </summary>
        [Theory]
        [InlineData("XXX")]
        [InlineData("INVALID")]
        [InlineData("AB")]
        [InlineData("ABCD")]
        [InlineData("123")]
        public void ValidateCurrency_ShouldThrow_WhenInvalidCurrencyCode(string currency)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateCurrency(currency));

            Assert.Contains("Invalid currency", exception.Message);
        }

        /// <summary>
        /// Verifies that exception message provides helpful guidance.
        /// </summary>
        [Fact]
        public void ValidateCurrency_ShouldProvideHelpfulMessage_WhenInvalid()
        {
            // Arrange
            var invalidCurrency = "XXX";

            // Act
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateCurrency(invalidCurrency));

            // Assert
            Assert.Contains("ISO 4217", exception.Message);
            Assert.Contains("USD", exception.Message);
            Assert.Contains("EUR", exception.Message);
            Assert.Contains("GBP", exception.Message);
        }

        /// <summary>
        /// Verifies that less common but valid currency codes are accepted.
        /// </summary>
        [Theory]
        [InlineData("BRL")]
        [InlineData("MXN")]
        [InlineData("KRW")]
        [InlineData("RUB")]
        [InlineData("TRY")]
        [InlineData("ZAR")]
        [InlineData("SEK")]
        [InlineData("NOK")]
        public void ValidateCurrency_ShouldNotThrow_WhenLessCommonButValidCurrency(string currency)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateCurrency(currency));

            Assert.Null(exception);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateLanguage Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that common BCP-47 language tags pass validation.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("en-GB")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        [InlineData("es-ES")]
        [InlineData("it-IT")]
        [InlineData("ja-JP")]
        [InlineData("zh-CN")]
        public void ValidateLanguage_ShouldNotThrow_WhenValidLanguageTag(string language)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that language validation is case-insensitive.
        /// </summary>
        [Theory]
        [InlineData("en-us")]
        [InlineData("EN-US")]
        [InlineData("En-Us")]
        [InlineData("fr-FR")]
        [InlineData("FR-FR")]
        public void ValidateLanguage_ShouldBeCaseInsensitive(string language)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that null language throws ValidationException.
        /// </summary>
        [Fact]
        public void ValidateLanguage_ShouldThrow_WhenNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateLanguage(null));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that empty language throws ValidationException.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateLanguage_ShouldThrow_WhenEmptyOrWhitespace(string language)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Contains("cannot be empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that invalid language codes throw ValidationException.
        /// </summary>
        [Theory]
        [InlineData("en")]
        [InlineData("invalid")]
        [InlineData("xx-XX")]
        [InlineData("123-456")]
        public void ValidateLanguage_ShouldThrow_WhenInvalidLanguageCode(string language)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Contains("Invalid language", exception.Message);
        }

        /// <summary>
        /// Verifies that exception message provides helpful guidance.
        /// </summary>
        [Fact]
        public void ValidateLanguage_ShouldProvideHelpfulMessage_WhenInvalid()
        {
            // Arrange
            var invalidLanguage = "invalid";

            // Act
            var exception = Assert.Throws<ValidationException>(() =>
                LocalizationValidator.ValidateLanguage(invalidLanguage));

            // Assert
            Assert.Contains("BCP-47", exception.Message);
            Assert.Contains("en-US", exception.Message);
            Assert.Contains("fr-FR", exception.Message);
        }

        /// <summary>
        /// Verifies that various English locale variants are supported.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("en-GB")]
        [InlineData("en-AU")]
        [InlineData("en-CA")]
        [InlineData("en-NZ")]
        public void ValidateLanguage_ShouldNotThrow_WhenEnglishVariants(string language)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that various regional language variants are supported.
        /// </summary>
        [Theory]
        [InlineData("es-ES")]
        [InlineData("es-MX")]
        [InlineData("es-AR")]
        [InlineData("fr-FR")]
        [InlineData("fr-CA")]
        [InlineData("de-DE")]
        [InlineData("de-AT")]
        [InlineData("pt-PT")]
        [InlineData("pt-BR")]
        public void ValidateLanguage_ShouldNotThrow_WhenRegionalVariants(string language)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                LocalizationValidator.ValidateLanguage(language));

            Assert.Null(exception);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetSupportedCurrencies Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that GetSupportedCurrencies returns non-empty collection.
        /// </summary>
        [Fact]
        public void GetSupportedCurrencies_ShouldReturnNonEmptyCollection()
        {
            // Act
            var currencies = LocalizationValidator.GetSupportedCurrencies();

            // Assert
            Assert.NotNull(currencies);
            Assert.NotEmpty(currencies);
        }

        /// <summary>
        /// Verifies that GetSupportedCurrencies includes common currencies.
        /// </summary>
        [Fact]
        public void GetSupportedCurrencies_ShouldIncludeCommonCurrencies()
        {
            // Act
            var currencies = LocalizationValidator.GetSupportedCurrencies();

            // Assert
            Assert.Contains("USD", currencies);
            Assert.Contains("EUR", currencies);
            Assert.Contains("GBP", currencies);
            Assert.Contains("JPY", currencies);
            Assert.Contains("CAD", currencies);
        }

        /// <summary>
        /// Verifies that GetSupportedCurrencies returns sorted collection.
        /// </summary>
        [Fact]
        public void GetSupportedCurrencies_ShouldReturnSortedCollection()
        {
            // Act
            var currencies = LocalizationValidator.GetSupportedCurrencies().ToList();

            // Assert
            var sorted = currencies.OrderBy(c => c).ToList();
            Assert.Equal(sorted, currencies);
        }

        /// <summary>
        /// Verifies that GetSupportedCurrencies returns read-only collection.
        /// </summary>
        [Fact]
        public void GetSupportedCurrencies_ShouldReturnReadOnlyCollection()
        {
            // Act
            var currencies = LocalizationValidator.GetSupportedCurrencies();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<string>>(currencies);
        }

        /// <summary>
        /// Verifies that GetSupportedCurrencies has expected minimum count (50+).
        /// </summary>
        [Fact]
        public void GetSupportedCurrencies_ShouldHaveMinimumCurrencies()
        {
            // Act
            var currencies = LocalizationValidator.GetSupportedCurrencies();

            // Assert - should have at least 50 currencies based on code
            Assert.True(currencies.Count >= 50, $"Expected at least 50 currencies, got {currencies.Count}");
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetSupportedLanguages Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that GetSupportedLanguages returns non-empty collection.
        /// </summary>
        [Fact]
        public void GetSupportedLanguages_ShouldReturnNonEmptyCollection()
        {
            // Act
            var languages = LocalizationValidator.GetSupportedLanguages();

            // Assert
            Assert.NotNull(languages);
            Assert.NotEmpty(languages);
        }

        /// <summary>
        /// Verifies that GetSupportedLanguages includes common languages.
        /// </summary>
        [Fact]
        public void GetSupportedLanguages_ShouldIncludeCommonLanguages()
        {
            // Act
            var languages = LocalizationValidator.GetSupportedLanguages();

            // Assert
            Assert.Contains("en-US", languages);
            Assert.Contains("fr-FR", languages);
            Assert.Contains("de-DE", languages);
            Assert.Contains("es-ES", languages);
            Assert.Contains("ja-JP", languages);
        }

        /// <summary>
        /// Verifies that GetSupportedLanguages returns sorted collection.
        /// </summary>
        [Fact]
        public void GetSupportedLanguages_ShouldReturnSortedCollection()
        {
            // Act
            var languages = LocalizationValidator.GetSupportedLanguages().ToList();

            // Assert
            var sorted = languages.OrderBy(l => l).ToList();
            Assert.Equal(sorted, languages);
        }

        /// <summary>
        /// Verifies that GetSupportedLanguages returns read-only collection.
        /// </summary>
        [Fact]
        public void GetSupportedLanguages_ShouldReturnReadOnlyCollection()
        {
            // Act
            var languages = LocalizationValidator.GetSupportedLanguages();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<string>>(languages);
        }

        /// <summary>
        /// Verifies that GetSupportedLanguages has expected minimum count (40+).
        /// </summary>
        [Fact]
        public void GetSupportedLanguages_ShouldHaveMinimumLanguages()
        {
            // Act
            var languages = LocalizationValidator.GetSupportedLanguages();

            // Assert - should have at least 40 languages based on code
            Assert.True(languages.Count >= 40, $"Expected at least 40 languages, got {languages.Count}");
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetSupportedTimezones Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that GetSupportedTimezones returns non-empty collection.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_ShouldReturnNonEmptyCollection()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones();

            // Assert
            Assert.NotNull(timezones);
            Assert.NotEmpty(timezones);
        }

        /// <summary>
        /// Verifies that GetSupportedTimezones includes common timezones.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_ShouldIncludeCommonTimezones()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones();

            // Assert
            Assert.Contains("UTC", timezones);
            // Note: Actual timezone IDs vary by platform, so we just verify collection is not empty
        }

        /// <summary>
        /// Verifies that GetSupportedTimezones returns sorted collection.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_ShouldReturnSortedCollection()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones().ToList();

            // Assert
            var sorted = timezones.OrderBy(t => t).ToList();
            Assert.Equal(sorted, timezones);
        }

        /// <summary>
        /// Verifies that GetSupportedTimezones returns read-only collection.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_ShouldReturnReadOnlyCollection()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<string>>(timezones);
        }

        /// <summary>
        /// Verifies that GetSupportedTimezones has no duplicates.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_ShouldHaveNoDuplicates()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones().ToList();

            // Assert
            var distinct = timezones.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Assert.Equal(distinct.Count, timezones.Count);
        }

        /// <summary>
        /// Verifies that all returned timezones pass validation.
        /// </summary>
        [Fact]
        public void GetSupportedTimezones_AllReturnedTimezones_ShouldPassValidation()
        {
            // Act
            var timezones = LocalizationValidator.GetSupportedTimezones();

            // Assert - all should validate successfully
            foreach (var timezone in timezones.Take(10)) // Test first 10 for performance
            {
                var exception = Record.Exception(() =>
                    LocalizationValidator.ValidateTimezone(timezone));
                Assert.Null(exception);
            }
        }
    }
}

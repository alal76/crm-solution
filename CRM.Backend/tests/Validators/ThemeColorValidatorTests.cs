// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using CRM.Infrastructure.Validation;
using Xunit;

namespace CRM.Tests.Validators
{
    /// <summary>
    /// Comprehensive unit tests for <see cref="ThemeColorValidator"/>.
    /// Tests validation of hexadecimal color codes for theme customization.
    /// </summary>
    public class ThemeColorValidatorTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateHexColor - Valid Colors Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that valid 6-digit hex colors pass validation.
        /// </summary>
        [Theory]
        [InlineData("#000000")]
        [InlineData("#FFFFFF")]
        [InlineData("#6750A4")]
        [InlineData("#FF5733")]
        [InlineData("#00FF00")]
        [InlineData("#0000FF")]
        [InlineData("#ABCDEF")]
        [InlineData("#123456")]
        public void ValidateHexColor_ShouldNotThrow_WhenValid6DigitHex(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that valid 3-digit hex colors pass validation.
        /// </summary>
        [Theory]
        [InlineData("#000")]
        [InlineData("#FFF")]
        [InlineData("#F00")]
        [InlineData("#0F0")]
        [InlineData("#00F")]
        [InlineData("#ABC")]
        [InlineData("#123")]
        public void ValidateHexColor_ShouldNotThrow_WhenValid3DigitHex(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that hex colors with lowercase letters pass validation.
        /// </summary>
        [Theory]
        [InlineData("#abcdef")]
        [InlineData("#ff5733")]
        [InlineData("#6750a4")]
        [InlineData("#abc")]
        [InlineData("#f0f")]
        public void ValidateHexColor_ShouldNotThrow_WhenLowercaseHex(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that hex colors with mixed case pass validation.
        /// </summary>
        [Theory]
        [InlineData("#AbCdEf")]
        [InlineData("#Ff5733")]
        [InlineData("#6750A4")]
        [InlineData("#AbC")]
        [InlineData("#F0f")]
        public void ValidateHexColor_ShouldNotThrow_WhenMixedCaseHex(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that hex colors with leading/trailing whitespace pass validation (trimmed).
        /// </summary>
        [Theory]
        [InlineData(" #6750A4")]
        [InlineData("#6750A4 ")]
        [InlineData("  #6750A4  ")]
        [InlineData("\t#6750A4\t")]
        public void ValidateHexColor_ShouldNotThrow_WhenHexWithWhitespace(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateHexColor - Null/Empty Input Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that null color is allowed (optional colors).
        /// </summary>
        [Fact]
        public void ValidateHexColor_ShouldNotThrow_WhenNull()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(null, "Test Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that empty string is allowed (optional colors).
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateHexColor_ShouldNotThrow_WhenEmptyOrWhitespace(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateHexColor - Invalid Format Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that hex color without # prefix throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("6750A4")]
        [InlineData("FFFFFF")]
        [InlineData("000")]
        [InlineData("ABC")]
        public void ValidateHexColor_ShouldThrow_WhenMissingHashPrefix(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
            Assert.Contains("hex", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that colors with invalid length throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#1")]
        [InlineData("#12")]
        [InlineData("#1234")]
        [InlineData("#12345")]
        [InlineData("#1234567")]
        public void ValidateHexColor_ShouldThrow_WhenInvalidLength(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that colors with non-hex characters throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#GGGGGG")]
        [InlineData("#XYZ")]
        [InlineData("#12345G")]
        [InlineData("#ZZZ")]
        [InlineData("#ZZZZZ1")]
        public void ValidateHexColor_ShouldThrow_WhenNonHexCharacters(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that colors with special characters throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#FF-FF-FF")]
        [InlineData("#FF FF FF")]
        [InlineData("#FF,FF,FF")]
        [InlineData("#(FFFFFF)")]
        public void ValidateHexColor_ShouldThrow_WhenSpecialCharacters(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that RGB/RGBA format throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("rgb(255, 0, 0)")]
        [InlineData("rgba(255, 0, 0, 1)")]
        [InlineData("rgb(0,0,0)")]
        public void ValidateHexColor_ShouldThrow_WhenRgbFormat(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that color names throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("red")]
        [InlineData("blue")]
        [InlineData("green")]
        [InlineData("black")]
        [InlineData("white")]
        public void ValidateHexColor_ShouldThrow_WhenColorName(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateHexColor - Custom Field Name Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that custom field name appears in error message.
        /// </summary>
        [Fact]
        public void ValidateHexColor_ShouldIncludeFieldName_InErrorMessage()
        {
            // Arrange
            var invalidColor = "invalid";
            var fieldName = "Primary Brand Color";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(invalidColor, fieldName));

            Assert.Contains(fieldName, exception.Message);
        }

        /// <summary>
        /// Verifies that error message provides example format.
        /// </summary>
        [Fact]
        public void ValidateHexColor_ShouldProvideExample_InErrorMessage()
        {
            // Arrange
            var invalidColor = "invalid";
            var fieldName = "Background Color";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(invalidColor, fieldName));

            Assert.Contains("#6750A4", exception.Message);
        }

        /// <summary>
        /// Verifies that different field names work correctly.
        /// </summary>
        [Theory]
        [InlineData("Primary Color")]
        [InlineData("Secondary Color")]
        [InlineData("Accent Color")]
        [InlineData("Background Color")]
        [InlineData("Text Color")]
        public void ValidateHexColor_ShouldUseProvidedFieldName(string fieldName)
        {
            // Arrange
            var invalidColor = "notahexcolor";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(invalidColor, fieldName));

            Assert.Contains(fieldName, exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateHexColor - Edge Cases Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that multiple # symbols throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("##FFFFFF")]
        [InlineData("#FF#FFFF")]
        [InlineData("###FFF")]
        public void ValidateHexColor_ShouldThrow_WhenMultipleHashSymbols(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that # symbol in middle/end throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("FF#FFFF")]
        [InlineData("FFFFFF#")]
        public void ValidateHexColor_ShouldThrow_WhenHashNotAtStart(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that whitespace within hex code throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#FF FF FF")]
        [InlineData("#F F F")]
        [InlineData("#FF F FFF")]
        public void ValidateHexColor_ShouldThrow_WhenWhitespaceInside(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that 8-digit hex (with alpha) throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#FFFFFFFF")]
        [InlineData("#00000000")]
        [InlineData("#6750A4FF")]
        public void ValidateHexColor_ShouldThrow_When8DigitHex(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that common theme colors pass validation.
        /// </summary>
        [Theory]
        [InlineData("#6750A4")] // Material Design primary
        [InlineData("#625B71")] // Material Design secondary
        [InlineData("#7D5260")] // Material Design tertiary
        [InlineData("#E8DEF8")] // Material Design primary container
        [InlineData("#E8E0EB")] // Material Design secondary container
        public void ValidateHexColor_ShouldNotThrow_WhenCommonMaterialColors(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Theme Color"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that validation rejects URL-encoded hex colors.
        /// </summary>
        [Theory]
        [InlineData("%236750A4")]
        [InlineData("%23FFF")]
        public void ValidateHexColor_ShouldThrow_WhenUrlEncoded(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that Unicode characters in color throw ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("#FF™FF")]
        [InlineData("#F€F")]
        [InlineData("#¡¡¡")]
        public void ValidateHexColor_ShouldThrow_WhenUnicodeCharacters(string color)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Contains("Invalid", exception.Message);
        }

        /// <summary>
        /// Verifies that boundary values (all 0s and all Fs) are valid.
        /// </summary>
        [Theory]
        [InlineData("#000000")] // Black
        [InlineData("#FFFFFF")] // White
        [InlineData("#000")]    // Black (short)
        [InlineData("#FFF")]    // White (short)
        public void ValidateHexColor_ShouldNotThrow_WhenBoundaryColors(string color)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                ThemeColorValidator.ValidateHexColor(color, "Test Color"));

            Assert.Null(exception);
        }
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using CRM.Core.Validation;
using Xunit;

namespace CRM.Tests.Validators
{
    /// <summary>
    /// Comprehensive unit tests for <see cref="UiConfigurationValidator"/>.
    /// Tests validation of module names, navigation keys, unique keys, and order values.
    /// </summary>
    public class UiConfigurationValidatorTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateModuleName Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that valid default module names pass validation.
        /// </summary>
        [Theory]
        [InlineData("Dashboard")]
        [InlineData("Accounts")]
        [InlineData("Contacts")]
        [InlineData("Leads")]
        [InlineData("Opportunities")]
        [InlineData("Products")]
        [InlineData("Services")]
        [InlineData("Campaigns")]
        [InlineData("Quotes")]
        [InlineData("Tasks")]
        [InlineData("Activities")]
        [InlineData("Notes")]
        [InlineData("Workflows")]
        [InlineData("Reports")]
        public void ValidateModuleName_ShouldNotThrow_WhenValidDefaultModuleName(string moduleName)
        {
            // Act & Assert
            var exception = Record.Exception(() => UiConfigurationValidator.ValidateModuleName(moduleName));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that custom module names are allowed by default.
        /// </summary>
        [Theory]
        [InlineData("CustomModule")]
        [InlineData("MyCustomDashboard")]
        [InlineData("Integration_Module")]
        public void ValidateModuleName_ShouldNotThrow_WhenCustomModuleAndAllowCustomTrue(string moduleName)
        {
            // Act & Assert
            var exception = Record.Exception(() => UiConfigurationValidator.ValidateModuleName(moduleName, allowCustom: true));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that custom module names are rejected when allowCustom is false.
        /// </summary>
        [Theory]
        [InlineData("CustomModule")]
        [InlineData("UnknownModule")]
        [InlineData("NotInList")]
        public void ValidateModuleName_ShouldThrow_WhenCustomModuleAndAllowCustomFalse(string moduleName)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateModuleName(moduleName, allowCustom: false));

            Assert.Contains("not recognized", exception.Message);
            Assert.Contains(moduleName, exception.Message);
        }

        /// <summary>
        /// Verifies that null module name throws ArgumentException.
        /// </summary>
        [Fact]
        public void ValidateModuleName_ShouldThrow_WhenNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateModuleName(null));

            Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that empty module name throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateModuleName_ShouldThrow_WhenEmptyOrWhitespace(string moduleName)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateModuleName(moduleName));

            Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that module names exceeding max length throw ArgumentException.
        /// </summary>
        [Fact]
        public void ValidateModuleName_ShouldThrow_WhenTooLong()
        {
            // Arrange - 101 characters
            var longModuleName = new string('A', 101);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateModuleName(longModuleName));

            Assert.Contains("too long", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that module name at max length (100 chars) passes validation.
        /// </summary>
        [Fact]
        public void ValidateModuleName_ShouldNotThrow_WhenMaxLength()
        {
            // Arrange - exactly 100 characters
            var maxLengthModuleName = new string('A', 100);

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.ValidateModuleName(maxLengthModuleName));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that module name validation is case-insensitive for default modules.
        /// </summary>
        [Theory]
        [InlineData("dashboard")]
        [InlineData("ACCOUNTS")]
        [InlineData("CoNtAcTs")]
        public void ValidateModuleName_ShouldNotThrow_WhenCaseVariesForDefaultModules(string moduleName)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.ValidateModuleName(moduleName, allowCustom: false));

            Assert.Null(exception);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateNavigationKey Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that valid navigation keys pass validation.
        /// </summary>
        [Theory]
        [InlineData("main-nav")]
        [InlineData("sidebar")]
        [InlineData("top-menu")]
        [InlineData("user-settings")]
        public void ValidateNavigationKey_ShouldNotThrow_WhenValidKey(string key)
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.ValidateNavigationKey(key));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that null navigation key throws ArgumentException.
        /// </summary>
        [Fact]
        public void ValidateNavigationKey_ShouldThrow_WhenNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateNavigationKey(null));

            Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that empty navigation key throws ArgumentException.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void ValidateNavigationKey_ShouldThrow_WhenEmptyOrWhitespace(string key)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateNavigationKey(key));

            Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that custom field name is used in error message.
        /// </summary>
        [Fact]
        public void ValidateNavigationKey_ShouldUseCustomFieldName_InErrorMessage()
        {
            // Arrange
            var customFieldName = "Menu Item ID";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateNavigationKey("", customFieldName));

            Assert.Contains(customFieldName, exception.Message);
        }

        /// <summary>
        /// Verifies that default field name is used when not specified.
        /// </summary>
        [Fact]
        public void ValidateNavigationKey_ShouldUseDefaultFieldName_WhenNotSpecified()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.ValidateNavigationKey(""));

            Assert.Contains("Navigation key", exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EnsureUniqueKeys Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that unique keys pass validation.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldNotThrow_WhenAllKeysUnique()
        {
            // Arrange
            var keys = new List<string> { "alpha", "beta", "gamma", "delta" };

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "Test Key"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that duplicate keys throw ArgumentException.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldThrow_WhenDuplicateKeysExist()
        {
            // Arrange
            var keys = new List<string> { "alpha", "beta", "alpha" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "Test Key"));

            Assert.Contains("unique", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("alpha", exception.Message);
        }

        /// <summary>
        /// Verifies that duplicate detection is case-insensitive.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldThrow_WhenDuplicateKeysWithDifferentCase()
        {
            // Arrange
            var keys = new List<string> { "Alpha", "beta", "ALPHA", "gamma" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "MenuItem Key"));

            Assert.Contains("unique", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Alpha", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that multiple duplicate keys are all reported.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldReportAllDuplicates_WhenMultipleDuplicatesExist()
        {
            // Arrange
            var keys = new List<string> { "alpha", "beta", "alpha", "gamma", "beta" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "Widget Key"));

            Assert.Contains("alpha", exception.Message);
            Assert.Contains("beta", exception.Message);
        }

        /// <summary>
        /// Verifies that null/empty keys are ignored (don't cause false duplicates).
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldIgnoreNullOrEmptyKeys()
        {
            // Arrange
            var keys = new List<string> { "alpha", "", "beta", null, "gamma", "  " };

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "Key"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that empty key collection passes validation.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldNotThrow_WhenEmptyCollection()
        {
            // Arrange
            var keys = new List<string>();

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, "Key"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that custom field name appears in error message.
        /// </summary>
        [Fact]
        public void EnsureUniqueKeys_ShouldIncludeFieldName_InErrorMessage()
        {
            // Arrange
            var keys = new List<string> { "dup", "dup" };
            var fieldName = "Dashboard Widget ID";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureUniqueKeys(keys, fieldName));

            Assert.Contains(fieldName, exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EnsureNonNegativeOrders Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that all non-negative orders pass validation.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldNotThrow_WhenAllOrdersNonNegative()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("item1", 0),
                ("item2", 1),
                ("item3", 10),
                ("item4", 100)
            };

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Item Order"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that negative order throws ArgumentException.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldThrow_WhenAnyOrderNegative()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("item1", 0),
                ("item2", -1),
                ("item3", 5)
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "MenuItem Order"));

            Assert.Contains("non-negative", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("item2", exception.Message);
        }

        /// <summary>
        /// Verifies that multiple negative orders are all reported.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldReportAllInvalidItems_WhenMultipleNegative()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("item1", 5),
                ("item2", -1),
                ("item3", 0),
                ("item4", -10),
                ("item5", 20)
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Order"));

            Assert.Contains("item2", exception.Message);
            Assert.Contains("item4", exception.Message);
        }

        /// <summary>
        /// Verifies that zero order is considered valid.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldNotThrow_WhenOrderIsZero()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("item1", 0),
                ("item2", 0),
                ("item3", 0)
            };

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Order"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that empty collection passes validation.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldNotThrow_WhenEmptyCollection()
        {
            // Arrange
            var items = new List<(string Id, int Order)>();

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Order"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that custom field name appears in error message.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldIncludeFieldName_InErrorMessage()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("widget1", -5)
            };
            var fieldName = "Widget Display Order";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, fieldName));

            Assert.Contains(fieldName, exception.Message);
        }

        /// <summary>
        /// Verifies that large positive orders are valid.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldNotThrow_WhenLargePositiveOrders()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("item1", int.MaxValue),
                ("item2", 1000000),
                ("item3", 999999)
            };

            // Act & Assert
            var exception = Record.Exception(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Order"));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that mixed valid/invalid orders correctly identifies invalids.
        /// </summary>
        [Fact]
        public void EnsureNonNegativeOrders_ShouldIdentifyOnlyNegatives_WhenMixedOrders()
        {
            // Arrange
            var items = new List<(string Id, int Order)>
            {
                ("good-a", 10),
                ("bad-x", -5),
                ("good-b", 0),
                ("bad-y", -1),
                ("good-c", 100)
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                UiConfigurationValidator.EnsureNonNegativeOrders(items, "Order"));

            Assert.Contains("bad-x", exception.Message);
            Assert.Contains("bad-y", exception.Message);
            Assert.DoesNotContain("good-a", exception.Message);
            Assert.DoesNotContain("good-b", exception.Message);
            Assert.DoesNotContain("good-c", exception.Message);
        }
    }
}

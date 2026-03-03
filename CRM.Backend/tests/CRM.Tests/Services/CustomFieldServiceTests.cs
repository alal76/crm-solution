// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CustomFieldValidationService.
/// Verifies required, regex, and dropdown validation rules.
/// </summary>
public class CustomFieldServiceTests : ServiceTestFixtureBase<CustomFieldValidationService>
{    public CustomFieldServiceTests()
    {    }

    /// <summary>
    /// Helper: serialises an anonymous definition into the format stored by the service.
    /// </summary>
    private static string SerializeDef(object definition) =>
        JsonSerializer.Serialize(definition);

    private CustomFieldValidationService BuildService(IEnumerable<CustomField> customFields)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(new List<CustomField>(customFields));
        MockContext.Setup(c => c.CustomFields).Returns(mockSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new CustomFieldValidationService(MockContext.Object, MockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – required field with empty value
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ShouldReturnInvalid_WhenRequiredFieldIsEmpty()
    {
        // Arrange
        var defJson = SerializeDef(new
        {
            EntityType = "Account",
            FieldKey = "Phone",
            Label = "Phone",
            DataType = 0, // Text
            IsRequired = true,
            IsActive = true,
            Options = new string[0],
            SortOrder = 0
        });

        var customField = new CustomField
        {
            Id = 1,
            EntityType = "Account",
            Key = "__def__Phone",
            Value = defJson,
            IsDeleted = false
        };

        var service = BuildService(new[] { customField });

        // Act
        var result = await service.ValidateAsync("Account", "Phone", string.Empty);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – regex pattern with non-matching value
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ShouldReturnInvalid_WhenValueDoesNotMatchRegex()
    {
        // Arrange
        var defJson = SerializeDef(new
        {
            EntityType = "Account",
            FieldKey = "ZipCode",
            Label = "Zip Code",
            DataType = 0, // Text
            IsRequired = false,
            IsActive = true,
            RegexPattern = @"^\d{5}$",
            RegexErrorMessage = "Zip Code must be exactly 5 digits.",
            Options = new string[0],
            SortOrder = 0
        });

        var customField = new CustomField
        {
            Id = 2,
            EntityType = "Account",
            Key = "__def__ZipCode",
            Value = defJson,
            IsDeleted = false
        };

        var service = BuildService(new[] { customField });

        // Act
        var result = await service.ValidateAsync("Account", "ZipCode", "ABCDE");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Zip Code must be exactly 5 digits.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 3 – dropdown value not in allowed options
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ShouldReturnInvalid_WhenDropdownValueNotInOptions()
    {
        // Arrange
        var defJson = SerializeDef(new
        {
            EntityType = "Account",
            FieldKey = "CompanySize",
            Label = "Company Size",
            DataType = 3, // Dropdown
            IsRequired = false,
            IsActive = true,
            Options = new[] { "Small", "Medium", "Large" },
            SortOrder = 0
        });

        var customField = new CustomField
        {
            Id = 3,
            EntityType = "Account",
            Key = "__def__CompanySize",
            Value = defJson,
            IsDeleted = false
        };

        var service = BuildService(new[] { customField });

        // Act
        var result = await service.ValidateAsync("Account", "CompanySize", "XLarge");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Small") || e.Contains("Medium") || e.Contains("Large"));
    }
}

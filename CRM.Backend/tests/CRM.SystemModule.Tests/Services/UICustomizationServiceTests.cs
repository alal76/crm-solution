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

using CRM.Core.Entities;
using Xunit;

namespace CRM.SystemModule.Tests.Services;

/// <summary>
/// Unit tests for UI Customization functionality.
/// Note: UICustomizationService is planned but not yet implemented.
/// These tests validate the SystemSettings entity which stores UI customization data.
/// </summary>
public class UICustomizationServiceTests
{
    [Fact]
    public void SystemSettings_CanStoreBrandingPreferences()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            Id = 1,
            CompanyName = "Test Company",
            PrimaryColor = "#6750A4",
            SecondaryColor = "#625B71"
        };

        // Assert
        Assert.Equal("#6750A4", settings.PrimaryColor);
        Assert.Equal("#625B71", settings.SecondaryColor);
    }

    [Fact]
    public void SystemSettings_SupportsDefaultValues()
    {
        // Arrange
        var settings = new SystemSettings();

        // Act & Assert - Verify defaults are set
        Assert.Equal("CRM System", settings.CompanyName);
        Assert.NotEmpty(settings.PrimaryColor);
    }

    [Fact]
    public void SystemSettings_ColorValues_AreValidHexFormat()
    {
        // Arrange
        var validColors = new[] { "#6750A4", "#625B71", "#7D5260", "#FFFFFF", "#000000" };

        // Act & Assert
        foreach (var color in validColors)
        {
            var settings = new SystemSettings { PrimaryColor = color };
            Assert.StartsWith("#", settings.PrimaryColor);
            Assert.Equal(7, settings.PrimaryColor.Length);
        }
    }

    [Fact]
    public void SystemSettings_CompanyLogo_CanBeSet()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            CompanyLogoUrl = "https://example.com/logo.png",
            CompanyLoginLogoUrl = "https://example.com/login-logo.png"
        };

        // Assert
        Assert.NotEmpty(settings.CompanyLogoUrl!);
        Assert.NotEmpty(settings.CompanyLoginLogoUrl!);
    }

    [Fact]
    public void SystemSettings_CompanyCustomization_PropertiesExist()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            CompanyName = "Acme Corp",
            CompanyLogoUrl = "https://example.com/logo.png",
            CompanyEmail = "contact@acme.com",
            CompanyPhone = "+1 555-1234"
        };

        // Assert
        Assert.NotEmpty(settings.CompanyName);
        Assert.NotEmpty(settings.CompanyLogoUrl!);
        Assert.NotEmpty(settings.CompanyEmail!);
        Assert.NotEmpty(settings.CompanyPhone!);
    }

    [Fact]
    public void SystemSettings_UseGroupHeaderColor_CanBeToggled()
    {
        // Arrange & Act
        var settingsEnabled = new SystemSettings { UseGroupHeaderColor = true };
        var settingsDisabled = new SystemSettings { UseGroupHeaderColor = false };

        // Assert
        Assert.True(settingsEnabled.UseGroupHeaderColor);
        Assert.False(settingsDisabled.UseGroupHeaderColor);
    }
}

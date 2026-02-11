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
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for SettingsController
/// Covers: System settings, lookups, modules, custom fields, preferences
/// </summary>
public class SettingsControllerTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILogger<SettingsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _mockLogger = new Mock<ILogger<SettingsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new SettingsController(_mockSettingsService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region System Settings Tests

    [Fact]
    public async Task GetAllSettings_ReturnsOkResult_WithSettings()
    {
        // Arrange
        var settings = new List<SystemSettingDto>
        {
            new SystemSettingDto { Key = "AppName", Value = "CRM Solution", Category = "General" },
            new SystemSettingDto { Key = "DefaultLanguage", Value = "en-US", Category = "Localization" }
        };

        _mockSettingsService.Setup(s => s.GetAllSettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var result = await _controller.GetAllSettings();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSettings = okResult.Value as IEnumerable<SystemSettingDto>;
        returnedSettings.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSettingByKey_ExistingSetting_ReturnsOk()
    {
        // Arrange
        var setting = new SystemSettingDto { Key = "AppName", Value = "CRM Solution" };

        _mockSettingsService.Setup(s => s.GetSettingByKeyAsync("AppName"))
            .ReturnsAsync(setting);

        // Act
        var result = await _controller.GetSettingByKey("AppName");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetSettingByKey_NonExisting_ReturnsNotFound()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.GetSettingByKeyAsync("NonExistent"))
            .ReturnsAsync((SystemSettingDto?)null);

        // Act
        var result = await _controller.GetSettingByKey("NonExistent");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSettingsByCategory_ReturnsFilteredSettings()
    {
        // Arrange
        var settings = new List<SystemSettingDto>
        {
            new SystemSettingDto { Key = "Key1", Category = "Security" }
        };

        _mockSettingsService.Setup(s => s.GetSettingsByCategoryAsync("Security"))
            .ReturnsAsync(settings);

        // Act
        var result = await _controller.GetSettingsByCategory("Security");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateSetting_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateSystemSettingDto
        {
            Key = "AppName",
            Value = "Updated CRM"
        };

        var updatedSetting = new SystemSettingDto { Key = "AppName", Value = "Updated CRM" };

        _mockSettingsService.Setup(s => s.UpdateSettingAsync(updateDto))
            .ReturnsAsync(updatedSetting);

        // Act
        var result = await _controller.UpdateSetting("AppName", updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateSetting_KeyMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateSystemSettingDto { Key = "DifferentKey" };

        // Act
        var result = await _controller.UpdateSetting("AppName", updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateSetting_ReadOnlySetting_ReturnsConflict()
    {
        // Arrange
        var updateDto = new UpdateSystemSettingDto { Key = "ReadOnlySetting" };

        _mockSettingsService.Setup(s => s.UpdateSettingAsync(updateDto))
            .ThrowsAsync(new InvalidOperationException("Setting is read-only"));

        // Act
        var result = await _controller.UpdateSetting("ReadOnlySetting", updateDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task BulkUpdateSettings_ValidData_ReturnsOk()
    {
        // Arrange
        var updates = new List<UpdateSystemSettingDto>
        {
            new UpdateSystemSettingDto { Key = "Key1", Value = "Value1" },
            new UpdateSystemSettingDto { Key = "Key2", Value = "Value2" }
        };

        _mockSettingsService.Setup(s => s.BulkUpdateSettingsAsync(updates))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkUpdateSettings(updates);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task ResetToDefaults_ValidCategory_ReturnsOk()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.ResetToDefaultsAsync("General"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetToDefaults("General");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Lookup Categories Tests

    [Fact]
    public async Task GetLookupCategories_ReturnsCategories()
    {
        // Arrange
        var categories = new List<LookupCategoryDto>
        {
            new LookupCategoryDto { Id = 1, Name = "Industry" },
            new LookupCategoryDto { Id = 2, Name = "LeadSource" }
        };

        _mockSettingsService.Setup(s => s.GetLookupCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetLookupCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetLookupCategoryById_ExistingCategory_ReturnsOk()
    {
        // Arrange
        var category = new LookupCategoryDto { Id = 1, Name = "Industry" };

        _mockSettingsService.Setup(s => s.GetLookupCategoryByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetLookupCategoryById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateLookupCategory_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateLookupCategoryDto
        {
            Name = "NewCategory",
            Description = "Description"
        };

        var createdCategory = new LookupCategoryDto { Id = 1, Name = "NewCategory" };

        _mockSettingsService.Setup(s => s.CreateLookupCategoryAsync(createDto))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _controller.CreateLookupCategory(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateLookupCategory_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateLookupCategoryDto { Name = "ExistingCategory" };

        _mockSettingsService.Setup(s => s.CreateLookupCategoryAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Category already exists"));

        // Act
        var result = await _controller.CreateLookupCategory(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UpdateLookupCategory_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateLookupCategoryDto
        {
            Id = 1,
            Name = "UpdatedCategory"
        };

        var updatedCategory = new LookupCategoryDto { Id = 1, Name = "UpdatedCategory" };

        _mockSettingsService.Setup(s => s.UpdateLookupCategoryAsync(updateDto))
            .ReturnsAsync(updatedCategory);

        // Act
        var result = await _controller.UpdateLookupCategory(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task DeleteLookupCategory_ExistingCategory_ReturnsNoContent()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.DeleteLookupCategoryAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLookupCategory(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteLookupCategory_HasItems_ReturnsConflict()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.DeleteLookupCategoryAsync(1))
            .ThrowsAsync(new InvalidOperationException("Category has items"));

        // Act
        var result = await _controller.DeleteLookupCategory(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Lookup Items Tests

    [Fact]
    public async Task GetLookupItems_ValidCategory_ReturnsItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Name = "Technology", CategoryId = 1 },
            new LookupItemDto { Id = 2, Name = "Healthcare", CategoryId = 1 }
        };

        _mockSettingsService.Setup(s => s.GetLookupItemsByCategoryAsync(1))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetLookupItems(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetLookupItemsByCategoryName_ReturnsItems()
    {
        // Arrange
        var items = new List<LookupItemDto>
        {
            new LookupItemDto { Id = 1, Name = "Technology" }
        };

        _mockSettingsService.Setup(s => s.GetLookupItemsByCategoryNameAsync("Industry"))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetLookupItemsByCategoryName("Industry");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateLookupItem_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateLookupItemDto
        {
            CategoryId = 1,
            Name = "Finance",
            Value = "finance"
        };

        var createdItem = new LookupItemDto { Id = 1, Name = "Finance" };

        _mockSettingsService.Setup(s => s.CreateLookupItemAsync(createDto))
            .ReturnsAsync(createdItem);

        // Act
        var result = await _controller.CreateLookupItem(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateLookupItem_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateLookupItemDto
        {
            Id = 1,
            Name = "Updated Industry"
        };

        var updatedItem = new LookupItemDto { Id = 1, Name = "Updated Industry" };

        _mockSettingsService.Setup(s => s.UpdateLookupItemAsync(updateDto))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateLookupItem(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task DeleteLookupItem_ExistingItem_ReturnsNoContent()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.DeleteLookupItemAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLookupItem(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ReorderLookupItems_ValidRequest_ReturnsOk()
    {
        // Arrange
        var reorderRequest = new ReorderLookupItemsRequest
        {
            CategoryId = 1,
            ItemIds = new List<int> { 2, 1, 3 }
        };

        _mockSettingsService.Setup(s => s.ReorderLookupItemsAsync(reorderRequest))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReorderLookupItems(reorderRequest);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Module Configuration Tests

    [Fact]
    public async Task GetModuleConfigurations_ReturnsConfigurations()
    {
        // Arrange
        var configs = new List<ModuleConfigurationDto>
        {
            new ModuleConfigurationDto { ModuleName = "Leads", IsEnabled = true },
            new ModuleConfigurationDto { ModuleName = "Campaigns", IsEnabled = false }
        };

        _mockSettingsService.Setup(s => s.GetModuleConfigurationsAsync())
            .ReturnsAsync(configs);

        // Act
        var result = await _controller.GetModuleConfigurations();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetModuleConfiguration_ExistingModule_ReturnsOk()
    {
        // Arrange
        var config = new ModuleConfigurationDto { ModuleName = "Leads", IsEnabled = true };

        _mockSettingsService.Setup(s => s.GetModuleConfigurationAsync("Leads"))
            .ReturnsAsync(config);

        // Act
        var result = await _controller.GetModuleConfiguration("Leads");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateModuleConfiguration_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateModuleConfigurationDto
        {
            ModuleName = "Leads",
            IsEnabled = true,
            Settings = new Dictionary<string, string> { { "DefaultOwner", "1" } }
        };

        var updatedConfig = new ModuleConfigurationDto { ModuleName = "Leads", IsEnabled = true };

        _mockSettingsService.Setup(s => s.UpdateModuleConfigurationAsync(updateDto))
            .ReturnsAsync(updatedConfig);

        // Act
        var result = await _controller.UpdateModuleConfiguration("Leads", updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task EnableModule_ValidModule_ReturnsOk()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.EnableModuleAsync("Campaigns"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EnableModule("Campaigns");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DisableModule_ValidModule_ReturnsOk()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.DisableModuleAsync("Campaigns"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DisableModule("Campaigns");

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Custom Fields Tests

    [Fact]
    public async Task GetCustomFieldDefinitions_ReturnsDefinitions()
    {
        // Arrange
        var definitions = new List<CustomFieldDefinitionDto>
        {
            new CustomFieldDefinitionDto { Id = 1, FieldName = "CustomField1", EntityType = "Account" }
        };

        _mockSettingsService.Setup(s => s.GetCustomFieldDefinitionsAsync())
            .ReturnsAsync(definitions);

        // Act
        var result = await _controller.GetCustomFieldDefinitions();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetCustomFieldsByEntity_ReturnsFilteredFields()
    {
        // Arrange
        var fields = new List<CustomFieldDefinitionDto>
        {
            new CustomFieldDefinitionDto { Id = 1, EntityType = "Account" }
        };

        _mockSettingsService.Setup(s => s.GetCustomFieldsByEntityAsync("Account"))
            .ReturnsAsync(fields);

        // Act
        var result = await _controller.GetCustomFieldsByEntity("Account");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task CreateCustomFieldDefinition_ValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateCustomFieldDefinitionDto
        {
            FieldName = "CustomField1",
            FieldType = "Text",
            EntityType = "Account"
        };

        var createdField = new CustomFieldDefinitionDto { Id = 1, FieldName = "CustomField1" };

        _mockSettingsService.Setup(s => s.CreateCustomFieldDefinitionAsync(createDto))
            .ReturnsAsync(createdField);

        // Act
        var result = await _controller.CreateCustomFieldDefinition(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateCustomFieldDefinition_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateCustomFieldDefinitionDto { FieldName = "ExistingField" };

        _mockSettingsService.Setup(s => s.CreateCustomFieldDefinitionAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Field already exists"));

        // Act
        var result = await _controller.CreateCustomFieldDefinition(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task UpdateCustomFieldDefinition_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateCustomFieldDefinitionDto
        {
            Id = 1,
            Label = "Updated Label"
        };

        var updatedField = new CustomFieldDefinitionDto { Id = 1, Label = "Updated Label" };

        _mockSettingsService.Setup(s => s.UpdateCustomFieldDefinitionAsync(updateDto))
            .ReturnsAsync(updatedField);

        // Act
        var result = await _controller.UpdateCustomFieldDefinition(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task DeleteCustomFieldDefinition_ExistingField_ReturnsNoContent()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.DeleteCustomFieldDefinitionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomFieldDefinition(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region User Preferences Tests

    [Fact]
    public async Task GetUserPreferences_ReturnsCurrentUserPreferences()
    {
        // Arrange
        var preferences = new UserPreferencesDto
        {
            UserId = 1,
            Theme = "dark",
            Language = "en-US"
        };

        _mockSettingsService.Setup(s => s.GetUserPreferencesAsync(1))
            .ReturnsAsync(preferences);

        // Act
        var result = await _controller.GetUserPreferences();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateUserPreferences_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateUserPreferencesDto
        {
            Theme = "light",
            Language = "es-ES"
        };

        var updatedPreferences = new UserPreferencesDto { Theme = "light", Language = "es-ES" };

        _mockSettingsService.Setup(s => s.UpdateUserPreferencesAsync(1, updateDto))
            .ReturnsAsync(updatedPreferences);

        // Act
        var result = await _controller.UpdateUserPreferences(updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task ResetUserPreferences_ReturnsOk()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.ResetUserPreferencesAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetUserPreferences();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Email Settings Tests

    [Fact]
    public async Task GetEmailSettings_ReturnsSettings()
    {
        // Arrange
        var settings = new EmailSettingsDto
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            EnableSsl = true
        };

        _mockSettingsService.Setup(s => s.GetEmailSettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var result = await _controller.GetEmailSettings();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateEmailSettings_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateEmailSettingsDto
        {
            SmtpHost = "smtp.newhost.com",
            SmtpPort = 465
        };

        var updatedSettings = new EmailSettingsDto { SmtpHost = "smtp.newhost.com" };

        _mockSettingsService.Setup(s => s.UpdateEmailSettingsAsync(updateDto))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _controller.UpdateEmailSettings(updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task TestEmailSettings_ValidData_ReturnsOk()
    {
        // Arrange
        var testDto = new TestEmailSettingsDto
        {
            RecipientEmail = "test@example.com"
        };

        _mockSettingsService.Setup(s => s.TestEmailSettingsAsync(testDto))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestEmailSettings(testDto);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task TestEmailSettings_FailedDelivery_ReturnsBadRequest()
    {
        // Arrange
        var testDto = new TestEmailSettingsDto { RecipientEmail = "invalid@test.com" };

        _mockSettingsService.Setup(s => s.TestEmailSettingsAsync(testDto))
            .ThrowsAsync(new InvalidOperationException("SMTP connection failed"));

        // Act
        var result = await _controller.TestEmailSettings(testDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Security Settings Tests

    [Fact]
    public async Task GetSecuritySettings_ReturnsSettings()
    {
        // Arrange
        var settings = new SecuritySettingsDto
        {
            PasswordMinLength = 8,
            RequireUppercase = true,
            RequireNumbers = true
        };

        _mockSettingsService.Setup(s => s.GetSecuritySettingsAsync())
            .ReturnsAsync(settings);

        // Act
        var result = await _controller.GetSecuritySettings();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task UpdateSecuritySettings_ValidData_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateSecuritySettingsDto
        {
            PasswordMinLength = 12,
            SessionTimeoutMinutes = 30
        };

        var updatedSettings = new SecuritySettingsDto { PasswordMinLength = 12 };

        _mockSettingsService.Setup(s => s.UpdateSecuritySettingsAsync(updateDto))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _controller.UpdateSecuritySettings(updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Export/Import Tests

    [Fact]
    public async Task ExportSettings_ReturnsFile()
    {
        // Arrange
        var exportData = new byte[] { 1, 2, 3 };

        _mockSettingsService.Setup(s => s.ExportSettingsAsync())
            .ReturnsAsync(exportData);

        // Act
        var result = await _controller.ExportSettings();

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task ImportSettings_ValidFile_ReturnsOk()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1000);

        _mockSettingsService.Setup(s => s.ImportSettingsAsync(It.IsAny<byte[]>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ImportSettings(file.Object);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ImportSettings_InvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1000);

        _mockSettingsService.Setup(s => s.ImportSettingsAsync(It.IsAny<byte[]>()))
            .ThrowsAsync(new InvalidOperationException("Invalid file format"));

        // Act
        var result = await _controller.ImportSettings(file.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}

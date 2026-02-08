// CRM Solution - Customer Relationship Management System
// Module Field Configuration Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ModuleFieldConfigurationService
/// Covers: Field visibility, field order, required fields, custom fields
/// </summary>
public class ModuleFieldConfigurationServiceTests
{
    private readonly Mock<IRepository<ModuleFieldConfiguration>> _mockFieldConfigRepository;
    private readonly Mock<IRepository<UserGroup>> _mockUserGroupRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ModuleFieldConfigurationService>> _mockLogger;
    private readonly ModuleFieldConfigurationService _service;

    public ModuleFieldConfigurationServiceTests()
    {
        _mockFieldConfigRepository = new Mock<IRepository<ModuleFieldConfiguration>>();
        _mockUserGroupRepository = new Mock<IRepository<UserGroup>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ModuleFieldConfigurationService>>();

        _service = new ModuleFieldConfigurationService(
            _mockFieldConfigRepository.Object,
            _mockUserGroupRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Get Configuration Tests

    [Fact]
    public async Task GetConfigurationAsync_ExistingModule_ReturnsConfiguration()
    {
        // Arrange
        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration
            {
                Id = 1,
                ModuleName = "Account",
                FieldName = "Name",
                IsVisible = true,
                DisplayOrder = 1
            }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(configs);

        // Act
        var result = await _service.GetConfigurationAsync("Account");

        // Assert
        result.Should().NotBeEmpty();
        result.First().FieldName.Should().Be("Name");
    }

    [Fact]
    public async Task GetConfigurationAsync_NonExistingModule_ReturnsEmpty()
    {
        // Arrange
        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration>());

        // Act
        var result = await _service.GetConfigurationAsync("NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConfigurationByGroupAsync_ReturnsGroupSpecificConfig()
    {
        // Arrange
        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration
            {
                Id = 1,
                ModuleName = "Account",
                FieldName = "Revenue",
                UserGroupId = 1,
                IsVisible = true
            }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(configs);

        // Act
        var result = await _service.GetConfigurationByGroupAsync("Account", 1);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFieldConfigurationAsync_ExistingField_ReturnsConfig()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            Id = 1,
            ModuleName = "Contact",
            FieldName = "Email",
            IsVisible = true
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration> { config });

        // Act
        var result = await _service.GetFieldConfigurationAsync("Contact", "Email");

        // Assert
        result.Should().NotBeNull();
        result!.FieldName.Should().Be("Email");
    }

    #endregion

    #region Set Configuration Tests

    [Fact]
    public async Task SetFieldVisibilityAsync_ValidField_UpdatesVisibility()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            Id = 1,
            ModuleName = "Account",
            FieldName = "Industry",
            IsVisible = true
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration> { config });

        _mockFieldConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.IsVisible = false; return c; });

        // Act
        var result = await _service.SetFieldVisibilityAsync("Account", "Industry", false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetFieldVisibilityAsync_NonExistingField_CreatesNew()
    {
        // Arrange
        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration>());

        _mockFieldConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.SetFieldVisibilityAsync("Account", "NewField", true);

        // Assert
        result.Should().BeTrue();
        _mockFieldConfigRepository.Verify(r => r.AddAsync(It.IsAny<ModuleFieldConfiguration>()), Times.Once);
    }

    [Fact]
    public async Task SetFieldRequiredAsync_ValidField_UpdatesRequired()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            Id = 1,
            ModuleName = "Contact",
            FieldName = "Phone",
            IsRequired = false
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration> { config });

        _mockFieldConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.IsRequired = true; return c; });

        // Act
        var result = await _service.SetFieldRequiredAsync("Contact", "Phone", true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetFieldOrderAsync_ValidFields_UpdatesOrder()
    {
        // Arrange
        var request = new SetFieldOrderRequest
        {
            ModuleName = "Account",
            FieldOrders = new Dictionary<string, int>
            {
                { "Name", 1 },
                { "Email", 2 },
                { "Phone", 3 }
            }
        };

        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration { Id = 1, ModuleName = "Account", FieldName = "Name" },
            new ModuleFieldConfiguration { Id = 2, ModuleName = "Account", FieldName = "Email" },
            new ModuleFieldConfiguration { Id = 3, ModuleName = "Account", FieldName = "Phone" }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(configs);

        _mockFieldConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => c);

        // Act
        var result = await _service.SetFieldOrderAsync(request);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Label Configuration Tests

    [Fact]
    public async Task SetFieldLabelAsync_ValidField_UpdatesLabel()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            Id = 1,
            ModuleName = "Account",
            FieldName = "Name",
            DisplayLabel = "Name"
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration> { config });

        _mockFieldConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.DisplayLabel = "Account Name"; return c; });

        // Act
        var result = await _service.SetFieldLabelAsync("Account", "Name", "Account Name");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetFieldHelpTextAsync_ValidField_UpdatesHelpText()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            Id = 1,
            ModuleName = "Opportunity",
            FieldName = "Amount",
            HelpText = null
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration> { config });

        _mockFieldConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => c);

        // Act
        var result = await _service.SetFieldHelpTextAsync("Opportunity", "Amount", "Expected deal value");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Group-Specific Configuration Tests

    [Fact]
    public async Task SetGroupFieldVisibilityAsync_ValidGroup_SetsVisibility()
    {
        // Arrange
        var group = new UserGroup { Id = 1, Name = "Sales" };

        _mockUserGroupRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(group);

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration>());

        _mockFieldConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.SetGroupFieldVisibilityAsync("Account", "Revenue", 1, true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetGroupFieldVisibilityAsync_InvalidGroup_ReturnsFalse()
    {
        // Arrange
        _mockUserGroupRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((UserGroup?)null);

        // Act
        var result = await _service.SetGroupFieldVisibilityAsync("Account", "Revenue", 999, true);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Bulk Configuration Tests

    [Fact]
    public async Task BulkUpdateConfigurationAsync_ValidRequest_UpdatesAll()
    {
        // Arrange
        var request = new BulkFieldConfigRequest
        {
            ModuleName = "Contact",
            Configurations = new List<FieldConfigItem>
            {
                new FieldConfigItem { FieldName = "FirstName", IsVisible = true, IsRequired = true },
                new FieldConfigItem { FieldName = "LastName", IsVisible = true, IsRequired = true },
                new FieldConfigItem { FieldName = "Nickname", IsVisible = false, IsRequired = false }
            }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(new List<ModuleFieldConfiguration>());

        _mockFieldConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.BulkUpdateConfigurationAsync(request);

        // Assert
        result.SuccessCount.Should().Be(3);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ValidModule_ResetsConfiguration()
    {
        // Arrange
        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration { Id = 1, ModuleName = "Lead", FieldName = "Name" },
            new ModuleFieldConfiguration { Id = 2, ModuleName = "Lead", FieldName = "Email" }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(configs);

        _mockFieldConfigRepository.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResetToDefaultsAsync("Lead");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateFieldConfigurationAsync_ValidConfig_ReturnsTrue()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            ModuleName = "Account",
            FieldName = "Name",
            IsVisible = true
        };

        // Act
        var result = await _service.ValidateFieldConfigurationAsync(config);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFieldConfigurationAsync_InvisibleRequired_ReturnsInvalid()
    {
        // Arrange
        var config = new ModuleFieldConfiguration
        {
            ModuleName = "Account",
            FieldName = "Name",
            IsVisible = false,
            IsRequired = true
        };

        // Act
        var result = await _service.ValidateFieldConfigurationAsync(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    #endregion

    #region Get Modules Tests

    [Fact]
    public async Task GetAvailableModulesAsync_ReturnsAllModules()
    {
        // Act
        var result = await _service.GetAvailableModulesAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("Account");
        result.Should().Contain("Contact");
    }

    [Fact]
    public async Task GetModuleFieldsAsync_ValidModule_ReturnsFields()
    {
        // Act
        var result = await _service.GetModuleFieldsAsync("Account");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetModuleFieldsAsync_InvalidModule_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetModuleFieldsAsync("InvalidModule");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Export/Import Tests

    [Fact]
    public async Task ExportConfigurationAsync_ValidModule_ReturnsJson()
    {
        // Arrange
        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration { Id = 1, ModuleName = "Account", FieldName = "Name" }
        };

        _mockFieldConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleFieldConfiguration, bool>>>()))
            .ReturnsAsync(configs);

        // Act
        var result = await _service.ExportConfigurationAsync("Account");

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ImportConfigurationAsync_ValidJson_ImportsConfiguration()
    {
        // Arrange
        var json = @"[{""ModuleName"":""Account"",""FieldName"":""Name"",""IsVisible"":true}]";

        _mockFieldConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleFieldConfiguration>()))
            .ReturnsAsync((ModuleFieldConfiguration c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.ImportConfigurationAsync(json);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ImportConfigurationAsync_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var result = await _service.ImportConfigurationAsync(json);

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetConfigurationStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var configs = new List<ModuleFieldConfiguration>
        {
            new ModuleFieldConfiguration { ModuleName = "Account", FieldName = "Name", IsVisible = true },
            new ModuleFieldConfiguration { ModuleName = "Account", FieldName = "Hidden", IsVisible = false },
            new ModuleFieldConfiguration { ModuleName = "Contact", FieldName = "Email", IsVisible = true }
        };

        _mockFieldConfigRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(configs);

        // Act
        var result = await _service.GetConfigurationStatisticsAsync();

        // Assert
        result.TotalConfigurations.Should().Be(3);
        result.ModuleCount.Should().Be(2);
    }

    #endregion
}

// Supporting classes for tests
public class SetFieldOrderRequest
{
    public string ModuleName { get; set; } = string.Empty;
    public Dictionary<string, int> FieldOrders { get; set; } = new();
}

public class BulkFieldConfigRequest
{
    public string ModuleName { get; set; } = string.Empty;
    public List<FieldConfigItem> Configurations { get; set; } = new();
}

public class FieldConfigItem
{
    public string FieldName { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool IsRequired { get; set; }
}

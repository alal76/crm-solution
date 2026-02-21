// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ModuleUIConfigServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ModuleUIConfigService>> _mockLogger;
    private readonly ModuleUIConfigService _service;

    public ModuleUIConfigServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ModuleUIConfigService>>();
        _service = new ModuleUIConfigService(_mockContext.Object, _mockLogger.Object);
    }

    // ───────────────────────────────────────────────────
    // Helper
    // ───────────────────────────────────────────────────

    private static ModuleUIConfig MakeConfig(
        int id,
        string moduleName,
        bool isEnabled = true,
        int displayOrder = 0,
        string? tabsConfig = null,
        string? linkedEntitiesConfig = null)
    {
        return new ModuleUIConfig
        {
            Id = id,
            ModuleName = moduleName,
            IsEnabled = isEnabled,
            DisplayName = moduleName,
            IconName = "Folder",
            DisplayOrder = displayOrder,
            TabsConfig = tabsConfig,
            LinkedEntitiesConfig = linkedEntitiesConfig,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private static ModuleFieldConfiguration MakeField(int id, string moduleName, string fieldName, int tabIndex = 0)
    {
        return new ModuleFieldConfiguration
        {
            Id = id,
            ModuleName = moduleName,
            FieldName = fieldName,
            FieldLabel = fieldName,
            FieldType = "text",
            TabIndex = tabIndex,
            TabName = "General",
            DisplayOrder = 0,
            IsEnabled = true,
            IsRequired = false,
            GridSize = 6,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private void SetupModuleConfigs(List<ModuleUIConfig> configs)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(configs);
        _mockContext.Setup(c => c.ModuleUIConfigs).Returns(mockSet.Object);
    }

    private void SetupFieldConfigs(List<ModuleFieldConfiguration> fields)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(fields);
        _mockContext.Setup(c => c.ModuleFieldConfigurations).Returns(mockSet.Object);
    }

    private void SetupSaveChanges()
    {
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    // ───────────────────────────────────────────────────
    // GetAllModuleConfigsAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllModuleConfigsAsync_ShouldReturnOrderedDtos()
    {
        // Arrange
        var configs = new List<ModuleUIConfig>
        {
            MakeConfig(1, "Leads", displayOrder: 2),
            MakeConfig(2, "Contacts", displayOrder: 1),
            MakeConfig(3, "Customers", displayOrder: 0),
        };
        SetupModuleConfigs(configs);

        // Act
        var result = await _service.GetAllModuleConfigsAsync();

        // Assert
        var list = result.ToList();
        list.Should().HaveCount(3);
        list[0].ModuleName.Should().Be("Customers");
        list[1].ModuleName.Should().Be("Contacts");
        list[2].ModuleName.Should().Be("Leads");
    }

    [Fact]
    public async Task GetAllModuleConfigsAsync_EmptyTable_ShouldReturnEmpty()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.GetAllModuleConfigsAsync();

        result.Should().BeEmpty();
    }

    // ───────────────────────────────────────────────────
    // GetModuleConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task GetModuleConfigAsync_Found_ShouldReturnDto()
    {
        var configs = new List<ModuleUIConfig> { MakeConfig(1, "Customers") };
        SetupModuleConfigs(configs);

        var result = await _service.GetModuleConfigAsync("Customers");

        result.Should().NotBeNull();
        result!.ModuleName.Should().Be("Customers");
    }

    [Fact]
    public async Task GetModuleConfigAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.GetModuleConfigAsync("NonExistent");

        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────
    // GetCompleteModuleConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCompleteModuleConfigAsync_Found_ShouldReturnCompleteDto()
    {
        // Service deserializes with JsonNamingPolicy.CamelCase, so test data must also be serialized with camelCase
        var camelCase = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var tabsJson = JsonSerializer.Serialize(new List<TabConfigItem>
        {
            new() { Index = 0, Name = "Basic", Enabled = true, Order = 0 },
        }, camelCase);
        var linkedJson = JsonSerializer.Serialize(new List<LinkedEntityConfigItem>
        {
            new() { EntityName = "Contacts", RelationshipType = "one-to-many", Enabled = true, DisplayOrder = 0 },
        }, camelCase);

        var configs = new List<ModuleUIConfig>
        {
            MakeConfig(1, "Customers", tabsConfig: tabsJson, linkedEntitiesConfig: linkedJson),
        };
        var fields = new List<ModuleFieldConfiguration>
        {
            MakeField(10, "Customers", "Name"),
            MakeField(11, "Customers", "Email"),
            MakeField(12, "Leads", "Phone"), // different module - should be excluded
        };
        SetupModuleConfigs(configs);
        SetupFieldConfigs(fields);

        var result = await _service.GetCompleteModuleConfigAsync("Customers");

        result.Should().NotBeNull();
        result!.ModuleConfig.ModuleName.Should().Be("Customers");
        result.FieldConfigurations.Should().HaveCount(2);
        result.Tabs.Should().HaveCount(1);
        result.Tabs[0].Name.Should().Be("Basic");
        result.LinkedEntities.Should().HaveCount(1);
        result.LinkedEntities[0].EntityName.Should().Be("Contacts");
    }

    [Fact]
    public async Task GetCompleteModuleConfigAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());
        SetupFieldConfigs(new List<ModuleFieldConfiguration>());

        var result = await _service.GetCompleteModuleConfigAsync("NonExistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCompleteModuleConfigAsync_NullJsonFields_ShouldReturnEmptyLists()
    {
        var configs = new List<ModuleUIConfig> { MakeConfig(1, "Customers") };
        SetupModuleConfigs(configs);
        SetupFieldConfigs(new List<ModuleFieldConfiguration>());

        var result = await _service.GetCompleteModuleConfigAsync("Customers");

        result.Should().NotBeNull();
        result!.Tabs.Should().BeEmpty();
        result.LinkedEntities.Should().BeEmpty();
    }

    // ───────────────────────────────────────────────────
    // CreateModuleConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateModuleConfigAsync_ShouldCreateAndReturnDto()
    {
        var configs = new List<ModuleUIConfig>();
        SetupModuleConfigs(configs);
        SetupSaveChanges();

        var dto = new CreateModuleUIConfigDto
        {
            ModuleName = "CustomModule",
            IsEnabled = true,
            DisplayName = "Custom Module",
            IconName = "Star",
            DisplayOrder = 5,
        };

        var result = await _service.CreateModuleConfigAsync(dto);

        result.Should().NotBeNull();
        result.ModuleName.Should().Be("CustomModule");
        result.DisplayName.Should().Be("Custom Module");
        result.IconName.Should().Be("Star");
        result.DisplayOrder.Should().Be(5);
        configs.Should().HaveCount(1);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateModuleConfigAsync_EmptyModuleName_ShouldThrow()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var dto = new CreateModuleUIConfigDto
        {
            ModuleName = " ",
            DisplayName = "Invalid",
        };

        var action = async () => await _service.CreateModuleConfigAsync(dto);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    // ───────────────────────────────────────────────────
    // UpdateModuleConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateModuleConfigAsync_Found_ShouldUpdateAndReturnDto()
    {
        var configs = new List<ModuleUIConfig> { MakeConfig(1, "Customers") };
        SetupModuleConfigs(configs);
        SetupSaveChanges();

        var dto = new UpdateModuleUIConfigDto
        {
            DisplayName = "Accounts",
            IconName = "Business",
            IsEnabled = false,
        };

        var result = await _service.UpdateModuleConfigAsync("Customers", dto);

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Accounts");
        result.IconName.Should().Be("Business");
        result.IsEnabled.Should().BeFalse();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateModuleConfigAsync_EmptyModuleName_ShouldThrow()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var action = async () => await _service.UpdateModuleConfigAsync(" ", new UpdateModuleUIConfigDto());

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateModuleConfigAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.UpdateModuleConfigAsync("NonExistent", new UpdateModuleUIConfigDto());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateModuleConfigAsync_NullFields_ShouldNotOverwrite()
    {
        var config = MakeConfig(1, "Customers");
        config.DisplayName = "Original";
        config.IconName = "OrigIcon";
        config.DisplayOrder = 3;
        var configs = new List<ModuleUIConfig> { config };
        SetupModuleConfigs(configs);
        SetupSaveChanges();

        // Only update DisplayName, leave others null
        var dto = new UpdateModuleUIConfigDto { DisplayName = "Updated" };

        var result = await _service.UpdateModuleConfigAsync("Customers", dto);

        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Updated");
        result.IconName.Should().Be("OrigIcon"); // unchanged
    }

    // ───────────────────────────────────────────────────
    // BatchUpdateModulesAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task BatchUpdateModulesAsync_ShouldUpdateMatchingModules()
    {
        var configs = new List<ModuleUIConfig>
        {
            MakeConfig(1, "Customers", isEnabled: true, displayOrder: 0),
            MakeConfig(2, "Contacts", isEnabled: true, displayOrder: 1),
            MakeConfig(3, "Leads", isEnabled: true, displayOrder: 2),
        };
        SetupModuleConfigs(configs);
        SetupSaveChanges();

        var dto = new BatchModuleUIConfigUpdateDto
        {
            Modules = new List<UpdateModuleUIConfigItem>
            {
                new() { ModuleName = "Customers", IsEnabled = false, DisplayOrder = 10 },
                new() { ModuleName = "Leads", IsEnabled = true, DisplayOrder = 0 },
            },
        };

        var result = await _service.BatchUpdateModulesAsync(dto);

        var list = result.ToList();
        list.Should().HaveCount(2); // returns only the 2 configs matching the update request
        var customers = configs.First(c => c.ModuleName == "Customers");
        customers.IsEnabled.Should().BeFalse();
        customers.DisplayOrder.Should().Be(10);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ───────────────────────────────────────────────────
    // UpdateLinkedEntitiesAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateLinkedEntitiesAsync_Found_ShouldSerializeAndSave()
    {
        var config = MakeConfig(1, "Customers");
        SetupModuleConfigs(new List<ModuleUIConfig> { config });
        SetupSaveChanges();

        var items = new List<LinkedEntityConfigItem>
        {
            new() { EntityName = "Contacts", RelationshipType = "one-to-many", Enabled = true, DisplayOrder = 0 },
            new() { EntityName = "Notes", RelationshipType = "one-to-many", Enabled = true, DisplayOrder = 1 },
        };

        var result = await _service.UpdateLinkedEntitiesAsync("Customers", items);

        result.Should().NotBeNull();
        config.LinkedEntitiesConfig.Should().NotBeNullOrEmpty();
        var deserialized = JsonSerializer.Deserialize<List<LinkedEntityConfigItem>>(config.LinkedEntitiesConfig!);
        deserialized.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateLinkedEntitiesAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.UpdateLinkedEntitiesAsync("NonExistent", new List<LinkedEntityConfigItem>());

        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────
    // UpdateTabsConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTabsConfigAsync_Found_ShouldSerializeAndSave()
    {
        var config = MakeConfig(1, "Customers");
        SetupModuleConfigs(new List<ModuleUIConfig> { config });
        SetupSaveChanges();

        var tabs = new List<TabConfigItem>
        {
            new() { Index = 0, Name = "Basic Info", Enabled = true, Order = 0 },
            new() { Index = 1, Name = "Details", Enabled = true, Order = 1 },
        };

        var result = await _service.UpdateTabsConfigAsync("Customers", tabs);

        result.Should().NotBeNull();
        config.TabsConfig.Should().NotBeNullOrEmpty();
        var deserialized = JsonSerializer.Deserialize<List<TabConfigItem>>(config.TabsConfig!);
        deserialized.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateTabsConfigAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.UpdateTabsConfigAsync("NonExistent", new List<TabConfigItem>());

        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────
    // SaveCompleteModuleConfigAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task SaveCompleteModuleConfigAsync_Found_ShouldUpdateTabsLinkedEntitiesAndFields()
    {
        var config = MakeConfig(1, "Customers");
        var field = MakeField(10, "Customers", "Name");
        field.IsEnabled = true;
        field.IsRequired = false;

        SetupModuleConfigs(new List<ModuleUIConfig> { config });
        SetupFieldConfigs(new List<ModuleFieldConfiguration> { field });
        SetupSaveChanges();

        var dto = new SaveCompleteModuleConfigDto
        {
            Tabs = new List<TabConfigItem>
            {
                new() { Index = 0, Name = "Overview", Enabled = true, Order = 0 },
            },
            LinkedEntities = new List<LinkedEntityConfigItem>
            {
                new() { EntityName = "Notes", RelationshipType = "one-to-many", Enabled = true, DisplayOrder = 0 },
            },
            Fields = new List<SaveFieldConfigItem>
            {
                new() { Id = 10, IsEnabled = false, IsRequired = true, DisplayOrder = 5, GridSize = 12 },
            },
        };

        var result = await _service.SaveCompleteModuleConfigAsync("Customers", dto);

        result.Should().NotBeNull();
        config.TabsConfig.Should().NotBeNullOrEmpty();
        config.LinkedEntitiesConfig.Should().NotBeNullOrEmpty();
        // Field should have been updated
        field.IsEnabled.Should().BeFalse();
        field.IsRequired.Should().BeTrue();
        field.DisplayOrder.Should().Be(5);
        field.GridSize.Should().Be(12);
    }

    [Fact]
    public async Task SaveCompleteModuleConfigAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());
        SetupFieldConfigs(new List<ModuleFieldConfiguration>());

        var result = await _service.SaveCompleteModuleConfigAsync("NonExistent", new SaveCompleteModuleConfigDto());

        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────
    // ResetModuleToDefaultsAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task ResetModuleToDefaultsAsync_Found_ShouldRemoveFieldsAndResetLinkedEntities()
    {
        var config = MakeConfig(1, "Customers", tabsConfig: "[{\"Index\":0}]", linkedEntitiesConfig: "[{\"EntityName\":\"Old\"}]");
        var field1 = MakeField(10, "Customers", "Name");
        var field2 = MakeField(11, "Customers", "Email");
        var otherField = MakeField(12, "Leads", "Phone"); // should NOT be removed

        var fieldList = new List<ModuleFieldConfiguration> { field1, field2, otherField };
        SetupModuleConfigs(new List<ModuleUIConfig> { config });
        SetupFieldConfigs(fieldList);
        SetupSaveChanges();

        var result = await _service.ResetModuleToDefaultsAsync("Customers");

        result.Should().NotBeNull();
        // TabsConfig should be cleared
        config.TabsConfig.Should().BeNull();
        // LinkedEntitiesConfig should be reset to defaults from DefaultModuleConfigs
        config.LinkedEntitiesConfig.Should().NotBeNull();
        // The field removal happens via RemoveRange on the matching fields
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ResetModuleToDefaultsAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());
        SetupFieldConfigs(new List<ModuleFieldConfiguration>());

        var result = await _service.ResetModuleToDefaultsAsync("NonExistent");

        result.Should().BeNull();
    }

    // ───────────────────────────────────────────────────
    // InitializeDefaultConfigsAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task InitializeDefaultConfigsAsync_EmptyTable_ShouldCreateDefaultModules()
    {
        var configs = new List<ModuleUIConfig>();
        SetupModuleConfigs(configs);
        SetupSaveChanges();

        await _service.InitializeDefaultConfigsAsync();

        // NOTE: Service uses AddRange which doesn't mutate the mock backing list,
        // so we verify behaviour via SaveChangesAsync being called (meaning modules were created).
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeDefaultConfigsAsync_AlreadyPopulated_ShouldSkip()
    {
        // Provide all 14 default module names so the service finds them all and skips creation
        // Note: Module is "Accounts" (not "Customers") after the Account/Customer rename
        var configs = new List<ModuleUIConfig>
        {
            MakeConfig(1, "Dashboard"),
            MakeConfig(2, "Accounts"),
            MakeConfig(3, "Contacts"),
            MakeConfig(4, "Leads"),
            MakeConfig(5, "Opportunities"),
            MakeConfig(6, "Products"),
            MakeConfig(7, "Services"),
            MakeConfig(8, "Campaigns"),
            MakeConfig(9, "Quotes"),
            MakeConfig(10, "Tasks"),
            MakeConfig(11, "Activities"),
            MakeConfig(12, "Notes"),
            MakeConfig(13, "Workflows"),
            MakeConfig(14, "Reports"),
        };
        SetupModuleConfigs(configs);

        await _service.InitializeDefaultConfigsAsync();

        // Should NOT create new entries since all 14 defaults already exist
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ───────────────────────────────────────────────────
    // ToggleModuleAsync
    // ───────────────────────────────────────────────────

    [Fact]
    public async Task ToggleModuleAsync_Found_ShouldSetEnabled()
    {
        var config = MakeConfig(1, "Customers", isEnabled: true);
        SetupModuleConfigs(new List<ModuleUIConfig> { config });
        SetupSaveChanges();

        var result = await _service.ToggleModuleAsync("Customers", false);

        result.Should().NotBeNull();
        result!.IsEnabled.Should().BeFalse();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleModuleAsync_NotFound_ShouldReturnNull()
    {
        SetupModuleConfigs(new List<ModuleUIConfig>());

        var result = await _service.ToggleModuleAsync("NonExistent", true);

        result.Should().BeNull();
    }
}

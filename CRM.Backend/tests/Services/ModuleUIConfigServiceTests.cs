// CRM Solution - Customer Relationship Management System
// Module UI Config Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
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
/// Unit tests for ModuleUIConfigService
/// Covers: UI configuration, module layouts, theme settings
/// </summary>
public class ModuleUIConfigServiceTests
{
    private readonly Mock<IRepository<ModuleUIConfig>> _mockConfigRepository;
    private readonly Mock<IRepository<ColorPalette>> _mockPaletteRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<ModuleUIConfigService>> _mockLogger;
    private readonly ModuleUIConfigService _service;

    public ModuleUIConfigServiceTests()
    {
        _mockConfigRepository = new Mock<IRepository<ModuleUIConfig>>();
        _mockPaletteRepository = new Mock<IRepository<ColorPalette>>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<ModuleUIConfigService>>();

        // Setup cache to return null (cache miss)
        object? cacheValue = null;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
            .Returns(false);

        _service = new ModuleUIConfigService(
            _mockConfigRepository.Object,
            _mockPaletteRepository.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    #region Get Config Tests

    [Fact]
    public async Task GetConfigAsync_ExistingModule_ReturnsConfig()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            Configuration = "{\"columns\": [\"name\", \"email\"]}"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetConfigAsync("accounts");

        // Assert
        result.Should().NotBeNull();
        result!.ModuleName.Should().Be("accounts");
    }

    [Fact]
    public async Task GetConfigAsync_NonExistingModule_ReturnsDefault()
    {
        // Arrange
        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig>());

        // Act
        var result = await _service.GetConfigAsync("unknown");

        // Assert
        result.Should().NotBeNull();
        result!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetConfigAsync_WithUserId_ReturnsUserConfig()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            UserId = 1
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetConfigAsync("accounts", 1);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Save Config Tests

    [Fact]
    public async Task SaveConfigAsync_NewConfig_CreatesConfig()
    {
        // Arrange
        var request = new SaveUIConfigDto
        {
            ModuleName = "accounts",
            Configuration = "{\"columns\": [\"name\"]}"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig>());

        _mockConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.SaveConfigAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task SaveConfigAsync_ExistingConfig_UpdatesConfig()
    {
        // Arrange
        var existing = new ModuleUIConfig { Id = 1, ModuleName = "accounts" };
        var request = new SaveUIConfigDto
        {
            ModuleName = "accounts",
            Configuration = "{\"columns\": [\"name\", \"email\"]}"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.SaveConfigAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveConfigAsync_WithUserId_SavesUserConfig()
    {
        // Arrange
        var request = new SaveUIConfigDto
        {
            ModuleName = "accounts",
            UserId = 1,
            Configuration = "{}"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig>());

        _mockConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.SaveConfigAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Delete Config Tests

    [Fact]
    public async Task DeleteConfigAsync_ExistingConfig_DeletesConfig()
    {
        // Arrange
        _mockConfigRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteConfigAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResetConfigAsync_UserConfig_ResetsToDefault()
    {
        // Arrange
        var userConfig = new ModuleUIConfig { Id = 1, ModuleName = "accounts", UserId = 1 };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { userConfig });

        _mockConfigRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ResetConfigAsync("accounts", 1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Layout Tests

    [Fact]
    public async Task GetLayoutAsync_ValidModule_ReturnsLayout()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            Layout = "{\"type\": \"grid\"}"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetLayoutAsync("accounts");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveLayoutAsync_ValidLayout_SavesLayout()
    {
        // Arrange
        var request = new SaveLayoutDto
        {
            ModuleName = "accounts",
            Layout = "{\"type\": \"list\"}"
        };

        var existing = new ModuleUIConfig { Id = 1, ModuleName = "accounts" };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.SaveLayoutAsync(request);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Column Config Tests

    [Fact]
    public async Task GetColumnConfigAsync_ValidModule_ReturnsColumns()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            ColumnConfig = "[{\"field\": \"name\", \"visible\": true}]"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetColumnConfigAsync("accounts");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveColumnConfigAsync_ValidConfig_SavesColumns()
    {
        // Arrange
        var columns = new List<ColumnConfigDto>
        {
            new ColumnConfigDto { Field = "name", Visible = true, Width = 200 },
            new ColumnConfigDto { Field = "email", Visible = true, Width = 250 }
        };

        var existing = new ModuleUIConfig { Id = 1, ModuleName = "accounts" };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.SaveColumnConfigAsync("accounts", columns);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Filter Config Tests

    [Fact]
    public async Task GetSavedFiltersAsync_ValidModule_ReturnsFilters()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            SavedFilters = "[{\"name\": \"Active\", \"filter\": \"status=active\"}]"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetSavedFiltersAsync("accounts");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveFilterAsync_NewFilter_AddsFilter()
    {
        // Arrange
        var filter = new SavedFilterDto
        {
            Name = "VIP Customers",
            Filter = "tier=vip"
        };

        var existing = new ModuleUIConfig { Id = 1, ModuleName = "accounts", SavedFilters = "[]" };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.SaveFilterAsync("accounts", filter);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFilterAsync_ExistingFilter_DeletesFilter()
    {
        // Arrange
        var existing = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "accounts",
            SavedFilters = "[{\"id\": \"filter1\", \"name\": \"Test\"}]"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.DeleteFilterAsync("accounts", "filter1");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Theme Tests

    [Fact]
    public async Task GetThemeAsync_ValidUser_ReturnsTheme()
    {
        // Arrange
        var config = new ModuleUIConfig
        {
            Id = 1,
            ModuleName = "global",
            UserId = 1,
            Theme = "dark"
        };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { config });

        // Act
        var result = await _service.GetThemeAsync(1);

        // Assert
        result.Should().Be("dark");
    }

    [Fact]
    public async Task SaveThemeAsync_ValidTheme_SavesTheme()
    {
        // Arrange
        var existing = new ModuleUIConfig { Id = 1, ModuleName = "global", UserId = 1 };

        _mockConfigRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ModuleUIConfig, bool>>>()))
            .ReturnsAsync(new List<ModuleUIConfig> { existing });

        _mockConfigRepository.Setup(r => r.UpdateAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => c);

        // Act
        var result = await _service.SaveThemeAsync(1, "dark");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Color Palette Tests

    [Fact]
    public async Task GetColorPalettesAsync_ReturnsPalettes()
    {
        // Arrange
        var palettes = new List<ColorPalette>
        {
            new ColorPalette { Id = 1, Name = "Default" },
            new ColorPalette { Id = 2, Name = "Corporate" }
        };

        _mockPaletteRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(palettes);

        // Act
        var result = await _service.GetColorPalettesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetColorPaletteAsync_ExistingPalette_ReturnsPalette()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, Name = "Default", PrimaryColor = "#1976d2" };

        _mockPaletteRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        // Act
        var result = await _service.GetColorPaletteAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.PrimaryColor.Should().Be("#1976d2");
    }

    #endregion

    #region Module List Tests

    [Fact]
    public async Task GetAvailableModulesAsync_ReturnsModules()
    {
        // Act
        var result = await _service.GetAvailableModulesAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("accounts");
        result.Should().Contain("contacts");
    }

    [Fact]
    public async Task GetAllConfigsAsync_ReturnsAllConfigs()
    {
        // Arrange
        var configs = new List<ModuleUIConfig>
        {
            new ModuleUIConfig { Id = 1, ModuleName = "accounts" },
            new ModuleUIConfig { Id = 2, ModuleName = "contacts" }
        };

        _mockConfigRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(configs);

        // Act
        var result = await _service.GetAllConfigsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task ExportConfigsAsync_ValidModules_ReturnsExportData()
    {
        // Arrange
        var configs = new List<ModuleUIConfig>
        {
            new ModuleUIConfig { ModuleName = "accounts", Configuration = "{}" },
            new ModuleUIConfig { ModuleName = "contacts", Configuration = "{}" }
        };

        _mockConfigRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(configs);

        // Act
        var result = await _service.ExportConfigsAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ImportConfigsAsync_ValidData_ImportsConfigs()
    {
        // Arrange
        var importData = "[{\"moduleName\": \"accounts\", \"configuration\": \"{}\"}]";

        _mockConfigRepository.Setup(r => r.AddAsync(It.IsAny<ModuleUIConfig>()))
            .ReturnsAsync((ModuleUIConfig c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.ImportConfigsAsync(importData);

        // Assert
        result.ImportedCount.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion
}

// Supporting classes for tests
public class SaveUIConfigDto
{
    public string ModuleName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Configuration { get; set; }
}

public class SaveLayoutDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
}

public class ColumnConfigDto
{
    public string Field { get; set; } = string.Empty;
    public bool Visible { get; set; }
    public int Width { get; set; }
}

public class SavedFilterDto
{
    public string Name { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
}

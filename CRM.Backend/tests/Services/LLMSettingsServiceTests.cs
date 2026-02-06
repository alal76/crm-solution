// CRM Solution - Customer Relationship Management System
// LLM Settings Service Unit Tests

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
/// Unit tests for LLMSettingsService
/// Covers: LLM provider configuration, model settings, API key management
/// </summary>
public class LLMSettingsServiceTests
{
    private readonly Mock<IRepository<LLMProviderSetting>> _mockSettingsRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<LLMSettingsService>> _mockLogger;
    private readonly LLMSettingsService _service;

    public LLMSettingsServiceTests()
    {
        _mockSettingsRepository = new Mock<IRepository<LLMProviderSetting>>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<LLMSettingsService>>();

        // Setup cache to return null (cache miss)
        object? cacheValue = null;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
            .Returns(false);

        _service = new LLMSettingsService(
            _mockSettingsRepository.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    #region Get Settings Tests

    [Fact]
    public async Task GetSettingsAsync_ExistingProvider_ReturnsSettings()
    {
        // Arrange
        var settings = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            Model = "gpt-4",
            IsActive = true
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { settings });

        // Act
        var result = await _service.GetSettingsAsync("OpenAI");

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().Be("gpt-4");
    }

    [Fact]
    public async Task GetSettingsAsync_NonExistingProvider_ReturnsNull()
    {
        // Arrange
        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting>());

        // Act
        var result = await _service.GetSettingsAsync("Unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveProviderAsync_ActiveProviderExists_ReturnsProvider()
    {
        // Arrange
        var settings = new List<LLMProviderSetting>
        {
            new LLMProviderSetting { Id = 1, Provider = "OpenAI", IsActive = true, Priority = 1 },
            new LLMProviderSetting { Id = 2, Provider = "Anthropic", IsActive = false, Priority = 2 }
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(settings.Where(s => s.IsActive).ToList());

        // Act
        var result = await _service.GetActiveProviderAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().Be("OpenAI");
    }

    [Fact]
    public async Task GetAllProvidersAsync_ReturnsAllProviders()
    {
        // Arrange
        var providers = new List<LLMProviderSetting>
        {
            new LLMProviderSetting { Id = 1, Provider = "OpenAI" },
            new LLMProviderSetting { Id = 2, Provider = "Anthropic" },
            new LLMProviderSetting { Id = 3, Provider = "Ollama" }
        };

        _mockSettingsRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(providers);

        // Act
        var result = await _service.GetAllProvidersAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    #endregion

    #region Save Settings Tests

    [Fact]
    public async Task SaveSettingsAsync_NewProvider_CreatesSettings()
    {
        // Arrange
        var request = new SaveLLMSettingsDto
        {
            Provider = "Anthropic",
            Model = "claude-3",
            ApiKey = "sk-test"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting>());

        _mockSettingsRepository.Setup(r => r.AddAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.SaveSettingsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task SaveSettingsAsync_ExistingProvider_UpdatesSettings()
    {
        // Arrange
        var existing = new LLMProviderSetting { Id = 1, Provider = "OpenAI", Model = "gpt-3.5" };
        var request = new SaveLLMSettingsDto
        {
            Provider = "OpenAI",
            Model = "gpt-4"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { existing });

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => s);

        // Act
        var result = await _service.SaveSettingsAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveSettingsAsync_WithEncryption_EncryptsApiKey()
    {
        // Arrange
        var request = new SaveLLMSettingsDto
        {
            Provider = "OpenAI",
            ApiKey = "sk-plaintext-key"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting>());

        _mockSettingsRepository.Setup(r => r.AddAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.SaveSettingsAsync(request);

        // Assert
        result.Should().NotBeNull();
        _mockSettingsRepository.Verify(r => r.AddAsync(It.Is<LLMProviderSetting>(
            s => s.ApiKeyEncrypted != "sk-plaintext-key" || s.ApiKeyEncrypted == null)), Times.Once);
    }

    #endregion

    #region Delete Settings Tests

    [Fact]
    public async Task DeleteSettingsAsync_ExistingProvider_DeletesSettings()
    {
        // Arrange
        var existing = new LLMProviderSetting { Id = 1, Provider = "Anthropic" };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { existing });

        _mockSettingsRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteSettingsAsync("Anthropic");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSettingsAsync_NonExistingProvider_ReturnsFalse()
    {
        // Arrange
        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting>());

        // Act
        var result = await _service.DeleteSettingsAsync("Unknown");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Activation Tests

    [Fact]
    public async Task ActivateProviderAsync_ValidProvider_ActivatesProvider()
    {
        // Arrange
        var providers = new List<LLMProviderSetting>
        {
            new LLMProviderSetting { Id = 1, Provider = "OpenAI", IsActive = true },
            new LLMProviderSetting { Id = 2, Provider = "Anthropic", IsActive = false }
        };

        _mockSettingsRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(providers);

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync((Expression<Func<LLMProviderSetting, bool>> predicate) =>
                providers.Where(predicate.Compile()).ToList());

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => s);

        // Act
        var result = await _service.ActivateProviderAsync("Anthropic");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateProviderAsync_ActiveProvider_DeactivatesProvider()
    {
        // Arrange
        var provider = new LLMProviderSetting { Id = 1, Provider = "OpenAI", IsActive = true };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => s);

        // Act
        var result = await _service.DeactivateProviderAsync("OpenAI");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Model Tests

    [Fact]
    public async Task GetAvailableModelsAsync_ValidProvider_ReturnsModels()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            AvailableModels = "gpt-4,gpt-4-turbo,gpt-3.5-turbo"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        // Act
        var result = await _service.GetAvailableModelsAsync("OpenAI");

        // Assert
        result.Should().Contain("gpt-4");
        result.Should().Contain("gpt-3.5-turbo");
    }

    [Fact]
    public async Task SetModelAsync_ValidModel_SetsModel()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            Model = "gpt-3.5-turbo"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => s);

        // Act
        var result = await _service.SetModelAsync("OpenAI", "gpt-4");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Connection Tests

    [Fact]
    public async Task TestConnectionAsync_ValidProvider_ReturnsSuccess()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            ApiKeyEncrypted = "encrypted-key",
            BaseUrl = "https://api.openai.com"
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        // Act
        var result = await _service.TestConnectionAsync("OpenAI");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task GetConfigurationAsync_ValidProvider_ReturnsConfig()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            Temperature = 0.7m,
            MaxTokens = 4096,
            TopP = 0.9m
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        // Act
        var result = await _service.GetConfigurationAsync("OpenAI");

        // Assert
        result.Should().NotBeNull();
        result!.Temperature.Should().Be(0.7m);
    }

    [Fact]
    public async Task SaveConfigurationAsync_ValidConfig_SavesConfig()
    {
        // Arrange
        var config = new LLMConfigurationDto
        {
            Temperature = 0.8m,
            MaxTokens = 2048,
            TopP = 0.95m
        };

        var provider = new LLMProviderSetting { Id = 1, Provider = "OpenAI" };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => s);

        // Act
        var result = await _service.SaveConfigurationAsync("OpenAI", config);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Usage Tests

    [Fact]
    public async Task GetUsageStatisticsAsync_ValidProvider_ReturnsStats()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            TotalTokensUsed = 100000,
            TotalRequestCount = 500
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        // Act
        var result = await _service.GetUsageStatisticsAsync("OpenAI");

        // Assert
        result.Should().NotBeNull();
        result!.TotalTokens.Should().Be(100000);
    }

    [Fact]
    public async Task ResetUsageStatisticsAsync_ValidProvider_ResetsStats()
    {
        // Arrange
        var provider = new LLMProviderSetting
        {
            Id = 1,
            Provider = "OpenAI",
            TotalTokensUsed = 100000
        };

        _mockSettingsRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<LLMProviderSetting, bool>>>()))
            .ReturnsAsync(new List<LLMProviderSetting> { provider });

        _mockSettingsRepository.Setup(r => r.UpdateAsync(It.IsAny<LLMProviderSetting>()))
            .ReturnsAsync((LLMProviderSetting s) => { s.TotalTokensUsed = 0; return s; });

        // Act
        var result = await _service.ResetUsageStatisticsAsync("OpenAI");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Provider List Tests

    [Fact]
    public void GetSupportedProviders_ReturnsSupportedList()
    {
        // Act
        var result = _service.GetSupportedProviders();

        // Assert
        result.Should().Contain("OpenAI");
        result.Should().Contain("Anthropic");
        result.Should().Contain("Ollama");
    }

    #endregion
}

// Supporting classes for tests
public class SaveLLMSettingsDto
{
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
}

public class LLMConfigurationDto
{
    public decimal Temperature { get; set; }
    public int MaxTokens { get; set; }
    public decimal TopP { get; set; }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Helpers;

/// <summary>
/// Unit tests for AIServiceHelper LLM service utility methods.
/// Tests provider resolution, model selection, and validation.
/// </summary>
public class AIServiceHelperTests
{
    #region GetDefaultModelForProvider Tests

    [Theory]
    [InlineData("openai", "gpt-4o-mini")]
    [InlineData("azure", "gpt-4o-mini")]
    [InlineData("anthropic", "claude-3-5-sonnet-20241022")]
    [InlineData("google", "gemini-pro")]
    [InlineData("deepseek", "deepseek-chat")]
    [InlineData("allenai", "allenai/OLMo-7B-Instruct")]
    [InlineData("local", "llama2")]
    public void GetDefaultModelForProvider_WithNoSettingsModel_ShouldReturnHardcodedDefault(
        string provider, string expectedModel)
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, provider);

        // Assert
        result.Should().Be(expectedModel);
    }

    [Fact]
    public void GetDefaultModelForProvider_WithConfiguredOpenAIModel_ShouldReturnConfiguredModel()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { DefaultModel = "gpt-4-turbo" }
        };

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, "openai");

        // Assert
        result.Should().Be("gpt-4-turbo");
    }

    [Fact]
    public void GetDefaultModelForProvider_WithConfiguredAzureModel_ShouldReturnConfiguredModel()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            Azure = new LLMProviderSettingsDto { DefaultModel = "gpt-4o" }
        };

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, "azure");

        // Assert
        result.Should().Be("gpt-4o");
    }

    [Fact]
    public void GetDefaultModelForProvider_WithConfiguredAnthropicModel_ShouldReturnConfiguredModel()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            Anthropic = new LLMProviderSettingsDto { DefaultModel = "claude-3-opus" }
        };

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, "anthropic");

        // Assert
        result.Should().Be("claude-3-opus");
    }

    [Fact]
    public void GetDefaultModelForProvider_WithUnknownProvider_ShouldReturnDefaultOpenAIModel()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, "unknown");

        // Assert
        result.Should().Be("gpt-4o-mini");
    }

    [Fact]
    public void GetDefaultModelForProvider_WithNullProvider_ShouldReturnDefaultOpenAIModel()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, null!);

        // Assert
        result.Should().Be("gpt-4o-mini");
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("OPENAI")]
    [InlineData("openai")]
    public void GetDefaultModelForProvider_ShouldBeCaseInsensitive(string provider)
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { DefaultModel = "gpt-4-turbo" }
        };

        // Act
        var result = AIServiceHelper.GetDefaultModelForProvider(settings, provider);

        // Assert
        result.Should().Be("gpt-4-turbo");
    }

    #endregion

    #region GetFirstAvailableProvider Tests

    [Fact]
    public void GetFirstAvailableProvider_WithEffectiveFallbackOrder_ShouldReturnFirst()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            EffectiveFallbackOrder = new List<string> { "azure", "openai", "local" }
        };

        // Act
        var result = AIServiceHelper.GetFirstAvailableProvider(settings);

        // Assert
        result.Should().Be("azure");
    }

    [Fact]
    public void GetFirstAvailableProvider_WithConfiguredProviderInFallbackOrder_ShouldReturnConfigured()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            FallbackOrder = new List<string> { "openai", "azure", "local" },
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true },
            Azure = new LLMProviderSettingsDto { IsConfigured = false }
        };

        // Act
        var result = AIServiceHelper.GetFirstAvailableProvider(settings);

        // Assert
        result.Should().Be("openai");
    }

    [Fact]
    public void GetFirstAvailableProvider_WithNoConfiguredProviders_ShouldReturnDefault()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            DefaultProvider = "local",
            FallbackOrder = new List<string> { "openai", "azure" },
            OpenAI = new LLMProviderSettingsDto { IsConfigured = false },
            Azure = new LLMProviderSettingsDto { IsConfigured = false }
        };

        // Act
        var result = AIServiceHelper.GetFirstAvailableProvider(settings);

        // Assert
        result.Should().Be("local");
    }

    [Fact]
    public void GetFirstAvailableProvider_WithNullSettings_ShouldReturnLocal()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetFirstAvailableProvider(settings);

        // Assert
        result.Should().Be("local");
    }

    [Fact]
    public void GetFirstAvailableProvider_WithEmptyEffectiveFallbackOrder_ShouldCheckFallbackOrder()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            EffectiveFallbackOrder = new List<string>(),
            FallbackOrder = new List<string> { "anthropic" },
            Anthropic = new LLMProviderSettingsDto { IsConfigured = true }
        };

        // Act
        var result = AIServiceHelper.GetFirstAvailableProvider(settings);

        // Assert
        result.Should().Be("anthropic");
    }

    #endregion

    #region GetConfiguredProviders Tests

    [Fact]
    public void GetConfiguredProviders_WithEffectiveFallbackOrder_ShouldReturnIt()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            EffectiveFallbackOrder = new List<string> { "openai", "azure" }
        };

        // Act
        var result = AIServiceHelper.GetConfiguredProviders(settings);

        // Assert
        result.Should().BeEquivalentTo(new[] { "openai", "azure" });
    }

    [Fact]
    public void GetConfiguredProviders_WithoutEffectiveFallbackOrder_ShouldComputeFromFallbackOrder()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            FallbackOrder = new List<string> { "openai", "azure", "local" },
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true },
            Azure = new LLMProviderSettingsDto { IsConfigured = false },
            Local = new LLMProviderSettingsDto { IsConfigured = true }
        };

        // Act
        var result = AIServiceHelper.GetConfiguredProviders(settings);

        // Assert
        result.Should().Contain("openai");
        result.Should().Contain("local");
        result.Should().NotContain("azure");
    }

    [Fact]
    public void GetConfiguredProviders_WithNoConfigured_AndLocalConfigured_ShouldReturnLocal()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            FallbackOrder = new List<string> { "openai", "azure" },
            OpenAI = new LLMProviderSettingsDto { IsConfigured = false },
            Azure = new LLMProviderSettingsDto { IsConfigured = false },
            Local = new LLMProviderSettingsDto { IsConfigured = true }
        };

        // Act
        var result = AIServiceHelper.GetConfiguredProviders(settings);

        // Assert
        result.Should().Contain("local");
    }

    #endregion

    #region GetProviderSettings Tests

    [Theory]
    [InlineData("openai")]
    [InlineData("azure")]
    [InlineData("anthropic")]
    [InlineData("google")]
    [InlineData("deepseek")]
    [InlineData("allenai")]
    [InlineData("local")]
    public void GetProviderSettings_WithConfiguredProvider_ShouldReturnSettings(string provider)
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "openai-model" },
            Azure = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "azure-model" },
            Anthropic = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "anthropic-model" },
            Google = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "google-model" },
            DeepSeek = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "deepseek-model" },
            AllenAI = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "allenai-model" },
            Local = new LLMProviderSettingsDto { IsConfigured = true, DefaultModel = "local-model" }
        };

        // Act
        var result = AIServiceHelper.GetProviderSettings(settings, provider);

        // Assert
        result.Should().NotBeNull();
        result!.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void GetProviderSettings_WithUnknownProvider_ShouldReturnNull()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetProviderSettings(settings, "unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetProviderSettings_WithNullProvider_ShouldReturnNull()
    {
        // Arrange
        var settings = new LLMSettingsDto();

        // Act
        var result = AIServiceHelper.GetProviderSettings(settings, null!);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("AZURE")]
    [InlineData("AnThRoPiC")]
    public void GetProviderSettings_ShouldBeCaseInsensitive(string provider)
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { DefaultModel = "test" },
            Azure = new LLMProviderSettingsDto { DefaultModel = "test" },
            Anthropic = new LLMProviderSettingsDto { DefaultModel = "test" }
        };

        // Act
        var result = AIServiceHelper.GetProviderSettings(settings, provider);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region IsProviderAvailable Tests

    [Fact]
    public void IsProviderAvailable_WithConfiguredAndEnabled_ShouldReturnTrue()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true, Enabled = true }
        };

        // Act
        var result = AIServiceHelper.IsProviderAvailable(settings, "openai");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsProviderAvailable_WithConfiguredAndEnabledNull_ShouldReturnTrue()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true, Enabled = null }
        };

        // Act
        var result = AIServiceHelper.IsProviderAvailable(settings, "openai");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsProviderAvailable_WithNotConfigured_ShouldReturnFalse()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { IsConfigured = false, Enabled = true }
        };

        // Act
        var result = AIServiceHelper.IsProviderAvailable(settings, "openai");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProviderAvailable_WithConfiguredButDisabled_ShouldReturnFalse()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = new LLMProviderSettingsDto { IsConfigured = true, Enabled = false }
        };

        // Act
        var result = AIServiceHelper.IsProviderAvailable(settings, "openai");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProviderAvailable_WithNullProviderSettings_ShouldReturnFalse()
    {
        // Arrange
        var settings = new LLMSettingsDto
        {
            OpenAI = null
        };

        // Act
        var result = AIServiceHelper.IsProviderAvailable(settings, "openai");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetValidTemperature Tests

    [Fact]
    public void GetValidTemperature_WithNullValue_ShouldReturnDefault()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidTemperature(null, 0.7);

        // Assert
        result.Should().Be(0.7);
    }

    [Fact]
    public void GetValidTemperature_WithValidValue_ShouldReturnValue()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidTemperature(0.5, 0.7);

        // Assert
        result.Should().Be(0.5);
    }

    [Fact]
    public void GetValidTemperature_WithValueBelowMin_ShouldClampToZero()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidTemperature(-0.5, 0.7);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void GetValidTemperature_WithValueAboveMax_ShouldClampToTwo()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidTemperature(2.5, 0.7);

        // Assert
        result.Should().Be(2.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void GetValidTemperature_WithBoundaryValues_ShouldReturnSameValue(double value)
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidTemperature(value, 0.7);

        // Assert
        result.Should().Be(value);
    }

    #endregion

    #region GetValidMaxTokens Tests

    [Fact]
    public void GetValidMaxTokens_WithNullValue_ShouldReturnDefault()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidMaxTokens(null, 1000);

        // Assert
        result.Should().Be(1000);
    }

    [Fact]
    public void GetValidMaxTokens_WithValidValue_ShouldReturnValue()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidMaxTokens(2000, 1000);

        // Assert
        result.Should().Be(2000);
    }

    [Fact]
    public void GetValidMaxTokens_WithValueBelowMin_ShouldClampToOne()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidMaxTokens(-100, 1000);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetValidMaxTokens_WithValueAboveMax_ShouldClampToMaximum()
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidMaxTokens(200000, 1000);

        // Assert
        result.Should().Be(128000);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4096)]
    [InlineData(128000)]
    public void GetValidMaxTokens_WithBoundaryValues_ShouldReturnSameValue(int value)
    {
        // Arrange & Act
        var result = AIServiceHelper.GetValidMaxTokens(value, 1000);

        // Assert
        result.Should().Be(value);
    }

    #endregion

    #region Provider Constants Tests

    [Fact]
    public void Providers_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        AIServiceHelper.Providers.OpenAI.Should().Be("openai");
        AIServiceHelper.Providers.Azure.Should().Be("azure");
        AIServiceHelper.Providers.Anthropic.Should().Be("anthropic");
        AIServiceHelper.Providers.Google.Should().Be("google");
        AIServiceHelper.Providers.DeepSeek.Should().Be("deepseek");
        AIServiceHelper.Providers.AllenAI.Should().Be("allenai");
        AIServiceHelper.Providers.Local.Should().Be("local");
    }

    [Fact]
    public void DefaultModels_Constants_ShouldHaveCorrectValues()
    {
        // Assert
        AIServiceHelper.DefaultModels.OpenAI.Should().Be("gpt-4o-mini");
        AIServiceHelper.DefaultModels.Azure.Should().Be("gpt-4o-mini");
        AIServiceHelper.DefaultModels.Anthropic.Should().Be("claude-3-5-sonnet-20241022");
        AIServiceHelper.DefaultModels.Google.Should().Be("gemini-pro");
        AIServiceHelper.DefaultModels.DeepSeek.Should().Be("deepseek-chat");
        AIServiceHelper.DefaultModels.AllenAI.Should().Be("allenai/OLMo-7B-Instruct");
        AIServiceHelper.DefaultModels.Local.Should().Be("llama2");
    }

    #endregion
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using CRM.Core.Ports.Output.Providers;
using CRM.Core.Features;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Factory for resolving AI/LLM provider implementations.
/// Supports runtime switching between Ollama, OpenAI, AzureOpenAI, Anthropic, Bedrock, and Gemini.
/// </summary>
public class AIProviderFactory : IProviderFactory<IAIPort>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIProviderFactory> _logger;

    public AIProviderFactory(
        IServiceProvider serviceProvider,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<AIProviderFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IAIPort GetProvider()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI)
            .GetAwaiter().GetResult();

        // Default to Ollama (local) if external is disabled
        if (!useExternal)
        {
            _logger.LogDebug("Feature flag disabled. Using Ollama (local) AI provider");
            return GetProviderByName(ProviderTypes.AI.Ollama);
        }

        var providerType = _configuration["Providers:AI:Type"] ?? ProviderTypes.AI.Ollama;
        _logger.LogDebug("Resolving AI provider: {ProviderType}", providerType);

        try
        {
            return GetProvider(providerType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve {ProviderType}. Falling back to Ollama", providerType);
            return GetProviderByName(ProviderTypes.AI.Ollama);
        }
    }

    /// <inheritdoc />
    public IAIPort GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
        }

        _logger.LogDebug("Resolving AI provider by name: {ProviderName}", providerName);
        return GetProviderByName(providerName);
    }

    private IAIPort GetProviderByName(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "ollama" => GetProviderOrFallback<IAIPort>("OllamaProvider"),
            "openai" => GetProviderOrFallback<IAIPort>("OpenAIProvider"),
            "azureopenai" => GetProviderOrFallback<IAIPort>("AzureOpenAIProvider"),
            "anthropic" => GetProviderOrFallback<IAIPort>("AnthropicAIProvider"),
            "bedrock" => GetProviderOrFallback<IAIPort>("BedrockProvider"),
            "gemini" => GetProviderOrFallback<IAIPort>("GeminiAIProvider"),
            "openrouter" => GetProviderOrFallback<IAIPort>("OpenRouterProvider"),
            _ => throw new InvalidOperationException($"Unknown AI provider: {providerName}")
        };
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableProviders()
    {
        return new[]
        {
            ProviderTypes.AI.Ollama,
            ProviderTypes.AI.OpenAI,
            ProviderTypes.AI.AzureOpenAI,
            ProviderTypes.AI.Anthropic,
            ProviderTypes.AI.Bedrock,
            ProviderTypes.AI.Gemini,
            ProviderTypes.AI.OpenRouter
        };
    }

    /// <inheritdoc />
    public string GetActiveProviderName()
    {
        var useExternal = _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI)
            .GetAwaiter().GetResult();

        if (!useExternal)
        {
            return ProviderTypes.AI.Ollama;
        }

        return _configuration["Providers:AI:Type"] ?? ProviderTypes.AI.Ollama;
    }

    /// <inheritdoc />
    public async Task<bool> IsProviderAvailableAsync(string providerName)
    {
        try
        {
            var provider = GetProvider(providerName);
            return await provider.IsAvailableAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Provider {ProviderName} is not available", providerName);
            return false;
        }
    }

    private TPort GetProviderOrFallback<TPort>(string providerTypeName) where TPort : class
    {
        var provider = ProviderResolution.ResolveByTypeName<TPort>(_serviceProvider, providerTypeName);
        if (provider != null)
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider {providerTypeName} is not registered. Ensure it is configured in appsettings and registered in DI.");
    }
}

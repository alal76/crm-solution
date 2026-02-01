using CRM.Core.Entities;

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Centralized helper class for AI/LLM service operations.
/// Consolidates common utilities used across AI controllers to avoid code duplication.
/// </summary>
public static class AIServiceHelper
{
    /// <summary>
    /// Gets the default model for a given LLM provider from settings.
    /// </summary>
    /// <param name="settings">The LLM settings DTO containing provider configurations</param>
    /// <param name="provider">The provider name (openai, azure, anthropic, google, deepseek, allenai, local)</param>
    /// <returns>The default model name for the specified provider</returns>
    public static string GetDefaultModelForProvider(LLMSettingsDto settings, string provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "openai" => settings.OpenAI?.DefaultModel ?? "gpt-4o-mini",
            "azure" => settings.Azure?.DefaultModel ?? "gpt-4o-mini",
            "anthropic" => settings.Anthropic?.DefaultModel ?? "claude-3-5-sonnet-20241022",
            "google" => settings.Google?.DefaultModel ?? "gemini-pro",
            "deepseek" => settings.DeepSeek?.DefaultModel ?? "deepseek-chat",
            "allenai" => settings.AllenAI?.DefaultModel ?? "allenai/OLMo-7B-Instruct",
            "local" => settings.Local?.DefaultModel ?? "llama2",
            _ => "gpt-4o-mini"
        };
    }

    /// <summary>
    /// Gets the first configured and enabled provider from settings.
    /// Uses EffectiveFallbackOrder which only contains configured providers.
    /// </summary>
    /// <param name="settings">The LLM settings DTO</param>
    /// <returns>The name of the first available provider, or the default provider</returns>
    public static string GetFirstAvailableProvider(LLMSettingsDto settings)
    {
        // Use effective fallback order which only contains configured providers
        if (settings.EffectiveFallbackOrder?.Count > 0)
        {
            return settings.EffectiveFallbackOrder[0];
        }

        // Legacy fallback: check providers in fallback order if effective order not computed
        if (settings.FallbackOrder?.Count > 0)
        {
            foreach (var provider in settings.FallbackOrder)
            {
                var providerSettings = GetProviderSettings(settings, provider);
                if (providerSettings?.IsConfigured == true)
                {
                    return provider;
                }
            }
        }

        // Fall back to default provider
        return settings.DefaultProvider ?? "local";
    }

    /// <summary>
    /// Gets the list of configured providers in fallback order.
    /// Returns only providers that have API keys configured.
    /// </summary>
    /// <param name="settings">The LLM settings DTO</param>
    /// <returns>List of configured provider names in fallback order</returns>
    public static List<string> GetConfiguredProviders(LLMSettingsDto settings)
    {
        // Use pre-computed effective order if available
        if (settings.EffectiveFallbackOrder?.Count > 0)
        {
            return settings.EffectiveFallbackOrder;
        }

        // Compute on demand if not available
        var configuredProviders = new List<string>();
        var allProviders = settings.FallbackOrder ?? new List<string> { "local" };

        foreach (var provider in allProviders)
        {
            var providerSettings = GetProviderSettings(settings, provider);
            if (providerSettings?.IsConfigured == true)
            {
                configuredProviders.Add(provider);
            }
        }

        // Ensure local is always available as last resort if configured
        if (configuredProviders.Count == 0 && settings.Local?.IsConfigured == true)
        {
            configuredProviders.Add("local");
        }

        return configuredProviders;
    }

    /// <summary>
    /// Gets the provider-specific settings from the main settings DTO.
    /// </summary>
    /// <param name="settings">The LLM settings DTO</param>
    /// <param name="provider">The provider name</param>
    /// <returns>The provider settings, or null if not found</returns>
    public static LLMProviderSettingsDto? GetProviderSettings(LLMSettingsDto settings, string provider)
    {
        return provider?.ToLowerInvariant() switch
        {
            "openai" => settings.OpenAI,
            "azure" => settings.Azure,
            "anthropic" => settings.Anthropic,
            "google" => settings.Google,
            "deepseek" => settings.DeepSeek,
            "allenai" => settings.AllenAI,
            "local" => settings.Local,
            _ => null
        };
    }

    /// <summary>
    /// Validates that a provider is configured and available.
    /// </summary>
    /// <param name="settings">The LLM settings DTO</param>
    /// <param name="provider">The provider to validate</param>
    /// <returns>True if the provider is configured and enabled</returns>
    public static bool IsProviderAvailable(LLMSettingsDto settings, string provider)
    {
        var providerSettings = GetProviderSettings(settings, provider);
        return providerSettings?.IsConfigured == true && providerSettings?.Enabled != false;
    }

    /// <summary>
    /// Gets the temperature value, with validation and defaults.
    /// </summary>
    /// <param name="requestedTemperature">The requested temperature (0.0-2.0)</param>
    /// <param name="defaultTemperature">The default from settings</param>
    /// <returns>A valid temperature value</returns>
    public static double GetValidTemperature(double? requestedTemperature, double defaultTemperature)
    {
        if (!requestedTemperature.HasValue)
            return defaultTemperature;

        // Clamp to valid range
        return Math.Clamp(requestedTemperature.Value, 0.0, 2.0);
    }

    /// <summary>
    /// Gets the max tokens value, with validation and defaults.
    /// </summary>
    /// <param name="requestedMaxTokens">The requested max tokens</param>
    /// <param name="defaultMaxTokens">The default from settings</param>
    /// <returns>A valid max tokens value</returns>
    public static int GetValidMaxTokens(int? requestedMaxTokens, int defaultMaxTokens)
    {
        if (!requestedMaxTokens.HasValue)
            return defaultMaxTokens;

        // Clamp to reasonable range (1 to 128000)
        return Math.Clamp(requestedMaxTokens.Value, 1, 128000);
    }

    /// <summary>
    /// Common provider names for reference
    /// </summary>
    public static class Providers
    {
        public const string OpenAI = "openai";
        public const string Azure = "azure";
        public const string Anthropic = "anthropic";
        public const string Google = "google";
        public const string DeepSeek = "deepseek";
        public const string AllenAI = "allenai";
        public const string Local = "local";
    }

    /// <summary>
    /// Default models for each provider
    /// </summary>
    public static class DefaultModels
    {
        public const string OpenAI = "gpt-4o-mini";
        public const string Azure = "gpt-4o-mini";
        public const string Anthropic = "claude-3-5-sonnet-20241022";
        public const string Google = "gemini-pro";
        public const string DeepSeek = "deepseek-chat";
        public const string AllenAI = "allenai/OLMo-7B-Instruct";
        public const string Local = "llama2";
    }
}

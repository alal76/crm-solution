// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.LLM;

/// <summary>
/// AP-036: Abstract base class for all LLM provider implementations.
/// Provides shared helpers: API key resolution, base URL resolution, prompt building.
/// </summary>
public abstract class LLMProviderBase : ILLMProvider
{
    protected readonly LLMProviderOptions Options;
    protected readonly HttpClient HttpClient;
    protected readonly ILLMSettingsService? SettingsService;
    protected readonly ILogger Logger;

    protected LLMProviderBase(
        LLMProviderOptions options,
        HttpClient httpClient,
        ILLMSettingsService? settingsService,
        ILogger logger)
    {
        Options = options;
        HttpClient = httpClient;
        SettingsService = settingsService;
        Logger = logger;
    }

    /// <inheritdoc />
    public abstract string ProviderName { get; }

    /// <inheritdoc />
    public abstract string[] SupportedAliases { get; }

    /// <inheritdoc />
    public abstract Task<LLMResponse> CallAsync(LLMRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the API key for a provider, checking DB-stored encrypted keys first, then config.
    /// AP-036: Extracted from LLMService.ResolveApiKeyAsync.
    /// </summary>
    protected async Task<string> ResolveApiKeyAsync(string provider, string configFallback)
    {
        if (SettingsService != null)
        {
            try
            {
                var key = await SettingsService.GetProviderApiKeyAsync(provider);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return key;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to resolve API key from DB for {Provider}, using config", provider);
            }
        }
        return configFallback;
    }

    /// <summary>
    /// Resolves the base URL for a provider, checking DB first, then config.
    /// AP-036: Extracted from LLMService.ResolveBaseUrlAsync.
    /// </summary>
    protected async Task<string> ResolveBaseUrlAsync(string provider, string configFallback)
    {
        if (SettingsService != null)
        {
            try
            {
                var url = await SettingsService.GetProviderBaseUrlAsync(provider);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to resolve base URL from DB for {Provider}, using config", provider);
            }
        }
        return configFallback;
    }

    /// <summary>
    /// Checks if an API key or URL is valid (not empty and not an unresolved placeholder).
    /// AP-036: Extracted from LLMService.IsValidApiKey.
    /// </summary>
    protected static bool IsValidApiKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Check for unresolved environment variable placeholders like ${VAR:} or ${VAR:default}
        if (value.StartsWith("${") && value.Contains(":"))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Builds a single prompt string from a message list for text-generation APIs (e.g., Hugging Face).
    /// AP-036: Extracted from LLMService.BuildPromptFromMessages.
    /// </summary>
    protected static string BuildPromptFromMessages(List<LLMMessage>? messages)
    {
        if (messages == null || messages.Count == 0)
        {
            return "";
        }

        var promptBuilder = new System.Text.StringBuilder();

        foreach (var message in messages)
        {
            switch (message.Role.ToLower())
            {
                case "system":
                    promptBuilder.AppendLine($"<|system|>\n{message.Content}</s>");
                    break;
                case "user":
                    promptBuilder.AppendLine($"<|user|>\n{message.Content}</s>");
                    break;
                case "assistant":
                    promptBuilder.AppendLine($"<|assistant|>\n{message.Content}</s>");
                    break;
                default:
                    promptBuilder.AppendLine(message.Content);
                    break;
            }
        }

        promptBuilder.AppendLine("<|assistant|>");
        return promptBuilder.ToString();
    }
}

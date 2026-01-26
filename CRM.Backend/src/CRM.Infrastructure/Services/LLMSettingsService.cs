/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * LLM Settings Service - Manages LLM provider settings stored in database
 */

using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing LLM settings in the database.
/// Settings from database take precedence over appsettings.json.
/// API keys are never stored in database - they remain in environment variables.
/// </summary>
public class LLMSettingsService : ILLMSettingsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LLMSettingsService> _logger;
    private readonly LLMProviderOptions _configOptions;
    private readonly ILLMService _llmService;

    public LLMSettingsService(
        ICrmDbContext context,
        ILogger<LLMSettingsService> logger,
        IOptions<LLMProviderOptions> configOptions,
        ILLMService llmService)
    {
        _context = context;
        _logger = logger;
        _configOptions = configOptions.Value;
        _llmService = llmService;
    }

    /// <summary>
    /// Gets all LLM settings, merging database values with appsettings.json defaults
    /// </summary>
    public async Task<LLMSettingsDto> GetSettingsAsync()
    {
        try
        {
            // Get all settings from database
            var dbSettings = await _context.LLMProviderSettings
                .Where(s => !s.IsDeleted)
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            // Build DTO with database values overriding config defaults
            var dto = new LLMSettingsDto
            {
                DefaultProvider = GetSettingValue(dbSettings, "DefaultProvider", _configOptions.DefaultProvider),
                EnableFallback = GetBoolSettingValue(dbSettings, "EnableFallback", _configOptions.EnableFallback),
                FallbackOrder = GetJsonSettingValue<List<string>>(dbSettings, "FallbackOrder", _configOptions.FallbackOrder.ToList()),
                DefaultMaxTokens = GetIntSettingValue(dbSettings, "DefaultMaxTokens", _configOptions.DefaultMaxTokens),
                DefaultTemperature = GetDoubleSettingValue(dbSettings, "DefaultTemperature", _configOptions.DefaultTemperature),
                TimeoutSeconds = GetIntSettingValue(dbSettings, "TimeoutSeconds", _configOptions.TimeoutSeconds),
                MaxRetries = GetIntSettingValue(dbSettings, "MaxRetries", _configOptions.MaxRetries),
                
                OpenAI = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "OpenAI.DefaultModel", _configOptions.OpenAI.DefaultModel),
                    BaseUrl = GetSettingValue(dbSettings, "OpenAI.BaseUrl", _configOptions.OpenAI.BaseUrl),
                    IsConfigured = _llmService.IsConfigured("openai")
                },
                
                Azure = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "Azure.DefaultModel", _configOptions.AzureOpenAI.DefaultModel),
                    ApiVersion = GetSettingValue(dbSettings, "Azure.ApiVersion", _configOptions.AzureOpenAI.ApiVersion),
                    IsConfigured = _llmService.IsConfigured("azure")
                },
                
                Anthropic = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "Anthropic.DefaultModel", _configOptions.Anthropic.DefaultModel),
                    BaseUrl = GetSettingValue(dbSettings, "Anthropic.BaseUrl", _configOptions.Anthropic.BaseUrl),
                    ApiVersion = GetSettingValue(dbSettings, "Anthropic.ApiVersion", _configOptions.Anthropic.ApiVersion),
                    IsConfigured = _llmService.IsConfigured("anthropic")
                },
                
                Google = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "Google.DefaultModel", _configOptions.GoogleCloud.DefaultModel),
                    Location = GetSettingValue(dbSettings, "Google.Location", _configOptions.GoogleCloud.Location),
                    UseVertexAI = GetBoolSettingValue(dbSettings, "Google.UseVertexAI", _configOptions.GoogleCloud.UseVertexAI),
                    IsConfigured = _llmService.IsConfigured("google")
                },
                
                Bedrock = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "Bedrock.DefaultModel", _configOptions.AWSBedrock.DefaultModel),
                    Region = GetSettingValue(dbSettings, "Bedrock.Region", _configOptions.AWSBedrock.Region),
                    UseDefaultCredentials = GetBoolSettingValue(dbSettings, "Bedrock.UseDefaultCredentials", _configOptions.AWSBedrock.UseDefaultCredentials),
                    IsConfigured = _llmService.IsConfigured("bedrock")
                },
                
                DeepSeek = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "DeepSeek.DefaultModel", _configOptions.DeepSeek.DefaultModel),
                    BaseUrl = GetSettingValue(dbSettings, "DeepSeek.BaseUrl", _configOptions.DeepSeek.BaseUrl),
                    IsConfigured = _llmService.IsConfigured("deepseek")
                },
                
                Local = new LLMProviderSettingsDto
                {
                    DefaultModel = GetSettingValue(dbSettings, "Local.DefaultModel", _configOptions.LocalLLM.DefaultModel),
                    BaseUrl = GetSettingValue(dbSettings, "Local.BaseUrl", _configOptions.LocalLLM.BaseUrl),
                    ApiFormat = GetSettingValue(dbSettings, "Local.ApiFormat", _configOptions.LocalLLM.ApiFormat),
                    Enabled = GetBoolSettingValue(dbSettings, "Local.Enabled", _configOptions.LocalLLM.Enabled),
                    IsConfigured = _llmService.IsConfigured("local")
                }
            };

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM settings from database");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific setting value by key
    /// </summary>
    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _context.LLMProviderSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key && !s.IsDeleted);
        return setting?.SettingValue;
    }

    /// <summary>
    /// Gets all settings for a specific category
    /// </summary>
    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
    {
        return await _context.LLMProviderSettings
            .Where(s => s.Category == category && !s.IsDeleted)
            .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
    }

    /// <summary>
    /// Updates LLM settings
    /// </summary>
    public async Task<LLMSettingsDto> UpdateSettingsAsync(UpdateLLMSettingsRequest request)
    {
        try
        {
            // Update general settings
            if (request.DefaultProvider != null)
                await SetSettingValueAsync("DefaultProvider", request.DefaultProvider, "string", "general");
            
            if (request.EnableFallback.HasValue)
                await SetSettingValueAsync("EnableFallback", request.EnableFallback.Value.ToString().ToLower(), "boolean", "general");
            
            if (request.FallbackOrder != null)
                await SetSettingValueAsync("FallbackOrder", JsonSerializer.Serialize(request.FallbackOrder), "json", "general");
            
            if (request.DefaultMaxTokens.HasValue)
                await SetSettingValueAsync("DefaultMaxTokens", request.DefaultMaxTokens.Value.ToString(), "integer", "general");
            
            if (request.DefaultTemperature.HasValue)
                await SetSettingValueAsync("DefaultTemperature", request.DefaultTemperature.Value.ToString(), "decimal", "general");
            
            if (request.TimeoutSeconds.HasValue)
                await SetSettingValueAsync("TimeoutSeconds", request.TimeoutSeconds.Value.ToString(), "integer", "general");
            
            if (request.MaxRetries.HasValue)
                await SetSettingValueAsync("MaxRetries", request.MaxRetries.Value.ToString(), "integer", "general");

            // Update provider-specific settings
            if (request.Providers != null)
            {
                foreach (var (providerKey, providerSettings) in request.Providers)
                {
                    var category = $"provider.{providerKey.ToLower()}";
                    var prefix = GetProviderPrefix(providerKey);
                    
                    if (providerSettings.DefaultModel != null)
                        await SetSettingValueAsync($"{prefix}.DefaultModel", providerSettings.DefaultModel, "string", category);
                    
                    if (providerSettings.BaseUrl != null)
                        await SetSettingValueAsync($"{prefix}.BaseUrl", providerSettings.BaseUrl, "string", category);
                    
                    if (providerSettings.ApiVersion != null)
                        await SetSettingValueAsync($"{prefix}.ApiVersion", providerSettings.ApiVersion, "string", category);
                    
                    if (providerSettings.Location != null)
                        await SetSettingValueAsync($"{prefix}.Location", providerSettings.Location, "string", category);
                    
                    if (providerSettings.Region != null)
                        await SetSettingValueAsync($"{prefix}.Region", providerSettings.Region, "string", category);
                    
                    if (providerSettings.ApiFormat != null)
                        await SetSettingValueAsync($"{prefix}.ApiFormat", providerSettings.ApiFormat, "string", category);
                    
                    if (providerSettings.Enabled.HasValue)
                        await SetSettingValueAsync($"{prefix}.Enabled", providerSettings.Enabled.Value.ToString().ToLower(), "boolean", category);
                    
                    if (providerSettings.UseVertexAI.HasValue)
                        await SetSettingValueAsync($"{prefix}.UseVertexAI", providerSettings.UseVertexAI.Value.ToString().ToLower(), "boolean", category);
                    
                    if (providerSettings.UseDefaultCredentials.HasValue)
                        await SetSettingValueAsync($"{prefix}.UseDefaultCredentials", providerSettings.UseDefaultCredentials.Value.ToString().ToLower(), "boolean", category);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("LLM settings updated successfully");
            
            return await GetSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LLM settings");
            throw;
        }
    }

    /// <summary>
    /// Sets a specific setting value
    /// </summary>
    public async Task SetSettingValueAsync(string key, string value, string valueType = "string", string category = "general", string? description = null)
    {
        var existing = await _context.LLMProviderSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        if (existing != null)
        {
            existing.SettingValue = value;
            existing.ValueType = valueType;
            existing.Category = category;
            existing.UpdatedAt = DateTime.UtcNow;
            if (description != null) existing.Description = description;
        }
        else
        {
            var newSetting = new LLMProviderSetting
            {
                SettingKey = key,
                SettingValue = value,
                ValueType = valueType,
                Category = category,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _context.LLMProviderSettings.Add(newSetting);
        }
    }

    /// <summary>
    /// Initializes default settings in database if they don't exist
    /// </summary>
    public async Task InitializeDefaultSettingsAsync()
    {
        try
        {
            var existingSettings = await _context.LLMProviderSettings.AnyAsync();
            if (existingSettings) return;

            var defaults = new List<LLMProviderSetting>
            {
                new() { SettingKey = "DefaultProvider", SettingValue = _configOptions.DefaultProvider, ValueType = "string", Category = "general", Description = "Default LLM provider to use" },
                new() { SettingKey = "EnableFallback", SettingValue = _configOptions.EnableFallback.ToString().ToLower(), ValueType = "boolean", Category = "general", Description = "Whether to fallback to other providers on failure" },
                new() { SettingKey = "FallbackOrder", SettingValue = JsonSerializer.Serialize(_configOptions.FallbackOrder), ValueType = "json", Category = "general", Description = "Order of providers for fallback" },
                new() { SettingKey = "DefaultMaxTokens", SettingValue = _configOptions.DefaultMaxTokens.ToString(), ValueType = "integer", Category = "general", Description = "Default maximum tokens for completions" },
                new() { SettingKey = "DefaultTemperature", SettingValue = _configOptions.DefaultTemperature.ToString(), ValueType = "decimal", Category = "general", Description = "Default temperature for completions" },
                new() { SettingKey = "TimeoutSeconds", SettingValue = _configOptions.TimeoutSeconds.ToString(), ValueType = "integer", Category = "general", Description = "Timeout in seconds for LLM requests" },
                new() { SettingKey = "MaxRetries", SettingValue = _configOptions.MaxRetries.ToString(), ValueType = "integer", Category = "general", Description = "Maximum retry attempts on failure" },
                
                // Provider defaults
                new() { SettingKey = "OpenAI.DefaultModel", SettingValue = _configOptions.OpenAI.DefaultModel, ValueType = "string", Category = "provider.openai" },
                new() { SettingKey = "OpenAI.BaseUrl", SettingValue = _configOptions.OpenAI.BaseUrl, ValueType = "string", Category = "provider.openai" },
                
                new() { SettingKey = "Azure.DefaultModel", SettingValue = _configOptions.AzureOpenAI.DefaultModel, ValueType = "string", Category = "provider.azure" },
                new() { SettingKey = "Azure.ApiVersion", SettingValue = _configOptions.AzureOpenAI.ApiVersion, ValueType = "string", Category = "provider.azure" },
                
                new() { SettingKey = "Anthropic.DefaultModel", SettingValue = _configOptions.Anthropic.DefaultModel, ValueType = "string", Category = "provider.anthropic" },
                new() { SettingKey = "Anthropic.BaseUrl", SettingValue = _configOptions.Anthropic.BaseUrl, ValueType = "string", Category = "provider.anthropic" },
                new() { SettingKey = "Anthropic.ApiVersion", SettingValue = _configOptions.Anthropic.ApiVersion, ValueType = "string", Category = "provider.anthropic" },
                
                new() { SettingKey = "Google.DefaultModel", SettingValue = _configOptions.GoogleCloud.DefaultModel, ValueType = "string", Category = "provider.google" },
                new() { SettingKey = "Google.Location", SettingValue = _configOptions.GoogleCloud.Location, ValueType = "string", Category = "provider.google" },
                new() { SettingKey = "Google.UseVertexAI", SettingValue = _configOptions.GoogleCloud.UseVertexAI.ToString().ToLower(), ValueType = "boolean", Category = "provider.google" },
                
                new() { SettingKey = "Bedrock.DefaultModel", SettingValue = _configOptions.AWSBedrock.DefaultModel, ValueType = "string", Category = "provider.bedrock" },
                new() { SettingKey = "Bedrock.Region", SettingValue = _configOptions.AWSBedrock.Region, ValueType = "string", Category = "provider.bedrock" },
                new() { SettingKey = "Bedrock.UseDefaultCredentials", SettingValue = _configOptions.AWSBedrock.UseDefaultCredentials.ToString().ToLower(), ValueType = "boolean", Category = "provider.bedrock" },
                
                new() { SettingKey = "DeepSeek.DefaultModel", SettingValue = _configOptions.DeepSeek.DefaultModel, ValueType = "string", Category = "provider.deepseek" },
                new() { SettingKey = "DeepSeek.BaseUrl", SettingValue = _configOptions.DeepSeek.BaseUrl, ValueType = "string", Category = "provider.deepseek" },
                
                new() { SettingKey = "Local.DefaultModel", SettingValue = _configOptions.LocalLLM.DefaultModel, ValueType = "string", Category = "provider.local" },
                new() { SettingKey = "Local.BaseUrl", SettingValue = _configOptions.LocalLLM.BaseUrl, ValueType = "string", Category = "provider.local" },
                new() { SettingKey = "Local.ApiFormat", SettingValue = _configOptions.LocalLLM.ApiFormat, ValueType = "string", Category = "provider.local" },
                new() { SettingKey = "Local.Enabled", SettingValue = _configOptions.LocalLLM.Enabled.ToString().ToLower(), ValueType = "boolean", Category = "provider.local" }
            };

            _context.LLMProviderSettings.AddRange(defaults);
            await _context.SaveChangesAsync();
            _logger.LogInformation("LLM settings initialized with {Count} default values", defaults.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default LLM settings");
            throw;
        }
    }

    /// <summary>
    /// Resets all settings to defaults from appsettings.json
    /// </summary>
    public async Task ResetToDefaultsAsync()
    {
        try
        {
            var allSettings = await _context.LLMProviderSettings.ToListAsync();
            _context.LLMProviderSettings.RemoveRange(allSettings);
            await _context.SaveChangesAsync();
            
            await InitializeDefaultSettingsAsync();
            _logger.LogInformation("LLM settings reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting LLM settings to defaults");
            throw;
        }
    }

    #region Helper Methods

    private static string GetSettingValue(Dictionary<string, string> settings, string key, string defaultValue)
        => settings.TryGetValue(key, out var value) ? value : defaultValue;

    private static bool GetBoolSettingValue(Dictionary<string, string> settings, string key, bool defaultValue)
    {
        if (settings.TryGetValue(key, out var value))
            return bool.TryParse(value, out var result) ? result : defaultValue;
        return defaultValue;
    }

    private static int GetIntSettingValue(Dictionary<string, string> settings, string key, int defaultValue)
    {
        if (settings.TryGetValue(key, out var value))
            return int.TryParse(value, out var result) ? result : defaultValue;
        return defaultValue;
    }

    private static double GetDoubleSettingValue(Dictionary<string, string> settings, string key, double defaultValue)
    {
        if (settings.TryGetValue(key, out var value))
            return double.TryParse(value, out var result) ? result : defaultValue;
        return defaultValue;
    }

    private static T GetJsonSettingValue<T>(Dictionary<string, string> settings, string key, T defaultValue) where T : class
    {
        if (settings.TryGetValue(key, out var value))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    private static string GetProviderPrefix(string providerKey) => providerKey.ToLower() switch
    {
        "openai" => "OpenAI",
        "azure" or "azureopenai" => "Azure",
        "anthropic" => "Anthropic",
        "google" or "gemini" => "Google",
        "bedrock" or "aws" => "Bedrock",
        "deepseek" => "DeepSeek",
        "local" or "ollama" => "Local",
        _ => providerKey
    };

    #endregion
}

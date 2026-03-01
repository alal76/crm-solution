// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing LLM settings in the database.
/// Settings from database take precedence over appsettings.json.
/// API keys are encrypted at rest using IEncryptionService and stored in the DB.
/// Falls back to environment-variable-based keys from IOptions if no DB key exists.
/// </summary>
public class LLMSettingsService : ILLMSettingsService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LLMSettingsService> _logger;
    private readonly LLMProviderOptions _configOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEncryptionService _encryptionService;

    public LLMSettingsService(
        ICrmDbContext context,
        ILogger<LLMSettingsService> logger,
        IOptions<LLMProviderOptions> configOptions,
        IServiceProvider serviceProvider,
        IEncryptionService encryptionService)
    {
        _context = context;
        _logger = logger;
        _configOptions = configOptions.Value;
        _serviceProvider = serviceProvider;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Gets all LLM settings, merging database values with appsettings.json defaults.
    /// API keys are masked for safe display.
    /// </summary>
    public async Task<LLMSettingsDto> GetSettingsAsync()
    {
        try
        {
            var dbSettings = await _context.LLMProviderSettings
                .Where(s => !s.IsDeleted)
                .ToListAsync();

            var settingsDict = dbSettings.ToDictionary(s => s.SettingKey, s => s.SettingValue);
            var encryptedKeys = new HashSet<string>(
                dbSettings.Where(s => s.IsEncrypted).Select(s => s.SettingKey));

            var dto = new LLMSettingsDto
            {
                DefaultProvider = GetSettingValue(settingsDict, "DefaultProvider", _configOptions.DefaultProvider),
                EnableFallback = GetBoolSettingValue(settingsDict, "EnableFallback", _configOptions.EnableFallback),
                FallbackOrder = GetJsonSettingValue<List<string>>(settingsDict, "FallbackOrder", _configOptions.FallbackOrder.ToList()),
                DefaultMaxTokens = GetIntSettingValue(settingsDict, "DefaultMaxTokens", _configOptions.DefaultMaxTokens),
                DefaultTemperature = GetDoubleSettingValue(settingsDict, "DefaultTemperature", _configOptions.DefaultTemperature),
                TimeoutSeconds = GetIntSettingValue(settingsDict, "TimeoutSeconds", _configOptions.TimeoutSeconds),
                MaxRetries = GetIntSettingValue(settingsDict, "MaxRetries", _configOptions.MaxRetries),

                OpenAI = BuildProviderDto(settingsDict, encryptedKeys, "OpenAI", "openai",
                    _configOptions.OpenAI.DefaultModel, _configOptions.OpenAI.BaseUrl, _configOptions.OpenAI.ApiKey),

                Azure = BuildProviderDto(settingsDict, encryptedKeys, "Azure", "azure",
                    _configOptions.AzureOpenAI.DefaultModel, null, _configOptions.AzureOpenAI.ApiKey,
                    apiVersion: _configOptions.AzureOpenAI.ApiVersion,
                    endpoint: _configOptions.AzureOpenAI.Endpoint,
                    deploymentName: _configOptions.AzureOpenAI.DeploymentName),

                Anthropic = BuildProviderDto(settingsDict, encryptedKeys, "Anthropic", "anthropic",
                    _configOptions.Anthropic.DefaultModel, _configOptions.Anthropic.BaseUrl, _configOptions.Anthropic.ApiKey,
                    apiVersion: _configOptions.Anthropic.ApiVersion),

                Google = BuildProviderDto(settingsDict, encryptedKeys, "Google", "google",
                    _configOptions.GoogleCloud.DefaultModel, null, _configOptions.GoogleCloud.ApiKey,
                    location: _configOptions.GoogleCloud.Location,
                    useVertexAI: _configOptions.GoogleCloud.UseVertexAI,
                    projectId: _configOptions.GoogleCloud.ProjectId),

                Bedrock = BuildProviderDto(settingsDict, encryptedKeys, "Bedrock", "bedrock",
                    _configOptions.AWSBedrock.DefaultModel, null, _configOptions.AWSBedrock.AccessKeyId,
                    region: _configOptions.AWSBedrock.Region,
                    useDefaultCredentials: _configOptions.AWSBedrock.UseDefaultCredentials),

                DeepSeek = BuildProviderDto(settingsDict, encryptedKeys, "DeepSeek", "deepseek",
                    _configOptions.DeepSeek.DefaultModel, _configOptions.DeepSeek.BaseUrl, _configOptions.DeepSeek.ApiKey),

                Groq = BuildProviderDto(settingsDict, encryptedKeys, "Groq", "groq",
                    _configOptions.Groq.DefaultModel, _configOptions.Groq.BaseUrl, _configOptions.Groq.ApiKey),

                AllenAI = BuildProviderDto(settingsDict, encryptedKeys, "AllenAI", "allenai",
                    _configOptions.AllenAI.DefaultModel, _configOptions.AllenAI.BaseUrl, _configOptions.AllenAI.ApiKey,
                    enabled: _configOptions.AllenAI.Enabled),

                Local = BuildProviderDto(settingsDict, encryptedKeys, "Local", "local",
                    _configOptions.LocalLLM.DefaultModel, _configOptions.LocalLLM.BaseUrl, _configOptions.LocalLLM.ApiKey,
                    apiFormat: _configOptions.LocalLLM.ApiFormat,
                    enabled: _configOptions.LocalLLM.Enabled),

                Custom = BuildProviderDto(settingsDict, encryptedKeys, "Custom", "custom",
                    "", _configOptions.CustomEndpoint.Url, _configOptions.CustomEndpoint.ApiKey)
            };

            dto.EffectiveFallbackOrder = ComputeEffectiveFallbackOrder(dto);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM settings from database");
            throw;
        }
    }

    private LLMProviderSettingsDto BuildProviderDto(
        Dictionary<string, string> settingsDict,
        HashSet<string> encryptedKeys,
        string prefix,
        string providerKey,
        string configModel,
        string? configBaseUrl,
        string? configApiKey,
        string? apiVersion = null,
        string? location = null,
        string? region = null,
        string? apiFormat = null,
        bool? enabled = null,
        bool? useVertexAI = null,
        bool? useDefaultCredentials = null,
        string? endpoint = null,
        string? deploymentName = null,
        string? projectId = null)
    {
        var dto = new LLMProviderSettingsDto
        {
            DefaultModel = GetSettingValue(settingsDict, $"{prefix}.DefaultModel", configModel),
            BaseUrl = GetSettingValue(settingsDict, $"{prefix}.BaseUrl", configBaseUrl ?? ""),
            ApiVersion = apiVersion != null ? GetSettingValue(settingsDict, $"{prefix}.ApiVersion", apiVersion) : GetNullableSettingValue(settingsDict, $"{prefix}.ApiVersion"),
            Location = location != null ? GetSettingValue(settingsDict, $"{prefix}.Location", location) : GetNullableSettingValue(settingsDict, $"{prefix}.Location"),
            Region = region != null ? GetSettingValue(settingsDict, $"{prefix}.Region", region) : GetNullableSettingValue(settingsDict, $"{prefix}.Region"),
            ApiFormat = apiFormat != null ? GetSettingValue(settingsDict, $"{prefix}.ApiFormat", apiFormat) : GetNullableSettingValue(settingsDict, $"{prefix}.ApiFormat"),
            UseVertexAI = useVertexAI != null ? GetBoolSettingValue(settingsDict, $"{prefix}.UseVertexAI", useVertexAI.Value) : GetNullableBoolSettingValue(settingsDict, $"{prefix}.UseVertexAI"),
            UseDefaultCredentials = useDefaultCredentials != null ? GetBoolSettingValue(settingsDict, $"{prefix}.UseDefaultCredentials", useDefaultCredentials.Value) : GetNullableBoolSettingValue(settingsDict, $"{prefix}.UseDefaultCredentials"),
            Endpoint = endpoint != null ? GetSettingValue(settingsDict, $"{prefix}.Endpoint", endpoint) : GetNullableSettingValue(settingsDict, $"{prefix}.Endpoint"),
            DeploymentName = deploymentName != null ? GetSettingValue(settingsDict, $"{prefix}.DeploymentName", deploymentName) : GetNullableSettingValue(settingsDict, $"{prefix}.DeploymentName"),
            ProjectId = projectId != null ? GetSettingValue(settingsDict, $"{prefix}.ProjectId", projectId) : GetNullableSettingValue(settingsDict, $"{prefix}.ProjectId"),
        };

        if (enabled != null)
            dto.Enabled = GetBoolSettingValue(settingsDict, $"{prefix}.Enabled", enabled.Value);
        else
            dto.Enabled = GetNullableBoolSettingValue(settingsDict, $"{prefix}.Enabled");

        var apiKeySettingKey = $"{prefix}.ApiKey";
        bool hasDbKey = settingsDict.ContainsKey(apiKeySettingKey) &&
                        !string.IsNullOrWhiteSpace(settingsDict[apiKeySettingKey]);
        bool hasConfigKey = IsValidApiKey(configApiKey);

        dto.HasApiKey = hasDbKey || hasConfigKey;
        dto.ApiKeyMasked = GetMaskedApiKey(settingsDict, encryptedKeys, apiKeySettingKey, configApiKey);
        dto.IsConfigured = DetermineIsConfigured(providerKey, dto);

        return dto;
    }

    private static bool DetermineIsConfigured(string providerKey, LLMProviderSettingsDto dto)
    {
        return providerKey switch
        {
            "local" or "ollama" => (dto.Enabled ?? false) && !string.IsNullOrWhiteSpace(dto.BaseUrl),
            "bedrock" or "aws" => (dto.UseDefaultCredentials ?? false) || dto.HasApiKey,
            "custom" => !string.IsNullOrWhiteSpace(dto.BaseUrl) && dto.HasApiKey,
            _ => dto.HasApiKey
        };
    }

    private string? GetMaskedApiKey(
        Dictionary<string, string> settingsDict,
        HashSet<string> encryptedKeys,
        string dbKey,
        string? configKey)
    {
        if (settingsDict.TryGetValue(dbKey, out var dbVal) && !string.IsNullOrWhiteSpace(dbVal))
        {
            string? realKey = encryptedKeys.Contains(dbKey) ? _encryptionService.Decrypt(dbVal) : dbVal;
            return MaskApiKey(realKey);
        }
        if (IsValidApiKey(configKey))
            return MaskApiKey(configKey);
        return null;
    }

    private static string? MaskApiKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length < 8)
            return key != null ? "****" : null;
        return $"{key[..4]}****{key[^4..]}";
    }

    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _context.LLMProviderSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key && !s.IsDeleted);
        if (setting == null) return null;
        if (setting.IsEncrypted)
            return _encryptionService.Decrypt(setting.SettingValue);
        return setting.SettingValue;
    }

    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
    {
        var settings = await _context.LLMProviderSettings
            .Where(s => s.Category == category && !s.IsDeleted)
            .ToListAsync();

        var dict = new Dictionary<string, string>();
        foreach (var s in settings)
        {
            dict[s.SettingKey] = s.IsEncrypted
                ? _encryptionService.Decrypt(s.SettingValue) ?? ""
                : s.SettingValue;
        }
        return dict;
    }

    public async Task<LLMSettingsDto> UpdateSettingsAsync(UpdateLLMSettingsRequest request)
    {
        try
        {
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

            if (request.Providers != null)
            {
                foreach (var (providerKey, ps) in request.Providers)
                {
                    var category = $"provider.{providerKey.ToLower()}";
                    var prefix = GetProviderPrefix(providerKey);

                    if (ps.DefaultModel != null)
                        await SetSettingValueAsync($"{prefix}.DefaultModel", ps.DefaultModel, "string", category);
                    if (ps.BaseUrl != null)
                        await SetSettingValueAsync($"{prefix}.BaseUrl", ps.BaseUrl, "string", category);
                    if (ps.ApiVersion != null)
                        await SetSettingValueAsync($"{prefix}.ApiVersion", ps.ApiVersion, "string", category);
                    if (ps.Location != null)
                        await SetSettingValueAsync($"{prefix}.Location", ps.Location, "string", category);
                    if (ps.Region != null)
                        await SetSettingValueAsync($"{prefix}.Region", ps.Region, "string", category);
                    if (ps.ApiFormat != null)
                        await SetSettingValueAsync($"{prefix}.ApiFormat", ps.ApiFormat, "string", category);
                    if (ps.Enabled.HasValue)
                        await SetSettingValueAsync($"{prefix}.Enabled", ps.Enabled.Value.ToString().ToLower(), "boolean", category);
                    if (ps.UseVertexAI.HasValue)
                        await SetSettingValueAsync($"{prefix}.UseVertexAI", ps.UseVertexAI.Value.ToString().ToLower(), "boolean", category);
                    if (ps.UseDefaultCredentials.HasValue)
                        await SetSettingValueAsync($"{prefix}.UseDefaultCredentials", ps.UseDefaultCredentials.Value.ToString().ToLower(), "boolean", category);
                    if (ps.Endpoint != null)
                        await SetSettingValueAsync($"{prefix}.Endpoint", ps.Endpoint, "string", category);
                    if (ps.DeploymentName != null)
                        await SetSettingValueAsync($"{prefix}.DeploymentName", ps.DeploymentName, "string", category);
                    if (ps.ProjectId != null)
                        await SetSettingValueAsync($"{prefix}.ProjectId", ps.ProjectId, "string", category);

                    // Handle API key: encrypt before storage
                    if (ps.ApiKey != null)
                    {
                        if (string.IsNullOrWhiteSpace(ps.ApiKey))
                        {
                            // Empty string = clear the API key
                            await RemoveSettingAsync($"{prefix}.ApiKey");
                        }
                        else
                        {
                            var encrypted = _encryptionService.Encrypt(ps.ApiKey);
                            await SetSettingValueAsync($"{prefix}.ApiKey", encrypted, "string", category,
                                $"Encrypted API key for {prefix}", isEncrypted: true);
                        }
                    }
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

    public async Task SetSettingValueAsync(string key, string value, string valueType = "string", string category = "general", string? description = null, bool isEncrypted = false)
    {
        var existing = await _context.LLMProviderSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        if (existing != null)
        {
            existing.SettingValue = value;
            existing.ValueType = valueType;
            existing.Category = category;
            existing.IsEncrypted = isEncrypted;
            if (description != null)
                existing.Description = description;
        }
        else
        {
            _context.LLMProviderSettings.Add(new LLMProviderSetting
            {
                SettingKey = key,
                SettingValue = value,
                ValueType = valueType,
                Category = category,
                Description = description,
                IsEncrypted = isEncrypted,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task RemoveSettingAsync(string key)
    {
        var existing = await _context.LLMProviderSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        if (existing != null)
            _context.LLMProviderSettings.Remove(existing);
    }

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
                new() { SettingKey = "DefaultMaxTokens", SettingValue = _configOptions.DefaultMaxTokens.ToString(), ValueType = "integer", Category = "general" },
                new() { SettingKey = "DefaultTemperature", SettingValue = _configOptions.DefaultTemperature.ToString(), ValueType = "decimal", Category = "general" },
                new() { SettingKey = "TimeoutSeconds", SettingValue = _configOptions.TimeoutSeconds.ToString(), ValueType = "integer", Category = "general" },
                new() { SettingKey = "MaxRetries", SettingValue = _configOptions.MaxRetries.ToString(), ValueType = "integer", Category = "general" },

                new() { SettingKey = "OpenAI.DefaultModel", SettingValue = _configOptions.OpenAI.DefaultModel, ValueType = "string", Category = "provider.openai" },
                new() { SettingKey = "OpenAI.BaseUrl", SettingValue = _configOptions.OpenAI.BaseUrl, ValueType = "string", Category = "provider.openai" },
                new() { SettingKey = "Azure.DefaultModel", SettingValue = _configOptions.AzureOpenAI.DefaultModel, ValueType = "string", Category = "provider.azure" },
                new() { SettingKey = "Azure.ApiVersion", SettingValue = _configOptions.AzureOpenAI.ApiVersion, ValueType = "string", Category = "provider.azure" },
                new() { SettingKey = "Anthropic.DefaultModel", SettingValue = _configOptions.Anthropic.DefaultModel, ValueType = "string", Category = "provider.anthropic" },
                new() { SettingKey = "Anthropic.BaseUrl", SettingValue = _configOptions.Anthropic.BaseUrl, ValueType = "string", Category = "provider.anthropic" },
                new() { SettingKey = "Google.DefaultModel", SettingValue = _configOptions.GoogleCloud.DefaultModel, ValueType = "string", Category = "provider.google" },
                new() { SettingKey = "Google.Location", SettingValue = _configOptions.GoogleCloud.Location, ValueType = "string", Category = "provider.google" },
                new() { SettingKey = "Bedrock.DefaultModel", SettingValue = _configOptions.AWSBedrock.DefaultModel, ValueType = "string", Category = "provider.bedrock" },
                new() { SettingKey = "Bedrock.Region", SettingValue = _configOptions.AWSBedrock.Region, ValueType = "string", Category = "provider.bedrock" },
                new() { SettingKey = "DeepSeek.DefaultModel", SettingValue = _configOptions.DeepSeek.DefaultModel, ValueType = "string", Category = "provider.deepseek" },
                new() { SettingKey = "DeepSeek.BaseUrl", SettingValue = _configOptions.DeepSeek.BaseUrl, ValueType = "string", Category = "provider.deepseek" },
                new() { SettingKey = "Groq.DefaultModel", SettingValue = _configOptions.Groq.DefaultModel, ValueType = "string", Category = "provider.groq" },
                new() { SettingKey = "Groq.BaseUrl", SettingValue = _configOptions.Groq.BaseUrl, ValueType = "string", Category = "provider.groq" },
                new() { SettingKey = "AllenAI.DefaultModel", SettingValue = _configOptions.AllenAI.DefaultModel, ValueType = "string", Category = "provider.allenai" },
                new() { SettingKey = "AllenAI.BaseUrl", SettingValue = _configOptions.AllenAI.BaseUrl, ValueType = "string", Category = "provider.allenai" },
                new() { SettingKey = "AllenAI.Enabled", SettingValue = _configOptions.AllenAI.Enabled.ToString().ToLower(), ValueType = "boolean", Category = "provider.allenai" },
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

    #region Runtime Settings Resolution (used by LLMService)

    public async Task<string?> GetProviderApiKeyAsync(string providerName)
    {
        var prefix = GetProviderPrefix(providerName);
        var dbKey = await GetSettingValueAsync($"{prefix}.ApiKey");
        if (!string.IsNullOrWhiteSpace(dbKey))
            return dbKey;

        return providerName.ToLower() switch
        {
            "openai" => GetConfigKeyOrNull(_configOptions.OpenAI.ApiKey),
            "azure" or "azureopenai" => GetConfigKeyOrNull(_configOptions.AzureOpenAI.ApiKey),
            "anthropic" => GetConfigKeyOrNull(_configOptions.Anthropic.ApiKey),
            "google" or "gemini" => GetConfigKeyOrNull(_configOptions.GoogleCloud.ApiKey),
            "bedrock" or "aws" => GetConfigKeyOrNull(_configOptions.AWSBedrock.AccessKeyId),
            "deepseek" => GetConfigKeyOrNull(_configOptions.DeepSeek.ApiKey),
            "groq" => GetConfigKeyOrNull(_configOptions.Groq.ApiKey),
            "allenai" or "huggingface" => GetConfigKeyOrNull(_configOptions.AllenAI.ApiKey),
            "local" or "ollama" => GetConfigKeyOrNull(_configOptions.LocalLLM.ApiKey),
            "custom" => GetConfigKeyOrNull(_configOptions.CustomEndpoint.ApiKey),
            _ => null
        };
    }

    public async Task<string?> GetProviderBaseUrlAsync(string providerName)
    {
        var prefix = GetProviderPrefix(providerName);
        var dbValue = await GetSettingValueAsync($"{prefix}.BaseUrl");
        if (!string.IsNullOrWhiteSpace(dbValue))
            return dbValue;

        return providerName.ToLower() switch
        {
            "openai" => _configOptions.OpenAI.BaseUrl,
            "azure" or "azureopenai" => _configOptions.AzureOpenAI.Endpoint,
            "anthropic" => _configOptions.Anthropic.BaseUrl,
            "deepseek" => _configOptions.DeepSeek.BaseUrl,
            "groq" => _configOptions.Groq.BaseUrl,
            "allenai" or "huggingface" => _configOptions.AllenAI.BaseUrl,
            "local" or "ollama" => _configOptions.LocalLLM.BaseUrl,
            "custom" => _configOptions.CustomEndpoint.Url,
            _ => null
        };
    }

    public async Task<Dictionary<string, string>> GetProviderRuntimeSettingsAsync(string providerName)
    {
        try
        {
            var prefix = GetProviderPrefix(providerName);
            var category = $"provider.{providerName.ToLower()}";
            var dbSettings = await GetSettingsByCategoryAsync(category);
            var result = new Dictionary<string, string>(dbSettings, StringComparer.OrdinalIgnoreCase);

            var apiKey = await GetProviderApiKeyAsync(providerName);
            if (!string.IsNullOrWhiteSpace(apiKey))
                result[$"{prefix}.ApiKey"] = apiKey;

            var baseUrl = await GetProviderBaseUrlAsync(providerName);
            if (!string.IsNullOrWhiteSpace(baseUrl))
                result[$"{prefix}.BaseUrl"] = baseUrl;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting runtime settings for provider {Provider}", providerName);
            return new Dictionary<string, string>();
        }
    }

    public async Task<(bool Success, string Message)> TestProviderConnectionAsync(string providerName)
    {
        try
        {
            var apiKey = await GetProviderApiKeyAsync(providerName);
            if (string.IsNullOrWhiteSpace(apiKey) && providerName.ToLower() != "local")
                return (false, "No API key configured. Please set the API key first.");

            // Resolve ILLMService lazily to avoid circular dependency
            var llmService = _serviceProvider.GetRequiredService<ILLMService>();

            var testRequest = new LLMRequest
            {
                Provider = providerName,
                Prompt = "Hello",
                MaxTokens = 5,
                Temperature = 0
            };

            var response = await llmService.CompletionAsync(testRequest);
            if (response.Success)
                return (true, $"Connection successful! Model: {response.Model}, Tokens: {response.TotalTokens}");
            return (false, response.Error ?? "Connection failed with no error details.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Provider connection test failed for {Provider}", providerName);
            return (false, $"Connection failed: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private static string GetSettingValue(Dictionary<string, string> settings, string key, string defaultValue)
        => settings.TryGetValue(key, out var value) ? value : defaultValue;

    private static string? GetNullableSettingValue(Dictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value) ? value : null;

    private static bool GetBoolSettingValue(Dictionary<string, string> settings, string key, bool defaultValue)
    {
        if (settings.TryGetValue(key, out var value))
            return bool.TryParse(value, out var result) ? result : defaultValue;
        return defaultValue;
    }

    private static bool? GetNullableBoolSettingValue(Dictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out var value))
            return bool.TryParse(value, out var result) ? result : null;
        return null;
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
            try { return JsonSerializer.Deserialize<T>(value) ?? defaultValue; }
            catch { return defaultValue; }
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
        "groq" => "Groq",
        "allenai" or "huggingface" or "ai2" => "AllenAI",
        "local" or "ollama" => "Local",
        "custom" => "Custom",
        _ => providerKey
    };

    private static string? GetConfigKeyOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (value.StartsWith("${") && value.Contains(":")) return null;
        return value;
    }

    private static bool IsValidApiKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.StartsWith("${") && value.Contains(":")) return false;
        return true;
    }

    private static List<string> ComputeEffectiveFallbackOrder(LLMSettingsDto dto)
    {
        var effectiveOrder = new List<string>();
        var providerConfigStatus = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            { "openai", dto.OpenAI?.IsConfigured ?? false },
            { "azure", dto.Azure?.IsConfigured ?? false },
            { "anthropic", dto.Anthropic?.IsConfigured ?? false },
            { "google", dto.Google?.IsConfigured ?? false },
            { "bedrock", dto.Bedrock?.IsConfigured ?? false },
            { "deepseek", dto.DeepSeek?.IsConfigured ?? false },
            { "groq", dto.Groq?.IsConfigured ?? false },
            { "allenai", dto.AllenAI?.IsConfigured ?? false },
            { "local", dto.Local?.IsConfigured ?? false },
            { "custom", dto.Custom?.IsConfigured ?? false }
        };

        if (!string.IsNullOrEmpty(dto.DefaultProvider) &&
            providerConfigStatus.TryGetValue(dto.DefaultProvider, out var isDefault) && isDefault)
            effectiveOrder.Add(dto.DefaultProvider.ToLower());

        if (dto.FallbackOrder != null)
        {
            foreach (var p in dto.FallbackOrder)
            {
                var n = p.ToLower();
                if (effectiveOrder.Contains(n)) continue;
                if (providerConfigStatus.TryGetValue(n, out var ic) && ic)
                    effectiveOrder.Add(n);
            }
        }

        if (effectiveOrder.Count == 0 &&
            providerConfigStatus.TryGetValue("local", out var lc) && lc)
            effectiveOrder.Add("local");

        return effectiveOrder;
    }

    #endregion
}

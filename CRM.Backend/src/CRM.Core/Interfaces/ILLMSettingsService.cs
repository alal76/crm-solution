// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing LLM provider settings stored in database.
/// Also provides runtime-resolved configuration for LLMService (DB values override appsettings).
/// </summary>
public interface ILLMSettingsService
{
    /// <summary>
    /// Gets all LLM settings from database, merged with appsettings.json defaults.
    /// API keys are returned masked (for admin UI display).
    /// </summary>
    Task<LLMSettingsDto> GetSettingsAsync();

    /// <summary>
    /// Gets a specific setting value by key
    /// </summary>
    Task<string?> GetSettingValueAsync(string key);

    /// <summary>
    /// Gets all settings for a specific category
    /// </summary>
    Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);

    /// <summary>
    /// Updates LLM settings (partial update supported).
    /// API keys are encrypted before storage.
    /// </summary>
    Task<LLMSettingsDto> UpdateSettingsAsync(UpdateLLMSettingsRequest request);

    /// <summary>
    /// Sets a specific setting value
    /// </summary>
    Task SetSettingValueAsync(string key, string value, string valueType = "string", string category = "general", string? description = null, bool isEncrypted = false);

    /// <summary>
    /// Initializes default settings in database if they don't exist
    /// </summary>
    Task InitializeDefaultSettingsAsync();

    /// <summary>
    /// Resets all settings to defaults from appsettings.json
    /// </summary>
    Task ResetToDefaultsAsync();

    /// <summary>
    /// Gets the decrypted API key for a provider from the database.
    /// Falls back to appsettings.json/env vars if no DB key exists.
    /// Used internally by LLMService for actual API calls.
    /// </summary>
    Task<string?> GetProviderApiKeyAsync(string providerName);

    /// <summary>
    /// Gets the effective base URL for a provider (DB value overrides config).
    /// Used internally by LLMService for actual API calls.
    /// </summary>
    Task<string?> GetProviderBaseUrlAsync(string providerName);

    /// <summary>
    /// Gets all effective runtime settings for a provider (merging DB over config).
    /// Returns a dictionary of setting key → value (API keys are decrypted).
    /// </summary>
    Task<Dictionary<string, string>> GetProviderRuntimeSettingsAsync(string providerName);

    /// <summary>
    /// Tests connectivity to a specific LLM provider using the stored or provided API key.
    /// Returns success/error message.
    /// </summary>
    Task<(bool Success, string Message)> TestProviderConnectionAsync(string providerName);
}

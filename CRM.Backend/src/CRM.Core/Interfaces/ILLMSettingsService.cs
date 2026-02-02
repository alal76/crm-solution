// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * LLM Settings Service Interface
 */

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing LLM provider settings stored in database
/// </summary>
public interface ILLMSettingsService
{
    /// <summary>
    /// Gets all LLM settings from database, merged with appsettings.json defaults
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
    /// Updates LLM settings (partial update supported)
    /// </summary>
    Task<LLMSettingsDto> UpdateSettingsAsync(UpdateLLMSettingsRequest request);

    /// <summary>
    /// Sets a specific setting value
    /// </summary>
    Task SetSettingValueAsync(string key, string value, string valueType = "string", string category = "general", string? description = null);

    /// <summary>
    /// Initializes default settings in database if they don't exist
    /// </summary>
    Task InitializeDefaultSettingsAsync();

    /// <summary>
    /// Resets all settings to defaults from appsettings.json
    /// </summary>
    Task ResetToDefaultsAsync();
}

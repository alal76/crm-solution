// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing system settings
/// </summary>
public interface ISystemSettingsService
{
    /// <summary>
    /// Gets the current system settings
    /// </summary>
    Task<SystemSettingsDto> GetSettingsAsync();

    /// <summary>
    /// Gets the module status for frontend permission checking
    /// </summary>
    Task<ModuleStatusDto> GetModuleStatusAsync();

    /// <summary>
    /// Updates system settings (partial update supported)
    /// </summary>
    Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, int? modifiedByUserId = null);
}

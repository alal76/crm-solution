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

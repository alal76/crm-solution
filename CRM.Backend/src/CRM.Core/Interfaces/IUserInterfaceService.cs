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
/// Service interface for managing user UI preferences and customizations
/// </summary>
public interface IUserInterfaceService
{
    /// <summary>
    /// Get UI preferences for a user
    /// </summary>
    Task<UIPreferenceDto?> GetUserUIPreferencesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update UI preferences for a user
    /// </summary>
    Task<UIPreferenceDto> SaveUIPreferencesAsync(int userId, CreateUpdateUIPreferenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset UI preferences to defaults for a user
    /// </summary>
    Task<bool> ResetUIPreferencesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get UI customization for a specific module/page
    /// </summary>
    Task<UICustomizationDto?> GetUICustomizationAsync(int userId, string moduleName, string pageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all UI customizations for a user
    /// </summary>
    Task<IEnumerable<UICustomizationDto>> GetAllUICustomizationsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save UI customization for a module/page
    /// </summary>
    Task<UICustomizationDto> SaveUICustomizationAsync(int userId, CreateUpdateUICustomizationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete UI customization for a module
    /// </summary>
    Task<bool> DeleteUICustomizationAsync(int userId, string moduleName, string pageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dashboard customization
    /// </summary>
    Task<DashboardCustomizationDto?> GetDashboardCustomizationAsync(int userId, string dashboardName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all dashboard customizations for a user
    /// </summary>
    Task<IEnumerable<DashboardCustomizationDto>> GetAllDashboardCustomizationsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save dashboard customization
    /// </summary>
    Task<DashboardCustomizationDto> SaveDashboardCustomizationAsync(int userId, CreateUpdateDashboardCustomizationDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete dashboard customization
    /// </summary>
    Task<bool> DeleteDashboardCustomizationAsync(int userId, string dashboardName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set default dashboard for a user
    /// </summary>
    Task<bool> SetDefaultDashboardAsync(int userId, string dashboardName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get saved views/searches for a module
    /// </summary>
    Task<string[]> GetSavedViewsAsync(int userId, string moduleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a view/search for a module
    /// </summary>
    Task<bool> SaveViewAsync(int userId, string moduleName, string viewName, object filterCriteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a saved view
    /// </summary>
    Task<bool> DeleteViewAsync(int userId, string moduleName, string viewName, CancellationToken cancellationToken = default);
}

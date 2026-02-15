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
/// Service interface for managing feature flags, targeting, variants, and audit trails
/// </summary>
public interface IFeatureFlagManagementService
{
    /// <summary>
    /// Get all feature flags (module and provider flags)
    /// </summary>
    Task<IEnumerable<FeatureFlagDto>> GetAllFlagsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific feature flag by name
    /// </summary>
    Task<FeatureFlagDto?> GetFlagAsync(string flagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a flag is enabled for a specific user with targeting
    /// </summary>
    Task<bool> IsFlagEnabledForUserAsync(string flagName, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a feature flag (enable/disable, set rollout percentage)
    /// </summary>
    Task<bool> UpdateFlagAsync(string flagName, UpdateFeatureFlagDto dto, int updatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set rollout percentage for a feature flag (0-100)
    /// </summary>
    Task<bool> SetRolloutPercentageAsync(string flagName, int percentage, int updatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set A/B testing variants for a flag
    /// </summary>
    Task<bool> SetVariantsAsync(string flagName, FlagVariantDto[] variants, int updatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get variant assignment for a user (A/B testing)
    /// </summary>
    Task<FlagVariantDto?> GetUserVariantAsync(string flagName, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the active provider for a category
    /// </summary>
    Task<string> GetActiveProviderAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the active provider for a category
    /// </summary>
    Task<bool> UpdateProviderTypeAsync(string category, string providerType, int updatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available providers for a category
    /// </summary>
    Task<IEnumerable<string>> GetAvailableProvidersAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit log for feature flag changes
    /// </summary>
    Task<IEnumerable<FeatureFlagAuditEntryDto>> GetAuditLogAsync(int count = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit log for a specific flag
    /// </summary>
    Task<IEnumerable<FeatureFlagAuditEntryDto>> GetFlagAuditLogAsync(string flagName, int count = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset all flags to default values
    /// </summary>
    Task<bool> ResetToDefaultsAsync(int updatedById, CancellationToken cancellationToken = default);
}

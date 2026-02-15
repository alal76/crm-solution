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
/// Service interface for managing Account/Contact communication preferences.
/// </summary>
public interface IPreferencesService
{
    Task<PreferencesDto?> GetByIdAsync(int preferencesId, CancellationToken cancellationToken = default);

    Task<PreferencesDto> GetAccountDefaultsAsync(int accountId, CancellationToken cancellationToken = default);

    Task<PreferencesDto> GetContactOverridesAsync(int contactId, CancellationToken cancellationToken = default);

    Task<PreferencesDto> GetEffectivePreferencesAsync(int contactId, CancellationToken cancellationToken = default);

    Task<PreferencesDto> UpdateAccountPreferencesAsync(int accountId, PreferencesDto dto, CancellationToken cancellationToken = default);

    Task<PreferencesDto> UpdateContactPreferencesAsync(int contactId, PreferencesDto dto, CancellationToken cancellationToken = default);

    Task<ContactPreferencesDto> GetContactPreferencesAsync(int contactId, bool effective = false, CancellationToken cancellationToken = default);

    Task SetContactUseCustomPreferencesAsync(int contactId, bool useCustomPreferences, CancellationToken cancellationToken = default);

    Task<CRM.Core.Models.Contact> ResetContactToAccountAsync(int contactId, CancellationToken cancellationToken = default);

    Task<int> BulkSetDefaultsAsync(int accountId, PreferencesDto dto, CancellationToken cancellationToken = default);
}

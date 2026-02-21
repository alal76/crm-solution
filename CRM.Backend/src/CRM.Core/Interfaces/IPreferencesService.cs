// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

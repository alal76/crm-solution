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
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing account default preferences and contact overrides.
/// </summary>
public class PreferencesService : IPreferencesService
{
    private const string AccountDefaultsCachePrefix = "Preferences:AccountDefaults:";
    private const string ContactOverridesCachePrefix = "Preferences:ContactOverrides:";
    private const string ContactEffectiveCachePrefix = "Preferences:ContactEffective:";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly ICrmDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PreferencesService> _logger;

    public PreferencesService(
        ICrmDbContext context,
        IMemoryCache cache,
        ILogger<PreferencesService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PreferencesDto?> GetByIdAsync(int preferencesId, CancellationToken cancellationToken = default)
    {
        var preferences = await _context.Preferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == preferencesId && !p.IsDeleted, cancellationToken);

        return preferences == null ? null : MapToDto(preferences);
    }

    /// <inheritdoc />
    public async Task<PreferencesDto> GetAccountDefaultsAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetAccountDefaultsCacheKey(accountId);
        if (_cache.TryGetValue(cacheKey, out PreferencesDto? cached) && cached != null)
        {
            return cached;
        }

        var account = await _context.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {accountId} not found.");
        }

        if (account.Preferences == null || account.Preferences.IsDeleted)
        {
            account.Preferences = CreateDefaultPreferences();
            account.Preferences.CreatedAt = DateTime.UtcNow;
            account.Preferences.UpdatedAt = DateTime.UtcNow;
            account.Preferences.IsDeleted = false;

            _context.Preferences.Add(account.Preferences);
            await _context.SaveChangesAsync(cancellationToken);

            account.PreferencesId = account.Preferences.Id;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created default preferences for account {AccountId}", accountId);
        }

        var dto = MapToDto(account.Preferences);
        _cache.Set(cacheKey, dto, CacheTtl);
        return dto;
    }

    /// <inheritdoc />
    public async Task<PreferencesDto> GetContactOverridesAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetContactOverridesCacheKey(contactId);
        if (_cache.TryGetValue(cacheKey, out PreferencesDto? cached) && cached != null)
        {
            return cached;
        }

        var contact = await _context.Contacts
            .Include(c => c.Preferences)
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        PreferencesDto dto;

        if (contact.Preferences != null && !contact.Preferences.IsDeleted)
        {
            dto = MapToDto(contact.Preferences);
        }
        else
        {
            dto = CreateDefaultDto();
        }

        _cache.Set(cacheKey, dto, CacheTtl);
        return dto;
    }

    /// <inheritdoc />
    public async Task<PreferencesDto> GetEffectivePreferencesAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetContactEffectiveCacheKey(contactId);
        if (_cache.TryGetValue(cacheKey, out PreferencesDto? cached) && cached != null)
        {
            return cached;
        }

        var contact = await _context.Contacts
            .Include(c => c.Preferences)
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        PreferencesDto dto;
        if (contact.UseCustomPreferences && contact.Preferences != null && !contact.Preferences.IsDeleted)
        {
            dto = MapToDto(contact.Preferences);
        }
        else if (contact.AccountId.HasValue)
        {
            dto = await GetAccountDefaultsAsync(contact.AccountId.Value, cancellationToken);
        }
        else if (contact.Preferences != null && !contact.Preferences.IsDeleted)
        {
            dto = MapToDto(contact.Preferences);
        }
        else
        {
            dto = CreateDefaultDto();
        }

        _cache.Set(cacheKey, dto, CacheTtl);
        return dto;
    }

    /// <inheritdoc />
    public async Task<PreferencesDto> UpdateAccountPreferencesAsync(int accountId, PreferencesDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDates(dto);

        var account = await _context.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {accountId} not found.");
        }

        if (account.Preferences == null || account.Preferences.IsDeleted)
        {
            account.Preferences = CreateDefaultPreferences();
            account.Preferences.CreatedAt = DateTime.UtcNow;
            account.Preferences.IsDeleted = false;
            _context.Preferences.Add(account.Preferences);
        }

        ApplyDto(account.Preferences, dto);
        account.Preferences.UpdatedAt = DateTime.UtcNow;

        if (account.PreferencesId == null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            account.PreferencesId = account.Preferences.Id;
        }

        await _context.SaveChangesAsync(cancellationToken);
        InvalidateAccountCache(accountId);
        await InvalidateContactEffectiveCacheAsync(accountId, cancellationToken);

        _logger.LogInformation("Updated account preferences for account {AccountId}", accountId);
        return MapToDto(account.Preferences);
    }

    /// <inheritdoc />
    public async Task<PreferencesDto> UpdateContactPreferencesAsync(int contactId, PreferencesDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDates(dto);

        var contact = await _context.Contacts
            .Include(c => c.Preferences)
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        if (contact.Preferences == null || contact.Preferences.IsDeleted)
        {
            // Create new preferences entity and add to context first
            var newPreferences = CreateDefaultPreferences();
            newPreferences.CreatedAt = DateTime.UtcNow;
            newPreferences.IsDeleted = false;
            _context.Preferences.Add(newPreferences);
            await _context.SaveChangesAsync(cancellationToken);

            // Then link via foreign key to avoid EF tracking issues
            contact.PreferencesId = newPreferences.Id;
            contact.Preferences = newPreferences;
        }

        ApplyDto(contact.Preferences, dto);
        contact.Preferences.UpdatedAt = DateTime.UtcNow;
        contact.UseCustomPreferences = true;

        await _context.SaveChangesAsync(cancellationToken);
        InvalidateContactCache(contactId);

        _logger.LogInformation("Updated contact preferences for contact {ContactId}", contactId);
        return MapToDto(contact.Preferences);
    }

    /// <inheritdoc />
    public async Task<ContactPreferencesDto> GetContactPreferencesAsync(int contactId, bool effective = false, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        var preferences = effective
            ? await GetEffectivePreferencesAsync(contactId, cancellationToken)
            : await GetContactOverridesAsync(contactId, cancellationToken);

        return new ContactPreferencesDto
        {
            UseCustomPreferences = contact.UseCustomPreferences,
            Preferences = preferences
        };
    }

    /// <inheritdoc />
    public async Task SetContactUseCustomPreferencesAsync(int contactId, bool useCustomPreferences, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        contact.UseCustomPreferences = useCustomPreferences;

        if (!useCustomPreferences)
        {
            contact.PreferencesId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        InvalidateContactCache(contactId);

        _logger.LogInformation("Set UseCustomPreferences={UseCustomPreferences} for contact {ContactId}", useCustomPreferences, contactId);
    }

    /// <inheritdoc />
    public async Task<CRM.Core.Models.Contact> ResetContactToAccountAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var contact = await _context.Contacts
            .Include(c => c.Preferences)
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact {contactId} not found.");
        }

        var oldPreferencesId = contact.PreferencesId;
        contact.UseCustomPreferences = false;
        contact.PreferencesId = null;

        await _context.SaveChangesAsync(cancellationToken);

        if (oldPreferencesId.HasValue)
        {
            var isReferenced = await _context.Accounts.AnyAsync(a => a.PreferencesId == oldPreferencesId.Value, cancellationToken)
                               || await _context.Contacts.AnyAsync(c => c.PreferencesId == oldPreferencesId.Value && c.Id != contactId, cancellationToken);

            if (!isReferenced && contact.Preferences != null)
            {
                contact.Preferences.IsDeleted = true;
                contact.Preferences.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        InvalidateContactCache(contactId);
        _logger.LogInformation("Reset contact {ContactId} preferences to account defaults", contactId);

        return contact;
    }

    /// <inheritdoc />
    public async Task<int> BulkSetDefaultsAsync(int accountId, PreferencesDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDates(dto);

        var account = await _context.Accounts
            .Include(a => a.Preferences)
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted, cancellationToken);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account {accountId} not found.");
        }

        if (account.Preferences == null || account.Preferences.IsDeleted)
        {
            account.Preferences = CreateDefaultPreferences();
            account.Preferences.CreatedAt = DateTime.UtcNow;
            account.Preferences.IsDeleted = false;
            _context.Preferences.Add(account.Preferences);
        }

        ApplyDto(account.Preferences, dto);
        account.Preferences.UpdatedAt = DateTime.UtcNow;

        if (account.PreferencesId == null)
        {
            await _context.SaveChangesAsync(cancellationToken);
            account.PreferencesId = account.Preferences.Id;
        }

        var contacts = await _context.Contacts
            .Where(c => c.AccountId == accountId && !c.UseCustomPreferences)
            .ToListAsync(cancellationToken);

        foreach (var contact in contacts)
        {
            contact.PreferencesId = null;
            contact.UseCustomPreferences = false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        InvalidateAccountCache(accountId);
        foreach (var contact in contacts)
        {
            InvalidateContactCache(contact.Id);
        }

        _logger.LogInformation("Bulk updated account defaults for account {AccountId} (contacts affected: {Count})", accountId, contacts.Count);
        return contacts.Count;
    }

    private static Preferences CreateDefaultPreferences()
    {
        return new Preferences
        {
            OptInEmail = true,
            OptInSms = false,
            OptInPhone = true,
            OptInPostal = false,
            PreferredContactMethod = null,
            PreferredLanguage = null,
            Timezone = null,
            DoNotCallDate = null,
            DoNotEmailDate = null
        };
    }

    private static PreferencesDto CreateDefaultDto()
    {
        return new PreferencesDto
        {
            OptInEmail = true,
            OptInSms = false,
            OptInPhone = true,
            OptInPostal = false
        };
    }

    private static void ApplyDto(Preferences entity, PreferencesDto dto)
    {
        entity.OptInEmail = dto.OptInEmail;
        entity.OptInSms = dto.OptInSms;
        entity.OptInPhone = dto.OptInPhone;
        entity.OptInPostal = dto.OptInPostal;
        entity.PreferredContactMethod = dto.PreferredContactMethod;
        entity.PreferredLanguage = dto.PreferredLanguage;
        entity.Timezone = dto.Timezone;
        entity.DoNotCallDate = dto.DoNotCallDate;
        entity.DoNotEmailDate = dto.DoNotEmailDate;
    }

    private static PreferencesDto MapToDto(Preferences entity)
    {
        return new PreferencesDto
        {
            Id = entity.Id,
            OptInEmail = entity.OptInEmail,
            OptInSms = entity.OptInSms,
            OptInPhone = entity.OptInPhone,
            OptInPostal = entity.OptInPostal,
            PreferredContactMethod = entity.PreferredContactMethod,
            PreferredLanguage = entity.PreferredLanguage,
            Timezone = entity.Timezone,
            DoNotCallDate = entity.DoNotCallDate,
            DoNotEmailDate = entity.DoNotEmailDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static void ValidateDates(PreferencesDto dto)
    {
        var now = DateTime.UtcNow;
        if (dto.DoNotCallDate.HasValue && dto.DoNotCallDate.Value < now)
        {
            throw new ArgumentException("DoNotCallDate must be in the future or null.");
        }

        if (dto.DoNotEmailDate.HasValue && dto.DoNotEmailDate.Value < now)
        {
            throw new ArgumentException("DoNotEmailDate must be in the future or null.");
        }
    }

    private static string GetAccountDefaultsCacheKey(int accountId) => $"{AccountDefaultsCachePrefix}{accountId}";
    private static string GetContactOverridesCacheKey(int contactId) => $"{ContactOverridesCachePrefix}{contactId}";
    private static string GetContactEffectiveCacheKey(int contactId) => $"{ContactEffectiveCachePrefix}{contactId}";

    private void InvalidateAccountCache(int accountId)
    {
        _cache.Remove(GetAccountDefaultsCacheKey(accountId));
    }

    private void InvalidateContactCache(int contactId)
    {
        _cache.Remove(GetContactOverridesCacheKey(contactId));
        _cache.Remove(GetContactEffectiveCacheKey(contactId));
    }

    private async Task InvalidateContactEffectiveCacheAsync(int accountId, CancellationToken cancellationToken)
    {
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.AccountId == accountId && !c.UseCustomPreferences)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        foreach (var contactId in contacts)
        {
            _cache.Remove(GetContactEffectiveCacheKey(contactId));
        }
    }
}

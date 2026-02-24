// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Infrastructure.Validation;

/// <summary>
/// Validation helpers for localization settings (TODO-SYS005-003).
/// Validates timezone identifiers, ISO 4217 currency codes, and BCP-47 language tags.
/// </summary>
public static class LocalizationValidator
{
    // ─── ISO 4217 Currency Codes (30+ common currencies) ────────────────────
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "INR", "BRL",
        "MXN", "KRW", "RUB", "TRY", "ZAR", "SEK", "NOK", "DKK", "NZD", "SGD",
        "HKD", "PLN", "CZK", "HUF", "ILS", "CLP", "PHP", "AED", "SAR", "MYR",
        "THB", "IDR", "BGN", "RON", "HRK", "PKR", "NGN", "EGP", "QAR", "KWD",
        "BHD", "OMR", "JOD", "UAH", "VND", "TWD", "COP", "PEN", "ARS", "BDT"
    };

    // ─── BCP-47 Language Codes (20+ common locales) ──────────────────────────
    private static readonly HashSet<string> ValidLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en-US", "en-GB", "en-AU", "en-CA", "en-NZ",
        "fr-FR", "fr-CA", "fr-BE", "fr-CH",
        "de-DE", "de-AT", "de-CH",
        "es-ES", "es-MX", "es-AR", "es-CO",
        "pt-PT", "pt-BR",
        "it-IT",
        "nl-NL", "nl-BE",
        "pl-PL",
        "ru-RU",
        "zh-CN", "zh-TW",
        "ja-JP",
        "ko-KR",
        "ar-SA", "ar-AE",
        "hi-IN",
        "tr-TR",
        "sv-SE",
        "nb-NO",
        "da-DK",
        "fi-FI",
        "cs-CZ",
        "hu-HU",
        "ro-RO",
        "uk-UA",
        "id-ID",
        "ms-MY",
        "th-TH",
        "vi-VN"
    };

    // ─── Public validation methods ────────────────────────────────────────────

    /// <summary>
    /// Validates a timezone identifier against both Windows and IANA timezone databases.
    /// Throws <see cref="ValidationException"/> if invalid.
    /// </summary>
    public static void ValidateTimezone(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            throw new ValidationException("Timezone cannot be empty.");

        // Try Windows timezone (works on all OSes via .NET)
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return; // Valid Windows timezone
        }
        catch (TimeZoneNotFoundException) { }
        catch (InvalidTimeZoneException) { }

        // Allow well-known IANA identifiers as a fallback (cross-platform)
        if (WellKnownIanaTimezones.Contains(timezone))
            return;

        throw new ValidationException(
            $"Invalid timezone: '{timezone}'. Use a valid IANA timezone identifier (e.g., 'America/New_York', 'Europe/London') " +
            $"or a Windows timezone identifier (e.g., 'Eastern Standard Time', 'UTC').");
    }

    /// <summary>
    /// Validates an ISO 4217 currency code.
    /// Throws <see cref="ValidationException"/> if invalid.
    /// </summary>
    public static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ValidationException("Currency code cannot be empty.");

        if (!ValidCurrencies.Contains(currency))
        {
            throw new ValidationException(
                $"Invalid currency: '{currency}'. Use a valid ISO 4217 currency code (e.g., 'USD', 'EUR', 'GBP').");
        }
    }

    /// <summary>
    /// Validates a BCP-47 language tag.
    /// Throws <see cref="ValidationException"/> if invalid.
    /// </summary>
    public static void ValidateLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ValidationException("Language code cannot be empty.");

        if (!ValidLanguages.Contains(language))
        {
            throw new ValidationException(
                $"Invalid language: '{language}'. Use a valid BCP-47 locale code (e.g., 'en-US', 'fr-FR', 'de-DE').");
        }
    }

    /// <summary>Returns the list of supported currencies (ISO 4217).</summary>
    public static IReadOnlyCollection<string> GetSupportedCurrencies() =>
        ValidCurrencies.OrderBy(c => c).ToList().AsReadOnly();

    /// <summary>Returns the list of supported language codes (BCP-47).</summary>
    public static IReadOnlyCollection<string> GetSupportedLanguages() =>
        ValidLanguages.OrderBy(l => l).ToList().AsReadOnly();

    /// <summary>Returns the list of timezone identifiers from <see cref="TimeZoneInfo.GetSystemTimeZones()"/>.</summary>
    public static IReadOnlyCollection<string> GetSupportedTimezones()
    {
        var systemZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(t => t.Id)
            .ToList();

        // Merge with well-known IANA list and deduplicate
        var all = systemZones
            .Concat(WellKnownIanaTimezones)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return all.AsReadOnly();
    }

    // ─── Well-known IANA timezone identifiers ────────────────────────────────
    private static readonly HashSet<string> WellKnownIanaTimezones = new(StringComparer.OrdinalIgnoreCase)
    {
        "UTC", "GMT",
        "America/New_York", "America/Chicago", "America/Denver", "America/Los_Angeles",
        "America/Phoenix", "America/Anchorage", "Pacific/Honolulu",
        "America/Toronto", "America/Vancouver", "America/Halifax",
        "America/Sao_Paulo", "America/Argentina/Buenos_Aires", "America/Mexico_City",
        "America/Bogota", "America/Lima", "America/Santiago",
        "Europe/London", "Europe/Dublin",
        "Europe/Paris", "Europe/Berlin", "Europe/Amsterdam", "Europe/Brussels",
        "Europe/Madrid", "Europe/Rome", "Europe/Zurich",
        "Europe/Stockholm", "Europe/Oslo", "Europe/Copenhagen", "Europe/Helsinki",
        "Europe/Warsaw", "Europe/Prague", "Europe/Budapest", "Europe/Bucharest",
        "Europe/Athens", "Europe/Istanbul", "Europe/Kiev", "Europe/Moscow",
        "Africa/Cairo", "Africa/Johannesburg", "Africa/Lagos", "Africa/Nairobi",
        "Asia/Dubai", "Asia/Riyadh", "Asia/Kolkata", "Asia/Dhaka", "Asia/Karachi",
        "Asia/Mumbai", "Asia/Colombo", "Asia/Kathmandu",
        "Asia/Bangkok", "Asia/Jakarta", "Asia/Singapore", "Asia/Kuala_Lumpur",
        "Asia/Manila", "Asia/Hong_Kong", "Asia/Tokyo", "Asia/Seoul",
        "Asia/Shanghai", "Asia/Taipei", "Asia/Chongqing",
        "Asia/Almaty", "Asia/Tashkent", "Asia/Tehran",
        "Australia/Sydney", "Australia/Melbourne", "Australia/Brisbane",
        "Australia/Perth", "Australia/Adelaide", "Australia/Darwin",
        "Pacific/Auckland", "Pacific/Fiji", "Pacific/Guam"
    };
}

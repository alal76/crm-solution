// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for timezone-aware billing date calculations.
///
/// TODO-SALES006-023: Timezone support for billing date calculations
///
/// DST handling strategy:
/// - Ambiguous local times (fall-back hour): use the standard-time offset
///   (i.e. prefer the unambiguous interpretation closer to UTC).
/// - Invalid local times (spring-forward gap): advance past the gap by
///   adding one hour and converting again.
/// - Unknown timezone identifiers: fall back to UTC with a warning log.
///
/// Runtime note: On .NET 6+ on Linux/macOS the runtime reads IANA timezone
/// data natively; on Windows it maps IANA → Windows IDs via ICU.  No
/// additional NuGet package is required for common timezone identifiers.
/// </summary>
public class BillingTimezoneService : IBillingTimezoneService
{
    private readonly ILogger<BillingTimezoneService> _logger;

    public BillingTimezoneService(ILogger<BillingTimezoneService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public DateTime ConvertBillingDateToUtc(DateTime localDate, string ianaTimezone)
    {
        var tz = ResolveTimezone(ianaTimezone);

        // Treat the incoming DateTime as Unspecified so TimeZoneInfo does not
        // complain about double-conversion from an already-UTC value.
        var unspecified = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);

        // Spring-forward gap: the local time does not exist in this timezone.
        if (tz.IsInvalidTime(unspecified))
        {
            // Advance past the DST gap (typically 1 hour) and retry.
            unspecified = unspecified.AddHours(1);
            _logger.LogDebug(
                "Billing date {LocalDate} was invalid (DST spring-forward) in {Tz}; advanced to {Fixed}.",
                localDate, ianaTimezone, unspecified);
        }

        // Fall-back ambiguous hour: pick the standard-time (non-DST) offset
        // which is the more conservative (later UTC) interpretation.
        if (tz.IsAmbiguousTime(unspecified))
        {
            var ambiguousOffsets = tz.GetAmbiguousTimeOffsets(unspecified);
            // Use the smaller (standard) offset → later UTC time.
            var standardOffset = ambiguousOffsets.Min();
            var utc = unspecified - standardOffset;
            _logger.LogDebug(
                "Billing date {LocalDate} was ambiguous (DST fall-back) in {Tz}; using standard offset {Offset}.",
                localDate, ianaTimezone, standardOffset);
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        }

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
    }

    /// <inheritdoc />
    public DateTime GetNextBillingDate(DateTime utcNow, string billingTimezone, BillingPeriod cycle)
    {
        var tz = ResolveTimezone(billingTimezone);

        // Convert current UTC time to the subscription's local timezone so the
        // billing day anchor (e.g. "the 1st of the month") is evaluated in local
        // wall-clock time rather than UTC.
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), tz);

        var nextLocal = cycle switch
        {
            BillingPeriod.Weekly => localNow.AddDays(7),
            BillingPeriod.Monthly => localNow.AddMonths(1),
            BillingPeriod.Quarterly => localNow.AddMonths(3),
            BillingPeriod.Yearly => localNow.AddYears(1),
            _ => localNow.AddMonths(1)
        };

        // Anchor to midnight on the next billing day in the local timezone.
        var nextLocalMidnight = nextLocal.Date;

        // Convert that local midnight back to UTC.
        return ConvertBillingDateToUtc(nextLocalMidnight, billingTimezone);
    }

    // -------------------------------------------------------------------------
    // BACK-009: Billing Timezone — extended API
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<TimezoneInfoDto> GetSupportedTimezones()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(tz => tz.BaseUtcOffset)
            .ThenBy(tz => tz.Id)
            .Select(tz => new TimezoneInfoDto
            {
                Id = tz.Id,
                DisplayName = tz.DisplayName,
                StandardName = tz.StandardName,
                BaseUtcOffsetHours = tz.BaseUtcOffset.TotalHours
            })
            .ToList();
    }

    /// <inheritdoc />
    public DateTime ConvertToTimezone(DateTime utc, string tzId)
    {
        var tz = ResolveTimezone(tzId);
        var utcKind = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcKind, tz);
    }

    /// <inheritdoc />
    public DateTime ConvertToUtc(DateTime local, string tzId)
        => ConvertBillingDateToUtc(local, tzId);

    /// <inheritdoc />
    public string FormatBillingDate(DateTime date, string tzId, string format = "yyyy-MM-dd")
    {
        // If the date is in UTC, convert it to the target timezone first.
        var local = date.Kind == DateTimeKind.Utc
            ? ConvertToTimezone(date, tzId)
            : date;
        return local.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves a timezone by its IANA identifier.
    /// Returns UTC if the identifier is null, empty, or unrecognised.
    /// </summary>
    private TimeZoneInfo ResolveTimezone(string? ianaTimezone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimezone) ||
            string.Equals(ianaTimezone, "UTC", StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(ianaTimezone);
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogWarning(
                "Timezone '{Timezone}' was not found on this system; falling back to UTC.",
                ianaTimezone);
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException ex)
        {
            _logger.LogWarning(ex,
                "Timezone '{Timezone}' is invalid on this system; falling back to UTC.",
                ianaTimezone);
            return TimeZoneInfo.Utc;
        }
    }
}

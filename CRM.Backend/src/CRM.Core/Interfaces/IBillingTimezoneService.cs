// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>Lightweight timezone descriptor returned by GetSupportedTimezones. BACK-009.</summary>
public sealed class TimezoneInfoDto
{
    /// <summary>System/IANA timezone identifier (e.g. "America/New_York").</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Human-readable display name (e.g. "(UTC-05:00) Eastern Time (US &amp; Canada)").</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Standard (non-DST) name (e.g. "Eastern Standard Time").</summary>
    public string StandardName { get; init; } = string.Empty;

    /// <summary>Base UTC offset in total hours (e.g. -5.0 for UTC-5).</summary>
    public double BaseUtcOffsetHours { get; init; }
}

/// <summary>
/// Timezone service for billing date calculations.
/// Handles DST transitions and cross-timezone subscription billing.
///
/// TODO-SALES006-023: Timezone support for billing date calculations
/// BACK-009: Billing Timezone extended API
///
/// Note: Uses <see cref="BillingPeriod"/> (already defined in ISubscriptionService.cs)
/// as the cycle parameter to avoid duplicating the enumeration.
/// </summary>
public interface IBillingTimezoneService
{
    /// <summary>
    /// Converts a local date to UTC using the supplied IANA timezone identifier.
    /// Handles ambiguous and invalid (DST gap) times gracefully.
    /// Falls back to UTC if the timezone identifier is unknown.
    /// </summary>
    /// <param name="localDate">Local datetime to convert.</param>
    /// <param name="ianaTimezone">IANA timezone identifier (e.g. "America/New_York").</param>
    /// <returns>Equivalent UTC datetime.</returns>
    DateTime ConvertBillingDateToUtc(DateTime localDate, string ianaTimezone);

    /// <summary>
    /// Computes the next billing date (UTC) for a subscription, taking the
    /// subscription's billing timezone into account so the billing day
    /// stays anchored to the same wall-clock date across DST transitions.
    /// </summary>
    /// <param name="utcNow">Current UTC time (injection point for testability).</param>
    /// <param name="billingTimezone">IANA timezone identifier for the subscription.</param>
    /// <param name="cycle">Billing cycle (Weekly / Monthly / Quarterly / Yearly).</param>
    /// <returns>Next billing date expressed as UTC midnight of the target day.</returns>
    DateTime GetNextBillingDate(DateTime utcNow, string billingTimezone, BillingPeriod cycle);

    // ── BACK-009 additions ────────────────────────────────────────────────────

    /// <summary>
    /// Returns all timezones supported by the current runtime, sorted by base UTC offset.
    /// BACK-009: Billing Timezone
    /// </summary>
    IReadOnlyList<TimezoneInfoDto> GetSupportedTimezones();

    /// <summary>
    /// Converts a UTC datetime to local time in the given timezone.
    /// Falls back to UTC if the timezone identifier is unknown.
    /// BACK-009: Billing Timezone
    /// </summary>
    /// <param name="utc">Source UTC datetime.</param>
    /// <param name="tzId">IANA or Windows timezone identifier.</param>
    /// <returns>Local datetime in the requested timezone (Kind = Unspecified).</returns>
    DateTime ConvertToTimezone(DateTime utc, string tzId);

    /// <summary>
    /// Converts a local datetime to UTC. Alias for <see cref="ConvertBillingDateToUtc"/> with BACK-009 naming.
    /// Falls back to UTC if the timezone identifier is unknown.
    /// BACK-009: Billing Timezone
    /// </summary>
    /// <param name="local">Local datetime to convert.</param>
    /// <param name="tzId">IANA or Windows timezone identifier.</param>
    /// <returns>Equivalent UTC datetime.</returns>
    DateTime ConvertToUtc(DateTime local, string tzId);

    /// <summary>
    /// Formats a datetime in the specified timezone using the given format string.
    /// Falls back to UTC if the timezone identifier is unknown.
    /// BACK-009: Billing Timezone
    /// </summary>
    /// <param name="date">Datetime to format (UTC or local).</param>
    /// <param name="tzId">Target IANA or Windows timezone identifier.</param>
    /// <param name="format">Standard or custom .NET date/time format (default "yyyy-MM-dd").</param>
    /// <returns>Formatted date string in the target timezone.</returns>
    string FormatBillingDate(DateTime date, string tzId, string format = "yyyy-MM-dd");
}

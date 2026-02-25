// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Timezone service for billing date calculations.
/// Handles DST transitions and cross-timezone subscription billing.
///
/// TODO-SALES006-023: Timezone support for billing date calculations
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
}

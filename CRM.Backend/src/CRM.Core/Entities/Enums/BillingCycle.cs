// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// BillingCycle enum — defines how frequently a subscription is billed.
///
/// USAGE:
/// This enum is the intended replacement for the current string-based BillingCycle
/// field on the Subscription entity. The Subscription entity currently stores the
/// billing cycle as a free-text string (e.g. "Monthly", "Quarterly") for backward
/// compatibility. Future work (TODO-SALES006-014) should migrate that column to use
/// this numeric enum for type safety and API contract clarity.
///
/// API CONTRACT:
/// - Always serialize/deserialize as the integer value (not the name string).
/// - Frontend TypeScript types should map these integer values to labels.
///
/// See also: <c>CRM.Core.Interfaces.BillingPeriod</c> (existing interface enum used
/// by the Subscription entity's computed BillingPeriod property). That enum may be
/// consolidated with this one in a future refactor.
/// </summary>
public enum BillingCycle
{
    /// <summary>Billed once per month.</summary>
    Monthly = 1,

    /// <summary>Billed once every three months.</summary>
    Quarterly = 2,

    /// <summary>Billed once per year (12 months).</summary>
    Annual = 3,

    /// <summary>Billed once per week.</summary>
    Weekly = 4,

    /// <summary>Billed once per day (e.g. usage-based daily charges).</summary>
    Daily = 5,

    /// <summary>Billed twice per year (every 6 months).</summary>
    Biannual = 6,

    /// <summary>
    /// Custom billing interval — requires separate configuration to specify
    /// the exact interval (e.g. every N days, every N months).
    /// </summary>
    Custom = 99
}

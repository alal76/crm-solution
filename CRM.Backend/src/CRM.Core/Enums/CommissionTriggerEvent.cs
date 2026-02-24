// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Enums;

/// <summary>
/// Trigger events for commission calculation.
/// Defines when commissions should be calculated and applied.
/// </summary>
public enum CommissionTriggerEvent
{
    /// <summary>Commission calculated when deal is closed/won</summary>
    OnClose = 0,

    /// <summary>Commission calculated when payment is received</summary>
    OnPayment = 1,

    /// <summary>Commission calculated when invoice is generated</summary>
    OnInvoice = 2,

    /// <summary>Commission calculated when order is placed</summary>
    OnOrder = 3,

    /// <summary>Commission calculated when contract is signed</summary>
    OnSignature = 4,

    /// <summary>Commission calculated on subscription start</summary>
    OnSubscriptionStart = 5,

    /// <summary>Commission calculated monthly (recurring)</summary>
    Monthly = 6
}

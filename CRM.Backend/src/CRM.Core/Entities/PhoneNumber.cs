// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Phone number entity - Master table for all phone numbers
/// Shared between Customers, Contacts, Leads, and Accounts via EntityPhoneLinks
/// </summary>
public class PhoneNumber : BaseEntity
{
    // Phone Details
    public string? Label { get; set; }
    public string CountryCode { get; set; } = "+1";
    public string? AreaCode { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public string? FormattedNumber { get; set; }

    // Capabilities
    public bool CanSMS { get; set; } = false;
    public bool CanWhatsApp { get; set; } = false;
    public bool CanFax { get; set; } = false;

    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedDate { get; set; }

    // Preferences
    public string? BestTimeToCall { get; set; }
    public string? Notes { get; set; }

    // Audit
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<EntityPhoneLink>? EntityPhoneLinks { get; set; }

    // Computed Properties
    public string FullNumber => string.IsNullOrEmpty(Extension)
        ? $"{CountryCode} {Number}"
        : $"{CountryCode} {Number} x{Extension}";
}

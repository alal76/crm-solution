// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

/// <summary>
/// Email address entity - Master table for all email addresses
/// Shared between Customers, Contacts, Leads, and Accounts via EntityEmailLinks
/// </summary>
public class EmailAddress : BaseEntity
{
    // Email Details
    public string? Label { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedDate { get; set; }

    // Deliverability
    public int BounceCount { get; set; } = 0;
    public DateTime? LastBounceDate { get; set; }
    public bool HardBounce { get; set; } = false;

    // Engagement Tracking
    public DateTime? LastEmailSent { get; set; }
    public DateTime? LastEmailOpened { get; set; }
    public decimal? EmailEngagementScore { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation Properties
    public ICollection<EntityEmailLink>? EntityEmailLinks { get; set; }

    // Computed Properties
    public bool IsDeliverable => !HardBounce && BounceCount < 3;
}

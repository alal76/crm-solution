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

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
/// Social media platform enumeration
/// </summary>
public enum SocialMediaPlatform
{
    LinkedIn = 0,
    Twitter = 1,
    Facebook = 2,
    Instagram = 3,
    YouTube = 4,
    TikTok = 5,
    WhatsApp = 6,
    Telegram = 7,
    WeChat = 8,
    Other = 99
}

/// <summary>
/// Social media account type
/// </summary>
public enum SocialMediaAccountType
{
    Personal = 0,
    CompanyPage = 1,
    Group = 2,
    Channel = 3
}

/// <summary>
/// Social media engagement level enumeration
/// </summary>
public enum SocialEngagementLevel
{
    High = 0,
    Medium = 1,
    Low = 2,
    Inactive = 3
}

/// <summary>
/// Social media account entity - Master table for all social media accounts
/// Shared between Customers, Contacts, Leads, and Accounts via EntitySocialMediaLinks
/// </summary>
public class SocialMediaAccount : BaseEntity
{
    // Platform Details
    public SocialMediaPlatform Platform { get; set; } = SocialMediaPlatform.LinkedIn;
    public string? PlatformOther { get; set; }
    public SocialMediaAccountType AccountType { get; set; } = SocialMediaAccountType.Personal;
    public string HandleOrUsername { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public string? DisplayName { get; set; }

    // Metrics
    public int? FollowerCount { get; set; }
    public int? FollowingCount { get; set; }

    // Status
    public bool IsVerifiedAccount { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? LastActivityDate { get; set; }
    public SocialEngagementLevel? EngagementLevel { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Is this social media handle/URL validated as real
    /// </summary>
    public bool IsValidated { get; set; } = false;

    /// <summary>
    /// Date of last validation check
    /// </summary>
    public DateTime? LastValidatedAt { get; set; }

    /// <summary>
    /// Validation error message if validation failed
    /// </summary>
    public string? ValidationError { get; set; }

    // Navigation Properties
    public ICollection<EntitySocialMediaLink>? EntitySocialMediaLinks { get; set; }
    public ICollection<SocialMediaFollow>? Followers { get; set; }

    // Computed Properties
    public string PlatformName => Platform == SocialMediaPlatform.Other
        ? PlatformOther ?? "Other"
        : Platform.ToString();

    /// <summary>
    /// Number of CRM users following this social media account
    /// </summary>
    public int FollowerCountInternal => Followers?.Count(f => f.IsActive) ?? 0;
}

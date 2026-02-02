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
/// Tracks following of social media accounts for contacts/accounts
/// Allows CRM users to follow a contact's social media activity
/// </summary>
public class SocialMediaFollow : BaseEntity
{
    /// <summary>
    /// The social media account being followed
    /// </summary>
    public int SocialMediaAccountId { get; set; }

    /// <summary>
    /// The CRM user who is following this social media account
    /// </summary>
    public int FollowedByUserId { get; set; }

    /// <summary>
    /// Type of entity the social media account belongs to (Customer, Contact, Lead, Account)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity the social media account belongs to
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// When the follow was initiated
    /// </summary>
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Is the follow active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Receive notifications for activity on this account
    /// </summary>
    public bool NotifyOnActivity { get; set; } = true;

    /// <summary>
    /// Notification frequency (Immediate, Daily, Weekly)
    /// </summary>
    public NotificationFrequency NotificationFrequency { get; set; } = NotificationFrequency.Daily;

    /// <summary>
    /// Last time notifications were sent
    /// </summary>
    public DateTime? LastNotifiedAt { get; set; }

    /// <summary>
    /// Notes about why following this account
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public SocialMediaAccount? SocialMediaAccount { get; set; }
    public User? FollowedByUser { get; set; }
}

/// <summary>
/// Notification frequency for social media follows
/// </summary>
public enum NotificationFrequency
{
    Immediate = 0,
    Daily = 1,
    Weekly = 2,
    Never = 3
}

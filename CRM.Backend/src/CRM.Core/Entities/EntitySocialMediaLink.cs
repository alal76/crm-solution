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
/// Junction table linking social media accounts to entities (Customers, Contacts, Leads, Accounts)
/// Enables sharing a single social media account between multiple entities
/// </summary>
public class EntitySocialMediaLink : BaseEntity
{
    // Foreign Key to SocialMediaAccount
    public int SocialMediaAccountId { get; set; }

    // Polymorphic Link
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }

    // Link Properties
    public bool IsPrimary { get; set; } = false;
    public bool PreferredForContact { get; set; } = false;
    public bool DoNotContact { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }

    // Audit
    public int? CreatedBy { get; set; }

    // Navigation Properties
    public SocialMediaAccount? SocialMediaAccount { get; set; }

    // Computed Properties
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow)
                         && (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}

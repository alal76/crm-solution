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
/// Email type enumeration
/// </summary>
public enum EmailType
{
    General = 0,
    Billing = 1,
    Support = 2,
    Orders = 3,
    Marketing = 4,
    Technical = 5,
    Executive = 6,
    Work = 7,
    Personal = 8,
    Other = 99
}

/// <summary>
/// Junction table linking email addresses to entities (Customers, Contacts, Leads, Accounts)
/// Enables sharing a single email address between multiple entities
/// </summary>
public class EntityEmailLink : BaseEntity
{
    // Foreign Key to EmailAddress
    public int EmailId { get; set; }

    // Polymorphic Link
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }

    // Link Properties
    public EmailType EmailType { get; set; } = EmailType.General;
    public bool IsPrimary { get; set; } = false;
    public bool DoNotEmail { get; set; } = false;
    public DateTime? UnsubscribedDate { get; set; }
    public bool MarketingOptIn { get; set; } = true;
    public bool TransactionalOnly { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }

    // Audit
    public int? CreatedBy { get; set; }

    // Navigation Properties
    public EmailAddress? EmailAddress { get; set; }

    // Computed Properties
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow)
                         && (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);

    public bool CanSendMarketing => !DoNotEmail && MarketingOptIn && !TransactionalOnly && IsActive;
}

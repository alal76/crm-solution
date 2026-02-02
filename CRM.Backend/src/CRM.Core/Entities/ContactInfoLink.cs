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
/// Polymorphic link table to associate contact-info records (Address, ContactDetail, SocialAccount)
/// with owner records (Account, Subscription, Contact, Lead, Prospect)
/// This avoids creating many specific junction types while keeping ownership explicit.
/// Note: Customer was renamed to Account, Account (subscription) was renamed to Subscription.
/// </summary>
public enum ContactInfoOwnerType
{
    Account = 0,        // Formerly Customer
    Subscription = 1,   // Formerly Account (the subscription/contract entity)
    Contact = 2,
    Lead = 3,
    Prospect = 4
}

public enum ContactInfoKind
{
    Address = 0,
    ContactDetail = 1,
    SocialAccount = 2
}

public class ContactInfoLink : BaseEntity
{
    public ContactInfoOwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }

    public ContactInfoKind InfoKind { get; set; }
    public int InfoId { get; set; }

    // Explicit nullable FKs to the concrete info tables to avoid EF creating shadow FKs
    // Only one of these will be populated depending on `InfoKind`.
    public int? AddressId { get; set; }
    public Address? Address { get; set; }

    public int? ContactDetailId { get; set; }
    public ContactDetail? ContactDetail { get; set; }

    public int? SocialAccountId { get; set; }
    public SocialAccount? SocialAccount { get; set; }

    // Optional metadata for the link
    public bool IsPrimaryForOwner { get; set; } = false;
    public string? Notes { get; set; }
}

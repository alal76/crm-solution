// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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

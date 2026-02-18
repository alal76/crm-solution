// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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

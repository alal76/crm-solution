// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Entities;

public enum ContactDetailType
{
    Email = 0,
    Phone = 1,
    Fax = 2,
    Other = 99
}

/// <summary>
/// ContactDetail stores atomic contact points (email, phone, fax)
/// </summary>
public class ContactDetail : BaseEntity
{
    public ContactDetailType DetailType { get; set; } = ContactDetailType.Email;
    public string Value { get; set; } = string.Empty; // phone number or email
    public string? Label { get; set; } // Work, Home, Mobile
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}

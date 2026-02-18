// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Communication preferences shared by Accounts and Contacts.
/// </summary>
[Table("Preferences")]
public class Preferences : BaseEntity
{
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public bool OptInPostal { get; set; } = false;

    public string? PreferredContactMethod { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Timezone { get; set; }

    public DateTime? DoNotCallDate { get; set; }
    public DateTime? DoNotEmailDate { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}

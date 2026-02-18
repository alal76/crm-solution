// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for communication preferences shared by Accounts and Contacts.
/// </summary>
public class PreferencesDto
{
    public int Id { get; set; }
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public bool OptInPostal { get; set; } = false;
    public string? PreferredContactMethod { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Timezone { get; set; }
    public DateTime? DoNotCallDate { get; set; }
    public DateTime? DoNotEmailDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for contact preferences response with override flag.
/// </summary>
public class ContactPreferencesDto
{
    public bool UseCustomPreferences { get; set; }
    public PreferencesDto Preferences { get; set; } = new();
}

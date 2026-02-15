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

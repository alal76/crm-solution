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

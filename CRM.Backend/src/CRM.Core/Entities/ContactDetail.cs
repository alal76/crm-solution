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

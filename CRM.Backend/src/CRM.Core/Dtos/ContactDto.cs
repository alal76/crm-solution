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

public class SocialMediaLinkDto
{
    public int Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Handle { get; set; }
}

public class ContactDto
{
    public int Id { get; set; }
    public string ContactType { get; set; } = "Other";

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? LastModified { get; set; }
    public string? ModifiedBy { get; set; }

    // Customer relationship
    public int? AccountId { get; set; }
    public string Status { get; set; } = "Active";

    public List<SocialMediaLinkDto> SocialMediaLinks { get; set; } = new();

    // === Normalized Contact Info Collections ===
    // These replace the flat contact fields above and are the source of truth
    public List<LinkedEmailDto>? EmailAddresses { get; set; }
    public List<LinkedPhoneDto>? PhoneNumbers { get; set; }
    public List<LinkedAddressDto>? Addresses { get; set; }
    public List<LinkedSocialMediaDto>? SocialMediaAccounts { get; set; }
}

public class CreateContactRequest
{
    public string ContactType { get; set; } = "Other";

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class UpdateContactRequest
{
    public string? ContactType { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }

    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class AddSocialMediaRequest
{
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Handle { get; set; }
}

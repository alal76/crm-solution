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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Entities;

namespace CRM.Core.Models;

/// <summary>
/// Contact type enumeration
/// </summary>
public enum ContactType
{
    Employee = 0,
    Partner = 1,
    Lead = 2,
    Customer = 3,
    Vendor = 4,
    Influencer = 5,
    Investor = 6,
    Media = 7,
    Other = 8
}

/// <summary>
/// Contact status
/// </summary>
public enum ContactStatus
{
    Active = 0,
    Inactive = 1,
    Pending = 2,
    Blocked = 3,
    Archived = 4
}

/// <summary>
/// Lead status for contacts of type Lead
/// </summary>
public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Unqualified = 3,
    Converted = 4,
    Lost = 5
}

/// <summary>
/// Preferred contact method
/// </summary>
public enum PreferredContactMethod
{
    Email = 0,
    Phone = 1,
    SMS = 2,
    Mail = 3,
    Social = 4,
    Any = 5
}

/// <summary>
/// Represents a contact entity (employee, partner, lead, or other)
/// </summary>
public class Contact
{
    public int Id { get; set; }

    #region Type & Status

    public ContactType ContactType { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.Active;
    public LeadStatus? LeadStatus { get; set; } // Only for Lead type

    #endregion

    #region Personal Information

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [MaxLength(20)]
    public string? Salutation { get; set; } // Mr., Mrs., Dr., etc.

    [MaxLength(20)]
    public string? Suffix { get; set; } // Jr., Sr., III, etc.

    [MaxLength(50)]
    public string? Nickname { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    #endregion

    #region Contact Information

    [EmailAddress]
    [MaxLength(200)]
    public string? EmailPrimary { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? EmailSecondary { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? EmailWork { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? PhonePrimary { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? PhoneSecondary { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? PhoneMobile { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? PhoneWork { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? PhoneFax { get; set; }

    #endregion

    #region Preferences

    /// <summary>
    /// Optional preference overrides for this contact.
    /// </summary>
    public int? PreferencesId { get; set; }

    /// <summary>
    /// Preference overrides (used when UseCustomPreferences = true).
    /// </summary>
    public Preferences? Preferences { get; set; }

    /// <summary>
    /// Indicates whether this contact uses custom preferences instead of account defaults.
    /// </summary>
    public bool UseCustomPreferences { get; set; } = false;

    #endregion

    #region Address - Primary

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? Address2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? ZipCode { get; set; }

    #endregion

    #region Address - Mailing (if different)

    [MaxLength(500)]
    public string? MailingAddress { get; set; }

    [MaxLength(100)]
    public string? MailingCity { get; set; }

    [MaxLength(100)]
    public string? MailingState { get; set; }

    [MaxLength(100)]
    public string? MailingCountry { get; set; }

    [MaxLength(20)]
    public string? MailingZipCode { get; set; }

    #endregion

    #region Professional Information

    [MaxLength(200)]
    public string? JobTitle { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(200)]
    public string? ReportsTo { get; set; }

    public int? ReportsToContactId { get; set; }
    public int? AssistantContactId { get; set; }

    [MaxLength(200)]
    public string? AssistantName { get; set; }

    [Phone]
    [MaxLength(50)]
    public string? AssistantPhone { get; set; }

    #endregion

    #region Lead Information (for Lead type)

    [MaxLength(100)]
    public string? LeadSource { get; set; }

    [Range(0, 100)]
    public int? LeadScore { get; set; } // 0-100

    public bool? IsQualified { get; set; }
    public DateTime? QualifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToAccountId { get; set; }

    [NotMapped]
    public int? ConvertedToCustomerId
    {
        get => ConvertedToAccountId;
        set => ConvertedToAccountId = value;
    }

    [MaxLength(20)]
    public string? LeadRating { get; set; } // Hot, Warm, Cold

    #endregion

    #region Communication Preferences

    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Email;

    [MaxLength(100)]
    public string? PreferredContactTime { get; set; }

    [MaxLength(50)]
    public string? Timezone { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }

    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public bool OptInMail { get; set; } = true;
    public bool DoNotContact { get; set; } = false;
    public DateTime? LastOptInDate { get; set; }
    public DateTime? LastOptOutDate { get; set; }

    #endregion

    #region Social Media

    [Url]
    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(100)]
    public string? TwitterHandle { get; set; }

    [Url]
    [MaxLength(500)]
    public string? FacebookUrl { get; set; }

    [MaxLength(100)]
    public string? InstagramHandle { get; set; }

    [Url]
    [MaxLength(500)]
    public string? Website { get; set; }

    [Url]
    [MaxLength(500)]
    public string? BlogUrl { get; set; }

    #endregion

    #region Relationship Information

    [MaxLength(10000)]
    public string? Notes { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? Interests { get; set; } // Comma-separated

    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated

    #endregion

    #region Assignment

    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }

    [MaxLength(100)]
    public string? Territory { get; set; }

    #endregion

    #region Relationships

    public int? AccountId { get; set; } // Related Account
    public int? CampaignId { get; set; } // Source campaign

    /// <summary>
    /// Alias for legacy CustomerId usage
    /// </summary>
    [NotMapped]
    public int? CustomerId
    {
        get => AccountId;
        set => AccountId = value;
    }

    /// <summary>
    /// Navigation property to the parent Account
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Alias for legacy Customer navigation usage
    /// </summary>
    [NotMapped]
    public Account? Customer
    {
        get => Account;
        set => Account = value;
    }

    #endregion

    #region Engagement Tracking

    public DateTime? LastActivityDate { get; set; }
    public DateTime? LastContactedDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    [Range(0, int.MaxValue)]
    public int? TotalInteractions { get; set; }

    [Range(0, int.MaxValue)]
    public int? EmailsReceived { get; set; }

    [Range(0, int.MaxValue)]
    public int? EmailsOpened { get; set; }

    [Range(0, int.MaxValue)]
    public int? LinksClicked { get; set; }

    #endregion

    #region System Fields

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }

    [MaxLength(200)]
    public string? ModifiedBy { get; set; }

    public int? CreatedByUserId { get; set; }

    #endregion

    #region Custom Fields & Photo

    [MaxLength(10000)]
    public string? CustomFields { get; set; } // JSON for custom data

    [Url]
    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    #endregion

    #region Merge Tracking

    /// <summary>If this record was merged, ID of the master record it was merged into</summary>
    public int? MergedIntoId { get; set; }

    /// <summary>Reference to the merge group this record belongs to</summary>
    public int? MergeGroupId { get; set; }

    /// <summary>Quick flag indicating this is a merged duplicate (soft-deleted)</summary>
    public bool IsMergedDuplicate { get; set; } = false;

    /// <summary>When this record was merged</summary>
    public DateTime? MergedAt { get; set; }

    #endregion

    #region Lookup FKs

    public int? PreferredContactMethodLookupId { get; set; }
    public LookupItem? PreferredContactMethodLookup { get; set; }

    #endregion

    #region Navigation Properties

    public ICollection<SocialMediaLink> SocialMediaLinks { get; set; } = new List<SocialMediaLink>();
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }

    /// <summary>
    /// Many-to-many relationships with accounts (via AccountContact junction)
    /// </summary>
    public ICollection<AccountContact>? AccountContacts { get; set; }

    #endregion

    #region Aliases

    /// <summary>Alias for EmailPrimary for service compatibility</summary>
    public string? Email
    {
        get => EmailPrimary;
        set => EmailPrimary = value;
    }

    /// <summary>Alias for JobTitle for service compatibility</summary>
    public string? Title
    {
        get => JobTitle;
        set => JobTitle = value;
    }

    #endregion
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

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
    public string? EmailWork { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? PhoneWork { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    // Mailing Address
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingCountry { get; set; }
    public string? MailingZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    // Professional Hierarchy
    public int? ReportsToContactId { get; set; }
    public int? AssistantContactId { get; set; }
    public string? AssistantName { get; set; }
    public string? AssistantPhone { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? LastModified { get; set; }
    public string? ModifiedBy { get; set; }

    // Personal details
    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }

    // Additional phone numbers
    public string? PhoneMobile { get; set; }
    public string? PhoneFax { get; set; }

    // Online presence
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }

    // Social
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }
    public string? BlogUrl { get; set; }

    // Communication preferences
    public bool DoNotContact { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool OptInEmail { get; set; }
    public bool OptInSms { get; set; }
    public bool OptInPhone { get; set; }
    public bool OptInMail { get; set; }
    public DateTime? LastOptInDate { get; set; }
    public DateTime? LastOptOutDate { get; set; }

    // Lead tracking
    public string? LeadStatus { get; set; }

    // Lead Information
    public string? LeadSource { get; set; }
    public int? LeadScore { get; set; }
    public bool? IsQualified { get; set; }
    public DateTime? QualifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToAccountId { get; set; }
    public string? LeadRating { get; set; }

    // Assignment & Classification
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Territory { get; set; }
    public string? Tags { get; set; }

    // Customer relationship
    public int? AccountId { get; set; }
    public string Status { get; set; } = "Active";

    // Engagement Tracking (system-computed, response only)
    public DateTime? LastActivityDate { get; set; }
    public DateTime? LastContactedDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public int? TotalInteractions { get; set; }
    public int? EmailsReceived { get; set; }
    public int? EmailsOpened { get; set; }
    public int? LinksClicked { get; set; }

    // Other
    public string? CustomFields { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }

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
    [StringLength(50)]
    public string ContactType { get; set; } = "Other";

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format for primary email")]
    [StringLength(200)]
    public string? EmailPrimary { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format for secondary email")]
    [StringLength(200)]
    public string? EmailSecondary { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format for work email")]
    [StringLength(200)]
    public string? EmailWork { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(50)]
    public string? PhonePrimary { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(50)]
    public string? PhoneSecondary { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(50)]
    public string? PhoneWork { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    // Mailing Address
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingCountry { get; set; }
    public string? MailingZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    // Professional Hierarchy
    public int? ReportsToContactId { get; set; }
    public int? AssistantContactId { get; set; }
    public string? AssistantName { get; set; }
    public string? AssistantPhone { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone format for mobile")]
    [StringLength(50)]
    public string? PhoneMobile { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone format for fax")]
    [StringLength(50)]
    public string? PhoneFax { get; set; }
    
    [Url(ErrorMessage = "Invalid URL format for website")]
    [StringLength(500)]
    public string? Website { get; set; }
    
    [Url(ErrorMessage = "Invalid URL format for LinkedIn")]
    [StringLength(500)]
    public string? LinkedInUrl { get; set; }
    
    [StringLength(100)]
    [RegularExpression(@"^@?[a-zA-Z0-9_]{1,15}$", ErrorMessage = "Invalid Twitter handle format")]
    public string? TwitterHandle { get; set; }

    // Social
    [Url(ErrorMessage = "Invalid URL format for Facebook")]
    [StringLength(500)]
    public string? FacebookUrl { get; set; }
    
    [StringLength(100)]
    [RegularExpression(@"^@?[a-zA-Z0-9._]{1,30}$", ErrorMessage = "Invalid Instagram handle format")]
    public string? InstagramHandle { get; set; }
    
    [Url(ErrorMessage = "Invalid URL format for blog")]
    [StringLength(500)]
    public string? BlogUrl { get; set; }

    public bool DoNotContact { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; }
    public bool OptInPhone { get; set; } = true;
    public bool OptInMail { get; set; } = true;
    public DateTime? LastOptInDate { get; set; }
    public DateTime? LastOptOutDate { get; set; }

    // Lead Information
    public string? LeadSource { get; set; }
    public int? LeadScore { get; set; }
    public bool? IsQualified { get; set; }
    public DateTime? QualifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToAccountId { get; set; }
    public string? LeadRating { get; set; }

    // Assignment & Classification
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Territory { get; set; }
    public string? Tags { get; set; }

    // Other
    public string? CustomFields { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }
}

public class UpdateContactRequest
{
    public string? ContactType { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }

    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? EmailWork { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? PhoneWork { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    // Mailing Address
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingCountry { get; set; }
    public string? MailingZipCode { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; }

    // Professional Hierarchy
    public int? ReportsToContactId { get; set; }
    public int? AssistantContactId { get; set; }
    public string? AssistantName { get; set; }
    public string? AssistantPhone { get; set; }

    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }
    public string? Gender { get; set; }
    public string? PhoneMobile { get; set; }
    public string? PhoneFax { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }

    // Social
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }
    public string? BlogUrl { get; set; }

    public bool? DoNotContact { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool? OptInEmail { get; set; }
    public bool? OptInSms { get; set; }
    public bool? OptInPhone { get; set; }
    public bool? OptInMail { get; set; }
    public DateTime? LastOptInDate { get; set; }
    public DateTime? LastOptOutDate { get; set; }

    // Lead Information
    public string? LeadSource { get; set; }
    public int? LeadScore { get; set; }
    public bool? IsQualified { get; set; }
    public DateTime? QualifiedDate { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public int? ConvertedToAccountId { get; set; }
    public string? LeadRating { get; set; }

    // Assignment & Classification
    public int? OwnerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Territory { get; set; }
    public string? Tags { get; set; }

    // Other
    public string? CustomFields { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }
}

public class AddSocialMediaRequest
{
    [Required(ErrorMessage = "Platform is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Platform must be between 2 and 50 characters")]
    public string Platform { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
    public string Url { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Handle { get; set; }
}

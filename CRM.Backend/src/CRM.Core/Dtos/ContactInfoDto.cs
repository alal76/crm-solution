using CRM.Core.Entities;

namespace CRM.Core.Dtos;

#region Address DTOs

/// <summary>
/// DTO for address responses
/// </summary>
public class AddressDto
{
    public int Id { get; set; }
    public string Label { get; set; } = "Primary";
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? County { get; set; }
    public string? CountryCode { get; set; }
    public string Country { get; set; } = "United States";
    
    // New fields for ZipCode/Locality linking
    public int? ZipCodeId { get; set; }
    public int? LocalityId { get; set; }
    public string? Locality { get; set; }
    public string? AddressXml { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? GeocodeAccuracy { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? VerificationSource { get; set; }
    public bool? IsResidential { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? AccessHours { get; set; }
    public string? SiteContactName { get; set; }
    public string? SiteContactPhone { get; set; }
    public string? Notes { get; set; }
    public string? FormattedAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for linked address (includes link metadata)
/// </summary>
public class LinkedAddressDto : AddressDto
{
    public int LinkId { get; set; }
    public string AddressType { get; set; } = "Primary";
    public bool IsPrimary { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public string? LinkNotes { get; set; }
}

/// <summary>
/// DTO for creating an address
/// </summary>
public class CreateAddressDto
{
    public string? Label { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? County { get; set; }
    public string? CountryCode { get; set; }
    public string Country { get; set; } = "United States";
    
    // New fields for ZipCode/Locality linking
    public int? ZipCodeId { get; set; }
    public int? LocalityId { get; set; }
    public string? Locality { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool? IsResidential { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? AccessHours { get; set; }
    public string? SiteContactName { get; set; }
    public string? SiteContactPhone { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for linking an address to an entity
/// </summary>
public class LinkAddressDto
{
    public int? AddressId { get; set; } // Existing address to link, or null to create new
    public CreateAddressDto? NewAddress { get; set; } // New address to create
    public string EntityType { get; set; } = "Customer"; // Customer, Contact, Lead, Account
    public int EntityId { get; set; }
    public string AddressType { get; set; } = "Primary"; // Primary, Billing, Shipping, etc.
    public bool IsPrimary { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Phone DTOs

/// <summary>
/// DTO for phone number responses
/// </summary>
public class PhoneNumberDto
{
    public int Id { get; set; }
    public string? Label { get; set; }
    public string CountryCode { get; set; } = "+1";
    public string? AreaCode { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public string? FormattedNumber { get; set; }
    public bool CanSMS { get; set; }
    public bool CanWhatsApp { get; set; }
    public bool CanFax { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? BestTimeToCall { get; set; }
    public string? Notes { get; set; }
    public string? FullNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for linked phone number (includes link metadata)
/// </summary>
public class LinkedPhoneDto : PhoneNumberDto
{
    public int LinkId { get; set; }
    public string PhoneType { get; set; } = "Office";
    public bool IsPrimary { get; set; }
    public bool DoNotCall { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public string? LinkNotes { get; set; }
}

/// <summary>
/// DTO for creating a phone number
/// </summary>
public class CreatePhoneNumberDto
{
    public string? Label { get; set; }
    public string CountryCode { get; set; } = "+1";
    public string? AreaCode { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public bool CanSMS { get; set; } = false;
    public bool CanWhatsApp { get; set; } = false;
    public bool CanFax { get; set; } = false;
    public string? BestTimeToCall { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for linking a phone number to an entity
/// </summary>
public class LinkPhoneDto
{
    public int? PhoneId { get; set; } // Existing phone to link, or null to create new
    public CreatePhoneNumberDto? NewPhone { get; set; } // New phone to create
    public string EntityType { get; set; } = "Customer";
    public int EntityId { get; set; }
    public string PhoneType { get; set; } = "Office"; // Office, Mobile, Fax, Home, etc.
    public bool IsPrimary { get; set; } = false;
    public bool DoNotCall { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Email DTOs

/// <summary>
/// DTO for email address responses
/// </summary>
public class EmailAddressDto
{
    public int Id { get; set; }
    public string? Label { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public int BounceCount { get; set; }
    public DateTime? LastBounceDate { get; set; }
    public bool HardBounce { get; set; }
    public DateTime? LastEmailSent { get; set; }
    public DateTime? LastEmailOpened { get; set; }
    public decimal? EmailEngagementScore { get; set; }
    public bool IsDeliverable { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for linked email address (includes link metadata)
/// </summary>
public class LinkedEmailDto : EmailAddressDto
{
    public int LinkId { get; set; }
    public string EmailType { get; set; } = "General";
    public bool IsPrimary { get; set; }
    public bool DoNotEmail { get; set; }
    public DateTime? UnsubscribedDate { get; set; }
    public bool MarketingOptIn { get; set; }
    public bool TransactionalOnly { get; set; }
    public bool CanSendMarketing { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public string? LinkNotes { get; set; }
}

/// <summary>
/// DTO for creating an email address
/// </summary>
public class CreateEmailAddressDto
{
    public string? Label { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for linking an email address to an entity
/// </summary>
public class LinkEmailDto
{
    public int? EmailId { get; set; } // Existing email to link, or null to create new
    public CreateEmailAddressDto? NewEmail { get; set; } // New email to create
    public string EntityType { get; set; } = "Customer";
    public int EntityId { get; set; }
    public string EmailType { get; set; } = "General"; // General, Billing, Support, etc.
    public bool IsPrimary { get; set; } = false;
    public bool DoNotEmail { get; set; } = false;
    public bool MarketingOptIn { get; set; } = true;
    public bool TransactionalOnly { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Social Media DTOs

/// <summary>
/// DTO for social media account responses
/// </summary>
public class SocialMediaAccountDto
{
    public int Id { get; set; }
    public string Platform { get; set; } = "LinkedIn";
    public string? PlatformOther { get; set; }
    public string AccountType { get; set; } = "Personal";
    public string HandleOrUsername { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public string? DisplayName { get; set; }
    public int? FollowerCount { get; set; }
    public int? FollowingCount { get; set; }
    public bool IsVerifiedAccount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public string? EngagementLevel { get; set; }
    public string? PlatformName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for linked social media account (includes link metadata)
/// </summary>
public class LinkedSocialMediaDto : SocialMediaAccountDto
{
    public int LinkId { get; set; }
    public bool IsPrimary { get; set; }
    public bool PreferredForContact { get; set; }
    public string? LinkNotes { get; set; }
}

/// <summary>
/// DTO for creating a social media account
/// </summary>
public class CreateSocialMediaAccountDto
{
    public string Platform { get; set; } = "LinkedIn";
    public string? PlatformOther { get; set; }
    public string AccountType { get; set; } = "Personal";
    public string HandleOrUsername { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public string? DisplayName { get; set; }
    public int? FollowerCount { get; set; }
    public int? FollowingCount { get; set; }
    public bool IsVerifiedAccount { get; set; } = false;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for linking a social media account to an entity
/// </summary>
public class LinkSocialMediaDto
{
    public int? SocialMediaAccountId { get; set; } // Existing account to link, or null to create new
    public CreateSocialMediaAccountDto? NewSocialMedia { get; set; } // New account to create
    public string EntityType { get; set; } = "Customer";
    public int EntityId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool PreferredForContact { get; set; } = false;
    public string? Notes { get; set; }
}

#endregion

#region Aggregate DTOs

/// <summary>
/// Complete contact info for an entity (all addresses, phones, emails, social media)
/// </summary>
public class EntityContactInfoDto
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    public List<LinkedAddressDto> Addresses { get; set; } = new();
    public List<LinkedPhoneDto> PhoneNumbers { get; set; } = new();
    public List<LinkedEmailDto> EmailAddresses { get; set; } = new();
    public List<LinkedSocialMediaDto> SocialMediaAccounts { get; set; } = new();
    
    // Primary contact info (convenience properties)
    public LinkedAddressDto? PrimaryAddress => Addresses.FirstOrDefault(a => a.IsPrimary) ?? Addresses.FirstOrDefault();
    public LinkedPhoneDto? PrimaryPhone => PhoneNumbers.FirstOrDefault(p => p.IsPrimary) ?? PhoneNumbers.FirstOrDefault();
    public LinkedEmailDto? PrimaryEmail => EmailAddresses.FirstOrDefault(e => e.IsPrimary) ?? EmailAddresses.FirstOrDefault();
    public LinkedSocialMediaDto? PrimarySocialMedia => SocialMediaAccounts.FirstOrDefault(s => s.IsPrimary) ?? SocialMediaAccounts.FirstOrDefault();
}

/// <summary>
/// Request to share contact info between entities
/// </summary>
public class ShareContactInfoDto
{
    public string SourceEntityType { get; set; } = "Customer";
    public int SourceEntityId { get; set; }
    public string TargetEntityType { get; set; } = "Contact";
    public int TargetEntityId { get; set; }
    
    // Which contact info to share (by ID)
    public List<int>? AddressIds { get; set; }
    public List<int>? PhoneIds { get; set; }
    public List<int>? EmailIds { get; set; }
    public List<int>? SocialMediaIds { get; set; }
    
    // Default types and settings for the links
    public string DefaultAddressType { get; set; } = "Primary";
    public string DefaultPhoneType { get; set; } = "Office";
    public string DefaultEmailType { get; set; } = "General";
    public bool SetAsPrimary { get; set; } = false;
}

#endregion

#region Locality DTOs

/// <summary>
/// DTO for locality (neighborhood/subdivision) responses
/// </summary>
public class LocalityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AlternateName { get; set; }
    public string LocalityType { get; set; } = "Neighborhood";
    public int? ZipCodeId { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public string CountryCode { get; set; } = "US";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsUserCreated { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new locality
/// </summary>
public class CreateLocalityDto
{
    public string Name { get; set; } = string.Empty;
    public string? AlternateName { get; set; }
    public string LocalityType { get; set; } = "Neighborhood";
    public int? ZipCodeId { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public string CountryCode { get; set; } = "US";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

#endregion

#region Social Media Follow DTOs

/// <summary>
/// DTO for social media follow responses
/// </summary>
public class SocialMediaFollowDto
{
    public int Id { get; set; }
    public int SocialMediaAccountId { get; set; }
    public int FollowedByUserId { get; set; }
    public string? FollowedByUserName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    public DateTime FollowedAt { get; set; }
    public bool IsActive { get; set; }
    public bool NotifyOnActivity { get; set; }
    public string NotificationFrequency { get; set; } = "Daily";
    public DateTime? LastNotifiedAt { get; set; }
    public string? Notes { get; set; }
    
    // Social media account details
    public string? Platform { get; set; }
    public string? HandleOrUsername { get; set; }
    public string? ProfileUrl { get; set; }
    public string? DisplayName { get; set; }
}

/// <summary>
/// DTO for following a social media account
/// </summary>
public class FollowSocialMediaDto
{
    public int SocialMediaAccountId { get; set; }
    public bool NotifyOnActivity { get; set; } = true;
    public string NotificationFrequency { get; set; } = "Daily"; // Immediate, Daily, Weekly, Never
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating follow settings
/// </summary>
public class UpdateFollowSettingsDto
{
    public bool? NotifyOnActivity { get; set; }
    public string? NotificationFrequency { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Validation DTOs

/// <summary>
/// Request to validate contact info
/// </summary>
public class ValidateContactInfoRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SocialMediaHandle { get; set; }
    public string? SocialMediaPlatform { get; set; }
    public string? CountryCode { get; set; } = "US";
    public bool CheckMxRecords { get; set; } = false;
}

/// <summary>
/// Response from contact info validation
/// </summary>
public class ValidateContactInfoResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuggestedCorrection { get; set; }
    public string? FormattedValue { get; set; }
    public string? NormalizedValue { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

#endregion


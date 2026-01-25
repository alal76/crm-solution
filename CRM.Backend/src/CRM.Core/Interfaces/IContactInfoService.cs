using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing consolidated contact information
/// (addresses, phone numbers, email addresses, social media accounts)
/// shared between Customers, Contacts, Leads, and Accounts
/// </summary>
public interface IContactInfoService
{
    #region Address Operations
    
    /// <summary>
    /// Get all addresses linked to an entity
    /// </summary>
    Task<List<LinkedAddressDto>> GetAddressesAsync(EntityType entityType, int entityId);
    
    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    Task<AddressDto?> GetAddressByIdAsync(int addressId);
    
    /// <summary>
    /// Create a new address and optionally link it to an entity
    /// </summary>
    Task<AddressDto> CreateAddressAsync(CreateAddressDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Link an existing address to an entity
    /// </summary>
    Task<LinkedAddressDto> LinkAddressAsync(LinkAddressDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Update an address
    /// </summary>
    Task<AddressDto> UpdateAddressAsync(int addressId, CreateAddressDto dto, int? updatedByUserId = null);
    
    /// <summary>
    /// Unlink an address from an entity (does not delete the address)
    /// </summary>
    Task UnlinkAddressAsync(int linkId);
    
    /// <summary>
    /// Delete an address (soft delete, also unlinks from all entities)
    /// </summary>
    Task DeleteAddressAsync(int addressId);
    
    /// <summary>
    /// Set an address as primary for an entity
    /// </summary>
    Task SetPrimaryAddressAsync(EntityType entityType, int entityId, int addressId, AddressType addressType);
    
    #endregion

    #region Phone Number Operations
    
    /// <summary>
    /// Get all phone numbers linked to an entity
    /// </summary>
    Task<List<LinkedPhoneDto>> GetPhoneNumbersAsync(EntityType entityType, int entityId);
    
    /// <summary>
    /// Get a specific phone number by ID
    /// </summary>
    Task<PhoneNumberDto?> GetPhoneNumberByIdAsync(int phoneId);
    
    /// <summary>
    /// Create a new phone number and optionally link it to an entity
    /// </summary>
    Task<PhoneNumberDto> CreatePhoneNumberAsync(CreatePhoneNumberDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Link an existing phone number to an entity
    /// </summary>
    Task<LinkedPhoneDto> LinkPhoneNumberAsync(LinkPhoneDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Update a phone number
    /// </summary>
    Task<PhoneNumberDto> UpdatePhoneNumberAsync(int phoneId, CreatePhoneNumberDto dto, int? updatedByUserId = null);
    
    /// <summary>
    /// Unlink a phone number from an entity
    /// </summary>
    Task UnlinkPhoneNumberAsync(int linkId);
    
    /// <summary>
    /// Delete a phone number
    /// </summary>
    Task DeletePhoneNumberAsync(int phoneId);
    
    /// <summary>
    /// Set a phone number as primary for an entity
    /// </summary>
    Task SetPrimaryPhoneAsync(EntityType entityType, int entityId, int phoneId, PhoneType phoneType);
    
    #endregion

    #region Email Address Operations
    
    /// <summary>
    /// Get all email addresses linked to an entity
    /// </summary>
    Task<List<LinkedEmailDto>> GetEmailAddressesAsync(EntityType entityType, int entityId);
    
    /// <summary>
    /// Get a specific email address by ID
    /// </summary>
    Task<EmailAddressDto?> GetEmailAddressByIdAsync(int emailId);
    
    /// <summary>
    /// Find an email address by email string
    /// </summary>
    Task<EmailAddressDto?> FindEmailByAddressAsync(string email);
    
    /// <summary>
    /// Create a new email address and optionally link it to an entity
    /// </summary>
    Task<EmailAddressDto> CreateEmailAddressAsync(CreateEmailAddressDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Link an existing email address to an entity
    /// </summary>
    Task<LinkedEmailDto> LinkEmailAddressAsync(LinkEmailDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Update an email address
    /// </summary>
    Task<EmailAddressDto> UpdateEmailAddressAsync(int emailId, CreateEmailAddressDto dto, int? updatedByUserId = null);
    
    /// <summary>
    /// Unlink an email address from an entity
    /// </summary>
    Task UnlinkEmailAddressAsync(int linkId);
    
    /// <summary>
    /// Delete an email address
    /// </summary>
    Task DeleteEmailAddressAsync(int emailId);
    
    /// <summary>
    /// Set an email address as primary for an entity
    /// </summary>
    Task SetPrimaryEmailAsync(EntityType entityType, int entityId, int emailId, EmailType emailType);
    
    /// <summary>
    /// Update email marketing preferences
    /// </summary>
    Task UpdateEmailPreferencesAsync(int linkId, bool doNotEmail, bool marketingOptIn, bool transactionalOnly);
    
    #endregion

    #region Social Media Account Operations
    
    /// <summary>
    /// Get all social media accounts linked to an entity
    /// </summary>
    Task<List<LinkedSocialMediaDto>> GetSocialMediaAccountsAsync(EntityType entityType, int entityId);
    
    /// <summary>
    /// Get a specific social media account by ID
    /// </summary>
    Task<SocialMediaAccountDto?> GetSocialMediaAccountByIdAsync(int socialMediaId);
    
    /// <summary>
    /// Create a new social media account and optionally link it to an entity
    /// </summary>
    Task<SocialMediaAccountDto> CreateSocialMediaAccountAsync(CreateSocialMediaAccountDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Link an existing social media account to an entity
    /// </summary>
    Task<LinkedSocialMediaDto> LinkSocialMediaAccountAsync(LinkSocialMediaDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Update a social media account
    /// </summary>
    Task<SocialMediaAccountDto> UpdateSocialMediaAccountAsync(int socialMediaId, CreateSocialMediaAccountDto dto, int? updatedByUserId = null);
    
    /// <summary>
    /// Unlink a social media account from an entity
    /// </summary>
    Task UnlinkSocialMediaAccountAsync(int linkId);
    
    /// <summary>
    /// Delete a social media account
    /// </summary>
    Task DeleteSocialMediaAccountAsync(int socialMediaId);
    
    /// <summary>
    /// Set a social media account as primary for an entity
    /// </summary>
    Task SetPrimarySocialMediaAsync(EntityType entityType, int entityId, int socialMediaId);
    
    #endregion

    #region Aggregate Operations
    
    /// <summary>
    /// Get all contact info for an entity
    /// </summary>
    Task<EntityContactInfoDto> GetEntityContactInfoAsync(EntityType entityType, int entityId);
    
    /// <summary>
    /// Share contact info between entities (e.g., share customer address with contact)
    /// </summary>
    Task ShareContactInfoAsync(ShareContactInfoDto dto, int? createdByUserId = null);
    
    /// <summary>
    /// Get all entities that share a specific address
    /// </summary>
    Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingAddressAsync(int addressId);
    
    /// <summary>
    /// Get all entities that share a specific phone number
    /// </summary>
    Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingPhoneAsync(int phoneId);
    
    /// <summary>
    /// Get all entities that share a specific email address
    /// </summary>
    Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingEmailAsync(int emailId);
    
    #endregion

    #region Social Media Follow Operations

    /// <summary>
    /// Follow a social media account to track activity
    /// </summary>
    Task<SocialMediaFollowDto> FollowSocialMediaAccountAsync(int socialMediaAccountId, int userId, bool notifyOnActivity = false, string notificationFrequency = "Never", string? notes = null);

    /// <summary>
    /// Unfollow a social media account
    /// </summary>
    Task UnfollowSocialMediaAccountAsync(int followId, int userId);

    /// <summary>
    /// Get all social media accounts a user is following
    /// </summary>
    Task<List<SocialMediaFollowDto>> GetUserFollowsAsync(int userId);

    /// <summary>
    /// Update follow settings for a social media account
    /// </summary>
    Task<SocialMediaFollowDto> UpdateFollowSettingsAsync(int followId, int userId, bool notifyOnActivity, string notificationFrequency, string? notes);

    /// <summary>
    /// Get all users following a social media account
    /// </summary>
    Task<List<SocialMediaFollowDto>> GetAccountFollowersAsync(int socialMediaAccountId);

    #endregion
}

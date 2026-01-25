using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for normalization operations (tags, custom fields, contact info)
/// </summary>
public interface INormalizationService
{
    /// <summary>
    /// Get tags for an entity
    /// </summary>
    Task<string?> GetTagsAsync(string entityType, int entityId);

    /// <summary>
    /// Get custom fields for an entity
    /// </summary>
    Task<string?> GetCustomFieldsAsync(string entityType, int entityId);

    /// <summary>
    /// Get primary contact detail value
    /// </summary>
    Task<string?> GetPrimaryContactDetailValueAsync(ContactInfoOwnerType ownerType, int ownerId, ContactDetailType detailType);

    /// <summary>
    /// Get primary email for an entity
    /// </summary>
    Task<string?> GetPrimaryEmailAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary phone for an entity
    /// </summary>
    Task<string?> GetPrimaryPhoneAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary fax for an entity
    /// </summary>
    Task<string?> GetPrimaryFaxAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary address for an entity
    /// </summary>
    Task<Address?> GetPrimaryAddressAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary social account for an entity
    /// </summary>
    Task<string?> GetPrimarySocialAccountAsync(ContactInfoOwnerType ownerType, int ownerId, SocialNetwork network);
}

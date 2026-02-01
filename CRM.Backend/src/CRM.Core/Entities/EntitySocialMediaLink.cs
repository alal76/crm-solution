namespace CRM.Core.Entities;

/// <summary>
/// Junction table linking social media accounts to entities (Customers, Contacts, Leads, Accounts)
/// Enables sharing a single social media account between multiple entities
/// </summary>
public class EntitySocialMediaLink : BaseEntity
{
    // Foreign Key to SocialMediaAccount
    public int SocialMediaAccountId { get; set; }
    
    // Polymorphic Link
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    
    // Link Properties
    public bool IsPrimary { get; set; } = false;
    public bool PreferredForContact { get; set; } = false;
    public bool DoNotContact { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    
    // Navigation Properties
    public SocialMediaAccount? SocialMediaAccount { get; set; }
    
    // Computed Properties
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) 
                         && (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}

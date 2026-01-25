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
    public string? Notes { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    
    // Navigation Properties
    public SocialMediaAccount? SocialMediaAccount { get; set; }
}

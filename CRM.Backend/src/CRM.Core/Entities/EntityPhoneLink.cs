namespace CRM.Core.Entities;

/// <summary>
/// Phone type enumeration
/// </summary>
public enum PhoneType
{
    Office = 0,
    Mobile = 1,
    Fax = 2,
    Home = 3,
    Direct = 4,
    TollFree = 5,
    Emergency = 6,
    Other = 99
}

/// <summary>
/// Junction table linking phone numbers to entities (Customers, Contacts, Leads, Accounts)
/// Enables sharing a single phone number between multiple entities
/// </summary>
public class EntityPhoneLink : BaseEntity
{
    // Foreign Key to PhoneNumber
    public int PhoneId { get; set; }
    
    // Polymorphic Link
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    
    // Link Properties
    public PhoneType PhoneType { get; set; } = PhoneType.Office;
    public bool IsPrimary { get; set; } = false;
    public bool DoNotCall { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    
    // Navigation Properties
    public PhoneNumber? PhoneNumber { get; set; }
    
    // Computed Properties
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) 
                         && (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}

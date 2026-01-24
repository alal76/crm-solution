namespace CRM.Core.Entities;

/// <summary>
/// Entity type for polymorphic linking
/// </summary>
public enum EntityType
{
    Customer = 0,
    Contact = 1,
    Lead = 2,
    Account = 3,
    Prospect = 4
}

/// <summary>
/// Address type enumeration
/// </summary>
public enum AddressType
{
    Primary = 0,
    Billing = 1,
    Shipping = 2,
    Physical = 3,
    Mailing = 4,
    Work = 5,
    Home = 6,
    Branch = 7,
    Warehouse = 8,
    Other = 99
}

/// <summary>
/// Junction table linking addresses to entities (Customers, Contacts, Leads, Accounts)
/// Enables sharing a single address between multiple entities
/// </summary>
public class EntityAddressLink : BaseEntity
{
    // Foreign Key to Address
    public int AddressId { get; set; }
    
    // Polymorphic Link
    public EntityType EntityType { get; set; }
    public int EntityId { get; set; }
    
    // Link Properties
    public AddressType AddressType { get; set; } = AddressType.Primary;
    public bool IsPrimary { get; set; } = false;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    
    // Navigation Properties
    public Address? Address { get; set; }
    
    // Computed Properties
    public bool IsActive => (!ValidFrom.HasValue || ValidFrom <= DateTime.UtcNow) 
                         && (!ValidTo.HasValue || ValidTo >= DateTime.UtcNow);
}

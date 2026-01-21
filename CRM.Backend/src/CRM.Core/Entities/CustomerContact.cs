namespace CRM.Core.Entities;

/// <summary>
/// Role of the contact within the customer organization
/// </summary>
public enum CustomerContactRole
{
    Primary = 0,
    Secondary = 1,
    Billing = 2,
    Technical = 3,
    DecisionMaker = 4,
    Influencer = 5,
    EndUser = 6,
    Executive = 7,
    Procurement = 8,
    Other = 9
}

/// <summary>
/// Junction entity for linking Contacts to Organization Customers
/// Represents the many-to-many relationship between Customer and Contact
/// </summary>
public class CustomerContact : BaseEntity
{
    /// <summary>
    /// The customer (organization) this contact is linked to
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// The contact ID from the Contacts module
    /// </summary>
    public int ContactId { get; set; }
    
    /// <summary>
    /// Role of the contact within the customer organization
    /// </summary>
    public CustomerContactRole Role { get; set; } = CustomerContactRole.Primary;
    
    /// <summary>
    /// Whether this is the primary contact for the customer
    /// </summary>
    public bool IsPrimaryContact { get; set; } = false;
    
    /// <summary>
    /// Whether this contact can make purchasing decisions
    /// </summary>
    public bool IsDecisionMaker { get; set; } = false;
    
    /// <summary>
    /// Whether this contact should receive billing communications
    /// </summary>
    public bool ReceivesBillingNotifications { get; set; } = false;
    
    /// <summary>
    /// Whether this contact should receive marketing communications
    /// </summary>
    public bool ReceivesMarketingEmails { get; set; } = true;
    
    /// <summary>
    /// Whether this contact should receive technical/support communications
    /// </summary>
    public bool ReceivesTechnicalUpdates { get; set; } = false;
    
    /// <summary>
    /// Job title/position at the customer organization (may differ from contact's own job title)
    /// </summary>
    public string? PositionAtCustomer { get; set; }
    
    /// <summary>
    /// Department within the customer organization
    /// </summary>
    public string? DepartmentAtCustomer { get; set; }
    
    /// <summary>
    /// Start date of the relationship
    /// </summary>
    public DateTime? RelationshipStartDate { get; set; }
    
    /// <summary>
    /// End date of the relationship (if no longer active)
    /// </summary>
    public DateTime? RelationshipEndDate { get; set; }
    
    /// <summary>
    /// Additional notes about this contact's relationship with the customer
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation property
    public Customer? Customer { get; set; }
}

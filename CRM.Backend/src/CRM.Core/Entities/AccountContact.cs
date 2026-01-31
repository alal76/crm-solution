using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Role of the contact within the account organization
/// </summary>
public enum AccountContactRole
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
/// Represents the many-to-many relationship between Account and Contact
/// </summary>
public class AccountContact : BaseEntity
{
    /// <summary>
    /// The customer (organization) this contact is linked to
    /// </summary>
    public int AccountId { get; set; }
    
    /// <summary>
    /// The contact ID from the Contacts module
    /// </summary>
    public int ContactId { get; set; }
    
    /// <summary>
    /// Role of the contact within the account organization
    /// </summary>
    public AccountContactRole Role { get; set; } = AccountContactRole.Primary;
    
    /// <summary>
    /// Whether this is the primary contact for the account
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
    /// Job title/position at the account organization (may differ from contact's own job title)
    /// </summary>
    public string? PositionAtAccount { get; set; }
    
    /// <summary>
    /// Department within the account organization
    /// </summary>
    public string? DepartmentAtAccount { get; set; }
    
    /// <summary>
    /// Start date of the relationship
    /// </summary>
    public DateTime? RelationshipStartDate { get; set; }
    
    /// <summary>
    /// End date of the relationship (if no longer active)
    /// </summary>
    public DateTime? RelationshipEndDate { get; set; }
    
    /// <summary>
    /// Additional notes about this contact's relationship with the account
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    public Account? Account { get; set; }
    public Contact? Contact { get; set; }
}

#region Backward Compatibility Aliases

/// <summary>
/// Backward compatibility alias for CustomerContact - use AccountContact instead
/// </summary>
[Obsolete("Use AccountContact instead. CustomerContact is deprecated.")]
public class CustomerContact : AccountContact { }

/// <summary>Backward compatibility alias</summary>
[Obsolete("Use AccountContactRole instead")]
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

#endregion

namespace CRM.Core.Entities;

/// <summary>
/// Polymorphic link table to associate contact-info records (Address, ContactDetail, SocialAccount)
/// with owner records (Customer, Account, Contact, Lead, Prospect)
/// This avoids creating many specific junction types while keeping ownership explicit.
/// </summary>
public enum ContactInfoOwnerType
{
    Customer = 0,
    Account = 1,
    Contact = 2,
    Lead = 3,
    Prospect = 4
}

public enum ContactInfoKind
{
    Address = 0,
    ContactDetail = 1,
    SocialAccount = 2
}

public class ContactInfoLink : BaseEntity
{
    public ContactInfoOwnerType OwnerType { get; set; }
    public int OwnerId { get; set; }

    public ContactInfoKind InfoKind { get; set; }
    public int InfoId { get; set; }

    // Optional metadata for the link
    public bool IsPrimaryForOwner { get; set; } = false;
    public string? Notes { get; set; }
}

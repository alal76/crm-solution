namespace CRM.Core.Entities;

public enum ContactDetailType
{
    Email = 0,
    Phone = 1,
    Fax = 2,
    Other = 99
}

/// <summary>
/// ContactDetail stores atomic contact points (email, phone, fax)
/// </summary>
public class ContactDetail : BaseEntity
{
    public ContactDetailType DetailType { get; set; } = ContactDetailType.Email;
    public string Value { get; set; } = string.Empty; // phone number or email
    public string? Label { get; set; } // Work, Home, Mobile
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}

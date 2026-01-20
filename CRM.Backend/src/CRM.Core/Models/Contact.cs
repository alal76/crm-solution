namespace CRM.Core.Models;

/// <summary>
/// Represents a contact entity (employee, partner, lead, or other)
/// </summary>
public class Contact
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of contact: Employee, Partner, Lead, Customer, or Other
    /// </summary>
    public ContactType ContactType { get; set; }
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    
    // Contact Information
    public string? EmailPrimary { get; set; }
    public string? EmailSecondary { get; set; }
    public string? PhonePrimary { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    
    // Professional Information
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? ReportsTo { get; set; } // For employees
    
    // Relationship Information
    public string? Notes { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Navigation Properties
    public ICollection<SocialMediaLink> SocialMediaLinks { get; set; } = new List<SocialMediaLink>();
}

/// <summary>
/// Contact type enumeration
/// </summary>
public enum ContactType
{
    Employee = 0,
    Partner = 1,
    Lead = 2,
    Customer = 3,
    Vendor = 4,
    Other = 5
}

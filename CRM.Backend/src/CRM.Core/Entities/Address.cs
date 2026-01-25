namespace CRM.Core.Entities;

/// <summary>
/// Enhanced Address entity - Master table for all physical addresses
/// Shared between Customers, Contacts, Leads, and Accounts via EntityAddressLinks
/// </summary>
public class Address : BaseEntity
{
    // Basic Address Fields
    public string Label { get; set; } = "Primary";
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? County { get; set; }
    public string? CountryCode { get; set; } = "US";
    public string Country { get; set; } = "United States";
    
    // Geocoding
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? GeocodeAccuracy { get; set; }
    
    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedDate { get; set; }
    public string? VerificationSource { get; set; }
    
    // Additional Info
    public bool? IsResidential { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? AccessHours { get; set; }
    public string? SiteContactName { get; set; }
    public string? SiteContactPhone { get; set; }
    public string? Notes { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Legacy field for backward compatibility
    public bool IsPrimary { get; set; } = false;
    
    // Navigation Properties
    public ICollection<EntityAddressLink>? EntityAddressLinks { get; set; }
    
    // Computed Properties
    public string FormattedAddress => 
        string.Join(", ", new[] { Line1, Line2, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}

namespace CRM.Core.Entities;

/// <summary>
/// Enhanced Address entity - Master table for all physical addresses
/// Shared between Customers, Contacts, Leads, and Accounts via EntityAddressLinks
/// Now linked to ZipCode master data for cascading dropdown support
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
    
    // Link to ZipCode master data for cascading dropdown support
    public int? ZipCodeId { get; set; }
    
    // Link to Locality (neighborhood/subdivision within a city)
    public int? LocalityId { get; set; }
    
    /// <summary>
    /// Locality/neighborhood name (denormalized for quick access)
    /// </summary>
    public string? Locality { get; set; }
    
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
    
    /// <summary>
    /// Complete address stored as XML with element names and values
    /// Format: <Address><Label>Primary</Label><Line1>123 Main St</Line1>...</Address>
    /// </summary>
    public string? AddressXml { get; set; }
    
    // Audit
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Legacy field for backward compatibility
    public bool IsPrimary { get; set; } = false;
    
    // Navigation Properties
    public ZipCode? ZipCodeData { get; set; }
    public Locality? LocalityData { get; set; }
    public ICollection<EntityAddressLink>? EntityAddressLinks { get; set; }
    
    // Computed Properties
    public string FormattedAddress => 
        string.Join(", ", new[] { Line1, Line2, Locality, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    /// <summary>
    /// Generate XML representation of the address
    /// </summary>
    public string GenerateAddressXml()
    {
        var elements = new List<string>();
        
        void AddElement(string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                elements.Add($"<{name}>{System.Security.SecurityElement.Escape(value)}</{name}>");
        }
        
        AddElement("Label", Label);
        AddElement("Line1", Line1);
        AddElement("Line2", Line2);
        AddElement("Line3", Line3);
        AddElement("Locality", Locality);
        AddElement("City", City);
        AddElement("County", County);
        AddElement("State", State);
        AddElement("PostalCode", PostalCode);
        AddElement("CountryCode", CountryCode);
        AddElement("Country", Country);
        AddElement("Latitude", Latitude?.ToString());
        AddElement("Longitude", Longitude?.ToString());
        
        return $"<Address>{string.Join("", elements)}</Address>";
    }
    
    /// <summary>
    /// Update AddressXml from current field values
    /// </summary>
    public void UpdateAddressXml()
    {
        AddressXml = GenerateAddressXml();
    }
}


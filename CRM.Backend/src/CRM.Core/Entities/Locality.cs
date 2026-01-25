namespace CRM.Core.Entities;

/// <summary>
/// Locality (neighborhood, subdivision, area within a city)
/// Allows for custom localities when the standard ZipCode data doesn't have them
/// </summary>
public class Locality : BaseEntity
{
    /// <summary>
    /// Locality name (e.g., "Downtown", "West End", "Sector 22")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Alternate/local name for the locality
    /// </summary>
    public string? AlternateName { get; set; }
    
    /// <summary>
    /// Locality type (Neighborhood, District, Sector, Ward, etc.)
    /// </summary>
    public LocalityType LocalityType { get; set; } = LocalityType.Neighborhood;
    
    /// <summary>
    /// Reference to the ZipCode (postal code) this locality belongs to
    /// </summary>
    public int? ZipCodeId { get; set; }
    
    /// <summary>
    /// City name (denormalized for quick lookup)
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// State/Province code
    /// </summary>
    public string? StateCode { get; set; }
    
    /// <summary>
    /// Country code (ISO 3166-1 alpha-2)
    /// </summary>
    public string CountryCode { get; set; } = "US";
    
    /// <summary>
    /// Geographic latitude
    /// </summary>
    public decimal? Latitude { get; set; }
    
    /// <summary>
    /// Geographic longitude
    /// </summary>
    public decimal? Longitude { get; set; }
    
    /// <summary>
    /// Is this a user-created locality (not from master data)
    /// </summary>
    public bool IsUserCreated { get; set; } = false;
    
    /// <summary>
    /// Is this locality active/valid
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// User who created this locality (if user-created)
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    // Navigation Properties
    public ZipCode? ZipCode { get; set; }
}

/// <summary>
/// Type of locality
/// </summary>
public enum LocalityType
{
    Neighborhood = 0,
    District = 1,
    Sector = 2,
    Ward = 3,
    Block = 4,
    Zone = 5,
    Quarter = 6,
    Suburb = 7,
    Village = 8,
    Township = 9,
    Other = 99
}

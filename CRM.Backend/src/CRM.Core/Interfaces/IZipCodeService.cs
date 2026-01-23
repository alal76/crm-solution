using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for postal code lookups
/// </summary>
public interface IZipCodeService
{
    /// <summary>
    /// Lookup address information by postal code
    /// </summary>
    /// <param name="postalCode">The postal/ZIP code to lookup</param>
    /// <param name="countryCode">Optional country code (defaults to "US")</param>
    /// <returns>List of matching address details</returns>
    Task<IEnumerable<ZipCodeLookupResult>> LookupByPostalCodeAsync(string postalCode, string? countryCode = null);
    
    /// <summary>
    /// Search for cities by name
    /// </summary>
    /// <param name="city">City name or partial name</param>
    /// <param name="countryCode">Optional country code</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of matching cities</returns>
    Task<IEnumerable<ZipCodeLookupResult>> SearchByCityAsync(string city, string? countryCode = null, int limit = 20);
    
    /// <summary>
    /// Get all states/provinces for a country
    /// </summary>
    /// <param name="countryCode">Country code</param>
    /// <returns>List of states/provinces</returns>
    Task<IEnumerable<StateInfo>> GetStatesAsync(string countryCode);
    
    /// <summary>
    /// Get all cities in a state
    /// </summary>
    /// <param name="countryCode">Country code</param>
    /// <param name="stateCode">State code</param>
    /// <returns>List of cities</returns>
    Task<IEnumerable<string>> GetCitiesAsync(string countryCode, string stateCode);
    
    /// <summary>
    /// Get count of zip codes in the database
    /// </summary>
    /// <returns>Total count</returns>
    Task<int> GetZipCodeCountAsync();
}

/// <summary>
/// Result of a postal code lookup
/// </summary>
public class ZipCodeLookupResult
{
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? StateCode { get; set; }
    public string? County { get; set; }
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// State/province information
/// </summary>
public class StateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

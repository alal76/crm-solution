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
    /// Get all countries available in the database
    /// </summary>
    /// <returns>List of countries with codes</returns>
    Task<IEnumerable<CountryInfo>> GetCountriesAsync();
    
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
    /// Get postal codes for a city
    /// </summary>
    /// <param name="countryCode">Country code</param>
    /// <param name="stateCode">State code</param>
    /// <param name="city">City name</param>
    /// <returns>List of postal codes with location details</returns>
    Task<IEnumerable<ZipCodeLookupResult>> GetPostalCodesForCityAsync(string countryCode, string stateCode, string city);
    
    /// <summary>
    /// Validate a postal code format for a specific country
    /// </summary>
    /// <param name="postalCode">The postal code to validate</param>
    /// <param name="countryCode">Country code</param>
    /// <returns>Validation result</returns>
    Task<ZipCodeValidationResult> ValidatePostalCodeAsync(string postalCode, string countryCode);
    
    /// <summary>
    /// Get count of zip codes in the database
    /// </summary>
    /// <returns>Total count</returns>
    Task<int> GetZipCodeCountAsync();
    
    /// <summary>
    /// Get total count of ZIP codes in the database
    /// </summary>
    /// <returns>Total count</returns>
    Task<int> GetTotalCountAsync();
    
    /// <summary>
    /// Get count of distinct countries in the database
    /// </summary>
    /// <returns>Country count</returns>
    Task<int> GetCountryCountAsync();
    
    /// <summary>
    /// Get localities (neighborhoods) for a postal code
    /// </summary>
    /// <param name="zipCodeId">ZipCode ID</param>
    /// <returns>List of localities</returns>
    Task<IEnumerable<LocalityInfo>> GetLocalitiesAsync(int zipCodeId);
    
    /// <summary>
    /// Get localities by city
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="countryCode">Country code</param>
    /// <returns>List of localities</returns>
    Task<IEnumerable<LocalityInfo>> GetLocalitiesByCityAsync(string city, string countryCode);
    
    /// <summary>
    /// Create a new user-defined locality
    /// </summary>
    /// <param name="name">Locality name</param>
    /// <param name="city">City name</param>
    /// <param name="stateCode">State code</param>
    /// <param name="countryCode">Country code</param>
    /// <param name="zipCodeId">Optional ZipCode ID</param>
    /// <param name="userId">User creating the locality</param>
    /// <returns>Created locality info</returns>
    Task<LocalityInfo> CreateLocalityAsync(string name, string city, string? stateCode, string countryCode, int? zipCodeId, int userId);
}

/// <summary>
/// Result of a postal code lookup
/// </summary>
public class ZipCodeLookupResult
{
    public int Id { get; set; }
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
/// Country information
/// </summary>
public class CountryInfo
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string PostalCodeFormat { get; set; } = string.Empty;
    public string PostalCodeRegex { get; set; } = string.Empty;
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

/// <summary>
/// Locality (neighborhood/subdivision) information
/// </summary>
public class LocalityInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AlternateName { get; set; }
    public string LocalityType { get; set; } = "Neighborhood";
    public int? ZipCodeId { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public bool IsUserCreated { get; set; }
}

/// <summary>
/// Postal code validation result
/// </summary>
public class ZipCodeValidationResult
{
    public bool IsValid { get; set; }
    public bool ExistsInDatabase { get; set; }
    public bool FormatValid { get; set; }
    public string? Message { get; set; }
    public string? ExpectedFormat { get; set; }
}

using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// API controller for postal/zip code lookups - supports address auto-population
/// with cascading dropdowns for Country, State, City, and Locality selection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Route("api/postalcodes")] // Alias route for Postal/Zip Code naming
public class ZipCodesController : ControllerBase
{
    private readonly IZipCodeService _zipCodeService;
    private readonly ILogger<ZipCodesController> _logger;

    public ZipCodesController(IZipCodeService zipCodeService, ILogger<ZipCodesController> logger)
    {
        _zipCodeService = zipCodeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available countries with postal/zip code formats
    /// </summary>
    /// <returns>List of countries</returns>
    [HttpGet("countries")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CountryInfo>>> GetCountries()
    {
        var results = await _zipCodeService.GetCountriesAsync();
        return Ok(results);
    }

    /// <summary>
    /// Lookup address information by postal code
    /// </summary>
    /// <param name="postalCode">The postal/ZIP code to lookup</param>
    /// <param name="countryCode">Optional country code (defaults to all countries)</param>
    /// <returns>List of matching address details</returns>
    [HttpGet("lookup/{postalCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ZipCodeLookupResult>>> LookupByPostalCode(
        string postalCode, 
        [FromQuery] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return BadRequest("Postal code is required");
        }

        var results = await _zipCodeService.LookupByPostalCodeAsync(postalCode, countryCode);
        return Ok(results);
    }

    /// <summary>
    /// Search for cities by name
    /// </summary>
    /// <param name="city">City name or partial name</param>
    /// <param name="countryCode">Optional country code</param>
    /// <param name="limit">Maximum number of results (default 20)</param>
    /// <returns>List of matching cities with postal codes</returns>
    [HttpGet("search/city")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ZipCodeLookupResult>>> SearchByCity(
        [FromQuery] string city, 
        [FromQuery] string? countryCode = null,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(city) || city.Length < 2)
        {
            return BadRequest("City name must be at least 2 characters");
        }

        var results = await _zipCodeService.SearchByCityAsync(city, countryCode, limit);
        return Ok(results);
    }

    /// <summary>
    /// Get all states/provinces for a country
    /// </summary>
    /// <param name="countryCode">Country code (e.g., "US", "CA")</param>
    /// <returns>List of states/provinces</returns>
    [HttpGet("states/{countryCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<StateInfo>>> GetStates(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Country code is required");
        }

        var results = await _zipCodeService.GetStatesAsync(countryCode);
        return Ok(results);
    }

    /// <summary>
    /// Get all cities in a state
    /// </summary>
    /// <param name="countryCode">Country code</param>
    /// <param name="stateCode">State code</param>
    /// <returns>List of city names</returns>
    [HttpGet("cities/{countryCode}/{stateCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<string>>> GetCities(string countryCode, string stateCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(stateCode))
        {
            return BadRequest("Country code and state code are required");
        }

        var results = await _zipCodeService.GetCitiesAsync(countryCode, stateCode);
        return Ok(results);
    }

    /// <summary>
    /// Get postal codes for a specific city
    /// </summary>
    /// <param name="countryCode">Country code</param>
    /// <param name="stateCode">State code</param>
    /// <param name="city">City name</param>
    /// <returns>List of postal codes</returns>
    [HttpGet("postalcodes/{countryCode}/{stateCode}/{city}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ZipCodeLookupResult>>> GetPostalCodes(string countryCode, string stateCode, string city)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(stateCode) || string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("Country code, state code, and city are required");
        }

        var results = await _zipCodeService.GetPostalCodesForCityAsync(countryCode, stateCode, city);
        return Ok(results);
    }

    /// <summary>
    /// Validate a postal code for a country
    /// </summary>
    /// <param name="postalCode">Postal code to validate</param>
    /// <param name="countryCode">Country code</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<ZipCodeValidationResult>> ValidatePostalCode(
        [FromQuery] string postalCode,
        [FromQuery] string countryCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Postal code and country code are required");
        }

        var result = await _zipCodeService.ValidatePostalCodeAsync(postalCode, countryCode);
        return Ok(result);
    }

    /// <summary>
    /// Get count of zip codes in the database
    /// </summary>
    /// <returns>Total count of zip codes</returns>
    [HttpGet("count")]
    [Authorize]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await _zipCodeService.GetZipCodeCountAsync();
        return Ok(new { count });
    }

    /// <summary>
    /// Get localities for a specific postal/zip code
    /// </summary>
    /// <param name="zipCodeId">The ID of the zip code</param>
    /// <returns>List of localities within the postal/zip code area</returns>
    [HttpGet("localities/{zipCodeId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LocalityInfo>>> GetLocalities(int zipCodeId)
    {
        var localities = await _zipCodeService.GetLocalitiesAsync(zipCodeId);
        return Ok(localities);
    }

    /// <summary>
    /// Get localities by city name
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="countryCode">Optional country code filter</param>
    /// <returns>List of localities in the city</returns>
    [HttpGet("localities/city")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LocalityInfo>>> GetLocalitiesByCity(
        [FromQuery] string city,
        [FromQuery] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City name is required");
        }

        var localities = await _zipCodeService.GetLocalitiesByCityAsync(city, countryCode);
        return Ok(localities);
    }

    /// <summary>
    /// Create a new locality (for user-defined neighborhoods/areas not in master data)
    /// </summary>
    /// <param name="request">Locality creation request</param>
    /// <returns>The created locality</returns>
    [HttpPost("localities")]
    [Authorize]
    public async Task<ActionResult<LocalityInfo>> CreateLocality([FromBody] CreateLocalityRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("User not authenticated");
            }

            var locality = await _zipCodeService.CreateLocalityAsync(
                request.Name,
                request.City,
                request.StateCode,
                request.CountryCode,
                request.ZipCodeId,
                userId);

            return CreatedAtAction(nameof(GetLocalities), new { zipCodeId = request.ZipCodeId }, locality);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

/// <summary>
/// Request model for creating a new locality
/// </summary>
public class CreateLocalityRequest
{
    /// <summary>
    /// Name of the locality (neighborhood, subdivision, etc.)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional alternate name for the locality
    /// </summary>
    public string? AlternateName { get; set; }

    /// <summary>
    /// Type of locality (Neighborhood, Subdivision, District, etc.)
    /// </summary>
    public string LocalityType { get; set; } = "Neighborhood";

    /// <summary>
    /// ID of the associated zip/postal code
    /// </summary>
    public int? ZipCodeId { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province code
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string StateCode { get; set; } = string.Empty;

    /// <summary>
    /// Country code (ISO 2-letter)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public string CountryCode { get; set; } = string.Empty;
}

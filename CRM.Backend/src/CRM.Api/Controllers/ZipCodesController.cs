// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.API.Controllers;

/// <summary>
/// API controller for postal/zip code lookups - supports address auto-population
/// with cascading dropdowns for Country, State, City, and Locality selection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Route("api/postalcodes")] // Alias route for Postal/Zip Code naming
public class ZipCodesController : CrmControllerBase
{
    private readonly IZipCodeService _zipCodeService;
    private readonly IZipCodeImportService? _zipCodeImportService;
    private readonly IZipCodeImportQueue? _importQueue;
    private readonly ILogger<ZipCodesController> _logger;

    public ZipCodesController(
        IZipCodeService zipCodeService,
        ILogger<ZipCodesController> logger,
        IZipCodeImportService? zipCodeImportService = null,
        IZipCodeImportQueue? importQueue = null)
    {
        _zipCodeService = zipCodeService;
        _logger = logger;
        _zipCodeImportService = zipCodeImportService;
        _importQueue = importQueue;
    }

    /// <summary>
    /// Get all available countries with postal/zip code formats
    /// </summary>
    /// <returns>List of countries</returns>
    [HttpGet("countries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CountryInfo>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<ZipCodeLookupResult>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<ZipCodeLookupResult>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<StateInfo>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<ZipCodeLookupResult>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ZipCodeValidationResult), StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<LocalityInfo>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(IEnumerable<LocalityInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocalityInfo>>> GetLocalitiesByCity(
        [FromQuery] string city,
        [FromQuery] string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City name is required");
        }

        var localities = await _zipCodeService.GetLocalitiesByCityAsync(city, countryCode ?? "US");
        return Ok(localities);
    }

    /// <summary>
    /// Create a new locality (for user-defined neighborhoods/areas not in master data)
    /// </summary>
    /// <param name="request">Locality creation request</param>
    /// <returns>The created locality</returns>
    [HttpPost("localities")]
    [Authorize]
    [ProducesResponseType(typeof(LocalityInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    #region Import Endpoints

    /// <summary>
    /// Trigger a background ZIP code import job.
    /// Returns 202 Accepted immediately; poll GET import/status for progress.
    /// </summary>
    /// <param name="request">Trigger request specifying source, optional country code, and optional CSV URL</param>
    [HttpPost("import/trigger")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult TriggerImport([FromBody] ImportTriggerRequest? request = null)
    {
        if (_importQueue == null)
            return NotFound("Import queue is not available. Ensure the import worker service is registered.");

        if (_zipCodeImportService?.IsImportRunning == true)
            return Conflict(new { message = "An import is already in progress", hint = "Poll GET import/status for progress" });

        var source = request?.Source ?? "GeoNames";
        var importRequest = new ZipCodeImportRequest(
            Source: source,
            CountryCode: request?.CountryCode?.ToUpperInvariant(),
            Url: request?.Url,
            RequestedBy: User.Identity?.Name ?? "admin");

        _importQueue.TryEnqueue(importRequest);

        _logger.LogInformation(
            "ZIP import queued (source={Source}, country={Country}) by {User}",
            source, request?.CountryCode ?? "all", User.Identity?.Name ?? "admin");

        return Accepted(new
        {
            message = $"Import job queued (source={source}). Poll GET api/zipcodes/import/status for progress.",
            source,
            countryCode = request?.CountryCode,
            statusUrl = Url.Action(nameof(GetImportStatus))
        });
    }

    /// <summary>
    /// Get the current ZIP code import status
    /// </summary>
    /// <returns>Import status including progress if running</returns>
    [HttpGet("import/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ZipCodeImportStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ZipCodeImportStatus>> GetImportStatus()
    {
        if (_zipCodeImportService == null)
        {
            return NotFound("ZIP code import service is not configured");
        }

        var status = await _zipCodeImportService.GetImportStatusAsync();
        return Ok(status);
    }

    /// <summary>
    /// Queue a background import of all countries from GeoNames.
    /// Returns 202 immediately; poll GET import/status for progress.
    /// </summary>
    [HttpPost("import/geonames")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult ImportFromGeoNames()
    {
        if (_importQueue == null)
            return NotFound("Import worker is not available");
        if (_zipCodeImportService?.IsImportRunning == true)
            return Conflict(new { message = "An import is already in progress" });

        _importQueue.TryEnqueue(new ZipCodeImportRequest("GeoNames", RequestedBy: User.Identity?.Name ?? "admin"));
        _logger.LogInformation("Admin queued GeoNames all-countries ZIP import");
        return Accepted(new { message = "GeoNames all-countries import queued.", statusUrl = Url.Action(nameof(GetImportStatus)) });
    }

    /// <summary>
    /// Queue a background import of a single country from GeoNames.
    /// Returns 202 immediately; poll GET import/status for progress.
    /// </summary>
    [HttpPost("import/geonames/{countryCode}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult ImportCountryFromGeoNames(string countryCode)
    {
        if (_importQueue == null)
            return NotFound("Import worker is not available");
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            return BadRequest("Country code must be a 2-letter ISO code");
        if (_zipCodeImportService?.IsImportRunning == true)
            return Conflict(new { message = "An import is already in progress" });

        _importQueue.TryEnqueue(new ZipCodeImportRequest("GeoNames-Country", CountryCode: countryCode.ToUpperInvariant(), RequestedBy: User.Identity?.Name ?? "admin"));
        _logger.LogInformation("Admin queued GeoNames ZIP import for {Country}", countryCode.ToUpperInvariant());
        return Accepted(new { message = $"GeoNames import for {countryCode.ToUpperInvariant()} queued.", statusUrl = Url.Action(nameof(GetImportStatus)) });
    }

    /// <summary>
    /// Queue a background import from a GitHub repository.
    /// Returns 202 immediately; poll GET import/status for progress.
    /// </summary>
    [HttpPost("import/github")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult ImportFromGitHub([FromBody] GitHubImportRequest? request = null)
    {
        if (_importQueue == null)
            return NotFound("Import worker is not available");
        if (_zipCodeImportService?.IsImportRunning == true)
            return Conflict(new { message = "An import is already in progress" });

        _importQueue.TryEnqueue(new ZipCodeImportRequest("GitHub", Url: request?.Url, RequestedBy: User.Identity?.Name ?? "admin"));
        _logger.LogInformation("Admin queued GitHub ZIP import from {Url}", request?.Url ?? "default");
        return Accepted(new { message = "GitHub ZIP import queued.", statusUrl = Url.Action(nameof(GetImportStatus)) });
    }

    /// <summary>
    /// Import ZIP codes from an uploaded CSV file.
    /// Accepts the Zeeshanahmad4 format (COUNTRY, POSTAL_CODE, CITY, STATE, SHORT_STATE,
    /// COUNTY, SHORT_COUNTY, COMMUNITY, SHORT_COMMUNITY, LATITUDE, LONGITUDE, ACCURACY)
    /// or any header-based CSV/TSV file with at least POSTAL_CODE and CITY columns.
    /// Download the dataset from:
    /// https://github.com/Zeeshanahmad4/Zip-code-of-all-countries-cities-in-the-world-CSV-TXT-SQL-DATABASE
    /// </summary>
    /// <param name="file">CSV file to import</param>
    /// <returns>Import result</returns>
    [HttpPost("import/csv-upload")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ZipCodeImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ZipCodeImportResult>> ImportFromCsvUpload(IFormFile file)
    {
        if (_zipCodeImportService == null)
        {
            return NotFound("ZIP code import service is not configured");
        }

        if (_zipCodeImportService.IsImportRunning)
        {
            return Conflict("An import is already in progress");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("A non-empty CSV file is required");
        }

        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".txt" && extension != ".tsv")
        {
            return BadRequest("Only .csv, .txt, and .tsv files are accepted");
        }

        _logger.LogInformation("Admin uploaded ZIP code CSV file: {FileName} ({Size:N0} bytes)",
            file.FileName, file.Length);

        using var stream = file.OpenReadStream();
        var sourceName = $"CSV Upload ({file.FileName})";
        var result = await _zipCodeImportService.ImportFromCsvStreamAsync(stream, sourceName);

        if (result.Success)
        {
            return Ok(result);
        }
        return StatusCode(500, result);
    }

    /// <summary>
    /// Get ZIP code statistics
    /// </summary>
    /// <returns>Statistics about ZIP code data</returns>
    [HttpGet("stats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ZipCodeStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<ZipCodeStats>> GetStats()
    {
        var countryCount = await _zipCodeService.GetCountryCountAsync();
        var totalCount = await _zipCodeService.GetTotalCountAsync();

        return Ok(new ZipCodeStats
        {
            TotalRecords = totalCount,
            CountryCount = countryCount
        });
    }

    #endregion
}

/// <summary>
/// Request for GitHub ZIP code import
/// </summary>
public class GitHubImportRequest
{
    /// <summary>
    /// Custom GitHub raw URL for ZIP code data JSON file
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Request for on-demand background ZIP code import via POST import/trigger
/// </summary>
public class ImportTriggerRequest
{
    /// <summary>
    /// Import source: "GeoNames" (all countries), "GeoNames-Country" (single country),
    /// "GitHub" (JSON format), or "CsvUrl" (CSV download URL).
    /// Defaults to "GeoNames".
    /// </summary>
    public string Source { get; set; } = "GeoNames";

    /// <summary>ISO 2-letter country code – required when Source=GeoNames-Country</summary>
    public string? CountryCode { get; set; }

    /// <summary>Direct download URL – used for Source=CsvUrl or Source=GitHub</summary>
    public string? Url { get; set; }
}

/// <summary>
/// ZIP code database statistics
/// </summary>
public class ZipCodeStats
{
    public int TotalRecords { get; set; }
    public int CountryCount { get; set; }
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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for postal code lookups for address auto-population
/// </summary>
public class ZipCodeService : IZipCodeService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ZipCodeService> _logger;
    
    // Postal code formats and regex patterns by country
    private static readonly Dictionary<string, (string Format, string Regex)> PostalCodeFormats = new()
    {
        { "US", ("12345 or 12345-6789", @"^\d{5}(-\d{4})?$") },
        { "CA", ("A1A 1A1", @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$") },
        { "GB", ("AA9A 9AA", @"^[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}$") },
        { "UK", ("AA9A 9AA", @"^[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}$") },
        { "AU", ("1234", @"^\d{4}$") },
        { "DE", ("12345", @"^\d{5}$") },
        { "FR", ("12345", @"^\d{5}$") },
        { "IT", ("12345", @"^\d{5}$") },
        { "ES", ("12345", @"^\d{5}$") },
        { "NL", ("1234 AB", @"^\d{4}\s?[A-Z]{2}$") },
        { "BE", ("1234", @"^\d{4}$") },
        { "CH", ("1234", @"^\d{4}$") },
        { "AT", ("1234", @"^\d{4}$") },
        { "JP", ("123-4567", @"^\d{3}-?\d{4}$") },
        { "CN", ("123456", @"^\d{6}$") },
        { "IN", ("123456", @"^\d{6}$") },
        { "BR", ("12345-678", @"^\d{5}-?\d{3}$") },
        { "MX", ("12345", @"^\d{5}$") },
        { "RU", ("123456", @"^\d{6}$") },
        { "PL", ("12-345", @"^\d{2}-?\d{3}$") },
        { "PT", ("1234-567", @"^\d{4}-?\d{3}$") },
        { "SE", ("123 45", @"^\d{3}\s?\d{2}$") },
        { "NO", ("1234", @"^\d{4}$") },
        { "DK", ("1234", @"^\d{4}$") },
        { "FI", ("12345", @"^\d{5}$") },
        { "IE", ("A65 F4E2", @"^[A-Z\d]{3}\s?[A-Z\d]{4}$") },
        { "NZ", ("1234", @"^\d{4}$") },
        { "SG", ("123456", @"^\d{6}$") },
        { "ZA", ("1234", @"^\d{4}$") },
        { "AE", ("12345", @"^\d{5}$") },
        { "KR", ("12345", @"^\d{5}$") },
        { "ID", ("12345", @"^\d{5}$") },
        { "TH", ("12345", @"^\d{5}$") },
        { "MY", ("12345", @"^\d{5}$") },
        { "PH", ("1234", @"^\d{4}$") },
        { "VN", ("123456", @"^\d{6}$") },
        { "AR", ("1234", @"^[A-Z]?\d{4}[A-Z]{0,3}$") },
        { "CL", ("1234567", @"^\d{7}$") },
        { "CO", ("123456", @"^\d{6}$") },
        { "PE", ("12345", @"^\d{5}$") },
    };

    public ZipCodeService(CrmDbContext context, ILogger<ZipCodeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> LookupByPostalCodeAsync(string postalCode, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return Enumerable.Empty<ZipCodeLookupResult>();
        }

        try
        {
            var normalizedPostalCode = postalCode.Trim().ToUpperInvariant().Replace(" ", "");
            
            var query = _context.ZipCodes
                .Where(z => z.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                query = query.Where(z => z.CountryCode == countryCode.ToUpperInvariant());
            }

            // Try exact match first
            var results = await query
                .Where(z => z.PostalCode.Replace(" ", "").ToUpper() == normalizedPostalCode)
                .Select(z => new ZipCodeLookupResult
                {
                    Id = z.Id,
                    PostalCode = z.PostalCode,
                    City = z.City,
                    State = z.State,
                    StateCode = z.StateCode,
                    County = z.County,
                    Country = z.Country,
                    CountryCode = z.CountryCode,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude
                })
                .Take(20)
                .ToListAsync();

            // If no exact match, try prefix match
            if (!results.Any())
            {
                results = await query
                    .Where(z => z.PostalCode.Replace(" ", "").ToUpper().StartsWith(normalizedPostalCode))
                    .Select(z => new ZipCodeLookupResult
                    {
                        Id = z.Id,
                        PostalCode = z.PostalCode,
                        City = z.City,
                        State = z.State,
                        StateCode = z.StateCode,
                        County = z.County,
                        Country = z.Country,
                        CountryCode = z.CountryCode,
                        Latitude = z.Latitude,
                        Longitude = z.Longitude
                    })
                    .Take(20)
                    .ToListAsync();
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up postal code: {PostalCode}", postalCode);
            return Enumerable.Empty<ZipCodeLookupResult>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> SearchByCityAsync(string city, string? countryCode = null, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(city) || city.Length < 2)
        {
            return Enumerable.Empty<ZipCodeLookupResult>();
        }

        try
        {
            var query = _context.ZipCodes
                .Where(z => z.IsActive && z.City.ToLower().Contains(city.ToLower()))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                query = query.Where(z => z.CountryCode == countryCode.ToUpperInvariant());
            }

            return await query
                .Select(z => new ZipCodeLookupResult
                {
                    PostalCode = z.PostalCode,
                    City = z.City,
                    State = z.State,
                    StateCode = z.StateCode,
                    County = z.County,
                    Country = z.Country,
                    CountryCode = z.CountryCode,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude
                })
                .Distinct()
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for city: {City}", city);
            return Enumerable.Empty<ZipCodeLookupResult>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StateInfo>> GetStatesAsync(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return Enumerable.Empty<StateInfo>();
        }

        try
        {
            var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();

            return await _context.ZipCodes
                .Where(z => z.IsActive && z.CountryCode == normalizedCountryCode && z.State != null && z.StateCode != null)
                .Select(z => new StateInfo
                {
                    Name = z.State!,
                    Code = z.StateCode!,
                    CountryCode = z.CountryCode
                })
                .Distinct()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting states for country: {CountryCode}", countryCode);
            return Enumerable.Empty<StateInfo>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCitiesAsync(string countryCode, string stateCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(stateCode))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
            var normalizedStateCode = stateCode.Trim().ToUpperInvariant();

            return await _context.ZipCodes
                .Where(z => z.IsActive && 
                       z.CountryCode == normalizedCountryCode && 
                       z.StateCode == normalizedStateCode)
                .Select(z => z.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cities for state: {CountryCode}/{StateCode}", countryCode, stateCode);
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<int> GetZipCodeCountAsync()
    {
        try
        {
            return await _context.ZipCodes.Where(z => z.IsActive).CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting zip code count");
            return 0;
        }
    }
    
    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _context.ZipCodes.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total zip code count");
            return 0;
        }
    }
    
    /// <inheritdoc />
    public async Task<int> GetCountryCountAsync()
    {
        try
        {
            return await _context.ZipCodes
                .Where(z => z.IsActive)
                .Select(z => z.CountryCode)
                .Distinct()
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country count");
            return 0;
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<CountryInfo>> GetCountriesAsync()
    {
        try
        {
            var countries = await _context.ZipCodes
                .Where(z => z.IsActive)
                .Select(z => new { z.CountryCode, z.Country })
                .Distinct()
                .OrderBy(c => c.Country)
                .ToListAsync();
            
            return countries.Select(c => new CountryInfo
            {
                Code = c.CountryCode,
                Name = c.Country,
                PostalCodeFormat = PostalCodeFormats.TryGetValue(c.CountryCode, out var format) ? format.Format : "",
                PostalCodeRegex = PostalCodeFormats.TryGetValue(c.CountryCode, out var regex) ? regex.Regex : ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting countries");
            return Enumerable.Empty<CountryInfo>();
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> GetPostalCodesForCityAsync(string countryCode, string stateCode, string city)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(stateCode) || string.IsNullOrWhiteSpace(city))
        {
            return Enumerable.Empty<ZipCodeLookupResult>();
        }

        try
        {
            var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
            var normalizedStateCode = stateCode.Trim().ToUpperInvariant();
            var normalizedCity = city.Trim().ToLower();

            return await _context.ZipCodes
                .Where(z => z.IsActive && 
                       z.CountryCode == normalizedCountryCode && 
                       z.StateCode == normalizedStateCode &&
                       z.City.ToLower() == normalizedCity)
                .Select(z => new ZipCodeLookupResult
                {
                    PostalCode = z.PostalCode,
                    City = z.City,
                    State = z.State,
                    StateCode = z.StateCode,
                    County = z.County,
                    Country = z.Country,
                    CountryCode = z.CountryCode,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude
                })
                .OrderBy(z => z.PostalCode)
                .Take(100)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting postal codes for city: {CountryCode}/{StateCode}/{City}", countryCode, stateCode, city);
            return Enumerable.Empty<ZipCodeLookupResult>();
        }
    }
    
    /// <inheritdoc />
    public async Task<ZipCodeValidationResult> ValidatePostalCodeAsync(string postalCode, string countryCode)
    {
        var result = new ZipCodeValidationResult
        {
            IsValid = false,
            ExistsInDatabase = false,
            FormatValid = false
        };

        if (string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(countryCode))
        {
            result.Message = "Postal code and country code are required";
            return result;
        }

        var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        var normalizedPostalCode = postalCode.Trim().ToUpperInvariant();

        // Check format
        if (PostalCodeFormats.TryGetValue(normalizedCountryCode, out var format))
        {
            result.ExpectedFormat = format.Format;
            result.FormatValid = Regex.IsMatch(normalizedPostalCode, format.Regex, RegexOptions.IgnoreCase);
            
            if (!result.FormatValid)
            {
                result.Message = $"Invalid format. Expected: {format.Format}";
                return result;
            }
        }
        else
        {
            // Unknown country - accept any format
            result.FormatValid = true;
        }

        // Check if exists in database
        try
        {
            result.ExistsInDatabase = await _context.ZipCodes
                .AnyAsync(z => z.IsActive && 
                          z.CountryCode == normalizedCountryCode && 
                          z.PostalCode.Replace(" ", "").ToUpper() == normalizedPostalCode.Replace(" ", ""));
            
            result.IsValid = result.FormatValid && result.ExistsInDatabase;
            
            if (!result.ExistsInDatabase && result.FormatValid)
            {
                result.Message = "Format is valid but postal code not found in database";
            }
            else if (result.IsValid)
            {
                result.Message = "Valid postal code";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating postal code: {PostalCode}", postalCode);
            result.Message = "Error validating postal code";
        }

        return result;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<LocalityInfo>> GetLocalitiesAsync(int zipCodeId)
    {
        try
        {
            return await _context.Localities
                .Where(l => l.ZipCodeId == zipCodeId && l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocalityInfo
                {
                    Id = l.Id,
                    Name = l.Name,
                    AlternateName = l.AlternateName,
                    LocalityType = l.LocalityType.ToString(),
                    ZipCodeId = l.ZipCodeId,
                    City = l.City,
                    StateCode = l.StateCode,
                    CountryCode = l.CountryCode,
                    IsUserCreated = l.IsUserCreated
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localities for ZipCode: {ZipCodeId}", zipCodeId);
            return Enumerable.Empty<LocalityInfo>();
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<LocalityInfo>> GetLocalitiesByCityAsync(string city, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Enumerable.Empty<LocalityInfo>();
        }
        
        try
        {
            return await _context.Localities
                .Where(l => l.City.ToLower() == city.ToLower() && 
                           l.CountryCode == countryCode.ToUpperInvariant() && 
                           l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocalityInfo
                {
                    Id = l.Id,
                    Name = l.Name,
                    AlternateName = l.AlternateName,
                    LocalityType = l.LocalityType.ToString(),
                    ZipCodeId = l.ZipCodeId,
                    City = l.City,
                    StateCode = l.StateCode,
                    CountryCode = l.CountryCode,
                    IsUserCreated = l.IsUserCreated
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localities for city: {City}", city);
            return Enumerable.Empty<LocalityInfo>();
        }
    }
    
    /// <inheritdoc />
    public async Task<LocalityInfo> CreateLocalityAsync(string name, string city, string? stateCode, string countryCode, int? zipCodeId, int userId)
    {
        var locality = new Locality
        {
            Name = name.Trim(),
            City = city.Trim(),
            StateCode = stateCode?.ToUpperInvariant(),
            CountryCode = countryCode.ToUpperInvariant(),
            ZipCodeId = zipCodeId,
            IsUserCreated = true,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Localities.Add(locality);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new locality: {Name} in {City}, {CountryCode} by user {UserId}", 
            name, city, countryCode, userId);
        
        return new LocalityInfo
        {
            Id = locality.Id,
            Name = locality.Name,
            AlternateName = locality.AlternateName,
            LocalityType = locality.LocalityType.ToString(),
            ZipCodeId = locality.ZipCodeId,
            City = locality.City,
            StateCode = locality.StateCode,
            CountryCode = locality.CountryCode,
            IsUserCreated = locality.IsUserCreated
        };
    }
}

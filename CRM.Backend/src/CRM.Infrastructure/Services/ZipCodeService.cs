using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for postal code lookups for address auto-population
/// </summary>
public class ZipCodeService : IZipCodeService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<ZipCodeService> _logger;

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
}

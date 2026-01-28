/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using CRM.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Caching decorator for ZipCodeService.
/// Caches frequently accessed data like countries, states, and cities.
/// </summary>
public class CachedZipCodeService : IZipCodeService
{
    private readonly IZipCodeService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedZipCodeService> _logger;

    // Cache configuration
    private static readonly TimeSpan CountriesCacheExpiration = TimeSpan.FromHours(24);
    private static readonly TimeSpan StatesCacheExpiration = TimeSpan.FromHours(12);
    private static readonly TimeSpan CitiesCacheExpiration = TimeSpan.FromHours(6);
    private static readonly TimeSpan PostalCodesCacheExpiration = TimeSpan.FromHours(6);
    private static readonly TimeSpan LookupCacheExpiration = TimeSpan.FromMinutes(30);

    // Cache keys
    private const string CountriesCacheKey = "ZipCode:Countries";
    private const string StatesPrefix = "ZipCode:States:";
    private const string CitiesPrefix = "ZipCode:Cities:";
    private const string PostalCodesPrefix = "ZipCode:PostalCodes:";
    private const string LookupPrefix = "ZipCode:Lookup:";
    private const string ValidationPrefix = "ZipCode:Validation:";

    public CachedZipCodeService(
        IZipCodeService innerService,
        IMemoryCache cache,
        ILogger<CachedZipCodeService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CountryInfo>> GetCountriesAsync()
    {
        return await _cache.GetOrCreateAsync(CountriesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CountriesCacheExpiration;
            _logger.LogDebug("Caching countries list");
            return await _innerService.GetCountriesAsync();
        }) ?? Enumerable.Empty<CountryInfo>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StateInfo>> GetStatesAsync(string countryCode)
    {
        var cacheKey = $"{StatesPrefix}{countryCode.ToUpperInvariant()}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = StatesCacheExpiration;
            _logger.LogDebug("Caching states for country: {CountryCode}", countryCode);
            return await _innerService.GetStatesAsync(countryCode);
        }) ?? Enumerable.Empty<StateInfo>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCitiesAsync(string countryCode, string stateCode)
    {
        var cacheKey = $"{CitiesPrefix}{countryCode.ToUpperInvariant()}:{stateCode.ToUpperInvariant()}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CitiesCacheExpiration;
            _logger.LogDebug("Caching cities for state: {StateCode}, {CountryCode}", stateCode, countryCode);
            return await _innerService.GetCitiesAsync(countryCode, stateCode);
        }) ?? Enumerable.Empty<string>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> GetPostalCodesForCityAsync(string countryCode, string stateCode, string city)
    {
        var cacheKey = $"{PostalCodesPrefix}{countryCode.ToUpperInvariant()}:{stateCode.ToUpperInvariant()}:{city.ToLowerInvariant()}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = PostalCodesCacheExpiration;
            _logger.LogDebug("Caching postal codes for city: {City}, {StateCode}", city, stateCode);
            return await _innerService.GetPostalCodesForCityAsync(countryCode, stateCode, city);
        }) ?? Enumerable.Empty<ZipCodeLookupResult>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> LookupByPostalCodeAsync(string postalCode, string? countryCode = null)
    {
        var cacheKey = $"{LookupPrefix}{postalCode.ToUpperInvariant()}:{countryCode?.ToUpperInvariant() ?? "ALL"}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LookupCacheExpiration;
            return await _innerService.LookupByPostalCodeAsync(postalCode, countryCode);
        }) ?? Enumerable.Empty<ZipCodeLookupResult>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ZipCodeLookupResult>> SearchByCityAsync(string city, string? countryCode = null, int limit = 20)
    {
        // Don't cache city searches as they can have many variations
        return await _innerService.SearchByCityAsync(city, countryCode, limit);
    }

    /// <inheritdoc />
    public async Task<ZipCodeValidationResult> ValidatePostalCodeAsync(string postalCode, string countryCode)
    {
        var cacheKey = $"{ValidationPrefix}{postalCode.ToUpperInvariant()}:{countryCode.ToUpperInvariant()}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LookupCacheExpiration;
            return await _innerService.ValidatePostalCodeAsync(postalCode, countryCode);
        }) ?? new ZipCodeValidationResult { IsValid = false, Message = "Validation failed" };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocalityInfo>> GetLocalitiesAsync(int zipCodeId)
    {
        // Don't cache localities as they can be user-created
        return await _innerService.GetLocalitiesAsync(zipCodeId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LocalityInfo>> GetLocalitiesByCityAsync(string city, string? countryCode = null)
    {
        return await _innerService.GetLocalitiesByCityAsync(city, countryCode ?? "US");
    }

    /// <inheritdoc />
    public async Task<LocalityInfo> CreateLocalityAsync(string name, string city, string? stateCode, string countryCode, int? zipCodeId, int userId)
    {
        return await _innerService.CreateLocalityAsync(name, city, stateCode, countryCode, zipCodeId, userId);
    }

    /// <inheritdoc />
    public async Task<int> GetZipCodeCountAsync()
    {
        return await _innerService.GetZipCodeCountAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync()
    {
        return await _innerService.GetTotalCountAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetCountryCountAsync()
    {
        return await _innerService.GetCountryCountAsync();
    }

    /// <summary>
    /// Clears all zip code related caches.
    /// Call this when master data is reseeded.
    /// </summary>
    public void ClearCache()
    {
        _logger.LogInformation("Clearing zip code cache");
        
        // Note: IMemoryCache doesn't have a Clear method, so we need to
        // remove specific keys or use a different approach.
        // For now, we'll rely on expiration.
        
        // Remove known fixed keys
        _cache.Remove(CountriesCacheKey);
    }
}

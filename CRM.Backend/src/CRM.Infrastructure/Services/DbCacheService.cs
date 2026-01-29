/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Cache key constants for database entities
/// </summary>
public static class DbCacheKeys
{
    public const string Departments = "db:departments";
    public const string DepartmentById = "db:department:id:";
    public const string DepartmentByCode = "db:department:code:";
    public const string UserGroups = "db:usergroups";
    public const string UserGroupById = "db:usergroup:id:";
    public const string LookupCategories = "db:lookupcategories";
    public const string LookupItems = "db:lookupitems:";
    public const string Products = "db:products";
    public const string ModuleFieldConfigs = "db:modulefieldconfigs";
    public const string ModuleFieldConfigByModule = "db:modulefieldconfig:module:";
    public const string ZipCodes = "db:zipcodes";
    public const string Countries = "db:countries";
    public const string States = "db:states:";
    public const string Cities = "db:cities:";
}

/// <summary>
/// Interface for database entity caching using Redis
/// </summary>
public interface IDbCacheService
{
    // Departments
    Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken ct = default);
    Task<Department?> GetDepartmentByIdAsync(int id, CancellationToken ct = default);
    Task<Department?> GetDepartmentByCodeAsync(string code, CancellationToken ct = default);
    Task InvalidateDepartmentsAsync(CancellationToken ct = default);
    
    // User Groups
    Task<IEnumerable<UserGroup>> GetUserGroupsAsync(CancellationToken ct = default);
    Task<UserGroup?> GetUserGroupByIdAsync(int id, CancellationToken ct = default);
    Task InvalidateUserGroupsAsync(CancellationToken ct = default);
    
    // Lookups
    Task<IEnumerable<LookupCategory>> GetLookupCategoriesAsync(CancellationToken ct = default);
    Task<IEnumerable<LookupItem>> GetLookupItemsAsync(string categoryName, CancellationToken ct = default);
    Task InvalidateLookupsAsync(CancellationToken ct = default);
    
    // Products
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken ct = default);
    Task InvalidateProductsAsync(CancellationToken ct = default);
    
    // Module Field Configurations
    Task<IEnumerable<ModuleFieldConfiguration>> GetModuleFieldConfigsAsync(CancellationToken ct = default);
    Task<IEnumerable<ModuleFieldConfiguration>> GetModuleFieldConfigsByModuleAsync(string moduleName, CancellationToken ct = default);
    Task InvalidateModuleFieldConfigsAsync(CancellationToken ct = default);
    
    // Cache Management
    Task InvalidateAllAsync(CancellationToken ct = default);
    Task WarmupCacheAsync(CancellationToken ct = default);
}

/// <summary>
/// Database entity caching service using Redis
/// Caches frequently accessed reference data to reduce database load
/// </summary>
public class DbCacheService : IDbCacheService
{
    private readonly CrmDbContext _context;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<DbCacheService> _logger;

    // Cache durations
    private static readonly TimeSpan ShortDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MediumDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan LongDuration = TimeSpan.FromHours(2);

    public DbCacheService(
        CrmDbContext context,
        IRedisCacheService cache,
        ILogger<DbCacheService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    #region Departments

    public async Task<IEnumerable<Department>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            DbCacheKeys.Departments,
            async () =>
            {
                _logger.LogDebug("Loading departments from database");
                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .OrderBy(d => d.Name)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<Department> { Items = departments };
            },
            LongDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<Department>();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id, CancellationToken ct = default)
    {
        var wrapper = await _cache.GetOrSetAsync(
            $"{DbCacheKeys.DepartmentById}{id}",
            async () =>
            {
                var dept = await _context.Departments
                    .Where(d => d.Id == id && !d.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);
                return new CacheWrapper<Department> { Items = dept != null ? new List<Department> { dept } : new List<Department>() };
            },
            MediumDuration,
            ct);
        return wrapper?.Items?.FirstOrDefault();
    }

    public async Task<Department?> GetDepartmentByCodeAsync(string code, CancellationToken ct = default)
    {
        var wrapper = await _cache.GetOrSetAsync(
            $"{DbCacheKeys.DepartmentByCode}{code.ToUpperInvariant()}",
            async () =>
            {
                var dept = await _context.Departments
                    .Where(d => d.DepartmentCode == code && !d.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);
                return new CacheWrapper<Department> { Items = dept != null ? new List<Department> { dept } : new List<Department>() };
            },
            MediumDuration,
            ct);
        return wrapper?.Items?.FirstOrDefault();
    }

    public async Task InvalidateDepartmentsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByPrefixAsync("db:department", ct);
    }

    #endregion

    #region User Groups

    public async Task<IEnumerable<UserGroup>> GetUserGroupsAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            DbCacheKeys.UserGroups,
            async () =>
            {
                _logger.LogDebug("Loading user groups from database");
                var groups = await _context.UserGroups
                    .Where(g => !g.IsDeleted && g.IsActive)
                    .OrderBy(g => g.DisplayOrder)
                    .ThenBy(g => g.Name)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<UserGroup> { Items = groups };
            },
            LongDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<UserGroup>();
    }

    public async Task<UserGroup?> GetUserGroupByIdAsync(int id, CancellationToken ct = default)
    {
        var wrapper = await _cache.GetOrSetAsync(
            $"{DbCacheKeys.UserGroupById}{id}",
            async () =>
            {
                var group = await _context.UserGroups
                    .Where(g => g.Id == id && !g.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);
                return new CacheWrapper<UserGroup> { Items = group != null ? new List<UserGroup> { group } : new List<UserGroup>() };
            },
            MediumDuration,
            ct);
        return wrapper?.Items?.FirstOrDefault();
    }

    public async Task InvalidateUserGroupsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByPrefixAsync("db:usergroup", ct);
    }

    #endregion

    #region Lookups

    public async Task<IEnumerable<LookupCategory>> GetLookupCategoriesAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            DbCacheKeys.LookupCategories,
            async () =>
            {
                _logger.LogDebug("Loading lookup categories from database");
                var categories = await _context.LookupCategories
                    .Where(c => c.IsActive)
                    .Include(c => c.Items.Where(i => i.IsActive))
                    .OrderBy(c => c.Name)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<LookupCategory> { Items = categories };
            },
            LongDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<LookupCategory>();
    }

    public async Task<IEnumerable<LookupItem>> GetLookupItemsAsync(string categoryName, CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            $"{DbCacheKeys.LookupItems}{categoryName}",
            async () =>
            {
                _logger.LogDebug("Loading lookup items for category: {Category}", categoryName);
                var category = await _context.LookupCategories
                    .Include(c => c.Items.Where(i => i.IsActive))
                    .FirstOrDefaultAsync(c => c.Name == categoryName && c.IsActive, ct);

                var items = category?.Items?.OrderBy(i => i.SortOrder).ToList() 
                    ?? new List<LookupItem>();
                return new CacheWrapper<LookupItem> { Items = items };
            },
            MediumDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<LookupItem>();
    }

    public async Task InvalidateLookupsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByPrefixAsync("db:lookup", ct);
    }

    #endregion

    #region Products

    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            DbCacheKeys.Products,
            async () =>
            {
                _logger.LogDebug("Loading products from database");
                var products = await _context.Products
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .OrderBy(p => p.Name)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<Product> { Items = products };
            },
            ShortDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<Product>();
    }

    public async Task InvalidateProductsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveAsync(DbCacheKeys.Products, ct);
    }

    #endregion

    #region Module Field Configurations

    public async Task<IEnumerable<ModuleFieldConfiguration>> GetModuleFieldConfigsAsync(CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            DbCacheKeys.ModuleFieldConfigs,
            async () =>
            {
                _logger.LogDebug("Loading module field configurations from database");
                var configs = await _context.ModuleFieldConfigurations
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.ModuleName)
                    .ThenBy(c => c.DisplayOrder)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<ModuleFieldConfiguration> { Items = configs };
            },
            LongDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<ModuleFieldConfiguration>();
    }

    public async Task<IEnumerable<ModuleFieldConfiguration>> GetModuleFieldConfigsByModuleAsync(string moduleName, CancellationToken ct = default)
    {
        var result = await _cache.GetOrSetAsync(
            $"{DbCacheKeys.ModuleFieldConfigByModule}{moduleName}",
            async () =>
            {
                _logger.LogDebug("Loading module field configurations for module: {Module}", moduleName);
                var configs = await _context.ModuleFieldConfigurations
                    .Where(c => c.ModuleName == moduleName && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .AsNoTracking()
                    .ToListAsync(ct);
                return new CacheWrapper<ModuleFieldConfiguration> { Items = configs };
            },
            MediumDuration,
            ct);

        return result?.Items ?? Enumerable.Empty<ModuleFieldConfiguration>();
    }

    public async Task InvalidateModuleFieldConfigsAsync(CancellationToken ct = default)
    {
        await _cache.RemoveByPrefixAsync("db:modulefieldconfig", ct);
    }

    #endregion

    #region Cache Management

    public async Task InvalidateAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Invalidating all database caches");
        await _cache.RemoveByPrefixAsync("db:", ct);
    }

    public async Task WarmupCacheAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Warming up database cache");
        
        try
        {
            // Load frequently accessed data into cache
            await GetDepartmentsAsync(ct);
            await GetUserGroupsAsync(ct);
            await GetLookupCategoriesAsync(ct);
            await GetModuleFieldConfigsAsync(ct);
            await GetProductsAsync(ct);
            
            _logger.LogInformation("Database cache warmup completed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache warmup failed - will populate on demand");
        }
    }

    #endregion
}

/// <summary>
/// Wrapper class for caching collections (required for proper JSON serialization)
/// </summary>
internal class CacheWrapper<T>
{
    public List<T> Items { get; set; } = new();
}

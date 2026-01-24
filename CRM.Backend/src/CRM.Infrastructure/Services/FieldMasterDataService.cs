using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing field-to-master-data links
/// </summary>
public class FieldMasterDataService : IFieldMasterDataService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<FieldMasterDataService> _logger;

    public FieldMasterDataService(ICrmDbContext context, ILogger<FieldMasterDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<FieldMasterDataLinkDto>> GetLinksForFieldAsync(int fieldConfigurationId)
    {
        var links = await _context.FieldMasterDataLinks
            .Where(l => l.FieldConfigurationId == fieldConfigurationId && !l.IsDeleted && l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        return links.Select(MapToDto).ToList();
    }

    public async Task<Dictionary<int, List<FieldMasterDataLinkDto>>> GetLinksForModuleAsync(string moduleName)
    {
        var fieldIds = await _context.ModuleFieldConfigurations
            .Where(f => f.ModuleName == moduleName && !f.IsDeleted)
            .Select(f => f.Id)
            .ToListAsync();

        var links = await _context.FieldMasterDataLinks
            .Where(l => fieldIds.Contains(l.FieldConfigurationId) && !l.IsDeleted && l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        return links.GroupBy(l => l.FieldConfigurationId)
            .ToDictionary(g => g.Key, g => g.Select(MapToDto).ToList());
    }

    public async Task<FieldMasterDataLinkDto?> GetLinkByIdAsync(int id)
    {
        var link = await _context.FieldMasterDataLinks
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        return link != null ? MapToDto(link) : null;
    }

    public async Task<FieldMasterDataLinkDto> CreateLinkAsync(CreateFieldMasterDataLinkDto dto)
    {
        var link = new FieldMasterDataLink
        {
            FieldConfigurationId = dto.FieldConfigurationId,
            SourceType = dto.SourceType,
            SourceName = dto.SourceName,
            DisplayField = dto.DisplayField,
            ValueField = dto.ValueField,
            FilterExpression = dto.FilterExpression,
            DependsOnField = dto.DependsOnField,
            DependsOnSourceColumn = dto.DependsOnSourceColumn,
            AllowFreeText = dto.AllowFreeText,
            ValidationType = dto.ValidationType,
            ValidationPattern = dto.ValidationPattern,
            ValidationMessage = dto.ValidationMessage,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.FieldMasterDataLinks.Add(link);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created master data link {LinkId} for field {FieldId}", link.Id, dto.FieldConfigurationId);
        return MapToDto(link);
    }

    public async Task<FieldMasterDataLinkDto> UpdateLinkAsync(int id, CreateFieldMasterDataLinkDto dto)
    {
        var link = await _context.FieldMasterDataLinks
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (link == null)
            throw new KeyNotFoundException($"Link with ID {id} not found");

        link.SourceType = dto.SourceType;
        link.SourceName = dto.SourceName;
        link.DisplayField = dto.DisplayField;
        link.ValueField = dto.ValueField;
        link.FilterExpression = dto.FilterExpression;
        link.DependsOnField = dto.DependsOnField;
        link.DependsOnSourceColumn = dto.DependsOnSourceColumn;
        link.AllowFreeText = dto.AllowFreeText;
        link.ValidationType = dto.ValidationType;
        link.ValidationPattern = dto.ValidationPattern;
        link.ValidationMessage = dto.ValidationMessage;
        link.SortOrder = dto.SortOrder;
        link.IsActive = dto.IsActive;
        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated master data link {LinkId}", id);
        return MapToDto(link);
    }

    public async Task<bool> DeleteLinkAsync(int id)
    {
        var link = await _context.FieldMasterDataLinks
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (link == null)
            return false;

        link.IsDeleted = true;
        link.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted master data link {LinkId}", id);
        return true;
    }

    public async Task<List<MasterDataSourceDto>> GetAvailableSourcesAsync()
    {
        var sources = new List<MasterDataSourceDto>();

        // Add lookup categories
        var categories = await _context.LookupCategories
            .Where(c => !c.IsDeleted && c.IsActive)
            .Select(c => c.Name)
            .ToListAsync();

        foreach (var category in categories)
        {
            sources.Add(new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.LookupCategory,
                SourceName = category,
                DisplayName = $"Lookup: {category}",
                AvailableFields = new List<string> { "Key", "Value", "Meta" }
            });
        }

        // Add database tables
        sources.AddRange(new[]
        {
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Table,
                SourceName = "ZipCodes",
                DisplayName = "Postal Codes / ZIP Codes",
                AvailableFields = new List<string> { "PostalCode", "City", "State", "StateCode", "County", "Country", "CountryCode" }
            },
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Table,
                SourceName = "Products",
                DisplayName = "Products",
                AvailableFields = new List<string> { "Id", "Name", "ProductCode", "Description", "Category", "UnitPrice" }
            },
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Table,
                SourceName = "Customers",
                DisplayName = "Customers",
                AvailableFields = new List<string> { "Id", "CompanyName", "AccountNumber", "Industry", "Website" }
            },
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Table,
                SourceName = "Users",
                DisplayName = "Users",
                AvailableFields = new List<string> { "Id", "FirstName", "LastName", "Email", "Role" }
            },
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Table,
                SourceName = "ServiceRequestTypes",
                DisplayName = "Service Request Types",
                AvailableFields = new List<string> { "Id", "Name", "RequestType", "CategoryId" }
            }
        });

        // Add API endpoints
        sources.AddRange(new[]
        {
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Api,
                SourceName = "/api/zipcodes/countries",
                DisplayName = "API: Countries",
                AvailableFields = new List<string> { "code", "name", "postalCodeFormat" }
            },
            new MasterDataSourceDto
            {
                SourceType = MasterDataSourceTypes.Api,
                SourceName = "/api/zipcodes/states/{countryCode}",
                DisplayName = "API: States/Provinces",
                AvailableFields = new List<string> { "code", "name" }
            }
        });

        return sources;
    }

    public async Task<List<MasterDataLookupResultDto>> GetMasterDataForFieldAsync(
        int fieldConfigurationId,
        Dictionary<string, string>? dependentValues = null,
        string? searchTerm = null,
        int limit = 100)
    {
        var links = await GetLinksForFieldAsync(fieldConfigurationId);
        if (!links.Any())
            return new List<MasterDataLookupResultDto>();

        var link = links.First(); // Primary link
        var results = new List<MasterDataLookupResultDto>();

        switch (link.SourceType)
        {
            case MasterDataSourceTypes.LookupCategory:
                results = await GetLookupCategoryDataAsync(link, searchTerm, limit);
                break;
            case MasterDataSourceTypes.Table:
                results = await GetTableDataAsync(link, dependentValues, searchTerm, limit);
                break;
        }

        return results;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateValueAsync(
        int fieldConfigurationId,
        string value,
        Dictionary<string, string>? dependentValues = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (true, null); // Empty values handled by required validation

        var links = await GetLinksForFieldAsync(fieldConfigurationId);
        if (!links.Any())
            return (true, null); // No validation configured

        var link = links.First();

        // Check regex pattern validation
        if (!string.IsNullOrEmpty(link.ValidationPattern))
        {
            try
            {
                if (!Regex.IsMatch(value, link.ValidationPattern, RegexOptions.IgnoreCase))
                {
                    return (false, link.ValidationMessage ?? $"Value does not match required pattern");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Invalid regex pattern for field {FieldId}: {Error}", fieldConfigurationId, ex.Message);
            }
        }

        // If free text is allowed, skip master data validation
        if (link.AllowFreeText)
            return (true, null);

        // Check if value exists in master data
        var masterData = await GetMasterDataForFieldAsync(fieldConfigurationId, dependentValues, value, 1);
        if (!masterData.Any(m => m.Value.Equals(value, StringComparison.OrdinalIgnoreCase) || 
                                  m.Display.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, link.ValidationMessage ?? "Value is not in the allowed list");
        }

        return (true, null);
    }

    private async Task<List<MasterDataLookupResultDto>> GetLookupCategoryDataAsync(
        FieldMasterDataLinkDto link, 
        string? searchTerm, 
        int limit)
    {
        var category = await _context.LookupCategories
            .FirstOrDefaultAsync(c => c.Name == link.SourceName && !c.IsDeleted && c.IsActive);

        if (category == null)
            return new List<MasterDataLookupResultDto>();

        var query = _context.LookupItems
            .Where(i => i.LookupCategoryId == category.Id && !i.IsDeleted && i.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Value.Contains(searchTerm) || i.Key.Contains(searchTerm));
        }

        var items = await query
            .OrderBy(i => i.SortOrder)
            .Take(limit)
            .ToListAsync();

        return items.Select(i => new MasterDataLookupResultDto
        {
            Value = link.ValueField == "Key" ? i.Key : i.Value,
            Display = link.DisplayField == "Value" ? i.Value : i.Key,
            Metadata = !string.IsNullOrEmpty(i.Meta) 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(i.Meta) 
                : null
        }).ToList();
    }

    private async Task<List<MasterDataLookupResultDto>> GetTableDataAsync(
        FieldMasterDataLinkDto link,
        Dictionary<string, string>? dependentValues,
        string? searchTerm,
        int limit)
    {
        var results = new List<MasterDataLookupResultDto>();

        switch (link.SourceName)
        {
            case "ZipCodes":
                results = await GetZipCodeDataAsync(link, dependentValues, searchTerm, limit);
                break;
            case "Products":
                results = await GetProductDataAsync(link, searchTerm, limit);
                break;
            case "Customers":
                results = await GetCustomerDataAsync(link, searchTerm, limit);
                break;
            // Add more table handlers as needed
        }

        return results;
    }

    private async Task<List<MasterDataLookupResultDto>> GetZipCodeDataAsync(
        FieldMasterDataLinkDto link,
        Dictionary<string, string>? dependentValues,
        string? searchTerm,
        int limit)
    {
        var query = _context.ZipCodes.Where(z => z.IsActive);

        // Apply dependent filters
        if (dependentValues != null)
        {
            if (dependentValues.TryGetValue("countryCode", out var countryCode) && !string.IsNullOrEmpty(countryCode))
                query = query.Where(z => z.CountryCode == countryCode);
            if (dependentValues.TryGetValue("stateCode", out var stateCode) && !string.IsNullOrEmpty(stateCode))
                query = query.Where(z => z.StateCode == stateCode);
            if (dependentValues.TryGetValue("city", out var city) && !string.IsNullOrEmpty(city))
                query = query.Where(z => z.City == city);
        }

        // Apply static filter expression
        if (!string.IsNullOrEmpty(link.FilterExpression))
        {
            try
            {
                var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(link.FilterExpression);
                if (filters != null)
                {
                    if (filters.TryGetValue("CountryCode", out var cc))
                        query = query.Where(z => z.CountryCode == cc);
                    if (filters.TryGetValue("StateCode", out var sc))
                        query = query.Where(z => z.StateCode == sc);
                }
            }
            catch { /* Ignore invalid filter */ }
        }

        // Apply search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(z => 
                z.PostalCode.Contains(searchTerm) ||
                z.City.Contains(searchTerm) ||
                z.State.Contains(searchTerm));
        }

        var data = await query.Take(limit).ToListAsync();

        return data.Select(z => new MasterDataLookupResultDto
        {
            Value = GetPropertyValue(z, link.ValueField) ?? z.PostalCode,
            Display = GetPropertyValue(z, link.DisplayField) ?? $"{z.City}, {z.State} {z.PostalCode}",
            Metadata = new Dictionary<string, object>
            {
                { "city", z.City },
                { "state", z.State },
                { "stateCode", z.StateCode ?? "" },
                { "country", z.Country },
                { "countryCode", z.CountryCode }
            }
        }).ToList();
    }

    private async Task<List<MasterDataLookupResultDto>> GetProductDataAsync(
        FieldMasterDataLinkDto link,
        string? searchTerm,
        int limit)
    {
        var query = _context.Products.Where(p => !p.IsDeleted && p.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || (p.ProductCode != null && p.ProductCode.Contains(searchTerm)));
        }

        var data = await query.Take(limit).ToListAsync();

        return data.Select(p => new MasterDataLookupResultDto
        {
            Value = GetPropertyValue(p, link.ValueField) ?? p.Id.ToString(),
            Display = GetPropertyValue(p, link.DisplayField) ?? p.Name,
            Metadata = new Dictionary<string, object>
            {
                { "productCode", p.ProductCode ?? "" },
                { "category", p.Category ?? "" },
                { "price", p.Price }
            }
        }).ToList();
    }

    private async Task<List<MasterDataLookupResultDto>> GetCustomerDataAsync(
        FieldMasterDataLinkDto link,
        string? searchTerm,
        int limit)
    {
        var query = _context.Customers.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Company.Contains(searchTerm) || c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm));
        }

        var data = await query.Take(limit).ToListAsync();

        return data.Select(c => new MasterDataLookupResultDto
        {
            Value = GetPropertyValue(c, link.ValueField) ?? c.Id.ToString(),
            Display = GetPropertyValue(c, link.DisplayField) ?? (string.IsNullOrEmpty(c.Company) ? $"{c.FirstName} {c.LastName}" : c.Company),
            Metadata = new Dictionary<string, object>
            {
                { "company", c.Company ?? "" },
                { "industry", c.Industry ?? "" }
            }
        }).ToList();
    }

    private static string? GetPropertyValue(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName);
        return prop?.GetValue(obj)?.ToString();
    }

    private static FieldMasterDataLinkDto MapToDto(FieldMasterDataLink link) => new()
    {
        Id = link.Id,
        FieldConfigurationId = link.FieldConfigurationId,
        SourceType = link.SourceType,
        SourceName = link.SourceName,
        DisplayField = link.DisplayField,
        ValueField = link.ValueField,
        FilterExpression = link.FilterExpression,
        DependsOnField = link.DependsOnField,
        DependsOnSourceColumn = link.DependsOnSourceColumn,
        AllowFreeText = link.AllowFreeText,
        ValidationType = link.ValidationType,
        ValidationPattern = link.ValidationPattern,
        ValidationMessage = link.ValidationMessage,
        SortOrder = link.SortOrder,
        IsActive = link.IsActive
    };
}

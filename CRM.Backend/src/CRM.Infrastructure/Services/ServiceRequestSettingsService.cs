/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing service request categories
/// </summary>
public class ServiceRequestCategoryService : IServiceRequestCategoryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ServiceRequestCategoryService> _logger;

    public ServiceRequestCategoryService(ICrmDbContext context, ILogger<ServiceRequestCategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ServiceRequestCategoryDto>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        var query = _context.ServiceRequestCategories
            .Include(c => c.Subcategories)
            .Include(c => c.ServiceRequests)
            .Where(c => !c.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new ServiceRequestCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            IconName = c.IconName,
            ColorCode = c.ColorCode,
            DefaultResponseTimeHours = c.DefaultResponseTimeHours,
            DefaultResolutionTimeHours = c.DefaultResolutionTimeHours,
            SubcategoryCount = c.Subcategories?.Count(s => !s.IsDeleted && s.IsActive) ?? 0,
            ServiceRequestCount = c.ServiceRequests?.Count(sr => !sr.IsDeleted) ?? 0,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }

    public async Task<ServiceRequestCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.ServiceRequestCategories
            .Include(c => c.Subcategories)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null) return null;

        return new ServiceRequestCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            IconName = category.IconName,
            ColorCode = category.ColorCode,
            DefaultResponseTimeHours = category.DefaultResponseTimeHours,
            DefaultResolutionTimeHours = category.DefaultResolutionTimeHours,
            SubcategoryCount = category.Subcategories?.Count(s => !s.IsDeleted && s.IsActive) ?? 0,
            ServiceRequestCount = category.ServiceRequests?.Count(sr => !sr.IsDeleted) ?? 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    public async Task<ServiceRequestCategoryDto> CreateCategoryAsync(CreateServiceRequestCategoryDto dto)
    {
        var category = new ServiceRequestCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            IconName = dto.IconName,
            ColorCode = dto.ColorCode,
            DefaultResponseTimeHours = dto.DefaultResponseTimeHours,
            DefaultResolutionTimeHours = dto.DefaultResolutionTimeHours,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequestCategories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created service request category: {Name}", category.Name);

        return await GetCategoryByIdAsync(category.Id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestCategoryDto> UpdateCategoryAsync(int id, UpdateServiceRequestCategoryDto dto)
    {
        var category = await _context.ServiceRequestCategories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;
        category.IconName = dto.IconName;
        category.ColorCode = dto.ColorCode;
        category.DefaultResponseTimeHours = dto.DefaultResponseTimeHours;
        category.DefaultResolutionTimeHours = dto.DefaultResolutionTimeHours;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated service request category: {Name}", category.Name);

        return await GetCategoryByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.ServiceRequestCategories.FindAsync(id);
        if (category == null) return false;

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted service request category: {Name}", category.Name);
        return true;
    }

    public async Task<bool> ReorderCategoriesAsync(List<int> categoryIds)
    {
        for (int i = 0; i < categoryIds.Count; i++)
        {
            var category = await _context.ServiceRequestCategories.FindAsync(categoryIds[i]);
            if (category != null)
            {
                category.DisplayOrder = i;
                category.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// Service implementation for managing service request subcategories
/// </summary>
public class ServiceRequestSubcategoryService : IServiceRequestSubcategoryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ServiceRequestSubcategoryService> _logger;

    public ServiceRequestSubcategoryService(ICrmDbContext context, ILogger<ServiceRequestSubcategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ServiceRequestSubcategoryDto>> GetAllSubcategoriesAsync(bool includeInactive = false)
    {
        var query = _context.ServiceRequestSubcategories
            .Include(s => s.Category)
            .Include(s => s.DefaultWorkflow)
            .Include(s => s.ServiceRequests)
            .Where(s => !s.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var subcategories = await query
            .OrderBy(s => s.Category!.DisplayOrder)
            .ThenBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return subcategories.Select(MapToDto).ToList();
    }

    public async Task<List<ServiceRequestSubcategoryDto>> GetSubcategoriesByCategoryAsync(int categoryId, bool includeInactive = false)
    {
        var query = _context.ServiceRequestSubcategories
            .Include(s => s.Category)
            .Include(s => s.DefaultWorkflow)
            .Include(s => s.ServiceRequests)
            .Where(s => s.CategoryId == categoryId && !s.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        var subcategories = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return subcategories.Select(MapToDto).ToList();
    }

    public async Task<ServiceRequestSubcategoryDto?> GetSubcategoryByIdAsync(int id)
    {
        var subcategory = await _context.ServiceRequestSubcategories
            .Include(s => s.Category)
            .Include(s => s.DefaultWorkflow)
            .Include(s => s.ServiceRequests)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return subcategory != null ? MapToDto(subcategory) : null;
    }

    public async Task<ServiceRequestSubcategoryDto> CreateSubcategoryAsync(CreateServiceRequestSubcategoryDto dto)
    {
        var subcategory = new ServiceRequestSubcategory
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            ResponseTimeHours = dto.ResponseTimeHours,
            ResolutionTimeHours = dto.ResolutionTimeHours,
            DefaultPriority = dto.DefaultPriority,
            DefaultWorkflowId = dto.DefaultWorkflowId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequestSubcategories.Add(subcategory);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created service request subcategory: {Name}", subcategory.Name);

        return await GetSubcategoryByIdAsync(subcategory.Id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestSubcategoryDto> UpdateSubcategoryAsync(int id, UpdateServiceRequestSubcategoryDto dto)
    {
        var subcategory = await _context.ServiceRequestSubcategories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Subcategory {id} not found");

        subcategory.Name = dto.Name;
        subcategory.Description = dto.Description;
        subcategory.CategoryId = dto.CategoryId;
        subcategory.DisplayOrder = dto.DisplayOrder;
        subcategory.IsActive = dto.IsActive;
        subcategory.ResponseTimeHours = dto.ResponseTimeHours;
        subcategory.ResolutionTimeHours = dto.ResolutionTimeHours;
        subcategory.DefaultPriority = dto.DefaultPriority;
        subcategory.DefaultWorkflowId = dto.DefaultWorkflowId;
        subcategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated service request subcategory: {Name}", subcategory.Name);

        return await GetSubcategoryByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<bool> DeleteSubcategoryAsync(int id)
    {
        var subcategory = await _context.ServiceRequestSubcategories.FindAsync(id);
        if (subcategory == null) return false;

        subcategory.IsDeleted = true;
        subcategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted service request subcategory: {Name}", subcategory.Name);
        return true;
    }

    public async Task<bool> ReorderSubcategoriesAsync(int categoryId, List<int> subcategoryIds)
    {
        for (int i = 0; i < subcategoryIds.Count; i++)
        {
            var subcategory = await _context.ServiceRequestSubcategories.FindAsync(subcategoryIds[i]);
            if (subcategory != null && subcategory.CategoryId == categoryId)
            {
                subcategory.DisplayOrder = i;
                subcategory.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static ServiceRequestSubcategoryDto MapToDto(ServiceRequestSubcategory s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        CategoryId = s.CategoryId,
        CategoryName = s.Category?.Name,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive,
        ResponseTimeHours = s.ResponseTimeHours,
        ResolutionTimeHours = s.ResolutionTimeHours,
        DefaultPriority = s.DefaultPriority,
        DefaultWorkflowId = s.DefaultWorkflowId,
        DefaultWorkflowName = s.DefaultWorkflow?.Name,
        ServiceRequestCount = s.ServiceRequests?.Count(sr => !sr.IsDeleted) ?? 0,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}

/// <summary>
/// Service implementation for managing service request custom field definitions
/// </summary>
public class ServiceRequestCustomFieldService : IServiceRequestCustomFieldService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ServiceRequestCustomFieldService> _logger;
    private const int MaxCustomFields = 15;

    public ServiceRequestCustomFieldService(ICrmDbContext context, ILogger<ServiceRequestCustomFieldService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ServiceRequestCustomFieldDefinitionDto>> GetAllFieldDefinitionsAsync(bool includeInactive = false)
    {
        var query = _context.ServiceRequestCustomFieldDefinitions
            .Include(f => f.Category)
            .Include(f => f.Subcategory)
            .Where(f => !f.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        var fields = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Name)
            .ToListAsync();

        return fields.Select(MapToDto).ToList();
    }

    public async Task<List<ServiceRequestCustomFieldDefinitionDto>> GetFieldDefinitionsByCategoryAsync(int? categoryId, int? subcategoryId)
    {
        var query = _context.ServiceRequestCustomFieldDefinitions
            .Include(f => f.Category)
            .Include(f => f.Subcategory)
            .Where(f => !f.IsDeleted && f.IsActive);

        // Get fields that apply globally OR to the specific category/subcategory
        query = query.Where(f =>
            (!f.CategoryId.HasValue && !f.SubcategoryId.HasValue) || // Global fields
            (categoryId.HasValue && f.CategoryId == categoryId && !f.SubcategoryId.HasValue) || // Category-specific
            (subcategoryId.HasValue && f.SubcategoryId == subcategoryId) // Subcategory-specific
        );

        var fields = await query
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.Name)
            .ToListAsync();

        return fields.Select(MapToDto).ToList();
    }

    public async Task<ServiceRequestCustomFieldDefinitionDto?> GetFieldDefinitionByIdAsync(int id)
    {
        var field = await _context.ServiceRequestCustomFieldDefinitions
            .Include(f => f.Category)
            .Include(f => f.Subcategory)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        return field != null ? MapToDto(field) : null;
    }

    public async Task<ServiceRequestCustomFieldDefinitionDto> CreateFieldDefinitionAsync(CreateServiceRequestCustomFieldDefinitionDto dto)
    {
        // Check max limit
        var activeCount = await GetActiveFieldCountAsync();
        if (activeCount >= MaxCustomFields)
        {
            throw new InvalidOperationException($"Maximum of {MaxCustomFields} custom fields allowed. Please deactivate or delete existing fields first.");
        }

        var field = new ServiceRequestCustomFieldDefinition
        {
            Name = dto.Name,
            FieldKey = dto.FieldKey,
            Description = dto.Description,
            FieldType = dto.FieldType,
            IsRequired = dto.IsRequired,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            DefaultValue = dto.DefaultValue,
            Placeholder = dto.Placeholder,
            HelpText = dto.HelpText,
            DropdownOptions = dto.DropdownOptions != null ? JsonSerializer.Serialize(dto.DropdownOptions) : null,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            MaxLength = dto.MaxLength,
            ValidationPattern = dto.ValidationPattern,
            ValidationMessage = dto.ValidationMessage,
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequestCustomFieldDefinitions.Add(field);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created custom field definition: {Name} ({FieldKey})", field.Name, field.FieldKey);

        return await GetFieldDefinitionByIdAsync(field.Id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestCustomFieldDefinitionDto> UpdateFieldDefinitionAsync(int id, UpdateServiceRequestCustomFieldDefinitionDto dto)
    {
        var field = await _context.ServiceRequestCustomFieldDefinitions.FindAsync(id)
            ?? throw new KeyNotFoundException($"Custom field {id} not found");

        field.Name = dto.Name;
        field.FieldKey = dto.FieldKey;
        field.Description = dto.Description;
        field.FieldType = dto.FieldType;
        field.IsRequired = dto.IsRequired;
        field.IsActive = dto.IsActive;
        field.DisplayOrder = dto.DisplayOrder;
        field.DefaultValue = dto.DefaultValue;
        field.Placeholder = dto.Placeholder;
        field.HelpText = dto.HelpText;
        field.DropdownOptions = dto.DropdownOptions != null ? JsonSerializer.Serialize(dto.DropdownOptions) : null;
        field.MinValue = dto.MinValue;
        field.MaxValue = dto.MaxValue;
        field.MaxLength = dto.MaxLength;
        field.ValidationPattern = dto.ValidationPattern;
        field.ValidationMessage = dto.ValidationMessage;
        field.CategoryId = dto.CategoryId;
        field.SubcategoryId = dto.SubcategoryId;
        field.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated custom field definition: {Name} ({FieldKey})", field.Name, field.FieldKey);

        return await GetFieldDefinitionByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<bool> DeleteFieldDefinitionAsync(int id)
    {
        var field = await _context.ServiceRequestCustomFieldDefinitions.FindAsync(id);
        if (field == null) return false;

        field.IsDeleted = true;
        field.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted custom field definition: {Name} ({FieldKey})", field.Name, field.FieldKey);
        return true;
    }

    public async Task<bool> ReorderFieldDefinitionsAsync(List<int> fieldIds)
    {
        for (int i = 0; i < fieldIds.Count; i++)
        {
            var field = await _context.ServiceRequestCustomFieldDefinitions.FindAsync(fieldIds[i]);
            if (field != null)
            {
                field.DisplayOrder = i;
                field.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActiveFieldCountAsync()
    {
        return await _context.ServiceRequestCustomFieldDefinitions
            .CountAsync(f => !f.IsDeleted && f.IsActive);
    }

    private static ServiceRequestCustomFieldDefinitionDto MapToDto(ServiceRequestCustomFieldDefinition f)
    {
        List<string>? dropdownOptions = null;
        if (!string.IsNullOrEmpty(f.DropdownOptions))
        {
            try
            {
                dropdownOptions = JsonSerializer.Deserialize<List<string>>(f.DropdownOptions);
            }
            catch { }
        }

        return new ServiceRequestCustomFieldDefinitionDto
        {
            Id = f.Id,
            Name = f.Name,
            FieldKey = f.FieldKey,
            Description = f.Description,
            FieldType = f.FieldType,
            IsRequired = f.IsRequired,
            IsActive = f.IsActive,
            DisplayOrder = f.DisplayOrder,
            DefaultValue = f.DefaultValue,
            Placeholder = f.Placeholder,
            HelpText = f.HelpText,
            DropdownOptions = f.DropdownOptions,
            DropdownOptionsList = dropdownOptions,
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            MaxLength = f.MaxLength,
            ValidationPattern = f.ValidationPattern,
            ValidationMessage = f.ValidationMessage,
            CategoryId = f.CategoryId,
            CategoryName = f.Category?.Name,
            SubcategoryId = f.SubcategoryId,
            SubcategoryName = f.Subcategory?.Name,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        };
    }
}

/// <summary>
/// Service implementation for managing service request types
/// </summary>
public class ServiceRequestTypeService : IServiceRequestTypeService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ServiceRequestTypeService> _logger;

    public ServiceRequestTypeService(ICrmDbContext context, ILogger<ServiceRequestTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ServiceRequestTypeDto>> GetAllTypesAsync(bool includeInactive = false)
    {
        var query = _context.ServiceRequestTypes
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Where(t => !t.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var types = await query
            .OrderBy(t => t.Category!.Name)
            .ThenBy(t => t.Subcategory!.Name)
            .ThenBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return types.Select(MapToDto).ToList();
    }

    public async Task<List<ServiceRequestTypeDto>> GetTypesBySubcategoryAsync(int subcategoryId, bool includeInactive = false)
    {
        var query = _context.ServiceRequestTypes
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Where(t => !t.IsDeleted && t.SubcategoryId == subcategoryId);

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var types = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return types.Select(MapToDto).ToList();
    }

    public async Task<List<ServiceRequestTypeDto>> GetTypesByCategoryAsync(int categoryId, bool includeInactive = false)
    {
        var query = _context.ServiceRequestTypes
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Where(t => !t.IsDeleted && t.CategoryId == categoryId);

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var types = await query
            .OrderBy(t => t.Subcategory!.Name)
            .ThenBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return types.Select(MapToDto).ToList();
    }

    public async Task<List<ServiceRequestTypeGroupedDto>> GetTypesGroupedAsync(bool includeInactive = false)
    {
        var types = await GetAllTypesAsync(includeInactive);

        return types
            .GroupBy(t => new { t.CategoryId, t.CategoryName })
            .Select(catGroup => new ServiceRequestTypeGroupedDto
            {
                CategoryId = catGroup.Key.CategoryId,
                CategoryName = catGroup.Key.CategoryName ?? "",
                Subcategories = catGroup
                    .GroupBy(t => new { t.SubcategoryId, t.SubcategoryName })
                    .Select(subGroup => new SubcategoryWithTypesDto
                    {
                        SubcategoryId = subGroup.Key.SubcategoryId,
                        SubcategoryName = subGroup.Key.SubcategoryName ?? "",
                        Types = subGroup.OrderBy(t => t.DisplayOrder).ThenBy(t => t.Name).ToList()
                    })
                    .OrderBy(s => s.SubcategoryName)
                    .ToList()
            })
            .OrderBy(c => c.CategoryName)
            .ToList();
    }

    public async Task<ServiceRequestTypeDto?> GetTypeByIdAsync(int id)
    {
        var type = await _context.ServiceRequestTypes
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return type != null ? MapToDto(type) : null;
    }

    public async Task<ServiceRequestTypeDto> CreateTypeAsync(CreateServiceRequestTypeDto dto)
    {
        var type = new ServiceRequestType
        {
            Name = dto.Name,
            RequestType = dto.RequestType,
            DetailedDescription = dto.DetailedDescription,
            WorkflowName = dto.WorkflowName,
            PossibleResolutions = dto.PossibleResolutions,
            FinalCustomerResolutions = dto.FinalCustomerResolutions,
            CategoryId = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            DefaultPriority = dto.DefaultPriority,
            ResponseTimeHours = dto.ResponseTimeHours,
            ResolutionTimeHours = dto.ResolutionTimeHours,
            Tags = dto.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequestTypes.Add(type);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created service request type: {Name}", type.Name);

        return await GetTypeByIdAsync(type.Id) ?? throw new InvalidOperationException();
    }

    public async Task<ServiceRequestTypeDto> UpdateTypeAsync(int id, UpdateServiceRequestTypeDto dto)
    {
        var type = await _context.ServiceRequestTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Service request type {id} not found");

        type.Name = dto.Name;
        type.RequestType = dto.RequestType;
        type.DetailedDescription = dto.DetailedDescription;
        type.WorkflowName = dto.WorkflowName;
        type.PossibleResolutions = dto.PossibleResolutions;
        type.FinalCustomerResolutions = dto.FinalCustomerResolutions;
        type.CategoryId = dto.CategoryId;
        type.SubcategoryId = dto.SubcategoryId;
        type.DisplayOrder = dto.DisplayOrder;
        type.IsActive = dto.IsActive;
        type.DefaultPriority = dto.DefaultPriority;
        type.ResponseTimeHours = dto.ResponseTimeHours;
        type.ResolutionTimeHours = dto.ResolutionTimeHours;
        type.Tags = dto.Tags;
        type.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated service request type: {Name}", type.Name);

        return await GetTypeByIdAsync(id) ?? throw new InvalidOperationException();
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        var type = await _context.ServiceRequestTypes.FindAsync(id);
        if (type == null) return false;

        type.IsDeleted = true;
        type.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted service request type: {Id}", id);
        return true;
    }

    public async Task<bool> ReorderTypesAsync(int subcategoryId, List<int> typeIds)
    {
        for (int i = 0; i < typeIds.Count; i++)
        {
            var type = await _context.ServiceRequestTypes
                .FirstOrDefaultAsync(t => t.Id == typeIds[i] && t.SubcategoryId == subcategoryId);
            if (type != null)
            {
                type.DisplayOrder = i;
                type.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static ServiceRequestTypeDto MapToDto(ServiceRequestType t)
    {
        return new ServiceRequestTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            RequestType = t.RequestType,
            DetailedDescription = t.DetailedDescription,
            WorkflowName = t.WorkflowName,
            PossibleResolutions = t.PossibleResolutions,
            FinalCustomerResolutions = t.FinalCustomerResolutions,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name,
            SubcategoryId = t.SubcategoryId,
            SubcategoryName = t.Subcategory?.Name,
            DisplayOrder = t.DisplayOrder,
            IsActive = t.IsActive,
            DefaultPriority = t.DefaultPriority,
            ResponseTimeHours = t.ResponseTimeHours,
            ResolutionTimeHours = t.ResolutionTimeHours,
            Tags = t.Tags,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}

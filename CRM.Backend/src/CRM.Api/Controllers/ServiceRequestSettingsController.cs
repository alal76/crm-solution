// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing service request settings (categories, subcategories, types, custom fields)
/// </summary>
[ApiController]
[Route("api/service-request-settings")]
[Authorize]
public class ServiceRequestSettingsController : CrmControllerBase
{
    private readonly IServiceRequestCategoryService _categoryService;
    private readonly IServiceRequestSubcategoryService _subcategoryService;
    private readonly IServiceRequestCustomFieldService _customFieldService;
    private readonly IServiceRequestTypeService _typeService;
    private readonly ILogger<ServiceRequestSettingsController> _logger;

    public ServiceRequestSettingsController(
        IServiceRequestCategoryService categoryService,
        IServiceRequestSubcategoryService subcategoryService,
        IServiceRequestCustomFieldService customFieldService,
        IServiceRequestTypeService typeService,
        ILogger<ServiceRequestSettingsController> logger)
    {
        _categoryService = categoryService;
        _subcategoryService = subcategoryService;
        _customFieldService = customFieldService;
        _typeService = typeService;
        _logger = logger;
    }

    #region Categories

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<ServiceRequestCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestCategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
    {
                var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);
        return Ok(categories);
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    [ProducesResponseType(typeof(ServiceRequestCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestCategoryDto>> GetCategory(int id)
    {
                var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound($"Category {id} not found");
        }
        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(ServiceRequestCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestCategoryDto>> CreateCategory([FromBody] CreateServiceRequestCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogWarning(dbEx, "Duplicate or constraint violation creating category {Name}", dto.Name);
            return Conflict(new { message = $"Category '{dto.Name}' already exists or violates a constraint." });
        }
        catch (InvalidOperationException ioEx)
        {
            _logger.LogWarning(ioEx, "Category already exists: {Name}", dto.Name);
            return Conflict(new { message = $"Category '{dto.Name}' already exists." });
        }
    }

    /// <summary>
    /// Update a category
    /// </summary>
    [HttpPut("categories/{id}")]
    [ProducesResponseType(typeof(ServiceRequestCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestCategoryDto>> UpdateCategory(int id, [FromBody] UpdateServiceRequestCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, dto);
            return Ok(category);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Category {id} not found");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
                var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result)
        {
            return NotFound($"Category {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder categories
    /// </summary>
    [HttpPost("categories/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderCategories([FromBody] List<int> categoryIds)
    {
                await _categoryService.ReorderCategoriesAsync(categoryIds);
        return Ok();
    }

    #endregion

    #region Subcategories

    /// <summary>
    /// Get all subcategories
    /// </summary>
    [HttpGet("subcategories")]
    [ProducesResponseType(typeof(List<ServiceRequestSubcategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestSubcategoryDto>>> GetSubcategories([FromQuery] bool includeInactive = false)
    {
                var subcategories = await _subcategoryService.GetAllSubcategoriesAsync(includeInactive);
        return Ok(subcategories);
    }

    /// <summary>
    /// Get subcategories by category
    /// </summary>
    [HttpGet("categories/{categoryId}/subcategories")]
    [ProducesResponseType(typeof(List<ServiceRequestSubcategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestSubcategoryDto>>> GetSubcategoriesByCategory(
        int categoryId, [FromQuery] bool includeInactive = false)
    {
                var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(categoryId, includeInactive);
        return Ok(subcategories);
    }

    /// <summary>
    /// Get a subcategory by ID
    /// </summary>
    [HttpGet("subcategories/{id}")]
    [ProducesResponseType(typeof(ServiceRequestSubcategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestSubcategoryDto>> GetSubcategory(int id)
    {
                var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(id);
        if (subcategory == null)
        {
            return NotFound($"Subcategory {id} not found");
        }
        return Ok(subcategory);
    }

    /// <summary>
    /// Create a new subcategory
    /// </summary>
    [HttpPost("subcategories")]
    [ProducesResponseType(typeof(ServiceRequestSubcategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestSubcategoryDto>> CreateSubcategory([FromBody] CreateServiceRequestSubcategoryDto dto)
    {
                var subcategory = await _subcategoryService.CreateSubcategoryAsync(dto);
        return CreatedAtAction(nameof(GetSubcategory), new { id = subcategory.Id }, subcategory);
    }

    /// <summary>
    /// Update a subcategory
    /// </summary>
    [HttpPut("subcategories/{id}")]
    [ProducesResponseType(typeof(ServiceRequestSubcategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestSubcategoryDto>> UpdateSubcategory(int id, [FromBody] UpdateServiceRequestSubcategoryDto dto)
    {
        try
        {
            var subcategory = await _subcategoryService.UpdateSubcategoryAsync(id, dto);
            return Ok(subcategory);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Subcategory {id} not found");
        }
    }

    /// <summary>
    /// Delete a subcategory
    /// </summary>
    [HttpDelete("subcategories/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubcategory(int id)
    {
                var result = await _subcategoryService.DeleteSubcategoryAsync(id);
        if (!result)
        {
            return NotFound($"Subcategory {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder subcategories within a category
    /// </summary>
    [HttpPost("categories/{categoryId}/subcategories/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderSubcategories(int categoryId, [FromBody] List<int> subcategoryIds)
    {
                await _subcategoryService.ReorderSubcategoriesAsync(categoryId, subcategoryIds);
        return Ok();
    }

    #endregion

    #region Custom Fields

    /// <summary>
    /// Get all custom field definitions
    /// </summary>
    [HttpGet("custom-fields")]
    [ProducesResponseType(typeof(List<ServiceRequestCustomFieldDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestCustomFieldDefinitionDto>>> GetCustomFields([FromQuery] bool includeInactive = false)
    {
                var fields = await _customFieldService.GetAllFieldDefinitionsAsync(includeInactive);
        return Ok(fields);
    }

    /// <summary>
    /// Get custom fields applicable to a category/subcategory
    /// </summary>
    [HttpGet("custom-fields/applicable")]
    [ProducesResponseType(typeof(List<ServiceRequestCustomFieldDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestCustomFieldDefinitionDto>>> GetApplicableCustomFields(
        [FromQuery] int? categoryId, [FromQuery] int? subcategoryId)
    {
                var fields = await _customFieldService.GetFieldDefinitionsByCategoryAsync(categoryId, subcategoryId);
        return Ok(fields);
    }

    /// <summary>
    /// Get a custom field by ID
    /// </summary>
    [HttpGet("custom-fields/{id}")]
    [ProducesResponseType(typeof(ServiceRequestCustomFieldDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestCustomFieldDefinitionDto>> GetCustomField(int id)
    {
                var field = await _customFieldService.GetFieldDefinitionByIdAsync(id);
        if (field == null)
        {
            return NotFound($"Custom field {id} not found");
        }
        return Ok(field);
    }

    /// <summary>
    /// Create a new custom field
    /// </summary>
    [HttpPost("custom-fields")]
    [ProducesResponseType(typeof(ServiceRequestCustomFieldDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestCustomFieldDefinitionDto>> CreateCustomField(
        [FromBody] CreateServiceRequestCustomFieldDefinitionDto dto)
    {
        try
        {
            var field = await _customFieldService.CreateFieldDefinitionAsync(dto);
            return CreatedAtAction(nameof(GetCustomField), new { id = field.Id }, field);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a custom field
    /// </summary>
    [HttpPut("custom-fields/{id}")]
    [ProducesResponseType(typeof(ServiceRequestCustomFieldDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestCustomFieldDefinitionDto>> UpdateCustomField(
        int id, [FromBody] UpdateServiceRequestCustomFieldDefinitionDto dto)
    {
        try
        {
            var field = await _customFieldService.UpdateFieldDefinitionAsync(id, dto);
            return Ok(field);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Custom field {id} not found");
        }
    }

    /// <summary>
    /// Delete a custom field
    /// </summary>
    [HttpDelete("custom-fields/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomField(int id)
    {
                var result = await _customFieldService.DeleteFieldDefinitionAsync(id);
        if (!result)
        {
            return NotFound($"Custom field {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder custom fields
    /// </summary>
    [HttpPost("custom-fields/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderCustomFields([FromBody] List<int> fieldIds)
    {
                await _customFieldService.ReorderFieldDefinitionsAsync(fieldIds);
        return Ok();
    }

    /// <summary>
    /// Get active custom field count
    /// </summary>
    [HttpGet("custom-fields/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetCustomFieldCount()
    {
                var count = await _customFieldService.GetActiveFieldCountAsync();
        return Ok(new { activeCount = count, maxAllowed = 15 });
    }

    #endregion

    #region Service Request Types

    /// <summary>
    /// Get all service request types
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(List<ServiceRequestTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetAllTypes([FromQuery] bool includeInactive = false)
    {
                var types = await _typeService.GetAllTypesAsync(includeInactive);
        return Ok(types);
    }

    /// <summary>
    /// Get all service request types grouped by category and subcategory
    /// </summary>
    [HttpGet("types/grouped")]
    [ProducesResponseType(typeof(List<ServiceRequestTypeGroupedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestTypeGroupedDto>>> GetTypesGrouped([FromQuery] bool includeInactive = false)
    {
                var grouped = await _typeService.GetTypesGroupedAsync(includeInactive);
        return Ok(grouped);
    }

    /// <summary>
    /// Get service request types by category
    /// </summary>
    [HttpGet("types/by-category/{categoryId}")]
    [ProducesResponseType(typeof(List<ServiceRequestTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetTypesByCategory(int categoryId, [FromQuery] bool includeInactive = false)
    {
                var types = await _typeService.GetTypesByCategoryAsync(categoryId, includeInactive);
        return Ok(types);
    }

    /// <summary>
    /// Get service request types by subcategory
    /// </summary>
    [HttpGet("types/by-subcategory/{subcategoryId}")]
    [ProducesResponseType(typeof(List<ServiceRequestTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetTypesBySubcategory(int subcategoryId, [FromQuery] bool includeInactive = false)
    {
                var types = await _typeService.GetTypesBySubcategoryAsync(subcategoryId, includeInactive);
        return Ok(types);
    }

    /// <summary>
    /// Get a single service request type by ID
    /// </summary>
    [HttpGet("types/{id}")]
    [ProducesResponseType(typeof(ServiceRequestTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestTypeDto>> GetTypeById(int id)
    {
                var type = await _typeService.GetTypeByIdAsync(id);
        if (type == null)
        {
            return NotFound($"Service request type {id} not found");
        }
        return Ok(type);
    }

    /// <summary>
    /// Create a new service request type
    /// </summary>
    [HttpPost("types")]
    [ProducesResponseType(typeof(ServiceRequestTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestTypeDto>> CreateType([FromBody] CreateServiceRequestTypeDto dto)
    {
        try
        {
            var type = await _typeService.CreateTypeAsync(dto);
            return CreatedAtAction(nameof(GetTypeById), new { id = type.Id }, type);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            _logger.LogWarning(dbEx, "Duplicate or constraint violation creating type {Name}", dto.Name);
            return Conflict(new { message = $"Type '{dto.Name}' already exists or violates a constraint." });
        }
    }

    /// <summary>
    /// Update an existing service request type
    /// </summary>
    [HttpPut("types/{id}")]
    [ProducesResponseType(typeof(ServiceRequestTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequestTypeDto>> UpdateType(int id, [FromBody] UpdateServiceRequestTypeDto dto)
    {
        try
        {
            var type = await _typeService.UpdateTypeAsync(id, dto);
            return Ok(type);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a service request type
    /// </summary>
    [HttpDelete("types/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteType(int id)
    {
                var result = await _typeService.DeleteTypeAsync(id);
        if (!result)
        {
            return NotFound($"Service request type {id} not found");
        }
        return NoContent();
    }

    /// <summary>
    /// Reorder service request types within a subcategory
    /// </summary>
    [HttpPost("types/reorder/{subcategoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderTypes(int subcategoryId, [FromBody] List<int> typeIds)
    {
                await _typeService.ReorderTypesAsync(subcategoryId, typeIds);
        return Ok();
    }

    #endregion
}

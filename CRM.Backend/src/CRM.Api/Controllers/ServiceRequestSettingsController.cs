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

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing service request settings (categories, subcategories, types, custom fields)
/// </summary>
[ApiController]
[Route("api/service-request-settings")]
[Authorize]
public class ServiceRequestSettingsController : ControllerBase
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
    public async Task<ActionResult<List<ServiceRequestCategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<ActionResult<ServiceRequestCategoryDto>> GetCategory(int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound($"Category {id} not found");
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<ServiceRequestCategoryDto>> CreateCategory([FromBody] CreateServiceRequestCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update a category
    /// </summary>
    [HttpPut("categories/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return NotFound($"Category {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Reorder categories
    /// </summary>
    [HttpPost("categories/reorder")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<int> categoryIds)
    {
        try
        {
            await _categoryService.ReorderCategoriesAsync(categoryIds);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering categories");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Subcategories

    /// <summary>
    /// Get all subcategories
    /// </summary>
    [HttpGet("subcategories")]
    public async Task<ActionResult<List<ServiceRequestSubcategoryDto>>> GetSubcategories([FromQuery] bool includeInactive = false)
    {
        try
        {
            var subcategories = await _subcategoryService.GetAllSubcategoriesAsync(includeInactive);
            return Ok(subcategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get subcategories by category
    /// </summary>
    [HttpGet("categories/{categoryId}/subcategories")]
    public async Task<ActionResult<List<ServiceRequestSubcategoryDto>>> GetSubcategoriesByCategory(
        int categoryId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(categoryId, includeInactive);
            return Ok(subcategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get a subcategory by ID
    /// </summary>
    [HttpGet("subcategories/{id}")]
    public async Task<ActionResult<ServiceRequestSubcategoryDto>> GetSubcategory(int id)
    {
        try
        {
            var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(id);
            if (subcategory == null)
                return NotFound($"Subcategory {id} not found");
            return Ok(subcategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategory {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new subcategory
    /// </summary>
    [HttpPost("subcategories")]
    public async Task<ActionResult<ServiceRequestSubcategoryDto>> CreateSubcategory([FromBody] CreateServiceRequestSubcategoryDto dto)
    {
        try
        {
            var subcategory = await _subcategoryService.CreateSubcategoryAsync(dto);
            return CreatedAtAction(nameof(GetSubcategory), new { id = subcategory.Id }, subcategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subcategory");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Update a subcategory
    /// </summary>
    [HttpPut("subcategories/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subcategory {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a subcategory
    /// </summary>
    [HttpDelete("subcategories/{id}")]
    public async Task<IActionResult> DeleteSubcategory(int id)
    {
        try
        {
            var result = await _subcategoryService.DeleteSubcategoryAsync(id);
            if (!result)
                return NotFound($"Subcategory {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subcategory {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Reorder subcategories within a category
    /// </summary>
    [HttpPost("categories/{categoryId}/subcategories/reorder")]
    public async Task<IActionResult> ReorderSubcategories(int categoryId, [FromBody] List<int> subcategoryIds)
    {
        try
        {
            await _subcategoryService.ReorderSubcategoriesAsync(categoryId, subcategoryIds);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering subcategories");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Custom Fields

    /// <summary>
    /// Get all custom field definitions
    /// </summary>
    [HttpGet("custom-fields")]
    public async Task<ActionResult<List<ServiceRequestCustomFieldDefinitionDto>>> GetCustomFields([FromQuery] bool includeInactive = false)
    {
        try
        {
            var fields = await _customFieldService.GetAllFieldDefinitionsAsync(includeInactive);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom fields");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get custom fields applicable to a category/subcategory
    /// </summary>
    [HttpGet("custom-fields/applicable")]
    public async Task<ActionResult<List<ServiceRequestCustomFieldDefinitionDto>>> GetApplicableCustomFields(
        [FromQuery] int? categoryId, [FromQuery] int? subcategoryId)
    {
        try
        {
            var fields = await _customFieldService.GetFieldDefinitionsByCategoryAsync(categoryId, subcategoryId);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applicable custom fields");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get a custom field by ID
    /// </summary>
    [HttpGet("custom-fields/{id}")]
    public async Task<ActionResult<ServiceRequestCustomFieldDefinitionDto>> GetCustomField(int id)
    {
        try
        {
            var field = await _customFieldService.GetFieldDefinitionByIdAsync(id);
            if (field == null)
                return NotFound($"Custom field {id} not found");
            return Ok(field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom field {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new custom field
    /// </summary>
    [HttpPost("custom-fields")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom field");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Update a custom field
    /// </summary>
    [HttpPut("custom-fields/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating custom field {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a custom field
    /// </summary>
    [HttpDelete("custom-fields/{id}")]
    public async Task<IActionResult> DeleteCustomField(int id)
    {
        try
        {
            var result = await _customFieldService.DeleteFieldDefinitionAsync(id);
            if (!result)
                return NotFound($"Custom field {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting custom field {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Reorder custom fields
    /// </summary>
    [HttpPost("custom-fields/reorder")]
    public async Task<IActionResult> ReorderCustomFields([FromBody] List<int> fieldIds)
    {
        try
        {
            await _customFieldService.ReorderFieldDefinitionsAsync(fieldIds);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering custom fields");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get active custom field count
    /// </summary>
    [HttpGet("custom-fields/count")]
    public async Task<ActionResult<object>> GetCustomFieldCount()
    {
        try
        {
            var count = await _customFieldService.GetActiveFieldCountAsync();
            return Ok(new { activeCount = count, maxAllowed = 15 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom field count");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Service Request Types

    /// <summary>
    /// Get all service request types
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetAllTypes([FromQuery] bool includeInactive = false)
    {
        try
        {
            var types = await _typeService.GetAllTypesAsync(includeInactive);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request types");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get all service request types grouped by category and subcategory
    /// </summary>
    [HttpGet("types/grouped")]
    public async Task<ActionResult<List<ServiceRequestTypeGroupedDto>>> GetTypesGrouped([FromQuery] bool includeInactive = false)
    {
        try
        {
            var grouped = await _typeService.GetTypesGroupedAsync(includeInactive);
            return Ok(grouped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grouped service request types");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get service request types by category
    /// </summary>
    [HttpGet("types/by-category/{categoryId}")]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetTypesByCategory(int categoryId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var types = await _typeService.GetTypesByCategoryAsync(categoryId, includeInactive);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting types for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get service request types by subcategory
    /// </summary>
    [HttpGet("types/by-subcategory/{subcategoryId}")]
    public async Task<ActionResult<List<ServiceRequestTypeDto>>> GetTypesBySubcategory(int subcategoryId, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var types = await _typeService.GetTypesBySubcategoryAsync(subcategoryId, includeInactive);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting types for subcategory {SubcategoryId}", subcategoryId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get a single service request type by ID
    /// </summary>
    [HttpGet("types/{id}")]
    public async Task<ActionResult<ServiceRequestTypeDto>> GetTypeById(int id)
    {
        try
        {
            var type = await _typeService.GetTypeByIdAsync(id);
            if (type == null)
                return NotFound($"Service request type {id} not found");
            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service request type {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new service request type
    /// </summary>
    [HttpPost("types")]
    public async Task<ActionResult<ServiceRequestTypeDto>> CreateType([FromBody] CreateServiceRequestTypeDto dto)
    {
        try
        {
            var type = await _typeService.CreateTypeAsync(dto);
            return CreatedAtAction(nameof(GetTypeById), new { id = type.Id }, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service request type");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Update an existing service request type
    /// </summary>
    [HttpPut("types/{id}")]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service request type {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a service request type
    /// </summary>
    [HttpDelete("types/{id}")]
    public async Task<IActionResult> DeleteType(int id)
    {
        try
        {
            var result = await _typeService.DeleteTypeAsync(id);
            if (!result)
                return NotFound($"Service request type {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service request type {Id}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Reorder service request types within a subcategory
    /// </summary>
    [HttpPost("types/reorder/{subcategoryId}")]
    public async Task<IActionResult> ReorderTypes(int subcategoryId, [FromBody] List<int> typeIds)
    {
        try
        {
            await _typeService.ReorderTypesAsync(subcategoryId, typeIds);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering types for subcategory {SubcategoryId}", subcategoryId);
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion
}

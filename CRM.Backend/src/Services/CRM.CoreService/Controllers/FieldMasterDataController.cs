using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Controller for managing field-to-master-data links
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FieldMasterDataController : ControllerBase
{
    private readonly IFieldMasterDataService _service;
    private readonly ILogger<FieldMasterDataController> _logger;

    public FieldMasterDataController(IFieldMasterDataService service, ILogger<FieldMasterDataController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get all master data links for a specific field
    /// </summary>
    [HttpGet("field/{fieldConfigurationId}")]
    public async Task<ActionResult<List<FieldMasterDataLinkDto>>> GetLinksForField(int fieldConfigurationId)
    {
        var links = await _service.GetLinksForFieldAsync(fieldConfigurationId);
        return Ok(links);
    }

    /// <summary>
    /// Get all master data links for a module
    /// </summary>
    [HttpGet("module/{moduleName}")]
    public async Task<ActionResult<Dictionary<int, List<FieldMasterDataLinkDto>>>> GetLinksForModule(string moduleName)
    {
        var links = await _service.GetLinksForModuleAsync(moduleName);
        return Ok(links);
    }

    /// <summary>
    /// Get a specific master data link by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FieldMasterDataLinkDto>> GetLinkById(int id)
    {
        var link = await _service.GetLinkByIdAsync(id);
        if (link == null)
            return NotFound();
        return Ok(link);
    }

    /// <summary>
    /// Create a new master data link
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FieldMasterDataLinkDto>> CreateLink([FromBody] CreateFieldMasterDataLinkDto dto)
    {
        try
        {
            var link = await _service.CreateLinkAsync(dto);
            return CreatedAtAction(nameof(GetLinkById), new { id = link.Id }, link);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating master data link for field {FieldId}", dto.FieldConfigurationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing master data link
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FieldMasterDataLinkDto>> UpdateLink(int id, [FromBody] CreateFieldMasterDataLinkDto dto)
    {
        try
        {
            var link = await _service.UpdateLinkAsync(id, dto);
            return Ok(link);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating master data link {LinkId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a master data link
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLink(int id)
    {
        var result = await _service.DeleteLinkAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Get all available master data sources
    /// </summary>
    [HttpGet("sources")]
    public async Task<ActionResult<List<MasterDataSourceDto>>> GetAvailableSources()
    {
        var sources = await _service.GetAvailableSourcesAsync();
        return Ok(sources);
    }

    /// <summary>
    /// Get master data lookup values for a field
    /// </summary>
    [HttpGet("lookup/{fieldConfigurationId}")]
    public async Task<ActionResult<List<MasterDataLookupResultDto>>> GetLookupData(
        int fieldConfigurationId,
        [FromQuery] string? search = null,
        [FromQuery] int limit = 100)
    {
        // Parse dependent values from query string
        var dependentValues = new Dictionary<string, string>();
        foreach (var key in Request.Query.Keys)
        {
            if (key != "search" && key != "limit" && !string.IsNullOrEmpty(Request.Query[key]))
            {
                dependentValues[key] = Request.Query[key]!;
            }
        }

        var data = await _service.GetMasterDataForFieldAsync(
            fieldConfigurationId, 
            dependentValues.Any() ? dependentValues : null,
            search,
            limit);
        return Ok(data);
    }

    /// <summary>
    /// Validate a value against field's master data constraints
    /// </summary>
    [HttpPost("validate/{fieldConfigurationId}")]
    public async Task<ActionResult<ValidationResultDto>> ValidateValue(
        int fieldConfigurationId,
        [FromBody] ValidateValueRequestDto request)
    {
        var (isValid, errorMessage) = await _service.ValidateValueAsync(
            fieldConfigurationId,
            request.Value,
            request.DependentValues);

        return Ok(new ValidationResultDto
        {
            IsValid = isValid,
            ErrorMessage = errorMessage
        });
    }
}

/// <summary>
/// Request DTO for validation
/// </summary>
public class ValidateValueRequestDto
{
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, string>? DependentValues { get; set; }
}

/// <summary>
/// Response DTO for validation
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing ITSM incident categories.
/// </summary>
[ApiController]
[Route("api/incident-categories")]
[Authorize]
[Produces("application/json")]
public class IncidentCategoriesController : ControllerBase
{
    private readonly IIncidentCategoryService _service;
    private readonly ILogger<IncidentCategoriesController> _logger;

    public IncidentCategoriesController(IIncidentCategoryService service, ILogger<IncidentCategoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(IncidentCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentCategoryDto>> Create([FromBody] CreateIncidentCategoryDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident category");
            return StatusCode(500, new { error = "Failed to create incident category" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentCategoryDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<IncidentCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IncidentCategoryDto>>> GetAll([FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(includeInactive, ct);
        return Ok(result);
    }
}

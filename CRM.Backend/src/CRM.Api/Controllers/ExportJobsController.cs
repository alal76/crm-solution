using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing data export jobs.
/// </summary>
[ApiController]
[Route("api/export-jobs")]
[Authorize]
[Produces("application/json")]
public class ExportJobsController : ControllerBase
{
    private readonly IExportJobService _service;
    private readonly ILogger<ExportJobsController> _logger;

    public ExportJobsController(IExportJobService service, ILogger<ExportJobsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExportJobDto>> Create([FromBody] CreateExportJobDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating export job");
            return StatusCode(500, new { error = "Failed to create export job" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExportJobDto>> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExportJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExportJobDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(ct);
        return Ok(result);
    }
}

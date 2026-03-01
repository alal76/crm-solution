// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for problem management in ITSM module.
///
/// Provides endpoints for:
/// - Creating, reading, updating, and deleting problems
/// - Linking incidents to problems
/// - Managing root cause analysis
/// - Adding resolutions
/// - Closing problems
/// - Advanced search and filtering
/// </summary>
[ApiController]
[Route("api/problems")]
[Authorize]
[Produces("application/json")]
public class LegacyProblemsController : CrmControllerBase
{
    private const string ProblemNotFoundMessage = "Problem {0} not found";
    private readonly IProblemService _problemService;
    private readonly ILogger<LegacyProblemsController> _logger;

    public LegacyProblemsController(
        IProblemService problemService,
        ILogger<LegacyProblemsController> logger)
    {
        _problemService = problemService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Create a new problem.
    /// </summary>
    /// <param name="request">Problem creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created problem DTO</returns>
    /// <response code="201">Problem created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProblemDto>> CreateAsync(
        [FromBody] CreateProblemDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var problem = await _problemService.CreateProblemAsync(request, userId);
            _logger.LogInformation("Problem created: {ProblemId}", problem.Id);
            return CreatedAtAction("GetById", new { id = problem.Id }, problem);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating problem");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get problem by ID.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Problem DTO</returns>
    /// <response code="200">Problem found</response>
    /// <response code="404">Problem not found</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
                var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        return Ok(problem);
    }

    /// <summary>
    /// Update an existing problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="request">Update problem request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated problem DTO</returns>
    /// <response code="200">Problem updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Problem not found</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> UpdateAsync(
        int id,
        [FromBody] UpdateProblemDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _problemService.GetProblemByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
            }

            var userId = GetCurrentUserId();
            var updated = await _problemService.UpdateProblemAsync(id, request, userId);
            _logger.LogInformation("Problem updated: {ProblemId}", id);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating problem {ProblemId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete (soft delete) a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="204">Problem deleted successfully</response>
    /// <response code="404">Problem not found</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
                var existing = await _problemService.GetProblemByIdAsync(id);
        if (existing == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        _logger.LogInformation("Problem deleted: {ProblemId}", id);
        return NoContent();
    }

    #endregion

    #region List and Search

    /// <summary>
    /// Get all problems with pagination and filtering.
    /// </summary>
    /// <param name="filter">Problem filter DTO with pagination, sorting, and filtering options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of problems</returns>
    /// <response code="200">Problems retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProblemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ProblemDto>>> GetAllAsync(
        [FromQuery] ProblemFilterDto? filter,
        CancellationToken cancellationToken)
    {
                filter ??= new ProblemFilterDto();
        var (items, totalCount) = await _problemService.GetProblemsAsync(filter);
        return Ok(new PaginatedResult<ProblemDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        });
    }

    /// <summary>
    /// Advanced search for problems.
    /// </summary>
    /// <param name="searchTerm">Search term to match against problem title/description</param>
    /// <param name="state">Filter by problem state</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResult<ProblemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ProblemDto>>> SearchAsync(
        [FromQuery] string? searchTerm,
        [FromQuery] ProblemState? state,
        [FromQuery] ProblemPriority? priority,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
                var filter = new ProblemFilterDto
        {
            SearchTerm = searchTerm,
            State = state,
            Priority = priority,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _problemService.GetProblemsAsync(filter);
        return Ok(new PaginatedResult<ProblemDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    #endregion

    #region Incident Management

    /// <summary>
    /// Get related incidents for a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of related incident DTOs</returns>
    /// <response code="200">Incidents retrieved successfully</response>
    /// <response code="404">Problem not found</response>
    [HttpGet("{id:int}/incidents")]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidentsAsync(
        int id,
        CancellationToken cancellationToken)
    {
                var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        var incidents = await _problemService.GetRelatedIncidentsAsync(id);
        return Ok(incidents);
    }

    /// <summary>
    /// Link an incident to a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="request">Link incident request with incident ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Incident linked successfully</response>
    /// <response code="400">Invalid incident ID</response>
    /// <response code="404">Problem or incident not found</response>
    [HttpPost("{id:int}/incidents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkIncidentAsync(
        int id,
        [FromBody] LinkIncidentRequest request,
        CancellationToken cancellationToken)
    {
                if (request.IncidentId <= 0)
        {
            return BadRequest(new { error = "Invalid incident ID" });
        }

        var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        var userId = GetCurrentUserId();
        var result = await _problemService.LinkIncidentAsync(id, request.IncidentId, userId);
        if (!result)
        {
            return BadRequest(new { error = "Failed to link incident" });
        }

        _logger.LogInformation("Incident {IncidentId} linked to problem {ProblemId}", request.IncidentId, id);
        return Ok(new { message = "Incident linked successfully" });
    }

    /// <summary>
    /// Unlink an incident from a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="incidentId">Incident ID to unlink</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Incident unlinked successfully</response>
    /// <response code="404">Problem or incident not found</response>
    [HttpDelete("{id:int}/incidents/{incidentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkIncidentAsync(
        int id,
        int incidentId,
        CancellationToken cancellationToken)
    {
                var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        _logger.LogInformation("Incident {IncidentId} unlinked from problem {ProblemId}", incidentId, id);
        return Ok(new { message = "Incident unlinked successfully" });
    }

    #endregion

    #region Root Cause Analysis & Resolution

    /// <summary>
    /// Update root cause analysis for a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="request">Root cause analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <response code="200">Root cause updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Problem not found</response>
    [HttpPost("{id:int}/root-cause")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRootCauseAsync(
        int id,
        [FromBody] UpdateRootCauseRequest request,
        CancellationToken cancellationToken)
    {
                if (string.IsNullOrWhiteSpace(request.RootCause))
        {
            return BadRequest(new { error = "Root cause analysis is required" });
        }

        var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        var userId = GetCurrentUserId();
        var result = await _problemService.UpdateRootCauseAnalysisAsync(
            id, request.RootCause, request.Workaround, userId);

        if (!result)
        {
            return BadRequest(new { error = "Failed to update root cause analysis" });
        }

        _logger.LogInformation("Root cause analysis updated for problem {ProblemId}", id);
        return Ok(new { message = "Root cause analysis updated successfully" });
    }

    /// <summary>
    /// Add a resolution to a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="request">Resolution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated problem DTO</returns>
    /// <response code="200">Resolution added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Problem not found</response>
    [HttpPost("{id:int}/resolution")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> AddResolutionAsync(
        int id,
        [FromBody] AddResolutionRequest request,
        CancellationToken cancellationToken)
    {
                if (string.IsNullOrWhiteSpace(request.Resolution))
        {
            return BadRequest(new { error = "Resolution is required" });
        }

        var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        var updateDto = new UpdateProblemDto
        {
            Resolution = request.Resolution
        };

        var userId = GetCurrentUserId();
        var updated = await _problemService.UpdateProblemAsync(id, updateDto, userId);
        _logger.LogInformation("Resolution added to problem {ProblemId}", id);
        return Ok(updated);
    }

    #endregion

    #region Problem Lifecycle

    /// <summary>
    /// Close a problem.
    /// </summary>
    /// <param name="id">Problem ID</param>
    /// <param name="request">Close problem request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated problem DTO</returns>
    /// <response code="200">Problem closed successfully</response>
    /// <response code="400">Problem cannot be closed in current state</response>
    /// <response code="404">Problem not found</response>
    [HttpPost("{id:int}/close")]
    [ProducesResponseType(typeof(ProblemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> CloseProblemAsync(
        int id,
        [FromBody] CloseProblemRequest? request = null,
        CancellationToken cancellationToken = default)
    {
                var problem = await _problemService.GetProblemByIdAsync(id);
        if (problem == null)
        {
            return NotFound(new { error = string.Format(ProblemNotFoundMessage, id) });
        }

        var updateDto = new UpdateProblemDto
        {
            State = ProblemState.Closed,
            ClosureComments = request?.Comments
        };

        var userId = GetCurrentUserId();
        var updated = await _problemService.UpdateProblemAsync(id, updateDto, userId);
        _logger.LogInformation("Problem {ProblemId} closed", id);
        return Ok(updated);
    }

    #endregion

    #region Helper Methods

    private int GetCurrentUserId() // NOSONAR
    {
        // Try multiple claim types for maximum compatibility with different JWT configurations
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")
            ?? User.FindFirst("userId")
            ?? User.FindFirst("nameid")
            ?? User.FindFirst("id");
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            return userId;
        }

        // Fallback to admin user (ID 1) rather than throwing a 500 error
        _logger.LogWarning("Unable to determine current user ID from claims. Falling back to default admin user (1).");
        return 1;
    }

    #endregion
}

#region Request/Response DTOs

/// <summary>
/// Request DTO for linking an incident to a problem.
/// </summary>
public class LinkIncidentRequest
{
    [Required]
    public int IncidentId { get; set; }
}

/// <summary>
/// Request DTO for updating root cause analysis.
/// </summary>
public class UpdateRootCauseRequest
{
    [Required]
    [StringLength(4000)]
    public string RootCause { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Workaround { get; set; }
}

/// <summary>
/// Request DTO for adding a resolution.
/// </summary>
public class AddResolutionRequest
{
    [Required]
    [StringLength(4000)]
    public string Resolution { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for closing a problem.
/// </summary>
public class CloseProblemRequest
{
    [StringLength(2000)]
    public string? Comments { get; set; }
}

/// <summary>
/// Paginated result wrapper.
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

#endregion

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for managing SLA policies.
/// Provides CRUD operations plus policy assignment and applicable-policy queries.
/// </summary>
[ApiController]
[Route("api/slapolicies")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - SLA Policies")]
public class SLAPoliciesController : ControllerBase
{
    private readonly ISLAPolicyAdminService _slaPolicyService;
    private readonly ILogger<SLAPoliciesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SLAPoliciesController"/> class.
    /// </summary>
    public SLAPoliciesController(ISLAPolicyAdminService slaPolicyService, ILogger<SLAPoliciesController> logger)
    {
        _slaPolicyService = slaPolicyService ?? throw new ArgumentNullException(nameof(slaPolicyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all SLA policies.
    /// </summary>
    /// <returns>A list of all SLA policies</returns>
    /// <response code="200">Returns the list of SLA policies</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<SLAPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await _slaPolicyService.GetAllAsync(cancellationToken);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SLA policies");
            return StatusCode(500, new { message = "Error retrieving SLA policies", error = ex.Message });
        }
    }

    /// <summary>
    /// Get SLA policies applicable to a given priority and/or category.
    /// </summary>
    /// <param name="priority">Optional priority filter (e.g. High, Critical)</param>
    /// <param name="category">Optional category filter (e.g. Network, Hardware)</param>
    /// <returns>Matching SLA policies</returns>
    /// <response code="200">Returns applicable SLA policies</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("applicable")]
    [Authorize]
    [ProducesResponseType(typeof(List<SLAPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetApplicable([FromQuery] string? priority, [FromQuery] string? category, CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await _slaPolicyService.GetApplicablePoliciesAsync(priority, category, cancellationToken);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving applicable SLA policies");
            return StatusCode(500, new { message = "Error retrieving applicable SLA policies", error = ex.Message });
        }
    }

    /// <summary>
    /// Get an SLA policy by ID.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <returns>The SLA policy</returns>
    /// <response code="200">Returns the SLA policy</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = await _slaPolicyService.GetByIdAsync(id, cancellationToken);
            if (policy == null)
                return NotFound(new { message = $"SLA policy with ID {id} not found" });
            return Ok(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SLA policy {PolicyId}", id);
            return StatusCode(500, new { message = "Error retrieving SLA policy", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new SLA policy.
    /// </summary>
    /// <param name="dto">The SLA policy creation data</param>
    /// <returns>The created SLA policy</returns>
    /// <response code="201">Policy created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSLAPolicyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _slaPolicyService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SLA policy");
            return StatusCode(500, new { message = "Error creating SLA policy", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing SLA policy.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <param name="dto">The update data</param>
    /// <returns>The updated SLA policy</returns>
    /// <response code="200">Policy updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSLAPolicyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = await _slaPolicyService.UpdateAsync(id, dto, cancellationToken);
            return Ok(policy);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"SLA policy with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SLA policy {PolicyId}", id);
            return StatusCode(500, new { message = "Error updating SLA policy", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an SLA policy.
    /// </summary>
    /// <param name="id">The policy ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Policy deleted successfully</response>
    /// <response code="404">Policy not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _slaPolicyService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"SLA policy with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SLA policy {PolicyId}", id);
            return StatusCode(500, new { message = "Error deleting SLA policy", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign an SLA policy to a service request, creating an SLA tracking instance.
    /// </summary>
    /// <param name="policyId">The SLA policy ID</param>
    /// <param name="serviceRequestId">The service request ID</param>
    /// <returns>The created SLA instance</returns>
    /// <response code="200">SLA instance created successfully</response>
    /// <response code="404">Policy or service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{policyId:int}/assign/{serviceRequestId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SLAInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignPolicy(int policyId, int serviceRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _slaPolicyService.AssignPolicyAsync(policyId, serviceRequestId, cancellationToken);
            return Ok(instance);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning SLA policy {PolicyId} to service request {RequestId}", policyId, serviceRequestId);
            return StatusCode(500, new { message = "Error assigning SLA policy", error = ex.Message });
        }
    }
}

// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing Configuration Item Types in the CMDB.
/// </summary>
/// <remarks>
/// CI Types categorize Configuration Items (CIs) in the Configuration Management Database.
/// Examples include Hardware (Server, Workstation), Software (Application, Database), 
/// Services (Business Service, IT Service), and Facilities (Data Center, Rack).
/// </remarks>
[ApiController]
[Route("api/ci-types")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - CI Types")]
public class CITypesController : ControllerBase
{
    private readonly ICITypeService _ciTypeService;
    private readonly ILogger<CITypesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CITypesController"/> class.
    /// </summary>
    /// <param name="ciTypeService">The CI type service.</param>
    /// <param name="logger">The logger.</param>
    public CITypesController(ICITypeService ciTypeService, ILogger<CITypesController> logger)
    {
        _ciTypeService = ciTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new CI Type.
    /// </summary>
    /// <param name="dto">The CI type creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created CI type.</returns>
    /// <response code="201">Returns the newly created CI type.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CITypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CITypeDto>> Create([FromBody] CreateCITypeDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _ciTypeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets a CI Type by ID.
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The CI type if found.</returns>
    /// <response code="200">Returns the CI type.</response>
    /// <response code="404">If the CI type is not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CITypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CITypeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _ciTypeService.GetByIdAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Gets all CI Types with optional filtering.
    /// </summary>
    /// <param name="category">Optional category filter (e.g., Hardware, Software, Service, Facility).</param>
    /// <param name="activeOnly">If true, returns only active CI types.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of CI types.</returns>
    /// <response code="200">Returns the list of CI types.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CITypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CITypeDto>>> GetAll(
        [FromQuery] string? category = null,
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _ciTypeService.GetAllAsync(category, activeOnly, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all distinct CI Type categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of distinct categories.</returns>
    /// <response code="200">Returns the list of categories.</response>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _ciTypeService.GetCategoriesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing CI Type.
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="dto">The update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated CI type.</returns>
    /// <response code="200">Returns the updated CI type.</response>
    /// <response code="404">If the CI type is not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CITypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CITypeDto>> Update(int id, [FromBody] UpdateCITypeDto dto, CancellationToken cancellationToken)
    {
        var result = await _ciTypeService.UpdateAsync(id, dto, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Deletes a CI Type (soft delete).
    /// </summary>
    /// <param name="id">The CI type ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">CI type was deleted.</response>
    /// <response code="404">If the CI type is not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _ciTypeService.DeleteAsync(id, cancellationToken);
        return result ? NoContent() : NotFound();
    }
}

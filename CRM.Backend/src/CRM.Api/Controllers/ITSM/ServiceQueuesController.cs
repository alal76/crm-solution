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
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers.ITSM;

/// <summary>
/// API controller for managing service queues.
/// Provides CRUD operations plus queue assignment and stats.
/// </summary>
[ApiController]
[Route("api/servicequeues")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("ITSM - Service Queues")]
public class ServiceQueuesController : CrmControllerBase
{
    private readonly IServiceQueueService _serviceQueueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceQueuesController"/> class.
    /// </summary>
    public ServiceQueuesController(IServiceQueueService serviceQueueService)
    {
        _serviceQueueService = serviceQueueService ?? throw new ArgumentNullException(nameof(serviceQueueService));
    }

    /// <summary>
    /// Get all service queues.
    /// </summary>
    /// <returns>A list of all service queues</returns>
    /// <response code="200">Returns the list of service queues</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<ServiceQueueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
                var queues = await _serviceQueueService.GetAllAsync(cancellationToken);
        return Ok(queues);
    }

    /// <summary>
    /// Get a service queue by ID.
    /// </summary>
    /// <param name="id">The queue ID</param>
    /// <returns>The service queue</returns>
    /// <response code="200">Returns the service queue</response>
    /// <response code="404">Queue not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceQueueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
                var queue = await _serviceQueueService.GetByIdAsync(id, cancellationToken);
        if (queue == null)
            return NotFound(new { message = $"Service queue with ID {id} not found" });
        return Ok(queue);
    }

    /// <summary>
    /// Create a new service queue.
    /// </summary>
    /// <param name="dto">The queue creation data</param>
    /// <returns>The created service queue</returns>
    /// <response code="201">Queue created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ServiceQueueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateServiceQueueDto dto, CancellationToken cancellationToken = default)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var queue = await _serviceQueueService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = queue.Id }, queue);
    }

    /// <summary>
    /// Update an existing service queue.
    /// </summary>
    /// <param name="id">The queue ID</param>
    /// <param name="dto">The queue update data</param>
    /// <returns>The updated service queue</returns>
    /// <response code="200">Queue updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Queue not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ServiceQueueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceQueueDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var queue = await _serviceQueueService.UpdateAsync(id, dto, cancellationToken);
            return Ok(queue);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Service queue with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete a service queue.
    /// </summary>
    /// <param name="id">The queue ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Queue deleted successfully</response>
    /// <response code="404">Queue not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _serviceQueueService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Service queue with ID {id} not found" });
        }
    }

    /// <summary>
    /// Assign a service request to a queue.
    /// </summary>
    /// <param name="queueId">The queue ID</param>
    /// <param name="serviceRequestId">The service request ID to assign</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Assignment successful</response>
    /// <response code="404">Queue or service request not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{queueId:int}/assign/{serviceRequestId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignToQueue(int queueId, int serviceRequestId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _serviceQueueService.AssignToQueueAsync(serviceRequestId, queueId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all service request items currently in a queue.
    /// </summary>
    /// <param name="queueId">The queue ID</param>
    /// <returns>List of service request queue items</returns>
    /// <response code="200">Returns the queue items</response>
    /// <response code="404">Queue not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{queueId:int}/items")]
    [Authorize]
    [ProducesResponseType(typeof(List<ServiceRequestQueueItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQueueItems(int queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _serviceQueueService.GetQueueItemsAsync(queueId, cancellationToken);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Service queue with ID {queueId} not found" });
        }
    }

    /// <summary>
    /// Get statistics for a service queue.
    /// </summary>
    /// <param name="queueId">The queue ID</param>
    /// <returns>The queue with current statistics</returns>
    /// <response code="200">Returns the queue stats</response>
    /// <response code="404">Queue not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{queueId:int}/stats")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceQueueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQueueStats(int queueId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _serviceQueueService.GetQueueStatsAsync(queueId, cancellationToken);
            return Ok(stats);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Service queue with ID {queueId} not found" });
        }
    }
}

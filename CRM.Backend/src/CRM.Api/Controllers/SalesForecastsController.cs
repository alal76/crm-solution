// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for sales forecast management.
/// </summary>
[ApiController]
[Route("api/sales-forecasts")]
[Authorize]
[Produces("application/json")]
public class SalesForecastsController : CrmControllerBase
{
    private const string ForecastNotFoundMessage = "Sales forecast {0} not found";
    private readonly ISalesForecastService _service;
    private readonly ILogger<SalesForecastsController> _logger;

    public SalesForecastsController(ISalesForecastService service, ILogger<SalesForecastsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all sales forecasts with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SalesForecast>>> GetAll(
        [FromQuery] int? userId = null,
        [FromQuery] int? teamId = null,
        [FromQuery] int? fiscalYear = null,
        [FromQuery] bool? isSubmitted = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var forecasts = await _service.GetAllAsync(userId, teamId, fiscalYear, isSubmitted, cancellationToken);
            return Ok(forecasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales forecasts");
            return Problem("An error occurred while retrieving sales forecasts.");
        }
    }

    /// <summary>Gets a sales forecast by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesForecast>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var forecast = await _service.GetByIdAsync(id, cancellationToken);
            if (forecast == null)
            {
                return NotFound(string.Format(ForecastNotFoundMessage, id));
            }
            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales forecast {ForecastId}", id);
            return Problem("An error occurred while retrieving the sales forecast.");
        }
    }

    /// <summary>Creates a new sales forecast.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesForecast>> Create([FromBody] SalesForecast forecast, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var created = await _service.CreateAsync(forecast, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sales forecast");
            return Problem("An error occurred while creating the sales forecast.");
        }
    }

    /// <summary>Updates an existing sales forecast.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(int id, [FromBody] SalesForecast forecast, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var updated = await _service.UpdateAsync(id, forecast, cancellationToken);
            if (!updated)
            {
                return NotFound(string.Format(ForecastNotFoundMessage, id));
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sales forecast {ForecastId}", id);
            return Problem("An error occurred while updating the sales forecast.");
        }
    }

    /// <summary>Deletes a sales forecast (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(string.Format(ForecastNotFoundMessage, id));
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sales forecast {ForecastId}", id);
            return Problem("An error occurred while deleting the sales forecast.");
        }
    }

    #endregion

    #region Forecast Operations

    /// <summary>Submits a forecast for review.</summary>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Submit(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _service.SubmitAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(string.Format(ForecastNotFoundMessage, id));
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting sales forecast {ForecastId}", id);
            return Problem("An error occurred while submitting the sales forecast.");
        }
    }

    /// <summary>Gets forecast history/snapshots for a period.</summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ForecastHistory>>> GetHistory(
        [FromQuery] string period,
        [FromQuery] int? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _service.GetHistoryAsync(period, userId, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast history for period {Period}", period);
            return Problem("An error occurred while retrieving forecast history.");
        }
    }

    /// <summary>Creates a snapshot of a forecast for historical tracking.</summary>
    [HttpPost("{id}/snapshot")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ForecastHistory>> CreateSnapshot(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = await _service.CreateSnapshotAsync(id, cancellationToken);
            return CreatedAtAction(nameof(GetHistory), null, snapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating snapshot for sales forecast {ForecastId}", id);
            return Problem("An error occurred while creating the forecast snapshot.");
        }
    }

    /// <summary>Gets forecast line items for a forecast.</summary>
    [HttpGet("{forecastId}/line-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ForecastLineItem>>> GetLineItems(int forecastId, CancellationToken cancellationToken = default)
    {
        try
        {
            var lineItems = await _service.GetLineItemsAsync(forecastId, cancellationToken);
            return Ok(lineItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving line items for forecast {ForecastId}", forecastId);
            return Problem("An error occurred while retrieving forecast line items.");
        }
    }

    #endregion
}

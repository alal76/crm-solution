using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _opportunityService;
    private readonly ILogger<OpportunitiesController> _logger;

    public OpportunitiesController(IOpportunityService opportunityService, ILogger<OpportunitiesController> logger)
    {
        _opportunityService = opportunityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOpen()
    {
        try
        {
            var opportunities = await _opportunityService.GetOpenOpportunitiesAsync();
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunities");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);
            if (opportunity == null)
                return NotFound();
            return Ok(opportunity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving opportunity {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByAccountAsync(customerId);
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving opportunities for customer {customerId}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("pipeline/total")]
    public async Task<IActionResult> GetTotalPipeline()
    {
        try
        {
            var totalPipeline = await _opportunityService.GetTotalPipelineAsync();
            return Ok(new { totalPipeline });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total pipeline");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Opportunity opportunity)
    {
        try
        {
            var id = await _opportunityService.CreateOpportunityAsync(opportunity);
            return CreatedAtAction(nameof(GetById), new { id }, opportunity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Opportunity opportunity)
    {
        try
        {
            opportunity.Id = id;
            await _opportunityService.UpdateOpportunityAsync(opportunity);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating opportunity {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _opportunityService.DeleteOpportunityAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting opportunity {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Api.Hubs;
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
    private readonly ICrmNotificationService _notificationService;

    public OpportunitiesController(
        IOpportunityService opportunityService, 
        ILogger<OpportunitiesController> logger,
        ICrmNotificationService notificationService)
    {
        _opportunityService = opportunityService;
        _logger = logger;
        _notificationService = notificationService;
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

    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetByAccountId(int accountId)
    {
        try
        {
            var opportunities = await _opportunityService.GetOpportunitiesByAccountAsync(accountId);
            return Ok(opportunities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving opportunities for account {accountId}");
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
            opportunity.Id = id;
            
            // Notify connected clients about the new opportunity
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Opportunity", id, opportunity, userId);
            
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
            
            // Notify connected clients about the update
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordUpdatedAsync("Opportunity", id, opportunity, userId);
            
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
            
            // Notify connected clients about the deletion
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordDeletedAsync("Opportunity", id, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting opportunity {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly IMarketingCampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(IMarketingCampaignService campaignService, ILogger<CampaignsController> logger)
    {
        _campaignService = campaignService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var campaigns = await _campaignService.GetAllCampaignsAsync();
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
                return NotFound();
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving campaign {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(MarketingCampaign campaign)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Business validation: End date must be after start date
            if (campaign.EndDate.HasValue && campaign.StartDate.HasValue && 
                campaign.EndDate.Value < campaign.StartDate.Value)
            {
                return BadRequest(new { message = "End date cannot be before start date" });
            }
            
            // Business validation: Budget cannot be negative
            if (campaign.Budget < 0)
            {
                return BadRequest(new { message = "Budget cannot be negative" });
            }
            
            var id = await _campaignService.CreateCampaignAsync(campaign);
            return CreatedAtAction(nameof(GetById), new { id }, campaign);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MarketingCampaign campaign)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Business validation: End date must be after start date
            if (campaign.EndDate.HasValue && campaign.StartDate.HasValue && 
                campaign.EndDate.Value < campaign.StartDate.Value)
            {
                return BadRequest(new { message = "End date cannot be before start date" });
            }
            
            // Business validation: Budget cannot be negative
            if (campaign.Budget < 0)
            {
                return BadRequest(new { message = "Budget cannot be negative" });
            }
            
            campaign.Id = id;
            await _campaignService.UpdateCampaignAsync(campaign);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating campaign {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _campaignService.DeleteCampaignAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting campaign {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/metrics")]
    public async Task<IActionResult> AddMetric(int id, CampaignMetric metric)
    {
        try
        {
            metric.CampaignId = id;
            await _campaignService.AddCampaignMetricAsync(metric);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding metric to campaign {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}

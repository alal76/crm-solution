// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.MarketingService.Controllers;

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
    public async Task<IActionResult> Create(CreateCampaignDto dto)
    {
        try
        {
            var id = await _campaignService.CreateCampaignAsync(dto);
            var created = await _campaignService.GetCampaignByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCampaignDto dto)
    {
        try
        {
            await _campaignService.UpdateCampaignAsync(id, dto);
            return NoContent();
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

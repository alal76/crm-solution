// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Exceptions;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing leads
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;
    private readonly ILeadAgingAlertService _leadAgingAlertService;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(
        ILeadService leadService,
        ILeadAgingAlertService leadAgingAlertService,
        ILogger<LeadsController> logger)
    {
        _leadService = leadService;
        _leadAgingAlertService = leadAgingAlertService;
        _logger = logger;
    }

    /// <summary>
    /// Get all leads with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var (items, totalCount, p, ps, totalPages) = await _leadService.GetAllAsync(page, pageSize);
            return Ok(new
            {
                data = items,
                totalCount,
                page = p,
                pageSize = ps,
                totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get lead by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeadDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null)
                return NotFound(new { message = $"Lead with ID {id} not found" });
            return Ok(lead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead {LeadId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new lead
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto request)
    {
        try
        {
            var lead = new Lead
            {
                FirstName = request.FirstName!,
                LastName = request.LastName!,
                Email = request.Email ?? string.Empty,
                Phone = request.Phone,
                CompanyName = request.Company ?? request.CompanyName ?? string.Empty,
                Title = request.Title,
                Source = Enum.TryParse<LeadSource>(request.Source, out var source) ? source : LeadSource.Manual,
                Region = request.Region,
                Website = request.Website,
                QualificationNotes = request.Notes ?? request.Description,
                OwnerId = request.OwnerId,
                CampaignId = request.CampaignId
            };

            var id = await _leadService.CreateAsync(lead);
            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = "Lead created successfully" });
        }
        catch (DuplicateExistsException dex)
        {
            return Conflict(new { message = dex.Message, entityType = dex.EntityType, existingRecordId = dex.ExistingRecordId, matchScore = dex.MatchScore });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lead");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a lead
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadDto request)
    {
        try
        {
            var updated = await _leadService.UpdateAsync(id, lead =>
            {
                if (!string.IsNullOrEmpty(request.FirstName))
                    lead.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName))
                    lead.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.Email))
                    lead.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Phone))
                    lead.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.CompanyName))
                    lead.CompanyName = request.CompanyName;
                if (!string.IsNullOrEmpty(request.Title))
                    lead.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadLifecycleStatus>(request.Status, out var status))
                    lead.Status = status;
                if (!string.IsNullOrEmpty(request.Source) && Enum.TryParse<LeadSource>(request.Source, out var src))
                    lead.Source = src;
                if (!string.IsNullOrEmpty(request.Region))
                    lead.Region = request.Region;
                if (!string.IsNullOrEmpty(request.Website))
                    lead.Website = request.Website;
                if (!string.IsNullOrEmpty(request.Notes))
                    lead.QualificationNotes = request.Notes;
                if (request.Score.HasValue)
                    lead.Score = request.Score.Value;
                if (request.OwnerId.HasValue)
                    lead.OwnerId = request.OwnerId.Value;
                if (request.CampaignId.HasValue)
                    lead.CampaignId = request.CampaignId.Value;
            });

            if (!updated)
                return NotFound(new { message = $"Lead with ID {id} not found" });

            return Ok(new { message = "Lead updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead {LeadId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a lead (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _leadService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Lead with ID {id} not found" });
            return Ok(new { message = "Lead deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lead {LeadId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Convert a lead to an opportunity
    /// </summary>
    [HttpPost("{id}/convert")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Convert(int id, [FromBody] ConvertLeadDto request)
    {
        try
        {
            var (opportunityId, leadId) = await _leadService.ConvertAsync(id, request.OpportunityName, request.AccountId, request.EstimatedValue, request.ExpectedCloseDate);
            return Ok(new
            {
                message = "Lead converted successfully",
                opportunityId,
                leadId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting lead {LeadId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Pre-flight duplicate check before form submission.
    /// Checks by email match OR (firstName + lastName + company) match.
    /// Only non-deleted leads are considered.
    /// TODO-CRM002-05
    /// </summary>
    /// <param name="email">Email to check (optional but recommended)</param>
    /// <param name="firstName">First name (used in name-based check)</param>
    /// <param name="lastName">Last name (used in name-based check)</param>
    /// <param name="company">Company name (used in name-based check)</param>
    [HttpGet("check-duplicate")]
    [ProducesResponseType(typeof(CheckDuplicateLeadResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string? email = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? company = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) && (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName)))
                return BadRequest(new { message = "Provide at least 'email' or both 'firstName' and 'lastName'." });

            var (isDuplicate, existingLeadId, matchedOn) = await _leadService.CheckDuplicateAsync(email, firstName, lastName, company);
            return Ok(new CheckDuplicateLeadResponse
            {
                IsDuplicate = isDuplicate,
                ExistingLeadId = existingLeadId,
                MatchedOn = matchedOn
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicate leads");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get leads by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<LeadSummaryDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByStatus(string status)
    {
        try
        {
            if (!Enum.TryParse<LeadLifecycleStatus>(status, true, out var leadStatus))
                return BadRequest(new { message = $"Invalid status: {status}" });

            var leads = await _leadService.GetByStatusAsync(leadStatus);
            return Ok(leads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leads by status {Status}", status);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get leads summary/stats
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _leadService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead stats");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets lead counts grouped by source channel with conversion rates (TODO-CRM002-03).
    /// </summary>
    [HttpGet("analytics/sources")]
    [ProducesResponseType(typeof(IEnumerable<LeadSourceAnalyticsDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSourceAnalytics(CancellationToken ct = default)
    {
        try
        {
            var analytics = await _leadService.GetSourceAnalyticsAsync(ct);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead source analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets UTM campaign attribution breakdown grouped by source/medium/campaign (TODO-CRM002-03).
    /// </summary>
    [HttpGet("analytics/attribution")]
    [ProducesResponseType(typeof(IEnumerable<LeadAttributionDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAttributionAnalytics(CancellationToken ct = default)
    {
        try
        {
            var analytics = await _leadService.GetAttributionAnalyticsAsync(ct);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead attribution analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    // =========================================================================
    // Lead Aging Alerts (TODO-CRM002-07)
    // =========================================================================

    /// <summary>
    /// Returns stale leads with a Warning or Critical staleness level.
    /// "Warning" = ≥ staleDays since last activity; "Critical" = ≥ 30 days (or 2× staleDays when ≥ 30).
    /// </summary>
    /// <param name="staleDays">Minimum days of inactivity to include (default = 14).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("aging-alerts")]
    [ProducesResponseType(typeof(IEnumerable<LeadAgingAlertDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAgingAlerts(
        [FromQuery] int staleDays = 14,
        CancellationToken ct = default)
    {
        try
        {
            var alerts = await _leadAgingAlertService.GetStaledLeadsAsync(staleDays, ct);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead aging alerts");
            return StatusCode(500, "Internal server error");
        }
    }

    // =========================================================================
    // Lead Nurture Campaign Integration (TODO-CRM002-06)
    // =========================================================================

    /// <summary>
    /// Enrols a lead in a nurture campaign.
    /// </summary>
    [HttpPost("{id}/nurture")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AssignNurtureCampaign(
        int id,
        [FromBody] AssignNurtureCampaignDto request,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _leadService.AssignToNurtureCampaignAsync(id, request.CampaignId, ct);
            if (!result)
                return NotFound(new { message = $"Lead {id} or campaign {request.CampaignId} not found." });

            return Ok(new { message = "Lead enrolled in nurture campaign.", leadId = id, campaignId = request.CampaignId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning lead {LeadId} to nurture campaign", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Returns the nurture campaign the lead is currently enrolled in (single entry or empty array).
    /// </summary>
    [HttpGet("{id}/nurture-campaigns")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetNurtureCampaigns(int id, CancellationToken ct = default)
    {
        try
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null)
                return NotFound(new { message = $"Lead {id} not found." });

            var campaign = await _leadService.GetNurtureCampaignAsync(id, ct);
            var result = campaign != null
                ? new[] { new { campaign.Id, campaign.Name } }
                : Array.Empty<object>();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nurture campaigns for lead {LeadId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Removes a lead from its nurture campaign.
    /// </summary>
    [HttpDelete("{id}/nurture-campaigns/{campaignId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RemoveFromNurtureCampaign(
        int id,
        int campaignId,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _leadService.RemoveFromNurtureCampaignAsync(id, campaignId, ct);
            if (!result)
                return NotFound(new { message = $"Lead {id} is not enrolled in campaign {campaignId}." });

            return Ok(new { message = "Lead removed from nurture campaign.", leadId = id, campaignId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing lead {LeadId} from nurture campaign {CampaignId}", id, campaignId);
            return StatusCode(500, "Internal server error");
        }
    }
}

#region Request DTOs

public class CreateLeadDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Source { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string? Description { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
    public int? Status { get; set; }
}

public class UpdateLeadDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public int? Score { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
}

public class ConvertLeadDto
{
    public string? OpportunityName { get; set; }
    public int? AccountId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}

public class CheckDuplicateLeadResponse
{
    public bool IsDuplicate { get; set; }
    public int? ExistingLeadId { get; set; }
    /// <summary>"email" or "name" indicating which field(s) matched</summary>
    public string? MatchedOn { get; set; }
}

/// <summary>Request body for nurture campaign assignment (TODO-CRM002-06).</summary>
public class AssignNurtureCampaignDto
{
    /// <summary>ID of the marketing campaign to enrol the lead in.</summary>
    public int CampaignId { get; set; }
}

#endregion

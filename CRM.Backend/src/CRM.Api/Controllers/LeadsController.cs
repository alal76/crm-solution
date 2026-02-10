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

using CRM.Core.Entities;
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
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(ILeadService leadService, ILogger<LeadsController> logger)
    {
        _leadService = leadService;
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
    [ProducesResponseType(typeof(Lead), 200)]
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
                if (!string.IsNullOrEmpty(request.FirstName)) lead.FirstName = request.FirstName;
                if (!string.IsNullOrEmpty(request.LastName)) lead.LastName = request.LastName;
                if (!string.IsNullOrEmpty(request.Email)) lead.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Phone)) lead.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.CompanyName)) lead.CompanyName = request.CompanyName;
                if (!string.IsNullOrEmpty(request.Title)) lead.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadLifecycleStatus>(request.Status, out var status))
                    lead.Status = status;
                if (!string.IsNullOrEmpty(request.Source) && Enum.TryParse<LeadSource>(request.Source, out var src))
                    lead.Source = src;
                if (!string.IsNullOrEmpty(request.Region)) lead.Region = request.Region;
                if (!string.IsNullOrEmpty(request.Website)) lead.Website = request.Website;
                if (!string.IsNullOrEmpty(request.Notes)) lead.QualificationNotes = request.Notes;
                if (request.Score.HasValue) lead.Score = request.Score.Value;
                if (request.OwnerId.HasValue) lead.OwnerId = request.OwnerId.Value;
                if (request.CampaignId.HasValue) lead.CampaignId = request.CampaignId.Value;
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
    /// Get leads by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<Lead>), 200)]
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

#endregion

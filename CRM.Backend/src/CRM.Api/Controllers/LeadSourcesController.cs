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
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing lead sources.
/// TODO-CRM002-03: Lead source tracking and attribution
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadSourcesController : ControllerBase
{
    private const string LeadSourceNotFoundMessage = "Lead source with ID {0} not found";
    private const string InternalServerErrorMessage = "Internal server error";
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadSourcesController> _logger;

    public LeadSourcesController(ICrmDbContext context, ILogger<LeadSourcesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all lead sources.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeadSourceDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false, CancellationToken ct = default)
    {
        try
        {
            var query = _context.LeadSources.AsNoTracking().Where(ls => !ls.IsDeleted);

            if (activeOnly)
                query = query.Where(ls => ls.IsActive);

            var sources = await query
                .OrderBy(ls => ls.Name)
                .Select(ls => new LeadSourceDto
                {
                    Id = ls.Id,
                    Name = ls.Name,
                    Code = ls.Code,
                    Description = ls.Description,
                    Channel = ls.Channel,
                    Medium = ls.Medium,
                    CampaignName = ls.CampaignName,
                    CostPerLead = ls.CostPerLead,
                    TotalSpend = ls.TotalSpend,
                    IsActive = ls.IsActive,
                    TrackingUrl = ls.TrackingUrl,
                    ExternalPlatformId = ls.ExternalPlatformId,
                    CreatedAt = ls.CreatedAt,
                    UpdatedAt = ls.UpdatedAt ?? ls.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead sources");
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Get lead source by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeadSourceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            var source = await _context.LeadSources
                .AsNoTracking()
                .FirstOrDefaultAsync(ls => ls.Id == id && !ls.IsDeleted, ct);

            if (source == null)
                return NotFound(new { message = string.Format(LeadSourceNotFoundMessage, id) });

            return Ok(new LeadSourceDto
            {
                Id = source.Id,
                Name = source.Name,
                Code = source.Code,
                Description = source.Description,
                Channel = source.Channel,
                Medium = source.Medium,
                CampaignName = source.CampaignName,
                CostPerLead = source.CostPerLead,
                TotalSpend = source.TotalSpend,
                IsActive = source.IsActive,
                TrackingUrl = source.TrackingUrl,
                ExternalPlatformId = source.ExternalPlatformId,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt ?? source.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead source {Id}", id);
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Create a new lead source.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateLeadSourceDto request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name is required" });

            var source = new LeadSourceEntity
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                Channel = request.Channel,
                Medium = request.Medium,
                CampaignName = request.CampaignName,
                CostPerLead = request.CostPerLead,
                TotalSpend = request.TotalSpend,
                IsActive = request.IsActive,
                TrackingUrl = request.TrackingUrl,
                ExternalPlatformId = request.ExternalPlatformId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.LeadSources.Add(source);
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Created lead source: {Name} (ID: {Id})", source.Name, source.Id);
            return CreatedAtAction(nameof(GetById), new { id = source.Id }, new { id = source.Id, message = "Lead source created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lead source");
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Update a lead source.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadSourceDto request, CancellationToken ct = default)
    {
        try
        {
            var source = await _context.LeadSources.FirstOrDefaultAsync(ls => ls.Id == id && !ls.IsDeleted, ct);
            if (source == null)
                return NotFound(new { message = string.Format(LeadSourceNotFoundMessage, id) });

            if (!string.IsNullOrWhiteSpace(request.Name))
                source.Name = request.Name;
            if (request.Code != null)
                source.Code = request.Code;
            if (request.Description != null)
                source.Description = request.Description;
            if (request.Channel.HasValue)
                source.Channel = request.Channel.Value;
            if (request.Medium != null)
                source.Medium = request.Medium;
            if (request.CampaignName != null)
                source.CampaignName = request.CampaignName;
            if (request.CostPerLead.HasValue)
                source.CostPerLead = request.CostPerLead;
            if (request.TotalSpend.HasValue)
                source.TotalSpend = request.TotalSpend;
            if (request.IsActive.HasValue)
                source.IsActive = request.IsActive.Value;
            if (request.TrackingUrl != null)
                source.TrackingUrl = request.TrackingUrl;
            if (request.ExternalPlatformId != null)
                source.ExternalPlatformId = request.ExternalPlatformId;

            source.UpdatedAt = DateTime.UtcNow;
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Updated lead source: {Id}", id);
            return Ok(new { message = "Lead source updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead source {Id}", id);
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Delete a lead source.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        try
        {
            var source = await _context.LeadSources.FirstOrDefaultAsync(ls => ls.Id == id && !ls.IsDeleted, ct);
            if (source == null)
                return NotFound(new { message = string.Format(LeadSourceNotFoundMessage, id) });

            source.IsDeleted = true;
            source.UpdatedAt = DateTime.UtcNow;
            await (_context as DbContext)!.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted lead source: {Id}", id);
            return Ok(new { message = "Lead source deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lead source {Id}", id);
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Get lead source statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(LeadSourceStatistics), 200)]
    public async Task<IActionResult> GetStatistics(CancellationToken ct = default)
    {
        try
        {
            var sources = await _context.LeadSources
                .AsNoTracking()
                .Where(ls => !ls.IsDeleted && ls.IsActive)
                .ToListAsync(ct);

            var leads = await _context.Leads
                .AsNoTracking()
                .Where(l => !l.IsDeleted && l.LeadSourceId.HasValue)
                .GroupBy(l => l.LeadSourceId)
                .Select(g => new { SourceId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var stats = new LeadSourceStatistics
            {
                TotalSources = sources.Count,
                ActiveSources = sources.Count(s => s.IsActive),
                TotalSpend = sources.Sum(s => s.TotalSpend ?? 0),
                AverageCostPerLead = sources.Average(s => s.CostPerLead ?? 0),
                SourceBreakdown = leads.ToDictionary(
                    l => sources.FirstOrDefault(s => s.Id == l.SourceId)?.Name ?? "Unknown",
                    l => l.Count)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lead source statistics");
            return StatusCode(500, InternalServerErrorMessage);
        }
    }

    /// <summary>
    /// Gets a lead source report: leads grouped by source with conversion metrics.
    /// TODO-CRM002-03: Lead source tracking and attribution report.
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(LeadSourceReport), 200)]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
            var to = toDate ?? DateTime.UtcNow;

            var sources = await _context.LeadSources
                .AsNoTracking()
                .Where(ls => !ls.IsDeleted)
                .ToListAsync(ct);

            var leads = await _context.Leads
                .AsNoTracking()
                .Where(l => !l.IsDeleted && l.CreatedAt >= from && l.CreatedAt <= to)
                .ToListAsync(ct);

            var report = new LeadSourceReport
            {
                FromDate = from,
                ToDate = to,
                TotalLeads = leads.Count,
                SourceDetails = sources.Select(source =>
                {
                    var sourceLeads = leads.Where(l => l.LeadSourceId == source.Id).ToList();
                    var converted = sourceLeads.Count(l => l.Status == LeadLifecycleStatus.Converted);
                    var total = sourceLeads.Count;

                    return new LeadSourceReportItem
                    {
                        SourceId = source.Id,
                        SourceName = source.Name,
                        Channel = source.Channel.ToString(),
                        TotalLeads = total,
                        ConvertedLeads = converted,
                        ConversionRate = total > 0 ? Math.Round((decimal)converted / total * 100, 2) : 0,
                        CostPerLead = source.CostPerLead ?? 0,
                        TotalSpend = source.TotalSpend ?? 0,
                        CostPerConversion = converted > 0 ? Math.Round((source.TotalSpend ?? 0) / converted, 2) : 0
                    };
                }).OrderByDescending(s => s.TotalLeads).ToList(),
                UnattributedLeads = leads.Count(l => !l.LeadSourceId.HasValue)
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating lead source report");
            return StatusCode(500, InternalServerErrorMessage);
        }
    }
}

#region DTOs

public class LeadSourceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public LeadSourceChannel Channel { get; set; }
    public string? Medium { get; set; }
    public string? CampaignName { get; set; }
    public decimal? CostPerLead { get; set; }
    public decimal? TotalSpend { get; set; }
    public bool IsActive { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ExternalPlatformId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateLeadSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public LeadSourceChannel Channel { get; set; } = LeadSourceChannel.Other;
    public string? Medium { get; set; }
    public string? CampaignName { get; set; }
    public decimal? CostPerLead { get; set; }
    public decimal? TotalSpend { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TrackingUrl { get; set; }
    public string? ExternalPlatformId { get; set; }
}

public class UpdateLeadSourceDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public LeadSourceChannel? Channel { get; set; }
    public string? Medium { get; set; }
    public string? CampaignName { get; set; }
    public decimal? CostPerLead { get; set; }
    public decimal? TotalSpend { get; set; }
    public bool? IsActive { get; set; }
    public string? TrackingUrl { get; set; }
    public string? ExternalPlatformId { get; set; }
}

public class LeadSourceStatistics
{
    public int TotalSources { get; set; }
    public int ActiveSources { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal AverageCostPerLead { get; set; }
    public Dictionary<string, int> SourceBreakdown { get; set; } = new();
}

/// <summary>
/// Lead source attribution report.
/// </summary>
public class LeadSourceReport
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalLeads { get; set; }
    public int UnattributedLeads { get; set; }
    public List<LeadSourceReportItem> SourceDetails { get; set; } = new();
}

/// <summary>
/// Individual source detail in a lead source report.
/// </summary>
public class LeadSourceReportItem
{
    public int SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal CostPerLead { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal CostPerConversion { get; set; }
}

#endregion

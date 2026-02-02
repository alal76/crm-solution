using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.SalesService.Controllers;

/// <summary>
/// Controller for managing leads
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(CrmDbContext context, ILogger<LeadsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all leads with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var query = _context.Leads
                .Where(l => !l.IsDeleted)
                .OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();
            var leads = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.Email,
                    l.Phone,
                    l.CompanyName,
                    l.Title,
                    Status = l.Status.ToString(),
                    Source = l.Source.ToString(),
                    l.Score,
                    l.FitScore,
                    l.EngagementScore,
                    l.OwnerId,
                    l.CreatedAt,
                    l.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = leads,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
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
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var lead = await _context.Leads
                .Where(l => l.Id == id && !l.IsDeleted)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.Email,
                    l.Phone,
                    l.CompanyName,
                    l.Title,
                    Status = l.Status.ToString(),
                    Source = l.Source.ToString(),
                    l.Score,
                    l.FitScore,
                    l.EngagementScore,
                    l.QualificationNotes,
                    l.Region,
                    l.Website,
                    l.OwnerId,
                    l.AccountId,
                    l.ContactId,
                    l.CampaignId,
                    l.MqlDate,
                    l.SqlDate,
                    l.CreatedAt,
                    l.UpdatedAt
                })
                .FirstOrDefaultAsync();

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
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest request)
    {
        try
        {
            var lead = new Lead
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                CompanyName = request.CompanyName,
                Title = request.Title,
                Status = LeadLifecycleStatus.New,
                Source = Enum.TryParse<LeadSource>(request.Source, out var source) ? source : LeadSource.Manual,
                Region = request.Region,
                Website = request.Website,
                QualificationNotes = request.Notes,
                OwnerId = request.OwnerId,
                CampaignId = request.CampaignId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = lead.Id }, new { id = lead.Id, message = "Lead created successfully" });
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadRequest request)
    {
        try
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            if (lead == null)
                return NotFound(new { message = $"Lead with ID {id} not found" });

            if (!string.IsNullOrEmpty(request.FirstName)) lead.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName)) lead.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.Email)) lead.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Phone)) lead.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.CompanyName)) lead.CompanyName = request.CompanyName;
            if (!string.IsNullOrEmpty(request.Title)) lead.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadLifecycleStatus>(request.Status, out var status))
                lead.Status = status;
            if (!string.IsNullOrEmpty(request.Source) && Enum.TryParse<LeadSource>(request.Source, out var source))
                lead.Source = source;
            if (!string.IsNullOrEmpty(request.Region)) lead.Region = request.Region;
            if (!string.IsNullOrEmpty(request.Website)) lead.Website = request.Website;
            if (!string.IsNullOrEmpty(request.Notes)) lead.QualificationNotes = request.Notes;
            if (request.Score.HasValue) lead.Score = request.Score.Value;
            if (request.OwnerId.HasValue) lead.OwnerId = request.OwnerId.Value;
            if (request.CampaignId.HasValue) lead.CampaignId = request.CampaignId.Value;

            lead.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            if (lead == null)
                return NotFound(new { message = $"Lead with ID {id} not found" });

            lead.IsDeleted = true;
            lead.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
    public async Task<IActionResult> Convert(int id, [FromBody] ConvertLeadRequest request)
    {
        try
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
            if (lead == null)
                return NotFound(new { message = $"Lead with ID {id} not found" });

            if (lead.Status == LeadLifecycleStatus.Converted)
                return BadRequest(new { message = "Lead has already been converted" });

            // Create opportunity
            var opportunity = new Opportunity
            {
                Name = request.OpportunityName ?? $"{lead.CompanyName} - Opportunity",
                AccountId = request.CustomerId ?? lead.AccountId ?? 0, // AccountId is required
                PrimaryContactId = lead.ContactId,
                Amount = request.EstimatedValue ?? 0,
                Stage = OpportunityStage.Discovery,
                Probability = 10,
                ExpectedCloseDate = request.ExpectedCloseDate ?? DateTime.UtcNow.AddMonths(3),
                SalesOwnerId = lead.OwnerId,
                LeadId = lead.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Opportunities.Add(opportunity);

            // Update lead status
            lead.Status = LeadLifecycleStatus.Converted;
            lead.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Lead converted successfully",
                opportunityId = opportunity.Id,
                leadId = lead.Id
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
    public async Task<IActionResult> GetByStatus(string status)
    {
        try
        {
            if (!Enum.TryParse<LeadLifecycleStatus>(status, true, out var leadStatus))
                return BadRequest(new { message = $"Invalid status: {status}" });

            var leads = await _context.Leads
                .Where(l => l.Status == leadStatus && !l.IsDeleted)
                .OrderByDescending(l => l.Score)
                .Select(l => new
                {
                    l.Id,
                    l.FirstName,
                    l.LastName,
                    l.Email,
                    l.CompanyName,
                    Status = l.Status.ToString(),
                    l.Score,
                    l.CreatedAt
                })
                .ToListAsync();

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
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = new
            {
                total = await _context.Leads.CountAsync(l => !l.IsDeleted),
                newLeads = await _context.Leads.CountAsync(l => l.Status == LeadLifecycleStatus.New && !l.IsDeleted),
                working = await _context.Leads.CountAsync(l => l.Status == LeadLifecycleStatus.Working && !l.IsDeleted),
                qualified = await _context.Leads.CountAsync(l => l.Status == LeadLifecycleStatus.Qualified && !l.IsDeleted),
                converted = await _context.Leads.CountAsync(l => l.Status == LeadLifecycleStatus.Converted && !l.IsDeleted),
                disqualified = await _context.Leads.CountAsync(l => l.Status == LeadLifecycleStatus.Disqualified && !l.IsDeleted),
                avgScore = await _context.Leads.Where(l => !l.IsDeleted).AverageAsync(l => (double?)l.Score) ?? 0
            };

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

public class CreateLeadRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Source { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public int? OwnerId { get; set; }
    public int? CampaignId { get; set; }
}

public class UpdateLeadRequest
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

public class ConvertLeadRequest
{
    public string? OpportunityName { get; set; }
    public int? CustomerId { get; set; }
    public decimal? EstimatedValue { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
}

#endregion

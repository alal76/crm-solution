// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

// CRM staff portal proxy — route moved to /api/portal/crm to avoid conflict with customer PortalController
// PORTAL-014: Route conflict fix — CustomerPortalController moved from /api/portal to /api/portal/crm
/// <summary>
/// CRM staff-facing portal proxy endpoints (PORTAL-01/02/03/04).
/// Staff access tickets/articles on behalf of customers.
/// Route changed to /api/portal/crm to avoid ambiguity with the public PortalController.
/// </summary>
[ApiController]
[Route("api/portal/crm")]
[Authorize]
public class CustomerPortalController : ControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<CustomerPortalController> _logger;

    public CustomerPortalController(ICrmDbContext db, ILogger<CustomerPortalController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    // ── Support Tickets ────────────────────────────────────────────────────

    /// <summary>Lists the authenticated user's support tickets.</summary>
    [HttpGet("tickets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var query = _db.ServiceRequests.AsNoTracking()
            .Where(t => t.CreatedByUserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { items, totalCount = total, page, pageSize });
    }

    /// <summary>Gets a specific ticket owned by the authenticated user.</summary>
    [HttpGet("tickets/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicket(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var query2 = _db.ServiceRequests.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.CreatedByUserId == userId && !t.IsDeleted, ct);
        var ticket2 = await query2;
        return ticket2 == null ? NotFound(new { message = "Ticket not found" }) : Ok(ticket2);
    }

    /// <summary>Creates a new support ticket from the portal.</summary>
    [HttpPost("tickets")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTicket([FromBody] PortalCreateTicketRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { message = "Title is required." });

        var userId = GetCurrentUserId();
        var ticket = new ServiceRequest
        {
            TicketNumber = $"TKT{DateTime.UtcNow:yyyyMMddHHmmss}",
            Subject = req.Title,
            Description = req.Description,
            CreatedByUserId = userId,
            Priority = ServiceRequestPriority.Medium,
            Status = ServiceRequestStatus.New,
            Channel = ServiceRequestChannel.SelfServicePortal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ServiceRequests.Add(ticket);
        await _db.SaveChangesAsync(ct);
        return Created($"api/portal/tickets/{ticket.Id}", ticket);
    }

    // ── Knowledge Base ─────────────────────────────────────────────────────

    /// <summary>Searches knowledge base articles from the portal.</summary>
    [HttpGet("kb/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchKb([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var query = _db.KnowledgeArticles.AsNoTracking()
            .Where(a => a.Status == ArticleStatus.Published && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(a => EF.Functions.Like(a.Title, $"%{q}%") || EF.Functions.Like(a.Content, $"%{q}%"));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.ViewCount)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new { a.Id, a.Title, a.Summary, a.CategoryId, a.ViewCount, a.CreatedAt })
            .ToListAsync(ct);

        return Ok(new { items, totalCount = total, page, pageSize });
    }

    /// <summary>Gets a knowledge base article.</summary>
    [HttpGet("kb/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKbArticle(int id, CancellationToken ct = default)
    {
        var article = await _db.KnowledgeArticles.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, ct);
        if (article == null) return NotFound(new { message = "Article not found" });

        // Increment view count
        var tracked = await _db.KnowledgeArticles.FindAsync(new object[] { id }, ct);
        if (tracked != null) { tracked.ViewCount++; await _db.SaveChangesAsync(ct); }

        return Ok(article);
    }

    /// <summary>Submits feedback on a knowledge base article.</summary>
    [HttpPost("kb/{id:int}/feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitKbFeedback(int id, [FromBody] PortalKbFeedbackRequest req, CancellationToken ct = default)
    {
        var exists = await _db.KnowledgeArticles.AnyAsync(a => a.Id == id && a.Status == ArticleStatus.Published && !a.IsDeleted, ct);
        if (!exists) return NotFound(new { message = "Article not found" });

        var userId = GetCurrentUserId();
        var feedback = new ArticleFeedback
        {
            KnowledgeArticleId = id,
            IsHelpful = req.IsHelpful,
            Comment = req.Comment,
            UserId = userId > 0 ? userId : null,
            SubmittedAt = DateTime.UtcNow
        };

        _db.ArticleFeedbacks.Add(feedback);
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Feedback submitted, thank you." });
    }

    // ── Deal Registration (Partner portal) ────────────────────────────────

    /// <summary>Registers a new deal from a partner portal user.</summary>
    [HttpPost("deals/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterDeal([FromBody] PortalRegisterDealRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.CompanyName))
            return BadRequest(new { message = "CompanyName is required." });

        var userId = GetCurrentUserId();
        var lead = new Lead
        {
            FirstName = req.ContactFirstName ?? string.Empty,
            LastName = req.ContactLastName ?? string.Empty,
            CompanyName = req.CompanyName,
            Email = req.Email ?? string.Empty,
            QualificationNotes = req.Notes,
            Source = LeadSource.Partner,
            Status = LeadLifecycleStatus.New,
            OwnerId = userId > 0 ? userId : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Deal registered by partner {UserId} for company '{Company}'", userId, req.CompanyName);
        return Created($"api/portal/deals/{lead.Id}", new { lead.Id, lead.CompanyName, message = "Deal registration submitted for review." });
    }
}

// ── Request DTOs ───────────────────────────────────────────────────────────

public record PortalCreateTicketRequest(string Title, string? Description, string? Priority);
public record PortalKbFeedbackRequest(bool IsHelpful, string? Comment);
public record PortalRegisterDealRequest(string CompanyName, string? ContactFirstName, string? ContactLastName, string? Email, string? Notes);

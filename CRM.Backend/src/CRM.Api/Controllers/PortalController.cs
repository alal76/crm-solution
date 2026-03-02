// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CRM.Core.Dtos;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.IdentityModel.Tokens;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Customer Portal endpoints accessible by authenticated portal users.
/// Portal auth uses its own JWT (claim: portal_user_id) separate from CRM auth.
/// All endpoints require a valid portal JWT token in the Authorization header.
/// </summary>
/// <remarks>PORTAL-015: Gated by EnableCustomerPortal feature flag.</remarks>
[ApiController]
[Route("api/portal")]
[AllowAnonymous] // NOSONAR - S4834: Route-level; actual auth done via ExtractPortalUserId helper
[FeatureGate(FeatureFlags.EnableCustomerPortal)]
public class PortalController : CrmControllerBase
{
    private readonly IPortalService _portalService;
    private readonly IConfiguration _configuration;
    private const string PortalAuthRequired = "Portal authentication required.";

    public PortalController(
        IPortalService portalService,
        IConfiguration configuration)
    {
        _portalService = portalService;
        _configuration = configuration;
    }

    // ── Config (public — no auth required) ───────────────────────────────────

    /// <summary>GET /api/portal/config — public portal configuration</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _portalService.GetConfigAsync(ct);
        return Ok(config);
    }

    // ── Tickets ───────────────────────────────────────────────────────────────

    /// <summary>GET /api/portal/tickets?page=1&pageSize=20</summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        var result = await _portalService.GetMyTicketsAsync(userId.Value, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>POST /api/portal/tickets — create a ticket</summary>
    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket(
        [FromBody] PortalCreateTicketDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        try
        {
            var ticket = await _portalService.CreateTicketAsync(userId.Value, dto, ct);
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>GET /api/portal/tickets/{id}</summary>
    [HttpGet("tickets/{id:int}")]
    public async Task<IActionResult> GetTicket(int id, CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        var ticket = await _portalService.GetTicketAsync(userId.Value, id, ct);
        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    /// <summary>GET /api/portal/tickets/{id}/comments</summary>
    [HttpGet("tickets/{id:int}/comments")]
    public async Task<IActionResult> GetTicketComments(int id, CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        var comments = await _portalService.GetTicketCommentsAsync(userId.Value, id, ct);
        return Ok(comments);
    }

    /// <summary>POST /api/portal/tickets/{id}/comments</summary>
    [HttpPost("tickets/{id:int}/comments")]
    public async Task<IActionResult> AddTicketComment(
        int id, [FromBody] PortalAddCommentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        try
        {
            var comment = await _portalService.AddTicketCommentAsync(userId.Value, id, dto, ct);
            return Ok(comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Knowledge Base ────────────────────────────────────────────────────────

    /// <summary>GET /api/portal/knowledge-base?search=&page=1&pageSize=20</summary>
    [HttpGet("knowledge-base")]
    public async Task<IActionResult> GetKnowledgeArticles(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _portalService.GetKnowledgeArticlesAsync(search, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>GET /api/portal/knowledge-base/{id}</summary>
    [HttpGet("knowledge-base/{id:int}")]
    public async Task<IActionResult> GetKnowledgeArticle(int id, CancellationToken ct)
    {
        var article = await _portalService.GetKnowledgeArticleAsync(id, ct);
        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }
    // ── Profile (PORTAL-019) ────────────────────────────────────────────────────────────

    /// <summary>GET /api/portal/profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        var profile = await _portalService.GetProfileAsync(userId.Value, ct);
        return Ok(profile);
    }

    /// <summary>PUT /api/portal/profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePortalProfileDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        try
        {
            var profile = await _portalService.UpdateProfileAsync(userId.Value, dto, ct);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>POST /api/portal/profile/change-password</summary>
    [HttpPost("profile/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePortalPasswordDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        try
        {
            await _portalService.ChangePasswordAsync(userId.Value, dto, ct);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Attachments (PORTAL-022) ────────────────────────────────────────────────────

    /// <summary>POST /api/portal/tickets/{ticketId}/attachments</summary>
    [HttpPost("tickets/{ticketId:int}/attachments")]
    [RequestSizeLimit(10_485_760)] // 10 MB
    public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file, CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var dto = await _portalService.UploadAttachmentAsync(
                ticketId, userId.Value, file.FileName, file.ContentType, stream, file.Length, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>GET /api/portal/tickets/{ticketId}/attachments</summary>
    [HttpGet("tickets/{ticketId:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int ticketId, CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        var attachments = await _portalService.GetAttachmentsAsync(ticketId, userId.Value, ct);
        return Ok(attachments);
    }

    // ── Cancel ticket (PORTAL-023) ───────────────────────────────────────────────────

    /// <summary>PATCH /api/portal/tickets/{ticketId}/cancel</summary>
    [HttpPatch("tickets/{ticketId:int}/cancel")]
    public async Task<IActionResult> CancelTicket(int ticketId, CancellationToken ct)
    {
        var userId = ExtractPortalUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = PortalAuthRequired });
        }

        try
        {
            await _portalService.CancelTicketAsync(ticketId, userId.Value, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the portal user ID from the Bearer JWT in the Authorization header.
    /// Returns null if the token is missing, invalid, or not a portal token.
    /// </summary>
    private int? ExtractPortalUserId()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            var token = authHeader["Bearer ".Length..].Trim();
            var secret = _configuration["Jwt:Secret"]
                ?? "development-only-jwt-secret-key-minimum-32-chars";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, validationParams, out _);

            // Must be a portal token
            var portalClaim = principal.FindFirst("portal");
            if (portalClaim?.Value != "true")
            {
                return null;
            }

            var userIdClaim = principal.FindFirst("portal_user_id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
            {
                return uid;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

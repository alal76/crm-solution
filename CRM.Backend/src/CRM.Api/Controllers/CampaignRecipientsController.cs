// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

/// <summary>
/// REST API Controller for Campaign Recipient management operations.
///
/// FUNCTIONAL VIEW:
/// This controller provides HTTP endpoints for:
/// - Managing campaign recipients (contacts/leads/accounts)
/// - Adding recipients to campaigns
/// - Tracking recipient engagement status
/// - Filtering and querying recipients by campaign
///
/// TECHNICAL VIEW:
/// - Uses ICampaignRecipientService for business logic (dependency injected)
/// - Uses ICrmDbContext for direct database operations
/// - All endpoints require authentication (JWT Bearer token)
/// - Returns standardized JSON responses with appropriate HTTP status codes
/// - Implements proper error handling with logging
///
/// API ROUTES:
/// - GET    /api/campaign-recipients              - Get all recipients with pagination
/// - GET    /api/campaign-recipients/{id}         - Get recipient by ID
/// - GET    /api/campaign-recipients/campaign/{campaignId} - Get recipients by campaign
/// - POST   /api/campaign-recipients              - Create new recipient
/// - PUT    /api/campaign-recipients/{id}         - Update recipient
/// - DELETE /api/campaign-recipients/{id}         - Delete recipient (soft delete)
/// </summary>
[ApiController]
[Route("api/campaign-recipients")]
[Authorize]
public class CampaignRecipientsController : ControllerBase
{
    private const string RecipientNotFoundMessage = "Campaign recipient with ID {0} not found";
    private readonly ICampaignRecipientService _campaignRecipientService;
    private readonly ICrmDbContext _context;
    private readonly ILogger<CampaignRecipientsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CampaignRecipientsController"/> class.
    /// </summary>
    /// <param name="campaignRecipientService">Service for campaign recipient business logic.</param>
    /// <param name="context">Database context for direct data operations.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    public CampaignRecipientsController(
        ICampaignRecipientService campaignRecipientService,
        ICrmDbContext context,
        ILogger<CampaignRecipientsController> logger)
    {
        _campaignRecipientService = campaignRecipientService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all campaign recipients with pagination.
    ///
    /// FUNCTIONAL: Returns paginated list of all campaign recipients.
    /// TECHNICAL: Filters out soft-deleted records, returns 200 OK with array.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated array of CampaignRecipientDto objects.</returns>
    /// <response code="200">Returns the list of campaign recipients.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CampaignRecipientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = await _context.CampaignRecipients
                .Where(r => !r.IsDeleted)
                .Include(r => r.Contact)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapToDto(r))
                .ToListAsync(cancellationToken);

            var totalCount = await _context.CampaignRecipients
                .CountAsync(r => !r.IsDeleted, cancellationToken);

            return Ok(new
            {
                items = recipients,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign recipients");
            return StatusCode(500, new { message = "Error retrieving campaign recipients", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific campaign recipient by ID.
    ///
    /// FUNCTIONAL: Returns detailed recipient information for viewing/editing.
    /// TECHNICAL: Returns 404 if recipient not found or deleted.
    /// </summary>
    /// <param name="id">The unique recipient identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CampaignRecipientDto if found.</returns>
    /// <response code="200">Returns the campaign recipient.</response>
    /// <response code="404">If recipient not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignRecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipient = await _context.CampaignRecipients
                .Include(r => r.Contact)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (recipient == null)
            {
                return NotFound(new { message = string.Format(RecipientNotFoundMessage, id) });
            }

            return Ok(MapToDto(recipient));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign recipient {Id}", id);
            return StatusCode(500, new { message = "Error retrieving campaign recipient", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all recipients for a specific campaign with pagination.
    ///
    /// FUNCTIONAL: Returns list of recipients associated with a campaign.
    /// TECHNICAL: Uses ICampaignRecipientService for optimized campaign-scoped queries.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of CampaignRecipientDto objects for the campaign.</returns>
    /// <response code="200">Returns the list of campaign recipients.</response>
    /// <response code="404">If campaign not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("campaign/{campaignId}")]
    [ProducesResponseType(typeof(IEnumerable<CampaignRecipientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCampaignId(
        int campaignId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = await _campaignRecipientService.GetRecipientsAsync(
                campaignId, page, pageSize, cancellationToken);

            var totalCount = await _campaignRecipientService.GetCountAsync(campaignId, cancellationToken);

            return Ok(new
            {
                items = recipients,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Campaign {CampaignId} not found", campaignId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipients for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error retrieving campaign recipients", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new campaign recipient.
    ///
    /// FUNCTIONAL: Adds a new recipient to a campaign.
    /// TECHNICAL: Creates CampaignRecipient entity, returns 201 Created with location header.
    /// </summary>
    /// <param name="dto">The campaign recipient data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created CampaignRecipientDto.</returns>
    /// <response code="201">Returns the newly created recipient.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="409">If recipient already exists in campaign.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignRecipientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CampaignRecipientDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify campaign exists
            var campaignExists = await _context.MarketingCampaigns
                .AnyAsync(c => c.Id == dto.CampaignId && !c.IsDeleted, cancellationToken);

            if (!campaignExists)
            {
                return BadRequest(new { message = $"Campaign with ID {dto.CampaignId} not found" });
            }

            // Check for duplicate recipient in campaign
            var existingRecipient = await _context.CampaignRecipients
                .AnyAsync(r => r.CampaignId == dto.CampaignId &&
                              ((dto.ContactId.HasValue && r.ContactId == dto.ContactId) ||
                               (dto.AccountId.HasValue && r.AccountId == dto.AccountId) ||
                               (!string.IsNullOrEmpty(dto.Email) && r.Email == dto.Email)) &&
                              !r.IsDeleted, cancellationToken);

            if (existingRecipient)
            {
                return Conflict(new { message = "Recipient already exists in this campaign" });
            }

            var recipient = new CampaignRecipient
            {
                CampaignId = dto.CampaignId,
                ContactId = dto.ContactId,
                AccountId = dto.AccountId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Status = CampaignRecipientStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CampaignRecipients.Add(recipient);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created campaign recipient {Id} for campaign {CampaignId}",
                recipient.Id, recipient.CampaignId);

            return CreatedAtAction(nameof(GetById), new { id = recipient.Id }, MapToDto(recipient));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation creating campaign recipient");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign recipient");
            return StatusCode(500, new { message = "Error creating campaign recipient", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing campaign recipient.
    ///
    /// FUNCTIONAL: Updates recipient information (email, status, etc.).
    /// TECHNICAL: Returns 404 if not found, 200 OK with updated entity.
    /// </summary>
    /// <param name="id">The recipient ID to update.</param>
    /// <param name="dto">The updated recipient data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated CampaignRecipientDto.</returns>
    /// <response code="200">Returns the updated recipient.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If recipient not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CampaignRecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CampaignRecipientDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipient = await _context.CampaignRecipients
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (recipient == null)
            {
                return NotFound(new { message = string.Format(RecipientNotFoundMessage, id) });
            }

            // Update fields
            recipient.Email = dto.Email;
            recipient.FirstName = dto.FirstName;
            recipient.LastName = dto.LastName;
            recipient.ContactId = dto.ContactId;
            recipient.AccountId = dto.AccountId;
            recipient.UpdatedAt = DateTime.UtcNow;

            // Update status if provided
            if (Enum.IsDefined(typeof(CampaignRecipientStatus), dto.Status))
            {
                recipient.Status = ((CampaignRecipientStatus)dto.Status).ToString();
            }

            _context.CampaignRecipients.Update(recipient);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated campaign recipient {Id}", id);

            return Ok(MapToDto(recipient));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Campaign recipient {Id} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign recipient {Id}", id);
            return StatusCode(500, new { message = "Error updating campaign recipient", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a campaign recipient (soft delete).
    ///
    /// FUNCTIONAL: Removes a recipient from the campaign.
    /// TECHNICAL: Sets IsDeleted flag, returns 204 No Content on success.
    /// </summary>
    /// <param name="id">The recipient ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Successfully deleted.</response>
    /// <response code="404">If recipient not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipient = await _context.CampaignRecipients
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (recipient == null)
            {
                return NotFound(new { message = string.Format(RecipientNotFoundMessage, id) });
            }

            // Soft delete
            recipient.IsDeleted = true;
            recipient.UpdatedAt = DateTime.UtcNow;

            _context.CampaignRecipients.Update(recipient);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted campaign recipient {Id}", id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Campaign recipient {Id} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign recipient {Id}", id);
            return StatusCode(500, new { message = "Error deleting campaign recipient", error = ex.Message });
        }
    }

    /// <summary>
    /// Add multiple recipients to a campaign.
    ///
    /// FUNCTIONAL: Bulk operation to add contacts/leads to a campaign.
    /// TECHNICAL: Uses ICampaignRecipientService for optimized bulk insert.
    /// </summary>
    /// <param name="campaignId">The campaign ID to add recipients to.</param>
    /// <param name="dto">The recipients to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of recipients added.</returns>
    /// <response code="200">Returns the count of added recipients.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If campaign not found.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpPost("campaign/{campaignId}/bulk")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddBulkRecipients(
        int campaignId,
        [FromBody] AddCampaignRecipientsDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedCount = await _campaignRecipientService.AddRecipientsAsync(
                campaignId, dto, cancellationToken);

            return Ok(new { addedCount, message = $"{addedCount} recipients added to campaign" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Campaign {CampaignId} not found", campaignId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding recipients to campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error adding recipients to campaign", error = ex.Message });
        }
    }

    /// <summary>
    /// Filter recipients by criteria.
    ///
    /// FUNCTIONAL: Search/filter recipients within a campaign.
    /// TECHNICAL: Uses ICampaignRecipientService filter functionality.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <param name="criteria">Filter criteria (name, email, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered list of recipients.</returns>
    /// <response code="200">Returns filtered recipients.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("campaign/{campaignId}/filter")]
    [ProducesResponseType(typeof(IEnumerable<CampaignRecipientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FilterRecipients(
        int campaignId,
        [FromQuery] string criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = await _campaignRecipientService.FilterAsync(
                campaignId, criteria ?? string.Empty, cancellationToken);

            return Ok(recipients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering recipients for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error filtering recipients", error = ex.Message });
        }
    }

    /// <summary>
    /// Get recipient count for a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recipient count.</returns>
    /// <response code="200">Returns the count.</response>
    /// <response code="500">If there was an internal error.</response>
    [HttpGet("campaign/{campaignId}/count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecipientCount(
        int campaignId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _campaignRecipientService.GetCountAsync(campaignId, cancellationToken);
            return Ok(new { campaignId, recipientCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recipient count for campaign {CampaignId}", campaignId);
            return StatusCode(500, new { message = "Error getting recipient count", error = ex.Message });
        }
    }

    /// <summary>
    /// Maps a CampaignRecipient entity to CampaignRecipientDto.
    /// </summary>
    private static CampaignRecipientDto MapToDto(CampaignRecipient recipient)
    {
        return new CampaignRecipientDto
        {
            Id = recipient.Id,
            CampaignId = recipient.CampaignId,
            ContactId = recipient.ContactId,
            AccountId = recipient.AccountId,
            Email = recipient.Email ?? string.Empty,
            FirstName = recipient.FirstName,
            LastName = recipient.LastName,
            Status = Enum.TryParse<CampaignRecipientStatus>(recipient.Status, out var status)
                ? (int)status
                : (int)CampaignRecipientStatus.Pending,
            AddedAt = recipient.CreatedAt,
            EngagedAt = recipient.FirstOpenedAt,
            Impressions = 0,
            Clicks = 0,
            Conversions = 0,
            Money = 0
        };
    }
}

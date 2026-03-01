// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API endpoints for managing account interactions.
/// </summary>
/// <remarks>
/// Interactions track all account touchpoints including calls, emails, meetings, and social media.
/// Supports linking to accounts, contacts, opportunities, and service requests.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InteractionsController : CrmControllerBase
{
    private const string InteractionNotFoundMessage = "Interaction not found";

    private readonly CrmDbContext _context;
    private readonly ILogger<InteractionsController> _logger;
    private readonly NormalizationService _normalization;

    public InteractionsController(CrmDbContext context, ILogger<InteractionsController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    /// <summary>
    /// Get all interactions with optional filtering.
    /// </summary>
    /// <param name="accountId">Filter by account ID.</param>
    /// <param name="opportunityId">Filter by opportunity ID.</param>
    /// <param name="assignedToUserId">Filter by assigned user ID.</param>
    /// <param name="interactionType">Filter by interaction type (Email, Phone, Meeting, etc.).</param>
    /// <param name="outcome">Filter by interaction outcome.</param>
    /// <param name="fromDate">Filter interactions from this date.</param>
    /// <param name="toDate">Filter interactions to this date.</param>
    /// <returns>List of interactions matching the filter criteria.</returns>
    /// <response code="200">Returns the list of interactions.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Interaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetInteractions(
        [FromQuery] int? accountId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? assignedToUserId = null,
        [FromQuery] InteractionType? interactionType = null,
        [FromQuery] InteractionOutcome? outcome = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _context.Interactions
            .Include(i => i.Account)
            .Include(i => i.Opportunity)
            .Include(i => i.AssignedToUser)
            .AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(i => i.AccountId == accountId);
        }
        if (opportunityId.HasValue)
        {
            query = query.Where(i => i.OpportunityId == opportunityId);
        }
        if (assignedToUserId.HasValue)
        {
            query = query.Where(i => i.AssignedToUserId == assignedToUserId);
        }
        if (interactionType.HasValue)
        {
            query = query.Where(i => i.InteractionType == interactionType);
        }
        if (outcome.HasValue)
        {
            query = query.Where(i => i.Outcome == outcome);
        }
        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate >= fromDate);
        }
        if (toDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate <= toDate);
        }

        var interactions = await query.OrderByDescending(i => i.InteractionDate).ToListAsync();
        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt))
            {
                it.Tags = nt;
            }
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf))
            {
                it.CustomFields = cf;
            }
        }

        return Ok(interactions);
    }

    /// <summary>
    /// Get an interaction by ID.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <returns>The interaction with the specified ID.</returns>
    /// <response code="200">Returns the interaction.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Interaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Interaction>> GetInteraction(int id)
    {
        var interaction = await _context.Interactions
            .Include(i => i.Account)
            .Include(i => i.Opportunity)
            .Include(i => i.AssignedToUser)
            .Include(i => i.Campaign)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (interaction == null)
        {
            return NotFound();
        }

        var nt = await _normalization.GetTagsAsync("Interaction", interaction.Id);
        if (!string.IsNullOrWhiteSpace(nt))
        {
            interaction.Tags = nt;
        }
        var cf = await _normalization.GetCustomFieldsAsync("Interaction", interaction.Id);
        if (!string.IsNullOrWhiteSpace(cf))
        {
            interaction.CustomFields = cf;
        }

        return Ok(interaction);
    }

    /// <summary>
    /// Create a new interaction.
    /// </summary>
    /// <param name="interaction">The interaction data to create.</param>
    /// <returns>The created interaction.</returns>
    /// <response code="201">Returns the newly created interaction.</response>
    /// <response code="400">Invalid interaction data provided.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Interaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Interaction>> CreateInteraction(Interaction interaction)
    {
        interaction.CreatedAt = DateTime.UtcNow;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (interaction.InteractionDate == default)
        {
            interaction.InteractionDate = DateTime.UtcNow;
        }

        _context.Interactions.Add(interaction);
        await _context.SaveChangesAsync();

        if (interaction.AccountId > 0)
        {
            var account = await _context.Accounts.FindAsync(interaction.AccountId);
            if (account != null)
            {
                account.LastActivityDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Interaction {InteractionId} created for account {AccountId}", interaction.Id, interaction.AccountId);
        return CreatedAtAction(nameof(GetInteraction), new { id = interaction.Id }, interaction);
    }

    /// <summary>
    /// Update an existing interaction.
    /// </summary>
    /// <param name="id">The interaction ID to update.</param>
    /// <param name="interaction">The updated interaction data.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Interaction was successfully updated.</response>
    /// <response code="400">Invalid data or ID mismatch.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInteraction(int id, Interaction interaction)
    {
        if (id != interaction.Id)
        {
            return BadRequest();
        }

        var existingInteraction = await _context.Interactions.FindAsync(id);
        if (existingInteraction == null)
        {
            return NotFound();
        }

        _context.Entry(existingInteraction).CurrentValues.SetValues(interaction);
        existingInteraction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Delete an interaction.
    /// </summary>
    /// <param name="id">The interaction ID to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Interaction was successfully deleted.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInteraction(int id)
    {
        var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound();
        }

        _context.Interactions.Remove(interaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Mark an interaction as completed.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">Optional completion details including outcome and notes.</param>
    /// <returns>The updated interaction.</returns>
    /// <response code="200">Returns the completed interaction.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(Interaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteInteraction(int id, [FromBody] CompleteInteractionRequest? request = null)
    {
        var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound();
        }

        interaction.IsCompleted = true;
        interaction.CompletedDate = DateTime.UtcNow;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (request != null)
        {
            if (request.Outcome.HasValue)
            {
                interaction.Outcome = request.Outcome.Value;
            }
            if (!string.IsNullOrEmpty(request.Notes))
            {
                interaction.Description = $"{interaction.Description}\n\nCompletion Notes: {request.Notes}".Trim();
            }
        }

        await _context.SaveChangesAsync();
        return Ok(interaction);
    }

    /// <summary>
    /// Quick log an interaction (auto-completes).
    /// </summary>
    /// <param name="request">The interaction details to log.</param>
    /// <returns>The created interaction.</returns>
    /// <response code="201">Returns the newly logged interaction.</response>
    /// <response code="400">Invalid interaction data provided.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("log")]
    [ProducesResponseType(typeof(Interaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Interaction>> LogInteraction([FromBody] LogInteractionRequest request)
    {
        var interaction = new Interaction
        {
            AccountId = request.AccountId,
            OpportunityId = request.OpportunityId,
            InteractionType = request.InteractionType,
            Direction = request.Direction,
            Subject = request.Subject,
            Description = request.Description ?? string.Empty,
            InteractionDate = DateTime.UtcNow,
            DurationMinutes = request.DurationMinutes,
            Outcome = request.Outcome,
            IsCompleted = true,
            CompletedDate = DateTime.UtcNow,
            AssignedToUserId = request.UserId,
            CreatedByUserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Interactions.Add(interaction);
        await _context.SaveChangesAsync();

        if (interaction.AccountId > 0)
        {
            var account = await _context.Accounts.FindAsync(interaction.AccountId);
            if (account != null)
            {
                account.LastActivityDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Quick interaction logged for account {AccountId}: {Type}", request.AccountId, request.InteractionType);
        return CreatedAtAction(nameof(GetInteraction), new { id = interaction.Id }, interaction);
    }

    /// <summary>
    /// Get interaction history for a specific account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="limit">Maximum number of interactions to return. Default is 50.</param>
    /// <returns>List of interactions for the account, ordered by most recent.</returns>
    /// <response code="200">Returns the account's interaction history.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("account/{accountId}/history")]
    [ProducesResponseType(typeof(IEnumerable<Interaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetAccountHistory(int accountId, [FromQuery] int limit = 50)
    {
        var interactions = await _context.Interactions
            .Include(i => i.AssignedToUser)
            .Include(i => i.Opportunity)
            .Where(i => i.AccountId == accountId)
            .OrderByDescending(i => i.InteractionDate)
            .Take(limit)
            .ToListAsync();

        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt))
            {
                it.Tags = nt;
            }
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf))
            {
                it.CustomFields = cf;
            }
        }

        return Ok(interactions);
    }

    /// <summary>
    /// Get interactions with follow-ups due within the next 7 days.
    /// </summary>
    /// <param name="userId">Optional filter by assigned user ID.</param>
    /// <returns>List of interactions with pending follow-ups.</returns>
    /// <response code="200">Returns interactions requiring follow-up.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("follow-ups")]
    [ProducesResponseType(typeof(IEnumerable<Interaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetFollowUps([FromQuery] int? userId = null)
    {
        var query = _context.Interactions
            .Include(i => i.Account)
            .Where(i => i.FollowUpDate != null && i.FollowUpDate <= DateTime.UtcNow.AddDays(7));

        if (userId.HasValue)
        {
            query = query.Where(i => i.AssignedToUserId == userId);
        }

        var interactions = await query
            .OrderBy(i => i.FollowUpDate)
            .ToListAsync();

        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt))
            {
                it.Tags = nt;
            }
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf))
            {
                it.CustomFields = cf;
            }
        }

        return Ok(interactions);
    }

    /// <summary>
    /// Get interaction statistics for a date range.
    /// </summary>
    /// <param name="fromDate">Start date for statistics. Defaults to 30 days ago.</param>
    /// <param name="toDate">End date for statistics. Defaults to today.</param>
    /// <returns>Statistics including counts by type, outcome, direction, and completion rates.</returns>
    /// <response code="200">Returns the interaction statistics.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetInteractionStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var query = _context.Interactions.Where(i => i.InteractionDate >= from && i.InteractionDate <= to);

        var stats = new
        {
            TotalInteractions = await query.CountAsync(),
            ByType = await query
                .GroupBy(i => i.InteractionType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            ByOutcome = await query
                .GroupBy(i => i.Outcome)
                .Select(g => new { Outcome = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            ByDirection = await query
                .GroupBy(i => i.Direction)
                .Select(g => new { Direction = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            AverageDuration = await query.AverageAsync(i => (double?)i.DurationMinutes) ?? 0,
            CompletionRate = await query.CountAsync(i => i.IsCompleted) * 100.0 / Math.Max(await query.CountAsync(), 1),
            InteractionsByDay = await query
                .GroupBy(i => i.InteractionDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync()
        };

        return Ok(stats);
    }

    /// <summary>
    /// Create a service request from an interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">The service request creation details.</param>
    /// <returns>The created service request.</returns>
    /// <response code="201">Returns the newly created service request.</response>
    /// <response code="400">Interaction is not linked to an account.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/create-service-request")]
    [ProducesResponseType(typeof(ServiceRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceRequest>> CreateServiceRequestFromInteraction(
        int id,
        [FromBody] CreateServiceRequestFromInteractionRequest request)
    {
                var interaction = await _context.Interactions
            .Include(i => i.Account)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (interaction == null)
        {
            return NotFound(new { message = InteractionNotFoundMessage });
        }

        if (interaction.AccountId <= 0)
        {
            return BadRequest(new { message = "Interaction must be linked to an account before creating a service request" });
        }

        // Determine priority
        var priority = ServiceRequestPriority.Medium;
        if (!string.IsNullOrEmpty(request.Priority) &&
            Enum.TryParse<ServiceRequestPriority>(request.Priority, true, out var parsedPriority))
        {
            priority = parsedPriority;
        }

        // If expediting, increase priority
        if (request.Expedite && priority < ServiceRequestPriority.Urgent)
        {
            priority = priority == ServiceRequestPriority.Medium ? ServiceRequestPriority.High : ServiceRequestPriority.Urgent;
        }

        var description = request.CopyInteractionDescription
            ? $"{request.Description}\n\n--- From Interaction ---\n{interaction.Description}".Trim()
            : request.Description ?? "";

        var serviceRequest = new ServiceRequest
        {
            TicketNumber = $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Subject = interaction.Subject ?? "Service Request from Interaction",
            Description = description,
            AccountId = interaction.AccountId,
            ContactId = interaction.ContactId,
            Status = ServiceRequestStatus.New,
            Priority = priority,
            Channel = GetChannelFromInteractionType(interaction.InteractionType),
            SourceInteractionId = interaction.Id,
            IsExpedited = request.Expedite,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        // Update interaction with linked service request
        interaction.CustomFields = System.Text.Json.JsonSerializer.Serialize(new
        {
            LinkedServiceRequestId = serviceRequest.Id
        });
        interaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created service request {ServiceRequestId} from interaction {InteractionId}",
            serviceRequest.Id, interaction.Id);

        return CreatedAtAction(nameof(GetInteraction), new { id = serviceRequest.Id }, serviceRequest);
    }

    /// <summary>
    /// Create a contact from an interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">The contact creation details.</param>
    /// <returns>The created contact and account IDs.</returns>
    /// <response code="200">Returns the created contact and account IDs.</response>
    /// <response code="400">Account ID is required or CreateAccountIfNeeded must be true.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/create-contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateContactFromInteraction(
        int id,
        [FromBody] CreateContactFromInteractionRequest request)
    {
                var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound(new { message = InteractionNotFoundMessage });
        }

        var accountId = request.AccountId ?? interaction.AccountId;

        // Create account if needed and none exists
        if (accountId <= 0 && request.CreateAccountIfNeeded)
        {
            var newAccount = new Account
            {
                Category = AccountCategory.Individual,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email ?? interaction.EmailAddress ?? "",
                Phone = request.Phone ?? interaction.PhoneNumber ?? "",
                LifecycleStage = AccountLifecycleStage.Lead,
                LeadSource = $"Interaction-{interaction.InteractionType}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();
            accountId = newAccount.Id;
        }

        if (accountId <= 0)
        {
            return BadRequest(new { message = "AccountId is required or CreateAccountIfNeeded must be true" });
        }

        var contact = new Contact
        {
            AccountId = accountId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailPrimary = request.Email ?? interaction.EmailAddress,
            PhonePrimary = request.Phone ?? interaction.PhoneNumber,
            JobTitle = request.Title,
            Status = ContactStatus.Active,
            DateAdded = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        // Link interaction to the new contact
        interaction.ContactId = contact.Id;
        if (interaction.AccountId <= 0)
        {
            interaction.AccountId = accountId;
        }
        interaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created contact {ContactId} from interaction {InteractionId}",
            contact.Id, interaction.Id);

        return Ok(new { contactId = contact.Id, accountId = accountId });
    }

    /// <summary>
    /// Link an interaction to entities (account, contact, opportunity, service request).
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">The linking details including entity IDs.</param>
    /// <returns>The updated interaction.</returns>
    /// <response code="200">Returns the updated interaction with links.</response>
    /// <response code="400">Referenced entity not found.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/link")]
    [ProducesResponseType(typeof(Interaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LinkInteraction(int id, [FromBody] LinkInteractionRequest request)
    {
                var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound(new { message = InteractionNotFoundMessage });
        }

        if (request.AccountId.HasValue)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId.Value);
            if (account == null)
            {
                return BadRequest(new { message = "Account not found" });
            }
            interaction.AccountId = request.AccountId.Value;
        }

        if (request.ContactId.HasValue)
        {
            var contact = await _context.Contacts.FindAsync(request.ContactId.Value);
            if (contact == null)
            {
                return BadRequest(new { message = "Contact not found" });
            }
            interaction.ContactId = request.ContactId.Value;
        }

        if (request.OpportunityId.HasValue)
        {
            var opportunity = await _context.Opportunities.FindAsync(request.OpportunityId.Value);
            if (opportunity == null)
            {
                return BadRequest(new { message = "Opportunity not found" });
            }
            interaction.OpportunityId = request.OpportunityId.Value;
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            interaction.Description = $"{interaction.Description}\n\n[Link Note]: {request.Notes}".Trim();
        }

        interaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Linked interaction {InteractionId} to entities", id);

        return Ok(interaction);
    }

    /// <summary>
    /// Add a note to an interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">The note content and type (internal or external).</param>
    /// <returns>Success confirmation.</returns>
    /// <response code="200">Note was successfully added.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/notes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddNote(int id, [FromBody] AddInteractionNoteRequest request)
    {
                var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound(new { message = InteractionNotFoundMessage });
        }

        var notePrefix = request.IsInternal ? "[Internal Note]" : "[Note]";
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var newNote = $"\n\n{notePrefix} ({timestamp}): {request.Note}";

        interaction.Description = (interaction.Description ?? "") + newNote;
        interaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Note added successfully" });
    }

    /// <summary>
    /// Update tags for an interaction.
    /// </summary>
    /// <param name="id">The interaction ID.</param>
    /// <param name="request">The list of tags to apply.</param>
    /// <returns>Success confirmation with updated tags.</returns>
    /// <response code="200">Tags were successfully updated.</response>
    /// <response code="404">Interaction not found.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTags(int id, [FromBody] TagInteractionRequest request)
    {
                var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
        {
            return NotFound(new { message = InteractionNotFoundMessage });
        }

        interaction.Tags = string.Join(",", request.Tags);
        interaction.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, tags = request.Tags });
    }

    /// <summary>
    /// Get interactions that need attention (follow-ups due, unlinked, etc.).
    /// </summary>
    /// <param name="limit">Maximum number of interactions to return. Default is 50.</param>
    /// <returns>List of interactions requiring attention, prioritized by urgency.</returns>
    /// <response code="200">Returns interactions needing attention.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("needs-attention")]
    [ProducesResponseType(typeof(IEnumerable<Interaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetNeedsAttention([FromQuery] int limit = 50)
    {
        var now = DateTime.UtcNow;

        var interactions = await _context.Interactions
            .Include(i => i.Account)
            .Where(i =>
                // Unlinked to account
                (i.AccountId <= 0) ||
                // Follow-up overdue
                (i.FollowUpDate != null && i.FollowUpDate < now && !i.IsCompleted) ||
                // High priority not completed
                (i.Priority >= 3 && !i.IsCompleted && i.CreatedAt < now.AddHours(-4)))
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => i.FollowUpDate)
            .Take(limit)
            .ToListAsync();

        return Ok(interactions);
    }

    private static ServiceRequestChannel GetChannelFromInteractionType(InteractionType type)
    {
        return type switch
        {
            InteractionType.Email => ServiceRequestChannel.Email,
            InteractionType.Phone => ServiceRequestChannel.Phone,
            InteractionType.Chat => ServiceRequestChannel.LiveChat,
            InteractionType.SocialMedia => ServiceRequestChannel.SocialMedia,
            InteractionType.WebForm => ServiceRequestChannel.SelfServicePortal,
            InteractionType.InPerson => ServiceRequestChannel.InPerson,
            _ => ServiceRequestChannel.API
        };
    }
}

public class CompleteInteractionRequest
{
    public InteractionOutcome? Outcome { get; set; }
    public string? Notes { get; set; }
}

public class LogInteractionRequest
{
    public int AccountId { get; set; }
    public int? OpportunityId { get; set; }
    public InteractionType InteractionType { get; set; }
    public InteractionDirection Direction { get; set; } = InteractionDirection.Outbound;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public InteractionOutcome Outcome { get; set; } = InteractionOutcome.None;
    public int? UserId { get; set; }
}

public class CreateServiceRequestFromInteractionRequest
{
    public int InteractionId { get; set; }
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public string? Priority { get; set; }
    public string? Description { get; set; }
    public bool CopyInteractionDescription { get; set; } = true;
    public bool Expedite { get; set; } = false;
}

public class CreateContactFromInteractionRequest
{
    public int InteractionId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public int? AccountId { get; set; }
    public bool CreateAccountIfNeeded { get; set; } = false;
}

public class LinkInteractionRequest
{
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
    public int? ServiceRequestId { get; set; }
    public string? Notes { get; set; }
}

public class AddInteractionNoteRequest
{
    public string Note { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
}

public class TagInteractionRequest
{
    public List<string> Tags { get; set; } = new();
}

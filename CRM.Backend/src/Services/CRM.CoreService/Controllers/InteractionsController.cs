using CRM.Core.Entities;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InteractionsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<InteractionsController> _logger;
    private readonly NormalizationService _normalization;

    public InteractionsController(CrmDbContext context, ILogger<InteractionsController> logger, NormalizationService normalization)
    {
        _context = context;
        _logger = logger;
        _normalization = normalization;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetInteractions(
        [FromQuery] int? customerId = null,
        [FromQuery] int? opportunityId = null,
        [FromQuery] int? assignedToUserId = null,
        [FromQuery] InteractionType? interactionType = null,
        [FromQuery] InteractionOutcome? outcome = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _context.Interactions
            .Include(i => i.Customer)
            .Include(i => i.Opportunity)
            .Include(i => i.AssignedToUser)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId);
        if (opportunityId.HasValue)
            query = query.Where(i => i.OpportunityId == opportunityId);
        if (assignedToUserId.HasValue)
            query = query.Where(i => i.AssignedToUserId == assignedToUserId);
        if (interactionType.HasValue)
            query = query.Where(i => i.InteractionType == interactionType);
        if (outcome.HasValue)
            query = query.Where(i => i.Outcome == outcome);
        if (fromDate.HasValue)
            query = query.Where(i => i.InteractionDate >= fromDate);
        if (toDate.HasValue)
            query = query.Where(i => i.InteractionDate <= toDate);

        var interactions = await query.OrderByDescending(i => i.InteractionDate).ToListAsync();
        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt)) it.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf)) it.CustomFields = cf;
        }

        return Ok(interactions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Interaction>> GetInteraction(int id)
    {
        var interaction = await _context.Interactions
            .Include(i => i.Customer)
            .Include(i => i.Opportunity)
            .Include(i => i.AssignedToUser)
            .Include(i => i.Campaign)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (interaction == null)
            return NotFound();

        var nt = await _normalization.GetTagsAsync("Interaction", interaction.Id);
        if (!string.IsNullOrWhiteSpace(nt)) interaction.Tags = nt;
        var cf = await _normalization.GetCustomFieldsAsync("Interaction", interaction.Id);
        if (!string.IsNullOrWhiteSpace(cf)) interaction.CustomFields = cf;

        return Ok(interaction);
    }

    [HttpPost]
    public async Task<ActionResult<Interaction>> CreateInteraction(Interaction interaction)
    {
        interaction.CreatedAt = DateTime.UtcNow;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (interaction.InteractionDate == default)
            interaction.InteractionDate = DateTime.UtcNow;

        _context.Interactions.Add(interaction);
        await _context.SaveChangesAsync();

        if (interaction.CustomerId > 0)
        {
            var customer = await _context.Customers.FindAsync(interaction.CustomerId);
            if (customer != null)
            {
                customer.LastActivityDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Interaction {InteractionId} created for customer {CustomerId}", interaction.Id, interaction.CustomerId);
        return CreatedAtAction(nameof(GetInteraction), new { id = interaction.Id }, interaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInteraction(int id, Interaction interaction)
    {
        if (id != interaction.Id)
            return BadRequest();

        var existingInteraction = await _context.Interactions.FindAsync(id);
        if (existingInteraction == null)
            return NotFound();

        _context.Entry(existingInteraction).CurrentValues.SetValues(interaction);
        existingInteraction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInteraction(int id)
    {
        var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
            return NotFound();

        _context.Interactions.Remove(interaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteInteraction(int id, [FromBody] CompleteInteractionRequest? request = null)
    {
        var interaction = await _context.Interactions.FindAsync(id);
        if (interaction == null)
            return NotFound();

        interaction.IsCompleted = true;
        interaction.CompletedDate = DateTime.UtcNow;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (request != null)
        {
            if (request.Outcome.HasValue)
                interaction.Outcome = request.Outcome.Value;
            if (!string.IsNullOrEmpty(request.Notes))
                interaction.Description = $"{interaction.Description}\n\nCompletion Notes: {request.Notes}".Trim();
        }

        await _context.SaveChangesAsync();
        return Ok(interaction);
    }

    [HttpPost("log")]
    public async Task<ActionResult<Interaction>> LogInteraction([FromBody] LogInteractionRequest request)
    {
        var interaction = new Interaction
        {
            CustomerId = request.CustomerId,
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

        if (interaction.CustomerId > 0)
        {
            var customer = await _context.Customers.FindAsync(interaction.CustomerId);
            if (customer != null)
            {
                customer.LastActivityDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Quick interaction logged for customer {CustomerId}: {Type}", request.CustomerId, request.InteractionType);
        return CreatedAtAction(nameof(GetInteraction), new { id = interaction.Id }, interaction);
    }

    [HttpGet("customer/{customerId}/history")]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetCustomerHistory(int customerId, [FromQuery] int limit = 50)
    {
        var interactions = await _context.Interactions
            .Include(i => i.AssignedToUser)
            .Include(i => i.Opportunity)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InteractionDate)
            .Take(limit)
            .ToListAsync();

        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt)) it.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf)) it.CustomFields = cf;
        }

        return Ok(interactions);
    }

    [HttpGet("follow-ups")]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetFollowUps([FromQuery] int? userId = null)
    {
        var query = _context.Interactions
            .Include(i => i.Customer)
            .Where(i => i.FollowUpDate != null && i.FollowUpDate <= DateTime.UtcNow.AddDays(7));

        if (userId.HasValue)
            query = query.Where(i => i.AssignedToUserId == userId);

        var interactions = await query
            .OrderBy(i => i.FollowUpDate)
            .ToListAsync();

        foreach (var it in interactions)
        {
            var nt = await _normalization.GetTagsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(nt)) it.Tags = nt;
            var cf = await _normalization.GetCustomFieldsAsync("Interaction", it.Id);
            if (!string.IsNullOrWhiteSpace(cf)) it.CustomFields = cf;
        }

        return Ok(interactions);
    }

    [HttpGet("stats")]
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
    /// Create a service request from an interaction
    /// </summary>
    [HttpPost("{id}/create-service-request")]
    public async Task<ActionResult<ServiceRequest>> CreateServiceRequestFromInteraction(
        int id, 
        [FromBody] CreateServiceRequestFromInteractionRequest request)
    {
        try
        {
            var interaction = await _context.Interactions
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (interaction == null)
                return NotFound(new { message = "Interaction not found" });

            if (interaction.CustomerId <= 0)
                return BadRequest(new { message = "Interaction must be linked to a customer before creating a service request" });

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
                CustomerId = interaction.CustomerId,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service request from interaction {InteractionId}", id);
            return StatusCode(500, new { message = "Error creating service request" });
        }
    }

    /// <summary>
    /// Create a contact from an interaction
    /// </summary>
    [HttpPost("{id}/create-contact")]
    public async Task<ActionResult> CreateContactFromInteraction(
        int id, 
        [FromBody] CreateContactFromInteractionRequest request)
    {
        try
        {
            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null)
                return NotFound(new { message = "Interaction not found" });

            var customerId = request.CustomerId ?? interaction.CustomerId;

            // Create customer if needed and none exists
            if (customerId <= 0 && request.CreateCustomerIfNeeded)
            {
                var newCustomer = new Customer
                {
                    Category = CustomerCategory.Individual,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email ?? interaction.EmailAddress ?? "",
                    Phone = request.Phone ?? interaction.PhoneNumber ?? "",
                    LifecycleStage = CustomerLifecycleStage.Lead,
                    LeadSource = $"Interaction-{interaction.InteractionType}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
                customerId = newCustomer.Id;
            }

            if (customerId <= 0)
                return BadRequest(new { message = "CustomerId is required or CreateCustomerIfNeeded must be true" });

            var contact = new Contact
            {
                AccountId = customerId,
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
            if (interaction.CustomerId <= 0)
                interaction.CustomerId = customerId;
            interaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created contact {ContactId} from interaction {InteractionId}", 
                contact.Id, interaction.Id);

            return Ok(new { contactId = contact.Id, customerId = customerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact from interaction {InteractionId}", id);
            return StatusCode(500, new { message = "Error creating contact" });
        }
    }

    /// <summary>
    /// Link an interaction to entities (customer, contact, opportunity, service request)
    /// </summary>
    [HttpPost("{id}/link")]
    public async Task<IActionResult> LinkInteraction(int id, [FromBody] LinkInteractionRequest request)
    {
        try
        {
            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null)
                return NotFound(new { message = "Interaction not found" });

            if (request.CustomerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(request.CustomerId.Value);
                if (customer == null)
                    return BadRequest(new { message = "Customer not found" });
                interaction.CustomerId = request.CustomerId.Value;
            }

            if (request.ContactId.HasValue)
            {
                var contact = await _context.Contacts.FindAsync(request.ContactId.Value);
                if (contact == null)
                    return BadRequest(new { message = "Contact not found" });
                interaction.ContactId = request.ContactId.Value;
            }

            if (request.OpportunityId.HasValue)
            {
                var opportunity = await _context.Opportunities.FindAsync(request.OpportunityId.Value);
                if (opportunity == null)
                    return BadRequest(new { message = "Opportunity not found" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking interaction {InteractionId}", id);
            return StatusCode(500, new { message = "Error linking interaction" });
        }
    }

    /// <summary>
    /// Add a note to an interaction
    /// </summary>
    [HttpPost("{id}/notes")]
    public async Task<IActionResult> AddNote(int id, [FromBody] AddInteractionNoteRequest request)
    {
        try
        {
            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null)
                return NotFound(new { message = "Interaction not found" });

            var notePrefix = request.IsInternal ? "[Internal Note]" : "[Note]";
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
            var newNote = $"\n\n{notePrefix} ({timestamp}): {request.Note}";

            interaction.Description = (interaction.Description ?? "") + newNote;
            interaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Note added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to interaction {InteractionId}", id);
            return StatusCode(500, new { message = "Error adding note" });
        }
    }

    /// <summary>
    /// Update tags for an interaction
    /// </summary>
    [HttpPost("{id}/tags")]
    public async Task<IActionResult> UpdateTags(int id, [FromBody] TagInteractionRequest request)
    {
        try
        {
            var interaction = await _context.Interactions.FindAsync(id);
            if (interaction == null)
                return NotFound(new { message = "Interaction not found" });

            interaction.Tags = string.Join(",", request.Tags);
            interaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, tags = request.Tags });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tags for interaction {InteractionId}", id);
            return StatusCode(500, new { message = "Error updating tags" });
        }
    }

    /// <summary>
    /// Get interactions that need attention (follow-ups due, unlinked, etc.)
    /// </summary>
    [HttpGet("needs-attention")]
    public async Task<ActionResult<IEnumerable<Interaction>>> GetNeedsAttention([FromQuery] int limit = 50)
    {
        var now = DateTime.UtcNow;
        
        var interactions = await _context.Interactions
            .Include(i => i.Customer)
            .Where(i => 
                // Unlinked to customer
                (i.CustomerId <= 0) ||
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
    public int CustomerId { get; set; }
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
    public int? CustomerId { get; set; }
    public bool CreateCustomerIfNeeded { get; set; } = false;
}

public class LinkInteractionRequest
{
    public int? CustomerId { get; set; }
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

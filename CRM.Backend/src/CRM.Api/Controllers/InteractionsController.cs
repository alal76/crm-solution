using CRM.Core.Entities;
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

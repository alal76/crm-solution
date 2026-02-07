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
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing customer interactions
/// </summary>
public class InteractionService : IInteractionService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<InteractionService> _logger;

    public InteractionService(ICrmDbContext dbContext, ILogger<InteractionService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Interaction>> GetInteractionsAsync(
        int? customerId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        InteractionType? interactionType = null,
        InteractionOutcome? outcome = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        _logger.LogDebug("Getting interactions with filters");

        var query = _dbContext.Interactions
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(i => i.AccountId == customerId.Value);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(i => i.OpportunityId == opportunityId.Value);
        }

        if (assignedToUserId.HasValue)
        {
            query = query.Where(i => i.AssignedToUserId == assignedToUserId.Value);
        }

        if (interactionType.HasValue)
        {
            query = query.Where(i => i.InteractionType == interactionType.Value);
        }

        if (outcome.HasValue)
        {
            query = query.Where(i => i.Outcome == outcome.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.InteractionDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(i => i.InteractionDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Interaction?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Getting interaction by ID: {InteractionId}", id);

        return await _dbContext.Interactions
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<Interaction> CreateAsync(Interaction interaction)
    {
        _logger.LogDebug("Creating interaction for account {AccountId}", interaction.AccountId);

        try
        {
            interaction.CreatedAt = DateTime.UtcNow;
            interaction.UpdatedAt = DateTime.UtcNow;

            _dbContext.Interactions.Add(interaction);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created interaction {InteractionId} for account {AccountId}", 
                interaction.Id, interaction.AccountId);

            return interaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating interaction for account {AccountId}", interaction.AccountId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Interaction interaction)
    {
        _logger.LogDebug("Updating interaction {InteractionId}", id);

        try
        {
            var existing = await _dbContext.Interactions
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (existing == null)
            {
                _logger.LogWarning("Interaction {InteractionId} not found", id);
                return false;
            }

            // Update properties
            existing.Subject = interaction.Subject;
            existing.Description = interaction.Description;
            existing.InteractionType = interaction.InteractionType;
            existing.Direction = interaction.Direction;
            existing.InteractionDate = interaction.InteractionDate;
            existing.DurationMinutes = interaction.DurationMinutes;
            existing.Outcome = interaction.Outcome;
            existing.IsCompleted = interaction.IsCompleted;
            existing.FollowUpDate = interaction.FollowUpDate;
            existing.FollowUpNotes = interaction.FollowUpNotes;
            existing.MeetingNotes = interaction.MeetingNotes;
            existing.AccountId = interaction.AccountId;
            existing.ContactId = interaction.ContactId;
            existing.OpportunityId = interaction.OpportunityId;
            existing.AssignedToUserId = interaction.AssignedToUserId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated interaction {InteractionId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating interaction {InteractionId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting interaction {InteractionId}", id);

        try
        {
            var existing = await _dbContext.Interactions
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (existing == null)
            {
                _logger.LogWarning("Interaction {InteractionId} not found", id);
                return false;
            }

            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted interaction {InteractionId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting interaction {InteractionId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Interaction?> CompleteAsync(int id, InteractionCompletionRequest? request = null)
    {
        _logger.LogDebug("Completing interaction {InteractionId}", id);

        try
        {
            var existing = await _dbContext.Interactions
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (existing == null)
            {
                _logger.LogWarning("Interaction {InteractionId} not found", id);
                return null;
            }

            existing.IsCompleted = true;
            existing.CompletedDate = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            if (request != null)
            {
                if (request.Outcome.HasValue)
                {
                    existing.Outcome = request.Outcome.Value;
                }
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    existing.MeetingNotes = string.IsNullOrEmpty(existing.MeetingNotes) 
                        ? request.Notes 
                        : $"{existing.MeetingNotes}\n\n{request.Notes}";
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Completed interaction {InteractionId}", id);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing interaction {InteractionId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Interaction> LogAsync(InteractionLogRequest request)
    {
        _logger.LogDebug("Logging interaction for account {AccountId}", request.CustomerId);

        try
        {
            var interaction = new Interaction
            {
                AccountId = request.CustomerId,
                OpportunityId = request.OpportunityId,
                InteractionType = request.InteractionType,
                Direction = request.Direction,
                Subject = request.Subject,
                Description = request.Description ?? string.Empty,
                DurationMinutes = request.DurationMinutes,
                Outcome = request.Outcome ?? InteractionOutcome.None,
                InteractionDate = DateTime.UtcNow,
                IsCompleted = true,
                CompletedDate = DateTime.UtcNow,
                AssignedToUserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Interactions.Add(interaction);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Logged interaction {InteractionId} for account {AccountId}", 
                interaction.Id, request.CustomerId);

            return interaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging interaction for account {AccountId}", request.CustomerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<InteractionStatistics> GetStatisticsAsync(
        int? customerId = null, 
        DateTime? fromDate = null, 
        DateTime? toDate = null)
    {
        _logger.LogDebug("Getting interaction statistics");

        try
        {
            var query = _dbContext.Interactions
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(i => i.AccountId == customerId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.InteractionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.InteractionDate <= toDate.Value);
            }

            var interactions = await query.ToListAsync();

            return new InteractionStatistics
            {
                TotalInteractions = interactions.Count,
                Calls = interactions.Count(i => i.InteractionType == InteractionType.Phone),
                Emails = interactions.Count(i => i.InteractionType == InteractionType.Email),
                Meetings = interactions.Count(i => i.InteractionType == InteractionType.Meeting),
                Successful = interactions.Count(i => i.Outcome == InteractionOutcome.Successful),
                FollowUpRequired = interactions.Count(i => i.FollowUpDate.HasValue && !i.IsCompleted),
                AverageDurationMinutes = interactions.Where(i => i.DurationMinutes.HasValue)
                    .DefaultIfEmpty()
                    .Average(i => i?.DurationMinutes ?? 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting interaction statistics");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Interaction>> GetCustomerHistoryAsync(int customerId, int limit = 50)
    {
        _logger.LogDebug("Getting interaction history for account {AccountId}", customerId);

        return await _dbContext.Interactions
            .Where(i => i.AccountId == customerId && !i.IsDeleted)
            .OrderByDescending(i => i.InteractionDate)
            .Take(limit)
            .ToListAsync();
    }
}

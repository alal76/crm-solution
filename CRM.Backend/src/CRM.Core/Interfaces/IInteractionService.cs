// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing account interactions
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// Get interactions with optional filtering
    /// </summary>
    Task<IEnumerable<Interaction>> GetInteractionsAsync(
        int? accountId = null,
        int? opportunityId = null,
        int? assignedToUserId = null,
        InteractionType? interactionType = null,
        InteractionOutcome? outcome = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get an interaction by ID
    /// </summary>
    Task<Interaction?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new interaction
    /// </summary>
    Task<Interaction> CreateAsync(Interaction interaction);

    /// <summary>
    /// Update an existing interaction
    /// </summary>
    Task<bool> UpdateAsync(int id, Interaction interaction);

    /// <summary>
    /// Delete an interaction
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Mark an interaction as complete
    /// </summary>
    Task<Interaction?> CompleteAsync(int id, InteractionCompletionRequest? request = null);

    /// <summary>
    /// Log a quick interaction
    /// </summary>
    Task<Interaction> LogAsync(InteractionLogRequest request);

    /// <summary>
    /// Get interaction statistics
    /// </summary>
    Task<InteractionStatistics> GetStatisticsAsync(int? accountId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get account interaction history
    /// </summary>
    Task<IEnumerable<Interaction>> GetAccountHistoryAsync(int accountId, int limit = 50);
}

/// <summary>
/// Interaction completion request
/// </summary>
public class InteractionCompletionRequest
{
    public InteractionOutcome? Outcome { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Interaction log request
/// </summary>
public class InteractionLogRequest
{
    public int AccountId { get; set; }
    public int? OpportunityId { get; set; }
    public InteractionType InteractionType { get; set; }
    public InteractionDirection Direction { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public InteractionOutcome? Outcome { get; set; }
    public int? UserId { get; set; }
}

/// <summary>
/// Interaction statistics DTO
/// </summary>
public class InteractionStatistics
{
    public int TotalInteractions { get; set; }
    public int Calls { get; set; }
    public int Emails { get; set; }
    public int Meetings { get; set; }
    public int Successful { get; set; }
    public int FollowUpRequired { get; set; }
    public double AverageDurationMinutes { get; set; }
}

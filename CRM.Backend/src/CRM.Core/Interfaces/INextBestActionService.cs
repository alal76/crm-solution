// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for generating next best action recommendations.
/// Implements TODO-AI-04.
/// </summary>
public interface INextBestActionService
{
    /// <summary>
    /// Gets recommended next-best-actions for a given account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ordered list of recommended actions.</returns>
    Task<IEnumerable<NextBestActionDto>> GetRecommendationsAsync(int accountId, CancellationToken ct = default);
}

/// <summary>
/// Type of recommended CRM action.
/// </summary>
public enum NextBestActionType
{
    /// <summary>Schedule a phone call with the account.</summary>
    ScheduleCall = 0,

    /// <summary>Send a follow-up email.</summary>
    SendEmail = 1,

    /// <summary>Create or update an opportunity.</summary>
    CreateOpportunity = 2,

    /// <summary>Assign or escalate a support ticket.</summary>
    AssignTicket = 3,

    /// <summary>Send a renewal reminder.</summary>
    SendRenewalReminder = 4,

    /// <summary>Schedule a demo or meeting.</summary>
    ScheduleDemo = 5
}

/// <summary>
/// A single recommended next-best-action.
/// </summary>
public class NextBestActionDto
{
    /// <summary>Type of action recommended.</summary>
    public NextBestActionType ActionType { get; set; }

    /// <summary>Priority rank from 1 (highest) to 5 (lowest).</summary>
    public int Priority { get; set; }

    /// <summary>Rationale explaining why this action was recommended.</summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>Optional suggested due date for the action.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Human-readable label for the action type.</summary>
    private string? _actionLabel;

    /// <summary>
    /// Human-readable label for the action. Defaults to the action type name but
    /// can be overridden with a custom label during object initialisation.
    /// </summary>
    public string ActionLabel
    {
        get => _actionLabel ?? ActionType.ToString();
        set => _actionLabel = value;
    }
}

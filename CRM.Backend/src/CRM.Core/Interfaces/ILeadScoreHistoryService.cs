// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for recording, querying, and analysing lead score history.
/// FEAT-AISCORING: AI Lead Scoring Real-time Triggers
/// </summary>
public interface ILeadScoreHistoryService
{
    /// <summary>
    /// Returns the most recent score history entries for a lead ordered by <c>ScoredAt</c> descending.
    /// </summary>
    /// <param name="leadId">Lead primary key.</param>
    /// <param name="limit">Maximum records to return (default 20).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<LeadScoreHistoryDto>> GetHistoryAsync(
        int leadId,
        int limit = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Builds a full score explanation including component breakdown, recent history, and trend.
    /// Returns <c>null</c> when the lead does not exist.
    /// </summary>
    Task<LeadScoreExplanationDto?> GetExplanationAsync(
        int leadId,
        CancellationToken ct = default);

    /// <summary>
    /// Appends a new <see cref="CRM.Core.Entities.LeadScoreHistory"/> row for the given lead.
    /// </summary>
    /// <param name="leadId">Lead primary key.</param>
    /// <param name="newScore">Score value after the change.</param>
    /// <param name="previousScore">Score value before the change.</param>
    /// <param name="reason">Short reason string: "auto_score", "decay", "manual", "lead_updated", etc.</param>
    /// <param name="scoredBy">"system", "user", or "decay".</param>
    /// <param name="components">Optional component snapshot — serialised to JSON.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordScoreAsync(
        int leadId,
        int newScore,
        int previousScore,
        string reason,
        string scoredBy,
        object? components = null,
        CancellationToken ct = default);

    /// <summary>
    /// Applies a 5 % score decay to the lead when <c>LastScoreDecayDate</c> is older than
    /// 14 days (or null). Updates <c>LastScoreDecayDate</c> and records a "decay" history entry.
    /// Does nothing when the lead was already decayed within the last 14 days or has a zero score.
    /// </summary>
    Task ApplyDecayAsync(int leadId, CancellationToken ct = default);
}

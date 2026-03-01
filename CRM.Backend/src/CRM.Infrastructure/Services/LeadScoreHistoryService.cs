// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implements lead score history recording, querying, explanation building, and decay logic.
/// FEAT-AISCORING: AI Lead Scoring Real-time Triggers
/// </summary>
public class LeadScoreHistoryService : ILeadScoreHistoryService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<LeadScoreHistoryService> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public LeadScoreHistoryService(ICrmDbContext context, ILogger<LeadScoreHistoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LeadScoreHistoryDto>> GetHistoryAsync(
        int leadId,
        int limit = 20,
        CancellationToken ct = default)
    {
        var rows = await _context.LeadScoreHistories
            .Where(h => h.LeadId == leadId)
            .OrderByDescending(h => h.ScoredAt)
            .Take(limit)
            .ToListAsync(ct);

        return rows.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<LeadScoreExplanationDto?> GetExplanationAsync(
        int leadId,
        CancellationToken ct = default)
    {
        var lead = await _context.Set<Lead>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct);

        if (lead == null)
        {
            return null;
        }

        var recentHistory = await _context.LeadScoreHistories
            .Where(h => h.LeadId == leadId)
            .OrderByDescending(h => h.ScoredAt)
            .Take(5)
            .ToListAsync(ct);

        var trend = CalculateTrend(recentHistory.Select(h => h.Score).ToList());

        return new LeadScoreExplanationDto
        {
            LeadId = leadId,
            CurrentScore = lead.FitScore,
            Components = new LeadScoreComponents
            {
                Fit = lead.FitScore,
                Engagement = lead.EngagementScore,
                Budget = lead.BudgetScore,
                Authority = lead.AuthorityScore,
                Need = lead.NeedScore,
                Timeline = lead.TimelineScore,
                Metrics = lead.MetricsScore,
                EconomicBuyer = lead.EconomicBuyerScore,
                DecisionCriteria = lead.DecisionCriteriaScore,
                DecisionProcess = lead.DecisionProcessScore,
                IdentifyPain = lead.IdentifyPainScore,
                Champion = lead.ChampionScore,
            },
            QualificationFramework = lead.QualificationFrameworkType.ToString(),
            RecentHistory = recentHistory.Select(MapToDto).ToList(),
            Trend = trend,
        };
    }

    /// <inheritdoc/>
    public async Task RecordScoreAsync(
        int leadId,
        int newScore,
        int previousScore,
        string reason,
        string scoredBy,
        object? components = null,
        CancellationToken ct = default)
    {
        string? componentsJson = null;
        if (components != null)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(components, _jsonOpts);
                // Truncate to 2000 chars as per column constraint
                componentsJson = serialized.Length > 2000
                    ? serialized[..2000]
                    : serialized;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize score components for lead {LeadId}", leadId);
            }
        }

        var entry = new LeadScoreHistory
        {
            LeadId = leadId,
            Score = newScore,
            PreviousScore = previousScore,
            Delta = newScore - previousScore,
            Reason = reason.Length > 200 ? reason[..200] : reason,
            ScoreComponentsJson = componentsJson,
            ScoredAt = DateTime.UtcNow,
            ScoredBy = scoredBy.Length > 20 ? scoredBy[..20] : scoredBy,
        };

        _context.LeadScoreHistories.Add(entry);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug(
            "Recorded score change for Lead {LeadId}: {PreviousScore} → {NewScore} ({Reason})",
            leadId, previousScore, newScore, reason);
    }

    /// <inheritdoc/>
    public async Task ApplyDecayAsync(int leadId, CancellationToken ct = default)
    {
        var lead = await _context.Set<Lead>()
            .FirstOrDefaultAsync(l => l.Id == leadId && !l.IsDeleted, ct);

        if (lead == null || lead.FitScore <= 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var decayThreshold = now.AddDays(-14);

        // Skip if already decayed within the last 14 days
        if (lead.LastScoreDecayDate.HasValue && lead.LastScoreDecayDate >= decayThreshold)
        {
            return;
        }

        var previousScore = lead.FitScore;
        // Apply 5 % decay, round down, floor at 0
        var newScore = (int)Math.Max(0, Math.Floor(previousScore * 0.95));

        lead.FitScore = newScore;
        lead.Score = newScore;
        lead.LastScoreDecayDate = now;
        lead.UpdatedAt = now;

        await _context.SaveChangesAsync(ct);

        await RecordScoreAsync(
            leadId,
            newScore,
            previousScore,
            "decay",
            "decay",
            ct: ct);

        _logger.LogInformation(
            "Decay applied to Lead {LeadId}: {PreviousScore} → {NewScore}",
            leadId, previousScore, newScore);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static LeadScoreHistoryDto MapToDto(LeadScoreHistory h)
    {
        Dictionary<string, int>? components = null;
        if (!string.IsNullOrWhiteSpace(h.ScoreComponentsJson))
        {
            try
            {
                components = JsonSerializer.Deserialize<Dictionary<string, int>>(
                    h.ScoreComponentsJson,
                    _jsonOpts);
            }
            catch { /* ignore malformed JSON */ }
        }

        return new LeadScoreHistoryDto
        {
            Id = h.Id,
            LeadId = h.LeadId,
            Score = h.Score,
            PreviousScore = h.PreviousScore,
            Delta = h.Delta,
            Reason = h.Reason,
            ScoreComponents = components,
            ScoredAt = h.ScoredAt,
            ScoredBy = h.ScoredBy,
        };
    }

    /// <summary>
    /// Compares the average of the newer half vs the older half of the score list.
    /// Scores arrive ordered newest-first.
    /// </summary>
    private static string CalculateTrend(List<int> scoresNewestFirst)
    {
        if (scoresNewestFirst.Count < 2)
        {
            return "stable";
        }

        // Oldest half vs newest half
        var half = scoresNewestFirst.Count / 2;
        var newerAvg = scoresNewestFirst.Take(half).Average();
        var olderAvg = scoresNewestFirst.Skip(half).Average();

        var delta = newerAvg - olderAvg;
        if (delta > 5) return "improving";
        if (delta < -5) return "declining";
        return "stable";
    }
}

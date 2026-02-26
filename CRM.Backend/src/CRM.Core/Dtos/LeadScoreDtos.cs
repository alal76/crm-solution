// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

// FEAT-AISCORING: AI Lead Scoring Real-time Triggers — DTOs for history & explanation

/// <summary>One entry in the lead score change log.</summary>
public class LeadScoreHistoryDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public int Score { get; set; }
    public int PreviousScore { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Dictionary<string, int>? ScoreComponents { get; set; }
    public DateTime ScoredAt { get; set; }
    public string ScoredBy { get; set; } = string.Empty;
}

/// <summary>Full score breakdown with recent history and trend for a single lead.</summary>
public class LeadScoreExplanationDto
{
    public int LeadId { get; set; }
    public int CurrentScore { get; set; }
    public LeadScoreComponents Components { get; set; } = new();
    public string QualificationFramework { get; set; } = string.Empty;
    public List<LeadScoreHistoryDto> RecentHistory { get; set; } = new();

    /// <summary>"improving", "declining", or "stable".</summary>
    public string Trend { get; set; } = "stable";
}

/// <summary>Individual score components derived from the Lead entity's BANT/MEDDIC fields.</summary>
public class LeadScoreComponents
{
    public int? Fit { get; set; }
    public int? Engagement { get; set; }
    public int? Budget { get; set; }
    public int? Authority { get; set; }
    public int? Need { get; set; }
    public int? Timeline { get; set; }
    public int? Metrics { get; set; }
    public int? EconomicBuyer { get; set; }
    public int? DecisionCriteria { get; set; }
    public int? DecisionProcess { get; set; }
    public int? IdentifyPain { get; set; }
    public int? Champion { get; set; }
}

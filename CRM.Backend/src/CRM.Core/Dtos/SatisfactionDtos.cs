// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Enums;

namespace CRM.Core.Dtos;

// ── Survey ────────────────────────────────────────────────────────────────────

/// <summary>Read-model DTO for a satisfaction survey (includes response summary).</summary>
public class SatisfactionSurveyDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public SurveyType Type { get; set; }
    public SurveyStatus Status { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? AccountId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ResponseReceivedAt { get; set; }

    // Denormalised response fields (null when no response yet)
    public int? Score { get; set; }
    public string? Comment { get; set; }
    public SentimentType? Sentiment { get; set; }

    public string? Subject { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Payload for creating a new satisfaction survey.</summary>
public class CreateSatisfactionSurveyDto
{
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public int EntityId { get; set; }

    [Required]
    public SurveyType Type { get; set; }

    public int? ContactId { get; set; }
    public int? AccountId { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────

/// <summary>Read-model DTO for a satisfaction response.</summary>
public class SatisfactionResponseDto
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public SentimentType Sentiment { get; set; }
    public DateTime RespondedAt { get; set; }
}

/// <summary>Payload to submit a survey response via public survey link.</summary>
public class SubmitSatisfactionResponseDto
{
    [Required]
    public string SurveyToken { get; set; } = string.Empty;

    [Required]
    [Range(0, 10)]
    public int Score { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}

// ── Metrics ───────────────────────────────────────────────────────────────────

/// <summary>Aggregated satisfaction metrics for a given time period / entity type.</summary>
public class SatisfactionMetricsDto
{
    public double AverageCSATScore { get; set; }
    public double NPSScore { get; set; }
    public int TotalSurveys { get; set; }
    public int TotalResponses { get; set; }
    public double ResponseRate { get; set; }

    /// <summary>Monthly breakdown: key = "yyyy-MM", value = average score.</summary>
    public List<MonthlyMetricDto> ByMonth { get; set; } = new();

    /// <summary>Distribution of scores: key = score value, value = count.</summary>
    public Dictionary<int, int> ScoreDistribution { get; set; } = new();
}

/// <summary>Score summary for a single calendar month.</summary>
public class MonthlyMetricDto
{
    public string Month { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int Count { get; set; }
}

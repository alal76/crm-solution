// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;

namespace CRM.Core.Entities;

/// <summary>
/// Represents the response submitted by a recipient for a satisfaction survey.
/// One survey has at most one response.
/// </summary>
public class SatisfactionResponse : BaseEntity
{
    /// <summary>FK to the parent survey.</summary>
    public int SurveyId { get; set; }

    /// <summary>Numeric score (0-10 for NPS, 1-5 for CSAT, 1-7 for CES).</summary>
    public int Score { get; set; }

    /// <summary>Optional free-text comment from the respondent.</summary>
    public string? Comment { get; set; }

    /// <summary>Sentiment classification automatically derived from the score.</summary>
    public SentimentType Sentiment { get; set; }

    /// <summary>Client IP address captured at submission time.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Browser user-agent string captured at submission time.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Timestamp when the respondent submitted the form.</summary>
    public DateTime RespondedAt { get; set; }

    // ── Navigation properties ─────────────────────────────────────────────────

    /// <summary>Parent survey for this response.</summary>
    public virtual SatisfactionSurvey? Survey { get; set; }
}

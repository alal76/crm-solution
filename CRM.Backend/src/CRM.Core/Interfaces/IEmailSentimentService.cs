// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for analyzing email sentiment.
/// Implements TODO-AI-07.
/// </summary>
public interface IEmailSentimentService
{
    /// <summary>
    /// Analyzes sentiment of an email body.
    /// </summary>
    /// <param name="emailBody">The raw email body text to analyze.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sentiment analysis result.</returns>
    Task<SentimentResult> AnalyzeSentimentAsync(string emailBody, CancellationToken ct = default);
}

/// <summary>
/// Sentiment categories.
/// </summary>
public enum SentimentCategory
{
    /// <summary>Positive sentiment.</summary>
    Positive = 1,

    /// <summary>Neutral sentiment.</summary>
    Neutral = 0,

    /// <summary>Negative sentiment.</summary>
    Negative = -1
}

/// <summary>
/// Result of email sentiment analysis.
/// </summary>
public class SentimentResult
{
    /// <summary>Overall sentiment classification.</summary>
    public SentimentCategory Sentiment { get; set; }

    /// <summary>Sentiment score between -1.0 (most negative) and 1.0 (most positive).</summary>
    public double Score { get; set; }

    /// <summary>Key phrases extracted from the email.</summary>
    public string[] KeyPhrases { get; set; } = Array.Empty<string>();

    /// <summary>When the analysis was performed.</summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Request DTO for email sentiment analysis endpoint.
/// </summary>
public class EmailSentimentRequest
{
    /// <summary>Email body text to analyze.</summary>
    public string Body { get; set; } = string.Empty;
}

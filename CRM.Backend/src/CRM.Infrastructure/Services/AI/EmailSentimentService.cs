// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Keyword-based email sentiment analysis service.
/// Implements TODO-AI-07.
/// </summary>
public class EmailSentimentService : IEmailSentimentService
{
    private static readonly string[] PositiveKeywords =
    [
        "great", "excellent", "fantastic", "wonderful", "amazing", "love", "happy",
        "pleased", "thank", "thanks", "grateful", "appreciate", "perfect", "outstanding",
        "impressive", "well done", "brilliant", "superb", "delighted", "satisfied",
        "helpful", "prompt", "efficient", "professional", "recommend"
    ];

    private static readonly string[] NegativeKeywords =
    [
        "terrible", "awful", "horrible", "bad", "poor", "disappoint", "cancel",
        "escalate", "unacceptable", "frustrated", "useless", "broken", "issue",
        "problem", "bug", "error", "fail", "wrong", "upset", "angry", "disgusted",
        "refund", "complaint", "lawsuit", "legal", "never again", "waste", "disgrace"
    ];

    private readonly ILogger<EmailSentimentService> _logger;

    public EmailSentimentService(ILogger<EmailSentimentService> logger)
    {
        _logger = logger;
    }

    public Task<SentimentResult> AnalyzeSentimentAsync(string emailBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(emailBody))
        {
            return Task.FromResult(new SentimentResult
            {
                Sentiment = SentimentCategory.Neutral,
                Score = 0,
                KeyPhrases = Array.Empty<string>(),
                AnalyzedAt = DateTime.UtcNow
            });
        }

        var lower = emailBody.ToLowerInvariant();
        var matched = new List<string>();

        int positiveHits = 0;
        int negativeHits = 0;

        foreach (var word in PositiveKeywords)
        {
            if (lower.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                positiveHits++;
                matched.Add(word);
            }
        }

        foreach (var word in NegativeKeywords)
        {
            if (lower.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                negativeHits++;
                matched.Add(word);
            }
        }

        int total = positiveHits + negativeHits;
        double score = total == 0
            ? 0
            : (double)(positiveHits - negativeHits) / total;

        var category = score switch
        {
            > 0.1 => SentimentCategory.Positive,
            < -0.1 => SentimentCategory.Negative,
            _ => SentimentCategory.Neutral
        };

        _logger.LogDebug(
            "Sentiment analysis: {Category} (score={Score:F2}, pos={Pos}, neg={Neg})",
            category, score, positiveHits, negativeHits);

        return Task.FromResult(new SentimentResult
        {
            Sentiment = category,
            Score = score,
            KeyPhrases = matched.Take(10).ToArray(),
            AnalyzedAt = DateTime.UtcNow
        });
    }
}

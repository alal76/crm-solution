// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Enums;

/// <summary>Type of customer satisfaction survey.</summary>
public enum SurveyType
{
    CSAT = 0,
    NPS = 1,
    CES = 2,
}

/// <summary>Lifecycle status of a satisfaction survey.</summary>
public enum SurveyStatus
{
    Pending = 0,
    Sent = 1,
    Responded = 2,
    Expired = 3,
    Cancelled = 4,
}

/// <summary>Sentiment classification derived from the response score.</summary>
public enum SentimentType
{
    VeryPositive = 0,
    Positive = 1,
    Neutral = 2,
    Negative = 3,
    VeryNegative = 4,
}

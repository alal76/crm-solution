// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for assessing the risk level of a sales opportunity.
/// Implements TODO-AI-09.
/// </summary>
public interface IDealRiskService
{
    /// <summary>
    /// Calculates the risk level for a specific opportunity.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID to evaluate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deal risk assessment, or null if opportunity not found.</returns>
    Task<DealRiskDto?> CalculateRiskAsync(int opportunityId, CancellationToken ct = default);
}

/// <summary>
/// Risk levels for deal assessment.
/// </summary>
public enum DealRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Deal risk assessment result for an opportunity.
/// </summary>
public class DealRiskDto
{
    /// <summary>The opportunity this assessment is for.</summary>
    public int OpportunityId { get; set; }

    /// <summary>Risk score from 0 (no risk) to 100 (extreme risk).</summary>
    public int RiskScore { get; set; }

    /// <summary>Categorical risk level derived from RiskScore.</summary>
    public DealRiskLevel RiskLevel { get; set; }

    /// <summary>Risk factors identified for this deal.</summary>
    public string[] RiskFactors { get; set; } = Array.Empty<string>();

    /// <summary>Suggested mitigation actions to reduce deal risk.</summary>
    public string[] MitigationSuggestions { get; set; } = Array.Empty<string>();

    /// <summary>When this assessment was calculated.</summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

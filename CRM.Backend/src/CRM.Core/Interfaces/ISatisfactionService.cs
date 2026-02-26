// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service contract for managing CSAT / NPS / CES satisfaction surveys and metrics.
/// </summary>
public interface ISatisfactionService
{
    /// <summary>Get a paginated list of surveys, optionally filtered by entity type.</summary>
    Task<PagedResultDto<SatisfactionSurveyDto>> GetSurveysAsync(
        int page,
        int pageSize,
        string? entityType,
        CancellationToken ct = default);

    /// <summary>Get a single survey by its primary key.</summary>
    Task<SatisfactionSurveyDto?> GetSurveyByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Get a survey by its public external token.</summary>
    Task<SatisfactionSurveyDto?> GetSurveyByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Create and persist a new survey (token is auto-generated).</summary>
    Task<SatisfactionSurveyDto> CreateSurveyAsync(CreateSatisfactionSurveyDto dto, CancellationToken ct = default);

    /// <summary>Submit a response using the survey's public token.</summary>
    Task<SatisfactionResponseDto> SubmitResponseAsync(SubmitSatisfactionResponseDto dto, CancellationToken ct = default);

    /// <summary>Get aggregated satisfaction metrics for a period / entity type.</summary>
    Task<SatisfactionMetricsDto> GetMetricsAsync(
        DateTime? from,
        DateTime? to,
        string? entityType,
        CancellationToken ct = default);

    /// <summary>Compute NPS score: ((promoters − detractors) / total) × 100.</summary>
    Task<double> GetNPSScoreAsync(DateTime? from, DateTime? to, CancellationToken ct = default);

    /// <summary>Compute CSAT score: (satisfied / total) × 100, where satisfied ≥ 4 on a 1–5 scale.</summary>
    Task<double> GetCSATScoreAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}

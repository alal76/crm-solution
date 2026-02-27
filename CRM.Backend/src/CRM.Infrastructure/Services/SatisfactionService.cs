// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implements CSAT / NPS / CES satisfaction survey management and metrics aggregation.
/// </summary>
public class SatisfactionService : ISatisfactionService
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<SatisfactionService> _logger;

    public SatisfactionService(ICrmDbContext db, ILogger<SatisfactionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<PagedResultDto<SatisfactionSurveyDto>> GetSurveysAsync(
        int page,
        int pageSize,
        string? entityType,
        CancellationToken ct = default)
    {
        var query = _db.SatisfactionSurveys
            .Include(s => s.Contact)
            .Include(s => s.Response)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(s => s.EntityType == entityType);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResultDto<SatisfactionSurveyDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<SatisfactionSurveyDto?> GetSurveyByIdAsync(int id, CancellationToken ct = default)
    {
        var survey = await _db.SatisfactionSurveys
            .Include(s => s.Contact)
            .Include(s => s.Response)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return survey is null ? null : MapToDto(survey);
    }

    public async Task<SatisfactionSurveyDto?> GetSurveyByTokenAsync(string token, CancellationToken ct = default)
    {
        var survey = await _db.SatisfactionSurveys
            .Include(s => s.Contact)
            .Include(s => s.Response)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.ExternalToken == token, ct);

        return survey is null ? null : MapToDto(survey);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<SatisfactionSurveyDto> CreateSurveyAsync(
        CreateSatisfactionSurveyDto dto,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var survey = new SatisfactionSurvey
        {
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Type = dto.Type,
            Status = SurveyStatus.Pending,
            ContactId = dto.ContactId,
            AccountId = dto.AccountId,
            Subject = dto.Subject,
            ExternalToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = now.AddDays(30),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.SatisfactionSurveys.Add(survey);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created SatisfactionSurvey {Id} (type={Type}, entity={EntityType}/{EntityId})",
            survey.Id, survey.Type, survey.EntityType, survey.EntityId);

        return MapToDto(survey);
    }

    public async Task<SatisfactionResponseDto> SubmitResponseAsync(
        SubmitSatisfactionResponseDto dto,
        CancellationToken ct = default)
    {
        var survey = await _db.SatisfactionSurveys
            .Include(s => s.Response)
            .FirstOrDefaultAsync(s => s.ExternalToken == dto.SurveyToken, ct)
            ?? throw new InvalidOperationException($"Survey with token '{dto.SurveyToken}' not found.");

        if (survey.Status == SurveyStatus.Responded)
            throw new InvalidOperationException("This survey has already been answered.");

        if (survey.ExpiresAt.HasValue && survey.ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("This survey link has expired.");

        var now = DateTime.UtcNow;
        var sentiment = ClassifySentiment(dto.Score, survey.Type);

        var response = new SatisfactionResponse
        {
            SurveyId = survey.Id,
            Score = dto.Score,
            Comment = dto.Comment,
            Sentiment = sentiment,
            RespondedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        survey.Status = SurveyStatus.Responded;
        survey.ResponseReceivedAt = now;
        survey.UpdatedAt = now;

        _db.SatisfactionResponses.Add(response);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("SatisfactionResponse submitted for survey {SurveyId}, score={Score}", survey.Id, dto.Score);

        return MapResponseToDto(response);
    }

    // ── Metrics ───────────────────────────────────────────────────────────────

    public async Task<SatisfactionMetricsDto> GetMetricsAsync(
        DateTime? from,
        DateTime? to,
        string? entityType,
        CancellationToken ct = default)
    {
        var surveyQuery = BuildSurveyQuery(from, to, entityType);
        var totalSurveys = await surveyQuery.CountAsync(ct);

        var responseQuery = BuildResponseQuery(from, to, entityType);
        var responses = await responseQuery.ToListAsync(ct);

        var totalResponses = responses.Count;
        var responseRate = totalSurveys > 0 ? (double)totalResponses / totalSurveys * 100 : 0;

        // Monthly breakdown
        var byMonth = responses
            .GroupBy(r => r.RespondedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new MonthlyMetricDto
            {
                Month = g.Key,
                AverageScore = g.Average(r => (double)r.Score),
                Count = g.Count(),
            })
            .ToList();

        // Score distribution
        var scoreDistribution = responses
            .GroupBy(r => r.Score)
            .ToDictionary(g => g.Key, g => g.Count());

        // Average CSAT (map to 1-100 range)
        var csatResponses = responses.Where(r => r.Survey?.Type == SurveyType.CSAT).ToList();
        var avgCsat = csatResponses.Count > 0
            ? csatResponses.Average(r => (double)r.Score) / 5.0 * 100
            : 0;

        // NPS calculation
        var npsScore = await ComputeNPSAsync(responses.Where(r => r.Survey?.Type == SurveyType.NPS).ToList());

        return new SatisfactionMetricsDto
        {
            AverageCSATScore = Math.Round(avgCsat, 2),
            NPSScore = Math.Round(npsScore, 2),
            TotalSurveys = totalSurveys,
            TotalResponses = totalResponses,
            ResponseRate = Math.Round(responseRate, 2),
            ByMonth = byMonth,
            ScoreDistribution = scoreDistribution,
        };
    }

    public async Task<double> GetNPSScoreAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var responses = await BuildResponseQuery(from, to, null)
            .Where(r => r.Survey!.Type == SurveyType.NPS)
            .ToListAsync(ct);

        return ComputeNPSAsync(responses).GetAwaiter().GetResult(); // NOSONAR S4462 -- sync overload required by interface; delegates to async implementation // NOSONAR S4462 -- sync overload required by interface; delegates to async implementation
    }

    public async Task<double> GetCSATScoreAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var responses = await BuildResponseQuery(from, to, null)
            .Where(r => r.Survey!.Type == SurveyType.CSAT)
            .ToListAsync(ct);

        if (responses.Count == 0) return 0;

        var satisfied = responses.Count(r => r.Score >= 4);
        return Math.Round((double)satisfied / responses.Count * 100, 2);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private IQueryable<SatisfactionSurvey> BuildSurveyQuery(DateTime? from, DateTime? to, string? entityType)
    {
        var q = _db.SatisfactionSurveys.AsNoTracking();
        if (from.HasValue) q = q.Where(s => s.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(s => s.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(s => s.EntityType == entityType);
        return q;
    }

    private IQueryable<SatisfactionResponse> BuildResponseQuery(DateTime? from, DateTime? to, string? entityType)
    {
        var q = _db.SatisfactionResponses
            .Include(r => r.Survey)
            .AsNoTracking();
        if (from.HasValue) q = q.Where(r => r.RespondedAt >= from.Value);
        if (to.HasValue) q = q.Where(r => r.RespondedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(r => r.Survey!.EntityType == entityType);
        return q;
    }

    private static Task<double> ComputeNPSAsync(List<SatisfactionResponse> responses)
    {
        if (responses.Count == 0) return Task.FromResult(0.0);
        var promoters = responses.Count(r => r.Score >= 9);
        var detractors = responses.Count(r => r.Score <= 6);
        var nps = (double)(promoters - detractors) / responses.Count * 100;
        return Task.FromResult(Math.Round(nps, 2));
    }

    private static SentimentType ClassifySentiment(int score, SurveyType surveyType)
    {
        return surveyType switch
        {
            SurveyType.NPS => score switch
            {
                >= 9 => SentimentType.VeryPositive,
                >= 7 => SentimentType.Positive,
                >= 5 => SentimentType.Neutral,
                >= 3 => SentimentType.Negative,
                _ => SentimentType.VeryNegative,
            },
            SurveyType.CSAT => score switch
            {
                5 => SentimentType.VeryPositive,
                4 => SentimentType.Positive,
                3 => SentimentType.Neutral,
                2 => SentimentType.Negative,
                _ => SentimentType.VeryNegative,
            },
            SurveyType.CES => score switch
            {
                >= 6 => SentimentType.VeryPositive,
                >= 5 => SentimentType.Positive,
                4 => SentimentType.Neutral,
                3 => SentimentType.Negative,
                _ => SentimentType.VeryNegative,
            },
            _ => SentimentType.Neutral,
        };
    }

    private static SatisfactionSurveyDto MapToDto(SatisfactionSurvey s) => new()
    {
        Id = s.Id,
        EntityType = s.EntityType,
        EntityId = s.EntityId,
        Type = s.Type,
        Status = s.Status,
        ContactId = s.ContactId,
        ContactName = s.Contact is not null
            ? $"{s.Contact.FirstName} {s.Contact.LastName}".Trim()
            : null,
        AccountId = s.AccountId,
        SentAt = s.SentAt,
        ExpiresAt = s.ExpiresAt,
        ResponseReceivedAt = s.ResponseReceivedAt,
        Score = s.Response?.Score,
        Comment = s.Response?.Comment,
        Sentiment = s.Response?.Sentiment,
        Subject = s.Subject,
        CreatedAt = s.CreatedAt,
    };

    private static SatisfactionResponseDto MapResponseToDto(SatisfactionResponse r) => new()
    {
        Id = r.Id,
        SurveyId = r.SurveyId,
        Score = r.Score,
        Comment = r.Comment,
        Sentiment = r.Sentiment,
        RespondedAt = r.RespondedAt,
    };
}

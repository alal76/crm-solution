// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// AI Lead Scoring and Sales Intelligence Controller
/// Provides AI-powered lead scoring, opportunity insights, and sales predictions
/// </summary>
[ApiController]
[Route("api/ai/leads")]
[Authorize]
public class AILeadScoringController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly IAllenAIService _aiService;
    private readonly ILLMService _llmService;
    private readonly ILLMSettingsService _llmSettingsService;
    private readonly ILogger<AILeadScoringController> _logger;

    public AILeadScoringController(
        CrmDbContext context,
        IAllenAIService aiService,
        ILLMService llmService,
        ILLMSettingsService llmSettingsService,
        ILogger<AILeadScoringController> logger)
    {
        _context = context;
        _aiService = aiService;
        _llmService = llmService;
        _llmSettingsService = llmSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Score a single lead using AI
    /// </summary>
    [HttpPost("{leadId}/score")]
    public async Task<IActionResult> ScoreLead(int leadId, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _context.Leads.FindAsync(new object[] { leadId }, cancellationToken);
            if (lead == null)
            {
                return NotFound(new { error = $"Lead with ID {leadId} not found" });
            }

            var score = await _aiService.ScoreLeadAsync(leadId, cancellationToken);

            return Ok(new LeadScoreResponse
            {
                Success = true,
                LeadId = leadId,
                Score = MapToDto(score)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to score lead {LeadId}", leadId);
            return StatusCode(500, new { error = "Failed to score lead", details = ex.Message });
        }
    }

    /// <summary>
    /// Batch score multiple leads
    /// </summary>
    [HttpPost("batch-score")]
    public async Task<IActionResult> BatchScoreLeads([FromBody] BatchScoreRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.LeadIds == null || !request.LeadIds.Any())
            {
                return BadRequest(new { error = "LeadIds array is required" });
            }

            if (request.LeadIds.Count > 100)
            {
                return BadRequest(new { error = "Maximum 100 leads can be scored at once" });
            }

            var scores = await _aiService.BatchScoreLeadsAsync(request.LeadIds, cancellationToken);

            return Ok(new BatchScoreResponse
            {
                Success = true,
                ScoredCount = scores.Count,
                Scores = scores.Select(MapToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to batch score leads");
            return StatusCode(500, new { error = "Failed to batch score leads", details = ex.Message });
        }
    }

    /// <summary>
    /// Get top leads by AI score
    /// </summary>
    [HttpGet("top")]
    public async Task<IActionResult> GetTopLeads([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            count = Math.Min(50, Math.Max(1, count));
            var topLeads = await _aiService.GetTopLeadsAsync(count, cancellationToken);

            return Ok(new TopLeadsResponse
            {
                Success = true,
                Count = topLeads.Count,
                Leads = topLeads.Select(s => new TopLeadDto
                {
                    LeadId = s.LeadId,
                    LeadName = s.Lead?.FirstName + " " + s.Lead?.LastName,
                    Company = s.Lead?.CompanyName,
                    Email = s.Lead?.Email,
                    Score = MapToDto(s)
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top leads");
            return StatusCode(500, new { error = "Failed to get top leads", details = ex.Message });
        }
    }

    /// <summary>
    /// Get lead scoring history
    /// </summary>
    [HttpGet("{leadId}/history")]
    public async Task<IActionResult> GetLeadScoreHistory(int leadId, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _context.LeadScores
                .Where(s => s.LeadId == leadId && !s.IsDeleted)
                .OrderByDescending(s => s.ScoredAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return Ok(new LeadScoreHistoryResponse
            {
                Success = true,
                LeadId = leadId,
                History = history.Select(MapToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get lead score history for {LeadId}", leadId);
            return StatusCode(500, new { error = "Failed to get lead score history", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze a lead using LLM for detailed insights
    /// </summary>
    [HttpPost("{leadId}/analyze")]
    public async Task<IActionResult> AnalyzeLead(int leadId, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _context.Leads
                .Include(l => l.ProductInterests)
                .FirstOrDefaultAsync(l => l.Id == leadId, cancellationToken);

            if (lead == null)
            {
                return NotFound(new { error = $"Lead with ID {leadId} not found" });
            }

            var settings = await _llmSettingsService.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new LeadAnalysisResponse
                {
                    Success = false,
                    Error = "AI service not configured. Please configure LLM settings."
                });
            }

            var systemPrompt = @"You are a sales intelligence AI analyzing leads for a CRM system.
Analyze the provided lead data and provide:
1. Conversion probability (0-100%)
2. Key strengths and weaknesses
3. Recommended next actions
4. Ideal follow-up timing
5. Potential objections and how to handle them
6. Suggested talking points

Respond with valid JSON:
{
  ""conversionProbability"": 75,
  ""confidence"": ""high|medium|low"",
  ""strengths"": [""List of positive indicators""],
  ""weaknesses"": [""List of concerns or gaps""],
  ""nextActions"": [
    {""action"": ""Action description"", ""priority"": ""high|medium|low"", ""timing"": ""Immediately|Within 24 hours|This week""}
  ],
  ""followUpRecommendation"": {
    ""timing"": ""Best time to follow up"",
    ""channel"": ""email|phone|linkedin"",
    ""reason"": ""Why this timing/channel""
  },
  ""objectionHandling"": [
    {""objection"": ""Potential objection"", ""response"": ""Suggested response""}
  ],
  ""talkingPoints"": [""Key points to discuss""],
  ""summary"": ""One paragraph executive summary""
}";

            var leadData = new
            {
                name = $"{lead.FirstName} {lead.LastName}",
                company = lead.CompanyName,
                title = lead.Title,
                email = lead.Email,
                phone = lead.Phone,
                source = lead.Source.ToString(),
                status = lead.Status.ToString(),
                score = lead.Score,
                fitScore = lead.FitScore,
                engagementScore = lead.EngagementScore,
                productInterests = lead.ProductInterests?.Select(p => p.Product?.Name ?? "Unknown").ToList(),
                daysInPipeline = (DateTime.UtcNow - lead.CreatedAt).Days,
                region = lead.Region,
                notes = lead.QualificationNotes
            };

            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = GetDefaultModelForProvider(settings, settings.DefaultProvider),
                Messages = new List<LLMMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = $"Analyze this lead:\n\n{JsonSerializer.Serialize(leadData, new JsonSerializerOptions { WriteIndented = true })}" }
                },
                Temperature = 0.4,
                MaxTokens = 1500,
                JsonMode = true
            };

            var response = await _llmService.ChatAsync(llmRequest, cancellationToken);

            if (response.Success)
            {
                try
                {
                    var analysis = JsonSerializer.Deserialize<LeadAnalysisResult>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(new LeadAnalysisResponse
                    {
                        Success = true,
                        LeadId = leadId,
                        Analysis = analysis,
                        Provider = response.Provider,
                        TokensUsed = response.TotalTokens
                    });
                }
                catch (JsonException)
                {
                    return Ok(new LeadAnalysisResponse
                    {
                        Success = true,
                        LeadId = leadId,
                        RawAnalysis = response.Content,
                        Provider = response.Provider
                    });
                }
            }

            return Ok(new LeadAnalysisResponse
            {
                Success = false,
                Error = response.Error ?? "Lead analysis failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze lead {LeadId}", leadId);
            return StatusCode(500, new { error = "Failed to analyze lead", details = ex.Message });
        }
    }

    /// <summary>
    /// Get scoring configuration and thresholds
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetScoringConfig()
    {
        return Ok(new ScoringConfigResponse
        {
            Categories = new[]
            {
                new ScoreCategoryDto { Name = "OnFire", MinScore = 80, MaxScore = 100, Color = "#FF5722", Description = "Extremely hot lead, ready to close" },
                new ScoreCategoryDto { Name = "Hot", MinScore = 60, MaxScore = 79, Color = "#FF9800", Description = "High potential, prioritize follow-up" },
                new ScoreCategoryDto { Name = "Warm", MinScore = 40, MaxScore = 59, Color = "#FFC107", Description = "Moderate interest, nurture needed" },
                new ScoreCategoryDto { Name = "Cool", MinScore = 20, MaxScore = 39, Color = "#2196F3", Description = "Low engagement, long-term nurture" },
                new ScoreCategoryDto { Name = "Cold", MinScore = 0, MaxScore = 19, Color = "#9E9E9E", Description = "Minimal interest or engagement" }
            },
            Weights = new ScoringWeightsDto
            {
                Demographic = 0.20m,
                Behavioral = 0.30m,
                Engagement = 0.25m,
                Intent = 0.25m
            },
            RefreshIntervalDays = 7
        });
    }

    private LeadScoreDto MapToDto(LeadScore score)
    {
        return new LeadScoreDto
        {
            Id = score.Id,
            LeadId = score.LeadId,
            OverallScore = score.OverallScore,
            Category = score.Category.ToString(),
            Confidence = score.Confidence,
            DemographicScore = score.DemographicScore,
            FirmographicScore = score.FirmographicScore,
            BehavioralScore = score.BehavioralScore,
            EngagementScore = score.EngagementScore,
            IntentScore = score.IntentScore,
            EngagementLevel = score.EngagementLevel.ToString(),
            IntentStrength = score.IntentStrength.ToString(),
            ConversionProbability = score.ConversionProbability,
            EstimatedDaysToConversion = score.EstimatedDaysToConversion,
            AIInsights = score.AIInsights,
            ScoredAt = score.ScoredAt,
            ExpiresAt = score.ExpiresAt,
            ModelVersion = score.ModelVersion
        };
    }

    private static string GetDefaultModelForProvider(LLMSettingsDto settings, string provider)
        => AIServiceHelper.GetDefaultModelForProvider(settings, provider);
}

#region Request/Response DTOs

public class BatchScoreRequest
{
    public List<int> LeadIds { get; set; } = new();
}

public class LeadScoreResponse
{
    public bool Success { get; set; }
    public int LeadId { get; set; }
    public LeadScoreDto? Score { get; set; }
    public string? Error { get; set; }
}

public class BatchScoreResponse
{
    public bool Success { get; set; }
    public int ScoredCount { get; set; }
    public List<LeadScoreDto> Scores { get; set; } = new();
    public string? Error { get; set; }
}

public class TopLeadsResponse
{
    public bool Success { get; set; }
    public int Count { get; set; }
    public List<TopLeadDto> Leads { get; set; } = new();
}

public class TopLeadDto
{
    public int LeadId { get; set; }
    public string? LeadName { get; set; }
    public string? Company { get; set; }
    public string? Email { get; set; }
    public LeadScoreDto? Score { get; set; }
}

public class LeadScoreHistoryResponse
{
    public bool Success { get; set; }
    public int LeadId { get; set; }
    public List<LeadScoreDto> History { get; set; } = new();
}

public class LeadScoreDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public decimal OverallScore { get; set; }
    public string Category { get; set; } = "";
    public decimal Confidence { get; set; }
    public decimal DemographicScore { get; set; }
    public decimal FirmographicScore { get; set; }
    public decimal BehavioralScore { get; set; }
    public decimal EngagementScore { get; set; }
    public decimal IntentScore { get; set; }
    public string? EngagementLevel { get; set; }
    public string? IntentStrength { get; set; }
    public decimal ConversionProbability { get; set; }
    public int? EstimatedDaysToConversion { get; set; }
    public string? AIInsights { get; set; }
    public DateTime ScoredAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? ModelVersion { get; set; }
}

public class LeadAnalysisResponse
{
    public bool Success { get; set; }
    public int LeadId { get; set; }
    public LeadAnalysisResult? Analysis { get; set; }
    public string? RawAnalysis { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
    public int TokensUsed { get; set; }
}

public class LeadAnalysisResult
{
    public int ConversionProbability { get; set; }
    public string? Confidence { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
    public List<NextActionDto>? NextActions { get; set; }
    public FollowUpRecommendationDto? FollowUpRecommendation { get; set; }
    public List<ObjectionHandlingDto>? ObjectionHandling { get; set; }
    public List<string>? TalkingPoints { get; set; }
    public string? Summary { get; set; }
}

public class NextActionDto
{
    public string Action { get; set; } = "";
    public string? Priority { get; set; }
    public string? Timing { get; set; }
}

public class FollowUpRecommendationDto
{
    public string? Timing { get; set; }
    public string? Channel { get; set; }
    public string? Reason { get; set; }
}

public class ObjectionHandlingDto
{
    public string Objection { get; set; } = "";
    public string Response { get; set; } = "";
}

public class ScoringConfigResponse
{
    public ScoreCategoryDto[] Categories { get; set; } = Array.Empty<ScoreCategoryDto>();
    public ScoringWeightsDto Weights { get; set; } = new();
    public int RefreshIntervalDays { get; set; }
}

public class ScoreCategoryDto
{
    public string Name { get; set; } = "";
    public int MinScore { get; set; }
    public int MaxScore { get; set; }
    public string Color { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ScoringWeightsDto
{
    public decimal Demographic { get; set; }
    public decimal Behavioral { get; set; }
    public decimal Engagement { get; set; }
    public decimal Intent { get; set; }
}

#endregion

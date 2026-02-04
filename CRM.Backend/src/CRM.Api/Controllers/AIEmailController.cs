// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// AI Email Intelligence Controller
/// Provides AI-powered email analysis and assistance:
/// - Sentiment analysis
/// - Response suggestions
/// - Subject line optimization
/// - Tone matching
/// - Key entity extraction
/// </summary>
[ApiController]
[Route("api/ai/email")]
[Authorize]
public class AIEmailController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILLMService _llmService;
    private readonly ILLMSettingsService _llmSettingsService;
    private readonly ILogger<AIEmailController> _logger;

    public AIEmailController(
        CrmDbContext context,
        ILLMService llmService,
        ILLMSettingsService llmSettingsService,
        ILogger<AIEmailController> logger)
    {
        _context = context;
        _llmService = llmService;
        _llmSettingsService = llmSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze an email for sentiment, key entities, and suggested actions
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeEmail([FromBody] EmailAnalysisRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmailContent))
            {
                return BadRequest(new { error = "Email content cannot be empty" });
            }

            var settings = await _llmSettingsService.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new EmailAnalysisResponse
                {
                    Success = false,
                    Error = "AI service not configured. Please configure LLM settings."
                });
            }

            var systemPrompt = @"You are an expert email analyst for a CRM system. Analyze the provided email and extract:
1. Sentiment (positive, negative, neutral, mixed) with confidence score (0-100)
2. Urgency level (low, medium, high, critical)
3. Email classification (inquiry, complaint, follow-up, thank_you, request, information, urgent_action)
4. Key entities: dates, amounts, names, action items
5. Suggested next actions for the sales/support team
6. Main topics/themes mentioned

Respond ONLY with valid JSON in this exact format:
{
  ""sentiment"": {
    ""label"": ""positive|negative|neutral|mixed"",
    ""confidence"": 85,
    ""explanation"": ""Brief explanation""
  },
  ""urgency"": ""low|medium|high|critical"",
  ""classification"": ""inquiry|complaint|follow-up|thank_you|request|information|urgent_action"",
  ""entities"": {
    ""dates"": [""2026-02-15"", ""next Monday""],
    ""amounts"": [""$5,000"", ""10%""],
    ""names"": [""John Smith""],
    ""action_items"": [""Schedule call"", ""Send proposal""]
  },
  ""suggested_actions"": [
    ""Schedule a follow-up call within 24 hours"",
    ""Send pricing proposal"",
    ""Create support ticket""
  ],
  ""topics"": [""pricing"", ""implementation timeline""],
  ""summary"": ""One sentence summary of the email""
}";

            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = GetDefaultModelForProvider(settings, settings.DefaultProvider),
                Messages = new List<LLMMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = $"Analyze this email:\n\nSubject: {request.Subject ?? "(No subject)"}\n\n{request.EmailContent}" }
                },
                Temperature = 0.3, // Lower for more consistent analysis
                MaxTokens = 1000,
                JsonMode = true
            };

            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                try
                {
                    var analysisResult = JsonSerializer.Deserialize<EmailAnalysisResult>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(new EmailAnalysisResponse
                    {
                        Success = true,
                        Analysis = analysisResult,
                        Provider = response.Provider,
                        TokensUsed = response.TotalTokens
                    });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse LLM response as JSON: {Content}", response.Content);
                    return Ok(new EmailAnalysisResponse
                    {
                        Success = true,
                        RawAnalysis = response.Content,
                        Provider = response.Provider
                    });
                }
            }

            return Ok(new EmailAnalysisResponse
            {
                Success = false,
                Error = response.Error ?? "Analysis failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email analysis failed");
            return StatusCode(500, new { error = "Email analysis failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate response suggestions for an email
    /// </summary>
    [HttpPost("suggest-response")]
    public async Task<IActionResult> SuggestResponse([FromBody] ResponseSuggestionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmailContent))
            {
                return BadRequest(new { error = "Email content cannot be empty" });
            }

            var settings = await _llmSettingsService.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new ResponseSuggestionResponse
                {
                    Success = false,
                    Error = "AI service not configured"
                });
            }

            // Get customer context if available
            string customerContext = "";
            if (request.AccountId.HasValue)
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.AccountId.Value);

                if (customer != null)
                {
                    customerContext = $"\n\nCustomer Context:\n- Company: {customer.Company}\n- Account Type: {customer.AccountType}\n- Industry: {customer.Industry}";
                }
            }

            var toneInstruction = request.Tone switch
            {
                "formal" => "Use a formal, professional tone with proper business language.",
                "friendly" => "Use a warm, friendly yet professional tone.",
                "casual" => "Use a casual, conversational tone while remaining professional.",
                "apologetic" => "Use an empathetic, apologetic tone acknowledging any issues.",
                "enthusiastic" => "Use an enthusiastic, positive tone showing excitement.",
                _ => "Use an appropriate professional tone."
            };

            var systemPrompt = $@"You are a professional email response assistant for a CRM system.
Generate {request.NumSuggestions ?? 3} different response options for the given email.
{toneInstruction}

{customerContext}

Respond with JSON in this format:
{{
  ""suggestions"": [
    {{
      ""subject"": ""Re: Original Subject"",
      ""body"": ""Full email response body..."",
      ""tone"": ""formal|friendly|casual"",
      ""intent"": ""acknowledge|resolve|followup|escalate""
    }}
  ],
  ""quick_replies"": [
    ""Thanks for reaching out! I'll look into this and get back to you shortly."",
    ""I've received your message and will respond within 24 hours.""
  ]
}}";

            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = GetDefaultModelForProvider(settings, settings.DefaultProvider),
                Messages = new List<LLMMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = $"Generate responses for this email:\n\nSubject: {request.Subject ?? "(No subject)"}\n\n{request.EmailContent}" }
                },
                Temperature = 0.7, // Higher for creative responses
                MaxTokens = 2000,
                JsonMode = true
            };

            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                try
                {
                    var suggestions = JsonSerializer.Deserialize<ResponseSuggestions>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(new ResponseSuggestionResponse
                    {
                        Success = true,
                        Suggestions = suggestions?.Suggestions ?? new List<EmailSuggestion>(),
                        QuickReplies = suggestions?.QuickReplies ?? new List<string>(),
                        Provider = response.Provider
                    });
                }
                catch (JsonException)
                {
                    return Ok(new ResponseSuggestionResponse
                    {
                        Success = true,
                        RawContent = response.Content
                    });
                }
            }

            return Ok(new ResponseSuggestionResponse
            {
                Success = false,
                Error = response.Error ?? "Suggestion generation failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Response suggestion failed");
            return StatusCode(500, new { error = "Response suggestion failed" });
        }
    }

    /// <summary>
    /// Optimize an email subject line for better engagement
    /// </summary>
    [HttpPost("optimize-subject")]
    public async Task<IActionResult> OptimizeSubject([FromBody] SubjectOptimizationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Subject) && string.IsNullOrWhiteSpace(request.EmailBody))
            {
                return BadRequest(new { error = "Subject or email body is required" });
            }

            var settings = await _llmSettingsService.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new SubjectOptimizationResponse
                {
                    Success = false,
                    Error = "AI service not configured"
                });
            }

            var purposeContext = request.Purpose switch
            {
                "sales" => "for a sales outreach email to maximize open rates",
                "followup" => "for a follow-up email that encourages response",
                "support" => "for a support response that's clear and helpful",
                "marketing" => "for a marketing email with high engagement",
                "internal" => "for internal communication that's clear and actionable",
                _ => "for professional business communication"
            };

            var systemPrompt = $@"You are an email subject line optimization expert.
Generate 5 optimized subject line variations {purposeContext}.

Consider:
- Clarity and relevance
- Urgency without being spammy
- Personalization opportunities
- Optimal length (40-60 characters)
- Action-oriented language

Respond with JSON:
{{
  ""original_score"": 65,
  ""suggestions"": [
    {{
      ""subject"": ""Optimized subject line here"",
      ""score"": 85,
      ""reason"": ""Why this is better""
    }}
  ],
  ""tips"": [""General tips for improvement""]
}}";

            var userContent = request.Subject != null
                ? $"Original subject: {request.Subject}\n\nEmail preview: {(request.EmailBody?.Length > 500 ? request.EmailBody[..500] + "..." : request.EmailBody ?? "")}"
                : $"Generate subject for this email:\n\n{request.EmailBody}";

            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = GetDefaultModelForProvider(settings, settings.DefaultProvider),
                Messages = new List<LLMMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = userContent }
                },
                Temperature = 0.8,
                MaxTokens = 800,
                JsonMode = true
            };

            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<SubjectOptimizationResult>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(new SubjectOptimizationResponse
                    {
                        Success = true,
                        OriginalScore = result?.OriginalScore ?? 50,
                        Suggestions = result?.Suggestions ?? new List<SubjectSuggestion>(),
                        Tips = result?.Tips ?? new List<string>(),
                        Provider = response.Provider
                    });
                }
                catch (JsonException)
                {
                    return Ok(new SubjectOptimizationResponse
                    {
                        Success = true,
                        RawContent = response.Content
                    });
                }
            }

            return Ok(new SubjectOptimizationResponse
            {
                Success = false,
                Error = response.Error ?? "Subject optimization failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subject optimization failed");
            return StatusCode(500, new { error = "Subject optimization failed" });
        }
    }

    /// <summary>
    /// Improve email writing - grammar, tone, clarity
    /// </summary>
    [HttpPost("improve")]
    public async Task<IActionResult> ImproveEmail([FromBody] EmailImproveRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmailContent))
            {
                return BadRequest(new { error = "Email content cannot be empty" });
            }

            var settings = await _llmSettingsService.GetSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new EmailImproveResponse
                {
                    Success = false,
                    Error = "AI service not configured"
                });
            }

            var improvementFocus = string.Join(", ", request.ImprovementAreas ?? new[] { "clarity", "grammar", "professionalism" });

            var systemPrompt = $@"You are a professional email writing assistant.
Improve the given email focusing on: {improvementFocus}

Provide:
1. An improved version of the email
2. A list of specific changes made
3. Overall quality scores (before and after)

Respond with JSON:
{{
  ""improved_email"": {{
    ""subject"": ""Improved subject if applicable"",
    ""body"": ""Full improved email body""
  }},
  ""changes"": [
    {{
      ""original"": ""Original phrase"",
      ""improved"": ""Improved phrase"",
      ""reason"": ""Why this change improves the email""
    }}
  ],
  ""scores"": {{
    ""original"": {{ ""clarity"": 70, ""tone"": 65, ""grammar"": 80, ""overall"": 72 }},
    ""improved"": {{ ""clarity"": 90, ""tone"": 85, ""grammar"": 95, ""overall"": 90 }}
  }},
  ""summary"": ""Brief summary of improvements""
}}";

            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = GetDefaultModelForProvider(settings, settings.DefaultProvider),
                Messages = new List<LLMMessage>
                {
                    new() { Role = "system", Content = systemPrompt },
                    new() { Role = "user", Content = $"Improve this email:\n\nSubject: {request.Subject ?? "(No subject)"}\n\n{request.EmailContent}" }
                },
                Temperature = 0.5,
                MaxTokens = 2000,
                JsonMode = true
            };

            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<EmailImprovementResult>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return Ok(new EmailImproveResponse
                    {
                        Success = true,
                        ImprovedEmail = result?.ImprovedEmail,
                        Changes = result?.Changes ?? new List<EmailChange>(),
                        Scores = result?.Scores,
                        Summary = result?.Summary,
                        Provider = response.Provider
                    });
                }
                catch (JsonException)
                {
                    return Ok(new EmailImproveResponse
                    {
                        Success = true,
                        RawContent = response.Content
                    });
                }
            }

            return Ok(new EmailImproveResponse
            {
                Success = false,
                Error = response.Error ?? "Email improvement failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email improvement failed");
            return StatusCode(500, new { error = "Email improvement failed" });
        }
    }

    private static string GetDefaultModelForProvider(LLMSettingsDto settings, string provider)
        => AIServiceHelper.GetDefaultModelForProvider(settings, provider);
}

#region Request DTOs

public class EmailAnalysisRequest
{
    public string EmailContent { get; set; } = "";
    public string? Subject { get; set; }
    public string? SenderEmail { get; set; }
    public int? AccountId { get; set; }
}

public class ResponseSuggestionRequest
{
    public string EmailContent { get; set; } = "";
    public string? Subject { get; set; }
    public string? Tone { get; set; } // formal, friendly, casual, apologetic, enthusiastic
    public int? NumSuggestions { get; set; }
    public int? AccountId { get; set; }
}

public class SubjectOptimizationRequest
{
    public string? Subject { get; set; }
    public string? EmailBody { get; set; }
    public string? Purpose { get; set; } // sales, followup, support, marketing, internal
}

public class EmailImproveRequest
{
    public string EmailContent { get; set; } = "";
    public string? Subject { get; set; }
    public string[]? ImprovementAreas { get; set; } // clarity, grammar, tone, professionalism, brevity
}

#endregion

#region Response DTOs

public class EmailAnalysisResponse
{
    public bool Success { get; set; }
    public EmailAnalysisResult? Analysis { get; set; }
    public string? RawAnalysis { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
    public int TokensUsed { get; set; }
}

public class EmailAnalysisResult
{
    public SentimentInfo? Sentiment { get; set; }
    public string? Urgency { get; set; }
    public string? Classification { get; set; }
    public EntityInfo? Entities { get; set; }
    public List<string>? SuggestedActions { get; set; }
    public List<string>? Topics { get; set; }
    public string? Summary { get; set; }
}

public class SentimentInfo
{
    public string Label { get; set; } = "";
    public int Confidence { get; set; }
    public string? Explanation { get; set; }
}

public class EntityInfo
{
    public List<string>? Dates { get; set; }
    public List<string>? Amounts { get; set; }
    public List<string>? Names { get; set; }
    public List<string>? ActionItems { get; set; }
}

public class ResponseSuggestionResponse
{
    public bool Success { get; set; }
    public List<EmailSuggestion>? Suggestions { get; set; }
    public List<string>? QuickReplies { get; set; }
    public string? RawContent { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
}

public class ResponseSuggestions
{
    public List<EmailSuggestion>? Suggestions { get; set; }
    public List<string>? QuickReplies { get; set; }
}

public class EmailSuggestion
{
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Tone { get; set; }
    public string? Intent { get; set; }
}

public class SubjectOptimizationResponse
{
    public bool Success { get; set; }
    public int OriginalScore { get; set; }
    public List<SubjectSuggestion>? Suggestions { get; set; }
    public List<string>? Tips { get; set; }
    public string? RawContent { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
}

public class SubjectOptimizationResult
{
    public int OriginalScore { get; set; }
    public List<SubjectSuggestion>? Suggestions { get; set; }
    public List<string>? Tips { get; set; }
}

public class SubjectSuggestion
{
    public string Subject { get; set; } = "";
    public int Score { get; set; }
    public string? Reason { get; set; }
}

public class EmailImproveResponse
{
    public bool Success { get; set; }
    public ImprovedEmail? ImprovedEmail { get; set; }
    public List<EmailChange>? Changes { get; set; }
    public EmailScores? Scores { get; set; }
    public string? Summary { get; set; }
    public string? RawContent { get; set; }
    public string? Error { get; set; }
    public string? Provider { get; set; }
}

public class EmailImprovementResult
{
    public ImprovedEmail? ImprovedEmail { get; set; }
    public List<EmailChange>? Changes { get; set; }
    public EmailScores? Scores { get; set; }
    public string? Summary { get; set; }
}

public class ImprovedEmail
{
    public string? Subject { get; set; }
    public string Body { get; set; } = "";
}

public class EmailChange
{
    public string Original { get; set; } = "";
    public string Improved { get; set; } = "";
    public string? Reason { get; set; }
}

public class EmailScores
{
    public ScoreSet? Original { get; set; }
    public ScoreSet? Improved { get; set; }
}

public class ScoreSet
{
    public int Clarity { get; set; }
    public int Tone { get; set; }
    public int Grammar { get; set; }
    public int Overall { get; set; }
}

#endregion

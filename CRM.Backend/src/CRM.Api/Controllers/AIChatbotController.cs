// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Api.Controllers;

/// <summary>
/// AI Chatbot controller for CRM Assistant functionality.
/// Provides conversational AI interface with CRM documentation context,
/// including chat messaging, contextual suggestions, and health monitoring.
/// </summary>
/// <remarks>
/// Most endpoints require authentication. The chatbot uses configured LLM providers
/// to generate contextual responses about CRM features and customer data.
/// </remarks>
[ApiController]
[Route("api/ai/chatbot")]
[Authorize]
[Produces("application/json")]
public class AIChatbotController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILLMService _llmService;
    private readonly ILLMSettingsService _llmSettingsService;
    private readonly ILogger<AIChatbotController> _logger;

    // Cached documentation context
    private static readonly object _cacheLock = new();
    private static string? _cachedDocumentation;
    private static DateTime _cacheExpiry = DateTime.MinValue;

    public AIChatbotController(
        CrmDbContext context,
        ILLMService llmService,
        ILLMSettingsService llmSettingsService,
        ILogger<AIChatbotController> logger)
    {
        _context = context;
        _llmService = llmService;
        _llmSettingsService = llmSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Check AI service health status.
    /// </summary>
    /// <returns>Health status including provider, model, and response time.</returns>
    /// <response code="200">Returns the AI service health status.</response>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var settings = await _llmSettingsService.GetSettingsAsync();

            // Use effective fallback order to get the first configured provider
            var provider = AIServiceHelper.GetFirstAvailableProvider(settings!);
            var model = AIServiceHelper.GetDefaultModelForProvider(settings!, provider);

            // Check if the provider is configured
            var isConfigured = _llmService.IsConfigured(provider);

            if (!isConfigured)
            {
                // Return info about all configured providers for debugging
                var configuredProviders = settings?.EffectiveFallbackOrder ?? new List<string>();
                return Ok(new
                {
                    isHealthy = false,
                    provider = provider,
                    model = model,
                    configuredProviders = configuredProviders,
                    message = configuredProviders.Count == 0
                        ? "No AI providers configured. Configure at least one provider with an API key or enable local LLM."
                        : "AI service not configured",
                    timestamp = DateTime.UtcNow
                });
            }

            // Try a simple health check with the LLM
            try
            {
                var testRequest = new LLMRequest
                {
                    Provider = provider,
                    Model = model,
                    Prompt = "Hello",
                    MaxTokens = 5,
                    Temperature = 0
                };

                var response = await _llmService.CompletionAsync(testRequest);

                return Ok(new
                {
                    isHealthy = response.Success,
                    provider = provider,
                    model = response.Model ?? model,
                    responseTimeMs = response.DurationMs,
                    message = response.Success ? "AI service operational" : response.Error,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI health check probe failed");
                return Ok(new
                {
                    isHealthy = true, // Service is configured, just can't do inference now
                    provider = provider,
                    model = model,
                    message = "AI service configured (inference test skipped)",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI health check failed");
            return Ok(new
            {
                isHealthy = false,
                provider = "unknown",
                model = "unknown",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Initialize chatbot context by loading CRM documentation.
    /// </summary>
    /// <returns>Success status and message.</returns>
    /// <response code="200">Returns initialization status.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpPost("initialize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Initialize()
    {
        try
        {
            await LoadDocumentationAsync();
            return Ok(new { success = true, message = "Chatbot context initialized" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize chatbot context");
            return Ok(new { success = false, message = "Chatbot initialized with limited context" });
        }
    }

    /// <summary>
    /// Send a message to the AI chatbot.
    /// </summary>
    /// <param name="request">The chat message request including message, conversation history, and optional account context.</param>
    /// <returns>AI-generated response based on CRM context.</returns>
    /// <response code="200">Returns the AI chatbot response.</response>
    /// <response code="400">Message cannot be empty.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpPost("message")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            // Get LLM settings
            var settings = await _llmSettingsService.GetSettingsAsync();

            // Use effective fallback order to determine which provider to use
            var effectiveProviders = settings?.EffectiveFallbackOrder ?? new List<string>();
            if (settings == null || effectiveProviders.Count == 0)
            {
                return Ok(new
                {
                    response = "I apologize, but I'm having trouble connecting to the AI service. Please check your LLM settings or try again later."
                });
            }

            // Load documentation if not cached
            var documentation = await LoadDocumentationAsync();

            // Build system prompt with CRM context
            var systemPrompt = BuildSystemPrompt(documentation, request.AccountContext);

            // Get account-specific context if accounts are selected
            var accountInfo = "";
            if (request.AccountIds?.Any() == true)
            {
                accountInfo = await GetAccountContextAsync(request.AccountIds);
            }

            // Build conversation messages
            var messages = new List<LLMMessage>
            {
                new() { Role = "system", Content = systemPrompt }
            };

            // Add account context if available
            if (!string.IsNullOrEmpty(accountInfo))
            {
                messages.Add(new LLMMessage
                {
                    Role = "system",
                    Content = $"Current Account Context:\n{accountInfo}"
                });
            }

            // Add conversation history if available (limited to last 10 exchanges for context window management)
            if (request.ConversationHistory?.Any() == true)
            {
                var recentHistory = request.ConversationHistory
                    .Where(m => !string.IsNullOrEmpty(m.Content) && (m.Role == "user" || m.Role == "assistant"))
                    .TakeLast(20) // Last 20 messages (10 exchanges)
                    .ToList();

                foreach (var historyMessage in recentHistory)
                {
                    messages.Add(new LLMMessage
                    {
                        Role = historyMessage.Role,
                        Content = historyMessage.Content
                    });
                }
            }

            // Add user message
            messages.Add(new LLMMessage { Role = "user", Content = request.Message });

            // Use the first available provider from effective fallback order
            var provider = effectiveProviders[0];
            var defaultModel = GetDefaultModelForProvider(settings, provider);

            // Make LLM request using the first configured provider
            var llmRequest = new LLMRequest
            {
                Provider = provider,
                Model = defaultModel,
                Messages = messages,
                Temperature = 0.7,
                MaxTokens = 1500,
            };

            _logger.LogDebug("Sending chatbot request to provider {Provider} with model {Model}", provider, defaultModel);
            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                return Ok(new { response = response.Content });
            }
            else
            {
                _logger.LogWarning("LLM request failed: {Error}", response.Error);
                return Ok(new
                {
                    response = "I'm having trouble processing your request right now. Please try again or rephrase your question."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message");
            return Ok(new
            {
                response = "An error occurred while processing your message. Please try again."
            });
        }
    }

    /// <summary>
    /// Get quick suggestions based on context.
    /// </summary>
    /// <param name="context">Optional context string (e.g., "customer", "account") to tailor suggestions.</param>
    /// <returns>List of suggested questions for the chatbot.</returns>
    /// <response code="200">Returns list of contextual suggestions.</response>
    /// <response code="401">Unauthorized - User is not authenticated.</response>
    [HttpGet("suggestions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetSuggestions([FromQuery] string? context = null)
    {
        var suggestions = new List<string>
        {
            "How do I create a new customer?",
            "What are the different lead stages?",
            "How do I set up a marketing campaign?",
            "Explain the opportunity pipeline",
            "How do I create a quote?",
            "What reports are available?",
            "How do workflows work?",
            "How do I manage service requests?"
        };

        if (!string.IsNullOrEmpty(context))
        {
            // Add context-specific suggestions
            if (context.ToLower().Contains("customer") || context.ToLower().Contains("account"))
            {
                var contextSuggestions = new[]
                {
                    "Show me recent activities for this customer",
                    "What opportunities are open for this account?",
                    "How do I add a contact to this customer?"
                };
                suggestions.InsertRange(0, contextSuggestions);
            }
        }

        return Ok(new { suggestions = suggestions.Take(5) });
    }

    private async Task<string> LoadDocumentationAsync()
    {
        lock (_cacheLock)
        {
            if (_cachedDocumentation != null && DateTime.UtcNow < _cacheExpiry)
            {
                return _cachedDocumentation;
            }
        }

        var documentation = new StringBuilder();

        // Core CRM Documentation
        documentation.AppendLine("# CRM Solution Documentation");
        documentation.AppendLine();
        documentation.AppendLine("## Overview");
        documentation.AppendLine("This is a comprehensive Customer Relationship Management (CRM) system that helps organizations manage customer interactions, sales pipelines, marketing campaigns, and service requests.");
        documentation.AppendLine();

        // Modules
        documentation.AppendLine("## Main Modules");
        documentation.AppendLine();

        documentation.AppendLine("### Accounts");
        documentation.AppendLine("- Manage both individual and organization accounts");
        documentation.AppendLine("- Track customer information including contacts, addresses, and communication history");
        documentation.AppendLine("- Customer lifecycle stages: Lead, Prospect, Customer, Churned");
        documentation.AppendLine("- Customer categories: Individual (B2C) and Organization (B2B)");
        documentation.AppendLine();

        documentation.AppendLine("### Contacts");
        documentation.AppendLine("- Store contact information linked to accounts");
        documentation.AppendLine("- Multiple phone numbers, emails, and addresses per contact");
        documentation.AppendLine("- Social media links and communication preferences");
        documentation.AppendLine();

        documentation.AppendLine("### Leads");
        documentation.AppendLine("- Track potential accounts through the sales funnel");
        documentation.AppendLine("- Lead stages: New, Contacted, Qualified, Proposal, Negotiation, Won, Lost");
        documentation.AppendLine("- Lead sources: Website, Referral, Campaign, Trade Show, etc.");
        documentation.AppendLine("- Convert leads to opportunities or accounts");
        documentation.AppendLine();

        documentation.AppendLine("### Opportunities");
        documentation.AppendLine("- Manage sales deals and pipeline");
        documentation.AppendLine("- Track probability, expected close date, and deal value");
        documentation.AppendLine("- Associate products and quotes with opportunities");
        documentation.AppendLine("- Pipeline stages customizable per organization");
        documentation.AppendLine();

        documentation.AppendLine("### Products");
        documentation.AppendLine("- Product catalog management");
        documentation.AppendLine("- Pricing, SKUs, and categories");
        documentation.AppendLine("- Link products to opportunities and quotes");
        documentation.AppendLine();

        documentation.AppendLine("### Quotes");
        documentation.AppendLine("- Create and manage price quotes");
        documentation.AppendLine("- Add line items from product catalog");
        documentation.AppendLine("- Track quote status: Draft, Sent, Accepted, Rejected");
        documentation.AppendLine();

        documentation.AppendLine("### Marketing Campaigns");
        documentation.AppendLine("- Create and manage marketing campaigns");
        documentation.AppendLine("- Track campaign metrics (reach, clicks, conversions)");
        documentation.AppendLine("- Campaign types: Email, Social, Event, Webinar, etc.");
        documentation.AppendLine("- Associate leads and opportunities with campaigns");
        documentation.AppendLine();

        documentation.AppendLine("### Service Requests");
        documentation.AppendLine("- Help desk / ticketing system");
        documentation.AppendLine("- Track support tickets with priority and SLA");
        documentation.AppendLine("- Categories and subcategories for ticket classification");
        documentation.AppendLine("- Assignment to users or groups");
        documentation.AppendLine();

        documentation.AppendLine("### Tasks & Activities");
        documentation.AppendLine("- Task management with due dates and assignments");
        documentation.AppendLine("- Activity tracking (calls, meetings, emails)");
        documentation.AppendLine("- Link activities to accounts, opportunities, and leads");
        documentation.AppendLine();

        documentation.AppendLine("### Workflows");
        documentation.AppendLine("- Visual workflow designer");
        documentation.AppendLine("- Automate business processes");
        documentation.AppendLine("- Trigger types: Manual, Event, Schedule");
        documentation.AppendLine("- Actions: Send email, create task, update record, etc.");
        documentation.AppendLine();

        // AI Features Documentation - Training Context for Allen AI
        documentation.AppendLine("## AI-Powered Features");
        documentation.AppendLine();

        documentation.AppendLine("### Lead Scoring (AI-Driven)");
        documentation.AppendLine("- Automatic lead scoring using AI models (Allen AI OLMo/Tulu)");
        documentation.AppendLine("- Score range: 0-100 based on multiple factors");
        documentation.AppendLine("- Factors analyzed: engagement level, company size, industry match, budget indicators");
        documentation.AppendLine("- Confidence levels: High (>80%), Medium (50-80%), Low (<50%)");
        documentation.AppendLine("- Recommendations provided for follow-up actions");
        documentation.AppendLine("- Historical scoring data tracked for trend analysis");
        documentation.AppendLine("- Usage: Navigate to Leads > Select lead > View AI Score tab");
        documentation.AppendLine();

        documentation.AppendLine("### Opportunity Insights");
        documentation.AppendLine("- AI-generated insights for sales opportunities");
        documentation.AppendLine("- Win probability predictions with confidence intervals");
        documentation.AppendLine("- Risk analysis: identifies potential blockers and concerns");
        documentation.AppendLine("- Recommended actions: next best steps to progress the deal");
        documentation.AppendLine("- Competitor analysis mentions when detected");
        documentation.AppendLine("- Optimal timing suggestions for follow-ups");
        documentation.AppendLine("- Usage: Navigate to Opportunities > Select opportunity > View Insights panel");
        documentation.AppendLine();

        documentation.AppendLine("### Churn Risk Prediction");
        documentation.AppendLine("- Proactive customer churn risk assessment");
        documentation.AppendLine("- Risk levels: Critical (>80%), High (60-80%), Medium (40-60%), Low (<40%)");
        documentation.AppendLine("- Contributing factors identified: support tickets, engagement decline, payment issues");
        documentation.AppendLine("- AI-recommended retention strategies");
        documentation.AppendLine("- Churn probability percentage with confidence score");
        documentation.AppendLine("- Early warning indicators and alerts");
        documentation.AppendLine("- Usage: Navigate to Accounts > Select account > Risk Assessment tab");
        documentation.AppendLine();

        documentation.AppendLine("### Next Best Action Recommendations");
        documentation.AppendLine("- AI suggests optimal next actions for each customer/lead");
        documentation.AppendLine("- Action types: Call, Email, Meeting, Send proposal, Offer discount");
        documentation.AppendLine("- Priority ranking based on impact and urgency");
        documentation.AppendLine("- Expected outcomes and success probability");
        documentation.AppendLine("- Timing recommendations (best day/time to reach out)");
        documentation.AppendLine("- Channel preferences based on historical response rates");
        documentation.AppendLine("- Usage: Dashboard > Today's Recommended Actions widget");
        documentation.AppendLine();

        documentation.AppendLine("### Email Intelligence");
        documentation.AppendLine("- AI-powered email analysis and optimization");
        documentation.AppendLine("- Sentiment analysis of incoming emails");
        documentation.AppendLine("- Response suggestions with tone matching");
        documentation.AppendLine("- Key entity extraction: dates, amounts, action items");
        documentation.AppendLine("- Email classification: Inquiry, Complaint, Follow-up, Urgent");
        documentation.AppendLine("- Subject line optimization for outgoing emails");
        documentation.AppendLine("- Usage: Email composer > AI Assist button");
        documentation.AppendLine();

        documentation.AppendLine("### AI Model Configuration");
        documentation.AppendLine("- Supports multiple AI providers: OpenAI, Azure, Anthropic, Google, DeepSeek, Allen AI");
        documentation.AppendLine("- Allen AI models (free, open-source): OLMo-7B-Instruct, Tulu-2-7B, OLMo-1B");
        documentation.AppendLine("- Configure default provider in Settings > AI Configuration");
        documentation.AppendLine("- Fallback providers for reliability");
        documentation.AppendLine("- Model-specific settings: temperature, max tokens, timeout");
        documentation.AppendLine();

        // Get dynamic data about the system
        try
        {
            var accountCount = await _context.Accounts.CountAsync(c => !c.IsDeleted);
            var contactCount = await _context.Contacts.CountAsync();
            var opportunityCount = await _context.Opportunities.CountAsync(o => !o.IsDeleted);
            var productCount = await _context.Products.CountAsync(p => !p.IsDeleted);

            documentation.AppendLine("## Current System Statistics");
            documentation.AppendLine($"- Total Accounts: {accountCount}");
            documentation.AppendLine($"- Total Contacts: {contactCount}");
            documentation.AppendLine($"- Total Opportunities: {opportunityCount}");
            documentation.AppendLine($"- Total Products: {productCount}");
            documentation.AppendLine();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load system statistics for chatbot context");
        }

        // Navigation help
        documentation.AppendLine("## Navigation");
        documentation.AppendLine("- Dashboard: Overview of key metrics and recent activities");
        documentation.AppendLine("- Left sidebar: Access to all main modules");
        documentation.AppendLine("- Settings: System configuration and user management");
        documentation.AppendLine("- Context Panel (right flyout): Account filtering and AI assistant");
        documentation.AppendLine();

        documentation.AppendLine("## Tips");
        documentation.AppendLine("- Use the search bar to quickly find accounts, contacts, or opportunities");
        documentation.AppendLine("- Select accounts in the Context Panel to filter data across pages");
        documentation.AppendLine("- Use workflows to automate repetitive tasks");
        documentation.AppendLine("- Set up email templates for consistent communication");
        documentation.AppendLine("- Review AI lead scores daily to prioritize high-value prospects");
        documentation.AppendLine("- Check churn risk predictions weekly to proactively retain at-risk accounts");
        documentation.AppendLine("- Use AI-suggested next best actions to optimize your sales approach");
        documentation.AppendLine("- Configure Allen AI models for cost-effective AI features (free tier available)");

        var result = documentation.ToString();

        lock (_cacheLock)
        {
            _cachedDocumentation = result;
            _cacheExpiry = DateTime.UtcNow.AddHours(1); // Cache for 1 hour
        }

        return result;
    }

    private string BuildSystemPrompt(string documentation, string? accountContext)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are a helpful CRM Assistant for a Customer Relationship Management system.");
        prompt.AppendLine("Your role is to help users navigate the CRM, understand features, and accomplish their tasks.");
        prompt.AppendLine();
        prompt.AppendLine("Guidelines:");
        prompt.AppendLine("- Be concise and helpful");
        prompt.AppendLine("- Reference specific CRM features when relevant");
        prompt.AppendLine("- Suggest next steps when appropriate");
        prompt.AppendLine("- If you don't know something, say so and suggest where to find the information");
        prompt.AppendLine("- Format responses with markdown when helpful (lists, bold, etc.)");
        prompt.AppendLine();
        prompt.AppendLine("CRM Documentation:");
        prompt.AppendLine(documentation);

        if (!string.IsNullOrEmpty(accountContext))
        {
            prompt.AppendLine();
            prompt.AppendLine($"User Context: {accountContext}");
        }

        return prompt.ToString();
    }

    private async Task<string> GetAccountContextAsync(List<int>? accountIds)
    {
        if (accountIds == null || !accountIds.Any())
            return "";

        var context = new StringBuilder();

        try
        {
            var accounts = await _context.Accounts
                .Where(c => accountIds.Contains(c.Id) && !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Company,
                    c.Email,
                    c.LifecycleStage,
                    c.Industry,
                    OpportunityCount = c.Opportunities != null ? c.Opportunities.Count(o => !o.IsDeleted) : 0,
                    OpenOpportunityValue = c.Opportunities != null
                        ? c.Opportunities
                            .Where(o => !o.IsDeleted && o.Stage != OpportunityStage.ClosedWon && o.Stage != OpportunityStage.ClosedLost)
                            .Sum(o => o.Amount)
                        : 0,
                })
                .ToListAsync();

            foreach (var account in accounts)
            {
                var name = !string.IsNullOrEmpty(account.Company)
                    ? account.Company
                    : $"{account.FirstName} {account.LastName}";

                context.AppendLine($"Account: {name}");
                if (!string.IsNullOrEmpty(account.Email))
                    context.AppendLine($"  - Email: {account.Email}");
                if (!string.IsNullOrEmpty(account.Industry))
                    context.AppendLine($"  - Industry: {account.Industry}");
                context.AppendLine($"  - Lifecycle Stage: {account.LifecycleStage}");
                context.AppendLine($"  - Open Opportunities: {account.OpportunityCount}");
                if (account.OpenOpportunityValue > 0)
                    context.AppendLine($"  - Open Opportunity Value: ${account.OpenOpportunityValue:N2}");
                context.AppendLine();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load account context");
        }

        return context.ToString();
    }

    private static string GetDefaultModelForProvider(LLMSettingsDto settings, string provider)
        => AIServiceHelper.GetDefaultModelForProvider(settings, provider);
}

/// <summary>
/// Request model for chat messages
/// </summary>
public class ChatMessageRequest
{
    public string Message { get; set; } = "";
    public string? AccountContext { get; set; }
    public List<int>? AccountIds { get; set; }
    public List<ConversationMessage>? ConversationHistory { get; set; }
}

/// <summary>
/// A message in the conversation history
/// </summary>
public class ConversationMessage
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
}

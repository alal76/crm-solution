// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CRM.Api.Controllers;

/// <summary>
/// AI Chatbot controller for CRM Assistant functionality
/// Provides conversational AI interface with CRM documentation context
/// </summary>
[ApiController]
[Route("api/ai/chatbot")]
[Authorize]
public class AIChatbotController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILLMService _llmService;
    private readonly ILLMSettingsService _llmSettingsService;
    private readonly ILogger<AIChatbotController> _logger;
    
    // Cached documentation context
    private static string? _cachedDocumentation;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly object _cacheLock = new();

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
    /// Initialize chatbot context by loading CRM documentation
    /// </summary>
    [HttpPost("initialize")]
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
    /// Send a message to the AI chatbot
    /// </summary>
    [HttpPost("message")]
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
            if (settings == null || string.IsNullOrEmpty(settings.DefaultProvider))
            {
                return Ok(new { 
                    response = "I apologize, but the AI service is not configured. Please contact your administrator to set up LLM settings." 
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

            // Get the default model from the provider settings
            var defaultModel = GetDefaultModelForProvider(settings, settings.DefaultProvider);

            // Make LLM request
            var llmRequest = new LLMRequest
            {
                Provider = settings.DefaultProvider,
                Model = defaultModel,
                Messages = messages,
                Temperature = 0.7,
                MaxTokens = 1500,
            };

            var response = await _llmService.ChatAsync(llmRequest);

            if (response.Success)
            {
                return Ok(new { response = response.Content });
            }
            else
            {
                _logger.LogWarning("LLM request failed: {Error}", response.Error);
                return Ok(new { 
                    response = "I'm having trouble processing your request right now. Please try again or rephrase your question." 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message");
            return Ok(new { 
                response = "An error occurred while processing your message. Please try again." 
            });
        }
    }

    /// <summary>
    /// Get quick suggestions based on context
    /// </summary>
    [HttpGet("suggestions")]
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
                suggestions.InsertRange(0, new[]
                {
                    "Show me recent activities for this customer",
                    "What opportunities are open for this account?",
                    "How do I add a contact to this customer?"
                });
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
        
        documentation.AppendLine("### Customers/Accounts");
        documentation.AppendLine("- Manage both individual and organization customers");
        documentation.AppendLine("- Track customer information including contacts, addresses, and communication history");
        documentation.AppendLine("- Customer lifecycle stages: Lead, Prospect, Customer, Churned");
        documentation.AppendLine("- Customer categories: Individual (B2C) and Organization (B2B)");
        documentation.AppendLine();

        documentation.AppendLine("### Contacts");
        documentation.AppendLine("- Store contact information linked to customers");
        documentation.AppendLine("- Multiple phone numbers, emails, and addresses per contact");
        documentation.AppendLine("- Social media links and communication preferences");
        documentation.AppendLine();

        documentation.AppendLine("### Leads");
        documentation.AppendLine("- Track potential customers through the sales funnel");
        documentation.AppendLine("- Lead stages: New, Contacted, Qualified, Proposal, Negotiation, Won, Lost");
        documentation.AppendLine("- Lead sources: Website, Referral, Campaign, Trade Show, etc.");
        documentation.AppendLine("- Convert leads to opportunities or customers");
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
        documentation.AppendLine("- Link activities to customers, opportunities, and leads");
        documentation.AppendLine();

        documentation.AppendLine("### Workflows");
        documentation.AppendLine("- Visual workflow designer");
        documentation.AppendLine("- Automate business processes");
        documentation.AppendLine("- Trigger types: Manual, Event, Schedule");
        documentation.AppendLine("- Actions: Send email, create task, update record, etc.");
        documentation.AppendLine();

        // Get dynamic data about the system
        try
        {
            var customerCount = await _context.Customers.CountAsync(c => !c.IsDeleted);
            var contactCount = await _context.Contacts.CountAsync();
            var opportunityCount = await _context.Opportunities.CountAsync(o => !o.IsDeleted);
            var productCount = await _context.Products.CountAsync(p => !p.IsDeleted);
            
            documentation.AppendLine("## Current System Statistics");
            documentation.AppendLine($"- Total Customers: {customerCount}");
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
        documentation.AppendLine("- Use the search bar to quickly find customers, contacts, or opportunities");
        documentation.AppendLine("- Select accounts in the Context Panel to filter data across pages");
        documentation.AppendLine("- Use workflows to automate repetitive tasks");
        documentation.AppendLine("- Set up email templates for consistent communication");

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
            var accounts = await _context.Customers
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

    private string GetDefaultModelForProvider(LLMSettingsDto settings, string provider)
    {
        return provider.ToLower() switch
        {
            "openai" => settings.OpenAI?.DefaultModel ?? "gpt-4",
            "azure" or "azureopenai" => settings.Azure?.DefaultModel ?? "gpt-4",
            "anthropic" => settings.Anthropic?.DefaultModel ?? "claude-3-sonnet-20240229",
            "google" or "gemini" => settings.Google?.DefaultModel ?? "gemini-1.5-pro",
            "bedrock" or "aws" => settings.Bedrock?.DefaultModel ?? "anthropic.claude-3-sonnet-20240229-v1:0",
            "deepseek" => settings.DeepSeek?.DefaultModel ?? "deepseek-chat",
            "local" or "ollama" => settings.Local?.DefaultModel ?? "llama3",
            _ => "gpt-4"
        };
    }
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

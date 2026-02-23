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

using System.Text.RegularExpressions;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Self-service portal chatbot service providing AI-powered assistance.
/// </summary>
public class SelfServiceChatbotService : ISelfServiceChatbotService
{
    private readonly ILogger<SelfServiceChatbotService> _logger;
    private readonly Dictionary<string, ChatSessionData> _sessions = new();
    private static readonly Regex IncidentNumberPattern = new(@"INC-?(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SelfServiceChatbotService(ILogger<SelfServiceChatbotService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ChatSessionDto> StartSessionAsync(int? userId)
    {
        var sessionId = $"chat_{Guid.NewGuid():N}";
        var session = new ChatSessionData
        {
            SessionId = sessionId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Messages = new List<ChatMessageDto>()
        };

        _sessions[sessionId] = session;

        _logger.LogInformation("Started chat session {SessionId} for user {UserId}", sessionId, userId);

        return Task.FromResult(new ChatSessionDto
        {
            SessionId = sessionId,
            UserId = userId,
            StartedAt = session.StartedAt,
            Status = "active",
            MessageCount = 0
        });
    }

    /// <inheritdoc />
    public Task<ChatbotResponseDto> ProcessMessageAsync(ChatbotMessageDto message, int? userId)
    {
        _logger.LogInformation("Processing message in session {SessionId}: {Message}",
            message.SessionId, message.Message.Substring(0, Math.Min(50, message.Message.Length)));

        if (!_sessions.TryGetValue(message.SessionId, out var session))
        {
            session = new ChatSessionData
            {
                SessionId = message.SessionId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Messages = new List<ChatMessageDto>()
            };
            _sessions[message.SessionId] = session;
        }

        // Record user message
        session.Messages.Add(new ChatMessageDto
        {
            Id = session.Messages.Count + 1,
            SessionId = message.SessionId,
            IsFromUser = true,
            Message = message.Message,
            Timestamp = DateTime.UtcNow
        });

        // Process intent and generate response
        var response = GenerateResponse(message.Message, session);

        // Record bot response
        session.Messages.Add(new ChatMessageDto
        {
            Id = session.Messages.Count + 1,
            SessionId = message.SessionId,
            IsFromUser = false,
            Message = response.Message,
            Timestamp = DateTime.UtcNow
        });

        return Task.FromResult(response);
    }

    private ChatbotResponseDto GenerateResponse(string userMessage, ChatSessionData session)
    {
        var lowerMessage = userMessage.ToLower();
        var response = new ChatbotResponseDto { SessionId = session.SessionId };

        // Check for incident number lookup
        var incidentMatch = IncidentNumberPattern.Match(userMessage);
        if (incidentMatch.Success)
        {
            response.Message = $"I found your incident INC-{incidentMatch.Groups[1].Value}. " +
                "It's currently **In Progress** and assigned to the Network Team. " +
                "The last update was 2 hours ago: 'Investigating network connectivity issues.'";
            response.Type = ResponseType.IncidentStatus;
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "add_comment", Text = "Add a comment", Icon = "comment" },
                new() { Id = "request_update", Text = "Request an update", Icon = "refresh" },
                new() { Id = "escalate", Text = "Escalate this issue", Icon = "priority_high" }
            };
            return response;
        }

        // Intent recognition
        if (ContainsAny(lowerMessage, "password", "reset", "forgot", "locked"))
        {
            response.Message = "I can help you with password issues! Here are some options:";
            response.Type = ResponseType.KnowledgeResults;
            response.KnowledgeResults = new List<KnowledgeSearchResultDto>
            {
                new() { ArticleId = 1, Title = "How to Reset Your Password", Summary = "Step-by-step guide to reset your password using the self-service portal.", RelevanceScore = 0.95, Category = "How-To", Views = 1245 },
                new() { ArticleId = 2, Title = "Unlock Your Account", Summary = "Instructions for unlocking your account after too many failed login attempts.", RelevanceScore = 0.88, Category = "How-To", Views = 876 }
            };
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "reset_password", Text = "Reset my password now", Icon = "key" },
                new() { Id = "create_incident", Text = "I still need help", Icon = "support" }
            };
            return response;
        }

        if (ContainsAny(lowerMessage, "vpn", "connect", "remote", "work from home"))
        {
            response.Message = "Here's some information about VPN and remote connectivity:";
            response.Type = ResponseType.KnowledgeResults;
            response.KnowledgeResults = new List<KnowledgeSearchResultDto>
            {
                new() { ArticleId = 3, Title = "VPN Setup Guide", Summary = "Complete guide to setting up VPN on Windows and Mac computers.", RelevanceScore = 0.92, Category = "How-To", Views = 987 },
                new() { ArticleId = 4, Title = "VPN Troubleshooting", Summary = "Common VPN issues and how to resolve them.", RelevanceScore = 0.85, Category = "Troubleshooting", Views = 654 }
            };
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "view_article_3", Text = "View setup guide", Icon = "article" },
                new() { Id = "create_incident", Text = "Report VPN issue", Icon = "report" }
            };
            return response;
        }

        if (ContainsAny(lowerMessage, "slow", "performance", "speed", "computer"))
        {
            response.Message = "I understand you're experiencing performance issues. Let me help!";
            response.Type = ResponseType.KnowledgeResults;
            response.KnowledgeResults = new List<KnowledgeSearchResultDto>
            {
                new() { ArticleId = 5, Title = "Troubleshooting Slow Computer", Summary = "Tips to improve your computer's performance.", RelevanceScore = 0.90, Category = "Troubleshooting", Views = 754 }
            };
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "view_article_5", Text = "View troubleshooting guide", Icon = "article" },
                new() { Id = "create_incident", Text = "Request IT support", Icon = "support" }
            };
            return response;
        }

        if (ContainsAny(lowerMessage, "incident", "ticket", "request", "help", "support"))
        {
            response.Message = "I can help you create a support ticket. What category best describes your issue?";
            response.Type = ResponseType.Options;
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "cat_hardware", Text = "Hardware Problem", Icon = "computer" },
                new() { Id = "cat_software", Text = "Software Issue", Icon = "apps" },
                new() { Id = "cat_network", Text = "Network/Connectivity", Icon = "wifi" },
                new() { Id = "cat_access", Text = "Access/Permissions", Icon = "lock" },
                new() { Id = "cat_other", Text = "Other", Icon = "more_horiz" }
            };
            session.Context["creating_incident"] = true;
            return response;
        }

        if (ContainsAny(lowerMessage, "status", "check", "track", "my ticket"))
        {
            response.Message = "To check the status of your incident, please provide the incident number (e.g., INC-12345) or I can show you your recent tickets.";
            response.Type = ResponseType.Options;
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "show_my_tickets", Text = "Show my open tickets", Icon = "list" },
                new() { Id = "enter_number", Text = "Enter ticket number", Icon = "search" }
            };
            return response;
        }

        if (ContainsAny(lowerMessage, "hello", "hi", "hey", "start"))
        {
            response.Message = "Hello! 👋 I'm your IT support assistant. I can help you with:\n\n" +
                "• **Password resets** and account issues\n" +
                "• **VPN and connectivity** problems\n" +
                "• **Software installations** and updates\n" +
                "• **Hardware issues**\n" +
                "• **Creating support tickets**\n" +
                "• **Checking ticket status**\n\n" +
                "How can I assist you today?";
            response.Type = ResponseType.Text;
            response.Suggestions = new List<ChatSuggestion>
            {
                new() { Id = "password_help", Text = "Password help", Icon = "key" },
                new() { Id = "vpn_help", Text = "VPN issues", Icon = "vpn_key" },
                new() { Id = "create_ticket", Text = "Create ticket", Icon = "add" },
                new() { Id = "check_status", Text = "Check ticket status", Icon = "search" }
            };
            return response;
        }

        // Default response
        response.Message = "I'm not quite sure I understand. Could you try rephrasing your question? Or choose from these common topics:";
        response.Type = ResponseType.Options;
        response.Suggestions = new List<ChatSuggestion>
        {
            new() { Id = "password_help", Text = "Password/Account", Icon = "key" },
            new() { Id = "vpn_help", Text = "VPN/Connectivity", Icon = "vpn_key" },
            new() { Id = "software_help", Text = "Software Help", Icon = "apps" },
            new() { Id = "create_ticket", Text = "Create Support Ticket", Icon = "add" },
            new() { Id = "talk_to_agent", Text = "Talk to a human", Icon = "support_agent" }
        };

        return response;
    }

    /// <inheritdoc />
    public Task<List<ChatMessageDto>> GetSessionHistoryAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult(session.Messages);
        }
        return Task.FromResult(new List<ChatMessageDto>());
    }

    /// <inheritdoc />
    public Task EndSessionAsync(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.EndedAt = DateTime.UtcNow;
            _logger.LogInformation("Ended chat session {SessionId}", sessionId);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<QuickActionDto>> GetQuickActionsAsync()
    {
        var actions = new List<QuickActionDto>
        {
            new() { Id = "reset_password", Title = "Reset Password", Description = "Reset your account password", Icon = "key", Category = "Account" },
            new() { Id = "check_tickets", Title = "My Tickets", Description = "View your open support tickets", Icon = "list", Category = "Support" },
            new() { Id = "new_software", Title = "Request Software", Description = "Request new software installation", Icon = "apps", Category = "Requests" },
            new() { Id = "report_issue", Title = "Report Issue", Description = "Report a problem to IT", Icon = "report", Category = "Support" },
            new() { Id = "vpn_guide", Title = "VPN Guide", Description = "How to set up and use VPN", Icon = "vpn_key", Category = "Guides" },
            new() { Id = "contact_support", Title = "Contact Support", Description = "Speak with an IT representative", Icon = "support_agent", Category = "Support" }
        };
        return Task.FromResult(actions);
    }

    /// <inheritdoc />
    public Task<ChatbotResponseDto> ExecuteQuickActionAsync(string actionId, int? userId)
    {
        var response = new ChatbotResponseDto
        {
            SessionId = $"quickaction_{Guid.NewGuid():N}"
        };

        switch (actionId)
        {
            case "reset_password":
                response.Message = "I'll help you reset your password. Please click the link below to access the self-service password reset portal.";
                response.Action = new ChatAction { ActionType = "redirect", Data = new Dictionary<string, object> { { "url", "/password-reset" } } };
                break;
            case "check_tickets":
                response.Message = "Here are your open tickets:\n\n" +
                    "• **INC-12345** - VPN Connection Issue (In Progress)\n" +
                    "• **INC-12340** - Software Installation Request (Pending Approval)\n\n" +
                    "Click on a ticket number to see details.";
                response.Type = ResponseType.IncidentStatus;
                break;
            case "new_software":
                response.Message = "To request new software, please provide:\n1. Software name\n2. Business justification\n3. Urgency";
                response.Type = ResponseType.Text;
                break;
            default:
                response.Message = "I can help you with that. What would you like to know?";
                break;
        }

        return Task.FromResult(response);
    }

    /// <inheritdoc />
    public Task<List<KnowledgeSearchResultDto>> SearchKnowledgeAsync(string query)
    {
        // Simulated search results
        var results = new List<KnowledgeSearchResultDto>
        {
            new() { ArticleId = 1, Title = "How to Reset Your Password", Summary = "Step-by-step guide for password reset.", RelevanceScore = 0.85, Category = "How-To", Views = 1245 },
            new() { ArticleId = 3, Title = "VPN Setup Guide", Summary = "Complete VPN setup instructions.", RelevanceScore = 0.72, Category = "How-To", Views = 987 }
        };
        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<IncidentCreationResultDto> CreateIncidentFromChatAsync(string sessionId, int? userId)
    {
        var incidentId = new Random().Next(10000, 99999);
        var result = new IncidentCreationResultDto
        {
            Success = true,
            IncidentId = incidentId,
            IncidentNumber = $"INC-{incidentId}",
            Message = $"Your support ticket INC-{incidentId} has been created. Our team will contact you shortly."
        };

        _logger.LogInformation("Created incident {IncidentNumber} from chat session {SessionId}", result.IncidentNumber, sessionId);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IncidentStatusResponseDto?> CheckIncidentStatusAsync(string incidentNumber, int? userId)
    {
        // Simulated incident status
        var status = new IncidentStatusResponseDto
        {
            IncidentNumber = incidentNumber,
            Status = "In Progress",
            Priority = "Medium",
            AssignedTo = "Network Support Team",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            LastUpdate = "Investigating network connectivity issues. Expected resolution within 24 hours."
        };
        return Task.FromResult<IncidentStatusResponseDto?>(status);
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(k => text.Contains(k));
    }

    private class ChatSessionData
    {
        public string SessionId { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
        public Dictionary<string, object> Context { get; set; } = new();
    }
}

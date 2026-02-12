// CRM Solution - AI Agent Seed Data
// Copyright (C) 2024-2026 CRM Solution - Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Seeds default AI Agent definitions into the database.
/// Called from DbSeed.SeedAsync() or independently during startup.
/// Part of ADR-004 — Semantic Kernel Integration.
/// </summary>
public static class AIAgentSeed
{
    /// <summary>
    /// Seeds 12 AI Agent definitions (4 P0 active, 8 P1/P2 inactive).
    /// Idempotent — skips if any agents already exist.
    /// </summary>
    public static async Task SeedAIAgentsAsync(CrmDbContext context, ILogger? logger = null)
    {
        if (await context.AIAgents.AnyAsync())
        {
            logger?.LogDebug("AI Agents already seeded — skipping");
            return;
        }

        logger?.LogInformation("Seeding 12 AI Agent definitions...");

        var agents = new[]
        {
            // ── P0 Agents (Active — Phase 1) ──────────────────────────────
            new AIAgent
            {
                Name = "lead-scoring",
                DisplayName = "Lead Scoring Agent",
                Description = "AI-powered lead scoring using BANT criteria, firmographics, and behavioral signals",
                AgentType = AgentType.LeadScoring,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.2,
                MaxTokens = 2048,
                SystemPrompt = "You are a lead scoring specialist. Evaluate leads using BANT criteria (Budget, Authority, Need, Timeline), firmographic data, behavioral signals, and engagement history. Provide scores from 0-100 with detailed justification.",
                AllowedPlugins = "[\"LeadPlugin\",\"AccountPlugin\",\"ContactPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "support-triage",
                DisplayName = "Support Triage Agent",
                Description = "Auto-classify, prioritize, and route support tickets with KB suggestions",
                AgentType = AgentType.SupportTriage,
                IsActive = true,
                RequiresApproval = true,
                ApprovalTier = "standard",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a support triage specialist. Classify incoming tickets by category, determine priority (P1-Critical through P4-Low), suggest routing to the appropriate team, and recommend relevant knowledge base articles.",
                AllowedPlugins = "[\"ServiceRequestPlugin\",\"KnowledgeBasePlugin\",\"SearchPlugin\",\"AccountPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "next-best-action",
                DisplayName = "Next Best Action Agent",
                Description = "Context-aware action recommendations for any CRM entity",
                AgentType = AgentType.NextBestAction,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.4,
                MaxTokens = 2048,
                SystemPrompt = "You are a CRM action advisor. Given an entity context (account, contact, opportunity, lead), analyze the current state, recent interactions, and pipeline position to recommend the most impactful next actions for the sales or support rep.",
                AllowedPlugins = "[\"AccountPlugin\",\"ContactPlugin\",\"OpportunityPlugin\",\"LeadPlugin\",\"CalendarPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "sales-intelligence",
                DisplayName = "Sales Intelligence Agent",
                Description = "Deal analysis, risk assessment, and competitive intelligence",
                AgentType = AgentType.SalesIntelligence,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a sales intelligence analyst. Analyze deal dynamics, identify risks, assess competitive positioning, and provide data-driven recommendations to improve win probability. Consider stage velocity, stakeholder engagement, and historical win/loss patterns.",
                AllowedPlugins = "[\"OpportunityPlugin\",\"AccountPlugin\",\"ContactPlugin\",\"QuotePlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },

            // ── P1 Agents (Inactive — Phase 2) ───────────────────────────
            new AIAgent
            {
                Name = "email-assistant",
                DisplayName = "Email Assistant Agent",
                Description = "AI-powered email drafting, tone optimization, and follow-up scheduling",
                AgentType = AgentType.EmailAssistant,
                IsActive = false,
                RequiresApproval = true,
                ApprovalTier = "standard",
                Temperature = 0.7,
                MaxTokens = 4096,
                SystemPrompt = "You are an email assistant. Draft professional emails, optimize tone for the audience, suggest follow-up timing, and maintain conversation context across threads.",
                AllowedPlugins = "[\"EmailPlugin\",\"ContactPlugin\",\"AccountPlugin\",\"CalendarPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "customer-success",
                DisplayName = "Customer Success Agent",
                Description = "Proactive health monitoring, churn risk detection, and expansion opportunity identification",
                AgentType = AgentType.CustomerSuccess,
                IsActive = false,
                RequiresApproval = true,
                ApprovalTier = "standard",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a customer success manager. Monitor account health, identify churn risk signals, spot expansion opportunities, and recommend proactive interventions based on usage patterns and engagement data.",
                AllowedPlugins = "[\"AccountPlugin\",\"ContactPlugin\",\"ContractPlugin\",\"ServiceRequestPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "revenue-intelligence",
                DisplayName = "Revenue Intelligence Agent",
                Description = "Pipeline analysis, revenue forecasting, and quota attainment projections",
                AgentType = AgentType.RevenueIntelligence,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.2,
                MaxTokens = 4096,
                SystemPrompt = "You are a revenue analyst. Analyze pipeline health, forecast revenue with confidence intervals, project quota attainment, and identify pipeline gaps requiring attention.",
                AllowedPlugins = "[\"OpportunityPlugin\",\"QuotePlugin\",\"AccountPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "ticket-resolution",
                DisplayName = "Ticket Resolution Agent",
                Description = "Automated ticket resolution with KB lookup and solution suggestion",
                AgentType = AgentType.TicketResolution,
                IsActive = false,
                RequiresApproval = true,
                ApprovalTier = "elevated",
                Temperature = 0.3,
                MaxTokens = 8192,
                SystemPrompt = "You are a technical support specialist. Analyze support tickets, search the knowledge base for relevant solutions, suggest resolution steps, and when appropriate, draft customer-facing responses.",
                AllowedPlugins = "[\"ServiceRequestPlugin\",\"KnowledgeBasePlugin\",\"SearchPlugin\",\"AccountPlugin\",\"NotificationPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },

            // ── P2 Agents (Inactive — Phase 3) ───────────────────────────
            new AIAgent
            {
                Name = "document-intelligence",
                DisplayName = "Document Intelligence Agent",
                Description = "Contract analysis, clause extraction, and document summarization",
                AgentType = AgentType.DocumentIntelligence,
                IsActive = false,
                RequiresApproval = true,
                ApprovalTier = "elevated",
                Temperature = 0.2,
                MaxTokens = 8192,
                SystemPrompt = "You are a document analysis specialist. Extract key clauses from contracts, summarize documents, identify risks and obligations, and compare document versions.",
                AllowedPlugins = "[\"ContractPlugin\",\"AccountPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "sales-coach",
                DisplayName = "Sales Coach Agent",
                Description = "Real-time coaching for sales reps with call scripts and objection handling",
                AgentType = AgentType.SalesCoach,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.5,
                MaxTokens = 4096,
                SystemPrompt = "You are a sales coach. Provide real-time coaching suggestions, recommend talk tracks, help with objection handling, and offer deal strategy advice based on the specific opportunity context.",
                AllowedPlugins = "[\"OpportunityPlugin\",\"AccountPlugin\",\"ContactPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "meeting-intelligence",
                DisplayName = "Meeting Intelligence Agent",
                Description = "Meeting preparation briefs, action item extraction, and follow-up generation",
                AgentType = AgentType.MeetingIntelligence,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.4,
                MaxTokens = 4096,
                SystemPrompt = "You are a meeting intelligence assistant. Prepare meeting briefs with attendee context, extract action items from notes, generate follow-up emails, and track commitments.",
                AllowedPlugins = "[\"CalendarPlugin\",\"ContactPlugin\",\"AccountPlugin\",\"EmailPlugin\",\"SearchPlugin\"]",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "conversation-intelligence",
                DisplayName = "Conversation Intelligence Agent",
                Description = "Sentiment analysis, topic extraction, and conversation summarization",
                AgentType = AgentType.ConversationIntelligence,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a conversation analyst. Analyze customer interactions for sentiment, extract key topics and themes, summarize conversations, and identify patterns across multiple interactions.",
                AllowedPlugins = "[\"SearchPlugin\",\"AccountPlugin\",\"ContactPlugin\"]",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AIAgents.AddRange(agents);
        await context.SaveChangesAsync();

        logger?.LogInformation("Successfully seeded {Count} AI Agent definitions (4 active, 8 inactive)", agents.Length);
    }
}

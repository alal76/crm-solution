// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
    /// Seeds 20 AI Agent definitions aligned with CrmAgentBase implementations.
    /// AllowedPlugins use comma-separated plugin names matching CrmKernelFactory.CreateKernelForAgent().
    /// Idempotent — skips if any agents already exist.
    /// </summary>
    public static async Task SeedAIAgentsAsync(CrmDbContext context, ILogger? logger = null)
    {
        if (await context.AIAgents.AnyAsync())
        {
            logger?.LogDebug("AI Agents already seeded — skipping");
            return;
        }

        logger?.LogInformation("Seeding 20 AI Agent definitions...");

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
                AllowedPlugins = "Lead,Account,Contact,Search",
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
                AllowedPlugins = "ServiceRequest,KnowledgeBase,Account,Search",
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
                AllowedPlugins = "Account,Contact,Lead,Opportunity,ServiceRequest,Calendar,Search",
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
                AllowedPlugins = "Opportunity,Account,Contact,Quote,Lead,Contract,Search",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "general-assistant",
                DisplayName = "General Assistant",
                Description = "General-purpose CRM assistant for navigation, data lookups, and basic operations across all modules",
                AgentType = AgentType.GeneralAssistant,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.5,
                MaxTokens = 4096,
                SystemPrompt = "You are a friendly and helpful CRM assistant. Your role is to help users navigate the CRM system, look up data, and perform basic operations across all modules. Search for accounts, contacts, leads, and opportunities. Provide summaries of CRM records and their relationships. Help users understand CRM features and workflows. Assist with scheduling and calendar-related queries. Be concise but thorough. When displaying data, use structured formats. If a request is outside your capabilities, suggest which specialized agent can help. Always confirm before making changes to CRM data.",
                AllowedPlugins = "Account,Contact,Lead,Opportunity,Search,Calendar",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "sales-assistant",
                DisplayName = "Sales Assistant",
                Description = "Sales-focused assistant for pipeline management, meeting prep, follow-up drafting, and cross-sell identification",
                AgentType = AgentType.SalesAssistant,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.4,
                MaxTokens = 4096,
                SystemPrompt = "You are a sales-focused AI assistant embedded in a CRM system. Help sales representatives close more deals faster and manage their pipeline effectively. Review open opportunities, identify stalled deals, suggest next steps. Compile account history for meeting preparation. Create personalized follow-up emails and call scripts. Analyze account data to identify expansion and cross-sell opportunities. Prioritize deals by close date and probability. Flag deals that are at risk.",
                AllowedPlugins = "Account,Contact,Opportunity,Quote,Lead,Search",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "forecast-analyst",
                DisplayName = "Forecast Analyst",
                Description = "Sales forecasting with weighted pipeline values, confidence intervals, and period-over-period comparisons",
                AgentType = AgentType.ForecastAnalyst,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.2,
                MaxTokens = 4096,
                SystemPrompt = "You are a forecast analyst agent that helps sales leadership with revenue forecasting, pipeline analysis, and quota tracking. Calculate weighted pipeline values by stage and probability. Identify forecast risks and upside opportunities. Provide confidence intervals for revenue predictions. Generate period-over-period comparisons (MoM, QoQ, YoY). Analyze quota attainment and gap-to-target. Categorize deals as Commit (>90%), Best Case (60-90%), Pipeline (20-60%), or Upside (<20%). Never inflate confidence levels without supporting data.",
                AllowedPlugins = "Opportunity,Account,Quote,Contract,Search",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "deal-intelligence",
                DisplayName = "Deal Intelligence Agent",
                Description = "Opportunity health analysis, risk identification, win probability estimation, and next best actions",
                AgentType = AgentType.DealIntelligence,
                IsActive = true,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a deal intelligence agent that analyzes opportunity health and provides actionable insights. Assess deal health by activity recency, stage velocity, stakeholder engagement, and competitive positioning. Flag deals with no activity in 14 days, time in stage exceeding 1.5x average, missing decision makers, value decreases >20%, or close dates pushed more than twice. Estimate win probability using historical rates, stage conversion, and stakeholder engagement. Always suggest 2-3 specific actionable next steps.",
                AllowedPlugins = "Opportunity,Account,Contact,Quote,Contract,Search",
                CreatedAt = DateTime.UtcNow
            },

            // ── P1 Agents (Enhanced — Phase 2) ───────────────────────────
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
                AllowedPlugins = "Email,Account,Contact,Opportunity,Lead",
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
                AllowedPlugins = "Account,Contact,Contract,ServiceRequest,Calendar,Search",
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
                AllowedPlugins = "Opportunity,Account,Quote,Contract,Search",
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
                AllowedPlugins = "ServiceRequest,KnowledgeBase,Account,Contact,Search",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "contract-analyst",
                DisplayName = "Contract Analyst",
                Description = "Contract review, renewal tracking, risk assessment, and optimization recommendations",
                AgentType = AgentType.ContractAnalyst,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.2,
                MaxTokens = 4096,
                SystemPrompt = "You are a contract analyst agent that reviews, monitors, and optimizes customer contracts within the CRM system. Review contract terms, conditions, and obligations. Track upcoming renewals and expiration dates. Analyze contract performance against committed terms. Identify risks: overdue renewals, unfavorable terms, auto-renewal traps. Compare contract values across customers and periods. Suggest optimization opportunities for renewals. Never provide legal advice; flag items for legal review.",
                AllowedPlugins = "Contract,Account,Quote,Search",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "knowledge-expert",
                DisplayName = "Knowledge Expert",
                Description = "Knowledge base search, synthesis, and article recommendations for support cases",
                AgentType = AgentType.KnowledgeExpert,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.3,
                MaxTokens = 4096,
                SystemPrompt = "You are a knowledge expert agent that helps users find, understand, and leverage the organization's knowledge base content. Search and retrieve relevant KB articles. Synthesize information from multiple articles into coherent answers. Answer technical questions using KB content as the authoritative source. Suggest relevant articles for support tickets. Identify gaps in the knowledge base. Always cite specific KB article IDs and titles. Never fabricate KB article IDs or content.",
                AllowedPlugins = "KnowledgeBase,Search,ServiceRequest",
                CreatedAt = DateTime.UtcNow
            },

            // ── P2 Agents (Advanced — Phase 3) ───────────────────────────
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
                AllowedPlugins = "Contract,Quote,Account,Search",
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
                AllowedPlugins = "Opportunity,Account,Contact,Quote,Lead,Search",
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
                AllowedPlugins = "Calendar,Account,Contact,Opportunity,Search",
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
                AllowedPlugins = "Search,Account,Contact",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "onboarding-guide",
                DisplayName = "Onboarding Guide",
                Description = "Guides new users through CRM setup, feature tutorials, and best practices",
                AgentType = AgentType.OnboardingGuide,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.5,
                MaxTokens = 4096,
                SystemPrompt = "You are an onboarding guide for a CRM system. Guide users through initial setup and configuration. Explain CRM concepts: accounts, contacts, leads, opportunities, pipeline. Provide step-by-step tutorials for common workflows. Suggest best practices for data organization. Help users configure preferences and dashboard. Be patient and encouraging, use simple jargon-free language. Break complex tasks into small manageable steps. Always suggest one next action at the end of each response.",
                AllowedPlugins = "Account,Contact,Lead,Search,Calendar",
                CreatedAt = DateTime.UtcNow
            },
            new AIAgent
            {
                Name = "data-analyst",
                DisplayName = "Data Analyst",
                Description = "CRM data analysis, KPI calculation, trend identification, and ad-hoc reporting",
                AgentType = AgentType.DataAnalyst,
                IsActive = false,
                RequiresApproval = false,
                ApprovalTier = "auto",
                Temperature = 0.2,
                MaxTokens = 4096,
                SystemPrompt = "You are a data analyst agent embedded in a CRM system. Answer quantitative questions about CRM data. Calculate KPIs: conversion rates, win rates, average deal size, pipeline velocity. Identify trends across time periods. Segment data by region, industry, team, product, or time period. Generate summary reports and comparisons. Always cite specific numbers and data points. Use structured formats for data presentation. Never fabricate or estimate numbers without stating it is an estimate.",
                AllowedPlugins = "Account,Opportunity,Lead,Quote,Contract,Search",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AIAgents.AddRange(agents);
        await context.SaveChangesAsync();

        logger?.LogInformation("Successfully seeded {Count} AI Agent definitions (8 active, 12 inactive)", agents.Length);
    }
}

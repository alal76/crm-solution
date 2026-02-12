# Semantic Kernel Integration — Detailed Implementation Plan

> **Document:** SK-INTEGRATION-PLAN  
> **ADR Reference:** [ADR-004-Semantic-Kernel-Integration.md](ADR-004-Semantic-Kernel-Integration.md)  
> **Gap Analysis Reference:** [ROADMAP_BEST_IN_CLASS.md](../ROADMAP_BEST_IN_CLASS.md)  
> **Created:** February 2026  
> **Total Duration:** 16 Weeks (4 Phases)  
> **Estimated Effort:** ~160 engineering days

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Gap-to-Agent Mapping](#2-gap-to-agent-mapping)
3. [Phase 0 — Foundation (Weeks 1–3)](#3-phase-0--foundation-weeks-13)
4. [Phase 1 — P0 Agents (Weeks 4–8)](#4-phase-1--p0-agents-weeks-48)
5. [Phase 2 — P1 Agents (Weeks 9–13)](#5-phase-2--p1-agents-weeks-913)
6. [Phase 3 — P2 Agents & Multi-Agent (Weeks 14–16)](#6-phase-3--p2-agents--multi-agent-weeks-1416)
7. [Code Architecture & Folder Structure](#7-code-architecture--folder-structure)
8. [NuGet Package Manifest](#8-nuget-package-manifest)
9. [Entity Model & EF Core Schema](#9-entity-model--ef-core-schema)
10. [CRM Plugin Catalog](#10-crm-plugin-catalog)
11. [Agent Specifications](#11-agent-specifications)
12. [Infrastructure Setup](#12-infrastructure-setup)
13. [Testing Strategy](#13-testing-strategy)
14. [Feature Flags & Rollout](#14-feature-flags--rollout)
15. [.NET 10 Upgrade Sequencing](#15-net-10-upgrade-sequencing)
16. [Success Metrics & Acceptance Criteria](#16-success-metrics--acceptance-criteria)
17. [Risk Register](#17-risk-register)
18. [Appendix — Configuration Reference](#18-appendix--configuration-reference)

---

## 1. Executive Summary

This plan translates [ADR-004](ADR-004-Semantic-Kernel-Integration.md) into actionable implementation steps. It covers:

- **12 AI agents** across 3 priority tiers addressing 8 gap categories
- **10 CRM plugins** exposing domain operations to agents
- **SK-to-IAIPort bridge** preserving the hexagonal architecture from ADR-001
- **Vector memory** with Qdrant for semantic search, RAG, and agent learning
- **Human-in-the-loop** approval workflows for write operations

### Timeline Overview

```
Week:  1  2  3  4  5  6  7  8  9  10  11  12  13  14  15  16
       ├──────┤  ├──────────────┤  ├────────────────────┤  ├────────┤
       Phase 0   Phase 1 (P0)      Phase 2 (P1)           Phase 3
       Foundation 4 Agents          5 Agents               3 Agents
                                                           Multi-Agent
```

### Gap Impact Summary

| Gap Category | Current | After Phase 1 | After Phase 2 | After Phase 3 | Target |
|--------------|---------|---------------|---------------|---------------|--------|
| AI Intelligence (§1.1) | 55% | 70% | 80% | 82% | 95% |
| Agentic AI (§3.1) | 18% | 45% | 60% | 65% | 80% |
| Service Excellence (§1.3) | 72% | 78% | 82% | 82% | 95% |
| Document Intelligence (§3.4) | 38% | 38% | 50% | 55% | 85% |
| RevOps (§3.3) | 42% | 48% | 55% | 55% | 90% |

---

## 2. Gap-to-Agent Mapping

Each agent below is traced back to one or more gaps from [ROADMAP_BEST_IN_CLASS.md](../ROADMAP_BEST_IN_CLASS.md):

| Agent | Priority | Primary Gap | Gap Sections | Key Capabilities Addressed |
|-------|----------|-------------|--------------|---------------------------|
| **Lead Scoring Agent** | P0 | AI Intelligence | §1.1.1, §1.1.4 | Predictive scoring, enrichment, behavioral signals |
| **Support Triage Agent** | P0 | Service Excellence | §1.3.1, §1.3.2 | Auto-classify, priority, route, suggest KB articles |
| **Next Best Action Agent** | P0 | AI Intelligence | §1.1.3, §1.1.4 | Context-aware recommendations, action prioritization |
| **Sales Intelligence Agent** | P0 | RevOps | §3.3.1, §3.3.2 | Deal inspection, competitor analysis, coaching |
| **Email Assistant Agent** | P1 | AI Intelligence | §1.1.5, §1.1.7 | Draft generation, sentiment analysis, follow-up scheduling |
| **Customer Success Agent** | P1 | Agentic AI | §3.1.4 | Churn prediction, health scoring, proactive outreach |
| **Revenue Intelligence Agent** | P1 | RevOps | §3.3.3, §3.3.4 | Pipeline analytics, forecast accuracy, deal risk |
| **Ticket Resolution Agent** | P1 | Service Excellence | §1.3.3, §1.3.4 | Automated resolution, knowledge retrieval, SLA management |
| **Document Intelligence Agent** | P1 | Document Intelligence | §3.4.1, §3.4.2 | Contract analysis, clause extraction, proposal generation |
| **Sales Coach Agent** | P2 | AI Intelligence | §1.1.8 | Call analysis, objection handling, coaching insights |
| **Meeting Intelligence Agent** | P2 | AI Intelligence | §1.1.6 | Summarization, action extraction, CRM auto-update |
| **Conversation Intelligence Agent** | P2 | Agentic AI | §3.1.7 | Multi-channel analysis, relationship mapping |

---

## 3. Phase 0 — Foundation (Weeks 1–3)

**Goal:** Install SK packages, build the IAIPort bridge, create the plugin framework, set up vector memory, and establish the agent execution pipeline.

### Week 1: SK Core Integration

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 0.1 | Add NuGet packages to `CRM.Infrastructure.csproj` | `dotnet restore` succeeds; packages listed in §8 below |
| 0.2 | Create `CRM.Infrastructure/AI/SK/` folder structure | Folders: Connectors/, Plugins/, Agents/, Memory/, Filters/ |
| 0.3 | Create `CrmChatCompletionConnector.cs` | Implements `IChatCompletionService`; delegates to `IAIPort.ChatAsync()` |
| 0.4 | Create `CrmEmbeddingConnector.cs` | Implements `ITextEmbeddingGenerationService`; delegates to `IAIPort.GetEmbeddingsAsync()` |
| 0.5 | Create `CrmKernelFactory.cs` | Builds `Kernel` with connectors, registers plugins, returns configured kernel |
| 0.6 | Unit test: Connector delegates to IAIPort correctly | 10+ tests covering chat, streaming, embeddings, error handling |

**CrmChatCompletionConnector Implementation Detail:**

```csharp
// CRM.Infrastructure/AI/SK/Connectors/CrmChatCompletionConnector.cs
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class CrmChatCompletionConnector : IChatCompletionService
{
    private readonly IAIPort _aiPort;
    private readonly ILogger<CrmChatCompletionConnector> _logger;

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    // Translates SK ChatHistory → IAIPort AIChatRequest
    // Translates SK ToolCallBehavior → IAIPort AITool[]
    // Translates IAIPort AIChatResponse → SK ChatMessageContent
    
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var request = TranslateToAIRequest(chatHistory, executionSettings, kernel);
        var response = await _aiPort.ChatAsync(request, cancellationToken);
        return TranslateToSKResponse(response);
    }
    
    // Maps SK → IAIPort: system message, user messages, assistant messages, tool calls
    // Maps IAIPort → SK: content, tool call results, usage metadata
}
```

### Week 2: Plugin Framework & Memory

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 0.7 | Create `CrmPluginBase.cs` abstract base | `[RequiresApproval]` attribute, logging, ICrmDbContext injection |
| 0.8 | Create `AccountPlugin.cs` (first plugin) | Methods: GetAccount, SearchAccounts, GetAccountHealth, GetRelatedContacts |
| 0.9 | Create `OpportunityPlugin.cs` | Methods: GetOpportunity, GetPipeline, GetStageHistory, UpdateStage |
| 0.10 | Create `SearchPlugin.cs` (wraps ISearchPort) | Methods: SemanticSearch, EntitySearch, KBSearch |
| 0.11 | Set up Qdrant in Docker | Container in `docker-compose.yml`, port 6334, collection creation script |
| 0.12 | Create `CrmMemoryStore.cs` | Wraps QdrantMemoryStore; 6 collections (accounts, contacts, kb, emails, conversations, agents) |
| 0.13 | Create memory seeding job for existing entities | Background service to embed Account descriptions, KB articles, Contact notes |
| 0.14 | Unit test: Plugin methods invocable by SK kernel | 15+ tests covering each plugin method |

### Week 3: Agent Execution Pipeline & Human-in-the-Loop

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 0.15 | Create `AgentExecutionService.cs` | Orchestrates: receive request → build kernel → execute → return result |
| 0.16 | Create `HumanApprovalFilter.cs` | Implements `IAutoFunctionInvocationFilter`; intercepts `[RequiresApproval]` calls |
| 0.17 | Create `AgentConversationService.cs` | Manages conversation history, context window, token tracking |
| 0.18 | Create `AgentController.cs` API endpoints | POST /api/agents/{agentId}/chat, GET /api/agents, GET /api/agents/{id}/conversations |
| 0.19 | Create EF Core entities: AIAgent, AgentConversation, AgentAction, AgentMemory, AgentApprovalRequest | Entities added to `CRM.Core/Entities/AI/`; DbSets added to `CrmDbContext` & `ICrmDbContext`; `dotnet ef migrations add AddAIAgentEntities` succeeds; `dotnet ef database update` applies cleanly |
| 0.20 | Create `AgentApprovalHub` (SignalR) | Real-time approval notifications to frontend; extends CrmNotificationHub |
| 0.21 | Create feature flags for agent subsystem | `EnableAgentSubsystem`, `EnableAgent_{AgentName}` per agent |
| 0.22 | Integration test: Full round-trip chat with tool call and approval | End-to-end test using InMemory DB + mock IAIPort |

**Phase 0 Completion Gate:**
- [ ] `dotnet build` succeeds with SK packages
- [ ] CrmChatCompletionConnector passes 10+ unit tests
- [ ] At least 3 plugins registered and invocable via SK kernel
- [ ] Qdrant running in Docker with health check
- [ ] Agent API endpoint returns chat responses
- [ ] Human approval filter intercepts write operations
- [ ] EF Core migration generates and applies cleanly (`dotnet ef database update`)

---

## 4. Phase 1 — P0 Agents (Weeks 4–8)

**Goal:** Ship the 4 highest-impact agents that address AI Intelligence (§1.1) and Service Excellence (§1.3).

### Agent 1: Lead Scoring Agent (Weeks 4–5)

**Gap Addressed:** §1.1.1 Predictive Lead Scoring, §1.1.4 Churn/Win Prediction  
**IAIPort Method Enhanced:** Builds on `GetNextBestActionsAsync()` with richer context

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 1.1 | Create `LeadScoringAgent.cs` | Extends `CrmAgentBase`; system prompt, tool list, scoring logic |
| 1.2 | Create `LeadPlugin.cs` | GetLead, GetLeadHistory, GetSimilarLeads, GetLeadInteractions, UpdateLeadScore |
| 1.3 | Create lead embedding pipeline | Embed: company, industry, interactions, email content → vector store |
| 1.4 | Define scoring prompt template | Multi-criteria: demographics, firmographics, behavioral, engagement |
| 1.5 | Create `LeadScoringBackgroundService.cs` | Scheduled re-scoring (configurable interval, default 6h) |
| 1.6 | Wire to existing `LeadScoreRulesController` | Agent scoring as alternative to rule-based scoring |
| 1.7 | Create `LeadScoringAgentTests.cs` | 20+ tests: score accuracy, edge cases, missing data, error handling |
| 1.8 | Create evaluation dataset | 100 labeled leads with expected scores for accuracy benchmarking |

**System Prompt (Condensed):**
```
You are an AI lead scoring specialist for a CRM system. Your job is to evaluate 
leads using BANT criteria (Budget, Authority, Need, Timeline), firmographic data, 
behavioral signals, and engagement history.

Score each lead from 0-100 with breakdown:
- Demographic Fit (0-25): title, role, seniority
- Firmographic Fit (0-25): company size, industry, revenue
- Behavioral Signals (0-25): website visits, email opens, content downloads
- Engagement Recency (0-25): last interaction, response time, frequency

Always cite specific data points backing your score. Flag data gaps.
```

**Allowed Plugins:** LeadPlugin (read-only + UpdateLeadScore), AccountPlugin (read), SearchPlugin (read), ContactPlugin (read)

---

### Agent 2: Support Triage Agent (Weeks 5–6)

**Gap Addressed:** §1.3.1 Intelligent Ticket Routing, §1.3.2 Self-Service KB  
**IAIPort Method Enhanced:** Uses `SummarizeAsync()` + `ExtractEntitiesAsync()` + vector search

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 2.1 | Create `SupportTriageAgent.cs` | Auto-classify category/subcategory, assign priority, suggest assignee |
| 2.2 | Create `ServiceRequestPlugin.cs` | GetTicket, SearchTickets, GetCategories, AssignTicket, UpdatePriority, AddComment |
| 2.3 | Create `KnowledgeBasePlugin.cs` | SearchArticles, GetArticle, GetPopularArticles, SuggestArticles |
| 2.4 | Embed entire KB into vector store | All KnowledgeArticles → `crm-kb-articles` collection on startup |
| 2.5 | Implement RAG pipeline for KB retrieval | Query → vector search → top-5 articles → LLM summarization → response |
| 2.6 | Create auto-triage webhook | New ServiceRequest → trigger agent → classify + route + suggest KB |
| 2.7 | Wire to `ServiceRequestsController` | Agent-assisted create endpoint; auto-classification on POST |
| 2.8 | Create `SupportTriageAgentTests.cs` | 20+ tests: classification accuracy, KB retrieval, routing logic |

**Workflow:**
```
1. New ticket arrives (API/email/portal)
2. Agent extracts: subject, description, customer context
3. Vector search KB for relevant articles (top 5)
4. Classify: category, subcategory, type, priority
5. Determine assignee based on: category rules, agent availability, SLA
6. If KB match ≥ 85% confidence → suggest self-service resolution
7. Create/update ticket with classification + suggested articles
8. If write → human approval (for auto-close/reassign)
```

---

### Agent 3: Next Best Action Agent (Weeks 6–7)

**Gap Addressed:** §1.1.3 Next Best Action, §1.1.4 Predictive Analytics  
**IAIPort Method Enhanced:** Replaces heuristic `GetNextBestActionsAsync()` with agentic reasoning

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 3.1 | Create `NextBestActionAgent.cs` | Context-aware recommendations for any entity type |
| 3.2 | Create `ActivityPlugin.cs` | GetRecentActivities, GetUpcoming, LogActivity, GetEntityTimeline |
| 3.3 | Create `CalendarPlugin.cs` | GetAvailableSlots, CheckConflicts, GetUpcomingMeetings |
| 3.4 | Define action taxonomy | 20+ action types: call, email, meeting, quote, follow-up, escalate, etc. |
| 3.5 | Implement context aggregation | Merge: entity data, recent activities, open opps, overdue tasks, SLA status |
| 3.6 | Wire to existing `GetNextBestActionsAsync` | Agent result can replace/augment the IAIPort direct call |
| 3.7 | Create dashboard widget API | GET /api/agents/next-best-actions/{entityType}/{entityId} |
| 3.8 | Create `NextBestActionAgentTests.cs` | 15+ tests: action relevance, priority ordering, context awareness |

**Action Taxonomy:**
```
OUTREACH:    schedule_call, send_email, schedule_meeting, send_quote
FOLLOW_UP:   follow_up_proposal, check_in_post_sale, renewal_reminder
ESCALATION:  escalate_to_manager, request_executive_sponsor
INTERNAL:    update_crm_record, create_task, reassign_opportunity
CONTENT:     share_case_study, send_product_info, share_pricing
ENGAGEMENT:  social_media_engage, event_invitation, referral_request
```

---

### Agent 4: Sales Intelligence Agent (Weeks 7–8)

**Gap Addressed:** §3.3.1 Deal Inspection, §3.3.2 Revenue Intelligence  
**IAIPort Method Enhanced:** Extends `AnalyzeSentimentAsync()` + `SummarizeAsync()`

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 4.1 | Create `SalesIntelligenceAgent.cs` | Deal analysis, risk assessment, competitive intelligence |
| 4.2 | Create `QuotePlugin.cs` | GetQuote, GetQuoteHistory, CompareQuotes, GetWinRates |
| 4.3 | Create `NotificationPlugin.cs` | SendAlert, NotifyTeam, CreateReminder |
| 4.4 | Implement deal health scoring | Multi-factor: engagement velocity, stakeholder coverage, competitive signals |
| 4.5 | Implement competitor analysis prompt | Extract competitor mentions from notes/emails, map strengths/weaknesses |
| 4.6 | Wire to opportunity detail page API | GET /api/agents/deal-intelligence/{opportunityId} |
| 4.7 | Create risk alert background service | Periodic scan for at-risk deals → notify owners |
| 4.8 | Create `SalesIntelligenceAgentTests.cs` | 15+ tests: deal risk accuracy, competitor extraction |

**Phase 1 Completion Gate:**
- [ ] All 4 agents pass unit tests (70+ total)
- [ ] Lead scoring accuracy ≥ 75% on evaluation dataset
- [ ] Support triage classification accuracy ≥ 80%
- [ ] Next best actions rated as relevant ≥ 70% by sample review
- [ ] Deal intelligence risk alerts generate for stale deals
- [ ] Human approval flow works end-to-end for write operations
- [ ] All agent API endpoints accessible and documented

---

## 5. Phase 2 — P1 Agents (Weeks 9–13)

### Agent 5: Email Assistant Agent (Weeks 9–10)

**Gap Addressed:** §1.1.5 AI Email Assistant, §1.1.7 Conversation Intelligence

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 5.1 | Create `EmailAssistantAgent.cs` | Draft generation, reply suggestion, tone adjustment, follow-up scheduling |
| 5.2 | Create `EmailPlugin.cs` | GetEmailHistory, DraftEmail, SendEmail, GetTemplates, ScheduleFollowUp |
| 5.3 | Integrate with existing `GenerateEmailDraftAsync()` on IAIPort | Agent enhances with context from CRM data |
| 5.4 | Implement multi-turn email thread analysis | Summarize thread, extract action items, identify sentiment shifts |
| 5.5 | Create tone/persona presets | Professional, Friendly, Urgent, Executive, Technical |
| 5.6 | Wire to email compose UI API | POST /api/agents/email/draft, POST /api/agents/email/reply |
| 5.7 | Create `EmailAssistantAgentTests.cs` | 15+ tests |

### Agent 6: Customer Success Agent (Weeks 10–11)

**Gap Addressed:** §3.1.4 Customer Success Agent, §1.1.4 Churn Prediction

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 6.1 | Create `CustomerSuccessAgent.cs` | Health monitoring, churn risk, expansion signals, QBR prep |
| 6.2 | Create health score computation | Weighted: support tickets, engagement, NPS, usage, payment timeliness |
| 6.3 | Implement churn early-warning | Score threshold triggers → proactive outreach recommendation |
| 6.4 | Create QBR preparation prompt | Auto-generate QBR deck data: metrics, achievements, recommendations |
| 6.5 | Wire to account detail page | GET /api/agents/customer-success/{accountId} |
| 6.6 | Create account health monitoring background job | Periodic scan → flag at-risk accounts → create tasks |
| 6.7 | Create `CustomerSuccessAgentTests.cs` | 15+ tests |

### Agent 7: Revenue Intelligence Agent (Week 11)

**Gap Addressed:** §3.3.3 Pipeline Analytics, §3.3.4 Forecast Accuracy

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 7.1 | Create `RevenueIntelligenceAgent.cs` | Pipeline analysis, forecast adjustment, revenue insights |
| 7.2 | Implement pipeline analysis prompt | Stage velocity, conversion rates, deal aging, risk distribution |
| 7.3 | Implement AI-assisted forecast | Compare CRM forecast vs AI-predicted, highlight discrepancies |
| 7.4 | Wire to dashboard | GET /api/agents/revenue-intelligence/pipeline-summary |
| 7.5 | Create `RevenueIntelligenceAgentTests.cs` | 10+ tests |

### Agent 8: Ticket Resolution Agent (Week 12)

**Gap Addressed:** §1.3.3 Automated Resolution, §1.3.4 SLA-Aware Escalation

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 8.1 | Create `TicketResolutionAgent.cs` | Multi-turn troubleshooting, KB-backed resolution, SLA awareness |
| 8.2 | Implement resolution workflow | Understand → search KB → propose fix → verify → close/escalate |
| 8.3 | Implement SLA countdown awareness | Agent adjusts urgency based on SLA time remaining |
| 8.4 | Create auto-resolution capability | For common issues with KB match ≥ 90% confidence → auto-resolve (with approval) |
| 8.5 | Wire to service request detail page | POST /api/agents/resolve/{serviceRequestId} |
| 8.6 | Create `TicketResolutionAgentTests.cs` | 15+ tests |

### Agent 9: Document Intelligence Agent (Weeks 12–13)

**Gap Addressed:** §3.4.1 Contract Analysis, §3.4.2 Proposal Generation

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 9.1 | Create `DocumentIntelligenceAgent.cs` | Contract analysis, clause extraction, risk flagging |
| 9.2 | Create `ContractPlugin.cs` | GetContract, GetClauses, GetRenewals, CompareVersions |
| 9.3 | Implement clause extraction prompt | Auto-identify: payment terms, SLA, liability, termination, renewal |
| 9.4 | Implement risk flagging | Non-standard clauses, missing clauses, unfavorable terms |
| 9.5 | Wire to contract detail page | POST /api/agents/document/analyze/{contractId} |
| 9.6 | Create `DocumentIntelligenceAgentTests.cs` | 10+ tests |

**Phase 2 Completion Gate:**
- [ ] All 5 agents pass unit tests (65+ total)
- [ ] Email draft quality rated ≥ 4/5 by sample review
- [ ] Customer success health scores correlate with churn (retrospective validation)
- [ ] Revenue intelligence forecast within 15% of actual
- [ ] Ticket resolution auto-resolve rate ≥ 20% for eligible tickets
- [ ] Document analysis extracts key clauses from sample contracts

---

## 6. Phase 3 — P2 Agents & Multi-Agent (Weeks 14–16)

### Agent 10: Sales Coach Agent (Week 14)

**Gap Addressed:** §1.1.8 Sales Coach

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 10.1 | Create `SalesCoachAgent.cs` | Deal-specific coaching, objection handling, negotiation tips |
| 10.2 | Implement coaching prompt library | 15+ coaching scenarios: pricing objection, competitor displacement, etc. |
| 10.3 | Wire to opportunity context panel | GET /api/agents/coach/{opportunityId} |
| 10.4 | Create `SalesCoachAgentTests.cs` | 10+ tests |

### Agent 11: Meeting Intelligence Agent (Week 14)

**Gap Addressed:** §1.1.6 Meeting Intelligence

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 11.1 | Create `MeetingIntelligenceAgent.cs` | Meeting prep, post-meeting summary, action item extraction |
| 11.2 | Implement meeting prep prompt | Agenda, attendee research, talking points, open items |
| 11.3 | Implement post-meeting summary | Input: transcript/notes → output: summary, actions, CRM updates |
| 11.4 | Wire to activity detail page | POST /api/agents/meeting/prep/{activityId}, POST /api/agents/meeting/summarize |
| 11.5 | Create `MeetingIntelligenceAgentTests.cs` | 10+ tests |

### Agent 12: Conversation Intelligence Agent (Week 15)

**Gap Addressed:** §3.1.7 Agent Analytics, §1.1.7 Conversation Intelligence

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 12.1 | Create `ConversationIntelligenceAgent.cs` | Cross-channel analysis, relationship strength, sentiment trends |
| 12.2 | Implement relationship mapping | Analyze interactions to map stakeholder relationships and influence |
| 12.3 | Implement sentiment trend analysis | Track sentiment over time across emails, calls, tickets |
| 12.4 | Wire to account/contact 360 view | GET /api/agents/conversation-intelligence/{entityType}/{entityId} |
| 12.5 | Create `ConversationIntelligenceAgentTests.cs` | 10+ tests |

### Multi-Agent Orchestration (Weeks 15–16)

**Gap Addressed:** §3.1.5 Multi-Agent Orchestration

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 13.1 | Create `AgentOrchestrator.cs` | Routes requests to appropriate agent(s), combines results |
| 13.2 | Implement agent selection strategy | Keyword + intent classification → agent routing |
| 13.3 | Implement sequential agent chaining | Agent A output → enriched context → Agent B input |
| 13.4 | Implement parallel agent execution | Fan-out to multiple agents → merge results |
| 13.5 | Create orchestrator API | POST /api/agents/orchestrate (natural language → agent selection → result) |
| 13.6 | Create `AgentOrchestratorTests.cs` | 15+ tests |

### Agent Analytics Dashboard (Week 16)

**Gap Addressed:** §3.1.7 Agent Analytics

| # | Task | Acceptance Criteria |
|---|------|---------------------|
| 14.1 | Create `AgentAnalyticsService.cs` | Usage stats, accuracy metrics, latency, cost tracking |
| 14.2 | Create analytics API endpoints | GET /api/agents/analytics/usage, /accuracy, /cost, /performance |
| 14.3 | Create agent evaluation framework | Automated scoring of agent outputs against labeled data |
| 14.4 | Wire to admin dashboard | Agent performance cards with key metrics |

**Phase 3 Completion Gate:**
- [ ] All 3 P2 agents pass unit tests (30+ total)
- [ ] Multi-agent orchestrator correctly routes 90%+ of test queries
- [ ] Agent analytics dashboard shows usage, accuracy, cost data
- [ ] Total test count across all phases: 200+ agent-related tests
- [ ] All feature flags operational for per-agent enable/disable

---

## 7. Code Architecture & Folder Structure

```
CRM.Backend/src/CRM.Infrastructure/
├── AI/
│   └── SK/                                    # NEW — Semantic Kernel integration
│       ├── Connectors/
│       │   ├── CrmChatCompletionConnector.cs  # IChatCompletionService → IAIPort bridge
│       │   └── CrmEmbeddingConnector.cs       # ITextEmbeddingGenerationService → IAIPort bridge
│       ├── Plugins/
│       │   ├── CrmPluginBase.cs               # Abstract base with logging, DB access
│       │   ├── AccountPlugin.cs               # Account CRUD + health
│       │   ├── ContactPlugin.cs               # Contact CRUD + lookup
│       │   ├── OpportunityPlugin.cs           # Opportunity CRUD + pipeline
│       │   ├── LeadPlugin.cs                  # Lead CRUD + scoring
│       │   ├── ServiceRequestPlugin.cs        # Ticket CRUD + routing
│       │   ├── EmailPlugin.cs                 # Email CRUD + templates
│       │   ├── KnowledgeBasePlugin.cs         # KB search + article CRUD
│       │   ├── SearchPlugin.cs                # ISearchPort semantic search
│       │   ├── CalendarPlugin.cs              # Calendar + availability
│       │   ├── NotificationPlugin.cs          # INotificationPort wrapper
│       │   ├── QuotePlugin.cs                 # Quote CRUD + line items
│       │   └── ContractPlugin.cs              # Contract CRUD + clauses
│       ├── Agents/
│       │   ├── CrmAgentBase.cs                # Abstract agent with prompt, plugins, memory
│       │   ├── LeadScoringAgent.cs            # P0
│       │   ├── SupportTriageAgent.cs          # P0
│       │   ├── NextBestActionAgent.cs         # P0
│       │   ├── SalesIntelligenceAgent.cs      # P0
│       │   ├── EmailAssistantAgent.cs         # P1
│       │   ├── CustomerSuccessAgent.cs        # P1
│       │   ├── RevenueIntelligenceAgent.cs    # P1
│       │   ├── TicketResolutionAgent.cs       # P1
│       │   ├── DocumentIntelligenceAgent.cs   # P1
│       │   ├── SalesCoachAgent.cs             # P2
│       │   ├── MeetingIntelligenceAgent.cs    # P2
│       │   └── ConversationIntelligenceAgent.cs # P2
│       ├── Memory/
│       │   ├── CrmMemoryStore.cs              # Qdrant wrapper with collection management
│       │   ├── MemorySeeder.cs                # Initial entity embedding job
│       │   └── MemoryCollections.cs           # Collection name constants
│       ├── Filters/
│       │   ├── HumanApprovalFilter.cs         # IAutoFunctionInvocationFilter
│       │   ├── AuditLoggingFilter.cs          # IFunctionInvocationFilter for audit trail
│       │   └── CostTrackingFilter.cs          # Token usage and cost tracking
│       ├── Orchestration/
│       │   ├── AgentOrchestrator.cs           # Multi-agent routing and chaining
│       │   └── AgentSelectionStrategy.cs      # Intent → agent mapping
│       ├── Services/
│       │   ├── AgentExecutionService.cs        # Agent lifecycle management
│       │   ├── AgentConversationService.cs     # Conversation history + context
│       │   ├── AgentAnalyticsService.cs        # Usage, accuracy, cost tracking
│       │   └── CrmKernelFactory.cs             # Kernel builder with DI
│       └── Prompts/
│           ├── lead-scoring.yaml              # SK prompt template format
│           ├── support-triage.yaml
│           ├── next-best-action.yaml
│           ├── sales-intelligence.yaml
│           ├── email-assistant.yaml
│           ├── customer-success.yaml
│           └── ... (one per agent)

CRM.Backend/src/CRM.Core/
├── Entities/
│   ├── AIAgent.cs                             # NEW — Agent definition entity
│   ├── AgentConversation.cs                   # NEW — Conversation history
│   ├── AgentAction.cs                         # NEW — Action audit log
│   ├── AgentMemory.cs                         # NEW — Long-term memory entries
│   └── AgentApprovalRequest.cs                # NEW — Human-in-the-loop approvals

CRM.Backend/src/CRM.Api/
├── Controllers/
│   ├── AgentController.cs                     # NEW — Agent chat/management API
│   ├── AgentAdminController.cs                # NEW — Agent configuration admin
│   └── AgentAnalyticsController.cs            # NEW — Agent metrics/analytics
├── Hubs/
│   └── AgentApprovalHub.cs                    # NEW — SignalR for approvals
```

---

## 8. NuGet Package Manifest

### Required Packages (Phase 0)

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.SemanticKernel` | 1.x (latest stable) | Core SK framework |
| `Microsoft.SemanticKernel.Abstractions` | 1.x | Interfaces for plugins, filters |
| `Microsoft.SemanticKernel.Connectors.Qdrant` | 1.x | Qdrant vector store connector |
| `Microsoft.SemanticKernel.Plugins.Core` | 1.x-alpha | Built-in plugins (time, math, text) |

### Optional Packages (Add as Needed)

| Package | Version | Purpose | Phase |
|---------|---------|---------|-------|
| `Microsoft.SemanticKernel.Connectors.OpenAI` | 1.x | Direct OpenAI connector (bypass IAIPort if needed) | — |
| `Microsoft.SemanticKernel.Connectors.AzureOpenAI` | 1.x | Direct Azure OpenAI connector | — |
| `Microsoft.SemanticKernel.Planners.Handlebars` | 1.x-preview | Complex multi-step planning | Phase 3 |
| `Microsoft.SemanticKernel.Agents.Core` | 1.x-alpha | Multi-agent chat framework | Phase 3 |
| `Microsoft.SemanticKernel.Agents.OpenAI` | 1.x-alpha | OpenAI Assistants connector | Phase 3 |

### Installation Command

```bash
cd CRM.Backend/src/CRM.Infrastructure
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Abstractions
dotnet add package Microsoft.SemanticKernel.Connectors.Qdrant
dotnet add package Microsoft.SemanticKernel.Plugins.Core --prerelease
```

---

## 9. Entity Model & EF Core Schema

> **Note:** This project uses EF Core code-first migrations per ADR-002 (Unified EF Core Schema Management).
> Schema changes are generated via `dotnet ef migrations add`, applied at startup by `db.Database.MigrateAsync()`,
> and seeded via `DbSeed.SeedAsync()`. **No raw SQL migration files are used.**

### 9.1 New Entities

**AIAgent Entity:**

```csharp
public class AIAgent : BaseEntity
{
    public string Name { get; set; } = string.Empty;           // "lead-scoring"
    public string DisplayName { get; set; } = string.Empty;    // "Lead Scoring Agent"
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;   // Full system prompt text
    public string AgentType { get; set; } = string.Empty;      // "LeadScoring", "SupportTriage", etc.
    public string AllowedPlugins { get; set; } = string.Empty;  // JSON: ["LeadPlugin","AccountPlugin"]
    public string Configuration { get; set; } = string.Empty;   // JSON: agent-specific config
    public bool IsActive { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;          // Default: require human approval
    public string ApprovalTier { get; set; } = "standard";     // "auto", "standard", "elevated", "admin"
    public double Temperature { get; set; } = 0.3;
    public int MaxTokens { get; set; } = 4096;
    public string ModelOverride { get; set; } = string.Empty;   // Override default model
    public int TotalConversations { get; set; }
    public int TotalActions { get; set; }
    public double AverageRating { get; set; }
}
```

**AgentConversation Entity:**

```csharp
public class AgentConversation : BaseEntity
{
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string? EntityType { get; set; }                    // "Account", "Lead", etc.
    public int? EntityId { get; set; }                         // Related entity ID
    public string Status { get; set; } = "Active";            // Active, Completed, Abandoned
    public string Messages { get; set; } = "[]";               // JSON: serialized message history
    public int MessageCount { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
    public double? UserRating { get; set; }                    // 1-5 star rating
    public string? UserFeedback { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

**AgentAction Entity:**

```csharp
public class AgentAction : BaseEntity
{
    public int ConversationId { get; set; }
    public AgentConversation Conversation { get; set; } = null!;
    public int AgentId { get; set; }
    public string ActionType { get; set; } = string.Empty;     // "ToolCall", "Response", "Approval"
    public string PluginName { get; set; } = string.Empty;     // "AccountPlugin"
    public string FunctionName { get; set; } = string.Empty;   // "GetAccountHealth"
    public string InputParameters { get; set; } = "{}";        // JSON: function arguments
    public string OutputResult { get; set; } = "{}";           // JSON: function result
    public string Status { get; set; } = "Completed";          // Pending, Approved, Rejected, Completed, Failed
    public int? ApprovalRequestId { get; set; }
    public AgentApprovalRequest? ApprovalRequest { get; set; }
    public int TokensUsed { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}
```

**AgentMemory Entity:**

```csharp
public class AgentMemory : BaseEntity
{
    public int AgentId { get; set; }
    public AIAgent Agent { get; set; } = null!;
    public string MemoryType { get; set; } = string.Empty;     // "Fact", "Preference", "Insight", "Pattern"
    public string Key { get; set; } = string.Empty;            // Unique key within agent+type
    public string Value { get; set; } = string.Empty;          // The memory content
    public string? EntityType { get; set; }                    // Optional entity association
    public int? EntityId { get; set; }
    public double Confidence { get; set; } = 1.0;             // 0.0 - 1.0
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }                   // Null = never expires
}
```

**AgentApprovalRequest Entity:**

```csharp
public class AgentApprovalRequest : BaseEntity
{
    public int AgentActionId { get; set; }
    public AgentAction AgentAction { get; set; } = null!;
    public int ConversationId { get; set; }
    public int AgentId { get; set; }
    public int RequestedByUserId { get; set; }                 // User who triggered the agent
    public int? ApprovedByUserId { get; set; }                 // User who approved/rejected
    public string ActionDescription { get; set; } = string.Empty;  // Human-readable description
    public string PluginName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string Parameters { get; set; } = "{}";             // JSON: proposed changes
    public string Status { get; set; } = "Pending";            // Pending, Approved, Rejected, Expired, AutoApproved
    public string ApprovalTier { get; set; } = "standard";
    public string? RejectionReason { get; set; }
    public DateTime? DecidedAt { get; set; }
    public DateTime ExpiresAt { get; set; }                    // Auto-expire if not decided
}
```

### 9.2 CrmDbContext Registration

Add DbSets to the existing **AI Entities** section in `CrmDbContext.cs` (adjacent to `DbSet<AIModel>`, `DbSet<Prediction>`, etc.):

```csharp
// CRM.Infrastructure/Data/CrmDbContext.cs — AI Entities section

// Existing AI entities
public DbSet<AIModel> AIModels { get; set; }
public DbSet<Prediction> Predictions { get; set; }
public DbSet<LeadScore> LeadScores { get; set; }
public DbSet<OpportunityInsight> OpportunityInsights { get; set; }
public DbSet<ChurnRisk> ChurnRisks { get; set; }
public DbSet<ActionRecommendation> ActionRecommendations { get; set; }
public DbSet<EmailIntelligence> EmailIntelligences { get; set; }

// New: AI Agent Subsystem (ADR-004)
public DbSet<AIAgent> AIAgents { get; set; }
public DbSet<AgentConversation> AgentConversations { get; set; }
public DbSet<AgentAction> AgentActions { get; set; }
public DbSet<AgentMemory> AgentMemories { get; set; }
public DbSet<AgentApprovalRequest> AgentApprovalRequests { get; set; }
```

Mirror the same DbSet declarations in `ICrmDbContext.cs`:

```csharp
// CRM.Core/Interfaces/ICrmDbContext.cs — add get-only properties
DbSet<AIAgent> AIAgents { get; }
DbSet<AgentConversation> AgentConversations { get; }
DbSet<AgentAction> AgentActions { get; }
DbSet<AgentMemory> AgentMemories { get; }
DbSet<AgentApprovalRequest> AgentApprovalRequests { get; }
```

### 9.3 Entity Type Configurations (Fluent API)

Create `IEntityTypeConfiguration<T>` classes following the existing pattern in `CrmDbContext.OnModelCreating()`:

```csharp
// CRM.Infrastructure/Data/Configurations/AIAgentConfiguration.cs
public class AIAgentConfiguration : IEntityTypeConfiguration<AIAgent>
{
    public void Configure(EntityTypeBuilder<AIAgent> builder)
    {
        builder.ToTable("AIAgents");
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.AgentType);
        builder.HasIndex(e => e.IsActive);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.AgentType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ApprovalTier).HasMaxLength(20).HasDefaultValue("standard");
        builder.Property(e => e.ModelOverride).HasMaxLength(100);
        builder.Property(e => e.Temperature).HasDefaultValue(0.3);
        builder.Property(e => e.MaxTokens).HasDefaultValue(4096);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

// Similar configurations for AgentConversation, AgentAction, AgentMemory, AgentApprovalRequest
// Key indexes:
//   AgentConversation: (AgentId), (UserId), (EntityType, EntityId), (Status)
//   AgentAction: (ConversationId), (AgentId), (PluginName, FunctionName)
//   AgentMemory: Unique (AgentId, MemoryType, Key), (EntityType, EntityId), (ExpiresAt)
//   AgentApprovalRequest: (Status), (AgentId), (RequestedByUserId), (ExpiresAt)
```

### 9.4 Migration Generation

Generate and apply the EF Core migration:

```bash
# Generate migration
cd CRM.Backend
dotnet ef migrations add AddAIAgentEntities \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api

# Verify migration SQL (optional review)
dotnet ef migrations script \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api

# Apply (manual — startup also runs MigrateAsync automatically)
dotnet ef database update \
    --project src/CRM.Infrastructure \
    --startup-project src/CRM.Api
```

At runtime, `Program.cs` calls `db.Database.MigrateAsync()` which applies all pending migrations automatically.

### 9.5 Seed Data

Seed the default AI Agent definitions via the existing `DbSeed.SeedAsync()` pattern:

```csharp
// In CRM.Infrastructure/Data/DbSeed.cs — add to SeedAsync method
private static async Task SeedAIAgentsAsync(CrmDbContext db)
{
    if (await db.AIAgents.AnyAsync()) return;  // Skip if already seeded

    var agents = new[]
    {
        new AIAgent { Name = "lead-scoring", DisplayName = "Lead Scoring Agent",
            Description = "AI-powered lead scoring using BANT criteria, firmographics, and behavioral signals",
            AgentType = "LeadScoring", IsActive = true, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.2, MaxTokens = 2048 },
        new AIAgent { Name = "support-triage", DisplayName = "Support Triage Agent",
            Description = "Auto-classify, prioritize, and route support tickets with KB suggestions",
            AgentType = "SupportTriage", IsActive = true, RequiresApproval = true,
            ApprovalTier = "standard", Temperature = 0.3, MaxTokens = 4096 },
        new AIAgent { Name = "next-best-action", DisplayName = "Next Best Action Agent",
            Description = "Context-aware action recommendations for any CRM entity",
            AgentType = "NextBestAction", IsActive = true, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.4, MaxTokens = 2048 },
        new AIAgent { Name = "sales-intelligence", DisplayName = "Sales Intelligence Agent",
            Description = "Deal analysis, risk assessment, and competitive intelligence",
            AgentType = "SalesIntelligence", IsActive = true, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.3, MaxTokens = 4096 },
        // P1/P2 agents seeded as inactive
        new AIAgent { Name = "email-assistant", DisplayName = "Email Assistant Agent",
            AgentType = "EmailAssistant", IsActive = false, RequiresApproval = true,
            ApprovalTier = "standard", Temperature = 0.7, MaxTokens = 4096 },
        new AIAgent { Name = "customer-success", DisplayName = "Customer Success Agent",
            AgentType = "CustomerSuccess", IsActive = false, RequiresApproval = true,
            ApprovalTier = "standard", Temperature = 0.3, MaxTokens = 4096 },
        new AIAgent { Name = "revenue-intelligence", DisplayName = "Revenue Intelligence Agent",
            AgentType = "RevenueIntelligence", IsActive = false, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.2, MaxTokens = 4096 },
        new AIAgent { Name = "ticket-resolution", DisplayName = "Ticket Resolution Agent",
            AgentType = "TicketResolution", IsActive = false, RequiresApproval = true,
            ApprovalTier = "elevated", Temperature = 0.3, MaxTokens = 8192 },
        new AIAgent { Name = "document-intelligence", DisplayName = "Document Intelligence Agent",
            AgentType = "DocumentIntelligence", IsActive = false, RequiresApproval = true,
            ApprovalTier = "elevated", Temperature = 0.2, MaxTokens = 8192 },
        new AIAgent { Name = "sales-coach", DisplayName = "Sales Coach Agent",
            AgentType = "SalesCoach", IsActive = false, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.5, MaxTokens = 4096 },
        new AIAgent { Name = "meeting-intelligence", DisplayName = "Meeting Intelligence Agent",
            AgentType = "MeetingIntelligence", IsActive = false, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.4, MaxTokens = 4096 },
        new AIAgent { Name = "conversation-intelligence", DisplayName = "Conversation Intelligence Agent",
            AgentType = "ConversationIntelligence", IsActive = false, RequiresApproval = false,
            ApprovalTier = "auto", Temperature = 0.3, MaxTokens = 4096 },
    };

    db.AIAgents.AddRange(agents);
    await db.SaveChangesAsync();
}
```

---

## 10. CRM Plugin Catalog

Each plugin exposes domain operations to SK agents. Plugins follow a **read/write split** with `[RequiresApproval]` on all write operations.

| Plugin | Read Methods | Write Methods | Injected Services |
|--------|-------------|---------------|-------------------|
| **AccountPlugin** | GetAccount, SearchAccounts, GetAccountHealth, GetRelatedContacts, GetAccountHistory | UpdateAccount, AddNote | IAccountService, ICrmDbContext |
| **ContactPlugin** | GetContact, SearchContacts, GetContactAccounts, GetContactInteractions | UpdateContact, AddNote | IContactsService, ICrmDbContext |
| **OpportunityPlugin** | GetOpportunity, GetPipeline, GetStageHistory, GetCompetitors, GetWinRates | UpdateStage, AddNote | IOpportunityService, ICrmDbContext |
| **LeadPlugin** | GetLead, SearchLeads, GetLeadHistory, GetSimilarLeads, GetLeadScore | UpdateLeadScore, ConvertLead | ILeadService, ICrmDbContext |
| **ServiceRequestPlugin** | GetTicket, SearchTickets, GetCategories, GetSLAStatus, GetRelatedArticles | AssignTicket, UpdatePriority, AddComment, CloseTicket | IServiceRequestService |
| **EmailPlugin** | GetEmailHistory, GetThread, GetTemplates, GetDrafts | DraftEmail, SendEmail, ScheduleFollowUp | IEmailTemplateService, INotificationPort |
| **KnowledgeBasePlugin** | SearchArticles, GetArticle, GetPopularArticles, GetCategories | CreateArticle, UpdateArticle | ICrmDbContext, ISearchPort |
| **SearchPlugin** | SemanticSearch, EntitySearch, FuzzySearch | — (read-only) | ISearchPort, CrmMemoryStore |
| **CalendarPlugin** | GetUpcomingMeetings, GetAvailableSlots, CheckConflicts | CreateMeeting, UpdateMeeting | IActivityService |
| **NotificationPlugin** | — | SendNotification, SendAlert, CreateTask | INotificationPort, ICrmDbContext |
| **QuotePlugin** | GetQuote, GetQuoteHistory, CompareQuotes, GetLineItems | CreateQuote, UpdateQuote | IQuoteService |
| **ContractPlugin** | GetContract, GetClauses, GetRenewals, CompareVersions | — (read-only for now) | IContractService |

### Plugin Method Annotation Pattern

```csharp
[KernelFunction("GetAccountHealth")]
[Description("Returns the health score, recent activity summary, and risk indicators for a customer account")]
public async Task<AccountHealthSummary> GetAccountHealthAsync(
    [Description("The account ID to analyze")] int accountId,
    CancellationToken cancellationToken = default)
{
    // Implementation using injected IAccountService
}

[KernelFunction("UpdateLeadScore")]
[Description("Updates the AI-computed score for a lead")]
[RequiresApproval(Tier = "auto")]  // Auto-approved for score updates
public async Task<bool> UpdateLeadScoreAsync(
    [Description("The lead ID")] int leadId,
    [Description("New score from 0-100")] int score,
    [Description("Scoring rationale")] string rationale,
    CancellationToken cancellationToken = default)
{
    // Implementation using injected ILeadService
}
```

---

## 11. Agent Specifications

### 11.1 Agent Base Class

```csharp
public abstract class CrmAgentBase
{
    public abstract string AgentName { get; }
    public abstract string AgentType { get; }
    public abstract string SystemPrompt { get; }
    public abstract string[] AllowedPlugins { get; }
    public virtual double Temperature => 0.3;
    public virtual int MaxTokens => 4096;
    
    // Override for agent-specific context enrichment
    public virtual async Task<string> EnrichContextAsync(
        string entityType, int entityId, ICrmDbContext context) => string.Empty;
    
    // Override for agent-specific post-processing
    public virtual async Task<AgentResponse> PostProcessAsync(
        AgentResponse rawResponse, CancellationToken cancellationToken) => rawResponse;
}
```

### 11.2 Lead Scoring Agent — Detailed Spec

| Property | Value |
|----------|-------|
| **Name** | `lead-scoring` |
| **Type** | LeadScoring |
| **Temperature** | 0.2 (low creativity — consistent scoring) |
| **Max Tokens** | 2048 |
| **Allowed Plugins** | LeadPlugin, AccountPlugin, ContactPlugin, SearchPlugin |
| **Approval Tier** | Auto (score update is low-risk) |
| **Trigger** | On lead create, on lead update, scheduled batch (6h) |

**Scoring Rubric:**
```
DEMOGRAPHIC FIT (0–25):
  - C-suite/VP title: +20, Director: +15, Manager: +10, Other: +5
  - Decision maker confirmed: +5
  
FIRMOGRAPHIC FIT (0–25):
  - Enterprise (1000+): +20, Mid-market (100-999): +15, SMB: +10
  - Target industry match: +5
  
BEHAVIORAL SIGNALS (0–25):
  - Demo requested: +20, Pricing page: +15, Case study: +10
  - 3+ page views in 7 days: +10, Email opens: +5
  
ENGAGEMENT RECENCY (0–25):
  - Interaction < 24h: +25, < 7 days: +20, < 30 days: +10, > 30 days: +0
```

**Context Enrichment:** Pulls last 10 interactions, account company data, similar won deals

### 11.3 Support Triage Agent — Detailed Spec

| Property | Value |
|----------|-------|
| **Name** | `support-triage` |
| **Type** | SupportTriage |
| **Temperature** | 0.3 |
| **Max Tokens** | 4096 |
| **Allowed Plugins** | ServiceRequestPlugin, KnowledgeBasePlugin, AccountPlugin, SearchPlugin |
| **Approval Tier** | Standard (ticket reassignment requires approval) |
| **Trigger** | On service request create, manual invoke |

**Classification Taxonomy:**
```
CATEGORIES:           SOFTWARE, HARDWARE, NETWORK, ACCOUNT, BILLING, OTHER
PRIORITIES:           P1-Critical, P2-High, P3-Medium, P4-Low
ROUTING RULES:
  - P1 + Software   → Senior Dev Team
  - P1 + Network    → NOC Team
  - P2 + Account    → Account Management
  - P3/P4 + any     → General Support Pool
```

**RAG Pipeline:**
1. Extract keywords from ticket subject + description
2. Vector search `crm-kb-articles` collection (top 5, similarity ≥ 0.75)
3. Include retrieved articles in context
4. LLM classifies + generates response with article references

---

## 12. Infrastructure Setup

### 12.1 Qdrant Vector Database

**Docker Compose Addition (docker-compose.yml):**

```yaml
  crm-qdrant:
    image: qdrant/qdrant:v1.8.0
    container_name: crm-qdrant
    ports:
      - "6333:6333"   # REST API
      - "6334:6334"   # gRPC
    volumes:
      - qdrant_storage:/qdrant/storage
    environment:
      - QDRANT__SERVICE__GRPC_PORT=6334
    networks:
      - crm-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:6333/healthz"]
      interval: 30s
      timeout: 10s
      retries: 3

volumes:
  qdrant_storage:
```

### 12.2 Vector Store Collections

| Collection | Embedding Dim | Distance | Source Entity | Estimated Size |
|------------|--------------|----------|---------------|----------------|
| `crm-accounts` | 1536 | Cosine | Accounts | ~10K vectors |
| `crm-contacts` | 1536 | Cosine | Contacts | ~50K vectors |
| `crm-kb-articles` | 1536 | Cosine | KnowledgeArticles | ~5K vectors |
| `crm-emails` | 1536 | Cosine | CommunicationMessages | ~100K vectors |
| `crm-conversations` | 1536 | Cosine | AgentConversations | ~50K vectors |
| `crm-agent-memory` | 1536 | Cosine | AgentMemory | ~20K vectors |

**Initialization Script (docker/init-scripts/qdrant-init.sh):**

```bash
#!/bin/bash
QDRANT_URL=${QDRANT_URL:-http://crm-qdrant:6333}

for COLLECTION in crm-accounts crm-contacts crm-kb-articles crm-emails crm-conversations crm-agent-memory; do
    curl -X PUT "${QDRANT_URL}/collections/${COLLECTION}" \
        -H "Content-Type: application/json" \
        -d '{
            "vectors": {
                "size": 1536,
                "distance": "Cosine"
            },
            "optimizers_config": {
                "indexing_threshold": 20000
            }
        }'
done
```

### 12.3 Memory Configuration (appsettings.json)

```json
{
  "SemanticKernel": {
    "VectorStore": {
      "Provider": "Qdrant",
      "Qdrant": {
        "Host": "crm-qdrant",
        "Port": 6334,
        "ApiKey": "",
        "UseTls": false
      },
      "EmbeddingDimension": 1536,
      "Collections": {
        "Accounts": "crm-accounts",
        "Contacts": "crm-contacts",
        "KBArticles": "crm-kb-articles",
        "Emails": "crm-emails",
        "Conversations": "crm-conversations",
        "AgentMemory": "crm-agent-memory"
      }
    },
    "Agents": {
      "MaxConcurrentConversations": 100,
      "DefaultTemperature": 0.3,
      "DefaultMaxTokens": 4096,
      "ApprovalTimeoutMinutes": 60,
      "MemorySearchTopK": 5,
      "MemoryMinSimilarity": 0.75
    }
  }
}
```

---

## 13. Testing Strategy

### 13.1 Test Pyramid

```
              ┌─────────────────┐
              │   E2E Tests     │ 10 tests — Agent API → Frontend
              │   (Playwright)  │ 
              ├─────────────────┤
              │  Integration    │ 40 tests — Agent + DB + Mock LLM
              │  Tests          │
              ├─────────────────┤
              │   Unit Tests    │ 200+ tests — Agents, Plugins, Connectors
              │                 │
              └─────────────────┘
```

### 13.2 Unit Test Targets

| Component | Test File | Min Tests | Focus Areas |
|-----------|-----------|-----------|-------------|
| CrmChatCompletionConnector | CrmChatCompletionConnectorTests.cs | 15 | Translation accuracy, error handling, streaming |
| CrmEmbeddingConnector | CrmEmbeddingConnectorTests.cs | 10 | Batch embeddings, dimension validation |
| CrmKernelFactory | CrmKernelFactoryTests.cs | 10 | Plugin registration, connector wiring |
| AccountPlugin | AccountPluginTests.cs | 10 | Each method, null handling, not-found |
| OpportunityPlugin | OpportunityPluginTests.cs | 10 | Pipeline queries, stage updates |
| LeadPlugin | LeadPluginTests.cs | 10 | Score updates, similar leads |
| ServiceRequestPlugin | ServiceRequestPluginTests.cs | 10 | Classification, routing |
| SearchPlugin | SearchPluginTests.cs | 8 | Semantic + entity search |
| HumanApprovalFilter | HumanApprovalFilterTests.cs | 15 | Approval tiers, timeout, auto-approve |
| AgentExecutionService | AgentExecutionServiceTests.cs | 15 | Lifecycle, context, error recovery |
| LeadScoringAgent | LeadScoringAgentTests.cs | 20 | Scoring accuracy, edge cases |
| SupportTriageAgent | SupportTriageAgentTests.cs | 20 | Classification, KB matching |
| NextBestActionAgent | NextBestActionAgentTests.cs | 15 | Action relevance, ranking |
| SalesIntelligenceAgent | SalesIntelligenceAgentTests.cs | 15 | Risk detection, competitor analysis |
| AgentOrchestrator | AgentOrchestratorTests.cs | 15 | Routing, chaining, parallel |
| **Per P1/P2 Agent** | {Agent}Tests.cs | 10 each | Core functionality |
| **Total** | | **~250** | |

### 13.3 Agent Evaluation Framework

For agents producing qualitative outputs (scoring, recommendations), we need evaluation datasets:

| Agent | Evaluation Dataset | Metric | Target |
|-------|-------------------|--------|--------|
| Lead Scoring | 100 labeled leads | Score accuracy (±10 points) | ≥ 75% |
| Support Triage | 200 categorized tickets | Classification accuracy | ≥ 80% |
| Next Best Action | 50 entity scenarios | Relevance rating (1-5) | ≥ 3.5 avg |
| Sales Intelligence | 30 deal analyses | Risk detection recall | ≥ 70% |
| Email Assistant | 50 email drafts | Quality rating (1-5) | ≥ 4.0 avg |
| Customer Success | 40 accounts | Churn prediction accuracy | ≥ 65% |

### 13.4 Mock LLM for Testing

```csharp
public class MockAIPort : IAIPort
{
    private readonly Dictionary<string, AIChatResponse> _scriptedResponses = new();
    
    public void ScriptResponse(string containsText, AIChatResponse response)
        => _scriptedResponses[containsText] = response;
    
    public Task<AIChatResponse> ChatAsync(AIChatRequest request, CancellationToken ct)
    {
        var match = _scriptedResponses.FirstOrDefault(
            kv => request.Messages.Any(m => m.Content.Contains(kv.Key)));
        return Task.FromResult(match.Value ?? DefaultResponse());
    }
}
```

---

## 14. Feature Flags & Rollout

### 14.1 Feature Flag Hierarchy

```
EnableAgentSubsystem                    ← Master kill-switch
├── EnableAgent_LeadScoring             ← Per-agent toggle
├── EnableAgent_SupportTriage
├── EnableAgent_NextBestAction
├── EnableAgent_SalesIntelligence
├── EnableAgent_EmailAssistant
├── EnableAgent_CustomerSuccess
├── EnableAgent_RevenueIntelligence
├── EnableAgent_TicketResolution
├── EnableAgent_DocumentIntelligence
├── EnableAgent_SalesCoach
├── EnableAgent_MeetingIntelligence
├── EnableAgent_ConversationIntelligence
├── EnableAgentOrchestrator             ← Multi-agent routing
├── EnableAgentApprovalWorkflow         ← Human-in-the-loop
└── EnableAgentMemory                   ← Vector store / RAG
```

### 14.2 Rollout Plan

| Week | Agents Enabled | Audience | Monitoring |
|------|---------------|----------|------------|
| Week 4 | Lead Scoring | Internal QA (5 users) | Manual review of all scores |
| Week 5 | + Support Triage | Internal QA (5 users) | Classification accuracy tracking |
| Week 6 | + Next Best Action | Internal QA (10 users) | Relevance feedback collection |
| Week 7 | + Sales Intelligence | Internal QA (10 users) | Deal risk review |
| Week 8 | All P0 Agents | All internal users (50) | Full metrics dashboard |
| Week 10 | + P1 Agents (internal) | Internal QA (10 users) | Phased quality review |
| Week 13 | All P0 + P1 Agents | All internal users | Full analytics |
| Week 16 | All Agents | All users + customers | Full monitoring |

### 14.3 FeatureManagement Configuration

```json
{
  "FeatureManagement": {
    "EnableAgentSubsystem": true,
    "EnableAgent_LeadScoring": true,
    "EnableAgent_SupportTriage": true,
    "EnableAgent_NextBestAction": true,
    "EnableAgent_SalesIntelligence": true,
    "EnableAgent_EmailAssistant": false,
    "EnableAgent_CustomerSuccess": false,
    "EnableAgent_RevenueIntelligence": false,
    "EnableAgent_TicketResolution": false,
    "EnableAgent_DocumentIntelligence": false,
    "EnableAgent_SalesCoach": false,
    "EnableAgent_MeetingIntelligence": false,
    "EnableAgent_ConversationIntelligence": false,
    "EnableAgentOrchestrator": false,
    "EnableAgentApprovalWorkflow": true,
    "EnableAgentMemory": true
  }
}
```

---

## 15. .NET 10 Upgrade Sequencing

> **Context:** There is an active plan to upgrade the CRM Solution from .NET 8.0 (current) to .NET 10 (GA: November 2025).
> This section analyzes how the Semantic Kernel integration should be sequenced relative to that upgrade.

### 15.1 Recommendation: SK First, Then .NET 10

| Option | Pros | Cons | **Verdict** |
|--------|------|------|-------------|
| **A — SK on .NET 8, then upgrade to .NET 10** | SK 1.x has full .NET 8 support; no unknowns. Agents ship sooner. TFM bump is a single PR touching Directory.Build.props + docker base images — low risk after SK is stable. | Two change waves. Minor: SK packages may release .NET 10-optimized builds shortly after GA. | ✅ **Recommended** |
| **B — .NET 10 first, then SK** | SK on latest runtime from day one. Single TFM going forward. | Delays agent delivery 4-8 weeks. .NET 10 upgrade itself may surface EF Core 10, ASP.NET 10 breaking changes that consume sprint capacity. SK packages will work on .NET 10 via netstandard2.0 / net8.0 TFM compat anyway. | ❌ Not recommended |
| **C — Simultaneous** | One combined effort. | Very high risk surface. Two large changes at once make root-cause analysis difficult if anything breaks. | ❌ Not recommended |

**Rationale for Option A:**

1. **Semantic Kernel 1.x targets `netstandard2.0` and `net8.0`** — it will run on .NET 10 without recompilation via TFM compatibility.
2. **The .NET 10 upgrade is primarily a TFM bump** (`net8.0` → `net10.0` in `Directory.Build.props`), plus updating NuGet packages (`Microsoft.EntityFrameworkCore` 8.x → 10.x, `Microsoft.AspNetCore.*` 8.x → 10.x). This is a well-understood, lower-risk change.
3. **The SK integration introduces 5 new entities, 12+ services, 15+ plugins, and a new Qdrant dependency.** Debugging these on a stable, known .NET 8 runtime is significantly easier.
4. **After SK is stable (end of Phase 1, ~Week 8),** the .NET 10 upgrade can be done as an isolated sprint with confidence that any regressions are from the TFM change, not SK.

### 15.2 Sequencing Timeline

```
┌──────────────────────────────────────────────────────────────────────────┐
│  Weeks 1-4    │  SK Phase 0: Foundation on .NET 8                       │
│  Weeks 5-8    │  SK Phase 1: P0 Agents on .NET 8                       │
│  Weeks 9-10   │  ── .NET 10 UPGRADE SPRINT ──                          │
│               │  • Directory.Build.props: net8.0 → net10.0             │
│               │  • NuGet: EF Core 10, ASP.NET 10, etc.                 │
│               │  • Docker base images: mcr.microsoft.com/dotnet/...:10.0│
│               │  • Regression run: all 5,160+ tests must pass          │
│               │  • Verify SK packages on .NET 10 runtime               │
│  Weeks 11-13  │  SK Phase 2: P1 Agents on .NET 10                      │
│  Weeks 14-16  │  SK Phase 3: Multi-Agent & Analytics on .NET 10        │
└──────────────────────────────────────────────────────────────────────────┘
```

### 15.3 .NET 10 Upgrade Checklist (for the dedicated sprint)

- [ ] Update `Directory.Build.props`: `<TargetFramework>net10.0</TargetFramework>`
- [ ] Update `global.json` to require .NET 10 SDK
- [ ] Update all `Microsoft.*` NuGet packages to 10.x
- [ ] Update all `Pomelo.EntityFrameworkCore.MySql` to 10.x-compatible version
- [ ] Update Docker base images in all `Dockerfile.*` files
- [ ] Run `dotnet build CRM.sln` — fix any API breaking changes
- [ ] Run `dotnet test` — all 5,160+ tests must pass
- [ ] Run E2E BVT suite (118 tests) against deployed build
- [ ] Verify SK Kernel, plugins, and Qdrant connector work on .NET 10
- [ ] Verify `db.Database.MigrateAsync()` works with EF Core 10
- [ ] Update CI/CD pipeline SDK version
- [ ] Deploy to dev server, smoke test all agent endpoints

### 15.4 SK Package Compatibility Notes

| Package | .NET 8 Support | .NET 10 Expected | Notes |
|---------|---------------|-------------------|-------|
| `Microsoft.SemanticKernel` 1.x | ✅ `net8.0` | ✅ via TFM compat | May release native `net10.0` TFM post-GA |
| `Microsoft.SemanticKernel.Connectors.Qdrant` | ✅ `net8.0` | ✅ via TFM compat | |
| `Microsoft.SemanticKernel.Planners.Handlebars` | ✅ `net8.0` | ✅ via TFM compat | |
| `Qdrant.Client` | ✅ `netstandard2.0` | ✅ native | |

---

## 16. Success Metrics & Acceptance Criteria

### 16.1 Phase-Level Acceptance

| Phase | Criteria | Measurement |
|-------|----------|-------------|
| **Phase 0** | SK kernel builds, connectors bridge to IAIPort, 3+ plugins work, Qdrant healthy | Build + 50 unit tests pass |
| **Phase 1** | 4 P0 agents operational, scoring accuracy ≥ 75%, triage accuracy ≥ 80% | 120+ tests pass + evaluation benchmarks |
| **Phase 2** | 5 P1 agents operational, email quality ≥ 4/5, auto-resolve rate ≥ 20% | 185+ tests pass + quality review |
| **Phase 3** | Multi-agent routing ≥ 90%, analytics dashboard live, all 12 agents operational | 250+ tests pass + E2E flow |

### 16.2 Business Metrics (Post-Launch)

| Metric | Baseline | 30-Day Target | 90-Day Target |
|--------|----------|--------------|---------------|
| Lead scoring time | Manual (15 min/lead) | Automated (< 5 sec) | < 3 sec |
| Ticket triage time | Manual (8 min/ticket) | Auto-classified (< 10 sec) | < 5 sec + auto-route |
| KB resolution rate | 0% (no auto-resolution) | 15% of eligible tickets | 25% of eligible tickets |
| Sales rep time saved | 0 hrs/week | 2 hrs/week (next best action) | 4 hrs/week |
| Email draft quality | N/A | 4.0/5.0 user rating | 4.2/5.0 user rating |
| Deal risk detection | 0% | 70% recall | 80% recall |
| Agent adoption rate | 0% | 30% of users | 60% of users |

### 16.3 Technical Metrics

| Metric | Target |
|--------|--------|
| Agent response latency (p50) | < 3 seconds |
| Agent response latency (p95) | < 8 seconds |
| Agent availability | ≥ 99.5% |
| Approval resolution time (p50) | < 5 minutes |
| Vector search latency (p50) | < 200ms |
| Memory seeding throughput | ≥ 100 entities/minute |
| Cost per agent conversation | < $0.05 (with caching) |

---

## 17. Risk Register

| # | Risk | Probability | Impact | Mitigation |
|---|------|------------|--------|------------|
| R1 | LLM hallucination in write operations | High | High | Human-in-the-loop for all writes; confidence thresholds; citation requirements |
| R2 | SK breaking changes (pre-1.0 packages) or .NET 10 API surface changes | Medium | Medium | Pin package versions; abstraction layer; adapter pattern; see §15 for .NET 10 sequencing |
| R3 | Vector store performance at scale | Low | Medium | Qdrant sharding; collection partitioning; embedding caching |
| R4 | Agent response latency | Medium | Medium | Streaming responses; background pre-computation; result caching |
| R5 | Token cost overrun | Medium | Low | Cost tracking filter; per-agent token budgets; model tiering |
| R6 | Low user adoption | Medium | High | Phased rollout; user feedback loop; training; progressive disclosure |
| R7 | Data leakage across tenants | Low | Critical | Tenant-scoped memory; filter by userId/accountId; prompt injection guard |
| R8 | Qdrant unavailability | Low | Medium | Fallback to SQL-based search; health check + circuit breaker |

---

## 18. Appendix — Configuration Reference

### Full appsettings.json Section

```json
{
  "SemanticKernel": {
    "Enabled": true,
    "VectorStore": {
      "Provider": "Qdrant",
      "Qdrant": {
        "Host": "crm-qdrant",
        "Port": 6334,
        "ApiKey": "",
        "UseTls": false
      },
      "InMemory": {
        "Enabled": false
      },
      "EmbeddingDimension": 1536,
      "Collections": {
        "Accounts": "crm-accounts",
        "Contacts": "crm-contacts",
        "KBArticles": "crm-kb-articles",
        "Emails": "crm-emails",
        "Conversations": "crm-conversations",
        "AgentMemory": "crm-agent-memory"
      }
    },
    "Agents": {
      "MaxConcurrentConversations": 100,
      "DefaultTemperature": 0.3,
      "DefaultMaxTokens": 4096,
      "ApprovalTimeoutMinutes": 60,
      "ConversationHistoryLimit": 50,
      "MemorySearchTopK": 5,
      "MemoryMinSimilarity": 0.75,
      "CostBudgetPerDay": 50.00,
      "CostBudgetPerConversation": 0.50,
      "EnableStreaming": true,
      "BackgroundScoringIntervalHours": 6,
      "MemorySeedingBatchSize": 100
    },
    "Models": {
      "Default": "gpt-4o",
      "Scoring": "gpt-4o-mini",
      "Embedding": "text-embedding-3-small",
      "LargeContext": "gpt-4o"
    }
  }
}
```

### Docker Compose Environment Variables

```yaml
environment:
  # Semantic Kernel
  - SemanticKernel__Enabled=true
  - SemanticKernel__VectorStore__Provider=Qdrant
  - SemanticKernel__VectorStore__Qdrant__Host=crm-qdrant
  - SemanticKernel__VectorStore__Qdrant__Port=6334
  - SemanticKernel__Agents__MaxConcurrentConversations=100
  - SemanticKernel__Agents__ApprovalTimeoutMinutes=60
  - SemanticKernel__Agents__CostBudgetPerDay=50.00
  # Feature Flags
  - FeatureManagement__EnableAgentSubsystem=true
  - FeatureManagement__EnableAgent_LeadScoring=true
  - FeatureManagement__EnableAgent_SupportTriage=true
  - FeatureManagement__EnableAgent_NextBestAction=true
  - FeatureManagement__EnableAgent_SalesIntelligence=true
  - FeatureManagement__EnableAgentApprovalWorkflow=true
  - FeatureManagement__EnableAgentMemory=true
```

### API Endpoints Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/agents` | List all available agents |
| GET | `/api/agents/{agentId}` | Get agent details |
| POST | `/api/agents/{agentId}/chat` | Send message to agent |
| GET | `/api/agents/{agentId}/conversations` | Get conversation history |
| GET | `/api/agents/conversations/{conversationId}` | Get specific conversation |
| POST | `/api/agents/conversations/{conversationId}/rate` | Rate conversation (1-5) |
| GET | `/api/agents/next-best-actions/{entityType}/{entityId}` | Get NBA for entity |
| GET | `/api/agents/deal-intelligence/{opportunityId}` | Get deal analysis |
| POST | `/api/agents/email/draft` | Generate email draft |
| POST | `/api/agents/resolve/{serviceRequestId}` | Attempt ticket resolution |
| POST | `/api/agents/orchestrate` | Multi-agent natural language |
| GET | `/api/agents/approvals/pending` | Get pending approvals |
| POST | `/api/agents/approvals/{id}/approve` | Approve action |
| POST | `/api/agents/approvals/{id}/reject` | Reject action |
| GET | `/api/agents/analytics/usage` | Agent usage stats |
| GET | `/api/agents/analytics/accuracy` | Agent accuracy metrics |
| GET | `/api/agents/analytics/cost` | Agent cost tracking |
| GET | `/api/agents/admin` | Admin: agent configuration |
| PUT | `/api/agents/admin/{agentId}` | Admin: update agent config |
| POST | `/api/agents/admin/{agentId}/toggle` | Admin: enable/disable agent |

---

## Related Documentation

- [ADR-004-Semantic-Kernel-Integration.md](ADR-004-Semantic-Kernel-Integration.md) — Architecture decision
- [ROADMAP_BEST_IN_CLASS.md](../ROADMAP_BEST_IN_CLASS.md) — Gap analysis (input)
- [ADR-001-Pluggable-Architecture-Strategy.md](ADR-001-Pluggable-Architecture-Strategy.md) — Hexagonal architecture
- [PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md](PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md) — Provider status
- [PHASE4_SERVICE_SPECIFICATIONS.md](../PHASE4_SERVICE_SPECIFICATIONS.md) — Service interfaces

---

**END OF IMPLEMENTATION PLAN**

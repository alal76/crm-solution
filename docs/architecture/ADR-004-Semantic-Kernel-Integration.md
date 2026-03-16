# ADR-004: Semantic Kernel Integration for Agentic AI

## Architecture Decision Record

| Field | Value |
|-------|-------|
| **ADR ID** | ADR-004 |
| **Title** | Semantic Kernel Integration for Agentic AI Capabilities |
| **Status** | ACCEPTED |
| **Date** | 2026-02-18 |
| **Decision Makers** | Architecture Team, Product Leadership |
| **Consulted** | Development Team, AI/ML Team, Operations |
| **Informed** | All Stakeholders |
| **Supersedes** | None |
| **Related** | ADR-001 (Pluggable Architecture Strategy) |

---

## Executive Summary

This ADR proposes integrating **Microsoft Semantic Kernel (SK)** as the AI orchestration layer for the CRM solution, enabling autonomous AI agents that address critical gaps identified in the best-in-class feature analysis. The current AI architecture (`IAIPort` with 4 LLM providers) provides raw model access but lacks agent loops, memory persistence, tool orchestration, multi-agent coordination, and human-in-the-loop controls.

SK will be layered **on top of** the existing hexagonal architecture — not replacing `IAIPort`, but consuming it through an SK Connector adapter. This preserves the pluggable provider model (ADR-001) while adding agentic capabilities.

**Key Decisions:**

| Decision | Recommendation |
|----------|----------------|
| AI orchestration framework? | **Semantic Kernel** (stable, production-ready, MIT license) |
| Replace IAIPort? | **No** — SK wraps IAIPort via custom connector |
| Agent framework? | **SK Agents** (evolving from SK Process/Handlebars) |
| Memory/Vector store? | **SK Memory abstractions** with Qdrant or pgvector |
| Multi-agent coordination? | **SK Process framework** for agent workflows |

**Gap Impact:** This integration directly addresses the following gap analysis scores:

| Gap Category | Current Score | Post-SK Target | Delta |
|--------------|---------------|----------------|-------|
| AI-Native Intelligence (§1.1) | 55% | 82% | +27% |
| Agentic AI (§3.1) | 18% | 65% | +47% |
| Document Intelligence (§3.4) | 38% | 55% | +17% |
| Service Excellence (§1.3) | 72% | 82% | +10% |
| RevOps (§3.3) | 42% | 55% | +13% |

---

## Table of Contents

1. [Context](#1-context)
2. [Problem Statement](#2-problem-statement)
3. [Decision Drivers](#3-decision-drivers)
4. [Options Considered](#4-options-considered)
5. [Decision Outcome](#5-decision-outcome)
6. [Integration Architecture](#6-integration-architecture)
7. [Agent Catalog](#7-agent-catalog)
8. [SK-to-IAIPort Bridge](#8-sk-to-iaiport-bridge)
9. [Plugin Architecture](#9-plugin-architecture)
10. [Memory & Vector Store](#10-memory--vector-store)
11. [Human-in-the-Loop](#11-human-in-the-loop)
12. [Entity Model](#12-entity-model)
13. [Risk Analysis](#13-risk-analysis)
14. [Consequences](#14-consequences)
15. [NuGet Packages](#15-nuget-packages)
16. [References](#16-references)

---

## 1. Context

### 1.1 Current AI Architecture

The CRM solution implements AI through a hexagonal port/adapter pattern (per ADR-001):

```
┌──────────────────────────────────────────────────────┐
│  CRM.Core/Ports/Output/Providers/IAIPort.cs          │
│  ────────────────────────────────────────────────     │
│  Text Generation:                                     │
│    CompleteAsync, ChatAsync, StreamChatAsync          │
│  Embeddings:                                          │
│    GetEmbeddingAsync, GetEmbeddingsAsync             │
│  CRM-Specific:                                        │
│    GenerateEmailDraftAsync, SuggestReplyAsync         │
│    SummarizeAsync, ExtractEntitiesAsync               │
│    AnalyzeSentimentAsync, GetNextBestActionsAsync     │
│  Tool Calling:                                        │
│    AITool, AIToolCall (in AIChatRequest/Response)     │
│  Usage Tracking:                                      │
│    EstimateTokens, GetUsageStatsAsync                │
└─────────────────────┬────────────────────────────────┘
                      │
        ┌─────────────┼─────────────────┐
        ▼             ▼                 ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│   Ollama     │ │ AzureOpenAI  │ │  OpenRouter   │
│   Provider   │ │   Provider   │ │   Provider    │
│   (~770 ln)  │ │   (~800 ln)  │ │   (~850 ln)   │
└──────────────┘ └──────────────┘ └──────────────┘
                      ▼
               ┌──────────────┐
               │   Bedrock    │
               │   Provider   │
               │   (~850 ln)  │
               └──────────────┘
```

**Capabilities:**
- ✅ Multi-provider LLM access (4 providers, 100+ models via OpenRouter)
- ✅ Chat completions with message history
- ✅ Streaming responses
- ✅ Embeddings (single and batch)
- ✅ Tool/function calling DTOs (`AITool`, `AIToolCall`)
- ✅ CRM-specific operations (email drafts, sentiment, entity extraction)
- ✅ Token usage tracking and cost estimation
- ✅ Provider health checks and factory switching

**Limitations (what IAIPort cannot do today):**
- ❌ No autonomous agent loop (reason → act → observe → repeat)
- ❌ No persistent memory across conversations
- ❌ No tool execution orchestration (DTOs exist, but no execution engine)
- ❌ No multi-agent coordination
- ❌ No human-in-the-loop approval flow
- ❌ No planning or goal decomposition
- ❌ No RAG (Retrieval-Augmented Generation) pipeline
- ❌ No vector store for semantic search/memory
- ❌ No agent analytics or evaluation

### 1.2 Existing AI Services

| Service | File | Purpose |
|---------|------|---------|
| `AILeadScoringService` | `CRM.Infrastructure/Services/AILeadScoringService.cs` | Rule-based + LLM scoring via IAIPort |
| `AIKnowledgeSearchService` | `CRM.Infrastructure/Services/AIKnowledgeSearchService.cs` | Embedding-based KB search via IAIPort |
| `AIChatbotController` | `CRM.Api/Controllers/AIChatbotController.cs` | Session-based chatbot (4 endpoints) |
| `AIEmailController` | `CRM.Api/Controllers/AIEmailController.cs` | Email AI operations (4 endpoints) |
| `AILeadScoringController` | `CRM.Api/Controllers/AILeadScoringController.cs` | Lead/opportunity scoring (6 endpoints) |

### 1.3 Best-in-Class Gap Analysis Findings

From `ROADMAP_BEST_IN_CLASS.md`, the AI-relevant gaps are:

**§1.1 AI-Native Intelligence Layer (55% → 95% target):**
- No autonomous agent loop
- Rule-based lead scoring, not ML-driven predictive scoring
- No conversation intelligence (call analysis, coaching insights)
- No real-time scoring triggers
- No vector DB for semantic retrieval
- No multi-agent orchestration

**§3.1 Agentic AI (18% → 80% target):**
- No AI Sales Agent (autonomous SDR — research, qualify, schedule)
- No AI Customer Success Agent (proactive outreach, health monitoring)
- No AI Support Agent (autonomous ticket resolution)
- No agent memory/context persistence
- No human-in-the-loop approval for AI actions
- No agent analytics (performance, cost, success metrics)

**§3.4 Document Intelligence (38% → 85% target):**
- No AI contract analysis or clause extraction
- No AI-powered proposal generation

**§1.3 Service Excellence (72% → 95% target):**
- No AI-powered ticket auto-classification

---

## 2. Problem Statement

The CRM's current AI implementation provides **model access** (LLM completions, embeddings) but not **agent intelligence** (autonomous reasoning, tool use, memory, multi-step planning). The existing `IAIPort` is essentially a "function call wrapper" around LLMs — it sends a prompt and gets a response, but cannot:

1. **Reason and Plan** — Break complex goals into sub-tasks
2. **Use Tools Autonomously** — Read CRM data, create records, send emails without explicit invocation
3. **Remember Context** — Persist knowledge across sessions
4. **Collaborate** — Multiple agents working on a shared problem
5. **Seek Approval** — Pause execution for human review of consequential actions

Without these capabilities, the CRM cannot deliver the AI Sales Agent, Support Agent, or any autonomous intelligence that best-in-class competitors are starting to offer.

---

## 3. Decision Drivers

| # | Driver | Weight | Notes |
|---|--------|--------|-------|
| 1 | **Production readiness** | Critical | Must be stable enough for enterprise CRM |
| 2 | **Hexagonal architecture compatibility** | Critical | Must work with existing IAIPort/provider pattern |
| 3 | **.NET native** | High | Team expertise is C#/.NET, avoid polyglot complexity |
| 4 | **Multi-LLM support** | High | Must work with Ollama, Azure, Bedrock, OpenRouter |
| 5 | **Memory/RAG support** | High | Vector store integration for semantic retrieval |
| 6 | **Agent orchestration** | High | Multi-step reasoning, tool calling, goal tracking |
| 7 | **Human-in-the-loop** | High | Critical for enterprise trust in AI actions |
| 8 | **Active maintenance** | Medium | Regular updates, community support |
| 9 | **Extensibility** | Medium | Custom plugins, connectors, memory providers |
| 10 | **Migration path** | Medium | Future compatibility with Microsoft Agent Framework |

---

## 4. Options Considered

### Option A: Custom Agent Framework (Build from Scratch)

Build a bespoke agent framework on top of IAIPort using the existing `AITool`/`AIToolCall` DTOs.

| Aspect | Assessment |
|--------|------------|
| **Effort** | 6-8 weeks for basic agent loop, 16+ weeks for full features |
| **Pros** | Full control, perfect fit to IAIPort, no external dependencies |
| **Cons** | Reinventing the wheel, no community, no memory/RAG abstractions, maintenance burden |
| **Risk** | High — complex to build planning, memory, multi-agent from scratch |

### Option B: Semantic Kernel ⭐ RECOMMENDED

Microsoft's open-source AI orchestration SDK for .NET.

| Aspect | Assessment |
|--------|------------|
| **Maturity** | GA (v1.x stable), 27.2K GitHub stars, 43 NuGet packages |
| **License** | MIT |
| **Effort** | 2-3 weeks for foundation, 8-12 weeks for full agent catalog |
| **Pros** | Native .NET, built-in memory/RAG, plugin system, function calling, process orchestration, active Microsoft investment |
| **Cons** | Adds dependency, learning curve, connector adapter needed for IAIPort |
| **Risk** | Low — production-ready, Microsoft-backed, large community |

### Option C: Microsoft Agent Framework (Preview)

Microsoft's emerging multi-agent framework built on SK.

| Aspect | Assessment |
|--------|------------|
| **Maturity** | Pre-release (0.x), experimental APIs |
| **Effort** | Unknown — APIs may change |
| **Pros** | Future direction, multi-agent native, built on SK |
| **Cons** | Pre-release, breaking changes expected, incomplete documentation |
| **Risk** | High — not production-ready, may change fundamentally |

### Option D: LangChain.NET / Semantic Memory

Community ports of LangChain concepts to .NET.

| Aspect | Assessment |
|--------|------------|
| **Maturity** | Low — small community, irregular updates |
| **Effort** | 3-4 weeks for basics |
| **Pros** | Familiar LangChain patterns |
| **Cons** | Small .NET community, less native, weaker tooling |
| **Risk** | Medium — maintenance and longevity concerns |

### Comparison Matrix

| Criterion | Custom | Semantic Kernel | Agent Framework | LangChain.NET |
|-----------|--------|-----------------|-----------------|---------------|
| Production Ready | ❌ (must build) | ✅ GA | ❌ Preview | ⚠️ Partial |
| .NET Native | ✅ | ✅ | ✅ | ⚠️ Port |
| Agent Loop | ❌ Build | ✅ Built-in | ✅ Built-in | ⚠️ Partial |
| Memory/RAG | ❌ Build | ✅ Built-in | ✅ Via SK | ❌ Minimal |
| Multi-Agent | ❌ Build | ✅ Process API | ✅ Native | ❌ No |
| Plugin System | ❌ Build | ✅ Rich | ✅ Via SK | ⚠️ Basic |
| Human-in-Loop | ❌ Build | ✅ Filters | ✅ Native | ❌ No |
| IAIPort Compat. | ✅ Native | ⚠️ Adapter | ⚠️ Adapter | ⚠️ Adapter |
| Community | ❌ None | ✅ 27K stars | ⚠️ New | ⚠️ Small |
| Migration Path | ❌ | ✅ → Agent FW | ✅ Final target | ❌ |
| Effort | 16+ weeks | 8-12 weeks | Unknown | 10-14 weeks |

---

## 5. Decision Outcome

### Chosen Option: **B — Semantic Kernel**

**Rationale:**

1. **Production-ready** — GA release with stable APIs, used in Microsoft 365 Copilot
2. **Perfect architectural fit** — Plugin system maps to CRM services, connectors map to IAIPort providers
3. **.NET native** — First-class C# support, no polyglot overhead
4. **Comprehensive** — Includes agent loop, memory, RAG, planning, function calling, process orchestration
5. **Future-proof** — Microsoft Agent Framework is built ON TOP of SK; adopting SK now creates a natural migration path
6. **Community** — 27.2K stars, 43 NuGet packages, active development, Microsoft investment

**Integration Strategy:**

```
IAIPort (existing) ← CrmAIConnector (new adapter) → Semantic Kernel
                                                         │
                                                    ┌────┴────┐
                                                    │ Plugins │ (wrap existing services)
                                                    │ Memory  │ (vector store)
                                                    │ Agents  │ (autonomous reasoning)
                                                    │ Process │ (multi-agent workflows)
                                                    └─────────┘
```

SK does **not** replace `IAIPort`. Instead:
- A `CrmAIConnector` adapter implements SK's `IChatCompletionService` and `ITextEmbeddingGenerationService` by delegating to `IAIPort`
- Existing CRM services become SK Plugins via `KernelPluginFactory.CreateFromType<T>()`
- The `AIProviderFactory` continues to select the underlying LLM provider
- SK adds the orchestration layer (agent loop, memory, planning) that IAIPort lacks

---

## 6. Integration Architecture

### 6.1 Layered Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         API LAYER (Controllers)                          │
│  AgentController, AIChatbotController, AILeadScoringController          │
└─────────────────────────────────────┬───────────────────────────────────┘
                                      │
┌─────────────────────────────────────▼───────────────────────────────────┐
│                      AGENT ORCHESTRATION (NEW)                           │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │  CRM.Infrastructure/AI/Agents/                                     │ │
│  │  ├── LeadScoringAgent.cs       (Predictive lead scoring)          │ │
│  │  ├── SalesAgent.cs             (Autonomous SDR)                   │ │
│  │  ├── SupportAgent.cs           (Ticket resolution)                │ │
│  │  ├── CustomerSuccessAgent.cs   (Proactive outreach)               │ │
│  │  ├── EmailAssistantAgent.cs    (Context-aware email)              │ │
│  │  ├── RevenueIntelAgent.cs      (Deal risk & forecasting)         │ │
│  │  ├── DocumentIntelAgent.cs     (Contract analysis)                │ │
│  │  └── AgentOrchestrator.cs      (Multi-agent coordinator)         │ │
│  └────────────────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │  CRM.Infrastructure/AI/Plugins/                                    │ │
│  │  ├── AccountPlugin.cs          (Read/write accounts)              │ │
│  │  ├── ContactPlugin.cs          (Read/write contacts)              │ │
│  │  ├── OpportunityPlugin.cs      (Pipeline operations)              │ │
│  │  ├── LeadPlugin.cs             (Lead operations)                  │ │
│  │  ├── ServiceRequestPlugin.cs   (Ticket operations)                │ │
│  │  ├── EmailPlugin.cs            (Send/draft emails)                │ │
│  │  ├── CalendarPlugin.cs         (Schedule meetings)                │ │
│  │  ├── SearchPlugin.cs           (Search CRM data)                  │ │
│  │  ├── KnowledgeBasePlugin.cs    (KB article retrieval)             │ │
│  │  └── NotificationPlugin.cs     (Send notifications)               │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────┬───────────────────────────────────┘
                                      │
┌─────────────────────────────────────▼───────────────────────────────────┐
│                      SEMANTIC KERNEL CORE                                │
│  ┌────────────────────────────────────────────────────────────────────┐ │
│  │  Kernel (IKernel)                                                  │ │
│  │  ├── Plugins (registered CRM services)                            │ │
│  │  ├── Memory (ISemanticTextMemory → Qdrant/pgvector)               │ │
│  │  ├── Filters (IAutoFunctionInvocationFilter → human-in-loop)      │ │
│  │  └── Connectors (CrmAIConnector → IAIPort)                        │ │
│  └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────┬───────────────────────────────────┘
                                      │
┌─────────────────────────────────────▼───────────────────────────────────┐
│                      EXISTING AI INFRASTRUCTURE                          │
│  ┌─────────────────┐ ┌──────────────────┐ ┌──────────────────────────┐ │
│  │  IAIPort         │ │ AIProviderFactory │ │ AdapterRegistry          │ │
│  │  (Port interface)│ │ (Provider select) │ │ (Health monitoring)      │ │
│  └────────┬────────┘ └────────┬─────────┘ └──────────────────────────┘ │
│           │                   │                                          │
│  ┌────────▼────────┐ ┌───────▼─────────┐ ┌──────────────────────────┐ │
│  │ OllamaProvider  │ │AzureOpenAIProvider│ │ OpenRouter/Bedrock      │ │
│  └─────────────────┘ └─────────────────┘ └──────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Kernel Factory

A `CrmKernelFactory` creates configured SK `Kernel` instances with CRM plugins and the IAIPort connector:

```csharp
// CRM.Infrastructure/AI/CrmKernelFactory.cs
public class CrmKernelFactory : ICrmKernelFactory
{
    private readonly IAIPort _aiPort;
    private readonly IServiceProvider _serviceProvider;

    public Kernel CreateKernel(AgentProfile? profile = null)
    {
        var builder = Kernel.CreateBuilder();

        // Bridge: IAIPort → SK Connector
        builder.Services.AddSingleton<IChatCompletionService>(
            new CrmChatCompletionConnector(_aiPort));
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(
            new CrmEmbeddingConnector(_aiPort));

        // Register CRM service plugins
        builder.Plugins.AddFromType<AccountPlugin>();
        builder.Plugins.AddFromType<ContactPlugin>();
        builder.Plugins.AddFromType<OpportunityPlugin>();
        builder.Plugins.AddFromType<LeadPlugin>();
        builder.Plugins.AddFromType<ServiceRequestPlugin>();
        builder.Plugins.AddFromType<EmailPlugin>();
        builder.Plugins.AddFromType<SearchPlugin>();
        builder.Plugins.AddFromType<KnowledgeBasePlugin>();

        // Human-in-the-loop filter
        builder.Services.AddSingleton<IAutoFunctionInvocationFilter>(
            new HumanApprovalFilter(_serviceProvider));

        // Memory (vector store for RAG)
        builder.Services.AddSingleton<ISemanticTextMemory>(
            _serviceProvider.GetRequiredService<ISemanticTextMemory>());

        return builder.Build();
    }
}
```

### 6.3 Data Flow — Agent Execution

```
User Request → AgentController
                    │
                    ▼
            AgentOrchestrator.RunAsync(agentType, request)
                    │
                    ▼
            CrmKernelFactory.CreateKernel(agentProfile)
                    │
                    ▼
            SK Agent Loop (reason → plan → act → observe)
                    │
              ┌─────┼──────────────┐
              ▼     ▼              ▼
         SK Plugin  SK Plugin    SK Memory
         (AccountPlugin)  (EmailPlugin)   (Vector Store)
              │     │              │
              ▼     ▼              ▼
         AccountService  INotificationPort  Qdrant/pgvector
              │     │
              ▼     ▼
         CrmDbContext → MariaDB
```

---

## 7. Agent Catalog

Agents mapped to specific gap analysis items:

### 7.1 P0 Agents (Critical — Address Lowest Scores)

| Agent | Gap Reference | Current Score | Target | Description |
|-------|---------------|---------------|--------|-------------|
| **Lead Scoring Agent** | §1.1 Predictive Lead Scoring | 55% | 82% | ML-driven scoring using firmographic data, engagement signals, and historical conversion patterns. Replaces rule-based `AILeadScoringService`. |
| **Support Agent** | §3.1 AI Support Agent | 18% | 65% | Autonomous ticket resolution: classify ticket → search KB → generate response → resolve or escalate. Human-in-the-loop for complex cases. |
| **Next Best Action Agent** | §1.1 Next Best Action Engine | 55% | 82% | Context-aware recommendations: analyzes account activity, pipeline position, engagement recency to suggest call/email/meeting/task. |
| **Sales Agent** | §3.1 AI Sales Agent | 18% | 65% | Autonomous SDR: research prospect → enrich data → qualify → personalize outreach → schedule meeting. |

### 7.2 P1 Agents (High Value)

| Agent | Gap Reference | Current Score | Target | Description |
|-------|---------------|---------------|--------|-------------|
| **Email Assistant Agent** | §1.1 Email AI Assistant | 55% | 82% | Context-aware email composition using account history, recent interactions, opportunity stage. Upgrades existing `GenerateEmailDraftAsync`. |
| **Customer Success Agent** | §3.1 AI CS Agent | 18% | 65% | Proactive health monitoring: detect churn signals → recommend interventions → auto-create tasks for CSMs. |
| **Revenue Intelligence Agent** | §3.3 RevOps | 42% | 55% | Deal risk scoring, pipeline coverage analysis, forecast accuracy. Aggregates signals across opportunities. |
| **Ticket Classifier Agent** | §1.3 Ticket Auto-Classification | 72% | 82% | Auto-classify incoming tickets: category, subcategory, priority, SLA assignment, initial agent routing. |
| **Document Intelligence Agent** | §3.4 AI Contract Analysis | 38% | 55% | Analyze contracts for risk clauses, extract key terms, suggest negotiation points. |

### 7.3 P2 Agents (Differentiators)

| Agent | Gap Reference | Description |
|-------|---------------|-------------|
| **Sales Coach Agent** | §1.1 AI Sales Coach | Analyzes rep performance, suggests improvements, provides deal-specific coaching. |
| **Meeting Intelligence Agent** | §1.1 Meeting Intelligence | Transcript analysis, action item extraction, follow-up generation. |
| **Conversation Intelligence Agent** | §1.1 Conversation Intelligence | Call sentiment analysis, talk-to-listen ratios, coaching insights. |

---

## 8. SK-to-IAIPort Bridge

### 8.1 Chat Completion Connector

```csharp
// CRM.Infrastructure/AI/Connectors/CrmChatCompletionConnector.cs
public class CrmChatCompletionConnector : IChatCompletionService
{
    private readonly IAIPort _aiPort;

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? settings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Convert SK ChatHistory → IAIPort AIChatRequest
        var request = new AIChatRequest
        {
            Messages = chatHistory.Select(m => new AIChatMessage
            {
                Role = m.Role.Label,
                Content = m.Content ?? string.Empty
            }).ToList(),
            Temperature = settings?.ExtensionData?.GetValueOrDefault("temperature") as double?,
            MaxTokens = settings?.ExtensionData?.GetValueOrDefault("max_tokens") as int?,
            Tools = ConvertKernelFunctions(kernel)  // Map SK functions → AITool
        };

        var response = await _aiPort.ChatAsync(request, cancellationToken);

        // Convert IAIPort AIChatResponse → SK ChatMessageContent
        return new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, response.Message.Content)
            {
                Metadata = new Dictionary<string, object?>
                {
                    ["usage"] = response.Usage,
                    ["model"] = response.Model,
                    ["tool_calls"] = response.ToolCalls
                }
            }
        };
    }

    // Streaming implementation delegates to IAIPort.StreamChatAsync
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? settings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // ... delegates to _aiPort.StreamChatAsync
    }
}
```

### 8.2 Embedding Connector

```csharp
// CRM.Infrastructure/AI/Connectors/CrmEmbeddingConnector.cs
public class CrmEmbeddingConnector : ITextEmbeddingGenerationService
{
    private readonly IAIPort _aiPort;

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _aiPort.GetEmbeddingsAsync(data, cancellationToken: cancellationToken);
        return response.Embeddings
            .Select(e => new ReadOnlyMemory<float>(e))
            .ToList();
    }
}
```

### 8.3 Key Principle: No Bypass

All LLM calls flow through IAIPort, preserving:
- **Provider switching** — Change LLM provider via feature flags without touching agents
- **Usage tracking** — All token consumption tracked in `AIUsageStats`
- **Health monitoring** — `AdapterRegistry` health checks still apply
- **Cost controls** — Token limits enforced at the IAIPort level

---

## 9. Plugin Architecture

### 9.1 SK Plugin Pattern

Each CRM service is exposed as an SK Plugin using `[KernelFunction]` attributes:

```csharp
// CRM.Infrastructure/AI/Plugins/AccountPlugin.cs
public class AccountPlugin
{
    private readonly IAccountService _accountService;

    public AccountPlugin(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [KernelFunction("get_account")]
    [Description("Gets an account/customer by ID")]
    public async Task<string> GetAccountAsync(
        [Description("The account ID")] int accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountService.GetByIdAsync(accountId, cancellationToken);
        return JsonSerializer.Serialize(account);
    }

    [KernelFunction("search_accounts")]
    [Description("Searches accounts by name, email, or company")]
    public async Task<string> SearchAccountsAsync(
        [Description("Search query")] string query,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _accountService.SearchAsync(query, cancellationToken);
        return JsonSerializer.Serialize(accounts);
    }

    [KernelFunction("get_account_health")]
    [Description("Gets the health score and recent activity for an account")]
    public async Task<string> GetAccountHealthAsync(
        [Description("The account ID")] int accountId,
        CancellationToken cancellationToken = default)
    {
        // Aggregates health score, recent interactions, open opportunities
        var details = await _accountService.GetAccountDetailsAsync(accountId, cancellationToken);
        return JsonSerializer.Serialize(details);
    }

    [KernelFunction("update_account")]
    [Description("Updates account fields. Requires human approval.")]
    [RequiresApproval]  // Custom attribute for human-in-the-loop
    public async Task<string> UpdateAccountAsync(
        [Description("The account ID")] int accountId,
        [Description("Fields to update as JSON")] string fieldsJson,
        CancellationToken cancellationToken = default)
    {
        // Update logic...
    }
}
```

### 9.2 Plugin-to-Service Mapping

| SK Plugin | CRM Service(s) | Capabilities |
|-----------|-----------------|--------------|
| `AccountPlugin` | `IAccountService` | Get, search, update, get health, get contacts |
| `ContactPlugin` | `IContactsService` | Get, search, create, update, get accounts |
| `OpportunityPlugin` | `IOpportunityService` | Get, search, update stage, get pipeline |
| `LeadPlugin` | `ILeadService` | Get, search, qualify, convert, score |
| `ServiceRequestPlugin` | `IServiceRequestService` | Get, create, update, assign, resolve, close |
| `EmailPlugin` | `INotificationPort` | Draft, send, list templates |
| `KnowledgeBasePlugin` | `ISearchPort`, KB service | Search articles, get by category |
| `SearchPlugin` | `ISearchPort` | Global search across entities |
| `CalendarPlugin` | `IActivityService` | Create meetings, check availability |
| `NotificationPlugin` | `INotificationPort` | Send in-app, email, SMS notifications |

### 9.3 Read vs Write Plugin Separation

For safety, plugins are split into **read** (no approval needed) and **write** (requires approval):

- **Read plugins** — Free for agents to call without human intervention
- **Write plugins** — Decorated with `[RequiresApproval]`, triggers human-in-the-loop flow

---

## 10. Memory & Vector Store

### 10.1 Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  SK Memory Abstractions                    │
│  ISemanticTextMemory / IMemoryStore                       │
└────────────────────────┬────────────────────────────────┘
                         │
            ┌────────────┼───────────────┐
            ▼            ▼               ▼
    ┌──────────────┐ ┌──────────────┐ ┌────────────────┐
    │   Qdrant     │ │  pgvector    │ │  In-Memory     │
    │  (Production)│ │  (MariaDB    │ │  (Dev/Test)    │
    │              │ │   extension) │ │                │
    └──────────────┘ └──────────────┘ └────────────────┘
```

### 10.2 Memory Collections

| Collection | Content | Purpose |
|------------|---------|---------|
| `crm-knowledge-articles` | KB article embeddings | RAG for support agent |
| `crm-account-profiles` | Account summary embeddings | Similarity search, account intelligence |
| `crm-interaction-history` | Interaction note embeddings | Context retrieval for agents |
| `crm-email-threads` | Email thread embeddings | Email assistant context |
| `crm-agent-memory-{agentId}` | Per-agent learned facts | Long-term agent memory |
| `crm-product-catalog` | Product description embeddings | Product recommendation |

### 10.3 Recommendation

- **Development/Test:** In-memory vector store (zero infrastructure)
- **Production (self-hosted):** Qdrant (OSS, Docker, purpose-built for vectors)
- **Production (cloud):** Azure AI Search or pgvector extension on existing DB

Add Qdrant to `docker-compose.providers.yml`:
```yaml
crm-qdrant:
  image: qdrant/qdrant:v1.8.0
  ports:
    - "6333:6333"
  volumes:
    - qdrant-data:/qdrant/storage
  networks:
    - crm-network
```

---

## 11. Human-in-the-Loop

### 11.1 Approval Filter

SK's `IAutoFunctionInvocationFilter` intercepts function calls before execution:

```csharp
public class HumanApprovalFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        // Check if function requires approval
        var requiresApproval = context.Function.Metadata
            .AdditionalProperties.ContainsKey("RequiresApproval");

        if (requiresApproval)
        {
            // Create approval request
            var approval = new AgentApprovalRequest
            {
                AgentId = context.ChatHistory.First().Content,
                FunctionName = context.Function.Name,
                Arguments = context.Arguments,
                Reasoning = "Agent wants to execute: " + context.Function.Description
            };

            // Persist to DB and notify via SignalR
            await _approvalService.CreateAsync(approval);
            await _notificationHub.SendApprovalRequest(approval);

            // Block execution until approved
            var result = await _approvalService.WaitForDecisionAsync(
                approval.Id, timeout: TimeSpan.FromHours(24));

            if (!result.Approved)
            {
                context.Result = new FunctionResult(context.Function,
                    "Action rejected by human reviewer: " + result.Reason);
                return; // Skip execution
            }
        }

        await next(context); // Execute the function
    }
}
```

### 11.2 Approval Tiers

| Action Type | Approval Required | Examples |
|-------------|-------------------|----------|
| **Read data** | None | Search accounts, get contact details |
| **Create activity** | None | Log notes, create tasks |
| **Send email** | Optional (configurable) | Drafts require review, follow-ups auto-send |
| **Update record** | Required | Change opportunity stage, update account |
| **Create record** | Required | Create new lead, create opportunity |
| **Delete/Archive** | Always Required | Close ticket, cancel subscription |
| **Financial** | Always Required | Approve quote, process payment |

---

## 12. Entity Model

### 12.1 New Entities for Agent Infrastructure

```csharp
// CRM.Core/Entities/AI/AIAgent.cs
public class AIAgent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;           // LeadScoring, Sales, Support, etc.
    public string SystemPrompt { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AllowedPlugins { get; set; }                 // JSON array of plugin names
    public string? Configuration { get; set; }                  // JSON agent-specific config
    public int MaxIterations { get; set; } = 10;               // Max reasoning steps
    public int ApprovalTier { get; set; } = 1;                 // 0=none, 1=writes, 2=all
    public string? ModelOverride { get; set; }                  // Use specific model
    public decimal? MaxTokenBudget { get; set; }               // Cost control per execution
}

// CRM.Core/Entities/AI/AgentConversation.cs
public class AgentConversation : BaseEntity
{
    public int AgentId { get; set; }
    public int? UserId { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Status { get; set; } = "Active";            // Active, Completed, Failed, Paused
    public string? Goal { get; set; }
    public int TotalSteps { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Outcome { get; set; }                       // JSON result summary

    public AIAgent Agent { get; set; } = null!;
    public ICollection<AgentAction> Actions { get; set; } = new List<AgentAction>();
}

// CRM.Core/Entities/AI/AgentAction.cs
public class AgentAction : BaseEntity
{
    public int ConversationId { get; set; }
    public int StepNumber { get; set; }
    public string ActionType { get; set; } = string.Empty;    // FunctionCall, Reasoning, Observation
    public string? FunctionName { get; set; }
    public string? Arguments { get; set; }                     // JSON
    public string? Result { get; set; }                        // JSON
    public string? Reasoning { get; set; }                     // LLM reasoning text
    public bool RequiredApproval { get; set; }
    public bool? WasApproved { get; set; }
    public int? ApprovedById { get; set; }
    public int TokensUsed { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? Error { get; set; }

    public AgentConversation Conversation { get; set; } = null!;
}

// CRM.Core/Entities/AI/AgentMemory.cs
public class AgentMemory : BaseEntity
{
    public int AgentId { get; set; }
    public string MemoryType { get; set; } = string.Empty;    // Fact, Preference, Context, Feedback
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public double Relevance { get; set; } = 1.0;
    public DateTime? ExpiresAt { get; set; }
    public string? Source { get; set; }                        // How the memory was learned

    public AIAgent Agent { get; set; } = null!;
}

// CRM.Core/Entities/AI/AgentApprovalRequest.cs
public class AgentApprovalRequest : BaseEntity
{
    public int ConversationId { get; set; }
    public int ActionId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? Reasoning { get; set; }
    public string Status { get; set; } = "Pending";           // Pending, Approved, Rejected, Expired
    public int? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewerNotes { get; set; }
    public DateTime ExpiresAt { get; set; }

    public AgentConversation Conversation { get; set; } = null!;
    public AgentAction Action { get; set; } = null!;
}
```

### 12.2 Database Tables

| Table | Purpose | Key Columns |
|-------|---------|-------------|
| `AIAgents` | Agent definitions and configuration | Name, Type, SystemPrompt, AllowedPlugins, MaxIterations |
| `AgentConversations` | Agent execution sessions | AgentId, UserId, EntityType, EntityId, Status, Goal, TotalTokens |
| `AgentActions` | Step-by-step execution log | ConversationId, StepNumber, ActionType, FunctionName, Reasoning |
| `AgentMemories` | Persistent agent knowledge | AgentId, MemoryType, Key, Value, Relevance, ExpiresAt |
| `AgentApprovalRequests` | Human-in-the-loop queue | ConversationId, FunctionName, Status, ReviewedById |

---

## 13. Risk Analysis

| # | Risk | Probability | Impact | Mitigation |
|---|------|-------------|--------|------------|
| 1 | **SK breaking changes** | Low | Medium | Pin NuGet versions, integration tests per upgrade |
| 2 | **LLM cost overrun** | Medium | High | MaxTokenBudget per agent, usage tracking via IAIPort, circuit breaker |
| 3 | **Agent hallucination** | Medium | High | Human-in-the-loop for writes, output validation plugins, RAG grounding |
| 4 | **Agent loop divergence** | Medium | Medium | MaxIterations limit (default 10), timeout per conversation |
| 5 | **Vector store latency** | Low | Medium | Qdrant benchmarked at <10ms, fallback to in-memory |
| 6 | **Plugin security** | Medium | High | Read/write separation, approval tiers, audit logging of all actions |
| 7 | **User trust** | Medium | High | Transparent reasoning display, easy override/undo, gradual rollout |
| 8 | **Migration to Agent Framework** | Low | Low | SK is the foundation of Agent Framework; smooth migration path |
| 9 | **.NET 10 upgrade interaction** | Low | Low | Resolved. Runtime baseline is now .NET 10; SK compatibility validated on the current platform. See [SK-INTEGRATION-PLAN.md §15](SK-INTEGRATION-PLAN.md#15-net-10-upgrade-sequencing). |

---

## 14. Consequences

### 14.1 Positive

- **Closes critical gaps** — Agentic AI score jumps from 18% to 65%
- **AI-Native Intelligence** — Lead scoring, next best action, churn prediction become autonomous
- **Competitive differentiation** — Few open-source CRMs have true agentic AI
- **Preserves architecture** — IAIPort and hexagonal pattern remain intact
- **Future-proof** — Natural migration path to Microsoft Agent Framework
- **Plugin reuse** — CRM services exposed as SK plugins can serve multiple agents
- **Memory/RAG** — Vector store enables semantic search across all CRM data

### 14.2 Negative

- **New dependency** — 5-8 new NuGet packages
- **Learning curve** — Team must learn SK concepts (Kernel, Plugins, Filters, Memory)
- **Infrastructure** — Vector store (Qdrant) adds one more container in production
- **Cost management** — Autonomous agents can consume more LLM tokens than manual calls
- **Testing complexity** — Agent behavior is non-deterministic, requires eval frameworks

### 14.3 Neutral

- **No IAIPort changes** — Existing providers work unchanged
- **Opt-in adoption** — Agents can be enabled/disabled per deployment via feature flags
- **Existing services preserved** — Current `AILeadScoringService` and `AIChatbotController` continue to work

---

## 15. NuGet Packages

### 15.1 Core Packages (Required)

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.SemanticKernel` | 1.x (latest stable) | Core kernel, plugins, function calling |
| `Microsoft.SemanticKernel.Abstractions` | 1.x | Interfaces and abstractions |
| `Microsoft.SemanticKernel.Connectors.Qdrant` | 1.x | Qdrant vector store connector |
| `Microsoft.SemanticKernel.Plugins.Core` | 1.x-alpha | Built-in utility plugins (Time, Math, Text) |

### 15.2 Optional Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.SemanticKernel.Connectors.Postgres` | 1.x | pgvector alternative to Qdrant |
| `Microsoft.SemanticKernel.Connectors.AzureAISearch` | 1.x | Azure AI Search for cloud deployments |
| `Microsoft.SemanticKernel.Planners.Handlebars` | 1.x-alpha | Advanced planning (if needed beyond auto function calling) |
| `Microsoft.Extensions.VectorData.Abstractions` | 9.x | New unified vector data abstractions |

---

## 16. References

| Resource | URL |
|----------|-----|
| Semantic Kernel GitHub | https://github.com/microsoft/semantic-kernel |
| SK .NET Documentation | https://learn.microsoft.com/semantic-kernel/overview |
| SK Concepts - Plugins | https://learn.microsoft.com/semantic-kernel/concepts/plugins |
| SK Concepts - Memory | https://learn.microsoft.com/semantic-kernel/concepts/vector-store-connectors |
| SK Concepts - Agents | https://learn.microsoft.com/semantic-kernel/concepts/agents |
| SK Concepts - Process | https://learn.microsoft.com/semantic-kernel/concepts/process-framework |
| SK Auto Function Calling | https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/function-calling |
| ADR-001 Pluggable Architecture | [ADR-001-Pluggable-Architecture-Strategy.md](ADR-001-Pluggable-Architecture-Strategy.md) |
| Best-in-Class Gap Analysis | [ROADMAP_BEST_IN_CLASS.md](../ROADMAP_BEST_IN_CLASS.md) |
| Existing IAIPort Interface | `CRM.Core/Ports/Output/Providers/IAIPort.cs` |

---

## Appendix: Feature Flag Integration

Agents are controlled via the existing feature flag system (per ADR-001):

```csharp
// CRM.Core/Features/FeatureFlags.cs (additions)
public const string EnableAgenticAI = "EnableAgenticAI";
public const string EnableLeadScoringAgent = "EnableLeadScoringAgent";
public const string EnableSalesAgent = "EnableSalesAgent";
public const string EnableSupportAgent = "EnableSupportAgent";
public const string EnableCustomerSuccessAgent = "EnableCustomerSuccessAgent";
public const string EnableEmailAssistantAgent = "EnableEmailAssistantAgent";
public const string EnableRevenueIntelAgent = "EnableRevenueIntelAgent";
public const string EnableDocumentIntelAgent = "EnableDocumentIntelAgent";
```

```json
// appsettings.json
{
  "FeatureManagement": {
    "EnableAgenticAI": false,
    "EnableLeadScoringAgent": false,
    "EnableSalesAgent": false,
    "EnableSupportAgent": false
  }
}
```

---

**END OF ADR-004**

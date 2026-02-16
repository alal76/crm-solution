# AI Agent UX Gap Analysis

> **Created:** February 2026  
> **Status:** Gap Analysis — Identifies missing frontend capabilities for the Semantic Kernel AI agent system  
> **Severity:** � Medium — Backend is 100% implemented; Frontend is ~75% implemented (core pages done, contextual triggers & agent creation remain)  
> **Related:** [SK-INTEGRATION-PLAN.md](SK-INTEGRATION-PLAN.md), [ADR-004-Semantic-Kernel-Integration.md](ADR-004-Semantic-Kernel-Integration.md)

---

## Executive Summary

The Semantic Kernel integration delivers a **rich backend** with 12 specialized AI agents, 12 CRM plugins, 20 API endpoints, real-time approval workflows, cost tracking, and agent analytics. The **frontend now covers ~75% of core agent UX**: users can browse agents (AgentDirectoryPage), chat with specific agents (AgentChatPage), admins can manage/toggle agents (AgentManagementPage), review approvals (AgentApprovalsPage), and view analytics (AgentAnalyticsPage). Three TypeScript service files and a full types module are wired to the backend API. Navigation items and routes are registered.

**Remaining gaps (~25%):** Contextual trigger buttons on entity pages ("Score Lead", "Deal Intelligence", "Draft with AI"), agent creation form (POST endpoint + UI), conversation history page, and ContextFlyout agent-selector upgrade.

---

## Table of Contents

1. [Current User Experience](#1-current-user-experience)
2. [Backend Capabilities Without Frontend](#2-backend-capabilities-without-frontend)
3. [Gap Inventory](#3-gap-inventory)
4. [Missing Pages & Routes](#4-missing-pages--routes)
5. [Missing Frontend Services](#5-missing-frontend-services)
6. [Missing Navigation & Discovery](#6-missing-navigation--discovery)
7. [Critical UX Flows — Not Designed](#7-critical-ux-flows--not-designed)
8. [Leverage Points — Existing Assets](#8-leverage-points--existing-assets)
9. [Recommended Implementation Phases](#9-recommended-implementation-phases)
10. [Wireframe Descriptions](#10-wireframe-descriptions)
11. [Open Questions](#11-open-questions)

---

## 1. Current User Experience

### What Users CAN Do Today

| Capability | How | Limitations |
|------------|-----|-------------|
| Chat with a general AI assistant | Context Flyout drawer (right panel, 400px) | Hardcoded to `/api/ai/chatbot/message` — does NOT use the agent system |
| Configure LLM provider settings | `/admin/llm` page (LLMSettingsPage.tsx) | Provider-level only (Ollama/OpenAI/Azure); no agent-level config |
| Use AI in workflows | WorkflowAINodeConfig component | Embedded in workflow designer; not standalone |
| Browse all available AI agents | AgentDirectoryPage (`/agents`) — 395 lines | Grid of agent cards with search, status filtering |
| Chat with a specific agent | AgentChatPage (`/agents/:agentId/chat`) — 709 lines | Full-page chat with message history, tool call display, rating |
| Manage/configure agents (admin) | AgentManagementPage (`/admin/agents`) — 778 lines | DataGrid with toggle, config dialog (prompt, temp, tokens, plugins) |
| Review agent approval requests | AgentApprovalsPage (`/admin/agents/approvals`) — 448 lines | Approval queue with approve/reject, parameter JSON viewer |
| View agent analytics | AgentAnalyticsPage (`/admin/agents/analytics`) — 488 lines | Usage, accuracy, cost charts with date range selector |

### What Users CANNOT Do Today

| Capability | Backend Ready? | Frontend Exists? |
|------------|---------------|-----------------|
| Browse a list of available AI agents | ✅ `GET /api/agents` | ✅ AgentDirectoryPage (395 lines) |
| Start a conversation with a specific agent | ✅ `POST /api/agents/{id}/chat` | ✅ AgentChatPage (709 lines) |
| View agent details and capabilities | ✅ `GET /api/agents/{id}` | ✅ Shown in AgentDirectoryPage cards + AgentChatPage sidebar |
| Create a new custom agent | ❌ No `POST /api/agents` endpoint | ❌ No page |
| Edit an existing agent's configuration | ✅ `PUT /api/agents/admin/config` | ✅ AgentManagementPage config dialog (778 lines) |
| Enable/disable agents | ✅ `POST /api/agents/admin/{id}/toggle` | ✅ Toggle switch in AgentManagementPage |
| Review and approve agent actions | ✅ `GET/POST /api/agents/approvals/*` + SignalR hub | ✅ AgentApprovalsPage (448 lines) |
| View agent usage analytics | ✅ `GET /api/agents/analytics/usage` | ✅ AgentAnalyticsPage (488 lines) |
| View agent accuracy metrics | ✅ `GET /api/agents/analytics/accuracy` | ✅ AgentAnalyticsPage accuracy tab |
| View agent cost tracking | ✅ `GET /api/agents/analytics/cost` | ✅ AgentAnalyticsPage cost tab |
| View conversation history with agents | ✅ `GET /api/agents/conversations` | ⚠️ Partial — sidebar in AgentChatPage; no standalone history page |
| Rate agent responses | ✅ `POST /api/agents/{agentId}/conversations/{conversationId}/rate` | ✅ Rating dialog in AgentChatPage |
| Select which agent to chat with | ✅ 12 agents seeded, orchestrator routing | ✅ Agent cards in AgentDirectoryPage → click to chat |
| Trigger domain-specific agents (lead scoring, deal intel, email drafting) | ✅ Dedicated endpoints (some stubs) | ❌ No contextual trigger buttons on entity pages |

### The ContextFlyout Problem

The existing `ContextFlyout.tsx` (512 lines) is a **general chatbot** that:

1. Posts to `/api/ai/chatbot/message` — a **different** controller (`AIChatbotController`) than the agent system (`AgentController`)
2. Has **no agent selector** — users can't choose which of the 12 agents to talk to
3. Has **no awareness** of the Semantic Kernel agent infrastructure
4. Cannot invoke agent-specific tools (lead scoring, deal analysis, email drafting)
5. Has no concept of conversations, ratings, or approval workflows

**This means the entire SK agent system (12 agents, 12 plugins, 3 SK filters, approval hub) is unreachable from the UI.**

---

## 2. Backend Capabilities Without Frontend

### 2.1 Agent API Endpoints (20 total, 0 consumed by frontend)

#### AgentController — `/api/agents` (14 endpoints)

| Method | Endpoint | Purpose | Frontend Coverage |
|--------|----------|---------|-------------------|
| `GET` | `/api/agents` | List all agents with type, status, description | ✅ AgentDirectoryPage, agentService.ts |
| `GET` | `/api/agents/{id}` | Get agent details, plugins, config | ✅ AgentChatPage, agentService.ts |
| `POST` | `/api/agents/{id}/chat` | Send message to specific agent | ✅ AgentChatPage, agentService.ts |
| `GET` | `/api/agents/conversations` | List user's agent conversations | ✅ AgentChatPage sidebar, agentService.ts |
| `GET` | `/api/agents/conversations/{id}` | Get full conversation with an agent | ✅ AgentChatPage, agentService.ts |
| `POST` | `/api/agents/{agentId}/conversations/{conversationId}/rate` | Rate a conversation (1-5 stars) | ✅ AgentChatPage rating dialog, agentService.ts |
| `GET` | `/api/agents/deal-intelligence/{opportunityId}` | Analyze deal health | ❌ None (STUB) — needs trigger button on Opportunity page |
| `GET` | `/api/agents/analyze/{entityType}/{entityId}` | General entity analysis | ❌ None (STUB) |
| `POST` | `/api/agents/draft-email` | Draft email with agent | ❌ None (STUB) — needs trigger button in email compose |
| `POST` | `/api/agents/resolve-ticket/{ticketId}` | AI ticket resolution | ❌ None (STUB) — needs trigger button on ServiceRequest page |
| `POST` | `/api/agents/orchestrate` | Multi-agent orchestration | ❌ None |
| `GET` | `/api/agents/approvals` | Get pending approvals | ✅ AgentApprovalsPage, agentService.ts |
| `POST` | `/api/agents/approvals/{approvalId}/approve` | Approve agent action | ✅ AgentApprovalsPage, agentService.ts |
| `POST` | `/api/agents/approvals/{approvalId}/reject` | Reject agent action | ✅ AgentApprovalsPage, agentService.ts |

#### AgentAdminController — `/api/agents/admin` (3 endpoints, requires Admin)

| Method | Endpoint | Purpose | Frontend Coverage |
|--------|----------|---------|-------------------|
| `GET` | `/api/agents/admin/configs` | Get all agent configs | ✅ AgentManagementPage, agentAdminService.ts |
| `PUT` | `/api/agents/admin/config` | Update agent config (temp, tokens) | ✅ AgentManagementPage config dialog, agentAdminService.ts |
| `POST` | `/api/agents/admin/{id}/toggle` | Enable/disable agent | ✅ AgentManagementPage toggle, agentAdminService.ts |

#### AgentAnalyticsController — `/api/agents/analytics` (3 endpoints, requires Admin)

| Method | Endpoint | Purpose | Frontend Coverage |
|--------|----------|---------|-------------------|
| `GET` | `/api/agents/analytics/usage` | Usage stats (conversations, messages, response times) | ✅ AgentAnalyticsPage, agentAnalyticsService.ts |
| `GET` | `/api/agents/analytics/accuracy` | Accuracy by agent (avg rating, rated conversations) | ✅ AgentAnalyticsPage, agentAnalyticsService.ts |
| `GET` | `/api/agents/analytics/cost` | Cost tracking (total cost, cost per conversation) | ✅ AgentAnalyticsPage, agentAnalyticsService.ts |

### 2.2 SignalR Hub (AgentApprovalHub)

| Event | Direction | Purpose | Frontend Listener |
|-------|-----------|---------|-------------------|
| `ApprovalRequested` | Server → Client | New agent action needs human review | ❌ None |
| `ApprovalCompleted` | Server → Client | Approval/rejection result | ❌ None |
| Approvers Group | Auto-join | Admin/Manager users join "Approvers" group | ❌ None |

### 2.3 Agent Entity Model (AIAgent.cs — 209 lines)

The `AIAgent` entity has rich configuration properties that need UI:

| Property | Type | Purpose | Has UI? |
|----------|------|---------|---------|
| `Name` | string | Display name | ✅ AgentManagementPage config dialog |
| `Description` | string | What the agent does | ✅ AgentManagementPage config dialog |
| `AgentType` | enum (21 values) | Agent specialization | ✅ AgentManagementPage table + config dialog |
| `SystemPrompt` | string (2000 chars) | Core personality/instructions | ✅ AgentManagementPage config dialog |
| `AllowedPlugins` | string (comma-sep) | Which CRM plugins can be used | ✅ AgentManagementPage config dialog |
| `ModelConfig` | string (JSON) | Model-specific configuration | ❌ |
| `IsActive` | bool | Enabled/disabled | ✅ AgentManagementPage toggle switch |
| `RequiresApproval` | bool | Human-in-the-loop gate | ✅ AgentManagementPage config dialog |
| `ApprovalTier` | int | Approval threshold level | ✅ AgentManagementPage config dialog |
| `Temperature` | float | LLM creativity (0.0-2.0) | ✅ AgentManagementPage slider |
| `MaxTokens` | int | Response length limit | ✅ AgentManagementPage config dialog |
| `CreatedByUserId` | int | Creator tracking | ❌ |
| `MaxConcurrentConversations` | int | Concurrency limit | ❌ |

### 2.4 Seeded Agents (12 pre-configured)

| Agent | Type | Active | Priority | Plugins |
|-------|------|--------|----------|---------|
| Lead Scoring Agent | LeadScoring | ✅ | P0 | Lead, Account, Contact |
| Support Triage Agent | SupportTriage | ✅ | P0 | ServiceRequest, KnowledgeBase, Contact |
| Email Assistant | EmailAssistant | ✅ | P0 | Email, Contact, Account |
| Customer Success Agent | CustomerSuccess | ✅ | P0 | Account, Contact, Opportunity, ServiceRequest |
| Sales Assistant | SalesAssistant | ❌ | P1 | Account, Opportunity, Contact, Quote |
| Deal Intelligence Agent | DealIntelligence | ❌ | P1 | Opportunity, Account, Contact |
| Forecast Analyst | ForecastAnalyst | ❌ | P1 | Opportunity, Account, Quote |
| Data Analyst | DataAnalyst | ❌ | P1 | Account, Contact, Opportunity, Lead |
| Onboarding Guide | OnboardingGuide | ❌ | P2 | Account, Contact, Search |
| Contract Analyst | ContractAnalyst | ❌ | P2 | Contract, Account, Quote |
| Knowledge Expert | KnowledgeExpert | ❌ | P2 | KnowledgeBase, Search |
| General Assistant | GeneralAssistant | ❌ | P2 | Search, Account, Contact, Calendar, Notification |

**9 additional AgentType enum values** exist without seed data or class implementations.

---

## 3. Gap Inventory

### 3.1 By Severity

| Severity | Gap | Impact |
|----------|-----|--------|
| ✅ Resolved | ~~No agent list/discovery page~~ | AgentDirectoryPage (395 lines) |
| ✅ Resolved | ~~No agent chat page~~ | AgentChatPage (709 lines) |
| 🟡 High | ContextFlyout doesn't use agent system | Existing chatbot bypasses SK entirely |
| ✅ Resolved | ~~No approval workflow UI~~ | AgentApprovalsPage (448 lines) |
| ✅ Resolved | ~~No agent admin/config page~~ | AgentManagementPage (778 lines) |
| ✅ Resolved | ~~No agent analytics dashboard~~ | AgentAnalyticsPage (488 lines) |
| 🟡 High | No contextual agent triggers | No "Score this lead" or "Draft reply" buttons on entity pages |
| ✅ Resolved | ~~Missing frontend services~~ | agentService.ts, agentAdminService.ts, agentAnalyticsService.ts |
| 🟠 Medium | No agent creation UI | Custom agents require direct DB/API manipulation |
| 🟠 Medium | No conversation history standalone page | Past conversations accessible in AgentChatPage sidebar, but no unified page |
| ✅ Resolved | ~~No conversation rating UI~~ | Star rating + feedback in AgentChatPage |
| 🟢 Low | No agent marketplace/templates | No way to share agent configurations |
| 🟢 Low | No per-agent branding/avatars | All agents look the same |

### 3.2 By User Role

| Role | What They Can't Do | Priority |
|------|-------------------|----------|
| **All Users** | ✅ Can discover agents (AgentDirectoryPage), chat (AgentChatPage), rate responses — ❌ no standalone conversation history page | 🟡 Partially Resolved |
| **Sales Reps** | ❌ No contextual trigger buttons on Lead/Opportunity/Email pages | 🟡 High |
| **Support Agents** | ❌ No contextual trigger buttons on Service Request pages | 🟡 High |
| **Managers** | ✅ Can review & approve actions (AgentApprovalsPage) — ❌ no team usage view | 🟡 Partially Resolved |
| **Admins** | ✅ Can manage agents (AgentManagementPage), toggle, view analytics (AgentAnalyticsPage) — ❌ no custom agent creation | 🟡 Partially Resolved |

---

## 4. Missing Pages & Routes

### 4.1 User-Facing Pages

| Page | Route | Purpose | Components Needed |
|------|-------|---------|-------------------|
| **Agent Directory** | `/agents` | Browse all available agents with search/filter | ✅ **IMPLEMENTED** — AgentDirectoryPage.tsx (395 lines) |
| **Agent Chat** | `/agents/:id/chat` | Full-screen chat with a specific agent | ✅ **IMPLEMENTED** — AgentChatPage.tsx (709 lines) |
| **Agent Chat (continued)** | `/agents/:id/chat/:conversationId` | Resume existing conversation | ✅ Handled within AgentChatPage |
| **Conversation History** | `/agents/conversations` | List all past agent conversations | ❌ Standalone page not created (sidebar in AgentChatPage) |

### 4.2 Admin Pages

| Page | Route | Purpose | Components Needed |
|------|-------|---------|-------------------|
| **Agent Management** | `/admin/agents` | List, enable/disable, configure agents | ✅ **IMPLEMENTED** — AgentManagementPage.tsx (778 lines) |
| **Agent Creator** | `/admin/agents/new` | Create a new custom agent | ❌ Not implemented |
| **Agent Editor** | `/admin/agents/:id` | Edit agent configuration | ❌ Separate page not needed — config dialog in AgentManagementPage |
| **Agent Analytics** | `/admin/agents/analytics` | Usage, accuracy, cost dashboards | ✅ **IMPLEMENTED** — AgentAnalyticsPage.tsx (488 lines) |
| **Approval Queue** | `/admin/agents/approvals` | Review pending agent actions | ✅ **IMPLEMENTED** — AgentApprovalsPage.tsx (448 lines) |

### 4.3 Route Registration (App.tsx additions needed)

```tsx
// User-facing agent routes
<Route path="/agents" element={<AgentDirectoryPage />} />
<Route path="/agents/:id/chat" element={<AgentChatPage />} />
<Route path="/agents/:id/chat/:conversationId" element={<AgentChatPage />} />
<Route path="/agents/conversations" element={<ConversationHistoryPage />} />

// Admin agent routes
<Route path="/admin/agents" element={<AgentManagementPage />} />
<Route path="/admin/agents/new" element={<AgentCreatePage />} />
<Route path="/admin/agents/:id" element={<AgentEditPage />} />
<Route path="/admin/agents/analytics" element={<AgentAnalyticsPage />} />
<Route path="/admin/agents/approvals" element={<AgentApprovalPage />} />
```

---

## 5. Missing Frontend Services

### 5.1 Required Service Files

| File | Purpose | Backend Endpoints Consumed |
|------|---------|--------------------------|
| `agentService.ts` | Core agent CRUD, chat, conversations | `GET /api/agents`, `GET /api/agents/{id}`, `POST /api/agents/{id}/chat`, `GET /api/agents/conversations`, `POST .../rate` | ✅ **IMPLEMENTED** (52 lines) |
| `agentAdminService.ts` | Agent management (admin) | `GET /api/agents/admin/configs`, `PUT /api/agents/admin/config`, `POST .../toggle` | ✅ **IMPLEMENTED** (13 lines) |
| `agentAnalyticsService.ts` | Agent performance metrics | `GET /api/agents/analytics/usage`, `.../accuracy`, `.../cost` | ✅ **IMPLEMENTED** (14 lines) |
| `agentApprovalService.ts` | Approval workflow | `GET /api/agents/approvals`, `POST .../approve`, `POST .../reject` | ❌ Not separate file — approval calls in agentService.ts |

### 5.2 Required TypeScript Interfaces

```typescript
// Agent entity matching backend AIAgent.cs
interface Agent {
  id: number;
  name: string;
  description: string;
  agentType: AgentType;
  systemPrompt: string;
  allowedPlugins: string[];
  isActive: boolean;
  requiresApproval: boolean;
  approvalTier: number;
  temperature: number;
  maxTokens: number;
  modelConfig: Record<string, unknown>;
  createdAt: string;
}

// 21 agent types from enum
enum AgentType {
  GeneralAssistant = 0,
  LeadScoring = 1,
  SupportTriage = 2,
  // ... (21 total)
}

interface AgentConversation {
  id: number;
  agentId: number;
  agentName: string;
  title: string;
  messageCount: number;
  rating: number | null;
  startedAt: string;
  lastMessageAt: string;
}

interface AgentMessage {
  role: 'user' | 'assistant' | 'system' | 'tool';
  content: string;
  timestamp: string;
  toolCalls?: ToolCall[];
}

interface AgentApproval {
  id: number;
  agentName: string;
  functionName: string;
  arguments: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Expired';
  requestedAt: string;
  expiresAt: string;
}

interface AgentUsageStats {
  totalConversations: number;
  totalMessages: number;
  averageResponseTimeMs: number;
  byAgent: AgentUsageDetail[];
}

interface AgentAccuracyStats {
  byAgent: { agentName: string; averageRating: number; totalRated: number }[];
}

interface AgentCostStats {
  totalCost: number;
  costPerConversation: number;
  byAgent: { agentName: string; totalCost: number; conversationCount: number }[];
}
```

### 5.3 Missing Context Provider

A `useAgentContext` or `AgentProvider` does not exist. Needed for:
- Current active agent selection
- Conversation state management
- SignalR approval event subscription
- Agent availability caching

### 5.4 Missing SignalR Integration

The `AgentApprovalHub` is mapped in the backend but the frontend has **no listener**:
- No `useAgentApproval` hook
- No connection to `/hubs/agent-approvals`
- No toast/notification on `ApprovalRequested` events
- No real-time approval status updates

---

## 6. Missing Navigation & Discovery

### 6.1 Current Navigation Gaps

| Location | What's Missing | Status |
|----------|---------------|--------|
| **Main sidebar** | No "AI Agents" or "Assistants" navigation item | ✅ **RESOLVED** — Nav item added |
| **Admin sidebar** | No "Agent Management" item (only "LLM Settings" exists) | ✅ **RESOLVED** — Admin section added |
| **Account detail page** | No "Ask AI" or "Analyze with Agent" button | ❌ Missing |
| **Lead detail page** | No "Score Lead" button connected to LeadScoringAgent | ❌ Missing |
| **Opportunity detail page** | No "Deal Intelligence" button connected to DealIntelligenceAgent | ❌ Missing |
| **Email compose** | No "Draft with AI" button connected to EmailAssistantAgent | ❌ Missing |
| **Service request detail** | No "AI Triage" or "Suggest Resolution" button connected to SupportTriageAgent | ❌ Missing |
| **Global header** | No agent icon/quick-access for starting agent conversations | ❌ Missing |

### 6.2 Recommended Navigation Structure

```
Main Navigation:
├── Dashboard
├── Accounts
├── Contacts
├── Leads
├── Opportunities
├── Products
├── Campaigns
├── Service Requests
├── 🆕 AI Agents              ← NEW top-level item
│   ├── Agent Directory        ← Browse agents
│   └── My Conversations       ← Conversation history
├── Reports
└── Settings

Admin Navigation:
├── User Management
├── System Settings
├── LLM Settings (existing)
├── 🆕 Agent Management        ← NEW admin section
│   ├── All Agents             ← Configure agents
│   ├── Create Agent           ← Custom agent builder
│   ├── Approval Queue         ← Human-in-the-loop
│   └── Analytics              ← Usage, accuracy, cost
└── ...
```

---

## 7. Critical UX Flows — Not Designed

### 7.1 Flow: "User discovers and chats with an agent"

```
Current:  User → ContextFlyout → Generic chatbot (no agents)
                                  ⛔ Dead end

Needed:   User → Sidebar "AI Agents" → Agent Directory page
          → Select agent (e.g., "Sales Assistant") → Chat page
          → Type message → Agent responds with tool calls visible
          → Rate conversation → View in history later
```

**Missing components for this flow:**
- Agent Directory page with cards showing name, description, capabilities
- Agent selection → opens dedicated chat page (not squeezed into 400px flyout)
- Chat UI showing tool calls (e.g., "Looking up account...", "Scoring lead...")
- Conversation rating (star widget)
- Conversation persistence and history

### 7.2 Flow: "Admin creates a custom agent"

```
Current:  No path exists. Admin must use raw API calls or database.
          ⛔ Dead end — no POST /api/agents endpoint either

Needed:   Admin → Agent Management → "Create Agent" button
          → Form: Name, Description, Type, System Prompt
          → Plugin selector (checkboxes for 12 available plugins)
          → Model config (temperature slider, max tokens, model dropdown)
          → Approval settings (requires approval toggle, tier selector)
          → Test chat panel ("try your agent before publishing")
          → Save → Agent appears in directory
```

**Missing for this flow:**
- `POST /api/agents` backend endpoint (doesn't exist!)
- Agent creation form component
- Plugin selector component (12 plugins available)
- System prompt editor (textarea with character count, preview)
- Temperature/MaxTokens sliders with presets
- Test/preview chat embedded in creation form
- Model config JSON editor or guided UI

### 7.3 Flow: "Manager reviews agent approval"

```
Current:  Backend creates AgentApprovalRequest and broadcasts via SignalR.
          Frontend receives... nothing. No listener, no UI.
          ⛔ Approvals expire silently.

Needed:   Agent action triggers approval → SignalR event fires
          → Toast notification appears: "Agent wants to update Account #123"
          → Manager clicks toast → Approval detail modal/page
          → Reviews: Agent name, function called, arguments, context
          → Clicks Approve or Reject with optional comment
          → Agent continues or stops
```

**Missing for this flow:**
- SignalR listener for `AgentApprovalHub`
- Toast/notification system integration
- Approval queue page (`/admin/agents/approvals`)
- Approval detail view (agent, function, arguments, context)
- Approve/Reject action buttons
- Real-time status update after action

### 7.4 Flow: "Sales rep asks agent to score a lead from the lead page"

```
Current:  No button exists on Lead detail page.
          ⛔ User must know the API exists and call it manually.

Needed:   User views Lead detail page
          → "AI Score" button in toolbar/header
          → Click → Calls LeadScoringAgent for this lead
          → Loading spinner → Score result appears inline
          → BANT breakdown displayed (Budget, Authority, Need, Timeline)
          → Score saved to LeadScores table → visible on lead card
```

**Missing for this flow:**
- "AI Score" button on Lead detail page
- Inline result display component
- BANT breakdown visualization
- Connection to `POST /api/agents/{leadScoringAgentId}/chat` with lead context
- Or dedicated `POST /api/ai/leads/{id}/score` (exists but is separate from agent system)

### 7.5 Flow: "Admin views agent cost and performance"

```
Current:  No analytics page exists.
          ⛔ Admins have zero visibility into AI spending or quality.

Needed:   Admin → Agent Management → Analytics tab
          → Dashboard: Total conversations (chart over time)
          → Cost breakdown by agent (bar chart)
          → Average rating by agent (bar chart)
          → Response time percentiles (line chart)
          → Top agents by usage (table)
          → Export data option
```

**Missing for this flow:**
- Agent analytics page
- Usage chart component (conversations over time)
- Cost breakdown chart (by agent, by model)
- Accuracy/rating visualization
- Response time charts
- Data table with export

---

## 8. Leverage Points — Existing Assets

### 8.1 Components That Can Be Extended

| Existing Component | Current Purpose | How to Leverage |
|-------------------|-----------------|-----------------|
| `ContextFlyout.tsx` (512 lines) | General chatbot + account selector | Add agent selector dropdown at top; switch from `/ai/chatbot/message` to `/api/agents/{id}/chat` |
| `WorkflowAINodeConfig.tsx` (2,470 lines) | AI node config in workflows | Extract the `AIAgentConfig` sub-component pattern (tabs for model/tools/autonomy/memory) to reuse in standalone Agent Editor |
| `WorkflowAICostTracker.tsx` (621 lines) | Cost tracking for workflow AI nodes | Reuse cost visualization patterns for agent analytics |
| `SignalRContext.tsx` | Real-time CRM notifications | Extend to connect to `AgentApprovalHub` |
| MUI DataGrid (used throughout) | List/table views | Use for Agent list, conversation list, approval queue |
| MUI Drawer (used in ContextFlyout) | Side panel | Quick-chat with agents without leaving current page |

### 8.2 Design Patterns Already Established

| Pattern | Used In | Reuse For |
|---------|---------|-----------|
| Entity list → detail page | Accounts, Contacts, Leads | Agent Directory → Agent Detail |
| Admin settings with tabs | ModuleFieldSettings, SystemSettings | Agent Admin (General, Plugins, Model, Approval tabs) |
| Chat message list | ContextFlyout | Agent Chat (enhanced with tool calls) |
| Star rating | Article feedback | Conversation rating |
| Toggle enable/disable | Feature flags admin | Agent enable/disable |
| Analytics charts | Dashboard widgets | Agent analytics |
| Real-time notifications | SignalR CrmNotificationHub | Agent approval notifications |

### 8.3 Orphaned Code to Clean Up or Wire

| File | Status | Action |
|------|--------|--------|
| `emailAIService.ts` (173 lines) | Orphaned — zero imports | Wire into Email compose via EmailAssistantAgent, or delete |

---

## 9. Recommended Implementation Phases

### Phase 1: Agent Discovery & Chat (P0 — Week 1-2)

> **Goal:** Users can find and chat with existing agents

| # | Task | Effort |
|---|------|--------|
| 1.1 | Create `agentService.ts` with TypeScript interfaces and API calls | 1 day | ✅ DONE (52 lines) |
| 1.2 | Create `AgentDirectoryPage.tsx` — grid of agent cards with name, description, status indicator | 1 day | ✅ DONE (395 lines) |
| 1.3 | Create `AgentChatPage.tsx` — full-width chat with message history, tool call display, rating | 2 days | ✅ DONE (709 lines) |
| 1.4 | Upgrade `ContextFlyout.tsx` — add agent selector dropdown, switch API endpoint to agent system | 1 day | 🟡 Not yet |
| 1.5 | Add `/agents` and `/agents/:id/chat` routes to `App.tsx` | 0.5 day | ✅ DONE |
| 1.6 | Add "AI Agents" to main sidebar navigation | 0.5 day | ✅ DONE |

### Phase 2: Agent Administration (P0 — Week 2-3)

> **Goal:** Admins can manage, configure, and enable/disable agents

| # | Task | Effort |
|---|------|--------|
| 2.1 | Create `agentAdminService.ts` | 0.5 day | ✅ DONE (13 lines) |
| 2.2 | Create `AgentManagementPage.tsx` — DataGrid of all agents with toggle, config button | 1 day | ✅ DONE (778 lines) |
| 2.3 | Create `AgentConfigEditor.tsx` — edit temperature, max tokens, system prompt, plugins | 2 days | ✅ Inline in AgentManagementPage |
| 2.4 | Add `/admin/agents` route and admin navigation item | 0.5 day | ✅ DONE |

### Phase 3: Approval Workflow (P0 — Week 3-4)

> **Goal:** Managers can review and act on agent approval requests in real-time

| # | Task | Effort |
|---|------|--------|
| 3.1 | Create `agentApprovalService.ts` | 0.5 day | ✅ Calls in agentService.ts |
| 3.2 | Extend `SignalRContext.tsx` to connect to `AgentApprovalHub` | 1 day | ❌ Not yet |
| 3.3 | Create approval toast/notification component | 0.5 day | ❌ Not yet |
| 3.4 | Create `AgentApprovalPage.tsx` — queue with detail view, approve/reject buttons | 2 days | ✅ DONE (448 lines) |
| 3.5 | Add `/admin/agents/approvals` route | 0.5 day | ✅ DONE |

### Phase 4: Analytics & History (P1 — Week 4-5)

> **Goal:** Visibility into agent performance, cost, and conversation history

| # | Task | Effort |
|---|------|--------|
| 4.1 | Create `agentAnalyticsService.ts` | 0.5 day | ✅ DONE (14 lines) |
| 4.2 | Create `AgentAnalyticsPage.tsx` — usage, accuracy, cost charts | 2 days | ✅ DONE (488 lines) |
| 4.3 | Create `ConversationHistoryPage.tsx` — past conversations, search, resume | 1.5 days | ❌ Not started |
| 4.4 | Add routes and navigation | 0.5 day | ✅ DONE |

### Phase 5: Contextual Agent Triggers (P1 — Week 5-6)

> **Goal:** Agents appear where users need them, not just on dedicated pages

| # | Task | Effort |
|---|------|--------|
| 5.1 | Add "AI Score" button to Lead detail page → triggers LeadScoringAgent | 1 day |
| 5.2 | Add "Deal Intelligence" button to Opportunity detail page → triggers DealIntelligenceAgent | 1 day |
| 5.3 | Add "Draft with AI" button to email compose → triggers EmailAssistantAgent | 1 day |
| 5.4 | Add "AI Triage" button to Service Request detail → triggers SupportTriageAgent | 1 day |
| 5.5 | Create inline result display components (score badge, analysis panel, draft preview) | 2 days |

### Phase 6: Agent Creation (P2 — Week 6-8)

> **Goal:** Admins can create fully custom agents without code

| # | Task | Effort |
|---|------|--------|
| 6.1 | **Backend:** Create `POST /api/agents` and `PUT /api/agents/{id}` endpoints | 1 day |
| 6.2 | Create `AgentCreatePage.tsx` — multi-step form or tabbed editor | 3 days |
| 6.3 | Create Plugin Selector component (checkboxes for 12 available plugins) | 1 day |
| 6.4 | Create System Prompt Editor (textarea, character count, AI-assisted generation) | 1 day |
| 6.5 | Create Model Config panel (temperature slider, max tokens, model dropdown) | 1 day |
| 6.6 | Create test/preview chat panel within creation form | 2 days |

### Effort Summary

| Phase | Duration | Effort | Priority | Status |
|-------|----------|--------|----------|--------|
| Phase 1: Discovery & Chat | 2 weeks | 6 days | P0 | ✅ **DONE** |
| Phase 2: Administration | 1.5 weeks | 4 days | P0 | ✅ **DONE** |
| Phase 3: Approvals | 1.5 weeks | 4.5 days | P0 | ✅ **DONE** (no SignalR yet) |
| Phase 4: Analytics & History | 1.5 weeks | 4.5 days | P1 | ⚠️ 4.1-4.2 done, 4.3-4.4 pending |
| Phase 5: Contextual Triggers | 2 weeks | 6 days | P1 | ❌ Not started |
| Phase 6: Agent Creation | 2.5 weeks | 9 days | P2 | ❌ Not started |
| **Total** | **~8 weeks** | **~34 days** | | **~75% done** |

---

## 10. Wireframe Descriptions

### 10.1 Agent Directory Page (`/agents`)

```
┌──────────────────────────────────────────────────────────────┐
│ 🤖 AI Agents                                    [Search... ] │
│                                                               │
│ Filter: [All ▾]  [Active Only ☑]   Sort: [Most Used ▾]      │
│                                                               │
│ ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐│
│ │ 🟢 Lead Scoring   │  │ 🟢 Support Triage │  │ 🟢 Email     ││
│ │ Agent             │  │ Agent             │  │ Assistant    ││
│ │                   │  │                   │  │              ││
│ │ Scores leads using│  │ Auto-classifies   │  │ Drafts       ││
│ │ BANT criteria and │  │ tickets and       │  │ professional ││
│ │ CRM data analysis │  │ suggests KB       │  │ emails with  ││
│ │                   │  │ resolutions       │  │ CRM context  ││
│ │ Plugins: 3        │  │ Plugins: 3        │  │ Plugins: 3   ││
│ │ ⭐ 4.2 (38 chats) │  │ ⭐ 4.5 (25 chats) │  │ ⭐ 4.0 (42)  ││
│ │                   │  │                   │  │              ││
│ │ [💬 Start Chat]   │  │ [💬 Start Chat]   │  │ [💬 Chat]    ││
│ └──────────────────┘  └──────────────────┘  └──────────────┘│
│                                                               │
│ ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐│
│ │ 🟢 Customer       │  │ ⚫ Sales          │  │ ⚫ Deal       ││
│ │ Success           │  │ Assistant        │  │ Intelligence ││
│ │ [💬 Start Chat]   │  │ [Inactive]       │  │ [Inactive]   ││
│ └──────────────────┘  └──────────────────┘  └──────────────┘│
└──────────────────────────────────────────────────────────────┘
```

### 10.2 Agent Chat Page (`/agents/:id/chat`)

```
┌──────────────────────────────────────────────────────────────┐
│ ← Back │ 🤖 Lead Scoring Agent │ ⚙ Settings │ ⭐ Rate      │
├──────────────────────────────────────────────────────────────┤
│                                                  │ Agent Info│
│  🤖 Hello! I'm the Lead Scoring Agent.          │           │
│     I can analyze your leads using BANT          │ Type:     │
│     criteria. Share a lead name or ID            │ Lead      │
│     and I'll provide a detailed score.           │ Scoring   │
│                                                  │           │
│  👤 Score lead "John Smith from Acme Corp"       │ Plugins:  │
│                                                  │ • Lead    │
│  🔧 [Tool Call: LeadPlugin.GetLeadDetails]       │ • Account │
│     → Found lead #142: John Smith, Acme Corp     │ • Contact │
│                                                  │           │
│  🔧 [Tool Call: AccountPlugin.GetAccount]        │ Temp: 0.3 │
│     → Acme Corp: Enterprise, $2M revenue         │ Tokens:   │
│                                                  │ 2000      │
│  🤖 **Lead Score: 78/100 (Hot)**                 │           │
│                                                  │           │
│     | Criteria  | Score | Notes              |   │           │
│     |-----------|-------|---------------------|  │           │
│     | Budget    | 8/10  | $2M revenue         |  │           │
│     | Authority | 7/10  | Decision maker      |  │           │
│     | Need      | 9/10  | Active evaluation   |  │           │
│     | Timeline  | 6/10  | Q2 target           |  │           │
│                                                  │           │
│  ┌──────────────────────────────────────────┐   │           │
│  │ Type a message...                [Send ➤]│   │           │
│  └──────────────────────────────────────────┘   │           │
└──────────────────────────────────────────────────────────────┘
```

### 10.3 Agent Admin Page (`/admin/agents`)

```
┌──────────────────────────────────────────────────────────────┐
│ Agent Management                        [+ Create Agent]     │
├──────────────────────────────────────────────────────────────┤
│ ┌────────────────────────────────────────────────────────────┐│
│ │ Name              │ Type          │ Active │ Usage │ ⚙    ││
│ ├───────────────────┼───────────────┼────────┼───────┼──────┤│
│ │ Lead Scoring      │ LeadScoring   │ 🟢 On  │ 38    │ Edit ││
│ │ Support Triage    │ SupportTriage │ 🟢 On  │ 25    │ Edit ││
│ │ Email Assistant   │ EmailAssistant│ 🟢 On  │ 42    │ Edit ││
│ │ Customer Success  │ CustomerSucc. │ 🟢 On  │ 18    │ Edit ││
│ │ Sales Assistant   │ SalesAssist.  │ 🔴 Off │ 0     │ Edit ││
│ │ Deal Intelligence │ DealIntel.    │ 🔴 Off │ 0     │ Edit ││
│ └────────────────────────────────────────────────────────────┘│
│                                                               │
│ ═══════ Agent Config Editor (inline or modal) ═══════        │
│ ┌─ General ─┬─ Plugins ─┬─ Model ─┬─ Approval ─┐           │
│ │                                                │           │
│ │ Name:        [Lead Scoring Agent          ]    │           │
│ │ Description: [Scores leads using BANT...   ]   │           │
│ │ System Prompt:                                  │           │
│ │ ┌─────────────────────────────────────────┐    │           │
│ │ │ You are an expert lead scoring agent... │    │           │
│ │ └─────────────────────────────────────────┘    │           │
│ │ Temperature: ────●──── 0.3                     │           │
│ │ Max Tokens:  [2000]                            │           │
│ │                                                │           │
│ │                    [Cancel] [Save Changes]     │           │
│ └────────────────────────────────────────────────┘           │
└──────────────────────────────────────────────────────────────┘
```

### 10.4 Approval Queue Page (`/admin/agents/approvals`)

```
┌──────────────────────────────────────────────────────────────┐
│ Agent Approval Queue                        3 pending        │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│ ┌────────────────────────────────────────────────────────┐   │
│ │ ⚠️ Lead Scoring Agent wants to update Lead #142        │   │
│ │                                                        │   │
│ │ Function: LeadPlugin.UpdateLeadScore                   │   │
│ │ Arguments: { "leadId": 142, "score": 78,               │   │
│ │              "tier": "Hot", "bant": {...} }             │   │
│ │ Requested: 2 minutes ago                               │   │
│ │ Expires: in 28 minutes                                 │   │
│ │ Conversation: "Score lead John Smith from Acme Corp"   │   │
│ │                                                        │   │
│ │                         [❌ Reject] [✅ Approve]        │   │
│ └────────────────────────────────────────────────────────┘   │
│                                                               │
│ ┌────────────────────────────────────────────────────────┐   │
│ │ ⚠️ Email Assistant wants to send email                  │   │
│ │ Function: EmailPlugin.SendEmail                        │   │
│ │ To: john.smith@acme.com                                │   │
│ │ Subject: "Follow-up on our meeting"                    │   │
│ │                         [❌ Reject] [✅ Approve]        │   │
│ └────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

---

## 11. Open Questions

| # | Question | Stakeholder | Impact |
|---|----------|-------------|--------|
| 1 | Should custom agent creation require a backend `POST /api/agents` endpoint, or is DB seeding sufficient for MVP? | Product / Backend | Blocks Phase 6 |
| 2 | Should the ContextFlyout be upgraded to support agents, or should agent chat be a standalone full-page experience only? | UX | Affects Phase 1 scope |
| 3 | How should tool calls be displayed to users? Collapsed by default? Always visible? Toggleable? | UX | Chat page design |
| 4 | Should agents have custom avatars/icons, or use a standard robot icon with color coding? | UX/Brand | Agent cards, chat |
| 5 | Should approval notifications use browser push notifications, or only in-app toasts? | Product | Phase 3 scope |
| 6 | What is the approval timeout (currently 30 min default)? Should users be able to configure per-agent? | Product | Approval UX |
| 7 | Should there be a "marketplace" for sharing agent configurations between CRM instances? | Product | Future roadmap |
| 8 | Should the 9 unimplemented AgentType enum values (no class, no seed) be removed or kept as placeholders? | Backend | Cleanup |
| 9 | How do users understand what plugins an agent has access to? Do they care? | UX Research | Agent detail page |
| 10 | Should conversation history be per-agent or a unified view across all agents? | UX | Conversation history page |

---

## Summary

| Dimension | Backend | Frontend | Gap |
|-----------|---------|----------|-----|
| **Agent CRUD** | 20 endpoints | 5 pages (Directory, Chat, Management, Analytics, Approvals) | 🟢 ~75% covered |
| **Agent Chat** | Full SK pipeline with plugins | AgentChatPage.tsx (709 lines) | 🟢 ~85% covered |
| **Agent Admin** | Config, toggle, analytics endpoints | AgentManagementPage.tsx (778 lines) | 🟢 ~80% covered |
| **Approval Workflow** | Entity + SignalR hub + endpoints | AgentApprovalsPage.tsx (448 lines) | 🟡 ~60% — No SignalR listener |
| **Analytics** | Usage, accuracy, cost endpoints | AgentAnalyticsPage.tsx (488 lines) | 🟢 ~80% covered |
| **Navigation** | N/A | Sidebar items added (user + admin) | 🟢 Resolved |
| **Services** | N/A | 3 service files (79 lines total) | 🟢 ~75% covered |
| **Contextual Triggers** | Some stubs exist | 0 buttons on entity pages | 🟡 ~80% gap |
| **Agent Creation** | ❌ No POST endpoint | ❌ No form | 🔴 100% gap (both sides) |

**The AI agent system is now ~75% connected to the frontend. Five core pages (Directory, Chat, Management, Analytics, Approvals) and three service files are implemented. Remaining work (~8-10 days) covers contextual entity-page triggers (Phases 4-5) and the agent creation/editing form (Phase 6). Phases 1-3 are complete.**

---

## 12. Implementation Plan — File-by-File Specification

> **Added:** February 2026  
> **Purpose:** Concrete implementation plan with exact file paths, TypeScript interfaces, API mappings, and component 11-specifications for parallel agent execution.

### 12.1 Files to Create

| # | File Path | Type | Lines (est.) | Phase | Status |
|---|-----------|------|-------------|-------|--------|
| 1 | `src/types/agents.ts` | TypeScript Types | ~200 | 1 | ✅ **DONE** (330 lines) |
| 2 | `src/services/agentService.ts` | API Service | ~120 | 1 | ✅ **DONE** (52 lines) |
| 3 | `src/services/agentAdminService.ts` | API Service | ~50 | 2 | ✅ **DONE** (13 lines) |
| 4 | `src/services/agentAnalyticsService.ts` | API Service | ~40 | 4 | ✅ **DONE** (14 lines) |
| 5 | `src/pages/AgentDirectoryPage.tsx` | User Page | ~400 | 1 | ✅ **DONE** (395 lines) |
| 6 | `src/pages/AgentChatPage.tsx` | User Page | ~500 | 1 | ✅ **DONE** (709 lines) |
| 7 | `src/pages/AgentManagementPage.tsx` | Admin Page | ~600 | 2 | ✅ **DONE** (778 lines) |
| 8 | `src/pages/AgentApprovalsPage.tsx` | Admin Page | ~400 | 3 | ✅ **DONE** (448 lines) |
| 9 | `src/pages/AgentAnalyticsPage.tsx` | Admin Page | ~450 | 4 | ✅ **DONE** (488 lines) |

### 12.2 Files to Modify

| # | File Path | Changes | Status |
|---|-----------|---------|--------|
| 1 | `src/App.tsx` | Add lazy imports + 5 routes for agent pages | ✅ **DONE** |
| 2 | `src/components/Navigation.tsx` | Add "AI Agents" menu category with 5 items | ✅ **DONE** |

### 12.3 TypeScript Types — `src/types/agents.ts`

Maps directly to backend entities from `CRM.Core/Entities/AI/`:

```typescript
// Enums matching backend
export enum AgentType {
  LeadScoring = 0, SupportTriage = 1, NextBestAction = 2, SalesIntelligence = 3,
  EmailAssistant = 4, CustomerSuccess = 5, RevenueIntelligence = 6, TicketResolution = 7,
  DocumentIntelligence = 8, SalesCoach = 9, MeetingIntelligence = 10,
  ConversationIntelligence = 11, Orchestrator = 12, GeneralAssistant = 13,
  SalesAssistant = 14, DealIntelligence = 15, ForecastAnalyst = 16,
  DataAnalyst = 17, OnboardingGuide = 18, ContractAnalyst = 19, KnowledgeExpert = 20
}

export enum ConversationStatus { Active = 0, Completed = 1, Cancelled = 2, Failed = 3, WaitingForApproval = 4 }
export enum ActionStatus { Pending = 0, Approved = 1, Rejected = 2, Executed = 3, Failed = 4, Cancelled = 5 }
export enum ActionType { Read = 0, Write = 1, Search = 2, Analyze = 3, Notify = 4, Generate = 5 }
export enum ApprovalStatus { Pending = 0, Approved = 1, Rejected = 2, Expired = 3, AutoApproved = 4 }

// Entities
export interface Agent {
  id: number;
  name: string;
  displayName: string;
  description?: string;
  systemPrompt: string;
  agentType: AgentType;
  allowedPlugins: string;    // CSV string from backend
  configuration?: string;    // JSON string
  isActive: boolean;
  requiresApproval: boolean;
  approvalTier?: string;
  temperature: number;
  maxTokens: number;
  modelOverride?: string;
  totalConversations: number;
  totalActions: number;
  averageRating?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface AgentConversation {
  id: number;
  agentId: number;
  userId: number;
  entityType?: string;
  entityId?: number;
  status: ConversationStatus;
  messages: string;          // JSON array of ChatMessageRecord
  messageCount: number;
  totalTokensUsed: number;
  estimatedCost: number;
  userRating?: number;
  userFeedback?: string;
  completedAt?: string;
  createdAt: string;
}

export interface ChatMessageRecord {
  role: 'user' | 'assistant' | 'system' | 'tool';
  content: string;
}

export interface AgentApproval {
  id: number;
  agentActionId: number;
  conversationId: number;
  agentId: number;
  requestedByUserId: number;
  approvedByUserId?: number;
  actionDescription: string;
  pluginName: string;
  functionName: string;
  parameters?: string;       // JSON
  status: ApprovalStatus;
  approvalTier: string;
  rejectionReason?: string;
  decidedAt?: string;
  expiresAt?: string;
  createdAt: string;
}

// Request/Response DTOs (match AgentController inline records)
export interface ChatRequest {
  message: string;
  conversationId?: number;
  entityType?: string;
  entityId?: number;
}

export interface ChatResponse {
  response: string;
  conversationId: number;
  history: ChatMessageRecord[];
}

export interface DraftEmailRequest {
  context: string;
  recipientEmail?: string;
  tone?: string;
  templateId?: number;
}

export interface OrchestrateRequest {
  message: string;
  agentTypes: string[];
  entityType?: string;
  entityId?: number;
}

export interface UpdateAgentRequest {
  systemPrompt?: string;
  temperature?: number;
  maxTokens?: number;
  allowedPlugins?: string;
  modelOverride?: string;
}

export interface RateRequest {
  rating: number;
  feedback?: string;
}

// Analytics DTOs (match AgentAnalyticsController inline records)
export interface AgentUsageMetric {
  agentId: number;
  agentName: string;
  totalConversations: number;
  totalActions: number;
  averageMessagesPerConversation: number;
}

export interface AgentAccuracyMetric {
  agentId: number;
  agentName: string;
  averageRating: number;
  ratedConversations: number;
  totalConversations: number;
}

export interface AgentCostMetric {
  agentId: number;
  agentName: string;
  totalActions: number;
  dailyCosts: DailyCost[];
}

export interface DailyCost {
  date: string;
  actionCount: number;
}
```

### 12.4 API Service Mapping

#### `agentService.ts` — Endpoint Map

| Service Method | HTTP | Backend Route | Notes |
|----------------|------|---------------|-------|
| `getAll()` | GET | `/agents` | Returns `Agent[]` |
| `getById(id)` | GET | `/agents/{id}` | Returns `Agent` |
| `chat(agentId, req)` | POST | `/agents/{agentId}/chat` | Returns `ChatResponse` |
| `getConversations(agentId, limit?)` | GET | `/agents/{agentId}/conversations?limit=` | Returns `AgentConversation[]` |
| `getConversation(conversationId)` | GET | `/agents/conversations/{conversationId}` | Returns `AgentConversation` |
| `rateConversation(conversationId, req)` | POST | `/agents/conversations/{conversationId}/rate` | Returns success |
| `getApprovals()` | GET | `/agents/approvals` | Returns `AgentApproval[]` |
| `approveAction(approvalId)` | POST | `/agents/approvals/{approvalId}/approve` | Returns success |
| `rejectAction(approvalId, reason)` | POST | `/agents/approvals/{approvalId}/reject` | Body: `{ reason }` |
| `draftEmail(req)` | POST | `/agents/email/draft` | Returns email draft |
| `resolveTicket(ticketId)` | POST | `/agents/resolve/{ticketId}` | Returns resolution |
| `orchestrate(req)` | POST | `/agents/orchestrate` | Returns orchestrated response |
| `getDealIntelligence(oppId)` | GET | `/agents/deal-intelligence/{oppId}` | Returns analysis |
| `getNextBestActions(type, id)` | GET | `/agents/next-best-actions/{type}/{id}` | Returns actions |

#### `agentAdminService.ts` — Endpoint Map

| Service Method | HTTP | Backend Route |
|----------------|------|---------------|
| `getConfigs()` | GET | `/agents/admin` |
| `updateConfig(agentId, req)` | PUT | `/agents/admin/{agentId}` |
| `toggleAgent(agentId)` | POST | `/agents/admin/{agentId}/toggle` |

#### `agentAnalyticsService.ts` — Endpoint Map

| Service Method | HTTP | Backend Route |
|----------------|------|---------------|
| `getUsage(days?)` | GET | `/agents/analytics/usage?days=` |
| `getAccuracy(days?)` | GET | `/agents/analytics/accuracy?days=` |
| `getCost(days?)` | GET | `/agents/analytics/cost?days=` |

### 12.5 Page Specifications

#### Page 1: `AgentDirectoryPage.tsx` — Route: `/agents`

**Purpose:** Browse and discover all available AI agents  
**Layout:** Grid of agent cards (3 per row on desktop, 1 on mobile)  
**Data:** `GET /agents` → filtered to `isActive=true` for users  
**Components:**
- Search bar with text filter on name/description
- Agent cards: avatar (colored icon by type), name, description, rating stars, conversation count, "Start Chat" button
- Chip tags for agent type, required approval badge
- Click card → navigate to `/agents/{id}/chat`

#### Page 2: `AgentChatPage.tsx` — Route: `/agents/:agentId/chat`

**Purpose:** Full-page conversational interface with a specific agent  
**Layout:** Left sidebar (conversation history), main chat area, agent info header  
**Data:** `POST /agents/{id}/chat`, `GET /agents/{id}/conversations`  
**Components:**
- Agent header: name, type badge, description, rating
- Message list: user/assistant bubbles, tool call indicators, timestamps
- Input area: text field + send button
- Conversation sidebar: list of past conversations, "New Conversation" button
- Rating dialog: star rating + feedback text on conversation end

#### Page 3: `AgentManagementPage.tsx` — Route: `/admin/agents`

**Purpose:** Admin configuration of all agents  
**Layout:** Table list + inline/dialog config editor  
**Data:** `GET /agents/admin`, `PUT /agents/admin/{id}`, `POST /agents/admin/{id}/toggle`  
**Components:**
- Table: Name, Type, Active toggle, Conversations count, Avg Rating, Actions column
- Config Editor (Dialog with tabs):
  - General: Name, Description, System Prompt (multiline)
  - Model: Temperature slider (0-2), Max Tokens input, Model Override
  - Plugins: Allowed Plugins (comma-separated or chip input)
  - Approval: RequiresApproval toggle, Approval Tier select

#### Page 4: `AgentApprovalsPage.tsx` — Route: `/admin/agents/approvals`

**Purpose:** Review and act on pending agent approval requests  
**Layout:** Card list of pending approvals, real-time updates via SignalR  
**Data:** `GET /agents/approvals`, `POST /agents/approvals/{id}/approve|reject`  
**Components:**
- Approval cards: Agent name, action description, plugin/function, parameters (JSON viewer), timestamps, expiry countdown
- Approve/Reject buttons with confirmation
- Real-time badge count via SignalR `ReceiveApprovalRequest` event
- Filter by status (Pending/Approved/Rejected/Expired)
- Rejection reason dialog for reject action

#### Page 5: `AgentAnalyticsPage.tsx` — Route: `/admin/agents/analytics`

**Purpose:** Usage, accuracy, and cost dashboards for all agents  
**Layout:** Three-section dashboard with charts  
**Data:** `GET /agents/analytics/usage|accuracy|cost`  
**Components:**
- Usage section: Bar chart of conversations per agent, total actions
- Accuracy section: Rating distribution, average rating per agent
- Cost section: Daily action counts, token usage trends
- Date range selector (7d/30d/90d)
- Summary cards: Total conversations, average rating, total actions

### 12.6 Route Registration — `App.tsx` Changes

```tsx
// Lazy imports to add:
const AgentDirectoryPage = React.lazy(() => import('./pages/AgentDirectoryPage'));
const AgentChatPage = React.lazy(() => import('./pages/AgentChatPage'));
const AgentManagementPage = React.lazy(() => import('./pages/AgentManagementPage'));
const AgentApprovalsPage = React.lazy(() => import('./pages/AgentApprovalsPage'));
const AgentAnalyticsPage = React.lazy(() => import('./pages/AgentAnalyticsPage'));

// User routes to add (near other feature routes):
<Route path="/agents" element={<ProtectedRoute><AgentDirectoryPage /></ProtectedRoute>} />
<Route path="/agents/:agentId/chat" element={<ProtectedRoute><AgentChatPage /></ProtectedRoute>} />

// Admin routes to add (inside admin section):
<Route path="/admin/agents" element={<ProtectedRoute><RoleBasedRoute requiredPage="Settings"><AgentManagementPage /></RoleBasedRoute></ProtectedRoute>} />
<Route path="/admin/agents/approvals" element={<ProtectedRoute><RoleBasedRoute requiredPage="Settings"><AgentApprovalsPage /></RoleBasedRoute></ProtectedRoute>} />
<Route path="/admin/agents/analytics" element={<ProtectedRoute><RoleBasedRoute requiredPage="Settings"><AgentAnalyticsPage /></RoleBasedRoute></ProtectedRoute>} />
```

### 12.7 Navigation Changes — `Navigation.tsx`

Add a new "AI Agents" category to the sidebar with these items:

| Label | Icon | Route | Permission |
|-------|------|-------|------------|
| Agent Directory | SmartToyOutlined | `/agents` | (all authenticated users) |
| Agent Chat | ChatOutlined | `/agents` | (all authenticated users) |
| Agent Management | SettingsOutlined | `/admin/agents` | Settings (admin only) |
| Approval Queue | ApprovalOutlined | `/admin/agents/approvals` | Settings (admin only) |
| Agent Analytics | AnalyticsOutlined | `/admin/agents/analytics` | Settings (admin only) |

### 12.8 Design Conventions (from existing codebase)

All new pages MUST follow these patterns:
- **State:** `useState` only (no Redux/Zustand)
- **API:** Import `apiClient` from `../services/apiClient`
- **Loading:** `<CircularProgress />` centered in a `<Box>`
- **Error:** `<Alert severity="error">` at page level
- **Table headers:** Light purple bg `#F5EFF7`, purple text `#6750A4`, `fontWeight: 600`
- **Page header:** Flex row with icon + title + subtitle + action button
- **Cards:** `borderRadius: 3, boxShadow: 1`
- **Empty state:** Use `<EnhancedEmptyState />` component
- **Permissions:** `useProfile()` → `hasPermission()`
- **Export:** `export default ComponentName`

---

**END OF GAP ANALYSIS**

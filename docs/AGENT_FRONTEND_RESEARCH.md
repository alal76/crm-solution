# AI Agent Frontend Research — Verbatim Code Reference

> **Generated:** Research output from comprehensive frontend agent system analysis
> **Purpose:** Understand existing patterns, interfaces, imports, and naming conventions

---

## Table of Contents

1. [types/agents.ts](#1-typesagentsts)
2. [services/agentService.ts](#2-servicesagentservicets)
3. [services/agentAdminService.ts](#3-servicesagentadminservicets)
4. [services/agentAnalyticsService.ts](#4-servicesagentanalyticsservicets)
5. [contexts/SignalRContext.tsx](#5-contextssignalrcontexttsx)
6. [App.tsx — Lazy Imports](#6-apptsx--lazy-imports)
7. [App.tsx — Route Definitions](#7-apptsx--route-definitions)
8. [pages/AgentDirectoryPage.tsx](#8-pagesagentdirectorypagetsx)
9. [components/ContextFlyout.tsx](#9-componentscontextflyouttsx)
10. [pages/AgentChatPage.tsx](#10-pagesagentchatpagetsx)
11. [pages/AgentManagementPage.tsx](#11-pagesagentmanagementpagetsx)
12. [pages/AgentApprovalsPage.tsx](#12-pagesagentapprovalspagetsx)
13. [pages/AgentAnalyticsPage.tsx](#13-pagesagentanalyticspagetsx)
14. [components/Navigation.tsx — Agent Sections](#14-componentsnavigationtsx--agent-sections)

---

## Key Patterns Summary

| Pattern | Detail |
|---------|--------|
| **Primary Color** | `#6750A4` (Material Design 3 purple) |
| **Hover Color** | `#57439B` / `#553d8a` |
| **Service Pattern** | Object literal exported as default, methods return `apiClient.get/post/put()` |
| **Type Module** | All agent types in `src/types/agents.ts` — enums, interfaces, DTOs, helpers |
| **Agent Categories** | Sales (blue), Support (green), Analytics (orange), General (purple) |
| **Route Protection** | `<ProtectedRoute>` for auth, `<RoleBasedRoute requiredPage="Settings">` for admin |
| **Code Splitting** | `React.lazy()` for all agent pages |
| **API Base** | `/agents/*` for user, `/agents/admin/*` for admin, `/agents/analytics/*` for analytics |
| **Chat API** | `/ai/chatbot/message` (ContextFlyout) vs `/agents/{agentId}/chat` (AgentChatPage) |
| **Real-time** | SignalR via `signalRService`, 1s polling for connection state |
| **Nav Category** | `{ id: 'agents', label: 'AI Agents', order: 5 }` |

---

## 1. types/agents.ts

**Path:** `CRM.Frontend/src/types/agents.ts` — 330 lines

```typescript
// ===== ENUMS =====

export enum AgentType {
  LeadScoring = 0,
  SupportTriage = 1,
  NextBestAction = 2,
  SalesIntelligence = 3,
  EmailAssistant = 4,
  CustomerSuccess = 5,
  RevenueIntelligence = 6,
  TicketResolution = 7,
  DocumentIntelligence = 8,
  SalesCoach = 9,
  MeetingIntelligence = 10,
  ConversationIntelligence = 11,
  Orchestrator = 12,
  GeneralAssistant = 13,
  SalesAssistant = 14,
  DealIntelligence = 15,
  ForecastAnalyst = 16,
  DataAnalyst = 17,
  OnboardingGuide = 18,
  ContractAnalyst = 19,
  KnowledgeExpert = 20,
}

export enum ConversationStatus {
  Active = 0,
  Completed = 1,
  Cancelled = 2,
  Failed = 3,
  WaitingForApproval = 4,
}

export enum ActionStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Executed = 3,
  Failed = 4,
  Cancelled = 5,
}

export enum ActionType {
  Read = 0,
  Write = 1,
  Search = 2,
  Analyze = 3,
  Notify = 4,
  Generate = 5,
}

export enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Expired = 3,
  AutoApproved = 4,
}

export enum MemoryType {
  ShortTerm = 0,
  LongTerm = 1,
  Episodic = 2,
  Semantic = 3,
  Procedural = 4,
  UserPreference = 5,
}

// ===== ENTITIES =====

export interface Agent {
  id: number;
  name: string;
  displayName: string;
  description?: string;
  systemPrompt: string;
  agentType: AgentType;
  allowedPlugins: string;
  configuration?: string;
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
  messages: string;
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

export interface AgentAction {
  id: number;
  conversationId: number;
  agentId: number;
  actionType: ActionType;
  pluginName: string;
  functionName: string;
  inputParameters?: string;
  outputResult?: string;
  status: ActionStatus;
  approvalRequestId?: number;
  tokensUsed: number;
  executionTimeMs: number;
  errorMessage?: string;
  createdAt: string;
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
  parameters?: string;
  status: ApprovalStatus;
  approvalTier: string;
  rejectionReason?: string;
  decidedAt?: string;
  expiresAt?: string;
  createdAt: string;
}

export interface AgentMemory {
  id: number;
  agentId: number;
  memoryType: MemoryType;
  key: string;
  value: string;
  entityType?: string;
  entityId?: number;
  confidence: number;
  accessCount: number;
  lastAccessedAt?: string;
  expiresAt?: string;
  createdAt: string;
}

// ===== REQUEST/RESPONSE DTOs =====

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

export interface RateRequest {
  rating: number;
  feedback?: string;
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

// ===== ANALYTICS DTOs =====

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

// ===== HELPER: Labels =====

export const AgentTypeLabels: Record<AgentType, string> = {
  [AgentType.LeadScoring]: 'Lead Scoring',
  [AgentType.SupportTriage]: 'Support Triage',
  [AgentType.NextBestAction]: 'Next Best Action',
  [AgentType.SalesIntelligence]: 'Sales Intelligence',
  [AgentType.EmailAssistant]: 'Email Assistant',
  [AgentType.CustomerSuccess]: 'Customer Success',
  [AgentType.RevenueIntelligence]: 'Revenue Intelligence',
  [AgentType.TicketResolution]: 'Ticket Resolution',
  [AgentType.DocumentIntelligence]: 'Document Intelligence',
  [AgentType.SalesCoach]: 'Sales Coach',
  [AgentType.MeetingIntelligence]: 'Meeting Intelligence',
  [AgentType.ConversationIntelligence]: 'Conversation Intelligence',
  [AgentType.Orchestrator]: 'Orchestrator',
  [AgentType.GeneralAssistant]: 'General Assistant',
  [AgentType.SalesAssistant]: 'Sales Assistant',
  [AgentType.DealIntelligence]: 'Deal Intelligence',
  [AgentType.ForecastAnalyst]: 'Forecast Analyst',
  [AgentType.DataAnalyst]: 'Data Analyst',
  [AgentType.OnboardingGuide]: 'Onboarding Guide',
  [AgentType.ContractAnalyst]: 'Contract Analyst',
  [AgentType.KnowledgeExpert]: 'Knowledge Expert',
};

// ===== HELPER: Colors =====

export const getAgentTypeColor = (agentType: AgentType): string => {
  switch (agentType) {
    // Sales — blue
    case AgentType.LeadScoring: return '#1565c0';
    case AgentType.SalesIntelligence: return '#1976d2';
    case AgentType.SalesAssistant: return '#1e88e5';
    case AgentType.SalesCoach: return '#2196f3';
    case AgentType.DealIntelligence: return '#42a5f5';
    case AgentType.RevenueIntelligence: return '#0d47a1';
    case AgentType.ForecastAnalyst: return '#1565c0';
    case AgentType.ContractAnalyst: return '#1e88e5';
    // Support — green
    case AgentType.SupportTriage: return '#2e7d32';
    case AgentType.TicketResolution: return '#388e3c';
    case AgentType.CustomerSuccess: return '#43a047';
    case AgentType.KnowledgeExpert: return '#66bb6a';
    // Analytics — orange
    case AgentType.DataAnalyst: return '#e65100';
    case AgentType.NextBestAction: return '#ef6c00';
    case AgentType.DocumentIntelligence: return '#f57c00';
    case AgentType.MeetingIntelligence: return '#fb8c00';
    case AgentType.ConversationIntelligence: return '#ff9800';
    // General / utility — purple
    case AgentType.GeneralAssistant: return '#6a1b9a';
    case AgentType.EmailAssistant: return '#7b1fa2';
    case AgentType.OnboardingGuide: return '#8e24aa';
    case AgentType.Orchestrator: return '#9c27b0';
    default: return '#757575';
  }
};
```

---

## 2. services/agentService.ts

**Path:** `CRM.Frontend/src/services/agentService.ts` — 51 lines

```typescript
import apiClient from './apiClient';
import {
  Agent,
  AgentConversation,
  AgentApproval,
  ChatRequest,
  ChatResponse,
  RateRequest,
  DraftEmailRequest,
  OrchestrateRequest,
} from '../types/agents';

const agentService = {
  getAll: () => apiClient.get<Agent[]>('/agents'),
  getById: (id: number) => apiClient.get<Agent>(`/agents/${id}`),
  chat: (agentId: number, request: ChatRequest) =>
    apiClient.post<ChatResponse>(`/agents/${agentId}/chat`, request),
  getConversations: (agentId: number, limit: number = 20) =>
    apiClient.get<AgentConversation[]>(`/agents/${agentId}/conversations`, { params: { limit } }),
  getConversation: (conversationId: number) =>
    apiClient.get<AgentConversation>(`/agents/conversations/${conversationId}`),
  rateConversation: (conversationId: number, request: RateRequest) =>
    apiClient.post(`/agents/conversations/${conversationId}/rate`, request),
  getApprovals: () => apiClient.get<AgentApproval[]>('/agents/approvals'),
  approveAction: (approvalId: number) =>
    apiClient.post(`/agents/approvals/${approvalId}/approve`),
  rejectAction: (approvalId: number, reason: string) =>
    apiClient.post(`/agents/approvals/${approvalId}/reject`, { reason }),
  draftEmail: (request: DraftEmailRequest) =>
    apiClient.post('/agents/email/draft', request),
  resolveTicket: (serviceRequestId: number) =>
    apiClient.post(`/agents/resolve/${serviceRequestId}`),
  orchestrate: (request: OrchestrateRequest) =>
    apiClient.post('/agents/orchestrate', request),
  getDealIntelligence: (opportunityId: number) =>
    apiClient.get(`/agents/deal-intelligence/${opportunityId}`),
  getNextBestActions: (entityType: string, entityId: number) =>
    apiClient.get(`/agents/next-best-actions/${entityType}/${entityId}`),
};

export default agentService;
```

---

## 3. services/agentAdminService.ts

**Path:** `CRM.Frontend/src/services/agentAdminService.ts` — 13 lines

```typescript
import apiClient from './apiClient';
import { Agent, UpdateAgentRequest } from '../types/agents';

const agentAdminService = {
  getConfigs: () => apiClient.get<Agent[]>('/agents/admin'),
  updateConfig: (agentId: number, request: UpdateAgentRequest) =>
    apiClient.put(`/agents/admin/${agentId}`, request),
  toggleAgent: (agentId: number) =>
    apiClient.post(`/agents/admin/${agentId}/toggle`),
};

export default agentAdminService;
```

---

## 4. services/agentAnalyticsService.ts

**Path:** `CRM.Frontend/src/services/agentAnalyticsService.ts` — 14 lines

```typescript
import apiClient from './apiClient';
import { AgentUsageMetric, AgentAccuracyMetric, AgentCostMetric } from '../types/agents';

const agentAnalyticsService = {
  getUsage: (days: number = 30) =>
    apiClient.get<AgentUsageMetric[]>('/agents/analytics/usage', { params: { days } }),
  getAccuracy: (days: number = 30) =>
    apiClient.get<AgentAccuracyMetric[]>('/agents/analytics/accuracy', { params: { days } }),
  getCost: (days: number = 30) =>
    apiClient.get<AgentCostMetric[]>('/agents/analytics/cost', { params: { days } }),
};

export default agentAnalyticsService;
```

---

## 5. contexts/SignalRContext.tsx

**Path:** `CRM.Frontend/src/contexts/SignalRContext.tsx` — 110 lines

```tsx
import React, { createContext, useContext, useEffect, useState, useCallback, ReactNode } from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import signalRService from '../services/signalRService';
import { useAuth } from './AuthContext';

interface SignalRContextValue {
  isConnected: boolean;
  connectionState: HubConnectionState;
  connect: () => Promise<boolean>;
  disconnect: () => Promise<void>;
}

const SignalRContext = createContext<SignalRContextValue | undefined>(undefined);

interface SignalRProviderProps {
  children: ReactNode;
}

const getAccessToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

export function SignalRProvider({ children }: SignalRProviderProps) {
  const { isAuthenticated } = useAuth();
  const [connectionState, setConnectionState] = useState<HubConnectionState>(
    signalRService.getConnectionState() ?? HubConnectionState.Disconnected
  );

  useEffect(() => {
    const intervalId = setInterval(() => {
      const currentState = signalRService.getConnectionState() ?? HubConnectionState.Disconnected;
      setConnectionState(prev => {
        if (prev !== currentState) return currentState;
        return prev;
      });
    }, 1000);
    return () => clearInterval(intervalId);
  }, []);

  useEffect(() => {
    const token = getAccessToken();
    if (isAuthenticated && token) {
      signalRService.connect(token).then(success => {
        if (success) {
          setConnectionState(signalRService.getConnectionState() ?? HubConnectionState.Disconnected);
        }
      });
    } else {
      signalRService.disconnect().then(() => {
        setConnectionState(signalRService.getConnectionState() ?? HubConnectionState.Disconnected);
      });
    }
    return () => { /* Don't disconnect on unmount */ };
  }, [isAuthenticated]);

  const connect = useCallback(async (): Promise<boolean> => {
    const token = getAccessToken();
    if (!token) return false;
    const success = await signalRService.connect(token);
    setConnectionState(signalRService.getConnectionState() ?? HubConnectionState.Disconnected);
    return success;
  }, []);

  const disconnect = useCallback(async (): Promise<void> => {
    await signalRService.disconnect();
    setConnectionState(signalRService.getConnectionState() ?? HubConnectionState.Disconnected);
  }, []);

  const value: SignalRContextValue = {
    isConnected: connectionState === HubConnectionState.Connected,
    connectionState,
    connect,
    disconnect,
  };

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  );
}

export function useSignalRContext(): SignalRContextValue {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalRContext must be used within a SignalRProvider');
  }
  return context;
}

export default SignalRContext;
```

---

## 6. App.tsx — Lazy Imports

**Path:** `CRM.Frontend/src/App.tsx` — Lines 212-217

```tsx
// AI Agent Pages
const AgentDirectoryPage = lazy(() => import('./pages/AgentDirectoryPage'));
const AgentChatPage = lazy(() => import('./pages/AgentChatPage'));
const AgentManagementPage = lazy(() => import('./pages/AgentManagementPage'));
const AgentApprovalsPage = lazy(() => import('./pages/AgentApprovalsPage'));
const AgentAnalyticsPage = lazy(() => import('./pages/AgentAnalyticsPage'));
```

---

## 7. App.tsx — Route Definitions

**Path:** `CRM.Frontend/src/App.tsx` — Lines ~1303-1358

```tsx
{/* AI Agents Routes */}
<Route
  path="/agents"
  element={
    <ProtectedRoute>
      <AgentDirectoryPage />
    </ProtectedRoute>
  }
/>
<Route
  path="/agents/:agentId/chat"
  element={
    <ProtectedRoute>
      <AgentChatPage />
    </ProtectedRoute>
  }
/>
<Route
  path="/admin/agents"
  element={
    <ProtectedRoute>
      <RoleBasedRoute requiredPage="Settings">
        <AgentManagementPage />
      </RoleBasedRoute>
    </ProtectedRoute>
  }
/>
<Route
  path="/admin/agents/approvals"
  element={
    <ProtectedRoute>
      <RoleBasedRoute requiredPage="Settings">
        <AgentApprovalsPage />
      </RoleBasedRoute>
    </ProtectedRoute>
  }
/>
<Route
  path="/admin/agents/analytics"
  element={
    <ProtectedRoute>
      <RoleBasedRoute requiredPage="Settings">
        <AgentAnalyticsPage />
      </RoleBasedRoute>
    </ProtectedRoute>
  }
/>
```

---

## 8. pages/AgentDirectoryPage.tsx

**Path:** `CRM.Frontend/src/pages/AgentDirectoryPage.tsx` — 395 lines

### Imports & Constants

```tsx
import { useState, useEffect, useMemo } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, TextField, Button, Chip,
  CircularProgress, Alert, Avatar, Rating, InputAdornment, Tooltip,
} from '@mui/material';
import {
  SmartToy as SmartToyIcon, Search as SearchIcon,
  Chat as ChatIcon, VerifiedUser as VerifiedUserIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import apiClient from '../services/apiClient';
import agentService from '../services/agentService';
import { Agent, AgentType, AgentTypeLabels, getAgentTypeColor } from '../types/agents';

type CategoryKey = 'All' | 'Sales' | 'Support' | 'Analytics' | 'General';

const CATEGORY_TYPES: Record<CategoryKey, AgentType[] | null> = {
  All: null,
  Sales: [AgentType.LeadScoring, AgentType.SalesIntelligence, AgentType.SalesAssistant,
    AgentType.DealIntelligence, AgentType.ForecastAnalyst, AgentType.SalesCoach,
    AgentType.RevenueIntelligence, AgentType.ContractAnalyst],
  Support: [AgentType.SupportTriage, AgentType.TicketResolution, AgentType.CustomerSuccess,
    AgentType.KnowledgeExpert],
  Analytics: [AgentType.DataAnalyst, AgentType.NextBestAction, AgentType.DocumentIntelligence,
    AgentType.MeetingIntelligence, AgentType.ConversationIntelligence],
  General: [AgentType.GeneralAssistant, AgentType.EmailAssistant, AgentType.OnboardingGuide,
    AgentType.Orchestrator],
};

const CATEGORY_COLORS: Record<CategoryKey, string> = {
  All: '#6750A4', Sales: '#1565c0', Support: '#2e7d32', Analytics: '#e65100', General: '#6a1b9a',
};
```

### Key Behaviors
- `getCategoryForAgent()` maps AgentType → CategoryKey
- Fetches agents via `agentService.getAll()`, filters active only
- Search TextField + category filter Chips
- Cards: Avatar with typeColor background, displayName, AgentType Chip, requiresApproval Chip (orange), 2-line clamped description, Rating + conversation count, "Start Chat" Button navigating to `/agents/${agent.id}/chat`
- Grid layout: `xs=12 sm=6 md=4`, hover shadow + `translateY(-2px)`
- Button color: `#6750A4`

---

## 9. components/ContextFlyout.tsx

**Path:** `CRM.Frontend/src/components/ContextFlyout.tsx` — 512 lines

### Structure
- Right-side MUI Drawer (400px width, persistent variant)
- Two sections: **Account Context** selector (Autocomplete, limit 500) + **CRM Assistant** chatbot
- Chat uses `apiClient.post('/ai/chatbot/message')` with conversationHistory + accountContext
- Initialization via `/ai/chatbot/initialize`
- Fixed FAB at bottom-right with Badge for selected accounts count
- Uses `useAccountContext()` for selectedAccounts, addAccount, removeAccount, clearAccounts, isFlyoutOpen, toggleFlyout
- Message bubble styling: user = `primary.main` bg + white text, assistant = `grey.100` bg
- Enter to send (Shift+Enter = newline), loading spinner, error handling

---

## 10. pages/AgentChatPage.tsx

**Path:** `CRM.Frontend/src/pages/AgentChatPage.tsx` — 709 lines

### Imports

```tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box, Typography, TextField, Button, Paper, CircularProgress, Alert,
  Avatar, Chip, IconButton, Dialog, DialogTitle, DialogContent,
  DialogActions, Rating as MuiRating, Tooltip, Divider, List, ListItem,
  ListItemText, ListItemButton, Badge,
} from '@mui/material';
import {
  Send as SendIcon, ArrowBack as ArrowBackIcon, Star as StarIcon,
  Info as InfoIcon, History as HistoryIcon, Download as DownloadIcon,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import agentService from '../services/agentService';
import {
  Agent, ChatMessageRecord, AgentConversation, AgentType,
  AgentTypeLabels, getAgentTypeColor, ChatRequest,
} from '../types/agents';
```

### State & Functions
- State: agent, messages (ChatMessageRecord[]), inputMessage, sending, error, loading, conversationId, conversations, showHistory, showRating, ratingValue, feedbackText, ratingSubmitting, typeColor
- `loadAgent`: `agentService.getById(agentId)` + sets typeColor via `getAgentTypeColor`
- `loadConversations`: `agentService.getConversations(agentId, 10)`
- `handleSendMessage`: builds ChatRequest, calls `agentService.chat(agentId, request)`, updates messages from response.data.history
- `handleSubmitRating`: `agentService.rateConversation(conversationId, { rating, feedback })`
- `handleLoadConversation`: JSON.parse(conv.messages) → set messages
- `handleExportChat`: downloads as .txt file

### Layout
- Full-height flex with header: back arrow, agent avatar (typeColor bg), displayName, type Chip, Info/History/Download/Rate icon buttons
- Message bubbles: system/tool as centered Chip, user right-aligned (`#E3F2FD` bg, borderRadius `16px 16px 4px 16px`), assistant left-aligned (`#F5F5F5` bg, `16px 16px 16px 4px`) with agent avatar
- Thinking indicator: agent avatar + "Agent is thinking..." + CircularProgress
- Input: multiline maxRows=4, purple send button (`#6750A4`), disabled during sending
- Rating dialog: 5-star MuiRating + optional feedback TextField + Submit button
- History dialog: list with messageCount, date, optional rating stars

---

## 11. pages/AgentManagementPage.tsx

**Path:** `CRM.Frontend/src/pages/AgentManagementPage.tsx` — 778 lines

### Imports

```tsx
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Paper, Switch, Button, Dialog,
  DialogTitle, DialogContent, DialogActions, TextField, Chip, Alert,
  CircularProgress, Avatar, Tabs, Tab, Slider, Rating, Tooltip,
  FormControlLabel,
} from '@mui/material';
import {
  SettingsOutlined, EditOutlined, SmartToyOutlined,
  ChatOutlined, StarOutlined, CheckCircleOutline,
} from '@mui/icons-material';
import agentAdminService from '../services/agentAdminService';
import {
  Agent, AgentType, AgentTypeLabels, getAgentTypeColor, UpdateAgentRequest,
} from '../types/agents';
```

### Structure
- Helper components: `TabPanel`, `SummaryCard` (icon + label + value + color)
- Loads agents via `agentAdminService.getConfigs()`
- `handleToggle`: calls `agentAdminService.toggleAgent(agentId)`
- `handleSave`: builds `UpdateAgentRequest`, calls `agentAdminService.updateConfig(agentId, request)`
- Computed stats: totalAgents, activeAgents, totalConversations, avgRating
- 4 SummaryCards: Total (purple), Active (green), Conversations (blue), Rating (orange)
- Table: purple `#6750A4` header, 7 columns (Agent avatar+names, Type Chip, Active Switch, Approval Chip, Conversations, Rating stars, Edit button), inactive rows `opacity: 0.6`
- Config Editor Dialog (maxWidth="md"): DialogTitle with avatar + name, 4 Tabs:
  - **General**: disabled fields + editable System Prompt (minRows=6)
  - **Model**: Temperature Slider 0-2 step 0.1 + Max Tokens 256-16384 step 256 + Model Override
  - **Plugins**: Allowed Plugins TextField + preview Chips
  - **Approval**: disabled Switch + disabled Tier + info Alert
- DialogActions: Cancel + Save (`#6750A4`, hover `#553d8a`)

---

## 12. pages/AgentApprovalsPage.tsx

**Path:** `CRM.Frontend/src/pages/AgentApprovalsPage.tsx` — 448 lines

### Imports

```tsx
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Card, CardContent, Button, Dialog, DialogTitle,
  DialogContent, DialogActions, TextField, Chip, Alert, CircularProgress,
  Divider, Badge, Paper,
} from '@mui/material';
import {
  ApprovalOutlined, CheckCircleOutline, CancelOutlined,
  AccessTimeOutlined, PersonOutlined,
} from '@mui/icons-material';
import agentService from '../services/agentService';
import { AgentApproval, ApprovalStatus, ApprovalStatusLabel } from '../types/agents';
```

### Key Details
- STATUS_COLOR_MAP: Pending=warning, Approved=success, Rejected=error, Expired=default, AutoApproved=info
- `ApprovalStatusLabel` map for display strings
- `formatRelativeTime` helper, `formatJson` helper (JSON.stringify pretty-print)
- Auto-refresh every 30 seconds, success auto-dismiss after 4s
- Filter chips: All, Pending, Approved, Rejected, Expired
- Cards show: Agent #id, status Chip, action description, plugin/function Chips (fontFamily monospace), parameters as formatted JSON in `<pre><code>` block (maxHeight 200), created/expires relative time, requester user #id
- Pending approvals get Approve (green) + Reject (red) buttons
- Reject dialog: required reason TextField, Cancel + Reject buttons

> **Note:** `ApprovalStatusLabel` is referenced but not actually exported from types/agents.ts — defined locally in the page component.

---

## 13. pages/AgentAnalyticsPage.tsx

**Path:** `CRM.Frontend/src/pages/AgentAnalyticsPage.tsx` — 488 lines

### Imports

```tsx
import React, { useState, useEffect, useCallback } from 'react';
import {
  Box, Typography, Card, CardContent, Grid, Alert, CircularProgress, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  ToggleButton, ToggleButtonGroup, LinearProgress, Rating,
} from '@mui/material';
import {
  AnalyticsOutlined, ChatOutlined, BoltOutlined, StarOutlined,
  GroupOutlined, TrendingUpOutlined, TrendingDownOutlined,
} from '@mui/icons-material';
import agentAnalyticsService from '../services/agentAnalyticsService';
import { AgentUsageMetric, AgentAccuracyMetric, AgentCostMetric } from '../types/agents';
```

### Key Details
- `computeTrend()` helper: compares recent 7 days vs previous 7 days → direction (up/down/flat) + percentage
- State: usage/accuracy/cost arrays, loading, error, days=30
- `loadData`: `Promise.all` for 3 API calls (getUsage, getAccuracy, getCost)
- ToggleButtonGroup: 7/30/90 days
- 4 Summary Cards: Total Conversations (purple `#6750A4`), Total Actions (blue `#2196F3`), Average Rating (orange `#FF9800`), Active Agents (green `#4CAF50`)
- 3 Data Tables with purple `#F5EFF7` headers:
  - **Usage**: Agent, Conversations, Distribution (purple LinearProgress), Actions, Avg Messages/Conv
  - **Accuracy**: Agent, Avg Rating (Rating + numeric), Rated, Total, Rating % (orange LinearProgress)
  - **Cost & Activity**: Agent, Total Actions, Daily Avg, Trend (TrendingUp red `#F44336` / TrendingDown green `#4CAF50` with percentage)

---

## 14. components/Navigation.tsx — Agent Sections

**Path:** `CRM.Frontend/src/components/Navigation.tsx` — 962 lines

### navItemsConfig Agent Entry

```typescript
'agent-directory': { label: 'AI Agents', icon: SmartToyIcon, path: '/agents', menuName: 'AgentDirectory' },
```

### adminItemsConfig Agent Entries

```typescript
'agent-management': { label: 'Agent Management', icon: SmartToyIcon, path: '/admin/agents', menuName: 'AgentManagement' },
'agent-approvals': { label: 'Agent Approvals', icon: PersonAddIcon, path: '/admin/agents/approvals', menuName: 'AgentApprovals' },
'agent-analytics': { label: 'Agent Analytics', icon: AnalyticsIcon, path: '/admin/agents/analytics', menuName: 'AgentAnalytics' },
```

### Category Definition

```typescript
{ id: 'agents', label: 'AI Agents', order: 5 }
```

### defaultNavItemsWithCategory Agent Entries

```typescript
{ id: 'agent-directory', order: 40, visible: true, category: 'agents' },
{ id: 'agent-management', order: 79, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
{ id: 'agent-approvals', order: 80, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
{ id: 'agent-analytics', order: 81, visible: true, category: 'admin', adminSubcategory: 'admin-workflows' },
```

### Path Detection (sidebar expansion)

- Line ~210-211: `path.includes('/agents')` → expand `agents` category
- SmartToyIcon imported at line 90
- Agents category default expanded at line ~122

---

## API Endpoint Summary

### Agent User Endpoints (`/agents/*`)

| Method | Path | Service Method | Purpose |
|--------|------|----------------|---------|
| GET | `/agents` | `agentService.getAll()` | List all agents |
| GET | `/agents/{id}` | `agentService.getById(id)` | Get single agent |
| POST | `/agents/{agentId}/chat` | `agentService.chat(agentId, req)` | Send chat message |
| GET | `/agents/{agentId}/conversations` | `agentService.getConversations(agentId, limit)` | List conversations |
| GET | `/agents/conversations/{id}` | `agentService.getConversation(id)` | Get conversation |
| POST | `/agents/conversations/{id}/rate` | `agentService.rateConversation(id, req)` | Rate conversation |
| GET | `/agents/approvals` | `agentService.getApprovals()` | List approvals |
| POST | `/agents/approvals/{id}/approve` | `agentService.approveAction(id)` | Approve action |
| POST | `/agents/approvals/{id}/reject` | `agentService.rejectAction(id, reason)` | Reject action |
| POST | `/agents/email/draft` | `agentService.draftEmail(req)` | Draft email |
| POST | `/agents/resolve/{serviceRequestId}` | `agentService.resolveTicket(id)` | Resolve ticket |
| POST | `/agents/orchestrate` | `agentService.orchestrate(req)` | Multi-agent orchestration |
| GET | `/agents/deal-intelligence/{id}` | `agentService.getDealIntelligence(id)` | Deal intelligence |
| GET | `/agents/next-best-actions/{type}/{id}` | `agentService.getNextBestActions(type, id)` | Next best actions |

### Agent Admin Endpoints (`/agents/admin/*`)

| Method | Path | Service Method | Purpose |
|--------|------|----------------|---------|
| GET | `/agents/admin` | `agentAdminService.getConfigs()` | List agent configs |
| PUT | `/agents/admin/{agentId}` | `agentAdminService.updateConfig(id, req)` | Update agent config |
| POST | `/agents/admin/{agentId}/toggle` | `agentAdminService.toggleAgent(id)` | Toggle active state |

### Agent Analytics Endpoints (`/agents/analytics/*`)

| Method | Path | Service Method | Purpose |
|--------|------|----------------|---------|
| GET | `/agents/analytics/usage` | `agentAnalyticsService.getUsage(days)` | Usage metrics |
| GET | `/agents/analytics/accuracy` | `agentAnalyticsService.getAccuracy(days)` | Accuracy metrics |
| GET | `/agents/analytics/cost` | `agentAnalyticsService.getCost(days)` | Cost metrics |

### AI Chatbot Endpoints (ContextFlyout)

| Method | Path | Purpose |
|--------|------|---------|
| POST | `/ai/chatbot/message` | Send chatbot message |
| POST | `/ai/chatbot/initialize` | Initialize chatbot |

---

**END OF RESEARCH**

# Workflow Module Enhancements

## Overview

This document describes the comprehensive enhancements made to the CRM Workflow Module, implementing a full-featured workflow engine with LLM integration, visual design tools, and enterprise-grade resilience patterns.

## New Frontend Components

### 1. RuleBuilder (`src/components/workflow/RuleBuilder.tsx`)

Visual AND/OR condition builder for workflow Condition nodes.

**Features:**
- Nested group support with AND/OR logical operators
- Field selection from entity schema
- Rich operator set (equals, contains, greater than, etc.)
- Variable references using `{{variable}}` syntax
- JSON toggle for advanced editing

**Usage:**
```tsx
import { RuleBuilder, createDefaultRuleGroup } from '../components/workflow';

<RuleBuilder
  value={conditionRules}
  onChange={setConditionRules}
  fields={entityFields}
  variables={workflowVariables}
/>
```

### 2. VersionDiffViewer (`src/components/workflow/VersionDiffViewer.tsx`)

Compare workflow versions side-by-side.

**Features:**
- Node and transition diff computation
- Visual change indicators (added/modified/removed)
- Tabbed view for nodes, transitions, and raw JSON
- Version selector dropdown

### 3. ExecutionTimeline (`src/components/workflow/ExecutionTimeline.tsx`)

Gantt-style visualization of workflow execution.

**Features:**
- Timeline bars with color-coded status
- Duration formatting (seconds, minutes, hours)
- Summary statistics (total steps, success rate, duration)
- Click-to-select step details

### 4. AuditLogViewer (`src/components/workflow/AuditLogViewer.tsx`)

View and export workflow change history.

**Features:**
- Event type filtering (Created, Modified, Activated, etc.)
- Date range selection
- Expandable row details
- CSV export capability

### 5. WorkflowSimulator (`src/components/workflow/WorkflowSimulator.tsx`)

Test workflows with sample data without side effects.

**Features:**
- Sample data templates per entity type
- Custom JSON input editor
- Step-by-step execution preview
- Simulated action results display

### 6. EnhancedPropertiesPanel (`src/components/workflow/EnhancedPropertiesPanel.tsx`)

Advanced node configuration with multiple tabs.

**Features:**
- **Basic Tab:** Action type selection
- **Inputs Tab:** Input variable mapping with static/variable/expression modes
- **Outputs Tab:** Output variable mapping to paths
- **LLM Tab:** Model selection, temperature, max tokens, JSON mode
- **Error Handling Tab:** Retry settings, fallback actions
- **Permissions Tab:** Role-based access, approvers

---

## Backend Services

### 1. LLMService (`CRM.Infrastructure/Services/LLMService.cs`)

Multi-provider AI integration service with enterprise cloud support.

**Supported Providers:**
| Provider | Models | Use Case |
|----------|--------|----------|
| **OpenAI** | GPT-4, GPT-4 Turbo, GPT-3.5 | General purpose, best quality |
| **Azure OpenAI** | GPT-4, GPT-3.5 (deployed) | Enterprise, data residency compliance |
| **Anthropic** | Claude 3 Opus/Sonnet/Haiku | Long context, reasoning |
| **Google Cloud** | Gemini 1.5 Pro/Flash | Multimodal, Google ecosystem |
| **AWS Bedrock** | Claude, Titan, Llama | AWS ecosystem, multi-model |
| **Local LLM** | Llama, Mistral, etc. | Privacy, offline, cost savings |
| **Custom Endpoint** | Any OpenAI-compatible | Self-hosted, specialized models |

**Features:**
- Automatic fallback between providers
- Variable interpolation in prompts
- JSON mode for structured responses
- Token usage tracking
- Configurable timeouts

**Configuration:**
```json
{
  "LLMProviders": {
    "DefaultProvider": "openai",
    "EnableFallback": true,
    "FallbackOrder": ["openai", "azure", "anthropic", "google", "local"],
    
    "OpenAI": {
      "ApiKey": "sk-...",
      "DefaultModel": "gpt-4"
    },
    
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com",
      "ApiKey": "your-azure-key",
      "DeploymentName": "gpt-4-deployment",
      "ApiVersion": "2024-02-01"
    },
    
    "Anthropic": {
      "ApiKey": "sk-ant-...",
      "DefaultModel": "claude-3-sonnet-20240229"
    },
    
    "GoogleCloud": {
      "ProjectId": "your-project-id",
      "Location": "us-central1",
      "ApiKey": "your-gemini-api-key",
      "DefaultModel": "gemini-1.5-pro",
      "UseVertexAI": false
    },
    
    "AWSBedrock": {
      "Region": "us-east-1",
      "DefaultModel": "anthropic.claude-3-sonnet-20240229-v1:0",
      "UseDefaultCredentials": true
    },
    
    "LocalLLM": {
      "BaseUrl": "http://localhost:11434",
      "DefaultModel": "llama3",
      "ApiFormat": "ollama",
      "Enabled": false
    }
  }
}
```

**Environment Variables:**
```bash
# OpenAI
export OPENAI_API_KEY="sk-..."

# Azure OpenAI
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"
export AZURE_OPENAI_API_KEY="your-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"

# Anthropic
export ANTHROPIC_API_KEY="sk-ant-..."

# Google Cloud
export GOOGLE_AI_API_KEY="your-gemini-key"
export GOOGLE_CLOUD_PROJECT_ID="your-project"
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/service-account.json"

# AWS Bedrock (or use IAM roles)
export AWS_REGION="us-east-1"
export AWS_ACCESS_KEY_ID="your-access-key"
export AWS_SECRET_ACCESS_KEY="your-secret-key"

# Local LLM
export LOCAL_LLM_URL="http://localhost:11434"
```

### 2. ResilienceService (`CRM.Infrastructure/Services/ResilienceService.cs`)

Circuit breaker and retry patterns using Polly.

**Patterns Implemented:**
- **Timeout:** Configurable per-service timeouts
- **Retry:** Exponential backoff with jitter
- **Circuit Breaker:** Automatic failure detection and recovery

**Service-Specific Configuration:**
```json
{
  "Resilience": {
    "DefaultTimeoutSeconds": 30,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "MaxRetryAttempts": 3,
    "Services": {
      "llm-openai": {
        "TimeoutSeconds": 60,
        "CircuitBreakerThreshold": 3
      }
    }
  }
}
```

---

## API Endpoints

### Audit Log

```
GET /api/workflow-instances/definitions/{definitionId}/audit-log
  ?eventType=string
  &eventCategory=string
  &fromDate=datetime
  &toDate=datetime
  &skip=int
  &take=int

GET /api/workflow-instances/definitions/{definitionId}/audit-log/export
  → Returns CSV file
```

### Execution Timeline

```
GET /api/workflow-instances/{instanceId}/timeline
  → Returns ExecutionTimeline with entries
```

### Skip Node

```
POST /api/workflow-instances/{instanceId}/skip-node/{nodeId}
  Body: { "reason": "string" }
```

---

## Database Schema Changes

Migration: `010_workflow_enhancements.sql`

### New Tables

1. **workflow_audit_log** - Comprehensive change tracking
2. **workflow_metrics** - Hourly aggregated metrics
3. **workflow_llm_usage** - LLM API call tracking
4. **workflow_circuit_breaker_state** - Circuit breaker persistence

### Schema Updates

- `workflow_tasks`: Added `idempotency_key`, `skip_reason`, `skipped_at`, `skipped_by`
- `workflow_definitions`: Added `version_number`, `version_notes`, `parent_version_id`, `owner_id`, `last_modified_by`, `last_modified_at`
- `workflow_node_instances`: Added `was_skipped`, `skip_reason`, `skipped_by`, `skipped_at`
- `workflow_nodes`: Added `allowed_roles`, `approvers`, `require_mfa`
- `workflow_instances`: Added `parent_instance_id`, `parent_node_instance_id`, `subprocess_level`

### Seed Data

Pre-built lifecycle workflow definitions:
- Lead Lifecycle Management
- Customer Onboarding
- Opportunity Follow-up Automation
- Support Ticket Escalation
- Quote Approval Process

---

## Usage Examples

### Starting a Workflow with LLM Action

```typescript
// Frontend
const instance = await workflowService.startWorkflow({
  workflowDefinitionId: 1,
  entityType: 'Lead',
  entityId: 123,
  triggerEvent: 'LeadScoreChanged',
  inputData: {
    score: 85,
    previousScore: 60
  }
});
```

### Configuring an LLM Node

```json
{
  "llmModel": "gpt-4",
  "llmPrompt": "Analyze this lead and suggest next actions:\n\nName: {{entity.name}}\nScore: {{entity.score}}\nSource: {{entity.source}}",
  "llmTemperature": 0.7,
  "llmMaxTokens": 500,
  "llmJsonMode": true,
  "llmJsonSchema": {
    "type": "object",
    "properties": {
      "priority": { "type": "string", "enum": ["high", "medium", "low"] },
      "suggestedActions": { "type": "array", "items": { "type": "string" } }
    }
  }
}
```

### Building Conditions

```typescript
const conditionGroup = {
  id: '1',
  logical: 'AND',
  conditions: [
    {
      id: '1a',
      field: 'score',
      operator: 'greaterThan',
      value: 80,
      valueType: 'number'
    },
    {
      id: '1b',
      logical: 'OR',
      conditions: [
        { field: 'source', operator: 'equals', value: 'Website' },
        { field: 'source', operator: 'equals', value: 'Referral' }
      ]
    }
  ]
};
```

---

## Package Dependencies

### Backend (.NET)
- `Polly` 8.2.0 - Resilience patterns

### Frontend (npm)
- No new dependencies - uses existing MUI components

---

## Security Considerations

1. **API Keys:** Store LLM API keys in environment variables or secrets manager
2. **Role-Based Access:** Workflow nodes can require specific roles
3. **MFA Support:** Critical nodes can require multi-factor authentication
4. **Audit Logging:** All workflow changes are logged with actor information
5. **Idempotency:** Prevent duplicate task execution with idempotency keys

---

## Performance Optimizations

1. **Circuit Breakers:** Prevent cascade failures from external services
2. **Metrics Aggregation:** Hourly rollups for dashboard performance
3. **Subprocess Hierarchy:** Efficient querying with level tracking
4. **Index Coverage:** Optimized indexes for common query patterns

---

## Future Enhancements

1. **Webhook Triggers:** HTTP-based workflow initiation
2. **Parallel Execution:** Fork/join patterns for concurrent processing
3. **Workflow Templates:** Pre-built industry-specific workflows
4. **Real-time Monitoring:** WebSocket-based live updates
5. **A/B Testing:** Compare workflow variant performance

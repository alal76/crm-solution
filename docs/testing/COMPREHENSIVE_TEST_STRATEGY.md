# CRM Solution - Comprehensive Test Strategy Document

**Version:** 2.0.0  
**Last Updated:** February 6, 2026  
**Document Status:** Active

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Test Architecture Overview](#2-test-architecture-overview)
3. [Backend Unit Tests Inventory](#3-backend-unit-tests-inventory)
4. [Frontend Unit Tests Inventory](#4-frontend-unit-tests-inventory)
5. [E2E Tests Inventory](#5-e2e-tests-inventory)
6. [Integration Tests Inventory](#6-integration-tests-inventory)
7. [Pluggable Architecture Tests](#7-pluggable-architecture-tests)
8. [Workflow Tests & Gap Analysis](#8-workflow-tests--gap-analysis)
9. [BVT Tests Inventory](#9-bvt-tests-inventory)
10. [Coverage Gap Analysis](#10-coverage-gap-analysis)
11. [Test Automation Scripts](#11-test-automation-scripts)
12. [Test Execution Plan](#12-test-execution-plan)
13. [Version Tracking](#13-version-tracking)

---

## 1. Executive Summary

### Current Test Statistics

| Test Category | Count | Framework | Location |
|--------------|-------|-----------|----------|
| Backend Unit Tests | ~1050 | xUnit + Moq | `CRM.Backend/tests/` |
| Frontend Unit Tests | ~180 | Jest + RTL | `CRM.Frontend/src/__tests__/` |
| E2E API Tests (BVT) | ~60 | Playwright | `e2e-tests/tests/bvt/` |
| E2E UI Tests | ~200+ | Playwright | `e2e-tests/tests/` |
| Integration Tests | ~55 | xUnit | `CRM.Backend/tests/Integration/` |
| Provider Tests | ~280 | xUnit | `CRM.Backend/tests/CRM.Tests/Providers/` |
| Performance Tests | ~20 | xUnit | `CRM.Backend/tests/Performance/` |

### Overall Coverage

| Area | Current Coverage | Target | Gap |
|------|-----------------|--------|-----|
| Core Entities | 85% | 95% | +10% |
| Business Services | 70% | 80% | +10% |
| API Controllers | 45% | 55% | +10% |
| Provider Integrations | 80% | 90% | +10% |
| Frontend Components | 60% | 70% | +10% |
| E2E Critical Paths | 90% | 95% | +5% |
| **Pluggable Architecture** | **85%** | **95%** | **+10%** |

---

## 2. Test Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           TEST PYRAMID                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│                           ┌─────────────┐                               │
│                           │    E2E      │  ← Playwright (UI + API)      │
│                           │   Tests     │     ~260 tests                │
│                          ─┴─────────────┴─                              │
│                         ┌─────────────────┐                             │
│                         │   Integration   │  ← xUnit + Live DB          │
│                         │     Tests       │     ~36 tests               │
│                        ─┴─────────────────┴─                            │
│                       ┌───────────────────────┐                         │
│                       │    Unit Tests         │  ← xUnit + Jest         │
│                       │    (Backend +         │     ~1000 tests         │
│                       │     Frontend)         │                         │
│                      ─┴───────────────────────┴─                        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Test Frameworks

| Layer | Framework | Assertion | Mocking |
|-------|-----------|-----------|---------|
| Backend Unit | xUnit 2.6.2 | FluentAssertions 6.12 | Moq 4.20 |
| Frontend Unit | Jest 29 | Jest/RTL | Jest mocks |
| E2E | Playwright 1.40 | Playwright expect | - |
| Performance | xUnit | Custom | - |

---

## 3. Backend Unit Tests Inventory

### 3.1 Entity Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Entities/CoreEntityTests.cs` | ~40 | Account, Contact, Lead, Opportunity entities | ✅ Active |
| `Entities/EntityValidationTests.cs` | ~25 | Entity validation rules | ✅ Active |
| `Entities/EnumTypeTests.cs` | ~50 | All enum type validations | ✅ Active |
| `EntityTests.cs` | ~20 | General entity tests | ✅ Active |
| `UserEntityTests.cs` | ~15 | User entity specific tests | ✅ Active |

### 3.2 Service Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Services/AccountServiceTests.cs` | ~45 | Account CRUD, filtering, lifecycle | ✅ Active |
| `Services/AuthenticationServiceTests.cs` | ~35 | Login, JWT, 2FA, password reset | ✅ Active |
| `Services/LeadServiceTests.cs` | ~30 | Lead management, scoring, conversion | ✅ Active |
| `Services/OpportunityServiceTests.cs` | ~35 | Opportunity pipeline, stages, amounts | ✅ Active |
| `Services/ProductServiceTests.cs` | ~25 | Product catalog, pricing | ✅ Active |
| `Services/UserServiceTests.cs` | ~30 | User CRUD, roles, permissions | ✅ Active |
| `Services/SystemSettingsServiceTests.cs` | ~20 | System configuration | ✅ Active |
| `Services/RelationshipServiceTests.cs` | ~25 | Account relationships | ✅ Active |
| `Services/CampaignExecutionServiceTests.cs` | ~30 | Campaign execution logic | ✅ Active |
| `Services/DuplicateDetectionTests.cs` | ~20 | Duplicate detection algorithms | ✅ Active |
| `Services/AllenAIServiceTests.cs` | ~15 | AI integration tests | ✅ Active |

### 3.3 ITSM Service Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Services/ITSM/IncidentServiceTests.cs` | ~25 | Incident management | ✅ Active |
| `Services/ITSM/ProblemServiceTests.cs` | ~20 | Problem management | ✅ Active |
| `Services/ITSM/ChangeServiceTests.cs` | ~20 | Change management | ✅ Active |
| `Services/ITSM/KnowledgeServiceTests.cs` | ~15 | Knowledge base | ✅ Active |
| `Services/ITSM/SLAServiceTests.cs` | ~20 | SLA management | ✅ Active |
| `Services/ITSM/CMDBServiceTests.cs` | ~15 | Configuration management | ✅ Active |
| `Services/ITSM/CatalogServiceTests.cs` | ~15 | Service catalog | ✅ Active |

### 3.4 Controller Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Controllers/AccountsControllerTests.cs` | ~20 | Account API endpoints | ✅ Active |
| `Controllers/ProductsControllerTests.cs` | ~15 | Product API endpoints | ✅ Active |
| `Controllers/OpportunitiesControllerTests.cs` | ~15 | Opportunity API endpoints | ✅ Active |
| `Controllers/DepartmentsControllerTests.cs` | ~10 | Department API endpoints | ✅ Active |

### 3.5 Provider Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Ports/ProviderPortContractTests.cs` | ~44 | Provider interface contracts | ✅ Active |
| `Factories/ProviderFactoryTests.cs` | ~24 | Factory pattern tests | ✅ Active |
| `CRM.Tests/Providers/AIProviderTests.cs` | ~20 | AI provider implementations | ✅ Active |
| `CRM.Tests/Providers/IntegrationProviderTests.cs` | ~46 | Integration providers (N8n, Zapier) | ✅ Active |
| `CRM.Tests/Providers/IntercomProviderTests.cs` | ~24 | Intercom chat provider | ✅ Active |
| `CRM.Tests/Providers/SupersetProviderTests.cs` | ~29 | Superset analytics | ✅ Active |
| `CRM.Tests/Providers/PowerBIProviderTests.cs` | ~27 | Power BI analytics | ✅ Active |
| `CRM.Tests/Providers/DocuSealProviderTests.cs` | ~27 | DocuSeal e-signature | ✅ Active |
| `CRM.Tests/Providers/DocuSignProviderTests.cs` | ~48 | DocuSign e-signature | ✅ Active |
| `CRM.Tests/Providers/BuiltInSignatureProviderTests.cs` | ~34 | Built-in signature | ✅ Active |

### 3.6 BVT Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `BVT/CriticalPathBVTTests.cs` | ~60 | Critical path validation | ✅ Active |
| `BVT/AIFeaturesBVTTests.cs` | ~15 | AI features verification | ✅ Active |
| `BVT/AllenAISmokeBVTTests.cs` | ~10 | AI smoke tests | ✅ Active |
| `BVT/ITSMCoreBVTTests.cs` | ~20 | ITSM core features | ✅ Active |
| `BVT/ITSMPhase4BVTTests.cs` | ~15 | ITSM Phase 4 features | ✅ Active |

### 3.7 Functional Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Functional/ApiEndpointFunctionalTests.cs` | ~28 | API endpoint integration | ✅ Active |
| `Functional/ITSMCoreFunctionalTests.cs` | ~15 | ITSM core integration | ✅ Active |
| `Functional/ITSMPhase4FunctionalTests.cs` | ~10 | ITSM Phase 4 integration | ✅ Active |
| `Functional/RelationshipCampaignFunctionalTests.cs` | ~10 | Relationships & campaigns | ✅ Active |

### 3.8 Integration Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Integration/BuiltInSearchProviderIntegrationTests.cs` | ~17 | Search provider with DB | ✅ Active |
| `Integration/MeilisearchProviderIntegrationTests.cs` | ~10 | Meilisearch integration | ⚠️ Requires Meilisearch |
| `Integration/ProviderDIIntegrationTests.cs` | ~9 | DI container integration | ✅ Active |

### 3.9 Performance Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Performance/PerformanceTests.cs` | ~15 | Load and stress tests | ⏸️ Manual Run |
| `Performance/PerformanceTestHarness.cs` | - | Test harness utilities | ✅ Active |

---

## 4. Frontend Unit Tests Inventory

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `LoginPage.comprehensive.test.tsx` | ~30 | Login, 2FA, OAuth, validation | ✅ Active |
| `LoginPage.test.tsx` | ~10 | Basic login tests | ✅ Active |
| `CustomersPage.comprehensive.test.tsx` | ~25 | Customer management UI | ✅ Active |
| `CustomersPage.test.tsx` | ~10 | Basic customer tests | ✅ Active |
| `ContactsPage.comprehensive.test.tsx` | ~20 | Contact management UI | ✅ Active |
| `OpportunitiesPage.comprehensive.test.tsx` | ~20 | Opportunity UI | ✅ Active |
| `OpportunitiesPage.test.tsx` | ~10 | Basic opportunity tests | ✅ Active |
| `ProductsPage.comprehensive.test.tsx` | ~15 | Product catalog UI | ✅ Active |
| `ProductsPage.test.tsx` | ~8 | Basic product tests | ✅ Active |
| `DashboardPage.comprehensive.test.tsx` | ~20 | Dashboard UI | ✅ Active |
| `AdminPages.comprehensive.test.tsx` | ~15 | Admin pages UI | ✅ Active |
| `Navigation.comprehensive.test.tsx` | ~10 | Navigation components | ✅ Active |
| `SharedComponents.comprehensive.test.tsx` | ~15 | Shared UI components | ✅ Active |
| `ServiceRequestsPage.test.tsx` | ~10 | Service requests UI | ✅ Active |
| `CampaignsPage.test.tsx` | ~10 | Campaigns UI | ✅ Active |
| `ITSMCorePages.test.tsx` | ~10 | ITSM core pages | ✅ Active |
| `ITSMPhase4Pages.test.tsx` | ~8 | ITSM Phase 4 pages | ✅ Active |
| `apiClient.test.ts` | ~10 | API client utilities | ✅ Active |

---

## 5. E2E Tests Inventory

### 5.1 BVT API Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `bvt/api-bvt.spec.ts` | ~55 | Core API critical paths | ✅ Active |
| `bvt/itsm-api-bvt.spec.ts` | ~25 | ITSM API verification | ✅ Active |
| `bvt/itsm-core-api-bvt.spec.ts` | ~20 | ITSM core API | ✅ Active |

### 5.2 Functional UI Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `functional/ui-functional.spec.ts` | ~30 | Core UI functional tests | ✅ Active |
| `functional/itsm-ui-functional.spec.ts` | ~20 | ITSM UI tests | ✅ Active |
| `functional/itsm-core-ui-functional.spec.ts` | ~15 | ITSM core UI | ✅ Active |

### 5.3 Module-Specific Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `customers/customers.spec.ts` | ~26 | Customer management E2E | ✅ Active |
| `contacts/contacts.spec.ts` | ~20 | Contact management E2E | ✅ Active |
| `leads/leads.spec.ts` | ~15 | Lead management E2E | ✅ Active |
| `opportunities/opportunities.spec.ts` | ~15 | Opportunity E2E | ✅ Active |
| `campaigns/campaigns.spec.ts` | ~15 | Campaign E2E | ✅ Active |
| `campaigns/campaign-execution.spec.ts` | ~10 | Campaign execution | ✅ Active |
| `campaigns/campaign-bugs.spec.ts` | ~8 | Campaign bug regression | ✅ Active |
| `workflows/workflows.spec.ts` | ~15 | Workflow E2E | ✅ Active |
| `workflow-execution/workflow-execution.spec.ts` | ~10 | Workflow execution | ✅ Active |
| `service-requests/` | ~15 | Service request E2E | ✅ Active |
| `users/create-users.spec.ts` | ~10 | User creation E2E | ✅ Active |
| `groups/create-groups.spec.ts` | ~10 | Group creation E2E | ✅ Active |
| `admin/admin.spec.ts` | ~12 | Admin functions E2E | ✅ Active |
| `dashboard/` | ~10 | Dashboard E2E | ✅ Active |

### 5.4 Auth Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `auth/authentication.spec.ts` | ~15 | Authentication flows | ✅ Active |
| `auth.setup.ts` | - | Auth setup fixture | ✅ Active |

### 5.5 Special Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `account-contact-linking.spec.ts` | ~10 | Account-contact relationships | ✅ Active |
| `notes-quotes-features.spec.ts` | ~15 | Notes & quotes features | ✅ Active |
| `relationships/` | ~10 | Relationship management | ✅ Active |
| `deduplication/` | ~8 | Duplicate detection | ✅ Active |
| `data-lifecycle/` | ~10 | Data lifecycle tests | ✅ Active |
| `persona/persona-e2e-journeys.spec.ts` | ~15 | User journey E2E | ✅ Active |
| `persona/persona-api-journeys.spec.ts` | ~12 | API journey tests | ✅ Active |

---

## 6. Integration Tests Inventory

### 6.1 Database Integration Tests

| Test Category | Test Count | Description | Status |
|--------------|------------|-------------|--------|
| Database Integration | ~36 | Tests requiring live database | ✅ Active |
| Search Integration | ~27 | Search provider tests | ✅ Active |
| Provider DI Integration | ~9 | DI container integration | ✅ Active |

### 6.2 Provider Integration Tests

| Test File | Test Count | Provider Category | Status |
|-----------|------------|-------------------|--------|
| `BuiltInSearchProviderIntegrationTests.cs` | ~17 | Search (BuiltIn) | ✅ Active |
| `MeilisearchProviderIntegrationTests.cs` | ~10 | Search (Meilisearch) | ⚠️ Requires Container |
| `ProviderDIIntegrationTests.cs` | ~9 | All Providers | ✅ Active |

### 6.3 External Service Integration Tests

| Test Category | Test Count | Description | Status |
|--------------|------------|-------------|--------|
| AI Integration | ~20 | AI/LLM provider tests | ⚠️ Conditional |
| Chat Integration | ~15 | Chatwoot/Intercom tests | ⚠️ Conditional |
| Notification Integration | ~15 | Novu/Twilio/SendGrid tests | ⚠️ Conditional |

---

## 7. Pluggable Architecture Tests

### 7.1 Provider Port Contract Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Ports/ProviderPortContractTests.cs` | ~44 | All port interface contracts | ✅ Active |

**Port Interfaces Tested:**

| Port Interface | Contract Tests | Description |
|---------------|----------------|-------------|
| `ISearchPort` | 6 tests | Search, index, suggest operations |
| `IChatPort` | 8 tests | Contact sync, conversations, messages |
| `INotificationPort` | 6 tests | Email, SMS, push notifications |
| `IAnalyticsPort` | 6 tests | Reports, dashboards, charts |
| `ISignaturePort` | 6 tests | E-signature requests, templates |
| `IAIPort` | 6 tests | Chat completion, embeddings |
| `IIntegrationPort` | 6 tests | Webhooks, workflow triggers |

### 7.2 Provider Factory Tests

| Test File | Test Count | Functional Area | Status |
|-----------|------------|-----------------|--------|
| `Factories/ProviderFactoryTests.cs` | ~24 | Factory pattern verification | ✅ Active |

**Factory Tests by Provider:**

| Factory | Tests | Providers Supported |
|---------|-------|---------------------|
| `SearchProviderFactory` | 4 | BuiltIn, Meilisearch, Algolia, Typesense |
| `ChatProviderFactory` | 3 | BuiltIn, Chatwoot, Intercom |
| `NotificationProviderFactory` | 4 | BuiltIn, Novu, Twilio, SendGrid |
| `AnalyticsProviderFactory` | 4 | BuiltIn, Superset, PowerBI, Metabase |
| `SignatureProviderFactory` | 3 | BuiltIn, DocuSeal, DocuSign |
| `AIProviderFactory` | 4 | Ollama, OpenAI, Azure, Anthropic, Bedrock |
| `IntegrationProviderFactory` | 3 | BuiltIn, N8n, Zapier |

### 7.3 Provider Implementation Tests

| Test File | Test Count | Provider | Status |
|-----------|------------|----------|--------|
| `Providers/AIProviderTests.cs` | ~20 | Ollama, Azure OpenAI, Bedrock | ✅ Active |
| `Providers/IntegrationProviderTests.cs` | ~46 | BuiltIn, N8n, Zapier | ✅ Active |
| `Providers/IntercomProviderTests.cs` | ~24 | Intercom Chat | ✅ Active |
| `Providers/SupersetProviderTests.cs` | ~29 | Apache Superset | ✅ Active |
| `Providers/PowerBIProviderTests.cs` | ~27 | Microsoft Power BI | ✅ Active |
| `Providers/DocuSealProviderTests.cs` | ~27 | DocuSeal E-Signature | ✅ Active |
| `Providers/DocuSignProviderTests.cs` | ~48 | DocuSign E-Signature | ✅ Active |
| `Providers/BuiltInSignatureProviderTests.cs` | ~34 | Built-in Signature | ✅ Active |

### 7.4 Provider Health & Monitoring Tests

| Test Area | Test Count | Description | Status |
|-----------|------------|-------------|--------|
| Health Check Endpoints | 6 | `/api/health/providers` | ✅ Active |
| Adapter Registry | 4 | Provider health monitoring | ✅ Active |
| Fallback Behavior | 8 | BuiltIn fallback when external fails | ✅ Active |
| Feature Flag Switching | 6 | Dynamic provider switching | ✅ Active |

### 7.5 Provider Test Coverage Summary

| Provider Category | BuiltIn | External Providers | Total Tests |
|------------------|---------|-------------------|-------------|
| Search | ✅ 17 | Meilisearch: 18, Algolia: 18 | 53 |
| Chat | ✅ 33 | Chatwoot: 29, Intercom: 24 | 86 |
| Notification | ✅ 26 | Novu: 34, Twilio: 19, SendGrid: 30 | 109 |
| Analytics | ✅ 36 | Superset: 29, PowerBI: 27 | 92 |
| E-Signature | ✅ 34 | DocuSeal: 27, DocuSign: 48 | 109 |
| AI/LLM | N/A | Ollama: 20, Azure: 20, Bedrock: 20 | 60 |
| Integration | ✅ 15 | N8n: 14, Zapier: 17 | 46 |
| **TOTAL** | **161** | **394** | **555** |

---

## 8. Workflow Tests & Gap Analysis

### 8.1 Business Workflows Requiring Pluggable Components

#### Search Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-SRCH-001 | Global Entity Search | ISearchPort | ✅ Tested |
| WF-SRCH-002 | Account Search & Filter | ISearchPort | ✅ Tested |
| WF-SRCH-003 | Contact Search | ISearchPort | ✅ Tested |
| WF-SRCH-004 | Opportunity Search | ISearchPort | ✅ Tested |
| WF-SRCH-005 | Product Catalog Search | ISearchPort | ✅ Tested |
| WF-SRCH-006 | Knowledge Article Search | ISearchPort | ✅ Tested |
| WF-SRCH-007 | Full-Text Index Rebuild | ISearchPort | ⚠️ Manual |
| WF-SRCH-008 | Search Suggestions/Autocomplete | ISearchPort | ✅ Tested |

#### Chat & Communication Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-CHAT-001 | Customer Chat Initiation | IChatPort | ✅ Tested |
| WF-CHAT-002 | Contact Sync to Chat Platform | IChatPort | ✅ Tested |
| WF-CHAT-003 | Agent Assignment | IChatPort | ✅ Tested |
| WF-CHAT-004 | Conversation History Sync | IChatPort | ✅ Tested |
| WF-CHAT-005 | Chat Webhook Processing | IChatPort | ✅ Tested |
| WF-CHAT-006 | Chat-to-CRM Activity Sync | IChatPort, IActivityService | ⚠️ Partial |
| WF-CHAT-007 | Chat Transcript Export | IChatPort | ❌ Not Tested |

#### Notification Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-NOTIF-001 | Send Email Notification | INotificationPort | ✅ Tested |
| WF-NOTIF-002 | Send SMS Notification | INotificationPort | ✅ Tested |
| WF-NOTIF-003 | Send Push Notification | INotificationPort | ✅ Tested |
| WF-NOTIF-004 | Bulk Email Campaign | INotificationPort | ✅ Tested |
| WF-NOTIF-005 | Email Template Rendering | INotificationPort | ✅ Tested |
| WF-NOTIF-006 | Notification Preferences | INotificationPort | ⚠️ Partial |
| WF-NOTIF-007 | Delivery Status Webhook | INotificationPort | ✅ Tested |
| WF-NOTIF-008 | Email Sequence Execution | INotificationPort | ❌ Not Tested |

#### Analytics & Reporting Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-ANLY-001 | Dashboard Loading | IAnalyticsPort | ✅ Tested |
| WF-ANLY-002 | Report Generation | IAnalyticsPort | ✅ Tested |
| WF-ANLY-003 | Chart Data Retrieval | IAnalyticsPort | ✅ Tested |
| WF-ANLY-004 | Embedded Dashboard Auth | IAnalyticsPort | ✅ Tested |
| WF-ANLY-005 | Data Source Refresh | IAnalyticsPort | ⚠️ Partial |
| WF-ANLY-006 | Export Report to PDF | IAnalyticsPort | ❌ Not Tested |
| WF-ANLY-007 | Scheduled Report Delivery | IAnalyticsPort, INotificationPort | ❌ Not Tested |

#### E-Signature Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-ESIG-001 | Create Signature Request | ISignaturePort | ✅ Tested |
| WF-ESIG-002 | Template Management | ISignaturePort | ✅ Tested |
| WF-ESIG-003 | Signer Notification | ISignaturePort, INotificationPort | ⚠️ Partial |
| WF-ESIG-004 | Document Status Check | ISignaturePort | ✅ Tested |
| WF-ESIG-005 | Webhook Processing | ISignaturePort | ✅ Tested |
| WF-ESIG-006 | Quote E-Signature | ISignaturePort, QuoteService | ⚠️ Partial |
| WF-ESIG-007 | Contract E-Signature | ISignaturePort, ContractService | ⚠️ Partial |
| WF-ESIG-008 | Audit Trail Generation | ISignaturePort | ✅ Tested |

#### AI/LLM Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-AI-001 | Email Drafting | IAIPort | ✅ Tested |
| WF-AI-002 | Sentiment Analysis | IAIPort | ✅ Tested |
| WF-AI-003 | Entity Extraction | IAIPort | ✅ Tested |
| WF-AI-004 | Lead Scoring | IAIPort | ✅ Tested |
| WF-AI-005 | Chat Completion | IAIPort | ✅ Tested |
| WF-AI-006 | Embeddings Generation | IAIPort | ✅ Tested |
| WF-AI-007 | Meeting Summary | IAIPort | ⚠️ Partial |
| WF-AI-008 | Call Transcription Analysis | IAIPort | ❌ Not Tested |

#### Integration Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-INT-001 | Webhook Registration | IIntegrationPort | ✅ Tested |
| WF-INT-002 | Event Publishing | IIntegrationPort | ✅ Tested |
| WF-INT-003 | Workflow Trigger | IIntegrationPort | ✅ Tested |
| WF-INT-004 | External Data Sync | IIntegrationPort | ⚠️ Partial |
| WF-INT-005 | CRM Event to N8n | IIntegrationPort | ✅ Tested |
| WF-INT-006 | CRM Event to Zapier | IIntegrationPort | ✅ Tested |
| WF-INT-007 | Bidirectional Sync | IIntegrationPort | ❌ Not Tested |

#### Authentication & OAuth Workflows

| Workflow ID | Workflow Name | Components Used | Test Status |
|-------------|--------------|-----------------|-------------|
| WF-AUTH-001 | Standard Login | AuthService | ✅ Tested |
| WF-AUTH-002 | Google OAuth Login | AuthService, GoogleOAuth | ⚠️ Partial |
| WF-AUTH-003 | Microsoft OAuth Login | AuthService, MicrosoftOAuth | ❌ Not Tested |
| WF-AUTH-004 | Password Setup (First Login) | AuthService | ⚠️ Partial |
| WF-AUTH-005 | Password Reset | AuthService, INotificationPort | ⚠️ Partial |
| WF-AUTH-006 | 2FA Setup | AuthService | ⚠️ Partial |
| WF-AUTH-007 | 2FA Verification | AuthService | ✅ Tested |
| WF-AUTH-008 | Session Refresh | AuthService | ✅ Tested |

### 8.2 Workflow Gap Analysis Summary

| Category | Total Workflows | Tested | Partial | Not Tested | Coverage |
|----------|----------------|--------|---------|------------|----------|
| Search | 8 | 7 | 1 | 0 | 87.5% |
| Chat | 7 | 5 | 1 | 1 | 71.4% |
| Notification | 8 | 6 | 1 | 1 | 75.0% |
| Analytics | 7 | 4 | 1 | 2 | 57.1% |
| E-Signature | 8 | 5 | 3 | 0 | 62.5% |
| AI/LLM | 8 | 6 | 1 | 1 | 75.0% |
| Integration | 7 | 5 | 1 | 1 | 71.4% |
| Authentication | 8 | 4 | 3 | 1 | 50.0% |
| **TOTAL** | **61** | **42** | **12** | **7** | **68.9%** |

### 8.3 High-Priority Workflow Gaps

| Priority | Workflow ID | Workflow Name | Gap Description | Remediation |
|----------|-------------|---------------|-----------------|-------------|
| 🔴 HIGH | WF-AUTH-002 | Google OAuth Login | Only setup script tested | Create E2E OAuth flow test |
| 🔴 HIGH | WF-AUTH-004 | Password Setup | Backend tested, no E2E | Create E2E password setup test |
| 🔴 HIGH | WF-AUTH-005 | Password Reset | No notification integration | Test with INotificationPort |
| 🔴 HIGH | WF-ANLY-006 | Export Report to PDF | Not implemented | Implement and test |
| 🟡 MED | WF-CHAT-006 | Chat-to-CRM Sync | Activity creation partial | Complete ActivityService integration |
| 🟡 MED | WF-NOTIF-008 | Email Sequence | Execution flow not tested | Create sequence execution tests |
| 🟡 MED | WF-ESIG-006 | Quote E-Signature | Quote→Signature partial | Test full flow |
| 🟡 MED | WF-INT-007 | Bidirectional Sync | Complex flow not tested | Create sync verification tests |

---

## 9. BVT Tests Inventory

### BVT Test IDs and Coverage

| Test ID | Test Name | Category | Technical Area |
|---------|-----------|----------|----------------|
| BVT-01-001 | Health check endpoint | Auth | System Health |
| BVT-01-002 | Login with valid credentials | Auth | Authentication |
| BVT-01-003 | Login with invalid credentials | Auth | Authentication |
| BVT-01-004 | Protected endpoint requires auth | Auth | Authorization |
| BVT-02-001 | Create customer | Customer | CRUD |
| BVT-02-002 | Read customer | Customer | CRUD |
| BVT-02-003 | Update customer | Customer | CRUD |
| BVT-02-004 | List customers | Customer | CRUD |
| BVT-02-005 | Delete customer | Customer | CRUD |
| BVT-03-001 | Create contact | Contact | CRUD |
| BVT-03-002 | Read contact | Contact | CRUD |
| BVT-03-003 | List contacts | Contact | CRUD |
| BVT-03-004 | Delete contact | Contact | CRUD |
| BVT-04-001 | Create lead | Lead | CRUD |
| BVT-04-002 | Read lead | Lead | CRUD |
| BVT-04-003 | List leads | Lead | CRUD |
| BVT-04-004 | Delete lead | Lead | CRUD |
| BVT-05-001 | Create opportunity | Opportunity | CRUD |
| BVT-05-002 | Read opportunity | Opportunity | CRUD |
| BVT-05-003 | List opportunities | Opportunity | CRUD |
| BVT-06-001 | Create service request | Service | CRUD |
| BVT-06-002 | Read service request | Service | CRUD |
| BVT-06-003 | List service requests | Service | CRUD |
| BVT-07-001 | Create campaign | Campaign | CRUD |
| BVT-07-002 | Read campaign | Campaign | CRUD |
| BVT-07-003 | List campaigns | Campaign | CRUD |
| BVT-08-001 | Create product | Product | CRUD |
| BVT-08-002 | Read product | Product | CRUD |
| BVT-08-003 | List products | Product | CRUD |
| BVT-09-001 | Create quote | Quote | CRUD |
| BVT-09-002 | Read quote | Quote | CRUD |
| BVT-09-003 | List quotes | Quote | CRUD |
| BVT-10-001 | List users | User | CRUD |
| BVT-10-002 | List user groups | UserGroup | CRUD |
| BVT-10-003 | Get current user profile | UserProfile | CRUD |
| BVT-11-001 | Get dashboard data | Dashboard | Data |
| BVT-11-002 | Get dashboard config | Dashboard | Config |
| BVT-12-001 | List notes | Notes | CRUD |
| BVT-12-002 | Create note | Notes | CRUD |
| BVT-13-001 | Get system settings | Settings | Config |
| BVT-13-002 | Get lookups | Lookups | Config |
| BVT-14-001 | List webhook subscriptions | ITSM | Webhooks |
| BVT-14-002 | Get webhook event types | ITSM | Webhooks |
| BVT-14-003 | Get email parsing config | ITSM | Email |
| BVT-14-004 | Get ITSM dashboard metrics | ITSM | Dashboard |
| BVT-14-005 | Get incident trends | ITSM | Analytics |
| BVT-14-006 | Get SLA compliance | ITSM | SLA |
| BVT-14-007 | Get executive summary | ITSM | Analytics |
| BVT-14-008 | Get monitoring sources | ITSM | Monitoring |
| BVT-14-009 | Get registered pipelines | ITSM | CI/CD |
| BVT-14-010 | Get chatbot quick actions | ITSM | Chatbot |

---

## 10. Coverage Gap Analysis

### 10.1 Untested Controllers

| Controller | Current Coverage | Priority | Gap Items |
|------------|-----------------|----------|-----------|
| `AuthController.cs` | 40% | 🔴 High | Password setup, OAuth flows, 2FA |
| `ContactsController.cs` | 0% | 🔴 High | All CRUD endpoints |
| `LeadsController.cs` | 0% | 🔴 High | All CRUD endpoints |
| `QuotesController.cs` | 0% | 🟡 Medium | All CRUD endpoints |
| `ActivitiesController.cs` | 0% | 🟡 Medium | All CRUD endpoints |
| `NotesController.cs` | 0% | 🟡 Medium | All CRUD endpoints |
| `TasksController.cs` | 0% | 🟡 Medium | All CRUD endpoints |
| `WorkflowController.cs` | 0% | 🟡 Medium | Workflow management |
| `WorkflowInstanceController.cs` | 0% | 🟡 Medium | Workflow execution |
| `CampaignsController.cs` | 0% | 🟡 Medium | Campaign management |
| `EmailTemplatesController.cs` | 0% | 🟢 Low | Template management |
| `ImportExportController.cs` | 0% | 🟢 Low | Import/Export features |
| `FileUploadController.cs` | 0% | 🟢 Low | File uploads |
| `ZipCodesController.cs` | 0% | 🟢 Low | ZIP code lookup |

### 10.2 Untested Services

| Service | Current Coverage | Priority | Gap Items |
|---------|-----------------|----------|-----------|
| `ActivityService.cs` | 0% | 🟡 Medium | Activity tracking, timeline |
| `ContactInfoService.cs` | 0% | 🟡 Medium | Contact info management |
| `ContactsService.cs` | 0% | 🔴 High | Contact CRUD |
| `WorkflowService.cs` | 20% | 🟡 Medium | Workflow execution |
| `WorkflowInstanceService.cs` | 0% | 🟡 Medium | Instance management |
| `ServiceRequestService.cs` | 0% | 🟡 Medium | Service request handling |
| `MarketingCampaignService.cs` | 30% | 🟡 Medium | Campaign management |
| `MergeService.cs` | 0% | 🟡 Medium | Record merging |
| `NormalizationService.cs` | 0% | 🟢 Low | Data normalization |
| `CalendarSyncService.cs` | 0% | 🟢 Low | Calendar integration |
| `EmailSyncService.cs` | 0% | 🟢 Low | Email sync |
| `DatabaseBackupService.cs` | 0% | 🟢 Low | Backup operations |
| `CloudDeploymentService.cs` | 0% | 🟢 Low | Cloud deployment |
| `ResilienceService.cs` | 0% | 🟢 Low | Circuit breaker, retry |

### 10.3 Untested Frontend Components

| Component Area | Current Coverage | Priority | Gap Items |
|---------------|-----------------|----------|-----------|
| SetupPasswordPage | 0% | 🔴 High | Password setup flow |
| ForgotPasswordPage | 0% | 🔴 High | Password reset flow |
| TwoFactorSetupPage | 0% | 🔴 High | 2FA setup |
| UserProfilePage | 0% | 🟡 Medium | Profile management |
| SettingsPages | 0% | 🟡 Medium | Settings UI |
| WorkflowDesigner | 0% | 🟡 Medium | Workflow builder |
| ReportsPage | 0% | 🟢 Low | Reporting UI |
| ImportExportPage | 0% | 🟢 Low | Import/export UI |

### 10.4 Untested E2E Flows

| Flow | Priority | Description |
|------|----------|-------------|
| Password Setup on First Login | 🔴 High | New user sets password |
| Password Reset | 🔴 High | User resets forgotten password |
| 2FA Setup | 🔴 High | User enables 2FA |
| Google OAuth Login | 🔴 High | Google SSO flow |
| User Registration | 🟡 Medium | Self-registration |
| Quote to Order Conversion | 🟡 Medium | Quote workflow |
| Lead Conversion | 🟡 Medium | Lead to opportunity |
| Campaign Execution | 🟡 Medium | Full campaign run |
| Workflow Execution | 🟡 Medium | Automated workflow |
| Email Template Management | 🟢 Low | Template CRUD |
| Report Generation | 🟢 Low | Report creation |

---

## 11. Test Automation Scripts

### 11.1 Master Test Runner Script

Located at: `scripts/run-all-tests.sh`

```bash
#!/bin/bash
# Full test execution with version tracking
```

### 11.2 Deployment + Test Script

Located at: `scripts/deploy-and-test.sh`

```bash
#!/bin/bash
# Deploy solution and run all tests
```

### 11.3 CI/CD Pipeline

Located at: `azure-pipelines.yml` and GitHub Actions

---

## 12. Test Execution Plan

### 12.1 Daily (CI)

| Test Suite | Duration | Trigger |
|------------|----------|---------|
| Backend Unit Tests | ~2 min | Every commit |
| Frontend Unit Tests | ~1 min | Every commit |
| API BVT Tests | ~3 min | Every commit |

### 12.2 Nightly

| Test Suite | Duration | Trigger |
|------------|----------|---------|
| Full Backend Tests | ~5 min | Scheduled 2 AM |
| Full E2E Tests | ~15 min | Scheduled 2 AM |
| Integration Tests | ~5 min | Scheduled 2 AM |

### 12.3 Weekly

| Test Suite | Duration | Trigger |
|------------|----------|---------|
| Performance Tests | ~30 min | Scheduled Sunday |
| Security Scans | ~10 min | Scheduled Sunday |

### 12.4 Pre-Release

| Test Suite | Duration | Trigger |
|------------|----------|---------|
| All Tests | ~45 min | Manual |
| Regression Suite | ~20 min | Manual |
| Smoke Tests | ~5 min | Manual |

---

## 13. Version Tracking

### Component Versions (Tracked in test reports)

| Component | Version Source | Current |
|-----------|---------------|---------|
| Backend API | `version.json` | 0.0.27 |
| Frontend | `package.json` | 0.0.27 |
| Database Schema | Migration ID | 2026-02 |
| Test Framework | NuGet/npm | See below |

### Test Framework Versions

| Framework | Version |
|-----------|---------|
| xUnit | 2.6.2 |
| Moq | 4.20.70 |
| FluentAssertions | 6.12.0 |
| Jest | 29.x |
| Playwright | 1.40.0 |
| React Testing Library | 14.x |

---

## Appendix A: Test Coverage Matrix

### Backend API Endpoints vs Tests

| Endpoint | Unit | Integration | E2E |
|----------|------|-------------|-----|
| POST /api/auth/login | ✅ | ✅ | ✅ |
| POST /api/auth/register | ❌ | ❌ | ❌ |
| POST /api/auth/setup-password | ⚠️ | ❌ | ⚠️ |
| POST /api/auth/forgot-password | ⚠️ | ❌ | ⚠️ |
| POST /api/auth/reset-password | ⚠️ | ❌ | ⚠️ |
| POST /api/auth/refresh | ✅ | ❌ | ❌ |
| POST /api/auth/google/callback | ⚠️ | ❌ | ⚠️ |
| GET /api/accounts | ✅ | ✅ | ✅ |
| POST /api/accounts | ✅ | ✅ | ✅ |
| PUT /api/accounts/:id | ✅ | ✅ | ✅ |
| DELETE /api/accounts/:id | ✅ | ✅ | ✅ |
| GET /api/contacts | ⚠️ | ❌ | ✅ |
| POST /api/contacts | ⚠️ | ❌ | ✅ |
| GET /api/leads | ⚠️ | ❌ | ✅ |
| POST /api/leads | ⚠️ | ❌ | ✅ |
| GET /api/opportunities | ✅ | ✅ | ✅ |
| POST /api/opportunities | ✅ | ✅ | ✅ |
| GET /api/products | ✅ | ✅ | ✅ |
| POST /api/products | ✅ | ✅ | ✅ |
| GET /api/quotes | ⚠️ | ❌ | ✅ |
| POST /api/quotes | ⚠️ | ❌ | ✅ |
| GET /api/campaigns | ⚠️ | ❌ | ✅ |
| POST /api/campaigns | ⚠️ | ❌ | ✅ |
| GET /api/users | ⚠️ | ❌ | ✅ |
| POST /api/users | ⚠️ | ❌ | ⚠️ |
| GET /api/servicerequests | ⚠️ | ❌ | ✅ |
| POST /api/servicerequests | ⚠️ | ❌ | ✅ |
| GET /api/notes | ⚠️ | ❌ | ✅ |
| POST /api/notes | ⚠️ | ❌ | ✅ |
| GET /api/health/providers | ✅ | ✅ | ✅ |
| GET /api/admin/features | ✅ | ✅ | ⚠️ |

**Legend:** ✅ Fully Tested | ⚠️ Partially Tested | ❌ Not Tested

---

## Appendix B: Pluggable Architecture Provider Matrix

### Provider Implementation Status

| Provider Category | Provider Name | Implementation | Unit Tests | Integration Tests | E2E Tests |
|------------------|---------------|----------------|------------|-------------------|-----------|
| **Search** | BuiltIn | ✅ Complete | ✅ 17 | ✅ 17 | ✅ Yes |
| | Meilisearch | ✅ Complete | ✅ 18 | ⚠️ Conditional | ⚠️ Manual |
| | Algolia | ✅ Complete | ✅ 18 | ❌ None | ❌ None |
| | Typesense | ⚠️ Partial | ❌ None | ❌ None | ❌ None |
| **Chat** | BuiltIn | ✅ Complete | ✅ 33 | ❌ None | ❌ None |
| | Chatwoot | ✅ Complete | ✅ 29 | ⚠️ Conditional | ❌ None |
| | Intercom | ✅ Complete | ✅ 24 | ⚠️ Conditional | ❌ None |
| **Notification** | BuiltIn | ✅ Complete | ✅ 26 | ❌ None | ❌ None |
| | Novu | ✅ Complete | ✅ 34 | ⚠️ Conditional | ❌ None |
| | Twilio | ✅ Complete | ✅ 19 | ⚠️ Conditional | ❌ None |
| | SendGrid | ✅ Complete | ✅ 30 | ⚠️ Conditional | ❌ None |
| **Analytics** | BuiltIn | ✅ Complete | ✅ 36 | ❌ None | ❌ None |
| | Superset | ✅ Complete | ✅ 29 | ⚠️ Conditional | ❌ None |
| | Power BI | ✅ Complete | ✅ 27 | ⚠️ Conditional | ❌ None |
| | Metabase | ⚠️ Partial | ❌ None | ❌ None | ❌ None |
| **E-Signature** | BuiltIn | ✅ Complete | ✅ 34 | ❌ None | ❌ None |
| | DocuSeal | ✅ Complete | ✅ 27 | ⚠️ Conditional | ❌ None |
| | DocuSign | ✅ Complete | ✅ 48 | ⚠️ Conditional | ❌ None |
| **AI/LLM** | Ollama | ✅ Complete | ✅ 20 | ⚠️ Conditional | ⚠️ Manual |
| | OpenAI | ✅ Complete | ⚠️ Partial | ⚠️ Conditional | ❌ None |
| | Azure OpenAI | ✅ Complete | ✅ 20 | ⚠️ Conditional | ❌ None |
| | Anthropic | ✅ Complete | ⚠️ Partial | ⚠️ Conditional | ❌ None |
| | Bedrock | ✅ Complete | ✅ 20 | ⚠️ Conditional | ❌ None |
| | OpenRouter | ✅ Complete | ⚠️ Partial | ❌ None | ❌ None |
| **Integration** | BuiltIn | ✅ Complete | ✅ 15 | ❌ None | ❌ None |
| | N8n | ✅ Complete | ✅ 14 | ⚠️ Conditional | ❌ None |
| | Zapier | ✅ Complete | ✅ 17 | ⚠️ Conditional | ❌ None |

### Provider Test Gap Priorities

| Priority | Provider | Gap | Remediation Effort |
|----------|----------|-----|-------------------|
| 🔴 HIGH | Typesense | No tests | Create unit test suite (2 days) |
| 🔴 HIGH | Metabase | No tests | Create unit test suite (2 days) |
| 🟡 MED | OpenAI | Partial coverage | Complete unit tests (1 day) |
| 🟡 MED | Anthropic | Partial coverage | Complete unit tests (1 day) |
| 🟡 MED | OpenRouter | Partial coverage | Complete unit tests (1 day) |
| 🟢 LOW | All Providers | E2E tests | Create conditional E2E suite (5 days) |

---

## Appendix C: Gap Remediation Plan

### Phase 1: Critical Gaps (Week 1-2) - +10% Unit Coverage Target

#### 1.1 Auth Controller Tests (Priority: HIGH)
- **Target:** Create `AuthControllerTests.cs` with 25+ tests
- **Coverage Areas:**
  - Password setup flow (WF-AUTH-004)
  - Password reset flow (WF-AUTH-005)
  - 2FA enrollment (WF-AUTH-006)
  - Google OAuth callback (WF-AUTH-002)
- **Estimated Tests:** 25

#### 1.2 Contact Service Tests (Priority: HIGH)
- **Target:** Create `ContactsServiceTests.cs` with 30+ tests
- **Coverage Areas:**
  - Full CRUD coverage
  - Contact linking to accounts
  - Contact search
- **Estimated Tests:** 30

#### 1.3 Lead Service Enhancement (Priority: HIGH)
- **Target:** Expand `LeadServiceTests.cs` by 15 tests
- **Coverage Areas:**
  - Lead conversion workflow
  - Lead scoring with AI
  - Lead routing rules
- **Estimated Tests:** 15

### Phase 2: Medium Priority (Week 3-4) - +5% Integration Coverage

#### 2.1 Workflow Tests
- **Target:** Create `WorkflowServiceTests.cs` with 25 tests
- **Coverage Areas:**
  - Workflow execution engine
  - Workflow instance management
  - Task assignment
- **Estimated Tests:** 25

#### 2.2 Campaign Tests Enhancement
- **Target:** Expand `CampaignExecutionServiceTests.cs` by 20 tests
- **Coverage Areas:**
  - Email sequence execution
  - Campaign metrics
  - A/B testing
- **Estimated Tests:** 20

#### 2.3 Frontend Component Tests
- **Target:** Create 5 new test files with 50 total tests
- **Components:**
  - SetupPasswordPage.test.tsx (10 tests)
  - ForgotPasswordPage.test.tsx (10 tests)
  - TwoFactorSetupPage.test.tsx (10 tests)
  - ProfilePage.test.tsx (10 tests)
  - WorkflowDesigner.test.tsx (10 tests)
- **Estimated Tests:** 50

### Phase 3: Provider E2E Tests (Week 5-6) - +5% E2E Coverage

#### 3.1 Provider Workflow E2E Tests
- **Target:** Create conditional E2E tests for provider workflows
- **Test Files:**
  - `provider-search.spec.ts` (10 tests)
  - `provider-notification.spec.ts` (10 tests)
  - `provider-signature.spec.ts` (10 tests)
  - `provider-analytics.spec.ts` (10 tests)
- **Estimated Tests:** 40

#### 3.2 OAuth E2E Tests
- **Target:** Create OAuth flow tests
- **Test Files:**
  - `auth/oauth.spec.ts` (8 tests)
  - `auth/password-setup.spec.ts` (12 tests)
- **Estimated Tests:** 20

### Coverage Improvement Summary

| Phase | New Tests | Coverage Increase |
|-------|-----------|-------------------|
| Phase 1 | +70 | +10% Unit Tests |
| Phase 2 | +95 | +5% Integration |
| Phase 3 | +60 | +5% E2E |
| **TOTAL** | **+225** | **+10% Overall** |

---

## Appendix D: Test Environment Requirements

### Provider Test Configuration

| Provider | Test Mode | Required Config | Container/Service |
|----------|-----------|-----------------|-------------------|
| Meilisearch | Integration | `MEILISEARCH_URL`, `MEILISEARCH_KEY` | `docker-compose.providers.yml` |
| Chatwoot | Integration | `CHATWOOT_URL`, `CHATWOOT_API_KEY` | External or docker |
| Novu | Integration | `NOVU_API_KEY`, `NOVU_APP_ID` | External or docker |
| Twilio | Sandbox | `TWILIO_SID`, `TWILIO_TOKEN` | External (sandbox) |
| SendGrid | Sandbox | `SENDGRID_API_KEY` | External (sandbox) |
| Superset | Integration | `SUPERSET_URL`, `SUPERSET_USER` | `docker-compose.providers.yml` |
| DocuSeal | Integration | `DOCUSEAL_URL`, `DOCUSEAL_API_KEY` | `docker-compose.providers.yml` |
| DocuSign | Sandbox | `DOCUSIGN_*` credentials | External (sandbox) |
| Ollama | Local | `OLLAMA_URL` | `docker-compose.ollama.yml` |
| Azure OpenAI | Sandbox | `AZURE_OPENAI_*` credentials | External |

### Test Data Requirements

| Test Category | Data Requirements | Setup Script |
|---------------|-------------------|--------------|
| Account Tests | 5 test accounts | `scripts/seed-test-data.sh` |
| Contact Tests | 10 test contacts | `scripts/seed-test-data.sh` |
| Lead Tests | 5 test leads | `scripts/seed-test-data.sh` |
| Opportunity Tests | 3 test opportunities | `scripts/seed-test-data.sh` |
| Campaign Tests | 2 test campaigns | `scripts/seed-test-data.sh` |
| OAuth Tests | Test OAuth credentials | `scripts/setup-google-oauth.sh` |
| Provider Tests | Provider API keys | Manual or `config/*.env` |

---

*Document generated: February 6, 2026*  
*Version: 2.0.0*  
*Next review: February 13, 2026*

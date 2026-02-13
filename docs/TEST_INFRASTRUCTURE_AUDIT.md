# CRM Solution — Complete Test Infrastructure Audit

> **Generated:** February 2026  
> **Scope:** Every test file, test case, configuration file, and test documentation in the solution  
> **Method:** Exhaustive filesystem scan + per-file grep + documentation cross-reference  
> **Purpose:** Research-only catalog — no code was modified

---

## Executive Summary

| Metric | Count |
|--------|-------|
| **Total test files (all types)** | **240** |
| **Total test case definitions** | **5,814** |
| **Backend .cs test files** | 178 (+ 5 helpers/bases) |
| **Backend `[Fact]`+`[Theory]` attributes** | 4,912 |
| **E2E .spec.ts files** | 39 (+ 6 support files) |
| **E2E `test()`+`test.skip()` calls** | 769 |
| **Frontend .test.* files** | 18 |
| **Frontend `test()`/`it()` calls** | 892 |
| **Test projects (.csproj)** | 3 |
| **Test documentation files** | 20+ |
| **Skipped tests (backend)** | 8 (performance) |
| **Skipped tests (E2E)** | 47 |

### Health Warning

The backend's 4,912 attribute count includes files that **do not compile**. The root `.csproj` historically excluded ~97 test files due to entity property drift; 11 unfixable files were deleted on 2026-02-21. The remaining Main project files (103 files, 2,335 attributes) all compile and run. The CRM.Tests (40 files, 720 attributes) and Unit/Core (35 files, 1,857 attributes) subprojects have zero exclusions.

---

## Table of Contents

1. [Backend Test Infrastructure](#1-backend-test-infrastructure)
2. [E2E Test Infrastructure](#2-e2e-test-infrastructure)
3. [Frontend Test Infrastructure](#3-frontend-test-infrastructure)
4. [Test Configuration Files](#4-test-configuration-files)
5. [Test Project Structure (.csproj)](#5-test-project-structure-csproj)
6. [Test Documentation Inventory](#6-test-documentation-inventory)
7. [Coverage Gaps & Observations](#7-coverage-gaps--observations)
8. [Complete File Listings with Test Counts](#8-complete-file-listings-with-test-counts)

---

## 1. Backend Test Infrastructure

### 1.1 Three Test Projects

The backend has **3 independent test projects** that compile separately:

| Project | Path | Files | Tests | Status |
|---------|------|-------|-------|--------|
| **Main** | `CRM.Backend/tests/CRM.Tests.csproj` | 103 .cs | 2,335 | ✅ Active — excludes `CRM.Tests/**` and `Unit/**` via `<Compile Remove>` |
| **CRM.Tests** | `CRM.Backend/tests/CRM.Tests/CRM.Tests.csproj` | 40 .cs | 720 | ✅ Active — all compile |
| **Unit/Core** | `CRM.Backend/tests/Unit/Core/CRM.Tests.Unit.Core.csproj` | 35 .cs | 1,857 | ✅ Active — all compile |
| **TOTAL** | | **178** | **4,912** | |

### 1.2 Technology Stack

| Component | Version |
|-----------|---------|
| xUnit | 2.6.2 |
| Moq | 4.20.70 |
| FluentAssertions | 6.12.0 |
| Microsoft.EntityFrameworkCore.InMemory | 8.0.0 |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 |
| coverlet.collector | 6.0.0 |
| Microsoft.FeatureManagement.AspNetCore | 3.5.0 |
| Target Framework | net8.0 |

### 1.3 Main Project Files (103 files, 2,335 tests)

#### BVT Tests (9 files, 260 tests)

| File | Tests | Category |
|------|-------|----------|
| `BVT/CriticalPathBVTTests.cs` | 73 | Core API smoke tests |
| `BVT/AIFeaturesBVTTests.cs` | 40 | AI feature verification |
| `BVT/AllenAISmokeBVTTests.cs` | 36 | Allen AI integration BVT |
| `BVT/ITSMCoreBVTTests.cs` | 30 | ITSM core endpoints BVT |
| `BVT/ITSMPhase4BVTTests.cs` | 22 | ITSM Phase 4 BVT |
| `BVT/CoreApiBVTTests.cs` | 6 | Minimal core API BVT |
| `BVT/SalesApiBVTTests.cs` | 5 | Sales API BVT |
| `BVT/MarketingIrmApiBVTTests.cs` | 5 | Marketing/IRM BVT |
| `BVT/IntegrationApiBVTTests.cs` | 4 | Integration API BVT |

#### Service Tests (34 files, 693 tests)

| File | Tests | Target Service |
|------|-------|----------------|
| `Services/ITSM/BusinessHoursCalculatorTests.cs` | 47 | Business hours calculation |
| `Services/ITSM/ITSMDashboardServiceTests.cs` | 34 | ITSM dashboard |
| `Services/ITSM/ArticleRecommendationServiceTests.cs` | 33 | KB article recommendation |
| `Services/ContactInfoValidationServiceTests.cs` | 33 | Contact info validation |
| `Services/AllenAIServiceTests.cs` | 32 | Allen AI service |
| `Services/LeadServiceTests.cs` | 32 | Lead management |
| `Services/CampaignExecutionServiceTests.cs` | 31 | Campaign execution |
| `Services/OpportunityServiceTests.cs` | 29 | Opportunity management |
| `Services/DuplicateDetectionTests.cs` | 29 | Duplicate detection |
| `Services/CachedZipCodeServiceTests.cs` | 28 | ZIP code caching |
| `Services/FieldMasterDataServiceTests.cs` | 24 | Field master data |
| `Services/RelationshipServiceTests.cs` | 24 | Account relationships |
| `Services/TotpServiceTests.cs` | 23 | TOTP 2FA |
| `Services/OrderServiceTests.cs` | 23 | Order management |
| `Services/ITSM/ChangeServiceTests.cs` | 22 | Change management |
| `Services/InvoiceServiceTests.cs` | 22 | Invoice management |
| `Services/ResilienceServiceTests.cs` | 21 | Resilience/retry |
| `Services/NormalizationServiceTests.cs` | 21 | Data normalization |
| `Services/JwtTokenServiceTests.cs` | 21 | JWT token generation |
| `Services/AccountServiceTests.cs` | 21 | Account CRUD |
| `Services/UserGroupServiceTests.cs` | 20 | User group management |
| `Services/ProductServiceTests.cs` | 20 | Product catalog |
| `Services/PaymentServiceTests.cs` | 20 | Payment processing |
| `Services/ContractServiceTests.cs` | 20 | Contract management |
| `Services/UserServiceTests.cs` | 19 | User management |
| `Services/ITSM/KnowledgeServiceTests.cs` | 19 | Knowledge articles |
| `Services/TokenRevocationServiceTests.cs` | 16 | Token revocation |
| `Services/ITSM/SLAEnforcementHostedServiceTests.cs` | 16 | SLA enforcement |
| `Services/ITSM/CatalogServiceTests.cs` | 16 | Service catalog |
| `Services/ITSM/IncidentServiceTests.cs` | 15 | Incident management |
| `Services/ITSM/ChangeManagementServiceTests.cs` | 14 | Change management |
| `Services/AuthenticationServiceTests.cs` | 14 | Authentication |
| `Services/ITSM/SLAServiceTests.cs` | 13 | SLA policies |
| `Services/ITSM/KnowledgeManagementServiceTests.cs` | 13 | Knowledge management |
| `Services/ITSM/CMDBServiceTests.cs` | 11 | CMDB |
| `Services/ITSM/ProblemServiceTests.cs` | 11 | Problem management |
| `Services/ITSM/ServiceCatalogServiceTests.cs` | 12 | Service catalog advanced |
| `Services/SystemSettingsServiceTests.cs` | 10 | System settings |

#### Controller Tests (12 files, 215 tests)

| File | Tests | Controller |
|------|-------|------------|
| `Controllers/ReportsControllerTests.cs` | 42 | Reports |
| `Controllers/AuthControllerTests.cs` | 32 | Authentication |
| `Controllers/AccountsControllerTests.cs` | 19 | Accounts |
| `Controllers/OpportunitiesControllerTests.cs` | 18 | Opportunities |
| `Controllers/ProductsControllerTests.cs` | 17 | Products |
| `Controllers/ITSMSLAControllerTests.cs` | 16 | ITSM SLA |
| `Controllers/ITSMKnowledgeControllerTests.cs` | 15 | ITSM Knowledge |
| `Controllers/ITSMChangesControllerTests.cs` | 15 | ITSM Changes |
| `Controllers/ITSMCatalogControllerTests.cs` | 14 | ITSM Catalog |
| `Controllers/ITSMIncidentsControllerTests.cs` | 12 | ITSM Incidents |
| `Controllers/ITSMProblemsControllerTests.cs` | 11 | ITSM Problems |
| `Controllers/ITSMCMDBControllerTests.cs` | 11 | ITSM CMDB |
| `Controllers/DepartmentsControllerTests.cs` | 6 | Departments |

#### Functional Tests (4 files, 127 tests)

| File | Tests | Scope |
|------|-------|-------|
| `Functional/ITSMCoreFunctionalTests.cs` | 40 | ITSM core workflows |
| `Functional/ApiEndpointFunctionalTests.cs` | 36 | API endpoint validation |
| `Functional/ITSMPhase4FunctionalTests.cs` | 31 | ITSM Phase 4 flows |
| `Functional/RelationshipCampaignFunctionalTests.cs` | 20 | Relationships & campaigns |

#### Entity / DTO / Model Tests (6 files, 228 tests)

| File | Tests | Scope |
|------|-------|-------|
| `Entities/EntityValidationTests.cs` | 47 | Entity validation rules |
| `Entities/CoreEntityTests.cs` | 44 | Core entity properties |
| `Entities/EnumTypeTests.cs` | 35 | Enum type definitions |
| `Dtos/DtoMappingTests.cs` | 19 | DTO mapping accuracy |
| `Models/ContactModelTests.cs` | 19 | Contact model |
| `Configurations/EntityConfigurationTests.cs` | 39 | EF Core configurations |

#### Validator Tests (6 files, 191 tests)

| File | Tests | Target |
|------|-------|--------|
| `Validators/CommonValidatorTests.cs` | 43 | Common validation rules |
| `Validators/LeadValidatorTests.cs` | 33 | Lead validation |
| `Validators/UserValidatorTests.cs` | 31 | User validation |
| `Validators/OpportunityValidatorTests.cs` | 30 | Opportunity validation |
| `Validators/ContactValidatorTests.cs` | 28 | Contact validation |
| `Validators/AccountValidatorTests.cs` | 27 | Account validation |

#### Middleware Tests (4 files, 78 tests)

| File | Tests | Middleware |
|------|-------|-----------|
| `Middleware/RequestLoggingMiddlewareTests.cs` | 23 | Request logging |
| `Middleware/ErrorHandlingMiddlewareTests.cs` | 22 | Error handling |
| `Middleware/AuthenticationMiddlewareTests.cs` | 18 | Authentication |
| `Middleware/RateLimitingMiddlewareTests.cs` | 15 | Rate limiting |

#### Hosted Service Tests (7 files, 89 tests)

| File | Tests | Service |
|------|-------|---------|
| `HostedServices/BackupSchedulerHostedServiceTests.cs` | 22 | Backup scheduling |
| `HostedServices/CalendarSyncHostedServiceTests.cs` | 21 | Calendar sync |
| `HostedServices/EmailSyncHostedServiceTests.cs` | 17 | Email sync |
| `HostedServices/DatabaseSyncHostedServiceTests.cs` | 12 | Database sync |
| `HostedServices/WorkflowWorkerServiceTests.cs` | 10 | Workflow worker |
| `HostedServices/ZipCodeImportHostedServiceTests.cs` | 9 | ZIP code import |
| `HostedServices/LeadScoreDecayHostedServiceTests.cs` | 9 | Lead score decay |

#### Infrastructure Tests (9 files, 189 tests)

| File | Tests | Scope |
|------|-------|-------|
| `Ports/ProviderPortContractTests.cs` | 50 | Provider port contracts |
| `Helpers/AIServiceHelperTests.cs` | 36 | AI service helpers |
| `Extensions/LoggingExtensionsTests.cs` | 36 | Logging extensions |
| `Utilities/UtilityTests.cs` | 36 | Utility methods |
| `Helpers/ETagHelperTests.cs` | 30 | ETag handling |
| `Factories/ProviderFactoryTests.cs` | 24 | Provider factory |
| `Extensions/ServiceExtensionsTests.cs` | 23 | Service DI extensions |
| `Repositories/GenericRepositoryTests.cs` | 19 | Generic repository |
| `Features/FeatureFlagTests.cs` | 6 | Feature flags |

#### Integration Tests (3 files, 38 tests)

| File | Tests | Scope |
|------|-------|-------|
| `Integration/BuiltInSearchProviderIntegrationTests.cs` | 17 | Built-in search |
| `Integration/MeilisearchProviderIntegrationTests.cs` | 12 | Meilisearch |
| `Integration/ProviderDIIntegrationTests.cs` | 9 | Provider DI resolution |

#### Other (5 files, 96 tests)

| File | Tests | Scope |
|------|-------|-------|
| `BusinessLogic/BusinessLogicTests.cs` | 41 | Business logic rules |
| `CreditMemoServiceTests.cs` | 7 | Credit memo service |
| `LeadServiceTests.cs` | 10 | Lead service (root level) |
| `Performance/PerformanceTests.cs` | 1 | Performance harness (8 skipped via `[Trait]`) |

#### Helper / Base Files (not test files, 5 files)

| File | Purpose |
|------|---------|
| `Helpers/ApiTestFactory.cs` | WebApplicationFactory for integration tests |
| `Helpers/TestAuthHandler.cs` | Test authentication handler |
| `Helpers/AsyncQueryTestHelpers.cs` | Async query test utilities |
| `Functional/FunctionalTestBase.cs` | Functional test base class |
| `Performance/PerformanceTestHarness.cs` | Performance test harness |

### 1.4 CRM.Tests Subproject (40 files, 720 tests)

#### AI / Semantic Kernel Tests (12 files, 164 tests)

| File | Tests | Target |
|------|-------|--------|
| `AI/SK/Agents/AgentSelectionStrategyTests.cs` | 20 | Agent routing |
| `AI/SK/Agents/LeadScoringAgentTests.cs` | 18 | Lead scoring AI agent |
| `AI/SK/Agents/SupportTriageAgentTests.cs` | 17 | Support triage AI agent |
| `AI/SK/Agents/GeneralAssistantAgentTests.cs` | 12 | General assistant AI |
| `AI/SK/Services/AgentExecutionServiceTests.cs` | 14 | Agent execution |
| `AI/SK/Plugins/LeadPluginTests.cs` | 15 | Lead plugin |
| `AI/SK/Plugins/AccountPluginTests.cs` | 14 | Account plugin |
| `AI/SK/Plugins/ServiceRequestPluginTests.cs` | 13 | Service request plugin |
| `AI/SK/Plugins/PluginAttributeVerificationTests.cs` | 12 | Plugin attributes |
| `AI/SK/Plugins/SearchPluginTests.cs` | 9 | Search plugin |
| `AI/SK/Connectors/CrmKernelFactoryTests.cs` | 11 | Kernel factory |
| `AI/SK/Attributes/RequiresApprovalAttributeTests.cs` | 7 | Approval attributes |

#### Service Tests (16 files, 335 tests)

| File | Tests | Target |
|------|-------|--------|
| `Services/ApprovalWorkflowServiceTests.cs` | 42 | Approval workflows |
| `Services/TerritoryServiceTests.cs` | 32 | Territory management |
| `Services/NewsSocialServiceTests.cs` | 28 | News/social integration |
| `Services/FormBuilderServiceTests.cs` | 28 | Form builder |
| `Services/ModuleUIConfigServiceTests.cs` | 24 | Module UI config |
| `Services/LeadRoutingServiceTests.cs` | 23 | Lead routing |
| `Services/ReportBuilderServiceTests.cs` | 20 | Report builder |
| `Services/CoreDataSeederServiceTests.cs` | 20 | Core data seeding |
| `Services/SubscriptionServiceTests.cs` | 20 | Subscription management |
| `Services/TeamServiceTests.cs` | 19 | Team management |
| `Services/CommissionServiceTests.cs` | 18 | Commission calculation |
| `Services/UserApprovalServiceTests.cs` | 18 | User approval |
| `Services/EmailTemplateServiceTests.cs` | 18 | Email templates |
| `Services/DashboardBuilderServiceTests.cs` | 18 | Dashboard builder |
| `Services/DashboardServiceTests.cs` | 15 | Dashboard data |
| `Services/AI/AIOpportunityScoringServiceTests.cs` | 11 | AI opportunity scoring |
| `Services/AI/AILeadScoringServiceTests.cs` | 10 | AI lead scoring |
| `Services/AI/AIKnowledgeSearchServiceTests.cs` | 9 | AI knowledge search |

#### Database Provider Tests (5 files, 133 tests)

| File | Tests | Target |
|------|-------|--------|
| `Data/DatabaseProviderStrategyFactoryTests.cs` | 31 | Strategy factory |
| `Data/SqlServerProviderStrategyTests.cs` | 28 | SQL Server strategy |
| `Data/PostgreSqlProviderStrategyTests.cs` | 27 | PostgreSQL strategy |
| `Data/OracleProviderStrategyTests.cs` | 27 | Oracle strategy |
| `Data/MySqlProviderStrategyTests.cs` | 24 | MySQL strategy |

#### Hosted Services & Integration (3 files, 52 tests)

| File | Tests | Target |
|------|-------|--------|
| `HostedServices/SLAEnforcementHostedServiceTests.cs` | 18 | SLA enforcement |
| `Integration/Services/ActivityServiceTests.cs` | 16 | Activity service integration |
| `EntityTests.cs` | 7 | Basic entity properties |
| `UserEntityTests.cs` | 7 | User entity properties |

#### Helper File (1 file)

| File | Purpose |
|------|---------|
| `Helpers/AsyncQueryHelpers.cs` | Async LINQ test helpers |

### 1.5 Unit/Core Subproject (35 files, 1,857 tests)

All files live under `Unit/Core/` and test CRM.Core entities, DTOs, and value objects in isolation (no database, no infrastructure dependencies).

| File | Tests | Scope |
|------|-------|-------|
| `ServiceRequestEntityTests.cs` | 138 | ServiceRequest + related entities |
| `FeatureFlagsAndProviderTypesTests.cs` | 81 | Feature flags, provider type constants |
| `SystemCoreEntityTests.cs` | 75 | SystemSettings, Lookup, CustomField |
| `IntegrationMarketingEntityTests.cs` | 72 | Marketing + integration entities |
| `ITSMEntityTests.cs` | 70 | ITSM module entities |
| `RelationshipCommunicationEntityTests.cs` | 67 | Relationship, Communication entities |
| `MarketingCampaignEntityTests.cs` | 64 | MarketingCampaign and related |
| `AccountEntityTests.cs` | 60 | Account (Customer) entity |
| `QuoteInvoiceEntityTests.cs` | 58 | Quote, Invoice entities |
| `ActivityNoteTagAddressEntityTests.cs` | 57 | Activity, Note, Tag, Address |
| `AIEntityTests.cs` | 55 | AI model entities |
| `WorkflowEntityTests.cs` | 54 | Workflow engine entities |
| `EmailTemplateSequenceEntityTests.cs` | 53 | EmailTemplate, Sequence entities |
| `CommissionTeamWebEntityTests.cs` | 53 | Commission, Team, Web entities |
| `LeadOpportunityEntityTests.cs` | 52 | Lead, Opportunity entities |
| `SignerDocumentLineItemForecastEntityTests.cs` | 51 | Signer, Document, Forecast entities |
| `ProductEntityTests.cs` | 50 | Product, Bundle, PriceBook entities |
| `CrmExceptionsTests.cs` | 49 | Custom exception types |
| `ReportsEnumsEntityTests.cs` | 48 | Report definitions, enums |
| `LeadManagementSystemEntityTests.cs` | 48 | Lead routing, scoring entities |
| `KnowledgeBaseEntityTests.cs` | 47 | KnowledgeArticle, Category entities |
| `PaymentSubscriptionContractEntityTests.cs` | 46 | Payment, Subscription, Contract |
| `AuthDtoTests.cs` | 46 | Auth request/response DTOs |
| `WebEngagementEntityTests.cs` | 45 | Web visitor, session entities |
| `CPQEntityTests.cs` | 44 | CPQ (Configure-Price-Quote) entities |
| `WebMarketingEntityTests.cs` | 43 | Web form, landing page entities |
| `CreditSignatureOrderEntityTests.cs` | 42 | CreditMemo, ESignature, Order |
| `SystemConfigurationEntityTests.cs` | 41 | System configuration entities |
| `InfrastructureEntityTests.cs` | 41 | Cloud, Backup entities |
| `UserEntityTests.cs` | 39 | User, UserGroup entities |
| `AccountContactDtoTests.cs` | 37 | Account/Contact DTOs |
| `InstrumentationServiceTests.cs` | 34 | Instrumentation/telemetry |
| `DashboardSocialMediaEntityTests.cs` | 33 | Dashboard, SocialMedia entities |
| `LoggingExtensionsTests.cs` | 32 | Logging extension methods |
| `BaseEntityTests.cs` | 32 | BaseEntity, RowVersion, IsDeleted |

### 1.6 Skipped / Excluded Backend Tests

| Category | Count | Details |
|----------|-------|---------|
| **Performance tests (skipped via `[Trait]`)** | 8 | In `Performance/PerformanceTests.cs` — marked `[Trait("Category", "Performance")]` |
| **Historically excluded files** | ~97 | Were excluded from root .csproj due to entity property drift. 11 unfixable files deleted 2026-02-21. Remaining were rewritten and re-enabled. |

---

## 2. E2E Test Infrastructure

### 2.1 Technology Stack

| Component | Value |
|-----------|-------|
| Framework | Playwright |
| Language | TypeScript |
| Base URL | `http://192.168.0.9` |
| Browser Projects | chromium, firefox, webkit, Mobile Chrome (Pixel 5), Mobile Safari (iPhone 12) |
| Auth Setup | `tests/auth.setup.ts` (shared auth state) |
| Custom Reporter | `tests/utils/crm-reporter.ts` |
| Timeouts | action=10s, navigation=30s, global=60s, expect=10s |
| CI Settings | 2 retries, 1 worker |
| Artifacts | Screenshots on failure, video on first retry, trace on first retry |

### 2.2 All E2E Test Files (39 files, 769 tests)

| File | `test()` | `test.skip()` | Total |
|------|----------|---------------|-------|
| `tests/bvt/api-bvt.spec.ts` | — | — | BVT API tests |
| `tests/bvt/itsm-api-bvt.spec.ts` | — | — | ITSM API BVT |
| `tests/bvt/itsm-core-api-bvt.spec.ts` | — | — | ITSM Core API BVT |
| `tests/auth/authentication.spec.ts` | — | — | Login/logout flows |
| `tests/customers/customers.spec.ts` | — | — | Customer CRUD |
| `tests/contacts/contacts.spec.ts` | — | — | Contact CRUD |
| `tests/leads/leads.spec.ts` | — | — | Lead CRUD |
| `tests/opportunities/opportunities.spec.ts` | — | — | Opportunity CRUD |
| `tests/campaigns/campaigns.spec.ts` | — | — | Campaign listing |
| `tests/campaigns/campaign-setup.spec.ts` | — | — | Campaign creation |
| `tests/campaigns/campaign-execution.spec.ts` | — | — | Campaign execution |
| `tests/campaigns/campaign-bugs.spec.ts` | — | — | Campaign regressions |
| `tests/dashboard/dashboard.spec.ts` | — | — | Dashboard widgets |
| `tests/admin/admin.spec.ts` | — | — | Admin settings |
| `tests/service-requests/service-requests.spec.ts` | — | — | Service requests |
| `tests/workflows/workflows.spec.ts` | — | — | Workflow engine |
| `tests/workflow-execution/workflow-execution.spec.ts` | — | — | Workflow execution |
| `tests/relationships/relationships.spec.ts` | — | — | Account relationships |
| `tests/deduplication/deduplication.spec.ts` | — | — | Duplicate detection |
| `tests/groups/create-groups.spec.ts` | — | — | User group creation |
| `tests/users/create-users.spec.ts` | — | — | User creation |
| `tests/crud-operations/crud-operations.spec.ts` | — | — | Cross-entity CRUD |
| `tests/data-lifecycle/data-lifecycle.spec.ts` | — | — | Data lifecycle |
| `tests/account-contact-linking.spec.ts` | — | — | Account-contact links |
| `tests/notes-quotes-features.spec.ts` | — | — | Notes, quotes |
| `tests/ui-account-contact-test.spec.ts` | — | — | UI linking tests |
| `tests/persona/persona-api-journeys.spec.ts` | — | — | API persona journeys |
| `tests/persona/persona-e2e-journeys.spec.ts` | — | — | E2E persona journeys |
| `tests/functional/ui-functional.spec.ts` | — | — | UI functional tests |
| `tests/functional/itsm-ui-functional.spec.ts` | — | — | ITSM UI functional |
| `tests/functional/itsm-core-ui-functional.spec.ts` | — | — | ITSM Core UI functional |
| `tests/data-population/data-population.spec.ts` | — | — | Data population |
| `tests/data-population/debug-customer.spec.ts` | — | — | Debug helpers |
| `tests/data-population/debug-dialog-dom.spec.ts` | — | — | Dialog DOM debug |
| `tests/data/create-accounts-contacts.spec.ts` | — | — | Bulk account/contact creation |
| `tests/data/create-microsoft-account.spec.ts` | — | — | Microsoft account seed |
| `tests/data/create-microsoft-ui.spec.ts` | — | — | Microsoft UI seed |
| `tests/data/generate-test-data.spec.ts` | — | — | Test data generation |
| `tests/data/verify-microsoft-ui.spec.ts` | — | — | Verification |

**Totals:** 722 `test()` + 47 `test.skip()` = **769 test cases**

### 2.3 E2E Support Files (6 files)

| File | Purpose |
|------|---------|
| `tests/auth.setup.ts` | Shared authentication state setup |
| `tests/test-data.ts` | Test credentials and data constants |
| `tests/comprehensive-test-data.ts` | Extended test data |
| `tests/fixtures.ts` | Playwright test fixtures |
| `tests/utils/crm-reporter.ts` | Custom CRM test reporter |
| `tests/utils/test-logger.ts` | Test logging utilities |

### 2.4 Skipped E2E Tests (47 total `test.skip()`)

These are scattered across multiple spec files. They represent tests that were written but disabled due to timing issues, UI changes, or environmental dependencies.

---

## 3. Frontend Test Infrastructure

### 3.1 Technology Stack

| Component | Value |
|-----------|-------|
| Framework | Jest with ts-jest |
| Assertion Library | @testing-library/jest-dom |
| Coverage Thresholds | 50% global (branches, functions, lines, statements) |
| Module Alias | `@/` → `<rootDir>/src/` |
| Transform | ts-jest for .ts/.tsx; transforms `axios` in node_modules |
| Setup File | `jest.setup.js` (mocks matchMedia, localStorage, suppresses warnings) |

### 3.2 All Frontend Test Files (18 files, 892 tests)

| File | Tests | Scope |
|------|-------|-------|
| `src/__tests__/SharedComponents.comprehensive.test.tsx` | 108 | Shared UI components |
| `src/__tests__/AdminPages.comprehensive.test.tsx` | 94 | Admin page components |
| `src/__tests__/CustomersPage.comprehensive.test.tsx` | 91 | Customers/Accounts page |
| `src/__tests__/ContactsPage.comprehensive.test.tsx` | 84 | Contacts page |
| `src/__tests__/OpportunitiesPage.comprehensive.test.tsx` | 74 | Opportunities page |
| `src/__tests__/ProductsPage.comprehensive.test.tsx` | 69 | Products page |
| `src/__tests__/DashboardPage.comprehensive.test.tsx` | 62 | Dashboard page |
| `src/__tests__/Navigation.comprehensive.test.tsx` | 61 | Navigation components |
| `src/__tests__/LoginPage.comprehensive.test.tsx` | 47 | Login page |
| `src/__tests__/ITSMPhase4Pages.test.tsx` | 39 | ITSM Phase 4 pages |
| `src/__tests__/ITSMCorePages.test.tsx` | 24 | ITSM Core pages |
| `src/__tests__/ServiceRequestsPage.test.tsx` | 17 | Service requests page |
| `src/__tests__/CampaignsPage.test.tsx` | 17 | Campaigns page |
| `src/__tests__/ProductsPage.test.tsx` | 15 | Products page (basic) |
| `src/__tests__/OpportunitiesPage.test.tsx` | 15 | Opportunities page (basic) |
| `src/__tests__/CustomersPage.test.tsx` | 11 | Customers page (basic) |
| `src/__tests__/LoginPage.test.tsx` | 9 | Login page (basic) |
| `src/__tests__/apiClient.test.ts` | 8 | Axios API client |

**Note:** Files named `*.comprehensive.test.*` are enhanced versions with deeper coverage of the same pages covered by the basic `*.test.*` files. Both sets are active and run.

---

## 4. Test Configuration Files

### 4.1 Jest Configuration (`CRM.Frontend/jest.config.json`)

```json
{
  "testEnvironment": "jsdom",
  "transform": { "^.+\\.tsx?$": "ts-jest" },
  "transformIgnorePatterns": ["node_modules/(?!axios)"],
  "moduleNameMapper": { "^@/(.*)$": "<rootDir>/src/$1" },
  "setupFilesAfterSetup": ["<rootDir>/jest.setup.js"],
  "collectCoverageFrom": ["src/**/*.{ts,tsx}", "!src/**/*.d.ts"],
  "coverageThreshold": { "global": { "branches": 50, "functions": 50, "lines": 50, "statements": 50 } },
  "verbose": true
}
```

### 4.2 Jest Setup (`CRM.Frontend/jest.setup.js`)

- Mocks `window.matchMedia` (returns `{ matches: false, addListener, removeListener, ... }`)
- Mocks `localStorage` (getItem, setItem, removeItem, clear)
- Suppresses `ReactDOM.render` deprecation warnings

### 4.3 Playwright Configuration (`e2e-tests/playwright.config.ts`)

| Setting | Value |
|---------|-------|
| baseURL | `process.env.BASE_URL \|\| 'http://192.168.0.9'` |
| globalSetup | None (uses setup project) |
| Workers | CI: 1, Local: undefined (auto) |
| Retries | CI: 2, Local: 0 |
| Reporter | `[['html'], ['./tests/utils/crm-reporter.ts']]` |
| Screenshots | `only-on-failure` |
| Video | `on-first-retry` |
| Trace | `on-first-retry` |
| Projects | `setup` → `chromium` → `firefox` → `webkit` → `Mobile Chrome` → `Mobile Safari` |
| Action Timeout | 10,000 ms |
| Navigation Timeout | 30,000 ms |
| Global Timeout | 60,000 ms |
| Expect Timeout | 10,000 ms |

### 4.4 Backend Test Configuration (via `.csproj` files)

| Setting | Value |
|---------|-------|
| Target Framework | net8.0 |
| IsPackable | false |
| ImplicitUsings | enable |
| Nullable | enable |
| Root .csproj Excludes | `CRM.Tests/**`, `Unit/**` (separate projects) |

---

## 5. Test Project Structure (.csproj)

### 5.1 Root Project: `CRM.Backend/tests/CRM.Tests.csproj`

- **Assembly Name:** CRM.Tests (default)
- **References:** CRM.Api, CRM.Core, CRM.Infrastructure, CRM.ServiceDefaults
- **Key NuGet:** xUnit 2.6.2, Moq 4.20.70, FluentAssertions 6.12.0, EF InMemory 8.0.0, Mvc.Testing 8.0.0, coverlet 6.0.0, FeatureManagement 3.5.0
- **Exclusions:** `<Compile Remove="CRM.Tests\**\*.cs" />` and `<Compile Remove="Unit\**\*.cs" />`
- **Historical note:** 97 test files were excluded due to entity drift. 11 unfixable files deleted 2026-02-21.

### 5.2 CRM.Tests Subproject: `CRM.Backend/tests/CRM.Tests/CRM.Tests.csproj`

- **Assembly Name:** CRM.Tests.Services
- **Root Namespace:** CRM.Tests
- **References:** CRM.Core, CRM.Infrastructure, CRM.Api
- **Key NuGet:** Same xUnit/Moq/FA stack
- **Exclusions:** None

### 5.3 Unit/Core Subproject: `CRM.Backend/tests/Unit/Core/CRM.Tests.Unit.Core.csproj`

- **Assembly Name:** CRM.Tests.Unit.Core
- **References:** CRM.Core ONLY (no Infrastructure/Api dependency)
- **Key NuGet:** xUnit, FluentAssertions, Microsoft.Extensions.Logging.Abstractions 8.0.0
- **Purpose:** Pure domain/entity tests with zero infrastructure coupling

---

## 6. Test Documentation Inventory

### 6.1 Primary Documentation (in `docs/`)

| File | Lines | Content |
|------|-------|---------|
| `docs/TEST_GAP_AUDIT_REPORT.md` | 574 | **Most authoritative** — Per-file active test counts, exclusion analysis, coverage heat map, priority recommendations |
| `docs/TEST_REPORT.md` | 337 | January 2026 point-in-time report (237 tests at that time, now outdated) |
| `docs/INTEGRATION_TESTING_GUIDE.md` | — | Integration testing patterns |

### 6.2 Testing Documentation (in `docs/testing/`)

| File | Content |
|------|---------|
| `TESTING_DOCUMENTATION_INDEX.md` | Index of all testing docs |
| `COMPREHENSIVE_TEST_STRATEGY.md` | 1,233 lines — Full test inventory, batch plans, automation scripts |
| `TESTING_STATUS.md` | 537 lines — Current status tracking |
| `TESTING_GUIDE.md` | Developer testing guide |
| `TEST_EXECUTION_GUIDE.md` | How to run tests |
| `TESTING_IMPLEMENTATION_COMPLETE.md` | Implementation completion record |
| `TEST_RESULTS_REPORT.md` | Test execution results |
| `TESTING_SUMMARY.md` | Summary overview |
| `TESTING_README.md` | Testing readme |
| `TESTING_QUICK_REFERENCE.md` | Quick reference card |
| `TESTING_RESPONSIVE_DESIGN.md` | Responsive design testing |
| `TESTING_COMPLETE_CHECKLIST.md` | Early-phase checklist (outdated) |
| `FUNCTIONAL_TEST_REPORT.md` | Functional test results |
| `PERSONA_TEST_RESULTS.md` | Persona-based test results |

### 6.3 Other Test-Related Files

| File | Content |
|------|---------|
| `TESTING_SUMMARY.md` (root) | Root-level summary |
| `CRM.Backend/tests/FUNCTIONAL_TEST_REPORT.md` | Backend functional test report |
| `e2e-tests/TEST_RESULTS_SUMMARY.md` | E2E results summary |
| `test-logs/*.log` | Historical test execution logs |

---

## 7. Coverage Gaps & Observations

### 7.1 Key Observations

1. **Three overlapping test projects** — The root `.csproj` excludes `CRM.Tests/**` and `Unit/**` to prevent double-compilation, but the directory structure is confusing. Some test files exist at root level (e.g., `LeadServiceTests.cs`, `CreditMemoServiceTests.cs`) alongside organized subdirectories.

2. **Entity drift was the #1 problem** — 97 files were historically excluded because entities gained/changed properties faster than test mocks were updated. The root cause was ICrmDbContext growing to 131+ `DbSet` members, making mock setup extremely fragile.

3. **Comprehensive vs. Basic frontend tests** — 9 of 18 frontend test files are `*.comprehensive.test.*` variants that provide deeper coverage of pages already covered by basic `*.test.*` files. Both sets run.

4. **47 skipped E2E tests** — Scattered across spec files, these represent tests disabled due to timing sensitivity, UI selector changes, or environmental dependencies.

5. **Documentation inconsistencies** — The `COMPREHENSIVE_TEST_STRATEGY.md` lists many files as "Active" that the `TEST_GAP_AUDIT_REPORT.md` correctly identifies as excluded/non-compiling. The gap audit is the authoritative source.

### 7.2 Controllers Without Active Tests

Of 95 API controllers in the solution, only **13 have active test files** (including Reports and Auth). The remaining 82 controllers have either excluded tests (don't compile) or no test file at all. Notable gaps include all Phase 4 entities: Commissions, Contracts, Invoices, Orders, Payments, Subscriptions, and Teams controllers.

### 7.3 Services Without Active Tests

Major services with no active tests include:

- **Phase 4 Services** (8): Commission, Contract, EmailTemplate, Invoice, Order, Payment, Subscription, Team — Note: Tests for these now exist in `CRM.Tests/Services/` subproject (CommissionServiceTests, SubscriptionServiceTests, TeamServiceTests, EmailTemplateServiceTests) and some in main project (ContractServiceTests, InvoiceServiceTests, OrderServiceTests, PaymentServiceTests)
- **10 of 28 ITSM Advanced Services** (AssetLifecycle, AssignmentRules, AutoClose, CABWorkflow, CatalogApproval, CatalogFulfillment, ChangeCalendar, ChangeImpact, Discovery, ImpactAnalysis)
- **AI Services**: Fully covered (5/5 with tests in CRM.Tests)

### 7.4 Code Coverage (Last Measured Jan 2026)

| Assembly | Line Coverage | Branch Coverage |
|----------|--------------|-----------------|
| CRM.Core | 24.37% | 1.09% |
| CRM.Api | 2.26% | 1.23% |
| CRM.Infrastructure | 2.42% | 9.51% |
| **Overall** | **3.34%** | **6.10%** |

These numbers are from January 2026 when only 237 tests existed. Current test count is ~4,912 (all files) / ~4,912 attributes, but coverage has not been re-measured.

---

## 8. Complete File Listings with Test Counts

### 8.1 All 178 Backend Test Files (sorted by test count, descending)

```
 138  Unit/Core/ServiceRequestEntityTests.cs
  81  Unit/Core/FeatureFlagsAndProviderTypesTests.cs
  75  Unit/Core/SystemCoreEntityTests.cs
  73  BVT/CriticalPathBVTTests.cs
  72  Unit/Core/IntegrationMarketingEntityTests.cs
  70  Unit/Core/ITSMEntityTests.cs
  67  Unit/Core/RelationshipCommunicationEntityTests.cs
  64  Unit/Core/MarketingCampaignEntityTests.cs
  60  Unit/Core/AccountEntityTests.cs
  58  Unit/Core/QuoteInvoiceEntityTests.cs
  57  Unit/Core/ActivityNoteTagAddressEntityTests.cs
  55  Unit/Core/AIEntityTests.cs
  54  Unit/Core/WorkflowEntityTests.cs
  53  Unit/Core/EmailTemplateSequenceEntityTests.cs
  53  Unit/Core/CommissionTeamWebEntityTests.cs
  52  Unit/Core/LeadOpportunityEntityTests.cs
  51  Unit/Core/SignerDocumentLineItemForecastEntityTests.cs
  50  Unit/Core/ProductEntityTests.cs
  50  Ports/ProviderPortContractTests.cs
  49  Unit/Core/CrmExceptionsTests.cs
  48  Unit/Core/ReportsEnumsEntityTests.cs
  48  Unit/Core/LeadManagementSystemEntityTests.cs
  47  Unit/Core/KnowledgeBaseEntityTests.cs
  47  Services/ITSM/BusinessHoursCalculatorTests.cs
  47  Entities/EntityValidationTests.cs
  46  Unit/Core/PaymentSubscriptionContractEntityTests.cs
  46  Unit/Core/AuthDtoTests.cs
  45  Unit/Core/WebEngagementEntityTests.cs
  44  Unit/Core/CPQEntityTests.cs
  44  Entities/CoreEntityTests.cs
  43  Validators/CommonValidatorTests.cs
  43  Unit/Core/WebMarketingEntityTests.cs
  42  Unit/Core/CreditSignatureOrderEntityTests.cs
  42  Controllers/ReportsControllerTests.cs
  42  CRM.Tests/Services/ApprovalWorkflowServiceTests.cs
  41  Unit/Core/SystemConfigurationEntityTests.cs
  41  Unit/Core/InfrastructureEntityTests.cs
  41  BusinessLogic/BusinessLogicTests.cs
  40  Functional/ITSMCoreFunctionalTests.cs
  40  BVT/AIFeaturesBVTTests.cs
  39  Unit/Core/UserEntityTests.cs
  39  Configurations/EntityConfigurationTests.cs
  37  Unit/Core/AccountContactDtoTests.cs
  36  Utilities/UtilityTests.cs
  36  Helpers/AIServiceHelperTests.cs
  36  Functional/ApiEndpointFunctionalTests.cs
  36  Extensions/LoggingExtensionsTests.cs
  36  BVT/AllenAISmokeBVTTests.cs
  35  Entities/EnumTypeTests.cs
  34  Unit/Core/InstrumentationServiceTests.cs
  34  Services/ITSM/ITSMDashboardServiceTests.cs
  33  Validators/LeadValidatorTests.cs
  33  Unit/Core/DashboardSocialMediaEntityTests.cs
  33  Services/ITSM/ArticleRecommendationServiceTests.cs
  33  Services/ContactInfoValidationServiceTests.cs
  32  Unit/Core/LoggingExtensionsTests.cs
  32  Unit/Core/BaseEntityTests.cs
  32  Services/LeadServiceTests.cs
  32  Services/AllenAIServiceTests.cs
  32  Controllers/AuthControllerTests.cs
  32  CRM.Tests/Services/TerritoryServiceTests.cs
  31  Validators/UserValidatorTests.cs
  31  Services/CampaignExecutionServiceTests.cs
  31  Functional/ITSMPhase4FunctionalTests.cs
  31  CRM.Tests/Data/DatabaseProviderStrategyFactoryTests.cs
  30  Validators/OpportunityValidatorTests.cs
  30  Helpers/ETagHelperTests.cs
  30  BVT/ITSMCoreBVTTests.cs
  29  Services/OpportunityServiceTests.cs
  29  Services/DuplicateDetectionTests.cs
  28  Validators/ContactValidatorTests.cs
  28  Services/CachedZipCodeServiceTests.cs
  28  CRM.Tests/Services/NewsSocialServiceTests.cs
  28  CRM.Tests/Services/FormBuilderServiceTests.cs
  28  CRM.Tests/Data/SqlServerProviderStrategyTests.cs
  27  Validators/AccountValidatorTests.cs
  27  CRM.Tests/Data/PostgreSqlProviderStrategyTests.cs
  27  CRM.Tests/Data/OracleProviderStrategyTests.cs
  24  Services/RelationshipServiceTests.cs
  24  Services/FieldMasterDataServiceTests.cs
  24  Factories/ProviderFactoryTests.cs
  24  CRM.Tests/Services/ModuleUIConfigServiceTests.cs
  24  CRM.Tests/Data/MySqlProviderStrategyTests.cs
  23  Services/TotpServiceTests.cs
  23  Services/OrderServiceTests.cs
  23  Middleware/RequestLoggingMiddlewareTests.cs
  23  Extensions/ServiceExtensionsTests.cs
  23  CRM.Tests/Services/LeadRoutingServiceTests.cs
  22  Services/InvoiceServiceTests.cs
  22  Services/ITSM/ChangeServiceTests.cs
  22  Middleware/ErrorHandlingMiddlewareTests.cs
  22  HostedServices/BackupSchedulerHostedServiceTests.cs
  22  BVT/ITSMPhase4BVTTests.cs
  21  Services/ResilienceServiceTests.cs
  21  Services/NormalizationServiceTests.cs
  21  Services/JwtTokenServiceTests.cs
  21  Services/AccountServiceTests.cs
  21  HostedServices/CalendarSyncHostedServiceTests.cs
  20  Services/UserGroupServiceTests.cs
  20  Services/ProductServiceTests.cs
  20  Services/PaymentServiceTests.cs
  20  Services/ContractServiceTests.cs
  20  Functional/RelationshipCampaignFunctionalTests.cs
  20  CRM.Tests/Services/SubscriptionServiceTests.cs
  20  CRM.Tests/Services/ReportBuilderServiceTests.cs
  20  CRM.Tests/Services/CoreDataSeederServiceTests.cs
  20  CRM.Tests/AI/SK/Agents/AgentSelectionStrategyTests.cs
  19  Services/UserServiceTests.cs
  19  Services/ITSM/KnowledgeServiceTests.cs
  19  Repositories/GenericRepositoryTests.cs
  19  Models/ContactModelTests.cs
  19  Dtos/DtoMappingTests.cs
  19  Controllers/AccountsControllerTests.cs
  19  CRM.Tests/Services/TeamServiceTests.cs
  18  Middleware/AuthenticationMiddlewareTests.cs
  18  Controllers/OpportunitiesControllerTests.cs
  18  CRM.Tests/Services/UserApprovalServiceTests.cs
  18  CRM.Tests/Services/EmailTemplateServiceTests.cs
  18  CRM.Tests/Services/DashboardBuilderServiceTests.cs
  18  CRM.Tests/Services/CommissionServiceTests.cs
  18  CRM.Tests/HostedServices/SLAEnforcementHostedServiceTests.cs
  18  CRM.Tests/AI/SK/Agents/LeadScoringAgentTests.cs
  17  Integration/BuiltInSearchProviderIntegrationTests.cs
  17  HostedServices/EmailSyncHostedServiceTests.cs
  17  Controllers/ProductsControllerTests.cs
  17  CRM.Tests/AI/SK/Agents/SupportTriageAgentTests.cs
  16  Services/TokenRevocationServiceTests.cs
  16  Services/ITSM/SLAEnforcementHostedServiceTests.cs
  16  Services/ITSM/CatalogServiceTests.cs
  16  Controllers/ITSMSLAControllerTests.cs
  16  CRM.Tests/Integration/Services/ActivityServiceTests.cs
  15  Services/ITSM/IncidentServiceTests.cs
  15  Middleware/RateLimitingMiddlewareTests.cs
  15  Controllers/ITSMKnowledgeControllerTests.cs
  15  Controllers/ITSMChangesControllerTests.cs
  15  CRM.Tests/Services/DashboardServiceTests.cs
  15  CRM.Tests/AI/SK/Plugins/LeadPluginTests.cs
  14  Services/ITSM/ChangeManagementServiceTests.cs
  14  Services/AuthenticationServiceTests.cs
  14  Controllers/ITSMCatalogControllerTests.cs
  14  CRM.Tests/AI/SK/Services/AgentExecutionServiceTests.cs
  14  CRM.Tests/AI/SK/Plugins/AccountPluginTests.cs
  13  Services/ITSM/SLAServiceTests.cs
  13  Services/ITSM/KnowledgeManagementServiceTests.cs
  13  CRM.Tests/AI/SK/Plugins/ServiceRequestPluginTests.cs
  12  Services/ITSM/ServiceCatalogServiceTests.cs
  12  Integration/MeilisearchProviderIntegrationTests.cs
  12  HostedServices/DatabaseSyncHostedServiceTests.cs
  12  Controllers/ITSMIncidentsControllerTests.cs
  12  CRM.Tests/AI/SK/Plugins/PluginAttributeVerificationTests.cs
  12  CRM.Tests/AI/SK/Agents/GeneralAssistantAgentTests.cs
  11  Services/ITSM/ProblemServiceTests.cs
  11  Services/ITSM/CMDBServiceTests.cs
  11  Controllers/ITSMProblemsControllerTests.cs
  11  Controllers/ITSMCMDBControllerTests.cs
  11  CRM.Tests/Services/AI/AIOpportunityScoringServiceTests.cs
  11  CRM.Tests/AI/SK/Connectors/CrmKernelFactoryTests.cs
  10  Services/SystemSettingsServiceTests.cs
  10  LeadServiceTests.cs
  10  HostedServices/WorkflowWorkerServiceTests.cs
  10  CRM.Tests/Services/AI/AILeadScoringServiceTests.cs
   9  Integration/ProviderDIIntegrationTests.cs
   9  HostedServices/ZipCodeImportHostedServiceTests.cs
   9  HostedServices/LeadScoreDecayHostedServiceTests.cs
   9  CRM.Tests/Services/AI/AIKnowledgeSearchServiceTests.cs
   9  CRM.Tests/AI/SK/Plugins/SearchPluginTests.cs
   7  CreditMemoServiceTests.cs
   7  CRM.Tests/UserEntityTests.cs
   7  CRM.Tests/EntityTests.cs
   7  CRM.Tests/AI/SK/Attributes/RequiresApprovalAttributeTests.cs
   6  Features/FeatureFlagTests.cs
   6  Controllers/DepartmentsControllerTests.cs
   6  BVT/CoreApiBVTTests.cs
   5  BVT/SalesApiBVTTests.cs
   5  BVT/MarketingIrmApiBVTTests.cs
   4  BVT/IntegrationApiBVTTests.cs
   1  Performance/PerformanceTests.cs
```

### 8.2 All 39 E2E Test Files

```
e2e-tests/tests/account-contact-linking.spec.ts
e2e-tests/tests/admin/admin.spec.ts
e2e-tests/tests/auth/authentication.spec.ts
e2e-tests/tests/bvt/api-bvt.spec.ts
e2e-tests/tests/bvt/itsm-api-bvt.spec.ts
e2e-tests/tests/bvt/itsm-core-api-bvt.spec.ts
e2e-tests/tests/campaigns/campaign-bugs.spec.ts
e2e-tests/tests/campaigns/campaign-execution.spec.ts
e2e-tests/tests/campaigns/campaign-setup.spec.ts
e2e-tests/tests/campaigns/campaigns.spec.ts
e2e-tests/tests/contacts/contacts.spec.ts
e2e-tests/tests/crud-operations/crud-operations.spec.ts
e2e-tests/tests/customers/customers.spec.ts
e2e-tests/tests/dashboard/dashboard.spec.ts
e2e-tests/tests/data-lifecycle/data-lifecycle.spec.ts
e2e-tests/tests/data-population/data-population.spec.ts
e2e-tests/tests/data-population/debug-customer.spec.ts
e2e-tests/tests/data-population/debug-dialog-dom.spec.ts
e2e-tests/tests/data/create-accounts-contacts.spec.ts
e2e-tests/tests/data/create-microsoft-account.spec.ts
e2e-tests/tests/data/create-microsoft-ui.spec.ts
e2e-tests/tests/data/generate-test-data.spec.ts
e2e-tests/tests/data/verify-microsoft-ui.spec.ts
e2e-tests/tests/deduplication/deduplication.spec.ts
e2e-tests/tests/functional/itsm-core-ui-functional.spec.ts
e2e-tests/tests/functional/itsm-ui-functional.spec.ts
e2e-tests/tests/functional/ui-functional.spec.ts
e2e-tests/tests/groups/create-groups.spec.ts
e2e-tests/tests/leads/leads.spec.ts
e2e-tests/tests/notes-quotes-features.spec.ts
e2e-tests/tests/opportunities/opportunities.spec.ts
e2e-tests/tests/persona/persona-api-journeys.spec.ts
e2e-tests/tests/persona/persona-e2e-journeys.spec.ts
e2e-tests/tests/relationships/relationships.spec.ts
e2e-tests/tests/service-requests/service-requests.spec.ts
e2e-tests/tests/ui-account-contact-test.spec.ts
e2e-tests/tests/users/create-users.spec.ts
e2e-tests/tests/workflow-execution/workflow-execution.spec.ts
e2e-tests/tests/workflows/workflows.spec.ts
```

### 8.3 All 18 Frontend Test Files

```
CRM.Frontend/src/__tests__/AdminPages.comprehensive.test.tsx        (94 tests)
CRM.Frontend/src/__tests__/CampaignsPage.test.tsx                   (17 tests)
CRM.Frontend/src/__tests__/ContactsPage.comprehensive.test.tsx      (84 tests)
CRM.Frontend/src/__tests__/CustomersPage.comprehensive.test.tsx     (91 tests)
CRM.Frontend/src/__tests__/CustomersPage.test.tsx                   (11 tests)
CRM.Frontend/src/__tests__/DashboardPage.comprehensive.test.tsx     (62 tests)
CRM.Frontend/src/__tests__/ITSMCorePages.test.tsx                   (24 tests)
CRM.Frontend/src/__tests__/ITSMPhase4Pages.test.tsx                 (39 tests)
CRM.Frontend/src/__tests__/LoginPage.comprehensive.test.tsx         (47 tests)
CRM.Frontend/src/__tests__/LoginPage.test.tsx                       (9 tests)
CRM.Frontend/src/__tests__/Navigation.comprehensive.test.tsx        (61 tests)
CRM.Frontend/src/__tests__/OpportunitiesPage.comprehensive.test.tsx (74 tests)
CRM.Frontend/src/__tests__/OpportunitiesPage.test.tsx               (15 tests)
CRM.Frontend/src/__tests__/ProductsPage.comprehensive.test.tsx      (69 tests)
CRM.Frontend/src/__tests__/ProductsPage.test.tsx                    (15 tests)
CRM.Frontend/src/__tests__/ServiceRequestsPage.test.tsx             (17 tests)
CRM.Frontend/src/__tests__/SharedComponents.comprehensive.test.tsx  (108 tests)
CRM.Frontend/src/__tests__/apiClient.test.ts                        (8 tests)
```

---

## Appendix A: Test Naming Convention

### Backend

```
{Method}_Should{ExpectedBehavior}_When{Condition}
```

Example: `GetById_ShouldReturnAccount_WhenAccountExists`

### E2E

```
test('{User action or scenario description}', ...)
test.skip('{Temporarily disabled test}', ...)
```

### Frontend

```
it('should {behavior} when {condition}', ...)
test('{component} renders {expected state}', ...)
```

---

## Appendix B: Running Tests

```bash
# Backend - all tests
cd CRM.Backend && dotnet test

# Backend - specific project
cd CRM.Backend && dotnet test tests/CRM.Tests.csproj
cd CRM.Backend && dotnet test tests/CRM.Tests/CRM.Tests.csproj
cd CRM.Backend && dotnet test tests/Unit/Core/CRM.Tests.Unit.Core.csproj

# Backend - with coverage
cd CRM.Backend && dotnet test --collect:"XPlat Code Coverage"

# E2E - all
cd e2e-tests && BASE_URL=http://192.168.0.9 npx playwright test

# E2E - BVT only
cd e2e-tests && BASE_URL=http://192.168.0.9 npx playwright test tests/bvt/

# E2E - specific browser
cd e2e-tests && npx playwright test --project=chromium

# Frontend - all
cd CRM.Frontend && npm test

# Frontend - with coverage
cd CRM.Frontend && npm test -- --coverage

# Frontend - specific file
cd CRM.Frontend && npm test -- --testPathPattern="LoginPage"
```

---

**END OF AUDIT**

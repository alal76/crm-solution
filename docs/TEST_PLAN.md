# CRM Solution - Comprehensive Test Plan

> **Last Updated:** February 13, 2026
> **Version:** 1.0

---

## Table of Contents
1. [Test Strategy](#test-strategy)
2. [Test Architecture](#test-architecture)
3. [Test Suites Overview](#test-suites-overview)
4. [Backend Test Suites](#backend-test-suites)
5. [E2E Test Suites](#e2e-test-suites)
6. [Frontend Test Suites](#frontend-test-suites)
7. [Individual Test Case Writeups](#individual-test-case-writeups)
8. [Coverage & Quality Metrics](#coverage--quality-metrics)
9. [CI/CD Integration](#cicd-integration)
10. [Troubleshooting & Known Issues](#troubleshooting--known-issues)

---

## 1. Test Strategy

The CRM Solution employs a layered testing strategy to ensure reliability, correctness, and maintainability across backend, frontend, and E2E layers. The approach includes:
- **Unit Tests:** Validate individual methods, business logic, and edge cases.
- **Integration Tests:** Verify service interactions, database operations, and provider integrations.
- **E2E Tests:** Simulate real user flows across the full stack using Playwright.
- **Frontend Tests:** Ensure UI components and pages behave as expected using Jest and React Testing Library.
- **Build Verification Tests (BVT):** Fast, critical-path API tests for CI/CD gating.

### Key Principles
- Test coverage targets: Backend 80%+, Frontend 50%+, E2E critical flows.
- All tests run in CI/CD pipelines; failures block deployment.
- Tests are organized by domain, service, and feature.
- Test data is seeded and isolated per test run.

---

## 2. Test Architecture

### Backend
- **Frameworks:** xUnit, Moq, FluentAssertions, EF Core InMemory, AspNetCore.Mvc.Testing
- **Projects:** CRM.UnitTests, CRM.IntegrationTests, CRM.Tests
- **Patterns:** Arrange-Act-Assert, dependency injection, test doubles

### E2E
- **Framework:** Playwright (multi-browser: chromium, firefox, webkit)
- **Specs:** 39 spec files, custom reporter, test data isolation
- **Flows:** Auth, CRUD, workflow, admin, ITSM, AI/analytics

### Frontend
- **Framework:** Jest, ts-jest, @testing-library/jest-dom
- **Coverage:** 50% minimum threshold, 17 test files
- **Patterns:** Component rendering, event simulation, snapshot testing

---

## 3. Test Suites Overview

| Layer     | Suite Name         | Files/Specs | Coverage | Purpose                  |
|-----------|--------------------|-------------|----------|--------------------------|
| Backend   | Unit Tests         | 35 dirs     | 80%+     | Service, entity, logic   |
| Backend   | Integration Tests  | 12 dirs     | 70%+     | DB, provider, API        |
| Backend   | BVT                | 1 file      | Critical | API smoke, CI/CD gate    |
| E2E       | Playwright Specs   | 39 files    | Flows    | Full-stack user flows    |
| Frontend  | Jest Tests         | 17 files    | 50%+     | UI, pages, components    |

---

## 4. Backend Test Suites

### 4.1 Unit Tests
- **Location:** CRM.Backend/tests/CRM.UnitTests/
- **Scope:** Entities, DTOs, service methods, validation logic
- **Examples:**
  - `AccountServiceTests`: CRUD, validation, merge, health score
  - `LeadServiceTests`: Scoring, conversion, duplicate detection
  - `OpportunityServiceTests`: Stage probability, product line items

### 4.2 Integration Tests
- **Location:** CRM.Backend/tests/CRM.IntegrationTests/
- **Scope:** Service-to-service, DB, provider, workflow
- **Examples:**
  - `BuiltInSearchProviderIntegrationTests`: Search, indexing, fallback
  - `NotificationProviderIntegrationTests`: Email, SMS, webhook

### 4.3 Build Verification Tests (BVT)
- **Location:** e2e-tests/tests/bvt/api-bvt.spec.ts
- **Scope:** Fast API smoke tests for CI/CD gating
- **Examples:**
  - Auth, CRUD, health, settings, dashboard

---

## 5. E2E Test Suites

### 5.1 Playwright Specs
- **Location:** e2e-tests/tests/
- **Scope:** Full-stack flows, user journeys, admin, ITSM, AI
- **Examples:**
  - `authentication.spec.ts`: Login, password reset, 2FA
  - `customers.spec.ts`: Account CRUD, merge, timeline
  - `itsm/incident.spec.ts`: Incident creation, escalation, resolution

---

## 6. Frontend Test Suites

### 6.1 Jest Tests
- **Location:** CRM.Frontend/src/
- **Scope:** UI components, pages, event handling, rendering
- **Examples:**
  - `DataGrid.test.tsx`: Sorting, filtering, inline editing
  - `AccountDetailsPage.test.tsx`: Details rendering, actions

---

## 7. Individual Test Case Writeups

### 7.1 Backend BVT Test Cases

- **Auth: Login Success**
  - Tests POST /api/auth/login with valid credentials; expects 200 OK and access token.
- **Auth: Login Failure**
  - Tests POST /api/auth/login with invalid credentials; expects 401 Unauthorized.
- **Account CRUD: Create**
  - Tests POST /api/accounts; validates creation, response, DB record.
- **Account CRUD: Read**
  - Tests GET /api/accounts/{id}; expects correct account data.
- **Account CRUD: Update**
  - Tests PUT /api/accounts/{id}; validates update, timestamps.
- **Account CRUD: Delete**
  - Tests DELETE /api/accounts/{id}; expects soft delete, IsDeleted flag.
- **Health Endpoint**
  - Tests GET /health; expects status healthy, 200 OK.
- **Settings Endpoint**
  - Tests GET /api/settings; validates config, feature flags.
- **Dashboard Data**
  - Tests GET /api/dashboard; expects summary stats, widgets.

### 7.2 Backend Service Test Cases

- **AccountService: Duplicate Email Validation**
  - Validates backend prevents duplicate emails on account creation.
- **LeadService: Lead Conversion Workflow**
  - Tests lead → account/contact/opportunity conversion logic.
- **OpportunityService: Stage Probability Automation**
  - Validates stage probability calculation and updates.
- **MergeService: Unmerge Records**
  - Tests snapshot restoration and related record relinking.

### 7.3 E2E Test Cases

- **Authentication: Login Flow**
  - Simulates user login, checks UI, token, redirects.
- **Customers: Account Creation**
  - Fills form, submits, verifies account appears in list.
- **Customers: Merge Accounts**
  - Selects duplicates, merges, checks timeline and survivor record.
- **ITSM: Incident Creation**
  - Creates incident, assigns, escalates, resolves, checks status.
- **AI: Lead Scoring Agent**
  - Submits lead, receives AI score, validates rubric.

### 7.4 Frontend Test Cases

- **DataGrid: Inline Editing**
  - Edits cell, triggers save, checks value and UI update.
- **AccountDetailsPage: Render Details**
  - Loads account, checks all fields, actions, and related entities.
- **Breadcrumbs: Navigation**
  - Simulates navigation, checks breadcrumb updates.

---

## 8. Coverage & Quality Metrics

- **Backend:** 80%+ line and branch coverage (xUnit, Coverlet)
- **Frontend:** 50%+ line coverage (Jest)
- **E2E:** All critical flows covered; failures block CI/CD
- **Quality Gates:** All tests must pass; coverage thresholds enforced

---

## 9. CI/CD Integration

- **Pipelines:** Azure DevOps, GitHub Actions
- **Stages:** Build → Test → Coverage → Deploy
- **Test Runs:** All suites run on PR, push, nightly
- **Artifacts:** Test reports, coverage, screenshots (E2E)

---

## 10. Troubleshooting & Known Issues

- **Entity Tracking Conflicts:** Fixed in Repository.UpdateAsync
- **MariaDB Row Size Limit:** Fixed in CrmDbContext.OnModelCreating
- **E2E Selector Issues:** Use specific selectors for MUI components
- **Test Data Isolation:** All tests use seeded, isolated data

---

**END OF TEST PLAN**

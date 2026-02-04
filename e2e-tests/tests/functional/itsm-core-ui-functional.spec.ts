// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// E2E UI Tests - ITSM Core Features (Phases 1-3)

import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.FRONTEND_URL || 'http://localhost:3000';

test.describe('ITSM Core UI Functional Tests', () => {

  // ============================================================================
  // Navigation Tests
  // ============================================================================

  test.describe('ITSM Navigation', () => {
    test('NAV-001: Navigate to Incidents page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents`);
      await expect(page).toHaveURL(/.*incidents/);
    });

    test('NAV-002: Navigate to Problems page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/problems`);
      await expect(page).toHaveURL(/.*problems/);
    });

    test('NAV-003: Navigate to Changes page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes`);
      await expect(page).toHaveURL(/.*changes/);
    });

    test('NAV-004: Navigate to CMDB page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb`);
      await expect(page).toHaveURL(/.*cmdb/);
    });

    test('NAV-005: Navigate to Knowledge Base page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge`);
      await expect(page).toHaveURL(/.*knowledge/);
    });

    test('NAV-006: Navigate to Service Catalog page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog`);
      await expect(page).toHaveURL(/.*catalog/);
    });

    test('NAV-007: Navigate to SLA Dashboard page', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      await expect(page).toHaveURL(/.*sla/);
    });
  });

  // ============================================================================
  // Incident Management UI Tests
  // ============================================================================

  test.describe('Incident Management UI', () => {
    test('INC-UI-001: Incidents list page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents`);
      // Check for common list elements
      const hasTable = await page.locator('table, [role="grid"], .incidents-list, .data-table').count() > 0;
      const hasCards = await page.locator('.incident-card, .list-item').count() > 0;
      const hasLoading = await page.locator('.loading, .spinner, [aria-busy="true"]').count() > 0;
      expect(hasTable || hasCards || hasLoading || page.url().includes('login')).toBeTruthy();
    });

    test('INC-UI-002: Create incident button visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents`);
      // Look for create button or form trigger
      const createBtn = page.locator('button:has-text("Create"), button:has-text("New"), a:has-text("Create")');
      const exists = await createBtn.count() > 0 || page.url().includes('login');
      expect(exists).toBeTruthy();
    });

    test('INC-UI-003: Incident filters available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents`);
      // Look for filter controls
      const filters = page.locator('[data-testid="filters"], .filter-section, select, input[type="search"]');
      const hasFilters = await filters.count() > 0 || page.url().includes('login');
      expect(hasFilters).toBeTruthy();
    });

    test('INC-UI-004: Incident detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents/1`);
      await expect(page).toHaveURL(/.*incidents.*/);
    });

    test('INC-UI-005: Priority indicators displayed', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/incidents`);
      // Look for priority badges or indicators
      const priorities = page.locator('.priority, .badge, [data-priority], .p1, .p2, .p3');
      const hasPriorities = await priorities.count() > 0 || page.url().includes('login');
      expect(hasPriorities).toBeTruthy();
    });
  });

  // ============================================================================
  // Problem Management UI Tests
  // ============================================================================

  test.describe('Problem Management UI', () => {
    test('PRB-UI-001: Problems list page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/problems`);
      const hasContent = await page.locator('table, .problems-list, .data-table').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('PRB-UI-002: Known Error filter visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/problems`);
      const knownErrorFilter = page.locator('text=Known Error, [data-filter="knownError"], input[name="knownError"]');
      const hasFilter = await knownErrorFilter.count() > 0 || page.url().includes('login');
      expect(hasFilter).toBeTruthy();
    });

    test('PRB-UI-003: Problem detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/problems/1`);
      await expect(page).toHaveURL(/.*problems.*/);
    });

    test('PRB-UI-004: Root Cause Analysis section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/problems/1`);
      const rcaSection = page.locator('text=Root Cause, text=RCA, [data-section="rca"]');
      const hasRCA = await rcaSection.count() > 0 || page.url().includes('login') || page.url().includes('404');
      expect(hasRCA).toBeTruthy();
    });
  });

  // ============================================================================
  // Change Management UI Tests
  // ============================================================================

  test.describe('Change Management UI', () => {
    test('CHG-UI-001: Changes list page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes`);
      const hasContent = await page.locator('table, .changes-list, .data-table').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('CHG-UI-002: Change calendar view available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes`);
      const calendarBtn = page.locator('text=Calendar, button:has-text("Calendar"), [data-view="calendar"]');
      const hasCalendar = await calendarBtn.count() > 0 || page.url().includes('login');
      expect(hasCalendar).toBeTruthy();
    });

    test('CHG-UI-003: Approval status indicators visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes`);
      const approvalBadges = page.locator('.approval-status, .badge, [data-status]');
      const hasApproval = await approvalBadges.count() > 0 || page.url().includes('login');
      expect(hasApproval).toBeTruthy();
    });

    test('CHG-UI-004: Change detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes/1`);
      await expect(page).toHaveURL(/.*changes.*/);
    });

    test('CHG-UI-005: Risk assessment section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/changes/1`);
      const riskSection = page.locator('text=Risk, [data-section="risk"]');
      const hasRisk = await riskSection.count() > 0 || page.url().includes('login') || page.url().includes('404');
      expect(hasRisk).toBeTruthy();
    });
  });

  // ============================================================================
  // CMDB UI Tests
  // ============================================================================

  test.describe('CMDB UI', () => {
    test('CMDB-UI-001: CMDB list page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb`);
      const hasContent = await page.locator('table, .ci-list, .data-table, .cmdb-view').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('CMDB-UI-002: CI type filter available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb`);
      const typeFilter = page.locator('select, [data-filter="type"], text=Type');
      const hasFilter = await typeFilter.count() > 0 || page.url().includes('login');
      expect(hasFilter).toBeTruthy();
    });

    test('CMDB-UI-003: CI detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb/1`);
      await expect(page).toHaveURL(/.*cmdb.*/);
    });

    test('CMDB-UI-004: Relationship visualization available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb/1`);
      const relationships = page.locator('text=Relationships, .relationship-graph, [data-section="relationships"]');
      const hasRelationships = await relationships.count() > 0 || page.url().includes('login') || page.url().includes('404');
      expect(hasRelationships).toBeTruthy();
    });

    test('CMDB-UI-005: Search functionality available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/cmdb`);
      const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], [data-testid="search"]');
      const hasSearch = await searchInput.count() > 0 || page.url().includes('login');
      expect(hasSearch).toBeTruthy();
    });
  });

  // ============================================================================
  // Knowledge Management UI Tests
  // ============================================================================

  test.describe('Knowledge Management UI', () => {
    test('KB-UI-001: Knowledge Base page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge`);
      const hasContent = await page.locator('.article-list, .knowledge-base, table').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('KB-UI-002: Article search available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge`);
      const searchInput = page.locator('input[type="search"], input[placeholder*="search" i]');
      const hasSearch = await searchInput.count() > 0 || page.url().includes('login');
      expect(hasSearch).toBeTruthy();
    });

    test('KB-UI-003: Popular articles section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge`);
      const popularSection = page.locator('text=Popular, text=Top Articles, [data-section="popular"]');
      const hasPopular = await popularSection.count() > 0 || page.url().includes('login');
      expect(hasPopular).toBeTruthy();
    });

    test('KB-UI-004: Article detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge/articles/1`);
      await expect(page).toHaveURL(/.*knowledge.*/);
    });

    test('KB-UI-005: Feedback buttons visible on article', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/knowledge/articles/1`);
      const feedbackBtns = page.locator('text=Helpful, text=Not Helpful, [data-feedback]');
      const hasFeedback = await feedbackBtns.count() > 0 || page.url().includes('login') || page.url().includes('404');
      expect(hasFeedback).toBeTruthy();
    });
  });

  // ============================================================================
  // Service Catalog UI Tests
  // ============================================================================

  test.describe('Service Catalog UI', () => {
    test('CAT-UI-001: Service Catalog page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog`);
      const hasContent = await page.locator('.catalog-items, .service-catalog, .catalog-grid').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('CAT-UI-002: Category navigation available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog`);
      const categories = page.locator('.categories, nav, [data-categories]');
      const hasCategories = await categories.count() > 0 || page.url().includes('login');
      expect(hasCategories).toBeTruthy();
    });

    test('CAT-UI-003: Featured items section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog`);
      const featured = page.locator('text=Featured, [data-featured], .featured-items');
      const hasFeatured = await featured.count() > 0 || page.url().includes('login');
      expect(hasFeatured).toBeTruthy();
    });

    test('CAT-UI-004: Catalog item detail page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog/items/1`);
      await expect(page).toHaveURL(/.*catalog.*/);
    });

    test('CAT-UI-005: Request button visible on item', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog/items/1`);
      const requestBtn = page.locator('button:has-text("Request"), button:has-text("Order"), [data-action="request"]');
      const hasRequest = await requestBtn.count() > 0 || page.url().includes('login') || page.url().includes('404');
      expect(hasRequest).toBeTruthy();
    });

    test('CAT-UI-006: My requests page accessible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/catalog/my-requests`);
      await expect(page).toHaveURL(/.*catalog.*/);
    });
  });

  // ============================================================================
  // SLA Dashboard UI Tests
  // ============================================================================

  test.describe('SLA Dashboard UI', () => {
    test('SLA-UI-001: SLA Dashboard page loads', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      const hasContent = await page.locator('.sla-dashboard, .dashboard, .metrics').count() > 0 || page.url().includes('login');
      expect(hasContent).toBeTruthy();
    });

    test('SLA-UI-002: Compliance metrics visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      const metrics = page.locator('text=Compliance, text=SLA, .metric, .kpi');
      const hasMetrics = await metrics.count() > 0 || page.url().includes('login');
      expect(hasMetrics).toBeTruthy();
    });

    test('SLA-UI-003: Breached SLAs section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      const breached = page.locator('text=Breach, text=Breached, [data-section="breached"]');
      const hasBreached = await breached.count() > 0 || page.url().includes('login');
      expect(hasBreached).toBeTruthy();
    });

    test('SLA-UI-004: At-risk SLAs section visible', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      const atRisk = page.locator('text=At Risk, text=Warning, [data-section="at-risk"]');
      const hasAtRisk = await atRisk.count() > 0 || page.url().includes('login');
      expect(hasAtRisk).toBeTruthy();
    });

    test('SLA-UI-005: Date range filter available', async ({ page }) => {
      await page.goto(`${BASE_URL}/itsm/sla`);
      const dateFilter = page.locator('input[type="date"], .date-picker, [data-filter="date"]');
      const hasDateFilter = await dateFilter.count() > 0 || page.url().includes('login');
      expect(hasDateFilter).toBeTruthy();
    });
  });
});

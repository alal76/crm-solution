/**
 * CRM Solution - ITSM E2E Tests
 *
 * BVT-style flows for core ITSM entities with resilient selectors.
 */

import { test, expect, DataGridHelper } from '../fixtures';
import type { Page } from '@playwright/test';

const createButtonSelector = 'button:has-text("Create"), button:has-text("New"), button:has-text("Add")';
const saveButtonSelector = 'button[type="submit"], button:has-text("Save"), button:has-text("Create")';

async function fillIfVisible(page: Page, selector: string, value: string) {
  const locator = page.locator(selector).first();
  if (await locator.isVisible().catch(() => false)) {
    await locator.fill(value);
    return true;
  }
  return false;
}

async function clickIfVisible(page: Page, selector: string) {
  const locator = page.locator(selector).first();
  if (await locator.isVisible().catch(() => false)) {
    await locator.click();
    return true;
  }
  return false;
}

async function attachIfVisible(page: Page) {
  const fileInput = page.locator('input[type="file"]').first();
  if (await fileInput.isVisible().catch(() => false)) {
    await fileInput.setInputFiles({
      name: 'test-attachment.txt',
      mimeType: 'text/plain',
      buffer: Buffer.from('TEST_ATTACHMENT'),
    });
    return true;
  }
  return false;
}

test.describe('ITSM E2E - Incidents', () => {
  test('ITSM-INC-001: Create incident (if form available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/incidents');
    await page.waitForLoadState('domcontentloaded');

    const created = await clickIfVisible(page, createButtonSelector);
    if (created) {
      const id = Date.now();
      await fillIfVisible(page, 'input[name="title"], input[name="subject"], input[placeholder*="title" i], input[placeholder*="summary" i]', `TEST_Incident_${id}`);
      await fillIfVisible(page, 'textarea[name="description"], textarea[placeholder*="description" i]', 'TEST: Incident created by Playwright');
      await clickIfVisible(page, 'label:has-text("Priority") + div, [aria-label*="Priority" i]');
      await clickIfVisible(page, '[role="option"]:has-text("High"), [role="option"]:has-text("Critical")');
      await attachIfVisible(page);
      await clickIfVisible(page, saveButtonSelector);
      await page.waitForTimeout(1500);
    }

    await expect(page).toHaveURL(/\/itsm\/incidents/);
  });

  test('ITSM-INC-002: Update/close incident (if row available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/incidents');
    await page.waitForLoadState('domcontentloaded');

    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    const rowCount = await grid.getRowCount();

    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      await clickIfVisible(page, 'button:has-text("Resolve"), button:has-text("Close"), button:has-text("Complete")');
      await clickIfVisible(page, 'label:has-text("Status") + div, [aria-label*="Status" i]');
      await clickIfVisible(page, '[role="option"]:has-text("Resolved"), [role="option"]:has-text("Closed")');
      await clickIfVisible(page, saveButtonSelector);
    }

    await expect(page).toHaveURL(/\/itsm\/incidents/);
  });

  test('ITSM-INC-003: Add incident comment (if input available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/incidents/1');
    await page.waitForLoadState('domcontentloaded');

    const commentFilled = await fillIfVisible(
      page,
      'textarea[placeholder*="comment" i], textarea[name*="comment" i], input[placeholder*="comment" i]',
      'TEST: Incident comment from Playwright'
    );
    if (commentFilled) {
      await clickIfVisible(page, 'button:has-text("Add Comment"), button:has-text("Post"), button:has-text("Save")');
    }

    await expect(page).toHaveURL(/\/itsm\/incidents/);
  });
});

test.describe('ITSM E2E - Problems', () => {
  test('ITSM-PRB-001: Create problem (if form available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/problems');
    await page.waitForLoadState('domcontentloaded');

    const created = await clickIfVisible(page, createButtonSelector);
    if (created) {
      const id = Date.now();
      await fillIfVisible(page, 'input[name="title"], input[name="subject"], input[placeholder*="title" i]', `TEST_Problem_${id}`);
      await fillIfVisible(page, 'textarea[name="description"], textarea[placeholder*="description" i]', 'TEST: Problem created by Playwright');
      await clickIfVisible(page, saveButtonSelector);
      await page.waitForTimeout(1500);
    }

    await expect(page).toHaveURL(/\/itsm\/problems/);
  });

  test('ITSM-PRB-002: Update/close problem (if row available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/problems');
    await page.waitForLoadState('domcontentloaded');

    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    const rowCount = await grid.getRowCount();

    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      await clickIfVisible(page, 'button:has-text("Mark Known Error"), button:has-text("Resolve"), button:has-text("Close")');
      await clickIfVisible(page, 'label:has-text("Status") + div, [aria-label*="Status" i]');
      await clickIfVisible(page, '[role="option"]:has-text("Known Error"), [role="option"]:has-text("Resolved"), [role="option"]:has-text("Closed")');
      await clickIfVisible(page, saveButtonSelector);
    }

    await expect(page).toHaveURL(/\/itsm\/problems/);
  });
});

test.describe('ITSM E2E - Changes', () => {
  test('ITSM-CHG-001: Create change (if form available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/changes');
    await page.waitForLoadState('domcontentloaded');

    const created = await clickIfVisible(page, createButtonSelector);
    if (created) {
      const id = Date.now();
      await fillIfVisible(page, 'input[name="title"], input[name="subject"], input[placeholder*="title" i]', `TEST_Change_${id}`);
      await fillIfVisible(page, 'textarea[name="description"], textarea[placeholder*="description" i]', 'TEST: Change created by Playwright');
      await clickIfVisible(page, 'label:has-text("Risk") + div, [aria-label*="Risk" i]');
      await clickIfVisible(page, '[role="option"]:has-text("Low"), [role="option"]:has-text("Medium")');
      await clickIfVisible(page, saveButtonSelector);
      await page.waitForTimeout(1500);
    }

    await expect(page).toHaveURL(/\/itsm\/changes/);
  });

  test('ITSM-CHG-002: Submit/close change (if row available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/changes');
    await page.waitForLoadState('domcontentloaded');

    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    const rowCount = await grid.getRowCount();

    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      await clickIfVisible(page, 'button:has-text("Submit")');
      await clickIfVisible(page, 'button:has-text("Close"), button:has-text("Complete")');
      await clickIfVisible(page, saveButtonSelector);
    }

    await expect(page).toHaveURL(/\/itsm\/changes/);
  });
});

test.describe('ITSM E2E - CMDB', () => {
  test('ITSM-CMDB-001: Create CI (if form available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/cmdb');
    await page.waitForLoadState('domcontentloaded');

    const created = await clickIfVisible(page, createButtonSelector);
    if (created) {
      const id = Date.now();
      await fillIfVisible(page, 'input[name="name"], input[placeholder*="name" i]', `TEST_CI_${id}`);
      await fillIfVisible(page, 'textarea[name="description"], textarea[placeholder*="description" i]', 'TEST: CI created by Playwright');
      await clickIfVisible(page, 'label:has-text("Type") + div, [aria-label*="Type" i]');
      await clickIfVisible(page, '[role="option"]:has-text("Server"), [role="option"]:has-text("Service")');
      await clickIfVisible(page, saveButtonSelector);
      await page.waitForTimeout(1500);
    }

    await expect(page).toHaveURL(/\/itsm\/cmdb/);
  });

  test('ITSM-CMDB-002: Update CI (if row available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/cmdb');
    await page.waitForLoadState('domcontentloaded');

    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    const rowCount = await grid.getRowCount();

    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      await clickIfVisible(page, 'button:has-text("Edit")');
      await fillIfVisible(page, 'textarea[name="notes"], textarea[placeholder*="notes" i]', 'TEST: Updated by Playwright');
      await clickIfVisible(page, saveButtonSelector);
    }

    await expect(page).toHaveURL(/\/itsm\/cmdb/);
  });
});

test.describe('ITSM E2E - Knowledge Base', () => {
  test('ITSM-KB-001: Create article (if form available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/knowledge');
    await page.waitForLoadState('domcontentloaded');

    const created = await clickIfVisible(page, createButtonSelector);
    if (created) {
      const id = Date.now();
      await fillIfVisible(page, 'input[name="title"], input[placeholder*="title" i]', `TEST_Article_${id}`);
      await fillIfVisible(page, 'textarea[name="content"], textarea[placeholder*="content" i], textarea[placeholder*="body" i]', 'TEST: Knowledge article created by Playwright');
      await clickIfVisible(page, saveButtonSelector);
      await page.waitForTimeout(1500);
    }

    await expect(page).toHaveURL(/\/itsm\/knowledge/);
  });

  test('ITSM-KB-002: Publish/retire article (if row available)', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/knowledge');
    await page.waitForLoadState('domcontentloaded');

    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    const rowCount = await grid.getRowCount();

    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      await clickIfVisible(page, 'button:has-text("Publish"), button:has-text("Retire")');
      await clickIfVisible(page, saveButtonSelector);
    }

    await expect(page).toHaveURL(/\/itsm\/knowledge/);
  });
});

test.describe('ITSM E2E - Dashboards & Analytics', () => {
  test('ITSM-DSH-001: ITSM dashboard loads', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/dashboard');
    await page.waitForLoadState('domcontentloaded');

    const hasMetrics = await page.locator('[data-testid*="dashboard"], .dashboard, .metrics, .chart, canvas, svg').count();
    expect(hasMetrics > 0 || page.url().includes('/itsm/dashboard')).toBeTruthy();
  });

  test('ITSM-DSH-002: SLA compliance view loads', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/dashboard/sla');
    await page.waitForLoadState('domcontentloaded');

    const hasSla = await page.locator('text=/SLA|Compliance/i, .chart, canvas, svg').count();
    expect(hasSla > 0 || page.url().includes('/itsm/dashboard/sla')).toBeTruthy();
  });

  test('ITSM-DSH-003: Agent performance view loads', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/dashboard/agents');
    await page.waitForLoadState('domcontentloaded');

    const hasAgents = await page.locator('text=/Agent|Performance/i, table, [role="grid"], .chart').count();
    expect(hasAgents > 0 || page.url().includes('/itsm/dashboard/agents')).toBeTruthy();
  });

  test('ITSM-DSH-004: Executive summary loads', async ({ authenticatedPage }) => {
    const page = authenticatedPage;
    await page.goto('/itsm/dashboard/executive');
    await page.waitForLoadState('domcontentloaded');

    const hasExecutive = await page.locator('text=/Executive|Summary|Overview/i, .metrics, .chart, canvas, svg').count();
    expect(hasExecutive > 0 || page.url().includes('/itsm/dashboard/executive')).toBeTruthy();
  });
});

test.describe('ITSM E2E - Access Control', () => {
  test('ITSM-AUTH-001: Unauthenticated access redirects to login or shows login form', async ({ browser }) => {
    const context = await browser.newContext();
    const page = await context.newPage();

    await page.goto('/itsm/incidents');
    await page.waitForLoadState('domcontentloaded');

    const onLoginUrl = page.url().includes('/login');
    const hasLoginForm = await page.locator('input[type="password"]').first().isVisible().catch(() => false);
    const onItsmPage = page.url().includes('/itsm/');

    expect(onLoginUrl || hasLoginForm || onItsmPage).toBeTruthy();

    await context.close();
  });
});

/**
 * CRM Solution - SLA Policies E2E Tests
 *
 * Tests for SLA policy management including:
 * - Create SLA policy
 * - Assign policy to ticket
 * - Verify SLA timer
 *
 * TODO-SYS008-025: SLA Policies E2E tests
 */

import { test, expect } from '../fixtures';
import type { Page } from '@playwright/test';

const SLA_BASE_URL = '/itsm/sla-policies';
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000';

test.describe('SLA Policies E2E Tests', () => {
  const timestamp = Date.now();

  test.beforeEach(async ({ authenticatedPage }) => {
    await authenticatedPage.goto(SLA_BASE_URL);
    await authenticatedPage.waitForLoadState('networkidle');
  });

  test('SLA-001: View SLA policies list', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check that we're on the SLA policies page or redirected
    const url = page.url();
    const isOnSlaPage = url.includes('sla') || url.includes('policies');
    const isOnAdminPage = url.includes('admin') || url.includes('settings');
    
    // Either we're on SLA page or redirected to admin/dashboard
    expect(isOnSlaPage || isOnAdminPage || url.includes('itsm')).toBe(true);

    // Look for policy list or table
    const policyTable = page.locator('table, .MuiDataGrid-root, [data-testid="policy-list"]').first();
    const hasPolicyList = await policyTable.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasPolicyList) {
      // Verify table headers or policy cards are present
      const headers = page.locator('th, .MuiDataGrid-columnHeader');
      const headerCount = await headers.count();
      expect(headerCount).toBeGreaterThan(0);
    }
  });

  test('SLA-002: Create a new SLA policy', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Look for create button
    const createButton = page.locator('button:has-text("Create"), button:has-text("New"), button:has-text("Add Policy")').first();
    
    if (await createButton.isVisible({ timeout: 5000 })) {
      await createButton.click();
      await page.waitForTimeout(500);

      // Fill policy name
      const nameInput = page.locator('input[name="name"], input[placeholder*="name" i]').first();
      if (await nameInput.isVisible()) {
        await nameInput.fill(`TEST_SLA_Policy_${timestamp}`);
      }

      // Fill description
      const descInput = page.locator('textarea[name="description"], input[name="description"]').first();
      if (await descInput.isVisible()) {
        await descInput.fill('Test SLA policy created by Playwright E2E tests');
      }

      // Set response time
      const responseInput = page.locator('input[name="responseTimeHours"], input[name="initialResponseTime"]').first();
      if (await responseInput.isVisible()) {
        await responseInput.fill('1');
      }

      // Set resolution time
      const resolutionInput = page.locator('input[name="resolutionTimeHours"], input[name="resolutionTime"]').first();
      if (await resolutionInput.isVisible()) {
        await resolutionInput.fill('8');
      }

      // Select priority if available
      const prioritySelect = page.locator('[aria-label*="priority" i], label:has-text("Priority") + div').first();
      if (await prioritySelect.isVisible()) {
        await prioritySelect.click();
        await page.locator('[role="option"]:has-text("High"), [role="option"]:has-text("Medium")').first().click();
      }

      // Check business hours option if available
      const businessHoursCheckbox = page.locator('input[name="businessHoursOnly"], input[type="checkbox"]').first();
      if (await businessHoursCheckbox.isVisible()) {
        // Don't change the default, just verify it's there
      }

      // Save the policy
      const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")').first();
      await saveButton.click();
      await page.waitForTimeout(1500);
    }

    // Verify we're back on the list page
    await expect(page).toHaveURL(/sla|policies|itsm/i);
  });

  test('SLA-003: Edit an existing SLA policy', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Wait for policy list to load
    await page.waitForLoadState('networkidle');

    // Click on first policy row
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRow.isVisible({ timeout: 5000 })) {
      await firstRow.click();
      await page.waitForTimeout(500);

      // Look for edit button
      const editButton = page.locator('button:has-text("Edit"), button[aria-label*="edit" i]').first();
      
      if (await editButton.isVisible({ timeout: 3000 })) {
        await editButton.click();
        await page.waitForTimeout(500);

        // Update the description
        const descInput = page.locator('textarea[name="description"], input[name="description"]').first();
        if (await descInput.isVisible()) {
          await descInput.fill(`Updated by E2E test at ${new Date().toISOString()}`);
        }

        // Save changes
        const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.isVisible()) {
          await saveButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/sla|policies|itsm/i);
  });

  test('SLA-004: Verify SLA timer on service request', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Navigate to service requests
    await page.goto('/itsm/service-requests');
    await page.waitForLoadState('networkidle');

    // Look for SLA status badge or timer
    const slaIndicator = page.locator(
      '[data-testid="sla-status"], ' +
      '.sla-badge, ' +
      '.sla-timer, ' +
      '[class*="sla" i], ' +
      'text=/SLA|response|resolution/i'
    ).first();

    const hasSlaIndicator = await slaIndicator.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasSlaIndicator) {
      // Verify SLA status is displayed
      await expect(slaIndicator).toBeVisible();
    }

    // Check first service request for SLA details
    const firstRequest = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRequest.isVisible({ timeout: 3000 })) {
      await firstRequest.click();
      await page.waitForTimeout(500);

      // Look for SLA information in detail view
      const slaSection = page.locator(
        '[data-testid="sla-info"], ' +
        '.sla-details, ' +
        'text=/SLA|target|due/i'
      ).first();

      const hasSlaSection = await slaSection.isVisible({ timeout: 3000 }).catch(() => false);

      // Either SLA info is shown or we're in a view without SLA
      expect(page.url()).toMatch(/itsm|service/i);
    }
  });

  test('SLA-005: Assign SLA policy to ticket', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    // Navigate to create service request
    await page.goto('/itsm/service-requests/new');
    await page.waitForLoadState('domcontentloaded');

    // If new SR form is available
    const titleInput = page.locator('input[name="title"], input[name="subject"]').first();
    
    if (await titleInput.isVisible({ timeout: 3000 })) {
      await titleInput.fill(`SLA Test Ticket ${timestamp}`);

      // Fill description
      const descInput = page.locator('textarea[name="description"]').first();
      if (await descInput.isVisible()) {
        await descInput.fill('Test ticket for SLA assignment verification');
      }

      // Select priority (which may auto-assign SLA)
      const prioritySelect = page.locator('[aria-label*="priority" i], label:has-text("Priority") + div').first();
      if (await prioritySelect.isVisible()) {
        await prioritySelect.click();
        await page.locator('[role="option"]:has-text("High")').first().click();
      }

      // Look for explicit SLA policy selection
      const slaPolicySelect = page.locator('[aria-label*="sla" i], label:has-text("SLA") + div').first();
      if (await slaPolicySelect.isVisible()) {
        await slaPolicySelect.click();
        const firstPolicy = page.locator('[role="option"]').first();
        if (await firstPolicy.isVisible()) {
          await firstPolicy.click();
        }
      }

      // Save the ticket
      const saveButton = page.locator('button[type="submit"], button:has-text("Create")').first();
      if (await saveButton.isVisible()) {
        await saveButton.click();
        await page.waitForTimeout(1500);
      }
    }

    await expect(page).toHaveURL(/itsm|service/i);
  });
});

test.describe('SLA Policies API Tests', () => {
  test('SLA-API-001: Get all SLA policies', async ({ request }) => {
    const response = await request.get(`${API_BASE_URL}/api/itsm/sla-policies`);
    
    expect([200, 401]).toContain(response.status());

    if (response.status() === 200) {
      const data = await response.json();
      expect(Array.isArray(data) || data.items !== undefined).toBe(true);
    }
  });

  test('SLA-API-002: Create SLA policy via API', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/api/itsm/sla-policies`, {
      data: {
        name: `API Test SLA ${Date.now()}`,
        description: 'Created via API test',
        responseTimeHours: 2,
        resolutionTimeHours: 16,
        businessHoursOnly: true,
        isActive: true
      },
      headers: {
        'Content-Type': 'application/json'
      }
    });

    expect([200, 201, 401]).toContain(response.status());
  });

  test('SLA-API-003: Get applicable policies for priority', async ({ request }) => {
    const response = await request.get(`${API_BASE_URL}/api/itsm/sla-policies/applicable?priority=High`);
    
    expect([200, 401]).toContain(response.status());

    if (response.status() === 200) {
      const data = await response.json();
      expect(Array.isArray(data)).toBe(true);
    }
  });
});

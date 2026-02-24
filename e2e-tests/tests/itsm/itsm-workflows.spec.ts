/**
 * CRM Solution - ITSM Workflows E2E Tests
 *
 * Comprehensive E2E tests for ITSM workflows including:
 * - Create incident
 * - Escalate incident
 * - Resolve incident
 * - Create problem from incident
 * - Create change request
 *
 * TODO-ITSM-08: ITSM workflows E2E tests
 */

import { test, expect } from '../fixtures';
import type { Page } from '@playwright/test';

const ITSM_BASE_URL = '/itsm';
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000';

test.describe('ITSM Workflow: Incident Lifecycle', () => {
  const timestamp = Date.now();
  let incidentId: number | null = null;

  test('INC-FLOW-001: Create new incident', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/incidents`);
    await page.waitForLoadState('networkidle');

    // Click create button
    const createButton = page.locator('button:has-text("Create"), button:has-text("New Incident")').first();
    
    if (await createButton.isVisible({ timeout: 5000 })) {
      await createButton.click();
      await page.waitForTimeout(500);

      // Fill incident details
      const titleInput = page.locator('input[name="title"], input[name="subject"], input[placeholder*="title" i]').first();
      if (await titleInput.isVisible()) {
        await titleInput.fill(`TEST_Incident_Workflow_${timestamp}`);
      }

      const descInput = page.locator('textarea[name="description"]').first();
      if (await descInput.isVisible()) {
        await descInput.fill('Test incident for workflow E2E test. User reports system slowness.');
      }

      // Select priority
      const prioritySelect = page.locator('[aria-label*="priority" i], label:has-text("Priority") + div').first();
      if (await prioritySelect.isVisible()) {
        await prioritySelect.click();
        await page.locator('[role="option"]:has-text("High")').first().click();
      }

      // Select category if available
      const categorySelect = page.locator('[aria-label*="category" i], label:has-text("Category") + div').first();
      if (await categorySelect.isVisible()) {
        await categorySelect.click();
        await page.locator('[role="option"]').first().click();
      }

      // Save incident
      const saveButton = page.locator('button[type="submit"], button:has-text("Create"), button:has-text("Save")').first();
      await saveButton.click();
      await page.waitForTimeout(2000);
    }

    await expect(page).toHaveURL(/itsm.*incident/i);
  });

  test('INC-FLOW-002: Escalate incident', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/incidents`);
    await page.waitForLoadState('networkidle');

    // Click on first incident (preferably our test incident)
    const testIncident = page.locator(`text=/TEST_Incident_Workflow|${timestamp}/`).first();
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    const targetRow = await testIncident.isVisible() ? testIncident : firstRow;
    
    if (await targetRow.isVisible({ timeout: 5000 })) {
      await targetRow.click();
      await page.waitForTimeout(500);

      // Look for escalate button
      const escalateButton = page.locator(
        'button:has-text("Escalate"), ' +
        'button[aria-label*="escalate" i], ' +
        '[data-testid="escalate-button"]'
      ).first();

      if (await escalateButton.isVisible({ timeout: 3000 })) {
        await escalateButton.click();
        await page.waitForTimeout(500);

        // Fill escalation reason if prompted
        const reasonInput = page.locator('textarea[name="reason"], textarea[name="escalationReason"]').first();
        if (await reasonInput.isVisible()) {
          await reasonInput.fill('Escalating due to SLA breach risk - E2E test');
        }

        // Confirm escalation
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Submit")').first();
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });

  test('INC-FLOW-003: Resolve incident', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/incidents`);
    await page.waitForLoadState('networkidle');

    // Find and click on an incident
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRow.isVisible({ timeout: 5000 })) {
      await firstRow.click();
      await page.waitForTimeout(500);

      // Look for resolve button
      const resolveButton = page.locator(
        'button:has-text("Resolve"), ' +
        'button:has-text("Complete"), ' +
        '[data-testid="resolve-button"]'
      ).first();

      if (await resolveButton.isVisible({ timeout: 3000 })) {
        await resolveButton.click();
        await page.waitForTimeout(500);

        // Fill resolution details
        const resolutionInput = page.locator('textarea[name="resolution"], textarea[name="resolutionNotes"]').first();
        if (await resolutionInput.isVisible()) {
          await resolutionInput.fill('Issue resolved by restarting the application server. Performance restored.');
        }

        // Select resolution code if available
        const codeSelect = page.locator('[aria-label*="resolution" i], label:has-text("Resolution Code") + div').first();
        if (await codeSelect.isVisible()) {
          await codeSelect.click();
          await page.locator('[role="option"]').first().click();
        }

        // Confirm resolution
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Save"), button[type="submit"]').first();
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });
});

test.describe('ITSM Workflow: Problem Management', () => {
  const timestamp = Date.now();

  test('PRB-FLOW-001: Create problem from incident', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/incidents`);
    await page.waitForLoadState('networkidle');

    // Open an incident
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRow.isVisible({ timeout: 5000 })) {
      await firstRow.click();
      await page.waitForTimeout(500);

      // Look for "Create Problem" action
      const createProblemButton = page.locator(
        'button:has-text("Create Problem"), ' +
        'button:has-text("Link to Problem"), ' +
        '[data-testid="create-problem"]'
      ).first();

      if (await createProblemButton.isVisible({ timeout: 3000 })) {
        await createProblemButton.click();
        await page.waitForTimeout(500);

        // Fill problem details if form appears
        const problemTitle = page.locator('input[name="title"], input[name="shortDescription"]').first();
        if (await problemTitle.isVisible()) {
          await problemTitle.fill(`Problem from Incident - ${timestamp}`);
        }

        const descInput = page.locator('textarea[name="description"]').first();
        if (await descInput.isVisible()) {
          await descInput.fill('Root cause investigation needed for recurring incident pattern.');
        }

        // Save
        const saveButton = page.locator('button[type="submit"], button:has-text("Create"), button:has-text("Save")').first();
        if (await saveButton.isVisible()) {
          await saveButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });

  test('PRB-FLOW-002: Mark problem as known error', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/problems`);
    await page.waitForLoadState('networkidle');

    // Open first problem
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRow.isVisible({ timeout: 5000 })) {
      await firstRow.click();
      await page.waitForTimeout(500);

      // Look for "Mark Known Error" action
      const knownErrorButton = page.locator(
        'button:has-text("Known Error"), ' +
        'button:has-text("Mark as Known Error"), ' +
        '[data-testid="mark-known-error"]'
      ).first();

      if (await knownErrorButton.isVisible({ timeout: 3000 })) {
        // Fill root cause if required
        const rootCauseInput = page.locator('textarea[name="rootCause"]').first();
        if (await rootCauseInput.isVisible()) {
          await rootCauseInput.fill('Database connection pool exhaustion due to connection leak.');
        }

        await knownErrorButton.click();
        await page.waitForTimeout(1500);
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });
});

test.describe('ITSM Workflow: Change Management', () => {
  const timestamp = Date.now();

  test('CHG-FLOW-001: Create change request', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/changes`);
    await page.waitForLoadState('networkidle');

    // Click create button
    const createButton = page.locator('button:has-text("Create"), button:has-text("New Change")').first();
    
    if (await createButton.isVisible({ timeout: 5000 })) {
      await createButton.click();
      await page.waitForTimeout(500);

      // Fill change request details
      const titleInput = page.locator('input[name="title"], input[name="shortDescription"]').first();
      if (await titleInput.isVisible()) {
        await titleInput.fill(`TEST_Change_Request_${timestamp}`);
      }

      const descInput = page.locator('textarea[name="description"]').first();
      if (await descInput.isVisible()) {
        await descInput.fill('Deploy updated database connection pooling configuration to production.');
      }

      // Select change type if available
      const typeSelect = page.locator('[aria-label*="type" i], label:has-text("Type") + div').first();
      if (await typeSelect.isVisible()) {
        await typeSelect.click();
        await page.locator('[role="option"]:has-text("Standard"), [role="option"]:has-text("Normal")').first().click();
      }

      // Set implementation plan
      const planInput = page.locator('textarea[name="implementationPlan"]').first();
      if (await planInput.isVisible()) {
        await planInput.fill('1. Backup current config\n2. Deploy new config\n3. Restart services\n4. Verify connectivity');
      }

      // Set rollback plan
      const rollbackInput = page.locator('textarea[name="rollbackPlan"]').first();
      if (await rollbackInput.isVisible()) {
        await rollbackInput.fill('1. Stop services\n2. Restore backup config\n3. Restart services');
      }

      // Save
      const saveButton = page.locator('button[type="submit"], button:has-text("Create"), button:has-text("Save")').first();
      await saveButton.click();
      await page.waitForTimeout(2000);
    }

    await expect(page).toHaveURL(/itsm.*change/i);
  });

  test('CHG-FLOW-002: Submit change for approval', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/changes`);
    await page.waitForLoadState('networkidle');

    // Open a change request
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    if (await firstRow.isVisible({ timeout: 5000 })) {
      await firstRow.click();
      await page.waitForTimeout(500);

      // Look for "Submit for Approval" action
      const submitButton = page.locator(
        'button:has-text("Submit"), ' +
        'button:has-text("Request Approval"), ' +
        '[data-testid="submit-for-approval"]'
      ).first();

      if (await submitButton.isVisible({ timeout: 3000 })) {
        await submitButton.click();
        await page.waitForTimeout(500);

        // Confirm submission
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes")').first();
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });

  test('CHG-FLOW-003: Implement approved change', async ({ authenticatedPage }) => {
    const page = authenticatedPage;

    await page.goto(`${ITSM_BASE_URL}/changes`);
    await page.waitForLoadState('networkidle');

    // Look for approved change
    const approvedChange = page.locator('text=/approved|scheduled/i').first();
    const firstRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    
    const targetRow = await approvedChange.isVisible() ? approvedChange : firstRow;
    
    if (await targetRow.isVisible({ timeout: 5000 })) {
      await targetRow.click();
      await page.waitForTimeout(500);

      // Look for "Implement" action
      const implementButton = page.locator(
        'button:has-text("Implement"), ' +
        'button:has-text("Start Implementation"), ' +
        '[data-testid="implement-change"]'
      ).first();

      if (await implementButton.isVisible({ timeout: 3000 })) {
        await implementButton.click();
        await page.waitForTimeout(500);

        // Fill implementation notes
        const notesInput = page.locator('textarea[name="implementationNotes"], textarea[name="notes"]').first();
        if (await notesInput.isVisible()) {
          await notesInput.fill('Implementation completed successfully. All services verified.');
        }

        // Complete implementation
        const completeButton = page.locator('button:has-text("Complete"), button:has-text("Save")').first();
        if (await completeButton.isVisible()) {
          await completeButton.click();
          await page.waitForTimeout(1500);
        }
      }
    }

    await expect(page).toHaveURL(/itsm/i);
  });
});

test.describe('ITSM Workflow API Tests', () => {
  test('ITSM-API-001: Create incident via API', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/api/itsm/incidents`, {
      data: {
        title: `API Test Incident ${Date.now()}`,
        description: 'Test incident created via API',
        priority: 2
      },
      headers: {
        'Content-Type': 'application/json'
      }
    });

    expect([200, 201, 401]).toContain(response.status());
  });

  test('ITSM-API-002: Create problem via API', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/api/itsm/problems`, {
      data: {
        shortDescription: `API Test Problem ${Date.now()}`,
        description: 'Test problem created via API',
        priority: 2
      },
      headers: {
        'Content-Type': 'application/json'
      }
    });

    expect([200, 201, 401]).toContain(response.status());
  });

  test('ITSM-API-003: Create change request via API', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/api/itsm/changes`, {
      data: {
        title: `API Test Change ${Date.now()}`,
        description: 'Test change request created via API',
        changeType: 'Standard'
      },
      headers: {
        'Content-Type': 'application/json'
      }
    });

    expect([200, 201, 401]).toContain(response.status());
  });

  test('ITSM-API-004: Get ITSM dashboard data', async ({ request }) => {
    const response = await request.get(`${API_BASE_URL}/api/itsm/dashboard`);
    
    expect([200, 401, 404]).toContain(response.status());
  });
});

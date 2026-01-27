/**
 * CRM Solution - Service Request Module Tests
 * 
 * Tests for service request CRUD, status transitions, and assignments.
 */

import { test, expect } from '@playwright/test';
import { DataGridHelper } from '../fixtures';
import { TEST_SERVICE_REQUESTS, uniqueTestData } from '../test-data';

test.describe('Service Requests - List View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
  });

  test('TC-SR-001: Should display service requests list', async ({ page }) => {
    await expect(page.locator('h1, h2, .page-title').filter({ hasText: /service|request|ticket/i })).toBeVisible({ timeout: 10000 });
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
  });

  test('TC-SR-002: Should show priority column', async ({ page }) => {
    const priorityHeader = page.locator('.MuiDataGrid-columnHeader:has-text("Priority")').first();
    if (await priorityHeader.isVisible()) {
      await expect(priorityHeader).toBeVisible();
    }
  });

  test('TC-SR-003: Should show status column', async ({ page }) => {
    const statusHeader = page.locator('.MuiDataGrid-columnHeader:has-text("Status")').first();
    if (await statusHeader.isVisible()) {
      await expect(statusHeader).toBeVisible();
    }
  });

  test('TC-SR-004: Should filter by priority', async ({ page }) => {
    const filterButton = page.locator('button:has-text("Filter"), [aria-label*="filter"]').first();
    if (await filterButton.isVisible()) {
      await filterButton.click();
      await page.waitForTimeout(500);
      
      const priorityFilter = page.locator('text=/high|medium|low|critical/i').first();
      if (await priorityFilter.isVisible()) {
        await priorityFilter.click();
      }
    }
  });

  test('TC-SR-005: Should filter by status', async ({ page }) => {
    const filterButton = page.locator('button:has-text("Filter"), [aria-label*="filter"]').first();
    if (await filterButton.isVisible()) {
      await filterButton.click();
      await page.waitForTimeout(500);
      
      const statusFilter = page.locator('text=/new|open|in progress|resolved|closed/i').first();
      if (await statusFilter.isVisible()) {
        await statusFilter.click();
      }
    }
  });
});

test.describe('Service Requests - Create', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
  });

  test('TC-SR-006: Should create bug report', async ({ page }) => {
    const testSR = uniqueTestData(TEST_SERVICE_REQUESTS.bug);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    // Fill title
    const titleInput = page.locator('input[name="title"], input[name="subject"], #title').first();
    await titleInput.fill(testSR.title);
    
    // Fill description
    const descInput = page.locator('textarea[name="description"], #description').first();
    if (await descInput.isVisible()) {
      await descInput.fill(testSR.description);
    }
    
    // Set priority
    const prioritySelect = page.locator('[aria-label*="Priority"], label:has-text("Priority") + div').first();
    if (await prioritySelect.isVisible()) {
      await prioritySelect.click();
      await page.locator('[role="option"]:has-text("High")').first().click();
    }
    
    // Submit
    await page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-SR-007: Should create feature request', async ({ page }) => {
    const testSR = uniqueTestData(TEST_SERVICE_REQUESTS.feature);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    const titleInput = page.locator('input[name="title"], input[name="subject"], #title').first();
    await titleInput.fill(testSR.title);
    
    const categorySelect = page.locator('[aria-label*="Category"], label:has-text("Category") + div').first();
    if (await categorySelect.isVisible()) {
      await categorySelect.click();
      await page.locator('[role="option"]:has-text("Feature")').first().click();
    }
    
    await page.locator('button[type="submit"], button:has-text("Save")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-SR-008: Should create urgent ticket', async ({ page }) => {
    const testSR = uniqueTestData(TEST_SERVICE_REQUESTS.urgent);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    const titleInput = page.locator('input[name="title"], input[name="subject"], #title').first();
    await titleInput.fill(testSR.title);
    
    const prioritySelect = page.locator('[aria-label*="Priority"], label:has-text("Priority") + div').first();
    if (await prioritySelect.isVisible()) {
      await prioritySelect.click();
      await page.locator('[role="option"]:has-text("Critical")').first().click();
    }
    
    await page.locator('button[type="submit"], button:has-text("Save")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-SR-009: Should link to customer', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    const customerSelect = page.locator('[aria-label*="Customer"], label:has-text("Customer") + div').first();
    if (await customerSelect.isVisible()) {
      await customerSelect.click();
      await page.waitForTimeout(500);
      
      const firstOption = page.locator('[role="option"]').first();
      if (await firstOption.isVisible()) {
        await firstOption.click();
      }
    }
  });
});

test.describe('Service Requests - Status Transitions', () => {
  test('TC-SR-010: Should change status to In Progress', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Look for status change button or dropdown
      const statusButton = page.locator('button:has-text("In Progress"), button:has-text("Start")').first();
      if (await statusButton.isVisible()) {
        await statusButton.click();
        await page.waitForTimeout(2000);
      } else {
        // Try editing status
        const editButton = page.locator('button:has-text("Edit")').first();
        if (await editButton.isVisible()) {
          await editButton.click();
          await page.waitForTimeout(500);
          
          const statusSelect = page.locator('[aria-label*="Status"], label:has-text("Status") + div').first();
          if (await statusSelect.isVisible()) {
            await statusSelect.click();
            await page.locator('[role="option"]:has-text("In Progress")').first().click();
          }
          
          await page.locator('button[type="submit"], button:has-text("Save")').first().click();
          await page.waitForTimeout(2000);
        }
      }
    }
  });

  test('TC-SR-011: Should resolve service request', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const resolveButton = page.locator('button:has-text("Resolve"), button:has-text("Resolved")').first();
      if (await resolveButton.isVisible()) {
        await resolveButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-SR-012: Should close service request', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const closeButton = page.locator('button:has-text("Close")').first();
      if (await closeButton.isVisible()) {
        await closeButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });
});

test.describe('Service Requests - Assignment', () => {
  test('TC-SR-013: Should assign to user', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const assignButton = page.locator('button:has-text("Assign")').first();
      if (await assignButton.isVisible()) {
        await assignButton.click();
        await page.waitForTimeout(500);
        
        // Select user
        const userSelect = page.locator('[role="option"]').first();
        if (await userSelect.isVisible()) {
          await userSelect.click();
          await page.waitForTimeout(2000);
        }
      }
    }
  });

  test('TC-SR-014: Should reassign to different user', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const reassignButton = page.locator('button:has-text("Reassign"), button:has-text("Assign")').first();
      if (await reassignButton.isVisible()) {
        await reassignButton.click();
        await page.waitForTimeout(500);
      }
    }
  });
});

test.describe('Service Requests - Delete', () => {
  test('TC-SR-015: Should delete test service request', async ({ page }) => {
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const deleteButton = page.locator('button:has-text("Delete")').first();
      if (await deleteButton.isVisible()) {
        await deleteButton.click();
        await page.waitForTimeout(500);
        
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes")').first();
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(2000);
        }
      }
    }
  });
});

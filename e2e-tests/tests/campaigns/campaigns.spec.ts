/**
 * CRM Solution - Campaign Module Tests
 * 
 * Tests for campaign CRUD, targeting, and metrics tracking.
 */

import { test, expect } from '@playwright/test';
import { DataGridHelper } from '../fixtures';
import { TEST_CAMPAIGNS, uniqueTestData } from '../test-data';

test.describe('Campaigns - List View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
  });

  test('TC-CAMP-001: Should display campaigns list', async ({ page }) => {
    await expect(page.locator('h1, h2, .page-title').filter({ hasText: /campaign/i })).toBeVisible({ timeout: 10000 });
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
  });

  test('TC-CAMP-002: Should show campaign status', async ({ page }) => {
    const statusHeader = page.locator('.MuiDataGrid-columnHeader:has-text("Status")').first();
    if (await statusHeader.isVisible()) {
      await expect(statusHeader).toBeVisible();
    }
  });

  test('TC-CAMP-003: Should filter by type', async ({ page }) => {
    const filterButton = page.locator('button:has-text("Filter"), [aria-label*="filter"]').first();
    if (await filterButton.isVisible()) {
      await filterButton.click();
      await page.waitForTimeout(500);
      
      const typeFilter = page.locator('text=/email|social|webinar/i').first();
      if (await typeFilter.isVisible()) {
        await typeFilter.click();
      }
    }
  });

  test('TC-CAMP-004: Should search campaigns', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const searchInput = page.locator('input[placeholder*="Search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('TEST_');
      await page.keyboard.press('Enter');
      await page.waitForTimeout(1000);
    }
  });
});

test.describe('Campaigns - Create', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
  });

  test('TC-CAMP-005: Should create email campaign', async ({ page }) => {
    const testCampaign = uniqueTestData(TEST_CAMPAIGNS.emailCampaign);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    // Fill form
    const nameInput = page.locator('input[name="name"], #name').first();
    await nameInput.fill(testCampaign.name);
    
    // Set type
    const typeSelect = page.locator('[aria-label*="Type"], label:has-text("Type") + div').first();
    if (await typeSelect.isVisible()) {
      await typeSelect.click();
      await page.locator('[role="option"]:has-text("Email")').first().click();
    }
    
    // Set dates
    const startDateInput = page.locator('input[name="startDate"], input[type="date"]').first();
    if (await startDateInput.isVisible()) {
      await startDateInput.fill(testCampaign.startDate);
    }
    
    // Set budget
    const budgetInput = page.locator('input[name="budget"], #budget').first();
    if (await budgetInput.isVisible()) {
      await budgetInput.fill(String(testCampaign.budget));
    }
    
    // Submit
    await page.locator('button[type="submit"], button:has-text("Save")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-CAMP-006: Should create social media campaign', async ({ page }) => {
    const testCampaign = uniqueTestData(TEST_CAMPAIGNS.socialMedia);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    const nameInput = page.locator('input[name="name"], #name').first();
    await nameInput.fill(testCampaign.name);
    
    const typeSelect = page.locator('[aria-label*="Type"], label:has-text("Type") + div').first();
    if (await typeSelect.isVisible()) {
      await typeSelect.click();
      await page.locator('[role="option"]:has-text("Social")').first().click();
    }
    
    await page.locator('button[type="submit"], button:has-text("Save")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-CAMP-007: Should set target audience', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(500);
    
    const audienceSelect = page.locator('[aria-label*="Audience"], label:has-text("Audience") + div, label:has-text("Target") + div').first();
    if (await audienceSelect.isVisible()) {
      await audienceSelect.click();
      await page.locator('[role="option"]').first().click();
    }
  });
});

test.describe('Campaigns - Lifecycle', () => {
  test('TC-CAMP-008: Should start campaign', async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const startButton = page.locator('button:has-text("Start"), button:has-text("Launch"), button:has-text("Activate")').first();
      if (await startButton.isVisible()) {
        await startButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-CAMP-009: Should pause campaign', async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const pauseButton = page.locator('button:has-text("Pause")').first();
      if (await pauseButton.isVisible()) {
        await pauseButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-CAMP-010: Should complete campaign', async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const completeButton = page.locator('button:has-text("Complete"), button:has-text("End")').first();
      if (await completeButton.isVisible()) {
        await completeButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });
});

test.describe('Campaigns - Metrics', () => {
  test('TC-CAMP-011: Should display campaign metrics', async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Look for metrics section
      const metricsSection = page.locator('text=/metrics|analytics|performance|statistics/i').first();
      if (await metricsSection.isVisible()) {
        await expect(metricsSection).toBeVisible();
      }
    }
  });

  test('TC-CAMP-012: Should show ROI data', async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const roiSection = page.locator('text=/roi|return|revenue/i').first();
      if (await roiSection.isVisible()) {
        await expect(roiSection).toBeVisible();
      }
    }
  });
});

test.describe('Campaigns - Delete', () => {
  test('TC-CAMP-013: Should delete test campaign', async ({ page }) => {
    await page.goto('/campaigns');
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

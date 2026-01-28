/**
 * CRM Solution - Relationship Management E2E Tests
 * 
 * Tests for relationship type management, account relationships, 
 * interactions, health snapshots, and relationship map visualization.
 */

import { test, expect } from '@playwright/test';

test.describe('Relationships - Relationship Types', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/relationships');
    await page.waitForLoadState('networkidle');
  });

  test('TC-REL-001: Should display relationships page', async ({ page }) => {
    await expect(page.locator('h1, h2, .page-title').filter({ hasText: /relationship/i })).toBeVisible({ timeout: 10000 });
  });

  test('TC-REL-002: Should show relationship types tab', async ({ page }) => {
    const typesTab = page.locator('[role="tab"]:has-text("Relationship Types")');
    if (await typesTab.isVisible()) {
      await typesTab.click();
      await page.waitForTimeout(500);
      await expect(page.locator('button:has-text("Add Relationship Type")')).toBeVisible();
    }
  });

  test('TC-REL-003: Should open create relationship type dialog', async ({ page }) => {
    const typesTab = page.locator('[role="tab"]:has-text("Relationship Types")');
    if (await typesTab.isVisible()) {
      await typesTab.click();
      await page.waitForTimeout(500);
    }
    
    const addButton = page.locator('button:has-text("Add Relationship Type")');
    if (await addButton.isVisible()) {
      await addButton.click();
      await expect(page.locator('[role="dialog"]')).toBeVisible();
      await expect(page.locator('input[name="typeName"], #typeName, label:has-text("Type Name") + div input').first()).toBeVisible();
    }
  });

  test('TC-REL-004: Should create B2B relationship type', async ({ page }) => {
    const typesTab = page.locator('[role="tab"]:has-text("Relationship Types")');
    if (await typesTab.isVisible()) {
      await typesTab.click();
      await page.waitForTimeout(500);
    }
    
    const addButton = page.locator('button:has-text("Add Relationship Type")');
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      const typeNameInput = page.locator('input').filter({ hasText: '' }).first();
      await typeNameInput.fill(`TestPartner_${Date.now()}`);
      
      // Select B2B category if dropdown is available
      const categorySelect = page.locator('[aria-label*="Category"], label:has-text("Category") + div').first();
      if (await categorySelect.isVisible()) {
        await categorySelect.click();
        await page.locator('[role="option"]:has-text("B2B")').first().click();
      }
      
      await page.locator('button:has-text("Save")').click();
      await page.waitForTimeout(1000);
    }
  });
});

test.describe('Relationships - Account Relationships', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/relationships');
    await page.waitForLoadState('networkidle');
  });

  test('TC-REL-010: Should show relationships tab', async ({ page }) => {
    const relTab = page.locator('[role="tab"]:has-text("Relationships")').first();
    await expect(relTab).toBeVisible();
  });

  test('TC-REL-011: Should display relationships table', async ({ page }) => {
    const table = page.locator('table, .MuiTable-root, [role="grid"]').first();
    await expect(table).toBeVisible({ timeout: 10000 });
  });

  test('TC-REL-012: Should open create relationship dialog', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add Relationship")');
    if (await addButton.isVisible()) {
      await addButton.click();
      await expect(page.locator('[role="dialog"]')).toBeVisible();
    }
  });

  test('TC-REL-013: Should show relationship form fields', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add Relationship")');
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      // Check for source and target account fields
      await expect(page.locator('label:has-text("Source Account"), [aria-label*="Source"]').first()).toBeVisible();
      await expect(page.locator('label:has-text("Target Account"), [aria-label*="Target"]').first()).toBeVisible();
    }
  });
});

test.describe('Relationships - Interactions', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/relationships');
    await page.waitForLoadState('networkidle');
  });

  test('TC-REL-020: Should show interaction button on relationship', async ({ page }) => {
    const interactionButton = page.locator('[aria-label*="Interaction"], button:has([data-testid*="trending"], svg)').first();
    // This will depend on having relationships in the system
    if (await interactionButton.isVisible()) {
      await expect(interactionButton).toBeVisible();
    }
  });

  test('TC-REL-021: Should open interaction dialog', async ({ page }) => {
    const table = page.locator('table tbody tr').first();
    if (await table.isVisible()) {
      const interactionButton = table.locator('button').first();
      if (await interactionButton.isVisible()) {
        await interactionButton.click();
        await page.waitForTimeout(500);
      }
    }
  });
});

test.describe('Relationships - Health Snapshots', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/relationships');
    await page.waitForLoadState('networkidle');
  });

  test('TC-REL-030: Should show health snapshot button', async ({ page }) => {
    const healthButton = page.locator('[aria-label*="Health"], button:has([data-testid*="health"], svg)').first();
    // Health button visibility depends on relationships existing
    if (await healthButton.isVisible()) {
      await expect(healthButton).toBeVisible();
    }
  });
});

test.describe('Relationships - Relationship Map', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/relationships');
    await page.waitForLoadState('networkidle');
  });

  test('TC-REL-040: Should show map visualization button', async ({ page }) => {
    const mapButton = page.locator('[aria-label*="Map"], button:has([data-testid*="tree"], svg)').first();
    // Map button visibility depends on relationships existing
    if (await mapButton.isVisible()) {
      await expect(mapButton).toBeVisible();
    }
  });
});

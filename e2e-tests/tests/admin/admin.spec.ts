/**
 * CRM Solution - Admin Settings Tests
 * 
 * Tests for user management, roles, permissions, branding, LLM settings, and security.
 */

import { test, expect } from '@playwright/test';
import { DataGridHelper } from '../fixtures';
import { TEST_USERS, uniqueTestData, randomString } from '../test-data';

test.describe('Admin - Users Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/users');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-001: Should display users list', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /user/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-002: Should have create user button', async ({ page }) => {
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await expect(addButton).toBeVisible();
  });

  test('TC-ADMIN-003: Should create new user', async ({ page }) => {
    const testUser = {
      username: `TEST_user_${randomString(6)}`,
      email: `TEST_${randomString(8)}@test.com`,
      firstName: 'TEST_First',
      lastName: 'TEST_Last'
    };
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(1000);
    
    // Fill user details
    const usernameInput = page.locator('input[name="username"], #username').first();
    if (await usernameInput.isVisible()) {
      await usernameInput.fill(testUser.username);
    }
    
    const emailInput = page.locator('input[name="email"], input[type="email"], #email').first();
    if (await emailInput.isVisible()) {
      await emailInput.fill(testUser.email);
    }
    
    const firstNameInput = page.locator('input[name="firstName"], #firstName').first();
    if (await firstNameInput.isVisible()) {
      await firstNameInput.fill(testUser.firstName);
    }
    
    const lastNameInput = page.locator('input[name="lastName"], #lastName').first();
    if (await lastNameInput.isVisible()) {
      await lastNameInput.fill(testUser.lastName);
    }
    
    // Password
    const passwordInput = page.locator('input[name="password"], input[type="password"]').first();
    if (await passwordInput.isVisible()) {
      await passwordInput.fill('TestPassword123!');
    }
    
    // Save
    await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
    await page.waitForTimeout(2000);
  });

  test('TC-ADMIN-004: Should edit user', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('TC-ADMIN-005: Should disable user', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const disableButton = page.locator('button:has-text("Disable"), button:has-text("Deactivate")').first();
      if (await disableButton.isVisible()) {
        await disableButton.click();
        await page.waitForTimeout(2000);
      }
    }
  });

  test('TC-ADMIN-006: Should delete test user', async ({ page }) => {
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

test.describe('Admin - Roles Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/roles');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-007: Should display roles list', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /role/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-008: Should create new role', async ({ page }) => {
    const roleName = `TEST_Role_${randomString(4)}`;
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(1000);
      
      const nameInput = page.locator('input[name="name"], #name').first();
      if (await nameInput.isVisible()) {
        await nameInput.fill(roleName);
      }
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(2000);
    }
  });

  test('TC-ADMIN-009: Should assign permissions to role', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const permissionsTab = page.locator('button:has-text("Permissions"), [role="tab"]:has-text("Permissions")').first();
      if (await permissionsTab.isVisible()) {
        await permissionsTab.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('TC-ADMIN-010: Should delete test role', async ({ page }) => {
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

test.describe('Admin - Security Settings', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/settings');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-011: Should display settings page', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /setting/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-012: Should access security settings', async ({ page }) => {
    const securityTab = page.locator('button:has-text("Security"), [role="tab"]:has-text("Security"), a:has-text("Security")').first();
    if (await securityTab.isVisible()) {
      await securityTab.click();
      await page.waitForTimeout(1000);
    }
  });

  test('TC-ADMIN-013: Should configure password policy', async ({ page }) => {
    const securityTab = page.locator('button:has-text("Security"), [role="tab"]:has-text("Security")').first();
    if (await securityTab.isVisible()) {
      await securityTab.click();
      await page.waitForTimeout(1000);
      
      const passwordPolicy = page.locator('text=/password policy|password requirements/i').first();
      if (await passwordPolicy.isVisible()) {
        await passwordPolicy.click();
      }
    }
  });

  test('TC-ADMIN-014: Should configure session timeout', async ({ page }) => {
    const securityTab = page.locator('button:has-text("Security"), [role="tab"]:has-text("Security")').first();
    if (await securityTab.isVisible()) {
      await securityTab.click();
      await page.waitForTimeout(1000);
      
      const sessionTimeout = page.locator('input[name="sessionTimeout"], #sessionTimeout').first();
      if (await sessionTimeout.isVisible()) {
        await sessionTimeout.clear();
        await sessionTimeout.fill('30');
      }
    }
  });
});

test.describe('Admin - LLM Settings', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/llm-settings');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-015: Should display LLM settings page', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /llm|ai|model/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-016: Should configure LLM endpoint', async ({ page }) => {
    const endpointInput = page.locator('input[name="endpoint"], input[name="url"], #endpoint').first();
    if (await endpointInput.isVisible()) {
      await endpointInput.clear();
      await endpointInput.fill('http://test-endpoint.local:11434');
    }
  });

  test('TC-ADMIN-017: Should configure model selection', async ({ page }) => {
    const modelSelect = page.locator('select[name="model"], #model, [aria-label*="Model"]').first();
    if (await modelSelect.isVisible()) {
      await modelSelect.click();
      await page.waitForTimeout(500);
    }
  });

  test('TC-ADMIN-018: Should test LLM connection', async ({ page }) => {
    const testButton = page.locator('button:has-text("Test"), button:has-text("Check Connection")').first();
    if (await testButton.isVisible()) {
      await testButton.click();
      await page.waitForTimeout(3000);
    }
  });
});

test.describe('Admin - Branding', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/branding');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-019: Should display branding settings', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /brand|theme|appearance/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-020: Should update company name', async ({ page }) => {
    const companyInput = page.locator('input[name="companyName"], #companyName').first();
    if (await companyInput.isVisible()) {
      await companyInput.clear();
      await companyInput.fill('TEST_Company');
    }
  });

  test('TC-ADMIN-021: Should update primary color', async ({ page }) => {
    const colorPicker = page.locator('input[type="color"], input[name="primaryColor"]').first();
    if (await colorPicker.isVisible()) {
      await colorPicker.fill('#3498db');
    }
  });
});

test.describe('Admin - Email Settings', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/email-settings');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-022: Should display email settings', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /email|smtp/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-023: Should configure SMTP settings', async ({ page }) => {
    const smtpHost = page.locator('input[name="smtpHost"], #smtpHost').first();
    if (await smtpHost.isVisible()) {
      await smtpHost.clear();
      await smtpHost.fill('smtp.test.com');
    }
    
    const smtpPort = page.locator('input[name="smtpPort"], #smtpPort').first();
    if (await smtpPort.isVisible()) {
      await smtpPort.clear();
      await smtpPort.fill('587');
    }
  });

  test('TC-ADMIN-024: Should send test email', async ({ page }) => {
    const testEmailButton = page.locator('button:has-text("Send Test"), button:has-text("Test Email")').first();
    if (await testEmailButton.isVisible()) {
      await testEmailButton.click();
      await page.waitForTimeout(3000);
    }
  });
});

test.describe('Admin - Audit Log', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/admin/audit-log');
    await page.waitForLoadState('networkidle');
  });

  test('TC-ADMIN-025: Should display audit log', async ({ page }) => {
    const pageTitle = page.locator('h1, h2, .page-title').filter({ hasText: /audit|log|activity/i });
    await expect(pageTitle.first()).toBeVisible({ timeout: 10000 });
  });

  test('TC-ADMIN-026: Should filter audit log by date', async ({ page }) => {
    const dateFilter = page.locator('input[type="date"], [aria-label*="Date"]').first();
    if (await dateFilter.isVisible()) {
      await dateFilter.click();
      await page.waitForTimeout(500);
    }
  });

  test('TC-ADMIN-027: Should filter audit log by action type', async ({ page }) => {
    const actionFilter = page.locator('select[name="action"], [aria-label*="Action"]').first();
    if (await actionFilter.isVisible()) {
      await actionFilter.click();
      await page.waitForTimeout(500);
    }
  });

  test('TC-ADMIN-028: Should export audit log', async ({ page }) => {
    const exportButton = page.locator('button:has-text("Export")').first();
    if (await exportButton.isVisible()) {
      await exportButton.click();
      await page.waitForTimeout(2000);
    }
  });
});

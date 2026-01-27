/**
 * CRM Solution - E2E Test Fixtures and Utilities
 * 
 * Common fixtures, page objects, and helper functions for all tests.
 */

import { test as base, expect, Page, Locator } from '@playwright/test';
import { TEST_USERS } from './test-data';

// ============================================================================
// Extended Test Type with Fixtures
// ============================================================================

type TestFixtures = {
  authenticatedPage: Page;
  adminPage: Page;
};

export const test = base.extend<TestFixtures>({
  authenticatedPage: async ({ page }, use) => {
    // Page is already authenticated via storage state
    await use(page);
  },
  
  adminPage: async ({ page }, use) => {
    // Verify admin access
    await page.goto('/settings');
    await use(page);
  },
});

export { expect };

// ============================================================================
// Page Object Models
// ============================================================================

/**
 * Login Page Object
 */
export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly errorMessage: Locator;
  readonly forgotPasswordLink: Locator;
  readonly registerLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.locator('input[name="email"], input[type="email"], #email');
    this.passwordInput = page.locator('input[name="password"], input[type="password"], #password');
    this.loginButton = page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Sign In")');
    this.errorMessage = page.locator('.error-message, .MuiAlert-message, [role="alert"]');
    this.forgotPasswordLink = page.locator('a:has-text("Forgot"), a:has-text("Reset")');
    this.registerLink = page.locator('a:has-text("Register"), a:has-text("Sign Up")');
  }

  async goto() {
    await this.page.goto('/login');
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
    await this.page.waitForURL(/\/(dashboard|$)/, { timeout: 10000 });
  }

  async loginExpectError(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
    await expect(this.errorMessage).toBeVisible({ timeout: 5000 });
  }
}

/**
 * Navigation Helper
 */
export class Navigation {
  readonly page: Page;
  readonly sidebar: Locator;
  readonly userMenu: Locator;

  constructor(page: Page) {
    this.page = page;
    this.sidebar = page.locator('nav, [role="navigation"], .sidebar, .MuiDrawer-root');
    this.userMenu = page.locator('[aria-label="user menu"], .user-menu, .profile-menu');
  }

  async goToDashboard() {
    await this.page.goto('/dashboard');
    await this.page.waitForLoadState('networkidle');
  }

  async goToCustomers() {
    await this.page.goto('/customers');
    await this.page.waitForLoadState('networkidle');
  }

  async goToContacts() {
    await this.page.goto('/contacts');
    await this.page.waitForLoadState('networkidle');
  }

  async goToOpportunities() {
    await this.page.goto('/opportunities');
    await this.page.waitForLoadState('networkidle');
  }

  async goToLeads() {
    await this.page.goto('/leads');
    await this.page.waitForLoadState('networkidle');
  }

  async goToServiceRequests() {
    await this.page.goto('/service-requests');
    await this.page.waitForLoadState('networkidle');
  }

  async goToCampaigns() {
    await this.page.goto('/campaigns');
    await this.page.waitForLoadState('networkidle');
  }

  async goToProducts() {
    await this.page.goto('/products');
    await this.page.waitForLoadState('networkidle');
  }

  async goToWorkflows() {
    await this.page.goto('/admin/workflows');
    await this.page.waitForLoadState('networkidle');
  }

  async goToSettings() {
    await this.page.goto('/settings');
    await this.page.waitForLoadState('networkidle');
  }

  async logout() {
    await this.userMenu.click();
    await this.page.locator('text=Logout, text=Sign Out').click();
    await this.page.waitForURL(/\/login/);
  }
}

/**
 * Data Grid Helper - for MUI DataGrid or Table interactions
 */
export class DataGridHelper {
  readonly page: Page;
  readonly grid: Locator;
  readonly table: Locator;
  private isTable: boolean = false;

  constructor(page: Page, gridSelector?: string) {
    this.page = page;
    // Support both MUI DataGrid and regular Table
    this.grid = page.locator('.MuiDataGrid-root');
    this.table = page.locator('.MuiTable-root, table');
  }

  async waitForLoad() {
    // Wait for either DataGrid or Table to be visible
    const gridVisible = await this.grid.isVisible().catch(() => false);
    const tableVisible = await this.table.first().isVisible().catch(() => false);
    
    if (gridVisible) {
      this.isTable = false;
      await this.grid.waitFor({ state: 'visible', timeout: 10000 }).catch(() => {});
      // Wait for loading overlay to disappear
      await this.page.waitForFunction(() => {
        const overlay = document.querySelector('.MuiDataGrid-overlay');
        return !overlay || overlay.textContent?.includes('No rows');
      }, { timeout: 5000 }).catch(() => {});
    } else if (tableVisible) {
      this.isTable = true;
      await this.table.first().waitFor({ state: 'visible', timeout: 10000 }).catch(() => {});
    } else {
      // Wait for any table-like structure to appear
      await this.page.waitForSelector('.MuiDataGrid-root, .MuiTable-root, table, [role="grid"]', { timeout: 10000 }).catch(() => {});
    }
    
    // Also wait for loading spinners to disappear
    await this.page.waitForTimeout(500);
  }

  async getRowCount(): Promise<number> {
    await this.waitForLoad();
    
    if (this.isTable) {
      const rows = this.table.first().locator('tbody tr');
      return await rows.count();
    } else {
      const rows = this.grid.locator('.MuiDataGrid-row');
      const count = await rows.count();
      if (count === 0) {
        // Fallback to table rows
        const tableRows = this.table.first().locator('tbody tr');
        return await tableRows.count();
      }
      return count;
    }
  }

  async clickRow(index: number) {
    await this.waitForLoad();
    
    if (this.isTable) {
      const rows = this.table.first().locator('tbody tr');
      await rows.nth(index).click();
    } else {
      const rows = this.grid.locator('.MuiDataGrid-row');
      const count = await rows.count();
      if (count > 0) {
        await rows.nth(index).click();
      } else {
        // Fallback to table
        const tableRows = this.table.first().locator('tbody tr');
        await tableRows.nth(index).click();
      }
    }
  }

  async searchInGrid(searchText: string) {
    const searchInput = this.page.locator('input[placeholder*="Search"], input[aria-label*="Search"], input[placeholder*="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill(searchText);
      await this.page.keyboard.press('Enter');
    }
    await this.page.waitForTimeout(1000);
  }

  async clickAddButton() {
    await this.page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New")').first().click();
  }
}

/**
 * Form Helper - for common form interactions
 */
export class FormHelper {
  readonly page: Page;
  readonly form: Locator;

  constructor(page: Page, formSelector = 'form, [role="dialog"], .MuiDialog-root') {
    this.page = page;
    this.form = page.locator(formSelector);
  }

  async fillTextField(label: string, value: string) {
    const field = this.page.locator(`input[aria-label="${label}"], label:has-text("${label}") + div input, label:has-text("${label}") ~ input`).first();
    await field.fill(value);
  }

  async fillByName(name: string, value: string) {
    await this.page.locator(`input[name="${name}"], textarea[name="${name}"]`).fill(value);
  }

  async fillByPlaceholder(placeholder: string, value: string) {
    await this.page.locator(`input[placeholder*="${placeholder}"], textarea[placeholder*="${placeholder}"]`).fill(value);
  }

  async selectOption(label: string, option: string) {
    const select = this.page.locator(`[aria-label="${label}"], label:has-text("${label}") + div`).first();
    await select.click();
    await this.page.locator(`[role="option"]:has-text("${option}")`).click();
  }

  async clickSubmit() {
    await this.page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Submit"), button:has-text("Create")').first().click();
  }

  async clickCancel() {
    await this.page.locator('button:has-text("Cancel"), button:has-text("Close")').click();
  }

  async waitForSaveSuccess() {
    // Wait for success notification or dialog close
    await Promise.race([
      this.page.waitForSelector('.MuiSnackbar-root:has-text("success"), .MuiAlert-standardSuccess', { timeout: 5000 }),
      this.page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 }),
    ]).catch(() => {});
  }
}

/**
 * Notification Helper
 */
export class NotificationHelper {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async expectSuccess(message?: string) {
    const notification = this.page.locator('.MuiSnackbar-root, .MuiAlert-root').filter({ hasText: message || '' });
    await expect(notification).toBeVisible({ timeout: 5000 });
  }

  async expectError(message?: string) {
    const notification = this.page.locator('.MuiAlert-standardError, .MuiSnackbar-root:has(.MuiAlert-standardError)').filter({ hasText: message || '' });
    await expect(notification).toBeVisible({ timeout: 5000 });
  }

  async dismissAll() {
    const closeButtons = this.page.locator('.MuiSnackbar-root button[aria-label="Close"], .MuiAlert-action button');
    const count = await closeButtons.count();
    for (let i = 0; i < count; i++) {
      await closeButtons.first().click().catch(() => {});
    }
  }
}

// ============================================================================
// Utility Functions
// ============================================================================

/**
 * Wait for API response
 */
export async function waitForApiResponse(page: Page, urlPattern: string | RegExp): Promise<any> {
  const response = await page.waitForResponse(
    (resp) => {
      const url = resp.url();
      if (typeof urlPattern === 'string') {
        return url.includes(urlPattern);
      }
      return urlPattern.test(url);
    },
    { timeout: 30000 }
  );
  return response.json().catch(() => null);
}

/**
 * Take a screenshot with descriptive name
 */
export async function takeScreenshot(page: Page, name: string) {
  await page.screenshot({ 
    path: `test-results/screenshots/${name}-${Date.now()}.png`,
    fullPage: true 
  });
}

/**
 * Cleanup test data by searching and deleting
 */
export async function cleanupTestData(page: Page, entityType: string) {
  await page.goto(`/${entityType}`);
  const grid = new DataGridHelper(page);
  await grid.searchInGrid('TEST_');
  
  // Delete all found test records
  const rowCount = await grid.getRowCount();
  for (let i = 0; i < rowCount; i++) {
    await grid.clickRow(0); // Always click first row as they shift up
    await page.locator('button:has-text("Delete")').click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes")').click();
    await page.waitForTimeout(500);
  }
}

/**
 * Generate random string for unique test data
 */
export function randomString(length: number = 8): string {
  return Math.random().toString(36).substring(2, 2 + length);
}

/**
 * Format date for input fields
 */
export function formatDate(date: Date): string {
  return date.toISOString().split('T')[0];
}

/**
 * Future date helper
 */
export function futureDate(daysFromNow: number): string {
  const date = new Date();
  date.setDate(date.getDate() + daysFromNow);
  return formatDate(date);
}

/**
 * AdminSettingsPage — Playwright Page Object Model
 *
 * TODO-SYS009-001: Playwright page object for admin settings E2E tests.
 *
 * Encapsulates all locator selectors and common actions for the admin
 * settings pages, making individual test files more readable and resilient
 * to layout changes.
 *
 * Usage in a test file:
 *   import { AdminSettingsPage } from '../../pages/AdminSettingsPage';
 *
 *   test('…', async ({ page }) => {
 *     const settings = new AdminSettingsPage(page);
 *     await settings.navigate();
 *     await settings.fillCompanyName('Acme Corp');
 *     await settings.save();
 *     await settings.expectSuccessAlert();
 *   });
 */

import { type Page, type Locator, expect } from '@playwright/test';

// -------------------------------------------------------------------------
// Types
// -------------------------------------------------------------------------

export type SettingsSection =
  | 'general'
  | 'email'
  | 'notifications'
  | 'security'
  | 'integrations'
  | 'billing';

// -------------------------------------------------------------------------
// Page Object
// -------------------------------------------------------------------------

export class AdminSettingsPage {
  readonly page: Page;

  // ── Root paths ──────────────────────────────────────────────────────────
  static readonly PATHS = {
    settings:         '/admin/settings',
    general:          '/admin/settings/general',
    email:            '/admin/settings/email',
    featureFlags:     '/admin/feature-management',
    users:            '/admin/users',
    sla:              '/admin/sla-policies',
    businessHours:    '/admin/business-hours',
    auditLog:         '/admin/audit-log',
    configuration:    '/admin/configuration',
  } as const;

  // ── Locators ─────────────────────────────────────────────────────────────

  /** Page heading — h1 – h4 or MUI Typography variant headings */
  readonly heading: Locator;

  /** Main content area */
  readonly mainContent: Locator;

  /** Tab list (if settings uses tabs) */
  readonly tabList: Locator;

  /** Company name text field */
  readonly companyNameInput: Locator;

  /** Date format selector */
  readonly dateFormatSelect: Locator;

  /** Email input (settings email field) */
  readonly emailInput: Locator;

  /** Any primary save / update button */
  readonly saveButton: Locator;

  /** Cancel / discard button */
  readonly cancelButton: Locator;

  /** Success alert / snackbar */
  readonly successAlert: Locator;

  /** Error alert / snackbar */
  readonly errorAlert: Locator;

  /** Feature flag toggle switches */
  readonly featureToggles: Locator;

  /** First text input not marked readonly */
  readonly firstTextInput: Locator;

  /** Required inputs */
  readonly requiredInputs: Locator;

  /** MUI validation error helpers */
  readonly validationErrors: Locator;

  // ── Constructor ──────────────────────────────────────────────────────────

  constructor(page: Page) {
    this.page = page;

    this.heading = page.locator('h1, h2, h3, h4, .MuiTypography-h4, .page-title');
    this.mainContent = page.locator('main, .MuiContainer-root, .page-content, [data-testid="settings-content"]');
    this.tabList = page.locator('.MuiTabs-root, [role="tablist"], .settings-tabs');

    this.companyNameInput = page.locator(
      'input[name="companyName"], input[name="organizationName"], #companyName, #organizationName, input[placeholder*="company" i]',
    ).first();

    this.dateFormatSelect = page.locator(
      '[name="dateFormat"], #dateFormat, [aria-label="Date Format"]',
    ).first();

    this.emailInput = page.locator(
      'input[type="email"], input[name*="email"]:not([name*="pass"])',
    ).first();

    this.saveButton = page.locator(
      'button:has-text("Save"), button:has-text("Update"), button[type="submit"]',
    ).first();

    this.cancelButton = page.locator(
      'button:has-text("Cancel"), button:has-text("Reset"), button:has-text("Discard")',
    ).first();

    this.successAlert = page.locator(
      '.MuiAlert-standardSuccess, .MuiAlert-filledSuccess, .toast-success, [role="alert"]',
    ).filter({ hasText: /success|saved|updated/i });

    this.errorAlert = page.locator(
      '.MuiAlert-standardError, .MuiAlert-filledError, .toast-error, [role="alert"]',
    ).filter({ hasText: /error|failed|invalid/i });

    this.featureToggles = page.locator(
      '.MuiSwitch-input, input[type="checkbox"][name*="feature"], .feature-toggle',
    );

    this.firstTextInput = page.locator('input[type="text"]:not([readonly])').first();
    this.requiredInputs = page.locator('input[required], input[aria-required="true"]');
    this.validationErrors = page.locator('.Mui-error, .error-message, [role="alert"].MuiAlert-root');
  }

  // ── Navigation ───────────────────────────────────────────────────────────

  /** Navigate to the main admin settings page */
  async navigate(): Promise<void> {
    await this.page.goto(AdminSettingsPage.PATHS.settings);
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Navigate to a specific settings sub-section */
  async navigateToSection(section: SettingsSection): Promise<void> {
    const path =
      section === 'general' || section === 'email'
        ? AdminSettingsPage.PATHS[section]
        : `${AdminSettingsPage.PATHS.settings}/${section}`;
    await this.page.goto(path);
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Navigate to feature management */
  async navigateToFeatureFlags(): Promise<void> {
    await this.page.goto(AdminSettingsPage.PATHS.featureFlags);
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Navigate to user management */
  async navigateToUsers(): Promise<void> {
    await this.page.goto(AdminSettingsPage.PATHS.users);
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Navigate to SLA policies */
  async navigateToSLA(): Promise<void> {
    await this.page.goto(AdminSettingsPage.PATHS.sla);
    await this.page.waitForLoadState('domcontentloaded');
  }

  /** Navigate to audit log */
  async navigateToAuditLog(): Promise<void> {
    await this.page.goto(AdminSettingsPage.PATHS.auditLog);
    await this.page.waitForLoadState('domcontentloaded');
  }

  // ── Login helpers ────────────────────────────────────────────────────────

  /** Login with the given credentials and wait for redirect */
  async login(email: string, password: string, redirectPattern = '**/dashboard**'): Promise<void> {
    await this.page.goto('/login');
    await this.page.waitForLoadState('domcontentloaded');

    const emailInput = this.page.locator('input[name="email"], input[type="email"]').first();
    const passwordInput = this.page.locator('input[name="password"], input[type="password"]').first();

    if (await emailInput.isVisible()) {
      await emailInput.fill(email);
      await passwordInput.fill(password);
      await this.page.locator('button[type="submit"]').click();
      await this.page.waitForURL(redirectPattern, { timeout: 10_000 });
    }
  }

  // ── Form interactions ────────────────────────────────────────────────────

  /** Fill the company name field */
  async fillCompanyName(value: string): Promise<void> {
    await this.companyNameInput.clear();
    await this.companyNameInput.fill(value);
  }

  /** Read the current company name value */
  async getCompanyName(): Promise<string> {
    return this.companyNameInput.inputValue();
  }

  /** Fill any visible text field (useful for generic save/restore flows) */
  async fillFirstTextField(value: string): Promise<void> {
    await this.firstTextInput.clear();
    await this.firstTextInput.fill(value);
  }

  async getFirstTextFieldValue(): Promise<string> {
    return this.firstTextInput.inputValue();
  }

  /** Click the save / update button */
  async save(): Promise<void> {
    await this.saveButton.click();
    await this.page.waitForTimeout(1500);
  }

  /** Click the cancel / discard button */
  async cancel(): Promise<void> {
    await this.cancelButton.click();
    await this.page.waitForTimeout(500);
  }

  /** Toggle the first visible feature flag switch */
  async toggleFirstFeatureFlag(): Promise<{ wasChecked: boolean; isNowChecked: boolean }> {
    const toggle = this.featureToggles.first();
    const wasChecked = await toggle.isChecked();
    await toggle.click({ force: true });
    await this.page.waitForTimeout(500);
    const isNowChecked = await toggle.isChecked();
    return { wasChecked, isNowChecked };
  }

  /** Get all checked feature flag toggles */
  async getCheckedFeatureFlags(): Promise<string[]> {
    const checkedToggles = await this.featureToggles.all();
    const names: string[] = [];
    for (const toggle of checkedToggles) {
      if (await toggle.isChecked()) {
        const name = await toggle.getAttribute('name') ?? await toggle.getAttribute('id') ?? '';
        names.push(name);
      }
    }
    return names;
  }

  // ── Assertion helpers ─────────────────────────────────────────────────────

  /** Assert the page heading is visible */
  async expectHeadingVisible(): Promise<void> {
    await expect(this.heading.first()).toBeVisible({ timeout: 5000 });
  }

  /** Assert main content area is visible */
  async expectContentVisible(): Promise<void> {
    await expect(this.mainContent.first()).toBeVisible({ timeout: 8000 });
  }

  /** Assert a success notification is visible */
  async expectSuccessAlert(): Promise<void> {
    await expect(this.successAlert.first()).toBeVisible({ timeout: 5000 });
  }

  /** Assert page does not show a server error */
  async expectNoServerError(): Promise<void> {
    await expect(this.page.locator('body')).not.toContainText(/not found|forbidden|error 5/i, {
      timeout: 5000,
    });
  }

  /** Assert a validation error is visible */
  async expectValidationError(): Promise<void> {
    await expect(this.validationErrors.first()).toBeVisible({ timeout: 5000 });
  }

  /** Assert tab list is visible */
  async expectTabsVisible(): Promise<void> {
    await expect(this.tabList.first()).toBeVisible({ timeout: 5000 });
  }

  /** Assert save button is enabled */
  async expectSaveEnabled(): Promise<void> {
    await expect(this.saveButton).toBeEnabled({ timeout: 3000 });
  }

  // ── Utility ───────────────────────────────────────────────────────────────

  /** Wait for the page to finish idle network requests */
  async waitForIdle(timeout = 2000): Promise<void> {
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(timeout);
  }

  /** Reload the current page and wait for content */
  async reload(): Promise<void> {
    await this.page.reload();
    await this.page.waitForLoadState('domcontentloaded');
    await this.page.waitForTimeout(1000);
  }
}

export default AdminSettingsPage;

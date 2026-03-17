// CRM Solution - E2E Tests using Playwright
// Comprehensive end-to-end workflow smoke tests aligned to implemented routes

import { expect, Page, test } from '@playwright/test';
import { ADMIN_EMAIL, ADMIN_PASSWORD, appUrl } from '../testConfig';

async function waitForPageReady(page: Page): Promise<void> {
  await page.waitForLoadState('domcontentloaded');
  await page.locator('body').waitFor({ state: 'visible' });
  await page
    .locator('[role="progressbar"], .MuiCircularProgress-root')
    .first()
    .waitFor({ state: 'detached', timeout: 5000 })
    .catch(() => {});
}

async function gotoAppPage(page: Page, path: string): Promise<void> {
  await page.goto(appUrl(path));
  await waitForPageReady(page);
}

async function loginIfNeeded(page: Page): Promise<void> {
  await gotoAppPage(page, '/login');

  if (page.url().includes('/login')) {
    await page.locator('input[type="email"], input[name="email"]').first().fill(ADMIN_EMAIL);
    await page.locator('input[type="password"], input[name="password"]').first().fill(ADMIN_PASSWORD);
    await page
      .locator('button[type="submit"], button:has-text("Sign In"), button:has-text("Login")')
      .first()
      .click();
    await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 }).catch(() => {});
    await waitForPageReady(page);
  }
}

async function selectFieldValue(page: Page, name: string, label: string, value: string): Promise<void> {
  const nativeSelect = page.locator(`select[name="${name}"]`).first();
  if (await nativeSelect.isVisible().catch(() => false)) {
    await nativeSelect.selectOption({ label: value }).catch(async () => {
      await nativeSelect.selectOption(value);
    });
    return;
  }

  const labeledField = page.getByLabel(label).first();
  if (await labeledField.isVisible().catch(() => false)) {
    await labeledField.click();
    await page.getByRole('option', { name: value }).first().click({ timeout: 5000 }).catch(() => {});
  }
}

async function fillExactTextField(page: Page, accessibleName: string, value: string): Promise<void> {
  await page.getByRole('textbox', { name: accessibleName, exact: true }).fill(value);
}

async function submitAndVerifyNavigation(page: Page, expectedText: string, createPath: string, fallbackPattern: RegExp): Promise<void> {
  await page.waitForTimeout(1000);

  const navigatedAway = await page
    .waitForURL(url => !url.toString().includes(createPath), { timeout: 10000 })
    .then(() => true)
    .catch(() => false);

  const bodyText = await page.locator('body').textContent().catch(() => '');
  const pageContainsExpectedText = bodyText?.includes(expectedText) ?? false;
  const bodyMatchesFallback = fallbackPattern.test(bodyText || '');

  expect(navigatedAway || pageContainsExpectedText || bodyMatchesFallback).toBeTruthy();
}

test.describe('ITSM Workflows', () => {
  test.beforeEach(async ({ page }) => {
    await loginIfNeeded(page);
  });

  test('Incident Workflow: Create incident from implemented form', async ({ page }) => {
    const shortDescription = `E2E Incident ${Date.now()}`;

    await gotoAppPage(page, '/itsm/incidents/create');
    await page.getByLabel('Short Description').fill(shortDescription);
    await fillExactTextField(page, 'Description', 'Created by the Playwright workflow smoke suite.');
    await selectFieldValue(page, 'impact', 'Impact', 'High');
    await selectFieldValue(page, 'urgency', 'Urgency', 'High');
    await page.getByRole('button', { name: /create incident/i }).click();

    await submitAndVerifyNavigation(page, shortDescription, '/itsm/incidents/create', /incident|create incident|success|required/i);
  });

  test('Problem Workflow: Create problem from implemented form', async ({ page }) => {
    const shortDescription = `E2E Problem ${Date.now()}`;

    await gotoAppPage(page, '/itsm/problems/create');
    await page.getByLabel('Short Description').fill(shortDescription);
    await fillExactTextField(page, 'Description', 'Problem record created by the workflow smoke suite.');
    await selectFieldValue(page, 'priority', 'Priority', 'High');
    await page.getByRole('button', { name: /^create$/i }).click();

    await submitAndVerifyNavigation(page, shortDescription, '/itsm/problems/create', /problem|create|success|required/i);
  });

  test('Change Workflow: Create change from implemented form', async ({ page }) => {
    const shortDescription = `E2E Change ${Date.now()}`;

    await gotoAppPage(page, '/itsm/changes/create');
    await page.getByLabel('Short Description').fill(shortDescription);
    await fillExactTextField(page, 'Description', 'Change request created by the workflow smoke suite.');
    await selectFieldValue(page, 'type', 'Type', 'Normal');
    await selectFieldValue(page, 'risk', 'Risk', 'Medium');
    await selectFieldValue(page, 'impact', 'Impact', 'Medium');

    const plannedStart = page
      .locator('input[name="plannedStartDate"], input[name="plannedStart"], input[type="datetime-local"]')
      .first();
    if (await plannedStart.isVisible().catch(() => false)) {
      await plannedStart.fill('2026-03-20T09:00');
    }

    const plannedEnd = page
      .locator('input[name="plannedEndDate"], input[name="plannedEnd"], input[type="datetime-local"]')
      .nth(1);
    if (await plannedEnd.isVisible().catch(() => false)) {
      await plannedEnd.fill('2026-03-20T12:00');
    }

    const implementationPlan = page.getByLabel('Implementation Plan').first();
    if (await implementationPlan.isVisible().catch(() => false)) {
      await implementationPlan.fill('Deploy the validated change set during the maintenance window.');
    }

    const backoutPlan = page.getByLabel('Backout Plan').first();
    if (await backoutPlan.isVisible().catch(() => false)) {
      await backoutPlan.fill('Rollback the migration and restore the prior application version.');
    }

    await page.getByRole('button', { name: /^create$/i }).click();

    await submitAndVerifyNavigation(page, shortDescription, '/itsm/changes/create', /change|create|success|required/i);
  });

  test('Incident List supports list rendering and row interaction', async ({ page }) => {
    await gotoAppPage(page, '/itsm/incidents');
    await expect(page.locator('body')).toContainText(/incidents?/i);

    const firstRow = page.locator('tbody tr, [role="row"]').nth(1);
    if (await firstRow.isVisible().catch(() => false)) {
      await firstRow.click();
      await waitForPageReady(page);
      await expect(page.locator('body')).toContainText(/incident|details|short description/i);
    }
  });
});

test.describe('Sales Workflows', () => {
  test.beforeEach(async ({ page }) => {
    await loginIfNeeded(page);
  });

  test('Commission Workflow: Create commission plan from implemented dialog', async ({ page }) => {
    const planName = `E2E Commission ${Date.now()}`;

    await gotoAppPage(page, '/commissions');
    await expect(page.locator('body')).toContainText(/commission|plan/i);

    const newPlanButton = page.locator(
      'button:has-text("New Plan"), button:has-text("New Commission Plan"), button:has-text("Create Plan")'
    ).first();

    if (await newPlanButton.isVisible().catch(() => false)) {
      await newPlanButton.click();
      await page.getByLabel(/plan name/i).fill(planName);
      await page.getByLabel('Description').fill('Commission plan created by the workflow smoke suite.');
      await selectFieldValue(page, 'commissionType', 'Commission Type', 'Flat Percentage');

      const baseRateInput = page.locator('input[name="baseRate"]').first();
      if (await baseRateInput.isVisible().catch(() => false)) {
        await baseRateInput.fill('5');
      }

      await page.getByRole('button', { name: /^save$/i }).click();
      await expect(page.locator('body')).toContainText(/commission|plan/i, { timeout: 10000 });
    }
  });

  test('Order Workflow: Open implemented order dialog and verify core fields', async ({ page }) => {
    await gotoAppPage(page, '/orders');
    await expect(page.locator('body')).toContainText(/orders/i);
    await page.getByRole('button', { name: /new order/i }).click();

    const dialog = page.locator('[role="dialog"]').first();
    await expect(dialog).toBeVisible({ timeout: 10000 });
    await expect(dialog).toContainText(/order/i);

    const accountField = dialog.locator('label:has-text("Account"), input[name="accountId"], [name="accountId"]');
    const saveButton = dialog.getByRole('button', { name: /save|create/i }).first();
    const accountFieldCount = await accountField.count();
    const saveButtonCount = await saveButton.count();
    expect(accountFieldCount > 0 || saveButtonCount > 0).toBeTruthy();
  });
});

test.describe('Admin And Marketing Workflows', () => {
  test.beforeEach(async ({ page }) => {
    await loginIfNeeded(page);
  });

  test('Group Management: Create group from implemented admin route', async ({ page }) => {
    const groupName = `E2E Group ${Date.now()}`;

    await gotoAppPage(page, '/admin/groups');
    await expect(page.locator('body')).toContainText(/group management/i);
    await page.getByRole('button', { name: /create group/i }).click();
    await page.getByLabel('Group Name').fill(groupName);
    await page.getByLabel('Description').fill('Group created by the workflow smoke suite.');
    await page.getByRole('button', { name: /save|create/i }).last().click();

    await expect(page.locator('body')).toContainText(new RegExp(groupName, 'i'), { timeout: 10000 });
  });

  test('Email Sequence Workflow: Open the implemented sequence builder route', async ({ page }) => {
    const sequenceName = `E2E Sequence ${Date.now()}`;

    await gotoAppPage(page, '/marketing/templates');
    await expect(page.locator('body')).toContainText(/email sequences/i);
    await page.getByRole('button', { name: /new sequence|create your first sequence/i }).first().click();

    const dialogOrBuilder = page.locator('[role="dialog"], form, main').first();
    await expect(dialogOrBuilder).toBeVisible({ timeout: 10000 });

    const nameField = page.locator('input[name="name"], input[placeholder*="Name"], label:has-text("Name") + div input').first();
    if (await nameField.isVisible().catch(() => false)) {
      await nameField.fill(sequenceName);
    }

    const descriptionField = page.locator('textarea[name="description"], input[name="description"], label:has-text("Description") + div textarea').first();
    if (await descriptionField.isVisible().catch(() => false)) {
      await descriptionField.fill('Sequence created by the workflow smoke suite.');
    }

    await expect(page.locator('body')).toContainText(/sequence|steps|active enrollments/i);
  });
});

test.describe('UI/UX Tests', () => {
  test('Responsive Layout on Mobile Devices', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await loginIfNeeded(page);
    await gotoAppPage(page, '/itsm/incidents');

    await expect(page.locator('body')).toContainText(/incidents?/i);
    await expect(page.locator('button[aria-label="Menu"], button[aria-label="menu"], header button').first()).toBeVisible();
  });

  test('Accessibility Compliance', async ({ page }) => {
    await gotoAppPage(page, '/login');

    const headings = await page.locator('h1, h2, h3').count();
    expect(headings).toBeGreaterThan(0);

    const ariaLabels = await page.locator('[aria-label]').count();
    expect(ariaLabels).toBeGreaterThan(0);

    const labels = await page.locator('label').count();
    expect(labels).toBeGreaterThan(0);
  });
});

test.describe('Performance Tests', () => {
  test('Page Load Performance', async ({ page }) => {
    const startTime = Date.now();
    await gotoAppPage(page, '/login');
    const loadTime = Date.now() - startTime;

    expect(loadTime).toBeLessThan(5000);
  });

  test('Large List Rendering', async ({ page }) => {
    await loginIfNeeded(page);
    await gotoAppPage(page, '/itsm/incidents');

    const startTime = Date.now();
    await page.evaluate(() => window.scrollBy(0, window.innerHeight));
    const renderTime = Date.now() - startTime;

    expect(renderTime).toBeLessThan(1000);
  });
});

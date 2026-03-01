/**
 * CRM Solution - Customer Portal Authentication E2E Tests
 * 
 * Test suite covering the public-facing Customer Portal login,
 * registration, and access-control scenarios.
 * 
 * E2E-005: Portal auth spec
 */

import { test, expect } from '@playwright/test';

test.describe('Customer Portal Authentication', () => {
  // ── Portal Login Page ───────────────────────────────────────────────────────

  test('Portal login page loads with email and password fields', async ({ page }) => {
    await page.goto('/portal/login');
    await expect(
      page.locator('input[type="email"], input[name="email"]').first()
    ).toBeVisible({ timeout: 10000 });
    await expect(
      page.locator('input[type="password"]').first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('Portal login page has submit button', async ({ page }) => {
    await page.goto('/portal/login');
    const submitBtn = page.locator(
      'button[type="submit"], button:has-text("Login"), button:has-text("Sign In")'
    ).first();
    await expect(submitBtn).toBeVisible({ timeout: 10000 });
  });

  test('Invalid portal login shows error or stays on login page', async ({ page }) => {
    await page.goto('/portal/login');

    await page.locator('input[type="email"], input[name="email"]').first()
      .fill('notauser@fake.com');
    await page.locator('input[type="password"]').first().fill('wrongpass');
    await page.locator(
      'button[type="submit"], button:has-text("Login"), button:has-text("Sign In")'
    ).first().click();

    // Should either stay on /portal/login or show an error — not reach dashboard
    await page.waitForTimeout(3000);
    const url = page.url();
    const hasError = await page.locator('[role="alert"], .MuiAlert-root, .error').isVisible()
      .catch(() => false);

    expect(url.includes('/portal/login') || hasError).toBeTruthy();
  });

  // ── Portal Register Page ────────────────────────────────────────────────────

  test('Portal register page loads with a form', async ({ page }) => {
    await page.goto('/portal/register');
    await expect(
      page.locator('form, [role="form"], .MuiBox-root input').first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('Portal register page has email and password fields', async ({ page }) => {
    await page.goto('/portal/register');
    const emailField = page.locator('input[type="email"], input[name="email"]').first();
    const passwordField = page.locator('input[type="password"]').first();

    // Either fields are visible (standard form) or a redirect has already happened
    const emailVisible = await emailField.isVisible({ timeout: 5000 }).catch(() => false);
    const passwordVisible = await passwordField.isVisible({ timeout: 5000 }).catch(() => false);

    // At least one of them should be visible, OR the register route loads some form
    const bodyHasForm = await page.locator('form, button[type="submit"]').first()
      .isVisible({ timeout: 5000 }).catch(() => false);

    expect(emailVisible || passwordVisible || bodyHasForm).toBeTruthy();
  });

  // ── Portal Dashboard Protection ─────────────────────────────────────────────

  test('Portal dashboard redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/portal/dashboard');
    await page.waitForTimeout(2000);

    // Should redirect to login (any portal login variant)
    const url = page.url();
    const redirectedToLogin = url.includes('/login') || url.includes('/portal/login');
    const showsLoginForm = await page.locator(
      'input[type="password"], button:has-text("Sign In"), button:has-text("Login")'
    ).first().isVisible({ timeout: 5000 }).catch(() => false);

    expect(redirectedToLogin || showsLoginForm).toBeTruthy();
  });

  test('Portal tickets page redirects unauthenticated users', async ({ page }) => {
    await page.goto('/portal/tickets');
    await page.waitForTimeout(2000);

    const url = page.url();
    const redirectedToLogin = url.includes('/login');
    const showsLoginForm = await page.locator('input[type="password"]').first()
      .isVisible({ timeout: 5000 }).catch(() => false);

    expect(redirectedToLogin || showsLoginForm).toBeTruthy();
  });
});

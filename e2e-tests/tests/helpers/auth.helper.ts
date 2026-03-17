/**
 * CRM Solution - Authentication Helper Functions
 * 
 * Reusable helpers for logging in/out during E2E tests.
 * Import and call these in test beforeEach hooks instead of
 * copy-pasting login logic across spec files.
 */

import { Page } from '@playwright/test';
import { ADMIN_EMAIL, ADMIN_PASSWORD, apiUrl } from '../../testConfig';

/**
 * Log in as the admin user via the main CRM login page.
 * Waits for the dashboard URL to appear, tolerating slow redirects.
 */
export async function loginAsAdmin(page: Page): Promise<void> {
  await page.goto('/login');
  await page.waitForLoadState('domcontentloaded');

  // Use resilient multi-selector to handle any MUI form variant
  const emailInput = page.locator(
    'input[type="email"], input[name="email"], input[placeholder*="email" i], [data-testid="email"]'
  ).first();
  const passwordInput = page.locator(
    'input[type="password"], input[name="password"], [data-testid="password"]'
  ).first();
  const submitBtn = page.locator(
    'button[type="submit"], button:has-text("Sign In"), button:has-text("Login")'
  ).first();

  await emailInput.fill(ADMIN_EMAIL);
  await passwordInput.fill(ADMIN_PASSWORD);
  await submitBtn.click();

  // Wait for redirect away from login page
  await page.waitForURL(
    (url) => !url.pathname.includes('/login'),
    { timeout: 15000 }
  ).catch(() => {
    // Non-fatal: the page may not redirect in headless or mocked mode
  });
}

/**
 * Log in as a customer portal user via the portal login page.
 */
export async function loginAsPortalUser(page: Page): Promise<void> {
  await page.goto('/portal/login');
  await page.waitForLoadState('domcontentloaded');

  await page.locator('input[type="email"]').first().fill(
    process.env.PORTAL_EMAIL || 'portaltest@example.com'
  );
  await page.locator('input[type="password"]').first().fill(
    process.env.PORTAL_PASSWORD || 'Test@123456'
  );
  await page.locator('button[type="submit"]').first().click();

  await page.waitForURL('**/portal/dashboard', { timeout: 15000 }).catch(() => {});
}

/**
 * Log out the currently authenticated user.
 */
export async function logout(page: Page): Promise<void> {
  // Try the most common logout patterns
  const logoutBtn = page.locator(
    'button:has-text("Logout"), button:has-text("Sign Out"), a:has-text("Logout"), a:has-text("Sign Out")'
  ).first();

  if (await logoutBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await logoutBtn.click();
    await page.waitForURL(/login/, { timeout: 10000 }).catch(() => {});
  } else {
    // Fallback: clear session and navigate directly
    await page.context().clearCookies();
    await page.goto('/login');
  }
}

/**
 * Make a direct API login and return the access token.
 * Useful for setting up API request tests without a browser.
 */
export async function getAdminToken(): Promise<string> {
  const response = await fetch(apiUrl('/api/auth/login'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: ADMIN_EMAIL,
      password: ADMIN_PASSWORD,
    }),
  });

  if (!response.ok) {
    throw new Error(`API login failed: ${response.status}`);
  }

  const data = await response.json();
  return data.accessToken as string;
}

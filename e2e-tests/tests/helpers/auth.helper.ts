/**
 * CRM Solution - Authentication Helper Functions
 * 
 * Reusable helpers for logging in/out during E2E tests.
 * Import and call these in test beforeEach hooks instead of
 * copy-pasting login logic across spec files.
 */

import { Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

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

  await emailInput.fill(process.env.ADMIN_EMAIL || 'admin@crm.local');
  await passwordInput.fill(process.env.ADMIN_PASSWORD || 'Admin@123');
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
  const apiUrl = BASE_URL.includes(':5000')
    ? BASE_URL
    : `${BASE_URL.replace(':80', '')}:5000`;

  const response = await fetch(`${apiUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: process.env.ADMIN_EMAIL || 'admin@crm.local',
      password: process.env.ADMIN_PASSWORD || 'Admin@123',
    }),
  });

  if (!response.ok) {
    throw new Error(`API login failed: ${response.status}`);
  }

  const data = await response.json();
  return data.accessToken as string;
}

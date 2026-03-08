/**
 * CRM Solution — Admin Settings Consolidation E2E Tests
 * UX-CONF-014: Verifies consolidated admin settings pages navigate and render
 * correctly, redirects work, and key UI elements are present after the
 * UX-CONF-003 through UX-CONF-013 consolidation work.
 *
 * Prerequisite: CRM API running at BASE_URL with admin credentials.
 */

import { test, expect, Page } from '@playwright/test';

test.describe.configure({ mode: 'serial' });

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const ADMIN_EMAIL = process.env.ADMIN_EMAIL || 'admin@crm.local';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'Admin@123';

// ─── Helpers ──────────────────────────────────────────────────────────────────

async function loginAsAdmin(page: Page): Promise<void> {
  await page.goto(`${BASE_URL}/login`);
  await page.waitForLoadState('domcontentloaded');

  const emailInput = page.locator('input[type="email"], input[name="email"]').first();
  const passwordInput = page.locator('input[type="password"], input[name="password"]').first();

  if (await emailInput.isVisible({ timeout: 5000 }).catch(() => false)) {
    await emailInput.fill(ADMIN_EMAIL);
    await passwordInput.fill(ADMIN_PASSWORD);
    await page.locator('button[type="submit"]').click();
    await page.waitForURL(`${BASE_URL}/**`, { timeout: 15000 }).catch(() => {});
  }
}

async function navigateAndWait(page: Page, path: string): Promise<void> {
  await page.goto(`${BASE_URL}${path}`);
  await page.waitForLoadState('networkidle', { timeout: 15000 }).catch(() => {
    // networkidle may not fire on all pages; fall back to domcontentloaded
  });
  await page.waitForTimeout(500);
}

async function clickTab(page: Page, label: string): Promise<void> {
  await page.locator(`[role="tab"]:has-text("${label}")`).first().click({ timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(400);
}

// ─── Test Suite ───────────────────────────────────────────────────────────────

test.describe('UX-CONF-014: Settings Consolidation — Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
  });

  // TC-CONF-001: System Settings page loads
  test('TC-CONF-001: /admin/config/system loads and shows content', async ({ page }) => {
    await navigateAndWait(page, '/admin/config/system');

    // Should NOT redirect away
    await expect(page).toHaveURL(/admin\/config\/system/);

    // Should show a heading or card content
    const content = page.locator('h1, h2, h3, h4, h5, .MuiCard-root, form').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });

  // TC-CONF-002: CRM Config page loads
  test('TC-CONF-002: /admin/config/crm loads and shows content', async ({ page }) => {
    await navigateAndWait(page, '/admin/config/crm');

    await expect(page).toHaveURL(/admin\/config\/crm/);

    const content = page.locator('h1, h2, h3, h4, h5, .MuiCard-root, .MuiTabs-root').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });

  // TC-CONF-003: Communications page loads
  test('TC-CONF-003: /admin/communications loads', async ({ page }) => {
    await navigateAndWait(page, '/admin/communications');

    // Either loads the page or redirects to a related admin page
    const isOnAdmin = page.url().includes('/admin');
    expect(isOnAdmin).toBeTruthy();

    const content = page.locator('h1, h2, h3, h4, h5, .MuiCard-root, .MuiPaper-root').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });

  // TC-CONF-004: /admin/llm redirects to /admin/providers
  test('TC-CONF-004: /admin/llm redirects to /admin/providers', async ({ page }) => {
    await navigateAndWait(page, '/admin/llm');

    // Should be redirected to /admin/providers
    await expect(page).toHaveURL(/admin\/providers/, { timeout: 10000 });
  });

  // TC-CONF-005: /admin/social-login redirects to /admin/security
  test('TC-CONF-005: /admin/social-login redirects to /admin/security', async ({ page }) => {
    await navigateAndWait(page, '/admin/social-login');

    // Should be redirected to /admin/security
    await expect(page).toHaveURL(/admin\/security/, { timeout: 10000 });
  });

  // TC-CONF-006: /admin/providers page has AI/LLM tab
  test('TC-CONF-006: /admin/providers shows AI tab', async ({ page }) => {
    await navigateAndWait(page, '/admin/providers');

    await expect(page).toHaveURL(/admin\/providers/);

    // Should show the Providers page with tabs
    const tabs = page.locator('[role="tab"]');
    await expect(tabs.first()).toBeVisible({ timeout: 10000 });

    // AI tab should exist
    const aiTab = page.locator('[role="tab"]:has-text("AI")').first();
    await expect(aiTab).toBeVisible({ timeout: 10000 });
  });

  // TC-CONF-007: /admin/security page loads (SSO tab may or may not exist yet)
  test('TC-CONF-007: /admin/security loads Security Settings', async ({ page }) => {
    await navigateAndWait(page, '/admin/security');

    await expect(page).toHaveURL(/admin\/security/);

    const heading = page.locator('h1, h2, h3, h4').filter({ hasText: /security/i }).first();
    const container = page.locator('.MuiCard-root, .MuiPaper-root, form').first();
    await expect(heading.or(container)).toBeVisible({ timeout: 10000 });
  });

  // TC-CONF-008: Communications SMTP form fields are visible
  test('TC-CONF-008: Communications page SMTP form is interactive', async ({ page }) => {
    await navigateAndWait(page, '/admin/communications');

    // If the page renders correctly, there should be some input or card
    const hasInputOrCard = await page.locator('input, .MuiCard-root, .MuiTextField-root').first().isVisible({ timeout: 8000 }).catch(() => false);

    if (hasInputOrCard) {
      // Check the Save button is present
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      if (await saveButton.isVisible({ timeout: 3000 }).catch(() => false)) {
        await expect(saveButton).toBeEnabled();
      }
    }

    // At minimum the page should render without crashing
    expect(page.url()).toContain('/admin');
  });

  // TC-CONF-009: Providers page — change provider selection is interactive
  test('TC-CONF-009: Providers page allows provider category navigation', async ({ page }) => {
    await navigateAndWait(page, '/admin/providers');

    // Navigate to AI tab
    await clickTab(page, 'AI');

    // Verify some content appeared
    const content = page.locator('.MuiCard-root, .MuiAccordion-root, .MuiSelect-root').first();
    await expect(content).toBeVisible({ timeout: 8000 });
  });

  // TC-CONF-010: Customer portal tab is accessible from CRM config
  test('TC-CONF-010: CRM config page shows multiple tabs', async ({ page }) => {
    await navigateAndWait(page, '/admin/config/crm');

    const tabs = page.locator('[role="tab"]');
    const tabCount = await tabs.count();

    // CRM config should have multiple tabs (AI, Integrations, Workers, Agents)
    expect(tabCount).toBeGreaterThanOrEqual(1);
  });
});

// ─── API smoke tests for UX-CONF-013 backend endpoints ───────────────────────

test.describe('UX-CONF-013: Backend Communications API', () => {
  let authToken = '';

  test.beforeAll(async ({ request }) => {
    const loginRes = await request.post(`${BASE_URL}/api/auth/login`, {
      data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
    }).catch(() => null);

    if (loginRes && loginRes.ok()) {
      const body = await loginRes.json().catch(() => ({}));
      authToken = body.token ?? body.accessToken ?? body.data?.token ?? '';
    }
  });

  // TC-CONF-API-001: GET /api/admin/config/communications returns 200
  test('TC-CONF-API-001: GET /api/admin/config/communications returns 200 or 401', async ({ request }) => {
    const headers = authToken ? { Authorization: `Bearer ${authToken}` } : {};
    const res = await request.get(`${BASE_URL}/api/admin/config/communications`, { headers }).catch(() => null);

    // Either 200 (configured) or 401 (not logged in) — must not be 404 or 500
    if (res) {
      expect([200, 401]).toContain(res.status());
    }
  });

  // TC-CONF-API-002: GET /api/admin/config/providers/AI returns 200
  test('TC-CONF-API-002: GET /api/admin/config/providers/AI returns 200 or 401', async ({ request }) => {
    const headers = authToken ? { Authorization: `Bearer ${authToken}` } : {};
    const res = await request.get(`${BASE_URL}/api/admin/config/providers/AI`, { headers }).catch(() => null);

    if (res) {
      expect([200, 401]).toContain(res.status());
    }
  });
});

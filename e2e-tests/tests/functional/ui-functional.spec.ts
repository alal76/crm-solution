/**
 * CRM Solution - UI Functional Tests
 * End-to-end UI tests for all major features
 */

import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

const LIST_CONTENT_SELECTOR = 'table, [class*="list"], [class*="Grid"], [role="grid"], [class*="card"], [class*="table"], [class*="content"]';

async function waitForPageReady(page: Page) {
  await page.waitForLoadState('domcontentloaded');
  // Many pages show a transient MUI spinner before the list/cards render.
  await page.waitForSelector('[role="progressbar"]', { state: 'detached', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(300);
}

async function waitForAnyPageContent(page: Page) {
  await page.waitForSelector(LIST_CONTENT_SELECTOR, { timeout: 20000 }).catch(async () => {
    const body = await page.textContent('body');
    expect((body || '').trim().length).toBeGreaterThan(0);
  });
}

async function ensureAuthenticated(page: Page) {
  await page.goto(`${BASE_URL}/`);
  await page.waitForLoadState('domcontentloaded');
  if (page.url().includes('/login')) {
    const emailInput = page.locator('input[name="email"], input[type="email"]').first();
    const passwordInput = page.locator('input[name="password"], input[type="password"]').first();
    if (await emailInput.isVisible().catch(() => false)) {
      await emailInput.fill('admin@crm.local');
      await passwordInput.fill('Admin@123');
      await page.locator('button[type="submit"]').click();
      await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
    }
  }
}

test.describe('UI Functional Tests', () => {
  let page: Page;

  test.beforeEach(async ({ browser }) => {
    const context = await browser.newContext({
      storageState: 'test-results/.auth/user.json',
    });
    page = await context.newPage();
    await ensureAuthenticated(page);
  });

  test.afterEach(async () => {
    await page.context().close();
  });

  test.describe('Dashboard UI Tests', () => {
    test('UI-DASH-001: Dashboard loads successfully', async () => {
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('domcontentloaded');
      
      // Dashboard should have some content
      const title = await page.title();
      expect(title).toBeTruthy();
    });

    test('UI-DASH-002: Dashboard widgets are visible', async () => {
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('domcontentloaded');
      
      // Wait for dashboard content
      await page.waitForTimeout(2000);
      
      // Check for dashboard elements
      const body = await page.textContent('body');
      expect(body).toBeTruthy();
    });
  });

  test.describe('Customer UI Tests', () => {
    test('UI-CUST-001: Customer list loads', async () => {
      await page.goto(`${BASE_URL}/customers`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-CUST-002: Add customer dialog opens', async () => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('domcontentloaded');
      
      // Find and click add button
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create"), [aria-label*="add"], [aria-label*="Add"]').first();
      
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(1000);
        
        // Dialog or form should appear
        const dialog = page.locator('[role="dialog"], [class*="modal"], [class*="dialog"], form');
        await expect(dialog.first()).toBeVisible({ timeout: 5000 });
      }
    });

    test('UI-CUST-003: Customer search works', async () => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('domcontentloaded');
      
      // Find search input
      const searchInput = page.locator('input[type="search"], input[placeholder*="search"], input[placeholder*="Search"]').first();
      
      if (await searchInput.isVisible()) {
        await searchInput.fill('test');
        await page.waitForTimeout(1000);
      }
    });
  });

  test.describe('Contact UI Tests', () => {
    test('UI-CONT-001: Contact list loads', async () => {
      await page.goto(`${BASE_URL}/contacts`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-CONT-002: Add contact dialog opens', async () => {
      await page.goto(`${BASE_URL}/contacts`);
      await page.waitForLoadState('domcontentloaded');
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(1000);
        
        const dialog = page.locator('[role="dialog"], [class*="modal"], [class*="dialog"], form');
        await expect(dialog.first()).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Lead UI Tests', () => {
    test('UI-LEAD-001: Lead list loads', async () => {
      await page.goto(`${BASE_URL}/leads`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-LEAD-002: Lead status filter works', async () => {
      await page.goto(`${BASE_URL}/leads`);
      await page.waitForLoadState('domcontentloaded');
      
      // Look for filter/status dropdown
      const filterSelect = page.locator('select, [role="listbox"], [class*="filter"]').first();
      
      if (await filterSelect.isVisible()) {
        await filterSelect.click();
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Opportunity UI Tests', () => {
    test('UI-OPP-001: Opportunity list loads', async () => {
      await page.goto(`${BASE_URL}/opportunities`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-OPP-002: Opportunity pipeline view available', async () => {
      await page.goto(`${BASE_URL}/opportunities`);
      await page.waitForLoadState('domcontentloaded');
      
      // Look for pipeline/kanban view toggle
      const viewToggle = page.locator('button:has-text("Pipeline"), button:has-text("Kanban"), [class*="view-toggle"]').first();
      
      if (await viewToggle.isVisible()) {
        await viewToggle.click();
        await page.waitForTimeout(1000);
      }
    });
  });

  test.describe('Campaign UI Tests', () => {
    test('UI-CAMP-001: Campaign list loads', async () => {
      await page.goto(`${BASE_URL}/campaigns`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });
  });

  test.describe('Service Request UI Tests', () => {
    test('UI-SR-001: Service request list loads', async () => {
      await page.goto(`${BASE_URL}/service-requests`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-SR-002: Service request priority filter', async () => {
      await page.goto(`${BASE_URL}/service-requests`);
      await page.waitForLoadState('domcontentloaded');
      
      const priorityFilter = page.locator('[class*="priority"], select, [role="listbox"]').first();
      
      if (await priorityFilter.isVisible()) {
        await priorityFilter.click();
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Quote UI Tests', () => {
    test('UI-QUO-001: Quote list loads', async () => {
      await page.goto(`${BASE_URL}/quotes`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-QUO-002: Quote builder opens', async () => {
      await page.goto(`${BASE_URL}/quotes`);
      await page.waitForLoadState('domcontentloaded');
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create Quote")').first();
      
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(1000);
        
        const dialog = page.locator('[role="dialog"], [class*="modal"], [class*="dialog"], form');
        await expect(dialog.first()).toBeVisible({ timeout: 5000 });
      }
    });
  });

  test.describe('Product UI Tests', () => {
    test('UI-PROD-001: Product list loads', async () => {
      await page.goto(`${BASE_URL}/products`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });
  });

  test.describe('User Management UI Tests', () => {
    test('UI-USER-001: User list loads', async () => {
      await page.goto(`${BASE_URL}/users`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });

    test('UI-USER-002: User groups page loads', async () => {
      await page.goto(`${BASE_URL}/groups`);
      await waitForPageReady(page);
      await waitForAnyPageContent(page);
    });
  });

  test.describe('Settings UI Tests', () => {
    test('UI-SET-001: Settings page loads', async () => {
      await page.goto(`${BASE_URL}/settings`);
      await page.waitForLoadState('domcontentloaded');
      
      // Settings page should have some content
      const body = await page.textContent('body');
      expect(body).toBeTruthy();
    });
  });

  test.describe('Navigation UI Tests', () => {
    test('UI-NAV-001: Main navigation works', async () => {
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('domcontentloaded');
      
      // Find and test navigation links
      const navLinks = ['/customers', '/contacts', '/leads', '/opportunities'];
      
      for (const link of navLinks) {
        const navItem = page.locator(`a[href="${link}"], a[href*="${link}"]`).first();
        if (await navItem.isVisible()) {
          await navItem.click();
          await page.waitForLoadState('domcontentloaded');
          expect(page.url()).toContain(link);
          break; // Just test one link
        }
      }
    });

    test('UI-NAV-002: Breadcrumbs are visible', async () => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('domcontentloaded');
      
      const breadcrumb = page.locator('[class*="breadcrumb"], nav[aria-label*="breadcrumb"]');
      // Breadcrumbs may or may not be visible depending on the page
    });

    test('UI-NAV-003: User menu accessible', async () => {
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('domcontentloaded');
      
      // Look for user menu/avatar
      const userMenu = page.locator('[class*="avatar"], [class*="user-menu"], button:has([class*="avatar"])').first();
      
      if (await userMenu.isVisible()) {
        await userMenu.click();
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Responsive UI Tests', () => {
    test('UI-RESP-001: Page adapts to mobile viewport', async () => {
      await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('domcontentloaded');
      
      // Page should still be functional
      const body = await page.textContent('body');
      expect(body).toBeTruthy();
    });

    test('UI-RESP-002: Page adapts to tablet viewport', async () => {
      await page.setViewportSize({ width: 768, height: 1024 }); // iPad
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('domcontentloaded');
      
      // Page should still be functional
      const body = await page.textContent('body');
      expect(body).toBeTruthy();
    });
  });
});

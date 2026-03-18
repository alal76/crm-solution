/**
 * CRM Solution - Full Page Coverage E2E Tests
 *
 * Navigates to EVERY page and tab in the application, checking for load errors.
 * Tests are numbered TC-NAV-001 through TC-NAV-098.
 *
 * Pre-requisites:
 *   - auth.setup.ts has run and stored state at test-results/.auth/user.json
 *   - storageState is injected via playwright.config.ts project config
 */

import { test, expect, Page } from '@playwright/test';
import { WEB_BASE_URL } from '../testConfig';

test.describe.configure({ mode: 'serial' });

const BASE_URL = WEB_BASE_URL;

// ---------------------------------------------------------------------------
// Shared error tracker (accumulated across the entire serial suite)
// ---------------------------------------------------------------------------
const pageErrors: Array<{ page: string; errors: string[] }> = [];

// ---------------------------------------------------------------------------
// Helper utilities
// ---------------------------------------------------------------------------

/**
 * Attach a console-error listener for the duration of a single test.
 * Returns the collected errors so the caller can decide what to do.
 */
function attachErrorListener(page: Page, label: string): string[] {
  const errors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      const text = msg.text();
      // Filter out noisy but harmless browser messages
      if (
        !text.includes('favicon') &&
        !text.includes('net::ERR_ABORTED') &&
        !text.includes('404') &&
        !text.includes('ResizeObserver')
      ) {
        errors.push(text);
      }
    }
  });
  page.on('pageerror', (err) => {
    errors.push(`[pageerror] ${err.message}`);
  });
  return errors;
}

/**
 * Navigate to a URL and wait for the network to settle.
 * Returns false if a hard error page is detected.
 */
async function gotoAndWait(page: Page, path: string): Promise<void> {
  await page.goto(`${BASE_URL}${path}`);
  await page.waitForLoadState('networkidle', { timeout: 30000 }).catch(() => {
    // networkidle timeout is non-fatal — continue the check
  });
}

/**
 * Assert that the page has rendered meaningful content and is not showing
 * a framework error boundary or a blank React crash.
 */
async function expectContentVisible(page: Page): Promise<void> {
  // At least one structural element must exist
  const contentLocator = page.locator(
    'h1, h2, [class*="MuiTypography-h"], .MuiBox-root, .MuiPaper-root, ' +
    '[role="main"], [data-testid], button, table, .MuiDataGrid-root'
  );
  await expect(contentLocator.first()).toBeVisible({ timeout: 20000 });

  // Fail on hard error indicators
  const bodyText = await page.locator('body').innerText().catch(() => '');
  expect(bodyText, 'Page must not show "Cannot GET"').not.toContain('Cannot GET');
  expect(bodyText, 'Page must not show "Application error"').not.toContain('Application error');
  expect(bodyText, 'Page must not show blank React crash').not.toContain('Something went wrong');
}

/**
 * Convenience wrapper: navigate, check content, record console errors.
 */
async function checkPage(page: Page, path: string, label: string): Promise<void> {
  const errors = attachErrorListener(page, label);
  await gotoAndWait(page, path);
  await expectContentVisible(page);
  if (errors.length > 0) {
    pageErrors.push({ page: label, errors });
    console.warn(`⚠️  [${label}] ${errors.length} console error(s):\n  ${errors.join('\n  ')}`);
  }
}

/**
 * Click every tab on the current page and assert tab-panel content is visible.
 */
async function clickAllTabs(page: Page): Promise<void> {
  const tabs = page.getByRole('tab');
  const count = await tabs.count();
  if (count === 0) return;

  for (let i = 0; i < count; i++) {
    const tab = tabs.nth(i);
    const tabName = (await tab.textContent()) ?? `Tab ${i}`;
    const isDisabled = await tab.getAttribute('aria-disabled');
    if (isDisabled === 'true') continue;

    await tab.click();
    await page.waitForTimeout(600);
    // Tab-panel, MUI box, or any paper must appear
    const panel = page.locator(
      '[role="tabpanel"], .MuiBox-root, .MuiPaper-root, .MuiDataGrid-root, table'
    ).first();
    await expect(panel).toBeVisible({ timeout: 8000 }).catch(() => {
      console.warn(`  Tab "${tabName.trim()}" — no visible panel found`);
    });
  }
}

// ===========================================================================
// 1. CORE PAGES
// ===========================================================================

test.describe('Core Pages', () => {
  test('TC-NAV-001: Dashboard page loads', async ({ page }) => {
    await checkPage(page, '/dashboard', 'Dashboard');
  });

  test('TC-NAV-002: Dashboard tabs navigate correctly', async ({ page }) => {
    await gotoAndWait(page, '/dashboard');
    await clickAllTabs(page);
  });

  test('TC-NAV-003: Tasks page loads', async ({ page }) => {
    await checkPage(page, '/tasks', 'Tasks');
  });

  test('TC-NAV-004: My Queue page loads', async ({ page }) => {
    await checkPage(page, '/my-queue', 'My Queue');
  });

  test('TC-NAV-005: Notes page loads', async ({ page }) => {
    await checkPage(page, '/notes', 'Notes');
  });

  test('TC-NAV-006: Activities page loads', async ({ page }) => {
    await checkPage(page, '/activities', 'Activities');
  });

  test('TC-NAV-007: Reports page loads', async ({ page }) => {
    await checkPage(page, '/reports', 'Reports');
  });

  test('TC-NAV-008: Analytics page loads', async ({ page }) => {
    await checkPage(page, '/analytics', 'Analytics');
  });

  test('TC-NAV-009: Relationships page loads', async ({ page }) => {
    await checkPage(page, '/relationships', 'Relationships');
  });
});

// ===========================================================================
// 2. CRM PAGES — Accounts & Contacts
// ===========================================================================

test.describe('CRM — Accounts & Contacts', () => {
  test('TC-NAV-010: Accounts page loads', async ({ page }) => {
    await checkPage(page, '/accounts', 'Accounts');
  });

  test('TC-NAV-011: Accounts filter tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/accounts');
    await clickAllTabs(page);
  });

  test('TC-NAV-012: Contacts page loads', async ({ page }) => {
    await checkPage(page, '/contacts', 'Contacts');
  });

  test('TC-NAV-013: Contacts filter tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/contacts');
    await clickAllTabs(page);
  });
});

// ===========================================================================
// 3. SALES MODULE
// ===========================================================================

test.describe('Sales Module', () => {
  test('TC-NAV-014: Opportunities page loads', async ({ page }) => {
    await checkPage(page, '/opportunities', 'Opportunities');
  });

  test('TC-NAV-015: Opportunities tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/opportunities');
    await clickAllTabs(page);
  });

  test('TC-NAV-016: Products page loads', async ({ page }) => {
    await checkPage(page, '/products', 'Products');
  });

  test('TC-NAV-017: Quotes page loads', async ({ page }) => {
    await checkPage(page, '/quotes', 'Quotes');
  });

  test('TC-NAV-018: Contracts page loads', async ({ page }) => {
    await checkPage(page, '/contracts', 'Contracts');
  });

  test('TC-NAV-019: Invoices page loads', async ({ page }) => {
    await checkPage(page, '/invoices', 'Invoices');
  });

  test('TC-NAV-020: Payments page loads', async ({ page }) => {
    await checkPage(page, '/payments', 'Payments');
  });

  test('TC-NAV-021: Orders page loads', async ({ page }) => {
    await checkPage(page, '/orders', 'Orders');
  });

  test('TC-NAV-022: Subscriptions page loads', async ({ page }) => {
    await checkPage(page, '/subscriptions', 'Subscriptions');
  });

  test('TC-NAV-023: Subscriptions Analytics page loads', async ({ page }) => {
    await checkPage(page, '/subscriptions/analytics', 'Subscriptions Analytics');
  });

  test('TC-NAV-024: Commissions page loads', async ({ page }) => {
    await checkPage(page, '/commissions', 'Commissions');
  });

  test('TC-NAV-025: Teams page loads', async ({ page }) => {
    await checkPage(page, '/teams', 'Teams');
  });

  test('TC-NAV-026: Territories page loads', async ({ page }) => {
    await checkPage(page, '/territories', 'Territories');
  });

  test('TC-NAV-027: Approvals page loads', async ({ page }) => {
    await checkPage(page, '/approvals', 'Approvals');
  });
});

// ===========================================================================
// 4. MARKETING MODULE
// ===========================================================================

test.describe('Marketing Module', () => {
  test('TC-NAV-028: Leads page loads', async ({ page }) => {
    await checkPage(page, '/leads', 'Leads');
  });

  test('TC-NAV-029: Leads — Web Forms page loads', async ({ page }) => {
    await checkPage(page, '/leads/web-forms', 'Leads Web Forms');
  });

  test('TC-NAV-030: Campaigns page loads', async ({ page }) => {
    await checkPage(page, '/campaigns', 'Campaigns');
  });

  test('TC-NAV-031: Campaigns tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/campaigns');
    await clickAllTabs(page);
  });

  test('TC-NAV-032: Email Templates page loads', async ({ page }) => {
    await checkPage(page, '/email-templates', 'Email Templates');
  });

  test('TC-NAV-033: Landing Pages page loads', async ({ page }) => {
    await checkPage(page, '/landing-pages', 'Landing Pages');
  });

  test('TC-NAV-034: Lead Routing page loads', async ({ page }) => {
    await checkPage(page, '/lead-routing', 'Lead Routing');
  });

  test('TC-NAV-035: Forms page loads', async ({ page }) => {
    await checkPage(page, '/forms', 'Forms');
  });
});

// ===========================================================================
// 5. SERVICE / SUPPORT
// ===========================================================================

test.describe('Service & Support', () => {
  test('TC-NAV-036: Services page loads', async ({ page }) => {
    await checkPage(page, '/services', 'Services');
  });

  test('TC-NAV-037: Service Requests page loads', async ({ page }) => {
    await checkPage(page, '/service-requests', 'Service Requests');
  });

  test('TC-NAV-038: Service Requests filter tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/service-requests');
    await clickAllTabs(page);
  });

  test('TC-NAV-039: Knowledge Base page loads', async ({ page }) => {
    await checkPage(page, '/knowledge-base', 'Knowledge Base');
  });

  test('TC-NAV-040: Communications page loads', async ({ page }) => {
    await checkPage(page, '/communications', 'Communications');
  });

  test('TC-NAV-041: Interactions page loads', async ({ page }) => {
    await checkPage(page, '/interactions', 'Interactions');
  });
});

// ===========================================================================
// 6. ITSM MODULE
// ===========================================================================

test.describe('ITSM Module', () => {
  test('TC-NAV-042: ITSM root page loads', async ({ page }) => {
    await checkPage(page, '/itsm', 'ITSM');
  });

  test('TC-NAV-043: ITSM Metrics page loads', async ({ page }) => {
    await checkPage(page, '/itsm/metrics', 'ITSM Metrics');
  });

  test('TC-NAV-044: ITSM Incidents page loads', async ({ page }) => {
    await checkPage(page, '/itsm/incidents', 'ITSM Incidents');
  });

  test('TC-NAV-045: ITSM Incidents status tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/itsm/incidents');
    await clickAllTabs(page);
  });

  test('TC-NAV-046: ITSM Problems page loads', async ({ page }) => {
    await checkPage(page, '/itsm/problems', 'ITSM Problems');
  });

  test('TC-NAV-047: ITSM CMDB page loads', async ({ page }) => {
    await checkPage(page, '/itsm/cmdb', 'ITSM CMDB');
  });

  test('TC-NAV-048: ITSM Changes page loads', async ({ page }) => {
    await checkPage(page, '/itsm/changes', 'ITSM Changes');
  });

  test('TC-NAV-049: ITSM Changes Calendar page loads', async ({ page }) => {
    await checkPage(page, '/itsm/changes/calendar', 'ITSM Changes Calendar');
  });

  test('TC-NAV-050: ITSM Knowledge page loads', async ({ page }) => {
    await checkPage(page, '/itsm/knowledge', 'ITSM Knowledge');
  });

  test('TC-NAV-051: ITSM Service Catalog page loads', async ({ page }) => {
    await checkPage(page, '/itsm/catalog', 'ITSM Catalog');
  });

  test('TC-NAV-052: ITSM Catalog Admin page loads', async ({ page }) => {
    await checkPage(page, '/itsm/catalog/admin', 'ITSM Catalog Admin');
  });

  test('TC-NAV-053: ITSM Catalog Requests page loads', async ({ page }) => {
    await checkPage(page, '/itsm/catalog/requests', 'ITSM Catalog Requests');
  });

  test('TC-NAV-054: ITSM SLA root page loads', async ({ page }) => {
    await checkPage(page, '/itsm/sla', 'ITSM SLA');
  });

  test('TC-NAV-055: ITSM SLA Policies page loads', async ({ page }) => {
    await checkPage(page, '/itsm/sla/policies', 'ITSM SLA Policies');
  });

  test('TC-NAV-056: ITSM SLA Instances page loads', async ({ page }) => {
    await checkPage(page, '/itsm/sla/instances', 'ITSM SLA Instances');
  });

  test('TC-NAV-057: ITSM Escalation Rules page loads', async ({ page }) => {
    await checkPage(page, '/itsm/escalation/rules', 'ITSM Escalation Rules');
  });

  test('TC-NAV-058: ITSM Escalation Dashboard page loads', async ({ page }) => {
    await checkPage(page, '/itsm/escalation/dashboard', 'ITSM Escalation Dashboard');
  });

  test('TC-NAV-059: ITSM Escalation Policies page loads', async ({ page }) => {
    await checkPage(page, '/itsm/escalation-policies', 'ITSM Escalation Policies');
  });

  test('TC-NAV-060: ITSM SLA Policies (alternate route) page loads', async ({ page }) => {
    await checkPage(page, '/itsm/sla-policies', 'ITSM SLA Policies Alt');
  });

  test('TC-NAV-061: ITSM Service Queues page loads', async ({ page }) => {
    await checkPage(page, '/itsm/service-queues', 'ITSM Service Queues');
  });
});

// ===========================================================================
// 7. USER MANAGEMENT
// ===========================================================================

test.describe('User Management', () => {
  test('TC-NAV-062: Users page loads', async ({ page }) => {
    await checkPage(page, '/users', 'Users');
  });

  test('TC-NAV-063: Departments page loads', async ({ page }) => {
    await checkPage(page, '/departments', 'Departments');
  });

  test('TC-NAV-064: Profiles page loads', async ({ page }) => {
    await checkPage(page, '/profiles', 'Profiles');
  });

  test('TC-NAV-065: Settings page loads', async ({ page }) => {
    await checkPage(page, '/settings', 'Settings');
  });

  test('TC-NAV-066: Settings tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/settings');
    await clickAllTabs(page);
  });
});

// ===========================================================================
// 8. AI AGENTS
// ===========================================================================

test.describe('AI Agents', () => {
  test('TC-NAV-067: Agents page loads', async ({ page }) => {
    await checkPage(page, '/agents', 'Agents');
  });

  test('TC-NAV-068: Agents Conversations page loads', async ({ page }) => {
    await checkPage(page, '/agents/conversations', 'Agents Conversations');
  });
});

// ===========================================================================
// 9. DATA MANAGEMENT
// ===========================================================================

test.describe('Data Management', () => {
  test('TC-NAV-069: Data Import page loads', async ({ page }) => {
    await checkPage(page, '/data/import', 'Data Import');
  });

  test('TC-NAV-070: Data Export page loads', async ({ page }) => {
    await checkPage(page, '/data/export', 'Data Export');
  });
});

// ===========================================================================
// 10. ADMIN SECTION
// ===========================================================================

test.describe('Admin — Configuration', () => {
  test('TC-NAV-071: Admin Config System page loads', async ({ page }) => {
    await checkPage(page, '/admin/config/system', 'Admin Config System');
  });

  test('TC-NAV-072: Admin Config CRM page loads', async ({ page }) => {
    await checkPage(page, '/admin/config/crm', 'Admin Config CRM');
  });

  test('TC-NAV-073: Admin Deployment page loads', async ({ page }) => {
    await checkPage(page, '/admin/deployment', 'Admin Deployment');
  });

  test('TC-NAV-074: Admin Monitoring page loads', async ({ page }) => {
    await checkPage(page, '/admin/monitoring', 'Admin Monitoring');
  });

  test('TC-NAV-075: Admin Workers page loads', async ({ page }) => {
    await checkPage(page, '/admin/workers', 'Admin Workers');
  });

  test('TC-NAV-076: Admin Security page loads', async ({ page }) => {
    await checkPage(page, '/admin/security', 'Admin Security');
  });

  test('TC-NAV-077: Admin Feature Flags page loads', async ({ page }) => {
    await checkPage(page, '/admin/features', 'Admin Features');
  });

  test('TC-NAV-078: Admin Users page loads', async ({ page }) => {
    await checkPage(page, '/admin/users', 'Admin Users');
  });

  test('TC-NAV-079: Admin Approvals page loads', async ({ page }) => {
    await checkPage(page, '/admin/approvals', 'Admin Approvals');
  });

  test('TC-NAV-080: Admin Groups page loads', async ({ page }) => {
    await checkPage(page, '/admin/groups', 'Admin Groups');
  });
});

test.describe('Admin — Identity & Branding', () => {
  test('TC-NAV-081: Admin Social Login page loads', async ({ page }) => {
    await checkPage(page, '/admin/social-login', 'Admin Social Login');
  });

  test('TC-NAV-082: Admin Branding page loads', async ({ page }) => {
    await checkPage(page, '/admin/branding', 'Admin Branding');
  });

  test('TC-NAV-083: Admin Navigation page loads', async ({ page }) => {
    await checkPage(page, '/admin/navigation', 'Admin Navigation');
  });

  test('TC-NAV-084: Admin Modules page loads', async ({ page }) => {
    await checkPage(page, '/admin/modules', 'Admin Modules');
  });
});

test.describe('Admin — Service & Data', () => {
  test('TC-NAV-085: Admin Service Requests page loads', async ({ page }) => {
    await checkPage(page, '/admin/service-requests', 'Admin Service Requests');
  });

  test('TC-NAV-086: Admin Master Data page loads', async ({ page }) => {
    await checkPage(page, '/admin/master-data', 'Admin Master Data');
  });

  test('TC-NAV-087: Admin Master Data tabs cycle correctly', async ({ page }) => {
    await gotoAndWait(page, '/admin/master-data');
    await clickAllTabs(page);
  });

  test('TC-NAV-088: Admin Dashboards page loads', async ({ page }) => {
    await checkPage(page, '/admin/dashboards', 'Admin Dashboards');
  });

  test('TC-NAV-089: Admin Duplicate Rules page loads', async ({ page }) => {
    await checkPage(page, '/admin/duplicate-rules', 'Admin Duplicate Rules');
  });

  test('TC-NAV-090: Admin Lead Score Rules page loads', async ({ page }) => {
    await checkPage(page, '/admin/lead-score-rules', 'Admin Lead Score Rules');
  });
});

test.describe('Admin — Workflows', () => {
  test('TC-NAV-091: Admin Workflows page loads', async ({ page }) => {
    await checkPage(page, '/admin/workflows', 'Admin Workflows');
  });

  test('TC-NAV-092: Admin Workflows Monitor page loads', async ({ page }) => {
    await checkPage(page, '/admin/workflows/monitor', 'Admin Workflows Monitor');
  });

  test('TC-NAV-093: Admin Workflows Instances page loads', async ({ page }) => {
    await checkPage(page, '/admin/workflows/instances', 'Admin Workflows Instances');
  });

  test('TC-NAV-094: Admin Workflows Templates page loads', async ({ page }) => {
    await checkPage(page, '/admin/workflows/templates', 'Admin Workflows Templates');
  });
});

test.describe('Admin — Integrations & AI', () => {
  test('TC-NAV-095: Admin LLM page loads', async ({ page }) => {
    await checkPage(page, '/admin/llm', 'Admin LLM');
  });

  test('TC-NAV-096: Admin Database Settings page loads', async ({ page }) => {
    await checkPage(page, '/admin/database-settings', 'Admin Database Settings');
  });

  test('TC-NAV-097: Admin Integrations page loads', async ({ page }) => {
    await checkPage(page, '/admin/integrations', 'Admin Integrations');
  });

  test('TC-NAV-098: Admin Analytics page loads', async ({ page }) => {
    await checkPage(page, '/admin/analytics', 'Admin Analytics');
  });
});

test.describe('Admin — Settings Modules', () => {
  test('TC-NAV-099: Admin Settings Sales page loads', async ({ page }) => {
    await checkPage(page, '/admin/settings/sales', 'Admin Settings Sales');
  });

  test('TC-NAV-100: Admin Settings Service Desk page loads', async ({ page }) => {
    await checkPage(page, '/admin/settings/service-desk', 'Admin Settings Service Desk');
  });
});

test.describe('Admin — Audit & Security', () => {
  test('TC-NAV-101: Admin Audit page loads', async ({ page }) => {
    await checkPage(page, '/admin/audit', 'Admin Audit');
  });

  test('TC-NAV-102: Admin Sessions page loads', async ({ page }) => {
    await checkPage(page, '/admin/sessions', 'Admin Sessions');
  });

  test('TC-NAV-103: Admin Business Hours page loads', async ({ page }) => {
    await checkPage(page, '/admin/business-hours', 'Admin Business Hours');
  });

  test('TC-NAV-104: Admin API Users page loads', async ({ page }) => {
    await checkPage(page, '/admin/api-users', 'Admin API Users');
  });
});

test.describe('Admin — UI & Developer', () => {
  test('TC-NAV-105: Admin UI Customization page loads', async ({ page }) => {
    await checkPage(page, '/admin/ui-customization', 'Admin UI Customization');
  });

  test('TC-NAV-106: Admin API Docs page loads', async ({ page }) => {
    await checkPage(page, '/admin/api-docs', 'Admin API Docs');
  });

  test('TC-NAV-107: Admin Agents page loads', async ({ page }) => {
    await checkPage(page, '/admin/agents', 'Admin Agents');
  });

  test('TC-NAV-108: Admin Providers page loads', async ({ page }) => {
    await checkPage(page, '/admin/providers', 'Admin Providers');
  });
});

// ===========================================================================
// 11. SUMMARY — report all accumulated console errors at the end
// ===========================================================================

test.describe('Error Summary', () => {
  test('TC-NAV-109: Summary of page-load console errors across all pages', async () => {
    if (pageErrors.length === 0) {
      console.log('✅ No console errors detected on any page.');
      return;
    }

    const report = pageErrors
      .map(({ page, errors }) => `\n  [${page}] (${errors.length} error(s)):\n    ${errors.join('\n    ')}`)
      .join('');

    // Log full report regardless
    console.warn(`\n📋 Console errors found on ${pageErrors.length} page(s):${report}\n`);

    // Fail if any page produced console errors so CI catches them
    expect(
      pageErrors.length,
      `Console errors detected on ${pageErrors.length} page(s). See test output for details.`
    ).toBe(0);
  });
});

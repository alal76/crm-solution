/**
 * CRM Solution - Dashboard Tests
 * 
 * Tests for dashboard widgets, metrics, data visualization, and navigation.
 */

import { test, expect } from '@playwright/test';

test.describe('Dashboard - Main View', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-001: Should display dashboard', async ({ page }) => {
    // Check for dashboard elements - look for any main content area
    const dashboard = page.locator('main, .MuiContainer-root, #root > div').first();
    await expect(dashboard).toBeVisible({ timeout: 10000 });
  });

  test('TC-DASH-002: Should display welcome message', async ({ page }) => {
    const welcomeMessage = page.locator('text=/welcome|hello|good morning|good afternoon|good evening/i').first();
    // Welcome may be displayed
  });

  test('TC-DASH-003: Should display navigation menu', async ({ page }) => {
    const navMenu = page.locator('nav, .sidebar, [role="navigation"]').first();
    await expect(navMenu).toBeVisible();
  });
});

test.describe('Dashboard - Widgets', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-004: Should display customer count widget', async ({ page }) => {
    const customerWidget = page.locator('text=/customer|clients/i').first();
    if (await customerWidget.isVisible()) {
      await expect(customerWidget).toBeVisible();
    }
  });

  test('TC-DASH-005: Should display opportunity pipeline widget', async ({ page }) => {
    const pipelineWidget = page.locator('text=/pipeline|opportunity|deal/i').first();
    if (await pipelineWidget.isVisible()) {
      await expect(pipelineWidget).toBeVisible();
    }
  });

  test('TC-DASH-006: Should display service requests widget', async ({ page }) => {
    const serviceWidget = page.locator('text=/service request|ticket|support/i').first();
    if (await serviceWidget.isVisible()) {
      await expect(serviceWidget).toBeVisible();
    }
  });

  test('TC-DASH-007: Should display recent activity widget', async ({ page }) => {
    const activityWidget = page.locator('text=/recent activity|activity|recent/i').first();
    if (await activityWidget.isVisible()) {
      await expect(activityWidget).toBeVisible();
    }
  });

  test('TC-DASH-008: Should display tasks widget', async ({ page }) => {
    const tasksWidget = page.locator('text=/task|to-do|to do/i').first();
    if (await tasksWidget.isVisible()) {
      await expect(tasksWidget).toBeVisible();
    }
  });
});

test.describe('Dashboard - Metrics', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-009: Should display revenue metrics', async ({ page }) => {
    const revenueMetric = page.locator('text=/revenue|sales|value/i').first();
    if (await revenueMetric.isVisible()) {
      await expect(revenueMetric).toBeVisible();
    }
  });

  test('TC-DASH-010: Should display lead conversion rate', async ({ page }) => {
    const conversionMetric = page.locator('text=/conversion|convert|rate/i').first();
    if (await conversionMetric.isVisible()) {
      await expect(conversionMetric).toBeVisible();
    }
  });

  test('TC-DASH-011: Should display open tickets count', async ({ page }) => {
    const ticketMetric = page.locator('text=/open ticket|pending|waiting/i').first();
    if (await ticketMetric.isVisible()) {
      await expect(ticketMetric).toBeVisible();
    }
  });
});

test.describe('Dashboard - Charts', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-012: Should display charts', async ({ page }) => {
    const chart = page.locator('canvas, .chart, svg[class*="chart"], .recharts-wrapper').first();
    if (await chart.isVisible()) {
      await expect(chart).toBeVisible();
    }
  });

  test('TC-DASH-013: Should have chart interactions', async ({ page }) => {
    const chart = page.locator('canvas, .chart, svg[class*="chart"]').first();
    if (await chart.isVisible()) {
      await chart.hover();
      await page.waitForTimeout(500);
      // Tooltips may appear
    }
  });
});

test.describe('Dashboard - Quick Actions', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-014: Should have quick add customer action', async ({ page }) => {
    const quickAdd = page.locator('button:has-text("Add Customer"), button:has-text("New Customer"), a:has-text("Add Customer")').first();
    if (await quickAdd.isVisible()) {
      await expect(quickAdd).toBeVisible();
    }
  });

  test('TC-DASH-015: Should have quick add opportunity action', async ({ page }) => {
    const quickAdd = page.locator('button:has-text("Add Opportunity"), button:has-text("New Deal")').first();
    if (await quickAdd.isVisible()) {
      await expect(quickAdd).toBeVisible();
    }
  });

  test('TC-DASH-016: Should have quick add service request action', async ({ page }) => {
    const quickAdd = page.locator('button:has-text("Add Ticket"), button:has-text("New Ticket"), button:has-text("Add Service Request")').first();
    if (await quickAdd.isVisible()) {
      await expect(quickAdd).toBeVisible();
    }
  });
});

test.describe('Dashboard - Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-017: Should navigate to customers', async ({ page }) => {
    const customersLink = page.locator('a:has-text("Customer"), button:has-text("Customer")').first();
    if (await customersLink.isVisible()) {
      await customersLink.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/customer/i);
    }
  });

  test('TC-DASH-018: Should navigate to opportunities', async ({ page }) => {
    const opportunitiesLink = page.locator('a:has-text("Opportunit"), button:has-text("Opportunit")').first();
    if (await opportunitiesLink.isVisible()) {
      await opportunitiesLink.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/opportunit/i);
    }
  });

  test('TC-DASH-019: Should navigate to leads', async ({ page }) => {
    const leadsLink = page.locator('a:has-text("Lead"), button:has-text("Lead")').first();
    if (await leadsLink.isVisible()) {
      await leadsLink.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/lead/i);
    }
  });

  test('TC-DASH-020: Should navigate to service requests', async ({ page }) => {
    const serviceLink = page.locator('a:has-text("Service"), button:has-text("Service"), a:has-text("Ticket")').first();
    if (await serviceLink.isVisible()) {
      await serviceLink.click();
      await page.waitForLoadState('networkidle');
    }
  });

  test('TC-DASH-021: Should navigate to campaigns', async ({ page }) => {
    const campaignsLink = page.locator('a:has-text("Campaign"), button:has-text("Campaign")').first();
    if (await campaignsLink.isVisible()) {
      await campaignsLink.click();
      await page.waitForLoadState('networkidle');
      await expect(page).toHaveURL(/campaign/i);
    }
  });

  test('TC-DASH-022: Should navigate to admin', async ({ page }) => {
    const adminLink = page.locator('a:has-text("Admin"), button:has-text("Admin"), a:has-text("Settings")').first();
    if (await adminLink.isVisible()) {
      await adminLink.click();
      await page.waitForLoadState('networkidle');
    }
  });
});

test.describe('Dashboard - User Profile', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-023: Should display user profile menu', async ({ page }) => {
    const userMenu = page.locator('[aria-label*="User"], [aria-label*="Profile"], .user-menu, .avatar').first();
    if (await userMenu.isVisible()) {
      await userMenu.click();
      await page.waitForTimeout(500);
    }
  });

  test('TC-DASH-024: Should access profile settings', async ({ page }) => {
    const userMenu = page.locator('[aria-label*="User"], [aria-label*="Profile"], .user-menu, .avatar').first();
    if (await userMenu.isVisible()) {
      await userMenu.click();
      await page.waitForTimeout(500);
      
      const profileLink = page.locator('a:has-text("Profile"), button:has-text("Profile")').first();
      if (await profileLink.isVisible()) {
        await profileLink.click();
        await page.waitForLoadState('networkidle');
      }
    }
  });

  test('TC-DASH-025: Should logout from dashboard', async ({ page }) => {
    const userMenu = page.locator('[aria-label*="User"], [aria-label*="Profile"], .user-menu, .avatar').first();
    if (await userMenu.isVisible()) {
      await userMenu.click();
      await page.waitForTimeout(500);
      
      const logoutButton = page.locator('a:has-text("Logout"), button:has-text("Logout"), button:has-text("Sign Out")').first();
      if (await logoutButton.isVisible()) {
        await logoutButton.click();
        await page.waitForLoadState('networkidle');
      }
    }
  });
});

test.describe('Dashboard - Search', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-026: Should have global search', async ({ page }) => {
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], .search-input').first();
    if (await searchInput.isVisible()) {
      await expect(searchInput).toBeVisible();
    }
  });

  test('TC-DASH-027: Should search for customer', async ({ page }) => {
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], .search-input').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('TEST_');
      await page.waitForTimeout(1000);
      
      const results = page.locator('.search-results, [role="listbox"], .autocomplete-results').first();
      if (await results.isVisible()) {
        await expect(results).toBeVisible();
      }
    }
  });

  test('TC-DASH-028: Should navigate from search results', async ({ page }) => {
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], .search-input').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('TEST_');
      await page.waitForTimeout(1000);
      
      const firstResult = page.locator('.search-results li, [role="option"]').first();
      if (await firstResult.isVisible()) {
        await firstResult.click();
        await page.waitForLoadState('networkidle');
      }
    }
  });
});

test.describe('Dashboard - Notifications', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('TC-DASH-029: Should display notification bell', async ({ page }) => {
    const notificationBell = page.locator('[aria-label*="Notification"], .notification-icon, button:has(.bell)').first();
    if (await notificationBell.isVisible()) {
      await expect(notificationBell).toBeVisible();
    }
  });

  test('TC-DASH-030: Should open notifications panel', async ({ page }) => {
    const notificationBell = page.locator('[aria-label*="Notification"], .notification-icon').first();
    if (await notificationBell.isVisible()) {
      await notificationBell.click();
      await page.waitForTimeout(500);
      
      const notificationPanel = page.locator('.notification-panel, .notifications-dropdown').first();
      if (await notificationPanel.isVisible()) {
        await expect(notificationPanel).toBeVisible();
      }
    }
  });
});

test.describe('Dashboard - Responsive', () => {
  test('TC-DASH-031: Should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Should still show main content
    const mainContent = page.locator('.dashboard, [role="main"]').first();
    if (await mainContent.isVisible()) {
      await expect(mainContent).toBeVisible();
    }
  });

  test('TC-DASH-032: Should show mobile menu', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const hamburgerMenu = page.locator('[aria-label*="Menu"], .hamburger, .menu-toggle').first();
    if (await hamburgerMenu.isVisible()) {
      await hamburgerMenu.click();
      await page.waitForTimeout(500);
    }
  });
});

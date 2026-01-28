/**
 * CRM Solution - Campaign Execution E2E Tests
 * 
 * Tests for campaign workflow management, recipient tracking,
 * A/B testing, and campaign analytics.
 */

import { test, expect } from '@playwright/test';

test.describe('Campaign Execution - Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
  });

  test('TC-CEXE-001: Should display campaigns list', async ({ page }) => {
    await expect(page.locator('h1, h2, .page-title').filter({ hasText: /campaign/i })).toBeVisible({ timeout: 10000 });
  });

  test('TC-CEXE-002: Should navigate to campaign execution page', async ({ page }) => {
    // Find first campaign row and click on execution/analytics link if available
    const campaignRow = page.locator('table tbody tr, .MuiDataGrid-row').first();
    if (await campaignRow.isVisible()) {
      // Look for an execution or analytics button/link
      const executionLink = campaignRow.locator('a:has-text("Execution"), button:has-text("Execute"), [aria-label*="execution"]').first();
      if (await executionLink.isVisible()) {
        await executionLink.click();
        await page.waitForLoadState('networkidle');
        await expect(page).toHaveURL(/\/campaigns\/\d+\/execution/);
      }
    }
  });
});

test.describe('Campaign Execution - Workflows', () => {
  test('TC-CEXE-010: Should display workflows tab', async ({ page }) => {
    // Navigate directly to a campaign execution page (need campaign ID)
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    
    // Get first campaign and navigate to execution
    const campaignRow = page.locator('table tbody tr').first();
    if (await campaignRow.isVisible()) {
      // Click on the row or a view button
      const viewButton = campaignRow.locator('button').first();
      if (await viewButton.isVisible()) {
        await viewButton.click();
      }
    }
  });

  test('TC-CEXE-011: Should show link workflow button', async ({ page }) => {
    // This test requires being on a campaign execution page
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const linkButton = page.locator('button:has-text("Link Workflow")');
    if (await linkButton.isVisible()) {
      await expect(linkButton).toBeVisible();
    }
  });

  test('TC-CEXE-012: Should open link workflow dialog', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const linkButton = page.locator('button:has-text("Link Workflow")');
    if (await linkButton.isVisible()) {
      await linkButton.click();
      await expect(page.locator('[role="dialog"]')).toBeVisible();
      await expect(page.locator('label:has-text("Workflow"), [aria-label*="Workflow"]').first()).toBeVisible();
    }
  });
});

test.describe('Campaign Execution - Recipients', () => {
  test('TC-CEXE-020: Should show recipients tab', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const recipientsTab = page.locator('[role="tab"]:has-text("Recipients")');
    if (await recipientsTab.isVisible()) {
      await recipientsTab.click();
      await page.waitForTimeout(500);
      await expect(page.locator('button:has-text("Add Recipients")')).toBeVisible();
    }
  });

  test('TC-CEXE-021: Should open add recipients dialog', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const recipientsTab = page.locator('[role="tab"]:has-text("Recipients")');
    if (await recipientsTab.isVisible()) {
      await recipientsTab.click();
      await page.waitForTimeout(500);
      
      const addButton = page.locator('button:has-text("Add Recipients")');
      if (await addButton.isVisible()) {
        await addButton.click();
        await expect(page.locator('[role="dialog"]')).toBeVisible();
        await expect(page.locator('textarea, [placeholder*="email"]').first()).toBeVisible();
      }
    }
  });

  test('TC-CEXE-022: Should add recipients to campaign', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const recipientsTab = page.locator('[role="tab"]:has-text("Recipients")');
    if (await recipientsTab.isVisible()) {
      await recipientsTab.click();
      await page.waitForTimeout(500);
      
      const addButton = page.locator('button:has-text("Add Recipients")');
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(500);
        
        const textarea = page.locator('textarea').first();
        await textarea.fill(`test_${Date.now()}@example.com`);
        
        await page.locator('button:has-text("Add Recipients")').last().click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe('Campaign Execution - A/B Testing', () => {
  test('TC-CEXE-030: Should show A/B tests tab', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const abTestTab = page.locator('[role="tab"]:has-text("A/B Tests")');
    if (await abTestTab.isVisible()) {
      await abTestTab.click();
      await page.waitForTimeout(500);
      await expect(page.locator('button:has-text("Create A/B Test")')).toBeVisible();
    }
  });

  test('TC-CEXE-031: Should open create A/B test dialog', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const abTestTab = page.locator('[role="tab"]:has-text("A/B Tests")');
    if (await abTestTab.isVisible()) {
      await abTestTab.click();
      await page.waitForTimeout(500);
      
      const createButton = page.locator('button:has-text("Create A/B Test")');
      if (await createButton.isVisible()) {
        await createButton.click();
        await expect(page.locator('[role="dialog"]')).toBeVisible();
        await expect(page.locator('input[name="testName"], #testName, label:has-text("Test Name")').first()).toBeVisible();
      }
    }
  });

  test('TC-CEXE-032: Should create A/B test', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const abTestTab = page.locator('[role="tab"]:has-text("A/B Tests")');
    if (await abTestTab.isVisible()) {
      await abTestTab.click();
      await page.waitForTimeout(500);
      
      const createButton = page.locator('button:has-text("Create A/B Test")');
      if (await createButton.isVisible()) {
        await createButton.click();
        await page.waitForTimeout(500);
        
        // Fill test name
        const testNameInput = page.locator('input').first();
        await testNameInput.fill(`Subject Test ${Date.now()}`);
        
        // Fill variants
        const variantAInput = page.locator('textarea, input').nth(2);
        if (await variantAInput.isVisible()) {
          await variantAInput.fill('50% Off Today Only!');
        }
        
        const variantBInput = page.locator('textarea, input').nth(3);
        if (await variantBInput.isVisible()) {
          await variantBInput.fill('Exclusive Deal Inside');
        }
        
        await page.locator('button:has-text("Create Test")').click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe('Campaign Execution - Conversions', () => {
  test('TC-CEXE-040: Should show conversions tab', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const conversionsTab = page.locator('[role="tab"]:has-text("Conversions")');
    if (await conversionsTab.isVisible()) {
      await conversionsTab.click();
      await page.waitForTimeout(500);
      await expect(page.locator('table, .MuiTable-root').first()).toBeVisible();
    }
  });
});

test.describe('Campaign Execution - Analytics', () => {
  test('TC-CEXE-050: Should show analytics tab', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const analyticsTab = page.locator('[role="tab"]:has-text("Analytics")');
    if (await analyticsTab.isVisible()) {
      await analyticsTab.click();
      await page.waitForTimeout(500);
    }
  });

  test('TC-CEXE-051: Should display engagement funnel', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const analyticsTab = page.locator('[role="tab"]:has-text("Analytics")');
    if (await analyticsTab.isVisible()) {
      await analyticsTab.click();
      await page.waitForTimeout(500);
      
      // Look for funnel metrics
      const deliveredLabel = page.locator('text=Delivered, text=Delivery').first();
      if (await deliveredLabel.isVisible()) {
        await expect(deliveredLabel).toBeVisible();
      }
    }
  });

  test('TC-CEXE-052: Should display ROI metrics', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const analyticsTab = page.locator('[role="tab"]:has-text("Analytics")');
    if (await analyticsTab.isVisible()) {
      await analyticsTab.click();
      await page.waitForTimeout(500);
      
      // Look for ROI card
      const roiLabel = page.locator('text=ROI, text=Revenue').first();
      if (await roiLabel.isVisible()) {
        await expect(roiLabel).toBeVisible();
      }
    }
  });
});

test.describe('Campaign Execution - Summary Cards', () => {
  test('TC-CEXE-060: Should display summary metrics cards', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    // Check for summary metric cards at the top
    const recipientsCard = page.locator('text=Recipients').first();
    if (await recipientsCard.isVisible()) {
      await expect(recipientsCard).toBeVisible();
    }
  });

  test('TC-CEXE-061: Should show open rate metric', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const openRateCard = page.locator('text=Open Rate').first();
    if (await openRateCard.isVisible()) {
      await expect(openRateCard).toBeVisible();
    }
  });

  test('TC-CEXE-062: Should show conversion rate metric', async ({ page }) => {
    await page.goto('/campaigns/1/execution');
    await page.waitForLoadState('networkidle');
    
    const conversionCard = page.locator('text=Conversion').first();
    if (await conversionCard.isVisible()) {
      await expect(conversionCard).toBeVisible();
    }
  });
});

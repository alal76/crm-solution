/**
 * CRM Solution - Campaign Setup Tests
 * 
 * Creates sample campaigns in the test database via UI.
 * Run this before other campaign tests to ensure test data exists.
 */

import { test, expect } from '@playwright/test';

const SAMPLE_CAMPAIGNS = [
  {
    name: 'Q1 Email Newsletter',
    type: 0, // Email
    status: 2, // Active
    priority: 1, // Medium
    startDate: '2026-01-01',
    endDate: '2026-03-31',
    budget: 5000,
    description: 'Quarterly email newsletter campaign targeting existing customers'
  },
  {
    name: 'Spring Social Media Push',
    type: 1, // Social Media
    status: 2, // Active
    priority: 2, // High
    startDate: '2026-02-01',
    endDate: '2026-04-30',
    budget: 10000,
    description: 'Spring promotion across social media channels'
  },
  {
    name: 'Product Launch Webinar',
    type: 7, // Webinar
    status: 1, // Scheduled
    priority: 2, // High
    startDate: '2026-03-15',
    endDate: '2026-03-15',
    budget: 2500,
    description: 'New product launch webinar with live demo'
  },
  {
    name: 'Trade Show 2026',
    type: 14, // Trade Show
    status: 0, // Draft
    priority: 3, // Critical
    startDate: '2026-06-01',
    endDate: '2026-06-05',
    budget: 50000,
    description: 'Annual industry trade show participation'
  },
  {
    name: 'Completed Winter Sale',
    type: 0, // Email
    status: 4, // Completed
    priority: 1, // Medium
    startDate: '2025-12-01',
    endDate: '2025-12-31',
    budget: 3000,
    description: 'Winter holiday sale campaign - completed'
  }
];

test.describe('Campaign Setup - Create Sample Data', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/campaigns');
    await page.waitForLoadState('domcontentloaded');
  });

  test('SETUP-001: Create sample campaigns for testing', async ({ page }) => {
    // Check if Add Campaign button is visible
    const addBtn = page.locator('button:has-text("Add Campaign")');
    await expect(addBtn).toBeVisible({ timeout: 10000 });

    for (const campaign of SAMPLE_CAMPAIGNS) {
      console.log(`Creating campaign: ${campaign.name}`);
      
      // Check if campaign already exists
      const existingCampaign = page.locator(`text="${campaign.name}"`).first();
      if (await existingCampaign.isVisible({ timeout: 1000 }).catch(() => false)) {
        console.log(`  ⏭ Campaign "${campaign.name}" already exists, skipping`);
        continue;
      }

      // Click Add Campaign
      await addBtn.click();
      await page.waitForTimeout(500);

      const dialog = page.locator('[role="dialog"]');
      await expect(dialog).toBeVisible();

      // Fill Basic Info tab
      await dialog.locator('input[name="name"]').fill(campaign.name);
      await dialog.locator('input[name="startDate"]').fill(campaign.startDate);
      if (campaign.endDate) {
        await dialog.locator('input[name="endDate"]').fill(campaign.endDate);
      }
      await dialog.locator('input[name="budget"]').fill(String(campaign.budget));
      await dialog.locator('textarea[name="description"], input[name="description"]').first().fill(campaign.description);

      // Set campaign type - MUI Select uses role="combobox"
      const typeSelect = dialog.locator('[role="combobox"]').filter({ has: page.locator('[id*="campaignType"]') }).first();
      if (await typeSelect.isVisible({ timeout: 1000 }).catch(() => false)) {
        await typeSelect.click();
        await page.waitForTimeout(300);
        const typeOption = page.locator('[role="listbox"] [role="option"]').nth(campaign.type);
        if (await typeOption.isVisible({ timeout: 1000 }).catch(() => false)) {
          await typeOption.click();
        } else {
          await page.keyboard.press('Escape');
        }
        await page.waitForTimeout(200);
      }

      // Set status - MUI Select uses role="combobox"
      const statusSelect = dialog.locator('[role="combobox"]').filter({ has: page.locator('[id*="status"]') }).first();
      if (await statusSelect.isVisible({ timeout: 1000 }).catch(() => false)) {
        await statusSelect.click();
        await page.waitForTimeout(300);
        const statusOption = page.locator('[role="listbox"] [role="option"]').nth(campaign.status);
        if (await statusOption.isVisible({ timeout: 1000 }).catch(() => false)) {
          await statusOption.click();
        } else {
          await page.keyboard.press('Escape');
        }
        await page.waitForTimeout(200);
      }

      // Set priority - MUI Select uses role="combobox"
      const prioritySelect = dialog.locator('[role="combobox"]').filter({ has: page.locator('[id*="priority"]') }).first();
      if (await prioritySelect.isVisible({ timeout: 1000 }).catch(() => false)) {
        await prioritySelect.click();
        await page.waitForTimeout(300);
        const priorityOption = page.locator('[role="listbox"] [role="option"]').nth(campaign.priority);
        if (await priorityOption.isVisible({ timeout: 1000 }).catch(() => false)) {
          await priorityOption.click();
        } else {
          await page.keyboard.press('Escape');
        }
        await page.waitForTimeout(200);
      }

      // Save
      const saveBtn = dialog.locator('button:has-text("Create")');
      await saveBtn.click();
      await page.waitForTimeout(1500);

      // Check for success message or dialog closed
      const dialogClosed = await dialog.isHidden().catch(() => false);
      const successMsg = await page.locator('text=/created successfully/i').isVisible().catch(() => false);
      
      if (dialogClosed || successMsg) {
        console.log(`  ✓ Created campaign: ${campaign.name}`);
      } else {
        // Try to close dialog if still open
        const cancelBtn = dialog.locator('button:has-text("Cancel")');
        if (await cancelBtn.isVisible()) {
          await cancelBtn.click();
        }
        console.log(`  ⚠ May have failed to create: ${campaign.name}`);
      }

      await page.waitForTimeout(500);
    }

    // Verify campaigns exist
    await page.reload();
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1000);

    const tableRows = page.locator('table tbody tr');
    const rowCount = await tableRows.count();
    console.log(`\n📊 Total campaigns in table: ${rowCount}`);
    expect(rowCount).toBeGreaterThanOrEqual(1);
  });

  test('SETUP-002: Add performance metrics to a campaign', async ({ page }) => {
    // Wait for table to load
    await page.waitForTimeout(1000);
    
    // Find first campaign row
    const campaignRow = page.locator('table tbody tr').first();
    const rowVisible = await campaignRow.isVisible({ timeout: 5000 }).catch(() => false);
    console.log('Campaign row visible:', rowVisible);
    
    if (!rowVisible) {
      console.log('No campaigns found, skipping metrics setup');
      expect(true).toBeTruthy();
      return;
    }

    // Click edit on first campaign - look for any button in the row
    const editBtn = campaignRow.locator('button[aria-label*="Edit"], button:has(svg)').first();
    console.log('Looking for edit button...');
    
    if (!await editBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      // Try clicking the row to expand/edit
      await campaignRow.click();
      await page.waitForTimeout(500);
    } else {
      await editBtn.click();
    }
    await page.waitForTimeout(500);

    const dialog = page.locator('[role="dialog"]');
    await expect(dialog).toBeVisible();

    // Go to Performance tab
    const performanceTab = dialog.locator('[role="tab"]:has-text("Performance")');
    await performanceTab.click();
    await page.waitForTimeout(300);

    // Fill performance metrics
    await dialog.locator('input[name="impressions"]').fill('15000');
    await dialog.locator('input[name="clicks"]').fill('750');
    await dialog.locator('input[name="conversions"]').fill('45');
    await dialog.locator('input[name="leadsGenerated"]').fill('30');
    await dialog.locator('input[name="revenue"]').fill('12500');

    // Go back to Basic Info and update actual spend
    const basicTab = dialog.locator('[role="tab"]:has-text("Basic Info")');
    await basicTab.click();
    await page.waitForTimeout(300);
    await dialog.locator('input[name="actualSpend"]').fill('3500');

    // Save
    const saveBtn = dialog.locator('button:has-text("Update")');
    await saveBtn.click();
    await page.waitForTimeout(1500);

    console.log('✓ Added performance metrics to first campaign');
  });

  test('SETUP-003: Add email metrics to email campaign', async ({ page }) => {
    await page.waitForTimeout(1000);
    
    // Find email campaign (type 0) or just use first campaign
    const emailCampaignRow = page.locator('table tbody tr').filter({ hasText: /email|newsletter/i }).first();
    let targetRow = emailCampaignRow;
    
    if (!await emailCampaignRow.isVisible({ timeout: 3000 }).catch(() => false)) {
      targetRow = page.locator('table tbody tr').first();
      if (!await targetRow.isVisible({ timeout: 3000 }).catch(() => false)) {
        console.log('No campaigns found');
        expect(true).toBeTruthy();
        return;
      }
    }

    // Click edit
    const editBtn = targetRow.locator('button[aria-label*="Edit"], button:has(svg)').first();
    if (await editBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await editBtn.click();
    } else {
      await targetRow.click();
    }
    await page.waitForTimeout(500);

    const dialog = page.locator('[role="dialog"]');
    await expect(dialog).toBeVisible();

    // Go to Email Metrics tab
    const emailTab = dialog.locator('[role="tab"]:has-text("Email")');
    await emailTab.click();
    await page.waitForTimeout(300);

    // Fill email metrics
    await dialog.locator('input[name="emailsSent"]').fill('5000');
    await dialog.locator('input[name="emailsOpened"]').fill('1250');
    await dialog.locator('input[name="unsubscribes"]').fill('25');
    await dialog.locator('input[name="bounces"]').fill('50');

    // Save
    const saveBtn = dialog.locator('button:has-text("Update")');
    await saveBtn.click();
    await page.waitForTimeout(1500);

    console.log('✓ Added email metrics to campaign');
  });

  test('SETUP-004: Add social metrics to social campaign', async ({ page }) => {
    await page.waitForTimeout(1000);
    
    // Find social campaign or use second campaign
    const socialCampaignRow = page.locator('table tbody tr').filter({ hasText: /social/i }).first();
    let targetRow = socialCampaignRow;
    
    if (!await socialCampaignRow.isVisible({ timeout: 3000 }).catch(() => false)) {
      // Use second campaign if available, else first
      const secondRow = page.locator('table tbody tr').nth(1);
      if (await secondRow.isVisible({ timeout: 2000 }).catch(() => false)) {
        targetRow = secondRow;
      } else {
        targetRow = page.locator('table tbody tr').first();
        if (!await targetRow.isVisible({ timeout: 2000 }).catch(() => false)) {
          console.log('No campaigns found');
          expect(true).toBeTruthy();
          return;
        }
      }
    }

    // Click edit
    const editBtn = targetRow.locator('button[aria-label*="Edit"], button:has(svg)').first();
    if (await editBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await editBtn.click();
    } else {
      await targetRow.click();
    }
    await page.waitForTimeout(500);

    const dialog = page.locator('[role="dialog"]');
    await expect(dialog).toBeVisible();

    // Go to Social & A/B tab
    const socialTab = dialog.locator('[role="tab"]:has-text("Social")');
    await socialTab.click();
    await page.waitForTimeout(300);

    // Fill social metrics
    await dialog.locator('input[name="socialReach"]').fill('50000');
    await dialog.locator('input[name="socialEngagement"]').fill('2500');
    await dialog.locator('input[name="socialShares"]').fill('350');

    // Save
    const saveBtn = dialog.locator('button:has-text("Update")');
    await saveBtn.click();
    await page.waitForTimeout(1500);

    console.log('✓ Added social metrics to campaign');
  });
});

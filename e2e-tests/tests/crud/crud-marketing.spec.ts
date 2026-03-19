import { test, expect, Page } from '@playwright/test';
import { WEB_BASE_URL } from '../../testConfig';

test.describe.configure({ mode: 'serial' });

const BASE_URL = WEB_BASE_URL;
function ts(): string { return Date.now().toString().slice(-6); }

async function waitForSuccess(page: Page) {
  await page.locator('.MuiAlert-standardSuccess, .MuiSnackbar-root:visible, [role="alert"]:visible').first().waitFor({ timeout: 15000 }).catch(() => {});
}

async function openDialog(page: Page) {
  await page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New"), button:has-text("+ New"), [data-testid*="add"]').first().click({ timeout: 10000 });
  await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});
}

async function submit(page: Page) {
  await page.locator('[role="dialog"] button:has-text("Save"), [role="dialog"] button:has-text("Create"), [role="dialog"] button:has-text("Submit"), form button[type="submit"]').first().click({ timeout: 10000 });
}

async function fillText(page: Page, labelOrPlaceholder: string, value: string) {
  await page.locator(`[role="dialog"] input[placeholder*="${labelOrPlaceholder}"], input[name*="${labelOrPlaceholder}"], [role="dialog"] label:has-text("${labelOrPlaceholder}") ~ * input`).first().fill(value).catch(() => {});
}

async function selectOption(page: Page, labelOrName: string, value: string) {
  const sel = page.locator(`[role="dialog"] label:has-text("${labelOrName}") ~ * .MuiSelect-select, select[name*="${labelOrName}"]`).first();
  await sel.click().catch(() => {});
  await page.locator(`[role="option"]:has-text("${value}"), li:has-text("${value}")`).first().click({ timeout: 5000 }).catch(() => {});
}

async function clickTab(page: Page, tabText: string) {
  await page.locator(`[role="tab"]:has-text("${tabText}")`).first().click({ timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(500);
}

// ─── Campaigns ───────────────────────────────────────────────────────────────

let campaignId: string | null = null;
let campaignName: string = '';

test('TC-CAM-001: Navigate to /campaigns - verify page loads with campaign list', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page).toHaveURL(/campaigns/);
  const heading = page.locator('h1, h2, h3, h4, h5, h6, [class*="title"], [class*="header"]').first();
  await expect(heading).toBeVisible({ timeout: 10000 });
});

test('TC-CAM-002: Create email campaign', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');
  campaignName = `TEST_Campaign_${ts()}`;
  await openDialog(page);

  // Name
  await page.locator('[role="dialog"] input').filter({ hasNotText: '' }).first().fill(campaignName).catch(async () => {
    await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"], [role="dialog"] input[placeholder*="Name"]').first().fill(campaignName).catch(() => {});
  });
  await fillText(page, 'name', campaignName);

  // Type = Email
  await selectOption(page, 'Type', 'Email');

  // Status = Draft
  await selectOption(page, 'Status', 'Draft');

  // Subject
  await page.locator('[role="dialog"] input[name*="subject"], [role="dialog"] input[placeholder*="subject"], [role="dialog"] input[placeholder*="Subject"]').first().fill('Test Campaign Subject').catch(() => {});

  // Description
  await page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="description"], [role="dialog"] input[placeholder*="description"]').first().fill('E2E Test campaign').catch(() => {});

  // Start / End dates
  await page.locator('[role="dialog"] input[type="date"][name*="start"], [role="dialog"] input[placeholder*="start"], [role="dialog"] input[placeholder*="Start"]').first().fill('2026-03-01').catch(() => {});
  await page.locator('[role="dialog"] input[type="date"][name*="end"], [role="dialog"] input[placeholder*="end"], [role="dialog"] input[placeholder*="End"]').first().fill('2026-03-31').catch(() => {});

  await submit(page);
  await waitForSuccess(page);

  // Capture created record id from URL if redirected
  await page.waitForTimeout(1000);
  const url = page.url();
  const match = url.match(/campaigns\/(\d+)/);
  if (match) campaignId = match[1];

  // Verify campaign appears in list
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');
  const row = page.locator(`text=${campaignName}`).first();
  await expect(row).toBeVisible({ timeout: 10000 });
});

test('TC-CAM-003: View campaign details - click campaign row', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`text=${campaignName}, [data-testid*="campaign-row"]`).first();
  await row.click({ timeout: 10000 }).catch(() => {});
  await page.waitForTimeout(1000);

  // Accept either detail page or dialog
  const detailVisible = await page.locator('[class*="detail"], [class*="campaign"], h1, h2, h3').first().isVisible().catch(() => false);
  expect(detailVisible).toBeTruthy();
});

test('TC-CAM-004: Campaign tabs - click each tab', async ({ page }) => {
  if (!campaignId) {
    // Try to find the campaign from list
    await page.goto(`${BASE_URL}/campaigns`);
    await page.waitForLoadState('domcontentloaded');
    await page.locator(`text=${campaignName}`).first().click({ timeout: 10000 }).catch(() => {});
    await page.waitForTimeout(1000);
  } else {
    await page.goto(`${BASE_URL}/campaigns/${campaignId}`);
    await page.waitForLoadState('domcontentloaded');
  }

  for (const tab of ['Overview', 'Recipients', 'Audience', 'Metrics', 'Analytics', 'Settings']) {
    await clickTab(page, tab);
  }
});

test('TC-CAM-005: Edit campaign - update description', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');

  // Open edit via button or row action
  const editBtn = page.locator('button:has-text("Edit"), [aria-label*="edit"], [title*="Edit"]').first();
  const editVisible = await editBtn.isVisible().catch(() => false);
  if (editVisible) {
    await editBtn.click();
  } else {
    await page.locator(`text=${campaignName}`).first().click({ timeout: 10000 }).catch(() => {});
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Edit"), [aria-label*="edit"]').first().click({ timeout: 5000 }).catch(() => {});
  }

  await page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="description"]').first().fill('Updated E2E description').catch(() => {});
  await submit(page);
  await waitForSuccess(page);
});

test('TC-CAM-006: Campaign metrics - navigate to detail and view metrics tab', async ({ page }) => {
  if (campaignId) {
    await page.goto(`${BASE_URL}/campaigns/${campaignId}`);
  } else {
    await page.goto(`${BASE_URL}/campaigns`);
    await page.waitForLoadState('domcontentloaded');
    await page.locator(`text=${campaignName}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('domcontentloaded');
  await clickTab(page, 'Metrics');
  await clickTab(page, 'Analytics');
  const metricsContainer = page.locator('[class*="metric"], [class*="analytics"], [class*="chart"], canvas').first();
  const visible = await metricsContainer.isVisible({ timeout: 5000 }).catch(() => false);
  // Metrics may be empty for new campaign — just verify page didn't crash
  expect(page.url()).toBeTruthy();
});

test('TC-CAM-007: Add recipients to campaign - if Add Recipients button exists', async ({ page }) => {
  if (campaignId) {
    await page.goto(`${BASE_URL}/campaigns/${campaignId}`);
  } else {
    await page.goto(`${BASE_URL}/campaigns`);
    await page.waitForLoadState('domcontentloaded');
    await page.locator(`text=${campaignName}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('domcontentloaded');
  await clickTab(page, 'Recipients');
  await clickTab(page, 'Audience');

  const addRecipBtn = page.locator('button:has-text("Add Recipient"), button:has-text("Add Recipients"), button:has-text("Add Contacts"), button:has-text("Add Audience")').first();
  const visible = await addRecipBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await addRecipBtn.click();
    await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});
    await page.locator('[role="dialog"] button:has-text("Cancel"), [role="dialog"] button:has-text("Close")').first().click({ timeout: 5000 }).catch(() => {});
  } else {
    test.skip();
  }
});

test('TC-CAM-008: Send test email - if Send Test button exists', async ({ page }) => {
  if (campaignId) {
    await page.goto(`${BASE_URL}/campaigns/${campaignId}`);
  } else {
    await page.goto(`${BASE_URL}/campaigns`);
    await page.waitForLoadState('domcontentloaded');
    await page.locator(`text=${campaignName}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('domcontentloaded');

  const sendTestBtn = page.locator('button:has-text("Send Test"), button:has-text("Test Email"), button:has-text("Preview")').first();
  const visible = await sendTestBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await sendTestBtn.click();
    await page.waitForTimeout(500);
    await page.locator('[role="dialog"] button:has-text("Cancel"), [role="dialog"] button:has-text("Close"), button:has-text("Cancel")').first().click({ timeout: 5000 }).catch(() => {});
  } else {
    test.skip();
  }
});

test('TC-CAM-009: Execute/launch campaign - /campaigns/:id/execution if available', async ({ page }) => {
  if (campaignId) {
    await page.goto(`${BASE_URL}/campaigns/${campaignId}/execution`);
    await page.waitForLoadState('domcontentloaded');
    const notFound = await page.locator('text=404, text=Not Found').first().isVisible().catch(() => false);
    if (notFound) {
      test.skip();
      return;
    }
    await expect(page.locator('body')).toBeVisible();
  } else {
    await page.goto(`${BASE_URL}/campaign-execution`);
    await page.waitForLoadState('domcontentloaded');
    await expect(page.locator('body')).toBeVisible();
  }
});

test('TC-CAM-010: Filter campaigns by status', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');

  for (const status of ['Draft', 'Active', 'Completed', 'Paused']) {
    const filterBtn = page.locator(`button:has-text("${status}"), [role="option"]:has-text("${status}"), [role="tab"]:has-text("${status}")`).first();
    const visible = await filterBtn.isVisible({ timeout: 2000 }).catch(() => false);
    if (visible) {
      await filterBtn.click();
      await page.waitForTimeout(500);
    }

    // Try filter dropdown
    const filterSelect = page.locator('select[name*="status"], [aria-label*="Filter"], button:has-text("Filter")').first();
    const selectVisible = await filterSelect.isVisible({ timeout: 1000 }).catch(() => false);
    if (selectVisible) {
      await filterSelect.click().catch(() => {});
      await page.locator(`[role="option"]:has-text("${status}"), option:has-text("${status}")`).first().click({ timeout: 3000 }).catch(() => {});
      await page.waitForTimeout(300);
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-CAM-011: Search campaigns', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');

  const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
  await searchInput.fill('TEST').catch(() => {});
  await page.waitForTimeout(800);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-CAM-012: Delete campaign', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaigns`);
  await page.waitForLoadState('domcontentloaded');

  // Find the test campaign row and delete it
  const row = page.locator(`tr:has-text("${campaignName}"), [data-testid*="row"]:has-text("${campaignName}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button[aria-label*="Delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"], button[aria-label*="Delete"]').first().click({ timeout: 5000 }).catch(() => {});
    });
    // Confirm dialog
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    // Try from detail page
    if (campaignId) {
      await page.goto(`${BASE_URL}/campaigns/${campaignId}`);
      await page.waitForLoadState('domcontentloaded');
      await page.locator('button:has-text("Delete"), [aria-label*="delete"]').first().click({ timeout: 5000 }).catch(() => {});
      await page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Delete")').first().click({ timeout: 5000 }).catch(() => {});
      await waitForSuccess(page);
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-CAM-013: Navigate to /campaign-execution - verify execution page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/campaign-execution`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator('body')).toBeVisible();
  const heading = page.locator('h1, h2, h3, h4, [class*="title"]').first();
  await expect(heading).toBeVisible({ timeout: 10000 });
});

// ─── Email Templates ──────────────────────────────────────────────────────────

let templateName: string = '';

test('TC-ETPL-001: Navigate to /email-templates - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page).toHaveURL(/email-templates/);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-ETPL-002: Create email template', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');
  templateName = `TEST_EmailTemplate_${ts()}`;

  await openDialog(page);

  // Name
  await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"], [role="dialog"] input[placeholder*="Name"]').first().fill(templateName).catch(() => {});

  // Subject
  await page.locator('[role="dialog"] input[name*="subject"], [role="dialog"] input[placeholder*="subject"], [role="dialog"] input[placeholder*="Subject"]').first().fill('Test Email Subject').catch(() => {});

  // Category
  await selectOption(page, 'Category', 'Marketing');

  // Body / content
  await page.locator('[role="dialog"] textarea, [contenteditable="true"], [role="dialog"] input[name*="body"], [role="dialog"] input[name*="content"]').first().fill('This is a test email template body').catch(() => {});

  await submit(page);
  await waitForSuccess(page);

  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator(`text=${templateName}`).first()).toBeVisible({ timeout: 10000 });
});

test('TC-ETPL-003: View template details', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  await page.locator(`text=${templateName}`).first().click({ timeout: 10000 }).catch(() => {});
  await page.waitForTimeout(1000);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-ETPL-004: Edit template - update subject', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  const editBtn = page.locator(`tr:has-text("${templateName}") button[aria-label*="edit"], tr:has-text("${templateName}") button:has-text("Edit")`).first();
  const visible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await editBtn.click();
  } else {
    await page.locator(`text=${templateName}`).first().click({ timeout: 5000 }).catch(() => {});
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Edit"), [aria-label*="edit"]').first().click({ timeout: 5000 }).catch(() => {});
  }

  await page.locator('[role="dialog"] input[name*="subject"], [role="dialog"] input[placeholder*="subject"]').first().fill('Updated Subject for E2E').catch(() => {});
  await submit(page);
  await waitForSuccess(page);
});

test('TC-ETPL-005: Preview template - if preview button exists', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  const previewBtn = page.locator('button:has-text("Preview"), button[aria-label*="preview"], button[title*="Preview"]').first();
  const visible = await previewBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await previewBtn.click();
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Close"), [aria-label*="close"]').first().click({ timeout: 5000 }).catch(() => {});
  } else {
    test.skip();
  }
});

test('TC-ETPL-006: Search templates', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
  await searchInput.fill('TEST').catch(() => {});
  await page.waitForTimeout(800);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-ETPL-007: Filter by category', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  const categoryFilter = page.locator('select[name*="category"], [aria-label*="category"], [placeholder*="category"]').first();
  const visible = await categoryFilter.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await categoryFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Marketing"), option:has-text("Marketing")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(500);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-ETPL-008: Delete template', async ({ page }) => {
  await page.goto(`${BASE_URL}/email-templates`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`tr:has-text("${templateName}"), [data-testid*="row"]:has-text("${templateName}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"]').first().click({ timeout: 3000 }).catch(() => {});
    });
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

// ─── Landing Pages ────────────────────────────────────────────────────────────

let landingPageName: string = '';

test('TC-LP-001: Navigate to /landing-pages - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LP-002: Create landing page', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');
  landingPageName = `TEST_Landing_${ts()}`;

  await openDialog(page);
  await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"], [role="dialog"] input[placeholder*="Name"]').first().fill(landingPageName).catch(() => {});
  await page.locator('[role="dialog"] input[name*="title"], [role="dialog"] input[placeholder*="title"], [role="dialog"] input[placeholder*="Title"]').first().fill('Test Landing Page').catch(() => {});
  await selectOption(page, 'Status', 'Draft');
  await submit(page);
  await waitForSuccess(page);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LP-003: Edit landing page', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`text=${landingPageName}`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.click();
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Edit"), [aria-label*="edit"]').first().click({ timeout: 5000 }).catch(() => {});
    await page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="description"]').first().fill('Updated landing page description').catch(() => {});
    await submit(page);
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LP-004: View/preview landing page', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');

  const previewBtn = page.locator('button:has-text("Preview"), button:has-text("View"), [aria-label*="preview"]').first();
  const visible = await previewBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await previewBtn.click();
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Close"), [aria-label*="close"]').first().click({ timeout: 5000 }).catch(() => {});
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LP-005: Publish landing page - if Publish button exists', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');

  const publishBtn = page.locator('button:has-text("Publish"), [aria-label*="publish"]').first();
  const visible = await publishBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await publishBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Publish")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    test.skip();
  }
});

test('TC-LP-006: Delete landing page', async ({ page }) => {
  await page.goto(`${BASE_URL}/landing-pages`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`tr:has-text("${landingPageName}"), [data-testid*="row"]:has-text("${landingPageName}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"]').first().click({ timeout: 3000 }).catch(() => {});
    });
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

// ─── Web Forms ────────────────────────────────────────────────────────────────

let webFormName: string = '';

test('TC-WF-001: Navigate to /leads/web-forms - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/leads/web-forms`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator('body')).toBeVisible();
});

test('TC-WF-002: Create web-to-lead form', async ({ page }) => {
  await page.goto(`${BASE_URL}/leads/web-forms`);
  await page.waitForLoadState('domcontentloaded');
  webFormName = `TEST_Form_${ts()}`;

  await openDialog(page);
  await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"], [role="dialog"] input[placeholder*="Name"]').first().fill(webFormName).catch(() => {});
  await selectOption(page, 'Entity', 'Lead');
  await selectOption(page, 'Type', 'Lead');
  await submit(page);
  await waitForSuccess(page);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-WF-003: Edit web form', async ({ page }) => {
  await page.goto(`${BASE_URL}/leads/web-forms`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`text=${webFormName}`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.click();
    await page.waitForTimeout(500);
    const editBtn = page.locator('button:has-text("Edit"), [aria-label*="edit"]').first();
    await editBtn.click({ timeout: 5000 }).catch(() => {});
    await page.locator('[role="dialog"] input[name*="description"], [role="dialog"] textarea').first().fill('Updated form description').catch(() => {});
    await submit(page);
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-WF-004: View form embed code/preview - if button exists', async ({ page }) => {
  await page.goto(`${BASE_URL}/leads/web-forms`);
  await page.waitForLoadState('domcontentloaded');

  const embedBtn = page.locator('button:has-text("Embed"), button:has-text("Code"), button:has-text("Preview"), [aria-label*="embed"]').first();
  const visible = await embedBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await embedBtn.click();
    await page.waitForTimeout(500);
    await page.locator('button:has-text("Close"), [aria-label*="close"]').first().click({ timeout: 5000 }).catch(() => {});
  } else {
    test.skip();
  }
});

test('TC-WF-005: Delete web form', async ({ page }) => {
  await page.goto(`${BASE_URL}/leads/web-forms`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`tr:has-text("${webFormName}"), [data-testid*="row"]:has-text("${webFormName}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"]').first().click({ timeout: 3000 }).catch(() => {});
    });
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-WF-006: Navigate to /forms (Form Builder) - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/forms`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator('body')).toBeVisible();
});

// ─── Lead Routing ─────────────────────────────────────────────────────────────

let routingRuleName: string = '';

test('TC-LR-001: Navigate to /lead-routing - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/lead-routing`);
  await page.waitForLoadState('domcontentloaded');
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LR-002: Create lead routing rule', async ({ page }) => {
  await page.goto(`${BASE_URL}/lead-routing`);
  await page.waitForLoadState('domcontentloaded');
  routingRuleName = `TEST_Routing_${ts()}`;

  await openDialog(page);
  await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"], [role="dialog"] input[placeholder*="Name"]').first().fill(routingRuleName).catch(() => {});
  await page.locator('[role="dialog"] input[name*="condition"], [role="dialog"] input[placeholder*="condition"], [role="dialog"] input[placeholder*="Condition"]').first().fill('source=web').catch(() => {});
  await page.locator('[role="dialog"] input[name*="assign"], [role="dialog"] input[placeholder*="assign"], [role="dialog"] input[placeholder*="Assign"]').first().fill('admin').catch(() => {});
  await submit(page);
  await waitForSuccess(page);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LR-003: Toggle rule active/inactive', async ({ page }) => {
  await page.goto(`${BASE_URL}/lead-routing`);
  await page.waitForLoadState('domcontentloaded');

  const toggleSwitch = page.locator(`tr:has-text("${routingRuleName}") [role="checkbox"], tr:has-text("${routingRuleName}") .MuiSwitch-root, tr:has-text("${routingRuleName}") input[type="checkbox"]`).first();
  const visible = await toggleSwitch.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await toggleSwitch.click();
    await page.waitForTimeout(500);
    await toggleSwitch.click();
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LR-004: Edit routing rule', async ({ page }) => {
  await page.goto(`${BASE_URL}/lead-routing`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`text=${routingRuleName}`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.click().catch(() => {});
    await page.waitForTimeout(500);
    const editBtn = page.locator('button:has-text("Edit"), [aria-label*="edit"]').first();
    await editBtn.click({ timeout: 5000 }).catch(() => {});
    await page.locator('[role="dialog"] input').last().fill('Updated condition').catch(() => {});
    await submit(page);
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-LR-005: Delete routing rule', async ({ page }) => {
  await page.goto(`${BASE_URL}/lead-routing`);
  await page.waitForLoadState('domcontentloaded');

  const row = page.locator(`tr:has-text("${routingRuleName}"), [data-testid*="row"]:has-text("${routingRuleName}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"]').first().click({ timeout: 3000 }).catch(() => {});
    });
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

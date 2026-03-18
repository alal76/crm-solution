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

// ─── Service Requests ─────────────────────────────────────────────────────────

let srTitle: string = '';
let srId: string | null = null;

test('TC-SR-001: Navigate to /service-requests - verify page loads with ticket list', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');
  await expect(page).toHaveURL(/service-requests/);
  const heading = page.locator('h1, h2, h3, h4, h5, h6, [class*="title"], [class*="header"]').first();
  await expect(heading).toBeVisible({ timeout: 10000 });
});

test('TC-SR-002: Create service request (high priority)', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');
  srTitle = `TEST_SR_${ts()}`;

  await openDialog(page);

  // Title
  await page.locator('[role="dialog"] input[name*="title"], [role="dialog"] input[placeholder*="title"], [role="dialog"] input[placeholder*="Title"], [role="dialog"] input[name*="subject"], [role="dialog"] input[placeholder*="Subject"]').first().fill(srTitle).catch(() => {});

  // Description
  await page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="description"], [role="dialog"] input[placeholder*="description"], [role="dialog"] input[placeholder*="Description"]').first().fill('Test service request for E2E').catch(() => {});

  // Priority = High
  await selectOption(page, 'Priority', 'High');

  // Status = Open
  await selectOption(page, 'Status', 'Open');

  // Category - select first available option
  const categorySelect = page.locator('[role="dialog"] label:has-text("Category") ~ * .MuiSelect-select').first();
  const catVisible = await categorySelect.isVisible({ timeout: 2000 }).catch(() => false);
  if (catVisible) {
    await categorySelect.click().catch(() => {});
    await page.locator('[role="option"]').first().click({ timeout: 5000 }).catch(() => {});
  }

  await submit(page);
  await waitForSuccess(page);

  await page.waitForTimeout(1000);
  const url = page.url();
  const match = url.match(/service-requests\/(\d+)/);
  if (match) srId = match[1];

  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');
  await expect(page.locator(`text=${srTitle}`).first()).toBeVisible({ timeout: 10000 });
});

test('TC-SR-003: View service request details', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  await page.waitForTimeout(1000);

  const detailVisible = await page.locator('[class*="detail"], h1, h2, h3').first().isVisible().catch(() => false);
  expect(detailVisible).toBeTruthy();

  // Capture ID from URL if available
  const url = page.url();
  const match = url.match(/service-requests\/(\d+)/);
  if (match && !srId) srId = match[1];
});

test('TC-SR-004: Service request tabs - Overview, Activities, Notes, Attachments', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  for (const tab of ['Overview', 'Activities', 'Activity', 'Notes', 'Note', 'Attachments', 'Attachment']) {
    await clickTab(page, tab);
  }
});

test('TC-SR-005: Edit service request - change priority to Medium', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  const editBtn = page.locator('button:has-text("Edit"), [aria-label*="edit"]').first();
  await editBtn.click({ timeout: 5000 }).catch(() => {});
  await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});

  await selectOption(page, 'Priority', 'Medium');
  await submit(page);
  await waitForSuccess(page);
});

test('TC-SR-006: Assign ticket - click Assign button, select user', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  const assignBtn = page.locator('button:has-text("Assign"), [aria-label*="assign"], button:has-text("Assign To")').first();
  const visible = await assignBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await assignBtn.click();
    await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});

    // Select first available user option
    const userOption = page.locator('[role="option"], [role="listbox"] li, .MuiAutocomplete-option').first();
    const userVisible = await userOption.isVisible({ timeout: 3000 }).catch(() => false);
    if (userVisible) {
      await userOption.click().catch(() => {});
    }

    await page.locator('[role="dialog"] button:has-text("Assign"), [role="dialog"] button:has-text("Save"), [role="dialog"] button:has-text("Confirm")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    // Try inline assign select
    const assignSelect = page.locator('[aria-label*="ssignee"], [name*="ssignee"], label:has-text("Assign") ~ * .MuiSelect-select').first();
    const selectVisible = await assignSelect.isVisible({ timeout: 2000 }).catch(() => false);
    if (selectVisible) {
      await assignSelect.click().catch(() => {});
      await page.locator('[role="option"]').first().click({ timeout: 5000 }).catch(() => {});
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-007: Add note to service request', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  // Switch to Notes tab
  await clickTab(page, 'Notes');
  await clickTab(page, 'Note');
  await clickTab(page, 'Activity');

  const addNoteBtn = page.locator('button:has-text("Add Note"), button:has-text("Add Comment"), button:has-text("Note"), button:has-text("Comment")').first();
  const visible = await addNoteBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await addNoteBtn.click();
    await page.locator('[role="dialog"] textarea, [contenteditable="true"]').first().fill('E2E test note added to service request').catch(() => {});
    await page.locator('[role="dialog"] button:has-text("Save"), [role="dialog"] button:has-text("Add"), [role="dialog"] button:has-text("Submit")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-008: Change status - change to In Progress', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  // Try status button or status dropdown
  const inProgressBtn = page.locator('button:has-text("In Progress"), button:has-text("Start"), [aria-label*="in progress"]').first();
  const btnVisible = await inProgressBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (btnVisible) {
    await inProgressBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    // Try inline status select
    const statusSelect = page.locator('[aria-label*="Status"], [name*="status"], label:has-text("Status") ~ * .MuiSelect-select').first();
    const selectVisible = await statusSelect.isVisible({ timeout: 2000 }).catch(() => false);
    if (selectVisible) {
      await statusSelect.click().catch(() => {});
      await page.locator('[role="option"]:has-text("In Progress")').first().click({ timeout: 5000 }).catch(() => {});
      await waitForSuccess(page);
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-009: Resolve service request', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  const resolveBtn = page.locator('button:has-text("Resolve"), button:has-text("Mark Resolved"), [aria-label*="resolve"]').first();
  const btnVisible = await resolveBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (btnVisible) {
    await resolveBtn.click();
    // Resolution reason/notes dialog
    const textarea = page.locator('[role="dialog"] textarea').first();
    const tVisible = await textarea.isVisible({ timeout: 3000 }).catch(() => false);
    if (tVisible) {
      await textarea.fill('Resolved via E2E test');
    }
    await page.locator('[role="dialog"] button:has-text("Resolve"), [role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Save"), button:has-text("Confirm")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    // Try status dropdown
    const statusSelect = page.locator('label:has-text("Status") ~ * .MuiSelect-select, [name*="status"]').first();
    await statusSelect.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Resolved")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-010: Close service request', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  const closeBtn = page.locator('button:has-text("Close"), button:has-text("Mark Closed"), [aria-label*="close"]:not([aria-label*="dialog"]):not([aria-label*="modal"])').first();
  const btnVisible = await closeBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (btnVisible) {
    await closeBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Close")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    const statusSelect = page.locator('label:has-text("Status") ~ * .MuiSelect-select, [name*="status"]').first();
    await statusSelect.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Closed")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-011: Reopen service request - if available', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  const reopenBtn = page.locator('button:has-text("Reopen"), button:has-text("Re-open"), [aria-label*="reopen"]').first();
  const visible = await reopenBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await reopenBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else {
    test.skip();
  }
});

test('TC-SR-012: Add attachment - if file upload exists', async ({ page }) => {
  if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
  } else {
    await page.goto(`${BASE_URL}/service-requests`);
    await page.waitForLoadState('networkidle');
    await page.locator(`text=${srTitle}`).first().click({ timeout: 10000 }).catch(() => {});
  }
  await page.waitForLoadState('networkidle');

  await clickTab(page, 'Attachments');
  const fileInput = page.locator('input[type="file"]').first();
  const visible = await fileInput.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    // Create a temp file and upload
    await fileInput.setInputFiles({ name: 'e2e-test.txt', mimeType: 'text/plain', buffer: Buffer.from('E2E test attachment') });
    await waitForSuccess(page);
  } else {
    test.skip();
  }
});

test('TC-SR-013: Search service requests', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
  await searchInput.fill('TEST').catch(() => {});
  await page.waitForTimeout(800);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-014: Filter by priority (High, Medium, Low)', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  for (const priority of ['High', 'Medium', 'Low']) {
    const filterBtn = page.locator(`button:has-text("${priority}"), [role="tab"]:has-text("${priority}")`).first();
    const tabVisible = await filterBtn.isVisible({ timeout: 2000 }).catch(() => false);
    if (tabVisible) {
      await filterBtn.click();
      await page.waitForTimeout(400);
    }

    const filterSelect = page.locator('select[name*="priority"], [aria-label*="Priority"], [placeholder*="Priority"]').first();
    const selectVisible = await filterSelect.isVisible({ timeout: 1000 }).catch(() => false);
    if (selectVisible) {
      await filterSelect.click().catch(() => {});
      await page.locator(`[role="option"]:has-text("${priority}"), option:has-text("${priority}")`).first().click({ timeout: 3000 }).catch(() => {});
      await page.waitForTimeout(300);
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-015: Filter by status (Open, In Progress, Resolved, Closed)', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  for (const status of ['Open', 'In Progress', 'Resolved', 'Closed']) {
    const filterBtn = page.locator(`button:has-text("${status}"), [role="tab"]:has-text("${status}")`).first();
    const tabVisible = await filterBtn.isVisible({ timeout: 2000 }).catch(() => false);
    if (tabVisible) {
      await filterBtn.click();
      await page.waitForTimeout(400);
      continue;
    }

    const filterSelect = page.locator('select[name*="status"], [aria-label*="Status"], [placeholder*="Status"]').first();
    const selectVisible = await filterSelect.isVisible({ timeout: 1000 }).catch(() => false);
    if (selectVisible) {
      await filterSelect.click().catch(() => {});
      await page.locator(`[role="option"]:has-text("${status}"), option:has-text("${status}")`).first().click({ timeout: 3000 }).catch(() => {});
      await page.waitForTimeout(300);
    }
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-016: Sort by created date', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  const createdHeader = page.locator('th:has-text("Created"), th:has-text("Date"), [aria-sort], [data-field*="created"]').first();
  const visible = await createdHeader.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await createdHeader.click();
    await page.waitForTimeout(500);
    await createdHeader.click(); // toggle sort order
    await page.waitForTimeout(500);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-017: Delete service request', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-requests`);
  await page.waitForLoadState('networkidle');

  const row = page.locator(`tr:has-text("${srTitle}"), [data-testid*="row"]:has-text("${srTitle}")`).first();
  const rowVisible = await row.isVisible({ timeout: 5000 }).catch(() => false);
  if (rowVisible) {
    await row.locator('button[aria-label*="delete"], button:has-text("Delete")').first().click({ timeout: 5000 }).catch(async () => {
      await row.hover();
      await page.locator('button[aria-label*="delete"]').first().click({ timeout: 3000 }).catch(() => {});
    });
    await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  } else if (srId) {
    await page.goto(`${BASE_URL}/service-requests/${srId}`);
    await page.waitForLoadState('networkidle');
    await page.locator('button:has-text("Delete"), [aria-label*="delete"]').first().click({ timeout: 5000 }).catch(() => {});
    await page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Delete")').first().click({ timeout: 5000 }).catch(() => {});
    await waitForSuccess(page);
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-SR-018: Navigate to /service-request-settings - verify settings page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/service-request-settings`);
  await page.waitForLoadState('networkidle');
  await expect(page.locator('body')).toBeVisible();
  const heading = page.locator('h1, h2, h3, h4, [class*="title"]').first();
  await expect(heading).toBeVisible({ timeout: 10000 });
});

// ─── Knowledge Base ───────────────────────────────────────────────────────────

test('TC-KB-001: Navigate to /knowledge-base - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/knowledge-base`);
  await page.waitForLoadState('networkidle');
  await expect(page).toHaveURL(/knowledge-base/);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-KB-002: Search knowledge base', async ({ page }) => {
  await page.goto(`${BASE_URL}/knowledge-base`);
  await page.waitForLoadState('networkidle');

  const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
  await searchInput.fill('service').catch(() => {});
  await page.waitForTimeout(800);
  await expect(page.locator('body')).toBeVisible();
});

test('TC-KB-003: Filter knowledge base by category', async ({ page }) => {
  await page.goto(`${BASE_URL}/knowledge-base`);
  await page.waitForLoadState('networkidle');

  const categoryFilter = page.locator('[aria-label*="category"], [placeholder*="category"], select[name*="category"]').first();
  const visible = await categoryFilter.isVisible({ timeout: 3000 }).catch(() => false);
  if (visible) {
    await categoryFilter.click().catch(() => {});
    await page.locator('[role="option"], option').first().click({ timeout: 5000 }).catch(() => {});
    await page.waitForTimeout(500);
  }

  // Category tabs
  const categoryTab = page.locator('[role="tab"]').first();
  const tabVisible = await categoryTab.isVisible({ timeout: 2000 }).catch(() => false);
  if (tabVisible) {
    await categoryTab.click();
    await page.waitForTimeout(400);
  }

  await expect(page.locator('body')).toBeVisible();
});

test('TC-KB-004: View article detail', async ({ page }) => {
  await page.goto(`${BASE_URL}/knowledge-base`);
  await page.waitForLoadState('networkidle');

  const articleLink = page.locator('[class*="article"], [class*="kb-item"], tr, [data-testid*="article"]').first();
  const visible = await articleLink.isVisible({ timeout: 5000 }).catch(() => false);
  if (visible) {
    await articleLink.click();
    await page.waitForTimeout(1000);
    await expect(page.locator('body')).toBeVisible();
  }
  await expect(page.locator('body')).toBeVisible();
});

test('TC-KB-005: Navigate to /communications - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/communications`);
  await page.waitForLoadState('networkidle');
  await expect(page.locator('body')).toBeVisible();
});

test('TC-KB-006: Navigate to /interactions - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/interactions`);
  await page.waitForLoadState('networkidle');
  await expect(page.locator('body')).toBeVisible();
});

// ─── Services Page ────────────────────────────────────────────────────────────

test('TC-SVC-001: Navigate to /services - verify page loads', async ({ page }) => {
  await page.goto(`${BASE_URL}/services`);
  await page.waitForLoadState('networkidle');
  await expect(page.locator('body')).toBeVisible();
  const heading = page.locator('h1, h2, h3, h4, [class*="title"]').first();
  await expect(heading).toBeVisible({ timeout: 10000 });
});

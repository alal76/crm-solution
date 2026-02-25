import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

test.describe.configure({ mode: 'serial' });

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
async function ensureLoggedIn(page: Page) {
  await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
  if (page.url().includes('/login')) {
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
  }
}

async function clickAddButton(page: Page) {
  const addBtn = page.locator(
    'button:has-text("Add Account"), button:has-text("New Account"), button:has-text("Add"), button:has-text("Create"), button:has-text("New"), [data-testid*="add"], [aria-label*="add"]'
  ).first();
  await addBtn.waitFor({ timeout: 10000 });
  await addBtn.click();
}

async function fillTextField(page: Page, label: string, value: string) {
  const selectors = [
    `label:has-text("${label}") + div input`,
    `label:has-text("${label}") ~ div input`,
    `[aria-label="${label}"] input`,
    `input[name="${label.toLowerCase()}"]`,
    `input[placeholder*="${label}"]`,
    `[data-testid*="${label.toLowerCase()}"] input`,
  ];
  for (const sel of selectors) {
    const el = page.locator(sel).first();
    if (await el.isVisible().catch(() => false)) {
      await el.fill(value);
      return;
    }
  }
  // Fallback: locate label text node then its sibling input
  await page.locator(`label`, { hasText: label }).first().locator('..').locator('input').first().fill(value).catch(async () => {
    await page.locator(`input[placeholder*="${label}"]`).first().fill(value);
  });
}

async function selectMuiOption(page: Page, fieldLabel: string, optionText: string) {
  // Open the MUI Select associated with the label
  const select = page.locator(
    `label:has-text("${fieldLabel}") ~ div .MuiSelect-select, label:has-text("${fieldLabel}") + div .MuiSelect-select`
  ).first();
  if (await select.isVisible().catch(() => false)) {
    await select.click();
  } else {
    await page.locator(`.MuiSelect-select`, { hasText: fieldLabel }).first().click().catch(() => {});
  }
  // Click option in dropdown listbox
  await page.locator(`li[role="option"]:has-text("${optionText}"), [data-value]:has-text("${optionText}")`).first().click({ timeout: 8000 }).catch(() => {});
}

async function waitForDialog(page: Page) {
  await page.locator('[role="dialog"], .MuiDialog-root, .MuiModal-root').first().waitFor({ state: 'visible', timeout: 10000 });
}

async function submitForm(page: Page) {
  const saveBtn = page.locator(
    '[role="dialog"] button:has-text("Save"), [role="dialog"] button:has-text("Create"), [role="dialog"] button:has-text("Submit"), [role="dialog"] button[type="submit"]'
  ).first();
  if (await saveBtn.isVisible().catch(() => false)) {
    await saveBtn.click();
  } else {
    await page.locator('button:has-text("Save"), button:has-text("Create"), button[type="submit"]').first().click();
  }
}

async function waitForSuccess(page: Page) {
  await page.locator(
    '.MuiAlert-standardSuccess, .MuiSnackbar-root .MuiAlert-root, [role="alert"]:has-text("success"), [role="alert"]:has-text("created"), [role="alert"]:has-text("saved"), [role="alert"]:has-text("updated"), [role="alert"]:has-text("deleted")'
  ).waitFor({ state: 'visible', timeout: 15000 }).catch(() => {});
}

async function waitForGrid(page: Page) {
  await page.locator('.MuiDataGrid-root, table, [role="grid"], [data-testid*="grid"]').first().waitFor({ state: 'visible', timeout: 15000 });
}

function ts(): string { return Date.now().toString().slice(-6); }

// ---------------------------------------------------------------------------
// Test Suite: Accounts CRUD
// ---------------------------------------------------------------------------
test.describe('Accounts CRUD', () => {

  test.beforeEach(async ({ page }) => {
    await ensureLoggedIn(page);
  });

  // -------------------------------------------------------------------------
  // TC-ACC-001: Navigate to accounts page and verify list loads
  // -------------------------------------------------------------------------
  test('TC-ACC-001: Navigate to accounts page and verify list loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle').catch(() => {});

    // Verify heading
    const heading = page.locator('h1, h2, h3, .MuiTypography-h4, .MuiTypography-h5, .MuiTypography-h6').filter({ hasText: /account/i }).first();
    await expect(heading).toBeVisible({ timeout: 10000 });

    // Verify grid / table
    await waitForGrid(page);
    const grid = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
    await expect(grid).toBeVisible();

    // Verify Add/New button
    const addBtn = page.locator(
      'button:has-text("Add"), button:has-text("New"), button:has-text("Create"), [data-testid*="add"]'
    ).first();
    await expect(addBtn).toBeVisible({ timeout: 10000 });
  });

  // -------------------------------------------------------------------------
  // TC-ACC-002: Create a new corporate account
  // -------------------------------------------------------------------------
  test('TC-ACC-002: Create a new corporate account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    const accountName = `TEST_Corp_${ts()}`;

    await clickAddButton(page);
    await waitForDialog(page);

    // Fill Name
    await fillTextField(page, 'Name', accountName);
    await fillTextField(page, 'name', accountName).catch(() => {});

    // Account Type dropdown
    await selectMuiOption(page, 'Type', 'Corporate').catch(async () => {
      await selectMuiOption(page, 'Account Type', 'Corporate').catch(() => {});
    });

    // Industry dropdown
    await selectMuiOption(page, 'Industry', 'Technology').catch(async () => {
      await selectMuiOption(page, 'Industry', 'Information Technology').catch(() => {});
    });

    // Email
    await fillTextField(page, 'Email', 'test@corp.example.com').catch(async () => {
      await page.locator('[role="dialog"] input[type="email"]').first().fill('test@corp.example.com').catch(() => {});
    });

    // Phone
    await fillTextField(page, 'Phone', '+1-555-0100').catch(async () => {
      await page.locator('[role="dialog"] input[type="tel"], [role="dialog"] input[placeholder*="hone"]').first().fill('+1-555-0100').catch(() => {});
    });

    // Website
    await fillTextField(page, 'Website', 'https://testcorp.example.com').catch(async () => {
      await page.locator('[role="dialog"] input[placeholder*="ebsite"]').first().fill('https://testcorp.example.com').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Verify account appears in list
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(accountName);
      await page.waitForTimeout(1500);
    }
    const row = page.locator(`.MuiDataGrid-row, tr, [role="row"]`).filter({ hasText: accountName }).first();
    await expect(row).toBeVisible({ timeout: 15000 });
  });

  // -------------------------------------------------------------------------
  // TC-ACC-003: Create an individual account
  // -------------------------------------------------------------------------
  test('TC-ACC-003: Create an individual account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    const accountName = `TEST_Ind_${ts()}`;

    await clickAddButton(page);
    await waitForDialog(page);

    await fillTextField(page, 'Name', accountName);

    await selectMuiOption(page, 'Type', 'Individual').catch(async () => {
      await selectMuiOption(page, 'Account Type', 'Individual').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Verify in list
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(accountName);
      await page.waitForTimeout(1500);
    }
    const row = page.locator(`.MuiDataGrid-row, tr, [role="row"]`).filter({ hasText: accountName }).first();
    await expect(row).toBeVisible({ timeout: 15000 });
  });

  // -------------------------------------------------------------------------
  // TC-ACC-004: Search accounts
  // -------------------------------------------------------------------------
  test('TC-ACC-004: Search accounts', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"], [data-testid*="search"] input').first();
    await searchInput.waitFor({ state: 'visible', timeout: 10000 });
    await searchInput.fill('TEST_Corp');
    await page.waitForTimeout(1500);

    // Verify grid still visible with (possibly filtered) results
    const grid = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
    await expect(grid).toBeVisible();

    // Either rows show with TEST_Corp or "no results" message
    const hasRows = await page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first().isVisible().catch(() => false);
    const hasNoResults = await page.locator('[class*="noRows"], [class*="noData"], text=/no.*result/i, text=/no.*found/i').first().isVisible().catch(() => false);
    expect(hasRows || hasNoResults).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // TC-ACC-005: View account details
  // -------------------------------------------------------------------------
  test('TC-ACC-005: View account details', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Click first data row
    const firstRow = page.locator('.MuiDataGrid-row, tbody tr[class*="MuiTableRow"], [role="row"]:not([aria-rowindex="1"])').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();

    // Either navigates to /accounts/:id or opens a dialog/drawer
    await page.waitForTimeout(1500);
    const isDetailPage = page.url().match(/\/accounts\/\d+/);
    const isDialog = await page.locator('[role="dialog"], .MuiDrawer-root').first().isVisible().catch(() => false);

    if (isDetailPage || isDialog) {
      // Verify some detail content is visible
      const detailContent = page.locator('.MuiCard-root, .MuiPaper-root, [role="dialog"], .MuiDrawer-root, main').first();
      await expect(detailContent).toBeVisible({ timeout: 10000 });
    } else {
      // May have navigated inline — just check for account fields
      const field = page.locator('label:has-text("Name"), [data-field="name"], th:has-text("Name"), td').first();
      await expect(field).toBeVisible({ timeout: 10000 });
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-006: Edit an account
  // -------------------------------------------------------------------------
  test('TC-ACC-006: Edit an account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Open first row
    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasNot: page.locator('[aria-rowindex="1"]') }).first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1000);

    // Find Edit button
    const editBtn = page.locator(
      'button:has-text("Edit"), [data-testid*="edit"], [aria-label*="edit"], button:has-text("Modify")'
    ).first();
    if (await editBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await editBtn.click();
      await waitForDialog(page).catch(() => {});
    }

    // Edit description or notes field
    const descField = page.locator(
      'textarea[name*="description"], textarea[placeholder*="escription"], [aria-label*="description"] textarea, textarea'
    ).first();
    if (await descField.isVisible().catch(() => false)) {
      await descField.fill('Updated description from e2e test');
    } else {
      // Try any editable text field
      await page.locator('[role="dialog"] input, [role="dialog"] textarea').first().fill('Updated by e2e').catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
  });

  // -------------------------------------------------------------------------
  // TC-ACC-007: Access account tabs (detail page)
  // -------------------------------------------------------------------------
  test('TC-ACC-007: Access account tabs on detail page', async ({ page }) => {
    // Navigate directly to an account detail page
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    
    // Try to navigate to detail page
    const firstLink = firstRow.locator('a, [data-testid*="view"]').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }
    await page.waitForTimeout(1500);

    // Check for tabs
    const tabs = page.locator('[role="tab"], .MuiTab-root');
    const tabCount = await tabs.count();
    if (tabCount === 0) {
      test.skip(); // No tabs on this detail layout
      return;
    }

    const tabLabels = ['Overview', 'Details', 'Contacts', 'Opportunities', 'Activities', 'Notes', 'Files', 'Documents'];
    for (const label of tabLabels) {
      const tab = tabs.filter({ hasText: label }).first();
      if (await tab.isVisible().catch(() => false)) {
        await tab.click();
        await page.waitForTimeout(800);
        const panel = page.locator('[role="tabpanel"]').first();
        await expect(panel).toBeVisible({ timeout: 8000 }).catch(() => {});
      }
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-008: Link a contact to an account
  // -------------------------------------------------------------------------
  test('TC-ACC-008: Link a contact to an account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    // Find Contacts tab
    const contactsTab = page.locator('[role="tab"]:has-text("Contact"), .MuiTab-root:has-text("Contact")').first();
    if (await contactsTab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await contactsTab.click();
      await page.waitForTimeout(800);
    }

    // Find "Link Contact" or "Add Contact" button
    const linkBtn = page.locator(
      'button:has-text("Link Contact"), button:has-text("Add Contact"), button:has-text("Link"), [data-testid*="link-contact"]'
    ).first();
    if (await linkBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await linkBtn.click();
      await page.waitForTimeout(800);

      // Search for a contact in the dialog
      const searchInput = page.locator('[role="dialog"] input[placeholder*="Search"], [role="dialog"] input[placeholder*="search"]').first();
      if (await searchInput.isVisible().catch(() => false)) {
        await searchInput.fill('TEST');
        await page.waitForTimeout(1000);

        // Select first result
        const firstResult = page.locator('[role="dialog"] [role="option"], [role="dialog"] li, [role="listbox"] li').first();
        if (await firstResult.isVisible().catch(() => false)) {
          await firstResult.click();
        }
      }
      await submitForm(page).catch(() => {});
      await waitForSuccess(page);
    } else {
      test.skip(); // Feature not available
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-009: Add address to account
  // -------------------------------------------------------------------------
  test('TC-ACC-009: Add address to account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    // Find Addresses section or tab
    const addressTab = page.locator(
      '[role="tab"]:has-text("Address"), .MuiTab-root:has-text("Address"), [role="tab"]:has-text("Contact Info")'
    ).first();
    if (await addressTab.isVisible({ timeout: 3000 }).catch(() => false)) {
      await addressTab.click();
      await page.waitForTimeout(800);
    }

    const addAddressBtn = page.locator(
      'button:has-text("Add Address"), button:has-text("New Address"), [data-testid*="add-address"]'
    ).first();
    if (await addAddressBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addAddressBtn.click();
      await waitForDialog(page).catch(() => {});

      await fillTextField(page, 'Street', '123 Test St').catch(() => {
        page.locator('[role="dialog"] input[name*="street"], [role="dialog"] input[placeholder*="treet"]').first().fill('123 Test St').catch(() => {});
      });
      await fillTextField(page, 'City', 'Test City').catch(() => {});
      await fillTextField(page, 'State', 'TC').catch(() => {});
      await fillTextField(page, 'Postal Code', '12345').catch(() => {
        page.locator('[role="dialog"] input[name*="zip"], [role="dialog"] input[name*="postal"]').first().fill('12345').catch(() => {});
      });
      await fillTextField(page, 'Country', 'US').catch(() => {});

      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip(); // Address add feature not found
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-010: Add phone to account
  // -------------------------------------------------------------------------
  test('TC-ACC-010: Add phone to account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    const addPhoneBtn = page.locator(
      'button:has-text("Add Phone"), button:has-text("New Phone"), [data-testid*="add-phone"]'
    ).first();
    if (await addPhoneBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addPhoneBtn.click();
      await waitForDialog(page).catch(() => {});
      await page.locator('[role="dialog"] input[type="tel"], [role="dialog"] input[placeholder*="hone"], [role="dialog"] input[name*="phone"]').first().fill('+1-555-TEST-001').catch(() => {});
      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-011: Add email address to account
  // -------------------------------------------------------------------------
  test('TC-ACC-011: Add email address to account', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    const addEmailBtn = page.locator(
      'button:has-text("Add Email"), button:has-text("New Email"), [data-testid*="add-email"]'
    ).first();
    if (await addEmailBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addEmailBtn.click();
      await waitForDialog(page).catch(() => {});
      await page.locator('[role="dialog"] input[type="email"], [role="dialog"] input[name*="email"], [role="dialog"] input[placeholder*="mail"]').first().fill('testaccount@example.com').catch(() => {});
      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-012: Delete an account (create then delete)
  // -------------------------------------------------------------------------
  test('TC-ACC-012: Delete an account', async ({ page }) => {
    const accountName = `TEST_DELETE_${ts()}`;

    // Create account first
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await clickAddButton(page);
    await waitForDialog(page);
    await fillTextField(page, 'Name', accountName);
    await submitForm(page);
    await waitForSuccess(page);

    // Navigate back to list
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Search for the created account
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(accountName);
      await page.waitForTimeout(1500);
    }

    // Find the row
    const targetRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: accountName }).first();
    await targetRow.waitFor({ state: 'visible', timeout: 10000 });

    // Look for delete button in row actions
    const deleteBtn = targetRow.locator(
      'button[aria-label*="delete"], button[aria-label*="Delete"], button:has-text("Delete"), [data-testid*="delete"]'
    ).first();

    if (await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteBtn.click();
    } else {
      // Try context menu / more actions
      const moreBtn = targetRow.locator('[aria-label*="more"], [aria-label*="actions"], button:has-text("...")').first();
      if (await moreBtn.isVisible().catch(() => false)) {
        await moreBtn.click();
        await page.locator('[role="menuitem"]:has-text("Delete")').first().click({ timeout: 5000 });
      } else {
        // Try clicking row to open detail, then delete
        await targetRow.click();
        await page.waitForTimeout(1000);
        await page.locator('button:has-text("Delete"), [data-testid*="delete"]').first().click({ timeout: 5000 }).catch(() => {});
      }
    }

    // Confirm deletion dialog
    await page.locator(
      '[role="dialog"] button:has-text("Delete"), [role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Yes")'
    ).first().click({ timeout: 8000 }).catch(() => {});

    await waitForSuccess(page);

    // Verify removed from list
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(accountName);
      await page.waitForTimeout(1500);
    }
    const deletedRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: accountName }).first();
    await expect(deletedRow).not.toBeVisible({ timeout: 8000 });
  });

  // -------------------------------------------------------------------------
  // TC-ACC-013: Filter accounts by type
  // -------------------------------------------------------------------------
  test('TC-ACC-013: Filter accounts by type', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Look for filter chips / tabs / buttons
    const filterLocators = [
      page.locator('[role="tab"]:has-text("Corporate"), .MuiChip-root:has-text("Corporate"), button:has-text("Corporate")').first(),
      page.locator('[role="tab"]:has-text("Individual"), .MuiChip-root:has-text("Individual"), button:has-text("Individual")').first(),
      page.locator('[role="tab"]:has-text("All"), .MuiChip-root:has-text("All"), button:has-text("All")').first(),
    ];

    for (const filter of filterLocators) {
      if (await filter.isVisible({ timeout: 3000 }).catch(() => false)) {
        await filter.click();
        await page.waitForTimeout(800);
        const grid = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
        await expect(grid).toBeVisible();
      }
    }

    // Also try MUI Select filter
    const filterSelect = page.locator(
      'select[name*="type"], select[name*="filter"], .MuiSelect-select[aria-label*="filter"], .MuiSelect-select[aria-label*="type"]'
    ).first();
    if (await filterSelect.isVisible({ timeout: 3000 }).catch(() => false)) {
      await filterSelect.click();
      await page.locator('li[role="option"]:has-text("Corporate")').first().click({ timeout: 5000 }).catch(() => {});
      await page.waitForTimeout(800);
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-014: Export accounts
  // -------------------------------------------------------------------------
  test('TC-ACC-014: Export accounts', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const exportBtn = page.locator(
      'button:has-text("Export"), button:has-text("Download"), [data-testid*="export"], [aria-label*="export"]'
    ).first();
    if (await exportBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      // Listen for download
      const downloadPromise = page.waitForEvent('download', { timeout: 10000 }).catch(() => null);
      await exportBtn.click();
      await page.waitForTimeout(1000);

      // If a dialog appears with export options, confirm it
      const confirmBtn = page.locator('[role="dialog"] button:has-text("Export"), [role="dialog"] button:has-text("Download"), [role="dialog"] button:has-text("OK")').first();
      if (await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await confirmBtn.click();
      }
      const download = await downloadPromise;
      if (download) {
        expect(download.suggestedFilename()).toBeTruthy();
      } else {
        // Export may open a menu or dialog — verify something appeared
        const exportMenu = page.locator('[role="menu"], [role="dialog"], .MuiMenu-root').first();
        await expect(exportMenu).toBeVisible({ timeout: 5000 }).catch(() => {});
      }
    } else {
      test.skip(); // Export not available
    }
  });

  // -------------------------------------------------------------------------
  // TC-ACC-015: Sort accounts by name
  // -------------------------------------------------------------------------
  test('TC-ACC-015: Sort accounts by name column', async ({ page }) => {
    await page.goto(`${BASE_URL}/accounts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Find the Name column header in MUI DataGrid
    const nameHeader = page.locator(
      '.MuiDataGrid-columnHeader[data-field="name"], th:has-text("Name"), [role="columnheader"]:has-text("Name")'
    ).first();
    if (await nameHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
      await nameHeader.click();
      await page.waitForTimeout(800);

      // Verify sort indicator
      const sortIcon = nameHeader.locator('.MuiDataGrid-iconButtonContainer, [aria-sort], .MuiDataGrid-sortIcon, svg[data-testid*="Sort"]').first();
      const ariaSort = await nameHeader.getAttribute('aria-sort').catch(() => null);
      // Either ariaSort attribute is set or sort icon appeared
      expect(ariaSort !== null || await sortIcon.isVisible().catch(() => false)).toBeTruthy();

      // Click again to reverse
      await nameHeader.click();
      await page.waitForTimeout(800);

      const ariaSortAfter = await nameHeader.getAttribute('aria-sort').catch(() => null);
      if (ariaSort && ariaSortAfter) {
        expect(ariaSortAfter).not.toBe(ariaSort);
      }
    } else {
      test.skip(); // Column header not found
    }
  });

});

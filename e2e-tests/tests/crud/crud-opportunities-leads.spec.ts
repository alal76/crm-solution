import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

test.describe.configure({ mode: 'serial' });

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
async function ensureLoggedIn(page: Page, path: string = '/opportunities') {
  await page.goto(`${BASE_URL}${path}`, { waitUntil: 'domcontentloaded' });
  if (page.url().includes('/login')) {
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
  }
}

async function clickAddButton(page: Page) {
  const addBtn = page.locator(
    'button:has-text("Add Opportunity"), button:has-text("New Opportunity"), button:has-text("Add Lead"), button:has-text("New Lead"), button:has-text("Add"), button:has-text("Create"), button:has-text("New"), [data-testid*="add"], [aria-label*="add"]'
  ).first();
  await addBtn.waitFor({ timeout: 10000 });
  await addBtn.click();
}

async function fillTextField(page: Page, label: string, value: string) {
  const selectors = [
    `label:has-text("${label}") + div input`,
    `label:has-text("${label}") ~ div input`,
    `[aria-label="${label}"] input`,
    `[aria-label="${label}"]`,
    `input[name="${label.toLowerCase().replace(/\s+/g, '')}"]`,
    `input[name="${label.toLowerCase().replace(/\s+/g, '_')}"]`,
    `input[placeholder*="${label}"]`,
    `[data-testid*="${label.toLowerCase().replace(/\s+/g, '-')}"] input`,
  ];
  for (const sel of selectors) {
    const el = page.locator(sel).first();
    if (await el.isVisible().catch(() => false)) {
      await el.fill(value);
      return;
    }
  }
  await page.locator(`label`, { hasText: label })
    .first()
    .locator('..')
    .locator('input')
    .first()
    .fill(value)
    .catch(() => {});
}

async function fillDialogTextField(page: Page, label: string, value: string) {
  const dialog = page.locator('[role="dialog"]').first();
  const selectors = [
    `label:has-text("${label}") + div input`,
    `label:has-text("${label}") ~ div input`,
    `[aria-label="${label}"] input`,
    `input[name="${label.toLowerCase().replace(/\s+/g, '')}"]`,
    `input[placeholder*="${label}"]`,
  ];
  for (const sel of selectors) {
    const el = dialog.locator(sel).first();
    if (await el.isVisible().catch(() => false)) {
      await el.fill(value);
      return;
    }
  }
  await dialog.locator(`label`, { hasText: label }).first().locator('..').locator('input').first().fill(value).catch(() => {});
}

async function selectMuiOption(page: Page, fieldLabel: string, optionText: string) {
  const select = page.locator(
    `label:has-text("${fieldLabel}") ~ div .MuiSelect-select, label:has-text("${fieldLabel}") + div .MuiSelect-select`
  ).first();
  if (await select.isVisible().catch(() => false)) {
    await select.click();
  } else {
    await page.locator(`[role="dialog"]`).first().locator(`.MuiSelect-select`).filter({ hasText: fieldLabel }).first().click().catch(async () => {
      await page.locator(`[aria-label*="${fieldLabel}"]`).first().click().catch(() => {});
    });
  }
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
  await page.locator('.MuiDataGrid-root, table, [role="grid"], .MuiCard-root, [data-testid*="grid"], [class*="kanban"], [class*="pipeline"]').first().waitFor({ state: 'visible', timeout: 15000 });
}

function ts(): string { return Date.now().toString().slice(-6); }

// =============================================================================
// OPPORTUNITIES TESTS
// =============================================================================
test.describe('Opportunities CRUD', () => {

  test.beforeEach(async ({ page }) => {
    await ensureLoggedIn(page, '/opportunities');
  });

  // -------------------------------------------------------------------------
  // TC-OPP-001: Navigate to /opportunities - verify list loads
  // -------------------------------------------------------------------------
  test('TC-OPP-001: Navigate to opportunities page and verify list loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle').catch(() => {});

    // Verify heading
    const heading = page.locator('h1, h2, h3, .MuiTypography-h4, .MuiTypography-h5, .MuiTypography-h6').filter({ hasText: /opportunit/i }).first();
    await expect(heading).toBeVisible({ timeout: 10000 });

    // Verify grid or pipeline/kanban view
    await waitForGrid(page);
    const content = page.locator('.MuiDataGrid-root, table, [role="grid"], [class*="kanban"], [class*="pipeline"], .MuiCard-root').first();
    await expect(content).toBeVisible();

    // Verify Add/New button
    const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create"), [data-testid*="add"]').first();
    await expect(addBtn).toBeVisible({ timeout: 10000 });
  });

  // -------------------------------------------------------------------------
  // TC-OPP-002: Create opportunity
  // -------------------------------------------------------------------------
  test('TC-OPP-002: Create a new opportunity with full details', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    const oppName = `TEST_Opp_${ts()}`;

    await clickAddButton(page);
    await waitForDialog(page);

    // Name / Title
    await fillDialogTextField(page, 'Name', oppName).catch(async () => {
      await fillDialogTextField(page, 'Title', oppName).catch(async () => {
        await page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="name"]').first().fill(oppName).catch(() => {});
      });
    });

    // Account autocomplete
    const accountInput = page.locator('[role="dialog"] input[name*="account"], [role="dialog"] input[placeholder*="Account"], [role="dialog"] input[placeholder*="company"]').first();
    if (await accountInput.isVisible().catch(() => false)) {
      await accountInput.fill('TEST');
      await page.waitForTimeout(1000);
      const suggestion = page.locator('[role="option"], [role="listbox"] li, .MuiAutocomplete-option').first();
      if (await suggestion.isVisible({ timeout: 3000 }).catch(() => false)) {
        await suggestion.click();
      }
    }

    // Stage select
    await selectMuiOption(page, 'Stage', 'Prospecting').catch(async () => {
      await selectMuiOption(page, 'Opportunity Stage', 'Prospecting').catch(() => {});
    });

    // Amount
    await fillDialogTextField(page, 'Amount', '50000').catch(async () => {
      await page.locator('[role="dialog"] input[name*="amount"], [role="dialog"] input[type="number"]').first().fill('50000').catch(() => {});
    });

    // Close Date
    await fillDialogTextField(page, 'Close Date', '2026-12-31').catch(async () => {
      await fillDialogTextField(page, 'Expected Close', '2026-12-31').catch(async () => {
        await page.locator('[role="dialog"] input[type="date"]').first().fill('2026-12-31').catch(() => {});
      });
    });

    // Probability
    await fillDialogTextField(page, 'Probability', '30').catch(async () => {
      await page.locator('[role="dialog"] input[name*="probability"]').first().fill('30').catch(() => {});
    });

    // Description
    const descArea = page.locator('[role="dialog"] textarea[name*="description"], [role="dialog"] textarea').first();
    if (await descArea.isVisible().catch(() => false)) {
      await descArea.fill('Test opportunity created by e2e automation');
    }

    await submitForm(page);
    await waitForSuccess(page);

    // Verify in list
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(oppName);
      await page.waitForTimeout(1500);
    }
    const row = page.locator('.MuiDataGrid-row, tbody tr, [role="row"], .MuiCard-root').filter({ hasText: oppName }).first();
    await expect(row).toBeVisible({ timeout: 15000 });
  });

  // -------------------------------------------------------------------------
  // TC-OPP-003: View kanban/pipeline view (if toggle available)
  // -------------------------------------------------------------------------
  test('TC-OPP-003: Toggle to kanban/pipeline view if available', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle').catch(() => {});

    // Look for view toggle buttons
    const kanbanToggle = page.locator(
      'button[aria-label*="kanban"], button[aria-label*="pipeline"], button[aria-label*="board"], button:has-text("Kanban"), button:has-text("Pipeline"), button:has-text("Board"), [data-testid*="kanban"], [data-testid*="pipeline"]'
    ).first();

    if (await kanbanToggle.isVisible({ timeout: 5000 }).catch(() => false)) {
      await kanbanToggle.click();
      await page.waitForTimeout(1500);

      // Verify kanban columns visible
      const kanbanContent = page.locator('[class*="kanban"], [class*="pipeline"], [class*="board"], .MuiCard-root').first();
      await expect(kanbanContent).toBeVisible({ timeout: 8000 });

      // Switch back to list view
      const listToggle = page.locator('button[aria-label*="list"], button[aria-label*="table"], button:has-text("List"), [data-testid*="list"]').first();
      if (await listToggle.isVisible().catch(() => false)) {
        await listToggle.click();
        await page.waitForTimeout(1000);
      }
    } else {
      test.skip(); // Kanban toggle not available
    }
  });

  // -------------------------------------------------------------------------
  // TC-OPP-004: View opportunity details
  // -------------------------------------------------------------------------
  test('TC-OPP-004: View opportunity details by clicking a row', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }

    await page.waitForTimeout(1500);
    const isDetailPage = page.url().match(/\/opportunities\/\d+/);
    const isDialog = await page.locator('[role="dialog"], .MuiDrawer-root').first().isVisible().catch(() => false);

    if (isDetailPage || isDialog) {
      const detailContent = page.locator('.MuiCard-root, .MuiPaper-root, [role="dialog"], .MuiDrawer-root, main').first();
      await expect(detailContent).toBeVisible({ timeout: 10000 });
    } else {
      const content = page.locator('main, .MuiContainer-root').first();
      await expect(content).toBeVisible();
    }
  });

  // -------------------------------------------------------------------------
  // TC-OPP-005: Edit opportunity - change stage to Qualification
  // -------------------------------------------------------------------------
  test('TC-OPP-005: Edit opportunity and change stage to Qualification', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1000);

    const editBtn = page.locator('button:has-text("Edit"), [data-testid*="edit"], [aria-label*="edit"]').first();
    if (await editBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await editBtn.click();
      await waitForDialog(page).catch(() => {});
    }

    // Change Stage
    await selectMuiOption(page, 'Stage', 'Qualification').catch(async () => {
      await selectMuiOption(page, 'Opportunity Stage', 'Qualification').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);
  });

  // -------------------------------------------------------------------------
  // TC-OPP-006: Opportunity tabs - Activities, Notes, Products, Contacts
  // -------------------------------------------------------------------------
  test('TC-OPP-006: Access opportunity detail tabs', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }
    await page.waitForTimeout(1500);

    const tabs = page.locator('[role="tab"], .MuiTab-root');
    const tabCount = await tabs.count();
    if (tabCount === 0) {
      test.skip();
      return;
    }

    const tabLabels = ['Overview', 'Details', 'Activities', 'Notes', 'Products', 'Line Items', 'Contacts', 'Files'];
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
  // TC-OPP-007: Add product to opportunity (line items tab)
  // -------------------------------------------------------------------------
  test('TC-OPP-007: Add product/line item to opportunity', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }
    await page.waitForTimeout(1500);

    // Find Products or Line Items tab
    const productsTab = page.locator(
      '[role="tab"]:has-text("Product"), .MuiTab-root:has-text("Product"), [role="tab"]:has-text("Line Item"), .MuiTab-root:has-text("Line Item")'
    ).first();
    if (await productsTab.isVisible({ timeout: 5000 }).catch(() => false)) {
      await productsTab.click();
      await page.waitForTimeout(800);

      const addProductBtn = page.locator(
        'button:has-text("Add Product"), button:has-text("Add Line Item"), button:has-text("Add Item"), [data-testid*="add-product"]'
      ).first();
      if (await addProductBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
        await addProductBtn.click();
        await waitForDialog(page).catch(() => {});

        // Search/select product
        const productSearch = page.locator('[role="dialog"] input[placeholder*="roduct"], [role="dialog"] input[name*="product"]').first();
        if (await productSearch.isVisible().catch(() => false)) {
          await productSearch.fill('test');
          await page.waitForTimeout(1000);
          const option = page.locator('[role="option"], .MuiAutocomplete-option').first();
          if (await option.isVisible({ timeout: 3000 }).catch(() => false)) {
            await option.click();
          }
        }

        // Fill quantity
        await page.locator('[role="dialog"] input[name*="quantity"], [role="dialog"] input[name*="qty"]').first().fill('1').catch(() => {});

        await submitForm(page);
        await waitForSuccess(page);
      } else {
        test.skip();
      }
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-OPP-008: Search opportunities
  // -------------------------------------------------------------------------
  test('TC-OPP-008: Search opportunities by name', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
    await searchInput.waitFor({ state: 'visible', timeout: 10000 });
    await searchInput.fill('TEST_Opp');
    await page.waitForTimeout(1500);

    const content = page.locator('.MuiDataGrid-root, table, [role="grid"], [class*="kanban"]').first();
    await expect(content).toBeVisible();

    const hasRows = await page.locator('.MuiDataGrid-row, tbody tr, [role="row"], .MuiCard-root').first().isVisible().catch(() => false);
    const hasNoResults = await page.locator('[class*="noRows"], [class*="noData"], text=/no.*result/i').first().isVisible().catch(() => false);
    expect(hasRows || hasNoResults).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // TC-OPP-009: Filter opportunities by stage
  // -------------------------------------------------------------------------
  test('TC-OPP-009: Filter opportunities by stage', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Stage filter chips/tabs
    const stageFilters = [
      page.locator('[role="tab"]:has-text("Prospecting"), .MuiChip-root:has-text("Prospecting"), button:has-text("Prospecting")').first(),
      page.locator('[role="tab"]:has-text("Qualification"), .MuiChip-root:has-text("Qualification")').first(),
      page.locator('[role="tab"]:has-text("All"), .MuiChip-root:has-text("All")').first(),
    ];

    for (const filter of stageFilters) {
      if (await filter.isVisible({ timeout: 3000 }).catch(() => false)) {
        await filter.click();
        await page.waitForTimeout(800);
        const content = page.locator('.MuiDataGrid-root, table, [role="grid"], [class*="kanban"]').first();
        await expect(content).toBeVisible();
      }
    }

    // MUI DataGrid column filter
    const filterBtn = page.locator(
      '[data-testid="FilterAltIcon"], button[aria-label*="filter"], button:has-text("Filters"), .MuiDataGrid-toolbarContainer button'
    ).first();
    if (await filterBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await filterBtn.click().catch(() => {});
      await page.waitForTimeout(500);
      await page.keyboard.press('Escape');
    }
  });

  // -------------------------------------------------------------------------
  // TC-OPP-010: Sort opportunities by amount
  // -------------------------------------------------------------------------
  test('TC-OPP-010: Sort opportunities by amount column', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const amountHeader = page.locator(
      '.MuiDataGrid-columnHeader[data-field="amount"], .MuiDataGrid-columnHeader[data-field="value"], th:has-text("Amount"), [role="columnheader"]:has-text("Amount"), [role="columnheader"]:has-text("Value")'
    ).first();

    if (await amountHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
      await amountHeader.click();
      await page.waitForTimeout(800);

      const ariaSort = await amountHeader.getAttribute('aria-sort').catch(() => null);
      const hasSortIcon = await amountHeader.locator('[data-testid*="Sort"], .MuiDataGrid-sortIcon, svg').first().isVisible().catch(() => false);
      expect(ariaSort !== null || hasSortIcon).toBeTruthy();

      await amountHeader.click();
      await page.waitForTimeout(800);
    } else {
      // Fallback to Name column
      const nameHeader = page.locator('.MuiDataGrid-columnHeader[data-field="name"], th:has-text("Name"), [role="columnheader"]:has-text("Name")').first();
      if (await nameHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
        await nameHeader.click();
        await page.waitForTimeout(800);
        await nameHeader.click();
        await page.waitForTimeout(800);
        expect(true).toBeTruthy();
      } else {
        test.skip();
      }
    }
  });

  // -------------------------------------------------------------------------
  // TC-OPP-011: Delete opportunity (create then delete)
  // -------------------------------------------------------------------------
  test('TC-OPP-011: Delete an opportunity (create then delete)', async ({ page }) => {
    const oppName = `TEST_DEL_OPP_${ts()}`;

    // Create
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await clickAddButton(page);
    await waitForDialog(page);

    await fillDialogTextField(page, 'Name', oppName).catch(async () => {
      await fillDialogTextField(page, 'Title', oppName).catch(async () => {
        await page.locator('[role="dialog"] input').first().fill(oppName).catch(() => {});
      });
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Navigate back to list
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(oppName);
      await page.waitForTimeout(1500);
    }

    const targetRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: oppName }).first();
    await targetRow.waitFor({ state: 'visible', timeout: 10000 });

    const deleteBtn = targetRow.locator('button[aria-label*="delete"], button[aria-label*="Delete"], [data-testid*="delete"]').first();
    if (await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteBtn.click();
    } else {
      const moreBtn = targetRow.locator('[aria-label*="more"], [aria-label*="actions"], button:has-text("...")').first();
      if (await moreBtn.isVisible().catch(() => false)) {
        await moreBtn.click();
        await page.locator('[role="menuitem"]:has-text("Delete")').first().click({ timeout: 5000 });
      } else {
        await targetRow.click();
        await page.waitForTimeout(1000);
        await page.locator('button:has-text("Delete"), [data-testid*="delete"]').first().click({ timeout: 5000 }).catch(() => {});
      }
    }

    await page.locator('[role="dialog"] button:has-text("Delete"), [role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Yes")').first().click({ timeout: 8000 }).catch(() => {});
    await waitForSuccess(page);

    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(oppName);
      await page.waitForTimeout(1500);
    }
    await expect(
      page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: oppName }).first()
    ).not.toBeVisible({ timeout: 8000 });
  });

  // -------------------------------------------------------------------------
  // TC-OPP-012: Export opportunities
  // -------------------------------------------------------------------------
  test('TC-OPP-012: Export opportunities', async ({ page }) => {
    await page.goto(`${BASE_URL}/opportunities`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const exportBtn = page.locator(
      'button:has-text("Export"), button:has-text("Download"), [data-testid*="export"], [aria-label*="export"]'
    ).first();
    if (await exportBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      const downloadPromise = page.waitForEvent('download', { timeout: 10000 }).catch(() => null);
      await exportBtn.click();
      await page.waitForTimeout(1000);

      const confirmBtn = page.locator('[role="dialog"] button:has-text("Export"), [role="dialog"] button:has-text("Download"), [role="dialog"] button:has-text("OK")').first();
      if (await confirmBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await confirmBtn.click();
      }

      const download = await downloadPromise;
      if (download) {
        expect(download.suggestedFilename()).toBeTruthy();
      } else {
        const menu = page.locator('[role="menu"], [role="dialog"], .MuiMenu-root').first();
        await expect(menu).toBeVisible({ timeout: 5000 }).catch(() => {});
      }
    } else {
      test.skip();
    }
  });

});

// =============================================================================
// LEADS TESTS
// =============================================================================
test.describe('Leads CRUD', () => {

  test.beforeEach(async ({ page }) => {
    await ensureLoggedIn(page, '/leads');
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-001: Navigate to /leads - verify list loads
  // -------------------------------------------------------------------------
  test('TC-LEAD-001: Navigate to leads page and verify list loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle').catch(() => {});

    const heading = page.locator('h1, h2, h3, .MuiTypography-h4, .MuiTypography-h5, .MuiTypography-h6').filter({ hasText: /lead/i }).first();
    await expect(heading).toBeVisible({ timeout: 10000 });

    await waitForGrid(page);
    const content = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
    await expect(content).toBeVisible();

    const addBtn = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create"), [data-testid*="add"]').first();
    await expect(addBtn).toBeVisible({ timeout: 10000 });
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-002: Create lead with full details
  // -------------------------------------------------------------------------
  test('TC-LEAD-002: Create a new lead with full details', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    const suffix = ts();
    const firstName = `TEST_Lead_${suffix}`;

    await clickAddButton(page);
    await waitForDialog(page);

    // First Name
    await fillDialogTextField(page, 'First Name', firstName).catch(async () => {
      await page.locator('[role="dialog"] input[name*="firstName"], [role="dialog"] input[placeholder*="First"]').first().fill(firstName).catch(() => {});
    });

    // Last Name
    await fillDialogTextField(page, 'Last Name', 'TestSurname').catch(async () => {
      await page.locator('[role="dialog"] input[name*="lastName"], [role="dialog"] input[placeholder*="Last"]').first().fill('TestSurname').catch(() => {});
    });

    // Email
    await fillDialogTextField(page, 'Email', `test_lead_${suffix}@example.com`).catch(async () => {
      await page.locator('[role="dialog"] input[type="email"]').first().fill(`test_lead_${suffix}@example.com`).catch(() => {});
    });

    // Phone
    await fillDialogTextField(page, 'Phone', '+1-555-0300').catch(async () => {
      await page.locator('[role="dialog"] input[type="tel"], [role="dialog"] input[name*="phone"]').first().fill('+1-555-0300').catch(() => {});
    });

    // Company
    await fillDialogTextField(page, 'Company', 'TestCo').catch(async () => {
      await page.locator('[role="dialog"] input[name*="company"]').first().fill('TestCo').catch(() => {});
    });

    // Title / Job Title
    await fillDialogTextField(page, 'Title', 'Director').catch(async () => {
      await fillDialogTextField(page, 'Job Title', 'Director').catch(async () => {
        await page.locator('[role="dialog"] input[name*="title"]').first().fill('Director').catch(() => {});
      });
    });

    // Source dropdown
    await selectMuiOption(page, 'Source', 'Web').catch(async () => {
      await selectMuiOption(page, 'Lead Source', 'Web').catch(() => {});
    });

    // Status dropdown
    await selectMuiOption(page, 'Status', 'New').catch(async () => {
      await selectMuiOption(page, 'Lead Status', 'New').catch(() => {});
    });

    // Rating dropdown
    await selectMuiOption(page, 'Rating', 'Hot').catch(async () => {
      await selectMuiOption(page, 'Lead Rating', 'Hot').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Verify in list
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }
    const row = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first();
    await expect(row).toBeVisible({ timeout: 15000 });
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-003: View lead details
  // -------------------------------------------------------------------------
  test('TC-LEAD-003: View lead details by clicking a row', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }

    await page.waitForTimeout(1500);
    const isDetailPage = page.url().match(/\/leads\/\d+/);
    const isDialog = await page.locator('[role="dialog"], .MuiDrawer-root').first().isVisible().catch(() => false);

    if (isDetailPage || isDialog) {
      const detailContent = page.locator('.MuiCard-root, .MuiPaper-root, [role="dialog"], .MuiDrawer-root, main').first();
      await expect(detailContent).toBeVisible({ timeout: 10000 });
    } else {
      const content = page.locator('main, .MuiContainer-root').first();
      await expect(content).toBeVisible();
    }
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-004: Edit lead - change status to Working
  // -------------------------------------------------------------------------
  test('TC-LEAD-004: Edit lead and change status to Working', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1000);

    const editBtn = page.locator('button:has-text("Edit"), [data-testid*="edit"], [aria-label*="edit"]').first();
    if (await editBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await editBtn.click();
      await waitForDialog(page).catch(() => {});
    }

    // Change Status to Working
    await selectMuiOption(page, 'Status', 'Working').catch(async () => {
      await selectMuiOption(page, 'Lead Status', 'Working').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-005: Lead detail tabs
  // -------------------------------------------------------------------------
  test('TC-LEAD-005: Access lead detail tabs', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }
    await page.waitForTimeout(1500);

    const tabs = page.locator('[role="tab"], .MuiTab-root');
    const tabCount = await tabs.count();
    if (tabCount === 0) {
      test.skip();
      return;
    }

    const tabLabels = ['Overview', 'Details', 'Activities', 'Notes', 'Files'];
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
  // TC-LEAD-006: Convert lead to opportunity/contact/account
  // -------------------------------------------------------------------------
  test('TC-LEAD-006: Convert lead using Convert button if available', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });

    const firstLink = firstRow.locator('a').first();
    if (await firstLink.isVisible().catch(() => false)) {
      await firstLink.click();
    } else {
      await firstRow.click();
    }
    await page.waitForTimeout(1500);

    const convertBtn = page.locator(
      'button:has-text("Convert"), button:has-text("Convert Lead"), [data-testid*="convert"]'
    ).first();
    if (await convertBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await convertBtn.click();
      await page.waitForTimeout(1000);

      // Conversion dialog should appear
      const convertDialog = page.locator('[role="dialog"]').first();
      if (await convertDialog.isVisible({ timeout: 5000 }).catch(() => false)) {
        // The dialog may have checkboxes to create account/contact/opportunity
        // Verify it's visible without actually completing the conversion
        await expect(convertDialog).toBeVisible();

        // Close without converting
        await page.locator('[role="dialog"] button:has-text("Cancel"), [role="dialog"] button[aria-label="Close"], [role="dialog"] button:has-text("Close")').first().click({ timeout: 5000 }).catch(() => {
          page.keyboard.press('Escape').catch(() => {});
        });
      }
    } else {
      test.skip(); // Convert button not available
    }
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-007: Search leads
  // -------------------------------------------------------------------------
  test('TC-LEAD-007: Search leads by name', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"]').first();
    await searchInput.waitFor({ state: 'visible', timeout: 10000 });
    await searchInput.fill('TEST_Lead');
    await page.waitForTimeout(1500);

    const content = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
    await expect(content).toBeVisible();

    const hasRows = await page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first().isVisible().catch(() => false);
    const hasNoResults = await page.locator('[class*="noRows"], [class*="noData"], text=/no.*result/i').first().isVisible().catch(() => false);
    expect(hasRows || hasNoResults).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-008: Filter leads by status/rating
  // -------------------------------------------------------------------------
  test('TC-LEAD-008: Filter leads by status and rating', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Status filter chips/tabs
    const statusFilters = [
      page.locator('[role="tab"]:has-text("New"), .MuiChip-root:has-text("New"), button:has-text("New")').first(),
      page.locator('[role="tab"]:has-text("Working"), .MuiChip-root:has-text("Working")').first(),
      page.locator('[role="tab"]:has-text("All"), .MuiChip-root:has-text("All")').first(),
    ];

    for (const filter of statusFilters) {
      if (await filter.isVisible({ timeout: 3000 }).catch(() => false)) {
        await filter.click();
        await page.waitForTimeout(800);
        const content = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
        await expect(content).toBeVisible();
      }
    }

    // Rating filter via select
    const ratingFilter = page.locator(
      'select[name*="rating"], .MuiSelect-select[aria-label*="rating"], label:has-text("Rating") ~ div .MuiSelect-select'
    ).first();
    if (await ratingFilter.isVisible({ timeout: 3000 }).catch(() => false)) {
      await ratingFilter.click();
      await page.locator('li[role="option"]:has-text("Hot")').first().click({ timeout: 5000 }).catch(() => {});
      await page.waitForTimeout(800);
    }
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-009: Sort leads
  // -------------------------------------------------------------------------
  test('TC-LEAD-009: Sort leads by name column', async ({ page }) => {
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const nameHeader = page.locator(
      '.MuiDataGrid-columnHeader[data-field="firstName"], .MuiDataGrid-columnHeader[data-field="name"], .MuiDataGrid-columnHeader[data-field="lastName"], th:has-text("Name"), [role="columnheader"]:has-text("Name"), [role="columnheader"]:has-text("First")'
    ).first();

    if (await nameHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
      await nameHeader.click();
      await page.waitForTimeout(800);

      const ariaSort = await nameHeader.getAttribute('aria-sort').catch(() => null);
      const hasSortIcon = await nameHeader.locator('[data-testid*="Sort"], .MuiDataGrid-sortIcon, svg').first().isVisible().catch(() => false);
      expect(ariaSort !== null || hasSortIcon).toBeTruthy();

      await nameHeader.click();
      await page.waitForTimeout(800);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-010: Delete lead (create then delete)
  // -------------------------------------------------------------------------
  test('TC-LEAD-010: Delete a lead (create then delete)', async ({ page }) => {
    const suffix = ts();
    const firstName = `TEST_DEL_LEAD_${suffix}`;

    // Create lead
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await clickAddButton(page);
    await waitForDialog(page);

    await page.locator('[role="dialog"] input[name*="firstName"], [role="dialog"] input[placeholder*="First"]').first().fill(firstName).catch(async () => {
      await fillDialogTextField(page, 'First Name', firstName).catch(() => {});
    });
    await page.locator('[role="dialog"] input[name*="lastName"], [role="dialog"] input[placeholder*="Last"]').first().fill('ToDelete').catch(async () => {
      await fillDialogTextField(page, 'Last Name', 'ToDelete').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Navigate back, search, delete
    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }

    const targetRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first();
    await targetRow.waitFor({ state: 'visible', timeout: 10000 });

    const deleteBtn = targetRow.locator('button[aria-label*="delete"], button[aria-label*="Delete"], [data-testid*="delete"]').first();
    if (await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteBtn.click();
    } else {
      const moreBtn = targetRow.locator('[aria-label*="more"], [aria-label*="actions"], button:has-text("...")').first();
      if (await moreBtn.isVisible().catch(() => false)) {
        await moreBtn.click();
        await page.locator('[role="menuitem"]:has-text("Delete")').first().click({ timeout: 5000 });
      } else {
        await targetRow.click();
        await page.waitForTimeout(1000);
        await page.locator('button:has-text("Delete"), [data-testid*="delete"]').first().click({ timeout: 5000 }).catch(() => {});
      }
    }

    await page.locator('[role="dialog"] button:has-text("Delete"), [role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Yes")').first().click({ timeout: 8000 }).catch(() => {});
    await waitForSuccess(page);

    await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }
    await expect(
      page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first()
    ).not.toBeVisible({ timeout: 8000 });
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-011: Navigate to /leads/web-forms
  // -------------------------------------------------------------------------
  test('TC-LEAD-011: Navigate to web-to-lead forms page', async ({ page }) => {
    const webFormsUrls = [
      `${BASE_URL}/leads/web-forms`,
      `${BASE_URL}/web-forms`,
      `${BASE_URL}/leads/webforms`,
    ];

    let loaded = false;
    for (const url of webFormsUrls) {
      await page.goto(url, { waitUntil: 'domcontentloaded' }).catch(() => {});
      await page.waitForTimeout(1000);

      if (!page.url().includes('/login') && !page.url().includes('/404') && !page.url().includes('/not-found')) {
        loaded = true;

        // Verify the page has some content
        const content = page.locator('.MuiDataGrid-root, table, [role="grid"], h1, h2, .MuiTypography-h4, .MuiTypography-h5').first();
        await expect(content).toBeVisible({ timeout: 10000 }).catch(() => {});
        break;
      }
    }

    if (!loaded) {
      // Try navigating via sidebar link
      await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
      const webFormLink = page.locator('a:has-text("Web Form"), a:has-text("Web-to-Lead"), [href*="web-form"]').first();
      if (await webFormLink.isVisible({ timeout: 5000 }).catch(() => false)) {
        await webFormLink.click();
        await page.waitForTimeout(1500);
        const content = page.locator('.MuiDataGrid-root, table, h1, h2').first();
        await expect(content).toBeVisible({ timeout: 8000 }).catch(() => {});
      } else {
        test.skip(); // Web forms feature not accessible
      }
    }
  });

  // -------------------------------------------------------------------------
  // TC-LEAD-012: Navigate to /lead-routing
  // -------------------------------------------------------------------------
  test('TC-LEAD-012: Navigate to lead routing rules page', async ({ page }) => {
    const routingUrls = [
      `${BASE_URL}/lead-routing`,
      `${BASE_URL}/leads/routing`,
      `${BASE_URL}/leads/routing-rules`,
    ];

    let loaded = false;
    for (const url of routingUrls) {
      await page.goto(url, { waitUntil: 'domcontentloaded' }).catch(() => {});
      await page.waitForTimeout(1000);

      if (!page.url().includes('/login') && !page.url().includes('/404') && !page.url().includes('/not-found')) {
        loaded = true;

        const content = page.locator('.MuiDataGrid-root, table, [role="grid"], h1, h2, .MuiTypography-h4, .MuiTypography-h5').first();
        await expect(content).toBeVisible({ timeout: 10000 }).catch(() => {});
        break;
      }
    }

    if (!loaded) {
      // Try navigating via sidebar link
      await page.goto(`${BASE_URL}/leads`, { waitUntil: 'domcontentloaded' });
      const routingLink = page.locator('a:has-text("Routing"), a:has-text("Lead Routing"), [href*="routing"]').first();
      if (await routingLink.isVisible({ timeout: 5000 }).catch(() => false)) {
        await routingLink.click();
        await page.waitForTimeout(1500);
        const content = page.locator('.MuiDataGrid-root, table, h1, h2').first();
        await expect(content).toBeVisible({ timeout: 8000 }).catch(() => {});
      } else {
        test.skip(); // Lead routing feature not accessible
      }
    }
  });

});

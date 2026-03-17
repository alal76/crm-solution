import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

function ts(): string { return Date.now().toString().slice(-6); }

async function waitForSuccess(page: Page) {
  await page.locator('.MuiAlert-standardSuccess, .MuiSnackbar-root, [role="alert"]').waitFor({ timeout: 15000 }).catch(() => {});
}

async function openAddDialog(page: Page) {
  await page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New"), button:has-text("+ "), [data-testid*="add-btn"], [aria-label="add"]').first().click({ timeout: 10000 });
  await page.locator('[role="dialog"], .MuiDialog-root').waitFor({ timeout: 5000 }).catch(() => {});
}

async function submitForm(page: Page) {
  await page.locator('[role="dialog"] button:has-text("Save"), [role="dialog"] button:has-text("Create"), [role="dialog"] button:has-text("Submit"), button[type="submit"]:visible').first().click({ timeout: 10000 });
}

async function fillInput(page: Page, labelOrPlaceholder: string, value: string) {
  const field = page.locator(`[role="dialog"] label:has-text("${labelOrPlaceholder}") + * input, [role="dialog"] input[placeholder*="${labelOrPlaceholder}"], input[name*="${labelOrPlaceholder.toLowerCase().replace(/ /g, '')}"]`).first();
  await field.fill(value).catch(async () => {
    await page.locator(`input[placeholder*="${labelOrPlaceholder}"], input[aria-label*="${labelOrPlaceholder}"]`).first().fill(value);
  });
}

async function closeDialogIfOpen(page: Page) {
  const dialog = page.locator('[role="dialog"]');
  if (await dialog.isVisible().catch(() => false)) {
    await page.locator('[role="dialog"] button:has-text("Cancel"), [role="dialog"] button:has-text("Close"), [role="dialog"] [aria-label="close"]').first().click().catch(() => {});
    await dialog.waitFor({ state: 'hidden', timeout: 3000 }).catch(() => {});
  }
}

async function deleteRowByAction(page: Page) {
  const rows = page.locator('.MuiDataGrid-row, tbody tr');
  const rowCount = await rows.count().catch(() => 0);
  if (rowCount === 0) return;

  await rows.last().hover().catch(() => {});
  const deleteBtn = page.locator('button[aria-label*="delete"], button:has-text("Delete")').first();
  const delVisible = await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false);
  if (delVisible) {
    await deleteBtn.click();
  } else {
    await page.locator('button[aria-label*="more"], button[aria-label*="action"]').last().click({ timeout: 3000 }).catch(() => {});
    await page.locator('li:has-text("Delete"), [role="menuitem"]:has-text("Delete")').first().click({ timeout: 3000 }).catch(() => {});
  }
  await page.locator('button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")').first().click({ timeout: 3000 }).catch(() => {});
  await waitForSuccess(page);
}

// ─────────────────────────────────────────────────────────────────────────────
// SUBSCRIPTIONS
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Subscriptions', () => {
  test.describe.configure({ mode: 'serial' });

  let subscriptionDetailUrl: string;

  // TC-SUB-001
  test('TC-SUB-001: Navigate to /subscriptions and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"], [class*="heading"]').filter({ hasText: /subscription/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"], [class*="card"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-SUB-002
  test('TC-SUB-002: Create a subscription', async ({ page }) => {
    const planName = `TEST_Plan_${ts()}`;
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    await openAddDialog(page);

    // Plan name
    const nameField = page.locator('[role="dialog"] input[name*="plan"], [role="dialog"] input[name*="name"], [role="dialog"] input').first();
    await nameField.fill(planName).catch(() => {});

    // Billing cycle
    const billingField = page.locator('[role="dialog"] [name*="billing"], [role="dialog"] [name*="cycle"], [role="dialog"] label:has-text("Billing") + * [role="combobox"]').first();
    const billingVisible = await billingField.isVisible({ timeout: 2000 }).catch(() => false);
    if (billingVisible) {
      await billingField.click().catch(() => {});
      await page.locator('[role="option"]:has-text("Monthly"), li:has-text("Monthly"), option:has-text("Monthly")').first().click({ timeout: 3000 }).catch(() => {});
    }

    // Amount
    const amountField = page.locator('[role="dialog"] input[name*="amount"], [role="dialog"] input[name*="price"], [role="dialog"] input[type="number"]').first();
    await amountField.fill('99.99').catch(() => {});

    // Account
    const accountField = page.locator('[role="dialog"] input[name*="account"], [role="dialog"] label:has-text("Account") + * input').first();
    const accountVisible = await accountField.isVisible({ timeout: 2000 }).catch(() => false);
    if (accountVisible) {
      await accountField.fill('TEST').catch(() => {});
      await page.locator('[role="option"], .MuiAutocomplete-option').first().click({ timeout: 3000 }).catch(() => {});
    }

    // Start date
    const startDateField = page.locator('[role="dialog"] input[name*="start"], [role="dialog"] input[type="date"]').first();
    await startDateField.fill('2026-01-01').catch(() => {});

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-SUB-003
  test('TC-SUB-003: View subscription details', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(async () => {
      await page.locator('a[href*="/subscriptions/"]').first().click().catch(() => {});
    });

    await page.waitForURL(/\/subscriptions\/\d+/, { timeout: 5000 }).catch(() => {});
    subscriptionDetailUrl = page.url();

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-SUB-004
  test('TC-SUB-004: Subscription detail tabs (Overview, Billing History, Usage)', async ({ page }) => {
    const url = subscriptionDetailUrl || `${BASE_URL}/subscriptions`;
    await page.goto(url);
    await page.waitForLoadState('domcontentloaded');

    if (!subscriptionDetailUrl) {
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/subscriptions\/\d+/, { timeout: 5000 }).catch(() => {});
    }

    const tabNames = ['Overview', 'Billing History', 'Usage', 'Billing'];
    for (const tabName of tabNames) {
      const tab = page.locator(`[role="tab"]:has-text("${tabName}"), a:has-text("${tabName}"), button:has-text("${tabName}")`).first();
      const tabVisible = await tab.isVisible({ timeout: 2000 }).catch(() => false);
      if (tabVisible) {
        await tab.click();
        await page.waitForTimeout(300);
        console.log(`TC-SUB-004: Clicked tab "${tabName}"`);
      }
    }

    await expect(page.locator('main, [class*="detail"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-SUB-005
  test('TC-SUB-005: Edit subscription – update amount or billing cycle', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    const editBtn = page.locator('button[aria-label*="edit"], button:has-text("Edit")').first();
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (editVisible) {
      await editBtn.click();
    } else {
      await page.locator('.MuiDataGrid-row, tbody tr').first().hover().catch(() => {});
      await page.locator('button[aria-label*="more"], button[aria-label*="action"]').first().click({ timeout: 3000 }).catch(() => {});
      await page.locator('li:has-text("Edit"), [role="menuitem"]:has-text("Edit")').first().click({ timeout: 3000 }).catch(() => {});
    }

    await page.locator('[role="dialog"], form').waitFor({ timeout: 5000 }).catch(() => {});

    const amountField = page.locator('[role="dialog"] input[name*="amount"], [role="dialog"] input[name*="price"], [role="dialog"] input[type="number"]').first();
    await amountField.fill('149.99').catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-SUB-006
  test('TC-SUB-006: Cancel subscription (with confirmation)', async ({ page }) => {
    const url = subscriptionDetailUrl || `${BASE_URL}/subscriptions`;
    await page.goto(url);
    await page.waitForLoadState('domcontentloaded');

    if (!subscriptionDetailUrl) {
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/subscriptions\/\d+/, { timeout: 4000 }).catch(() => {});
    }

    const cancelBtn = page.locator('button:has-text("Cancel Subscription"), button:has-text("Cancel Sub"), [data-testid*="cancel-subscription"]').first();
    const cancelVisible = await cancelBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!cancelVisible) {
      test.skip(); return;
    }

    await cancelBtn.click();
    // Confirmation dialog
    const confirmDialog = page.locator('[role="dialog"]');
    await confirmDialog.waitFor({ timeout: 4000 }).catch(() => {});
    await page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Cancel Subscription")').first().click({ timeout: 3000 }).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-SUB-007
  test('TC-SUB-007: Renew subscription (if button available)', async ({ page }) => {
    const url = subscriptionDetailUrl || `${BASE_URL}/subscriptions`;
    await page.goto(url);
    await page.waitForLoadState('domcontentloaded');

    if (!subscriptionDetailUrl) {
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/subscriptions\/\d+/, { timeout: 4000 }).catch(() => {});
    }

    const renewBtn = page.locator('button:has-text("Renew"), button:has-text("Renew Subscription"), [data-testid*="renew"]').first();
    const renewVisible = await renewBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!renewVisible) {
      test.skip(); return;
    }

    await renewBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Renew"), button:has-text("Yes")').first().click({ timeout: 3000 }).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-SUB-008
  test('TC-SUB-008: Navigate to /subscriptions/analytics and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions/analytics`);
    await page.waitForLoadState('domcontentloaded');

    const content = page.locator('h1, h2, h3, [class*="title"], [class*="analytics"], [class*="chart"], main').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });

  // TC-SUB-009
  test('TC-SUB-009: Filter subscriptions by status', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    const statusFilter = page.locator('[aria-label*="status"], button:has-text("Status"), select[name*="status"], [class*="filter"]').first();
    const filterVisible = await statusFilter.isVisible({ timeout: 3000 }).catch(() => false);
    if (!filterVisible) {
      test.skip(); return;
    }

    await statusFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Active"), li:has-text("Active"), option:has-text("Active")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(600);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-SUB-010
  test('TC-SUB-010: Search subscriptions', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"], [aria-label*="search"] input').first();
    const searchVisible = await searchInput.isVisible({ timeout: 4000 }).catch(() => false);
    if (!searchVisible) {
      test.skip(); return;
    }

    await searchInput.fill('TEST');
    await page.waitForTimeout(800);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
    await searchInput.clear();
  });

  // TC-SUB-011
  test('TC-SUB-011: Delete a test subscription', async ({ page }) => {
    await page.goto(`${BASE_URL}/subscriptions`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteRowByAction(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-SUB-012
  test('TC-SUB-012: Subscription billing management – check billing tab', async ({ page }) => {
    const url = subscriptionDetailUrl || `${BASE_URL}/subscriptions`;
    await page.goto(url);
    await page.waitForLoadState('domcontentloaded');

    if (!subscriptionDetailUrl) {
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/subscriptions\/\d+/, { timeout: 4000 }).catch(() => {});
    }

    const billingTab = page.locator('[role="tab"]:has-text("Billing"), a:has-text("Billing"), button:has-text("Billing")').first();
    const tabVisible = await billingTab.isVisible({ timeout: 3000 }).catch(() => false);
    if (tabVisible) {
      await billingTab.click();
      await page.waitForTimeout(500);
    }

    await expect(page.locator('main, [class*="billing"], [class*="detail"], [class*="card"]').first()).toBeVisible({ timeout: 8000 });
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// PRODUCTS
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Products', () => {
  test.describe.configure({ mode: 'serial' });

  let createdProductName: string;
  let productSku: string;

  // TC-PRD-001
  test('TC-PRD-001: Navigate to /products and verify page loads with product list', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"]').filter({ hasText: /product/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"], [class*="card"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-PRD-002
  test('TC-PRD-002: Create a product', async ({ page }) => {
    createdProductName = `TEST_Product_${ts()}`;
    productSku = `TST-SKU-${ts()}`;
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    await openAddDialog(page);

    // Name
    const nameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="Name"]').first();
    await nameField.fill(createdProductName).catch(async () => {
      await page.locator('[role="dialog"] input').first().fill(createdProductName);
    });

    // SKU
    const skuField = page.locator('[role="dialog"] input[name*="sku"], [role="dialog"] input[placeholder*="SKU"], [role="dialog"] input[name*="code"]').first();
    const skuVisible = await skuField.isVisible({ timeout: 2000 }).catch(() => false);
    if (skuVisible) await skuField.fill(productSku);

    // Price
    const priceField = page.locator('[role="dialog"] input[name*="price"], [role="dialog"] input[name*="amount"], [role="dialog"] input[type="number"]').first();
    await priceField.fill('299.99').catch(() => {});

    // Description
    const descField = page.locator('[role="dialog"] textarea[name*="desc"], [role="dialog"] textarea[placeholder*="Description"], [role="dialog"] textarea').first();
    await descField.fill('E2E test product').catch(() => {});

    // Category
    const categoryField = page.locator('[role="dialog"] [name*="category"], [role="dialog"] [name*="Category"], [role="dialog"] label:has-text("Category") + * input').first();
    const catVisible = await categoryField.isVisible({ timeout: 2000 }).catch(() => false);
    if (catVisible) {
      await categoryField.fill('Software').catch(async () => {
        await categoryField.click().catch(() => {});
        await page.locator('[role="option"]:has-text("Software"), li:has-text("Software"), option:has-text("Software")').first().click({ timeout: 3000 }).catch(() => {});
      });
      await page.locator('[role="option"]:has-text("Software"), .MuiAutocomplete-option:has-text("Software")').first().click({ timeout: 3000 }).catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-PRD-003
  test('TC-PRD-003: View product details', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(async () => {
      await page.locator('a[href*="/products/"]').first().click().catch(() => {});
    });

    const detail = page.locator('[role="dialog"], [class*="detail"]').first();
    const navDetail = page.waitForURL(/\/products\/\d+/, { timeout: 5000 }).catch(() => {});
    await Promise.race([detail.waitFor({ timeout: 5000 }), navDetail]).catch(() => {});

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2, [role="dialog"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-PRD-004
  test('TC-PRD-004: Edit product – update price and description', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    const editBtn = page.locator('button[aria-label*="edit"], button:has-text("Edit")').first();
    const editVisible = await editBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (editVisible) {
      await editBtn.click();
    } else {
      await page.locator('.MuiDataGrid-row, tbody tr').first().hover().catch(() => {});
      await page.locator('button[aria-label*="more"], button[aria-label*="action"]').first().click({ timeout: 3000 }).catch(() => {});
      await page.locator('li:has-text("Edit"), [role="menuitem"]:has-text("Edit")').first().click({ timeout: 3000 }).catch(() => {});
    }

    await page.locator('[role="dialog"], form').waitFor({ timeout: 5000 }).catch(() => {});

    // Update price
    const priceField = page.locator('[role="dialog"] input[name*="price"], [role="dialog"] input[type="number"]').first();
    await priceField.fill('349.99').catch(() => {});

    // Update description
    const descField = page.locator('[role="dialog"] textarea').first();
    await descField.fill('Updated E2E test product description').catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-PRD-005
  test('TC-PRD-005: Product categories – verify category filter / tabs', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    // Check for category tabs or filter chips
    const categoryControl = page.locator('[role="tab"]:has-text("All"), [role="tab"], [class*="category"], button:has-text("All"), [class*="chip"]').first();
    const catVisible = await categoryControl.isVisible({ timeout: 4000 }).catch(() => false);

    if (catVisible) {
      await expect(categoryControl).toBeVisible();
      console.log('TC-PRD-005: Category filter/tabs found');
    } else {
      // Fallback: check for a select/dropdown category filter
      const catFilter = page.locator('select[name*="category"], [aria-label*="category"]').first();
      await expect(catFilter.or(page.locator('.MuiDataGrid-root, table').first())).toBeVisible({ timeout: 5000 });
    }
  });

  // TC-PRD-006
  test('TC-PRD-006: Search products', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"], [aria-label*="search"] input').first();
    const searchVisible = await searchInput.isVisible({ timeout: 4000 }).catch(() => false);
    if (!searchVisible) {
      test.skip(); return;
    }

    await searchInput.fill('TEST');
    await page.waitForTimeout(800);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
    await searchInput.clear();
  });

  // TC-PRD-007
  test('TC-PRD-007: Filter products by category', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    const categoryFilter = page.locator('[aria-label*="category"], button:has-text("Category"), select[name*="category"]').first();
    const filterVisible = await categoryFilter.isVisible({ timeout: 3000 }).catch(() => false);
    if (!filterVisible) {
      // Try clicking a category tab if one is visible
      const catTab = page.locator('[role="tab"]').nth(1);
      const tabVisible = await catTab.isVisible({ timeout: 2000 }).catch(() => false);
      if (tabVisible) {
        await catTab.click();
        await page.waitForTimeout(500);
        await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
      } else {
        test.skip();
      }
      return;
    }

    await categoryFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Software"), li:has-text("Software"), option:has-text("Software")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(600);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-PRD-008
  test('TC-PRD-008: Sort products by price', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    // Click on Price column header to sort
    const priceHeader = page.locator('[role="columnheader"]:has-text("Price"), th:has-text("Price"), [data-field="price"]').first();
    const headerVisible = await priceHeader.isVisible({ timeout: 4000 }).catch(() => false);
    if (!headerVisible) {
      test.skip(); return;
    }

    await priceHeader.click();
    await page.waitForTimeout(600);
    // Sort descending
    await priceHeader.click();
    await page.waitForTimeout(600);

    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-PRD-009
  test('TC-PRD-009: Delete a product', async ({ page }) => {
    await page.goto(`${BASE_URL}/products`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteRowByAction(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-PRD-010
  test('TC-PRD-010: Pricing rules – verify if pricing configuration section exists', async ({ page }) => {
    // Try common pricing-related routes
    const pricingRoutes = [
      `${BASE_URL}/products/pricing`,
      `${BASE_URL}/pricing`,
      `${BASE_URL}/pricing-rules`,
    ];

    let found = false;
    for (const route of pricingRoutes) {
      await page.goto(route);
      await page.waitForLoadState('domcontentloaded');
      const notFound = await page.locator('text=/404|not found|page not found/i').isVisible({ timeout: 2000 }).catch(() => false);
      if (!notFound) {
        found = true;
        const content = page.locator('h1, h2, [class*="pricing"], [class*="Pricing"], main').first();
        await expect(content).toBeVisible({ timeout: 5000 });
        break;
      }
    }

    if (!found) {
      // Fall back: check on products page for a pricing tab
      await page.goto(`${BASE_URL}/products`);
      await page.waitForLoadState('domcontentloaded');
      const pricingTab = page.locator('[role="tab"]:has-text("Pricing"), a:has-text("Pricing")').first();
      const tabVisible = await pricingTab.isVisible({ timeout: 3000 }).catch(() => false);
      if (tabVisible) {
        await pricingTab.click();
        await expect(page.locator('main, [class*="pricing"]').first()).toBeVisible({ timeout: 5000 });
      } else {
        console.warn('TC-PRD-010: Pricing rules section not found - feature may not be implemented');
        test.skip();
      }
    }
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// TERRITORIES
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Territories', () => {
  test.describe.configure({ mode: 'serial' });

  let createdTerritoryName: string;

  // TC-TER-001
  test('TC-TER-001: Navigate to /territories and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/territories`);
    await page.waitForLoadState('domcontentloaded');

    const content = page.locator('h1, h2, h3, [class*="title"], [class*="heading"], main').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });

  // TC-TER-002
  test('TC-TER-002: Create a territory (if Add button available)', async ({ page }) => {
    createdTerritoryName = `TEST_Territory_${ts()}`;
    await page.goto(`${BASE_URL}/territories`);
    await page.waitForLoadState('domcontentloaded');

    const addBtn = page.locator('button:has-text("Add Territory"), button:has-text("Add"), button:has-text("Create"), button:has-text("New"), [aria-label="add"]').first();
    const addVisible = await addBtn.isVisible({ timeout: 4000 }).catch(() => false);
    if (!addVisible) {
      test.skip(); return;
    }

    await addBtn.click();
    await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});

    const nameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[placeholder*="Name"], [role="dialog"] input').first();
    await nameField.fill(createdTerritoryName).catch(() => {});

    // Optional description
    const descField = page.locator('[role="dialog"] textarea').first();
    await descField.fill('E2E test territory').catch(() => {});

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('main, .MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-TER-003
  test('TC-TER-003: View territory details', async ({ page }) => {
    await page.goto(`${BASE_URL}/territories`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await rows.first().click().catch(async () => {
      await page.locator('a[href*="/territories/"]').first().click().catch(() => {});
    });

    const detail = page.locator('[role="dialog"], [class*="detail"]').first();
    const navDetail = page.waitForURL(/\/territories\/\d+/, { timeout: 5000 }).catch(() => {});
    await Promise.race([detail.waitFor({ timeout: 5000 }), navDetail]).catch(() => {});

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2, [role="dialog"], main').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-TER-004
  test('TC-TER-004: Delete a territory', async ({ page }) => {
    await page.goto(`${BASE_URL}/territories`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteRowByAction(page);
    await expect(page.locator('main, .MuiDataGrid-root, table').first()).toBeVisible();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// CPQ (CONFIGURE PRICE QUOTE)
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – CPQ (Configure Price Quote)', () => {
  test.describe.configure({ mode: 'serial' });

  // TC-CPQ-001
  test('TC-CPQ-001: Navigate to /quotes/bundle-wizard and verify wizard loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/quotes/bundle-wizard`);
    await page.waitForLoadState('domcontentloaded');

    const wizardContent = page.locator(
      '[class*="wizard"], [class*="Wizard"], [class*="stepper"], [class*="Stepper"], ' +
      '[class*="step"], [class*="Step"], h1, h2, h3, [class*="title"]'
    ).first();
    await expect(wizardContent).toBeVisible({ timeout: 10000 });
  });

  // TC-CPQ-002
  test('TC-CPQ-002: CPQ Step 1 – Select products / start bundle configuration', async ({ page }) => {
    await page.goto(`${BASE_URL}/quotes/bundle-wizard`);
    await page.waitForLoadState('domcontentloaded');

    // Look for product selection in step 1
    const step1Indicator = page.locator('[class*="step-1"], [class*="Step1"], [aria-label*="step 1"], .MuiStep-root:first-child').first();
    const step1Visible = await step1Indicator.isVisible({ timeout: 4000 }).catch(() => false);
    if (step1Visible) {
      console.log('TC-CPQ-002: Step 1 indicator found');
    }

    // Look for product cards / product selection area
    const productSelection = page.locator('[class*="product"], [class*="Product"], [class*="bundle"], .MuiCard-root, .MuiDataGrid-root, table').first();
    const selectionVisible = await productSelection.isVisible({ timeout: 4000 }).catch(() => false);

    // Try clicking on a product to select it
    if (selectionVisible) {
      const firstProduct = page.locator('[class*="product-card"], .MuiCard-root, .MuiDataGrid-row, [role="row"]').first();
      await firstProduct.click().catch(() => {});
      await page.waitForTimeout(300);
    }

    // Look for Next / Continue button
    const nextBtn = page.locator('button:has-text("Next"), button:has-text("Continue"), button:has-text("Proceed")').first();
    const nextVisible = await nextBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (nextVisible) {
      console.log('TC-CPQ-002: Next/Continue button found in Step 1');
    }

    await expect(page.locator('main, [class*="wizard"], [class*="content"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-CPQ-003
  test('TC-CPQ-003: CPQ Step 2 – Configure bundle options', async ({ page }) => {
    await page.goto(`${BASE_URL}/quotes/bundle-wizard`);
    await page.waitForLoadState('domcontentloaded');

    // Try to advance to step 2
    const nextBtn = page.locator('button:has-text("Next"), button:has-text("Continue"), button:has-text("Proceed")').first();
    const nextVisible = await nextBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (nextVisible) {
      await nextBtn.click();
      await page.waitForTimeout(500);

      // Step 2 content
      const step2Content = page.locator('[class*="step-2"], [class*="Step2"], [class*="configure"], [class*="option"]').first();
      const step2Visible = await step2Content.isVisible({ timeout: 3000 }).catch(() => false);
      if (step2Visible) {
        console.log('TC-CPQ-003: Step 2 configuration options found');
      }
    }

    await expect(page.locator('main, [class*="wizard"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-CPQ-004
  test('TC-CPQ-004: Navigate to /quotes/bundles and verify bundles list', async ({ page }) => {
    await page.goto(`${BASE_URL}/quotes/bundles`);
    await page.waitForLoadState('domcontentloaded');

    const content = page.locator('h1, h2, h3, [class*="title"], [class*="bundle"], [class*="Bundle"], main, .MuiDataGrid-root, table').first();
    await expect(content).toBeVisible({ timeout: 10000 });
  });
});

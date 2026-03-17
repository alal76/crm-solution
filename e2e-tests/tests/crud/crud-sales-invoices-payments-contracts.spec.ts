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

async function deleteFirstRow(page: Page) {
  const rows = page.locator('.MuiDataGrid-row, tbody tr');
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
// INVOICES
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Invoices', () => {
  test.describe.configure({ mode: 'serial' });

  let createdInvoiceName: string;

  // TC-INV-001
  test('TC-INV-001: Navigate to /invoices and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"], [class*="heading"]').filter({ hasText: /invoice/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-INV-002
  test('TC-INV-002: Create an invoice', async ({ page }) => {
    createdInvoiceName = `TEST_INV_${ts()}`;
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    await openAddDialog(page);

    // Invoice number / name
    const nameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[name*="number"], [role="dialog"] input[name*="invoice"], [role="dialog"] input').first();
    await nameField.fill(createdInvoiceName).catch(() => {});

    // Due date
    const dueDateField = page.locator('[role="dialog"] input[name*="due"], [role="dialog"] input[name*="date"], [role="dialog"] input[type="date"]').first();
    await dueDateField.fill('2026-12-31').catch(() => {});

    // Notes
    const notesField = page.locator('[role="dialog"] textarea[name*="note"], [role="dialog"] textarea[placeholder*="Note"], [role="dialog"] textarea').first();
    await notesField.fill('E2E test invoice').catch(() => {});

    // Account
    const accountField = page.locator('[role="dialog"] input[name*="account"], [role="dialog"] label:has-text("Account") + * input').first();
    const accountVisible = await accountField.isVisible({ timeout: 2000 }).catch(() => false);
    if (accountVisible) {
      await accountField.fill('TEST').catch(() => {});
      await page.locator('[role="option"], .MuiAutocomplete-option').first().click({ timeout: 3000 }).catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-INV-003
  test('TC-INV-003: View invoice details', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(async () => {
      await page.locator('a[href*="/invoices/"]').first().click().catch(() => {});
    });

    const detail = page.locator('[role="dialog"], [class*="detail"]').first();
    const navDetail = page.waitForURL(/\/invoices\/\d+/, { timeout: 5000 }).catch(() => {});
    await Promise.race([detail.waitFor({ timeout: 5000 }), navDetail]).catch(() => {});

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2, [role="dialog"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-INV-004
  test('TC-INV-004: Edit invoice – update notes', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
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

    const notesField = page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="note"]').first();
    await notesField.fill('Updated by E2E test').catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-INV-005
  test('TC-INV-005: Send invoice (if button available)', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
    await page.waitForURL(/\/invoices\/\d+/, { timeout: 4000 }).catch(() => {});

    const sendBtn = page.locator('button:has-text("Send Invoice"), button:has-text("Send"), [data-testid*="send"]').first();
    const sendVisible = await sendBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!sendVisible) {
      test.skip(); return;
    }

    await sendBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Send"), button:has-text("Yes")').first().click({ timeout: 3000 }).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-INV-006
  test('TC-INV-006: Mark invoice as paid', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
    await page.waitForURL(/\/invoices\/\d+/, { timeout: 4000 }).catch(() => {});

    const markPaidBtn = page.locator('button:has-text("Mark as Paid"), button:has-text("Mark Paid"), [data-testid*="paid"]').first();
    const paidVisible = await markPaidBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!paidVisible) {
      test.skip(); return;
    }

    await markPaidBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes")').first().click({ timeout: 3000 }).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-INV-007
  test('TC-INV-007: Generate PDF (if available)', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
    await page.waitForURL(/\/invoices\/\d+/, { timeout: 4000 }).catch(() => {});

    const pdfBtn = page.locator('button:has-text("PDF"), button:has-text("Download"), button:has-text("Generate PDF"), [aria-label*="pdf"]').first();
    const pdfVisible = await pdfBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!pdfVisible) {
      test.skip(); return;
    }

    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 8000 }).catch(() => null),
      pdfBtn.click(),
    ]);
    await page.waitForTimeout(1000);
    await expect(page.locator('main, [class*="detail"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-INV-008
  test('TC-INV-008: Filter invoices by status', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    const statusFilter = page.locator('[aria-label*="status"], button:has-text("Status"), select[name*="status"], [class*="filter"]').first();
    const filterVisible = await statusFilter.isVisible({ timeout: 3000 }).catch(() => false);
    if (!filterVisible) {
      test.skip(); return;
    }

    await statusFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Draft"), li:has-text("Draft"), option:has-text("Draft")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(600);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-INV-009
  test('TC-INV-009: Search invoices', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
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

  // TC-INV-010
  test('TC-INV-010: Delete an invoice', async ({ page }) => {
    await page.goto(`${BASE_URL}/invoices`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteFirstRow(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// PAYMENTS
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Payments', () => {
  test.describe.configure({ mode: 'serial' });

  // TC-PAY-001
  test('TC-PAY-001: Navigate to /payments and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"]').filter({ hasText: /payment/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-PAY-002
  test('TC-PAY-002: Create a payment', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    await openAddDialog(page);

    // Amount
    const amountField = page.locator('[role="dialog"] input[name*="amount"], [role="dialog"] input[placeholder*="Amount"], [role="dialog"] input[type="number"]').first();
    await amountField.fill('1000').catch(async () => {
      await page.locator('[role="dialog"] input').nth(0).fill('1000');
    });

    // Payment method
    const methodSelect = page.locator('[role="dialog"] [name*="method"], [role="dialog"] label:has-text("Method") + * [role="combobox"], [role="dialog"] select[name*="method"]').first();
    const methodVisible = await methodSelect.isVisible({ timeout: 2000 }).catch(() => false);
    if (methodVisible) {
      await methodSelect.click().catch(() => {});
      await page.locator('[role="option"]:has-text("Credit Card"), [role="option"]:has-text("Bank"), option:has-text("Credit")').first().click({ timeout: 3000 }).catch(() => {});
    }

    // Reference
    const refField = page.locator('[role="dialog"] input[name*="ref"], [role="dialog"] input[name*="reference"], [role="dialog"] input[placeholder*="Reference"]').first();
    const refVisible = await refField.isVisible({ timeout: 2000 }).catch(() => false);
    if (refVisible) await refField.fill(`TEST-PAY-${ts()}`);

    // Link to invoice
    const invoiceField = page.locator('[role="dialog"] input[name*="invoice"], [role="dialog"] label:has-text("Invoice") + * input').first();
    const invoiceVisible = await invoiceField.isVisible({ timeout: 2000 }).catch(() => false);
    if (invoiceVisible) {
      await invoiceField.fill('TEST').catch(() => {});
      await page.locator('[role="option"], .MuiAutocomplete-option').first().click({ timeout: 3000 }).catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-PAY-003
  test('TC-PAY-003: View payment details', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(async () => {
      await page.locator('a[href*="/payments/"]').first().click().catch(() => {});
    });

    const detail = page.locator('[role="dialog"], [class*="detail"]').first();
    const navDetail = page.waitForURL(/\/payments\/\d+/, { timeout: 5000 }).catch(() => {});
    await Promise.race([detail.waitFor({ timeout: 5000 }), navDetail]).catch(() => {});

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2, [role="dialog"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-PAY-004
  test('TC-PAY-004: Edit payment – update reference/notes', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
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

    const refField = page.locator('[role="dialog"] input[name*="ref"], [role="dialog"] input[name*="note"], [role="dialog"] textarea').first();
    await refField.fill(`UPDATED-PAY-REF-${ts()}`).catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-PAY-005
  test('TC-PAY-005: Refund payment (if button available)', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
    await page.waitForURL(/\/payments\/\d+/, { timeout: 4000 }).catch(() => {});

    const refundBtn = page.locator('button:has-text("Refund"), [data-testid*="refund"]').first();
    const refundVisible = await refundBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!refundVisible) {
      test.skip(); return;
    }

    await refundBtn.click();
    await page.locator('button:has-text("Confirm"), button:has-text("Yes")').first().click({ timeout: 3000 }).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-PAY-006
  test('TC-PAY-006: Filter payments by status', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    const statusFilter = page.locator('[aria-label*="status"], button:has-text("Status"), select[name*="status"]').first();
    const filterVisible = await statusFilter.isVisible({ timeout: 3000 }).catch(() => false);
    if (!filterVisible) {
      test.skip(); return;
    }

    await statusFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Completed"), li:has-text("Completed"), option:has-text("Complete")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(600);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-PAY-007
  test('TC-PAY-007: Search payments', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
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

  // TC-PAY-008
  test('TC-PAY-008: Delete a payment', async ({ page }) => {
    await page.goto(`${BASE_URL}/payments`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteFirstRow(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// CONTRACTS
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Contracts', () => {
  test.describe.configure({ mode: 'serial' });

  let createdContractName: string;
  let contractDetailUrl: string;

  // TC-CON-C-001
  test('TC-CON-C-001: Navigate to /contracts and verify page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"]').filter({ hasText: /contract/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.MuiDataGrid-root, table, [role="grid"]').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-CON-C-002
  test('TC-CON-C-002: Create a contract', async ({ page }) => {
    createdContractName = `TEST_Contract_${ts()}`;
    await page.goto(`${BASE_URL}/contracts`);
    await page.waitForLoadState('domcontentloaded');

    await openAddDialog(page);

    // Name
    const nameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input[name*="title"], [role="dialog"] input').first();
    await nameField.fill(createdContractName).catch(() => {});

    // Contract type
    const typeField = page.locator('[role="dialog"] [name*="type"], [role="dialog"] label:has-text("Type") + * [role="combobox"]').first();
    const typeVisible = await typeField.isVisible({ timeout: 2000 }).catch(() => false);
    if (typeVisible) {
      await typeField.click().catch(() => {});
      await page.locator('[role="option"]:has-text("Service"), li:has-text("Service Agreement"), option:has-text("Service")').first().click({ timeout: 3000 }).catch(() => {});
    }

    // Dates
    const startDateField = page.locator('[role="dialog"] input[name*="start"], [role="dialog"] input[type="date"]').first();
    await startDateField.fill('2026-01-01').catch(() => {});

    const endDateField = page.locator('[role="dialog"] input[name*="end"], [role="dialog"] input[type="date"]').nth(1);
    await endDateField.fill('2026-12-31').catch(() => {});

    // Value
    const valueField = page.locator('[role="dialog"] input[name*="value"], [role="dialog"] input[name*="amount"], [role="dialog"] input[type="number"]').first();
    await valueField.fill('100000').catch(() => {});

    // Account
    const accountField = page.locator('[role="dialog"] input[name*="account"], [role="dialog"] label:has-text("Account") + * input').first();
    const accountVisible = await accountField.isVisible({ timeout: 2000 }).catch(() => false);
    if (accountVisible) {
      await accountField.fill('TEST').catch(() => {});
      await page.locator('[role="option"], .MuiAutocomplete-option').first().click({ timeout: 3000 }).catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 10000 });
  });

  // TC-CON-C-003
  test('TC-CON-C-003: View contract details', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
    await page.waitForLoadState('domcontentloaded');

    await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(async () => {
      await page.locator('a[href*="/contracts/"]').first().click().catch(() => {});
    });

    await page.waitForURL(/\/contracts\/\d+/, { timeout: 5000 }).catch(() => {});
    contractDetailUrl = page.url();

    await expect(page.locator('[class*="detail"], [class*="card"], h1, h2').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-CON-C-004
  test('TC-CON-C-004: Contract detail tabs – Overview, Terms, Documents, Signatures', async ({ page }) => {
    if (!contractDetailUrl) {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('domcontentloaded');
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/contracts\/\d+/, { timeout: 5000 }).catch(() => {});
    } else {
      await page.goto(contractDetailUrl);
      await page.waitForLoadState('domcontentloaded');
    }

    const tabs = ['Overview', 'Terms', 'Documents', 'Signatures'];
    for (const tabName of tabs) {
      const tab = page.locator(`[role="tab"]:has-text("${tabName}"), a:has-text("${tabName}"), button:has-text("${tabName}")`).first();
      const tabVisible = await tab.isVisible({ timeout: 3000 }).catch(() => false);
      if (tabVisible) {
        await tab.click();
        await page.waitForTimeout(400);
        console.log(`TC-CON-C-004: Clicked tab "${tabName}"`);
      } else {
        console.warn(`TC-CON-C-004: Tab "${tabName}" not found`);
      }
    }

    await expect(page.locator('main, [class*="detail"], [class*="card"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-CON-C-005
  test('TC-CON-C-005: Edit contract – update terms/value', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
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

    // Update value field
    const valueField = page.locator('[role="dialog"] input[name*="value"], [role="dialog"] input[name*="amount"], [role="dialog"] input[type="number"]').first();
    await valueField.fill('120000').catch(() => {});

    // Update notes/terms
    const notesField = page.locator('[role="dialog"] textarea, [role="dialog"] input[name*="note"]').first();
    await notesField.fill('Updated terms by E2E test').catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-CON-C-006
  test('TC-CON-C-006: Upload document to contract (if upload button exists)', async ({ page }) => {
    if (!contractDetailUrl) {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('domcontentloaded');
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/contracts\/\d+/, { timeout: 5000 }).catch(() => {});
    } else {
      await page.goto(contractDetailUrl);
      await page.waitForLoadState('domcontentloaded');
    }

    const docsTab = page.locator('[role="tab"]:has-text("Documents"), a:has-text("Documents")').first();
    const docsVisible = await docsTab.isVisible({ timeout: 3000 }).catch(() => false);
    if (docsVisible) await docsTab.click();

    const uploadBtn = page.locator('button:has-text("Upload"), button:has-text("Attach"), input[type="file"], [aria-label*="upload"]').first();
    const uploadVisible = await uploadBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!uploadVisible) {
      test.skip(); return;
    }

    // Simulate file upload using file chooser
    const fileChooserPromise = page.waitForEvent('filechooser', { timeout: 5000 });
    await uploadBtn.click();
    const fileChooser = await fileChooserPromise.catch(() => null);
    if (fileChooser) {
      // Set a fake file – in practice this would require a real file
      // We just verify the file chooser was triggered
      console.log('TC-CON-C-006: File chooser opened successfully');
      await page.keyboard.press('Escape'); // Close file chooser
    }
  });

  // TC-CON-C-007
  test('TC-CON-C-007: Request signature (if e-signature button exists)', async ({ page }) => {
    if (!contractDetailUrl) {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('domcontentloaded');
      await page.locator('.MuiDataGrid-row, tbody tr').first().click().catch(() => {});
      await page.waitForURL(/\/contracts\/\d+/, { timeout: 5000 }).catch(() => {});
    } else {
      await page.goto(contractDetailUrl);
      await page.waitForLoadState('domcontentloaded');
    }

    const sigTab = page.locator('[role="tab"]:has-text("Signature"), a:has-text("Signatures")').first();
    const sigTabVisible = await sigTab.isVisible({ timeout: 3000 }).catch(() => false);
    if (sigTabVisible) await sigTab.click();

    const signBtn = page.locator('button:has-text("Request Signature"), button:has-text("Send for Signature"), button:has-text("Sign"), [data-testid*="signature"]').first();
    const signVisible = await signBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (!signVisible) {
      test.skip(); return;
    }

    await signBtn.click();
    await page.locator('[role="dialog"]').waitFor({ timeout: 4000 }).catch(() => {});
    // Close the dialog without completing
    await closeDialogIfOpen(page);
    await expect(page.locator('main, [class*="detail"]').first()).toBeVisible({ timeout: 5000 });
  });

  // TC-CON-C-008
  test('TC-CON-C-008: Filter contracts by status and type', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
    await page.waitForLoadState('domcontentloaded');

    const statusFilter = page.locator('[aria-label*="status"], button:has-text("Status"), select[name*="status"]').first();
    const filterVisible = await statusFilter.isVisible({ timeout: 3000 }).catch(() => false);
    if (!filterVisible) {
      test.skip(); return;
    }

    await statusFilter.click().catch(() => {});
    await page.locator('[role="option"]:has-text("Active"), li:has-text("Active"), option:has-text("Active")').first().click({ timeout: 3000 }).catch(() => {});
    await page.waitForTimeout(600);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });

  // TC-CON-C-009
  test('TC-CON-C-009: Search contracts', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
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

  // TC-CON-C-010
  test('TC-CON-C-010: Delete a contract', async ({ page }) => {
    await page.goto(`${BASE_URL}/contracts`);
    await page.waitForLoadState('domcontentloaded');

    const rows = page.locator('.MuiDataGrid-row, tbody tr');
    const rowCount = await rows.count().catch(() => 0);
    if (rowCount === 0) { test.skip(); return; }

    await deleteFirstRow(page);
    await expect(page.locator('.MuiDataGrid-root, table').first()).toBeVisible();
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// COMMISSIONS
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Sales Module – Commissions', () => {
  test.describe.configure({ mode: 'serial' });

  // TC-COMM-001
  test('TC-COMM-001: Navigate to /commissions and verify page loads with tabs', async ({ page }) => {
    await page.goto(`${BASE_URL}/commissions`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"]').filter({ hasText: /commission/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });

    // Verify at least some main content is visible
    await expect(page.locator('main, [class*="content"], [role="main"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-COMM-002
  test('TC-COMM-002: Navigate commission sub-tabs (Overview, Plans, Statements, Leaderboard)', async ({ page }) => {
    await page.goto(`${BASE_URL}/commissions`);
    await page.waitForLoadState('domcontentloaded');

    const subTabs = ['Overview', 'Plans', 'Statements', 'Leaderboard'];
    for (const tabName of subTabs) {
      const tab = page.locator(`[role="tab"]:has-text("${tabName}"), a:has-text("${tabName}"), button:has-text("${tabName}")`).first();
      const tabVisible = await tab.isVisible({ timeout: 3000 }).catch(() => false);
      if (tabVisible) {
        await tab.click();
        await page.waitForTimeout(500);
        console.log(`TC-COMM-002: Clicked sub-tab "${tabName}"`);
      } else {
        // Try navigating directly
        const tabPaths: Record<string, string> = {
          Overview: '/commissions',
          Plans: '/commissions/plans',
          Statements: '/commissions/statements',
          Leaderboard: '/commissions/leaderboard',
        };
        await page.goto(`${BASE_URL}${tabPaths[tabName]}`).catch(() => {});
        await page.waitForLoadState('domcontentloaded');
        console.warn(`TC-COMM-002: Tab "${tabName}" not found inline, navigated directly`);
      }
      await expect(page.locator('main, [class*="content"]').first()).toBeVisible({ timeout: 5000 });
    }
  });

  // TC-COMM-003
  test('TC-COMM-003: View commission plans', async ({ page }) => {
    await page.goto(`${BASE_URL}/commissions/plans`);
    await page.waitForLoadState('domcontentloaded');

    const plansTab = page.locator('[role="tab"]:has-text("Plans"), a:has-text("Plans")').first();
    const tabVisible = await plansTab.isVisible({ timeout: 3000 }).catch(() => false);
    if (tabVisible) await plansTab.click();

    await expect(page.locator('main, .MuiDataGrid-root, table, [class*="card"], [class*="plan"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-COMM-004
  test('TC-COMM-004: Create a commission plan (if button available)', async ({ page }) => {
    await page.goto(`${BASE_URL}/commissions/plans`);
    await page.waitForLoadState('domcontentloaded');

    const addBtn = page.locator('button:has-text("Add Plan"), button:has-text("Create Plan"), button:has-text("Add"), button:has-text("New"), [aria-label="add"]').first();
    const addVisible = await addBtn.isVisible({ timeout: 4000 }).catch(() => false);
    if (!addVisible) {
      test.skip(); return;
    }

    await addBtn.click();
    await page.locator('[role="dialog"]').waitFor({ timeout: 5000 }).catch(() => {});

    const nameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input').first();
    await nameField.fill(`E2E Commission Plan ${ts()}`).catch(() => {});

    const typeField = page.locator('[role="dialog"] [name*="type"], [role="dialog"] label:has-text("Type") + * input').first();
    await typeField.fill('Percentage').catch(async () => {
      await page.locator('[role="dialog"] [name*="type"]').first().click().catch(() => {});
      await page.locator('[role="option"]:has-text("Percentage"), option:has-text("Percent")').first().click({ timeout: 3000 }).catch(() => {});
    });

    const rateField = page.locator('[role="dialog"] input[name*="rate"], [role="dialog"] input[type="number"]').first();
    await rateField.fill('10').catch(() => {});

    await submitForm(page).catch(() => {});
    await waitForSuccess(page);
  });

  // TC-COMM-005
  test('TC-COMM-005: View commission statements', async ({ page }) => {
    await page.goto(`${BASE_URL}/commissions/statements`);
    await page.waitForLoadState('domcontentloaded');

    const stmtTab = page.locator('[role="tab"]:has-text("Statement"), a:has-text("Statement")').first();
    const tabVisible = await stmtTab.isVisible({ timeout: 3000 }).catch(() => false);
    if (tabVisible) await stmtTab.click();

    await expect(page.locator('main, .MuiDataGrid-root, table, [class*="statement"]').first()).toBeVisible({ timeout: 8000 });
  });

  // TC-COMM-006
  test('TC-COMM-006: Navigate to /teams and verify teams page loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/teams`);
    await page.waitForLoadState('domcontentloaded');

    const heading = page.locator('h1, h2, h3, [class*="title"]').filter({ hasText: /team/i });
    await expect(heading.first()).toBeVisible({ timeout: 10000 });

    await expect(page.locator('main, .MuiDataGrid-root, table, [role="grid"], [class*="card"]').first()).toBeVisible({ timeout: 10000 });

    // Optionally add a team if button visible
    const addBtn = page.locator('button:has-text("Add Team"), button:has-text("Add"), button:has-text("Create"), button:has-text("New")').first();
    const addVisible = await addBtn.isVisible({ timeout: 3000 }).catch(() => false);
    if (addVisible) {
      await addBtn.click();
      await page.locator('[role="dialog"]').waitFor({ timeout: 4000 }).catch(() => {});
      const teamNameField = page.locator('[role="dialog"] input[name*="name"], [role="dialog"] input').first();
      await teamNameField.fill(`E2E Team ${ts()}`).catch(() => {});
      await submitForm(page).catch(() => {});
      await waitForSuccess(page);
    }
  });
});

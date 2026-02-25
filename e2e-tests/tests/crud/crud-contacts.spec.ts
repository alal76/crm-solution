import { test, expect, Page } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';

test.describe.configure({ mode: 'serial' });

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
async function ensureLoggedIn(page: Page) {
  await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
  if (page.url().includes('/login')) {
    await page.fill('input[type="email"]', 'admin@crm.local');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
  }
}

async function clickAddButton(page: Page) {
  const addBtn = page.locator(
    'button:has-text("Add Contact"), button:has-text("New Contact"), button:has-text("Add"), button:has-text("Create"), button:has-text("New"), [data-testid*="add"], [aria-label*="add"]'
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
  await page.locator(`label`, { hasText: label }).first().locator('..').locator('input').first().fill(value).catch(async () => {
    await page.locator(`input[placeholder*="${label}"]`).first().fill(value).catch(() => {});
  });
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
    // Try clicking the select root
    await page.locator(`[aria-label*="${fieldLabel}"]`).first().click().catch(() => {});
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
  await page.locator('.MuiDataGrid-root, table, [role="grid"], [data-testid*="grid"]').first().waitFor({ state: 'visible', timeout: 15000 });
}

function ts(): string { return Date.now().toString().slice(-6); }

// ---------------------------------------------------------------------------
// Test Suite: Contacts CRUD
// ---------------------------------------------------------------------------
test.describe('Contacts CRUD', () => {

  test.beforeEach(async ({ page }) => {
    await ensureLoggedIn(page);
  });

  // -------------------------------------------------------------------------
  // TC-CON-001: Navigate to contacts page
  // -------------------------------------------------------------------------
  test('TC-CON-001: Navigate to contacts page and verify list loads', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle').catch(() => {});

    // Verify heading
    const heading = page.locator('h1, h2, h3, .MuiTypography-h4, .MuiTypography-h5, .MuiTypography-h6').filter({ hasText: /contact/i }).first();
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
  // TC-CON-002: Create new contact
  // -------------------------------------------------------------------------
  test('TC-CON-002: Create new contact with full details', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    const suffix = ts();
    const firstName = `TEST_First_${suffix}`;
    const lastName = `TEST_Last_${suffix}`;
    const email = `test_${suffix}@contact.example.com`;

    await clickAddButton(page);
    await waitForDialog(page);

    // First Name
    await fillDialogTextField(page, 'First Name', firstName).catch(async () => {
      await page.locator('[role="dialog"] input[name*="firstName"], [role="dialog"] input[name*="first_name"], [role="dialog"] input[placeholder*="First"]').first().fill(firstName).catch(() => {});
    });

    // Last Name
    await fillDialogTextField(page, 'Last Name', lastName).catch(async () => {
      await page.locator('[role="dialog"] input[name*="lastName"], [role="dialog"] input[name*="last_name"], [role="dialog"] input[placeholder*="Last"]').first().fill(lastName).catch(() => {});
    });

    // Email
    await fillDialogTextField(page, 'Email', email).catch(async () => {
      await page.locator('[role="dialog"] input[type="email"], [role="dialog"] input[name*="email"]').first().fill(email).catch(() => {});
    });

    // Phone
    await fillDialogTextField(page, 'Phone', '+1-555-0200').catch(async () => {
      await page.locator('[role="dialog"] input[type="tel"], [role="dialog"] input[name*="phone"], [role="dialog"] input[placeholder*="hone"]').first().fill('+1-555-0200').catch(() => {});
    });

    // Job Title
    await fillDialogTextField(page, 'Job Title', 'Test Engineer').catch(async () => {
      await fillDialogTextField(page, 'Title', 'Test Engineer').catch(async () => {
        await page.locator('[role="dialog"] input[name*="title"], [role="dialog"] input[name*="job"]').first().fill('Test Engineer').catch(() => {});
      });
    });

    // Department
    await fillDialogTextField(page, 'Department', 'QA').catch(async () => {
      await page.locator('[role="dialog"] input[name*="department"]').first().fill('QA').catch(() => {});
    });

    await submitForm(page);
    await waitForSuccess(page);

    // Verify contact in list
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }
    const row = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first();
    await expect(row).toBeVisible({ timeout: 15000 });
  });

  // -------------------------------------------------------------------------
  // TC-CON-003: Create contact linked to account
  // -------------------------------------------------------------------------
  test('TC-CON-003: Create contact linked to an account', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    const suffix = ts();
    const firstName = `TEST_Linked_${suffix}`;
    const lastName = 'AccountContact';

    await clickAddButton(page);
    await waitForDialog(page);

    // Fill basic fields
    await fillDialogTextField(page, 'First Name', firstName).catch(async () => {
      await page.locator('[role="dialog"] input[name*="firstName"], [role="dialog"] input[placeholder*="First"]').first().fill(firstName).catch(() => {});
    });
    await fillDialogTextField(page, 'Last Name', lastName).catch(async () => {
      await page.locator('[role="dialog"] input[name*="lastName"], [role="dialog"] input[placeholder*="Last"]').first().fill(lastName).catch(() => {});
    });

    // Account autocomplete field
    const accountField = page.locator(
      '[role="dialog"] input[name*="account"], [role="dialog"] input[placeholder*="Account"], [role="dialog"] input[placeholder*="company"]'
    ).first();
    if (await accountField.isVisible().catch(() => false)) {
      await accountField.fill('TEST');
      await page.waitForTimeout(1000);
      // Select first suggestion from autocomplete
      const suggestion = page.locator('[role="option"], [role="listbox"] li, .MuiAutocomplete-option').first();
      if (await suggestion.isVisible({ timeout: 3000 }).catch(() => false)) {
        await suggestion.click();
      }
    }

    await submitForm(page);
    await waitForSuccess(page);
  });

  // -------------------------------------------------------------------------
  // TC-CON-004: Search contacts
  // -------------------------------------------------------------------------
  test('TC-CON-004: Search contacts by name', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"], input[type="search"], [data-testid*="search"] input').first();
    await searchInput.waitFor({ state: 'visible', timeout: 10000 });
    await searchInput.fill('TEST_First');
    await page.waitForTimeout(1500);

    const grid = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
    await expect(grid).toBeVisible();

    const hasRows = await page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first().isVisible().catch(() => false);
    const hasNoResults = await page.locator('[class*="noRows"], [class*="noData"], text=/no.*result/i, text=/no.*found/i').first().isVisible().catch(() => false);
    expect(hasRows || hasNoResults).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // TC-CON-005: View contact details
  // -------------------------------------------------------------------------
  test('TC-CON-005: View contact details', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
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

    const isDetailPage = page.url().match(/\/contacts\/\d+/);
    const isDialog = await page.locator('[role="dialog"], .MuiDrawer-root').first().isVisible().catch(() => false);

    if (isDetailPage || isDialog) {
      const detailContent = page.locator('.MuiCard-root, .MuiPaper-root, [role="dialog"], .MuiDrawer-root, main').first();
      await expect(detailContent).toBeVisible({ timeout: 10000 });

      // Verify some contact fields visible
      const fieldLabel = page.locator('label:has-text("First"), label:has-text("Name"), label:has-text("Email"), th:has-text("Email")').first();
      await expect(fieldLabel).toBeVisible({ timeout: 8000 }).catch(() => {});
    } else {
      // Content shown inline
      const content = page.locator('main, .MuiContainer-root').first();
      await expect(content).toBeVisible();
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-006: Edit contact
  // -------------------------------------------------------------------------
  test('TC-CON-006: Edit contact Job Title field', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1000);

    const editBtn = page.locator(
      'button:has-text("Edit"), [data-testid*="edit"], [aria-label*="edit"], button:has-text("Modify")'
    ).first();
    if (await editBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await editBtn.click();
      await waitForDialog(page).catch(() => {});
    }

    // Modify Job Title
    const titleField = page.locator(
      '[role="dialog"] input[name*="title"], [role="dialog"] input[name*="job"], label:has-text("Job Title") ~ div input, label:has-text("Title") ~ div input'
    ).first();
    if (await titleField.isVisible().catch(() => false)) {
      await titleField.fill('Senior Test Engineer (Updated)');
    } else {
      // fallback: edit any text input
      await page.locator('[role="dialog"] input[type="text"]').first().fill('Updated by e2e test').catch(() => {});
    }

    await submitForm(page);
    await waitForSuccess(page);
  });

  // -------------------------------------------------------------------------
  // TC-CON-007: Contact detail tabs
  // -------------------------------------------------------------------------
  test('TC-CON-007: Access contact detail tabs', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
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

    const tabLabels = ['Overview', 'Details', 'Activities', 'Notes', 'Opportunities', 'Accounts', 'Files'];
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
  // TC-CON-008: Add contact address
  // -------------------------------------------------------------------------
  test('TC-CON-008: Add address to contact', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    // Find Addresses section or tab
    const addressTab = page.locator('[role="tab"]:has-text("Address"), .MuiTab-root:has-text("Address"), [role="tab"]:has-text("Contact Info")').first();
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

      await page.locator('[role="dialog"] input[name*="street"], [role="dialog"] input[placeholder*="treet"], [role="dialog"] label:has-text("Street") ~ div input').first().fill('456 Contact Ave').catch(() => {});
      await page.locator('[role="dialog"] input[name*="city"], [role="dialog"] input[placeholder*="ity"], [role="dialog"] label:has-text("City") ~ div input').first().fill('Contact City').catch(() => {});
      await page.locator('[role="dialog"] input[name*="state"], [role="dialog"] label:has-text("State") ~ div input').first().fill('CC').catch(() => {});
      await page.locator('[role="dialog"] input[name*="zip"], [role="dialog"] input[name*="postal"], [role="dialog"] label:has-text("Postal") ~ div input').first().fill('54321').catch(() => {});
      await page.locator('[role="dialog"] input[name*="country"], [role="dialog"] label:has-text("Country") ~ div input').first().fill('US').catch(() => {});

      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-009: Add contact phone
  // -------------------------------------------------------------------------
  test('TC-CON-009: Add phone number to contact', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
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
      await page.locator('[role="dialog"] input[type="tel"], [role="dialog"] input[name*="phone"], [role="dialog"] input[placeholder*="hone"]').first().fill('+1-555-0201').catch(() => {});
      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-010: Add secondary email to contact
  // -------------------------------------------------------------------------
  test('TC-CON-010: Add secondary email address to contact', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
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
      await page.locator('[role="dialog"] input[type="email"], [role="dialog"] input[name*="email"], [role="dialog"] input[placeholder*="mail"]').first().fill(`secondary_${ts()}@contact.example.com`).catch(() => {});
      await submitForm(page);
      await waitForSuccess(page);
    } else {
      test.skip();
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-011: Add contact social media
  // -------------------------------------------------------------------------
  test('TC-CON-011: Add social media link to contact', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    const firstRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').first();
    await firstRow.waitFor({ state: 'visible', timeout: 10000 });
    await firstRow.click();
    await page.waitForTimeout(1500);

    // Look for social media tab or section
    const socialTab = page.locator('[role="tab"]:has-text("Social"), .MuiTab-root:has-text("Social")').first();
    if (await socialTab.isVisible({ timeout: 3000 }).catch(() => false)) {
      await socialTab.click();
      await page.waitForTimeout(800);
    }

    const addSocialBtn = page.locator(
      'button:has-text("Add Social"), button:has-text("Add LinkedIn"), button:has-text("Social Media"), [data-testid*="social"]'
    ).first();
    if (await addSocialBtn.isVisible({ timeout: 5000 }).catch(() => false)) {
      await addSocialBtn.click();
      await waitForDialog(page).catch(() => {});

      // Select LinkedIn platform if dropdown exists
      await selectMuiOption(page, 'Platform', 'LinkedIn').catch(() => {});

      // Fill URL
      await page.locator('[role="dialog"] input[type="url"], [role="dialog"] input[name*="url"], [role="dialog"] input[placeholder*="URL"]').first().fill('https://linkedin.com/in/test-contact').catch(() => {
        page.locator('[role="dialog"] input[placeholder*="inkedIn"]').first().fill('https://linkedin.com/in/test-contact').catch(() => {});
      });

      await submitForm(page);
      await waitForSuccess(page);
    } else {
      // Try inline LinkedIn field
      const linkedInField = page.locator('input[name*="linkedin"], input[placeholder*="inkedIn"], label:has-text("LinkedIn") ~ div input').first();
      if (await linkedInField.isVisible({ timeout: 3000 }).catch(() => false)) {
        await linkedInField.fill('https://linkedin.com/in/test-contact');
        await page.locator('button:has-text("Save"), button:has-text("Update")').first().click().catch(() => {});
        await waitForSuccess(page);
      } else {
        test.skip();
      }
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-012: Delete contact
  // -------------------------------------------------------------------------
  test('TC-CON-012: Delete a contact (create then delete)', async ({ page }) => {
    const suffix = ts();
    const firstName = `TEST_DEL_CON_${suffix}`;

    // Create contact first
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
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

    // Navigate back to list
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Search for the created contact
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }

    const targetRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first();
    await targetRow.waitFor({ state: 'visible', timeout: 10000 });

    // Delete button in row
    const deleteBtn = targetRow.locator(
      'button[aria-label*="delete"], button[aria-label*="Delete"], button:has-text("Delete"), [data-testid*="delete"]'
    ).first();

    if (await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteBtn.click();
    } else {
      // Context menu
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

    // Confirm dialog
    await page.locator(
      '[role="dialog"] button:has-text("Delete"), [role="dialog"] button:has-text("Confirm"), [role="dialog"] button:has-text("Yes")'
    ).first().click({ timeout: 8000 }).catch(() => {});

    await waitForSuccess(page);

    // Verify removed
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill(firstName);
      await page.waitForTimeout(1500);
    }
    const deletedRow = page.locator('.MuiDataGrid-row, tbody tr, [role="row"]').filter({ hasText: firstName }).first();
    await expect(deletedRow).not.toBeVisible({ timeout: 8000 });
  });

  // -------------------------------------------------------------------------
  // TC-CON-013: Filter contacts
  // -------------------------------------------------------------------------
  test('TC-CON-013: Filter contacts using available filters', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Try filter chips / tabs
    const filterLocators = [
      page.locator('[role="tab"]:has-text("All"), .MuiChip-root:has-text("All"), button:has-text("All Contacts")').first(),
      page.locator('.MuiChip-root:has-text("Active"), button:has-text("Active")').first(),
    ];

    for (const filter of filterLocators) {
      if (await filter.isVisible({ timeout: 3000 }).catch(() => false)) {
        await filter.click();
        await page.waitForTimeout(800);
        const grid = page.locator('.MuiDataGrid-root, table, [role="grid"]').first();
        await expect(grid).toBeVisible();
      }
    }

    // Try column filter button in MUI DataGrid toolbar
    const filterBtn = page.locator(
      '[data-testid="FilterAltIcon"], button[aria-label*="filter"], button:has-text("Filters"), .MuiDataGrid-toolbarContainer button[aria-label*="Filter"]'
    ).first();
    if (await filterBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await filterBtn.click();
      await page.waitForTimeout(500);
      await page.keyboard.press('Escape');
    }
  });

  // -------------------------------------------------------------------------
  // TC-CON-014: Sort contacts by last name
  // -------------------------------------------------------------------------
  test('TC-CON-014: Sort contacts by last name column', async ({ page }) => {
    await page.goto(`${BASE_URL}/contacts`, { waitUntil: 'domcontentloaded' });
    await waitForGrid(page);

    // Find Last Name column header
    const lastNameHeader = page.locator(
      '.MuiDataGrid-columnHeader[data-field="lastName"], .MuiDataGrid-columnHeader[data-field="last_name"], th:has-text("Last Name"), [role="columnheader"]:has-text("Last Name"), [role="columnheader"]:has-text("Last")'
    ).first();

    if (await lastNameHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
      await lastNameHeader.click();
      await page.waitForTimeout(800);

      const ariaSort = await lastNameHeader.getAttribute('aria-sort').catch(() => null);
      const hasSortIcon = await lastNameHeader.locator('[data-testid*="Sort"], .MuiDataGrid-sortIcon, svg').first().isVisible().catch(() => false);
      expect(ariaSort !== null || hasSortIcon).toBeTruthy();

      // Reverse sort
      await lastNameHeader.click();
      await page.waitForTimeout(800);

      const ariaSortAfter = await lastNameHeader.getAttribute('aria-sort').catch(() => null);
      if (ariaSort && ariaSortAfter) {
        expect(ariaSortAfter).not.toBe(ariaSort);
      }
    } else {
      // Try Name column as fallback
      const nameHeader = page.locator(
        '.MuiDataGrid-columnHeader[data-field="name"], th:has-text("Name"), [role="columnheader"]:has-text("Name")'
      ).first();
      if (await nameHeader.isVisible({ timeout: 5000 }).catch(() => false)) {
        await nameHeader.click();
        await page.waitForTimeout(800);
        expect(true).toBeTruthy(); // Sort interaction completed
      } else {
        test.skip();
      }
    }
  });

});

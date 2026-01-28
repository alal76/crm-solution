/**
 * Debug script to test Customer creation and see validation errors
 */

import { test } from '@playwright/test';

test('Debug Customer Creation', async ({ page }) => {
  console.log('\n========== Testing Customer Creation ==========');
  
  await page.goto('/customers');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(1000);
  
  // Click Add Account button
  const addButton = page.locator('button:has-text("Add Account")');
  await addButton.click();
  
  await page.waitForTimeout(2000);
  
  // Take screenshot of the form
  await page.screenshot({ path: 'debug-customer-form-empty.png', fullPage: true });
  
  console.log('Dialog opened. Checking available fields...');
  
  // List all visible inputs with their labels
  const inputs = await page.locator('.MuiDialogContent-root input, .MuiDialogContent-root textarea').all();
  console.log(`Found ${inputs.length} inputs in dialog`);
  
  for (let i = 0; i < Math.min(inputs.length, 30); i++) {
    try {
      const input = inputs[i];
      const name = await input.getAttribute('name').catch(() => null);
      const type = await input.getAttribute('type').catch(() => null);
      const id = await input.getAttribute('id').catch(() => null);
      const ariaLabel = await input.getAttribute('aria-label').catch(() => null);
      const isRequired = await input.getAttribute('aria-required').catch(() => null);
      
      // Try to find associated label
      let labelText = '';
      if (id) {
        labelText = await page.locator(`label[for="${id}"]`).textContent().catch(() => '');
      }
      
      console.log(`  Input[${i}]: name="${name}" type="${type}" id="${id}" label="${labelText}" required="${isRequired}"`);
    } catch (e) {
      console.log(`  Input[${i}]: Error reading`);
    }
  }
  
  // Look for category/type selector
  const categorySelect = page.locator('.MuiDialogContent-root select, .MuiDialogContent-root [role="combobox"]');
  const selectCount = await categorySelect.count();
  console.log(`\nFound ${selectCount} select/combobox elements`);
  
  // Now try to fill the form
  console.log('\n--- Attempting to fill form ---');
  
  // Fill company name
  const companyInput = page.locator('input[name="company"]');
  if (await companyInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await companyInput.fill('Test Company Debug');
    console.log('Filled company input');
  }
  
  // Fill firstName
  const firstNameInput = page.locator('input[name="firstName"]');
  if (await firstNameInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await firstNameInput.fill('TestFirst');
    console.log('Filled firstName input');
  }
  
  // Fill lastName
  const lastNameInput = page.locator('input[name="lastName"]');
  if (await lastNameInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await lastNameInput.fill('TestLast');
    console.log('Filled lastName input');
  }
  
  // Fill email
  const emailInput = page.locator('input[name="email"]');
  if (await emailInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await emailInput.fill('test@debug.local');
    console.log('Filled email input');
  }
  
  // Fill phone
  const phoneInput = page.locator('input[name="phone"]');
  if (await phoneInput.isVisible({ timeout: 1000 }).catch(() => false)) {
    await phoneInput.fill('+15551234567');
    console.log('Filled phone input');
  }
  
  // Take screenshot after filling
  await page.screenshot({ path: 'debug-customer-form-filled.png', fullPage: true });
  
  // Click save button
  console.log('\n--- Clicking Save button ---');
  const saveButton = page.locator('button:has-text("Save")');
  await saveButton.click();
  
  await page.waitForTimeout(3000);
  
  // Check for error messages
  const errorAlert = page.locator('.MuiAlert-standardError, .MuiAlert-root[severity="error"]');
  if (await errorAlert.isVisible({ timeout: 1000 }).catch(() => false)) {
    const errorText = await errorAlert.textContent();
    console.log(`ERROR ALERT: ${errorText}`);
  }
  
  // Check for field-level errors
  const fieldErrors = page.locator('.MuiFormHelperText-root.Mui-error');
  const errorCount = await fieldErrors.count();
  console.log(`Found ${errorCount} field-level errors`);
  
  for (let i = 0; i < errorCount; i++) {
    const errorText = await fieldErrors.nth(i).textContent();
    console.log(`  Field error[${i}]: ${errorText}`);
  }
  
  // Take final screenshot
  await page.screenshot({ path: 'debug-customer-form-after-save.png', fullPage: true });
  
  // Check if dialog is still open
  const dialogVisible = await page.locator('.MuiDialogContent-root').isVisible();
  console.log(`\nDialog still visible: ${dialogVisible}`);
});

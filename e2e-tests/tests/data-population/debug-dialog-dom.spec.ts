/**
 * Debug script to capture what's on the page after clicking Add User
 */

import { test } from '@playwright/test';

test('Debug User Dialog DOM', async ({ page }) => {
  await page.goto('/admin/users');
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(1000);
  
  // Click Add User button
  const addButton = page.locator('button:has-text("Add User")');
  await addButton.click();
  
  await page.waitForTimeout(2000);
  
  // Capture the full page HTML structure of dialogs
  const dialogHTML = await page.evaluate(() => {
    const dialogs = document.querySelectorAll('[role="dialog"], .MuiDialog-root, .MuiDrawer-root, [role="presentation"]');
    return Array.from(dialogs).map((d, i) => ({
      index: i,
      tagName: d.tagName,
      role: d.getAttribute('role'),
      className: d.className,
      id: d.id,
      childCount: d.children.length,
      hasForm: d.querySelector('form') !== null,
      inputCount: d.querySelectorAll('input').length,
      buttonTexts: Array.from(d.querySelectorAll('button')).map(b => b.textContent?.trim()).slice(0, 10),
    }));
  });
  
  console.log('Dialog structures found:');
  for (const d of dialogHTML) {
    console.log(JSON.stringify(d, null, 2));
  }
  
  // Check for visible dialogs
  const visibleDialogs = await page.locator('[role="dialog"]:visible, .MuiDialog-root:visible, [role="presentation"]:visible').count();
  console.log(`\nVisible dialogs: ${visibleDialogs}`);
  
  // Check for MuiDialog-paper which is the actual dialog container
  const paperCount = await page.locator('.MuiDialog-paper, .MuiPaper-root').count();
  console.log(`MuiDialog-paper count: ${paperCount}`);
  
  // Check the actual visible input fields
  const visibleInputs = await page.locator('input:visible').count();
  console.log(`Visible inputs: ${visibleInputs}`);
  
  // Check for DialogTitle
  const dialogTitles = await page.locator('.MuiDialogTitle-root').allTextContents();
  console.log(`Dialog titles: ${JSON.stringify(dialogTitles)}`);
  
  await page.screenshot({ path: 'debug-user-dialog-detailed.png', fullPage: true });
});

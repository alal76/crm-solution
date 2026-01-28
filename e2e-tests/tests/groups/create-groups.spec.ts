import { test, expect } from '@playwright/test';
import path from 'path';

// Auth file is in e2e-tests/test-results/.auth/user.json
const authFile = path.join(__dirname, '../../test-results/.auth/user.json');

test.describe('Create Groups via UI', () => {
  test.use({ storageState: authFile });

  const groups = [
    { name: 'User', description: 'Basic user access with read permissions' },
    { name: 'Manager', description: 'Manager level with team oversight permissions' },
    { name: 'CRM Admin', description: 'Full CRM administration access' },
    { name: 'User Admin', description: 'User and group management permissions' },
  ];

  test('Create 4 groups: User, Manager, CRM Admin, User Admin', async ({ page }) => {
    // Navigate to groups settings
    await page.goto('/admin/groups');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    for (const group of groups) {
      console.log(`Creating group: ${group.name}`);
      
      // Click Create Group button
      const addButton = page.getByRole('button', { name: /create group/i });
      await addButton.waitFor({ state: 'visible', timeout: 10000 });
      await addButton.click();
      
      // Wait for dialog
      await page.waitForSelector('.MuiDialog-paper, .MuiDialogContent-root', { timeout: 5000 });
      await page.waitForTimeout(500);
      
      // Fill group name - MUI TextField uses label not name attribute
      const nameInput = page.getByLabel('Group Name');
      await nameInput.fill(group.name);
      
      // Fill description if field exists
      const descInput = page.getByLabel('Description');
      if (await descInput.isVisible().catch(() => false)) {
        await descInput.fill(group.description);
      }
      
      // Submit the form
      const saveButton = page.getByRole('button', { name: /save|create|add|submit/i }).last();
      await saveButton.click();
      
      // Wait for dialog to close and success
      await page.waitForTimeout(1500);
      
      // Verify the group appears in the list
      const groupInList = page.getByText(group.name, { exact: true });
      await expect(groupInList.first()).toBeVisible({ timeout: 5000 });
      
      console.log(`✓ Created group: ${group.name}`);
    }

    console.log('All 4 groups created successfully!');
  });
});

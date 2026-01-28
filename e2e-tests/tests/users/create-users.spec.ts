import { test, expect, Page } from '@playwright/test';
import path from 'path';

const authFile = path.join(__dirname, '../../test-results/.auth/user.json');

// Statistics tracking
const stats = { created: 0, failed: 0 };

// Groups to distribute users among (from seeded master data)
const groups = ['User', 'Manager', 'CRM Admin', 'User Admin'];

// Departments from seeded master data
const departments = ['Executive', 'Sales', 'Marketing', 'Engineering', 'Finance', 'Human Resources', 'IT', 'Operations', 'Customer Support', 'Legal'];

// Roles (hardcoded in the form)
const roles = ['User', 'Manager', 'Admin'];

// Helper functions
function randomString(length: number = 8): string {
  return Math.random().toString(36).substring(2, 2 + length);
}

const firstNames = [
  'James', 'Mary', 'Robert', 'Patricia', 'John', 'Jennifer', 'Michael', 'Linda',
  'David', 'Elizabeth', 'William', 'Barbara', 'Richard', 'Susan', 'Joseph', 'Jessica',
  'Thomas', 'Sarah', 'Christopher', 'Karen'
];

const lastNames = [
  'Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis',
  'Rodriguez', 'Martinez', 'Hernandez', 'Lopez', 'Gonzalez', 'Wilson', 'Anderson',
  'Thomas', 'Taylor', 'Moore', 'Jackson', 'Martin'
];

// Generate 20 users with random group, department, and role assignments
function generateUsers(count: number) {
  const users = [];
  for (let i = 1; i <= count; i++) {
    const firstName = firstNames[(i - 1) % firstNames.length];
    const lastName = lastNames[(i - 1) % lastNames.length];
    const uniqueSuffix = randomString(4);
    
    users.push({
      username: `${firstName.toLowerCase()}.${lastName.toLowerCase()}_${uniqueSuffix}`,
      email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}_${uniqueSuffix}@testcrm.local`,
      password: 'TestPass123!@#',
      firstName: firstName,
      lastName: lastName,
      department: departments[i % departments.length],
      group: groups[i % groups.length],
      role: roles[i % roles.length],
    });
  }
  return users;
}

// Helper to fill MUI TextField by label
async function fillByLabel(page: Page, label: string, value: string): Promise<boolean> {
  try {
    const field = page.getByLabel(label, { exact: false });
    if (await field.isVisible({ timeout: 1000 })) {
      await field.fill(value);
      return true;
    }
  } catch (e) {}
  return false;
}

// Helper to select MUI dropdown by label
async function selectByLabel(page: Page, label: string, optionText: string): Promise<boolean> {
  try {
    // Find the select/combobox by label
    const selectField = page.getByLabel(label, { exact: false });
    if (await selectField.isVisible({ timeout: 1000 })) {
      await selectField.click();
      await page.waitForTimeout(300);
      
      // Find and click the option in the dropdown
      const option = page.locator(`[role="option"]:has-text("${optionText}"), li:has-text("${optionText}")`).first();
      if (await option.isVisible({ timeout: 2000 })) {
        await option.click();
        await page.waitForTimeout(200);
        return true;
      }
    }
  } catch (e) {
    console.log(`    ⚠️ Could not select ${optionText} for ${label}`);
  }
  return false;
}

test.describe('Create 20 Users via UI', () => {
  test.use({ storageState: authFile });
  test.setTimeout(300000); // 5 minutes

  test.afterAll(async () => {
    console.log('\n' + '='.repeat(50));
    console.log('📊 USER CREATION SUMMARY');
    console.log('='.repeat(50));
    console.log(`Created: ${stats.created}`);
    console.log(`Failed:  ${stats.failed}`);
    console.log('='.repeat(50));
  });

  test('Create 20 users with random group, department, and role distribution', async ({ page }) => {
    console.log('\n📝 Creating 20 Users distributed among groups, departments, and roles...');
    console.log('Groups:', groups.join(', '));
    console.log('Departments:', departments.slice(0, 5).join(', '), '...');
    console.log('Roles:', roles.join(', '));
    console.log('');
    
    const users = generateUsers(20);
    
    // Count distributions
    const groupCounts: Record<string, number> = {};
    groups.forEach(g => groupCounts[g] = 0);
    users.forEach(u => groupCounts[u.group]++);
    console.log('Group distribution:', groupCounts);
    console.log('');

    for (const user of users) {
      try {
        // Navigate to users page
        await page.goto('/admin/users');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);
        
        // Click Add User button
        const addButton = page.getByRole('button', { name: /add user/i });
        await addButton.waitFor({ state: 'visible', timeout: 10000 });
        await addButton.click();
        
        // Wait for dialog
        await page.waitForSelector('.MuiDialog-paper, .MuiDialogContent-root', { timeout: 5000 });
        await page.waitForTimeout(500);
        
        // Fill user form
        await fillByLabel(page, 'Username', user.username);
        await fillByLabel(page, 'Email', user.email);
        await fillByLabel(page, 'First Name', user.firstName);
        await fillByLabel(page, 'Last Name', user.lastName);
        await fillByLabel(page, 'Password', user.password);
        await fillByLabel(page, 'Confirm Password', user.password);
        
        // Select role from dropdown
        await selectByLabel(page, 'Role', user.role);
        
        // Select department from dropdown
        await selectByLabel(page, 'Department', user.department);
        
        // Select primary group from dropdown
        await selectByLabel(page, 'Primary Group', user.group);
        
        // Submit the form
        const saveButton = page.getByRole('button', { name: /save|create|submit/i }).last();
        await saveButton.click();
        
        // Wait for dialog to close
        await page.waitForTimeout(2000);
        
        // Check for errors
        const errorAlert = page.locator('.MuiAlert-standardError, [role="alert"]:has-text("error")').first();
        if (await errorAlert.isVisible().catch(() => false)) {
          const errorText = await errorAlert.textContent().catch(() => 'Unknown error');
          throw new Error(`Validation error: ${errorText?.substring(0, 50)}`);
        }
        
        // Check if dialog closed (success indicator)
        const dialogStillOpen = await page.locator('.MuiDialog-paper:visible').isVisible().catch(() => false);
        if (!dialogStillOpen) {
          stats.created++;
          console.log(`  ✅ [${user.group}/${user.department}/${user.role}] ${user.firstName} ${user.lastName}`);
        } else {
          // Try to close dialog and mark as failed
          await page.keyboard.press('Escape');
          throw new Error('Dialog did not close after save');
        }
        
      } catch (error) {
        stats.failed++;
        const errorMessage = error instanceof Error ? error.message : 'Unknown error';
        console.log(`  ❌ Failed: ${user.firstName} ${user.lastName} - ${errorMessage}`);
        
        // Make sure dialog is closed before next iteration
        await page.keyboard.press('Escape').catch(() => {});
        await page.waitForTimeout(300);
      }
    }
    
    // Final summary
    console.log(`\n📊 Result: ${stats.created} users created, ${stats.failed} failed`);
    
    // Assert we created at least most of the users
    expect(stats.created).toBeGreaterThan(15);
  });
});

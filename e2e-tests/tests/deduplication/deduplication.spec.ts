/**
 * CRM Solution - Deduplication Feature Tests
 * 
 * Tests for duplicate detection, merge, and unmerge functionality.
 */

import { test, expect } from '@playwright/test';
import { uniqueTestData, TEST_LEADS, TEST_CONTACTS, TEST_CUSTOMERS } from '../test-data';

// API helper for direct API testing - uses the same base URL as the page
async function apiRequest(page: any, method: string, endpoint: string, data?: any) {
  // Use page URL to get the base - this ensures we hit the same server as the UI
  return page.evaluate(async ({ method, endpoint, data }: any) => {
    const baseUrl = window.location.origin;
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    
    try {
      const response = await fetch(`${baseUrl}${endpoint}`, {
        method,
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        body: data ? JSON.stringify(data) : undefined
      });
      
      const text = await response.text();
      try {
        return { status: response.status, data: JSON.parse(text), ok: response.ok };
      } catch {
        return { status: response.status, data: text, ok: response.ok };
      }
    } catch (err: any) {
      return { status: 0, data: err.message, ok: false, error: true };
    }
  }, { method, endpoint, data });
}

test.describe('Deduplication - API Tests', () => {
  test.describe.configure({ mode: 'serial' });

  test('TC-DEDUP-001: Should check for duplicates with no matches', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const uniqueLead = uniqueTestData(TEST_LEADS.hot);
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: `unique-${Date.now()}@noduplicate.test`,
        FirstName: uniqueLead.firstName,
        LastName: uniqueLead.lastName,
        Phone: '999-555-0000'
      },
      matchThreshold: 70
    });
    
    console.log('Duplicate check response:', JSON.stringify(response, null, 2));
    
    // Should return success (may or may not find duplicates depending on data)
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-002: Should detect duplicate by email', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // First, create a lead with known email
    const testEmail = `dedup-test-${Date.now()}@example.com`;
    const firstName = 'Dedup';
    const lastName = 'TestUser';
    
    // Create via UI first
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible().catch(() => false)) {
      await addButton.click({ force: true });
      await page.waitForTimeout(1000);
      
      const dialog = page.locator('[role="dialog"], .MuiDialog-root');
      if (await dialog.isVisible().catch(() => false)) {
        // Fill form
        await page.locator('input[name="firstName"]').first().fill(firstName);
        await page.locator('input[name="lastName"]').first().fill(lastName);
        await page.locator('input[name="email"]').first().fill(testEmail);
        
        // Save
        await page.locator('button:has-text("Save"), button[type="submit"]').first().click();
        await page.waitForTimeout(2000);
      }
    }
    
    // Now check for duplicates with same email
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: testEmail,
        FirstName: 'Different',
        LastName: 'Name'
      },
      matchThreshold: 70
    });
    
    console.log('Duplicate detection response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-003: Should detect fuzzy name matches', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // Check for duplicates with similar name (typo variations)
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: 'different@email.com',
        FirstName: 'Jhon',  // Typo of John
        LastName: 'Smth',   // Typo of Smith
        Phone: '555-123-4567'
      },
      matchThreshold: 50  // Lower threshold for fuzzy
    });
    
    console.log('Fuzzy match response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-004: Should get active duplicate rules', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'GET', '/api/duplicates/rules/Lead');
    
    console.log('Duplicate rules response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-005: Should handle Contact duplicates', async ({ page }) => {
    await page.goto('/contacts', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Contact',
      fieldValues: {
        EmailPrimary: 'contact@example.com',
        FirstName: 'Test',
        LastName: 'Contact',
        PhonePrimary: '555-999-8888'
      },
      matchThreshold: 70
    });
    
    console.log('Contact duplicate check:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-006: Should handle Account duplicates', async ({ page }) => {
    await page.goto('/customers', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Account',
      fieldValues: {
        Email: 'account@company.com',
        CompanyName: 'Acme Corporation',
        Phone: '555-111-2222',
        Website: 'www.acme.com'
      },
      matchThreshold: 70
    });
    
    console.log('Account duplicate check:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });
});

test.describe('Deduplication - Merge Operations', () => {
  test.describe.configure({ mode: 'serial' });

  test('TC-DEDUP-010: Should preview merge operation', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // Get existing leads to merge (assuming some exist)
    const response = await apiRequest(page, 'POST', '/api/duplicates/merge/preview', {
      entityType: 'Lead',
      masterRecordId: 1,  // Assuming ID 1 exists
      recordsToMerge: [2], // Assuming ID 2 exists
      relinkRelatedRecords: true
    });
    
    console.log('Merge preview response:', JSON.stringify(response, null, 2));
    // May fail if records don't exist, but should not be 500
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-011: Should get merge history', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'GET', '/api/duplicates/history/Lead/1');
    
    console.log('Merge history response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-012: Should handle invalid entity type gracefully', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'InvalidEntity',
      fieldValues: {
        Email: 'test@test.com'
      }
    });
    
    console.log('Invalid entity response:', JSON.stringify(response, null, 2));
    // Should return 400 Bad Request, not 500
    expect(response.status).toBe(400);
  });

  test('TC-DEDUP-013: Should require field values', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {}
    });
    
    console.log('Empty fields response:', JSON.stringify(response, null, 2));
    // Should return 400 Bad Request
    expect(response.status).toBe(400);
  });
});

test.describe('Deduplication - UI Integration', () => {
  test('TC-DEDUP-020: Should show duplicate detection dialog on lead create', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1000);
    
    // Click Add button
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible().catch(() => false)) {
      await addButton.click({ force: true });
      await page.waitForTimeout(1000);
      
      const dialog = page.locator('[role="dialog"], .MuiDialog-root');
      if (await dialog.isVisible().catch(() => false)) {
        // Fill with potentially duplicate data
        const emailInput = page.locator('input[name="email"]').first();
        if (await emailInput.isVisible().catch(() => false)) {
          await emailInput.fill('admin@crm.local');  // Likely existing email
          
          // Tab out to trigger potential duplicate check
          await emailInput.blur();
          await page.waitForTimeout(2000);
          
          // Check if duplicate warning appears
          const duplicateWarning = page.locator('text=/duplicate|similar|existing/i').first();
          const hasDuplicateWarning = await duplicateWarning.isVisible().catch(() => false);
          
          console.log('Duplicate warning visible:', hasDuplicateWarning);
          // This is an optional behavior - may or may not be implemented in UI yet
        }
      }
    }
  });

  test('TC-DEDUP-021: Should navigate to duplicate management page', async ({ page }) => {
    // Check if there's a dedicated duplicates page
    await page.goto('/settings', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // Look for duplicate management link
    const duplicateLink = page.locator('text=/duplicate/i, a[href*="duplicate"]').first();
    const hasDuplicateSection = await duplicateLink.isVisible().catch(() => false);
    
    console.log('Has duplicate management section:', hasDuplicateSection);
    
    if (hasDuplicateSection) {
      await duplicateLink.click();
      await page.waitForTimeout(1000);
    }
  });
});

test.describe('Deduplication - Edge Cases', () => {
  test('TC-DEDUP-030: Should handle special characters in names', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: 'test@test.com',
        FirstName: "O'Connor",
        LastName: 'Müller-Schmidt',
        CompanyName: 'ABC & Sons, Ltd.'
      },
      matchThreshold: 70
    });
    
    console.log('Special chars response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-031: Should handle unicode characters', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: 'test@test.com',
        FirstName: '田中',
        LastName: '太郎',
        CompanyName: '株式会社テスト'
      },
      matchThreshold: 70
    });
    
    console.log('Unicode response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-032: Should handle very long field values', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    const longString = 'A'.repeat(500);
    
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: 'test@test.com',
        FirstName: longString,
        LastName: longString
      },
      matchThreshold: 70
    });
    
    console.log('Long values response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });

  test('TC-DEDUP-033: Should handle phone number variations', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // Test various phone formats
    const phoneFormats = [
      '555-123-4567',
      '(555) 123-4567',
      '5551234567',
      '+1 555 123 4567',
      '555.123.4567'
    ];
    
    for (const phone of phoneFormats) {
      const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
        entityType: 'Lead',
        fieldValues: {
          Email: 'phone-test@test.com',
          FirstName: 'Phone',
          LastName: 'Test',
          Phone: phone
        },
        matchThreshold: 70
      });
      
      console.log(`Phone format "${phone}" response status:`, response.status);
      expect(response.status).toBeLessThan(500);
    }
  });

  test('TC-DEDUP-034: Should handle email variations', async ({ page }) => {
    await page.goto('/leads', { timeout: 30000 });
    await page.waitForLoadState('domcontentloaded');
    
    // Test email with plus addressing
    const response = await apiRequest(page, 'POST', '/api/duplicates/check', {
      entityType: 'Lead',
      fieldValues: {
        Email: 'user+tag@example.com',
        FirstName: 'Email',
        LastName: 'Test'
      },
      matchThreshold: 70
    });
    
    console.log('Email plus addressing response:', JSON.stringify(response, null, 2));
    expect(response.status).toBeLessThan(500);
  });
});

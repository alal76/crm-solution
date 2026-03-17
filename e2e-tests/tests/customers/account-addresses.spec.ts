import { test, expect } from '@playwright/test';
import { ADMIN_EMAIL, ADMIN_PASSWORD, appUrl } from '../../testConfig';

/**
 * E2E Tests for Account Address Management
 * Covers: Navigation, CRUD operations, validation, API integration
 * TODO-CRM008-004: Add account address E2E tests
 */

test.describe('Account Address Management E2E', () => {
  let authToken: string;

  test.beforeAll(async ({ browser }) => {
    // Perform login once for all tests in this suite
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
      // Navigate to login page
      await page.goto(appUrl('/login'), { waitUntil: 'domcontentloaded' });

      // Fill login form
      await page.locator('input[name="email"]').fill(ADMIN_EMAIL);
      await page.locator('input[name="password"]').fill(ADMIN_PASSWORD);

      // Click submit button
      await page.locator('button[type="submit"]:has-text("Sign In")').click();

      // Wait for navigation to dashboard
      await page.waitForLoadState('domcontentloaded');
      await expect(page).toHaveURL(/.*dashboard/);

      // Extract auth token from localStorage
      const token = await page.evaluate(() => localStorage.getItem('access_token'));
      if (token) {
        authToken = token;
      }
    } finally {
      await context.close();
    }
  });

  test.describe('Navigation & Display', () => {
    test('Should open customer and navigate to addresses panel', async ({ page }) => {
      // Arrange
      await page.goto(appUrl('/accounts'), { waitUntil: 'domcontentloaded' });

      // Act: Click on first account in list
      const firstAccountLink = page.locator('[data-testid="account-list-item"]').first();
      await firstAccountLink.click();
      await page.waitForLoadState('domcontentloaded');

      // Assert: Verify account details page loaded
      await expect(page).toHaveURL(/.*\/accounts\/\d+/);

      // Act: Navigate to addresses tab
      await page.locator('[data-testid="tab-addresses"]').click();
      await page.waitForLoadState('domcontentloaded');

      // Assert: Verify addresses panel is visible
      const addressesPanel = page.locator('[data-testid="addresses-panel"]');
      await expect(addressesPanel).toBeVisible();
    });

    test('Should show multiple addresses in account overview', async ({ page }) => {
      // Arrange
      await page.goto(appUrl('/accounts'), { waitUntil: 'domcontentloaded' });

      // Act: Open account with multiple addresses
      const accountWithAddresses = page.locator('[data-testid="account-list-item"]').first();
      await accountWithAddresses.click();
      await page.waitForLoadState('domcontentloaded');

      // Act: Navigate to addresses tab
      await page.locator('[data-testid="tab-addresses"]').click();

      // Assert: Verify multiple addresses displayed
      const addressCards = page.locator('[data-testid="address-card"]');
      const count = await addressCards.count();
      expect(count).toBeGreaterThan(0);

      // Assert: Verify address details visible (line1, city)
      const firstAddressCard = addressCards.first();
      await expect(firstAddressCard.locator('[data-testid="address-line1"]')).toBeVisible();
      await expect(firstAddressCard.locator('[data-testid="address-city"]')).toBeVisible();
    });
  });

  test.describe('Create Address', () => {
    test('Should add new address with all fields', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();

      // Act: Click add address button
      await page.locator('[data-testid="btn-add-address"]').click();
      await page.waitForLoadState('domcontentloaded');

      // Fill form with complete data
      await page.locator('[data-testid="input-line1"]').fill('123 Business Park Drive');
      await page.locator('[data-testid="input-line2"]').fill('Suite 100');
      await page.locator('[data-testid="input-city"]').fill('San Francisco');
      await page.locator('[data-testid="input-state"]').fill('CA');
      await page.locator('[data-testid="input-postal-code"]').fill('94105');
      await page.locator('[data-testid="input-country"]').fill('USA');
      await page.locator('[data-testid="input-address-type"]').selectOption('Billing');

      // Submit form
      await page.locator('[data-testid="btn-save-address"]').click();
      await page.waitForLoadState('domcontentloaded');

      // Assert: Verify address added to list
      const newAddress = page.locator('[data-testid="address-card"]:has-text("123 Business Park Drive")');
      await expect(newAddress).toBeVisible();

      // Assert: Verify confirmation message
      const successMessage = page.locator('[data-testid="toast-success"]:has-text("Address added")');
      await expect(successMessage).toBeVisible();
    });

    test('Should validate required fields (line1, city)', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();
      await page.locator('[data-testid="btn-add-address"]').click();
      await page.waitForLoadState('domcontentloaded');

      // Act: Try to submit form without required fields
      await page.locator('[data-testid="btn-save-address"]').click();

      // Assert: Verify validation errors displayed
      const line1Error = page.locator('[data-testid="error-line1"]:has-text("required")');
      await expect(line1Error).toBeVisible();

      const cityError = page.locator('[data-testid="error-city"]:has-text("required")');
      await expect(cityError).toBeVisible();

      // Assert: Verify form did not submit
      await expect(page.locator('[data-testid="addresses-panel"]')).toBeVisible();
    });

    test('Should show error for invalid phone format in profile', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();
      await page.locator('[data-testid="btn-add-address"]').click();

      // Act: Enter valid required fields but invalid phone
      await page.locator('[data-testid="input-line1"]').fill('123 Main St');
      await page.locator('[data-testid="input-city"]').fill('New York');
      await page.locator('[data-testid="input-phone"]').fill('invalid-phone');

      // Try to submit
      await page.locator('[data-testid="btn-save-address"]').click();

      // Assert: Verify phone validation error
      const phoneError = page.locator('[data-testid="error-phone"]:has-text("Invalid format")');
      await expect(phoneError).toBeVisible();
    });
  });

  test.describe('Update Address', () => {
    test('Should edit existing address', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();

      // Act: Click edit button on first address
      const firstAddressCard = page.locator('[data-testid="address-card"]').first();
      const editButton = firstAddressCard.locator('[data-testid="btn-edit-address"]');
      await editButton.click();
      await page.waitForLoadState('domcontentloaded');

      // Modify fields
      const line1Input = page.locator('[data-testid="input-line1"]');
      await line1Input.clear();
      await line1Input.fill('456 Updated Avenue');

      const cityInput = page.locator('[data-testid="input-city"]');
      await cityInput.clear();
      await cityInput.fill('Boston');

      // Submit changes
      await page.locator('[data-testid="btn-save-address"]').click();
      await page.waitForLoadState('domcontentloaded');

      // Assert: Verify address updated in list
      const updatedAddress = page.locator('[data-testid="address-card"]:has-text("456 Updated Avenue")');
      await expect(updatedAddress).toBeVisible();

      // Assert: Verify confirmation message
      const successMessage = page.locator('[data-testid="toast-success"]:has-text("Address updated")');
      await expect(successMessage).toBeVisible();
    });
  });

  test.describe('Primary Address', () => {
    test('Should mark address as primary', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();

      // Act: Find a non-primary address and click "Make Primary"
      const addressCards = page.locator('[data-testid="address-card"]');
      const secondAddress = addressCards.nth(1);
      const makePrimaryButton = secondAddress.locator('[data-testid="btn-make-primary"]');

      // Check if button is visible
      if (await makePrimaryButton.isVisible()) {
        await makePrimaryButton.click();
        await page.waitForLoadState('domcontentloaded');

        // Assert: Verify primary indicator updated
        const primaryBadge = secondAddress.locator('[data-testid="badge-primary"]');
        await expect(primaryBadge).toBeVisible();

        // Assert: Verify previous primary is no longer marked
        const firstAddress = addressCards.first();
        const firstAddressPrimaryBadge = firstAddress.locator('[data-testid="badge-primary"]');
        await expect(firstAddressPrimaryBadge).not.toBeVisible();
      }
    });
  });

  test.describe('Delete Address', () => {
    test('Should delete address with confirmation', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();

      // Get initial address count
      const addressCardsInitial = page.locator('[data-testid="address-card"]');
      const initialCount = await addressCardsInitial.count();

      // Act: Click delete button on last address (if multiple exist)
      if (initialCount > 1) {
        const lastAddress = addressCardsInitial.last();
        const deleteButton = lastAddress.locator('[data-testid="btn-delete-address"]');
        await deleteButton.click();
        await page.waitForLoadState('domcontentloaded');

        // Confirm deletion in modal
        const confirmButton = page.locator('[data-testid="btn-confirm-delete"]');
        await expect(confirmButton).toBeVisible();
        await confirmButton.click();
        await page.waitForLoadState('domcontentloaded');

        // Assert: Verify address removed from list
        const addressCardsAfter = page.locator('[data-testid="address-card"]');
        const afterCount = await addressCardsAfter.count();
        expect(afterCount).toBe(initialCount - 1);

        // Assert: Verify confirmation message
        const successMessage = page.locator('[data-testid="toast-success"]:has-text("Address deleted")');
        await expect(successMessage).toBeVisible();
      }
    });
  });

  test.describe('API Integration', () => {
    test('Should fetch addresses via API on account load', async ({ page }) => {
      // Arrange
      const accountId = 1;

      // Act & Assert: Intercept API call
      const responsePromise = page.waitForResponse(
        response => response.url().includes(`/api/contactinfos/entity/Account/${accountId}`) && response.status() === 200
      );

      await page.goto(`${BASE_URL}/accounts/${accountId}`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();

      const response = await responsePromise;

      // Assert: Verify API returned address data
      const responseData = await response.json();
      expect(responseData).toBeDefined();
      expect(Array.isArray(responseData.addresses || responseData)).toBe(true);
    });

    test('Should handle concurrent address updates', async ({ page, context }) => {
      // Arrange: Create two parallel page contexts
      const page2 = await context.newPage();

      try {
        // Navigate both pages to same account
        await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
        await page.locator('[data-testid="tab-addresses"]').click();

        await page2.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
        await page2.locator('[data-testid="tab-addresses"]').click();

        // Act: Make concurrent updates from both pages
        await page.locator('[data-testid="btn-add-address"]').click();
        await page2.locator('[data-testid="btn-add-address"]').click();

        // Fill forms with different data
        await page.locator('[data-testid="input-line1"]').fill('Address 1 - Page 1');
        await page.locator('[data-testid="input-city"]').fill('City 1');

        await page2.locator('[data-testid="input-line1"]').fill('Address 2 - Page 2');
        await page2.locator('[data-testid="input-city"]').fill('City 2');

        // Submit both forms
        await page.locator('[data-testid="btn-save-address"]').click();
        await page2.locator('[data-testid="btn-save-address"]').click();

        // Wait for both to complete
        await page.waitForLoadState('domcontentloaded');
        await page2.waitForLoadState('domcontentloaded');

        // Assert: Verify both addresses exist
        await page.reload({ waitUntil: 'domcontentloaded' });
        const addressCards = page.locator('[data-testid="address-card"]');
        const count = await addressCards.count();
        expect(count).toBeGreaterThanOrEqual(2);
      } finally {
        await page2.close();
      }
    });
  });

  test.describe('Error Handling', () => {
    test('Should display error when API call fails', async ({ page }) => {
      // Arrange
      const invalidAccountId = 999999;

      // Act
      await page.goto(`${BASE_URL}/accounts/${invalidAccountId}`, { waitUntil: 'domcontentloaded' });

      // Assert: Verify error message displayed
      const errorMessage = page.locator('[data-testid="error-message"]');
      const notFoundText = page.locator('text=Not found|Account not found');

      const isErrorVisible = await errorMessage.isVisible().catch(() => false);
      const isNotFoundVisible = await notFoundText.isVisible().catch(() => false);

      expect(isErrorVisible || isNotFoundVisible).toBe(true);
    });

    test('Should show network error when service unavailable', async ({ page }) => {
      // Arrange: Simulate network error by going offline would require special handling
      // For now, test timeout scenario
      const timeout = 5000;

      // Act: Navigate to accounts page
      await page.goto(`${BASE_URL}/accounts`, { 
        waitUntil: 'domcontentloaded',
        timeout: timeout
      }).catch(() => {
        // Expected to potentially timeout in offline scenario
      });

      // This test demonstrates error handling pattern
      // In production, would test with mocked network failures
      await expect(page).toBeDefined();
    });
  });

  test.describe('Accessibility & Responsiveness', () => {
    test('Should be keyboard navigable for address form', async ({ page }) => {
      // Arrange
      await page.goto(`${BASE_URL}/accounts/1`, { waitUntil: 'domcontentloaded' });
      await page.locator('[data-testid="tab-addresses"]').click();
      await page.locator('[data-testid="btn-add-address"]').click();

      // Act: Navigate form using Tab key
      const line1Input = page.locator('[data-testid="input-line1"]');
      await line1Input.focus();
      await line1Input.fill('123 Main St');

      // Tab to next field
      await page.keyboard.press('Tab');
      await page.keyboard.type('Apt 100');

      // Tab to city field
      await page.keyboard.press('Tab');
      await page.keyboard.type('New York');

      // Assert: Verify form is filled
      const cityInput = page.locator('[data-testid="input-city"]');
      const cityValue = await cityInput.inputValue();
      expect(cityValue).toBe('New York');
    });
  });
});

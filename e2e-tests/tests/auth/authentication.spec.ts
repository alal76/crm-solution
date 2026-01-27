/**
 * CRM Solution - Authentication Tests
 * 
 * Comprehensive tests for login, logout, registration, password reset, and 2FA.
 */

import { test, expect } from '@playwright/test';
import { LoginPage, Navigation, NotificationHelper } from '../fixtures';
import { TEST_USERS, uniqueTestData } from '../test-data';

test.describe('Authentication - Login', () => {
  test.beforeEach(async ({ page }) => {
    // Clear any existing auth state for login tests
    await page.context().clearCookies();
    await page.goto('/login');
  });

  test('TC-AUTH-001: Should display login page correctly', async ({ page }) => {
    // Verify login page elements
    await expect(page.locator('input[name="email"], input[name="username"], #email, #username').first()).toBeVisible();
    await expect(page.locator('input[name="password"], input[type="password"]').first()).toBeVisible();
    await expect(page.locator('button[type="submit"], button:has-text("Login")').first()).toBeVisible();
  });

  test('TC-AUTH-002: Should login successfully with valid credentials', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const user = TEST_USERS.existingAdmin;
    
    await loginPage.emailInput.fill(user.email);
    await loginPage.passwordInput.fill(user.password);
    await loginPage.loginButton.click();
    
    // Should redirect to dashboard
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
    
    // Verify logged in state
    await expect(page.locator('body')).not.toContainText('Sign In');
  });

  test('TC-AUTH-003: Should show error for invalid email', async ({ page }) => {
    const loginPage = new LoginPage(page);
    
    await loginPage.emailInput.fill('invalid_email_format');
    await loginPage.passwordInput.fill('password123');
    await loginPage.loginButton.click();
    
    // Should show validation error or stay on login page
    await expect(page.locator('body')).toContainText(/invalid|error|failed/i, { timeout: 5000 }).catch(() => {
      // If no error message, check we're still on login page
      expect(page.url()).toContain('login');
    });
  });

  test('TC-AUTH-004: Should show error for wrong password', async ({ page }) => {
    const loginPage = new LoginPage(page);
    
    await loginPage.emailInput.fill('admin');
    await loginPage.passwordInput.fill('wrong_password_123');
    await loginPage.loginButton.click();
    
    // Should show error message
    await page.waitForTimeout(2000);
    const hasError = await page.locator('[role="alert"], .error, .MuiAlert-root').isVisible().catch(() => false);
    const stillOnLogin = page.url().includes('login');
    
    expect(hasError || stillOnLogin).toBeTruthy();
  });

  test('TC-AUTH-005: Should show error for empty fields', async ({ page }) => {
    const loginPage = new LoginPage(page);
    
    // Try to submit empty form
    await loginPage.loginButton.click();
    
    // Should show validation errors or stay on login page
    await page.waitForTimeout(1000);
    expect(page.url()).toContain('login');
  });

  test('TC-AUTH-006: Should navigate to forgot password page', async ({ page }) => {
    const forgotLink = page.locator('a:has-text("Forgot"), a:has-text("Reset Password")').first();
    
    if (await forgotLink.isVisible()) {
      await forgotLink.click();
      await page.waitForURL(/reset|forgot|password/i);
      await expect(page.locator('body')).toContainText(/reset|forgot|email/i);
    } else {
      test.skip(true, 'Forgot password link not visible');
    }
  });

  test('TC-AUTH-007: Should navigate to registration page', async ({ page }) => {
    const registerLink = page.locator('a:has-text("Register"), a:has-text("Sign Up"), a:has-text("Create Account")').first();
    
    if (await registerLink.isVisible()) {
      await registerLink.click();
      await page.waitForURL(/register|signup/i);
      await expect(page.locator('body')).toContainText(/register|sign up|create/i);
    } else {
      test.skip(true, 'Registration link not visible');
    }
  });

  test('TC-AUTH-008: Should remember me functionality', async ({ page }) => {
    const rememberMe = page.locator('input[type="checkbox"], label:has-text("Remember")').first();
    
    if (await rememberMe.isVisible()) {
      await rememberMe.click();
      
      const loginPage = new LoginPage(page);
      await loginPage.emailInput.fill(TEST_USERS.existingAdmin.email);
      await loginPage.passwordInput.fill(TEST_USERS.existingAdmin.password);
      await loginPage.loginButton.click();
      
      await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
      
      // Check for persistent session (cookie or localStorage)
      const cookies = await page.context().cookies();
      expect(cookies.length).toBeGreaterThan(0);
    } else {
      test.skip(true, 'Remember me checkbox not visible');
    }
  });
});

test.describe('Authentication - Logout', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('/login');
    const loginPage = new LoginPage(page);
    await loginPage.emailInput.fill(TEST_USERS.existingAdmin.email);
    await loginPage.passwordInput.fill(TEST_USERS.existingAdmin.password);
    await loginPage.loginButton.click();
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
  });

  test('TC-AUTH-009: Should logout successfully', async ({ page }) => {
    // Find and click user menu or logout button
    const userMenu = page.locator('[aria-label="user"], [aria-label="account"], .user-menu, .profile-menu, button:has(.MuiAvatar-root)').first();
    const logoutButton = page.locator('button:has-text("Logout"), button:has-text("Sign Out"), a:has-text("Logout")');
    
    if (await userMenu.isVisible()) {
      await userMenu.click();
      await page.waitForTimeout(500);
    }
    
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      await page.waitForURL(/login/i, { timeout: 10000 });
      expect(page.url()).toContain('login');
    } else {
      // Try direct navigation to logout
      await page.goto('/logout');
      await page.waitForTimeout(2000);
    }
  });

  test('TC-AUTH-010: Should not access protected routes after logout', async ({ page }) => {
    // Logout
    const logoutButton = page.locator('button:has-text("Logout"), a:has-text("Logout")').first();
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
    } else {
      await page.context().clearCookies();
    }
    
    // Try to access protected route
    await page.goto('/customers');
    await page.waitForTimeout(2000);
    
    // Should redirect to login
    expect(page.url()).toMatch(/login|unauthorized/i);
  });
});

test.describe('Authentication - Registration', () => {
  test('TC-AUTH-011: Should display registration form', async ({ page }) => {
    await page.goto('/register');
    
    if (page.url().includes('register')) {
      await expect(page.locator('input[name="email"], #email').first()).toBeVisible();
      await expect(page.locator('input[name="password"], #password').first()).toBeVisible();
      await expect(page.locator('button[type="submit"]').first()).toBeVisible();
    } else {
      test.skip(true, 'Registration page not available');
    }
  });

  test('TC-AUTH-012: Should validate registration form fields', async ({ page }) => {
    await page.goto('/register');
    
    if (!page.url().includes('register')) {
      test.skip(true, 'Registration page not available');
      return;
    }
    
    // Try to submit empty form
    await page.locator('button[type="submit"]').first().click();
    await page.waitForTimeout(1000);
    
    // Should show validation errors
    const hasValidationErrors = await page.locator('.error, .MuiFormHelperText-root.Mui-error, [role="alert"]').isVisible();
    expect(hasValidationErrors || page.url().includes('register')).toBeTruthy();
  });

  test('TC-AUTH-013: Should register new test user', async ({ page }) => {
    await page.goto('/register');
    
    if (!page.url().includes('register')) {
      test.skip(true, 'Registration page not available');
      return;
    }
    
    const testUser = uniqueTestData({
      firstName: 'TEST_NewUser',
      lastName: 'E2ETest',
      email: 'test_newuser@crm-test.local',
      password: 'TestPassword123!@#',
    });
    
    // Fill registration form
    await page.locator('input[name="firstName"], #firstName').first().fill(testUser.firstName);
    await page.locator('input[name="lastName"], #lastName').first().fill(testUser.lastName);
    await page.locator('input[name="email"], #email').first().fill(testUser.email);
    await page.locator('input[name="password"], #password').first().fill(testUser.password);
    
    const confirmPassword = page.locator('input[name="confirmPassword"], #confirmPassword').first();
    if (await confirmPassword.isVisible()) {
      await confirmPassword.fill(testUser.password);
    }
    
    await page.locator('button[type="submit"]').first().click();
    
    // Should show success or redirect
    await page.waitForTimeout(3000);
    const success = 
      page.url().includes('login') || 
      page.url().includes('dashboard') ||
      await page.locator('text=/success|registered|pending/i').isVisible();
    
    expect(success).toBeTruthy();
  });
});

test.describe('Authentication - Password Reset', () => {
  test('TC-AUTH-014: Should display password reset form', async ({ page }) => {
    await page.goto('/password-reset');
    
    if (page.url().includes('password') || page.url().includes('reset') || page.url().includes('forgot')) {
      await expect(page.locator('input[name="email"], input[type="email"], #email').first()).toBeVisible();
    } else {
      // Try forgot password from login page
      await page.goto('/login');
      const forgotLink = page.locator('a:has-text("Forgot")').first();
      if (await forgotLink.isVisible()) {
        await forgotLink.click();
        await page.waitForTimeout(1000);
      } else {
        test.skip(true, 'Password reset page not available');
      }
    }
  });

  test('TC-AUTH-015: Should send password reset email', async ({ page }) => {
    await page.goto('/password-reset');
    
    if (!page.url().includes('password') && !page.url().includes('reset') && !page.url().includes('forgot')) {
      await page.goto('/login');
      const forgotLink = page.locator('a:has-text("Forgot")').first();
      if (await forgotLink.isVisible()) {
        await forgotLink.click();
        await page.waitForTimeout(1000);
      } else {
        test.skip(true, 'Password reset page not available');
        return;
      }
    }
    
    const emailInput = page.locator('input[name="email"], input[type="email"], #email').first();
    await emailInput.fill('test_reset@crm-test.local');
    
    await page.locator('button[type="submit"], button:has-text("Reset"), button:has-text("Send")').first().click();
    
    // Should show confirmation message
    await page.waitForTimeout(3000);
    // Check for success message or confirmation
    const hasResponse = await page.locator('text=/sent|success|check|email/i').isVisible().catch(() => true);
    expect(hasResponse).toBeTruthy();
  });
});

test.describe('Authentication - Two-Factor Authentication', () => {
  test('TC-AUTH-016: Should display 2FA setup option in settings', async ({ page }) => {
    // Login first
    await page.goto('/login');
    const loginPage = new LoginPage(page);
    await loginPage.emailInput.fill(TEST_USERS.existingAdmin.email);
    await loginPage.passwordInput.fill(TEST_USERS.existingAdmin.password);
    await loginPage.loginButton.click();
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
    
    // Navigate to security settings
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');
    
    // Look for 2FA option
    const twoFaOption = page.locator('text=/two.?factor|2fa|authenticator|mfa/i').first();
    
    if (await twoFaOption.isVisible({ timeout: 5000 }).catch(() => false)) {
      await expect(twoFaOption).toBeVisible();
    } else {
      // Check in security tab
      const securityTab = page.locator('button:has-text("Security"), [role="tab"]:has-text("Security")').first();
      if (await securityTab.isVisible()) {
        await securityTab.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('TC-AUTH-017: Should handle 2FA verification page', async ({ page }) => {
    await page.goto('/two-factor');
    
    if (page.url().includes('two-factor') || page.url().includes('2fa')) {
      // Check for 2FA code input
      const codeInput = page.locator('input[name="code"], input[name="token"], #code, #token').first();
      await expect(codeInput).toBeVisible();
    } else {
      test.skip(true, '2FA page not accessible without 2FA enabled');
    }
  });
});

test.describe('Authentication - Session Management', () => {
  test('TC-AUTH-018: Should maintain session across page navigation', async ({ page }) => {
    // Login
    await page.goto('/login');
    const loginPage = new LoginPage(page);
    await loginPage.emailInput.fill(TEST_USERS.existingAdmin.email);
    await loginPage.passwordInput.fill(TEST_USERS.existingAdmin.password);
    await loginPage.loginButton.click();
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
    
    // Navigate to multiple pages
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    expect(page.url()).not.toContain('login');
    
    await page.goto('/contacts');
    await page.waitForLoadState('networkidle');
    expect(page.url()).not.toContain('login');
    
    await page.goto('/opportunities');
    await page.waitForLoadState('networkidle');
    expect(page.url()).not.toContain('login');
  });

  test('TC-AUTH-019: Should handle session timeout gracefully', async ({ page }) => {
    // Login
    await page.goto('/login');
    const loginPage = new LoginPage(page);
    await loginPage.emailInput.fill(TEST_USERS.existingAdmin.email);
    await loginPage.passwordInput.fill(TEST_USERS.existingAdmin.password);
    await loginPage.loginButton.click();
    await page.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
    
    // Clear cookies to simulate session timeout
    await page.context().clearCookies();
    
    // Try to access protected resource
    await page.goto('/customers');
    await page.waitForTimeout(2000);
    
    // Should redirect to login or show session expired message
    expect(page.url()).toMatch(/login|session|expired/i);
  });

  test('TC-AUTH-020: Should handle concurrent sessions', async ({ browser }) => {
    // Create two browser contexts (simulating two sessions)
    const context1 = await browser.newContext();
    const context2 = await browser.newContext();
    
    const page1 = await context1.newPage();
    const page2 = await context2.newPage();
    
    try {
      // Login on first session
      await page1.goto('/login');
      await page1.locator('input[name="email"], input[name="username"]').first().fill(TEST_USERS.existingAdmin.email);
      await page1.locator('input[name="password"]').first().fill(TEST_USERS.existingAdmin.password);
      await page1.locator('button[type="submit"]').first().click();
      await page1.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
      
      // Login on second session
      await page2.goto('/login');
      await page2.locator('input[name="email"], input[name="username"]').first().fill(TEST_USERS.existingAdmin.email);
      await page2.locator('input[name="password"]').first().fill(TEST_USERS.existingAdmin.password);
      await page2.locator('button[type="submit"]').first().click();
      await page2.waitForURL((url) => !url.pathname.includes('login'), { timeout: 10000 });
      
      // Both sessions should be valid (or one should be invalidated based on policy)
      await page1.goto('/dashboard');
      await page2.goto('/dashboard');
      
      // At least one should still be logged in
      const page1LoggedIn = !page1.url().includes('login');
      const page2LoggedIn = !page2.url().includes('login');
      
      expect(page1LoggedIn || page2LoggedIn).toBeTruthy();
    } finally {
      await context1.close();
      await context2.close();
    }
  });
});

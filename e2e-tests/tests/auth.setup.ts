/**
 * CRM Solution - Authentication Setup
 * 
 * This file handles authentication before tests run.
 * It stores the auth state for reuse across all tests.
 */

import { test as setup, expect } from '@playwright/test';
import { TEST_USERS } from './test-data';
import path from 'path';
import fs from 'fs';

const authFile = path.join(__dirname, '../test-results/.auth/user.json');

setup('authenticate', async ({ page }) => {
  // Ensure auth directory exists
  const authDir = path.dirname(authFile);
  if (!fs.existsSync(authDir)) {
    fs.mkdirSync(authDir, { recursive: true });
  }

  // Navigate to login page
  await page.goto('/login');
  
  // Wait for page to load
  await page.waitForLoadState('networkidle');
  
  // Use environment credentials or defaults
  const username = process.env.TEST_USERNAME || TEST_USERS.existingAdmin.email;
  const password = process.env.TEST_PASSWORD || TEST_USERS.existingAdmin.password;
  
  console.log(`🔐 Attempting login with: ${username}`);
  
  // Take screenshot to debug login page structure
  await page.screenshot({ path: 'test-results/login-page.png' });
  
  // Wait for login form - MUI uses input[type="email"] for the email field
  await page.waitForSelector('input', { timeout: 10000 });
  
  // Find all inputs and log them for debugging
  const inputs = await page.locator('input').all();
  console.log(`📋 Found ${inputs.length} input fields`);
  
  // Fill email/username field - first text/email input
  const emailInput = page.locator('input[type="email"], input[type="text"]').first();
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(username);
  
  // Find and fill password field
  const passwordInput = page.locator('input[type="password"]').first();
  await passwordInput.waitFor({ state: 'visible', timeout: 5000 });
  await passwordInput.fill(password);
  
  // Take screenshot before submit
  await page.screenshot({ path: 'test-results/login-filled.png' });
  
  // Click the submit button
  const loginButton = page.locator('button[type="submit"]').first();
  await loginButton.click();
  
  // Wait for response
  await page.waitForTimeout(3000);
  
  // Take screenshot after submit
  await page.screenshot({ path: 'test-results/login-submitted.png' });
  
  // Check for error messages
  const errorAlert = page.locator('[role="alert"], .MuiAlert-root').first();
  if (await errorAlert.isVisible()) {
    const errorText = await errorAlert.textContent();
    console.log(`⚠️ Login message: ${errorText}`);
  }
  
  // Wait for successful login - should redirect away from login page
  try {
    await page.waitForURL((url) => {
      const urlPath = url.pathname;
      console.log(`📍 Current path: ${urlPath}`);
      return !urlPath.includes('/login');
    }, { timeout: 20000 });
  } catch (e) {
    await page.screenshot({ path: 'test-results/login-timeout.png' });
    console.log('❌ Login timed out. Check test-results/login-timeout.png');
    throw e;
  }
  
  // Verify we're logged in by checking for navigation
  await expect(page.locator('nav, .sidebar, [role="navigation"], .MuiDrawer-root').first()).toBeVisible({ timeout: 10000 });
  
  // Save authentication state
  await page.context().storageState({ path: authFile });
  
  console.log('✅ Authentication successful, state saved');
});

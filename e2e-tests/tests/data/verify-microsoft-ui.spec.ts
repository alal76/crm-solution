/**
 * UI Test: Navigate to Microsoft Account and Verify Data Displays Correctly
 * Tests the frontend to identify any UI issues
 */

import { test, expect } from '@playwright/test';
import { WEB_BASE_URL } from '../../testConfig';

const BASE_URL = WEB_BASE_URL;

test.describe('Verify Microsoft Account UI Display', () => {
  
  test('Navigate to Customers page and find Microsoft', async ({ page }) => {
    console.log('\n🌐 Navigating to application...');
    await page.goto(BASE_URL);
    
    // Wait for page to load
    await page.waitForLoadState('domcontentloaded');
    
    // Take screenshot of home page
    await page.screenshot({ path: 'test-results/screenshots/01-home.png' });
    console.log('📸 Screenshot: Home page saved');
    
    // Look for Customers/Accounts navigation
    console.log('\n🔍 Looking for navigation menu...');
    
    // Try to find and click on Customers or Accounts menu item
    const customersLink = page.locator('text=Customers').first();
    const accountsLink = page.locator('text=Accounts').first();
    
    if (await customersLink.isVisible()) {
      console.log('   Found "Customers" link');
      await customersLink.click();
    } else if (await accountsLink.isVisible()) {
      console.log('   Found "Accounts" link');
      await accountsLink.click();
    } else {
      // Try sidebar navigation
      const sidebarItems = await page.locator('[class*="sidebar"], [class*="nav"], [class*="menu"]').allTextContents();
      console.log('   Sidebar items found:', sidebarItems.slice(0, 5).join(', '));
    }
    
    await page.waitForLoadState('domcontentloaded');
    await page.screenshot({ path: 'test-results/screenshots/02-customers-list.png' });
    console.log('📸 Screenshot: Customers list saved');
    
    // Search for Microsoft
    console.log('\n🔍 Searching for Microsoft Corporation...');
    const searchInput = page.locator('input[placeholder*="search"], input[type="search"], input[placeholder*="Search"]').first();
    
    if (await searchInput.isVisible()) {
      await searchInput.fill('Microsoft');
      await page.waitForTimeout(1000); // Wait for search
      console.log('   Entered search term "Microsoft"');
    }
    
    await page.screenshot({ path: 'test-results/screenshots/03-search-microsoft.png' });
    console.log('📸 Screenshot: Search results saved');
    
    // Try to find Microsoft in the list
    const microsoftRow = page.locator('text=Microsoft Corporation').first();
    if (await microsoftRow.isVisible()) {
      console.log('   ✅ Found Microsoft Corporation in the list');
      await microsoftRow.click();
    }
    
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(500);
    await page.screenshot({ path: 'test-results/screenshots/04-microsoft-details.png' });
    console.log('📸 Screenshot: Microsoft details saved');
  });

  test('Check Microsoft Account Overview Page', async ({ page }) => {
    console.log('\n🌐 Navigating directly to Microsoft account...');
    
    // Navigate directly to the account (ID 3 based on the creation)
    await page.goto(`${BASE_URL}/customers/3`);
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(500);
    
    // Take screenshots of different sections
    await page.screenshot({ path: 'test-results/screenshots/05-account-overview.png', fullPage: true });
    console.log('📸 Screenshot: Account overview (full page) saved');
    
    // Check for company name
    const companyName = await page.locator('text=Microsoft Corporation').first();
    const hasCompanyName = await companyName.isVisible();
    console.log(`   Company name visible: ${hasCompanyName ? '✅' : '❌'}`);
    
    // Check for industry
    const industry = await page.locator('text=Technology').first();
    const hasIndustry = await industry.isVisible();
    console.log(`   Industry visible: ${hasIndustry ? '✅' : '❌'}`);
    
    // Check for address
    const address = await page.locator('text=Redmond').first();
    const hasAddress = await address.isVisible();
    console.log(`   Address (Redmond) visible: ${hasAddress ? '✅' : '❌'}`);
    
    // Look for Contacts section
    console.log('\n🔍 Looking for Contacts section...');
    const contactsSection = page.locator('text=Contacts').first();
    if (await contactsSection.isVisible()) {
      console.log('   ✅ Found Contacts section');
      
      // Check for executives
      const satya = await page.locator('text=Satya Nadella').first();
      const hasSatya = await satya.isVisible();
      console.log(`   Satya Nadella visible: ${hasSatya ? '✅' : '❌'}`);
      
      const brad = await page.locator('text=Brad Smith').first();
      const hasBrad = await brad.isVisible();
      console.log(`   Brad Smith visible: ${hasBrad ? '✅' : '❌'}`);
      
      const amy = await page.locator('text=Amy Hood').first();
      const hasAmy = await amy.isVisible();
      console.log(`   Amy Hood visible: ${hasAmy ? '✅' : '❌'}`);
    } else {
      console.log('   ⚠️ Contacts section not found on overview page');
    }
    
    // Look for Notes section to check our detailed data
    console.log('\n🔍 Looking for Notes section...');
    const notesSection = page.locator('text=Notes').first();
    if (await notesSection.isVisible()) {
      console.log('   ✅ Found Notes section');
      
      // Check for some key data points
      const revenue = await page.locator('text=$245').first();
      const hasRevenue = await revenue.isVisible();
      console.log(`   Revenue data visible: ${hasRevenue ? '✅' : '❌'}`);
    }
    
    // Take final screenshot
    await page.screenshot({ path: 'test-results/screenshots/06-account-details-final.png', fullPage: true });
    console.log('\n📸 Screenshot: Final account view saved');
  });

  test('Check individual contact details', async ({ page }) => {
    console.log('\n🌐 Navigating to Satya Nadella contact...');
    
    // Navigate to contact ID 7 (Satya Nadella)
    await page.goto(`${BASE_URL}/contacts/7`);
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(500);
    
    await page.screenshot({ path: 'test-results/screenshots/07-satya-nadella-contact.png', fullPage: true });
    console.log('📸 Screenshot: Satya Nadella contact saved');
    
    // Check for key data
    const satyaName = await page.locator('text=Satya Nadella').first();
    const hasName = await satyaName.isVisible();
    console.log(`   Contact name visible: ${hasName ? '✅' : '❌'}`);
    
    const ceo = await page.locator('text=CEO').first();
    const hasCEO = await ceo.isVisible();
    console.log(`   CEO title visible: ${hasCEO ? '✅' : '❌'}`);
    
    // Check for linked account
    const microsoft = await page.locator('text=Microsoft').first();
    const hasMicrosoft = await microsoft.isVisible();
    console.log(`   Microsoft link visible: ${hasMicrosoft ? '✅' : '❌'}`);
    
    // Check for notes
    const notes = await page.locator('text=Biography').first();
    const hasNotes = await notes.isVisible();
    console.log(`   Biography/Notes visible: ${hasNotes ? '✅' : '❌'}`);
  });

  test('Collect all visible issues', async ({ page }) => {
    console.log('\n🔍 Scanning for potential UI issues...');
    
    const issues: string[] = [];
    
    // Go to customers list
    await page.goto(`${BASE_URL}/customers`);
    await page.waitForLoadState('domcontentloaded');
    
    // Check for error messages
    const errorMessages = await page.locator('[class*="error"], [class*="alert-danger"], .MuiAlert-standardError').allTextContents();
    if (errorMessages.length > 0) {
      issues.push(`Error messages found: ${errorMessages.join(', ')}`);
    }
    
    // Check for empty states
    const emptyStates = await page.locator('text=No data, text=No results, text=Empty').count();
    if (emptyStates > 0) {
      issues.push(`Empty state messages found`);
    }
    
    // Check console errors
    const consoleErrors: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    // Navigate through key pages
    await page.goto(`${BASE_URL}/customers/3`);
    await page.waitForLoadState('domcontentloaded');
    
    await page.goto(`${BASE_URL}/contacts/7`);
    await page.waitForLoadState('domcontentloaded');
    
    if (consoleErrors.length > 0) {
      issues.push(`Console errors: ${consoleErrors.slice(0, 3).join('; ')}`);
    }
    
    console.log('\n📋 UI Issues Summary:');
    if (issues.length === 0) {
      console.log('   ✅ No critical issues found');
    } else {
      for (const issue of issues) {
        console.log(`   ⚠️ ${issue}`);
      }
    }
    
    // Final screenshots
    await page.screenshot({ path: 'test-results/screenshots/08-final-check.png', fullPage: true });
    console.log('\n📸 All screenshots saved to test-results/screenshots/');
  });
});

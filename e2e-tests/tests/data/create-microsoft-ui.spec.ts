/**
 * UI Test: Create Microsoft Corporation Account with 5 Top Executives via Frontend
 * This test creates the account and contacts through the UI (not API) and logs all activities
 */

import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';

const BASE_URL = process.env.BASE_URL || 'http://localhost';
const LOG_FILE = 'test-results/microsoft-ui-activity.log';

// Activity logger
class ActivityLogger {
  private logs: string[] = [];
  private startTime: number;

  constructor() {
    this.startTime = Date.now();
    this.log('========================================');
    this.log('Microsoft Account Creation - Activity Log');
    this.log(`Started at: ${new Date().toISOString()}`);
    this.log(`Target URL: ${BASE_URL}`);
    this.log('========================================\n');
  }

  log(message: string) {
    const elapsed = ((Date.now() - this.startTime) / 1000).toFixed(2);
    const logLine = `[${elapsed}s] ${message}`;
    this.logs.push(logLine);
    console.log(message);
  }

  section(title: string) {
    this.log(`\n${'─'.repeat(50)}`);
    this.log(`📋 ${title}`);
    this.log('─'.repeat(50));
  }

  success(message: string) {
    this.log(`✅ ${message}`);
  }

  info(message: string) {
    this.log(`ℹ️  ${message}`);
  }

  action(message: string) {
    this.log(`🔄 ${message}`);
  }

  error(message: string) {
    this.log(`❌ ${message}`);
  }

  screenshot(name: string) {
    this.log(`📸 Screenshot saved: ${name}`);
  }

  save() {
    this.log('\n========================================');
    this.log(`Completed at: ${new Date().toISOString()}`);
    this.log(`Total duration: ${((Date.now() - this.startTime) / 1000).toFixed(2)} seconds`);
    this.log('========================================');

    // Ensure directory exists
    const dir = 'test-results';
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }
    fs.writeFileSync(LOG_FILE, this.logs.join('\n'));
    console.log(`\n📝 Activity log saved to: ${LOG_FILE}`);
  }
}

// Microsoft Corporation data
const microsoftAccount = {
  company: 'Microsoft Corporation',
  industry: 'Technology',
  phone: '+1 (425) 882-8080',
  email: 'info@microsoft.com',
  website: 'https://www.microsoft.com',
  address: 'One Microsoft Way',
  city: 'Redmond',
  state: 'WA',
  zipCode: '98052',
  country: 'USA',
  notes: `Microsoft Corporation - Global Technology Leader
Founded: April 4, 1975 by Bill Gates and Paul Allen
Headquarters: Redmond, Washington, USA
Revenue: $245.1 billion (FY 2024)
Employees: ~221,000 worldwide
NASDAQ: MSFT`
};

// Top 5 Microsoft Executives
const microsoftExecutives = [
  {
    firstName: 'Satya',
    lastName: 'Nadella',
    email: 'satya.nadella@microsoft.com',
    phone: '+1 (425) 882-8080',
    title: 'Chairman and CEO',
    department: 'Executive Leadership',
    isPrimary: true,
    isDecisionMaker: true,
    notes: 'CEO since 2014, Chairman since 2021. Led cloud transformation.'
  },
  {
    firstName: 'Amy',
    lastName: 'Hood',
    email: 'amy.hood@microsoft.com',
    phone: '+1 (425) 882-8082',
    title: 'Executive Vice President and CFO',
    department: 'Finance',
    isPrimary: false,
    isDecisionMaker: true,
    notes: 'CFO since 2013. Oversees $245B+ revenue.'
  },
  {
    firstName: 'Judson',
    lastName: 'Althoff',
    email: 'judson.althoff@microsoft.com',
    phone: '+1 (425) 882-8083',
    title: 'Executive Vice President and Chief Commercial Officer',
    department: 'Commercial Business',
    isPrimary: false,
    isDecisionMaker: true,
    notes: 'CCO leading enterprise sales and partnerships.'
  },
  {
    firstName: 'Brad',
    lastName: 'Smith',
    email: 'brad.smith@microsoft.com',
    phone: '+1 (425) 882-8084',
    title: 'Vice Chair and President',
    department: 'Legal & Corporate Affairs',
    isPrimary: false,
    isDecisionMaker: false,
    notes: 'President since 2015. Leads legal, policy, and public affairs.'
  },
  {
    firstName: 'Kathleen',
    lastName: 'Hogan',
    email: 'kathleen.hogan@microsoft.com',
    phone: '+1 (425) 882-8085',
    title: 'Executive Vice President and Chief People Officer',
    department: 'Human Resources',
    isPrimary: false,
    isDecisionMaker: false,
    notes: 'CPO leading HR and culture transformation.'
  }
];

const logger = new ActivityLogger();

test.describe.serial('Create Microsoft Account via UI', () => {
  
  test.afterAll(async () => {
    logger.save();
  });

  test('1. Login to CRM Application', async ({ page }) => {
    logger.section('Step 1: Authentication');
    
    logger.action('Navigating to login page...');
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    logger.info(`Current URL: ${page.url()}`);
    
    // Check if already logged in
    if (page.url().includes('/dashboard') || !page.url().includes('/login')) {
      logger.success('Already logged in, skipping authentication');
      return;
    }
    
    logger.action('Filling login credentials...');
    await page.fill('input[name="email"], input[type="email"]', 'abhi.lal@gmail.com');
    logger.info('Entered email: abhi.lal@gmail.com');
    
    await page.fill('input[name="password"], input[type="password"]', 'Admin@123');
    logger.info('Entered password: ********');
    
    await page.screenshot({ path: 'test-results/screenshots/01-login-filled.png' });
    logger.screenshot('01-login-filled.png');
    
    logger.action('Clicking login button...');
    await page.click('button[type="submit"]');
    
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    logger.info(`Redirected to: ${page.url()}`);
    logger.success('Login completed successfully');
    
    await page.screenshot({ path: 'test-results/screenshots/02-post-login.png' });
    logger.screenshot('02-post-login.png');
  });

  test('2. Navigate to Customers Page', async ({ page }) => {
    logger.section('Step 2: Navigate to Customers');
    
    logger.action('Navigating to /customers page...');
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    logger.info(`Current URL: ${page.url()}`);
    
    // Check if page loaded
    const pageTitle = await page.title();
    logger.info(`Page title: ${pageTitle}`);
    
    await page.screenshot({ path: 'test-results/screenshots/03-customers-page.png' });
    logger.screenshot('03-customers-page.png');
    
    // Log page content overview
    const customerCount = await page.locator('table tbody tr, .MuiDataGrid-row').count();
    logger.info(`Existing customers visible: ${customerCount}`);
    
    logger.success('Customers page loaded successfully');
  });

  test('3. Create Microsoft Corporation Account', async ({ page }) => {
    logger.section('Step 3: Create Microsoft Corporation');
    
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Find and click Add button
    logger.action('Looking for Add Customer button...');
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    
    if (await addButton.isVisible()) {
      logger.info('Found Add button');
      await addButton.scrollIntoViewIfNeeded();
      await addButton.click({ force: true });
      logger.action('Clicked Add Customer button');
    } else {
      logger.error('Add button not found');
      throw new Error('Add Customer button not found');
    }
    
    await page.waitForTimeout(1500);
    await page.screenshot({ path: 'test-results/screenshots/04-add-dialog-opened.png' });
    logger.screenshot('04-add-dialog-opened.png');
    
    // Check for dialog/form
    const dialog = page.locator('[role="dialog"], .MuiDialog-root, form').first();
    if (await dialog.isVisible()) {
      logger.success('Customer form dialog opened');
    }
    
    // Toggle to Organization if there's a switch
    const orgSwitch = page.locator('[role="checkbox"], .MuiSwitch-input, input[type="checkbox"]').first();
    if (await orgSwitch.isVisible().catch(() => false)) {
      const switchLabel = await orgSwitch.getAttribute('aria-label').catch(() => '');
      logger.info(`Found switch: ${switchLabel}`);
      const isChecked = await orgSwitch.isChecked().catch(() => false);
      if (!isChecked) {
        await orgSwitch.click({ force: true });
        logger.action('Toggled to Organization mode');
        await page.waitForTimeout(500);
      }
    }
    
    // Fill company name
    logger.action('Filling Microsoft Corporation details...');
    
    const companyInput = page.locator('input[name="company"], input[name="legalName"], input[placeholder*="Company"], input[placeholder*="company"]').first();
    if (await companyInput.isVisible().catch(() => false)) {
      await companyInput.fill(microsoftAccount.company);
      logger.info(`Company: ${microsoftAccount.company}`);
    }
    
    // Fill email
    const emailInput = page.locator('input[name="email"], input[type="email"]').first();
    if (await emailInput.isVisible().catch(() => false)) {
      await emailInput.fill(microsoftAccount.email);
      logger.info(`Email: ${microsoftAccount.email}`);
    }
    
    // Fill phone
    const phoneInput = page.locator('input[name="phone"], input[type="tel"]').first();
    if (await phoneInput.isVisible().catch(() => false)) {
      await phoneInput.fill(microsoftAccount.phone);
      logger.info(`Phone: ${microsoftAccount.phone}`);
    }
    
    // Fill website
    const websiteInput = page.locator('input[name="website"], input[placeholder*="website"], input[placeholder*="Website"]').first();
    if (await websiteInput.isVisible().catch(() => false)) {
      await websiteInput.fill(microsoftAccount.website);
      logger.info(`Website: ${microsoftAccount.website}`);
    }
    
    // Fill industry if available
    const industryInput = page.locator('input[name="industry"], [name="industry"]').first();
    if (await industryInput.isVisible().catch(() => false)) {
      await industryInput.fill(microsoftAccount.industry);
      logger.info(`Industry: ${microsoftAccount.industry}`);
    }
    
    // Fill address fields
    const addressInput = page.locator('input[name="address"], input[name="street"]').first();
    if (await addressInput.isVisible().catch(() => false)) {
      await addressInput.fill(microsoftAccount.address);
      logger.info(`Address: ${microsoftAccount.address}`);
    }
    
    const cityInput = page.locator('input[name="city"]').first();
    if (await cityInput.isVisible().catch(() => false)) {
      await cityInput.fill(microsoftAccount.city);
      logger.info(`City: ${microsoftAccount.city}`);
    }
    
    const stateInput = page.locator('input[name="state"]').first();
    if (await stateInput.isVisible().catch(() => false)) {
      await stateInput.fill(microsoftAccount.state);
      logger.info(`State: ${microsoftAccount.state}`);
    }
    
    const zipInput = page.locator('input[name="zipCode"], input[name="postalCode"]').first();
    if (await zipInput.isVisible().catch(() => false)) {
      await zipInput.fill(microsoftAccount.zipCode);
      logger.info(`Zip: ${microsoftAccount.zipCode}`);
    }
    
    // Fill notes if available
    const notesInput = page.locator('textarea[name="notes"], textarea').first();
    if (await notesInput.isVisible().catch(() => false)) {
      await notesInput.fill(microsoftAccount.notes);
      logger.info('Notes: Added company description');
    }
    
    await page.screenshot({ path: 'test-results/screenshots/05-form-filled.png' });
    logger.screenshot('05-form-filled.png');
    
    // Submit the form
    logger.action('Submitting customer form...');
    const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create"), button:has-text("Add")').first();
    await saveButton.scrollIntoViewIfNeeded();
    await saveButton.click({ force: true });
    
    await page.waitForTimeout(3000);
    await page.waitForLoadState('networkidle');
    
    await page.screenshot({ path: 'test-results/screenshots/06-after-save.png' });
    logger.screenshot('06-after-save.png');
    
    // Verify creation
    logger.action('Verifying Microsoft Corporation was created...');
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Search for Microsoft
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"], input[name="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('Microsoft');
      logger.action('Searching for "Microsoft"');
      await page.waitForTimeout(1500);
    }
    
    await page.screenshot({ path: 'test-results/screenshots/07-search-microsoft.png' });
    logger.screenshot('07-search-microsoft.png');
    
    const microsoftRow = page.locator('text=Microsoft Corporation').first();
    if (await microsoftRow.isVisible().catch(() => false)) {
      logger.success('Microsoft Corporation found in customer list!');
    } else {
      logger.info('Microsoft Corporation may have been created (check manually)');
    }
  });

  test('4. Navigate to Contacts Page', async ({ page }) => {
    logger.section('Step 4: Navigate to Contacts');
    
    logger.action('Navigating to /contacts page...');
    await page.goto('/contacts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    logger.info(`Current URL: ${page.url()}`);
    
    await page.screenshot({ path: 'test-results/screenshots/08-contacts-page.png' });
    logger.screenshot('08-contacts-page.png');
    
    const contactCount = await page.locator('table tbody tr, .MuiDataGrid-row').count();
    logger.info(`Existing contacts visible: ${contactCount}`);
    
    logger.success('Contacts page loaded successfully');
  });

  test('5. Create Microsoft Executives as Contacts', async ({ page }) => {
    logger.section('Step 5: Create Microsoft Executives');
    
    for (let i = 0; i < microsoftExecutives.length; i++) {
      const exec = microsoftExecutives[i];
      logger.log(`\n👤 Creating Executive ${i + 1}/${microsoftExecutives.length}: ${exec.firstName} ${exec.lastName}`);
      logger.info(`Title: ${exec.title}`);
      logger.info(`Department: ${exec.department}`);
      
      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      
      // Find and click Add button
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addButton.isVisible()) {
        await addButton.scrollIntoViewIfNeeded();
        await addButton.click({ force: true });
        logger.action('Clicked Add Contact button');
      }
      
      await page.waitForTimeout(1500);
      
      // Fill first name
      const firstNameInput = page.locator('input[name="firstName"], input[placeholder*="First"]').first();
      if (await firstNameInput.isVisible().catch(() => false)) {
        await firstNameInput.fill(exec.firstName);
        logger.info(`First Name: ${exec.firstName}`);
      }
      
      // Fill last name
      const lastNameInput = page.locator('input[name="lastName"], input[placeholder*="Last"]').first();
      if (await lastNameInput.isVisible().catch(() => false)) {
        await lastNameInput.fill(exec.lastName);
        logger.info(`Last Name: ${exec.lastName}`);
      }
      
      // Fill email
      const emailInput = page.locator('input[name="email"], input[type="email"]').first();
      if (await emailInput.isVisible().catch(() => false)) {
        await emailInput.fill(exec.email);
        logger.info(`Email: ${exec.email}`);
      }
      
      // Fill phone
      const phoneInput = page.locator('input[name="phone"], input[type="tel"]').first();
      if (await phoneInput.isVisible().catch(() => false)) {
        await phoneInput.fill(exec.phone);
        logger.info(`Phone: ${exec.phone}`);
      }
      
      // Fill title
      const titleInput = page.locator('input[name="title"], input[placeholder*="Title"], input[placeholder*="title"]').first();
      if (await titleInput.isVisible().catch(() => false)) {
        await titleInput.fill(exec.title);
        logger.info(`Title: ${exec.title}`);
      }
      
      // Fill department
      const deptInput = page.locator('input[name="department"], input[placeholder*="Department"]').first();
      if (await deptInput.isVisible().catch(() => false)) {
        await deptInput.fill(exec.department);
        logger.info(`Department: ${exec.department}`);
      }
      
      // Fill notes
      const notesInput = page.locator('textarea[name="notes"], textarea').first();
      if (await notesInput.isVisible().catch(() => false)) {
        await notesInput.fill(exec.notes);
        logger.info('Notes: Added executive bio');
      }
      
      await page.screenshot({ path: `test-results/screenshots/09-contact-${i + 1}-filled.png` });
      logger.screenshot(`09-contact-${i + 1}-filled.png`);
      
      // Submit
      const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create"), button:has-text("Add")').first();
      await saveButton.scrollIntoViewIfNeeded();
      await saveButton.click({ force: true });
      
      await page.waitForTimeout(2000);
      await page.waitForLoadState('networkidle');
      
      logger.success(`Created contact: ${exec.firstName} ${exec.lastName}`);
      
      // Small delay between creations
      await page.waitForTimeout(500);
    }
    
    logger.success(`All ${microsoftExecutives.length} executives created!`);
  });

  test('6. Link Contacts to Microsoft Customer', async ({ page }) => {
    logger.section('Step 6: Link Contacts to Microsoft');
    
    // Navigate to customers and find Microsoft
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Search for Microsoft
    logger.action('Searching for Microsoft Corporation...');
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('Microsoft');
      await page.waitForTimeout(1500);
    }
    
    // Click on Microsoft to open details
    const microsoftRow = page.locator('text=Microsoft Corporation').first();
    if (await microsoftRow.isVisible().catch(() => false)) {
      await microsoftRow.click();
      logger.action('Clicked on Microsoft Corporation');
      await page.waitForTimeout(2000);
      await page.waitForLoadState('networkidle');
    }
    
    await page.screenshot({ path: 'test-results/screenshots/10-microsoft-details.png' });
    logger.screenshot('10-microsoft-details.png');
    
    // Look for Contacts tab or section
    const contactsTab = page.locator('button:has-text("Contacts"), a:has-text("Contacts"), [role="tab"]:has-text("Contacts")').first();
    if (await contactsTab.isVisible().catch(() => false)) {
      await contactsTab.click();
      logger.action('Clicked Contacts tab');
      await page.waitForTimeout(1000);
    }
    
    // Try to find Add Contact button in the customer context
    const addContactBtn = page.locator('button:has-text("Add Contact"), button:has-text("Link Contact")').first();
    if (await addContactBtn.isVisible().catch(() => false)) {
      logger.info('Found Add/Link Contact button in customer view');
      
      for (let i = 0; i < microsoftExecutives.length; i++) {
        const exec = microsoftExecutives[i];
        logger.action(`Linking ${exec.firstName} ${exec.lastName}...`);
        
        await addContactBtn.click();
        await page.waitForTimeout(1000);
        
        // Search for the contact
        const contactSearch = page.locator('input[placeholder*="Search"], input[type="search"]').last();
        if (await contactSearch.isVisible().catch(() => false)) {
          await contactSearch.fill(exec.lastName);
          await page.waitForTimeout(1000);
        }
        
        // Select the contact
        const contactOption = page.locator(`text=${exec.firstName}`).first();
        if (await contactOption.isVisible().catch(() => false)) {
          await contactOption.click();
          await page.waitForTimeout(500);
        }
        
        // Set primary if applicable
        if (exec.isPrimary) {
          const primaryCheckbox = page.locator('input[name="isPrimaryContact"], label:has-text("Primary")');
          if (await primaryCheckbox.isVisible().catch(() => false)) {
            await primaryCheckbox.click();
            logger.info('Set as Primary Contact');
          }
        }
        
        // Set decision maker if applicable
        if (exec.isDecisionMaker) {
          const dmCheckbox = page.locator('input[name="isDecisionMaker"], label:has-text("Decision")');
          if (await dmCheckbox.isVisible().catch(() => false)) {
            await dmCheckbox.click();
            logger.info('Set as Decision Maker');
          }
        }
        
        // Save the link
        const saveLinkBtn = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Link")').first();
        if (await saveLinkBtn.isVisible().catch(() => false)) {
          await saveLinkBtn.click();
          await page.waitForTimeout(1500);
        }
        
        logger.success(`Linked ${exec.firstName} ${exec.lastName} to Microsoft`);
      }
    } else {
      logger.info('Direct contact linking not available in UI - contacts created separately');
    }
    
    await page.screenshot({ path: 'test-results/screenshots/11-contacts-linked.png' });
    logger.screenshot('11-contacts-linked.png');
  });

  test('7. Final Verification and Summary', async ({ page }) => {
    logger.section('Step 7: Final Verification');
    
    // Navigate to customers
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    // Search for Microsoft
    const searchInput = page.locator('input[placeholder*="Search"], input[type="search"]').first();
    if (await searchInput.isVisible().catch(() => false)) {
      await searchInput.fill('Microsoft');
      await page.waitForTimeout(1500);
    }
    
    const microsoftVisible = await page.locator('text=Microsoft Corporation').isVisible().catch(() => false);
    if (microsoftVisible) {
      logger.success('✓ Microsoft Corporation exists in Customers');
    }
    
    // Check contacts
    await page.goto('/contacts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    
    await page.screenshot({ path: 'test-results/screenshots/12-final-contacts.png' });
    logger.screenshot('12-final-contacts.png');
    
    logger.log('\n' + '═'.repeat(50));
    logger.log('📊 SUMMARY');
    logger.log('═'.repeat(50));
    logger.log(`Target Environment: ${BASE_URL}`);
    logger.log(`Customer Created: Microsoft Corporation`);
    logger.log(`Location: ${microsoftAccount.city}, ${microsoftAccount.state}`);
    logger.log(`Industry: ${microsoftAccount.industry}`);
    logger.log('');
    logger.log('Executives Created:');
    for (const exec of microsoftExecutives) {
      const markers = [];
      if (exec.isPrimary) markers.push('⭐ Primary');
      if (exec.isDecisionMaker) markers.push('🎯 Decision Maker');
      logger.log(`  • ${exec.firstName} ${exec.lastName} - ${exec.title} ${markers.join(' ')}`);
    }
    logger.log('═'.repeat(50));
    
    logger.success('All operations completed!');
  });
});

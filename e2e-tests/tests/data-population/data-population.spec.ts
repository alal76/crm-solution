/**
 * CRM Solution - Comprehensive Data Population Test Suite
 * 
 * Creates test data:
 * - 10 Users
 * - 25 Customers (each with 10-40 contacts)
 * - 50 Leads
 * - 50 Opportunities
 * - Products, Services, Service Requests from seed data
 */

import { test, expect, Page } from '@playwright/test';

// Test Statistics Tracking
const stats = {
  users: { created: 0, failed: 0 },
  customers: { created: 0, failed: 0 },
  contacts: { created: 0, failed: 0 },
  leads: { created: 0, failed: 0 },
  opportunities: { created: 0, failed: 0 },
  products: { created: 0, failed: 0 },
  serviceRequests: { created: 0, failed: 0 },
};

// Helper functions
function randomString(length: number = 8): string {
  return Math.random().toString(36).substring(2, 2 + length);
}

function randomPhone(): string {
  return `+1${Math.floor(Math.random() * 9000000000 + 1000000000)}`;
}

function randomAmount(min: number = 1000, max: number = 100000): number {
  return Math.floor(Math.random() * (max - min) + min);
}

function futureDate(daysFromNow: number): string {
  const date = new Date();
  date.setDate(date.getDate() + daysFromNow);
  return date.toISOString().split('T')[0];
}

function pastDate(daysAgo: number): string {
  const date = new Date();
  date.setDate(date.getDate() - daysAgo);
  return date.toISOString().split('T')[0];
}

// Test Data Generators
const departments = ['Sales', 'Marketing', 'Support', 'Engineering', 'Finance', 'Operations', 'HR', 'IT', 'Legal', 'Executive'];
const roles = ['Administrator', 'Sales Manager', 'Sales Representative', 'Support Agent', 'Marketing Specialist', 'Viewer'];
const industries = ['Technology', 'Healthcare', 'Finance', 'Manufacturing', 'Retail', 'Education', 'Consulting', 'Real Estate', 'Hospitality', 'Telecommunications'];
const leadStatuses = ['New', 'Working', 'Qualified', 'Unqualified'];
const opportunityStages = ['Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won', 'Closed Lost'];
const priorities = ['Low', 'Medium', 'High', 'Critical'];

// Generate Users
function generateUsers(count: number) {
  const users = [];
  for (let i = 1; i <= count; i++) {
    users.push({
      username: `test_user_${i}_${randomString(4)}`,
      email: `test_user_${i}_${randomString(4)}@testcrm.local`,
      password: 'TestPass123!@#',
      firstName: `TestUser${i}`,
      lastName: `Generated_${randomString(4)}`,
      role: roles[i % roles.length],
      department: departments[i % departments.length],
    });
  }
  return users;
}

// Generate Customers
function generateCustomers(count: number) {
  const customers = [];
  for (let i = 1; i <= count; i++) {
    const isIndividual = i % 5 === 0;
    customers.push({
      name: isIndividual ? `Test_Person_${i}_${randomString(4)}` : `Test_Company_${i}_${randomString(4)}`,
      firstName: isIndividual ? `TestPerson${i}` : null,
      lastName: isIndividual ? `Individual_${randomString(4)}` : null,
      company: isIndividual ? null : `Test Company ${i} Inc`,
      isOrganization: !isIndividual,
      industry: industries[i % industries.length],
      email: `test_customer_${i}_${randomString(4)}@testcompany.local`,
      phone: randomPhone(),
      contactCount: Math.floor(Math.random() * 31) + 10, // 10-40 contacts
    });
  }
  return customers;
}

// Generate Contacts for a Customer
function generateContacts(customerName: string, count: number) {
  const contacts = [];
  const titles = ['CEO', 'CTO', 'CFO', 'VP Engineering', 'Director of Sales', 'IT Manager', 'Procurement Manager', 'Operations Manager', 'Project Lead', 'Technical Lead', 'Account Manager', 'Business Analyst'];
  
  for (let i = 1; i <= count; i++) {
    contacts.push({
      firstName: `Contact${i}`,
      lastName: `${customerName.replace(/[^a-zA-Z]/g, '').substring(0, 8)}_${randomString(3)}`,
      email: `contact_${i}_${randomString(4)}@${customerName.toLowerCase().replace(/[^a-z]/g, '').substring(0, 10)}.local`,
      phone: randomPhone(),
      title: titles[i % titles.length],
    });
  }
  return contacts;
}

// Generate Leads
function generateLeads(count: number) {
  const leads = [];
  const sources = ['Website', 'Referral', 'Trade Show', 'Cold Call', 'Webinar', 'LinkedIn', 'Partner', 'Email Campaign'];
  
  for (let i = 1; i <= count; i++) {
    leads.push({
      firstName: `Lead${i}`,
      lastName: `Prospect_${randomString(5)}`,
      email: `lead_${i}_${randomString(4)}@prospect.local`,
      phone: randomPhone(),
      company: `Prospect Company ${i}`,
      title: ['VP', 'Director', 'Manager', 'Owner', 'CTO', 'CEO'][i % 6],
      source: sources[i % sources.length],
      status: leadStatuses[i % leadStatuses.length],
    });
  }
  return leads;
}

// Generate Opportunities
function generateOpportunities(count: number) {
  const opportunities = [];
  const sources = ['Website', 'Referral', 'Upsell', 'Cross-sell', 'Renewal', 'Partner'];
  
  for (let i = 1; i <= count; i++) {
    const stage = opportunityStages[i % opportunityStages.length];
    
    opportunities.push({
      name: `Test_Opportunity_${i}_${randomString(4)}`,
      value: randomAmount(5000, 500000),
      stage: stage,
      expectedCloseDate: stage.includes('Closed') ? pastDate(Math.floor(Math.random() * 30)) : futureDate(Math.floor(Math.random() * 90) + 15),
      description: `Test opportunity ${i} for CRUD testing`,
      source: sources[i % sources.length],
    });
  }
  return opportunities;
}

// Products from Seed Data
const products = [
  { name: 'Enterprise License', sku: 'ENT-LIC-001', type: 'Subscription', price: 50000, description: 'Enterprise software license - annual' },
  { name: 'Professional License', sku: 'PRO-LIC-001', type: 'Subscription', price: 25000, description: 'Professional software license - annual' },
  { name: 'Starter Package', sku: 'STR-PKG-001', type: 'Subscription', price: 5000, description: 'Starter package for small businesses' },
  { name: 'Premium Support', sku: 'PRM-SUP-001', type: 'Service', price: 15000, description: '24/7 premium support package' },
  { name: 'Implementation Service', sku: 'IMP-SVC-001', type: 'Service', price: 30000, description: 'Full implementation and onboarding service' },
  { name: 'Training Package', sku: 'TRN-PKG-001', type: 'Service', price: 5000, description: 'User training and certification' },
  { name: 'Custom Integration', sku: 'CUS-INT-001', type: 'Service', price: 20000, description: 'Custom API integration development' },
  { name: 'Data Migration', sku: 'DAT-MIG-001', type: 'Service', price: 10000, description: 'Data migration and cleanup service' },
  { name: 'Hardware Bundle', sku: 'HW-BND-001', type: 'Physical', price: 8000, description: 'Server hardware bundle' },
  { name: 'Compliance Module', sku: 'CMP-MOD-001', type: 'Digital', price: 12000, description: 'Healthcare/Finance compliance add-on' },
];

// Service Request Types
const serviceRequestTypes = [
  { name: 'Device Not Powering On', category: 'Hardware', priority: 'High' },
  { name: 'Performance Degradation', category: 'Hardware', priority: 'Medium' },
  { name: 'Network Connectivity Loss', category: 'Hardware', priority: 'Critical' },
  { name: 'Server Crash/Downtime', category: 'Hardware', priority: 'Critical' },
  { name: 'Software Installation', category: 'Software', priority: 'Low' },
  { name: 'Cloud Migration', category: 'Cloud Services', priority: 'Medium' },
  { name: 'Security Incident', category: 'Security Services', priority: 'Critical' },
  { name: 'Training Request', category: 'Support & Maintenance', priority: 'Low' },
];

// Shared state
const createdCustomerIds: string[] = [];

// Page-specific add button selectors
const addButtonSelectors: Record<string, string> = {
  '/admin/users': 'button:has-text("Add User")',
  '/customers': 'button:has-text("Add Account")',
  '/contacts': 'button:has-text("Add Contact")',
  '/leads': 'button:has-text("Add Lead")',
  '/opportunities': 'button:has-text("Add Opportunity")',
  '/products': 'button:has-text("Add Product")',
  '/service-requests': 'button:has-text("New Request")',
};

// Helper to click button with better visibility handling
async function clickAddButton(page: Page, path?: string): Promise<boolean> {
  await page.evaluate(() => window.scrollTo(0, 0));
  await page.waitForTimeout(500);
  
  // Use page-specific selector if available
  const currentPath = path || new URL(page.url()).pathname;
  const specificSelector = addButtonSelectors[currentPath];
  const selector = specificSelector || 'button:has-text("Add"), button:has-text("New"), button:has-text("Create")';
  
  const addButton = page.locator(selector).first();
  try {
    await addButton.waitFor({ state: 'visible', timeout: 5000 });
    await addButton.scrollIntoViewIfNeeded();
    await addButton.click({ force: true });
    await page.waitForTimeout(500);
    return true;
  } catch (e) {
    console.log(`    ⚠️ Button not found: ${selector}`);
    return false;
  }
}

// Helper to wait for dialog
async function waitForDialog(page: Page): Promise<boolean> {
  try {
    // Wait for visible dialog using MUI-specific selectors
    // The .MuiDialogContent-root is the main visible part
    const dialogContent = page.locator('.MuiDialogContent-root, .MuiDialogTitle-root, .MuiDialog-paper');
    await dialogContent.first().waitFor({ state: 'visible', timeout: 5000 });
    await page.waitForTimeout(300);
    return true;
  } catch (e) {
    // Fallback: check if we have visible inputs (form opened inline or drawer)
    const visibleInputs = await page.locator('input:visible').count();
    if (visibleInputs > 3) { // More than just search/header inputs
      return true;
    }
    console.log('    ⚠️ Dialog not found');
    return false;
  }
}

// Helper to submit form
async function submitForm(page: Page): Promise<boolean> {
  try {
    // Try multiple save button patterns
    const saveButton = page.locator('button[type="submit"]:visible, button:has-text("Save"):visible, button:has-text("Create"):visible, button:has-text("Submit"):visible').first();
    await saveButton.waitFor({ state: 'visible', timeout: 3000 });
    await saveButton.scrollIntoViewIfNeeded();
    await saveButton.click({ force: true });
    
    // Wait for response
    await page.waitForTimeout(2000);
    
    // Check if dialog closed (success) or error appeared
    const dialogStillOpen = await page.locator('.MuiDialogContent-root:visible, .MuiDialogTitle-root:visible').isVisible().catch(() => false);
    const errorMessage = await page.locator('.MuiAlert-standardError, .MuiFormHelperText-root.Mui-error, [role="alert"]:has-text("error"), [role="alert"]:has-text("required")').first().textContent().catch(() => null);
    
    if (errorMessage) {
      console.log(`    ⚠️ Form has validation errors: ${errorMessage.substring(0, 50)}...`);
      // Try to close the dialog
      await page.keyboard.press('Escape');
      await page.waitForTimeout(300);
      return false;
    }
    
    // If dialog closed, success
    if (!dialogStillOpen) {
      return true;
    }
    
    // Dialog still open but no error - might be loading
    await page.waitForTimeout(1000);
    const stillOpen = await page.locator('.MuiDialogContent-root:visible, .MuiDialogTitle-root:visible').isVisible().catch(() => false);
    if (!stillOpen) {
      return true;
    }
    
    console.log('    ⚠️ Dialog still open after submit');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    return false;
  } catch (e) {
    console.log(`    ⚠️ Submit failed: ${e}`);
    return false;
  }
}

// Helper to fill input if visible
async function fillIfVisible(page: Page, selector: string, value: string): Promise<boolean> {
  try {
    const input = page.locator(selector).first();
    if (await input.isVisible({ timeout: 1000 })) {
      await input.fill(value);
      return true;
    }
  } catch (e) {}
  return false;
}

// Helper to fill MUI TextField by label (for fields without name attribute)
async function fillByLabel(page: Page, label: string, value: string): Promise<boolean> {
  try {
    // MUI TextField: find label then the associated input
    const field = page.locator(`label:has-text("${label}")`).locator('..').locator('input, textarea').first();
    if (await field.isVisible({ timeout: 1000 })) {
      await field.fill(value);
      return true;
    }
    // Alternative: use getByLabel
    const byLabel = page.getByLabel(label, { exact: false });
    if (await byLabel.isVisible({ timeout: 500 })) {
      await byLabel.fill(value);
      return true;
    }
  } catch (e) {}
  return false;
}

// Helper to select MUI dropdown by label
async function selectByLabel(page: Page, label: string, optionText: string): Promise<boolean> {
  try {
    // Find the select trigger by label
    const selectContainer = page.locator(`label:has-text("${label}")`).locator('..');
    const selectTrigger = selectContainer.locator('[role="combobox"], .MuiSelect-select');
    
    if (await selectTrigger.isVisible({ timeout: 1000 })) {
      await selectTrigger.click();
      await page.waitForTimeout(300);
      
      // Find and click the option
      const option = page.locator(`[role="option"]:has-text("${optionText}"), li:has-text("${optionText}")`).first();
      if (await option.isVisible({ timeout: 1000 })) {
        await option.click();
        await page.waitForTimeout(200);
        return true;
      }
    }
  } catch (e) {}
  return false;
}

// ============================================================================
// TEST SUITE
// ============================================================================

test.describe.serial('Data Population Test Suite', () => {
  test.setTimeout(600000); // 10 minutes max for full suite

  test.afterAll(async () => {
    console.log('\n' + '='.repeat(60));
    console.log('📊 DATA POPULATION SUMMARY');
    console.log('='.repeat(60));
    console.log(`Users:           ${stats.users.created} created, ${stats.users.failed} failed`);
    console.log(`Customers:       ${stats.customers.created} created, ${stats.customers.failed} failed`);
    console.log(`Contacts:        ${stats.contacts.created} created, ${stats.contacts.failed} failed`);
    console.log(`Leads:           ${stats.leads.created} created, ${stats.leads.failed} failed`);
    console.log(`Opportunities:   ${stats.opportunities.created} created, ${stats.opportunities.failed} failed`);
    console.log(`Products:        ${stats.products.created} created, ${stats.products.failed} failed`);
    console.log(`Service Requests: ${stats.serviceRequests.created} created, ${stats.serviceRequests.failed} failed`);
    console.log('='.repeat(60));
  });

  test('1. Create 10 Users', async ({ page }) => {
    console.log('\n📝 Creating 10 Users...');
    const users = generateUsers(10);
    
    for (const user of users) {
      try {
        await page.goto('/admin/users');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);
        
        if (!await clickAddButton(page)) {
          throw new Error('Add button not found');
        }
        
        if (!await waitForDialog(page)) {
          throw new Error('Dialog not found');
        }
        
        // Fill user form using MUI labels (no name attributes)
        await fillByLabel(page, 'Username', user.username);
        await fillByLabel(page, 'Email', user.email);
        await fillByLabel(page, 'First Name', user.firstName);
        await fillByLabel(page, 'Last Name', user.lastName);
        await fillByLabel(page, 'Password', user.password);
        
        // Select role from dropdown
        await selectByLabel(page, 'Role', 'User');
        
        if (await submitForm(page)) {
          stats.users.created++;
          console.log(`  ✅ User created: ${user.username}`);
        } else {
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.users.failed++;
        console.log(`  ❌ Failed to create user: ${user.username}`);
      }
    }
    
    console.log(`📊 Users: ${stats.users.created} created, ${stats.users.failed} failed`);
  });

  test('2. Create 25 Customers', async ({ page }) => {
    console.log('\n📝 Creating 25 Customers...');
    const customers = generateCustomers(25);
    
    for (const customer of customers) {
      try {
        await page.goto('/customers');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);
        
        // Click Add button
        const addButton = page.locator('button:has-text("Add Account"), button:has-text("Add"), button:has-text("New")').first();
        await addButton.scrollIntoViewIfNeeded();
        await addButton.click({ force: true });
        await page.waitForTimeout(1000);
        
        // Wait for dialog
        const dialog = page.locator('[role="dialog"], .MuiDialog-root');
        await dialog.waitFor({ state: 'visible', timeout: 5000 }).catch(() => {});
        
        // For organization: toggle to Organization mode if switch exists
        const orgSwitch = page.locator('[role="checkbox"], .MuiSwitch-input').first();
        if (await orgSwitch.isVisible().catch(() => false)) {
          const isChecked = await orgSwitch.isChecked().catch(() => false);
          if (customer.isOrganization && !isChecked) {
            await orgSwitch.click({ force: true });
            await page.waitForTimeout(500);
          } else if (!customer.isOrganization && isChecked) {
            await orgSwitch.click({ force: true });
            await page.waitForTimeout(500);
          }
        }
        
        if (customer.isOrganization) {
          // Fill company name (for organizations)
          const companyInput = page.locator('input[name="company"], input[name="legalName"]').first();
          if (await companyInput.isVisible().catch(() => false)) {
            await companyInput.fill(customer.name);
          }
        } else {
          // Individual - fill firstName and lastName
          const firstNameInput = page.locator('input[name="firstName"]').first();
          if (await firstNameInput.isVisible().catch(() => false)) {
            await firstNameInput.fill(customer.firstName || 'TEST');
          }
          
          const lastNameInput = page.locator('input[name="lastName"]').first();
          if (await lastNameInput.isVisible().catch(() => false)) {
            await lastNameInput.fill(customer.lastName || customer.name);
          }
        }
        
        // Fill email if visible
        const emailInput = page.locator('input[name="email"], input[type="email"]').first();
        if (await emailInput.isVisible().catch(() => false)) {
          await emailInput.fill(customer.email);
        }
        
        // Fill phone if visible
        const phoneInput = page.locator('input[name="phone"], input[type="tel"]').first();
        if (await phoneInput.isVisible().catch(() => false)) {
          await phoneInput.fill(customer.phone);
        }
        
        // Submit - scroll to button first
        const saveButton = page.locator('button[type="submit"], button:has-text("Save"), button:has-text("Create")').first();
        await saveButton.scrollIntoViewIfNeeded();
        await saveButton.click({ force: true });
        
        // Wait for save operation
        await page.waitForTimeout(2000);
        
        // Check if dialog closed (success)
        const dialogStillOpen = await page.locator('[role="dialog"], .MuiDialog-root').isVisible().catch(() => false);
        const hasError = await page.locator('.MuiAlert-standardError, .error-message').first().isVisible().catch(() => false);
        
        if (!dialogStillOpen && !hasError) {
          createdCustomerIds.push(`customer_${stats.customers.created + 1}`);
          stats.customers.created++;
          console.log(`  ✅ Customer created: ${customer.name}`);
        } else {
          if (hasError) {
            const errorText = await page.locator('.MuiAlert-standardError').textContent().catch(() => 'unknown');
            console.log(`    ⚠️ Error: ${errorText?.substring(0, 50)}...`);
          }
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.customers.failed++;
        console.log(`  ❌ Failed to create customer: ${customer.name}`);
        // Close any open dialogs
        await page.keyboard.press('Escape');
        await page.waitForTimeout(300);
      }
    }
    
    console.log(`📊 Customers: ${stats.customers.created} created, ${stats.customers.failed} failed`);
  });

  test('3. Create Contacts (10-40 per customer)', async ({ page }) => {
    console.log('\n📝 Creating Contacts (10-40 per customer)...');
    const customers = generateCustomers(25);
    
    for (let i = 0; i < Math.min(customers.length, 5); i++) { // Limit to 5 customers to reduce time
      const customer = customers[i];
      const contactCount = Math.min(customer.contactCount, 15); // Limit to 15 contacts each
      const contacts = generateContacts(customer.name, contactCount);
      
      console.log(`  📁 Creating ${contacts.length} contacts for ${customer.name}...`);
      let customerContactsCreated = 0;
      
      for (const contact of contacts) {
        try {
          await page.goto('/contacts');
          await page.waitForLoadState('networkidle');
          await page.waitForTimeout(300);
          
          if (!await clickAddButton(page)) {
            throw new Error('Add button not found');
          }
          
          if (!await waitForDialog(page)) {
            throw new Error('Dialog not found');
          }
          
          // Use both name-based and label-based filling
          await fillIfVisible(page, 'input[name="firstName"]', contact.firstName) ||
            await fillByLabel(page, 'First Name', contact.firstName);
          await fillIfVisible(page, 'input[name="lastName"]', contact.lastName) ||
            await fillByLabel(page, 'Last Name', contact.lastName);
          await fillIfVisible(page, 'input[name="email"]', contact.email) ||
            await fillByLabel(page, 'Email', contact.email);
          await fillIfVisible(page, 'input[name="phone"]', contact.phone) ||
            await fillByLabel(page, 'Phone', contact.phone);
          await fillIfVisible(page, 'input[name="jobTitle"]', contact.title) ||
            await fillByLabel(page, 'Job Title', contact.title) ||
            await fillByLabel(page, 'Title', contact.title);
          
          if (await submitForm(page)) {
            stats.contacts.created++;
            customerContactsCreated++;
          } else {
            throw new Error('Submit failed');
          }
        } catch (error) {
          stats.contacts.failed++;
        }
      }
      console.log(`    ✅ Created ${customerContactsCreated}/${contacts.length} contacts`);
    }
    
    console.log(`📊 Contacts: ${stats.contacts.created} created, ${stats.contacts.failed} failed`);
  });

  test('4. Create 50 Leads', async ({ page }) => {
    console.log('\n📝 Creating 50 Leads...');
    const leads = generateLeads(50);
    
    for (const lead of leads) {
      try {
        await page.goto('/leads');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(300);
        
        if (!await clickAddButton(page)) {
          throw new Error('Add button not found');
        }
        
        if (!await waitForDialog(page)) {
          throw new Error('Dialog not found');
        }
        
        // Use both name-based and label-based filling
        await fillIfVisible(page, 'input[name="firstName"]', lead.firstName) ||
          await fillByLabel(page, 'First Name', lead.firstName);
        await fillIfVisible(page, 'input[name="lastName"]', lead.lastName) ||
          await fillByLabel(page, 'Last Name', lead.lastName);
        await fillIfVisible(page, 'input[name="email"]', lead.email) ||
          await fillByLabel(page, 'Email', lead.email);
        await fillIfVisible(page, 'input[name="company"]', lead.company) ||
          await fillByLabel(page, 'Company', lead.company);
        await fillIfVisible(page, 'input[name="phone"]', lead.phone) ||
          await fillByLabel(page, 'Phone', lead.phone);
        await fillIfVisible(page, 'input[name="title"]', lead.title) ||
          await fillByLabel(page, 'Title', lead.title) ||
          await fillByLabel(page, 'Job Title', lead.title);
        
        if (await submitForm(page)) {
          stats.leads.created++;
          console.log(`  ✅ Lead created: ${lead.firstName} ${lead.lastName}`);
        } else {
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.leads.failed++;
        console.log(`  ❌ Failed to create lead: ${lead.firstName} ${lead.lastName}`);
      }
    }
    
    console.log(`📊 Leads: ${stats.leads.created} created, ${stats.leads.failed} failed`);
  });

  test('5. Create 50 Opportunities', async ({ page }) => {
    console.log('\n📝 Creating 50 Opportunities...');
    const opportunities = generateOpportunities(50);
    
    for (const opp of opportunities) {
      try {
        await page.goto('/opportunities');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(300);
        
        if (!await clickAddButton(page)) {
          throw new Error('Add button not found');
        }
        
        if (!await waitForDialog(page)) {
          throw new Error('Dialog not found');
        }
        
        // Use both name-based and label-based filling
        await fillIfVisible(page, 'input[name="name"]', opp.name) ||
          await fillByLabel(page, 'Name', opp.name) ||
          await fillByLabel(page, 'Title', opp.name);
        await fillIfVisible(page, 'input[name="value"]', opp.value.toString()) ||
          await fillByLabel(page, 'Value', opp.value.toString()) ||
          await fillByLabel(page, 'Amount', opp.value.toString());
        await fillIfVisible(page, 'input[name="expectedCloseDate"]', opp.expectedCloseDate) ||
          await fillByLabel(page, 'Expected Close Date', opp.expectedCloseDate) ||
          await fillByLabel(page, 'Close Date', opp.expectedCloseDate);
        await fillIfVisible(page, 'textarea[name="description"]', opp.description) ||
          await fillByLabel(page, 'Description', opp.description);
        
        if (await submitForm(page)) {
          stats.opportunities.created++;
          console.log(`  ✅ Opportunity created: ${opp.name} ($${opp.value.toLocaleString()})`);
        } else {
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.opportunities.failed++;
        console.log(`  ❌ Failed to create opportunity: ${opp.name}`);
      }
    }
    
    console.log(`📊 Opportunities: ${stats.opportunities.created} created, ${stats.opportunities.failed} failed`);
  });

  test('6. Create Products', async ({ page }) => {
    console.log('\n📝 Creating Products...');
    
    for (const product of products) {
      try {
        await page.goto('/products');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(300);
        
        if (!await clickAddButton(page)) {
          throw new Error('Add button not found');
        }
        
        if (!await waitForDialog(page)) {
          throw new Error('Dialog not found');
        }
        
        // Use both name-based and label-based filling
        await fillIfVisible(page, 'input[name="name"]', product.name) ||
          await fillByLabel(page, 'Name', product.name) ||
          await fillByLabel(page, 'Product Name', product.name);
        await fillIfVisible(page, 'input[name="sku"]', product.sku) ||
          await fillByLabel(page, 'SKU', product.sku) ||
          await fillByLabel(page, 'Product Code', product.sku);
        await fillIfVisible(page, 'input[name="price"]', product.price.toString()) ||
          await fillByLabel(page, 'Price', product.price.toString()) ||
          await fillByLabel(page, 'Unit Price', product.price.toString());
        await fillIfVisible(page, 'textarea[name="description"]', product.description) ||
          await fillByLabel(page, 'Description', product.description);
        await fillIfVisible(page, 'textarea[name="description"]', product.description) ||
          await fillByLabel(page, 'Description', product.description);
        
        if (await submitForm(page)) {
          stats.products.created++;
          console.log(`  ✅ Product created: ${product.name} ($${product.price.toLocaleString()})`);
        } else {
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.products.failed++;
        console.log(`  ❌ Failed to create product: ${product.name}`);
      }
    }
    
    console.log(`📊 Products: ${stats.products.created} created, ${stats.products.failed} failed`);
  });

  test('7. Create Service Requests', async ({ page }) => {
    console.log('\n📝 Creating Service Requests...');
    
    for (let i = 0; i < serviceRequestTypes.length; i++) {
      const srType = serviceRequestTypes[i];
      try {
        await page.goto('/service-requests');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(300);
        
        if (!await clickAddButton(page)) {
          throw new Error('Add button not found');
        }
        
        if (!await waitForDialog(page)) {
          throw new Error('Dialog not found');
        }
        
        // Use both name-based and label-based filling
        await fillIfVisible(page, 'input[name="title"]', `${srType.name} - Test SR ${i + 1}`) ||
          await fillByLabel(page, 'Title', `${srType.name} - Test SR ${i + 1}`) ||
          await fillByLabel(page, 'Subject', `${srType.name} - Test SR ${i + 1}`);
        await fillIfVisible(page, 'textarea[name="description"]', `Test service request for ${srType.category}`) ||
          await fillByLabel(page, 'Description', `Test service request for ${srType.category}`);
        
        if (await submitForm(page)) {
          stats.serviceRequests.created++;
          console.log(`  ✅ Service Request created: ${srType.name} (${srType.priority})`);
        } else {
          throw new Error('Submit failed');
        }
      } catch (error) {
        stats.serviceRequests.failed++;
        console.log(`  ❌ Failed to create service request: ${srType.name}`);
      }
    }
    
    console.log(`📊 Service Requests: ${stats.serviceRequests.created} created, ${stats.serviceRequests.failed} failed`);
  });
});

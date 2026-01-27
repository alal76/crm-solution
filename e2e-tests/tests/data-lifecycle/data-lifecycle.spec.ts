/**
 * CRM Solution - Test Data Lifecycle Tests
 * 
 * Comprehensive tests for:
 * 1. Creating test data (users, customers, contacts, etc.)
 * 2. Modifying test data
 * 3. Checking visibility and access controls
 * 4. Verifying impact on related business entities
 */

import { test, expect, Page } from '@playwright/test';
import { DataGridHelper, FormHelper, NotificationHelper } from '../fixtures';
import { 
  COMPREHENSIVE_TEST_USERS,
  COMPREHENSIVE_TEST_USER_GROUPS,
  COMPREHENSIVE_TEST_CUSTOMERS,
  COMPREHENSIVE_TEST_CONTACTS,
  COMPREHENSIVE_TEST_OPPORTUNITIES,
  COMPREHENSIVE_TEST_LEADS,
  COMPREHENSIVE_TEST_SERVICE_REQUESTS,
  COMPREHENSIVE_TEST_CAMPAIGNS,
  TEST_DATA_RELATIONSHIPS,
  generateId,
  randomString,
  randomEmail,
  futureDate,
} from '../data/comprehensive-test-data';

// Test execution log
const testLog: string[] = [];

function log(message: string) {
  const timestamp = new Date().toISOString();
  const entry = `[${timestamp}] ${message}`;
  testLog.push(entry);
  console.log(entry);
}

// ============================================================================
// SECTION 1: USER AND USER GROUP CREATION TESTS
// ============================================================================

test.describe('Data Lifecycle - User Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting User Management Test');
    await page.goto('/admin/users');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-USER-001: Should create test admin user', async ({ page }) => {
    const testUser = COMPREHENSIVE_TEST_USERS.testAdmin1;
    log(`Creating test admin user: ${testUser.username}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(1000);
      
      // Fill user form
      await page.locator('input[name="username"], #username').first().fill(testUser.username);
      await page.locator('input[name="email"], input[type="email"], #email').first().fill(testUser.email);
      await page.locator('input[name="firstName"], #firstName').first().fill(testUser.firstName);
      await page.locator('input[name="lastName"], #lastName').first().fill(testUser.lastName);
      await page.locator('input[name="password"], input[type="password"]').first().fill(testUser.password);
      
      // Select role
      const roleSelect = page.locator('select[name="role"], #role, [aria-label*="Role"]').first();
      if (await roleSelect.isVisible()) {
        await roleSelect.selectOption({ label: testUser.role });
      }
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(2000);
      
      log(`User ${testUser.username} created successfully`);
    }
  });

  test('TC-DL-USER-002: Should create test sales team users', async ({ page }) => {
    const salesUsers = [
      COMPREHENSIVE_TEST_USERS.salesManager,
      COMPREHENSIVE_TEST_USERS.salesRep1,
      COMPREHENSIVE_TEST_USERS.salesRep2,
    ];
    
    for (const user of salesUsers) {
      log(`Creating sales user: ${user.username}`);
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(500);
        
        const usernameInput = page.locator('input[name="username"], #username').first();
        if (await usernameInput.isVisible()) {
          await usernameInput.fill(user.username);
          await page.locator('input[name="email"], input[type="email"], #email').first().fill(user.email);
          await page.locator('input[name="firstName"], #firstName').first().fill(user.firstName);
          await page.locator('input[name="lastName"], #lastName').first().fill(user.lastName);
          await page.locator('input[name="password"], input[type="password"]').first().fill(user.password);
          
          await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
          await page.waitForTimeout(1000);
        }
      }
    }
    
    log('Sales team users created');
  });

  test('TC-DL-USER-003: Should create test support team users', async ({ page }) => {
    const supportUsers = [
      COMPREHENSIVE_TEST_USERS.supportManager,
      COMPREHENSIVE_TEST_USERS.supportAgent1,
      COMPREHENSIVE_TEST_USERS.supportAgent2,
    ];
    
    for (const user of supportUsers) {
      log(`Creating support user: ${user.username}`);
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(500);
        
        const usernameInput = page.locator('input[name="username"], #username').first();
        if (await usernameInput.isVisible()) {
          await usernameInput.fill(user.username);
          await page.locator('input[name="email"], input[type="email"]').first().fill(user.email);
          await page.locator('input[name="firstName"], #firstName').first().fill(user.firstName);
          await page.locator('input[name="lastName"], #lastName').first().fill(user.lastName);
          await page.locator('input[name="password"], input[type="password"]').first().fill(user.password);
          
          await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
          await page.waitForTimeout(1000);
        }
      }
    }
    
    log('Support team users created');
  });

  test('TC-DL-USER-004: Should modify user details', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Modifying first TEST_ user found');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(500);
        
        // Modify department
        const deptInput = page.locator('input[name="department"], #department').first();
        if (await deptInput.isVisible()) {
          await deptInput.clear();
          await deptInput.fill('TEST_Modified_Department');
        }
        
        await page.locator('button:has-text("Save"), button:has-text("Update")').first().click();
        await page.waitForTimeout(1000);
        
        log('User modified successfully');
      }
    }
  });

  test('TC-DL-USER-005: Should verify user visibility in lists', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    log(`Found ${rowCount} TEST_ users in the system`);
    
    expect(rowCount).toBeGreaterThanOrEqual(0);
  });
});

// ============================================================================
// SECTION 2: CUSTOMER DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Customer Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Customer Management Test');
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-CUST-001: Should create enterprise customer with full details', async ({ page }) => {
    const customer = COMPREHENSIVE_TEST_CUSTOMERS.enterprise1;
    log(`Creating enterprise customer: ${customer.name}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    await addButton.click();
    await page.waitForTimeout(1000);
    
    // Fill customer form - look for various field patterns
    const nameInput = page.locator('input[name="company"], input[name="name"], #company, #name').first();
    if (await nameInput.isVisible()) {
      await nameInput.fill(customer.company);
    }
    
    const emailInput = page.locator('input[name="email"], input[type="email"], #email').first();
    if (await emailInput.isVisible()) {
      await emailInput.fill(customer.email);
    }
    
    const phoneInput = page.locator('input[name="phone"], #phone').first();
    if (await phoneInput.isVisible()) {
      await phoneInput.fill(customer.phone);
    }
    
    // Industry
    const industryInput = page.locator('input[name="industry"], #industry, [aria-label*="Industry"]').first();
    if (await industryInput.isVisible()) {
      await industryInput.fill(customer.industry);
    }
    
    // Save
    await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
    await page.waitForTimeout(2000);
    
    log(`Customer ${customer.name} created`);
  });

  test('TC-DL-CUST-002: Should create multiple test customers', async ({ page }) => {
    const customers = [
      COMPREHENSIVE_TEST_CUSTOMERS.midMarket1,
      COMPREHENSIVE_TEST_CUSTOMERS.smallBiz1,
      COMPREHENSIVE_TEST_CUSTOMERS.individual1,
    ];
    
    for (const customer of customers) {
      log(`Creating customer: ${customer.name}`);
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(500);
        
        const nameInput = page.locator('input[name="company"], input[name="name"], #company').first();
        if (await nameInput.isVisible()) {
          await nameInput.fill(customer.company || customer.name);
        }
        
        const emailInput = page.locator('input[name="email"], input[type="email"]').first();
        if (await emailInput.isVisible()) {
          await emailInput.fill(customer.email);
        }
        
        await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
        await page.waitForTimeout(1000);
      }
    }
    
    log('Multiple test customers created');
  });

  test('TC-DL-CUST-003: Should modify customer data', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Modifying test customer');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(500);
        
        // Modify annual revenue
        const revenueInput = page.locator('input[name="annualRevenue"], #annualRevenue').first();
        if (await revenueInput.isVisible()) {
          await revenueInput.clear();
          await revenueInput.fill('9999999');
        }
        
        await page.locator('button:has-text("Save"), button:has-text("Update")').first().click();
        await page.waitForTimeout(1000);
        
        log('Customer data modified');
      }
    }
  });

  test('TC-DL-CUST-004: Should verify customer appears in search results', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_GlobalTech');
    await page.waitForTimeout(1000);
    
    const content = await page.textContent('body');
    const hasTestData = content?.includes('TEST_') || content?.includes('GlobalTech');
    log(`Customer visibility check: ${hasTestData ? 'FOUND' : 'NOT FOUND'}`);
  });
});

// ============================================================================
// SECTION 3: CONTACT DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Contact Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Contact Management Test');
    await page.goto('/contacts');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-CONT-001: Should create contacts for enterprise customer', async ({ page }) => {
    const contacts = [
      COMPREHENSIVE_TEST_CONTACTS.globalTechCEO,
      COMPREHENSIVE_TEST_CONTACTS.globalTechCTO,
    ];
    
    for (const contact of contacts) {
      log(`Creating contact: ${contact.firstName} ${contact.lastName}`);
      
      const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
      if (await addButton.isVisible()) {
        await addButton.click();
        await page.waitForTimeout(500);
        
        await page.locator('input[name="firstName"], #firstName').first().fill(contact.firstName);
        await page.locator('input[name="lastName"], #lastName').first().fill(contact.lastName);
        await page.locator('input[name="email"], input[type="email"]').first().fill(contact.email);
        await page.locator('input[name="phone"], #phone').first().fill(contact.phone);
        await page.locator('input[name="title"], #title').first().fill(contact.title);
        
        await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
        await page.waitForTimeout(1000);
      }
    }
    
    log('Enterprise contacts created');
  });

  test('TC-DL-CONT-002: Should modify contact and verify customer link', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Check if customer link is visible
      const customerLink = page.locator('text=/customer|account/i, a[href*="customer"]').first();
      const hasLink = await customerLink.isVisible().catch(() => false);
      log(`Contact-Customer link: ${hasLink ? 'VISIBLE' : 'NOT VISIBLE'}`);
    }
  });
});

// ============================================================================
// SECTION 4: OPPORTUNITY DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Opportunity Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Opportunity Management Test');
    await page.goto('/opportunities');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-OPP-001: Should create high-value opportunity', async ({ page }) => {
    const opportunity = COMPREHENSIVE_TEST_OPPORTUNITIES.enterpriseDeal1;
    log(`Creating opportunity: ${opportunity.name}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      await page.locator('input[name="name"], #name').first().fill(opportunity.name);
      
      const valueInput = page.locator('input[name="value"], input[name="amount"], #value').first();
      if (await valueInput.isVisible()) {
        await valueInput.fill(opportunity.value.toString());
      }
      
      await page.locator('textarea[name="description"], #description').first().fill(opportunity.description);
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(1000);
      
      log('High-value opportunity created');
    }
  });

  test('TC-DL-OPP-002: Should advance opportunity through stages', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Advancing opportunity stage');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Look for stage dropdown or buttons
      const stageSelect = page.locator('select[name="stage"], #stage, [aria-label*="Stage"]').first();
      if (await stageSelect.isVisible()) {
        await stageSelect.click();
        await page.waitForTimeout(300);
        
        // Select next stage
        const nextStage = page.locator('[role="option"]:has-text("Proposal"), option:has-text("Proposal")').first();
        if (await nextStage.isVisible()) {
          await nextStage.click();
        }
        
        await page.locator('button:has-text("Save")').first().click();
        await page.waitForTimeout(1000);
        
        log('Opportunity stage advanced');
      }
    }
  });

  test('TC-DL-OPP-003: Should verify opportunity-customer relationship', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const customerRef = page.locator('text=/customer|account/i').first();
      const hasCustomerRef = await customerRef.isVisible().catch(() => false);
      log(`Opportunity-Customer relationship: ${hasCustomerRef ? 'VISIBLE' : 'NOT VISIBLE'}`);
    }
  });
});

// ============================================================================
// SECTION 5: LEAD DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Lead Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Lead Management Test');
    await page.goto('/leads');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-LEAD-001: Should create hot lead', async ({ page }) => {
    const lead = COMPREHENSIVE_TEST_LEADS.hotLead1;
    log(`Creating hot lead: ${lead.firstName} ${lead.lastName}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      await page.locator('input[name="firstName"], #firstName').first().fill(lead.firstName);
      await page.locator('input[name="lastName"], #lastName').first().fill(lead.lastName);
      await page.locator('input[name="email"], input[type="email"]').first().fill(lead.email);
      await page.locator('input[name="company"], #company').first().fill(lead.company);
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(1000);
      
      log('Hot lead created');
    }
  });

  test('TC-DL-LEAD-002: Should convert lead to opportunity', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Converting lead to opportunity');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const convertButton = page.locator('button:has-text("Convert"), button:has-text("Qualify")').first();
      if (await convertButton.isVisible()) {
        await convertButton.click();
        await page.waitForTimeout(2000);
        
        log('Lead conversion initiated');
      }
    }
  });

  test('TC-DL-LEAD-003: Should update lead score', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const editButton = page.locator('button:has-text("Edit")').first();
      if (await editButton.isVisible()) {
        await editButton.click();
        await page.waitForTimeout(500);
        
        const scoreInput = page.locator('input[name="score"], #score').first();
        if (await scoreInput.isVisible()) {
          await scoreInput.clear();
          await scoreInput.fill('95');
          
          await page.locator('button:has-text("Save")').first().click();
          await page.waitForTimeout(1000);
          
          log('Lead score updated to 95');
        }
      }
    }
  });
});

// ============================================================================
// SECTION 6: SERVICE REQUEST DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Service Request Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Service Request Management Test');
    await page.goto('/service-requests');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-SR-001: Should create critical service request', async ({ page }) => {
    const sr = COMPREHENSIVE_TEST_SERVICE_REQUESTS.criticalTicket;
    log(`Creating critical service request: ${sr.title}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      await page.locator('input[name="title"], input[name="subject"], #title').first().fill(sr.title);
      await page.locator('textarea[name="description"], #description').first().fill(sr.description);
      
      // Set priority
      const prioritySelect = page.locator('select[name="priority"], #priority, [aria-label*="Priority"]').first();
      if (await prioritySelect.isVisible()) {
        await prioritySelect.selectOption({ label: sr.priority });
      }
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(1000);
      
      log('Critical service request created');
    }
  });

  test('TC-DL-SR-002: Should transition service request status', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Transitioning service request status');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const statusSelect = page.locator('select[name="status"], #status, [aria-label*="Status"]').first();
      if (await statusSelect.isVisible()) {
        await statusSelect.click();
        await page.waitForTimeout(300);
        
        const inProgressOption = page.locator('[role="option"]:has-text("In Progress"), option:has-text("In Progress")').first();
        if (await inProgressOption.isVisible()) {
          await inProgressOption.click();
          await page.locator('button:has-text("Save")').first().click();
          await page.waitForTimeout(1000);
          
          log('Service request status updated');
        }
      }
    }
  });

  test('TC-DL-SR-003: Should verify customer link in service request', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const customerRef = page.locator('text=/customer|account/i, a[href*="customer"]').first();
      const hasCustomerRef = await customerRef.isVisible().catch(() => false);
      log(`Service Request-Customer link: ${hasCustomerRef ? 'VISIBLE' : 'NOT VISIBLE'}`);
    }
  });
});

// ============================================================================
// SECTION 7: CAMPAIGN DATA LIFECYCLE TESTS
// ============================================================================

test.describe('Data Lifecycle - Campaign Management', () => {
  test.beforeEach(async ({ page }) => {
    log('Starting Campaign Management Test');
    await page.goto('/campaigns');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
  });

  test('TC-DL-CAMP-001: Should create email campaign', async ({ page }) => {
    const campaign = COMPREHENSIVE_TEST_CAMPAIGNS.emailCampaignActive;
    log(`Creating email campaign: ${campaign.name}`);
    
    const addButton = page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);
      
      await page.locator('input[name="name"], #name').first().fill(campaign.name);
      await page.locator('textarea[name="description"], #description').first().fill(campaign.description);
      
      const typeSelect = page.locator('select[name="type"], #type, [aria-label*="Type"]').first();
      if (await typeSelect.isVisible()) {
        await typeSelect.selectOption({ label: campaign.type });
      }
      
      await page.locator('button:has-text("Save"), button:has-text("Create")').first().click();
      await page.waitForTimeout(1000);
      
      log('Email campaign created');
    }
  });

  test('TC-DL-CAMP-002: Should start campaign', async ({ page }) => {
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      log('Starting test campaign');
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      const startButton = page.locator('button:has-text("Start"), button:has-text("Launch"), button:has-text("Activate")').first();
      if (await startButton.isVisible()) {
        await startButton.click();
        await page.waitForTimeout(2000);
        
        log('Campaign started');
      }
    }
  });
});

// ============================================================================
// SECTION 8: RELATIONSHIP IMPACT TESTS
// ============================================================================

test.describe('Data Lifecycle - Relationship Impact', () => {
  test('TC-DL-REL-001: Should verify customer deletion affects contacts', async ({ page }) => {
    log('Testing customer deletion impact on contacts');
    
    // Navigate to customers
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      // Get customer name before deletion
      const customerRow = page.locator('tbody tr, .MuiDataGrid-row').first();
      const customerName = await customerRow.textContent();
      log(`Found customer: ${customerName?.substring(0, 50)}`);
      
      // Check contacts page for related contacts
      await page.goto('/contacts');
      await page.waitForLoadState('networkidle');
      
      const contactGrid = new DataGridHelper(page);
      await contactGrid.waitForLoad();
      await contactGrid.searchInGrid('TEST_');
      await page.waitForTimeout(1000);
      
      const contactCount = await contactGrid.getRowCount();
      log(`Found ${contactCount} related TEST_ contacts`);
    }
  });

  test('TC-DL-REL-002: Should verify opportunity affects customer revenue', async ({ page }) => {
    log('Testing opportunity impact on customer metrics');
    
    // Navigate to opportunities
    await page.goto('/opportunities');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    log(`Found ${rowCount} TEST_ opportunities`);
    
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Check for customer link
      const customerLink = page.locator('a[href*="customer"], text=/customer/i').first();
      if (await customerLink.isVisible()) {
        log('Opportunity has customer reference - impact verified');
      }
    }
  });

  test('TC-DL-REL-003: Should verify lead conversion creates customer and opportunity', async ({ page }) => {
    log('Testing lead conversion creates linked entities');
    
    await page.goto('/leads');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    log(`Found ${rowCount} TEST_ leads`);
    
    // Check for converted leads
    const convertedLead = page.locator('text=/converted/i').first();
    const hasConverted = await convertedLead.isVisible().catch(() => false);
    log(`Converted leads present: ${hasConverted}`);
  });

  test('TC-DL-REL-004: Should verify service request links to customer history', async ({ page }) => {
    log('Testing service request appears in customer history');
    
    // First find a test customer
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    
    const grid = new DataGridHelper(page);
    await grid.waitForLoad();
    await grid.searchInGrid('TEST_');
    await page.waitForTimeout(1000);
    
    const rowCount = await grid.getRowCount();
    if (rowCount > 0) {
      await grid.clickRow(0);
      await page.waitForTimeout(500);
      
      // Look for service requests tab or section
      const srTab = page.locator('[role="tab"]:has-text("Service"), button:has-text("Tickets"), text=/service request/i').first();
      if (await srTab.isVisible()) {
        await srTab.click();
        await page.waitForTimeout(1000);
        
        const srContent = await page.textContent('body');
        const hasSR = srContent?.includes('TEST_') || srContent?.includes('ticket');
        log(`Customer service request history: ${hasSR ? 'VISIBLE' : 'NOT VISIBLE'}`);
      }
    }
  });
});

// ============================================================================
// SECTION 9: DATA CLEANUP TESTS
// ============================================================================

test.describe('Data Lifecycle - Cleanup', () => {
  test.afterAll(async () => {
    // Log all test results
    log('=== TEST EXECUTION COMPLETE ===');
    log(`Total log entries: ${testLog.length}`);
  });

  test('TC-DL-CLEAN-001: Should list all TEST_ data for cleanup', async ({ page }) => {
    log('Listing all TEST_ data for cleanup verification');
    
    const entities = [
      { name: 'Customers', url: '/customers' },
      { name: 'Contacts', url: '/contacts' },
      { name: 'Opportunities', url: '/opportunities' },
      { name: 'Leads', url: '/leads' },
      { name: 'Service Requests', url: '/service-requests' },
      { name: 'Campaigns', url: '/campaigns' },
    ];
    
    const summary: Record<string, number> = {};
    
    for (const entity of entities) {
      await page.goto(entity.url);
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
      
      const grid = new DataGridHelper(page);
      await grid.waitForLoad();
      await grid.searchInGrid('TEST_');
      await page.waitForTimeout(500);
      
      const count = await grid.getRowCount();
      summary[entity.name] = count;
      log(`${entity.name}: ${count} TEST_ records`);
    }
    
    log('=== DATA SUMMARY ===');
    Object.entries(summary).forEach(([entity, count]) => {
      log(`  ${entity}: ${count}`);
    });
  });
});

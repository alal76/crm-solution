/**
 * CRM Solution - UI Functionality Test: Create Accounts with Linked Contacts
 * Creates 10 accounts via UI and links random contacts (2-25 per account)
 */

import { test, expect, Page, request } from '@playwright/test';

const TEST_ACCOUNTS = [
  { firstName: 'Michael', lastName: 'Chen', company: 'Quantum Dynamics Labs', industry: 'Technology', email: 'info@quantumdynamics.io', phone: '555-1001', address: '100 Quantum Plaza', city: 'San Jose', state: 'California', zipCode: '95110', country: 'USA', notes: 'AI and quantum computing' },
  { firstName: 'Sarah', lastName: 'Mitchell', company: 'Horizon Pharmaceuticals', industry: 'Healthcare', email: 'contact@horizonpharma.com', phone: '555-1002', address: '200 Research Drive', city: 'Cambridge', state: 'Massachusetts', zipCode: '02142', country: 'USA', notes: 'Pharmaceutical R&D' },
  { firstName: 'David', lastName: 'Rodriguez', company: 'Nexus Logistics Group', industry: 'Transportation', email: 'ops@nexuslogistics.net', phone: '555-1003', address: '300 Freight Terminal', city: 'Memphis', state: 'Tennessee', zipCode: '38118', country: 'USA', notes: 'Freight logistics' },
  { firstName: 'Emily', lastName: 'Nakamura', company: 'Evergreen Sustainable', industry: 'Energy', email: 'hello@evergreen.eco', phone: '555-1004', address: '400 Solar Blvd', city: 'Las Vegas', state: 'Nevada', zipCode: '89101', country: 'USA', notes: 'Renewable energy' },
  { firstName: 'Robert', lastName: 'Goldstein', company: 'Atlas Capital Partners', industry: 'Financial Services', email: 'invest@atlascap.com', phone: '555-1005', address: '500 Wall Tower', city: 'New York', state: 'New York', zipCode: '10005', country: 'USA', notes: 'Private equity' },
  { firstName: 'Maria', lastName: 'Gonzalez', company: 'Coastal Hospitality', industry: 'Hospitality', email: 'res@coastal.com', phone: '555-1006', address: '600 Ocean Blvd', city: 'Miami', state: 'Florida', zipCode: '33139', country: 'USA', notes: 'Hotel management' },
  { firstName: 'James', lastName: 'Peterson', company: 'Apex Aerospace', industry: 'Manufacturing', email: 'sales@apexaero.com', phone: '555-1007', address: '700 Aviation Way', city: 'Wichita', state: 'Kansas', zipCode: '67201', country: 'USA', notes: 'Aircraft components' },
  { firstName: 'Alexandra', lastName: 'Kim', company: 'Digital Frontier', industry: 'Media', email: 'studio@dfrontier.tv', phone: '555-1008', address: '800 Hollywood', city: 'Los Angeles', state: 'California', zipCode: '90028', country: 'USA', notes: 'Film production' },
  { firstName: 'Thomas', lastName: 'O\'Brien', company: 'TerraForm Build', industry: 'Construction', email: 'proj@terraform.com', phone: '555-1009', address: '900 Builder Ave', city: 'Dallas', state: 'Texas', zipCode: '75201', country: 'USA', notes: 'Construction' },
  { firstName: 'Priya', lastName: 'Sharma', company: 'Nova EdTech', industry: 'Education', email: 'learn@novaed.com', phone: '555-1010', address: '1000 Campus Cir', city: 'Boulder', state: 'Colorado', zipCode: '80301', country: 'USA', notes: 'EdTech platform' }
];

const FIRST_NAMES = ['James', 'Emma', 'William', 'Olivia', 'Benjamin', 'Ava', 'Lucas', 'Sophia', 'Henry', 'Isabella', 'Alexander', 'Mia', 'Daniel', 'Charlotte', 'Matthew', 'Amelia', 'Joseph', 'Harper', 'Samuel', 'Evelyn', 'David', 'Abigail', 'Andrew', 'Emily', 'Christopher'];
const LAST_NAMES = ['Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez', 'Hernandez', 'Lopez', 'Gonzalez', 'Wilson', 'Anderson', 'Thomas', 'Taylor', 'Moore', 'Jackson', 'Martin', 'Lee', 'Perez', 'Thompson', 'White', 'Harris', 'Sanchez'];
const JOB_TITLES = ['CEO', 'CFO', 'CTO', 'VP Sales', 'Director', 'Manager', 'Engineer', 'Analyst'];
const DEPARTMENTS = ['Executive', 'Sales', 'Marketing', 'Engineering', 'Operations', 'Finance', 'HR', 'IT'];

async function fillFieldByLabel(page: Page, label: string, value: string, dialog?: any): Promise<boolean> {
  try {
    // Scope to dialog if provided
    const container = dialog || page;
    
    // Use getByLabel which works well with Material UI
    const field = container.getByLabel(label, { exact: false }).first();
    if (await field.isVisible({ timeout: 500 }).catch(() => false)) {
      await field.clear();
      await field.fill(value);
      console.log(`    ✓ Filled "${label}" = "${value}" via label`);
      return true;
    }
    // Fallback 2: try common field name mappings
    const nameMap: Record<string, string> = {
      'First Name': 'firstName',
      'Last Name': 'lastName',
      'Company Name': 'company',
      'Email': 'email',
      'Phone': 'phone',
      'Address': 'address',
      'City': 'city',
      'State': 'state',
      'Zip Code': 'zipCode',
      'Country': 'country',
      'Industry': 'industry',
      'Notes': 'notes'
    };
    const fieldName = nameMap[label];
    if (fieldName) {
      const byFieldName = container.locator(`input[name="${fieldName}"], textarea[name="${fieldName}"]`).first();
      if (await byFieldName.isVisible({ timeout: 200 }).catch(() => false)) {
        await byFieldName.clear();
        await byFieldName.fill(value);
        console.log(`    ✓ Filled "${label}" = "${value}" via name="${fieldName}"`);
        return true;
      }
    }
    console.log(`  ⚠️ Field "${label}" not found or not visible`);
  } catch (err) {
    console.log(`  Could not fill ${label}: ${err}`);
  }
  return false;
}

test.describe('Create 10 Accounts with Contacts via UI', () => {
  test.setTimeout(180000); // 3 minutes total
  
  test('create accounts and link contacts', async ({ page }) => {
    // Get API token
    const apiContext = await request.newContext({ baseURL: 'http://localhost:5000' });
    const loginResp = await apiContext.post('/api/auth/login', {
      data: { email: 'abhi.lal@gmail.com', password: 'Admin@123' }
    });
    const { accessToken: token } = await loginResp.json();
    
    const results: string[] = [];
    let totalContacts = 0;
    let totalLinks = 0;
    
    for (let i = 0; i < TEST_ACCOUNTS.length; i++) {
      const acct = TEST_ACCOUNTS[i];
      const numContacts = Math.floor(Math.random() * 24) + 2; // 2-25
      console.log(`[${i+1}/10] ${acct.company} (${numContacts} contacts)`);
      
      try {
        // === CREATE ACCOUNT VIA UI ===
        console.log(`  Navigating to /customers...`);
        await page.goto('/customers', { waitUntil: 'networkidle' });
        await page.waitForLoadState('domcontentloaded');
        await page.waitForTimeout(2000);
        
        // Wait for page content to load
        console.log(`  Waiting for page content...`);
        await page.waitForSelector('button, table, .MuiDataGrid-root', { timeout: 10000 });
        
        console.log(`  Looking for Add Account button...`);
        const addBtn = page.locator('button:has-text("Add Account")').first();
        const isVisible = await addBtn.isVisible().catch(() => false);
        console.log(`  Button visible: ${isVisible}`);
        
        if (!isVisible) {
          // Try alternative selectors
          const altBtn = page.getByRole('button', { name: /Add Account/i });
          console.log(`  Trying role-based selector...`);
          await altBtn.waitFor({ state: 'visible', timeout: 5000 });
          await altBtn.click();
        } else {
          await addBtn.click();
        }
        
        console.log(`  Clicked Add Account, waiting for dialog...`);
        await page.waitForTimeout(1000);
        
        // Wait for dialog to open
        const dialogEl = await page.waitForSelector('[role="dialog"]', { state: 'visible', timeout: 5000 }).catch(() => null);
        if (!dialogEl) {
          console.log(`  Dialog not found, trying alternative...`);
          // Maybe the dialog structure is different
          await page.waitForSelector('.MuiDialog-root, .MuiModal-root', { state: 'visible', timeout: 3000 }).catch(() => null);
        }
        
        // Get dialog locator for scoped field filling
        const dialog = page.locator('[role="dialog"]').first();
        console.log(`  Dialog opened, filling form...`);
        
        // FIRST: Select Organization as Account Type (Category = 1)
        // This is required to be able to link contacts later
        console.log(`  Setting Account Type to Organization...`);
        const accountTypeSelect = dialog.locator('label:has-text("Account Type")').locator('..').locator('div[role="combobox"], .MuiSelect-select').first();
        if (await accountTypeSelect.isVisible({ timeout: 1000 }).catch(() => false)) {
          await accountTypeSelect.click();
          await page.waitForTimeout(300);
          // Select "Organization" from dropdown
          const orgOption = page.locator('[role="option"]:has-text("Organization"), [role="listbox"] li:has-text("Organization")').first();
          if (await orgOption.isVisible({ timeout: 1000 }).catch(() => false)) {
            await orgOption.click();
            await page.waitForTimeout(500);
            console.log(`    ✓ Selected Organization account type`);
          } else {
            console.log(`    ⚠️ Could not find Organization option`);
          }
        } else {
          // Try alternative: look for select with name or id containing category/type
          const altSelect = dialog.locator('div[role="combobox"]').first();
          if (await altSelect.isVisible({ timeout: 500 }).catch(() => false)) {
            await altSelect.click();
            await page.waitForTimeout(300);
            const orgOpt = page.locator('[role="option"]').filter({ hasText: 'Organization' }).first();
            if (await orgOpt.isVisible({ timeout: 500 }).catch(() => false)) {
              await orgOpt.click();
              console.log(`    ✓ Selected Organization via alt selector`);
            }
          }
        }
        
        // Fill form - firstName and lastName are REQUIRED
        // Use labels that match the field configurations in the UI
        // Pass dialog to scope the field selection
        await fillFieldByLabel(page, 'First Name', acct.firstName, dialog);
        await fillFieldByLabel(page, 'Last Name', acct.lastName, dialog);
        await fillFieldByLabel(page, 'Business Name', acct.company, dialog); // Try Business Name label
        await fillFieldByLabel(page, 'Company Name', acct.company, dialog);  // Also try Company Name
        await fillFieldByLabel(page, 'Company', acct.company, dialog);       // Also try just Company
        await fillFieldByLabel(page, 'Email', acct.email, dialog);
        await fillFieldByLabel(page, 'Phone', acct.phone, dialog);
        await fillFieldByLabel(page, 'Address', acct.address, dialog);
        await fillFieldByLabel(page, 'City', acct.city, dialog);
        await fillFieldByLabel(page, 'State', acct.state, dialog);
        await fillFieldByLabel(page, 'Zip Code', acct.zipCode, dialog);
        await fillFieldByLabel(page, 'Country', acct.country, dialog);
        await fillFieldByLabel(page, 'Industry', acct.industry, dialog);
        await fillFieldByLabel(page, 'Notes', acct.notes, dialog);
        
        console.log(`  Form filled, clicking Create...`);
        await page.waitForTimeout(500); // Give UI time to register values
        const saveBtn = dialog.locator('button:has-text("Create")').first();
        const saveBtnVisible = await saveBtn.isVisible().catch(() => false);
        console.log(`  Create button visible: ${saveBtnVisible}`);
        
        if (saveBtnVisible) {
          await saveBtn.click();
        } else {
          // Try alternative
          await page.getByRole('button', { name: /Create|Save/i }).click();
        }
        
        // Wait for API response 
        console.log(`  Waiting for API response...`);
        
        // Listen for API response
        const responsePromise = page.waitForResponse(resp => 
          resp.url().includes('/api/customers') && resp.request().method() === 'POST',
          { timeout: 10000 }
        ).catch(() => null);
        
        // Give UI time to process
        await page.waitForTimeout(1500);
        
        const apiResponse = await responsePromise;
        if (apiResponse) {
          console.log(`  API Status: ${apiResponse.status()}`);
          if (apiResponse.status() !== 201 && apiResponse.status() !== 200) {
            try {
              const body = await apiResponse.json();
              console.log(`  API Error: ${JSON.stringify(body)}`);
            } catch {}
          }
        }
        
        // Check for error alert
        const errorAlert = page.locator('.MuiAlert-root:has-text("Failed"), .MuiAlert-standardError');
        const hasError = await errorAlert.isVisible({ timeout: 1000 }).catch(() => false);
        if (hasError) {
          const errorText = await errorAlert.textContent().catch(() => 'Unknown error');
          console.log(`  ❌ Error: ${errorText}`);
          results.push(`✗ ${acct.company}: ${errorText}`);
          // Close dialog
          await page.keyboard.press('Escape');
          await page.waitForTimeout(500);
          continue;
        }
        
        // Wait for dialog to close completely
        const dialogClosed = await page.waitForSelector('[role="dialog"]', { state: 'hidden', timeout: 5000 }).then(() => true).catch(() => false);
        if (!dialogClosed) {
          console.log(`  ⚠️ Dialog still open, checking for errors...`);
          const dialogError = await page.locator('[role="dialog"] .MuiAlert-root').textContent().catch(() => null);
          if (dialogError) {
            console.log(`  ❌ Dialog error: ${dialogError}`);
            results.push(`✗ ${acct.company}: ${dialogError}`);
            await page.keyboard.press('Escape');
            await page.waitForTimeout(500);
            continue;
          }
        }
        
        console.log(`  Account created via UI`);
        await page.waitForTimeout(1000);
        
        // Get created account ID via API - search by email since company may be empty for Individual accounts
        const custResp = await apiContext.get('/api/customers', { headers: { Authorization: `Bearer ${token}` } });
        const customers = await custResp.json();
        const created = customers.find((c: any) => c.email === acct.email || 
          (c.firstName === acct.firstName && c.lastName === acct.lastName));
        const acctId = created?.id;
        
        if (!acctId) {
          console.log(`  ⚠️ Account not found by email ${acct.email} or name ${acct.firstName} ${acct.lastName}`);
          results.push(`✗ ${acct.company}: Failed to create`);
          continue;
        }
        console.log(`  Found account ID: ${acctId}`);
        
        // === CREATE CONTACTS VIA API ===
        const contactIds: number[] = [];
        const domain = acct.company.toLowerCase().replace(/[^a-z]/g, '').slice(0, 10);
        
        for (let j = 0; j < numContacts; j++) {
          const fn = FIRST_NAMES[Math.floor(Math.random() * FIRST_NAMES.length)];
          const ln = LAST_NAMES[Math.floor(Math.random() * LAST_NAMES.length)];
          
          const resp = await apiContext.post('/api/contacts', {
            headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
            data: {
              firstName: fn, lastName: ln,
              email: `${fn.toLowerCase()}.${ln.toLowerCase()}${j}@${domain}.com`,
              phone: `555-${String(6000 + i*100 + j).padStart(4,'0')}`,
              mobilePhone: `555-${String(7000 + i*100 + j).padStart(4,'0')}`,
              jobTitle: JOB_TITLES[Math.floor(Math.random() * JOB_TITLES.length)],
              department: DEPARTMENTS[Math.floor(Math.random() * DEPARTMENTS.length)],
              company: acct.company, address: acct.address, city: acct.city,
              state: acct.state, zipCode: acct.zipCode, country: acct.country,
              notes: `Contact ${j+1} for ${acct.company}`, status: 0
            }
          });
          if (resp.ok()) {
            const c = await resp.json();
            contactIds.push(c.id);
            totalContacts++;
          }
        }
        
        // === LINK CONTACTS VIA API (more reliable than UI) ===
        console.log(`  Linking ${contactIds.length} contacts via API...`);
        let linkedCount = 0;
        for (let k = 0; k < contactIds.length; k++) {
          const resp = await apiContext.post(`/api/customers/${acctId}/contacts`, {
            headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
            data: { 
              contactId: contactIds[k], 
              role: k % 5, 
              isPrimaryContact: k === 0, 
              isDecisionMaker: k < 3, 
              receivesMarketingEmails: true 
            }
          });
          if (resp.ok()) {
            linkedCount++;
            totalLinks++;
          } else {
            console.log(`  ⚠️ Failed to link contact ${contactIds[k]}: ${resp.status()}`);
          }
        }
        console.log(`  Linked ${linkedCount}/${contactIds.length} contacts`);
        
        results.push(`✓ ${acct.company}: ${contactIds.length} contacts, ${linkedCount} linked`);
        
      } catch (err: any) {
        results.push(`✗ ${acct.company}: ${err.message?.slice(0,50) || 'Error'}`);
      }
    }
    
    // Summary
    console.log('\n=== SUMMARY ===');
    results.forEach(r => console.log(r));
    console.log(`Total: ${results.filter(r => r.startsWith('✓')).length}/10 accounts, ${totalContacts} contacts, ${totalLinks} links`);
    
    expect(results.filter(r => r.startsWith('✓')).length).toBeGreaterThanOrEqual(8);
    await apiContext.dispose();
  });
});

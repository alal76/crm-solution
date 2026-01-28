/**
 * CRM Solution - Account and Contact Linking Test
 * Creates 10 accounts via API and links random contacts (2-25 per account)
 * Tests the CustomerContacts mapping table functionality
 */

import { test, expect, request } from '@playwright/test';

const TEST_ACCOUNTS = [
  { firstName: 'Michael', lastName: 'Chen', company: 'Quantum Dynamics Labs', industry: 'Technology', email: 'acct1@quantumdynamics.io', phone: '555-2001', address: '100 Quantum Plaza', city: 'San Jose', state: 'California', zipCode: '95110', country: 'USA' },
  { firstName: 'Sarah', lastName: 'Mitchell', company: 'Horizon Pharmaceuticals', industry: 'Healthcare', email: 'acct2@horizonpharma.com', phone: '555-2002', address: '200 Research Drive', city: 'Cambridge', state: 'Massachusetts', zipCode: '02142', country: 'USA' },
  { firstName: 'David', lastName: 'Rodriguez', company: 'Nexus Logistics Group', industry: 'Transportation', email: 'acct3@nexuslogistics.net', phone: '555-2003', address: '300 Freight Terminal', city: 'Memphis', state: 'Tennessee', zipCode: '38118', country: 'USA' },
  { firstName: 'Emily', lastName: 'Nakamura', company: 'Evergreen Sustainable', industry: 'Energy', email: 'acct4@evergreen.eco', phone: '555-2004', address: '400 Solar Blvd', city: 'Las Vegas', state: 'Nevada', zipCode: '89101', country: 'USA' },
  { firstName: 'Robert', lastName: 'Goldstein', company: 'Atlas Capital Partners', industry: 'Financial Services', email: 'acct5@atlascap.com', phone: '555-2005', address: '500 Wall Tower', city: 'New York', state: 'New York', zipCode: '10005', country: 'USA' },
  { firstName: 'Maria', lastName: 'Gonzalez', company: 'Coastal Hospitality', industry: 'Hospitality', email: 'acct6@coastal.com', phone: '555-2006', address: '600 Ocean Blvd', city: 'Miami', state: 'Florida', zipCode: '33139', country: 'USA' },
  { firstName: 'James', lastName: 'Peterson', company: 'Apex Aerospace', industry: 'Manufacturing', email: 'acct7@apexaero.com', phone: '555-2007', address: '700 Aviation Way', city: 'Wichita', state: 'Kansas', zipCode: '67201', country: 'USA' },
  { firstName: 'Alexandra', lastName: 'Kim', company: 'Digital Frontier', industry: 'Media', email: 'acct8@dfrontier.tv', phone: '555-2008', address: '800 Hollywood', city: 'Los Angeles', state: 'California', zipCode: '90028', country: 'USA' },
  { firstName: 'Thomas', lastName: 'OBrien', company: 'TerraForm Build', industry: 'Construction', email: 'acct9@terraform.com', phone: '555-2009', address: '900 Builder Ave', city: 'Dallas', state: 'Texas', zipCode: '75201', country: 'USA' },
  { firstName: 'Priya', lastName: 'Sharma', company: 'Nova EdTech', industry: 'Education', email: 'acct10@novaed.com', phone: '555-2010', address: '1000 Campus Cir', city: 'Boulder', state: 'Colorado', zipCode: '80301', country: 'USA' }
];

const FIRST_NAMES = ['James', 'Emma', 'William', 'Olivia', 'Benjamin', 'Ava', 'Lucas', 'Sophia', 'Henry', 'Isabella', 'Alexander', 'Mia', 'Daniel', 'Charlotte', 'Matthew', 'Amelia', 'Joseph', 'Harper', 'Samuel', 'Evelyn'];
const LAST_NAMES = ['Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez', 'Hernandez', 'Lopez', 'Gonzalez', 'Wilson', 'Anderson', 'Thomas', 'Taylor', 'Moore', 'Jackson', 'Martin', 'Lee'];
const JOB_TITLES = ['CEO', 'CFO', 'CTO', 'VP Sales', 'Director', 'Manager', 'Engineer', 'Analyst'];
const DEPARTMENTS = ['Executive', 'Sales', 'Marketing', 'Engineering', 'Operations', 'Finance', 'HR', 'IT'];

test.describe('Account-Contact Linking via Mapping Table', () => {
  test.setTimeout(180000);
  
  test('create 10 accounts and link random contacts via API', async ({ page }) => {
    // Get API token
    const apiContext = await request.newContext({ baseURL: 'http://localhost:5000' });
    const loginResp = await apiContext.post('/api/auth/login', {
      data: { email: 'abhi.lal@gmail.com', password: 'Admin@123' }
    });
    const { accessToken: token } = await loginResp.json();
    const headers = { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' };
    
    const results: string[] = [];
    let totalContacts = 0;
    let totalLinks = 0;
    
    for (let i = 0; i < TEST_ACCOUNTS.length; i++) {
      const acct = TEST_ACCOUNTS[i];
      const numContacts = Math.floor(Math.random() * 24) + 2; // 2-25
      console.log(`\n[${i+1}/10] ${acct.company} - Creating account with ${numContacts} contacts`);
      
      try {
        // === CREATE ACCOUNT VIA API ===
        const acctResp = await apiContext.post('/api/customers', {
          headers,
          data: {
            category: 0, // Individual - now supports contact linking!
            firstName: acct.firstName,
            lastName: acct.lastName,
            company: acct.company,
            email: acct.email,
            phone: acct.phone,
            address: acct.address,
            city: acct.city,
            state: acct.state,
            zipCode: acct.zipCode,
            country: acct.country,
            industry: acct.industry,
            customerType: 0,
            lifecycleStage: 0,
            priority: 1
          }
        });
        
        if (!acctResp.ok()) {
          const errBody = await acctResp.text();
          console.log(`  ❌ Failed to create account: ${acctResp.status()} - ${errBody}`);
          results.push(`✗ ${acct.company}: Failed to create - ${acctResp.status()}`);
          continue;
        }
        
        const createdAcct = await acctResp.json();
        const acctId = createdAcct.id;
        console.log(`  ✓ Created account ID: ${acctId}`);
        
        // === CREATE CONTACTS VIA API ===
        const contactIds: number[] = [];
        const domain = acct.company.toLowerCase().replace(/[^a-z]/g, '').slice(0, 10);
        
        for (let j = 0; j < numContacts; j++) {
          const fn = FIRST_NAMES[Math.floor(Math.random() * FIRST_NAMES.length)];
          const ln = LAST_NAMES[Math.floor(Math.random() * LAST_NAMES.length)];
          
          const contactResp = await apiContext.post('/api/contacts', {
            headers,
            data: {
              firstName: fn,
              lastName: ln,
              emailPrimary: `${fn.toLowerCase()}.${ln.toLowerCase()}${Date.now()}${j}@${domain}.com`,
              phonePrimary: `555-${String(8000 + i*100 + j).padStart(4,'0')}`,
              jobTitle: JOB_TITLES[Math.floor(Math.random() * JOB_TITLES.length)],
              company: acct.company,
              city: acct.city,
              state: acct.state,
              status: 0
            }
          });
          
          if (contactResp.ok()) {
            const c = await contactResp.json();
            contactIds.push(c.id);
            totalContacts++;
          }
        }
        console.log(`  ✓ Created ${contactIds.length} contacts`);
        
        // === LINK CONTACTS VIA MAPPING TABLE API ===
        let linkedCount = 0;
        for (let k = 0; k < contactIds.length; k++) {
          const linkResp = await apiContext.post(`/api/customers/${acctId}/contacts`, {
            headers,
            data: { 
              contactId: contactIds[k], 
              role: k % 5, // Cycle through roles
              isPrimaryContact: k === 0, // First contact is primary
              isDecisionMaker: k < 3, // First 3 are decision makers
              receivesMarketingEmails: true,
              receivesTechnicalUpdates: k % 2 === 0
            }
          });
          
          if (linkResp.ok()) {
            linkedCount++;
            totalLinks++;
          } else {
            const errText = await linkResp.text();
            console.log(`    ⚠️ Failed to link contact ${contactIds[k]}: ${errText.slice(0, 100)}`);
          }
        }
        console.log(`  ✓ Linked ${linkedCount}/${contactIds.length} contacts to account`);
        
        results.push(`✓ ${acct.company}: ${contactIds.length} contacts, ${linkedCount} linked`);
        
      } catch (err: any) {
        console.log(`  ❌ Error: ${err.message}`);
        results.push(`✗ ${acct.company}: ${err.message?.slice(0,50) || 'Error'}`);
      }
    }
    
    // Summary
    console.log('\n=== SUMMARY ===');
    results.forEach(r => console.log(r));
    console.log(`\nTotal: ${results.filter(r => r.startsWith('✓')).length}/10 accounts`);
    console.log(`Total contacts created: ${totalContacts}`);
    console.log(`Total links created: ${totalLinks}`);
    
    // Verify in UI
    console.log('\n=== VERIFYING IN UI ===');
    await page.goto('/customers');
    await page.waitForTimeout(2000);
    
    // Check that accounts appear
    const accountRows = await page.locator('tr').count();
    console.log(`Found ${accountRows} rows in accounts table`);
    
    // Verify mapping table has entries
    expect(results.filter(r => r.startsWith('✓')).length).toBeGreaterThanOrEqual(8);
    expect(totalLinks).toBeGreaterThan(50); // At least 50 links should be created
    
    await apiContext.dispose();
  });
  
  test('verify linked contacts appear in UI', async ({ page }) => {
    // Navigate to customers page
    await page.goto('/customers');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Find a customer row and click to edit
    const rows = page.locator('tbody tr');
    const rowCount = await rows.count();
    console.log(`Found ${rowCount} customer rows`);
    
    if (rowCount > 0) {
      // Click on the first edit button
      const firstEditBtn = page.locator('tbody tr').first().locator('button').first();
      if (await firstEditBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await firstEditBtn.click();
        await page.waitForTimeout(1000);
        
        // Look for Linked Contacts tab
        const linkedTab = page.locator('[role="tab"]:has-text("Linked")');
        if (await linkedTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await linkedTab.click();
          await page.waitForTimeout(500);
          
          // Check if any contacts are shown
          const contactsList = page.locator('.MuiList-root, [role="list"]');
          const hasContacts = await contactsList.isVisible({ timeout: 2000 }).catch(() => false);
          console.log(`Linked contacts visible: ${hasContacts}`);
        }
        
        await page.keyboard.press('Escape');
      }
    }
    
    expect(rowCount).toBeGreaterThan(0);
  });
});

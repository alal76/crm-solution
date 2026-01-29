/**
 * UI Test: Create 10 Accounts with 1-10 Contacts Each
 * Creates accounts and links varying numbers of contacts to each
 */

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://localhost';
const API_URL = process.env.API_URL || (BASE_URL === 'http://localhost' ? 'http://localhost:5000' : BASE_URL);

// Account data with industry variety
const accounts = [
  { company: 'Alpha Technologies Inc', industry: 'Technology', contactCount: 1 },
  { company: 'Beta Manufacturing Ltd', industry: 'Manufacturing', contactCount: 2 },
  { company: 'Gamma Healthcare Systems', industry: 'Healthcare', contactCount: 3 },
  { company: 'Delta Financial Services', industry: 'Finance', contactCount: 4 },
  { company: 'Epsilon Retail Group', industry: 'Retail', contactCount: 5 },
  { company: 'Zeta Logistics Corp', industry: 'Logistics', contactCount: 6 },
  { company: 'Eta Energy Solutions', industry: 'Energy', contactCount: 7 },
  { company: 'Theta Education Partners', industry: 'Education', contactCount: 8 },
  { company: 'Iota Media Networks', industry: 'Media', contactCount: 9 },
  { company: 'Kappa Consulting Group', industry: 'Consulting', contactCount: 10 },
];

// Contact titles for variety
const titles = [
  'CEO', 'CTO', 'CFO', 'COO', 'VP Sales', 'VP Marketing', 
  'Director of Operations', 'IT Manager', 'Sales Manager', 'Account Executive',
  'Project Manager', 'Business Analyst', 'HR Director', 'Procurement Manager', 'Legal Counsel'
];

// Departments
const departments = [
  'Executive', 'Engineering', 'Finance', 'Operations', 'Sales', 
  'Marketing', 'IT', 'Human Resources', 'Legal', 'Procurement'
];

// Helper to get auth token
async function getAuthToken(request: any): Promise<string> {
  const loginResponse = await request.post(`${API_URL}/api/auth/login`, {
    data: {
      email: 'abhi.lal@gmail.com',
      password: 'Admin@123'
    }
  });
  
  if (!loginResponse.ok()) {
    throw new Error(`Login failed: ${loginResponse.status()}`);
  }
  
  const loginData = await loginResponse.json();
  return loginData.accessToken || loginData.token;
}

test.describe.serial('Create 10 Accounts with 1-10 Contacts Each', () => {
  
  for (let i = 0; i < accounts.length; i++) {
    const account = accounts[i];
    
    test(`Account ${i + 1}: ${account.company} with ${account.contactCount} contact(s)`, async ({ request }) => {
      // Get auth token for each test
      const authToken = await getAuthToken(request);
      
      // Step 1: Create the account
      console.log(`\n📦 Creating account: ${account.company}`);
      
      const customerResponse = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          category: 1, // Organization
          company: account.company,
          industry: account.industry,
          customerType: 0,
          status: 'Active',
          phone: `555-${String(100 + i).padStart(3, '0')}-${String(1000 + i * 100).padStart(4, '0')}`,
          email: `info@${account.company.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')}.com`,
          website: `https://www.${account.company.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')}.com`,
          address: `${100 + i * 10} Business Park Drive`,
          city: 'San Francisco',
          state: 'CA',
          zipCode: `9410${i}`,
          country: 'USA',
          notes: `Test account - ${account.contactCount} contacts linked`
        }
      });

      if (!customerResponse.ok()) {
        const errText = await customerResponse.text();
        console.log(`   ❌ Customer creation failed: ${customerResponse.status()} - ${errText}`);
      }
      expect(customerResponse.ok()).toBeTruthy();
      const customerData = await customerResponse.json();
      const customerId = customerData.id;
      console.log(`   ✅ Created customer ID: ${customerId}`);

      // Step 2: Create contacts and link them
      const contactIds: number[] = [];
      
      for (let j = 0; j < account.contactCount; j++) {
        const firstName = `Contact${j + 1}`;
        const lastName = account.company.split(' ')[0];
        const title = titles[j % titles.length];
        const department = departments[j % departments.length];
        
        console.log(`   👤 Creating contact ${j + 1}/${account.contactCount}: ${firstName} ${lastName}`);
        
        const contactResponse = await request.post(`${API_URL}/api/contacts`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            firstName: firstName,
            lastName: lastName,
            email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${j}@${account.company.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')}.com`,
            phone: `555-${String(200 + i).padStart(3, '0')}-${String(2000 + j * 10).padStart(4, '0')}`,
            mobile: `555-${String(300 + i).padStart(3, '0')}-${String(3000 + j * 10).padStart(4, '0')}`,
            title: title,
            department: department,
            status: 'Active',
            preferredContactMethod: 'Email',
            notes: `Contact ${j + 1} for ${account.company}`
          }
        });

        if (!contactResponse.ok()) {
          const errText = await contactResponse.text();
          console.log(`      ❌ Contact creation failed: ${contactResponse.status()} - ${errText}`);
        }
        expect(contactResponse.ok()).toBeTruthy();
        const contactData = await contactResponse.json();
        contactIds.push(contactData.id);
        console.log(`      ✅ Created contact ID: ${contactData.id}`);
      }

      // Step 3: Link contacts to customer
      for (let j = 0; j < contactIds.length; j++) {
        const contactId = contactIds[j];
        const isPrimary = j === 0; // First contact is primary
        const isDecisionMaker = j < 2; // First two are decision makers
        
        console.log(`   🔗 Linking contact ${contactId} to customer ${customerId}`);
        
        const linkResponse = await request.post(`${API_URL}/api/customers/${customerId}/contacts`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            contactId: contactId,
            isPrimaryContact: isPrimary,
            isDecisionMaker: isDecisionMaker,
            positionAtCustomer: titles[j % titles.length],
            notes: `Linked contact ${j + 1}`
          }
        });

        if (!linkResponse.ok()) {
          const errorText = await linkResponse.text();
          console.log(`      ⚠️ Link response: ${linkResponse.status()} - ${errorText}`);
        } else {
          console.log(`      ✅ Linked successfully`);
        }
        
        expect(linkResponse.ok()).toBeTruthy();
      }

      console.log(`\n✅ Completed: ${account.company} with ${account.contactCount} contacts linked`);
    });
  }

  test('Verify all accounts and contacts created', async ({ request }) => {
    const authToken = await getAuthToken(request);
    
    // Verify customers
    const customersResponse = await request.get(`${API_URL}/api/customers?pageSize=100`, {
      headers: { 'Authorization': `Bearer ${authToken}` }
    });
    expect(customersResponse.ok()).toBeTruthy();
    const customersData = await customersResponse.json();
    
    // Check for our created accounts
    const items = customersData.items || customersData.data || customersData;
    const ourAccounts = Array.isArray(items) ? items.filter((c: any) => 
      accounts.some(a => a.company === c.company)
    ) : [];
    
    console.log(`\n📊 Summary:`);
    console.log(`   Total customers in system: ${Array.isArray(items) ? items.length : 'N/A'}`);
    console.log(`   Our test accounts found: ${ourAccounts.length}`);

    // Verify contacts
    const contactsResponse = await request.get(`${API_URL}/api/contacts?pageSize=200`, {
      headers: { 'Authorization': `Bearer ${authToken}` }
    });
    expect(contactsResponse.ok()).toBeTruthy();
    const contactsData = await contactsResponse.json();
    const contactItems = contactsData.items || contactsData.data || contactsData;
    
    console.log(`   Total contacts in system: ${Array.isArray(contactItems) ? contactItems.length : 'N/A'}`);
    
    // Expected total: 1+2+3+4+5+6+7+8+9+10 = 55 contacts
    console.log(`   Expected new contacts: 55`);
    
    // List each account with its contact count
    for (const acc of ourAccounts) {
      const customerContactsResponse = await request.get(`${API_URL}/api/customers/${acc.id}/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      if (customerContactsResponse.ok()) {
        const ccData = await customerContactsResponse.json();
        const ccItems = ccData.items || ccData.data || ccData || [];
        console.log(`   📋 ${acc.company}: ${Array.isArray(ccItems) ? ccItems.length : 0} contacts linked`);
      }
    }
  });
});

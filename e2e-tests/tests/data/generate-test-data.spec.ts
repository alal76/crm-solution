/**
 * CRM Solution - Test Data Generator
 * Creates comprehensive test data via API for all features
 */

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const API_URL = `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;

// Store created IDs for relationships
const createdData = {
  customers: [] as number[],
  contacts: [] as number[],
  leads: [] as number[],
  opportunities: [] as number[],
  campaigns: [] as number[],
  products: [] as number[],
  quotes: [] as number[],
  serviceRequests: [] as number[],
  notes: [] as number[]
};

test.describe.serial('Test Data Generation', () => {
  
  test.beforeAll(async ({ request }) => {
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: { email: 'abhi.lal@gmail.com', password: 'Admin@123' }
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
  });

  test('1. Create Products (10 items)', async ({ request }) => {
    const products = [
      { name: 'CRM Enterprise License', sku: 'CRM-ENT-001', price: 9999.00, category: 'Software' },
      { name: 'CRM Standard License', sku: 'CRM-STD-001', price: 4999.00, category: 'Software' },
      { name: 'CRM Basic License', sku: 'CRM-BAS-001', price: 999.00, category: 'Software' },
      { name: 'Annual Support Package', sku: 'SUP-ANN-001', price: 1999.00, category: 'Services' },
      { name: 'Premium Support Package', sku: 'SUP-PRM-001', price: 4999.00, category: 'Services' },
      { name: 'Implementation Services', sku: 'SVC-IMP-001', price: 14999.00, category: 'Services' },
      { name: 'Training Package (5 days)', sku: 'TRN-5D-001', price: 2499.00, category: 'Training' },
      { name: 'Custom Development (per hour)', sku: 'DEV-HR-001', price: 150.00, category: 'Development' },
      { name: 'API Integration Module', sku: 'MOD-API-001', price: 2999.00, category: 'Add-ons' },
      { name: 'Analytics Dashboard Pro', sku: 'MOD-ANA-001', price: 1499.00, category: 'Add-ons' }
    ];

    for (const product of products) {
      const response = await request.post(`${API_URL}/api/products`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: { ...product, description: `${product.name} - Premium CRM solution`, isActive: true }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.products.push(created.id);
      }
    }
    expect(createdData.products.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.products.length} products`);
  });

  test('2. Create Customers (15 companies)', async ({ request }) => {
    const companies = [
      { company: 'Acme Corporation', industry: 'Manufacturing', type: 0 },
      { company: 'GlobalTech Solutions', industry: 'Technology', type: 0 },
      { company: 'First National Bank', industry: 'Financial Services', type: 0 },
      { company: 'HealthCare Plus', industry: 'Healthcare', type: 0 },
      { company: 'EduLearn Systems', industry: 'Education', type: 0 },
      { company: 'RetailMax Stores', industry: 'Retail', type: 0 },
      { company: 'LogiTrans Shipping', industry: 'Logistics', type: 0 },
      { company: 'GreenEnergy Co', industry: 'Energy', type: 0 },
      { company: 'MediaStream Inc', industry: 'Media', type: 0 },
      { company: 'FoodService Pro', industry: 'Food & Beverage', type: 0 },
      { company: 'AutoDrive Motors', industry: 'Automotive', type: 0 },
      { company: 'SkyHigh Airlines', industry: 'Aviation', type: 0 },
      { company: 'TeleCom Networks', industry: 'Telecommunications', type: 0 },
      { company: 'PropertyFirst Realty', industry: 'Real Estate', type: 0 },
      { company: 'PharmaCure Labs', industry: 'Pharmaceuticals', type: 0 }
    ];

    for (let i = 0; i < companies.length; i++) {
      const company = companies[i];
      const response = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Account',
          lastName: `Manager ${i + 1}`,
          company: company.company,
          industry: company.industry,
          email: `account${i + 1}@${company.company.toLowerCase().replace(/\s/g, '')}.com`,
          phone: `555-${String(1000 + i).padStart(4, '0')}`,
          type: company.type,
          lifecycleStage: Math.floor(Math.random() * 5)
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.customers.push(created.id);
      }
    }
    expect(createdData.customers.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.customers.length} customers`);
  });

  test('3. Create Contacts (30 contacts)', async ({ request }) => {
    const firstNames = ['John', 'Jane', 'Michael', 'Sarah', 'David', 'Emily', 'Robert', 'Lisa', 'William', 'Jennifer'];
    const lastNames = ['Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez', 'Martinez'];
    const titles = ['CEO', 'CTO', 'CFO', 'VP Sales', 'VP Marketing', 'Director IT', 'Manager', 'Analyst', 'Consultant', 'Engineer'];

    for (let i = 0; i < 30; i++) {
      const firstName = firstNames[i % firstNames.length];
      const lastName = lastNames[Math.floor(i / firstNames.length) % lastNames.length];
      const title = titles[i % titles.length];
      
      const response = await request.post(`${API_URL}/api/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: firstName,
          lastName: `${lastName} ${i + 1}`,
          email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${i + 1}@example.com`,
          phone: `555-${String(2000 + i).padStart(4, '0')}`,
          title: title,
          department: ['Sales', 'Engineering', 'Marketing', 'Finance', 'Operations'][i % 5],
          contactType: i % 5
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.contacts.push(created.id);
      }
    }
    expect(createdData.contacts.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.contacts.length} contacts`);
  });

  test('4. Create Leads (20 leads)', async ({ request }) => {
    const sources = ['Website', 'Referral', 'Trade Show', 'Cold Call', 'Social Media', 'Email Campaign', 'Partner', 'Webinar'];
    
    for (let i = 0; i < 20; i++) {
      const response = await request.post(`${API_URL}/api/leads`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Lead',
          lastName: `Prospect ${i + 1}`,
          email: `lead${i + 1}@prospect.com`,
          company: `Prospect Company ${i + 1}`,
          phone: `555-${String(3000 + i).padStart(4, '0')}`,
          source: sources[i % sources.length],
          status: i % 4,
          estimatedValue: Math.floor(Math.random() * 50000) + 5000,
          notes: `Lead generated from ${sources[i % sources.length]}`
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.leads.push(created.id);
      }
    }
    expect(createdData.leads.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.leads.length} leads`);
  });

  test('5. Create Campaigns (5 campaigns)', async ({ request }) => {
    const campaigns = [
      { name: 'Q1 Product Launch', type: 1, budget: 50000 },
      { name: 'Spring Email Campaign', type: 2, budget: 15000 },
      { name: 'Trade Show 2026', type: 3, budget: 75000 },
      { name: 'Social Media Blitz', type: 4, budget: 25000 },
      { name: 'Customer Appreciation Month', type: 5, budget: 30000 }
    ];

    for (let i = 0; i < campaigns.length; i++) {
      const campaign = campaigns[i];
      const startDate = new Date();
      startDate.setDate(startDate.getDate() + (i * 30));
      const endDate = new Date(startDate);
      endDate.setDate(endDate.getDate() + 30);

      const response = await request.post(`${API_URL}/api/campaigns`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          name: campaign.name,
          description: `${campaign.name} - Marketing campaign`,
          type: campaign.type,
          status: i % 3,
          startDate: startDate.toISOString(),
          endDate: endDate.toISOString(),
          budget: campaign.budget,
          targetAudience: 'Enterprise customers'
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.campaigns.push(created.id);
      }
    }
    expect(createdData.campaigns.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.campaigns.length} campaigns`);
  });

  test('6. Create Opportunities (15 opportunities)', async ({ request }) => {
    const stages = ['Prospecting', 'Qualification', 'Needs Analysis', 'Proposal', 'Negotiation', 'Closed Won'];
    
    for (let i = 0; i < 15; i++) {
      const customerId = createdData.customers[i % createdData.customers.length];
      const closeDate = new Date();
      closeDate.setDate(closeDate.getDate() + Math.floor(Math.random() * 90) + 30);

      const response = await request.post(`${API_URL}/api/opportunities`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: `Opportunity ${i + 1} - Enterprise Deal`,
          description: `Enterprise CRM implementation opportunity`,
          customerId: customerId,
          value: Math.floor(Math.random() * 100000) + 10000,
          probability: Math.floor(Math.random() * 60) + 20,
          stage: stages[i % stages.length],
          expectedCloseDate: closeDate.toISOString(),
          source: ['Inbound', 'Outbound', 'Partner', 'Referral'][i % 4]
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.opportunities.push(created.id);
      }
    }
    expect(createdData.opportunities.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.opportunities.length} opportunities`);
  });

  test('7. Create Service Requests (20 tickets)', async ({ request }) => {
    const types = ['Bug Report', 'Feature Request', 'General Inquiry', 'Billing Question', 'Technical Support'];
    const priorities = [0, 1, 2, 3]; // Low, Medium, High, Critical
    
    for (let i = 0; i < 20; i++) {
      const customerId = createdData.customers[i % createdData.customers.length];

      const response = await request.post(`${API_URL}/api/servicerequests`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: `Ticket ${i + 1}: ${types[i % types.length]}`,
          description: `Customer reported issue requiring attention - ${types[i % types.length]}`,
          customerId: customerId,
          priority: priorities[i % priorities.length],
          status: i % 5,
          category: types[i % types.length]
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.serviceRequests.push(created.id);
      }
    }
    expect(createdData.serviceRequests.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.serviceRequests.length} service requests`);
  });

  test('8. Create Quotes (15 quotes)', async ({ request }) => {
    for (let i = 0; i < 15; i++) {
      const customerId = createdData.customers[i % createdData.customers.length];
      const opportunityId = createdData.opportunities.length > 0 
        ? createdData.opportunities[i % createdData.opportunities.length] 
        : undefined;
      
      const subtotal = Math.floor(Math.random() * 50000) + 5000;
      const discount = subtotal * 0.1;
      const tax = (subtotal - discount) * 0.08;

      const response = await request.post(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          customerId: customerId,
          opportunityId: opportunityId,
          title: `Quote ${i + 1} - Enterprise Package`,
          description: `Custom CRM solution quote`,
          status: i % 5,
          validityDays: 30,
          subtotal: subtotal,
          discountTotal: discount,
          taxTotal: tax,
          total: subtotal - discount + tax,
          notes: `Quote generated for enterprise customer`
        }
      });
      if (response.ok()) {
        const created = await response.json();
        createdData.quotes.push(created.id);
      }
    }
    expect(createdData.quotes.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.quotes.length} quotes`);
  });

  test('9. Create Notes for Entities (30 notes)', async ({ request }) => {
    const noteTypes = [0, 1, 2, 3, 4, 5, 6, 7]; // All note types
    const entityTypes = ['Customer', 'Contact', 'Lead', 'Opportunity', 'Quote'];

    for (let i = 0; i < 30; i++) {
      const entityType = entityTypes[i % entityTypes.length];
      let entityId: number | undefined;

      switch (entityType) {
        case 'Customer':
          entityId = createdData.customers[i % createdData.customers.length];
          break;
        case 'Contact':
          entityId = createdData.contacts[i % createdData.contacts.length];
          break;
        case 'Lead':
          entityId = createdData.leads[i % createdData.leads.length];
          break;
        case 'Opportunity':
          entityId = createdData.opportunities[i % createdData.opportunities.length];
          break;
        case 'Quote':
          entityId = createdData.quotes[i % createdData.quotes.length];
          break;
      }

      if (entityId) {
        const response = await request.post(`${API_URL}/api/notes`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            title: `Note ${i + 1} for ${entityType}`,
            content: `This is a test note attached to ${entityType} ${entityId}. Contains important information about the ${entityType.toLowerCase()}.`,
            entityType: entityType,
            entityId: entityId,
            noteType: noteTypes[i % noteTypes.length],
            visibility: i % 3,
            isPinned: i % 5 === 0,
            isImportant: i % 3 === 0
          }
        });
        if (response.ok()) {
          const created = await response.json();
          createdData.notes.push(created.id);
        }
      }
    }
    expect(createdData.notes.length).toBeGreaterThan(0);
    console.log(`Created ${createdData.notes.length} notes`);
  });

  test('10. Summary Report', async () => {
    console.log('\n=== TEST DATA GENERATION SUMMARY ===');
    console.log(`Products:        ${createdData.products.length}`);
    console.log(`Customers:       ${createdData.customers.length}`);
    console.log(`Contacts:        ${createdData.contacts.length}`);
    console.log(`Leads:           ${createdData.leads.length}`);
    console.log(`Campaigns:       ${createdData.campaigns.length}`);
    console.log(`Opportunities:   ${createdData.opportunities.length}`);
    console.log(`Service Requests: ${createdData.serviceRequests.length}`);
    console.log(`Quotes:          ${createdData.quotes.length}`);
    console.log(`Notes:           ${createdData.notes.length}`);
    console.log('=====================================\n');
    
    const totalCreated = 
      createdData.products.length +
      createdData.customers.length +
      createdData.contacts.length +
      createdData.leads.length +
      createdData.campaigns.length +
      createdData.opportunities.length +
      createdData.serviceRequests.length +
      createdData.quotes.length +
      createdData.notes.length;
    
    expect(totalCreated).toBeGreaterThan(100);
  });
});

/**
 * CRM Solution - Build Verification Tests (BVT)
 * Critical path tests for all major features
 */

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const API_URL = `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;

test.describe('BVT - Build Verification Tests', () => {
  
  test.beforeAll(async ({ request }) => {
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: { email: 'abhi.lal@gmail.com', password: 'Admin@123' }
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
  });

  test.describe('BVT-01: Authentication Critical Path', () => {
    test('BVT-01-001: Health check endpoint responds', async ({ request }) => {
      const response = await request.get(`${API_URL}/health`);
      expect(response.ok()).toBeTruthy();
      const data = await response.json();
      expect(data.status).toBe('healthy');
    });

    test('BVT-01-002: Login with valid credentials succeeds', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/auth/login`, {
        data: { email: 'abhi.lal@gmail.com', password: 'Admin@123' }
      });
      expect(response.ok()).toBeTruthy();
      const data = await response.json();
      expect(data.accessToken).toBeTruthy();
    });

    test('BVT-01-003: Login with invalid credentials fails', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/auth/login`, {
        data: { email: 'invalid@example.com', password: 'wrongpassword' }
      });
      expect(response.ok()).toBeFalsy();
    });

    test('BVT-01-004: Protected endpoint requires auth', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/customers`);
      expect(response.status()).toBe(401);
    });
  });

  test.describe('BVT-02: Customer CRUD Critical Path', () => {
    let customerId: number;

    test('BVT-02-001: Create customer', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'BVT',
          lastName: 'Customer',
          company: 'BVT Test Corp',
          email: `bvt-${Date.now()}@example.com`,
          phone: '555-BVT1'
        }
      });
      expect(response.ok()).toBeTruthy();
      const customer = await response.json();
      customerId = customer.id;
      expect(customerId).toBeGreaterThan(0);
    });

    test('BVT-02-002: Read customer', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/customers/${customerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const customer = await response.json();
      expect(customer.id).toBe(customerId);
    });

    test('BVT-02-003: Update customer', async ({ request }) => {
      const response = await request.put(`${API_URL}/api/customers/${customerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: customerId,
          firstName: 'BVT Updated',
          lastName: 'Customer',
          company: 'BVT Updated Corp',
          email: `bvt-updated-${Date.now()}@example.com`,
          phone: '555-BVT2'
        }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-02-004: List customers', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const customers = await response.json();
      expect(Array.isArray(customers)).toBeTruthy();
    });

    test('BVT-02-005: Delete customer', async ({ request }) => {
      const response = await request.delete(`${API_URL}/api/customers/${customerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-03: Contact CRUD Critical Path', () => {
    let contactId: number;

    test('BVT-03-001: Create contact', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'BVT',
          lastName: 'Contact',
          email: `bvt-contact-${Date.now()}@example.com`,
          phone: '555-CNT1'
        }
      });
      expect(response.ok()).toBeTruthy();
      const contact = await response.json();
      contactId = contact.id;
      expect(contactId).toBeGreaterThan(0);
    });

    test('BVT-03-002: Read contact', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/contacts/${contactId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-03-003: List contacts', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/contacts`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const contacts = await response.json();
      expect(Array.isArray(contacts)).toBeTruthy();
    });

    test('BVT-03-004: Delete contact', async ({ request }) => {
      const response = await request.delete(`${API_URL}/api/contacts/${contactId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-04: Lead CRUD Critical Path', () => {
    let leadId: number;

    test('BVT-04-001: Create lead', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/leads`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'BVT',
          lastName: 'Lead',
          email: `bvt-lead-${Date.now()}@example.com`,
          company: 'BVT Lead Corp',
          source: 'Website',
          status: 0
        }
      });
      expect(response.ok()).toBeTruthy();
      const lead = await response.json();
      leadId = lead.id;
      expect(leadId).toBeGreaterThan(0);
    });

    test('BVT-04-002: Read lead', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/leads/${leadId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-04-003: List leads', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/leads`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-04-004: Delete lead', async ({ request }) => {
      const response = await request.delete(`${API_URL}/api/leads/${leadId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-05: Opportunity CRUD Critical Path', () => {
    let opportunityId: number;
    let testCustomerId: number;

    test.beforeAll(async ({ request }) => {
      const response = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Opp',
          lastName: 'Customer',
          company: 'Opp Corp',
          email: `opp-cust-${Date.now()}@example.com`
        }
      });
      if (response.ok()) {
        const customer = await response.json();
        testCustomerId = customer.id;
      }
    });

    test('BVT-05-001: Create opportunity', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/opportunities`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: 'BVT Opportunity',
          description: 'Test opportunity',
          customerId: testCustomerId,
          value: 10000,
          probability: 50,
          expectedCloseDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString()
        }
      });
      expect(response.ok()).toBeTruthy();
      const opportunity = await response.json();
      opportunityId = opportunity.id;
      expect(opportunityId).toBeGreaterThan(0);
    });

    test('BVT-05-002: Read opportunity', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/opportunities/${opportunityId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-05-003: List opportunities', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/opportunities`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-06: Service Request CRUD Critical Path', () => {
    let serviceRequestId: number;
    let testCustomerId: number;

    test.beforeAll(async ({ request }) => {
      const response = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'SR',
          lastName: 'Customer',
          company: 'SR Corp',
          email: `sr-cust-${Date.now()}@example.com`
        }
      });
      if (response.ok()) {
        const customer = await response.json();
        testCustomerId = customer.id;
      }
    });

    test('BVT-06-001: Create service request', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/servicerequests`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: 'BVT Service Request',
          description: 'Test service request',
          customerId: testCustomerId,
          priority: 2,
          status: 0
        }
      });
      expect(response.ok()).toBeTruthy();
      const sr = await response.json();
      serviceRequestId = sr.id;
      expect(serviceRequestId).toBeGreaterThan(0);
    });

    test('BVT-06-002: Read service request', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/servicerequests/${serviceRequestId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-06-003: List service requests', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/servicerequests`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-07: Campaign CRUD Critical Path', () => {
    let campaignId: number;

    test('BVT-07-001: Create campaign', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/campaigns`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          name: 'BVT Campaign',
          description: 'Test campaign',
          type: 1,
          status: 0,
          startDate: new Date().toISOString(),
          endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
          budget: 5000
        }
      });
      expect(response.ok()).toBeTruthy();
      const campaign = await response.json();
      campaignId = campaign.id;
      expect(campaignId).toBeGreaterThan(0);
    });

    test('BVT-07-002: Read campaign', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/campaigns/${campaignId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-07-003: List campaigns', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/campaigns`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-08: Product CRUD Critical Path', () => {
    let productId: number;

    test('BVT-08-001: Create product', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/products`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          name: 'BVT Product',
          sku: `BVT-SKU-${Date.now()}`,
          description: 'Test product',
          price: 99.99,
          category: 'Software',
          isActive: true
        }
      });
      expect(response.ok()).toBeTruthy();
      const product = await response.json();
      productId = product.id;
      expect(productId).toBeGreaterThan(0);
    });

    test('BVT-08-002: Read product', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/products/${productId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-08-003: List products', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/products`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-09: Quote CRUD Critical Path', () => {
    let quoteId: number;
    let testCustomerId: number;

    test.beforeAll(async ({ request }) => {
      const response = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Quote',
          lastName: 'Customer',
          company: 'Quote Corp',
          email: `quote-cust-${Date.now()}@example.com`
        }
      });
      if (response.ok()) {
        const customer = await response.json();
        testCustomerId = customer.id;
      }
    });

    test('BVT-09-001: Create quote', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          customerId: testCustomerId,
          title: 'BVT Quote',
          status: 1,
          validityDays: 30,
          subtotal: 1000,
          total: 1000
        }
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      quoteId = quote.id;
      expect(quoteId).toBeGreaterThan(0);
    });

    test('BVT-09-002: Read quote', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/quotes/${quoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-09-003: List quotes', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-10: User & Groups Critical Path', () => {
    test('BVT-10-001: List users', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/users`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-10-002: List user groups', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/usergroups`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-10-003: Get current user profile', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/userprofiles/me`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-11: Dashboard Critical Path', () => {
    test('BVT-11-001: Get dashboard data', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/dashboard`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-11-002: Get dashboard configuration', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/dashboardconfig`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      // May be 404 if no config exists, which is acceptable
      expect(response.status()).toBeLessThan(500);
    });
  });

  test.describe('BVT-12: Notes Critical Path', () => {
    test('BVT-12-001: List notes', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/notes`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-12-002: Create note', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/notes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: 'BVT Note',
          content: 'BVT test note content',
          noteType: 0,
          visibility: 1
        }
      });
      expect(response.ok()).toBeTruthy();
    });
  });

  test.describe('BVT-13: System Settings Critical Path', () => {
    test('BVT-13-001: Get system settings', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/systemsettings`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('BVT-13-002: Get lookups', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/lookups/categories`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
    });
  });
});

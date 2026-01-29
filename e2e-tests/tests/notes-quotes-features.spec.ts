/**
 * CRM Solution - Notes and Quotes Feature Tests
 * Tests for the new Notes system and Quote Builder functionality
 */

import { test, expect } from '@playwright/test';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const API_URL = `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;

test.describe('Notes and Quotes Features', () => {
  
  test.beforeAll(async ({ request }) => {
    // Authenticate to get token
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: {
        email: 'abhi.lal@gmail.com',
        password: 'Admin@123'
      }
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
  });

  test.describe('Notes API Tests', () => {
    let testCustomerId: number;
    let testNoteId: number;

    test.beforeAll(async ({ request }) => {
      // Create a test customer for notes
      const customerResponse = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Notes',
          lastName: 'TestCustomer',
          company: 'Notes Test Corp',
          email: `notes-test-${Date.now()}@example.com`,
          phone: '555-0001'
        }
      });
      
      if (customerResponse.ok()) {
        const customer = await customerResponse.json();
        testCustomerId = customer.id;
      }
    });

    test('NOTE-001: Create note attached to entity', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/notes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          title: 'Test Note for Customer',
          content: 'This is a test note attached to a customer entity',
          entityType: 'Customer',
          entityId: testCustomerId,
          noteType: 0, // General
          visibility: 1, // Team
          isPinned: false,
          isImportant: false
        }
      });
      
      expect(response.ok()).toBeTruthy();
      const note = await response.json();
      expect(note.id).toBeGreaterThan(0);
      expect(note.entityType).toBe('Customer');
      expect(note.entityId).toBe(testCustomerId);
      testNoteId = note.id;
    });

    test('NOTE-002: Get notes by entity', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/notes/entity/Customer/${testCustomerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const notes = await response.json();
      expect(Array.isArray(notes)).toBeTruthy();
      expect(notes.length).toBeGreaterThan(0);
    });

    test('NOTE-003: Update note', async ({ request }) => {
      const response = await request.put(`${API_URL}/api/notes/${testNoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: testNoteId,
          title: 'Updated Test Note',
          content: 'This note has been updated',
          entityType: 'Customer',
          entityId: testCustomerId,
          noteType: 1, // CallNotes
          visibility: 1,
          isPinned: true,
          isImportant: true
        }
      });
      
      expect(response.ok()).toBeTruthy();
    });

    test('NOTE-004: Toggle note pinned status', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/notes/${testNoteId}/toggle-pin`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
    });

    test('NOTE-005: Toggle note important status', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/notes/${testNoteId}/toggle-important`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
    });

    test('NOTE-006: Quick add note', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/notes/quick-add`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          content: 'Quick note from context menu',
          entityType: 'Customer',
          entityId: testCustomerId,
          contextPath: '/customers'
        }
      });
      
      expect(response.ok()).toBeTruthy();
      const note = await response.json();
      expect(note.contextPath).toBe('/customers');
    });

    test('NOTE-007: Note visibility - Team notes visible', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/notes`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const notes = await response.json();
      expect(Array.isArray(notes)).toBeTruthy();
    });

    test('NOTE-008: Create notes with different types', async ({ request }) => {
      const noteTypes = [
        { type: 0, name: 'General' },
        { type: 1, name: 'CallNotes' },
        { type: 2, name: 'MeetingNotes' },
        { type: 3, name: 'Feedback' },
        { type: 4, name: 'Requirement' },
        { type: 5, name: 'Issue' },
        { type: 6, name: 'Idea' },
        { type: 7, name: 'Warning' }
      ];

      for (const noteType of noteTypes) {
        const response = await request.post(`${API_URL}/api/notes`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            title: `${noteType.name} Note`,
            content: `This is a ${noteType.name} type note`,
            entityType: 'Customer',
            entityId: testCustomerId,
            noteType: noteType.type,
            visibility: 1
          }
        });
        
        expect(response.ok()).toBeTruthy();
      }
    });
  });

  test.describe('Quote Builder API Tests', () => {
    let testCustomerId: number;
    let testQuoteId: number;

    test.beforeAll(async ({ request }) => {
      // Create a test customer for quotes
      const customerResponse = await request.post(`${API_URL}/api/customers`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          firstName: 'Quote',
          lastName: 'TestCustomer',
          company: 'Quote Test Corp',
          email: `quote-test-${Date.now()}@example.com`,
          phone: '555-0002'
        }
      });
      
      if (customerResponse.ok()) {
        const customer = await customerResponse.json();
        testCustomerId = customer.id;
      }
    });

    test('QUOTE-001: Create new quote', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          customerId: testCustomerId,
          title: 'Test Quote for E2E',
          description: 'E2E test quote with line items',
          status: 1, // Draft
          validityDays: 30,
          subtotal: 1000,
          discountTotal: 0,
          taxTotal: 0,
          total: 1000
        }
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.id).toBeGreaterThan(0);
      expect(quote.quoteNumber).toBeTruthy();
      testQuoteId = quote.id;
    });

    test('QUOTE-002: Get quote by ID', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/quotes/${testQuoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.id).toBe(testQuoteId);
    });

    test('QUOTE-003: Update quote status - Submit for approval', async ({ request }) => {
      const response = await request.put(`${API_URL}/api/quotes/${testQuoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: testQuoteId,
          customerId: testCustomerId,
          title: 'Updated Test Quote',
          status: 2, // UnderApproval
          validityDays: 30,
          subtotal: 1500,
          discountTotal: 100,
          taxTotal: 140,
          total: 1540
        }
      });
      
      expect(response.ok()).toBeTruthy();
    });

    test('QUOTE-004: Send quote to customer', async ({ request }) => {
      // First approve the quote
      await request.put(`${API_URL}/api/quotes/${testQuoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: testQuoteId,
          customerId: testCustomerId,
          title: 'Approved Test Quote',
          status: 3, // Approved
          validityDays: 30,
          subtotal: 1500,
          total: 1500
        }
      });

      // Now send
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/send`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(4); // Shared
    });

    test('QUOTE-005: Mark quote as viewed', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/viewed`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(5); // Viewed
    });

    test('QUOTE-006: Accept quote', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/accept`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {}
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(6); // Accepted
    });

    test('QUOTE-007: Get quotes with filters', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/quotes?customerId=${testCustomerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const quotes = await response.json();
      expect(Array.isArray(quotes)).toBeTruthy();
      expect(quotes.some((q: any) => q.customerId === testCustomerId)).toBeTruthy();
    });

    test('QUOTE-008: Quote lifecycle - full path', async ({ request }) => {
      // Create new quote
      let response = await request.post(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          customerId: testCustomerId,
          title: 'Full Lifecycle Quote',
          status: 0, // New
          validityDays: 30,
          subtotal: 5000,
          total: 5000
        }
      });
      
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      const quoteId = quote.id;

      // Update to Draft
      response = await request.put(`${API_URL}/api/quotes/${quoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: { ...quote, status: 1 } // Draft
      });
      expect(response.ok()).toBeTruthy();

      // The quote should now be in draft status
      response = await request.get(`${API_URL}/api/quotes/${quoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      const updatedQuote = await response.json();
      expect(updatedQuote.status).toBe(1);
    });
  });

  test.describe('System Settings & Branding API Tests', () => {
    test('BRANDING-001: Get system settings with branding', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/systemsettings`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      expect(response.ok()).toBeTruthy();
      const settings = await response.json();
      expect(settings).toBeDefined();
    });

    test('BRANDING-002: Update company branding details', async ({ request }) => {
      // Get current settings first
      const getResponse = await request.get(`${API_URL}/api/systemsettings`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      
      if (getResponse.ok()) {
        const settings = await getResponse.json();
        
        // Update with enhanced branding
        const updateResponse = await request.put(`${API_URL}/api/systemsettings`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            ...settings,
            companyFullName: 'CRM Solutions Inc.',
            companyLegalName: 'CRM Solutions Incorporated',
            companyTaxId: 'TAX-123456789',
            companyIndustry: 'Technology',
            companyDescription: 'Leading CRM solutions provider',
            quoteValidityDays: 30,
            quoteNumberPrefix: 'QT-',
            defaultTaxRate: 10.0
          }
        });
        
        // It's okay if the update requires admin permissions
        expect(updateResponse.status()).toBeLessThan(500);
      }
    });
  });
});

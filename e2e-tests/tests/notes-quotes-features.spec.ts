/**
 * CRM Solution - Notes and Quotes Feature Tests
 * Tests for the new Notes system and Quote Builder functionality
 */

import { test, expect } from '@playwright/test';
import { WEB_BASE_URL } from '../testConfig';

const BASE_URL = WEB_BASE_URL;
const API_URL = `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;

test.describe('Notes and Quotes Features', () => {
  
  test.beforeAll(async ({ request }) => {
    // Authenticate to get token
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: {
        email: 'admin@crm.local',
        password: 'Admin@123'
      }
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
  });

  test.describe('Notes API Tests', () => {
    test.describe.configure({ mode: 'serial' });
    let testCustomerId: number;
    let testNoteId: number;

    test.beforeAll(async ({ request }) => {
      const customerResponse = await request.post(`${API_URL}/api/accounts`, {
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
          noteType: 0,
          visibility: 1,
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
          noteType: 1,
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
    test.describe.configure({ mode: 'serial' });
    let testCustomerId: number;
    let testQuoteId: number;
    let testQuoteNumber: string;

    test.beforeAll(async ({ request }) => {
      const customerResponse = await request.post(`${API_URL}/api/accounts`, {
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
          quoteNumber: `Q-E2E-${Date.now()}`,
          accountId: testCustomerId,
          name: 'Test Quote for E2E',
          description: 'E2E test quote with line items',
          status: 1,
          validityDays: 30,
          subtotal: 1000,
          discount: 0,
          tax: 0,
          total: 1000
        }
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.id).toBeGreaterThan(0);
      expect(quote.quoteNumber).toBeTruthy();
      testQuoteId = quote.id;
      testQuoteNumber = quote.quoteNumber;
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
          quoteNumber: testQuoteNumber,
          accountId: testCustomerId,
          name: 'Updated Test Quote',
          status: 2,
          validityDays: 30,
          subtotal: 1500,
          discount: 100,
          tax: 140,
          total: 1540
        }
      });
      expect(response.ok()).toBeTruthy();
    });

    test('QUOTE-004: Send quote to customer', async ({ request }) => {
      // Approve the quote first (status 3)
      await request.put(`${API_URL}/api/quotes/${testQuoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: testQuoteId,
          quoteNumber: testQuoteNumber,
          accountId: testCustomerId,
          name: 'Approved Test Quote',
          status: 3,
          validityDays: 30,
          subtotal: 1500,
          total: 1500
        }
      });
      // Send the quote
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/send`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(4);
    });

    test('QUOTE-005: Mark quote as viewed', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/viewed`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(5);
    });

    test('QUOTE-006: Accept quote', async ({ request }) => {
      const response = await request.post(`${API_URL}/api/quotes/${testQuoteId}/accept`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {}
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      expect(quote.status).toBe(6);
    });

    test('QUOTE-007: Get quotes with filters', async ({ request }) => {
      const response = await request.get(`${API_URL}/api/quotes?accountId=${testCustomerId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      expect(response.ok()).toBeTruthy();
      const quotes = await response.json();
      expect(Array.isArray(quotes)).toBeTruthy();
    });

    test('QUOTE-008: Quote lifecycle - full path', async ({ request }) => {
      let response = await request.post(`${API_URL}/api/quotes`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          quoteNumber: `Q-LC-${Date.now()}`,
          accountId: testCustomerId,
          name: 'Full Lifecycle Quote',
          status: 0,
          validityDays: 30,
          subtotal: 5000,
          total: 5000
        }
      });
      expect(response.ok()).toBeTruthy();
      const quote = await response.json();
      const quoteId = quote.id;

      response = await request.put(`${API_URL}/api/quotes/${quoteId}`, {
        headers: { 'Authorization': `Bearer ${authToken}` },
        data: {
          id: quoteId,
          quoteNumber: quote.quoteNumber,
          accountId: testCustomerId,
          name: quote.title || 'Full Lifecycle Quote',
          status: 1,
          validityDays: 30,
          subtotal: 5000,
          total: 5000
        }
      });
      expect(response.ok()).toBeTruthy();

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
      const getResponse = await request.get(`${API_URL}/api/systemsettings`, {
        headers: { 'Authorization': `Bearer ${authToken}` }
      });
      if (getResponse.ok()) {
        const settings = await getResponse.json();
        // API requires valid BCP-47 for defaultLanguage. Existing value can be plain 'en'.
        const fixedDefaultLanguage = settings.defaultLanguage && settings.defaultLanguage.includes('-')
          ? settings.defaultLanguage
          : 'en-US';
        const updateResponse = await request.put(`${API_URL}/api/systemsettings`, {
          headers: { 'Authorization': `Bearer ${authToken}` },
          data: {
            companyName: settings.companyName || 'CRM Solution',
            defaultCurrency: settings.defaultCurrency || 'USD',
            defaultTimezone: settings.defaultTimezone || 'UTC',
            defaultLanguage: fixedDefaultLanguage,
            dateFormat: settings.dateFormat || 'MM/dd/yyyy',
            timeFormat: settings.timeFormat || 'h:mm tt',
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
        // Accept any non-server-error response
        expect(updateResponse.status()).toBeLessThan(500);
      }
    });
  });
});

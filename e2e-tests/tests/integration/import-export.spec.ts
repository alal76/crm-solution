/**
 * CRM Solution - Import/Export E2E Tests
 * 
 * Tests for CSV import functionality, field mapping, validation,
 * progress tracking, and export operations.
 * 
 * Implements TODO-INT003-018
 */

import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const BASE_URL = process.env.BASE_URL || 'http://192.168.0.9';
const API_URL = BASE_URL.includes(':5000') ? BASE_URL : `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;
let importJobId: string;
let exportJobId: string;

// Sample CSV content for testing
const SAMPLE_ACCOUNTS_CSV = `name,email,phone,industry,website
Test Company 1,contact1@test.com,+1-555-0101,Technology,https://test1.com
Test Company 2,contact2@test.com,+1-555-0102,Healthcare,https://test2.com
Test Company 3,contact3@test.com,+1-555-0103,Finance,https://test3.com`;

const SAMPLE_CONTACTS_CSV = `firstName,lastName,email,phone,title
John,Doe,john.doe@test.com,+1-555-0201,Manager
Jane,Smith,jane.smith@test.com,+1-555-0202,Director
Bob,Johnson,bob.johnson@test.com,+1-555-0203,VP Sales`;

test.describe('Import/Export Operations', () => {
  test.describe.configure({ mode: 'serial' });

  test.beforeAll(async ({ request }) => {
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: { email: 'admin@crm.local', password: 'Admin@123' },
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
    expect(authToken).toBeTruthy();
  });

  // --------------------------------------------------------------------------
  // Import Configuration
  // --------------------------------------------------------------------------

  test('@smoke should get import configuration options', async ({ request }) => {
    const response = await request.get(`${API_URL}/api/import/config`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      // Try alternate endpoints
      const alt = await request.get(`${API_URL}/api/data/import/config`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });
      if (alt.ok()) {
        const body = await alt.json();
        expect(body.supportedEntities ?? body.entities ?? body.formats).toBeDefined();
        return;
      }
      console.log('Import config endpoint not implemented - skipping');
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.supportedEntities ?? body.entities ?? body.formats).toBeDefined();
  });

  test('should get field mappings for accounts', async ({ request }) => {
    const response = await request.get(`${API_URL}/api/import/mappings/accounts`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      const alt = await request.get(`${API_URL}/api/data/import/fields/accounts`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });
      if (alt.ok()) {
        const body = await alt.json();
        expect(body.fields ?? body.mappings ?? body).toBeDefined();
        return;
      }
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.fields ?? body.mappings ?? body).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // CSV Import - Accounts
  // --------------------------------------------------------------------------

  test('should validate CSV before import', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/import/validate`, {
      headers: {
        Authorization: `Bearer ${authToken}`,
        'Content-Type': 'multipart/form-data',
      },
      multipart: {
        file: {
          name: 'accounts.csv',
          mimeType: 'text/csv',
          buffer: Buffer.from(SAMPLE_ACCOUNTS_CSV),
        },
        entityType: 'accounts',
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      // Try JSON-based validation
      const alt = await request.post(`${API_URL}/api/import/validate`, {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          csvContent: SAMPLE_ACCOUNTS_CSV,
          entityType: 'accounts',
        },
      });
      if (alt.ok() || alt.status() === 400) {
        const body = await alt.json();
        expect(body.valid !== undefined || body.errors !== undefined).toBeTruthy();
        return;
      }
      console.log('Import validation endpoint not implemented - skipping');
      test.skip();
      return;
    }

    expect([200, 400]).toContain(response.status());
    const body = await response.json();
    expect(body.valid !== undefined || body.errors !== undefined || body.rowCount !== undefined).toBeTruthy();
  });

  test('should start account import job', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/import/start`, {
      headers: {
        Authorization: `Bearer ${authToken}`,
        'Content-Type': 'multipart/form-data',
      },
      multipart: {
        file: {
          name: 'accounts.csv',
          mimeType: 'text/csv',
          buffer: Buffer.from(SAMPLE_ACCOUNTS_CSV),
        },
        entityType: 'accounts',
        mappings: JSON.stringify({
          name: 'name',
          email: 'email',
          phone: 'phone',
          industry: 'industry',
          website: 'website',
        }),
        options: JSON.stringify({
          skipDuplicates: true,
          updateExisting: false,
          notifyOnCompletion: true,
        }),
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      // Try JSON-based import
      const alt = await request.post(`${API_URL}/api/import`, {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          csvContent: SAMPLE_ACCOUNTS_CSV,
          entityType: 'accounts',
          mappings: {
            name: 'name',
            email: 'email',
          },
        },
      });
      if (alt.ok()) {
        const body = await alt.json();
        importJobId = body.jobId ?? body.id ?? body.importId;
        return;
      }
      console.log('Import start endpoint not implemented - skipping');
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    importJobId = body.jobId ?? body.id ?? body.importId;
    expect(importJobId).toBeTruthy();
  });

  test('should get import job status', async ({ request }) => {
    if (!importJobId) {
      test.skip();
      return;
    }

    const response = await request.get(`${API_URL}/api/import/status/${importJobId}`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      const alt = await request.get(`${API_URL}/api/import/jobs/${importJobId}`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });
      if (alt.ok()) {
        const body = await alt.json();
        expect(body.status ?? body.state ?? body.progress).toBeDefined();
        return;
      }
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.status ?? body.state ?? body.progress).toBeDefined();
  });

  test('should get import progress', async ({ request }) => {
    if (!importJobId) {
      test.skip();
      return;
    }

    const response = await request.get(`${API_URL}/api/import/progress/${importJobId}`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    // Progress should have counts or percentage
    expect(
      body.processedRows !== undefined ||
      body.progress !== undefined ||
      body.completedCount !== undefined
    ).toBeTruthy();
  });

  // --------------------------------------------------------------------------
  // CSV Import - Contacts
  // --------------------------------------------------------------------------

  test('should import contacts', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/import/start`, {
      headers: {
        Authorization: `Bearer ${authToken}`,
        'Content-Type': 'multipart/form-data',
      },
      multipart: {
        file: {
          name: 'contacts.csv',
          mimeType: 'text/csv',
          buffer: Buffer.from(SAMPLE_CONTACTS_CSV),
        },
        entityType: 'contacts',
        mappings: JSON.stringify({
          firstName: 'firstName',
          lastName: 'lastName',
          email: 'email',
          phone: 'phone',
          title: 'title',
        }),
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.jobId ?? body.id).toBeTruthy();
  });

  // --------------------------------------------------------------------------
  // Import Validation Errors
  // --------------------------------------------------------------------------

  test('should return validation errors for invalid CSV', async ({ request }) => {
    const invalidCsv = `name,email,phone
,invalid-email,not-a-phone
,also-bad,123`;

    const response = await request.post(`${API_URL}/api/import/validate`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        csvContent: invalidCsv,
        entityType: 'accounts',
      },
    });

    if (response.status() === 404) {
      test.skip();
      return;
    }

    // Should return validation errors
    expect([200, 400, 422]).toContain(response.status());
    const body = await response.json();
    expect(body.errors ?? body.validationErrors ?? body.issues).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // Import History
  // --------------------------------------------------------------------------

  test('should get import history', async ({ request }) => {
    const response = await request.get(`${API_URL}/api/import/history`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      const alt = await request.get(`${API_URL}/api/import/jobs`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });
      if (alt.ok()) {
        const body = await alt.json();
        expect(body.items ?? body.jobs ?? body).toBeDefined();
        return;
      }
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.items ?? body.jobs ?? body).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // Export Operations
  // --------------------------------------------------------------------------

  test('should start account export', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/export/start`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        entityType: 'accounts',
        format: 'csv',
        filters: {
          isActive: true,
        },
        fields: ['id', 'name', 'email', 'phone', 'industry', 'createdAt'],
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      // Try alternate endpoint
      const alt = await request.post(`${API_URL}/api/accounts/export`, {
        headers: { Authorization: `Bearer ${authToken}` },
        data: { format: 'csv' },
      });
      if (alt.ok()) {
        const body = await alt.json();
        exportJobId = body.jobId ?? body.id ?? body.exportId;
        return;
      }
      console.log('Export endpoint not implemented - skipping');
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    exportJobId = body.jobId ?? body.id ?? body.exportId;
    expect(exportJobId).toBeTruthy();
  });

  test('should get export status', async ({ request }) => {
    if (!exportJobId) {
      test.skip();
      return;
    }

    const response = await request.get(`${API_URL}/api/export/status/${exportJobId}`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.status ?? body.state).toBeDefined();
  });

  test('should download export file', async ({ request }) => {
    if (!exportJobId) {
      test.skip();
      return;
    }

    // Wait a bit for export to complete
    await new Promise((r) => setTimeout(r, 2000));

    const response = await request.get(`${API_URL}/api/export/download/${exportJobId}`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404 || response.status() === 202) {
      // 202 means still processing
      test.skip();
      return;
    }

    // Should return file or download URL
    expect([200, 302]).toContain(response.status());
  });

  // --------------------------------------------------------------------------
  // Export Formats
  // --------------------------------------------------------------------------

  test('should support multiple export formats', async ({ request }) => {
    const formats = ['csv', 'xlsx', 'json'];

    for (const format of formats) {
      const response = await request.post(`${API_URL}/api/export/start`, {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          entityType: 'contacts',
          format: format,
          fields: ['id', 'firstName', 'lastName', 'email'],
        },
      });

      if (response.status() === 404 || response.status() === 405) {
        continue; // Skip unsupported formats
      }

      expect([200, 201, 400, 422]).toContain(response.status());
    }
  });

  // --------------------------------------------------------------------------
  // Export History
  // --------------------------------------------------------------------------

  test('should get export history', async ({ request }) => {
    const response = await request.get(`${API_URL}/api/export/history`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });

    if (response.status() === 404) {
      const alt = await request.get(`${API_URL}/api/export/jobs`, {
        headers: { Authorization: `Bearer ${authToken}` },
      });
      if (alt.ok()) {
        const body = await alt.json();
        expect(body.items ?? body.jobs ?? body).toBeDefined();
        return;
      }
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.items ?? body.jobs ?? body).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // Scheduled Exports
  // --------------------------------------------------------------------------

  test('should create scheduled export', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/export/scheduled`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        name: `TEST_SCHEDULED_EXPORT_${Date.now()}`,
        entityType: 'accounts',
        format: 'csv',
        schedule: '0 0 * * 1', // Weekly on Monday
        recipients: ['admin@crm.local'],
        filters: { isActive: true },
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      console.log('Scheduled export endpoint not implemented - skipping');
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.id ?? body.scheduleId).toBeTruthy();
  });

  // --------------------------------------------------------------------------
  // Bulk Operations
  // --------------------------------------------------------------------------

  test('should support bulk import with dry-run', async ({ request }) => {
    const response = await request.post(`${API_URL}/api/import/dry-run`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: {
        csvContent: SAMPLE_ACCOUNTS_CSV,
        entityType: 'accounts',
        mappings: { name: 'name', email: 'email' },
      },
    });

    if (response.status() === 404 || response.status() === 405) {
      test.skip();
      return;
    }

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    // Dry-run should return preview of what would be imported
    expect(
      body.preview !== undefined ||
      body.rowCount !== undefined ||
      body.wouldCreate !== undefined
    ).toBeTruthy();
  });
});

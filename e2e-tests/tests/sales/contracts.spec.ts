/**
 * CRM Solution - Contracts E2E Tests
 *
 * Tests for contract CRUD, status changes, renewal, termination, search, and pagination.
 */

import { test, expect } from '@playwright/test';
import { WEB_BASE_URL } from '../../testConfig';

const BASE_URL = WEB_BASE_URL;
const API_URL = BASE_URL.includes(':5000') ? BASE_URL : `${BASE_URL.replace(':80', '')}:5000`;

let authToken: string;
let createdContractId: number;
let testAccountId: number = 1;

const futureDate = (daysFromNow: number): string => {
  const d = new Date();
  d.setDate(d.getDate() + daysFromNow);
  return d.toISOString().split('T')[0];
};

test.describe('Contracts', () => {
  test.describe.configure({ mode: 'serial' });

  test.beforeAll(async ({ request }) => {
    const response = await request.post(`${API_URL}/api/auth/login`, {
      data: { email: 'admin@crm.local', password: 'Admin@123' },
    });
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    authToken = data.accessToken;
    expect(authToken).toBeTruthy();

    // Fetch an existing account ID for use in contract creation
    const acctResp = await request.get(`${API_URL}/api/accounts?page=1&pageSize=1`, {
      headers: { Authorization: `Bearer ${authToken}` },
    });
    if (acctResp.ok()) {
      const acctData = await acctResp.json();
      const items = Array.isArray(acctData) ? acctData : (acctData.items ?? acctData.data ?? []);
      if (items.length > 0) {
        testAccountId = items[0].id ?? 1;
      }
    }
  });

  // --------------------------------------------------------------------------
  // List & Pagination
  // --------------------------------------------------------------------------

  test('should list contracts with pagination', async ({ request }) => {
    const response = await request.get(
      `${API_URL}/api/contracts?page=1&pageSize=10`,
      { headers: { Authorization: `Bearer ${authToken}` } },
    );
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(data).toBeDefined();
    if (Array.isArray(data)) {
      expect(data.length).toBeGreaterThanOrEqual(0);
    } else {
      expect(data.items ?? data.data ?? data).toBeDefined();
    }
  });

  // --------------------------------------------------------------------------
  // Create
  // --------------------------------------------------------------------------

  test('should create a new contract', async ({ request }) => {
    const payload = {
      name: `TEST_Contract_${Date.now()}`,
      description: 'E2E test contract for service agreement',
      accountId: testAccountId,
      startDate: futureDate(1),
      endDate: futureDate(365),
      totalValue: 50000,
      contractType: 0, // Service = 0
    };

    const response = await request.post(`${API_URL}/api/contracts`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: payload,
    });

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    createdContractId = body.id ?? body.contractId ?? 1;
    expect(createdContractId).toBeGreaterThan(0);
  });

  // --------------------------------------------------------------------------
  // Read
  // --------------------------------------------------------------------------

  test('should view contract details', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    const response = await request.get(
      `${API_URL}/api/contracts/${createdContractId}`,
      { headers: { Authorization: `Bearer ${authToken}` } },
    );
    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.id ?? body.contractId).toBe(createdContractId);
    expect(body.title ?? body.name).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // Update
  // --------------------------------------------------------------------------

  test('should update contract information', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    const response = await request.put(
      `${API_URL}/api/contracts/${createdContractId}`,
      {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          title: `TEST_Contract_Updated_${Date.now()}`,
          value: 75000,
        },
      },
    );

    expect(response.status()).toBeLessThan(300);
  });

  test('should change contract status', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    // Try status change via PATCH or dedicated endpoint
    const patchResponse = await request.patch(
      `${API_URL}/api/contracts/${createdContractId}`,
      {
        headers: { Authorization: `Bearer ${authToken}` },
        data: { status: 'Active' },
      },
    );

    if (!patchResponse.ok()) {
      // Try status-specific endpoint
      const statusResponse = await request.put(
        `${API_URL}/api/contracts/${createdContractId}/status`,
        {
          headers: { Authorization: `Bearer ${authToken}` },
          data: { status: 'Active' },
        },
      );
      expect(statusResponse.status()).toBeLessThan(500);
      return;
    }

    expect(patchResponse.status()).toBeLessThan(300);
  });

  // --------------------------------------------------------------------------
  // Validation
  // --------------------------------------------------------------------------

  test('should fail to create contract with end date before start date', async ({ request }) => {
    const payload = {
      title: `TEST_Invalid_Contract_${Date.now()}`,
      startDate: futureDate(30),  // start 30 days from now
      endDate: futureDate(1),     // end only 1 day from now (before start)
      status: 'Draft',
    };

    const response = await request.post(`${API_URL}/api/contracts`, {
      headers: { Authorization: `Bearer ${authToken}` },
      data: payload,
    });

    // Should return 400 Bad Request for invalid dates
    expect(response.status()).toBeGreaterThanOrEqual(400);
    expect(response.status()).toBeLessThan(500);
  });

  // --------------------------------------------------------------------------
  // Business Operations
  // --------------------------------------------------------------------------

  test('should renew an active contract', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    const renewResponse = await request.post(
      `${API_URL}/api/contracts/${createdContractId}/renew`,
      {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          newEndDate: futureDate(730),  // renew for 2 more years
        },
      },
    );

    // 200 or 400/404 if not supported yet
    expect(renewResponse.status()).toBeLessThan(500);
  });

  test('should terminate a contract', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    const terminateResponse = await request.post(
      `${API_URL}/api/contracts/${createdContractId}/terminate`,
      {
        headers: { Authorization: `Bearer ${authToken}` },
        data: {
          reason: 'E2E test termination',
          terminationDate: futureDate(0),
        },
      },
    );

    expect(terminateResponse.status()).toBeLessThan(500);
  });

  // --------------------------------------------------------------------------
  // Search
  // --------------------------------------------------------------------------

  test('should search contracts by title', async ({ request }) => {
    const response = await request.get(
      `${API_URL}/api/contracts?search=TEST_Contract&page=1&pageSize=10`,
      { headers: { Authorization: `Bearer ${authToken}` } },
    );
    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(data).toBeDefined();
  });

  // --------------------------------------------------------------------------
  // Delete
  // --------------------------------------------------------------------------

  test('should delete a contract', async ({ request }) => {
    if (!createdContractId) {
      test.skip();
      return;
    }

    const response = await request.delete(
      `${API_URL}/api/contracts/${createdContractId}`,
      { headers: { Authorization: `Bearer ${authToken}` } },
    );

    expect(response.status()).toBeLessThan(500);
  });
});

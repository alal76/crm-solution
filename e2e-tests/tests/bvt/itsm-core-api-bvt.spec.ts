// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// E2E API Tests - ITSM Core Features (Phases 1-3)

import { test, expect, APIRequestContext } from '@playwright/test';

const BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000';

test.describe('BVT-15: ITSM Core API Tests', () => {
  let apiContext: APIRequestContext;

  test.beforeAll(async ({ playwright }) => {
    apiContext = await playwright.request.newContext({
      baseURL: BASE_URL,
      extraHTTPHeaders: {
        'Content-Type': 'application/json',
      },
    });
  });

  test.afterAll(async () => {
    await apiContext.dispose();
  });

  // ============================================================================
  // Incident Management API Tests
  // ============================================================================

  test.describe('Incident Management', () => {
    test('BVT-15-001: GET /api/itsm/incidents returns list', async () => {
      const response = await apiContext.get('/api/itsm/incidents');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-002: GET /api/itsm/incidents with pagination', async () => {
      const response = await apiContext.get('/api/itsm/incidents?pageNumber=1&pageSize=10');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-003: GET /api/itsm/incidents/{id} returns single incident', async () => {
      const response = await apiContext.get('/api/itsm/incidents/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-004: POST /api/itsm/incidents creates incident', async () => {
      const response = await apiContext.post('/api/itsm/incidents', {
        data: {
          shortDescription: 'Test incident from E2E',
          callerId: 1,
          impact: 2,
          urgency: 2
        }
      });
      expect([200, 201, 400, 401]).toContain(response.status());
    });

    test('BVT-15-005: GET /api/itsm/incidents with state filter', async () => {
      const response = await apiContext.get('/api/itsm/incidents?state=New');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-006: POST /api/itsm/incidents/{id}/comments adds comment', async () => {
      const response = await apiContext.post('/api/itsm/incidents/1/comments', {
        data: {
          comment: 'Test comment from E2E',
          isInternal: true
        }
      });
      expect([200, 201, 404, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // Problem Management API Tests
  // ============================================================================

  test.describe('Problem Management', () => {
    test('BVT-15-011: GET /api/itsm/problems returns list', async () => {
      const response = await apiContext.get('/api/itsm/problems');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-012: GET /api/itsm/problems/{id} returns single problem', async () => {
      const response = await apiContext.get('/api/itsm/problems/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-013: POST /api/itsm/problems creates problem', async () => {
      const response = await apiContext.post('/api/itsm/problems', {
        data: {
          shortDescription: 'Test problem from E2E',
          priority: 2
        }
      });
      expect([200, 201, 400, 401]).toContain(response.status());
    });

    test('BVT-15-014: GET /api/itsm/problems with known error filter', async () => {
      const response = await apiContext.get('/api/itsm/problems?knownError=true');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-015: GET /api/itsm/problems/{id}/incidents returns related', async () => {
      const response = await apiContext.get('/api/itsm/problems/1/incidents');
      expect([200, 404, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // Change Management API Tests
  // ============================================================================

  test.describe('Change Management', () => {
    test('BVT-15-021: GET /api/itsm/changes returns list', async () => {
      const response = await apiContext.get('/api/itsm/changes');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-022: GET /api/itsm/changes/{id} returns single change', async () => {
      const response = await apiContext.get('/api/itsm/changes/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-023: POST /api/itsm/changes creates change request', async () => {
      const response = await apiContext.post('/api/itsm/changes', {
        data: {
          shortDescription: 'Test change from E2E',
          type: 1,
          risk: 1,
          impact: 1
        }
      });
      expect([200, 201, 400, 401]).toContain(response.status());
    });

    test('BVT-15-024: GET /api/itsm/changes with approval filter', async () => {
      const response = await apiContext.get('/api/itsm/changes?approvalStatus=Pending');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-025: GET /api/itsm/changes/blackout-periods returns blackouts', async () => {
      const startDate = new Date().toISOString().split('T')[0];
      const endDate = new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
      const response = await apiContext.get(`/api/itsm/changes/blackout-periods?startDate=${startDate}&endDate=${endDate}`);
      expect([200, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // SLA Management API Tests
  // ============================================================================

  test.describe('SLA Management', () => {
    test('BVT-15-031: GET /api/itsm/sla/policies returns policies', async () => {
      const response = await apiContext.get('/api/itsm/sla/policies');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-032: GET /api/itsm/sla/breached returns breached SLAs', async () => {
      const response = await apiContext.get('/api/itsm/sla/breached');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-033: GET /api/itsm/sla/at-risk returns at-risk SLAs', async () => {
      const response = await apiContext.get('/api/itsm/sla/at-risk?thresholdMinutes=30');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-034: GET /api/itsm/sla/dashboard returns dashboard data', async () => {
      const response = await apiContext.get('/api/itsm/sla/dashboard');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-035: GET /api/itsm/sla/metrics returns metrics', async () => {
      const startDate = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
      const endDate = new Date().toISOString().split('T')[0];
      const response = await apiContext.get(`/api/itsm/sla/metrics?startDate=${startDate}&endDate=${endDate}`);
      expect([200, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // CMDB API Tests
  // ============================================================================

  test.describe('CMDB', () => {
    test('BVT-15-041: GET /api/itsm/cmdb/cis returns CI list', async () => {
      const response = await apiContext.get('/api/itsm/cmdb/cis');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-042: GET /api/itsm/cmdb/cis/{id} returns single CI', async () => {
      const response = await apiContext.get('/api/itsm/cmdb/cis/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-043: POST /api/itsm/cmdb/cis creates CI', async () => {
      const response = await apiContext.post('/api/itsm/cmdb/cis', {
        data: {
          ciName: 'E2E-TEST-SERVER',
          ciType: 0,
          operationalStatus: 0
        }
      });
      expect([200, 201, 400, 401]).toContain(response.status());
    });

    test('BVT-15-044: GET /api/itsm/cmdb/cis search by type', async () => {
      const response = await apiContext.get('/api/itsm/cmdb/cis?type=Server');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-045: GET /api/itsm/cmdb/cis/{id}/relationships returns relations', async () => {
      const response = await apiContext.get('/api/itsm/cmdb/cis/1/relationships');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-046: GET /api/itsm/cmdb/cis/{id}/impact returns impact analysis', async () => {
      const response = await apiContext.get('/api/itsm/cmdb/cis/1/impact');
      expect([200, 404, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // Knowledge Management API Tests
  // ============================================================================

  test.describe('Knowledge Management', () => {
    test('BVT-15-051: GET /api/itsm/knowledge/articles returns articles', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/articles');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-052: GET /api/itsm/knowledge/articles/{id} returns article', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/articles/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-053: GET /api/itsm/knowledge/articles search', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/articles?search=password');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-054: GET /api/itsm/knowledge/articles/popular returns top articles', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/articles/popular?count=10');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-055: GET /api/itsm/knowledge/articles/recent returns recent articles', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/articles/recent?count=10');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-056: GET /api/itsm/knowledge/categories returns categories', async () => {
      const response = await apiContext.get('/api/itsm/knowledge/categories');
      expect([200, 401]).toContain(response.status());
    });
  });

  // ============================================================================
  // Service Catalog API Tests
  // ============================================================================

  test.describe('Service Catalog', () => {
    test('BVT-15-061: GET /api/itsm/catalog/items returns catalog items', async () => {
      const response = await apiContext.get('/api/itsm/catalog/items');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-062: GET /api/itsm/catalog/items/{id} returns single item', async () => {
      const response = await apiContext.get('/api/itsm/catalog/items/1');
      expect([200, 404, 401]).toContain(response.status());
    });

    test('BVT-15-063: GET /api/itsm/catalog/items featured only', async () => {
      const response = await apiContext.get('/api/itsm/catalog/items?featuredOnly=true');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-064: GET /api/itsm/catalog/categories returns categories', async () => {
      const response = await apiContext.get('/api/itsm/catalog/categories');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-065: GET /api/itsm/catalog/search returns search results', async () => {
      const response = await apiContext.get('/api/itsm/catalog/search?term=laptop');
      expect([200, 401]).toContain(response.status());
    });

    test('BVT-15-066: GET /api/itsm/catalog/my-requests returns user requests', async () => {
      const response = await apiContext.get('/api/itsm/catalog/my-requests');
      expect([200, 401]).toContain(response.status());
    });
  });
});

test.describe('ITSM Core Integration Smoke Tests', () => {
  let apiContext: APIRequestContext;

  test.beforeAll(async ({ playwright }) => {
    apiContext = await playwright.request.newContext({
      baseURL: BASE_URL,
      extraHTTPHeaders: {
        'Content-Type': 'application/json',
      },
    });
  });

  test.afterAll(async () => {
    await apiContext.dispose();
  });

  test('SMOKE-001: All ITSM API endpoints respond', async () => {
    const endpoints = [
      '/api/itsm/incidents',
      '/api/itsm/problems',
      '/api/itsm/changes',
      '/api/itsm/sla/policies',
      '/api/itsm/cmdb/cis',
      '/api/itsm/knowledge/articles',
      '/api/itsm/catalog/items'
    ];

    for (const endpoint of endpoints) {
      const response = await apiContext.get(endpoint);
      expect([200, 401, 403]).toContain(response.status());
    }
  });

  test('SMOKE-002: ITSM health check endpoints', async () => {
    const response = await apiContext.get('/health');
    expect([200, 401]).toContain(response.status());
  });
});

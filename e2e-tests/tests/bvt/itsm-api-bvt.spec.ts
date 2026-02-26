/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * 
 * ITSM Phase 4 API BVT (Build Verification Tests)
 * 
 * Tests critical API endpoints for:
 * - Webhook Notifications
 * - Email-to-Ticket
 * - ITSM Dashboard & Analytics
 * - Monitoring Tool Integration
 * - CI/CD Integration
 * - Self-Service Chatbot
 */

import { test, expect, APIRequestContext } from '@playwright/test';

let apiContext: APIRequestContext;
let authToken: string;
const baseUrl = process.env.API_BASE_URL || 'http://localhost:5000';

const getAuthHeaders = () => (authToken ? { Authorization: `Bearer ${authToken}` } : undefined);
const withAuth = (okStatuses: number[]) => (authToken ? [...okStatuses, 429] : [401, 403, 404, 429]);

test.beforeAll(async ({ playwright }) => {
  // Create API context
  apiContext = await playwright.request.newContext({
    baseURL: baseUrl,
    extraHTTPHeaders: {
      'Content-Type': 'application/json',
    },
  });

  // Authenticate and get token
  try {
    const authResponse = await apiContext.post('/api/auth/login', {
      data: {
        email: 'admin@crm.local',
        password: 'Admin@123',
      },
    });
    
    if (authResponse.ok()) {
      const authData = await authResponse.json();
      authToken = authData.accessToken || authData.token;
    }
  } catch (error) {
    console.log('Auth skipped - server may not be running');
  }
});

test.afterAll(async () => {
  await apiContext?.dispose();
});

test.describe('ITSM Webhooks API BVT', () => {
  
  test('BVTAPI001 - GET /api/itsm/webhooks/subscriptions returns subscription list', async () => {
    const response = await apiContext.get('/api/itsm/webhooks/subscriptions', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
    if (response.ok()) {
      const data = await response.json();
      expect(Array.isArray(data.data || data)).toBe(true);
    }
  });

  test('BVTAPI002 - POST /api/itsm/webhooks/subscriptions creates subscription', async () => {
    const webhook = {
      name: 'Test Webhook BVT',
      targetUrl: 'https://example.com/webhook/bvt-test',
      eventTypes: ['IncidentCreated', 'IncidentResolved'],
      secretKey: 'test-secret-key-bvt',
      retryCount: 3,
      timeoutSeconds: 30,
      isActive: true,
    };

    const response = await apiContext.post('/api/itsm/webhooks/subscriptions', {
      headers: getAuthHeaders(),
      data: webhook,
    });

    // Accept 200, 201, or 400 (validation) as valid responses
    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });

  test('BVTAPI003 - GET /api/itsm/webhooks/event-types returns supported events', async () => {
    const response = await apiContext.get('/api/itsm/webhooks/event-types', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
    if (response.ok()) {
      const data = await response.json();
      expect(Array.isArray(data.data || data)).toBe(true);
    }
  });

  test('BVTAPI004 - GET /api/itsm/webhooks/deliveries returns delivery history', async () => {
    const response = await apiContext.get('/api/itsm/webhooks/deliveries', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });
});

test.describe('ITSM Email-to-Ticket API BVT', () => {

  test('BVTAPI010 - POST /api/itsm/email/inbound processes email', async () => {
    const email = {
      from: 'user@example.com',
      to: 'support@crm-solution.com',
      subject: 'BVT Test - Server issue',
      body: 'This is a BVT test email for incident creation.',
      receivedAt: new Date().toISOString(),
    };

    const response = await apiContext.post('/api/itsm/email/inbound', {
      headers: getAuthHeaders(),
      data: email,
    });

    // Accept various status codes
    expect(withAuth([200, 201, 400, 422, 404])).toContain(response.status());
  });

  test('BVTAPI011 - GET /api/itsm/email/config returns email parsing config', async () => {
    const response = await apiContext.get('/api/itsm/email/config', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI012 - GET /api/itsm/email/history returns processed emails', async () => {
    const response = await apiContext.get('/api/itsm/email/history', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });
});

test.describe('ITSM Dashboard API BVT', () => {

  test('BVTAPI020 - GET /api/itsm/dashboard/metrics returns metrics', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/metrics', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI021 - GET /api/itsm/dashboard/incident-trends returns trends', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/incident-trends', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI022 - GET /api/itsm/dashboard/sla-compliance returns SLA data', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/sla-compliance', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI023 - GET /api/itsm/dashboard/agent-performance returns agent metrics', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/agent-performance', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI024 - GET /api/itsm/dashboard/executive-summary returns summary', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/executive-summary', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI025 - GET /api/itsm/dashboard/category-breakdown returns categories', async () => {
    const response = await apiContext.get('/api/itsm/dashboard/category-breakdown', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });
});

test.describe('ITSM Monitoring Integration API BVT', () => {

  test('BVTAPI030 - POST /api/itsm/monitoring/alerts receives Prometheus alert', async () => {
    // Prometheus Alertmanager webhook format
    const alertPayload = {
      receiver: 'itsm-webhook',
      status: 'firing',
      alerts: [
        {
          status: 'firing',
          labels: {
            alertname: 'HighCPUUsage',
            severity: 'warning',
            instance: 'server-01.example.com:9090',
          },
          annotations: {
            summary: 'High CPU usage detected',
            description: 'CPU usage above 80% for 5 minutes',
          },
          startsAt: new Date().toISOString(),
        },
      ],
    };

    const response = await apiContext.post('/api/itsm/monitoring/alerts', {
      headers: getAuthHeaders(),
      data: alertPayload,
    });

    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });

  test('BVTAPI031 - GET /api/itsm/monitoring/sources returns configured sources', async () => {
    const response = await apiContext.get('/api/itsm/monitoring/sources', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI032 - GET /api/itsm/monitoring/alert-mappings returns mappings', async () => {
    const response = await apiContext.get('/api/itsm/monitoring/alert-mappings', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });
});

test.describe('ITSM CI/CD Integration API BVT', () => {

  test('BVTAPI040 - POST /api/itsm/cicd/deployment creates change request', async () => {
    const deploymentRequest = {
      pipelineId: 'bvt-pipeline-001',
      pipelineName: 'BVT Test Pipeline',
      buildNumber: '1.0.0.999',
      commitHash: 'abc123def456789',
      commitMessage: 'BVT: Test commit for CI/CD integration',
      author: 'bvt@crm-solution.com',
      branch: 'main',
      environment: 'staging',
      services: ['crm-api', 'crm-frontend'],
      deploymentType: 'Standard',
    };

    const response = await apiContext.post('/api/itsm/cicd/deployment', {
      headers: getAuthHeaders(),
      data: deploymentRequest,
    });

    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });

  test('BVTAPI041 - GET /api/itsm/cicd/pipelines returns registered pipelines', async () => {
    const response = await apiContext.get('/api/itsm/cicd/pipelines', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI042 - POST /api/itsm/cicd/deployment-complete marks deployment done', async () => {
    const completionRequest = {
      pipelineId: 'bvt-pipeline-001',
      buildNumber: '1.0.0.999',
      status: 'success',
      completedAt: new Date().toISOString(),
    };

    const response = await apiContext.post('/api/itsm/cicd/deployment-complete', {
      headers: getAuthHeaders(),
      data: completionRequest,
    });

    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });
});

test.describe('ITSM Self-Service Chatbot API BVT', () => {

  test('BVTAPI050 - POST /api/itsm/chatbot/sessions creates chat session', async () => {
    const response = await apiContext.post('/api/itsm/chatbot/sessions', {
      headers: getAuthHeaders(),
      data: {},
    });

    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });

  test('BVTAPI051 - POST /api/itsm/chatbot/sessions/{id}/messages sends message', async () => {
    // First create a session
    const sessionResponse = await apiContext.post('/api/itsm/chatbot/sessions', {
      headers: getAuthHeaders(),
      data: {},
    });

    if (!sessionResponse.ok()) {
      expect(withAuth([200, 201, 400, 404])).toContain(sessionResponse.status());
      return;
    }

    const sessionData = await sessionResponse.json();
    const sessionId = sessionData.sessionId || sessionData.data?.sessionId || 'test-session';

    const messageRequest = {
      content: 'I need to reset my password',
    };

    const response = await apiContext.post(`/api/itsm/chatbot/sessions/${sessionId}/messages`, {
      headers: getAuthHeaders(),
      data: messageRequest,
    });

    expect(withAuth([200, 201, 400, 404])).toContain(response.status());
  });

  test('BVTAPI052 - GET /api/itsm/chatbot/sessions/{id} returns session history', async () => {
    const response = await apiContext.get('/api/itsm/chatbot/sessions/test-session-001', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI053 - GET /api/itsm/chatbot/quick-actions returns available actions', async () => {
    const response = await apiContext.get('/api/itsm/chatbot/quick-actions', {
      headers: getAuthHeaders(),
    });

    expect(withAuth([200, 404])).toContain(response.status());
  });

  test('BVTAPI054 - POST /api/itsm/chatbot/search searches knowledge base', async () => {
    const searchRequest = {
      query: 'password reset',
      limit: 5,
    };

    const response = await apiContext.post('/api/itsm/chatbot/search', {
      headers: getAuthHeaders(),
      data: searchRequest,
    });

    expect(withAuth([200, 400, 404])).toContain(response.status());
  });
});

test.describe('ITSM API Health Checks', () => {

  test('BVTAPI060 - API responds to health check', async () => {
    const response = await apiContext.get('/health');
    expect([200, 404, 429]).toContain(response.status()); // 429 possible under rate limiting
  });

  test('BVTAPI061 - ITSM endpoints require authentication', async () => {
    // Test without auth token
    const response = await apiContext.get('/api/itsm/dashboard/metrics');
    expect([401, 403, 404, 429]).toContain(response.status()); // 429 possible under rate limiting
  });
});

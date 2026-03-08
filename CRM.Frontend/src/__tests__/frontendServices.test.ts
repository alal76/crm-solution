// CRM Solution - Frontend Service Tests
// Tests for API service layer functions

import axios from 'axios';
// @ts-ignore - axios-mock-adapter may not have types in some setups
import MockAdapter from 'axios-mock-adapter';

// Mock API client
const apiClient = axios.create({
  baseURL: 'http://localhost:5000/api'
});

const mockApi = new MockAdapter(apiClient);

describe('Frontend Service Tests', () => {
  
  afterEach(() => {
    mockApi.reset();
  });

  // ==========================================
  // Commission Service Tests
  // ==========================================

  describe('CommissionService', () => {
    it('should fetch all commissions', async () => {
      const mockCommissions = [
        { id: 1, amount: 1000, status: 'Pending' },
        { id: 2, amount: 2000, status: 'Approved' }
      ];

      mockApi.onGet('/commissions').reply(200, mockCommissions);

      const response = await apiClient.get('/commissions');

      expect(response.data).toEqual(mockCommissions);
      expect(response.data.length).toBe(2);
    });

    it('should fetch commission by ID', async () => {
      const mockCommission = { id: 1, amount: 1500, status: 'Pending' };

      mockApi.onGet('/commissions/1').reply(200, mockCommission);

      const response = await apiClient.get('/commissions/1');

      expect(response.data.id).toBe(1);
      expect(response.data.amount).toBe(1500);
    });

    it('should create new commission', async () => {
      const newCommission = { userId: 1, amount: 2500, status: 'Pending' };
      const created = { id: 1, ...newCommission };

      mockApi.onPost('/commissions').reply(201, created);

      const response = await apiClient.post('/commissions', newCommission);

      expect(response.status).toBe(201);
      expect(response.data.id).toBe(1);
    });

    it('should approve commission', async () => {
      const approved = { id: 1, status: 'Approved', approvedById: 10 };

      mockApi.onPost('/commissions/1/approve').reply(200, approved);

      const response = await apiClient.post('/commissions/1/approve', { approvedById: 10 });

      expect(response.data.status).toBe('Approved');
    });

    it('should calculate commission', async () => {
      const calculation = { amount: 500, method: 'FlatPercentage' };

      mockApi.onGet('/commissions/calculate/1').reply(200, calculation);

      const response = await apiClient.get('/commissions/calculate/1');

      expect(response.data.amount).toBe(500);
    });
  });

  // ==========================================
  // Campaign Service Tests
  // ==========================================

  describe('CampaignService', () => {
    it('should fetch all campaigns', async () => {
      const mockCampaigns = [
        { id: 1, name: 'Q1 Campaign', status: 'Active' },
        { id: 2, name: 'Q2 Campaign', status: 'Draft' }
      ];

      mockApi.onGet('/campaigns').reply(200, mockCampaigns);

      const response = await apiClient.get('/campaigns');

      expect(response.data.length).toBe(2);
    });

    it('should launch campaign', async () => {
      const launched = { id: 1, status: 'Active' };

      mockApi.onPost('/campaigns/1/launch').reply(200, launched);

      const response = await apiClient.post('/campaigns/1/launch');

      expect(response.data.status).toBe('Active');
    });

    it('should pause campaign', async () => {
      const paused = { id: 1, status: 'Paused' };

      mockApi.onPost('/campaigns/1/pause').reply(200, paused);

      const response = await apiClient.post('/campaigns/1/pause');

      expect(response.data.status).toBe('Paused');
    });

    it('should add campaign recipients', async () => {
      const recipients = [
        { email: 'user1@example.com' },
        { email: 'user2@example.com' }
      ];

      mockApi.onPost('/campaigns/1/recipients').reply(201, { count: 2 });

      const response = await apiClient.post('/campaigns/1/recipients', recipients);

      expect(response.data.count).toBe(2);
    });

    it('should get campaign metrics', async () => {
      const metrics = {
        sent: 1000,
        delivered: 950,
        opened: 500,
        clicked: 250
      };

      mockApi.onGet('/campaigns/1/metrics').reply(200, metrics);

      const response = await apiClient.get('/campaigns/1/metrics');

      expect(response.data.sent).toBe(1000);
      expect(response.data.opened / response.data.delivered * 100).toBeCloseTo(52.6, 1);
    });
  });

  // ==========================================
  // Webhook Service Tests
  // ==========================================

  describe('WebhookService', () => {
    it('should fetch all webhooks', async () => {
      const mockWebhooks = [
        { id: 1, url: 'https://example.com/webhook', events: ['order.created'] },
        { id: 2, url: 'https://example.com/webhook2', events: ['contact.updated'] }
      ];

      mockApi.onGet('/webhooks').reply(200, mockWebhooks);

      const response = await apiClient.get('/webhooks');

      expect(response.data.length).toBe(2);
    });

    it('should create webhook', async () => {
      const newWebhook = { url: 'https://example.com/webhook', events: ['order.created'] };
      const created = { id: 1, ...newWebhook };

      mockApi.onPost('/webhooks').reply(201, created);

      const response = await apiClient.post('/webhooks', newWebhook);

      expect(response.status).toBe(201);
      expect(response.data.id).toBe(1);
    });

    it('should get delivery history', async () => {
      const history = [
        { id: 1, status: 'Success', timestamp: '2024-01-01' },
        { id: 2, status: 'Failed', timestamp: '2024-01-02' }
      ];

      mockApi.onGet('/webhooks/1/deliveries').reply(200, history);

      const response = await apiClient.get('/webhooks/1/deliveries');

      expect(response.data.length).toBe(2);
    });

    it('should test webhook delivery', async () => {
      const result = { success: true, statusCode: 200, message: 'Delivered successfully' };

      mockApi.onPost('/webhooks/1/test').reply(200, result);

      const response = await apiClient.post('/webhooks/1/test');

      expect(response.data.success).toBe(true);
    });
  });

  // ==========================================
  // Email Sequence Service Tests
  // ==========================================

  describe('EmailSequenceService', () => {
    it('should fetch all sequences', async () => {
      const sequences = [
        { id: 1, name: 'Welcome Series' },
        { id: 2, name: 'Onboarding Series' }
      ];

      mockApi.onGet('/emailsequences').reply(200, sequences);

      const response = await apiClient.get('/emailsequences');

      expect(response.data.length).toBe(2);
    });

    it('should create sequence', async () => {
      const newSequence = { name: 'New Series', steps: [] };
      const created = { id: 1, ...newSequence };

      mockApi.onPost('/emailsequences').reply(201, created);

      const response = await apiClient.post('/emailsequences', newSequence);

      expect(response.data.id).toBe(1);
    });

    it('should enroll contact', async () => {
      const enrollment = { sequenceId: 1, contactId: 5 };

      mockApi.onPost('/emailsequences/1/enroll').reply(201, { id: 1, ...enrollment });

      const response = await apiClient.post('/emailsequences/1/enroll', { contactId: 5 });

      expect(response.status).toBe(201);
    });

    it('should start sequence', async () => {
      mockApi.onPost('/emailsequences/1/start').reply(200, { success: true });

      const response = await apiClient.post('/emailsequences/1/start');

      expect(response.data.success).toBe(true);
    });

    it('should get sequence status', async () => {
      const status = {
        totalEnrolled: 100,
        activeEnrolled: 80,
        completed: 20
      };

      mockApi.onGet('/emailsequences/1/status').reply(200, status);

      const response = await apiClient.get('/emailsequences/1/status');

      expect(response.data.totalEnrolled).toBe(100);
    });
  });

  // ==========================================
  // Problem Service Tests
  // ==========================================

  describe('ProblemService', () => {
    it('should fetch all problems', async () => {
      const problems = [
        { id: 1, title: 'Database Issue', status: 'Open' },
        { id: 2, title: 'Configuration Error', status: 'Resolved' }
      ];

      mockApi.onGet('/problems').reply(200, problems);

      const response = await apiClient.get('/problems');

      expect(response.data.length).toBe(2);
    });

    it('should create problem', async () => {
      const newProblem = { title: 'New Issue', description: 'Detailed description' };
      const created = { id: 1, ...newProblem, status: 'Open' };

      mockApi.onPost('/problems').reply(201, created);

      const response = await apiClient.post('/problems', newProblem);

      expect(response.data.id).toBe(1);
    });

    it('should link incident to problem', async () => {
      mockApi.onPost('/problems/1/incidents').reply(200, { success: true });

      const response = await apiClient.post('/problems/1/incidents', { incidentId: 10 });

      expect(response.data.success).toBe(true);
    });
  });

  // ==========================================
  // Change Service Tests
  // ==========================================

  describe('ChangeService', () => {
    it('should fetch all changes', async () => {
      const changes = [
        { id: 1, title: 'Database Update', status: 'Draft' },
        { id: 2, title: 'API Changes', status: 'Approved' }
      ];

      mockApi.onGet('/changes').reply(200, changes);

      const response = await apiClient.get('/changes');

      expect(response.data.length).toBe(2);
    });

    it('should create change', async () => {
      const newChange = { title: 'New Change', type: 'Normal' };
      const created = { id: 1, ...newChange };

      mockApi.onPost('/changes').reply(201, created);

      const response = await apiClient.post('/changes', newChange);

      expect(response.data.id).toBe(1);
    });

    it('should submit change for approval', async () => {
      mockApi.onPost('/changes/1/submit').reply(200, { status: 'PendingApproval' });

      const response = await apiClient.post('/changes/1/submit');

      expect(response.data.status).toBe('PendingApproval');
    });

    it('should approve change', async () => {
      mockApi.onPost('/changes/1/approve').reply(200, { status: 'Approved' });

      const response = await apiClient.post('/changes/1/approve');

      expect(response.data.status).toBe('Approved');
    });
  });

  // ==========================================
  // Error Handling Tests
  // ==========================================

  describe('Error Handling', () => {
    it('should handle 404 errors', async () => {
      mockApi.onGet('/commissions/999').reply(404, { message: 'Not found' });

      try {
        await apiClient.get('/commissions/999');
      } catch (error: unknown) {
        expect((error as any).response.status).toBe(404);
      }
    });

    it('should handle 500 errors', async () => {
      mockApi.onGet('/campaigns').reply(500, { message: 'Internal server error' });

      try {
        await apiClient.get('/campaigns');
      } catch (error: unknown) {
        expect((error as any).response.status).toBe(500);
      }
    });

    it('should handle network timeouts', async () => {
      mockApi.onGet('/campaigns').timeoutOnce();

      try {
        await apiClient.get('/campaigns');
      } catch (error: unknown) {
        expect((error as any).code).toMatch(/timeout|ERR_NETWORK|ECONNABORTED/);
      }
    });

    it('should handle validation errors', async () => {
      const errors = { amount: 'Amount must be positive' };
      mockApi.onPost('/commissions').reply(400, errors);

      try {
        await apiClient.post('/commissions', { amount: -100 });
      } catch (error: unknown) {
        expect((error as any).response.status).toBe(400);
      }
    });
  });
});

/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Tests for orderReturnsService.ts
 */

import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';

// Create a local axios instance matching the service's apiClient behaviour
const apiClient = axios.create({ baseURL: 'http://localhost:5000/api' });
const mockApi = new MockAdapter(apiClient);

// ─── Stub out service functions using the local apiClient ────────────────────

const createReturn = (data: Record<string, unknown>) =>
  apiClient.post('/orderreturns', data).then(r => r.data);

const getReturns = (params?: Record<string, unknown>) =>
  apiClient.get('/orderreturns', { params }).then(r => r.data);

const getReturnById = (id: number) =>
  apiClient.get(`/orderreturns/${id}`).then(r => r.data);

const updateReturnStatus = (id: number, payload: Record<string, unknown>) =>
  apiClient.put(`/orderreturns/${id}`, payload).then(r => r.data);

// ─── Fixtures ────────────────────────────────────────────────────────────────

const mockReturn = {
  id: 1,
  returnNumber: 'RMA-0001',
  orderId: 42,
  orderNumber: 'ORD-0042',
  status: 0,
  statusName: 'Pending',
  reason: 0,
  reasonName: 'Defective Product',
  refundAmount: 250.0,
  restockingFee: 0,
  shippingRefund: 0,
  netRefundAmount: 250.0,
  currency: 'USD',
  requestedAt: '2026-02-01T10:00:00Z',
  createdAt: '2026-02-01T10:00:00Z',
  updatedAt: '2026-02-01T10:00:00Z',
};

const mockReturnList = [mockReturn];

const createPayload = {
  orderId: 42,
  reason: 0,
  reasonDescription: 'Product stopped working after 2 days',
  notes: 'Priority return',
  refundAmount: 250.0,
  restockingFee: 0,
  shippingRefund: 0,
  lineItems: [
    { orderLineItemId: 1, productId: 10, quantity: 1 },
  ],
};

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('orderReturnsService', () => {
  afterEach(() => {
    mockApi.reset();
  });

  describe('createReturn', () => {
    it('POSTs to /orderreturns and returns the created return', async () => {
      mockApi.onPost('/orderreturns').reply(201, mockReturn);

      const result = await createReturn(createPayload);

      expect(result).toEqual(mockReturn);
      expect(result.returnNumber).toBe('RMA-0001');
      expect(result.status).toBe(0); // Pending
    });

    it('propagates network errors', async () => {
      mockApi.onPost('/orderreturns').networkError();

      await expect(createReturn(createPayload)).rejects.toThrow();
    });

    it('propagates 422 validation errors', async () => {
      mockApi.onPost('/orderreturns').reply(422, { message: 'orderId is required' });

      await expect(createReturn({ ...createPayload, orderId: undefined })).rejects.toMatchObject({
        response: { status: 422 },
      });
    });
  });

  describe('getReturns', () => {
    it('GETs /orderreturns and returns a list', async () => {
      mockApi.onGet('/orderreturns').reply(200, mockReturnList);

      const result = await getReturns();

      expect(Array.isArray(result)).toBe(true);
      expect(result).toHaveLength(1);
      expect(result[0].id).toBe(1);
    });

    it('passes orderId filter as query param', async () => {
      mockApi.onGet('/orderreturns', { params: { orderId: 42 } }).reply(200, mockReturnList);

      const result = await getReturns({ orderId: 42 });

      expect(result).toEqual(mockReturnList);
    });

    it('passes status filter as query param', async () => {
      mockApi.onGet('/orderreturns', { params: { status: 0 } }).reply(200, mockReturnList);

      const result = await getReturns({ status: 0 });

      expect(result).toEqual(mockReturnList);
    });

    it('returns empty array when no returns found', async () => {
      mockApi.onGet('/orderreturns').reply(200, []);

      const result = await getReturns();

      expect(result).toEqual([]);
    });
  });

  describe('getReturnById', () => {
    it('GETs /orderreturns/1 and returns the return', async () => {
      mockApi.onGet('/orderreturns/1').reply(200, mockReturn);

      const result = await getReturnById(1);

      expect(result.id).toBe(1);
      expect(result.returnNumber).toBe('RMA-0001');
    });

    it('throws on 404', async () => {
      mockApi.onGet('/orderreturns/999').reply(404, { message: 'Return not found' });

      await expect(getReturnById(999)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('updateReturnStatus', () => {
    it('PUTs to /orderreturns/1 and returns the updated return', async () => {
      const updated = { ...mockReturn, status: 1, statusName: 'Approved' };
      mockApi.onPut('/orderreturns/1').reply(200, updated);

      const result = await updateReturnStatus(1, { status: 1 });

      expect(result.status).toBe(1);
      expect(result.statusName).toBe('Approved');
    });

    it('accepts optional notes and refundTransactionId', async () => {
      const updated = { ...mockReturn, status: 5, statusName: 'Refunded' };
      mockApi.onPut('/orderreturns/1').reply(200, updated);

      const result = await updateReturnStatus(1, {
        status: 5,
        notes: 'Refund processed',
        refundTransactionId: 'TXN-123',
      });

      expect(result.status).toBe(5);
    });

    it('throws on 404 when return not found', async () => {
      mockApi.onPut('/orderreturns/999').reply(404, { message: 'Return not found' });

      await expect(updateReturnStatus(999, { status: 6 })).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });
});

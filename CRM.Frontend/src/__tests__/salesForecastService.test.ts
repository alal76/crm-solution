/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Tests for salesForecastService.ts
 */

jest.mock('../services/apiClient', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

import apiClient from '../services/apiClient';
import {
  salesForecastService,
  ForecastCategory,
  SalesForecastDto,
  ForecastLineItemDto,
  ForecastHistoryDto,
} from '../services/salesForecastService';

const mockedGet = apiClient.get as jest.Mock;
const mockedPost = apiClient.post as jest.Mock;
const mockedPut = apiClient.put as jest.Mock;
const mockedDelete = apiClient.delete as jest.Mock;

const mockForecast: SalesForecastDto = {
  id: 1,
  createdAt: '2026-01-01T00:00:00Z',
  name: 'Q1 2026 Forecast',
  period: '2026-Q1',
  periodStartDate: '2026-01-01T00:00:00Z',
  periodEndDate: '2026-03-31T00:00:00Z',
  fiscalYear: 2026,
  fiscalQuarter: 1,
  quotaAmount: 100000,
  currencyCode: 'USD',
  closedWonAmount: 20000,
  commitAmount: 30000,
  bestCaseAmount: 15000,
  pipelineAmount: 50000,
  omittedAmount: 0,
  forecastAmount: 50000,
  gapToQuota: 50000,
  coverageRatio: 1,
  forecastAttainmentPercent: 50,
  isSubmitted: false,
};

const mockLineItem: ForecastLineItemDto = {
  id: 10,
  createdAt: '2026-01-05T00:00:00Z',
  category: ForecastCategory.Commit,
  amount: 15000,
  closeDate: '2026-02-15T00:00:00Z',
  stage: 'Negotiation',
  probability: 80,
  salesForecastId: 1,
  opportunityId: 42,
};

const mockHistory: ForecastHistoryDto = {
  id: 100,
  createdAt: '2026-01-10T00:00:00Z',
  snapshotDate: '2026-01-10T00:00:00Z',
  period: '2026-Q1',
  quotaAmount: 100000,
  closedWonAmount: 10000,
  commitAmount: 20000,
  bestCaseAmount: 10000,
  pipelineAmount: 40000,
  weeksRemaining: 8,
};

describe('salesForecastService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getAll', () => {
    it('GETs /api/sales-forecasts with no filters', async () => {
      mockedGet.mockResolvedValueOnce({ data: [mockForecast] });

      const result = await salesForecastService.getAll();

      expect(mockedGet).toHaveBeenCalledWith('/api/sales-forecasts');
      expect(result).toEqual([mockForecast]);
    });

    it('appends userId/teamId/fiscalYear/isSubmitted as query params', async () => {
      mockedGet.mockResolvedValueOnce({ data: [] });

      await salesForecastService.getAll({ userId: 5, teamId: 9, fiscalYear: 2026, isSubmitted: true });

      const calledUrl = mockedGet.mock.calls[0][0] as string;
      expect(calledUrl).toContain('/api/sales-forecasts?');
      expect(calledUrl).toContain('userId=5');
      expect(calledUrl).toContain('teamId=9');
      expect(calledUrl).toContain('fiscalYear=2026');
      expect(calledUrl).toContain('isSubmitted=true');
    });
  });

  describe('getById', () => {
    it('GETs /api/sales-forecasts/{id}', async () => {
      mockedGet.mockResolvedValueOnce({ data: mockForecast });

      const result = await salesForecastService.getById(1);

      expect(mockedGet).toHaveBeenCalledWith('/api/sales-forecasts/1');
      expect(result).toEqual(mockForecast);
    });
  });

  describe('create', () => {
    it('POSTs to /api/sales-forecasts', async () => {
      mockedPost.mockResolvedValueOnce({ data: mockForecast });

      const payload = { name: 'Q1 2026 Forecast', period: '2026-Q1', fiscalYear: 2026 };
      const result = await salesForecastService.create(payload);

      expect(mockedPost).toHaveBeenCalledWith('/api/sales-forecasts', payload);
      expect(result).toEqual(mockForecast);
    });
  });

  describe('update', () => {
    it('PUTs to /api/sales-forecasts/{id}', async () => {
      mockedPut.mockResolvedValueOnce({ data: {} });

      await salesForecastService.update(1, { quotaAmount: 120000 });

      expect(mockedPut).toHaveBeenCalledWith('/api/sales-forecasts/1', { quotaAmount: 120000 });
    });
  });

  describe('remove', () => {
    it('DELETEs /api/sales-forecasts/{id}', async () => {
      mockedDelete.mockResolvedValueOnce({ data: {} });

      await salesForecastService.remove(1);

      expect(mockedDelete).toHaveBeenCalledWith('/api/sales-forecasts/1');
    });
  });

  describe('submit', () => {
    it('POSTs to /api/sales-forecasts/{id}/submit', async () => {
      mockedPost.mockResolvedValueOnce({ data: {} });

      await salesForecastService.submit(1);

      expect(mockedPost).toHaveBeenCalledWith('/api/sales-forecasts/1/submit');
    });
  });

  describe('getHistory', () => {
    it('GETs /api/sales-forecasts/history with period param', async () => {
      mockedGet.mockResolvedValueOnce({ data: [mockHistory] });

      const result = await salesForecastService.getHistory('2026-Q1');

      const calledUrl = mockedGet.mock.calls[0][0] as string;
      expect(calledUrl).toContain('/api/sales-forecasts/history?');
      expect(calledUrl).toContain('period=2026-Q1');
      expect(result).toEqual([mockHistory]);
    });

    it('includes userId when provided', async () => {
      mockedGet.mockResolvedValueOnce({ data: [] });

      await salesForecastService.getHistory('2026-Q1', 7);

      const calledUrl = mockedGet.mock.calls[0][0] as string;
      expect(calledUrl).toContain('userId=7');
    });
  });

  describe('createSnapshot', () => {
    it('POSTs to /api/sales-forecasts/{id}/snapshot', async () => {
      mockedPost.mockResolvedValueOnce({ data: mockHistory });

      const result = await salesForecastService.createSnapshot(1);

      expect(mockedPost).toHaveBeenCalledWith('/api/sales-forecasts/1/snapshot');
      expect(result).toEqual(mockHistory);
    });
  });

  describe('getLineItems', () => {
    it('GETs /api/sales-forecasts/{forecastId}/line-items', async () => {
      mockedGet.mockResolvedValueOnce({ data: [mockLineItem] });

      const result = await salesForecastService.getLineItems(1);

      expect(mockedGet).toHaveBeenCalledWith('/api/sales-forecasts/1/line-items');
      expect(result).toEqual([mockLineItem]);
    });
  });
});

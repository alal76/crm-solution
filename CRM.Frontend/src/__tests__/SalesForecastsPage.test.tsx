/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Tests for SalesForecastsPage.tsx
 */
import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SalesForecastsPage from '../pages/SalesForecastsPage';
import { renderWithProviders } from '../test-utils/renderWithProviders';
import { ForecastCategory, SalesForecastDto, ForecastHistoryDto, ForecastLineItemDto } from '../services/salesForecastService';

jest.mock('../services/salesForecastService', () => {
  const actual = jest.requireActual('../services/salesForecastService');
  return {
    ...actual,
    salesForecastService: {
      getAll: jest.fn(),
      getById: jest.fn(),
      create: jest.fn(),
      update: jest.fn(),
      remove: jest.fn(),
      submit: jest.fn(),
      getHistory: jest.fn(),
      createSnapshot: jest.fn(),
      getLineItems: jest.fn(),
    },
  };
});

// eslint-disable-next-line @typescript-eslint/no-var-requires
const { salesForecastService } = require('../services/salesForecastService');

// jsdom does not implement ResizeObserver, which recharts' ResponsiveContainer requires.
class ResizeObserverMock {
  observe() {}
  unobserve() {}
  disconnect() {}
}
(global as unknown as { ResizeObserver: unknown }).ResizeObserver = ResizeObserverMock;

const mockForecasts: SalesForecastDto[] = [
  {
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
    pipelineAmount: 90000,
    omittedAmount: 0,
    forecastAmount: 50000,
    gapToQuota: 50000,
    coverageRatio: 1.8,
    forecastAttainmentPercent: 50,
    isSubmitted: false,
  },
];

const mockHistory: ForecastHistoryDto[] = [
  {
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
  },
];

const mockLineItems: ForecastLineItemDto[] = [
  {
    id: 10,
    createdAt: '2026-01-05T00:00:00Z',
    category: ForecastCategory.Commit,
    amount: 30000,
    closeDate: '2026-02-15T00:00:00Z',
    stage: 'Negotiation',
    probability: 80,
    salesForecastId: 1,
    opportunityId: 42,
  },
];

describe('SalesForecastsPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    salesForecastService.getAll.mockResolvedValue(mockForecasts);
    salesForecastService.getHistory.mockResolvedValue(mockHistory);
    salesForecastService.getLineItems.mockResolvedValue(mockLineItems);
    salesForecastService.submit.mockResolvedValue(undefined);
  });

  it('renders the page header', async () => {
    renderWithProviders(<SalesForecastsPage />);

    expect(await screen.findByRole('heading', { name: /sales forecasts/i })).toBeInTheDocument();
  });

  it('loads forecasts and history on mount', async () => {
    renderWithProviders(<SalesForecastsPage />);

    await waitFor(() => expect(salesForecastService.getAll).toHaveBeenCalled());
    await waitFor(() => expect(salesForecastService.getHistory).toHaveBeenCalled());
  });

  it('renders KPI summary cards with computed totals', async () => {
    renderWithProviders(<SalesForecastsPage />);

    expect(await screen.findByText('Total Quota')).toBeInTheDocument();
    expect(screen.getByText('Total Forecast')).toBeInTheDocument();
    expect(screen.getAllByText('Closed Won').length).toBeGreaterThan(0);
    expect(screen.getByText('Coverage Ratio')).toBeInTheDocument();

    // Total quota across the single mock forecast is $100,000.00 -> formatted as $100.0K
    await waitFor(() => expect(screen.getAllByText('$100.0K').length).toBeGreaterThan(0));
  });

  it('renders the forecasts list', async () => {
    renderWithProviders(<SalesForecastsPage />);

    expect(await screen.findByText('Q1 2026 Forecast')).toBeInTheDocument();
    expect(screen.getByText('2026-Q1')).toBeInTheDocument();
    expect(screen.getByText('Draft')).toBeInTheDocument();
  });

  it('drills into a forecast to show line items grouped by category on row click', async () => {
    const user = userEvent.setup();
    renderWithProviders(<SalesForecastsPage />);

    const row = await screen.findByText('Q1 2026 Forecast');
    await user.click(row);

    await waitFor(() => expect(salesForecastService.getLineItems).toHaveBeenCalledWith(1));
    expect(await screen.findByText(/Line Items — Q1 2026 Forecast/i)).toBeInTheDocument();
    expect(await screen.findByText(/Commit/i)).toBeInTheDocument();
  });

  it('submits a draft forecast when the Submit button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<SalesForecastsPage />);

    await screen.findByText('Q1 2026 Forecast');
    const submitButton = screen.getByRole('button', { name: /submit/i });
    await user.click(submitButton);

    await waitFor(() => expect(salesForecastService.submit).toHaveBeenCalledWith(1));
  });
});

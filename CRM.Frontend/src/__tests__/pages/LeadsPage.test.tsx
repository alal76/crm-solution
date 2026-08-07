import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import LeadsPage from '../../pages/LeadsPage';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

// REM-ORPHAN-003: LeadsPage now calls the real `/api/leads` Lead API via
// leadService (which wraps apiClient) instead of the legacy
// `/contacts/type/Lead` Contacts-as-Leads flow. The envelope returned by
// GET /api/leads is `{ data: LeadSummaryDto[], totalCount, page, pageSize,
// totalPages }` — see CRM.Api.Controllers.LeadsController.GetAll.
const mockLeadsResponse = {
  data: {
    data: [
      {
        id: 42,
        firstName: 'Ada',
        lastName: 'Lovelace',
        fullName: 'Ada Lovelace',
        email: 'ada@example.com',
        phone: '555-0100',
        companyName: 'Analytical Engines Inc',
        title: 'Chief Mathematician',
        status: 'New',
        source: 'Web',
        score: 72,
        fitScore: 60,
        engagementScore: 80,
        createdAt: '2026-01-01T00:00:00Z',
        qualificationFrameworkType: 'None',
      },
    ],
    totalCount: 1,
    page: 1,
    pageSize: 1000,
    totalPages: 1,
  },
};

jest.mock('../../services/apiClient', () => ({
  __esModule: true,
  default: {
    get: jest.fn((url: string) => {
      if (typeof url === 'string' && url.startsWith('/leads')) {
        return Promise.resolve(mockLeadsResponse);
      }
      return Promise.resolve({ data: [] });
    }),
    post: jest.fn().mockResolvedValue({ data: [] }),
    put: jest.fn().mockResolvedValue({ data: [] }),
    delete: jest.fn().mockResolvedValue({ data: [] }),
  },
}));

jest.mock('../../hooks/usePagination', () => ({
  usePagination: (data: unknown[]) => ({
    page: 0,
    pageSize: 25,
    totalCount: data.length,
    totalPages: 1,
    paginatedData: data,
    startIndex: 0,
    endIndex: data.length,
    setPage: jest.fn(),
    setPageSize: jest.fn(),
    setTotalCount: jest.fn(),
    handlePageChange: jest.fn(),
    handlePageSizeChange: jest.fn(),
    pageSizeOptions: [25],
    reset: jest.fn(),
  }),
}));

jest.mock('../../hooks/useApiState', () => ({
  useApiState: () => ({
    data: null,
    loading: false,
    error: null,
    success: null,
    setLoading: jest.fn(),
    setError: jest.fn(),
    setSuccess: jest.fn(),
    clearError: jest.fn(),
    clearSuccess: jest.fn(),
    execute: jest.fn(),
    reset: jest.fn(),
  }),
}));

const mockUseEntityTypeSubscription = jest.fn();
jest.mock('../../hooks/useSignalR', () => ({
  useEntityTypeSubscription: (...args: unknown[]) => mockUseEntityTypeSubscription(...args),
}));

jest.mock('../../contexts/ProfileContext', () => ({
  useProfile: () => ({
    profile: null,
    moduleStatus: null,
    isLoading: false,
    canAccessPage: () => true,
    canAccessMenu: () => true,
    hasPermission: () => true,
    isModuleEnabled: () => true,
    updateProfile: jest.fn(),
    refreshModuleStatus: jest.fn(),
  }),
}));

jest.mock('../../components/AdvancedSearch', () => ({
  __esModule: true,
  default: () => <div data-testid="advanced-search" />,
  filterData: (data: unknown[]) => data,
}));

jest.mock('../../components/ContactInfo', () => ({
  ContactInfoPanel: () => <div data-testid="contact-info" />,
}));

jest.mock('../../components/NotesTab', () => () => <div data-testid="notes-tab" />);

jest.mock('../../components/LookupSelect', () => () => <div data-testid="lookup-select" />);

jest.mock('../../components/EntitySelect', () => () => <div data-testid="entity-select" />);

jest.mock('../../components/leads/LeadScoreExplanationDrawer', () => () => <div data-testid="score-drawer" />);

jest.mock('../../services/logger', () => ({
  info: jest.fn(),
  warn: jest.fn(),
  error: jest.fn(),
  debug: jest.fn(),
}));

describe('LeadsPage', () => {
  beforeEach(() => {
    mockUseEntityTypeSubscription.mockClear();
  });

  it('renders the leads header', async () => {
    renderWithProviders(<LeadsPage />);

    expect(await screen.findByRole('heading', { name: /^leads$/i })).toBeInTheDocument();
  });

  it('fetches from the real /api/leads endpoint and renders lead rows using the LeadDto field names', async () => {
    renderWithProviders(<LeadsPage />);

    // companyName / email / title (not company / emailPrimary / jobTitle from the old Contact shape)
    expect(await screen.findByText('Ada Lovelace')).toBeInTheDocument();
    expect(screen.getByText('ada@example.com')).toBeInTheDocument();
    expect(screen.getByText('Analytical Engines Inc')).toBeInTheDocument();
    expect(screen.getByText('Chief Mathematician')).toBeInTheDocument();
  });

  it('subscribes to real-time updates using the "Lead" entity type, not "Contact"', async () => {
    renderWithProviders(<LeadsPage />);

    await waitFor(() => expect(mockUseEntityTypeSubscription).toHaveBeenCalled());
    expect(mockUseEntityTypeSubscription.mock.calls[0][0]).toBe('Lead');
  });
});

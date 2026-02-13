import React from 'react';
import { screen } from '@testing-library/react';
import CustomersPage from '../../pages/CustomersPage';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

jest.mock('../../services/apiClient', () => ({
  __esModule: true,
  default: {
    get: jest.fn().mockResolvedValue({ data: [] }),
    post: jest.fn().mockResolvedValue({ data: [] }),
    put: jest.fn().mockResolvedValue({ data: [] }),
    delete: jest.fn().mockResolvedValue({ data: [] }),
  },
}));

jest.mock('../../services/duplicateService', () => ({
  scanForDuplicates: jest.fn().mockResolvedValue([]),
  getPendingCandidates: jest.fn().mockResolvedValue([]),
}));

jest.mock('../../hooks/useFieldConfig', () => ({
  useFieldConfig: () => ({
    fieldConfigs: [],
    tabs: [],
    loading: false,
    error: null,
    refresh: jest.fn(),
    getTabFields: () => [],
    isFieldVisible: () => true,
  }),
  dispatchFieldConfigUpdate: jest.fn(),
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

jest.mock('../../hooks/useSignalR', () => ({
  useEntityTypeSubscription: jest.fn(),
}));

jest.mock('../../contexts/AccountContextProvider', () => ({
  useAccountContext: () => ({
    selectedAccounts: [],
    isContextActive: false,
    getAccountIds: () => [],
  }),
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

jest.mock('../../components/duplicates/DuplicateDetectionDialog', () => () => <div data-testid="duplicate-dialog" />);

jest.mock('../../components/duplicates/MergeDialog', () => () => <div data-testid="merge-dialog" />);

jest.mock('../../components/FieldRenderer', () => () => <div data-testid="field-renderer" />);

jest.mock('../../components/ImportExportButtons', () => () => <div data-testid="import-export" />);

jest.mock('../../services/logger', () => ({
  info: jest.fn(),
  warn: jest.fn(),
  error: jest.fn(),
  debug: jest.fn(),
}));

describe('CustomersPage', () => {
  it('renders the accounts header', async () => {
    renderWithProviders(<CustomersPage />);

    expect(await screen.findByRole('heading', { name: /^accounts$/i })).toBeInTheDocument();
  });
});

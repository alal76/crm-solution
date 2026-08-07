import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import AccountOverviewPage from '../../pages/AccountOverviewPage';
import { renderWithProviders } from '../../test-utils/renderWithProviders';
import apiClient from '../../services/apiClient';
import agentService from '../../services/agentService';
import { getNewsSocialStatus, getNewsSocialFeeds } from '../../services/newsSocialService';
import { contactInfoService } from '../../services/contactInfoService';

/**
 * Component tests for the "Merge with..." entry point added to AccountOverviewPage
 * (REV-FE-001). This closes the gap where merging an account was only possible from
 * AccountsPage's bulk-select toolbar — here a single account can be merged by picking
 * a target via EntitySelect and launching the same MergeDialog wizard used elsewhere.
 *
 * Note: CRA's default Jest config sets `resetMocks: true`, which resets every mock
 * function (wiping any `.mockResolvedValue()`/implementation) before each test runs.
 * So implementations must be (re)installed in `beforeEach`, not only inside the
 * `jest.mock()` factory — otherwise `apiClient.get` etc. silently resolve to
 * `undefined` on every test after the first.
 */

const mockAccounts = [
  { id: 1, company: 'Acme Corp', firstName: '', lastName: '', email: 'acme@example.com' },
  { id: 2, company: 'Beta Inc', firstName: '', lastName: '', email: 'beta@example.com' },
];

jest.mock('../../services/apiClient', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

jest.mock('../../services/agentService', () => ({
  __esModule: true,
  default: {
    getNextBestActions: jest.fn(),
    draftEmail: jest.fn(),
  },
}));

jest.mock('../../services/newsSocialService', () => ({
  getNewsSocialStatus: jest.fn(),
  getNewsSocialFeeds: jest.fn(),
  refreshNewsSocialFeeds: jest.fn(),
}));

jest.mock('../../services/contactInfoService', () => ({
  contactInfoService: {
    getAddresses: jest.fn(),
    getEmailAddresses: jest.fn(),
    getPhoneNumbers: jest.fn(),
  },
}));

jest.mock('../../components/duplicates/MergeHistoryPanel', () => () => <div data-testid="merge-history-panel" />);

jest.mock('../../components/common/ConcurrencyConflictDialog', () => () => <div data-testid="concurrency-dialog" />);

jest.mock('../../components/common/UserEditingIndicator', () => ({
  UserEditingIndicator: () => <div data-testid="user-editing-indicator" />,
}));

jest.mock('../../components/common/RecordComments', () => ({
  RecordComments: () => <div data-testid="record-comments" />,
}));

jest.mock('../../components/crm/accounts', () => ({
  AccountHierarchyTree: () => <div data-testid="account-hierarchy-tree" />,
}));

// Lightweight EntitySelect stub: exposes a button per excluded-filtered mock account
// so the test can "pick" a merge target without depending on EntitySelect's own
// internal apiClient fetch/render behavior (tested separately).
jest.mock('../../components/EntitySelect', () => ({
  __esModule: true,
  default: ({ onChange, name, excludeIds }: any) => (
    <div data-testid="entity-select-account">
      {mockAccounts
        .filter((a) => !excludeIds?.includes(a.id))
        .map((a) => (
          <button key={a.id} onClick={() => onChange({ target: { name, value: a.id } })}>
            Pick {a.company}
          </button>
        ))}
    </div>
  ),
}));

// Capture the props MergeDialog is rendered with so the test can assert the two
// records (current account + picked target) are passed through correctly.
const mockMergeDialogSpy = jest.fn();
jest.mock('../../components/duplicates/MergeDialog', () => ({
  __esModule: true,
  default: (props: any) => {
    mockMergeDialogSpy(props);
    if (!props.open) return null;
    return (
      <div data-testid="merge-dialog">
        entityType:{props.entityType} records:{props.records.map((r: any) => r.id).join(',')}
      </div>
    );
  },
}));

const mockedApiClient = apiClient as jest.Mocked<typeof apiClient>;
const mockedAgentService = agentService as jest.Mocked<typeof agentService>;
const mockedGetNewsSocialStatus = getNewsSocialStatus as jest.Mock;
const mockedGetNewsSocialFeeds = getNewsSocialFeeds as jest.Mock;
const mockedContactInfoService = contactInfoService as jest.Mocked<typeof contactInfoService>;

describe('AccountOverviewPage — Merge with...', () => {
  beforeEach(() => {
    mockedApiClient.get.mockImplementation((url: string) => {
      if (url === '/accounts') return Promise.resolve({ data: mockAccounts });
      if (url === '/users') return Promise.resolve({ data: [] });
      if (typeof url === 'string' && url.includes('/contacts')) return Promise.resolve({ data: [] });
      if (url === '/activities') return Promise.resolve({ data: { items: [] } });
      return Promise.resolve({ data: [] });
    });
    mockedApiClient.post.mockResolvedValue({ data: {} } as any);
    mockedApiClient.put.mockResolvedValue({ data: {} } as any);
    mockedApiClient.delete.mockResolvedValue({ data: {} } as any);

    mockedAgentService.getNextBestActions.mockResolvedValue({ data: {} } as any);
    mockedAgentService.draftEmail.mockResolvedValue({ data: {} } as any);

    mockedGetNewsSocialStatus.mockResolvedValue({ newsApiConfigured: false, socialApiConfigured: false });
    mockedGetNewsSocialFeeds.mockResolvedValue({ newsItems: [], socialFeeds: [] });

    mockedContactInfoService.getAddresses.mockResolvedValue({ data: [] } as any);
    mockedContactInfoService.getEmailAddresses.mockResolvedValue({ data: [] } as any);
    mockedContactInfoService.getPhoneNumbers.mockResolvedValue({ data: [] } as any);
  });

  async function selectFirstAccount(user: ReturnType<typeof userEvent.setup>) {
    renderWithProviders(<AccountOverviewPage />);
    const item = await screen.findByText('Acme Corp');
    await user.click(item);
    // Detail panel header repeats the company name; wait for it to render.
    await waitFor(() => {
      expect(screen.getAllByText('Acme Corp').length).toBeGreaterThan(1);
    });
  }

  it('shows a "Merge with..." action once an account is selected', async () => {
    const user = userEvent.setup();
    await selectFirstAccount(user);

    expect(screen.getByRole('button', { name: /merge with/i })).toBeInTheDocument();
  });

  it('opens the account picker when "Merge with..." is clicked', async () => {
    const user = userEvent.setup();
    await selectFirstAccount(user);

    await user.click(screen.getByRole('button', { name: /merge with/i }));

    expect(screen.getByText('Merge with Another Account')).toBeInTheDocument();
    expect(screen.getByTestId('entity-select-account')).toBeInTheDocument();
    // The currently-viewed account must not be offered as its own merge target.
    expect(screen.queryByRole('button', { name: /pick acme corp/i })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /pick beta inc/i })).toBeInTheDocument();
  });

  it('launches MergeDialog with the current account and the picked account as records', async () => {
    const user = userEvent.setup();
    await selectFirstAccount(user);

    await user.click(screen.getByRole('button', { name: /merge with/i }));
    await user.click(screen.getByRole('button', { name: /pick beta inc/i }));
    await user.click(screen.getByRole('button', { name: /continue/i }));

    const dialog = await screen.findByTestId('merge-dialog');
    expect(dialog).toHaveTextContent('entityType:Account');
    expect(dialog).toHaveTextContent('records:1,2');
  });
});

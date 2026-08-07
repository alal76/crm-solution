import React from 'react';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { renderWithProviders } from '../../../../test-utils/renderWithProviders';
import AccountHierarchyTree, { buildAccountHierarchy } from '../AccountHierarchyTree';
import accountService from '../../../../services/accountService';
import type { Account } from '../../../../types';

/**
 * Component tests for AccountHierarchyTree (REV-FE-001).
 * Verifies the flat account list is grouped into a parent -> children hierarchy
 * via Account.parentAccountId (there is no ChildAccounts navigation property on
 * the backend Account entity, so this grouping must happen client-side), that the
 * tree renders using SimpleTreeView/TreeItem, and that clicking a node either
 * navigates to the account detail route or invokes a provided onSelectAccount callback.
 */

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

jest.mock('../../../../services/accountService', () => ({
  __esModule: true,
  default: {
    getAll: jest.fn(),
  },
}));

const mockedAccountService = accountService as jest.Mocked<typeof accountService>;

const mockAccounts: Partial<Account>[] = [
  { id: 1, company: 'Parent Co' },
  { id: 2, company: 'Child Co', parentAccountId: 1 },
  { id: 3, company: 'Grandchild Co', parentAccountId: 2 },
  { id: 4, company: 'Unrelated Co' },
];

describe('buildAccountHierarchy', () => {
  it('groups a flat account list into a parent -> children tree via parentAccountId', () => {
    const tree = buildAccountHierarchy(mockAccounts as Account[]);

    expect(tree.map((n) => n.name).sort()).toEqual(['Parent Co', 'Unrelated Co']);

    const parent = tree.find((n) => n.name === 'Parent Co');
    expect(parent?.children).toHaveLength(1);
    expect(parent?.children[0].name).toBe('Child Co');
    expect(parent?.children[0].children[0].name).toBe('Grandchild Co');
  });

  it('treats accounts with a dangling/self parentAccountId as roots', () => {
    const tree = buildAccountHierarchy([
      { id: 5, company: 'Orphan Co', parentAccountId: 999 } as Account,
      { id: 6, company: 'Self Parent Co', parentAccountId: 6 } as Account,
    ]);
    expect(tree.map((n) => n.name).sort()).toEqual(['Orphan Co', 'Self Parent Co']);
  });
});

describe('AccountHierarchyTree', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders a tree from provided account data with parent/child relationships', () => {
    renderWithProviders(<AccountHierarchyTree accounts={mockAccounts as Account[]} />);

    expect(screen.getByText('Parent Co')).toBeInTheDocument();
    expect(screen.getByText('Child Co')).toBeInTheDocument();
    expect(screen.getByText('Grandchild Co')).toBeInTheDocument();
    expect(screen.getByText('Unrelated Co')).toBeInTheDocument();
  });

  it('fetches all accounts itself when no accounts prop is given', async () => {
    mockedAccountService.getAll.mockResolvedValue({ data: mockAccounts } as any);

    renderWithProviders(<AccountHierarchyTree />);

    expect(await screen.findByText('Parent Co')).toBeInTheDocument();
    expect(mockedAccountService.getAll).toHaveBeenCalledTimes(1);
  });

  it('navigates to the account detail route when a node is clicked and no onSelectAccount is provided', async () => {
    const user = userEvent.setup();
    renderWithProviders(<AccountHierarchyTree accounts={mockAccounts as Account[]} />);

    await user.click(screen.getByText('Parent Co'));

    expect(mockNavigate).toHaveBeenCalledWith('/accounts/1');
  });

  it('invokes onSelectAccount instead of navigating when provided', async () => {
    const user = userEvent.setup();
    const onSelectAccount = jest.fn();
    renderWithProviders(
      <AccountHierarchyTree accounts={mockAccounts as Account[]} onSelectAccount={onSelectAccount} />
    );

    await user.click(screen.getByText('Child Co'));

    expect(onSelectAccount).toHaveBeenCalledWith(2);
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('restricts the rendered tree to the subtree rooted at rootAccountId', () => {
    renderWithProviders(<AccountHierarchyTree accounts={mockAccounts as Account[]} rootAccountId={1} />);

    expect(screen.getByText('Parent Co')).toBeInTheDocument();
    expect(screen.getByText('Child Co')).toBeInTheDocument();
    expect(screen.getByText('Grandchild Co')).toBeInTheDocument();
    expect(screen.queryByText('Unrelated Co')).not.toBeInTheDocument();
  });

  it('shows an empty state message when there is nothing to display', () => {
    renderWithProviders(<AccountHierarchyTree accounts={[]} />);

    expect(screen.getByText(/no account hierarchy to display/i)).toBeInTheDocument();
  });
});

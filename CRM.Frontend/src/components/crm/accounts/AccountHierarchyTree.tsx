/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the Source-Available License (see LICENSE) v3.0
 */
// REV-FE-001: Account Hierarchy Tree — visualizes parent/child account relationships.
// Account.cs has no ChildAccounts navigation property, so the hierarchy is built
// client-side from the flat account list via Account.parentAccountId.

import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Box, CircularProgress, Typography } from '@mui/material';
import { Business as BusinessIcon } from '@mui/icons-material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view'; // v8 API, matches KnowledgeCategoryManagementPage
import accountService from '../../../services/accountService';
import type { Account } from '../../../types';

export interface AccountTreeNode {
  id: number;
  name: string;
  children: AccountTreeNode[];
}

export interface AccountHierarchyTreeProps {
  /** Pre-loaded accounts to build the tree from. If omitted, the component fetches all accounts itself. */
  accounts?: Account[];
  /** When set, restricts the rendered tree to this account and its descendants only. */
  rootAccountId?: number;
  /** Called when a node is clicked with the clicked account's id. Defaults to navigating to `/accounts/:id`. */
  onSelectAccount?: (accountId: number) => void;
  /** Id of the account to highlight as selected in the tree. */
  selectedAccountId?: number;
}

function getAccountDisplayName(account: Account): string {
  return (
    account.company ||
    account.displayName ||
    `${account.firstName || ''} ${account.lastName || ''}`.trim() ||
    `Account #${account.id}`
  );
}

/** Build a parent -> children hierarchy from a flat account list via parentAccountId. */
export function buildAccountHierarchy(accounts: Account[]): AccountTreeNode[] {
  const map = new Map<number, AccountTreeNode>();
  for (const acc of accounts) {
    map.set(acc.id, { id: acc.id, name: getAccountDisplayName(acc), children: [] });
  }

  const roots: AccountTreeNode[] = [];
  for (const acc of accounts) {
    const node = map.get(acc.id);
    if (!node) continue;
    const parentId = acc.parentAccountId;
    if (parentId != null && parentId !== acc.id && map.has(parentId)) {
      map.get(parentId)!.children.push(node);
    } else {
      roots.push(node);
    }
  }

  const sortNodes = (nodes: AccountTreeNode[]) => {
    nodes.sort((a, b) => a.name.localeCompare(b.name));
    nodes.forEach((n) => sortNodes(n.children));
  };
  sortNodes(roots);
  return roots;
}

/** Find the subtree rooted at `rootId` within a built hierarchy (searches all branches). */
function findSubtree(nodes: AccountTreeNode[], rootId: number): AccountTreeNode | null {
  for (const node of nodes) {
    if (node.id === rootId) return node;
    const found = findSubtree(node.children, rootId);
    if (found) return found;
  }
  return null;
}

function AccountTreeNodes({ nodes }: { nodes: AccountTreeNode[] }) {
  return (
    <>
      {nodes.map((node) => (
        <TreeItem
          key={String(node.id)}
          itemId={String(node.id)}
          label={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, py: 0.25 }}>
              <BusinessIcon fontSize="small" sx={{ opacity: 0.6 }} />
              <Typography variant="body2" noWrap sx={{ flex: 1 }}>
                {node.name}
              </Typography>
            </Box>
          }
        >
          {node.children.length > 0 && <AccountTreeNodes nodes={node.children} />}
        </TreeItem>
      ))}
    </>
  );
}

const AccountHierarchyTree: React.FC<AccountHierarchyTreeProps> = ({
  accounts: accountsProp,
  rootAccountId,
  onSelectAccount,
  selectedAccountId,
}) => {
  const navigate = useNavigate();
  const [fetchedAccounts, setFetchedAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(accountsProp === undefined);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (accountsProp !== undefined) return;
    let cancelled = false;
    setLoading(true);
    setError(null);
    accountService
      .getAll()
      .then((res) => {
        if (!cancelled) setFetchedAccounts(res.data || []);
      })
      .catch(() => {
        if (!cancelled) setError('Failed to load account hierarchy');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accountsProp]);

  const accounts = accountsProp ?? fetchedAccounts;

  const tree = useMemo(() => {
    const fullTree = buildAccountHierarchy(accounts);
    if (rootAccountId == null) return fullTree;
    const subtree = findSubtree(fullTree, rootAccountId);
    return subtree ? [subtree] : [];
  }, [accounts, rootAccountId]);

  // Account hierarchies are typically small (a handful of subsidiaries/branches), so
  // default to fully expanded — that's more useful here than KnowledgeCategoryManagementPage's
  // collapsed-by-default tree, since the point of this view is to see the whole family at a glance.
  const allNodeIds = useMemo(() => {
    const ids: string[] = [];
    const collect = (nodes: AccountTreeNode[]) => {
      for (const n of nodes) {
        ids.push(String(n.id));
        collect(n.children);
      }
    };
    collect(tree);
    return ids;
  }, [tree]);

  const handleSelect = useCallback(
    (_event: React.SyntheticEvent | null, itemId: string | null) => {
      if (itemId === null) return;
      const id = parseInt(itemId, 10);
      if (Number.isNaN(id)) return;
      if (onSelectAccount) {
        onSelectAccount(id);
      } else {
        navigate(`/accounts/${id}`);
      }
    },
    [onSelectAccount, navigate]
  );

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={2}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  if (tree.length === 0) {
    return (
      <Typography color="text.secondary" variant="body2" sx={{ p: 2, textAlign: 'center' }}>
        No account hierarchy to display.
      </Typography>
    );
  }

  return (
    <SimpleTreeView
      selectedItems={selectedAccountId != null ? String(selectedAccountId) : null}
      onSelectedItemsChange={handleSelect}
      defaultExpandedItems={allNodeIds}
      aria-label="account hierarchy"
    >
      <AccountTreeNodes nodes={tree} />
    </SimpleTreeView>
  );
};

export default AccountHierarchyTree;
export { AccountHierarchyTree };

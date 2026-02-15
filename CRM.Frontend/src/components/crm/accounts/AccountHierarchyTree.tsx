import React, { useState, useEffect } from 'react';
import {
  Box,
  CircularProgress,
  Alert,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Select,
  MenuItem,
  FormControl,
  FormLabel,
  Stack,
  Typography,
  IconButton,
  Tooltip,
} from '@mui/material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import EditIcon from '@mui/icons-material/Edit';
import LinkIcon from '@mui/icons-material/Link';
import accountService from '../../../services/accountService';

interface AccountHierarchyNode {
  id: number;
  firstName?: string;
  lastName?: string;
  company?: string;
  email?: string;
  parentAccountId?: number | null;
  children: AccountHierarchyNode[];
}

interface AccountHierarchyTreeProps {
  onAccountSelect?: (accountId: number) => void;
  onNavigate?: (accountId: number) => void;
}

/**
 * AccountHierarchyTree Component
 * Displays account relationships in a tree structure with drag-drop support
 * for reassigning parent accounts.
 */
export const AccountHierarchyTree: React.FC<AccountHierarchyTreeProps> = ({
  onAccountSelect,
  onNavigate,
}) => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [hierarchyNodes, setHierarchyNodes] = useState<AccountHierarchyNode[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [expanded, setExpanded] = useState<string[]>([]);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null);
  const [newParentId, setNewParentId] = useState<number | null>(null);
  const [updating, setUpdating] = useState(false);

  interface Account {
    id: number;
    firstName?: string;
    lastName?: string;
    company?: string;
    email?: string;
    parentAccountId?: number | null;
  }

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await accountService.getAll();
      setAccounts(response);

      // Build hierarchy from flat account list
      const hierarchy = buildHierarchy(response);
      setHierarchyNodes(hierarchy);

      // Expand root nodes by default
      const rootIds = hierarchy.map(n => String(n.id));
      setExpanded(rootIds);
    } catch (err) {
      setError('Failed to load account hierarchy');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const buildHierarchy = (accounts: Account[]): AccountHierarchyNode[] => {
    const accountMap = new Map<number, AccountHierarchyNode>();

    // Create nodes for all accounts
    accounts.forEach(account => {
      accountMap.set(account.id, {
        ...account,
        children: [],
      });
    });

    // Build parent-child relationships
    const roots: AccountHierarchyNode[] = [];
    accounts.forEach(account => {
      const node = accountMap.get(account.id)!;
      if (account.parentAccountId) {
        const parent = accountMap.get(account.parentAccountId);
        if (parent) {
          parent.children.push(node);
        }
      } else {
        roots.push(node);
      }
    });

    // Sort children by name
    const sortNode = (node: AccountHierarchyNode) => {
      node.children.sort((a, b) => {
        const nameA = `${a.firstName || ''} ${a.lastName || ''}`.trim();
        const nameB = `${b.firstName || ''} ${b.lastName || ''}`.trim();
        return nameA.localeCompare(nameB);
      });
      node.children.forEach(sortNode);
    };

    roots.sort((a, b) => {
      const nameA = `${a.firstName || ''} ${a.lastName || ''}`.trim();
      const nameB = `${b.firstName || ''} ${b.lastName || ''}`.trim();
      return nameA.localeCompare(nameB);
    });

    roots.forEach(sortNode);
    return roots;
  };

  const handleToggle = (nodeId: string) => {
    setExpanded(prev =>
      prev.includes(nodeId) ? prev.filter(id => id !== nodeId) : [...prev, nodeId]
    );
  };

  const getAccountLabel = (account: AccountHierarchyNode): string => {
    const name = `${account.firstName || ''} ${account.lastName || ''}`.trim();
    const company = account.company ? ` (${account.company})` : '';
    return name || `${account.email || 'Unknown'}${company}`;
  };

  const handleEditParent = (accountId: number) => {
    setSelectedAccountId(accountId);
    const account = accounts.find(a => a.id === accountId);
    setNewParentId(account?.parentAccountId || null);
    setEditDialogOpen(true);
  };

  const handleUpdateParent = async () => {
    if (selectedAccountId === null) return;

    try {
      setUpdating(true);
      setError(null);

      const updatedAccount = {
        ...accounts.find(a => a.id === selectedAccountId)!,
        parentAccountId: newParentId,
      };

      await accountService.update(selectedAccountId, updatedAccount);

      // Reload hierarchy
      await loadAccounts();
      setEditDialogOpen(false);
    } catch (err) {
      setError('Failed to update parent account');
      console.error(err);
    } finally {
      setUpdating(false);
    }
  };

  const renderNode = (node: AccountHierarchyNode): React.ReactNode => {
    const nodeId = String(node.id);
    const hasChildren = node.children.length > 0;

    return (
      <TreeItem
        key={nodeId}
        nodeId={nodeId}
        label={
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              py: 0.5,
              cursor: 'pointer',
              '&:hover .action-buttons': {
                visibility: 'visible',
              },
            }}
            onClick={() => onAccountSelect?.(node.id)}
          >
            <Typography variant="body2">{getAccountLabel(node)}</Typography>
            <Box className="action-buttons" sx={{ visibility: 'hidden', display: 'flex', gap: 0.5 }}>
              <Tooltip title="Edit parent">
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleEditParent(node.id);
                  }}
                >
                  <EditIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Navigate to account">
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation();
                    onNavigate?.(node.id);
                  }}
                >
                  <LinkIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
          </Box>
        }
      >
        {hasChildren && node.children.map(child => renderNode(child))}
      </TreeItem>
    );
  };

  return (
    <Box sx={{ p: 2 }}>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : hierarchyNodes.length === 0 ? (
        <Alert severity="info">No accounts found</Alert>
      ) : (
        <SimpleTreeView
          defaultCollapseIcon={<ExpandMoreIcon />}
          defaultExpandIcon={<ChevronRightIcon />}
          expandedItems={expanded}
          onExpandedItemsChange={(e, nodeIds) => setExpanded(nodeIds)}
          sx={{
            flexGrow: 1,
            overflowY: 'auto',
          }}
        >
          {hierarchyNodes.map(node => renderNode(node))}
        </SimpleTreeView>
      )}

      {/* Edit Parent Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Change Parent Account</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 2 }}>
            <Typography variant="body2" color="textSecondary">
              Current parent:{' '}
              {newParentId
                ? getAccountLabel(
                    hierarchyNodes.flatMap(n => {
                      const collect: AccountHierarchyNode[] = [n];
                      const stack = [n];
                      while (stack.length > 0) {
                        const current = stack.pop()!;
                        collect.push(...current.children);
                        stack.push(...current.children);
                      }
                      return collect;
                    }).find(n => n.id === newParentId) || { id: newParentId }
                  )
                : 'None (Root)'}
            </Typography>

            <FormControl fullWidth>
              <FormLabel>Select New Parent</FormLabel>
              <Select
                value={newParentId || ''}
                onChange={(e) => setNewParentId(e.target.value ? Number(e.target.value) : null)}
                disabled={updating}
              >
                <MenuItem value="">None (Root)</MenuItem>
                {accounts
                  .filter(a => a.id !== selectedAccountId)
                  .map(account => (
                    <MenuItem key={account.id} value={account.id}>
                      {getAccountLabel(account as AccountHierarchyNode)}
                    </MenuItem>
                  ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)} disabled={updating}>
            Cancel
          </Button>
          <Button
            onClick={handleUpdateParent}
            variant="contained"
            disabled={updating}
          >
            {updating ? 'Updating...' : 'Update'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AccountHierarchyTree;

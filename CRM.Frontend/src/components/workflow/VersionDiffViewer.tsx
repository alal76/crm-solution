/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Version Diff Viewer - Compare workflow versions side by side
 */

import React, { useState, useEffect, useMemo } from 'react';
import {
  Box,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Chip,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  IconButton,
  Tabs,
  Tab,
  Alert,
} from '@mui/material';
import {
  Compare as CompareIcon,
  Add as AddIcon,
  Remove as RemoveIcon,
  Edit as EditIcon,
  Close as CloseIcon,
  SwapHoriz as SwapIcon,
} from '@mui/icons-material';
import { WorkflowVersionDetail, WorkflowNode, WorkflowTransition } from '../../services/workflowService';

// ============================================================================
// Types
// ============================================================================

interface VersionDiffViewerProps {
  open: boolean;
  onClose: () => void;
  versions: { id: number; versionNumber: number; label?: string; status: string; createdAt: string }[];
  loadVersion: (versionId: number) => Promise<WorkflowVersionDetail>;
}

interface DiffChange {
  type: 'added' | 'removed' | 'modified' | 'unchanged';
  path: string;
  oldValue?: any;
  newValue?: any;
  description: string;
}

interface NodeDiff {
  node: WorkflowNode;
  status: 'added' | 'removed' | 'modified' | 'unchanged';
  changes?: string[];
}

interface TransitionDiff {
  transition: WorkflowTransition;
  status: 'added' | 'removed' | 'modified' | 'unchanged';
  changes?: string[];
}

// ============================================================================
// Diff Utilities
// ============================================================================

const computeNodeDiffs = (
  oldNodes: WorkflowNode[],
  newNodes: WorkflowNode[]
): NodeDiff[] => {
  const diffs: NodeDiff[] = [];
  const oldNodeMap = new Map(oldNodes.map(n => [n.nodeKey, n]));
  const newNodeMap = new Map(newNodes.map(n => [n.nodeKey, n]));

  // Check for added and modified nodes
  for (const newNode of newNodes) {
    const oldNode = oldNodeMap.get(newNode.nodeKey);
    if (!oldNode) {
      diffs.push({ node: newNode, status: 'added' });
    } else {
      const changes: string[] = [];
      if (oldNode.name !== newNode.name) changes.push(`Name: "${oldNode.name}" → "${newNode.name}"`);
      if (oldNode.nodeType !== newNode.nodeType) changes.push(`Type: ${oldNode.nodeType} → ${newNode.nodeType}`);
      if (oldNode.configuration !== newNode.configuration) changes.push('Configuration changed');
      if (oldNode.positionX !== newNode.positionX || oldNode.positionY !== newNode.positionY) changes.push('Position changed');
      if (oldNode.timeoutMinutes !== newNode.timeoutMinutes) changes.push(`Timeout: ${oldNode.timeoutMinutes} → ${newNode.timeoutMinutes}`);
      if (oldNode.retryCount !== newNode.retryCount) changes.push(`Retries: ${oldNode.retryCount} → ${newNode.retryCount}`);

      if (changes.length > 0) {
        diffs.push({ node: newNode, status: 'modified', changes });
      } else {
        diffs.push({ node: newNode, status: 'unchanged' });
      }
    }
  }

  // Check for removed nodes
  for (const oldNode of oldNodes) {
    if (!newNodeMap.has(oldNode.nodeKey)) {
      diffs.push({ node: oldNode, status: 'removed' });
    }
  }

  return diffs;
};

const computeTransitionDiffs = (
  oldTransitions: WorkflowTransition[],
  newTransitions: WorkflowTransition[]
): TransitionDiff[] => {
  const diffs: TransitionDiff[] = [];
  const oldTransMap = new Map(oldTransitions.map(t => [t.transitionKey, t]));
  const newTransMap = new Map(newTransitions.map(t => [t.transitionKey, t]));

  for (const newTrans of newTransitions) {
    const oldTrans = oldTransMap.get(newTrans.transitionKey);
    if (!oldTrans) {
      diffs.push({ transition: newTrans, status: 'added' });
    } else {
      const changes: string[] = [];
      if (oldTrans.label !== newTrans.label) changes.push(`Label: "${oldTrans.label || ''}" → "${newTrans.label || ''}"`);
      if (oldTrans.conditionType !== newTrans.conditionType) changes.push(`Condition Type: ${oldTrans.conditionType} → ${newTrans.conditionType}`);
      if (oldTrans.conditionExpression !== newTrans.conditionExpression) changes.push('Condition expression changed');
      if (oldTrans.isDefault !== newTrans.isDefault) changes.push(`Default: ${oldTrans.isDefault} → ${newTrans.isDefault}`);

      if (changes.length > 0) {
        diffs.push({ transition: newTrans, status: 'modified', changes });
      } else {
        diffs.push({ transition: newTrans, status: 'unchanged' });
      }
    }
  }

  for (const oldTrans of oldTransitions) {
    if (!newTransMap.has(oldTrans.transitionKey)) {
      diffs.push({ transition: oldTrans, status: 'removed' });
    }
  }

  return diffs;
};

// ============================================================================
// Status Chip Component
// ============================================================================

const StatusChip: React.FC<{ status: 'added' | 'removed' | 'modified' | 'unchanged' }> = ({ status }) => {
  const config = {
    added: { icon: <AddIcon fontSize="small" />, color: 'success' as const, label: 'Added' },
    removed: { icon: <RemoveIcon fontSize="small" />, color: 'error' as const, label: 'Removed' },
    modified: { icon: <EditIcon fontSize="small" />, color: 'warning' as const, label: 'Modified' },
    unchanged: { icon: null, color: 'default' as const, label: 'Unchanged' },
  };

  const { icon, color, label } = config[status];

  if (status === 'unchanged') return null;

  return (
    <Chip
      icon={icon || undefined}
      label={label}
      color={color}
      size="small"
      sx={{ height: 22, fontSize: 11 }}
    />
  );
};

// ============================================================================
// Main Component
// ============================================================================

export const VersionDiffViewer: React.FC<VersionDiffViewerProps> = ({
  open,
  onClose,
  versions,
  loadVersion,
}) => {
  const [leftVersionId, setLeftVersionId] = useState<number | ''>('');
  const [rightVersionId, setRightVersionId] = useState<number | ''>('');
  const [leftVersion, setLeftVersion] = useState<WorkflowVersionDetail | null>(null);
  const [rightVersion, setRightVersion] = useState<WorkflowVersionDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [tabValue, setTabValue] = useState(0);

  // Load versions when selection changes
  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const [left, right] = await Promise.all([
          leftVersionId ? loadVersion(leftVersionId as number) : null,
          rightVersionId ? loadVersion(rightVersionId as number) : null,
        ]);
        setLeftVersion(left);
        setRightVersion(right);
      } catch (err: any) {
        setError(err.message || 'Failed to load versions');
      } finally {
        setLoading(false);
      }
    };

    if (leftVersionId || rightVersionId) {
      load();
    }
  }, [leftVersionId, rightVersionId, loadVersion]);

  // Set default versions when dialog opens
  useEffect(() => {
    if (open && versions.length >= 2) {
      setLeftVersionId(versions[1].id); // Previous version
      setRightVersionId(versions[0].id); // Latest version
    }
  }, [open, versions]);

  // Compute diffs
  const nodeDiffs = useMemo(() => {
    if (!leftVersion || !rightVersion) return [];
    return computeNodeDiffs(leftVersion.nodes, rightVersion.nodes);
  }, [leftVersion, rightVersion]);

  const transitionDiffs = useMemo(() => {
    if (!leftVersion || !rightVersion) return [];
    return computeTransitionDiffs(leftVersion.transitions, rightVersion.transitions);
  }, [leftVersion, rightVersion]);

  // Summary counts
  const summary = useMemo(() => ({
    nodesAdded: nodeDiffs.filter(d => d.status === 'added').length,
    nodesRemoved: nodeDiffs.filter(d => d.status === 'removed').length,
    nodesModified: nodeDiffs.filter(d => d.status === 'modified').length,
    transitionsAdded: transitionDiffs.filter(d => d.status === 'added').length,
    transitionsRemoved: transitionDiffs.filter(d => d.status === 'removed').length,
    transitionsModified: transitionDiffs.filter(d => d.status === 'modified').length,
  }), [nodeDiffs, transitionDiffs]);

  const handleSwapVersions = () => {
    const temp = leftVersionId;
    setLeftVersionId(rightVersionId);
    setRightVersionId(temp);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <CompareIcon />
          <Typography variant="h6">Compare Versions</Typography>
          <Box sx={{ flex: 1 }} />
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {/* Version Selectors */}
        <Box sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'center' }}>
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>From Version</InputLabel>
            <Select
              value={leftVersionId}
              label="From Version"
              onChange={(e) => setLeftVersionId(e.target.value as number)}
            >
              {versions.map((v) => (
                <MenuItem key={v.id} value={v.id}>
                  v{v.versionNumber} {v.label && `(${v.label})`} - {v.status}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <IconButton onClick={handleSwapVersions} title="Swap versions">
            <SwapIcon />
          </IconButton>

          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>To Version</InputLabel>
            <Select
              value={rightVersionId}
              label="To Version"
              onChange={(e) => setRightVersionId(e.target.value as number)}
            >
              {versions.map((v) => (
                <MenuItem key={v.id} value={v.id}>
                  v{v.versionNumber} {v.label && `(${v.label})`} - {v.status}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
        )}

        {leftVersion && rightVersion && (
          <>
            {/* Summary */}
            <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
              <Typography variant="subtitle2" gutterBottom>Change Summary</Typography>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Chip
                  icon={<AddIcon />}
                  label={`${summary.nodesAdded} nodes added`}
                  color="success"
                  variant="outlined"
                  size="small"
                />
                <Chip
                  icon={<RemoveIcon />}
                  label={`${summary.nodesRemoved} nodes removed`}
                  color="error"
                  variant="outlined"
                  size="small"
                />
                <Chip
                  icon={<EditIcon />}
                  label={`${summary.nodesModified} nodes modified`}
                  color="warning"
                  variant="outlined"
                  size="small"
                />
                <Divider orientation="vertical" flexItem />
                <Chip
                  icon={<AddIcon />}
                  label={`${summary.transitionsAdded} transitions added`}
                  color="success"
                  variant="outlined"
                  size="small"
                />
                <Chip
                  icon={<RemoveIcon />}
                  label={`${summary.transitionsRemoved} transitions removed`}
                  color="error"
                  variant="outlined"
                  size="small"
                />
                <Chip
                  icon={<EditIcon />}
                  label={`${summary.transitionsModified} transitions modified`}
                  color="warning"
                  variant="outlined"
                  size="small"
                />
              </Box>
            </Paper>

            {/* Tabs */}
            <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
              <Tab label={`Nodes (${nodeDiffs.filter(d => d.status !== 'unchanged').length} changes)`} />
              <Tab label={`Transitions (${transitionDiffs.filter(d => d.status !== 'unchanged').length} changes)`} />
              <Tab label="Raw JSON" />
            </Tabs>

            {/* Nodes Tab */}
            {tabValue === 0 && (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Node Key</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Changes</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {nodeDiffs
                    .filter(d => d.status !== 'unchanged')
                    .map((diff) => (
                      <TableRow
                        key={diff.node.nodeKey}
                        sx={{
                          backgroundColor:
                            diff.status === 'added' ? 'success.lighter' :
                            diff.status === 'removed' ? 'error.lighter' :
                            diff.status === 'modified' ? 'warning.lighter' : 'inherit',
                        }}
                      >
                        <TableCell>
                          <Typography variant="body2" fontFamily="monospace">
                            {diff.node.nodeKey}
                          </Typography>
                        </TableCell>
                        <TableCell>{diff.node.name}</TableCell>
                        <TableCell>
                          <Chip label={diff.node.nodeType} size="small" />
                        </TableCell>
                        <TableCell>
                          <StatusChip status={diff.status} />
                        </TableCell>
                        <TableCell>
                          {diff.changes?.map((change, i) => (
                            <Typography key={i} variant="caption" display="block" color="text.secondary">
                              • {change}
                            </Typography>
                          ))}
                        </TableCell>
                      </TableRow>
                    ))}
                  {nodeDiffs.filter(d => d.status !== 'unchanged').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography color="text.secondary">No node changes</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            )}

            {/* Transitions Tab */}
            {tabValue === 1 && (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Transition Key</TableCell>
                    <TableCell>Label</TableCell>
                    <TableCell>Condition</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Changes</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transitionDiffs
                    .filter(d => d.status !== 'unchanged')
                    .map((diff) => (
                      <TableRow
                        key={diff.transition.transitionKey}
                        sx={{
                          backgroundColor:
                            diff.status === 'added' ? 'success.lighter' :
                            diff.status === 'removed' ? 'error.lighter' :
                            diff.status === 'modified' ? 'warning.lighter' : 'inherit',
                        }}
                      >
                        <TableCell>
                          <Typography variant="body2" fontFamily="monospace">
                            {diff.transition.transitionKey}
                          </Typography>
                        </TableCell>
                        <TableCell>{diff.transition.label || '-'}</TableCell>
                        <TableCell>
                          <Chip label={diff.transition.conditionType} size="small" />
                        </TableCell>
                        <TableCell>
                          <StatusChip status={diff.status} />
                        </TableCell>
                        <TableCell>
                          {diff.changes?.map((change, i) => (
                            <Typography key={i} variant="caption" display="block" color="text.secondary">
                              • {change}
                            </Typography>
                          ))}
                        </TableCell>
                      </TableRow>
                    ))}
                  {transitionDiffs.filter(d => d.status !== 'unchanged').length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center">
                        <Typography color="text.secondary">No transition changes</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            )}

            {/* Raw JSON Tab */}
            {tabValue === 2 && (
              <Box sx={{ display: 'flex', gap: 2 }}>
                <Paper
                  variant="outlined"
                  sx={{ flex: 1, p: 1, maxHeight: 400, overflow: 'auto' }}
                >
                  <Typography variant="caption" color="text.secondary">
                    v{leftVersion.versionNumber}
                  </Typography>
                  <pre style={{ fontSize: 11, margin: 0 }}>
                    {JSON.stringify({ nodes: leftVersion.nodes, transitions: leftVersion.transitions }, null, 2)}
                  </pre>
                </Paper>
                <Paper
                  variant="outlined"
                  sx={{ flex: 1, p: 1, maxHeight: 400, overflow: 'auto' }}
                >
                  <Typography variant="caption" color="text.secondary">
                    v{rightVersion.versionNumber}
                  </Typography>
                  <pre style={{ fontSize: 11, margin: 0 }}>
                    {JSON.stringify({ nodes: rightVersion.nodes, transitions: rightVersion.transitions }, null, 2)}
                  </pre>
                </Paper>
              </Box>
            )}
          </>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default VersionDiffViewer;

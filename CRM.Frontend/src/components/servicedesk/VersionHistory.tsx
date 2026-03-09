/**
 * CRM Solution - Knowledge Article Version History Component
 *
 * Displays version history for a knowledge article with:
 * - Version list showing all historical versions
 * - Diff viewer to compare versions
 * - Restore functionality for previous versions
 *
 * TODO-SD002-006: VersionHistory.tsx component
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Tooltip,
  CircularProgress,
  Alert,
  Divider,
  Stack,
  useTheme,
} from '@mui/material';
import {
  History as HistoryIcon,
  Restore as RestoreIcon,
  Compare as CompareIcon,
  Visibility as ViewIcon,
  Close as CloseIcon,
  ArrowBack as ArrowBackIcon,
  Add as AddIcon,
  Remove as RemoveIcon,
} from '@mui/icons-material';

// Types for article version
interface ArticleVersion {
  id: number;
  articleId: number;
  versionNumber: number;
  title: string;
  content: string;
  changedById: number;
  changedByName?: string;
  changedAt: string;
  changeNote?: string;
}

interface VersionHistoryProps {
  articleId: number;
  currentVersion?: number;
  onRestore?: (versionId: number) => void;
  onClose?: () => void;
  readOnly?: boolean;
}

interface DiffLine {
  type: 'added' | 'removed' | 'unchanged';
  content: string;
  lineNumber?: number;
}

// Simple diff algorithm
const computeDiff = (oldText: string, newText: string): DiffLine[] => {
  const oldLines = oldText.split('\n');
  const newLines = newText.split('\n');
  const result: DiffLine[] = [];

  // Simple line-by-line diff (for better diff, use a library like diff-match-patch)
  let oldIndex = 0;
  let newIndex = 0;

  while (oldIndex < oldLines.length || newIndex < newLines.length) {
    if (oldIndex >= oldLines.length) {
      // Remaining new lines are additions
      result.push({ type: 'added', content: newLines[newIndex], lineNumber: newIndex + 1 });
      newIndex++;
    } else if (newIndex >= newLines.length) {
      // Remaining old lines are removals
      result.push({ type: 'removed', content: oldLines[oldIndex], lineNumber: oldIndex + 1 });
      oldIndex++;
    } else if (oldLines[oldIndex] === newLines[newIndex]) {
      // Lines match
      result.push({ type: 'unchanged', content: oldLines[oldIndex], lineNumber: newIndex + 1 });
      oldIndex++;
      newIndex++;
    } else {
      // Lines differ - mark as removed then added
      result.push({ type: 'removed', content: oldLines[oldIndex], lineNumber: oldIndex + 1 });
      result.push({ type: 'added', content: newLines[newIndex], lineNumber: newIndex + 1 });
      oldIndex++;
      newIndex++;
    }
  }

  return result;
};

const VersionHistory: React.FC<VersionHistoryProps> = ({
  articleId,
  currentVersion,
  onRestore,
  onClose,
  readOnly = false,
}) => {
  const theme = useTheme();
  const [versions, setVersions] = useState<ArticleVersion[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedVersion, setSelectedVersion] = useState<ArticleVersion | null>(null);
  const [compareFrom, setCompareFrom] = useState<ArticleVersion | null>(null);
  const [compareTo, setCompareTo] = useState<ArticleVersion | null>(null);
  const [showCompareDialog, setShowCompareDialog] = useState(false);
  const [showViewDialog, setShowViewDialog] = useState(false);
  const [showRestoreDialog, setShowRestoreDialog] = useState(false);
  const [restoring, setRestoring] = useState(false);

  const fetchVersions = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/knowledgearticles/${articleId}/versions`);
      if (!response.ok) {
        throw new Error('Failed to fetch version history');
      }
      const data = await response.json();
      setVersions(data);
    } catch (err) {
      setError(err instanceof Error ? (err as Error).message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  }, [articleId]);

  useEffect(() => {
    fetchVersions();
  }, [fetchVersions]);

  const handleViewVersion = (version: ArticleVersion) => {
    setSelectedVersion(version);
    setShowViewDialog(true);
  };

  const handleCompareClick = (version: ArticleVersion) => {
    if (!compareFrom) {
      setCompareFrom(version);
    } else if (!compareTo) {
      setCompareTo(version);
      setShowCompareDialog(true);
    }
  };

  const handleClearCompare = () => {
    setCompareFrom(null);
    setCompareTo(null);
  };

  const handleRestoreClick = (version: ArticleVersion) => {
    setSelectedVersion(version);
    setShowRestoreDialog(true);
  };

  const handleConfirmRestore = async () => {
    if (!selectedVersion) return;

    setRestoring(true);
    try {
      if (onRestore) {
        await onRestore(selectedVersion.id);
      } else {
        // Default restore API call
        const response = await fetch(`/api/knowledgearticles/${articleId}/restore/${selectedVersion.id}`, {
          method: 'POST',
        });
        if (!response.ok) {
          throw new Error('Failed to restore version');
        }
      }
      setShowRestoreDialog(false);
      fetchVersions(); // Refresh the version list
    } catch (err) {
      setError(err instanceof Error ? (err as Error).message : 'Failed to restore version');
    } finally {
      setRestoring(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const diffLines = compareFrom && compareTo ? computeDiff(compareFrom.content, compareTo.content) : [];

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Paper sx={{ p: 3 }}>
      {/* Header */}
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Stack direction="row" alignItems="center" spacing={1}>
          <HistoryIcon color="primary" />
          <Typography variant="h6">Version History</Typography>
          {currentVersion && (
            <Chip label={`Current: v${currentVersion}`} size="small" color="primary" />
          )}
        </Stack>
        <Stack direction="row" spacing={1}>
          {compareFrom && !compareTo && (
            <Button size="small" onClick={handleClearCompare} startIcon={<CloseIcon />}>
              Cancel Compare
            </Button>
          )}
          {onClose && (
            <IconButton onClick={onClose} size="small">
              <CloseIcon />
            </IconButton>
          )}
        </Stack>
      </Stack>

      {compareFrom && !compareTo && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Selected v{compareFrom.versionNumber} for comparison. Click another version to compare.
        </Alert>
      )}

      <Divider sx={{ mb: 2 }} />

      {/* Version Table */}
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Version</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Changed By</TableCell>
              <TableCell>Date</TableCell>
              <TableCell>Note</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {versions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Typography color="text.secondary">No version history available</Typography>
                </TableCell>
              </TableRow>
            ) : (
              versions.map((version) => (
                <TableRow
                  key={version.id}
                  sx={{
                    backgroundColor:
                      compareFrom?.id === version.id
                        ? theme.palette.action.selected
                        : version.versionNumber === currentVersion
                        ? theme.palette.action.hover
                        : 'inherit',
                  }}
                >
                  <TableCell>
                    <Chip
                      label={`v${version.versionNumber}`}
                      size="small"
                      color={version.versionNumber === currentVersion ? 'primary' : 'default'}
                    />
                  </TableCell>
                  <TableCell>{version.title}</TableCell>
                  <TableCell>{version.changedByName || `User ${version.changedById}`}</TableCell>
                  <TableCell>{formatDate(version.changedAt)}</TableCell>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                      {version.changeNote || '-'}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Stack direction="row" spacing={0.5} justifyContent="flex-end">
                      <Tooltip title="View">
                        <IconButton size="small" onClick={() => handleViewVersion(version)}>
                          <ViewIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title={compareFrom ? 'Compare with selected' : 'Select to compare'}>
                        <IconButton
                          size="small"
                          onClick={() => handleCompareClick(version)}
                          color={compareFrom?.id === version.id ? 'primary' : 'default'}
                        >
                          <CompareIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      {!readOnly && version.versionNumber !== currentVersion && (
                        <Tooltip title="Restore this version">
                          <IconButton size="small" onClick={() => handleRestoreClick(version)}>
                            <RestoreIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )}
                    </Stack>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* View Version Dialog */}
      <Dialog open={showViewDialog} onClose={() => setShowViewDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">
              Version {selectedVersion?.versionNumber} - {selectedVersion?.title}
            </Typography>
            <IconButton onClick={() => setShowViewDialog(false)}>
              <CloseIcon />
            </IconButton>
          </Stack>
        </DialogTitle>
        <DialogContent>
          {selectedVersion && (
            <Box>
              <Typography variant="caption" color="text.secondary" gutterBottom>
                Changed by {selectedVersion.changedByName || `User ${selectedVersion.changedById}`} on{' '}
                {formatDate(selectedVersion.changedAt)}
                {selectedVersion.changeNote && ` - ${selectedVersion.changeNote}`}
              </Typography>
              <Divider sx={{ my: 2 }} />
              <Paper
                variant="outlined"
                sx={{
                  p: 2,
                  maxHeight: 400,
                  overflow: 'auto',
                  backgroundColor: theme.palette.grey[50],
                  fontFamily: 'monospace',
                  whiteSpace: 'pre-wrap',
                }}
              >
                {selectedVersion.content}
              </Paper>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowViewDialog(false)}>Close</Button>
          {!readOnly && selectedVersion && selectedVersion.versionNumber !== currentVersion && (
            <Button
              variant="contained"
              startIcon={<RestoreIcon />}
              onClick={() => {
                setShowViewDialog(false);
                setShowRestoreDialog(true);
              }}
            >
              Restore This Version
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Compare Dialog */}
      <Dialog
        open={showCompareDialog}
        onClose={() => {
          setShowCompareDialog(false);
          handleClearCompare();
        }}
        maxWidth="lg"
        fullWidth
      >
        <DialogTitle>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">
              Compare v{compareFrom?.versionNumber} → v{compareTo?.versionNumber}
            </Typography>
            <IconButton
              onClick={() => {
                setShowCompareDialog(false);
                handleClearCompare();
              }}
            >
              <CloseIcon />
            </IconButton>
          </Stack>
        </DialogTitle>
        <DialogContent>
          <Paper
            variant="outlined"
            sx={{
              p: 2,
              maxHeight: 500,
              overflow: 'auto',
              backgroundColor: theme.palette.grey[50],
              fontFamily: 'monospace',
              fontSize: '0.85rem',
            }}
          >
            {diffLines.map((line, index) => (
              <Box
                key={index}
                sx={{
                  display: 'flex',
                  alignItems: 'flex-start',
                  backgroundColor:
                    line.type === 'added'
                      ? theme.palette.success.light + '40'
                      : line.type === 'removed'
                      ? theme.palette.error.light + '40'
                      : 'transparent',
                  borderLeft: `3px solid ${
                    line.type === 'added'
                      ? theme.palette.success.main
                      : line.type === 'removed'
                      ? theme.palette.error.main
                      : 'transparent'
                  }`,
                  px: 1,
                  py: 0.25,
                }}
              >
                <Box sx={{ width: 24, mr: 1, color: 'text.secondary' }}>
                  {line.type === 'added' ? (
                    <AddIcon fontSize="small" color="success" />
                  ) : line.type === 'removed' ? (
                    <RemoveIcon fontSize="small" color="error" />
                  ) : null}
                </Box>
                <Typography
                  component="pre"
                  sx={{
                    m: 0,
                    whiteSpace: 'pre-wrap',
                    wordBreak: 'break-word',
                    textDecoration: line.type === 'removed' ? 'line-through' : 'none',
                  }}
                >
                  {line.content || ' '}
                </Typography>
              </Box>
            ))}
          </Paper>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              setShowCompareDialog(false);
              handleClearCompare();
            }}
          >
            Close
          </Button>
        </DialogActions>
      </Dialog>

      {/* Restore Confirmation Dialog */}
      <Dialog open={showRestoreDialog} onClose={() => setShowRestoreDialog(false)}>
        <DialogTitle>Restore Version</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to restore version {selectedVersion?.versionNumber}? This will create a new version
            with the content from the selected version.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowRestoreDialog(false)} disabled={restoring}>
            Cancel
          </Button>
          <Button
            variant="contained"
            color="primary"
            onClick={handleConfirmRestore}
            disabled={restoring}
            startIcon={restoring ? <CircularProgress size={16} /> : <RestoreIcon />}
          >
            {restoring ? 'Restoring...' : 'Restore'}
          </Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
};

export default VersionHistory;

import React, { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import ShareIcon from '@mui/icons-material/Share';
import apiClient from '../../services/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

type SharePermission = 'View' | 'Edit' | 'Admin';

interface ShareInfo {
  shareId: number;
  userId: number;
  userEmail?: string;
  userName?: string;
  permission: string;
  sharedByUserName?: string;
  sharedAt: string;
}

interface Props {
  /** Whether the dialog is open */
  open: boolean;
  /** The numeric ID of the report being shared */
  reportId: number;
  /** Human-readable report name shown in the title */
  reportName?: string;
  /** Called when the dialog should close */
  onClose: () => void;
}

// ─── Component ────────────────────────────────────────────────────────────────

/**
 * Report Share Dialog
 *
 * Provides a management UI for sharing a saved report with other user accounts.
 * Supports View / Edit / Admin permission levels and allows revoking access.
 *
 * Calls:
 *  GET    /api/reports/{id}/shares
 *  POST   /api/reports/{id}/shares  { userIds, permission }
 *  DELETE /api/reports/{id}/shares/{userId}
 *
 * TODO-RPT-03
 */
const ReportShareDialog: React.FC<Props> = ({ open, reportId, reportName, onClose }) => {
  const [shares, setShares] = useState<ShareInfo[]>([]);
  const [loadingShares, setLoadingShares] = useState(false);
  const [userIdInput, setUserIdInput] = useState('');
  const [permission, setPermission] = useState<SharePermission>('View');
  const [sharing, setSharing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // ── Load existing shares ──────────────────────────────────────────────────

  const loadShares = async () => {
    setLoadingShares(true);
    setError(null);
    try {
      const res = await apiClient.get<ShareInfo[]>(`/reports/${reportId}/shares`);
      setShares(res.data);
    } catch {
      setError('Failed to load share information.');
    } finally {
      setLoadingShares(false);
    }
  };

  useEffect(() => {
    if (open) {
      loadShares();
      setUserIdInput('');
      setPermission('View');
      setSuccess(null);
      setError(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, reportId]);

  // ── Share with users ──────────────────────────────────────────────────────

  const handleShare = async () => {
    const ids = userIdInput
      .split(/[\s,;]+/)
      .map((v) => Number.parseInt(v.trim(), 10))
      .filter((n) => !Number.isNaN(n) && n > 0);

    if (ids.length === 0) {
      setError('Please enter at least one valid user ID.');
      return;
    }

    setSharing(true);
    setError(null);
    setSuccess(null);

    try {
      await apiClient.post(`/reports/${reportId}/shares`, {
        userIds: ids,
        groupIds: [],
        permission,
      });

      setSuccess(`Report shared with ${ids.length} user(s) as ${permission}.`);
      setUserIdInput('');
      await loadShares();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: string } })?.response?.data ??
        'Failed to share report.';
      setError(typeof msg === 'string' ? msg : 'Failed to share report.');
    } finally {
      setSharing(false);
    }
  };

  // ── Revoke access ─────────────────────────────────────────────────────────

  const handleRevoke = async (userId: number) => {
    setError(null);
    setSuccess(null);
    try {
      await apiClient.delete(`/reports/${reportId}/shares/${userId}`);
      setSuccess('Access revoked.');
      await loadShares();
    } catch {
      setError('Failed to revoke access.');
    }
  };

  // ── Render ────────────────────────────────────────────────────────────────

  const permissionColor = (p: string) => {
    if (p === 'Admin') return 'error';
    if (p === 'Edit') return 'warning';
    return 'default';
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <ShareIcon fontSize="small" />
        Share Report{reportName ? `: ${reportName}` : ''}
      </DialogTitle>

      <DialogContent dividers>
        {/* Error / success banners */}
        {error && (
          <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        {success && (
          <Alert severity="success" onClose={() => setSuccess(null)} sx={{ mb: 2 }}>
            {success}
          </Alert>
        )}

        {/* Add share section */}
        <Typography variant="subtitle2" gutterBottom>
          Share with users
        </Typography>
        <Box sx={{ display: 'flex', gap: 1, mb: 3, flexWrap: 'wrap' }}>
          <TextField
            label="User IDs (comma-separated)"
            size="small"
            value={userIdInput}
            onChange={(e) => setUserIdInput(e.target.value)}
            placeholder="e.g. 5, 12, 34"
            sx={{ flex: 1, minWidth: 180 }}
          />
          <FormControl size="small" sx={{ minWidth: 100 }}>
            <InputLabel>Permission</InputLabel>
            <Select
              label="Permission"
              value={permission}
              onChange={(e) => setPermission(e.target.value as SharePermission)}
            >
              <MenuItem value="View">View</MenuItem>
              <MenuItem value="Edit">Edit</MenuItem>
              <MenuItem value="Admin">Admin</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="contained"
            onClick={handleShare}
            disabled={sharing || !userIdInput.trim()}
            startIcon={sharing ? <CircularProgress size={16} /> : <ShareIcon />}
          >
            Share
          </Button>
        </Box>

        {/* Existing shares table */}
        <Typography variant="subtitle2" gutterBottom>
          Current shares
        </Typography>

        {loadingShares ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
            <CircularProgress size={24} />
          </Box>
        ) : shares.length === 0 ? (
          <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
            This report has not been shared with anyone yet.
          </Typography>
        ) : (
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>User</TableCell>
                  <TableCell>Permission</TableCell>
                  <TableCell>Shared by</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {shares.map((s) => (
                  <TableRow key={s.shareId}>
                    <TableCell>
                      <Typography variant="body2">
                        {s.userName || s.userEmail || `User #${s.userId}`}
                      </Typography>
                      {s.userEmail && s.userName && (
                        <Typography variant="caption" color="text.secondary">
                          {s.userEmail}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={s.permission}
                        size="small"
                        color={permissionColor(s.permission) as 'default' | 'error' | 'warning'}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">
                        {s.sharedByUserName ?? '—'}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Revoke access">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleRevoke(s.userId)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
};

export default ReportShareDialog;

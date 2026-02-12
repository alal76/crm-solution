import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Chip,
  Alert,
  CircularProgress,
  Divider,
  Badge,
  Paper,
} from '@mui/material';
import {
  ApprovalOutlined,
  CheckCircleOutline,
  CancelOutlined,
  AccessTimeOutlined,
  PersonOutlined,
} from '@mui/icons-material';
import agentService from '../services/agentService';
import { AgentApproval, ApprovalStatus } from '../types/agents';

const STATUS_COLOR_MAP: Record<string, 'warning' | 'success' | 'error' | 'default' | 'info'> = {
  Pending: 'warning',
  Approved: 'success',
  Rejected: 'error',
  Expired: 'default',
  AutoApproved: 'info',
};

const ApprovalStatusLabel: Record<number, string> = {
  [ApprovalStatus.Pending]: 'Pending',
  [ApprovalStatus.Approved]: 'Approved',
  [ApprovalStatus.Rejected]: 'Rejected',
  [ApprovalStatus.Expired]: 'Expired',
  [ApprovalStatus.AutoApproved]: 'AutoApproved',
};

const getStatusLabel = (status: ApprovalStatus): string =>
  ApprovalStatusLabel[status] ?? 'Unknown';

const STATUS_FILTERS = ['all', 'Pending', 'Approved', 'Rejected', 'Expired'];

function formatRelativeTime(dateString: string | null | undefined): string {
  if (!dateString) return '—';
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const absDiffMs = Math.abs(diffMs);
  const minutes = Math.floor(absDiffMs / 60000);
  const hours = Math.floor(absDiffMs / 3600000);
  const days = Math.floor(absDiffMs / 86400000);

  if (diffMs < 0) {
    // Future date
    if (minutes < 60) return `Expires in ${minutes} min`;
    if (hours < 24) return `Expires in ${hours} hour${hours !== 1 ? 's' : ''}`;
    return `Expires in ${days} day${days !== 1 ? 's' : ''}`;
  }
  // Past date
  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes} min ago`;
  if (hours < 24) return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
  return `${days} day${days !== 1 ? 's' : ''} ago`;
}

function formatJson(params: string | null | undefined): string {
  if (!params) return '—';
  try {
    const parsed = JSON.parse(params);
    return JSON.stringify(parsed, null, 2);
  } catch {
    return params;
  }
}

const AgentApprovalsPage: React.FC = () => {
  const [approvals, setApprovals] = useState<AgentApproval[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
  const [selectedApprovalId, setSelectedApprovalId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [processing, setProcessing] = useState(false);

  const loadApprovals = useCallback(async () => {
    try {
      setError(null);
      const { data } = await agentService.getApprovals();
      const sorted = [...data].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
      setApprovals(sorted);
    } catch (err: any) {
      setError(err?.message || 'Failed to load approvals');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadApprovals();
    const interval = setInterval(loadApprovals, 30000);
    return () => clearInterval(interval);
  }, [loadApprovals]);

  useEffect(() => {
    if (success) {
      const timer = setTimeout(() => setSuccess(null), 4000);
      return () => clearTimeout(timer);
    }
    return undefined;
  }, [success]);

  const handleApprove = async (id: number) => {
    try {
      setProcessing(true);
      setError(null);
      await agentService.approveAction(id);
      setSuccess('Approval granted successfully.');
      await loadApprovals();
    } catch (err: any) {
      setError(err?.message || 'Failed to approve action');
    } finally {
      setProcessing(false);
    }
  };

  const openRejectDialog = (id: number) => {
    setSelectedApprovalId(id);
    setRejectReason('');
    setRejectDialogOpen(true);
  };

  const handleRejectConfirm = async () => {
    if (!selectedApprovalId || !rejectReason.trim()) return;
    try {
      setProcessing(true);
      setError(null);
      setRejectDialogOpen(false);
      await agentService.rejectAction(selectedApprovalId, rejectReason.trim());
      setSuccess('Action rejected successfully.');
      setSelectedApprovalId(null);
      setRejectReason('');
      await loadApprovals();
    } catch (err: any) {
      setError(err?.message || 'Failed to reject action');
    } finally {
      setProcessing(false);
    }
  };

  const filteredApprovals = approvals.filter((a) =>
    statusFilter === 'all' ? true : getStatusLabel(a.status) === statusFilter
  );

  const pendingCount = approvals.filter((a) => a.status === ApprovalStatus.Pending).length;

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Page Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, gap: 2 }}>
        <ApprovalOutlined sx={{ fontSize: 32, color: '#6750A4' }} />
        <Box sx={{ flex: 1 }}>
          <Typography variant="h5" fontWeight={700}>
            Agent Approval Queue
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Review and approve AI agent actions
          </Typography>
        </Box>
        <Badge badgeContent={pendingCount} color="warning" showZero>
          <Chip label="Pending" variant="outlined" />
        </Badge>
      </Box>

      {/* Alerts */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}

      {/* Filter Chips */}
      <Box sx={{ display: 'flex', gap: 1, mb: 3, flexWrap: 'wrap' }}>
        {STATUS_FILTERS.map((filter) => (
          <Chip
            key={filter}
            label={filter === 'all' ? 'All' : filter}
            variant={statusFilter === filter ? 'filled' : 'outlined'}
            color={filter === 'all' ? 'primary' : STATUS_COLOR_MAP[filter] || 'default'}
            onClick={() => setStatusFilter(filter)}
            sx={{ cursor: 'pointer' }}
          />
        ))}
      </Box>

      {/* Approvals List */}
      {filteredApprovals.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">
            {statusFilter === 'all'
              ? 'No approval requests found.'
              : `No ${statusFilter.toLowerCase()} approvals found.`}
          </Typography>
        </Paper>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {filteredApprovals.map((approval) => (
            <Card key={approval.id} variant="outlined">
              <CardContent>
                {/* Header row */}
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    mb: 1.5,
                  }}
                >
                  <Typography variant="subtitle1" fontWeight={600}>
                    Agent #{approval.agentId}
                  </Typography>
                  <Chip
                    label={getStatusLabel(approval.status)}
                    size="small"
                    color={STATUS_COLOR_MAP[getStatusLabel(approval.status)] || 'default'}
                  />
                </Box>

                {/* Action description */}
                <Typography variant="body1" fontWeight={600} sx={{ mb: 1 }}>
                  {approval.actionDescription || 'No description'}
                </Typography>

                {/* Plugin + Function */}
                <Box sx={{ display: 'flex', gap: 1, mb: 1.5, flexWrap: 'wrap' }}>
                  {approval.pluginName && (
                    <Chip
                      label={`Plugin: ${approval.pluginName}`}
                      size="small"
                      variant="outlined"
                      sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}
                    />
                  )}
                  {approval.functionName && (
                    <Chip
                      label={`Function: ${approval.functionName}`}
                      size="small"
                      variant="outlined"
                      sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}
                    />
                  )}
                </Box>

                {/* Parameters */}
                {approval.parameters && (
                  <Box sx={{ mb: 1.5 }}>
                    <Typography variant="caption" color="text.secondary" fontWeight={600}>
                      Parameters:
                    </Typography>
                    <Box
                      component="pre"
                      sx={{
                        backgroundColor: '#f5f5f5',
                        borderRadius: 1,
                        p: 1.5,
                        mt: 0.5,
                        fontSize: '0.8rem',
                        fontFamily: 'monospace',
                        overflow: 'auto',
                        maxHeight: 200,
                        whiteSpace: 'pre-wrap',
                        wordBreak: 'break-word',
                      }}
                    >
                      <code>{formatJson(approval.parameters)}</code>
                    </Box>
                  </Box>
                )}

                <Divider sx={{ my: 1.5 }} />

                {/* Timestamps + Meta */}
                <Box
                  sx={{
                    display: 'flex',
                    gap: 3,
                    flexWrap: 'wrap',
                    alignItems: 'center',
                    mb: approval.status === ApprovalStatus.Pending ? 2 : 0,
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <AccessTimeOutlined sx={{ fontSize: 16, color: 'text.secondary' }} />
                    <Typography variant="caption" color="text.secondary">
                      Created: {formatRelativeTime(approval.createdAt)}
                    </Typography>
                  </Box>
                  {approval.expiresAt && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <AccessTimeOutlined sx={{ fontSize: 16, color: 'text.secondary' }} />
                      <Typography variant="caption" color="text.secondary">
                        {formatRelativeTime(approval.expiresAt)}
                      </Typography>
                    </Box>
                  )}
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <PersonOutlined sx={{ fontSize: 16, color: 'text.secondary' }} />
                    <Typography variant="caption" color="text.secondary">
                      Requested by User #{approval.requestedByUserId}
                    </Typography>
                  </Box>
                </Box>

                {/* Rejected details */}
                {approval.status === ApprovalStatus.Rejected && approval.rejectionReason && (
                  <Alert severity="error" variant="outlined" sx={{ mt: 1.5 }}>
                    <Typography variant="body2">
                      <strong>Rejection reason:</strong> {approval.rejectionReason}
                    </Typography>
                  </Alert>
                )}

                {/* Approved details */}
                {(approval.status === ApprovalStatus.Approved || approval.status === ApprovalStatus.AutoApproved) && (
                  <Box
                    sx={{
                      display: 'flex',
                      gap: 2,
                      mt: 1.5,
                      flexWrap: 'wrap',
                      alignItems: 'center',
                    }}
                  >
                    {approval.approvedByUserId && (
                      <Typography variant="caption" color="success.main">
                        Approved by User #{approval.approvedByUserId}
                      </Typography>
                    )}
                    {approval.decidedAt && (
                      <Typography variant="caption" color="text.secondary">
                        Decided: {formatRelativeTime(approval.decidedAt)}
                      </Typography>
                    )}
                  </Box>
                )}

                {/* Action buttons for Pending */}
                {approval.status === ApprovalStatus.Pending && (
                  <Box sx={{ display: 'flex', gap: 1.5 }}>
                    <Button
                      variant="contained"
                      color="success"
                      size="small"
                      startIcon={<CheckCircleOutline />}
                      onClick={() => handleApprove(approval.id)}
                      disabled={processing}
                    >
                      Approve
                    </Button>
                    <Button
                      variant="contained"
                      color="error"
                      size="small"
                      startIcon={<CancelOutlined />}
                      onClick={() => openRejectDialog(approval.id)}
                      disabled={processing}
                    >
                      Reject
                    </Button>
                  </Box>
                )}
              </CardContent>
            </Card>
          ))}
        </Box>
      )}

      {/* Reject Dialog */}
      <Dialog
        open={rejectDialogOpen}
        onClose={() => setRejectDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Reject Action</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Please provide a reason for rejecting this action.
          </Typography>
          <TextField
            autoFocus
            fullWidth
            multiline
            rows={3}
            label="Rejection Reason"
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
            required
            error={rejectDialogOpen && rejectReason.trim() === ''}
            helperText={
              rejectDialogOpen && rejectReason.trim() === '' ? 'Reason is required' : ''
            }
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectDialogOpen(false)} disabled={processing}>
            Cancel
          </Button>
          <Button
            onClick={handleRejectConfirm}
            color="error"
            variant="contained"
            disabled={processing || !rejectReason.trim()}
          >
            Reject
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AgentApprovalsPage;

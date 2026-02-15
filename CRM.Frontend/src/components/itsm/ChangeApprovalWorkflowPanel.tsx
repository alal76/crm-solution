/**
 * ChangeApprovalWorkflow - CAB approval workflow UI
 */

import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Stack,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Chip,
  Avatar,
  AvatarGroup,
  Divider,
  Alert,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import {
  CheckCircle as ApprovedIcon,
  Cancel as RejectedIcon,
  HourglassEmpty as PendingIcon,
  Comment as CommentIcon,
} from '@mui/icons-material';
import { ChangeApproval, ApprovalStatus } from '../../services/changeService';

interface ChangeApprovalWorkflowProps {
  approvals: ChangeApproval[];
  changeStatus: number;
  onApprove?: (comments?: string) => Promise<void>;
  onReject?: (reason: string) => Promise<void>;
  currentUserIsApprover?: boolean;
  loading?: boolean;
}

const getApprovalStatusIcon = (status: ApprovalStatus) => {
  switch (status) {
    case ApprovalStatus.Approved:
      return <ApprovedIcon sx={{ color: 'success.main' }} />;
    case ApprovalStatus.Rejected:
      return <RejectedIcon sx={{ color: 'error.main' }} />;
    case ApprovalStatus.Deferred:
      return <CommentIcon sx={{ color: 'warning.main' }} />;
    default:
      return <PendingIcon sx={{ color: 'info.main' }} />;
  }
};

const getApprovalStatusLabel = (status: ApprovalStatus): string => {
  const labels = ['Pending', 'Approved', 'Rejected', 'Deferred'];
  return labels[status] || 'Unknown';
};

export const ChangeApprovalWorkflow: React.FC<ChangeApprovalWorkflowProps> = ({
  approvals = [],
  changeStatus,
  onApprove,
  onReject,
  currentUserIsApprover = false,
  loading = false,
}) => {
  const [actionDialogOpen, setActionDialogOpen] = useState<'approve' | 'reject' | null>(null);
  const [actionComments, setActionComments] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleApprove = async () => {
    setSubmitting(true);
    try {
      await onApprove?.(actionComments);
      setActionDialogOpen(null);
      setActionComments('');
    } finally {
      setSubmitting(false);
    }
  };

  const handleReject = async () => {
    setSubmitting(true);
    try {
      await onReject?.(actionComments);
      setActionDialogOpen(null);
      setActionComments('');
    } finally {
      setSubmitting(false);
    }
  };

  const approvedCount = approvals.filter((a) => a.status === ApprovalStatus.Approved).length;
  const rejectedCount = approvals.filter((a) => a.status === ApprovalStatus.Rejected).length;
  const pendingCount = approvals.filter((a) => a.status === ApprovalStatus.Pending).length;

  return (
    <Box>
      {/* Summary Stats */}
      <Paper sx={{ p: 2, mb: 2, bgcolor: 'background.default' }}>
        <Stack direction="row" spacing={3}>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Approved
            </Typography>
            <Typography variant="h6">{approvedCount}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Pending
            </Typography>
            <Typography variant="h6">{pendingCount}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Rejected
            </Typography>
            <Typography variant="h6">{rejectedCount}</Typography>
          </Box>
          <Box sx={{ ml: 'auto' }}>
            <Typography variant="caption" color="text.secondary">
              Total Required
            </Typography>
            <Typography variant="h6">{approvals.length}</Typography>
          </Box>
        </Stack>
      </Paper>

      {/* Approvals Table */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: 'action.hover' }}>
              <TableCell>Approver</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Response Date</TableCell>
              <TableCell>Comments</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {approvals.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} sx={{ textAlign: 'center', py: 3 }}>
                  <Typography color="text.secondary">No approvers assigned yet</Typography>
                </TableCell>
              </TableRow>
            ) : (
              approvals.map((approval) => (
                <TableRow key={approval.id} hover>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Avatar sx={{ width: 32, height: 32 }}>
                        {approval.approverName?.[0] || '?'}
                      </Avatar>
                      <Box>
                        <Typography variant="body2">{approval.approverName}</Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {getApprovalStatusIcon(approval.status)}
                      <Chip
                        label={getApprovalStatusLabel(approval.status)}
                        size="small"
                        color={
                          approval.status === ApprovalStatus.Approved
                            ? 'success'
                            : approval.status === ApprovalStatus.Rejected
                            ? 'error'
                            : 'default'
                        }
                        variant="outlined"
                      />
                    </Box>
                  </TableCell>
                  <TableCell>
                    {approval.respondedAt
                      ? new Date(approval.respondedAt).toLocaleDateString()
                      : '-'}
                  </TableCell>
                  <TableCell>{approval.comments || '-'}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Action Buttons */}
      {currentUserIsApprover && changeStatus < 3 && (
        <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
          <Button
            variant="contained"
            color="success"
            onClick={() => setActionDialogOpen('approve')}
            disabled={loading || submitting}
          >
            Approve
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={() => setActionDialogOpen('reject')}
            disabled={loading || submitting}
          >
            Reject
          </Button>
        </Stack>
      )}

      {/* Action Dialogs */}
      <Dialog open={actionDialogOpen !== null} onClose={() => setActionDialogOpen(null)}>
        <DialogTitle>
          {actionDialogOpen === 'approve' ? 'Approve Change' : 'Reject Change'}
        </DialogTitle>
        <DialogContent sx={{ pt: 2, minWidth: 400 }}>
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Comments"
            value={actionComments}
            onChange={(e) => setActionComments(e.target.value)}
            placeholder="Add your comments..."
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setActionDialogOpen(null)} disabled={submitting}>
            Cancel
          </Button>
          <Button
            onClick={actionDialogOpen === 'approve' ? handleApprove : handleReject}
            variant="contained"
            color={actionDialogOpen === 'approve' ? 'success' : 'error'}
            disabled={submitting}
          >
            {actionDialogOpen === 'approve' ? 'Approve' : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default ChangeApprovalWorkflow;
